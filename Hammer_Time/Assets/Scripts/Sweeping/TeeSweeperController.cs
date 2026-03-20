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
            
            if (isSweeping)
            {
                sweepTimeRemaining -= Time.deltaTime;
                if (sweepTimeRemaining <= 0)
                {
                    StopSweeping(false);
                }
            }
        }
        
        DetectRockTaps();
    }
    
    void DetectRockTaps()
    {
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
        Debug.Log($"[TeeSweeperController] Eligibility: Rock.moving = {isMoving}");
        
        if (!isMoving) return false;
        
        float rockY = rock.transform.position.y;
        Debug.Log($"[TeeSweeperController] Eligibility: Rock Y = {rockY}, T-line = {TEE_LINE_Y}");
        
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
            Debug.LogError("[TeeSweeperController] Active sweeper is NULL!");
            return;
        }
        
        activeSweeperStats = activeSweeper.GetComponent("CharacterStats");
        attachedRockRB = rock.GetComponent<Rigidbody2D>();
        attachedRockGO = rock;
        
        // GameObject is already active (doesn't disable anymore)
        // Just activate the specific sweeper - THIS MAKES IT VISIBLE!
        activeSweeper.gameObject.SetActive(true);
        isActive = true;
        
        Debug.Log($"[TeeSweeperController] SWEEPER NOW VISIBLE: {activeSweeper.name}");
        
        StartSweeping();
        
        Debug.Log($"[TeeSweeperController] Attached - {(isRedRock ? "Yellow" : "Red")} sweeping rock {rock.name}");
    }
    
    void StartSweeping()
    {
        if (isSweeping || activeSweeper == null || activeSweeperStats == null) return;
        
        PropertyInfo enduranceProp = activeSweeperStats.GetType().GetProperty("sweepEndurance");
        if (enduranceProp != null)
        {
            object statObj = enduranceProp.GetValue(activeSweeperStats);
            MethodInfo getValueMethod = statObj.GetType().GetMethod("GetValue");
            if (getValueMethod != null)
            {
                sweepTimeRemaining = (float)getValueMethod.Invoke(statObj, null) * 0.02f;
            }
        }
        
        isSweeping = true;
        
        // DON'T USE AUDIO - regular sweepers are using it
        // T-line sweeping is silent to avoid audio conflicts
        // Visual animation is enough feedback
        
        activeSweeper.SendMessage("Sweep", SendMessageOptions.DontRequireReceiver);
        if (sweep != null) ((MonoBehaviour)sweep).SendMessage("OnSweep", SendMessageOptions.DontRequireReceiver);
        if (sm != null) ((MonoBehaviour)sm).SendMessage("CallOut", "Sweep", SendMessageOptions.DontRequireReceiver);
        
        if (sweepButton != null) sweepButton.SetActive(false);
        if (whoaButton != null) whoaButton.SetActive(true);
        
        Debug.Log($"[TeeSweeperController] Started sweeping - {sweepTimeRemaining:F2}s (silent mode to avoid audio conflicts)");
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