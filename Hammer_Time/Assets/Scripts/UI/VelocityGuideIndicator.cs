using System.Collections;
using UnityEngine;

/// <summary>
/// Visual guide that shows the player the correct swipe velocity to match.
/// Animates a line from launcher to hogline at the predicted velocity,
/// helping players calibrate their swipe speed.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class VelocityGuideIndicator : MonoBehaviour
{
    [Header("References")]
    public Transform launcher; // Start point (0, -24)
    public Transform hogline;  // End point reference
    
    [Header("Animation Settings")]
    [Tooltip("Time to pause at hogline before restarting (seconds)")]
    public float pauseDuration = 1.5f;
    
    [Tooltip("Duration of fade-out effect at end of pause (seconds)")]
    public float fadeOutDuration = 0.5f;
    
    [Tooltip("How thick the line appears (world units)")]
    public float lineWidth = 0.15f;
    
    [Header("Position Settings")]
    [Tooltip("Y position of bottom endpoint (launcher position)")]
    public float startY = -24.66f;
    
    [Tooltip("Y position of top endpoint (hogline position)")]
    public float endY = -16.5f;
    
    // Components
    private LineRenderer lineRenderer;
    private Coroutine animationCoroutine;
    
    // State
    private float targetVelocity;
    private bool isActive;
    private Color currentColor;
    
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }
    
    /// <summary>
    /// Setup LineRenderer with default settings
    /// </summary>
    private void SetupLineRenderer()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
        // CRITICAL: Set sorting layer to render above background
        lineRenderer.sortingLayerName = "Default"; // Use same layer as other UI elements
        lineRenderer.sortingOrder = 100; // High order = renders on top (above bg, rocks, etc.)
        
        // Start hidden
        lineRenderer.enabled = false;
        
        // Set initial positions
        lineRenderer.SetPosition(0, new Vector3(0f, startY, 0f));
        lineRenderer.SetPosition(1, new Vector3(0f, startY, 0f));
        
        Debug.Log($"[VelocityGuide] LineRenderer sorting: Layer='{lineRenderer.sortingLayerName}', Order={lineRenderer.sortingOrder}");
    }
    
    /// <summary>
    /// Start the velocity guide animation
    /// </summary>
    /// <param name="velocity">Predicted velocity in m/s</param>
    /// <param name="color">Color to use for the guide line (from shooting knob)</param>
    public void StartGuide(float velocity, Color color)
    {
        targetVelocity = velocity;
        currentColor = color;
        
        // Update line color (static, no cycling)
        lineRenderer.startColor = currentColor;
        lineRenderer.endColor = currentColor;
        
        // Enable and start animation
        isActive = true;
        lineRenderer.enabled = true;
        
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        animationCoroutine = StartCoroutine(AnimateVelocityGuide());
        
        Debug.Log($"[VelocityGuide] Started - Velocity: {velocity:F2} m/s, Color: {color}");
    }
    
    /// <summary>
    /// Stop the velocity guide animation
    /// </summary>
    public void StopGuide()
    {
        isActive = false;
        
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        
        lineRenderer.enabled = false;
        
        Debug.Log("[VelocityGuide] Stopped");
    }
    
    /// <summary>
    /// Main animation loop - moves top endpoint from start to end at target velocity
    /// </summary>
    private IEnumerator AnimateVelocityGuide()
    {
        while (isActive)
        {
            // Reset to start position
            Vector3 startPos = new Vector3(0f, startY, 0f);
            Vector3 endPos = new Vector3(0f, startY, 0f);
            
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
            
            // Animate top endpoint moving upward at target velocity
            float distance = endY - startY; // e.g., -16.5 - (-24.66) = 8.16 units
            float duration = distance / targetVelocity; // Time to travel at velocity
            
            float elapsed = 0f;
            
            while (elapsed < duration && isActive)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration; // 0 to 1
                
                // Bottom stays at start, top moves to end
                float currentTopY = Mathf.Lerp(startY, endY, t);
                endPos = new Vector3(0f, currentTopY, 0f);
                
                lineRenderer.SetPosition(0, startPos); // Bottom stays fixed
                lineRenderer.SetPosition(1, endPos);   // Top moves
                
                yield return null;
            }
            
            // Ensure we reach final position
            if (isActive)
            {
                endPos = new Vector3(0f, endY, 0f);
                lineRenderer.SetPosition(0, startPos);
                lineRenderer.SetPosition(1, endPos);
                
                // Pause at hogline with fade-out
                // PHASE 1: Hold at full opacity for (pauseDuration - fadeOutDuration) seconds
                float holdDuration = pauseDuration - fadeOutDuration;
                if (holdDuration > 0f)
                {
                    yield return new WaitForSeconds(holdDuration);
                }
                
                // PHASE 2: Fade out over fadeOutDuration seconds
                if (isActive && fadeOutDuration > 0f)
                {
                    float fadeElapsed = 0f;
                    Color fullColor = currentColor;
                    
                    while (fadeElapsed < fadeOutDuration && isActive)
                    {
                        fadeElapsed += Time.deltaTime;
                        float fadeT = fadeElapsed / fadeOutDuration; // 0 to 1
                        
                        // Fade alpha from full to zero
                        Color fadedColor = fullColor;
                        fadedColor.a = Mathf.Lerp(fullColor.a, 0f, fadeT);
                        
                        lineRenderer.startColor = fadedColor;
                        lineRenderer.endColor = fadedColor;
                        
                        yield return null;
                    }
                    
                    // Restore full color for next loop
                    lineRenderer.startColor = fullColor;
                    lineRenderer.endColor = fullColor;
                }
            }
            
            // Loop continues...
        }
    }
    
    /// <summary>
    /// Update the target velocity during animation (if shot parameters change)
    /// </summary>
    public void UpdateVelocity(float newVelocity)
    {
        if (isActive && Mathf.Abs(targetVelocity - newVelocity) > 0.1f)
        {
            targetVelocity = newVelocity;
            Debug.Log($"[VelocityGuide] Updated velocity to {newVelocity:F2} m/s");
            
            // Restart animation with new velocity
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AnimateVelocityGuide());
        }
    }
    
    /// <summary>
    /// Check if guide is currently active
    /// </summary>
    public bool IsActive => isActive;
}
