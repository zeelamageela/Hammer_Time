using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// TeeSweeperController - Manages T-line sweeping for opponent rocks behind Y=6.5
/// 
/// PLAYER FLOW:
/// 1. Player taps any opponent rock behind T-line (Y > 6.5)  
/// 2. T-line sweeper attaches and follows that rock
/// 3. Sweeper automatically sweeps the rock (tap = intent to sweep)
/// 4. Sweeper stays active until rock stops or leaves play
/// 5. Regular sweepers (L/R) continue to work independently
/// </summary>
public class TeeSweeperController : MonoBehaviour
{
    // References (using object to avoid assembly dependency issues)
    private object sm;
    private object rm;
    private object gm;
    private object sweep;
    private MonoBehaviour sweeperRedTee;
    private MonoBehaviour sweeperYellowTee;
    private AudioSource[] teeRockSounds;  // Separate audio for tee sweeper
    private GameObject sweepButton;
    private GameObject whoaButton;
    private Collider2D sweeperTeeCol;
    
    // State tracking
    private Rigidbody2D attachedRockRB;
    private GameObject attachedRockGO;
    private MonoBehaviour activeSweeper;
    private object activeSweeperStats;
    private bool isActive;
    private bool isSweeping;
    private float sweepTimeRemaining;
    private const float TEE_LINE_Y = 6.5f;
    private const float BACK_LINE_Y = 8.0f;  // Back line - sweeper deactivates here
    private const float VELOCITY_THRESHOLD = 0.01f;
    
    void Start()
    {
        Debug.Log("[TeeSweeperController] Start() called - staying active to detect taps");
        // DON'T disable GameObject - we need Update() to run for tap detection!
        // Instead, we just keep sweepers inactive until attached
    }
    
    public void Initialize(object smObj, object rmObj, object gmObj, object sweepObj, object redTee, object yellowTee, object audio, object sweepBtn, object whoaBtn)
    {
        sm = smObj;
        rm = rmObj;
        gm = gmObj;
        sweep = sweepObj;
        sweeperRedTee = redTee as MonoBehaviour;
        sweeperYellowTee = yellowTee as MonoBehaviour;
        teeRockSounds = audio as AudioSource[];  // Store reference but don't use it (avoid conflict with regular sweepers)
        sweepButton = sweepBtn as GameObject;
        whoaButton = whoaBtn as GameObject;
        
        // Hide both sweepers initially
        if (sweeperRedTee != null) sweeperRedTee.gameObject.SetActive(false);
        if (sweeperYellowTee != null) sweeperYellowTee.gameObject.SetActive(false);
        
        UpdateCollider();
        Debug.Log("[TeeSweeperController] Initialized - sweepers hidden until player taps rock");
    }
    
    public void UpdateCollider()
    {
        // Cache collider reference from both sweepers
        if (sweeperRedTee != null)
        {
            BoxCollider2D col = sweeperRedTee.GetComponent<BoxCollider2D>();
            if (col != null) sweeperTeeCol = col;
        }
        
        if (sweeperYellowTee != null)
        {
            BoxCollider2D col = sweeperYellowTee.GetComponent<BoxCollider2D>();
            if (col != null && sweeperTeeCol == null) sweeperTeeCol = col;
        }
    }
    
    public void ForceDetach()
    {
        DetachFromRock();
        
        // Make absolutely sure both sweepers are hidden
        if (sweeperRedTee != null) sweeperRedTee.gameObject.SetActive(false);
        if (sweeperYellowTee != null) sweeperYellowTee.gameObject.SetActive(false);
        
        Debug.Log("[TeeSweeperController] Force detach - all sweepers hidden");
    }
    
    void Update()
    {
        if (isActive && attachedRockRB != null)
        {
            UpdatePosition();
            UpdateRotation();
            CheckRockStatus();
            
            // ? NEW: Check if main sweepers stopped and tee sweeper needs to take over
            CheckMainSweeperHandoff();
            
            // ? NEW: INTELLIGENT AUTO-SWEEPING LOGIC
            // Tee line sweeping is ALWAYS automated (no player control over sweep/whoa decisions)
            // Players can tap to ATTACH sweeper, but sweeping decisions are AI-controlled
            EvaluateAndSweep();
            
            // ? REMOVED: endurance-based sweeping timeout
            // Tee sweepers sweep continuously until rock stops
            // No need to check sweepTimeRemaining
        }
        
        // ? NEW: AUTOMATIC ROCK DETECTION
        // Instead of waiting for player tap, automatically detect rocks crossing tee line
        AutoDetectAndAttach();
    }
    
    
    
    /// <summary>
    /// ? NEW: CHECK IF MAIN SWEEPERS STOPPED - TEE SWEEPER TAKEOVER
    /// 
    /// If rock crossed tee line with main sweepers active (they continued sweeping),
    /// but then main sweepers stop (whoa called), tee sweeper should take over!
    /// 
    /// This handles the handoff: Main sweepers ? Tee sweeper
    /// </summary>
    void CheckMainSweeperHandoff()
    {
        // Only check if tee sweeper is tracking but NOT visible (standby mode)
        if (activeSweeper == null || activeSweeper.gameObject.activeInHierarchy) return;
        
        // Check if main sweepers are still active
        bool mainSweepersSweeping = false;
        
        if (sm != null)
        {
            System.Type smType = sm.GetType();
            FieldInfo sweeperLField = smType.GetField("sweeperL");
            FieldInfo sweeperRField = smType.GetField("sweeperR");
            
            if (sweeperLField != null && sweeperRField != null)
            {
                MonoBehaviour sweeperL = sweeperLField.GetValue(sm) as MonoBehaviour;
                MonoBehaviour sweeperR = sweeperRField.GetValue(sm) as MonoBehaviour;
                
                // Check if either sweeper is actively sweeping
                if (sweeperL != null && sweeperL.gameObject.activeInHierarchy)
                {
                    Component sweepComp = sweeperL.GetComponent("Sweep");
                    if (sweepComp != null)
                    {
                        FieldInfo sweepingField = sweepComp.GetType().GetField("isSweeping");
                        if (sweepingField != null)
                        {
                            mainSweepersSweeping = (bool)sweepingField.GetValue(sweepComp);
                        }
                    }
                }
                
                if (!mainSweepersSweeping && sweeperR != null && sweeperR.gameObject.activeInHierarchy)
                {
                    Component sweepComp = sweeperR.GetComponent("Sweep");
                    if (sweepComp != null)
                    {
                        FieldInfo sweepingField = sweepComp.GetType().GetField("isSweeping");
                        if (sweepingField != null)
                        {
                            mainSweepersSweeping = (bool)sweepingField.GetValue(sweepComp);
                        }
                    }
                }
            }
        }
        
        // Main sweepers stopped? Activate tee sweeper!
        if (!mainSweepersSweeping)
        {
            Debug.Log($"[TeeSweeperController] HANDOFF: Main sweepers STOPPED - activating tee sweeper");
            
            // Deactivate main sweepers completely
            if (sm != null)
            {
                System.Type smType = sm.GetType();
                FieldInfo sweeperLField = smType.GetField("sweeperL");
                FieldInfo sweeperRField = smType.GetField("sweeperR");
                
                if (sweeperLField != null)
                {
                    MonoBehaviour sweeperL = sweeperLField.GetValue(sm) as MonoBehaviour;
                    if (sweeperL != null && sweeperL.gameObject.activeInHierarchy)
                    {
                        sweeperL.gameObject.SetActive(false);
                    }
                }
                
                if (sweeperRField != null)
                {
                    MonoBehaviour sweeperR = sweeperRField.GetValue(sm) as MonoBehaviour;
                    if (sweeperR != null && sweeperR.gameObject.activeInHierarchy)
                    {
                        sweeperR.gameObject.SetActive(false);
                    }
                }
            }
            
            // Activate tee sweeper
            activeSweeper.gameObject.SetActive(true);
            
            // ? NEW: TEXT CALLOUT FOR HANDOFF
            if (attachedRockGO != null)
            {
                Component rockInfo = attachedRockGO.GetComponent("Rock_Info");
                if (rockInfo != null)
                {
                    FieldInfo teamNameField = rockInfo.GetType().GetField("teamName");
                    if (teamNameField != null)
                    {
                        string rockTeamName = teamNameField.GetValue(rockInfo) as string;
                        
                        Component gspComp = FindFirstObjectByType(Type.GetType("GameSettingsPersist")) as Component;
                        if (gspComp != null)
                        {
                            FieldInfo redTeamNameField = gspComp.GetType().GetField("redTeamName");
                            if (redTeamNameField != null)
                            {
                                string redTeamName = redTeamNameField.GetValue(gspComp) as string;
                                bool isRedRock = (rockTeamName == redTeamName);
                                
                                string sweepingTeamName = isRedRock ? "Yellow" : "Red";
                                string calloutMessage = $"{sweepingTeamName} is sweeping behind T-Line";
                                
                                // Show callout
                                Component textCalloutManager = FindFirstObjectByType(Type.GetType("TextCalloutManager")) as Component;
                                if (textCalloutManager != null)
                                {
                                    Vector3 rockPos = attachedRockGO.transform.position;
                                    Vector3 calloutPos = rockPos + new Vector3(0f, 1.0f, 0f);
                                    
                                    System.Type calloutType = textCalloutManager.GetType();
                                    MethodInfo showCalloutMethod = calloutType.GetMethod("ShowCallout", 
                                        new Type[] { typeof(Vector3), typeof(string), typeof(bool), typeof(Transform), typeof(float) });
                                    
                                    if (showCalloutMethod != null)
                                    {
                                        showCalloutMethod.Invoke(textCalloutManager, new object[] { 
                                            calloutPos, calloutMessage, false, null, 3.0f 
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            Debug.Log($"[TeeSweeperController] TEE SWEEPER NOW VISIBLE (handoff): {activeSweeper.name}");
            
            // Start sweeping with tee sweeper
            StartSweeping();
        }
    }
    
    /// <summary>
    /// ? NEW: INTELLIGENT TEE LINE SWEEPING LOGIC
    /// 
    /// Automatically decides whether to SWEEP or WHOA based on:
    /// 1. Rock ownership (friendly vs opponent)
    /// 2. Rock trajectory (moving toward vs away from scoring)
    /// 
    /// LOGIC:
    /// - FRIENDLY rock moving TOWARD scoring ? SWEEP (help it reach)
    /// - FRIENDLY rock moving AWAY from scoring ? WHOA (don't make it worse)
    /// - OPPONENT rock moving TOWARD scoring ? WHOA (don't help them score)
    /// - OPPONENT rock moving AWAY from scoring ? SWEEP (push it further away!)
    /// 
    /// This is FULLY AUTOMATED - players have no control over sweep/whoa decisions.
    /// Players can only TAP to attach sweeper to a rock.
    /// </summary>
    void EvaluateAndSweep()
    {
        if (attachedRockRB == null || attachedRockGO == null) return;
        
        // Get rock info to determine ownership
        Component rockInfo = attachedRockGO.GetComponent("Rock_Info");
        if (rockInfo == null) return;
        
        FieldInfo teamNameField = rockInfo.GetType().GetField("teamName");
        if (teamNameField == null) return;
        
        string rockTeamName = teamNameField.GetValue(rockInfo) as string;
        
        // Get current team name to determine if rock is friendly or opponent
        Component gspComp = FindFirstObjectByType(Type.GetType("GameSettingsPersist")) as Component;
        if (gspComp == null) return;
        
        FieldInfo redTeamNameField = gspComp.GetType().GetField("redTeamName");
        FieldInfo redHammerField = gm.GetType().GetField("redHammer");
        FieldInfo rockCurrentField = gm.GetType().GetField("rockCurrent");
        
        if (redTeamNameField == null || redHammerField == null || rockCurrentField == null) return;
        
        string redTeamName = redTeamNameField.GetValue(gspComp) as string;
        bool redHammer = (bool)redHammerField.GetValue(gm);
        int rockCurrent = (int)rockCurrentField.GetValue(gm);
        
        // Determine current team (team that just threw this rock)
        bool isRedTeamTurn = (rockCurrent % 2 == 0) ? redHammer : !redHammer;
        string currentTeamName = isRedTeamTurn ? redTeamName : ((MonoBehaviour)gm).GetType().GetField("yellowTeamName")?.GetValue(gm) as string;
        
        // Is this rock friendly (our team) or opponent?
        bool isFriendlyRock = (rockTeamName == currentTeamName);
        
        // ========================================
        // TRAJECTORY ANALYSIS: Moving toward or away from scoring?
        // ========================================
        Vector2 button = new Vector2(0f, 6.5f);
        Vector2 currentPos = attachedRockGO.transform.position;
        Vector2 velocity = attachedRockRB.linearVelocity;
        
        // Calculate direction to button from current position
        Vector2 toButton = button - currentPos;
        float distToButton = toButton.magnitude;
        
        // Dot product: positive = moving toward button, negative = moving away
        float dotProduct = Vector2.Dot(velocity.normalized, toButton.normalized);
        
        bool movingTowardButton = dotProduct > 0.3f; // At least ~70° toward button (cos 70° ? 0.3)
        bool inScoringZone = (currentPos.y >= 5.0f && currentPos.y <= 9.0f); // In the house
        
        // Check if rock is moving AWAY from scoring (backward or out sideways)
        bool movingBackward = velocity.y < 0.1f; // Moving backward or barely forward
        bool movingOutSideways = !inScoringZone && Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y); // Outside house, moving more sideways than forward
        
        bool movingAwayFromScoring = movingBackward || movingOutSideways;
        bool movingTowardScoring = movingTowardButton && inScoringZone && !movingAwayFromScoring;
        
        // ========================================
        // DECISION LOGIC: SWEEP or WHOA?
        // ========================================
        bool shouldSweep = false;
        string reason = "";
        
        if (isFriendlyRock)
        {
            // FRIENDLY ROCK LOGIC
            if (movingTowardScoring)
            {
                shouldSweep = true;
                reason = $"FRIENDLY rock moving TOWARD scoring (dot={dotProduct:F2}, Y={currentPos.y:F2}) ? SWEEP!";
            }
            else
            {
                shouldSweep = false;
                reason = $"FRIENDLY rock moving AWAY from scoring (dot={dotProduct:F2}, Y vel={velocity.y:F2}) ? WHOA!";
            }
        }
        else
        {
            // OPPONENT ROCK LOGIC (OPPOSITE!)
            if (movingAwayFromScoring || !movingTowardButton)
            {
                shouldSweep = true;
                reason = $"OPPONENT rock moving AWAY from scoring (dot={dotProduct:F2}) ? SWEEP! (push it further!)";
            }
            else
            {
                shouldSweep = false;
                reason = $"OPPONENT rock moving TOWARD scoring (dot={dotProduct:F2}, Y={currentPos.y:F2}) ? WHOA! (don't help them!)";
            }
        }
        
        // ========================================
        // APPLY DECISION: Start or stop sweeping
        // ========================================
        if (shouldSweep && !isSweeping)
        {
            // Should be sweeping but isn't - start sweeping!
            Debug.Log($"[TeeSweeperController] AUTO-SWEEP: {reason}");
            StartSweeping();
        }
        else if (!shouldSweep && isSweeping)
        {
            // Should NOT be sweeping but is - stop sweeping!
            Debug.Log($"[TeeSweeperController] AUTO-WHOA: {reason}");
            StopSweeping(false);
        }
        
        // Log evaluation every 0.5 seconds to avoid spam
        if (Time.frameCount % 30 == 0) // Every ~0.5s at 60fps
        {
            Debug.Log($"[TeeSweeperController] Evaluation: {(isFriendlyRock ? "FRIENDLY" : "OPPONENT")} rock, " +
                      $"MovingToward={movingTowardScoring}, MovingAway={movingAwayFromScoring}, " +
                      $"Sweeping={isSweeping}, Decision: {reason}");
        }
    }
    
    
    /// <summary>
    /// ? NEW: AUTOMATIC ROCK DETECTION AND ATTACHMENT
    /// 
    /// Continuously scans for rocks crossing the tee line (Y > 6.5)
    /// Automatically attaches sweeper when eligible rock detected
    /// NO PLAYER INPUT REQUIRED - fully automated!
    /// </summary>
    void AutoDetectAndAttach()
    {
        // If already attached to a rock, don't scan for new ones
        if (isActive && attachedRockRB != null) return;
        
        // Get GameManager to access rock list
        if (gm == null) return;
        
        // Access rockList via reflection
        System.Type gmType = gm.GetType();
        FieldInfo rockListField = gmType.GetField("rockList");
        if (rockListField == null) return;
        
        var rockList = rockListField.GetValue(gm) as System.Collections.IList;
        if (rockList == null) return;
        
        // Scan all rocks for one that just crossed tee line
        foreach (var rockEntry in rockList)
        {
            // Get rock GameObject from rockEntry
            System.Type entryType = rockEntry.GetType();
            FieldInfo rockField = entryType.GetField("rock");
            if (rockField == null) continue;
            
            GameObject rock = rockField.GetValue(rockEntry) as GameObject;
            if (rock == null || !rock.activeInHierarchy) continue;
            
            // Check if rock is eligible for tee sweeping
            if (IsEligibleForTeeSweep(rock))
            {
                // Check if rock JUST crossed tee line (Y between 6.5 and 6.8)
                // This prevents attaching to rocks already in the house
                float rockY = rock.transform.position.y;
                
                if (rockY > TEE_LINE_Y && rockY < TEE_LINE_Y + 0.3f)
                {
                    // Found an eligible rock that just crossed tee line!
                    Debug.Log($"[TeeSweeperController] AUTO-ATTACH: Rock {rock.name} crossed tee line at Y={rockY:F2}");
                    AttachToRock(rock);
                    return; // Only attach to one rock at a time
                }
            }
        }
    }
    
    /// <summary>
    /// LEGACY: Manual tap detection (DISABLED by default)
    /// Kept for backward compatibility if manual control is ever needed
    /// Set enableManualTapControl = true in Inspector to re-enable
    /// </summary>
    public bool enableManualTapControl = false; // DISABLED: Fully automatic now!
    
    void DetectRockTaps()
    {
        // ? AUTOMATIC MODE: Tap detection disabled (tee sweepers attach automatically)
        if (!enableManualTapControl) return;
        
        // LEGACY CODE BELOW (only runs if enableManualTapControl = true)
        
        // CRITICAL: Don't interfere with flick shot mode!
        // Check if flick shot mode is active using reflection
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
                        bool flickShotMode = (bool)flickModeProp.GetValue(visualSettings);
                        if (flickShotMode)
                        {
                            // Flick shot mode is active - don't process rock taps
                            return;
                        }
                    }
                }
            }
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[TeeSweeperController] Mouse click detected!");
            
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            
            Debug.Log($"[TeeSweeperController] Mouse position: {mousePos2D}");
            
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            
            if (hit.collider != null)
            {
                Debug.Log($"[TeeSweeperController] Hit: {hit.collider.gameObject.name}, Layer: {hit.collider.gameObject.layer}");
                
                if (hit.collider.gameObject.layer == 3)
                {
                    GameObject clickedRock = hit.collider.gameObject;
                    Debug.Log($"[TeeSweeperController] Rock clicked: {clickedRock.name}");
                    
                    if (IsEligibleForTeeSweep(clickedRock))
                    {
                        Debug.Log($"[TeeSweeperController] Rock eligible - attempting attach");
                        AttachToRock(clickedRock);
                    }
                    else
                    {
                        Debug.Log($"[TeeSweeperController] Rock NOT eligible - Y={clickedRock.transform.position.y}");
                    }
                }
                else
                {
                    Debug.Log($"[TeeSweeperController] Hit non-rock object (layer {hit.collider.gameObject.layer})");
                }
            }
            else
            {
                Debug.Log("[TeeSweeperController] Raycast hit nothing");
            }
        }
    }
    
    /// <summary>
    /// Check if a rock is eligible for T-line sweeping
    /// ANY moving rock past T-line can be swept (own team OR opponent)
    /// In curling, you can sweep any rock behind the T-line
    /// </summary>
    bool IsEligibleForTeeSweep(GameObject rock)
    {
        if (rock == null)
        {
            Debug.Log("[TeeSweeperController] Eligibility: Rock is NULL");
            return false;
        }
        
        Component rockInfo = rock.GetComponent("Rock_Info");
        if (rockInfo == null)
        {
            Debug.Log("[TeeSweeperController] Eligibility: Rock_Info component not found");
            return false;
        }
        
        FieldInfo movingField = rockInfo.GetType().GetField("moving");
        if (movingField == null)
        {
            Debug.Log("[TeeSweeperController] Eligibility: 'moving' field not found");
            return false;
        }
        
        bool isMoving = (bool)movingField.GetValue(rockInfo);
        //Debug.Log($"[TeeSweeperController] Eligibility: Rock.moving = {isMoving}");
        
        if (!isMoving) return false;
        
        float rockY = rock.transform.position.y;
        //Debug.Log($"[TeeSweeperController] Eligibility: Rock Y = {rockY}, T-line = {TEE_LINE_Y}");
        
        // Must be behind T-line (Y > 6.5)
        if (rockY <= TEE_LINE_Y)
        {
            Debug.Log($"[TeeSweeperController] Eligibility: FAILED - Rock not past T-line");
            return false;
        }
        
        // ANY rock past T-line is eligible!
        Debug.Log("[TeeSweeperController] Eligibility: PASSED - Rock is eligible!");
        return true;
    }
    
    void AttachToRock(GameObject rock)
    {
        Component gspComp = FindFirstObjectByType(Type.GetType("GameSettingsPersist")) as Component;
        Component rockInfo = rock.GetComponent("Rock_Info");
        
        if (rockInfo == null || gspComp == null) return;
        
        FieldInfo teamNameField = rockInfo.GetType().GetField("teamName");
        FieldInfo redTeamNameField = gspComp.GetType().GetField("redTeamName");
        
        if (teamNameField == null || redTeamNameField == null) return;
        
        string rockTeamName = teamNameField.GetValue(rockInfo) as string;
        string redTeamName = redTeamNameField.GetValue(gspComp) as string;
        
        bool isRedRock = (rockTeamName == redTeamName);
        
        activeSweeper = isRedRock ? sweeperYellowTee : sweeperRedTee;
        if (activeSweeper == null)
        {
            //Debug.LogError("[TeeSweeperController] Active sweeper is NULL!");
            return;
        }
        
        activeSweeperStats = activeSweeper.GetComponent("CharacterStats");
        attachedRockRB = rock.GetComponent<Rigidbody2D>();
        attachedRockGO = rock;
        
        // ? NEW: SWEEPER HANDOFF LOGIC
        // Check if main sweepers (L/R) are currently sweeping this rock
        bool mainSweepersSweeping = false;
        
        if (sm != null)
        {
            // Check if SweeperManager has active sweeping
            System.Type smType = sm.GetType();
            FieldInfo sweeperLField = smType.GetField("sweeperL");
            FieldInfo sweeperRField = smType.GetField("sweeperR");
            
            if (sweeperLField != null && sweeperRField != null)
            {
                MonoBehaviour sweeperL = sweeperLField.GetValue(sm) as MonoBehaviour;
                MonoBehaviour sweeperR = sweeperRField.GetValue(sm) as MonoBehaviour;
                
                // Check if either sweeper is actively sweeping
                if (sweeperL != null && sweeperL.gameObject.activeInHierarchy)
                {
                    // Check if sweeper is in "sweeping" state (has Sweep component with isSweeping flag)
                    Component sweepComp = sweeperL.GetComponent("Sweep");
                    if (sweepComp != null)
                    {
                        FieldInfo sweepingField = sweepComp.GetType().GetField("isSweeping");
                        if (sweepingField != null)
                        {
                            mainSweepersSweeping = (bool)sweepingField.GetValue(sweepComp);
                        }
                    }
                }
                
                if (!mainSweepersSweeping && sweeperR != null && sweeperR.gameObject.activeInHierarchy)
                {
                    Component sweepComp = sweeperR.GetComponent("Sweep");
                    if (sweepComp != null)
                    {
                        FieldInfo sweepingField = sweepComp.GetType().GetField("isSweeping");
                        if (sweepingField != null)
                        {
                            mainSweepersSweeping = (bool)sweepingField.GetValue(sweepComp);
                        }
                    }
                }
            }
        }
        
        if (mainSweepersSweeping)
        {
            // Main sweepers are active - let them continue!
            Debug.Log($"[TeeSweeperController] Main sweepers are ACTIVE - keeping them, tee sweeper on standby");
            
            // Don't activate tee sweeper yet - main sweepers have priority
            // Tee sweeper will activate only if main sweepers stop
            isActive = true; // Track rock but don't show tee sweeper yet
            activeSweeper.gameObject.SetActive(false); // Keep hidden
            
            return; // Exit without starting tee sweeper
        }
        else
        {
            // Main sweepers are NOT active - remove them and activate tee sweeper
            Debug.Log($"[TeeSweeperController] Main sweepers INACTIVE - removing them, activating tee sweeper");
            
            // Deactivate main sweepers (they're not sweeping anyway)
            if (sm != null)
            {
                System.Type smType = sm.GetType();
                FieldInfo sweeperLField = smType.GetField("sweeperL");
                FieldInfo sweeperRField = smType.GetField("sweeperR");
                
                if (sweeperLField != null)
                {
                    MonoBehaviour sweeperL = sweeperLField.GetValue(sm) as MonoBehaviour;
                    if (sweeperL != null && sweeperL.gameObject.activeInHierarchy)
                    {
                        sweeperL.gameObject.SetActive(false);
                        Debug.Log($"[TeeSweeperController] Deactivated left main sweeper");
                    }
                }
                
                if (sweeperRField != null)
                {
                    MonoBehaviour sweeperR = sweeperRField.GetValue(sm) as MonoBehaviour;
                    if (sweeperR != null && sweeperR.gameObject.activeInHierarchy)
                    {
                        sweeperR.gameObject.SetActive(false);
                        Debug.Log($"[TeeSweeperController] Deactivated right main sweeper");
                    }
                }
            }
            
            // Activate tee sweeper
            activeSweeper.gameObject.SetActive(true);
            isActive = true;
            
            Debug.Log($"[TeeSweeperController] TEE SWEEPER NOW VISIBLE: {activeSweeper.name}");
            
            // ? NEW: TEXT CALLOUT
            // Show callout: "{Team Name} is sweeping behind T-Line"
            string sweepingTeamName = isRedRock ? "Yellow" : "Red"; // Opposite team sweeps
            string calloutMessage = $"{sweepingTeamName} is sweeping behind T-Line";
            
            // Find TextCalloutManager and show callout
            Component textCalloutManager = FindFirstObjectByType(Type.GetType("TextCalloutManager")) as Component;
            if (textCalloutManager != null)
            {
                // Get rock position for callout location
                Vector3 rockPos = rock.transform.position;
                Vector3 calloutPos = rockPos + new Vector3(0f, 1.0f, 0f); // 1 unit above rock
                
                // Call ShowCallout method via reflection
                System.Type calloutType = textCalloutManager.GetType();
                MethodInfo showCalloutMethod = calloutType.GetMethod("ShowCallout", 
                    new Type[] { typeof(Vector3), typeof(string), typeof(bool), typeof(Transform), typeof(float) });
                
                if (showCalloutMethod != null)
                {
                    showCalloutMethod.Invoke(textCalloutManager, new object[] { 
                        calloutPos,      // position
                        calloutMessage,  // message
                        false,           // followTarget (false - fixed position)
                        null,            // target (null - no follow)
                        3.0f             // duration (3 seconds)
                    });
                    
                    Debug.Log($"[TeeSweeperController] TEXT CALLOUT: '{calloutMessage}' at {calloutPos}");
                }
            }
            
            StartSweeping();
            
            Debug.Log($"[TeeSweeperController] Attached - {(isRedRock ? "Yellow" : "Red")} sweeping rock {rock.name}");
        }
    }
    
    void StartSweeping()
    {
        if (isSweeping || activeSweeper == null || activeSweeperStats == null) return;
        
        // ? FIX: Set sweepTimeRemaining to VERY HIGH value for continuous sweeping
        // Tee sweepers should sweep until rock stops, not based on endurance timer
        sweepTimeRemaining = 999999f; // Effectively infinite
        
        isSweeping = true;
        
        // DON'T USE AUDIO - regular sweepers are using it
        // T-line sweeping is silent to avoid audio conflicts
        // Visual animation is enough feedback
        
        activeSweeper.SendMessage("Sweep", SendMessageOptions.DontRequireReceiver);
        if (sweep != null) ((MonoBehaviour)sweep).SendMessage("OnSweep", SendMessageOptions.DontRequireReceiver);
        if (sm != null) ((MonoBehaviour)sm).SendMessage("CallOut", "Sweep", SendMessageOptions.DontRequireReceiver);
        
        if (sweepButton != null) sweepButton.SetActive(false);
        if (whoaButton != null) whoaButton.SetActive(true);
        
        Debug.Log($"[TeeSweeperController] Started sweeping - continuous until rock stops");
    }
    
    void StopSweeping(bool playerCalled)
    {
        if (!isSweeping) return;
        
        isSweeping = false;
        
        // No audio to disable - we're running silent to avoid conflicts
        
        if (activeSweeper != null) activeSweeper.SendMessage("Whoa", SendMessageOptions.DontRequireReceiver);
        if (sweep != null) ((MonoBehaviour)sweep).SendMessage("OnWhoa", SendMessageOptions.DontRequireReceiver);
        if (playerCalled && sm != null) ((MonoBehaviour)sm).SendMessage("CallOut", "Whoa", SendMessageOptions.DontRequireReceiver);
        
        if (whoaButton != null) whoaButton.SetActive(false);
        if (sweepButton != null) sweepButton.SetActive(true);
        
        Debug.Log("[TeeSweeperController] Stopped sweeping");
    }
    
    void DetachFromRock()
    {
        if (isSweeping) StopSweeping(false);
        
        // Hide the active sweeper - THIS MAKES IT INVISIBLE!
        if (activeSweeper != null)
        {
            activeSweeper.gameObject.SetActive(false);
            Debug.Log($"[TeeSweeperController] SWEEPER NOW HIDDEN: {activeSweeper.name}");
        }
        
        if (sweepButton != null) sweepButton.SetActive(false);
        if (whoaButton != null) whoaButton.SetActive(false);
        
        attachedRockRB = null;
        attachedRockGO = null;
        activeSweeper = null;
        activeSweeperStats = null;
        isActive = false;
        
        // DON'T disable GameObject - we need Update() to keep running for tap detection!
        // gameObject.SetActive(false);  ? This was the problem!
        
        Debug.Log("[TeeSweeperController] Detached from rock - ready for next tap");
    }
    
    void UpdatePosition()
    {
        if (attachedRockRB == null) return;
        transform.position = new Vector3(attachedRockRB.position.x, attachedRockRB.position.y, 0f);
    }
    
    void UpdateRotation()
    {
        if (attachedRockRB == null) return;
        
        Vector2 velocity = attachedRockRB.linearVelocity;
        
        if (velocity != Vector2.zero && (Mathf.Abs(velocity.x) > 0.02f || Mathf.Abs(velocity.y) > 0.005f))
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
            
            if (activeSweeper != null)
            {
                FieldInfo yOffsetField = activeSweeper.GetType().GetField("yOffset");
                if (yOffsetField != null) yOffsetField.SetValue(activeSweeper, 0.6f);
            }
        }
    }
    
    void CheckRockStatus()
    {
        if (attachedRockRB == null || attachedRockGO == null)
        {
            DetachFromRock();
            return;
        }
        
        float velocity = attachedRockRB.linearVelocity.magnitude;
        if (velocity < VELOCITY_THRESHOLD)
        {
            Debug.Log($"[TeeSweeperController] Rock stopped - detaching");
            DetachFromRock();
            return;
        }
        
        Component rockInfo = attachedRockGO.GetComponent("Rock_Info");
        if (rockInfo != null)
        {
            FieldInfo movingField = rockInfo.GetType().GetField("moving");
            if (movingField != null && !(bool)movingField.GetValue(rockInfo))
            {
                Debug.Log("[TeeSweeperController] Rock no longer in play - detaching");
                DetachFromRock();
                return;
            }
        }
        
        Vector3 rockPos = attachedRockGO.transform.position;
        
        // Sweeper zone: Between T-line (6.5) and back line (8.0)
        if (Mathf.Abs(rockPos.x) > 3f || rockPos.y > BACK_LINE_Y || rockPos.y < TEE_LINE_Y)
        {
            Debug.Log($"[TeeSweeperController] Rock out of zone (Y={rockPos.y:F2}, zone={TEE_LINE_Y}-{BACK_LINE_Y}) - detaching");
            DetachFromRock();
        }
    }
    
    public void OnWhoaButton()
    {
        StopSweeping(true);
    }
    
    public void OnSweepButton()
    {
        StartSweeping();
    }
    
    /// <summary>
    /// Disable colliders on tee sweepers (for AI sweeping - they don't use tap detection)
    /// </summary>
    public void DisableColliders()
    {
        if (sweeperTeeCol != null)
        {
            sweeperTeeCol.enabled = false;
        }
        
        // Also disable colliders on both sweeper GameObjects
        if (sweeperRedTee != null)
        {
            BoxCollider2D col = sweeperRedTee.GetComponent<BoxCollider2D>();
            if (col != null) col.enabled = false;
        }
        
        if (sweeperYellowTee != null)
        {
            BoxCollider2D col = sweeperYellowTee.GetComponent<BoxCollider2D>();
            if (col != null) col.enabled = false;
        }
        
        Debug.Log("[TeeSweeperController] Colliders disabled for AI sweeping");
    }
    
    /// <summary>
    /// Enable colliders on tee sweepers (for player sweeping - needs tap detection)
    /// </summary>
    public void EnableColliders()
    {
        if (sweeperTeeCol != null)
        {
            sweeperTeeCol.enabled = true;
        }
        
        // Also enable colliders on both sweeper GameObjects
        if (sweeperRedTee != null)
        {
            BoxCollider2D col = sweeperRedTee.GetComponent<BoxCollider2D>();
            if (col != null) col.enabled = true;
        }
        
        if (sweeperYellowTee != null)
        {
            BoxCollider2D col = sweeperYellowTee.GetComponent<BoxCollider2D>();
            if (col != null) col.enabled = true;
        }
        
        Debug.Log("[TeeSweeperController] Colliders enabled for player sweeping");
    }
}