using System.Text;
using UnityEngine;
using UnityEngine.UI;

public static class InspectRow
{
    static void Dump(StringBuilder sb, Transform t)
    {
        var rt = (RectTransform)t;
        var tx = t.GetComponent<Text>();
        sb.AppendLine(t.parent.name + "/" + t.name + " anchors=" + rt.anchorMin + rt.anchorMax + " pivot=" + rt.pivot + " pos=" + rt.anchoredPosition + " size=" + rt.sizeDelta +
            (tx != null ? " fontSize=" + tx.fontSize + " best=" + tx.resizeTextForBestFit + " min=" + tx.resizeTextMinSize + " max=" + tx.resizeTextMaxSize + " align=" + tx.alignment + " wrap=" + tx.horizontalOverflow + "/" + tx.verticalOverflow : ""));
    }

    public static string Run(string rowPath)
    {
        var sb = new StringBuilder();
        var row = GameObject.Find("Canvas").transform.Find(rowPath);
        if (row == null) return "not found";
        Dump(sb, row);
        foreach (Transform c in row) Dump(sb, c);
        return sb.ToString();
    }
}
