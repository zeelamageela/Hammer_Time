using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;

public class XPManager : MonoBehaviour
{
    CareerManager cm;
    public CareerStats[] cStats;
    public CareerStats modStats;
    GameSettingsPersist gsp;

    public int level = 1;
    public int xp = 0;
    public int xpToNextLevel = 100;
    public int skillPoints = 0;

    // Track milestones to avoid double-rewarding
    private HashSet<string> milestonesAchieved = new HashSet<string>();

    public int activePlayer;
    public GameObject xpGO;
    public int skillPointsTotal;

    public Button setButton;
    public Button resetButton;
    public Button upgradeButton;
    public Text upgradeText;

    public Text skillPointsText;
    public Text xpCostText;
    public Text xpTotal;
    public Text cash;

    public Slider drawSlider;
    public Text drawText;
    public Button[] drawButtons;
    public Slider takeOutSlider;
    public Text takeOutText;
    public Button[] takeOutButtons;
    public Slider guardSlider;
    public Text guardText;
    public Button[] guardButtons;
    public Slider strengthSlider;
    public Text strengthText;
    public Button[] strengthButtons;
    public Slider endurSlider;
    public Text endurText;
    public Button[] endurButtons;
    public Slider healthSlider;
    public Text healthText;
    public Button[] healthButtons;

    // Start is called before the first frame update

    void Start()
    {
        cm = FindObjectOfType<CareerManager>();
        LoadFromCareerManager(cm);
        AddXP(0);
    }
    public void LoadFromCareerManager(CareerManager cm)
    {
        level = cm.level;
        xp = (int)cm.xp;
        xpToNextLevel = 100 + (level - 1) * 20; // Or however you calculate it
        skillPoints = cm.skillPoints;
        // If you track milestones, copy them as well
        
    }

    public void SaveToCareerManager(CareerManager cm)
    {
        cm.level = level;
        cm.xp = xp;
        cm.skillPoints = skillPoints;
        // If you track milestones, copy them as well
    }

    // Update is called once per frame
    void Update()
    {
        skillPointsText.text = skillPoints.ToString();
        xpCostText.text = skillPointsTotal.ToString();
        //cash.text = "$" + cm.earnings.ToString("n0");

        drawText.text = drawSlider.value.ToString();
        takeOutText.text = takeOutSlider.value.ToString();
        guardText.text = guardSlider.value.ToString();
        strengthText.text = strengthSlider.value.ToString();
        endurText.text = endurSlider.value.ToString();
        healthText.text = healthSlider.value.ToString();

        if (skillPoints <= 0)
        {
            drawButtons[1].gameObject.SetActive(false);
            takeOutButtons[1].gameObject.SetActive(false);
            guardButtons[1].gameObject.SetActive(false);
            strengthButtons[1].gameObject.SetActive(false);
            endurButtons[1].gameObject.SetActive(false);
            healthButtons[1].gameObject.SetActive(false);
        }
        else
        {
            drawButtons[1].gameObject.SetActive(true);
            takeOutButtons[1].gameObject.SetActive(true);
            guardButtons[1].gameObject.SetActive(true);
            strengthButtons[1].gameObject.SetActive(true);
            endurButtons[1].gameObject.SetActive(true);
            healthButtons[1].gameObject.SetActive(true);
        }

    }
    public void AddXP(int amount)
    {
        xp += amount;
        while (xp >= xpToNextLevel)
        {
            xp -= xpToNextLevel;
            level++;
            skillPoints+=5;
            xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.2f); // Increase XP needed per level
        }
    }
    public void AwardMilestone(string milestoneId, int points)
    {
        if (!milestonesAchieved.Contains(milestoneId))
        {
            skillPoints += points;
            milestonesAchieved.Add(milestoneId);
            // Optionally: Show UI feedback here
        }
    }


    public void SetSliders(int select)
    {
        // Set sliders to match the selected player's stats
        if (select == 3)
        {
            drawSlider.value = cm.cStats.drawAccuracy;
            takeOutSlider.value = cm.cStats.takeOutAccuracy;
            guardSlider.value = cm.cStats.guardAccuracy;
            strengthSlider.value = cm.cStats.sweepStrength;
            endurSlider.value = cm.cStats.sweepEndurance;
            healthSlider.value = cm.cStats.sweepCohesion;
        }
        else
        {
            drawSlider.value = cm.activePlayers[select].draw;
            takeOutSlider.value = cm.activePlayers[select].takeOut;
            guardSlider.value = cm.activePlayers[select].guard;
            strengthSlider.value = cm.activePlayers[select].sweepStrength;
            endurSlider.value = cm.activePlayers[select].sweepEnduro;
            healthSlider.value = cm.activePlayers[select].sweepCohesion;
        }
        activePlayer = select;
        skillPointsText.text = skillPoints.ToString();
    }

    public void ApplySlidersToPlayer()
    {
        // Apply slider values to the selected player
        if (activePlayer == 3)
        {
            cm.cStats.drawAccuracy = (int)drawSlider.value;
            cm.cStats.takeOutAccuracy = (int)takeOutSlider.value;
            cm.cStats.guardAccuracy = (int)guardSlider.value;
            cm.cStats.sweepStrength = (int)strengthSlider.value;
            cm.cStats.sweepEndurance = (int)endurSlider.value;
            cm.cStats.sweepCohesion = (int)healthSlider.value;
        }
        else
        {
            cm.activePlayers[activePlayer].draw = (int)drawSlider.value;
            cm.activePlayers[activePlayer].takeOut = (int)takeOutSlider.value;
            cm.activePlayers[activePlayer].guard = (int)guardSlider.value;
            cm.activePlayers[activePlayer].sweepStrength = (int)strengthSlider.value;
            cm.activePlayers[activePlayer].sweepEnduro = (int)endurSlider.value;
            cm.activePlayers[activePlayer].sweepCohesion = (int)healthSlider.value;
        }
    }

    public void ButtonAdd(int skill)
    {
        switch (skill)
        {
            case 0: drawSlider.value += 1; break;
            case 1: takeOutSlider.value += 1; break;
            case 2: guardSlider.value += 1; break;
            case 3: strengthSlider.value += 1; break;
            case 4: endurSlider.value += 1; break;
            case 5: healthSlider.value += 1; break;
        }
        skillPoints--;
        ApplySlidersToPlayer();
    }

    public void ButtonSubtract(int skill)
    {
        switch (skill)
        {
            case 0: drawSlider.value -= 1; break;
            case 1: takeOutSlider.value -= 1; break;
            case 2: guardSlider.value -= 1; break;
            case 3: strengthSlider.value -= 1; break;
            case 4: endurSlider.value -= 1; break;
            case 5: healthSlider.value -= 1; break;
        }
        skillPoints++;
        ApplySlidersToPlayer();
    }

    public void SetPlayer()
    {
        // When switching to a new player, update sliders to match their stats
        SetSliders(activePlayer);
    }

    public void SwitchPlayers(bool nextUp)
    {
        ApplySlidersToPlayer(); // Save current slider values to player before switching

        if (nextUp)
        {
            activePlayer = (activePlayer + 1) % 4;
        }
        else
        {
            activePlayer = (activePlayer - 1 + 4) % 4;
        }

        SetSliders(activePlayer);
        TeamMenu tm = FindObjectOfType<TeamMenu>();
        tm.playerSelect = activePlayer;
        tm.SkillMenu();
    }

    public void ResetStats()
    {
        // Optionally, reset stats to some default or previous values
        SetSliders(activePlayer);
    }

    //public void Back()
    //{
    //    this.gameObject.SetActive(false);
    //}
}
