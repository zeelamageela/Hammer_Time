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

	// Start is called before the first frame update
	void Start()
	{
		cm = FindObjectOfType<CareerManager>();
		gsp = GameObject.Find("GameSettingsPersist").GetComponent<GameSettingsPersist>();
		//Debug.Log("Number of Teams at top of start - " + gsp.numberOfTeams);

		careerEarnings = gsp.earnings;

		//Debug.Log("Gsp In Progress is " + gsp.inProgress);
		//Debug.Log("Gsp Career Load is " + gsp.careerLoad);

		if (gsp.careerLoad)
		{
			//cm.LoadCareer();
			gsp.LoadCareer();
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
			CashGames cg = FindObjectOfType<CashGames>();
			numberOfTeams = gsp.numberOfTeams;
			prize = gsp.prize;
			careerEarningsText.text = "$ " + gsp.earnings.ToString();

			teams = new Team[numberOfTeams];

			teamList = new List<Team_List>();
			cg.SetUp();
        }
		else
		{
			numberOfTeams = gsp.numberOfTeams;
			prize = gsp.prize;
			careerEarningsText.text = "$ " + gsp.earnings.ToString();

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
		careerEarningsText.text = "$ " + gsp.earnings.ToString();
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
		cm = FindObjectOfType<CareerManager>();
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
				draw--;
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

				if (teams[playerTeam].name == gsp.redTeamName)
				{
					if (gsp.redScore > gsp.yellowScore)
					{
						teams[oppTeam].loss++;
						teams[playerTeam].wins++;
						//gsp.record.x++;
					}
					else
					{
						teams[oppTeam].wins++;
						teams[playerTeam].loss++;
						//gsp.record.y++;
					}
				}
				else
				{
					if (gsp.redScore < gsp.yellowScore)
					{
						teams[oppTeam].loss++;
						teams[playerTeam].wins++;
						//gsp.record.x++;
					}
					else
					{
						teams[oppTeam].wins++;
						teams[playerTeam].loss++;
						//gsp.record.y++;
					}
				}
				Debug.Log(teams[oppTeam].name + " " + teams[oppTeam].wins + " Wins");
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
				draw--;
				for (int i = 0; i < teams.Length; i++)
				{
					if (teams[i].name == gsp.playerTeam.nextOpp)
						oppTeam = i;
					if (teams[i].player)
						playerTeam = i;
				}

				Debug.Log("OppTeam is " + oppTeam);

				if (gsp.playerTeam.name == gsp.redTeamName)
				{
					if (gsp.redScore > gsp.yellowScore)
					{
						teams[oppTeam].loss++;
						teams[playerTeam].wins++;
						//gsp.record.x++;
					}
					else
					{
						teams[oppTeam].wins++;
						teams[playerTeam].loss++;
						//gsp.record.y++;
					}
				}
				else
				{
					if (gsp.redScore < gsp.yellowScore)
					{
						teams[oppTeam].loss++;
						teams[playerTeam].wins++;
						//gsp.record.x++;
					}
					else
					{
						teams[oppTeam].wins++;
						teams[playerTeam].loss++;
						//gsp.record.y++;
					}
				}
				Debug.Log(teams[oppTeam].name + " " + teams[oppTeam].wins + " Wins");
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
						+ cm.cStats.sweepCohesion;
					teams[i].strength = Mathf.RoundToInt(strength / 6f);
					playerTeam = i;
				}
				teamList.Add(new Team_List(teams[i]));
			}

			//teamList[playerTeam].team.name = gsp.teamName;
			teamList.Sort();
			yield return new WaitForEndOfFrame();
			SetDraw();
		}
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

		//cm.SaveCareer();
		StartCoroutine(SaveCareer());
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

		for (int i = 0; i < teams.Length; i++)
        {
			teams[i].record.x = teams[i].wins;
			teams[i].record.y = teams[i].loss;
        }

		Debug.Log("Tourny Record is " + teams[playerTeam].wins + " - " + teams[playerTeam].loss);
		draw++;
		//yield return new WaitForEndOfFrame();
		yield return StartCoroutine(DrawScoring());
	}

	IEnumerator SimRestDraw()
	{
		int tempDraw = draw - 1;
		Debug.Log("Temp Draw " + tempDraw);
		Team[] games = new Team[teams.Length];
		//SetDraw();
		for (int i = 0; i < teams.Length; i++)
		{
			if (i % 2 == 0)
				games[i] = teams[drawFormat[draw].game[i / 2].x];
			else
				games[i] = teams[drawFormat[draw].game[(i - 1) / 2].y];
		}
		yield return new WaitForEndOfFrame();
		for (int i = 0; i < games.Length; i++)
        {
			if (i % 2 == 0)
			{
				Debug.Log("Player Team is " + playerTeam);
				//Debug.Log("Settling Game - " + games[i].name);
				if (games[i].name == teams[playerTeam].name | games[i].name == teams[oppTeam].name)
                {
					Debug.Log("Player Game skip sim - " + i + " - " + games[i].name);
				}
				else if (Random.Range(0, games[i].strength) > Random.Range(0, games[i + 1].strength))
				{
					//if (i + 1 != playerTeam & i + 1 != oppTeam)
						games[i + 1].loss++;
					//if (i != playerTeam & i != oppTeam)
						games[i].wins++;
				}
				else
				{
					//if (i != playerTeam & i != oppTeam)
						games[i].loss++;
					//if (i + 1 != playerTeam & i + 1 != oppTeam)
						games[i + 1].wins++;
				}
			}
        }
		draw++;
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
			if (cm.currentTourny.qualifier)
			{
				vsTitle.text = "Results";
				if (teams[playerTeam].rank <= 4)
				{
					heading.text = "Qualified!";
					gsp.earnings += gsp.prize * 0.25f;
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
				pm.nextButton.gameObject.SetActive(true);
			}
			else
            {

				contButton.gameObject.SetActive(true);
				pm.nextButton.gameObject.SetActive(false);
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
		CareerManager cm = FindObjectOfType<CareerManager>();
		gsp = FindObjectOfType<GameSettingsPersist>();
		gsp.teams = teams;
		float winnings;
		if (gsp.cashGame)
			winnings = gsp.cash;
		else
			winnings = teams[playerTeam].earnings;

		if (gsp.cashGame == false)
		{
			Debug.Log("cm.teamRecords Length is " + cm.teamRecords.Length);

			for (int i = 0; i < teams.Length; i++)
			{
				teams[i].wins += (int)cm.teamRecords[i].x;
				teams[i].loss += (int)cm.teamRecords[i].y;
				teams[i].earnings += cm.teamRecords[i].z;
				teams[i].id = (int)cm.teamRecords[i].w;
			}

			gsp.earnings = teams[playerTeam].earnings;
			gsp.record = new Vector2(teams[playerTeam].wins, teams[playerTeam].loss);
		}

		//Debug.Log("PlayerTeam name is " + teams[playerTeam].name);
		Debug.Log("PlayerTeam record is " + gsp.record.x + " - " + gsp.record.y);

		cm.earnings = gsp.earnings;
		cm.cash += winnings;
        cm.record = gsp.record;
        gsp.draw = 0;
		gsp.playoffRound = 0;
		gsp.tournyInProgress = false;
		Debug.Log("gsp.inProgress is " + gsp.tournyInProgress);
		gsp.playoffTeams = null;
		Debug.Log("CM Record is " + cm.record.x + " - " + cm.record.y);
		Debug.Log("CM earnings are " + cm.earnings);
		
		cm.TournyResults();
		cm.SetUpCareer();
		SceneManager.LoadScene("Arena_Selector");
    }

	IEnumerator SaveCareer()
	{
		Debug.Log("Saving in TournyManager");
		myFile = new EasyFileSave("my_player_data");

		//Vector2 tempRecord = new Vector2(gsp.record.x, gsp.record.y);
		//inProgress = true;
		//myFile.Add("First Name", gsp.firstName);
		//myFile.Add("Team Name", gsp.teamName);
		//myFile.Add("Team Colour", cm.teamColour);
		//myFile.Add("Career Earnings", gsp.earnings);
		//Debug.Log("TM Career Earnings - " + gsp.earnings);
		gsp.loadGame = false;
		//myFile.Add("Tourny Record", gsp.record);
		//myFile.Add("Career Record", cm.record);
		myFile.Add("Tourny In Progress", true);
		gsp.tournyInProgress = true;
		gsp.gameInProgress = false;
		myFile.Add("Game Load", false);
		myFile.Add("Game In Progress", false);
		myFile.Add("Draw", draw);
		myFile.Add("Number Of Teams", numberOfTeams);
		myFile.Add("Prize", prize);
		myFile.Add("Rocks", gsp.rocks);
		myFile.Add("Ends", gsp.ends);
		myFile.Add("Games", gsp.games);
		//myFile.Add("Player Team", playerTeam);
		myFile.Add("OppTeam", oppTeam);
		myFile.Add("Playoff Round", playoffRound);
		myFile.Add("BG", gsp.bg);
		string[] nameList = new string[teams.Length];
        int[] winsList = new int[teams.Length];
        int[] lossList = new int[teams.Length];
        int[] rankList = new int[teams.Length];
        string[] nextOppList = new string[teams.Length];
        int[] strengthList = new int[teams.Length];
        int[] idList = new int[teams.Length];
		bool[] playerList = new bool[teams.Length];
		float[] earningsList = new float[teams.Length];

        for (int i = 0; i < teams.Length; i++)
        {
			nameList[i] = teams[i].name;
            winsList[i] = teams[i].wins;
            lossList[i] = teams[i].loss;
            rankList[i] = teams[i].rank;
            nextOppList[i] = teams[i].nextOpp;
            strengthList[i] = teams[i].strength;
            idList[i] = teams[i].id;
			playerList[i] = teams[i].player;
			earningsList[i] = teams[i].earnings;
			//Debug.Log("Tourny Id List - " + idList[i]);
        }

		myFile.Add("Tourny Name List", nameList);
        myFile.Add("Tourny Wins List", winsList);
        myFile.Add("Tourny Loss List", lossList);
        myFile.Add("Tourny Rank List", rankList);
        myFile.Add("Tourny NextOpp List", nextOppList);
        myFile.Add("Tourny Strength List", strengthList);
        myFile.Add("Tourny Team ID List", idList);
		myFile.Add("Tourny Earnings List", earningsList);
		myFile.Add("Tourny Player List", playerList);
		//yield return myFile.TestDataSaveLoad();


		int[] tempTRX = new int[cm.teamRecords.Length];
		int[] tempTRY = new int[cm.teamRecords.Length];
		float[] tempTRZ = new float[cm.teamRecords.Length];
		int[] tempTRW = new int[cm.teamRecords.Length];

		for (int i = 0; i < cm.teamRecords.Length; i++)
		{
			tempTRX[i] = (int)cm.teamRecords[i].x;
			tempTRY[i] = (int)cm.teamRecords[i].y;
			tempTRZ[i] = cm.teamRecords[i].z;
			tempTRW[i] = (int)cm.teamRecords[i].w;
		}
		myFile.Add("Team Records X", tempTRX);
		myFile.Add("Team Records Y", tempTRY);
		myFile.Add("Team Records Z", tempTRZ);
		myFile.Add("Team Records W", tempTRW);

		yield return myFile.Append();


		//cm.SaveCareer();
	}
}
