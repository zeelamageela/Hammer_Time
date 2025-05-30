using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TigerForge;
using MoreMountains.Tools;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Lofelt.NiceVibrations;
using System;

public class TournySelector : MonoBehaviour
{
    CareerManager cm;
    public XPManager xpm;
    public EquipmentManager em;
    public GameObject dialogueGO;
    public DialogueTrigger coachGreen;

    public string mode;

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
    public Button backButton;
    public Color btnWht;
    public Color btnBlk;

    public Text XPText;

    public GameObject playButton;
    public Tourny[] locals;
    public Tourny[] tournies;
    public Tourny[] tourQual;
    public Tourny[] tour;
    public Tourny[] provQual;
    public Tourny[] majors;
    public Tourny[] singleKOs;

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
    public GameObject playerMenu;
    public GameObject xpWindow;
    public GameObject profPanelGO;
    public ProfilePanel profPanel;
    public GameObject teamPanel;
    public TeamMenu teamMenu;
    public GameObject powerUpPanel;
    public GameObject skillbars;
    public GameObject options;

    public ProvStandings provStandings;
    public TourStandings tourStandings;
    int week;

    public int menuBut_Select;
    public bool provQualDialogue;
    bool tourQualDialogue;
    EasyFileSave myFile;
    public bool teamPaid;

    private bool isFirstRun = true; // Add this field to track the first run 
    private int lastExpandedIndex = -1;

    private void Start()
    {
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

        SponsorManager pm = FindObjectOfType<SponsorManager>();

        Debug.Log("TSel cm.week is " + cm.week);

        if (cm.week == 0)
        {
            cm.NewSeason();
        }
        else
        {
            cm.LoadCareer();
        }

        teamMenu.TeamMenuOpen();
        pm.SetUp();

        em.SetInventory();

        //provStandings.SetUp();
        //Debug.Log("Skill Points are " + xpm.skillPoints);
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
                int rnd = UnityEngine.Random.Range(0, i);

                // Save the value of the current i, otherwise it'll overwrite when we swap the values
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
                int rnd = UnityEngine.Random.Range(0, i);

                // Save the value of the current i, otherwise it'll overwrite when we swap the values
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
        mode = cm.debug;

        bool tourniesComplete = false;
        bool tourComplete = false;
        bool provQualComplete = false;
        bool skoComplete = false;

        int nextTourny = 0;
        int nextTour = 0;
        int nextProvQual = 0;
        int nextTourny2 = 0;
        int nextTourny3 = 0;
        int nextSKO = 0;

        int[] rnd = new int[3] { 0, 1, 2 };
        Shuffle(null, rnd);

        int tCount = 0;

        for (int i = 0; i < tournies.Length; i++)
        {
            //Debug.Log("tournies[" + i + "].complete = " + tournies[i].complete);
            if (tournies[i].complete)
            {
                tourniesComplete = true;
            }
            else
            {
                if (tCount == 1)
                {
                    nextTourny2 = i;
                    //Debug.Log("TSel nextTourny2 is " + nextTourny2);
                    tourniesComplete = false;
                    tCount = 2;
                }
                else if (tCount == 2)
                {
                    nextTourny3 = i;
                    //Debug.Log("TSel nextTourny2 is " + nextTourny2);
                    tourniesComplete = false;
                    break;
                }
                else
                {
                    nextTourny = i;
                    //Debug.Log("TSel nextTourny is " + nextTourny);
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
                //Debug.Log("TSel nextTour is " + nextTour);
                tourComplete = false;
                break;
            }
        }

        for (int i = 0; i < singleKOs.Length; i++)
        {
            if (singleKOs[i].complete)
            {
                skoComplete = true;
            }
            else
            {
                nextSKO = i;
                //Debug.Log("TSel nextTour is " + nextTour);
                skoComplete = false;
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
                //Debug.Log("TSel nextProvQual is " + nextProvQual);
                provQualComplete = false;
                break;
            }
        }

        StorylineManager slm = FindObjectOfType<StorylineManager>();

        int localSelect = UnityEngine.Random.Range(0, locals.Length);
        //Debug.Log("local Select is " + localSelect);

        if (cm.provQual)
            provQualComplete = true;
        switch (mode)
        {

            case "tour":
                #region Tour
                if (cm.week == 1)
                {
                    cm.cash = 100000;
                    slm.skipTutorials = true;
                    slm.FirstFiveTrigger();
                }
                //tournies[0].complete = true;
                if (!tourComplete)
                {
                    activeTournies[0] = emptyTourny;
                    activeTournies[1] = tour[nextTour];
                    activeTournies[2] = emptyTourny;
                }
                else
                {
                    slm.EndOfGame(6);
                    cm.gameOver = true;

                    StartCoroutine(EndOfSeason());
                }
                #endregion
                break;

            case "singleKO":
                #region SingleKO
                if (cm.week == 1)
                {
                    cm.cash = 100000;
                    slm.skipTutorials = true;
                    slm.FirstFiveTrigger();
                }
                //tournies[0].complete = true;
                if (!skoComplete)
                {
                    activeTournies[0] = emptyTourny;
                    activeTournies[1] = singleKOs[nextSKO];
                    activeTournies[2] = emptyTourny;
                }
                else
                {
                    slm.EndOfGame(7);
                    cm.gameOver = true;

                    StartCoroutine(EndOfSeason());
                }
                #endregion
                break;
            case "tournies":
                #region Tournies
                if (cm.week == 1)
                {
                    cm.cash = 100000;
                    slm.skipTutorials = true;
                    slm.FirstFiveTrigger();
                }
                //tournies[0].complete = true;
                if (!tourniesComplete)
                {
                    activeTournies[1] = tournies[nextTourny];
                    if (nextTourny2 == 0)
                        activeTournies[0] = emptyTourny;
                    else
                        activeTournies[0] = tournies[nextTourny2];
                    if (nextTourny3 == 0)
                        activeTournies[2] = emptyTourny;
                    else
                        activeTournies[2] = tournies[nextTourny3];
                }
                else
                {
                    slm.EndOfGame(8);
                    cm.gameOver = true;

                    StartCoroutine(EndOfSeason());
                }
                #endregion
                break;

            case "provQual":
                #region Tournies
                if (cm.week == 1)
                {
                    cm.cash = 100000;
                    slm.skipTutorials = true;
                    slm.FirstFiveTrigger();
                }
                //tournies[0].complete = true;
                if (!provQualComplete)
                {
                    activeTournies[0] = emptyTourny;
                    activeTournies[1] = provQual[nextProvQual];
                    activeTournies[2] = emptyTourny;
                }
                else
                {
                    slm.EndOfGame(9);
                    cm.gameOver = true;

                    StartCoroutine(EndOfSeason());
                }
                #endregion
                break;
            default:
                #region Regular
                if (cm.week == 1)
                {
                    slm.FirstFiveTrigger();
                    //tournies[0].complete = true;
                    activeTournies[0] = emptyTourny;
                    activeTournies[1] = tournies[nextTourny];
                    activeTournies[2] = emptyTourny;

                }
                else if (cm.week == 2)
                {
                    slm.FirstFiveTrigger();
                    activeTournies[0] = emptyTourny;
                    activeTournies[1] = provQual[nextProvQual];
                    activeTournies[2] = tournies[nextTourny];
                }
                else if (cm.week == 3)
                {
                    slm.FirstFiveTrigger();
                    activeTournies[0] = emptyTourny;
                    if (provQualComplete)
                        activeTournies[1] = tournies[nextTourny];
                    else
                        activeTournies[1] = provQual[nextProvQual];
                    activeTournies[2] = emptyTourny;
                }
                else if (cm.week == 4)
                {
                    slm.FirstFiveTrigger();
                    activeTournies[0] = locals[localSelect];
                    if (provQualComplete)
                        activeTournies[1] = tournies[nextTourny2];
                    else
                        activeTournies[1] = provQual[nextProvQual];
                    activeTournies[2] = tournies[nextTourny];
                }
                else if (cm.week == 5)
                {
                    slm.FirstFiveTrigger();
                    activeTournies[0] = locals[localSelect];
                    activeTournies[1] = tournies[nextTourny];
                    if (provQualComplete)
                        activeTournies[2] = tournies[nextTourny2];
                    else
                        activeTournies[2] = provQual[nextProvQual];
                }
                else if (cm.week == 6)
                {
                    slm.FirstFiveTrigger();
                    activeTournies[0] = locals[localSelect];
                    if (cm.provQual)
                        activeTournies[1] = tour[nextTour];
                    else
                        activeTournies[1] = tournies[nextTourny];
                    if (cm.provQual)
                        activeTournies[2] = emptyTourny;
                    else
                        activeTournies[2] = tournies[nextTourny2];
                }
                else
                {
                    Debug.Log("tourniesComplete - " + tourniesComplete + " | tourComplete - " + tourComplete + " | provQualComplete - " + provQualComplete);
                    if (!cm.provQual)
                        tourComplete = true;

                    if (tourChampionship.complete & provChampionship.complete)
                    {
                        slm.EndOfGame(4);
                        cm.gameOver = true;
                        StartCoroutine(EndOfSeason());
                    }
                    else if (tourniesComplete && tourComplete)
                    {
                        if (cm.provQual)
                        {
                            for (int i = 0; i < cm.tourRankList.Count; i++)
                            {
                                if (cm.playerTeam.id == cm.tourRankList[i].team.id)
                                {
                                    if (i < 6)
                                        cm.tourQual = true;
                                }
                            }
                        }
                        else tourChampionship.complete = true;

                        bool champQual = false;
                        for (int i = 0; i < provStandings.teams.Length; i++)
                        {
                            if (provStandings.teams[i].player)
                            {
                                if (i < 16)
                                {
                                    champQual = true;
                                    break;
                                }

                            }
                        }

                        if (cm.tourQual)
                            tourChampionship.complete = false;
                        else
                            tourChampionship.complete = true;

                        if (champQual)
                            provChampionship.complete = false;
                        else
                            tourChampionship.complete = true;

                        if (tourChampionship.complete & provChampionship.complete)
                        {
                            if (!cm.tourQual && !champQual && !cm.provQual)
                                slm.EndOfGame(0);
                            else if (!cm.tourQual && !champQual)
                                slm.EndOfGame(1);
                            else if (cm.tourQual && !champQual)
                                slm.EndOfGame(2);
                            else if (!cm.tourQual && champQual)
                                slm.EndOfGame(3);
                            else if (cm.tourQual && champQual)
                                slm.EndOfGame(4);
                            cm.gameOver = true;
                            StartCoroutine(EndOfSeason());
                        }

                        if (!tourChampionship.complete)
                            activeTournies[rnd[0]] = tourChampionship;
                        else
                            activeTournies[rnd[0]] = emptyTourny;

                        activeTournies[rnd[1]] = emptyTourny;

                        if (!provChampionship.complete)
                            activeTournies[rnd[2]] = provChampionship;
                        else
                            activeTournies[rnd[2]] = emptyTourny;
                    }
                    else if (tourniesComplete && !tourComplete)
                    {
                        activeTournies[rnd[0]] = emptyTourny;
                        activeTournies[rnd[1]] = tour[nextTour];
                        activeTournies[rnd[2]] = emptyTourny;
                    }
                    else if (!tourniesComplete && tourComplete)
                    {
                        activeTournies[rnd[0]] = emptyTourny;
                        activeTournies[rnd[1]] = tournies[nextTourny];
                        if (nextTourny2 == 0)
                            activeTournies[rnd[2]] = emptyTourny;
                        else
                            activeTournies[rnd[2]] = tournies[nextTourny2];
                    }
                    else
                    {
                        if (week % 2 == 0)
                        {
                            activeTournies[0] = tournies[nextTourny];
                            activeTournies[1] = tour[nextTour];
                            if (nextTourny2 == 0)
                                activeTournies[2] = emptyTourny;
                            else
                                activeTournies[2] = tournies[nextTourny2];
                        }
                        else
                        {
                            activeTournies[0] = emptyTourny;
                            activeTournies[1] = tour[nextTour];
                            activeTournies[2] = tournies[nextTourny];
                        }
                    }


                    if (activeTournies[1].name == emptyTourny.name)
                        activeTournies[1] = locals[localSelect];
                    else if (activeTournies[0].name == emptyTourny.name)
                        activeTournies[0] = locals[localSelect];
                    else if (activeTournies[2].name == emptyTourny.name)
                        activeTournies[2] = locals[localSelect];
                }

                if (cm.cash < cm.costPerWeek)
                {
                    slm.EndOfGame(5);
                    cm.gameOver = true;

                    StartCoroutine(EndOfSeason());
                }
                #endregion
                break;
        }

        SetPanels();
        cm.SaveCareer();
    }

    public void SetPanels()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (emptyTourny.name == activeTournies[i].name)
            {
                panelGOs[i].GetComponent<Image>().color = dimmed[0];
                panels[i].location.text = "";
                panels[i].location.transform.parent.GetComponent<Image>().color = dimmed[0];
                panels[i].titleButton.interactable = false;
                panels[i].buttonText.text = activeTournies[i].name;
                panels[i].buttonText.color = dimmed[0];
                panels[i].teams.text = "";
                panels[i].format.text = "";
                panels[i].purse.text = "";
                panels[i].entry.text = "";
                panels[i].sprite.enabled = false;
                playButton.SetActive(false);
            }
            else
            {
                if (activeTournies[i].entryFee > cm.cash)
                {
                    panelGOs[i].GetComponent<Image>().color = dimmed[0];
                    panels[i].location.transform.parent.GetComponent<Image>().color = dimmed[0];
                    panels[i].titleButton.interactable = false;
                }
                else
                {
                    panelGOs[i].GetComponent<Image>().color = yellow[0];
                    panels[i].titleButton.interactable = true;
                }
                panels[i].location.text = activeTournies[i].location;
                panels[i].buttonText.text = activeTournies[i].name;
                panels[i].teams.text = activeTournies[i].teams.ToString();
                panels[i].format.text = activeTournies[i].format;
                panels[i].purse.text = "$" + activeTournies[i].prizeMoney.ToString("n0");
                panels[i].entry.text = "$" + activeTournies[i].entryFee.ToString("n0");
                panels[i].sprite.enabled = true;
                panels[i].sprite.sprite = activeTournies[i].image;
                playButton.SetActive(false);
            }
        }


    }

    public void EndOfGame()
    {
        cm = FindObjectOfType<CareerManager>();
        dialogueGO.SetActive(true);
        coachGreen.TriggerDialogue("Story", 0);

        cm.EndCareer();
    }

    IEnumerator EndOfSeason()
    {
        StorylineManager slm = FindObjectOfType<StorylineManager>();
        Debug.Log("End of Game dialogue is " + slm.dialogueGO.activeSelf);
        yield return new WaitUntil(() => !slm.dialogueGO.activeSelf);

        Debug.Log("End of Game movin along");
        cm.EndCareer();
        SceneManager.LoadScene("SplashMenu");
    }

    public void NextWeek()
    {
        cm.NextWeek();
    }

    public void PlayTourny()
    {
        for (int i = 0; i < tournies.Length; i++)
        {
            for (int j = 0; j < activeTournies.Length; j++)
            {
                if (activeTournies[j].id == tournies[i].id)
                {
                    tournies[i].complete = true;
                }
            }
        }

        for (int i = 0; i < tour.Length; i++)
        {
            for (int j = 0; j < activeTournies.Length; j++)
            {
                if (activeTournies[j].id == tour[i].id)
                {
                    tour[i].complete = true;
                }
            }
        }

        for (int i = 0; i < provQual.Length; i++)
        {
            for (int j = 0; j < activeTournies.Length; j++)
            {
                if (activeTournies[j].id == provQual[i].id)
                {
                    provQual[i].complete = true;
                }
            }
        }

        for (int i = 0; i < singleKOs.Length; i++)
        {
            for (int j = 0; j < activeTournies.Length; j++)
            {
                if (activeTournies[j].id == singleKOs[i].id)
                {
                    singleKOs[i].complete = true;
                }
            }
        }

        if (currentTourny.championship)
        {
            if (currentTourny.name == tourChampionship.name)
                tourChampionship.complete = true;
            if (currentTourny.name == provChampionship.name)
                provChampionship.complete = true;
        }

        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        TeamMenu tm = FindObjectOfType<TeamMenu>();
        XPManager xpm = FindObjectOfType<XPManager>();

        gsp.LoadFromTournySelector();

        Debug.Log("TSel Current Tourny Name is " + currentTourny.name);

        cm.tournies = tournies;
        cm.tour = tour;
        cm.prov = provQual;
        cm.champ = new Tourny[2];
        cm.champ[0] = tourChampionship;
        cm.champ[1] = provChampionship;
        cm.activeTournies = activeTournies;
        tm.SetTeam();
        xpm.SaveToCareerManager(cm);
        //cm.SaveCareer();

        //SceneManager.LoadScene("Tourny_Menu_1");
        TournyMenuLoad(FindObjectOfType<TournySettings>());
        //StartCoroutine(TournyMenuLoad());
    }

    private void TournyMenuLoad(TournySettings ts)
    {
        teamNameText.text = "Confirm";
        for (int i = 0; i < menuButtons.Length; i++)
        {
            menuButtons[i].gameObject.SetActive(false);
        }
        TournyWindow(false); ;
        skillbars.SetActive(true);
        ts.Settings();
    }

    public void SelectTourny(int button)
    {
        currentTourny = activeTournies[button];
        Debug.Log("Button is " + button);
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        cm.SetupTourny(this, gsp);
        //playButton.SetActive(true);
        PlayTourny();
    }

    public void TournyWindow(bool on)
    {
        if (on)
        {
            teamNameText.text = "Available Tournies";
            mainMenuGO.SetActive(true);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
            xpWindow.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);

            //tournyBtn.SetActive(false);
            //playerBtn.SetActive(false);
            //teamBtn.SetActive(false);
            //sponsorBtn.SetActive(false);
            //standingsBtn.SetActive(false);
        }
        else
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
            xpWindow.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);

            //tournyBtn.SetActive(true);
            //playerBtn.SetActive(true);
            //teamBtn.SetActive(true);
            //sponsorBtn.SetActive(true);
            //standingsBtn.SetActive(true);
        }
    }

    public void Profile(Button button)
    {
        cm = FindObjectOfType<CareerManager>();
        bool on;
        if (profPanelGO.activeSelf)
            on = false;
        else
            on = true;

        if (cm.provRankList.Count > 0)
        {
            for (int i = 0; i < cm.teams.Length; i++)
            {
                if (cm.playerTeamIndex == cm.provRankList[i].team.id)
                {
                    if (cm.teams[i].rank == 1 | cm.teams[i].rank == 21)
                        profPanel.provRank.text = cm.teams[i].rank.ToString() + "st";
                    else if (cm.teams[i].rank == 2 | cm.teams[i].rank == 22)
                        profPanel.provRank.text = cm.teams[i].rank.ToString() + "nd";
                    else if (cm.teams[i].rank == 3 | cm.teams[i].rank == 23)
                        profPanel.provRank.text = cm.teams[i].rank.ToString() + "rd";
                    else if (cm.teams[i].rank > 3)
                        profPanel.provRank.text = cm.teams[i].rank.ToString() + "th";
                }
            }
        }

        if (on)
        {
            teamNameText.text = cm.playerName + " " + cm.teamName;
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(true);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
            xpWindow.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);
            skillbars.SetActive(true);


            for (int i = 0; i < menuButtons.Length; i++)
            {
                menuButtons[i].gameObject.SetActive(false);

            }

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
                        if (i == 0 | i == 20)
                            profPanel.provRank.text = (i + 1).ToString() + "st";
                        else if (i == 1 | i == 21)
                            profPanel.provRank.text = (i + 1).ToString() + "nd";
                        else if (i == 2 | i == 22)
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
            for (int i = 0; i < menuButtons.Length; i++)
            {
                menuButtons[i].gameObject.SetActive(true);
                menuButtons[i].transform.GetChild(0).gameObject.SetActive(true);
                menuButtons[i].GetComponent<Image>().enabled = true;
                menuButtons[i].GetComponent<Image>().color = new Color(btnBlk.r, btnBlk.g, btnBlk.b, 1f);
                //menuButtons[i].GetComponent<Image>().color = btnBlk;
                menuButtons[i].transform.GetChild(0).GetComponent<Text>().color = btnWht;

            }
            teamNameText.text = cm.playerName + " " + cm.teamName;
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);
            skillbars.SetActive(true);

        }

    }

    public void Options(bool on)
    {
        options.SetActive(true);
    }

    public void QuitToMenu()
    {
        SceneManager.LoadScene("SplashMenu");
    }

    public void Standings(bool on)
    {

        if (on)
        {
            teamNameText.text = "Overall Standings";
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(true);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
            xpWindow.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);
            skillbars.SetActive(false);
        }
        else
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
            xpWindow.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);
            skillbars.SetActive(true);
        }
    }

    public void TourStandings(bool on)
    {
        if (on)
        {
            teamNameText.text = "Tour Standings";
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(true);
            playerMenu.SetActive(false);
            xpWindow.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);
            skillbars.SetActive(false);
        }
        else
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
            xpWindow.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);
            skillbars.SetActive(true);
        }
    }

    public void Sponsors(bool on)
    {
        if (on)
        {
            teamNameText.text = "Sponsors";
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
            xpWindow.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(true);
            skillbars.SetActive(true);

            //tournyBtn.SetActive(false);
            //playerBtn.SetActive(false);
            //teamBtn.SetActive(false);
            //sponsorBtn.SetActive(false);
            //standingsBtn.SetActive(false);
        }
        else
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
            xpWindow.SetActive(false);
            powerUpPanel.SetActive(false);
            teamPanel.SetActive(false);
            skillbars.SetActive(true);

            //tournyBtn.SetActive(true);
            //playerBtn.SetActive(true);
            //teamBtn.SetActive(true);
            //sponsorBtn.SetActive(true);
            //standingsBtn.SetActive(true);
        }
    }

    public void PlayerEquip(bool on)
    {
        em.MainMenu();
        if (on)
        {
            teamNameText.text = "Gear";
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(true);
            xpWindow.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);
            skillbars.SetActive(true);

            //tournyBtn.SetActive(false);
            //playerBtn.SetActive(false);
            //teamBtn.SetActive(false);
            //sponsorBtn.SetActive(false);
            //standingsBtn.SetActive(false);
        }
        else
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
            xpWindow.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);
            skillbars.SetActive(true);

            //tournyBtn.SetActive(true);
            //playerBtn.SetActive(true);
            //teamBtn.SetActive(true);
            //sponsorBtn.SetActive(true);
            //standingsBtn.SetActive(true);
        }
    }

    public void XP(bool on)
    {
        if (on)
        {
            teamNameText.text = "SkillPoints";
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
            xpWindow.SetActive(true);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);

            //tournyBtn.SetActive(false);
            //playerBtn.SetActive(false);
            //teamBtn.SetActive(false);
            //sponsorBtn.SetActive(false);
            //standingsBtn.SetActive(false);
        }
        else
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
            xpWindow.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);

            //tournyBtn.SetActive(true);
            //playerBtn.SetActive(true);
            //teamBtn.SetActive(true);
            //sponsorBtn.SetActive(true);
            //standingsBtn.SetActive(true);
        }
    }

    public void TeamWindow(bool on)
    {
        if (on)
        {
            teamNameText.text = "Team";
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
            xpWindow.SetActive(false);
            teamPanel.SetActive(true);
            powerUpPanel.SetActive(false);
            skillbars.SetActive(true);

            //tournyBtn.SetActive(false);
            //playerBtn.SetActive(false);
            //teamBtn.SetActive(false);
            //sponsorBtn.SetActive(false);
            //standingsBtn.SetActive(false);
        }
        else
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
            xpWindow.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);
            skillbars.SetActive(true);

            //tournyBtn.SetActive(true);
            //playerBtn.SetActive(true);
            //teamBtn.SetActive(true);
            //sponsorBtn.SetActive(true);
            //standingsBtn.SetActive(true);
        }
    }

    public void Expand(Button expandButton)
    {
        Animator expandAnim = expandButton.GetComponent<Animator>();

        // Turn off all panels initially  
        TournyWindow(false);
        PlayerEquip(false);
        TeamWindow(false);
        Sponsors(false);
        Standings(false);

        // Identify the selected button  
        for (int i = 0; i < menuButtons.Length; i++)
        {
            menuButtons[i].gameObject.SetActive(true);

            if (expandButton == menuButtons[i])
            {
                menuBut_Select = i;
            }
        }
        int previousExpanded = lastExpandedIndex;
        menuBut_Select = Array.IndexOf(menuButtons, expandButton);
        lastExpandedIndex = menuBut_Select;

        // Start the expand/shrink process  
        StartCoroutine(HandleExpandShrink(0.01f, menuBut_Select, expandButton, previousExpanded));
    }

    IEnumerator HandleExpandShrink(float waitTime, int menuSelector, Button expandButton, int previousExpanded)
    {
        AudioManager am = FindObjectOfType<AudioManager>();
        am.PlayBG(menuSelector);

        yield return new WaitForSeconds(waitTime);

        Animator expandAnim = expandButton.GetComponent<Animator>();
        if (!isFirstRun && previousExpanded != -1 && previousExpanded != menuSelector)
        {
            Animator shrinkAnim = menuButtons[previousExpanded].GetComponent<Animator>();
            shrinkAnim.SetBool("Expand", false);
            shrinkAnim.SetBool("Shrink", true);
            StartCoroutine(ResetShrink(shrinkAnim, 0.5f));
        }
        else
        {
            isFirstRun = false;
        }
        // Shrink previously expanded button only  
        if (!isFirstRun)
        {
            for (int i = 0; i < menuButtons.Length; i++)
            {
                if (i != menuSelector && menuButtons[i].GetComponent<Animator>().GetBool("Expand"))
                {
                    Debug.Log("shrinking button is " + menuButtons[i].name);

                    Animator shrinkAnim = menuButtons[i].GetComponent<Animator>();

                    shrinkAnim.SetBool("Expand", false);
                    shrinkAnim.SetBool("Shrink", true);
                    // After setting shrinkAnim.SetBool("Shrink", true);
                    StartCoroutine(ResetShrink(shrinkAnim, 0.5f)); // 0.5f = duration of shrink animation
                }
            }
        }
        else
        {
            isFirstRun = false; // Set to false after the first run  
        }

        // Expand the selected button  
        expandAnim.SetBool("Shrink", false);
        expandAnim.SetBool("Expand", true);
        expandButton.transform.GetChild(0).gameObject.SetActive(false);

        // Update button visuals  
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (i == menuSelector)
            {
                menuButtons[i].GetComponent<Image>().color = btnBlk;
                menuButtons[i].transform.GetChild(0).GetComponent<Text>().color = btnBlk;
            }
            else
            {
                menuButtons[i].transform.GetChild(0).gameObject.SetActive(true);
                menuButtons[i].GetComponent<Image>().enabled = true;
                menuButtons[i].GetComponent<Image>().color = new Color(btnBlk.r, btnBlk.g, btnBlk.b, 1f);
                menuButtons[i].transform.GetChild(0).GetComponent<Text>().color = btnWht;
            }
        }

        // Wait for the expand animation to complete  
        yield return new WaitUntil(() => menuButtons[menuSelector].GetComponent<RectTransform>().sizeDelta.y >= 799f);

        // Turn off the expanded button and activate the corresponding panel  
        menuButtons[menuSelector].gameObject.SetActive(false);
        switch (menuSelector)
        {
            case 0:
                TournyWindow(true);
                break;
            case 1:
                PlayerEquip(true);
                break;
            case 2:
                TeamWindow(true);
                break;
            case 3:
                Sponsors(true);
                break;
            case 4:
                Standings(true);
                break;
            case 5:
                break;
        }
    }


    IEnumerator ResetShrink(Animator anim, float delay)
        {
            yield return new WaitForSeconds(delay);
            anim.SetBool("Shrink", false);
        }

    }

