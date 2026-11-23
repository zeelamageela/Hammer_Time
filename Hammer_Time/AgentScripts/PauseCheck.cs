using UnityEngine;
using UnityEngine.UI;
using System.Text;
public static class PauseCheck
{
    public static string Start()
    {
        var pm = Object.FindFirstObjectByType<PauseMenu>();
        string r = "profile at start: law=" + RockPhysicsSettings.Current.law + " power=" + RockPhysicsSettings.Current.stopPower + " choice=" + RockPhysicsBootstrap.Current + "\n";
        if (pm == null) return r + "no PauseMenu in this scene";
        pm.Pause();
        foreach (var n in new[] { "PhysicsPaceButton", "StopCurveButton" })
        {
            var b = pm.pauseMenuUI.transform.Find(n);
            r += n + " exists=" + (b != null) + (b != null ? " label='" + b.GetComponentInChildren<Text>().text + "'" : "") + "\n";
        }
        return r;
    }
    public static string TapStop()
    {
        var pm = Object.FindFirstObjectByType<PauseMenu>();
        var b = pm.pauseMenuUI.transform.Find("StopCurveButton");
        b.GetComponent<Button>().onClick.Invoke();
        return "after tap: '" + b.GetComponentInChildren<Text>().text + "' law=" + RockPhysicsSettings.Current.law + " power=" + RockPhysicsSettings.Current.stopPower.ToString("F2") + " strength=" + RockPhysicsSettings.Current.stopStrength.ToString("F4");
    }
    public static string End()
    {
        var pm = Object.FindFirstObjectByType<PauseMenu>(); if (pm != null) pm.Resume();
        PlayerPrefs.DeleteKey("PhysicsProfile"); PlayerPrefs.DeleteKey("StopPower");
        return "resumed, prefs cleared";
    }
}
