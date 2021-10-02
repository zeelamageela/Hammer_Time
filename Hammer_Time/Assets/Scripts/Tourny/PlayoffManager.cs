using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayoffManager : MonoBehaviour
{
	public TournyManager tm;

	public List<PlayoffTeam_List> playoffTeamList;
	public Team[] playoffTeams;

	public BracketDisplay[] brackDisplay;
	GameObject[] row;
	public GameObject playoffs;
	public GameObject semiWinner;
	public GameObject finalWinner;
	public GameObject vs;
	public Button simButton;
	public Button contButton;
	public Button playButton;
	public Text heading;
	public Scrollbar scrollBar;

	GameSettingsPersist gsp;

	int pTeams;
	public int playerTeam;

	private void Start()
	{

	}

	public void LoadTournySettings(int numberOfTeams)
    {

		playoffTeamList = new List<PlayoffTeam_List>();

		if (numberOfTeams % 2 == 0)
        {
			pTeams = numberOfTeams / 2;
        }
		else
        {
			pTeams = (numberOfTeams + 1) / 2;
		}
		playoffTeams = new Team[pTeams];

		switch (pTeams)
		{
			case 3:

				brackDisplay = new BracketDisplay[5];
				heading.text = "Semifinals";
				for (int i = 0; i < pTeams; i++)
				{
					playoffTeamList.Add(new PlayoffTeam_List(tm.teamList[i].team));
					brackDisplay[i].name.text = playoffTeamList[i].team.name;
					brackDisplay[i].rank.text = playoffTeamList[i].team.rank.ToString();

				}
				break;

			case 4:
				heading.text = "3 vs 4";
				for (int i = 0; i < pTeams; i++)
				{
					playoffTeams[i] = tm.teamList[i].team;
					brackDisplay[i].name.text = playoffTeams[i].name;
					brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
				}
				break;

			case 5:

				heading.text = "4 vs 5";
				for (int i = 0; i < pTeams; i++)
				{
					playoffTeams[i] = tm.teamList[i].team;
					brackDisplay[i].name.text = playoffTeams[i].name;
					brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
				}
				break;
			case 6:

				heading.text = "4 vs 5";
				for (int i = 0; i < pTeams; i++)
				{
					playoffTeams[i] = tm.teamList[i].team;
					brackDisplay[i].name.text = playoffTeams[i].name;
					brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
				}
				break;

		}
	}

	IEnumerator RefreshPlayoffPanel()
	{
		for (int i = 0; i < brackDisplay.Length; i++)
		{
			brackDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = false;

			yield return new WaitForEndOfFrame();
			brackDisplay[i].name.gameObject.transform.parent.GetComponent<ContentSizeFitter>().enabled = true;
		}

		for (int i = 0; i < tm.vsDisplay.Length; i++)
		{
			tm.vsDisplay[i].name.gameObject.GetComponent<ContentSizeFitter>().enabled = false;

			yield return new WaitForEndOfFrame();
			tm.vsDisplay[i].name.gameObject.GetComponent<ContentSizeFitter>().enabled = true;
		}
	}

	public void SetPlayoffs(int playoffRound)
	{

		switch (playoffRound)
		{
			case 0:
				
				switch (tm.teams[playerTeam].rank)
				{
					case 1:
						playButton.gameObject.SetActive(false);
						tm.teams[playerTeam].nextOpp = "-----";
						tm.vsDisplay[1].name.text = "BYE TO FINALS";
						tm.vsDisplay[1].rank.text = "-";
						break;
					case 2:
						playButton.gameObject.SetActive(true);
						tm.teams[playerTeam].nextOpp = playoffTeams[2].name;
						tm.vsDisplay[1].name.text = playoffTeams[2].name;
						tm.vsDisplay[1].rank.text = playoffTeams[2].rank.ToString();
						break;
					case 3:
						playButton.gameObject.SetActive(true);
						tm.teams[playerTeam].nextOpp = playoffTeams[1].name;
						tm.vsDisplay[1].name.text = playoffTeams[1].name;
						tm.vsDisplay[1].rank.text = playoffTeams[1].rank.ToString();
						break;
					default:
						playButton.gameObject.SetActive(false);
						vs.SetActive(false);
						playButton.gameObject.SetActive(false);
						break;
				}

				StartCoroutine(RefreshPlayoffPanel());

				standings.SetActive(false);
				playoffs.SetActive(true);
				playoffRound++;

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				scrollBar.value = 0;
				break;

			case 2:
				heading.text = "Finals";

				for (int i = 0; i < 4; i++)
				{
					brackDisplay[i].name.text = playoffTeams[i].name;
					brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
				}

				semiWinner.SetActive(true);
				brackDisplay[3].name.text = playoffTeams[3].name;
				brackDisplay[3].rank.text = playoffTeams[3].rank.ToString();

				if (playoffTeams[0].name == teams[playerTeam].name)
				{
					playButton.gameObject.SetActive(true);

					vsDisplay[0].name.text = playoffTeams[0].name;
					vsDisplay[0].rank.text = playoffTeams[0].rank.ToString();
					teams[playerTeam].nextOpp = playoffTeams[3].name;
					vsDisplay[1].name.text = playoffTeams[3].name;
					vsDisplay[1].rank.text = playoffTeams[3].rank.ToString();
				}
				else if (playoffTeams[3].name == teams[playerTeam].name)
				{
					playButton.gameObject.SetActive(true);
					vsDisplay[0].name.text = playoffTeams[3].name;
					vsDisplay[0].rank.text = playoffTeams[3].rank.ToString();
					teams[playerTeam].nextOpp = playoffTeams[0].name;
					vsDisplay[1].name.text = playoffTeams[0].name;
					vsDisplay[1].rank.text = playoffTeams[0].rank.ToString();
				}
				else
				{
					vs.SetActive(false);
					playButton.gameObject.SetActive(false);
				}

				standings.SetActive(false);
				playoffs.SetActive(true);
				StartCoroutine(RefreshPlayoffPanel());

				simButton.gameObject.SetActive(true);
				contButton.gameObject.SetActive(false);
				scrollBar.value = 0.5f;
				break;

			case 3:
				heading.text = "Winner";

				for (int i = 0; i < 5; i++)
				{
					brackDisplay[i].name.text = playoffTeams[i].name;
					brackDisplay[i].rank.text = playoffTeams[i].rank.ToString();
				}

				semiWinner.SetActive(true);
				finalWinner.SetActive(true);
				brackDisplay[4].name.text = playoffTeams[4].name;
				brackDisplay[4].rank.text = playoffTeams[4].rank.ToString();

				standings.SetActive(false);
				playoffs.SetActive(true);
				StartCoroutine(RefreshPlayoffPanel());

				if (teams[playerTeam].name == playoffTeams[4].name)
					heading.text = "You Win!";
				else
					heading.text = "So Close!";

				vs.SetActive(false);
				playButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(false);
				simButton.gameObject.SetActive(false);
				scrollBar.value = 1;

				for (int i = 0; i < playoffTeams.Length; i++)
				{
					if (playoffTeams[i].name == teams[playerTeam].name)
					{

					}
				}
				break;
		}
	}

	IEnumerator SimPlayoff()
	{
		switch (playoffRound)
		{
			case 1:
				Team semiX = playoffTeams[1];
				Team semiY = playoffTeams[2];

				if (Random.Range(0, semiX.strength) > Random.Range(0, semiY.strength))
				{
					playoffTeams[3] = semiX;
				}
				else
				{
					playoffTeams[3] = semiY;
				}

				semiWinner.SetActive(true);
				brackDisplay[3].rank.text = playoffTeams[3].rank.ToString();
				brackDisplay[3].name.text = playoffTeams[3].name;
				StartCoroutine(RefreshPlayoffPanel());
				playoffRound++;
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				break;

			case 2:
				if (Random.Range(0, playoffTeams[0].strength) > Random.Range(0, playoffTeams[3].strength))
				{
					playoffTeams[4] = playoffTeams[0];
				}
				else
				{
					playoffTeams[4] = playoffTeams[3];
				}
				playoffRound++;
				SetPlayoffs();
				simButton.gameObject.SetActive(false);
				contButton.gameObject.SetActive(true);
				break;

			default:
				break;

		}
		SetPlayoffs();
		yield break;
		//SetPlayoffs();
	}

}