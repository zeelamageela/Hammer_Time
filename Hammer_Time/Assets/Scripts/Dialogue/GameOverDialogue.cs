using UnityEngine;

/// <summary>
/// Plays the coach's game-over dialogue on the main menu. Lives in SplashMenu next to the DialogueCanvas prefab.
/// Pick the ending by index (the scenario number passed to StorylineManager.EndOfGame); anything without its own
/// asset falls back to <see cref="fallback"/>, so a missing entry never leaves the player with nothing.
/// </summary>
public class GameOverDialogue : MonoBehaviour
{
    [Tooltip("Index = ending scenario (5 = bankrupt / nothing affordable). Leave an entry empty to use the fallback.")]
    public DialogueData[] endings;

    public DialogueData fallback;

    // Keeps the list as long as the number of endings EndingPicker can produce, without losing what's already assigned.
    void OnValidate()
    {
        if (endings != null && endings.Length >= EndingPicker.EndingCount) return;
        System.Array.Resize(ref endings, EndingPicker.EndingCount);
    }

    // CareerManager survives scene loads, so when the game sends the player back here after a game over it is
    // still around with the ending recorded. A fresh launch has no CareerManager yet, so nothing plays.
    void Start()
    {
        CareerManager cm = FindAnyObjectByType<CareerManager>();
        if (cm == null || !cm.gameOver || cm.endScenario < 0) return;
        Show(cm);
    }

    void Show(CareerManager cm)
    {
        DialogueData data = Pick(endings, fallback, cm.endScenario);
        if (data == null)
        {
            Debug.LogWarning("[GameOverDialogue] No dialogue assigned for ending " + cm.endScenario);
            return;
        }

        if (DialogueController.Instance == null)
        {
            Debug.LogWarning("[GameOverDialogue] No DialogueController in this scene, skipping.");
            return;
        }

        // One-shot: Start runs again whenever the menu reloads, and the ending shouldn't replay each time.
        cm.endScenario = -1;
        DialogueController.Instance.Show(data);
    }

    public static DialogueData Pick(DialogueData[] endings, DialogueData fallback, int scenario)
    {
        if (endings != null && scenario >= 0 && scenario < endings.Length && endings[scenario] != null)
            return endings[scenario];
        return fallback;
    }
}
