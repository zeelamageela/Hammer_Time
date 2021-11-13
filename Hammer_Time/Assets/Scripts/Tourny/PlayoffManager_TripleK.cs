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

				StartCoroutine(SimPlayoff());
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
            #region Round 1
            case 1:
				heading.text = "Round 1";

				for (int i = 0; i < teams.Length; i++)
				{
					winnersDisplay1[i].name.text = teams[i].name;
					winnersDisplay1[i].rank.text = teams[i].wins.ToString() + "-" + teams[i].loss.ToString();
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
#endregion

            #region Round 2
            case 2:
				heading.text = "B - Round 2";

				for (int i = 0; i < losersDisplayA2.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 8].x)
							{
								Debug.Log("Setting Losers Bracket X-" + gameList[(i / 2) + 8].x + " - " + teams[j].name);
                                losersDisplayA2[i].name.text = teams[j].name;
								losersDisplayA2[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 8].y)
							{
								Debug.Log("Setting Losers Bracket Y-" + gameList[((i - 1) / 2) + 8].y + " - " + teams[j].name);
								losersDisplayA2[i].name.text = teams[j].name;
								losersDisplayA2[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
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
#endregion
            #region Round 3
            case 3:
				heading.text = "A - Round 3";

				for (int i = 0; i < winnersDisplay3.Length; i++)
				{
					for (int j = 0; j < teams.Length; j++)
					{
						if (i % 2 == 0)
						{
							if (teams[j].id == gameList[(i / 2) + 12].x)
							{
								Debug.Log("Setting Winners Bracket X-" + gameList[(i / 2) + 12].x + " - " + teams[j].name);
								winnersDisplay3[i].name.text = teams[j].name;
								winnersDisplay3[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
							}
						}
						else
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 12].y)
							{
								Debug.Log("Setting Winners Bracket Y-" + gameList[((i - 1) / 2) + 12].x + " - " + teams[j].name);
								winnersDisplay3[i].name.text = teams[j].name;
								winnersDisplay3[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
							}
						}
					}
				}

				for (int i = 0; i < winnersBracket.transform.childCount; i++)
				{
					if (i <= 1)
						winnersBracket.transform.GetChild(i).gameObject.SetActive(true);
					else
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
			#endregion
			#region Round 4
			case 4:
				heading.text = "B - Round 4";

				for (int i = 0; i < losersDisplayA4.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 16].x)
							{
								Debug.Log("Setting Losers Bracket X-" + gameList[(i / 2) + 8].x + " - " + teams[j].name);
								losersDisplayA4[i].name.text = teams[j].name;
								losersDisplayA4[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 16].y)
							{
								Debug.Log("Setting Losers Bracket Y-" + gameList[((i - 1) / 2) + 8].y + " - " + teams[j].name);
								losersDisplayA4[i].name.text = teams[j].name;
								losersDisplayA4[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				for (int i = 0; i < losersBracket1.transform.childCount; i++)
				{
					if (i <= 1)
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
			#endregion

			#region Round 5
			case 5:
				heading.text = "C - Round 5";

				for (int i = 0; i < losersDisplayB5.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 20].x)
							{
								Debug.Log("Setting Losers Bracket C X-" + gameList[(i / 2) + 20].x + " - " + teams[j].name);
								losersDisplayB5[i].name.text = teams[j].name;
								losersDisplayB5[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 20].y)
							{
								Debug.Log("Setting Losers Bracket C Y-" + gameList[((i - 1) / 2) + 20].y + " - " + teams[j].name);
								losersDisplayB5[i].name.text = teams[j].name;
								losersDisplayB5[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				for (int i = 0; i < losersBracket1.transform.childCount; i++)
				{
					if (i <= 0)
						losersBracket2.transform.GetChild(i).gameObject.SetActive(true);
					else
						losersBracket2.transform.GetChild(i).gameObject.SetActive(false);
				}

				winnersBracket.SetActive(false);
				losersBracket1.SetActive(false);
				losersBracket2.SetActive(true);
				finalsBracket.SetActive(false);

				StartCoroutine(RefreshPlayoffPanel());

				playoffs.SetActive(true);

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				horizScrollBar.value = 0;
				vertScrollBar.value = 0;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion

			#region Round 6
			case 6:
				heading.text = "C - Round 6";

				for (int i = 0; i < losersDisplayB6.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 24].x)
							{
								Debug.Log("Setting Losers Bracket X-" + gameList[(i / 2) + 24].x + " - " + teams[j].name);
								losersDisplayB6[i].name.text = teams[j].name;
								losersDisplayB6[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 24].y)
							{
								Debug.Log("Setting Losers Bracket Y-" + gameList[((i - 1) / 2) + 24].y + " - " + teams[j].name);
								losersDisplayB6[i].name.text = teams[j].name;
								losersDisplayB6[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				for (int i = 0; i < losersBracket2.transform.childCount; i++)
				{
					if (i <= 1)
						losersBracket2.transform.GetChild(i).gameObject.SetActive(true);
					else
						losersBracket2.transform.GetChild(i).gameObject.SetActive(false);
				}

				winnersBracket.SetActive(false);
				losersBracket1.SetActive(false);
				losersBracket2.SetActive(true);
				finalsBracket.SetActive(false);

				StartCoroutine(RefreshPlayoffPanel());

				playoffs.SetActive(true);

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				horizScrollBar.value = 0;
				vertScrollBar.value = 0;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion

			#region Round 7
			case 7:
				heading.text = "A - Round 7";

				for (int i = 0; i < winnersDisplay7.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 26].x)
							{
								Debug.Log("Setting Losers Bracket X-" + gameList[(i / 2) + 24].x + " - " + teams[j].name);
								winnersDisplay7[i].name.text = teams[j].name;
								winnersDisplay7[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 26].y)
							{
								Debug.Log("Setting Losers Bracket Y-" + gameList[((i - 1) / 2) + 24].y + " - " + teams[j].name);
								winnersDisplay7[i].name.text = teams[j].name;
								winnersDisplay7[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				for (int i = 0; i < winnersBracket.transform.childCount; i++)
				{
					if (i <= 2)
						winnersBracket.transform.GetChild(i).gameObject.SetActive(true);
					else
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
			#endregion

			#region Round 8
			case 8:
				heading.text = "B - Round 8";

				for (int i = 0; i < losersDisplayA8.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 28].x)
							{
								Debug.Log("Setting Losers Bracket X-" + gameList[(i / 2) + 28].x + " - " + teams[j].name);
								losersDisplayA8[i].name.text = teams[j].name;
								losersDisplayA8[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 28].y)
							{
								Debug.Log("Setting Losers Bracket Y-" + gameList[((i - 1) / 2) + 28].y + " - " + teams[j].name);
								losersDisplayA8[i].name.text = teams[j].name;
								losersDisplayA8[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				for (int i = 0; i < losersBracket1.transform.childCount; i++)
				{
					if (i <= 2)
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
			#endregion

			#region Round 9
			case 9:
				heading.text = "B - Round 9";

				for (int i = 0; i < losersDisplayA9.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 30].x)
							{
								Debug.Log("Setting Losers Bracket X-" + gameList[(i / 2) + 30].x + " - " + teams[j].name);
								losersDisplayA9[i].name.text = teams[j].name;
								losersDisplayA9[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 30].y)
							{
								Debug.Log("Setting Losers Bracket Y-" + gameList[((i - 1) / 2) + 30].y + " - " + teams[j].name);
								losersDisplayA9[i].name.text = teams[j].name;
								losersDisplayA9[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				for (int i = 0; i < losersBracket1.transform.childCount; i++)
				{
					if (i <= 2)
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
			#endregion

			#region Round 10
			case 10:
				heading.text = "C - Round 10";

				for (int i = 0; i < losersDisplayB10.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 32].x)
							{
								Debug.Log("Setting Losers Bracket X-" + gameList[(i / 2) + 32].x + " - " + teams[j].name);
								losersDisplayB10[i].name.text = teams[j].name;
								losersDisplayB10[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 32].y)
							{
								Debug.Log("Setting Losers Bracket Y-" + gameList[((i - 1) / 2) + 32].y + " - " + teams[j].name);
								losersDisplayB10[i].name.text = teams[j].name;
								losersDisplayB10[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				for (int i = 0; i < losersBracket2.transform.childCount; i++)
				{
					if (i <= 2)
						losersBracket2.transform.GetChild(i).gameObject.SetActive(true);
					else
						losersBracket2.transform.GetChild(i).gameObject.SetActive(false);
				}

				winnersBracket.SetActive(false);
				losersBracket1.SetActive(false);
				losersBracket2.SetActive(true);
				finalsBracket.SetActive(false);

				StartCoroutine(RefreshPlayoffPanel());

				playoffs.SetActive(true);

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				horizScrollBar.value = 0;
				vertScrollBar.value = 0;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion

			#region Round 11
			case 11:
				heading.text = "C - Round 11";

				for (int i = 0; i < losersDisplayB11.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 34].x)
							{
								Debug.Log("Setting Losers Bracket X-" + gameList[(i / 2) + 34].x + " - " + teams[j].name);
								losersDisplayB11[i].name.text = teams[j].name;
								losersDisplayB11[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 34].y)
							{
								Debug.Log("Setting Losers Bracket Y-" + gameList[((i - 1) / 2) + 34].y + " - " + teams[j].name);
								losersDisplayB11[i].name.text = teams[j].name;
								losersDisplayB11[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				for (int i = 0; i < losersBracket2.transform.childCount; i++)
				{
					if (i <= 2)
						losersBracket2.transform.GetChild(i).gameObject.SetActive(true);
					else
						losersBracket2.transform.GetChild(i).gameObject.SetActive(false);
				}

				winnersBracket.SetActive(false);
				losersBracket1.SetActive(false);
				losersBracket2.SetActive(true);
				finalsBracket.SetActive(false);

				StartCoroutine(RefreshPlayoffPanel());

				playoffs.SetActive(true);

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				horizScrollBar.value = 0;
				vertScrollBar.value = 0;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion

			#region Round 12
			case 12:
				heading.text = "A - Round 12";

				for (int i = 0; i < winnersDisplay12.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 26].x)
							{
								Debug.Log("Setting Losers Bracket X-" + gameList[(i / 2) + 24].x + " - " + teams[j].name);
								winnersDisplay7[i].name.text = teams[j].name;
								winnersDisplay7[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 26].y)
							{
								Debug.Log("Setting Losers Bracket Y-" + gameList[((i - 1) / 2) + 24].y + " - " + teams[j].name);
								winnersDisplay7[i].name.text = teams[j].name;
								winnersDisplay7[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				for (int i = 0; i < winnersBracket.transform.childCount; i++)
				{
					if (i <= 2)
						winnersBracket.transform.GetChild(i).gameObject.SetActive(true);
					else
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
				#endregion

		}
	}

	public void OnSim()
	{
		StartCoroutine(SimPlayoff());
	}

	IEnumerator SimPlayoff()
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
							gameList[(i / 2) + 12].x = gameX[i].id;
							gameX[i].wins++;
							gameList[(i / 2) + 8].x = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[(i / 2) + 12].x = gameY[i].id;
							gameY[i].wins++;
							gameList[(i / 2) + 8].x = gameX[i].id;
							gameX[i].loss++;
						}
					}
					else
                    {
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[((i - 1) / 2) + 12].y = gameX[i].id;
							gameX[i].wins++;
							gameList[((i - 1) / 2) + 8].y = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[((i - 1) / 2) + 12].y = gameY[i].id;
							gameY[i].wins++;
							gameList[((i - 1) / 2) + 8].y = gameX[i].id;
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

				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < teams.Length; j++)
					{
						if (gameList[i + 8].x == teams[j].id)
						{
							gameX[i] = teams[j];
						}
						else if (gameList[i + 8].y == teams[j].id)
						{
							gameY[i] = teams[j];
						}
					}
				}

				for (int i = 0; i < 4; i++)
				{
					if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
					{
						gameList[i + 16].y = gameX[i].id;
						gameX[i].wins++;
						gameList[i + 20].x = gameY[i].id;
						gameY[i].loss++;
					}
					else
					{
						gameList[i + 16].y = gameY[i].id;
						gameY[i].wins++;
						gameList[i + 20].x = gameX[i].id;
						gameX[i].loss++;
					}
				}
				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SetPlayoffs();
				break;
			#endregion
			#region Round 3
			case 3:
				gameX = new Team[4];
				gameY = new Team[4];

				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < teams.Length; j++)
					{
						if (gameList[i + 12].x == teams[j].id)
						{
							gameX[i] = teams[j];
						}
						else if (gameList[i + 12].y == teams[j].id)
						{
							gameY[i] = teams[j];
						}
					}
				}

				for (int i = 0; i < 4; i++)
				{
					if (i % 2 == 0)
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[(i / 2) + 26].x = gameX[i].id;
							gameX[i].wins++;
							gameList[19 - i].x = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[(i / 2) + 26].x = gameY[i].id;
							gameY[i].wins++;
							gameList[19 - i].x = gameX[i].id;
							gameX[i].loss++;
						}
					}
					else
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[((i - 1) / 2) + 26].y = gameX[i].id;
							gameX[i].wins++;
							gameList[19 - i].x = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[((i - 1) / 2) + 26].y = gameY[i].id;
							gameY[i].wins++;
							gameList[19 - i].x = gameX[i].id;
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

			#region Round 4
			case 4:
				gameX = new Team[4];
				gameY = new Team[4];

				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < teams.Length; j++)
					{
						if (gameList[i + 16].x == teams[j].id)
						{
							gameX[i] = teams[j];
						}
						else if (gameList[i + 16].y == teams[j].id)
						{
							gameY[i] = teams[j];
						}
					}
				}

				for (int i = 0; i < 4; i++)
				{
					if (i % 2 == 0)
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[(i / 2) + 28].x = gameX[i].id;
							gameX[i].wins++;
							gameList[23 - i].y = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[(i / 2) + 28].x = gameY[i].id;
							gameY[i].wins++;
							gameList[23 - i].y = gameX[i].id;
							gameX[i].loss++;
						}
					}
					else
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[((i - 1) / 2) + 28].y = gameX[i].id;
							gameX[i].wins++;
							gameList[23 - i].y = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[((i - 1) / 2) + 28].y = gameY[i].id;
							gameY[i].wins++;
							gameList[23 - i].y = gameX[i].id;
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

			#region Round 5
			case 5:
				gameX = new Team[4];
				gameY = new Team[4];

				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < teams.Length; j++)
					{
						if (gameList[i + 20].x == teams[j].id)
						{
							gameX[i] = teams[j];
						}
						else if (gameList[i + 20].y == teams[j].id)
						{
							gameY[i] = teams[j];
						}
					}
				}

				for (int i = 0; i < 4; i++)
				{
					if (i % 2 == 0)
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[(i / 2) + 24].x = gameX[i].id;
							gameX[i].wins++;
							gameY[i].loss++;
						}
						else
						{
							gameList[(i / 2) + 24].x = gameY[i].id;
							gameY[i].wins++;
							gameX[i].loss++;
						}
					}
					else
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[((i - 1) / 2) + 24].y = gameX[i].id;
							gameX[i].wins++;
							gameY[i].loss++;
						}
						else
						{
							gameList[((i - 1) / 2) + 24].y = gameY[i].id;
							gameY[i].wins++;
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

			#region Round 6
			case 6:
				gameX = new Team[2];
				gameY = new Team[2];

				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < teams.Length; j++)
					{
						if (gameList[i + 24].x == teams[j].id)
						{
							gameX[i] = teams[j];
						}
						else if (gameList[i + 24].y == teams[j].id)
						{
							gameY[i] = teams[j];
						}
					}
				}

				for (int i = 0; i < 2; i++)
				{
					if (i == 0)
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[32].y = gameX[i].id;
							gameX[i].wins++;
							gameY[i].loss++;
						}
						else
						{
							gameList[32].y = gameY[i].id;
							gameY[i].wins++;
							gameX[i].loss++;
						}
					}
					else
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[33].y = gameX[i].id;
							gameX[i].wins++;
							gameY[i].loss++;
						}
						else
						{
							gameList[33].y = gameY[i].id;
							gameY[i].wins++;
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

			#region Round 7
			case 7:
				gameX = new Team[2];
				gameY = new Team[2];

				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < teams.Length; j++)
					{
						if (gameList[i + 26].x == teams[j].id)
						{
							gameX[i] = teams[j];
						}
						else if (gameList[i + 26].y == teams[j].id)
						{
							gameY[i] = teams[j];
						}
					}
				}

				for (int i = 0; i < 2; i++)
				{
					if (i == 0)
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[36].x = gameX[i].id;
							gameX[i].wins++;
							gameList[30].x = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[36].x = gameY[i].id;
							gameY[i].wins++;
							gameList[30].x = gameX[i].id;
							gameX[i].loss++;
						}
					}
					else
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[36].y = gameX[i].id;
							gameX[i].wins++;
							gameList[31].y = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[36].y = gameY[i].id;
							gameY[i].wins++;
							gameList[31].y = gameX[i].id;
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

			#region Round 8
			case 8:
				gameX = new Team[2];
				gameY = new Team[2];

				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < teams.Length; j++)
					{
						if (gameList[i + 28].x == teams[j].id)
						{
							gameX[i] = teams[j];
						}
						else if (gameList[i + 28].y == teams[j].id)
						{
							gameY[i] = teams[j];
						}
					}
				}

				for (int i = 0; i < 2; i++)
				{
					if (i == 0)
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[28].y = gameX[i].id;
							gameX[i].wins++;
							gameList[30].x = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[28].y = gameY[i].id;
							gameY[i].wins++;
							gameList[30].x = gameX[i].id;
							gameX[i].loss++;
						}
					}
					else
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[29].x = gameX[i].id;
							gameX[i].wins++;
							gameList[31].x = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[29].x = gameY[i].id;
							gameY[i].wins++;
							gameList[31].x = gameX[i].id;
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

			#region Round 9
			case 9:
				gameX = new Team[2];
				gameY = new Team[2];

				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < teams.Length; j++)
					{
						if (gameList[i + 30].x == teams[j].id)
						{
							gameX[i] = teams[j];
						}
						else if (gameList[i + 30].y == teams[j].id)
						{
							gameY[i] = teams[j];
						}
					}
				}

				for (int i = 0; i < 2; i++)
				{
					if (i == 0)
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[38].x = gameX[i].id;
							gameX[i].wins++;
							gameList[35].x = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[38].x = gameY[i].id;
							gameY[i].wins++;
							gameList[35].x = gameX[i].id;
							gameX[i].loss++;
						}
					}
					else
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[38].y = gameX[i].id;
							gameX[i].wins++;
							gameList[34].x = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[38].y = gameY[i].id;
							gameY[i].wins++;
							gameList[34].x = gameX[i].id;
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

			#region Round 10
			case 10:
				gameX = new Team[2];
				gameY = new Team[2];

				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < teams.Length; j++)
					{
						if (gameList[i + 32].x == teams[j].id)
						{
							gameX[i] = teams[j];
						}
						else if (gameList[i + 32].y == teams[j].id)
						{
							gameY[i] = teams[j];
						}
					}
				}

				for (int i = 0; i < 2; i++)
				{
					if (i == 0)
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[34].y = gameX[i].id;
							gameX[i].wins++;
							gameY[i].loss++;
						}
						else
						{
							gameList[34].y = gameY[i].id;
							gameY[i].wins++;
							gameX[i].loss++;
						}
					}
					else
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[35].y = gameX[i].id;
							gameX[i].wins++;
							gameY[i].loss++;
						}
						else
						{
							gameList[35].y = gameY[i].id;
							gameY[i].wins++;
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

			#region Round 11
			case 11:
				gameX = new Team[2];
				gameY = new Team[2];

				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < teams.Length; j++)
					{
						if (gameList[i + 34].x == teams[j].id)
						{
							gameX[i] = teams[j];
						}
						else if (gameList[i + 34].y == teams[j].id)
						{
							gameY[i] = teams[j];
						}
					}
				}

				for (int i = 0; i < 2; i++)
				{
					if (i == 0)
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[39].x = gameX[i].id;
							gameX[i].wins++;
							gameY[i].loss++;
						}
						else
						{
							gameList[39].x = gameY[i].id;
							gameY[i].wins++;
							gameX[i].loss++;
						}
					}
					else
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[39].y = gameX[i].id;
							gameX[i].wins++;
							gameY[i].loss++;
						}
						else
						{
							gameList[39].y = gameY[i].id;
							gameY[i].wins++;
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

			#region Round 12
			case 12:
				gameX = new Team[2];
				gameY = new Team[2];

				for (int j = 0; j < teams.Length; j++)
				{
					if (gameList[36].x == teams[j].id)
					{
						gameX[0] = teams[j];
					}
					else if (gameList[36].y == teams[j].id)
					{
						gameY[0] = teams[j];
					}
				}
				if (Random.Range(0, gameX[0].strength) > Random.Range(0, gameY[0].strength))
				{
					gameList[41].x = gameX[0].id;
					gameX[0].wins++;
					gameList[40].x = gameY[0].id;
					gameY[0].loss++;
				}
				else
				{
					gameList[41].x = gameY[0].id;
					gameY[0].wins++;
					gameList[40].x = gameX[0].id;
					gameX[0].loss++;
				}

				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SetPlayoffs();
				break;
			#endregion

			#region Round 13
			case 13:
				gameX = new Team[2];
				gameY = new Team[2];

				for (int j = 0; j < teams.Length; j++)
				{
					if (gameList[37].x == teams[j].id)
					{
						gameX[0] = teams[j];
					}
					else if (gameList[37].y == teams[j].id)
					{
						gameY[0] = teams[j];
					}
				}
				if (Random.Range(0, gameX[0].strength) > Random.Range(0, gameY[0].strength))
				{
					gameList[40].x = gameX[0].id;
					gameX[0].wins++;
					gameList[39].x = gameY[0].id;
					gameY[0].loss++;
				}
				else
				{
					gameList[40].x = gameY[0].id;
					gameY[0].wins++;
					gameList[39].x = gameX[0].id;
					gameX[0].loss++;
				}

				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SetPlayoffs();
				break;
			#endregion

			#region Round 14
			case 14:
				gameX = new Team[2];
				gameY = new Team[2];

				for (int j = 0; j < teams.Length; j++)
				{
					if (gameList[38].x == teams[j].id)
					{
						gameX[0] = teams[j];
					}
					else if (gameList[38].y == teams[j].id)
					{
						gameY[0] = teams[j];
					}
				}
				if (Random.Range(0, gameX[0].strength) > Random.Range(0, gameY[0].strength))
				{
					gameList[39].y = gameX[0].id;
					gameX[0].wins++;
					gameY[0].loss++;
				}
				else
				{
					gameList[39].y = gameY[0].id;
					gameY[0].wins++;
					gameX[0].loss++;
				}

				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SetPlayoffs();
				break;
			#endregion

			#region Round 15
			case 15:
				gameX = new Team[2];
				gameY = new Team[2];

				for (int j = 0; j < teams.Length; j++)
				{
					if (gameList[38].x == teams[j].id)
					{
						gameX[0] = teams[j];
					}
					else if (gameList[38].y == teams[j].id)
					{
						gameY[0] = teams[j];
					}
				}
				if (Random.Range(0, gameX[0].strength) > Random.Range(0, gameY[0].strength))
				{
					gameList[39].y = gameX[0].id;
					gameX[0].wins++;
					gameY[0].loss++;
				}
				else
				{
					gameList[39].y = gameY[0].id;
					gameY[0].wins++;
					gameX[0].loss++;
				}

				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SetPlayoffs();
				break;
			#endregion
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
					gameList[(i / 2) + 12].x = gameX[i].id;
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
