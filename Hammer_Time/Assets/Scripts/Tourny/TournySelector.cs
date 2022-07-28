using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TigerForge;

public class TournySelector : MonoBehaviour
{
    CareerManager cm;
    public XPManager xpm;
    public GameObject dialogueGO;
    public DialogueTrigger coachGreen;

    public Sprite buttonOutline_pressed;
    public Sprite buttonFilled_pressed;
    public Sprite buttonOutline;
    public Sprite buttonFilled;

    public Text weekText;
    public Text teamNameText;
    public Text teamNameAlertText;
    public Text earningsText;
    public Text recordText;

    public Scrollbar hScroll;
    bool drag;

    public Button[] menuButtons;

    public Text XPText;

    public GameObject playButton;
    public Tourny[] locals;
    public Tourny[] tournies;
    public Tourny[] tourQual;
    public Tourny[] tour;
    public Tourny[] provQual;
    public Tourny[] majors;
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

    public GameObject quitButton;
    public GameObject mainMenuGO;
    public GameObject profPanelGO;
    public ProfilePanel profPanel;
    public GameObject teamPanel;
    public TeamMenu teamMenu;
    public GameObject powerUpPanel;

    public ProvStandings provStandings;
    public TourStandings tourStandings;
    int week;

    public int menuBut_Select;
    public bool provQualDialogue;
    bool tourQualDialogue;
    EasyFileSave myFile;
    public bool teamPaid;


    private void Start()
    {
        //GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        //gsp.inProgress = false;
        cm = FindObjectOfType<CareerManager>();

        
        //if (cm.inProgress)
        //cm.LoadCareer();
        teamNameText.text = cm.playerName + " " + cm.teamName;
        //recordText.text = cm.record.x.ToString() + "-" + cm.record.y.ToString();
        //earningsText.text = "$" + cm.earnings.ToString();
        //XPText.text = cm.xp.ToString() + "/" + cm.totalXp.ToString();

        //Profile(true);
        SetUp();
        teamPaid = false;
    }

    public void SetUp()
    {
        cm = FindObjectOfType<CareerManager>();

        PowerUpManager pm = FindObjectOfType<PowerUpManager>();
        xpm.SetSkillPoints();

        Debug.Log("TSel cm.week is " + cm.week);

        if (cm.week == 0)
        {
            StartCoroutine(NewSeason());
        }
        else
        {
            cm.LoadCareer();
        }

        teamMenu.TeamMenuOpen();
        pm.SetUp();

        provStandings.PrintRows();
        tourStandings.PrintRows();
        Debug.Log("Skill Points are " + xpm.skillPoints);
        SetActiveTournies();
    }

    IEnumerator WaitForDialogue()
    {
        yield return new WaitUntil(() => dialogueGO.activeSelf == false);
        teamMenu.TeamMenuOpen();
        Expand(menuButtons[2]);
        if (cm.provQual)
        {
            if (!cm.qualDialogue[2] | !cm.qualDialogue[3])
            {
                dialogueGO.SetActive(true);
                if (cm.week < 10)
                {
                    coachGreen.TriggerDialogue("Qualifiers", 3);
                }
                else
                {
                    coachGreen.TriggerDialogue("Qualifiers", 2);
                }
                cm.qualDialogue[3] = true;
                cm.qualDialogue[2] = true;
            }
        }
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

        //if (cm.week > 3 & xpm.skillPoints > 0)
        //{
        //    teamNameText.gameObject.SetActive(false);
        //    teamNameAlertText.text = teamNameText.text;
        //    teamNameAlertText.gameObject.SetActive(true);
        //}
        //else
        //{
        //    teamNameText.gameObject.SetActive(true);
        //    teamNameAlertText.gameObject.SetActive(false);
        //}
    }

    void Shuffle(Tourny[] a = null, int[] b = null)
    {
        if (a != null)
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


        if (b != null)
        {
            // Loops through array
            for (int i = b.Length - 1; i > 0; i--)
            {
                // Randomize a number between 0 and i (so that the range decreases each time)
                int rnd = Random.Range(0, i);

                // Save the value of the current i, otherwise it'll overright when we swap the values
                int temp = b[i];

                // Swap the new and old values
                b[i] = b[rnd];
                b[rnd] = temp;
            }
        }
    }

    public void SetActiveTournies()
    {
        weekText.text = "Week " + cm.week.ToString();

        bool tourniesComplete = false;
        bool tourComplete = false;
        bool provQualComplete = false;
        int nextTourny = 0;
        int nextTour = 0;
        int nextProvQual = 0;
        int nextTourny2 = 0;

        int[] rnd = new int[3] { 0, 1, 2 };
        Shuffle(null, rnd);

        int tCount = 0;
        for (int i = 0; i < tournies.Length; i++)
        {
            if (tournies[i].complete)
            {
                tourniesComplete = true;
            }
            else
            {
                if (tCount == 1)
                {
                    nextTourny2 = i;
                    Debug.Log("TSel nextTourny2 is " + nextTourny2);
                    tourniesComplete = false;
                    break;
                }
                else
                {
                    nextTourny = i;
                    Debug.Log("TSel nextTourny is " + nextTourny);
                    tourniesComplete = false;
                    tCount = 1;
                }
            }
        }

        for (int i = 0; i < tour.Length; i++)
        {
            if (tour[i].complete)
            {
                tourComplete = true;
            }
            else
            {
                nextTour = i;
                Debug.Log("TSel nextTour is " + nextTour);
                tourComplete = false;
                break;
            }
        }

        for (int i = 0; i < provQual.Length; i++)
        {
            if (provQual[i].complete)
            {
                provQualComplete = true;
            }
            else
            {
                nextProvQual = i;
                Debug.Log("TSel nextProvQual is " + nextProvQual);
                provQualComplete = false;
                break;
            }
        }

        if (cm.provQual)
            provQualComplete = true;

        if (cm.week == 1)
        {
            tournies[0].complete = true;
            activeTournies[0] = emptyTourny;
            activeTournies[1] = tournies[0];
            activeTournies[2] = emptyTourny;
        }
        else
        {
            if (tourChampionship.complete & provChampionship.complete)
            {
                EndOfSeason();
            }
            else if (tourniesComplete & tourComplete & provQualComplete)
            {
                if (cm.tourQual & !tourChampionship.complete)
                    activeTournies[rnd[0]] = tourChampionship;
                else
                    activeTournies[rnd[0]] = emptyTourny;

                activeTournies[rnd[1]] = emptyTourny;

                if (cm.provQual & !provChampionship.complete)
                    activeTournies[rnd[2]] = provChampionship;
                else
                    activeTournies[rnd[2]] = emptyTourny;
            }
            else if (!tourniesComplete & !tourComplete & provQualComplete)
            {
                tournies[nextTourny].complete = true;
                tournies[nextTourny2].complete = true;
                tour[nextTour].complete = true;
                activeTournies[rnd[0]] = tour[nextTour];
                activeTournies[rnd[1]] = tournies[nextTourny];
                if (nextTourny2 == 0)
                    activeTournies[rnd[2]] = emptyTourny;
                else
                    activeTournies[rnd[2]] = tournies[nextTourny2];
            }
            else if (tourniesComplete & !tourComplete & !provQualComplete)
            {
                provQual[nextProvQual].complete = true;
                tour[nextTour].complete = true;
                activeTournies[rnd[0]] = provQual[nextProvQual];
                activeTournies[rnd[1]] = emptyTourny;
                activeTournies[rnd[2]] = tour[nextTour];
            }
            else if (!tourniesComplete & tourComplete & !provQualComplete)
            {
                provQual[nextProvQual].complete = true;
                tournies[nextTourny].complete = true;
                tournies[nextTourny2].complete = true;
                activeTournies[rnd[0]] = provQual[nextProvQual];
                activeTournies[rnd[1]] = tournies[nextTourny];
                activeTournies[rnd[2]] = tournies[nextTourny2];
            }
            else
            {
                if (cm.week > 3)
                {
                    provQual[nextProvQual].complete = true;
                    tournies[nextTourny].complete = true;
                    tour[nextTour].complete = true;
                    activeTournies[rnd[0]] = provQual[nextProvQual];
                    activeTournies[rnd[1]] = tournies[nextTourny];
                    activeTournies[rnd[2]] = tour[nextTour];
                }
                else
                {
                    provQual[nextProvQual].complete = true;
                    tournies[nextTourny].complete = true;
                    tournies[nextTourny2].complete = true;
                    activeTournies[rnd[0]] = provQual[nextProvQual];
                    activeTournies[rnd[1]] = tournies[nextTourny];
                    activeTournies[rnd[2]] = tournies[nextTourny2];
                }
            }
        }

        SetPanels();

    }

    public void SSetActiveTournies()
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
        if (cm.earnings < 0)
        {
            activeTournies[0] = emptyTourny;
        }
        else if (cm.week == 1 | cm.week == 3 | cm.week == 5)
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

        if (cm.earnings < 0)
            activeTournies[1] = emptyTourny;
        else if (cm.week == 1)
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


        if (cm.earnings < 0)
            activeTournies[0] = emptyTourny;
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
                }
            }
        }
        Debug.Log("Prov Qual Complete is " + provQualComplete);



        if (cm.earnings < 0)
            activeTournies[2] = emptyTourny;
        else if (cm.tourQual && tourComplete && !tourChampionship.complete)
        {
            activeTournies[2] = tourChampionship;
        }
        else if (cm.provQual && provQualComplete && !provChampionship.complete)
        {
            if (cm.week < 10)
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
            //dialogueGO.SetActive(true);
            //coachGreen.TriggerDialogue("Review", 0);
            //cm.reviewDialogue[0] = true;
            //quitButton.SetActive(true);
            cm.EndCareer();
        }
        else
        {
            activeTournies[2] = emptyTourny;
            panelGOs[2].GetComponent<Image>().color = dimmed[0];
            panelGOs[2].transform.GetChild(1).GetComponent<Image>().color = dimmed[0];
        }

        SetPanels();

    }

    public void SetPanels()
    {
        //for (int i = 0; i < activeTournies.Length; i++)
        //{
        //    if (activeTournies[i] == emptyTourny)
        //        panelGOs[i].SetActive(false);
        //    else
        //        panelGOs[i].SetActive(true);
        //}

        if (activeTournies[2] == emptyTourny)
        {
            if (activeTournies[0] == emptyTourny)
                hScroll.value = 0.5f;
            else
                hScroll.value = 0f;
        }
        else
            hScroll.value = 1f;
            
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].location.text = activeTournies[i].location;
            panels[i].buttonText.text = activeTournies[i].name;
            panels[i].teams.text = activeTournies[i].teams.ToString();
            panels[i].format.text = activeTournies[i].format;
            panels[i].purse.text = "$" + activeTournies[i].prizeMoney.ToString("n0");
            panels[i].entry.text = "$" + activeTournies[i].entryFee.ToString("n0");
            playButton.SetActive(false);
        }


    }

    IEnumerator NewSeason()
    {
        //dialogueGO.SetActive(true);
        //coachGreen.TriggerDialogue("Intro", 0);
        //cm.introDialogue[7] = true;
        cm.NewSeason();
        //yield return new WaitUntil(() => !dialogueGO.activeSelf);
        yield return new WaitForSeconds(0.75f);
        //teamMenu.TeamMenuOpen();
        //Expand(menuButtons[2]);
    }

    public void EndOfGame()
    {
        cm = FindObjectOfType<CareerManager>();
        dialogueGO.SetActive(true);
        coachGreen.TriggerDialogue("Story", 0);

        cm.EndCareer();
    }

    public void EndOfSeason()
    {
        cm.EndCareer();
        SceneManager.LoadScene("SplashMenu");
    }

    public void NextWeek()
    {
        cm.NextWeek();
    }

    public void PlayTourny()
    {
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        TeamMenu tm = FindObjectOfType<TeamMenu>();
        XPManager xpm = FindObjectOfType<XPManager>();

        gsp.LoadFromTournySelector();
        //cm.PlayTourny();
        //for (int i = 0; i < tournies.Length; i++)
        //{
        //    for (int j = 0; j < activeTournies.Length; j++)
        //    {
        //        if (activeTournies[j].id == tournies[i].id)
        //            tournies[i].complete = true;
        //    }
        //}
        //for (int i = 0; i < tour.Length; i++)
        //{
        //    for (int j = 0; j < activeTournies.Length; j++)
        //    {
        //        if (activeTournies[j].id == tour[i].id)
        //            tour[i].complete = true;
        //    }
        //}

        //for (int i = 0; i < provQual.Length; i++)
        //{
        //    for (int j = 0; j < activeTournies.Length; j++)
        //    {
        //        if (activeTournies[j].id == provQual[i].id)
        //            provQual[i].complete = true;
        //    }
        //}

        if (currentTourny.championship)
        {
            if (currentTourny.name == tourChampionship.name)
                tourChampionship.complete = true;
            if (currentTourny.name == provChampionship.name)
                provChampionship.complete = true;
        }

        Debug.Log("TSel Current Tourny Name is " + currentTourny.name);

        //else
        //{
        //    for (int i = 0; i < tournies.Length; i++)
        //    {
        //        if (currentTourny.id == tournies[i].id)
        //            tournies[i].complete = true;
        //    }
        //}

        cm.tournies = tournies;
        cm.tour = tour;
        cm.prov = provQual;
        cm.champ = new Tourny[2];
        cm.champ[0] = tourChampionship;
        cm.champ[1] = provChampionship;
        cm.activeTournies = activeTournies;
        tm.SetTeam();
        xpm.SetSkillPoints();
        cm.SaveCareer();
        SceneManager.LoadScene("Tourny_Menu_1");
    }

    public void SelectTourny(int button)
    {
        currentTourny = activeTournies[button];
        Debug.Log("Button is " + button);
        cm.SetupTourny();
        //playButton.SetActive(true);
        PlayTourny();
    }

    public void Profile(bool on)
    {
        cm = FindObjectOfType<CareerManager>();

        if (cm.provRankList.Count > 0)
        {
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
        }

        if (on)
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(true);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            xpm.xpGO.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);
            profPanel.earnings.text = "$ " + cm.earnings.ToString("n0");
            profPanel.record.text = cm.record.x.ToString() + " - " + cm.record.y.ToString();

            if (cm.provQual)
                profPanel.provQual.text = "Yes";
            else
                profPanel.provQual.text = "No";

            if (cm.provRankList.Count > 0)
            {
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
            
        }
        else
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            xpm.xpGO.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);
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
            xpm.xpGO.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);
        }
        else
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            xpm.xpGO.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);
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
            xpm.xpGO.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);
        }
        else
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            xpm.xpGO.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);
        }
    }

    public void PowerUp(bool on)
    {
        if (on)
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            xpm.xpGO.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(true);
        }
        else
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            xpm.xpGO.SetActive(false);
            powerUpPanel.SetActive(false);
            teamPanel.SetActive(false);
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
            xpm.xpGO.SetActive(true);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);
        }
        else
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            xpm.xpGO.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);
        }
    }

    public void TeamWindow(bool on)
    {
        if (on)
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            xpm.xpGO.SetActive(false);
            teamPanel.SetActive(true);
            powerUpPanel.SetActive(false);
        }
        else
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            xpm.xpGO.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);
        }
    }

    public void Expand(Button expandButton)
    {

        Animator expandAnim = expandButton.GetComponent<Animator>();
        
        //Animator panelAnim = tournyName.transform.parent.gameObject.GetComponent<Animator>();

        expandAnim.SetBool("Shrink", false);
        expandAnim.SetBool("Expand", true);
        expandButton.transform.GetChild(0).gameObject.SetActive(false);

        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (expandButton != menuButtons[i])
            {
                menuButtons[i].gameObject.SetActive(true);
                Image img = menuButtons[i].GetComponent<Image>();
                img.color += new Color(img.color.r, img.color.g, img.color.b, 1f);
                expandAnim = menuButtons[i].GetComponent<Animator>();
                expandAnim.SetBool("Expand", false);
                expandAnim.SetBool("Shrink", true);
            }
            else
            {
                menuBut_Select = i;
                StartCoroutine(WaitForTime(0.1f, i, expandButton));
            }
        }

    }

    IEnumerator WaitForTime(float waitTime, int menuSelector, Button expandButton)
    {
        Profile(false);

        AudioManager am = FindObjectOfType<AudioManager>();
        am.PlayBG(menuSelector);

        yield return new WaitForSeconds(waitTime);

        //expandButton.interactable = false;

        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (i == menuSelector)
            {
                //menuButtons[i].GetComponent<Image>().enabled = false;
                //menuButtons[i].gameObject.SetActive(false);
            }
            else
            {
                menuButtons[i].transform.GetChild(0).gameObject.SetActive(true);
                //menuButtons[i].GetComponent<Image>().enabled = true;
                //menuButtons[i].gameObject.SetActive(true);
                //menuButtons[i].interactable = true;
                Image img = menuButtons[i].GetComponent<Image>();
                img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
                //Debug.Log("img color is " + img.color);
            }

        }

        yield return new WaitUntil(() => menuButtons[menuSelector].GetComponent<RectTransform>().sizeDelta.y == 800f);
        
        menuButtons[menuSelector].gameObject.SetActive(false);
        switch (menuSelector)
        {
            case 0:
                mainMenuGO.SetActive(true);
                //SetActiveTournies();
                break;
            case 1:
                XPWindow(true);
                xpm.SetSkillPoints();
                break;
            case 2:
                TeamWindow(true);
                break;
            case 3:
                PowerUp(true);
                break;
            case 4:
                Standings(true);
                break;
        }
    }
}
