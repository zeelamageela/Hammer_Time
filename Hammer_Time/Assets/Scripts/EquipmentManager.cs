using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;
using ExitGames.Client.Photon;
using System.Linq;

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
    public Text title;

    Equipment origEquip;
    Equipment newEquip;
    public Equipment blank;
    public Sprite[] gsImgs;

    public Color buttonDisabledColor;
    public Color buttonEnabledColor;
    public TeamMenu teamMenu;
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

        // Use LoadItems if loaded from save, otherwise GenerateItems
        if (cm.loadedFromSave)
        {
            // Handles
            if (handles == null)
            {
                Debug.LogError("Handles array is null when loading from save!");
                return; // or handle error
            }
            handles = LoadItems(handles, "handle");

            // Heads
            if (heads == null)
            {
                Debug.LogError("Heads array is null when loading from save!");
                return;
            }
            heads = LoadItems(heads, "head");

            // Footwear
            if (footwear == null)
            {
                Debug.LogError("Footwear array is null when loading from save!");
                return;
            }
            footwear = LoadItems(footwear, "footwear");

            // Apparel
            if (apparel == null)
            {
                Debug.LogError("Apparel array is null when loading from save!");
                return;
            }
            apparel = LoadItems(apparel, "apparel");

            //Debug.Log("Set Inventory - Loaded items from save: " + cm.activeEquipID.Length);
        }
        else
        {
            handles = GenerateItems(handles, "handle");
            heads = GenerateItems(heads, "head");
            footwear = GenerateItems(footwear, "footwear");
            apparel = GenerateItems(apparel, "apparel");
        }

        // After handles, heads, footwear, apparel are set
        if (activeEquip == null || activeEquip.Length != 4)
            activeEquip = new Equipment[4];

        if (inventory == null)
            inventory = new List<Inventory_List>();
        else
            inventory.Clear();

        foreach (var arr in new[] { handles, heads, footwear, apparel })
        {
            foreach (var eq in arr)
            {
                if (eq != null && eq.owned)
                    inventory.Add(new Inventory_List(eq));
            }
        }

        if (cm.inventoryID != null)
        {
            foreach (var arr in new[] { handles, heads, footwear, apparel })
            {
                foreach (var eq in arr)
                {
                    if (eq != null)
                        eq.owned = cm.inventoryID.Contains(eq.id);
                }
            }
        }

        // If this is the first run (no save), set activeEquip to the first owned of each type
        if (cm.activeEquipID == null)
        {
            Debug.Log("Set Inventory - cm.activeEquipID is null");
            // 0: handle, 1: head, 2: footwear, 3: apparel
            activeEquip[0] = handles.FirstOrDefault(e => e != null && e.owned) ?? handles[0];
            activeEquip[1] = heads.FirstOrDefault(e => e != null && e.owned) ?? heads[0];
            activeEquip[2] = footwear.FirstOrDefault(e => e != null && e.owned) ?? footwear[0];
            activeEquip[3] = apparel.FirstOrDefault(e => e != null && e.owned) ?? apparel[0];

            for (int i = 0; i < activeEquip.Length; i++)
                if (activeEquip[i] != null)
                    activeEquip[i].active = true;

            cm.activeEquipID = new int[activeEquip.Length];
            for (int i = 0; i < activeEquip.Length; i++)
                cm.activeEquipID[i] = activeEquip[i]?.id ?? -1;
            Debug.Log("Set Inventory - cm.activeEquip is ");
        }

        LoadActiveEquipFromCareerManager(cm);

        cm.inventoryID = inventory.Select(inv => inv.equipment.id).ToArray();
        cm.activeEquipID = activeEquip.Select(eq => eq.id).ToArray();
        
        foreach (var eq in activeEquip)
            SetPoints(eq);
        //cm.SaveCareer();
    }

    public void LoadActiveEquipFromCareerManager(CareerManager cm)
    {
        if (cm == null || cm.activeEquipID == null)
            return;

        // Build a lookup for all equipment by ID, filtering out duplicates
        var allEquipment = new List<Equipment>();
        if (handles != null) allEquipment.AddRange(handles.Where(e => e != null));
        if (heads != null) allEquipment.AddRange(heads.Where(e => e != null));
        if (footwear != null) allEquipment.AddRange(footwear.Where(e => e != null));
        if (apparel != null) allEquipment.AddRange(apparel.Where(e => e != null));

        var equipDict = allEquipment
            .GroupBy(e => e.id)
            .Select(g => g.First())
            .ToDictionary(e => e.id, e => e);

        var duplicateIds = allEquipment.GroupBy(e => e.id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateIds.Count > 0)
            Debug.LogWarning("Duplicate Equipment IDs found: " + string.Join(", ", duplicateIds));

        // Assign activeEquip from IDs
        activeEquip = new Equipment[cm.activeEquipID.Length];
        for (int i = 0; i < cm.activeEquipID.Length; i++)
        {
            if (equipDict.TryGetValue(cm.activeEquipID[i], out var eq))
                activeEquip[i] = eq;
            else
                activeEquip[i] = null;
        }
        cm.SaveCareer();
    }

    public void MainMenu()
    {
        title.text = "Equipment";
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
            equipUIs[i].image.color = activeEquip[i].color;
        }
    }

    public void InventoryMenu(int n)
    {
        title.text = "Inventory";
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
        inventoryHeader.image.color = activeEquip[n].color;

        for (int i = 0; i < inventoryUIs.Length; i++)
        {
            inventoryUIs[i].name.text = invDisplay[i].name;
            inventoryUIs[i].name.transform.GetComponentInParent<Button>().interactable = true;

            inventoryUIs[i].cost.text = "$" + invDisplay[i].cost.ToString("n0");
            inventoryUIs[i].image.sprite = invDisplay[i].img;
            inventoryUIs[i].image.color = invDisplay[i].color;
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
            inventoryHeader.image.color = newEquip.color;

            invDisplay[n] = origEquip;

            for (int i = 0; i < inventoryUIs.Length; i++)
            {
                inventoryUIs[i].name.text = invDisplay[i].name;
                //inventoryUIs[i].name.transform.GetComponentInParent<Button>().interactable = true;

                inventoryUIs[i].cost.text = "$" + invDisplay[i].cost.ToString("n0");
                inventoryUIs[i].image.sprite = invDisplay[i].img;
                inventoryUIs[i].image.color = invDisplay[i].color;
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
        title.text = "Buy";
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
        forSaleHeader.image.color = activeEquip[n].color;

        CareerManager cm = FindObjectOfType<CareerManager>();

        for (int i = 0; i < forSaleUIs.Length; i++)
        {
            if (cm.cash < forSale[i].cost)
            {
                if (forSaleUIs[i].name.transform.parent.GetComponent<Button>().interactable)
                {
                    forSaleUIs[i].name.rectTransform.anchoredPosition -= new Vector2(15f, 15f);
                    forSaleUIs[i].cost.rectTransform.anchoredPosition -= new Vector2(15f, 15f);
                    forSaleUIs[i].image.transform.parent.transform.localPosition -= new Vector3(35f, 35f, 0);
                }
                forSaleUIs[i].name.transform.GetComponentInParent<Button>().interactable = false;
                forSaleUIs[i].name.transform.GetComponentInParent<Image>().color = buttonDisabledColor;
            }
            else
            {
                if (!forSaleUIs[i].name.transform.parent.GetComponent<Button>().interactable)
                {
                    forSaleUIs[i].name.rectTransform.anchoredPosition += new Vector2(15f, 15f);
                    forSaleUIs[i].cost.rectTransform.anchoredPosition += new Vector2(15f, 15f);
                    forSaleUIs[i].image.transform.parent.transform.localPosition += new Vector3(35f, 35f, 0);
                }
                forSaleUIs[i].name.transform.GetComponentInParent<Button>().interactable = true;
                forSaleUIs[i].name.transform.GetComponentInParent<Image>().color = buttonEnabledColor;
            }
            forSaleUIs[i].name.text = forSale[i].name;
            forSaleUIs[i].cost.text = "$" + forSale[i].cost.ToString("n0");
            forSaleUIs[i].image.sprite = forSale[i].img;
            forSaleUIs[i].image.color = forSale[i].color;
        }
        if (newEquip != null)
            Debug.Log("newEquip is " + newEquip.name + " - origEquip is " + origEquip.name);
    }

    public void BuyItem(int n)
    {
        title.text = "Confirm";
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
        forSaleHeader.image.color = newEquip.color;

        confirmText.text = "Buy this card for $" + newEquip.cost.ToString("n0") + "?";
        if (newEquip != null)
            Debug.Log("newEquip is " + newEquip.name + " - origEquip is " + origEquip.name);
    }

    public void Confirm(bool buy)
    {
        confirmMenu.SetActive(false);

        if (buy)
        {
            title.text = "Inventory";
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
                equipUIs[i].image.color = activeEquip[i].color;
            }

            CareerManager cm = FindObjectOfType<CareerManager>();
            cm.cash -= newEquip.cost;
            teamMenu.CashDeltaText(-newEquip.cost);
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
            title.text = "Buy";
            mainMenu.SetActive(false);
            buyMenu.SetActive(true);

            ResetPoints(newEquip);
            SetPoints(origEquip);

            forSaleHeader.name.text = origEquip.name;
            forSaleHeader.cost.text = "$" + origEquip.cost.ToString("n0");
            forSaleHeader.text.text = origEquip.text;
            forSaleHeader.image.sprite = origEquip.img;
            forSaleHeader.image.color = origEquip.color;

            for (int i = 0; i < equipUIs.Length; i++)
            {
                equipUIs[i].name.text = activeEquip[i].name;
                equipUIs[i].cost.text = "$" + activeEquip[i].cost.ToString("n0");
                equipUIs[i].text.text = activeEquip[i].text;
                equipUIs[i].image.sprite = activeEquip[i].img;
                equipUIs[i].image.color = activeEquip[i].color;
                if (activeEquip[i].handle)
                    equipUIs[i].image.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 12f);
                if (activeEquip[i].head)
                    equipUIs[i].image.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 12f);
                if (activeEquip[i].footwear)
                    equipUIs[i].image.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
                if (activeEquip[i].apparel)
                    equipUIs[i].image.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -36f);
            }
            if (newEquip != null)
                Debug.Log("newEquip is " + newEquip.name + " - origEquip is " + origEquip.name);
        }
    }

    public Equipment[] GenerateItems(Equipment[]equipList, string type)
    {
        Equipment[] temp = new Equipment[30];

        switch (type)
        {
            case "handle":
                {
                    temp = new Equipment[30];
                    for (int i = 0; i < temp.Length; i++)
                    {
                        temp[i] = new Equipment();
                        temp[i].cost = Random.Range(0, 10000);
                        temp[i].id = i;
                        int[] cats = DistributePoints(temp[i].cost, 3);

                        temp[i].stats[3] = cats[0];
                        temp[i].stats[4] = cats[1];
                        temp[i].stats[5] = cats[2];

                        temp[i].handle = true;

                        if (temp[i].cost < 500)
                        {
                            temp[i].name = "Wooden Handle";
                            temp[i].img = gsImgs[0];
                            temp[i].color = new Color(0f, 1f, 1f, 1f);
                        }
                        else if (temp[i].cost < 1500)
                        {
                            temp[i].name = "Fibreglass Handle";
                            temp[i].img = gsImgs[1];
                            temp[i].color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        }
                        else if (temp[i].cost < 3500)
                        {
                            temp[i].name = "Composite Handle";
                            temp[i].img = gsImgs[2];
                            temp[i].color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        }
                        else if (temp[i].cost < 7500)
                        {
                            temp[i].name = "Carbon Fibre Handle";
                            temp[i].img = gsImgs[3];
                            temp[i].color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        }
                        else
                        {
                            temp[i].name = "Exotic Carbon Fibre Handle";
                            temp[i].img = gsImgs[4];
                            temp[i].color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        }
                    }
                    break;
                }
            case "head":
                {
                    temp = new Equipment[30];
                    for (int i = 0; i < temp.Length; i++)
                    {
                        temp[i] = new Equipment();
                        temp[i].cost = Random.Range(0f, 10000f);
                        temp[i].id = i + 30;
                        int[] cats = DistributePoints(temp[i].cost, 3);

                        temp[i].stats[3] = cats[0];
                        temp[i].stats[4] = cats[1];
                        temp[i].stats[5] = cats[2];

                        temp[i].head = true;

                        if (i == 0)
                            temp[i] = equipList[i];

                        if (temp[i].cost < 500)
                        {
                            temp[i].name = "Basic Bristled Head";
                            temp[i].img = gsImgs[5];
                            temp[i].color = new Color(0f, 1f, 1f, 1f);
                        }
                        else if (temp[i].cost < 1500)
                        {
                            temp[i].name = "Fabric Head";
                            temp[i].img = gsImgs[6];
                            temp[i].color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        }
                        else if (temp[i].cost < 3500)
                        {
                            temp[i].name = "Advanced Fabric Head";
                            temp[i].img = gsImgs[7];
                            temp[i].color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        }
                        else if (temp[i].cost < 7500)
                        {
                            temp[i].name = "Premium Fabric Head";
                            temp[i].img = gsImgs[7];
                            temp[i].color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        }
                        else
                        {
                            temp[i].name = "Exotic Fabric Head";
                            temp[i].img = gsImgs[8];
                            temp[i].color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        }
                    }
                    break;
                }
            case "footwear":
                {
                    temp = new Equipment[20];
                    for (int i = 0; i < temp.Length; i++)
                    {
                        temp[i] = new Equipment();
                        temp[i].cost = Random.Range(0, 10000);
                        temp[i].id = i + 60;
                        int[] cats = DistributePoints(temp[i].cost, 3);

                        temp[i].stats[0] = cats[0];
                        temp[i].stats[1] = cats[1];
                        temp[i].stats[2] = cats[2];

                        temp[i].footwear = true;

                        if (i == 0)
                            temp[i] = equipList[i];

                        if (temp[i].cost < 500)
                        {
                            temp[i].name = "Half Slider";
                            temp[i].img = gsImgs[9];
                            temp[i].color = new Color(0f, 1f, 1f, 1f);
                        }
                        else if (temp[i].cost < 1500)
                        {
                            temp[i].name = "Full Slider";
                            temp[i].img = gsImgs[10];
                            temp[i].color = new Color(0f, 1f, 1f, 1f);
                        }
                        else if (temp[i].cost < 3500)
                        {
                            temp[i].name = "Premium Slider";
                            temp[i].img = gsImgs[11];
                            temp[i].color = new Color(0f, 1f, 1f, 1f);
                        }
                        else if (temp[i].cost < 7500)
                        {
                            temp[i].name = "Premium Shoes";
                            temp[i].img = gsImgs[12];
                            temp[i].color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        }
                        else
                        {
                            temp[i].name = "Exotic Shoes";
                            temp[i].img = gsImgs[13];
                            temp[i].color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        }
                    }
                    break;
                }
            case "apparel":
                {
                    temp = new Equipment[20];
                    for (int i = 0; i < temp.Length; i++)
                    {
                        temp[i] = new Equipment();
                        temp[i].cost = Random.Range(0, 10000);
                        temp[i].id = i + 80;
                        int[] cats = DistributePoints(temp[i].cost / 2f, 3);

                        temp[i].stats[3] = cats[0];
                        temp[i].stats[4] = cats[1];
                        temp[i].stats[5] = cats[2];

                        cats = DistributePoints(temp[i].cost / 2f, 3);

                        temp[i].stats[0] = cats[0];
                        temp[i].stats[1] = cats[1];
                        temp[i].stats[2] = cats[2];

                        temp[i].apparel = true;

                        if (temp[i].cost < 500)
                        {
                            temp[i].name = "Basic Nylon Jersey";
                            temp[i].img = gsImgs[14];
                            temp[i].color = new Color(0f, 1f, 1f, 1f);
                        }
                        else if (temp[i].cost < 1500)
                        {
                            temp[i].name = "Cotton Jersey";
                            temp[i].img = gsImgs[15];
                            temp[i].color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        }
                        else if (temp[i].cost < 3500)
                        {
                            temp[i].name = "Poly-lycra Blended Uniform";
                            temp[i].img = gsImgs[16];
                            temp[i].color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        }
                        else if (temp[i].cost < 7500)
                        {
                            temp[i].name = "Textured Fleece Uniform";
                            temp[i].img = gsImgs[17];
                            temp[i].color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        }
                        else
                        {
                            temp[i].name = "Dri-fit Jersey";
                            temp[i].img = gsImgs[18];
                            temp[i].color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        }
                    }

                    break;
                }
        }

        temp[0] = equipList[0];

        return temp;
    }

    public Equipment[] LoadItems(Equipment[] equipList, string type)
    {
        Equipment[] temp = new Equipment[30];

        switch (type)
        {
            case "handle":
                {
                    temp = new Equipment[30]; 
                    
                    for (int i = 0; i < temp.Length; i++)
                    {
                        //Debug.Log("index is " + i);

                        if (i < equipList.Length)
                        {
                            temp[i] = equipList[i];
                        }
                        else
                        {
                            temp[i] = new Equipment();
                            temp[i].cost = Random.Range(500f, 15000f);
                        }

                        if (temp[i].stats == null || temp[i].stats.Length != 6)
                            temp[i].stats = new int[6];
                        if (temp[i].oppStats == null || temp[i].oppStats.Length != 6)
                            temp[i].oppStats = new int[6];

                        temp[i].handle = true;

                        if (temp[i].cost < 500)
                        {
                            temp[i].name = "Wooden Handle";
                            temp[i].img = gsImgs[0];
                        }
                        else if (temp[i].cost < 1500)
                        {
                            temp[i].name = "Fibreglass Handle";
                            temp[i].img = gsImgs[1];
                        }
                        else if (temp[i].cost < 3500)
                        {
                            temp[i].name = "Composite Handle";
                            temp[i].img = gsImgs[2];
                        }
                        else if (temp[i].cost < 7500)
                        {
                            temp[i].name = "Carbon Fibre Handle";
                            temp[i].img = gsImgs[3];
                        }
                        else
                        {
                            temp[i].name = "Exotic Carbon Fibre Handle";
                            temp[i].img = gsImgs[4];
                        }

                        Debug.Log("Loaded " + temp[i].name + " of cost " + temp[i].cost);
                    }
                    break;
                }
            case "head":
                {
                    temp = new Equipment[30];
                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (i < equipList.Length)
                        {
                            temp[i] = equipList[i];
                            temp[i].cost = equipList[i].cost;
                        }
                        else
                        {
                            temp[i] = new Equipment();
                            temp[i].cost = Random.Range(500f, 15000f);
                        }

                        temp[i].head = true;

                        if (temp[i].cost < 500)
                        {
                            temp[i].name = "Basic Bristled Head";
                            temp[i].img = gsImgs[5];
                        }
                        else if (temp[i].cost < 1500)
                        {
                            temp[i].name = "Fabric Head";
                            temp[i].img = gsImgs[6];
                        }
                        else if (temp[i].cost < 3500)
                        {
                            temp[i].name = "Advanced Fabric Head";
                            temp[i].img = gsImgs[7];
                        }
                        else if (temp[i].cost < 7500)
                        {
                            temp[i].name = "Premium Fabric Head";
                            temp[i].img = gsImgs[7];
                        }
                        else
                        {
                            temp[i].name = "Exotic Fabric Head";
                            temp[i].img = gsImgs[8];
                        }
                    }
                    break;
                }
            case "footwear":
                {
                    temp = new Equipment[20];
                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (i < equipList.Length)
                        {
                            temp[i] = equipList[i];
                            temp[i].cost = equipList[i].cost;
                        }
                        else
                        {
                            temp[i] = new Equipment();
                            temp[i].cost = Random.Range(500f, 15000f);
                        }

                        temp[i].footwear = true;

                        if (i == 0)
                            temp[i] = equipList[i];

                        if (temp[i].cost < 500)
                        {
                            temp[i].name = "Half Slider";
                            temp[i].img = gsImgs[9];
                        }
                        else if (temp[i].cost < 1500)
                        {
                            temp[i].name = "Full Slider";
                            temp[i].img = gsImgs[10];
                        }
                        else if (temp[i].cost < 3500)
                        {
                            temp[i].name = "Premium Slider";
                            temp[i].img = gsImgs[11];
                        }
                        else if (temp[i].cost < 7500)
                        {
                            temp[i].name = "Premium Shoes";
                            temp[i].img = gsImgs[12];
                        }
                        else
                        {
                            temp[i].name = "Exotic Shoes";
                            temp[i].img = gsImgs[13];
                        }
                    }
                    break;
                }
            case "apparel":
                {
                    temp = new Equipment[20];
                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (i < equipList.Length)
                        {
                            temp[i] = equipList[i];
                            temp[i].cost = equipList[i].cost;
                        }
                        else
                        {
                            temp[i] = new Equipment();
                            temp[i].cost = Random.Range(500f, 15000f);
                        }

                        temp[i].apparel = true;

                        if (temp[i].cost < 500)
                        {
                            temp[i].name = "Basic Nylon Jersey";
                            temp[i].img = gsImgs[14];
                            temp[i].color = new Color(0f, 1f, 1f, 1f);
                        }
                        else if (temp[i].cost < 1500)
                        {
                            temp[i].name = "Cotton Jersey";
                            temp[i].img = gsImgs[15];
                            temp[i].color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        }
                        else if (temp[i].cost < 3500)
                        {
                            temp[i].name = "Poly-lycra Blended Uniform";
                            temp[i].img = gsImgs[16];
                            temp[i].color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        }
                        else if (temp[i].cost < 7500)
                        {
                            temp[i].name = "Textured Fleece Uniform";
                            temp[i].img = gsImgs[17];
                            temp[i].color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        }
                        else
                        {
                            temp[i].name = "Dri-fit Jersey";
                            temp[i].img = gsImgs[18];
                            temp[i].color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        }
                    }

                    break;
                }
        }

        return temp;
    }

    public int[] DistributePoints(float cost, int categories = 3)
    {

        int availPts = Mathf.RoundToInt(cost / 1000f);
        int[] cats = new int[categories];

        for (int i = 0; i < cats.Length; i++)
        {
            if (i == 0)
                cats[0] = Random.Range(0, availPts);
            else if (i == 1)
                cats[1] = availPts - Random.Range(0, cats[0]);
            else if (i == 2)
                cats[2] = availPts - cats[0] - cats[1];
        }

        return cats;
        
    }

    public void SetPoints(Equipment equip)
    {
        CareerManager cm = FindObjectOfType<CareerManager>();

        cm.modStats.drawAccuracy += equip.stats[0];
        cm.modStats.guardAccuracy += equip.stats[1];
        cm.modStats.takeOutAccuracy += equip.stats[2];
        cm.modStats.sweepEndurance += equip.stats[3];
        cm.modStats.sweepStrength += equip.stats[4];
        cm.modStats.sweepCohesion += equip.stats[5];
        cm.oppStats.drawAccuracy += equip.oppStats[0];
        cm.oppStats.guardAccuracy += equip.oppStats[1];
        cm.oppStats.takeOutAccuracy += equip.oppStats[2];
        cm.oppStats.sweepEndurance += equip.oppStats[3];
        cm.oppStats.sweepStrength += equip.oppStats[4];
        cm.oppStats.sweepCohesion += equip.oppStats[5];
    }

    public void ResetPoints(Equipment equip)
    {
        CareerManager cm = FindObjectOfType<CareerManager>();

        cm.modStats.drawAccuracy -= equip.stats[0];
        cm.modStats.guardAccuracy -= equip.stats[1];
        cm.modStats.takeOutAccuracy -= equip.stats[2];
        cm.modStats.sweepEndurance -= equip.stats[3];
        cm.modStats.sweepStrength -= equip.stats[4];
        cm.modStats.sweepCohesion -= equip.stats[5];
        cm.oppStats.drawAccuracy -= equip.oppStats[0];
        cm.oppStats.guardAccuracy -= equip.oppStats[1];
        cm.oppStats.takeOutAccuracy -= equip.oppStats[2];
        cm.oppStats.sweepEndurance -= equip.oppStats[3];
        cm.oppStats.sweepStrength -= equip.oppStats[4];
        cm.oppStats.sweepCohesion -= equip.oppStats[5];

    }

    public void LoadAllEquipmentFromSave(
    int[] ids, float[] costs, float[] colorX, float[] colorY, float[] colorZ, float[] colorA,
    int[] durations,
    int[] stats0, int[] stats1, int[] stats2, int[] stats3, int[] stats4, int[] stats5,
    int[] oppStats0, int[] oppStats1, int[] oppStats2, int[] oppStats3, int[] oppStats4, int[] oppStats5,
    int handlesCount, int headsCount, int footwearCount, int apparelCount)
    {
        int idx = 0;

        handles = new Equipment[handlesCount];
        heads = new Equipment[headsCount];
        footwear = new Equipment[footwearCount];
        apparel = new Equipment[apparelCount];

        // Helper to fill an array
        void FillArray(Equipment[] arr, int typeOffset)
        {
            for (int i = 0; i < arr.Length; i++, idx++)
            {
                if (ids[idx] == -1)
                {
                    arr[i] = null;
                    continue;
                }
                Equipment eq = new Equipment();
                eq.id = ids[idx];
                eq.cost = costs[idx];
                eq.color = new Color(colorX[idx], colorY[idx], colorZ[idx], colorA[idx]);
                eq.duration = durations[idx];
                eq.stats = new int[6] { stats0[idx], stats1[idx], stats2[idx], stats3[idx], stats4[idx], stats5[idx] };
                eq.oppStats = new int[6] { oppStats0[idx], oppStats1[idx], oppStats2[idx], oppStats3[idx], oppStats4[idx], oppStats5[idx] };
                eq.handle = (typeOffset == 0);
                eq.head = (typeOffset == 1);
                eq.footwear = (typeOffset == 2);
                eq.apparel = (typeOffset == 3);
                arr[i] = eq;
            }
        }

        FillArray(handles, 0);
        FillArray(heads, 1);
        FillArray(footwear, 2);
        FillArray(apparel, 3);
    }
}
