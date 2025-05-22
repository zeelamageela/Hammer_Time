using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun.UtilityScripts;
using static Photon.Pun.UtilityScripts.PunTeams;

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
    public Button simButton;

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
                simButton.gameObject.SetActive(true);
                contButton.transform.GetComponentInChildren<Text>().text = "Start Game>";

                if (gsp.redHammer)
                {
                    //redHammerPNG.SetActive(true);
                    info.text = gsp.redTeamName + " has the hammer";
                }
                else
                {
                    //yellowHammerPNG.SetActive(true);
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
                    simButton.gameObject.SetActive(false);
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
                    simButton.gameObject.SetActive(true);

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
                simButton.gameObject.SetActive(true);
                contButton.transform.GetComponentInChildren<Text>().text = "Next End>";

                if (gsp.redHammer)
                {
                    //redHammerPNG.SetActive(true);
                    info.text = gsp.redTeamName + " has the hammer";
                }
                else
                {
                    //yellowHammerPNG.SetActive(true);
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
                if (gsp.KO3)
                    draw.text = "Round " + gsp.playoffRound.ToString();
                else if (gsp.KO1)
                {
                    switch(gsp.playoffRound)
                    {
                        case 0:
                            draw.text = "Round of 16";
                            break;
                        case 1:
                            draw.text = "Quarterfinals";
                            break;
                        case 2:
                            draw.text = "Semifinals";
                            break;
                        case 3:
                            draw.text = "Finals";
                            break;
                    }
                }
                else
                {
                    draw.text = "Playoff Round " + gsp.playoffRound.ToString(); 
                    if (gsp.playoffRound == 1)
                        draw.text = "Round of 16";
                    if (gsp.playoffRound == 2)
                        draw.text = "Quarterfinals";
                    if (gsp.playoffRound == 3)
                        draw.text = "Semifinals";
                    if (gsp.playoffRound == 4)
                        draw.text = "Finals";
                }
            }
            //else
            //{
            //    draw.text = "Draw " + (gsp.draw + 1).ToString();
            //}


            if (cm != null)
                tournyName.text = cm.currentTourny.name;


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

                redTeamName.text = gsp.redTeamName;
                yellowTeamName.text = gsp.yellowTeamName;
                //redTotalScore.text = gsp.redScore.ToString();
                //yellowTotalScore.text = gsp.yellowScore.ToString();

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
            Vector2 tempTotal = Vector2.zero;
            for (int i = 0; i < gsp.score.Length; i++)
            {
                tempTotal.x += gsp.score[i].x;
                tempTotal.y += gsp.score[i].y;
            }
            redTotalScore.text = tempTotal.x.ToString();
            yellowTotalScore.text = tempTotal.y.ToString();
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

    public void SimEnd()
    {
        gsp.endCurrent++;
        gsp.gameInProgress = true;

        if (gsp.ends >= gsp.endCurrent)
        {
            Debug.Log("Game is on!");

            Vector2Int[] tempScore = new Vector2Int[gsp.ends];
            for (int j = 0; j < gsp.endCurrent; j++)
            {
                if (j < gsp.endCurrent - 1)
                {
                    tempScore[j] = gsp.score[j];
                }
                else
                {
                    // --- Improved realism starts here ---

                    // Example: Use average skill from CareerStats or Team objects
                    float redSkill = gsp.redTeam != null ? gsp.redTeam.strength : 5f;
                    float yellowSkill = gsp.yellowTeam != null ? gsp.yellowTeam.strength : 5f;

                    // Add a small random factor for upsets
                    float redChance = redSkill + Random.Range(-1f, 1.5f);
                    float yellowChance = yellowSkill + Random.Range(-1f, 1.5f);

                    // Hammer advantage: team with hammer gets a bonus
                    if (gsp.redHammer)
                        redChance += 1.0f;
                    else
                        yellowChance += 1.0f;

                    // Simulate end result
                    if (redChance > yellowChance)
                    {
                        // Weighted random: 60% chance for 1, 30% for 2, 10% for 3
                        float r = Random.value;
                        int score = (r < 0.6f) ? 1 : (r < 0.9f) ? 2 : 3;
                        tempScore[j].x = score;
                        tempScore[j].y = 0;
                        gsp.redHammer = false;
                    }
                    else if (yellowChance > redChance)
                    {
                        float r = Random.value;
                        int score = (r < 0.6f) ? 1 : (r < 0.9f) ? 2 : 3;
                        tempScore[j].y = score;
                        tempScore[j].x = 0;
                        gsp.redHammer = true;
                    }
                    else
                    {
                        // Blank end
                        tempScore[j].x = 0;
                        tempScore[j].y = 0;
                    }
                }
            }

            gsp.score = new Vector2Int[gsp.ends];
            for (int j = 0; j < gsp.score.Length; j++)
            {
                gsp.score[j] = tempScore[j];
            }
        }
        else
        {
            if (gsp.redScore != gsp.yellowScore)
            {
                Debug.Log("Game is over and not tied");
                Vector2Int[] tempScore = new Vector2Int[gsp.ends];
                for (int j = 0; j < gsp.endCurrent; j++)
                {
                    if (j < gsp.endCurrent - 1)
                        tempScore[j] = gsp.score[j];
                    else
                    {
                        if (Random.Range(0, gsp.redTeam.strength) >= Random.Range(0, gsp.yellowTeam.strength))
                        {
                            tempScore[j].x = Mathf.FloorToInt(Mathf.Pow(Random.value, 2) * 5);
                            if (tempScore[j].x > 0)
                                gsp.redHammer = false;
                        }
                        else
                        {
                            tempScore[j].y = Mathf.FloorToInt(Mathf.Pow(Random.value, 2) * 5);
                            if (tempScore[j].y > 0)
                                gsp.redHammer = true;
                        }
                    }
                }

                Debug.Log("Strength is " + gsp.redTeam.strength + " and " + gsp.yellowTeam.strength);
                gsp.score = new Vector2Int[gsp.ends];
                for (int j = 0; j < gsp.score.Length; j++)
                {
                    gsp.score[j] = tempScore[j];
                }
            }
            else
            {
                Debug.Log("Game is tied and NOT over");
                Vector2Int[] tempScore = new Vector2Int[gsp.endCurrent];
                for (int j = 0; j < gsp.endCurrent; j++)
                {
                    if (j < gsp.endCurrent - 1)
                        tempScore[j] = gsp.score[j];
                    else
                    {
                        bool hammerIsRed = gsp.redHammer;
                        float hammerWeight = 0.75f; // 75% chance hammer team scores
                        float r = Random.value;

                        if (r < hammerWeight)
                        {
                            // Hammer team scores
                            int score = (Random.value < 0.6f) ? 1 : (Random.value < 0.9f) ? 2 : 3;
                            if (hammerIsRed)
                            {
                                tempScore[j].x = score;
                                tempScore[j].y = 0;
                                gsp.redHammer = false;
                            }
                            else
                            {
                                tempScore[j].y = score;
                                tempScore[j].x = 0;
                                gsp.redHammer = true;
                            }
                        }
                        else
                        {
                            // Non-hammer team scores (rare)
                            int score = (Random.value < 0.8f) ? 1 : 2; // Even more likely to be 1
                            if (!hammerIsRed)
                            {
                                tempScore[j].x = score;
                                tempScore[j].y = 0;
                                gsp.redHammer = false;
                            }
                            else
                            {
                                tempScore[j].y = score;
                                tempScore[j].x = 0;
                                gsp.redHammer = true;
                            }
                        }
                        // Tie (blank end) is extremely rare
                        // If you want to allow a tiny chance, you could add:
                        // else if (Random.value < 0.01f) { tempScore[j].x = 0; tempScore[j].y = 0; }
                    }
                }

                Debug.Log("Strength is " + gsp.redTeam.strength + " and " + gsp.yellowTeam.strength);
                gsp.score = new Vector2Int[gsp.endCurrent];
                for (int j = 0; j < gsp.score.Length; j++)
                {
                    gsp.score[j] = tempScore[j];
                }
            }
        }

        Vector2 tempTotal = Vector2.zero;

        for (int i = 0; i < gsp.score.Length; i++)
        {
            tempTotal.x += gsp.score[i].x;
            tempTotal.y += gsp.score[i].y;
        }

        gsp.redScore = (int)tempTotal.x;
        gsp.yellowScore = (int)tempTotal.y;

        if (ends > gsp.endCurrent)
        {
            Debug.Log("Game is being played");
            if (gsp.redHammer)
            {
                //redHammerPNG.SetActive(true);
                info.text = gsp.redTeamName + " has the hammer";
            }
            else
            {
                //yellowHammerPNG.SetActive(true);
                info.text = gsp.yellowTeamName + " has the hammer";
            }

            if (gsp.KO1)
            {
                if (gsp.playoffRound == 1)
                {
                    draw.text = "Round of 16";
                }
                if (gsp.playoffRound == 2)
                {
                    draw.text = "Quarterfinals";
                }
                if (gsp.playoffRound == 3)
                {
                    draw.text = "Semifinals";
                }
                if (gsp.playoffRound == 4)
                {
                    draw.text = "Finals";
                }
            }
            else if (gsp.KO3)
            {
                draw.text = "Round " + gsp.playoffRound.ToString();
            }
            else if (gsp.cashGame)
            {
                draw.text = "Cash Game";
            }
            else
            {
                draw.text = "Draw " + (gsp.draw + 1).ToString();

                if (gsp.playoffRound == 1)
                {
                    draw.text = "Round of 16";
                }
                if (gsp.playoffRound == 2)
                {
                    draw.text = "Quarterfinals";
                }
                if (gsp.playoffRound == 3)
                {
                    draw.text = "Semifinals";
                }
                if (gsp.playoffRound == 4)
                {
                    draw.text = "Finals";
                }
            }
            end.text = "End " + (gsp.endCurrent + 1).ToString() + "/" + ends;
        }
        else
        {
            if (gsp.redScore != gsp.yellowScore)
            {
                Debug.Log("Game is not tied and over");
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
                simButton.gameObject.SetActive(false);
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
                Debug.Log("Game is tied and extra end!");
                ends++;
                contButton.gameObject.SetActive(true);
                endButton.gameObject.SetActive(false);
                simButton.gameObject.SetActive(true);

                draw.text = "Draw " + (gsp.draw + 1).ToString();

                if (gsp.cashGame)
                {
                    draw.text = "Cash Game";
                }

                end.text = "Extra End!";

                if (gsp.redScore != gsp.yellowScore)
                    simButton.gameObject.SetActive(false);
            }
        }
        
        
        for (int i = 0; i < ends; i++)
        {
            scoreCols[i].SetActive(true);
            scoreCols[i].transform.GetChild(0).gameObject.SetActive(true);

            scoreCols[i].transform.GetChild(1).gameObject.SetActive(false);
            scoreCols[i].transform.GetChild(2).gameObject.SetActive(false);
            Debug.Log("I IS " + i);

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

        tempTotal = Vector2.zero;
        for (int i = 0; i < gsp.score.Length; i++)
        {
            tempTotal.x += gsp.score[i].x;
            tempTotal.y += gsp.score[i].y;
        }

        redTotalScore.text = tempTotal.x.ToString();
        gsp.redScore = (int)tempTotal.x;
        yellowTotalScore.text = tempTotal.y.ToString();
        gsp.yellowScore = (int)tempTotal.y;
        //gsp.LoadFromEndMenu();
    }

    Vector2Int SimScore(int xx, Team redTeam, Team yellowTeam)
    {
        Vector2Int[] tempScore = new Vector2Int[gsp.ends];
        for (int j = 0; j < gsp.endCurrent; j++)
        {
            if (j < gsp.endCurrent - 1)
                tempScore[j] = gsp.score[j];
            else
            {
                if (Random.Range(0, redTeam.strength) >= Random.Range(0, redTeam.strength))
                {
                    tempScore[j].x = Random.Range(0, redTeam.strength);
                    if (tempScore[j].x > 0)
                        gsp.redHammer = false;
                }
                else
                {
                    tempScore[j].y = Random.Range(0, redTeam.strength);
                    if (tempScore[j].y > 0)
                        gsp.redHammer = true;
                }
            }
        }
        Debug.Log("Strength is " + gsp.redTeam.strength + " and " + gsp.yellowTeam.strength);

        gsp.score = new Vector2Int[tempScore.Length];
        for (int j = 0; j < gsp.score.Length; j++)
        {
            gsp.score[j] = tempScore[j];
        }
        return tempScore[xx];
    }
    public void EndGame()
    {

        if (gsp.cashGame)
        {
            float winnings;

            gsp.tournyInProgress = false;
            Debug.Log("gsp.inProgress is " + gsp.tournyInProgress);
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

            gsp.tournyCash += winnings;
            cm.cash += gsp.tournyCash;

            SceneManager.LoadScene("Arena_Selector");
        }

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
        }
        if (gsp.KO3)
            SceneManager.LoadScene("Tourny_Home_3K");
        else if (gsp.KO1)
            SceneManager.LoadScene("Tourny_Home_SingleK");
        else
            SceneManager.LoadScene("Tourny_Home_1");
    }
}
