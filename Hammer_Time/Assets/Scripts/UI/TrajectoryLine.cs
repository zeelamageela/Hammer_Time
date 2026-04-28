using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryLine : MonoBehaviour
{
    private LineRenderer lr;
    private EdgeCollider2D edgeCol;
    public GameObject launcher;
    public Traj_Transform trajTransform;
    public GameManager gm;
    public CameraManager cm;
    public TeamManager tm;

    GameObject rock;
    Rock_Info rockInfo;

    public float springDistance;
    public Vector3 springDirection;
    public float angle;

    public GameObject curlPointGO;
    public Vector3 curlPoint;
    public GameObject targetPointGO;
    public Vector3 targetPoint;
    public GameObject hogLinePointGO;
    public Vector3 hogLinePoint;

    public GameObject aimCircle;
    public int dotCount;
    public GameObject dot;

    public GameObject shootKnob;
    Color knobColour;

    public int lookAheadCount;
    int lkAhd;
    List<GameObject> dots;
    List<Vector2> points;
    
    // Line renderer for actual rock path
    private LineRenderer actualPathLine;
    private GameObject actualPathLineObj;
    private List<Vector3> actualPathPoints = new List<Vector3>();
    
    // Fixed trajectory color: #636363 at 30% opacity
    private static readonly Color TRAJECTORY_COLOR = new Color(0.388f, 0.388f, 0.388f, 0.3f); // #636363 with 30% alpha

    bool aiTurn;
    Vector2 initVel;
    
    // Physics simulation
    private TrajectorySimulator trajectorySimulator;
    public bool usePhysicsSimulation = true;
    
    [Header("Physics Tuning")]
    [Tooltip("EFFECTIVE ice friction for trajectory prediction - Higher = shorter distance. Rock has 0.38 but ACTUAL effective damping is ~0.62 due to angular drag + other factors. Tune this to match reality!")]
    [Range(0.3f, 1.0f)]
    public float iceFriction = 0.62f;  // TUNED: Increased from 0.38 to match ACTUAL rock behavior (travels ~60% of predicted distance)
    
    [Tooltip("Curl strength - base lateral force multiplier")]
    public float curlStrength = 0.25f;  // TUNED: Reduced from 0.3 for less curl
    
    [Tooltip("How much curl increases at the end (0.0 = no late breaking, 3.0 = very dramatic)")]
    [Range(0f, 25f)]
    public float lateBreakingIntensity = 1.5f;  // TUNED: Reduced from 2.0 for less dramatic late breaking
    
    [Tooltip("Shape of the late breaking curve (0.01 = extremely subtle, 1.0 = linear, 2.0 = very exponential/late)")]
    [Range(0.01f, 25f)]
    public float lateBreakingCurve = 0.8f;
    
    [Header("Velocity Tuning - Player Feel")]
    [Tooltip("Velocity multiplier for pullback calculation. Higher = more speed from same pullback. Default 5.0 matches original feel.")]
    [Range(0.1f, 30.0f)]
    public float velocityMultiplier = 5.0f;
    
    [Tooltip("Minimum pullback distance before trajectory shows. Too low = accidental throws.")]
    [Range(0.1f, 2.0f)]
    public float minPullbackDistance = 1f;
    
    [Tooltip("Maximum pullback distance allowed. Limits max shot power.")]
    [Range(2.0f, 6.0f)]
    public float maxPullbackDistance = 2.75f;
    
    [Tooltip("Minimum velocity (m/s) from smallest valid pullback. Controls weakest possible shot.")]
    [Range(0.1f, 5.0f)]
    public float minVelocity = 5f;
    
    [Tooltip("Maximum velocity (m/s) from largest pullback. Controls strongest possible shot.")]
    [Range(8.0f, 25.0f)]
    public float maxVelocity = 16.0f;
    
    // Track previous values to detect changes
    private float lastIceFriction = -1f; // FIXED: Initialize to -1 to force first update
    private float lastCurlStrength = -1f;
    private float lastLateBreakingIntensity = -1f;
    private float lastLateBreakingCurve = -1f;
    
    [Header("Collision Visualization")]
    public GameObject collisionMarker; // Assign in inspector
    public GameObject hitRockGhost; // Assign in inspector - shows where hit rock will end up
    private GameObject currentCollisionMarker;
    private GameObject currentHitRockGhost;
    public bool showCollisionPrediction = true;
    public bool showHitRockTrajectory = false; // Show the path of the rock that gets hit
    public Color hitRockTrajectoryColor = new Color(1f, 0.5f, 0f, 0.5f);
    
    // Post-collision trajectory line
    private LineRenderer postCollisionLine;
    private GameObject postCollisionLineObj;
    private LineRenderer hitRockPostCollisionLine;
    private GameObject hitRockPostCollisionLineObj;
    
    // NEW: Additional line to show hit rock's exit direction at collision point
    private LineRenderer hitRockDirectionLine;
    private GameObject hitRockDirectionLineObj;
    
    // Store trajectory speeds for variable dot sizing
    private List<float> trajectorySpeed;
    
    // Visualization settings (controlled by UI toggles)
    private GameVisualizationSettings visualSettings;
    private bool trajectoryDotsVisible = true;
    private bool collisionLinesVisible = true;
    private bool aimCircleVisible = true;
    private bool guidelinesVisible = true;
    private bool curlLineVisible = true;
    private bool collisionWarningVisible = true;
    
    // Alternative aim visualization (when aim circle is OFF)
    private LineRenderer aimVerticalLine;
    private GameObject aimVerticalLineObj;
    private LineRenderer aimHorizontalLine;
    private GameObject aimHorizontalLineObj;
    private LineRenderer aimCurlLine;
    private GameObject aimCurlLineObj;
    
    // Collision warning indicator (small dotted line at collision point)
    private LineRenderer collisionWarningLine;
    private GameObject collisionWarningLineObj;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        edgeCol = GetComponent<EdgeCollider2D>();

        dots = new List<GameObject>();
        trajectorySpeed = new List<float>();
        aimCircle.GetComponent<SpriteRenderer>().enabled = false;
        
        // Subscribe to visualization settings
        visualSettings = GameVisualizationSettings.Instance;
        trajectoryDotsVisible = visualSettings.TrajectoryVisible;
        collisionLinesVisible = visualSettings.CollisionLinesVisible;
        aimCircleVisible = visualSettings.AimCircleVisible;
        guidelinesVisible = visualSettings.GuidelinesVisible;
        curlLineVisible = visualSettings.CurlLineVisible;
        collisionWarningVisible = visualSettings.CollisionWarningVisible;
        
        visualSettings.OnTrajectoryVisibilityChanged += OnTrajectoryVisibilityChanged;
        visualSettings.OnCollisionLinesVisibilityChanged += OnCollisionLinesVisibilityChanged;
        visualSettings.OnAimCircleVisibilityChanged += OnAimCircleVisibilityChanged;
        visualSettings.OnGuidelinesVisibilityChanged += OnGuidelinesVisibilityChanged;
        visualSettings.OnCurlLineVisibilityChanged += OnCurlLineVisibilityChanged;
        visualSettings.OnCollisionWarningVisibilityChanged += OnCollisionWarningVisibilityChanged;
        
        Debug.Log($"[TrajectoryLine] Visualization settings initialized - Dots: {trajectoryDotsVisible}, Collision: {collisionLinesVisible}, Aim Circle: {aimCircleVisible}, Guidelines: {guidelinesVisible}, CurlLine: {curlLineVisible}, CollisionWarning: {collisionWarningVisible}");

        lr.enabled = false;
        CareerManager cm = FindAnyObjectByType<CareerManager>();
        lkAhd = Mathf.RoundToInt(lookAheadCount * cm.cStats.sweepEndurance);
        
        // Create actual path line renderer (follows rock)
        actualPathLineObj = new GameObject("ActualPathLine");
        actualPathLineObj.transform.parent = transform;
        actualPathLine = actualPathLineObj.AddComponent<LineRenderer>();
        actualPathLine.startWidth = 0.06f; // 50% thinner (was 0.12f)
        actualPathLine.endWidth = 0.04f;   // 50% thinner (was 0.08f)
        actualPathLine.material = new Material(Shader.Find("Sprites/Default"));
        // Fixed gray color: #636363 at 30% opacity
        actualPathLine.startColor = TRAJECTORY_COLOR;
        actualPathLine.endColor = TRAJECTORY_COLOR;
        actualPathLine.enabled = false;
        
        // Create post-collision line renderer for thrown rock (ORANGE ARROW)
        postCollisionLineObj = new GameObject("PostCollisionArrow");
        postCollisionLineObj.transform.parent = transform;
        postCollisionLine = postCollisionLineObj.AddComponent<LineRenderer>();
        postCollisionLine.startWidth = 0.15f; // REDUCED: Thinner arrows (was 0.25f)
        postCollisionLine.endWidth = 0.08f;   // REDUCED: Tapers to point (was 0.15f)
        postCollisionLine.material = new Material(Shader.Find("Sprites/Default"));
        postCollisionLine.startColor = new Color(1f, 0.5f, 0f, 1f); // Bright orange - full opacity
        postCollisionLine.endColor = new Color(1f, 0.3f, 0f, 0.8f); // Slightly transparent end
        postCollisionLine.sortingOrder = 1; // Render on top of other elements
        postCollisionLine.enabled = false;
        
        // Create post-collision line renderer for hit rock (YELLOW ARROW)
        hitRockPostCollisionLineObj = new GameObject("HitRockPostCollisionArrow");
        hitRockPostCollisionLineObj.transform.parent = transform;
        hitRockPostCollisionLine = hitRockPostCollisionLineObj.AddComponent<LineRenderer>();
        hitRockPostCollisionLine.startWidth = 0.15f; // REDUCED: Thinner arrows (was 0.25f)
        hitRockPostCollisionLine.endWidth = 0.08f;   // REDUCED: Tapers to point (was 0.15f)
        hitRockPostCollisionLine.material = new Material(Shader.Find("Sprites/Default"));
        hitRockPostCollisionLine.startColor = new Color(1f, 1f, 0f, 1f); // Bright yellow - full opacity
        hitRockPostCollisionLine.endColor = new Color(1f, 1f, 0f, 0.8f); // Slightly transparent end
        hitRockPostCollisionLine.sortingOrder = 1; // Render on top of other elements
        hitRockPostCollisionLine.enabled = false;
        
        // OLD: hitRockDirectionLine - now redundant, merged into hitRockPostCollisionLine
        // Keeping the variable for backwards compatibility but it won't be used
        
        // Create alternative aim visualization lines (for when aim circle is OFF)
        // VERTICAL LINE: Shows lateral aim position (X) at Y=8
        aimVerticalLineObj = new GameObject("AimVerticalLine");
        aimVerticalLineObj.transform.parent = transform;
        aimVerticalLine = aimVerticalLineObj.AddComponent<LineRenderer>();
        aimVerticalLine.startWidth = 0.08f;
        aimVerticalLine.endWidth = 0.08f;
        aimVerticalLine.material = new Material(Shader.Find("Sprites/Default"));
        aimVerticalLine.startColor = Color.white;
        aimVerticalLine.endColor = Color.white;
        aimVerticalLine.sortingOrder = 1; // Render on top of other elements
        aimVerticalLine.enabled = false;
        
        // HORIZONTAL LINE: Shows weight (distance) at Y=aim circle Y
        aimHorizontalLineObj = new GameObject("AimHorizontalLine");
        aimHorizontalLineObj.transform.parent = transform;
        aimHorizontalLine = aimHorizontalLineObj.AddComponent<LineRenderer>();
        aimHorizontalLine.startWidth = 0.08f;
        aimHorizontalLine.endWidth = 0.08f;
        aimHorizontalLine.material = new Material(Shader.Find("Sprites/Default"));
        aimHorizontalLine.startColor = Color.white;
        aimHorizontalLine.endColor = Color.white;
        aimHorizontalLine.sortingOrder = 1; // Render on top of other elements
        aimHorizontalLine.enabled = false;
        
        // CURL LINE: Shows turn and curl direction from vertical line to aim circle
        aimCurlLineObj = new GameObject("AimCurlLine");
        aimCurlLineObj.transform.parent = transform;
        aimCurlLine = aimCurlLineObj.AddComponent<LineRenderer>();
        aimCurlLine.startWidth = 0.08f;
        aimCurlLine.endWidth = 0.08f;
        aimCurlLine.material = new Material(Shader.Find("Sprites/Default"));
        aimCurlLine.startColor = Color.white;
        aimCurlLine.endColor = Color.white;
        aimCurlLine.sortingOrder = 1; // Render on top of other elements
        aimCurlLine.enabled = false;
        
        // Create collision warning line (small dotted vertical indicator at collision point)
        collisionWarningLineObj = new GameObject("CollisionWarningLine");
        collisionWarningLineObj.transform.parent = transform;
        collisionWarningLine = collisionWarningLineObj.AddComponent<LineRenderer>();
        collisionWarningLine.startWidth = 0.06f;
        collisionWarningLine.endWidth = 0.06f;
        collisionWarningLine.material = new Material(Shader.Find("Sprites/Default"));
        collisionWarningLine.startColor = new Color(1f, 0f, 0f, 0.8f); // Red with transparency
        collisionWarningLine.endColor = new Color(1f, 0f, 0f, 0.8f);
        collisionWarningLine.sortingOrder = 2; // Render on top of aim lines
        collisionWarningLine.enabled = false;
        
        // Make it dotted by using textureMode and material settings
        collisionWarningLine.textureMode = LineTextureMode.Tile;
        // Note: For actual dotted appearance, you'd need a dotted texture
        // For now, we'll use opacity and thin width to make it subtle
        
        Debug.Log("[TrajectoryLine] Collision warning line created");
        
        // Initialize physics simulator ONCE at startup for better performance
        UpdateSimulator();
        
        Debug.Log($"[TrajectoryLine] Physics initialized - trajectory will use tuned damping ratio");
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log(collider.name);

        if (collider == rock.GetComponent<Collider2D>())
        {
            Debug.Log(collider.name);
        }
        //else trajCollision = true;
    }

    private void Update()
    {
        // CRITICAL FIX: Add comprehensive null checks
        if (gm == null || gm.rockList == null || gm.rockList.Count == 0)
            return;
        
        // CRITICAL FIX: Check for negative rockCurrent (before first rock)
        if (gm.rockCurrent < 0 || gm.rockCurrent >= gm.rockList.Count)
            return;
        
        if (gm.rockList[gm.rockCurrent] == null || gm.rockList[gm.rockCurrent].rock == null)
            return;
        
        rock = gm.rockList[gm.rockCurrent].rock;
        rockInfo = gm.rockList[gm.rockCurrent].rockInfo;
        rock = gm.rockList[gm.rockCurrent].rock;
        rockInfo = gm.rockList[gm.rockCurrent].rockInfo;
        
        if (gm.aiTeamRed)
        {
            if (gm.redHammer)
            {
                if (gm.rockCurrent % 2 == 0)
                    aiTurn = false;
                else
                    aiTurn = true;
            }
            else
            {
                if (gm.rockCurrent % 2 == 0)
                    aiTurn = true;
                else
                    aiTurn = false;
            }
        }
        else if (gm.aiTeamYellow)
        {
            if (gm.redHammer)
            {
                if (gm.rockCurrent % 2 == 0)
                    aiTurn = true;
                else
                    aiTurn = false;
            }
            else
            {
                if (gm.rockCurrent % 2 == 0)
                    aiTurn = false;
                else
                    aiTurn = true;
            }
        }

        knobColour = shootKnob.GetComponent<SpriteRenderer>().color;

        // HUMAN ROCK PATH TRACKING
        // Only track and display path for human-thrown rocks (not AI)
        if (rock != null && rockInfo != null && !aiTurn)
        {
            if (rockInfo.released && !rockInfo.rest)
            {
                // Rock is moving - actively trace the path
                Vector2 rockPos = new Vector2(rock.transform.position.x, rock.transform.position.y);
                
                // Add current position to actual path
                actualPathPoints.Add(new Vector3(rockPos.x, rockPos.y, 0f));
                
                // Update line renderer
                actualPathLine.enabled = true;
                actualPathLine.positionCount = actualPathPoints.Count;
                actualPathLine.SetPositions(actualPathPoints.ToArray());
                
                // Color is always fixed - no dynamic changes needed
            }
            else if (rockInfo.released && rockInfo.rest)
            {
                // Rock has stopped - keep path visible but don't add more points
                actualPathLine.enabled = true;
            }
        }
    }

    // Public method to clear trajectory visualization (called when any turn starts)
    public void ClearTrajectory()
    {
        // Clear dots
        if (dots.Count != 0)
        {
            foreach (GameObject dot in dots)
            {
                Destroy(dot);
            }
            dots.Clear();
        }
        
        // Clear actual path from previous shot
        if (actualPathPoints.Count > 0)
        {
            actualPathPoints.Clear();
            actualPathLine.enabled = false;
        }
        
        // CRITICAL FIX: Clear the actual path line positions too (prevents black line artifact)
        if (actualPathLine != null)
        {
            actualPathLine.positionCount = 0;
        }
        
        // Clean up collision markers and arrows
        if (currentCollisionMarker != null)
        {
            Destroy(currentCollisionMarker);
            currentCollisionMarker = null;
        }
        if (currentHitRockGhost != null)
        {
            Destroy(currentHitRockGhost);
            currentHitRockGhost = null;
        }
        if (postCollisionLine != null)
        {
            postCollisionLine.enabled = false;
        }
        if (hitRockPostCollisionLine != null)
        {
            hitRockPostCollisionLine.enabled = false;
        }
        
        // Hide alternative aim visualization lines
        if (aimVerticalLine != null)
        {
            aimVerticalLine.enabled = false;
        }
        if (aimHorizontalLine != null)
        {
            aimHorizontalLine.enabled = false;
        }
        if (aimCurlLine != null)
        {
            aimCurlLine.enabled = false;
        }
        
        // Hide collision warning line
        if (collisionWarningLine != null)
        {
            collisionWarningLine.enabled = false;
        }
        
        // CRITICAL FIX: Also hide the main trajectory line renderer
        lr.enabled = false;
    }
    
    // Public method to hide the trajectory line renderer
    public void HideTrajectoryLine()
    {
        if (lr != null)
        {
            lr.enabled = false;
        }
    }
    
    // Public method to show the trajectory line renderer (only for player turns)
    public void ShowTrajectoryLine()
    {
        if (lr != null)
        {
            // CRITICAL FIX: Clear positions BEFORE enabling to prevent showing old trajectory
            lr.positionCount = 0;
            lr.enabled = true;
        }
    }

    public void DrawTrajectory()
    {
        //aiTurn = false;
        
        // Clear previous trajectory before drawing new one
        ClearTrajectory();

        springDistance = trajTransform.springDistance;
        
        // PERFORMANCE: Only update simulator if PHYSICS settings changed (not turn direction!)
        // Turn direction is passed to SimulateTrajectory() every time, so no simulator update needed
        bool physicsSettingsChanged = iceFriction != lastIceFriction 
            || curlStrength != lastCurlStrength 
            || lateBreakingIntensity != lastLateBreakingIntensity 
            || lateBreakingCurve != lastLateBreakingCurve;
        
        if (physicsSettingsChanged)
        {
            UpdateSimulator();
            lastIceFriction = iceFriction;
            lastCurlStrength = curlStrength;
            
            Debug.Log($"⚠️ PHYSICS SETTINGS CHANGED! Updated simulator.");
        }
        
        Debug.Log($"[DrawTrajectory] START - springDistance: {springDistance}, usePhysics: {usePhysicsSimulation}, " +
                  $"physicsChanged: {physicsSettingsChanged}");
        
        if (springDistance < 1f)
        {
            // Not pulled back enough, don't show trajectory
            lr.positionCount = 2;
            lr.SetPosition(0, launcher.transform.position);
            lr.SetPosition(1, launcher.transform.position);
            Debug.Log("Spring distance too small, not drawing trajectory");
            return;
        }

        List<Vector3> pos = new List<Vector3>();
        
        if (usePhysicsSimulation)
        {
            // === PHYSICS-BASED TRAJECTORY WITH COLLISION DETECTION ===
            
            // Get rock component to check rotation
            Rock_Info rockInfo = gm.rockList[gm.rockCurrent].rockInfo;
            Rock_Flick rockFlick = gm.rockList[gm.rockCurrent].rock.GetComponent<Rock_Flick>();
            RockManager rm = FindObjectOfType<RockManager>();
            
            // CRITICAL FIX: Use rm.inturn as SINGLE SOURCE OF TRUTH
            // GameManager and TurnAnim ensure rm.inturn and rock.flipAxis are ALWAYS synchronized
            // Reading rm.inturn is simpler and more reliable than reading rock.flipAxis
            bool isInTurn = rm.inturn;
            
            // Debug.Log($"[Trajectory] rm.inturn={rm.inturn}, flipAxis will be={isInTurn}");
            
            // Get actual positions
            Vector2 launcherPos = new Vector2(
                launcher.transform.position.x, 
                launcher.transform.position.y
            );
            
            // Get the rock's current pullback position (where it's being dragged to)
            Vector2 pullbackPos = new Vector2(
                gm.rockList[gm.rockCurrent].rock.transform.position.x,
                gm.rockList[gm.rockCurrent].rock.transform.position.y
            );
            
            // CRITICAL FIX: Calculate pullback distance (for logging), but velocity comes from ROCK calculation
            // This ensures trajectory uses EXACT same velocity as the rock will use when released
            Vector2 displacement = launcherPos - pullbackPos;
            float pullbackDistance = displacement.magnitude;
            
            // DETERMINISTIC: Get velocity that Rock_Flick WILL use (use same calculation for consistency during preview)
            // NOTE: This is ONLY for preview! Actual rock velocity is set in Rock_Flick.Release()
            // Pass inspector parameters so trajectory calculation matches player feel settings
            Vector2 initialVelocity = TrajectorySimulator.CalculateInitialVelocityFromPullback(
                pullbackPos,
                launcherPos,
                velocityMultiplier,
                minPullbackDistance,
                maxPullbackDistance,
                minVelocity,
                maxVelocity
            );
            
            Debug.Log($"[TrajectoryLine] Preview velocity: {initialVelocity.magnitude:F2} m/s (pullback: {pullbackDistance:F3} units)");
            
            // NOW log everything AFTER variables are declared
            Debug.Log($"🎯 [TrajectoryLine] SIMULATING TRAJECTORY:");
            Debug.Log($"   rm.inturn = {rm.inturn}");
            Debug.Log($"   isInTurn (USED FOR SIMULATION) = {isInTurn}");
            Debug.Log($"   Launcher position: {launcherPos}");
            Debug.Log($"   Pullback position: {pullbackPos}");
            Debug.Log($"   Lateral offset (X): {pullbackPos.x:F4} units");
            Debug.Log($"   Pullback distance (Y): {pullbackPos.y:F4} units");
            Debug.Log($"   Spring displacement: ({(launcherPos.x - pullbackPos.x):F4}, {(launcherPos.y - pullbackPos.y):F4})");
            Debug.Log($"   Initial velocity: {initialVelocity}");
            Debug.Log($"   Velocity magnitude: {initialVelocity.magnitude:F3}");
            Debug.Log($"   Lateral velocity (X): {initialVelocity.x:F4} units/s");
            Debug.Log($"   If isInTurn=true → dirMult=-1 → Rock curls in in-turn direction");
            Debug.Log($"   If isInTurn=false → dirMult=+1 → Rock curls in out-turn direction");
            
            // Debug.Log($"Pullback: {pullbackPos}, Launcher: {launcherPos}, InitVel: {initialVelocity.magnitude}");
            
            // Get all rocks currently in play (excluding the current rock being thrown)
            List<GameObject> rocksInPlay = new List<GameObject>();
            foreach (var rockEntry in gm.rockList)
            {
                if (rockEntry.rock != null 
                    && rockEntry.rock.activeInHierarchy 
                    && rockEntry.rockInfo.inPlay 
                    && rockEntry.rockInfo != rockInfo) // Don't include the rock we're throwing
                {
                    rocksInPlay.Add(rockEntry.rock);
                }
            }
            
            // Simulate the trajectory with collision detection
            // CRITICAL: Pass forPlayerPreview = true so curl matches visual expectation
            List<Vector2> simulatedPath = trajectorySimulator.SimulateTrajectory(
                launcherPos,
                initialVelocity,
                isInTurn,
                250,
                rocksInPlay,
                forPlayerPreview: true  // Use player-friendly curl (matches Rock_Force visual)
            );
            
            // Get collision info and speeds
            TrajectorySimulator.CollisionInfo collisionInfo = trajectorySimulator.GetCollisionInfo();
            trajectorySpeed = trajectorySimulator.GetTrajectorySpeed();
            
            // Convert to Vector3 for rendering
            foreach (Vector2 point in simulatedPath)
            {
                pos.Add(new Vector3(point.x, point.y, 0f));
            }
            
            // Set line renderer
            lr.positionCount = pos.Count;
            for (int i = 0; i < pos.Count; i++)
            {
                lr.SetPosition(i, pos[i]);
            }
            
            // Debug.Log($"Physics simulation complete - {pos.Count} points, collision: {collisionInfo.hasCollision}");
            
            // Visualize collision if detected (only if visibility enabled)
            if (collisionInfo.hasCollision && showCollisionPrediction && collisionLinesVisible)
            {
                Debug.Log($"[TrajectoryLine] COLLISION DETECTED! Point: {collisionInfo.collisionPoint}, Index: {collisionInfo.collisionIndex}");
                Debug.Log($"[TrajectoryLine] Thrown rock path count: {collisionInfo.thrownRockPostCollisionPath.Count}, Hit rock path count: {collisionInfo.hitRockPostCollisionPath.Count}");
                
                // Show collision point marker at impact location
                if (collisionMarker != null)
                {
                    currentCollisionMarker = Instantiate(
                        collisionMarker, 
                        collisionInfo.collisionPoint, 
                        Quaternion.identity
                    );
                    currentCollisionMarker.transform.parent = transform;
                    
                    // CRITICAL: Disable all physics components to prevent interaction
                    Rigidbody2D markerRb = currentCollisionMarker.GetComponent<Rigidbody2D>();
                    if (markerRb != null) markerRb.simulated = false;
                    
                    Collider2D markerCol = currentCollisionMarker.GetComponent<Collider2D>();
                    if (markerCol != null) markerCol.enabled = false;
                    
                    Debug.Log($"[TrajectoryLine] Collision marker instantiated at {collisionInfo.collisionPoint}");
                }
                else
                {
                    Debug.LogWarning("[TrajectoryLine] collisionMarker prefab is NULL! Assign it in Inspector");
                }
                
                // DIRECTIONAL ARROWS: Show ACTUAL deflection directions from collision
                // Orange arrow: Thrown rock's DEFLECTION direction (post-collision velocity)
                // Yellow arrow: Hit rock's EXIT direction (post-collision velocity)
                
                float arrowLength = 0.8f; // REDUCED: Smaller arrows for less clutter (was 1.5f)
                
                // Draw thrown rock's DEFLECTION direction (ORANGE ARROW)
                // Use the VELOCITY at collision point, not the path points
                if (collisionInfo.collisionIndex > 0 && collisionInfo.thrownRockPostCollisionPath.Count >= 2)
                {
                    Vector2 collisionPoint = collisionInfo.collisionPoint;
                    
                    // Calculate velocity from first 2 points of simulated path
                    // These points are TIME_STEP apart (0.05s), so velocity = delta / time
                    Vector2 firstPoint = collisionInfo.thrownRockPostCollisionPath[0]; // Collision point
                    Vector2 secondPoint = collisionInfo.thrownRockPostCollisionPath[1]; // Next point
                    
                    // Direction is from first point to second point (actual deflection)
                    Vector2 deflectionDirection = (secondPoint - firstPoint).normalized;
                    Vector2 arrowEnd = collisionPoint + deflectionDirection * arrowLength;
                    
                    postCollisionLine.enabled = true;
                    postCollisionLine.positionCount = 2;
                    postCollisionLine.SetPosition(0, new Vector3(collisionPoint.x, collisionPoint.y, 0f));
                    postCollisionLine.SetPosition(1, new Vector3(arrowEnd.x, arrowEnd.y, 0f));
                    
                    Debug.Log($"[Collision Viz] Shooter DEFLECTION arrow (ORANGE): {collisionPoint} ? {arrowEnd} (angle: {Mathf.Atan2(deflectionDirection.y, deflectionDirection.x) * Mathf.Rad2Deg:F1}°)");
                    Debug.Log($"[Collision Viz] Orange line enabled: {postCollisionLine.enabled}, material: {postCollisionLine.material != null}, color: {postCollisionLine.startColor}");
                }
                else
                {
                    Debug.LogWarning($"[Collision Viz] Cannot weight ORANGE arrow - collisionIndex: {collisionInfo.collisionIndex}, path count: {collisionInfo.thrownRockPostCollisionPath.Count}");
                }
                
                // Draw hit rock's EXIT direction (YELLOW ARROW)
                // Use the VELOCITY at impact, not continuation
                if (collisionInfo.collisionIndex > 0 && collisionInfo.hitRockPostCollisionPath.Count >= 2)
                {
                    // Hit rock starts at its original position (target rock position)
                    Vector2 hitRockStart = collisionInfo.hitRockPostCollisionPath[0];
                    Vector2 hitRockSecond = collisionInfo.hitRockPostCollisionPath[1];
                    
                    // Direction is the exit velocity of the hit rock
                    Vector2 exitDirection = (hitRockSecond - hitRockStart).normalized;
                    Vector2 hitArrowEnd = hitRockStart + exitDirection * arrowLength;
                    
                    hitRockPostCollisionLine.enabled = true;
                    hitRockPostCollisionLine.positionCount = 2;
                    hitRockPostCollisionLine.SetPosition(0, new Vector3(hitRockStart.x, hitRockStart.y, 0f));
                    hitRockPostCollisionLine.SetPosition(1, new Vector3(hitArrowEnd.x, hitArrowEnd.y, 0f));
                    
                    Debug.Log($"[Collision Viz] Hit rock EXIT arrow (YELLOW): {hitRockStart} ? {hitArrowEnd} (angle: {Mathf.Atan2(exitDirection.y, exitDirection.x) * Mathf.Rad2Deg:F1}°)");
                    Debug.Log($"[Collision Viz] Yellow line enabled: {hitRockPostCollisionLine.enabled}, material: {hitRockPostCollisionLine.material != null}, color: {hitRockPostCollisionLine.startColor}");
                }
                else
                {
                    Debug.LogWarning($"[Collision Viz] Cannot weight YELLOW arrow - collisionIndex: {collisionInfo.collisionIndex}, path count: {collisionInfo.hitRockPostCollisionPath.Count}");
                }
            }
            else
            {
                if (!collisionInfo.hasCollision)
                {
                    Debug.Log("[TrajectoryLine] No collision detected in trajectory");
                }
                if (!showCollisionPrediction)
                {
                    Debug.LogWarning("[TrajectoryLine] showCollisionPrediction is FALSE - enable it in Inspector!");
                }
            }
        }
        else
        {
            // === ORIGINAL BEZIER CURVE METHOD ===
            hogLinePoint = new Vector3(hogLinePointGO.transform.position.x, -15.75f, 0f);
            curlPoint = curlPointGO.transform.position;
            targetPoint = targetPointGO.transform.position;

            if (springDistance < 1.25)
            {
                lr.positionCount = Mathf.RoundToInt(Mathf.Lerp(3f, 5f, springDistance));
            }
            else if (springDistance < 1.5)
            {
                lr.positionCount = Mathf.RoundToInt(Mathf.Lerp(5f, 100f, springDistance));
            }
            else
            {
                lr.positionCount = 250;
            }

            float t = 0f;
            Vector3 B;
            lr.SetPosition(0, launcher.transform.position);

            for (int i = 1; i < lr.positionCount; i++)
            {
                B = ((1 - t) * (1 - t) * hogLinePoint) + (2 * (1 - t) * t * curlPoint) + (t * t * targetPoint);
                
                lr.SetPosition(i, B);
                pos.Add(B);
                t += (1 / (float)lr.positionCount);
            }
        }

        // Set line width based on pullback
        lr.startWidth = Mathf.Lerp(0f, 0.3f, springDistance / 3.25f);
        lr.endWidth = Mathf.Lerp(0f, 0.1f, springDistance / 3.25f);

        // Draw dots along trajectory (only if visibility enabled)
        Debug.Log($"[Dots] Checking dot creation: pos.Count={pos.Count}, dotCount={dotCount}, dot={dot}, visible={trajectoryDotsVisible}");
        
        if (pos.Count > 0 && dotCount > 0 && dot != null && trajectoryDotsVisible)
        {
            // Draw dots all the way through trajectory (including to collision point)
            int maxDotIndex = pos.Count;
            
            int counter = Mathf.Max(1, maxDotIndex / dotCount);
            
            Debug.Log($"Drawing {dotCount} dots from {maxDotIndex} trajectory points (every {counter} points)");
            
            // Ensure we weight dots all the way to the end
            for (int i = 1; i < maxDotIndex; i += counter)
            {
                if (dots.Count >= dotCount) break; // Limit to dotCount
                
                int pointIndex = i;
                Vector2 dotPos = pos[pointIndex];

                // IMPROVED: Variable dot size based on speed at that point
                float speed = trajectorySpeed != null && pointIndex < trajectorySpeed.Count 
                    ? trajectorySpeed[pointIndex] 
                    : 5f; // Default mid-speed
                    
                // Map speed to dot size: faster = bigger dots (0.35), slower = 75% smaller (0.0875)
                // Typical speeds: 0-20 units/sec
                float minDotSize = 0.35f * 0.25f; // 75% smaller = 25% of original
                float maxDotSize = 0.35f; // Current largest size
                float dotSize = Mathf.Lerp(minDotSize, maxDotSize, Mathf.Clamp01(speed / 15f));

                GameObject dotPlace = Instantiate(dot, dotPos, Quaternion.identity);
                dotPlace.transform.parent = transform;
                dotPlace.transform.localScale = Vector3.one * dotSize;
                
                SpriteRenderer dotRenderer = dotPlace.GetComponent<SpriteRenderer>();
                if (dotRenderer != null)
                {
                    dotRenderer.color = knobColour;
                }
                
                dots.Add(dotPlace);
            }
            
            // ENSURE last point has a dot (collision point or end point)
            if (dots.Count > 0 && maxDotIndex > 1)
            {
                Vector2 lastDotPos = pos[maxDotIndex - 1];
                GameObject lastDot = Instantiate(dot, lastDotPos, Quaternion.identity);
                lastDot.transform.parent = transform;
                lastDot.transform.localScale = Vector3.one * 0.25f;
                
                SpriteRenderer lastDotRenderer = lastDot.GetComponent<SpriteRenderer>();
                if (lastDotRenderer != null)
                {
                    lastDotRenderer.color = knobColour;
                }
                
                dots.Add(lastDot);
            }
            
            Debug.Log($"Actually created {dots.Count} dot GameObjects");
        }
        else
        {
            Debug.LogWarning($"Cannot weight dots - pos.Count: {pos.Count}, dotCount: {dotCount}, dot: {dot}");
        }

        // Store points for later comparison
        points = new List<Vector2>();
        for (int i = 0; i < pos.Count; i++)
        {
            points.Add(pos[i]);
        }

        // Position aim circle at ideal endpoint (as if no rocks were there)
        if (aimCircle != null && aimCircleVisible)
        {
            aimCircle.GetComponent<SpriteRenderer>().enabled = true;
            
            // Hide alternative aim lines when aim circle is ON
            if (aimVerticalLine != null) aimVerticalLine.enabled = false;
            if (aimHorizontalLine != null) aimHorizontalLine.enabled = false;
        }
        else
        {
            aimCircle.GetComponent<SpriteRenderer>().enabled = false;
            
            // Show alternative aim lines when aim circle is OFF
            UpdateAlternativeAimVisualization();
        }

        if (usePhysicsSimulation && trajectorySimulator != null)
        {
            // FIXED: Simulate trajectory WITHOUT rocks to get ideal target position
            // This makes aiming consistent regardless of what rocks are in play

            // CRITICAL: Use flipAxis from the rock, NOT rm.inturn!
            Rock_Force aimRockForce = gm.rockList[gm.rockCurrent].rock.GetComponent<Rock_Force>();
            RockManager rm = FindObjectOfType<RockManager>();
            bool isInTurn = aimRockForce != null ? aimRockForce.flipAxis : (rm != null ? rm.inturn : false);

            Vector2 launcherPos = new Vector2(launcher.transform.position.x, launcher.transform.position.y);
            Vector2 pullbackPos = new Vector2(
                gm.rockList[gm.rockCurrent].rock.transform.position.x,
                gm.rockList[gm.rockCurrent].rock.transform.position.y
            );

            // DETERMINISTIC: Use direct calculation with inspector parameters
            Vector2 initialVelocity = TrajectorySimulator.CalculateInitialVelocityFromPullback(
                pullbackPos,
                launcherPos,
                velocityMultiplier,
                minPullbackDistance,
                maxPullbackDistance,
                minVelocity,
                maxVelocity
            );

            // Simulate WITHOUT any rocks (empty list) to get ideal position
            // CRITICAL: Pass forPlayerPreview = true for consistent visual curl
            List<Vector2> idealPath = trajectorySimulator.SimulateTrajectory(
                launcherPos, initialVelocity, isInTurn, 250, new List<GameObject>(), forPlayerPreview: true
            );

            TrajectorySimulator.CollisionInfo idealInfo = trajectorySimulator.GetCollisionInfo();

            if (idealInfo.finalPosition != Vector2.zero)
            {
                aimCircle.transform.position = idealInfo.finalPosition;
            }
            else if (idealPath.Count > 0)
            {
                aimCircle.transform.position = idealPath[idealPath.Count - 1];
            }
        }
        else if (pos.Count > 0)
        {
            // Fallback to last trajectory point (Bezier mode)
            aimCircle.transform.position = pos[pos.Count - 1];
        }
        aimCircle.GetComponent<SpriteRenderer>().color = knobColour;

        CameraManager camManager = FindObjectOfType<CameraManager>();
        camManager.Trajectory();
    }
    
    // Helper method to recreate the simulator when settings change
    private void UpdateSimulator()
    {
        // SIMPLIFIED: No late breaking parameters
        trajectorySimulator = new TrajectorySimulator(iceFriction, curlStrength);
    }

    public void Release()
    {
        aimCircle.GetComponent<SpriteRenderer>().enabled = false;
        //lr.enabled = true;
        lr.startWidth = 0.075f;
        lr.endWidth = 0.075f;
        
        // KEEP trajectory dots visible during rock movement
        // They will be cleared when DrawTrajectory() is called for the next turn
        
        // Initialize actual path tracking for human player
        if (!aiTurn)
        {
            actualPathPoints.Clear();
            if (rock != null)
            {
                Vector2 startPos = new Vector2(rock.transform.position.x, rock.transform.position.y);
                actualPathPoints.Add(new Vector3(startPos.x, startPos.y, 0f));
            }
        }
        
        // Clean up collision visualization (will show actual collision when it happens)
        if (currentCollisionMarker != null)
        {
            Destroy(currentCollisionMarker);
            currentCollisionMarker = null;
        }
        if (currentHitRockGhost != null)
        {
            Destroy(currentHitRockGhost);
            currentHitRockGhost = null;
        }
        if (postCollisionLine != null)
        {
            postCollisionLine.enabled = false;
        }
        if (hitRockPostCollisionLine != null)
        {
            hitRockPostCollisionLine.enabled = false;
        }
    }


    //void DrawQuadraticBezierCurve(Vector3 point0, Vector3 point1, Vector3 point2)
    //{
    //    lr.positionCount = 200;
    //    float t = 0f;
    //    Vector3 B = new Vector3(0, -25, 0);

    //    lr.SetPosition(0, launcher.transform.position);

    //    for (int i = 1; i<lr.positionCount; i++)
    //    {
    //        B = ((1 - t) * (1 - t) * point0) + (2 * (1 - t) * t * point1) + (t * t * point2);
    //        lr.SetPosition(i, B);
    //        t += (1 / (float)lr.positionCount);
    //    }
    //}
    
    /// <summary>
    /// Called when trajectory dot visibility setting changes (from UI toggle)
    /// </summary>
    private void OnTrajectoryVisibilityChanged(bool visible)
    {
        trajectoryDotsVisible = visible;
        Debug.Log($"[TrajectoryLine] Trajectory dots visibility changed to: {visible}");
        
        // If currently showing trajectory, redraw it with new visibility setting
        if (gm != null && gm.rockList != null && gm.rockCurrent < gm.rockList.Count)
        {
            GameObject currentRock = gm.rockList[gm.rockCurrent].rock;
            Rock_Info currentRockInfo = gm.rockList[gm.rockCurrent].rockInfo;
            
            // Only redraw if we're in aiming mode (not released yet)
            if (currentRock != null && currentRockInfo != null && !currentRockInfo.released)
            {
                // Clear existing dots
                if (dots.Count > 0)
                {
                    foreach (GameObject dot in dots)
                    {
                        if (dot != null) Destroy(dot);
                    }
                    dots.Clear();
                }
                
                // If turning ON, redraw trajectory to show dots
                if (visible)
                {
                    DrawTrajectory();
                }
            }
        }
    }
    
    /// <summary>
    /// Called when collision lines visibility setting changes (from UI toggle)
    /// </summary>
    private void OnCollisionLinesVisibilityChanged(bool visible)
    {
        collisionLinesVisible = visible;
        Debug.Log($"[TrajectoryLine] Collision lines visibility changed to: {visible}");
        
        // Immediately hide/show collision visualization if it exists
        if (!visible)
        {
            // Hide all collision visualization
            if (currentCollisionMarker != null)
            {
                currentCollisionMarker.SetActive(false);
            }
            if (postCollisionLine != null)
            {
                postCollisionLine.enabled = false;
            }
            if (hitRockPostCollisionLine != null)
            {
                hitRockPostCollisionLine.enabled = false;
            }
        }
        else
        {
            // Re-enable if aiming and collision exists
            if (gm != null && gm.rockList != null && gm.rockCurrent < gm.rockList.Count)
            {
                GameObject currentRock = gm.rockList[gm.rockCurrent].rock;
                Rock_Info currentRockInfo = gm.rockList[gm.rockCurrent].rockInfo;
                
                if (currentRock != null && currentRockInfo != null && !currentRockInfo.released)
                {
                    // Redraw to show collision visualization
                    DrawTrajectory();
                }
            }
        }
    }
    
    /// <summary>
    /// Called when aim circle visibility setting changes (from UI toggle)
    /// </summary>
    private void OnAimCircleVisibilityChanged(bool visible)
    {
        aimCircleVisible = visible;
        Debug.Log($"[TrajectoryLine] Aim circle visibility changed to: {visible}");
        
        // Toggle between aim circle and alternative visualization
        if (aimCircle != null)
        {
            aimCircle.GetComponent<SpriteRenderer>().enabled = visible;
        }
        
        // Update visualization if currently aiming
        if (gm != null && gm.rockList != null && gm.rockCurrent < gm.rockList.Count)
        {
            GameObject currentRock = gm.rockList[gm.rockCurrent].rock;
            Rock_Info currentRockInfo = gm.rockList[gm.rockCurrent].rockInfo;
            
            if (currentRock != null && currentRockInfo != null && !currentRockInfo.released)
            {
                if (visible)
                {
                    // Show aim circle, hide alternative lines
                    if (aimVerticalLine != null) aimVerticalLine.enabled = false;
                    if (aimHorizontalLine != null) aimHorizontalLine.enabled = false;
                    if (aimCurlLine != null) aimCurlLine.enabled = false;
                }
                else
                {
                    // Hide aim circle, show alternative lines
                    UpdateAlternativeAimVisualization();
                }
            }
        }
    }
    
    /// <summary>
    /// Draw alternative aim visualization when aim circle is OFF
    /// Shows vertical line at lateral aim position and horizontal line for weight
    /// </summary>
    private void UpdateAlternativeAimVisualization()
    {
        if (shootKnob == null || launcher == null || aimCircle == null)
            return;
        
        // Get pullback position (from shooting knob / rock position)
        Vector2 pullbackPos = Vector2.zero;
        if (gm != null && gm.rockList != null && gm.rockCurrent < gm.rockList.Count)
        {
            GameObject currentRock = gm.rockList[gm.rockCurrent].rock;
            if (currentRock != null)
            {
                pullbackPos = currentRock.transform.position;
            }
        }
        
        Vector2 launcherPos = launcher.transform.position;
        
        // Calculate straight-line trajectory direction (no curl)
        Vector2 direction = (launcherPos - pullbackPos).normalized;
        
        // COLLISION DETECTION DISABLED FOR GUIDE LINES
        // Guide lines show ideal aim positions, not collision warnings
        // Actual trajectory (with curl) handles collision visualization separately
        bool hasCollision = false; // Always false - no collision detection for guide lines
        
        // Determine vertical line position and height
        float verticalLineX;
        float verticalLineTopY;
        float verticalLineBottomY = 8.4f; // Default bottom anchor
        
        // HORIZONTAL LINE: Use aim circle's Y position (ideal endpoint without collisions)
        // This shows where the rock WOULD go if no obstacles were present
        float horizontalLineY = aimCircle.transform.position.y;
        Debug.Log($"[Horizontal Line] Using AIM CIRCLE Y position: Y={horizontalLineY:F2}");
        
        
        
        
        // ===== GUIDE LINE MODE (no collision detection) =====
        // Calculate X position from straight-line projection
        float deltaY = horizontalLineY - pullbackPos.y;
        if (horizontalLineY > 8.0f)
        {
            // If aiming above 8.0, use actual trajectory direction for better visual guidance
            // This accounts for curl and gives a more accurate vertical line position
            deltaY = 8.0f - pullbackPos.y;
            Debug.Log($"[Vertical Line] Aiming above 8.0, using trajectory direction for vertical line X calculation");
        }
        if (Mathf.Abs(direction.y) > 0.001f)
        {
            float t = deltaY / direction.y;
            verticalLineX = pullbackPos.x + (direction.x * t);
        }
        else
        {
            verticalLineX = pullbackPos.x;
        }
        
        // ZONE-BASED LOGIC for vertical line endpoints
        // CRITICAL: Use ACTUAL final trajectory point Y position for vertical line endpoints!
        // This makes the vertical line end exactly at the trajectory dot's Y position
        //if (points != null && points.Count > 0)
        //{
        //    // STRAIGHT-LINE PROJECTION LOGIC (ignore trajectory)
        //    // Use the last simulated trajectory point's Y (distance/weight position)
        //    float trajectoryDotY = points[points.Count - 1].y;
        //    Debug.Log($"[Vertical Line] Using FINAL TRAJECTORY DOT Y position for endpoints: Y={trajectoryDotY:F2}");
            
        //    // Simple logic: vertical line goes from trajectory dot Y up to Y=8.4
        //    // This is based on STRAIGHT-LINE aim, NOT actual trajectory
        //    verticalLineTopY = trajectoryDotY - 0.5f;
        //}
        //else
        //{
        //    verticalLineTopY = horizontalLineY - 0.5f; // Default to 0.5 above horizontal line if no trajectory points
        //     Debug.LogWarning($"[Vertical Line] No trajectory points available, defaulting vertical line top Y to: {verticalLineTopY:F2}");
        //}

        verticalLineTopY = horizontalLineY - 0.5f;

        if (verticalLineTopY > 7.5f)
            verticalLineTopY = 7.5f;

        if (horizontalLineY <= 8.0f)
            verticalLineBottomY = 8.4f;
        else
            verticalLineBottomY = horizontalLineY + 0.4f;


        // SKILL-BASED LINE WIDTH
        // Get shooter's AIM accuracy (X-axis) from CareerManager (for player) or CharacterStats (for AI)
        // Aim skill controls lateral positioning = line width represents X-axis precision
        float aimSkill = 50f; // Default mid-skill
        
        // Check if this is the player's rock or AI rock
        CareerManager cm = FindFirstObjectByType<CareerManager>();
        TeamManager teamManager = FindFirstObjectByType<TeamManager>();
        
        if (cm != null && cm.cStats != null)
        {
            // Player rock - use AIM accuracy (lateral positioning skill)
            aimSkill = cm.cStats.aimAccuracy;
        }
        else if (teamManager != null)
        {
            // AI rock - try to get current team's average skill
            // Fall back to default 50 if not available
            aimSkill = 50f;
        }
        
        // Map AIM skill (0-100) to line width (0.15 thick for beginners, 0.04 thin for pros)
        // Skill 0 = 0.15 (thick), Skill 100 = 0.04 (thin)
        float lineWidth = Mathf.Lerp(0.10f, 0.04f, aimSkill / 100f);
        
        // VERTICAL LINE: Shows where rock would go WITHOUT CURL (straight-line aim)
        // Always shows ideal aim position (no collision warnings)
        if (aimVerticalLine != null && guidelinesVisible)
        {
            aimVerticalLine.enabled = true;
            aimVerticalLine.positionCount = 2;
            // Top of vertical line: trajectory endpoint (+0.4 offset)
            aimVerticalLine.SetPosition(0, new Vector3(verticalLineX, verticalLineTopY, 0f));
            // Bottom of vertical line: extend down to Y=8.4 for visual reference
            aimVerticalLine.SetPosition(1, new Vector3(verticalLineX, verticalLineBottomY, 0f));
            
            // Apply skill-based width
            aimVerticalLine.startWidth = lineWidth;
            aimVerticalLine.endWidth = lineWidth;
            
            // Color: Always greyish-black (no red collision warning)
            // Greyish-black: #3A3A3A (dark grey, 23% brightness)
            Color lineColor = new Color(0.23f, 0.23f, 0.23f, 1f);
            
            aimVerticalLine.startColor = lineColor;
            aimVerticalLine.endColor = lineColor;
        }
        
        // HORIZONTAL LINE: Fixed width across ice at trajectory endpoint Y
        // Shows weight (distance shot will travel) - TEAM COLOR
        if (aimHorizontalLine != null && guidelinesVisible)
        {
            // Fixed X positions spanning ice width
            float iceLeftEdge = -2.23f;  // Tunable - left edge of ice
            float iceRightEdge = 2.25f;  // Tunable - right edge of ice
            
            aimHorizontalLine.enabled = true;
            aimHorizontalLine.positionCount = 2;
            aimHorizontalLine.SetPosition(0, new Vector3(iceLeftEdge, horizontalLineY, 0f));
            aimHorizontalLine.SetPosition(1, new Vector3(iceRightEdge, horizontalLineY, 0f));
            
            // Apply skill-based width
            aimHorizontalLine.startWidth = lineWidth;
            aimHorizontalLine.endWidth = lineWidth;
            
            // TEAM COLOR (shooting knob color)
            Color lineColor = shootKnob.GetComponent<SpriteRenderer>().color;
            aimHorizontalLine.startColor = lineColor;
            aimHorizontalLine.endColor = lineColor;
        }
        
        // CURL LINE: Shows turn and curl from vertical line to aim circle
        // ENHANCED with accuracy visualization (width, offset, gradient)
        if (aimCurlLine != null && aimCircle != null && curlLineVisible)
        {
            // Find the minimum Y value of the vertical line (closest to hack)
            float curlLineY = Mathf.Min(verticalLineTopY, verticalLineBottomY) + 0.5f;
            
            // Start at vertical line X, end at aim circle X
            float curlLineStartX = verticalLineX;
            float curlLineEndX = aimCircle.transform.position.x;
            
            // === ENHANCED CURL LINE WITH ACCURACY VISUALIZATION ===
            
            // Get shooter's weight accuracy (controls distance/weight error)
            float weightAccuracy = GetShooterWeightAccuracy(); // 0-100
            float weightRatio = Mathf.Clamp01(weightAccuracy / 100f);
            
            // Calculate weight error range (using AI formula from AI_Target.cs)
            // Low skill (0-40): baseMaxError = 0.99 (very hard - 99cm miss)
            // Mid skill (40-70): baseMaxError = ~0.50 (moderate - 50cm miss)
            // High skill (70-100): baseMaxError = 0.02 (easy - 2cm miss)
            float weightBaseMaxError = Mathf.Lerp(1.0f, 0.02f, weightRatio * weightRatio); // Quadratic scaling
            float weightMaxError = weightBaseMaxError * (1f - weightRatio);
            
            // MINIMUM WIDTH for visibility (requirement: 0.25 minimum)
            weightMaxError = Mathf.Max(weightMaxError, 0.25f);
            
            // === WIDTH: Reflects ACTUAL weight error range ===
            // Line width shows "rock could land anywhere in this vertical range"
            float curlLineWidth = weightMaxError * 2.0f; // Double to show ±error (both short and long)
            
            // === VERTICAL OFFSET: Short bias (30/70 - most shots go SHORT) ===
            // Offset curl line BELOW ideal trajectory by 40% of weight error
            float shortBias = -weightMaxError * 0.4f; // 40% below center line
            float curlLineYWithBias = curlLineY + shortBias;
            
            // === GRADIENT FADE: Skill-based transparency transition ===
            // High skill (70-100%): Sharp fade (fades at 70% of distance)
            // Medium skill (40-70%): Moderate fade (fades at 50% of distance)
            // Low skill (0-40%): Gradual fade (fades at 30% of distance)
            
            // Calculate where fade STARTS (as % along line from start to end)
            // High skill = later fade (0.7), Low skill = earlier fade (0.2)
            float fadeStartRatio = Mathf.Lerp(0.2f, 0.7f, weightRatio);
            
            // TEAM COLOR with gradient
            Color teamColor = shootKnob.GetComponent<SpriteRenderer>().color;
            
            // Apply to line renderer
            aimCurlLine.enabled = true;
            aimCurlLine.positionCount = 2;
            aimCurlLine.SetPosition(0, new Vector3(curlLineStartX, curlLineYWithBias, 0f));
            aimCurlLine.SetPosition(1, new Vector3(curlLineEndX, curlLineYWithBias, 0f));
            
            // Set WIDTH (skill-based - shows weight error range)
            aimCurlLine.startWidth = curlLineWidth;
            aimCurlLine.endWidth = curlLineWidth;
            
            // === GRADIENT WITH BIAS (skill-based fade point) ===
            // Unity LineRenderer.colorGradient allows us to set custom fade curves
            Gradient curlGradient = new Gradient();
            
            // Define gradient color keys (where colors change)
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            colorKeys[0] = new GradientColorKey(teamColor, 0f); // Start: full color
            colorKeys[1] = new GradientColorKey(teamColor, 1f); // End: same color (alpha does the fade)
            
            // Define gradient ALPHA keys (where transparency changes)
            // This is where we apply the SKILL-BASED FADE BIAS!
            // Three keys: opaque at start → hold opacity until fade point → transparent at end
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];
            alphaKeys[0] = new GradientAlphaKey(1.0f, 0f); // Start: fully opaque
            alphaKeys[1] = new GradientAlphaKey(1.0f, fadeStartRatio); // Hold opacity until fade point
            alphaKeys[2] = new GradientAlphaKey(0.05f, 1.0f); // Then fade to transparent at end
            
            curlGradient.SetKeys(colorKeys, alphaKeys);
            
            // Apply gradient to line renderer
            aimCurlLine.colorGradient = curlGradient;
            
            Debug.Log($"[Curl Line Enhanced] Weight skill: {weightAccuracy:F0}%, Error: ±{weightMaxError:F2}m\n" +
                      $"  Width: {curlLineWidth:F2} (shows weight error range)\n" +
                      $"  Y offset: {shortBias:F2} (30/70 short bias)\n" +
                      $"  Fade starts at: {fadeStartRatio:F0}% ({(weightAccuracy >= 70 ? "SHARP" : weightAccuracy >= 40 ? "MODERATE" : "GRADUAL")} fade)\n" +
                      $"  Curl: {curlLineStartX:F2} → {curlLineEndX:F2} (offset: {Mathf.Abs(curlLineEndX - curlLineStartX):F2})");
        }
        
        // === COLLISION WARNING INDICATOR ===
        // Show small dotted line at collision point if trajectory will hit a rock
        // This is SEPARATE from vertical guide line (which shows straight-line aim)
        UpdateCollisionWarningLine();
    }
    
    /// <summary>
    /// Get shooter's weight accuracy (Y-axis skill) from active character
    /// </summary>
    private float GetShooterWeightAccuracy()
    {
        // Check if this is the player's rock or AI rock
        CareerManager cm = FindFirstObjectByType<CareerManager>();
        TeamManager teamManager = FindFirstObjectByType<TeamManager>();
        
        if (cm != null && cm.cStats != null)
        {
            // Player rock - use WEIGHT accuracy (distance control skill)
            return cm.cStats.weightAccuracy;
        }
        else if (teamManager != null)
        {
            // AI rock - try to get current team member's weight skill
            // Determine which team member is shooting based on rock number
            if (gm != null && gm.rockList != null && gm.rockCurrent < gm.rockList.Count)
            {
                int memberIndex = gm.rockCurrent / 4; // 0-3 for lead, second, third, skip
                memberIndex = Mathf.Clamp(memberIndex, 0, 3);
                
                // Get the correct team
                bool isRedTeam = (gm.rockCurrent % 2 == 0) ? gm.redHammer : !gm.redHammer;
                
                if (isRedTeam && teamManager.teamRed != null && memberIndex < teamManager.teamRed.Length)
                {
                    return teamManager.teamRed[memberIndex].charStats.weightAccuracy.GetValue();
                }
                else if (!isRedTeam && teamManager.teamYellow != null && memberIndex < teamManager.teamYellow.Length)
                {
                    return teamManager.teamYellow[memberIndex].charStats.weightAccuracy.GetValue();
                }
            }
        }
        
        return 50f; // Default mid-skill
    }
    
    /// <summary>
    /// Draw collision warning indicator at collision point on trajectory
    /// Small dotted vertical line (0.5 units) showing where rock will hit obstacle
    /// Shown with guide lines (aim circle OFF), independent of collision visualization toggle
    /// </summary>
    private void UpdateCollisionWarningLine()
    {
        if (collisionWarningLine == null)
        {
            return;
        }
        
        // Only show if toggle is ON
        if (!collisionWarningVisible)
        {
            collisionWarningLine.enabled = false;
            return;
        }
        
        // Only show when aim circle is OFF (guide lines mode)
        if (aimCircleVisible)
        {
            collisionWarningLine.enabled = false;
            return;
        }
        
        // Check if trajectory simulator detected a collision
        if (trajectorySimulator == null)
        {
            collisionWarningLine.enabled = false;
            return;
        }
        
        TrajectorySimulator.CollisionInfo collisionInfo = trajectorySimulator.GetCollisionInfo();
        
        if (!collisionInfo.hasCollision)
        {
            collisionWarningLine.enabled = false;
            return;
        }
        
        // Draw small vertical line at collision point
        Vector2 collisionPoint = collisionInfo.collisionPoint;
        float indicatorHeight = 0.5f; // 0.5 units tall (subtle)
        
        // Center the indicator around collision point
        float topY = collisionPoint.y + (indicatorHeight / 2f);
        float bottomY = collisionPoint.y - (indicatorHeight / 2f);
        
        collisionWarningLine.enabled = true;
        collisionWarningLine.positionCount = 2;
        collisionWarningLine.SetPosition(0, new Vector3(collisionPoint.x, topY, 0f));
        collisionWarningLine.SetPosition(1, new Vector3(collisionPoint.x, bottomY, 0f));
        
        Debug.Log($"[Collision Warning] Indicator drawn at ({collisionPoint.x:F2}, {collisionPoint.y:F2}) - height: {indicatorHeight}");
    }
    
    /// <summary>
    /// Get shooter's aim accuracy (X-axis skill) from active character
    /// </summary>
    private float GetShooterAimAccuracy()
    {
        // Check if this is the player's rock or AI rock
        CareerManager cm = FindFirstObjectByType<CareerManager>();
        TeamManager teamManager = FindFirstObjectByType<TeamManager>();
        
        if (cm != null && cm.cStats != null)
        {
            // Player rock - use AIM accuracy (lateral positioning skill)
            return cm.cStats.aimAccuracy;
        }
        else if (teamManager != null)
        {
            // AI rock - try to get current team member's aim skill
            if (gm != null && gm.rockList != null && gm.rockCurrent < gm.rockList.Count)
            {
                int memberIndex = gm.rockCurrent / 4; // 0-3 for lead, second, third, skip
                memberIndex = Mathf.Clamp(memberIndex, 0, 3);
                
                // Get the correct team
                bool isRedTeam = (gm.rockCurrent % 2 == 0) ? gm.redHammer : !gm.redHammer;
                
                if (isRedTeam && teamManager.teamRed != null && memberIndex < teamManager.teamRed.Length)
                {
                    return teamManager.teamRed[memberIndex].charStats.aimAccuracy.GetValue();
                }
                else if (!isRedTeam && teamManager.teamYellow != null && memberIndex < teamManager.teamYellow.Length)
                {
                    return teamManager.teamYellow[memberIndex].charStats.aimAccuracy.GetValue();
                }
            }
        }
        
        return 50f; // Default mid-skill
    }
    
    /// <summary>
    /// Called when guidelines visibility setting changes (from UI toggle)
    /// </summary>
    private void OnGuidelinesVisibilityChanged(bool visible)
    {
        guidelinesVisible = visible;
        Debug.Log($"[TrajectoryLine] Guidelines visibility changed to: {visible}");
        
        // If currently showing trajectory, update visualization
        if (gm != null && gm.rockList != null && gm.rockCurrent < gm.rockList.Count)
        {
            GameObject currentRock = gm.rockList[gm.rockCurrent].rock;
            Rock_Info currentRockInfo = gm.rockList[gm.rockCurrent].rockInfo;
            
            // Only update if we're in aiming mode (not released yet)
            if (currentRock != null && currentRockInfo != null && !currentRockInfo.released)
            {
                // Update alternative aim visualization
                UpdateAlternativeAimVisualization();
            }
        }
    }
    
    /// <summary>
    /// Called when curl line visibility setting changes (from UI toggle)
    /// </summary>
    private void OnCurlLineVisibilityChanged(bool visible)
    {
        curlLineVisible = visible;
        Debug.Log($"[TrajectoryLine] Curl line visibility changed to: {visible}");
        
        // If currently showing trajectory, update visualization
        if (gm != null && gm.rockList != null && gm.rockCurrent < gm.rockList.Count)
        {
            GameObject currentRock = gm.rockList[gm.rockCurrent].rock;
            Rock_Info currentRockInfo = gm.rockList[gm.rockCurrent].rockInfo;
            
            // Only update if we're in aiming mode (not released yet)
            if (currentRock != null && currentRockInfo != null && !currentRockInfo.released)
            {
                // Update alternative aim visualization
                UpdateAlternativeAimVisualization();
            }
        }
    }
    
    /// <summary>
    /// Called when collision warning visibility setting changes (from UI toggle)
    /// </summary>
    private void OnCollisionWarningVisibilityChanged(bool visible)
    {
        collisionWarningVisible = visible;
        Debug.Log($"[TrajectoryLine] Collision warning visibility changed to: {visible}");
        
        // If currently showing trajectory, update visualization
        if (gm != null && gm.rockList != null && gm.rockCurrent < gm.rockList.Count)
        {
            GameObject currentRock = gm.rockList[gm.rockCurrent].rock;
            Rock_Info currentRockInfo = gm.rockList[gm.rockCurrent].rockInfo;
            
            // Only update if we're in aiming mode (not released yet)
            if (currentRock != null && currentRockInfo != null && !currentRockInfo.released)
            {
                // Update collision warning line
                UpdateCollisionWarningLine();
            }
        }
    }
    
    /// <summary>
    /// Cleanup subscriptions when destroyed
    /// </summary>
    void OnDestroy()
    {
        if (visualSettings != null)
        {
            visualSettings.OnTrajectoryVisibilityChanged -= OnTrajectoryVisibilityChanged;
            visualSettings.OnCollisionLinesVisibilityChanged -= OnCollisionLinesVisibilityChanged;
            visualSettings.OnAimCircleVisibilityChanged -= OnAimCircleVisibilityChanged;
            visualSettings.OnGuidelinesVisibilityChanged -= OnGuidelinesVisibilityChanged;
            visualSettings.OnCurlLineVisibilityChanged -= OnCurlLineVisibilityChanged;
            visualSettings.OnCollisionWarningVisibilityChanged -= OnCollisionWarningVisibilityChanged;
        }
    }
}