using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TournyManager : MonoBehaviour
{
	public StandingDisplay[] standDisplay;
	public Team[] teams;
	public DrawFormat[] drawFormat;

	public Text heading;
	GameSettingsPersist gsp;

	public int draw;

	// Start is called before the first frame update
	void Start()
    {
		gsp = GameObject.Find("GameSettingsPersist").GetComponent<GameSettingsPersist>();

		int playerTeam = Random.Range(0, 6);
		teams[playerTeam].name = gsp.teamName;

		for (int i = 0; i < teams.Length; i++)
        {
			teams[i].strength = Random.Range(0, 10);
        }
		PrintRows(teams);
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
		PrintRows(a);
		//for (int i = 0; i < a.Length; i++)
		//{
		//	Print;
		//}
	}

	void PrintRows(Team[] a)
    {
		for (int i = 0; i < a.Length; i++)
        {
			standDisplay[i].name.text = a[i].name;
			standDisplay[i].wins.text = a[i].wins.ToString();
			standDisplay[i].loss.text = a[i].loss.ToString();
        }
		SetDraw();
    }

	void SetDraw()
    {
		teams[drawFormat[draw].game1.x].nextOpp = teams[drawFormat[draw].game1.y].name;
		teams[drawFormat[draw].game1.y].nextOpp = teams[drawFormat[draw].game1.x].name;


		teams[drawFormat[draw].game2.x].nextOpp = teams[drawFormat[draw].game2.y].name;
		teams[drawFormat[draw].game2.y].nextOpp = teams[drawFormat[draw].game2.x].name;


		teams[drawFormat[draw].game3.x].nextOpp = teams[drawFormat[draw].game3.y].name;
		teams[drawFormat[draw].game3.y].nextOpp = teams[drawFormat[draw].game3.x].name;

		for (int i = 0; i < teams.Length; i++)
        {
			standDisplay[i].nextOpp.text = teams[i].nextOpp;
        }
    }

	public void SimDraw()
    {
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

		heading.text = "After Draw " + draw;
		StartCoroutine(NextDraw());
	}

	IEnumerator NextDraw()
    {
		if (draw < drawFormat.Length)
			draw++;
		else
			heading.text = "End of Round Robin";
		yield return new WaitForSeconds(0.1f);
		PrintRows(teams);

    }
}
