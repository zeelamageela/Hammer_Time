using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Target : MonoBehaviour
{
    public GameManager gm;
    public TutorialManager tm;
    public RockManager rm;

    public AIManager aim;
    public AI_Shooter aiShoot;
    public AI_Strategy aiStrat;

    Rock_Info rockInfo;
    Rock_Flick rockFlick;
    Rigidbody2D rockRB;

    public GameObject closestRock;
    Rock_Info closestRockInfo;

    public Transform cenGuard;
    public Transform tCenGuard;
    public Transform lCornGuard;
    public Transform rCornGuard;

    public float takeOutOffset;
    public float peelOffset;
    public float raiseOffset;
    public float tickOffset;
    public Transform targetCircle;
    public Vector2 targetPos;
    float targetX;
    float targetY;
    public float takeOutX;
    float raiseY;


    public void OnTarget(string target, int rockCurrent, int rockTarget)
    {
        targetPos = new Vector2(targetCircle.position.x, targetCircle.position.y);

        if (gm.houseList.Count != 0)
        {
            closestRock = gm.houseList[0].rock;
            closestRockInfo = gm.houseList[0].rockInfo;
        }
        rockInfo = gm.rockList[rockCurrent].rockInfo;
        rockFlick = gm.rockList[rockCurrent].rock.GetComponent<Rock_Flick>();
        rockRB = gm.rockList[rockCurrent].rock.GetComponent<Rigidbody2D>();

        switch (target)
        {
            case "Guard Reading":
                StartCoroutine(GuardReading(rockCurrent));
                break;

            case "Auto Take Out":
                StartCoroutine(TakeOutAutoTarget(rockCurrent));
                break;

            case "Manual Take Out":
                StartCoroutine(TakeOutManualTarget(rockCurrent));
                break;

            case "Take Out":
                StartCoroutine(TakeOutTarget(rockCurrent, rockTarget));
                break;

            case "Manual Peel":
                StartCoroutine(PeelManualTarget(rockCurrent));
                break;

            case "Peel":
                StartCoroutine(PeelTarget(rockCurrent, rockTarget));
                break;

            case "Manual Tap Back":
                StartCoroutine(TapManualTarget(rockCurrent));
                break;

            case "Tap Back":
                StartCoroutine(TapTarget(rockCurrent, rockTarget));
                break;

            case "Manual Tick Shot":
                StartCoroutine(TickShotManualTarget(rockCurrent));
                break;

            case "Tick Shot":
                StartCoroutine(TickShotTarget(rockCurrent, rockTarget));
                break;

            case "Auto Draw Twelve Foot":
                StartCoroutine(DrawTwelveFoot(rockCurrent));
                break;

            case "Auto Draw Four Foot":
                StartCoroutine(DrawFourFoot(rockCurrent));
                break;
        }
    }

    

    IEnumerator GuardReading(int rockCurrent)
    {
        //if there's guards
        if (gm.gList.Count != 0)
        {
            //for each item in guard list
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
                // left corner 
                else if (posX < -0.4f && posX > -1.25f)
                {
                    lCornGuard = guard.lastTransform;
                    Debug.Log("Left Guard - " + guard.lastTransform.position.x + ", " + guard.lastTransform.position.y);
                }
                // right corner
                else if (posX > 0.4f && posX < 1.25f)
                {
                    rCornGuard = guard.lastTransform;
                    Debug.Log("Right Guard - " + guard.lastTransform.position.x + ", " + guard.lastTransform.position.y);
                }

                else
                {
                    tCenGuard = null;
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

    IEnumerator TakeOutManualTarget(int rockCurrent)
    {
        targetX = targetPos.x;
        targetY = targetPos.y;

        if (rm.inturn == false)
        {

            takeOutX = (-0.205f * ((targetX + 1.35f) / 2.7f)) + 0.087f;
        }
        else
        {
            takeOutX = (-0.19f * ((targetX + 1.35f) / 2.7f)) + 0.11f;
        }

        aiShoot.OnShot("Take Out", rockCurrent);
        yield break;
    }

    IEnumerator TakeOutAutoTarget(int rockCurrent)
    {
        yield return StartCoroutine(GuardReading(rockCurrent));
        yield return new WaitForEndOfFrame();
        #region House Has Rocks
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
                        aiShoot.OnShot("Take Out", rockCurrent);
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
                            aiShoot.OnShot("Take Out", rockCurrent);
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
                            aiShoot.OnShot("Peel", rockCurrent);
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
                        aiShoot.OnShot("Raise", rockCurrent);
                        Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                        yield break;
                    }
                    //if the closest rock is to the right and there's a right guard
                    else if (rCornGuard & closestRock.transform.position.x > 0f)
                    {
                        targetX = rCornGuard.position.x;
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
                        aiShoot.OnShot("Peel", rockCurrent);
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
                        aiShoot.OnShot("Peel", rockCurrent);
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
                        aiShoot.OnShot("Take Out", rockCurrent);
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
                            aiShoot.OnShot("Left Centre Guard", rockCurrent);
                            Debug.Log("Centre Guard");
                            yield break;
                        }
                        else if (gm.houseList[1].rock.transform.position.x < 0f & !lCornGuard)
                        {
                            aiShoot.OnShot("Left Corner Guard", rockCurrent);
                            Debug.Log("Left Corner Guard");
                            yield break;
                        }
                        else if (gm.houseList[1].rock.transform.position.x > 0f & !rCornGuard)
                        {
                            aiShoot.OnShot("Right Corner Guard", rockCurrent);
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
                            aiShoot.OnShot("Peel", rockCurrent);
                            Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                            yield break;
                        }
                        else if (gm.houseList[1].rock.transform.position.x < 0f && lCornGuard)
                        {
                            targetX = lCornGuard.position.x;
                            takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                            aiShoot.OnShot("Peel", rockCurrent);
                            Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                            yield break;
                        }
                        else if (gm.houseList[1].rock.transform.position.x > 0f && rCornGuard)
                        {
                            targetX = rCornGuard.position.x;
                            takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                            aiShoot.OnShot("Peel", rockCurrent);
                            Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                            yield break;
                        }
                        //if the second rock is not guarded
                        else
                        {
                            targetX = gm.houseList[1].rock.transform.position.x;
                            takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                            aiShoot.OnShot("Take Out", rockCurrent);
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
                        aiShoot.OnShot("Left Centre Guard", rockCurrent);
                        Debug.Log("Centre Guard");
                        yield break;
                    }
                    else if (closestRock.transform.position.x < 0f & !lCornGuard)
                    {
                        aiShoot.OnShot("Left Corner Guard", rockCurrent);
                        Debug.Log("Left Corner Guard");
                        yield break;
                    }
                    else if (closestRock.transform.position.x > 0f & !rCornGuard)
                    {
                        aiShoot.OnShot("Right Corner Guard", rockCurrent);
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
        #endregion

        #region No rocks in House, but Guards
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
                    aiShoot.OnShot("Raise", rockCurrent);
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //right corner guard is not mine
                else if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                    aiShoot.OnShot("Raise", rockCurrent);
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //left corner guard is not mine
                else if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
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
                    aiShoot.OnShot("Raise", rockCurrent);
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //guard is not mine
                else
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
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
                    aiShoot.OnShot("Raise", rockCurrent);
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //guard is not mine
                else
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
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
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //right guard is not mine
                else if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //left guard is mine
                else if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Raise", rockCurrent);
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //right guard is mine
                else if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Raise", rockCurrent);
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else
                {
                    targetX = 0f;
                    takeOutX = 0f;
                    aiShoot.OnShot("Peel", rockCurrent);
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
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //right guard is not mine
                else if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //centre guard is mine
                else if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Raise", rockCurrent);
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //right guard is mine
                else if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Raise", rockCurrent);
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else
                {
                    targetX = 0f;
                    takeOutX = 0f;
                    aiShoot.OnShot("Peel", rockCurrent);
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
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //centre guard is not mine
                else if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //left guard is mine
                else if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //centre guard is mine
                else if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else
                {
                    aiShoot.OnShot("Button", rockCurrent);
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
                    aiShoot.OnShot("Raise", rockCurrent);
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //if it's theirs
                else
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
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
        #endregion

        #region No guards or rocks in House
        else
        {
            targetX = 0f;
            takeOutX = 0f;
            aiShoot.OnShot("Peel", rockCurrent);
            Debug.Log("No Targets available - Button");
            yield break;
        }
        #endregion

    }

    IEnumerator TakeOutTarget(int rockCurrent, int rockTarget)
    {
        yield return StartCoroutine(GuardReading(rockCurrent));

        targetX = gm.rockList[rockTarget].rock.transform.position.x;
        targetY = gm.rockList[rockTarget].rock.transform.position.y;


        //rm.inturn = false;
        //takeOutX = (-0.205f * ((targetX + 1.35f) / 2.7f)) + 0.087f;

        //rm.inturn = true;
        //takeOutX = (-0.19f * ((targetX + 1.35f) / 2.7f)) + 0.11f;

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

        aiShoot.OnShot("Take Out", rockCurrent);
        Debug.Log(gm.rockList[rockTarget].rockInfo.teamName + " " + gm.rockList[rockTarget].rockInfo.rockNumber);
        yield break;
    }

    IEnumerator PeelManualTarget(int rockCurrent)
    {
        targetX = targetPos.x;
        targetY = targetPos.y;


        //rm.inturn = false;
        //takeOutX = (-0.205f * ((targetX + 1.35f) / 2.7f)) + 0.087f;

        //rm.inturn = true;
        //takeOutX = (-0.19f * ((targetX + 1.35f) / 2.7f)) + 0.11f;

        if (rm.inturn == false)
        {
            takeOutX = (-0.222f * ((targetX + 1.35f) / 2.7f)) + 0.102f;
        }
        else
        {
            takeOutX = (-0.219f * ((targetX + 1.35f) / 2.7f)) + 0.122f;
        }

        aiShoot.OnShot("Peel", rockCurrent);
        yield break;
    }

    IEnumerator PeelTarget(int rockCurrent, int rockTarget)
    {
        yield return StartCoroutine(GuardReading(rockCurrent));

        targetX = gm.rockList[rockTarget].rock.transform.position.x;
        targetY = gm.rockList[rockTarget].rock.transform.position.y;


        //rm.inturn = false;
        //takeOutX = (-0.205f * ((targetX + 1.35f) / 2.7f)) + 0.087f;

        //rm.inturn = true;
        //takeOutX = (-0.19f * ((targetX + 1.35f) / 2.7f)) + 0.11f;

        if (targetX > 0f)
        {
            rm.inturn = false;
            takeOutX = (-0.222f * ((targetX + 1.35f) / 2.7f)) + 0.102f;
        }
        else
        {
            rm.inturn = true;
            takeOutX = (-0.219f * ((targetX + 1.35f) / 2.7f)) + 0.122f;
        }

        aiShoot.OnShot("Peel", rockCurrent);
        Debug.Log(gm.rockList[rockTarget].rockInfo.teamName + " " + gm.rockList[rockTarget].rockInfo.rockNumber);
        yield break;
    }

    IEnumerator TapManualTarget(int rockCurrent)
    {
        targetX = targetPos.x;
        targetY = targetPos.y;

        if (rm.inturn == false)
        {
            takeOutX = (-0.178f * ((targetX + 1.35f) / 2.7f)) + 0.056f;
        }
        else
        {
            takeOutX = (-0.18f * ((targetX + 1.35f) / 2.7f)) + 0.12f;
        }

        aiShoot.OnShot("Raise", rockCurrent);
        yield break;
    }

    IEnumerator TapTarget(int rockCurrent, int rockTarget)
    {
        yield return StartCoroutine(GuardReading(rockCurrent));

        targetX = gm.rockList[rockTarget].rock.transform.position.x;
        targetY = gm.rockList[rockTarget].rock.transform.position.y;


        //rm.inturn = false;
        //takeOutX = (-0.205f * ((targetX + 1.35f) / 2.7f)) + 0.087f;

        //rm.inturn = true;
        //takeOutX = (-0.19f * ((targetX + 1.35f) / 2.7f)) + 0.11f;

        if (targetX > 0f)
        {
            rm.inturn = false;
            takeOutX = (-0.178f * ((targetX + 1.35f) / 2.7f)) + 0.056f;
        }
        else
        {
            rm.inturn = true;
            takeOutX = (-0.18f * ((targetX + 1.35f) / 2.7f)) + 0.12f;
        }

        aiShoot.OnShot("Raise", rockCurrent);
        Debug.Log(gm.rockList[rockTarget].rockInfo.teamName + " " + gm.rockList[rockTarget].rockInfo.rockNumber);
        yield break;
    }


    IEnumerator TickShotManualTarget(int rockCurrent)
    {
        targetX = targetPos.x;
        targetY = targetPos.y;

        if (rm.inturn == false)
        {
            takeOutX = (-0.04f * ((targetX + 0.4f) / 0.8f)) - 0.005f;
        }
        else
        {
            takeOutX = (-0.039f * ((targetX + 0.4f) / 0.8f)) + 0.042f;
        }

        aiShoot.OnShot("Tick", rockCurrent);
        yield break;
    }

    IEnumerator TickShotTarget(int rockCurrent, int rockTarget)
    {
        yield return StartCoroutine(GuardReading(rockCurrent));

        targetX = gm.rockList[rockTarget].rock.transform.position.x;
        targetY = gm.rockList[rockTarget].rock.transform.position.y;


        //rm.inturn = false;
        //takeOutX = (-0.205f * ((targetX + 1.35f) / 2.7f)) + 0.087f;

        //rm.inturn = true;
        //takeOutX = (-0.19f * ((targetX + 1.35f) / 2.7f)) + 0.11f;

        if (targetX > 0f)
        {
            rm.inturn = false;
            takeOutX = (-0.04f * ((targetX + 0.4f) / 0.8f)) - 0.005f;
        }
        else
        {
            rm.inturn = true;
            takeOutX = (-0.039f * ((targetX + 0.4f) / 0.8f)) + 0.042f;
        }

        aiShoot.OnShot("Tick", rockCurrent);
        Debug.Log(gm.rockList[rockTarget].rockInfo.teamName + " " + gm.rockList[rockTarget].rockInfo.rockNumber);
        yield break;
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
                    aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                    yield break;
                }
                //centre guard to the left
                else if (cenGuard.position.x < 0f)
                {
                    rm.inturn = false;
                    aiShoot.OnShot("Top Twelve Foot", rockCurrent);
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
                        aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                        yield break;
                    }
                    //centre guard to the left
                    else if (cenGuard.position.x < 0f)
                    {
                        rm.inturn = false;
                        aiShoot.OnShot("Top Twelve Foot", rockCurrent);
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
                            aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                            yield break;
                        }
                        //centre guard to the left
                        else if (cenGuard.position.x < 0f)
                        {
                            rm.inturn = false;
                            aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                            yield break;
                        }
                    }
                    //left corner guard is high
                    else if (lCornGuard.position.y < 2.0f)
                    {
                        rm.inturn = false;
                        aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                        yield break;
                    }
                    //right corner guard is high
                    else if (rCornGuard.position.y < 2.0f)
                    {
                        rm.inturn = true;
                        aiShoot.OnShot("Top Twelve Foot", rockCurrent);
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
                            aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                            yield break;
                        }
                        //centre guard to the left
                        else if (cenGuard.position.x < 0f)
                        {
                            rm.inturn = false;
                            aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                            yield break;
                        }
                    }
                    //left corner guard is higher
                    else if (lCornGuard.position.y < cenGuard.position.y)
                    {
                        rm.inturn = false;
                        aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                        yield break;
                    }
                    //right corner guard is higher
                    else if (rCornGuard.position.y < cenGuard.position.y)
                    {
                        rm.inturn = true;
                        aiShoot.OnShot("Top Twelve Foot", rockCurrent);
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
                        aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                        yield break;
                    }
                    //centre guard to the left
                    else if (cenGuard.position.x < 0f)
                    {
                        rm.inturn = false;
                        aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                        yield break;
                    }
                }
            }

            //centre guard and a left guard
            else if (cenGuard && lCornGuard && !rCornGuard)
            {
                    rm.inturn = false;
                    aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                    yield break;
            }

            //centre guard and a right guard
            else if (cenGuard && rCornGuard && !lCornGuard)
            {
                if (cenGuard.position.x > 0f)
                {
                    rm.inturn = true;
                    aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                    yield break;
                }
                else if (cenGuard.position.x < 0f)
                {
                    rm.inturn = true;
                    aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                    yield break;
                }
            }

            //right and a left guard
            else if (rCornGuard && lCornGuard && !cenGuard)
            {
                if (rCornGuard.position.y < lCornGuard.position.y)
                {
                    rm.inturn = true;
                    aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                    yield break;
                }
                else if (lCornGuard.position.y < rCornGuard.position.y)
                {
                    rm.inturn = false;
                    aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                    yield break;
                }
            }

            //right corner guard
            else if (rCornGuard && !lCornGuard && !cenGuard)
            {
                rm.inturn = true;
                aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                yield break;
            }

            //left corner guard
            else if (lCornGuard && !rCornGuard && !cenGuard)
            {
                rm.inturn = false;
                aiShoot.OnShot("Left Twelve Foot", rockCurrent);
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

            aiShoot.OnShot("Top Twelve Foot", rockCurrent);
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
                    aiShoot.OnShot("Top Four Foot", rockCurrent);
                    yield break;
                }
                //centre guard to the left
                else
                {
                    rm.inturn = false;
                    aiShoot.OnShot("Top Four Foot", rockCurrent);
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
                        aiShoot.OnShot("Top Four Foot", rockCurrent);
                        yield break;
                    }
                    //centre guard to the left
                    else
                    {
                        rm.inturn = false;
                        aiShoot.OnShot("Top Four Foot", rockCurrent);
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
                            aiShoot.OnShot("Top Four Foot", rockCurrent);
                            yield break;
                        }
                        //centre guard to the left
                        else
                        {
                            rm.inturn = false;
                            aiShoot.OnShot("Top Four Foot", rockCurrent);
                            yield break;
                        }
                    }
                    //left corner guard is high
                    else if (lCornGuard.position.y < 2.0f)
                    {
                        rm.inturn = false;
                        aiShoot.OnShot("Top Four Foot", rockCurrent);
                        yield break;
                    }
                    //right corner guard is high
                    else
                    {
                        rm.inturn = true;
                        aiShoot.OnShot("Top Four Foot", rockCurrent);
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
                            aiShoot.OnShot("Top Four Foot", rockCurrent);
                            yield break;
                        }
                        //centre guard to the left
                        else
                        {
                            rm.inturn = false;
                            aiShoot.OnShot("Top Four Foot", rockCurrent);
                            yield break;
                        }
                    }
                    //left corner guard is higher
                    else if (lCornGuard.position.y < cenGuard.position.y)
                    {
                        rm.inturn = false;
                        aiShoot.OnShot("Top Four Foot", rockCurrent);
                        yield break;
                    }
                    //right corner guard is higher
                    else if (rCornGuard.position.y < cenGuard.position.y)
                    {
                        rm.inturn = true;
                        aiShoot.OnShot("Top Four Foot", rockCurrent);
                        yield break;
                    }
                    else
                    {
                        rm.inturn = false;
                        aiShoot.OnShot("Top Four Foot", rockCurrent);
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
                        aiShoot.OnShot("Top Four Foot", rockCurrent);
                        yield break;
                    }
                    //centre guard to the left
                    else
                    {
                        rm.inturn = false;
                        aiShoot.OnShot("Top Four Foot", rockCurrent);
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
                    aiShoot.OnShot("Top Four Foot", rockCurrent);
                    yield break;
                }
                else
                {
                    rm.inturn = false;
                    aiShoot.OnShot("Left Four Foot", rockCurrent);
                    yield break;
                }
            }

            //centre guard and a right guard
            else if (cenGuard && rCornGuard && !lCornGuard)
            {
                if (cenGuard.position.x > 0f)
                {
                    rm.inturn = true;
                    aiShoot.OnShot("Right Four Foot", rockCurrent);
                    yield break;
                }
                else
                {
                    rm.inturn = true;
                    aiShoot.OnShot("Top Four Foot", rockCurrent);
                    yield break;
                }
            }

            //right and a left guard
            else if (rCornGuard && lCornGuard && !cenGuard)
            {
                if (rCornGuard.position.y < lCornGuard.position.y)
                {
                    rm.inturn = true;
                    aiShoot.OnShot("Right Four Foot", rockCurrent);
                    yield break;
                }
                else
                {
                    rm.inturn = false;
                    aiShoot.OnShot("Left Four Foot", rockCurrent);
                    yield break;
                }
            }

            //right corner guard
            else if (rCornGuard && !lCornGuard && !cenGuard)
            {
                rm.inturn = true;
                aiShoot.OnShot("Right Four Foot", rockCurrent);
                yield break;
            }

            //left corner guard
            else
            {
                rm.inturn = false;
                aiShoot.OnShot("Left Four Foot", rockCurrent);
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
                aiShoot.OnShot("Top Four Foot", rockCurrent);
            }
            else
            {
                aiShoot.OnShot("Button", rockCurrent);
            }
            yield break;
        }
    }
}
