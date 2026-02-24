using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TigerForge;

public class PlayoffManager_SingleK : MonoBehaviour
{
	public TournyManager tm;
	public Team[] playoffTeams;

	public BracketDisplay[] roundOf16Display;
	public BracketDisplay[] quartersDisplay;
	public BracketDisplay[] semisDisplay;
	public BracketDisplay[] finalsDisplay;
	public BracketDisplay winnerDisplay;

	public GameObject[] row;
	public GameObject playoffs;
	public Button nextButton;
	public Button simButton;
	public Button contButton;
	public Button playButton;
	public Text heading;
	public Scrollbar scrollBar;
	public Text careerEarningsText;

	public Color yellow;
	public Color dark;
	public Color lighter;

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
		playoffTeams = new Team[31];

		Debug.Log($"[SingleK.Start] playoffRound={playoffRound}, careerLoad={gsp.careerLoad}, gameInProgress={gsp.gameInProgress}, justFinishedGame={gsp.justFinishedGame}");

		// CRITICAL FIX: Handle five distinct scenarios:
		// 1. Returning from a completed game (justFinishedGame == true, need to advance)
		// 2. Fresh tournament start (playoffRound == 0 OR teams not yet seeded)
		// 3. Loading saved tournament between games (careerLoad == true, !justFinishedGame)
		// 4. Normal round display (playoffRound > 0, teams seeded, just viewing bracket)

		// CRITICAL FIX: Check if playoff teams are seeded
		bool teamsNotSeeded = (gsp.playoffTeams == null || gsp.playoffTeams.Length == 0 || gsp.playoffTeams[0] == null);
		
	// CRITICAL FIX: If loading from save AND returning from game, check if bracket exists
	// This handles the case where we just completed a game but bracket isn't in memory yet
	if (gsp.careerLoad && gsp.justFinishedGame && playoffRound > 0 && teamsNotSeeded)
	{
		Debug.LogWarning($"[SingleK.Start] SPECIAL: Bracket not in memory after save load (Round {playoffRound})");
		Debug.LogWarning($"[SingleK.Start] Cannot advance without bracket data - will display fresh bracket");
		// CRITICAL: We can't advance playoffs without the previous bracket state
		// The game result is stored in team wins/losses, but we don't know bracket positions
		// Best we can do is re-seed and let player continue from current state
		gsp.justFinishedGame = false;  // Clear flag - can't process without data
		StartCoroutine(SetSeeding(tm.teams.Length));
		return;
	}
		
		// If returning from a game but teams aren't seeded yet, we need to seed FIRST
		// This happens in debug mode where we jump straight to a game without initial seeding
		if (teamsNotSeeded && gsp.justFinishedGame && playoffRound > 0)
		{
			Debug.LogWarning($"[SingleK.Start] SPECIAL CASE: Returning from game but bracket not seeded - will seed then show results");
			// Seed the bracket first - this will populate the initial bracket
			StartCoroutine(SetSeeding(tm.teams.Length));
			// Note: We DON'T process the game result here - player will need to view bracket
			// then manually advance. This is a rare edge case (debug mode only)
			gsp.justFinishedGame = false; // Clear flag
		}
		// Check playoffRound > 0 before advancing - if 0, this is a FRESH tournament
		else if (gsp.justFinishedGame && playoffRound > 0 && !teamsNotSeeded)
		{
			// Scenario 1: Just returned from completing a game - need to process result and advance
			Debug.Log("[SingleK.Start] SCENARIO 1: Returning from completed game - advancing playoffs");
			LoadAndAdvancePlayoffs();
			gsp.justFinishedGame = false; // Clear flag after processing
		}
		else if (playoffRound == 0 || teamsNotSeeded)
		{
			// Scenario 2: Fresh tournament start OR teams not yet seeded
			Debug.Log("[SingleK.Start] SCENARIO 2: Fresh tournament start - setting seeding");
			gsp.justFinishedGame = false; // Clear stale flag if present
			StartCoroutine(SetSeeding(tm.teams.Length));
		}
		else if (gsp.careerLoad)
		{
			// Scenario 3: Loading saved tournament, player is between games - just restore state
			Debug.Log("[SingleK.Start] SCENARIO 3: Loading saved tournament (between games) - restoring state");
			LoadPlayoffs();
		}
		else if (playoffTeams != null && playoffTeams.Length > 0 && playoffTeams[0] != null)
		{
			// Scenario 4: Normal viewing - teams are seeded, just display current round
			Debug.Log($"[SingleK.Start] SCENARIO 4: Viewing round {playoffRound} - displaying bracket");
			
			// Load teams from gsp first if not already loaded
			if (playoffTeams[0] == tm.tTeamList.nullTeam || playoffTeams[0].name == "")
			{
				for (int i = 0; i < playoffTeams.Length; i++)
					playoffTeams[i] = gsp.playoffTeams[i];
			}
			
			StartCoroutine(SetPlayoffs());
		}
		else
		{
			// Fallback: Teams not ready - initialize them
			Debug.LogWarning("[SingleK.Start] FALLBACK: Teams not ready - initializing");
			StartCoroutine(SetSeeding(tm.teams.Length));
		}
	}

    IEnumerator SetSeeding(int numberOfTeams)
    {
        playoffTeams = new Team[31];
        heading.text = "Single Elimination";
        playoffRound++;

        // Wait until TournyManager has completed its SetupStandings coroutine
        yield return new WaitUntil(() => tm != null && tm.isStandingsReady);

        for (int i = 0; i < playoffTeams.Length; i++)
        {
            if (i < tm.teams.Length)
            {
                tm.teams[i].rank = 0;
                playoffTeams[i] = tm.teams[i];
                if (i < roundOf16Display.Length)
                {
                    roundOf16Display[i].name.text = playoffTeams[i].name;
                    roundOf16Display[i].rank.text = playoffTeams[i].rank.ToString();
                }
            }
            else
            {
                playoffTeams[i] = tm.tTeamList.nullTeam;
            }
        }

        tm.playoffRound = playoffRound;
        gsp.playoffTeams = playoffTeams;

        StartCoroutine(SetPlayoffs());
    }

    IEnumerator RefreshPlayoffPanel()
	{
		for (int i = 0; i < roundOf16Display.Length; i++)
		{
			roundOf16Display[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = false;

			yield return new WaitForEndOfFrame();
			roundOf16Display[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = true;
		}

		for (int i = 0; i < quartersDisplay.Length; i++)
		{
			quartersDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = false;

			yield return new WaitForEndOfFrame();
			quartersDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = true;
		}

		for (int i = 0; i < semisDisplay.Length; i++)
		{
			semisDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = false;

			yield return new WaitForEndOfFrame();
			semisDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = true;
		}

		for (int i = 0; i < finalsDisplay.Length; i++)
		{
			finalsDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = false;

			yield return new WaitForEndOfFrame();
			finalsDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = true;
		}

		winnerDisplay.name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = false;

		yield return new WaitForEndOfFrame();
		winnerDisplay.name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = true;

		//for (int i = 0; i < tm.vsDisplay.Length; i++)
		//{
		//	tm.vsDisplay[i].name.gameObject.GetComponent<ContentSizeFitter>().enabled = false;

		//	yield return new WaitForEndOfFrame();
		//	tm.vsDisplay[i].name.gameObject.GetComponent<ContentSizeFitter>().enabled = true;
		//}
	}

	#region Phase 2: Helper Methods
	
	// Helper variables for playoff logic
	int roundLength;
	int playerGame;
	
	/// <summary>
	/// Gets the opponent team index for the player in the current playoff round
	/// Uses standard single elimination pairing (0v1, 2v3, etc.)
	/// </summary>
	int GetPlayerOpponentIndex()
	{
		int[] config = SharedTournamentLogic.GetSingleEliminationRoundConfig(playoffRound);
		int startIndex = config[0];
		int matchCount = config[1];
		
		// Find opponent in bracket pairs
		for (int i = 0; i < matchCount * 2; i += 2)
		{
			int idx1 = startIndex + i;
			int idx2 = startIndex + i + 1;
			
			if (idx1 >= playoffTeams.Length || idx2 >= playoffTeams.Length)
				break;
				
			if (playoffTeams[idx1].player)
				return idx2;
			if (playoffTeams[idx2].player)
				return idx1;
		}

        return -1; // Player not found
    }

    /// <summary>
    /// Simulates matches for a given round (skips player's game if already processed)
    /// </summary>
    void SimulateRoundMatches(BracketDisplay[] currentDisplay, int startIndex, int nextRoundStart, int playerGame, int eliminationRank)
    {
        for (int i = 0; i < currentDisplay.Length; i++)
        {
            if (i % 2 == 0)
            {
                int game = i / 2;
                Team gameX = playoffTeams[startIndex + i];
                Team gameY = playoffTeams[startIndex + i + 1];
                
                // Check for null teams (safety check)
                if (gameX == null || gameY == null)
                {
                    Debug.LogWarning($"[SimulateRoundMatches] Null team at index {startIndex + i} or {startIndex + i + 1}");
                    continue;
                }

                if (game != playerGame)
                {
                    if (Random.Range(0, gameX.strength) > Random.Range(0, gameY.strength))
                    {
                        playoffTeams[nextRoundStart + game] = gameX;
                        
                        // Only increment wins/losses if this is NOT returning from a player game (playerGame != 99)
                        // When playerGame == 99, it means we're simulating all games (OnSim button)
                        // When playerGame is a specific game number, we're returning from player's game
                        // and wins/losses were already updated in LoadAndAdvancePlayoffs
                        if (playerGame == 99)
                        {
                            gameX.wins++;
                            gameY.loss++;
                        }
                        
                        gameX.rank = 0;
                        gameY.rank = eliminationRank;
                    }
                    else
                    {
                        playoffTeams[nextRoundStart + game] = gameY;
                        
                        // Only increment wins/losses when simulating (playerGame == 99)
                        if (playerGame == 99)
                        {
                            gameX.loss++;
                            gameY.wins++;
                        }
                        
                        gameX.rank = eliminationRank;
                        gameY.rank = 0;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Updates a bracket display with team info and colors
    /// </summary>
    void UpdateBracketDisplay(BracketDisplay[] display, int teamStartIndex, int rowStartIndex, int eliminationRank)
    {
        for (int i = 0; i < display.Length; i++)
        {
            Team team = playoffTeams[teamStartIndex + i];

            if (team.rank == eliminationRank)
            {
                display[i].rank.text = eliminationRank == 9 ? "KO" :
                                       eliminationRank == 5 ? "KO" :
                                       eliminationRank == 3 ? "3rd" :
                                       eliminationRank == 2 ? "2nd" : "KO";
                display[i].bg.GetComponent<Image>().color = dark;
            }
            else
            {
                display[i].rank.text = team.rank.ToString();
                display[i].bg.GetComponent<Image>().color = yellow;
            }

            display[i].name.text = team.name;
            display[i].name.transform.parent.gameObject.SetActive(true);
            row[rowStartIndex + i].SetActive(true);
        }
    }

    /// <summary>
    /// Shows the next round's winners
    /// </summary>
    void ShowNextRoundWinners(BracketDisplay[] display, int teamStartIndex, int rowStartIndex)
    {
        for (int i = 0; i < display.Length; i++)
        {
            display[i].rank.text = playoffTeams[teamStartIndex + i].rank.ToString();
            display[i].name.text = playoffTeams[teamStartIndex + i].name;
            display[i].name.transform.parent.gameObject.SetActive(true);
            row[rowStartIndex + i].SetActive(true);
        }
    }

    /// <summary>
    /// Hides a bracket display and its rows
    /// </summary>
    void HideBracketDisplay(BracketDisplay[] display, int rowStartIndex)
    {
        for (int i = 0; i < display.Length; i++)
        {
            display[i].name.transform.parent.gameObject.SetActive(false);
            row[rowStartIndex + i].SetActive(false);
        }
    }

    /// <summary>
    /// Calculates which game number the player is in based on their bracket position
    /// </summary>
    int CalculatePlayerGameNumber(int playerTeamIndex, int startIndex)
    {
        int positionInRound = playerTeamIndex - startIndex;
        return positionInRound / 2;
    }

    /// <summary>
    /// Updates bracket based on player's match result
    /// </summary>
    void ProcessPlayerMatchResult(bool playerWon, int nextRoundStart, int eliminationRank)
    {
        if (playerWon)
        {
            Debug.Log($"Player won! Advancing to next round at index {nextRoundStart + playerGame}");
            
            // Copy player's team to next round
            playoffTeams[nextRoundStart + playerGame] = playoffTeams[playerTeam];
            
            // CRITICAL FIX: Transfer player flag to new position
            playoffTeams[nextRoundStart + playerGame].player = true;
            playoffTeams[playerTeam].player = false;  // Clear from old position
            
            // Update stats
            playoffTeams[nextRoundStart + playerGame].wins++;
            playoffTeams[oppTeam].loss++;
            playoffTeams[oppTeam].rank = eliminationRank;
        }
        else
        {
            Debug.Log($"Player lost. Opponent advancing to next round at index {nextRoundStart + playerGame}");
            
            // Copy opponent's team to next round
            playoffTeams[nextRoundStart + playerGame] = playoffTeams[oppTeam];
            
            // Player flag stays with player team (at their old position - they're eliminated)
            // Opponent doesn't get player flag
            
            // Update stats
            playoffTeams[oppTeam].wins++;
            playoffTeams[playerTeam].loss++;
            playoffTeams[playerTeam].rank = eliminationRank;
        }
    }

    /// <summary>
    /// Displays teams for a given round with player highlighting
    /// </summary>
    void DisplayRoundTeams(BracketDisplay[] display, int startIndex, bool highlightPlayer = false)
    {
        for (int i = 0; i < display.Length; i++)
        {
            display[i].rank.text = playoffTeams[startIndex + i].rank.ToString();
            display[i].name.text = playoffTeams[startIndex + i].name;
            row[startIndex + i].SetActive(true);

            if (playoffTeams[startIndex + i].player && highlightPlayer)
            {
                display[i].panel.GetComponent<Image>().color = yellow;
                display[i].name.transform.parent.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Sets up VS display showing player and their opponent
    /// </summary>
    /// <returns>True if player is still in tournament, false if knocked out</returns>
    bool SetupVsDisplay(int roundStartIndex, int roundLength)
    {
        for (int i = 0; i < roundLength; i++)
        {
            if (playoffTeams[roundStartIndex + i].player)
            {
                tm.vsDisplay[0].name.text = playoffTeams[roundStartIndex + i].name;
                tm.vsDisplay[0].rank.text = playoffTeams[roundStartIndex + i].rank.ToString();

                // Find opponent
                if (i % 2 == 0)
                {
                    tm.vsDisplay[1].name.text = playoffTeams[roundStartIndex + i + 1].name;
                    tm.vsDisplay[1].rank.text = playoffTeams[roundStartIndex + i + 1].rank.ToString();
                    playoffTeams[roundStartIndex + i].nextOpp = playoffTeams[roundStartIndex + i + 1].name;
                }
                else
                {
                    tm.vsDisplay[1].name.text = playoffTeams[roundStartIndex + i - 1].name;
                    tm.vsDisplay[1].rank.text = playoffTeams[roundStartIndex + i - 1].rank.ToString();
                    playoffTeams[roundStartIndex + i].nextOpp = playoffTeams[roundStartIndex + i - 1].name;
                }
                return true; // Player found and active
            }
        }
        return false; // Player not in this round (knocked out)
    }

    /// <summary>
    /// Shows "Knocked Out" message in VS display
    /// </summary>
    void ShowKnockedOutDisplay()
    {
        for (int i = 0; i < playoffTeams.Length; i++)
        {
            if (playoffTeams[i].player)
            {
                tm.vsDisplay[0].name.text = playoffTeams[i].name;
                tm.vsDisplay[0].rank.text = playoffTeams[i].rank.ToString();
            }
        }
        tm.vsDisplay[1].name.text = "Knocked Out!";
        tm.vsDisplay[1].rank.text = playoffRound == 4 ? "X" : " ";
    }

    /// <summary>
    /// Configures UI buttons based on whether player is still active
    /// </summary>
    void ConfigurePlayoffButtons(bool playerActive)
    {
        playButton.gameObject.SetActive(playerActive);
        simButton.gameObject.SetActive(true);
        contButton.gameObject.SetActive(false);
    }

    #endregion

    #region Phase 2: Refactored LoadAndAdvancePlayoffs

    void LoadAndAdvancePlayoffs()
    {
        Debug.Log($"[SingleK] Loading playoffs - Round {playoffRound}");

        // Load saved teams
        for (int i = 0; i < playoffTeams.Length; i++)
            playoffTeams[i] = gsp.playoffTeams[i];

        // Get round config
        int[] config = SharedTournamentLogic.GetSingleEliminationRoundConfig(playoffRound);
        int startIdx = config[0];
        int matchCount = config[1];
        int nextRoundStart = config[2];
        int elimRank = SharedTournamentLogic.GetSingleEliminationRank(playoffRound);

        Debug.Log($"[SingleK] Round {playoffRound}: start={startIdx}, matches={matchCount}, next={nextRoundStart}");

        // Find player in current round
        playerTeam = -1;
        for (int i = 0; i < matchCount * 2; i++)
        {
            int idx = startIdx + i;
            if (playoffTeams[idx] != null && playoffTeams[idx].player)
            {
                playerTeam = idx;
                oppTeam = (i % 2 == 0) ? idx + 1 : idx - 1;
                playerGame = i / 2;
                break;
            }
        }

        if (playerTeam == -1)
        {
            Debug.LogError($"[SingleK] Player not found in round {playoffRound}!");
            return;
        }

        Debug.Log($"[SingleK] Player at {playerTeam} vs {oppTeam}, game {playerGame}");

        // Determine winner and advance
        bool playerWon = SharedTournamentLogic.DeterminePlayerWon(gsp);
        int winnerIdx = playerWon ? playerTeam : oppTeam;
        int loserIdx = playerWon ? oppTeam : playerTeam;
        int nextIdx = nextRoundStart + playerGame;

        Debug.Log($"[SingleK] Player {(playerWon ? "WON" : "LOST")} - advancing {playoffTeams[winnerIdx].name}");

        // CRITICAL FIX: Set player flag BEFORE copying, since the copy creates a reference
        // If player won, the Team object needs player=true
        if (playerWon)
        {
            playoffTeams[winnerIdx].player = true;  // Ensure winner team has flag
            Debug.Log($"[SingleK] Set player flag on winner team: {playoffTeams[winnerIdx].name}");
        }
        
        // Update wins/losses
        playoffTeams[winnerIdx].wins++;
        playoffTeams[loserIdx].loss++;
        playoffTeams[loserIdx].rank = elimRank;

        // Copy winner to next round (creates a reference to the same Team object)
        playoffTeams[nextIdx] = playoffTeams[winnerIdx];
        
        // Clear player flag from loser's team if player lost
        if (!playerWon)
        {
            playoffTeams[playerTeam].player = false;
        }

        Debug.Log($"[SingleK] Advanced {playoffTeams[nextIdx].name} to index {nextIdx} with {playoffTeams[nextIdx].wins} wins (player={playoffTeams[nextIdx].player})");

        // CRITICAL: Save to gsp BEFORE simulating
        gsp.playoffTeams = playoffTeams;
        
        Debug.Log($"[SingleK] Calling SimPlayoff to finish Round {playoffRound} (player game {playerGame} already played)");
        
        // Continue simulation - this will simulate OTHER games in the CURRENT round
        // IMPORTANT: Don't increment playoffRound yet - SimPlayoff will do it!
        StartCoroutine(SimPlayoff(playerGame));
    }

    #endregion

    void LoadPlayoffs()
    {
        gsp.careerLoad = false;
        Debug.Log($"[LoadPlayoffs] Loading saved playoffs - Round {playoffRound}");
        Debug.Log($"gsp.playerTeam.nextOpp: {gsp.playerTeam.nextOpp}");

        // Load teams from persistent storage or initialize from TournyManager
        if (gsp.playoffTeams != null && gsp.playoffTeams.Length > 0)
        {
            for (int i = 0; i < gsp.playoffTeams.Length; i++)
            {
                playoffTeams[i] = gsp.playoffTeams[i];
            }
        }
        else
        {
            List<Team_List> teamList = new List<Team_List>();
            for (int i = 0; i < tm.teams.Length; i++)
            {
                teamList.Add(new Team_List(tm.teams[i]));
            }

            gsp.playoffTeams = new Team[playoffTeams.Length];
            Debug.Log($"gsp.playoffTeams Length is {gsp.playoffTeams.Length}");

            for (int i = 0; i < playoffTeams.Length; i++)
            {
                // CRITICAL FIX: Check if team slot is initialized before accessing it
                if (playoffTeams[i] == null)
                {
                    Debug.LogWarning($"[LoadPlayoffs] playoffTeams[{i}] is null - skipping");
                    gsp.playoffTeams[i] = tm.tTeamList.nullTeam;  // Use null team placeholder
                    continue;
                }
                
                for (int j = 0; j < teamList.Count; j++)
                {
                    if (playoffTeams[i].id == teamList[j].team.id)
                    {
                        playoffTeams[i] = teamList[j].team;
                    }
                }
                gsp.playoffTeams[i] = playoffTeams[i];
            }
        }

        Debug.Log($"OppTeam is {oppTeam}");

        // Handle tournament completion (Round 5)
        if (playoffRound == 5)
        {
            // Display all rounds
            DisplayRoundTeams(roundOf16Display, 0);
            DisplayRoundTeams(quartersDisplay, roundOf16Display.Length);
            DisplayRoundTeams(semisDisplay, roundOf16Display.Length + quartersDisplay.Length);
            DisplayRoundTeams(finalsDisplay, roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length);

            // Display winner (index 30 is the final winner position)
            winnerDisplay.name.text = playoffTeams[30].name;
            winnerDisplay.rank.text = playoffTeams[30].rank.ToString();
            winnerDisplay.name.transform.parent.gameObject.SetActive(true);
            row[30].SetActive(true);

            heading.text = "Loaded...Tourny Over";
            tm.playoffRound = playoffRound;
            StartCoroutine(SetPlayoffs());
            return;
        }

        // Handle active playoff rounds (1-4)
        string[] loadedRoundNames = { "", "Loaded...Round of 16", "Loaded...Quarterfinals", "Loaded...Semifinals", "Loaded...Finals" };
        heading.text = loadedRoundNames[playoffRound];

        // Display Round of 16 (always visible in rounds 1-4)
        DisplayRoundTeams(roundOf16Display, 0, highlightPlayer: true);

        // Display Quarterfinals (visible in rounds 2-4)
        if (playoffRound >= 2)
        {
            DisplayRoundTeams(quartersDisplay, roundOf16Display.Length, highlightPlayer: true);
        }
        else
        {
            HideBracketDisplay(quartersDisplay, roundOf16Display.Length);
        }

        // Display Semifinals (visible in rounds 3-4)
        if (playoffRound >= 3)
        {
            DisplayRoundTeams(semisDisplay, roundOf16Display.Length + quartersDisplay.Length, highlightPlayer: true);
        }
        else
        {
            HideBracketDisplay(semisDisplay, roundOf16Display.Length + quartersDisplay.Length);
        }

        // Display Finals (visible only in round 4)
        if (playoffRound >= 4)
        {
            int finalsOffset = roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length;
            for (int i = 0; i < finalsDisplay.Length; i++)
            {
                finalsDisplay[i].name.text = playoffTeams[finalsOffset + i].name;
                finalsDisplay[i].rank.text = playoffTeams[finalsOffset + i].rank.ToString();
                finalsDisplay[i].name.transform.parent.gameObject.SetActive(false); // Hidden until SetPlayoffs
                row[finalsOffset + i].SetActive(true);

                if (playoffTeams[finalsOffset + i].player)
                    finalsDisplay[i].bg.GetComponent<Image>().color = yellow;
            }
        }
        else
        {
            HideBracketDisplay(finalsDisplay, roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length);
        }

        // Hide winner display (not visible until round 5)
        winnerDisplay.name.transform.parent.gameObject.SetActive(false);
        row[30].SetActive(false);

        tm.playoffRound = playoffRound;
        StartCoroutine(SetPlayoffs());
    }

    IEnumerator SetPlayoffs()
    {
        if (playoffRound < 1)
        {
            playoffRound = 1;
            gsp.playoffRound = playoffRound;
        }
        Debug.Log("Set Playoffs - Round " + playoffRound);

        // Handle tournament completion (Case 5)
        if (playoffRound == 5)
        {
            heading.text = "Finals";
            winnerDisplay.name.text = playoffTeams[30].name;
            winnerDisplay.rank.text = playoffTeams[30].rank.ToString();
            winnerDisplay.bg.GetComponent<Image>().color = yellow;
            row[30].SetActive(true);

            playoffs.SetActive(true);

            tm.vsTitle.text = "Results";
            tm.vsVS.text = " ";
            tm.vs.SetActive(true);

            // Distribute prizes
            for (int i = 24; i < playoffTeams.Length; i++)
            {
                if (i == 30)
                {
                    playoffTeams[i].earnings = gsp.prize * 0.5f;
                    playoffTeams[i].rank = 1;

                    if (playoffTeams[i].player)
                    {
                        heading.text = "You Win!";
                        gsp.tournyEarnings += gsp.prize * 0.5f;
                        tm.vs.SetActive(true);
                        tm.vsDisplay[1].name.text = "$" + (gsp.prize * 0.5f).ToString("n0");
                        tm.vsDisplay[0].name.text = playoffTeams[30].name;
                        tm.vsDisplay[0].rank.text = playoffTeams[30].rank.ToString() + "st";
                        tm.vsDisplay[1].rank.gameObject.SetActive(false);
                    }
                }
                else if (i == 28 || i == 29)
                {
                    if (playoffTeams[i].id != playoffTeams[30].id)
                    {
                        playoffTeams[i].earnings = gsp.prize * 0.25f;
                        playoffTeams[i].rank = 2;

                        if (playoffTeams[i].player)
                        {
                            heading.text = "Runner-up";
                            gsp.tournyEarnings += gsp.prize * 0.25f;
                            tm.vs.SetActive(true);
                            tm.vsDisplay[1].name.text = "$" + (gsp.prize * 0.25f).ToString("n0");
                            tm.vsDisplay[0].name.text = playoffTeams[i].name;
                            tm.vsDisplay[0].rank.text = playoffTeams[i].rank.ToString() + "st";
                            tm.vsDisplay[1].rank.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    if (playoffTeams[i].id != playoffTeams[28].id && playoffTeams[i].id != playoffTeams[29].id)
                    {
                        playoffTeams[i].earnings = gsp.prize * 0.125f;
                        playoffTeams[i].rank = 3;

                        if (playoffTeams[i].player)
                        {
                            heading.text = "3rd Place";
                            gsp.tournyEarnings += gsp.prize * 0.125f;
                            tm.vs.SetActive(true);
                            tm.vsDisplay[1].name.text = "$" + (gsp.prize * 0.125f).ToString("n0");
                            tm.vsDisplay[0].name.text = playoffTeams[i].name;
                            tm.vsDisplay[0].rank.text = playoffTeams[i].rank.ToString() + "st";
                            tm.vsDisplay[1].rank.gameObject.SetActive(false);
                        }
                    }
                }
            }

            Debug.Log("GSP Earnings after calculation - " + gsp.tournyEarnings.ToString());
            careerEarningsText.text = "$ " + gsp.tournyEarnings.ToString("n0");

            playButton.gameObject.SetActive(false);
            contButton.gameObject.SetActive(false);
            simButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(true);
            scrollBar.value = 1;

            yield return new WaitUntil(() => tm != null && tm.isStandingsReady);
            cm.SaveCareer();
            yield break;
        }

        // Handle active playoff rounds (1-4)
        string[] roundNames = { "", "Round of 16", "Quarterfinals", "Semifinals", "Finals" };
        float[] scrollPositions = { 0f, 0f, 0.25f, 0.5f, 0.75f };

        heading.text = roundNames[playoffRound];

        // Get round configuration
        int[] config = SharedTournamentLogic.GetSingleEliminationRoundConfig(playoffRound);
        int startIndex = config[0];

        // Display teams for current round
        BracketDisplay[] currentDisplay;
        if (playoffRound == 1)
            currentDisplay = roundOf16Display;
        else if (playoffRound == 2)
            currentDisplay = quartersDisplay;
        else if (playoffRound == 3)
            currentDisplay = semisDisplay;
        else if (playoffRound == 4)
            currentDisplay = finalsDisplay;
        else
        {
            Debug.LogError($"[SetPlayoffs] Invalid playoffRound: {playoffRound}");
            yield break;
        }

        DisplayRoundTeams(currentDisplay, startIndex, highlightPlayer: true);

        // Hide future rounds
        if (playoffRound < 2)
        {
            HideBracketDisplay(quartersDisplay, roundOf16Display.Length);
            HideBracketDisplay(semisDisplay, roundOf16Display.Length + quartersDisplay.Length);
            HideBracketDisplay(finalsDisplay, roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length);
            winnerDisplay.name.transform.parent.gameObject.SetActive(false);
            row[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + finalsDisplay.Length].SetActive(false);
        }
        else if (playoffRound < 3)
        {
            HideBracketDisplay(semisDisplay, roundOf16Display.Length + quartersDisplay.Length);
            HideBracketDisplay(finalsDisplay, roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length);
            winnerDisplay.name.transform.parent.gameObject.SetActive(false);
            row[30].SetActive(false);
        }
        else if (playoffRound < 4)
        {
            HideBracketDisplay(finalsDisplay, roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length);
            winnerDisplay.name.transform.parent.gameObject.SetActive(false);
            row[30].SetActive(false);
        }

        // Setup VS display and check if player is still active
        bool playerActive = SetupVsDisplay(startIndex, currentDisplay.Length);

        if (!playerActive && playoffRound > 1)
        {
            ShowKnockedOutDisplay();
        }

        // Configure UI buttons
        ConfigurePlayoffButtons(playerActive);

        // Special case for Round 1: sync teams with TournyManager
        if (playoffRound == 1)
        {
            for (int i = 0; i < tm.teams.Length; i++)
            {
                tm.teams[i] = playoffTeams[i];
            }
        }

        // Set scrollbar position
        scrollBar.value = scrollPositions[playoffRound];

        playoffs.SetActive(true);

        yield return new WaitUntil(() => tm != null && tm.isStandingsReady);
        cm.SaveCareer();
    }

    public void OnSim()
    {
        StartCoroutine(SimPlayoff(99));
    }

	IEnumerator SimPlayoff(int playerGame)
	{
		Debug.Log("Sim Playoffs - Round " + playoffRound);

		// Get round configuration using SharedTournamentLogic
		int[] config = SharedTournamentLogic.GetSingleEliminationRoundConfig(playoffRound);
		if (config[1] == 0)
		{
			Debug.Log("Bonk! Need another round");
			StartCoroutine(SetPlayoffs());
			yield break;
		}

		int startIndex = config[0];
		int nextRoundStart = config[2];
		int eliminationRank = SharedTournamentLogic.GetSingleEliminationRank(playoffRound);

		// Get current round display
		BracketDisplay[] currentDisplay = playoffRound == 1 ? roundOf16Display :
										  playoffRound == 2 ? quartersDisplay :
										  playoffRound == 3 ? semisDisplay :
										  finalsDisplay;

		// Simulate matches for this round (skipping player's game)
		Debug.Log($"[SimPlayoff] About to simulate Round {playoffRound}: playerGame={playerGame}, startIndex={startIndex}, nextRoundStart={nextRoundStart}");
		SimulateRoundMatches(currentDisplay, startIndex, nextRoundStart, playerGame, eliminationRank);
		
		// Log what got populated
		for (int i = 0; i < 8; i++)
		{
			int idx = nextRoundStart + i;
			if (idx < playoffTeams.Length && playoffTeams[idx] != null)
			{
				Debug.Log($"[SimPlayoff] After simulation - Round {playoffRound + 1} slot {idx}: {playoffTeams[idx].name} (player={playoffTeams[idx].player})");
			}
			else
			{
				Debug.LogWarning($"[SimPlayoff] After simulation - Round {playoffRound + 1} slot {idx}: NULL or empty!");
			}
		}

		// Update all bracket displays up to current round
		UpdateBracketDisplay(roundOf16Display, 0, 0, 9);

		if (playoffRound >= 2)
		{
			UpdateBracketDisplay(quartersDisplay, roundOf16Display.Length, roundOf16Display.Length, 5);
		}

		if (playoffRound >= 3)
		{
			UpdateBracketDisplay(semisDisplay, roundOf16Display.Length + quartersDisplay.Length,
								roundOf16Display.Length + quartersDisplay.Length, 3);
		}

		if (playoffRound >= 4)
		{
			// Update finals display
			UpdateBracketDisplay(finalsDisplay, roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length,
								roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length, 2);

			// Handle finals special case - simulate the championship match if needed
			if (playerGame == 99)
			{
				Team gameX = playoffTeams[28];
				Team gameY = playoffTeams[29];

				if (Random.Range(0, gameX.strength) > Random.Range(0, gameY.strength))
				{
					playoffTeams[30] = gameX;
					gameX.wins++;
					gameX.rank = 1;
					gameY.loss++;
					gameY.rank = 2;
				}
				else
				{
					playoffTeams[30] = gameY;
					gameX.loss++;
					gameX.rank = 2;
					gameY.wins++;
					gameY.rank = 1;
				}
			}

			// Show winner
			winnerDisplay.rank.text = "1st";
			winnerDisplay.name.text = playoffTeams[30].name;
			winnerDisplay.name.transform.parent.gameObject.SetActive(true);
			row[30].SetActive(true);
		}

		// Show next round winners
		if (playoffRound == 1)
		{
			ShowNextRoundWinners(quartersDisplay, roundOf16Display.Length, roundOf16Display.Length);
		}
		else if (playoffRound == 2)
		{
			ShowNextRoundWinners(semisDisplay, roundOf16Display.Length + quartersDisplay.Length,
								roundOf16Display.Length + quartersDisplay.Length);
		}
		else if (playoffRound == 3)
		{
			ShowNextRoundWinners(finalsDisplay, roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length,
								roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length);
		}

		// Hide future rounds that haven't been played yet
		if (playoffRound < 2)
		{
			HideBracketDisplay(semisDisplay, roundOf16Display.Length + quartersDisplay.Length);
		}

		if (playoffRound < 3)
		{
			HideBracketDisplay(finalsDisplay, roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length);
		}

		if (playoffRound < 4)
		{
			winnerDisplay.name.transform.parent.gameObject.SetActive(false);
			row[30].SetActive(false);
		}

		// ALWAYS increment round after simulating (both "Sim All" and "Return from game" scenarios)
		playoffRound++;
		gsp.playoffRound = playoffRound;
		tm.playoffRound = playoffRound;
		Debug.Log($"[SimPlayoff] Advanced to Round {playoffRound}");
		
		simButton.gameObject.SetActive(false);
		contButton.gameObject.SetActive(true);
		StartCoroutine(SetPlayoffs());

		yield break;
	}

	IEnumerator LoadCareer()
	{
		gsp.LoadCareer();

		yield return careerEarningsText.text = "$ " + gsp.tournyEarnings.ToString();
	}
    public void SavePlayoff()
    {
        if (cm != null)
            cm.SaveTournyState(this);
    }

    IEnumerator SaveCareer(bool inProgress)
	{
		Debug.Log("Saving in PlayoffManager, inProgress is " + inProgress);

		myFile = new EasyFileSave("my_player_data");

		//myFile.Add("Career Record", gsp.record);
		//Debug.Log("gsp.record is " + tm.teams[tm.playerTeam].wins + " - " + tm.teams[tm.playerTeam].loss);
		myFile.Add("BG", gsp.bg);
		//Vector2 tempRecord = new Vector2(gsp.record.x, gsp.record.y);
		//myFile.Add("Player Name", gsp.firstName);
		//myFile.Add("Team Name", gsp.teamName);
		//myFile.Add("Team Colour", gsp.teamColour);
		//myFile.Add("Career Earnings", gsp.earnings);

		//if (!inProgress)
		//      {
		//	tm.draw = 0;
		//	playoffRound = 0;
		//	tm.playoffRound = 0;

		//      }
		myFile.Add("Tourny In Progress", inProgress);
		gsp.tournyInProgress = inProgress;
		Debug.Log("gsp.inProgress is " + gsp.tournyInProgress);
		myFile.Add("Single Knockout Tourny", gsp.KO1);
		//myFile.Add("Draw", gsp.draw);
		myFile.Add("Number Of Teams", gsp.numberOfTeams);
		//myFile.Add("Player Team", gsp.playerTeamIndex);
		myFile.Add("OppTeam", oppTeam);
		myFile.Add("Playoff Round", playoffRound);

		string[] nameList = new string[gsp.teams.Length];
		int[] winsList = new int[gsp.teams.Length];
		int[] lossList = new int[gsp.teams.Length];
		int[] rankList = new int[gsp.teams.Length];
		int[] strengthList = new int[gsp.teams.Length];
		int[] idList = new int[gsp.teams.Length];
		float[] earningsList = new float[gsp.teams.Length];
		bool[] playerList = new bool[gsp.teams.Length];

		for (int i = 0; i < gsp.teams.Length; i++)
		{
			nameList[i] = gsp.teams[i].name;
			winsList[i] = gsp.teams[i].wins;
			lossList[i] = gsp.teams[i].loss;
			rankList[i] = gsp.teams[i].rank;
			strengthList[i] = gsp.teams[i].strength;
			idList[i] = gsp.teams[i].id;
			earningsList[i] = gsp.teams[i].earnings;
			playerList[i] = gsp.teams[i].player;
		}

		myFile.Add("Tourny Name List", nameList);
		myFile.Add("Tourny Wins List", winsList);
		myFile.Add("Tourny Loss List", lossList);
		myFile.Add("Tourny Rank List", rankList);
		myFile.Add("Tourny Strength List", strengthList);
		myFile.Add("Tourny Team ID List", idList);
		myFile.Add("Tourny Earnings List", earningsList);
		myFile.Add("Tourny Player List", playerList);

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

#region SharedTournamentLogic - Phase 1 Refactoring Helper

/// <summary>
/// Shared logic for all tournament types - eliminates code duplication
/// across PlayoffManager, PlayoffManager_SingleK, and PlayoffManager_TripleK
/// </summary>
public static class SharedTournamentLogic
{
	/// <summary>
	/// Simulate a match between two teams based on their strength
	/// </summary>
	/// <returns>The ID of the winning team</returns>
	public static int SimulateMatch(Team team1, Team team2)
	{
		if (Random.Range(0, team1.strength) > Random.Range(0, team2.strength))
			return team1.id;
		else
			return team2.id;
	}

	/// <summary>
	/// Record match result for both teams (updates tournament stats)
	/// </summary>
	public static void RecordMatchResult(Team winner, Team loser)
	{
		winner.tournamentWins++;
		loser.tournamentLosses++;
	}

	/// <summary>
	/// Calculate prize distribution using exponential decay formula
	/// Matches the existing prize calculation logic
	/// </summary>
	/// <param name="rank">Team's final rank (1st, 2nd, 3rd, etc.)</param>
	/// <param name="totalTeams">Total number of teams in tournament</param>
	/// <param name="totalPrize">Total prize pool</param>
	/// <returns>Prize amount for this rank</returns>
	public static float CalculatePrize(int rank, int totalTeams, float totalPrize)
	{
		switch (rank)
		{
			case 1: return totalPrize * 0.5f;      // 50% for 1st
			case 2: return totalPrize * 0.25f;     // 25% for 2nd
			case 3: return totalPrize * 0.15f;     // 15% for 3rd
			case 4: return totalPrize * 0.075f;    // 7.5% for 4th
			case 5: return totalPrize * 0.038f;    // 3.8% for 5th
			default:
				// Exponential decay for remaining positions
				float p = 1.4f;
				float remaining = totalTeams - 5f;
				float prizePayout = ((Mathf.Pow(p, remaining - (rank - 6))) /
								   (Mathf.Pow(p, remaining) - 1f)) *
								   (totalPrize * 0.15f) * (p - 1);
				return prizePayout;
		}
	}

	/// <summary>
	/// Determine player's opponent in a bracket array
	/// Assumes teams are paired sequentially (0vs1, 2vs3, etc.)
	/// </summary>
	/// <param name="bracket">Array of teams in bracket</param>
	/// <param name="playerTeamId">ID of player's team</param>
	/// <param name="startIndex">Starting index in bracket to search from</param>
	/// <param name="matchCount">Number of matches to check</param>
	/// <returns>ID of opponent team, or -1 if not found</returns>
	public static int GetOpponentId(Team[] bracket, int playerTeamId, int startIndex, int matchCount)
	{
		for (int i = 0; i < matchCount; i++)
		{
			int team1Index = startIndex + (i * 2);
			int team2Index = startIndex + (i * 2) + 1;

			if (team1Index >= bracket.Length || team2Index >= bracket.Length)
				break;

			if (bracket[team1Index].id == playerTeamId)
				return bracket[team2Index].id;
			else if (bracket[team2Index].id == playerTeamId)
				return bracket[team1Index].id;
		}

		return -1;
	}

	/// <summary>
	/// Find the index of player's team in a bracket array
	/// </summary>
	/// <param name="bracket">Array of teams</param>
	/// <returns>Index of player team, or -1 if not found</returns>
	public static int FindPlayerTeamIndex(Team[] bracket)
	{
		for (int i = 0; i < bracket.Length; i++)
		{
			if (bracket[i] != null && bracket[i].player)
				return i;
		}
		return -1;
	}

	/// <summary>
	/// Determine if player won their match based on game settings
	/// </summary>
	/// <param name="gsp">Game settings persist object</param>
	/// <returns>True if player won, false otherwise</returns>
	public static bool DeterminePlayerWon(GameSettingsPersist gsp)
	{
		if (gsp.playerTeam.name == gsp.redTeamName)
			return gsp.redScore > gsp.yellowScore;
		else if (gsp.playerTeam.name == gsp.yellowTeamName)
			return gsp.yellowScore > gsp.redScore;

		Debug.LogWarning("[SharedTournamentLogic] Could not determine player team color!");
		return false;
	}

	/// <summary>
	/// Get elimination rank based on tournament round
	/// For single elimination tournaments
	/// </summary>
	/// <param name="round">Round number (1-4)</param>
	/// <returns>Rank assigned to teams eliminated in this round</returns>
	public static int GetSingleEliminationRank(int round)
	{
		switch (round)
		{
			case 1: return 9;  // Lost in Round of 16 (9th-16th place)
			case 2: return 5;  // Lost in Quarterfinals (5th-8th place)
			case 3: return 3;  // Lost in Semifinals (3rd-4th place)
			case 4: return 2;  // Lost in Finals (2nd place)
			default: return 99; // Unknown
		}
	}

	/// <summary>
	/// Get elimination rank for Page Playoff system
	/// </summary>
	/// <param name="round">Round number (1-3)</param>
	/// <param name="position">Position in that round</param>
	/// <returns>Rank assigned</returns>
	public static int GetPagePlayoffRank(int round, int position)
	{
		// Page Playoff: 1v2 (winner to finals), 3v4
		// Round 2: Loser of 1v2 vs Winner of 3v4
		// Round 3: Finals

		if (round == 1)
		{
			// Lost in first round
			return position == 0 ? 3 : 4; // 1v2 loser gets 3rd, 3v4 loser gets 4th
		}
		else if (round == 2)
		{
			return 3; // Lost in semis gets 3rd
		}
		else if (round == 3)
		{
			return 2; // Lost in finals gets 2nd
		}

		return 99;
	}

	/// <summary>
	/// Calculate next round index for single elimination bracket
	/// </summary>
	/// <param name="round">Current round (1-4)</param>
	/// <param name="currentIndex">Current position in bracket</param>
	/// <returns>Index in next round's bracket</returns>
	public static int CalculateNextRoundIndex(int round, int currentIndex)
	{
		// Bracket layout for 16-team single elimination:
		// Round 1 (0-15)   -> Round 2 (16-23)
		// Round 2 (16-23)  -> Round 3 (24-27)
		// Round 3 (24-27)  -> Round 4 (28-29)
		// Round 4 (28-29)  -> Finals (30)

		int[] roundStarts = { 0, 16, 24, 28, 30 };

		if (round < 1 || round >= roundStarts.Length - 1)
		{
			Debug.LogError($"[SharedTournamentLogic] Invalid round: {round}");
			return -1;
		}

		int positionInRound = currentIndex - roundStarts[round - 1];
		int nextRoundStart = roundStarts[round];

		return nextRoundStart + (positionInRound / 2);
	}

	/// <summary>
	/// Get round configuration for single elimination
	/// Returns [startIndex, matchCount, nextRoundStart]
	/// </summary>
	public static int[] GetSingleEliminationRoundConfig(int round)
	{
		switch (round)
		{
			case 1: return new int[] { 0, 8, 16 };   // Round of 16: 8 matches
			case 2: return new int[] { 16, 4, 24 };  // Quarterfinals: 4 matches
			case 3: return new int[] { 24, 2, 28 };  // Semifinals: 2 matches
			case 4: return new int[] { 28, 1, 30 };  // Finals: 1 match
			default:
				Debug.LogError($"[SharedTournamentLogic] Invalid round: {round}");
				return new int[] { 0, 0, 0 };
		}
	}

	/// <summary>
	/// Get round configuration for Page Playoff system (4 teams)
	/// Returns [startIndex, matchCount, nextRoundStart, displayEndIndex]
	/// Bracket: [0-3] initial, [4-6] semi-finals, [7] finals winner loser, [8] champion
	/// </summary>
	public static int[] GetPagePlayoffRoundConfig(int round)
	{
		switch (round)
		{
			case 1: return new int[] { 0, 2, 4, 7 };  // Round 1: 1v2, 3v4 -> positions 4,5,6
			case 2: return new int[] { 4, 1, 7, 8 };  // Semifinals: 5v6 -> position 7
			case 3: return new int[] { 4, 1, 8, 9 };  // Finals: 4v7 -> position 8
			case 4: return new int[] { 0, 0, 9, 9 };  // Tournament complete
			default:
				Debug.LogError($"[SharedTournamentLogic] Invalid Page Playoff round: {round}");
				return new int[] { 0, 0, 0, 0 };
		}
	}

	/// <summary>
	/// Apply tournament earnings to season earnings
	/// Called at end of tournament
	/// </summary>
	public static void CompleteTournamentForTeam(Team team)
	{
		team.CompleteTournament(); // Uses Team's built-in method
	}

	/// <summary>
	/// Reset tournament stats for a team
	/// Called at start of new tournament
	/// </summary>
	public static void StartTournamentForTeam(Team team)
	{
		team.StartTournament(); // Uses Team's built-in method
	}

	/// <summary>
	/// Distribute prizes to all teams based on final rankings
	/// </summary>
	/// <param name="teams">Array of teams</param>
	/// <param name="totalPrize">Total prize pool</param>
	public static void DistributePrizes(Team[] teams, float totalPrize)
	{
		int totalTeams = teams.Length;

		foreach (Team team in teams)
		{
			if (team == null) continue;

			float prize = CalculatePrize(team.rank, totalTeams, totalPrize);
			team.tournamentEarnings += prize;

			Debug.Log($"[SharedTournamentLogic] {team.name} (Rank {team.rank}) receives ${prize:n0}");
		}
	}

	/// <summary>
	/// Log tournament state for debugging
	/// </summary>
	public static void LogTournamentState(Team[] teams, int currentRound, string tournamentType)
	{
		Debug.Log($"=== {tournamentType} - Round {currentRound} ===");

		for (int i = 0; i < teams.Length; i++)
		{
			if (teams[i] == null) continue;

			string playerMarker = teams[i].player ? " [PLAYER]" : "";
			Debug.Log($"[{i}] {teams[i].name}{playerMarker} - " +
					 $"W:{teams[i].tournamentWins} L:{teams[i].tournamentLosses} " +
					 $"Rank:{teams[i].rank} Earnings:${teams[i].tournamentEarnings:n0}");
		}
	}
}

#endregion