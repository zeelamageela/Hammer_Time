using UnityEngine;

/// <summary>
/// Manages visibility settings for trajectory and collision visualization
/// during player turns. Can be controlled via UI toggles.
/// </summary>
public class GameVisualizationSettings : MonoBehaviour
{
    private static GameVisualizationSettings instance;
    public static GameVisualizationSettings Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GameVisualizationSettings");
                instance = go.AddComponent<GameVisualizationSettings>();
                DontDestroyOnLoad(go);
                instance.LoadSettings();
            }
            return instance;
        }
    }

    // Persistent settings keys
    private const string TRAJECTORY_VISIBLE_KEY = "TrajectoryVisible";
    private const string COLLISION_LINES_VISIBLE_KEY = "CollisionLinesVisible";
    private const string AIM_CIRCLE_VISIBLE_KEY = "AimCircleVisible";

    // Current settings (cached for performance)
    private bool trajectoryVisible = true;
    private bool collisionLinesVisible = true;
    private bool aimCircleVisible = true;

    // Public properties with change notifications
    public bool TrajectoryVisible
    {
        get => trajectoryVisible;
        set
        {
            if (trajectoryVisible != value)
            {
                trajectoryVisible = value;
                PlayerPrefs.SetInt(TRAJECTORY_VISIBLE_KEY, value ? 1 : 0);
                PlayerPrefs.Save();
                OnTrajectoryVisibilityChanged?.Invoke(value);
                Debug.Log($"[GameVisualizationSettings] Trajectory visibility: {value}");
            }
        }
    }

    public bool CollisionLinesVisible
    {
        get => collisionLinesVisible;
        set
        {
            if (collisionLinesVisible != value)
            {
                collisionLinesVisible = value;
                PlayerPrefs.SetInt(COLLISION_LINES_VISIBLE_KEY, value ? 1 : 0);
                PlayerPrefs.Save();
                OnCollisionLinesVisibilityChanged?.Invoke(value);
                Debug.Log($"[GameVisualizationSettings] Collision lines visibility: {value}");
            }
        }
    }

    public bool AimCircleVisible
    {
        get => aimCircleVisible;
        set
        {
            if (aimCircleVisible != value)
            {
                aimCircleVisible = value;
                PlayerPrefs.SetInt(AIM_CIRCLE_VISIBLE_KEY, value ? 1 : 0);
                PlayerPrefs.Save();
                OnAimCircleVisibilityChanged?.Invoke(value);
                Debug.Log($"[GameVisualizationSettings] Aim circle visibility: {value}");
            }
        }
    }

    // Events for systems to subscribe to
    public delegate void VisibilityChangedDelegate(bool visible);
    public event VisibilityChangedDelegate OnTrajectoryVisibilityChanged;
    public event VisibilityChangedDelegate OnCollisionLinesVisibilityChanged;
    public event VisibilityChangedDelegate OnAimCircleVisibilityChanged;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Load settings from PlayerPrefs
    /// </summary>
    private void LoadSettings()
    {
        trajectoryVisible = PlayerPrefs.GetInt(TRAJECTORY_VISIBLE_KEY, 1) == 1;
        collisionLinesVisible = PlayerPrefs.GetInt(COLLISION_LINES_VISIBLE_KEY, 1) == 1;
        aimCircleVisible = PlayerPrefs.GetInt(AIM_CIRCLE_VISIBLE_KEY, 1) == 1;
        
        Debug.Log($"[GameVisualizationSettings] Loaded settings - Trajectory: {trajectoryVisible}, Collision: {collisionLinesVisible}, AimCircle: {aimCircleVisible}");
    }

    /// <summary>
    /// Toggle trajectory visibility (for UI toggle callback)
    /// </summary>
    public void ToggleTrajectoryVisibility(bool visible)
    {
        TrajectoryVisible = visible;
    }

    /// <summary>
    /// Toggle collision lines visibility (for UI toggle callback)
    /// </summary>
    public void ToggleCollisionLinesVisibility(bool visible)
    {
        CollisionLinesVisible = visible;
    }

    /// <summary>
    /// Toggle aim circle visibility (for UI toggle callback)
    /// </summary>
    public void ToggleAimCircleVisibility(bool visible)
    {
        AimCircleVisible = visible;
    }

    /// <summary>
    /// Reset to defaults
    /// </summary>
    public void ResetToDefaults()
    {
        TrajectoryVisible = true;
        CollisionLinesVisible = true;
        AimCircleVisible = true;
    }
}
