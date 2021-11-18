using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TigerForge;

public class PlayoffManager_TripleK : MonoBehaviour
{
	
	public CareerManager cm;
	public Team[] teams;

	public GameObject winnersBracket;
	public GameObject losersBracket1;
	public GameObject losersBracket2;
	public GameObject finalsBracket;

	public Color green;
	public Color red;
	public Color yellow;
	public Color dimmed;

	public VSDisplay[] vsDisplay;
	public Text vsDisplayTitle;
	public Text vsDisplayVS;
	public GameObject vsDisplayGO;

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
	bool cont;
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
		cm = FindObjectOfType<CareerManager>();
		myFile = new EasyFileSave("my_player_data");

		//StartCoroutine(LoadCareer());
		Debug.Log("Career Earnings before playoffs - $ " + gsp.earnings.ToString());

		teams = new Team[16];
		gameList = new Vector2[46];
		playoffs.SetActive(true);
		if (cm)
		{
			playerTeam = cm.playerTeamIndex;
		}
		if (gsp.careerLoad)
		{
			gsp.LoadCareer();
			careerEarnings = gsp.earnings;
			if (cm)
			{
				playerTeam = cm.playerTeamIndex;
			}
			if (gsp.inProgress)
			{
				gsp.LoadKOTourny();
				gsp.inProgress = false;
				gsp.careerLoad = false;
				Debug.Log("Playoff Round BEFORE the minus - " + gsp.playoffRound);
				//gsp.playoffRound--;
				Debug.Log("Playoff Round AFTER the minus - " + gsp.playoffRound);
				LoadPlayoffs();
			}
		}
		else
		{
			//playerTeam = tm.playerTeam;
			playoffRound = gsp.playoffRound;
		}

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
		//teams = new Team[9];
		heading.text = "Page Playoff";

		playoffRound++;

		if (gsp.teams.Length > 0)
		{
			for (int i = 0; i < teams.Length; i++)
			{
				teams[i] = gsp.teams[i];
				teams[i].wins = 0;
				teams[i].loss = 0;
			}
		}
		else
        {
			for (int i = 0; i < teams.Length; i++)
			{
				teams[i] = tTeamList.teams[i];
			}
		}
		//teams[0].name = gsp.teamName;
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


	void LoadPlayoffs()
	{
		
		if (myFile.Load())
		{
			Debug.Log("Load Playoffs - Round " + playoffRound);
            for (int i = 0; i < teams.Length; i++)
            {
                teams[i] = gsp.teams[i];
            }
            //teams = gsp.teams;
			int[] winsList = myFile.GetArray<int>("Tourny Wins List");
			int[] lossList = myFile.GetArray<int>("Tourny Loss List");
			int [] rankList = myFile.GetArray<int>("Tourny Rank List");
			string[] nextOppList = myFile.GetArray<string>("Tourny NextOpp List");
			int[] strengthList = myFile.GetArray<int>("Tourny Strength List");
			int[] idList = myFile.GetArray<int>("Tourny Team ID List");
			//StartCoroutine(Wait());

			for (int i = 0; i < teams.Length; i++)
			{
				teams[i].id = idList[i];
				teams[i].wins = winsList[i];
				Debug.Log("Wins List is " + teams[i].wins);
				teams[i].loss = lossList[i];
				Debug.Log("Loss List is " + lossList[i]);
				teams[i].rank = rankList[i];
				teams[i].nextOpp = nextOppList[i];
				teams[i].strength = strengthList[i];
			}
			for (int i = 0; i < teams.Length; i++)
			{
				if (teams[i].id == gsp.playerTeam.id)
					playerTeam = i;
				if (teams[i].name == gsp.playerTeam.nextOpp)
					oppTeam = i;
			}
			int[] gameListX = myFile.GetArray<int>("Tourny Game X List");
			int[] gameListY = myFile.GetArray<int>("Tourny Game Y List");

			for (int i = 0; i < gameList.Length; i++)
            {
				gameList[i] = new Vector2(gameListX[i], gameListY[i]);
            }
			myFile.Dispose();
        }
		Debug.Log("OppTeam is " + oppTeam);

		if (gsp.redScore != gsp.yellowScore)
		{
			Debug.Log("We are returning from a game");
			cont = true;
			//SimPlayoff();
		}
		StartCoroutine(ResetBrackets(gsp.playoffRound));
		//SetPlayoffs();
	}

	public void SetPlayoffs()
	{
		Debug.Log("Set Playoffs 3K - Round " + playoffRound);
		bool playerGame = false;
		switch (playoffRound)
		{
            #region Round 1
            case 1:
				heading.text = "Round 1";
				playerGame = true;
				for (int i = 0; i < winnersDisplay1.Length; i++)
				{
					winnersDisplay1[i].name.text = teams[i].name;
					winnersDisplay1[i].rank.text = teams[i].wins.ToString() + "-" + teams[i].loss.ToString();
					if (i % 2 == 0 && gameList[i / 2].x == playerTeam)
					{
						oppTeam = (int)gameList[i / 2].y;
						winnersDisplay1[i].bg.GetComponent<Image>().color = yellow;
						playerGame = true;
					}
					if (i % 2 == 1 && gameList[(i - 1) / 2].y == playerTeam)
					{
						oppTeam = (int)gameList[(i - 1) / 2].x;
						winnersDisplay1[i].bg.GetComponent<Image>().color = yellow;
						playerGame = true;
					}
				}

				for (int i = 0; i < teams.Length; i++)
				{
					if (teams[i].id == playerTeam)
					{
						vsDisplay[0].rank.text = teams[i].wins.ToString() + "-" + teams[i].loss.ToString();
						vsDisplay[0].name.text = teams[i].name;
					}
					if (teams[i].id == oppTeam)
					{
						vsDisplay[1].rank.text = teams[i].wins.ToString() + "-" + teams[i].loss.ToString();
						vsDisplay[1].name.text = teams[i].name;
					}
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
				playButton.gameObject.SetActive(true);
				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				horizScrollBar.value = 0;
				//StartCoroutine(SaveCareer(true));
				break;
#endregion
            #region Round 2
            case 2:
				heading.text = "A - Round 2";

				for (int i = 0; i < losersDisplayA2.Length; i++)
				{
					for (int j = 0; j < teams.Length; j++)
					{
						if (i % 2 == 0)
						{
							if (teams[j].id == gameList[(i / 2) + 8].x)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[(i / 2) + 8].y;
									losersDisplayA2[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A X-" + gameList[(i / 2) + 8].x + " - " + teams[j].name);
								losersDisplayA2[i].name.text = teams[j].name;
								losersDisplayA2[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
						else
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 8].y)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 8].x;
									losersDisplayA2[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A Y-" + gameList[((i - 1) / 2) + 8].y + " - " + teams[j].name);
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
				vertScrollBar.value = 0.56f;
				//StartCoroutine(SaveCareer(true));
				break;
#endregion
            #region Round 3
            case 3:
				heading.text = "W - Round 3";

				for (int i = 0; i < winnersDisplay3.Length; i++)
				{
					for (int j = 0; j < teams.Length; j++)
					{
						if (i % 2 == 0)
						{
							if (teams[j].id == gameList[(i / 2) + 12].x)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[(i / 2) + 12].y;
									winnersDisplay3[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Winners Bracket X-" + gameList[(i / 2) + 12].x + " - " + teams[j].name);
								winnersDisplay3[i].name.text = teams[j].name;
								winnersDisplay3[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
							}
						}
						else
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 12].y)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 12].x;
									winnersDisplay3[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
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
				vertScrollBar.value = 1f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 4
			case 4:
				heading.text = "A - Round 4";

				for (int i = 0; i < losersDisplayA4.Length; i++)
				{
					losersDisplayA4[i].panel.transform.parent.gameObject.SetActive(true);
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 16].x)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[(i / 2) + 16].y;
									losersDisplayA4[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A X-" + gameList[(i / 2) + 16].x + " - " + teams[j].name);
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
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 16].x;
									losersDisplayA4[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A Y-" + gameList[((i - 1) / 2) + 16].y + " - " + teams[j].name);
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
				vertScrollBar.value = 0.74f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 5
			case 5:
				heading.text = "B - Round 5";

				for (int i = 0; i < losersDisplayB5.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 20].x)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[(i / 2) + 20].y;
									losersDisplayB5[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B X-" + gameList[(i / 2) + 20].x + " - " + teams[j].name);
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
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 20].x;
									losersDisplayB5[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B Y-" + gameList[((i - 1) / 2) + 20].y + " - " + teams[j].name);
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
				vertScrollBar.value = 0.33f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 6
			case 6:
				heading.text = "B - Round 6";

				for (int i = 0; i < losersDisplayB6.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 24].x)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[(i / 2) + 24].y;
									losersDisplayB6[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B X-" + gameList[(i / 2) + 24].x + " - " + teams[j].name);
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
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 24].x;
									losersDisplayB6[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B Y-" + gameList[((i - 1) / 2) + 24].y + " - " + teams[j].name);
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
				vertScrollBar.value = 0.3f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 7
			case 7:
				heading.text = "W - Round 7";

				for (int i = 0; i < winnersDisplay7.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 26].x)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[(i / 2) + 26].y;
									winnersDisplay7[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Winners Bracket X-" + gameList[(i / 2) + 26].x + " - " + teams[j].name);
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
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 26].x;
									winnersDisplay7[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Winners Bracket Y-" + gameList[((i - 1) / 2) + 26].y + " - " + teams[j].name);
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
				horizScrollBar.value = 0.35f;
				vertScrollBar.value = 1f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 8
			case 8:
				heading.text = "A - Round 8";

				for (int i = 0; i < losersDisplayA8.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 28].x)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[(i / 2) + 28].y;
									losersDisplayA8[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A X-" + gameList[(i / 2) + 28].x + " - " + teams[j].name);
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
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 28].x;
									losersDisplayA8[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A Y-" + gameList[((i - 1) / 2) + 28].y + " - " + teams[j].name);
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
				horizScrollBar.value = 0.3f;
				vertScrollBar.value = 0.77f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 9
			case 9:
				heading.text = "A - Round 9";

				for (int i = 0; i < 4; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 30].x)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[(i / 2) + 30].y;
									losersDisplayA9[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A X-" + gameList[(i / 2) + 30].x + " - " + teams[j].name);
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
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 30].x;
									losersDisplayA9[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A Y-" + gameList[((i - 1) / 2) + 30].y + " - " + teams[j].name);
								losersDisplayA9[i].name.text = teams[j].name;
								losersDisplayA9[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				for (int i = 0; i < losersBracket1.transform.childCount; i++)
				{
					if (i <= 3)
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
				horizScrollBar.value = 0.63f;
				vertScrollBar.value = 1f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 10
			case 10:
				heading.text = "B - Round 10";

				for (int i = 0; i < losersDisplayB10.Length; i++)
				{
					losersDisplayB10[i].panel.transform.parent.gameObject.SetActive(true);
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 32].x)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[(i / 2) + 32].y;
									losersDisplayB10[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B X-" + gameList[(i / 2) + 32].x + " - " + teams[j].name);
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
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 32].x;
									losersDisplayB10[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B Y-" + gameList[((i - 1) / 2) + 32].y + " - " + teams[j].name);
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
				horizScrollBar.value = 0.3f;
				vertScrollBar.value = 0.76f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 11
			case 11:
				heading.text = "B - Round 11";

				for (int i = 0; i < losersDisplayB11.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 34].x)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[(i / 2) + 34].y;
									losersDisplayB11[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B X-" + gameList[(i / 2) + 34].x + " - " + teams[j].name);
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
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 34].x;
									losersDisplayB11[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B Y-" + gameList[((i - 1) / 2) + 34].y + " - " + teams[j].name);
								losersDisplayB11[i].name.text = teams[j].name;
								losersDisplayB11[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				for (int i = 0; i < losersBracket2.transform.childCount; i++)
				{
					if (i <= 3)
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
				horizScrollBar.value = 0.61f;
				vertScrollBar.value = 1f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 12
			case 12:
				heading.text = "W - Round 12";

				for (int i = 0; i < 2; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[36].x)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[36].y;
									winnersDisplay12[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Winners Bracket X-" + gameList[36].x + " - " + teams[j].name);
								winnersDisplay12[i].name.text = teams[j].name;
								winnersDisplay12[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[36].y)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[36].x;
									winnersDisplay12[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Winners Bracket Y-" + gameList[36].y + " - " + teams[j].name);
								winnersDisplay12[i].name.text = teams[j].name;
								winnersDisplay12[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				for (int i = 0; i < winnersBracket.transform.childCount; i++)
				{
					winnersBracket.transform.GetChild(i).gameObject.SetActive(true);
				}

				winnersBracket.SetActive(true);
				losersBracket1.SetActive(false);
				losersBracket2.SetActive(false);
				finalsBracket.SetActive(false);

				StartCoroutine(RefreshPlayoffPanel());

				playoffs.SetActive(true);

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				horizScrollBar.value = 0.75f;
				vertScrollBar.value = 0.76f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 13
			case 13:
				heading.text = "A - Round 13";

				for (int i = 0; i < 2; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[37].x)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[37].y;
									losersDisplayA13[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A X-" + gameList[37].x + " - " + teams[j].name);
								losersDisplayA13[i].name.text = teams[j].name;
								losersDisplayA13[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[37].y)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[37].x;
									losersDisplayA13[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A Y-" + gameList[37].y + " - " + teams[j].name);
								losersDisplayA13[i].name.text = teams[j].name;
								losersDisplayA13[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				for (int i = 0; i < losersBracket1.transform.childCount; i++)
				{
					losersBracket1.transform.GetChild(i).gameObject.SetActive(true);
				}

				winnersBracket.SetActive(false);
				losersBracket1.SetActive(true);
				losersBracket2.SetActive(false);
				finalsBracket.SetActive(false);

				StartCoroutine(RefreshPlayoffPanel());

				playoffs.SetActive(true);

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				horizScrollBar.value = 0.93f;
				vertScrollBar.value = 1f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 14
			case 14:
				heading.text = "B - Round 14";

				for (int i = 0; i < 2; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[38].x)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[38].y;
									losersDisplayB14[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B X-" + gameList[38].x + " - " + teams[j].name);
								losersDisplayB14[i].name.text = teams[j].name;
								losersDisplayB14[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[38].y)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[38].x;
									losersDisplayB14[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B Y-" + gameList[38].y + " - " + teams[j].name);
								losersDisplayB14[i].name.text = teams[j].name;
								losersDisplayB14[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				for (int i = 0; i < losersBracket2.transform.childCount; i++)
				{
					losersBracket2.transform.GetChild(i).gameObject.SetActive(true);
				}

				winnersBracket.SetActive(false);
				losersBracket1.SetActive(false);
				losersBracket2.SetActive(true);
				finalsBracket.SetActive(false);

				StartCoroutine(RefreshPlayoffPanel());

				playoffs.SetActive(true);

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				horizScrollBar.value = 0.93f;
				vertScrollBar.value = 1f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 15
			case 15:
				heading.text = "F - Round 15";

				for (int i = 0; i < finalsDisplay15.Length; i++)
				{
					if (i == 0)
                    {
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[41].x)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = 99;
									finalsDisplay15[i].bg.GetComponent<Image>().color = yellow;
								}

								Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay15[i].name.text = teams[j].name;
								finalsDisplay15[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[41 - (i / 2)].y)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[41 - (i / 2)].x;
									finalsDisplay15[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay15[i].name.text = teams[j].name;
								finalsDisplay15[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[41 - ((i + 1) / 2)].x)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[41 - ((i + 1) / 2)].y;
									finalsDisplay15[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket Y-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay15[i].name.text = teams[j].name;
								finalsDisplay15[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				for (int i = 0; i < finalsBracket.transform.childCount; i++)
				{
					if (i == 0)
						finalsBracket.transform.GetChild(i).gameObject.SetActive(true);
					else
						finalsBracket.transform.GetChild(i).gameObject.SetActive(false);
				}

				winnersBracket.SetActive(false);
				losersBracket1.SetActive(false);
				losersBracket2.SetActive(false);
				finalsBracket.SetActive(true);

				StartCoroutine(RefreshPlayoffPanel());

				playoffs.SetActive(true);

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				horizScrollBar.value = 0;
				vertScrollBar.value = 0.9f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 16
			case 16:
				heading.text = "F - Round 16";

				finalsDisplay15[0].panel.transform.parent.gameObject.SetActive(false);
				for (int i = 0; i < 3; i++)
				{
					finalsDisplay16[i].panel.transform.parent.gameObject.SetActive(true);
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[41].x && i == 0)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[41].y;
									finalsDisplay16[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay16[i].name.text = teams[j].name;
								finalsDisplay16[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
							if (teams[j].id == gameList[42].y && i == 2)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[42].x;
									finalsDisplay16[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay16[i].name.text = teams[j].name;
								finalsDisplay16[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[41].y)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[41].x;
									finalsDisplay16[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket Y-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay16[i].name.text = teams[j].name;
								finalsDisplay16[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}


				for (int i = 0; i < finalsBracket.transform.childCount; i++)
				{
					if (i <= 2)
						finalsBracket.transform.GetChild(i).gameObject.SetActive(true);
					else
						finalsBracket.transform.GetChild(i).gameObject.SetActive(false);
				}

				winnersBracket.SetActive(false);
				losersBracket1.SetActive(false);
				losersBracket2.SetActive(false);
				finalsBracket.SetActive(true);

				StartCoroutine(RefreshPlayoffPanel());

				playoffs.SetActive(true);

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				horizScrollBar.value = 0;
				vertScrollBar.value = 0.92f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 17
			case 17:
				heading.text = "F - Round 17";

				finalsDisplay16[2].panel.transform.parent.gameObject.SetActive(false);
				for (int i = 0; i < finalsDisplay17.Length; i++)
				{
					finalsDisplay17[i].panel.transform.parent.gameObject.SetActive(true);
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (i == 0 && teams[j].id == gameList[44].x)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[44].y;
									finalsDisplay17[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay17[i].name.text = teams[j].name;
								finalsDisplay17[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
							if (i == 2 && teams[j].id == gameList[42].y)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[42].x;
									finalsDisplay17[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay17[i].name.text = teams[j].name;
								finalsDisplay17[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[42].x)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[42].y;
									finalsDisplay17[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket Y-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay17[i].name.text = teams[j].name;
								finalsDisplay17[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				for (int i = 0; i < finalsBracket.transform.childCount; i++)
				{
					if (i <= 3)
						finalsBracket.transform.GetChild(i).gameObject.SetActive(true);
					else
						finalsBracket.transform.GetChild(i).gameObject.SetActive(false);
				}

				winnersBracket.SetActive(false);
				losersBracket1.SetActive(false);
				losersBracket2.SetActive(false);
				finalsBracket.SetActive(true);

				StartCoroutine(RefreshPlayoffPanel());

				playoffs.SetActive(true);

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				horizScrollBar.value = 0.3f;
				vertScrollBar.value = 0.9f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 18
			case 18:
				heading.text = "F - Round 18";

				for (int i = 0; i < finalsDisplay18.Length; i++)
				{

					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[43].x)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[43].y;
									finalsDisplay18[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay18[i].name.text = teams[j].name;
								finalsDisplay18[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[43].y)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[43].x;
									finalsDisplay18[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket Y-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay18[i].name.text = teams[j].name;
								finalsDisplay18[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				for (int i = 0; i < finalsBracket.transform.childCount; i++)
				{
					if (i <= 3)
						finalsBracket.transform.GetChild(i).gameObject.SetActive(true);
					else
						finalsBracket.transform.GetChild(i).gameObject.SetActive(false);
				}

				winnersBracket.SetActive(false);
				losersBracket1.SetActive(false);
				losersBracket2.SetActive(false);
				finalsBracket.SetActive(true);

				StartCoroutine(RefreshPlayoffPanel());

				playoffs.SetActive(true);

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				horizScrollBar.value = 0.61f;
				vertScrollBar.value = 0.95f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 19
			case 19:
				heading.text = "F - Round 19";

				for (int i = 0; i < finalsDisplay19.Length; i++)
				{
					finalsDisplay19[i].panel.transform.parent.gameObject.SetActive(true);

					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[44].x)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[44].y;
									finalsDisplay19[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay19[i].name.text = teams[j].name;
								finalsDisplay19[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[44].y)
							{
								if (teams[j].id == playerTeam)
								{
									oppTeam = (int)gameList[44].x;
									finalsDisplay19[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket Y-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay19[i].name.text = teams[j].name;
								finalsDisplay19[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}
				finalsDisplay17[0].panel.transform.parent.gameObject.SetActive(false);
				for (int i = 0; i < finalsBracket.transform.childCount; i++)
				{
					if (i <= 4)
						finalsBracket.transform.GetChild(i).gameObject.SetActive(true);
					else
						finalsBracket.transform.GetChild(i).gameObject.SetActive(false);
				}

				winnersBracket.SetActive(false);
				losersBracket1.SetActive(false);
				losersBracket2.SetActive(false);
				finalsBracket.SetActive(true);

				StartCoroutine(RefreshPlayoffPanel());

				playoffs.SetActive(true);

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				horizScrollBar.value = 0.92f;
				vertScrollBar.value = 1f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 20
			case 20:
				heading.text = "F - Round 20";

				for (int j = 0; j < teams.Length; j++)
				{
					if (teams[j].id == gameList[45].x)
					{
						Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
						finalsDisplay20[0].name.text = teams[j].name;
						finalsDisplay20[0].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
						break;
					}
				}

				for (int i = 0; i < finalsBracket.transform.childCount; i++)
				{
					finalsBracket.transform.GetChild(i).gameObject.SetActive(true);
				}

				winnersBracket.SetActive(false);
				losersBracket1.SetActive(false);
				losersBracket2.SetActive(false);
				finalsBracket.SetActive(true);

				StartCoroutine(RefreshPlayoffPanel());

				playoffs.SetActive(true);

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				horizScrollBar.value = 0.95f;
				vertScrollBar.value = 0.96f;
				//StartCoroutine(SaveCareer(true));
				break;
				#endregion
		}

		if (playerGame)
			vsDisplayGO.SetActive(true);
		else
			vsDisplayGO.SetActive(false);

		for (int i = 0; i < teams.Length; i++)
		{
			if (teams[i].id == playerTeam)
			{
				vsDisplay[0].rank.text = teams[i].wins.ToString() + "-" + teams[i].loss.ToString();
				vsDisplay[0].name.text = teams[i].name;
			}
			if (teams[i].id == oppTeam)
			{
				vsDisplay[1].rank.text = teams[i].wins.ToString() + "-" + teams[i].loss.ToString();
				vsDisplay[1].name.text = teams[i].name;
			}
		}

		StartCoroutine(RefreshPlayoffPanel());

		if (playoffRound > 0)
        {
			StartCoroutine(SaveCareer(true));
		}
		else if (playoffRound > 18)
			StartCoroutine(SaveCareer(false));
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
						if (cont & gsp.redTeamName == gameX[i].name)
                        {
							if (gsp.redScore > gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameX[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
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
						if (cont & gsp.redTeamName == gameY[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameY[i].name)
						{
							if (gsp.redScore > gsp.yellowScore)
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
						else if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
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
				//playoffRound++;
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
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

				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
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
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
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
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
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
							gameY[i].rank = 13;
							gameY[i].loss++;
						}
						else
						{
							gameList[(i / 2) + 24].x = gameY[i].id;
							gameY[i].wins++;
							gameX[i].rank = 13;
							gameX[i].loss++;
						}
					}
					else
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[((i - 1) / 2) + 24].y = gameX[i].id;
							gameX[i].wins++;
							gameY[i].rank = 13;
							gameY[i].loss++;
						}
						else
						{
							gameList[((i - 1) / 2) + 24].y = gameY[i].id;
							gameY[i].wins++;
							gameX[i].rank = 13;
							gameX[i].loss++;
						}
					}
				}
				StartCoroutine(RefreshPlayoffPanel());
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
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
							gameY[i].rank = 11;
							gameY[i].loss++;
						}
						else
						{
							gameList[32].y = gameY[i].id;
							gameY[i].wins++;
							gameX[i].rank = 11;
							gameX[i].loss++;
						}
					}
					else
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[33].y = gameX[i].id;
							gameX[i].wins++;
							gameY[i].rank = 11;
							gameY[i].loss++;
						}
						else
						{
							gameList[33].y = gameY[i].id;
							gameY[i].wins++;
							gameX[i].rank = 11;
							gameX[i].loss++;
						}
					}
				}
				StartCoroutine(RefreshPlayoffPanel());
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
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
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
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
							gameList[30].y = gameX[i].id;
							gameX[i].wins++;
							gameList[32].x = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[30].y = gameY[i].id;
							gameY[i].wins++;
							gameList[32].x = gameX[i].id;
							gameX[i].loss++;
						}
					}
					else
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[31].x = gameX[i].id;
							gameX[i].wins++;
							gameList[33].x = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[31].x = gameY[i].id;
							gameY[i].wins++;
							gameList[33].x = gameX[i].id;
							gameX[i].loss++;
						}
					}
				}
				StartCoroutine(RefreshPlayoffPanel());
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
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
							gameList[37].x = gameX[i].id;
							gameX[i].wins++;
							gameList[35].x = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[37].x = gameY[i].id;
							gameY[i].wins++;
							gameList[35].x = gameX[i].id;
							gameX[i].loss++;
						}
					}
					else
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[37].y = gameX[i].id;
							gameX[i].wins++;
							gameList[34].x = gameY[i].id;
							gameY[i].loss++;
						}
						else
						{
							gameList[37].y = gameY[i].id;
							gameY[i].wins++;
							gameList[34].x = gameX[i].id;
							gameX[i].loss++;
						}
					}
				}
				StartCoroutine(RefreshPlayoffPanel());
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
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
							gameY[i].rank = 9;
							gameY[i].loss++;
						}
						else
						{
							gameList[34].y = gameY[i].id;
							gameY[i].wins++;
							gameX[i].rank = 9;
							gameX[i].loss++;
						}
					}
					else
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[35].y = gameX[i].id;
							gameX[i].wins++;
							gameY[i].rank = 9;
							gameY[i].loss++;
						}
						else
						{
							gameList[35].y = gameY[i].id;
							gameY[i].wins++;
							gameX[i].rank = 9;
							gameX[i].loss++;
						}
					}
				}
				StartCoroutine(RefreshPlayoffPanel());
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
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
							gameList[38].x = gameX[i].id;
							gameX[i].wins++;
							gameY[i].rank = 7;
							gameY[i].loss++;
						}
						else
						{
							gameList[38].x = gameY[i].id;
							gameY[i].wins++;
							gameX[i].rank = 7;
							gameX[i].loss++;
						}
					}
					else
					{
						if (Random.Range(0, gameX[i].strength) > Random.Range(0, gameY[i].strength))
						{
							gameList[38].y = gameX[i].id;
							gameX[i].wins++;
							gameY[i].rank = 7;
							gameY[i].loss++;
						}
						else
						{
							gameList[38].y = gameY[i].id;
							gameY[i].wins++;
							gameX[i].rank = 7;
							gameX[i].loss++;
						}
					}
				}
				StartCoroutine(RefreshPlayoffPanel());
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
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
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
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
					gameList[40].y = gameX[0].id;
					gameX[0].wins++;
					gameList[39].x = gameY[0].id;
					gameY[0].loss++;
				}
				else
				{
					gameList[40].y = gameY[0].id;
					gameY[0].wins++;
					gameList[39].x = gameX[0].id;
					gameX[0].loss++;
				}

				StartCoroutine(RefreshPlayoffPanel());
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
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
					gameY[0].rank = 6;
					gameY[0].loss++;
				}
				else
				{
					gameList[39].y = gameY[0].id;
					gameY[0].wins++;
					gameX[0].rank = 6;
					gameX[0].loss++;
				}

				StartCoroutine(RefreshPlayoffPanel());
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
				break;
			#endregion
			#region Round 15
			case 15:
				gameX = new Team[2];
				gameY = new Team[2];
				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < teams.Length; j++)
					{
						if (gameList[i + 39].x == teams[j].id)
						{
							gameX[i] = teams[j];
						}
						else if (gameList[i + 39].y == teams[j].id)
						{
							gameY[i] = teams[j];
						}
					}
				}
				
				if (Random.Range(0, gameX[0].strength) > Random.Range(0, gameY[0].strength))
				{
					gameList[42].y = gameX[0].id;
					gameX[0].wins++;
					gameY[0].rank = 5;
					gameY[0].loss++;
				}
				else
				{
					gameList[42].y = gameY[0].id;
					gameY[0].wins++;
					gameY[0].rank = 5;
					gameX[0].loss++;
				}

				if (Random.Range(0, gameX[1].strength) > Random.Range(0, gameY[1].strength))
				{
					gameList[41].y = gameX[1].id;
					gameX[1].wins++;
					gameList[42].x = gameY[1].id;
					gameY[1].loss++;
				}
				else
				{
					gameList[41].y = gameY[1].id;
					gameY[1].wins++;
					gameList[42].x = gameX[1].id;
					gameX[1].loss++;
				}

				StartCoroutine(RefreshPlayoffPanel());
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
				break;
			#endregion
			#region Round 16
			case 16:
				gameX = new Team[2];
				gameY = new Team[2];

				for (int j = 0; j < teams.Length; j++)
				{
					if (gameList[41].x == teams[j].id)
					{
						gameX[0] = teams[j];
					}
					else if (gameList[41].y == teams[j].id)
					{
						gameY[0] = teams[j];
					}
				}
				if (Random.Range(0, gameX[0].strength) > Random.Range(0, gameY[0].strength))
				{
					gameList[44].x = gameX[0].id;
					gameX[0].wins++;
					gameList[43].x = gameY[0].id;
					gameY[0].loss++;
				}
				else
				{
					gameList[44].x = gameY[0].id;
					gameY[0].wins++;
					gameList[43].x = gameX[0].id;
					gameX[0].loss++;
				}

				StartCoroutine(RefreshPlayoffPanel());
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
				break;
			#endregion
			#region Round 17
			case 17:
				gameX = new Team[2];
				gameY = new Team[2];

				for (int j = 0; j < teams.Length; j++)
				{
					if (gameList[42].x == teams[j].id)
					{
						gameX[0] = teams[j];
					}
					else if (gameList[42].y == teams[j].id)
					{
						gameY[0] = teams[j];
					}
				}
				if (Random.Range(0, gameX[0].strength) > Random.Range(0, gameY[0].strength))
				{
					gameList[43].y = gameX[0].id;
					gameX[0].wins++;
					gameY[0].rank = 4;
					gameY[0].loss++;
				}
				else
				{
					gameList[43].y = gameY[0].id;
					gameY[0].wins++;
					gameX[0].rank = 4;
					gameX[0].loss++;
				}

				StartCoroutine(RefreshPlayoffPanel());
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
				break;
			#endregion
			#region Round 18
			case 18:
				gameX = new Team[2];
				gameY = new Team[2];

				for (int j = 0; j < teams.Length; j++)
				{
					if (gameList[43].x == teams[j].id)
					{
						gameX[0] = teams[j];
					}
					else if (gameList[43].y == teams[j].id)
					{
						gameY[0] = teams[j];
					}
				}
				if (Random.Range(0, gameX[0].strength) > Random.Range(0, gameY[0].strength))
				{
					gameList[44].y = gameX[0].id;
					gameX[0].wins++;
					gameY[0].rank = 3;
					gameY[0].loss++;
				}
				else
				{
					gameList[44].y = gameY[0].id;
					gameY[0].wins++;
					gameX[0].rank = 3;
					gameX[0].loss++;
				}

				StartCoroutine(RefreshPlayoffPanel());
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
				break;
			#endregion
			#region Round 19
			case 19:
				gameX = new Team[2];
				gameY = new Team[2];

				for (int j = 0; j < teams.Length; j++)
				{
					if (gameList[44].x == teams[j].id)
					{
						gameX[0] = teams[j];
					}
					else if (gameList[44].y == teams[j].id)
					{
						gameY[0] = teams[j];
					}
				}
				if (Random.Range(0, gameX[0].strength) > Random.Range(0, gameY[0].strength))
				{
					gameList[45].x = gameX[0].id;
					gameX[0].wins++;
					gameY[0].rank = 2;
					gameY[0].loss++;
				}
				else
				{
					gameList[45].x = gameY[0].id;
					gameY[0].wins++;
					gameX[0].rank = 2;
					gameX[0].loss++;
				}

				for (int i = 0; i < gameX.Length; i++)
				{
					gameX[0].tourRecord = new Vector2(gameX[0].wins, gameX[0].loss);
					gameY[0].tourRecord = new Vector2(gameY[0].wins, gameY[0].loss);
				}

				StartCoroutine(RefreshPlayoffPanel());
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
				break;
			#endregion
			#region Round 20
			case 20:
				gameX = new Team[2];
				gameY = new Team[2];

				for (int j = 0; j < teams.Length; j++)
				{
					if (gameList[44].x == teams[j].id)
					{
						gameX[0] = teams[j];
					}
					else if (gameList[44].y == teams[j].id)
					{
						gameY[0] = teams[j];
					}
				}
				if (Random.Range(0, gameX[0].strength) > Random.Range(0, gameY[0].strength))
				{
					gameList[45].x = gameX[0].id;
					gameX[0].wins++;
					gameY[0].rank = 2;
					gameY[0].loss++;
				}
				else
				{
					gameList[45].x = gameY[0].id;
					gameY[0].wins++;
					gameX[0].rank = 2;
					gameX[0].loss++;
				}

				StartCoroutine(RefreshPlayoffPanel());
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SimResults();
				break;
			#endregion
			default:
				SetPlayoffs();
				break;

		}
		yield break;
	}

	public void SimResults()
	{
		Debug.Log("Sim Results 3K - Round " + playoffRound);
		switch (playoffRound)
		{
			#region Round 1
			case 1:
				heading.text = "1 - Results";

				for (int i = 0; i < winnersDisplay3.Length; i++)
				{
					for (int j = 0; j < teams.Length; j++)
					{
						if (i % 2 == 0)
						{
							if (teams[j].id == gameList[(i / 2) + 12].x)
							{
								Debug.Log("Setting Winners Bracket X-" + teams[j].id + " - " + teams[j].name);
								winnersDisplay3[i].name.text = teams[j].name;
								winnersDisplay3[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
							}
						}
						else
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 12].y)
							{
								Debug.Log("Setting Winners Bracket Y-" + teams[j].id + " - " + teams[j].name);
								winnersDisplay3[i].name.text = teams[j].name;
								winnersDisplay3[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
							}
						}
					}
				}
				for (int i = 0; i < winnersDisplay1.Length; i++)
				{
					if (teams[i].id == gameList[Mathf.FloorToInt(i / 2f)].x | teams[i].id == gameList[Mathf.FloorToInt(i / 2f)].y)
                    {
						if (teams[i].id == gameList[Mathf.FloorToInt(i / 4f) + 12].x | teams[i].id == gameList[Mathf.FloorToInt(i / 4f) + 12].y)
						{
							winnersDisplay1[i].panel.GetComponent<Image>().color = yellow;
							Debug.Log("Who won the game? -" + teams[i].id + " - " + teams[i].name);
						}
						else
							winnersDisplay1[i].panel.GetComponent<Image>().color = dimmed;
					}
				}
				for (int i = 0; i < winnersBracket.transform.childCount; i++)
				{
					if (i <= 1)
						winnersBracket.transform.GetChild(i).gameObject.SetActive(true);
					else
						winnersBracket.transform.GetChild(i).gameObject.SetActive(false);
				}

				StartCoroutine(RefreshPlayoffPanel());

				playoffs.SetActive(true);
				playoffRound++;
                simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				horizScrollBar.value = 0;
				vertScrollBar.value = 1;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 2
			case 2:
				heading.text = "A2 - Results";

				for (int i = 0; i < losersDisplayA4.Length; i++)
				{
					if (i % 2 == 0)
					{
						losersDisplayA4[i].panel.transform.parent.gameObject.SetActive(false);
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 16].y)
							{
								Debug.Log("Setting Losers Bracket A Y-" + gameList[((i - 1) / 2) + 8].y + " - " + teams[j].name);
								losersDisplayA4[i].name.text = teams[j].name;
								losersDisplayA4[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								losersDisplayA4[i].panel.GetComponent<Image>().color = yellow;
								break;
							}
						}
					}
				}
				for (int i = 0; i < losersDisplayA2.Length; i++)
                {
					for (int j = 0; j < teams.Length; j++)
                    {
						if (i % 2 == 0)
						{
							if (teams[j].id == gameList[8 + (i / 2)].x)
							{
								if (teams[j].id == gameList[((i - 1) / 2) + 16].y)
									losersDisplayA2[i].panel.GetComponent<Image>().color = yellow;
								else
									losersDisplayA2[i].panel.GetComponent<Image>().color = dimmed;
							}
						}
						else
                        {
							if (teams[j].id == gameList[8 + ((i - 1) / 2)].y)
							{
								if (teams[j].id == gameList[((i - 1) / 2) + 16].y)
									losersDisplayA2[i].panel.GetComponent<Image>().color = yellow;
								else
									losersDisplayA2[i].panel.GetComponent<Image>().color = dimmed;
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

				StartCoroutine(RefreshPlayoffPanel());

				playoffRound++;
				playButton.gameObject.SetActive(false);
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				horizScrollBar.value = 0;
				vertScrollBar.value = 0.56f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 3
			case 3:
				heading.text = "W3 - Results";

                for (int i = 0; i < winnersDisplay3.Length; i++)
                {
                    for (int j = 0; j < teams.Length; j++)
                    {
                        if (i % 2 == 0)
                        {
                            if (teams[j].id == gameList[12 + Mathf.FloorToInt(i / 2f)].x)
                            {
                                if (teams[j].id == gameList[26 + Mathf.FloorToInt(i / 4f)].x | teams[j].id == gameList[26 + Mathf.FloorToInt(i / 4f)].y)
								{
									winnersDisplay3[i].panel.GetComponent<Image>().color = yellow;
									Debug.Log("Who won the game? -" + teams[j].id + " - " + teams[j].name);
								}
								else
									winnersDisplay3[i].panel.GetComponent<Image>().color = dimmed;
                            }
                        }
                        else
                        {
                            if (teams[j].id == gameList[12 + Mathf.FloorToInt(i / 2f)].y)
							{
								if (teams[j].id == gameList[26 + Mathf.FloorToInt(i / 4f)].x | teams[j].id == gameList[26 + Mathf.FloorToInt(i / 4f)].y)
								{
									winnersDisplay3[i].panel.GetComponent<Image>().color = yellow;
									Debug.Log("Who won the game? -" + teams[j].id + " - " + teams[j].name);
								}
                                else
                                    winnersDisplay3[i].panel.GetComponent<Image>().color = dimmed;
                            }
                        }
                    }
                }

                for (int i = 0; i < winnersDisplay7.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 26].x)
							{
								Debug.Log("Setting Winners Bracket X-" + gameList[(i / 2) + 26].x + " - " + teams[j].name);
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
								Debug.Log("Setting Winners Bracket Y-" + gameList[((i - 1) / 2) + 26].y + " - " + teams[j].name);
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

				StartCoroutine(RefreshPlayoffPanel());

				playoffRound++;
				playoffs.SetActive(true);

				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				horizScrollBar.value = 0.35f;
				vertScrollBar.value = 1f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 4
			case 4:
				heading.text = "A4 - Results";

				for (int i = 0; i < losersDisplayA4.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 16].x)
							{
								if (teams[j].id == gameList[(i / 4) + 28].x | teams[j].id == gameList[(i / 4) + 28].y)
									losersDisplayA4[i].panel.GetComponent<Image>().color = yellow;
								else
									losersDisplayA4[i].panel.GetComponent<Image>().color = dimmed;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 16].y)
							{
								if (teams[j].id == gameList[((i - 1) / 4) + 28].x | teams[j].id == gameList[((i - 1) / 4) + 28].y)
									losersDisplayA4[i].panel.GetComponent<Image>().color = yellow;
								else
									losersDisplayA4[i].panel.GetComponent<Image>().color = dimmed;
							}
						}
					}
				}

				for (int i = 0; i < losersDisplayA8.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 28].x)
							{
								Debug.Log("Setting Losers Bracket A X-" + gameList[(i / 2) + 28].x + " - " + teams[j].name);
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
								Debug.Log("Setting Losers Bracket A Y-" + gameList[((i - 1) / 2) + 28].y + " - " + teams[j].name);
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
				playoffRound++;
				playoffs.SetActive(true);

				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				horizScrollBar.value = 0.3f;
				vertScrollBar.value = 0.77f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 5
			case 5:
				heading.text = "B5 - Results";

				for (int i = 0; i < losersDisplayB5.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 20].x)
							{
								if (teams[j].id == gameList[(i / 4) + 24].x | teams[j].id == gameList[(i / 4) + 24].y)
								{
									losersDisplayB5[i].panel.GetComponent<Image>().color = yellow;
								}
								else
								{
									losersDisplayB5[i].panel.GetComponent<Image>().color = dimmed;
									losersDisplayB5[i].rank.text = "13th";
								}
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 20].y)
							{
								if (teams[j].id == gameList[(i / 4) + 24].x | teams[j].id == gameList[(i / 4) + 24].y)
								{
									losersDisplayB5[i].panel.GetComponent<Image>().color = yellow;
								}
								else
								{
									losersDisplayB5[i].panel.GetComponent<Image>().color = dimmed;
									losersDisplayB5[i].rank.text = "13th";
								}
							}
						}
					}
				}

				for (int i = 0; i < losersDisplayB6.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 24].x)
							{
								Debug.Log("Setting Losers Bracket B X-" + gameList[(i / 2) + 24].x + " - " + teams[j].name);
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
								Debug.Log("Setting Losers Bracket B Y-" + gameList[((i - 1) / 2) + 24].y + " - " + teams[j].name);
								losersDisplayB6[i].name.text = teams[j].name;
								losersDisplayB6[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				for (int i = 0; i < losersBracket1.transform.childCount; i++)
				{
					if (i <= 1)
						losersBracket2.transform.GetChild(i).gameObject.SetActive(true);
					else
						losersBracket2.transform.GetChild(i).gameObject.SetActive(false);
				}
				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				playoffs.SetActive(true);

				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				horizScrollBar.value = 0;
				vertScrollBar.value = 0.33f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 6
			case 6:
				heading.text = "B6 - Results";

				for (int i = 0; i < losersDisplayB6.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 24].x)
							{
								if (teams[j].id == gameList[(i / 2) + 32].y)
								{
									losersDisplayB6[i].panel.GetComponent<Image>().color = yellow;
								}
								else
								{
									losersDisplayB6[i].panel.GetComponent<Image>().color = dimmed;
									losersDisplayB6[i].rank.text = "11th";
								}
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 24].y)
							{
								if (teams[j].id == gameList[((i - 1) / 2) + 32].y)
								{
									losersDisplayB6[i].panel.GetComponent<Image>().color = yellow;
								}
								else
								{
									losersDisplayB6[i].panel.GetComponent<Image>().color = dimmed;
									losersDisplayB6[i].rank.text = "11th";
								}
							}
						}
					}
				}
				for (int i = 0; i < losersDisplayB10.Length; i++)
				{
					if (i % 2 == 0)
					{
						losersDisplayB10[i].panel.transform.parent.gameObject.SetActive(false);
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 32].y)
							{
								Debug.Log("Setting Losers Bracket B Y-" + gameList[((i - 1) / 2) + 32].y + " - " + teams[j].name);
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
				playoffRound++;
				playoffs.SetActive(true);

				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				horizScrollBar.value = 0.3f;
				vertScrollBar.value = 0.27f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 7
			case 7:
				heading.text = "W7 - Results";

				for (int i = 0; i < winnersDisplay7.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 26].x)
							{
								if (teams[j].id == gameList[36].x | teams[j].id == gameList[36].y)
									winnersDisplay7[i].panel.GetComponent<Image>().color = yellow;
								else
									winnersDisplay7[i].panel.GetComponent<Image>().color = dimmed;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 26].y)
							{
								if (teams[j].id == gameList[36].x | teams[j].id == gameList[36].y)
									winnersDisplay7[i].panel.GetComponent<Image>().color = yellow;
								else
									winnersDisplay7[i].panel.GetComponent<Image>().color = dimmed;
							}
						}
					}
				}

				for (int i = 0; i < 2; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[36].x)
							{
								Debug.Log("Setting Winners Bracket X-" + gameList[36].x + " - " + teams[j].name);
								winnersDisplay12[i].name.text = teams[j].name;
								winnersDisplay12[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[36].y)
							{
								Debug.Log("Setting Winners Bracket Y-" + gameList[36].y + " - " + teams[j].name);
								winnersDisplay12[i].name.text = teams[j].name;
								winnersDisplay12[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				winnersDisplay12[2].panel.transform.parent.gameObject.SetActive(false);
				for (int i = 0; i < winnersBracket.transform.childCount; i++)
				{
					if (i <= 3)
						winnersBracket.transform.GetChild(i).gameObject.SetActive(true);
					else
						winnersBracket.transform.GetChild(i).gameObject.SetActive(false);
				}

				winnersBracket.SetActive(true);
				losersBracket1.SetActive(false);
				losersBracket2.SetActive(false);
				finalsBracket.SetActive(false);

				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				playoffs.SetActive(true);

				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				horizScrollBar.value = 0.75f;
				vertScrollBar.value = 0.76f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 8
			case 8:
				heading.text = "A8 - Results";

				for (int i = 0; i < losersDisplayA8.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 28].x)
							{
								if (teams[j].id == gameList[30].y | teams[j].id == gameList[31].x)
									losersDisplayA8[i].panel.GetComponent<Image>().color = yellow;
								else
									losersDisplayA8[i].panel.GetComponent<Image>().color = dimmed;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 28].y)
							{
								if (teams[j].id == gameList[30].y | teams[j].id == gameList[31].x)
									losersDisplayA8[i].panel.GetComponent<Image>().color = yellow;
								else
									losersDisplayA8[i].panel.GetComponent<Image>().color = dimmed;
							}
						}
					}
				}
				for (int i = 0; i < 4; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 30].x)
							{
								Debug.Log("Setting Losers Bracket A X-" + gameList[(i / 2) + 30].x + " - " + teams[j].name);
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
								Debug.Log("Setting Losers Bracket A Y-" + gameList[((i - 1) / 2) + 30].y + " - " + teams[j].name);
								losersDisplayA9[i].name.text = teams[j].name;
								losersDisplayA9[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}
				for (int i = 0; i < losersBracket1.transform.childCount; i++)
				{
					if (i <= 3)
						losersBracket1.transform.GetChild(i).gameObject.SetActive(true);
					else
						losersBracket1.transform.GetChild(i).gameObject.SetActive(false);
				}

				winnersBracket.SetActive(false);
				losersBracket1.SetActive(true);
				losersBracket2.SetActive(false);
				finalsBracket.SetActive(false);

				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				horizScrollBar.value = 0.63f;
				vertScrollBar.value = 0.78f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 9
			case 9:
				heading.text = "A9 - Results";

				for (int i = 0; i < 4; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 30].x)
							{
								if (teams[j].id == gameList[37].x | teams[j].id == gameList[37].y)
									losersDisplayA9[i].panel.GetComponent<Image>().color = yellow;
								else
									losersDisplayA9[i].panel.GetComponent<Image>().color = dimmed;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 30].y)
							{
								if (teams[j].id == gameList[37].x | teams[j].id == gameList[37].y)
									losersDisplayA9[i].panel.GetComponent<Image>().color = yellow;
								else
									losersDisplayA9[i].panel.GetComponent<Image>().color = dimmed;
							}
						}
					}
				}

				for (int i = 0; i < 2; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[37].x)
							{
								Debug.Log("Setting Losers Bracket A X-" + gameList[37].x + " - " + teams[j].name);
								losersDisplayA13[i].name.text = teams[j].name;
								losersDisplayA13[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();

								if (teams[j].id == playerTeam)
									losersDisplayA13[i].bg.GetComponent<Image>().color = yellow;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[37].y)
							{
								Debug.Log("Setting Losers Bracket A Y-" + gameList[37].y + " - " + teams[j].name);
								losersDisplayA13[i].name.text = teams[j].name;
								losersDisplayA13[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();

								if (teams[j].id == playerTeam)
									losersDisplayA13[i].bg.GetComponent<Image>().color = yellow;
							}
						}
					}
				}

				losersDisplayA13[2].panel.transform.parent.gameObject.SetActive(false);

				for (int i = 0; i < losersBracket1.transform.childCount; i++)
				{
					if (i <= 4)
						losersBracket1.transform.GetChild(i).gameObject.SetActive(true);
					else
						losersBracket1.transform.GetChild(i).gameObject.SetActive(false);
				}
				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				playoffs.SetActive(true);
				horizScrollBar.value = 0.94f;
				vertScrollBar.value = 0.63f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 10
			case 10:
				heading.text = "B10 - Results";

				for (int i = 0; i < losersDisplayB10.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 32].x)
							{
								if (teams[j].id == gameList[(i / 2) + 34].y)
								{
									losersDisplayB10[i].panel.GetComponent<Image>().color = yellow;
								}
								else
								{
									losersDisplayB10[i].panel.GetComponent<Image>().color = dimmed;
									losersDisplayB10[i].rank.text = "9th";
								}
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 32].y)
							{
								if (teams[j].id == gameList[((i - 1) / 2) + 34].y)
								{
									losersDisplayB10[i].panel.GetComponent<Image>().color = yellow;
								}
								else
								{
									losersDisplayB10[i].panel.GetComponent<Image>().color = dimmed;
									losersDisplayB10[i].rank.text = "9th";
								}
							}
						}
					}
				}

				for (int i = 0; i < losersDisplayB11.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 34].x)
							{
								Debug.Log("Setting Losers Bracket B X-" + gameList[(i / 2) + 34].x + " - " + teams[j].name);
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
								Debug.Log("Setting Losers Bracket B Y-" + gameList[((i - 1) / 2) + 34].y + " - " + teams[j].name);
								losersDisplayB11[i].name.text = teams[j].name;
								losersDisplayB11[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				for (int i = 0; i < losersBracket2.transform.childCount; i++)
				{
					if (i <= 3)
						losersBracket2.transform.GetChild(i).gameObject.SetActive(true);
					else
						losersBracket2.transform.GetChild(i).gameObject.SetActive(false);
				}
				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				horizScrollBar.value = 0.62f;
				vertScrollBar.value = 0.73f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 11
			case 11:
				heading.text = "B11 - Results";

				for (int i = 0; i < losersDisplayB11.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 34].x)
							{
								if (teams[j].id == gameList[38].x | teams[j].id == gameList[38].y)
								{
									losersDisplayB11[i].panel.GetComponent<Image>().color = yellow;
								}
								else
								{
									losersDisplayB11[i].panel.GetComponent<Image>().color = dimmed;
									losersDisplayB11[i].rank.text = "7th";
								}
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 34].y)
							{
								if (teams[j].id == gameList[38].x | teams[j].id == gameList[38].y)
								{
									losersDisplayB11[i].panel.GetComponent<Image>().color = yellow;
								}
								else
								{
									losersDisplayB11[i].panel.GetComponent<Image>().color = dimmed;
									losersDisplayB11[i].rank.text = "7th";
								}
							}
						}
					}
				}

				for (int i = 0; i < 2; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[38].x)
							{
								Debug.Log("Setting Losers Bracket B X-" + gameList[38].x + " - " + teams[j].name);
								losersDisplayB14[i].name.text = teams[j].name;
								losersDisplayB14[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[38].y)
							{
								Debug.Log("Setting Losers Bracket B Y-" + gameList[38].y + " - " + teams[j].name);
								losersDisplayB14[i].name.text = teams[j].name;
								losersDisplayB14[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}

				losersDisplayB14[2].panel.transform.parent.gameObject.SetActive(false);

				for (int i = 0; i < losersBracket2.transform.childCount; i++)
				{
					if (i <= 4)
						losersBracket2.transform.GetChild(i).gameObject.SetActive(true);
					else
						losersBracket2.transform.GetChild(i).gameObject.SetActive(false);
				}

				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				horizScrollBar.value = 0.62f;
				vertScrollBar.value = 0.73f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 12
			case 12:
				heading.text = "W12 - Results";

				for (int i = 0; i < 2; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[36].x)
							{
								if (teams[j].id == gameList[41].x)
									winnersDisplay12[i].panel.GetComponent<Image>().color = yellow;
								else
									winnersDisplay12[i].panel.GetComponent<Image>().color = dimmed;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[36].y)
							{
								if (teams[j].id == gameList[41].x)
									winnersDisplay12[i].panel.GetComponent<Image>().color = yellow;
								else
									winnersDisplay12[i].panel.GetComponent<Image>().color = dimmed;
							}
						}
					}
				}
				for (int j = 0; j < teams.Length; j++)
				{
					if (teams[j].id == gameList[41].x)
					{
						Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
						winnersDisplay12[2].panel.transform.parent.gameObject.SetActive(true);
						winnersDisplay12[2].name.text = teams[j].name;
						winnersDisplay12[2].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
						break;
					}
				}
				for (int i = 0; i < winnersBracket.transform.childCount; i++)
				{
					winnersBracket.transform.GetChild(i).gameObject.SetActive(true);
				}

				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				horizScrollBar.value = 1f;
				vertScrollBar.value = 0.76f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 13
			case 13:
				heading.text = "A13 - Results";

				for (int i = 0; i < 2; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[37].x)
							{
								if (teams[j].id == gameList[40].y)
									losersDisplayA13[i].panel.GetComponent<Image>().color = yellow;
								else
									losersDisplayA13[i].panel.GetComponent<Image>().color = dimmed;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[37].y)
							{
								if (teams[j].id == gameList[40].y)
									losersDisplayA13[i].panel.GetComponent<Image>().color = yellow;
								else
									losersDisplayA13[i].panel.GetComponent<Image>().color = dimmed;
							}
						}
					}
				}
				for (int j = 0; j < teams.Length; j++)
				{
					if (teams[j].id == gameList[40].y)
					{
						Debug.Log("Setting Losers Bracket A Y-" + gameList[40].y + " - " + teams[j].name);
						losersDisplayA13[2].panel.transform.parent.gameObject.SetActive(true);
						losersDisplayA13[2].name.text = teams[j].name;
						losersDisplayA13[2].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
						break;
					}
				}
				for (int i = 0; i < losersBracket1.transform.childCount; i++)
				{
					losersBracket1.transform.GetChild(i).gameObject.SetActive(true);
				}

				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				horizScrollBar.value = 0.93f;
				vertScrollBar.value = 1f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 14
			case 14:
				heading.text = "B14 - Results";

				for (int i = 0; i < 2; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[38].x)
							{
								if (teams[j].id == gameList[39].y)
								{
									losersDisplayB14[i].panel.GetComponent<Image>().color = yellow;
								}
								else
								{
									losersDisplayB14[i].panel.GetComponent<Image>().color = dimmed;
									losersDisplayB14[i].rank.text = "6th";
								}
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[38].y)
							{
								if (teams[j].id == gameList[39].y)
								{
									losersDisplayB14[i].panel.GetComponent<Image>().color = yellow;
								}
								else
								{
									losersDisplayB14[i].panel.GetComponent<Image>().color = dimmed;
									losersDisplayB14[i].rank.text = "6th";
								}
							}
						}
					}
				}
				for (int j = 0; j < teams.Length; j++)
				{
					if (teams[j].id == gameList[39].y)
					{
						Debug.Log("Setting Losers Bracket B Y-" + gameList[38].y + " - " + teams[j].name);
						losersDisplayB14[2].panel.transform.parent.gameObject.SetActive(true);
						losersDisplayB14[2].name.text = teams[j].name;
						losersDisplayB14[2].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
						break;
					}
				}
				for (int i = 0; i < losersBracket2.transform.childCount; i++)
				{
					losersBracket2.transform.GetChild(i).gameObject.SetActive(true);
				}

				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				horizScrollBar.value = 0.93f;
				vertScrollBar.value = 1f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 15
			case 15:
				heading.text = "F15 - Results";

				for (int i = 1; i < finalsDisplay15.Length; i++)
				{
					if(i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[40].y)
							{
								if (teams[j].id == gameList[41].y)
									finalsDisplay15[1].panel.GetComponent<Image>().color = yellow;
								else
									finalsDisplay15[1].panel.GetComponent<Image>().color = dimmed;
							}
							if (teams[j].id == gameList[39].y)
							{
								if (teams[j].id == gameList[42].y)
								{
									finalsDisplay15[3].panel.GetComponent<Image>().color = yellow;
								}
								else
								{
									finalsDisplay15[3].panel.GetComponent<Image>().color = dimmed;
									finalsDisplay15[3].rank.text = "5th";
								}
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[40].x)
							{
								if (teams[j].id == gameList[41].y)
									finalsDisplay15[1].panel.GetComponent<Image>().color = yellow;
								else
									finalsDisplay15[1].panel.GetComponent<Image>().color = dimmed;
							}
							if (teams[j].id == gameList[39].x)
							{
								if (teams[j].id == gameList[42].y)
								{
									finalsDisplay15[3].panel.GetComponent<Image>().color = yellow;
								}
								else
								{
									finalsDisplay15[3].panel.GetComponent<Image>().color = dimmed;
									finalsDisplay15[3].rank.text = "5th";
								}
							}
						}
					}
				}
				for (int i = 0; i < 3; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[41].x && i == 0)
							{
								finalsDisplay16[i].panel.transform.parent.gameObject.SetActive(false);
							}
							if (teams[j].id == gameList[42].y && i == 2)
							{
								Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay16[i].panel.transform.parent.gameObject.SetActive(true);
								finalsDisplay16[i].name.text = teams[j].name;
								finalsDisplay16[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[41].y)
							{
								Debug.Log("Setting Finals Bracket Y-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay16[i].panel.transform.parent.gameObject.SetActive(true);
								finalsDisplay16[i].name.text = teams[j].name;
								finalsDisplay16[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}
				for (int j = 0; j < teams.Length; j++)
				{
					if (teams[j].id == gameList[42].x)
					{
						Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
						finalsDisplay17[1].panel.transform.parent.gameObject.SetActive(true);
						finalsDisplay17[1].name.text = teams[j].name;
						finalsDisplay17[1].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
						break;
					}
				}
				finalsDisplay17[0].panel.transform.parent.gameObject.SetActive(false);
				finalsDisplay17[2].panel.transform.parent.gameObject.SetActive(false);
				for (int i = 0; i < finalsBracket.transform.childCount; i++)
				{
					if (i <= 2)
						finalsBracket.transform.GetChild(i).gameObject.SetActive(true);
					else
						finalsBracket.transform.GetChild(i).gameObject.SetActive(false);
				}

				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				horizScrollBar.value = 0;
				vertScrollBar.value = 0.9f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 16
			case 16:
				heading.text = "F16 - Results";

				for (int i = 0; i < 2; i++)
				{
					finalsDisplay16[i].panel.transform.parent.gameObject.SetActive(true);
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[44].x && teams[j].id == gameList[41].x)
							{
								finalsDisplay16[i].panel.GetComponent<Image>().color = yellow;
								break;
							}
							else
								finalsDisplay16[i].panel.GetComponent<Image>().color = dimmed;
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[44].x && teams[j].id == gameList[41].y)
							{
								finalsDisplay16[i].panel.GetComponent<Image>().color = yellow;
								break;
							}
							else
								finalsDisplay16[i].panel.GetComponent<Image>().color = dimmed;
						}
					}
				}

				for (int i = 0; i < finalsDisplay17.Length; i++)
				{
					for (int j = 0; j < teams.Length; j++)
					{
						if (i == 0 && teams[j].id == gameList[44].x)
						{
							Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
							finalsDisplay17[i].panel.transform.parent.gameObject.SetActive(true);
							finalsDisplay17[i].name.text = teams[j].name;
							finalsDisplay17[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
							break;
						}
					}
				}
				finalsDisplay17[2].panel.transform.parent.gameObject.SetActive(false);

				for (int j = 0; j < teams.Length; j++)
				{
					if (teams[j].id == gameList[43].x)
					{
						Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
						finalsDisplay18[0].panel.transform.parent.gameObject.SetActive(true);
						finalsDisplay18[0].name.text = teams[j].name;
						finalsDisplay18[0].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
						break;
					}
				}
				finalsDisplay18[1].panel.transform.parent.gameObject.SetActive(false);
				for (int i = 0; i < finalsBracket.transform.childCount; i++)
				{
					if (i <= 3)
						finalsBracket.transform.GetChild(i).gameObject.SetActive(true);
					else
						finalsBracket.transform.GetChild(i).gameObject.SetActive(false);
				}

				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				horizScrollBar.value = 0;
				vertScrollBar.value = 0.92f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 17
			case 17:
				heading.text = "F17 - Results";

				for (int i = 0; i < finalsDisplay17.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (i == 2 && teams[j].id == gameList[42].y)
							{
								if (teams[j].rank == 4)
								{
									finalsDisplay17[i].panel.GetComponent<Image>().color = dimmed;
									finalsDisplay17[i].rank.text = "4th";
								}
								else
									finalsDisplay17[i].panel.GetComponent<Image>().color = yellow;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[42].x)
							{
								if (teams[j].rank == 4)
								{
									finalsDisplay17[i].panel.GetComponent<Image>().color = dimmed;
									finalsDisplay17[i].rank.text = "4th";
								}
								else
									finalsDisplay17[i].panel.GetComponent<Image>().color = yellow;
							}
						}
					}
				}
				for (int i = 0; i < finalsDisplay18.Length; i++)
				{
					finalsDisplay18[i].panel.transform.parent.gameObject.SetActive(true);
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[43].x)
							{
								Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay18[i].name.text = teams[j].name;
								finalsDisplay18[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[43].y)
							{
								Debug.Log("Setting Finals Bracket Y-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay18[i].name.text = teams[j].name;
								finalsDisplay18[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}
				for (int i = 0; i < finalsBracket.transform.childCount; i++)
				{
					if (i <= 3)
						finalsBracket.transform.GetChild(i).gameObject.SetActive(true);
					else
						finalsBracket.transform.GetChild(i).gameObject.SetActive(false);
				}

				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				horizScrollBar.value = 0.3f;
				vertScrollBar.value = 0.9f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 18
			case 18:
				heading.text = "Semifinal Results";

				for (int i = 0; i < finalsDisplay18.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[43].x)
							{
								if (teams[j].loss > 2)
								{
									finalsDisplay18[i].panel.GetComponent<Image>().color = dimmed;
									finalsDisplay18[i].rank.text = "3rd";
								}
								else
									finalsDisplay18[i].panel.GetComponent<Image>().color = yellow;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[43].y)
							{
								if (teams[j].loss > 2)
								{
									finalsDisplay18[i].panel.GetComponent<Image>().color = dimmed;
									finalsDisplay18[i].rank.text = "3rd";
								}
								else
									finalsDisplay18[i].panel.GetComponent<Image>().color = yellow;
							}
						}
					}
				}
				for (int i = 0; i < finalsDisplay19.Length; i++)
				{
					if (i % 2 == 0)
					{
						finalsDisplay19[0].panel.transform.parent.gameObject.SetActive(false);
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[44].y)
							{
								Debug.Log("Setting Finals Bracket Y-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay19[i].name.text = teams[j].name;
								finalsDisplay19[i].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
								break;
							}
						}
					}
				}
				for (int i = 0; i < finalsBracket.transform.childCount; i++)
				{
					if (i <= 4)
						finalsBracket.transform.GetChild(i).gameObject.SetActive(true);
					else
						finalsBracket.transform.GetChild(i).gameObject.SetActive(false);
				}

				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				horizScrollBar.value = 0.61f;
				vertScrollBar.value = 0.95f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 19
			case 19:
				heading.text = "Final Results";

				for (int i = 0; i < finalsDisplay19.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[44].x)
							{
								if (teams[j].rank == 2)
								{
									finalsDisplay19[i].panel.GetComponent<Image>().color = dimmed;
									finalsDisplay19[i].rank.text = "2nd";
								}
								else
									finalsDisplay19[i].panel.GetComponent<Image>().color = yellow;
							}
						}
					}
					else
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[44].y)
							{
								if (teams[j].rank == 2)
								{
									finalsDisplay19[i].panel.GetComponent<Image>().color = dimmed;
									finalsDisplay19[i].rank.text = "2nd";
								}
								else
									finalsDisplay19[i].panel.GetComponent<Image>().color = yellow;
							}
						}
					}
				}

				for (int j = 0; j < teams.Length; j++)
				{
					if (teams[j].id == gameList[45].x)
					{
						Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
						finalsDisplay20[0].name.text = teams[j].name;
						finalsDisplay20[0].rank.text = teams[j].wins.ToString() + "-" + teams[j].loss.ToString();
						break;
					}
				}

				for (int i = 0; i < finalsBracket.transform.childCount; i++)
				{
					finalsBracket.transform.GetChild(i).gameObject.SetActive(true);
				}

				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				horizScrollBar.value = 0.92f;
				vertScrollBar.value = 1f;
				//StartCoroutine(SaveCareer(true));
				contButton.gameObject.SetActive(false);
				playButton.gameObject.SetActive(false);
				nextButton.gameObject.SetActive(true);
				break;
			#endregion
		}

		for (int i = 0; i < teams.Length; i++)
		{
			if (teams[i].id == playerTeam)
			{
				vsDisplay[0].rank.text = teams[i].wins.ToString() + "-" + teams[i].loss.ToString();
				vsDisplay[0].name.text = teams[i].name;
			}
			if (teams[i].id == oppTeam)
			{
				vsDisplay[1].rank.text = teams[i].wins.ToString() + "-" + teams[i].loss.ToString();
				vsDisplay[1].name.text = teams[i].name;
			}
		}

        if (playoffRound < 15)
        {
            StartCoroutine(SimToFinals());
        }
    }
	public void PlayRound()
	{
		gsp.TournyKOSetup();
		SceneManager.LoadScene("End_Menu_Tourny_1");
	}
	public void TournyComplete()
	{
		CareerManager cm = FindObjectOfType<CareerManager>();
		gsp.teams = teams;
		cm.earnings = gsp.earnings;
		cm.record = gsp.record;
		gsp.draw = 0;
		gsp.playoffRound = 0;
		gsp.inProgress = false;
		Debug.Log("CM Record is " + cm.record.x + " - " + cm.record.y);
		Debug.Log("CM earnings are " + cm.earnings);
		cm.TournyResults();
		SceneManager.LoadScene("Arena_Selector");
	}
	IEnumerator ResetBrackets(int poRound)
	{
		for (int i = 0; i < poRound; i++)
		{
			playoffRound = i;
			yield return new WaitForSeconds(0.01f);
			SetPlayoffs();
			yield return new WaitForSeconds(0.01f);
			SimResults();
			yield return new WaitForSeconds(0.01f);
		}
		//playoffRound = poRound;
	}
	IEnumerator SimToFinals()
	{
		yield return new WaitForSeconds(0.05f);
		SetPlayoffs();
		yield return new WaitForSeconds(0.05f);
		OnSim();
		yield return new WaitForSeconds(0.05f);
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

		myFile.Add("Knockout Tourny", gsp.KO);
		myFile.Add("Career Record", gsp.record);
		Debug.Log("gsp.record is " + gsp.record.x + " - " + gsp.record.y);
		
		myFile.Add("Career Earnings", gsp.earnings);

		myFile.Add("Tourny In Progress", inProgress);
		gsp.inProgress = inProgress;
		myFile.Add("Number Of Teams", 16);
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

		myFile.Add("Tourny Name List", nameList);
		myFile.Add("Tourny Wins List", winsList);
		myFile.Add("Tourny Loss List", lossList);
		myFile.Add("Tourny Rank List", rankList);
		myFile.Add("Tourny NextOpp List", nextOppList);
		myFile.Add("Tourny Strength List", strengthList);
		myFile.Add("Tourny Team ID List", idList);

		int[] gameListX = new int[gameList.Length];
		int[] gameListY = new int[gameList.Length];

		for (int i = 0; i < gameList.Length; i++)
		{
			//Debug.Log("playoffID i is " + i);
			gameListX[i] = (int)gameList[i].x;
			gameListY[i] = (int)gameList[i].y;
		}

		myFile.Add("Tourny Game X List", gameListX);
		myFile.Add("Tourny Game Y List", gameListY);
		//yield return myFile.TestDataSaveLoad();

		yield return myFile.Append();
	}

	IEnumerator RefreshPlayoffPanel()
	{
		for (int i = 0; i < winnersBracket.transform.childCount; i++)
		{
			for (int j = 0; j < winnersBracket.transform.GetChild(i).childCount; j++)
			{
				for (int k = 1; k < 3; k++)
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
				for (int k = 1; k < 3; k++)
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
				for (int k = 1; k < 3; k++)
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
				for (int k = 1; k < 3; k++)
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

	public void Menu()
	{
		//StartCoroutine(SaveCareer());

		SceneManager.LoadScene("SplashMenu");
	}
}
