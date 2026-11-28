using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProvStandings : MonoBehaviour
{

	public StandingDisplay[] standDisplay;
	public Team[] teams;


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
		GameLog.Info("XX----------ProvStandings--XX");
		tour = false;
		if (FindObjectOfType<CareerManager>().provQual)
			tourButton.SetActive(true);
		else
			tourButton.SetActive(false);

        PrintRows();
    }

    public void SetUp()
    {
		tour = false;
		PrintRows();
    }

    /// <summary>
    /// Shows the money list, or the tour table if <see cref="tour"/> is set, then flips so the button shows the other.
    /// Built afresh from the career each time.
    /// </summary>
    public void PrintRows()
    {
		CareerManager cm = FindFirstObjectByType<CareerManager>();
		if (cm == null || cm.teams == null) return;

		if (row != null)
		{
			for (int i = 0; i < row.Length; i++)
				Destroy(row[i]);
		}

		bool showTour = tour;
		buttonText.text = showTour ? "<<Overall Standings" : "Tour Standings>>";
		headers[2].text = showTour ? "Points" : "Earnings";

		List<Team> table = showTour ? Standings.Tour(cm.tourTeams) : Standings.Money(cm.teams);
		teams = table.ToArray();
		row = new GameObject[table.Count];
		standDisplay = new StandingDisplay[table.Count];

		for (int i = 0; i < table.Count; i++)
		{
			Team team = table[i];
			row[i] = Instantiate(standTextRow, standTextParent);
			row[i].name = "Row " + (i + 1);
			row[i].GetComponent<RectTransform>().position = new Vector2(0f, i * -125f);

			standDisplay[i] = row[i].GetComponent<RowVariables>().standDisplay;
			standDisplay[i].name.text = (i + 1) + " - " + team.name;
			standDisplay[i].wins.text = team.seasonWins.ToString();
			standDisplay[i].loss.text = team.seasonLosses.ToString();
			standDisplay[i].nextOpp.text = showTour ? team.tourPoints.ToString() : "$ " + team.seasonEarnings.ToString("n0");
			standDisplay[i].panel.enabled = team.id == cm.playerTeamIndex;
		}

		int place = Standings.Place(table, cm.playerTeamIndex);
		if (table.Count > 1)
			scrollbar.value = Mathf.Clamp01(1f - (Mathf.Max(place, 1) - 1f) / (table.Count - 1));

		tour = !showTour;

		if (gameObject.activeInHierarchy)
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
