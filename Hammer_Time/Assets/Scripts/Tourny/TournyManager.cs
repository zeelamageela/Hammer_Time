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
	
	public Vector2 careerRecord;
	public float careerEarnings;
	string teamName;
	public bool isStandingsReady;

	// Start is called before the first frame update
	void Start()
	{
		cm = FindFirstObjectByType<CareerManager>();
		gsp = GameObject.Find("GameSettingsPersist").GetComponent<GameSettingsPersist>();
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

			if (numberOfTeams > 0)
				teams = new Team[numberOfTeams];
			else
				teams = new Team[cm.currentTournyTeams.Length];

			teamList = new List<Team_List>();

			standDisplay = new StandingDisplay[teams.Length];

			StartCoroutine(SetupStandings());

			//Debug.Log("Draw at top of start - " + gsp.draw);

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
		dfList.DrawSelector(teams.Length, 1, gsp.games);

		yield return new WaitForEndOfFrame();

		drawFormat = dfList.currentFormat;

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

		if (gsp.draw > 0)
		{
			playoffRound = gsp.playoffRound;
			teamList = gsp.teamList;
			teams = gsp.teams;
			draw = gsp.draw;

			Debug.Log("draw is " + draw);

			if (playoffRound > 0)
			{
				pm.enabled = true;
				standings.SetActive(false);
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
				
				// Simulate remaining games in the current draw
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
				
				// Simulate remaining games in the current draw
				StartCoroutine(SimRestDraw());
			}

		}
		else
        {
			teams = cm.currentTournyTeams;
			gsp.teams = teams;
			cm.teamRecords = new Vector4[teams.Length];

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
					float strength = cm.cStats.drawAccuracy
						+ cm.cStats.takeOutAccuracy
						+ cm.cStats.guardAccuracy
						+ cm.cStats.sweepStrength
						+ cm.cStats.sweepEndurance
						+ cm.cStats.sweepCohesion
						+ cm.modStats.drawAccuracy
						+ cm.modStats.takeOutAccuracy
						+ cm.modStats.guardAccuracy
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

		for (int i = 0; i < teamList.Count; i++)
        {
			standDisplay[i].name.text = teamList[i].team.name;
            standDisplay[i].wins.text = teamList[i].team.wins.ToString();
            standDisplay[i].loss.text = teamList[i].team.loss.ToString();
            standDisplay[i].nextOpp.text = teamList[i].team.nextOpp;
            teamList[i].team.rank = i + 1;
        }

		vsDisplay[0].name.text = teams[playerTeam].name;
		vsDisplay[0].rank.text = teams[playerTeam].rank.ToString();

		for (int i = 0; i < teamList.Count; i++)
        {
			if (teams[playerTeam].name == teamList[i].team.name)
				standDisplay[i].panel.enabled = true;
			else
				standDisplay[i].panel.enabled = false;

			if (teams[playerTeam].nextOpp == teamList[i].team.name)
			{
				tempRank = i + 1;
				vsDisplay[1].name.text = teamList[i].team.name;
				vsDisplay[1].rank.text = teamList[i].team.rank.ToString();
			}
		}

		standScrollBar.value = (teams[playerTeam].rank - numberOfTeams) / (1f - numberOfTeams);
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
		
		// Determine if player won based on their team color
		if (teams[playerTeam].name == gsp.redTeamName)
		{
			playerWon = gsp.redScore > gsp.yellowScore;
		}
		else if (teams[playerTeam].name == gsp.yellowTeamName)
		{
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

		Debug.Log($"[TournyManager] Player match result: {teams[playerTeam].name} ({gsp.redScore}) vs {teams[oppTeam].name} ({gsp.yellowScore}) - Player won: {playerWon}");
		Debug.Log($"[TournyManager] {teams[playerTeam].name}: {teams[playerTeam].wins}W-{teams[playerTeam].loss}L, {teams[oppTeam].name}: {teams[oppTeam].wins}W-{teams[oppTeam].loss}L");
	}

    #region Set
    void SetDraw()
    {
		//Debug.Log("Setting Draw - " + draw);
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
				if (Random.Range(0, games[i].strength) > Random.Range(0, games[i + 1].strength))
				{
					games[i + 1].loss++;
                    games[i].wins++;

                    //if (games[i].name == teams[playerTeam].name)
                    //    gsp.record.x++;
                    //if (games[i + 1].name == teams[playerTeam].name)
                    //    gsp.record.y++;
                }
				else
				{
					games[i].loss++;
                    games[i + 1].wins++;

                    //if (games[i].name == teams[playerTeam].name)
                    //    gsp.record.y++;
                    //if (games[i + 1].name == teams[playerTeam].name)
                    //    gsp.record.x++;
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
		// Note: gsp.draw was already incremented by EndMenu.EndGame(), so we need to decrement it
		// to get back to the draw that was just played
		int tempDraw = draw - 1;
		Debug.Log("Temp Draw " + tempDraw);
		Team[] games = new Team[teams.Length];
		
		// Build games array from draw format
		// Note: EndMenu incremented gsp.draw, so draw-1 is the draw that was just played
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
                // Get the team indices from the draw format
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
		Debug.Log("Draw Scoring draw is " + draw);
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
		gsp.TournySetup();
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








