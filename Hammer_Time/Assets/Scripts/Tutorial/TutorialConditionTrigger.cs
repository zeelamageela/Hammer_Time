using UnityEngine;
using System;

/// <summary>
/// Helper component for custom tutorial conditions.
/// Attach to game objects that need to trigger tutorial progression.
/// </summary>
public class TutorialConditionTrigger : MonoBehaviour
{
    [Header("Condition")]
    [Tooltip("Unique identifier for this condition")]
    public string conditionId;
    
    [Tooltip("Automatically trigger when this GameObject is enabled?")]
    public bool triggerOnEnable = false;
    
    // Event that external systems can subscribe to
    public static event Action<string> OnConditionMet;
    
    private void OnEnable()
    {
        if (triggerOnEnable && !string.IsNullOrEmpty(conditionId))
        {
            TriggerCondition();
        }
    }
    
    /// <summary>
    /// Call this to trigger the condition
    /// </summary>
    public void TriggerCondition()
    {
        if (string.IsNullOrEmpty(conditionId))
        {
            Debug.LogWarning("TutorialConditionTrigger: conditionId is empty");
            return;
        }
        
        Debug.Log($"Tutorial condition met: {conditionId}");
        OnConditionMet?.Invoke(conditionId);
    }
    
    /// <summary>
    /// Static helper to trigger a condition from anywhere in code
    /// </summary>
    public static void Trigger(string conditionId)
    {
        OnConditionMet?.Invoke(conditionId);
    }
}

/// <summary>
/// Component that waits for a custom tutorial condition to be met.
/// Can be used with TutorialStep's CustomCondition type.
/// </summary>
public class TutorialConditionWaiter : MonoBehaviour
{
    private string waitingForConditionId;
    private bool conditionMet;
    
    private void OnEnable()
    {
        TutorialConditionTrigger.OnConditionMet += OnConditionTriggered;
    }
    
    private void OnDisable()
    {
        TutorialConditionTrigger.OnConditionMet -= OnConditionTriggered;
    }
    
    /// <summary>
    /// Wait for a specific condition to be met
    /// </summary>
    public void WaitForCondition(string conditionId, Action onComplete)
    {
        waitingForConditionId = conditionId;
        conditionMet = false;
        
        StartCoroutine(WaitForConditionCoroutine(onComplete));
    }
    
    private System.Collections.IEnumerator WaitForConditionCoroutine(Action onComplete)
    {
        yield return new WaitUntil(() => conditionMet);
        onComplete?.Invoke();
    }
    
    private void OnConditionTriggered(string conditionId)
    {
        if (conditionId == waitingForConditionId)
        {
            conditionMet = true;
        }
    }
}
