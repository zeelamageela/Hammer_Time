using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class XPManager : MonoBehaviour
{
    CareerManager cm;
    public CareerStats[] cStats;
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
        skillPointsTotal = 20 + Mathf.FloorToInt(xpMult / 47.59f);
        //Debug.Log("Skill Points Total is " + skillPointsTotal);
        Debug.Log("SkillPointsTotal pre-calc is " + skillPointsTotal);
        skillPoints = (skillPointsTotal * 4)
                    - cStats[3].drawAccuracy
                    - cStats[3].takeOutAccuracy
                    - cStats[3].guardAccuracy
                    - cStats[3].sweepStrength
                    - cStats[3].sweepEndurance
                    - cStats[3].sweepCohesion
                    -cStats[2].drawAccuracy
                    - cStats[2].takeOutAccuracy
                    - cStats[2].guardAccuracy
                    - cStats[2].sweepStrength
                    - cStats[2].sweepEndurance
                    - cStats[2].sweepCohesion
                    - cStats[1].drawAccuracy
                    - cStats[1].takeOutAccuracy
                    - cStats[1].guardAccuracy
                    - cStats[1].sweepStrength
                    - cStats[1].sweepEndurance
                    - cStats[1].sweepCohesion
                    -cStats[0].drawAccuracy
                    - cStats[0].takeOutAccuracy
                    - cStats[0].guardAccuracy
                    - cStats[0].sweepStrength
                    - cStats[0].sweepEndurance
                    - cStats[0].sweepCohesion;

        Debug.Log("SkillPoints post-calc is " + skillPoints);
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
