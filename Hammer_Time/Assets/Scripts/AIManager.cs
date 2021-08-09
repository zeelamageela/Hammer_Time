using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public GameManager gm;
    public TutorialManager tm;
    public RockManager rm;

    public AI_Target aiTarg;
    public AI_Shooter aiShoot;
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

    public bool aggressive;

    public Transform cenGuard;
    public Transform tCenGuard;
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

            aiShoot.OnShot(testing, gm.rockCurrent);
            //StartCoroutine(Shot(testing));
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

            aiTarg.OnTarget("Take Out", gm.rockCurrent, testingRockNumber);
            //StartCoroutine(Shot(testing));
            //StartCoroutine(TickShot(gm.rockCurrent));
            //StartCoroutine(TapTarget(gm.rockCurrent, testingRockNumber));
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

            aiTarg.OnTarget("Take Out", gm.rockCurrent, 0);
            //StartCoroutine(Shot(testing));
            //StartCoroutine(TakeOutAutoTarget(gm.rockCurrent));
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

            aiTarg.OnTarget("Auto Draw Four Foot", gm.rockCurrent, 0);
            //StartCoroutine(DrawFourFoot(gm.rockCurrent));
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

            aiTarg.OnTarget("Auto Draw Twelve Foot", gm.rockCurrent, 0);
            //StartCoroutine(DrawTwelveFoot(gm.rockCurrent));
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

        aggressive = true;

        //if (gm.redScore > gm.yellowScore)
        //{
        //    aggressive = true;
        //}
        //else if (gm.redScore < gm.yellowScore)
        //{
        //    aggressive = false;
        //}
        //else
        //{
        //    aggressive = true;
        //}

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
            aiStrat.OnShot(rockCurrent);
        }
        else
            aiStrat.Conservative(rockCurrent);
    }

}
