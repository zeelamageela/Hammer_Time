using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Strategy : MonoBehaviour
{
    public GameManager gm;
    public TutorialManager tm;
    public RockManager rm;

    public AIManager aim;
    public AI_Shooter aiShoot;
    public AI_Target aiTarg;

    Rock_Info rockInfo;
    Rock_Flick rockFlick;
    Rigidbody2D rockRB;


    public Transform cenGuard;
    public Transform tCenGuard;
    public Transform lCornGuard;
    public Transform rCornGuard;

    public float takeOutOffset;
    public float peelOffset;
    public float raiseOffset;
    public float tickOffset;

    public float takeOutX;

    public float osMult;
    GameObject closestRock;
    Rock_Info closestRockInfo;

    string phase;

    private void Update()
    {
        cenGuard = aiTarg.cenGuard;
        tCenGuard = aiTarg.tCenGuard;
        lCornGuard = aiTarg.lCornGuard;
        rCornGuard = aiTarg.rCornGuard;
    }

    public void OnShot(int rockCurrent)
    {
        if (gm.houseList.Count != 0)
        {
            closestRock = gm.houseList[0].rock;
            closestRockInfo = gm.houseList[0].rockInfo;
        }

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
        else if (gm.rockTotal - rockCurrent <= 12 && gm.rockTotal - rockCurrent >= 7)
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
        else if (gm.rockTotal - rockCurrent <= 4)
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

        if (gm.redHammer)
        {
            if (gm.endTotal - gm.endCurrent >= 2)
            {
                if (gm.redScore - gm.yellowScore >= 2)
                    AggressiveNotHammer(rockCurrent, phase);
                if (gm.redScore <= gm.yellowScore)
                    ConservativeStealOrBlank(rockCurrent, phase);
                else
                    ConservativeSteal(rockCurrent, phase);
            }
            else if (gm.endTotal - gm.endCurrent == 1)
            {
                if (gm.redScore - gm.yellowScore <= 1)
                    ConservativeStealOrBlank(rockCurrent, phase);
                else
                    AggressiveNotHammer(rockCurrent, phase);
            }
            else if (gm.redScore < gm.yellowScore)
                ConservativeStealOrBlank(rockCurrent, phase);
            else
                AggressiveNotHammer(rockCurrent, phase);
        }
        else
        {
            if (gm.endTotal - gm.endCurrent >= 2)
            {
                if (gm.redScore - gm.yellowScore >= 2)
                    AggressiveHammer(rockCurrent, phase);
                if (gm.redScore <= gm.yellowScore)
                    ConservativeScoreTwoOrBlankHammer(rockCurrent, phase);
                else
                    ConservativeSteal(rockCurrent, phase);
            }
            else if (gm.endTotal - gm.endCurrent == 1)
            {
                if (gm.redScore - gm.yellowScore <= 1)
                    ConservativeScoreTwoOrBlankHammer(rockCurrent, phase);
                else
                    AggressiveHammer(rockCurrent, phase);
            }
            else
            {
                if (gm.redScore < gm.yellowScore)
                    ConservativeScoreTwoOrBlankHammer(rockCurrent, phase);
                else
                    AggressiveHammer(rockCurrent, phase);
            }
                
        }
        Debug.Log("Phase is " + phase);
    }

    public void ConservativeSteal(int rockCurrent, string phase)
    {
        GameObject rock = gm.rockList[rockCurrent].rock;
        Rock_Info rockInfo = gm.rockList[rockCurrent].rockInfo;

        if (gm.houseList.Count != 0)
        {
            closestRock = gm.houseList[0].rock;
            closestRockInfo = gm.houseList[0].rockInfo;
        }

        Debug.Log("Phase is " + phase);

        aiTarg.OnTarget("Guard Reading", rockCurrent, 0);

        switch (phase)
        {
            #region Early Not Hammer
            case "early hammer":
                //if there's rocks in the house
                if (gm.houseList.Count > 0)
                {
                    //if I have shot rock
                    if (closestRockInfo.teamName == rockInfo.teamName)
                    {
                        aiShoot.OnShot("Centre Guard", rockCurrent);
                    }
                    //if they have shot rock
                    else
                    {
                        //if it's in the centre
                        if (Mathf.Abs(closestRock.transform.position.x) <= 0.5f)
                        {
                            if (cenGuard | tCenGuard)
                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            else
                                aiTarg.OnTarget("Tap Back", rockCurrent, closestRockInfo.rockIndex);
                        }
                        //if it's on the wings and it's above the tee line
                        else if (closestRock.transform.position.y <= 6.5f)
                            aiTarg.OnTarget("Tap Back", rockCurrent, closestRockInfo.rockIndex);
                        else
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }
                }
                //if there's guards
                else if (gm.gList.Count != 0)
                {
                    if (cenGuard | tCenGuard)
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    else
                        aiShoot.OnShot("Centre Guard", rockCurrent);
                }
                else
                    aiShoot.OnShot("Centre Guard", rockCurrent);

                break;
            #endregion

            #region Middle Not Hammer
            case "middle hammer":

                if (gm.houseList.Count != 0)
                {
                    //if I have shot rock
                    if (closestRockInfo.teamName == rockInfo.teamName)
                    {
                        if (gm.houseList.Count > 1)
                        {
                            if (gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                            {
                                if (Mathf.Abs(gm.houseList[1].rock.transform.position.x) <= 0.5f)
                                {
                                    if (cenGuard | tCenGuard)
                                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                    else
                                        aiShoot.OnShot("Centre Guard", rockCurrent);
                                }
                                else if (gm.houseList[1].rock.transform.position.x < 0)
                                {
                                    if (lCornGuard)
                                        aiTarg.OnTarget("Peel", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                    else
                                        aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                                }
                                else
                                {
                                    if (rCornGuard)
                                        aiTarg.OnTarget("Peel", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                    else
                                        aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                                }
                            }
                            else
                            {
                                if (cenGuard | tCenGuard)
                                {
                                    if (lCornGuard)
                                        aiTarg.OnTarget("Peel", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                    else if (rCornGuard)
                                        aiTarg.OnTarget("Peel", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                    else
                                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                }
                                else
                                    aiShoot.OnShot("Centre Guard", rockCurrent);
                            }
                        }
                        else
                        {
                            if (cenGuard | tCenGuard)
                            {
                                if (lCornGuard)
                                    aiTarg.OnTarget("Peel", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                else if (rCornGuard)
                                    aiTarg.OnTarget("Peel", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                else
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            }
                            else
                                aiShoot.OnShot("Centre Guard", rockCurrent);
                        }
                    }
                    //if they have shot rock
                    else
                    {
                        //if it's in the centre
                        if (Mathf.Abs(closestRock.transform.position.x) <= 0.5f)
                        {
                            if (cenGuard | tCenGuard)
                            {
                                if (Mathf.Abs(cenGuard.position.x - closestRock.transform.position.x) <= 0.1f | Mathf.Abs(cenGuard.position.x - closestRock.transform.position.x) <= 0.1f)
                                {
                                    if (gm.houseList.Count > 1)
                                    {
                                        if (gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                                        {
                                            if (Mathf.Abs(cenGuard.position.x - gm.houseList[1].rock.transform.position.x) <= 0.1f | Mathf.Abs(cenGuard.position.x - gm.houseList[1].rock.transform.position.x) <= 0.1f)
                                            {
                                                if (cenGuard)
                                                    aiTarg.OnTarget("Peel", rockCurrent, cenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                else if (tCenGuard)
                                                    aiTarg.OnTarget("Peel", rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                else
                                                    aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                            }
                                            else
                                            {
                                                if (gm.houseList[1].rock.transform.position.x < 0)
                                                {
                                                    if (lCornGuard)
                                                        aiTarg.OnTarget("Peel", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                    else
                                                        aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                                }
                                                else
                                                {
                                                    if (rCornGuard)
                                                        aiTarg.OnTarget("Peel", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                    else
                                                        aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                                }
                                            }
                                        }
                                        else
                                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                    }
                                    else
                                    {
                                        if (cenGuard)
                                            aiTarg.OnTarget("Peel", rockCurrent, cenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                        else if (tCenGuard)
                                            aiTarg.OnTarget("Peel", rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                        else
                                            aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                    }
                                }
                                else
                                    aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);

                            }  
                            else
                                aiTarg.OnTarget("Tap Back", rockCurrent, closestRockInfo.rockIndex);
                        }
                        //if it's on the wings and it's above the tee line
                        else if (closestRock.transform.position.y <= 6.5f)
                        {
                            if (closestRock.transform.position.x < 0)
                            {
                                if (lCornGuard)
                                    aiTarg.OnTarget("Peel", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                else
                                    aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                            }
                            else
                            {
                                if (rCornGuard)
                                    aiTarg.OnTarget("Peel", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                else
                                    aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                            }
                        }
                        else
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }
                }
                //if there's guards
                else if (gm.gList.Count != 0)
                {
                    if (rCornGuard)
                        aiTarg.OnTarget("Peel", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                    else if (lCornGuard)
                        aiTarg.OnTarget("Peel", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                    else
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                }
                else
                    aiShoot.OnShot("Centre Guard", rockCurrent);

                break;
            #endregion

            #region Late Not Hammer
            case "late hammer":
                //there's rocks in the house
                if (gm.houseList.Count != 0)
                {
                    //They have shot rock
                    if (closestRockInfo.teamName != rockInfo.teamName)
                    {
                        if (Mathf.Abs(closestRock.transform.position.x) <= 0.5f && cenGuard | tCenGuard)
                        {
                            if (cenGuard)
                            {
                                if (Mathf.Abs(cenGuard.position.x - closestRock.transform.position.x) > 0.1f)
                                    aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                                else
                                    aiTarg.OnTarget("Peel", rockCurrent, cenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                            }
                            //there's a tight centre guard
                            else if (tCenGuard)
                            {
                                if (Mathf.Abs(tCenGuard.position.x - closestRock.transform.position.x) > 0.1f)
                                    aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                                else
                                    aiTarg.OnTarget("Peel", rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                            }
                        }
                        //there's a centre guard

                        //it's on the left wing
                        else if (closestRock.transform.position.x < 0 & lCornGuard)
                        {
                            if (Mathf.Abs(lCornGuard.position.x - closestRock.transform.position.x) > 0.1f)
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            else
                                aiTarg.OnTarget("Peel", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                        }
                        //it's on the right wing
                        else if (closestRock.transform.position.x > 0 & rCornGuard)
                        {
                            if (Mathf.Abs(rCornGuard.position.x - closestRock.transform.position.x) > 0.1f)
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            else
                                aiTarg.OnTarget("Peel", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                        }
                        else
                            aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                    }
                    //I have shot rock
                    else if (closestRockInfo.teamName == rockInfo.teamName)
                    {
                        //more than one rock in house
                        if (gm.houseList.Count > 1)
                        {
                            //I have second shot
                            if (gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                            {
                                //second shot is to the left
                                if (gm.houseList[1].rock.transform.position.x <= 0f)
                                    aiShoot.OnShot("Left Corner Guard", rockCurrent);
                                else if (gm.houseList[1].rock.transform.position.x > 0f)
                                    aiShoot.OnShot("Right Corner Guard", rockCurrent);
                                else if (cenGuard | tCenGuard)
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                else
                                    aiShoot.OnShot("Centre Guard", rockCurrent);
                            }
                            else
                            {
                                //second shot is in the middle
                                if (Mathf.Abs(gm.houseList[1].rock.transform.position.x) <= 0.5f)
                                {
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                }
                                //second shot is not in the middle
                                else
                                {
                                    //second shot is to the left
                                    if (gm.houseList[1].rock.transform.position.x < 0)
                                    {
                                        if (lCornGuard)
                                            aiTarg.OnTarget("Peel", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                        else
                                            aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                    }
                                    //second shot is to the right
                                    else
                                    {
                                        if (rCornGuard)
                                            aiTarg.OnTarget("Peel", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                        else
                                            aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                    }
                                }

                            }
                        }
                        //only one rock in house
                        else
                        {
                            if (Mathf.Abs(closestRock.transform.position.x) <= 0.5f)
                            {
                                if (cenGuard | tCenGuard)
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                else
                                    aiShoot.OnShot("Centre Guard", rockCurrent);
                            }
                            else
                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                    }
                }
                else
                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);

                break;
            #endregion

            default:
                break;
        }
    }

    public void AggressiveHammer(int rockCurrent, string phase)
    {
        //Aggressive is to steal at all costs
        GameObject rock = gm.rockList[rockCurrent].rock;
        Rock_Info rockInfo = gm.rockList[rockCurrent].rockInfo;

        if (gm.houseList.Count != 0)
        {
            closestRock = gm.houseList[0].rock;
            closestRockInfo = gm.houseList[0].rockInfo;
        }

        Debug.Log("Phase is " + phase);

        aiTarg.OnTarget("Guard Reading", rockCurrent, 0);

        switch (phase)
        {
            #region Early Hammer
            case "early hammer":

                //left corner guard
                if (lCornGuard)
                {
                    aiShoot.OnShot("Right Corner Guard", rockCurrent);
                }
                //right corner guard
                else if (rCornGuard)
                {
                    aiShoot.OnShot("Left Corner Guard", rockCurrent);
                }
                else if (cenGuard)
                {
                    aiTarg.OnTarget("Tick Shot", rockCurrent, cenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                }
                else if (tCenGuard)
                {
                    aiTarg.OnTarget("Tick Shot", rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                }
                //left and right corner guard
                else if (rCornGuard & lCornGuard)
                {
                    aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                }
                else
                {
                    if (Random.value > 0.5f)
                        aiShoot.OnShot("Left Corner Guard", rockCurrent);
                    else
                        aiShoot.OnShot("Right Corner Guard", rockCurrent);
                }

                break;
            #endregion

            #region Middle Hammer
            case "middle hammer":

                //no one is in the house
                if (gm.houseList.Count == 0)
                {
                    //tight centre and no centre guard
                    if (lCornGuard && !rCornGuard)
                    {
                        aiShoot.OnShot("Right Corner Guard", rockCurrent);
                    }
                    //centre and no tight centre guard
                    else if (rCornGuard && !lCornGuard)
                    {
                        aiShoot.OnShot("Left Corner Guard", rockCurrent);
                    }
                    //centre and tight centre guards
                    else if (lCornGuard & rCornGuard)
                    {
                        aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                    }
                    else
                    {
                        if (Random.value > 0.5f)
                            aiShoot.OnShot("Left Corner Guard", rockCurrent);
                        else
                            aiShoot.OnShot("Right Corner Guard", rockCurrent);
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
                            aiShoot.OnShot("Left High Corner Guard", rockCurrent);
                        }
                        else
                        {
                            rm.inturn = false;
                            aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                        }
                    }
                    //right guard only
                    else if (rCornGuard && !lCornGuard)
                    {
                        //the rock is not guarded
                        if (Mathf.Abs(closestRock.transform.position.x - rCornGuard.position.x) >= 0.1f)
                        {
                            aiShoot.OnShot("Right High Corner Guard", rockCurrent);
                        }
                        else
                        {
                            rm.inturn = true;
                            aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                        }
                    }
                    //left and right corner guards
                    else if (rCornGuard && lCornGuard)
                    {
                        //the rock is not guarded on the left
                        if (Mathf.Abs(closestRock.transform.position.x - lCornGuard.position.x) >= 0.1f)
                        {
                            aiShoot.OnShot("Right High Corner Guard", rockCurrent);
                        }
                        //the rock is not guarded on the right
                        else if (Mathf.Abs(closestRock.transform.position.x - rCornGuard.position.x) >= 0.1f)
                        {
                            aiShoot.OnShot("Left High Corner Guard", rockCurrent);
                        }
                        else
                        {
                            if (closestRock.transform.position.x < 0)
                            {
                                rm.inturn = true;
                                aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                            }
                            else if (closestRock.transform.position.x > 0)
                            {
                                rm.inturn = false;
                                aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                            }
                            else aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                        }
                    }
                    //any other guard combo
                    else aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
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
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            }
                            else
                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        //centre guard
                        if (cenGuard)
                        {
                            //if the tight centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f)
                            {
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            }
                            else
                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else
                        {
                            aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                        }
                    }
                    //if it's not in the four foot and there's more than one rock in the house
                    else if (gm.houseList.Count > 1)
                    {
                        //if the second shot is mine
                        if (gm.houseList[1].rockInfo.teamName == closestRockInfo.teamName)
                        {

                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else
                        {
                            aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                        }
                    }
                    //not in the four foot and the only rock
                    else
                    {
                        aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                    }
                }
                break;
            #endregion

            #region Late Hammer
            case "late hammer":

                //no one is in the house
                if (gm.houseList.Count == 0)
                {
                    if (gm.rockTotal - rockCurrent > 1)
                    {
                        //left corner and no right corner guard
                        if (lCornGuard && !rCornGuard)
                        {
                            aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                        }
                        //right corner and no left corner guard
                        else if (rCornGuard && !lCornGuard)
                        {
                            aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                        }
                        //right corner and left corner guards
                        else if (lCornGuard & rCornGuard)
                        {
                            aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                        }
                        else
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                    }
                    else
                    {
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
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
                                aiShoot.OnShot("Left High Corner Guard", rockCurrent);
                            }
                            else
                            {
                                rm.inturn = false;
                                aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                            }
                        }
                        //right guard only
                        else if (rCornGuard && !lCornGuard)
                        {
                            //the rock is not guarded
                            if (Mathf.Abs(closestRock.transform.position.x - rCornGuard.position.x) >= 0.1f)
                            {
                                aiShoot.OnShot("Right High Corner Guard", rockCurrent);
                            }
                            else
                            {
                                rm.inturn = true;
                                aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                            }
                        }
                        //left and right corner guards
                        else if (rCornGuard && lCornGuard)
                        {
                            //the rock is not guarded on the left
                            if (Mathf.Abs(closestRock.transform.position.x - lCornGuard.position.x) >= 0.1f)
                            {
                                aiShoot.OnShot("Right High Corner Guard", rockCurrent);
                            }
                            //the rock is not guarded on the right
                            else if (Mathf.Abs(closestRock.transform.position.x - rCornGuard.position.x) >= 0.1f)
                            {
                                aiShoot.OnShot("High Left Corner Guard", rockCurrent);
                            }
                            else
                            {
                                if (closestRock.transform.position.x < 0)
                                {
                                    rm.inturn = true;
                                    aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                                }
                                else if (closestRock.transform.position.x > 0)
                                {
                                    rm.inturn = false;
                                    aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                                }
                                else aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                            }
                        }
                        //any other guard combo
                        else aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, rockCurrent);
                    }
                    else
                    {
                        //if there's more than one rock in the house
                        if (gm.houseList.Count > 1)
                        {
                            //if the second shot is mine
                            if (gm.houseList[1].rockInfo.teamName == closestRockInfo.teamName)
                            {

                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            }
                            else
                            {
                                aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                            }
                        }
                        else
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
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
                                aiTarg.OnTarget("Tap Back", rockCurrent, closestRockInfo.rockIndex);
                            }
                            else
                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        //centre guard
                        if (cenGuard)
                        {
                            //if the tight centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f)
                            {
                                aiTarg.OnTarget("Tap Back", rockCurrent, closestRockInfo.rockIndex);
                            }
                            else
                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else
                        {
                            aiTarg.OnTarget("Tap Back", rockCurrent, closestRockInfo.rockIndex);
                        }
                    }
                    //if it's not in the four foot and there's more than one rock in the house
                    else if (gm.houseList.Count > 1)
                    {
                        //if the second shot is mine
                        if (gm.houseList[1].rockInfo.teamName == closestRockInfo.teamName)
                        {

                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else
                        {
                            aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                        }
                    }
                    //not in the four foot and the only rock
                    else
                    {
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }

                }
                break;
            #endregion

            default:
                break;
        }
    }

    public void ConservativeScoreTwoOrBlankHammer(int rockCurrent, string phase)
    {
        GameObject rock = gm.rockList[rockCurrent].rock;
        Rock_Info rockInfo = gm.rockList[rockCurrent].rockInfo;

        if (gm.houseList.Count != 0)
        {
            closestRock = gm.houseList[0].rock;
            closestRockInfo = gm.houseList[0].rockInfo;
        }

        Debug.Log("Phase is " + phase);

        aiTarg.OnTarget("Guard Reading", rockCurrent, 0);

        switch (phase)
        {
            #region Early Hammer
            case "early hammer":
                //if there's rocks in the house
                if (gm.houseList.Count != 0)
                {
                    //if they are shot rock
                    if (closestRockInfo.teamName != rockInfo.teamName)
                    {
                        //take it out
                        aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                    }
                    else
                    {
                        //if the rock is to the left
                        if (closestRock.transform.position.x <= 0f)
                        {
                            aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                        }
                        else
                        {
                            aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                        }
                    }
                }
                else
                {
                    if (Random.value > 0.5f)
                        aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                    else
                        aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                }

                break;
            #endregion

            #region Middle Hammer
            case "middle hammer":

                if (gm.houseList.Count != 0)
                {
                    if (closestRockInfo.teamName != rockInfo.teamName)
                    {

                        aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                    }
                    else if (closestRockInfo.teamName == rockInfo.teamName)
                    {
                        if (gm.houseList.Count > 1)
                        {
                            if (gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                            {
                                if (gm.houseList[1].rock.transform.position.x > 0.5f && rCornGuard)
                                {
                                    aiTarg.OnTarget("Take Out", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                }
                                else if (gm.houseList[1].rock.transform.position.x < -0.5f && lCornGuard)
                                {
                                    aiTarg.OnTarget("Take Out", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                }
                                else
                                    aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                            }
                            else
                            {
                                if (cenGuard)
                                    aiShoot.OnShot("High Centre Guard", rockCurrent);
                                else if (tCenGuard)
                                    aiShoot.OnShot("Centre Guard", rockCurrent);
                                else
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            }
                        }
                        else
                        {
                            if (closestRock.transform.position.y < 6.6f)
                            {
                                if (closestRock.transform.position.x > 0f)
                                    aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                                else
                                    aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                            }
                            else
                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                    }
                }
                else
                {
                    if (rCornGuard)
                    {
                        aiTarg.OnTarget("Take Out", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                    }
                    else if (lCornGuard)
                    {
                        aiTarg.OnTarget("Take Out", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                    }
                    else
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                }

                break;
            #endregion

            #region Late Hammer
            case "late hammer":

                if (gm.houseList.Count != 0)
                {
                    if (closestRockInfo.teamName != rockInfo.teamName)
                    {
                        if (cenGuard)
                        {
                            if (Mathf.Abs(cenGuard.position.x - closestRock.transform.position.x) > 0.1f)
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            else
                                aiTarg.OnTarget("Peel", rockCurrent, cenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                        }
                        else if (tCenGuard)
                        {
                            if (Mathf.Abs(tCenGuard.position.x - closestRock.transform.position.x) > 0.1f)
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            else
                                aiTarg.OnTarget("Peel", rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                        }
                        else
                            aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);

                    }
                    else if (closestRockInfo.teamName == rockInfo.teamName)
                    {
                        if (gm.houseList.Count > 1)
                        {
                            if (gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                            {
                                if (closestRock.transform.position.x <= 0f)
                                {
                                    aiShoot.OnShot("Left Corner Guard", rockCurrent);
                                }
                                else
                                {
                                    aiShoot.OnShot("Right Corner Guard", rockCurrent);
                                }
                            }
                            else
                                aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                        }
                        else
                        {
                            if (closestRock.transform.position.x <= 0f)
                            {
                                aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                            }
                            else
                            {
                                aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                            }
                        }
                    }
                }
                else
                {
                    if (gm.rockTotal - gm.rockCurrent <= 1)
                        aiTarg.OnTarget("Take Out", rockCurrent, 0);
                    else if (gm.endTotal - gm.endCurrent <= 1)
                    {
                        if (Random.value > 0.5f)
                            aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                        else
                            aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                    }
                }

                break;
            #endregion

            default:
                break;
        }
    }

    public void AggressiveNotHammer(int rockCurrent, string phase)
    {
        //Aggressive is to steal at all costs
        GameObject rock = gm.rockList[rockCurrent].rock;
        Rock_Info rockInfo = gm.rockList[rockCurrent].rockInfo;

        if (gm.houseList.Count != 0)
        {
            closestRock = gm.houseList[0].rock;
            closestRockInfo = gm.houseList[0].rockInfo;
        }

        Debug.Log("Phase is " + phase);

        aiTarg.OnTarget("Guard Reading", rockCurrent, 0);

        switch (phase)
        {
            #region Early No Hammer
            case "early no hammer":

                //no one is in the house
                if (gm.houseList.Count == 0)
                {
                    //tight centre and no centre guard
                    if (tCenGuard && !cenGuard)
                    {
                        aiShoot.OnShot("Centre Guard", rockCurrent);
                    }
                    //centre and no tight centre guard
                    else if (cenGuard && !tCenGuard)
                    {
                        aiShoot.OnShot("Tight Centre Guard", rockCurrent);
                    }
                    //centre and tight centre guards
                    else if (cenGuard & tCenGuard)
                    {
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, rockCurrent);
                    }
                    else
                    {
                        //randomly choose
                        if (Random.value > 0.5f)
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, rockCurrent);
                        }
                        else aiShoot.OnShot("Tight Centre Guard", rockCurrent);
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
                            aiShoot.OnShot("Tight Centre Guard", rockCurrent);
                        }
                        else
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                    }
                    //centre and no tight centre guard
                    else if (cenGuard && !tCenGuard)
                    {
                        if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                        {
                            aiShoot.OnShot("High Centre Guard", rockCurrent);
                        }
                        else
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                    }
                    //centre and tight centre guards
                    else if (cenGuard & tCenGuard)
                    {
                        if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                        {
                            aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                        }
                        else
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                    }
                    //any other guard combo
                    else
                    {
                        //randomly choose
                        if (Random.value > 0.5f)
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else aiShoot.OnShot("Tight Centre Guard", rockCurrent);
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
                                aiTarg.OnTarget("Auto Take Out", rockCurrent, rockCurrent);
                            }
                            else
                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            aiTarg.OnTarget("Auto Take Out", rockCurrent, rockCurrent);
                        }
                    }
                    else if (gm.houseList.Count >= 2)
                    {
                        if (gm.houseList[1].rockInfo.teamName == closestRockInfo.teamName)
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else
                        {
                            aiTarg.OnTarget("Auto Take Out", rockCurrent, rockCurrent);
                        }
                    }
                    else
                    {
                        aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                    }
                }
                break;
            #endregion

            #region Middle No Hammer
            case "middle no hammer":
                //no one is in the house
                if (gm.houseList.Count == 0)
                {
                    //tight centre and no centre guard
                    if (tCenGuard && !cenGuard)
                    {
                        aiShoot.OnShot("Centre Guard", rockCurrent);
                    }
                    //centre and no tight centre guard
                    else if (cenGuard && !tCenGuard)
                    {
                        aiShoot.OnShot("Tight Centre Guard", rockCurrent);
                    }
                    //centre and tight centre guards
                    else if (cenGuard & tCenGuard)
                    {
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }
                    else
                    {
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
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
                            aiShoot.OnShot("Centre Guard", rockCurrent);
                        }
                        else
                        {
                            aiTarg.OnTarget("DrawFourFoot", rockCurrent, rockCurrent);
                        }
                    }
                    //centre and no tight centre guard
                    else if (cenGuard && !tCenGuard)
                    {
                        if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                        {
                            aiShoot.OnShot("High Centre Guard", rockCurrent);
                        }
                        else
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
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
                                        aiShoot.OnShot("Left Corner Guard", rockCurrent);
                                    }
                                    else aiShoot.OnShot("Right Corner Guard", rockCurrent);
                                }
                                //if second shot is not mine
                                else
                                {
                                    aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                }
                            }
                            //if there's only one rock in the house
                            else
                                aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                        }
                        else
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                    }
                    //any other guard combo
                    else
                    {
                        aiShoot.OnShot("Centre Guard", rockCurrent);
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
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            }
                            else
                                aiTarg.OnTarget("Take Out", rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                        }
                        //centre and no tight centre guard
                        else if (cenGuard && !tCenGuard)
                        {
                            //if the centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f)
                            {
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            }
                            else aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        //centre and tight centre guards
                        else if (cenGuard & tCenGuard)
                        {
                            //if the centre and tight centre guards are not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f & Mathf.Abs(closestRock.transform.position.x - tCenGuard.position.x) >= 0.1f)
                            {
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            }
                            else aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else
                        {
                            aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                        }
                    }
                    //if it's not in the four foot and there's more than one rock in the house
                    else if (gm.houseList.Count > 1)
                    {
                        //if the second shot is mine
                        if (gm.houseList[1].rockInfo.teamName == closestRockInfo.teamName)
                        {

                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else
                        {
                            aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                        }
                    }
                    else
                    {
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }
                }
                break;
            #endregion

            #region Late No Hammer
            case "late no hammer":

                #region No Rocks in House
                if (gm.houseList.Count == 0)
                {
                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                }
                #endregion
                #region I have Shot Rock
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
                                                aiShoot.OnShot("Centre Guard", rockCurrent);
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
                                                            if (lCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                            else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[2].rockInfo.rockIndex);
                                                        }
                                                        else if (gm.houseList[2].rock.transform.position.x > 0)
                                                        {
                                                            if (rCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                            else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[2].rockInfo.rockIndex);
                                                        }
                                                    }
                                                    else if (gm.houseList[1].rock.transform.position.x < 0)
                                                    {
                                                        if (lCornGuard) aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                                        else aiShoot.OnShot("Left Corner Guard", rockCurrent);
                                                    }
                                                    else if (gm.houseList[1].rock.transform.position.x > 0)
                                                    {
                                                        if (rCornGuard) aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                                        else aiShoot.OnShot("Right Corner Guard", rockCurrent);
                                                    }
                                                }
                                                //if second shot is not mine
                                                else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                                                {
                                                    if (gm.houseList[1].rock.transform.position.x < 0)
                                                    {
                                                        if (lCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                        else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                                    }
                                                    else if (gm.houseList[1].rock.transform.position.x > 0)
                                                    {
                                                        if (rCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                        else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                                    }
                                                    else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                                }
                                                //no second shot rock
                                                else
                                                {
                                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                                }
                                            }

                                        }
                                        //centre and no tight centre guard
                                        else if (cenGuard && !tCenGuard)
                                        {
                                            //if the centre guard is not covering it
                                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f)
                                            {
                                                aiShoot.OnShot("Tight Centre Guard", rockCurrent);
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
                                                            if (lCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                            else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[2].rockInfo.rockIndex);
                                                        }
                                                        else if (gm.houseList[2].rock.transform.position.x > 0)
                                                        {
                                                            if (rCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                            else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[2].rockInfo.rockIndex);
                                                        }
                                                    }
                                                    //if second shot is to the left
                                                    else if (gm.houseList[1].rock.transform.position.x < 0)
                                                    {
                                                        if (lCornGuard) aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                                        else aiShoot.OnShot("Left Corner Guard", rockCurrent);
                                                    }
                                                    //if second shot is to the right
                                                    else if (gm.houseList[1].rock.transform.position.x > 0)
                                                    {
                                                        if (rCornGuard) aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                                        else aiShoot.OnShot("Right Corner Guard", rockCurrent);
                                                    }
                                                }
                                                //if second shot is not mine
                                                else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                                                {
                                                    if (gm.houseList[1].rock.transform.position.x < 0)
                                                    {
                                                        if (lCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                        else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                                    }
                                                    else if (gm.houseList[1].rock.transform.position.x > 0)
                                                    {
                                                        if (rCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                        else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                                    }
                                                    else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                                }
                                                //no second shot rock
                                                else
                                                {
                                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                                }
                                            }
                                        }
                                        //centre and tight centre guards
                                        else if (cenGuard & tCenGuard)
                                        {
                                            //if the centre and tight centre guards are not covering it
                                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f & Mathf.Abs(closestRock.transform.position.x - tCenGuard.position.x) >= 0.1f)
                                            {
                                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                            }
                                            else aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                        }
                                        else
                                        {
                                            //if second shot is mine
                                            if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                                            {
                                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                            }
                                            //if second shot is not mine
                                            else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                                            {
                                                aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                            }
                                        }
                                    }
                                    //if it's not in the four foot
                                    else
                                    {
                                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                    }
                                }
                #endregion
                #region They have Shot Rock
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
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            }
                            //if the tight centre guard is covering it
                            else
                            {
                                //if second shot is mine
                                if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                                {
                                    aiTarg.OnTarget("Take Out", rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);

                                    //if third shot exists and is not mine
                                    if (gm.houseList.Count > 2 && gm.houseList[2].rockInfo.teamName != rockInfo.teamName)
                                    {
                                        if (gm.houseList[2].rock.transform.position.x < 0)
                                        {
                                            if (lCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                            else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                        }
                                        else if (gm.houseList[2].rock.transform.position.x > 0)
                                        {
                                            if (rCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                            else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                        }
                                    }
                                    else if (gm.houseList[1].rock.transform.position.x < 0)
                                    {
                                        if (lCornGuard) aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                        else aiShoot.OnShot("Left Corner Guard", rockCurrent);
                                    }
                                    else if (gm.houseList[1].rock.transform.position.x > 0)
                                    {
                                        if (rCornGuard) aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                        else aiShoot.OnShot("Right Corner Guard", rockCurrent);
                                    }
                                }
                                //if second shot is not mine
                                else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                                {
                                    if (gm.houseList[1].rock.transform.position.x < 0)
                                    {
                                        if (lCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                        else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                    }
                                    else if (gm.houseList[1].rock.transform.position.x > 0)
                                    {
                                        if (rCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                        else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                    }
                                    else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                }
                                //no second shot rock
                                else
                                {
                                    aiTarg.OnTarget("Take Out", rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                }
                            }

                        }
                        //centre and no tight centre guard
                        else if (cenGuard && !tCenGuard)
                        {
                            //if the centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f)
                            {
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            }
                            else
                            {
                                //if second shot is mine
                                if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                                {
                                    aiTarg.OnTarget("Take Out", rockCurrent, cenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                }
                                //if second shot is not mine
                                else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                                {
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                }
                            }
                        }
                        //centre and tight centre guards
                        else if (cenGuard & tCenGuard)
                        {
                            //if the centre and tight centre guards are not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f & Mathf.Abs(closestRock.transform.position.x - tCenGuard.position.x) >= 0.1f)
                            {
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            }
                            else aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else
                        {
                            //if second shot is mine
                            if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                            {
                                if (Mathf.Abs(closestRock.transform.position.x - gm.houseList[1].rock.transform.position.x) >= 0.2f)
                                    aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                else if (closestRock.transform.position.x > gm.houseList[1].rockInfo.rockIndex)
                                    aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                else
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            }
                            //if second shot is not mine
                            else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                            {
                                if (Mathf.Abs(closestRock.transform.position.x - gm.houseList[1].rock.transform.position.x) >= 0.2f)
                                    aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                else if (closestRock.transform.position.x > gm.houseList[1].rockInfo.rockIndex)
                                    aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                else
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            }
                        }
                    }
                    //if it's not in the four foot and there's more than one rock in the house
                    else if (gm.houseList.Count > 1)
                    {
                        //if the second shot is mine
                        if (gm.houseList[1].rockInfo.teamName == closestRockInfo.teamName)
                        {

                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else
                        {
                            aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                        }
                    }
                    else
                    {
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }
                }
                #endregion
                //default
                else
                {
                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    Debug.Log("Default Late Phase");
                }
                break;
#endregion

            default:
                break;
        }
    }

    public void ConservativeStealOrBlank(int rockCurrent, string phase)
    {
        {
            GameObject rock = gm.rockList[rockCurrent].rock;
            Rock_Info rockInfo = gm.rockList[rockCurrent].rockInfo;

            if (gm.houseList.Count != 0)
            {
                closestRock = gm.houseList[0].rock;
                closestRockInfo = gm.houseList[0].rockInfo;
            }

            Debug.Log("Phase is " + phase);

            aiTarg.OnTarget("Guard Reading", rockCurrent, 0);

            switch (phase)
            {
            #region Early Not Hammer
                case "early no hammer":

                    if (gm.houseList.Count != 0)
                    {
                        if (closestRockInfo.teamName == rockInfo.teamName)
                            aiShoot.OnShot("Centre Guard", rockCurrent);
                        else
                            aiTarg.OnTarget("Tap Back", rockCurrent, closestRockInfo.rockIndex);
                    }
                    else
                    {
                        Debug.Log("Am I Making it here??");
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }

                    break;
#endregion

            #region Middle Not Hammer
                case "middle no hammer":

                    if (gm.houseList.Count != 0)
                    {
                        Debug.Log("Steal or Blank rocks in house");
                        if (closestRockInfo.teamName != rockInfo.teamName)
                        {
                            aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                        }
                        else if (closestRockInfo.teamName == rockInfo.teamName)
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            //if (Mathf.Abs(closestRock.transform.position.x) <= 0.5f)
                            //{
                            //    if (cenGuard)
                            //        aiShoot.OnShot("High Centre Guard", rockCurrent);
                            //    else
                            //        aiShoot.OnShot("Centre Guard", rockCurrent);
                            //}
                            //else if (closestRock.transform.position.x > 0f)
                            //    aiShoot.OnShot("Left Four Foot", rockCurrent);
                            //else
                            //    aiShoot.OnShot("Right Four Foot", rockCurrent);
                        }
                        else aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }
                    else
                    {
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }

                    break;
#endregion

            #region Late Not Hammer
                case "late no hammer":
                    //there's rocks in the house
                    if (gm.houseList.Count != 0)
                    {
                        //They have shot rock
                        if (closestRockInfo.teamName != rockInfo.teamName)
                        {
                            //They have second shot too
                            if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                            {
                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            }
                            //there's a centre guard
                            else if (cenGuard)
                            {
                                if (Mathf.Abs(cenGuard.position.x - closestRock.transform.position.x) > 0.1f)
                                    aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                                else
                                    aiTarg.OnTarget("Peel", rockCurrent, cenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                            }
                            //there's a tight centre guard
                            else if (tCenGuard)
                            {
                                if (Mathf.Abs(tCenGuard.position.x - closestRock.transform.position.x) > 0.1f)
                                    aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                                else
                                    aiTarg.OnTarget("Peel", rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                            }
                            else
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);

                        }
                        //I have shot rock
                        else if (closestRockInfo.teamName == rockInfo.teamName)
                        {
                            //more than one rock in house
                            if (gm.houseList.Count > 1)
                            {
                                //I have second shot
                                if (gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                                {
                                    //second shot is to the left
                                    if (gm.houseList[1].rock.transform.position.x <= 0f)
                                        aiShoot.OnShot("Left Corner Guard", rockCurrent);
                                    else
                                        aiShoot.OnShot("Right Corner Guard", rockCurrent);
                                }
                                else
                                {
                                    //second shot is in the middle
                                    if (Mathf.Abs(gm.houseList[1].rock.transform.position.x) <= 0.5f)
                                    {
                                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                    }
                                    //second shot is not in the middle
                                    else
                                    {
                                        //second shot is to the left
                                        if (gm.houseList[1].rock.transform.position.x < 0)
                                        {
                                            if (lCornGuard)
                                                aiTarg.OnTarget("Peel", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                            else
                                                aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                        }
                                        //second shot is to the right
                                        else
                                        {
                                            if (rCornGuard)
                                                aiTarg.OnTarget("Peel", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                            else
                                                aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                        }
                                            aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                    }
                                        
                                }
                            }
                            //only one rock in house
                            else
                            {
                                if (Mathf.Abs(closestRock.transform.position.x) <= 0.5f)
                                {
                                    if (cenGuard | tCenGuard)
                                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                    else
                                        aiShoot.OnShot("Centre Guard", rockCurrent);
                                }
                                else
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            }
                        }
                    }
                    else
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);

                    break;
#endregion

                default:
                    break;
            }
        }
    }
}
