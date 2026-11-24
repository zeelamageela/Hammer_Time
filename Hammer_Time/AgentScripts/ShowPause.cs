using UnityEngine;

// Play-mode helper: opens the pause menu with pretend training levels (in memory only) so it can be screenshotted.
public static class ShowPause
{
    public static string Run(string levels)
    {
        var parts = levels.Split(',');
        var owned = new int[5];
        for (int i = 0; i < 5 && i < parts.Length; i++) owned[i] = int.Parse(parts[i]);
        if (levels != "none") TrainingAids.Use(owned);
        var pm = Object.FindFirstObjectByType<PauseMenu>();
        if (pm == null) return "no PauseMenu";
        pm.Pause();
        return "paused; gated=" + TrainingAids.Gated;
    }

    public static string Tester()
    {
        var pm = Object.FindFirstObjectByType<PauseMenu>();
        if (pm == null) return "no PauseMenu";
        pm.ToggleTester();
        return "tester toggled";
    }
}
