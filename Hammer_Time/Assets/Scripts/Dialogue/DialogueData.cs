using UnityEngine;

/// <summary>
/// ScriptableObject that stores dialogue content.
/// Create instances via: Assets > Create > Dialogue > Dialogue Data
/// </summary>
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data", order = 1)]
public class DialogueData : ScriptableObject
{
    [Header("Character Info")]
    [Tooltip("Name displayed in dialogue box (e.g., 'Coach', 'Announcer')")]
    public string characterName = "Coach";
    
    [Header("Dialogue Content")]
    [Tooltip("Each entry is one dialogue box. Click Next to advance.")]
    [TextArea(3, 10)]
    public string[] dialogueLines;
    
    [Header("Behavior")]
    [Tooltip("Should the game pause while this dialogue is displayed?")]
    public bool pauseGame = true;
    
    [Tooltip("Optional: Auto-advance after this many seconds (0 = wait for player input)")]
    public float autoAdvanceDelay = 0f;
    
    [Header("Text Replacement")]
    [Tooltip("If true, replaces tokens like {PLAYER_NAME}, {TEAM_NAME}, {EARNINGS}, etc.")]
    public bool useTextReplacement = true;
    
    /// <summary>
    /// Gets a specific dialogue line with text replacement applied if enabled
    /// </summary>
    public string GetLine(int index, CareerManager cm = null, GameManager gm = null)
    {
        if (index < 0 || index >= dialogueLines.Length)
            return "";
            
        string line = dialogueLines[index];
        
        if (useTextReplacement)
        {
            line = TextReplacementUtility.Replace(line, cm, gm);
        }
        
        return line;
    }
    
    /// <summary>
    /// Returns total number of dialogue lines
    /// </summary>
    public int LineCount => dialogueLines != null ? dialogueLines.Length : 0;
}
