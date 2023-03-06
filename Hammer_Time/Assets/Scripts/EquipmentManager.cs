using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class EquipUI
{
    public Text name;
    public Text cost;
    public Text text;
    public Image image;
}

public class EquipmentManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject buyMenu;
    public GameObject confirmMenu;

    public Equipment[] activeEquip;

    public Equipment[] handles;
    public Equipment[] heads;
    public Equipment[] footwear;
    public Equipment[] apparel;

    public Equipment[] forSaleEquip;

    public EquipUI[] equipUIs;
    public EquipUI[] forSaleUIs;
    public EquipUI forSaleHeader;

    public Text confirmText;

    Equipment origEquip;
    Equipment newEquip;

    // Start is called before the first frame update
    void Start()
    {
        MainMenu();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void MainMenu()
    {
        mainMenu.SetActive(true);
        buyMenu.SetActive(false);
        confirmMenu.SetActive(false);

        for (int i = 0; i < equipUIs.Length; i++)
        {
            equipUIs[i].name.text = activeEquip[i].name;
            equipUIs[i].cost.text = "$" + activeEquip[i].cost.ToString("n0");
            equipUIs[i].text.text = activeEquip[i].text;
            equipUIs[i].image.sprite = activeEquip[i].img;
        }

        for (int i = 0; i < activeEquip.Length; i++)
        {
            SetPoints(i);
        }
    }

    public void BuyMenu(int n)
    {
        mainMenu.SetActive(false);
        buyMenu.SetActive(true);
        confirmMenu.SetActive(false);

        Equipment[] tempEquip = new Equipment[forSaleUIs.Length];
        switch (n)
        {
            case 0:
                tempEquip = handles;
                break;

            case 1:
                tempEquip = heads;
                break;

            case 2:
                tempEquip = footwear;
                break;

            case 3:
                tempEquip = apparel;
                break;

            default:
                tempEquip = handles;
                break;
        }

        forSaleEquip = new Equipment[forSaleUIs.Length];

        for(int i = 0; i < forSaleEquip.Length; i++)
        {
            forSaleEquip[i] = tempEquip[i];
        }

        origEquip = activeEquip[n];

        forSaleHeader.name.text = activeEquip[n].name;
        forSaleHeader.cost.text = "$" + activeEquip[n].cost.ToString("n0");
        forSaleHeader.text.text = activeEquip[n].text;
        forSaleHeader.image.sprite = activeEquip[n].img;

        for (int i = 0; i < forSaleUIs.Length; i++)
        {
            forSaleUIs[i].name.text = tempEquip[i].name;
            forSaleUIs[i].cost.text = "$" + tempEquip[i].cost.ToString("n0");
            forSaleUIs[i].image.sprite = tempEquip[i].img;
        }
    }

    public void BuyItem(int n)
    {
        mainMenu.SetActive(false);
        buyMenu.SetActive(true);
        confirmMenu.SetActive(true);

        newEquip = forSaleEquip[n];

        forSaleHeader.name.text = newEquip.name;
        forSaleHeader.cost.text = "$" + newEquip.cost.ToString("n0");
        forSaleHeader.text.text = newEquip.text;
        forSaleHeader.image.sprite = newEquip.img;

        confirmText.text = "Buy this card for $" + newEquip.cost.ToString("n0") + "?";
    }

    public void Confirm(bool buy)
    {
        confirmMenu.SetActive(false);

        if (buy)
        {
            mainMenu.SetActive(true);
            buyMenu.SetActive(false);

            for (int i = 0; i < equipUIs.Length; i++)
            {
                if (activeEquip[i].name == origEquip.name)
                {
                    activeEquip[i] = newEquip;
                }
                equipUIs[i].name.text = activeEquip[i].name;
                equipUIs[i].cost.text = "$" + activeEquip[i].cost.ToString("n0");
                equipUIs[i].text.text = activeEquip[i].text;
                equipUIs[i].image.sprite = activeEquip[i].img;
            }
        }
        else
        {
            mainMenu.SetActive(false);
            buyMenu.SetActive(true);

            forSaleHeader.name.text = origEquip.name;
            forSaleHeader.cost.text = "$" + origEquip.cost.ToString("n0");
            forSaleHeader.text.text = origEquip.text;
            forSaleHeader.image.sprite = origEquip.img;

            for (int i = 0; i < equipUIs.Length; i++)
            {
                equipUIs[i].name.text = activeEquip[i].name;
                equipUIs[i].cost.text = "$" + activeEquip[i].cost.ToString("n0");
                equipUIs[i].text.text = activeEquip[i].text;
                equipUIs[i].image.sprite = activeEquip[i].img;
            }
        }
    }

    public void GenerateItems(Equipment[]equipList)
    {
        Equipment[] temp = new Equipment[10 * equipList.Length];


    }

    public void SetPoints(int card)
    {
        CareerManager cm = FindObjectOfType<CareerManager>();

        cm.modStats.drawAccuracy += activeEquip[card].draw;
        cm.modStats.guardAccuracy += activeEquip[card].guard;
        cm.modStats.takeOutAccuracy += activeEquip[card].takeOut;
        cm.modStats.sweepEndurance += activeEquip[card].sweepEnduro;
        cm.modStats.sweepStrength += activeEquip[card].sweepStrength;
        cm.modStats.sweepCohesion += activeEquip[card].sweepCohesion;
        cm.oppStats.drawAccuracy += activeEquip[card].oppDraw;
        cm.oppStats.guardAccuracy += activeEquip[card].oppGuard;
        cm.oppStats.takeOutAccuracy += activeEquip[card].oppTakeOut;
        cm.oppStats.sweepEndurance += activeEquip[card].oppEnduro;
        cm.oppStats.sweepStrength += activeEquip[card].oppStrength;
        cm.oppStats.sweepCohesion += activeEquip[card].oppCohesion;

    }

    public void ResetPoints(int card)
    {
        CareerManager cm = FindObjectOfType<CareerManager>();

        cm.modStats.drawAccuracy -= activeEquip[card].draw;
        cm.modStats.guardAccuracy -= activeEquip[card].guard;
        cm.modStats.takeOutAccuracy -= activeEquip[card].takeOut;
        cm.modStats.sweepEndurance -= activeEquip[card].sweepEnduro;
        cm.modStats.sweepStrength -= activeEquip[card].sweepStrength;
        cm.modStats.sweepCohesion -= activeEquip[card].sweepCohesion;
        cm.oppStats.drawAccuracy -= activeEquip[card].oppDraw;
        cm.oppStats.guardAccuracy -= activeEquip[card].oppGuard;
        cm.oppStats.takeOutAccuracy -= activeEquip[card].oppTakeOut;
        cm.oppStats.sweepEndurance -= activeEquip[card].oppEnduro;
        cm.oppStats.sweepStrength -= activeEquip[card].oppStrength;
        cm.oppStats.sweepCohesion -= activeEquip[card].oppCohesion;

    }
}
