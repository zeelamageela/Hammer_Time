using System.Text;
using UnityEngine;
using UnityEngine.UI;

// Play-mode check of the two-panel pause menu: opens it, presses Advanced / Back, moves the Physics and Stop sliders, then puts the settings back.
public static class PauseWire
{
    static Transform Deep(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
        return null;
    }

    public static string Run()
    {
        var sb = new StringBuilder();
        var pm = Object.FindFirstObjectByType<PauseMenu>();
        var ui = pm.pauseMenuUI.transform;
        var player = ui.Find("PlayerOptions").gameObject;
        var adv = ui.Find("AdvancedOptions").gameObject;
        var origPhysics = RockPhysicsBootstrap.Current;
        int origStop = RockPhysicsBootstrap.StopIndex();

        pm.Pause();
        sb.AppendLine("after Pause: player=" + player.activeSelf + " advanced=" + adv.activeSelf + " (want true/false)");

        var advBtn = Deep(ui, "AdvancedSettings").GetComponent<Button>();
        sb.AppendLine("Advanced button visible=" + advBtn.gameObject.activeSelf + " label='" + advBtn.GetComponentInChildren<Text>(true).text + "'");
        advBtn.onClick.Invoke();
        sb.AppendLine("after Advanced: player=" + player.activeSelf + " advanced=" + adv.activeSelf + " paused=" + PauseMenu.GameIsPaused + " (want false/true, still paused)");

        var ps = Deep(ui, "Physics Slider").GetComponent<Slider>();
        var ss = Deep(ui, "Stop Slider").GetComponent<Slider>();
        sb.AppendLine("physics slider 0.." + ps.maxValue + " whole=" + ps.wholeNumbers + " value=" + ps.value + " label='" + ps.GetComponentInChildren<Text>(true).text + "'");
        sb.AppendLine("stop slider 0.." + ss.maxValue + " whole=" + ss.wholeNumbers + " value=" + ss.value + " label='" + ss.GetComponentInChildren<Text>(true).text + "'");

        ps.value = 2;
        sb.AppendLine("physics -> 2: Current=" + RockPhysicsBootstrap.Current + " label='" + ps.GetComponentInChildren<Text>(true).text + "'");
        ps.value = 0;
        sb.AppendLine("physics -> 0: Current=" + RockPhysicsBootstrap.Current + " label='" + ps.GetComponentInChildren<Text>(true).text + "'");
        ss.value = 3;
        sb.AppendLine("stop -> 3: " + RockPhysicsBootstrap.StopLabel() + " label='" + ss.GetComponentInChildren<Text>(true).text + "'");

        var backBtn = Deep(ui, "BackButton").GetComponent<Button>();
        backBtn.onClick.Invoke();
        sb.AppendLine("after Back: player=" + player.activeSelf + " advanced=" + adv.activeSelf + " paused=" + PauseMenu.GameIsPaused + " (want true/false, still paused)");

        // put the tester settings back as they were
        ps.value = (int)origPhysics;
        ss.value = origStop;
        sb.AppendLine("restored: " + RockPhysicsBootstrap.Current + ", " + RockPhysicsBootstrap.StopLabel());
        pm.Resume();
        return sb.ToString();
    }
}
