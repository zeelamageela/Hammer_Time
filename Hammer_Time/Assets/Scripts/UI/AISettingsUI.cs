using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Controller for EV Optimization settings in Pause Menu
/// Allows runtime toggling of AI EV evaluation system
/// NOTE: Uses FindObjectOfType to locate components dynamically
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
    
    // We'll use FindObjectOfType at runtime instead of storing typed references
    private MonoBehaviour aiStrategyComponent;
    
    void Start()
    {
        // Find AI_Strategy component dynamically
        aiStrategyComponent = FindObjectOfType(System.Type.GetType("AI_Strategy")) as MonoBehaviour;
        
        if (aiStrategyComponent == null)
        {
            Debug.LogWarning("[AISettingsUI] AI_Strategy not found in scene - AI settings disabled");
            gameObject.SetActive(false);
            return;
        }
        
        // Initialize UI from current AI settings
        InitializeUI();
        
        // Hook up event listeners
        if (evOptimizationToggle != null)
            evOptimizationToggle.onValueChanged.AddListener(OnEVToggleChanged);
        
        if (evWeightSlider != null)
            evWeightSlider.onValueChanged.AddListener(OnEVWeightChanged);
        
        if (evLoggingToggle != null)
            evLoggingToggle.onValueChanged.AddListener(OnEVLoggingChanged);
        
        Debug.Log("[AISettingsUI] Initialized successfully");
    }
    
    /// <summary>
    /// Initialize UI elements from AI_Strategy current settings
    /// </summary>
    private void InitializeUI()
    {
        if (aiStrategyComponent == null) return;
        
        // Use reflection to get current values
        var type = aiStrategyComponent.GetType();
        
        if (evOptimizationToggle != null)
        {
            var field = type.GetField("useEVOptimization");
            if (field != null)
                evOptimizationToggle.isOn = (bool)field.GetValue(aiStrategyComponent);
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
            }
        }
        
        if (evLoggingToggle != null)
        {
            var field = type.GetField("evVerboseLogging");
            if (field != null)
                evLoggingToggle.isOn = (bool)field.GetValue(aiStrategyComponent);
        }
    }
    
    /// <summary>
    /// Called when EV Optimization toggle is changed
    /// </summary>
    private void OnEVToggleChanged(bool enabled)
    {
        if (aiStrategyComponent == null) return;
        
        // Update AI_Strategy setting via reflection
        var type = aiStrategyComponent.GetType();
        var field = type.GetField("useEVOptimization");
        if (field != null)
            field.SetValue(aiStrategyComponent, enabled);
        
        // Also update EVEvaluationSystem if it exists
        var evSysType = System.Type.GetType("EVEvaluationSystem");
        if (evSysType != null)
        {
            var evSys = FindObjectOfType(evSysType) as MonoBehaviour;
            if (evSys != null)
            {
                var method = evSysType.GetMethod("SetEVEnabled");
                if (method != null)
                    method.Invoke(evSys, new object[] { enabled });
            }
        }
        
        Debug.Log($"[AISettingsUI] EV Optimization {(enabled ? "ENABLED" : "DISABLED")}");
    }
    
    /// <summary>
    /// Called when EV Weight slider is changed
    /// </summary>
    private void OnEVWeightChanged(float weight)
    {
        if (aiStrategyComponent == null) return;
        
        weight = Mathf.Clamp01(weight);
        
        // Update AI_Strategy setting via reflection
        var type = aiStrategyComponent.GetType();
        var field = type.GetField("evWeight");
        if (field != null)
            field.SetValue(aiStrategyComponent, weight);
        
        // Also update EVEvaluationSystem if it exists
        var evSysType = System.Type.GetType("EVEvaluationSystem");
        if (evSysType != null)
        {
            var evSys = FindObjectOfType(evSysType) as MonoBehaviour;
            if (evSys != null)
            {
                var method = evSysType.GetMethod("SetEVWeight");
                if (method != null)
                    method.Invoke(evSys, new object[] { weight });
            }
        }
        
        UpdateWeightLabel(weight);
    }
    
    /// <summary>
    /// Called when EV Logging toggle is changed
    /// </summary>
    private void OnEVLoggingChanged(bool enabled)
    {
        if (aiStrategyComponent == null) return;
        
        // Update AI_Strategy setting via reflection
        var type = aiStrategyComponent.GetType();
        var field = type.GetField("evVerboseLogging");
        if (field != null)
            field.SetValue(aiStrategyComponent, enabled);
        
        // Also update EVEvaluationSystem if it exists
        var evSysType = System.Type.GetType("EVEvaluationSystem");
        if (evSysType != null)
        {
            var evSys = FindObjectOfType(evSysType) as MonoBehaviour;
            if (evSys != null)
            {
                var field2 = evSysType.GetField("verboseLogging");
                if (field2 != null)
                    field2.SetValue(evSys, enabled);
            }
        }
        
        Debug.Log($"[AISettingsUI] EV Logging {(enabled ? "ENABLED" : "DISABLED")}");
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
