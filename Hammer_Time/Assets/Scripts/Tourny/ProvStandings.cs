using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProvStandings : MonoBehaviour
{

	public StandingDisplay[] standDisplay;
	public Team[] teams;
	public List<Standings_List> provRankList;

	public List<TourStandings_List> tourRankList;

	public Scrollbar scrollbar;

	GameObject[] row;

	public Text[] headers;
	public Text buttonText;

	public Transform standTextParent;
	public GameObject standTextRow;

	public bool tour;

	public GameObject tourButton;
	// Start is called before the first frame update
	private void Start()
    {
		tour = false;
		if (FindObjectOfType<CareerManager>().provQual)
			tourButton.SetActive(true);
		else
			tourButton.SetActive(false);
	}

	public void SetUp()
    {
		tour = false;
		PrintRows();
    }

    public void PrintRows()
    {

		CareerManager cm = FindObjectOfType<CareerManager>();

		if (row != null)
		{
			for (int i = 0; i < row.Length; i++)
			{
				Destroy(row[i]);
			}
		}

		if (tour)
		{
			Debug.Log("Printing Rows for Tour Standings");

			buttonText.text = "<<Overall Standings";
			headers[2].text = "Points";


			if (tourRankList != null)
			{
				Debug.Log("tourRankList is not Null");
				//tourRankList.Clear();
				////teams = cm.teams;
				//tourRankList = cm.tourRankList;
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
					standDisplay[i].name.text = tourRankList[i].team.rank.ToString() + " - " + tourRankList[i].team.name;
					standDisplay[i].wins.text = tourRankList[i].team.tourRecord.x.ToString();
					standDisplay[i].loss.text = tourRankList[i].team.tourRecord.y.ToString();
					standDisplay[i].nextOpp.text = tourRankList[i].team.tourPoints.ToString();
					tourRankList[i].team.rank = i + 1;
				}

				int tempRank = 0;
				for (int i = 0; i < tourRankList.Count; i++)
				{
					if (cm.playerTeamIndex == tourRankList[i].team.id)
					{
						tempRank = i;
						//scrollbar.value = (i - tourRankList.Count) / (1f - tourRankList.Count);
						standDisplay[i].panel.enabled = true;
					}
					else
						standDisplay[i].panel.enabled = false;
				}
				scrollbar.value = (tempRank - tourRankList.Count) / (1f - tourRankList.Count);
				Debug.Log("Scrollbar Value is " + scrollbar.value + " i is " + tempRank);
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
					standDisplay[i].name.text = (i + 1) + " - " + tourRankList[i].team.name;
					standDisplay[i].wins.text = tourRankList[i].team.tourRecord.x.ToString();
					standDisplay[i].loss.text = tourRankList[i].team.tourRecord.y.ToString();
					standDisplay[i].nextOpp.text = tourRankList[i].team.tourPoints.ToString();
					tourRankList[i].team.rank = i + 1;
				}

				int tempRank = 0;
				for (int i = 0; i < tourRankList.Count; i++)
				{
					if (cm.playerTeamIndex == tourRankList[i].team.id)
					{
						tempRank = i;
						standDisplay[i].panel.enabled = true;
						//Debug.Log("Scrollbar Value is " + scrollbar.value);
					}
					else
						standDisplay[i].panel.enabled = false;
				}
				scrollbar.value = (tempRank - tourRankList.Count) / (1f - tourRankList.Count);
				Debug.Log("Scrollbar Value is " + scrollbar.value + " i is " + tempRank);
			}
			tour = false;
        }
		else
		{
			Debug.Log("Printing Rows for Overall Standings");

			buttonText.text = "Tour Standings>>";
			headers[2].text = "Earnings";

			if (provRankList != null)
			{
				Debug.Log("provRankList is not Null");
				//teams = cm.teams;

				//if (cm.provRankList.Count > 0)
				//{
				//	provRankList.Clear();
				//	provRankList = cm.provRankList;
				//}

				row = new GameObject[provRankList.Count];
				standDisplay = new StandingDisplay[provRankList.Count];
				Debug.Log("provRankList Count is " + provRankList.Count + " - cm.prList Count is " + cm.provRankList.Count);

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
					standDisplay[i].name.text = (i + 1) + " - " + provRankList[i].team.name;
					standDisplay[i].wins.text = provRankList[i].team.wins.ToString();
					standDisplay[i].loss.text = provRankList[i].team.loss.ToString();
					standDisplay[i].nextOpp.text = "$ " + provRankList[i].team.earnings.ToString("n0");
					provRankList[i].team.rank = i + 1;
				}
				Debug.Log("cm.playerTeamIndex is " + cm.playerTeamIndex);

				int tempRank = 0;
				for (int i = 0; i < provRankList.Count; i++)
				{
					if (cm.playerTeamIndex == provRankList[i].team.id)
					{
						Debug.Log("cm.playerTeamIndex is " + cm.playerTeamIndex + " - i is " + i);
						tempRank = i;
						standDisplay[i].panel.enabled = true;
					}
					else
						standDisplay[i].panel.enabled = false;
				}
				scrollbar.value = (tempRank - provRankList.Count) / (1f - provRankList.Count);
				Debug.Log("Scrollbar.value is " + scrollbar.value + " - i is " + tempRank);

			}
			else
			{
				Debug.Log("cm.playerTeamIndex is " + cm.playerTeamIndex);
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
					standDisplay[i].name.text = (i + 1) + " - " + provRankList[i].team.name;
					standDisplay[i].wins.text = provRankList[i].team.wins.ToString();
					standDisplay[i].loss.text = provRankList[i].team.loss.ToString();
					standDisplay[i].nextOpp.text = "$ " + provRankList[i].team.earnings.ToString("n0");
					provRankList[i].team.rank = i + 1;
				}

				int tempRank = 0;

				for (int i = 0; i < provRankList.Count; i++)
				{
					if (cm.playerTeamIndex == provRankList[i].team.id)
					{
						tempRank = i;
						standDisplay[i].panel.enabled = true;
					}
					else
						standDisplay[i].panel.enabled = false;
				}
				scrollbar.value = (tempRank - provRankList.Count) / (1f - provRankList.Count);
				Debug.Log("Scrollbar.value is " + scrollbar.value + " - i is " + tempRank);
			}
			tour = true;
		}


		if (gameObject.activeSelf)
			StartCoroutine(RefreshPanel());
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
