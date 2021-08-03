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
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }
                    else
                    {
                        aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                    }
                    break;

                case 1:

                    aiTarg.OnTarget("Auto Take Out", rockCurrent, rockCurrent);
                    break;

                case 2:

                    aiTarg.OnTarget("Auto Take Out", rockCurrent, rockCurrent);
                    break;

                case 3:

                    aiTarg.OnTarget("Auto Take Out", rockCurrent, rockCurrent);
                    break;

                case 4:

                    aiTarg.OnTarget("Auto Take Out", rockCurrent, rockCurrent);
                    break;

                case 5:

                    aiTarg.OnTarget("Auto Take Out", rockCurrent, rockCurrent);
                    break;

                case 6:

                    aiTarg.OnTarget("Auto Take Out", rockCurrent, rockCurrent);
                    break;

                case 7:

                    aiTarg.OnTarget("Auto Take Out", rockCurrent, rockCurrent);
                    break;

                case 8:

                    aiTarg.OnTarget("Auto Take Out", rockCurrent, rockCurrent);
                    break;

                case 9:

                    aiTarg.OnTarget("Auto Take Out", rockCurrent, rockCurrent);
                    break;

                case 10:

                    aiTarg.OnTarget("Auto Take Out", rockCurrent, rockCurrent);
                    break;

                case 11:

                    aiTarg.OnTarget("Auto Take Out", rockCurrent, rockCurrent);
                    break;

                case 12:
                    //rm.inturn = true;

                    aiTarg.OnTarget("Auto Take Out", rockCurrent, rockCurrent);
                    break;

                case 13:

                    aiTarg.OnTarget("Auto Take Out", rockCurrent, rockCurrent);
                    break;

                case 14:

                    aiTarg.OnTarget("Auto Take Out", rockCurrent, rockCurrent);
                    break;

                case 15:

                    aiTarg.OnTarget("Auto Take Out", rockCurrent, rockCurrent);
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

        aiTarg.OnTarget("Guard Reading", rockCurrent, 0);

        switch (phase)
        {
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

            case "early hammer":
                //if there's guards
                if (gm.gList.Count != 0)
                {
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
                }
                break;


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


            case "late no hammer":

                //if no rocks in the house
                if (gm.houseList.Count == 0)
                {
                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
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
                                aiTarg.OnTarget("Take Out", rockCurrent, cenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                            }
                            //if second shot is not mine
                            else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                            {
                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            }
                        }
                    }
                    //if it's not in the four foot
                    else
                    {
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
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
                                aiTarg.OnTarget("Take Out", rockCurrent, cenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                            }
                            //if second shot is not mine
                            else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                            {
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
                //default
                else
                {
                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
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
                        else aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
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
                                aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
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
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }

                }
                break;


            default:
                break;
        }
    }
}
