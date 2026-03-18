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
    
    [Tooltip("Toggle for guidelines visibility (vertical + horizontal aim lines)")]
    public Toggle guidelinesToggle;
    
    [Tooltip("Toggle for curl line visibility (shows curl from vertical line to aim circle)")]
    public Toggle curlLineToggle;
    
    [Tooltip("Toggle for collision warning line visibility (small red line at collision point)")]
    public Toggle collisionWarningToggle;
    
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
        
        if (guidelinesToggle != null)
        {
            guidelinesToggle.isOn = visualSettings.GuidelinesVisible;
            guidelinesToggle.onValueChanged.AddListener(OnGuidelinesToggleChanged);
        }
        else
        {
            Debug.LogWarning("[VisualizationToggleUI] Guidelines toggle is not assigned in Inspector!");
        }
        
        if (curlLineToggle != null)
        {
            curlLineToggle.isOn = visualSettings.CurlLineVisible;
            curlLineToggle.onValueChanged.AddListener(OnCurlLineToggleChanged);
        }
        else
        {
            Debug.LogWarning("[VisualizationToggleUI] Curl line toggle is not assigned in Inspector!");
        }
        
        if (collisionWarningToggle != null)
        {
            collisionWarningToggle.isOn = visualSettings.CollisionWarningVisible;
            collisionWarningToggle.onValueChanged.AddListener(OnCollisionWarningToggleChanged);
        }
        else
        {
            Debug.LogWarning("[VisualizationToggleUI] Collision warning toggle is not assigned in Inspector!");
        }
        
        Debug.Log("[VisualizationToggleUI] Initialized - Trajectory: " + visualSettings.TrajectoryVisible + 
                  ", Collision: " + visualSettings.CollisionLinesVisible +
                  ", AimCircle: " + visualSettings.AimCircleVisible +
                  ", Guidelines: " + visualSettings.GuidelinesVisible +
                  ", CurlLine: " + visualSettings.CurlLineVisible +
                  ", CollisionWarning: " + visualSettings.CollisionWarningVisible);
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

    /// <summary>
    /// Called when guidelines toggle value changes
    /// </summary>
    private void OnGuidelinesToggleChanged(bool isOn)
    {
        visualSettings.ToggleGuidelinesVisibility(isOn);
        Debug.Log($"[VisualizationToggleUI] Player toggled guidelines: {isOn}");
    }

    /// <summary>
    /// Called when curl line toggle value changes
    /// </summary>
    private void OnCurlLineToggleChanged(bool isOn)
    {
        visualSettings.ToggleCurlLineVisibility(isOn);
        Debug.Log($"[VisualizationToggleUI] Player toggled curl line: {isOn}");
    }

    /// <summary>
    /// Called when collision warning toggle value changes
    /// </summary>
    private void OnCollisionWarningToggleChanged(bool isOn)
    {
        visualSettings.ToggleCollisionWarningVisibility(isOn);
        Debug.Log($"[VisualizationToggleUI] Player toggled collision warning: {isOn}");
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
        
        if (guidelinesToggle != null)
        {
            guidelinesToggle.onValueChanged.RemoveListener(OnGuidelinesToggleChanged);
        }
        
        if (curlLineToggle != null)
        {
            curlLineToggle.onValueChanged.RemoveListener(OnCurlLineToggleChanged);
        }
        
        if (collisionWarningToggle != null)
        {
            collisionWarningToggle.onValueChanged.RemoveListener(OnCollisionWarningToggleChanged);
        }
    }
}
