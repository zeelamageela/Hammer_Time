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

    public string testing;
    public string testingTakeOut;
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

    public bool story;
    bool inturn;
    float targetX;
    float targetY;
    public float takeOutX;
    float raiseY;

    GameObject closestRock;
    Rock_Info closestRockInfo;

    // OnEnable is called when the Game Object is enabled
    void OnEnable()
    {

    }

    //private void Start()
    //{
    //    story = gm.gsp.story;
    //}
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

            aiTarg.OnTarget(testingTakeOut, gm.rockCurrent, testingRockNumber);
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

            aiTarg.OnTarget("Player Draw", gm.rockCurrent, 0);
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

    

    public void OnShot(int rockCurrent)
    {
        rockInfo = gm.rockList[rockCurrent].rockInfo;
        rockFlick = gm.rockList[rockCurrent].rock.GetComponent<Rock_Flick>();
        rockRB = gm.rockList[rockCurrent].rock.GetComponent<Rigidbody2D>();


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

        }

        aiStrat.SimpleAIShoot(rockCurrent);
        //aiStrat.OnShot(rockCurrent);
    }

}
