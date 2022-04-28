using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRockPlacerment : MonoBehaviour
{
    public GameManager gm;
    public RockManager rm;
    public TeamManager tm;
    public CameraManager camera;
    public Transform house;
    public bool placed;
    public bool placed1;
    public int rockCurrent;

    public Vector2[] placePos;
    public Vector2[] rockPos;

    public GameObject playerStratGO;
    public bool aggressive;
    int playerSelection;
    public int round;

    public GameObject dialogueGO;
    public DialogueTrigger coachDialogue;
    public DialogueTrigger announDialogue;

    int guardCounter = 0;
    int houseCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        placed = false;
        rockPos = new Vector2[gm.rockCurrent];
    }

    public void OnRockPlace(int rockCrnt, bool redTeam)
    {
        placed = false;
        rockCurrent = rockCrnt;

        //StartCoroutine(RandomRockPlace());
        StartCoroutine(StratSelect(redTeam, true));

        #region Strategic Selection
        //if (redTeam)
        //{
        //    if (gm.redHammer)
        //    {
        //        if (rockCurrent % 2 == 0)
        //            StartCoroutine(StratSelect(redTeam, true));
        //        else
        //            StartCoroutine(StratSelect(redTeam, false));
        //    }
        //    else
        //    {
        //        if (rockCurrent % 2 == 0)
        //            StartCoroutine(StratSelect(redTeam, false));
        //        else
        //            StartCoroutine(StratSelect(redTeam, true));
        //    }
        //}
        //else
        //{
        //    if (gm.redHammer)
        //    {
        //        if (rockCurrent % 2 == 0)
        //            StartCoroutine(StratSelect(redTeam, false));
        //        else
        //            StartCoroutine(StratSelect(redTeam, true));
        //    }
        //    else
        //    {
        //        if (rockCurrent % 2 == 0)
        //            StartCoroutine(StratSelect(redTeam, true));
        //        else
        //            StartCoroutine(StratSelect(redTeam, false));
        //    }
        //}
        #endregion
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
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
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
                                    + (Random.insideUnitCircle * ((1 - (0.09f * gsp.cStats.drawAccuracy)) * 1.25f));
                            else
                                rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 1.25f);
                        }
                        else if (!gm.redHammer && i % 2 == 0)
                        {
                            houseRed++;
                            if (gsp.aiRed)
                                rockPos[i] = placePos[placeSelector]
                                    + (Random.insideUnitCircle * ((1 - (0.09f * gsp.cStats.drawAccuracy)) * 1.25f));
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

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < rockCurrent + 1; i++)
        {
            gm.rockList[i].rock.GetComponent<CircleCollider2D>().radius = 0.14f;
            gm.rockList[i].rock.GetComponent<SpriteRenderer>().enabled = false;
            gm.rockList[i].rock.GetComponent<SpringJoint2D>().enabled = false;
            gm.rockList[i].rock.GetComponent<Rock_Flick>().enabled = false;
            gm.rockList[i].rock.transform.parent = null;
            //rm.rb.DeadRock(i);
            yield return new WaitForEndOfFrame();
            Debug.Log("Rock Position " + i + " " + rockPos[i].x + ", " + rockPos[i].y);
            gm.rockList[i].rock.GetComponent<Rigidbody2D>().position = rockPos[i];

            gm.rockList[i].rock.GetComponent<CircleCollider2D>().enabled = true;
            gm.rockList[i].rock.GetComponent<Rock_Release>().enabled = true;
            gm.rockList[i].rock.GetComponent<Rock_Force>().enabled = true;
            gm.rockList[i].rock.GetComponent<Rock_Colliders>().enabled = true;
            yield return new WaitForEndOfFrame();
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
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForEndOfFrame();

        //rocksPlaced = true;
        gm.rockCurrent = rockCurrent - 1;
        gm.rockTotal = 16;
        yield return new WaitForEndOfFrame();
        //placed = true;
    }

    IEnumerator StratSelect(bool redTeam, bool aiTurn)
    {
        round++;
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        CareerManager cm = FindObjectOfType<CareerManager>();
        if (!aiTurn)
            playerStratGO.SetActive(true);

        gm.rockBar.EndUpdate(gsp.yellowScore, gsp.redScore);

        yield return new WaitForEndOfFrame();

        tm.SetCharacter(rockCurrent, true);
        tm.SetCharacter(rockCurrent, false);

        //if (!aiTurn)
        //    yield return new WaitUntil(() => !playerStratGO.activeSelf);
        Debug.Log("RockCurrent is " + rockCurrent + " and aiTurn is " + aiTurn);
        //if (rockCurrent == 8 | rockCurrent == 12)
        //    round++;
        if (rockCurrent < gm.rockCurrent + 1)
            yield return StartCoroutine(Placement(redTeam, aiTurn));
        else
            playerStratGO.SetActive(false);

        yield return new WaitForEndOfFrame();
    }

    IEnumerator Placement(bool redTeam, bool aiTurn)
    {
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        Debug.Log("RedTeam is " + redTeam);
        gm.houseList.Sort();
        //int houseCount = 0;
        bool aiAgg;

        if (redTeam)
        {
            if (gsp.yellowScore > gsp.redScore)
                aiAgg = false;
            else
                aiAgg = true;
        }
        else
        {
            if (gsp.redScore > gsp.yellowScore)
                aiAgg = false;
            else
                aiAgg = true;
        }

        Debug.Log("AI Aggressive is " + aiAgg);

        Random.InitState(System.DateTime.Now.Millisecond);

        int shotSelector = 1;
        int takeOutSelector = 99;
        int freezeSelector = 99;

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

        #region old scenario
        //if (houseCount > 3)
        //{
        //    aiAgg = false;
        //}
        //else if (activeScore < otherScore)
        //{
        //    aiAgg = true;
        //}
        //else
        //{
        //    aiAgg = false;
        //}

        //float activeLuckStat;
        //float otherLuckStat;

        //activeLuckStat = activeCharStats.drawAccuracy.GetValue() + activeCharStats.guardAccuracy.GetValue() + activeCharStats.takeOutAccuracy.GetValue();
        //activeLuckStat /= 3f;
        //otherLuckStat = otherCharStats.drawAccuracy.GetValue() + otherCharStats.guardAccuracy.GetValue() + otherCharStats.takeOutAccuracy.GetValue();
        //otherLuckStat /= 3f;

        //Debug.Log("gm.houseList.Count is " + gm.houseList.Count);
        //Debug.Log("Active Luck is " + activeLuckStat);
        //Debug.Log("Active Team is " + activeTeamName);
        //Debug.Log("Other Team is " + otherTeamName);
       
        //if (aiTurn)
        //{
        //    //and the ai chooses a defensive strategy...
        //    //aggressive on defensive - prob of player guards and rocks in house based on draw and guard accuracy
        //    //player rocks based on guard and draw accuracy, ai rocks unhindered
        //    if (!aiAgg)
        //    {
        //        Debug.Log("houseCount is " + houseCount);
        //        Debug.Log("gm.houseList is " + gm.houseList.Count);
        //        if (rockCurrent < 5)
        //        {
        //            if (houseCount > 0)
        //            {
        //                //if there's an opponent, takeout
        //                bool stopCounting = false;

        //                for (int j = 0; j < gm.rockList.Count; j++)
        //                {
        //                    if (!stopCounting && gm.rockList[j].rockInfo.teamName
        //                        == otherTeamName && gm.rockList[j].rockInfo.inHouse && !gm.rockList[j].rockInfo.outOfPlay)
        //                    {
                                
        //                        takeOutSelector = gm.rockList[j].rockInfo.rockIndex;
        //                        stopCounting = true;
        //                        Debug.Log("Takeout Selector < 5 - " + takeOutSelector);
        //                    }
        //                }

        //                if (stopCounting)
        //                {
        //                    if (Random.Range(0f, 10f) < activeCharStats.takeOutAccuracy.GetValue())
        //                    {
        //                        shotSelector = 4;
        //                        Debug.Log("Takeout - HIT - " + gm.rockList[takeOutSelector].rockInfo.teamName + " " + gm.rockList[takeOutSelector].rockInfo.rockNumber);
        //                        Debug.Log("Takeout Check - SUCCESS");
        //                        gm.gHUD.Message("Takeout Check - SUCCESS");
        //                    }
        //                    else
        //                    {
        //                        stopCounting = false;
        //                        for (int j = 0; j < gm.rockList.Count; j++)
        //                        {
        //                            if (!stopCounting && !gm.rockList[j].rockInfo.outOfPlay && j != takeOutSelector)
        //                            {
        //                                takeOutSelector = gm.rockList[j].rockInfo.rockIndex;
        //                                stopCounting = true;
        //                            }
        //                        }
        //                        if (stopCounting)
        //                        {
        //                            if (Random.Range(0f, 10f) < activeCharStats.takeOutAccuracy.GetValue())
        //                            {
        //                                shotSelector = 4;
        //                                Debug.Log("Takeout - HIT - " + gm.rockList[takeOutSelector].rockInfo.teamName + " " + gm.rockList[takeOutSelector].rockInfo.rockNumber);
        //                                Debug.Log("Takeout Check - crash - FAIL");
        //                                gm.gHUD.Message("Takeout Check - crash - FAIL");
        //                            }
        //                            else
        //                            {
        //                                shotSelector = 1;
        //                                Debug.Log("Takeout Check - out - FAIL");
        //                                gm.gHUD.Message("Takeout Check - out - FAIL");
        //                            }
        //                        }
        //                        else
        //                        {
        //                            shotSelector = 1;
        //                            Debug.Log("Takeout Check - out - FAIL");
        //                            gm.gHUD.Message("Takeout Check - out - FAIL");
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (houseCount > 2)
        //                    {
        //                        //guard
        //                        if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                        {
        //                            shotSelector = 3;
        //                            Debug.Log("No Takeout Target - Draw Check - SUCCESS");
        //                            gm.gHUD.Message("No Takeout Target - Draw Check - SUCCESS");
        //                        }
        //                        else
        //                        {
        //                            shotSelector = 1;
        //                            Debug.Log("No Takeout Target - Draw Check - FAIL");
        //                            gm.gHUD.Message("No Takeout Target - Draw Check - FAIL");
        //                        }
        //                    }
        //                    else
        //                    {
        //                        //draw to house
        //                        if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                        {
        //                            shotSelector = 3;
        //                            Debug.Log("No Takeout Target - Draw Check - SUCCESS");
        //                            gm.gHUD.Message("No Takeout Target - Draw Check - SUCCESS");
        //                        }
        //                        else
        //                        {
        //                            shotSelector = 1;
        //                            Debug.Log("No Takeout Target - Draw Check - FAIL");
        //                            gm.gHUD.Message("No Takeout Target - Draw Check - FAIL");
        //                        }
        //                    }
        //                    //draw to house
        //                    if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                    {
        //                        shotSelector = 0;
        //                        Debug.Log("No Takeout Target - Draw Check - SUCCESS");
        //                        gm.gHUD.Message("No Takeout Target - Draw Check - SUCCESS");
        //                    }
        //                    else
        //                    {
        //                        shotSelector = 1;
        //                        Debug.Log("No Takeout Target - Draw Check - FAIL");
        //                        gm.gHUD.Message("No Takeout Target - Draw Check - FAIL");
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                //draw to house
        //                if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                {
        //                    shotSelector = 0;
        //                    Debug.Log("Draw Check - SUCCESS");
        //                }
        //                else
        //                {
        //                    if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                    {
        //                        shotSelector = 3;
        //                        Debug.Log("Draw Check - short - FAIL");
        //                        gm.gHUD.Message("Draw Check - short - FAIL");
        //                    }
        //                    else
        //                    {
        //                        shotSelector = 1;
        //                        Debug.Log("Draw Check - long - FAIL");
        //                        gm.gHUD.Message("Draw Check - long - FAIL");
        //                    }
        //                }
        //            }
        //        }
        //        else if (houseCount > 0)
        //        {
        //            //if there's an opponent, takeout
        //            bool stopCounting = false;

        //            for (int j = 0; j < gm.rockList.Count; j++)
        //            {
        //                if (!stopCounting && gm.rockList[j].rockInfo.teamName
        //                        == otherTeamName && gm.rockList[j].rockInfo.inHouse && !gm.rockList[j].rockInfo.outOfPlay)
        //                {
        //                    takeOutSelector = gm.rockList[j].rockInfo.rockIndex;
        //                    stopCounting = true;
        //                }
        //            }
        //            if (stopCounting)
        //            {
        //                if (Random.Range(0f, 10f) < activeCharStats.takeOutAccuracy.GetValue())
        //                {
        //                    shotSelector = 4;
        //                    Debug.Log("Takeout - HIT - "
        //                        + gm.rockList[takeOutSelector].rockInfo.teamName + " " + gm.rockList[takeOutSelector].rockInfo.rockNumber);
        //                    Debug.Log("Takeout Check - SUCCESS");
        //                    gm.gHUD.Message("Takeout Check - SUCCESS");
                            
        //                }
        //                else
        //                {
        //                    stopCounting = false;
        //                    for (int j = 0; j < gm.rockList.Count; j++)
        //                    {
        //                        if (!stopCounting && !gm.rockList[j].rockInfo.outOfPlay && j != takeOutSelector)
        //                        {
        //                            takeOutSelector = j;
        //                            stopCounting = true;
        //                        }
        //                    }
        //                    if (stopCounting)
        //                    {
        //                        if (Random.Range(0f, 10f) < activeCharStats.takeOutAccuracy.GetValue())
        //                        {
        //                            shotSelector = 4;
        //                            Debug.Log("Takeout - HIT - " + gm.rockList[takeOutSelector].rockInfo.teamName + " " + gm.rockList[takeOutSelector].rockInfo.rockNumber);
        //                            Debug.Log("Takeout Check - crash - FAIL");
        //                            gm.gHUD.Message("Takeout Check - crash - FAIL");
        //                        }
        //                        else
        //                        {
        //                            shotSelector = 1;
        //                            Debug.Log("Takeout Check - out - FAIL");
        //                            gm.gHUD.Message("Takeout Check - out - FAIL");
        //                        }
        //                    }
        //                    else
        //                    {
        //                        shotSelector = 1;
        //                        Debug.Log("Takeout Check - No Target - out - FAIL");
        //                        gm.gHUD.Message("Takeout Check - No Target - out - FAIL");
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                //draw to house
        //                if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                {
        //                    shotSelector = 0;
        //                    Debug.Log("No Takeout Target - Draw Check - SUCCESS");
        //                    gm.gHUD.Message("No Takeout Target - Draw Check - SUCCESS");
        //                }
        //                else
        //                {
        //                    shotSelector = 1;
        //                    Debug.Log("No Takeout Target - Draw Check - FAIL");
        //                    gm.gHUD.Message("No Takeout Target - Draw Check - FAIL");
        //                }
        //            }
        //        }
        //        else if (guardCounter > 0)
        //        {

        //            //if there's an opponent guard, takeout
        //            bool stopCounting = false;
                    
        //            for (int j = 0; j < gm.rockList.Count; j++)
        //            {
        //                if (!stopCounting && gm.rockList[j].rockInfo.teamName
        //                        == otherTeamName && !gm.rockList[j].rockInfo.outOfPlay)
        //                {
        //                    takeOutSelector = j;
        //                    stopCounting = true;
        //                }
        //            }
        //            if (stopCounting)
        //            {
        //                if (Random.Range(0f, 10f) < activeCharStats.takeOutAccuracy.GetValue())
        //                {
        //                    shotSelector = 4;
        //                    Debug.Log("Takeout - HIT - " + gm.rockList[takeOutSelector].rockInfo.teamName + " " + gm.rockList[takeOutSelector].rockInfo.rockNumber);
        //                    Debug.Log("Takeout Check - SUCCESS");
        //                    gm.gHUD.Message("Takeout Check - SUCCESS");
        //                }
        //                else
        //                {
        //                    stopCounting = false;
        //                    for (int j = 0; j < gm.rockList.Count; j++)
        //                    {
        //                        if (!stopCounting && !gm.rockList[j].rockInfo.outOfPlay && j != takeOutSelector)
        //                        {
        //                            takeOutSelector = j;
        //                            stopCounting = true;
        //                        }
        //                    }
        //                    if (stopCounting)
        //                    {
        //                        if (Random.Range(0f, 10f) < activeCharStats.takeOutAccuracy.GetValue())
        //                        {
        //                            shotSelector = 4;
        //                            Debug.Log("Takeout - HIT - " + gm.rockList[takeOutSelector].rockInfo.teamName + " " + gm.rockList[takeOutSelector].rockInfo.rockNumber);
        //                            Debug.Log("Takeout Check - crash - FAIL");
        //                            gm.gHUD.Message("Takeout Check - crash - FAIL");
        //                        }
        //                        else
        //                        {
        //                            shotSelector = 1;
        //                            Debug.Log("Takeout Check - out - FAIL");
        //                            gm.gHUD.Message("Takeout Check - out - FAIL");
        //                        }
        //                    }
        //                    else
        //                    {
        //                        shotSelector = 1;
        //                        Debug.Log("Takeout Check - FAIL");
        //                        gm.gHUD.Message("Takeout Check - FAIL");
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                //draw to house
        //                if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                {
        //                    shotSelector = 0;
        //                    Debug.Log("No Takeout Target - Draw Check - SUCCESS");
        //                    gm.gHUD.Message("No Takeout Target - Draw Check - SUCCESS");
        //                }
        //                else
        //                {
        //                    if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                    {
        //                        shotSelector = 3;
        //                        Debug.Log("No Takeout Target - Draw Check - short - FAIL");
        //                        gm.gHUD.Message("No Takeout Target - Draw Check - short - FAIL");
        //                    }
        //                    else
        //                    {
        //                        shotSelector = 1;
        //                        Debug.Log("No Takeout Target - Draw Check - long - FAIL");
        //                        gm.gHUD.Message("No Takeout Target - Draw Check - long - FAIL");
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            //draw to house
        //            if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //            {
        //                shotSelector = 0;
        //                Debug.Log("Draw Check - SUCCESS");
        //                gm.gHUD.Message("Draw Check - SUCCESS");
        //            }
        //            else
        //            {
        //                if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                {
        //                    shotSelector = 3;
        //                    Debug.Log("Draw Check - short - FAIL");
        //                    gm.gHUD.Message("Draw Check - short - FAIL");
        //                }
        //                else
        //                {
        //                    shotSelector = 1;
        //                    Debug.Log("Draw Check - long - FAIL");
        //                    gm.gHUD.Message("Draw Check - long - FAIL");
        //                }
        //            }
        //        }
        //    }
        //    //and the ai chooses an aggressive strategy...
        //    //aggressive on aggressive - high prob of guards and rocks in house
        //    //player rocks based on draw and guard accuracy, ai rocks unhindered
        //    else
        //    {
        //        if (rockCurrent % 2 == 0)
        //        {
        //            if (guardCounter < 3)
        //            {
        //                //guard check
        //                if (Random.Range(0f, 10f) < activeCharStats.guardAccuracy.GetValue())
        //                {
        //                    shotSelector = 3;
        //                    Debug.Log("Guard Check - SUCCESS");
        //                    gm.gHUD.Message("Guard Check - SUCCESS");
        //                }
        //                else
        //                {
        //                    if (Random.Range(0f, 10f) < activeCharStats.guardAccuracy.GetValue())
        //                    {
        //                        shotSelector = 0;
        //                        Debug.Log("Guard Check - long - FAIL");
        //                        gm.gHUD.Message("Guard Check - long - FAIL");
        //                    }
        //                    else
        //                    {
        //                        shotSelector = 1;
        //                        Debug.Log("Guard Check - short - FAIL");
        //                        gm.gHUD.Message("Guard Check - short - FAIL");
        //                    }
        //                }
        //            }
        //            else if (houseCount < 2)
        //            {
        //                //draw check
        //                if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                {
        //                    shotSelector = 0;
        //                    Debug.Log("Draw Check - SUCCESS");
        //                }
        //                else
        //                {
        //                    if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                    {
        //                        shotSelector = 3;
        //                        Debug.Log("Draw Check - short - FAIL");
        //                        gm.gHUD.Message("Draw Check - short - FAIL");
        //                    }
        //                    else
        //                    {
        //                        shotSelector = 1;
        //                        Debug.Log("Draw Check - long - FAIL");
        //                        gm.gHUD.Message("Draw Check - long - FAIL");
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                //freeze to opponent stone if there is one
        //                bool stopCounting = false;

        //                for (int j = 0; j < gm.rockList.Count; j++)
        //                {
        //                    if (!stopCounting && gm.rockList[j].rockInfo.teamName
        //                        == otherTeamName && gm.rockList[j].rockInfo.inHouse && !gm.rockList[j].rockInfo.outOfPlay)
        //                    {
        //                        freezeSelector = j;
        //                        stopCounting = true;
        //                    }
        //                }
        //                if (stopCounting)
        //                {
        //                    if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                    {
        //                        shotSelector = 5;
        //                        Debug.Log("Freeze - TARGET - " + gm.rockList[freezeSelector].rockInfo.teamName + " " + gm.rockList[freezeSelector].rockInfo.rockNumber);
        //                        Debug.Log("Freeze Check - SUCCESS");
        //                        gm.gHUD.Message("Freeze Check - SUCCESS");
        //                    }
        //                    else
        //                    {
        //                        shotSelector = 0;
        //                        Debug.Log("Freeze Check - FAIL");
        //                        gm.gHUD.Message("Freeze Check - FAIL");
        //                    }
        //                }
        //                else
        //                {
        //                    //draw to house
        //                    if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                    {
        //                        shotSelector = 0;
        //                        Debug.Log("Draw Check - SUCCESS");
        //                        gm.gHUD.Message("Draw Check - SUCCESS");
        //                    }
        //                    else
        //                    {
        //                        if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                        {
        //                            shotSelector = 3;
        //                            Debug.Log("Draw Check - short - FAIL");
        //                            gm.gHUD.Message("Draw Check - short - FAIL");
        //                        }
        //                        else
        //                        {
        //                            shotSelector = 1;
        //                            Debug.Log("Draw Check - long - FAIL");
        //                            gm.gHUD.Message("Draw Check - long - FAIL");
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (guardCounter < 3)
        //            {
        //                //guard check
        //                if (Random.Range(0f, 10f) < activeCharStats.guardAccuracy.GetValue())
        //                {
        //                    shotSelector = 3;
        //                    Debug.Log("Guard Check - SUCCESS");
        //                    gm.gHUD.Message("Guard Check - SUCCESS");
        //                }
        //                else
        //                {
        //                    if (Random.Range(0f, 10f) < activeCharStats.guardAccuracy.GetValue())
        //                    {
        //                        shotSelector = 0;
        //                        Debug.Log("Guard Check - long - FAIL");
        //                        gm.gHUD.Message("Guard Check - long - FAIL");
        //                    }
        //                    else
        //                    {
        //                        shotSelector = 1;
        //                        Debug.Log("Guard Check - short - FAIL");
        //                        gm.gHUD.Message("Guard Check - short - FAIL");
        //                    }
        //                }
        //            }
        //            else if (houseCount < 2)
        //            {
        //                //draw check
        //                if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                {
        //                    shotSelector = 0;
        //                    Debug.Log("Draw Check - SUCCESS");
        //                    gm.gHUD.Message("Draw Check - SUCCESS");
        //                }
        //                else
        //                {
        //                    if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                    {
        //                        shotSelector = 3;
        //                        Debug.Log("Draw Check - short - FAIL");
        //                        gm.gHUD.Message("Draw Check - short - FAIL");
        //                    }
        //                    else
        //                    {
        //                        shotSelector = 1;
        //                        Debug.Log("Draw Check - long - FAIL");
        //                        gm.gHUD.Message("Draw Check - long - FAIL");
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                //freeze check
        //                bool stopCounting = false;

        //                for (int j = 0; j < gm.rockList.Count; j++)
        //                {
        //                    if (!stopCounting && gm.rockList[j].rockInfo.teamName
        //                        == otherTeamName && gm.rockList[j].rockInfo.inHouse && !gm.rockList[j].rockInfo.outOfPlay)
        //                    {
        //                        freezeSelector = j;
        //                        stopCounting = true;
        //                    }
        //                }
        //                if (stopCounting)
        //                {
        //                    if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                    {
        //                        shotSelector = 5;
        //                        Debug.Log("Freeze - TARGET - " + gm.rockList[freezeSelector].rockInfo.teamName + " " + gm.rockList[freezeSelector].rockInfo.rockNumber);
        //                        Debug.Log("Freeze Check - SUCCESS");
        //                        gm.gHUD.Message("Freeze Check - SUCCESS");
        //                    }
        //                    else
        //                    {
        //                        if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                        {
        //                            shotSelector = 3;
        //                            Debug.Log("Freeze Check - short - FAIL");
        //                            gm.gHUD.Message("Freeze Check - short - FAIL");
        //                        }
        //                        else
        //                        {
        //                            shotSelector = 1;
        //                            Debug.Log("Freeze Check - long - FAIL");
        //                            gm.gHUD.Message("Freeze Check - long - FAIL");
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    //draw to house
        //                    if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                    {
        //                        shotSelector = 0;
        //                        Debug.Log("Draw Check - SUCCESS");
        //                        gm.gHUD.Message("Draw Check - SUCCESS");
        //                    }
        //                    else
        //                    {
        //                        if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                        {
        //                            shotSelector = 3;
        //                            Debug.Log("Draw Check - short - FAIL");
        //                            gm.gHUD.Message("Draw Check - short - FAIL");
        //                        }
        //                        else
        //                        {
        //                            shotSelector = 1;
        //                            Debug.Log("Draw Check - long - FAIL");
        //                            gm.gHUD.Message("Draw Check - long - FAIL");
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        //else
        //{
        //    Debug.Log("Player Shot - Placement");

        //    Random.InitState(System.DateTime.Now.Millisecond);

        //    bool stopCounting = false;
        //    switch(playerSelection)
        //    {
        //        case 0:
        //            #region Draw
        //            //draw to house
        //            if (Random.Range(0f, 6f) < activeCharStats.drawAccuracy.GetValue())
        //            {
        //                shotSelector = 0;
        //                Debug.Log("Draw Check - SUCCESS");
        //                gm.gHUD.Message("Draw Check - SUCCESS");
        //            }
        //            else
        //            {
        //                if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                {
        //                    shotSelector = 3;
        //                    Debug.Log("Draw Check - short - FAIL");
        //                    gm.gHUD.Message("Draw Check - short - FAIL");
        //                }
        //                else
        //                {
        //                    shotSelector = 1;
        //                    Debug.Log("Draw Check - long - FAIL");
        //                    gm.gHUD.Message("Draw Check - long - FAIL");
        //                }
        //            }
        //            break;
        //            #endregion
        //        case 1:
        //            #region Left Guard
        //            shotSelector = 6;
        //            break;
        //            #endregion
        //        case 2:
        //            #region Centre Guard
        //            shotSelector = 6;
        //            break;
        //            #endregion
        //        case 3:
        //            #region Right Guard
        //            shotSelector = 6;
        //            break;
        //            #endregion
        //        case 4:
        //            #region Takeout
        //            //if there's an opponent, takeout
        //            //stopCounting = false;

        //            for (int j = 0; j < gm.rockList.Count; j++)
        //            {
        //                if (!stopCounting && gm.rockList[j].rockInfo.teamName
        //                    == otherTeamName && gm.rockList[j].rockInfo.inHouse && !gm.rockList[j].rockInfo.outOfPlay)
        //                {

        //                    takeOutSelector = gm.rockList[j].rockInfo.rockIndex;
        //                    stopCounting = true;
        //                    Debug.Log("Takeout Selector < 5 - " + takeOutSelector);
        //                }
        //            }

        //            if (stopCounting)
        //            {
        //                if (Random.Range(0f, 6f) < activeCharStats.takeOutAccuracy.GetValue())
        //                {
        //                    shotSelector = 4;
        //                    Debug.Log("Takeout - HIT - " + gm.rockList[takeOutSelector].rockInfo.teamName + " " + gm.rockList[takeOutSelector].rockInfo.rockNumber);
        //                    Debug.Log("Takeout Check - SUCCESS");
        //                    gm.gHUD.Message("Takeout Check - SUCCESS");
        //                }
        //                else
        //                {
        //                    stopCounting = false;
        //                    for (int j = 0; j < gm.rockList.Count; j++)
        //                    {
        //                        if (!stopCounting && !gm.rockList[j].rockInfo.outOfPlay && j != takeOutSelector)
        //                        {
        //                            takeOutSelector = j;
        //                            stopCounting = true;
        //                        }
        //                    }
        //                    if (stopCounting)
        //                    {
        //                        if (Random.Range(0f, 10f) < activeCharStats.takeOutAccuracy.GetValue())
        //                        {
        //                            shotSelector = 4;
        //                            Debug.Log("Takeout - HIT - " + gm.rockList[takeOutSelector].rockInfo.teamName + " " + gm.rockList[takeOutSelector].rockInfo.rockNumber);
        //                            Debug.Log("Takeout Check - crash - FAIL");
        //                            gm.gHUD.Message("Takeout Check - crash - FAIL");
        //                        }
        //                        else
        //                        {
        //                            shotSelector = 1;
        //                            Debug.Log("Takeout Check - out - FAIL");
        //                            gm.gHUD.Message("Takeout Check - out - FAIL");
        //                        }
        //                    }
        //                    else
        //                    {
        //                        shotSelector = 1;
        //                        Debug.Log("Takeout Check - FAIL");
        //                        gm.gHUD.Message("Takeout Check - FAIL");
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                //draw to house
        //                if (Random.Range(0f, 6f) < activeCharStats.drawAccuracy.GetValue())
        //                {
        //                    shotSelector = 0;
        //                    Debug.Log("No Takeout Target - Draw Check - SUCCESS");
        //                    gm.gHUD.Message("No Takeout Target - Draw Check - SUCCESS");
        //                }
        //                else
        //                {
        //                    if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                    {
        //                        shotSelector = 3;
        //                        Debug.Log("No Takeout Target - Draw Check - short - FAIL");
        //                        gm.gHUD.Message("No Takeout Target - Draw Check - short - FAIL");
        //                    }
        //                    else
        //                    {
        //                        shotSelector = 1;
        //                        Debug.Log("No Takeout Target - Draw Check - long - FAIL");
        //                        gm.gHUD.Message("No Takeout Target - Draw Check - long - FAIL");
        //                    }
        //                }
        //            }
        //            break;
        //            #endregion
        //        case 5:
        //            #region Freeze
        //            //freeze to opponent stone if there is one
        //            stopCounting = false;

        //            for (int j = 0; j < gm.houseList.Count; j++)
        //            {
        //                if (!stopCounting && gm.houseList[j].rockInfo.teamName == otherTeamName)
        //                {
        //                    freezeSelector = j;
        //                    stopCounting = true;
        //                }
        //            }
        //            if (stopCounting)
        //            {
        //                if (Random.Range(0f, 10f) < activeCharStats.takeOutAccuracy.GetValue())
        //                {
        //                    shotSelector = 4;
        //                    Debug.Log("Freeze - TARGET - " + gm.houseList[freezeSelector].rockInfo.teamName + " " + gm.houseList[freezeSelector].rockInfo.rockNumber);
        //                    Debug.Log("Freeze Check - SUCCESS");
        //                    gm.gHUD.Message("Freeze Check - SUCCESS");
        //                }
        //                else
        //                {
        //                    shotSelector = 1;
        //                    Debug.Log("Freeze Check - FAIL");
        //                    gm.gHUD.Message("Freeze Check - FAIL");
        //                }
        //            }
        //            else
        //            {
        //                //draw to house
        //                if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
        //                {
        //                    shotSelector = 0;
        //                    Debug.Log("No Freeze Target - Draw Check - SUCCESS");
        //                    gm.gHUD.Message("No Freeze Target - Draw Check - SUCCESS");
        //                }
        //                else
        //                {
        //                    shotSelector = 1;
        //                    Debug.Log("No Freeze Target - Draw Check - FAIL");
        //                    gm.gHUD.Message("No Freeze Target - Draw Check - FAIL");
        //                }
        //            }
        //            break;
        //        #endregion
        //        default:
        //            break;
        //    }
        //}
        //if the player chooses a draw in the house..

        //and the ai is using a defensive strategy...
        //
        #endregion

        int scenario = 0;
        #region Scenario Selector
        if (gm.endCurrent == gm.endTotal - 1)
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

        ShotSelector(shotSelector, takeOutSelector, shooter, activeCharStats, otherCharStats);

        Debug.Log("Team Yellow TakeOut Accuracy " + tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue());

        Debug.Log("Team Red Draw Accuracy " + tm.teamRed[shooter].charStats.drawAccuracy.GetValue());
            
        //ShotSelector(guardCount, shotSelector, houseCount, guardCounter, takeOutSelector, freezeSelector, shooter);

        yield return new WaitUntil(() => placed = true);

        for (int i = 0; i < rockCurrent + 1; i++)
        {
            //Handheld.Vibrate();
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
            yield return new WaitForEndOfFrame();

            gm.houseList.Clear();
            gm.gList.Clear();
            int counter = 0;
            foreach (Rock_List rock in gm.rockList)
            {
                if (rock.rockInfo.inPlay == true && rock.rockInfo.inHouse && rock.rockInfo.distance < 1.5f)
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
        }

        //gm.rockCurrent = rockCurrent - 1;
        //gm.rockCurrent--;
        placed1 = true;
    }

    void ShotSelector(int shotSelector, int takeOutSelector, int shooter, CharacterStats activeCharStats, CharacterStats otherCharStats)
    {
        Random.InitState((int)System.DateTime.Now.Ticks);
        //placed = false;
        int placeSelector;
        Debug.Log("Shot Selector - " + shotSelector);
        switch (shotSelector)
        {
            case 0:
                #region Draw Random
                Debug.Log("Case 0 - House");
                //Debug.Log("Case 0 - " + houseCount + " - i is " + rockCurrent);
                placeSelector = 9;
                rockPos[rockCurrent] = placePos[placeSelector]
                    + (Random.insideUnitCircle
                    * (1.5f - (0.05f * activeCharStats.drawAccuracy.GetValue())));
                //houseCount++;
                //gm.houseList.Add(new House_List (gm.rockList[rockCurrent].rock, gm.rockList[rockCurrent].rockInfo));
                Debug.Log("case 0 rockPos is - " + rockPos[rockCurrent].x + ", " + rockPos[rockCurrent].y);
                break;
                #endregion
            case 1:
                #region Out
                Debug.Log("Case 1 - Out");
                placeSelector = 10;
                rockPos[rockCurrent] = placePos[placeSelector];
                Debug.Log("case 1 rockPos is - " + rockPos[rockCurrent].x + ", " + rockPos[rockCurrent].y);
                break;
            #endregion
            case 2:
                #region Draw Four Foot
                Debug.Log("Case 2 - Four Foot");
                placeSelector = 9;
                rockPos[rockCurrent] = placePos[placeSelector] + (Random.insideUnitCircle * 0.5f);
                Debug.Log("case 2 rockPos is - " + rockPos[rockCurrent].x + ", " + rockPos[rockCurrent].y);
                break;
            #endregion
            case 3:
                #region AutoGuard
                Debug.Log("Case 3 - Guard");
                Debug.Log("case 3 rockPos is - " + rockPos[rockCurrent].x + ", " + rockPos[rockCurrent].y);
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
                    * Random.Range(0f, 1.5f - (0.1f * activeCharStats.guardAccuracy.GetValue())));
                break;
            #endregion
            case 4:
                #region Takeout
                //takeOut check
                Debug.Log("Takeout Selector - " + takeOutSelector);
                if (Random.Range(0f, 10f) < activeCharStats.takeOutAccuracy.GetValue())
                {
                    //placeSelector = 9;
                    rockPos[rockCurrent] = rockPos[takeOutSelector]
                        + (Random.insideUnitCircle * (1.5f - (0.05f * activeCharStats.takeOutAccuracy.GetValue())));
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

                if (Random.Range(0f, 10f) < activeCharStats.takeOutAccuracy.GetValue())
                {

                    rockPos[takeOutSelector] = placePos[10];
                    Debug.Log("Opponent Rock Out of Play Check - SUCCESS");
                    houseCount--;
                }
                else
                {
                    rockPos[takeOutSelector].x +=
                        Random.Range(0f, 0.05f * activeCharStats.takeOutAccuracy.GetValue());
                    rockPos[takeOutSelector].y +=
                        0.05f * activeCharStats.takeOutAccuracy.GetValue();
                    Debug.Log("Opponent Rock Out of Play Check - FAIL");
                }
                Debug.Log("Case 4 - Takeout - " + takeOutSelector);
                break;
                #endregion
            case 5:
                #region Freeze
                Debug.Log("Case 5 - Freeze - " + takeOutSelector);
                //takeOut check
                if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
                {
                    rockPos[rockCurrent].y = rockPos[takeOutSelector].y - 0.5f;
                    rockPos[rockCurrent].x = rockPos[takeOutSelector].x;
                    rockPos[rockCurrent] = rockPos[rockCurrent] + (Random.insideUnitCircle
                            * (0.5f - (0.05f * activeCharStats.drawAccuracy.GetValue())));
                    Debug.Log("Close Freeze Check - SUCCESS");
                    
                }
                else
                {
                    rockPos[rockCurrent].y = rockPos[takeOutSelector].y - 0.5f;
                    rockPos[rockCurrent].x = rockPos[takeOutSelector].x;
                    rockPos[rockCurrent] = rockPos[rockCurrent] + (Random.insideUnitCircle
                            * (2f - (0.1f * activeCharStats.drawAccuracy.GetValue())));
                    Debug.Log("Close Freeze Check - FAIL");
                }
                houseCount++;

                Random.InitState((int)System.DateTime.Now.Ticks);

                if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
                {
                    rockPos[takeOutSelector].y += 0.5f - (0.05f * activeCharStats.drawAccuracy.GetValue());
                    Debug.Log("Opponent Freeze Check - SUCCESS");
                }
                else
                {
                    rockPos[takeOutSelector].x += Random.Range(0f, 0.1f * activeCharStats.drawAccuracy.GetValue());
                    rockPos[takeOutSelector].y += 0.5f - (0.05f * activeCharStats.drawAccuracy.GetValue());
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
                rockPos[rockCurrent] = placePos[placeSelector] * Random.Range(0f, 1.5f - (0.1f * activeCharStats.guardAccuracy.GetValue()));
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
                if (Random.Range(0f, 11f) < skill)
                {
                    shotSelector = 3;
                    Debug.Log("Guard Check - SUCCESS");
                    gm.gHUD.Message("Guard Check - SUCCESS");
                }
                else
                {
                    //RANDOM check
                    if (Random.Range(0f, 1f) < 0.5f)
                    {
                        shotSelector = 0;
                        Debug.Log("Guard Check - long - FAIL");
                        gm.gHUD.Message("Guard Check - long - FAIL");
                    }
                    else
                    {
                        shotSelector = 1;
                        Debug.Log("Guard Check - short - FAIL");
                        gm.gHUD.Message("Guard Check - short - FAIL");
                    }
                }
                break;

            case "Draw":
                if (Random.Range(0f, 11f) < skill)
                {
                    shotSelector = 0;
                    Debug.Log("Draw Check - SUCCESS");
                    gm.gHUD.Message("Draw Check - SUCCESS");
                }
                else
                {
                    //RANDOM check
                    if (Random.Range(0f, 1f) < 0.5f)
                    {
                        shotSelector = 3;
                        Debug.Log("Draw Check - short - FAIL");
                        gm.gHUD.Message("Draw Check - short - FAIL");
                    }
                    else
                    {
                        shotSelector = 1;
                        Debug.Log("Draw Check - long - FAIL");
                        gm.gHUD.Message("Draw Check - long - FAIL");
                    }
                }
                break;

            case "Draw Four Foot":
                if (Random.Range(0f, 11f) < skill)
                {
                    shotSelector = 2;
                    Debug.Log("Draw Check - SUCCESS");
                    gm.gHUD.Message("Draw Check - SUCCESS");
                }
                else
                {
                    //RANDOM check
                    if (Random.Range(0f, 1f) < 0.5f)
                    {
                        shotSelector = 3;
                        Debug.Log("Draw Check - short - FAIL");
                        gm.gHUD.Message("Draw Check - short - FAIL");
                    }
                    else
                    {
                        shotSelector = 1;
                        Debug.Log("Draw Check - long - FAIL");
                        gm.gHUD.Message("Draw Check - long - FAIL");
                    }
                }
                break;

            case "Takeout":
                //SKILL check
                if (Random.Range(0f, 11f) < skill)
                {
                    shotSelector = 4;
                    Debug.Log("Takeout Check - SUCCESS");
                    gm.gHUD.Message("Takeout Check - SUCCESS");
                }
                //SKILL check - fail
                else
                {
                    //RANDOM crash check
                    if (Random.Range(0f, 1f) < 0.5f)
                    {
                        shotSelector = 99;
                        Debug.Log("Takeout Check - crash - FAIL");
                        gm.gHUD.Message("Takeout Check - crash - FAIL");
                    }
                    //crash check - out
                    else
                    {
                        shotSelector = 1;
                        Debug.Log("Takeout Check - out - FAIL");
                        gm.gHUD.Message("Takeout Check - out - FAIL");
                    }
                }
                break;

            case "Freeze":
                //SKILL check
                if (Random.Range(0f, 11f) < skill)
                {
                    shotSelector = 5;
                    Debug.Log("Freeze Check - SUCCESS");
                    gm.gHUD.Message("Freeze Check - SUCCESS");
                }
                //SKILL check - fail
                else
                {
                    //RANDOM check
                    if (Random.Range(0f, 1f) < 0.5f)
                    {
                        shotSelector = 3;
                        Debug.Log("Freeze Check - short - FAIL");
                        gm.gHUD.Message("Freeze Check - short - FAIL");
                    }
                    else
                    {
                        shotSelector = 1;
                        Debug.Log("Freeze Check - long - FAIL");
                        gm.gHUD.Message("Freeze Check - long - FAIL");
                    }
                }
                break;
            default:
                shotSelector = 1;
                Debug.Log("Skill Check Default - FAIL");
                gm.gHUD.Message("Skill Check Default - FAIL");
                break;
        }

        return shotSelector;
    }
}
