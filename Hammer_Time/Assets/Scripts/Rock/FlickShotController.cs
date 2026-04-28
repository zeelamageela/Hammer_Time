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
    
    [Tooltip("Line renderer for input zone border (shows valid drag area)")]
    private LineRenderer inputZoneBorder;
    
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
    
    [Tooltip("Target position for power drag (Y coordinate, AT hog line)")]
    public float powerDragTargetY = -16f; // Exactly at hog line
    
    [Header("Input Zone Validation")]
    [Tooltip("Maximum X distance from center (�X units) for valid input")]
    [Range(0.5f, 2.0f)]
    public float inputZoneMaxX = 1.0f;
    
    [Tooltip("Y buffer below launcher (how far below launcher to allow clicks)")]
    [Range(0.0f, 2.0f)]
    public float inputZoneBufferY = 0.5f;
    
    [Tooltip("Y buffer above hog line - SET TO 0 to end exactly at hog line (-16f)")]
    [Range(0.0f, 2.0f)]
    public float inputZoneBufferAboveHog = 0.0f; // ? Changed from 0.5f to 0.0f - zone ends at Y=-16
    
    [Header("Visual Feedback")]
    [Tooltip("Minimum distance cursor must move before adding to line (prevents tiny segments)")]
    [Range(0.05f, 0.5f)]
    public float minDrawDistance = 0.20f;
    
    [Header("Velocity Calculation (Distance/Time)")]
    [Tooltip("DEPRECATED - Now using dynamic velocity window based on target")]
    [Range(1.0f, 10.0f)]
    public float minDragVelocity = 5.0f; // DEPRECATED - kept for backward compatibility
    
    [Tooltip("DEPRECATED - Now using dynamic velocity window based on target")]
    [Range(10.0f, 50.0f)]
    public float maxDragVelocity = 16.0f; // DEPRECATED - kept for backward compatibility
    
    [Tooltip("Velocity scale multiplier - adjusts how drag velocity maps to rock velocity (1.0 = 1:1 mapping)")]
    [Range(0.5f, 2.0f)]
    public float velocityScaleMultiplier = 1.0f; // ? NEW: Fine-tune feel (1.0 = natural)
    
    [Header("Dynamic Velocity Window (NEW!)")]
    [Tooltip("BASE velocity tolerance around target (�X m/s) - modified by skill scaling")]
    [Range(0.5f, 3.0f)]
    public float velocityTolerance = 1.5f; // �1.5 m/s base window
    
    [Tooltip("Absolute minimum rock velocity (safety clamp)")]
    [Range(3.0f, 7.0f)]
    public float absoluteMinVelocity = 5.0f; // Never go below this
    
    [Tooltip("Absolute maximum rock velocity (safety clamp)")]
    [Range(12.0f, 18.0f)]
    public float absoluteMaxVelocity = 16.0f; // Never go above this
    
    [Header("Skill-Based Tolerance Scaling")]
    [Tooltip("Enable skill-based tolerance (uses weight skill from CharacterStats)")]
    public bool useSkillScaling = true;

    [Tooltip("Tolerance multiplier at 0% weight skill (unskilled = WIDE tolerance, forgiving!)")]
    [Range(1.0f, 2.0f)]
    public float lowSkillScale = 1.4f; // Beginners get 140% tolerance (easier!)

    [Tooltip("Tolerance multiplier at 100% weight skill (skilled = TIGHT tolerance, precise!)")]
    [Range(0.5f, 1.0f)]
    public float highSkillScale = 0.7f; // Experts get 70% tolerance (harder!)
    
    [Header("(DEPRECATED - No longer using speed bands)")]
    [Tooltip("DEPRECATED: Speed bands removed - now using continuous velocity")]
    public int speedBands = 7; // DEPRECATED - kept for backward compatibility
    
    [Tooltip("DEPRECATED: No longer using tolerance bands")]
    public float perfectTolerance = 0.15f; // DEPRECATED
    
    [Header("Skill Tuning")]
    [Tooltip("Forgiveness factor - higher = more forgiving (easier to hit speed bands)")]
    [Range(0.5f, 2.0f)]
    public float forgivenessFactor = 1.7f;
    
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
    private float calculatedSpeed; // 0-1 normalized speed (continuous, not banded)
    private float dragVelocity; // Actual drag velocity (distance/time) in units/second
    
    // Dynamic velocity window (calculated per shot)
    private float dynamicMinVelocity; // Target - tolerance
    private float dynamicMaxVelocity; // Target + tolerance
    private float targetRockVelocity; // The velocity we're aiming for
    
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
        
        // Create input zone border (green rectangle showing valid drag area)
        GameObject inputZoneObj = new GameObject("InputZoneBorder");
        inputZoneBorder = inputZoneObj.AddComponent<LineRenderer>();
        inputZoneBorder.enabled = false;
        inputZoneBorder.startWidth = 0.05f;
        inputZoneBorder.endWidth = 0.05f;
        inputZoneBorder.positionCount = 5; // Rectangle (4 corners + close loop)
        Color zoneColor = new Color(0f, 1f, 0f, 0.3f); // Green, transparent
        inputZoneBorder.startColor = zoneColor;
        inputZoneBorder.endColor = zoneColor;
        inputZoneBorder.material = new Material(Shader.Find("Sprites/Default"));
        inputZoneBorder.useWorldSpace = true;
        Debug.Log("[FlickShot] Input zone border created (green rectangle)");
        
        // Create velocity guide indicator
        GameObject velocityGuideObj = new GameObject("VelocityGuide");
        velocityGuide = velocityGuideObj.AddComponent<VelocityGuideIndicator>();
        velocityGuide.startY = powerDragStartY;  // Launcher position (-25f)
        velocityGuide.endY = powerDragTargetY;   // ? NEW: Exactly at hog line (-16f, not -16.5f)
        velocityGuide.pauseDuration = 1.5f; // Pause at hogline (1.5s total)
        velocityGuide.fadeOutDuration = 0.5f; // Fade out over last 0.5s
        velocityGuide.lineWidth = 0.2f;
        Debug.Log($"[FlickShot] Velocity guide created: {velocityGuide.startY:F1} to {velocityGuide.endY:F1} (swipe zone matches exactly!)");
        
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
        
        Debug.Log($"[FlickShot] Aim position set - Rock: {rockPosition}, Launcher: {launcherPosition}, Pullback: {direction}, Aim Direction (FLIPPED): {aimDirection}, Angle: {aimAngle:F1}�, Distance: {pullbackDistance:F2}");
        Debug.Log($"[FlickShot] Stored pullback position: {storedPullbackPosition}");
        Debug.Log($"[FlickShot] Aim locked! Click launcher to start power, or click rock to re-aim.");
    }
    
    /// <summary>
    /// Reset back to aiming phase - allows player to adjust aim after setting it
    /// Called when player clicks rock in AimSet phase
    /// </summary>
    public void ResetToAiming()
    {
        currentPhase = FlickShotPhase.AimingPhase;
        
        // Re-enable rock collider so it can be clicked/dragged
        CircleCollider2D rockCollider = GetComponent<CircleCollider2D>();
        if (rockCollider != null)
        {
            rockCollider.enabled = true;
        }
        
        Debug.Log("[FlickShot] Reset to AimingPhase - player can now adjust aim");
    }

    /// <summary>
    /// Transition from aiming to power phase
    /// Triggered when player clicks on launcher
    /// </summary>
    [System.Obsolete]
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
        
        // Disable rock collider so it can't be clicked during power phase
        CircleCollider2D rockCollider = GetComponent<CircleCollider2D>();
        if (rockCollider != null)
        {
            rockCollider.enabled = false;
            Debug.Log("[FlickShot] Rock collider disabled - can't click rock during power phase");
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
        
        // Setup camera for power phase (frame drag zone from launcher to hog line)
        if (cameraManager != null)
        {
            System.Type camType = cameraManager.GetType();
            System.Reflection.MethodInfo powerPhaseSetupMethod = camType.GetMethod("PowerPhaseSetup");
            if (powerPhaseSetupMethod != null)
            {
                powerPhaseSetupMethod.Invoke(cameraManager, null);
                Debug.Log("[FlickShot] Power phase camera setup called");
            }
            else
            {
                Debug.LogWarning("[FlickShot] PowerPhaseSetup method not found on CameraManager");
            }
        }
        
        // Enable swipe trail line (will draw as player swipes)
        if (swipeTrailLine != null)
        {
            swipeTrailLine.enabled = true;
            swipePoints.Clear();
            Debug.Log("[FlickShot] Swipe trail enabled - ready to draw");
        }
        
        // ? NEW: Show input zone border ONLY in flick shot mode during power phase
        // Configured in SetupInputZoneBorder() - will be visible when player can actually swipe
        SetupInputZoneBorder();
        
        // Power drag starts at launcher Y position
        powerDragStartPos = new Vector2(launcher.transform.position.x, powerDragStartY);
        powerDragStartTime = Time.time;
        lastFeedbackTime = Time.time;
        
        // Initialize speed slider
        InitializeSpeedSlider();
        
        // Start velocity guide indicator
        if (velocityGuide != null)
        {
            // ? NEW: Calculate target velocity and dynamic window!
            targetRockVelocity = GetTargetVelocityFromTrajectory();
            
            if (targetRockVelocity <= 0f)
            {
                Debug.LogWarning("[FlickShot] Failed to get target velocity from trajectory - using fallback");
                targetRockVelocity = 10f; // Fallback to medium velocity
            }
            
            // ? SKILL-BASED TOLERANCE: Scale tolerance based on weight skill!
            float scaledTolerance = CalculateSkillScaledTolerance();
            
            // ? DYNAMIC VELOCITY WINDOW: Calculate min/max around target
            dynamicMinVelocity = targetRockVelocity - scaledTolerance;
            dynamicMaxVelocity = targetRockVelocity + scaledTolerance;
            
            // Safety clamp to absolute limits (prevent going outside physics range)
            dynamicMinVelocity = Mathf.Max(dynamicMinVelocity, absoluteMinVelocity);
            dynamicMaxVelocity = Mathf.Min(dynamicMaxVelocity, absoluteMaxVelocity);
            
            Debug.Log($"[FlickShot] ? DYNAMIC VELOCITY WINDOW (SKILL-SCALED):");
            Debug.Log($"  Target velocity: {targetRockVelocity:F2} m/s");
            Debug.Log($"  Base tolerance: �{velocityTolerance:F2} m/s");
            Debug.Log($"  Scaled tolerance: �{scaledTolerance:F2} m/s (skill-adjusted!)");
            Debug.Log($"  Min velocity: {dynamicMinVelocity:F2} m/s (target - scaled tolerance)");
            Debug.Log($"  Max velocity: {dynamicMaxVelocity:F2} m/s (target + scaled tolerance)");
            Debug.Log($"  Window size: {dynamicMaxVelocity - dynamicMinVelocity:F2} m/s");
            Debug.Log($"  Absolute limits: {absoluteMinVelocity:F2} - {absoluteMaxVelocity:F2} m/s");
            
            // ? CRITICAL: Map rock velocity ? ideal drag velocity
            // We need to show how fast to SWIPE, not how fast rock travels!
            float idealDragVelocity = CalculateIdealDragVelocityForRockSpeed(targetRockVelocity);
            
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
            
            // ? Pass DRAG VELOCITY to guide (how fast to swipe!)
            velocityGuide.StartGuide(idealDragVelocity, guideColor);
            Debug.Log($"[FlickShot] ? Velocity guide started:");
            Debug.Log($"  Rock velocity: {targetRockVelocity:F2} m/s (how fast rock travels)");
            Debug.Log($"  Drag velocity: {idealDragVelocity:F1} units/s (how fast YOU swipe!)");
            Debug.Log($"  Color: {guideColor}");
            
            // Show velocity callout at launcher using TextCalloutManager directly
            if (showSpeedFeedback && TextCalloutManager.Instance != null)
            {
                // Calculate ideal drag time based on drag velocity
                float distance = velocityGuide.endY - velocityGuide.startY; // -16.5 - (-24.66) = 8.16 units
                float idealTime = distance / idealDragVelocity; // Time guide takes to animate
                
                Vector2 launcherPos = launcher.transform.position;
                string velocityMessage = $"Rock: {targetRockVelocity:F1} m/s";
                string swipeMessage = $"Swipe: {idealDragVelocity:F0} units/s";
                string timeMessage = $"Time: {idealTime:F2}s";
                
                // Show velocity and time as stacked callouts
                TextCalloutManager.Instance.ShowCallout(launcherPos, velocityMessage, followTarget: false, target: null, duration: 5f);
                TextCalloutManager.Instance.ShowCallout(launcherPos, swipeMessage, followTarget: false, target: null, duration: 5f);
                TextCalloutManager.Instance.ShowCallout(launcherPos, timeMessage, followTarget: false, target: null, duration: 5f);
                
                Debug.Log($"[FlickShot] Velocity callouts shown: {velocityMessage} | {swipeMessage} | {timeMessage}");
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
        Debug.Log($"[FlickShot] Using aim direction: angle={aimAngle:F1}�, direction={aimDirection}");
        
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
        
        // Wait for mouse down to start dragging (with input zone validation!)
        if (!isPowerDragging)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                
                // ? VALIDATE: Click must be in valid input zone!
                if (!IsInValidInputZone(clickPos))
                {
                    Debug.LogWarning($"[FlickShot] Click OUTSIDE valid zone ({clickPos.x:F2}, {clickPos.y:F2}) - IGNORED!");
                    if (TextCalloutManager.Instance != null)
                    {
                        TextCalloutManager.Instance.ShowCallout(clickPos, "Click in green zone!", false, null, 2f);
                    }
                    return; // Block rogue clicks!
                }
                
                isPowerDragging = true;
                powerDragStartTime = Time.time;
                swipePoints.Clear();
                
                // KEEP velocity guide running during drag (don't stop it!)
                // This allows player to time their swipe to the animation
                Debug.Log("[FlickShot] Power swipe started - velocity guide continues animating");
                
                // Add starting point at click position (not launcher!)
                Vector3 startPos = new Vector3(clickPos.x, clickPos.y, -1f);
                swipePoints.Add(startPos);
                
                Debug.Log($"[FlickShot] Power swipe started at {clickPos} - draw your path!");
            }
            return;
        }
        
        // Get current mouse position in world space
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mousePos3D = new Vector3(mouseWorldPos.x, mouseWorldPos.y, -1f);
        
        // ? NEW: Calculate velocity from DISTANCE/TIME (actual velocity!)
        float currentDragTime = Time.time - powerDragStartTime;
        if (currentDragTime > 0.01f && swipePoints.Count > 0) // Avoid divide by zero
        {
            // Calculate DISTANCE traveled (Y-axis)
            Vector3 startPos = swipePoints[0]; // First point
            float distanceTraveled = Mathf.Abs(mouseWorldPos.y - startPos.y);
            
            // Calculate VELOCITY = distance / time (units/second)
            dragVelocity = distanceTraveled / currentDragTime;
            
            // Normalize and calculate speed (0-1)
            calculatedSpeed = CalculateSpeedFromVelocity(dragVelocity);
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
        
        // ? NEW: Add cursor position with minimum draw distance (cleaner lines!)
        // Only add point if moved far enough from LAST point (prevents tiny segments)
        if (swipePoints.Count == 0 || Vector3.Distance(swipePoints[swipePoints.Count - 1], mousePos3D) > minDrawDistance)
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
            
            // ? NEW: Calculate final distance traveled (Y-axis)
            Vector3 startPos = swipePoints.Count > 0 ? swipePoints[0] : launcher.transform.position;
            float dragDistance = Mathf.Abs(mouseWorldPos.y - startPos.y);
            
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
    /// Get predicted velocity based on current drag speed
    /// ? NEW: Uses DYNAMIC velocity window (centered on target!)
    /// </summary>
    private float GetPredictedVelocity()
    {
        // ? Map calculatedSpeed (0-1) to DYNAMIC velocity range!
        // calculatedSpeed = 0.0 ? dynamicMinVelocity (target - tolerance)
        // calculatedSpeed = 0.5 ? targetRockVelocity (PERFECT!)
        // calculatedSpeed = 1.0 ? dynamicMaxVelocity (target + tolerance)
        
        float predictedVel = Mathf.Lerp(dynamicMinVelocity, dynamicMaxVelocity, calculatedSpeed);
        
        // Safety clamp (should already be within range, but just in case)
        predictedVel = Mathf.Clamp(predictedVel, absoluteMinVelocity, absoluteMaxVelocity);
        
        return predictedVel;
    }
    
    /// <summary>
    /// Calculate predicted stop position based on initial velocity
    /// Uses TrajectorySimulator to get accurate prediction matching real physics
    /// </summary>
    private float CalculatePredictedStopPosition(float initialVelocity)
    {
        Debug.Log($"[FlickShot Prediction] ======== PREDICTION START ========");
        Debug.Log($"[FlickShot Prediction] Initial velocity: {initialVelocity:F2} m/s");
        Debug.Log($"[FlickShot Prediction] Aim direction: {aimDirection}, angle: {aimAngle:F1}�");
        
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
                Debug.Log($"  Velocity: {initialVelocity:F1} m/s, Direction: {aimDirection}, Angle: {aimAngle:F1}�");
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
    /// ? REFACTORED: Calculate speed multiplier (0-1) from drag VELOCITY (distance/time)
    /// Now maps to DYNAMIC min/max drag velocities (centered on target!)
    /// </summary>
    private float CalculateSpeedFromVelocity(float dragVelocity)
    {
        // ? Calculate dynamic drag velocity range from rock velocity range
        // We need to map: rock velocity range ? drag velocity range
        
        // Get ideal drag velocities for min/max rock velocities
        float minDragVel = CalculateIdealDragVelocityForRockSpeed(dynamicMinVelocity);
        float maxDragVel = CalculateIdealDragVelocityForRockSpeed(dynamicMaxVelocity);
        
        // Map player's drag velocity to normalized speed (0-1)
        // dragVelocity = minDragVel ? normalizedSpeed = 0.0 (slowest)
        // dragVelocity = midDragVel ? normalizedSpeed = 0.5 (PERFECT!)
        // dragVelocity = maxDragVel ? normalizedSpeed = 1.0 (fastest)
        float normalizedSpeed = Mathf.InverseLerp(minDragVel, maxDragVel, dragVelocity);
        
        // Apply forgiveness factor (smooth extremes toward middle)
        normalizedSpeed = Mathf.Lerp(0.5f, normalizedSpeed, 1f / forgivenessFactor);
        
        return Mathf.Clamp01(normalizedSpeed);
    }
    
    /// <summary>
    /// DEPRECATED: Old time-based calculation (kept for reference)
    /// Now using velocity-based calculation instead!
    /// </summary>
    private float CalculateSpeedFromDragTime(float dragTime)
    {
        Debug.LogWarning("[FlickShot] DEPRECATED: Using old time-based calculation - should use velocity-based!");
        
        float normalizedSpeed;
        
        if (dragTime <= 0.1f)
        {
            normalizedSpeed = 1.0f;
        }
        else if (dragTime >= 1.5f)
        {
            normalizedSpeed = 0.0f;
        }
        else
        {
            normalizedSpeed = 1.0f - ((dragTime - 0.1f) / (1.5f - 0.1f));
        }
        
        normalizedSpeed = Mathf.Lerp(0.5f, normalizedSpeed, 1f / forgivenessFactor);
        
        return Mathf.Clamp01(normalizedSpeed);
    }
    
    /// <summary>
    /// ? REFACTORED: Calculate final velocity from drag velocity (distance/time)
    /// NO MORE SPEED BANDS - continuous velocity for realism!
    /// </summary>
    private void CalculateFinalVelocity(float dragTime, float dragDistance)
    {
        // Calculate drag velocity (distance / time)
        if (dragTime > 0.01f)
        {
            dragVelocity = dragDistance / dragTime;
        }
        else
        {
            dragVelocity = maxDragVelocity; // Ultra-fast = max
        }
        
        // Calculate normalized speed (0-1) from velocity
        calculatedSpeed = CalculateSpeedFromVelocity(dragVelocity);
        
        Debug.Log($"[FlickShot] Drag: {dragDistance:F2}m in {dragTime:F2}s = {dragVelocity:F1} units/s ? speed {calculatedSpeed:F3} (continuous, no bands!)");
    }

    /// <summary>
    /// Initialize speed slider for power phase
    /// Auto-detects slider and components in scene
    /// </summary>
    [System.Obsolete]
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
    /// ? NEW: Calculate ideal drag time based on target velocity
    /// Uses velocity-based calculation (distance/time)
    /// </summary>
    private float CalculateIdealDragTime()
    {
        // Get distance we'll be dragging (launcher to hog line)
        float distance = Mathf.Abs(powerDragTargetY - powerDragStartY); // ~9 units
        
        // Calculate ideal velocity for "perfect" shot (middle speed = 0.5)
        float perfectNormalized = 0.5f;
        float idealVelocity = Mathf.Lerp(minDragVelocity, maxDragVelocity, perfectNormalized);
        
        // Calculate time needed: time = distance / velocity
        float idealTime = distance / idealVelocity;
        
        Debug.Log($"[FlickShot] Ideal drag: {distance:F1}m at {idealVelocity:F1} units/s = {idealTime:F2}s");
        
        return idealTime;
    }
    
    /// <summary>
    /// ? NEW: Calculate ideal DRAG velocity (units/s input speed) for a target rock velocity
    /// This maps rock speed (m/s) ? input speed (units/s) using the inverse formula
    /// </summary>
    private float CalculateIdealDragVelocityForRockSpeed(float targetRockVelocity)
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
        
        // Inverse mapping: rock velocity ? normalized speed (0-1)
        float normalizedSpeed = Mathf.InverseLerp(minVel, maxVel, targetRockVelocity);
        
        // Reverse forgiveness factor
        // calculatedSpeed = Lerp(0.5, normalizedSpeed, 1/forgiveness)
        // Solve for normalizedSpeed before forgiveness:
        // normalizedSpeed = 0.5 + (calculatedSpeed - 0.5) * forgiveness
        float rawNormalized = 0.5f + (normalizedSpeed - 0.5f) * forgivenessFactor;
        rawNormalized = Mathf.Clamp01(rawNormalized);
        
        // Map normalized speed ? drag velocity
        float idealDragVel = Mathf.Lerp(minDragVelocity, maxDragVelocity, rawNormalized);
        
        // ? NEW: Apply velocity scaling for more natural feel
        // velocityScaleMultiplier = 1.0 ? drag velocity ? rock velocity (intuitive!)
        // velocityScaleMultiplier < 1.0 ? slower swipes (easier)
        // velocityScaleMultiplier > 1.0 ? faster swipes (harder)
        idealDragVel *= velocityScaleMultiplier;
        
        Debug.Log($"[FlickShot] Drag velocity calculation:");
        Debug.Log($"  Target rock velocity: {targetRockVelocity:F2} m/s");
        Debug.Log($"  Normalized speed: {normalizedSpeed:F3}");
        Debug.Log($"  Raw normalized (pre-forgiveness): {rawNormalized:F3}");
        Debug.Log($"  Base drag velocity: {idealDragVel / velocityScaleMultiplier:F1} units/s");
        Debug.Log($"  Scaled drag velocity: {idealDragVel:F1} units/s (�{velocityScaleMultiplier:F2})");
        Debug.Log($"  Drag velocity range: {minDragVelocity * velocityScaleMultiplier:F1} - {maxDragVelocity * velocityScaleMultiplier:F1} units/s");
        
        return idealDragVel;
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
    /// ? REFACTORED: Get feedback message based on CONTINUOUS speed (no bands!)
    /// Now uses actual velocity deviation from target (more intuitive!)
    /// </summary>
    private string GetSpeedFeedbackMessage()
    {
        // Get actual velocity from current speed
        float actualVelocity = GetPredictedVelocity();
        
        // Calculate deviation from target in m/s
        float deviationVelocity = actualVelocity - targetRockVelocity;
        float deviationPercent = Mathf.Abs(deviationVelocity / targetRockVelocity) * 100f;
        
        // Give feedback based on velocity deviation
        if (deviationPercent < 5f)
            return "Perfect!"; // Within 5% of target
        else if (deviationPercent < 15f)
        {
            return deviationVelocity < 0 ? "Slightly Slow" : "Slightly Fast";
        }
        else if (deviationPercent < 30f)
        {
            return deviationVelocity < 0 ? "Too Slow" : "Too Fast";
        }
        else
        {
            return deviationVelocity < 0 ? "Way Too Slow!" : "Way Too Fast!";
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
        
        // ? NEW: Calculate final velocity (distance/time, no bands!)
        CalculateFinalVelocity(dragTime, dragDistance);
        float targetSpeed = GetPredictedVelocity();
        float predictedStopY = CalculatePredictedStopPosition(targetSpeed);
        
        Debug.Log($"[FlickShot] RELEASED - Time: {dragTime:F3}s, Distance: {dragDistance:F2}m, Velocity: {dragVelocity:F1} units/s, Speed: {calculatedSpeed:F3} (continuous!)");
        Debug.Log($"[FlickShot] *** CYAN LINE PREDICTION: Y = {predictedStopY:F2} ***");
        Debug.Log($"[FlickShot] *** TARGET VELOCITY: {targetSpeed:F2} m/s ***");
        Debug.Log($"[FlickShot] *** AIM DIRECTION: {aimDirection}, ANGLE: {aimAngle:F1}� ***");
        
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
            // STACKED CALLOUTS - each piece of info gets its own callout!
            if (TextCalloutManager.Instance != null)
            {
                Vector3 rockPosition = rb != null ? (Vector3)rb.position : transform.position;
                
                // Callout 1: Speed feedback message (Perfect!/Too Fast/etc.)
                string speedMessage = GetSpeedFeedbackMessage();
                TextCalloutManager.Instance.ShowRockCallout(gameObject, speedMessage);
                
                // Callout 2: Actual velocity (what we got)
                TextCalloutManager.Instance.ShowRockCallout(gameObject, $"{targetSpeed:F2} m/s");
                
                Debug.Log($"[FlickShot] *** SPEED CALLOUTS: {speedMessage} | {targetSpeed:F2} m/s ***");
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
        Debug.Log($"[FlickShot] *** ACTUAL VELOCITY APPLIED: {finalVelocity.magnitude:F2} m/s at angle {aimAngle:F1}� ***");
        
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
    [System.Obsolete]
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
        
        Debug.Log($"[FlickShot] Rock launched directly with velocity: {velocity.magnitude:F2} m/s at angle {aimAngle:F1}�");
        
        // Start coroutine to set released = true after rock starts moving
        StartCoroutine(SetReleasedAfterMoving());
    }

    /// <summary>
    /// Wait for rock to actually start moving, then set released = true
    /// </summary>
    [System.Obsolete]
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
        
        if (inputZoneBorder != null)
        {
            inputZoneBorder.enabled = false;
            Debug.Log("[FlickShot] Input zone border hidden (OnDisable)");
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
    
    /// <summary>
    /// ? NEW: Check if mouse position is in valid input zone
    /// Prevents rogue clicks outside intended drag area
    /// </summary>
    private bool IsInValidInputZone(Vector2 mousePos)
    {
        float minY = powerDragStartY - inputZoneBufferY;  // -25.5f (below launcher)
        float maxY = powerDragTargetY + inputZoneBufferAboveHog; // -15.5f (above hog line)
        float maxX = inputZoneMaxX; // �1.0 unit from center
        
        bool inZone = mousePos.y >= minY && 
                      mousePos.y <= maxY && 
                      Mathf.Abs(mousePos.x) <= maxX;
        
        if (!inZone)
        {
            Debug.Log($"[FlickShot] Position ({mousePos.x:F2}, {mousePos.y:F2}) OUTSIDE zone: X must be �{maxX}, Y must be {minY:F1} to {maxY:F1}");
        }
        
        return inZone;
    }
    
    /// <summary>
    /// ? NEW: Calculate skill-scaled tolerance based on weight accuracy
    /// INVERTED: Low skill = WIDE tolerance (easier), High skill = TIGHT tolerance (harder)
    /// </summary>
    private float CalculateSkillScaledTolerance()
    {
        if (!useSkillScaling)
        {
            Debug.Log($"[FlickShot Skill] Skill scaling DISABLED - using base tolerance: �{velocityTolerance:F2} m/s");
            return velocityTolerance; // Use base value
        }
        
        float weightSkill = GetPlayerWeightSkill(); // 0-100
        float normalizedSkill = weightSkill / 100f; // 0-1
        
        // INVERTED SCALING: Low skill (0) ? lowSkillScale (1.4x), High skill (1) ? highSkillScale (0.7x)
        // This makes beginners have WIDER tolerance (easier), experts have TIGHTER tolerance (harder)
        float scalingFactor = Mathf.Lerp(lowSkillScale, highSkillScale, normalizedSkill);
        
        // Apply to base tolerance
        float scaledTolerance = velocityTolerance * scalingFactor;
        
        Debug.Log($"[FlickShot Skill] === SKILL-BASED TOLERANCE ===");
        Debug.Log($"  Weight skill: {weightSkill:F1}% (normalized: {normalizedSkill:F3})");
        Debug.Log($"  Scaling factor: {scalingFactor:F2}x (INVERTED: low skill = wider)");
        Debug.Log($"  Base tolerance: �{velocityTolerance:F2} m/s");
        Debug.Log($"  Scaled tolerance: �{scaledTolerance:F2} m/s");
        Debug.Log($"  Skill range: {lowSkillScale:F2}x (0%) to {highSkillScale:F2}x (100%)");
        
        return scaledTolerance;
    }

    /// <summary>
    /// Get player's weight accuracy skill from CharacterStats
    /// Returns value 0-100
    /// </summary>
    [System.Obsolete]
    private float GetPlayerWeightSkill()
    {
        // Check if this is an AI shot or player shot
        if (gm != null)
        {
            System.Type gmType = gm.GetType();
            System.Reflection.FieldInfo aiTeamYellowField = gmType.GetField("aiTeamYellow");
            System.Reflection.FieldInfo aiTeamRedField = gmType.GetField("aiTeamRed");
            System.Reflection.FieldInfo redTurnField = gmType.GetField("redTurn");
            
            if (aiTeamYellowField != null && aiTeamRedField != null && redTurnField != null)
            {
                bool isAIYellow = (bool)aiTeamYellowField.GetValue(gm);
                bool isAIRed = (bool)aiTeamRedField.GetValue(gm);
                bool isRedTurn = (bool)redTurnField.GetValue(gm);
                
                // Determine if current shooter is AI
                bool isAIShot = (isRedTurn && isAIRed) || (!isRedTurn && isAIYellow);
                
                if (isAIShot)
                {
                    // AI shot - get AI team's weight accuracy
                    TeamManager teamManager = FindObjectOfType<TeamManager>();
                    if (teamManager != null)
                    {
                        float aiWeight = GetAITeamWeightSkill(teamManager, isRedTurn);
                        Debug.Log($"[FlickShot Skill] AI shot detected - weight skill: {aiWeight:F1}%");
                        return aiWeight;
                    }
                }
            }
        }
        
        // Player shot - get from CareerManager
        CareerManager cm = FindObjectOfType<CareerManager>();
        if (cm != null && cm.cStats != null)
        {
            float playerWeight = cm.cStats.weightAccuracy; // 0-100
            Debug.Log($"[FlickShot Skill] Player shot detected - weight skill: {playerWeight:F1}%");
            return playerWeight;
        }
        
        Debug.LogWarning("[FlickShot Skill] Could not find weight skill - using default 50%");
        return 50f; // Fallback: average skill
    }
    
    /// <summary>
    /// Get AI team's weight accuracy from TeamManager
    /// </summary>
    private float GetAITeamWeightSkill(TeamManager teamManager, bool isRedTeam)
    {
        System.Type tmType = teamManager.GetType();
        
        // Get the AI team (opposite of player team)
        System.Reflection.FieldInfo redTeamField = tmType.GetField("redTeam");
        System.Reflection.FieldInfo yellowTeamField = tmType.GetField("yellowTeam");
        
        if (redTeamField != null && yellowTeamField != null)
        {
            object redTeam = redTeamField.GetValue(teamManager);
            object yellowTeam = yellowTeamField.GetValue(teamManager);
            object aiTeam = isRedTeam ? redTeam : yellowTeam;
            
            if (aiTeam != null)
            {
                // Get weight field from Team
                System.Type teamType = aiTeam.GetType();
                System.Reflection.FieldInfo weightField = teamType.GetField("weight");
                
                if (weightField != null)
                {
                    int weight = (int)weightField.GetValue(aiTeam);
                    Debug.Log($"[FlickShot Skill] AI team weight: {weight}");
                    return (float)weight;
                }
            }
        }
        
        Debug.LogWarning("[FlickShot Skill] Could not find AI team weight - using default 50");
        return 50f; // Fallback
    }
    
    /// <summary>
    /// ? NEW: Setup and show input zone border (green rectangle)
    /// Only visible during PowerPhase when player is using flick shot mode
    /// </summary>
    private void SetupInputZoneBorder()
    {
        if (inputZoneBorder == null) return;
        
        // Check if we should show input zone
        // Only show if: flick shot mode is enabled AND we're in power phase
        bool shouldShowZone = isEnabled && currentPhase == FlickShotPhase.PowerPhase;
        
        if (shouldShowZone)
        {
            float minY = powerDragStartY - inputZoneBufferY;  // -25.5f (below launcher)
            float maxY = powerDragTargetY + inputZoneBufferAboveHog; // -15.5f (above hog line)
            float maxX = inputZoneMaxX; // �1.0 unit from center
            
            // Draw zone rectangle (5 points to close loop)
            inputZoneBorder.SetPosition(0, new Vector3(-maxX, minY, -1f));
            inputZoneBorder.SetPosition(1, new Vector3(maxX, minY, -1f));
            inputZoneBorder.SetPosition(2, new Vector3(maxX, maxY, -1f));
            inputZoneBorder.SetPosition(3, new Vector3(-maxX, maxY, -1f));
            inputZoneBorder.SetPosition(4, new Vector3(-maxX, minY, -1f)); // Close loop
            inputZoneBorder.enabled = true;
            
            Debug.Log($"[FlickShot] ? Input zone SHOWN (player's turn, flick mode enabled): X=�{maxX}, Y={minY} to {maxY}");
        }
        else
        {
            // Hide input zone if not in power phase or flick mode disabled
            inputZoneBorder.enabled = false;
            Debug.Log("[FlickShot] ? Input zone HIDDEN (not in power phase or flick mode disabled)");
        }
    }
}
