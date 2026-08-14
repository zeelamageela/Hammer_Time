using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TigerForge;
using System;
using Random = UnityEngine.Random;

public class TournyManager : MonoBehaviour
{
	public PlayoffManager pm;
	public StandingDisplay[] standDisplay;
	public BracketDisplay[] brackDisplay;
	public VSDisplay[] vsDisplay;
	public TournyTeamList tTeamList;
	public Team[] teams;
	public DrawFormat[] drawFormat;
	public Team[] playoffTeams;
	public List<Team_List> teamList;
	public DrawFormatList dfList;
	CareerManager cm;
	public bool cashGame;

	public GameObject[] standDisplayTest;
	public GameObject standings;
	//public Transform panel;
	public Transform standTextParent;
	GameObject[] row;
	public GameObject standTextRow;
	public GameObject playoffs;
	public GameObject semiWinner;
	public GameObject finalWinner;
	public GameObject vs;
	public Text vsTitle;
	public Text vsVS;
	public GameObject vsText;
	public GameObject vsNextGame;

	public Button simButton;
	public Button contButton;
	public Button playButton;
	public Text heading;
	public Text careerEarningsText;
	public Scrollbar scrollBar;
	public Scrollbar standScrollBar;

	GameSettingsPersist gsp;
	EasyFileSave myFile;
	public int numberOfTeams;
	public int prize;
	public int draw;
	public int playoffRound;
	public int playerTeam;
	public int oppTeam;
	
	// Two-pool tournament tracking
	public int currentPoolView;        // 0 for Pool A, 1 for Pool B
	public bool isTwoPoolTournament;   // True if this is a 2-pool tournament
	public GameObject poolUIParent;    // Parent GameObject containing pool label and buttons
	public Text poolLabel;             // UI Text to display "Pool A" or "Pool B"
	public Button poolLeftButton;      // Button to switch to Pool A (previous)
	public Button poolRightButton;     // Button to switch to Pool B (next)
	
	public Vector2 careerRecord;
	public float careerEarnings;
	string teamName;
	public bool isStandingsReady;

	// Start is called before the first frame update
	void Start()
	{
		cm = FindFirstObjectByType<CareerManager>();
		gsp = GameSettingsPersist.instance;
		//Debug.Log("Number of Teams at top of start - " + gsp.numberOfTeams);

		careerEarnings = cm.earnings;

		//Debug.Log("Gsp In Progress is " + gsp.inProgress);
		//Debug.Log("Gsp Career Load is " + gsp.careerLoad);

		if (gsp.careerLoad)
		{
			//cm.LoadCareer();
			//gsp.LoadCareer();
            if (gsp.tournyInProgress)
            {
                //gsp.gameInProgress = false;
                gsp.LoadTourny();

                //playoffRound--;
                Debug.Log("Playoff Round is " + gsp.playoffRound);
            }
            else
            {
                Debug.Log("In Progress is False");
                gsp.draw = draw;
                gsp.playoffRound = 0;
            }
        }

		if (cashGame)
        {
			CashGames cg = FindFirstObjectByType<CashGames>();
			numberOfTeams = gsp.numberOfTeams;
			prize = gsp.prize;
			careerEarningsText.text = "$ " + gsp.tournyEarnings.ToString();

			teams = new Team[numberOfTeams];

			teamList = new List<Team_List>();
			cg.SetUp();
        }
		else
		{
			numberOfTeams = gsp.numberOfTeams;
			prize = gsp.prize;
			careerEarningsText.text = "$ " + gsp.tournyEarnings.ToString();

			int cmTeamCount = (cm != null && cm.currentTournyTeams != null) ? cm.currentTournyTeams.Length : 0;
			Debug.Log($"[TournyManager.Start] numberOfTeams={numberOfTeams}, cm.currentTournyTeams.Length={cmTeamCount}, gsp.draw={gsp.draw}");

			if (numberOfTeams > 0)
				teams = new Team[numberOfTeams];
			else if (cmTeamCount > 0)
				teams = new Team[cmTeamCount];
			else
			{
				Debug.LogError($"[TournyManager] numberOfTeams=0 and cm.currentTournyTeams unavailable — defaulting to 8 teams to avoid hang");
				teams = new Team[8];
			}

			teamList = new List<Team_List>();

			standDisplay = new StandingDisplay[teams.Length];

			StartCoroutine(SetupStandings());

			//Debug.Log("Draw at top of start - " + gsp.weight);

			//PrintRows(teams);
		}
	}

	public void ClearMoney()
	{
		myFile = new EasyFileSave("my_player_data");

		if (myFile.Load())
		{
			myFile.Dispose();
		}
		careerEarnings = 0;
		//myFile.Add("Career Earnings", gsp.earnings);
		myFile.Save();
		careerEarningsText.text = "$ " + gsp.tournyEarnings.ToString();
	}

	IEnumerator RefreshPanel()
	{
		for (int i = 0; i < standDisplay.Length; i++)
        {
			standDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = false;
			standDisplay[i].nextOpp.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = false;

			yield return new WaitForEndOfFrame();
			standDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = true;
			standDisplay[i].nextOpp.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = true;
		}

        //OnSim();
        //for (int i = 0; i < vsDisplay.Length; i++)
        //{
        //    vsDisplay[i].name.gameObject.GetComponent<ContentSizeFitter>().enabled = false;

        //    yield return new WaitForEndOfFrame();
        //    vsDisplay[i].name.gameObject.GetComponent<ContentSizeFitter>().enabled = true;
        //}
    }

	IEnumerator SetupStandings()
    {
        isStandingsReady = false;
        cm = FindFirstObjectByType<CareerManager>();
		//yield return new WaitUntil(() => teams.Length >= numberOfTeams);
		row = new GameObject[teams.Length];
		//Debug.Log("Setup Stand Team Length is " + teams.Length);
		yield return new WaitUntil(() => teams.Length > 0);

		// Clear any pre-existing children (e.g. template rows placed in the scene for layout reference)
		foreach (Transform child in standTextParent)
			Destroy(child.gameObject);
		yield return new WaitForEndOfFrame();

		// CRITICAL FIX: Always initialize drawFormat, even when loading saved tournament
		// This was skipped when weight > 0, causing drawFormat.Length = 0
		for (int i = 0; i < teams.Length; i++)
		{
			row[i] = Instantiate(standTextRow, standTextParent);
			row[i].name = "Row " + (i + 1);
			row[i].GetComponent<RectTransform>().position = new Vector2(0f, i * -125f);
			//Text[] tList = row.transform.GetComponentsInChildren<Text>();

			RowVariables rv = row[i].GetComponent<RowVariables>();
			yield return new WaitForEndOfFrame();

			standDisplay[i] = rv.standDisplay;
		}
		
		// Initialize pool UI as hidden (will be shown if needed)
		if (poolUIParent != null)
		{
			poolUIParent.SetActive(false);
		}

        if (gsp.draw > 0)
        {
            playoffRound = gsp.playoffRound;
            teams = gsp.teams;
            draw = gsp.draw;

            // Rebuild teamList from loaded teams
            teamList = new List<Team_List>();
            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i] != null)
                {
                    teamList.Add(new Team_List(teams[i]));
                }
            }
            Debug.Log($"[TournyManager] Rebuilt teamList from save - {teamList.Count} teams");

            // CRITICAL: Find player team by player flag, not by index
            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i].player)
                {
                    playerTeam = i;
                    Debug.Log($"[TournyManager] Found player team at index {playerTeam}: {teams[i].name}");
                    break;
                }
            }

            // Find opponent team
            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i].name == teams[playerTeam].nextOpp)
                {
                    oppTeam = i;
                    Debug.Log($"[TournyManager] Found opponent team at index {oppTeam}: {teams[i].name}");
                    break;
                }
            }

            // Initialize drawFormat
            Debug.Log($"[TournyManager.SetupStandings] Calling DrawSelector with teams.Length={teams.Length}, games={gsp.games}");
            
            if (teams.Length == 16)
            {
                // Check if this is a two-pool tournament by checking team poolIds
                bool hasPools = false;
                for (int i = 0; i < teams.Length; i++)
                {
                    if (teams[i].poolId >= 0)
                    {
                        hasPools = true;
                        break;
                    }
                }
                
                if (hasPools)
                {
                    dfList.DrawSelector_TwoPool(teams.Length, 1, gsp.games);
                    isTwoPoolTournament = true;
                    currentPoolView = teams[playerTeam].poolId;
                    
                    // Show pool UI
                    if (poolUIParent != null)
                    {
                        poolUIParent.SetActive(true);
                    }
                    
                    if (poolLabel != null)
                    {
                        poolLabel.text = currentPoolView == 0 ? "Pool A" : "Pool B";
                    }
                    
                    // Update button visibility
                    UpdatePoolButtonVisibility();
                    
                    Debug.Log($"[TournyManager] Restored two-pool tournament, viewing Pool {(currentPoolView == 0 ? "A" : "B")}");
                }
                else
                {
                    dfList.DrawSelector(teams.Length, 1, gsp.games);
                }
            }
            else
            {
                dfList.DrawSelector(teams.Length, 1, gsp.games);
            }
            
            yield return new WaitForEndOfFrame();
            drawFormat = dfList.currentFormat;

            Debug.Log($"[TournyManager.SetupStandings] weight={draw}, drawFormat.Length={drawFormat?.Length ?? 0}, teamList.Count={teamList.Count}");

            // CRITICAL: Sync cm.record with tournament stats
            if (cm != null)
            {
                cm.record.x = teams[playerTeam].seasonWins;
                cm.record.y = teams[playerTeam].seasonLosses;
                Debug.Log($"[TournyManager] Synced cm.record to {cm.record.x}-{cm.record.y}");
            }

            if (playoffRound > 0)
            {
				pm.enabled = true;
				standings.SetActive(false);
				
				// Hide pool UI when entering playoffs
				if (poolUIParent != null)
				{
					poolUIParent.SetActive(false);
				}
			}
			else if (gsp.justFinishedGame)
			{
				// CRITICAL FIX: Game just finished - process the result and update standings
				Debug.Log("[TournyManager] Just finished game - processing result");
				gsp.justFinishedGame = false;  // Clear the flag
				
				// Find player and opponent teams
				for (int i = 0; i < teams.Length; i++)
				{
					if (teams[i].player)
						playerTeam = i;
				}
				for (int i = 0; i < teams.Length; i++)
				{
					if (teams[i].name == teams[playerTeam].nextOpp)
						oppTeam = i;
				}
				Debug.Log("PlayerTeam is " + playerTeam);
				Debug.Log("OppTeam is " + oppTeam);

				// Process player's match result and update wins/losses
				ProcessPlayerMatchResult();
				
				// Simulate remaining games in the current weight
				StartCoroutine(SimRestDraw());
			}
			else if (gsp.gameInProgress)
			{
				gsp.tournyInProgress = true;
				Debug.Log("gsp.inProgress is " + gsp.tournyInProgress);
				gsp.gameInProgress = false;
				
				// Find player and opponent teams
				for (int i = 0; i < teams.Length; i++)
				{
					if (teams[i].player)
						playerTeam = i;
				}
				for (int i = 0; i < teams.Length; i++)
				{
					if (teams[i].name == teams[playerTeam].nextOpp)
						oppTeam = i;
				}
				Debug.Log("PlayerTeam is " + playerTeam);
				Debug.Log("OppTeam is " + oppTeam);

				// Process player's match result and update wins/losses
				ProcessPlayerMatchResult();
				
				// Simulate remaining games in the current weight
				StartCoroutine(SimRestDraw());
			}
			else if (gsp.tournyInProgress)
			{
				gsp.careerLoad = false;
				Debug.Log("Setup Stand inProgress is " + true);
				//playerTeam = gsp.playerTeamIndex;
				for (int i = 0; i < teams.Length; i++)
                {
                    if (teams[i].name == gsp.playerTeam.nextOpp)
                        oppTeam = i;
                    if (teams[i].name == gsp.playerTeam.name)
                        playerTeam = i;
                }
                //gsp.inProgress = false;
                yield return new WaitForEndOfFrame();
				StartCoroutine(DrawScoring());
            }
			else
			{
				gsp.tournyInProgress = true;
				Debug.Log("gsp.inProgress is " + gsp.tournyInProgress);
				
				// Find player and opponent teams
				for (int i = 0; i < teams.Length; i++)
				{
					if (teams[i].name == gsp.playerTeam.nextOpp)
						oppTeam = i;
					if (teams[i].player)
						playerTeam = i;
				}

				Debug.Log("OppTeam is " + oppTeam);

				// Process player's match result and update wins/losses
				ProcessPlayerMatchResult();
				
				// Simulate remaining games in the current weight
				StartCoroutine(SimRestDraw());
			}

		}
		else
        {
			if (cm == null || cm.currentTournyTeams == null || cm.currentTournyTeams.Length == 0)
			{
				Debug.LogError($"[TournyManager] Cannot start tournament: CareerManager.currentTournyTeams is {(cm == null ? "null (no CareerManager)" : cm.currentTournyTeams == null ? "null" : "empty")}. Was SetupTourny() called?");
				yield break;
			}
			teams = cm.currentTournyTeams;
			gsp.teams = teams;
			Debug.Log($"[TournyManager.SetupStandings] NEW tourny — teams.Length={teams.Length}, row.Length={row.Length}, standDisplay.Length={standDisplay.Length}");
			if (teams.Length != row.Length)
				Debug.LogWarning($"[TournyManager.SetupStandings] MISMATCH: teams({teams.Length}) != row({row.Length}) — extra rows will be hidden");
			cm.teamRecords = new Vector4[teams.Length];
			
			// Ensure gsp.games is valid before handing it to DrawSelector.
			// If LoadTournySettings wasn't reached (e.g. cm null crash), games stays 0 and
			// DrawSelector returns an empty format, which crashes SetDraw().
			if (gsp.games <= 0)
			{
				int defaultGames = teams.Length > 12 ? 5 : teams.Length > 8 ? 4 : 3;
				Debug.LogWarning($"[TournyManager] gsp.games={gsp.games} is invalid — defaulting to {defaultGames} for {teams.Length} teams");
				gsp.games = defaultGames;
			}

			// CRITICAL FIX: Initialize drawFormat for NEW tournament
			// For 16-team tournaments, use two-pool format (Pool A and Pool B)
			Debug.Log($"[TournyManager.SetupStandings] NEW tournament - Calling DrawSelector with teams.Length={teams.Length}, games={gsp.games}");
			
			if (teams.Length == 16)
			{
				// Use two-pool format for 16-team tournaments
				dfList.DrawSelector_TwoPool(teams.Length, 1, gsp.games);
				Debug.Log("[TournyManager] Using two-pool format for 16 teams");
			}
			else
			{
				// Use regular format for other team counts
				dfList.DrawSelector(teams.Length, 1, gsp.games);
			}
			
			yield return new WaitForEndOfFrame();
			drawFormat = dfList.currentFormat;
			Debug.Log($"[TournyManager.SetupStandings] drawFormat.Length={drawFormat?.Length ?? 0}");

			for (int i = 0; i < teams.Length; i++)
			{
				cm.teamRecords[i].x = teams[i].wins;
				cm.teamRecords[i].y = teams[i].loss;
				cm.teamRecords[i].z = teams[i].earnings;
				cm.teamRecords[i].w = teams[i].id;

				teams[i].wins = 0;
				teams[i].loss = 0;
				teams[i].earnings = 0;
			}

			Debug.Log("Team Record - " + teams[0].name + " - " + cm.teamRecords[0]);

			for (int i = 0; i < teams.Length; i++)
			{
				teams[i].strength = Random.Range(0, 10);
				if (teams[i].player)
				{
					float strength = cm.cStats.weightAccuracy
						+ cm.cStats.aimAccuracy
						+ cm.cStats.finesseAccuracy
						+ cm.cStats.sweepStrength
						+ cm.cStats.sweepEndurance
						+ cm.cStats.sweepCohesion
						+ cm.modStats.weightAccuracy
						+ cm.modStats.aimAccuracy
						+ cm.modStats.finesseAccuracy
						+ cm.modStats.sweepStrength
						+ cm.modStats.sweepEndurance;

                    teams[i].strength = Mathf.RoundToInt(strength / 6f);
					playerTeam = i;
				}
				teamList.Add(new Team_List(teams[i]));
			}

			//teamList[playerTeam].team.name = gsp.teamName;
			teamList.Sort();
			yield return new WaitForEndOfFrame();
			
			// Set up two-pool tournament if we have 16 teams
			if (teams.Length == 16)
			{
				SetupTwoPoolTournament();
			}
			
			if (!gsp.KO1) 
				SetDraw();
		}

        isStandingsReady = true;
        yield return new WaitForEndOfFrame();
	}

	void Shuffle(Team[] a)
	{
		// Loops through array
		for (int i = a.Length - 1; i > 0; i--)
		{
			// Randomize a number between 0 and i (so that the range decreases each time)
			int rnd = Random.Range(0, i);

			// Save the value of the current i, otherwise it'll overright when we swap the values
			Team temp = a[i];

			// Swap the new and old values
			a[i] = a[rnd];
			a[rnd] = temp;
		}

		// Print
		//PrintRows(a);
		//for (int i = 0; i < a.Length; i++)
		//{
		//	Print;
		//}
	}

	void PrintRows()
	{
		int tempRank;
		teamList.Sort();

		// Filter teams by pool if this is a two-pool tournament
		List<Team_List> displayTeams = teamList;
		if (isTwoPoolTournament)
		{
			displayTeams = new List<Team_List>();
			for (int i = 0; i < teamList.Count; i++)
			{
				if (teamList[i].team.poolId == currentPoolView)
				{
					displayTeams.Add(teamList[i]);
				}
			}
			Debug.Log($"[TournyManager] PrintRows - Filtered to {displayTeams.Count} teams for Pool {(currentPoolView == 0 ? "A" : "B")}");
		}

		// Assign ranks within the pool
		for (int i = 0; i < displayTeams.Count; i++)
		{
			displayTeams[i].team.rank = i + 1;
		}

		// Display teams — cap at available display slots to avoid index-out-of-bounds
		int showCount = Mathf.Min(displayTeams.Count, standDisplay.Length, row.Length);
		if (showCount < displayTeams.Count)
			Debug.LogWarning($"[TournyManager.PrintRows] Team count ({displayTeams.Count}) > display slots ({showCount}) — truncating display");
		for (int i = 0; i < showCount; i++)
		{
			row[i].SetActive(true);
			standDisplay[i].name.text = displayTeams[i].team.name;
			standDisplay[i].wins.text = displayTeams[i].team.wins.ToString();
			standDisplay[i].loss.text = displayTeams[i].team.loss.ToString();
			standDisplay[i].nextOpp.text = displayTeams[i].team.nextOpp;
		}

		// Hide unused display rows (always, not just for two-pool)
		for (int i = showCount; i < row.Length; i++)
		{
			row[i].SetActive(false);
		}

		vsDisplay[0].name.text = teams[playerTeam].name;
		vsDisplay[0].rank.text = teams[playerTeam].rank.ToString();

		for (int i = 0; i < showCount; i++)
		{
			if (teams[playerTeam].name == displayTeams[i].team.name)
				standDisplay[i].panel.enabled = true;
			else
				standDisplay[i].panel.enabled = false;

			if (teams[playerTeam].nextOpp == displayTeams[i].team.name)
			{
				tempRank = i + 1;
				vsDisplay[1].name.text = displayTeams[i].team.name;
				vsDisplay[1].rank.text = displayTeams[i].team.rank.ToString();
			}
		}

		int displayTeamCount = isTwoPoolTournament ? displayTeams.Count : showCount;
		standScrollBar.value = (teams[playerTeam].rank - displayTeamCount) / (1f - displayTeamCount);
		StartCoroutine(RefreshPanel());

		// Save tournament state using CareerManager's save system
		cm.SaveCareer();
	}

	/// <summary>
	/// Processes player's match result and updates wins/losses
	/// </summary>
	void ProcessPlayerMatchResult()
	{
		bool playerWon = false;
		int playerScore = 0;
		int oppScore = 0;
		
		// Determine if player won based on their team color
		if (teams[playerTeam].name == gsp.redTeamName)
		{
			playerScore = gsp.redScore;
			oppScore = gsp.yellowScore;
			playerWon = gsp.redScore > gsp.yellowScore;
		}
		else if (teams[playerTeam].name == gsp.yellowTeamName)
		{
			playerScore = gsp.yellowScore;
			oppScore = gsp.redScore;
			playerWon = gsp.yellowScore > gsp.redScore;
		}
		else
		{
			Debug.LogWarning("[TournyManager] Could not determine player team color!");
			return;
		}

		// Update wins and losses
		if (playerWon)
		{
			teams[playerTeam].wins++;
			teams[oppTeam].loss++;
		}
		else
		{
			teams[oppTeam].wins++;
			teams[playerTeam].loss++;
		}
		
		// Update point differential for two-pool tournaments
		if (isTwoPoolTournament)
		{
			int differential = playerScore - oppScore;
			teams[playerTeam].pointDifferential += differential;
			teams[oppTeam].pointDifferential -= differential;
			Debug.Log($"[TournyManager] Point differential updated: {teams[playerTeam].name} = {teams[playerTeam].pointDifferential}, {teams[oppTeam].name} = {teams[oppTeam].pointDifferential}");
		}

		Debug.Log($"[TournyManager] Player match result: {teams[playerTeam].name} ({playerScore}) vs {teams[oppTeam].name} ({oppScore}) - Player won: {playerWon}");
		Debug.Log($"[TournyManager] {teams[playerTeam].name}: {teams[playerTeam].wins}W-{teams[playerTeam].loss}L, {teams[oppTeam].name}: {teams[oppTeam].wins}W-{teams[oppTeam].loss}L");
	}

	/// <summary>
	/// Sets up a two-pool tournament by assigning teams to pools
	/// </summary>
	void SetupTwoPoolTournament()
	{
		Debug.Log("[TournyManager] Setting up two-pool tournament");
		
		// Assign teams to pools: 0-7 = Pool A, 8-15 = Pool B
		for (int i = 0; i < teams.Length; i++)
		{
			if (i < 8)
			{
				teams[i].poolId = 0; // Pool A
			}
			else
			{
				teams[i].poolId = 1; // Pool B
			}
			Debug.Log($"[TournyManager] Team {i}: {teams[i].name} assigned to Pool {(teams[i].poolId == 0 ? "A" : "B")}");
		}
		
		// Find which pool the player is in and set it as the current view
		for (int i = 0; i < teams.Length; i++)
		{
			if (teams[i].player)
			{
				currentPoolView = teams[i].poolId;
				Debug.Log($"[TournyManager] Player team found in Pool {(currentPoolView == 0 ? "A" : "B")}");
				break;
			}
		}
		
		// Show pool UI
		if (poolUIParent != null)
		{
			poolUIParent.SetActive(true);
		}
		
		// Set pool label text
		if (poolLabel != null)
		{
			poolLabel.text = currentPoolView == 0 ? "Pool A" : "Pool B";
		}
		
		// Update button visibility
		UpdatePoolButtonVisibility();
		
		isTwoPoolTournament = true;
	}

	/// <summary>
	/// Updates which pool navigation buttons are visible based on current pool
	/// Pool A (0): Only show right arrow
	/// Pool B (1): Only show left arrow
	/// </summary>
	void UpdatePoolButtonVisibility()
	{
		if (poolLeftButton != null && poolRightButton != null)
		{
			if (currentPoolView == 0) // Pool A
			{
				poolLeftButton.gameObject.SetActive(false);
				poolRightButton.gameObject.SetActive(true);
			}
			else // Pool B
			{
				poolLeftButton.gameObject.SetActive(true);
				poolRightButton.gameObject.SetActive(false);
			}
		}
	}

	/// <summary>
	/// Switch to Pool A (called by left button)
	/// </summary>
	public void SwitchToPoolA()
	{
		if (isTwoPoolTournament && currentPoolView != 0)
		{
			currentPoolView = 0;
			if (poolLabel != null)
			{
				poolLabel.text = "Pool A";
			}
			UpdatePoolButtonVisibility();
			PrintRows();
			Debug.Log("[TournyManager] Switched to Pool A");
		}
	}

	/// <summary>
	/// Switch to Pool B (called by right button)
	/// </summary>
	public void SwitchToPoolB()
	{
		if (isTwoPoolTournament && currentPoolView != 1)
		{
			currentPoolView = 1;
			if (poolLabel != null)
			{
				poolLabel.text = "Pool B";
			}
			UpdatePoolButtonVisibility();
			PrintRows();
			Debug.Log("[TournyManager] Switched to Pool B");
		}
	}

    #region Set
    void SetDraw()
    {
		//Debug.Log("Setting Draw - " + weight);
		if (draw < drawFormat.Length)
		{
			for (int i = 0; i < drawFormat[draw].game.Length; i++)
			{
				teams[drawFormat[draw].game[i].x].nextOpp = teams[drawFormat[draw].game[i].y].name;
				teams[drawFormat[draw].game[i].y].nextOpp = teams[drawFormat[draw].game[i].x].name;
			}
		}
		else if (draw == drawFormat.Length)
        {
			for (int i = 0; i < teamList.Count; i++)
            {
				teamList[i].team.nextOpp = "-----";
            }
        }

		for (int i = 0; i < teams.Length; i++)
		{
			if (teams[i].name == teams[playerTeam].nextOpp)
				oppTeam = i;
		}

		//yield return new WaitUntil(() => standDisplay.Length >= row.Length);

		//yield return new WaitUntil(() => standDisplay.Length );
		PrintRows();
    }

    #endregion

    #region Sim
    IEnumerator SimDraw()
    {
		Team[] games = new Team[teams.Length];

		//SetDraw();
		for (int i = 0; i < teams.Length; i++)
        {
			if (i % 2 == 0)
				games[i] = teams[drawFormat[draw].game[i / 2].x];
			else
				games[i] = teams[drawFormat[draw].game[i / 2].y];
        }

		for (int i = 0; i < games.Length; i++)
		{
			if (i % 2 == 0)
			{
				int team1Score = Random.Range(0, games[i].strength + 1);
				int team2Score = Random.Range(0, games[i + 1].strength + 1);
				
				if (team1Score > team2Score)
				{
					games[i + 1].loss++;
					games[i].wins++;
					
					// Update point differential for two-pool tournaments
					if (isTwoPoolTournament)
					{
						int differential = team1Score - team2Score;
						games[i].pointDifferential += differential;
						games[i + 1].pointDifferential -= differential;
					}
				}
				else
				{
					games[i].loss++;
					games[i + 1].wins++;
					
					// Update point differential for two-pool tournaments
					if (isTwoPoolTournament)
					{
						int differential = team2Score - team1Score;
						games[i + 1].pointDifferential += differential;
						games[i].pointDifferential -= differential;
					}
				}
			}
		}

		// Legacy code - record property now just wraps seasonWins/seasonLosses
		// No need to manually sync since they're already the same
		// for (int i = 0; i < teams.Length; i++)
        // {
		// 	teams[i].record.x = teams[i].wins;
		// 	teams[i].record.y = teams[i].loss;
        // }

		Debug.Log("Tourny Record is " + teams[playerTeam].wins + " - " + teams[playerTeam].loss);
		draw++;
		//yield return new WaitForEndOfFrame();
		yield return StartCoroutine(DrawScoring());
	}

	IEnumerator SimRestDraw()
	{
		// Note: gsp.weight was already incremented by EndMenu.EndGame(), so we need to decrement it
		// to get back to the weight that was just played
		int tempDraw = draw - 1;
		Debug.Log($"[SimRestDraw] Starting - weight={draw}, tempDraw={tempDraw}, teams.Length={teams.Length}, drawFormat.Length={drawFormat.Length}");
		Team[] games = new Team[teams.Length];
		
		// Build games array from weight format
		// Note: EndMenu incremented gsp.weight, so weight-1 is the weight that was just played
		for (int i = 0; i < teams.Length; i++)
		{
			if (i % 2 == 0)
				games[i] = teams[drawFormat[tempDraw].game[i / 2].x];
			else
				games[i] = teams[drawFormat[tempDraw].game[i / 2].y];
		}
		
		yield return new WaitForEndOfFrame();

        // Simulate only the games that don't involve player or opponent
        // Simulate only the games that don't involve player or opponent
        for (int i = 0; i < games.Length; i++)
        {
            if (i % 2 == 0)
            {
                // Get the team indices from the weight format
                int team1Index = drawFormat[tempDraw].game[i / 2].x;
                int team2Index = drawFormat[tempDraw].game[i / 2].y;

                // Skip if either team is the player or opponent
                bool isPlayerGame = (team1Index == playerTeam || team1Index == oppTeam ||
                                     team2Index == playerTeam || team2Index == oppTeam);

                if (isPlayerGame)
                {
                    Debug.Log($"[SimRestDraw] SKIP player game: teams[{team1Index}] vs teams[{team2Index}]");
                    continue;
                }

                // Simulate other games
                Debug.Log($"[SimRestDraw] Simulating: teams[{team1Index}] vs teams[{team2Index}]");
                if (Random.Range(0, games[i].strength) > Random.Range(0, games[i + 1].strength))
                {
                    games[i + 1].loss++;
                    games[i].wins++;
                }
                else
                {
                    games[i].loss++;
                    games[i + 1].wins++;
                }
            }
        }

		yield return StartCoroutine(DrawScoring());
	}

	IEnumerator DrawScoring()
    {
		Debug.Log($"[DrawScoring] weight={draw}, drawFormat.Length={drawFormat.Length}");
		if (draw < drawFormat.Length)
		{
			Debug.Log("Draw number " + draw);
			yield return new WaitForSeconds(0.1f);
			heading.text = "Draw " + (draw + 1);
			SetDraw();
		}
		else if (draw == drawFormat.Length)
		{
			//Debug.Log("Final End");
			heading.text = "End of Draws";
			SetDraw();
			for (int i = 0; i < teams.Length; i++)
            {
				teams[i].nextOpp = "-----";
            }

			//SetDraw();
			if (cm.currentTourny != null && cm.currentTourny.qualifier)
			{
				vsTitle.text = "Results";
				if (teams[playerTeam].rank <= 4)
				{
					heading.text = "Qualified!";
					gsp.tournyEarnings += gsp.prize * 0.25f;
					//tm.teams[playerTeam].earnings = gsp.prize * 0.075f;

					vs.SetActive(true);

					vsVS.text = "wins";
					vsDisplay[0].name.text = teams[playerTeam].name;
					vsDisplay[0].rank.text = teams[playerTeam].rank.ToString();
					vsDisplay[1].name.text = "$" + (gsp.prize * 0.25f).ToString("n0");
					vsDisplay[1].rank.gameObject.SetActive(false);
				}
				else
				{
					heading.text = "Did Not Qualify";

					vs.SetActive(true);

					vsDisplay[0].name.text = teams[playerTeam].name;
					vsDisplay[0].rank.text = teams[playerTeam].rank.ToString();
					vsDisplay[1].name.text = "$0";
					vsDisplay[1].rank.gameObject.SetActive(false);
				}
				contButton.gameObject.SetActive(false);
				
				// Only access pm.nextButton if pm exists
				if (pm != null)
				{
					pm.nextButton.gameObject.SetActive(true);
				}
			}
			else
            {
				contButton.gameObject.SetActive(true);
				
				// Only access pm.nextButton if pm exists
				if (pm != null)
				{
					pm.nextButton.gameObject.SetActive(false);
				}
				
				playButton.gameObject.SetActive(false);
				simButton.gameObject.SetActive(false);
			}

			playButton.gameObject.SetActive(false);
			simButton.gameObject.SetActive(false);
		}
		else
			heading.text = "End of Round Robin";

	}
	#endregion

	public void OnSim()
	{
		//playoffRound = pm.playoffRound;
		if (pm != null && pm.playoffRound > 0)
		{
			pm.OnSim();
		}
		else if (draw < drawFormat.Length)
		{
			StartCoroutine(SimDraw());
		}
	}

	public void PlayDraw()
    {
		// CRITICAL: Pass forceNewGame=true to clear any previous game state
		// This is starting a NEW game from the tournament home screen
		gsp.TournySetup(btn: 0, forceNewGame: true);
		// Save immediately after TournySetup so the disk save reflects current tournament state
		// (team names, ends, rocks, tournyInProgress=true, current bracket).
		// Without this, the recovery branch in LoadTourny reads a stale save.
		if (cm == null) cm = FindFirstObjectByType<CareerManager>();
		if (cm != null) cm.SaveCareer();
		SceneManager.LoadScene("End_Menu_Tourny_1");
    }

	public void Menu()
    {
		//StartCoroutine(SaveCareer());

		SceneManager.LoadScene("SplashMenu");
    }

	public void TournyComplete()
    {
		CareerManager cm = FindFirstObjectByType<CareerManager>();
		gsp = FindFirstObjectByType<GameSettingsPersist>();
		gsp.teams = teams;
		
		// Don't overwrite playoff earnings - they're already calculated correctly
		// For cash games, use cm.cash directly
		if (gsp.cashGame)
		{
			gsp.tournyEarnings = cm.cash;
		}
		// For regular tournaments, gsp.tournyEarnings is already set by playoff managers
		// DO NOT overwrite it here!
		
		// Restore cumulative team stats (wins/losses/earnings) from before tournament started
		if (gsp.cashGame == false)
		{
			Debug.Log("cm.teamRecords Length is " + cm.teamRecords.Length);

			for (int i = 0; i < teams.Length; i++)
			{
				// Add back the cumulative stats that were saved before tournament
				teams[i].wins += (int)cm.teamRecords[i].x;
				teams[i].loss += (int)cm.teamRecords[i].y;
				teams[i].earnings += cm.teamRecords[i].z;
				teams[i].id = (int)cm.teamRecords[i].w;
			}
			
			Debug.Log($"[TournyManager] Player team cumulative record: {teams[playerTeam].wins}-{teams[playerTeam].loss}, earnings: ${teams[playerTeam].earnings:N0}");
		}

		//Debug.Log("PlayerTeam name is " + teams[playerTeam].name);
		Debug.Log("PlayerTeam record is " + gsp.tournyRecord.x + " - " + gsp.tournyRecord.y);

        gsp.draw = 0;
		gsp.playoffRound = 0;
		gsp.tournyInProgress = false;
		Debug.Log("gsp.inProgress is " + gsp.tournyInProgress);
		gsp.playoffTeams = null;
		Debug.Log("CM Record is " + cm.record.x + " - " + cm.record.y);
		Debug.Log("CM earnings are " + cm.earnings);
		
		cm.TournyResults();
		cm.LoadCareer();
		SceneManager.LoadScene("Arena_Selector");
    }

}









