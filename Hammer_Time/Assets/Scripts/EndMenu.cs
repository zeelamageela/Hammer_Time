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
    public Image redTeamColor;
    public Image yellowTeamColor;

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
            gsp.loadGame = false;
            ends = gsp.ends;

            if (gsp.cashGame)
            {
                end.text = "Cash Game";
            }
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
                end.text = "End " + (gsp.endCurrent + 1).ToString() + "/" + ends;
                draw.text = "Draw " + (gsp.draw + 1).ToString();
                if (gsp.cashGame)
                {
                    draw.text = "Cash Game";
                }
            }
            else if (gsp.endCurrent >= ends)
            {
                if (gsp.redScore != gsp.yellowScore)
                {
                    if (gsp.redScore > gsp.yellowScore)
                    {
                        info.text = "Team " + gsp.redTeamName + " Wins";
                    }
                    else
                    {
                        info.text = "Team " + gsp.yellowTeamName + " Wins";
                    }

                    contButton.gameObject.SetActive(false);
                    endButton.gameObject.SetActive(true);
                    contButton.transform.GetComponentInChildren<Text>().text = "Tourny Home>";

                    draw.text = "Draw " + (gsp.draw + 1).ToString();
                    end.text = " ";

                    if (gsp.cashGame)
                    {
                        if (gsp.aiYellow)
                        {
                            if (gsp.redScore > gsp.yellowScore)
                            {
                                info.text = "You Win";
                                draw.text = "$" + gsp.prize * 2f;
                            }
                            else
                            {
                                info.text = "You Lose";
                                draw.text = " ";
                            }
                        }
                        else
                        {
                            if (gsp.yellowScore > gsp.redScore)
                            {
                                info.text = "You Win";
                                draw.text = "$" + gsp.prize * 2f;
                            }
                            else
                            {
                                info.text = "You Lose";
                                draw.text = " ";
                            }
                        }
                        contButton.transform.GetComponentInChildren<Text>().text = "Next Week>";
                        ends++;
                    }
                }
                else
                {
                    info.text = "Tie Game!";
                    ends++;
                    contButton.gameObject.SetActive(true);
                    endButton.gameObject.SetActive(false);

                    int extraEnd = ends - gsp.endCurrent + 1;
                    end.text = "Extra End";

                    draw.text = "Draw " + (gsp.draw + 1).ToString();

                    if (gsp.cashGame)
                    {
                        draw.text = "Cash Game";
                    }
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

                draw.text = "Draw " + (gsp.draw + 1).ToString();
                end.text = "End " + (gsp.endCurrent + 1).ToString() + "/" + ends;

                if (gsp.cashGame)
                {
                    draw.text = "Cash Game";
                }
            }

            if (gsp.playoffRound > 0)
            {
                if (gsp.KO)
                    draw.text = "Round " + gsp.playoffRound.ToString();
                else
                {
                    draw.text = "Playoff Round " + gsp.playoffRound.ToString();
                    if (gsp.playoffRound == 1)
                        draw.text = "Quarterfinals";
                    if (gsp.playoffRound == 2)
                        draw.text = "Semifinals";
                    if (gsp.playoffRound == 3)
                        draw.text = "Finals";
                }
            }
            //else
            //{
            //    draw.text = "Draw " + (gsp.draw + 1).ToString();
            //}


            if (cm != null)
                tournyName.text = cm.currentTourny.name;


            redTeamName.text = gsp.redTeamName;
            yellowTeamName.text = gsp.yellowTeamName;
            redTotalScore.text = gsp.redScore.ToString();
            yellowTotalScore.text = gsp.yellowScore.ToString();

            if (gsp.aiYellow)
            {
                yellowSpinner.SetActive(false);
                yellowSpinnerAI.SetActive(true);
                redTeamColor.color = white;
                yellowTeamColor.color = dimmed;
            }
            else
            {
                yellowSpinner.SetActive(true);
                yellowSpinnerAI.SetActive(false);
                redTeamColor.color = dimmed;
                yellowTeamColor.color = white;
            }

            for (int i = 0; i < ends; i++)
            {
                scoreCols[i].SetActive(true);
                scoreCols[i].transform.GetChild(0).gameObject.SetActive(true);

                scoreCols[i].transform.GetChild(1).gameObject.SetActive(false);
                scoreCols[i].transform.GetChild(2).gameObject.SetActive(false);
                Debug.Log("I IS " + i);

                if (gsp.endCurrent - 1 > gsp.score.Length)
                {
                    Vector2Int[] tempScore = new Vector2Int[gsp.endCurrent];
                    for (int j = 0; j < gsp.endCurrent; j++)
                    {
                        if (j <= gsp.score.Length)
                            tempScore[j] = gsp.score[j];
                        else
                        {
                            tempScore[j].x = gsp.redScore;
                            tempScore[j].y = gsp.yellowScore;
                        }
                    }

                    gsp.score = new Vector2Int[tempScore.Length];
                    for (int j = 0; j < gsp.score.Length; j++)
                    {
                        gsp.score[j] = tempScore[j];
                    }
                }

                if (i < gsp.endCurrent)
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
        if (gsp.tourny)
        {
            //if (gsp.aiRed)
            //{
            //    if (gsp.redScore > gsp.yellowScore)
            //        gsp.record.y += 1;

            //    if (gsp.redScore < gsp.yellowScore)
            //        gsp.record.x += 1;
            //}
            //if (gsp.aiYellow)
            //{
            //    if (gsp.redScore > gsp.yellowScore)
            //        gsp.record.x += 1;

            //    if (gsp.redScore < gsp.yellowScore)
            //        gsp.record.y += 1;
            //}

            if (gsp.playoffRound > 0)
            {
                //gsp.playoffRound++;
                Debug.Log("Play off Round - " + gsp.playoffRound);
            }
            else
                gsp.draw++;
            //gsp.tournyInProgress = false;
            //gsp.gameInProgress = false;
            cm.SaveCareer();
            SceneManager.LoadScene("Tourny_Home_1");
        }

        if (gsp.cashGame)
        {
            float winnings;

            gsp.tournyInProgress = false;
            Debug.Log("CM Record is " + cm.record.x + " - " + cm.record.y);
            Debug.Log("CM earnings are " + cm.earnings);

            cm.TournyResults();
            cm.SetUpCareer();
            if (gsp.skinsGame)
            {
                winnings = gsp.skins * 2f;
            }
            else
            {
                winnings = gsp.prize * 2f;
            }

            gsp.cash += winnings;
            cm.cash += gsp.cash;

            SceneManager.LoadScene("Arena_Selector");
        }
        else if (gsp.KO)
            SceneManager.LoadScene("Tourny_Home_3K");
        else
            SceneManager.LoadScene("Tourny_Home_1");
    }
}
