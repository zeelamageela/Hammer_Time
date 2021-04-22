using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public enum GameState { START, REDTURN, YELLOWTURN, SCORE, RESET, END }


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

    int redRocks_left;
    int yellowRocks_left;
    int redRock_current;
    int yellowRock_current;
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
            redRock_current = rockCurrent / 2;
            yellowRock_current = (rockCurrent / 2) + 1;
        }
        else
        {
            hammer = 1;
            notHammer = 0;
            yellowRock_current = rockCurrent / 2;
            redRock_current = (rockCurrent / 2) + 1;
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
                    yellowRock_go.transform.position = new Vector2(yellowRock_go.transform.position.x, yellowRock_go.transform.position.y - ((k-1) * 0.4f));
                }
                else if (k > yRocks)
                {
                    float j = k - yRocks;
                    yellowRock_go.transform.position = new Vector2(yellowRock_go.transform.position.x + 0.4f, yellowRock_go.transform.position.y - ((j-1) * 0.4f));
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
                    redRock_go.transform.position = new Vector2(redRock_go.transform.position.x, redRock_go.transform.position.y - ((k-1) * 0.4f));
                }
                else if (k > yRocks)
                {
                    float j = k - yRocks;
                    redRock_go.transform.position = new Vector2(redRock_go.transform.position.x - 0.4f, redRock_go.transform.position.y - ((j-1) * 0.4f));
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
        redRock_current = 0;
        yellowRock_current = 0;
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
        redRocksLeft_Slider.value = redRock_current;

        yield return new WaitUntil(() => redRock.rest == true);

        redTurn_Display.enabled = false;
        vcam.enabled = false;
        rockCurrent++;

        if (redRock.inHouse)
        {
            float distance = redRock.distance;
            Debug.Log(redRock_1.name + " is " + distance + " from button");
            houseList.Add(new House_List(redRock_1, redRock));
        }

        if (rockCurrent == rockTotal - 1)
        {
            StartCoroutine(Scoring());
        }
        else
        {
            redTurn_Display.enabled = false;
            yellowTurn_Display.enabled = true;
            yellowTurn_Display.text = "Next Rock";
            StartCoroutine(CheckScore());

            OnYellowTurn();
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

        yield return new WaitUntil(() => yellowRock.rest == true);

        yellowTurn_Display.enabled = false;
        vcam.enabled = false;

        if (yellowRock.inHouse)
        {
            float distance = yellowRock.distance;
            Debug.Log(yellowRock_1.name + " is " + distance + " from button");
            houseList.Add(new House_List(yellowRock_1, yellowRock));
        }
        rockCurrent++;
        if (rockCurrent < rockTotal)
        {
            StartCoroutine(CheckScore());
        }
        else
        {
            redTurn_Display.enabled = false;
            yellowTurn_Display.enabled = true;
            yellowTurn_Display.text = "Next Rock";
            StartCoroutine(Scoring());


            OnRedTurn();
        }
            
    }

    IEnumerator CheckScore()
    {
        yellowTurn_Display.enabled = false;
        redTurn_Display.enabled = false;

        houseList.Sort();
        foreach(House_List rock in houseList)
        {
            Debug.Log(rock.rockInfo.name + " - " + rock.rockInfo.distance);
        }

        yield return new WaitForSeconds(0.5f);

        int tempRedScore = 0;
        int tempYellowScore = 0;
        int houseRock = houseList.Count;

        if (houseRock != 0)
        {
            if (houseList[0].rockInfo.teamName == redRock.teamName)
            {
                for (int i = 0; i < houseRock; i++)
                {
                    if (houseList[i].rockInfo.teamName == redRock.teamName)
                    {
                        tempRedScore++;
                    }
                }
                redTurn_Display.enabled = true;
                redTurn_Display.text = redRock.teamName + " is sitting " + tempRedScore;
            }

            else if (houseList[0].rockInfo.teamName == yellowRock.teamName)
            {
                for (int i = 0; i < houseRock; i++)
                {
                    if (houseList[i].rockInfo.teamName == yellowRock.teamName)
                    {
                        tempYellowScore++;
                    }
                }
                yellowTurn_Display.enabled = true;
                yellowTurn_Display.text = yellowRock.teamName + " is sitting " + tempYellowScore;
            }
        }
        else
        {
            yellowTurn_Display.enabled = true;
            yellowTurn_Display.text = "No Rocks";
        }
    }

    IEnumerator Scoring()
    {
        houseList.Sort();
        foreach (House_List rock in houseList)
        {
            Debug.Log(rock.rockInfo.teamName + " " + rock.rockInfo.rockNumber + " - " + rock.rockInfo.distance);
        }

        int houseRocks = houseList.Count;
        if (houseList == null)
        {
            yield break;
        }
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
}
