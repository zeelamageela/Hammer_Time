using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TigerForge;

public class PlayoffManager : MonoBehaviour
{
	public TournyManager tm;
	public Team[] playoffTeams;

	public BracketDisplay[] brackDisplay;

	public GameObject[] row;
	public GameObject playoffs;
	public Button nextButton;
	public Button simButton;
	public Button contButton;
	public Button playButton;
	public Text heading;
	public Scrollbar scrollBar;
	public Text careerEarningsText;

	GameSettingsPersist gsp;
    CareerManager cm;

    EasyFileSave myFile;
	//int pTeams;
	public int playerTeam;
	public int oppTeam;
	public int playoffRound;

	public float careerEarnings;
	public Vector2 careerRecord;

	private void Start()
	{
		gsp = FindFirstObjectByType<GameSettingsPersist>();
		cm = FindFirstObjectByType<CareerManager>();

        playoffs.SetActive(true);

		careerEarnings = tm.careerEarnings;
		careerRecord = tm.careerRecord;

		playerTeam = tm.playerTeam;
		playoffRound = gsp.playoffRound;
		playoffTeams = new Team[9];

		Debug.Log($"[PlayoffManager.Start] playoffRound={playoffRound}, careerLoad={gsp.careerLoad}, gameInProgress={gsp.gameInProgress}, justFinishedGame={gsp.justFinishedGame}");

		// CRITICAL FIX: Handle four distinct scenarios:
		// 1. Fresh tournament start (playoffRound == 0)
		// 2. Returning from a completed game (justFinishedGame == true, need to advance)
		// 3. Loading saved tournament between games (careerLoad == true, !justFinishedGame)
		// 4. Unexpected fallback

		if (gsp.justFinishedGame)
		{
			// Scenario 2: Just returned from completing a game - need to process result and advance
			// This takes priority over careerLoad because we MUST process the game result
			Debug.Log("[PlayoffManager.Start] SCENARIO 2: Returning from completed game - advancing playoffs");
			LoadAndAdvancePlayoffs();
			gsp.justFinishedGame = false; // Clear flag after processing
		}
		else if (gsp.careerLoad)
		{
			// Scenario 3: Loading saved tournament, player is between games - just restore state
			Debug.Log("[PlayoffManager.Start] SCENARIO 3: Loading saved tournament (between games) - restoring state");
			LoadPlayoffs();
		}
		else if (playoffRound == 0)
		{
			// Scenario 1: Fresh tournament start
			Debug.Log("[PlayoffManager.Start] SCENARIO 1: Fresh tournament start - setting seeding");
			SetSeeding(tm.teams.Length);
		}
		else
		{
			// Fallback: Unexpected state - try to load current playoff state
			Debug.LogWarning($"[PlayoffManager.Start] FALLBACK: Unexpected state - loading playoff state (playoffRound={playoffRound})");
			LoadPlayoffs();
		}
	}

	public void SetSeeding(int numberOfTeams)
    {
		//pTeams = 4;
		playoffTeams = new Team[9];
		heading.text = "Page Playoff";

		playoffRound++;
		
		// Check if this is a two-pool tournament
		if (tm.isTwoPoolTournament)
		{
			Debug.Log("[PlayoffManager] Two-pool tournament - selecting top 2 from each pool");
			
			// Separate teams by pool
			List<Team_List> poolA = new List<Team_List>();
			List<Team_List> poolB = new List<Team_List>();
			
			foreach (var teamEntry in tm.teamList)
			{
				if (teamEntry.team.poolId == 0)
					poolA.Add(teamEntry);
				else if (teamEntry.team.poolId == 1)
					poolB.Add(teamEntry);
			}
			
			// Sort each pool separately
			poolA.Sort();
			poolB.Sort();
			
			// Take top 2 from each pool and assign seeding
			// Seeding: 1A (rank 1), 1B (rank 2), 2A (rank 3), 2B (rank 4)
			if (poolA.Count >= 2 && poolB.Count >= 2)
			{
				playoffTeams[0] = poolA[0].team;
				playoffTeams[0].rank = 1;
				
				playoffTeams[1] = poolB[0].team;
				playoffTeams[1].rank = 2;
				
				playoffTeams[2] = poolA[1].team;
				playoffTeams[2].rank = 3;
				
				playoffTeams[3] = poolB[1].team;
				playoffTeams[3].rank = 4;
				
				Debug.Log($"[PlayoffManager] Pool A Top 2: {poolA[0].team.name}, {poolA[1].team.name}");
				Debug.Log($"[PlayoffManager] Pool B Top 2: {poolB[0].team.name}, {poolB[1].team.name}");
				Debug.Log($"[PlayoffManager] Playoff seeding: 1-{playoffTeams[0].name}, 2-{playoffTeams[1].name}, 3-{playoffTeams[2].name}, 4-{playoffTeams[3].name}");
			}
			else
			{
				Debug.LogError("[PlayoffManager] Not enough teams in pools for playoffs!");
			}
		}
		else
		{
			// Standard seeding - take top 4 teams
			for (int i = 0; i < 4; i++)
			{
				playoffTeams[i] = tm.teamList[i].team;
			}
		}
		
		// Set up bracket display
		for (int i = 0; i < playoffTeams.Length; i++)
		{
			if (i < 4)
            {
				brackDisplay[i].name.text = playoffTeams[i].name;
				brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
			}
			else
            {
				playoffTeams[i] = tm.tTeamList.nullTeam;
            }
		}
		
		tm.playoffRound = playoffRound;
		SetPlayoffs();
	}

	IEnumerator RefreshPlayoffPanel()
	{
		for (int i = 0; i < brackDisplay.Length; i++)
		{
			brackDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = false;

			yield return new WaitForEndOfFrame();
			brackDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = true;
		}

		//for (int i = 0; i < tm.vsDisplay.Length; i++)
		//{
		//	tm.vsDisplay[i].name.gameObject.GetComponent<ContentSizeFitter>().enabled = false;

		//	yield return new WaitForEndOfFrame();
		//	tm.vsDisplay[i].name.gameObject.GetComponent<ContentSizeFitter>().enabled = true;
		//}
	}

	#region Page Playoff Helper Methods

	/// <summary>
	/// Displays teams for Page Playoff bracket with optional player highlighting
	/// </summary>
	void DisplayPagePlayoffTeams(int displayCount, bool highlightPlayer = false)
    {
        for (int i = 0; i < displayCount; i++)
        {
            brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
            brackDisplay[i].name.text = playoffTeams[i].name;
            brackDisplay[i].name.transform.parent.gameObject.SetActive(true);
            row[i].SetActive(true);

            if (playoffTeams[i].player && highlightPlayer)
            {
                brackDisplay[i].bg.GetComponent<Image>().color = Color.yellow;
            }
        }
    }

    /// <summary>
    /// Sets up VS display for Page Playoff system
    /// Returns true if player is active, false if knocked out
    /// </summary>
    bool SetupPagePlayoffVsDisplay()
    {
        // Find player's team in playoff teams
        Team playerTeamData = null;
        int playerPlayoffIndex = -1;
        
        for (int i = 0; i < playoffTeams.Length; i++)
        {
            if (playoffTeams[i] != null && playoffTeams[i].player)
            {
                playerTeamData = playoffTeams[i];
                playerPlayoffIndex = i;
                break;
            }
        }
        
        if (playerTeamData == null)
        {
            Debug.LogWarning("[PlayoffManager] Player team not found in playoff teams");
            return false;
        }
        
        int playerRank = playerTeamData.rank;
        
        // Always set up the player's side of VS display
        tm.vsDisplay[0].name.text = playerTeamData.name;
        tm.vsDisplay[0].rank.text = playerRank.ToString();

        switch (playoffRound)
        {
            case 1:
                if (playerRank >= 1 && playerRank <= 4)
                {
                    int opponentRank = (playerRank % 2 == 1) ? playerRank + 1 : playerRank - 1;
                    tm.vsDisplay[1].name.text = playoffTeams[opponentRank - 1].name;
                    tm.vsDisplay[1].rank.text = playoffTeams[opponentRank - 1].rank.ToString();
                    if (tm.teams != null && playerTeam >= 0 && playerTeam < tm.teams.Length)
                        tm.teams[playerTeam].nextOpp = playoffTeams[opponentRank - 1].name;
                    return true;
                }
                break;

            case 2:
                if (playoffTeams[4].name == playerTeamData.name)
                {
                    // Winner of 1v2 gets BYE to finals
                    tm.vsDisplay[1].name.text = "BYE TO FINALS";
                    tm.vsDisplay[1].rank.text = "-";
                    if (tm.teams != null && playerTeam >= 0 && playerTeam < tm.teams.Length)
                        tm.teams[playerTeam].nextOpp = playoffTeams[4].name;
                    return true; // Player IS active, just has a bye (changed from false)
                }
                else if (playoffTeams[5].name == playerTeamData.name)
                {
                    tm.vsDisplay[1].name.text = playoffTeams[6].name;
                    tm.vsDisplay[1].rank.text = playoffTeams[6].rank.ToString();
                    if (tm.teams != null && playerTeam >= 0 && playerTeam < tm.teams.Length)
                        tm.teams[playerTeam].nextOpp = playoffTeams[6].name;
                    return true;
                }
                else if (playoffTeams[6].name == playerTeamData.name)
                {
                    tm.vsDisplay[1].name.text = playoffTeams[5].name;
                    tm.vsDisplay[1].rank.text = playoffTeams[5].rank.ToString();
                    if (tm.teams != null && playerTeam >= 0 && playerTeam < tm.teams.Length)
                        tm.teams[playerTeam].nextOpp = playoffTeams[5].name;
                    return true;
                }
                break;

            case 3:
                if (playoffTeams[4].name == playerTeamData.name)
                {
                    tm.vsDisplay[1].name.text = playoffTeams[7].name;
                    tm.vsDisplay[1].rank.text = playoffTeams[7].rank.ToString();
                    if (tm.teams != null && playerTeam >= 0 && playerTeam < tm.teams.Length)
                        tm.teams[playerTeam].nextOpp = playoffTeams[7].name;
                    return true;
                }
                else if (playoffTeams[7].name == playerTeamData.name)
                {
                    tm.vsDisplay[1].name.text = playoffTeams[4].name;
                    tm.vsDisplay[1].rank.text = playoffTeams[4].rank.ToString();
                    if (tm.teams != null && playerTeam >= 0 && playerTeam < tm.teams.Length)
                        tm.teams[playerTeam].nextOpp = playoffTeams[4].name;
                    return true;
                }
                break;
        }

        return false; // Player knocked out
    }

    /// <summary>
    /// Processes player's match result for Page Playoff
    /// </summary>
    void ProcessPagePlayoffMatchResult(bool playerWon, int round, bool isGame1)
    {
        // Update wins/losses for player and opponent
        if (playerWon)
        {
            tm.teams[playerTeam].wins++;
            tm.teams[oppTeam].loss++;
            Debug.Log($"[PlayoffManager] Player won! {tm.teams[playerTeam].name}: {tm.teams[playerTeam].wins}W-{tm.teams[playerTeam].loss}L");
        }
        else
        {
            tm.teams[oppTeam].wins++;
            tm.teams[playerTeam].loss++;
            Debug.Log($"[PlayoffManager] Player lost. {tm.teams[playerTeam].name}: {tm.teams[playerTeam].wins}W-{tm.teams[playerTeam].loss}L");
        }
        
        // Move teams to appropriate bracket positions
        switch (round)
        {
            case 1:
                if (isGame1) // 1v2 match
                {
                    if (playerWon)
                    {
                        playoffTeams[4] = tm.teams[playerTeam];
                        playoffTeams[5] = tm.teams[oppTeam];
                    }
                    else
                    {
                        playoffTeams[5] = tm.teams[playerTeam];
                        playoffTeams[4] = tm.teams[oppTeam];
                    }
                }
                else // 3v4 match
                {
                    playoffTeams[6] = playerWon ? tm.teams[playerTeam] : tm.teams[oppTeam];
                }
                break;

            case 2:
                playoffTeams[7] = playerWon ? tm.teams[playerTeam] : tm.teams[oppTeam];
                break;

            case 3:
                playoffTeams[8] = playerWon ? tm.teams[playerTeam] : tm.teams[oppTeam];
                break;
        }
    }

    /// <summary>
    /// Configures UI buttons for Page Playoff
    /// playerHasGame: true if player has an actual game to play (not knocked out, not BYE)
    /// </summary>
    void ConfigurePagePlayoffButtons(bool playerHasGame)
    {
        if (playerHasGame)
        {
            // Player has a game - show ONLY Play button
            playButton.gameObject.SetActive(true);
            simButton.gameObject.SetActive(false);
            contButton.gameObject.SetActive(false);
        }
        else
        {
            // Player knocked out, has BYE, or all AI games - show ONLY Sim button
            playButton.gameObject.SetActive(false);
            simButton.gameObject.SetActive(true);
            contButton.gameObject.SetActive(false);
        }
    }

    #endregion

	void LoadAndAdvancePlayoffs()
	{
		Debug.Log($"[LoadAndAdvancePlayoffs] Loading playoffs - Round {playoffRound}");
		
		// CRITICAL FIX: Check if playoff teams exist before loading (can be null from fresh round robin)
		if (gsp.playoffTeams != null && gsp.playoffTeams.Length > 0)
		{
			for (int i = 0; i < playoffTeams.Length && i < gsp.playoffTeams.Length; i++)
				playoffTeams[i] = gsp.playoffTeams[i];
			Debug.Log($"[LoadAndAdvancePlayoffs] Loaded {gsp.playoffTeams.Length} teams from save");
		}
		else
		{
			Debug.LogWarning("[LoadAndAdvancePlayoffs] No saved playoff teams - just advanced from round robin");
		}
		//playoffTeams = gsp.playoffTeams;

		for (int i = 0; i < tm.teams.Length; i++)
		{
			if (tm.teams[i].player)
				playerTeam = i;
		}
        // CRITICAL FIX: Find opponent using game scores instead of nextOpp
        oppTeam = -1;

        if (playerTeam >= 0 && playerTeam < tm.teams.Length)
        {
            string playerTeamName = tm.teams[playerTeam].name;

            if (playerTeamName == gsp.redTeamName)
            {
                for (int i = 0; i < tm.teams.Length; i++)
                {
                    if (tm.teams[i].name == gsp.yellowTeamName)
                    {
                        oppTeam = i;
                        break;
                    }
                }
            }
            else if (playerTeamName == gsp.yellowTeamName)
            {
                for (int i = 0; i < tm.teams.Length; i++)
                {
                    if (tm.teams[i].name == gsp.redTeamName)
                    {
                        oppTeam = i;
                        break;
                    }
                }
            }
        }

        if (oppTeam < 0)
        {
            Debug.LogError("[LoadAndAdvancePlayoffs] Could not find opponent!");
            playoffRound++;
            SetPlayoffs();
            return;
        }

        Debug.Log("OppTeam is " + oppTeam);

        bool playerWon = SharedTournamentLogic.DeterminePlayerWon(gsp);
	
	switch (playoffRound)
		{
			case 1:
				bool isGame1 = false;
				bool isGame2 = false;
				
				for (int i = 0; i < 4; i++)
				{
					if (playoffTeams[i].player)
					{
						if (i < 2)
							isGame1 = true;
						else
							isGame2 = true;
					}
				}

				
				ProcessPagePlayoffMatchResult(playerWon, 1, isGame1);
				
				if (!isGame1) // Simulate game 1 if player wasn't in it
				{
					if (SharedTournamentLogic.SimulateMatch(playoffTeams[0], playoffTeams[1]) == playoffTeams[0].id)
					{
						playoffTeams[4] = playoffTeams[0];
						playoffTeams[5] = playoffTeams[1];
					}
					else
					{
						playoffTeams[4] = playoffTeams[1];
						playoffTeams[5] = playoffTeams[0];
					}
				}
				
				if (!isGame2) // Simulate game 2 if player wasn't in it
				{
					playoffTeams[6] = SharedTournamentLogic.SimulateMatch(playoffTeams[2], playoffTeams[3]) == playoffTeams[2].id
						? playoffTeams[2]
						: playoffTeams[3];
				}
				
				StartCoroutine(SimPlayoff(isGame1, isGame2));
				break;

			case 2:
				// Special handling for Round 2:
				// - Team at position 4 has BYE (automatic advance to finals)
				// - Only teams at positions 5 and 6 play a match
				
				// Check if player is the team with BYE
				if (playoffTeams[4].player)
				{
					// Player has BYE - no match result to process
					// Team 4 automatically stays at position 4 and will advance to finals
					Debug.Log("[LoadAndAdvancePlayoffs] Player has BYE to finals");
					
					// Still need to simulate the 5v6 match if player wasn't in it
					if (SharedTournamentLogic.SimulateMatch(playoffTeams[5], playoffTeams[6]) == playoffTeams[5].id)
					{
						playoffTeams[7] = playoffTeams[5];
					}
					else
					{
						playoffTeams[7] = playoffTeams[6];
					}
				}
				else
				{
					// Player was in the 5v6 match - process their result
					ProcessPagePlayoffMatchResult(playerWon, 2, false);
				}
				
				playoffRound++;
				SetPlayoffs();
				break;

			case 3:
				ProcessPagePlayoffMatchResult(playerWon, 3, false);
				playoffRound++;
				SetPlayoffs();
				break;
		}
	}

    void LoadPlayoffs()
    {
		Debug.Log($"[LoadPlayoffs] Starting - playoffRound={playoffRound}");

		// CRITICAL: This method should ONLY restore saved state, NOT advance rounds
		// Round advancement is handled by LoadAndAdvancePlayoffs() via justFinishedGame flag
        gsp.careerLoad = false;

        Debug.Log($"[LoadPlayoffs] Restoring playoff state for round {playoffRound}");
		Debug.Log("gsp.playerTeam.nextOpp - " + gsp.playerTeam.nextOpp);

		if (gsp.playoffTeams != null && gsp.playoffTeams.Length > 0)
		{
			Debug.Log($"[LoadPlayoffs] Loading {gsp.playoffTeams.Length} teams from saved playoff bracket");
			for (int i = 0; i < gsp.playoffTeams.Length; i++)
			{
				// CRITICAL: Don't overwrite rank! The saved team already has the correct rank
				// Only set rank = i+1 if the team's rank is 0 or invalid
				if (i < 4 && gsp.playoffTeams[i].rank == 0)
				{
					gsp.playoffTeams[i].rank = i + 1;
				}
				playoffTeams[i] = gsp.playoffTeams[i];
				Debug.Log($"[LoadPlayoffs] Position {i}: {playoffTeams[i].name} (rank {playoffTeams[i].rank})");
			}
		}
		else
        {
			List<Team_List> teamList = new List<Team_List>();


			for (int i = 0; i < tm.teams.Length; i++)
            {
				teamList.Add(new Team_List(tm.teams[i]));
            }
			teamList.Sort();
			gsp.playoffTeams = new Team[playoffTeams.Length];
			Debug.Log("gsp.playoffTeams Length is " + gsp.playoffTeams.Length);

			for (int i = 0; i < 4; i++)
			{
				playoffTeams[i] = teamList[i].team;
				gsp.playoffTeams[i] = teamList[i].team;
			}
        }
		//playoffTeams = gsp.playoffTeams;

		for (int i = 0; i < gsp.teams.Length; i++)
		{
			if (gsp.teams[i].player)
				playerTeam = i;
		}
		for (int i = 0; i < tm.teams.Length; i++)
		{
			if (gsp.teams[i].name == gsp.teams[playerTeam].nextOpp)
				oppTeam = i;
		}
		Debug.Log("OppTeam is " + oppTeam);

		for (int i = 0; i < playoffTeams.Length; i++)
		{
			if (playoffRound <= 1 && i < 4)
			{
				brackDisplay[i].name.text = playoffTeams[i].name;
				brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
				heading.text = "Loaded...Page Playoff";
			}
			else if (playoffRound == 2 && i < 7)
			{
				brackDisplay[i].name.text = playoffTeams[i].name;
				brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
				heading.text = "Loaded...Semifinals";
			}
			else if (playoffRound == 3 && i < 8)
			{
				brackDisplay[i].name.text = playoffTeams[i].name;
				brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
				heading.text = "Loaded...Finals";
			}
			else if (playoffRound == 4)
			{
				brackDisplay[i].name.text = playoffTeams[i].name;
				brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
				heading.text = "Loaded...Tourny Over";
			}
			else
			{
				playoffTeams[i] = tm.tTeamList.nullTeam;
			}
		}
		tm.playoffRound = playoffRound;


		SetPlayoffs();
	}

	public void SetPlayoffs()
	{
		if (playoffRound < 1)
		{
			playoffRound = 1;
			gsp.playoffRound = playoffRound;
		}
		Debug.Log("Set Playoffs - Round " + playoffRound);
		switch (playoffRound)
        {
        case 1:
                #region Case 1
                heading.text = "Page Playoff - Round 1";
                
                DisplayPagePlayoffTeams(4, highlightPlayer: true);
                bool playerActive1 = SetupPagePlayoffVsDisplay();
                ConfigurePagePlayoffButtons(playerActive1);
                
                playoffs.SetActive(true);
                StartCoroutine(RefreshPlayoffPanel());
                scrollBar.value = 0;
				gsp.AutoSave();
                break;
            #endregion
            case 2:
                #region Case 2
                heading.text = "Semifinals";
                
                DisplayPagePlayoffTeams(7, highlightPlayer: true);
                bool playerActive2 = SetupPagePlayoffVsDisplay();

				// Special case: BYE handling
				// If player is at position 4 (winner of 1v2), they have a BYE to finals
				// Player has no game to play - show only Sim button (to simulate other games)
				bool hasActualGame = false;

                if (playerActive2)
				{
					hasActualGame = tm.vsDisplay[1].name.text != "BYE TO FINALS";
				}
				else
				{
					hasActualGame = false;
                }

                ConfigurePagePlayoffButtons(hasActualGame);
                
                playoffs.SetActive(true);
                StartCoroutine(RefreshPlayoffPanel());
                scrollBar.value = 0.5f;
				gsp.AutoSave();
				break;
            #endregion
            case 3:
                #region Case 3
                heading.text = "Finals";
                
                DisplayPagePlayoffTeams(8, highlightPlayer: true);
                bool playerActive3 = SetupPagePlayoffVsDisplay();
                ConfigurePagePlayoffButtons(playerActive3);
                
                playoffs.SetActive(true);
                StartCoroutine(RefreshPlayoffPanel());
                scrollBar.value = 1f;
				gsp.AutoSave();
				break;
            #endregion
            case 4:
				#region Case 4
				heading.text = "Tournament Complete";
				
				// Display all teams in bracket
				DisplayPagePlayoffTeams(9, highlightPlayer: false);
				
                playoffs.SetActive(true);
                StartCoroutine(RefreshPlayoffPanel());

				tm.vsTitle.text = "Results";
				tm.vsVS.text = " ";
				tm.vs.SetActive(true);
				
				// Prize distribution for top 4
				float prize1 = gsp.prize * 0.5f;
				float prize2 = gsp.prize * 0.25f;
				float prize3 = gsp.prize * 0.15f;
				float prize4 = gsp.prize * 0.075f;

			// Distribute prizes to all teams in a single loop
			for (int i = 0; i < tm.teamList.Count; i++)
			{
				Team team = tm.teamList[i].team;
				float prize = 0f;
				int rank = 0;
				
			// Determine prize and rank based on playoff position
				if (team.id == playoffTeams[8].id)
				{
					prize = prize1;
					rank = 1;
				}
				else if (team.id == playoffTeams[4].id || team.id == playoffTeams[7].id)
				{
					prize = prize2;
					rank = 2;
				}
				else if (team.id == playoffTeams[5].id || team.id == playoffTeams[6].id)
				{
					prize = prize3;
					rank = 3;
				}
				else if (team.id == playoffTeams[2].id || team.id == playoffTeams[3].id)
				{
					prize = prize4;
					rank = 4;
				}
				else if (i > 3)
				{
					// Use SharedTournamentLogic for remaining prizes
					prize = SharedTournamentLogic.CalculatePrize(i + 1, tm.teamList.Count, gsp.prize);
					rank = i + 1;
				}

				// Update team earnings and rank
				if (rank > 0)
				{
					team.earnings += Mathf.RoundToInt(prize);
					team.rank = rank;

					// Display player's results
					if (team.player)
					{
						gsp.tournyEarnings += Mathf.RoundToInt(prize);

						// Set heading based on rank
						if (rank == 1)
							heading.text = "You Win!";
						else if (rank == 2)
							heading.text = "Runner-up";
						else if (rank == 3)
							heading.text = "3rd Place";
						else
							heading.text = rank + "th Place";

						// Update VS display
						tm.vs.SetActive(true);
						tm.vsDisplay[0].name.text = team.name;
						tm.vsDisplay[0].rank.text = rank.ToString();
						tm.vsDisplay[1].name.text = "$" + Mathf.RoundToInt(prize).ToString("n0");
						tm.vsDisplay[1].rank.gameObject.SetActive(false);
					}
				}
			}
			
			Debug.Log($"GSP Earnings after calculation - ${gsp.tournyEarnings:n0}");
			careerEarningsText.text = "$ " + gsp.tournyEarnings.ToString("n0");

				//gsp.record = new Vector2(gsp.record.x + tm.teams[playerTeam].wins, gsp.record.y + tm.teams[playerTeam].loss);

				gsp.AutoSave();
				//heading.text = "So Close!";
				
				playButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(false);
				simButton.gameObject.SetActive(false);
				nextButton.gameObject.SetActive(true);
				scrollBar.value = 1;

				break;
                #endregion
        }
    }

	public void OnSim()
    {
		StartCoroutine(SimPlayoff(false, false));
    }

    IEnumerator SimPlayoff(bool game1, bool game2)
	{

		Debug.Log("Sim Playoffs - Round " + playoffRound);
		Team game1X;
		Team game1Y;
		Team game2X;
		Team game2Y;

		switch (playoffRound)
		{
			case 1:
				if (!game1)
                {
					game1X = playoffTeams[0];
					game1Y = playoffTeams[1];

					if (Random.Range(0, game1X.strength) > Random.Range(0, game1Y.strength))
					{
						playoffTeams[4] = game1X;
						playoffTeams[5] = game1Y;
					}
					else
					{
						playoffTeams[4] = game1Y;
						playoffTeams[5] = game1X;
					}
				}
				
				if (!game2)
				{
					game2X = playoffTeams[2];
					game2Y = playoffTeams[3];

					if (Random.Range(0, game2X.strength) > Random.Range(0, game2Y.strength))
					{
						playoffTeams[6] = game2X;
					}
					else
					{
						playoffTeams[6] = game2Y;
					}
				}
				
				for (int i = 0; i < 7; i++)
                {
					brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
					brackDisplay[i].name.text = playoffTeams[i].name;
					brackDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[i].SetActive(true);
				}
				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				SetPlayoffs();
				break;

			case 2:
				// In Page Playoff semifinals:
				// - Team at position 4 (winner of 1v2) has already advanced to finals - NO GAME
				// - Teams at positions 5 and 6 play for the other finals spot
				
				game1X = playoffTeams[5];
				game1Y = playoffTeams[6];

				// Simulate the 5v6 match - winner goes to position 7
				if (Random.Range(0, game1X.strength) > Random.Range(0, game1Y.strength))
				{
					playoffTeams[7] = game1X;
				}
				else
				{
					playoffTeams[7] = game1Y;
				}
				
				// Team at position 4 does NOT play - they automatically advance to finals
				// (they will face position 7 in round 3)

				for (int i = 0; i < 8; i++)
				{
					brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
					brackDisplay[i].name.text = playoffTeams[i].name;
					brackDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[i].SetActive(true);
				}
			StartCoroutine(RefreshPlayoffPanel());
			playoffRound++;
			SetPlayoffs();
			break;

		case 3:
				game1X = playoffTeams[4];
				game1Y = playoffTeams[7];

				if (Random.Range(0, game1X.strength) > Random.Range(0, game1Y.strength))
				{
					playoffTeams[8] = game1X;
				}
				else
				{
					playoffTeams[8] = game1Y;
				}

				for (int i = 0; i < 9; i++)
				{
					brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
					brackDisplay[i].name.text = playoffTeams[i].name;
					brackDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[i].SetActive(true);
				}

			StartCoroutine(RefreshPlayoffPanel());
			playoffRound++;
			SetPlayoffs();
			break;

		default:
				SetPlayoffs();
				break;

		}
		yield break;
	}

	IEnumerator LoadCareer()
	{
		gsp.LoadCareer();

		yield return careerEarningsText.text = "$ " + gsp.tournyEarnings.ToString();
	}

	IEnumerator SaveCareer(bool inProgress)
	{
		Debug.Log("Saving in PlayoffManager, inProgress is " + inProgress);

		myFile = new EasyFileSave("my_player_data");

		//myFile.Add("Career Record", gsp.record);
		Debug.Log("gsp.record is " + tm.teams[tm.playerTeam].wins + " - " + tm.teams[tm.playerTeam].loss);
		myFile.Add("BG", gsp.bg);
		//Vector2 tempRecord = new Vector2(gsp.record.x, gsp.record.y);
		//myFile.Add("Player Name", gsp.firstName);
		//myFile.Add("Team Name", gsp.teamName);
		//myFile.Add("Team Colour", gsp.teamColour);
		//myFile.Add("Career Earnings", gsp.earnings);

		//if (!inProgress)
		//      {
		//	tm.weight = 0;
		//	playoffRound = 0;
		//	tm.playoffRound = 0;

		//      }
		myFile.Add("Tourny In Progress", inProgress);
		gsp.tournyInProgress = inProgress;
		Debug.Log("gsp.inProgress is " + gsp.tournyInProgress);
		//myFile.Add("Draw", gsp.weight);
		myFile.Add("Number Of Teams", gsp.numberOfTeams);
		//myFile.Add("Player Team", gsp.playerTeamIndex);
		myFile.Add("OppTeam", oppTeam);
		myFile.Add("Playoff Round", playoffRound);

		string[] nameList = new string[tm.teams.Length];
		int[] winsList = new int[tm.teams.Length];
		int[] lossList = new int[tm.teams.Length];
		int[] rankList = new int[tm.teams.Length];
		string[] nextOppList = new string[tm.teams.Length];
		int[] strengthList = new int[tm.teams.Length];
		int[] idList = new int[tm.teams.Length];
		float[] earningsList = new float[tm.teams.Length];

		for (int i = 0; i < tm.teams.Length; i++)
		{
			nameList[i] = tm.teams[i].name;
			winsList[i] = tm.teams[i].wins;
			lossList[i] = tm.teams[i].loss;
			rankList[i] = tm.teams[i].rank;
			nextOppList[i] = tm.teams[i].nextOpp;
			strengthList[i] = tm.teams[i].strength;
			idList[i] = tm.teams[i].id;
			earningsList[i] = tm.teams[i].earnings;
		}

		myFile.Add("Tourny Name List", nameList);
		myFile.Add("Tourny Wins List", winsList);
		myFile.Add("Tourny Loss List", lossList);
		myFile.Add("Tourny Rank List", rankList);
		myFile.Add("Tourny NextOpp List", nextOppList);
		myFile.Add("Tourny Strength List", strengthList);
		myFile.Add("Tourny Team ID List", idList);
		myFile.Add("Tourny Earnings List", earningsList);

		int[] playoffIDList = new int[playoffTeams.Length];
		int[] playoffRankList = new int[playoffTeams.Length];

		for (int i = 0; i < playoffTeams.Length; i++)
		{
			//Debug.Log("playoffID i is " + i);
			playoffIDList[i] = playoffTeams[i].id;
			playoffRankList[i] = playoffTeams[i].rank;
			//Debug.Log("Playoff ID List - " + playoffIDList[i]);
		}

		myFile.Add("Playoff ID List", playoffIDList);
		myFile.Add("Playoff Rank List", playoffRankList);
		//yield return myFile.TestDataSaveLoad();
		yield return myFile.Append();
	}
}