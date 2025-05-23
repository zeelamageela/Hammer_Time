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

	public float waitTime;
	public GameObject winnersBracket;
	public GameObject losersBracket1;
	public GameObject losersBracket2;
	public GameObject finalsBracket;

	public Color green;
	public Color red;
	public Color yellow;
	public Color[] yellows;
	public Color yellow1;
	public Color yellow2;
	public Color yellow3;
	public Color dimmed;
	public Color[] dims;
	public Color dim1;
	public Color dim2;
	public Color dim3;

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

	bool simInProgress;

	private void Start()
	{
		gsp = FindObjectOfType<GameSettingsPersist>();
		cm = FindObjectOfType<CareerManager>();

		StartCoroutine(LoadCareer());
		Debug.Log("Career Earnings before playoffs - $ " + gsp.tournyEarnings.ToString());

		teams = new Team[16];
		gameList = new Vector2[46];
		playoffs.SetActive(true);

		if (cm)
		{
			playerTeam = cm.playerTeamIndex;
			Debug.Log("Player Team Index - " + cm.playerTeamIndex);
		}
		if (gsp.careerLoad)
		{
			Debug.Log("Career Load P3K");
			//gsp.LoadCareer();
			careerEarnings = gsp.tournyEarnings;

            if (cm)
            {
                playerTeam = cm.playerTeamIndex;
                Debug.Log("Career Load - Player Team Index - " + cm.playerTeamIndex);
            }
            if (gsp.tournyInProgress)
			{
				gsp.LoadKOTourny();
				gsp.tournyInProgress = false;
				Debug.Log("gsp.inProgress is " + gsp.tournyInProgress);
				gsp.careerLoad = false;
				//Debug.Log("Playoff Round BEFORE the minus - " + gsp.playoffRound);
				//gsp.playoffRound--;
				//Debug.Log("Playoff Round AFTER the minus - " + gsp.playoffRound);
				LoadPlayoff();
			}
		}
		else
		{
			//playerTeam = tm.playerTeam;
			playoffRound = gsp.playoffRound;
		}

		Debug.Log("Career Earnings before playoffs - $ " + gsp.tournyEarnings.ToString());

		if (playoffRound > 0)
			LoadPlayoff();
		else
			SetSeeding();

		//SetBrackets();
	}

    public void SetSeeding()
    {
        // If loading a tournament in progress, use saved teams
        if (gsp.teams != null && gsp.teams.Length == teams.Length)
        {
            for (int i = 0; i < teams.Length; i++)
            {
                teams[i] = gsp.teams[i];
            }
        }
        else
        {
            // Otherwise, seed from CareerManager's teamPool
            for (int i = 0; i < teams.Length; i++)
            {
                teams[i] = cm.teamPool[i];
            }
        }

        heading.text = "No Game Data";
        Debug.Log("-->playoffRound is " + playoffRound);
        playoffRound++;

        gsp.tournyRecord = new Vector2(0f, 0f);

        cm.teamRecords = new Vector4[teams.Length];
        cm.tourRecords = new Vector4[teams.Length];

        for (int i = 0; i < teams.Length; i++)
        {
            cm.teamRecords[i].x = teams[i].wins;
            cm.teamRecords[i].y = teams[i].loss;
            cm.teamRecords[i].z = teams[i].earnings;
            cm.teamRecords[i].w = teams[i].id;

            cm.tourRecords[i].x = teams[i].tourRecord.x;
            cm.tourRecords[i].y = teams[i].tourRecord.y;
            cm.tourRecords[i].z = teams[i].tourPoints;
            cm.tourRecords[i].w = teams[i].id;

            teams[i].wins = 0;
            teams[i].loss = 0;
            teams[i].earnings = 0;
            teams[i].tourPoints = 0;
            teams[i].tourRecord = Vector2.zero;
        }

        Debug.Log("Tour Record - " + teams[0].name + " - " + cm.tourRecords[0]);

        gsp.teams = teams;

        for (int i = 0; i < teams.Length; i++)
        {
            // Use the team's existing strength property
            if (!teams[i].player)
                teams[i].strength = Random.Range(0, 10);

            if (i % 2 == 0 && i + 1 < teams.Length)
                gameList[i / 2] = new Vector2(teams[i].id, teams[i + 1].id);
        }

        SetPlayoffs();
    }

    void LoadPlayoffs()
	{
		TournyTeamList tTeamList = FindObjectOfType<TournyTeamList>();
		myFile = new EasyFileSave("my_player_data");
		if (myFile.Load())
		{
			Debug.Log("Load Playoffs - Round " + playoffRound);
            //teams = gsp.teams;
			int[] winsList = myFile.GetArray<int>("Tourny Wins List");
			int[] lossList = myFile.GetArray<int>("Tourny Loss List");
			int [] rankList = myFile.GetArray<int>("Tourny Rank List");
			string[] nextOppList = myFile.GetArray<string>("Tourny NextOpp List");
			int[] strengthList = myFile.GetArray<int>("Tourny Strength List");
			int[] idList = myFile.GetArray<int>("Tourny Team ID List");
			bool[] playerList = myFile.GetArray<bool>("Tourny Player List");
			float[] earningsList = myFile.GetArray<float>("Tourny Earnings List");
			float[] tourPointsList = myFile.GetArray<float>("Tourny Tour Points List");
			//StartCoroutine(Wait());
			for (int i = 0; i < teams.Length; i++)
			{
				teams[i] = gsp.teams[i];
			}
			for (int i = 0; i < teams.Length; i++)
			{
				teams[i].id = idList[i];
				teams[i].wins = winsList[i];
				//Debug.Log("Wins List is " + teams[i].wins);
				teams[i].loss = lossList[i];
				//Debug.Log("Loss List is " + lossList[i]);
				teams[i].rank = rankList[i];
				teams[i].nextOpp = nextOppList[i];
				teams[i].strength = strengthList[i];
				teams[i].player = playerList[i];
				teams[i].earnings = earningsList[i];
				teams[i].tourPoints = tourPointsList[i];


			}
			for (int i = 0; i < teams.Length; i++)
			{
				if (teams[i].player)
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
			playerTeam = gsp.playerTeamIndex;
			Debug.Log("We are returning from a game");
			cont = true;
			//playoffRound--;
			//StartCoroutine(ResetBrackets(gsp.playoffRound));
			//StartCoroutine(SimPlayoff());
		}
		StartCoroutine(ResetBrackets(gsp.playoffRound));
		//SetPlayoffs();
	}
    
    public void LoadPlayoff()
    {
        if (cm != null)
        {
            Team[] loadedTeams;
            int loadedRound;
            Vector2[] loadedGameList;
            cm.LoadPlayoffState(out loadedTeams, out loadedRound, out loadedGameList);

            if (loadedTeams != null)
            {
                teams = loadedTeams;
                playoffRound = loadedRound;
                gameList = loadedGameList;
            }
        }
    }
    public void SavePlayoff()
    {
        if (cm != null)
            cm.SavePlayoffState(teams, playoffRound, gameList);
    }

    public void SetPlayoffs()
	{
		Debug.Log("Set Playoffs 3K - Round " + playoffRound);
		bool playerGame = false;
		vsDisplayTitle.text = "Next Game";
		switch (playoffRound)
		{
			#region Round 1
			case 1:
				heading.text = "Triple Knockout - Round 1";
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
					if (teams[i].player)
					{
						if (teams[i].loss == 2)
						{
							vsDisplay[0].rank.text = "OXX";
						}
						if (teams[i].loss == 1)
						{
							vsDisplay[0].rank.text = "00X";
						}
						if (teams[i].loss == 0)
						{
							vsDisplay[0].rank.text = "000";
						}
						vsDisplay[0].name.text = teams[i].name;
					}
					if (teams[i].id == oppTeam)
					{
                        if (teams[i].loss == 2)
                        {
                            vsDisplay[0].rank.text = "OXX";
                        }
                        if (teams[i].loss == 1)
                        {
                            vsDisplay[0].rank.text = "00X";
                        }
                        if (teams[i].loss == 0)
                        {
                            vsDisplay[0].rank.text = "000";
                        }
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
				horizScrollBar.value = 0;
				//StartCoroutine(SaveCareer(true));
				break;
#endregion
            #region Round 2
            case 2:
				heading.text = "Loser's Bracket - Draw 2";

				for (int i = 0; i < losersDisplayA2.Length; i++)
				{
					for (int j = 0; j < teams.Length; j++)
					{
						if (i % 2 == 0)
						{
							if (teams[j].id == gameList[(i / 2) + 8].x)
							{
								if (teams[j].player)
								{
									oppTeam = (int)gameList[(i / 2) + 8].y;
									losersDisplayA2[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A X-" + gameList[(i / 2) + 8].x + " - " + teams[j].name);
								losersDisplayA2[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayA2[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayA2[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayA2[i].rank.text = "000";
                                }
                                break;
							}
						}
						else
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 8].y)
							{
								if (teams[j].player)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 8].x;
									losersDisplayA2[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A Y-" + gameList[((i - 1) / 2) + 8].y + " - " + teams[j].name);
								losersDisplayA2[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayA2[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayA2[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayA2[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0;
				vertScrollBar.value = 0.56f;
				//StartCoroutine(SaveCareer(true));
				break;
#endregion
            #region Round 3
            case 3:
				heading.text = "Winner's Bracket - Draw 3";

				for (int i = 0; i < winnersDisplay3.Length; i++)
				{
					for (int j = 0; j < teams.Length; j++)
					{
						if (i % 2 == 0)
						{
							if (teams[j].id == gameList[(i / 2) + 12].x)
							{
								if (teams[j].player)
								{
									oppTeam = (int)gameList[(i / 2) + 12].y;
									winnersDisplay3[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Winners Bracket X-" + gameList[(i / 2) + 12].x + " - " + teams[j].name);
								winnersDisplay3[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    winnersDisplay3[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    winnersDisplay3[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    winnersDisplay3[i].rank.text = "000";
                                }
                            }
						}
						else
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 12].y)
							{
								if (teams[j].player)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 12].x;
									winnersDisplay3[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Winners Bracket Y-" + gameList[((i - 1) / 2) + 12].x + " - " + teams[j].name);
								winnersDisplay3[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    winnersDisplay3[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    winnersDisplay3[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    winnersDisplay3[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0;
				vertScrollBar.value = 1f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 4
			case 4:
				heading.text = "Loser's Bracket - Draw 4";

				for (int i = 0; i < losersDisplayA4.Length; i++)
				{
					losersDisplayA4[i].panel.transform.parent.gameObject.SetActive(true);
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 16].x)
							{
								if (teams[j].player)
								{
									oppTeam = (int)gameList[(i / 2) + 16].y;
									losersDisplayA4[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A X-" + gameList[(i / 2) + 16].x + " - " + teams[j].name);
								losersDisplayA4[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayA4[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayA4[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayA4[i].rank.text = "000";
                                }
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
								if (teams[j].player)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 16].x;
									losersDisplayA4[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A Y-" + gameList[((i - 1) / 2) + 16].y + " - " + teams[j].name);
								losersDisplayA4[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayA4[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayA4[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayA4[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0;
				vertScrollBar.value = 0.74f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 5
			case 5:
				heading.text = "Knockout Bracket - Draw 5";

				for (int i = 0; i < losersDisplayB5.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 20].x)
							{
								if (teams[j].player)
								{
									oppTeam = (int)gameList[(i / 2) + 20].y;
									losersDisplayB5[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B X-" + gameList[(i / 2) + 20].x + " - " + teams[j].name);
								losersDisplayB5[i].name.text = teams[j].name;

                                if (teams[j].loss == 2)
                                {
                                    losersDisplayB5[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayB5[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayB5[i].rank.text = "000";
                                }
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
								if (teams[j].player)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 20].x;
									losersDisplayB5[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B Y-" + gameList[((i - 1) / 2) + 20].y + " - " + teams[j].name);
								losersDisplayB5[i].name.text = teams[j].name;

                                if (teams[j].loss == 2)
                                {
                                    losersDisplayB5[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayB5[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayB5[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0;
				vertScrollBar.value = 0.33f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 6
			case 6:
				heading.text = "Knockout Bracket - Draw 6";

				for (int i = 0; i < losersDisplayB6.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 24].x)
							{
								if (teams[j].player)
								{
									oppTeam = (int)gameList[(i / 2) + 24].y;
									losersDisplayB6[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B X-" + gameList[(i / 2) + 24].x + " - " + teams[j].name);
								losersDisplayB6[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayB6[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayB6[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayB6[i].rank.text = "000";
                                }
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
								if (teams[j].player)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 24].x;
									losersDisplayB6[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B Y-" + gameList[((i - 1) / 2) + 24].y + " - " + teams[j].name);
								losersDisplayB6[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayB6[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayB6[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayB6[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0;
				vertScrollBar.value = 0.3f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 7
			case 7:
				heading.text = "Winner's Bracket - Draw 7";

				for (int i = 0; i < winnersDisplay7.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 26].x)
							{
								if (teams[j].player)
								{
									oppTeam = (int)gameList[(i / 2) + 26].y;
									winnersDisplay7[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Winners Bracket X-" + gameList[(i / 2) + 26].x + " - " + teams[j].name);
								winnersDisplay7[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    winnersDisplay7[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    winnersDisplay7[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    winnersDisplay7[i].rank.text = "000";
                                }
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
								if (teams[j].player)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 26].x;
									winnersDisplay7[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Winners Bracket Y-" + gameList[((i - 1) / 2) + 26].y + " - " + teams[j].name);
								winnersDisplay7[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    winnersDisplay7[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    winnersDisplay7[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    winnersDisplay7[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0.35f;
				vertScrollBar.value = 1f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 8
			case 8:
				heading.text = "Loser's Bracket - Draw 8";

				for (int i = 0; i < losersDisplayA8.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 28].x)
							{
								if (teams[j].player)
								{
									oppTeam = (int)gameList[(i / 2) + 28].y;
									losersDisplayA8[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A X-" + gameList[(i / 2) + 28].x + " - " + teams[j].name);
								losersDisplayA8[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayA8[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayA8[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayA8[i].rank.text = "000";
                                }
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
								if (teams[j].player)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 28].x;
									losersDisplayA8[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A Y-" + gameList[((i - 1) / 2) + 28].y + " - " + teams[j].name);
								losersDisplayA8[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayA8[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayA8[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayA8[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0.3f;
				vertScrollBar.value = 0.77f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 9
			case 9:
				heading.text = "Loser's Bracket - Draw 9";

				for (int i = 0; i < 4; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 30].x)
							{
								if (teams[j].player)
								{
									oppTeam = (int)gameList[(i / 2) + 30].y;
									losersDisplayA9[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A X-" + gameList[(i / 2) + 30].x + " - " + teams[j].name);
								losersDisplayA9[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayA9[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayA9[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayA9[i].rank.text = "000";
                                }
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
								if (teams[j].player)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 30].x;
									losersDisplayA9[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A Y-" + gameList[((i - 1) / 2) + 30].y + " - " + teams[j].name);
								losersDisplayA9[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayA9[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayA9[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayA9[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0.63f;
				vertScrollBar.value = 1f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 10
			case 10:
				heading.text = "Knockout Bracket - Round 10";

				for (int i = 0; i < losersDisplayB10.Length; i++)
				{
					losersDisplayB10[i].panel.transform.parent.gameObject.SetActive(true);
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 32].x)
							{
								if (teams[j].player)
								{
									oppTeam = (int)gameList[(i / 2) + 32].y;
									losersDisplayB10[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B X-" + gameList[(i / 2) + 32].x + " - " + teams[j].name);
								losersDisplayB10[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayB10[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayB10[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayB10[i].rank.text = "000";
                                }
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
								if (teams[j].player)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 32].x;
									losersDisplayB10[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B Y-" + gameList[((i - 1) / 2) + 32].y + " - " + teams[j].name);
								losersDisplayB10[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayB10[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayB10[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayB10[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0.3f;
				vertScrollBar.value = 0.76f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 11
			case 11:
				heading.text = "Knockout Bracket - Draw 11";

				for (int i = 0; i < losersDisplayB11.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[(i / 2) + 34].x)
							{
								if (teams[j].player)
								{
									oppTeam = (int)gameList[(i / 2) + 34].y;
									losersDisplayB11[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B X-" + gameList[(i / 2) + 34].x + " - " + teams[j].name);
								losersDisplayB11[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayB11[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayB11[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayB11[i].rank.text = "000";
                                }
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
								if (teams[j].player)
								{
									oppTeam = (int)gameList[((i - 1) / 2) + 34].x;
									losersDisplayB11[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B Y-" + gameList[((i - 1) / 2) + 34].y + " - " + teams[j].name);
								losersDisplayB11[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayB11[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayB11[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayB11[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0.61f;
				vertScrollBar.value = 1f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 12
			case 12:
				heading.text = "Winner's Bracket - Draw 12";

				for (int i = 0; i < 2; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[36].x)
							{
								if (teams[j].player)
								{
									oppTeam = (int)gameList[36].y;
									winnersDisplay12[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Winners Bracket X-" + gameList[36].x + " - " + teams[j].name);
								winnersDisplay12[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    winnersDisplay12[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    winnersDisplay12[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    winnersDisplay12[i].rank.text = "000";
                                }
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
								if (teams[j].player)
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
				horizScrollBar.value = 0.75f;
				vertScrollBar.value = 0.76f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 13
			case 13:
				heading.text = "Loser's Bracket - Draw 13";

				for (int i = 0; i < 2; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[37].x)
							{
								if (teams[j].player)
								{
									oppTeam = (int)gameList[37].y;
									losersDisplayA13[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A X-" + gameList[37].x + " - " + teams[j].name);
								losersDisplayA13[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayA13[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayA13[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayA13[i].rank.text = "000";
                                }
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
								if (teams[j].player)
								{
									oppTeam = (int)gameList[37].x;
									losersDisplayA13[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket A Y-" + gameList[37].y + " - " + teams[j].name);
								losersDisplayA13[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayA13[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayA13[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayA13[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0.93f;
				vertScrollBar.value = 1f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 14
			case 14:
				heading.text = "Knockout Bracket - Draw 14";

				for (int i = 0; i < 2; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[38].x)
							{
								if (teams[j].player)
								{
									oppTeam = (int)gameList[38].y;
									losersDisplayB14[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B X-" + gameList[38].x + " - " + teams[j].name);
								losersDisplayB14[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayB14[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayB14[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayB14[i].rank.text = "000";
                                }
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
								if (teams[j].player)
								{
									oppTeam = (int)gameList[38].x;
									losersDisplayB14[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Losers Bracket B Y-" + gameList[38].y + " - " + teams[j].name);
								losersDisplayB14[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayB14[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayB14[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayB14[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0.93f;
				vertScrollBar.value = 1f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 15
			case 15:
				heading.text = "Final Bracket - Draw 15";

				for (int i = 0; i < finalsDisplay15.Length; i++)
				{
					if (i == 0)
                    {
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[41].x)
							{
								if (teams[j].player)
								{
									oppTeam = 99;
									finalsDisplay15[i].bg.GetComponent<Image>().color = yellow;
								}

								Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay15[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    finalsDisplay15[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    finalsDisplay15[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    finalsDisplay15[i].rank.text = "000";
                                }
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
								if (teams[j].player)
								{
									oppTeam = (int)gameList[41 - (i / 2)].x;
									finalsDisplay15[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay15[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    finalsDisplay15[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    finalsDisplay15[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    finalsDisplay15[i].rank.text = "000";
                                }
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
								if (teams[j].player)
								{
									oppTeam = (int)gameList[41 - ((i + 1) / 2)].y;
									finalsDisplay15[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket Y-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay15[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    finalsDisplay15[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    finalsDisplay15[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    finalsDisplay15[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0;
				vertScrollBar.value = 0.9f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 16
			case 16:
				heading.text = "Final Bracket - Draw 16";

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
								if (teams[j].player)
								{
									oppTeam = (int)gameList[41].y;
									finalsDisplay16[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay16[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    finalsDisplay16[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    finalsDisplay16[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    finalsDisplay16[i].rank.text = "000";
                                }
                                break;
							}
							if (teams[j].id == gameList[42].y && i == 2)
							{
								if (teams[j].player)
								{
									oppTeam = (int)gameList[42].x;
									finalsDisplay16[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay16[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    finalsDisplay16[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    finalsDisplay16[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    finalsDisplay16[i].rank.text = "000";
                                }
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
								if (teams[j].player)
								{
									oppTeam = (int)gameList[41].x;
									finalsDisplay16[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket Y-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay16[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    finalsDisplay16[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    finalsDisplay16[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    finalsDisplay16[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0;
				vertScrollBar.value = 0.92f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 17
			case 17:
				heading.text = "Final Bracket - Draw 17";

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
								if (teams[j].player)
								{
									oppTeam = (int)gameList[44].y;
									finalsDisplay17[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay17[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    finalsDisplay17[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    finalsDisplay17[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    finalsDisplay17[i].rank.text = "000";
                                }
                                break;
							}
							if (i == 2 && teams[j].id == gameList[42].y)
							{
								if (teams[j].player)
								{
									oppTeam = (int)gameList[42].x;
									finalsDisplay17[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay17[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    finalsDisplay17[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    finalsDisplay17[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    finalsDisplay17[i].rank.text = "000";
                                }
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
								if (teams[j].player)
								{
									oppTeam = (int)gameList[42].y;
									finalsDisplay17[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket Y-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay17[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    finalsDisplay17[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    finalsDisplay17[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    finalsDisplay17[i].rank.text = "000";
                                }
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

				//simButton.gameObject.SetActive(true);
				//contButton.gameObject.SetActive(false);
				horizScrollBar.value = 0.3f;
				vertScrollBar.value = 0.9f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 18
			case 18:
				heading.text = "Final Bracket - Draw 18";

				for (int i = 0; i < finalsDisplay18.Length; i++)
				{

					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[43].x)
							{
								if (teams[j].player)
								{
									oppTeam = (int)gameList[43].y;
									finalsDisplay18[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay18[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    finalsDisplay18[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    finalsDisplay18[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    finalsDisplay18[i].rank.text = "000";
                                }
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
								if (teams[j].player)
								{
									oppTeam = (int)gameList[43].x;
									finalsDisplay18[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket Y-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay18[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    finalsDisplay18[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    finalsDisplay18[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    finalsDisplay18[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0.61f;
				vertScrollBar.value = 0.95f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 19
			case 19:
				heading.text = "Final Bracket - Draw 19";

				for (int i = 0; i < finalsDisplay19.Length; i++)
				{
					finalsDisplay19[i].panel.transform.parent.gameObject.SetActive(true);

					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[44].x)
							{
								if (teams[j].player)
								{
									oppTeam = (int)gameList[44].y;
									finalsDisplay19[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay19[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    finalsDisplay19[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    finalsDisplay19[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    finalsDisplay19[i].rank.text = "000";
                                }
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
								if (teams[j].player)
								{
									oppTeam = (int)gameList[44].x;
									finalsDisplay19[i].bg.GetComponent<Image>().color = yellow;
									playerGame = true;
								}
								Debug.Log("Setting Finals Bracket Y-" + teams[j].id + " - " + teams[j].name);
								finalsDisplay19[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    finalsDisplay19[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    finalsDisplay19[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    finalsDisplay19[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0.92f;
				vertScrollBar.value = 1f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 20
			case 20:
				heading.text = "Finals";

				for (int j = 0; j < teams.Length; j++)
				{
					if (teams[j].id == gameList[45].x)
					{
						Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
						finalsDisplay20[0].name.text = teams[j].name;
                        if (teams[j].loss == 2)
                        {
                            finalsDisplay20[0].rank.text = "OXX";
                        }
                        if (teams[j].loss == 1)
                        {
                            finalsDisplay20[0].rank.text = "00X";
                        }
                        if (teams[j].loss == 0)
                        {
                            finalsDisplay20[0].rank.text = "000";
                        }
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
				horizScrollBar.value = 0.95f;
				vertScrollBar.value = 0.96f;
				//StartCoroutine(SaveCareer(true));
				break;
				#endregion
		}

		if (playerGame)
		{
			simInProgress = false;
			vsDisplayGO.SetActive(true);
			playButton.gameObject.SetActive(true);
			simButton.gameObject.SetActive(true);
			contButton.gameObject.SetActive(false);
			nextButton.gameObject.SetActive(false);
		}
		else
		{

			if (playoffRound < 20)
			{
				vsDisplayGO.SetActive(false);
				nextButton.gameObject.SetActive(false);
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(false);
				StartCoroutine(SimToFinals());
			}
			else
			{
                nextButton.gameObject.SetActive(true);
				simButton.gameObject.SetActive(false);
				vsDisplayGO.SetActive(true);
			}

		}

		for (int i = 0; i < teams.Length; i++)
		{
			if (playoffRound < 19)
			{
				if (teams[i].player)
				{
					if (teams[i].loss == 3)
					{
						vsDisplayTitle.text = "XXX";
						vsDisplay[0].rank.text = "KO";
						vsDisplay[0].name.text = teams[i].name;
						vsDisplayVS.text = "Is";
						vsDisplay[1].rank.text = "-";
						vsDisplay[1].name.text = "Knocked Out!";
					}
					if (teams[i].loss == 2)
						vsDisplay[0].rank.text = "OXX";
					if (teams[i].loss == 1)
						vsDisplay[0].rank.text = "OOX";
					if (teams[i].loss == 0)
						vsDisplay[0].rank.text = "000";
					vsDisplay[0].name.text = teams[i].name;
				}
				if (teams[i].id == oppTeam)
				{
					if (teams[i].loss == 3)
					{
					}
					if (teams[i].loss == 2)
						vsDisplay[1].rank.text = "OXX";
					if (teams[i].loss == 1)
						vsDisplay[1].rank.text = "OOX";
					if (teams[i].loss == 0)
						vsDisplay[1].rank.text = "OOO";
					vsDisplay[1].name.text = teams[i].name;
				}
			}
		}

		StartCoroutine(RefreshPlayoffPanel());

		cm.SaveTournyState();

    }

	public void OnSim()
	{
		StartCoroutine(SimPlayoff());
	}

	IEnumerator SimPlayoff()
	{
		playButton.gameObject.SetActive(false);
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
					if (cont & gsp.redTeamName == gameY[i].name)
					{
						if (gsp.redScore < gsp.yellowScore)
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
					else if (cont & gsp.yellowTeamName == gameY[i].name)
					{
						if (gsp.redScore > gsp.yellowScore)
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
					else
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
				}
				StartCoroutine(RefreshPlayoffPanel());
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
						if (cont & gsp.redTeamName == gameY[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameY[i].name)
						{
							if (gsp.redScore > gsp.yellowScore)
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
					}
					else
					{
						if (cont & gsp.redTeamName == gameY[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameY[i].name)
						{
							if (gsp.redScore > gsp.yellowScore)
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
				}
				StartCoroutine(RefreshPlayoffPanel());
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
						if (cont & gsp.redTeamName == gameY[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameY[i].name)
						{
							if (gsp.redScore > gsp.yellowScore)
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
					}
					else
					{
						if (cont & gsp.redTeamName == gameY[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameY[i].name)
						{
							if (gsp.redScore > gsp.yellowScore)
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
				}
				StartCoroutine(RefreshPlayoffPanel());
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
						if (cont & gsp.redTeamName == gameY[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameY[i].name)
						{
							if (gsp.redScore > gsp.yellowScore)
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
					}
					else
					{
						if (cont & gsp.redTeamName == gameY[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameY[i].name)
						{
							if (gsp.redScore > gsp.yellowScore)
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
				}
				StartCoroutine(RefreshPlayoffPanel());
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
						if (cont & gsp.redTeamName == gameY[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameY[i].name)
						{
							if (gsp.redScore > gsp.yellowScore)
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
					}
					else
					{
						if (cont & gsp.redTeamName == gameY[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameY[i].name)
						{
							if (gsp.redScore > gsp.yellowScore)
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
				}
				StartCoroutine(RefreshPlayoffPanel());
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
						if (cont & gsp.redTeamName == gameY[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameY[i].name)
						{
							if (gsp.redScore > gsp.yellowScore)
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
					}
					else
					{
						if (cont & gsp.redTeamName == gameY[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameY[i].name)
						{
							if (gsp.redScore > gsp.yellowScore)
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
				}

				StartCoroutine(RefreshPlayoffPanel());
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
						if (cont & gsp.redTeamName == gameY[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameY[i].name)
						{
							if (gsp.redScore > gsp.yellowScore)
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
					}
					else
					{
						if (cont & gsp.redTeamName == gameY[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameY[i].name)
						{
							if (gsp.redScore > gsp.yellowScore)
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
				}
				StartCoroutine(RefreshPlayoffPanel());
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
						if (cont & gsp.redTeamName == gameY[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameY[i].name)
						{
							if (gsp.redScore > gsp.yellowScore)
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
					}
					else
					{
						if (cont & gsp.redTeamName == gameY[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameY[i].name)
						{
							if (gsp.redScore > gsp.yellowScore)
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
				}
				StartCoroutine(RefreshPlayoffPanel());
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
						if (cont & gsp.redTeamName == gameY[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameY[i].name)
						{
							if (gsp.redScore > gsp.yellowScore)
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
					}
					else
					{
						if (cont & gsp.redTeamName == gameY[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameY[i].name)
						{
							if (gsp.redScore > gsp.yellowScore)
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
				}
				StartCoroutine(RefreshPlayoffPanel());
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
						if (cont & gsp.redTeamName == gameY[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameY[i].name)
						{
							if (gsp.redScore > gsp.yellowScore)
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
					}
					else
					{
						if (cont & gsp.redTeamName == gameY[i].name)
						{
							if (gsp.redScore < gsp.yellowScore)
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
						else if (cont & gsp.yellowTeamName == gameY[i].name)
						{
							if (gsp.redScore > gsp.yellowScore)
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
				}
				StartCoroutine(RefreshPlayoffPanel());
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

				if (cont & gsp.redTeamName == gameY[0].name)
				{
					if (gsp.redScore < gsp.yellowScore)
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
				}
				else if (cont & gsp.yellowTeamName == gameY[0].name)
				{
					if (gsp.redScore > gsp.yellowScore)
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
				}
				else
				{
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
				}

				StartCoroutine(RefreshPlayoffPanel());
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

				if (cont & gsp.redTeamName == gameY[0].name)
				{
					if (gsp.redScore < gsp.yellowScore)
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
				}
				else if (cont & gsp.yellowTeamName == gameY[0].name)
				{
					if (gsp.redScore > gsp.yellowScore)
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
				}
				else
				{
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
				}

				StartCoroutine(RefreshPlayoffPanel());
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

				if (cont & gsp.redTeamName == gameY[0].name)
				{
					if (gsp.redScore < gsp.yellowScore)
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
				}
				else if (cont & gsp.yellowTeamName == gameY[0].name)
				{
					if (gsp.redScore > gsp.yellowScore)
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
				}
				else
				{
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
				}

				StartCoroutine(RefreshPlayoffPanel());
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

				if (cont & gsp.redTeamName == gameY[0].name)
				{
					if (gsp.redScore < gsp.yellowScore)
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
				}
				else if (cont & gsp.yellowTeamName == gameY[0].name)
				{
					if (gsp.redScore > gsp.yellowScore)
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
				}
				else
				{
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
						gameX[0].rank = 5;
						gameX[0].loss++;
					}
				}

				if (cont & gsp.redTeamName == gameY[1].name)
				{
					if (gsp.redScore < gsp.yellowScore)
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
				}
				else if (cont & gsp.yellowTeamName == gameY[1].name)
				{
					if (gsp.redScore > gsp.yellowScore)
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
				}
				else
				{
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
				}

				StartCoroutine(RefreshPlayoffPanel());
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

				if (cont & gsp.redTeamName == gameY[0].name)
				{
					if (gsp.redScore < gsp.yellowScore)
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
				}
				else if (cont & gsp.yellowTeamName == gameY[0].name)
				{
					if (gsp.redScore > gsp.yellowScore)
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
				}
				else
				{
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
				}

				StartCoroutine(RefreshPlayoffPanel());
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
				if (cont & gsp.redTeamName == gameY[0].name)
				{
					if (gsp.redScore < gsp.yellowScore)
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
				}
				else if (cont & gsp.yellowTeamName == gameY[0].name)
				{
					if (gsp.redScore > gsp.yellowScore)
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
				}
				else
				{
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
				}

				StartCoroutine(RefreshPlayoffPanel());
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

				if (cont & gsp.redTeamName == gameY[0].name)
				{
					if (gsp.redScore < gsp.yellowScore)
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
				}
				else if (cont & gsp.yellowTeamName == gameY[0].name)
				{
					if (gsp.redScore > gsp.yellowScore)
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
				}
				else
				{
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
				}

				StartCoroutine(RefreshPlayoffPanel());
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

				if (cont & gsp.redTeamName == gameY[0].name)
				{
					if (gsp.redScore < gsp.yellowScore)
					{
						gameList[45].x = gameX[0].id;
						gameX[0].rank = 1;
						gameX[0].wins++;
						gameY[0].rank = 2;
						gameY[0].loss++;
					}
					else
					{
						gameList[45].x = gameY[0].id;
						gameY[0].rank = 1;
						gameY[0].wins++;
						gameX[0].rank = 2;
						gameX[0].loss++;
					}
				}
				else if (cont & gsp.yellowTeamName == gameY[0].name)
				{
					if (gsp.redScore > gsp.yellowScore)
					{
						gameList[45].x = gameX[0].id;
						gameX[0].rank = 1;
						gameX[0].wins++;
						gameY[0].rank = 2;
						gameY[0].loss++;
					}
					else
					{
						gameList[45].x = gameY[0].id;
						gameY[0].rank = 1;
						gameY[0].wins++;
						gameX[0].rank = 2;
						gameX[0].loss++;
					}
				}
				else
				{
					if (Random.Range(0, gameX[0].strength) > Random.Range(0, gameY[0].strength))
					{
						gameList[45].x = gameX[0].id;
						gameX[0].rank = 1;
						gameX[0].wins++;
						gameY[0].rank = 2;
						gameY[0].loss++;
					}
					else
					{
						gameList[45].x = gameY[0].id;
						gameY[0].rank = 1;
						gameY[0].wins++;
						gameX[0].rank = 2;
						gameX[0].loss++;
					}
				}
				//if (Random.Range(0, gameX[0].strength) > Random.Range(0, gameY[0].strength))
				//{
				//	gameList[45].x = gameX[0].id;
				//	gameX[0].wins++;
				//	gameY[0].rank = 2;
				//	gameY[0].loss++;
				//}
				//else
				//{
				//	gameList[45].x = gameY[0].id;
				//	gameY[0].wins++;
				//	gameX[0].rank = 2;
				//	gameX[0].loss++;
				//}

				for (int i = 0; i < gameX.Length; i++)
				{
					gameX[0].tourRecord = new Vector2(gameX[0].wins, gameX[0].loss);
					gameY[0].tourRecord = new Vector2(gameY[0].wins, gameY[0].loss);
				}

				StartCoroutine(RefreshPlayoffPanel());
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
					gameX[0].rank = 1;
					gameY[0].rank = 2;
					gameY[0].loss++;
				}
				else
				{
					gameList[45].x = gameY[0].id;
					gameY[0].wins++;
					gameY[0].rank = 1;
					gameX[0].rank = 2;
					gameX[0].loss++;
				}

				StartCoroutine(RefreshPlayoffPanel());
				SimResults();
				break;
			#endregion
			default:
				//SetPlayoffs();
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
				heading.text = "Draw 1 - Results";
				vsDisplayTitle.text = "Winners Bracket";
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
								if (teams[j].loss == 2)
                                {
                                    winnersDisplay3[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    winnersDisplay3[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    winnersDisplay3[i].rank.text = "000";
                                }
                            }
						}
						else
						{
							if (teams[j].id == gameList[((i - 1) / 2) + 12].y)
							{
								Debug.Log("Setting Winners Bracket Y-" + teams[j].id + " - " + teams[j].name);
								winnersDisplay3[i].name.text = teams[j].name;
                                if (teams[j].loss == 2)
                                {
                                    winnersDisplay3[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    winnersDisplay3[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    winnersDisplay3[i].rank.text = "000";
                                }
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

				winnersBracket.SetActive(true);
				losersBracket1.SetActive(false);
				losersBracket2.SetActive(false);
				finalsBracket.SetActive(false);
				playoffRound++;
				horizScrollBar.value = 0;
				vertScrollBar.value = 1;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 2
			case 2:
				heading.text = "Draw 2 - Results";
				vsDisplayTitle.text = "Loser's Bracket";

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
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayA4[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayA4[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayA4[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0;
				vertScrollBar.value = 0.56f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 3
			case 3:
				heading.text = "Draw 3 - Results";
				vsDisplayTitle.text = "Winners Bracket";

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
                                if (teams[j].loss == 2)
                                {
                                    winnersDisplay7[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    winnersDisplay7[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    winnersDisplay7[i].rank.text = "000";
                                }
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
                                if (teams[j].loss == 2)
                                {
                                    winnersDisplay7[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    winnersDisplay7[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    winnersDisplay7[i].rank.text = "000";
                                }
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

				horizScrollBar.value = 0.35f;
				vertScrollBar.value = 1f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 4
			case 4:
				heading.text = " Draw 4 - Results";
				vsDisplayTitle.text = "Loser's Bracket";

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
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayA8[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayA8[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayA8[i].rank.text = "000";
                                }
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
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayA8[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayA8[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayA8[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0.3f;
				vertScrollBar.value = 0.77f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 5
			case 5:
				heading.text = "Draw 5 - Results";
				vsDisplayTitle.text = "Knockout Bracket";

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
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayB6[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayB6[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayB6[i].rank.text = "000";
                                }
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
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayB6[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayB6[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayB6[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0;
				vertScrollBar.value = 0.33f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 6
			case 6:
				heading.text = "Draw 6 - Results";
				vsDisplayTitle.text = "Knockout Bracket";

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
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayB10[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayB10[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayB10[i].rank.text = "000";
                                }
                                losersDisplayB10[i].panel.GetComponent<Image>().color = yellow;
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
				horizScrollBar.value = 0.3f;
				vertScrollBar.value = 0.27f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 7
			case 7:
				heading.text = "Draw 7 - Results";
				vsDisplayTitle.text = "Winner's Bracket";

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
                                if (teams[j].loss == 2)
                                {
                                    winnersDisplay12[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    winnersDisplay12[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    winnersDisplay12[i].rank.text = "000";
                                }
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
                                if (teams[j].loss == 2)
                                {
                                    winnersDisplay12[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    winnersDisplay12[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    winnersDisplay12[i].rank.text = "000";
                                }
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
				horizScrollBar.value = 0.75f;
				vertScrollBar.value = 0.76f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 8
			case 8:
				heading.text = "Draw 8 - Results";
				vsDisplayTitle.text = "Loser's Bracket";

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
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayA9[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayA9[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayA9[i].rank.text = "000";
                                }
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
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayA9[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayA9[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayA9[i].rank.text = "000";
                                }
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
				heading.text = "Draw 9 - Results";
				vsDisplayTitle.text = "Loser's Bracket";

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
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayA13[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayA13[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayA13[i].rank.text = "000";
                                }

                                if (teams[j].player)
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
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayA13[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayA13[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayA13[i].rank.text = "000";
                                }

                                if (teams[j].player)
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
				horizScrollBar.value = 0.94f;
				vertScrollBar.value = 0.63f;
				//StartCoroutine(SaveCareer(true));
				break;
			#endregion
			#region Round 10
			case 10:
				heading.text = "Draw 10 - Results";
				vsDisplayTitle.text = "Knockout Bracket";

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
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayB11[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayB11[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayB11[i].rank.text = "000";
                                }
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
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayB11[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayB11[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayB11[i].rank.text = "000";
                                }
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
				heading.text = "Draw 11 - Results";
				vsDisplayTitle.text = "Knockout Bracket";

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
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayB14[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayB14[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayB14[i].rank.text = "000";
                                }
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
                                if (teams[j].loss == 2)
                                {
                                    losersDisplayB14[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    losersDisplayB14[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    losersDisplayB14[i].rank.text = "000";
                                }
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
				heading.text = "Draw 12 - Results";
				vsDisplayTitle.text = "Winner's Bracket";

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
                        if (teams[j].loss == 2)
                        {
                            winnersDisplay12[2].rank.text = "OXX";
                        }
                        if (teams[j].loss == 1)
                        {
                            winnersDisplay12[2].rank.text = "00X";
                        }
                        if (teams[j].loss == 0)
                        {
                            winnersDisplay12[2].rank.text = "000";
                        }
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
				heading.text = "Draw 13 - Results";
				vsDisplayTitle.text = "Loser's Bracket";

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
                        if (teams[j].loss == 2)
                        {
                            losersDisplayA13[2].rank.text = "OXX";
                        }
                        if (teams[j].loss == 1)
                        {
                            losersDisplayA13[2].rank.text = "00X";
                        }
                        if (teams[j].loss == 0)
                        {
                            losersDisplayA13[2].rank.text = "000";
                        }
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
				heading.text = " Draw 14 - Results";
				vsDisplayTitle.text = "Knockout Bracket";

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
					if (teams[j].id == gameList[38].y)
					{
						Debug.Log("Setting Losers Bracket B Y-" + gameList[38].y + " - " + teams[j].name);
						losersDisplayB14[2].panel.transform.parent.gameObject.SetActive(true);
						losersDisplayB14[2].name.text = teams[j].name;
                        if (teams[j].loss == 2)
                        {
                            losersDisplayB14[2].rank.text = "OXX";
                        }
                        if (teams[j].loss == 1)
                        {
                            losersDisplayB14[2].rank.text = "00X";
                        }
                        if (teams[j].loss == 0)
                        {
                            losersDisplayB14[2].rank.text = "000";
                        }
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
				heading.text = " Draw 15 - Results";
				vsDisplayTitle.text = "Final Bracket";

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
                                if (teams[j].loss == 2)
                                {
                                    finalsDisplay16[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    finalsDisplay16[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    finalsDisplay16[i].rank.text = "000";
                                }
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
                                if (teams[j].loss == 2)
                                {
                                    finalsDisplay16[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    finalsDisplay16[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    finalsDisplay16[i].rank.text = "000";
                                }
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
                        if (teams[j].loss == 2)
                        {
                            finalsDisplay17[1].rank.text = "OXX";
                        }
                        if (teams[j].loss == 1)
                        {
                            finalsDisplay17[1].rank.text = "00X";
                        }
                        if (teams[j].loss == 0)
                        {
                            finalsDisplay17[1].rank.text = "000";
                        }
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
				heading.text = "Draw 16 - Results";
				vsDisplayTitle.text = "Final Bracket";

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
                            if (teams[j].loss == 2)
                            {
                                finalsDisplay17[i].rank.text = "OXX";
                            }
                            if (teams[j].loss == 1)
                            {
                                finalsDisplay17[i].rank.text = "00X";
                            }
                            if (teams[j].loss == 0)
                            {
                                finalsDisplay17[i].rank.text = "000";
                            }
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
                        if (teams[j].loss == 2)
                        {
                            finalsDisplay18[0].rank.text = "OXX";
                        }
                        if (teams[j].loss == 1)
                        {
                            finalsDisplay18[0].rank.text = "00X";
                        }
                        if (teams[j].loss == 0)
                        {
                            finalsDisplay18[0].rank.text = "000";
                        }
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
				heading.text = " Draw 17 - Results";
				vsDisplayTitle.text = "Final Bracket";

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
                                if (teams[j].loss == 2)
                                {
                                    finalsDisplay18[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    finalsDisplay18[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    finalsDisplay18[i].rank.text = "000";
                                }
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
                                if (teams[j].loss == 2)
                                {
                                    finalsDisplay18[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    finalsDisplay18[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    finalsDisplay18[i].rank.text = "000";
                                }
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
				vsDisplayTitle.text = "Final Bracket";

				for (int i = 0; i < finalsDisplay18.Length; i++)
				{
					if (i % 2 == 0)
					{
						for (int j = 0; j < teams.Length; j++)
						{
							if (teams[j].id == gameList[43].x)
							{
								if (teams[j].rank == 3)
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
								if (teams[j].rank == 3)
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
                                if (teams[j].loss == 2)
                                {
                                    finalsDisplay19[i].rank.text = "OXX";
                                }
                                if (teams[j].loss == 1)
                                {
                                    finalsDisplay19[i].rank.text = "00X";
                                }
                                if (teams[j].loss == 0)
                                {
                                    finalsDisplay19[i].rank.text = "000";
                                }
                                finalsDisplay19[i].panel.GetComponent<Image>().color = yellow;
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

				for (int i = 0; i < teams.Length; i++)
				{
					float p = 1.4f;
					float totalTeams = teams.Length - 5f;
					float prizePayout = ((Mathf.Pow(p, totalTeams - ((teams[i].rank - 1) + 1))) / (Mathf.Pow(p, totalTeams) - 1f)) * (gsp.prize * 0.15f) * (p - 1);

					if (teams[i].rank == 1)
					{
						prizePayout = gsp.prize * 0.5f;
					}
					else if (teams[i].rank == 2)
						prizePayout = gsp.prize * 0.25f;
					else if (teams[i].rank == 3)
						prizePayout = gsp.prize * 0.15f;
					else if (teams[i].rank == 4)
						prizePayout = gsp.prize * 0.075f;
					else if (teams[i].rank == 5)
						prizePayout = gsp.prize * 0.038f;
					else
					{
						Debug.Log("Rank " + teams[i].rank + " - " + teams[i].name + " Payout is $" + prizePayout);
					}

					Debug.Log("Position " + (i + 1) + " Payout is $" + prizePayout);
					teams[i].earnings += prizePayout;

					//Debug.Log("Prize Payout multiplier is " + prizePayout);
					if (teams[i].player)
					{
						//gsp.earnings = teams[i].earnings;
						gsp.tournyEarnings = prizePayout;
						gsp.tournyCash = prizePayout;
						vsDisplayGO.SetActive(true);

						vsDisplayTitle.text = "Results";
						vsDisplayVS.text = "Wins";
						vsDisplay[0].name.text = teams[i].name;
						vsDisplay[0].rank.text = teams[i].rank.ToString();
						vsDisplay[1].name.text = "$" + prizePayout.ToString("n0");
						vsDisplay[1].rank.gameObject.SetActive(false);
					}
					//prizePayout = Mathf.RoundToInt(prizePayout);
					//tm.teamList[i].team.earnings += prizePayout;

				}
				Debug.Log("Career Earnings after calculation - " + gsp.tournyEarnings.ToString());
				careerEarningsText.text = "$ " + gsp.tournyEarnings.ToString("n0");

				for (int j = 0; j < teams.Length; j++)
				{
					if (teams[j].id == gameList[45].x)
					{
						Debug.Log("Setting Finals Bracket X-" + teams[j].id + " - " + teams[j].name);
						finalsDisplay20[0].name.text = teams[j].name;
                        if (teams[j].loss == 2)
                        {
                            finalsDisplay20[0].rank.text = "OXX";
                        }
                        if (teams[j].loss == 1)
                        {
                            finalsDisplay20[0].rank.text = "00X";
                        }
                        if (teams[j].loss == 0)
                        {
                            finalsDisplay20[0].rank.text = "000";
                        }

                        nextButton.gameObject.SetActive(true);
						simButton.gameObject.SetActive(false);

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
				break;
			#endregion
		}

		if (playoffRound < 19)
		{
			
			for (int i = 0; i < teams.Length; i++)
			{
				if (teams[i].player)
				{
					//vsDisplay[0].rank.text = teams[i].wins.ToString() + "-" + teams[i].loss.ToString();
					//vsDisplay[0].name.text = teams[i].name;

					if (teams[i].loss == 3)
					{
						vsDisplay[0].rank.text = "XXX";
					}
					if (teams[i].loss == 2)
					{
						vsDisplay[0].rank.text = "OXX";
					}
					if (teams[i].loss == 1)
					{
						vsDisplay[0].rank.text = "OOX";
					}
					if (teams[i].loss == 0)
					{
						vsDisplay[0].rank.text = "OOO";
					}
					vsDisplay[0].name.text = teams[i].name;
				}
				if (teams[i].id == oppTeam)
				{
					if (teams[i].loss == 2)
					{
						vsDisplay[1].rank.text = "OXX";
					}
					if (teams[i].loss == 1)
					{
						vsDisplay[1].rank.text = "OOX";
					}
					if (teams[i].loss == 0)
					{
						vsDisplay[1].rank.text = "OOO";
					}
					vsDisplay[1].name.text = teams[i].name;
				}
			}

			if (simInProgress)
			{
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(false);
			}
			else
			{
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
			}
		}

        //if (playoffRound < 15)
        //{
        //    StartCoroutine(SimToFinals());
        //}
		for (int i = 0; i < teams.Length; i++)
        {
			teams[i].tourRecord.x = teams[i].wins;
			teams[i].tourRecord.y = teams[i].loss;

			//if (teams[i].id == playerTeam)
   //         {
			//	gsp.record = new Vector2(teams[i].wins, teams[i].loss);
   //         }
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

		for (int i = 0; i < teams.Length; i++)
		{
			teams[i].wins += (int)cm.teamRecords[i].x;
			teams[i].loss += (int)cm.teamRecords[i].y;
			teams[i].earnings += cm.teamRecords[i].z;
			teams[i].id = (int)cm.teamRecords[i].w;

			teams[i].tourRecord.x += cm.tourRecords[i].x;
			teams[i].tourRecord.y += cm.tourRecords[i].y;
			teams[i].tourPoints += cm.tourRecords[i].z;
			teams[i].id = (int)cm.tourRecords[i].w;
		}

		for (int i = 0; i < teams.Length; i++)
		{
			if (teams[i].id == playerTeam)
			{
				gsp.tournyEarnings = teams[i].earnings;
				gsp.tournyRecord = new Vector2(teams[i].wins, teams[i].loss);
			}
		}
		
        gsp.draw = 0;
		gsp.playoffRound = 0;
		gsp.tournyInProgress = false;
		Debug.Log("gsp.inProgress is " + gsp.tournyInProgress);
		Debug.Log("CM Record is " + cm.record.x + " - " + cm.record.y);
		Debug.Log("CM earnings are " + cm.earnings);
		cm.TournyResults();
		SceneManager.LoadScene("Arena_Selector");
	}

	IEnumerator ResetBrackets(int poRound)
	{
		playoffs.SetActive(false);
		playButton.transform.parent.gameObject.SetActive(false);

		for (int i = poRound - 1; i < poRound; i++)
		{
			playoffRound = i;
			yield return new WaitForSeconds(0.01f);
			SetPlayoffs();
            
            yield return new WaitForSeconds(0.01f);
			SimResults();
			if (playoffRound > 1)
				playoffRound--;
			else
				playoffRound = 1;
			yield return new WaitForSeconds(0.01f);
			Debug.Log("Playoff Round in Reset is " + playoffRound + " and i is " + i);
		}
		yield return new WaitForSeconds(waitTime);
		playoffs.SetActive(true);
		playButton.transform.parent.gameObject.SetActive(true);
		if (cont)
			StartCoroutine(SimPlayoff());
		//playoffRound = poRound;
	}

	IEnumerator SimToFinals()
	{
		simInProgress = true;
		yield return new WaitForSeconds(0.001f);
		vsDisplayGO.SetActive(false);
		nextButton.gameObject.SetActive(false);
		simButton.gameObject.SetActive(false);
		contButton.gameObject.SetActive(false);
		OnSim();
		yield return new WaitForSeconds(0.001f);
		vsDisplayGO.SetActive(false);
		nextButton.gameObject.SetActive(false);
		simButton.gameObject.SetActive(false);
		contButton.gameObject.SetActive(false);
		SetPlayoffs();
		yield return new WaitForSeconds(0.05f);
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

		myFile.Add("Triple Knockout Tourny", true);
		//myFile.Add("Career Record", cm.record);
		Debug.Log("gsp.record is " + gsp.tournyRecord.x + " - " + gsp.tournyRecord.y);

		myFile.Add("BG", gsp.bg);
		//myFile.Add("Career Earnings", gsp.earnings);
		
		myFile.Add("Tourny In Progress", inProgress);
		Debug.Log("gsp.inProgress is " + gsp.tournyInProgress);
		gsp.tournyInProgress = inProgress;
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

		int[] tourWinsList = new int[teams.Length];
		int[] tourLossList = new int[teams.Length];
		float[] tourEarningsList = new float[teams.Length];
		float[] tourPointsList = new float[teams.Length];
		bool[] tourPlayerList = new bool[teams.Length];

		for (int i = 0; i < teams.Length; i++)
		{
			nameList[i] = teams[i].name;
			winsList[i] = teams[i].wins;
			lossList[i] = teams[i].loss;
			rankList[i] = teams[i].rank;
			nextOppList[i] = teams[i].nextOpp;
			strengthList[i] = teams[i].strength;
			idList[i] = teams[i].id;
			tourWinsList[i] = (int)teams[i].tourRecord.x;
			tourLossList[i] = (int)teams[i].tourRecord.y;
			tourEarningsList[i] = teams[i].earnings;
			tourPlayerList[i] = teams[i].player;
			tourPointsList[i] = teams[i].tourPoints;
		}

		myFile.Add("Tourny Name List", nameList);
		myFile.Add("Tourny Wins List", winsList);
		myFile.Add("Tourny Loss List", lossList);
		myFile.Add("Tourny Rank List", rankList);
		myFile.Add("Tourny NextOpp List", nextOppList);
		myFile.Add("Tourny Strength List", strengthList);
		myFile.Add("Tourny Team ID List", idList);
		myFile.Add("Tourny Player List", tourPlayerList);
		myFile.Add("Tourny Earnings List", tourEarningsList);
		myFile.Add("Tourny Tour Points List", tourPointsList);

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
		int[] tempTRX = new int[cm.teamRecords.Length];
		int[] tempTRY = new int[cm.teamRecords.Length];
		float[] tempTRZ = new float[cm.teamRecords.Length];
		int[] tempTRW = new int[cm.teamRecords.Length];

		int[] tempTourTRX = new int[cm.tourRecords.Length];
		int[] tempTourTRY = new int[cm.tourRecords.Length];
		float[] tempTourTRZ = new float[cm.tourRecords.Length];
		int[] tempTourTRW = new int[cm.tourRecords.Length];

		for (int i = 0; i < cm.teamRecords.Length; i++)
		{
			tempTRX[i] = (int)cm.teamRecords[i].x;
			tempTRY[i] = (int)cm.teamRecords[i].y;
			tempTRZ[i] = cm.teamRecords[i].z;
			tempTRW[i] = (int)cm.teamRecords[i].w;
		}

		for (int i = 0; i < cm.tourRecords.Length; i++)
		{
			tempTourTRX[i] = (int)cm.tourRecords[i].x;
			tempTourTRY[i] = (int)cm.tourRecords[i].y;
			tempTourTRZ[i] = cm.tourRecords[i].z;
			tempTourTRW[i] = (int)cm.tourRecords[i].w;
		}

		myFile.Add("Team Records X", tempTRX);
		myFile.Add("Team Records Y", tempTRY);
		myFile.Add("Team Records Z", tempTRZ);
		myFile.Add("Team Records W", tempTRW);

		myFile.Add("Tour Records X", tempTourTRX);
		myFile.Add("Tour Records Y", tempTourTRY);
		myFile.Add("Tour Records Z", tempTourTRZ);
		myFile.Add("Tour Records W", tempTourTRW);

		yield return myFile.Append();
	}

	IEnumerator RefreshPlayoffPanel()
	{
		//for (int i = 0; i < vsDisplay.Length; i++)
		//{
		//	vsDisplay[i].name.gameObject.GetComponent<ContentSizeFitter>().enabled = false;

		//	yield return new WaitForEndOfFrame();
		//	vsDisplay[i].name.gameObject.GetComponent<ContentSizeFitter>().enabled = true;
		//}

		if (winnersBracket.activeSelf)
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
		}
		if (losersBracket1.activeSelf)
		{
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
		}
		if (losersBracket2.activeSelf)
		{
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
		}
		if (finalsBracket.activeSelf)
		{
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
		}

    }

	public void Menu()
    {
		cm.SaveTournyState();

        SceneManager.LoadScene("SplashMenu");
	}
}
