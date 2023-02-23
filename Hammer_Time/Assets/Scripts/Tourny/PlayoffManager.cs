using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TigerForge;

public class PlayoffManager : MonoBehaviour
{
	public TournyManager tm;
	public Team[] playoffTeams;

	public BracketDisplay[] brackDisplay;

	public GameObject[] row;
	public GameObject playoffs;
	public Button nextButton;
	public Button simButton;
	public Button contButton;
	public Button playButton;
	public Text heading;
	public Scrollbar scrollBar;
	public Text careerEarningsText;

	GameSettingsPersist gsp;

	EasyFileSave myFile;
	//int pTeams;
	public int playerTeam;
	public int oppTeam;
	public int playoffRound;

	public float careerEarnings;
	public Vector2 careerRecord;

	private void Start()
	{
		gsp = FindObjectOfType<GameSettingsPersist>();

		playoffs.SetActive(true);

		careerEarnings = tm.careerEarnings;
		careerRecord = tm.careerRecord;

		playerTeam = tm.playerTeam;
		playoffRound = gsp.playoffRound;
		playoffTeams = new Team[9];

		//Debug.Log("Career Earnings before playoffs - $ " + gsp.earnings.ToString());
		if (gsp.careerLoad)
			LoadPlayoffs();
		else if (playoffRound > 0)
			LoadAndAdvancePlayoffs();
		else
			SetSeeding(tm.teams.Length);
	}

	public void SetSeeding(int numberOfTeams)
    {

		//pTeams = 4;
		playoffTeams = new Team[9];
		heading.text = "Page Playoff";

		playoffRound++;
		for (int i = 0; i < playoffTeams.Length; i++)
		{
			if (i < 4)
            {
				playoffTeams[i] = tm.teamList[i].team;
				brackDisplay[i].name.text = playoffTeams[i].name;
				brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
			}
			else
            {
				playoffTeams[i] = tm.tTeamList.nullTeam;
            }
		}
		tm.playoffRound = playoffRound;
		SetPlayoffs();
	}

	IEnumerator RefreshPlayoffPanel()
	{
		for (int i = 0; i < brackDisplay.Length; i++)
		{
			brackDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = false;

			yield return new WaitForEndOfFrame();
			brackDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = true;
		}

		//for (int i = 0; i < tm.vsDisplay.Length; i++)
		//{
		//	tm.vsDisplay[i].name.gameObject.GetComponent<ContentSizeFitter>().enabled = false;

		//	yield return new WaitForEndOfFrame();
		//	tm.vsDisplay[i].name.gameObject.GetComponent<ContentSizeFitter>().enabled = true;
		//}
	}

	void LoadAndAdvancePlayoffs()
	{
		//playoffRound--;
		Debug.Log("Load Playoffs - Round " + playoffRound);
		Debug.Log("gsp.playerTeam.nextOpp - " + gsp.playerTeam.nextOpp);
		for (int i = 0; i < playoffTeams.Length; i++)
        {
			playoffTeams[i] = gsp.playoffTeams[i];
        }
		//playoffTeams = gsp.playoffTeams;

		for (int i = 0; i < tm.teams.Length; i++)
		{
			if (tm.teams[i].player)
				playerTeam = i;
		}
		for (int i = 0; i < tm.teams.Length; i++)
		{
			if (tm.teams[i].name == tm.teams[playerTeam].nextOpp)
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
					if (playoffTeams[i].player)
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
							Debug.Log("Player Red beat Yellow");
							playoffTeams[4] = tm.teams[playerTeam];
							playoffTeams[5] = tm.teams[oppTeam];
						}
						else
						{
							Debug.Log("Yellow beat Player Red");
							playoffTeams[5] = tm.teams[playerTeam];
							playoffTeams[4] = tm.teams[oppTeam];
						}
					}
					else
					{
						if (gsp.redScore < gsp.yellowScore)
						{
							Debug.Log("Player Yellow beat Red");
							playoffTeams[4] = tm.teams[playerTeam];
							playoffTeams[5] = tm.teams[oppTeam];
						}
						else
						{
							Debug.Log("Red beat Player Yellow");
							playoffTeams[5] = tm.teams[playerTeam];
							playoffTeams[4] = tm.teams[oppTeam];
						}
					}
				}

				if (game2)
				{
					if (gsp.playerTeam.name == gsp.redTeamName)
					{
						if (gsp.redScore > gsp.yellowScore)
						{
							playoffTeams[6] = tm.teams[playerTeam];
						}
						else
						{
							playoffTeams[6] = tm.teams[oppTeam];
						}
					}
					else
					{
						if (gsp.redScore < gsp.yellowScore)
						{
							playoffTeams[6] = tm.teams[playerTeam];
						}
						else
						{
							playoffTeams[6] = tm.teams[oppTeam];
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
						playoffTeams[7] = tm.teams[playerTeam];
					}
					else
					{
						playoffTeams[7] = tm.teams[oppTeam];
					}
				}
				else
				{
					if (gsp.redScore < gsp.yellowScore)
					{
						playoffTeams[7] = tm.teams[playerTeam];
					}
					else
					{
						playoffTeams[7] = tm.teams[oppTeam];
					}
				}
				playoffRound++;
				break;

			case 3:

				if (gsp.playerTeam.name == gsp.redTeamName)
				{
					if (gsp.redScore > gsp.yellowScore)
					{
						playoffTeams[8] = tm.teams[playerTeam];
					}
					else
					{
						playoffTeams[8] = tm.teams[oppTeam];
					}
				}
				else
				{
					if (gsp.redScore < gsp.yellowScore)
					{
						playoffTeams[8] = tm.teams[playerTeam];
					}
					else
					{
						playoffTeams[8] = tm.teams[oppTeam];
					}
				}
				playoffRound++;
				break;
		}

		SetPlayoffs();
	}

	void LoadPlayoffs()
	{
		gsp.careerLoad = false;
		//playoffRound--;
		Debug.Log("Load Playoffs - Round " + playoffRound);
		Debug.Log("gsp.playerTeam.nextOpp - " + gsp.playerTeam.nextOpp);
		for (int i = 0; i < playoffTeams.Length; i++)
		{
			if (i < 4)
			{
				gsp.playoffTeams[i].rank = i + 1;
			}
			playoffTeams[i] = gsp.playoffTeams[i];
			
		}
		//playoffTeams = gsp.playoffTeams;

		for (int i = 0; i < gsp.teams.Length; i++)
		{
			if (gsp.teams[i].player)
				playerTeam = i;
		}
		for (int i = 0; i < tm.teams.Length; i++)
		{
			if (gsp.teams[i].name == gsp.teams[playerTeam].nextOpp)
				oppTeam = i;
		}
		Debug.Log("OppTeam is " + oppTeam);

		for (int i = 0; i < playoffTeams.Length; i++)
		{
			if (playoffRound == 1 && i < 4)
			{
				brackDisplay[i].name.text = playoffTeams[i].name;
				brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
				heading.text = "Loaded...Page Playoff";
			}
			else if (playoffRound == 2 && i < 7)
			{
				brackDisplay[i].name.text = playoffTeams[i].name;
				brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
				heading.text = "Loaded...Semifinals";
			}
			else if (playoffRound == 3 && i < 8)
			{
				brackDisplay[i].name.text = playoffTeams[i].name;
				brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
				heading.text = "Loaded...Finals";
			}
			else if (playoffRound == 4)
			{
				brackDisplay[i].name.text = playoffTeams[i].name;
				brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
				heading.text = "Loaded...Tourny Over";
			}
			else
			{
				playoffTeams[i] = tm.tTeamList.nullTeam;
			}
		}
		tm.playoffRound = playoffRound;


		SetPlayoffs();
	}

	public void SetPlayoffs()
	{
		if (playoffRound < 1)
		{
			playoffRound = 1;
			gsp.playoffRound = playoffRound;
		}
		Debug.Log("Set Playoffs - Round " + playoffRound);
		switch (playoffRound)
        {
			case 1:
                #region Case 1
                switch (tm.teams[playerTeam].rank)
                {
                    case 1:
                        playButton.gameObject.SetActive(true);
                        tm.vsDisplay[1].name.text = playoffTeams[1].name;
                        tm.vsDisplay[1].rank.text = playoffTeams[1].rank.ToString();
						tm.teams[tm.playerTeam].nextOpp = playoffTeams[1].name;
						break;
                    case 2:
                        playButton.gameObject.SetActive(true);
                        tm.vsDisplay[1].name.text = playoffTeams[0].name;
                        tm.vsDisplay[1].rank.text = playoffTeams[0].rank.ToString();
						tm.teams[tm.playerTeam].nextOpp = playoffTeams[0].name;
						break;
                    case 3:
                        playButton.gameObject.SetActive(true);
                        tm.vsDisplay[1].name.text = playoffTeams[3].name;
                        tm.vsDisplay[1].rank.text = playoffTeams[3].rank.ToString();
						tm.teams[tm.playerTeam].nextOpp = playoffTeams[3].name;
						break;
					case 4:
						playButton.gameObject.SetActive(true);
						tm.vsDisplay[1].name.text = playoffTeams[2].name;
						tm.vsDisplay[1].rank.text = playoffTeams[2].rank.ToString();
						tm.teams[tm.playerTeam].nextOpp = playoffTeams[2].name;
						break;
					default:
						tm.vsDisplay[1].name.text = "Knocked Out!";
                        playButton.gameObject.SetActive(false);
                        break;
                }

                StartCoroutine(RefreshPlayoffPanel());

                playoffs.SetActive(true);

                simButton.gameObject.SetActive(true);
                contButton.gameObject.SetActive(false);
                scrollBar.value = 0;
				StartCoroutine(SaveCareer(true));
                break;
            #endregion
            case 2:
                #region Case 2
                heading.text = "Semifinals";

                for (int i = 0; i < 7; i++)
                {
                    brackDisplay[i].name.text = playoffTeams[i].name;
                    brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
					brackDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[i].SetActive(true);
				}

                //brackDisplay[4].name.text = playoffTeams[4].name;
                //brackDisplay[4].rank.text = playoffTeams[4].rank.ToString();


				if (playoffTeams[4].name == tm.teams[playerTeam].name)
                {
                    playButton.gameObject.SetActive(false);

                    tm.vsDisplay[0].name.text = playoffTeams[4].name;
                    tm.vsDisplay[0].rank.text = playoffTeams[4].rank.ToString();
                    tm.vsDisplay[1].name.text = "BYE TO FINALS";
                    tm.vsDisplay[1].rank.text = "-";
					tm.teams[playerTeam].nextOpp = playoffTeams[4].name;
				}
                else if (playoffTeams[5].name == tm.teams[playerTeam].name)
                {
                    playButton.gameObject.SetActive(true);
                    tm.vsDisplay[0].name.text = playoffTeams[5].name;
                    tm.vsDisplay[0].rank.text = playoffTeams[5].rank.ToString();
                    tm.vsDisplay[1].name.text = playoffTeams[6].name;
                    tm.vsDisplay[1].rank.text = playoffTeams[6].rank.ToString();
					tm.teams[playerTeam].nextOpp = playoffTeams[6].name;
				}
				else if (playoffTeams[6].name == tm.teams[playerTeam].name)
                {
					playButton.gameObject.SetActive(true);
					tm.vsDisplay[0].name.text = playoffTeams[6].name;
					tm.vsDisplay[0].rank.text = playoffTeams[6].rank.ToString();
					tm.vsDisplay[1].name.text = playoffTeams[5].name;
					tm.vsDisplay[1].rank.text = playoffTeams[5].rank.ToString();
					tm.teams[playerTeam].nextOpp = playoffTeams[5].name;
				}
                else
                {
                    tm.vs.SetActive(false);
                    playButton.gameObject.SetActive(false);
                }

                playoffs.SetActive(true);
                StartCoroutine(RefreshPlayoffPanel());

                simButton.gameObject.SetActive(true);
                contButton.gameObject.SetActive(false);
                scrollBar.value = 0.5f;
				StartCoroutine(SaveCareer(true));
				break;
            #endregion
            case 3:
                #region Case 3
                heading.text = "Finals";

				for (int i = 0; i < 8; i++)
				{
					brackDisplay[i].name.text = playoffTeams[i].name;
					brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
					brackDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[i].SetActive(true);
				}

				if (playoffTeams[4].name == tm.teams[playerTeam].name)
				{
					playButton.gameObject.SetActive(true);
					tm.vsDisplay[0].name.text = playoffTeams[4].name;
					tm.vsDisplay[0].rank.text = playoffTeams[4].rank.ToString();
					tm.vsDisplay[1].name.text = playoffTeams[7].name;
					tm.vsDisplay[1].rank.text = playoffTeams[7].rank.ToString();
					tm.teams[playerTeam].nextOpp = playoffTeams[7].name;
				}
				else if (playoffTeams[7].name == tm.teams[playerTeam].name)
				{
					playButton.gameObject.SetActive(true);
					tm.vsDisplay[0].name.text = playoffTeams[7].name;
					tm.vsDisplay[0].rank.text = playoffTeams[7].rank.ToString();
					tm.vsDisplay[1].name.text = playoffTeams[4].name;
					tm.vsDisplay[1].rank.text = playoffTeams[4].rank.ToString();
					tm.teams[playerTeam].nextOpp = playoffTeams[4].name;
				}
				else
				{
					tm.vs.SetActive(false);
					playButton.gameObject.SetActive(false);
				}

				playoffs.SetActive(true);
				StartCoroutine(RefreshPlayoffPanel());

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				scrollBar.value = 1f;
				StartCoroutine(SaveCareer(true));
				break;
            #endregion
            case 4:
				#region Case 4
				for (int i = 0; i < 9; i++)
				{
					brackDisplay[i].name.text = playoffTeams[i].name;
					brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
					//brackDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[i].SetActive(true);
				}

                playoffs.SetActive(true);
                StartCoroutine(RefreshPlayoffPanel());

				tm.vsTitle.text = "Results";
				tm.vsVS.text = " ";
				tm.vs.SetActive(true);

				if (tm.teams[playerTeam].name == playoffTeams[8].name)
				{
					heading.text = "You Win!";

					gsp.earnings += gsp.prize * 0.5f;
					//tm.teams[playerTeam].earnings = gsp.prize * 0.5f;
					tm.teams[playerTeam].rank = 1;
					tm.vs.SetActive(true);

					tm.vsDisplay[0].name.text = tm.teams[playerTeam].name;
					tm.vsDisplay[0].rank.text = tm.teams[playerTeam].rank.ToString();
					tm.vsDisplay[1].name.text = "$" + (gsp.prize * 0.5f).ToString("n0");
					tm.vsDisplay[1].rank.gameObject.SetActive(false);
				}
				else if (tm.teams[playerTeam].name == playoffTeams[4].name | tm.teams[playerTeam].name == playoffTeams[7].name)
				{
					heading.text = "Runner-up";
					gsp.earnings += gsp.prize * 0.25f;
					//tm.teams[playerTeam].earnings = gsp.prize * 0.25f;
					tm.teams[playerTeam].rank = 2;
					tm.vs.SetActive(true);

					tm.vsDisplay[0].name.text = tm.teams[playerTeam].name;
					tm.vsDisplay[0].rank.text = tm.teams[playerTeam].rank.ToString();
					tm.vsDisplay[1].name.text = "$" + (gsp.prize * 0.25f).ToString("n0");
					tm.vsDisplay[1].rank.gameObject.SetActive(false);
				}
				else if (tm.teams[playerTeam].name == playoffTeams[5].name | tm.teams[playerTeam].name == playoffTeams[6].name)
				{
					heading.text = "3rd Place";
					gsp.earnings += gsp.prize * 0.15f;
					//tm.teams[playerTeam].earnings = gsp.prize * 0.15f;
					tm.teams[playerTeam].rank = 3;
					tm.vs.SetActive(true);

					tm.vsDisplay[0].name.text = tm.teams[playerTeam].name;
					tm.vsDisplay[0].rank.text = tm.teams[playerTeam].rank.ToString();
					tm.vsDisplay[1].name.text = "$" + (gsp.prize * 0.15f).ToString("n0");
					tm.vsDisplay[1].rank.gameObject.SetActive(false);
				}
				else if (tm.teams[playerTeam].name == playoffTeams[2].name | tm.teams[playerTeam].name == playoffTeams[3].name)
				{
					heading.text = "4th Place";
					gsp.earnings += gsp.prize * 0.075f;
					//tm.teams[playerTeam].earnings = gsp.prize * 0.075f;
					tm.teams[playerTeam].rank = 4;

					tm.vs.SetActive(true);

					tm.vsDisplay[0].name.text = tm.teams[playerTeam].name;
					tm.vsDisplay[0].rank.text = tm.teams[playerTeam].rank.ToString();
					tm.vsDisplay[1].name.text = "$" + (gsp.prize * 0.075f).ToString("n0");
					tm.vsDisplay[1].rank.gameObject.SetActive(false);
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
							tm.vsDisplay[1].name.text = "$" + prizePayout.ToString("n0");
							tm.vsDisplay[1].rank.gameObject.SetActive(false);
						}
					}
					//tm.vsDisplay[1].rank.gameObject.transform.parent.gameObject.SetActive(false);
				}

				for (int i = 0; i < tm.teamList.Count; i++)
				{
					float p = 1.4f;
					float totalTeams = tm.teamList.Count - 4;
					float prizePayout = ((Mathf.Pow(p, totalTeams - (i + 1))) / (Mathf.Pow(p, totalTeams) - 1f)) * (gsp.prize * 0.15f) * (p - 1);

					if (tm.teamList[i].team.id == playoffTeams[8].id)
					{
						tm.teamList[i].team.earnings += gsp.prize * 0.5f;
						tm.teamList[i].team.rank = 1;
					}
					else if (tm.teamList[i].team.id == playoffTeams[4].id | tm.teamList[i].team.id == playoffTeams[7].id)
					{
						tm.teamList[i].team.earnings += gsp.prize * 0.25f;
						tm.teamList[i].team.rank = 2;
					}
					else if (tm.teamList[i].team.id == playoffTeams[5].id | tm.teamList[i].team.id == playoffTeams[6].id)
					{
						tm.teamList[i].team.earnings += gsp.prize * 0.15f;
						tm.teamList[i].team.rank = 3;
					}
					else if (tm.teamList[i].team.id == playoffTeams[2].id | tm.teamList[i].team.id == playoffTeams[3].id)
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
                Debug.Log("GSP Earnings after calculation - " + gsp.earnings.ToString());
				careerEarningsText.text = "$ " + gsp.earnings.ToString("n0");
				
				//gsp.record = new Vector2(gsp.record.x + tm.teams[playerTeam].wins, gsp.record.y + tm.teams[playerTeam].loss);

				StartCoroutine(SaveCareer(false));
				//heading.text = "So Close!";
				
				playButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(false);
				simButton.gameObject.SetActive(false);
				nextButton.gameObject.SetActive(true);
				scrollBar.value = 1;

				break;
                #endregion
        }
    }

	public void OnSim()
    {
		StartCoroutine(SimPlayoff(false, false));
    }

    IEnumerator SimPlayoff(bool game1, bool game2)
	{

		Debug.Log("Sim Playoffs - Round " + playoffRound);
		Team game1X;
		Team game1Y;
		Team game2X;
		Team game2Y;

		switch (playoffRound)
		{
			case 1:
				if (!game1)
                {
					game1X = playoffTeams[0];
					game1Y = playoffTeams[1];

					if (Random.Range(0, game1X.strength) > Random.Range(0, game1Y.strength))
					{
						playoffTeams[4] = game1X;
						playoffTeams[5] = game1Y;
					}
					else
					{
						playoffTeams[4] = game1Y;
						playoffTeams[5] = game1X;
					}
				}
				
				if (!game2)
				{
					game2X = playoffTeams[2];
					game2Y = playoffTeams[3];

					if (Random.Range(0, game2X.strength) > Random.Range(0, game2Y.strength))
					{
						playoffTeams[6] = game2X;
					}
					else
					{
						playoffTeams[6] = game2Y;
					}
				}
				
				for (int i = 0; i < 7; i++)
                {
					brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
					brackDisplay[i].name.text = playoffTeams[i].name;
					brackDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[i].SetActive(true);
				}
				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SetPlayoffs();
				break;

			case 2:
				game1X = playoffTeams[5];
				game1Y = playoffTeams[6];

				if (Random.Range(0, game1X.strength) > Random.Range(0, game1Y.strength))
				{
					playoffTeams[7] = game1X;
				}
				else
				{
					playoffTeams[7] = game1Y;
				}

				for (int i = 0; i < 8; i++)
				{
					brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
					brackDisplay[i].name.text = playoffTeams[i].name;
					brackDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[i].SetActive(true);
				}
				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SetPlayoffs();
				break;

			case 3:
				game1X = playoffTeams[4];
				game1Y = playoffTeams[7];

				if (Random.Range(0, game1X.strength) > Random.Range(0, game1Y.strength))
				{
					playoffTeams[8] = game1X;
				}
				else
				{
					playoffTeams[8] = game1Y;
				}

				for (int i = 0; i < 9; i++)
				{
					brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
					brackDisplay[i].name.text = playoffTeams[i].name;
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

	IEnumerator LoadCareer()
	{
		gsp.LoadCareer();

		yield return careerEarningsText.text = "$ " + gsp.earnings.ToString();
	}

	IEnumerator SaveCareer(bool inProgress)
	{
		Debug.Log("Saving in PlayoffManager, inProgress is " + inProgress);

		myFile = new EasyFileSave("my_player_data");

		//myFile.Add("Career Record", gsp.record);
		Debug.Log("gsp.record is " + gsp.record.x + " - " + gsp.record.y);
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
		//myFile.Add("Draw", gsp.draw);
		myFile.Add("Number Of Teams", gsp.numberOfTeams);
		//myFile.Add("Player Team", gsp.playerTeamIndex);
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