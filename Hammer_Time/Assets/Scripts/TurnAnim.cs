using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnAnim : MonoBehaviour
{
    public Animator anim;
    public GameManager gm;
    public RockManager rm;

    public Camera uiCam;
    public Collider2D col;
    GameObject rock;
    Rock_Force rockForce;
    //bool isPressed = false;
    public bool turnAI;
    AudioManager am;

    void Start()
    {
        //anim = GetComponent<Animator>();

        am = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();

    }

    private void OnEnable()
    {
        SetTurn(rm.inturn);
        //Debug.Log("rm.inturn");
    }

    /// <summary>
    /// Public method to toggle the turn - can be called by UI Button OnClick event
    /// </summary>
    public void ToggleTurn()
    {
        if (gm.rockList.Count != 0 && gm.rockCurrent < gm.rockList.Count)
        {
            am.Play("Button");
            
            // CRITICAL FIX: Toggle BOTH rm.inturn AND animator immediately (synchronously)
            // This ensures trajectory reads the correct value during drag
            rm.inturn = !rm.inturn;
            
            // Update animator IMMEDIATELY (not in coroutine)
            if (rm.inturn)
            {
                anim.SetBool("inturn", true);
            }
            else
            {
                anim.SetBool("inturn", false);
            }
            
            // Also update the rock's flipAxis immediately
            rock = gm.rockList[gm.rockCurrent].rock;
            Rock_Force rockForce = rock.GetComponent<Rock_Force>();
            if (rockForce != null)
            {
                rockForce.flipAxis = rm.inturn;
                Debug.Log($"?? [TurnAnim] Updated flipAxis = {rm.inturn} ({(rm.inturn ? "IN-TURN (LEFT)" : "OUT-TURN (RIGHT)")})");
            }
            
            // CRITICAL: Force trajectory redraw so player sees immediate visual feedback
            // ALWAYS redraw when toggle is clicked, regardless of drag state!
            // The trajectory line persists even when not dragging, so we need to update it
            TrajectoryLine trajLine = FindObjectOfType<TrajectoryLine>();
            if (trajLine != null)
            {
                Debug.Log($"?? [TurnAnim] Forcing trajectory redraw NOW (toggle clicked)");
                trajLine.DrawTrajectory();
                Debug.Log($"?? [TurnAnim] Trajectory redraw COMPLETE");
            }
            else
            {
                Debug.LogWarning($"?? [TurnAnim] Cannot redraw trajectory - trajLine is NULL!");
            }
            
            // Start coroutine ONLY for collider enable/disable timing (animation protection)
            StartCoroutine(ToggleColliderDelay());
            
            Debug.Log($"[TurnAnim] Turn toggled IMMEDIATELY - rm.inturn={rm.inturn}, flipAxis={rockForce?.flipAxis}, animator={rm.inturn}");
        }
    }
    
    /// <summary>
    /// Helper coroutine to prevent double-clicks during animation
    /// </summary>
    private IEnumerator ToggleColliderDelay()
    {
        col.enabled = false;
        yield return new WaitForSeconds(0.25f);
        col.enabled = true;
    }
    
    void Update()
    {
        // CRITICAL FIX: Check for negative rockCurrent (before first rock)
        if (gm.rockList.Count != 0 && gm.rockCurrent >= 0 && gm.rockCurrent < gm.rockList.Count)
        {
            rock = gm.rockList[gm.rockCurrent].rock;
            bool inturn = rm.inturn;
            
            // CRITICAL FIX: Check if it's an AI turn - if so, DON'T allow manual toggle!
            bool isAITurn = false;
            
            // Only check for AI turn if the rock is actually released/being thrown
            // During setup/pullback, allow human players to toggle freely
            Rock_Info currentRockInfo = gm.rockList[gm.rockCurrent].rockInfo;
            bool rockBeingThrown = currentRockInfo != null && (currentRockInfo.released || currentRockInfo.shotTaken);
            
            if (rockBeingThrown && (gm.aiTeamRed || gm.aiTeamYellow))
            {
                // Determine if current rock belongs to AI team
                bool isRedTurn = (gm.rockCurrent % 2 == 0) ? gm.redHammer : !gm.redHammer;
                isAITurn = (isRedTurn && gm.aiTeamRed) || (!isRedTurn && gm.aiTeamYellow);
            }

            // Only block toggle if rock is actively being thrown by AI
            if (!isAITurn)
            {
                // Handle mouse clicks (for PC/Editor)
                if (Input.GetMouseButtonDown(0))
                {
                    if (CheckRaycastHit(Input.mousePosition))
                    {
                        ToggleTurn();
                    }
                }

                // Handle touch input (for mobile/tablet)
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Began)
                    {
                        if (CheckRaycastHit(touch.position))
                        {
                            ToggleTurn();
                        }
                    }
                }
            }

            if (turnAI)
            {
                // AI sets rm.inturn directly, update animator immediately to match
                if (rm.inturn)
                {
                    anim.SetBool("inturn", true);
                }
                else
                {
                    anim.SetBool("inturn", false);
                }
                turnAI = false;
            }
        }
    }

    /// <summary>
    /// Checks if a screen position hits the turn toggle collider
    /// Works with both mouse and touch input
    /// </summary>
    private bool CheckRaycastHit(Vector3 screenPosition)
    {
        Vector3 worldPos = uiCam.ScreenToWorldPoint(screenPosition);
        Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(worldPos2D, Vector2.zero);

        return (hit.collider == col);
    }

    public void SetTurn(bool inturn)
    {
        // FIXED: Animator should match rm.inturn directly (not inverted)
        // Must match ToggleTurn() to stay consistent
        if (inturn)
        {
            anim.SetBool("inturn", true);
        }
        else
        {
            anim.SetBool("inturn", false);
        }
    }
}
