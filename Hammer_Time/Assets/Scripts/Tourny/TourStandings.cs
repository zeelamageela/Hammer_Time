using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TourStandings : MonoBehaviour
{

	public StandingDisplay[] standDisplay;
	public Team[] teams;
	public List<TourStandings_List> tourRankList;
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
		CareerManager cm = FindObjectOfType<CareerManager>();

		if (tourRankList != null)
		{
			tourRankList.Clear();
			for (int i = 0; i < row.Length; i++)
			{
				Destroy(row[i]);
			}
			//teams = cm.teams;
			tourRankList = cm.tourRankList;
			row = new GameObject[tourRankList.Count];
			standDisplay = new StandingDisplay[tourRankList.Count];

			tourRankList.Sort();

			for (int i = 0; i < tourRankList.Count; i++)
			{
				row[i] = Instantiate(standTextRow, standTextParent);
				row[i].name = "Row " + (i + 1);
				row[i].GetComponent<RectTransform>().position = new Vector2(0f, i * -125f);
				//Text[] tList = row.transform.GetComponentsInChildren<Text>();

				RowVariables rv = row[i].GetComponent<RowVariables>();
				//yield return new WaitForEndOfFrame();

				standDisplay[i] = rv.standDisplay;
			}

			for (int i = 0; i < tourRankList.Count; i++)
			{
				//Debug.Log("Counting to tourRankLimit - " + i);
				standDisplay[i].name.text = tourRankList[i].team.name;
				standDisplay[i].wins.text = tourRankList[i].team.tourRecord.x.ToString();
				standDisplay[i].loss.text = tourRankList[i].team.tourRecord.y.ToString();
				standDisplay[i].nextOpp.text = tourRankList[i].team.tourPoints.ToString();
				tourRankList[i].team.rank = i + 1;
			}

			for (int i = 0; i < tourRankList.Count; i++)
			{
				if (cm.playerTeamIndex == tourRankList[i].team.id)
				{
					scrollbar.value = (i - tourRankList.Count) / (1f - tourRankList.Count);
					standDisplay[i].panel.enabled = true;
					Debug.Log("Scrollbar Value is " + scrollbar.value + " i is " + i);
				}
				else
					standDisplay[i].panel.enabled = false;
			}
		}
		else
		{
			tourRankList = new List<TourStandings_List>();
			teams = cm.tourTeams;
			for (int i = 0; i < teams.Length; i++)
			{
				tourRankList.Add(new TourStandings_List(teams[i]));
			}
			row = new GameObject[tourRankList.Count];
			standDisplay = new StandingDisplay[tourRankList.Count];

			tourRankList.Sort();

			for (int i = 0; i < tourRankList.Count; i++)
			{
				row[i] = Instantiate(standTextRow, standTextParent);
				row[i].name = "Row " + (i + 1);
				row[i].GetComponent<RectTransform>().position = new Vector2(0f, i * -125f);
				//Text[] tList = row.transform.GetComponentsInChildren<Text>();

				RowVariables rv = row[i].GetComponent<RowVariables>();
				//yield return new WaitForEndOfFrame();

				standDisplay[i] = rv.standDisplay;
			}

			for (int i = 0; i < tourRankList.Count; i++)
			{
				//Debug.Log("Counting to tourRankLimit - " + i);
				standDisplay[i].name.text = tourRankList[i].team.name;
				standDisplay[i].wins.text = tourRankList[i].team.tourRecord.x.ToString();
				standDisplay[i].loss.text = tourRankList[i].team.tourRecord.y.ToString();
				standDisplay[i].nextOpp.text = tourRankList[i].team.tourPoints.ToString();
				tourRankList[i].team.rank = i + 1;
			}

			for (int i = 0; i < tourRankList.Count; i++)
			{
				if (cm.playerTeamIndex == tourRankList[i].team.id)
				{
					scrollbar.value = (i - tourRankList.Count) / (1f - tourRankList.Count);
					standDisplay[i].panel.enabled = true;
					//Debug.Log("Scrollbar Value is " + scrollbar.value);
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

