using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Puts the Game Pace slider in TournyGame's pause menu: the Physics Slider moves from AdvancedOptions to PlayerOptions (above the
/// Advanced button) under a "Game Pace" heading. It runs by itself once (while the scene has no Game Pace slider); after that
/// move and restyle things by hand.
/// </summary>
public static class PauseMenuPaceSlider
{
    const string ScenePath = "Assets/Scenes/TournyGame.unity";
    const float MenuY = 250f;   // raised so the taller list still fits on screen

    [InitializeOnLoadMethod]
    static void Watch()
    {
        if (Application.isBatchMode) return;
        EditorApplication.delayCall += Run;
        EditorApplication.playModeStateChanged += state => { if (state == PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall += Run; };
    }

    static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(ScenePath)) return;
        if (File.ReadAllText(ScenePath).Contains("m_Name: Game Pace Slider")) return;

        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        bool opened = !scene.isLoaded;
        if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        bool hadEdits = !opened && scene.isDirty;
        try
        {
            var menu = scene.GetRootGameObjects().Select(g => g.GetComponentInChildren<PauseMenu>(true)).FirstOrDefault(m => m != null);
            var ui = menu != null && menu.pauseMenuUI != null ? menu.pauseMenuUI.transform : null;
            var player = ui != null ? ui.Find("PlayerOptions") as RectTransform : null;
            var advanced = ui != null ? ui.Find("AdvancedOptions") : null;
            if (player == null || advanced == null) { Debug.LogWarning("[Game Pace] No two-panel pause menu in " + ScenePath); return; }
            if (player.Find("Game Pace Slider") != null) return;   // already there, not saved yet

            var slider = advanced.Find("Physics Slider");
            var heading = player.Find("Trajectory");
            if (slider == null || heading == null) { Debug.LogWarning("[Game Pace] Physics Slider or Trajectory heading missing in " + ScenePath); return; }

            var title = Object.Instantiate(heading.gameObject, player);
            title.name = "Game Pace";
            var text = title.GetComponent<Text>();
            if (text != null) text.text = "Game Pace";
            slider.SetParent(player, false);
            slider.name = "Game Pace Slider";

            title.transform.SetAsLastSibling();
            slider.SetAsLastSibling();
            var advancedButton = player.Find("AdvancedSettings");
            if (advancedButton != null) advancedButton.SetAsLastSibling();
            player.anchoredPosition = new Vector2(player.anchoredPosition.x, MenuY);

            EditorSceneManager.MarkSceneDirty(scene);
            if (hadEdits) Debug.Log("[Game Pace] Slider added. TournyGame had unsaved edits, so save it yourself to keep this.");
            else
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[Game Pace] Slider added and saved " + ScenePath);
            }
        }
        finally
        {
            if (opened) EditorSceneManager.CloseScene(scene, true);
        }
    }
}
