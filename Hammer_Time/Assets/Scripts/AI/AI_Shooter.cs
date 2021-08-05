using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Shooter : MonoBehaviour
{
    public GameManager gm;
    public TutorialManager tm;
    public RockManager rm;

    public AIManager aim;
    public AI_Target aiTarg;
    public AI_Strategy aiStrat;

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

    public float takeOutOffset;
    public float peelOffset;
    public float raiseOffset;
    public float tickOffset;

    float targetX;
    float targetY;
    public float takeOutX;
    float raiseY;

    public float osMult;
    GameObject closestRock;
    Rock_Info closestRockInfo;

    public void OnShot(string aiShotType, int rockCurrent)
    {
        rockInfo = gm.rockList[rockCurrent].rockInfo;
        rockFlick = gm.rockList[rockCurrent].rock.GetComponent<Rock_Flick>();
        rockRB = gm.rockList[rockCurrent].rock.GetComponent<Rigidbody2D>();

        StartCoroutine(Shot(aiShotType, rm.inturn));
    }

    IEnumerator Shot(string aiShotType, bool inturn)
    {
        Debug.Log("AI Shot " + aiShotType);
        gm.dbText.text = aiShotType;
        rockFlick.isPressedAI = true;
        takeOutX = aiTarg.takeOutX;
        yield return new WaitForSeconds(0.5f);

        float shotX;
        float shotY;

        switch (aiShotType)
        {
            case "Centre Guard":
                if (inturn)
                {
                    shotX = -1f * Random.Range(aim.centreGuard.x + guardAccu.x, aim.centreGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(aim.centreGuard.x + guardAccu.x, aim.centreGuard.x - guardAccu.x);
                }

                shotY = Random.Range(aim.centreGuard.y + guardAccu.y, aim.centreGuard.y - guardAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Tight Centre Guard":
                if (inturn)
                {
                    shotX = -1f * Random.Range(aim.tightCentreGuard.x + guardAccu.x, aim.tightCentreGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(aim.tightCentreGuard.x + guardAccu.x, aim.tightCentreGuard.x - guardAccu.x);
                }
                shotY = Random.Range(aim.tightCentreGuard.y + guardAccu.y, aim.tightCentreGuard.y - guardAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "High Centre Guard":
                if (inturn)
                {
                    shotX = -1f * Random.Range(aim.highCentreGuard.x + guardAccu.x, aim.highCentreGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(aim.highCentreGuard.x + guardAccu.x, aim.highCentreGuard.x - guardAccu.x);
                }
                shotY = Random.Range(aim.highCentreGuard.y + guardAccu.y, aim.highCentreGuard.y - guardAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Left Corner Guard":
                if (inturn)
                {
                    shotX = -1f * Random.Range(aim.rightCornerGuard.x + guardAccu.x, aim.rightCornerGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(aim.leftCornerGuard.x + guardAccu.x, aim.leftCornerGuard.x - guardAccu.x);
                }
                shotY = Random.Range(aim.leftCornerGuard.y + guardAccu.y, aim.leftCornerGuard.y - guardAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Left Tight Corner Guard":
                shotX = Random.Range(aim.leftTightCornerGuard.x + guardAccu.x, aim.leftTightCornerGuard.x - guardAccu.x);
                shotY = Random.Range(aim.leftTightCornerGuard.y + guardAccu.y, aim.leftTightCornerGuard.y - guardAccu.y);
                yield return new WaitForFixedUpdate();
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Left High Corner Guard":
                shotX = Random.Range(aim.leftHighCornerGuard.x + guardAccu.x, aim.leftHighCornerGuard.x - guardAccu.x);
                shotY = Random.Range(aim.leftHighCornerGuard.y + guardAccu.y, aim.leftHighCornerGuard.y - guardAccu.y);
                yield return new WaitForFixedUpdate();
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Right Corner Guard":
                shotX = Random.Range(aim.rightCornerGuard.x + guardAccu.x, aim.rightTightCornerGuard.x - guardAccu.x);
                shotY = Random.Range(aim.rightTightCornerGuard.y + guardAccu.y, aim.rightTightCornerGuard.y - guardAccu.y);
                yield return new WaitForFixedUpdate();
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Right Tight Corner Guard":
                shotX = Random.Range(aim.rightTightCornerGuard.x + guardAccu.x, aim.rightTightCornerGuard.x - guardAccu.x);
                shotY = Random.Range(aim.rightTightCornerGuard.y + guardAccu.y, aim.rightTightCornerGuard.y - guardAccu.y);
                yield return new WaitForFixedUpdate();
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Right High Corner Guard":
                shotX = Random.Range(aim.rightHighCornerGuard.x + guardAccu.x, aim.rightHighCornerGuard.x - guardAccu.x);
                shotY = Random.Range(aim.rightHighCornerGuard.y + guardAccu.y, aim.rightHighCornerGuard.y - guardAccu.y);
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Top Twelve Foot":
                if (inturn)
                {
                    shotX = -1f * Random.Range(aim.topTwelveFoot.x + drawAccu.x, aim.topTwelveFoot.x - drawAccu.x);
                }
                else
                {
                    shotX = Random.Range(aim.topTwelveFoot.x + drawAccu.x, aim.topTwelveFoot.x - drawAccu.x);
                }
                shotY = Random.Range(aim.topTwelveFoot.y + drawAccu.y, aim.topTwelveFoot.y - drawAccu.y);
                yield return new WaitForFixedUpdate();
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Left Twelve Foot":
                if (inturn)
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
                if (inturn)
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
                if (inturn)
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
                if (inturn)
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
                if (inturn)
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
                if (inturn)
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
                if (inturn)
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
                if (inturn)
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
                    //if (inturn)
                    //{
                    //    takeOutOffset = -takeOutOffset;
                    //}

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
