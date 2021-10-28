using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProvStandings : MonoBehaviour
{

	public StandingDisplay[] standDisplay;
	public Team[] teams;
	public List<Standings_List> provRankList;
	GameObject[] row;

	public Transform standTextParent;
	public GameObject standTextRow;

	// Start is called before the first frame update
	private void Start()
    {
		
	}
    public void PrintRows()
    {
		CareerManager cm = FindObjectOfType<CareerManager>();

		if (provRankList != null)
        {
			provRankList.Clear();
        }
		teams = cm.teams;
		provRankList = cm.provRankList;
		row = new GameObject[teams.Length];
		standDisplay = new StandingDisplay[teams.Length];

		for (int i = 0; i < cm.totalTeams; i++)
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
		for (int i = 0; i < cm.totalTeams; i++)
		{
            Debug.Log("Counting to provRankLimit - " + i);
			standDisplay[i].name.text = provRankList[i].team.name;
			standDisplay[i].wins.text = provRankList[i].team.wins.ToString();
			standDisplay[i].loss.text = provRankList[i].team.loss.ToString();
			standDisplay[i].nextOpp.text = "$ " + provRankList[i].team.earnings.ToString();
			provRankList[i].team.rank = i + 1;
		}

		for (int i = 0; i < provRankList.Count; i++)
		{
			if (cm.playerTeamIndex == provRankList[i].team.id)
				standDisplay[i].panel.enabled = true;
			else
				standDisplay[i].panel.enabled = false;
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
