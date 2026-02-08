using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Shooter : MonoBehaviour
{
    public GameManager gm;
    public TutorialManager tm;
    public RockManager rm;

    public AIManager aim;
    public AI_Target aiTarg;
    public AI_Strategy aiStrat;
    public AI_Sweeper aiSweep;

    Rock_Info rockInfo;
    Rock_Flick rockFlick;
    Rigidbody2D rockRB;
    
    int currentRockNumber;

    public Vector2 centreGuard;
    public Vector2 tightCentreGuard;
    public Vector2 highCentreGuard;

    public Vector2 leftHighCornerGuard;
    public Vector2 leftTightCornerGuard;
    public Vector2 leftCornerGuard;
    public Vector2 rightHighCornerGuard;
    public Vector2 rightTightCornerGuard;
    public Vector2 rightCornerGuard;

    public Vector2 topTwelveFoot;
    public Vector2 backTwelveFoot;
    public Vector2 leftTwelveFoot;
    public Vector2 rightTwelveFoot;

    public Vector2 backFourFoot;
    public Vector2 topFourFoot;
    public Vector2 leftFourFoot;
    public Vector2 rightFourFoot;
    public Vector2 button;

    public Vector2 peel;
    public Vector2 takeOut;
    public Vector2 raise;
    public Vector2 tick;


    public Vector2 guardAccu;
    public Vector2 drawAccu;
    public Vector2 toAccu;
    public Vector2 tickAccu;

    public float takeOutOffset;
    public float peelOffset;
    public float raiseOffset;
    public float tickOffset;

    float targetX;
    float targetY;
    public float takeOutX;
    public float takeOutY;
    float raiseY;
    GameSettingsPersist gsp;

    public void Start()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
    }


    public void OnShot(string aiShotType, int rockCurrent)
    {
        rockInfo = gm.rockList[rockCurrent].rockInfo;
        rockFlick = gm.rockList[rockCurrent].rock.GetComponent<Rock_Flick>();
        rockRB = gm.rockList[rockCurrent].rock.GetComponent<Rigidbody2D>();
        currentRockNumber = rockCurrent;

        StartCoroutine(Shot(aiShotType, rm.inturn));
    }

    IEnumerator Shot(string aiShotType, bool inturn)
    {
        Debug.Log("AI Shot " + aiShotType);
        gm.dbText.text = aiShotType;
        rockFlick.isPressedAI = true;
        takeOutX = aiTarg.takeOutX;
        takeOutY = aiTarg.takeOutY;

        aiSweep.OnSweep(true, aiShotType, aiTarg.targetPos, inturn);
        
        // CRITICAL FIX: Set BOTH flipAxis AND rm.inturn to keep everything synchronized!
        // This ensures the button, trajectory, and actual shot all match
        GameObject rock = gm.rockList[currentRockNumber].rock;
        Rock_Force rockForce = rock.GetComponent<Rock_Force>();
        if (rockForce != null)
        {
            rockForce.flipAxis = inturn;
            rm.inturn = inturn;  // SYNC rm.inturn with the AI's choice!
            Debug.Log($"[AI_Shooter] Set flipAxis AND rm.inturn = {inturn} for {aiShotType}");
        }

        yield return new WaitForSeconds(0.5f);

        float shotX;
        float shotY;

        switch (aiShotType)
        {
            #region Centre Guards
            case "Centre Guard":
                {
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.guardAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.15f);
                    
                    shotX = centreGuard.x + error.x;
                    shotY = centreGuard.y + error.y;
                    
                    if (inturn)
                        shotX = -shotX;
                    
                    rockFlick.rb.isKinematic = true;
                    rockRB.position = new Vector2(shotX, shotY);
                    yield return new WaitForFixedUpdate();
                    rockFlick.mouseUp = true;
                }
                break;

            case "Tight Centre Guard":
                {
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.guardAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.15f);
                    
                    shotX = tightCentreGuard.x + error.x;
                    shotY = tightCentreGuard.y + error.y;
                    
                    if (inturn)
                        shotX = -shotX;
                    
                    rockFlick.rb.isKinematic = true;
                    rockRB.position = new Vector2(shotX, shotY);
                    yield return new WaitForFixedUpdate();
                    rockFlick.mouseUp = true;
                }
                break;

            case "High Centre Guard":
                {
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.guardAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.15f);
                    
                    shotX = highCentreGuard.x + error.x;
                    shotY = highCentreGuard.y + error.y;
                    
                    if (inturn)
                        shotX = -shotX;
                    
                    rockFlick.rb.isKinematic = true;
                    rockRB.position = new Vector2(shotX, shotY);
                    yield return new WaitForFixedUpdate();
                    rockFlick.mouseUp = true;
                }
                break;
            #endregion

            #region Corner Guards
            case "Left Corner Guard":
                {
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.guardAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.15f);
                    
                    Vector2 targetPos = inturn ? rightCornerGuard : leftCornerGuard;
                    shotX = targetPos.x + error.x;
                    shotY = targetPos.y + error.y;
                    
                    if (inturn)
                        shotX = -shotX;
                    
                    rockFlick.rb.isKinematic = true;
                    rockRB.position = new Vector2(shotX, shotY);
                    yield return new WaitForFixedUpdate();
                    rockFlick.mouseUp = true;
                }
                break;

            case "Left Tight Corner Guard":
                {
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.guardAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.15f);
                    
                    Vector2 targetPos = inturn ? rightTightCornerGuard : leftTightCornerGuard;
                    shotX = targetPos.x + error.x;
                    shotY = targetPos.y + error.y;
                    
                    if (inturn)
                        shotX = -shotX;
                    
                    rockFlick.rb.isKinematic = true;
                    rockRB.position = new Vector2(shotX, shotY);
                    yield return new WaitForFixedUpdate();
                    rockFlick.mouseUp = true;
                }
                break;

            case "Left High Corner Guard":
                {
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.guardAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.15f);
                    
                    Vector2 targetPos = inturn ? rightHighCornerGuard : leftHighCornerGuard;
                    shotX = targetPos.x + error.x;
                    shotY = targetPos.y + error.y;
                    
                    if (inturn)
                        shotX = -shotX;
                    
                    rockFlick.rb.isKinematic = true;
                    rockRB.position = new Vector2(shotX, shotY);
                    yield return new WaitForFixedUpdate();
                    rockFlick.mouseUp = true;
                }
                break;

            case "Right Corner Guard":
                {
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.guardAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.15f);
                    
                    Vector2 targetPos = inturn ? leftCornerGuard : rightCornerGuard;
                    shotX = targetPos.x + error.x;
                    shotY = targetPos.y + error.y;
                    
                    if (inturn)
                        shotX = -shotX;
                    
                    rockFlick.rb.isKinematic = true;
                    rockRB.position = new Vector2(shotX, shotY);
                    yield return new WaitForFixedUpdate();
                    rockFlick.mouseUp = true;
                }
                break;

            case "Right Tight Corner Guard":
                {
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.guardAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.15f);
                    
                    Vector2 targetPos = inturn ? leftTightCornerGuard : rightTightCornerGuard;
                    shotX = targetPos.x + error.x;
                    shotY = targetPos.y + error.y;
                    
                    if (inturn)
                        shotX = -shotX;
                    
                    rockFlick.rb.isKinematic = true;
                    rockRB.position = new Vector2(shotX, shotY);
                    yield return new WaitForFixedUpdate();
                    rockFlick.mouseUp = true;
                }
                break;

            case "Right High Corner Guard":
                {
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.guardAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.15f);
                    
                    Vector2 targetPos = inturn ? leftHighCornerGuard : rightHighCornerGuard;
                    shotX = targetPos.x + error.x;
                    shotY = targetPos.y + error.y;
                    
                    if (inturn)
                        shotX = -shotX;

                    rockFlick.rb.isKinematic = true;
                    rockRB.position = new Vector2(shotX, shotY);
                    yield return new WaitForFixedUpdate();
                    rockFlick.mouseUp = true;
                }
                break;
            #endregion

            #region Twelve Foot Draws
            case "Top Twelve Foot":
                {
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.drawAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.12f);
                    
                    shotX = topTwelveFoot.x + error.x;
                    shotY = topTwelveFoot.y + error.y;
                    
                    if (inturn)
                        shotX = -shotX;

                    rockFlick.rb.isKinematic = true;
                    rockRB.position = new Vector2(shotX, shotY);
                    yield return new WaitForFixedUpdate();
                    rockFlick.mouseUp = true;
                }
                break;

            case "Left Twelve Foot":
                {
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.drawAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.12f);
                    
                    Vector2 targetPos = inturn ? rightTwelveFoot : leftTwelveFoot;
                    shotX = targetPos.x + error.x;
                    shotY = targetPos.y + error.y;
                    
                    if (inturn)
                        shotX = -shotX;

                    rockFlick.rb.isKinematic = true;
                    rockRB.position = new Vector2(shotX, shotY);
                    yield return new WaitForFixedUpdate();
                    rockFlick.mouseUp = true;
                }
                break;

            case "Back Twelve Foot":
                {
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.drawAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.12f);
                    
                    shotX = backTwelveFoot.x + error.x;
                    shotY = backTwelveFoot.y + error.y;
                    
                    if (inturn)
                        shotX = -shotX;

                    rockFlick.rb.isKinematic = true;
                    rockRB.position = new Vector2(shotX, shotY);
                    yield return new WaitForFixedUpdate();
                    rockFlick.mouseUp = true;
                }
                break;

            case "Right Twelve Foot":
                {
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.drawAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.12f);
                    
                    Vector2 targetPos = inturn ? leftTwelveFoot : rightTwelveFoot;
                    shotX = targetPos.x + error.x;
                    shotY = targetPos.y + error.y;
                    
                    if (inturn)
                        shotX = -shotX;

                    rockFlick.rb.isKinematic = true;
                    rockRB.position = new Vector2(shotX, shotY);
                    yield return new WaitForFixedUpdate();
                    rockFlick.mouseUp = true;
                }
                break;
            #endregion

            #region Four Foot Draws
            case "Button":
                {
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.drawAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.12f);
                    
                    shotX = button.x + error.x;
                    shotY = button.y + error.y;
                    
                    if (inturn)
                        shotX = -shotX;

                    rockFlick.rb.isKinematic = true;
                    rockRB.position = new Vector2(shotX, shotY);
                    yield return new WaitForFixedUpdate();
                    rockFlick.mouseUp = true;
                }
                break;

            case "Left Four Foot":
                {
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.drawAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.12f);
                    
                    Vector2 targetPos = inturn ? rightFourFoot : leftFourFoot;
                    shotX = targetPos.x + error.x;
                    shotY = targetPos.y + error.y;
                    
                    if (inturn)
                        shotX = -shotX;

                    rockFlick.rb.isKinematic = true;
                    rockRB.position = new Vector2(shotX, shotY);
                    yield return new WaitForFixedUpdate();
                    rockFlick.mouseUp = true;
                }
                break;

            case "Right Four Foot":
                {
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.drawAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.12f);
                    
                    Vector2 targetPos = inturn ? leftFourFoot : rightFourFoot;
                    shotX = targetPos.x + error.x;
                    shotY = targetPos.y + error.y;
                    
                    if (inturn)
                        shotX = -shotX;

                    rockFlick.rb.isKinematic = true;
                    rockRB.position = new Vector2(shotX, shotY);
                    yield return new WaitForFixedUpdate();
                    rockFlick.mouseUp = true;
                }
                break;

            case "Top Four Foot":
                {
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.drawAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.12f);
                    
                    shotX = topFourFoot.x + error.x;
                    shotY = topFourFoot.y + error.y;
                    
                    if (inturn)
                        shotX = -shotX;

                    rockFlick.rb.isKinematic = true;
                    rockRB.position = new Vector2(shotX, shotY);
                    yield return new WaitForFixedUpdate();
                    rockFlick.mouseUp = true;
                }
                break;

            case "Back Four Foot":
                {
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.drawAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.12f);
                    
                    shotX = backFourFoot.x + error.x;
                    shotY = backFourFoot.y + error.y;
                    
                    if (inturn)
                        shotX = -shotX;

                    rockFlick.rb.isKinematic = true;
                    rockRB.position = new Vector2(shotX, shotY);
                    yield return new WaitForFixedUpdate();
                    rockFlick.mouseUp = true;
                }
                break;
            #endregion

            #region Take Outs
            case "Peel":
                // Physics-based shot: AI_Target already calculated shot position WITH accuracy error applied
                // DO NOT apply error again here - just use the calculated position directly!
                if (takeOutX != 0f)
                {
                    shotX = takeOutX;
                    shotY = takeOutY;
                }
                else
                {
                    // Fallback: use draw accuracy if no target calculated
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.drawAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.12f);
                    shotX = button.x + error.x;
                    shotY = button.y + error.y;
                }

                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);

                Debug.Log("Peel Position is (" + rockRB.position.x + " ," + rockRB.position.y + ")");
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Take Out":
                // Physics-based shot: AI_Target already calculated shot position WITH accuracy error applied
                // DO NOT apply error again here - just use the calculated position directly!
                if (takeOutX != 0f)
                {
                    shotX = takeOutX;
                    shotY = takeOutY;
                    
                    Debug.Log($"[AI_Shooter] Take Out - Using physics-calculated position: ({shotX:F3}, {shotY:F3})");
                }
                else
                {
                    // Fallback: use draw accuracy if no target calculated
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.drawAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.12f);
                    shotX = button.x + error.x;
                    shotY = button.y + error.y;
                    
                    Debug.LogWarning($"[AI_Shooter] Take Out fallback - No physics position available");
                }

                rockRB.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);

                Debug.Log("Take Out Position is (" + rockRB.position.x + " ," + rockRB.position.y + ")");
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Tick":
                // Physics-based shot: AI_Target already calculated shot position WITH accuracy error applied
                // DO NOT apply error again here - just use the calculated position directly!
                if (takeOutX != 0f)
                {
                    shotX = takeOutX;
                    shotY = takeOutY;
                }
                else
                {
                    // Fallback
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.guardAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.1f);
                    shotX = button.x + error.x;
                    shotY = button.y + error.y;
                }

                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);

                Debug.Log("Tick Shot Position is (" + rockRB.position.x + " ," + rockRB.position.y + ")");
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Raise":
                // Physics-based shot: AI_Target already calculated shot position WITH accuracy error applied
                // DO NOT apply error again here - just use the calculated position directly!
                if (takeOutX != 0f)
                {
                    shotX = takeOutX;
                    shotY = takeOutY;
                }
                else
                {
                    // Fallback
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.takeOutAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.12f);
                    shotX = button.x + error.x;
                    shotY = button.y + error.y;
                }

                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);

                Debug.Log("Raise Position is (" + rockRB.position.x + " ," + rockRB.position.y + ")");
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;
            #endregion

            case "Draw To Target":

                shotX = takeOutX;
                shotY = takeOutY;

                //shotX = Random.Range(takeOutX + drawAccu.x, takeOutX - drawAccu.x);
                //shotY = Random.Range(takeOutY + drawAccu.y, takeOutY - drawAccu.y);

                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                rockFlick.mouseUp = true;
                break;

            case "Guard To Target":
                {
                    CharacterStats stats = GetShooterStats();
                    float accuracy = stats != null ? stats.guardAccuracy.GetValue() : 70f;
                    Vector2 error = GetAccuracyError(accuracy, 0.15f);
                    
                    shotX = takeOutX + error.x;
                    shotY = takeOutY + error.y;

                    rockFlick.rb.isKinematic = true;
                    rockRB.position = new Vector2(shotX, shotY);
                    rockFlick.mouseUp = true;
                }
                break;

            default:
                break;
        }

    }


    /// <summary>
    /// Get character stats for the current shooter
    /// </summary>
    private CharacterStats GetShooterStats()
    {
        TeamManager tm = FindObjectOfType<TeamManager>();
        if (tm == null) return null;
        
        int memberIndex = currentRockNumber / 4;
        memberIndex = Mathf.Clamp(memberIndex, 0, 3);
        
        bool isRedTeam = (currentRockNumber % 2 == 0) ? gm.redHammer : !gm.redHammer;
        
        if (isRedTeam && tm.teamRed != null && memberIndex < tm.teamRed.Length)
            return tm.teamRed[memberIndex].charStats;
        else if (!isRedTeam && tm.teamYellow != null && memberIndex < tm.teamYellow.Length)
            return tm.teamYellow[memberIndex].charStats;
        
        return null;
    }
    
    /// <summary>
    /// Apply character-based accuracy using realistic distribution
    /// Returns error offset to add to target position
    /// </summary>
    private Vector2 GetAccuracyError(float accuracy, float baseMaxError)
    {
        // Convert accuracy from 0-100 to 0-1 scale
        float accuracyRatio = Mathf.Clamp01(accuracy / 100f);
        
        // Calculate max error based on accuracy (better accuracy = less error)
        float maxError = baseMaxError * (1f - accuracyRatio);
        
        // Use circular distribution for natural shot spread
        return Random.insideUnitCircle * maxError;
    }

}
