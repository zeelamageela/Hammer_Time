using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public Slider aimSlider;
    public Text aimText;
    public Button[] aimButtons;
    public Slider weighttSlider;
    public Text weightText;
    public Button[] weightButtons;
    public Slider finesseSlider;
    public Text finesseText;
    public Button[] finesseButtons;
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
        cm = FindFirstObjectByType<CareerManager>();
        LoadFromCareerManager(cm);
        AddXP(0);
    }
    
    void OnDisable()
    {
        // Save back to CareerManager when XPManager is closed
        if (cm != null)
        {
            SaveToCareerManager(cm);
            Debug.Log("[XPManager] Saved XP/skillPoints back to CareerManager on disable");
        }
    }
    
    void OnDestroy()
    {
        // Backup save on destroy
        if (cm != null)
        {
            SaveToCareerManager(cm);
            Debug.Log("[XPManager] Saved XP/skillPoints back to CareerManager on destroy");
        }
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

        aimText.text = aimSlider.value.ToString();
        weightText.text = weighttSlider.value.ToString();
        finesseText.text = finesseSlider.value.ToString();
        strengthText.text = strengthSlider.value.ToString();
        endurText.text = endurSlider.value.ToString();
        healthText.text = healthSlider.value.ToString();

        if (skillPoints <= 0)
        {
            aimButtons[1].gameObject.SetActive(false);
            weightButtons[1].gameObject.SetActive(false);
            finesseButtons[1].gameObject.SetActive(false);
            strengthButtons[1].gameObject.SetActive(false);
            endurButtons[1].gameObject.SetActive(false);
            healthButtons[1].gameObject.SetActive(false);
        }
        else
        {
            aimButtons[1].gameObject.SetActive(true);
            weightButtons[1].gameObject.SetActive(true);
            finesseButtons[1].gameObject.SetActive(true);
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
            aimSlider.value = cm.cStats.weightAccuracy;
            weighttSlider.value = cm.cStats.aimAccuracy;
            finesseSlider.value = cm.cStats.finesseAccuracy;
            strengthSlider.value = cm.cStats.sweepStrength;
            endurSlider.value = cm.cStats.sweepEndurance;
            healthSlider.value = cm.cStats.sweepCohesion;
        }
        else
        {
            aimSlider.value = cm.activePlayers[select].weight;
            weighttSlider.value = cm.activePlayers[select].aim;
            finesseSlider.value = cm.activePlayers[select].finesse;
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
            cm.cStats.weightAccuracy = (int)aimSlider.value;
            cm.cStats.aimAccuracy = (int)weighttSlider.value;
            cm.cStats.finesseAccuracy = (int)finesseSlider.value;
            cm.cStats.sweepStrength = (int)strengthSlider.value;
            cm.cStats.sweepEndurance = (int)endurSlider.value;
            cm.cStats.sweepCohesion = (int)healthSlider.value;
        }
        else
        {
            cm.activePlayers[activePlayer].weight = (int)aimSlider.value;
            cm.activePlayers[activePlayer].aim = (int)weighttSlider.value;
            cm.activePlayers[activePlayer].finesse = (int)finesseSlider.value;
            cm.activePlayers[activePlayer].sweepStrength = (int)strengthSlider.value;
            cm.activePlayers[activePlayer].sweepEnduro = (int)endurSlider.value;
            cm.activePlayers[activePlayer].sweepCohesion = (int)healthSlider.value;
        }
    }

    public void ButtonAdd(int skill)
    {
        switch (skill)
        {
            case 0: aimSlider.value += 1; break;
            case 1: weighttSlider.value += 1; break;
            case 2: finesseSlider.value += 1; break;
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
            case 0: aimSlider.value -= 1; break;
            case 1: weighttSlider.value -= 1; break;
            case 2: finesseSlider.value -= 1; break;
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
        TeamMenu tm = FindFirstObjectByType<TeamMenu>();
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

