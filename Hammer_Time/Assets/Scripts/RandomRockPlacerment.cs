using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRockPlacerment : MonoBehaviour
{
    public GameManager gm;
    public RockManager rm;
    public TeamManager tm;

    public bool placed;
    int rockCurrent;

    public Vector2[] placePos;
    public Vector2[] rockPos;

    public GameObject playerStratGO;
    public bool aggressive;

    // Start is called before the first frame update
    void Start()
    {
        placed = false;
    }

    // Update is called once per frame
    public void OnRockPlace(int rockCrnt, bool redTeam)
    {
        placed = false;
        rockCurrent = rockCrnt;
        rockPos = new Vector2[rockCurrent];

        //StartCoroutine(RandomRockPlace());

        StartCoroutine(PlayerStrategy());
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
        placed = true;
    }

    IEnumerator PlayerStrategy()
    {
        playerStratGO.SetActive(true);
        yield return new WaitForEndOfFrame();

        yield return new WaitUntil(() => !playerStratGO.activeSelf);

        yield return StartCoroutine(FirstPlacement());

        //if (rockCurrent < 8)
        //{
        //    yield return new WaitUntil(() => !playerStratGO.activeSelf);
        //}
        //else
        //    yield return new WaitForEndOfFrame();

        yield return new WaitForEndOfFrame();

        //rocksPlaced = true;
        gm.rockCurrent = rockCurrent - 1;
        gm.rockTotal = 16;
        yield return new WaitForEndOfFrame();
        placed = true;
    }

    IEnumerator FirstPlacement()
    {
        
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        bool redTeam;
        if (gsp.teamName == gsp.redTeamName)
            redTeam = true;
        else
            redTeam = false;

        int houseCount = 0;
        bool aiAgg = false;

        int guardCounter = 0;

        bool[] guardCount = new bool[9];
        for (int i = 0; i < 9; i++)
            guardCount[i] = false;

        if (redTeam)
        {
            if (gsp.redScore > gsp.yellowScore)
                aiAgg = true;
            else
                aiAgg = false;
        }
        else
        {
            if (gsp.yellowScore > gsp.redScore)
                aiAgg = true;
            else
                aiAgg = false;
        }

        for (int i = 0; i < rockCurrent; i++)
        {
            int placeSelector;
            int shotSelector;


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
                                shotSelector = Random.Range(0, 4);
                            else
                            {

                                if (guardCounter < 3)
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.guardAccuracy)
                                        shotSelector = 4;
                                    else
                                        shotSelector = Random.Range(0, 3);
                                }
                                else if (guardCounter < 5)
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.guardAccuracy)
                                        shotSelector = Random.Range(2, 4);
                                    else
                                        shotSelector = Random.Range(0, 3);
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.drawAccuracy)
                                        shotSelector = Random.Range(0, 2);
                                    else
                                        shotSelector = Random.Range(2, 4);
                                }
                            }
                        }
                        else
                        {
                            if (redTeam)
                            {
                                if (guardCounter < 3)
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.guardAccuracy)
                                        shotSelector = 4;
                                    else
                                        shotSelector = Random.Range(0, 3);
                                }
                                else if (guardCounter < 5)
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.guardAccuracy)
                                        shotSelector = Random.Range(2, 4);
                                    else
                                        shotSelector = Random.Range(0, 3);
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.drawAccuracy)
                                        shotSelector = Random.Range(0, 2);
                                    else
                                        shotSelector = Random.Range(2, 4);
                                }
                            }
                            else
                                shotSelector = Random.Range(0, 4);
                        }
                    }
                    else
                    {
                        if (!gm.redHammer)
                        {
                            if (redTeam)
                                shotSelector = Random.Range(0, 4);
                            else
                            {

                                if (guardCounter < 3)
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.guardAccuracy)
                                        shotSelector = 4;
                                    else
                                        shotSelector = Random.Range(0, 3);
                                }
                                else if (guardCounter < 5)
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.guardAccuracy)
                                        shotSelector = Random.Range(2, 4);
                                    else
                                        shotSelector = Random.Range(0, 3);
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.drawAccuracy)
                                        shotSelector = Random.Range(0, 2);
                                    else
                                        shotSelector = Random.Range(2, 4);
                                }
                            }
                        }
                        else
                        {
                            if (redTeam)
                            {
                                if (guardCounter < 3)
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.guardAccuracy)
                                        shotSelector = 4;
                                    else
                                        shotSelector = Random.Range(0, 3);
                                }
                                else if (guardCounter < 5)
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.guardAccuracy)
                                        shotSelector = Random.Range(2, 4);
                                    else
                                        shotSelector = Random.Range(0, 3);
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.drawAccuracy)
                                        shotSelector = Random.Range(0, 2);
                                    else
                                        shotSelector = Random.Range(2, 4);
                                }
                            }
                            else
                                shotSelector = Random.Range(0, 4);
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
                            if (redTeam)
                            {
                                if (guardCounter < 5)
                                    shotSelector = Random.Range(2, 4);
                                else
                                    shotSelector = Random.Range(0, 3);
                            }
                            else
                            {

                                if (guardCounter < 5)
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.guardAccuracy)
                                        shotSelector = 4;
                                    else
                                        shotSelector = Random.Range(0, 3);
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.drawAccuracy)
                                        shotSelector = 0;
                                    else
                                        shotSelector = Random.Range(2, 4);
                                }
                            }
                        }
                        else
                        {
                            if (redTeam)
                            {
                                if (guardCounter < 5)
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.guardAccuracy)
                                        shotSelector = 4;
                                    else
                                        shotSelector = Random.Range(0, 3);
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.drawAccuracy)
                                        shotSelector = 0;
                                    else
                                        shotSelector = Random.Range(2, 4);
                                }
                            }
                            else
                            {
                                if (guardCounter < 5)
                                    shotSelector = Random.Range(2, 4);
                                else
                                    shotSelector = Random.Range(0, 2);
                            }
                        }
                    }
                    else
                    {
                        if (!gm.redHammer)
                        {
                            if (redTeam)
                            {
                                if (guardCounter < 5)
                                    shotSelector = Random.Range(2, 4);
                                else
                                    shotSelector = Random.Range(0, 3);
                            }
                            else
                            {

                                if (guardCounter < 5)
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.guardAccuracy)
                                        shotSelector = 4;
                                    else
                                        shotSelector = Random.Range(0, 3);
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.drawAccuracy)
                                        shotSelector = 0;
                                    else
                                        shotSelector = Random.Range(2, 4);
                                }
                            }
                        }
                        else
                        {
                            if (redTeam)
                            {
                                if (guardCounter < 5)
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.guardAccuracy)
                                        shotSelector = 4;
                                    else
                                        shotSelector = Random.Range(0, 3);
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) < gsp.cStats.drawAccuracy)
                                        shotSelector = 0;
                                    else
                                        shotSelector = Random.Range(2, 4);
                                }
                            }
                            else
                            {
                                if (guardCounter < 5)
                                    shotSelector = Random.Range(2, 4);
                                else
                                    shotSelector = Random.Range(0, 2);
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
                            if (redTeam)
                            {
                                if (Random.Range(0f, 10f) > gsp.cStats.takeOutAccuracy)
                                    shotSelector = 0;
                                else
                                    shotSelector = 1;
                            }
                            else
                            {
                                if (Random.Range(0f, 10f) < gsp.cStats.takeOutAccuracy)
                                    shotSelector = Random.Range(0, 3);
                                else
                                    shotSelector = 1;
                            }
                        }
                        else
                        {
                            if (redTeam)
                            {
                                if (Random.Range(0f, 10f) < gsp.cStats.takeOutAccuracy)
                                    shotSelector = Random.Range(0, 3);
                                else
                                    shotSelector = 1;
                            }
                            else
                            {
                                if (Random.Range(0f, 10f) > gsp.cStats.takeOutAccuracy)
                                    shotSelector = 0;
                                else
                                    shotSelector = 1;
                            }
                        }
                    }
                    else
                    {
                        if (!gm.redHammer)
                        {
                            if (redTeam)
                            {
                                if (Random.Range(0f, 10f) > gsp.cStats.takeOutAccuracy)
                                    shotSelector = 0;
                                else
                                    shotSelector = 1;
                            }
                            else
                            {
                                if (Random.Range(0f, 10f) < gsp.cStats.takeOutAccuracy)
                                    shotSelector = Random.Range(0, 3);
                                else
                                    shotSelector = 1;
                            }
                        }
                        else
                        {
                            if (redTeam)
                            {
                                if (Random.Range(0f, 10f) < gsp.cStats.takeOutAccuracy)
                                    shotSelector = Random.Range(0, 3);
                                else
                                    shotSelector = 1;
                            }
                            else
                            {
                                if (Random.Range(0f, 10f) > gsp.cStats.takeOutAccuracy)
                                    shotSelector = 0;
                                else
                                    shotSelector = 1;
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
                                if (guardCounter < 3)
                                    shotSelector = 4;
                                else if (guardCounter < 5)
                                {
                                    if (Random.Range(0f, 10f) > gsp.cStats.takeOutAccuracy)
                                        shotSelector = 4;
                                    else
                                        shotSelector = Random.Range(0, 3);
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) > gsp.cStats.takeOutAccuracy)
                                        shotSelector = 3;
                                    else
                                        shotSelector = Random.Range(0, 3);
                                }
                            }
                            else
                            {
                                if (Random.Range(0f, 10f) < gsp.cStats.takeOutAccuracy)
                                    shotSelector = Random.Range(0, 3);
                                else
                                    shotSelector = 1;
                            }
                        }
                        else
                        {
                            if (redTeam)
                            {
                                if (Random.Range(0f, 10f) < gsp.cStats.takeOutAccuracy)
                                    shotSelector = Random.Range(0, 3);
                                else
                                    shotSelector = 1;
                            }
                            else
                            {
                                if (guardCounter < 3)
                                    shotSelector = 4;
                                else if (guardCounter < 5)
                                {
                                    if (Random.Range(0f, 10f) > gsp.cStats.takeOutAccuracy)
                                        shotSelector = 4;
                                    else
                                        shotSelector = Random.Range(0, 3);
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) > gsp.cStats.takeOutAccuracy)
                                        shotSelector = 3;
                                    else
                                        shotSelector = Random.Range(0, 3);
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
                                if (guardCounter < 3)
                                    shotSelector = 4;
                                else if (guardCounter < 5)
                                {
                                    if (Random.Range(0f, 10f) > gsp.cStats.takeOutAccuracy)
                                        shotSelector = 4;
                                    else
                                        shotSelector = Random.Range(0, 3);
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) > gsp.cStats.takeOutAccuracy)
                                        shotSelector = 3;
                                    else
                                        shotSelector = Random.Range(0, 3);
                                }
                            }
                            else
                            {
                                if (Random.Range(0f, 10f) < gsp.cStats.takeOutAccuracy)
                                    shotSelector = Random.Range(0, 3);
                                else
                                    shotSelector = 1;
                            }
                        }
                        else
                        {
                            if (redTeam)
                            {
                                if (Random.Range(0f, 10f) < gsp.cStats.takeOutAccuracy)
                                    shotSelector = Random.Range(0, 3);
                                else
                                    shotSelector = 1;
                            }
                            else
                            {
                                if (guardCounter < 3)
                                    shotSelector = 4;
                                else if (guardCounter < 5)
                                {
                                    if (Random.Range(0f, 10f) > gsp.cStats.takeOutAccuracy)
                                        shotSelector = 4;
                                    else
                                        shotSelector = Random.Range(0, 3);
                                }
                                else
                                {
                                    if (Random.Range(0f, 10f) > gsp.cStats.takeOutAccuracy)
                                        shotSelector = 3;
                                    else
                                        shotSelector = Random.Range(0, 3);
                                }
                            }
                        }
                    }
                }
            }


            Debug.Log("House Count is " + houseCount);

            Debug.Log("Guard Count is " + guardCounter);
            switch (shotSelector)
            {
                case 0:
                    Debug.Log("Case 0 - House");
                    Debug.Log("Case 0 - " + houseCount);
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
                            {
                                if (redTeam)
                                    rockPos[i] = placePos[placeSelector]
                                        + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.drawAccuracy)));
                                else
                                    rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 1.25f);
                            }
                            else 
                            {
                                if (redTeam)
                                    rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 1.25f);
                                else
                                    rockPos[i] = placePos[placeSelector]
                                        + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.drawAccuracy)));
                            }
                        }
                        else
                        {
                            if (gm.redHammer)
                            {
                                if (redTeam)
                                    rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 1.25f);
                                else
                                    rockPos[i] = placePos[placeSelector]
                                        + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.drawAccuracy)));
                            }
                            else
                            {
                                if (redTeam)
                                    rockPos[i] = placePos[placeSelector]
                                        + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.drawAccuracy)));
                                else
                                    rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 1.25f);
                            }
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
                                if (i % 2 == 1)
                                {
                                    if (gm.redHammer)
                                    {
                                        if (redTeam)
                                            rockPos[i] = placePos[placeSelector]
                                                + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.guardAccuracy)));
                                        else
                                            rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 0.5f);
                                    }
                                    else
                                    {
                                        if (redTeam)
                                            rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 0.5f);
                                        else
                                            rockPos[i] = placePos[placeSelector]
                                                + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.guardAccuracy)));
                                    }
                                }
                                else
                                {
                                    if (gm.redHammer)
                                    {
                                        if (redTeam)
                                            rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 0.5f);
                                        else
                                            rockPos[i] = placePos[placeSelector]
                                                + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.guardAccuracy)));
                                    }
                                    else
                                    {
                                        if (redTeam)
                                            rockPos[i] = placePos[placeSelector]
                                                + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.guardAccuracy)));
                                        else
                                            rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 0.5f);
                                    }
                                }
                            }
                        }
                        else
                        {
                            guardCount[placeSelector] = true;
                            if (i % 2 == 1)
                            {
                                if (gm.redHammer)
                                {
                                    if (redTeam)
                                        rockPos[i] = placePos[placeSelector]
                                            + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.guardAccuracy)));
                                    else
                                        rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 0.5f);
                                }
                                else
                                {
                                    if (redTeam)
                                        rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 0.5f);
                                    else
                                        rockPos[i] = placePos[placeSelector]
                                            + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.guardAccuracy)));
                                }
                            }
                            else
                            {
                                if (gm.redHammer)
                                {
                                    if (redTeam)
                                        rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 0.5f);
                                    else
                                        rockPos[i] = placePos[placeSelector]
                                            + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.guardAccuracy)));
                                }
                                else
                                {
                                    if (redTeam)
                                        rockPos[i] = placePos[placeSelector]
                                            + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.guardAccuracy)));
                                    else
                                        rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 0.5f);
                                }
                            }
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
                                if (i % 2 == 1)
                                {
                                    if (gm.redHammer)
                                    {
                                        if (redTeam)
                                            rockPos[i] = placePos[placeSelector]
                                                + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.guardAccuracy)));
                                        else
                                            rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 0.5f);
                                    }
                                    else
                                    {
                                        if (redTeam)
                                            rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 0.5f);
                                        else
                                            rockPos[i] = placePos[placeSelector]
                                                + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.guardAccuracy)));
                                    }
                                }
                                else
                                {
                                    if (gm.redHammer)
                                    {
                                        if (redTeam)
                                            rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 0.5f);
                                        else
                                            rockPos[i] = placePos[placeSelector]
                                                + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.guardAccuracy)));
                                    }
                                    else
                                    {
                                        if (redTeam)
                                            rockPos[i] = placePos[placeSelector]
                                                + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.guardAccuracy)));
                                        else
                                            rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 0.5f);
                                    }
                                }
                            }
                        }
                        else
                        {
                            guardCount[placeSelector] = true;
                            if (i % 2 == 1)
                            {
                                if (gm.redHammer)
                                {
                                    if (redTeam)
                                        rockPos[i] = placePos[placeSelector]
                                            + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.guardAccuracy)));
                                    else
                                        rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 0.5f);
                                }
                                else
                                {
                                    if (redTeam)
                                        rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 0.5f);
                                    else
                                        rockPos[i] = placePos[placeSelector]
                                            + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.guardAccuracy)));
                                }
                            }
                            else
                            {
                                if (gm.redHammer)
                                {
                                    if (redTeam)
                                        rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 0.5f);
                                    else
                                        rockPos[i] = placePos[placeSelector]
                                            + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.guardAccuracy)));
                                }
                                else
                                {
                                    if (redTeam)
                                        rockPos[i] = placePos[placeSelector]
                                            + (Random.insideUnitCircle * (1.5f - (0.1f * gsp.cStats.guardAccuracy)));
                                    else
                                        rockPos[i] = placePos[placeSelector] + (Random.insideUnitCircle * 0.5f);
                                }
                            }
                        }
                    }
                    Debug.Log("case 3 rockPos is - " + rockPos[i].x + ", " + rockPos[i].y);

                    break;
                default:
                    Debug.Log("Default - Out - " + i);
                    Debug.Log("Case " + shotSelector + " - " + i);
                    placeSelector = 10;
                    rockPos[i] = placePos[placeSelector];
                    Debug.Log("case 2 rockPos is - " + rockPos[i].x + ", " + rockPos[i].y);
                    break;
                    placeSelector = 10;
                    rockPos[i] = placePos[placeSelector];
                    Debug.Log("case 2 rockPos is - " + rockPos[i].x + ", " + rockPos[i].y);
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

        
    }
}
