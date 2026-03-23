using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a timer below the rock showing time from release to hog line.
/// Shows precise 3-decimal velocity on the rock itself.
/// Timer starts at (0:00.000) and stops at next hog line, lingering before fade-out.
/// </summary>
public class RockTimerDisplay : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Timer text displayed below rock")]
    public Text timerText;
    
    [Tooltip("Velocity text displayed on/near rock")]
    public Text velocityText;
    
    [Header("Timer Settings")]
    [Tooltip("Y offset below rock for timer display")]
    public float timerYOffset = -0.5f;
    
    [Tooltip("Y offset from rock for velocity display")]
    public float velocityYOffset = 0.3f;
    
    [Tooltip("How long timer lingers at hog line before disappearing")]
    public float lingerDuration = 2.0f;
    
    [Tooltip("Fade out duration after lingering")]
    public float fadeOutDuration = 0.5f;
    
    [Header("Hog Line Positions")]
    [Tooltip("Starting hog line Y position (near launcher)")]
    public float startHogLineY = -16f;
    
    [Tooltip("Ending hog line Y position (at house)")]
    public float endHogLineY = 15f;
    
    // State tracking
    private bool isTimerActive = false;
    private bool hasReachedHogLine = false;
    private float startTime = 0f;
    private float elapsedTime = 0f;
    private float lingerStartTime = 0f;
    private bool isLingering = false;
    
    // Cached references
    private Rigidbody2D rb;
    private Canvas parentCanvas;
    private Camera mainCamera;
    private RectTransform timerRect;
    private RectTransform velocityRect;
    
    void Awake()
    {
        // Get rock rigidbody
        rb = GetComponent<Rigidbody2D>();
        
        // Find main camera
        mainCamera = Camera.main;
        
        // Create UI if not assigned
        if (timerText == null || velocityText == null)
        {
            CreateTimerUI();
        }
        
        // Initially hide displays
        if (timerText != null) timerText.enabled = false;
        if (velocityText != null) velocityText.enabled = false;
    }
    
    /// <summary>
    /// Create timer and velocity UI elements
    /// </summary>
    private void CreateTimerUI()
    {
        // Find or create canvas
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Debug.LogWarning("[RockTimer] No Canvas found! Timer UI will not display.");
            return;
        }
        
        parentCanvas = canvasObj.GetComponent<Canvas>();
        
        // Create timer text GameObject
        GameObject timerObj = new GameObject("RockTimer");
        timerObj.transform.SetParent(parentCanvas.transform, false);
        
        timerRect = timerObj.AddComponent<RectTransform>();
        timerRect.sizeDelta = new Vector2(200, 50);
        
        timerText = timerObj.AddComponent<Text>();
        timerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        timerText.fontSize = 24;
        timerText.alignment = TextAnchor.MiddleCenter;
        timerText.color = Color.white;
        timerText.text = "(0:00.000)";
        timerText.enabled = false;
        
        // Add outline for readability
        Outline timerOutline = timerObj.AddComponent<Outline>();
        timerOutline.effectColor = Color.black;
        timerOutline.effectDistance = new Vector2(1, -1);
        
        // Create velocity text GameObject
        GameObject velocityObj = new GameObject("RockVelocity");
        velocityObj.transform.SetParent(parentCanvas.transform, false);
        
        velocityRect = velocityObj.AddComponent<RectTransform>();
        velocityRect.sizeDelta = new Vector2(150, 40);
        
        velocityText = velocityObj.AddComponent<Text>();
        velocityText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        velocityText.fontSize = 20;
        velocityText.alignment = TextAnchor.MiddleCenter;
        velocityText.color = new Color(0f, 1f, 1f, 1f); // Cyan
        velocityText.text = "0.000 m/s";
        velocityText.enabled = false;
        
        // Add outline for readability
        Outline velocityOutline = velocityObj.AddComponent<Outline>();
        velocityOutline.effectColor = Color.black;
        velocityOutline.effectDistance = new Vector2(1, -1);
        
        Debug.Log("[RockTimer] Timer and velocity UI created");
    }
    
    /// <summary>
    /// Start the timer when rock is released
    /// Called by Rock_Release or FlickShotController
    /// </summary>
    public void StartTimer()
    {
        if (timerText == null || velocityText == null)
        {
            Debug.LogWarning("[RockTimer] Timer or velocity text not initialized!");
            return;
        }
        
        isTimerActive = true;
        hasReachedHogLine = false;
        isLingering = false;
        startTime = Time.time;
        elapsedTime = 0f;
        
        timerText.enabled = true;
        velocityText.enabled = true;
        
        // Reset alpha
        Color timerColor = timerText.color;
        timerColor.a = 1f;
        timerText.color = timerColor;
        
        Color velColor = velocityText.color;
        velColor.a = 1f;
        velocityText.color = velColor;
        
        Debug.Log($"[RockTimer] Timer started at {startTime:F3}");
    }
    
    /// <summary>
    /// Stop the timer and start lingering phase
    /// </summary>
    private void StopTimer()
    {
        isTimerActive = false;
        hasReachedHogLine = true;
        isLingering = true;
        lingerStartTime = Time.time;
        
        Debug.Log($"[RockTimer] Timer stopped at {elapsedTime:F3}s, starting linger phase");
    }
    
    /// <summary>
    /// Hide timer and velocity displays
    /// </summary>
    public void HideTimer()
    {
        if (timerText != null) timerText.enabled = false;
        if (velocityText != null) velocityText.enabled = false;
        
        isTimerActive = false;
        hasReachedHogLine = false;
        isLingering = false;
    }
    
    void Update()
    {
        if (rb == null || timerText == null || velocityText == null)
            return;
        
        // Update timer if active
        if (isTimerActive)
        {
            elapsedTime = Time.time - startTime;
            
            // Format as (M:SS.mmm)
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            float seconds = elapsedTime % 60f;
            timerText.text = $"({minutes}:{seconds:00.000})";
            
            // Update velocity (3 decimals)
            float velocity = rb.linearVelocity.magnitude;
            velocityText.text = $"{velocity:F3} m/s";
            
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
        else if (isLingering)
        {
            // Linger phase - keep displaying but start fading after linger duration
            float lingerElapsed = Time.time - lingerStartTime;
            
            if (lingerElapsed < lingerDuration)
            {
                // Still lingering - no fade yet
                // Keep displaying final time and velocity
            }
            else
            {
                // Start fading out
                float fadeElapsed = lingerElapsed - lingerDuration;
                float fadeProgress = Mathf.Clamp01(fadeElapsed / fadeOutDuration);
                float alpha = 1f - fadeProgress;
                
                // Fade both timer and velocity
                Color timerColor = timerText.color;
                timerColor.a = alpha;
                timerText.color = timerColor;
                
                Color velColor = velocityText.color;
                velColor.a = alpha;
                velocityText.color = velColor;
                
                // Fully hidden - disable
                if (fadeProgress >= 1f)
                {
                    HideTimer();
                }
            }
        }
        
        // Update UI positions to follow rock
        UpdateUIPositions();
    }
    
    /// <summary>
    /// Update UI element positions to follow rock
    /// </summary>
    private void UpdateUIPositions()
    {
        if (parentCanvas == null || mainCamera == null || rb == null)
            return;
        
        // Get rock world position
        Vector3 rockWorldPos = rb.position;
        
        // Update timer position (below rock)
        if (timerText != null && timerText.enabled && timerRect != null)
        {
            Vector3 timerWorldPos = rockWorldPos + Vector3.up * timerYOffset;
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(mainCamera, timerWorldPos);
            
            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                screenPos,
                mainCamera,
                out canvasPos
            );
            
            timerRect.anchoredPosition = canvasPos;
        }
        
        // Update velocity position (above rock)
        if (velocityText != null && velocityText.enabled && velocityRect != null)
        {
            Vector3 velocityWorldPos = rockWorldPos + Vector3.up * velocityYOffset;
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(mainCamera, velocityWorldPos);
            
            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                screenPos,
                mainCamera,
                out canvasPos
            );
            
            velocityRect.anchoredPosition = canvasPos;
        }
    }
    
    void OnDisable()
    {
        // Clean up when rock is disabled
        HideTimer();
    }
}
