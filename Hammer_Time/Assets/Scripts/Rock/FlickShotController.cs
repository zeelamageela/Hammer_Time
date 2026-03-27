using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles "Flick Shot" mode - a two-phase aiming system:
/// Phase 1: Aim by rotating shooting knob around launcher (locked distance)
/// Phase 2: Power by dragging rock toward hog line (speed determines power)
/// </summary>
public class FlickShotController : MonoBehaviour
{
    [Header("Component References")]
    [Tooltip("Auto-assigned from same GameObject")]
    public Rigidbody2D rb;
    
    [Tooltip("Launcher GameObject - finds by tag if not assigned")]
    public GameObject launcher;
    
    [Tooltip("Trajectory Line - finds by name if not assigned")]
    public GameObject trajectoryLineObj;
    
    [Tooltip("Game Manager - finds by tag if not assigned")]
    public GameObject gameManagerObj;
    
    [Tooltip("Camera Manager - finds in scene if not assigned")]
    public GameObject cameraManagerObj;
    
    [Tooltip("Shooting Knob - finds by name if not assigned")]
    public GameObject shootingKnobObj;
    
    [Tooltip("Line renderer for drawing swipe trail")]
    private LineRenderer swipeTrailLine;
    
    [Tooltip("Line renderer for predicted stop position")]
    private LineRenderer predictedStopLine;
    
    [Tooltip("Velocity guide indicator - shows player the correct swipe speed")]
    private VelocityGuideIndicator velocityGuide;
    
    [Tooltip("Track cursor positions during swipe")]
    private List<Vector3> swipePoints = new List<Vector3>();
    
    [Header("Phase 1: Aim Settings")]
    [Tooltip("Locked pullback distance for aiming phase")]
    [Range(2.0f, 5.0f)]
    public float aimLockDistance = 3.5f;
    
    [Tooltip("Rotation speed for aiming (degrees per pixel of mouse movement)")]
    [Range(0.1f, 5.0f)]
    public float aimSensitivity = 1.5f;
    
    [Header("Phase 2: Power Settings")]
    [Tooltip("Start position for power drag (Y coordinate)")]
    public float powerDragStartY = -25f;
    
    [Tooltip("Target position for power drag (Y coordinate, near hog line)")]
    public float powerDragTargetY = -16f;
    
    [Tooltip("Minimum drag time to register shot (seconds)")]
    [Range(0.05f, 0.5f)]
    public float minDragTime = 0.1f;
    
    [Tooltip("Maximum drag time for fastest shot (seconds)")]
    [Range(0.2f, 2.0f)]
    public float maxDragTime = 1.5f;
    
    [Header("Speed Quantization")]
    [Tooltip("Number of speed bands (e.g., 7 for more precision: Very Slow, Slow, Slow-Med, Medium, Med-Fast, Fast, Very Fast)")]
    [Range(3, 10)]
    public int speedBands = 7; // Was 5, now 7 for more precision
    
    [Tooltip("Tolerance for 'Perfect' speed band (% above/below center)")]
    [Range(0.05f, 0.3f)]
    public float perfectTolerance = 0.15f;
    
    [Header("Skill Tuning")]
    [Tooltip("Forgiveness factor - higher = more forgiving (easier to hit speed bands)")]
    [Range(0.5f, 2.0f)]
    public float forgivenessFactor = 1.2f;
    
    [Header("Visual Feedback")]
    [Tooltip("Show speed feedback text callouts during drag")]
    public bool showSpeedFeedback = true;
    
    [Tooltip("Feedback update interval (seconds)")]
    [Range(0.05f, 0.5f)]
    public float feedbackInterval = 0.1f;
    
    [Header("Speed Guide Slider")]
    [Tooltip("Name of slider GameObject to find (default: 'FlickShotSpeedSlider')")]
    public string speedSliderName = "FlickShotSpeedSlider";
    
    [Tooltip("Auto-detected each turn")]
    private UnityEngine.UI.Slider speedSlider;
    
    [Tooltip("Auto-detected from slider handle")]
    private UnityEngine.UI.Image sliderHandleImage;
    
    [Tooltip("Auto-detected shooter animator")]
    private ShooterAnim shooterAnim;
    
    // Shooter animation control
    private bool isShooterAnimControlActive = false;
    
    // State tracking
    public enum FlickShotPhase
    {
        Inactive,
        AimingPhase,
        AimSet,        // NEW: Aim has been set, ready for power click
        PowerPhase,
        Released
    }
    
    public FlickShotPhase currentPhase = FlickShotPhase.Inactive;
    
    // Aim phase state
    private Vector2 aimDirection;
    private float aimAngle;
    private Vector3 lastMousePosition;
    private Vector2 storedPullbackPosition; // CRITICAL: Store pullback position from aim phase
    
    // Power phase state
    private Vector2 powerDragStartPos;
    private float powerDragStartTime;
    private float lastFeedbackTime;
    private string lastFeedbackMessage = "";
    private bool isPowerDragging = false;  // Track if we're actively dragging
    
    // Speed slider state
    private float idealDragTime = 0.8f;
    private float ghostCycleStartTime = 0f;
    private bool isSliderActive = false;
    private float lastPlayerSliderValue = 0f;
    
    // Calculated values
    private float calculatedSpeed;
    private int speedBand;
    
    // Cached references (using dynamic types to avoid compilation issues)
    private bool isEnabled = false;
    private object trajLine; // TrajectoryLine component
    private object gm; // GameManager component  
    private object rockInfo; // Rock_Info component
    private object cameraManager; // CameraManager component
    private System.Reflection.PropertyInfo minVelocityProp;
    private System.Reflection.PropertyInfo maxVelocityProp;
    private System.Reflection.MethodInfo switchCameraMethod;
    
    void Start()
    {
        // Auto-assign references if not set
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (launcher == null) launcher = GameObject.FindWithTag("Launcher");
        if (trajectoryLineObj == null) trajectoryLineObj = GameObject.Find("TrajectoryLine");
        if (gameManagerObj == null) gameManagerObj = GameObject.FindWithTag("GameController");
        if (cameraManagerObj == null) cameraManagerObj = GameObject.Find("CameraManager");
        if (shootingKnobObj == null) shootingKnobObj = GameObject.Find("ShootingKnob");
        
        // Get component references using reflection to avoid type dependencies
        if (trajectoryLineObj != null)
        {
            trajLine = trajectoryLineObj.GetComponent("TrajectoryLine");
            if (trajLine != null)
            {
                System.Type trajType = trajLine.GetType();
                minVelocityProp = trajType.GetProperty("minVelocity");
                maxVelocityProp = trajType.GetProperty("maxVelocity");
            }
        }
        if (gameManagerObj != null) gm = gameManagerObj.GetComponent("GameManager");
        if (cameraManagerObj != null)
        {
            cameraManager = cameraManagerObj.GetComponent("CameraManager");
            if (cameraManager != null)
            {
                System.Type camType = cameraManager.GetType();
                switchCameraMethod = camType.GetMethod("SwitchCamera");
            }
        }
        rockInfo = GetComponent("Rock_Info");
        
        // Create swipe trail line renderer (BLACK line that draws as player swipes)
        GameObject swipeTrailObj = new GameObject("SwipeTrail");
        swipeTrailLine = swipeTrailObj.AddComponent<LineRenderer>();
        swipeTrailLine.enabled = false;
        swipeTrailLine.startWidth = 0.05f; // 75% thinner (was 0.2f)
        swipeTrailLine.endWidth = 0.05f;
        swipeTrailLine.positionCount = 0;
        
        // 66% more transparent (34% opacity instead of 100%)
        Color swipeColor = Color.black;
        swipeColor.a = 0.34f;
        swipeTrailLine.startColor = swipeColor;
        swipeTrailLine.endColor = swipeColor;
        
        swipeTrailLine.material = new Material(Shader.Find("Sprites/Default"));
        
        // Enable smoothing for cleaner line
        swipeTrailLine.useWorldSpace = true;
        swipeTrailLine.numCornerVertices = 8;  // Smooth corners
        swipeTrailLine.numCapVertices = 8;     // Smooth ends
        
        Debug.Log("[FlickShot] Swipe trail line created (black, thin, 34% opacity, smoothed)");
        
        // Create predicted stop line (CYAN horizontal line)
        GameObject predictedStopObj = new GameObject("PredictedStopLine");
        predictedStopLine = predictedStopObj.AddComponent<LineRenderer>();
        predictedStopLine.enabled = false;
        predictedStopLine.startWidth = 0.15f;
        predictedStopLine.endWidth = 0.15f;
        predictedStopLine.positionCount = 2;
        Color cyanColor = new Color(0f, 0.8f, 1f, 0.8f);
        predictedStopLine.startColor = cyanColor;
        predictedStopLine.endColor = cyanColor;
        predictedStopLine.material = new Material(Shader.Find("Sprites/Default"));
        Debug.Log("[FlickShot] Predicted stop line created (cyan horizontal)");
        
        // Create velocity guide indicator
        GameObject velocityGuideObj = new GameObject("VelocityGuide");
        velocityGuide = velocityGuideObj.AddComponent<VelocityGuideIndicator>();
        velocityGuide.startY = -24.66f;  // Launcher position (updated)
        velocityGuide.endY = -16.5f;     // Hog line position (updated)
        velocityGuide.pauseDuration = 1.5f; // Pause at hogline (1.5s total)
        velocityGuide.fadeOutDuration = 0.5f; // Fade out over last 0.5s
        velocityGuide.lineWidth = 0.2f;
        Debug.Log("[FlickShot] Velocity guide indicator created with 1.5s pause + 0.5s fade-out");
        
        // Subscribe to flick shot mode changes using reflection
        System.Type settingsType = System.Type.GetType("GameVisualizationSettings");
        if (settingsType != null)
        {
            System.Reflection.PropertyInfo instanceProp = settingsType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceProp != null)
            {
                object visualSettings = instanceProp.GetValue(null);
                if (visualSettings != null)
                {
                    System.Reflection.PropertyInfo flickModeProp = settingsType.GetProperty("FlickShotMode");
                    if (flickModeProp != null)
                    {
                        isEnabled = (bool)flickModeProp.GetValue(visualSettings);
                    }
                    
                    System.Reflection.EventInfo modeChangedEvent = settingsType.GetEvent("OnFlickShotModeChanged");
                    if (modeChangedEvent != null)
                    {
                        System.Delegate handler = System.Delegate.CreateDelegate(modeChangedEvent.EventHandlerType, this, "OnFlickShotModeChanged");
                        modeChangedEvent.AddEventHandler(visualSettings, handler);
                    }
                }
            }
        }
        
        Debug.Log($"[FlickShotController] Initialized - Mode: {(isEnabled ? "ENABLED" : "DISABLED")}");
    }
    
    void Update()
    {
        if (!isEnabled) return;
        
        switch (currentPhase)
        {
            case FlickShotPhase.AimingPhase:
                // Still dragging to aim - do nothing, wait for Rock_Flick.OnMouseUp()
                break;
            
            case FlickShotPhase.AimSet:
                // Aim has been set, waiting for player to click launcher
                CheckForLauncherClick();
                break;
            
            case FlickShotPhase.PowerPhase:
                UpdatePowerPhase();
                break;
        }
    }
    
    /// <summary>
    /// Check if player clicked on launcher to start power phase
    /// Only active when aim has been set (AimSet phase)
    /// </summary>
    private void CheckForLauncherClick()
    {
        if (Input.GetMouseButtonDown(0) && launcher != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            
            // Check if clicking near launcher (within 1 unit radius)
            Vector2 launcherPos = launcher.transform.position;
            float distToLauncher = Vector2.Distance(mousePos2D, launcherPos);
            
            if (distToLauncher < 1.0f)
            {
                Debug.Log($"[FlickShot] Launcher clicked! Starting power phase immediately (distance to launcher: {distToLauncher:F2})");
                StartPowerPhase();
            }
            else
            {
                Debug.Log($"[FlickShot] Click detected but too far from launcher ({distToLauncher:F2} units) - click closer to launcher (0, -25)");
            }
        }
    }
    
    /// <summary>
    /// Called when flick shot mode setting changes
    /// </summary>
    private void OnFlickShotModeChanged(bool enabled)
    {
        isEnabled = enabled;
        Debug.Log($"[FlickShotController] Mode changed to: {(enabled ? "ENABLED" : "DISABLED")}");
        
        if (!enabled)
        {
            // Reset to inactive state if disabled mid-shot
            currentPhase = FlickShotPhase.Inactive;
        }
    }
    
    /// <summary>
    /// Start flick shot sequence - called when rock is enabled/ready
    /// In flick shot mode, we use normal pullback to SET AIM (not fire)
    /// Then player clicks launcher to start power flick
    /// </summary>
    public void StartFlickShot()
    {
        if (!isEnabled) return;
        
        currentPhase = FlickShotPhase.AimingPhase;
        
        Debug.Log($"[FlickShot] Flick shot mode active - use normal pullback to SET AIM (won't fire on release)");
        
        // In flick shot mode, we DON'T auto-position the rock
        // Instead, we let the normal Rock_Flick pullback handle positioning
        // We just intercept the release to prevent firing
    }
    
    /// <summary>
    /// Set the aim position from Rock_Flick after pullback release
    /// This transitions from AimingPhase to AimSet phase
    /// </summary>
    public void SetAimPosition(Vector2 rockPosition, Vector2 launcherPosition)
    {
        // Calculate aim direction from rock position relative to launcher
        Vector2 direction = rockPosition - launcherPosition;
        float pullbackDistance = direction.magnitude;
        
        // CRITICAL: Only accept aim if pullback is significant (at least 2 units)
        if (pullbackDistance < 2.0f)
        {
            Debug.Log($"[FlickShot] Pullback too small ({pullbackDistance:F2} units) - need at least 2.0 units. Try again!");
            // Stay in AimingPhase - player needs to try again
            return;
        }
        
        // CRITICAL: FLIP the direction! Rock goes OPPOSITE of pullback
        // If you pull DOWN (negative Y), rock should go UP (positive Y) toward house
        aimDirection = -direction.normalized;  // NEGATIVE!
        aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        
        // CRITICAL FIX: Store the pullback position for velocity calculation later
        storedPullbackPosition = rockPosition;
        
        // Transition to AimSet phase - ready for power click
        currentPhase = FlickShotPhase.AimSet;
        
        Debug.Log($"[FlickShot] Aim position set - Rock: {rockPosition}, Launcher: {launcherPosition}, Pullback: {direction}, Aim Direction (FLIPPED): {aimDirection}, Angle: {aimAngle:F1}°, Distance: {pullbackDistance:F2}");
        Debug.Log($"[FlickShot] Stored pullback position: {storedPullbackPosition}");
        Debug.Log($"[FlickShot] Aim locked! Click on launcher (0, -25) to start power phase.");
    }
    
    /// <summary>
    /// Transition from aiming to power phase
    /// Triggered when player clicks on launcher
    /// </summary>
    private void StartPowerPhase()
    {
        currentPhase = FlickShotPhase.PowerPhase;
        
        Debug.Log("[FlickShot] Returning rock to launcher before power phase...");
        
        // CRITICAL: Move rock back to launcher position
        // This prepares it for the normal launch system
        if (launcher != null)
        {
            rb.position = launcher.transform.position;
            rb.linearVelocity = Vector2.zero;
            
            // Make sure rock is kinematic at launcher (ready for drag)
            rb.isKinematic = true;
            
            // Re-enable and reset spring to normal pullback state
            SpringJoint2D spring = GetComponent<SpringJoint2D>();
            if (spring != null)
            {
                spring.enabled = true; // Re-enable spring
                spring.dampingRatio = 0.2f;
                spring.frequency = 1.5f;
            }
            
            Debug.Log($"[FlickShot] Rock positioned at launcher: {rb.position}, spring re-enabled");
        }
        
        // Aim direction was already set by SetAimPosition() when player released pullback
        // It will be used when we release the power drag
        
        // Hide normal shooting knob during power phase
        if (shootingKnobObj != null)
        {
            SpriteRenderer knobSprite = shootingKnobObj.GetComponent<SpriteRenderer>();
            if (knobSprite != null)
            {
                knobSprite.enabled = false;
                Debug.Log("[FlickShot] Shooting knob hidden for power phase");
            }
            
            LineRenderer knobLine = shootingKnobObj.GetComponent<LineRenderer>();
            if (knobLine != null)
            {
                knobLine.enabled = false;
            }
        }
        
        // Disable aim camera
        if (cameraManager != null)
        {
            System.Type camType = cameraManager.GetType();
            System.Reflection.FieldInfo aimCameraField = camType.GetField("aim");
            if (aimCameraField != null)
            {
                object aimCamera = aimCameraField.GetValue(cameraManager);
                if (aimCamera != null)
                {
                    System.Reflection.PropertyInfo depthProp = aimCamera.GetType().GetProperty("depth");
                    if (depthProp != null)
                    {
                        depthProp.SetValue(aimCamera, -1); // Disable aim camera
                        Debug.Log("[FlickShot] Aim camera disabled (depth = -1)");
                    }
                }
            }
        }
        
        // Enable swipe trail line (will draw as player swipes)
        if (swipeTrailLine != null)
        {
            swipeTrailLine.enabled = true;
            swipePoints.Clear();
            Debug.Log("[FlickShot] Swipe trail enabled - ready to draw");
        }
        
        // Power drag starts at launcher Y position
        powerDragStartPos = new Vector2(launcher.transform.position.x, powerDragStartY);
        powerDragStartTime = Time.time;
        lastFeedbackTime = Time.time;
        
        // Initialize speed slider
        InitializeSpeedSlider();
        
        // Start velocity guide indicator
        if (velocityGuide != null)
        {
            // CRITICAL FIX: Get target velocity from TrajectoryLine (based on aim/trajectory endpoint)
            // NOT from drag timing - that comes later!
            float targetVelocity = GetTargetVelocityFromTrajectory();
            
            if (targetVelocity <= 0f)
            {
                Debug.LogWarning("[FlickShot] Failed to get target velocity from trajectory - using fallback");
                targetVelocity = 10f; // Fallback to medium velocity
            }
            
            // Get the shooting knob color directly (it already calculates color based on aim circle Y)
            Color guideColor = Color.white;
            if (shootingKnobObj != null)
            {
                SpriteRenderer knobSprite = shootingKnobObj.GetComponent<SpriteRenderer>();
                if (knobSprite != null)
                {
                    guideColor = knobSprite.color;
                    // Set to 60% opacity for velocity guide
                    guideColor.a = 0.6f;
                    Debug.Log($"[FlickShot] Using shooting knob color at 60% opacity: {guideColor}");
                }
            }
            
            velocityGuide.StartGuide(targetVelocity, guideColor);
            Debug.Log($"[FlickShot] Velocity guide started - {targetVelocity:F2} m/s (from trajectory), Color: {guideColor}");
            
            // Show velocity callout at launcher using TextCalloutManager directly
            if (showSpeedFeedback && TextCalloutManager.Instance != null)
            {
                // Calculate ideal drag time for this velocity
                float distance = velocityGuide.endY - velocityGuide.startY; // -16.5 - (-24.66) = 8.16 units
                float idealTime = distance / targetVelocity; // Time guide takes to animate
                
                Vector2 launcherPos = launcher.transform.position;
                string velocityMessage = $"Target: {targetVelocity:F1} m/s";
                string timeMessage = $"Swipe in {idealTime:F2}s";
                
                // Show velocity and time as stacked callouts
                TextCalloutManager.Instance.ShowCallout(launcherPos, velocityMessage, followTarget: false, target: null, duration: 5f);
                TextCalloutManager.Instance.ShowCallout(launcherPos, timeMessage, followTarget: false, target: null, duration: 5f);
                
                Debug.Log($"[FlickShot] Velocity callouts shown: {velocityMessage} | {timeMessage}");
            }
        }
        
        // DISABLED: Shooter animation control removed
        // if (shooterAnim != null)
        // {
        //     shooterAnim.StartSwipeControl();
        //     isShooterAnimControlActive = true;
        //     Debug.Log("[FlickShot] Shooter animation control STARTED - player drives animation!");
        // }
        // else
        // {
        //     Debug.LogWarning("[FlickShot] No ShooterAnim found - animation control disabled");
        // }
        
        Debug.Log($"[FlickShot] Phase 2: POWER started - Drag from Y={powerDragStartY} to Y={powerDragTargetY} for speed!");
        Debug.Log($"[FlickShot] Using aim direction: angle={aimAngle:F1}°, direction={aimDirection}");
        
        // Show initial feedback
        if (showSpeedFeedback)
        {
            ShowCallout(launcher.transform.position, "Drag down for Power!", followTarget: false, duration: feedbackInterval * 2f);
        }
    }
    
    /// <summary>
    /// Update power phase (track drag speed from launcher down the sheet)
    /// FIXED: Now calculates velocity DURING drag for real-time cyan line preview!
    /// Player swipes and we draw a trail, then show feedback AFTER release
    /// </summary>
    private void UpdatePowerPhase()
    {
        // Update speed slider animation
        UpdateSpeedSlider();
        
        // Wait for mouse down to start dragging
        if (!isPowerDragging)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isPowerDragging = true;
                powerDragStartTime = Time.time;
                swipePoints.Clear();
                
                // KEEP velocity guide running during drag (don't stop it!)
                // This allows player to time their swipe to the animation
                Debug.Log("[FlickShot] Power swipe started - velocity guide continues animating");
                
                // Add starting point at launcher
                Vector3 startPos = launcher.transform.position;
                startPos.z = -1f; // In front of everything
                swipePoints.Add(startPos);
                
                Debug.Log("[FlickShot] Power swipe started - draw your path!");
            }
            return;
        }
        
        // Get current mouse position in world space
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mousePos3D = new Vector3(mouseWorldPos.x, mouseWorldPos.y, -1f);
        
        // CRITICAL FIX: Calculate velocity DURING drag for real-time preview!
        float currentDragTime = Time.time - powerDragStartTime;
        if (currentDragTime > 0.01f) // Avoid divide by zero
        {
            // Calculate speed continuously (use unified formula!)
            calculatedSpeed = CalculateSpeedFromDragTime(currentDragTime);
            float previewVelocity = GetPredictedVelocity();
            float previewStopY = CalculatePredictedStopPosition(previewVelocity);
            
            // Update cyan line DURING drag to show real-time prediction!
            if (predictedStopLine != null)
            {
                float lineWidth = 3f;
                Vector3 leftPoint = new Vector3(-lineWidth, previewStopY, -1f);
                Vector3 rightPoint = new Vector3(lineWidth, previewStopY, -1f);
                
                predictedStopLine.SetPosition(0, leftPoint);
                predictedStopLine.SetPosition(1, rightPoint);
                
                if (!predictedStopLine.enabled)
                {
                    predictedStopLine.enabled = true;
                    Debug.Log("[FlickShot] Cyan prediction line enabled during drag");
                }
            }
            
            // DISABLED: Shooter animation control removed
            // if (isShooterAnimControlActive && shooterAnim != null)
            // {
            //     float swipeProgress = CalculateSwipeProgress(mouseWorldPos.y);
            //     shooterAnim.SetSwipeProgress(swipeProgress);
            // }
        }
        
        // Add cursor position to trail with smoothing (reduce jitter)
        // Only sample if cursor moved far enough to avoid dense clustering
        float minSampleDistance = 0.15f; // Sample every 15cm (smoother, less jagged)
        
        if (swipePoints.Count == 0 || Vector3.Distance(swipePoints[swipePoints.Count - 1], mousePos3D) > minSampleDistance)
        {
            swipePoints.Add(mousePos3D);
            
            // Apply Catmull-Rom smoothing if we have enough points
            if (swipePoints.Count >= 4)
            {
                // Smooth the trail by interpolating between last few points
                List<Vector3> smoothedPoints = SmoothSwipePath(swipePoints);
                
                // Update line renderer with smoothed points
                if (swipeTrailLine != null)
                {
                    swipeTrailLine.positionCount = smoothedPoints.Count;
                    swipeTrailLine.SetPositions(smoothedPoints.ToArray());
                }
            }
            else
            {
                // Not enough points yet - just use raw points
                if (swipeTrailLine != null)
                {
                    swipeTrailLine.positionCount = swipePoints.Count;
                    swipeTrailLine.SetPositions(swipePoints.ToArray());
                }
            }
        }
        
        // Check for release (mouse up)
        if (Input.GetMouseButtonUp(0))
        {
            float dragTime = Time.time - powerDragStartTime;
            float currentY = Mathf.Clamp(mouseWorldPos.y, powerDragTargetY, powerDragStartY);
            float dragDistance = Mathf.Abs(currentY - powerDragStartY);
            
            // DISABLED: Shooter animation control removed
            // if (isShooterAnimControlActive && shooterAnim != null)
            // {
            //     if (!shooterAnim.CanRelease())
            //     {
            //         Debug.LogWarning($"[FlickShot] Release too early! Progress: {shooterAnim.GetSwipeProgress():F2}, need >= {shooterAnim.releaseThreshold:F2}");
            //         ShowCallout(transform.position, "Release too early!", followTarget: false, duration: 2f);
            //         ShowCallout(transform.position, "Swipe further down", followTarget: false, duration: 2f);
            //         
            //         // Reset for another try
            //         isPowerDragging = false;
            //         return;
            //     }
            //     
            //     Debug.Log($"[FlickShot] Valid release at progress: {shooterAnim.GetSwipeProgress():F2}");
            // }
            
            ReleaseFlickShot(dragTime, dragDistance);
        }
    }
    
    /// <summary>
    /// Get target velocity from TrajectoryLine based on current aim/trajectory endpoint
    /// This is the velocity needed to reach the aimed position
    /// FIXED: Use stored pullback position from aim phase (not current position!)
    /// </summary>
    private float GetTargetVelocityFromTrajectory()
    {
        if (trajLine == null)
        {
            Debug.LogWarning("[FlickShot] trajLine is null - cannot get target velocity");
            return 10f; // Fallback
        }
        
        // CRITICAL FIX: Use stored pullback position from aim phase!
        // The rock has been moved back to launcher by StartPowerPhase(), so we can't use current position
        if (storedPullbackPosition == Vector2.zero)
        {
            Debug.LogWarning("[FlickShot] No stored pullback position - aim not set yet");
            return 10f;
        }
        
        if (launcher == null)
        {
            Debug.LogWarning("[FlickShot] Launcher is null");
            return 10f;
        }
        
        Vector2 pullbackPos = storedPullbackPosition; // Use stored position from aim phase
        Vector2 launcherPos = launcher.transform.position;
        
        System.Type trajType = trajLine.GetType();
        
        // Get parameters from TrajectoryLine (use correct field types!)
        float velocityMult = 5.0f;
        float minPullback = 1f;
        float maxPullback = 2.75f;
        float minVel = 5f;
        float maxVel = 16f;
        
        // CRITICAL: velocityMultiplier is a PUBLIC FIELD, not property
        System.Reflection.FieldInfo velocityMultField = trajType.GetField("velocityMultiplier", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo minPullProp = trajType.GetField("minPullbackDistance",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo maxPullProp = trajType.GetField("maxPullbackDistance",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo minVelProp = trajType.GetField("minVelocity",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo maxVelProp = trajType.GetField("maxVelocity",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        
        if (velocityMultField != null)
        {
            object multObj = velocityMultField.GetValue(trajLine);
            if (multObj != null) velocityMult = (float)multObj;
        }
        if (minPullProp != null)
        {
            object obj = minPullProp.GetValue(trajLine);
            if (obj != null) minPullback = (float)obj;
        }
        if (maxPullProp != null)
        {
            object obj = maxPullProp.GetValue(trajLine);
            if (obj != null) maxPullback = (float)obj;
        }
        if (minVelProp != null)
        {
            object obj = minVelProp.GetValue(trajLine);
            if (obj != null) minVel = (float)obj;
        }
        if (maxVelProp != null)
        {
            object obj = maxVelProp.GetValue(trajLine);
            if (obj != null) maxVel = (float)obj;
        }
        
        // CRITICAL: Use TrajectorySimulator's static method to get EXACT velocity
        Vector2 initialVelocity = TrajectorySimulator.CalculateInitialVelocityFromPullback(
            pullbackPos,
            launcherPos,
            velocityMult,
            minPullback,
            maxPullback,
            minVel,
            maxVel
        );
        
        float targetVel = initialVelocity.magnitude;
        
        // Calculate what the trajectory line sees
        Vector2 displacement = launcherPos - pullbackPos;
        float actualPullbackDist = displacement.magnitude;
        
        Debug.Log($"[FlickShot] Got velocity using STORED pullback position: {targetVel:F2} m/s");
        Debug.Log($"  Stored pullback pos: {pullbackPos} (from aim phase)");
        Debug.Log($"  Launcher pos: {launcherPos}");
        Debug.Log($"  Displacement vector: {displacement}");
        Debug.Log($"  Actual pullback distance: {actualPullbackDist:F2} units");
        Debug.Log($"  Parameters: velocityMult={velocityMult:F2}, minPull={minPullback:F2}, maxPull={maxPullback:F2}");
        Debug.Log($"  Velocity range: {minVel:F2} to {maxVel:F2}");
        Debug.Log($"  Initial velocity vector: {initialVelocity}");
        Debug.Log($"  Normalized pullback: {Mathf.InverseLerp(minPullback, maxPullback, actualPullbackDist):F3}");
        
        return targetVel;
    }
    
    /// <summary>
    /// Get predicted velocity based on current drag time
    /// </summary>
    private float GetPredictedVelocity()
    {
        float minVel = 5f;
        float maxVel = 13f;
        
        // Try to get velocity range from TrajectoryLine
        if (minVelocityProp != null && trajLine != null)
        {
            object minVelObj = minVelocityProp.GetValue(trajLine);
            if (minVelObj != null) minVel = (float)minVelObj;
        }
        if (maxVelocityProp != null && trajLine != null)
        {
            object maxVelObj = maxVelocityProp.GetValue(trajLine);
            if (maxVelObj != null) maxVel = (float)maxVelObj;
        }
        
        return Mathf.Lerp(minVel, maxVel, calculatedSpeed);
    }
    
    /// <summary>
    /// Calculate predicted stop position based on initial velocity
    /// Uses TrajectorySimulator to get accurate prediction matching real physics
    /// </summary>
    private float CalculatePredictedStopPosition(float initialVelocity)
    {
        Debug.Log($"[FlickShot Prediction] ======== PREDICTION START ========");
        Debug.Log($"[FlickShot Prediction] Initial velocity: {initialVelocity:F2} m/s");
        Debug.Log($"[FlickShot Prediction] Aim direction: {aimDirection}, angle: {aimAngle:F1}°");
        
        // Use TrajectorySimulator to get REAL predicted stop position
        if (trajLine != null)
        {
            Debug.Log($"[FlickShot Prediction] trajLine found: {trajLine.GetType().Name}");
            
            // Get TrajectorySimulator from TrajectoryLine
            System.Type trajType = trajLine.GetType();
            
            // FIXED: Use correct field name "trajectorySimulator" not "simulator"
            System.Reflection.FieldInfo simulatorField = trajType.GetField("trajectorySimulator", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (simulatorField != null)
            {
                Debug.Log("[FlickShot Prediction] Found 'trajectorySimulator' field (private)!");
                object simulator = simulatorField.GetValue(trajLine);
                if (simulator != null)
                {
                    Debug.Log($"[FlickShot Prediction] Simulator is NOT null: {simulator.GetType().Name}");
                    return SimulatePrediction(simulator, initialVelocity);
                }
                else
                {
                    Debug.LogError("[FlickShot Prediction] Simulator field found but VALUE is null!");
                }
            }
            else
            {
                Debug.LogError("[FlickShot Prediction] Could not find 'trajectorySimulator' field!");
                Debug.LogError("[FlickShot Prediction] Check if field name has changed in TrajectoryLine!");
                ListAllMembers(trajType);
            }
        }
        else
        {
            Debug.LogError("[FlickShot Prediction] trajLine is NULL!");
        }
        
        // Fallback
        return FallbackPrediction(initialVelocity);
    }
    
    /// <summary>
    /// Debug helper: List all fields and properties on TrajectoryLine
    /// </summary>
    private void ListAllMembers(System.Type type)
    {
        Debug.Log($"[FlickShot Prediction] === Fields in {type.Name} ===");
        foreach (var field in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
        {
            Debug.Log($"  Field: {field.Name} ({field.FieldType.Name})");
        }
        
        Debug.Log($"[FlickShot Prediction] === Properties in {type.Name} ===");
        foreach (var prop in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
        {
            Debug.Log($"  Property: {prop.Name} ({prop.PropertyType.Name})");
        }
    }
    
    /// <summary>
    /// Run trajectory simulation with given simulator
    /// </summary>
    private float SimulatePrediction(object simulator, float initialVelocity)
    {
        if (simulator == null)
        {
            Debug.LogError("[FlickShot Prediction] SIMULATOR IS NULL - TrajectoryLine.simulator field exists but value is null!");
            Debug.LogError("[FlickShot Prediction] This means TrajectoryLine hasn't initialized its simulator yet.");
            return FallbackPrediction(initialVelocity);
        }
        
        Debug.Log($"[FlickShot Prediction] Simulator found and NOT null: {simulator.GetType().Name}");
        
        // Simulate trajectory with calculated velocity
        Vector2 startPos = new Vector2(0f, -25f); // Launcher position
        Vector2 testVelocity = aimDirection * initialVelocity;
        
        // CRITICAL: Get turn direction from Rock_Force RIGHT NOW (not at initialization)
        bool isInTurn = false;
        Rock_Force rockForce = GetComponent<Rock_Force>();
        if (rockForce != null)
        {
            System.Type forceType = rockForce.GetType();
            System.Reflection.FieldInfo flipAxisField = forceType.GetField("flipAxis", 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            if (flipAxisField != null)
            {
                isInTurn = (bool)flipAxisField.GetValue(rockForce);
            }
        }
        
        // Call SimulateTrajectory on the simulator
        System.Type simType = simulator.GetType();
        System.Reflection.MethodInfo simMethod = simType.GetMethod("SimulateTrajectory");
        
        if (simMethod != null)
        {
            // Parameters: startPos, velocity, isInTurn, maxPoints, rocksInPlay, forPlayerPreview
            // CRITICAL: Use forPlayerPreview=FALSE for flick shot - we want ACTUAL stop position WITHOUT sweeping!
            object[] parameters = new object[] { startPos, testVelocity, isInTurn, 300, null, false };
            
            // CRITICAL FIX: Invoke on INSTANCE (simulator), not TYPE (simType)!
            object result = simMethod.Invoke(simulator, parameters);
            
            if (result is List<Vector2> trajectory && trajectory.Count > 0)
            {
                Vector2 finalPos = trajectory[trajectory.Count - 1];
                
                Debug.Log($"[FlickShot Prediction] *** TrajectorySimulator SUCCESS! ***");
                Debug.Log($"  Velocity: {initialVelocity:F1} m/s, Direction: {aimDirection}, Angle: {aimAngle:F1}°");
                Debug.Log($"  Turn: {(isInTurn ? "IN" : "OUT")}, Points simulated: {trajectory.Count}");
                Debug.Log($"  Predicted UNSWEPT stop: Y = {finalPos.y:F2}");
                
                return finalPos.y;
            }
            else
            {
                Debug.LogWarning($"[FlickShot Prediction] SimulateTrajectory returned invalid result: {result?.GetType().Name ?? "null"}");
            }
        }
        else
        {
            Debug.LogWarning("[FlickShot Prediction] Could not find SimulateTrajectory method!");
        }
        
        // Fallback
        return FallbackPrediction(initialVelocity);
    }
    
    /// <summary>
    /// Fallback prediction using simple physics
    /// </summary>
    private float FallbackPrediction(float initialVelocity)
    {
        Debug.LogWarning("[FlickShot Prediction] Using fallback formula - TrajectorySimulator not available!");
        float hogLineY = -16f;
        float frictionFactor = 1.8f;
        float estimatedDistance = (initialVelocity * initialVelocity) / (2f * frictionFactor);
        float predictedStopY = hogLineY + estimatedDistance;
        predictedStopY = Mathf.Clamp(predictedStopY, -16f, 15f);
        
        Debug.Log($"[FlickShot Prediction] Fallback formula: velocity={initialVelocity:F1} m/s ? predicted Y={predictedStopY:F1}");
        return predictedStopY;
    }
    
    /// <summary>
    /// Calculate speed multiplier (0-1) from drag time
    /// Used by both preview and final velocity calculation
    /// UNIFIED FORMULA - matches slider timing!
    /// </summary>
    private float CalculateSpeedFromDragTime(float dragTime)
    {
        float normalizedSpeed;
        
        if (dragTime <= minDragTime)
        {
            // Ultra-fast drag = maximum speed
            normalizedSpeed = 1.0f;
        }
        else if (dragTime >= maxDragTime)
        {
            // Ultra-slow drag = minimum speed
            normalizedSpeed = 0.0f;
        }
        else
        {
            // Linear interpolation: faster drag = higher speed
            // Invert so shorter time = higher speed
            normalizedSpeed = 1.0f - ((dragTime - minDragTime) / (maxDragTime - minDragTime));
        }
        
        // Apply forgiveness factor (optional - makes it easier)
        normalizedSpeed = Mathf.Lerp(0.5f, normalizedSpeed, 1f / forgivenessFactor);
        
        return Mathf.Clamp01(normalizedSpeed);
    }
    
    /// <summary>
    /// Calculate which speed band the drag falls into
    /// Now uses unified formula from CalculateSpeedFromDragTime
    /// </summary>
    private void CalculateSpeedBand(float dragTime, float dragDistance)
    {
        // Use unified formula
        calculatedSpeed = CalculateSpeedFromDragTime(dragTime);
        
        // Calculate speed band from calculatedSpeed
        speedBand = Mathf.FloorToInt(calculatedSpeed * speedBands);
        speedBand = Mathf.Clamp(speedBand, 0, speedBands - 1);
    }
    
    /// <summary>
    /// Initialize speed slider for power phase
    /// Auto-detects slider and components in scene
    /// </summary>
    private void InitializeSpeedSlider()
    {
        // Auto-detect speed slider by name
        GameObject sliderObj = GameObject.Find(speedSliderName);
        if (sliderObj != null)
        {
            speedSlider = sliderObj.GetComponent<UnityEngine.UI.Slider>();
            
            if (speedSlider != null)
            {
                Debug.Log($"[FlickShot] Found speed slider: {speedSliderName}");
                
                // Auto-detect handle image from slider
                if (speedSlider.handleRect != null)
                {
                    sliderHandleImage = speedSlider.handleRect.GetComponent<UnityEngine.UI.Image>();
                    
                    if (sliderHandleImage != null)
                    {
                        Debug.Log($"[FlickShot] Found slider handle image");
                    }
                    else
                    {
                        Debug.LogWarning($"[FlickShot] Slider handle has no Image component!");
                    }
                }
                else
                {
                    Debug.LogWarning($"[FlickShot] Slider has no handle rect!");
                }
            }
            else
            {
                Debug.LogWarning($"[FlickShot] GameObject '{speedSliderName}' found but has no Slider component!");
            }
        }
        else
        {
            Debug.LogWarning($"[FlickShot] Speed slider '{speedSliderName}' not found in scene - slider disabled");
            return;
        }
        
        // Auto-detect shooter animator
        shooterAnim = FindObjectOfType<ShooterAnim>();
        if (shooterAnim != null)
        {
            Debug.Log($"[FlickShot] Found shooter animator: {shooterAnim.name}");
        }
        
        // Configure slider if found
        if (speedSlider != null)
        {
            // CRITICAL: Make sure GameObject is active BEFORE configuring!
            if (!speedSlider.gameObject.activeSelf)
            {
                speedSlider.gameObject.SetActive(true);
                Debug.Log($"[FlickShot] Speed slider GameObject re-enabled for new turn");
            }
            
            speedSlider.minValue = 0f;
            speedSlider.maxValue = 1f;
            speedSlider.value = 0f;
            speedSlider.interactable = false; // Visual only
            
            // Calculate ideal drag time based on target speed band
            idealDragTime = CalculateIdealDragTime();
            ghostCycleStartTime = Time.time;
            isSliderActive = true;
            lastPlayerSliderValue = 0f;
            
            // Set ghost rock to 50% opacity (shows ideal timing)
            if (sliderHandleImage != null)
            {
                Color ghostColor = sliderHandleImage.color;
                ghostColor.a = 0.5f;
                sliderHandleImage.color = ghostColor;
            }
            
            Debug.Log($"[FlickShot] Speed slider initialized - ideal time: {idealDragTime:F2}s");
        }
    }
    
    /// <summary>
    /// Update speed slider animation each frame
    /// Ghost rock cycles at ideal speed, player rock follows cursor
    /// FIXED: Check phase FIRST to avoid hiding slider during PowerPhase!
    /// </summary>
    private void UpdateSpeedSlider()
    {
        // CRITICAL FIX: Check if NOT in power phase FIRST, then hide
        if (currentPhase != FlickShotPhase.PowerPhase)
        {
            // Not in power phase anymore - ensure slider is hidden
            if (speedSlider != null && speedSlider.gameObject.activeSelf)
            {
                speedSlider.gameObject.SetActive(false);
                Debug.Log("[FlickShot] Slider hidden - not in power phase");
            }
            isSliderActive = false;
            return;
        }
        
        // Now check if slider exists and is initialized
        if (!isSliderActive || speedSlider == null)
        {
            Debug.LogWarning("[FlickShot] Slider not active or null - skipping update");
            return;
        }
        
        if (!isPowerDragging)
        {
            // BEFORE DRAG: Animate ghost rock cycling at ideal speed
            // Cycle: 0?1 (animate up) ? pause ? fade out ? reappear at 0 ? repeat
            
            float elapsedTime = Time.time - ghostCycleStartTime;
            float cycleDuration = idealDragTime * 1.5f; // Total cycle time (includes pause/fade)
            float cycleProgress = (elapsedTime % cycleDuration) / cycleDuration;
            
            float ghostValue = 0f;
            float ghostAlpha = 0.5f;
            
            // Split cycle into phases:
            // 0.0-0.53: Animate up (0?1) over idealDragTime
            // 0.53-0.80: Pause at top (LONGER pause - 27% of cycle!)
            // 0.80-1.0: Fade out and reset to bottom
            
            float animatePhase = idealDragTime / cycleDuration;  // ~0.53 typically
            float pausePhase = animatePhase + 0.27f;             // ~0.80 (LONGER pause!)
            float fadePhase = 1.0f;                              // 1.0 (end)
            
            if (cycleProgress < animatePhase)
            {
                // PHASE 1: Animate up (0?1)
                ghostValue = cycleProgress / animatePhase;
                ghostAlpha = 0.5f; // Visible
            }
            else if (cycleProgress < pausePhase)
            {
                // PHASE 2: Pause at top
                ghostValue = 1.0f; // At top
                ghostAlpha = 0.5f; // Visible
            }
            else
            {
                // PHASE 3: Fade out and reset to bottom
                ghostValue = 0f; // Back to bottom
                float fadeProgress = (cycleProgress - pausePhase) / (fadePhase - pausePhase);
                ghostAlpha = 0.5f * (1f - fadeProgress); // Fade from 0.5 to 0
            }
            
            speedSlider.value = ghostValue;
            
            // Update alpha
            if (sliderHandleImage != null)
            {
                Color ghostColor = Color.white;
                ghostColor.a = ghostAlpha;
                sliderHandleImage.color = ghostColor;
            }
        }
        else
        {
            // DURING DRAG: Show player's actual drag progress
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float targetProgress = CalculateSwipeProgress(mouseWorldPos.y);
            
            // Smooth the slider movement (prevents jitter)
            float smoothedProgress = Mathf.Lerp(lastPlayerSliderValue, targetProgress, Time.deltaTime * 10f);
            lastPlayerSliderValue = smoothedProgress;
            
            speedSlider.value = smoothedProgress;
            
            // Change handle to full opacity during drag
            if (sliderHandleImage != null)
            {
                Color playerColor = sliderHandleImage.color;
                playerColor.a = 1f; // Full opacity
                
                // Color feedback: Green if matching ghost speed, yellow/red if off
                float speedRatio = CalculateSpeedMatchingRatio(smoothedProgress);
                if (Mathf.Abs(speedRatio - 1f) < 0.15f)
                    playerColor = Color.Lerp(playerColor, Color.green, 0.5f); // Perfect!
                else if (Mathf.Abs(speedRatio - 1f) < 0.3f)
                    playerColor = Color.Lerp(playerColor, Color.yellow, 0.5f); // Close
                else
                    playerColor = Color.Lerp(playerColor, Color.red, 0.5f); // Off
                
                sliderHandleImage.color = playerColor;
            }
            
            // Link to shooter animation (if available)
            UpdateShooterAnimationFromSlider(smoothedProgress);
        }
    }
    
    /// <summary>
    /// Smooth swipe path using simple averaging for cleaner visual
    /// Reduces jitter from rapid cursor movements
    /// </summary>
    private List<Vector3> SmoothSwipePath(List<Vector3> rawPoints)
    {
        if (rawPoints.Count < 4) return rawPoints;
        
        List<Vector3> smoothed = new List<Vector3>();
        
        // Keep first point as-is
        smoothed.Add(rawPoints[0]);
        
        // Smooth middle points using 3-point average
        for (int i = 1; i < rawPoints.Count - 1; i++)
        {
            Vector3 prev = rawPoints[i - 1];
            Vector3 current = rawPoints[i];
            Vector3 next = rawPoints[i + 1];
            
            // Average of previous, current, and next point
            Vector3 smoothPoint = (prev + current + next) / 3f;
            smoothed.Add(smoothPoint);
        }
        
        // Keep last point as-is
        smoothed.Add(rawPoints[rawPoints.Count - 1]);
        
        return smoothed;
    }
    
    /// <summary>
    /// Calculate swipe progress (0-1) from cursor Y position
    /// Maps launcher (-25) to hog line (-16)
    /// </summary>
    private float CalculateSwipeProgress(float cursorY)
    {
        float startY = powerDragStartY;   // -25f
        float endY = powerDragTargetY;    // -16f
        return Mathf.InverseLerp(startY, endY, cursorY);
    }
    
    /// <summary>
    /// Calculate ideal drag time for current target speed band
    /// FIXED: Now matches unified speed formula exactly!
    /// Perfect (middle) = ~0.8s, Faster = shorter, Slower = longer
    /// </summary>
    private float CalculateIdealDragTime()
    {
        // Perfect band = middle band (0.5 normalized speed)
        // From CalculateSpeedFromDragTime: 0.5 = midpoint between min and max drag time
        
        float perfectNormalized = 0.5f; // Middle speed band
        
        // Solve for drag time that gives 0.5 normalized speed:
        // 0.5 = 1.0 - ((dragTime - minDragTime) / (maxDragTime - minDragTime))
        // 0.5 = (dragTime - minDragTime) / (maxDragTime - minDragTime)
        // dragTime = minDragTime + 0.5 * (maxDragTime - minDragTime)
        
        float idealTime = minDragTime + (perfectNormalized * (maxDragTime - minDragTime));
        
        Debug.Log($"[FlickShot] Ideal drag time calculated: {idealTime:F2}s (min: {minDragTime:F2}, max: {maxDragTime:F2})");
        
        return idealTime;
    }
    
    /// <summary>
    /// Calculate how well player's speed matches ghost speed
    /// Returns ratio: 1.0 = perfect match, <1 = too slow, >1 = too fast
    /// </summary>
    private float CalculateSpeedMatchingRatio(float playerProgress)
    {
        float elapsedTime = Time.time - powerDragStartTime;
        if (elapsedTime < 0.01f) return 1f; // Avoid divide by zero
        
        float playerSpeed = playerProgress / elapsedTime;
        float ghostSpeed = 1f / idealDragTime;
        
        return playerSpeed / ghostSpeed;
    }
    
    /// <summary>
    /// Link slider progress to shooter animation
    /// Allows swipe to control animation frame (optional feature)
    /// </summary>
    private void UpdateShooterAnimationFromSlider(float sliderProgress)
    {
        if (shooterAnim == null) return;
        
        // Map slider progress (0-1) to shooter animation
        // This creates a "drag shooter along with your finger" effect
        // shooterAnim should have a method to set animation progress directly
        
        // Example: shooterAnim.SetAnimationProgress(sliderProgress);
        // (Implement this in ShooterAnim if you want the feature)
    }
    
    /// <summary>
    /// Hide and clean up speed slider
    /// Clears references for next turn
    /// </summary>
    private void CleanupSpeedSlider()
    {
        if (speedSlider != null)
        {
            speedSlider.gameObject.SetActive(false);
            isSliderActive = false;
        }
        
        // Reset handle color
        if (sliderHandleImage != null)
        {
            Color resetColor = sliderHandleImage.color;
            resetColor.a = 0.5f;
            sliderHandleImage.color = resetColor;
        }
        
        // Clear references for next turn (will be auto-detected again)
        speedSlider = null;
        sliderHandleImage = null;
        shooterAnim = null;
        
        Debug.Log("[FlickShot] Speed slider cleaned up - references cleared for next turn");
    }
    
    /// <summary>
    /// Get feedback message for current speed with more precision (7 bands)
    /// </summary>
    private string GetSpeedFeedbackMessage()
    {
        // 7 speed bands: 0=Very Slow, 1=Slow, 2=Slow-Med, 3=PERFECT, 4=Med-Fast, 5=Fast, 6=Very Fast
        int perfectBand = speedBands / 2; // Middle band is "perfect" (band 3 for 7 bands)
        
        if (speedBand == perfectBand)
            return "Perfect!";
        else if (speedBand == perfectBand - 1)
            return "Slightly Slow";
        else if (speedBand == perfectBand + 1)
            return "Slightly Fast";
        else if (speedBand < perfectBand - 1)
        {
            if (speedBand == 0)
                return "Way Too Slow!";
            else
                return "Too Slow";
        }
        else // speedBand > perfectBand + 1
        {
            if (speedBand == speedBands - 1)
                return "Way Too Fast!";
            else
                return "Too Fast";
        }
    }
    
    /// <summary>
    /// Helper method to show callout using reflection (avoids hard dependency on TextCalloutManager)
    /// </summary>
    private void ShowCallout(Vector2 position, string message, bool followTarget, float duration)
    {
        // Find TextCalloutManager using reflection
        System.Type calloutManagerType = System.Type.GetType("TextCalloutManager");
        if (calloutManagerType != null)
        {
            System.Reflection.PropertyInfo instanceProp = calloutManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceProp != null)
            {
                object calloutManager = instanceProp.GetValue(null);
                if (calloutManager != null)
                {
                    // Find the specific ShowCallout overload with 5 parameters
                    System.Type[] paramTypes = new System.Type[] {
                        typeof(Vector2),
                        typeof(string),
                        typeof(bool),
                        typeof(Transform),
                        typeof(float)
                    };
                    System.Reflection.MethodInfo showMethod = calloutManagerType.GetMethod("ShowCallout", paramTypes);
                    if (showMethod != null)
                    {
                        showMethod.Invoke(calloutManager, new object[] { position, message, followTarget, followTarget ? transform : null, duration });
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Release flick shot and calculate final velocity
    /// Show final feedback AFTER release
    /// </summary>
    private void ReleaseFlickShot(float dragTime, float dragDistance)
    {
        currentPhase = FlickShotPhase.Released;
        isPowerDragging = false;
        
        // Stop velocity guide if still active
        if (velocityGuide != null && velocityGuide.IsActive)
        {
            velocityGuide.StopGuide();
            Debug.Log("[FlickShot] Velocity guide stopped on release");
        }
        
        // DISABLED: Shooter animation control removed
        // if (isShooterAnimControlActive && shooterAnim != null)
        // {
        //     shooterAnim.CompleteRelease();
        //     isShooterAnimControlActive = false;
        //     Debug.Log("[FlickShot] Shooter animation released - natural follow-through!");
        // }
        
        // Calculate final speed
        CalculateSpeedBand(dragTime, dragDistance);
        float targetSpeed = GetPredictedVelocity();
        float predictedStopY = CalculatePredictedStopPosition(targetSpeed);
        
        Debug.Log($"[FlickShot] RELEASED - Time: {dragTime:F3}s, Speed: {calculatedSpeed:F2}, Band: {speedBand}");
        Debug.Log($"[FlickShot] *** CYAN LINE PREDICTION: Y = {predictedStopY:F2} ***");
        Debug.Log($"[FlickShot] *** TARGET VELOCITY: {targetSpeed:F2} m/s ***");
        Debug.Log($"[FlickShot] *** AIM DIRECTION: {aimDirection}, ANGLE: {aimAngle:F1}° ***");
        
        // Show predicted stop line (CYAN horizontal line at predicted Y)
        if (predictedStopLine != null)
        {
            float lineWidth = 3f;
            Vector3 leftPoint = new Vector3(-lineWidth, predictedStopY, -1f);
            Vector3 rightPoint = new Vector3(lineWidth, predictedStopY, -1f);
            
            predictedStopLine.SetPosition(0, leftPoint);
            predictedStopLine.SetPosition(1, rightPoint);
            predictedStopLine.enabled = true;
            
            Debug.Log($"[FlickShot] *** CYAN LINE DRAWN AT Y={predictedStopY:F2} (Watch where rock actually stops!) ***");
        }
        
        // ?? FIX: Hide cyan line when rock STOPS moving (not after 0.5s)
        // This way you can see the prediction vs actual result!
        StartCoroutine(HidePredictionLineWhenRockStops());
        
        // Show detailed speed callout that FOLLOWS the rock - USING STACKING!
        if (showSpeedFeedback)
        {
            // Get velocity range from TrajectoryLine
            float minVel = 5f;
            float maxVel = 13f;
            
            if (minVelocityProp != null && trajLine != null)
            {
                object minVelObj = minVelocityProp.GetValue(trajLine);
                if (minVelObj != null) minVel = (float)minVelObj;
            }
            if (maxVelocityProp != null && trajLine != null)
            {
                object maxVelObj = maxVelocityProp.GetValue(trajLine);
                if (maxVelObj != null) maxVel = (float)maxVelObj;
            }
            
            // Calculate what "raw" input would have been (before quantization)
            float rawNormalizedSpeed = Mathf.Lerp(calculatedSpeed, 0.5f, forgivenessFactor - 1f);
            float rawDragTime = minDragTime + ((1f - rawNormalizedSpeed) * (maxDragTime - minDragTime));
            float rawSpeed = Mathf.Lerp(minVel, maxVel, rawNormalizedSpeed);
            float correctionAmount = targetSpeed - rawSpeed;
            
            // STACKED CALLOUTS - each piece of info gets its own callout!
            if (TextCalloutManager.Instance != null)
            {
                Vector3 rockPosition = rb != null ? (Vector3)rb.position : transform.position;
                
                // Callout 1: Speed feedback message (Perfect!/Too Fast/etc.)
                string speedMessage = GetSpeedFeedbackMessage();
                TextCalloutManager.Instance.ShowRockCallout(gameObject, speedMessage);
                
                // Callout 2: Actual velocity
                TextCalloutManager.Instance.ShowRockCallout(gameObject, $"{targetSpeed:F2} m/s");
                
                // Callout 3: Adjustment (if any)
                if (Mathf.Abs(correctionAmount) > 0.01f)
                {
                    string adjustmentText = correctionAmount > 0 ? 
                        $"+{correctionAmount:F2} m/s" : 
                        $"{correctionAmount:F2} m/s";
                    TextCalloutManager.Instance.ShowRockCallout(gameObject, adjustmentText);
                }
                
                // Callout 4: Predicted stop
                TextCalloutManager.Instance.ShowRockCallout(gameObject, $"Stop: Y={predictedStopY:F1}");
                
                // Callout 5: Input time
                TextCalloutManager.Instance.ShowRockCallout(gameObject, $"Time: {dragTime:F2}s");
                
                Debug.Log($"[FlickShot] *** STACKED SPEED CALLOUTS: {speedMessage} | {targetSpeed:F2} m/s | Predicted Y={predictedStopY:F1} | Time={dragTime:F2}s ***");
            }
            else
            {
                Debug.LogWarning("[FlickShot] TextCalloutManager.Instance is null! Callout not shown.");
            }
        }
        
        // CRITICAL: Start rock timer display!
        RockTimerDisplay timerDisplay = GetComponent<RockTimerDisplay>();
        if (timerDisplay != null)
        {
            timerDisplay.StartTimer();
            Debug.Log("[FlickShot] Rock timer started!");
        }
        else
        {
            Debug.LogWarning("[FlickShot] No RockTimerDisplay component found on rock!");
        }
        
        // Calculate and apply velocity
        Vector2 finalVelocity = aimDirection * targetSpeed;
        Debug.Log($"[FlickShot] *** ACTUAL VELOCITY APPLIED: {finalVelocity.magnitude:F2} m/s at angle {aimAngle:F1}° ***");
        
        ApplyFlickShotVelocity(finalVelocity);
        
        // Hide swipe trail after 1 second
        StartCoroutine(HideSwipeTrailAfterDelay(1f));
        
        // Keep cyan predicted line visible until turn ends (don't hide it!)
        
        // Hide speed slider
        CleanupSpeedSlider();
        
        // Start coroutine to monitor actual stop position
        StartCoroutine(MonitorActualStopPosition(predictedStopY));
    }
    
    /// <summary>
    /// Monitor where rock actually stops and compare to prediction
    /// </summary>
    private IEnumerator MonitorActualStopPosition(float predictedY)
    {
        // Wait for rock to stop moving
        float stoppedTime = 0f;
        Vector2 lastPos = rb.position;
        
        while (stoppedTime < 0.5f) // Rock must be stopped for 0.5s
        {
            yield return new WaitForSeconds(0.1f);
            
            float distMoved = Vector2.Distance(rb.position, lastPos);
            
            if (distMoved < 0.01f && rb.linearVelocity.magnitude < 0.1f)
            {
                stoppedTime += 0.1f;
            }
            else
            {
                stoppedTime = 0f;
            }
            
            lastPos = rb.position;
        }
        
        // Rock has stopped!
        float actualY = rb.position.y;
        float error = actualY - predictedY;
        float errorPercent = Mathf.Abs(error / (predictedY + 25f)) * 100f; // Percent of ice length
        
        Debug.Log($"[FlickShot] ========================================");
        Debug.Log($"[FlickShot] *** PREDICTION ACCURACY REPORT ***");
        Debug.Log($"[FlickShot] Predicted Y: {predictedY:F2}");
        Debug.Log($"[FlickShot] Actual Y: {actualY:F2}");
        Debug.Log($"[FlickShot] Error: {error:F2} meters ({(error > 0 ? "too short" : "too long")})");
        Debug.Log($"[FlickShot] Error %: {errorPercent:F1}% of ice length");
        Debug.Log($"[FlickShot] ========================================");
        
        // Show error callout as STACKED messages
        if (TextCalloutManager.Instance != null && Mathf.Abs(error) > 0.5f)
        {
            // Callout 1: Error label
            TextCalloutManager.Instance.ShowRockCallout(gameObject, "Prediction Error:");
            
            // Callout 2: Error amount with direction
            string errorText = $"{Mathf.Abs(error):F2}m {(error > 0 ? "short" : "long")}";
            TextCalloutManager.Instance.ShowRockCallout(gameObject, errorText);
        }
    }
    
    /// <summary>
    /// Hide swipe trail after delay
    /// </summary>
    private IEnumerator HideSwipeTrailAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (swipeTrailLine != null)
        {
            swipeTrailLine.enabled = false;
            swipePoints.Clear();
            Debug.Log("[FlickShot] Swipe trail hidden");
        }
    }
    
    /// <summary>
    /// ?? FIX: Hide cyan prediction line after delay (syncs with trajectory line)
    /// </summary>
    private IEnumerator HidePredictionLineAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HidePredictionLine();
    }
    
    /// <summary>
    /// ?? NEW: Hide cyan line when rock STOPS moving (not on timer)
    /// This lets you compare prediction vs actual stop position!
    /// </summary>
    private IEnumerator HidePredictionLineWhenRockStops()
    {
        if (rb == null) yield break;
        
        // Wait for rock to start moving first
        yield return new WaitForSeconds(0.5f);
        
        // Now wait for rock to stop (velocity < 0.1 m/s for 0.5 seconds)
        float stoppedTime = 0f;
        Vector2 lastPos = rb.position;
        
        while (stoppedTime < 0.5f)
        {
            yield return new WaitForSeconds(0.1f);
            
            float distMoved = Vector2.Distance(rb.position, lastPos);
            
            if (distMoved < 0.01f && rb.linearVelocity.magnitude < 0.1f)
            {
                stoppedTime += 0.1f;
            }
            else
            {
                stoppedTime = 0f; // Reset if rock is still moving
            }
            
            lastPos = rb.position;
        }
        
        // Rock has stopped - now hide the cyan line
        Debug.Log($"[FlickShot] ?? Rock stopped at Y={rb.position.y:F2} - hiding cyan prediction line");
        HidePredictionLine();
    }
    
    /// <summary>
    /// Apply calculated velocity to rock using normal launch system
    /// Maps drag speed to synthetic pullback distance, then triggers normal Rock_Flick release
    /// </summary>
    private void ApplyFlickShotVelocity(Vector2 velocity)
    {
        // Get velocity and pullback ranges from TrajectoryLine
        float minVel = 5f;
        float maxVel = 13f;
        float minPullback = 1.5f;
        float maxPullback = 5.5f;
        
        // Try to get velocity range from TrajectoryLine
        if (minVelocityProp != null && trajLine != null)
        {
            object minVelObj = minVelocityProp.GetValue(trajLine);
            if (minVelObj != null) minVel = (float)minVelObj;
        }
        if (maxVelocityProp != null && trajLine != null)
        {
            object maxVelObj = maxVelocityProp.GetValue(trajLine);
            if (maxVelObj != null) maxVel = (float)maxVelObj;
        }
        
        // Get pullback range from TrajectoryLine
        if (trajLine != null)
        {
            System.Type trajType = trajLine.GetType();
            System.Reflection.PropertyInfo minPullProp = trajType.GetProperty("minPullbackDistance");
            System.Reflection.PropertyInfo maxPullProp = trajType.GetProperty("maxPullbackDistance");
            
            if (minPullProp != null)
            {
                object minPullObj = minPullProp.GetValue(trajLine);
                if (minPullObj != null) minPullback = (float)minPullObj;
            }
            if (maxPullProp != null)
            {
                object maxPullObj = maxPullProp.GetValue(trajLine);
                if (maxPullObj != null) maxPullback = (float)maxPullObj;
            }
        }
        
        // Calculate synthetic pullback distance from desired velocity
        // Inverse of: velocity = Lerp(minVel, maxVel, (pullback - minPull) / (maxPull - minPull))
        float targetSpeed = velocity.magnitude;
        float normalizedVel = Mathf.InverseLerp(minVel, maxVel, targetSpeed);
        float syntheticPullback = Mathf.Lerp(minPullback, maxPullback, normalizedVel);
        syntheticPullback = Mathf.Clamp(syntheticPullback, minPullback, maxPullback);
        
        Debug.Log($"[FlickShot] Calculated synthetic pullback: {syntheticPullback:F2} units for target velocity {targetSpeed:F2} m/s");
        
        // CRITICAL: DON'T position rock at pullback location!
        // Rock must stay at launcher so it crosses hog line trigger
        // Instead, we'll directly set the velocity in Rock_Flick
        Vector2 launcherPos = launcher.transform.position;
        rb.position = launcherPos; // Keep rock AT LAUNCHER
        
        Debug.Log($"[FlickShot] Rock positioned at LAUNCHER: {launcherPos} (will cross hog line trigger)");
        
        // CRITICAL: Set shotTaken = true so GameManager knows the shot is happening
        // But DON'T set released yet - wait for rock to actually start moving!
        if (rockInfo != null)
        {
            System.Reflection.FieldInfo shotTakenField = rockInfo.GetType().GetField("shotTaken");
            if (shotTakenField != null)
            {
                shotTakenField.SetValue(rockInfo, true);
                Debug.Log("[FlickShot] Set Rock_Info.shotTaken = true");
            }
            
            // DON'T set released = true yet!
            // It will be set after the rock actually starts moving
        }
        
        // CRITICAL: Call TrajectoryLine.Release() to hide trajectory
        if (trajLine != null)
        {
            System.Type trajType = trajLine.GetType();
            System.Reflection.MethodInfo releaseMethod = trajType.GetMethod("Release");
            if (releaseMethod != null)
            {
                releaseMethod.Invoke(trajLine, null);
                Debug.Log("[FlickShot] Called TrajectoryLine.Release() to hide trajectory");
            }
        }
        
        // CRITICAL: Unparent shooting knob from rock!
        if (shootingKnobObj != null)
        {
            Component shootKnobComp = shootingKnobObj.GetComponent("ShootingKnob");
            if (shootKnobComp != null)
            {
                System.Reflection.MethodInfo unparentMethod = shootKnobComp.GetType().GetMethod("UnParentandHide");
                if (unparentMethod != null)
                {
                    unparentMethod.Invoke(shootKnobComp, null);
                    Debug.Log("[FlickShot] Called ShootingKnob.UnParentandHide() - rock is now free!");
                }
            }
        }
        
        // CRITICAL: Make rock visible!
        SpriteRenderer rockSprite = GetComponent<SpriteRenderer>();
        if (rockSprite != null)
        {
            rockSprite.enabled = true;
            Debug.Log("[FlickShot] Rock sprite enabled");
        }
        
        // CRITICAL: Directly apply the calculated velocity
        // Don't rely on Rock_Flick.Release() to calculate from position
        rb.isKinematic = false;
        rb.linearDamping = 0f; // Will be restored at hog line
        rb.linearVelocity = velocity;
        
        // CRITICAL: Enable continuous collision detection for high-speed collisions!
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // CRITICAL: Disable ALL other components that might interfere!
        Rock_Force rockForce = GetComponent<Rock_Force>();
        if (rockForce != null)
        {
            rockForce.enabled = false;
            Debug.Log("[FlickShot] Rock_Force disabled - will be enabled at hog line");
        }
        
        // CRITICAL: Disable Rock_Colliders temporarily to prevent premature OutOfPlay trigger!
        Rock_Colliders rockColliders = GetComponent<Rock_Colliders>();
        if (rockColliders != null)
        {
            rockColliders.enabled = false;
            Debug.Log("[FlickShot] Rock_Colliders DISABLED during launch - will be enabled after crossing Y=-20");
            
            // Start coroutine to re-enable it after rock moves past Y=-20
            StartCoroutine(ReenableCollidersAfterLaunch());
        }
        
        Debug.Log($"[FlickShot] Physics applied - isKinematic: {rb.isKinematic}, velocity: {rb.linearVelocity.magnitude:F2} m/s, position: {rb.position}, damping: {rb.linearDamping}, collisionMode: {rb.collisionDetectionMode}");
        
        // Disable spring
        SpringJoint2D spring = GetComponent<SpringJoint2D>();
        if (spring != null)
        {
            spring.enabled = false;
        }
        
        // Enable launcher collider
        if (launcher != null)
        {
            Collider2D launcherCol = launcher.GetComponent<Collider2D>();
            if (launcherCol != null) launcherCol.enabled = true;
        }
        
        // Adjust collider size and ENSURE it's enabled for physics collisions!
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            col.radius = 0.14f;
            col.enabled = true; // CRITICAL: Enable collider for physics!
            Debug.Log($"[FlickShot] CircleCollider2D enabled - radius: {col.radius}, isTrigger: {col.isTrigger}");
        }
        
        // Disable Rock_Flick component (we're handling the launch)
        Component rockFlickComp = GetComponent("Rock_Flick");
        if (rockFlickComp != null)
        {
            ((MonoBehaviour)rockFlickComp).enabled = false;
            Debug.Log("[FlickShot] Rock_Flick disabled - flick shot handling launch directly");
        }
        
        // Play release sound
        AudioSource[] rockSounds = GetComponents<AudioSource>();
        if (rockSounds != null && rockSounds.Length > 1)
        {
            rockSounds[1].enabled = true;
        }
        
        Debug.Log($"[FlickShot] Rock launched directly with velocity: {velocity.magnitude:F2} m/s at angle {aimAngle:F1}°");
        
        // Start coroutine to set released = true after rock starts moving
        StartCoroutine(SetReleasedAfterMoving());
    }
    
    /// <summary>
    /// Wait for rock to actually start moving, then set released = true
    /// </summary>
    private IEnumerator SetReleasedAfterMoving()
    {
        // Wait for next FixedUpdate so physics applies velocity
        yield return new WaitForFixedUpdate();
        
        Debug.Log($"[FlickShot] After FixedUpdate - velocity: {rb.linearVelocity.magnitude:F2} m/s, position: {rb.position}");
        
        // Now set released = true
        if (rockInfo != null)
        {
            System.Reflection.FieldInfo releasedField = rockInfo.GetType().GetField("released");
            if (releasedField != null)
            {
                releasedField.SetValue(rockInfo, true);
                Debug.Log($"[FlickShot] Set Rock_Info.released = true AFTER velocity applied (velocity: {rb.linearVelocity.magnitude:F2} m/s)");
            }
        }
        
        // Monitor rock for a few seconds to see what happens
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(0.5f);
            Debug.Log($"[FlickShot] Rock monitor [{i * 0.5f}s]: pos={rb.position}, vel={rb.linearVelocity.magnitude:F2} m/s, isKinematic={rb.isKinematic}, parent={transform.parent?.name ?? "null"}");
            
            if (rb.linearVelocity.magnitude < 0.1f)
            {
                Debug.LogWarning($"[FlickShot] Rock stopped moving at {rb.position} after {i * 0.5f}s!");
                break;
            }
        }
    }
    
    /// <summary>
    /// Re-enable Rock_Colliders and Rock_Force after rock has crossed hog line
    /// </summary>
    private IEnumerator ReenableCollidersAfterLaunch()
    {
        // Wait until rock crosses hog line (Y=-16)
        while (rb.position.y < -16f)
        {
            yield return new WaitForFixedUpdate();
        }
        
        Debug.Log($"[FlickShot] Rock crossed hog line (Y=-16), re-enabling components at position {rb.position}");
        
        // Re-enable Rock_Colliders for trigger detection
        Rock_Colliders rockColliders = GetComponent<Rock_Colliders>();
        if (rockColliders != null)
        {
            rockColliders.enabled = true;
            Debug.Log("[FlickShot] Rock_Colliders re-enabled for trigger detection");
        }
        
        // CRITICAL: Re-enable Rock_Force for curl and friction!
        // Rock_Force.Release() will handle setting proper damping
        Rock_Force rockForce = GetComponent<Rock_Force>();
        if (rockForce != null)
        {
            rockForce.enabled = true;
            Debug.Log("[FlickShot] Rock_Force re-enabled for curl and friction");
        }
        
        // DON'T set linearDamping here - let Rock_Force.Release() handle it!
        // Rock_Force will be triggered by Rock_Release trigger and will set proper damping
        Debug.Log($"[FlickShot] Waiting for Rock_Release trigger to restore damping (current: {rb.linearDamping})");
    }
    
    /// <summary>
    /// Cancel flick shot (e.g., if player clicks away)
    /// </summary>
    public void CancelFlickShot()
    {
        currentPhase = FlickShotPhase.Inactive;
        
        // Hide visual elements
        if (swipeTrailLine != null)
        {
            swipeTrailLine.enabled = false;
            swipePoints.Clear();
        }
        
        if (predictedStopLine != null)
        {
            predictedStopLine.enabled = false;
        }
        
        // Reset rock to launcher position
        if (launcher != null)
        {
            transform.position = launcher.transform.position;
            SpringJoint2D spring = GetComponent<SpringJoint2D>();
            if (spring != null)
            {
                spring.dampingRatio = 1f;
                spring.frequency = 10000f;
            }
        }
        
        Debug.Log("[FlickShot] Shot cancelled");
    }
    
    void OnDestroy()
    {
        // Clean up visual elements
        if (swipeTrailLine != null)
        {
            swipeTrailLine.enabled = false;
        }
        
        if (predictedStopLine != null)
        {
            predictedStopLine.enabled = false;
        }
        
        // Unsubscribe from events using reflection
        System.Type settingsType = System.Type.GetType("GameVisualizationSettings");
        if (settingsType != null)
        {
            System.Reflection.PropertyInfo instanceProp = settingsType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceProp != null)
            {
                object visualSettings = instanceProp.GetValue(null);
                if (visualSettings != null)
                {
                    System.Reflection.EventInfo modeChangedEvent = settingsType.GetEvent("OnFlickShotModeChanged");
                    if (modeChangedEvent != null)
                    {
                        System.Delegate handler = System.Delegate.CreateDelegate(modeChangedEvent.EventHandlerType, this, "OnFlickShotModeChanged");
                        modeChangedEvent.RemoveEventHandler(visualSettings, handler);
                    }
                }
            }
        }
    }
    
    void OnDisable()
    {
        // Clean up visual elements when rock is disabled (reset for next turn)
        if (swipeTrailLine != null)
        {
            swipeTrailLine.enabled = false;
            swipePoints.Clear();
            Debug.Log("[FlickShot] Swipe trail hidden (OnDisable)");
        }
        
        if (predictedStopLine != null)
        {
            predictedStopLine.enabled = false;
            Debug.Log("[FlickShot] Predicted stop line hidden (OnDisable)");
        }
        
        // Clean up speed slider
        CleanupSpeedSlider();
        
        // Reset phase
        currentPhase = FlickShotPhase.Inactive;
        isPowerDragging = false;
    }
    
    /// <summary>
    /// ?? FIX: Hide cyan prediction line when shot is released
    /// Called from ReleaseFlickShot() to sync with trajectory line visibility
    /// </summary>
    private void HidePredictionLine()
    {
        if (predictedStopLine != null)
        {
            predictedStopLine.enabled = false;
            Debug.Log("[FlickShot] ?? Cyan prediction line hidden when shot released");
        }
    }
}
