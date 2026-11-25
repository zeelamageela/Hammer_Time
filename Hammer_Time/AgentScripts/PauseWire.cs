using System.Text;
using UnityEngine;
using UnityEngine.UI;

// Play-mode check: opens the pause menu as a player would (no pretend training) and lists what is shown and what each label says.
public static class PauseWire
{
    public static string Natural()
    {
        var sb = new StringBuilder();
        var pm = Object.FindFirstObjectByType<PauseMenu>();
        var cm = Object.FindFirstObjectByType<CareerManager>();
        sb.AppendLine("career=" + (cm != null) + " trainingLevels=" + (cm != null && cm.trainingLevels != null ? string.Join(",", cm.trainingLevels) : "null") + " gated=" + TrainingAids.Gated +
                      " tutorial=" + (TutorialGameManager.Instance != null && TutorialGameManager.Instance.isTutorialGame) + " DevOptions=" + DevOptions.Enabled);
        pm.Pause();
        var list = pm.pauseMenuUI.transform.Find("PlayerOptions");
        foreach (Transform c in list)
        {
            var txt = c.GetComponentInChildren<Text>(true);
            sb.AppendLine("  " + c.name + " active=" + c.gameObject.activeSelf + " text='" + (txt != null ? txt.text : "-") + "'");
        }
        pm.Resume();
        return sb.ToString();
    }
}
