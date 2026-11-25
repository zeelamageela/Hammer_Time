using System.Text;
using UnityEngine;
using UnityEngine.UI;

public static class InspectSliders
{
    public static string Run()
    {
        var sb = new StringBuilder();
        foreach (var s in Object.FindObjectsByType<Slider>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string path = s.name; var t = s.transform.parent;
            while (t != null) { path = t.name + "/" + path; t = t.parent; }
            if (!path.Contains("PauseMenu")) continue;
            var label = s.GetComponentInChildren<Text>(true);
            sb.AppendLine(path + " min=" + s.minValue + " max=" + s.maxValue + " whole=" + s.wholeNumbers + " value=" + s.value + " label='" + (label != null ? label.text : "-") + "' persistentEvents=" + s.onValueChanged.GetPersistentEventCount());
        }
        foreach (var b in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string path = b.name; var t = b.transform.parent;
            while (t != null) { path = t.name + "/" + path; t = t.parent; }
            if (!path.Contains("PauseMenu")) continue;
            sb.Append(path + " clicks=" + b.onClick.GetPersistentEventCount());
            for (int i = 0; i < b.onClick.GetPersistentEventCount(); i++) sb.Append(" [" + b.onClick.GetPersistentTarget(i) + "." + b.onClick.GetPersistentMethodName(i) + "]");
            sb.AppendLine();
        }
        var pm = Object.FindFirstObjectByType<PauseMenu>(FindObjectsInactive.Include);
        sb.AppendLine("PauseMenu.pauseMenuUI=" + (pm.pauseMenuUI != null ? pm.pauseMenuUI.name : "null") + " volumeSlider=" + (pm.volumeSlider != null ? pm.volumeSlider.name : "null") + " debugToggle=" + (pm.debugToggle != null ? pm.debugToggle.name : "null"));
        return sb.ToString();
    }
}
