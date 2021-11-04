using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TigerForge;

public class TournySelector : MonoBehaviour
{
    CareerManager cm;

    public Text weekText;
    public Text teamNameText;

    public Tourny[] tournies;
    public Tourny[] tour;
    public Tourny[] provQual;
    public Tourny tourChampionship;
    public Tourny provChampionship;

    public Tourny[] panel1Tournies;
    public Tourny[] panel2Tournies;

    public Tourny[] activeTournies;

    public Tourny emptyTourny;
    public Tourny currentTourny;

    public TournyPanel[] panels;

    public GameObject mainMenuGO;
    public GameObject profPanelGO;
    public ProfilePanel profPanel;

    public ProvStandings provStandings;
    int week;

    EasyFileSave myFile;

    // Start is called before the first frame update
    void Start()
    {
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        //gsp.inProgress = false;
        cm = FindObjectOfType<CareerManager>();
        //if (cm.inProgress)
            //cm.LoadCareer();
        teamNameText.text = cm.playerName + " " + cm.teamName;
        if (cm.week == 0)
            NewSeason();
        else
        {
            cm.LoadCareer();

        }

        SetActiveTournies();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Shuffle(Tourny[] a)
    {
        // Loops through array
        for (int i = a.Length - 1; i > 0; i--)
        {
            // Randomize a number between 0 and i (so that the range decreases each time)
            int rnd = Random.Range(0, i);

            // Save the value of the current i, otherwise it'll overright when we swap the values
            Tourny temp = a[i];

            // Swap the new and old values
            a[i] = a[rnd];
            a[rnd] = temp;
        }
    }

    public void SetActiveTournies()
    {
        bool tourniesComplete = false;
        bool tourComplete = false;
        bool provQualComplete = false;

        if (cm.week > 0)
            weekText.text = "Week " + cm.week.ToString();
        else
            weekText.text = "Welcome to Season " + cm.season.ToString();

        if (cm.provQual)
        {
            if (Random.Range(0f, 1f) > 0.5f)
            {
                for (int i = 0; i < tournies.Length; i++)
                {
                    if (tournies[i].complete == true)
                    {
                        tourniesComplete = true;
                        Debug.Log(tournies[i].name + " complete");
                    }
                    else
                    {
                        tourniesComplete = false;
                        activeTournies[0] = tournies[i];
                        break;
                    }
                }

                if (tourniesComplete)
                {
                    for (int i = 0; i < tour.Length; i++)
                    {
                        if (tour[i].complete == true)
                        {
                            tourComplete = true;
                            Debug.Log(tour[i].name + " complete");
                        }
                        else
                        {
                            tourComplete = false;
                            activeTournies[0] = tour[i];
                            break;
                        }
                    }

                    if (tourComplete)
                    {
                        activeTournies[0] = emptyTourny;
                    }
                }
            }
            else
            {
                //Shuffle(tour);
                for (int i = 0; i < tour.Length; i++)
                {
                    if (tour[i].complete == true)
                    {
                        tourComplete = true;
                        Debug.Log(tour[i].name + " complete");
                    }
                    else
                    {
                        tourComplete = false;
                        activeTournies[0] = tour[i];
                        break;
                    }
                }

                if (tourComplete)
                {
                    for (int i = 0; i < tournies.Length; i++)
                    {
                        if (tournies[i].complete == true)
                        {
                            tourniesComplete = true;
                            Debug.Log(tournies[i].name + " complete");
                        }
                        else
                        {
                            tourniesComplete = false;
                            activeTournies[0] = tournies[i];
                            break;
                        }
                    }

                    if (tourniesComplete)
                    {
                        activeTournies[0] = emptyTourny;
                    }
                }
            }
        }
        else if (Random.Range(0f, 1f) > 0.5f)
        {
            //Shuffle(tournies);
            for (int i = 0; i < tournies.Length; i++)
            {
                if (tournies[i].complete == true)
                {
                    tourniesComplete = true;
                    Debug.Log(tournies[i].name + " complete");
                }
                else
                {
                    tourniesComplete = false;
                    activeTournies[0] = tournies[i];
                    break;
                }
            }

            if (tourniesComplete)
            {
                for (int i = 0; i < provQual.Length; i++)
                {
                    if (provQual[i].complete == true)
                    {
                        provQualComplete = true;
                        Debug.Log(provQual[i].name + " " + provQual[i].location + " complete");
                    }
                    else
                    {
                        provQualComplete = false;
                        activeTournies[0] = provQual[i];
                        break;
                    }
                }

                if (provQualComplete)
                {
                    for (int i = 0; i < tour.Length; i++)
                    {
                        if (tour[i].complete == true)
                        {
                            tourComplete = true;
                        }
                        else
                        {
                            tourComplete = false;
                            activeTournies[0] = tour[i];
                            break;
                        }
                    }

                    if (tourComplete)
                    {
                        activeTournies[0] = emptyTourny;
                    }
                }
            }
        }
        else
        {
            if (Random.Range(0f, 1f) > 0.5f)
            {
                //Shuffle(provQual);
                for (int i = 0; i < provQual.Length; i++)
                {
                    if (provQual[i].complete == true)
                    {
                        provQualComplete = true;
                        Debug.Log(provQual[i].name + " " + provQual[i].location + " complete");
                    }
                    else
                    {
                        provQualComplete = false;
                        activeTournies[0] = provQual[i];
                        break;
                    }
                }

                if (provQualComplete)
                {
                    for (int i = 0; i < tournies.Length; i++)
                    {
                        if (tournies[i].complete == true)
                        {
                            tourniesComplete = true;
                            Debug.Log(tournies[i].name + " complete");
                        }
                        else
                        {
                            tourniesComplete = false;
                            activeTournies[0] = tournies[i];
                            break;
                        }
                    }

                    if (tourniesComplete)
                    {
                        for (int i = 0; i < tour.Length; i++)
                        {
                            if (tour[i].complete == true)
                            {
                                tourComplete = true;
                                Debug.Log(tour[i].name + " complete");
                            }
                            else
                            {
                                tourComplete = false;
                                activeTournies[0] = tour[i];
                                break;
                            }
                        }

                        if (tourComplete)
                        {
                            activeTournies[0] = emptyTourny;
                        }
                    }
                }
            }
            else
            {
                //Shuffle(tour);
                for (int i = 0; i < tour.Length; i++)
                {
                    if (tour[i].complete == true)
                    {
                        tourComplete = true;
                        Debug.Log(tour[i].name + " complete");
                    }
                    else
                    {
                        tourComplete = false;
                        activeTournies[0] = tour[i];
                        break;
                    }
                }

                if (tourComplete)
                {
                    for (int i = 0; i < tournies.Length; i++)
                    {
                        if (tournies[i].complete == true)
                        {
                            tourniesComplete = true;
                            Debug.Log(tournies[i].name + " complete");
                        }
                        else
                        {
                            tourniesComplete = false;
                            activeTournies[0] = tournies[i];
                            break;
                        }
                    }

                    if (tourniesComplete)
                    {
                        for (int i = 0; i < provQual.Length; i++)
                        {
                            if (provQual[i].complete == true)
                            {
                                provQualComplete = true;
                                Debug.Log(provQual[i].name + " " + provQual[i].location + " complete");
                            }
                            else
                            {
                                provQualComplete = false;
                                activeTournies[0] = provQual[i];
                                break;
                            }
                        }

                        if (provQualComplete)
                        {
                            activeTournies[0] = emptyTourny;
                        }
                    }
                }
            }
        }


        if (cm.provQual)
        {
            if (activeTournies[0].tour)
            {
                for (int i = 0; i < tournies.Length; i++)
                {
                    if (tournies[i].complete == true)
                    {
                        tourniesComplete = true;
                        Debug.Log(tournies[i].name + " complete");
                    }
                    else
                    {
                        tourniesComplete = false;
                        activeTournies[1] = tournies[i];
                        break;
                    }
                }

                if (tourniesComplete)
                {
                    activeTournies[1] = emptyTourny;
                }
            }
            else
            {
                //Shuffle(tour);
                for (int i = 0; i < tour.Length; i++)
                {
                    if (tour[i].complete == true)
                    {
                        tourComplete = true;
                        Debug.Log(tour[i].name + " complete");
                    }
                    else
                    {
                        tourComplete = false;
                        activeTournies[1] = tour[i];
                        break;
                    }
                }

                if (tourComplete)
                {
                    for (int i = 0; i < tournies.Length; i++)
                    {
                        if (tournies[i].complete == true)
                        {
                            tourniesComplete = true;
                            Debug.Log(tournies[i].name + " complete");
                        }
                        else
                        {
                            tourniesComplete = false;
                            activeTournies[1] = tournies[i];
                            break;
                        }
                    }

                    if (tourniesComplete)
                    {
                        activeTournies[1] = emptyTourny;
                    }
                }
            }
        }
        else if (activeTournies[0].tour | activeTournies[0].qualifier)
        {
            //Shuffle(tournies);
            for (int i = 0; i < tournies.Length; i++)
            {
                if (tournies[i].complete == true)
                {
                    tourniesComplete = true;
                    Debug.Log(tournies[i].name + " complete");
                }
                else
                {
                    tourniesComplete = false;
                    activeTournies[1] = tournies[i];
                    //provQual[i].complete = true;
                    break;
                }
            }

            if (tourniesComplete)
            {
                for (int i = 0; i < tour.Length; i++)
                {
                    if (tour[i].complete == true)
                    {
                        tourComplete = true;
                        Debug.Log(tour[i].name + " complete");
                    }
                    else
                    {
                        tourComplete = false;
                        activeTournies[1] = tour[i];
                        break;
                    }
                }

                if (tourComplete)
                {
                    for (int i = 0; i < provQual.Length; i++)
                    {
                        if (provQual[i].complete == true)
                        {
                            provQualComplete = true;
                            Debug.Log(provQual[i].name + " " + provQual[i].location + " complete");
                        }
                        else
                        {
                            provQualComplete = false;
                            activeTournies[1] = provQual[i];
                            break;
                        }
                    }

                    if (provQualComplete)
                    {
                        activeTournies[1] = emptyTourny;
                    }
                }
            }
        }
        else
        {
            //Shuffle(tour);
            if (Random.Range(0f, 1f) > 0.5f)
            {
                for (int i = 0; i < tour.Length; i++)
                {
                    if (tour[i].complete == true)
                    {
                        tourComplete = true;
                        Debug.Log(tour[i].name + " complete");
                    }
                    else
                    {
                        tourComplete = false;
                        activeTournies[1] = tour[i];
                        //tour[i].complete = true;
                        break;
                    }
                }

                if (tourComplete)
                {
                    for (int i = 0; i < tournies.Length; i++)
                    {
                        if (tournies[i].complete == true)
                        {
                            tourniesComplete = true;
                            Debug.Log(tournies[i].name + " complete");
                        }
                        else
                        {
                            tourniesComplete = false;
                            activeTournies[1] = tournies[i];
                            break;
                        }
                    }

                    if (tourniesComplete)
                    {
                        for (int i = 0; i < provQual.Length; i++)
                        {
                            if (provQual[i].complete == true)
                            {
                                provQualComplete = true;
                                Debug.Log(provQual[i].name + " " + provQual[i].location + " complete");
                            }
                            else
                            {
                                provQualComplete = false;
                                activeTournies[1] = provQual[i];
                                break;
                            }
                        }

                        if (provQualComplete)
                        {
                            activeTournies[1] = emptyTourny;
                        }
                    }
                }
            }
            else
            {
                //Shuffle(provQual);
                for (int i = 0; i < provQual.Length; i++)
                {
                    if (provQual[i].complete == true)
                    {
                        provQualComplete = true;
                        Debug.Log(provQual[i].name + " " + provQual[i].location + " complete");
                    }
                    else
                    {
                        provQualComplete = false;
                        activeTournies[1] = provQual[i];
                        //provQual[i].complete = true;
                        break;
                    }
                }

                if (provQualComplete)
                {
                    for (int i = 0; i < tournies.Length; i++)
                    {
                        if (tournies[i].complete == true)
                        {
                            tourniesComplete = true;
                            Debug.Log(tournies[i].name + " complete");
                        }
                        else
                        {
                            tourniesComplete = false;
                            activeTournies[1] = tournies[i];
                            break;
                        }
                    }

                    if (tourniesComplete)
                    {
                        for (int i = 0; i < tour.Length; i++)
                        {
                            if (tour[i].complete == true)
                            {
                                tourComplete = true;
                                Debug.Log(tour[i].name + " complete");
                            }
                            else
                            {
                                tourComplete = false;
                                activeTournies[1] = tour[i];
                                //tour[i].complete = true;
                                break;
                            }
                        }

                        if (tourComplete)
                        {
                            activeTournies[1] = emptyTourny;
                        }
                    }
                }
            }
        }
        if (activeTournies[0].name == activeTournies[1].name && activeTournies[0].location == activeTournies[1].location)
        {
            activeTournies[0] = emptyTourny;
        }
        if (cm.provQual && provQualComplete && !provChampionship.complete)
        {
            activeTournies[2] = provChampionship;
        }
        else if (cm.tourQual && tourComplete)
        {
            activeTournies[2] = tourChampionship;
        }
        else
            activeTournies[2] = emptyTourny;


        SetPanels();

    }

    public void SetPanels()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].location.text = activeTournies[i].location;
            panels[i].buttonText.text = activeTournies[i].name;
            panels[i].teams.text = activeTournies[i].teams.ToString();
            panels[i].format.text = activeTournies[i].format;
            panels[i].purse.text = "$" + activeTournies[i].prizeMoney.ToString();
            panels[i].entry.text = "$" + activeTournies[i].entryFee.ToString();

        }
    }
    public void NewSeason()
    {
        cm.NewSeason();
    }

    public void NextWeek()
    {
        cm.NextWeek();
    }

    public void PlayTourny()
    {
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        gsp.LoadFromTournySelector();
        cm.PlayTourny();
        if (currentTourny.tour)
        {
            for (int i = 0; i < tour.Length; i++)
            {
                if (currentTourny.id == tour[i].id)
                    tour[i].complete = true;
            }
        }
        else if (currentTourny.qualifier)
        {
            for (int i = 0; i < provQual.Length; i++)
            {
                if (currentTourny.id == provQual[i].id)
                    provQual[i].complete = true;
            }
        }
        else if (currentTourny.championship)
        {
            if (currentTourny.name == tourChampionship.name)
                tourChampionship.complete = true;
            else if (currentTourny.name == provChampionship.name)
                provChampionship.complete = true;
        }
        else
        {
            for (int i = 0; i < tournies.Length; i++)
            {
                if (currentTourny.id == tournies[i].id)
                    tournies[i].complete = true;
            }
        }
        cm.tournies = tournies;
        cm.tour = tour;
        cm.prov = provQual;
        cm.champ = new Tourny[2];
        cm.champ[0] = tourChampionship;
        cm.champ[1] = provChampionship;
        cm.activeTournies = activeTournies;
        cm.SaveCareer();
        SceneManager.LoadScene("Tourny_Menu_1");
    }

    public void SelectTourny(int button)
    {
        currentTourny = activeTournies[button];
        Debug.Log("Button is " + button);
        cm.SetupTourny();
    }

    public void Profile(bool on)
    {
        int tempRank;

        for (int i = 0; i < cm.teams.Length; i++)
        {
            if (cm.playerTeamIndex == cm.teams[i].id)
            {
                if (cm.teams[i].rank == 1)
                    profPanel.provRank.text = cm.teams[i].rank.ToString() + "st";
                else if (cm.teams[i].rank == 2)
                    profPanel.provRank.text = cm.teams[i].rank.ToString() + "nd";
                else if (cm.teams[i].rank == 3)
                    profPanel.provRank.text = cm.teams[i].rank.ToString() + "rd";
                else if (cm.teams[i].rank > 3)
                    profPanel.provRank.text = cm.teams[i].rank.ToString() + "th";

            }
        }
        if (on)
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(true);
            profPanel.earnings.text = "$ " + cm.earnings.ToString();
            profPanel.record.text = cm.record.x.ToString() + " - " + cm.record.y.ToString();

            if (cm.provQual)
                profPanel.provQual.text = "Yes";
            else
                profPanel.provQual.text = "No";

            //profPanel.tourRank.text = cm.tourRankList.ToString();
            //profPanel.tourPoints.text = cm.tourPoints.ToString();
        }
        else
        {
            mainMenuGO.SetActive(true);
            profPanelGO.SetActive(false);
        }
    }

    public void Standings(bool on)
    {
        if (on)
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(true);
            provStandings.PrintRows();
        }
        else
        {
            mainMenuGO.SetActive(true);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
        }
    }
}
