using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Linq;

/// <summary>
/// UI Controller for EV Optimization settings in Pause Menu
/// Allows runtime toggling of AI EV evaluation system
/// NOTE: Uses reflection to avoid compile order issues
/// </summary>
public class AISettingsUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Toggle to enable/disable EV optimization")]
    public Toggle evOptimizationToggle;
    
    [Tooltip("Slider to control EV weight (0-1)")]
    public Slider evWeightSlider;
    
    [Tooltip("Text label showing current EV weight percentage")]
    public Text evWeightLabel;
    
    [Tooltip("Toggle to enable/disable verbose EV logging")]
    public Toggle evLoggingToggle;
    
    [Header("Debug")]
    [Tooltip("Show detailed logs for troubleshooting")]
    public bool debugMode = true;
    
    private MonoBehaviour aiStrategyComponent;
    private MonoBehaviour evSystemComponent;
    
    void Start()
    {
        if (debugMode)
            Debug.Log("[AISettingsUI] Starting initialization...");
        
        // Find AI_Strategy using reflection (avoids compile-time dependency)
        var aiStrategyTypes = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.Name == "AI_Strategy");
        
        System.Type aiStrategyType = aiStrategyTypes.FirstOrDefault();
        
        if (aiStrategyType != null)
        {
            aiStrategyComponent = FindObjectOfType(aiStrategyType) as MonoBehaviour;
        }
        
        if (aiStrategyComponent == null)
        {
            Debug.LogWarning("[AISettingsUI] AI_Strategy not found in scene - AI settings disabled");
            gameObject.SetActive(false);
            return;
        }
        
        if (debugMode)
            Debug.Log($"[AISettingsUI] Found AI_Strategy: {aiStrategyComponent.gameObject.name}");
        
        // Initialize UI from current AI settings
        InitializeUI();
        
        // Hook up event listeners
        if (evOptimizationToggle != null)
        {
            evOptimizationToggle.onValueChanged.AddListener(OnEVToggleChanged);
            if (debugMode)
                Debug.Log("[AISettingsUI] EV Optimization toggle listener added");
        }
        
        if (evWeightSlider != null)
        {
            evWeightSlider.onValueChanged.AddListener(OnEVWeightChanged);
            if (debugMode)
                Debug.Log("[AISettingsUI] EV Weight slider listener added");
        }
        
        if (evLoggingToggle != null)
        {
            evLoggingToggle.onValueChanged.AddListener(OnEVLoggingChanged);
            if (debugMode)
                Debug.Log("[AISettingsUI] EV Logging toggle listener added");
        }
        
        Debug.Log("[AISettingsUI] ? Initialized successfully");
    }
    
    
    
    /// <summary>
    /// Initialize UI elements from AI_Strategy current settings
    /// </summary>
    private void InitializeUI()
    {
        if (aiStrategyComponent == null) return;
        
        var type = aiStrategyComponent.GetType();
        
        if (debugMode)
        {
            var useEVField = type.GetField("useEVOptimization");
            var weightField = type.GetField("evWeight");
            var logField = type.GetField("evVerboseLogging");
            Debug.Log($"[AISettingsUI] Initializing UI - useEV={useEVField?.GetValue(aiStrategyComponent)}, weight={weightField?.GetValue(aiStrategyComponent)}, logging={logField?.GetValue(aiStrategyComponent)}");
        }
        
        if (evOptimizationToggle != null)
        {
            var field = type.GetField("useEVOptimization");
            if (field != null)
            {
                evOptimizationToggle.isOn = (bool)field.GetValue(aiStrategyComponent);
                if (debugMode)
                    Debug.Log($"[AISettingsUI] Set toggle to {evOptimizationToggle.isOn}");
            }
        }
        
        if (evWeightSlider != null)
        {
            evWeightSlider.minValue = 0f;
            evWeightSlider.maxValue = 1f;
            
            var field = type.GetField("evWeight");
            if (field != null)
            {
                float weight = (float)field.GetValue(aiStrategyComponent);
                evWeightSlider.value = weight;
                UpdateWeightLabel(weight);
                if (debugMode)
                    Debug.Log($"[AISettingsUI] Set slider to {weight}");
            }
        }
        
        if (evLoggingToggle != null)
        {
            var field = type.GetField("evVerboseLogging");
            if (field != null)
            {
                evLoggingToggle.isOn = (bool)field.GetValue(aiStrategyComponent);
                if (debugMode)
                    Debug.Log($"[AISettingsUI] Set logging toggle to {evLoggingToggle.isOn}");
            }
        }
    }
    
    /// <summary>
    /// Called when EV Optimization toggle is changed
    /// </summary>
    private void OnEVToggleChanged(bool enabled)
    {
        if (aiStrategyComponent == null)
        {
            Debug.LogError("[AISettingsUI] AI_Strategy is null in OnEVToggleChanged!");
            return;
        }
        
        if (debugMode)
            Debug.Log($"[AISettingsUI] Toggle changed to {enabled}");
        
        // Update AI_Strategy field via reflection
        var type = aiStrategyComponent.GetType();
        var field = type.GetField("useEVOptimization");
        if (field != null)
        {
            field.SetValue(aiStrategyComponent, enabled);
            if (debugMode)
                Debug.Log($"[AISettingsUI] Updated AI_Strategy.useEVOptimization to {enabled}");
        }
        
        // Also update EVEvaluationSystem if it exists
        FindAndUpdateEVSystem("useEVEvaluation", enabled);
        
        Debug.Log($"[AISettingsUI] ? EV Optimization {(enabled ? "ENABLED" : "DISABLED")}");
    }
    
    /// <summary>
    /// Called when EV Weight slider is changed
    /// </summary>
    private void OnEVWeightChanged(float weight)
    {
        if (aiStrategyComponent == null)
        {
            Debug.LogError("[AISettingsUI] AI_Strategy is null in OnEVWeightChanged!");
            return;
        }
        
        weight = Mathf.Clamp01(weight);
        
        if (debugMode)
            Debug.Log($"[AISettingsUI] Slider changed to {weight}");
        
        // Update AI_Strategy field via reflection
        var type = aiStrategyComponent.GetType();
        var field = type.GetField("evWeight");
        if (field != null)
        {
            field.SetValue(aiStrategyComponent, weight);
            if (debugMode)
                Debug.Log($"[AISettingsUI] Updated AI_Strategy.evWeight to {weight}");
        }
        
        // Also update EVEvaluationSystem if it exists
        FindAndUpdateEVSystem("evWeight", weight);
        
        // Update label
        UpdateWeightLabel(weight);
        
        if (debugMode)
            Debug.Log($"[AISettingsUI] ? EV Weight set to {(weight * 100f):F0}%");
    }
    
    /// <summary>
    /// Called when EV Logging toggle is changed
    /// </summary>
    private void OnEVLoggingChanged(bool enabled)
    {
        if (aiStrategyComponent == null)
        {
            Debug.LogError("[AISettingsUI] AI_Strategy is null in OnEVLoggingChanged!");
            return;
        }
        
        if (debugMode)
            Debug.Log($"[AISettingsUI] Logging toggle changed to {enabled}");
        
        // Update AI_Strategy field via reflection
        var type = aiStrategyComponent.GetType();
        var field = type.GetField("evVerboseLogging");
        if (field != null)
        {
            field.SetValue(aiStrategyComponent, enabled);
            if (debugMode)
                Debug.Log($"[AISettingsUI] Updated AI_Strategy.evVerboseLogging to {enabled}");
        }
        
        // Also update EVEvaluationSystem if it exists
        FindAndUpdateEVSystem("verboseLogging", enabled);
        
        Debug.Log($"[AISettingsUI] ? EV Logging {(enabled ? "ENABLED" : "DISABLED")}");
    }
    
    /// <summary>
    /// Helper: Find and update EV System field via reflection
    /// </summary>
    private void FindAndUpdateEVSystem(string fieldName, object value)
    {
        // Try to find EV System if not already cached
        if (evSystemComponent == null)
        {
            var evSystemTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.Name == "EVEvaluationSystem");
            
            System.Type evSystemType = evSystemTypes.FirstOrDefault();
            
            if (evSystemType != null)
            {
                evSystemComponent = FindObjectOfType(evSystemType) as MonoBehaviour;
            }
        }
        
        // Update field if component found
        if (evSystemComponent != null)
        {
            var type = evSystemComponent.GetType();
            var field = type.GetField(fieldName);
            if (field != null)
            {
                field.SetValue(evSystemComponent, value);
                if (debugMode)
                    Debug.Log($"[AISettingsUI] Updated EVEvaluationSystem.{fieldName} to {value}");
            }
        }
        else if (debugMode)
        {
            Debug.LogWarning("[AISettingsUI] EVEvaluationSystem not found - will be created when game starts");
        }
    }
    
    /// <summary>
    /// Update the weight label text
    /// </summary>
    private void UpdateWeightLabel(float weight)
    {
        if (evWeightLabel != null)
        {
            int percentage = Mathf.RoundToInt(weight * 100f);
            evWeightLabel.text = $"EV Influence: {percentage}%";
        }
    }
    
    /// <summary>
    /// Clean up event listeners when destroyed
    /// </summary>
    void OnDestroy()
    {
        if (evOptimizationToggle != null)
            evOptimizationToggle.onValueChanged.RemoveListener(OnEVToggleChanged);
        
        if (evWeightSlider != null)
            evWeightSlider.onValueChanged.RemoveListener(OnEVWeightChanged);
        
        if (evLoggingToggle != null)
            evLoggingToggle.onValueChanged.RemoveListener(OnEVLoggingChanged);
    }
}
