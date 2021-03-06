using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndMenu : MonoBehaviour
{
    GameSettingsPersist gsp;
    CareerManager cm;

    public Color yellow;
    public Color dimmed;
    public Color white;

    public Text tournyName;
    public Text end;
    public Text draw;
    public Text info;
    public Text redTotalScore;
    public Text yellowTotalScore;

    public Text redTeamName;
    public Image redTeamPanel;
    public Text yellowTeamName;
    public Image yellowTeamPanel;


    public Button contButton;
    public Button endButton;
    public Button menuButton;

    public GameObject yellowSpinner;
    public GameObject yellowSpinnerAI;
    public GameObject redHammerPNG;
    public GameObject yellowHammerPNG;

    public GameObject scoreboard;
    public Vector2[] score;
    public GameObject[] scoreCols;

    int ends;
    // Start is called before the first frame update
    void Start()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
        cm = FindObjectOfType<CareerManager>();

        if (gsp)
        {
            ends = gsp.ends;
            if (gsp.endCurrent == 0)
            {
                contButton.gameObject.SetActive(true);
                endButton.gameObject.SetActive(false);
                contButton.transform.GetComponentInChildren<Text>().text = "Start Game>";

                if (gsp.redHammer)
                {
                    redHammerPNG.SetActive(true);
                    info.text = gsp.redTeamName + " has the hammer";
                }
                else
                {
                    yellowHammerPNG.SetActive(true);
                    info.text = gsp.yellowTeamName + " has the hammer";
                }
            }
            else if (gsp.endCurrent >= ends)
            {
                if (gsp.redScore != gsp.yellowScore)
                {
                    if (gsp.redScore > gsp.yellowScore)
                        info.text = gsp.redTeamName + " Wins!";
                    else
                        info.text = gsp.yellowTeamName + " Wins!";

                    contButton.gameObject.SetActive(false);
                    endButton.gameObject.SetActive(true);
                    contButton.transform.GetComponentInChildren<Text>().text = "Tourny Home>";
                }
                else
                {
                    info.text = "Extra End";
                    ends++;
                    contButton.gameObject.SetActive(true);
                    endButton.gameObject.SetActive(false);
                }
            }
            else
            {
                contButton.gameObject.SetActive(true);
                endButton.gameObject.SetActive(false);
                contButton.transform.GetComponentInChildren<Text>().text = "Next End>";
                if (gsp.redHammer)
                {
                    redHammerPNG.SetActive(true);
                    info.text = gsp.redTeamName + " has the hammer";
                }
                else
                {
                    yellowHammerPNG.SetActive(true);
                    info.text = gsp.yellowTeamName + " has the hammer";
                }
            }

            if (gsp.playoffRound > 0)
            {
                if (gsp.KO)
                    draw.text = "Round " + gsp.playoffRound.ToString();
                else
                    draw.text = "Playoff Round " + gsp.playoffRound.ToString();
            }
            else
            {
                draw.text = "Draw " + (gsp.draw + 1).ToString();
            }

            tournyName.text = cm.currentTourny.name;
            end.text = "End " + (gsp.endCurrent + 1).ToString();
            redTeamName.text = gsp.redTeamName;
            yellowTeamName.text = gsp.yellowTeamName;
            //redTeamPanel.color = gsp.redTeamColour;
            //yellowTeamPanel.color = gsp.yellowTeamColour;
            redTotalScore.text = gsp.redScore.ToString();
            yellowTotalScore.text = gsp.yellowScore.ToString();

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


            for (int i = 0; i < ends; i++)
            {
                scoreCols[i].SetActive(true);
                scoreCols[i].transform.GetChild(0).gameObject.SetActive(true);

                scoreCols[i].transform.GetChild(1).gameObject.SetActive(false);
                scoreCols[i].transform.GetChild(2).gameObject.SetActive(false);
                Debug.Log("I IS " + i);
                if (i < (gsp.endCurrent))
                {
                    scoreCols[i].transform.GetChild(1).gameObject.SetActive(true);
                    scoreCols[i].transform.GetChild(2).gameObject.SetActive(true);
                    scoreCols[i].transform.GetChild(1).GetComponent<Text>().text = gsp.score[i].x.ToString();
                    scoreCols[i].transform.GetChild(2).GetComponent<Text>().text = gsp.score[i].y.ToString();
                }
                else if (i == gsp.endCurrent)
                {
                    scoreCols[i].transform.GetChild(0).GetComponent<Text>().color = yellow;

                    if (gsp.redHammer)
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
        }

        
        
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
        CareerManager cm = FindObjectOfType<CareerManager>();
        gsp = FindObjectOfType<GameSettingsPersist>();

        SceneManager.LoadScene("TournyGame");
    }

    public void EndGame()
    {
        if (gsp.playoffRound > 0)
        {
            gsp.playoffRound++;
            Debug.Log("Play off Round - " + gsp.playoffRound);
        }
        else
            gsp.draw++;

        if (gsp.KO)
            SceneManager.LoadScene("Tourny_Home_3K");
        else
            SceneManager.LoadScene("Tourny_Home_1");
    }
}
