using System.Text;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Builds the Training screen in Arena_Selector by cloning the equipment screen (PlayerPanel): same frame, same rows, same confirm box.
// Safe to re-run: it removes a previous TrainingPanel / TrainingManager first. Also turns the hub's "Standings" button into "Training".
public static class BuildTraining
{
    const string MenuPath = "Canvas/Menu2";

    static Transform FindChild(Transform root, string path)
    {
        return root == null ? null : root.Find(path);
    }

    // min/max are the offsets from the anchors' corners (left,bottom) and (right,top)
    static void Place(Text t, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var rt = (RectTransform)t.transform;
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
    }

    static void ClearClicks(Button b)
    {
        for (int i = b.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(b.onClick, i);
    }

    public static string Run()
    {
        var log = new StringBuilder();
        var canvas = GameObject.Find("Canvas");
        var menu2 = canvas != null ? canvas.transform.Find("Menu2") : null;
        if (menu2 == null) return "Canvas/Menu2 not found - open Arena_Selector first";

        var playerPanel = menu2.Find("PlayerPanel");
        if (playerPanel == null) return "PlayerPanel not found";
        var selector = Object.FindFirstObjectByType<TournySelector>();
        var teamMenu = Object.FindFirstObjectByType<TeamMenu>();

        // ---- remove a previous build ----
        var old = menu2.Find("TrainingPanel");
        if (old != null) Object.DestroyImmediate(old.gameObject);
        var oldMgr = Object.FindFirstObjectByType<TrainingManager>();
        if (oldMgr != null) Object.DestroyImmediate(oldMgr.gameObject);

        // ---- clone the equipment screen ----
        bool wasActive = playerPanel.gameObject.activeSelf;
        var clone = Object.Instantiate(playerPanel.gameObject, menu2);
        clone.name = "TrainingPanel";
        clone.transform.SetSiblingIndex(playerPanel.GetSiblingIndex() + 1);
        playerPanel.gameObject.SetActive(wasActive);

        var equip = clone.transform.Find("Equipment");
        var em = equip.GetComponent<EquipmentManager>();
        if (em != null) Object.DestroyImmediate(em);   // the clone must not become a second equipment manager

        // keep the Confirm box, drop the rest of the buy / inventory machinery
        var confirm = FindChild(equip, "BuyItem/Confirm");
        confirm.SetParent(equip, false);
        confirm.SetAsLastSibling();
        foreach (var junk in new[] { "BuyItem", "Inventory", "Skillbars", "InfoPanel", "Button_1" })
        {
            var t = equip.Find(junk);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }
        // the equipment screen's BackButton switches the ORIGINAL Equipment / XPPanel objects off; a clone of it would do the same to them
        var back = equip.Find("BackButton");
        if (back != null) Object.DestroyImmediate(back.gameObject);

        // ---- five rows from the four equipment rows ----
        var itemMenu = equip.Find("ItemMenu");
        var first = itemMenu.GetChild(0);
        while (itemMenu.childCount < TrainingCatalog.Tracks)
        {
            var extra = Object.Instantiate(first.gameObject, itemMenu);
            extra.transform.SetAsLastSibling();
        }

        // ---- the manager ----
        var mgrGO = new GameObject("TrainingManager");
        var mgr = mgrGO.AddComponent<TrainingManager>();
        mgr.panel = clone;
        mgr.teamMenu = teamMenu;
        mgr.confirmMenu = confirm.gameObject;
        mgr.confirmText = confirm.Find("Text").GetComponent<Text>();
        mgr.rows = new TrainingRowUI[TrainingCatalog.Tracks];

        for (int i = 0; i < TrainingCatalog.Tracks; i++)
        {
            var row = itemMenu.GetChild(i);
            row.name = "Track_" + i + "_" + ((TrainingTrack)i);
            var btn = row.GetComponent<Button>();
            ClearClicks(btn);
            UnityEventTools.AddIntPersistentListener(btn.onClick, mgr.OnRowTapped, i);

            var ui = new TrainingRowUI
            {
                button = btn,
                name = row.Find("Name").GetComponent<Text>(),
                cost = row.Find("Cost").GetComponent<Text>(),
                text = row.Find("Text").GetComponent<Text>(),
                image = row.Find("Panel/Image").GetComponent<Image>(),
            };
            // five rows in a frame sized for four: shorter rows
            var le = row.GetComponent<LayoutElement>();
            if (le == null) le = row.gameObject.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = 148f;
            ((RectTransform)row).sizeDelta = new Vector2(((RectTransform)row).sizeDelta.x, 148f);

            // the equipment rows position their text with fixed pixel margins that only work at 190 high: set explicit rects.
            // With the icon hidden the text starts at the left edge: name top-left, price top-right, description underneath.
            Place(ui.name, new Vector2(0f, 0.46f), new Vector2(0.72f, 1f), new Vector2(70f, 0f), new Vector2(0f, -4f));
            Place(ui.cost, new Vector2(0.72f, 0.46f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(-30f, -4f));
            Place(ui.text, new Vector2(0f, 0f), new Vector2(1f, 0.5f), new Vector2(70f, 6f), new Vector2(-30f, 0f));
            ui.name.resizeTextMaxSize = 64; ui.cost.resizeTextMaxSize = 64;
            ui.text.fontSize = 34; ui.text.verticalOverflow = VerticalWrapMode.Overflow;

            ui.image.sprite = null;
            ui.image.enabled = false;           // art slot for later: Track_i/Panel/Image
            ui.name.text = TrainingCatalog.TrackName((TrainingTrack)i);
            ui.cost.text = "$" + TrainingCatalog.LevelCost[0].ToString("n0");
            ui.text.text = TrainingCatalog.TrackBlurb((TrainingTrack)i);
            mgr.rows[i] = ui;
        }

        foreach (var name in new[] { "YesButton", "NoButton" })
        {
            var b = confirm.Find(name).GetComponent<Button>();
            ClearClicks(b);
            UnityEventTools.AddBoolPersistentListener(b.onClick, mgr.Confirm, name == "YesButton");
        }

        // the frame's title still said "Team" (copied from the original)
        var title = clone.transform.Find("PanelFGParent/Title");
        if (title != null) { var tt = title.GetComponent<Text>(); if (tt != null) tt.text = "Training"; }

        clone.SetActive(false);

        // ---- hub wiring ----
        if (selector != null)
        {
            selector.trainingManager = mgr;
            EditorUtility.SetDirty(selector);
        }
        var standingsBtn = menu2.Find("StandingsButton");
        if (standingsBtn != null)
        {
            standingsBtn.name = "TrainingButton";
            foreach (var t in standingsBtn.GetComponentsInChildren<Text>(true))
                if (t.text == "Standings") { t.text = "Training"; EditorUtility.SetDirty(t); }
        }

        EditorUtility.SetDirty(mgr);
        EditorSceneManager.MarkSceneDirty(clone.scene);
        EditorSceneManager.SaveScene(clone.scene);
        log.AppendLine("Built TrainingPanel with " + TrainingCatalog.Tracks + " rows; selector=" + (selector != null) + " teamMenu=" + (teamMenu != null) +
                       " standingsBtn=" + (standingsBtn != null));
        return log.ToString();
    }
}
