using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

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
    public GameObject inventoryMenu;
    public GameObject sellMenu;
    public GameObject confirmMenu;

    public Equipment[] activeEquip;

    public Equipment[] handles;
    public Equipment[] heads;
    public Equipment[] footwear;
    public Equipment[] apparel;

    public List<Inventory_List> inventory;
    //public Inventory_List inventory;

    public Equipment[] invDisplay;
    public Equipment[] forSale;

    public EquipUI[] equipUIs;
    public EquipUI[] forSaleUIs;
    public EquipUI[] inventoryUIs;
    public EquipUI forSaleHeader;
    public EquipUI inventoryHeader;

    public Text confirmText;
    public Text sellText;

    Equipment origEquip;
    Equipment newEquip;
    public Equipment blank;

    // Start is called before the first frame update
    void Start()
    {
        //SetInventory();
        //MainMenu();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetInventory()
    {

        CareerManager cm = FindObjectOfType<CareerManager>();

        if (cm.activeEquipID == null)
        {
            cm.activeEquipID = new int[activeEquip.Length];
            for (int i = 0; i < activeEquip.Length; i++)
            {
                cm.activeEquipID[i] = activeEquip[i].id;
            }
        }
        else
        {
            for (int i = 0; i < cm.activeEquipID.Length; i++)
            {
                if (cm.activeEquipID[i] < 30)
                {
                    for (int j = 0; j < handles.Length; j++)
                    {
                        if (handles[j].id == cm.activeEquipID[i])
                            handles[j].active = true;
                        else
                            handles[j].active = false;
                    }
                }
                if (cm.activeEquipID[i] >= 30 && cm.activeEquipID[i] < 60)
                {
                    for (int j = 0; j < heads.Length; j++)
                    {
                        if (heads[j].id == cm.activeEquipID[i])
                            heads[j].active = true;
                        else
                            heads[j].active = false;
                    }
                }

                if (cm.activeEquipID[i] >= 60 && cm.activeEquipID[i] < 80)
                {
                    for (int j = 0; j < footwear.Length; j++)
                    {
                        if (footwear[j].id == cm.activeEquipID[i])
                            footwear[j].active = true;
                        else
                            footwear[j].active = false;
                    }
                }

                if (cm.activeEquipID[i] >= 80)
                {
                    for (int j = 0; j < apparel.Length; j++)
                    {
                        if (apparel[j].id == cm.activeEquipID[i])
                            apparel[j].active = true;
                        else
                            apparel[j].active = false;
                    }
                }
            }
        }

        if (cm.inventoryID == null)
        {
            cm.inventoryID = new int[inventory.Count];
            for (int i = 0; i < inventory.Count; i++)
            {
                cm.inventoryID[i] = inventory[i].equipment.id;
            }
        }
        else
        {
            for (int i = 0; i < cm.inventoryID.Length; i++)
            {
                for (int j = 0; j < handles.Length; j++)
                {
                    if (handles[j].id == cm.inventoryID[i])
                        handles[j].owned = true;
                }
                for (int j = 0; j < heads.Length; j++)
                {
                    if (heads[j].id == cm.inventoryID[i])
                        heads[j].owned = true;
                }
                for (int j = 0; j < footwear.Length; j++)
                {
                    if (footwear[j].id == cm.inventoryID[i])
                        footwear[j].owned = true;
                }
                for (int j = 0; j < apparel.Length; j++)
                {
                    if (apparel[j].id == cm.inventoryID[i])
                        apparel[j].owned = true;
                }
            }
        }

        for (int i = 0; i < handles.Length; i++)
        {
            if (handles[i].owned)
            {
                inventory.Add(new Inventory_List(handles[i]));
            }
        }
        for (int i = 0; i < heads.Length; i++)
        {
            if (heads[i].owned)
            {
                inventory.Add(new Inventory_List(heads[i]));
            }
        }
        for (int i = 0; i < footwear.Length; i++)
        {
            if (footwear[i].owned)
            {
                inventory.Add(new Inventory_List(footwear[i]));
            }
        }
        for (int i = 0; i < apparel.Length; i++)
        {
            if (apparel[i].owned)
            {
                inventory.Add(new Inventory_List(apparel[i]));
            }
        }

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].equipment.active)
            {
                if (inventory[i].equipment.handle)
                    activeEquip[0] = inventory[i].equipment;
                if (inventory[i].equipment.head)
                    activeEquip[1] = inventory[i].equipment;
                if (inventory[i].equipment.footwear)
                    activeEquip[2] = inventory[i].equipment;
                if (inventory[i].equipment.apparel)
                    activeEquip[3] = inventory[i].equipment;
            }
        }

        //inventory.Sort();

        cm.inventoryID = new int[inventory.Count];
        for (int i = 0; i < inventory.Count; i++)
        {
            cm.inventoryID[i] = inventory[i].equipment.id;
        }

        cm.activeEquipID = new int[activeEquip.Length];
        for (int i = 0; i < activeEquip.Length; i++)
        {
            cm.activeEquipID[i] = activeEquip[i].id;
        }

        for (int i = 0; i < activeEquip.Length; i++)
        {
            SetPoints(activeEquip[i]);
        }
    }

    public void MainMenu()
    {
        mainMenu.SetActive(true);
        buyMenu.SetActive(false);
        confirmMenu.SetActive(false);
        inventoryMenu.SetActive(false);

        for (int i = 0; i < equipUIs.Length; i++)
        {
            equipUIs[i].name.text = activeEquip[i].name;
            equipUIs[i].cost.text = "$" + activeEquip[i].cost.ToString("n0");
            equipUIs[i].text.text = activeEquip[i].text;
            equipUIs[i].image.sprite = activeEquip[i].img;
        }
    }

    public void InventoryMenu(int n)
    {
        mainMenu.SetActive(false);
        buyMenu.SetActive(false);
        confirmMenu.SetActive(false);
        inventoryMenu.SetActive(true);

        //Equipment[] tempEquip = new Equipment[invDisplay.Length];
        bool[] tempBool = new bool[4];

        for (int i = 0; i < tempBool.Length; i++)
        {
            if (i == n)
                tempBool[i] = true;
            else
                tempBool[i] = false;
        }

        invDisplay = new Equipment[4];
        newEquip = null;

        List<Equipment> tempEquip = new List<Equipment>();

        for (int i = 0; i < inventory.Count; i++)
        {

            if (!inventory[i].equipment.active)
            {
                if (n == 0 && inventory[i].equipment.handle)
                {
                    tempEquip.Add(inventory[i].equipment);
                }
                if (n == 1 && inventory[i].equipment.head)
                {
                    tempEquip.Add(inventory[i].equipment);
                }
                if (n == 2 && inventory[i].equipment.footwear)
                {
                    tempEquip.Add(inventory[i].equipment);
                }
                if (n == 3 && inventory[i].equipment.apparel)
                {
                    tempEquip.Add(inventory[i].equipment);
                }
            }
        }

        for (int i = 0; i < invDisplay.Length; i++)
        {
            if (i < tempEquip.Count)
                invDisplay[i] = tempEquip[i];
            else
                invDisplay[i] = blank;
        }
        origEquip = activeEquip[n];
        //ResetPoints(origEquip);

        inventoryHeader.name.text = activeEquip[n].name;
        inventoryHeader.cost.text = "$" + activeEquip[n].cost.ToString("n0");
        inventoryHeader.text.text = activeEquip[n].text;
        inventoryHeader.image.sprite = activeEquip[n].img;

        for (int i = 0; i < inventoryUIs.Length; i++)
        {
            inventoryUIs[i].name.text = invDisplay[i].name;
            inventoryUIs[i].name.transform.GetComponentInParent<Button>().interactable = true;

            inventoryUIs[i].cost.text = "$" + invDisplay[i].cost.ToString("n0");
            inventoryUIs[i].image.sprite = invDisplay[i].img;
        }

        if (newEquip != null)
            Debug.Log("newEquip is " + newEquip.name + " - origEquip is " + origEquip.name);
    }

    public void InventorySwitch(int n)
    {
        Debug.Log("n is " + n);

        if (invDisplay[n].owned)
        {
            ResetPoints(origEquip);
            if (newEquip != null)
                origEquip = newEquip;
            newEquip = invDisplay[n];
            SetPoints(newEquip);

            newEquip.active = true;
            origEquip.active = false;

            for (int i = 0; i < activeEquip.Length; i++)
            {
                if (activeEquip[i].name == origEquip.name)
                    activeEquip[i] = newEquip;
            }

            inventoryHeader.name.text = newEquip.name;
            inventoryHeader.cost.text = "$" + newEquip.cost.ToString("n0");
            inventoryHeader.text.text = newEquip.text;
            inventoryHeader.image.sprite = newEquip.img;

            invDisplay[n] = origEquip;

            for (int i = 0; i < inventoryUIs.Length; i++)
            {
                inventoryUIs[i].name.text = invDisplay[i].name;
                //inventoryUIs[i].name.transform.GetComponentInParent<Button>().interactable = true;

                inventoryUIs[i].cost.text = "$" + invDisplay[i].cost.ToString("n0");
                inventoryUIs[i].image.sprite = invDisplay[i].img;
            }
            if (newEquip != null)
                Debug.Log("newEquip is " + newEquip.name + " - origEquip is " + origEquip.name);
        }
        else
        {
            int itemType = 0;
            for (int i = 0; i < activeEquip.Length; i++)
            {
                if (activeEquip[i].name == origEquip.name)
                {
                    itemType = i;
                }

            }
            if (newEquip != null)
                Debug.Log("newEquip is " + newEquip.name + " - origEquip is " + origEquip.name);
            BuyMenu(itemType);
        }
    }

    public void BuyMenu(int n)
    {
        mainMenu.SetActive(false);
        buyMenu.SetActive(true);
        confirmMenu.SetActive(false);
        inventoryMenu.SetActive(false);

        List<Equipment> tempEquip = new List<Equipment>();

        switch (n)
        {
            case 0:
                for(int i = 0; i < handles.Length; i++)
                {
                    if (!handles[i].owned)
                        tempEquip.Add(handles[i]);
                }
                break;

            case 1:
                for (int i = 0; i < heads.Length; i++)
                {
                    if (!heads[i].owned)
                        tempEquip.Add(heads[i]);
                }
                break;

            case 2:
                for (int i = 0; i < footwear.Length; i++)
                {
                    if (!footwear[i].owned)
                        tempEquip.Add(footwear[i]);
                }
                break;

            case 3:
                for (int i = 0; i < apparel.Length; i++)
                {
                    if (!apparel[i].owned)
                        tempEquip.Add(apparel[i]);
                }
                break;

            default:
                tempEquip = null;
                break;
        }

        forSale = new Equipment[forSaleUIs.Length];

        int j = 0;
        for (int i = 0; i < forSale.Length; i++)
        {
            if (j + 1 > tempEquip.Count)
            {
                forSale[i] = blank;
            }
            else
            {
                forSale[i] = tempEquip[j];
            }
            j++;
        }

        origEquip = activeEquip[n];
        //ResetPoints(origEquip);

        forSaleHeader.name.text = activeEquip[n].name;
        forSaleHeader.cost.text = "$" + activeEquip[n].cost.ToString("n0");
        forSaleHeader.text.text = activeEquip[n].text;
        forSaleHeader.image.sprite = activeEquip[n].img;

        for (int i = 0; i < forSaleUIs.Length; i++)
        {
            if (forSale[i].owned)
            {
                forSaleUIs[i].name.text = forSale[i].name;
                //forSaleUIs[i].name.transform.GetComponentInParent<Button>().interactable = false;
            }
            else
            {
                forSaleUIs[i].name.text = forSale[i].name;
                //forSaleUIs[i].name.transform.GetComponentInParent<Button>().interactable = true;
            }
            forSaleUIs[i].cost.text = "$" + forSale[i].cost.ToString("n0");
            forSaleUIs[i].image.sprite = forSale[i].img;
        }
        if (newEquip != null)
            Debug.Log("newEquip is " + newEquip.name + " - origEquip is " + origEquip.name);
    }

    public void BuyItem(int n)
    {
        mainMenu.SetActive(false);
        buyMenu.SetActive(true);
        confirmMenu.SetActive(true);

        ResetPoints(origEquip);
        newEquip = forSale[n];
        SetPoints(newEquip);

        forSaleHeader.name.text = newEquip.name;
        forSaleHeader.cost.text = "$" + newEquip.cost.ToString("n0");
        forSaleHeader.text.text = newEquip.text;
        forSaleHeader.image.sprite = newEquip.img;

        confirmText.text = "Buy this card for $" + newEquip.cost.ToString("n0") + "?";
        if (newEquip != null)
            Debug.Log("newEquip is " + newEquip.name + " - origEquip is " + origEquip.name);
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

            CareerManager cm = FindObjectOfType<CareerManager>();
            cm.cash -= newEquip.cost;

            origEquip.active = false;
            newEquip.active = true;
            inventory.Add(new Inventory_List(newEquip));

            cm.inventoryID = new int[inventory.Count];
            for (int i = 0; i < inventory.Count; i++)
            {
                cm.inventoryID[i] = inventory[i].equipment.id;
            }

            cm.activeEquipID = new int[activeEquip.Length];
            for (int i = 0; i < activeEquip.Length; i++)
            {
                cm.activeEquipID[i] = activeEquip[i].id;
            }

            for (int i = 0; i < forSale.Length; i++)
            {
                if (forSale[i].name == newEquip.name)
                {
                    forSale[i] = blank;
                    Equipment[] tempEquip = new Equipment[1];
                    if (newEquip.handle)
                        tempEquip = handles;
                    if (newEquip.head)
                        tempEquip = heads;
                    if (newEquip.footwear)
                        tempEquip = footwear;
                    if (newEquip.apparel)
                        tempEquip = apparel;

                    for (int j = 0; j < tempEquip.Length; j++)
                    {
                        if (tempEquip[j].name == newEquip.name)
                            tempEquip[j].owned = true;
                    }
                }
            }

            if (newEquip != null)
                Debug.Log("newEquip is " + newEquip.name + " - origEquip is " + origEquip.name);
        }
        else
        {
            mainMenu.SetActive(false);
            buyMenu.SetActive(true);

            ResetPoints(newEquip);
            SetPoints(origEquip);

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
            if (newEquip != null)
                Debug.Log("newEquip is " + newEquip.name + " - origEquip is " + origEquip.name);
        }
    }

    public void GenerateItems(Equipment[]equipList, int type)
    {
        Equipment[] temp = new Equipment[20];
        if (type == 0 | type == 1)
            temp = new Equipment[30];

        for (int i = 0; i < temp.Length; i++)
        {
            temp[i].cost = Random.Range(0, 10000);
            if (temp[i].cost > 500)
                temp[i].name = "Fibreglass Handle";
            else if (temp[i].cost > 1500)
                temp[i].name = "Composite Handle";
            else if (temp[i].cost > 2500)
                temp[i].name = "Carbon Fibre Handle";
            else if (temp[i].cost > 7500)
                temp[i].name = "Exotic Carbon Fibre Handle";
            else
                temp[i].name = "Wooden Handle";
        }
    }

    public void SetPoints(Equipment equip)
    {
        CareerManager cm = FindObjectOfType<CareerManager>();

        cm.modStats.drawAccuracy += equip.draw;
        cm.modStats.guardAccuracy += equip.guard;
        cm.modStats.takeOutAccuracy += equip.takeOut;
        cm.modStats.sweepEndurance += equip.sweepEnduro;
        cm.modStats.sweepStrength += equip.sweepStrength;
        cm.modStats.sweepCohesion += equip.sweepCohesion;
        cm.oppStats.drawAccuracy += equip.oppDraw;
        cm.oppStats.guardAccuracy += equip.oppGuard;
        cm.oppStats.takeOutAccuracy += equip.oppTakeOut;
        cm.oppStats.sweepEndurance += equip.oppEnduro;
        cm.oppStats.sweepStrength += equip.oppStrength;
        cm.oppStats.sweepCohesion += equip.oppCohesion;
    }

    public void ResetPoints(Equipment equip)
    {
        CareerManager cm = FindObjectOfType<CareerManager>();

        cm.modStats.drawAccuracy -= equip.draw;
        cm.modStats.guardAccuracy -= equip.guard;
        cm.modStats.takeOutAccuracy -= equip.takeOut;
        cm.modStats.sweepEndurance -= equip.sweepEnduro;
        cm.modStats.sweepStrength -= equip.sweepStrength;
        cm.modStats.sweepCohesion -= equip.sweepCohesion;
        cm.oppStats.drawAccuracy -= equip.oppDraw;
        cm.oppStats.guardAccuracy -= equip.oppGuard;
        cm.oppStats.takeOutAccuracy -= equip.oppTakeOut;
        cm.oppStats.sweepEndurance -= equip.oppEnduro;
        cm.oppStats.sweepStrength -= equip.oppStrength;
        cm.oppStats.sweepCohesion -= equip.oppCohesion;

    }
}
