using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a timer below the rock showing time from release to hog line.
/// Shows precise 3-decimal velocity on the rock itself.
/// Timer starts at (0:00.000) and stops at next hog line, lingering before fade-out.
/// Uses the existing TextCallout system for UI display.
/// </summary>
public class RockTimerDisplay : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Y offset below rock for timer display (in world units)")]
    public float timerYOffset = -0.5f;
    
    [Tooltip("Y offset from rock for velocity display (in world units)")]
    public float velocityYOffset = 0.3f;
    
    [Tooltip("How long timer lingers at hog line before disappearing")]
    public float lingerDuration = 2.0f;
    
    [Tooltip("Fade out duration after lingering")]
    public float fadeOutDuration = 0.1f;
    
    [Header("Hog Line Positions")]
    [Tooltip("Starting hog line Y position (near launcher)")]
    public float startHogLineY = -16f;
    
    [Tooltip("Ending hog line Y position (at house)")]
    public float endHogLineY = 15f;
    
    [Header("Text Appearance")]
    [Tooltip("Timer text color")]
    public Color timerColor = Color.white;
    
    [Tooltip("Velocity text color")]
    public Color velocityColor = new Color(0f, 1f, 1f, 1f); // Cyan
    
    [Tooltip("Timer font size")]
    public float timerFontSize = 24f;
    
    [Tooltip("Velocity font size")]
    public float velocityFontSize = 20f;
    
    // State tracking
    private bool isTimerActive = false;
    private bool hasReachedHogLine = false;
    private float startTime = 0f;
    private float elapsedTime = 0f;
    
    // Cached references
    private Rigidbody2D rb;
    private TextCallout timerCallout;
    private TextCallout velocityCallout;
    
    void Awake()
    {
        // Get rock rigidbody
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            Debug.LogError("[RockTimerDisplay] No Rigidbody2D found on rock!");
        }
    }
    
    /// <summary>
    /// Start the timer when rock is released
    /// Called by Rock_Release or FlickShotController
    /// </summary>
    public void StartTimer()
    {
        if (TextCalloutManager.Instance == null)
        {
            Debug.LogWarning("[RockTimer] TextCalloutManager not found!");
            return;
        }
        
        if (rb == null)
        {
            Debug.LogWarning("[RockTimer] Rigidbody2D not found!");
            return;
        }
        
        isTimerActive = true;
        hasReachedHogLine = false;
        startTime = Time.time;
        elapsedTime = 0f;
        
        // Create persistent callouts that follow the rock
        // Use very long duration to keep them alive, we'll manually stop them
        Vector3 rockPos = rb.position;
        
        // Timer callout (below rock)
        timerCallout = TextCalloutManager.Instance.ShowCallout(
            targetPosition: rockPos + Vector3.up * timerYOffset,
            text: "(0:00.000)",
            followTarget: true,
            target: transform,
            duration: 999f, // Very long duration - we'll manually stop it
            floatDistance: 0f, // Don't float - stay fixed relative to rock
            textColor: timerColor,
            fontSize: timerFontSize,
            fadeDuration: fadeOutDuration
        );
        
        // Velocity callout (above/on rock)
        velocityCallout = TextCalloutManager.Instance.ShowCallout(
            targetPosition: rockPos + Vector3.up * velocityYOffset,
            text: "0.000 m/s",
            followTarget: true,
            target: transform,
            duration: 999f, // Very long duration - we'll manually stop it
            floatDistance: 0f, // Don't float - stay fixed relative to rock
            textColor: velocityColor,
            fontSize: velocityFontSize,
            fadeDuration: fadeOutDuration
        );
        
        // CRITICAL FIX: Set text to full opacity immediately after creation
        // The callout animation starts at alpha=0 and fades in, but with floatDistance=0
        // and very long duration, we need to force full visibility for persistent display
        StartCoroutine(ForceCalloutVisibility());
        
        Debug.Log($"[RockTimer] Timer started at {startTime:F3}");
    }
    
    /// <summary>
    /// Force callout visibility after creation
    /// Needed because TextCallout starts at alpha=0 and with floatDistance=0,
    /// the fade-in doesn't work properly for persistent displays
    /// </summary>
    private System.Collections.IEnumerator ForceCalloutVisibility()
    {
        // Wait one frame for callouts to be fully initialized
        yield return null;
        
        // Force timer callout to full opacity and set to persistent mode
        if (timerCallout != null)
        {
            // Stop the animation coroutine
            timerCallout.StopAllCoroutines();
            
            // Set to full opacity
            Text timerTextComp = timerCallout.GetComponent<Text>();
            if (timerTextComp != null)
            {
                Color color = timerColor;
                color.a = 1f;
                timerTextComp.color = color;
                Debug.Log($"[RockTimer] Timer callout forced to full opacity: {color}");
            }
            
            // Start persistent follow coroutine to keep it following the rock
            StartCoroutine(FollowRockPersistent(timerCallout, timerYOffset));
        }
        
        // Force velocity callout to full opacity and set to persistent mode
        if (velocityCallout != null)
        {
            // Stop the animation coroutine
            velocityCallout.StopAllCoroutines();
            
            // Set to full opacity
            Text velocityTextComp = velocityCallout.GetComponent<Text>();
            if (velocityTextComp != null)
            {
                Color color = velocityColor;
                color.a = 1f;
                velocityTextComp.color = color;
                Debug.Log($"[RockTimer] Velocity callout forced to full opacity: {color}");
            }
            
            // Start persistent follow coroutine to keep it following the rock
            StartCoroutine(FollowRockPersistent(velocityCallout, velocityYOffset));
        }
    }
    
    /// <summary>
    /// Coroutine to keep a callout following the rock without animation
    /// </summary>
    private System.Collections.IEnumerator FollowRockPersistent(TextCallout callout, float yOffset)
    {
        if (callout == null || rb == null) yield break;
        
        // Get the UpdatePosition method via reflection
        System.Type calloutType = callout.GetType();
        System.Reflection.MethodInfo updatePosMethod = calloutType.GetMethod("UpdatePosition", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (updatePosMethod == null)
        {
            Debug.LogError("[RockTimer] Could not find UpdatePosition method on TextCallout!");
            yield break;
        }
        
        // Keep updating position every frame while callout exists
        while (callout != null && callout.gameObject != null && callout.gameObject.activeInHierarchy)
        {
            // Calculate world position relative to rock
            Vector3 worldPos = (Vector3)rb.position + Vector3.up * yOffset;
            
            // Call UpdatePosition using reflection
            updatePosMethod.Invoke(callout, new object[] { worldPos });
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Stop the timer and trigger linger/fade-out
    /// </summary>
    private void StopTimer()
    {
        isTimerActive = false;
        hasReachedHogLine = true;
        
        Debug.Log($"[RockTimer] Timer stopped at {elapsedTime:F3}s");
        
        // Start coroutine to handle linger and fade
        StartCoroutine(LingerAndFade());
    }
    
    /// <summary>
    /// Handle lingering at hog line then fading out
    /// </summary>
    private System.Collections.IEnumerator LingerAndFade()
    {
        // Linger phase - keep displaying at full opacity
        yield return new WaitForSeconds(lingerDuration);
        
        // Fade out phase
        float fadeElapsed = 0f;
        Color timerStartColor = timerColor;
        Color velocityStartColor = velocityColor;
        timerStartColor.a = 1f;
        velocityStartColor.a = 1f;
        
        while (fadeElapsed < fadeOutDuration)
        {
            fadeElapsed += Time.deltaTime;
            float alpha = 1f - (fadeElapsed / fadeOutDuration);
            
            // Fade timer
            if (timerCallout != null)
            {
                Text timerTextComp = timerCallout.GetComponent<Text>();
                if (timerTextComp != null)
                {
                    Color color = timerStartColor;
                    color.a = alpha;
                    timerTextComp.color = color;
                }
            }
            
            // Fade velocity
            if (velocityCallout != null)
            {
                Text velocityTextComp = velocityCallout.GetComponent<Text>();
                if (velocityTextComp != null)
                {
                    Color color = velocityStartColor;
                    color.a = alpha;
                    velocityTextComp.color = color;
                }
            }
            
            yield return null;
        }
        
        // Fully faded - hide the callouts
        HideTimer();
    }
    
    /// <summary>
    /// Hide timer and velocity displays
    /// </summary>
    public void HideTimer()
    {
        if (timerCallout != null)
        {
            timerCallout.ForceStop();
            timerCallout = null;
        }
        
        if (velocityCallout != null)
        {
            velocityCallout.ForceStop();
            velocityCallout = null;
        }
        
        isTimerActive = false;
        hasReachedHogLine = false;
    }
    
    void Update()
    {
        if (rb == null)
            return;
        
        // Update timer if active
        if (isTimerActive)
        {
            elapsedTime = Time.time - startTime;
            
            // Format as (M:SS.mmm)
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            float seconds = elapsedTime % 60f;
            string timerText = $"({minutes}:{seconds:00.000})";
            
            // Update velocity (3 decimals)
            float velocity = rb.linearVelocity.magnitude;
            string velocityText = $"{velocity:F3} m/s";
            
            // Update callout texts if they exist
            UpdateCalloutText(timerCallout, timerText);
            UpdateCalloutText(velocityCallout, velocityText);
            
            // Check if reached hog line
            float rockY = rb.position.y;
            
            // Determine which hog line we're approaching based on direction
            bool movingUp = rb.linearVelocity.y > 0;
            float targetHogLineY = movingUp ? endHogLineY : startHogLineY;
            
            // Stop timer when crossing hog line
            if ((movingUp && rockY >= targetHogLineY) || (!movingUp && rockY <= targetHogLineY))
            {
                StopTimer();
            }
        }
    }
    
    /// <summary>
    /// Update the text of a callout without recreating it
    /// </summary>
    private void UpdateCalloutText(TextCallout callout, string newText)
    {
        if (callout == null || callout.gameObject == null)
            return;
        
        Text textComponent = callout.GetComponent<Text>();
        if (textComponent != null)
        {
            textComponent.text = newText;
        }
    }
    
    void OnDisable()
    {
        // Clean up when rock is disabled
        HideTimer();
    }
}
