using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    public GameManager gm;
    public Text mainDisplay;
    public Text clickDisplay;

    public GameObject scoreboard;
    public Image scoreboardUI;
    public Image[] scoreCards;
    public Text[] scoreCardsText;
    public Image redHammerPNG;
    public Image yellowHammerPNG;

    public bool scoreCheck;
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
            Scoreboard(1, 0, 0);
            scoreCheck = false;
        }

        if (!mainDisplay.enabled)
        {
            clickDisplay.enabled = false;
        }
    }
    public void SetHUD(int redRocksLeft, int yellowRocksLeft, int rocksPerTeam, int rockCurrent, Rock_Info rock)
    {
        scoreboard.gameObject.SetActive(false);
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
        yield return new WaitForSeconds(0.25f);
        clickDisplay.enabled = true;
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

    public void CheckScore(bool noRocks, string teamName, int score)
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
            mainDisplay.text = teamName + " is sitting " + score;
            waitTime = 1.5f;
        }

        StartCoroutine(ClickDisplay());
    }

    public void ScoringUI(string hammerTeamName, string teamName, int score)
    {
        mainDisplay.enabled = true;
        float waitTime;

        if (score == 0)
        {
            mainDisplay.text = hammerTeamName + " keeps hammer";
            waitTime = 1f;
        }
        else
        {
            if (hammerTeamName == teamName)
            {
                mainDisplay.text = teamName + " scored " + score;
            }
            else
            {
                mainDisplay.text = teamName + " stole " + score;
            }

            waitTime = 2f;
        }

        StartCoroutine(ClickDisplay());
    }
    public void Scoreboard(int endNumber, int redScore, int yellowScore)
    {
        int cardNumber;
        int totalScore;

        if (redScore > 0)
        {
            cardNumber = endNumber + 11;
            totalScore = redScore;
            StartCoroutine(ScoreCards(cardNumber, totalScore));
        }
        else if (yellowScore > 0)
        {
            cardNumber = endNumber - 1;
            totalScore = yellowScore;
            StartCoroutine(ScoreCards(cardNumber, totalScore));
        }
        else
        {
            if (scoreboard.activeSelf)
            {
                scoreboard.SetActive(false);
            }
            else
            {
                scoreboard.SetActive(true);
            }
        }

    }

    IEnumerator ScoreCards(int cardNumber, int totalScore)
    {
        scoreboard.SetActive(true);
        scoreCards[cardNumber].gameObject.SetActive(true);
        scoreCardsText[cardNumber].text = totalScore.ToString();

        yield return new WaitForSeconds(2f);

        scoreboard.SetActive(true);

        float waitTime = 2.5f;
        StartCoroutine(ScoreboardTimer(waitTime));
    }

    public void SetHammer(bool redHammer)
    {
        if (redHammer)
        {
            redHammerPNG.enabled = true;
            yellowHammerPNG.enabled = false;
            Scoreboard(1, 0, 0);
        }
        else
        {
            yellowHammerPNG.enabled = true;
            redHammerPNG.enabled = false;
            Scoreboard(1, 0, 0);
        }
    }

    public void EndOfGame(int redScore, string redTeamName, int yellowScore, string yellowTeamName)
    {
        float waitTime;

        if (redScore > yellowScore)
        {
            mainDisplay.enabled = true;
            mainDisplay.text = redTeamName + " Wins!";
            waitTime = 2.5f;
        }
        else if (redScore < yellowScore)
        {
            mainDisplay.enabled = true;
            mainDisplay.text = yellowTeamName + " Wins!";
            waitTime = 2.5f;
        }
        else
        {
            mainDisplay.enabled = true;
            mainDisplay.text = "Tie Game!";
            waitTime = 2f;
        }

        StartCoroutine(ClickDisplay());
    }
}
