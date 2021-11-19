using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class XPManager : MonoBehaviour
{
    CareerManager cm;
    CareerStats cStats;
    GameSettingsPersist gsp;

    public int skillPoints;
    public int skillPointsTotal;

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
    void OnEnable()
    {
        cm = FindObjectOfType<CareerManager>();
        cStats = cm.cStats;
        SetSkillPoints();
        SetSliders();
    }

    // Update is called once per frame
    void Update()
    {
        skillPointsText.text = skillPoints.ToString();
        xpCostText.text = skillPointsTotal.ToString();
        cash.text = cm.earnings.ToString();
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

    //Display the data from the cm
    //Spend the skill points
    //Assign back to the cm
    //Display results
    public void SetSliders()
    {
        drawSlider.value = cStats.drawAccuracy;
        takeOutSlider.value = cStats.takeOutAccuracy;
        guardSlider.value = cStats.guardAccuracy;
        strengthSlider.value = cStats.sweepStrength;
        endurSlider.value = cStats.sweepEndurance;
        healthSlider.value = cStats.sweepHealth;

    }

    public void SetSkillPoints()
    {
        float exponent = 2f;
        float xpMult = Mathf.Pow(cm.totalXp, exponent);
        skillPointsTotal = 30 + Mathf.FloorToInt(xpMult / 625f);
        Debug.Log("Skill Points Total is " + skillPointsTotal);
        skillPoints = skillPointsTotal
                    - cStats.drawAccuracy
                    - cStats.takeOutAccuracy
                    - cStats.guardAccuracy
                    - cStats.sweepStrength
                    - cStats.sweepEndurance
                    - (int)(cStats.sweepHealth / 10);
    }

    public void ButtonAdd(int skill)
    {
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
                healthSlider.value += 10;
                skillPoints--;
                break;
        }
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
                healthSlider.value -= 10;
                skillPoints++;
                break;
        }
    }
    public void Back()
    {
        this.gameObject.SetActive(false);
    }
}
