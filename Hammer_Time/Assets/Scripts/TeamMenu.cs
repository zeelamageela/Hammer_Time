using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TigerForge;

public class TeamMenu : MonoBehaviour
{
    CareerManager cm;
    public SponsorManager pm;
    public TournySelector tSel;
    public XPManager xpm;

    public GameObject teamMenu;
    public GameObject agentMenu;
    public GameObject setTeamButton;
    public GameObject skillMenu;
    public GameObject navMenu;

    public Player[] activePlayers;
    public Player[] freeAgents;

    public Color buttonDisabledColor;
    public Color buttonEnabledColor;

    public Player[] playerPool;

    public PlayerDisplay[] teamDisplay;

    public PlayerDisplay replaceMemberDisplay;
    public PlayerDisplay[] freeAgentDisplay;
    public PlayerDisplay skillDisplay;

    public GameObject dialogueGO;
    public DialogueTrigger coachGreen;

    public Slider drawSlider;
    public Slider guardSlider;
    public Slider takeOutSlider;
    public Slider strengthSlider;
    public Slider enduranceSlider;
    public Slider healthSlider;

    public Slider oppDrawSlider;
    public Slider oppGuardSlider;
    public Slider oppTakeOutSlider;
    public Slider oppStrengthSlider;
    public Slider oppEnduranceSlider;
    public Slider oppHealthSlider;

    public float xp;
    public float cash;
    //public float costPerWeek;
    public Text xpText;
    public Text cashText;
    public Text costText;
    public Text incomeText;
    public Text recordText;

    public int playerSelect;

    int oppStatBase;

    EasyFileSave myFile;

    bool callCount;

    // Start is called before the first frame update
    void Start()
    {
        cm = FindObjectOfType<CareerManager>();
        pm = FindObjectOfType<SponsorManager>();
        //Shuffle(playerPool);
        callCount = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (cm)
        {
            if (cm.week < 5)
                oppStatBase = 5;
            else if (cm.week < 10)
                oppStatBase = 7;
            else
                oppStatBase = 10;

            drawSlider.value = cm.cStats.drawAccuracy + cm.modStats.drawAccuracy;
            guardSlider.value = cm.cStats.guardAccuracy + cm.modStats.guardAccuracy;
            takeOutSlider.value = cm.cStats.takeOutAccuracy + cm.modStats.takeOutAccuracy;
            enduranceSlider.value = cm.cStats.sweepEndurance + cm.modStats.sweepEndurance;
            strengthSlider.value = cm.cStats.sweepStrength + cm.modStats.sweepStrength;
            healthSlider.value = cm.cStats.sweepCohesion + cm.modStats.sweepCohesion;

            //oppDrawSlider.value = oppStatBase + cm.oppStats.drawAccuracy;
            //oppGuardSlider.value = oppStatBase + cm.oppStats.guardAccuracy;
            //oppTakeOutSlider.value = oppStatBase + cm.oppStats.takeOutAccuracy;
            //oppEnduranceSlider.value = oppStatBase + cm.oppStats.sweepEndurance;
            //oppStrengthSlider.value = oppStatBase + cm.oppStats.sweepStrength;
            //oppHealthSlider.value = oppStatBase + cm.oppStats.sweepCohesion;

            xp = cm.xp;
            cash = cm.cash;
            xpText.text = xp.ToString();
            cashText.text = "$" + cash.ToString("n0");
            recordText.text = cm.record.x.ToString() + "-" + cm.record.y.ToString();

            if (cm.activePlayers.Length > 0 && pm)
            {
                cm.costPerWeek = cm.activePlayers[0].cost + cm.activePlayers[1].cost + cm.activePlayers[2].cost + pm.costPerWeek;

                float cost = cm.activePlayers[0].cost + cm.activePlayers[1].cost + cm.activePlayers[2].cost;
                float income = pm.costPerWeek;

                incomeText.text = "$" + income.ToString("n0");
                costText.text = "$" + cost.ToString("n0");
            }
        }
    }

    public void TeamMenuOpen()
    {
        if (!callCount)
        {
            callCount = true;
            cm = FindObjectOfType<CareerManager>();
            StartCoroutine(SetUpTeam());
        }
        //cm.SaveCareer();
    }

    IEnumerator SetUpTeam()
    {
        myFile = new EasyFileSave("my_player_data");

        cm.coachDialogue = new bool[tSel.coachGreen.dialogue.Length];
        //cm.qualDialogue = new bool[tSel.coachGreen.qualDialogue.Length];
        //cm.reviewDialogue = new bool[tSel.coachGreen.reviewDialogue.Length];
        //cm.introDialogue = new bool[tSel.coachGreen.introDialogue.Length];
        //cm.storyDialogue = new bool[tSel.coachGreen.storyDialogue.Length];
        //cm.helpDialogue = new bool[tSel.coachGreen.helpDialogue.Length];
        //cm.strategyDialogue = new bool[tSel.coachGreen.strategyDialogue.Length];

        for (int i = 0; i < cm.coachDialogue.Length; i++)
            cm.coachDialogue[i] = false;
        for (int i = 0; i < cm.qualDialogue.Length; i++)
            cm.qualDialogue[i] = false;
        for (int i = 0; i < cm.reviewDialogue.Length; i++)
            cm.reviewDialogue[i] = false;
        for (int i = 0; i < cm.introDialogue.Length; i++)
            cm.introDialogue[i] = false;
        for (int i = 0; i < cm.storyDialogue.Length; i++)
            cm.storyDialogue[i] = false;
        for (int i = 0; i < cm.helpDialogue.Length; i++)
            cm.helpDialogue[i] = false;
        for (int i = 0; i < cm.strategyDialogue.Length; i++)
            cm.strategyDialogue[i] = false;

        //Debug.Log("TeamMenu Earnings are " + cm.earnings);
        if (cm.week > 1)
        {
            //cm.LoadCareer();
            if (myFile.Load())
            {
                for (int i = 0; i < activePlayers.Length; i++)
                {
                    //Debug.Log("Active Players ID is " + i + " - " + activePlayers[i].id);
                    for (int j = 0; j < playerPool.Length; j++)
                    {
                        if (activePlayers[i].id == playerPool[j].id)
                        {
                            activePlayers[i].name = playerPool[j].name;
                            activePlayers[i].description = playerPool[j].description;
                            activePlayers[i].cost = playerPool[j].cost;
                            activePlayers[i].image = playerPool[j].image;
                            activePlayers[i].view = playerPool[j].view;
                            activePlayers[i].active = true;
                            playerPool[j].active = true;
                        }
                    }
                }

                if (cm.week == 2)
                {
                    Debug.Log("TEAM MENU - Player Rank is " + cm.playerTeam.rank);
                }
            }
            cm.teamPaid = false;

            Shuffle(playerPool);
        }
        else
        {
            for (int i = 0; i < activePlayers.Length; i++)
            {
                activePlayers[i] = playerPool[i];
            }

            Shuffle(playerPool);

            //dialogueGO.SetActive(true);
            //coachGreen.TriggerDialogue("Intro", 8);
            //cm.introDialogue[8] = true;
        }

        SelectFreeAgents();
        cm.activePlayers = activePlayers;

        ViewTeam();

        yield return new WaitForEndOfFrame();
    }

    public void SelectFreeAgents()
    {
        //Shuffle(playerPool);

        for (int i = 0; i < freeAgents.Length; i++)
        {
            bool stop = false;
            for (int j = 0; j < playerPool.Length; j++)
            {
                if (!playerPool[j].active && !playerPool[j].view && !stop)
                {
                    playerPool[j].view = true;
                    freeAgents[i] = playerPool[j];
                    stop = true;
                }
            }
        }
    }

    public void ViewTeam()
    {
        for (int i = 0; i < freeAgentDisplay.Length; i++)
        {
            if (!freeAgentDisplay[i].charName.transform.parent.GetComponent<Button>().interactable)
            {
                freeAgentDisplay[i].charName.rectTransform.anchoredPosition += new Vector2(15f, 15f);
                freeAgentDisplay[i].cost.rectTransform.anchoredPosition += new Vector2(15f, 15f);
                //freeAgentDisplay[i].photo.rectTransform.anchoredPosition += new Vector2(35f, 35f);
                freeAgentDisplay[i].photo.transform.parent.gameObject.GetComponent<Image>().rectTransform.anchoredPosition += new Vector2(35f, 35f);
                GameObject tempPanel = freeAgentDisplay[i].charName.transform.parent.GetChild(2).gameObject;
                tempPanel.GetComponent<Image>().rectTransform.anchoredPosition += new Vector2(35f, 35f);
                freeAgentDisplay[i].charName.transform.parent.GetComponent<Image>().color = buttonEnabledColor;
                freeAgentDisplay[i].charName.transform.parent.GetComponent<Button>().interactable = true;
            }
        }

        for (int i = 0; i < teamDisplay.Length; i++)
        {
            teamDisplay[i].charName.transform.parent.gameObject.SetActive(true);
        }

        navMenu.SetActive(false);
        skillMenu.SetActive(false);
        teamMenu.SetActive(true);
        setTeamButton.SetActive(true);
        agentMenu.SetActive(false);
        //pm.nextWeekButton.gameObject.SetActive(false);
        PreviewPoints();

        for (int i = 0; i < playerPool.Length; i++)
        {
            playerPool[i].view = false;

            if (playerPool[i].id == activePlayers[0].id | playerPool[i].id == activePlayers[1].id | playerPool[i].id == activePlayers[2].id)
                playerPool[i].active = true;
            else
                playerPool[i].active = false;
        }

        for (int i = 0; i < teamDisplay.Length; i++)
        {
            if (i == 0)
                teamDisplay[i].charName.text = "Lead - " + activePlayers[i].name;
            if (i == 1)
                teamDisplay[i].charName.text = "Second - " + activePlayers[i].name;
            if (i == 2)
                teamDisplay[i].charName.text = "Third - " + activePlayers[i].name;
            teamDisplay[i].cost.text = "$" + activePlayers[i].cost.ToString("N0");
            teamDisplay[i].photo.sprite = activePlayers[i].image;
            teamDisplay[i].description.text = activePlayers[i].description;
        }

    }

    public void ViewFreeAgents()
    {
        agentMenu.SetActive(true);
        teamMenu.SetActive(false);

        replaceMemberDisplay.charName.text = activePlayers[playerSelect].name;
        replaceMemberDisplay.cost.text = "$" + activePlayers[playerSelect].cost.ToString("N0");
        replaceMemberDisplay.photo.sprite = activePlayers[playerSelect].image;
        replaceMemberDisplay.description.text = activePlayers[playerSelect].description;
        if (playerSelect == 0)
            replaceMemberDisplay.charName.transform.parent.GetChild(4).GetComponent<Text>().text = "Lead";
        else if (playerSelect == 1)
            replaceMemberDisplay.charName.transform.parent.GetChild(4).GetComponent<Text>().text = "Second";
        else
            replaceMemberDisplay.charName.transform.parent.GetChild(4).GetComponent<Text>().text = "Third";

        float moneyToSpend = cm.cash - pm.costPerWeek;
        Debug.Log("Money to spend preTeam is " + moneyToSpend);

        for (int i = 0; i < activePlayers.Length; i++)
        {
            if (i != playerSelect)
            {
                moneyToSpend -= (activePlayers[i].cost);
            }
        }
        Debug.Log("Money to Spend postTeam is " + moneyToSpend);
        for (int i = 0; i < freeAgentDisplay.Length; i++)
        {
            freeAgentDisplay[i].charName.text = freeAgents[i].name;
            freeAgentDisplay[i].cost.text = "$" + freeAgents[i].cost.ToString("N0");
            freeAgentDisplay[i].photo.sprite = freeAgents[i].image;

            if (freeAgents[i].cost > moneyToSpend)
            {
                freeAgentDisplay[i].charName.rectTransform.anchoredPosition -= new Vector2(15f, 15f);
                freeAgentDisplay[i].cost.rectTransform.anchoredPosition -= new Vector2(15f, 15f);
                //freeAgentDisplay[i].photo.rectTransform.anchoredPosition -= new Vector2(15f, 15f);
                freeAgentDisplay[i].photo.transform.parent.gameObject.GetComponent<Image>().rectTransform.anchoredPosition -= new Vector2(35f, 35f);
                GameObject tempPanel = freeAgentDisplay[i].charName.transform.parent.GetChild(2).gameObject;
                tempPanel.GetComponent<Image>().rectTransform.anchoredPosition -= new Vector2(35f, 35f);
                freeAgentDisplay[i].charName.transform.parent.GetComponent<Image>().color = buttonDisabledColor;
                freeAgentDisplay[i].charName.transform.parent.GetComponent<Button>().interactable = false;
            }
        }

    }

    void TeamSelect()
    {
        for (int i = 0; i < activePlayers.Length; i++)
        {
            bool stop = false;
            for (int j = 0; j < playerPool.Length; j++)
            {
                if (playerPool[j].active && !stop)
                {
                    activePlayers[i] = playerPool[j];
                    stop = true;
                }
            }
        }
    }

    public void ChangeTeamMember(int freeAgent)
    {
        int playerToReplace = 99;

        UnPreviewPoints();
        for (int i = 0; i < activePlayers.Length; i++)
        {
            if (activePlayers[i].name == replaceMemberDisplay.charName.text)
            {
                playerToReplace = i;
            }
        }

        Player tempPlayer = activePlayers[playerToReplace];

        for (int i = 0; i < playerPool.Length; i++)
        {
            if (freeAgents[freeAgent].id == playerPool[i].id)
            {
                playerPool[i].active = true;
                activePlayers[playerToReplace] = playerPool[i];
            }
        }
        freeAgents[freeAgent] = tempPlayer;

        ViewTeam();
    }

    public void SetTeam()
    {
        float teamCost = 0f;

        for (int i = 0; i < activePlayers.Length; i++)
        {
            teamCost += activePlayers[i].cost;
        }

        Debug.Log("Team Cost is " + teamCost);

        cm.cash -= cm.costPerWeek;
        cm.teamPaid = true;
        //teamMenu.SetActive(false);
        //agentMenu.SetActive(false);
        ////pm.cardParent.SetActive(true);
        //setTeamButton.SetActive(false);
        //pm.nextWeekButton.SetActive(true);
        //cm.SaveCareer();
        
        for (int i = 0; i < teamDisplay.Length; i++)
        {
            teamDisplay[i].charName.rectTransform.anchoredPosition -= new Vector2(15f, 15f);
            teamDisplay[i].cost.rectTransform.anchoredPosition -= new Vector2(15f, 15f);
            teamDisplay[i].description.rectTransform.anchoredPosition -= new Vector2(15f, 15f);
            //freeAgentDisplay[i].photo.rectTransform.anchoredPosition -= new Vector2(15f, 15f);
            teamDisplay[i].photo.transform.parent.gameObject.GetComponent<Image>().rectTransform.anchoredPosition -= new Vector2(35f, 35f);
            GameObject tempPanel = freeAgentDisplay[i].charName.transform.parent.GetChild(2).gameObject;
            tempPanel.GetComponent<Image>().rectTransform.anchoredPosition -= new Vector2(35f, 35f);
            teamDisplay[i].charName.transform.parent.GetComponent<Image>().color = buttonDisabledColor;
            teamDisplay[i].charName.transform.parent.GetComponent<Button>().interactable = false;
        }

        //pm.SetUp();
    }

    public void NavMenu(int player)
    {
        navMenu.transform.SetSiblingIndex(player);
        for (int i = 0; i < teamDisplay.Length; i++)
        {
            if (i == player)
                teamDisplay[i].charName.transform.parent.gameObject.SetActive(false);
            else
                teamDisplay[i].charName.transform.parent.gameObject.SetActive(true);
        }
        navMenu.SetActive(true);
        playerSelect = player;

    }

    public void SkillMenu()
    {
        if (playerSelect == 3)
        {
            skillDisplay.charName.text = cm.playerName + " " + cm.teamName;
            skillDisplay.cost.text = " ";
            skillDisplay.photo.enabled = false;
            skillDisplay.description.text = "Your stats.";
        }
        else
        {
            skillDisplay.charName.text = activePlayers[playerSelect].name;
            skillDisplay.cost.text = "$" + activePlayers[playerSelect].cost.ToString("N0");
            skillDisplay.photo.enabled = true;
            skillDisplay.photo.sprite = activePlayers[playerSelect].image;
            skillDisplay.description.text = activePlayers[playerSelect].description;
            if (playerSelect == 0)
                skillDisplay.charName.transform.parent.GetChild(4).GetComponent<Text>().text = "Lead";
            else if (playerSelect == 1)
                skillDisplay.charName.transform.parent.GetChild(4).GetComponent<Text>().text = "Second";
            else
                skillDisplay.charName.transform.parent.GetChild(4).GetComponent<Text>().text = "Third";
        }

        skillMenu.SetActive(true);
        navMenu.SetActive(false);
        teamMenu.SetActive(false);
        setTeamButton.SetActive(false);
        agentMenu.SetActive(false);
    }

    void Shuffle(Player[] a)
    {
        Debug.Log("Shuffling");
        // Loops through array
        for (int i = a.Length - 1; i > 0; i--)
        {
            // Randomize a number between 0 and i (so that the range decreases each time)
            int rnd = Random.Range(0, i);

            // Save the value of the current i, otherwise it'll overright when we swap the values
            Player temp = a[i];

            // Swap the new and old values
            a[i] = a[rnd];
            a[rnd] = temp;
        }
    }

    public void PreviewPoints()
    {
        cm = FindObjectOfType<CareerManager>();
        pm = FindObjectOfType<SponsorManager>();

        cm.modStats.drawAccuracy = activePlayers[0].draw + activePlayers[1].draw + activePlayers[2].draw;
        cm.modStats.guardAccuracy = activePlayers[0].guard + activePlayers[1].guard + activePlayers[2].guard;
        cm.modStats.takeOutAccuracy = activePlayers[0].takeOut + activePlayers[1].takeOut + activePlayers[2].takeOut;
        cm.modStats.sweepEndurance = activePlayers[0].sweepEnduro + activePlayers[1].sweepEnduro + activePlayers[2].sweepEnduro;
        cm.modStats.sweepStrength = activePlayers[0].sweepStrength + activePlayers[1].sweepStrength + activePlayers[2].sweepStrength;
        cm.modStats.sweepCohesion = activePlayers[0].sweepCohesion + activePlayers[1].sweepCohesion + activePlayers[2].sweepCohesion;
        cm.oppStats.drawAccuracy = activePlayers[0].oppDraw + activePlayers[1].oppDraw + activePlayers[2].oppDraw;
        cm.oppStats.guardAccuracy = activePlayers[0].oppGuard + activePlayers[1].oppGuard + activePlayers[2].oppGuard;
        cm.oppStats.takeOutAccuracy = activePlayers[0].oppTakeOut + activePlayers[1].oppTakeOut + activePlayers[2].oppTakeOut;
        cm.oppStats.sweepEndurance = activePlayers[0].oppEnduro + activePlayers[1].oppEnduro + activePlayers[2].oppEnduro;
        cm.oppStats.sweepStrength = activePlayers[0].oppStrength + activePlayers[1].oppStrength + activePlayers[2].oppStrength;
        cm.oppStats.sweepCohesion = activePlayers[0].oppCohesion + activePlayers[1].oppCohesion + activePlayers[2].oppCohesion;
    }

    public void UnPreviewPoints()
    {
        cm = FindObjectOfType<CareerManager>();
        pm = FindObjectOfType<SponsorManager>();

        for (int i = 0; i < pm.activeCards.Length; i++)
        {
            cm.modStats.drawAccuracy += pm.activeCards[i].draw;
            cm.modStats.guardAccuracy += pm.activeCards[i].guard;
            cm.modStats.takeOutAccuracy += pm.activeCards[i].takeOut;
            cm.modStats.sweepEndurance += pm.activeCards[i].sweepEnduro;
            cm.modStats.sweepStrength += pm.activeCards[i].sweepStrength;
            cm.modStats.sweepCohesion += pm.activeCards[i].sweepCohesion;
            cm.oppStats.drawAccuracy += pm.activeCards[i].oppDraw;
            cm.oppStats.guardAccuracy += pm.activeCards[i].oppGuard;
            cm.oppStats.takeOutAccuracy += pm.activeCards[i].oppTakeOut;
            cm.oppStats.sweepEndurance += pm.activeCards[i].oppEnduro;
            cm.oppStats.sweepStrength += pm.activeCards[i].oppStrength;
            cm.oppStats.sweepCohesion += pm.activeCards[i].oppCohesion;
        }
    }
}
