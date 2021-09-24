using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Random = UnityEngine.Random;

public class TournyManager : MonoBehaviour
{
	public StandingDisplay[] standDisplay;
	public Team[] teams;
	public DrawFormat[] drawFormat;

	public List<Team_List> teamList;

	public Button simButton;
	public Button contButton;
	public Text heading;
	GameSettingsPersist gsp;

	public int draw;
	// Start is called before the first frame update
	void Start()
    {
		gsp = GameObject.Find("GameSettingsPersist").GetComponent<GameSettingsPersist>();

		teamList = new List<Team_List>();

		Shuffle(teams);

		int playerTeam = Random.Range(0, 6);
		teams[playerTeam].name = gsp.teamName;

		for (int i = 0; i < teams.Length; i++)
		{
			teamList.Add(new Team_List(teams[i]));
			teams[i].strength = Random.Range(0, 10);
        }

		SetDraw();
		//PrintRows(teams);
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
		//panel.GetComponent<ContentSizeFitter>().enabled = false;

		//yield return new WaitForEndOfFrame();
		//panel.GetComponent<ContentSizeFitter>().enabled = true;
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
		teamList.Sort();

		for (int i = 0; i < teamList.Count; i++)
        {
			standDisplay[i].name.text = teamList[i].team.name;
			standDisplay[i].wins.text = teamList[i].team.wins.ToString();
			standDisplay[i].loss.text = teamList[i].team.loss.ToString();
			standDisplay[i].nextOpp.text = teamList[i].team.nextOpp;
		}

		StartCoroutine(RefreshPanel());
    }

	void SetDraw()
    {
		if (draw < drawFormat.Length)
		{
			teams[drawFormat[draw].game1.x].nextOpp = teams[drawFormat[draw].game1.y].name;
			teams[drawFormat[draw].game1.y].nextOpp = teams[drawFormat[draw].game1.x].name;


			teams[drawFormat[draw].game2.x].nextOpp = teams[drawFormat[draw].game2.y].name;
			teams[drawFormat[draw].game2.y].nextOpp = teams[drawFormat[draw].game2.x].name;


			teams[drawFormat[draw].game3.x].nextOpp = teams[drawFormat[draw].game3.y].name;
			teams[drawFormat[draw].game3.y].nextOpp = teams[drawFormat[draw].game3.x].name;
		}

		PrintRows();
    }

	public void SimDraw()
    {
		if (draw < drawFormat.Length)
		{
			//SetDraw();
			Team game1X = teams[drawFormat[draw].game1.x];
			Team game1Y = teams[drawFormat[draw].game1.y];

			Team game2X = teams[drawFormat[draw].game2.x];
			Team game2Y = teams[drawFormat[draw].game2.y];

			Team game3X = teams[drawFormat[draw].game3.x];
			Team game3Y = teams[drawFormat[draw].game3.y];

			if (Random.Range(0, game1X.strength) > Random.Range(0, game1Y.strength))
			{
				game1Y.loss++;
				game1X.wins++;
			}
			else
			{
				game1X.loss++;
				game1Y.wins++;
			}

			if (Random.Range(0, game2X.strength) > Random.Range(0, game2Y.strength))
			{
				game2Y.loss++;
				game2X.wins++;
			}
			else
			{
				game2X.loss++;
				game2Y.wins++;
			}

			if (Random.Range(0, game3X.strength) > Random.Range(0, game3Y.strength))
			{
				game3Y.loss++;
				game3X.wins++;
			}
			else
			{
				game3X.loss++;
				game3Y.wins++;
			}
			//Array.Sort(teams);
		}
		
		StartCoroutine(NextDraw());

	}

	IEnumerator NextDraw()
	{
		if (draw < drawFormat.Length - 1)
		{
			Debug.Log("Draw number " + draw);
			yield return new WaitForSeconds(0.1f);
			heading.text = "After Draw " + (draw + 1);
			SetDraw();
			draw++;
		}
		else if (draw == drawFormat.Length - 1)
        {
			Debug.Log("Final End");
			heading.text = "End of Draw";
			SetDraw();
			simButton.gameObject.SetActive(false);
			contButton.gameObject.SetActive(true);
        }
		else
			heading.text = "End of Round Robin";
    }

	void SetPlayoffs()
    {
		teamList[0].
    }

	public void Menu()
    {
		SceneManager.LoadScene("SplashMenu");
    }
}
