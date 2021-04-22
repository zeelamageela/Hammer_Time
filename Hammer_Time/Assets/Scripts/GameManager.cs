 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public enum GameState { START, REDTURN, YELLOWTURN, SCORE, RESET, END, DEBUG }


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

    public Transform launcher;
    public Transform yellowRocksInactive;
    public Transform redRocksInactive;
    public Collider2D boardCollider;

    int redRocks_left;
    int yellowRocks_left;
    public int redScore;
    public int yellowScore;
    //public GameHUD gHUD;

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
    public GameObject db;

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
        redTurn_Display.enabled = false;
        yellowTurn_Display.enabled = false;
        redRocksLeft_Display.enabled = false;
        yellowRocksLeft_Display.enabled = false;
        redRocksLeft_Slider.enabled = false;
        yellowRocksLeft_Slider.enabled = false;

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
        mainDisplay.gameObject.SetActive(false);
        redButton.gameObject.SetActive(false);
        yellowButton.gameObject.SetActive(false);
        StartCoroutine(SetupRocks());
        OnYellowTurn();
    }

    public void SetHammerYellow()
    {
        redHammer = false;
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
            rockList.Sort();
        }
    }

    IEnumerator ResetGame()
    {
        Debug.Log("Next End");

        yield return new WaitForSeconds(2f);
        houseList.Clear();
        foreach (Rock_List rock in rockList)
        {
            rock.rockInfo.outOfPlay = true;
        }
        rockList.Clear();

        yield return new WaitForSeconds(2f);

        endCurrent++;
        redRocks_left = rocksPerTeam;
        yellowRocks_left = rocksPerTeam;
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
        Debug.Log("Red Turn");

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

        state = GameState.REDTURN;
        StartCoroutine(RedTurn());
    }

    IEnumerator RedTurn()
    {
        redRocks_left--;

        GameObject redRock_1 = rockList[rockCurrent].rock;

        redRock = redRock_1.GetComponent<Rock_Info>();
        Debug.Log(redRock_1.name);

        yellowTurn_Display.enabled = false;
        redTurn_Display.enabled = true;
        redTurn_Display.text = redRock_1.name;
        redRocksLeft_Display.text = redRocks_left + " Rocks Left";
        redRocksLeft_Slider.maxValue = rocksPerTeam;
        redRocksLeft_Slider.value = redRock.rockNumber;

        yield return new WaitUntil(() => redRock.shotTaken == true);
        boardCollider.enabled = true;

        yield return new WaitUntil(() => redRock.rest == true);

        redTurn_Display.enabled = false;
        vcam.enabled = false;

        if (redRock.inHouse)
        {
            redTurn_Display.enabled = true;
            float distance = redRock.distance;
            redTurn_Display.text = redRock_1.name + " shot is " + distance + " from button";
        }

        ++rockCurrent;

        if (rockCurrent <= rockTotal)
        {
            StartCoroutine(CheckScore());
            OnRedTurn();
        }
        else
        {
            StartCoroutine(Scoring());
        }

    }

    public void OnYellowTurn()
    {
        Debug.Log("Yellow Turn");

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
        state = GameState.YELLOWTURN;
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
        yield return new WaitUntil(() => yellowRock.rest == true);

        yellowTurn_Display.enabled = false;
        vcam.enabled = false;

        if (yellowRock.inHouse)
        {
            yellowTurn_Display.enabled = true;
            float distance = yellowRock.distance;
            yellowTurn_Display.text = yellowRock_1.name + " shot is " + distance + " from button";
        }

        for(int i = 0; i<= rockCurrent; i++)
        {
            GameObject go = rockList[i].rock;

            if(rockList[i].rockInfo.outOfPlay)
            {
                go.SetActive(false);
                float yRocks = rocksPerTeam * 0.5f;
                int k = (i / 2);

                if (k <= yRocks)
                {
                    go.transform.position = new Vector2(go.transform.position.x, go.transform.position.y - ((k - 1) * 0.4f));
                }
                else if (k > yRocks)
                {
                    float j = k - yRocks;
                    go.transform.position = new Vector2(go.transform.position.x + 0.4f, go.transform.position.y - ((j - 1) * 0.4f));
                }
            }
        }

        ++rockCurrent;

        if (rockCurrent <= rockTotal)
        {
            StartCoroutine(CheckScore());
            OnRedTurn();
        }
        else
        {
            StartCoroutine(Scoring());
        }

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

    IEnumerator OutOfPlay()
    {
        foreach (Rock_List rock in rockList)
        {
            if (rock.rockInfo.outOfPlay)
            {
                rock.rock.SetActive(false);
            }
            else if (rock.rockInfo.inPlay != true)
            {
                rock.rock.SetActive(false);
            }
            else yield break;
        }
    }

    IEnumerator CheckScore()
    {
        yellowTurn_Display.enabled = false;
        redTurn_Display.enabled = false;

        StartCoroutine(AllStopped());
        yield return new WaitForFixedUpdate();

        StartCoroutine(OutOfPlay());
        yield return new WaitForFixedUpdate();

        houseList.Clear();


        foreach (Rock_List rock in rockList)
        {
            rock.rock.GetComponent<Rock_Colliders>().enabled = true;

            if (rock.rockInfo.inHouse)
            {
                houseList.Add(new House_List(rock.rock, rock.rockInfo));
            }
            if (rock.rockInfo.outOfPlay && rock.rockInfo.stopped)
            {
                rock.rock.SetActive(false);
            }
            if (rock.rockInfo.inPlay != true && rock.rockInfo.stopped)
            {
                rock.rock.SetActive(false);
            }
            
        }

        yield return new WaitForFixedUpdate();

        houseList.Sort();

        if (houseList.Count != 0)
        {
            foreach (House_List rock in houseList)
            {
                Debug.Log(rock.rockInfo.name + " - " + rock.rockInfo.distance);
            }
        }

        yield return new WaitForSeconds(0.5f);

        int tempRedScore = 0;
        int tempYellowScore = 0;
        int houseRock = houseList.Count;

        if (houseRock != 0)
        {
            if (houseList[0].rockInfo.teamName == "Red")
            {
                for (int i = 0; i < houseRock; i++)
                {
                    if (houseList[i].rockInfo.teamName == "Red")
                    {
                        tempRedScore++;
                    }
                }
                redTurn_Display.enabled = true;
                redTurn_Display.text = houseList[0].rockInfo.teamName + " is sitting " + tempRedScore;
            }

            else if (houseList[0].rockInfo.teamName == rockList[1].rockInfo.teamName)
            {
                for (int i = 0; i < houseRock; i++)
                {
                    if (houseList[i].rockInfo.teamName == rockList[1].rockInfo.teamName)
                    {
                        tempYellowScore++;
                    }
                }
                yellowTurn_Display.enabled = true;
                yellowTurn_Display.text = houseList[0].rockInfo.teamName + " is sitting " + tempYellowScore;
            }
        }
        else
        {
            mainDisplay.enabled = true;
            mainDisplay.text = "No Rocks In House";
        }
    }

    IEnumerator Scoring()
    {
        yellowTurn_Display.enabled = false;
        redTurn_Display.enabled = false;

        houseList.Clear();

        foreach (Rock_List rock in rockList)
        {
            if (rock.rockInfo.inHouse)
            {
                houseList.Add(new House_List(rock.rock, rock.rockInfo));
            }
            if (rock.rockInfo.outOfPlay)
            {
                rock.rock.SetActive(false);
            }
            if (rock.rockInfo.inPlay != true)
            {
                rock.rock.SetActive(false);
            }

        }

        houseList.Sort();

        foreach (House_List rock in houseList)
        {
            Debug.Log(rock.rockInfo.name + " - " + rock.rockInfo.distance);
        }

        redRock = rockList[0].rockInfo;
        yellowRock = rockList[1].rockInfo;

        int houseRocks = houseList.Count;

        if (houseList[0].rockInfo.teamName == redRock.teamName)
        {
            StartCoroutine(RedCount());

            redTurn_Display.enabled = true;

            if (redHammer)
            {
                redTurn_Display.text = redRock.teamName + " scored " + redScore;
                redHammer = false;
            }
            else
            {
                redTurn_Display.text = redRock.teamName + " stole " + redScore;
                redHammer = false;
            }
        }

        else if (houseList[0].rockInfo.teamName == yellowRock.teamName)
        {
            StartCoroutine(YellowCount());

            yellowTurn_Display.enabled = true;

            if (redHammer)
            {
                yellowTurn_Display.text = yellowRock.teamName + " stole " + yellowScore;
                redHammer = true;
            }
            else
            {
                yellowTurn_Display.text = yellowRock.teamName + " scored " + yellowScore;
                redHammer = true;
            }
        }

        else if (houseRocks == 0)
        {
            if (redHammer)
            {
                redHammer = true;
                redTurn_Display.enabled = true;
                redTurn_Display.text = redRock.teamName + " keeps hammer";
            }
            else
            {
                redHammer = false;
                yellowTurn_Display.enabled = true;
                yellowTurn_Display.text = yellowRock.teamName + " keeps hammer";
            }
        }

        yield return new WaitForSeconds(2f);

        state = GameState.RESET;
        StartCoroutine(ResetGame());
    }

    IEnumerator YellowCount()
    {
        int houseRocks = houseList.Count;

        for (int i = 0; i < houseRocks; i++)
        {
            if (houseList[i].rockInfo.teamName == yellowRock.teamName)
            {
                yellowScore++;
            }
            else yield return null;
        }
    }

    IEnumerator RedCount()
    {
        int houseRocks = houseList.Count;

        for (int i = 0; i < houseRocks; i++)
        {
            if (houseList[i].rockInfo.teamName == redRock.teamName)
            {
                redScore++;
            }
            else yield return null;
        }
    }

    public void OnDebug()
    {

        redButton.gameObject.SetActive(false);
        yellowButton.gameObject.SetActive(false);

        rockTotal = 8;
        rockCurrent = rockTotal - 1;
        rocksPerTeam = 1;
        redHammer = true;
        StartCoroutine(DebugMode());
        db.SetActive(false);
        mainDisplay.enabled = false;
        state = GameState.DEBUG;
    }

    IEnumerator DebugMode()
    {
        for (int i = 1; i < rockCurrent; i++)
        {
            Vector2 rockPlace = new Vector2(Random.Range(-2f, 2f), Random.Range(0f, 8f));

            if (i % 2 == 1)
            {
                GameObject yellowRock_go = Instantiate(yellowShooter, yellowRocksInactive);

                float yRocks = rocksPerTeam * 0.5f;
                int k = (i / 2) + 1;
                yellowRock_go.transform.position = new Vector2(rockPlace.x, rockPlace.y);

                Rock_Info yellowRock_info = yellowRock_go.GetComponent<Rock_Info>();
                yellowRock_info.rockNumber = k;
                yellowRock_go.name = yellowRock_info.teamName + " " + yellowRock_info.rockNumber;
                yellowRock_go.GetComponent<SpringJoint2D>().enabled = false;
                yellowRock_go.GetComponent<Rock_Colliders>().enabled = true;

                yellowRock_info.moving = false;
                yellowRock_info.stopped = true;
                yellowRock_info.rest = true;
                rockList.Add(new Rock_List(yellowRock_go, yellowRock_info));
                yield return new WaitForSeconds(0.1f);
            }

            if (i % 2 == 0)
            {
                GameObject redRock_go = Instantiate(redShooter, redRocksInactive);

                float yRocks = rocksPerTeam * 0.5f;
                int k = (i / 2);

                Rock_Info redRock_info = redRock_go.GetComponent<Rock_Info>();
                redRock_info.rockNumber = k;
                redRock_go.name = redRock_info.teamName + " " + redRock_info.rockNumber;
                redRock_go.GetComponent<SpringJoint2D>().enabled = false;
                redRock_go.GetComponent<Rock_Colliders>().enabled = true;
                
                redRock_go.transform.position = new Vector2(rockPlace.x, rockPlace.y);

                redRock_info.moving = false;
                redRock_info.stopped = true;
                redRock_info.rest = true;

                rockList.Add(new Rock_List(redRock_go, redRock_info));
                yield return new WaitForSeconds(0.1f);
            }
        }
        yield return new WaitForFixedUpdate();
        
        rockList.Sort();

        yield return new WaitForFixedUpdate();

        StartCoroutine(CheckScore());

        yield return new WaitForFixedUpdate();

        StartCoroutine(SetupRocksDebug());

        yield return new WaitForFixedUpdate();

        rockCurrent--;

        yield return new WaitForFixedUpdate();

        OnYellowTurn();

    }

    IEnumerator SetupRocksDebug()
    {
        for (int i = rockCurrent; i <= rockTotal; i++)
        {
            if (i % 2 == 1)
            {
                GameObject yellowRock_go = Instantiate(yellowShooter, yellowRocksInactive);

                float yRocks = rocksPerTeam * 0.5f;
                int k = (i / 2) + 1;

                yellowRock_go.transform.position = new Vector2(yellowRock_go.transform.position.x, yellowRock_go.transform.position.y - (0.4f));
                Rock_Info yellowRock_info = yellowRock_go.GetComponent<Rock_Info>();
                yellowRock_info.rockNumber = k;
                yellowRock_go.name = yellowRock_info.teamName + " " + yellowRock_info.rockNumber;
                yellowRock_go.GetComponent<Rock_Flick>().enabled = false;
                yellowRock_go.GetComponent<Rock_Release>().enabled = false;
                yellowRock_go.GetComponent<Rock_Force>().enabled = false;
                rockList.Add(new Rock_List(yellowRock_go, yellowRock_info));
                yield return new WaitForSeconds(0.1f);
            }
            if (i % 2 == 0)
            {
                GameObject redRock_go = Instantiate(redShooter, redRocksInactive);
                float yRocks = rocksPerTeam / 2f;
                int k = (i / 2);

                redRock_go.transform.position = new Vector2(redRock_go.transform.position.x, redRock_go.transform.position.y - (0.4f));

                Rock_Info redRock_info = redRock_go.GetComponent<Rock_Info>();
                redRock_info.rockNumber = k;
                redRock_go.name = redRock_info.teamName + " " + redRock_info.rockNumber;
                redRock_go.GetComponent<Rock_Flick>().enabled = false;
                redRock_go.GetComponent<Rock_Release>().enabled = false;
                redRock_go.GetComponent<Rock_Force>().enabled = false;
                rockList.Add(new Rock_List(redRock_go, redRock_info));
                yield return new WaitForSeconds(0.1f);
            }
        }
        rockList.Sort();
        foreach (Rock_List rock in rockList)
        {
            Debug.Log(rockList.IndexOf(rock) + rock.rockInfo.teamName);
        }
    }

}
