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
    private const string GUIDELINES_VISIBLE_KEY = "GuidelinesVisible";
    private const string CURL_LINE_VISIBLE_KEY = "CurlLineVisible";
    private const string COLLISION_WARNING_VISIBLE_KEY = "CollisionWarningVisible";

    // Current settings (cached for performance)
    private bool trajectoryVisible = true;
    private bool collisionLinesVisible = true;
    private bool aimCircleVisible = true;
    private bool guidelinesVisible = true;
    private bool curlLineVisible = true;
    private bool collisionWarningVisible = true;

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

    public bool GuidelinesVisible
    {
        get => guidelinesVisible;
        set
        {
            if (guidelinesVisible != value)
            {
                guidelinesVisible = value;
                PlayerPrefs.SetInt(GUIDELINES_VISIBLE_KEY, value ? 1 : 0);
                PlayerPrefs.Save();
                OnGuidelinesVisibilityChanged?.Invoke(value);
                Debug.Log($"[GameVisualizationSettings] Guidelines visibility: {value}");
            }
        }
    }

    public bool CurlLineVisible
    {
        get => curlLineVisible;
        set
        {
            if (curlLineVisible != value)
            {
                curlLineVisible = value;
                PlayerPrefs.SetInt(CURL_LINE_VISIBLE_KEY, value ? 1 : 0);
                PlayerPrefs.Save();
                OnCurlLineVisibilityChanged?.Invoke(value);
                Debug.Log($"[GameVisualizationSettings] Curl line visibility: {value}");
            }
        }
    }

    public bool CollisionWarningVisible
    {
        get => collisionWarningVisible;
        set
        {
            if (collisionWarningVisible != value)
            {
                collisionWarningVisible = value;
                PlayerPrefs.SetInt(COLLISION_WARNING_VISIBLE_KEY, value ? 1 : 0);
                PlayerPrefs.Save();
                OnCollisionWarningVisibilityChanged?.Invoke(value);
                Debug.Log($"[GameVisualizationSettings] Collision warning visibility: {value}");
            }
        }
    }

    // Events for systems to subscribe to
    public delegate void VisibilityChangedDelegate(bool visible);
    public event VisibilityChangedDelegate OnTrajectoryVisibilityChanged;
    public event VisibilityChangedDelegate OnCollisionLinesVisibilityChanged;
    public event VisibilityChangedDelegate OnAimCircleVisibilityChanged;
    public event VisibilityChangedDelegate OnGuidelinesVisibilityChanged;
    public event VisibilityChangedDelegate OnCurlLineVisibilityChanged;
    public event VisibilityChangedDelegate OnCollisionWarningVisibilityChanged;

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
        guidelinesVisible = PlayerPrefs.GetInt(GUIDELINES_VISIBLE_KEY, 1) == 1;
        curlLineVisible = PlayerPrefs.GetInt(CURL_LINE_VISIBLE_KEY, 1) == 1;
        collisionWarningVisible = PlayerPrefs.GetInt(COLLISION_WARNING_VISIBLE_KEY, 1) == 1;
        
        Debug.Log($"[GameVisualizationSettings] Loaded settings - Trajectory: {trajectoryVisible}, Collision: {collisionLinesVisible}, AimCircle: {aimCircleVisible}, Guidelines: {guidelinesVisible}, CurlLine: {curlLineVisible}, CollisionWarning: {collisionWarningVisible}");
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
    /// Toggle guidelines visibility (for UI toggle callback)
    /// </summary>
    public void ToggleGuidelinesVisibility(bool visible)
    {
        GuidelinesVisible = visible;
    }

    /// <summary>
    /// Toggle curl line visibility (for UI toggle callback)
    /// </summary>
    public void ToggleCurlLineVisibility(bool visible)
    {
        CurlLineVisible = visible;
    }

    /// <summary>
    /// Toggle collision warning visibility (for UI toggle callback)
    /// </summary>
    public void ToggleCollisionWarningVisibility(bool visible)
    {
        CollisionWarningVisible = visible;
    }

    /// <summary>
    /// Reset to defaults
    /// </summary>
    public void ResetToDefaults()
    {
        TrajectoryVisible = true;
        CollisionLinesVisible = true;
        AimCircleVisible = true;
        GuidelinesVisible = true;
        CurlLineVisible = true;
        CollisionWarningVisible = true;
    }
}
