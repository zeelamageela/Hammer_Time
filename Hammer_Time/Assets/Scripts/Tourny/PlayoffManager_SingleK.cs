using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TigerForge;
using static Photon.Pun.UtilityScripts.PunTeams;

public class PlayoffManager_SingleK : MonoBehaviour
{
	public TournyManager tm;
	public Team[] playoffTeams;

	public BracketDisplay[] roundOf16Display;
	public BracketDisplay[] quartersDisplay;
	public BracketDisplay[] semisDisplay;
	public BracketDisplay[] finalsDisplay;
	public BracketDisplay winnerDisplay;

	public GameObject[] row;
	public GameObject playoffs;
	public Button nextButton;
	public Button simButton;
	public Button contButton;
	public Button playButton;
	public Text heading;
	public Scrollbar scrollBar;
	public Text careerEarningsText;

	public Color yellow;
	public Color dark;
	public Color light;

	GameSettingsPersist gsp;
    CareerManager cm;

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
		cm = FindObjectOfType<CareerManager>();
        playoffs.SetActive(true);

		careerEarnings = tm.careerEarnings;
		careerRecord = tm.careerRecord;

		playerTeam = tm.playerTeam;
		playoffRound = gsp.playoffRound;
		playoffTeams = new Team[31];

		//Debug.Log("Career Earnings before playoffs - $ " + gsp.earnings.ToString());
		if (gsp.careerLoad)
		{
			Debug.Log("LOADING HERE");
			LoadPlayoffs();
		}
		else if (playoffRound > 0)
		{
			Debug.Log("LOADING HERE");
			LoadAndAdvancePlayoffs();
		}
		else
		{
			Debug.Log("LOADING HERE");
			SetSeeding(tm.teams.Length);
		}
	}

    public void SetSeeding(int numberOfTeams)
    {
        playoffTeams = new Team[31];
        tm.teams = new Team[16];
        heading.text = "Single Elimination";

        playoffRound++;

        // Use the teams from the tournament manager (should be set up before this)
        for (int i = 0; i < playoffTeams.Length; i++)
        {
            if (i < tm.teams.Length)
            {
                tm.teams[i].rank = 0;
                playoffTeams[i] = tm.teams[i];
                if (i < roundOf16Display.Length)
                {
                    roundOf16Display[i].name.text = playoffTeams[i].name;
                    roundOf16Display[i].rank.text = playoffTeams[i].rank.ToString();
                }
            }
            else
            {
                playoffTeams[i] = tm.tTeamList.nullTeam;
            }
        }

        tm.playoffRound = playoffRound;
        gsp.playoffTeams = playoffTeams;

        SetPlayoffs();
    }

    IEnumerator RefreshPlayoffPanel()
	{
		for (int i = 0; i < roundOf16Display.Length; i++)
		{
			roundOf16Display[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = false;

			yield return new WaitForEndOfFrame();
			roundOf16Display[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = true;
		}

		for (int i = 0; i < quartersDisplay.Length; i++)
		{
			quartersDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = false;

			yield return new WaitForEndOfFrame();
			quartersDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = true;
		}

		for (int i = 0; i < semisDisplay.Length; i++)
		{
			semisDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = false;

			yield return new WaitForEndOfFrame();
			semisDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = true;
		}

		for (int i = 0; i < finalsDisplay.Length; i++)
		{
			finalsDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = false;

			yield return new WaitForEndOfFrame();
			finalsDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = true;
		}

		winnerDisplay.name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = false;

		yield return new WaitForEndOfFrame();
		winnerDisplay.name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = true;

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

		int playerGame;
		int roundLength;

		for (int i = 0; i < playoffTeams.Length; i++)
        {
			playoffTeams[i] = gsp.playoffTeams[i];
        }
        //playoffTeams = gsp.playoffTeams;

        for (int i = playoffTeams.Length - 1; i >= 0; i--)
        {
			if (playoffTeams[i].player)
				playerTeam = i;
		}
		for (int i = playoffTeams.Length - 1; i >= 0; i--)
		{
			if (gsp.playoffTeams[i].player)
				if (i % 2 == 0)
				{
					oppTeam = i + 1;
				}
				else
				{
					oppTeam = i - 1;
				}
		}
		Debug.Log("OppTeam is " + oppTeam);
		switch (playoffRound)
        {
			case 1:
                #region Round 1
                playerGame = 0;
				roundLength = roundOf16Display.Length;
				
				for (int i = 0; i < roundLength; i++)
				{
					if (playoffTeams[i].player)
					{
						playerTeam = i;
						if (i % 2 == 0)
						{
							playerGame = i / 2;
						}
						else
						{
							playerGame = (i - 1) / 2;
						}
					}
				}

				Debug.Log("Game  " + playerGame + " - " + gsp.redTeamName + " - " + gsp.redScore + " vs " + gsp.yellowTeamName + " - " + gsp.yellowScore);
				if (gsp.playerTeam.name == gsp.redTeamName)
				{
					if (gsp.redScore > gsp.yellowScore)
					{
						Debug.Log("Player Red beat Yellow");
						playoffTeams[roundLength + playerGame] = playoffTeams[playerTeam];
						playoffTeams[oppTeam].rank = 9;
					}
					else
					{
						Debug.Log("Yellow beat Player Red");
						playoffTeams[roundLength + playerGame] = playoffTeams[oppTeam];
						playoffTeams[playerTeam].rank = 9;
					}
				}
				else
				{
					if (gsp.redScore < gsp.yellowScore)
					{
						Debug.Log("Player Yellow beat Red");
						playoffTeams[roundLength + playerGame] = playoffTeams[playerTeam];
						playoffTeams[oppTeam].rank = 9;
					}
					else
					{
						Debug.Log("Red beat Player Yellow");
						playoffTeams[roundLength + playerGame] = playoffTeams[oppTeam];
						playoffTeams[playerTeam].rank = 9;
					}
				}

				StartCoroutine(SimPlayoff(playerGame));
				break;
#endregion
            case 2:
                #region Round 2
                playerGame = 0;
				roundLength = quartersDisplay.Length + roundOf16Display.Length;

				for (int i = roundOf16Display.Length; i < roundLength; i++)
				{
					if (playoffTeams[i].player)
					{
						playerTeam = i;
						if (i % 2 == 0)
						{
							playerGame = i / 2;
							oppTeam = i + 1;
						}
						else
						{
							playerGame = (i - 1) / 2;
							oppTeam = i - 1;
						}
					}
				}

				Debug.Log("Game  " + playerGame + " - " + gsp.redTeamName + " - " + gsp.redScore + " vs " + gsp.yellowTeamName + " - " + gsp.yellowScore);
				if (gsp.playerTeam.name == gsp.redTeamName)
				{
					if (gsp.redScore > gsp.yellowScore)
					{
						Debug.Log("Player Red beat Yellow");
						playoffTeams[roundLength + playerGame] = playoffTeams[playerTeam];
						playoffTeams[oppTeam].rank = 5;
					}
					else
					{
						Debug.Log("Yellow beat Player Red");
						playoffTeams[roundLength + playerGame] = playoffTeams[oppTeam];
						playoffTeams[playerTeam].rank = 5;
					}
				}
				else
				{
					if (gsp.redScore < gsp.yellowScore)
					{
						Debug.Log("Player Yellow beat Red");
						playoffTeams[roundLength + playerGame] = playoffTeams[playerTeam];
						playoffTeams[oppTeam].rank = 5;
					}
					else
					{
						Debug.Log("Red beat Player Yellow");
						playoffTeams[roundLength + playerGame] = playoffTeams[oppTeam];
						playoffTeams[playerTeam].rank = 5;
					}
				}

				StartCoroutine(SimPlayoff(playerGame));
				break;
#endregion
            case 3:
                #region Round 3
                playerGame = 0;
				roundLength = semisDisplay.Length + quartersDisplay.Length + roundOf16Display.Length;

				for (int i = roundOf16Display.Length + quartersDisplay.Length; i < roundLength; i++)
				{
					if (playoffTeams[i].player)
					{
						playerTeam = i;
						if (i % 2 == 0)
						{
							playerGame = i / 2;
							oppTeam = i + 1;
						}
						else
						{
							playerGame = (i - 1) / 2;
							oppTeam = i - 1;
						}
					}
				}

				Debug.Log("Game  " + playerGame + " - " + gsp.redTeamName + " - " + gsp.redScore + " vs " + gsp.yellowTeamName + " - " + gsp.yellowScore);
				if (gsp.playerTeam.name == gsp.redTeamName)
				{
					if (gsp.redScore > gsp.yellowScore)
					{
						Debug.Log("Player Red beat Yellow");
						playoffTeams[roundLength + playerGame] = tm.teams[playerTeam];
						playoffTeams[oppTeam].rank = 3;
					}
					else
					{
						Debug.Log("Yellow beat Player Red");
						playoffTeams[roundLength + playerGame] = tm.teams[oppTeam];
						playoffTeams[playerTeam].rank = 3;
					}
				}
				else
				{
					if (gsp.redScore < gsp.yellowScore)
					{
						Debug.Log("Player Yellow beat Red");
						playoffTeams[roundLength + playerGame] = tm.teams[playerTeam];
						playoffTeams[oppTeam].rank = 3;
					}
					else
					{
						Debug.Log("Red beat Player Yellow");
						playoffTeams[roundLength + playerGame] = tm.teams[oppTeam];
						playoffTeams[playerTeam].rank = 3;
					}
				}

				StartCoroutine(SimPlayoff(playerGame));
				break;
			#endregion
			case 4:
				#region Round 4
				playerGame = 0;
				roundLength = finalsDisplay.Length + semisDisplay.Length + quartersDisplay.Length + roundOf16Display.Length;

				for (int i = semisDisplay.Length + quartersDisplay.Length + roundOf16Display.Length; i < roundLength; i++)
				{
					if (playoffTeams[i].player)
					{
						playerTeam = i;
						if (i % 2 == 0)
						{
							playerGame = i / 2;
							oppTeam = i + 1;
						}
						else
						{
							playerGame = (i - 1) / 2;
							oppTeam = i - 1;
						}
					}
				}

				Debug.Log("Game  " + playerGame + " - " + gsp.redTeamName + " - " + gsp.redScore + " vs " + gsp.yellowTeamName + " - " + gsp.yellowScore);
				if (gsp.playerTeam.name == gsp.redTeamName)
				{
					if (gsp.redScore > gsp.yellowScore)
					{
						Debug.Log("Player Red beat Yellow");
						playoffTeams[roundLength + playerGame] = tm.teams[playerTeam];
						playoffTeams[playerTeam].rank = 1;
						playoffTeams[oppTeam].rank = 2;
					}
					else
					{
						Debug.Log("Yellow beat Player Red");
						playoffTeams[roundLength + playerGame] = tm.teams[oppTeam];
						playoffTeams[playerTeam].rank = 2;
						playoffTeams[oppTeam].rank = 1;
					}
				}
				else
				{
					if (gsp.redScore < gsp.yellowScore)
					{
						Debug.Log("Player Yellow beat Red");
						playoffTeams[roundLength + playerGame] = tm.teams[playerTeam];
						playoffTeams[playerTeam].rank = 1;
						playoffTeams[oppTeam].rank = 2;
					}
					else
					{
						Debug.Log("Red beat Player Yellow");
						playoffTeams[roundLength + playerGame] = tm.teams[oppTeam];
						playoffTeams[playerTeam].rank = 2;
						playoffTeams[oppTeam].rank = 1;
					}
				}

				StartCoroutine(SimPlayoff(playerGame));
				break;
				#endregion
		}

		//SetPlayoffs();
	}

	void LoadPlayoffs()
	{
		gsp.careerLoad = false;
		//playoffRound--;
		Debug.Log("Load Playoffs - Round " + playoffRound);
		Debug.Log("gsp.playerTeam.nextOpp - " + gsp.playerTeam.nextOpp);

		if (gsp.playoffTeams != null && gsp.playoffTeams.Length > 0)
		{
			for (int i = 0; i < gsp.playoffTeams.Length; i++)
			{
				playoffTeams[i] = gsp.playoffTeams[i];
			}
		}
		else
        {
			List<Team_List> teamList = new List<Team_List>();


			for (int i = 0; i < tm.teams.Length; i++)
            {
				teamList.Add(new Team_List(tm.teams[i]));
            }
			//teamList.Sort();
			gsp.playoffTeams = new Team[playoffTeams.Length];
			Debug.Log("gsp.playoffTeams Length is " + gsp.playoffTeams.Length);

			for (int i = 0; i < playoffTeams.Length; i++)
			{
				for (int j = 0; j < teamList.Count; j++)
                {
					if (playoffTeams[i].id == teamList[j].team.id)
                    {
						playoffTeams[i] = teamList[j].team;
                    }
                }
				gsp.playoffTeams[i] = playoffTeams[i];
			}
        }
		//playoffTeams = gsp.playoffTeams;

		Debug.Log("OppTeam is " + oppTeam);

		int offset = roundOf16Display.Length;
		switch (playoffRound)
        {
			case 1:
				#region Round 1

				for (int i = 0; i < roundOf16Display.Length; i++)
				{
					roundOf16Display[i].rank.text = playoffTeams[i].rank.ToString();
					roundOf16Display[i].name.text = playoffTeams[i].name;
					roundOf16Display[i].name.transform.parent.gameObject.SetActive(true);
					row[i].SetActive(true);

					if (playoffTeams[i].player)
						roundOf16Display[i].bg.GetComponent<Image>().color = yellow;
				}

				for (int i = 0; i < quartersDisplay.Length; i++)
				{
					quartersDisplay[i].name.transform.parent.gameObject.SetActive(false);
					row[roundOf16Display.Length + i].SetActive(false);
				}

				for (int i = 0; i < semisDisplay.Length; i++)
				{
					semisDisplay[i].name.transform.parent.gameObject.SetActive(false);
					row[roundOf16Display.Length + quartersDisplay.Length + i].SetActive(false);
				}

				for (int i = 0; i < finalsDisplay.Length; i++)
				{
					finalsDisplay[i].name.transform.parent.gameObject.SetActive(false);
					row[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i].SetActive(false);
				}

				offset = 30;
				winnerDisplay.name.transform.parent.gameObject.SetActive(false);
				row[offset].SetActive(false);
				heading.text = "Loaded...Round of 16";
				#endregion
				break;

			case 2:
				#region Round 2

				for (int i = 0; i < roundOf16Display.Length; i++)
				{
					roundOf16Display[i].rank.text = playoffTeams[i].rank.ToString();
					roundOf16Display[i].name.text = playoffTeams[i].name;
					roundOf16Display[i].name.transform.parent.gameObject.SetActive(true);
					row[i].SetActive(true);

					if (playoffTeams[i].player)
						roundOf16Display[i].bg.GetComponent<Image>().color = yellow;
				}

				offset = roundOf16Display.Length;
				for (int i = 0; i < quartersDisplay.Length; i++)
				{
					Debug.Log("playoff round is " + playoffRound + " - i is " + i);
					quartersDisplay[i].name.text = playoffTeams[i + offset].name;
					quartersDisplay[i].rank.text = playoffTeams[i + offset].rank.ToString();
					quartersDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[offset + i].SetActive(true);

					if (playoffTeams[i + offset].player)
						quartersDisplay[i].bg.GetComponent<Image>().color = yellow;
				}

				for (int i = 0; i < semisDisplay.Length; i++)
				{
					semisDisplay[i].name.transform.parent.gameObject.SetActive(false);
					row[roundOf16Display.Length + quartersDisplay.Length + i].SetActive(false);
				}

				for (int i = 0; i < finalsDisplay.Length; i++)
				{
					finalsDisplay[i].name.transform.parent.gameObject.SetActive(false);
					row[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i].SetActive(false);
				}

				offset = 30;
				winnerDisplay.name.transform.parent.gameObject.SetActive(false);
				row[offset].SetActive(false);
				heading.text = "Loaded...Quarterfinals";
				#endregion
				break;

			case 3:
				#region Round 3

				for (int i = 0; i < roundOf16Display.Length; i++)
				{
					roundOf16Display[i].rank.text = playoffTeams[i].rank.ToString();
					roundOf16Display[i].name.text = playoffTeams[i].name;
					roundOf16Display[i].name.transform.parent.gameObject.SetActive(true);
					row[i].SetActive(true);

					if (playoffTeams[i].player)
						roundOf16Display[i].bg.GetComponent<Image>().color = yellow;
				}

				offset = roundOf16Display.Length;
				for (int i = 0; i < quartersDisplay.Length; i++)
				{
					Debug.Log("playoff round is " + playoffRound + " - i is " + i);
					quartersDisplay[i].name.text = playoffTeams[i + offset].name;
					quartersDisplay[i].rank.text = playoffTeams[i + offset].rank.ToString();
					quartersDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[offset + i].SetActive(true);

					if (playoffTeams[i + offset].player)
						quartersDisplay[i].bg.GetComponent<Image>().color = yellow;
				}

				offset = roundOf16Display.Length + quartersDisplay.Length;
				for (int i = 0; i < semisDisplay.Length; i++)
				{
					semisDisplay[i].name.text = playoffTeams[i + offset].name;
					semisDisplay[i].rank.text = playoffTeams[i + offset].rank.ToString();
					semisDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[offset + i].SetActive(true);

					if (playoffTeams[i + offset].player)
						semisDisplay[i].bg.GetComponent<Image>().color = yellow;
				}

				for (int i = 0; i < finalsDisplay.Length; i++)
				{
					finalsDisplay[i].name.transform.parent.gameObject.SetActive(false);
					row[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i].SetActive(false);
				}

				offset = 30;
				winnerDisplay.name.transform.parent.gameObject.SetActive(false);
				row[offset].SetActive(false);
				heading.text = "Loaded...Semifinals";
				#endregion
				break;

			case 4:
				#region Round 4

				for (int i = 0; i < roundOf16Display.Length; i++)
				{
					roundOf16Display[i].rank.text = playoffTeams[i].rank.ToString();
					roundOf16Display[i].name.text = playoffTeams[i].name;
					roundOf16Display[i].name.transform.parent.gameObject.SetActive(true);
					row[i].SetActive(true);

					if (playoffTeams[i].player)
						roundOf16Display[i].bg.GetComponent<Image>().color = yellow;
				}

				offset = roundOf16Display.Length;
				for (int i = 0; i < quartersDisplay.Length; i++)
				{
					quartersDisplay[i].name.text = playoffTeams[i + offset].name;
					quartersDisplay[i].rank.text = playoffTeams[i + offset].rank.ToString();
					quartersDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[offset + i].SetActive(true);

					if (playoffTeams[i + offset].player)
						quartersDisplay[i].bg.GetComponent<Image>().color = yellow;
				}

				offset = roundOf16Display.Length + quartersDisplay.Length;
				for (int i = 0; i < semisDisplay.Length; i++)
				{
					semisDisplay[i].name.text = playoffTeams[i + offset].name;
					semisDisplay[i].rank.text = playoffTeams[i + offset].rank.ToString();
					semisDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[offset + i].SetActive(true);

					if (playoffTeams[i + offset].player)
						semisDisplay[i].bg.GetComponent<Image>().color = yellow;
				}

				offset = roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length;
				for (int i = 0; i < finalsDisplay.Length; i++)
				{
					finalsDisplay[i].name.text = playoffTeams[i + offset].name;
					finalsDisplay[i].rank.text = playoffTeams[i + offset].rank.ToString();
					finalsDisplay[i].name.transform.parent.gameObject.SetActive(false);
					row[offset + i].SetActive(true);

					if (playoffTeams[i + offset].player)
						finalsDisplay[i].bg.GetComponent<Image>().color = yellow;
				}

				offset = 30;
				winnerDisplay.name.transform.parent.gameObject.SetActive(false);
				row[offset].SetActive(false);
				heading.text = "Loaded...Finals";
				#endregion
				break;

			case 5:
				#region Round 5

				for (int i = 0; i < roundOf16Display.Length; i++)
				{
					roundOf16Display[i].rank.text = playoffTeams[i].rank.ToString();
					roundOf16Display[i].name.text = playoffTeams[i].name;
					roundOf16Display[i].name.transform.parent.gameObject.SetActive(true);
					row[i].SetActive(true);
				}

				offset = roundOf16Display.Length;
				for (int i = 0; i < quartersDisplay.Length; i++)
				{
					quartersDisplay[i].name.text = playoffTeams[i + offset].name;
					quartersDisplay[i].rank.text = playoffTeams[i + offset].rank.ToString();
					quartersDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[offset + i].SetActive(true);
				}

				offset = roundOf16Display.Length + quartersDisplay.Length;
				for (int i = 0; i < semisDisplay.Length; i++)
				{
					semisDisplay[i].name.text = playoffTeams[i + offset].name;
					semisDisplay[i].rank.text = playoffTeams[i + offset].rank.ToString();
					semisDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[offset + i].SetActive(true);
				}

				offset = roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length;
				for (int i = 0; i < finalsDisplay.Length; i++)
				{
					finalsDisplay[i].name.text = playoffTeams[i + offset].name;
					finalsDisplay[i].rank.text = playoffTeams[i + offset].rank.ToString();
					finalsDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[offset + i].SetActive(true);
				}

				offset = roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + finalsDisplay.Length;
				winnerDisplay.name.text = playoffTeams[1 + offset].name;
				winnerDisplay.rank.text = playoffTeams[1 + offset].rank.ToString();
				winnerDisplay.name.transform.parent.gameObject.SetActive(true);
				row[offset + 1].SetActive(true);
				heading.text = "Loaded...Tourny Over";
				#endregion
				break;
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

		bool ko;
		int offset;
		switch (playoffRound)
        {
			case 1:
				#region Case 1

				heading.text = "Round of 16";
				playButton.gameObject.SetActive(true);

				for (int i = 0; i < roundOf16Display.Length; i++)
				{
					roundOf16Display[i].rank.text = playoffTeams[i].rank.ToString();
					roundOf16Display[i].name.text = playoffTeams[i].name;

					row[i].SetActive(true);

					if (playoffTeams[i].player)
					{
						roundOf16Display[i].panel.GetComponent<Image>().color = yellow;
						roundOf16Display[i].name.transform.parent.gameObject.SetActive(true);
					}
				}

				for (int i = 0; i < quartersDisplay.Length; i++)
				{
					quartersDisplay[i].name.transform.parent.gameObject.SetActive(false);
					row[roundOf16Display.Length + i].SetActive(false);
				}

				for (int i = 0; i < semisDisplay.Length; i++)
				{
					semisDisplay[i].name.transform.parent.gameObject.SetActive(false);
					row[roundOf16Display.Length + quartersDisplay.Length + i].SetActive(false);
				}

				for (int i = 0; i < finalsDisplay.Length; i++)
				{
					finalsDisplay[i].name.transform.parent.gameObject.SetActive(false);
					row[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i].SetActive(false);
				}

				winnerDisplay.name.transform.parent.gameObject.SetActive(false);
				row[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + finalsDisplay.Length].SetActive(false);

				for (int i = 0; i < roundOf16Display.Length; i++)
                {
					if (playoffTeams[i].player)
					{
						tm.vsDisplay[0].name.text = playoffTeams[i].name;
						tm.vsDisplay[0].rank.text = playoffTeams[i].rank.ToString();

						if (i % 2 == 0)
						{
							playoffTeams[i].nextOpp = playoffTeams[i + 1].name;
                            tm.vsDisplay[1].name.text = playoffTeams[i + 1].name;
							tm.vsDisplay[1].rank.text = playoffTeams[i + 1].rank.ToString();
							playoffTeams[i].nextOpp = playoffTeams[1 + 1].name;
						}
						else
                        {
                            playoffTeams[i].nextOpp = playoffTeams[i - 1].name;
                            tm.vsDisplay[1].name.text = playoffTeams[i - 1].name;
							tm.vsDisplay[1].rank.text = playoffTeams[i - 1].rank.ToString();
							playoffTeams[i].nextOpp = playoffTeams[1 - 1].name;
						}
					}
                }

                //StartCoroutine(RefreshPlayoffPanel());

                playoffs.SetActive(true);

                simButton.gameObject.SetActive(true);
                contButton.gameObject.SetActive(false);
                scrollBar.value = 0;
				for (int i = 0; i < tm.teams.Length; i++)
                {
					tm.teams[i] = playoffTeams[i];
                }
                break;
            #endregion
            case 2:
                #region Case 2
                heading.text = "Quarterfinals";

				ko = true;
				offset = roundOf16Display.Length;
				for (int i = 0; i < quartersDisplay.Length; i++)
				{
					if (playoffTeams[i + offset].player)
					{
						tm.vsDisplay[0].name.text = playoffTeams[roundOf16Display.Length + i].name;
						tm.vsDisplay[0].rank.text = playoffTeams[roundOf16Display.Length + i].rank.ToString();

						quartersDisplay[i].panel.GetComponent<Image>().color = yellow;
						quartersDisplay[i].name.transform.parent.gameObject.SetActive(true);

						if (i % 2 == 0)
						{
							tm.vsDisplay[1].name.text = playoffTeams[roundOf16Display.Length + i + 1].name;
							tm.vsDisplay[1].rank.text = playoffTeams[roundOf16Display.Length + i + 1].rank.ToString();
							playoffTeams[i + offset].nextOpp = playoffTeams[roundOf16Display.Length + i + 1].name;
						}
						else
						{
							tm.vsDisplay[1].name.text = playoffTeams[roundOf16Display.Length + i - 1].name;
							tm.vsDisplay[1].rank.text = playoffTeams[roundOf16Display.Length + i - 1].rank.ToString();
							playoffTeams[i + offset].nextOpp = playoffTeams[roundOf16Display.Length + i - 1].name;
						}
						ko = false;
					}
				}
				playButton.gameObject.SetActive(true);
                simButton.gameObject.SetActive(true);
                contButton.gameObject.SetActive(false);
                if (ko)
                {
                    playButton.gameObject.SetActive(false);

                    for (int i = 0; i < playoffTeams.Length; i++)
					{
						if (playoffTeams[i].player)
						{
							tm.vsDisplay[0].name.text = playoffTeams[i].name;
							tm.vsDisplay[0].rank.text = playoffTeams[i].rank.ToString();
						}
					}
					tm.vsDisplay[1].name.text = "Knocked Out!";
					tm.vsDisplay[1].rank.text = " ";
				}
				playoffs.SetActive(true);
                //StartCoroutine(RefreshPlayoffPanel());

                scrollBar.value = 0.25f;
                break;
            #endregion
            case 3:
                #region Case 3
                heading.text = "Semifinals";

				ko = true;

				offset = roundOf16Display.Length + quartersDisplay.Length;
				for (int i = 0; i < semisDisplay.Length; i++)
				{
					if (playoffTeams[i + offset].player)
					{
						semisDisplay[i].panel.GetComponent<Image>().color = yellow;
						tm.vsDisplay[0].name.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + i].name;
						tm.vsDisplay[0].rank.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + i].rank.ToString();

						if (i % 2 == 0)
						{
							tm.vsDisplay[1].name.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + i + 1].name;
							tm.vsDisplay[1].rank.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + i + 1].rank.ToString();
							playoffTeams[i].nextOpp = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + i + 1].name;
						}
						else
						{
							tm.vsDisplay[1].name.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + i - 1].name;
							tm.vsDisplay[1].rank.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + i - 1].rank.ToString();
							playoffTeams[i].nextOpp = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + i - 1].name;
						}
						ko = false;
					}
				}

				if (ko)
				{
					for (int i = 0; i < playoffTeams.Length; i++)
					{
						if (playoffTeams[i].player)
						{
							tm.vsDisplay[0].name.text = playoffTeams[i].name;
							tm.vsDisplay[0].rank.text = playoffTeams[i].rank.ToString();
						}
					}
					tm.vsDisplay[1].name.text = "Knocked Out!";
					tm.vsDisplay[1].rank.text = " ";
				}
				playoffs.SetActive(true);
				//StartCoroutine(RefreshPlayoffPanel());

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				scrollBar.value = 0.5f;
                break;
			#endregion
			case 4:
				#region Case 4
				heading.text = "Finals";

				ko = true;

				offset = roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length;
				for (int i = 0; i < finalsDisplay.Length; i++)
				{
					if (playoffTeams[i + offset].player)
					{
						finalsDisplay[i].panel.GetComponent<Image>().color = yellow;
						tm.vsDisplay[0].name.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i].name;
						tm.vsDisplay[0].rank.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i].rank.ToString();

						if (i % 2 == 0)
						{
							tm.vsDisplay[1].name.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i + 1].name;
							tm.vsDisplay[1].rank.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i + 1].rank.ToString();
							playoffTeams[i].nextOpp = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i + 1].name;
						}
						else
						{
							tm.vsDisplay[1].name.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i - 1].name;
							tm.vsDisplay[1].rank.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i - 1].rank.ToString();
							playoffTeams[i].nextOpp = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i - 1].name;
						}
						ko = false;
					}
				}

				if (ko)
				{
					for (int i = 0; i < playoffTeams.Length; i++)
					{
						if (playoffTeams[i].player)
						{
							tm.vsDisplay[0].name.text = playoffTeams[i].name;
							tm.vsDisplay[0].rank.text = playoffTeams[i].rank.ToString();
						}
					}
					tm.vsDisplay[1].name.text = "Knocked Out!";
					tm.vsDisplay[1].rank.text = "X";
				}
				playoffs.SetActive(true);
				//StartCoroutine(RefreshPlayoffPanel());

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				scrollBar.value = 0.75f;
                break;
			#endregion
			case 5:
				#region Case 5

				heading.text = "Finals";
				winnerDisplay.name.text = playoffTeams[30].name;
				winnerDisplay.rank.text = playoffTeams[30].rank.ToString();
				winnerDisplay.bg.GetComponent<Image>().color = yellow;
				//brackDisplay[i].name.transform.parent.gameObject.SetActive(true);
				row[30].SetActive(true);

				playoffs.SetActive(true);
                //StartCoroutine(RefreshPlayoffPanel());

				tm.vsTitle.text = "Results";
				tm.vsVS.text = " ";
				tm.vs.SetActive(true);

				for (int i = 24; i < playoffTeams.Length; i++)
                {
                    if (i == 30)
                    {
                        playoffTeams[i].earnings = gsp.prize * 0.5f;
                        playoffTeams[i].rank = 1;

                        if (playoffTeams[i].player)
                        {
							heading.text = "You Win!";
							gsp.tournyEarnings += gsp.prize * 0.5f;

							tm.vs.SetActive(true);

							tm.vsDisplay[1].name.text = "$" + (gsp.prize * 0.5f).ToString("n0");
							tm.vsDisplay[0].name.text = playoffTeams[30].name;
							tm.vsDisplay[0].rank.text = playoffTeams[30].rank.ToString() + "st";
							tm.vsDisplay[1].rank.gameObject.SetActive(false);
						}
                    }
                    else if (i == 28 | i == 29)
                    {
						if (playoffTeams[i].id != playoffTeams[30].id)
						{
							playoffTeams[i].earnings = gsp.prize * 0.25f;
                            playoffTeams[i].rank = 2;

                            if (playoffTeams[i].player)
							{
								heading.text = "Runner-up";
								gsp.tournyEarnings += gsp.prize * 0.25f;

								tm.vs.SetActive(true);

								tm.vsDisplay[1].name.text = "$" + (gsp.prize * 0.25f).ToString("n0");
								tm.vsDisplay[0].name.text = playoffTeams[i].name;
								tm.vsDisplay[0].rank.text = playoffTeams[i].rank.ToString() + "st";
								tm.vsDisplay[1].rank.gameObject.SetActive(false);
							}
						}
                    }
					else
                    {
						if (playoffTeams[i].id != playoffTeams[28].id | playoffTeams[i].id != playoffTeams[29].id)
						{
							playoffTeams[i].earnings = gsp.prize * 0.125f;
                            playoffTeams[i].rank = 3;

                            if (playoffTeams[i].player)
							{
								heading.text = "3rd Place";
								gsp.tournyEarnings += gsp.prize * 0.125f;

								tm.vs.SetActive(true);

								tm.vsDisplay[1].name.text = "$" + (gsp.prize * 0.125f).ToString("n0");
								tm.vsDisplay[0].name.text = playoffTeams[i].name;
								tm.vsDisplay[0].rank.text = playoffTeams[i].rank.ToString() + "st";
								tm.vsDisplay[1].rank.gameObject.SetActive(false);
							}
						}
                    }
                }

                Debug.Log("GSP Earnings after calculation - " + gsp.tournyEarnings.ToString());
				careerEarningsText.text = "$ " + gsp.tournyEarnings.ToString("n0");

                //gsp.record = new Vector2(gsp.record.x + tm.teams[playerTeam].wins, gsp.record.y + tm.teams[playerTeam].loss);

                //heading.text = "So Close!";

                playButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(false);
				simButton.gameObject.SetActive(false);
				nextButton.gameObject.SetActive(true);
				scrollBar.value = 1;

				break;
                #endregion

        }
		cm.SaveCareer();
    }

	public void OnSim()
    {
		switch (playoffRound)
        {
            case 1:
				StartCoroutine(SimPlayoff(99));
				break;
			case 2:
				StartCoroutine(SimPlayoff(99));
				break;
			case 3:
				StartCoroutine(SimPlayoff(99));
				break;
			case 4:
				StartCoroutine(SimPlayoff(99));
				break;
			case 5:
				StartCoroutine(SimPlayoff(99));
				break;
		}
    }

    IEnumerator SimPlayoff(int playerGame)
	{

		Debug.Log("Sim Playoffs - Round " + playoffRound);
		Team gameX;
		Team gameY;
		int game = 0;

		switch (playoffRound)
		{
			case 1:
                #region Round 1
                for (int i = 0; i < roundOf16Display.Length; i++)
				{
					if (i % 2 == 0)
					{
						game = i / 2;
						gameX = playoffTeams[i];
						gameY = playoffTeams[i + 1];

						if (game != playerGame)
						{
							if (Random.Range(0, gameX.strength) > Random.Range(0, gameY.strength))
							{
								playoffTeams[roundOf16Display.Length + game] = gameX;
								gameX.wins++;
								gameX.rank = 0;
								gameY.loss++;
								gameY.rank = 9;
							}
							else
							{
								playoffTeams[roundOf16Display.Length + game] = gameY;
								gameX.loss++;
								gameX.rank = 9;
								gameY.wins++;
								gameY.rank = 0;
							}
						}
					}

				}

				for (int i = 0; i < roundOf16Display.Length; i++)
				{
					if (playoffTeams[i].rank == 9)
					{
						roundOf16Display[i].rank.text = "KO";
						roundOf16Display[i].bg.GetComponent<Image>().color = dark;
					}
					else
					{
						roundOf16Display[i].rank.text = playoffTeams[i].rank.ToString();
						roundOf16Display[i].bg.GetComponent<Image>().color = yellow;
					}

					roundOf16Display[i].name.text = playoffTeams[i].name;
					roundOf16Display[i].name.transform.parent.gameObject.SetActive(true);
					row[i].SetActive(true);
				}

				for (int i = 0; i < quartersDisplay.Length; i++)
				{
					quartersDisplay[i].rank.text = playoffTeams[roundOf16Display.Length + i].rank.ToString();
					quartersDisplay[i].name.text = playoffTeams[roundOf16Display.Length + i].name;
					quartersDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[roundOf16Display.Length + i].SetActive(true);
				}

				for (int i = 0; i < semisDisplay.Length; i++)
				{
					semisDisplay[i].name.transform.parent.gameObject.SetActive(false);
					row[roundOf16Display.Length + quartersDisplay.Length + i].SetActive(false);
				}

				for (int i = 0; i < finalsDisplay.Length; i++)
				{
					finalsDisplay[i].name.transform.parent.gameObject.SetActive(false);
					row[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i].SetActive(false);
				}

				winnerDisplay.name.transform.parent.gameObject.SetActive(false);
				row[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + finalsDisplay.Length].SetActive(false);

				//StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SetPlayoffs();
				break;
            #endregion
            case 2:
                #region Round 2
                for (int i = 0; i < quartersDisplay.Length; i++)
				{
					if (i % 2 == 0)
					{
						game = i / 2;
						gameX = playoffTeams[roundOf16Display.Length + i];
						gameY = playoffTeams[roundOf16Display.Length + i + 1];

						if (game != playerGame)
						{
							if (Random.Range(0, gameX.strength) > Random.Range(0, gameY.strength))
							{
								playoffTeams[roundOf16Display.Length + quartersDisplay.Length + game] = gameX;
								gameX.wins++;
								gameX.rank = 0;
								gameY.loss++;
								gameY.rank = 5;
							}
							else
							{
								playoffTeams[roundOf16Display.Length + quartersDisplay.Length + game] = gameY;
								gameX.loss++;
								gameX.rank = 5;
								gameY.wins++;
								gameY.rank = 0;
							}
						}
					}

				}

				for (int i = 0; i < roundOf16Display.Length; i++)
				{
					if (playoffTeams[i].rank == 9)
					{
						roundOf16Display[i].rank.text = "KO";
						roundOf16Display[i].bg.GetComponent<Image>().color = dark;
					}
					else
					{
						roundOf16Display[i].rank.text = playoffTeams[i].rank.ToString();
						roundOf16Display[i].bg.GetComponent<Image>().color = yellow;
					}

					roundOf16Display[i].name.text = playoffTeams[i].name;
					roundOf16Display[i].name.transform.parent.gameObject.SetActive(true);
					row[i].SetActive(true);
				}

				for (int i = 0; i < quartersDisplay.Length; i++)
				{
					if (playoffTeams[roundOf16Display.Length + i].rank == 5)
					{
						quartersDisplay[i].rank.text = "KO";
						quartersDisplay[i].bg.GetComponent<Image>().color = dark;
					}
					else
					{
						quartersDisplay[i].rank.text = playoffTeams[roundOf16Display.Length + i].rank.ToString();
						quartersDisplay[i].bg.GetComponent<Image>().color = yellow;
					}

					quartersDisplay[i].name.text = playoffTeams[roundOf16Display.Length + i].name;
					quartersDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[roundOf16Display.Length + i].SetActive(true);
				}

				for (int i = 0; i < semisDisplay.Length; i++)
				{
					Debug.Log("playoffTeams Length is " + playoffTeams.Length + " - playoffTeams range is " + (roundOf16Display.Length + quartersDisplay.Length + i));
                    semisDisplay[i].rank.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + i].rank.ToString();
					semisDisplay[i].name.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + i].name;
					semisDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[roundOf16Display.Length + quartersDisplay.Length + i].SetActive(true);
				}

				for (int i = 0; i < finalsDisplay.Length; i++)
				{
					finalsDisplay[i].name.transform.parent.gameObject.SetActive(false);
					row[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i].SetActive(false);
				}

				winnerDisplay.name.transform.parent.gameObject.SetActive(false);
				row[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + finalsDisplay.Length].SetActive(false);

				//StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SetPlayoffs();
				break;
            #endregion
            case 3:
                #region Round 3
                for (int i = 0; i < semisDisplay.Length; i++)
				{
					if (i % 2 == 0)
					{
						game = i / 2;
						gameX = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + i];
						gameY = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + i + 1];

						if (game != playerGame)
						{
							if (Random.Range(0, gameX.strength) > Random.Range(0, gameY.strength))
                            {
								playoffTeams[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + game] = gameX;
								gameX.wins++;
								gameX.rank = 0;
								gameY.loss++;
								gameY.rank = 3;
							}
							else
							{
								playoffTeams[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + game] = gameY;
								gameX.loss++;
								gameX.rank = 3;
								gameY.wins++;
								gameY.rank = 0;
							}
						}
					}

				}

				for (int i = 0; i < roundOf16Display.Length; i++)
				{
					if (playoffTeams[i].rank == 9)
					{
						roundOf16Display[i].rank.text = "KO";
						roundOf16Display[i].bg.GetComponent<Image>().color = dark;
					}
					else
					{
						roundOf16Display[i].rank.text = playoffTeams[i].rank.ToString();
						roundOf16Display[i].bg.GetComponent<Image>().color = yellow;
					}

					roundOf16Display[i].name.text = playoffTeams[i].name;
					roundOf16Display[i].name.transform.parent.gameObject.SetActive(true);
					row[i].SetActive(true);
				}

				for (int i = 0; i < quartersDisplay.Length; i++)
				{
					if (playoffTeams[roundOf16Display.Length + i].rank == 5)
					{
						quartersDisplay[i].rank.text = "KO";
						quartersDisplay[i].bg.GetComponent<Image>().color = dark;
					}
					else
					{
						quartersDisplay[i].rank.text = playoffTeams[roundOf16Display.Length + i].rank.ToString();
						quartersDisplay[i].bg.GetComponent<Image>().color = yellow;
					}

					quartersDisplay[i].name.text = playoffTeams[roundOf16Display.Length + i].name;
					quartersDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[roundOf16Display.Length + i].SetActive(true);
				}

				for (int i = 0; i < semisDisplay.Length; i++)
				{
					if (playoffTeams[roundOf16Display.Length + quartersDisplay.Length + i].rank == 3)
					{
						semisDisplay[i].rank.text = "3rd";
						semisDisplay[i].bg.GetComponent<Image>().color = dark;
					}
					else
					{
						semisDisplay[i].rank.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + i].rank.ToString();
						semisDisplay[i].bg.GetComponent<Image>().color = yellow;
					}

					semisDisplay[i].name.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + i].name;
					semisDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[roundOf16Display.Length + quartersDisplay.Length + i].SetActive(true);
				}

				for (int i = 0; i < finalsDisplay.Length; i++)
				{
					finalsDisplay[i].rank.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i].rank.ToString();
					finalsDisplay[i].name.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i].name;
					finalsDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i].SetActive(true);
				}

				winnerDisplay.name.transform.parent.gameObject.SetActive(false);
				row[30].SetActive(false);

				//StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SetPlayoffs();
				break;
            #endregion
            case 4:
                #region Round 4
                if (playerGame == 99)
				{
					gameX = playoffTeams[28];
					gameY = playoffTeams[29];

					if (Random.Range(0, gameX.strength) > Random.Range(0, gameY.strength))
					{
						playoffTeams[30] = gameX;
						gameX.wins++;
						gameX.rank = 1;
						gameY.loss++;
						gameY.rank = 2;
					}
					else
					{
						playoffTeams[30] = gameY;
						gameX.loss++;
						gameX.rank = 2;
						gameY.wins++;
						gameY.rank = 1;
					}
				}

				for (int i = 0; i < roundOf16Display.Length; i++)
				{
					if (playoffTeams[i].rank == 9)
					{
						roundOf16Display[i].rank.text = "KO";
						roundOf16Display[i].bg.GetComponent<Image>().color = dark;
					}
					else
					{
						roundOf16Display[i].rank.text = playoffTeams[i].rank.ToString();
						roundOf16Display[i].bg.GetComponent<Image>().color = yellow;
					}

					roundOf16Display[i].name.text = playoffTeams[i].name;
					roundOf16Display[i].name.transform.parent.gameObject.SetActive(true);
					row[i].SetActive(true);
				}

				for (int i = 0; i < quartersDisplay.Length; i++)
				{
					if (playoffTeams[roundOf16Display.Length + i].rank == 5)
					{
						quartersDisplay[i].rank.text = "KO";
						quartersDisplay[i].bg.GetComponent<Image>().color = dark;
					}
					else
					{
						quartersDisplay[i].rank.text = playoffTeams[roundOf16Display.Length + i].rank.ToString();
						quartersDisplay[i].bg.GetComponent<Image>().color = yellow;
					}

					quartersDisplay[i].name.text = playoffTeams[roundOf16Display.Length + i].name;
					quartersDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[roundOf16Display.Length + i].SetActive(true);
				}

				for (int i = 0; i < semisDisplay.Length; i++)
				{
					if (playoffTeams[roundOf16Display.Length + quartersDisplay.Length + i].rank == 3)
					{
						semisDisplay[i].rank.text = "3rd";
						semisDisplay[i].bg.GetComponent<Image>().color = dark;
					}
					else
					{
						semisDisplay[i].rank.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + i].rank.ToString();
						semisDisplay[i].bg.GetComponent<Image>().color = yellow;
					}

					semisDisplay[i].name.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + i].name;
					semisDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[roundOf16Display.Length + quartersDisplay.Length + i].SetActive(true);
				}

				for (int i = 0; i < finalsDisplay.Length; i++)
				{
					if (playoffTeams[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i].rank == 2)
					{
						finalsDisplay[i].rank.text = "2nd";
						finalsDisplay[i].bg.GetComponent<Image>().color = dark;
					}
					else
					{
						finalsDisplay[i].rank.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i].rank.ToString();
						finalsDisplay[i].bg.GetComponent<Image>().color = yellow;
					}

					finalsDisplay[i].name.text = playoffTeams[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i].name;
					finalsDisplay[i].name.transform.parent.gameObject.SetActive(true);
					row[roundOf16Display.Length + quartersDisplay.Length + semisDisplay.Length + i].SetActive(true);
				}

				winnerDisplay.rank.text = "1st";
				winnerDisplay.name.text = playoffTeams[30].name;
				winnerDisplay.name.transform.parent.gameObject.SetActive(true);
				row[30].SetActive(true);

				//StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				SetPlayoffs();
				break;
            #endregion
            default:
				Debug.Log("Bonk! Need another round");
				SetPlayoffs();
				break;

		}

		yield break;
	}

	IEnumerator LoadCareer()
	{
		gsp.LoadCareer();

		yield return careerEarningsText.text = "$ " + gsp.tournyEarnings.ToString();
	}
    public void SavePlayoff()
    {
        if (cm != null)
            cm.SaveTournyState(this);
    }

    IEnumerator SaveCareer(bool inProgress)
	{
		Debug.Log("Saving in PlayoffManager, inProgress is " + inProgress);

		myFile = new EasyFileSave("my_player_data");

		//myFile.Add("Career Record", gsp.record);
		//Debug.Log("gsp.record is " + tm.teams[tm.playerTeam].wins + " - " + tm.teams[tm.playerTeam].loss);
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
		Debug.Log("gsp.inProgress is " + gsp.tournyInProgress);
		myFile.Add("Single Knockout Tourny", gsp.KO1);
		//myFile.Add("Draw", gsp.draw);
		myFile.Add("Number Of Teams", gsp.numberOfTeams);
		//myFile.Add("Player Team", gsp.playerTeamIndex);
		myFile.Add("OppTeam", oppTeam);
		myFile.Add("Playoff Round", playoffRound);

		string[] nameList = new string[gsp.teams.Length];
		int[] winsList = new int[gsp.teams.Length];
		int[] lossList = new int[gsp.teams.Length];
		int[] rankList = new int[gsp.teams.Length];
		int[] strengthList = new int[gsp.teams.Length];
		int[] idList = new int[gsp.teams.Length];
		float[] earningsList = new float[gsp.teams.Length];
		bool[] playerList = new bool[gsp.teams.Length];

		for (int i = 0; i < gsp.teams.Length; i++)
		{
			nameList[i] = gsp.teams[i].name;
			winsList[i] = gsp.teams[i].wins;
			lossList[i] = gsp.teams[i].loss;
			rankList[i] = gsp.teams[i].rank;
			strengthList[i] = gsp.teams[i].strength;
			idList[i] = gsp.teams[i].id;
			earningsList[i] = gsp.teams[i].earnings;
			playerList[i] = gsp.teams[i].player;
		}

		myFile.Add("Tourny Name List", nameList);
		myFile.Add("Tourny Wins List", winsList);
		myFile.Add("Tourny Loss List", lossList);
		myFile.Add("Tourny Rank List", rankList);
		myFile.Add("Tourny Strength List", strengthList);
		myFile.Add("Tourny Team ID List", idList);
		myFile.Add("Tourny Earnings List", earningsList);
		myFile.Add("Tourny Player List", playerList);

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