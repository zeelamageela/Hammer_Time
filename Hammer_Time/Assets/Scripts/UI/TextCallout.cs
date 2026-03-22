using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Individual floating text callout instance.
/// Handles animation, positioning, following targets, and returning to pool.
/// </summary>
[RequireComponent(typeof(Text))]
public class TextCallout : MonoBehaviour
{
    // Cached components
    private Text textComponent;
    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private Camera mainCamera;

    // Animation state
    private Coroutine animationCoroutine;
    private bool isFollowingTarget = false;
    private Transform followTarget = null;
    private Vector3 startWorldPosition;
    private Vector3 targetWorldOffset;

    // Animation parameters (set during Initialize)
    private float duration = 2f;
    private float floatDistance = 1f;
    private float fadeDuration = 0.5f;

    // Reference to manager for returning to pool
    private TextCalloutManager manager;

    private void Awake()
    {
        // Cache components
        textComponent = GetComponent<Text>();
        if (textComponent == null)
        {
            Debug.LogError("[TextCallout] Missing UI Text component!");
        }

        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("[TextCallout] Missing RectTransform!");
        }

        // Find parent canvas
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogWarning("[TextCallout] No parent Canvas found! World-to-screen conversion may fail.");
        }

        // Cache main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("[TextCallout] No main camera found! Using Camera.current fallback.");
        }
    }

    /// <summary>
    /// Initialize the callout with all parameters
    /// </summary>
    public void Initialize(
        string text,
        Vector3 startPosition,
        bool followTarget,
        Transform target,
        float duration,
        float floatDistance,
        Color textColor,
        float fontSize,
        float fadeDuration,
        TextCalloutManager manager)
    {
        // Set text content
        if (textComponent != null)
        {
            textComponent.text = text;
            Color color = textColor;
            color.a = 1f; // Start fully visible
            textComponent.color = color;
            textComponent.fontSize = (int)fontSize; // UI Text uses int fontSize
        }

        // Set position parameters
        this.startWorldPosition = startPosition;
        this.isFollowingTarget = followTarget;
        this.followTarget = target;
        this.targetWorldOffset = Vector3.zero;

        // If following, calculate offset from target
        if (followTarget && target != null)
        {
            this.targetWorldOffset = startPosition - target.position;
        }

        // Set animation parameters
        this.duration = duration;
        this.floatDistance = floatDistance;
        this.fadeDuration = fadeDuration;

        // Store manager reference
        this.manager = manager;

        // Set initial screen position
        UpdatePosition(startWorldPosition);
    }

    /// <summary>
    /// Start the callout animation
    /// </summary>
    public void StartAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(AnimateCallout());
    }

    /// <summary>
    /// Main animation coroutine
    /// </summary>
    private IEnumerator AnimateCallout()
    {
        float elapsed = 0f;
        Vector3 startLocalPos = startWorldPosition;

        // Main animation loop
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Calculate current world position
            Vector3 currentWorldPos;

            if (isFollowingTarget && followTarget != null)
            {
                // Follow target + offset + float upward
                currentWorldPos = followTarget.position + targetWorldOffset + (Vector3.up * floatDistance * t);
            }
            else
            {
                // Static position + float upward
                currentWorldPos = startLocalPos + (Vector3.up * floatDistance * t);
            }

            // Update screen position
            UpdatePosition(currentWorldPos);

            // Fade out during last portion
            if (elapsed > duration - fadeDuration)
            {
                float fadeT = 1f - ((elapsed - (duration - fadeDuration)) / fadeDuration);
                if (textComponent != null)
                {
                    Color color = textComponent.color;
                    color.a = fadeT;
                    textComponent.color = color;
                }
            }

            yield return null;
        }

        // Animation complete - return to pool
        ReturnToPool();
    }

    /// <summary>
    /// Update the callout's screen position from a world position
    /// </summary>
    private void UpdatePosition(Vector3 worldPosition)
    {
        if (rectTransform == null || parentCanvas == null)
            return;

        // WORLD SPACE CANVAS: Just use world position directly!
        if (parentCanvas.renderMode == RenderMode.WorldSpace)
        {
            // For World Space canvas, position is directly in world coordinates
            rectTransform.position = worldPosition;
            return;
        }

        // SCREEN SPACE CANVAS: Convert world to screen to canvas local
        Camera cam = mainCamera ?? Camera.current;
        if (cam == null)
        {
            Debug.LogWarning("[TextCallout] No camera available for world-to-screen conversion!");
            return;
        }

        // Convert world position to screen position
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldPosition);

        // Convert screen position to canvas local position
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            screenPos,
            cam,
            out canvasPos
        );

        // Update position
        rectTransform.anchoredPosition = canvasPos;
    }

    /// <summary>
    /// Force stop the animation and return to pool immediately
    /// </summary>
    public void ForceStop()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        ReturnToPool();
    }

    /// <summary>
    /// Return this callout to the pool
    /// </summary>
    private void ReturnToPool()
    {
        // Reset state
        isFollowingTarget = false;
        followTarget = null;

        // Return to manager
        if (manager != null)
        {
            manager.ReturnToPool(this);
        }
        else
        {
            // Fallback: just disable
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        // Clean up coroutine if disabled externally
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }
}
