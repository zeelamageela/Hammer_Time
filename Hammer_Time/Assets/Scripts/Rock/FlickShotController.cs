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
    
    [Tooltip("Green shooting knob that follows drag in power phase")]
    public GameObject powerKnobObj;
    
    [Tooltip("Line renderer for power swipe visualization")]
    private LineRenderer powerSwipeLine;
    
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
    [Tooltip("Number of speed bands (e.g., 5 = Very Slow, Slow, Medium, Fast, Very Fast)")]
    [Range(3, 10)]
    public int speedBands = 5;
    
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
    
    // Power phase state
    private Vector2 powerDragStartPos;
    private float powerDragStartTime;
    private float lastFeedbackTime;
    private string lastFeedbackMessage = "";
    private bool isPowerDragging = false;  // Track if we're actively dragging
    
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
        
        // Create power phase knob (clone of shooting knob)
        if (shootingKnobObj != null && powerKnobObj == null)
        {
            powerKnobObj = Instantiate(shootingKnobObj);
            powerKnobObj.name = "PowerKnob";
            powerKnobObj.SetActive(false);
            
            // Set to green color
            SpriteRenderer powerSprite = powerKnobObj.GetComponent<SpriteRenderer>();
            if (powerSprite != null)
            {
                powerSprite.color = new Color(0.2f, 1f, 0.2f, 1f); // Bright green
            }
            
            // Get the line renderer from power knob (we'll use it for swipe visualization)
            powerSwipeLine = powerKnobObj.GetComponent<LineRenderer>();
            if (powerSwipeLine != null)
            {
                // Enable and configure the line for swipe visualization
                powerSwipeLine.enabled = true;
                powerSwipeLine.startWidth = 0.3f;
                powerSwipeLine.endWidth = 0.1f;
                powerSwipeLine.positionCount = 2;
                Debug.Log("[FlickShot] Power swipe line configured");
            }
            
            Debug.Log("[FlickShot] Power knob created (green)");
        }
        
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
        
        aimDirection = direction.normalized;
        aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        
        // Transition to AimSet phase - ready for power click
        currentPhase = FlickShotPhase.AimSet;
        
        Debug.Log($"[FlickShot] Aim position set - Rock: {rockPosition}, Launcher: {launcherPosition}, Direction: {aimDirection}, Angle: {aimAngle:F1}°, Distance: {pullbackDistance:F2}");
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
        
        // Show green power knob at launcher position
        if (powerKnobObj != null)
        {
            powerKnobObj.SetActive(true);
            powerKnobObj.transform.position = launcher.transform.position;
            Debug.Log("[FlickShot] Green power knob visible at launcher");
        }
        
        // Power drag starts at launcher Y position
        powerDragStartPos = new Vector2(launcher.transform.position.x, powerDragStartY);
        powerDragStartTime = Time.time;
        lastFeedbackTime = Time.time;
        
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
    /// Player drags mouse from Y=-25 (hack) toward Y=-16 (hog line)
    /// Drag TIME determines rock speed
    /// </summary>
    private void UpdatePowerPhase()
    {
        // Wait for mouse down to start dragging
        if (!isPowerDragging)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isPowerDragging = true;
                powerDragStartTime = Time.time;
                lastFeedbackTime = Time.time;
                Debug.Log("[FlickShot] Power drag started - swipe down!");
            }
            return; // Wait for drag to start
        }
        
        // Get current mouse position in world space
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // Update green power knob to follow mouse Y position
        Vector3 knobPos = launcher.transform.position;
        knobPos.y = Mathf.Clamp(mouseWorldPos.y, powerDragTargetY, powerDragStartY);
        
        if (powerKnobObj != null)
        {
            powerKnobObj.transform.position = knobPos;
        }
        
        // Draw swipe line from launcher to knob position
        if (powerSwipeLine != null)
        {
            powerSwipeLine.SetPosition(0, launcher.transform.position); // Start at launcher
            powerSwipeLine.SetPosition(1, knobPos); // End at knob
            
            // Color based on speed zone (like shooting knob)
            Color swipeColor = GetColorForDragPosition(knobPos.y);
            powerSwipeLine.startColor = swipeColor;
            powerSwipeLine.endColor = swipeColor;
        }
        
        // Track how far down the sheet the mouse has been dragged
        float currentY = Mathf.Clamp(mouseWorldPos.y, powerDragTargetY, powerDragStartY);
        float dragDistance = Mathf.Abs(currentY - powerDragStartY);
        float dragTime = Time.time - powerDragStartTime;
        
        // Provide feedback at intervals
        if (showSpeedFeedback && Time.time - lastFeedbackTime >= feedbackInterval)
        {
            CalculateSpeedBand(dragTime, dragDistance);
            
            // Show feedback at knob position
            if (powerKnobObj != null)
            {
                ShowSpeedFeedback(GetSpeedFeedbackMessage(), powerKnobObj.transform.position);
            }
            else
            {
                ShowSpeedFeedback(GetSpeedFeedbackMessage(), mouseWorldPos);
            }
            
            lastFeedbackTime = Time.time;
        }
        
        // Check for release (mouse up)
        if (Input.GetMouseButtonUp(0))
        {
            ReleaseFlickShot(dragTime, dragDistance);
        }
    }
    
    /// <summary>
    /// Get color for drag position (based on speed zones like shooting knob)
    /// </summary>
    private Color GetColorForDragPosition(float dragY)
    {
        // Calculate drag progress (0 = at launcher, 1 = at hog line)
        float dragProgress = Mathf.InverseLerp(powerDragStartY, powerDragTargetY, dragY);
        
        // Map to velocity bands
        // Green = perfect zone (middle)
        // Yellow = fast zone
        // Red = too fast zone
        
        if (dragProgress < 0.2f)
        {
            // Way too slow - dark green
            return new Color(0.1f, 0.5f, 0.1f);
        }
        else if (dragProgress < 0.4f)
        {
            // Too slow - green
            return new Color(0.2f, 0.8f, 0.2f);
        }
        else if (dragProgress < 0.6f)
        {
            // Perfect! - bright green
            return new Color(0.2f, 1f, 0.2f);
        }
        else if (dragProgress < 0.8f)
        {
            // Too fast - yellow
            return Color.yellow;
        }
        else
        {
            // Way too fast - red
            return new Color(1f, 0.3f, 0.3f);
        }
    }
    
    /// <summary>
    /// Calculate which speed band the drag falls into
    /// </summary>
    private void CalculateSpeedBand(float dragTime, float dragDistance)
    {
        // Normalize drag time to 0-1 range (faster drag = lower value)
        float normalizedTime = Mathf.Clamp01((dragTime - minDragTime) / (maxDragTime - minDragTime));
        normalizedTime = 1f - normalizedTime; // Invert so faster = higher
        
        // Apply forgiveness factor (compress toward center)
        normalizedTime = Mathf.Lerp(0.5f, normalizedTime, 1f / forgivenessFactor);
        
        // Calculate speed band (0 = slowest, speedBands-1 = fastest)
        speedBand = Mathf.FloorToInt(normalizedTime * speedBands);
        speedBand = Mathf.Clamp(speedBand, 0, speedBands - 1);
        
        // Calculate final speed multiplier
        calculatedSpeed = normalizedTime;
    }
    
    /// <summary>
    /// Get feedback message for current speed
    /// </summary>
    private string GetSpeedFeedbackMessage()
    {
        int perfectBand = speedBands / 2; // Middle band is "perfect"
        
        if (speedBand == perfectBand)
            return "Perfect!";
        else if (Mathf.Abs(speedBand - perfectBand) == 1)
            return speedBand < perfectBand ? "A bit slow..." : "A bit fast...";
        else if (speedBand < perfectBand)
            return speedBand == 0 ? "Way too slow!" : "Too slow!";
        else
            return speedBand == speedBands - 1 ? "Way too fast!" : "Too fast!";
    }
    
    /// <summary>
    /// Show speed feedback text callout
    /// </summary>
    private void ShowSpeedFeedback(string message, Vector2 position)
    {
        if (message == lastFeedbackMessage) return; // Don't spam same message
        
        lastFeedbackMessage = message;
        
        // Use TextCalloutManager if available
        ShowCallout(position, message, followTarget: false, duration: feedbackInterval * 2f);
        
        Debug.Log($"[FlickShot] Speed Feedback: {message} (Band: {speedBand}/{speedBands - 1})");
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
    /// </summary>
    private void ReleaseFlickShot(float dragTime, float dragDistance)
    {
        currentPhase = FlickShotPhase.Released;
        isPowerDragging = false; // Reset drag state
        
        // Hide power knob and swipe line
        if (powerKnobObj != null)
        {
            powerKnobObj.SetActive(false);
            Debug.Log("[FlickShot] Power knob hidden");
        }
        
        if (powerSwipeLine != null)
        {
            powerSwipeLine.enabled = false;
        }
        
        // Calculate final speed band
        CalculateSpeedBand(dragTime, dragDistance);
        
        Debug.Log($"[FlickShot] RELEASED - Time: {dragTime:F3}s, Distance: {dragDistance:F2}, Speed: {calculatedSpeed:F2}, Band: {speedBand}");
        
        // Calculate velocity based on aim direction and calculated speed
        // Map calculatedSpeed (0-1) to trajectory velocity range
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
        
        float targetSpeed = Mathf.Lerp(minVel, maxVel, calculatedSpeed);
        
        Vector2 finalVelocity = aimDirection * targetSpeed;
        
        Debug.Log($"[FlickShot] Final velocity: {finalVelocity.magnitude:F2} m/s at angle {aimAngle:F1}°");
        
        // Apply velocity to rock
        ApplyFlickShotVelocity(finalVelocity);
        
        // Show final feedback
        if (showSpeedFeedback)
        {
            string finalMessage = GetSpeedFeedbackMessage();
            ShowCallout(rb.position, finalMessage, followTarget: true, duration: 2f);
        }
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
        
        // Position rock at synthetic pullback distance in aimed direction
        Vector2 launcherPos = launcher.transform.position;
        Vector2 pullbackPos = launcherPos + aimDirection * syntheticPullback;
        rb.position = pullbackPos;
        
        Debug.Log($"[FlickShot] Rock positioned at synthetic pullback: {pullbackPos} (launcher + aim direction * {syntheticPullback:F2})");
        
        // Get Rock_Flick component to trigger normal release
        Component rockFlickComp = GetComponent("Rock_Flick");
        if (rockFlickComp != null)
        {
            MonoBehaviour rockFlick = (MonoBehaviour)rockFlickComp;
            
            // CRITICAL: Ensure Rock_Flick is enabled before triggering release
            rockFlick.enabled = true;
            
            // Set the rock as "released" from user input perspective (mouseUp = true)
            System.Reflection.FieldInfo mouseUpField = rockFlick.GetType().GetField("mouseUp");
            if (mouseUpField != null)
            {
                mouseUpField.SetValue(rockFlick, true);
                Debug.Log($"[FlickShot] Triggering normal Rock_Flick.Release() - will calculate velocity from synthetic pullback {syntheticPullback:F2}");
            }
            
            // CRITICAL: Make sure isPressed is false so Update() sees mouseUp
            System.Reflection.FieldInfo isPressedField = rockFlick.GetType().GetField("isPressed");
            if (isPressedField != null)
            {
                isPressedField.SetValue(rockFlick, false);
            }
        }
        else
        {
            Debug.LogError("[FlickShot] Could not find Rock_Flick component to trigger release!");
        }
        
        // CRITICAL: Set shotTaken = true so GameManager knows the shot is happening
        if (rockInfo != null)
        {
            System.Reflection.PropertyInfo shotTakenProp = rockInfo.GetType().GetProperty("shotTaken");
            if (shotTakenProp != null)
            {
                shotTakenProp.SetValue(rockInfo, true);
                Debug.Log("[FlickShot] Set Rock_Info.shotTaken = true");
            }
            else
            {
                // Try as field if not a property
                System.Reflection.FieldInfo shotTakenField = rockInfo.GetType().GetField("shotTaken");
                if (shotTakenField != null)
                {
                    shotTakenField.SetValue(rockInfo, true);
                    Debug.Log("[FlickShot] Set Rock_Info.shotTaken = true (field)");
                }
            }
        }
    }
    
    /// <summary>
    /// Cancel flick shot (e.g., if player clicks away)
    /// </summary>
    public void CancelFlickShot()
    {
        currentPhase = FlickShotPhase.Inactive;
        
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
}
