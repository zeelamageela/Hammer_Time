using UnityEngine;

// Edit-mode helper (does NOT touch the career save): shows or hides the Training panel and fills its rows so it can be screenshotted.
// Always call ShowTraining.Hide afterwards so the scene is left as it was.
public static class ShowTraining
{
    public static string Show()
    {
        var mgr = Object.FindFirstObjectByType<TrainingManager>(FindObjectsInactive.Include);
        if (mgr == null) return "no TrainingManager";
        var sel = Object.FindFirstObjectByType<TournySelector>();
        // hub buttons, as the real Expand leaves them: only the panel and the stat bars
        mgr.panel.SetActive(true);
        mgr.Refresh();
        return "panel shown";
    }

    public static string Hide()
    {
        var mgr = Object.FindFirstObjectByType<TrainingManager>(FindObjectsInactive.Include);
        if (mgr == null) return "no TrainingManager";
        mgr.panel.SetActive(false);
        return "panel hidden";
    }
}
