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
        gsp = FindFirstObjectByType<GameSettingsPersist>();
        cm = FindFirstObjectByType<CareerManager>();

        if (gsp)
        {
            gsp.loadGame = false;
            ends = gsp.ends;
            
            // ? CRITICAL FIX: Ensure score array exists BEFORE any calculations
            if (gsp.score == null || gsp.score.Length != ends)
            {
                Debug.LogWarning($"[EndMenu.Start] Score array invalid (null or length {gsp.score?.Length} != {ends}) - initializing");
                gsp.score = new Vector2Int[ends];
                for (int i = 0; i < ends; i++)
                {
                    gsp.score[i] = new Vector2Int(0, 0);
                }
            }
            
            // ? CRITICAL FIX: Recalculate totals from score array FIRST!
            // This MUST happen BEFORE any winner determination logic
            Vector2 recalculatedTotal = Vector2.zero;
            for (int i = 0; i < Mathf.Min(gsp.endCurrent, gsp.score.Length); i++)
            {
                recalculatedTotal.x += gsp.score[i].x;
                recalculatedTotal.y += gsp.score[i].y;
            }
            
            // Update gsp totals to match calculated values
            gsp.redScore = (int)recalculatedTotal.x;
            gsp.yellowScore = (int)recalculatedTotal.y;
            
            Debug.Log($"[EndMenu.Start] Scores recalculated from array - Red: {gsp.redScore}, Yellow: {gsp.yellowScore} (from {gsp.endCurrent} completed ends)");

            if (gsp.cashGame)
            {
                end.text = "Cash Game";
            }

            if (gsp.endCurrent == 0)
            {
                // CRITICAL FIX: Reset scores AND score array at start of new game
                gsp.redScore = 0;
                gsp.yellowScore = 0;
                
                // Clear previous game's score array
                gsp.score = new Vector2Int[ends];
                for (int i = 0; i < ends; i++)
                {
                    gsp.score[i] = new Vector2Int(0, 0);
                }
                Debug.Log($"[EndMenu] New game - cleared score array ({ends} ends)");
                
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
            }
            
            redTeamName.text = gsp.redTeamName;
            yellowTeamName.text = gsp.yellowTeamName;

            // Now populate the scoreboard UI
            for (int i = 0; i < ends; i++)
            {
                if (i < gsp.endCurrent)
                {
                    // Past ends - show actual scores
                    scoreCols[i].transform.GetChild(1).gameObject.SetActive(true);
                    scoreCols[i].transform.GetChild(2).gameObject.SetActive(true);
                    scoreCols[i].transform.GetChild(1).GetComponent<Text>().text = gsp.score[i].x.ToString();
                    scoreCols[i].transform.GetChild(2).GetComponent<Text>().text = gsp.score[i].y.ToString();
                }
                else if (i == gsp.endCurrent)
                {
                    // Current end - show hammer
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
            
            // CRITICAL FIX: Recalculate total scores from score array to ensure consistency
            Vector2 tempTotal = Vector2.zero;
            for (int i = 0; i < Mathf.Min(gsp.endCurrent, gsp.score.Length); i++)
            {
                tempTotal.x += gsp.score[i].x;
                tempTotal.y += gsp.score[i].y;
            }
            
            // Update gsp scores to match calculated totals
            gsp.redScore = (int)tempTotal.x;
            gsp.yellowScore = (int)tempTotal.y;
            
            redTotalScore.text = tempTotal.x.ToString();
            yellowTotalScore.text = tempTotal.y.ToString();
            
            Debug.Log($"[EndMenu] Score totals recalculated - Red: {tempTotal.x}, Yellow: {tempTotal.y} (from {gsp.endCurrent} ends)");
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
        CareerManager cm = FindFirstObjectByType<CareerManager>();
        gsp = FindFirstObjectByType<GameSettingsPersist>();

        SceneManager.LoadScene("TournyGame");
    }

	public void SimEnd()
	{
		Debug.Log("[EndMenu] SimEnd called - simulating to end of game");
        gsp.endCurrent++;
        gsp.gameInProgress = true;

        int endsLeft = gsp.ends - gsp.endCurrent;
        bool lastTwoEnds = gsp.endCurrent >= gsp.ends - 1; // true for last two ends

        if (gsp.ends >= gsp.endCurrent)
        {
            Debug.Log("Game is on!");

            // CRITICAL FIX: Preserve existing scores, don't create new array from scratch
            Vector2Int[] tempScore = new Vector2Int[gsp.ends];
            
            // Copy ALL existing scores first
            if (gsp.score != null)
            {
                for (int j = 0; j < Mathf.Min(gsp.score.Length, tempScore.Length); j++)
                {
                    tempScore[j] = gsp.score[j];
                }
            }
            
            // Now only simulate/update the CURRENT end (endCurrent - 1)
            int currentEndIndex = gsp.endCurrent - 1;
            if (currentEndIndex >= 0 && currentEndIndex < tempScore.Length)
            {
                // --- Improved realism using actual career stats ---

                // FIXED: Use cm.cStats for player, Team.strength for opponent
                float redSkill = 5f;
                float yellowSkill = 5f;

                if (cm != null && gsp.redTeam != null && gsp.yellowTeam != null)
                {
                    // Determine which team is player and which is opponent
                    if (gsp.redTeam.player)
                    {
                        // Player is red team - use cm.cStats (includes equipment!)
                        float playerAvg = (cm.cStats.drawAccuracy + cm.cStats.guardAccuracy + cm.cStats.takeOutAccuracy) / 3f;
                        
                        // Opponent is yellow team - use their team strength
                        float oppTeamStrength = gsp.yellowTeam.strength;
                        
                        redSkill = playerAvg / 10f;  // Normalize to ~1-10 range (stats are 0-100)
                        yellowSkill = oppTeamStrength / 100f;  // Team strength is typically 100-500
                        
                        Debug.Log($"[SimEnd] Player (red) cStats avg: {playerAvg} ? skill {redSkill:F2}, Opponent (yellow) strength: {oppTeamStrength} ? skill {yellowSkill:F2}");
                    }
                    else if (gsp.yellowTeam.player)
                    {
                        // Player is yellow team - use cm.cStats (includes equipment!)
                        float playerAvg = (cm.cStats.drawAccuracy + cm.cStats.guardAccuracy + cm.cStats.takeOutAccuracy) / 3f;
                        
                        // Opponent is red team - use their team strength
                        float oppTeamStrength = gsp.redTeam.strength;
                        
                        yellowSkill = playerAvg / 10f;
                        redSkill = oppTeamStrength / 100f;
                        
                        Debug.Log($"[SimEnd] Player (yellow) cStats avg: {playerAvg} ? skill {yellowSkill:F2}, Opponent (red) strength: {oppTeamStrength} ? skill {redSkill:F2}");
                    }
                    else
                    {
                        // AI vs AI - use team strength for both
                        redSkill = gsp.redTeam.strength / 100f;
                        yellowSkill = gsp.yellowTeam.strength / 100f;
                        Debug.Log($"[SimEnd] AI vs AI - red strength: {gsp.redTeam.strength} ? {redSkill:F2}, yellow strength: {gsp.yellowTeam.strength} ? {yellowSkill:F2}");
                    }
                }
                else
                {
                    // Fallback to team strength if CareerManager not found
                    redSkill = gsp.redTeam != null ? gsp.redTeam.strength / 100f : 0.5f;
                    yellowSkill = gsp.yellowTeam != null ? gsp.yellowTeam.strength / 100f : 0.5f;
                    Debug.LogWarning($"[SimEnd] Using fallback team strength - red: {redSkill:F2}, yellow: {yellowSkill:F2}");
                }

                // Add a small random factor for upsets
                float redChance = redSkill + Random.Range(-1f, 1.5f);
                float yellowChance = yellowSkill + Random.Range(-1f, 1.5f);

                // Hammer advantage: team with hammer gets a bonus
                if (gsp.redHammer)
                    redChance += 1.0f;
                else
                    yellowChance += 1.0f;
                
                Debug.Log($"[SimEnd] Final chances - Red: {redChance:F2}, Yellow: {yellowChance:F2}");
                
                // --- Blank end logic ---
                if (!lastTwoEnds)
                {
                    // Early/mid game: favor blank ends (e.g., 40% chance)
                    float blankChance = 0.4f;
                    if (Random.value < blankChance)
                    {
                        tempScore[currentEndIndex].x = 0;
                        tempScore[currentEndIndex].y = 0;
                        // Hammer is retained - don't change it
                        Debug.Log($"[SimEnd] Blank end - hammer retained by {(gsp.redHammer ? "red" : "yellow")}");
                    }
                    else
                    {
                        // Score points based on chances
                        SimulateScoringEnd(ref tempScore[currentEndIndex], redChance, yellowChance, lastTwoEnds);
                    }
                }
                else
                {
                    // Last two ends: force a score
                    SimulateScoringEnd(ref tempScore[currentEndIndex], redChance, yellowChance, lastTwoEnds);
                }
            }

            // Update the score array (already has all previous scores preserved)
            gsp.score = tempScore;
            
            Debug.Log($"[EndMenu.SimEnd] Updated end {currentEndIndex + 1} - preserved all {tempScore.Length} ends");
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

        // CRITICAL FIX: Recalculate total scores from the COMPLETE score array
        Vector2 tempTotal = Vector2.zero;
        for (int i = 0; i < gsp.score.Length; i++)
        {
            tempTotal.x += gsp.score[i].x;
            tempTotal.y += gsp.score[i].y;
        }

        gsp.redScore = (int)tempTotal.x;
        gsp.yellowScore = (int)tempTotal.y;
        
        Debug.Log($"[EndMenu.SimEnd] Total scores recalculated - Red: {gsp.redScore}, Yellow: {gsp.yellowScore} (from {gsp.score.Length} ends)");

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
        
        // CRITICAL FIX: If game is over after simulation, show the End Game button
        // Player clicks it manually to see the result before advancing
        if (gsp.endCurrent >= gsp.ends && gsp.redScore != gsp.yellowScore)
        {
            Debug.Log("[EndMenu.SimEnd] Game complete after simulation - player must click End Game button");
            // The button is already visible from the UI logic above, player clicks it to continue
        }
    }

    /// <summary>
    /// Helper method to simulate scoring an end (not blank)
    /// </summary>
    private void SimulateScoringEnd(ref Vector2Int endScore, float redChance, float yellowChance, bool lastTwoEnds)
    {
        if (redChance > yellowChance)
        {
            // Red scores
            float r = Random.value;
            int score = (r < 0.6f) ? 1 : (r < 0.9f) ? 2 : 3;
            endScore.x = score;
            endScore.y = 0;
            gsp.redHammer = false;
            Debug.Log($"[SimEnd] Red scores {score}");
        }
        else if (yellowChance > redChance)
        {
            // Yellow scores
            float r = Random.value;
            int score = (r < 0.6f) ? 1 : (r < 0.9f) ? 2 : 3;
            endScore.y = score;
            endScore.x = 0;
            gsp.redHammer = true;
            Debug.Log($"[SimEnd] Yellow scores {score}");
        }
        else
        {
            // Tie - force hammer team to score if last two ends
            if (lastTwoEnds)
            {
                if (gsp.redHammer)
                {
                    endScore.x = 1;
                    endScore.y = 0;
                    gsp.redHammer = false;
                }
                else
                {
                    endScore.y = 1;
                    endScore.x = 0;
                    gsp.redHammer = true;
                }
                Debug.Log($"[SimEnd] Tie broken - hammer team scores 1");
            }
            else
            {
                // Blank end
                endScore.x = 0;
                endScore.y = 0;
                Debug.Log($"[SimEnd] Blank end (tie)");
            }
        }
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
        // CRITICAL FIX: Set justFinishedGame flag FIRST, before any scene loading
        // This ensures playoff managers know to process the game result
        gsp.justFinishedGame = true;
        Debug.Log("[EndMenu.EndGame] Set justFinishedGame = true");

        if (gsp.cashGame)
        {
            float winnings;

            gsp.tournyInProgress = false;
            Debug.Log("gsp.inProgress is " + gsp.tournyInProgress);
            Debug.Log("CM Record is " + cm.record.x + " - " + cm.record.y);
            Debug.Log("CM earnings are " + cm.earnings);

            cm.TournyResults();
            cm.LoadCareer();
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
            // CRITICAL FIX: Only call LoadFromEndMenu for PLAYOFFS
            // Regular tournaments handle wins/losses in TournyManager.ProcessPlayerMatchResult()
            if (gsp.playoffRound > 0)
            {
                // Playoffs: Update team records in GSP
                gsp.LoadFromEndMenu();
                Debug.Log("[EndMenu] Playoff game - updated team records via LoadFromEndMenu");
            }
            else
            {
                // Regular tournament: Don't update here - TournyManager will handle it
                Debug.Log("[EndMenu] Regular tournament - TournyManager will process player match result");
            }
            
            // CRITICAL FIX: Don't simulate other games here!
            // For regular tournaments, TournyManager.SimRestDraw() handles ALL AI game simulation
            // For playoffs, there are no "other games" to simulate (single-elimination bracket)
            // Simulating here would cause DOUBLE simulation for regular tournaments!
            
            // Clear gameInProgress flag - we're done with this game
            gsp.gameInProgress = false;
            Debug.Log("[EndMenu] Game finished - cleared gameInProgress flag");
            
            // Keep tournyInProgress = true so we route back to tournament home
            gsp.tournyInProgress = true;
            
            if (gsp.playoffRound > 0)
            {
                Debug.Log("[EndMenu] Playoff Round - " + gsp.playoffRound);
            }
            else
            {
                gsp.draw++;
                Debug.Log("[EndMenu] Incremented draw to " + gsp.draw);
            }
            
            // Save with updated team records AND simulated games
            if (cm != null)
            {
                Debug.Log("[EndMenu] Saving career with all game results...");
                cm.SaveCareer();
                Debug.Log("[EndMenu] Save complete - loading tournament home scene");
            }
            else
            {
                Debug.LogWarning("[EndMenu] CareerManager not found - skipping save (non-career mode?)");
            }
        }
        
        // Load appropriate tournament home scene
        // CRITICAL FIX: Use correct scene names matching TournySettings
        if (gsp.KO3)
        {
            SceneManager.LoadScene("Tourny_Home_1");  // Triple-K uses standard home
        }
        else if (gsp.KO1)
        {
            SceneManager.LoadScene("Tourny_Home_SingleK");
        }
        else
        {
            SceneManager.LoadScene("Tourny_Home_1");
        }
    }
    
    /// <summary>
    /// Simulates games for all OTHER teams in the current draw (excluding player's game)
    /// </summary>
    private void SimulateOtherGames()
    {
        if (cm == null || cm.currentTourny == null || gsp.teams == null)
        {
            Debug.LogWarning("[EndMenu] Cannot simulate other games - missing data");
            return;
        }
        
        Debug.Log($"[EndMenu] Simulating other teams' games for draw {gsp.draw}");
        
        // Track which teams have been processed to avoid double-counting
        HashSet<string> processedTeams = new HashSet<string>();
        
        // Find player's team and opponent to skip them
        Team playerTeam = null;
        Team playerOpp = null;
        
        foreach (var team in gsp.teams)
        {
            if (team.player)
            {
                playerTeam = team;
                // Find the opponent
                foreach (var t in gsp.teams)
                {
                    if (t.name == playerTeam.nextOpp)
                    {
                        playerOpp = t;
                        break;
                    }
                }
                break;
            }
        }
        
        if (playerTeam != null && playerOpp != null)
        {
            processedTeams.Add(playerTeam.name);
            processedTeams.Add(playerOpp.name);
            Debug.Log($"[EndMenu] Skipping player game: {playerTeam.name} vs {playerOpp.name}");
        }
        
        // Simulate remaining games
        foreach (var team1 in gsp.teams)
        {
            // Skip if already processed
            if (processedTeams.Contains(team1.name))
                continue;
            
            // Find this team's opponent
            Team team2 = null;
            foreach (var t in gsp.teams)
            {
                if (t.name == team1.nextOpp)
                {
                    team2 = t;
                    break;
                }
            }
            
            if (team2 != null && !processedTeams.Contains(team2.name))
            {
                // Simulate this game based on team strength
                bool team1Wins = Random.Range(0, team1.strength) > Random.Range(0, team2.strength);
                
                if (team1Wins)
                {
                    team1.wins++;
                    team2.loss++;
                    Debug.Log($"[EndMenu] Simulated: {team1.name} defeats {team2.name}");
                }
                else
                {
                    team2.wins++;
                    team1.loss++;
                    Debug.Log($"[EndMenu] Simulated: {team2.name} defeats {team1.name}");
                }
                
                // Mark both teams as processed
                processedTeams.Add(team1.name);
                processedTeams.Add(team2.name);
            }
        }
        
        Debug.Log($"[EndMenu] Simulation complete - processed {processedTeams.Count} teams");
    }
}
