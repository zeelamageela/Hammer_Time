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

    public Vector2 peel;
    public Vector2 takeOut;
    public Vector2 raise;
    public Vector2 tick;

    public string testing;
    public int testingRockNumber;

    public Vector2 guardAccu;
    public Vector2 drawAccu;
    public Vector2 toAccu;
    public Vector2 tickAccu;

    public bool aggressive;

    public Transform cenGuard;
    public Transform tCenGuard;
    public Transform hCenGuard;
    public Transform lCornGuard;
    public Transform rCornGuard;

    public float takeOutOffset;
    public float peelOffset;
    public float raiseOffset;
    public float tickOffset;

    bool inturn;
    float targetX;
    float targetY;
    public float takeOutX;
    float raiseY;

    public float osMult;
    GameObject closestRock;
    Rock_Info closestRockInfo;

    void Start()
    {
        ShotLength();
    }
    // OnEnable is called when the Game Object is enabled
    void OnEnable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        inturn = rm.inturn;

        if (Input.GetKeyDown(KeyCode.A))
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
            //StartCoroutine(TickShot(gm.rockCurrent));
            //StartCoroutine(Shot("Take Out"));
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            rockInfo = gm.rockList[gm.rockCurrent].rockInfo;
            rockFlick = gm.rockList[gm.rockCurrent].rock.GetComponent<Rock_Flick>();
            rockRB = gm.rockList[gm.rockCurrent].rock.GetComponent<Rigidbody2D>();

            if (gm.houseList.Count != 0)
            {
                closestRock = gm.houseList[0].rock;
                closestRockInfo = gm.houseList[0].rockInfo;
            }

            //StartCoroutine(Shot(testing));
            //StartCoroutine(TickShot(gm.rockCurrent));
            StartCoroutine(TakeOutTarget(gm.rockCurrent, testingRockNumber));
        }
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

            //StartCoroutine(Shot(testing));
            StartCoroutine(TakeOutAutoTarget(gm.rockCurrent));
            //StartCoroutine(Shot("Take Out"));
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            rockInfo = gm.rockList[gm.rockCurrent].rockInfo;
            rockFlick = gm.rockList[gm.rockCurrent].rock.GetComponent<Rock_Flick>();
            rockRB = gm.rockList[gm.rockCurrent].rock.GetComponent<Rigidbody2D>();

            if (gm.houseList.Count != 0)
            {
                closestRock = gm.houseList[0].rock;
                closestRockInfo = gm.houseList[0].rockInfo;
            }

            StartCoroutine(DrawFourFoot(gm.rockCurrent));
            //StartCoroutine(TickShot(gm.rockCurrent));
            //StartCoroutine(Shot("Take Out"));
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            rockInfo = gm.rockList[gm.rockCurrent].rockInfo;
            rockFlick = gm.rockList[gm.rockCurrent].rock.GetComponent<Rock_Flick>();
            rockRB = gm.rockList[gm.rockCurrent].rock.GetComponent<Rigidbody2D>();

            if (gm.houseList.Count != 0)
            {
                closestRock = gm.houseList[0].rock;
                closestRockInfo = gm.houseList[0].rockInfo;
            }

            StartCoroutine(DrawTwelveFoot(gm.rockCurrent));
            //StartCoroutine(TickShot(gm.rockCurrent));
            //StartCoroutine(Shot("Take Out"));
        }
    }

    public void ShotLength()
    {
        centreGuard.y = centreGuard.y + (osMult * 0.18f);
        tightCentreGuard.y = tightCentreGuard.y + (osMult * 0.18f);
        highCentreGuard.y = highCentreGuard.y + (osMult * 0.18f);
        leftHighCornerGuard.y = leftHighCornerGuard.y + (osMult * 0.18f);
        leftTightCornerGuard.y = leftTightCornerGuard.y + (osMult * 0.18f);
        leftCornerGuard.y = leftCornerGuard.y + (osMult * 0.18f);
        rightHighCornerGuard.y = rightHighCornerGuard.y + (osMult * 0.18f);
        rightTightCornerGuard.y = rightTightCornerGuard.y + (osMult * 0.18f);
        rightCornerGuard.y = rightCornerGuard.y + (osMult * 0.18f);

        topTwelveFoot.y = topTwelveFoot.y + (osMult * 0.18f);
        backTwelveFoot.y = backTwelveFoot.y + (osMult * 0.18f);
        leftTwelveFoot.y = leftTwelveFoot.y + (osMult * 0.18f);
        rightTwelveFoot.y = rightTwelveFoot.y + (osMult * 0.18f);

        topFourFoot.y = topFourFoot.y + (osMult * 0.18f);
        backFourFoot.y = backFourFoot.y + (osMult * 0.18f);
        leftFourFoot.y = leftFourFoot.y + (osMult * 0.18f);
        rightFourFoot.y = rightFourFoot.y + (osMult * 0.18f);
        button.y = button.y + (osMult * 0.18f);

        peel.y = peel.y + (osMult * 0.18f);
        takeOut.y = takeOut.y + (osMult * 0.18f);
        raise.y = raise.y + (osMult * 0.18f);
        tick.y = tick.y + (osMult * 0.18f);
    }

    public void OnShot(int rockCurrent)
    {
        rockInfo = gm.rockList[rockCurrent].rockInfo;
        rockFlick = gm.rockList[rockCurrent].rock.GetComponent<Rock_Flick>();
        rockRB = gm.rockList[rockCurrent].rock.GetComponent<Rigidbody2D>();

        //aggressive = true;

        if (gm.redScore > gm.yellowScore)
        {
            aggressive = true;
        }
        else if (gm.redScore < gm.yellowScore)
        {
            aggressive = false;
        }
        else
        {
            aggressive = true;
        }

        if (gm.houseList.Count != 0)
        {
            closestRock = gm.houseList[0].rock;
            closestRockInfo = gm.houseList[0].rockInfo;

            //if (gm.houseList.Count > 1)
            //{
            //    if (closestRockInfo.teamName == rockInfo.teamName)
            //    {
            //        if (gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
            //        {
            //            aggressive = false;
            //        }
            //        else
            //        {
            //            aggressive = true;
            //        }
            //    }
            //    else
            //    {
            //        aggressive = true;
            //    }
            //}
            //else
            //{
            //    aggressive = true;
            //}
        }


        if (aggressive)
        {
            Aggressive(rockCurrent);
        }
        else
            Conservative(rockCurrent);
    }

    IEnumerator GuardReading(int rockCurrent)
    {
        if (gm.gList.Count != 0)
        {
            foreach (Guard_List guard in gm.gList)
            {
                float posX;
                float posY;

                posX = guard.lastTransform.position.x;
                posY = guard.lastTransform.position.y;
                // center lane
                if (Mathf.Abs(posX) <= 0.4f)
                {
                    if (posY <= 3.5f)
                    {
                        cenGuard = guard.lastTransform;
                        Debug.Log("Centre Guard - " + guard.lastTransform.position.x + ", " + guard.lastTransform.position.y);
                    }
                    else
                    {
                        tCenGuard = guard.lastTransform;
                        Debug.Log("Tight Centre Guard - " + guard.lastTransform.position.x + ", " + guard.lastTransform.position.y);
                    }
                }
                // left channel 
                else if (posX < -0.4f && posX > -1.25f)
                {
                    lCornGuard = guard.lastTransform;
                    Debug.Log("Left Guard - " + guard.lastTransform.position.x + ", " + guard.lastTransform.position.y);
                }
                // right channel
                else if (posX > 0.4f && posX < 1.25f)
                {
                    rCornGuard = guard.lastTransform;
                    Debug.Log("Right Guard - " + guard.lastTransform.position.x + ", " + guard.lastTransform.position.y);
                }

                else
                {
                    cenGuard = null;
                    lCornGuard = null;
                    rCornGuard = null;
                    Debug.Log("No Guards");
                }
            }

        }
        else
        {
            cenGuard = null;
            lCornGuard = null;
            rCornGuard = null;

            Debug.Log("No Guards");
        }

        yield return new WaitForEndOfFrame();
    }

    IEnumerator TakeOutAutoTarget(int rockCurrent)
    {
        yield return StartCoroutine(GuardReading(rockCurrent));
        yield return new WaitForEndOfFrame();
        //if the house has rocks in it
        if (gm.houseList.Count != 0)
        {
            Debug.Log("houseList is not 0");
            //if the closest rock is not my team
            if (closestRockInfo.teamName != rockInfo.teamName)
            {
                //if it's in the four foot
                if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.6f)
                {
                    //if there's no centre guard
                    if (!cenGuard)
                    {
                        targetX = closestRock.transform.position.x;
                        if (targetX > 0f)
                        {
                            rm.inturn = false;
                            takeOutX = (-0.205f * ((targetX + 1.35f) / 2.7f)) + 0.087f;
                        }
                        else
                        {
                            rm.inturn = true;
                            takeOutX = (0.15f * ((targetX - 1.35f) / -2.7f)) - 0.05f;
                        }
                        StartCoroutine(Shot("Take Out"));
                        Debug.Log(closestRockInfo.teamName + " " + closestRockInfo.rockNumber);
                        yield break;
                    }
                    else
                    {
                        //if the centre guard is mine
                        if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                        {
                            //let's run it back
                            targetX = cenGuard.position.x;
                            if (targetX > 0f)
                            {
                                rm.inturn = false;
                                takeOutX = (-0.142f * ((targetX) / 1.65f)) - 0.011f;
                            }
                            else
                            {
                                rm.inturn = true;
                                takeOutX = (0.13f * (targetX / -1.65f)) + 0.015f;
                            }
                            StartCoroutine(Shot("Take Out"));
                            Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                            yield break;
                        }
                        //if the centre guard is not mine
                        else if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                        {
                            //let's take it out
                            targetX = cenGuard.position.x;
                            if (targetX > 0f)
                            {
                                rm.inturn = false;
                                takeOutX = (-0.142f * ((targetX) / 1.65f)) - 0.011f;
                            }
                            else
                            {
                                rm.inturn = true;
                                takeOutX = (0.13f * (targetX / -1.65f)) + 0.015f;
                            }
                            StartCoroutine(Shot("Peel"));
                            Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                            yield break;
                        }
                    }
                }
                //if it's not in the four foot
                else
                {
                    //if there's a centre guard and the closest rock is in the middle
                    if (cenGuard & Mathf.Abs(closestRock.transform.position.x) <= 0.5f)
                    {
                        targetX = cenGuard.position.x; 
                        if (targetX > 0f)
                        {
                            rm.inturn = false;
                            takeOutX = (-0.142f * ((targetX) / 1.65f)) - 0.011f;
                        }
                        else
                        {
                            rm.inturn = true;
                            takeOutX = (0.13f * (targetX / -1.65f)) + 0.015f;
                        }
                        yield return StartCoroutine(Shot("Raise"));
                        Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                        yield break;
                    }
                    //if the closest rock is to the right and there's a right guard
                    else if (rCornGuard & closestRock.transform.position.x > 0f)
                    {
                        targetX = rCornGuard.position.x;
                        if(targetX > 0f)
                        {
                            rm.inturn = false;
                            takeOutX = (-0.142f * ((targetX) / 1.65f)) - 0.011f;
                        }
                        else
                        {
                            rm.inturn = true;
                            takeOutX = (0.13f * (targetX / -1.65f)) + 0.015f;
                        }
                        yield return StartCoroutine(Shot("Peel"));
                        Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                        yield break;
                    }
                    //if there's a left guard and the closest rock is to the left
                    else if (lCornGuard & closestRock.transform.position.x < 0f)
                    {
                        targetX = lCornGuard.position.x;
                        if (targetX > 0f)
                        {
                            rm.inturn = false;
                            takeOutX = (-0.142f * ((targetX) / 1.65f)) - 0.011f;
                        }
                        else
                        {
                            rm.inturn = true;
                            takeOutX = (0.13f * (targetX / -1.65f)) + 0.015f;
                        }
                        yield return StartCoroutine(Shot("Peel"));
                        Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                        yield break;
                    }
                    else
                    {
                        Debug.Log("House List Count is " + gm.houseList.Count);
                        targetX = closestRock.transform.position.x;
                        if (targetX > 0f)
                        {
                            rm.inturn = false;
                            takeOutX = (-0.205f * ((targetX + 1.35f) / 2.7f)) + 0.087f;
                        }
                        else
                        {
                            rm.inturn = true;
                            takeOutX = (0.15f * ((targetX - 1.35f) / -2.7f)) - 0.05f;
                        }
                        StartCoroutine(Shot("Take Out"));
                        Debug.Log("Target is " + closestRockInfo.teamName + " " + closestRockInfo.rockNumber);
                        yield break;
                    }
                }
            }
            //if the closest rock is my team
            else if (closestRockInfo.teamName == rockInfo.teamName)
            {
                //if there's more than one rock in the house
                if (gm.houseList.Count >= 2)
                {
                    //if the second rock is mine
                    if (gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                    {
                        //if the second rock is not guarded
                        if (Mathf.Abs(gm.houseList[1].rock.transform.position.x) <= 0.5f & !cenGuard)
                        {
                            yield return StartCoroutine(Shot("Centre Guard"));
                            Debug.Log("Centre Guard");
                            yield break;
                        }
                        else if (gm.houseList[1].rock.transform.position.x < 0f & !lCornGuard)
                        {
                            yield return StartCoroutine(Shot("Left Corner Guard"));
                            Debug.Log("Left Corner Guard");
                            yield break;
                        }
                        else if (gm.houseList[1].rock.transform.position.x > 0f & !rCornGuard)
                        {
                            yield return StartCoroutine(Shot("Right Corner Guard"));
                            Debug.Log("Right Corner Guard");
                            yield break;
                        }
                        else
                        {
                            if (gm.houseList.Count >= 3 && gm.houseList[2].rockInfo.teamName != rockInfo.teamName)
                            {

                            }
                            yield return StartCoroutine(DrawFourFoot(gm.rockCurrent));
                            Debug.Log("Drawing Four Foot");
                            yield break;
                        }
                    }
                    //if the second rock is not mine
                    else
                    {
                        //if the second rock is guarded
                        if (Mathf.Abs(gm.houseList[1].rock.transform.position.x) <= 0.5f && cenGuard)
                        {
                            targetX = cenGuard.position.x;
                            takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                            yield return StartCoroutine(Shot("Peel"));
                            Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                            yield break;
                        }
                        else if (gm.houseList[1].rock.transform.position.x < 0f && lCornGuard)
                        {
                            targetX = lCornGuard.position.x;
                            takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                            yield return StartCoroutine(Shot("Peel"));
                            Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                            yield break;
                        }
                        else if (gm.houseList[1].rock.transform.position.x > 0f && rCornGuard)
                        {
                            targetX = rCornGuard.position.x;
                            takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                            yield return StartCoroutine(Shot("Peel"));
                            Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                            yield break;
                        }
                        //if the second rock is not guarded
                        else
                        {
                            targetX = gm.houseList[1].rock.transform.position.x;
                            takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                            yield return StartCoroutine(Shot("Take Out"));
                            Debug.Log(gm.houseList[1].rockInfo.teamName + " " + gm.houseList[1].rockInfo.rockNumber);
                            yield break;
                        }
                    }
                }
                //if there's not more that one rock in the house
                else
                {
                    //if the rock is not guarded
                    if (Mathf.Abs(closestRock.transform.position.x) <= 0.5f & !cenGuard)
                    {
                        yield return StartCoroutine(Shot("Centre Guard"));
                        Debug.Log("Centre Guard");
                        yield break;
                    }
                    else if (closestRock.transform.position.x < 0f & !lCornGuard)
                    {
                        yield return StartCoroutine(Shot("Left Corner Guard"));
                        Debug.Log("Left Corner Guard");
                        yield break;
                    }
                    else if (closestRock.transform.position.x > 0f & !rCornGuard)
                    {
                        yield return StartCoroutine(Shot("Right Corner Guard"));
                        Debug.Log("Right Corner Guard");
                        yield break;
                    }
                    else
                    {
                        yield return StartCoroutine(DrawFourFoot(gm.rockCurrent));
                        Debug.Log("Drawing Four Foot");
                        yield break;
                    }
                }

            }
        }
        //if there's guards
        else if (gm.gList.Count != 0)
        {
            //centre, left and right guards
            if (cenGuard && rCornGuard && lCornGuard)
            {
                //centre guard is mine
                if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Raise"));
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //right corner guard is not mine
                else if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                    yield return StartCoroutine(Shot("Raise"));
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //left corner guard is not mine
                else if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
            }
            //right guard only
            else if (rCornGuard & !cenGuard & !lCornGuard)
            {
                //guard is mine
                if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                    yield return StartCoroutine(Shot("Raise"));
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //guard is not mine
                else
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
            }
            //left guard only
            else if (!cenGuard & !rCornGuard & lCornGuard)
            {
                //guard is mine
                if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Raise"));
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //guard is not mine
                else
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
            }
            //right and left guards
            else if (!cenGuard & rCornGuard & lCornGuard)
            {
                //left guard is not mine
                if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //right guard is not mine
                else if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //left guard is mine
                else if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Raise"));
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //right guard is mine
                else if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Raise"));
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else
                {
                    targetX = 0f;
                    takeOutX = 0f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log("No Targets available - Button");
                    yield break;
                }
            }
            //centre and right guards
            else if (cenGuard & rCornGuard & !lCornGuard)
            {
                //centre guard is not mine
                if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //right guard is not mine
                else if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //centre guard is mine
                else if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Raise"));
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //right guard is mine
                else if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Raise"));
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else
                {
                    targetX = 0f;
                    takeOutX = 0f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log("No Targets available - Button");
                    yield break;
                }
            }
            //centre and left guards
            else if (cenGuard & !rCornGuard & lCornGuard)
            {
                //left guard is not mine
                if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //centre guard is not mine
                else if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //left guard is mine
                else if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //centre guard is mine
                else if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else
                {
                    yield return StartCoroutine(Shot("Button"));
                    Debug.Log("No Targets available - Button");
                    yield break;
                }
            }
            //centre guard only
            else if (cenGuard & !rCornGuard & !lCornGuard)
            {
                //if it's mine
                if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Raise"));
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //if it's theirs
                else
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
            }
            else
            {
                targetX = 0f;
                takeOutX = 0f;
                yield return StartCoroutine(DrawFourFoot(gm.rockCurrent));
                Debug.Log("No Targets available - Drawing Four Foot");
                yield break;
            }
        }
        else
        {
            targetX = 0f;
            takeOutX = 0f;
            yield return StartCoroutine(Shot("Peel"));
            Debug.Log("No Targets available - Button");
            yield break;
        }

    }

    IEnumerator TakeOutTarget(int rockCurrent, int rockTarget)
    {
        yield return StartCoroutine(GuardReading(rockCurrent));

        targetX = gm.rockList[rockTarget].rock.transform.position.x;
        targetY = gm.rockList[rockTarget].rock.transform.position.y;


        //rm.inturn = false;
        //takeOutX = (-0.205f * ((targetX + 1.35f) / 2.7f)) + 0.087f;

        rm.inturn = true;
        takeOutX = (-0.19f * ((targetX + 1.35f) / 2.7f)) + 0.11f;

        if (targetX > 0f)
        {
            rm.inturn = false;
            takeOutX = (-0.205f * ((targetX + 1.35f) / 2.7f)) + 0.087f;
        }
        else
        {
            rm.inturn = true;
            takeOutX = (-0.19f * ((targetX + 1.35f) / 2.7f)) + 0.11f;
        }

        StartCoroutine(Shot("Take Out"));
        Debug.Log(gm.rockList[rockTarget].rockInfo.teamName + " " + gm.rockList[rockTarget].rockInfo.rockNumber);
        yield break;
    }

    IEnumerator TapTarget(int rockCurrent, int rockTarget)
    {
        yield return StartCoroutine(GuardReading(rockCurrent));

        targetX = gm.rockList[rockTarget].rock.transform.position.x;
        targetY = gm.rockList[rockTarget].rock.transform.position.y;


        //rm.inturn = false;
        //takeOutX = (-0.205f * ((targetX + 1.35f) / 2.7f)) + 0.087f;

        rm.inturn = true;
        takeOutX = (-0.19f * ((targetX + 1.35f) / 2.7f)) + 0.11f;

        if (targetX > 0f)
        {
            rm.inturn = false;
            takeOutX = (-0.205f * ((targetX + 1.35f) / 2.7f)) + 0.087f;
        }
        else
        {
            rm.inturn = true;
            takeOutX = (-0.19f * ((targetX + 1.35f) / 2.7f)) + 0.11f;
        }

        StartCoroutine(Shot("Raise"));
        Debug.Log(gm.rockList[rockTarget].rockInfo.teamName + " " + gm.rockList[rockTarget].rockInfo.rockNumber);
        yield break;
    }

    IEnumerator TickShot(int rockCurrent)
    {
        yield return StartCoroutine(GuardReading(rockCurrent));

        if (gm.gList.Count != 0)
        {
            if (cenGuard)
            {
                if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                    StartCoroutine(Shot("Tick"));
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                    StartCoroutine(Shot("Tick"));
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
            }
            else
            {
                StartCoroutine(Shot("Centre Guard"));
                Debug.Log("No Target - Centre Guard");
                yield break;
            }
        }
        
    }

    IEnumerator DrawTwelveFoot(int rockCurrent)
    {
        yield return StartCoroutine(GuardReading(rockCurrent));

        //if there's at least one guard
        if (gm.gList.Count != 0)
        {
            //only a centre guard
            if (cenGuard && !lCornGuard && !rCornGuard)
            {
                //centre guard to the right
                if (cenGuard.position.x > 0f)
                {
                    rm.inturn = true;
                    StartCoroutine(Shot("Top Twelve Foot"));
                    yield break;
                }
                //centre guard to the left
                else if (cenGuard.position.x < 0f)
                {
                    rm.inturn = false;
                    StartCoroutine(Shot("Top Twelve Foot"));
                    yield break;
                }
            }

            //centre guard and a right guard and a left guard
            else if (cenGuard && rCornGuard && lCornGuard)
            {
                //high centre guard
                if (cenGuard.position.y < 2.0f)
                {
                    //centre guard to the right
                    if (cenGuard.position.x > 0f)
                    {
                        rm.inturn = true;
                        StartCoroutine(Shot("Top Twelve Foot"));
                        yield break;
                    }
                    //centre guard to the left
                    else if (cenGuard.position.x < 0f)
                    {
                        rm.inturn = false;
                        StartCoroutine(Shot("Top Twelve Foot"));
                        yield break;
                    }
                }
                //centre guard is medium height
                else if (cenGuard.position.y < 3.0f)
                {
                    //corner guards are high
                    if (rCornGuard.position.y < 2.0f && lCornGuard.position.y < 2.0f)
                    {
                        //centre guard to the right
                        if (cenGuard.position.x > 0f)
                        {
                            rm.inturn = true;
                            StartCoroutine(Shot("Top Twelve Foot"));
                            yield break;
                        }
                        //centre guard to the left
                        else if (cenGuard.position.x < 0f)
                        {
                            rm.inturn = false;
                            StartCoroutine(Shot("Top Twelve Foot"));
                            yield break;
                        }
                    }
                    //left corner guard is high
                    else if (lCornGuard.position.y < 2.0f)
                    {
                        rm.inturn = false;
                        StartCoroutine(Shot("Top Twelve Foot"));
                        yield break;
                    }
                    //right corner guard is high
                    else if (rCornGuard.position.y < 2.0f)
                    {
                        rm.inturn = true;
                        StartCoroutine(Shot("Top Twelve Foot"));
                        yield break;
                    }
                }
                //low centre guard
                else if (cenGuard.position.y < 4.8f)
                {
                    //both corner guards are higher
                    if (rCornGuard.position.y < cenGuard.position.y && lCornGuard.position.y < cenGuard.position.y)
                    {
                        //centre guard to the right
                        if (cenGuard.position.x > 0f)
                        {
                            rm.inturn = true;
                            StartCoroutine(Shot("Top Twelve Foot"));
                            yield break;
                        }
                        //centre guard to the left
                        else if (cenGuard.position.x < 0f)
                        {
                            rm.inturn = false;
                            StartCoroutine(Shot("Top Twelve Foot"));
                            yield break;
                        }
                    }
                    //left corner guard is higher
                    else if (lCornGuard.position.y < cenGuard.position.y)
                    {
                        rm.inturn = false;
                        StartCoroutine(Shot("Top Twelve Foot"));
                        yield break;
                    }
                    //right corner guard is higher
                    else if (rCornGuard.position.y < cenGuard.position.y)
                    {
                        rm.inturn = true;
                        StartCoroutine(Shot("Top Twelve Foot"));
                        yield break;
                    }
                }
                //any other situation
                else
                {
                    //centre guard to the right
                    if (cenGuard.position.x > 0f)
                    {
                        rm.inturn = true;
                        StartCoroutine(Shot("Top Twelve Foot"));
                        yield break;
                    }
                    //centre guard to the left
                    else if (cenGuard.position.x < 0f)
                    {
                        rm.inturn = false;
                        StartCoroutine(Shot("Top Twelve Foot"));
                        yield break;
                    }
                }
            }

            //centre guard and a left guard
            else if (cenGuard && lCornGuard && !rCornGuard)
            {
                if (cenGuard.position.x > 0f)
                {
                    rm.inturn = false;
                    StartCoroutine(Shot("Top Twelve Foot"));
                    yield break;
                }
                else if (cenGuard.position.x < 0f)
                {
                    rm.inturn = false;
                    StartCoroutine(Shot("Left Twelve Foot"));
                    yield break;
                }
            }

            //centre guard and a right guard
            else if (cenGuard && rCornGuard && !lCornGuard)
            {
                if (cenGuard.position.x > 0f)
                {
                    rm.inturn = true;
                    StartCoroutine(Shot("Right Twelve Foot"));
                    yield break;
                }
                else if (cenGuard.position.x < 0f)
                {
                    rm.inturn = true;
                    StartCoroutine(Shot("Top Twelve Foot"));
                    yield break;
                }
            }

            //right and a left guard
            else if (rCornGuard && lCornGuard && !cenGuard)
            {
                if (rCornGuard.position.y < lCornGuard.position.y)
                {
                    rm.inturn = true;
                    StartCoroutine(Shot("Right Twelve Foot"));
                    yield break;
                }
                else if (lCornGuard.position.y < rCornGuard.position.y)
                {
                    rm.inturn = false;
                    StartCoroutine(Shot("Left Twelve Foot"));
                    yield break;
                }
            }

            //right corner guard
            else if (rCornGuard && !lCornGuard && !cenGuard)
            {
                rm.inturn = true;
                StartCoroutine(Shot("Right Twelve Foot"));
                yield break;
            }

            //left corner guard
            else if (lCornGuard && !rCornGuard && !cenGuard)
            {
                rm.inturn = false;
                StartCoroutine(Shot("Left Twelve Foot"));
                yield break;
            }
        }

        //if there's no guards
        else
        {
            if (Random.value > 0.5f)
            {
                rm.inturn = true;
            }
            else rm.inturn = false;

            StartCoroutine(Shot("Top Twelve Foot"));
            yield break;
        }
        
    }

    IEnumerator DrawFourFoot(int rockCurrent)
    {
        //read where the guards are
        yield return StartCoroutine(GuardReading(rockCurrent));

        //if there are guards
        if (gm.gList.Count != 0)
        {
            //only a centre guard
            if (cenGuard && !lCornGuard && !rCornGuard)
            {
                //centre guard to the right
                if (cenGuard.position.x > 0f)
                {
                    rm.inturn = true;
                    StartCoroutine(Shot("Top Four Foot"));
                    yield break;
                }
                //centre guard to the left
                else
                {
                    rm.inturn = false;
                    StartCoroutine(Shot("Top Four Foot"));
                    yield break;
                }
            }

            //centre guard and a right guard and a left guard
            else if (cenGuard && rCornGuard && lCornGuard)
            {
                //high centre guard
                if (cenGuard.position.y < 2.0f)
                {
                    //centre guard to the right
                    if (cenGuard.position.x > 0f)
                    {
                        rm.inturn = true;
                        StartCoroutine(Shot("Top Four Foot"));
                        yield break;
                    }
                    //centre guard to the left
                    else
                    {
                        rm.inturn = false;
                        StartCoroutine(Shot("Top Four Foot"));
                        yield break;
                    }
                }
                //centre guard is medium height
                else if (cenGuard.position.y < 3.0f)
                {
                    //corner guards are high
                    if (rCornGuard.position.y < 2.0f && lCornGuard.position.y < 2.0f)
                    {
                        //centre guard to the right
                        if (cenGuard.position.x > 0f)
                        {
                            rm.inturn = true;
                            StartCoroutine(Shot("Top Four Foot"));
                            yield break;
                        }
                        //centre guard to the left
                        else
                        {
                            rm.inturn = false;
                            StartCoroutine(Shot("Top Four Foot"));
                            yield break;
                        }
                    }
                    //left corner guard is high
                    else if (lCornGuard.position.y < 2.0f)
                    {
                        rm.inturn = false;
                        StartCoroutine(Shot("Top Four Foot"));
                        yield break;
                    }
                    //right corner guard is high
                    else
                    {
                        rm.inturn = true;
                        StartCoroutine(Shot("Top Four Foot"));
                        yield break;
                    }
                }
                //low centre guard
                else if (cenGuard.position.y < 4.8f)
                {
                    //both corner guards are higher
                    if (rCornGuard.position.y < cenGuard.position.y && lCornGuard.position.y < cenGuard.position.y)
                    {
                        //centre guard to the right
                        if (cenGuard.position.x > 0f)
                        {
                            rm.inturn = true;
                            StartCoroutine(Shot("Top Four Foot"));
                            yield break;
                        }
                        //centre guard to the left
                        else
                        {
                            rm.inturn = false;
                            StartCoroutine(Shot("Top Four Foot"));
                            yield break;
                        }
                    }
                    //left corner guard is higher
                    else if (lCornGuard.position.y < cenGuard.position.y)
                    {
                        rm.inturn = false;
                        StartCoroutine(Shot("Top Four Foot"));
                        yield break;
                    }
                    //right corner guard is higher
                    else if (rCornGuard.position.y < cenGuard.position.y)
                    {
                        rm.inturn = true;
                        StartCoroutine(Shot("Top Four Foot"));
                        yield break;
                    }
                    else
                    {
                        rm.inturn = false;
                        StartCoroutine(Shot("Top Four Foot"));
                        yield break;
                    }
                }
                //any other situation
                else
                {
                    //centre guard to the right
                    if (cenGuard.position.x > 0f)
                    {
                        rm.inturn = true;
                        StartCoroutine(Shot("Top Four Foot"));
                        yield break;
                    }
                    //centre guard to the left
                    else
                    {
                        rm.inturn = false;
                        StartCoroutine(Shot("Top Four Foot"));
                        yield break;
                    }
                }
            }

            //centre guard and a left guard
            else if (cenGuard && lCornGuard && !rCornGuard)
            {
                if (cenGuard.position.x > 0f)
                {
                    rm.inturn = false;
                    StartCoroutine(Shot("Top Four Foot"));
                    yield break;
                }
                else
                {
                    rm.inturn = false;
                    StartCoroutine(Shot("Left Four Foot"));
                    yield break;
                }
            }

            //centre guard and a right guard
            else if (cenGuard && rCornGuard && !lCornGuard)
            {
                if (cenGuard.position.x > 0f)
                {
                    rm.inturn = true;
                    StartCoroutine(Shot("Right Four Foot"));
                    yield break;
                }
                else
                {
                    rm.inturn = true;
                    StartCoroutine(Shot("Top Four Foot"));
                    yield break;
                }
            }

            //right and a left guard
            else if (rCornGuard && lCornGuard && !cenGuard)
            {
                if (rCornGuard.position.y < lCornGuard.position.y)
                {
                    rm.inturn = true;
                    StartCoroutine(Shot("Right Four Foot"));
                    yield break;
                }
                else
                {
                    rm.inturn = false;
                    StartCoroutine(Shot("Left Four Foot"));
                    yield break;
                }
            }

            //right corner guard
            else if (rCornGuard && !lCornGuard && !cenGuard)
            {
                rm.inturn = true;
                StartCoroutine(Shot("Right Four Foot"));
                yield break;
            }

            //left corner guard
            else
            {
                rm.inturn = false;
                StartCoroutine(Shot("Left Four Foot"));
                yield break;
            }
        }

        //if there's no guards
        else
        {
            if (Random.value > 0.5f)
            {
                rm.inturn = true;
            }
            else rm.inturn = false;

            if (rockCurrent < 4)
            {
                StartCoroutine(Shot("Top Four Foot"));
            }
            else
            {
                StartCoroutine(Shot("Button"));
            }
            yield break;
        }
    }

    public void Conservative(int rockCurrent)
    {

        {
            GameObject rock = gm.rockList[rockCurrent].rock;
            Rock_Info rockInfo = gm.rockList[rockCurrent].rockInfo;

            switch (rockCurrent)
            {
                case 0:

                    if (gm.redScore > gm.yellowScore)
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                    else
                    {
                        StartCoroutine(DrawTwelveFoot(rockCurrent));
                    }
                    break;

                case 1:

                    StartCoroutine(TakeOutAutoTarget(rockCurrent));
                    break;

                case 2:

                    StartCoroutine(TakeOutAutoTarget(rockCurrent));
                    break;

                case 3:

                    StartCoroutine(TakeOutAutoTarget(rockCurrent));
                    break;

                case 4:

                    StartCoroutine(TakeOutAutoTarget(rockCurrent));
                    break;

                case 5:

                    StartCoroutine(TakeOutAutoTarget(rockCurrent));
                    break;

                case 6:

                    StartCoroutine(TakeOutAutoTarget(rockCurrent));
                    break;

                case 7:

                    StartCoroutine(TakeOutAutoTarget(rockCurrent));
                    break;

                case 8:

                    StartCoroutine(TakeOutAutoTarget(rockCurrent));
                    break;

                case 9:

                    StartCoroutine(TakeOutAutoTarget(rockCurrent));
                    break;

                case 10:

                    StartCoroutine(TakeOutAutoTarget(rockCurrent));
                    break;

                case 11:

                    StartCoroutine(TakeOutAutoTarget(rockCurrent));
                    break;

                case 12:
                    //rm.inturn = true;

                    StartCoroutine(TakeOutAutoTarget(rockCurrent));
                    break;

                case 13:

                    StartCoroutine(TakeOutAutoTarget(rockCurrent));
                    break;

                case 14:

                    StartCoroutine(TakeOutAutoTarget(rockCurrent));
                    break;

                case 15:

                    StartCoroutine(TakeOutAutoTarget(rockCurrent));
                    break;

                default:
                    break;
            }
        }
    }

    public void Aggressive(int rockCurrent)
    {
        //Aggressive is to steal at all costs
        GameObject rock = gm.rockList[rockCurrent].rock;
        Rock_Info rockInfo = gm.rockList[rockCurrent].rockInfo;
        string phase;

        //early phase is shots 1-3 in an 8 rock game
        if (gm.rockTotal - rockCurrent >= 11)
        {
            if (gm.redHammer)
            {
                phase = "early no hammer";
            }
            else
            {
                phase = "early hammer";
            }
        }
        //middle phase is shots 4 and 5 in an 8 rock game
        else if (gm.rockTotal - rockCurrent <= 10 && gm.rockTotal - rockCurrent >= 7)
        {
            if (gm.redHammer)
            {
                phase = "middle no hammer";
            }
            else
            {
                phase = "middle hammer";
            }
        }
        //late phase is shots 6-8 in an 8 rock game
        else if (gm.rockTotal - rockCurrent <= 6)
        {
            if (gm.redHammer)
            {
                phase = "late no hammer";
            }
            else
            {
                phase = "late hammer";
            }
        }
        else
        {
            if (gm.redHammer)
            {
                phase = "late no hammer";
                Debug.Log("Default Phase - late no hammer");
            }
            else
            {
                phase = "late hammer";
                Debug.Log("Default Phase - late hammer");
            }
        }

        Debug.Log("Phase is " + phase);

        StartCoroutine(GuardReading(rockCurrent));

        switch (phase)
        { 
            case "early no hammer":
                
                //no one is in the house
                if (gm.houseList.Count == 0)
                {
                    //tight centre and no centre guard
                    if (tCenGuard && !cenGuard)
                    {
                        StartCoroutine(Shot("Centre Guard"));
                    }
                    //centre and no tight centre guard
                    else if (cenGuard && !tCenGuard)
                    {
                        StartCoroutine(Shot("Tight Centre Guard"));
                    }
                    //centre and tight centre guards
                    else if (cenGuard & tCenGuard)
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                    else
                    {
                        //randomly choose
                        if (Random.value > 0.5f)
                        {
                            StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                        else StartCoroutine(Shot("Tight Centre Guard"));
                    }
                }
                //closest rock is mine
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    //tight centre and no centre guard
                    if (tCenGuard && !cenGuard)
                    {
                        if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                        {
                            StartCoroutine(Shot("Tight Centre Guard"));
                        }
                        else
                        {
                            StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                    }
                    //centre and no tight centre guard
                    else if (cenGuard && !tCenGuard)
                    {
                        if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                        {
                            StartCoroutine(Shot("High Centre Guard"));
                        }
                        else
                        {
                            StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                    }
                    //centre and tight centre guards
                    else if (cenGuard & tCenGuard)
                    {
                        if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                        {
                            StartCoroutine(DrawTwelveFoot(rockCurrent));
                        }
                        else
                        {
                            StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                    }
                    //any other guard combo
                    else
                    {
                        //randomly choose
                        if (Random.value > 0.5f)
                        {
                            StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                        else StartCoroutine(Shot("Tight Centre Guard"));
                    }
                }
                //closest rock is not mine
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    //if it's in the four foot
                    if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                    {
                        //if there's guards
                        if (gm.gList.Count != 0)
                        {
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f)
                            {
                                StartCoroutine(TakeOutAutoTarget(rockCurrent));
                            }
                            else
                                StartCoroutine(DrawFourFoot(rockCurrent));
                            StartCoroutine(TakeOutAutoTarget(rockCurrent));
                        }
                    }
                    else if (gm.houseList.Count >= 2)
                    {
                        if (gm.houseList[1].rockInfo.teamName == closestRockInfo.teamName)
                        {
                            StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                        else
                        {
                            StartCoroutine(TakeOutAutoTarget(rockCurrent));
                        }
                    }
                    else
                    {
                        StartCoroutine(DrawTwelveFoot(rockCurrent));
                    }
                }
                break;

            case "early hammer":
                //if there's guards
                if (gm.gList.Count != 0)
                {
                    //left corner guard
                    if (lCornGuard)
                    {
                        StartCoroutine(Shot("Right Corner Guard"));
                    }
                    //right corner guard
                    else if (rCornGuard)
                    {
                        StartCoroutine(Shot("Left Corner Guard"));
                    }
                    //left and right corner guard
                    else if (rCornGuard & lCornGuard)
                    {
                        StartCoroutine(DrawTwelveFoot(rockCurrent));
                    }
                    else
                    {
                        if (Random.value > 0.5f)
                            StartCoroutine(Shot("Left Corner Guard"));
                        else
                            StartCoroutine(Shot("Right Corner Guard"));
                    }
                }
                break;


            case "middle no hammer":
                //no one is in the house
                if (gm.houseList.Count == 0)
                {
                    //tight centre and no centre guard
                    if (tCenGuard && !cenGuard)
                    {
                        StartCoroutine(Shot("Centre Guard"));
                    }
                    //centre and no tight centre guard
                    else if (cenGuard && !tCenGuard)
                    {
                        StartCoroutine(Shot("Tight Centre Guard"));
                    }
                    //centre and tight centre guards
                    else if (cenGuard & tCenGuard)
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                    else
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                }
                //closest rock is mine
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    //tight centre and no centre guard
                    if (tCenGuard && !cenGuard)
                    {
                        if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                        {
                            StartCoroutine(Shot("Centre Guard"));
                        }
                        else
                        {
                            StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                    }
                    //centre and no tight centre guard
                    else if (cenGuard && !tCenGuard)
                    {
                        if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                        {
                            StartCoroutine(Shot("High Centre Guard"));
                        }
                        else
                        {
                            StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                    }
                    //centre and tight centre guards
                    else if (cenGuard & tCenGuard)
                    {
                        //if closest is in the four foot
                        if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                        {
                            //if there's more than one rock in the house
                            if (gm.houseList.Count > 1)
                            {
                                //if the second shot is mine
                                if (gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                                {
                                    if (gm.houseList[1].rock.transform.position.x < 0f)
                                    {
                                        StartCoroutine(Shot("Left Corner Guard"));
                                    }
                                    else StartCoroutine(Shot("Right Corner Guard"));
                                }
                                //if second shot is not mine
                                else
                                {
                                    StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[1].rockInfo.rockIndex));
                                }
                            }
                            //if there's only one rock in the house
                            else
                            StartCoroutine(DrawTwelveFoot(rockCurrent));
                        }
                        else
                        {
                            StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                    }
                    //any other guard combo
                    else
                    {
                        StartCoroutine(Shot("Centre Guard"));
                    }
                }
                //closest rock is not mine
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    //if it's in the four foot
                    if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                    {
                        //tight centre and no centre guard
                        if (tCenGuard && !cenGuard)
                        {
                            //if the tight centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - tCenGuard.position.x) >= 0.1f)
                            {
                                StartCoroutine(TakeOutTarget(rockCurrent, closestRockInfo.rockIndex));
                            }
                            else
                                StartCoroutine(TakeOutTarget(rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex));
                        }
                        //centre and no tight centre guard
                        else if (cenGuard && !tCenGuard)
                        {
                            //if the centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f)
                            {
                                StartCoroutine(TakeOutTarget(rockCurrent, closestRockInfo.rockIndex));
                            }
                            else StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                        //centre and tight centre guards
                        else if (cenGuard & tCenGuard)
                        {
                            //if the centre and tight centre guards are not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f & Mathf.Abs(closestRock.transform.position.x - tCenGuard.position.x) >= 0.1f)
                            {
                                StartCoroutine(TakeOutTarget(rockCurrent, closestRockInfo.rockIndex));
                            }
                            else StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                        else
                        {
                            StartCoroutine(TakeOutTarget(rockCurrent, closestRockInfo.rockIndex));
                        }
                    }
                    //if it's not in the four foot and there's more than one rock in the house
                    else if (gm.houseList.Count > 1)
                    {
                        //if the second shot is mine
                        if (gm.houseList[1].rockInfo.teamName == closestRockInfo.teamName)
                        {

                            StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                        else
                        {
                            StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[1].rockInfo.rockIndex));
                        }
                    }
                    else
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                }
                break;

            case "middle hammer":

                //no one is in the house
                if (gm.houseList.Count == 0)
                {
                    //tight centre and no centre guard
                    if (lCornGuard && !rCornGuard)
                    {
                        StartCoroutine(Shot("Right Corner Guard"));
                    }
                    //centre and no tight centre guard
                    else if (rCornGuard && !lCornGuard)
                    {
                        StartCoroutine(Shot("Left Corner Guard"));
                    }
                    //centre and tight centre guards
                    else if (lCornGuard & rCornGuard)
                    {
                        StartCoroutine(DrawTwelveFoot(rockCurrent));
                    }
                    else
                    {
                        if (Random.value > 0.5f)
                            StartCoroutine(Shot("Left Corner Guard"));
                        else
                            StartCoroutine(Shot("Right Corner Guard"));
                    }
                }
                //closest rock is mine
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    //left guard only
                    if (lCornGuard && !rCornGuard)
                    {
                        if (Mathf.Abs(closestRock.transform.position.x - lCornGuard.position.x) >= 0.1f)
                        {
                            StartCoroutine(Shot("High Left Corner Guard"));
                        }
                        else
                        {
                            rm.inturn = false;
                            StartCoroutine(Shot("Left Twelve Foot"));
                        }
                    }
                    //right guard only
                    else if (rCornGuard && !lCornGuard)
                    {
                        //the rock is not guarded
                        if (Mathf.Abs(closestRock.transform.position.x - rCornGuard.position.x) >= 0.1f)
                        {
                            StartCoroutine(Shot("High Right Corner Guard"));
                        }
                        else
                        {
                            rm.inturn = true;
                            StartCoroutine(Shot("Right Twelve Foot"));
                        }
                    }
                    //left and right corner guards
                    else if (rCornGuard && lCornGuard)
                    {
                        //the rock is not guarded on the left
                        if (Mathf.Abs(closestRock.transform.position.x - lCornGuard.position.x) >= 0.1f)
                        {
                            StartCoroutine(Shot("High Right Corner Guard"));
                        }
                        //the rock is not guarded on the right
                        else if(Mathf.Abs(closestRock.transform.position.x - rCornGuard.position.x) >= 0.1f)
                        {
                            StartCoroutine(Shot("High Left Corner Guard"));
                        }
                        else
                        {
                            if (closestRock.transform.position.x < 0)
                            {
                                rm.inturn = true;
                                StartCoroutine(Shot("Right Twelve Foot"));
                            }
                            else if (closestRock.transform.position.x > 0)
                            {
                                rm.inturn = false;
                                StartCoroutine(Shot("Left Twelve Foot"));
                            }
                            else StartCoroutine(DrawTwelveFoot(rockCurrent));
                        }
                    }
                    //any other guard combo
                    else StartCoroutine(DrawTwelveFoot(rockCurrent));
                }
                //closest rock is not mine
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    //if it's in the four foot
                    if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                    {
                        //tight centre guard
                        if (tCenGuard)
                        {
                            //if the tight centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - tCenGuard.position.x) >= 0.1f)
                            {
                                StartCoroutine(TakeOutTarget(rockCurrent, closestRockInfo.rockIndex));
                            }
                            else
                                StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                        //centre guard
                        if (cenGuard)
                        {
                            //if the tight centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f)
                            {
                                StartCoroutine(TakeOutTarget(rockCurrent, closestRockInfo.rockIndex));
                            }
                            else
                                StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                        else
                        {
                            StartCoroutine(TakeOutTarget(rockCurrent, closestRockInfo.rockIndex));
                        }
                    }
                    //if it's not in the four foot and there's more than one rock in the house
                    else if (gm.houseList.Count > 1)
                    {
                        //if the second shot is mine
                        if (gm.houseList[1].rockInfo.teamName == closestRockInfo.teamName)
                        {

                            StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                        else
                        {
                            StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[1].rockInfo.rockIndex));
                        }
                    }
                    //not in the four foot and the only rock
                    else
                    {
                        StartCoroutine(DrawTwelveFoot(rockCurrent));
                    }
                }
                break;


            case "late no hammer":

                //if no rocks in the house
                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(DrawFourFoot(rockCurrent));
                }
                //if the closest rock is mine
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    //if it's in the four foot
                    if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6f)) <= 0.5f)
                    {
                        //tight centre and no centre guard
                        if (tCenGuard && !cenGuard)
                        {
                            //if the tight centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - tCenGuard.position.x) >= 0.1f)
                            {
                                StartCoroutine(Shot("Centre Guard"));
                            }
                            //if the tight centre guard is covering it
                            else
                            {
                                //if second shot is mine
                                if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                                {
                                    //if third shot exists and is not mine
                                    if (gm.houseList.Count > 2 && gm.houseList[2].rockInfo.teamName != rockInfo.teamName)
                                    {
                                        if (gm.houseList[2].rock.transform.position.x < 0)
                                        {
                                            if (lCornGuard) StartCoroutine(TakeOutTarget(rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex));
                                            else StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[2].rockInfo.rockIndex));
                                        }
                                        else if (gm.houseList[2].rock.transform.position.x > 0)
                                        {
                                            if (rCornGuard) StartCoroutine(TakeOutTarget(rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex));
                                            else StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[2].rockInfo.rockIndex));
                                        }
                                    }
                                    else if (gm.houseList[1].rock.transform.position.x < 0)
                                    {
                                        if (lCornGuard) StartCoroutine(DrawFourFoot(rockCurrent));
                                        else StartCoroutine(Shot("Left Corner Guard"));
                                    }
                                    else if (gm.houseList[1].rock.transform.position.x > 0)
                                    {
                                        if (rCornGuard) StartCoroutine(DrawFourFoot(rockCurrent));
                                        else StartCoroutine(Shot("Right Corner Guard"));
                                    }
                                }
                                //if second shot is not mine
                                else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                                {
                                    if (gm.houseList[1].rock.transform.position.x < 0)
                                    {
                                        if (lCornGuard) StartCoroutine(TakeOutTarget(rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex));
                                        else StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[1].rockInfo.rockIndex));
                                    }
                                    else if (gm.houseList[1].rock.transform.position.x > 0)
                                    {
                                        if (rCornGuard) StartCoroutine(TakeOutTarget(rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex));
                                        else StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[1].rockInfo.rockIndex));
                                    }
                                    else StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[1].rockInfo.rockIndex));
                                }
                                //no second shot rock
                                else
                                {
                                    StartCoroutine(DrawFourFoot(rockCurrent));
                                }
                            }

                        }
                        //centre and no tight centre guard
                        else if (cenGuard && !tCenGuard)
                        {
                            //if the centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f)
                            {
                                StartCoroutine(Shot("Tight Centre Guard"));
                            }
                            //if the centre guard is covering shot rock
                            else
                            {
                                //if second shot exists and is mine
                                if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                                {
                                    //if third shot exists and is not mine
                                    if (gm.houseList.Count > 2 && gm.houseList[2].rockInfo.teamName != rockInfo.teamName)
                                    {
                                        if (gm.houseList[2].rock.transform.position.x < 0)
                                        {
                                            if (lCornGuard) StartCoroutine(TakeOutTarget(rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex));
                                            else StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[2].rockInfo.rockIndex));
                                        }
                                        else if (gm.houseList[2].rock.transform.position.x > 0)
                                        {
                                            if (rCornGuard) StartCoroutine(TakeOutTarget(rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex));
                                            else StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[2].rockInfo.rockIndex));
                                        }
                                    }
                                    //if second shot is to the left
                                    else if (gm.houseList[1].rock.transform.position.x < 0)
                                    {
                                        if (lCornGuard) StartCoroutine(DrawFourFoot(rockCurrent));
                                        else StartCoroutine(Shot("Left Corner Guard"));
                                    }
                                    //if second shot is to the right
                                    else if (gm.houseList[1].rock.transform.position.x > 0)
                                    {
                                        if (rCornGuard) StartCoroutine(DrawFourFoot(rockCurrent));
                                        else StartCoroutine(Shot("Right Corner Guard"));
                                    }
                                }
                                //if second shot is not mine
                                else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                                {
                                    if (gm.houseList[1].rock.transform.position.x < 0)
                                    {
                                        if (lCornGuard) StartCoroutine(TakeOutTarget(rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex));
                                        else StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[1].rockInfo.rockIndex));
                                    }
                                    else if (gm.houseList[1].rock.transform.position.x > 0)
                                    {
                                        if (rCornGuard) StartCoroutine(TakeOutTarget(rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex));
                                        else StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[1].rockInfo.rockIndex));
                                    }
                                    else StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[1].rockInfo.rockIndex));
                                }
                                //no second shot rock
                                else
                                {
                                    StartCoroutine(DrawFourFoot(rockCurrent));
                                }
                            }
                        }
                        //centre and tight centre guards
                        else if (cenGuard & tCenGuard)
                        {
                            //if the centre and tight centre guards are not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f & Mathf.Abs(closestRock.transform.position.x - tCenGuard.position.x) >= 0.1f)
                            {
                                StartCoroutine(DrawFourFoot(rockCurrent));
                            }
                            else StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                        else
                        {
                            //if second shot is mine
                            if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                            {
                                StartCoroutine(TakeOutTarget(rockCurrent, cenGuard.gameObject.GetComponent<Rock_Info>().rockIndex));
                            }
                            //if second shot is not mine
                            else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                            {
                                StartCoroutine(DrawFourFoot(rockCurrent));
                            }
                        }
                    }
                    //if it's not in the four foot
                    else
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                }
                //closest rock is not mine
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    //if it's in the four foot
                    if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                    {
                        //tight centre and no centre guard
                        if (tCenGuard && !cenGuard)
                        {
                            //if the tight centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - tCenGuard.position.x) >= 0.1f)
                            {
                                StartCoroutine(TakeOutTarget(rockCurrent, closestRockInfo.rockIndex));
                            }
                            //if the tight centre guard is covering it
                            else
                            {
                                //if second shot is mine
                                if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                                {
                                    StartCoroutine(TakeOutTarget(rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex));

                                    //if third shot exists and is not mine
                                    if (gm.houseList.Count > 2 && gm.houseList[2].rockInfo.teamName != rockInfo.teamName)
                                    {
                                        if (gm.houseList[2].rock.transform.position.x < 0)
                                        {
                                            if (lCornGuard) StartCoroutine(TakeOutTarget(rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex));
                                            else StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[1].rockInfo.rockIndex));
                                        }
                                        else if (gm.houseList[2].rock.transform.position.x > 0)
                                        {
                                            if (rCornGuard) StartCoroutine(TakeOutTarget(rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex));
                                            else StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[1].rockInfo.rockIndex));
                                        }
                                    }
                                    else if (gm.houseList[1].rock.transform.position.x < 0)
                                    {
                                        if (lCornGuard) StartCoroutine(DrawFourFoot(rockCurrent));
                                        else StartCoroutine(Shot("Left Corner Guard"));
                                    }
                                    else if (gm.houseList[1].rock.transform.position.x > 0)
                                    {
                                        if (rCornGuard) StartCoroutine(DrawFourFoot(rockCurrent));
                                        else StartCoroutine(Shot("Right Corner Guard"));
                                    }
                                }
                                //if second shot is not mine
                                else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                                {
                                    if (gm.houseList[1].rock.transform.position.x < 0)
                                    {
                                        if (lCornGuard) StartCoroutine(TakeOutTarget(rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex));
                                        else StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[1].rockInfo.rockIndex));
                                    }
                                    else if (gm.houseList[1].rock.transform.position.x > 0)
                                    {
                                        if (rCornGuard) StartCoroutine(TakeOutTarget(rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex));
                                        else StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[1].rockInfo.rockIndex));
                                    }
                                    else StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[1].rockInfo.rockIndex));
                                }
                                //no second shot rock
                                else
                                {
                                    StartCoroutine(TakeOutTarget(rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex));
                                }
                            }

                        }
                        //centre and no tight centre guard
                        else if (cenGuard && !tCenGuard)
                        {
                            //if the centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f)
                            {
                                StartCoroutine(TakeOutTarget(rockCurrent, closestRockInfo.rockIndex));
                            }
                            else
                            {
                                //if second shot is mine
                                if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                                {
                                    StartCoroutine(TakeOutTarget(rockCurrent, cenGuard.gameObject.GetComponent<Rock_Info>().rockIndex));
                                }
                                //if second shot is not mine
                                else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                                {
                                    StartCoroutine(DrawFourFoot(rockCurrent));
                                }
                            }
                        }
                        //centre and tight centre guards
                        else if (cenGuard & tCenGuard)
                        {
                            //if the centre and tight centre guards are not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f & Mathf.Abs(closestRock.transform.position.x - tCenGuard.position.x) >= 0.1f)
                            {
                                StartCoroutine(TakeOutTarget(rockCurrent, closestRockInfo.rockIndex));
                            }
                            else StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                        else
                        {
                            //if second shot is mine
                            if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                            {
                                StartCoroutine(TakeOutTarget(rockCurrent, cenGuard.gameObject.GetComponent<Rock_Info>().rockIndex));
                            }
                            //if second shot is not mine
                            else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                            {
                                StartCoroutine(DrawFourFoot(rockCurrent));
                            }
                        }
                    }
                    //if it's not in the four foot and there's more than one rock in the house
                    else if (gm.houseList.Count > 1)
                    {
                        //if the second shot is mine
                        if (gm.houseList[1].rockInfo.teamName == closestRockInfo.teamName)
                        {

                            StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                        else
                        {
                            StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[1].rockInfo.rockIndex));
                        }
                    }
                    else
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                }
                //default
                else
                {
                    StartCoroutine(DrawFourFoot(rockCurrent));
                    Debug.Log("Default Late Phase");
                }
                break;


            case "late hammer":

                //no one is in the house
                if (gm.houseList.Count == 0)
                {
                    if (gm.rockTotal - rockCurrent > 1)
                    {
                        //left corner and no right corner guard
                        if (lCornGuard && !rCornGuard)
                        {
                            StartCoroutine(Shot("Left Twelve Foot"));
                        }
                        //right corner and no left corner guard
                        else if (rCornGuard && !lCornGuard)
                        {
                            StartCoroutine(Shot("Right Twelve Foot"));
                        }
                        //right corner and left corner guards
                        else if (lCornGuard & rCornGuard)
                        {
                            StartCoroutine(DrawTwelveFoot(rockCurrent));
                        }
                        else
                        {
                            StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                    }
                    else
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                }
                //closest rock is mine
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    if (gm.rockTotal - rockCurrent > 1)
                    {
                        //left guard only
                        if (lCornGuard && !rCornGuard)
                        {
                            if (Mathf.Abs(closestRock.transform.position.x - lCornGuard.position.x) >= 0.1f)
                            {
                                StartCoroutine(Shot("High Left Corner Guard"));
                            }
                            else
                            {
                                rm.inturn = false;
                                StartCoroutine(Shot("Left Twelve Foot"));
                            }
                        }
                        //right guard only
                        else if (rCornGuard && !lCornGuard)
                        {
                            //the rock is not guarded
                            if (Mathf.Abs(closestRock.transform.position.x - rCornGuard.position.x) >= 0.1f)
                            {
                                StartCoroutine(Shot("High Right Corner Guard"));
                            }
                            else
                            {
                                rm.inturn = true;
                                StartCoroutine(Shot("Right Twelve Foot"));
                            }
                        }
                        //left and right corner guards
                        else if (rCornGuard && lCornGuard)
                        {
                            //the rock is not guarded on the left
                            if (Mathf.Abs(closestRock.transform.position.x - lCornGuard.position.x) >= 0.1f)
                            {
                                StartCoroutine(Shot("High Right Corner Guard"));
                            }
                            //the rock is not guarded on the right
                            else if (Mathf.Abs(closestRock.transform.position.x - rCornGuard.position.x) >= 0.1f)
                            {
                                StartCoroutine(Shot("High Left Corner Guard"));
                            }
                            else
                            {
                                if (closestRock.transform.position.x < 0)
                                {
                                    rm.inturn = true;
                                    StartCoroutine(Shot("Right Twelve Foot"));
                                }
                                else if (closestRock.transform.position.x > 0)
                                {
                                    rm.inturn = false;
                                    StartCoroutine(Shot("Left Twelve Foot"));
                                }
                                else StartCoroutine(DrawTwelveFoot(rockCurrent));
                            }
                        }
                        //any other guard combo
                        else StartCoroutine(DrawTwelveFoot(rockCurrent));
                    }
                    else
                    {
                        //if there's more than one rock in the house
                        if (gm.houseList.Count > 1)
                        {
                            //if the second shot is mine
                            if (gm.houseList[1].rockInfo.teamName == closestRockInfo.teamName)
                            {

                                StartCoroutine(DrawFourFoot(rockCurrent));
                            }
                            else
                            {
                                StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[1].rockInfo.rockIndex));
                            }
                        }
                        else
                        {
                            StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                    }
                }
                //closest rock is not mine
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    //if it's in the four foot
                    if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                    {
                        //tight centre guard
                        if (tCenGuard)
                        {
                            //if the tight centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - tCenGuard.position.x) >= 0.1f)
                            {
                                StartCoroutine(TakeOutTarget(rockCurrent, closestRockInfo.rockIndex));
                            }
                            else
                                StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                        //centre guard
                        if (cenGuard)
                        {
                            //if the tight centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f)
                            {
                                StartCoroutine(TakeOutTarget(rockCurrent, closestRockInfo.rockIndex));
                            }
                            else
                                StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                        else
                        {
                            StartCoroutine(TakeOutTarget(rockCurrent, closestRockInfo.rockIndex));
                        }
                    }
                    //if it's not in the four foot and there's more than one rock in the house
                    else if (gm.houseList.Count > 1)
                    {
                        //if the second shot is mine
                        if (gm.houseList[1].rockInfo.teamName == closestRockInfo.teamName)
                        {

                            StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                        else
                        {
                            StartCoroutine(TakeOutTarget(rockCurrent, gm.houseList[1].rockInfo.rockIndex));
                        }
                    }
                    //not in the four foot and the only rock
                    else
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                        
                }
                break;


            default:
                break;
        }
    }

    IEnumerator Shot(string aiShotType)
    {
        Debug.Log("AI Shot " + aiShotType);
        gm.dbText.text = aiShotType;
        rockFlick.isPressedAI = true;

        yield return new WaitForSeconds(0.5f);

        float shotX;
        float shotY;

        switch (aiShotType)
        {
            case "Centre Guard":
                if (rm.inturn)
                {
                    shotX = -1f * Random.Range(centreGuard.x + guardAccu.x, centreGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(centreGuard.x + guardAccu.x, centreGuard.x - guardAccu.x);
                }
                
                shotY = Random.Range(centreGuard.y + guardAccu.y, centreGuard.y - guardAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Tight Centre Guard":
                if (rm.inturn)
                {
                    shotX = -1f * Random.Range(tightCentreGuard.x + guardAccu.x, tightCentreGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(tightCentreGuard.x + guardAccu.x, tightCentreGuard.x - guardAccu.x);
                }
                shotY = Random.Range(tightCentreGuard.y + guardAccu.y, tightCentreGuard.y - guardAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "High Centre Guard":
                if (rm.inturn)
                {
                    shotX = -1f * Random.Range(highCentreGuard.x + guardAccu.x, highCentreGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(highCentreGuard.x + guardAccu.x, highCentreGuard.x - guardAccu.x);
                }
                shotY = Random.Range(highCentreGuard.y + guardAccu.y, highCentreGuard.y - guardAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Left Corner Guard":
                if (rm.inturn)
                {
                    shotX = -1f * Random.Range(rightCornerGuard.x + guardAccu.x, rightCornerGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(leftCornerGuard.x + guardAccu.x, leftCornerGuard.x - guardAccu.x);
                }
                shotY = Random.Range(leftCornerGuard.y + guardAccu.y, leftCornerGuard.y - guardAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Left Tight Corner Guard":
                shotX = Random.Range(leftTightCornerGuard.x + guardAccu.x, leftTightCornerGuard.x - guardAccu.x);
                shotY = Random.Range(leftTightCornerGuard.y + guardAccu.y, leftTightCornerGuard.y - guardAccu.y);
                yield return new WaitForFixedUpdate();
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Left High Corner Guard":
                shotX = Random.Range(leftHighCornerGuard.x + guardAccu.x, leftHighCornerGuard.x - guardAccu.x);
                shotY = Random.Range(leftHighCornerGuard.y + guardAccu.y, leftHighCornerGuard.y - guardAccu.y);
                yield return new WaitForFixedUpdate();
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Right Corner Guard":
                 shotX = Random.Range(rightCornerGuard.x + guardAccu.x, rightCornerGuard.x - guardAccu.x);
                 shotY = Random.Range(rightCornerGuard.y + guardAccu.y, rightCornerGuard.y - guardAccu.y);
                yield return new WaitForFixedUpdate();
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Right Tight Corner Guard":
                shotX = Random.Range(rightTightCornerGuard.x + guardAccu.x, rightTightCornerGuard.x - guardAccu.x);
                shotY = Random.Range(rightTightCornerGuard.y + guardAccu.y, rightTightCornerGuard.y - guardAccu.y);
                yield return new WaitForFixedUpdate();
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
                if (rm.inturn)
                {
                    shotX = -1f * Random.Range(topTwelveFoot.x + drawAccu.x, topTwelveFoot.x - drawAccu.x);
                }
                else
                {
                    shotX = Random.Range(topTwelveFoot.x + drawAccu.x, topTwelveFoot.x - drawAccu.x);
                }
                 shotY = Random.Range(topTwelveFoot.y + drawAccu.y, topTwelveFoot.y - drawAccu.y);
                yield return new WaitForFixedUpdate();
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Left Twelve Foot":
                if (rm.inturn)
                {
                    shotX = -1f * Random.Range(rightTwelveFoot.x + drawAccu.x, rightTwelveFoot.x - drawAccu.x);
                }
                else
                {
                    shotX = Random.Range(leftTwelveFoot.x + drawAccu.x, leftTwelveFoot.x - drawAccu.x);
                }
                
                shotY = Random.Range(leftTwelveFoot.y + drawAccu.y, leftTwelveFoot.y - drawAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Back Twelve Foot":
                if (rm.inturn)
                {
                    shotX = -1f * Random.Range(backTwelveFoot.x + drawAccu.x, backTwelveFoot.x - drawAccu.x);
                }
                else
                {
                    shotX = Random.Range(backTwelveFoot.x + drawAccu.x, backTwelveFoot.x - drawAccu.x);
                }
                shotY = Random.Range(backTwelveFoot.y + drawAccu.y, backTwelveFoot.y - drawAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Right Twelve Foot":
                if (rm.inturn)
                {
                    shotX = -1f * Random.Range(leftTwelveFoot.x + drawAccu.x, leftTwelveFoot.x - drawAccu.x);
                }
                else
                {
                    shotX = Random.Range(rightTwelveFoot.x + drawAccu.x, rightTwelveFoot.x - drawAccu.x);
                }
                shotY = Random.Range(rightTwelveFoot.y + drawAccu.y, rightTwelveFoot.y - drawAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Button":
                if (rm.inturn)
                {
                    shotX = -1f * Random.Range(button.x + drawAccu.x, button.x - drawAccu.x);
                }
                else
                {
                    shotX = Random.Range(button.x + drawAccu.x, button.x - drawAccu.x);
                }
                shotY = Random.Range(button.y + drawAccu.y, button.y - drawAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Left Four Foot":
                if (rm.inturn)
                {
                    shotX = -1f * Random.Range(rightFourFoot.x + drawAccu.x, rightFourFoot.x - drawAccu.x);
                }
                else
                {
                    shotX = Random.Range(leftFourFoot.x + drawAccu.x, leftFourFoot.x - drawAccu.x);
                }
                shotY = Random.Range(leftFourFoot.y + drawAccu.y, leftFourFoot.y - drawAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Right Four Foot":
                if (rm.inturn)
                {
                    shotX = -1f * Random.Range(leftFourFoot.x + drawAccu.x, leftFourFoot.x - drawAccu.x);
                }
                else
                {
                    shotX = Random.Range(rightFourFoot.x + drawAccu.x, rightFourFoot.x - drawAccu.x);
                }
                shotY = Random.Range(rightFourFoot.y + drawAccu.y, rightFourFoot.y - drawAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Top Four Foot":
                if (rm.inturn)
                {
                    shotX = -1f * Random.Range(topFourFoot.x + drawAccu.x, topFourFoot.x - drawAccu.x);
                }
                else
                {
                    shotX = Random.Range(topFourFoot.x + drawAccu.x, topFourFoot.x - drawAccu.x);
                }
                shotY = Random.Range(topFourFoot.y + drawAccu.y, topFourFoot.y - drawAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Back Four Foot":
                if (rm.inturn)
                {
                    shotX = -1f * Random.Range(backFourFoot.x + drawAccu.x, backFourFoot.x - drawAccu.x);
                }
                else
                {
                    shotX = Random.Range(backFourFoot.x + drawAccu.x, backFourFoot.x - drawAccu.x);
                }
                shotY = Random.Range(backFourFoot.y + drawAccu.y, backFourFoot.y - drawAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Peel":
                if (takeOutX != 0f)
                {
                    shotX = Random.Range(takeOutX + toAccu.x, takeOutX - toAccu.x) + peelOffset;
                    shotY = Random.Range(peel.y + toAccu.y, peel.y - toAccu.y);
                }
                else
                {
                    shotX = Random.Range(button.x + drawAccu.x, button.x - drawAccu.x);
                    shotY = Random.Range(button.y + drawAccu.y, button.y - drawAccu.y);
                }

                rockRB.position = new Vector2(shotX, shotY);

                Debug.Log("Peel Position is (" + rockRB.position.x + " ," + rockRB.position.y + ")");
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Take Out":

                if (takeOutX != 0f)
                {
                    if (rm.inturn)
                    {
                        takeOutOffset = -takeOutOffset;
                    }

                    shotX = Random.Range(takeOutX + toAccu.x, takeOutX - toAccu.x) + takeOutOffset;
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
                rockFlick.mouseUp = true;
                break;

            case "Tick":


                if (takeOutX != 0f)
                {
                    shotX = Random.Range(takeOutX + toAccu.x, takeOutX - toAccu.x) + tickOffset;
                    shotY = Random.Range(tick.y + toAccu.y, tick.y - toAccu.y);
                }
                else
                {
                    shotX = Random.Range(button.x + drawAccu.x, button.x - drawAccu.x);
                    shotY = Random.Range(button.y + drawAccu.y, button.y - drawAccu.y);
                }

                rockRB.position = new Vector2(shotX, shotY);

                Debug.Log("Tick Shot Position is (" + rockRB.position.x + " ," + rockRB.position.y + ")");
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Raise":


                if (takeOutX != 0f)
                {
                    shotX = Random.Range(takeOutX + toAccu.x, takeOutX - toAccu.x) + raiseOffset;
                    shotY = Random.Range(raise.y + toAccu.y, raise.y - toAccu.y);
                }
                else
                {
                    shotX = Random.Range(button.x + drawAccu.x, button.x - drawAccu.x);
                    shotY = Random.Range(button.y + drawAccu.y, button.y - drawAccu.y);
                }

                rockRB.position = new Vector2(shotX, shotY);

                Debug.Log("Raise Position is (" + rockRB.position.x + " ," + rockRB.position.y + ")");
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            default:
                break;
        }

    }
}
