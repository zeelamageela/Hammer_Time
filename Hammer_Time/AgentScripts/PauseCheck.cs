using UnityEngine;
using UnityEngine.UI;
public static class PauseCheck
{
    public static string Start()
    {
        var pm = Object.FindFirstObjectByType<PauseMenu>();
        string r = "profile at start: law=" + RockPhysicsSettings.Current.law + " stretch=" + RockPhysicsSettings.Current.timeStretch + " choice=" + RockPhysicsBootstrap.Current + " devOptions=" + DevOptions.Enabled + " flickAvailable=" + GameVisualizationSettings.FlickModeAvailable + "\n";
        if (pm == null) return r + "no PauseMenu in this scene";
        pm.Pause();
        var btn = pm.pauseMenuUI.transform.Find("PhysicsPaceButton");
        r += "button exists=" + (btn != null);
        if (btn != null) r += " label='" + btn.GetComponentInChildren<Text>().text + "'";
        ScreenCapture.CaptureScreenshot("AgentScripts/pause_shot.png");
        return r;
    }
    public static string Tap()
    {
        var pm = Object.FindFirstObjectByType<PauseMenu>();
        pm.pauseMenuUI.transform.Find("PhysicsPaceButton").GetComponent<Button>().onClick.Invoke();
        var t = pm.pauseMenuUI.transform.Find("PhysicsPaceButton").GetComponentInChildren<Text>().text;
        return "after tap: '" + t + "' law=" + RockPhysicsSettings.Current.law + " stretch=" + RockPhysicsSettings.Current.timeStretch.ToString("F2") + " anyMoving=" + RockSlide.AnyRockMoving();
    }
    public static string End()
    {
        var pm = Object.FindFirstObjectByType<PauseMenu>(); if (pm != null) pm.Resume();
        PlayerPrefs.DeleteKey("PhysicsProfile");
        return "resumed";
    }
}
