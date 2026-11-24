using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Prints how the Arena_Selector menu buttons are wired (persistent onClick listeners), so the Training menu can copy the pattern.
public static class InspectMenu
{
    static GameObject Find(string path)
    {
        string[] parts = path.Split('/');
        var root = GameObject.Find(parts[0]);
        if (root == null) return null;
        Transform t = root.transform;
        for (int i = 1; i < parts.Length && t != null; i++) t = t.Find(parts[i]);
        return t != null ? t.gameObject : null;
    }

    static void Dump(StringBuilder sb, string path)
    {
        var go = Find(path);
        if (go == null) { sb.AppendLine(path + ": NOT FOUND"); return; }
        var rt = go.GetComponent<RectTransform>();
        sb.AppendLine(path + "  active=" + go.activeSelf + (rt != null ? "  anchored=" + rt.anchoredPosition + " size=" + rt.sizeDelta + " anchors=" + rt.anchorMin + "-" + rt.anchorMax : ""));
        var btn = go.GetComponent<Button>();
        if (btn != null)
        {
            for (int i = 0; i < btn.onClick.GetPersistentEventCount(); i++)
                sb.AppendLine("    onClick[" + i + "] -> " + btn.onClick.GetPersistentTarget(i) + "." + btn.onClick.GetPersistentMethodName(i));
        }
        var txt = go.GetComponentInChildren<Text>(true);
        if (txt != null) sb.AppendLine("    text='" + txt.text + "' font=" + (txt.font != null ? txt.font.name : "null") + " size=" + txt.fontSize);
    }

    public static string Run()
    {
        var sb = new StringBuilder();
        foreach (var p in new[] {
            "Canvas/Menu2/EquipButton", "Canvas/Menu2/StandingsButton", "Canvas/Menu2/TeamButton", "Canvas/Menu2/SponsorButton",
            "Canvas/Menu2/PlayerPanel", "Canvas/Menu2/PlayerPanel/Equipment", "Canvas/Menu2/PlayerPanel/Equipment/ItemMenu",
            "Canvas/Menu2/PlayerPanel/Equipment/ItemMenu/Broom", "Canvas/Menu2/PlayerPanel/Equipment/BuyItem/Confirm",
            "Canvas/Menu2/PlayerPanel/Equipment/BuyItem/Confirm/YesButton", "Canvas/Menu2/PlayerPanel/Equipment/BuyItem/Confirm/NoButton",
            "Canvas/Menu2/Standings", "Canvas/Menu2/Standings/BackButton (1)", "Canvas/Menu2/PlayerPanel/PanelBGParent", "Canvas/Menu2/PlayerPanel/PanelFGParent" })
            Dump(sb, p);

        var pp = Find("Canvas/Menu2/PlayerPanel");
        if (pp != null)
            foreach (Transform c in pp.transform)
            {
                sb.AppendLine("PlayerPanel child: " + c.name + " children=" + c.childCount);
                foreach (Transform cc in c) sb.AppendLine("      " + cc.name + (cc.GetComponent<Button>() != null ? " [Button]" : ""));
            }

        var em = Object.FindFirstObjectByType<EquipmentManager>();
        sb.AppendLine("EquipmentManager mainMenu=" + (em != null && em.mainMenu != null ? em.mainMenu.name : "null") + " title=" + (em != null && em.title != null ? em.title.name : "null"));
        var tms = Object.FindObjectsByType<EquipmentManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var m in tms) sb.AppendLine("  EquipmentManager on " + m.gameObject.name + " equipUIs=" + (m.equipUIs != null ? m.equipUIs.Length : -1));
        return sb.ToString();
    }
}
