using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;
using TigerForge;
using Photon.Pun;
using Lofelt.NiceVibrations;

public enum GameState { START, DRAWTOBUTTON, REDTURN, YELLOWTURN, CHECKSCORE, SCORE, RESET, END, DEBUG }


public class GameManager : MonoBehaviour
{
    #region Variables
    AudioManager am;
    public TutorialManager tm;
    public TeamManager teamM;
    public AIManager aim;
    public StoryManager storyM;

    public int endCurrent;
    public int endTotal;
    public bool redHammer;
    public int rocksPerTeam;
    public int rockTotal;
    public int rockCurrent;

    public GameSettingsPersist gsp;
    public RockManager rm;
    public GameObject redShooter;
    public GameObject yellowShooter;
    public GameObject shooterAnimRed;
    public GameObject shooterAnimYellow;
    public GameObject shooterAnimRedRock;
    public GameObject shooterAnimYellowRock;

    public SweeperManager sm;

    public GameObject shooterGO;
    public Transform launcher;
    public Transform yellowRocksInactive;
    public Transform redRocksInactive;
    public Collider2D boardCollider;
    public Collider2D launchCollider;

    public GameObject yellowSpin;
    public GameObject redSpin;

    public string redTeamName;
    public int redScore;
    public string yellowTeamName;
    public int yellowScore;

    public Vector2[] score;
    //public int[] endScoreRed;
    //public int[] endScoreYellow;

    public LoadGame lg;

    public GameHUD gHUD;
    public RockBar rockBar;

    public bool aiTeamRed;
    public bool aiTeamYellow;
    public bool target;
    public bool mixed;
    public bool multiplayer;
    public bool debug;
    public Text dbText;

    public GameState state;

    Rock_Info redRock;
    Rock_Info yellowRock;

    public DialogueManager coachGreen;

    public GameObject targetButtons;
    public GameObject targetAi;
    public GameObject targetPlayer;
    public GameObject targetStory;

    public Button redButton;
    public Button yellowButton;
    public GameObject db;
    public GameObject dbrandom;

    public CameraManager cm;
    public GameObject vcam_go;
    public CinemachineVirtualCamera vcam;
    Transform tFollowTarget;

    public ShootingKnob knob;
    public List<Rock_List> rockList;
    public List<House_List> houseList;
    public List<Guard_List> gList;

    EasyFileSave myFile;
    #endregion

    void Start()
    {
        state = GameState.START;

        //sweepButton.gameObject.SetActive(false);

        GameObject boards = GameObject.Find("BG/Boards_CREATED");
        boardCollider = boards.GetComponent<Collider2D>();
        myFile = new EasyFileSave("my_game_data");
        am = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        StartCoroutine(SetupGame());
    }

    #region "Setup and Reset"
    IEnumerator SetupGame()
    {
        //gHUD.SetHUD(redRock);
        Debug.Log("Game Start");
        gsp = FindObjectOfType<GameSettingsPersist>();

        endCurrent = gsp.endCurrent;
        rockCurrent = gsp.rockCurrent;
        rocksPerTeam = gsp.rocks;
        redHammer = gsp.redHammer;
        endTotal = gsp.ends;
        //rockTotal = 16;
        aiTeamYellow = gsp.aiYellow;
        aiTeamRed = gsp.aiRed;
        mixed = gsp.mixed;
        rockCurrent = 2 * (8 - gsp.rocks);
        score = new Vector2[endTotal + 1];

        if (gsp.redScore > 0 | gsp.yellowScore > 0)
        {
            redScore = gsp.redScore;
            yellowScore = gsp.yellowScore;
            cm.ui.enabled = false;
        }

        redTeamName = gsp.redTeamName;
        yellowTeamName = gsp.yellowTeamName;

        debug = gsp.debug;

        Debug.Log("redHammer is " + redHammer);
        am.Play("Theme");
        am.Play("Ambience");
        targetAi.SetActive(false);
        targetPlayer.SetActive(false);
        targetStory.SetActive(false);

        rockList = new List<Rock_List>();
        houseList = new List<House_List>();
        gList = new List<Guard_List>();
        //endScoreRed = new int[endTotal];
        //endScoreYellow = new int[endTotal];
        //yield return new WaitForSeconds(2f);
        yield return new WaitForEndOfFrame();

        gHUD.SetHammer(redHammer);

        cm.ui.enabled = true;

        if (endCurrent > 1)
        {
            for (int i = 0; i < endCurrent; i++)
            {
                //gHUD.ScoringPanel();
            }
        }
        if (gsp.loadGame)
        {
            StartCoroutine(LoadGame());
        }
        else if (gsp.story)
        {
            StartCoroutine(StoryLoad());
        }
        else
        {
            yield return StartCoroutine(SetupRocks());

            yield return new WaitUntil(() => rockList.Count == 16);

            if (rockCurrent > 0)
            {
                cm.HouseView();
                rockBar.ResetBar(redHammer);
                int tempRckCrnt = rockCurrent;
                for (int i = 0; i < rockCurrent; i++)
                {
                    //cm.TopView();
                    //Debug.Log("placed1 is " + rm.rrp.placed1);
                    rm.rrp.placed1 = false;
                    //Debug.Log("placed1 is " + rm.rrp.placed1);
                    if (gsp.aiRed)
                    {
                        rm.rrp.OnRockPlace(i, false);
                    }
                    else if (gsp.aiYellow)
                    {
                        rm.rrp.OnRockPlace(i, true);
                    }

                    if (i % 2 == 0)
                    {
                        if (redHammer)
                            rockBar.ActiveRock(false, i);
                        else
                            rockBar.ActiveRock(true, i);
                    }
                    else
                    {
                        if (redHammer)
                            rockBar.ActiveRock(true, i);
                        else
                            rockBar.ActiveRock(false, i);
                    }

                    //yield return new WaitForEndOfFrame();
                    rockBar.ShotUpdate(i, rockList[i].rockInfo.outOfPlay);
                    yield return new WaitUntil(() => rockBar.rockListUI.Count == 16);

                    yield return new WaitUntil(() => rm.rrp.placed1);

                    yield return new WaitForSeconds(0.25f);

                    gHUD.MainDisplayOff();
                    //yield return StartCoroutine(CheckScore());

                    //yield return new WaitUntil(() => !gHUD.panel.activeSelf);
                }
                rockCurrent--;

                rockBar.EndUpdate(yellowScore, redScore);
                //rockBar.ResetBar(redHammer);
                //rockBar.EndUpdate(yellowScore, redScore);
                //yield return new WaitUntil(() => rockBar.rockListUI.Count == 16);
                //yield return new WaitUntil(() => rm.rrp.placed1);
                //Debug.Log("Checking Score");
                //StartCoroutine(CheckScore());
                StartCoroutine(CheckScore());
            }
            else
            {
                rockBar.ResetBar(redHammer);
                rockBar.EndUpdate(yellowScore, redScore);
                yield return new WaitUntil(() => rockBar.rockListUI.Count == 16);
                rm.rrp.placed = true;
                if (redHammer)
                    OnYellowTurn();
                else
                    OnRedTurn();
            }
        }
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

        for (int i = 1; i <= 16; i++)
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.SoftImpact);
            if (i % 2 == notHammer)
            {
                GameObject yellowRock_go;
                if (multiplayer)
                {
                    yellowRock_go = PhotonNetwork.Instantiate(yellowShooter.gameObject.name, yellowRocksInactive.transform.position, Quaternion.identity, 0);
                    yellowRock_go.transform.parent = yellowRocksInactive;
                }
                else
                {
                    yellowRock_go = Instantiate(yellowShooter, yellowRocksInactive);
                }

                int yRocks = 4;
                int k = (i / 2) + notHammer;

                if (k <= yRocks)
                {
                    yellowRock_go.transform.position = new Vector2(yellowRock_go.transform.position.x, yellowRock_go.transform.position.y - ((k - 1) * 0.4f));
                }
                else if (k > yRocks)
                {
                    float j = k - yRocks;
                    yellowRock_go.transform.position = new Vector2(yellowRock_go.transform.position.x - 0.4f, yellowRock_go.transform.position.y - ((j - 1) * 0.4f));
                }

                Rock_Info yellowRock_info = yellowRock_go.GetComponent<Rock_Info>();
                yellowRock_info.rockNumber = k;
                yellowRock_info.rockIndex = i - 1;
                yellowRock_info.teamName = yellowTeamName;
                yellowRock_go.name = yellowTeamName + " " + yellowRock_info.rockNumber;
                yellowRock_go.GetComponent<Rock_Flick>().enabled = false;
                yellowRock_go.GetComponent<Rock_Release>().enabled = false;
                yellowRock_go.GetComponent<Rock_Force>().enabled = false;
                yellowRock_go.GetComponent<CircleCollider2D>().enabled = false;
                rockList.Add(new Rock_List(yellowRock_go, yellowRock_info));
                yield return new WaitForSeconds(0.025f);
            }
            if (i % 2 == hammer)
            {
                GameObject redRock_go;
                if (multiplayer)
                {
                    redRock_go = PhotonNetwork.Instantiate(redShooter.gameObject.name, redRocksInactive.transform.position, Quaternion.identity, 0);
                    redRock_go.transform.parent = redRocksInactive;
                }
                else
                {
                    redRock_go = Instantiate(redShooter, redRocksInactive);
                }
                int yRocks = 4;
                int k = (i / 2) + hammer;
                if (k <= yRocks)
                {
                    redRock_go.transform.position = new Vector2(redRock_go.transform.position.x, redRock_go.transform.position.y - ((k - 1) * 0.4f));
                }
                else if (k > yRocks)
                {
                    float j = k - yRocks;
                    redRock_go.transform.position = new Vector2(redRock_go.transform.position.x + 0.4f, redRock_go.transform.position.y - ((j - 1) * 0.4f));
                }


                Rock_Info redRock_info = redRock_go.GetComponent<Rock_Info>();
                redRock_info.rockNumber = k;
                redRock_info.rockIndex = i - 1;
                redRock_info.teamName = redTeamName;
                redRock_go.name = redTeamName + " " + redRock_info.rockNumber;

                redRock_go.GetComponent<CircleCollider2D>().enabled = false;
                redRock_go.GetComponent<Rock_Flick>().enabled = false;
                redRock_go.GetComponent<Rock_Release>().enabled = false;
                redRock_go.GetComponent<Rock_Force>().enabled = false;
                rockList.Add(new Rock_List(redRock_go, redRock_info));
                yield return new WaitForSeconds(0.025f);
            }
            //rockList.Sort();
        }


        //scoreboard.SetActive(false);

    }
    IEnumerator ResetGame()
    {
        gList.Clear();
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
        rockCurrent = 2 * (8 - rocksPerTeam);
        gHUD.SetHammer(redHammer);

        yield return StartCoroutine(SetupRocks());
        yield return new WaitUntil(() => rockList.Count == 16);

        if (rockCurrent > 0)
        {
            if (gsp.aiRed)
            {
                rm.rrp.OnRockPlace(rockCurrent, false);
            }
            else
            {
                rm.rrp.OnRockPlace(rockCurrent, true);
            }
            yield return new WaitUntil(() => rm.rrp.placed);
            rockBar.ResetBar(redHammer);
            yield return new WaitUntil(() => rockBar.rockListUI.Count == 16);
            //if (redHammer)
            //    OnYellowTurn();
            //else
            //    OnRedTurn();
            //StartCoroutine(SaveGame());
            yield return new WaitUntil(() => rm.rrp.placed);
            StartCoroutine(CheckScore());
        }
        else
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
    #endregion

    #region Turns
    public void OnRedTurn()
    {
            cm.ShotSetup();

        state = GameState.REDTURN;
        //Debug.Log("Red Turn");

        if (GameObject.FindGameObjectsWithTag("Player").Length >= 1)
        {
            Destroy(GameObject.FindGameObjectWithTag("Player"));
        }

        teamM.SetCharacter(rockCurrent, true);

        shooterGO = Instantiate(shooterAnimRed);
        Instantiate(shooterAnimRedRock, shooterGO.transform);
        sm.SetupSweepers(true);


        GameObject redRock_1 = rockList[rockCurrent].rock;
        redRock_1.GetComponent<Rock_Flick>().enabled = true;
        redRock_1.GetComponent<Rock_Release>().enabled = true;
        redRock_1.GetComponent<Rock_Force>().enabled = true;
        redRock_1.GetComponent<Rock_Colliders>().enabled = true;
        boardCollider.enabled = false;
        launchCollider.enabled = false;
        //StartCoroutine(SaveGame());
        StartCoroutine(RedTurn());
    }

    IEnumerator RedTurn()
    {

        rm.inturn = false;
        
        GameObject redRock_1 = rockList[rockCurrent].rock;

        redRock = redRock_1.GetComponent<Rock_Info>();
        Debug.Log(redRock_1.name);

        //Debug.Log("Current Rock is " + rockCurrent);
        rockBar.ActiveRock(true, rockCurrent);

        if (debug)
        {
            dbText.enabled = true;
        }

        if (aiTeamRed)
        {
            gHUD.Message(redTeamName + " Turn");

            yield return new WaitForSeconds(1f);
            aim.OnShot(rockCurrent);
        }
        else if (target)
        {
            cm.HouseView();
            targetPlayer.SetActive(true);
            targetButtons.SetActive(true);
            if (debug)
            {
                dbText.enabled = true;
            }
        }

        yield return new WaitUntil(() => redRock.shotTaken == true);

        if (target)
        {
            targetButtons.SetActive(false);
            targetPlayer.SetActive(false);
        }
        
        //am.Play("RockScrape");
        cm.RockFollow(redRock_1.transform);
        boardCollider.enabled = true;

        rockBar.ActiveRock(true, rockCurrent);

        yield return new WaitUntil(() => redRock.released == true);

        if (aiTeamRed)
        {
            gHUD.mainDisplay.enabled = false;
            targetAi.SetActive(false);
        }
        redRock_1.GetComponent<Rock_Flick>().enabled = false;
        sm.Release(redRock_1, aiTeamRed);
        rm.GetComponent<Sweep>().EnterSweepZone();

        yield return new WaitUntil(() => redRock.rest == true);
        if (debug)
        {
            dbText.enabled = false;
        }
        //am.Stop("RockScrape");
        rm.GetComponent<Sweep>().ExitSweepZone();

        Debug.Log("redRock at Rest");
        vcam.enabled = false;

        StartCoroutine(AllStopped());

        if (!redRock.outOfPlay)
        {
            if (redRock.inPlay && !redRock.inHouse)
            {
                if (redRock_1.transform.position.y <= 6.5f)
                {
                    redRock.freeGuard = true;

                    gList.Add(new Guard_List(rockCurrent, redRock.freeGuard, redRock_1.transform));
                }
            }
            rockBar.IdleRock(rockCurrent);
        }
        else
        {
            rockBar.DeadRock(rockCurrent);
        }

        StartCoroutine(CheckScore());
    }

    public void OnYellowTurn()
    {
        state = GameState.YELLOWTURN;
        Debug.Log("Yellow Turn");

        cm.ShotSetup();

        if (GameObject.FindGameObjectsWithTag("Player").Length >= 1)
        {
            Destroy(GameObject.FindGameObjectWithTag("Player"));
        }

        teamM.SetCharacter(rockCurrent, false);
        shooterGO = Instantiate(shooterAnimYellow);

        Instantiate(shooterAnimYellowRock, shooterGO.transform);

        GameObject yellowRock_1 = rockList[rockCurrent].rock;
        yellowRock_1.GetComponent<Rock_Flick>().enabled = true;
        yellowRock_1.GetComponent<Rock_Release>().enabled = true;
        yellowRock_1.GetComponent<Rock_Force>().enabled = true;
        yellowRock_1.GetComponent<Rock_Colliders>().enabled = true;
        boardCollider.enabled = false;
        launchCollider.enabled = false;

        sm.SetupSweepers(false);

        StartCoroutine(YellowTurn());
    }

    IEnumerator YellowTurn()
    {
        GameObject yellowRock_1 = rockList[rockCurrent].rock;

        rm.inturn = false;
        //Debug.Log("rmInturn is " + rm.inturn);

        yellowRock = yellowRock_1.GetComponent<Rock_Info>();
        Debug.Log(yellowRock_1.name);

        rockBar.ActiveRock(false, rockCurrent);
        if (debug)
        {
            dbText.enabled = true;
        }
        if (aiTeamYellow)
        {
            gHUD.Message(yellowTeamName + " Turn");

            if (debug)
            {
                dbText.enabled = true;
            }

            yield return new WaitForSeconds(1f);
            aim.OnShot(rockCurrent);
        }
        else if (target)
        {
            cm.HouseView();
            targetPlayer.SetActive(true);
            targetButtons.SetActive(true);
        }

        yield return new WaitUntil(() => yellowRock.shotTaken == true);

        if (target)
        {
            targetButtons.SetActive(false);
            targetPlayer.SetActive(false);
        }
        
        cm.RockFollow(yellowRock_1.transform);
        //am.Play("RockScrape");
        boardCollider.enabled = true;
        rockBar.ActiveRock(false, rockCurrent);

        yield return new WaitUntil(() => yellowRock.released == true);

        sm.Release(yellowRock_1, aiTeamYellow);

        if (aiTeamYellow)
        {
            gHUD.mainDisplay.enabled = false;
            targetAi.SetActive(false);
        }

        yield return new WaitUntil(() => yellowRock.rest == true);

        if (debug)
        {
            dbText.enabled = false;
        }
        //am.Stop("RockScrape");

        if (debug)
        {
            dbText.enabled = false;
        }

        rm.GetComponent<Sweep>().ExitSweepZone();

        vcam.enabled = false;

        yield return new WaitForEndOfFrame();

        if (!yellowRock.outOfPlay)
        {
            if (yellowRock.inPlay & !yellowRock.inHouse)
            {
                if (yellowRock_1.transform.position.y <= 6.5f)
                {
                    if (rockCurrent < 6)
                    {
                        yellowRock.freeGuard = true;
                    }

                    gList.Add(new Guard_List(rockCurrent, yellowRock.freeGuard, yellowRock_1.transform));

                }
            }

            Debug.Log("Out Of Play is " + yellowRock.outOfPlay);
            Debug.Log("Rock Current is " + rockCurrent);
            rockBar.IdleRock(rockCurrent);
        }
        else
        {
            Debug.Log("Out Of Play is " + yellowRock.outOfPlay);
            Debug.Log("Rock Current is " + rockCurrent);
            rockBar.DeadRock(rockCurrent);
        }

        StartCoroutine(CheckScore());

    }
    #endregion

    #region End of Turn
    IEnumerator AllStopped()
    {
        foreach (Rock_List rock in rockList)
        {
            Rigidbody2D rockRB = rock.rock.GetComponent<Rigidbody2D>();
            if (Mathf.Abs(rockRB.velocity.y) > 0.01f && Mathf.Abs(rockRB.velocity.x) > 0.01f)
            {
                //Debug.Log(rock.rockInfo.teamName + " " + rock.rockInfo.rockNumber + " is still moving :(");
                yield return new WaitUntil(() => Mathf.Abs(rockRB.velocity.y) < 0.01f && Mathf.Abs(rockRB.velocity.x) < 0.01f);
            }
        }
        yield return new WaitForEndOfFrame();
    }

    IEnumerator CheckScore()
    {
        state = GameState.CHECKSCORE;


        //Debug.Log("Check Score");

        yield return StartCoroutine(AllStopped());

        Debug.Log("All Stopped");
        yield return new WaitForFixedUpdate();

        rockBar.ShotUpdate(rockCurrent, rockList[rockCurrent].rockInfo.outOfPlay);

        houseList.Clear();
        gList.Clear();

        Destroy(shooterGO);
        cm.HouseView();

        Debug.Log("Rock List is " + rockList.Count);
        foreach (Rock_List rock in rockList)
        {
            if (rock.rockInfo.inHouse == true && rock.rockInfo.inPlay)
            {
                houseList.Add(new House_List(rock.rock, rock.rockInfo));
                //Debug.Log("Adding House " + rock.rockInfo.teamName + rock.rockInfo.rockNumber + " - " + rock.rockInfo.rockIndex);
            }
            if (rock.rockInfo.inPlay && !rock.rockInfo.inHouse && rock.rock.transform.position.y <= 6.5f)
            {
                gList.Add(new Guard_List(rockCurrent, rock.rockInfo.freeGuard, rock.rock.transform));
                //Debug.Log("Guard " + rock.rockInfo.name + " - " + rock.rockInfo.distance);
            }
        }

        //yield return new WaitForFixedUpdate();

        houseList.Sort();
        // if the we have shot all the rocks, go to the final scoring
        if (rockList.Count == rockCurrent + 1)
        {
            //Debug.Log("Rock List " + rockList.Count + " equals " + "Current Rock " + (rockCurrent + 1));
            StartCoroutine(Scoring());
        }
        // or else we will just check the score
        else
        {
            if (houseList.Count != 0)
            {
                int counter = 0;
                foreach (House_List rock in houseList)
                {
                    counter++;
                    Debug.Log("House List " + counter + " - " + rock.rockInfo.name + " - " + rock.rockInfo.distance);
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

                //send the info to the ui
                gHUD.CheckScore(noRocks, winningTeamName, houseScore, false);

            }
            // if the house is empty move along to next turn
            else if (houseList.Count == 0)
            {
                bool noRocks = true;
                gHUD.CheckScore(noRocks, " ", 0, false);
            }

            yield return StartCoroutine(WaitForClick());
            gHUD.clickDisplay.enabled = false;
            gHUD.MainDisplayOff();
            gHUD.ScoreboardOff();

            StartCoroutine(SaveGame(true));
            //Debug.Log("Current Rock is " + rockCurrent);
            NextTurn();
        }
    }

    public void NextTurn()
    {
        Debug.Log("Next Turn");

        rockCurrent++;
        //Debug.Log("Current Rock is " + rockCurrent);

        //gsp.AutoSave();
        if (rm.rrp.placed)
        {
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
        else
        {
            if (gsp.aiRed)
            {
                rm.rrp.OnRockPlace(rockCurrent, false);
            }
            else
            {
                rm.rrp.OnRockPlace(rockCurrent, true);
            }
        }
    }
    #endregion

    #region Scoring and End of Game
    IEnumerator Scoring()
    {
        Debug.Log("Current rock is " + rockCurrent);

        state = GameState.SCORE;
        Debug.Log("Scoring");

        if (redHammer)
        {
            //Debug.Log("red hammer");
            redRock = rockList[1].rockInfo;
            yellowRock = rockList[0].rockInfo;
        }
        else
        {
            //Debug.Log("not red hammer");
            redRock = rockList[0].rockInfo;
            yellowRock = rockList[1].rockInfo;
        }

        if (houseList.Count != 0)
        {
            Debug.Log("Rocks in house");
            houseList.Clear();

            foreach (Rock_List rock in rockList)
            {
                if (rock.rockInfo.inHouse && rock.rockInfo.inPlay)
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
                gHUD.ScoringUI(redTeamName, winningTeamName, houseScore);
            }
            else
            {
                gHUD.ScoringUI(yellowTeamName, winningTeamName, houseScore);
            }

            if (gsp.score.Length < 1)
                gsp.score = new Vector2Int[endTotal + 1];
            

            if (winningTeamName == redTeamName)
            {
                gsp.score[endCurrent] = new Vector2Int(houseScore, 0);
                redScore += houseScore;
                //gHUD.ScoringPanel();
                redHammer = false;
                gHUD.SetHammer(redHammer);
            }
            if (winningTeamName == yellowTeamName)
            {
                gsp.score[endCurrent] = new Vector2Int(0, houseScore);
                yellowScore += houseScore;
                //gHUD.ScoringPanel();
                redHammer = true;
                gHUD.SetHammer(redHammer);
            }

            //endScoreRed[endCurrent] = redScore;
            //endScoreYellow[endCurrent] = yellowScore;

            yield return new WaitForSeconds(1.5f);

        }
        else if (houseList.Count == 0)
        {
            if (redHammer)
            {
                redHammer = true;
                //gHUD.ScoringPanel();
                gHUD.ScoringUI(redTeamName, " ", 0);
                gHUD.SetHammer(redHammer);
            }
            else
            {
                redHammer = false;
                //gHUD.ScoringPanel();
                gHUD.ScoringUI(yellowTeamName, " ", 0);
                gHUD.SetHammer(redHammer);
            }
        }

        rockBar.EndUpdate(yellowScore, redScore);

        yield return StartCoroutine(WaitForClick());


        if (endCurrent < endTotal)
        {
            gHUD.ScoreboardOff();
            gHUD.MainDisplayOff();
            state = GameState.RESET;

                rockCurrent = gsp.rockCurrent;
                gsp.LoadFromGM();
                endCurrent++;
                gsp.endCurrent = endCurrent;
                yield return StartCoroutine(SaveGame(true));
                //yield return StartCoroutine(SaveGame());
                yield return new WaitForEndOfFrame();
                SceneManager.LoadScene("End_menu_Tourny_1");
                //StartCoroutine(ResetGame());
        }
        else if (endCurrent >= endTotal)
        {
            state = GameState.END;
            yield return StartCoroutine(SaveGame(false));
            StartCoroutine(EndOfGame());
        }
    }

    IEnumerator EndOfGame()
    {
        gHUD.EndOfGame(redScore, redTeamName, yellowScore, yellowTeamName);
        
        yield return new WaitForSeconds(2f);

        if (gsp.tourny)
        {
            endCurrent++;

            gsp.LoadFromGM();

            SceneManager.LoadScene("End_Menu_Tourny_1");
        }
        else
        {
            endCurrent++;
            gsp.LoadFromGM();
            SceneManager.LoadScene("End_Menu_1");
        }
    }
    #endregion

    #region Utilities and Special Situations
    IEnumerator WaitForClick()
    {
        while (!Input.GetMouseButtonDown(0))
        {
            yield return null;
        }
        //Debug.Log("Clickeddd");
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

    IEnumerator SaveGame(bool inProgress)
    {
        myFile = new EasyFileSave("my_game_data");
        myFile.Add("In Progress", inProgress);
        myFile.Add("Red Hammer", redHammer);
        myFile.Add("End Total", endTotal);
        myFile.Add("Current End", endCurrent);
        myFile.Add("Rock Total", rockTotal);
        myFile.Add("Rocks", rocksPerTeam);
        myFile.Add("Current Rock", rockCurrent);
        myFile.Add("Red Score", redScore);
        myFile.Add("Yellow Score", yellowScore);
        myFile.Add("Ai Yellow", aiTeamYellow);
        myFile.Add("Team", target);
        myFile.Add("Debug", debug);
        Debug.Log("endTotal is " + endTotal);

        myFile.Add("End " + endCurrent + " Score", new Vector2Int(redScore, yellowScore));

        int[] redScoreList = new int[score.Length];
        int[] yellowScoreList = new int[score.Length];

        for (int i = 0; i < endCurrent; i++)
        {
            redScoreList[i] = gsp.score[i].x;
            yellowScoreList[i] = gsp.score[i].y;
        }
        myFile.Add("Red Score List", redScoreList);
        myFile.Add("Yellow Score List", yellowScoreList);

       
        for (int i = 1; i < rockTotal; i++)
        {
            myFile.Add("Rock Position " + rockList[i].rockInfo.rockIndex, new Vector2(rockList[i].rock.transform.position.x, rockList[i].rock.transform.position.y));
            myFile.Add("Rock In Play " + rockList[i].rockInfo.rockIndex, rockList[i].rockInfo.inPlay);
        }

        yield return new WaitForEndOfFrame();

        myFile.Append();
    }

    IEnumerator LoadGame()
    {
        if (myFile.Load())
        {
            Debug.Log("Loading Game!!!!!!!");
            redHammer = myFile.GetBool("Red Hammer");
            endTotal = myFile.GetInt("End Total");
            endCurrent = myFile.GetInt("Current End");
            //rockTotal = myFile.GetInt("Rocks Per Team") * 2;
            aiTeamRed = myFile.GetBool("Ai Red");
            aiTeamYellow = myFile.GetBool("Ai Yellow");
            redScore = myFile.GetInt("Red Score");
            yellowScore = myFile.GetInt("Yellow Score");
            rocksPerTeam = myFile.GetInt("Rocks");

            int[] redScoreList = myFile.GetArray<int>("Red Score List");
            int[] yellowScoreList = myFile.GetArray<int>("Yellow Score List");

            for (int i = 0; i < redScoreList.Length; i++)
            {
                score[i].x = redScoreList[i];
                score[i].y = yellowScoreList[i];
            }

            for (int i = 1; i < endCurrent; i++)
            {
                gHUD.ScoringPanel();
            }
        }

        yield return StartCoroutine(SetupRocks());

        yield return new WaitForEndOfFrame();

        rockBar.ResetBar(redHammer);
        rockBar.EndUpdate(yellowScore, redScore);
        //rockBar.EndUpdate(yellowScore, redScore);
        //yield return StartCoroutine(WaitForClick());
        if (myFile.Load())
            rockCurrent = myFile.GetInt("Current Rock");
        //lg.enabled = true;

        //yield return new WaitUntil(() => lg.rocksPlaced == true);
        Debug.Log("Current Rock is " + rockCurrent);

        //yield return StartCoroutine(WaitForClick());
        gsp.loadGame = false;
        yield return new WaitUntil(() => rockBar.rockListUI.Count == 16);

        if (rockCurrent > 0)
        {
            yield return StartCoroutine(PlaceRocks());

            //if (myFile.Load())
            //{
                //rockCurrent = myFile.GetInt("Current Rock");
                //if (rockCurrent < 0)
                //{
                //    rockCurrent = 0;
                //}
            //}
        }
            

        myFile.Dispose();


        yield return StartCoroutine(CheckScore());
    }

    IEnumerator StoryLoad()
    {
        Debug.Log("Story Game!!!!!!!");
        redHammer = storyM.redHammer;
        endTotal = storyM.ends;
        endCurrent = storyM.endCurrent;
        rockTotal = storyM.rocks * 2;
        aiTeamRed = storyM.aiRed;
        aiTeamYellow = storyM.aiYellow;

        //gHUD.Scoreboard(3, 3, 0);
        //gHUD.Scoreboard(4, 0, 2);
        //gHUD.Scoreboard(6, 5, 0);
        //gHUD.Scoreboard(8, 6, 0);

        redScore = storyM.redScore;
        yellowScore = storyM.yellowScore;
        rockBar.EndUpdate(yellowScore, redScore);

        yield return StartCoroutine(SetupRocks());

        rockBar.ResetBar(redHammer);
        yield return new WaitForEndOfFrame();
            rockCurrent = storyM.rockCurrent - 1;

        Debug.Log("Current Rock is " + rockCurrent);
        for (int i = 0; i <= rockCurrent; i++)
        {
            rockList[i].rockInfo.placed = true;
        }

        yield return new WaitForEndOfFrame();

        for (int i = 0; i <= rockCurrent; i++)
        {
            rockList[i].rock.GetComponent<CircleCollider2D>().radius = 0.14f;
            rockList[i].rock.GetComponent<SpriteRenderer>().enabled = true;
            rockList[i].rock.GetComponent<SpringJoint2D>().enabled = false;
            rockList[i].rock.GetComponent<Rock_Flick>().enabled = false;
            rockList[i].rock.transform.parent = null;
            //rockBar.DeadRock(i);
            yield return new WaitForEndOfFrame();
        }

        for (int i = 0; i <= rockCurrent; i++)
        {
            Vector2 rockTrans = storyM.rockPos[i];
            Debug.Log("Rock Position " + i + " " + rockTrans.x + ", " + rockTrans.y);
            rockList[i].rock.GetComponent<Rigidbody2D>().position = rockTrans;

            if (storyM.rockPos[i].y >= 8f)
            {
                rockList[i].rock.SetActive(false);
                rockList[i].rockInfo.inPlay = false;
                rockList[i].rockInfo.outOfPlay = true;
                //rockBar.DeadRock(i);
            }
            else
            {
                rockList[i].rock.GetComponent<CircleCollider2D>().enabled = true;
                rockList[i].rock.GetComponent<Rock_Release>().enabled = true;
                rockList[i].rock.GetComponent<Rock_Force>().enabled = true;
                rockList[i].rock.GetComponent<Rock_Colliders>().enabled = true;
                rockList[i].rockInfo.inPlay = true;
                rockList[i].rockInfo.outOfPlay = false;
                rockList[i].rockInfo.moving = false;
                rockList[i].rockInfo.shotTaken = true;
                rockList[i].rockInfo.released = true;
                rockList[i].rockInfo.stopped = true;
                rockList[i].rockInfo.rest = true;
                Debug.Log("i is equal to " + i);

                yield return new WaitForEndOfFrame();
            }
            
        }

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < rockCurrent; i++)
        {

        }
        //rocksPlaced = true;

        yield return new WaitForEndOfFrame();

        yield return StartCoroutine(CheckScore());
    }

    IEnumerator PlaceRocks()
    {
        //yield return new WaitForSeconds(3.5f);

        for (int i = 0; i <= rockCurrent; i++)
        {
            rockList[i].rockInfo.placed = true;
        }

        yield return new WaitForEndOfFrame();

        for (int i = 0; i <= rockCurrent; i++)
        {
            rockList[i].rock.GetComponent<CircleCollider2D>().radius = 0.14f;
            rockList[i].rock.GetComponent<SpriteRenderer>().enabled = true;
            rockList[i].rock.GetComponent<SpringJoint2D>().enabled = false;
            rockList[i].rock.GetComponent<Rock_Flick>().enabled = false;
            rockList[i].rock.transform.parent = null;
            //rockBar.DeadRock(i);
            yield return new WaitForEndOfFrame();
            if (myFile.Load())
            {
                if (myFile.GetBool("Rock In Play " + i))
                {
                    Vector2 rockTrans = myFile.GetUnityVector2("Rock Position " + i);
                    Debug.Log("Placing Rock Position " + i + " " + rockTrans.x + ", " + rockTrans.y);
                    rockList[i].rock.GetComponent<Rigidbody2D>().position = rockTrans;

                    rockList[i].rock.GetComponent<CircleCollider2D>().enabled = true;
                    rockList[i].rock.GetComponent<Rock_Release>().enabled = true;
                    rockList[i].rock.GetComponent<Rock_Force>().enabled = true;
                    rockList[i].rock.GetComponent<Rock_Colliders>().enabled = true;
                    rockList[i].rockInfo.inPlay = true;
                    rockList[i].rockInfo.outOfPlay = false;
                    rockList[i].rockInfo.moving = false;
                    rockList[i].rockInfo.shotTaken = true;
                    rockList[i].rockInfo.released = true;
                    rockList[i].rockInfo.stopped = true;
                    rockList[i].rockInfo.rest = true;
                    Debug.Log("i is equal to " + i);
                }
                else
                {
                    rockList[i].rock.SetActive(false);
                    rockList[i].rockInfo.inPlay = false;
                    rockList[i].rockInfo.outOfPlay = true;

                }

            }

            //rockBar.ShotUpdate(rockCurrent, rockList[i].rockInfo.outOfPlay);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForEndOfFrame();


    }

    #endregion
}
