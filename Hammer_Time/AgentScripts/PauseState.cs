using System.Text;
using UnityEngine;
using UnityEngine.UI;

public static class PauseState
{
    public static string CameraCanvas()
    {
        var pm = Object.FindFirstObjectByType<PauseMenu>();
        var canvas = pm.pauseMenuUI.GetComponentInParent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        canvas.planeDistance = 1f;
        return "canvas now " + canvas.renderMode + " camera=" + (canvas.worldCamera != null ? canvas.worldCamera.name : "null");
    }

    public static string Run()
    {
        var pm = Object.FindFirstObjectByType<PauseMenu>();
        if (pm == null) return "no PauseMenu";
        var sb = new StringBuilder();
        var ui = pm.pauseMenuUI;
        sb.AppendLine("pauseMenuUI=" + (ui != null ? ui.name : "null") + " activeSelf=" + (ui != null && ui.activeSelf) + " timeScale=" + Time.timeScale);
        return sb.ToString();
    }
}
