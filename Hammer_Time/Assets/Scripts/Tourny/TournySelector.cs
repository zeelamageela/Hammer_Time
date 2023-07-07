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

public class TournySelector : MonoBehaviour
{
    CareerManager cm;
    public XPManager xpm;
    public EquipmentManager em;
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

        SponsorManager pm = FindObjectOfType<SponsorManager>();
        xpm.SetSkillPoints();

        Debug.Log("TSel cm.week is " + cm.week);

        if (cm.week == 0)
        {
            StartCoroutine(NewSeason());
        }
        else
        {
            cm.SetUpCareer();
        }

        teamMenu.TeamMenuOpen();
        pm.SetUp();

        provStandings.PrintRows();
        tourStandings.PrintRows();
        //Debug.Log("Skill Points are " + xpm.skillPoints);
        SetActiveTournies();
        em.SetInventory();
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

        int localSelect = Random.Range(0, locals.Length);
        //Debug.Log("local Select is " + localSelect);

        if (cm.provQual)
            provQualComplete = true;

        if (cm.week == 1)
        {
            //tournies[0].complete = true;
            activeTournies[0] = emptyTourny;
            activeTournies[1] = tournies[nextTourny];
            activeTournies[2] = emptyTourny;
        }
        else if (cm.week == 2)
        {
            activeTournies[0] = locals[localSelect];
            activeTournies[1] = tournies[nextTourny];
            activeTournies[2] = tournies[nextTourny2];
        }
        else if (cm.week == 3)
        {
            activeTournies[0] = emptyTourny;
            activeTournies[1] = tour[nextTour];
            activeTournies[2] = emptyTourny;
        }
        else if (cm.week == 4)
        {
            activeTournies[0] = locals[localSelect];
            activeTournies[1] = provQual[nextProvQual];
            activeTournies[2] = tournies[nextTourny];
        }
        else if (cm.week == 5)
        {
            activeTournies[0] = locals[localSelect];
            activeTournies[1] = tour[nextTour];
            if (provQualComplete)
                activeTournies[2] = tournies[nextTourny];
            else
                activeTournies[2] = provQual[nextProvQual];
        }
        else
        {
            Debug.Log("tourniesComplete - " + tourniesComplete + " | tourComplete - " + tourComplete + " | provQualComplete - " + provQualComplete);

            if (tourChampionship.complete & provChampionship.complete)
            {
                EndOfSeason();
            }
            else if (tourniesComplete & tourComplete & provQualComplete)
            {
                for (int i = 0; i < cm.tourRankList.Count; i++)
                {
                    if (cm.playerTeam.id == cm.tourRankList[i].team.id)
                    {
                        if (i < 6)
                            cm.tourQual = true;
                    }
                }
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
            else if (!tourniesComplete & tourComplete & provQualComplete)
            {
                activeTournies[rnd[0]] = emptyTourny;

                activeTournies[rnd[1]] = tournies[nextTourny];

                if (nextTourny2 == 0)
                    activeTournies[rnd[2]] = emptyTourny;
                else
                    activeTournies[rnd[2]] = tournies[nextTourny2];
            }
            else if (tourniesComplete & !tourComplete & provQualComplete)
            {
                activeTournies[rnd[0]] = emptyTourny;
                activeTournies[rnd[1]] = emptyTourny;
                if (cm.week % 2 == 0)
                    activeTournies[rnd[2]] = emptyTourny;
                else
                    activeTournies[rnd[2]] = tour[nextTour];
            }
            else if (!tourniesComplete & !tourComplete & provQualComplete)
            {
                if (cm.week % 2 == 0)
                    activeTournies[rnd[0]] = emptyTourny;
                else
                    activeTournies[rnd[0]] = tour[nextTour];

                activeTournies[rnd[1]] = tournies[nextTourny];

                if (nextTourny2 == 0)
                    activeTournies[rnd[2]] = emptyTourny;
                else
                    activeTournies[rnd[2]] = tournies[nextTourny2];
            }
            else if (tourniesComplete & !tourComplete & !provQualComplete)
            {
                activeTournies[rnd[0]] = provQual[nextProvQual];
                activeTournies[rnd[1]] = emptyTourny;

                if (cm.week % 2 == 0)
                    activeTournies[rnd[2]] = emptyTourny;
                else
                    activeTournies[rnd[2]] = tour[nextTour];
            }
            else if (!tourniesComplete & tourComplete & !provQualComplete)
            {
                activeTournies[rnd[0]] = provQual[nextProvQual];
                activeTournies[rnd[1]] = tournies[nextTourny];
                activeTournies[rnd[2]] = emptyTourny;
            }
            else
            {

                activeTournies[rnd[0]] = provQual[nextProvQual];
                activeTournies[rnd[1]] = tournies[nextTourny];
                if (cm.week % 2 == 0)
                {
                    if (nextTourny2 == 0)
                        activeTournies[rnd[2]] = emptyTourny;
                    else
                        activeTournies[rnd[2]] = tournies[nextTourny2];
                }
                else
                    activeTournies[rnd[2]] = tour[nextTour];

            }

            for (int i = 0; i < 3; i++)
            {
                if (activeTournies[i].name == emptyTourny.name)
                {
                    activeTournies[i] = locals[localSelect];
                    break;
                }
            }
        }

        SetPanels();
        //cm.SaveCareer();
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
        xpm.SetSkillPoints();
        cm.SaveCareer();
        SceneManager.LoadScene("Tourny_Menu_1");
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
        teamNameText.text = "Team " + cm.teamName + " Members";
        if (on)
        {
            mainMenuGO.SetActive(true);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
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
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);

            //tournyBtn.SetActive(true);
            //playerBtn.SetActive(true);
            //teamBtn.SetActive(true);
            //sponsorBtn.SetActive(true);
            //standingsBtn.SetActive(true);
        }
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
            teamNameText.text = cm.playerName + " " + cm.teamName;
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(true);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);

            //tournyBtn.SetActive(false);
            //playerBtn.SetActive(false);
            //teamBtn.SetActive(false);
            //sponsorBtn.SetActive(false);
            //standingsBtn.SetActive(false);

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
            teamNameText.text = cm.playerName + " " + cm.teamName;
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);

            //tournyBtn.SetActive(true);
            //playerBtn.SetActive(true);
            //teamBtn.SetActive(true);
            //sponsorBtn.SetActive(true);
            //standingsBtn.SetActive(true);
        }
    }

    public void Standings(bool on)
    {
        teamNameText.text = cm.playerName + " " + cm.teamName;

        if (on)
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(true);
            provStandings.PrintRows();
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
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
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);

            //tournyBtn.SetActive(true);
            //playerBtn.SetActive(true);
            //teamBtn.SetActive(true);
            //sponsorBtn.SetActive(true);
            //standingsBtn.SetActive(true);
        }
    }

    public void TourStandings(bool on)
    {
        teamNameText.text = cm.playerName + " " + cm.teamName;
        if (on)
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(true);
            playerMenu.SetActive(false);
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
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);

            //tournyBtn.SetActive(true);
            //playerBtn.SetActive(true);
            //teamBtn.SetActive(true);
            //sponsorBtn.SetActive(true);
            //standingsBtn.SetActive(true);
        }
    }

    public void Sponsors(bool on)
    {
        teamNameText.text = "Sponsors";
        if (on)
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(true);

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
            powerUpPanel.SetActive(false);
            teamPanel.SetActive(false);

            //tournyBtn.SetActive(true);
            //playerBtn.SetActive(true);
            //teamBtn.SetActive(true);
            //sponsorBtn.SetActive(true);
            //standingsBtn.SetActive(true);
        }
    }

    public void PlayerEquip(bool on)
    {
        teamNameText.text = "Gear";
        if (on)
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(true);
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
        teamNameText.text = "Team " + cm.teamName;
        if (on)
        {
            mainMenuGO.SetActive(false);
            profPanelGO.SetActive(false);
            provStandings.gameObject.SetActive(false);
            tourStandings.gameObject.SetActive(false);
            playerMenu.SetActive(false);
            teamPanel.SetActive(true);
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
            teamPanel.SetActive(false);
            powerUpPanel.SetActive(false);

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

        //Animator panelAnim = tournyName.transform.parent.gameObject.GetComponent<Animator>();

        expandAnim.SetBool("Shrink", false);
        expandAnim.SetBool("Expand", true);
        expandButton.transform.GetChild(0).gameObject.SetActive(false);

        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (expandButton != menuButtons[i])
            {
                menuButtons[i].gameObject.SetActive(true);
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
        //Profile(false);

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
                menuButtons[i].GetComponent<Image>().color = btnBlk;
                menuButtons[i].transform.GetChild(0).GetComponent<Text>().color = btnBlk;
            }
            else
            {
                menuButtons[i].transform.GetChild(0).gameObject.SetActive(true);
                menuButtons[i].GetComponent<Image>().enabled = true;
                menuButtons[i].GetComponent<Image>().color = new Color(btnBlk.r, btnBlk.g, btnBlk.b, 1f);
                //menuButtons[i].GetComponent<Image>().color = btnBlk;
                menuButtons[i].transform.GetChild(0).GetComponent<Text>().color = btnWht;
                //Image img = menuButtons[i].GetComponent<Image>();
                //Color tempBG = new Color(img.color.r, img.color.g, img.color.b, 1f);
                //Text txt = menuButtons[i].transform.GetChild(0).gameObject.GetComponent<Text>();
                //Color tempText = new Color(txt.color.r, txt.color.g, txt.color.b, 1f);

                //Debug.Log("Button is " + menuButtons[i] + " / TempBG is " + tempBG + " / TempText is " + tempText);

                //txt.color = tempBG;
                //img.color = new Color(tempText.r, tempText.g, tempText.b, tempText.a);

                //menuButtons[i].GetComponent<Image>().color = tempText;

                //menuButtons[i].GetComponent<Image>().enabled = true;
                //menuButtons[i].gameObject.SetActive(true);
                //menuButtons[i].interactable = true;
                //Image img = menuButtons[i].GetComponent<Image>();
                //img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
                //Debug.Log("img color is " + img.color);
            }
        }

        yield return new WaitUntil(() => menuButtons[menuSelector].GetComponent<RectTransform>().sizeDelta.y == 800f);

        menuButtons[menuSelector].gameObject.SetActive(false);
        switch (menuSelector)
        {
            //Main Tourny Window
            case 0:
                TournyWindow(true);
                teamNameText.text = "Tournies";
                //SetActiveTournies();
                break;
            case 1:
                PlayerEquip(true);
                em.MainMenu();
                //xpm.SetSkillPoints();
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
        }
    }
}
