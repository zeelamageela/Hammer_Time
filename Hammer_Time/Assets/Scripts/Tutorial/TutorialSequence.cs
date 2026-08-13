using UnityEngine;

/// <summary>
/// A sequence of tutorial steps that can be played in order.
/// Create via: Assets > Create > Tutorial > Tutorial Sequence
/// </summary>
[CreateAssetMenu(fileName = "NewTutorialSequence", menuName = "Tutorial/Tutorial Sequence", order = 2)]
public class TutorialSequence : ScriptableObject
{
    [Header("Sequence Info")]
    [Tooltip("Unique ID for this tutorial (e.g., 'intro_tutorial', 'sweep_tutorial')")]
    public string sequenceId = "new_tutorial";
    
    [Tooltip("Display name shown to player")]
    public string displayName = "Tutorial";
    
    [TextArea(2, 4)]
    [Tooltip("Brief description of what this tutorial teaches")]
    public string description;
    
    [Header("Steps")]
    [Tooltip("Ordered list of tutorial steps")]
    public TutorialStep[] steps;
    
    [Header("Settings")]
    [Tooltip("Can this tutorial be skipped?")]
    public bool canSkip = true;
    
    [Tooltip("Should this tutorial auto-start under certain conditions?")]
    public bool autoStart = false;

    [Tooltip("Condition that must be true for auto-start (e.g., week == 1)")]
    public TutorialAutoStartCondition autoStartCondition;

    [Tooltip("Sequence ID to play immediately after this one completes (leave empty for none)")]
    public string chainSequenceId;
    
    /// <summary>
    /// Returns total number of steps in this sequence
    /// </summary>
    public int StepCount => steps != null ? steps.Length : 0;
    
    /// <summary>
    /// Check if a step index is valid
    /// </summary>
    public bool IsValidStep(int index)
    {
        return steps != null && index >= 0 && index < steps.Length;
    }
    
    /// <summary>
    /// Get a specific step by index
    /// </summary>
    public TutorialStep GetStep(int index)
    {
        if (!IsValidStep(index))
            return null;
        
        return steps[index];
    }
}

/// <summary>
/// Defines when a tutorial should auto-start
/// </summary>
[System.Serializable]
public class TutorialAutoStartCondition
{
    public bool checkWeek = false;
    public int requiredWeek = 1;
    
    public bool checkFirstTime = true;
    
    public bool checkScene = false;
    public string requiredSceneName;
    
    public bool Evaluate(CareerManager cm)
    {
        if (checkWeek && cm != null && cm.week != requiredWeek)
            return false;
        
        // Additional checks can be added here
        
        return true;
    }
}
