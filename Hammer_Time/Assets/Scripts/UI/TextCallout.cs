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
    private Vector3 currentWorldPosition; // Track current position for stacking
    private Vector3 finalWorldPosition;   // Track final position (after full animation)
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
            color.a = 0f; // Start invisible (will fade in during animation)
            textComponent.color = color;
            textComponent.fontSize = (int)fontSize; // UI Text uses int fontSize
        }

        // Set position parameters
        this.startWorldPosition = startPosition;
        this.currentWorldPosition = startPosition;
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
        
        // Calculate final position (start + full float distance)
        if (isFollowingTarget && target != null)
        {
            this.finalWorldPosition = target.position + targetWorldOffset + (Vector3.up * floatDistance);
        }
        else
        {
            this.finalWorldPosition = startPosition + (Vector3.up * floatDistance);
        }

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
    /// Phase 1: Snap-in (18% time) - ULTRA-fast movement + fade-in to 66% height (66-18 ratio!)
    /// Phase 2: Hold (60% time) - Pause at 66% height, fully visible
    /// Phase 3: Float-out (22% time) - Gentle movement + fade-out to 100% height
    /// </summary>
    private IEnumerator AnimateCallout()
    {
        float elapsed = 0f;
        Vector3 startLocalPos = startWorldPosition;
        
        // Calculate phase durations (66-18 ratio for snap!)
        float snapInDuration = duration * 0.18f;      // 18% - ULTRA-fast snap to 66% height
        float holdDuration = duration * 0.60f;        // 60% - Extended hold for readability
        float floatOutDuration = duration * 0.22f;    // 22% - Gentle float-out
        
        // Calculate phase boundaries
        float holdStartTime = snapInDuration;
        float floatOutStartTime = snapInDuration + holdDuration;
        
        // PHASE 1: ULTRA-SNAP-IN (0-18% time ? 0-66% height)
        while (elapsed < holdStartTime)
        {
            elapsed += Time.deltaTime;
            float phase1T = Mathf.Clamp01(elapsed / snapInDuration);
            
            // Snap to 66% height using quartic ease-in (SUPER aggressive!)
            float easedT = phase1T * phase1T * phase1T * phase1T; // Quartic for ULTRA snap
            float currentHeight = 0.66f * easedT; // 0 ? 66%
            
            // Calculate current world position
            Vector3 currentWorldPos;
            if (isFollowingTarget && followTarget != null)
            {
                currentWorldPos = followTarget.position + targetWorldOffset + (Vector3.up * floatDistance * currentHeight);
            }
            else
            {
                currentWorldPos = startLocalPos + (Vector3.up * floatDistance * currentHeight);
            }
            
            currentWorldPosition = currentWorldPos; // Update tracked position
            UpdatePosition(currentWorldPos);
            
            // Fade in opacity (0% ? 100%) - Cubic for smooth but fast fade
            if (textComponent != null)
            {
                Color color = textComponent.color;
                color.a = phase1T * phase1T * phase1T; // Cubic ease for smooth fade-in
                textComponent.color = color;
            }
            
            yield return null;
        }
        
        // PHASE 2: HOLD (18-78% time at 66% height)
        while (elapsed < floatOutStartTime)
        {
            elapsed += Time.deltaTime;
            
            // Stay at 66% height, fully visible
            Vector3 currentWorldPos;
            if (isFollowingTarget && followTarget != null)
            {
                currentWorldPos = followTarget.position + targetWorldOffset + (Vector3.up * floatDistance * 0.66f);
            }
            else
            {
                currentWorldPos = startLocalPos + (Vector3.up * floatDistance * 0.66f);
            }
            
            currentWorldPosition = currentWorldPos; // Update tracked position
            UpdatePosition(currentWorldPos);
            
            // Full opacity
            if (textComponent != null)
            {
                Color color = textComponent.color;
                color.a = 1f;
                textComponent.color = color;
            }
            
            yield return null;
        }
        
        // PHASE 3: FLOAT-OUT (78-100% time ? 66-100% height)
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float phase3T = Mathf.Clamp01((elapsed - floatOutStartTime) / floatOutDuration);
            
            // Float from 66% to 100% height using quadratic ease-out
            float easedT = 1f - (1f - phase3T) * (1f - phase3T); // Quad ease-out for gentle float
            float currentHeight = 0.66f + (0.34f * easedT); // 66% ? 100%
            
            // Calculate current world position
            Vector3 currentWorldPos;
            if (isFollowingTarget && followTarget != null)
            {
                currentWorldPos = followTarget.position + targetWorldOffset + (Vector3.up * floatDistance * currentHeight);
            }
            else
            {
                currentWorldPos = startLocalPos + (Vector3.up * floatDistance * currentHeight);
            }
            
            currentWorldPosition = currentWorldPos; // Update tracked position
            UpdatePosition(currentWorldPos);
            
            // Fade out opacity (100% ? 0%)
            if (textComponent != null)
            {
                Color color = textComponent.color;
                color.a = 1f - phase3T; // Linear fade-out
                textComponent.color = color;
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
    
    /// <summary>
    /// Get the current world position of this callout
    /// </summary>
    public Vector3 GetCurrentPosition()
    {
        return currentWorldPosition;
    }
    
    /// <summary>
    /// Get the final world position of this callout (after animation completes)
    /// Used by stacking system to reserve space for the full animation
    /// </summary>
    public Vector3 GetFinalPosition()
    {
        // Update final position if following a moving target
        if (isFollowingTarget && followTarget != null)
        {
            return followTarget.position + targetWorldOffset + (Vector3.up * floatDistance);
        }
        
        return finalWorldPosition;
    }
    
    /// <summary>
    /// Check if this callout is following a specific target
    /// </summary>
    public bool IsFollowingTarget(Transform target)
    {
        return isFollowingTarget && followTarget == target;
    }
}
