using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TigerForge;

public class TournySelector : MonoBehaviour
{
    CareerManager cm;
    public GameObject dialogueGO;
    public DialogueTrigger coachGreen;

    public Text weekText;
    public Text teamNameText;
    public Text teamNameAlertText;
    public Text earningsText;
    public Text recordText;

    public Scrollbar hScroll;
    bool drag;

    public XPManager xpm;
    public Text XPText;

    public GameObject playButton;
    public Tourny[] tournies;
    public Tourny[] tour;
    public Tourny[] provQual;
    public Tourny tourChampionship;
    public Tourny provChampionship;

    public Tourny[] panel1Tournies;
    public Tourny[] panel2Tournies;

    public Tourny[] activeTournies;
    public GameObject[] panelGOs;

    public Tourny emptyTourny;
    public Tourny currentTourny;

    public Color[] green;
    public Color[] dimmed;
    public Color[] yellow;
    public Color[] purple;

    public TournyPanel[] panels;

    public GameObject mainMenuGO;
    public GameObject profPanelGO;
    public ProfilePanel profPanel;

    public ProvStandings provStandings;
    public TourStandings tourStandings;
    int week;

    public bool provQualDialogue;
    bool tourQualDialogue;
    EasyFileSave myFile;

    // Start is called before the first frame update
    void Start()
    {
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        //gsp.inProgress = false;
        cm = FindObjectOfType<CareerManager>();
        xpm.SetSkillPoints();
        //if (cm.inProgress)
            //cm.LoadCareer();
        teamNameText.text = cm.playerName + " " + cm.teamName;
        //recordText.text = cm.record.x.ToString() + "-" + cm.record.y.ToString();
        //earningsText.text = "$" + cm.earnings.ToString();
        XPText.text = cm.xp.ToString() + "/" + cm.totalXp.ToString();

        provStandings.PrintRows();
        tourStandings.PrintRows();

        if (cm.week == 0)
            NewSeason();
        else
        {
            cm.LoadCareer();

            if (cm.week == 2)
            {
                Debug.Log("Player Rank is " + cm.playerTeam.rank);
                dialogueGO.SetActive(true);
                if (cm.playerTeam.rank < 5)
                {
                    coachGreen.TriggerDialogue(7);
                }
                else
                {
                    coachGreen.TriggerDialogue(8);
                }
                cm.coachDialogue[7] = true;
                cm.coachDialogue[8] = true;
            }
            
            if (cm.week == 3)
            {
                if (cm.xp < 25)
                    cm.xp = 25;
                dialogueGO.SetActive(true);
                coachGreen.TriggerDialogue(9);
                cm.coachDialogue[9] = true;
            }

            if (cm.week == 4)
            {
                dialogueGO.SetActive(true);
                coachGreen.TriggerDialogue(10);
                cm.coachDialogue[10] = true;
            }
            if (cm.week == 5)
            {
                dialogueGO.SetActive(true);
                coachGreen.TriggerDialogue(11);
                cm.coachDialogue[11] = true;
            }
            if (cm.provQual)
            {
                if (!cm.coachDialogue[3] | !cm.coachDialogue[4])
                {
                    dialogueGO.SetActive(true);
                    if (cm.week < 10)
                    {
                        coachGreen.TriggerDialogue(4);
                    }
                    else
                    {
                        coachGreen.TriggerDialogue(3);
                    }
                    cm.coachDialogue[3] = true;
                    cm.coachDialogue[4] = true;
                }
            }

        }
        Debug.Log("Skill Points are " + xpm.skillPoints);
       
        SetActiveTournies();
    }

    // Update is called once per frame
    public void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            drag = true;
            if (Input.GetMouseButtonUp(0))
                drag = false;
        }

        if (Input.GetMouseButtonUp(0) && drag)
            drag = false;

        //Debug.Log("drag = " + drag);
        if (!drag)
        {
            if (hScroll.value < 0.25f)
            {
                hScroll.value = Mathf.Lerp(hScroll.value, 0, 2 * Time.deltaTime);
            }
            if (hScroll.value >= 0.25f && hScroll.value < 0.75f)
            {
                hScroll.value = Mathf.Lerp(hScroll.value, 0.5f, 2 * Time.deltaTime);
            }
            if (hScroll.value >= 0.75f)
            {
                hScroll.value = Mathf.Lerp(hScroll.value, 1, 2 * Time.deltaTime);
            }
        }

        if (cm.week > 1 & xpm.skillPoints > 0)
        {
            teamNameText.gameObject.SetActive(false);
            teamNameAlertText.text = teamNameText.text;
            teamNameAlertText.gameObject.SetActive(true);
        }
        else
        {
            teamNameText.gameObject.SetActive(true);
            teamNameAlertText.gameObject.SetActive(false);
        }
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
        {
            weekText.text = "Week " + cm.week.ToString();

            for (int i = 0; i < panelGOs.Length; i++)
            {
                panelGOs[i].GetComponent<Image>().color = Color.white;
                panelGOs[i].transform.GetChild(1).GetComponent<Image>().color = dimmed[1];
            }
        }
        else
            weekText.text = "Welcome to Season " + cm.season.ToString();

        #region Panel 1
        if (cm.week == 1 | cm.week == 3 | cm.week == 5)
            activeTournies[0] = emptyTourny;
        else if (cm.week == 2)
        {
            activeTournies[0] = tournies[1];
            hScroll.value = 0f;
        }
        else if (cm.week == 4)
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
                    hScroll.value = 0f;
                    break;
                }
            }
        }
        else
        {
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
                            panelGOs[0].GetComponent<Image>().color = dimmed[0];
                            panelGOs[0].transform.GetChild(1).GetComponent<Image>().color = dimmed[0];
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
                            panelGOs[0].GetComponent<Image>().color = dimmed[0];
                            panelGOs[0].transform.GetChild(1).GetComponent<Image>().color = dimmed[0];
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
                            panelGOs[0].GetComponent<Image>().color = dimmed[0];
                            panelGOs[0].transform.GetChild(1).GetComponent<Image>().color = dimmed[0];
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
                                panelGOs[0].GetComponent<Image>().color = dimmed[0];
                                panelGOs[0].transform.GetChild(1).GetComponent<Image>().color = dimmed[0];
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
                                panelGOs[0].GetComponent<Image>().color = dimmed[0];
                                panelGOs[0].transform.GetChild(1).GetComponent<Image>().color = dimmed[0];
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Panel 2
        if (cm.week == 1)
            activeTournies[1] = tournies[0];
        else if (cm.week == 2)
            activeTournies[1] = emptyTourny;
        else if (cm.week == 3)
            activeTournies[1] = tournies[2];
        else if (cm.week == 4)
        {
            if (cm.provQual)
            {
                for (int i = 0; i < tournies.Length; i++)
                {
                    if (tournies[i].complete == true)
                    {
                        tourniesComplete = true;
                        Debug.Log(tournies[i].name + " complete");
                    }
                    else if (activeTournies[0] == tournies[i])
                    {
                        Debug.Log(tournies[i].name + " skipped");
                    }
                    else
                    {
                        tourniesComplete = false;
                        activeTournies[1] = tournies[i];
                        break;
                    }
                }
            }
            else
            {
                if (Random.Range(0f, 1f) > 0.5f)
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
                }
                else
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
                }
            }
        }
        else if (cm.week == 5)
            activeTournies[1] = tour[0];
        else if (cm.provQual)
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
                    panelGOs[1].GetComponent<Image>().color = dimmed[0];
                    panelGOs[1].transform.GetChild(1).GetComponent<Image>().color = dimmed[0];
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
                        panelGOs[1].GetComponent<Image>().color = dimmed[0];
                        panelGOs[1].transform.GetChild(1).GetComponent<Image>().color = dimmed[0];
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
                        panelGOs[1].GetComponent<Image>().color = dimmed[0];
                        panelGOs[1].transform.GetChild(1).GetComponent<Image>().color = dimmed[0];
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
                            panelGOs[1].GetComponent<Image>().color = dimmed[0];
                            panelGOs[1].transform.GetChild(1).GetComponent<Image>().color = dimmed[0];
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
                            panelGOs[1].GetComponent<Image>().color = dimmed[0];
                            panelGOs[1].transform.GetChild(1).GetComponent<Image>().color = dimmed[0];
                        }
                    }
                }
            }
        }
        if (activeTournies[0].name == activeTournies[1].name && activeTournies[0].location == activeTournies[1].location)
        {
            activeTournies[0] = emptyTourny;
            panelGOs[0].GetComponent<Image>().color = dimmed[0];
            panelGOs[0].transform.GetChild(1).GetComponent<Image>().color = dimmed[0];
        }
        #endregion


        if (cm.provQual)
        {
            provQualComplete = true;
        }
        else
        {
            for (int i = 0; i < provQual.Length; i++)
            {
                if (provQual[i].complete)
                {
                    provQualComplete = true;
                }
                else
                {
                    provQualComplete = false;
                }
            }
        }

        for (int i = 0; i < tour.Length; i++)
        {
            if (tour[i].complete)
                tourComplete = true;
            else
                tourComplete = false;
        }

        Debug.Log("Tour Complete is " + tourComplete);

        if (tourComplete)
        {
            for (int i = 0; i < cm.tourRankList.Count; i++)
            {
                if (cm.playerTeamIndex == cm.tourRankList[i].team.id)
                {
                    Debug.Log("Tour rank is" + (i + 1));
                    if (i < 6)
                    {
                        cm.tourQual = true;
                        if (!cm.coachDialogue[5])
                        {
                            dialogueGO.SetActive(true);
                            coachGreen.TriggerDialogue(5);
                            cm.coachDialogue[5] = true;
                        }
                    }
                    else
                    {
                        Debug.Log("Outside the top 6"); 
                        if (!cm.coachDialogue[2])
                        {
                            dialogueGO.SetActive(true);
                            coachGreen.TriggerDialogue(2);
                            cm.coachDialogue[2] = true;
                        }
                    }
                }
            }
        }
        Debug.Log("Prov Qual Complete is " + provQualComplete);



        if (cm.tourQual && tourComplete && !tourChampionship.complete)
        {
            activeTournies[2] = tourChampionship;
        }
        else if (cm.provQual && provQualComplete && !provChampionship.complete)
        {
            if (week < 10)
            {
                panelGOs[2].GetComponent<Image>().color = dimmed[0];
                panelGOs[2].transform.GetChild(1).GetComponent<Image>().color = dimmed[0];
                activeTournies[2] = emptyTourny;
            }
            else
            {
                panelGOs[2].GetComponent<Image>().color = Color.white;
                activeTournies[2] = provChampionship;
            }

        }
        else if (provChampionship.complete)
        {
            dialogueGO.SetActive(true);
            coachGreen.TriggerDialogue(6);
            cm.coachDialogue[6] = true;
        }
        else
        {

            activeTournies[2] = emptyTourny;
            panelGOs[2].GetComponent<Image>().color = dimmed[0];
            panelGOs[1].transform.GetChild(1).GetComponent<Image>().color = dimmed[0];
        }


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
            playButton.SetActive(false);
        }
    }
    public void NewSeason()
    {
        dialogueGO.SetActive(true);
        coachGreen.TriggerDialogue(0);
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
        playButton.SetActive(true);
    }

    public void Profile(bool on)
    {
        cm = FindObjectOfType<CareerManager>();

        for (int i = 0; i < cm.teams.Length; i++)
        {
            if (cm.playerTeamIndex == cm.provRankList[i].team.id)
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


            cm.provRankList.Sort();
            for (int i = 0; i < cm.provRankList.Count; i++)
            {
                if (cm.playerTeamIndex == cm.provRankList[i].team.id)
                {
                    if (i == 0)
                        profPanel.provRank.text = (i + 1).ToString() + "st";
                    else if (i == 1)
                        profPanel.provRank.text = (i + 1).ToString() + "nd";
                    else if (i == 2)
                        profPanel.provRank.text = (i + 1).ToString() + "rd";
                    else if (i > 2)
                        profPanel.provRank.text = (i + 1).ToString() + "th";

                }
            }
            cm.tourRankList.Sort();

            for (int i = 0; i < cm.tourRankList.Count; i++)
            {
                if (cm.playerTeamIndex == cm.tourRankList[i].team.id)
                {
                    if (i == 0)
                        profPanel.tourRank.text = (i + 1).ToString() + "st";
                    else if (i == 1)
                        profPanel.tourRank.text = (i + 1).ToString() + "nd";
                    else if (i == 2)
                        profPanel.tourRank.text = (i + 1).ToString() + "rd";
                    else if (i > 2)
                        profPanel.tourRank.text = (i + 1).ToString() + "th";

                    profPanel.tourPoints.text = cm.tourRankList[i].team.tourPoints.ToString() + " points";
                }
            }
        }
        else
        {
            mainMenuGO.SetActive(true);
            profPanelGO.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            provStandings.gameObject.SetActive(false);
        }
    }

    public void Standings(bool on)
    {
        if (on)
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(true);
            tourStandings.gameObject.SetActive(false);
        }
        else
        {
            mainMenuGO.SetActive(true);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
        }
    }

    public void TourStandings(bool on)
    {
        if (on)
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(true);
        }
        else
        {
            mainMenuGO.SetActive(true);
            profPanelGO.SetActive(false);
            tourStandings.gameObject.SetActive(false);
        }
    }

    public void XPWindow(bool on)
    {
        if (on)
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            xpm.gameObject.SetActive(true);
        }
        else
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(true);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            xpm.gameObject.SetActive(false);
        }
    }
}
