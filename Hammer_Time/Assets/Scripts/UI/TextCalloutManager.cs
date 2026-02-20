using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Global singleton manager for spawning floating text callouts anywhere in the game.
/// Call from anywhere with: TextCalloutManager.Instance.ShowCallout(position, "Text!", followTarget: true);
/// </summary>
public class TextCalloutManager : MonoBehaviour
{
    #region Singleton
    private static TextCalloutManager _instance;
    public static TextCalloutManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TextCalloutManager>();
                if (_instance == null)
                {
                    Debug.LogError("[TextCalloutManager] No instance found in scene! Add TextCalloutManager to a GameObject.");
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else if (_instance != this)
        {
            Destroy(gameObject); // Enforce singleton
        }
    }
    #endregion

    [Header("Callout Prefab")]
    [Tooltip("The TextCallout prefab to spawn (must have TextCallout component and UI Text)")]
    public GameObject calloutPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Parent transform for spawned callouts (usually Canvas)")]
    public Transform calloutParent;

    [Tooltip("If no parent specified, create callouts at this offset from target")]
    public Vector3 defaultWorldOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Pool Settings")]
    [Tooltip("Pre-instantiate this many callouts for performance")]
    public int poolSize = 10;

    [Tooltip("Auto-expand pool if we run out of callouts")]
    public bool autoExpandPool = true;

    [Header("Default Appearance")]
    [Tooltip("Default text color (can be overridden per callout)")]
    public Color defaultTextColor = Color.white;

    [Tooltip("Default font size (can be overridden per callout)")]
    public float defaultFontSize = 24f;

    [Header("Default Animation")]
    [Tooltip("How long the callout stays visible")]
    public float defaultDuration = 2f;

    [Tooltip("How far the callout floats upward")]
    public float defaultFloatDistance = 1f;

    [Tooltip("Fade out over this duration at the end")]
    public float defaultFadeDuration = 0.5f;

    // Object pool
    private Queue<TextCallout> calloutPool = new Queue<TextCallout>();
    private List<TextCallout> activeCallouts = new List<TextCallout>();

    private void Start()
    {
        // Validate setup
        if (calloutPrefab == null)
        {
            Debug.LogError("[TextCalloutManager] Callout prefab not assigned! Assign in Inspector.");
            return;
        }

        if (calloutPrefab.GetComponent<TextCallout>() == null)
        {
            Debug.LogError("[TextCalloutManager] Callout prefab missing TextCallout component!");
            return;
        }

        // Create initial pool
        InitializePool();
    }

    /// <summary>
    /// Create initial object pool for performance
    /// </summary>
    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateCalloutObject();
        }

        Debug.Log($"[TextCalloutManager] Initialized pool with {poolSize} callouts");
    }

    /// <summary>
    /// Create a new callout object and add to pool
    /// </summary>
    private TextCallout CreateCalloutObject()
    {
        GameObject calloutObj = Instantiate(calloutPrefab, calloutParent);
        calloutObj.SetActive(false);

        TextCallout callout = calloutObj.GetComponent<TextCallout>();
        if (callout == null)
        {
            Debug.LogError("[TextCalloutManager] Spawned callout missing TextCallout component!");
            Destroy(calloutObj);
            return null;
        }

        calloutPool.Enqueue(callout);
        return callout;
    }

    /// <summary>
    /// Get a callout from the pool (or create new if needed)
    /// </summary>
    private TextCallout GetCalloutFromPool()
    {
        // Try to get from pool
        if (calloutPool.Count > 0)
        {
            TextCallout callout = calloutPool.Dequeue();
            activeCallouts.Add(callout);
            return callout;
        }

        // Pool empty - create new if allowed
        if (autoExpandPool)
        {
            Debug.Log("[TextCalloutManager] Pool empty, creating new callout");
            TextCallout newCallout = CreateCalloutObject();
            if (newCallout != null)
            {
                calloutPool.Dequeue(); // Remove from pool (was just added)
                activeCallouts.Add(newCallout);
                return newCallout;
            }
        }

        Debug.LogWarning("[TextCalloutManager] No callouts available in pool!");
        return null;
    }

    /// <summary>
    /// Return a callout to the pool
    /// </summary>
    public void ReturnToPool(TextCallout callout)
    {
        if (callout == null) return;

        callout.gameObject.SetActive(false);
        activeCallouts.Remove(callout);
        calloutPool.Enqueue(callout);
    }

    #region Public API

    /// <summary>
    /// Show a callout with full customization options
    /// </summary>
    public TextCallout ShowCallout(
        Vector3 targetPosition,
        string text,
        bool followTarget = false,
        Transform target = null,
        float? duration = null,
        float? floatDistance = null,
        Color? textColor = null,
        float? fontSize = null,
        float? fadeDuration = null)
    {
        if (calloutPrefab == null)
        {
            Debug.LogError("[TextCalloutManager] Cannot show callout - prefab not assigned!");
            return null;
        }

        // Get callout from pool
        TextCallout callout = GetCalloutFromPool();
        if (callout == null)
        {
            Debug.LogError("[TextCalloutManager] Failed to get callout from pool!");
            return null;
        }

        // Configure callout
        callout.Initialize(
            text: text,
            startPosition: targetPosition,
            followTarget: followTarget,
            target: target,
            duration: duration ?? defaultDuration,
            floatDistance: floatDistance ?? defaultFloatDistance,
            textColor: textColor ?? defaultTextColor,
            fontSize: fontSize ?? defaultFontSize,
            fadeDuration: fadeDuration ?? defaultFadeDuration,
            manager: this
        );

        callout.gameObject.SetActive(true);
        callout.StartAnimation();

        Debug.Log($"[TextCalloutManager] Spawned callout: '{text}' at {targetPosition}");

        return callout;
    }

    /// <summary>
    /// Simple callout - just position and text
    /// </summary>
    public TextCallout ShowCallout(Vector3 targetPosition, string text)
    {
        return ShowCallout(targetPosition, text, followTarget: false);
    }

    /// <summary>
    /// Callout that follows a transform
    /// </summary>
    public TextCallout ShowCallout(Transform target, string text, bool followTarget = true)
    {
        if (target == null)
        {
            Debug.LogWarning("[TextCalloutManager] Target transform is null!");
            return ShowCallout(Vector3.zero, text, followTarget: false);
        }

        return ShowCallout(
            targetPosition: target.position + defaultWorldOffset,
            text: text,
            followTarget: followTarget,
            target: target
        );
    }

    /// <summary>
    /// Quick callout for rock objects (common use case)
    /// </summary>
    public TextCallout ShowRockCallout(GameObject rock, string text)
    {
        if (rock == null)
        {
            Debug.LogWarning("[TextCalloutManager] Rock GameObject is null!");
            return null;
        }

        return ShowCallout(
            target: rock.transform,
            text: text,
            followTarget: true
        );
    }

    /// <summary>
    /// Clear all active callouts immediately
    /// </summary>
    public void ClearAllCallouts()
    {
        foreach (TextCallout callout in activeCallouts.ToArray()) // ToArray to avoid modification during iteration
        {
            callout.ForceStop();
        }
    }

    #endregion

    #region Debug Helpers

    /// <summary>
    /// Get pool statistics
    /// </summary>
    public string GetPoolStats()
    {
        return $"Active: {activeCallouts.Count} | Pooled: {calloutPool.Count} | Total: {activeCallouts.Count + calloutPool.Count}";
    }

    private void OnGUI()
    {
        // Debug overlay (only in editor)
        if (!Application.isEditor) return;

        GUI.Label(new Rect(10, 10, 300, 20), $"[TextCallout Pool] {GetPoolStats()}");
    }

    #endregion
}
