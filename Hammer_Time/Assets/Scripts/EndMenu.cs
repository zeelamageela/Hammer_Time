using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndMenu : MonoBehaviour
{
    GameSettingsPersist gsp;
    public Text endText;
    public Text endNumber;
    public Text redScore;
    public Text yellowScore;
    public Button contButton;
    public Button menuButton;
    public GameObject yellowSpinner;
    public GameObject yellowSpinnerAI;
    public GameObject redHammerPNG;
    public GameObject yellowHammerPNG;
    public GameObject sbRedHammerPNG;
    public GameObject sbYellowHammerPNG;
    public string contScene;
    public GameObject scoreboard;
    public Image[] scoreCards;
    public Text[] scoreCardsText;

    // Start is called before the first frame update
    void Start()
    {
        //for (int i = 0; i < 24; i++)
        //{
        //    scoreCardsText[i] = scoreCards[i].gameObject.GetComponent<Text>();
        //}

        foreach (Image scoreCard in scoreCards)
        {
            scoreCard.gameObject.SetActive(false);
        }
        

        gsp = FindObjectOfType<GameSettingsPersist>();

        if (gsp)
        {
            if (gsp.endCurrent > gsp.ends)
            {
                if (gsp.redScore != gsp.yellowScore)
                {
                    if (gsp.redScore > gsp.yellowScore)
                        endText.text = gsp.redTeamName + " Wins!";
                    else
                        endText.text = gsp.yellowTeamName + " Wins!";

                    contButton.transform.GetComponentInChildren<Text>().text = "Continue>";
                    endNumber.enabled = false;
                    
                    if (!gsp.tourny)
                        contButton.gameObject.SetActive(false);
                }

                else
                    endText.text = "Extra End";
            }
            else
            {
                contButton.transform.GetComponentInChildren<Text>().text = "Next End>";
                if (gsp.redHammer)
                {
                    redHammerPNG.SetActive(true);
                    sbRedHammerPNG.SetActive(true);
                }
                else
                {
                    yellowHammerPNG.SetActive(true);
                    sbYellowHammerPNG.SetActive(true);
                }
            }

            endNumber.text = gsp.endCurrent.ToString();
            redScore.text = gsp.redScore.ToString();
            yellowScore.text = gsp.yellowScore.ToString();

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

            for (int i = 0; i < gsp.endCurrent; i++)
            {
                Scoreboard(i + 1, gsp.score[i].x, gsp.score[i].y);
                //if (i > 0 && gsp.score[i].x - gsp.score[i - 1].x > 0)
                //{
                //    Scoreboard(i + 1, gsp.score[i].x, 0);
                //}
                //else if (i > 0 && gsp.score[i].y - gsp.score[i - 1].y > 0)
                //    Scoreboard(i + 1, 0, gsp.score[i].y);
                //else if (i == 0)
                //    Scoreboard(i + 1, gsp.score[i].x, gsp.score[i].y);
                //else
                //    Scoreboard(i + 1, 0, 0);
            }
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
            //if (scoreboard.activeSelf)
            //{
            //    scoreboard.SetActive(false);
            //}
            //else
            //{
            //    scoreboard.SetActive(true);
            //}
        }

    }

    IEnumerator ScoreCards(int cardNumber, int totalScore)
    {
        scoreboard.SetActive(true);
        scoreCards[cardNumber].gameObject.SetActive(true);
        //scoreCards[cardNumber].gameObject.transform.GetChild(0).GetComponent<Text>().text = "12";
        scoreCards[cardNumber].gameObject.transform.GetChild(0).GetComponent<Text>().text = totalScore.ToString();

        yield return new WaitForSeconds(2f);

        //scoreboard.SetActive(true);

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
        if (gsp && gsp.tourny && gsp.endCurrent > gsp.ends && gsp.redScore != gsp.yellowScore)
        {
            if (gsp.playoffRound > 0)
                gsp.playoffRound++;
            else
                gsp.draw++;
        }
        SceneManager.LoadScene(contScene);
    }
}
