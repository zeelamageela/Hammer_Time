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
	public int draw;
	public int playoffRound;
	public int playerTeam;
	public int oppTeam;
	public bool inProgress;
	
	public Vector2 careerRecord;
	public float careerEarnings;
	string teamName;

	// Start is called before the first frame update
	void Start()
	{
		gsp = GameObject.Find("GameSettingsPersist").GetComponent<GameSettingsPersist>();
		myFile = new EasyFileSave("my_player_data");

        if (myFile.Load())
        {
            careerEarnings = myFile.GetFloat("Career Earnings");
			careerRecord = myFile.GetUnityVector2("Career Record");

			bool inProgress = myFile.GetBool("In Progress");

			myFile.Dispose();
			if (inProgress)
            {
				Debug.Log("In Progress is True");
				if (gsp.careerLoad)
					gsp.LoadTourny();
            }
			else
			{
				gsp.draw = 0;
				gsp.playoffRound = 0;
			}

        }

		numberOfTeams = gsp.numberOfTeams;
		careerEarningsText.text = "$ " + careerEarnings.ToString();
		teams = new Team[numberOfTeams];

		teamList = new List<Team_List>();

		standDisplay = new StandingDisplay[teams.Length];

		StartCoroutine(SetupStandings());

        Debug.Log("Draw at top of start - " + gsp.draw);
		
		//PrintRows(teams);
	}

	public void ClearMoney()
    {
		if (myFile.Load())
		{
			myFile.Dispose();
		}
		careerEarnings = 0;
		myFile.Add("Career Earnings", careerEarnings);
		myFile.Save();
		careerEarningsText.text = "$ " + careerEarnings.ToString();

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

		for (int i = 0; i < vsDisplay.Length; i++)
		{
			vsDisplay[i].name.gameObject.GetComponent<ContentSizeFitter>().enabled = false;

			yield return new WaitForEndOfFrame();
			vsDisplay[i].name.gameObject.GetComponent<ContentSizeFitter>().enabled = true;
		}
    }

	IEnumerator SetupStandings()
    {
		//yield return new WaitUntil(() => teams.Length >= numberOfTeams);
		row = new GameObject[teams.Length];
		Debug.Log("Team Length is " + teams.Length);
		dfList.DrawSelector(teams.Length);

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

			if (playoffRound > 0)
			{
				pm.enabled = true;
				standings.SetActive(false);
			}
			else if (inProgress)
            {
				for (int i = 0; i < teams.Length; i++)
				{
					if (teams[i].name == gsp.playerTeam.nextOpp)
						oppTeam = i;
					if (teams[i].name == gsp.playerTeam.name)
						playerTeam = i;
				}

				SetDraw();
            }
			else
			{
				draw--;
				for (int i = 0; i < teams.Length; i++)
				{
					if (teams[i].name == gsp.playerTeam.nextOpp)
						oppTeam = i;
					if (teams[i].name == gsp.playerTeam.name)
						playerTeam = i;
				}

				Debug.Log("OppTeam is " + oppTeam);

				if (gsp.playerTeam.name == gsp.redTeamName)
				{
					if (gsp.redScore > gsp.yellowScore)
					{
						teams[oppTeam].loss++;
						teams[playerTeam].wins++;
					}
					else
					{
						teams[oppTeam].wins++;
						teams[playerTeam].loss++;
					}
				}
				else
				{
					if (gsp.redScore < gsp.yellowScore)
					{
						teams[oppTeam].loss++;
						teams[playerTeam].wins++;
					}
					else
					{
						teams[oppTeam].wins++;
						teams[playerTeam].loss++;
					}
				}
				Debug.Log(teams[oppTeam].name + " " + teams[oppTeam].wins + " Wins");
				StartCoroutine(SimRestDraw());
			}

		}

		else
		{
			Shuffle(tTeamList.teams);

			for (int i = 0; i < teams.Length; i++)
            {
				teams[i] = tTeamList.teams[i];
            }

			Shuffle(teams);

			for (int i = 0; i < teams.Length; i++)
			{
				teamList.Add(new Team_List(teams[i]));
				teams[i].strength = Random.Range(0, 10);
			}

			playerTeam = Random.Range(0, teams.Length);
			teamList[playerTeam].team.name = gsp.teamName;

			yield return new WaitForEndOfFrame();
			SetDraw();
		}

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
			if (teams[playerTeam].nextOpp == teamList[i].team.name)
			{
				tempRank = i + 1;
				vsDisplay[1].name.text = teamList[i].team.name;
				vsDisplay[1].rank.text = tempRank.ToString();
			}
        }
		StartCoroutine(RefreshPanel());

		StartCoroutine(SaveCareer());
    }

    #region Set
    void SetDraw()
    {
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

		scrollBar.value = (teams[playerTeam].rank - 16f) / 15f;
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

		//games[0] = teams[drawFormat[draw].game[0].x];

		//games[1] = teams[drawFormat[draw].game[0].y];

		//games[2] = teams[drawFormat[draw].game[1].x];
		//games[3] = teams[drawFormat[draw].game[1].y];

		//games[4] = teams[drawFormat[draw].game[2].x];
		//games[5] = teams[drawFormat[draw].game[2].y];

		for (int i = 0; i < games.Length; i++)
		{
			if (i % 2 == 0)
			{
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
		
		draw++;
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
		StartCoroutine(SaveCareer());

		SceneManager.LoadScene("SplashMenu");
    }

	IEnumerator SaveCareer()
	{
		myFile = new EasyFileSave("my_player_data");

		//Vector2 tempRecord = new Vector2(careerRecord.x, careerRecord.y);
		myFile.Add("First Name", gsp.firstName);
		myFile.Add("Team Name", gsp.teamName);
		myFile.Add("Team Colour", gsp.teamColour);
		myFile.Add("Career Earnings", careerEarnings);
		myFile.Add("Career Record", careerRecord);
		myFile.Add("In Progress", true);
		myFile.Add("Draw", draw);
		myFile.Add("Number Of Teams", numberOfTeams);
		myFile.Add("Player Team", playerTeam);
		myFile.Add("OppTeam", oppTeam);
		myFile.Add("Playoff Round", playoffRound);

		string[] nameList = new string[teams.Length];
        int[] winsList = new int[teams.Length];
        int[] lossList = new int[teams.Length];
        int[] rankList = new int[teams.Length];
        string[] nextOppList = new string[teams.Length];
        int[] strengthList = new int[teams.Length];
        int[] idList = new int[teams.Length];

        for (int i = 0; i < teams.Length; i++)
        {
			nameList[i] = teams[i].name;
            winsList[i] = teams[i].wins;
            lossList[i] = teams[i].loss;
            rankList[i] = teams[i].rank;
            nextOppList[i] = teams[i].nextOpp;
            strengthList[i] = teams[i].strength;
            idList[i] = teams[i].id;
        }
		myFile.Add("Teams Name", nameList);
        myFile.Add("Teams Wins", winsList);
        myFile.Add("Teams Loss", lossList);
        myFile.Add("Teams Rank", rankList);
        myFile.Add("Teams NextOpp", nextOppList);
        myFile.Add("Teams Strength", strengthList);
        myFile.Add("Teams ID", idList);

		int[] playoffIDList = new int[playoffTeams.Length];

		for (int i = 0; i < playoffTeams.Length; i++)
        {
			playoffIDList[i] = playoffTeams[i].id;
        }

		myFile.Add("Playoff ID List", playoffIDList);
        //yield return myFile.TestDataSaveLoad();
        yield return myFile.Save();
    }
}
