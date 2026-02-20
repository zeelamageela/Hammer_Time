using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using Lofelt.NiceVibrations;
using System;
using Random = UnityEngine.Random;

public class RandomRockPlacerment : MonoBehaviour
{
    public GameManager gm;
    public RockManager rm;
    public TeamManager tm;
    public CameraManager cam;
    public Transform house;
    public bool placed;
    public bool placed1;
    public int rockCurrent;
    public float pauseAfterRockPlacement = 0f;

    public Vector2[] placePos;
    public Vector2[] rockPos;

    public GameObject playerStratGO;
    public bool aggressive;
    int playerSelection;
    public int round;

    public MMF_Player fltFdbk;
    public MMF_FloatingText fltText;

    public HapticClip drawHap;
    public HapticClip hitHap;

    public GameObject dialogueGO;
    public DialogueTrigger coachDialogue;
    public DialogueTrigger announDialogue;

    int guardCounter = 0;
    int houseCount = 0;

    private bool lastShotWasTakeout = false;
    private GameObject lastTakeoutTarget = null;
    
    // AI SYSTEM INTEGRATION
    [Header("AI Systems (Auto-assigned)")]
    private AI_Strategy aiStrategy;
    private AI_Target aiTarget;
    private TrajectorySimulator trajectorySimulator;
    
    [Header("Smart Placement Settings")]
    [Tooltip("Use AI systems for realistic rock placement")]
    public bool useSmartPlacement = true;
    
    [Header("Trajectory Visualization")]
    [Tooltip("Show trajectory paths for placed rocks")]
    public bool showTrajectoryLines = true;
    
    [Tooltip("How long to display trajectory lines (seconds)")]
    public float trajectoryDisplayDuration = 2.0f;
    
    [Tooltip("Line width for trajectory visualization")]
    public float trajectoryLineWidth = 0.02f;
    
    [Tooltip("Red team trajectory color")]
    public Color redTeamTrajectoryColor = new Color(1f, 0.2f, 0.2f, 0.6f);
    
    [Tooltip("Yellow team trajectory color")]
    public Color yellowTeamTrajectoryColor = new Color(1f, 1f, 0.2f, 0.6f);
    
    // Trajectory line tracking
    private List<GameObject> activeTrajectoryLines = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        placed = false;
        rockPos = new Vector2[gm.rockCurrent];
        HapticController.Play();
        Debug.Log("fldfdbk is " + fltFdbk);
        fltText = fltFdbk.GetFeedbackOfType<MMF_FloatingText>();
        
        // Initialize AI systems
        aiStrategy = FindObjectOfType<AI_Strategy>();
        aiTarget = FindObjectOfType<AI_Target>();
        
        // Initialize trajectory simulator (same physics as live game)
        TrajectoryLine trajectoryLine = FindObjectOfType<TrajectoryLine>();
        if (trajectoryLine != null)
        {
            trajectorySimulator = new TrajectorySimulator(
                trajectoryLine.iceFriction,
                trajectoryLine.curlStrength
            );
            Debug.Log($"[RandomRockPlacement] Trajectory simulator initialized with friction={trajectoryLine.iceFriction}, curl={trajectoryLine.curlStrength}");
        }
        else
        {
            Debug.LogWarning("[RandomRockPlacement] TrajectoryLine not found - smart placement will be disabled");
            useSmartPlacement = false;
        }
    }

    public Coroutine OnRockPlace(int rockCrnt, bool redTeam, bool mixed = false)
    {
        placed = false;
        rockCurrent = rockCrnt;
        return StartCoroutine(StratSelect(redTeam, true));
    }

    public void Help()
    {
        dialogueGO.SetActive(true);

        if (round == 1)
            coachDialogue.TriggerDialogue("Strategy", 0);
        else if (round == 2)
            coachDialogue.TriggerDialogue("Strategy", 1);
    }

    public void OnChoice(int selection)
    {
        //aggressive = aggro;
        playerSelection = selection;
        playerStratGO.SetActive(false);
        Debug.Log("Player Selection - " + playerSelection);
    }

    IEnumerator RandomRockPlace()
    {
        GameSettingsPersist gsp = FindFirstObjectByType<GameSettingsPersist>();
        CareerManager cm = FindFirstObjectByType<CareerManager>();

        int houseCount = 0;
        int houseRed = 0;
        bool[] guardCount = new bool[9];
        for (int i = 0; i < 9; i++)
            guardCount[i] = false;
        for (int i = 0; i < rockCurrent + 1; i++)
        {
            Random.InitState(System.DateTime.Now.Millisecond);
            int placeSelector;
            int shotSelector;
            if (i < 4 && Random.Range(0f, 1f) < 0.5f)
                shotSelector = Random.Range(1, 4);
            else
                shotSelector = Random.Range(0, 4);

            switch (shotSelector)
            {
                case 0:
                    placeSelector = 9;
                    if (houseCount > 4)
                    {
                        placeSelector = 10;
                        rockPos[i] = placePos[placeSelector];
                    }
                    else if (houseRed > 2)
                    {
                        if (gm.redHammer && i % 2 != 0)
                        {
                            rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 1.25f);
                        }
                        else if (!gm.redHammer && i % 2 != 1)
                            rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 1.25f);
                        else
                        {
                            placeSelector = 10;
                            rockPos[i] = placePos[placeSelector];
                        }
                    }

                    else if ((houseCount - houseRed) > 2)
                    {
                        if (gm.redHammer && i % 2 == 1)
                        {
                            houseRed++;
                            if (gsp.aiRed)
                                rockPos[i] = placePos[placeSelector]
                                    + (Random.insideUnitCircle * ((1f - (0.009f * cm.cStats.drawAccuracy)) * 1.25f));
                            else
                                rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 1.25f);
                        }
                        else if (!gm.redHammer && i % 2 == 0)
                        {
                            houseRed++;
                            if (gsp.aiRed)
                                rockPos[i] = placePos[placeSelector]
                                    + (Random.insideUnitCircle * ((1 - (0.009f * cm.cStats.drawAccuracy)) * 1.25f));
                            else
                                rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 1.25f);
                        }
                        else
                        {
                            placeSelector = 10;
                            rockPos[i] = placePos[placeSelector];
                        }
                    }
                    else
                    {
                        houseCount++;
                        rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 1.25f);
                    }
                    Debug.Log("case 0 rockPos is - " + rockPos[i].x + ", " + rockPos[i].y);
                    break;
                case 1:
                    placeSelector = 10;
                    rockPos[i] = placePos[placeSelector];
                    break;
                case 2:
                    placeSelector = 10;
                    rockPos[i] = placePos[placeSelector];
                    break;
                case 3:
                    if (i % 2 != 0)
                    {
                        placeSelector = Random.Range(0, 6);
                        if (guardCount[placeSelector])
                        {
                            placeSelector = Random.Range(0, 6);
                            if (guardCount[placeSelector])
                            {
                                placeSelector = 10;
                                rockPos[i] = placePos[placeSelector];
                            }
                            else
                            {
                                guardCount[placeSelector] = true;
                                rockPos[i] = placePos[placeSelector];
                            }
                        }
                        else
                        {
                            guardCount[placeSelector] = true;
                            rockPos[i] = placePos[placeSelector];
                        }

                    }
                    else
                    {
                        placeSelector = Random.Range(6, 9);
                        if (guardCount[placeSelector])
                        {
                            placeSelector = Random.Range(6, 9);
                            if (guardCount[placeSelector])
                            {
                                placeSelector = 10;
                                rockPos[i] = placePos[placeSelector];
                            }
                            else
                            {
                                guardCount[placeSelector] = true;
                                rockPos[i] = placePos[placeSelector];
                            }
                        }
                        else
                        {
                            guardCount[placeSelector] = true;
                            rockPos[i] = placePos[placeSelector];
                        }
                    }

                    rockPos[i] += Random.insideUnitCircle * 0.35f;
                    break;
            }
        }

        for (int i = 0; i < rockCurrent + 1; i++)
        {
            gm.rockList[i].rockInfo.placed = true;
        }

        //yield return new WaitForEndOfFrame();
        yield return null;

        for (int i = 0; i < rockCurrent + 1; i++)
        {
            gm.rockList[i].rock.GetComponent<CircleCollider2D>().radius = 0.14f;
            gm.rockList[i].rock.GetComponent<SpriteRenderer>().enabled = false;
            gm.rockList[i].rock.GetComponent<SpringJoint2D>().enabled = false;
            gm.rockList[i].rock.GetComponent<Rock_Flick>().enabled = false;
            gm.rockList[i].rock.transform.parent = null;
            //rm.rb.DeadRock(i);
            //yield return new WaitForEndOfFrame();
            yield return null;
            Debug.Log("Rock Position " + i + " " + rockPos[i].x + ", " + rockPos[i].y);
            gm.rockList[i].rock.GetComponent<Rigidbody2D>().position = rockPos[i];

            gm.rockList[i].rock.GetComponent<CircleCollider2D>().enabled = true;
            gm.rockList[i].rock.GetComponent<Rock_Release>().enabled = true;
            gm.rockList[i].rock.GetComponent<Rock_Force>().enabled = true;
            gm.rockList[i].rock.GetComponent<Rock_Colliders>().enabled = true;
            //yield return new WaitForEndOfFrame();
            yield return null;
            if (rockPos[i].y > 8f)
            {
                gm.rockList[i].rockInfo.inPlay = false;
                gm.rockList[i].rockInfo.outOfPlay = true;
                gm.rockList[i].rock.SetActive(false);
            }
            else
            {
                gm.rockList[i].rock.GetComponent<SpriteRenderer>().enabled = true;
                gm.rockList[i].rockInfo.inPlay = true;
                gm.rockList[i].rockInfo.outOfPlay = false;
            }
            gm.rockList[i].rockInfo.moving = false;
            gm.rockList[i].rockInfo.shotTaken = true;
            gm.rockList[i].rockInfo.released = true;
            gm.rockList[i].rockInfo.stopped = true;
            gm.rockList[i].rockInfo.rest = true;
            Debug.Log("i is equal to " + i);

            //rm.rb.ShotUpdate(rockCurrent, gm.rockList[i].rockInfo.outOfPlay);
            //yield return new WaitForEndOfFrame();
            yield return null;
        }

        //yield return new WaitForEndOfFrame();
        yield return null;

        //rocksPlaced = true;
        gm.rockCurrent = rockCurrent - 1;
        gm.rockTotal = 16;
        //yield return new WaitForEndOfFrame();
        yield return null;
        //placed = true;
    }

    IEnumerator StratSelect(bool redTeam, bool aiTurn)
    {
        round++;
        GameSettingsPersist gsp = FindFirstObjectByType<GameSettingsPersist>();
        CareerManager cm = FindFirstObjectByType<CareerManager>();
        if (!aiTurn)
            playerStratGO.SetActive(true);

        gm.rockBar.EndUpdate(gsp.yellowScore, gsp.redScore);

        //yield return new WaitForEndOfFrame();
        yield return null;

        if (aiTurn && redTeam)
        //tm.SetCharacter(rockCurrent, true);
        //tm.SetCharacter(rockCurrent, false);

        //if (!aiTurn)
        //    yield return new WaitUntil(() => !playerStratGO.activeSelf);
        Debug.Log("RockCurrent is " + rockCurrent + " and aiTurn is " + aiTurn);
        //if (rockCurrent == 8 | rockCurrent == 12)
        //    round++;
        if (rockCurrent < gm.rockCurrent + 1)
            yield return StartCoroutine(Placement(redTeam));
        else
            playerStratGO.SetActive(false);

        //yield return new WaitForEndOfFrame();
        yield return null;
    }

    private int EvaluateBestAIShot(bool isBehind, int rocksInHouse, int guardsInPlay, int aiSkill)
    {
        // Example logic:
        // - If behind and there are opponent rocks in the house, prefer takeout.
        // - If ahead, prefer guard.
        // - If house is empty, prefer draw.
        // - Use skill to bias toward more aggressive shots for higher skill.

        if (isBehind && rocksInHouse > 0 && aiSkill > 7)
            return 4; // Takeout
        if (!isBehind && guardsInPlay < 2 && aiSkill > 5)
            return 3; // Guard
        if (rocksInHouse == 0)
            return 0; // Draw
                      // Fallback: random between draw and guard
        return (Random.value < 0.5f) ? 0 : 3;
    }
    
    /// <summary>
    /// SMART PLACEMENT: Use AI systems to determine realistic rock placement
    /// This simulates what AI would actually shoot, then places result
    /// </summary>
    IEnumerator SmartPlacement(bool redTeam)
    {
        GameSettingsPersist gsp = FindFirstObjectByType<GameSettingsPersist>();
        Debug.Log("[SmartPlacement] Starting smart placement for rock " + rockCurrent);
        
        gm.houseList.Sort();
        Random.InitState(System.DateTime.Now.Millisecond);
        
        // Get team info
        int shooter = Mathf.FloorToInt(rockCurrent / 4);
        string activeTeamName;
        string otherTeamName;
        int activeScore;
        int otherScore;
        CharacterStats activeCharStats;
        
        // Determine teams
        if (rockCurrent % 2 == 0)
        {
            if (gm.redHammer)
            {
                activeTeamName = gsp.yellowTeamName;
                otherTeamName = gsp.redTeamName;
                activeScore = gsp.yellowScore;
                otherScore = gsp.redScore;
                activeCharStats = tm.teamYellow[shooter].charStats;
            }
            else
            {
                activeTeamName = gsp.redTeamName;
                otherTeamName = gsp.yellowTeamName;
                activeScore = gsp.redScore;
                otherScore = gsp.yellowScore;
                activeCharStats = tm.teamRed[shooter].charStats;
            }
        }
        else
        {
            if (gm.redHammer)
            {
                activeTeamName = gsp.redTeamName;
                otherTeamName = gsp.yellowTeamName;
                activeScore = gsp.redScore;
                otherScore = gsp.yellowScore;
                activeCharStats = tm.teamRed[shooter].charStats;
            }
            else
            {
                activeTeamName = gsp.yellowTeamName;
                otherTeamName = gsp.redTeamName;
                activeScore = gsp.yellowScore;
                otherScore = gsp.redScore;
                activeCharStats = tm.teamYellow[shooter].charStats;
            }
        }
        
        // Build shot context (simplified - just use local variables)
        bool isBehind = activeScore < otherScore;
        bool hasHammer = (rockCurrent % 2 == 0) ? gm.redHammer : !gm.redHammer;
        int rocksInHouse = gm.houseList.Count;
        int guardsInPlay = gm.gList.Count;
        
        // STEP 1: Determine shot type
        string shotType = DetermineSmartShotTypeSimple(isBehind, hasHammer, rocksInHouse, guardsInPlay);
        Debug.Log($"[SmartPlacement] AI chose shot type: {shotType}");
        
        // STEP 2: Calculate position based on shot type
        Vector2 targetPosition = Vector2.zero;
        
        switch (shotType)
        {
            case "Draw":
            case "Draw Four Foot":
                targetPosition = CalculateDrawPosition(activeCharStats);
                fltText.Value = "Draw";
                break;
                
            case "Guard":
            case "Centre Guard":
            case "Corner Guard":
                targetPosition = CalculateGuardPosition(activeCharStats);
                fltText.Value = "Guard";
                break;
                
            case "Take Out":
                // Find best takeout target
                int targetRock = FindBestTakeoutTarget(activeTeamName);
                if (targetRock >= 0)
                {
                    Vector2 shooterPos, targetPos;
                    if (CalculateTakeoutPositions(targetRock, activeCharStats, out shooterPos, out targetPos))
                    {
                        targetPosition = shooterPos;
                        rockPos[targetRock] = targetPos; // Update target rock position
                        lastShotWasTakeout = true;
                        if (targetRock < gm.rockList.Count)
                        {
                            lastTakeoutTarget = gm.rockList[targetRock].rock;
                        }
                        fltText.Value = "Takeout";
                        Debug.Log($"[SmartPlacement] Takeout target {targetRock}: shooter={shooterPos}, target={targetPos}");
                    }
                    else
                    {
                        // Fallback to draw
                        targetPosition = CalculateDrawPosition(activeCharStats);
                        fltText.Value = "Draw";
                    }
                }
                else
                {
                    // No target available, draw instead
                    targetPosition = CalculateDrawPosition(activeCharStats);
                    fltText.Value = "Draw";
                }
                break;
                
            default:
                // Out of play
                targetPosition = placePos[10];
                fltText.Value = "Out";
                break;
        }
        
        // STEP 3: Apply character accuracy variance
        float accuracy = GetAccuracyForShot(shotType, activeCharStats);
        float baseError = GetBaseErrorForShot(shotType);
        Vector2 error = GetAccuracyError(accuracy, baseError);
        
        rockPos[rockCurrent] = targetPosition + error;
        
        Debug.Log($"[SmartPlacement] Final position for rock {rockCurrent}: {rockPos[rockCurrent]} (error: {error})");
        
        placed = true;
        yield return StartCoroutine(CompletePlacement());
    }

    IEnumerator Placement(bool redTeam)
    {
        // USE SMART PLACEMENT if enabled and AI systems available
        if (useSmartPlacement && aiStrategy != null && trajectorySimulator != null)
        {
            Debug.Log("[Placement] Using SMART PLACEMENT (AI-driven)");
            yield return StartCoroutine(SmartPlacement(redTeam));
            yield break;
        }
        
        // FALLBACK: Original placement logic
        Debug.Log("[Placement] Using LEGACY PLACEMENT (random)");
        
        GameSettingsPersist gsp = FindFirstObjectByType<GameSettingsPersist>();
        Debug.Log("RedTeam is " + redTeam);
        gm.houseList.Sort();
        //int houseCount = 0;
        
        Random.InitState(System.DateTime.Now.Millisecond);

        int shotSelector = 1;
        int takeOutSelector = 99;
        //int freezeSelector = 99;

        int shooter = Mathf.FloorToInt(rockCurrent / 4);
        string activeTeamName;
        string otherTeamName;
        int activeScore;
        int otherScore;
        
        CharacterStats activeCharStats;
        CharacterStats otherCharStats;

        Debug.Log("RockCurrent is " + rockCurrent);
        Debug.Log("gm.houseList.Count is " + gm.houseList.Count);
        Debug.Log("guardCounter is " + guardCounter);

        if (rockCurrent % 2 == 0)
        {
            if (gm.redHammer)
            {
                activeTeamName = gsp.yellowTeamName;
                otherTeamName = gsp.redTeamName;
                activeScore = gsp.yellowScore;
                otherScore = gsp.redScore;
                activeCharStats = tm.teamYellow[shooter].charStats;
                otherCharStats = tm.teamRed[shooter].charStats;
            }
            else
            {

                activeTeamName = gsp.redTeamName;
                otherTeamName = gsp.yellowTeamName;
                activeScore = gsp.redScore;
                otherScore = gsp.yellowScore;
                activeCharStats = tm.teamRed[shooter].charStats;
                otherCharStats = tm.teamYellow[shooter].charStats;
            }
        }
        else
        {
            if (gm.redHammer)
            {

                activeTeamName = gsp.redTeamName;
                otherTeamName = gsp.yellowTeamName;
                activeScore = gsp.redScore;
                otherScore = gsp.yellowScore;
                activeCharStats = tm.teamRed[shooter].charStats;
                otherCharStats = tm.teamYellow[shooter].charStats;
            }
            else
            {
                activeTeamName = gsp.yellowTeamName;
                otherTeamName = gsp.redTeamName;
                activeScore = gsp.yellowScore;
                otherScore = gsp.redScore;
                activeCharStats = tm.teamYellow[shooter].charStats;
                otherCharStats = tm.teamRed[shooter].charStats;
            }
        }

        int scenario = 0;
        #region Scenario Selector
        if (gm.endCurrent >= gm.endTotal - 1)
        {
            if (activeScore == otherScore)
                scenario = 9;
            else if (activeScore < otherScore)
                scenario = 10;
            else
                scenario = 11;
            Debug.Log("Last End");
        }
        else if (gm.endCurrent == gm.endTotal - 2)
        {
            if (activeScore == otherScore)
                scenario = 6;
            else if (activeScore < otherScore)
                scenario = 7;
            else
                scenario = 8;
            Debug.Log("Penultimate End");
        }
        else if (gm.endCurrent >= gm.endTotal - 4)
        {
            if (activeScore == otherScore)
                scenario = 3;
            else if (activeScore < otherScore)
                scenario = 4;
            else
                scenario = 5;
            Debug.Log("Middle Ends");
        }
        else
        {
            if (activeScore == otherScore)
                scenario = 0;
            else if (activeScore < otherScore)
                scenario = 1;
            else
                scenario = 2;
            Debug.Log("Early Ends");
        }
        #endregion


        switch (scenario)
        {
            case 0:
                #region Early Game - Tied
                Debug.Log("Early Game - Tied");
                #endregion
                break;
            case 1:
                #region Early Game - Losing
                Debug.Log("Early Game - Losing");
                #endregion
                break;
            case 2:
                #region Early Game - Winning
                Debug.Log("Early Game - Winning");
                #endregion
                break;
            case 3:
                #region Mid Game - Tied
                Debug.Log("Mid Game - Tied");
                #endregion
                break;
            case 4:
                #region Mid Game - Losing
                Debug.Log("Mid Game - Losing");
                #endregion
                break;
            case 5:
                #region Mid Game - Winning
                Debug.Log("Mid Game - Winning");
                #endregion
                break;
            case 6:
                #region Penultimate End - Tied
                Debug.Log("Penultimate End - Tied");
                //with hammer
                    //blank to keep hammer
                //without hammer
                    //force 1 to take hammer
                if (rockCurrent < 5)
                {
                    if (gm.houseList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "House", out hit, out takeOutSelector);

                        if (hit)
                        {
                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                            //Crash check
                            if (shotSelector == 99)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = 1;
                                }
                            }
                        }
                        else
                        {
                            if (gm.houseList.Count > 2)
                            {
                                shotSelector = SkillCheck("Guard", activeCharStats.drawAccuracy.GetValue());
                            }
                            else
                            {
                                shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                            }
                        }
                    }
                    else
                    {
                        if (gm.houseList.Count > 2)
                        {
                            shotSelector = SkillCheck("Guard", activeCharStats.drawAccuracy.GetValue());
                        }
                        else
                        {
                            shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                        }
                    }
                }
                else
                {
                    //if there's rocks in the hosue 
                    if (gm.houseList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "House", out hit, out takeOutSelector);

                        if (hit)
                        {
                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                            //Crash check
                            if (shotSelector == 99)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = 1;
                                }
                            }
                        }
                        else
                        {
                            if (gm.houseList.Count > 2)
                            {
                                shotSelector = SkillCheck("Guard", activeCharStats.drawAccuracy.GetValue());
                            }
                            else
                            {
                                shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                            }
                        }
                    }
                    else if (gm.gList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "Guards", out hit, out takeOutSelector);

                        if (hit)
                        {
                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                            //Crash check
                            if (shotSelector == 99)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = 1;
                                }
                            }
                        }
                        else
                        {
                            if (gm.houseList.Count > 2)
                            {
                                shotSelector = SkillCheck("Guard", activeCharStats.drawAccuracy.GetValue());
                            }
                            else
                            {
                                shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                            }
                        }
                    }
                    else
                    {
                        if (gm.houseList.Count > 2)
                        {
                            shotSelector = SkillCheck("Guard", activeCharStats.drawAccuracy.GetValue());
                        }
                        else
                        {
                            shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                        }
                    }
                }
                #endregion
                break;
            case 7:
                #region Penultimate End - Losing
                Debug.Log("Penultimate End - Losing");
                //with hammer
                    //blank to keep hammer or score 2
                //without hammer
                    //steal 1

                if (rockCurrent < 5)
                {
                    //if there's rocks in the house
                    if (gm.houseList.Count > 0)
                    {
                        //target an opponent's rock
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "House", out hit, out takeOutSelector);
                        //if there's a target
                        if (hit)
                        {
                            if (rockCurrent % 2 == 1)
                            {
                                if (gm.rockList[takeOutSelector].rock.transform.position.y > 6.5f)
                                {
                                    shotSelector = SkillCheck("Freeze", activeCharStats.drawAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                    //Crash check
                                    if (shotSelector == 99)
                                    {
                                        TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                        if (hit)
                                        {
                                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                        }
                                        else
                                        {
                                            shotSelector = 1;
                                        }
                                    }
                                }
                            }
                            //if there's no target
                            else
                            {
                                shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                //Crash check
                                if (shotSelector == 99)
                                {
                                    TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                    if (hit)
                                    {
                                        shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                    }
                                    else
                                    {
                                        shotSelector = 1;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (gm.houseList.Count > 2)
                            {
                                shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                            }
                            else
                            {
                                shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                            }
                        }
                    }
                    else
                    {
                        if (gm.gList.Count < 2)
                        {
                            shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                        }
                        else
                        {
                            shotSelector = SkillCheck("Draw", activeCharStats.guardAccuracy.GetValue());
                        }
                    }
                }
                else if (rockCurrent > 13)
                {
                    if (gm.houseList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "House", out hit, out takeOutSelector);

                        if (hit)
                        {
                            if (rockCurrent % 2 == 1)
                            {
                                if (gm.rockList[takeOutSelector].rock.transform.position.y > 6.5f)
                                {
                                    shotSelector = SkillCheck("Freeze", activeCharStats.takeOutAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                    //Crash check
                                    if (shotSelector == 99)
                                    {
                                        TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                        if (hit)
                                        {
                                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                        }
                                        else
                                        {
                                            shotSelector = 1;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                //Crash check
                                if (shotSelector == 99)
                                {
                                    TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                    if (hit)
                                    {
                                        shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                    }
                                    else
                                    {
                                        shotSelector = 1;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (gm.houseList.Count > 2)
                            {
                                shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                            }
                            else
                            {
                                shotSelector = SkillCheck("Draw Four Foot", activeCharStats.drawAccuracy.GetValue());
                            }
                        }
                    }
                    else
                    {
                        shotSelector = SkillCheck("Draw Four Foot", activeCharStats.drawAccuracy.GetValue());
                    }
                }
                else
                {
                    if (gm.houseList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "House", out hit, out takeOutSelector);

                        if (hit)
                        {
                            if (rockCurrent % 2 == 1)
                            {
                                //if the target is behind the tee line
                                if (gm.rockList[takeOutSelector].rock.transform.position.y > 6.5f)
                                {
                                    shotSelector = SkillCheck("Freeze", activeCharStats.drawAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                    //Crash check
                                    if (shotSelector == 99)
                                    {
                                        TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                        if (hit)
                                        {
                                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                        }
                                        else
                                        {
                                            shotSelector = 1;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                //Crash check
                                if (shotSelector == 99)
                                {
                                    TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                    if (hit)
                                    {
                                        shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                    }
                                    else
                                    {
                                        shotSelector = 1;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (gm.gList.Count > 0)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "Guards", out hit, out takeOutSelector);
                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                    //Crash check
                                    if (shotSelector == 99)
                                    {
                                        TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                        if (hit)
                                        {
                                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                        }
                                        else
                                        {
                                            shotSelector = 1;
                                        }
                                    }
                                }
                                else
                                {
                                    if (gm.houseList.Count > 2)
                                    {
                                        shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                                    }
                                    else
                                    {
                                        shotSelector = SkillCheck("Draw Four Foot", activeCharStats.drawAccuracy.GetValue());
                                    }
                                }
                            }
                            else if (gm.houseList.Count > 2)
                            {
                                shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                            }
                            else
                            {
                                shotSelector = SkillCheck("Draw Four Foot", activeCharStats.drawAccuracy.GetValue());
                            }
                        }
                    }
                    else
                    {
                        if (gm.gList.Count < 2)
                        {
                            shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                        }
                        else
                        {
                            shotSelector = SkillCheck("Draw", activeCharStats.guardAccuracy.GetValue());
                        }
                    }
                }
                #endregion
                break;
            case 8:
                #region Penultimate End - Winning
                Debug.Log("Penultimate End - Winning");
                //with hammer
                    //prevent scoring (very conservative)
                //without hammer
                    //force 1

                if (rockCurrent % 2 == 1)
                {

                }
                if (rockCurrent < 5)
                {
                    if (gm.houseList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "House", out hit, out takeOutSelector);

                        if (hit)
                        {
                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                            //Crash check
                            if (shotSelector == 99)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = 1;
                                }
                            }
                        }
                        else
                        {
                            if (gm.houseList.Count > 2)
                            {
                                shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                            }
                            else
                            {
                                shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                            }
                        }
                    }
                    else
                    {
                        if (gm.houseList.Count > 2)
                        {
                            shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                        }
                        else
                        {
                            shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                        }
                    }
                }
                else if (rockCurrent > 13)
                {
                    if (gm.houseList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "House", out hit, out takeOutSelector);

                        if (hit)
                        {
                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                            //Crash check
                            if (shotSelector == 99)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = 1;
                                }
                            }
                        }
                        else
                        {
                            shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                        }
                    }
                    else
                    {
                        shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                    }
                }
                else
                {
                    if (gm.houseList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "House", out hit, out takeOutSelector);

                        if (hit)
                        {
                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                            //Crash check
                            if (shotSelector == 99)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = 1;
                                }
                            }
                        }
                        else
                        {
                            if (gm.gList.Count > 0)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "Guards", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                    //Crash check
                                    if (shotSelector == 99)
                                    {
                                        TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                        if (hit)
                                        {
                                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                        }
                                        else
                                        {
                                            shotSelector = 1;
                                        }
                                    }
                                }
                                else if (gm.houseList.Count > 2)
                                {
                                    shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                                }
                            }
                            else if (gm.houseList.Count > 2)
                            {
                                shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                            }
                            else
                            {
                                shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                            }
                        }
                    }
                    else if (gm.gList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "Guards", out hit, out takeOutSelector);

                        if (hit)
                        {
                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                            //Crash check
                            if (shotSelector == 99)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = 1;
                                }
                            }
                        }
                        else
                        {
                            shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                        }
                    }
                    else
                    {
                        shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                    }
                }
                #endregion
                break;
            case 9:
                #region Last End - Tied
                //with hammer
                    //prevent opponent scoring at all costs
                //without hammer
                    //steal 1 at all costs
                Debug.Log("Last End - Tied");
                if (rockCurrent < 5)
                {
                    if (gm.houseList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "House", out hit, out takeOutSelector);

                        if (hit)
                        {
                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                            //Crash check
                            if (shotSelector == 99)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = 1;
                                }
                            }
                        }
                        else
                        {
                            if (gm.houseList.Count > 2)
                            {
                                shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                            }
                            else
                            {
                                shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                            }
                        }
                    }
                    else
                    {
                        if (gm.houseList.Count > 2)
                        {
                            shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                        }
                        else
                        {
                            shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                        }
                    }
                }
                else if (rockCurrent > 13)
                {
                    if (gm.houseList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "House", out hit, out takeOutSelector);

                        if (hit)
                        {
                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                            //Crash check
                            if (shotSelector == 99)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = 1;
                                }
                            }
                        }
                        else
                        {
                            shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                        }
                    }
                    else
                    {
                        shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                    }
                }
                else
                {
                    if (gm.houseList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "House", out hit, out takeOutSelector);

                        if (hit)
                        {
                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                            //Crash check
                            if (shotSelector == 99)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = 1;
                                }
                            }
                        }
                        else
                        {
                            if (gm.gList.Count > 0)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "Guards", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                    //Crash check
                                    if (shotSelector == 99)
                                    {
                                        TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                        if (hit)
                                        {
                                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                        }
                                        else
                                        {
                                            shotSelector = 1;
                                        }
                                    }
                                }
                                else if (gm.houseList.Count > 2)
                                {
                                    shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                                }
                            }
                            else if (gm.houseList.Count > 2)
                            {
                                shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                            }
                            else
                            {
                                shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                            }
                        }
                    }
                    else if (gm.gList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "Guards", out hit, out takeOutSelector);

                        if (hit)
                        {
                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                            //Crash check
                            if (shotSelector == 99)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = 1;
                                }
                            }
                        }
                        else
                        {
                            shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                        }
                    }
                    else
                    {
                        shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                    }
                }
                #endregion
                break;
            case 10:
                #region Last End - Losing
                Debug.Log("Last End - Losing");
                //with hammer
                    //score
                //without hammer
                    //steal
                if (rockCurrent < 5)
                {
                    if (gm.houseList.Count > 2)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "House", out hit, out takeOutSelector);

                        if (hit)
                        {
                            shotSelector = SkillCheck("Freeze", activeCharStats.takeOutAccuracy.GetValue());
                            //Crash check
                            if (shotSelector == 99)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = 4;
                                }
                                else
                                {
                                    shotSelector = 1;
                                }
                            }
                        }
                        else
                        {
                            if (gm.houseList.Count > 2)
                            {
                                shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                            }
                            else
                            {
                                shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                            }
                        }
                    }
                    else
                    {
                        shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                    }
                }
                else if (rockCurrent > 13)
                {
                    if (gm.houseList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "House", out hit, out takeOutSelector);

                        if (hit)
                        {
                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                            //Crash check
                            if (shotSelector == 99)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = 1;
                                }
                            }
                        }
                        else
                        {
                            shotSelector = SkillCheck("Draw Four Foot", activeCharStats.drawAccuracy.GetValue());
                        }
                    }
                    else
                    {
                        shotSelector = SkillCheck("Draw Four Foot", activeCharStats.drawAccuracy.GetValue());
                    }
                }
                else
                {
                    if (gm.gList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "Guards", out hit, out takeOutSelector);

                        if (hit)
                        {
                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                            //Crash check
                            if (shotSelector == 99)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = 1;
                                }
                            }
                        }
                        else
                        {
                            shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                        }
                    }
                    else if (gm.houseList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "House", out hit, out takeOutSelector);

                        if (hit)
                        {
                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                            //Crash check
                            if (shotSelector == 99)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = 1;
                                }
                            }
                        }
                        else
                        {
                            if (gm.gList.Count > 0)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "Guards", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                    //Crash check
                                    if (shotSelector == 99)
                                    {
                                        TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                        if (hit)
                                        {
                                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                        }
                                        else
                                        {
                                            shotSelector = 1;
                                        }
                                    }
                                }
                                else if (gm.houseList.Count > 2)
                                {
                                    shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                                }
                            }
                            else if (gm.houseList.Count > 2)
                            {
                                shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                            }
                            else
                            {
                                shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                            }
                        }
                    }
                    else
                    {
                        shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                    }
                }
                #endregion
                break;
            case 11:
                #region Last End - Winning
                Debug.Log("Last End - Winning");
                if (rockCurrent < 5)
                {
                    if (gm.houseList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "House", out hit, out takeOutSelector);

                        if (hit)
                        {
                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                            //Crash check
                            if (shotSelector == 99)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = 1;
                                }
                            }
                        }
                        else
                        {
                            if (gm.houseList.Count > 2)
                            {
                                shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                            }
                            else
                            {
                                shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                            }
                        }
                    }
                    else
                    {
                        if (gm.houseList.Count > 2)
                        {
                            shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                        }
                        else
                        {
                            shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                        }
                    }
                }
                else if (rockCurrent > 13)
                {
                    if (gm.houseList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "House", out hit, out takeOutSelector);

                        if (hit)
                        {
                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                            //Crash check
                            if (shotSelector == 99)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = 1;
                                }
                            }
                        }
                        else
                        {
                            shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                        }
                    }
                    else
                    {
                        shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                    }
                }
                else
                {
                    if (gm.houseList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "House", out hit, out takeOutSelector);

                        if (hit)
                        {
                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                            //Crash check
                            if (shotSelector == 99)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = 1;
                                }
                            }
                        }
                        else
                        {
                            if (gm.gList.Count > 0)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "Guards", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                    //Crash check
                                    if (shotSelector == 99)
                                    {
                                        TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                        if (hit)
                                        {
                                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                        }
                                        else
                                        {
                                            shotSelector = 1;
                                        }
                                    }
                                }
                                else if (gm.houseList.Count > 2)
                                {
                                    shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                                }
                            }
                            else if (gm.houseList.Count > 2)
                            {
                                shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
                            }
                            else
                            {
                                shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                            }
                        }
                    }
                    else if (gm.gList.Count > 0)
                    {
                        bool hit;
                        TakeOutTarget(activeTeamName, otherTeamName, "Guards", out hit, out takeOutSelector);

                        if (hit)
                        {
                            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                            //Crash check
                            if (shotSelector == 99)
                            {
                                TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);

                                if (hit)
                                {
                                    shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
                                }
                                else
                                {
                                    shotSelector = 1;
                                }
                            }
                        }
                        else
                        {
                            shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                        }
                    }
                    else
                    {
                        shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
                    }
                }
                #endregion
                break;
        }

        // Count rocks in house and guards
        int rocksInHouse = gm.houseList.Count;
        int guardsInPlay = gm.gList.Count;
        bool isBehind = activeScore < otherScore;
        int aiSkill = 8; // Set a default AI skill level
        //int aiSkill = activeCharStats.drawAccuracy.GetValue(); // Or average of skills

        // Use improved AI logic
        if (gsp.aiRed && redTeam || gsp.aiYellow && !redTeam)
        {
            shotSelector = EvaluateBestAIShot(isBehind, rocksInHouse, guardsInPlay, aiSkill);
        }

        if (gsp.cashGame)
        {
            if (rockCurrent == 11)
            {
                playerSelection = 1;
                ShotSelector(7, 99);
            }
            else if (rockCurrent == 10)
            {
                playerSelection = 2;
                ShotSelector(7, 99);
            }
            else
            {
                playerSelection = 3;
                ShotSelector(7, 99);
            }

            Debug.Log("Player Selection - " + playerSelection);
        }

        else
            ShotSelector(shotSelector, takeOutSelector, activeCharStats, otherCharStats);

        //Debug.Log("Team Yellow TakeOut Accuracy " + tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue());

        //Debug.Log("Team Red Draw Accuracy " + tm.teamRed[shooter].charStats.drawAccuracy.GetValue());
            
        //ShotSelector(guardCount, shotSelector, houseCount, guardCounter, takeOutSelector, freezeSelector, shooter);

        yield return new WaitUntil(() => placed = true);

        yield return StartCoroutine(CompletePlacement());
    }

    IEnumerator CompletePlacement()
    {
        // MINIMUM SPACING: Ensure rocks don't touch when placed
        const float ROCK_RADIUS = 0.14f;
        const float MIN_SPACING_GAP = 0.08f; // Small gap between rocks (adjustable)
        const float MIN_SAFE_DISTANCE = (ROCK_RADIUS * 2f) + MIN_SPACING_GAP; // 0.28 + 0.08 = 0.36 units

        for (int i = 0; i < rockCurrent + 1; i++)
        {

            gm.rockList[i].rockInfo.placed = true;
        }

        for (int i = 0; i < rockCurrent + 1; i++)
        {
            gm.rockList[i].rock.GetComponent<CircleCollider2D>().radius = 0.14f;
            gm.rockList[i].rock.GetComponent<SpriteRenderer>().enabled = false;
            gm.rockList[i].rock.GetComponent<SpringJoint2D>().enabled = false;
            gm.rockList[i].rock.GetComponent<Rock_Flick>().enabled = false;
            gm.rockList[i].rock.transform.parent = null;
            //rm.rb.DeadRock(i);
            //yield return new WaitForEndOfFrame();
            
            // SPACING CHECK: Ensure this rock doesn't touch any previously placed rocks
            bool tooClose = true;
            int maxAttempts = 10;
            int attempt = 0;
            Vector2 originalPos = rockPos[i];
            
            while (tooClose && attempt < maxAttempts)
            {
                tooClose = false;
                
                // Check distance to all previously placed rocks
                for (int j = 0; j < i; j++)
                {
                    if (rockPos[j].y < 8f) // Only check rocks that are in play
                    {
                        float distance = Vector2.Distance(rockPos[i], rockPos[j]);
                        
                        if (distance < MIN_SAFE_DISTANCE)
                        {
                            tooClose = true;
                            
                            // Nudge the rock away from the collision
                            Vector2 awayDirection = (rockPos[i] - rockPos[j]).normalized;
                            if (awayDirection.magnitude < 0.01f)
                            {
                                // If rocks are exactly overlapping, push in random direction
                                awayDirection = Random.insideUnitCircle.normalized;
                            }
                            
                            // Move rock to safe distance
                            rockPos[i] = rockPos[j] + (awayDirection * MIN_SAFE_DISTANCE);
                            
                            Debug.Log($"[Placement] Rock {i} too close to rock {j} (dist={distance:F3}), adjusting: {originalPos} -> {rockPos[i]}");
                            break; // Recheck all rocks after adjustment
                        }
                    }
                }
                
                attempt++;
            }
            
            if (attempt >= maxAttempts)
            {
                Debug.LogWarning($"[Placement] Rock {i} couldn't find safe spacing after {maxAttempts} attempts, using best position");
            }
            
            //Debug.Log("Rock Position " + i + " " + rockPos[i]);
            gm.rockList[i].rock.transform.position = rockPos[i];

            gm.rockList[i].rock.GetComponent<CircleCollider2D>().enabled = true;
            gm.rockList[i].rock.GetComponent<Rock_Release>().enabled = true;
            gm.rockList[i].rock.GetComponent<Rock_Force>().enabled = true;
            gm.rockList[i].rock.GetComponent<Rock_Colliders>().enabled = true;

            //yield return new WaitForEndOfFrame();

            if (rockPos[i].y > 8f)
            {
                gm.rockList[i].rockInfo.inPlay = false;
                gm.rockList[i].rockInfo.outOfPlay = true;
                gm.rockList[i].rock.SetActive(false);
            }
            else
            {
                gm.rockList[i].rock.GetComponent<SpriteRenderer>().enabled = true;
                gm.rockList[i].rockInfo.inPlay = true;
                gm.rockList[i].rockInfo.outOfPlay = false;
            }
            gm.rockList[i].rockInfo.moving = false;
            gm.rockList[i].rockInfo.shotTaken = true;
            gm.rockList[i].rockInfo.released = true;
            gm.rockList[i].rockInfo.stopped = true;
            gm.rockList[i].rockInfo.rest = true;
            //Debug.Log("i is equal to " + i);
            //Handheld.Vibrate();
            //rm.rb.ShotUpdate(rockCurrent, gm.rockList[i].rockInfo.outOfPlay);
            //yield return new WaitForEndOfFrame();
            yield return null;

        }

        gm.houseList.Clear();
        gm.gList.Clear();
        int counter = 0;
        foreach (Rock_List rock in gm.rockList)
        {
            if (rock.rockInfo.inPlay == true && rock.rockInfo.inHouse)
            {
                counter++;
                gm.houseList.Add(new House_List(rock.rock, rock.rockInfo));
                Debug.Log("Adding House " + counter + " - " + rock.rockInfo.teamName + rock.rockInfo.rockNumber);
            }
            if (rock.rockInfo.inPlay && !rock.rockInfo.inHouse && rock.rock.transform.position.y <= 6.5f)
            {
                gm.gList.Add(new Guard_List(rockCurrent, rock.rockInfo.freeGuard, rock.rock.transform));
                Debug.Log("Guard " + rock.rockInfo.name + " - " + rock.rockInfo.distance);
            }
        }
        if (gm.houseList.Count > 0)
        {
            Debug.Log("houseList shot rock - " + gm.houseList[0].rockInfo.teamName + " " + gm.houseList[0].rockInfo.rockNumber);
            gm.houseList.Sort();
            Debug.Log("Sorted houseList - " + gm.houseList[0].rockInfo.teamName + " " + gm.houseList[0].rockInfo.rockNumber);
        }


        //if (gm.gsp.debug)
        //    fltFdbk.PlayFeedbacks(gm.rockList[rockCurrent].rock.transform.position, 1f);

        //fltText.TargetTransform = gm.rockList[rockCurrent].rock.transform;
        //fltFdbk.PlayFeedbacks(gm.rockList[rockCurrent].rock.transform.position);
        //gm.rockCurrent = rockCurrent - 1;
        //gm.rockCurrent--;
        placed1 = true;
        
        if (pauseAfterRockPlacement > 0f && !gm.rockList[rockCurrent].rockInfo.outOfPlay)
        {
            GameObject tempRock = Instantiate(gm.rockList[rockCurrent].rock, gm.rockList[rockCurrent].rock.transform.position, 
                Quaternion.identity);
            tempRock.GetComponent<Animator>().enabled = true;

            if (lastShotWasTakeout)
            {
                GameObject tempHitRock = Instantiate(lastTakeoutTarget, lastTakeoutTarget.transform.position,
                    Quaternion.identity);
                tempHitRock.name = "HitRock Anim Temp";
                tempHitRock.GetComponent<Animator>().enabled = true;
            }

            yield return new WaitForSeconds(pauseAfterRockPlacement);

            Destroy(tempRock);
            if (lastShotWasTakeout)
            {
                Destroy(GameObject.Find("HitRock Anim Temp"));
            }
        }
    }

    void ShotSelector(int shotSelector, int takeOutSelector, CharacterStats activeCharStats = null, CharacterStats otherCharStats = null)
    {
        Random.InitState((int)System.DateTime.Now.Ticks);
        //placed = false;
        int placeSelector;
        Debug.Log("Shot Selector - " + shotSelector);
        lastShotWasTakeout = false;
        switch (shotSelector)
        {
            case 0:
                #region Draw Random
                Debug.Log("Case 0 - House");
                HapticController.Play(drawHap);
                fltText.Value = "Draw";
                //Debug.Log("Case 0 - " + houseCount + " - i is " + rockCurrent);
                placeSelector = 9;
                rockPos[rockCurrent] = placePos[placeSelector]
                    + (Random.insideUnitCircle
                    * (1.5f - (0.01f * activeCharStats.drawAccuracy.GetValue())));
                //houseCount++;
                //gm.houseList.Add(new House_List (gm.rockList[rockCurrent].rock, gm.rockList[rockCurrent].rockInfo));
                Debug.Log("case 0 rockPos is - " + rockPos[rockCurrent].x + ", " + rockPos[rockCurrent].y);
                break;
                #endregion
            case 1:
                #region Out
                Debug.Log("Case 1 - Out");
                fltText.Value = "Out";
                placeSelector = 10;
                rockPos[rockCurrent] = placePos[placeSelector];
                Debug.Log("case 1 rockPos is - " + rockPos[rockCurrent].x + ", " + rockPos[rockCurrent].y);
                break;
            #endregion
            case 2:
                #region Draw Four Foot
                Debug.Log("Case 2 - Four Foot");
                HapticController.Play(drawHap);
                fltText.Value = "Four Foot";
                placeSelector = 9;
                rockPos[rockCurrent] = placePos[placeSelector] + (Random.insideUnitCircle * 0.5f);
                Debug.Log("case 2 rockPos is - " + rockPos[rockCurrent].x + ", " + rockPos[rockCurrent].y);
                break;
                #endregion
            case 3:
                #region AutoGuard
                Debug.Log("Case 3 - Guard");
                Debug.Log("case 3 rockPos is - " + rockPos[rockCurrent].x + ", " + rockPos[rockCurrent].y);
                HapticController.Play(drawHap);
                fltText.Value = "Guard";
                int guardSelect;

                if (rockCurrent % 2 == 1)
                {
                    if (Random.Range(0f, 1f) < 0.5f)
                        guardSelect = 1;
                    else
                        guardSelect = 3;
                }
                else
                {
                    guardSelect = 2;
                }

                Random.InitState((int)System.DateTime.Now.Ticks);
                switch (guardSelect)
                {
                    case 1:
                        placeSelector = Random.Range(0, 3);
                        break;
                    case 2:
                        placeSelector = Random.Range(3, 6);
                        break;
                    case 3:
                        placeSelector = Random.Range(6, 9);
                        break;
                    default:
                        placeSelector = 10;
                        break;
                }
                if (placeSelector != 10)
                {
                    guardCounter++;
                }
                rockPos[rockCurrent] = placePos[placeSelector]
                    + (Random.insideUnitCircle
                    * Random.Range(0f, 1.5f - (0.01f * activeCharStats.guardAccuracy.GetValue())));
                break;
                #endregion
            case 4:
                #region Takeout
                //takeOut check
                Debug.Log("Takeout Selector - " + takeOutSelector);
                lastShotWasTakeout = true;
                HapticController.Play(hitHap);
                fltText.Value = "Takeout";
                //fltFdbk.PlayFeedbacks(lastTakeoutTarget.transform.position);
                lastTakeoutTarget = null;
                if (Random.Range(0f, 100f) < activeCharStats.takeOutAccuracy.GetValue())
                {
                    //placeSelector = 9;
                    rockPos[rockCurrent] = rockPos[takeOutSelector]
                        + (Random.insideUnitCircle * (1.5f - (0.005f * activeCharStats.takeOutAccuracy.GetValue())));
                    Debug.Log("Hit and Roll Check - SUCCESS");
                    houseCount++;
                }
                else
                {
                    placeSelector = 10;
                    rockPos[rockCurrent] = placePos[placeSelector];
                    Debug.Log("Hit and Roll Check - FAIL");
                }

                Random.InitState((int)System.DateTime.Now.Ticks);

                if (Random.Range(0f, 100f) < activeCharStats.takeOutAccuracy.GetValue())
                {
                    rockPos[takeOutSelector] = placePos[10];
                    Debug.Log("Opponent Rock Out of Play Check - SUCCESS");
                    houseCount--;
                    if (rockPos[rockCurrent] == placePos[10])
                    {
                        fltText.Value = "HIT";
                    }
                }
                else
                {
                    rockPos[takeOutSelector] += 
                        (Random.insideUnitCircle * (1.5f - (0.01f * activeCharStats.takeOutAccuracy.GetValue())));
                    Debug.Log("Opponent Rock Out of Play Check - FAIL");
                    if (rockPos[rockCurrent] == placePos[10])
                    {
                        fltText.Value = "MISS";
                    }
                }
                if (takeOutSelector != 99 && takeOutSelector < gm.rockList.Count)
                {
                    lastTakeoutTarget = gm.rockList[takeOutSelector].rock;
                }
                Debug.Log("Case 4 - Takeout - " + takeOutSelector);
                break;
                #endregion
            case 5:
                #region Freeze
                Debug.Log("Case 5 - Freeze - " + takeOutSelector);
                fltText.Value = "Freeze";
                //Freeze check
                if (Random.Range(0f, 100f) < activeCharStats.drawAccuracy.GetValue())
                {
                    rockPos[rockCurrent].y = rockPos[takeOutSelector].y - 0.25f;
                    rockPos[rockCurrent].x = rockPos[takeOutSelector].x;
                    rockPos[rockCurrent] = rockPos[rockCurrent] + (Random.insideUnitCircle
                            * (0.5f - (0.005f * activeCharStats.drawAccuracy.GetValue())));
                    Debug.Log("Close Freeze Check - SUCCESS");
                    
                }
                else
                {
                    rockPos[rockCurrent].y = rockPos[takeOutSelector].y - 0.25f;
                    rockPos[rockCurrent].x = rockPos[takeOutSelector].x;
                    rockPos[rockCurrent] = rockPos[rockCurrent] + (Random.insideUnitCircle
                            * (2f - (0.01f * activeCharStats.drawAccuracy.GetValue())));
                    Debug.Log("Close Freeze Check - FAIL");
                }
                houseCount++;

                Random.InitState((int)System.DateTime.Now.Ticks);

                if (Random.Range(0f, 100f) < activeCharStats.drawAccuracy.GetValue())
                {
                    rockPos[takeOutSelector].y += 0.5f - (0.005f * activeCharStats.drawAccuracy.GetValue());
                    Debug.Log("Opponent Freeze Check - SUCCESS");
                }
                else
                {
                    rockPos[takeOutSelector].x += Random.Range(0f, 0.1f * activeCharStats.drawAccuracy.GetValue());
                    rockPos[takeOutSelector].y += 1.5f - (0.005f * activeCharStats.drawAccuracy.GetValue());
                    Debug.Log("Opponent Freeze Check - FAIL");
                }
                break;
                #endregion
            case 6:
                #region Manual Guard
                Debug.Log("Case 6 - Guard");
                Debug.Log("case 6 rockPos is - " + rockPos[rockCurrent].x + ", " + rockPos[rockCurrent].y);

                switch (playerSelection)
                {
                    case 1:
                        placeSelector = Random.Range(0, 3);
                        break;
                    case 2:
                        placeSelector = Random.Range(3, 6);
                        break;
                    case 3:
                        placeSelector = Random.Range(6, 9);
                        break;
                    default:
                        placeSelector = 10;
                        break;
                }
                if (placeSelector != 10)
                {
                    guardCounter++;
                }
                rockPos[rockCurrent] = placePos[placeSelector] * Random.Range(0f, 1.5f - (0.01f * activeCharStats.guardAccuracy.GetValue()));
                break;
            #endregion

            case 7:
                #region Mixed Setup
                Debug.Log("Case 7 - MixedSetup");

                switch (playerSelection)
                {
                    case 1:
                        placeSelector = 9;
                        rockPos[rockCurrent] = placePos[placeSelector] + new Vector2(0f, 0.5f);
                        break;
                    case 2:
                        placeSelector = 7;
                        rockPos[rockCurrent] = placePos[placeSelector];
                        break;
                    case 3:
                        placeSelector = 10;
                        rockPos[rockCurrent] = placePos[placeSelector];
                        break;
                    default:
                        placeSelector = 10;
                        break;
                }

                Debug.Log("case 7 rockPos is - " + rockPos[rockCurrent].x + ", " + rockPos[rockCurrent].y);
                StartCoroutine(CompletePlacement());
                break;
                #endregion
            default:
                #region Out
                Debug.Log("Default - Out - " + rockCurrent);
                Debug.Log("Case " + shotSelector + " - " + rockCurrent);
                placeSelector = 10;
                rockPos[rockCurrent] = placePos[placeSelector];
                Debug.Log("Default rockPos is - " + rockPos[rockCurrent].x + ", " + rockPos[rockCurrent].y);
                break;
                #endregion
        }

        //gm.cm.HouseView();
        //playerStratGO.SetActive(false);
        Debug.Log("Rock " + rockCurrent + " is placed");
        placed = true;

        StartCoroutine(CompletePlacement());
    }

    void TakeOutTarget(string activeTeamName, string otherTeamName, string targetRange, out bool hit, out int takeOutSelector)
    {
        hit = false;
        takeOutSelector = 99;

        Random.InitState(System.DateTime.Now.Millisecond);

        switch (targetRange)
        {
            case "House":
                foreach (House_List rock in gm.houseList)
                {
                    if (!hit && rock.rockInfo.teamName == otherTeamName)
                    {
                        bool guarded = false;
                        //guard check
                        foreach (Guard_List guard in gm.gList)
                        {
                            if (!guarded
                                && Mathf.Abs(guard.lastTransform.position.x - rock.rock.transform.position.x) < 0.5f)
                            {
                                guarded = true;
                            }
                        }
                        //if not guarded, that's the target
                        if (!guarded)
                        {
                            hit = true;
                            takeOutSelector = rock.rockInfo.rockIndex;
                            Debug.Log("Takeout Selector " + rockCurrent + " - " + takeOutSelector);
                        }
                    }
                }
                Debug.Log("Takeout Target - House - TakeoutSelector - " + takeOutSelector);
                break;

            case "Guards":

                if (rockCurrent % 2 == 1)
                {
                    foreach (Guard_List rock in gm.gList)
                    {
                        if (!hit && Mathf.Abs(rock.lastTransform.position.x) < 0.75f)
                        {
                            hit = true;
                            takeOutSelector = rock.rockIndex;
                        }
                    }
                }
                else
                {
                    foreach (Guard_List rock in gm.gList)
                    {
                        if (!hit && Mathf.Abs(rock.lastTransform.position.x) > 0.5f)
                        {
                            hit = true;
                            takeOutSelector = rock.rockIndex;
                        }
                    }
                }

                Debug.Log("Takeout Target - Guards - TakeoutSelector - " + takeOutSelector);
                break;

            case "All":

                foreach (Rock_List rock in gm.rockList)
                {
                    if (!hit && rock.rockInfo.inPlay)
                    {
                        hit = true;
                        takeOutSelector = rock.rockInfo.rockIndex;
                    }
                }
                Debug.Log("Takeout Target - All - TakeoutSelector - " + takeOutSelector);
                break;
            default:
                takeOutSelector = 99;
                Debug.Log("Takeout Target - Default - TakeoutSelector - " + takeOutSelector);
                break;
        }
    }

    int SkillCheck(string shot, int skill)
    {
        int shotSelector;

        switch (shot)
        {
            case "Guard":
                if (Random.Range(0f, 100f) <= skill)
                {
                    shotSelector = 3;
                    Debug.Log("Guard Check - SUCCESS");
                    fltText.Value = "Guard Check - SUCCESS";
                }
                else
                {
                    //RANDOM check
                    if (Random.Range(0f, 1f) < 0.5f)
                    {
                        shotSelector = 0;
                        Debug.Log("Guard Check - long - FAIL");
                        fltText.Value = "Guard Check - long - FAIL";
                    }
                    else
                    {
                        shotSelector = 1;
                        Debug.Log("Guard Check - short - FAIL");
                        fltText.Value = "Guard Check - short - FAIL";
                    }
                }
                break;

            case "Draw":
                if (Random.Range(0f, 100f) <= skill)
                {
                    shotSelector = 0;
                    Debug.Log("Draw Check - SUCCESS");
                    fltText.Value = "Draw Check - SUCCESS";
                }
                else
                {
                    //RANDOM check
                    if (Random.Range(0f, 1f) < 0.5f)
                    {
                        shotSelector = 3;
                        Debug.Log("Draw Check - short - FAIL");
                        fltText.Value = "Draw Check - short - FAIL";
                    }
                    else
                    {
                        shotSelector = 1;
                        Debug.Log("Draw Check - long - FAIL");
                        fltText.Value = "Draw Check - long - FAIL";
                    }
                }
                break;

            case "Draw Four Foot":
                if (Random.Range(0f, 100f) <= skill)
                {
                    shotSelector = 2;
                    Debug.Log("Draw Check - SUCCESS");
                    fltText.Value = "Draw Check - SUCCESS";
                }
                else
                {
                    //RANDOM check
                    if (Random.Range(0f, 1f) < 0.5f)
                    {
                        shotSelector = 3;
                        Debug.Log("Draw Check - short - FAIL");
                        fltText.Value = "Draw Check - short - FAIL";
                    }
                    else
                    {
                        shotSelector = 1;
                        Debug.Log("Draw Check - long - FAIL");
                        fltText.Value = "Draw Check - long - FAIL";
                    }
                }
                break;

            case "Takeout":
                //SKILL check
                if (Random.Range(0f, 100f) <= skill)
                {
                    shotSelector = 4;
                    Debug.Log("Takeout Check - SUCCESS");
                    fltText.Value = "Takeout Check - SUCCESS";
                }
                //SKILL check - fail
                else
                {
                    //RANDOM crash check
                    if (Random.Range(0f, 1f) < 0.25f)
                    {
                        shotSelector = 99;
                        Debug.Log("Takeout Check - crash - FAIL");
                        fltText.Value = "Takeout Check - crash - FAIL";
                    }
                    //crash check - out
                    else
                    {
                        shotSelector = 1;
                        Debug.Log("Takeout Check - out - FAIL");
                        fltText.Value = "Takeout Check - out - FAIL";
                    }
                }
                break;

            case "Freeze":
                //SKILL check
                if (Random.Range(0f, 100f) <= skill)
                {
                    shotSelector = 5;
                    Debug.Log("Freeze Check - SUCCESS");
                    fltText.Value = "Freeze Check - SUCCESS";
                }
                //SKILL check - fail
                else
                {
                    //RANDOM check
                    if (Random.Range(0f, 1f) < 0.5f)
                    {
                        shotSelector = 3;
                        Debug.Log("Freeze Check - short - FAIL");
                        fltText.Value = "Freeze Check - short - FAIL";
                    }
                    else
                    {
                        shotSelector = 1;
                        Debug.Log("Freeze Check - long - FAIL");
                        fltText.Value = "Freeze Check - long - FAIL";
                    }
                }
                break;

            default:
                shotSelector = 1;
                Debug.Log("Skill Check Default - FAIL");
                fltText.Value = "Skill Check Default - FAIL";
                break;
        }
        
        return shotSelector;
    }
    
    #region SMART PLACEMENT HELPERS
    
    /// <summary>
    /// Determine if current rock is red team (for trajectory color)
    /// </summary>
    private bool DetermineIfRedTeam()
    {
        // Even rocks (0, 2, 4...) are one team, odd (1, 3, 5...) are other
        // Which team depends on who has hammer
        if (rockCurrent % 2 == 0)
        {
            return !gm.redHammer; // Even rocks are red if red doesn't have hammer
        }
        else
        {
            return gm.redHammer; // Odd rocks are red if red has hammer
        }
    }
    
    /// <summary>
    /// Get all rocks currently in play (for trajectory simulation obstacles)
    /// </summary>
    private List<GameObject> GetRocksInPlay()
    {
        List<GameObject> rocks = new List<GameObject>();
        
        for (int i = 0; i < rockCurrent; i++)
        {
            if (i < gm.rockList.Count && gm.rockList[i].rockInfo.inPlay)
            {
                rocks.Add(gm.rockList[i].rock);
            }
        }
        
        return rocks;
    }
    
    /// <summary>
    /// Get accuracy stat for shot type
    /// </summary>
    private float GetAccuracyForShot(string shotType, CharacterStats stats)
    {
        if (stats == null) return 70f;
        
        switch (shotType)
        {
            case "Guard":
            case "Centre Guard":
            case "Corner Guard":
                return stats.guardAccuracy.GetValue();
                
            case "Take Out":
            case "Freeze":
                return stats.takeOutAccuracy.GetValue();
                
            case "Draw":
            case "Draw Four Foot":
            default:
                return stats.drawAccuracy.GetValue();
        }
    }
    
    /// <summary>
    /// Get base error for shot type
    /// </summary>
    private float GetBaseErrorForShot(string shotType)
    {
        switch (shotType)
        {
            case "Guard":
            case "Centre Guard":
            case "Corner Guard":
                return 0.15f;
            case "Take Out":
                return 0.35f;
            case "Draw":
            case "Draw Four Foot":
                return 0.12f;
            case "Freeze":
                return 0.10f;
            default:
                return 0.15f;
        }
    }
    
    /// <summary>
    /// Apply accuracy error (realistic curling distribution)
    /// Most errors are in WEIGHT (Y-axis), with minor LINE errors (X-axis)
    /// </summary>
    private Vector2 GetAccuracyError(float accuracy, float baseMaxError)
    {
        float accuracyRatio = Mathf.Clamp01(accuracy / 100f);
        float maxError = baseMaxError * (1f - accuracyRatio);
        
        // REALISTIC CURLING ERROR DISTRIBUTION:
        // Weight (Y) errors are 4-5x more common than line (X) errors
        // Professional curlers can control line very well, but weight is hard!
        
        // Generate separate X and Y errors
        float yError = Random.Range(-maxError, maxError); // Full range for weight
        float xError = Random.Range(-maxError * 0.2f, maxError * 0.2f); // 20% range for line
        
        // Result: Y errors dominate (80% of variance)
        // X errors are minimal (20% of variance)
        return new Vector2(xError, yError);
    }
    
    /// <summary>
    /// Determine shot type based on current game state (simplified strategy)
    /// </summary>
    private string DetermineSmartShotTypeSimple(bool isBehind, bool hasHammer, int rocksInHouse, int guardsInPlay)
    {
        // SPECIAL CASE: 0-0 tie - randomly choose aggressive or conservative
        GameSettingsPersist gsp = FindFirstObjectByType<GameSettingsPersist>();
        if (gsp.redScore == 0 && gsp.yellowScore == 0)
        {
            bool playAggressive = Random.value < 0.5f; // 50/50 split
            Debug.Log($"[SmartPlacement] 0-0 tie: playing {(playAggressive ? "AGGRESSIVE" : "CONSERVATIVE")}");
            
            if (playAggressive)
            {
                // Aggressive: build position early, attack later
                if (rockCurrent < 4)
                    return "Draw";
                else if (rocksInHouse > 0)
                    return "Take Out";
                else
                    return "Guard";
            }
            else
            {
                // Conservative: guards early, careful draws
                if (rockCurrent < 4)
                    return guardsInPlay < 2 ? "Guard" : "Draw";
                else if (rocksInHouse > 2)
                    return "Take Out";
                else
                    return "Guard";
            }
        }
        
        // Early rocks: build position
        if (rockCurrent < 4)
        {
            if (rocksInHouse == 0)
                return "Draw";
            else if (guardsInPlay < 2)
                return "Guard";
            else
                return "Draw";
        }
        
        // Late rocks: more aggressive
        if (rockCurrent > 12)
        {
            if (rocksInHouse > 0)
                return "Take Out";
            else
                return "Draw";
        }
        
        // Mid-game: balanced
        if (isBehind)
        {
            // Behind: aggressive
            if (rocksInHouse > 0)
                return "Take Out";
            else if (guardsInPlay < 2)
                return "Guard";
            else
                return "Draw";
        }
        else
        {
            // Ahead or tied: build position
            if (guardsInPlay < 2 && rocksInHouse < 3)
                return "Guard";
            else if (rocksInHouse > 3)
                return "Take Out";
            else
                return "Draw";
        }
    }
    
    /// <summary>
    /// Calculate draw position using FULL PHYSICS SIMULATION
    /// Simulates the actual shot trajectory to get realistic final position
    /// </summary>
    private Vector2 CalculateDrawPosition(CharacterStats stats)
    {
        if (trajectorySimulator == null)
        {
            // Fallback to simple calculation
            Vector2 target = placePos[9]; // Button
            if (gm.houseList.Count > 3)
            {
                target += Random.insideUnitCircle * 0.5f;
            }
            return target;
        }
        
        // STEP 1: Determine target location
        Vector2 desiredTarget;
        
        if (gm.houseList.Count > 3)
        {
            // Crowded house: aim for four-foot circle
            desiredTarget = placePos[9]; // Button
        }
        else if (gm.gList.Count > 1)
        {
            // Guards present: draw behind them
            desiredTarget = new Vector2(0f, 7.0f); // Back of house
        }
        else
        {
            // Open house: button
            desiredTarget = placePos[9];
        }
        
        // STEP 2: Calculate velocity needed to reach target
        Vector2 launcherPos = new Vector2(0f, -25f);
        List<GameObject> obstacles = GetRocksInPlay();
        
        Vector2 bestFinalPos = desiredTarget;
        float bestScore = -1000f;
        List<Vector2> bestPath = null; // Track best path for visualization
        
        // Try both turn directions
        foreach (bool tryInTurn in new[] { true, false })
        {
            // Try a few lateral offsets to find clearest path
            for (float lateralOffset = -0.1f; lateralOffset <= 0.1f; lateralOffset += 0.05f)
            {
                Vector2 aimPoint = desiredTarget + new Vector2(lateralOffset, 0f);
                
                // Calculate velocity
                Vector2 velocity = trajectorySimulator.CalculateVelocityToTarget(
                    launcherPos,
                    aimPoint,
                    tryInTurn
                );
                
                // Simulate full trajectory
                List<Vector2> path = trajectorySimulator.SimulateTrajectory(
                    launcherPos,
                    velocity,
                    tryInTurn,
                    250,
                    obstacles,
                    forPlayerPreview: false
                );
                
                if (path.Count == 0) continue;
                
                Vector2 finalPos = path[path.Count - 1];
                
                // Score this path
                float score = ScoreDrawPath(path, desiredTarget, obstacles);
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestFinalPos = finalPos;
                    bestPath = path; // Save best path
                }
            }
        }
        
        // Draw trajectory visualization
        if (bestPath != null)
        {
            bool isRedTeam = DetermineIfRedTeam();
            DrawTrajectoryForPlacedRock(bestPath, isRedTeam);
        }
        
        Debug.Log($"[SmartPlacement] Physics draw: target={desiredTarget}, final={bestFinalPos}, score={bestScore:F2}");
        
        return bestFinalPos;
    }
    
    /// <summary>
    /// Score a draw path based on quality
    /// </summary>
    private float ScoreDrawPath(List<Vector2> path, Vector2 target, List<GameObject> obstacles)
    {
        if (path.Count == 0) return -1000f;
        
        Vector2 finalPos = path[path.Count - 1];
        
        // Base score: distance to target (closer = better)
        float distanceToTarget = Vector2.Distance(finalPos, target);
        float score = 10f - distanceToTarget * 2f;
        
        // BONUS: Rock stops in house
        if (finalPos.y > 5.0f && finalPos.y < 8.0f)
        {
            score += 3f;
        }
        
        // BONUS: Close to button
        float distanceToButton = Vector2.Distance(finalPos, placePos[9]);
        if (distanceToButton < 0.5f)
        {
            score += 2f;
        }
        
        // PENALTY: Hit any rocks along path
        bool hitRock = false;
        foreach (var rock in obstacles)
        {
            if (rock == null) continue;
            
            foreach (var point in path)
            {
                if (Vector2.Distance(point, rock.transform.position) < 0.28f)
                {
                    hitRock = true;
                    score -= 3f;
                    break;
                }
            }
            if (hitRock) break;
        }
        
        // PENALTY: Goes out of play
        if (finalPos.y > 8.0f)
        {
            score -= 10f;
        }
        
        return score;
    }
    
    /// <summary>
    /// Calculate guard position using FULL PHYSICS SIMULATION
    /// Simulates the actual shot trajectory to place guard realistically
    /// </summary>
    private Vector2 CalculateGuardPosition(CharacterStats stats)
    {
        // STEP 1: Determine guard strategy
        int guardSelect;
        
        if (rockCurrent % 2 == 1)
        {
            guardSelect = Random.value < 0.5f ? 1 : 3; // Left or Right
        }
        else
        {
            guardSelect = 2; // Center
        }
        
        int placeSelector;
        switch (guardSelect)
        {
            case 1:
                placeSelector = Random.Range(0, 3); // Left guards
                break;
            case 2:
                placeSelector = Random.Range(3, 6); // Center guards
                break;
            case 3:
                placeSelector = Random.Range(6, 9); // Right guards
                break;
            default:
                placeSelector = 4;
                break;
        }
        
        Vector2 targetGuardPos = placePos[placeSelector];
        
        // IMPROVED CORNER GUARD POSITIONS: Make them actually block!
        // Old corner guards at (1.33, 1.22) don't block anything
        // New positions at (~0.75, 3.0) create effective blocks
        if (guardSelect == 1) // Left guards
        {
            // Place between center line and house, good Y coverage
            targetGuardPos = new Vector2(
                Random.Range(-0.85f, -0.65f), // X: Left side blocking position
                Random.Range(2.8f, 3.2f)       // Y: Guard zone (blocks path to house)
            );
        }
        else if (guardSelect == 3) // Right guards
        {
            // Mirror left guards
            targetGuardPos = new Vector2(
                Random.Range(0.65f, 0.85f),    // X: Right side blocking position
                Random.Range(2.8f, 3.2f)       // Y: Guard zone
            );
        }
        else // Center guards (guardSelect == 2)
        {
            // Keep center guard logic (already good)
            targetGuardPos = placePos[placeSelector];
        }
        
        // STEP 2: If physics available, simulate the shot
        if (trajectorySimulator != null)
        {
            Vector2 launcherPos = new Vector2(0f, -25f);
            List<GameObject> obstacles = GetRocksInPlay();
            
            Vector2 bestFinalPos = targetGuardPos;
            float bestScore = -1000f;
            List<Vector2> bestPath = null; // Track best path for visualization
            
            // Try both turn directions
            foreach (bool tryInTurn in new[] { true, false })
            {
                // Calculate velocity for guard shot (lighter weight)
                Vector2 velocity = trajectorySimulator.CalculateVelocityToTarget(
                    launcherPos,
                    targetGuardPos,
                    tryInTurn
                );
                
                // Guards are lighter weight - reduce velocity
                velocity *= 0.85f;
                
                // Simulate trajectory
                List<Vector2> path = trajectorySimulator.SimulateTrajectory(
                    launcherPos,
                    velocity,
                    tryInTurn,
                    250,
                    obstacles,
                    forPlayerPreview: false
                );
                
                if (path.Count == 0) continue;
                
                Vector2 finalPos = path[path.Count - 1];
                
                // Score this path
                float score = ScoreGuardPath(path, targetGuardPos, obstacles);
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestFinalPos = finalPos;
                    bestPath = path; // Save best path
                }
            }
            
            // Draw trajectory visualization
            if (bestPath != null)
            {
                bool isRedTeam = DetermineIfRedTeam();
                DrawTrajectoryForPlacedRock(bestPath, isRedTeam);
            }
            
            Debug.Log($"[SmartPlacement] Physics guard: target={targetGuardPos}, final={bestFinalPos}, score={bestScore:F2}");
            
            return bestFinalPos;
        }
        
        // Fallback: simple placement
        return targetGuardPos;
    }
    
    /// <summary>
    /// Score a guard path based on quality
    /// </summary>
    private float ScoreGuardPath(List<Vector2> path, Vector2 target, List<GameObject> obstacles)
    {
        if (path.Count == 0) return -1000f;
        
        Vector2 finalPos = path[path.Count - 1];
        
        // Base score: distance to target
        float distanceToTarget = Vector2.Distance(finalPos, target);
        float score = 10f - distanceToTarget * 3f;
        
        // BONUS: Rock stops in guard zone (Y between 3.0 and 6.5)
        if (finalPos.y > 3.0f && finalPos.y < 6.5f)
        {
            score += 5f;
        }
        
        // BONUS: Not too close to existing guards (spacing)
        bool tooClose = false;
        foreach (var rock in obstacles)
        {
            if (rock == null) continue;
            
            float dist = Vector2.Distance(finalPos, rock.transform.position);
            if (dist < 0.5f)
            {
                tooClose = true;
                score -= 2f;
            }
        }
        
        // PENALTY: Goes into house (too long)
        if (finalPos.y > 6.5f)
        {
            score -= 5f;
        }
        
        // PENALTY: Too short (doesn't reach guard zone)
        if (finalPos.y < 3.0f)
        {
            score -= 8f;
        }
        
        // PENALTY: Goes out of play
        if (finalPos.y > 8.0f)
        {
            score -= 15f;
        }
        
        return score;
    }
    
    /// <summary>
    /// Calculate takeout positions using ENHANCED PHYSICS
    /// Simulates hit-and-roll outcomes based on collision physics
    /// </summary>
    private bool CalculateTakeoutPositions(int targetRockIndex, CharacterStats stats, out Vector2 shooterPos, out Vector2 targetPos)
    {
        shooterPos = placePos[10]; // Default: out
        targetPos = placePos[10];
        
        if (targetRockIndex < 0 || targetRockIndex >= rockPos.Length)
            return false;
        
        Vector2 targetRockPos = rockPos[targetRockIndex];
        
        // Check if we have physics simulator
        if (trajectorySimulator != null)
        {
            // PHYSICS-BASED TAKEOUT SIMULATION
            Vector2 launcherPos = new Vector2(0f, -25f);
            List<GameObject> obstacles = GetRocksInPlay();
            
            float bestScore = -1000f;
            bool foundGoodShot = false;
            List<Vector2> bestPath = null; // Track best path for visualization
            
            // Try different approaches
            foreach (bool tryInTurn in new[] { true, false })
            {
                // Try aiming directly at target
                Vector2 velocity = trajectorySimulator.CalculateVelocityToTarget(
                    launcherPos,
                    targetRockPos,
                    tryInTurn
                );
                
                // Takeout weight (faster than draw)
                velocity *= 1.15f;
                
                // Simulate trajectory
                List<Vector2> path = trajectorySimulator.SimulateTrajectory(
                    launcherPos,
                    velocity,
                    tryInTurn,
                    250,
                    obstacles,
                    forPlayerPreview: false
                );
                
                if (path.Count == 0) continue;
                
                // Check if we hit the target
                bool hitTarget = false;
                int collisionIndex = -1;
                
                for (int i = 0; i < path.Count; i++)
                {
                    float distToTarget = Vector2.Distance(path[i], targetRockPos);
                    if (distToTarget < 0.28f) // Rock diameter
                    {
                        hitTarget = true;
                        collisionIndex = i;
                        break;
                    }
                }
                
                if (hitTarget)
                {
                    // COLLISION PHYSICS: Calculate post-collision positions
                    Vector2 collisionPoint = path[collisionIndex];
                    Vector2 approachDir = (collisionPoint - targetRockPos).normalized;
                    
                    // Shooter rock continues forward (reduced momentum)
                    Vector2 shooterFinal = collisionPoint + approachDir * 0.4f;
                    
                    // Target rock pushed forward
                    Vector2 targetFinal = targetRockPos + approachDir * 0.6f;
                    
                    // Apply skill check
                    float hitChance = stats.takeOutAccuracy.GetValue();
                    bool successfulHit = Random.Range(0f, 100f) < hitChance;
                    
                    if (successfulHit)
                    {
                        // Good hit - target likely removed
                        if (Random.Range(0f, 100f) < hitChance * 0.8f)
                        {
                            targetFinal = placePos[10]; // Out of play
                        }
                        
                        // Score this outcome
                        float score = 10f;
                        if (targetFinal == placePos[10])
                            score += 5f; // Bonus for removing target
                        
                        if (score > bestScore)
                        {
                            bestScore = score;
                            shooterPos = shooterFinal;
                            targetPos = targetFinal;
                            foundGoodShot = true;
                            bestPath = path; // Save best path
                        }
                    }
                }
            }
            
            // Draw trajectory visualization
            if (bestPath != null && foundGoodShot)
            {
                bool isRedTeam = DetermineIfRedTeam();
                DrawTrajectoryForPlacedRock(bestPath, isRedTeam);
            }
            
            if (foundGoodShot)
            {
                Debug.Log($"[SmartPlacement] Physics takeout: shooter={shooterPos}, target={targetPos}, score={bestScore:F2}");
                return true;
            }
        }
        
        // FALLBACK: Simple skill-based calculation
        if (Random.Range(0f, 100f) < stats.takeOutAccuracy.GetValue())
        {
            // Hit and roll - shooter stays near target
            shooterPos = targetRockPos + Random.insideUnitCircle * 0.5f;
        }
        else
        {
            // Miss - shooter goes out
            shooterPos = placePos[10];
        }
        
        // Target rock position based on accuracy
        if (Random.Range(0f, 100f) < stats.takeOutAccuracy.GetValue())
        {
            // Target removed
            targetPos = placePos[10];
        }
        else
        {
            // Target moved but stays in
            targetPos = targetRockPos + Random.insideUnitCircle * 0.75f;
        }
        
        return true;
    }
    
    /// <summary>
    /// Find best opponent rock to take out (ENHANCED with path checking)
    /// Considers guards and shot difficulty
    /// </summary>
    private int FindBestTakeoutTarget(string activeTeam)
    {
        int bestTarget = -1;
        float bestScore = -1000f;
        
        // Evaluate each opponent rock in house
        foreach (var houseRock in gm.houseList)
        {
            if (houseRock.rockInfo.teamName == activeTeam)
                continue; // Skip own rocks
            
            int rockIndex = houseRock.rockInfo.rockIndex;
            Vector2 rockPos = houseRock.rock.transform.position;
            
            float score = 0f;
            
            // BONUS: Closer rocks are better (shot rock likely to stay)
            float distanceToButton = Vector2.Distance(rockPos, placePos[9]);
            score += (2.0f - distanceToButton) * 2f;
            
            // BONUS: Rock is unguarded
            bool guarded = false;
            float guardDistance = 999f;
            
            foreach (var guard in gm.gList)
            {
                float lateralDist = Mathf.Abs(guard.lastTransform.position.x - rockPos.x);
                if (lateralDist < 0.5f)
                {
                    guarded = true;
                    guardDistance = Mathf.Min(guardDistance, Vector2.Distance(guard.lastTransform.position, rockPos));
                }
            }
            
            if (!guarded)
            {
                score += 5f; // Big bonus for unguarded
            }
            else
            {
                // Penalty based on how well guarded
                score -= (3f / guardDistance); // Closer guard = harder shot
            }
            
            // BONUS: If this is shot rock (closest)
            if (gm.houseList.Count > 0 && gm.houseList[0].rockInfo.rockIndex == rockIndex)
            {
                score += 3f; // Priority: remove shot rock
            }
            
            // PHYSICS CHECK: Can we actually hit this rock?
            if (trajectorySimulator != null)
            {
                Vector2 launcherPos = new Vector2(0f, -25f);
                List<GameObject> obstacles = GetRocksInPlay();
                
                bool canHit = false;
                
                // Try both turns
                foreach (bool tryInTurn in new[] { true, false })
                {
                    Vector2 velocity = trajectorySimulator.CalculateVelocityToTarget(
                        launcherPos,
                        rockPos,
                        tryInTurn
                    );
                    
                    velocity *= 1.15f; // Takeout weight
                    
                    List<Vector2> path = trajectorySimulator.SimulateTrajectory(
                        launcherPos,
                        velocity,
                        tryInTurn,
                        100, // Shorter sim for speed
                        obstacles,
                        forPlayerPreview: false
                    );
                    
                    // Check if path gets close to target
                    foreach (var point in path)
                    {
                        if (Vector2.Distance(point, rockPos) < 0.35f)
                        {
                            canHit = true;
                            break;
                        }
                    }
                    
                    if (canHit) break;
                }
                
                if (!canHit)
                {
                    score -= 10f; // Huge penalty if we can't physically hit it
                }
            }
            
            Debug.Log($"[SmartPlacement] Target eval: rock {rockIndex} @ {rockPos}, guarded={guarded}, score={score:F2}");
            
            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = rockIndex;
            }
        }
        
        if (bestTarget >= 0)
        {
            Debug.Log($"[SmartPlacement] Best takeout target: rock {bestTarget}, score={bestScore:F2}");
        }
        
        return bestTarget;
    }
    
    #endregion
    
    #region TRAJECTORY VISUALIZATION
    
    /// <summary>
    /// Draw a trajectory line for a placed rock's simulated path
    /// </summary>
    private void DrawTrajectoryForPlacedRock(List<Vector2> path, bool isRedTeam)
    {
        if (!showTrajectoryLines || path == null || path.Count < 2)
            return;
        
        // Create line renderer object
        GameObject lineObj = new GameObject($"PlacementTrajectory_Rock{rockCurrent}");
        lineObj.transform.SetParent(transform);
        
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
        
        // Configure line appearance
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = isRedTeam ? redTeamTrajectoryColor : yellowTeamTrajectoryColor;
        lineRenderer.endColor = isRedTeam ? redTeamTrajectoryColor : yellowTeamTrajectoryColor;
        lineRenderer.startWidth = trajectoryLineWidth;
        lineRenderer.endWidth = trajectoryLineWidth;
        lineRenderer.positionCount = path.Count;
        lineRenderer.useWorldSpace = true;
        lineRenderer.sortingLayerName = "Default";
        lineRenderer.sortingOrder = 5; // Above rocks
        
        // Set positions
        Vector3[] positions = new Vector3[path.Count];
        for (int i = 0; i < path.Count; i++)
        {
            positions[i] = new Vector3(path[i].x, path[i].y, -0.1f); // Slightly in front
        }
        lineRenderer.SetPositions(positions);
        
        // Track for cleanup
        activeTrajectoryLines.Add(lineObj);
        
        // Fade out and destroy after duration
        StartCoroutine(FadeAndDestroyTrajectory(lineObj, lineRenderer));
        
        Debug.Log($"[TrajectoryViz] Drew trajectory for rock {rockCurrent}: {path.Count} points, team={(isRedTeam ? "Red" : "Yellow")}");
    }
    
    /// <summary>
    /// Fade out and destroy a trajectory line
    /// </summary>
    private IEnumerator FadeAndDestroyTrajectory(GameObject lineObj, LineRenderer lineRenderer)
    {
        float elapsed = 0f;
        Color startColor = lineRenderer.startColor;
        Color endColor = lineRenderer.endColor;
        
        // Wait for display duration
        yield return new WaitForSeconds(trajectoryDisplayDuration);
        
        // Fade out over 0.5 seconds
        float fadeTime = 0.5f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeTime);
            
            lineRenderer.startColor = new Color(startColor.r, startColor.g, startColor.b, startColor.a * alpha);
            lineRenderer.endColor = new Color(endColor.r, endColor.g, endColor.b, endColor.a * alpha);
            
            yield return null;
        }
        
        // Cleanup
        activeTrajectoryLines.Remove(lineObj);
        Destroy(lineObj);
    }
    
    /// <summary>
    /// Clear all active trajectory lines
    /// </summary>
    private void ClearAllTrajectories()
    {
        foreach (var lineObj in activeTrajectoryLines)
        {
            if (lineObj != null)
                Destroy(lineObj);
        }
        activeTrajectoryLines.Clear();
    }
    
    #endregion
}
