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

    //public Player[] playerPool;

    public PlayerDisplay[] teamDisplay;

    public PlayerDisplay replaceMemberDisplay;
    public PlayerDisplay navDisplay;
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
    public Slider drawModSlider;
    public Slider guardModSlider;
    public Slider takeOutModSlider;
    public Slider strengthModSlider;
    public Slider enduranceModSlider;
    public Slider healthModSlider;

    public Slider oppDrawSlider;
    public Slider oppGuardSlider;
    public Slider oppTakeOutSlider;
    public Slider oppStrengthSlider;
    public Slider oppEnduranceSlider;
    public Slider oppHealthSlider;

    public float xp;
    public float cash;
    //public float costPerWeek;
    public Text title;
    public Text xpText;
    public Text cashText;
    public Text cashDeltaText;
    public Text expenseDeltaText;
    public Text incomeDeltaText;
    public Text costText;
    public Text incomeText;
    public Text recordText;

    public int playerSelect;

    int oppStatBase;

    EasyFileSave myFile;

    bool callCount;
    int cdCount;

    // Start is called before the first frame update
    void Start()
    {
        cm = FindFirstObjectByType<CareerManager>();
        pm = FindFirstObjectByType<SponsorManager>();
        //Shuffle(playerPool);
        callCount = false;
        cdCount = 0;
        CashDeltaText(cm.cashDelta);
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

            // Calculate team base stats (sum of all 4 players)
            int teamBaseDraw = 0;
            int teamBaseGuard = 0;
            int teamBaseTakeOut = 0;
            int teamBaseStrength = 0;
            int teamBaseEndurance = 0;
            int teamBaseCohesion = 0;
            
            // Add the 3 team members' stats
            if (cm.activePlayers != null && cm.activePlayers.Length >= 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    teamBaseDraw += cm.activePlayers[i].weight;
                    teamBaseGuard += cm.activePlayers[i].finesse;
                    teamBaseTakeOut += cm.activePlayers[i].aim;
                    teamBaseStrength += cm.activePlayers[i].sweepStrength;
                    teamBaseEndurance += cm.activePlayers[i].sweepEnduro;
                    teamBaseCohesion += cm.activePlayers[i].sweepCohesion;
                }
            }
            
            // Add the Skip's (player character) stats
            teamBaseDraw += cm.cStats.weightAccuracy;
            teamBaseGuard += cm.cStats.finesseAccuracy;
            teamBaseTakeOut += cm.cStats.aimAccuracy;
            teamBaseStrength += cm.cStats.sweepStrength;
            teamBaseEndurance += cm.cStats.sweepEndurance;
            teamBaseCohesion += cm.cStats.sweepCohesion;
            
            // Base sliders show TEAM total (all 4 players combined)
            drawSlider.value = teamBaseDraw;
            guardSlider.value = teamBaseGuard;
            takeOutSlider.value = teamBaseTakeOut;
            strengthSlider.value = teamBaseStrength;
            enduranceSlider.value = teamBaseEndurance;
            healthSlider.value = teamBaseCohesion;

            // Mod sliders show TEAM total + equipment/sponsor bonuses
            drawModSlider.value = teamBaseDraw + cm.modStats.weightAccuracy;
            guardModSlider.value = teamBaseGuard + cm.modStats.finesseAccuracy;
            takeOutModSlider.value = teamBaseTakeOut + cm.modStats.aimAccuracy;
            strengthModSlider.value = teamBaseStrength + cm.modStats.sweepStrength;
            enduranceModSlider.value = teamBaseEndurance + cm.modStats.sweepEndurance;
            healthModSlider.value = teamBaseCohesion + cm.modStats.sweepCohesion;

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

    public void CashDeltaText(float cashDeltaIn)
    {
        Debug.Log("Cash Delta Text is pressed");
        Animator textAnim = cashDeltaText.GetComponent<Animator>();
        if (cashDeltaIn != 0)
        {
            if (cashDeltaIn < 0)
            {
                cashDeltaIn = -cashDeltaIn;
                cashDeltaText.text = "- $" + cashDeltaIn.ToString("n0");
                textAnim.SetBool("Add", false);
            }
            else
            {
                cashDeltaText.text = "+ $" + cashDeltaIn.ToString("n0");
                textAnim.SetBool("Add", true);
            }

            textAnim.SetTrigger("Pop");
        }

        cdCount++;
    }
    public void ExpenseDeltaText(float deltaIn)
    {
        Debug.Log("Expense Delta Text is pressed");
        Animator textAnim = expenseDeltaText.GetComponent<Animator>();
        if (cdCount > 0 && deltaIn != 0)
        {
            if (deltaIn < 0)
            {
                deltaIn = -deltaIn;
                expenseDeltaText.text = "- $" + deltaIn.ToString("n0");
                textAnim.SetBool("Add", false);
            }
            else
            {
                expenseDeltaText.text = "+ $" + deltaIn.ToString("n0");
                textAnim.SetBool("Add", true);
            }
            textAnim.SetTrigger("Pop");
        }
        
        cdCount++;
    }
    public void IncomeDeltaText(float deltaIn)
    {
        Debug.Log("Income Delta Text is pressed");
        Animator textAnim = incomeDeltaText.GetComponent<Animator>();
        if (cdCount > 0 && deltaIn != 0)
        {
            if (deltaIn < 0)
            {
                deltaIn = -deltaIn;
                incomeDeltaText.text = "- $" + deltaIn.ToString("n0");
                textAnim.SetBool("Add", false);
            }
            else
            {
                incomeDeltaText.text = "+ $" + deltaIn.ToString("n0");
                textAnim.SetBool("Add", true);
            }
            textAnim.SetTrigger("Pop");
        }
        if (cdCount > 0)
        {
        }
        cdCount++;
    }

    public void TeamMenuOpen()
    {
        if (!callCount)
        {
            title.text = "Team";
            callCount = true;
            cm = FindFirstObjectByType<CareerManager>();
            StartCoroutine(SetUpTeam());
        }
        //cm.SaveCareer();
    }

    IEnumerator SetUpTeam()
    {
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
            // Load activePlayers from CareerManager (contains saved stats and metadata)
            if (cm.activePlayers != null && cm.activePlayers.Length >= 3)
            {
                // Copy from cm.activePlayers to local activePlayers
                for (int i = 0; i < activePlayers.Length && i < cm.activePlayers.Length; i++)
                {
                    activePlayers[i] = cm.activePlayers[i];
                }
                
                // Mark these players as active in playerPool
                for (int i = 0; i < activePlayers.Length; i++)
                {
                    for (int j = 0; j < cm.playerPool.Length; j++)
                    {
                        if (activePlayers[i].id == cm.playerPool[j].id)
                        {
                            cm.playerPool[j].active = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                // Fallback: load from playerPool if cm.activePlayers is empty
                Debug.LogWarning("[TeamMenu] cm.activePlayers is null or empty, loading from playerPool");
                for (int i = 0; i < activePlayers.Length; i++)
                {
                    activePlayers[i] = cm.playerPool[i];
                    cm.playerPool[i].active = true;
                }
            }

            if (cm.week == 2)
            {
                Debug.Log("TEAM MENU - Player Rank is " + cm.playerTeam.rank);
            }
            cm.teamPaid = false;

            Shuffle(cm.playerPool);
        }
        else
        {
            // Week 1: Create independent copies from playerPool
            for (int i = 0; i < activePlayers.Length; i++)
            {
                // Clone the player so changes to playerPool don't affect our team
                activePlayers[i] = new Player
                {
                    id = cm.playerPool[i].id,
                    name = cm.playerPool[i].name,
                    description = cm.playerPool[i].description,
                    cost = cm.playerPool[i].cost,
                    image = cm.playerPool[i].image,
                    active = true,
                    view = cm.playerPool[i].view,
                    weight = cm.playerPool[i].weight,
                    finesse = cm.playerPool[i].finesse,
                    aim = cm.playerPool[i].aim,
                    sweepStrength = cm.playerPool[i].sweepStrength,
                    sweepEnduro = cm.playerPool[i].sweepEnduro,
                    sweepCohesion = cm.playerPool[i].sweepCohesion
                };
            }

            Shuffle(cm.playerPool);
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
            for (int j = 0; j < cm.playerPool.Length; j++)
            {
                if (!cm.playerPool[j].active && !cm.playerPool[j].view && !stop)
                {
                    cm.playerPool[j].view = true;
                    freeAgents[i] = cm.playerPool[j];
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
        title.text = "Team";
        navMenu.SetActive(false);
        skillMenu.SetActive(false);
        teamMenu.SetActive(true);
        setTeamButton.SetActive(true);
        agentMenu.SetActive(false);
        //pm.nextWeekButton.gameObject.SetActive(false);
        // DON'T call PreviewPoints() - it was overwriting modStats incorrectly!
        // modStats should only contain equipment/sponsor bonuses, not team member stats

        for (int i = 0; i < cm.playerPool.Length; i++)
        {
            cm.playerPool[i].view = false;

            if (cm.playerPool[i].id == activePlayers[0].id | cm.playerPool[i].id == activePlayers[1].id | cm.playerPool[i].id == activePlayers[2].id)
                cm.playerPool[i].active = true;
            else
                cm.playerPool[i].active = false;
        }

        for (int i = 0; i < teamDisplay.Length; i++)
        {
            if (i == 0)
                teamDisplay[i].charName.text = "Lead - " + activePlayers[i].name;
            if (i == 1)
                teamDisplay[i].charName.text = "2nd - " + activePlayers[i].name;
            if (i == 2)
                teamDisplay[i].charName.text = "3rd - " + activePlayers[i].name;
            teamDisplay[i].cost.text = "$" + activePlayers[i].cost.ToString("N0");
            teamDisplay[i].photo.sprite = activePlayers[i].image;
            teamDisplay[i].description.text = activePlayers[i].description;
        }
    }

    public void ViewFreeAgents()
    {
        title.text = "Free Agents";
        agentMenu.SetActive(true);
        teamMenu.SetActive(false);
        navMenu.SetActive(false);

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
            for (int j = 0; j < cm.playerPool.Length; j++)
            {
                if (cm.playerPool[j].active && !stop)
                {
                    activePlayers[i] = cm.playerPool[j];
                    stop = true;
                }
            }
        }
    }

    public void ChangeTeamMember(int freeAgent)
    {
        int playerToReplace = 99;

        // DON'T call UnPreviewPoints() - it was manipulating modStats incorrectly
        for (int i = 0; i < activePlayers.Length; i++)
        {
            if (activePlayers[i].name == replaceMemberDisplay.charName.text)
            {
                playerToReplace = i;
            }
        }

        Player tempPlayer = activePlayers[playerToReplace];

        for (int i = 0; i < cm.playerPool.Length; i++)
        {
            if (freeAgents[freeAgent].id == cm.playerPool[i].id)
            {
                cm.playerPool[i].active = true;
                
                // Clone the player so changes to playerPool don't affect our team
                activePlayers[playerToReplace] = new Player
                {
                    id = cm.playerPool[i].id,
                    name = cm.playerPool[i].name,
                    description = cm.playerPool[i].description,
                    cost = cm.playerPool[i].cost,
                    image = cm.playerPool[i].image,
                    active = true,
                    view = cm.playerPool[i].view,
                    weight = cm.playerPool[i].weight,
                    finesse = cm.playerPool[i].finesse,
                    aim = cm.playerPool[i].aim,
                    sweepStrength = cm.playerPool[i].sweepStrength,
                    sweepEnduro = cm.playerPool[i].sweepEnduro,
                    sweepCohesion = cm.playerPool[i].sweepCohesion
                };
            }
        }
        freeAgents[freeAgent] = tempPlayer;
        ExpenseDeltaText(activePlayers[playerToReplace].cost - tempPlayer.cost);
        ViewTeam();
    }

    public void SetTeam()
    {
        float teamCost = 0f;

        for (int i = 0; i < activePlayers.Length; i++)
        {
            teamCost += activePlayers[i].cost;
        }

        // Find the player's team in cm.teams and replace its players with activePlayers
        for (int i = 0; i < cm.teams.Length; i++)
        {
            if (cm.teams[i].player)
            {
                // Clear the existing players list
                cm.teams[i].players.Clear();
                // Add each active player to the team
                foreach (var p in activePlayers)
                {
                    cm.teams[i].players.Add(p);
                }
                cm.teams[i].players.Add(cm.playerCharacter); // Add the player's character to the team
                
                // IMPORTANT: Update team skills from the new player roster
                cm.teams[i].UpdateTeamSkillsFromPlayers();
                
                Debug.Log($"[TeamMenu] Team skills updated: weight={cm.teams[i].weight}, strength={cm.teams[i].strength}");
                break; // Only one player team expected
            }
        }

        Debug.Log("Team Cost is " + teamCost);
        
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
        //navMenu.transform.SetSiblingIndex(player);
        //for (int i = 0; i < teamDisplay.Length; i++)
        //{
        //    if (i == player)
        //        teamDisplay[i].charName.transform.parent.gameObject.SetActive(false);
        //    else
        //        teamDisplay[i].charName.transform.parent.gameObject.SetActive(true);
        //}
        title.text = "Team Member";
        navDisplay.charName.text = activePlayers[player].name;
        navDisplay.cost.text = "$" + activePlayers[player].cost.ToString("N0");
        navDisplay.photo.sprite = activePlayers[player].image;
        navDisplay.description.text = activePlayers[player].description;

        if (playerSelect == 0)
            navDisplay.charName.transform.parent.GetChild(3).GetComponent<Text>().text = "Lead";
        else if (playerSelect == 1)
            navDisplay.charName.transform.parent.GetChild(3).GetComponent<Text>().text = "Second";
        else
            navDisplay.charName.transform.parent.GetChild(3).GetComponent<Text>().text = "Third";

        teamMenu.SetActive(false);
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

        title.text = "Skill Points";
        skillMenu.SetActive(true);
        navMenu.SetActive(false);
        teamMenu.SetActive(false);
        setTeamButton.SetActive(false);
        agentMenu.SetActive(false);

        XPManager xpm = FindFirstObjectByType<XPManager>();
        xpm.activePlayer = playerSelect;
        xpm.SetPlayer();
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
}

