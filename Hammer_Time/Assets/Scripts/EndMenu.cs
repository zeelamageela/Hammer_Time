using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndMenu : MonoBehaviour
{
    GameSettingsPersist gsp;
    public Text endNumber;
    public Text redScore;
    public Text yellowScore;
    public GameObject yellowSpinner;
    public GameObject yellowSpinnerAI;
    public GameObject redHammerPNG;
    public GameObject yellowHammerPNG;
    public string contScene;
    public GameObject scoreboard;
    public Image[] scoreCards;
    public Text[] scoreCardsText;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Image scoreCard in scoreCards)
        {
            scoreCard.gameObject.SetActive(false);
        }

        gsp = FindObjectOfType<GameSettingsPersist>();

        if (gsp)
        {
            if (gsp.endCurrent >= gsp.ends && gsp.redScore != gsp.yellowScore)
            {
                endNumber.text = "Game Over!";
            }
            else
            {
                endNumber.text = gsp.endCurrent.ToString();
                redScore.text = gsp.redScore.ToString();
                yellowScore.text = gsp.yellowScore.ToString();

                if (gsp.redHammer)
                    redHammerPNG.SetActive(true);
                else
                    yellowHammerPNG.SetActive(true);
            }

            if (gsp.aiYellow)
            {
                yellowSpinner.SetActive(false);
                yellowSpinnerAI.SetActive(true);
            }
            else
            {
                yellowSpinner.SetActive(true);
                yellowSpinnerAI.SetActive(false);
            }


        }

        for (int i = 1; i < gsp.endCurrent; i++)
        {
            Scoreboard(i, gsp.score[i - 1].x, gsp.score[i - 1].y);
        }
        
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

    }

    public void Menu()
    {
        SceneManager.LoadScene("SplashMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    public void Continue()
    {
        SceneManager.LoadScene(contScene);
    }
}
