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
            forSaleUIs[i].text.text = tempEquip[i].text;
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
    }

    public void GenerateItems(Equipment[]equipList)
    {
        Equipment[] temp = new Equipment[10 * equipList.Length];


    }
}
