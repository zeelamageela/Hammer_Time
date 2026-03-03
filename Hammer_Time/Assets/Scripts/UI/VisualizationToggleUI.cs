using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Example UI controller for trajectory and collision visualization toggles.
/// Attach this to a GameObject in your options/settings menu.
/// Assign your Toggle components in the Inspector.
/// </summary>
public class VisualizationToggleUI : MonoBehaviour
{
    [Header("UI Toggle References")]
    [Tooltip("Toggle for trajectory dots visibility")]
    public Toggle trajectoryDotsToggle;
    
    [Tooltip("Toggle for collision lines/arrows visibility")]
    public Toggle collisionLinesToggle;
    
    [Tooltip("Toggle for aim circle visibility (OFF = show aim lines instead)")]
    public Toggle aimCircleToggle;
    
    private GameVisualizationSettings visualSettings;

    void Start()
    {
        // Get settings instance
        visualSettings = GameVisualizationSettings.Instance;
        
        // Initialize toggle states from saved settings
        if (trajectoryDotsToggle != null)
        {
            trajectoryDotsToggle.isOn = visualSettings.TrajectoryVisible;
            trajectoryDotsToggle.onValueChanged.AddListener(OnTrajectoryToggleChanged);
        }
        else
        {
            Debug.LogWarning("[VisualizationToggleUI] Trajectory toggle is not assigned in Inspector!");
        }
        
        if (collisionLinesToggle != null)
        {
            collisionLinesToggle.isOn = visualSettings.CollisionLinesVisible;
            collisionLinesToggle.onValueChanged.AddListener(OnCollisionLinesToggleChanged);
        }
        else
        {
            Debug.LogWarning("[VisualizationToggleUI] Collision lines toggle is not assigned in Inspector!");
        }
        
        if (aimCircleToggle != null)
        {
            aimCircleToggle.isOn = visualSettings.AimCircleVisible;
            aimCircleToggle.onValueChanged.AddListener(OnAimCircleToggleChanged);
        }
        else
        {
            Debug.LogWarning("[VisualizationToggleUI] Aim circle toggle is not assigned in Inspector!");
        }
        
        Debug.Log("[VisualizationToggleUI] Initialized - Trajectory: " + visualSettings.TrajectoryVisible + 
                  ", Collision: " + visualSettings.CollisionLinesVisible +
                  ", AimCircle: " + visualSettings.AimCircleVisible);
    }

    /// <summary>
    /// Called when trajectory dots toggle value changes
    /// </summary>
    private void OnTrajectoryToggleChanged(bool isOn)
    {
        visualSettings.ToggleTrajectoryVisibility(isOn);
        Debug.Log($"[VisualizationToggleUI] Player toggled trajectory dots: {isOn}");
    }

    /// <summary>
    /// Called when collision lines toggle value changes
    /// </summary>
    private void OnCollisionLinesToggleChanged(bool isOn)
    {
        visualSettings.ToggleCollisionLinesVisibility(isOn);
        Debug.Log($"[VisualizationToggleUI] Player toggled collision lines: {isOn}");
    }

    /// <summary>
    /// Called when aim circle toggle value changes
    /// </summary>
    private void OnAimCircleToggleChanged(bool isOn)
    {
        visualSettings.ToggleAimCircleVisibility(isOn);
        Debug.Log($"[VisualizationToggleUI] Player toggled aim circle: {isOn}");
    }

    void OnDestroy()
    {
        // Clean up listeners
        if (trajectoryDotsToggle != null)
        {
            trajectoryDotsToggle.onValueChanged.RemoveListener(OnTrajectoryToggleChanged);
        }
        
        if (collisionLinesToggle != null)
        {
            collisionLinesToggle.onValueChanged.RemoveListener(OnCollisionLinesToggleChanged);
        }
        
        if (aimCircleToggle != null)
        {
            aimCircleToggle.onValueChanged.RemoveListener(OnAimCircleToggleChanged);
        }
    }
}
