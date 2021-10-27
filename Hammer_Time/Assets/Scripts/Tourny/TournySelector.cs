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

    public Tourny[] activeTournies;

    public Tourny emptyTourny;
    public Tourny currentTourny;

    public TournyPanel[] panels;

    public GameObject mainMenuGO;
    public GameObject profPanelGO;
    public ProfilePanel profPanel;

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
        if (cm.week > 0)
            weekText.text = "Week " + cm.week.ToString();
        else
            weekText.text = "Welcome to Season " + cm.season.ToString();
        
        if (Random.Range(0f, 1f) > 0.5f)
        {
            Shuffle(tournies);
            for (int i = 0; i < tournies.Length; i++)
            {
                if (tournies[i].complete != true)
                {
                    activeTournies[0] = tournies[i];
                    //provQual[i].complete = true;
                    break;
                }
            }
        }
        else
        {
            if (Random.Range(0f, 1f) > 0.5f)
            {
                Shuffle(provQual);
                for (int i = 0; i < provQual.Length; i++)
                {
                    if (provQual[i].complete != true)
                    {
                        activeTournies[0] = provQual[i];
                        //provQual[i].complete = true;
                        break;
                    }
                }
            }
            else
            {
                //Shuffle(tour);
                for (int i = 0; i < tour.Length; i++)
                {
                    if (tour[i].complete != true)
                    {
                        activeTournies[0] = tour[i];
                        //tour[i].complete = true;
                        break;
                    }
                }
            }
        }

        if (activeTournies[0].tour | activeTournies[0].qualifier)
        {
            Shuffle(tournies);
            for (int i = 0; i < tournies.Length; i++)
            {
                if (tournies[i].complete != true)
                {
                    activeTournies[1] = tournies[i];
                    //provQual[i].complete = true;
                    break;
                }
            }
        }
        else
        {
            Shuffle(tour);
            if (Random.Range(0f, 1f) > 0.5f)
            {
                for (int i = 0; i < tour.Length; i++)
                {
                    if (tour[i].complete != true)
                    {
                        activeTournies[1] = tour[i];
                        //tour[i].complete = true;
                        break;
                    }
                }
            }
            else
            {
                Shuffle(provQual);
                for (int i = 0; i < provQual.Length; i++)
                {
                    if (provQual[i].complete != true)
                    {
                        activeTournies[1] = provQual[i];
                        //provQual[i].complete = true;
                        break;
                    }
                }
            }
        }

        if (cm.tourQual)
            activeTournies[2] = tourChampionship;
        else if (cm.provQual)
            activeTournies[2] = provChampionship;
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
        if (on)
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(true);
            profPanel.earnings.text = "$ " + cm.earnings.ToString();
            profPanel.record.text = cm.record.x.ToString() + " - " + cm.record.y.ToString();
            profPanel.provRank.text = cm.playerTeam.rank.ToString();

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
}
