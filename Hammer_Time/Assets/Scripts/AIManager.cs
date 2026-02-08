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

        // W KEY: Start AI vs AI game for testing AI takeouts and strategy
        //if (Input.GetKeyDown(KeyCode.W))
        //{
        //    Debug.Log("[AIManager] W pressed - Starting AI vs AI game");
        //    StartCoroutine(StartAIvsAIGame());
        //}

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
        }
    }
    
    /// <summary>
    /// Starts an AI vs AI game for testing purposes (press W)
    /// Both teams are controlled by AI so you can observe strategy and targeting
    /// </summary>
    IEnumerator StartAIvsAIGame()
    {
        // Set both teams to AI
        gm.aiTeamRed = true;
        gm.aiTeamYellow = true;
        
        Debug.Log("[AIManager] AI vs AI mode enabled - Red: AI, Yellow: AI");
        
        // Wait a frame to ensure settings are applied
        yield return new WaitForEndOfFrame();
        
        // Trigger the current turn based on who should be shooting
        if (gm.rockCurrent % 2 == 0)
        {
            if (gm.redHammer)
            {
                Debug.Log("[AIManager] Starting Yellow AI turn");
                gm.OnYellowTurn();
            }
            else
            {
                Debug.Log("[AIManager] Starting Red AI turn");
                gm.OnRedTurn();
            }
        }
        else
        {
            if (gm.redHammer)
            {
                Debug.Log("[AIManager] Starting Red AI turn");
                gm.OnRedTurn();
            }
            else
            {
                Debug.Log("[AIManager] Starting Yellow AI turn");
                gm.OnYellowTurn();
            }
        }
    }

    

    public void OnShot(int rockCurrent)
    {
        // Clear trajectory from previous turn
        TrajectoryLine trajectoryLine = FindAnyObjectByType<TrajectoryLine>();
        if (trajectoryLine != null)
        {
            trajectoryLine.ClearTrajectory();
        }
        
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

        //aiStrat.SimpleAIShoot(rockCurrent);
        aiStrat.OnShot(rockCurrent);
    }

}
