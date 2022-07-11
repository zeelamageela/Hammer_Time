using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProvStandings : MonoBehaviour
{

	public StandingDisplay[] standDisplay;
	public Team[] teams;
	public List<Standings_List> provRankList;
	public Scrollbar scrollbar;

	GameObject[] row;

	public Transform standTextParent;
	public GameObject standTextRow;

	// Start is called before the first frame update
	private void Start()
    {
		
	}
    public void PrintRows()
    {
		Debug.Log("Printing Rows for Standings");

		CareerManager cm = FindObjectOfType<CareerManager>();

		if (provRankList != null)
        {
			provRankList.Clear();
			for (int i = 0; i < row.Length; i++)
            {
				Destroy(row[i]);
            }
		//teams = cm.teams;
			provRankList = cm.provRankList;
			row = new GameObject[provRankList.Count];
			standDisplay = new StandingDisplay[provRankList.Count];

			for (int i = 0; i < provRankList.Count; i++)
			{
				row[i] = Instantiate(standTextRow, standTextParent);
				row[i].name = "Row " + (i + 1);
				row[i].GetComponent<RectTransform>().position = new Vector2(0f, i * -125f);
				//Text[] tList = row.transform.GetComponentsInChildren<Text>();

				RowVariables rv = row[i].GetComponent<RowVariables>();
				//yield return new WaitForEndOfFrame();

				standDisplay[i] = rv.standDisplay;
			}

			provRankList.Sort();
			for (int i = 0; i < provRankList.Count; i++)
			{
				//Debug.Log("Counting to provRankLimit - " + i);
				standDisplay[i].name.text = provRankList[i].team.name;
				standDisplay[i].wins.text = provRankList[i].team.wins.ToString();
				standDisplay[i].loss.text = provRankList[i].team.loss.ToString();
				standDisplay[i].nextOpp.text = "$ " + provRankList[i].team.earnings.ToString("n0");
				provRankList[i].team.rank = i + 1;
			}

			for (int i = 0; i < provRankList.Count; i++)
			{
				if (cm.playerTeamIndex == provRankList[i].team.id)
				{
					scrollbar.value = (float)(i - provRankList.Count) / (1f - provRankList.Count);
					standDisplay[i].panel.enabled = true;
				}
				else
					standDisplay[i].panel.enabled = false;
			}
		}
		else
        {
			provRankList = new List<Standings_List>();
			teams = cm.teams;
			for (int i = 0; i < teams.Length; i++)
			{
				provRankList.Add(new Standings_List(teams[i]));
			}

			row = new GameObject[provRankList.Count];
			standDisplay = new StandingDisplay[provRankList.Count];

			for (int i = 0; i < provRankList.Count; i++)
			{
				row[i] = Instantiate(standTextRow, standTextParent);
				row[i].name = "Row " + (i + 1);
				row[i].GetComponent<RectTransform>().position = new Vector2(0f, i * -125f);
				//Text[] tList = row.transform.GetComponentsInChildren<Text>();

				RowVariables rv = row[i].GetComponent<RowVariables>();
				//yield return new WaitForEndOfFrame();

				standDisplay[i] = rv.standDisplay;
			}

			provRankList.Sort();
			for (int i = 0; i < provRankList.Count; i++)
			{
				//Debug.Log("Counting to provRankLimit - " + i);
				standDisplay[i].name.text = provRankList[i].team.name;
				standDisplay[i].wins.text = provRankList[i].team.wins.ToString();
				standDisplay[i].loss.text = provRankList[i].team.loss.ToString();
				standDisplay[i].nextOpp.text = "$ " + provRankList[i].team.earnings.ToString("n0");
				provRankList[i].team.rank = i + 1;
			}

			for (int i = 0; i < provRankList.Count; i++)
			{
				if (cm.playerTeamIndex == provRankList[i].team.id)
				{
					scrollbar.value = (float)(i - provRankList.Count) / (1f - provRankList.Count);
					standDisplay[i].panel.enabled = true;
				}
				else
					standDisplay[i].panel.enabled = false;
			}
		}
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

	}
}
