using UnityEngine;

/// <summary>
/// Helper class for migrating from the old dialogue system to the new one.
/// Provides backwards compatibility while you transition.
/// </summary>
public class DialogueMigrationHelper : MonoBehaviour
{
    [Header("Old System References (for migration)")]
    public DialogueManager oldDialogueManager;
    public DialogueTrigger oldDialogueTrigger;
    
    [Header("New System Mappings")]
    [Tooltip("Map old dialogue array indices to new DialogueData assets")]
    public DialogueData[] dialogueMappings;
    
    /// <summary>
    /// Call this instead of old TriggerDialogue calls.
    /// Automatically maps old index to new DialogueData.
    /// </summary>
    public void TriggerDialogue(int index)
    {
        if (dialogueMappings == null || index < 0 || index >= dialogueMappings.Length)
        {
            Debug.LogWarning($"No dialogue mapping for index: {index}");
            return;
        }
        
        DialogueData dialogue = dialogueMappings[index];
        if (dialogue != null)
        {
            DialogueController.Instance.Show(dialogue);
        }
        else
        {
            Debug.LogWarning($"Dialogue mapping at index {index} is null");
        }
    }
    
    /// <summary>
    /// Migration helper for old string-based dialogue triggers
    /// </summary>
    public void TriggerDialogue(string dialogueType, int index)
    {
        // Map old string types to new DialogueData assets
        // You can customize this based on your old system
        
        switch (dialogueType)
        {
            case "Qualifiers":
                TriggerDialogue(index);
                break;
            case "Review":
                TriggerDialogue(index + 10); // Offset for different categories
                break;
            case "Intro":
                TriggerDialogue(index + 20);
                break;
            case "Story":
                TriggerDialogue(index + 30);
                break;
            case "Help":
                TriggerDialogue(index + 40);
                break;
            default:
                Debug.LogWarning($"Unknown dialogue type: {dialogueType}");
                break;
        }
    }
}

/// <summary>
/// Extension methods to make migration easier
/// </summary>
public static class DialogueExtensions
{
    /// <summary>
    /// Quick helper to show dialogue from any MonoBehaviour
    /// </summary>
    public static void ShowDialogue(this MonoBehaviour mb, DialogueData dialogue)
    {
        DialogueController.Instance.Show(dialogue);
    }
    
    /// <summary>
    /// Quick helper to show a message from any MonoBehaviour
    /// </summary>
    public static void ShowMessage(this MonoBehaviour mb, string message, string character = "Coach")
    {
        DialogueController.Instance.ShowQuickMessage(message, character);
    }
}
