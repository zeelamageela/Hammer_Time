using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TigerForge;

public class PlayoffManager_TripleK : MonoBehaviour
{
	public TournyManager tm;
	public Team[] teams;

	public GameObject winnersBracket;
	public GameObject losersBracket1;
	public GameObject losersBracket2;
	public GameObject finalsBracket;

	public BracketDisplay[] winnersDisplay1;
	public BracketDisplay[] winnersDisplay3;
	public BracketDisplay[] winnersDisplay7;
	public BracketDisplay[] winnersDisplay12;

	public BracketDisplay[] losersDisplayA2;
	public BracketDisplay[] losersDisplayA4;
	public BracketDisplay[] losersDisplayA8;
	public BracketDisplay[] losersDisplayA9;
	public BracketDisplay[] losersDisplayA13;

	public BracketDisplay[] losersDisplayB5;
	public BracketDisplay[] losersDisplayB6;
	public BracketDisplay[] losersDisplayB10;
	public BracketDisplay[] losersDisplayB11;
	public BracketDisplay[] losersDisplayB14;

	public BracketDisplay[] finalsDisplay15;
	public BracketDisplay[] finalsDisplay16;
	public BracketDisplay[] finalsDisplay17;
	public BracketDisplay[] finalsDisplay18;
	public BracketDisplay[] finalsDisplay19;
	public BracketDisplay[] finalsDisplay20;

	public BracketDisplay[] brackDisplay;

	public Vector2[] gameList;
	public GameObject[] row;
	public GameObject playoffs;
	public Button nextButton;
	public Button simButton;
	public Button contButton;
	public Button playButton;
	public Text heading;
	public Scrollbar horizScrollBar;
	public Scrollbar vertScrollBar;
	public Text careerEarningsText;

	GameSettingsPersist gsp;

	EasyFileSave myFile;
	int pTeams;
	public int playerTeam;
	public int oppTeam;
	public int playoffRound;

	public float careerEarnings;
	public Vector2 careerRecord;

	private void Start()
	{
		gsp = FindObjectOfType<GameSettingsPersist>();

		myFile = new EasyFileSave("my_player_data");

		//StartCoroutine(LoadCareer());
		Debug.Log("Career Earnings before playoffs - $ " + gsp.earnings.ToString());

		playoffs.SetActive(true);
		if (gsp.careerLoad)
		{
			gsp.LoadCareer();
			careerEarnings = gsp.earnings;
			careerRecord = gsp.record;
		}
		else if (gsp.inProgress)
		{
			gsp.LoadTourny();
			gsp.inProgress = false;
			gsp.careerLoad = false;
			Debug.Log("Playoff Round BEFORE the minus - " + gsp.playoffRound);
			//gsp.playoffRound--;
			Debug.Log("Playoff Round AFTER the minus - " + gsp.playoffRound);
		}

		//playerTeam = tm.playerTeam;
		playoffRound = gsp.playoffRound;
		teams = new Team[16];
		gameList = new Vector2[45];
		
		Debug.Log("Career Earnings before playoffs - $ " + gsp.earnings.ToString());

		if (playoffRound > 0)
			LoadPlayoffs();
		else
			SetSeeding();

		//SetBrackets();
	}

	public void SetSeeding()
	{
		TournyTeamList tTeamList = FindObjectOfType<TournyTeamList>();
		pTeams = 4;
		//teams = new Team[9];
		heading.text = "Page Playoff";

		playoffRound++;

		for (int i = 0; i < teams.Length; i++)
		{
			teams[i] = tTeamList.teams[i];
		}

		for (int i = 0; i < teams.Length; i++)
		{
			if (i % 2 == 0)
			{
				gameList[i / 2] = new Vector2(teams[i].id, teams[i + 1].id);
				teams[i].strength = Random.Range(8, 10);
			}
			else
				teams[i].strength = Random.Range(0, 2);
		}

		SetPlayoffs();
	}

	IEnumerator RefreshPlayoffPanel()
	{
		for (int i = 0; i < winnersBracket.transform.childCount; i++)
		{
			for (int j = 0; j < winnersBracket.transform.GetChild(i).childCount; j++)
			{
				for (int k = 0; k < 2; k++)
				{
					winnersBracket.transform.GetChild(i).GetChild(j).GetChild(k).GetComponent<ContentSizeFitter>().enabled = false;
					yield return new WaitForEndOfFrame();
					winnersBracket.transform.GetChild(i).GetChild(j).GetChild(k).GetComponent<ContentSizeFitter>().enabled = true;
				}
			}
		}

		for (int i = 0; i < losersBracket1.transform.childCount; i++)
		{
			for (int j = 0; j < losersBracket1.transform.GetChild(i).childCount; j++)
			{
				for (int k = 0; j < 2; j++)
				{
					losersBracket1.transform.GetChild(i).GetChild(j).GetChild(k).GetComponent<ContentSizeFitter>().enabled = false;
					yield return new WaitForEndOfFrame();
					losersBracket1.transform.GetChild(i).GetChild(j).GetChild(k).GetComponent<ContentSizeFitter>().enabled = true;
				}
			}
		}

		for (int i = 0; i < losersBracket2.transform.childCount; i++)
		{
			for (int j = 0; j < losersBracket2.transform.GetChild(i).childCount; j++)
			{
				for (int k = 0; k < 2; k++)
				{
					losersBracket2.transform.GetChild(i).GetChild(j).GetChild(k).GetComponent<ContentSizeFitter>().enabled = false;
					yield return new WaitForEndOfFrame();
					losersBracket2.transform.GetChild(i).GetChild(j).GetChild(k).GetComponent<ContentSizeFitter>().enabled = true;
				}
			}
		}

		for (int i = 0; i < finalsBracket.transform.childCount; i++)
		{
			for (int j = 0; j < finalsBracket.transform.GetChild(i).childCount; j++)
			{
				for (int k = 0; k < 2; k++)
				{
					finalsBracket.transform.GetChild(i).GetChild(j).GetChild(k).GetComponent<ContentSizeFitter>().enabled = false;
					yield return new WaitForEndOfFrame();
					finalsBracket.transform.GetChild(i).GetChild(j).GetChild(k).GetComponent<ContentSizeFitter>().enabled = true;
				}
			}
		}

		//for (int i = 0; i < tm.vsDisplay.Length; i++)
		//{
		//	tm.vsDisplay[i].name.gameObject.GetComponent<ContentSizeFitter>().enabled = false;

		//	yield return new WaitForEndOfFrame();
		//	tm.vsDisplay[i].name.gameObject.GetComponent<ContentSizeFitter>().enabled = true;
		//}
	}

	void LoadPlayoffs()
	{
		Debug.Log("Load Playoffs - Round " + playoffRound);
		for (int i = 0; i < teams.Length; i++)
		{
			teams[i] = gsp.teams[i];
		}
		//teams = gsp.teams;

		for (int i = 0; i < tm.teams.Length; i++)
		{
			if (tm.teams[i].id == gsp.playerTeam.id)
				playerTeam = i;
			if (tm.teams[i].name == gsp.playerTeam.nextOpp)
				oppTeam = i;
		}

		Debug.Log("OppTeam is " + oppTeam);
		switch (playoffRound)
		{
			case 1:
				bool game1 = false;
				bool game2 = false;

				for (int i = 0; i < 4; i++)
				{
					if (tm.teams[playerTeam] == teams[i])
					{
						if (i < 2)
							game1 = true;
						else
							game2 = true;
					}
				}

				if (game1)
				{
					Debug.Log("Game 1 - " + gsp.redTeamName + " - " + gsp.redScore + " vs " + gsp.yellowTeamName + " - " + gsp.yellowScore);
					if (gsp.playerTeam.name == gsp.redTeamName)
					{
						if (gsp.redScore > gsp.yellowScore)
						{
							Debug.Log("Red beat Yellow");
							teams[4] = tm.teams[playerTeam];
							teams[5] = tm.teams[oppTeam];
						}
						else
						{
							Debug.Log("Yellow beat Red");
							teams[5] = tm.teams[playerTeam];
							teams[4] = tm.teams[oppTeam];
						}
					}
					else
					{
						if (gsp.redScore < gsp.yellowScore)
						{
							Debug.Log("Yellow beat Red");
							teams[4] = tm.teams[playerTeam];
							teams[5] = tm.teams[oppTeam];
						}
						else
						{
							Debug.Log("Red beat Yellow");
							teams[5] = tm.teams[playerTeam];
							teams[4] = tm.teams[oppTeam];
						}
					}
				}

				if (game2)
				{
					if (gsp.playerTeam.name == gsp.redTeamName)
					{
						if (gsp.redScore > gsp.yellowScore)
						{
							teams[6] = tm.teams[playerTeam];
						}
						else
						{
							teams[6] = tm.teams[oppTeam];
						}
					}
					else
					{
						if (gsp.redScore < gsp.yellowScore)
						{
							teams[6] = tm.teams[playerTeam];
						}
						else
						{
							teams[6] = tm.teams[oppTeam];
						}
					}
				}

				StartCoroutine(SimPlayoff(game1, game2));
				break;

			case 2:

				if (gsp.playerTeam.name == gsp.redTeamName)
				{
					if (gsp.redScore > gsp.yellowScore)
					{
						teams[7] = tm.teams[playerTeam];
					}
					else
					{
						teams[7] = tm.teams[oppTeam];
					}
				}
				else
				{
					if (gsp.redScore < gsp.yellowScore)
					{
						teams[7] = tm.teams[playerTeam];
					}
					else
					{
						teams[7] = tm.teams[oppTeam];
					}
				}
				playoffRound++;
				break;

			case 3:

				if (gsp.playerTeam.name == gsp.redTeamName)
				{
					if (gsp.redScore > gsp.yellowScore)
					{
						teams[8] = tm.teams[playerTeam];
					}
					else
					{
						teams[8] = tm.teams[oppTeam];
					}
				}
				else
				{
					if (gsp.redScore < gsp.yellowScore)
					{
						teams[8] = tm.teams[playerTeam];
					}
					else
					{
						teams[8] = tm.teams[oppTeam];
					}
				}
				playoffRound++;
				break;
		}

		SetPlayoffs();
	}

	public void SetPlayoffs()
	{
		Debug.Log("Set Playoffs 3K - Round " + playoffRound);
		switch (playoffRound)
		{
			case 1:
				heading.text = "Round 1";

				for (int i = 0; i < teams.Length; i++)
				{
					winnersDisplay1[i].name.text = teams[i].name;
					winnersDisplay1[i].rank.text = teams[i].wins.ToString() + " " + teams[i].loss.ToString();
				}

				winnersBracket.transform.GetChild(0).gameObject.SetActive(true);

				for (int i = 1; i < winnersBracket.transform.childCount; i++)
                {
					winnersBracket.transform.GetChild(i).gameObject.SetActive(false);
                }
				winnersBracket.SetActive(true);
				losersBracket1.SetActive(false);
				losersBracket2.SetActive(false);
				finalsBracket.SetActive(false);

				StartCoroutine(RefreshPlayoffPanel());

				playoffs.SetActive(true);

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				horizScrollBar.value = 0;
				vertScrollBar.value = 0;
				//StartCoroutine(SaveCareer(true));
				break;

			case 2:
				heading.text = "Round 2";

				for (int i = 0; i < losersDisplayA2.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							for (int k = 0; )
							if (teams[j].id == gameList[i / 2].x)
                            {
								losersDisplayA2[i].name.text = teams[j].name;
								losersDisplayA2[i].rank.text = teams[j].wins.ToString() + " " + teams[j].loss.ToString();
							}
						}
					}
					else
                    {
						for (int k = 0; k < teams.Length; k++)
                        {
							if (teams[k].id == gameList[(i - 1) / 2].y)
							{
								losersDisplayA2[i].name.text = teams[k].name;
								losersDisplayA2[i].rank.text = teams[k].wins.ToString() + " " + teams[k].loss.ToString();
							}
                        }
                    }
				}

				for (int i = 0; i < losersBracket1.transform.childCount; i++)
				{
					if (i == 0)
						losersBracket1.transform.GetChild(i).gameObject.SetActive(true);
					else
						losersBracket1.transform.GetChild(i).gameObject.SetActive(false);
				}

				winnersBracket.SetActive(false);
				losersBracket1.SetActive(true);
				losersBracket2.SetActive(false);
				finalsBracket.SetActive(false);

				StartCoroutine(RefreshPlayoffPanel());

				playoffs.SetActive(true);

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				horizScrollBar.value = 0;
				vertScrollBar.value = 0;
				//StartCoroutine(SaveCareer(true));
				break;

			case 3:
				heading.text = "Round 3";

				for (int i = 0; i < losersDisplayA2.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[i / 2].x)
							{
								winnersDisplay3[i].name.text = teams[j].name;
								winnersDisplay3[i].rank.text = teams[j].wins.ToString() + " " + teams[j].loss.ToString();
							}
						}
					}
					else
					{
						for (int k = 0; k < teams.Length; k++)
						{
							if (teams[k].id == gameList[(i - 1) / 2].y)
							{
								winnersDisplay3[i].name.text = teams[k].name;
								winnersDisplay3[i].rank.text = teams[k].wins.ToString() + " " + teams[k].loss.ToString();
							}
						}
					}
				}

				for (int i = 0; i < losersBracket1.transform.childCount; i++)
				{
					if (i == 1)
						winnersBracket.transform.GetChild(i).gameObject.SetActive(true);
					else
						winnersBracket.transform.GetChild(i).gameObject.SetActive(false);
				}

				winnersBracket.SetActive(false);
				losersBracket1.SetActive(true);
				losersBracket2.SetActive(false);
				finalsBracket.SetActive(false);

				StartCoroutine(RefreshPlayoffPanel());

				playoffs.SetActive(true);

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				horizScrollBar.value = 0;
				vertScrollBar.value = 0;
				//StartCoroutine(SaveCareer(true));
				break;

			case 4:
				for (int i = 0; i < 9; i++)
				{
					brackDisplay[i].name.text = teams[i].name;
					brackDisplay[i].rank.text = teams[i].rank.ToString();
					//brackDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[i].SetActive(true);
				}

				playoffs.SetActive(true);
				StartCoroutine(RefreshPlayoffPanel());

				if (tm.teams[playerTeam].name == teams[8].name)
				{
					heading.text = "You Win!";

					gsp.earnings += gsp.prize * 0.5f;
					//tm.teams[playerTeam].earnings = gsp.prize * 0.5f;
					tm.teams[playerTeam].rank = 1;
					tm.vs.SetActive(true);

					tm.vsDisplay[0].name.text = tm.teams[playerTeam].name;
					tm.vsDisplay[0].rank.text = tm.teams[playerTeam].rank.ToString();
					tm.vsDisplay[1].name.text = "$" + (gsp.prize * 0.5f).ToString();
					tm.vsDisplay[1].rank.gameObject.transform.parent.gameObject.SetActive(false);
				}
				else if (tm.teams[playerTeam].name == teams[4].name | tm.teams[playerTeam].name == teams[7].name)
				{
					heading.text = "Runner-up";
					gsp.earnings += gsp.prize * 0.25f;
					//tm.teams[playerTeam].earnings = gsp.prize * 0.25f;
					tm.teams[playerTeam].rank = 2;
					tm.vs.SetActive(true);

					tm.vsDisplay[0].name.text = tm.teams[playerTeam].name;
					tm.vsDisplay[0].rank.text = tm.teams[playerTeam].rank.ToString();
					tm.vsDisplay[1].name.text = "$" + (gsp.prize * 0.25f).ToString();
					tm.vsDisplay[1].rank.gameObject.transform.parent.gameObject.SetActive(false);
				}
				else if (tm.teams[playerTeam].name == teams[5].name | tm.teams[playerTeam].name == teams[6].name)
				{
					heading.text = "3rd Place";
					gsp.earnings += gsp.prize * 0.15f;
					//tm.teams[playerTeam].earnings = gsp.prize * 0.15f;
					tm.teams[playerTeam].rank = 3;
					tm.vs.SetActive(true);

					tm.vsDisplay[0].name.text = tm.teams[playerTeam].name;
					tm.vsDisplay[0].rank.text = tm.teams[playerTeam].rank.ToString();
					tm.vsDisplay[1].name.text = "$" + (gsp.prize * 0.15f).ToString();
					tm.vsDisplay[1].rank.gameObject.transform.parent.gameObject.SetActive(false);
				}
				else if (tm.teams[playerTeam].name == teams[2].name | tm.teams[playerTeam].name == teams[3].name)
				{
					heading.text = "4th Place";
					gsp.earnings += gsp.prize * 0.075f;
					//tm.teams[playerTeam].earnings = gsp.prize * 0.075f;
					tm.teams[playerTeam].rank = 4;

					tm.vs.SetActive(true);

					tm.vsDisplay[0].name.text = tm.teams[playerTeam].name;
					tm.vsDisplay[0].rank.text = tm.teams[playerTeam].rank.ToString();
					tm.vsDisplay[1].name.text = "$" + (gsp.prize * 0.075f).ToString();
					tm.vsDisplay[1].rank.gameObject.transform.parent.gameObject.SetActive(false);
				}
				else
				{
					for (int i = 4; i < tm.teamList.Count; i++)
					{
						float p = 1.4f;
						float totalTeams = tm.teamList.Count - 4;
						float prizePayout = ((Mathf.Pow(p, totalTeams - (i + 1))) / (Mathf.Pow(p, totalTeams) - 1f)) * (gsp.prize * 0.15f) * (p - 1);

						Debug.Log("Position " + (i + 1) + " Payout is $" + prizePayout);
						if (tm.teams[playerTeam].name == tm.teamList[i].team.name)
						{
							if (i > 3)
							{
								heading.text = (i + 1) + "th Place";
								tm.teamList[i].team.rank = i + 1;
								//tm.teamList[i].team.earnings += Mathf.RoundToInt(prizePayout);
							}
							//float prizePayout = (totalTeams - i) / (totalTeams);
							//float prizePayout = ((1 - p) / Mathf.Pow(1 - p, totalTeams) * Mathf.Pow(p, (i - 1))) * 10000f;
							//float prizePayout = ((Mathf.Pow(p, totalTeams - (i + 1))) / (Mathf.Pow(p, totalTeams) - 1f)) * 10000f * (p - 1);

							Debug.Log("Prize Payout is " + prizePayout);
							prizePayout = Mathf.RoundToInt(prizePayout);
							gsp.earnings += prizePayout;

							tm.vs.SetActive(true);

							tm.vsDisplay[0].name.text = tm.teams[playerTeam].name;
							tm.vsDisplay[0].rank.text = tm.teams[playerTeam].rank.ToString();
							tm.vsDisplay[1].name.text = "$" + prizePayout.ToString();
							tm.vsDisplay[1].rank.text = "";
						}
					}
					tm.vsDisplay[1].rank.gameObject.transform.parent.gameObject.SetActive(false);
				}

				for (int i = 0; i < tm.teamList.Count; i++)
				{
					float p = 1.4f;
					float totalTeams = tm.teamList.Count - 4;
					float prizePayout = ((Mathf.Pow(p, totalTeams - (i + 1))) / (Mathf.Pow(p, totalTeams) - 1f)) * (gsp.prize * 0.15f) * (p - 1);

					if (tm.teamList[i].team.id == teams[8].id)
					{
						tm.teamList[i].team.earnings += gsp.prize * 0.5f;
						tm.teamList[i].team.rank = 1;
					}
					else if (tm.teamList[i].team.id == teams[4].id | tm.teamList[i].team.id == teams[7].id)
					{
						tm.teamList[i].team.earnings += gsp.prize * 0.25f;
						tm.teamList[i].team.rank = 2;
					}
					else if (tm.teamList[i].team.id == teams[5].id | tm.teamList[i].team.id == teams[6].id)
					{
						tm.teamList[i].team.earnings += gsp.prize * 0.15f;
						tm.teamList[i].team.rank = 3;
					}
					else if (tm.teamList[i].team.id == teams[2].id | tm.teamList[i].team.id == teams[3].id)
					{
						tm.teamList[i].team.earnings += gsp.prize * 0.075f;
						tm.teamList[i].team.rank = 4;
					}
					else
					{
						tm.teamList[i].team.earnings += Mathf.RoundToInt(prizePayout);
						Debug.Log("Position " + (i + 1) + " Payout is $" + prizePayout);
					}

					Debug.Log("Prize Payout multiplier is " + prizePayout);
					//prizePayout = Mathf.RoundToInt(prizePayout);
					//tm.teamList[i].team.earnings += prizePayout;

				}
				Debug.Log("Career Earnings after calculation - " + gsp.earnings.ToString());
				careerEarningsText.text = "$ " + gsp.earnings.ToString();

				//gsp.record = new Vector2(gsp.record.x + tm.teams[playerTeam].wins, gsp.record.y + tm.teams[playerTeam].loss);

				StartCoroutine(SaveCareer(false));
				//heading.text = "So Close!";

				playButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(false);
				simButton.gameObject.SetActive(false);
				nextButton.gameObject.SetActive(true);
				horizScrollBar.value = 1;

				break;

		}
	}

	public void OnSim()
	{
		StartCoroutine(SimPlayoff(false, false));
	}

	IEnumerator SimPlayoff(bool game1, bool game2)
	{
		Debug.Log("Sim Playoffs - Round " + playoffRound);
		Team[] gameX;
		Team[] gameY;
		Team game1X;
		Team game1Y;
		Team game2X;
		Team game2Y;

		switch (playoffRound)
		{
            #region Round 1
            case 1:
				gameX = new Team[8];
				gameY = new Team[8];

				for (int i = 0; i < 8; i++)
                {
					for (int j = 0; j < teams.Length; j++)
					{
						if (gameList[i].x == teams[j].id)
						{
							gameX[i] = teams[j];
						}
						else if (gameList[i].y == teams[j].id)
                        {
							gameY[i] = teams[j];
                        }
					}
                }

				for (int i = 0; i < 8; i++)
                {
					if (i % 2 == 0)
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[(i / 2) + 8].x = gameX[i].id;
							gameX[i].wins++;
							gameList[(i / 2) + 12].x = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[(i / 2) + 8].x = gameY[i].id;
							gameY[i].wins++;
							gameList[(i / 2) + 12].x = gameX[i].id;
							gameX[i].loss++;
						}
					}
					else
                    {
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[((i - 1) / 2) + 8].y = gameX[i].id;
							gameX[i].wins++;
							gameList[((i - 1) / 2) + 12].y = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[((i - 1) / 2) + 8].y = gameY[i].id;
							gameY[i].wins++;
							gameList[((i - 1) / 2) + 12].y = gameX[i].id;
							gameX[i].loss++;
						}
					}
				}
				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SetPlayoffs();
				break;
            #endregion
            #region Round 2
            case 2:
				gameX = new Team[4];
				gameY = new Team[4];

				for (int i = 0; i < 8; i++)
				{
					for (int j = 0; j < teams.Length; j++)
					{
						if (gameList[i].x == teams[j].id)
						{
							gameX[i] = teams[j];
						}
						else if (gameList[i].y == teams[j].id)
						{
							gameY[i] = teams[j];
						}
					}
				}

				for (int i = 0; i < 8; i++)
				{
					if (i % 2 == 0)
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[(i / 2) + 16].x = gameX[i].id;
							gameX[i].wins++;
							gameList[(i / 2) + 12].x = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[(i / 2) + 16].x = gameY[i].id;
							gameY[i].wins++;
							gameList[(i / 2) + 12].x = gameX[i].id;
							gameX[i].loss++;
						}
					}
					else
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[((i - 1) / 2) + 16].y = gameX[i].id;
							gameX[i].wins++;
							gameList[((i - 1) / 2) + 12].y = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[((i - 1) / 2) + 16].y = gameY[i].id;
							gameY[i].wins++;
							gameList[((i - 1) / 2) + 12].y = gameX[i].id;
							gameX[i].loss++;
						}
					}
				}
				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SetPlayoffs();
				break;
			#endregion
			case 3:
				game1X = teams[4];
				game1Y = teams[7];

				if (Random.Range(0, game1X.strength) > Random.Range(0, game1Y.strength))
				{
					teams[8] = game1X;
				}
				else
				{
					teams[8] = game1Y;
				}

				for (int i = 0; i < 9; i++)
				{
					brackDisplay[i].rank.text = teams[i].rank.ToString();
					brackDisplay[i].name.text = teams[i].name;
					brackDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[i].SetActive(true);
				}
				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SetPlayoffs();
				break;

			default:
				SetPlayoffs();
				break;

		}
		yield break;
	}

	public void SimRound(int numberOfGames)
    {
		Team[] gameX;
		Team[] gameY;
		gameX = new Team[numberOfGames];
		gameY = new Team[numberOfGames];

		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < teams.Length; j++)
			{
				if (gameList[i].x == teams[j].id)
				{
					gameX[i] = teams[j];
				}
				else if (gameList[i].y == teams[j].id)
				{
					gameY[i] = teams[j];
				}
			}
		}

		for (int i = 0; i < 8; i++)
		{
			if (i % 2 == 0)
			{
				if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
				{
					gameList[(i / 2) + 8].x = gameX[i].id;
					gameList[(i / 2) + 12].x = gameY[i].id;
				}
				else
				{
					gameList[(i / 2) + 8].x = gameY[i].id;
					gameList[(i / 2) + 12].x = gameX[i].id;
				}
			}
			else
			{
				if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
				{
					gameList[((i - 1) / 2) + 8].y = gameX[i].id;
					gameList[((i - 1) / 2) + 12].y = gameY[i].id;
				}
				else
				{
					gameList[((i - 1) / 2) + 8].y = gameY[i].id;
					gameList[((i - 1) / 2) + 12].y = gameX[i].id;
				}
			}
		}
	}
	IEnumerator LoadCareer()
	{
		gsp.LoadCareer();

		yield return careerEarningsText.text = "$ " + gsp.earnings.ToString();
	}

	IEnumerator SaveCareer(bool inProgress)
	{
		Debug.Log("Saving in PlayoffManager, inProgress is " + inProgress);

		myFile = new EasyFileSave("my_player_data");

		myFile.Add("Career Record", gsp.record);
		Debug.Log("gsp.record is " + gsp.record.x + " - " + gsp.record.y);
		//Vector2 tempRecord = new Vector2(gsp.record.x, gsp.record.y);
		//myFile.Add("Player Name", gsp.firstName);
		//myFile.Add("Team Name", gsp.teamName);
		//myFile.Add("Team Colour", gsp.teamColour);
		myFile.Add("Career Earnings", gsp.earnings);

		//if (!inProgress)
		//      {
		//	tm.draw = 0;
		//	playoffRound = 0;
		//	tm.playoffRound = 0;

		//      }
		myFile.Add("Tourny In Progress", inProgress);
		gsp.inProgress = inProgress;
		myFile.Add("Draw", gsp.draw);
		myFile.Add("Number Of Teams", gsp.numberOfTeams);
		myFile.Add("Player Team", playerTeam);
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

		int[] playoffIDList = new int[teams.Length];

		for (int i = 0; i < teams.Length; i++)
		{
			//Debug.Log("playoffID i is " + i);
			playoffIDList[i] = teams[i].id;
			Debug.Log("Playoff ID List - " + playoffIDList[i]);
		}

		myFile.Add("Playoff ID List", playoffIDList);
		//yield return myFile.TestDataSaveLoad();
		yield return myFile.Append();
	}
}
