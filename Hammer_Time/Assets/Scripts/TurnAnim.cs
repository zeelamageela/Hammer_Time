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
            
            // Toggle the turn
            rm.inturn = !rm.inturn;
            
            // CRITICAL FIX: Also update the rock's flipAxis immediately!
            // This prevents RockManager from overriding the player's choice
            rock = gm.rockList[gm.rockCurrent].rock;
            Rock_Force rockForce = rock.GetComponent<Rock_Force>();
            if (rockForce != null)
            {
                rockForce.flipAxis = rm.inturn;
                Debug.Log($"[TurnAnim] Turn toggled - set rm.inturn={rm.inturn} AND rock.flipAxis={rockForce.flipAxis}");
            }
            
            // Start animation
            StartCoroutine(IsPressed(rm.inturn));
            
            Debug.Log($"Turn toggled to: {(rm.inturn ? "IN-TURN" : "OUT-TURN")}");
        }
    }
    
    void Update()
    {
        if (gm.rockList.Count != 0 && gm.rockCurrent < gm.rockList.Count)
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
                // AI sets rm.inturn directly, animate with ORIGINAL inversion
                // The IsPressed coroutine will invert it for the animator
                StartCoroutine(IsPressed(rm.inturn));
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

    IEnumerator IsPressed(bool inturn)
    {
        col.enabled = false;

        // FIXED: Animator should match rm.inturn directly (not inverted)
        // rm.inturn = true ? animator "inturn" = true ? shows IN-TURN (LEFT curl) graphic
        // rm.inturn = false ? animator "inturn" = false ? shows OUT-TURN (RIGHT curl) graphic
        // This matches the physics convention: rm.inturn = flipAxis
        if (inturn)
        {
            anim.SetBool("inturn", true);
        }
        else
        {
            anim.SetBool("inturn", false);
        }

        yield return new WaitForSeconds(0.25f);
        col.enabled = true;
    }

    public void SetTurn(bool inturn)
    {
        // FIXED: Animator should match rm.inturn directly (not inverted)
        // Must match IsPressed() to stay consistent
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
