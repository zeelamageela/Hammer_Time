using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public enum GameState { START, REDTURN, YELLOWTURN, CHECKSCORE, SCORE, RESET, END, DEBUG }


public class GameManager : MonoBehaviour
{
    int endCurrent;
    public int endTotal;
    public bool redHammer;
    public int rocksPerTeam;
    public int rockTotal;
    public int rockCurrent;

    public RockManager rm;
    public GameObject redShooter;
    public GameObject yellowShooter;
    public GameObject shooterAnimRed;
    public GameObject shooterAnimYellow;

    public SweeperManager sm;
    public Sweeper sweeper;
    GameObject shooterGO;

    public Transform launcher;
    public Transform yellowRocksInactive;
    public Transform redRocksInactive;
    public Collider2D boardCollider;
    public Collider2D launchCollider;

    public GameObject yellowSpin;
    public GameObject redSpin;

    int redRocks_left;
    int yellowRocks_left;
    public int redScore;
    public int yellowScore;

    public GameHUD gHUD;
    public RockBar rockBar;

    public GameObject debug;

    public GameState state;

    Rock_Info redRock;
    Rock_Info yellowRock;

    public Button redButton;
    public Button yellowButton;
    public Button sweepButton;
    public GameObject db;
    public GameObject dbrandom;

    public CameraManager cm;
    public GameObject vcam_go;
    public CinemachineVirtualCamera vcam;
    Transform tFollowTarget;

    public ShootingKnob knob;
    public List<Rock_List> rockList;
    public List<House_List> houseList;

    void Start()
    {
        state = GameState.START;
        redButton.gameObject.SetActive(false);
        yellowButton.gameObject.SetActive(false);
        sweepButton.gameObject.SetActive(false);

        StartCoroutine(SetupGame());
    }

    IEnumerator SetupGame()
    {
        //gHUD.SetHUD(redRock);
        Debug.Log("Game Start");

        endCurrent = 1;
        rockCurrent = 0;
        redRocks_left = rocksPerTeam;
        yellowRocks_left = rocksPerTeam;
        rockTotal = rocksPerTeam * 2;

        rockList = new List<Rock_List>();
        houseList = new List<House_List>();

        yield return new WaitForSeconds(1f);

        redButton.gameObject.SetActive(true);
        yellowButton.gameObject.SetActive(true);
    }

    public void SetHammerRed()
    {
        redHammer = true;
        gHUD.SetHammer(redHammer);
        db.SetActive(false);
        redButton.gameObject.SetActive(false);
        yellowButton.gameObject.SetActive(false);

        StartCoroutine(SetupRocks());
        OnYellowTurn();
    }

    public void SetHammerYellow()
    {
        redHammer = false;
        db.SetActive(false);

        gHUD.SetHammer(redHammer);

        redButton.gameObject.SetActive(false);
        yellowButton.gameObject.SetActive(false);

        StartCoroutine(SetupRocks());
        OnRedTurn();
    }

    IEnumerator SetupRocks()
    {
        int hammer;
        int notHammer;

        if (redHammer)
        {
            hammer = 0;
            notHammer = 1;
        }
        else
        {
            hammer = 1;
            notHammer = 0;
        }

        for (int i = 1; i <= rockTotal; i++)
        {
            if (i % 2 == notHammer)
            {
                GameObject yellowRock_go = Instantiate(yellowShooter, yellowRocksInactive);

                float yRocks = rocksPerTeam * 0.5f;
                int k = (i / 2) + notHammer;

                if (k <= yRocks)
                {
                    yellowRock_go.transform.position = new Vector2(yellowRock_go.transform.position.x, yellowRock_go.transform.position.y - ((k - 1) * 0.4f));
                }
                else if (k > yRocks)
                {
                    float j = k - yRocks;
                    yellowRock_go.transform.position = new Vector2(yellowRock_go.transform.position.x + 0.4f, yellowRock_go.transform.position.y - ((j - 1) * 0.4f));
                }

                Rock_Info yellowRock_info = yellowRock_go.GetComponent<Rock_Info>();
                yellowRock_info.rockNumber = k;
                yellowRock_go.name = yellowRock_info.teamName + " " + yellowRock_info.rockNumber;
                yellowRock_go.GetComponent<Rock_Flick>().enabled = false;
                yellowRock_go.GetComponent<Rock_Release>().enabled = false;
                yellowRock_go.GetComponent<Rock_Force>().enabled = false;
                rockList.Add(new Rock_List(yellowRock_go, yellowRock_info));
                yield return new WaitForSeconds(0.1f);
            }
            if (i % 2 == hammer)
            {
                GameObject redRock_go = Instantiate(redShooter, redRocksInactive);
                float yRocks = rocksPerTeam / 2f;
                int k = (i / 2) + hammer;
                if (k <= yRocks)
                {
                    redRock_go.transform.position = new Vector2(redRock_go.transform.position.x, redRock_go.transform.position.y - ((k - 1) * 0.4f));
                }
                else if (k > yRocks)
                {
                    float j = k - yRocks;
                    redRock_go.transform.position = new Vector2(redRock_go.transform.position.x - 0.4f, redRock_go.transform.position.y - ((j - 1) * 0.4f));
                }


                Rock_Info redRock_info = redRock_go.GetComponent<Rock_Info>();
                redRock_info.rockNumber = k;
                redRock_go.name = redRock_info.teamName + " " + redRock_info.rockNumber;
                redRock_go.GetComponent<Rock_Flick>().enabled = false;
                redRock_go.GetComponent<Rock_Release>().enabled = false;
                redRock_go.GetComponent<Rock_Force>().enabled = false;
                rockList.Add(new Rock_List(redRock_go, redRock_info));
                yield return new WaitForSeconds(0.1f);
            }
            //rockList.Sort();
        }

        rockBar.ResetBar(redHammer);

    }

    IEnumerator ResetGame()
    {
        houseList.Clear();

        if (rockList.Count != 0)
        {
            foreach (Rock_List rock in rockList)
            {
                Destroy(rock.rock);
            }
        }

        yield return new WaitForFixedUpdate();

        rockList.Clear();
        endCurrent++;

        yield return new WaitForSeconds(1f);

        redRocks_left = rocksPerTeam;
        yellowRocks_left = rocksPerTeam;
        rockCurrent = 0;

        gHUD.SetHammer(redHammer);

        if (redHammer)
        {
            StartCoroutine(SetupRocks());
            OnYellowTurn();
        }
        else
        {
            StartCoroutine(SetupRocks());
            OnRedTurn();
        }
    }

    public void OnRedTurn()
    {
        shooterGO = Instantiate(shooterAnimRed);
        sm.SetupSweepers();

        Debug.Log("Red Turn");
        state = GameState.REDTURN;

        //vcam = vcam_go.GetComponent<CinemachineVirtualCamera>();

        //tFollowTarget = launcher.transform;
        //vcam.LookAt = tFollowTarget;
        //vcam.Follow = tFollowTarget;
        //vcam.enabled = true;

        cm.ShotSetup();

        GameObject redRock_1 = rockList[rockCurrent].rock;
        redRock_1.GetComponent<Rock_Flick>().enabled = true;
        redRock_1.GetComponent<Rock_Release>().enabled = true;
        redRock_1.GetComponent<Rock_Force>().enabled = true;
        redRock_1.GetComponent<Rock_Colliders>().enabled = true;
        boardCollider.enabled = false;
        launchCollider.enabled = false;

        StartCoroutine(RedTurn());
    }

    IEnumerator RedTurn()
    {

        redRocks_left--;
        rm.inturn = true;
        cm.TopViewAuto();
        Debug.Log("rmInturn is " + rm.inturn);

        GameObject redRock_1 = rockList[rockCurrent].rock;

        redRock = redRock_1.GetComponent<Rock_Info>();
        Debug.Log(redRock_1.name);

        //gHUD.SetHUD(redRocks_left, yellowRocks_left, rocksPerTeam, rockCurrent, redRock);
        rockBar.ActiveRock(true);
        
        yield return new WaitUntil(() => redRock.shotTaken == true);

        cm.RockFollow(redRock.transform);
        boardCollider.enabled = true;

        yield return new WaitUntil(() => redRock.released == true);

        sm.Release(redRock_1);
        //sweeper.AttachToRock(redRock_1);
        rm.GetComponent<Sweep>().EnterSweepZone();

        yield return new WaitUntil(() => redRock.rest == true);

        cm.TopViewAuto();

        if (!redRock.outOfPlay)
        {
            rockBar.IdleRock();
        }
        else
        {
            rockBar.DeadRock(rockCurrent);
        }

        rm.GetComponent<Sweep>().ExitSweepZone();

        Debug.Log("redRock at Rest");
        vcam.enabled = false;

        StartCoroutine(AllStopped());

        foreach (Rock_List rock in rockList)
        {
            bool outOfPlay;
            int rockIndex;
            rockIndex = rockList.IndexOf(rock);
            outOfPlay = rock.rockInfo.outOfPlay;
            rockBar.ShotUpdate(rockIndex, outOfPlay);
        }

        StartCoroutine(CheckScore());
    }

    public void OnYellowTurn()
    {
        shooterGO = Instantiate(shooterAnimYellow);
        sm.SetupSweepers();

        Debug.Log("Yellow Turn");
        state = GameState.YELLOWTURN;

        cm.ShotSetup();

        //vcam = vcam_go.GetComponent<CinemachineVirtualCamera>();

        //tFollowTarget = launcher.transform;
        //vcam.LookAt = tFollowTarget;
        //vcam.Follow = tFollowTarget;
        //vcam.enabled = true;

        GameObject yellowRock_1 = rockList[rockCurrent].rock;
        yellowRock_1.GetComponent<Rock_Flick>().enabled = true;
        yellowRock_1.GetComponent<Rock_Release>().enabled = true;
        yellowRock_1.GetComponent<Rock_Force>().enabled = true;
        yellowRock_1.GetComponent<Rock_Colliders>().enabled = true;
        boardCollider.enabled = false;
        launchCollider.enabled = false;

        StartCoroutine(YellowTurn());
    }

    IEnumerator YellowTurn()
    {
        yellowRocks_left--;
        cm.TopViewAuto();
        rm.inturn = true;
        Debug.Log("rmInturn is " + rm.inturn);

        GameObject yellowRock_1 = rockList[rockCurrent].rock;

        yellowRock = yellowRock_1.GetComponent<Rock_Info>();
        Debug.Log(yellowRock_1.name);

        rockBar.ActiveRock(false);
        //gHUD.SetHUD(redRocks_left, yellowRocks_left, rocksPerTeam, rockCurrent, yellowRock);


        yield return new WaitUntil(() => yellowRock.shotTaken == true);

        cm.RockFollow(yellowRock.transform);
        boardCollider.enabled = true;

        yield return new WaitUntil(() => yellowRock.released == true);

        sm.Release(yellowRock_1);
        //sweeper.AttachToRock(yellowRock_1);
        rm.GetComponent<Sweep>().EnterSweepZone();

        yield return new WaitUntil(() => yellowRock.rest == true);
        
        
        if (!yellowRock.outOfPlay)
        {
            Debug.Log("Idle Rockbar");
            rockBar.IdleRock();
        }
        else
        {
            Debug.Log("Dead Rockbar");
            rockBar.DeadRock(rockCurrent);
        }

        rm.GetComponent<Sweep>().ExitSweepZone();

        vcam.enabled = false;

        StartCoroutine(AllStopped());

        yield return new WaitForEndOfFrame();

        foreach(Rock_List rock in rockList)
        {
            bool outOfPlay;
            int rockIndex;
            rockIndex = rockList.IndexOf(rock);
            outOfPlay = rock.rockInfo.outOfPlay;
            rockBar.ShotUpdate(rockIndex, outOfPlay);
        }

        yield return new WaitForEndOfFrame();

        StartCoroutine(CheckScore());

    }

    IEnumerator AllStopped()
    {
        bool allSleeping = false;

        while (!allSleeping)
        {
            allSleeping = true;

            foreach (Rock_List rock in rockList)
            {
                Rigidbody2D rockRB = rock.rock.GetComponent<Rigidbody2D>();
                if (!rockRB.IsSleeping())
                {
                    allSleeping = false;
                    yield return null;
                    break;
                }
            }
        }

        yield return new WaitForFixedUpdate();
    }

    public IEnumerator CheckScore()
    {
        state = GameState.CHECKSCORE;

        Destroy(shooterGO);

        Debug.Log("Check Score");

        StartCoroutine(AllStopped());
        Debug.Log("All Stopped");
        yield return new WaitForFixedUpdate();

        houseList.Clear();

        foreach (Rock_List rock in rockList)
        {
            if (rock.rockInfo.inHouse == true)
            {
                houseList.Add(new House_List(rock.rock, rock.rockInfo));
                
            } 
        }

        yield return new WaitForFixedUpdate();

        houseList.Sort();

        if (rockList.Count == rockCurrent + 1)
        {
            Debug.Log("Rock List " + rockList.Count + " equals " + "Current Rock " + (rockCurrent + 1));
            StartCoroutine(Scoring());
        }
        else
        {
            if (houseList.Count != 0)
            {
                foreach (House_List rock in houseList)
                {
                    Debug.Log(rock.rockInfo.name + " - " + rock.rockInfo.distance);
                }
            }

            yield return new WaitForFixedUpdate();

            int houseRock = houseList.Count;

            // if the list isnt empty
            if (houseList.Count != 0)
            {
                int houseScore = 0;
                string winningTeamName = houseList[0].rockInfo.teamName;
                bool stopCounting = false;

                // lets loop the list
                for (int i = 0; i < houseList.Count; i++)
                {
                    if (!stopCounting)
                    {
                        // lets only count until the team changes
                        if (houseList[i].rockInfo.teamName == winningTeamName)
                        {
                            houseScore++;
                        }
                        else if(houseList[i].rockInfo.teamName != winningTeamName)
                        {
                            stopCounting = true;
                        }
                    }
                }
                bool noRocks = false;
                gHUD.CheckScore(noRocks, winningTeamName, houseScore);

                yield return new WaitForSeconds(1f);
            }
            else if (houseList.Count == 0)
            {
                bool noRocks = true;
                gHUD.CheckScore(noRocks, " ", 0);
                yield return new WaitForSeconds(1f);
            }

            yield return new WaitForFixedUpdate();

            NextTurn();
        }
    }

    void NextTurn()
    {
        Debug.Log("Next Turn");

        ++rockCurrent;

        if (rockCurrent % 2 == 1)
        {
            if (redHammer)
            {
                OnRedTurn();
            }
            else
            {
                OnYellowTurn();
            }
        }
        else if (rockCurrent % 2 == 0)
        {
            if (redHammer)
            {
                OnYellowTurn();
            }
            else
            {
                OnRedTurn();
            }
        }
    }

    IEnumerator Scoring()
    {
        Debug.Log("Current rock is " + rockCurrent);

        state = GameState.SCORE;
        Debug.Log("Scoring");

        if (redHammer)
        {
            Debug.Log("red hammer");
            redRock = rockList[1].rockInfo;
            yellowRock = rockList[0].rockInfo;
        }
        else
        {
            Debug.Log("not red hammer");
            redRock = rockList[0].rockInfo;
            yellowRock = rockList[1].rockInfo;
        }

        if (houseList.Count != 0)
        {
            Debug.Log("Rocks in house");
            houseList.Clear();

            foreach (Rock_List rock in rockList)
            {
                if (rock.rockInfo.inHouse)
                {
                    houseList.Add(new House_List(rock.rock, rock.rockInfo));
                }
            }

            yield return new WaitForFixedUpdate();

            houseList.Sort();

            yield return new WaitForFixedUpdate();
            int houseRock = houseList.Count;

            int houseScore = 0;
            string winningTeamName = houseList[0].rockInfo.teamName;
            bool stopCounting = false;

            // lets loop the list
            for (int i = 0; i < houseList.Count; i++)
            {
                if (!stopCounting)
                {
                    // lets only count until the team changes
                    if (houseList[i].rockInfo.teamName == winningTeamName)
                    {
                        houseScore++;
                    }
                    else if (houseList[i].rockInfo.teamName != winningTeamName)
                    {
                        stopCounting = true;
                    }
                }
            }

            if (redHammer)
            {
                gHUD.ScoringUI(redRock.teamName, winningTeamName, houseScore);
            }
            else
            {
                gHUD.ScoringUI(yellowRock.teamName, winningTeamName, houseScore);
            }

            if (winningTeamName == redRock.teamName)
            {
                redScore = redScore + houseScore;
                gHUD.Scoreboard(endCurrent, redScore, 0);
                redHammer = false;
                gHUD.SetHammer(redHammer);
            }
            else if (winningTeamName == yellowRock.teamName)
            {
                yellowScore = yellowScore + houseScore;
                gHUD.Scoreboard(endCurrent, 0, yellowScore);
                redHammer = true;
                gHUD.SetHammer(redHammer);
            }

            yield return new WaitForSeconds(1.5f);
            
            
        }
        else if (houseList.Count == 0)
        {
            if (redHammer)
            {
                redHammer = true;
                gHUD.Scoreboard(endCurrent, 0, 0);
                gHUD.ScoringUI(redRock.teamName, " ", 0);
                gHUD.SetHammer(redHammer);
            }
            else
            {
                redHammer = false;
                gHUD.Scoreboard(endCurrent, 0, 0);
                gHUD.ScoringUI(yellowRock.teamName, " ", 0);
                gHUD.SetHammer(redHammer);
            }
        }

        yield return new WaitForSeconds(1f);

        rockBar.EndUpdate(yellowScore, redScore);
        state = GameState.RESET;
        StartCoroutine(ResetGame());
    }

    public void OnDebug()
    {
        db.SetActive(false);

        GetComponent<Debug_Placement>().enabled = true;
        GetComponent<Debug_Shooting>().enabled = true;
        GetComponent<Debug_Random>().enabled = true;

        dbrandom.SetActive(true);
    }

    public void OnDebugReset()
    {
        StartCoroutine(Scoring());
    }

}
