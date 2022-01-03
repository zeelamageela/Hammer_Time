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
    int rockCurrent;

    public Vector2[] placePos;
    public Vector2[] rockPos;

    public GameObject playerStratGO;
    public bool aggressive;
    public int round;

    public GameObject dialogueGO;
    public DialogueTrigger coachDialogue;
    public DialogueTrigger announDialogue;
    // Start is called before the first frame update
    void Start()
    {
        placed = false;
    }

    public void OnRockPlace(int rockCrnt, bool redTeam)
    {
        placed = false;
        rockCurrent = rockCrnt;

        //StartCoroutine(RandomRockPlace());

        StartCoroutine(PlayerStrategy());
    }

    public void Help()
    {
        dialogueGO.SetActive(true);

        if (round == 1)
            coachDialogue.TriggerDialogue("Strategy", 0);
        else if (round == 2)
            coachDialogue.TriggerDialogue("Strategy", 1);
    }

    public void OnChoice(bool aggro)
    {
        aggressive = aggro;

        playerStratGO.SetActive(false);
    }

    IEnumerator RandomRockPlace()
    {
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        int houseCount = 0;
        int houseRed = 0;
        bool[] guardCount = new bool[9];
        for (int i = 0; i < 9; i++)
            guardCount[i] = false;
        for (int i = 0; i < rockCurrent; i++)
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

        for (int i = 0; i < rockCurrent; i++)
        {
            gm.rockList[i].rockInfo.placed = true;
        }

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < rockCurrent; i++)
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

    IEnumerator PlayerStrategy()
    {
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        CareerManager cm = FindObjectOfType<CareerManager>();
        playerStratGO.SetActive(true);
        gm.rockBar.EndUpdate(gsp.yellowScore, gsp.redScore);
        if (round < 1)
        {
            if (!cm.strategyDialogue[0])
            {
                dialogueGO.SetActive(true);
                coachDialogue.TriggerDialogue("Strategy", 0);
                cm.strategyDialogue[0] = true;
            }
        }
        else if (round == 1)
        {
            if (!cm.strategyDialogue[1])
            {
                dialogueGO.SetActive(true);
                coachDialogue.TriggerDialogue("Strategy", 1);
                cm.strategyDialogue[1] = true;
            }
        }
        else
            playerStratGO.SetActive(false);

        yield return new WaitForEndOfFrame();

        tm.SetCharacter(gm.rockCurrent, true);
        tm.SetCharacter(gm.rockCurrent, false);

        yield return new WaitUntil(() => !playerStratGO.activeSelf);
        round++;

        if (round == 1)
        {
            rockPos = new Vector2[rockCurrent];

            //if (!cm.strategyDialogue[0])
            //{
            //    coachDialogue.TriggerDialogue("Strategy", 0);
            //    cm.strategyDialogue[0] = true;
            //}
            yield return StartCoroutine(FirstPlacement());
            
        }
        else if (round == 2)
        {
            yield return StartCoroutine(SecondPlacement());
            //placed = true;
        }
        else
            playerStratGO.SetActive(false);

        //if (rockCurrent < 8)
        //{
        //    yield return new WaitUntil(() => !playerStratGO.activeSelf);
        //}
        //else
        //    yield return new WaitForEndOfFrame();

        yield return new WaitForEndOfFrame();

        //rocksPlaced = true;
    }

    IEnumerator FirstPlacement()
    {
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        bool redTeam;

        if (gsp.teamName == gsp.redTeamName)
            redTeam = true;
        else
            redTeam = false;

        Debug.Log("RedTeam is " + redTeam);
        int houseCount = 0;
        bool aiAgg;

        int guardCounter = 0;

        bool[] guardCount = new bool[9];

        for (int i = 0; i < 9; i++)
            guardCount[i] = false;

        rockCurrent = 8;

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

        for (int i = 0; i < rockCurrent; i++)
        {
            Random.InitState(System.DateTime.Now.Millisecond);

            int placeSelector;
            int shotSelector;

            int shooter = Mathf.FloorToInt(i / 4);

            //If the player chooses an aggressive strategy...
            if (aggressive)
            {
                //and the ai chooses a defensive strategy...
                //aggressive on defensive - prob of player guards and rocks in house based on draw and guard accuracy
                //player rocks based on guard and draw accuracy, ai rocks unhindered
                if (!aiAgg)
                {
                    if (i % 2 == 0)
                    {
                        if (gm.redHammer)
                        {
                            if (guardCounter < 3)
                            {
                                if (redTeam)
                                {
                                    if (Random.Range(0f, 10f) < tm.teamYellow[shooter].charStats.guardAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) > tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                            }
                            else if (houseCount < 4)
                            {
                                if (redTeam)
                                {
                                    if (Random.Range(0f, 10f) < tm.teamYellow[shooter].charStats.drawAccuracy.GetValue())
                                        shotSelector = 0;
                                    else
                                        shotSelector = 1;
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) > tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())
                                        shotSelector = 0;
                                    else
                                        shotSelector = 1;
                                }
                            }
                            else
                                shotSelector = 1;
                        }
                        else
                        {
                            if (guardCounter < 3)
                            {
                                if (redTeam)
                                {
                                    if (Random.Range(0f, 10f) < tm.teamRed[shooter].charStats.guardAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) > tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                            }
                            else if (houseCount < 4)
                            {
                                if (redTeam)
                                {
                                    if (Random.Range(0f, 10f) < tm.teamRed[shooter].charStats.drawAccuracy.GetValue())
                                        shotSelector = 0;
                                    else
                                        shotSelector = 1;
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) > tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())
                                        shotSelector = 0;
                                    else
                                        shotSelector = 1;
                                }
                            }
                            else
                                shotSelector = 1;
                        }
                    }
                    else
                    {
                        if (gm.redHammer)
                        {
                            if (guardCounter < 3)
                            {
                                if (redTeam)
                                {
                                    if (Random.Range(0f, 10f) < tm.teamYellow[shooter].charStats.guardAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) > tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                            }
                            else if (houseCount < 4)
                            {
                                if (redTeam)
                                {
                                    if (Random.Range(0f, 10f) < tm.teamYellow[shooter].charStats.drawAccuracy.GetValue())
                                        shotSelector = 0;
                                    else
                                        shotSelector = 1;
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) > tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())
                                        shotSelector = 0;
                                    else
                                        shotSelector = 1;
                                }
                            }
                            else
                                shotSelector = 1;
                        }
                        else
                        {
                            if (guardCounter < 3)
                            {
                                if (redTeam)
                                {
                                    if (Random.Range(0f, 10f) < tm.teamRed[shooter].charStats.guardAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) > tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                            }
                            else if (houseCount < 4)
                            {
                                if (redTeam)
                                {
                                    if (Random.Range(0f, 10f) < tm.teamRed[shooter].charStats.drawAccuracy.GetValue())
                                        shotSelector = 0;
                                    else
                                        shotSelector = 1;
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) > tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())
                                        shotSelector = 0;
                                    else
                                        shotSelector = 1;
                                }
                            }
                            else
                                shotSelector = 1;
                        }
                    }
                }
                //and the ai chooses an aggressive strategy...
                //aggressive on aggressive - high prob of guards and rocks in house
                //player rocks based on draw and guard accuracy, ai rocks unhindered
                else
                {
                    if (i % 2 == 0)
                    {
                        if (gm.redHammer)
                        {
                            if (guardCounter < 4)
                            {
                                if (Random.Range(0f, 10f) < tm.teamYellow[shooter].charStats.guardAccuracy.GetValue())
                                    shotSelector = 3;
                                else
                                    shotSelector = 1;
                            }
                            else if (houseCount < 5)
                            {
                                if (Random.Range(0f, 10f) < tm.teamYellow[shooter].charStats.drawAccuracy.GetValue())
                                    shotSelector = 0;
                                else
                                    shotSelector = 1;
                            }
                            else
                                shotSelector = 1;
                        }
                        else
                        {
                            if (guardCounter < 4)
                            {
                                if (Random.Range(0f, 10f) < tm.teamRed[shooter].charStats.guardAccuracy.GetValue())
                                    shotSelector = 3;
                                else
                                    shotSelector = 1;
                            }
                            else if (houseCount < 5)
                            {
                                if (Random.Range(0f, 10f) < tm.teamRed[shooter].charStats.drawAccuracy.GetValue())
                                    shotSelector = 0;
                                else
                                    shotSelector = 1;
                            }
                            else
                                shotSelector = 1;
                        }
                    }
                    else
                    {
                        if (!gm.redHammer)
                        {
                            if (guardCounter < 4)
                            {
                                if (Random.Range(0f, 10f) < tm.teamYellow[shooter].charStats.guardAccuracy.GetValue())
                                    shotSelector = 3;
                                else
                                    shotSelector = 1;
                            }
                            else if (houseCount < 5)
                            {
                                if (Random.Range(0f, 10f) < tm.teamYellow[shooter].charStats.drawAccuracy.GetValue())
                                    shotSelector = 0;
                                else
                                    shotSelector = 1;
                            }
                            else
                                shotSelector = 1;
                        }
                        else
                        {
                            if (guardCounter < 4)
                            {
                                if (Random.Range(0f, 10f) < tm.teamRed[shooter].charStats.guardAccuracy.GetValue())
                                    shotSelector = 3;
                                else
                                    shotSelector = 1;
                            }
                            else if (houseCount < 5)
                            {
                                if (Random.Range(0f, 10f) < tm.teamRed[shooter].charStats.drawAccuracy.GetValue())
                                    shotSelector = 0;
                                else
                                    shotSelector = 1;
                            }
                            else
                                shotSelector = 1;
                        }
                    }
                }
            }
            //if the player chooses a defensive strategy...
            else
            {
                //defensive on defensive - low prob of any rocks in play
                //player rocks based on takeOut accuracy, ai rocks based on takeOut accuracy
                if (!aiAgg)
                {
                    if (i % 2 == 0)
                    {
                        if (gm.redHammer)
                        {
                            if (guardCounter < 2)
                            {
                                if (Random.Range(0f, 10f) > tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())
                                    shotSelector = 3;
                                else
                                    shotSelector = 1;
                            }
                            else if (houseCount < 3)
                            {
                                if (Random.Range(0f, 10f) > tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())
                                    shotSelector = 0;
                                else
                                    shotSelector = 1;
                            }
                            else
                                shotSelector = 1;
                        }
                        else
                        {
                            if (guardCounter < 2)
                            {
                                if (Random.Range(0f, 10f) > tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())
                                    shotSelector = 3;
                                else
                                    shotSelector = 1;
                            }
                            else if (houseCount < 3)
                            {
                                if (Random.Range(0f, 10f) > tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())
                                    shotSelector = 0;
                                else
                                    shotSelector = 1;
                            }
                            else
                                shotSelector = 1;
                        }
                    }
                    else
                    {
                        if (!gm.redHammer)
                        {
                            if (guardCounter < 2)
                            {
                                if (Random.Range(0f, 10f) > tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())
                                    shotSelector = 3;
                                else
                                    shotSelector = 1;
                            }
                            else if (houseCount < 3)
                            {
                                if (Random.Range(0f, 10f) > tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())
                                    shotSelector = 0;
                                else
                                    shotSelector = 1;
                            }
                            else
                                shotSelector = 1;
                        }
                        else
                        {
                            if (guardCounter < 2)
                            {
                                if (Random.Range(0f, 10f) > tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())
                                    shotSelector = 3;
                                else
                                    shotSelector = 1;
                            }
                            else if (houseCount < 3)
                            {
                                if (Random.Range(0f, 10f) > tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())
                                    shotSelector = 0;
                                else
                                    shotSelector = 1;
                            }
                            else
                                shotSelector = 1;
                        }
                    }
                }
                //defensive on aggressive - prob ai guards and rocks in house based on player's take out accuracy
                //player rocks based on takeOut accuracy, ai rocks based on takeOut accuracy
                else
                {
                    if (i % 2 == 0)
                    {
                        if (gm.redHammer)
                        {
                            if (guardCounter < 3)
                            {
                                if (redTeam)
                                {
                                    if (Random.Range(0f, 10f) > tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) < tm.teamYellow[shooter].charStats.guardAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                            }
                            else if (houseCount < 3)
                            {
                                if (redTeam)
                                {
                                    if (Random.Range(0f, 10f) > tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())
                                        shotSelector = 0;
                                    else
                                        shotSelector = 1;
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) < tm.teamYellow[shooter].charStats.drawAccuracy.GetValue())
                                        shotSelector = 0;
                                    else
                                        shotSelector = 1;
                                }
                            }
                            else
                                shotSelector = 1;
                        }
                        else
                        {
                            if (guardCounter < 3)
                            {
                                if (redTeam)
                                {
                                    if (Random.Range(0f, 10f) < tm.teamRed[shooter].charStats.guardAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) > tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                            }
                            else if (houseCount < 4)
                            {
                                if (redTeam)
                                {
                                    if (Random.Range(0f, 10f) < tm.teamRed[shooter].charStats.drawAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) > tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                            }
                            else
                                shotSelector = 1;
                        }
                    }
                    else
                    {
                        if (!gm.redHammer)
                        {
                            if (guardCounter < 3)
                            {
                                if (redTeam)
                                {
                                    if (Random.Range(0f, 10f) > tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) < tm.teamYellow[shooter].charStats.guardAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                            }
                            else if (houseCount < 4)
                            {
                                if (redTeam)
                                {
                                    if (Random.Range(0f, 10f) > tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())
                                        shotSelector = 0;
                                    else
                                        shotSelector = 1;
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) < tm.teamYellow[shooter].charStats.drawAccuracy.GetValue())
                                        shotSelector = 0;
                                    else
                                        shotSelector = 1;
                                }
                            }
                            else
                                shotSelector = 1;
                        }
                        else
                        {
                            if (guardCounter < 3)
                            {
                                if (redTeam)
                                {
                                    if (Random.Range(0f, 10f) < tm.teamRed[shooter].charStats.guardAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) > tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                            }
                            else if (houseCount < 4)
                            {
                                if (redTeam)
                                {
                                    if (Random.Range(0f, 10f) < tm.teamRed[shooter].charStats.drawAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) > tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())
                                        shotSelector = 3;
                                    else
                                        shotSelector = 1;
                                }
                            }
                            else
                                shotSelector = 1;
                        }
                    }
                }
            }


            Debug.Log("Team Yellow TakeOut Accuracy " + tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue());

            Debug.Log("Team Red Draw Accuracy " + tm.teamRed[shooter].charStats.drawAccuracy.GetValue());
            switch (shotSelector)
            {
                case 0:
                    Debug.Log("Case 0 - House");
                    Debug.Log("Case 0 - " + houseCount + " - i is " + i);
                    placeSelector = 9;
                    if (houseCount > 5)
                    {
                        placeSelector = 10;
                        rockPos[i] = placePos[placeSelector];
                    }
                    else
                    {
                        houseCount++;
                        if (i % 2 == 1)
                        {
                            if (gm.redHammer)
                                rockPos[i] = placePos[placeSelector]
                                    + (Random.insideUnitCircle
                                    * (1.5f - (0.05f * tm.teamRed[shooter].charStats.drawAccuracy.GetValue())));
                            else
                                rockPos[i] = placePos[placeSelector]
                                    + (Random.insideUnitCircle
                                    * (1.5f - (0.05f * tm.teamYellow[shooter].charStats.drawAccuracy.GetValue())));
                        }
                        else
                        {
                            if (gm.redHammer)
                                rockPos[i] = placePos[placeSelector]
                                    + (Random.insideUnitCircle
                                    * (1.5f - (0.05f * tm.teamYellow[shooter].charStats.drawAccuracy.GetValue())));
                            else
                                rockPos[i] = placePos[placeSelector]
                                    + (Random.insideUnitCircle
                                    * (1.5f - (0.05f * tm.teamRed[shooter].charStats.drawAccuracy.GetValue())));
                        }
                    }
                    Debug.Log("case 0 rockPos is - " + rockPos[i].x + ", " + rockPos[i].y);
                    break;
                case 1:
                    Debug.Log("Case 1 - Out");
                    placeSelector = 10;
                    rockPos[i] = placePos[placeSelector];
                    Debug.Log("case 1 rockPos is - " + rockPos[i].x + ", " + rockPos[i].y);
                    break;
                case 2:
                    Debug.Log("Case 2 - Out");
                    placeSelector = 10;
                    rockPos[i] = placePos[placeSelector];
                    Debug.Log("case 2 rockPos is - " + rockPos[i].x + ", " + rockPos[i].y);
                    break;
                case 3:
                    Debug.Log("Case 3 - Guard");
                    Debug.Log("Case 3 - " + guardCounter);
                    if (i % 2 == 1)
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
                                guardCounter++;

                                if (gm.redHammer)
                                    rockPos[i] = placePos[placeSelector]
                                        + (Random.insideUnitCircle
                                        * (1.5f - (0.1f * tm.teamRed[shooter].charStats.guardAccuracy.GetValue())));
                                else
                                    rockPos[i] = placePos[placeSelector]
                                        + (Random.insideUnitCircle
                                        * (1.5f - (0.1f * tm.teamYellow[shooter].charStats.guardAccuracy.GetValue())));
                            }
                        }
                        else
                        {
                            guardCount[placeSelector] = true;
                            guardCounter++;

                            if (gm.redHammer)
                                rockPos[i] = placePos[placeSelector]
                                    + (Random.insideUnitCircle
                                    * (1.5f - (0.1f * tm.teamRed[shooter].charStats.guardAccuracy.GetValue())));
                            else
                                rockPos[i] = placePos[placeSelector]
                                    + (Random.insideUnitCircle
                                    * (1.5f - (0.1f * tm.teamYellow[shooter].charStats.guardAccuracy.GetValue())));
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
                                guardCounter++;

                                if (gm.redHammer)
                                    rockPos[i] = placePos[placeSelector]
                                        + (Random.insideUnitCircle
                                        * (1.5f - (0.1f * tm.teamYellow[shooter].charStats.guardAccuracy.GetValue())));
                                else
                                    rockPos[i] = placePos[placeSelector]
                                        + (Random.insideUnitCircle
                                        * (1.5f - (0.1f * tm.teamRed[shooter].charStats.guardAccuracy.GetValue())));
                            }
                        }
                        else
                        {
                            guardCount[placeSelector] = true;
                            guardCounter++;

                            if (gm.redHammer)
                                rockPos[i] = placePos[placeSelector]
                                    + (Random.insideUnitCircle
                                    * (1.5f - (0.1f * tm.teamYellow[shooter].charStats.guardAccuracy.GetValue())));
                            else
                                rockPos[i] = placePos[placeSelector]
                                    + (Random.insideUnitCircle
                                    * (1.5f - (0.1f * tm.teamRed[shooter].charStats.guardAccuracy.GetValue())));
                        }
                    }
                    Debug.Log("case 3 rockPos is - " + rockPos[i].x + ", " + rockPos[i].y);

                    break;
                default:
                    Debug.Log("Default - Out - " + i);
                    Debug.Log("Case " + shotSelector + " - " + i);
                    placeSelector = 10;
                    rockPos[i] = placePos[placeSelector];
                    Debug.Log("Default rockPos is - " + rockPos[i].x + ", " + rockPos[i].y);
                    break;
            }
        }

        for (int i = 0; i < rockCurrent; i++)
        {
            gm.rockList[i].rockInfo.placed = true;
        }

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < rockCurrent; i++)
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

        //gm.rockCurrent = rockCurrent - 1;
        gm.rockCurrent--;
        placed1 = true;
    }


    IEnumerator SecondPlacement()
    {
        Debug.Log("Second Round");
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        bool redTeam;

        if (gsp.teamName == gsp.redTeamName)
            redTeam = true;
        else
            redTeam = false;

        Debug.Log("RedTeam is " + redTeam);
        rockCurrent = 12;

        int houseCount = 0;

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

        for (int i = 8; i < rockCurrent; i++)
        {
            Random.InitState((int)System.DateTime.Now.Ticks);
            int placeSelector;
            int shotSelector;
            int takeOutSelector = 99;
            int freezeSelector = 99;

            int shooter = Mathf.FloorToInt(i / 4);

            if (gm.houseList.Count > 0)
                Debug.Log("House List Count is " + gm.houseList.Count);
            else
                Debug.Log("House List empty");
            //If the player chooses an aggressive strategy...
            if (aggressive)
            {
                //and the ai chooses a defensive strategy...
                //aggressive on defensive - prob of player guards and rocks in house based on draw and guard accuracy
                //player rocks based on guard and draw accuracy, ai rocks unhindered
                if (!aiAgg)
                {
                    if (i % 2 == 0)
                    {
                        if (gm.redHammer)
                        {
                            if (redTeam)
                            {
                                if (gm.houseList.Count == 0)
                                    shotSelector = 0;
                                else
                                {
                                    int score = 0;
                                    bool stopCounting = false;

                                    for (int j = 0; j < gm.houseList.Count; j++)
                                    {
                                        if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.redTeamName)
                                        {
                                            takeOutSelector = j;
                                            score++;
                                            stopCounting = true;
                                        }
                                    }
                                    if (stopCounting)
                                        shotSelector = 4;
                                    else
                                    {
                                        shotSelector = 0;
                                        takeOutSelector = 99;
                                    }
                                }
                            }
                            else
                            {
                                if (gm.houseList.Count == 0)
                                    shotSelector = 0;
                                else
                                {
                                    int score = 0;
                                    bool stopCounting = false;

                                    for (int j = 0; j < gm.houseList.Count; j++)
                                    {
                                        if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.yellowTeamName)
                                        {
                                            freezeSelector = j;
                                            score++;
                                            stopCounting = true;
                                        }
                                    }
                                    if (stopCounting)
                                        shotSelector = 5;
                                    else
                                    {
                                        shotSelector = 0;
                                        takeOutSelector = 99;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (redTeam)
                            {
                                if (gm.houseList.Count == 0)
                                    shotSelector = 0;
                                else
                                {
                                    int score = 0;
                                    bool stopCounting = false;

                                    for (int j = 0; j < gm.houseList.Count; j++)
                                    {
                                        if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.yellowTeamName)
                                        {
                                            freezeSelector = j;
                                            score++;
                                            stopCounting = true;
                                        }
                                    }
                                    if (stopCounting)
                                        shotSelector = 5;
                                    else
                                    {
                                        shotSelector = 0;
                                        takeOutSelector = 99;
                                    }
                                }
                            }
                            else
                            {
                                if (gm.houseList.Count == 0)
                                    shotSelector = 0;
                                else
                                {
                                    int score = 0;
                                    bool stopCounting = false;

                                    for (int j = 0; j < gm.houseList.Count; j++)
                                    {
                                        if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.redTeamName)
                                        {
                                            takeOutSelector = j;
                                            score++;
                                            stopCounting = true;
                                        }
                                    }
                                    if (stopCounting)
                                        shotSelector = 4;
                                    else
                                    {
                                        shotSelector = 0;
                                        takeOutSelector = 99;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!gm.redHammer)
                        {
                            if (redTeam)
                            {
                                if (gm.houseList.Count == 0)
                                    shotSelector = 0;
                                else
                                {
                                    int score = 0;
                                    bool stopCounting = false;

                                    for (int j = 0; j < gm.houseList.Count; j++)
                                    {
                                        if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.redTeamName)
                                        {
                                            takeOutSelector = j;
                                            score++;
                                            stopCounting = true;
                                        }
                                    }
                                    Debug.Log("TakeOut - " + takeOutSelector);
                                    Debug.Log("House Score " + score);
                                    if (stopCounting)
                                        shotSelector = 4;
                                    else
                                    {
                                        shotSelector = 0;
                                        takeOutSelector = 99;
                                    }
                                }
                            }
                            else
                            {
                                if (gm.houseList.Count == 0)
                                    shotSelector = 0;
                                else
                                {
                                    int score = 0;
                                    bool stopCounting = false;

                                    for (int j = 0; j < gm.houseList.Count; j++)
                                    {
                                        if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.yellowTeamName)
                                        {
                                            freezeSelector = j;
                                            score++;
                                            stopCounting = true;
                                        }
                                    }
                                    if (stopCounting)
                                        shotSelector = 5;
                                    else
                                    {
                                        shotSelector = 0;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (redTeam)
                            {
                                if (gm.houseList.Count == 0)
                                    shotSelector = 0;
                                else
                                {
                                    int score = 0;
                                    bool stopCounting = false;

                                    for (int j = 0; j < gm.houseList.Count; j++)
                                    {
                                        if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.yellowTeamName)
                                        {
                                            freezeSelector = j;
                                            score++;
                                            stopCounting = true;
                                        }
                                    }
                                    if (stopCounting)
                                        shotSelector = 5;
                                    else
                                    {
                                        shotSelector = 0;
                                    }
                                }
                            }
                            else
                            {
                                if (gm.houseList.Count == 0)
                                    shotSelector = 0;
                                else
                                {
                                    int score = 0;
                                    bool stopCounting = false;

                                    for (int j = 0; j < gm.houseList.Count; j++)
                                    {
                                        if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.redTeamName)
                                        {
                                            takeOutSelector = j;
                                            score++;
                                            stopCounting = true;
                                        }
                                    }
                                    if (stopCounting)
                                        shotSelector = 4;
                                    else
                                    {
                                        shotSelector = 0;
                                    }
                                }
                            }
                        }
                    }
                }
                //and the ai chooses an aggressive strategy...
                //aggressive on aggressive - high prob of guards and rocks in house
                //player rocks based on draw and guard accuracy, ai rocks unhindered
                else
                {
                    if (i % 2 == 0)
                    {
                        if (gm.redHammer)
                        {
                            if (gm.houseList.Count == 0)
                                shotSelector = 0;
                            else
                            {
                                int score = 0;
                                bool stopCounting = false;

                                for (int j = 0; j < gm.houseList.Count; j++)
                                {
                                    if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.redTeamName)
                                    {
                                        freezeSelector = j;
                                        score++;
                                        stopCounting = true;
                                    }
                                }
                                if (stopCounting)
                                    shotSelector = 5;
                                else
                                    shotSelector = 0;
                            }
                        }
                        else
                        {
                            if (gm.houseList.Count == 0)
                                shotSelector = 0;
                            else
                            {
                                int score = 0;
                                bool stopCounting = false;

                                for (int j = 0; j < gm.houseList.Count; j++)
                                {
                                    if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.yellowTeamName)
                                    {
                                        freezeSelector = j;
                                        score++;
                                        stopCounting = true;
                                    }
                                }

                                if (stopCounting)
                                    shotSelector = 5;
                                else
                                    shotSelector = 0;
                            }
                        }
                    }
                    else
                    {
                        if (!gm.redHammer)
                        {
                            if (gm.houseList.Count == 0)
                                shotSelector = 0;
                            else
                            {
                                int score = 0;
                                bool stopCounting = false;

                                for (int j = 0; j < gm.houseList.Count; j++)
                                {
                                    if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.yellowTeamName)
                                    {
                                        freezeSelector = j;
                                        score++;
                                        stopCounting = true;
                                    }
                                }

                                if (stopCounting)
                                    shotSelector = 5;
                                else
                                    shotSelector = 0;
                            }
                        }
                        else
                        {
                            if (gm.houseList.Count == 0)
                                shotSelector = 0;
                            else
                            {
                                int score = 0;
                                bool stopCounting = false;

                                for (int j = 0; j < gm.houseList.Count; j++)
                                {
                                    if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.redTeamName)
                                    {
                                        freezeSelector = j;
                                        score++;
                                        stopCounting = true;
                                    }
                                }

                                if (stopCounting)
                                    shotSelector = 5;
                                else
                                    shotSelector = 0;
                            }
                        }
                    }
                }
            }
            //if the player chooses a defensive strategy...
            else
            {
                //defensive on defensive - low prob of any rocks in play
                //player rocks based on takeOut accuracy, ai rocks based on takeOut accuracy
                if (!aiAgg)
                {
                    if (i % 2 == 0)
                    {
                        if (gm.redHammer)
                        {
                            if (gm.houseList.Count == 0)
                                shotSelector = 0;
                            else
                            {
                                int score = 0;
                                bool stopCounting = false;

                                for (int j = 0; j < gm.houseList.Count; j++)
                                {
                                    if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.redTeamName)
                                    {
                                        takeOutSelector = j;
                                        score++;
                                        stopCounting = true;
                                    }
                                }
                                if (stopCounting)
                                    shotSelector = 4;
                                else
                                    shotSelector = 0;
                            }
                        }
                        else
                        {
                            if (gm.houseList.Count == 0)
                                shotSelector = 0;
                            else
                            {
                                int score = 0;
                                bool stopCounting = false;

                                for (int j = 0; j < gm.houseList.Count; j++)
                                {
                                    if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.yellowTeamName)
                                    {
                                        takeOutSelector = j;
                                        score++;
                                        stopCounting = true;
                                    }
                                }
                                if (stopCounting)
                                    shotSelector = 4;
                                else
                                    shotSelector = 0;
                            }
                        }
                    }
                    else
                    {
                        if (!gm.redHammer)
                        {
                            if (gm.houseList.Count == 0)
                                shotSelector = 0;
                            else
                            {
                                int score = 0;
                                bool stopCounting = false;

                                for (int j = 0; j < gm.houseList.Count; j++)
                                {
                                    if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.redTeamName)
                                    {
                                        takeOutSelector = j;
                                        score++;
                                        stopCounting = true;
                                    }
                                }
                                if (stopCounting)
                                    shotSelector = 4;
                                else
                                    shotSelector = 0;
                            }
                        }
                        else
                        {
                            if (gm.houseList.Count == 0)
                                shotSelector = 0;
                            else
                            {
                                int score = 0;
                                bool stopCounting = false;

                                for (int j = 0; j < gm.houseList.Count; j++)
                                {
                                    if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.yellowTeamName)
                                    {
                                        takeOutSelector = j;
                                        score++;
                                        stopCounting = true;
                                    }
                                }
                                if (stopCounting)
                                    shotSelector = 4;
                                else
                                    shotSelector = 0;
                            }
                        }
                    }
                }
                //defensive on aggressive - prob ai guards and rocks in house based on player's take out accuracy
                //player rocks based on takeOut accuracy, ai rocks based on takeOut accuracy
                else
                {
                    if (i % 2 == 0)
                    {
                        if (gm.redHammer)
                        {
                            if (redTeam)
                            {
                                if (gm.houseList.Count == 0)
                                    shotSelector = 0;
                                else
                                {
                                    int score = 0;
                                    bool stopCounting = false;

                                    for (int j = 0; j < gm.houseList.Count; j++)
                                    {
                                        if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.redTeamName)
                                        {
                                            takeOutSelector = j;
                                            score++;
                                            stopCounting = true;
                                        }
                                    }
                                    if (stopCounting)
                                        shotSelector = 4;
                                    else
                                        shotSelector = 0;
                                }
                            }
                            else
                            {
                                if (gm.houseList.Count == 0)
                                    shotSelector = 0;
                                else
                                {
                                    int score = 0;
                                    bool stopCounting = false;

                                    for (int j = 0; j < gm.houseList.Count; j++)
                                    {
                                        if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.yellowTeamName)
                                        {
                                            freezeSelector = j;
                                            score++;
                                            stopCounting = true;
                                        }
                                    }
                                    if (stopCounting)
                                        shotSelector = 5;
                                    else
                                        shotSelector = 0;
                                }
                            }
                        }
                        else
                        {
                            if (redTeam)
                            {
                                if (gm.houseList.Count == 0)
                                    shotSelector = 0;
                                else
                                {
                                    int score = 0;
                                    bool stopCounting = false;

                                    for (int j = 0; j < gm.houseList.Count; j++)
                                    {
                                        if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.yellowTeamName)
                                        {
                                            freezeSelector = j;
                                            score++;
                                            stopCounting = true;
                                        }
                                    }
                                    if (stopCounting)
                                        shotSelector = 5;
                                    else
                                        shotSelector = 0;
                                }
                            }
                            else
                            {
                                if (gm.houseList.Count == 0)
                                    shotSelector = 0;
                                else
                                {
                                    int score = 0;
                                    bool stopCounting = false;

                                    for (int j = 0; j < gm.houseList.Count; j++)
                                    {
                                        if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.redTeamName)
                                        {
                                            takeOutSelector = j;
                                            score++;
                                            stopCounting = true;
                                        }
                                    }
                                    if (stopCounting)
                                        shotSelector = 4;
                                    else
                                        shotSelector = 0;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!gm.redHammer)
                        {
                            if (redTeam)
                            {
                                if (gm.houseList.Count == 0)
                                    shotSelector = 0;
                                else
                                {
                                    int score = 0;
                                    bool stopCounting = false;

                                    for (int j = 0; j < gm.houseList.Count; j++)
                                    {
                                        if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.redTeamName)
                                        {
                                            takeOutSelector = j;
                                            score++;
                                            stopCounting = true;
                                        }
                                    }
                                    if (stopCounting)
                                        shotSelector = 4;
                                    else
                                        shotSelector = 0;
                                }
                            }
                            else
                            {
                                if (gm.houseList.Count == 0)
                                    shotSelector = 0;
                                else
                                {
                                    int score = 0;
                                    bool stopCounting = false;

                                    for (int j = 0; j < gm.houseList.Count; j++)
                                    {
                                        if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.yellowTeamName)
                                        {
                                            freezeSelector = j;
                                            score++;
                                            stopCounting = true;
                                        }
                                    }
                                    if (stopCounting)
                                        shotSelector = 5;
                                    else
                                        shotSelector = 0;
                                }
                            }
                        }
                        else
                        {
                            if (redTeam)
                            {
                                if (gm.houseList.Count == 0)
                                    shotSelector = 0;
                                else
                                {
                                    int score = 0;
                                    bool stopCounting = false;

                                    for (int j = 0; j < gm.houseList.Count; j++)
                                    {
                                        if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.yellowTeamName)
                                        {
                                            freezeSelector = j;
                                            score++;
                                            stopCounting = true;
                                        }
                                    }
                                    if (stopCounting)
                                        shotSelector = 5;
                                    else
                                        shotSelector = 0;
                                }
                            }
                            else
                            {
                                if (gm.houseList.Count == 0)
                                    shotSelector = 0;
                                else
                                {
                                    int score = 0;
                                    bool stopCounting = false;

                                    for (int j = 0; j < gm.houseList.Count; j++)
                                    {
                                        if (!stopCounting && gm.houseList[j].rockInfo.teamName == gsp.redTeamName)
                                        {
                                            takeOutSelector = j;
                                            score++;
                                            stopCounting = true;
                                        }
                                    }
                                    if (stopCounting)
                                        shotSelector = 4;
                                    else
                                        shotSelector = 0;
                                }
                            }
                        }
                    }
                }
            }

            Debug.Log("Team Yellow TakeOut Accuracy " + tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue());

            Debug.Log("Team Red Draw Accuracy " + tm.teamRed[shooter].charStats.drawAccuracy.GetValue());

            Random.InitState((int)System.DateTime.Now.Ticks);
            switch (shotSelector)
            {
                case 0:
                    Debug.Log("Case 0 - House");
                    Debug.Log("Case 0 - " + houseCount + " - i is " + i);
                    placeSelector = 9;
                    if (houseCount > 5)
                    {
                        placeSelector = 10;
                        rockPos[i] = placePos[placeSelector];
                    }
                    else
                    {
                        houseCount++;
                        if (i % 2 == 1)
                        {
                            Random.InitState((int)System.DateTime.Now.Ticks);
                            if (gm.redHammer)
                                rockPos[i] = placePos[placeSelector]
                                    + (Random.insideUnitCircle
                                    * (1.5f - (0.05f * tm.teamRed[shooter].charStats.drawAccuracy.GetValue())));
                            else
                                rockPos[i] = placePos[placeSelector]
                                    + (Random.insideUnitCircle
                                    * (1.5f - (0.05f * tm.teamYellow[shooter].charStats.drawAccuracy.GetValue())));
                        }
                        else
                        {
                            Random.InitState((int)System.DateTime.Now.Ticks);
                            if (gm.redHammer)
                                rockPos[i] = placePos[placeSelector]
                                    + (Random.insideUnitCircle
                                    * (1.5f - (0.05f * tm.teamYellow[shooter].charStats.drawAccuracy.GetValue())));
                            else
                                rockPos[i] = placePos[placeSelector]
                                    + (Random.insideUnitCircle
                                    * (1.5f - (0.05f * tm.teamRed[shooter].charStats.drawAccuracy.GetValue())));
                        }
                    }
                    Debug.Log("case 0 rockPos is - " + i + " - " + rockPos[i].x + ", " + rockPos[i].y);
                    break;
                case 1:
                    Debug.Log("Case 1 - Out");
                    placeSelector = 10;
                    rockPos[i] = placePos[placeSelector];
                    Debug.Log("case 1 rockPos is - " + rockPos[i].x + ", " + rockPos[i].y);
                    break;
                case 2:
                    Debug.Log("Case 2 - Out");
                    placeSelector = 10;
                    rockPos[i] = placePos[placeSelector];
                    Debug.Log("case 2 rockPos is - " + rockPos[i].x + ", " + rockPos[i].y);
                    break;
                case 3:
                    Debug.Log("Case 3 - Guard");
                    Debug.Log("case 3 rockPos is - " + rockPos[i].x + ", " + rockPos[i].y);

                    break;

                case 4:
                    if (i % 2 == 1)
                    {
                        if (gm.redHammer)
                        {
                            //takeOut check
                            if (Random.Range(0f, 10f) < tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())
                            {
                                Random.InitState((int)System.DateTime.Now.Ticks);

                                if (Random.Range(0f, 15f) < tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())
                                {
                                    placeSelector = 10;
                                    rockPos[i] = placePos[placeSelector];
                                }
                                else
                                {
                                    placeSelector = 9;
                                    rockPos[i] = rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex]
                                        + (Random.insideUnitCircle
                                        * (1.5f - (0.05f * tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())));
                                }
                                rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex] = placePos[10];
                            }
                            else
                            {
                                Random.InitState((int)System.DateTime.Now.Ticks);

                                if (Random.Range(0f, 10f) < tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())
                                {
                                    placeSelector = 10;
                                    rockPos[i] = placePos[placeSelector];
                                }
                                else
                                {
                                    placeSelector = 9;
                                    rockPos[i] = rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex]
                                        + (Random.insideUnitCircle
                                        * (1.5f - (0.05f * tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())));
                                }
                                rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex].x +=
                                    Random.Range(0f, 0.05f * tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue());

                                rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex].y +=
                                    0.05f * tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue();
                            }
                        }
                        else
                        {
                            Random.InitState((int)System.DateTime.Now.Ticks);
                            if (Random.Range(0f, 10f) < tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())
                            {
                                Random.InitState((int)System.DateTime.Now.Ticks);

                                if (Random.Range(0f, 15f) > tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())
                                {
                                    placeSelector = 10;
                                    rockPos[i] = placePos[placeSelector];
                                }
                                else
                                {
                                    placeSelector = 9;
                                    rockPos[i] = rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex]
                                        + (Random.insideUnitCircle
                                        * (1.5f - (0.05f * tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())));
                                }

                                rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex] = placePos[10];
                            }
                            else
                            {
                                Random.InitState((int)System.DateTime.Now.Ticks);

                                if (Random.Range(0f, 10f) < tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())
                                {
                                    placeSelector = 10;
                                    rockPos[i] = placePos[placeSelector];
                                }
                                else
                                {
                                    placeSelector = 9;
                                    rockPos[i] = rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex]
                                        + (Random.insideUnitCircle
                                        * (1.5f - (0.05f * tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())));
                                }
                                rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex].x +=
                                    Random.Range(0f, 0.05f * tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue());

                                rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex].y +=
                                    0.05f * tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue();
                            }
                        }
                    }
                    else
                    {
                        if (!gm.redHammer)
                        {
                            //takeOut check
                            if (Random.Range(0f, 10f) < tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())
                            {
                                Random.InitState((int)System.DateTime.Now.Ticks);

                                if (Random.Range(0f, 15f) < tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())
                                {
                                    placeSelector = 10;
                                    rockPos[i] = placePos[placeSelector];
                                }
                                else
                                {
                                    placeSelector = 9;
                                    rockPos[i] = rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex]
                                        + (Random.insideUnitCircle
                                        * (1.5f - (0.05f * tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())));
                                }
                                rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex] = placePos[10];
                            }
                            else
                            {
                                Random.InitState((int)System.DateTime.Now.Ticks);

                                if (Random.Range(0f, 10f) < tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())
                                {
                                    placeSelector = 10;
                                    rockPos[i] = placePos[placeSelector];
                                }
                                else
                                {
                                    placeSelector = 9;
                                    rockPos[i] = rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex]
                                        + (Random.insideUnitCircle
                                        * (1.5f - (0.05f * tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue())));
                                }
                                Random.InitState((int)System.DateTime.Now.Ticks);

                                rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex].x +=
                                    Random.Range(0f, 0.05f * tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue());

                                rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex].y +=
                                    0.05f * tm.teamRed[shooter].charStats.takeOutAccuracy.GetValue();
                            }
                        }
                        else
                        {
                            Random.InitState((int)System.DateTime.Now.Ticks);
                            if (Random.Range(0f, 10f) < tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())
                            {
                                Random.InitState((int)System.DateTime.Now.Ticks);

                                if (Random.Range(0f, 15f) > tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())
                                {
                                    placeSelector = 10;
                                    rockPos[i] = placePos[placeSelector];
                                }
                                else
                                {
                                    Random.InitState((int)System.DateTime.Now.Ticks);
                                    placeSelector = 9;
                                    rockPos[i] = rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex]
                                        + (Random.insideUnitCircle
                                        * (1.5f - (0.05f * tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())));
                                }

                                rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex] = placePos[10];
                            }
                            else
                            {
                                Random.InitState((int)System.DateTime.Now.Ticks);

                                if (Random.Range(0f, 10f) < tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())
                                {
                                    placeSelector = 10;
                                    rockPos[i] = placePos[placeSelector];
                                }
                                else
                                {
                                    placeSelector = 9;
                                    rockPos[i] = rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex]
                                        + (Random.insideUnitCircle
                                        * (1.5f - (0.05f * tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue())));
                                }
                                rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex].x +=
                                    Random.Range(0f, 0.05f * tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue());

                                rockPos[gm.houseList[takeOutSelector].rockInfo.rockIndex].y +=
                                    0.05f * tm.teamYellow[shooter].charStats.takeOutAccuracy.GetValue();
                            }
                        }
                    }
                    Debug.Log("Case 4 - Takeout - " + takeOutSelector);
                    break;

                case 5:
                    Debug.Log("Case 5 - Freeze - " + freezeSelector);

                    if (i % 2 == 1)
                    {
                        if (gm.redHammer)
                        {
                            if (Random.Range(0f, 10f) < tm.teamRed[shooter].charStats.drawAccuracy.GetValue())
                            {
                                rockPos[i].y = gm.houseList[freezeSelector].rock.transform.position.y
                                    - (0.5f + (0.05f * tm.teamRed[shooter].charStats.drawAccuracy.GetValue()));
                                rockPos[i].x = gm.houseList[freezeSelector].rock.transform.position.x;
                                rockPos[i] = rockPos[i] + (Random.insideUnitCircle
                                        * (1f - (0.05f * tm.teamRed[shooter].charStats.drawAccuracy.GetValue())));
                            }
                            else
                            {
                                rockPos[i].y = gm.houseList[freezeSelector].rock.transform.position.y
                                    - (1f + (0.05f * tm.teamRed[shooter].charStats.drawAccuracy.GetValue()));
                                rockPos[i].x = gm.houseList[freezeSelector].rock.transform.position.x;
                                rockPos[i] = rockPos[i] + (Random.insideUnitCircle
                                        * (1f - (0.05f * tm.teamRed[shooter].charStats.drawAccuracy.GetValue())));
                            }
                        }
                        else
                        {
                            if (Random.Range(0f, 10f) < tm.teamYellow[shooter].charStats.drawAccuracy.GetValue())
                            {
                                rockPos[i].y = gm.houseList[freezeSelector].rock.transform.position.y
                                    - (0.5f + (0.05f * tm.teamYellow[shooter].charStats.drawAccuracy.GetValue()));
                                rockPos[i].x = gm.houseList[freezeSelector].rock.transform.position.x;
                                rockPos[i] = rockPos[i] + (Random.insideUnitCircle
                                        * (1f - (0.05f * tm.teamYellow[shooter].charStats.drawAccuracy.GetValue())));
                            }
                            else
                            {
                                rockPos[i].y = gm.houseList[freezeSelector].rock.transform.position.y
                                    - (1f + (0.05f * tm.teamYellow[shooter].charStats.drawAccuracy.GetValue()));
                                rockPos[i].x = gm.houseList[freezeSelector].rock.transform.position.x;
                                rockPos[i] = rockPos[i] + (Random.insideUnitCircle
                                        * (1f - (0.05f * tm.teamYellow[shooter].charStats.drawAccuracy.GetValue())));
                            }
                        }
                    }
                    else
                    {
                        if (!gm.redHammer)
                        {
                            if (Random.Range(0f, 10f) < tm.teamRed[shooter].charStats.drawAccuracy.GetValue())
                            {
                                rockPos[i].y = gm.houseList[freezeSelector].rock.transform.position.y
                                    - (0.5f + (0.05f * tm.teamRed[shooter].charStats.drawAccuracy.GetValue()));
                                rockPos[i].x = gm.houseList[freezeSelector].rock.transform.position.x;
                                rockPos[i] = rockPos[i] + (Random.insideUnitCircle
                                        * (1f - (0.05f * tm.teamRed[shooter].charStats.drawAccuracy.GetValue())));
                            }
                            else
                            {
                                rockPos[i].y = gm.houseList[freezeSelector].rock.transform.position.y
                                    - (1f + (0.05f * tm.teamRed[shooter].charStats.drawAccuracy.GetValue()));
                                rockPos[i].x = gm.houseList[freezeSelector].rock.transform.position.x;
                                rockPos[i] = rockPos[i] + (Random.insideUnitCircle
                                        * (1f - (0.05f * tm.teamRed[shooter].charStats.drawAccuracy.GetValue())));
                            }
                        }
                        else
                        {
                            if (Random.Range(0f, 10f) < tm.teamYellow[shooter].charStats.drawAccuracy.GetValue())
                            {
                                rockPos[i].y = gm.houseList[freezeSelector].rock.transform.position.y
                                    - (0.5f + (0.05f * tm.teamYellow[shooter].charStats.drawAccuracy.GetValue()));
                                rockPos[i].x = gm.houseList[freezeSelector].rock.transform.position.x;
                                rockPos[i] = rockPos[i] + (Random.insideUnitCircle
                                        * (1f - (0.05f * tm.teamYellow[shooter].charStats.drawAccuracy.GetValue())));
                            }
                            else
                            {
                                rockPos[i].y = gm.houseList[freezeSelector].rock.transform.position.y
                                    - (1f + (0.05f * tm.teamYellow[shooter].charStats.drawAccuracy.GetValue()));
                                rockPos[i].x = gm.houseList[freezeSelector].rock.transform.position.x;
                                rockPos[i] = rockPos[i] + (Random.insideUnitCircle
                                        * (1f - (0.05f * tm.teamYellow[shooter].charStats.drawAccuracy.GetValue())));
                            }
                        }
                    }
                    break;

                default:
                    Debug.Log("Default - Out - " + i);
                    Debug.Log("Case " + shotSelector + " - " + i);
                    placeSelector = 10;
                    rockPos[i] = placePos[placeSelector];
                    Debug.Log("Default rockPos is - " + rockPos[i].x + ", " + rockPos[i].y);
                    break;
            }
        }

        for (int i = 8; i < rockCurrent; i++)
        {
            gm.rockList[i].rockInfo.placed = true;
            Debug.Log("Placed Rock Position " + i + " " + rockPos[i].x + ", " + rockPos[i].y);
        }

        Debug.Log("Round 2 - Placing Rocks - " + rockCurrent);
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < rockCurrent; i++)
        {
            Debug.Log("Rock Position " + i + " " + rockPos[i].x + ", " + rockPos[i].y);
            gm.rockList[i].rock.GetComponent<CircleCollider2D>().radius = 0.14f;
            gm.rockList[i].rock.GetComponent<SpriteRenderer>().enabled = false;
            gm.rockList[i].rock.GetComponent<SpringJoint2D>().enabled = false;
            gm.rockList[i].rock.GetComponent<Rock_Flick>().enabled = false;
            gm.rockList[i].rock.transform.parent = null;
            //rm.rb.DeadRock(i);
            yield return new WaitForEndOfFrame();
            Debug.Log("Rock Position " + i + " " + rockPos[i].x + ", " + rockPos[i].y);
            gm.rockList[i].rock.GetComponent<Rigidbody2D>().position = new Vector2(rockPos[i].x, rockPos[i].y);

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
            Debug.Log("rockList " + gm.rockList[i].rockInfo.teamName + " " + gm.rockList[i].rockInfo.rockNumber);
            //rm.rb.ShotUpdate(rockCurrent, gm.rockList[i].rockInfo.outOfPlay);
            yield return new WaitForEndOfFrame();
            gm.rockCurrent = rockCurrent - 1;
            gm.rockTotal = 16;
            yield return new WaitForEndOfFrame();


        }

        placed = true;
        gm.cm.HouseView();
        playerStratGO.SetActive(false);
    }
}
