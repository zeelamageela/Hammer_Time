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

        if (houseCount > 3)
        {
            aiAgg = false;
        }
        else if (activeScore < otherScore)
        {
            aiAgg = true;
        }
        else
        {
            aiAgg = false;
        }

        float activeLuckStat;
        float otherLuckStat;

        activeLuckStat = activeCharStats.drawAccuracy.GetValue() + activeCharStats.guardAccuracy.GetValue() + activeCharStats.takeOutAccuracy.GetValue();
        activeLuckStat /= 3f;
        otherLuckStat = otherCharStats.drawAccuracy.GetValue() + otherCharStats.guardAccuracy.GetValue() + otherCharStats.takeOutAccuracy.GetValue();
        otherLuckStat /= 3f;

        Debug.Log("Active Luck is " + activeLuckStat);
        Debug.Log("Active Team is " + activeTeamName);
        Debug.Log("Other Team is " + otherTeamName);
        if (aiTurn)
        {
            //and the ai chooses a defensive strategy...
            //aggressive on defensive - prob of player guards and rocks in house based on draw and guard accuracy
            //player rocks based on guard and draw accuracy, ai rocks unhindered
            if (!aiAgg)
            {
                Debug.Log("houseCount is " + houseCount);
                Debug.Log("gm.houseList is " + gm.houseList.Count);
                if (rockCurrent < 5)
                {
                    if (houseCount > 0)
                    {
                        //if there's an opponent, takeout
                        bool stopCounting = false;

                        for (int j = 0; j < gm.rockList.Count; j++)
                        {
                            if (!stopCounting && gm.rockList[j].rockInfo.teamName
                                == otherTeamName && gm.rockList[j].rockInfo.inHouse && !gm.rockList[j].rockInfo.outOfPlay)
                            {
                                
                                takeOutSelector = gm.rockList[j].rockInfo.rockIndex;
                                stopCounting = true;
                                Debug.Log("Takeout Selector < 5 - " + takeOutSelector);
                            }
                        }

                        if (stopCounting)
                        {
                            if (Random.Range(0f, 10f) < activeCharStats.takeOutAccuracy.GetValue())
                            {
                                shotSelector = 4;
                                Debug.Log("Takeout - HIT - " + gm.rockList[takeOutSelector].rockInfo.teamName + " " + gm.rockList[takeOutSelector].rockInfo.rockNumber);
                                Debug.Log("Takeout Check - SUCCESS");
                                gm.gHUD.Message("Takeout Check - SUCCESS");
                            }
                            else
                            {
                                stopCounting = false;
                                for (int j = 0; j < gm.rockList.Count; j++)
                                {
                                    if (!stopCounting && !gm.rockList[j].rockInfo.outOfPlay && j != takeOutSelector)
                                    {
                                        takeOutSelector = j;
                                        stopCounting = true;
                                    }
                                }
                                if (stopCounting)
                                {
                                    if (Random.Range(0f, 10f) < activeCharStats.takeOutAccuracy.GetValue())
                                    {
                                        shotSelector = 4;
                                        Debug.Log("Takeout - HIT - " + gm.rockList[takeOutSelector].rockInfo.teamName + " " + gm.rockList[takeOutSelector].rockInfo.rockNumber);
                                        Debug.Log("Takeout Check - crash - FAIL");
                                        gm.gHUD.Message("Takeout Check - crash - FAIL");
                                    }
                                    else
                                    {
                                        shotSelector = 1;
                                        Debug.Log("Takeout Check - out - FAIL");
                                        gm.gHUD.Message("Takeout Check - out - FAIL");
                                    }
                                }
                                else
                                {
                                    shotSelector = 1;
                                    Debug.Log("Takeout Check - FAIL");
                                    gm.gHUD.Message("Takeout Check - FAIL");
                                }
                            }
                        }
                        else
                        {
                            //draw to house
                            if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
                            {
                                shotSelector = 0;
                                Debug.Log("No Takeout Target - Draw Check - SUCCESS");
                                gm.gHUD.Message("No Takeout Target - Draw Check - SUCCESS");
                            }
                            else
                            {
                                shotSelector = 1;
                                Debug.Log("No Takeout Target - Draw Check - FAIL");
                                gm.gHUD.Message("No Takeout Target - Draw Check - FAIL");
                            }
                        }
                    }
                    else
                    {
                        //draw to house
                        if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
                        {
                            shotSelector = 0;
                            Debug.Log("Draw Check - SUCCESS");
                        }
                        else
                        {
                            if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
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
                    }
                }
                else if (houseCount > 0)
                {
                    //if there's an opponent, takeout
                    bool stopCounting = false;

                    for (int j = 0; j < gm.rockList.Count; j++)
                    {
                        if (!stopCounting && gm.rockList[j].rockInfo.teamName
                                == otherTeamName && gm.rockList[j].rockInfo.inHouse && !gm.rockList[j].rockInfo.outOfPlay)
                        {
                            takeOutSelector = gm.rockList[j].rockInfo.rockIndex;
                            stopCounting = true;
                        }
                    }
                    if (stopCounting)
                    {
                        if (Random.Range(0f, 10f) < activeCharStats.takeOutAccuracy.GetValue())
                        {
                            shotSelector = 4;
                            Debug.Log("Takeout - HIT - "
                                + gm.rockList[takeOutSelector].rockInfo.teamName + " " + gm.rockList[takeOutSelector].rockInfo.rockNumber);
                            Debug.Log("Takeout Check - SUCCESS");
                            gm.gHUD.Message("Takeout Check - SUCCESS");
                        }
                        else
                        {
                            stopCounting = false;
                            for (int j = 0; j < gm.rockList.Count; j++)
                            {
                                if (!stopCounting && !gm.rockList[j].rockInfo.outOfPlay && j != takeOutSelector)
                                {
                                    takeOutSelector = j;
                                    stopCounting = true;
                                }
                            }
                            if (stopCounting)
                            {
                                if (Random.Range(0f, 10f) < activeCharStats.takeOutAccuracy.GetValue())
                                {
                                    shotSelector = 4;
                                    Debug.Log("Takeout - HIT - " + gm.rockList[takeOutSelector].rockInfo.teamName + " " + gm.rockList[takeOutSelector].rockInfo.rockNumber);
                                    Debug.Log("Takeout Check - crash - FAIL");
                                    gm.gHUD.Message("Takeout Check - crash - FAIL");
                                }
                                else
                                {
                                    shotSelector = 1;
                                    Debug.Log("Takeout Check - out - FAIL");
                                    gm.gHUD.Message("Takeout Check - out - FAIL");
                                }
                            }
                            else
                            {
                                shotSelector = 1;
                                Debug.Log("Takeout Check - FAIL");
                                gm.gHUD.Message("Takeout Check - FAIL");
                            }
                        }
                    }
                    else
                    {
                        //draw to house
                        if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
                        {
                            shotSelector = 0;
                            Debug.Log("No Takeout Target - Draw Check - SUCCESS");
                            gm.gHUD.Message("No Takeout Target - Draw Check - SUCCESS");
                        }
                        else
                        {
                            shotSelector = 1;
                            Debug.Log("No Takeout Target - Draw Check - FAIL");
                            gm.gHUD.Message("No Takeout Target - Draw Check - FAIL");
                        }
                    }
                }
                else if (guardCounter > 0)
                {

                    //if there's an opponent guard, takeout
                    bool stopCounting = false;
                    
                    for (int j = 0; j < gm.rockList.Count; j++)
                    {
                        if (!stopCounting && gm.rockList[j].rockInfo.teamName
                                == otherTeamName && !gm.rockList[j].rockInfo.outOfPlay)
                        {
                            takeOutSelector = j;
                            stopCounting = true;
                        }
                    }
                    if (stopCounting)
                    {
                        if (Random.Range(0f, 10f) < activeCharStats.takeOutAccuracy.GetValue())
                        {
                            shotSelector = 4;
                            Debug.Log("Takeout - HIT - " + gm.rockList[takeOutSelector].rockInfo.teamName + " " + gm.rockList[takeOutSelector].rockInfo.rockNumber);
                            Debug.Log("Takeout Check - SUCCESS");
                            gm.gHUD.Message("Takeout Check - SUCCESS");
                        }
                        else
                        {
                            stopCounting = false;
                            for (int j = 0; j < gm.rockList.Count; j++)
                            {
                                if (!stopCounting && !gm.rockList[j].rockInfo.outOfPlay && j != takeOutSelector)
                                {
                                    takeOutSelector = j;
                                    stopCounting = true;
                                }
                            }
                            if (stopCounting)
                            {
                                if (Random.Range(0f, 10f) < activeCharStats.takeOutAccuracy.GetValue())
                                {
                                    shotSelector = 4;
                                    Debug.Log("Takeout - HIT - " + gm.rockList[takeOutSelector].rockInfo.teamName + " " + gm.rockList[takeOutSelector].rockInfo.rockNumber);
                                    Debug.Log("Takeout Check - crash - FAIL");
                                    gm.gHUD.Message("Takeout Check - crash - FAIL");
                                }
                                else
                                {
                                    shotSelector = 1;
                                    Debug.Log("Takeout Check - out - FAIL");
                                    gm.gHUD.Message("Takeout Check - out - FAIL");
                                }
                            }
                            else
                            {
                                shotSelector = 1;
                                Debug.Log("Takeout Check - FAIL");
                                gm.gHUD.Message("Takeout Check - FAIL");
                            }
                        }
                    }
                    else
                    {
                        //draw to house
                        if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
                        {
                            shotSelector = 0;
                            Debug.Log("No Takeout Target - Draw Check - SUCCESS");
                            gm.gHUD.Message("No Takeout Target - Draw Check - SUCCESS");
                        }
                        else
                        {
                            if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
                            {
                                shotSelector = 3;
                                Debug.Log("No Takeout Target - Draw Check - short - FAIL");
                                gm.gHUD.Message("No Takeout Target - Draw Check - short - FAIL");
                            }
                            else
                            {
                                shotSelector = 1;
                                Debug.Log("No Takeout Target - Draw Check - long - FAIL");
                                gm.gHUD.Message("No Takeout Target - Draw Check - long - FAIL");
                            }
                        }
                    }
                }
                else
                {
                    //draw to house
                    if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
                    {
                        shotSelector = 0;
                        Debug.Log("Draw Check - SUCCESS");
                        gm.gHUD.Message("Draw Check - SUCCESS");
                    }
                    else
                    {
                        if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
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
                }
            }
            //and the ai chooses an aggressive strategy...
            //aggressive on aggressive - high prob of guards and rocks in house
            //player rocks based on draw and guard accuracy, ai rocks unhindered
            else
            {
                if (rockCurrent % 2 == 0)
                {
                    if (guardCounter < 3)
                    {
                        //guard check
                        if (Random.Range(0f, 10f) < activeCharStats.guardAccuracy.GetValue())
                        {
                            shotSelector = 3;
                            Debug.Log("Guard Check - SUCCESS");
                            gm.gHUD.Message("Guard Check - SUCCESS");
                        }
                        else
                        {
                            shotSelector = 1;
                            Debug.Log("Guard Check - FAIL");
                            gm.gHUD.Message("Guard Check - FAIL");
                        }
                    }
                    else if (houseCount < 2)
                    {
                        //draw check
                        if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
                        {
                            shotSelector = 0;
                            Debug.Log("Draw Check - SUCCESS");
                        }
                        else
                        {
                            if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
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
                    }
                    else
                    {
                        //freeze to opponent stone if there is one
                        bool stopCounting = false;

                        for (int j = 0; j < gm.rockList.Count; j++)
                        {
                            if (!stopCounting && gm.rockList[j].rockInfo.teamName
                                == otherTeamName && gm.rockList[j].rockInfo.inHouse && !gm.rockList[j].rockInfo.outOfPlay)
                            {
                                freezeSelector = j;
                                stopCounting = true;
                            }
                        }
                        if (stopCounting)
                        {
                            if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
                            {
                                shotSelector = 5;
                                Debug.Log("Freeze - TARGET - " + gm.rockList[freezeSelector].rockInfo.teamName + " " + gm.rockList[freezeSelector].rockInfo.rockNumber);
                                Debug.Log("Freeze Check - SUCCESS");
                                gm.gHUD.Message("Freeze Check - SUCCESS");
                            }
                            else
                            {
                                shotSelector = 0;
                                Debug.Log("Freeze Check - FAIL");
                                gm.gHUD.Message("Freeze Check - FAIL");
                            }
                        }
                        else
                        {
                            //draw to house
                            if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
                            {
                                shotSelector = 0;
                                Debug.Log("Draw Check - SUCCESS");
                                gm.gHUD.Message("Draw Check - SUCCESS");
                            }
                            else
                            {
                                shotSelector = 1;
                                Debug.Log("Draw Check - FAIL");
                                gm.gHUD.Message("Draw Check - FAIL");
                            }
                        }
                    }
                }
                else
                {
                    if (guardCounter < 3)
                    {
                        //guard check
                        if (Random.Range(0f, 10f) < activeCharStats.guardAccuracy.GetValue())
                        {
                            shotSelector = 3;
                            Debug.Log("Guard Check - SUCCESS");
                            gm.gHUD.Message("Guard Check - SUCCESS");
                        }
                        else
                        {
                            if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
                            {
                                shotSelector = 1;
                                Debug.Log("Guard Check - short - FAIL");
                                gm.gHUD.Message("Guard Check - short - FAIL");
                            }
                            else
                            {
                                shotSelector = 3;
                                Debug.Log("Guard Check - long - FAIL");
                                gm.gHUD.Message("Guard Check - long - FAIL");
                            }
                        }
                    }
                    else if (houseCount < 2)
                    {
                        //draw check
                        if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
                        {
                            shotSelector = 0;
                            Debug.Log("Draw Check - SUCCESS");
                            gm.gHUD.Message("Draw Check - SUCCESS");
                        }
                        else
                        {
                            if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
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
                    }
                    else
                    {
                        //freeze check
                        bool stopCounting = false;

                        for (int j = 0; j < gm.rockList.Count; j++)
                        {
                            if (!stopCounting && gm.rockList[j].rockInfo.teamName
                                == otherTeamName && gm.rockList[j].rockInfo.inHouse && !gm.rockList[j].rockInfo.outOfPlay)
                            {
                                freezeSelector = j;
                                stopCounting = true;
                            }
                        }
                        if (stopCounting)
                        {
                            if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
                            {
                                shotSelector = 5;
                                Debug.Log("Freeze - TARGET - " + gm.rockList[freezeSelector].rockInfo.teamName + " " + gm.rockList[freezeSelector].rockInfo.rockNumber);
                                Debug.Log("Freeze Check - SUCCESS");
                                gm.gHUD.Message("Freeze Check - SUCCESS");
                            }
                            else
                            {
                                if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
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
                        }
                        else
                        {
                            //draw to house
                            if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
                            {
                                shotSelector = 0;
                                Debug.Log("Draw Check - SUCCESS");
                                gm.gHUD.Message("Draw Check - SUCCESS");
                            }
                            else
                            {
                                if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
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
                        }
                    }
                }
            }
        }
        else
        {
            Debug.Log("Player Shot - Placement");

            Random.InitState(System.DateTime.Now.Millisecond);

            bool stopCounting = false;
            switch(playerSelection)
            {
                case 0:
                    #region Draw
                    //draw to house
                    if (Random.Range(0f, 6f) < activeCharStats.drawAccuracy.GetValue())
                    {
                        shotSelector = 0;
                        Debug.Log("Draw Check - SUCCESS");
                        gm.gHUD.Message("Draw Check - SUCCESS");
                    }
                    else
                    {
                        if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
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
                    #endregion
                case 1:
                    #region Left Guard
                    shotSelector = 6;
                    break;
                    #endregion
                case 2:
                    #region Centre Guard
                    shotSelector = 6;
                    break;
                    #endregion
                case 3:
                    #region Right Guard
                    shotSelector = 6;
                    break;
                    #endregion
                case 4:
                    #region Takeout
                    //if there's an opponent, takeout
                    //stopCounting = false;

                    for (int j = 0; j < gm.rockList.Count; j++)
                    {
                        if (!stopCounting && gm.rockList[j].rockInfo.teamName
                            == otherTeamName && gm.rockList[j].rockInfo.inHouse && !gm.rockList[j].rockInfo.outOfPlay)
                        {

                            takeOutSelector = gm.rockList[j].rockInfo.rockIndex;
                            stopCounting = true;
                            Debug.Log("Takeout Selector < 5 - " + takeOutSelector);
                        }
                    }

                    if (stopCounting)
                    {
                        if (Random.Range(0f, 6f) < activeCharStats.takeOutAccuracy.GetValue())
                        {
                            shotSelector = 4;
                            Debug.Log("Takeout - HIT - " + gm.rockList[takeOutSelector].rockInfo.teamName + " " + gm.rockList[takeOutSelector].rockInfo.rockNumber);
                            Debug.Log("Takeout Check - SUCCESS");
                            gm.gHUD.Message("Takeout Check - SUCCESS");
                        }
                        else
                        {
                            stopCounting = false;
                            for (int j = 0; j < gm.rockList.Count; j++)
                            {
                                if (!stopCounting && !gm.rockList[j].rockInfo.outOfPlay && j != takeOutSelector)
                                {
                                    takeOutSelector = j;
                                    stopCounting = true;
                                }
                            }
                            if (stopCounting)
                            {
                                if (Random.Range(0f, 10f) < activeCharStats.takeOutAccuracy.GetValue())
                                {
                                    shotSelector = 4;
                                    Debug.Log("Takeout - HIT - " + gm.rockList[takeOutSelector].rockInfo.teamName + " " + gm.rockList[takeOutSelector].rockInfo.rockNumber);
                                    Debug.Log("Takeout Check - crash - FAIL");
                                    gm.gHUD.Message("Takeout Check - crash - FAIL");
                                }
                                else
                                {
                                    shotSelector = 1;
                                    Debug.Log("Takeout Check - out - FAIL");
                                    gm.gHUD.Message("Takeout Check - out - FAIL");
                                }
                            }
                            else
                            {
                                shotSelector = 1;
                                Debug.Log("Takeout Check - FAIL");
                                gm.gHUD.Message("Takeout Check - FAIL");
                            }
                        }
                    }
                    else
                    {
                        //draw to house
                        if (Random.Range(0f, 6f) < activeCharStats.drawAccuracy.GetValue())
                        {
                            shotSelector = 0;
                            Debug.Log("No Takeout Target - Draw Check - SUCCESS");
                            gm.gHUD.Message("No Takeout Target - Draw Check - SUCCESS");
                        }
                        else
                        {
                            if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
                            {
                                shotSelector = 3;
                                Debug.Log("No Takeout Target - Draw Check - short - FAIL");
                                gm.gHUD.Message("No Takeout Target - Draw Check - short - FAIL");
                            }
                            else
                            {
                                shotSelector = 1;
                                Debug.Log("No Takeout Target - Draw Check - long - FAIL");
                                gm.gHUD.Message("No Takeout Target - Draw Check - long - FAIL");
                            }
                        }
                    }
                    break;
                    #endregion
                case 5:
                    #region Freeze
                    //freeze to opponent stone if there is one
                    stopCounting = false;

                    for (int j = 0; j < gm.houseList.Count; j++)
                    {
                        if (!stopCounting && gm.houseList[j].rockInfo.teamName == otherTeamName)
                        {
                            freezeSelector = j;
                            stopCounting = true;
                        }
                    }
                    if (stopCounting)
                    {
                        if (Random.Range(0f, 10f) < activeCharStats.takeOutAccuracy.GetValue())
                        {
                            shotSelector = 4;
                            Debug.Log("Freeze - TARGET - " + gm.houseList[freezeSelector].rockInfo.teamName + " " + gm.houseList[freezeSelector].rockInfo.rockNumber);
                            Debug.Log("Freeze Check - SUCCESS");
                            gm.gHUD.Message("Freeze Check - SUCCESS");
                        }
                        else
                        {
                            shotSelector = 1;
                            Debug.Log("Freeze Check - FAIL");
                            gm.gHUD.Message("Freeze Check - FAIL");
                        }
                    }
                    else
                    {
                        //draw to house
                        if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
                        {
                            shotSelector = 0;
                            Debug.Log("No Freeze Target - Draw Check - SUCCESS");
                            gm.gHUD.Message("No Freeze Target - Draw Check - SUCCESS");
                        }
                        else
                        {
                            shotSelector = 1;
                            Debug.Log("No Freeze Target - Draw Check - FAIL");
                            gm.gHUD.Message("No Freeze Target - Draw Check - FAIL");
                        }
                    }
                    break;
                #endregion
                default:
                    break;
            }
        }
        //if the player chooses a draw in the house..

        //and the ai is using a defensive strategy...
        //


        ShotSelector(shotSelector, takeOutSelector, freezeSelector, shooter, activeCharStats, otherCharStats);

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
            yield return new WaitForEndOfFrame();
            Debug.Log("Rock Position " + i + " " + rockPos[i]);
            gm.rockList[i].rock.transform.position = rockPos[i];

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
            //Debug.Log("i is equal to " + i);
            //Handheld.Vibrate();
            //rm.rb.ShotUpdate(rockCurrent, gm.rockList[i].rockInfo.outOfPlay);
            yield return new WaitForEndOfFrame();
        }

        //gm.rockCurrent = rockCurrent - 1;
        //gm.rockCurrent--;
        placed1 = true;
    }

    void ShotSelector(int shotSelector, int takeOutSelector, int freezeSelector, int shooter, CharacterStats activeCharStats, CharacterStats otherCharStats)
    {
        Random.InitState((int)System.DateTime.Now.Ticks);
        //placed = false;
        int placeSelector;
        Debug.Log("Shot Selector - " + shotSelector);
        switch (shotSelector)
        {
            case 0:
                #region Draw to Position
                Debug.Log("Case 0 - House");
                Debug.Log("Case 0 - " + houseCount + " - i is " + rockCurrent);
                placeSelector = 9;
                rockPos[rockCurrent] = placePos[placeSelector]
                    + (Random.insideUnitCircle
                    * (1.5f - (0.05f * activeCharStats.drawAccuracy.GetValue())));
                houseCount++;
                gm.houseList.Add(new House_List (gm.rockList[rockCurrent].rock, gm.rockList[rockCurrent].rockInfo));
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
                #region Out
                Debug.Log("Case 2 - Out");
                placeSelector = 10;
                rockPos[rockCurrent] = placePos[placeSelector];
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
                Debug.Log("Case 5 - Freeze - " + freezeSelector);
                //takeOut check
                if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
                {
                    rockPos[rockCurrent].y = rockPos[freezeSelector].y - 0.5f;
                    rockPos[rockCurrent].x = rockPos[freezeSelector].x;
                    rockPos[rockCurrent] = rockPos[rockCurrent] + (Random.insideUnitCircle
                            * (0.5f - (0.05f * activeCharStats.drawAccuracy.GetValue())));
                    Debug.Log("Close Freeze Check - SUCCESS");
                }
                else
                {
                    rockPos[rockCurrent].y = rockPos[freezeSelector].y - 0.5f;
                    rockPos[rockCurrent].x = rockPos[freezeSelector].x;
                    rockPos[rockCurrent] = rockPos[rockCurrent] + (Random.insideUnitCircle
                            * (2f - (0.1f * activeCharStats.drawAccuracy.GetValue())));
                    Debug.Log("Close Freeze Check - FAIL");
                }

                Random.InitState((int)System.DateTime.Now.Ticks);

                if (Random.Range(0f, 10f) < activeCharStats.drawAccuracy.GetValue())
                {
                    rockPos[freezeSelector].y += 0.5f - (0.05f * activeCharStats.drawAccuracy.GetValue());
                    Debug.Log("Opponent Freeze Check - SUCCESS");
                }
                else
                {
                    rockPos[freezeSelector].x += Random.Range(0f, 0.1f * activeCharStats.drawAccuracy.GetValue());
                    rockPos[freezeSelector].y += 0.5f - (0.05f * activeCharStats.drawAccuracy.GetValue());
                    Debug.Log("Opponent Freeze Check - FAIL");
                }
                break;
            #endregion
            case 6:
                #region Guard
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
    

}
