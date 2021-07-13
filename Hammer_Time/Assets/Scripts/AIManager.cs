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

    public Vector2 guardAccu;
    public Vector2 drawAccu;
    public Vector2 toAccu;
    public Vector2 tickAccu;

    public bool aggressive;

    public Transform cenGuard;
    public Transform lCornGuard;
    public Transform rCornGuard;

    public float takeOutOffset;
    public float peelOffset;
    public float raiseOffset;
    public float tickOffset;

    bool inturn;
    float targetX;
    public float takeOutX;
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

            //StartCoroutine(Shot(testing));
            StartCoroutine(TickShot(gm.rockCurrent));
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

            StartCoroutine(Shot(testing));
            //StartCoroutine(TickShot(gm.rockCurrent));
            //StartCoroutine(Shot("Take Out"));
        }
    }

    public void OnShot(int rockCurrent)
    {
        rockInfo = gm.rockList[rockCurrent].rockInfo;
        rockFlick = gm.rockList[rockCurrent].rock.GetComponent<Rock_Flick>();
        rockRB = gm.rockList[rockCurrent].rock.GetComponent<Rigidbody2D>();

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

                posX = guard.lastTransform.position.x;

                // center lane
                if (Mathf.Abs(posX) <= 0.4f)
                {
                    cenGuard = guard.lastTransform;
                }
                // left channel 
                else if (posX < -0.4f && posX > -1.25f)
                {
                    lCornGuard = guard.lastTransform;
                }
                // right channel
                else if (posX > 0.4f && posX < 1.25f)
                {
                    rCornGuard = guard.lastTransform;
                }

                else
                {
                    cenGuard = null;
                    lCornGuard = null;
                    rCornGuard = null;
                }

                Debug.Log("posX is " + guard.lastTransform.position.x);
            }

        }
        else
        {
            cenGuard = null;
            lCornGuard = null;
            rCornGuard = null;
        }

        yield return new WaitForEndOfFrame();
    }

    IEnumerator TakeOutTarget(int rockCurrent)
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
                if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.4f)
                {
                    //if there's no centre guard
                    if (!cenGuard)
                    {
                        targetX = closestRock.transform.position.x;
                        takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
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
                            takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                            StartCoroutine(Shot("Take Out"));
                            Debug.Log(closestRockInfo.teamName + " " + closestRockInfo.rockNumber);
                            yield break;
                        }
                        //if the centre guard is not mine
                        else if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                        {
                            //let's take it out
                            targetX = cenGuard.position.x;
                            takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                            StartCoroutine(Shot("Peel"));
                            Debug.Log(closestRockInfo.teamName + " " + closestRockInfo.rockNumber);
                            yield break;
                        }
                    }
                }
                //if it's not in the four foot
                else
                {
                    //if there's more than one in the house
                    if (gm.houseList.Count > 1)
                    {
                        //if second shot is not mine
                        if (gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                        {
                            //if the second shot is also close to the pin
                            if (Vector2.Distance(gm.houseList[1].rock.transform.position, new Vector2(0f, 6.5f)) <= 0.6f)
                            {
                                //if there's no centre guard
                                if (!cenGuard)
                                {
                                    //if the rocks are close together
                                    if (Vector2.Distance(gm.houseList[1].rock.transform.position, closestRock.transform.position) <= 0.5f)
                                    {
                                        targetX = gm.houseList[1].rock.transform.position.x;
                                        takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                                        StartCoroutine(Shot("Take Out"));
                                        Debug.Log(gm.houseList[1].rockInfo.teamName + " " + gm.houseList[1].rockInfo.rockNumber);
                                        yield break;
                                    }
                                    //if the rocks are not close together
                                    else
                                    {
                                        targetX = closestRock.transform.position.x;
                                        takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                                        StartCoroutine(Shot("Take Out"));
                                        Debug.Log(closestRockInfo.teamName + " " + closestRockInfo.rockNumber);
                                        yield break;
                                    }
                                }
                                //if there's a centre guard
                                else
                                {
                                    if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                                    {
                                        targetX = cenGuard.position.x;
                                        takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                                        StartCoroutine(Shot("Take Out"));
                                        Debug.Log(closestRockInfo.teamName + " " + closestRockInfo.rockNumber);
                                        yield break;
                                    }
                                    else
                                    {
                                        targetX = cenGuard.position.x;
                                        takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                                        StartCoroutine(Shot("Peel"));
                                        Debug.Log(closestRockInfo.teamName + " " + closestRockInfo.rockNumber);
                                        yield break;
                                    }
                                }
                            }
                            //
                            else
                            {
                                if (gm.houseList[1].rock.transform.position.x < 0f)
                                {
                                    if (rCornGuard)
                                    {
                                        if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                                        {
                                            targetX = rCornGuard.position.x;
                                            takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                                            StartCoroutine(Shot("Take Out"));
                                            Debug.Log(closestRockInfo.teamName + " " + closestRockInfo.rockNumber);
                                            yield break;
                                        }
                                        else
                                        {
                                            targetX = rCornGuard.position.x;
                                            takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                                            StartCoroutine(Shot("Peel"));
                                            Debug.Log(closestRockInfo.teamName + " " + closestRockInfo.rockNumber);
                                            yield break;
                                        }
                                    }
                                    else
                                    {
                                        targetX = gm.houseList[1].rock.transform.position.x;
                                        takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                                        StartCoroutine(Shot("Take Out"));
                                        Debug.Log(gm.houseList[1].rockInfo.teamName + " " + gm.houseList[1].rockInfo.rockNumber);
                                        yield break;
                                    }
                                }
                                else
                                {
                                    if (lCornGuard)
                                    {
                                        if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                                        {
                                            targetX = lCornGuard.position.x;
                                            takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                                            StartCoroutine(Shot("Take Out"));
                                            Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                                            yield break;
                                        }
                                        else
                                        {
                                            targetX = rCornGuard.position.x;
                                            takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                                            StartCoroutine(Shot("Peel"));
                                            Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                                            yield break;
                                        }
                                    }
                                    else
                                    {
                                        targetX = gm.houseList[1].rock.transform.position.x;
                                        takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                                        StartCoroutine(Shot("Take Out"));
                                        Debug.Log(gm.houseList[1].rockInfo.teamName + " " + gm.houseList[1].rockInfo.rockNumber);
                                        yield break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!cenGuard)
                            {
                                //if the rocks are close together
                                if (Vector2.Distance(gm.houseList[1].rock.transform.position, closestRock.transform.position) <= 0.5f)
                                {
                                    targetX = gm.houseList[1].rock.transform.position.x;
                                    takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                                    StartCoroutine(Shot("Take Out"));
                                    Debug.Log(gm.houseList[1].rockInfo.teamName + " " + gm.houseList[1].rockInfo.rockNumber);
                                    yield break;
                                }
                                //if the rocks are not close together
                                else
                                {
                                    targetX = closestRock.transform.position.x;
                                    takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                                    StartCoroutine(Shot("Take Out"));
                                    Debug.Log(closestRockInfo.teamName + " " + closestRockInfo.rockNumber);
                                    yield break;
                                }
                            }
                            //if there's a centre guard
                            else
                            {
                                //if it's my centre guard
                                if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                                {
                                    targetX = cenGuard.position.x;
                                    takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                                    StartCoroutine(Shot("Take Out"));
                                    Debug.Log(closestRockInfo.teamName + " " + closestRockInfo.rockNumber);
                                    yield break;
                                }
                                //if it's not my centre guard
                                else
                                {
                                    targetX = cenGuard.position.x;
                                    takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                                    StartCoroutine(Shot("Peel"));
                                    Debug.Log(closestRockInfo.teamName + " " + closestRockInfo.rockNumber);
                                    yield break;
                                }
                            }
                        }
                    }
                    //if there's only one in the house
                    else
                    {
                        Debug.Log("House List Count is " + gm.houseList.Count);
                        targetX = closestRock.transform.position.x;
                        takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                        StartCoroutine(Shot("Take Out"));
                        Debug.Log(closestRockInfo.teamName + " " + closestRockInfo.rockNumber);
                        yield break;
                    }
                }
            }
            else
            {

                Debug.Log("My rock is in the four foot");
                if (gm.gList.Count != 0)
                {
                    if (!cenGuard)
                    {
                        StartCoroutine(Shot("Tight Centre Guard"));
                        Debug.Log("no Target - Guard");
                        yield break;
                    }
                    else
                    {
                        //if the centre guard is mine
                        if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                        {
                            //let's run it back
                            targetX = cenGuard.position.x;
                            takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                            StartCoroutine(Shot("Take Out"));
                            Debug.Log(closestRockInfo.teamName + " " + closestRockInfo.rockNumber);
                            yield break;
                        }
                        //if the centre guard is not mine
                        else
                        {
                            //let's take it out
                            targetX = cenGuard.position.x;
                            takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                            StartCoroutine(Shot("Peel"));
                            Debug.Log(closestRockInfo.teamName + " " + closestRockInfo.rockNumber);
                            yield break;
                        }
                    }
                    
                }
                else
                {
                    targetX = closestRock.transform.position.x;
                    takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                    StartCoroutine(Shot("Button"));
                    Debug.Log("no Target - button");
                    yield break;
                }

            }
        }
        else if (gm.gList.Count != 0)
        {
            if (cenGuard && rCornGuard && lCornGuard)
            {
                if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Raise"));
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                    yield return StartCoroutine(Shot("Raise"));
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
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
            else if (!cenGuard && rCornGuard && !lCornGuard)
            {
                if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                    yield return StartCoroutine(Shot("Raise"));
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
            }
            else if (!cenGuard && !rCornGuard && lCornGuard)
            {
                if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Raise"));
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
            }
            else if (!cenGuard && rCornGuard && lCornGuard)
            {
                if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Raise"));
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
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
            else if (cenGuard && rCornGuard && !lCornGuard)
            {
                if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Raise"));
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
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
            else if (cenGuard && !rCornGuard && lCornGuard)
            {
                if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Peel"));
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
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
            else if (cenGuard && !rCornGuard && !lCornGuard)
            {
                if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    yield return StartCoroutine(Shot("Raise"));
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
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
            else
            {
                targetX = 0f;
                takeOutX = 0f;
                yield return StartCoroutine(Shot("Peel"));
                Debug.Log("No Targets available - Button");
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
        //no guards
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
                else if (cenGuard.position.x < 0f)
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
                    else if (cenGuard.position.x < 0f)
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
                        else if (cenGuard.position.x < 0f)
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
                    else if (rCornGuard.position.y < 2.0f)
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
                        else if (cenGuard.position.x < 0f)
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
                    else if (cenGuard.position.x < 0f)
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
                else if (cenGuard.position.x < 0f)
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
                else if (cenGuard.position.x < 0f)
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
                else if (lCornGuard.position.y < rCornGuard.position.y)
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
            else if (lCornGuard && !rCornGuard && !cenGuard)
            {
                rm.inturn = false;
                StartCoroutine(Shot("Left Four Foot"));
                yield break;
            }
        }
        else
        {
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

                    StartCoroutine(TakeOutTarget(rockCurrent));
                    break;

                case 2:

                    StartCoroutine(TakeOutTarget(rockCurrent));
                    break;

                case 3:

                    StartCoroutine(TakeOutTarget(rockCurrent));
                    break;

                case 4:

                    StartCoroutine(TakeOutTarget(rockCurrent));
                    break;

                case 5:

                    StartCoroutine(TakeOutTarget(rockCurrent));
                    break;

                case 6:

                    StartCoroutine(TakeOutTarget(rockCurrent));
                    break;

                case 7:

                    StartCoroutine(TakeOutTarget(rockCurrent));
                    break;

                case 8:

                    StartCoroutine(TakeOutTarget(rockCurrent));
                    break;

                case 9:

                    StartCoroutine(TakeOutTarget(rockCurrent));
                    break;

                case 10:

                    StartCoroutine(TakeOutTarget(rockCurrent));
                    break;

                case 11:

                    StartCoroutine(TakeOutTarget(rockCurrent));
                    break;

                case 12:
                    //rm.inturn = true;

                    StartCoroutine(TakeOutTarget(rockCurrent));
                    break;

                case 13:

                    StartCoroutine(TakeOutTarget(rockCurrent));
                    break;

                case 14:

                    StartCoroutine(TakeOutTarget(rockCurrent));
                    break;

                case 15:

                    StartCoroutine(TakeOutTarget(rockCurrent));
                    break;

                default:
                    break;
            }
        }
    }

    public void Aggressive(int rockCurrent)
    {
        GameObject rock = gm.rockList[rockCurrent].rock;
        Rock_Info rockInfo = gm.rockList[rockCurrent].rockInfo;

        switch (rockCurrent)
        { 
            case 0:

                //randomly choose
                if (Random.value > 0.5f)
                {
                    StartCoroutine(DrawFourFoot(rockCurrent));
                }
                else StartCoroutine(Shot("Tight Centre Guard"));
                break;

            case 1:

                //if the first rock is in the middle
                if (Mathf.Abs(gm.rockList[0].rock.transform.position.x) <= 0.35f)
                {
                    //if it's a guard
                    if (gm.gList.Count != 0)
                    {
                        StartCoroutine(TickShot(rockCurrent));
                    }
                    else StartCoroutine(DrawFourFoot(rockCurrent));
                }
                else StartCoroutine(DrawFourFoot(rockCurrent));
                break;

            case 2:
                //if no rocks in the house
                if (gm.houseList.Count == 0)
                {
                    //if there are guards
                    if (gm.gList.Count != 0)
                    {
                        StartCoroutine(TakeOutTarget(rockCurrent));
                    }
                    //if there are no guards
                    else
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                }
                //if the closest rock is mine
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    //if there's at least one guard
                    if (gm.gList.Count != 0)
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                    else
                    {
                        if (Mathf.Abs(closestRock.transform.position.x) <= 0.35f)
                        {
                            StartCoroutine(Shot("Centre Guard"));
                        }
                        else if (closestRock.transform.position.x < 0f)
                        {
                            StartCoroutine(Shot("Left Corner Guard"));
                        }
                        else if (closestRock.transform.position.x > 0f)
                        {
                            StartCoroutine(Shot("Right Corner Guard"));
                        }
                    }
                }
                //if the closest rock is not mine
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    //if there's a guard
                    if (gm.gList.Count != 0)
                    {
                        StartCoroutine(TakeOutTarget(rockCurrent));
                    }
                    else
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                }
                else StartCoroutine(DrawFourFoot(rockCurrent));
                break;

            case 3:
                //if no one is in the house
                if (gm.houseList.Count == 0)
                {
                    //check out the guards
                    GuardReading(rockCurrent);

                    if (cenGuard && lCornGuard)
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                    else if (cenGuard && rCornGuard)
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                    else if (cenGuard)
                    {
                        StartCoroutine(TickShot(rockCurrent));
                    }
                    else
                    {
                        StartCoroutine(DrawTwelveFoot(rockCurrent));
                    }
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    if (Mathf.Abs(closestRock.transform.position.x) <= 0.35f)
                    {
                        StartCoroutine(Shot("Tight Centre Guard"));
                    }
                    else if (closestRock.transform.position.x < 0f)
                    {
                        StartCoroutine(Shot("Left Tight Corner Guard"));
                    }
                    else if (closestRock.transform.position.x > 0f)
                    {
                        StartCoroutine(Shot("Right Tight Corner Guard"));
                    }
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    if (Mathf.Abs(closestRock.transform.position.x) <= 0.35f)
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                    else
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                }
                break;

            case 4:

                if (gm.houseList.Count == 0)
                {
                    if (gm.gList.Count != 0)
                    {
                        StartCoroutine(TakeOutTarget(rockCurrent));
                    }
                    else
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    if (gm.gList.Count != 0)
                    {
                        StartCoroutine(DrawTwelveFoot(rockCurrent));
                    }
                    else
                    {
                        StartCoroutine(Shot("Tight Centre Guard"));
                    }
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    if (gm.gList.Count != 0)
                    {
                        StartCoroutine(TakeOutTarget(rockCurrent));
                    }
                    else
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                }
                else StartCoroutine(DrawFourFoot(rockCurrent));
                break;

            case 5:

                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(DrawFourFoot(rockCurrent));
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                    {
                        if (gm.houseList[1].rockInfo.teamName == closestRockInfo.teamName)
                        {
                            StartCoroutine(Shot("Tight Centre Guard"));
                        }
                        else
                        {
                            if (Random.value > 0.5f)
                            {
                                StartCoroutine(DrawTwelveFoot(rockCurrent));
                            }
                            else
                            {
                                StartCoroutine(DrawFourFoot(rockCurrent));
                            }
                        }
                    }
                    else if (gm.gList.Count != 0)
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                    {
                        StartCoroutine(TakeOutTarget(rockCurrent));
                    }
                    else if (gm.houseList.Count >= 2)
                    {
                        if (gm.houseList[1].rockInfo.teamName == closestRockInfo.teamName)
                        {
                            StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                        else
                        {
                            StartCoroutine(TakeOutTarget(rockCurrent));
                        }
                    }
                    else
                    {
                        StartCoroutine(DrawTwelveFoot(rockCurrent));
                    }
                }
                break;

            case 6:

                if (gm.houseList.Count == 0)
                {
                    if (gm.gList.Count != 0)
                    {
                        StartCoroutine(TakeOutTarget(rockCurrent));
                    }
                    else
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    if (gm.gList.Count != 0)
                    {
                        StartCoroutine(DrawTwelveFoot(rockCurrent));
                    }
                    else
                    {
                        StartCoroutine(Shot("Tight Centre Guard"));
                    }
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    if (gm.gList.Count != 0)
                    {
                        StartCoroutine(TakeOutTarget(rockCurrent));
                    }
                    else
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                }
                else StartCoroutine(DrawFourFoot(rockCurrent));
                break;

            case 7:

                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(DrawFourFoot(rockCurrent));
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                    {
                        if (Random.value > 0.5f)
                        {
                            StartCoroutine(Shot("Left Tight Corner Guard"));
                        }
                        else
                        {
                            StartCoroutine(Shot("Right Tight Corner Guard"));
                        }
                    }
                    else
                    {
                        StartCoroutine(DrawTwelveFoot(rockCurrent));
                    }
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                    else if (gm.houseList.Count >= 2)
                    {
                        if (gm.houseList[1].rockInfo.teamName == closestRockInfo.teamName)
                        {
                            StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                        else
                        {
                            StartCoroutine(TakeOutTarget(rockCurrent));
                        }
                    }
                    else
                    {
                        StartCoroutine(DrawTwelveFoot(rockCurrent));
                    }
                }
                break;

            case 8:

                if (gm.houseList.Count == 0)
                {
                    if (gm.gList.Count != 0)
                    {
                        StartCoroutine(TakeOutTarget(rockCurrent));
                    }
                    else
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    if (gm.gList.Count != 0)
                    {
                        StartCoroutine(TakeOutTarget(rockCurrent));
                    }
                    else
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    if (gm.gList.Count != 0)
                    {
                        StartCoroutine(TakeOutTarget(rockCurrent));
                    }
                    else
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                }
                else StartCoroutine(DrawFourFoot(rockCurrent));
                break;

            case 9:

                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(DrawFourFoot(rockCurrent));
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                    {
                        StartCoroutine(Shot("Centre Guard"));
                    }
                    else
                    {
                        StartCoroutine(DrawTwelveFoot(rockCurrent));
                    }
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                    else if (gm.houseList.Count >= 2)
                    {
                        if (gm.houseList[1].rockInfo.teamName == closestRockInfo.teamName)
                        {
                            StartCoroutine(TakeOutTarget(rockCurrent));
                        }
                        else
                        {
                            StartCoroutine(DrawFourFoot(rockCurrent));
                        }
                    }
                    else
                    {
                        StartCoroutine(DrawTwelveFoot(rockCurrent));
                    }
                }
                break;

            case 10:

                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(DrawFourFoot(rockCurrent));
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    if (gm.gList.Count != 0)
                    {
                        StartCoroutine(TakeOutTarget(rockCurrent));
                    }
                    else
                    {
                        StartCoroutine(DrawFourFoot(rockCurrent));
                    }
                }
                else StartCoroutine(DrawFourFoot(rockCurrent));
                break;

            case 11:

                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(TakeOutTarget(rockCurrent));
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    StartCoroutine(DrawTwelveFoot(rockCurrent));
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    StartCoroutine(TakeOutTarget(rockCurrent));
                }
                else StartCoroutine(DrawFourFoot(rockCurrent));
                break;

            case 12:
                //rm.inturn = true;

                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(DrawTwelveFoot(rockCurrent));
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    StartCoroutine(Shot("Right Tight Corner Guard"));
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    StartCoroutine(TakeOutTarget(rockCurrent));
                }
                else StartCoroutine(Shot("Button"));
                break;

            case 13:

                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(DrawFourFoot(rockCurrent));
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    StartCoroutine(Shot("Tight Centre Guard"));
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    StartCoroutine(TakeOutTarget(rockCurrent));
                }
                else StartCoroutine(DrawFourFoot(rockCurrent));
                break;

            case 14:

                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(DrawTwelveFoot(rockCurrent));
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    StartCoroutine(Shot("Right Tight Corner Guard"));
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    StartCoroutine(TakeOutTarget(rockCurrent));
                }
                else StartCoroutine(DrawFourFoot(rockCurrent));
                break;

            case 15:
                if (gm.houseList.Count == 0)
                {
                    StartCoroutine(DrawFourFoot(rockCurrent));
                }
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    StartCoroutine(DrawFourFoot(rockCurrent));
                }
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    StartCoroutine(DrawFourFoot(rockCurrent));
                }
                else StartCoroutine(DrawFourFoot(rockCurrent));
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
                shotX = Random.Range(topFourFoot.x + drawAccu.x, topFourFoot.x - drawAccu.x);
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
