using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public GameManager gm;
    public TutorialManager tm;
    public RockManager rm;

    Rock_Info rockInfo;
    Rock_Flick rockFlick;
    Rigidbody2D rockRB;

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

    public Vector2 takeOut;
    public Vector2 raise;

    public string testing;

    public Vector2 guardAccu;
    public Vector2 drawAccu;
    public Vector2 toAccu;

    bool inturn;
    float targetX;
    float takeOutX;
    float raiseY;

    GameObject closestRock;
    Rock_Info closestRockInfo;
    // OnEnable is called when the Game Object is enabled
    void OnEnable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        inturn = rm.inturn;
        if (Input.GetKeyDown(KeyCode.D))
        {
            rockInfo = gm.rockList[gm.rockCurrent].rockInfo;
            rockFlick = gm.rockList[gm.rockCurrent].rock.GetComponent<Rock_Flick>();
            rockRB = gm.rockList[gm.rockCurrent].rock.GetComponent<Rigidbody2D>();

            if (gm.houseList.Count != 0)
            {
                closestRock = gm.houseList[0].rock;
                closestRockInfo = gm.houseList[0].rockInfo;
            }

            StartCoroutine(Shot(testing));
            //StartCoroutine(Shot("Take Out"));
        }
    }

    public void OnShot(int rockCurrent)
    {
        rockInfo = gm.rockList[rockCurrent].rockInfo;
        rockFlick = gm.rockList[rockCurrent].rock.GetComponent<Rock_Flick>();
        rockRB = gm.rockList[rockCurrent].rock.GetComponent<Rigidbody2D>();

        if (gm.houseList.Count != 0)
        {
            closestRock = gm.houseList[0].rock;
            closestRockInfo = gm.houseList[0].rockInfo;
        }

        Aggressive(rockCurrent);
    }

    IEnumerator TakeOutTarget(int rockCurrent)
    {
        takeOut.y = -27.5f;

        if (gm.houseList.Count != 0)
        {//if an enemy rock is the closest rock
            if (closestRockInfo.teamName != rockInfo.teamName)
            {
                for (int i = 0; i < rockCurrent; i++)
                {
                    //if a rock is thrown and in play
                    if (gm.rockList[i].rockInfo.shotTaken && gm.rockList[i].rockInfo.inPlay)
                    {
                        //if the rock is in line with the closest rock
                        if (gm.rockList[i].rock.transform.position.x - Mathf.Abs(closestRock.transform.position.x) <= 0.35f)
                        {
                            // if the rock is in the guard zone
                            if (gm.rockList[i].rock.transform.position.y >= 1.6f)
                            {
                                targetX = gm.rockList[i].rock.transform.position.x;
                                takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                                yield break;
                            }
                            // if the rock is in front of the enemy closest rock
                            else if (gm.rockList[i].rock.transform.position.y >= closestRock.transform.position.y)
                            {
                                //if it's close enough to possibly knock the other rock
                                if (Vector2.Distance(gm.rockList[i].rock.transform.position, closestRock.transform.position) <= 0.35f)
                                {
                                    targetX = gm.rockList[i].rock.transform.position.x;
                                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                                    Debug.Log(gm.rockList[i].rockInfo.teamName + " " + gm.rockList[i].rockInfo.rockNumber);
                                    yield break;
                                }
                                //if it's in line with the closest rock
                                else if (gm.rockList[i].rock.transform.position.x - Mathf.Abs(closestRock.transform.position.x) <= 0.35f)
                                {
                                    targetX = gm.rockList[i].rock.transform.position.x;
                                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                                    Debug.Log(gm.rockList[i].rockInfo.teamName + " " + gm.rockList[i].rockInfo.rockNumber);
                                    yield break;
                                }
                            }
                        }
                        else
                        {
                            // or else the closest is exposed and we will take it out
                            targetX = closestRock.transform.position.x;
                            takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                            Debug.Log(closestRockInfo.teamName + " " + closestRockInfo.rockNumber);
                            yield break;
                        }
                    }
                }
            }
            else if (closestRockInfo.teamName == rockInfo.teamName)
            {
                for (int i = 1; i <= gm.houseList.Count; i++)
                {
                    if (gm.houseList[i].rockInfo.teamName != closestRockInfo.teamName)
                    {
                        if (Vector2.Distance(gm.houseList[i].rock.transform.position, closestRock.transform.position) >= 0.2f)
                        {
                            //if the rock is not in line with the closest rock
                            if (gm.rockList[i].rock.transform.position.x - Mathf.Abs(closestRock.transform.position.x) >= 0.15f)
                            {
                                //we will take it out
                                targetX = gm.rockList[i].rock.transform.position.x;
                                takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                                yield break;
                            }
                        }
                    }
                }
                for (int i = 0; i < rockCurrent; i++)
                {
                    //if a rock has been thrown and is in play
                    if (gm.rockList[i].rockInfo.shotTaken && gm.rockList[i].rockInfo.inPlay)
                    {
                        //if the rock is not in line with the closest rock
                        if (gm.rockList[i].rock.transform.position.x - Mathf.Abs(closestRock.transform.position.x) >= 0.15f)
                        {
                            //if the rock is in the guard zone
                            if (gm.rockList[i].rock.transform.position.y >= 1.6f)
                            {
                                //then we will take it out
                                targetX = gm.rockList[i].rock.transform.position.x;
                                takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                                yield break;
                            }
                            //or if the rock is in front of our closest rock
                            else if (gm.rockList[i].rock.transform.position.y >= closestRock.transform.position.y)
                            {
                                //it's not in line with our closest rock
                                if (Vector2.Distance(gm.rockList[i].rock.transform.position, closestRock.transform.position) >= 0.25f)
                                {
                                    //then we will take it out
                                    targetX = gm.rockList[i].rock.transform.position.x;
                                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                                    yield break;
                                }
                                //it's far enough away from our rock
                                else if (gm.rockList[i].rock.transform.position.x - Mathf.Abs(closestRock.transform.position.x) >= 0.25f)
                                {
                                    //then we will take it out
                                    targetX = gm.rockList[i].rock.transform.position.x;
                                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                                    yield break;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        else if (gm.houseList.Count == 0)
        {
            if (gm.redHammer)
            {
                for (int i = 0; i < rockCurrent; i++)
                {
                    //if a rock has been thrown and is in play
                    if (gm.rockList[i].rockInfo.shotTaken && gm.rockList[i].rockInfo.inPlay)
                    {
                        //if the rock is in the guard zone
                        if (gm.rockList[i].rock.transform.position.y >= 1.6f)
                        {
                            //if it's a corner guard
                            if (Mathf.Abs(gm.rockList[i].rock.transform.position.x) <= 0.25f)
                            {
                                if (gm.rockList[i].rockInfo.teamName != rockInfo.teamName)
                                {
                                    //then we will take it out
                                    targetX = gm.rockList[i].rock.transform.position.x;
                                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                                    Debug.Log(gm.rockList[i].rockInfo.teamName + " " + gm.rockList[i].rockInfo.rockNumber);
                                    yield break;
                                }
                                else
                                {
                                    //then we will raise it
                                    targetX = gm.rockList[i].rock.transform.position.x;
                                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                                    takeOut.y = raise.y;
                                    Debug.Log(gm.rockList[i].rockInfo.teamName + " " + gm.rockList[i].rockInfo.rockNumber);
                                    yield break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < rockCurrent; i++)
                {
                    //if a rock has been thrown and is in play
                    if (gm.rockList[i].rockInfo.shotTaken && gm.rockList[i].rockInfo.inPlay)
                    {
                        //if the rock is in the guard zone
                        if (gm.rockList[i].rock.transform.position.y >= 1.6f)
                        {
                            //if it's a centre guard
                            if (Mathf.Abs(gm.rockList[i].rock.transform.position.x) <= 0.25f)
                            {
                                if (gm.rockList[i].rockInfo.teamName != rockInfo.teamName)
                                {
                                    //then we will take it out
                                    targetX = gm.rockList[i].rock.transform.position.x;
                                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                                    Debug.Log(gm.rockList[i].rockInfo.teamName + " " + gm.rockList[i].rockInfo.rockNumber);
                                    yield break;
                                }
                                else
                                {
                                    //then we will raise it
                                    targetX = gm.rockList[i].rock.transform.position.x;
                                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                                    Debug.Log(gm.rockList[i].rockInfo.teamName + " " + gm.rockList[i].rockInfo.rockNumber);
                                    takeOut.y = raise.y;
                                    yield break;
                                }
                            }
                        }
                    }
                }
            }
            targetX = 0f;
            takeOutX = 0f;
            yield break;
        }
        
    }

    public void Conservative(int rockCurrent)
    {

    }

    public void Aggressive(int rockCurrent)
    {
        GameObject rock = gm.rockList[rockCurrent].rock;
        Rock_Info rockInfo = gm.rockList[rockCurrent].rockInfo;

        switch (rockCurrent)
        { 
            case 0:
                StartCoroutine(Shot("Tight Centre Guard"));
                break;
            case 1:
                StartCoroutine(Shot("Right Tight Corner Guard"));
                break;
            case 2:

                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(Shot("Top Four Foot"));
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    StartCoroutine(Shot("Centre Guard"));
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    StartCoroutine(Shot("Left Four Foot"));
                }
                else StartCoroutine(Shot("Button"));
                break;

            case 3:

                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(Shot("Left Twelve Foot"));
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    StartCoroutine(Shot("Right Tight Corner Guard"));
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    StartCoroutine(Shot("Take Out"));
                }
                else StartCoroutine(Shot("Button"));
                break;

            case 4:

                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(Shot("Back Four Foot"));
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    StartCoroutine(Shot("Tight Centre Guard"));
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    StartCoroutine(Shot("Take Out"));
                }
                else StartCoroutine(Shot("Button"));
                break;

            case 5:

                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(Shot("Take Out"));
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    StartCoroutine(Shot("Left Tight Corner Guard"));
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    StartCoroutine(Shot("Take Out"));
                }
                else StartCoroutine(Shot("Back Four Foot"));
                break;

            case 6:

                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(Shot("Take Out"));
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    StartCoroutine(Shot("Tight Centre Guard"));
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    StartCoroutine(Shot("Take Out"));
                }
                else StartCoroutine(Shot("Button"));
                break;

            case 7:

                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(Shot("Take Out"));
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    StartCoroutine(Shot("Centre Guard"));
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    StartCoroutine(Shot("Take Out"));
                }
                else StartCoroutine(Shot("Button"));
                break;

            case 8:
                StartCoroutine(Shot("Take Out"));
                break;
            case 9:
                StartCoroutine(Shot("Take Out"));
                break;
            case 10:
                StartCoroutine(Shot("Take Out"));
                break;
            case 11:
                StartCoroutine(Shot("Take Out"));
                break;
            case 12:
                rm.inturn = true;

                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(Shot("Right Four Foot"));
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    StartCoroutine(Shot("Right Tight Corner Guard"));
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    StartCoroutine(Shot("Take Out"));
                }
                else StartCoroutine(Shot("Button"));
                break;

            case 13:

                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(Shot("Top Four Foot"));
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    StartCoroutine(Shot("Tight Centre Guard"));
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    StartCoroutine(Shot("Take Out"));
                }
                else StartCoroutine(Shot("Button"));
                break;
            case 14:

                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(Shot("Button"));
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    StartCoroutine(Shot("Centre Guard"));
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    StartCoroutine(Shot("Take Out"));
                }
                else StartCoroutine(Shot("Button"));

                break;
            case 15:
                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(Shot("Button"));
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    StartCoroutine(Shot("Back Four Foot"));
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    StartCoroutine(Shot("Take Out"));
                }
                else StartCoroutine(Shot("Button"));
                break;
            default:
                break;
        }
    }

    IEnumerator Shot(string aiShotType)
    {
        Debug.Log("AI Shot " + aiShotType);

        rockFlick.isPressedAI = true;

        yield return new WaitForSeconds(0.5f);

        float shotX;
        float shotY;

        switch (aiShotType)
        {
            case "Centre Guard":
                 shotX = Random.Range(centreGuard.x + guardAccu.x, centreGuard.x - guardAccu.x);
                 shotY = Random.Range(centreGuard.y + guardAccu.y, centreGuard.y - guardAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Tight Centre Guard":
                shotX = Random.Range(tightCentreGuard.x + guardAccu.x, tightCentreGuard.x - guardAccu.x);
                shotY = Random.Range(tightCentreGuard.y + guardAccu.y, tightCentreGuard.y - guardAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "High Centre Guard":
                shotX = Random.Range(highCentreGuard.x + guardAccu.x, highCentreGuard.x - guardAccu.x);
                shotY = Random.Range(highCentreGuard.y + guardAccu.y, highCentreGuard.y - guardAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Left Corner Guard":
                shotX = Random.Range(leftCornerGuard.x + guardAccu.x, leftCornerGuard.x - guardAccu.x);
                shotY = Random.Range(leftCornerGuard.y + guardAccu.y, leftCornerGuard.y - guardAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Left Tight Corner Guard":
                shotX = Random.Range(leftTightCornerGuard.x + guardAccu.x, leftTightCornerGuard.x - guardAccu.x);
                shotY = Random.Range(leftTightCornerGuard.y + guardAccu.y, leftTightCornerGuard.y - guardAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Left High Corner Guard":
                shotX = Random.Range(leftHighCornerGuard.x + guardAccu.x, leftHighCornerGuard.x - guardAccu.x);
                shotY = Random.Range(leftHighCornerGuard.y + guardAccu.y, leftHighCornerGuard.y - guardAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Right Corner Guard":
                 shotX = Random.Range(rightCornerGuard.x + guardAccu.x, rightCornerGuard.x - guardAccu.x);
                 shotY = Random.Range(rightCornerGuard.y + guardAccu.y, rightCornerGuard.y - guardAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Right Tight Corner Guard":
                shotX = Random.Range(rightTightCornerGuard.x + guardAccu.x, rightTightCornerGuard.x - guardAccu.x);
                shotY = Random.Range(rightTightCornerGuard.y + guardAccu.y, rightTightCornerGuard.y - guardAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Right High Corner Guard":
                shotX = Random.Range(rightHighCornerGuard.x + guardAccu.x, rightHighCornerGuard.x - guardAccu.x);
                shotY = Random.Range(rightHighCornerGuard.y + guardAccu.y, rightHighCornerGuard.y - guardAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Top Twelve Foot":
                 shotX = Random.Range(topTwelveFoot.x + drawAccu.x, topTwelveFoot.x - drawAccu.x);
                 shotY = Random.Range(topTwelveFoot.y + drawAccu.y, topTwelveFoot.y - drawAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Left Twelve Foot":
                shotX = Random.Range(leftTwelveFoot.x + drawAccu.x, leftTwelveFoot.x - drawAccu.x);
                shotY = Random.Range(leftTwelveFoot.y + drawAccu.y, leftTwelveFoot.y - drawAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Back Twelve Foot":
                shotX = Random.Range(backTwelveFoot.x + drawAccu.x, backTwelveFoot.x - drawAccu.x);
                shotY = Random.Range(backTwelveFoot.y + drawAccu.y, backTwelveFoot.y - drawAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Right Twelve Foot":
                shotX = Random.Range(rightTwelveFoot.x + drawAccu.x, rightTwelveFoot.x - drawAccu.x);
                shotY = Random.Range(rightTwelveFoot.y + drawAccu.y, rightTwelveFoot.y - drawAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Button":
                 shotX = Random.Range(button.x + drawAccu.x, button.x - drawAccu.x);
                 shotY = Random.Range(button.y + drawAccu.y, button.y - drawAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Left Four Foot":
                shotX = Random.Range(leftFourFoot.x + drawAccu.x, leftFourFoot.x - drawAccu.x);
                shotY = Random.Range(leftFourFoot.y + drawAccu.y, leftFourFoot.y - drawAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Right Four Foot":
                shotX = Random.Range(rightFourFoot.x + drawAccu.x, rightFourFoot.x - drawAccu.x);
                shotY = Random.Range(rightFourFoot.y + drawAccu.y, rightFourFoot.y - drawAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Top Four Foot":
                shotX = Random.Range(topFourFoot.x + drawAccu.x, topFourFoot.x - drawAccu.x);
                shotY = Random.Range(topFourFoot.y + drawAccu.y, topFourFoot.y - drawAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Back Four Foot":
                shotX = Random.Range(backFourFoot.x + drawAccu.x, backFourFoot.x - drawAccu.x);
                shotY = Random.Range(backFourFoot.y + drawAccu.y, backFourFoot.y - drawAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Take Out":

                yield return StartCoroutine(TakeOutTarget(gm.rockCurrent));

                if (takeOutX != 0f)
                {
                    shotX = Random.Range(takeOutX + toAccu.x, takeOutX - toAccu.x);
                    shotY = Random.Range(takeOut.y + toAccu.y, takeOut.y - toAccu.y);
                }
                else
                {
                    shotX = Random.Range(button.x + drawAccu.x, button.x - drawAccu.x);
                    shotY = Random.Range(button.y + drawAccu.y, button.y - drawAccu.y);
                }

                rockRB.position = new Vector2(shotX, shotY);

                Debug.Log("Take Out Position is (" + rockRB.position.x + " ," + rockRB.position.y + ")");
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Raise":
                rockRB.position = raise;
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            default:
                break;
        }

    }
}
