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
		Debug.Log("Number of Teams at top of start - " + gsp.numberOfTeams);

		careerEarnings = gsp.earnings;

		Debug.Log("Gsp In Progress is " + gsp.inProgress);
		Debug.Log("Gsp Career Load is " + gsp.careerLoad);

		if (gsp.careerLoad)
		{
			//cm.LoadCareer();
			gsp.LoadCareer();
            if (gsp.inProgress)
            {
                Debug.Log("In Progress is True");
                gsp.LoadTourny();
                playoffRound--;
				Debug.Log("Playoff Round is " + playoffRound);
            }
            else
            {
                Debug.Log("In Progress is False");
                gsp.draw = draw;
                gsp.playoffRound = 0;
            }
        }


        numberOfTeams = gsp.numberOfTeams;
		prize = gsp.prize;
		careerEarningsText.text = "$ " + gsp.earnings.ToString();

		teams = new Team[numberOfTeams];

		teamList = new List<Team_List>();

		standDisplay = new StandingDisplay[teams.Length];

		StartCoroutine(SetupStandings());

        Debug.Log("Draw at top of start - " + gsp.draw);
		
		//PrintRows(teams);
	}

	public void ClearMoney()
	{
		myFile = new EasyFileSave("my_player_data");

		if (myFile.Load())
		{
			myFile.Dispose();
		}
		careerEarnings = 0;
		myFile.Add("Career Earnings", gsp.earnings);
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

		//for (int i = 0; i < vsDisplay.Length; i++)
		//{
		//	vsDisplay[i].name.gameObject.GetComponent<ContentSizeFitter>().enabled = false;

		//	yield return new WaitForEndOfFrame();
		//	vsDisplay[i].name.gameObject.GetComponent<ContentSizeFitter>().enabled = true;
		//}
    }

	IEnumerator SetupStandings()
	{
		cm = FindObjectOfType<CareerManager>();
		//yield return new WaitUntil(() => teams.Length >= numberOfTeams);
		row = new GameObject[teams.Length];
		Debug.Log("Setup Stand Team Length is " + teams.Length);
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
			else if (gsp.inProgress)
			{
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
						gsp.record.x++;
					}
					else
					{
						teams[oppTeam].wins++;
						teams[playerTeam].loss++;
						gsp.record.y++;
					}
				}
				else
				{
					if (gsp.redScore < gsp.yellowScore)
					{
						teams[oppTeam].loss++;
						teams[playerTeam].wins++;
						gsp.record.x++;
					}
					else
					{
						teams[oppTeam].wins++;
						teams[playerTeam].loss++;
						gsp.record.y++;
					}
				}
				Debug.Log(teams[oppTeam].name + " " + teams[oppTeam].wins + " Wins");
				StartCoroutine(SimRestDraw());
			}

		}
		else
        {
			for (int i = 0; i < teams.Length; i++)
			{
				teams[i] = gsp.teams[i];
				teams[i].wins = 0;
				teams[i].loss = 0;
				//teams[i].earnings = 0;
				//teams[i].tourPoints = 0;
				//teams[i].tourRecord = Vector2.zero;
			}

			for (int i = 0; i < teams.Length; i++)
			{
				teams[i].strength = Random.Range(0, 10);
				if (teams[i].name == gsp.teamName)
				{
					playerTeam = i;
				}
				teamList.Add(new Team_List(teams[i]));
			}

			//teamList[playerTeam].team.name = gsp.teamName;
			teamList.Sort();
			yield return new WaitForEndOfFrame();
			SetDraw();
		}
		//else
		//{
		//	Shuffle(tTeamList.teams);

		//	for (int i = 0; i < teams.Length; i++)
  //          {
		//		teams[i] = tTeamList.teams[i];
  //          }

		//	Shuffle(teams);

		//	for (int i = 0; i < teams.Length; i++)
		//	{
		//		teamList.Add(new Team_List(teams[i]));
		//		teams[i].strength = Random.Range(0, 10);
		//	}

		//	playerTeam = Random.Range(0, teams.Length);
		//	teamList[playerTeam].team.name = gsp.teamName;

		//	yield return new WaitForEndOfFrame();
		//	SetDraw();
		//}

		//yield return new WaitUntil( () => standDisplay.Length == teams.Length);
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
		Debug.Log("Setting Draw - " + draw);
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

                    if (games[i].name == teams[playerTeam].name)
                        gsp.record.x++;
                    if (games[i + 1].name == teams[playerTeam].name)
                        gsp.record.y++;
                }
				else
				{
					games[i].loss++;
                    games[i + 1].wins++;

                    if (games[i].name == teams[playerTeam].name)
                        gsp.record.y++;
                    if (games[i + 1].name == teams[playerTeam].name)
                        gsp.record.x++;
                }
			}
		}

		Debug.Log("Career Record is " + gsp.record.x + " - " + gsp.record.y);
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
			playButton.gameObject.SetActive(false);
			simButton.gameObject.SetActive(false);
			contButton.gameObject.SetActive(true);
			for (int i = 0; i < teams.Length; i++)
            {
				teams[i].nextOpp = "-----";
            }

		}
		else
			heading.text = "End of Round Robin";
	}
	#endregion

	public void OnSim()
	{
		//playoffRound = pm.playoffRound;
		if (playoffRound > 0)
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

		gsp.teams = teams;
		float winnings = cm.cash + (gsp.earnings - cm.earnings);
        cm.earnings = gsp.earnings;
		cm.cash = winnings;
        cm.record = gsp.record;
        gsp.draw = 0;
		gsp.playoffRound = 0;
		gsp.inProgress = false;
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
		myFile.Add("Career Earnings", gsp.earnings);
		Debug.Log("TM Career Earnings - " + gsp.earnings);
		myFile.Add("Career Record", gsp.record);
		myFile.Add("Tourny In Progress", true);
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
			//Debug.Log("Tourny Id List - " + idList[i]);
        }

		myFile.Add("Tourny Name List", nameList);
        myFile.Add("Tourny Wins List", winsList);
        myFile.Add("Tourny Loss List", lossList);
        myFile.Add("Tourny Rank List", rankList);
        myFile.Add("Tourny NextOpp List", nextOppList);
        myFile.Add("Tourny Strength List", strengthList);
        myFile.Add("Tourny Team ID List", idList);
		myFile.Add("Tourny Player List", playerList);
        //yield return myFile.TestDataSaveLoad();
        yield return myFile.Append();


		//cm.SaveCareer();
	}
}
