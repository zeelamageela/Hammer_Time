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
    public AI_Sweeper aiSweep;

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
    public float takeOutY;
    float raiseY;
    GameSettingsPersist gsp;

    public void Start()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
    }


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
        takeOutY = aiTarg.takeOutY;

        aiSweep.OnSweep(true, aiShotType, aiTarg.targetPos, inturn);

        yield return new WaitForSeconds(0.5f);

        float shotX;
        float shotY;

        switch (aiShotType)
        {
            #region Centre Guards
            case "Centre Guard":
                if (inturn)
                {
                    shotX = -1f * Random.Range(centreGuard.x + guardAccu.x, centreGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(centreGuard.x + guardAccu.x, centreGuard.x - guardAccu.x);
                }

                shotY = Random.Range(centreGuard.y + guardAccu.y, centreGuard.y - guardAccu.y);
                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Tight Centre Guard":
                if (inturn)
                {
                    shotX = -1f * Random.Range(tightCentreGuard.x + guardAccu.x, tightCentreGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(tightCentreGuard.x + guardAccu.x, tightCentreGuard.x - guardAccu.x);
                }
                shotY = Random.Range(tightCentreGuard.y + guardAccu.y, tightCentreGuard.y - guardAccu.y);
                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "High Centre Guard":
                if (inturn)
                {
                    shotX = -1f * Random.Range(highCentreGuard.x + guardAccu.x, highCentreGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(highCentreGuard.x + guardAccu.x, highCentreGuard.x - guardAccu.x);
                }
                shotY = Random.Range(highCentreGuard.y + guardAccu.y, highCentreGuard.y - guardAccu.y);
                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;
            #endregion

            #region Corner Guards
            case "Left Corner Guard":
                if (inturn)
                {
                    shotX = -1f * Random.Range(rightCornerGuard.x + guardAccu.x, rightCornerGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(leftCornerGuard.x + guardAccu.x, leftCornerGuard.x - guardAccu.x);
                }
                shotY = Random.Range(leftCornerGuard.y + guardAccu.y, leftCornerGuard.y - guardAccu.y);
                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Left Tight Corner Guard":
                if (inturn)
                {
                    shotX = -1f * Random.Range(rightTightCornerGuard.x + guardAccu.x, rightTightCornerGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(leftTightCornerGuard.x + guardAccu.x, leftTightCornerGuard.x - guardAccu.x);
                }
                shotY = Random.Range(leftTightCornerGuard.y + guardAccu.y, leftTightCornerGuard.y - guardAccu.y);
                yield return new WaitForFixedUpdate();
                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Left High Corner Guard":
                if (inturn)
                {
                    shotX = -1f * Random.Range(rightHighCornerGuard.x + guardAccu.x, rightHighCornerGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(leftHighCornerGuard.x + guardAccu.x, leftHighCornerGuard.x - guardAccu.x);
                }
                shotY = Random.Range(leftHighCornerGuard.y + guardAccu.y, leftHighCornerGuard.y - guardAccu.y);
                yield return new WaitForFixedUpdate();
                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Right Corner Guard":
                if (inturn)
                {
                    shotX = -1f * Random.Range(leftCornerGuard.x + guardAccu.x, leftCornerGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(rightCornerGuard.x + guardAccu.x, rightCornerGuard.x - guardAccu.x);
                }
                shotY = Random.Range(rightCornerGuard.y + guardAccu.y, rightCornerGuard.y - guardAccu.y);
                yield return new WaitForFixedUpdate();
                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Right Tight Corner Guard":
                if (inturn)
                {
                    shotX = -1f * Random.Range(leftTightCornerGuard.x + guardAccu.x, leftTightCornerGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(rightTightCornerGuard.x + guardAccu.x, rightTightCornerGuard.x - guardAccu.x);
                }
                shotY = Random.Range(rightTightCornerGuard.y + guardAccu.y, rightTightCornerGuard.y - guardAccu.y);
                yield return new WaitForFixedUpdate();
                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Right High Corner Guard":
                if (inturn)
                {
                    shotX = -1f * Random.Range(leftHighCornerGuard.x + guardAccu.x, leftHighCornerGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(rightHighCornerGuard.x + guardAccu.x, rightHighCornerGuard.x - guardAccu.x);
                }
                shotY = Random.Range(rightHighCornerGuard.y + guardAccu.y, rightHighCornerGuard.y - guardAccu.y);

                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;
            #endregion

            #region Twelve Foot Draws
            case "Top Twelve Foot":
                if (inturn)
                {
                    shotX = -1f * Random.Range(topTwelveFoot.x + drawAccu.x, topTwelveFoot.x - drawAccu.x);
                }
                else
                {
                    shotX = Random.Range(topTwelveFoot.x + drawAccu.x, topTwelveFoot.x - drawAccu.x);
                }
                shotY = Random.Range(topTwelveFoot.y + drawAccu.y, topTwelveFoot.y - drawAccu.y);

                rockFlick.rb.isKinematic = true;
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

                rockFlick.rb.isKinematic = true;
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

                rockFlick.rb.isKinematic = true;
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

                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;
            #endregion

            #region Four Foot Draws
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

                rockFlick.rb.isKinematic = true;
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

                rockFlick.rb.isKinematic = true;
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
                rockFlick.rb.isKinematic = true;
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

                rockFlick.rb.isKinematic = true;
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

                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;
            #endregion

            #region Take Outs
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

                rockFlick.rb.isKinematic = true;
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

                rockFlick.rb.isKinematic = true;
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

                rockFlick.rb.isKinematic = true;
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

                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);

                Debug.Log("Raise Position is (" + rockRB.position.x + " ," + rockRB.position.y + ")");
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;
            #endregion

            case "Draw To Target":

                shotX = takeOutX;
                shotY = takeOutY;

                //shotX = Random.Range(takeOutX + drawAccu.x, takeOutX - drawAccu.x);
                //shotY = Random.Range(takeOutY + drawAccu.y, takeOutY - drawAccu.y);

                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                rockFlick.mouseUp = true;
                break;

            case "Guard To Target":

                //shotX = takeOutX;
                //shotY = takeOutY;

                shotX = Random.Range(takeOutX + guardAccu.x, takeOutX - guardAccu.x);
                shotY = Random.Range(takeOutY + guardAccu.y, takeOutY - guardAccu.y);

                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                rockFlick.mouseUp = true;
                break;

            default:
                break;
        }

    }


    IEnumerator TargetShot(string aiShotType, bool inturn)
    {
        Debug.Log("AI Shot " + aiShotType);
        gm.dbText.text = aiShotType;
        rockFlick.isPressedAI = true;
        takeOutX = aiTarg.takeOutX;
        takeOutY = aiTarg.takeOutY;
        yield return new WaitForSeconds(0.5f);

        float shotX;
        float shotY;

        switch (aiShotType)
        {
            #region Centre Guards
            case "Centre Guard":
                if (inturn)
                {
                    shotX = -1f * Random.Range(centreGuard.x + guardAccu.x, centreGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(centreGuard.x + guardAccu.x, centreGuard.x - guardAccu.x);
                }

                shotY = Random.Range(centreGuard.y + guardAccu.y, centreGuard.y - guardAccu.y);
                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Tight Centre Guard":
                if (inturn)
                {
                    shotX = -1f * Random.Range(tightCentreGuard.x + guardAccu.x, tightCentreGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(tightCentreGuard.x + guardAccu.x, tightCentreGuard.x - guardAccu.x);
                }
                shotY = Random.Range(tightCentreGuard.y + guardAccu.y, tightCentreGuard.y - guardAccu.y);
                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "High Centre Guard":
                if (inturn)
                {
                    shotX = -1f * Random.Range(highCentreGuard.x + guardAccu.x, highCentreGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(highCentreGuard.x + guardAccu.x, highCentreGuard.x - guardAccu.x);
                }
                shotY = Random.Range(highCentreGuard.y + guardAccu.y, highCentreGuard.y - guardAccu.y);
                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;
            #endregion

            #region Corner Guards
            case "Left Corner Guard":
                if (inturn)
                {
                    shotX = -1f * Random.Range(rightCornerGuard.x + guardAccu.x, rightCornerGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(leftCornerGuard.x + guardAccu.x, leftCornerGuard.x - guardAccu.x);
                }
                shotY = Random.Range(leftCornerGuard.y + guardAccu.y, leftCornerGuard.y - guardAccu.y);
                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Left Tight Corner Guard":
                shotX = Random.Range(leftTightCornerGuard.x + guardAccu.x, leftTightCornerGuard.x - guardAccu.x);
                shotY = Random.Range(leftTightCornerGuard.y + guardAccu.y, leftTightCornerGuard.y - guardAccu.y);
                yield return new WaitForFixedUpdate();
                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Left High Corner Guard":
                shotX = Random.Range(leftHighCornerGuard.x + guardAccu.x, leftHighCornerGuard.x - guardAccu.x);
                shotY = Random.Range(leftHighCornerGuard.y + guardAccu.y, leftHighCornerGuard.y - guardAccu.y);
                yield return new WaitForFixedUpdate();
                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Right Corner Guard":
                if (inturn)
                {
                    shotX = -1f * Random.Range(leftCornerGuard.x + guardAccu.x, leftCornerGuard.x - guardAccu.x);
                }
                else
                {
                    shotX = Random.Range(rightCornerGuard.x + guardAccu.x, rightCornerGuard.x - guardAccu.x);
                }
                shotX = Random.Range(rightCornerGuard.x + guardAccu.x, rightCornerGuard.x - guardAccu.x);
                shotY = Random.Range(rightCornerGuard.y + guardAccu.y, rightCornerGuard.y - guardAccu.y);
                yield return new WaitForFixedUpdate();
                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Right Tight Corner Guard":
                shotX = Random.Range(rightTightCornerGuard.x + guardAccu.x, rightTightCornerGuard.x - guardAccu.x);
                shotY = Random.Range(rightTightCornerGuard.y + guardAccu.y, rightTightCornerGuard.y - guardAccu.y);
                yield return new WaitForFixedUpdate();
                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;

            case "Right High Corner Guard":
                shotX = Random.Range(rightHighCornerGuard.x + guardAccu.x, rightHighCornerGuard.x - guardAccu.x);
                shotY = Random.Range(rightHighCornerGuard.y + guardAccu.y, rightHighCornerGuard.y - guardAccu.y);

                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;
            #endregion

            #region Twelve Foot Draws
            case "Top Twelve Foot":
                if (inturn)
                {
                    shotX = -1f * Random.Range(topTwelveFoot.x + drawAccu.x, topTwelveFoot.x - drawAccu.x);
                }
                else
                {
                    shotX = Random.Range(topTwelveFoot.x + drawAccu.x, topTwelveFoot.x - drawAccu.x);
                }
                shotY = Random.Range(topTwelveFoot.y + drawAccu.y, topTwelveFoot.y - drawAccu.y);

                rockFlick.rb.isKinematic = true;
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

                rockFlick.rb.isKinematic = true;
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

                rockFlick.rb.isKinematic = true;
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

                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;
            #endregion

            #region Four Foot Draws
            case "Button":
                aiTarg.aiTarget.transform.position = new Vector2(0f, 6.5f);

                if (inturn)
                {
                    shotX = -1f * Random.Range(button.x + drawAccu.x, button.x - drawAccu.x);
                }
                else
                {
                    shotX = Random.Range(button.x + drawAccu.x, button.x - drawAccu.x);
                }
                shotY = Random.Range(button.y + drawAccu.y, button.y - drawAccu.y);

                rockFlick.rb.isKinematic = true;
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

                rockFlick.rb.isKinematic = true;
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
                rockFlick.rb.isKinematic = true;
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

                rockFlick.rb.isKinematic = true;
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

                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;
            #endregion

            #region Take Outs
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

                rockFlick.rb.isKinematic = true;
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

                rockFlick.rb.isKinematic = true;
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

                rockFlick.rb.isKinematic = true;
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

                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);

                Debug.Log("Raise Position is (" + rockRB.position.x + " ," + rockRB.position.y + ")");
                yield return new WaitForFixedUpdate();
                rockFlick.mouseUp = true;
                break;
            #endregion

            case "Draw To Target":

                shotX = takeOutX;
                shotY = takeOutY;

                //shotX = Random.Range(takeOutX + drawAccu.x, takeOutX - drawAccu.x);
                //shotY = Random.Range(takeOutY + drawAccu.y, takeOutY - drawAccu.y);

                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                rockFlick.mouseUp = true;
                break;

            case "Guard To Target":

                //shotX = takeOutX;
                //shotY = takeOutY;

                shotX = Random.Range(takeOutX + guardAccu.x, takeOutX - guardAccu.x);
                shotY = Random.Range(takeOutY + guardAccu.y, takeOutY - guardAccu.y);

                rockFlick.rb.isKinematic = true;
                rockRB.position = new Vector2(shotX, shotY);
                rockFlick.mouseUp = true;
                break;

            default:
                break;
        }

    }
}
