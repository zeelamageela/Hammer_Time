using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockManager : MonoBehaviour
{
    public GameManager gm;
    public CameraManager cm;
    public RandomRockPlacerment rrp;
    public RockBar rb;
    public GameObject rock;
    public Rock_Info rockInfo;
    public ShootingKnob shootKnob;
    public Transform house;
    public bool inturn;
    bool isPressed;

    void OnEnable()
    {

    }

    // Update is called once per frame
    private int lastRockIndex = -1; // Track which rock we last set the turn for
    
    void FixedUpdate()
    {
        // CRITICAL FIX: Check if GameManager exists and rockList is initialized
        if (gm == null || gm.rockList == null || gm.rockList.Count == 0)
            return;
        
        if (gm.rockCurrent < gm.rockList.Count)
        {
            // Additional null check for the rock itself
            if (gm.rockList[gm.rockCurrent] == null || gm.rockList[gm.rockCurrent].rock == null)
                return;
            
            //Debug.Log("Rock List Count is " + gm.rockList.Count);
            rock = gm.rockList[gm.rockCurrent].rock;
            rockInfo = gm.rockList[gm.rockCurrent].rockInfo;

            // CRITICAL FIX: Only set flipAxis for AI turns, NEVER for player turns
            // Player turns are handled by:
            //   1. GameManager.OnRedTurn/OnYellowTurn initializes rm.inturn = false
            //   2. TurnAnim.ToggleTurn() updates BOTH rm.inturn AND flipAxis when button is clicked
            // RockManager should only manage AI rock turn setup
            bool isAITurn = (gm.rockCurrent % 2 == 0) 
                ? (gm.redHammer ? gm.aiTeamYellow : gm.aiTeamRed) 
                : (gm.redHammer ? gm.aiTeamRed : gm.aiTeamYellow);
                
            Rock_Flick rockFlick = rock.GetComponent<Rock_Flick>();
            Rock_Force rockForce = rock.GetComponent<Rock_Force>();
                
            // Check if rock is being actively shot
            bool rockIsActiveForShooting = (rockFlick != null && 
                (rockFlick.isPressed || rockFlick.isPressedAI || rockFlick.mouseUp || rockInfo.released));
                
            // Check if Rock_Force component is not enabled yet
            bool rockNotYetActivated = (rockForce != null && !rockForce.enabled);
                
            // ONLY set flipAxis for AI turns (never player turns) when rock is ready
            if (isAITurn && lastRockIndex != gm.rockCurrent && !rockIsActiveForShooting && !rockNotYetActivated)
            {
                // ONLY set flipAxis when rock is newly activated, enabled, and NOT being shot
                if (inturn)
                {
                    rock.GetComponent<Rock_Force>().flipAxis = true;
                }
                else
                {
                    rock.GetComponent<Rock_Force>().flipAxis = false;
                }
                lastRockIndex = gm.rockCurrent;
                Debug.Log($"[RockManager] AI Turn - Set rock #{gm.rockCurrent} flipAxis to: {(inturn ? "IN-TURN" : "OUT-TURN")}");
            }
                
            // For player turns, only update lastRockIndex to prevent re-checking
            if (!isAITurn && lastRockIndex != gm.rockCurrent)
            {
                lastRockIndex = gm.rockCurrent;
                Debug.Log($"[RockManager] Player Turn - rock #{gm.rockCurrent} - flipAxis controlled by TurnAnim button");
            }

            // REMOVED: InPlayZoom during active rock movement
            // RockFollow now handles ALL camera behavior during rock travel
            // Only adjust top view depth when rock enters play area
            if (rock.transform.position.y >= -4f)
            {
                cm.top.depth = -1;
            }
        }
    }

}


