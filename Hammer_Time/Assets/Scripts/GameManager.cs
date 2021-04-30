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

    public GameObject redShooter;
    public GameObject yellowShooter;
    public GameObject shooterAnim;
    GameObject shooterGO;

    public Transform launcher;
    public Transform yellowRocksInactive;
    public Transform redRocksInactive;
    public Collider2D boardCollider;
    public Collider2D launchCollider;

    public Rock_Traj trajectory;

    int redRocks_left;
    int yellowRocks_left;
    public int redScore;
    public int yellowScore;
    //public GameHUD gHUD;

    public GameObject debug;

    public GameState state;

    Rock_Info redRock;
    Rock_Info yellowRock;

    public Text mainDisplay;
    public Text redTurn_Display;
    public Text yellowTurn_Display;
    public Text redRocksLeft_Display;
    public Text yellowRocksLeft_Display;
    public Slider redRocksLeft_Slider;
    public Slider yellowRocksLeft_Slider;
    public Button redButton;
    public Button yellowButton;
    public Button sweepButton;
    public GameObject db;
    public GameObject dbrandom;

    public GameObject vcam_go;
    public CinemachineVirtualCamera vcam;
    Transform tFollowTarget;

    public List<Rock_List> rockList;
    public List<House_List> houseList;

    void Start()
    {
        state = GameState.START;
        redButton.gameObject.SetActive(false);
        yellowButton.gameObject.SetActive(false);
        sweepButton.gameObject.SetActive(false);

        redTurn_Display.enabled = false;
        yellowTurn_Display.enabled = false;
        redRocksLeft_Display.enabled = false;
        yellowRocksLeft_Display.enabled = false;
        redRocksLeft_Slider.enabled = false;
        yellowRocksLeft_Slider.enabled = false;
        boardCollider.enabled = false;

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

        dbrandom.SetActive(false);

        redButton.gameObject.SetActive(true);
        yellowButton.gameObject.SetActive(true);
        redRocksLeft_Display.enabled = true;
        yellowRocksLeft_Display.enabled = true;
        redRocksLeft_Slider.enabled = true;
        yellowRocksLeft_Slider.enabled = true;

        mainDisplay.text = "Hammer?";
        redRocksLeft_Display.text = redRocks_left + " Rocks Left";
        yellowRocksLeft_Display.text = yellowRocks_left + " Rocks Left";

    }

    public void SetHammerRed()
    {
        redHammer = true;
        db.SetActive(false);
        mainDisplay.gameObject.SetActive(false);
        redButton.gameObject.SetActive(false);
        yellowButton.gameObject.SetActive(false);

        StartCoroutine(SetupRocks());
        OnYellowTurn();
    }

    public void SetHammerYellow()
    {
        redHammer = false;
        db.SetActive(false);
        mainDisplay.gameObject.SetActive(false);
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
    }

    IEnumerator ResetGame()
    {
        mainDisplay.gameObject.SetActive(true);

        yield return new WaitForFixedUpdate();
        
        mainDisplay.text = redScore + " - " + yellowScore;

        yield return new WaitForSeconds(3f);

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

        yield return new WaitForSeconds(2f);

        mainDisplay.text = "End " + endCurrent;

        redRocks_left = rocksPerTeam;
        yellowRocks_left = rocksPerTeam;
        redRocksLeft_Display.text = redRocks_left + " Rocks Left";
        yellowRocksLeft_Display.text = yellowRocks_left + " Rocks Left";
        rockCurrent = 0;


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
        //shooterGO = Instantiate(shooterAnim);

        Debug.Log("Red Turn");
        state = GameState.REDTURN;

        vcam = vcam_go.GetComponent<CinemachineVirtualCamera>();

        tFollowTarget = launcher.transform;
        vcam.LookAt = tFollowTarget;
        vcam.Follow = tFollowTarget;
        vcam.enabled = true;

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

        GameObject redRock_1 = rockList[rockCurrent].rock;

        redRock = redRock_1.GetComponent<Rock_Info>();
        Debug.Log(redRock_1.name);

        redTurn_Display.enabled = true;
        redTurn_Display.text = redRock_1.name;
        redRocksLeft_Display.text = redRocks_left + " Rocks Left";
        redRocksLeft_Slider.maxValue = rocksPerTeam;
        redRocksLeft_Slider.value = redRock.rockNumber;

        yield return new WaitUntil(() => redRock.shotTaken == true);

        boardCollider.enabled = true;

        yield return new WaitUntil(() => redRock.released == true);
        sweepButton.gameObject.SetActive(true);

        yield return new WaitUntil(() => redRock.rest == true);
        Debug.Log("redRock at Rest");
        redTurn_Display.enabled = false;
        vcam.enabled = false;
        sweepButton.gameObject.SetActive(false);

        StartCoroutine(CheckScore());
    }

    public void OnYellowTurn()
    {
        //shooterGO = Instantiate(shooterAnim);

        Debug.Log("Yellow Turn");
        state = GameState.YELLOWTURN;

        vcam = vcam_go.GetComponent<CinemachineVirtualCamera>();

        tFollowTarget = launcher.transform;
        vcam.LookAt = tFollowTarget;
        vcam.Follow = tFollowTarget;
        vcam.enabled = true;

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

        GameObject yellowRock_1 = rockList[rockCurrent].rock;

        yellowRock = yellowRock_1.GetComponent<Rock_Info>();
        Debug.Log(yellowRock_1.name);

        redTurn_Display.enabled = false;
        yellowTurn_Display.text = yellowRock_1.name;
        yellowRocksLeft_Display.text = yellowRocks_left + " Rocks Left";
        yellowRocksLeft_Slider.maxValue = rocksPerTeam;
        yellowRocksLeft_Slider.value = yellowRock.rockNumber;

        yield return new WaitUntil(() => yellowRock.shotTaken == true);

        boardCollider.enabled = true;

        yield return new WaitUntil(() => yellowRock.released == true);
        sweepButton.gameObject.SetActive(true);

        yield return new WaitUntil(() => yellowRock.rest == true);

        yellowTurn_Display.enabled = false;
        vcam.enabled = false;
        sweepButton.gameObject.SetActive(false);

        StartCoroutine(AllStopped());

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
        yellowTurn_Display.enabled = false;
        redTurn_Display.enabled = false;
        mainDisplay.text = " ";
        mainDisplay.gameObject.SetActive(true);

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
            Debug.Log(rockList.Count + " equals " + (rockCurrent + 1));
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

            yield return new WaitForSeconds(0.5f);

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


                mainDisplay.text = winningTeamName + " is sitting " + houseScore;
                yield return new WaitForSeconds(1.5f);
            }
            else if (houseList.Count == 0)
            {
                mainDisplay.text = "No Rocks In House";
                yield return new WaitForSeconds(1.5f);
            }

            yield return new WaitForFixedUpdate();

            NextTurn();

            yield return new WaitForSeconds(1f);
            mainDisplay.gameObject.SetActive(false);
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
        yellowTurn_Display.enabled = false;
        redTurn_Display.enabled = false;
        mainDisplay.gameObject.SetActive(true);

        if (redHammer)
        {
            redRock = rockList[1].rockInfo;
            yellowRock = rockList[0].rockInfo;
        }
        else
        {
            redRock = rockList[0].rockInfo;
            yellowRock = rockList[1].rockInfo;
        }

        if (houseList.Count != 0)
        {
            Debug.Log("house list not null");
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
                        else if (houseList[i].rockInfo.teamName != winningTeamName)
                        {
                            stopCounting = true;
                        }
                    }
                }

                if (winningTeamName != rockList[1].rockInfo.teamName)
                {
                    mainDisplay.text = winningTeamName + " stole " + houseScore;
                }
                else
                {
                    mainDisplay.text = winningTeamName + " scored " + houseScore;
                }

                if (winningTeamName == redRock.teamName)
                {
                    redScore = redScore + houseScore;
                }
                else if (winningTeamName == yellowRock.teamName)
                {
                    yellowScore = yellowScore + houseScore;
                }

                yield return new WaitForSeconds(1.5f);
            }
            else if (houseList.Count == 0)
            {
                mainDisplay.text = "No Rocks In House";
                yield return new WaitForSeconds(1.5f);
            }
            //    foreach (House_List rock in houseList)
            //    {
            //        Debug.Log(rock.rockInfo.name + " - " + rock.rockInfo.distance);
            //    }


            //    if (redHammer)
            //    {
            //        redRock = rockList[1].rockInfo;
            //        yellowRock = rockList[0].rockInfo;
            //    }
            //    else
            //    {
            //        redRock = rockList[0].rockInfo;
            //        yellowRock = rockList[1].rockInfo;
            //    }

            //    int houseRocks = houseList.Count;


            //    if (houseList[0].rockInfo.teamName == redRock.teamName)
            //    {
            //        RedScore();

            //        mainDisplay.gameObject.SetActive(true);

            //        if (redHammer)
            //        {
            //            mainDisplay.text = redRock.teamName + " scored " + redScore;
            //            redHammer = false;
            //        }
            //        else
            //        {
            //            mainDisplay.text = redRock.teamName + " stole " + redScore;
            //            redHammer = false;
            //        }
            //    }

            //    if (houseList[0].rockInfo.teamName == yellowRock.teamName)
            //    {
            //        YellowScore();

            //        mainDisplay.gameObject.SetActive(true);

            //        if (redHammer)
            //        {
            //            mainDisplay.text = yellowRock.teamName + " stole " + yellowScore;
            //            redHammer = true;
            //        }
            //        else
            //        {
            //            mainDisplay.text = yellowRock.teamName + " scored " + yellowScore;
            //            redHammer = true;
            //        }
            //    }
            //}

            else
            {
                if (redHammer)
                {
                    redHammer = true;
                    mainDisplay.enabled = true;
                    mainDisplay.text = redRock.teamName + " keeps hammer";
                }
                else
                {
                    redHammer = false;
                    mainDisplay.enabled = true;
                    mainDisplay.text = yellowRock.teamName + " keeps hammer";
                }
            }

            yield return new WaitForSeconds(1f);

            state = GameState.RESET;
            StartCoroutine(ResetGame());
        }

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
