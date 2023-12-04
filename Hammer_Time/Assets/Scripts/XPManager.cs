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

    public int activePlayer;
    public GameObject xpGO;
    public int skillPoints;
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
        cStats = new CareerStats[4];
        for (int i = 0; i < cStats.Length; i++)
        {
            cStats[i] = new CareerStats();
            if (i == 3)
            {
                cStats[i].drawAccuracy = cm.cStats.drawAccuracy;
                cStats[i].takeOutAccuracy = cm.cStats.takeOutAccuracy;
                cStats[i].guardAccuracy = cm.cStats.guardAccuracy;
                cStats[i].sweepStrength = cm.cStats.sweepStrength;
                cStats[i].sweepEndurance = cm.cStats.sweepEndurance;
                cStats[i].sweepCohesion = cm.cStats.sweepCohesion;
            }
            else
            {
                cStats[i].drawAccuracy = cm.activePlayers[i].draw;
                cStats[i].takeOutAccuracy = cm.activePlayers[i].takeOut;
                cStats[i].guardAccuracy = cm.activePlayers[i].guard;
                cStats[i].sweepStrength = cm.activePlayers[i].sweepStrength;
                cStats[i].sweepEndurance = cm.activePlayers[i].sweepEnduro;
                cStats[i].sweepCohesion = cm.activePlayers[i].sweepCohesion;
            }
            modStats.drawAccuracy += cStats[i].drawAccuracy;
            modStats.takeOutAccuracy += cStats[i].takeOutAccuracy;
            modStats.guardAccuracy += cStats[i].guardAccuracy;
            modStats.sweepStrength += cStats[i].sweepStrength;
            modStats.sweepEndurance += cStats[i].sweepEndurance;
            modStats.sweepCohesion += cStats[i].sweepCohesion;
        }
        activePlayer = 3;
        SetSliders(3);
    }

    // Update is called once per frame
    void Update()
    {
        skillPointsText.text = skillPoints.ToString();
        xpCostText.text = skillPointsTotal.ToString();
        //cash.text = "$" + cm.earnings.ToString("n0");
        xpTotal.text = cm.totalXp.ToString();

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

    public void SetSliders(int select)
    {
        Debug.Log("Select is " + select);
        drawSlider.value = cStats[select].drawAccuracy;
        takeOutSlider.value = cStats[select].takeOutAccuracy;
        guardSlider.value = cStats[select].guardAccuracy;
        strengthSlider.value = cStats[select].sweepStrength;
        endurSlider.value = cStats[select].sweepEndurance;
        healthSlider.value = cStats[select].sweepCohesion;
        SetSkillPoints(select);
    }

    public void SetSkillPoints(int select)
    {
        float exponent = 1.2f;
        float xpMult = Mathf.Pow(cm.totalXp, exponent);
        skillPointsTotal = 63 + Mathf.FloorToInt(xpMult / 47.59f);

        Debug.Log("SkillPoints pre-calc is " + skillPointsTotal);

        int modPoints = 0;
        for (int i = 0; i < 4; i++)
        {
            modPoints += cStats[i].drawAccuracy;
            modPoints += cStats[i].takeOutAccuracy;
            modPoints += cStats[i].guardAccuracy;
            modPoints += cStats[i].sweepStrength;
            modPoints += cStats[i].sweepEndurance;
            modPoints += cStats[i].sweepCohesion;
        }

        skillPoints = skillPointsTotal - modPoints;

        Debug.Log("SkillPoints post-calc is " + skillPoints);

        if (skillPoints < 0)
            skillPoints = 0;
    }

    public void SetSkillPoints2(int select)
    {
        float exponent = 1.2f;
        float xpMult = Mathf.Pow(cm.totalXp, exponent);
        skillPointsTotal = 4 + Mathf.FloorToInt(xpMult / 47.59f);
    }

    public void ButtonAdd(int skill)
    {
        if (setButton.IsActive())
            Debug.Log("Set Button is active");
        else
            Debug.Log("Set button is inactive");

        switch (skill)
        {
            case 0:
                drawSlider.value += 1;
                skillPoints--;
                break;
            case 1:
                takeOutSlider.value += 1;
                skillPoints--;
                break;
            case 2:
                guardSlider.value += 1;
                skillPoints--;
                break;
            case 3:
                strengthSlider.value += 1;
                skillPoints--;
                break;
            case 4:
                endurSlider.value += 1;
                skillPoints--;
                break;
            case 5:
                healthSlider.value += 1;
                skillPoints--;
                break;
        }
        SetStats();
    }

    public void ButtonSubtract(int skill)
    {
        switch (skill)
        {
            case 0:
                drawSlider.value -= 1;
                skillPoints++;
                break;
            case 1:
                takeOutSlider.value -= 1;
                skillPoints++;
                break;
            case 2:
                guardSlider.value -= 1;
                skillPoints++;
                break;
            case 3:
                strengthSlider.value -= 1;
                skillPoints++;
                break;
            case 4:
                endurSlider.value -= 1;
                skillPoints++;
                break;
            case 5:
                healthSlider.value -= 1;
                skillPoints++;
                break;
        }
        SetStats();
    }

    public void SetStats()
    {
        cm = FindObjectOfType<CareerManager>();

        cStats[activePlayer].drawAccuracy = (int)drawSlider.value;
        cStats[activePlayer].takeOutAccuracy = (int)takeOutSlider.value;
        cStats[activePlayer].guardAccuracy = (int)guardSlider.value;
        cStats[activePlayer].sweepStrength = (int)strengthSlider.value;
        cStats[activePlayer].sweepEndurance = (int)endurSlider.value;
        cStats[activePlayer].sweepCohesion = (int)healthSlider.value;

        if (activePlayer == 3)
        {
            cm.cStats.drawAccuracy = cStats[activePlayer].drawAccuracy;
            cm.cStats.takeOutAccuracy = cStats[activePlayer].takeOutAccuracy;
            cm.cStats.guardAccuracy = cStats[activePlayer].guardAccuracy;
            cm.cStats.sweepStrength = cStats[activePlayer].sweepStrength;
            cm.cStats.sweepEndurance = cStats[activePlayer].sweepEndurance;
            cm.cStats.sweepCohesion = cStats[activePlayer].sweepCohesion;
        }
        else
        {
            cm.activePlayers[activePlayer].draw = cStats[activePlayer].drawAccuracy;
            cm.activePlayers[activePlayer].takeOut = cStats[activePlayer].takeOutAccuracy;
            cm.activePlayers[activePlayer].guard = cStats[activePlayer].guardAccuracy;
            cm.activePlayers[activePlayer].sweepStrength = cStats[activePlayer].sweepStrength;
            cm.activePlayers[activePlayer].sweepEnduro = cStats[activePlayer].sweepEndurance;
            cm.activePlayers[activePlayer].sweepCohesion = cStats[activePlayer].sweepCohesion;
        }

        //resetButton.interactable = false;
        //this.gameObject.SetActive(false);
    }

    public void SwitchPlayers(bool nextUp)
    {
        SetStats();

        if (nextUp)
        {
            activePlayer++;
            if (activePlayer > 3)
                activePlayer = 0;
        }
        else
        {
            activePlayer--;
            if (activePlayer < 0)
                activePlayer = 3;
        }

        cStats = new CareerStats[4];
        for (int i = 0; i < cStats.Length; i++)
        {
            cStats[i] = new CareerStats();
            if (i == 3)
            {
                cStats[i].drawAccuracy = cm.cStats.drawAccuracy;
                cStats[i].takeOutAccuracy = cm.cStats.takeOutAccuracy;
                cStats[i].guardAccuracy = cm.cStats.guardAccuracy;
                cStats[i].sweepStrength = cm.cStats.sweepStrength;
                cStats[i].sweepEndurance = cm.cStats.sweepEndurance;
                cStats[i].sweepCohesion = cm.cStats.sweepCohesion;
            }
            else
            {
                Debug.Log("i = " + i);
                cStats[i].drawAccuracy = cm.activePlayers[i].draw;
                cStats[i].takeOutAccuracy = cm.activePlayers[i].takeOut;
                cStats[i].guardAccuracy = cm.activePlayers[i].guard;
                cStats[i].sweepStrength = cm.activePlayers[i].sweepStrength;
                cStats[i].sweepEndurance = cm.activePlayers[i].sweepEnduro;
                cStats[i].sweepCohesion = cm.activePlayers[i].sweepCohesion;
            }
            
        }
        SetSliders(activePlayer);
        TeamMenu tm = FindObjectOfType<TeamMenu>();
        tm.playerSelect = activePlayer;
        tm.SkillMenu();
    }

    public void ResetStats()
    {
        cStats = new CareerStats[4];
        for (int i = 0; i < cStats.Length; i++)
        {
            cStats[i].drawAccuracy = cm.activePlayers[i].draw;
            cStats[i].takeOutAccuracy = cm.activePlayers[i].takeOut;
            cStats[i].guardAccuracy = cm.activePlayers[i].guard;
            cStats[i].sweepStrength = cm.activePlayers[i].sweepStrength;
            cStats[i].sweepEndurance = cm.activePlayers[i].sweepEnduro;
            cStats[i].sweepCohesion = cm.activePlayers[i].sweepCohesion;
        }
    }

    //public void Back()
    //{
    //    this.gameObject.SetActive(false);
    //}
}
