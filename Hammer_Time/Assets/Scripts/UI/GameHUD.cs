using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    public GameManager gm;
    public StoryManager sm;

    public Text mainDisplay;
    public Text clickDisplay;
    public Text logDisplay;

    string[] actionLogs;

    public GameObject scoreboard;
    public Image scoreboardUI;
    public Image[] scoreCards;
    public Text[] scoreCardsText;
    public Image redHammerPNG;
    public Image yellowHammerPNG;
    public GameObject panel;
    public bool scoreCheck;

    public GameObject[] scoreCols;
    public GameObject scorePanel;

    public Text redTeamName;
    public Text yellowTeamName;
    public Image redTeamPanel;
    public Image yellowTeamPanel;
    public Text redTotalScore;
    public Text yellowTotalScore;

    private void Start()
    {
        foreach (Image scoreCard in scoreCards)
        {
            scoreCard.gameObject.SetActive(false);
        }

        clickDisplay.enabled = false;
        //foreach (Text scoreCardText in scoreCardsText)
        //{
        //    scoreCardText.enabled = false;
        //}
    }

    private void Update()
    {
        if (scoreCheck)
        {
            ScoringPanel();
            scoreCheck = false;
        }

        if (!mainDisplay.enabled)
        {
            clickDisplay.enabled = false;
            panel.SetActive(false);
        }

        if (mainDisplay.enabled)
        {
            panel.SetActive(true);
        }

        //if (sm.dialogueGO.activeSelf)
        //{
        //    mainDisplay.enabled = false;
        //    clickDisplay.enabled = false;
        //}
    }
    public void SetHUD(int redRocksLeft, int yellowRocksLeft, int rocksPerTeam, int rockCurrent, Rock_Info rock)
    {
        //scoreboard.gameObject.SetActive(false);

        mainDisplay.enabled = true;
        mainDisplay.text = rock.teamName + " Turn";


        float waitTime = 2f;
        StartCoroutine(MainDisplayTimer(waitTime));
    }
    IEnumerator MainDisplayTimer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        mainDisplay.enabled = false;
    }
    IEnumerator ClickDisplay()
    {
        yield return new WaitForSeconds(1f);
        clickDisplay.enabled = true;
        //yield return StartCoroutine(WaitForClick());
    }

    public void MainDisplayOff()
    {
        mainDisplay.enabled = false;
        clickDisplay.enabled = false;
    }
    IEnumerator ScoreboardTimer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        scoreboard.SetActive(false);
    }

    public void ScoreboardOff()
    {
        scoreboard.SetActive(false);
        clickDisplay.enabled = false;
    }
    IEnumerator WaitForClick()
    {
        while (!Input.GetMouseButtonDown(0))
        {
            yield return null;
        }
    }

    IEnumerator RefreshPanel()
    {
        panel.GetComponent<ContentSizeFitter>().enabled = false;

        yield return new WaitForEndOfFrame();
        panel.GetComponent<ContentSizeFitter>().enabled = true;
    }

    public void CheckScore(bool noRocks, string teamName, int score, bool houseClick)
    {
        mainDisplay.enabled = true;
        float waitTime;

        if (noRocks)
        {
            mainDisplay.text = "No Rocks in House";
            waitTime = 1f;
        }
        else
        {
            mainDisplay.text = teamName + " is Sitting " + score;
            waitTime = 1.5f;
        }

        StartCoroutine(RefreshPanel());

        if (!houseClick)
            StartCoroutine(ClickDisplay());
        else
            StartCoroutine(MainDisplayTimer(waitTime));
    }

    public void Message(string message)
    {
        mainDisplay.enabled = true;
        mainDisplay.text = message;
        StartCoroutine(RefreshPanel());
    }

    public void MessageLog(string message)
    {
        logDisplay.enabled = true;

        actionLogs[3] = actionLogs[2];
        actionLogs[2] = actionLogs[1];
        actionLogs[1] = actionLogs[0];
        actionLogs[0] = message;

        logDisplay.text = actionLogs[0] + "/n" + actionLogs[1] + "/n" + actionLogs[2] + "/n" + actionLogs[3];
    }

    public void ScoringUI(string hammerTeamName, string teamName, int score)
    {
        mainDisplay.enabled = true;
        scorePanel.SetActive(false);
        ScoringPanel();

        if (score == 0)
        {
            mainDisplay.text = hammerTeamName + " Keeps Hammer";
        }
        else
        {
            if (hammerTeamName == teamName)
            {
                mainDisplay.text = teamName + " Scored " + score;
            }
            else
            {
                mainDisplay.text = teamName + " Stole " + score;
            }

        }
        StartCoroutine(ClickDisplay());
    }

    public void ScoringPanel()
    {
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();

        for (int i = 0; i < gsp.ends; i++)
        {
            scoreCols[i].SetActive(true);
            scoreCols[i].transform.GetChild(0).gameObject.SetActive(true);

            scoreCols[i].transform.GetChild(1).gameObject.SetActive(false);
            scoreCols[i].transform.GetChild(2).gameObject.SetActive(false);
            Debug.Log("I IS " + i);
            if (i < (gm.endCurrent - 1))
            {
                scoreCols[i].transform.GetChild(1).gameObject.SetActive(true);
                scoreCols[i].transform.GetChild(2).gameObject.SetActive(true);
                scoreCols[i].transform.GetChild(1).GetComponent<Text>().text = gsp.score[i].x.ToString();
                scoreCols[i].transform.GetChild(2).GetComponent<Text>().text = gsp.score[i].y.ToString();
            }
            else if (i == gsp.endCurrent - 1)
            {
                scoreCols[i].transform.GetChild(0).GetComponent<Text>().color = Color.yellow;

                if (gm.redHammer)
                {
                    scoreCols[i].transform.GetChild(1).gameObject.SetActive(true);
                    scoreCols[i].transform.GetChild(1).GetComponent<Text>().text = "H";
                }
                else
                {
                    scoreCols[i].transform.GetChild(2).gameObject.SetActive(true);
                    scoreCols[i].transform.GetChild(2).GetComponent<Text>().text = "H";
                }
            }
            else
            {
                scoreCols[i].transform.GetChild(1).gameObject.SetActive(false);
                scoreCols[i].transform.GetChild(2).gameObject.SetActive(false);
            }
        }
        redTeamName.text = gsp.redTeamName;
        yellowTeamName.text = gsp.yellowTeamName;
        redTeamPanel.color = gsp.redTeamColour;
        yellowTeamPanel.color = gsp.yellowTeamColour;
        redTotalScore.text = gsp.redScore.ToString();
        yellowTotalScore.text = gsp.yellowScore.ToString();
    }

    #region Old Scoreboard
    //public void Scoreboard(int endNumber, int redScore, int yellowScore)
    //{
    //    int cardNumber;
    //    int totalScore;

    //    if (redScore > 0)
    //    {
    //        cardNumber = endNumber + 11;
    //        totalScore = redScore;
    //        StartCoroutine(ScoreCards(cardNumber, totalScore));
    //    }
    //    else if (yellowScore > 0)
    //    {
    //        cardNumber = endNumber - 1;
    //        totalScore = yellowScore;
    //        StartCoroutine(ScoreCards(cardNumber, totalScore));
    //    }
    //    else
    //    {
    //        if (scoreboard.activeSelf)
    //        {
    //            scoreboard.SetActive(false);
    //        }
    //        else
    //        {
    //            scoreboard.SetActive(true);
    //        }
    //    }

    //}
    #endregion

    IEnumerator ScoreCards(int cardNumber, int totalScore)
    {
        //scoreboard.SetActive(true);
        scoreCards[cardNumber].gameObject.SetActive(true);
        scoreCardsText[cardNumber].text = totalScore.ToString();

        yield return new WaitForSeconds(2f);

        //scoreboard.SetActive(true);

        float waitTime = 2.5f;
        StartCoroutine(ScoreboardTimer(waitTime));
    }

    public void SetHammer(bool redHammer)
    {
        if (redHammer)
        {
            redHammerPNG.enabled = true;
            yellowHammerPNG.enabled = false;
            //Scoreboard(1, 0, 0);
        }
        else
        {
            yellowHammerPNG.enabled = true;
            redHammerPNG.enabled = false;
            //Scoreboard(1, 0, 0);
        }
    }

    public void EndOfGame(int redScore, string redTeamName, int yellowScore, string yellowTeamName)
    {

        if (redScore > yellowScore)
        {
            mainDisplay.enabled = true;
            mainDisplay.text = redTeamName + " Wins!";
        }
        else if (redScore < yellowScore)
        {
            mainDisplay.enabled = true;
            mainDisplay.text = yellowTeamName + " Wins!";
        }
        else
        {
            mainDisplay.enabled = true;
            mainDisplay.text = "Tie Game!";
        }

        StartCoroutine(ClickDisplay());
    }
}
