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
        
        // Create swipe trail line renderer (BLACK line that draws as player swipes)
        GameObject swipeTrailObj = new GameObject("SwipeTrail");
        swipeTrailLine = swipeTrailObj.AddComponent<LineRenderer>();
        swipeTrailLine.enabled = false;
        swipeTrailLine.startWidth = 0.05f; // 75% thinner (was 0.2f)
        swipeTrailLine.endWidth = 0.05f;
        swipeTrailLine.positionCount = 0;
        swipeTrailLine.startColor = Color.black;
        swipeTrailLine.endColor = Color.black;
        swipeTrailLine.material = new Material(Shader.Find("Sprites/Default"));
        Debug.Log("[FlickShot] Swipe trail line created (black, thin)");
        
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
        
        // Transition to AimSet phase - ready for power click
        currentPhase = FlickShotPhase.AimSet;
        
        Debug.Log($"[FlickShot] Aim position set - Rock: {rockPosition}, Launcher: {launcherPosition}, Pullback: {direction}, Aim Direction (FLIPPED): {aimDirection}, Angle: {aimAngle:F1}°, Distance: {pullbackDistance:F2}");
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
    /// Player swipes and we draw a trail, then show feedback AFTER release
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
                swipePoints.Clear();
                
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
        
        // Add cursor position to trail (sample every few frames for smooth line)
        if (swipePoints.Count == 0 || Vector3.Distance(swipePoints[swipePoints.Count - 1], mousePos3D) > 0.2f)
        {
            swipePoints.Add(mousePos3D);
            
            // Update line renderer with all points
            if (swipeTrailLine != null)
            {
                swipeTrailLine.positionCount = swipePoints.Count;
                swipeTrailLine.SetPositions(swipePoints.ToArray());
            }
        }
        
        // Check for release (mouse up)
        if (Input.GetMouseButtonUp(0))
        {
            float dragTime = Time.time - powerDragStartTime;
            float currentY = Mathf.Clamp(mouseWorldPos.y, powerDragTargetY, powerDragStartY);
            float dragDistance = Mathf.Abs(currentY - powerDragStartY);
            
            ReleaseFlickShot(dragTime, dragDistance);
        }
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
    /// Uses physics simulation to estimate where rock will stop
    /// </summary>
    private float CalculatePredictedStopPosition(float initialVelocity)
    {
        // Simple physics estimate: rock decelerates from hog line to house
        // Based on empirical data from logs:
        // - Rock loses ~17-18% velocity per 0.5s after hog line
        // - Damping factor ? 0.9 (90% retained per frame at 50 FPS)
        
        float hogLineY = -16f;
        float distanceFromHogToHouse = 22.5f; // From Y=-16 to Y=6.5
        
        // Estimate travel distance using energy/friction model
        // v^2 = u^2 + 2as, where a = -friction
        // Approximation: distance ? velocity^2 / (2 * friction)
        
        float frictionFactor = 1.8f; // Empirical from logs
        float estimatedDistance = (initialVelocity * initialVelocity) / (2f * frictionFactor);
        
        float predictedStopY = hogLineY + estimatedDistance;
        
        // Clamp to reasonable range (-16 to 15)
        predictedStopY = Mathf.Clamp(predictedStopY, -16f, 15f);
        
        return predictedStopY;
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
        
        // Calculate final speed
        CalculateSpeedBand(dragTime, dragDistance);
        float targetSpeed = GetPredictedVelocity();
        float predictedStopY = CalculatePredictedStopPosition(targetSpeed);
        
        Debug.Log($"[FlickShot] RELEASED - Time: {dragTime:F3}s, Speed: {calculatedSpeed:F2}, Band: {speedBand}");
        
        // Show predicted stop line (CYAN horizontal line at predicted Y)
        if (predictedStopLine != null)
        {
            float lineWidth = 3f;
            Vector3 leftPoint = new Vector3(-lineWidth, predictedStopY, -1f);
            Vector3 rightPoint = new Vector3(lineWidth, predictedStopY, -1f);
            
            predictedStopLine.SetPosition(0, leftPoint);
            predictedStopLine.SetPosition(1, rightPoint);
            predictedStopLine.enabled = true;
            
            Debug.Log($"[FlickShot] Predicted stop line shown at Y={predictedStopY:F1}");
        }
        
        // Show speed callout at shooter position
        if (showSpeedFeedback)
        {
            string speedMessage = GetSpeedFeedbackMessage();
            Vector2 shooterPos = new Vector2(0f, -25f);
            ShowCallout(shooterPos, speedMessage + $" ({targetSpeed:F1} m/s)", followTarget: false, duration: 3f);
            Debug.Log($"[FlickShot] Speed callout: {speedMessage} ({targetSpeed:F1} m/s)");
        }
        
        // Calculate and apply velocity
        Vector2 finalVelocity = aimDirection * targetSpeed;
        Debug.Log($"[FlickShot] Final velocity: {finalVelocity.magnitude:F2} m/s at angle {aimAngle:F1}°");
        
        ApplyFlickShotVelocity(finalVelocity);
        
        // Hide swipe trail after 1 second
        StartCoroutine(HideSwipeTrailAfterDelay(1f));
        
        // Hide predicted line after 3 seconds
        StartCoroutine(HidePredictedLineAfterDelay(3f));
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
    /// Hide predicted stop line after delay
    /// </summary>
    private IEnumerator HidePredictedLineAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (predictedStopLine != null)
        {
            predictedStopLine.enabled = false;
            Debug.Log("[FlickShot] Predicted stop line hidden");
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
