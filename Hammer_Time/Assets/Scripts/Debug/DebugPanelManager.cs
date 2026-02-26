using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages the debug panel display - shows real-time rock physics and shot information
/// Only active when Debug Mode is enabled in GameSettings
/// 
/// USAGE:
/// 1. Attach to a Canvas GameObject in your scene
/// 2. Assign the 5 TextMeshProUGUI components in the Inspector:
///    - rockInfoText: "Rock #X at (x, y, z)"
///    - shotInfoText: "Shot Type: Takeout | Target: Rock #Y at (x, y)"
///    - velocityText: "Velocity: (x, y) | Speed: X.XX m/s"
///    - linearForceText: "Linear Forces: (x, y)"
///    - angularForceText: "Angular Force: X.XX | Rotation: X.XX°"
/// 3. Panel will auto-show/hide based on GameSettings.debugMode
/// 
/// UPDATE FREQUENCY: Every 0.05 seconds (20 times per second) for smooth updates without performance impact
/// </summary>
public class DebugPanelManager : MonoBehaviour
{
    [Header("Text Components (assign in Inspector)")]
    [Tooltip("Displays current rock number and position")]
    public TextMeshPro rockInfoText;
    
    [Tooltip("Displays shot type and target info")]
    public TextMeshPro shotInfoText;
    
    [Tooltip("Displays velocity vector and speed")]
    public TextMeshPro velocityText;
    
    [Tooltip("Displays linear forces (X, Y)")]
    public TextMeshPro linearForceText;
    
    [Tooltip("Displays angular force and rotation")]
    public TextMeshPro angularForceText;
    
    [Header("References (auto-found)")]
    private GameManager gm;
    private GameSettingsPersist gsp;
    private Canvas debugCanvas;
    
    [Header("Update Settings")]
    [Tooltip("How often to update the debug panel (seconds)")]
    [Range(0.01f, 0.5f)]
    public float updateInterval = 0.05f; // 20 updates per second
    
    private float updateTimer = 0f;
    
    // Cached data for comparison (avoid GC allocations)
    private int lastRockIndex = -1;
    private Vector3 lastRockPos = Vector3.zero;
    private Vector2 lastVelocity = Vector2.zero;
    private Vector2 lastLinearForce = Vector2.zero;
    private float lastAngularForce = 0f;
    private float lastRotation = 0f;
    
    void Start()
    {
        // Find references
        gm = FindObjectOfType<GameManager>();
        gsp = FindObjectOfType<GameSettingsPersist>();
        debugCanvas = GetComponent<Canvas>();
        
        if (gm == null)
        {
            Debug.LogWarning("[DebugPanel] GameManager not found - debug panel will not function!");
        }
        
        if (gsp == null)
        {
            Debug.LogWarning("[DebugPanel] GameSettings not found - assuming debug mode OFF");
        }
        
        if (debugCanvas == null)
        {
            Debug.LogWarning("[DebugPanel] Canvas component not found on this GameObject!");
        }
        
        // Validate text components
        ValidateTextComponents();
        
        // Initialize panel visibility
        UpdatePanelVisibility();
    }
    
    void Update()
    {
        // Check if debug mode is enabled
        if (gsp != null && !gsp.debug)
        {
            if (debugCanvas != null && debugCanvas.enabled)
            {
                debugCanvas.enabled = false;
            }
            return;
        }
        
        // Enable canvas if debug mode is on
        if (debugCanvas != null && !debugCanvas.enabled)
        {
            debugCanvas.enabled = true;
        }
        
        // Update on interval (not every frame - performance optimization)
        updateTimer += Time.deltaTime;
        
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateAllDebugTexts();
        }
    }
    
    /// <summary>
    /// Update panel visibility based on debug mode setting
    /// </summary>
    private void UpdatePanelVisibility()
    {
        if (debugCanvas == null) return;
        
        bool shouldShow = (gsp != null && gsp.debug);
        debugCanvas.enabled = shouldShow;
        
        if (shouldShow)
        {
            Debug.Log("[DebugPanel] Debug mode ENABLED - panel visible");
        }
    }
    
    /// <summary>
    /// Validate that all text components are assigned
    /// </summary>
    private void ValidateTextComponents()
    {
        int missingCount = 0;
        
        if (rockInfoText == null)
        {
            Debug.LogWarning("[DebugPanel] rockInfoText not assigned!");
            missingCount++;
        }
        
        if (shotInfoText == null)
        {
            Debug.LogWarning("[DebugPanel] shotInfoText not assigned!");
            missingCount++;
        }
        
        if (velocityText == null)
        {
            Debug.LogWarning("[DebugPanel] velocityText not assigned!");
            missingCount++;
        }
        
        if (linearForceText == null)
        {
            Debug.LogWarning("[DebugPanel] linearForceText not assigned!");
            missingCount++;
        }
        
        if (angularForceText == null)
        {
            Debug.LogWarning("[DebugPanel] angularForceText not assigned!");
            missingCount++;
        }
        
        if (missingCount > 0)
        {
            Debug.LogError($"[DebugPanel] {missingCount}/5 text components missing - assign them in Inspector!");
        }
        else
        {
            Debug.Log("[DebugPanel] All text components assigned ✓");
        }
    }
    
    /// <summary>
    /// Update all debug text fields with current rock data
    /// </summary>
    private void UpdateAllDebugTexts()
    {
        if (gm == null) return;
        
        // Get current rock index
        int currentRockIndex = gm.rockCurrent;
        
        // Check if we have a valid rock
        if (currentRockIndex < 0 || currentRockIndex >= gm.rockList.Count)
        {
            // No active rock - clear display
            ClearAllTexts();
            return;
        }
        
        var rockEntry = gm.rockList[currentRockIndex];
        
        if (rockEntry.rock == null || !rockEntry.rock.activeInHierarchy)
        {
            // Rock not active - clear display
            ClearAllTexts();
            return;
        }
        
        // Get rock components
        GameObject currentRock = rockEntry.rock;
        Rock_Info rockInfo = rockEntry.rockInfo;
        Rigidbody2D rb = currentRock.GetComponent<Rigidbody2D>();
        Rock_Force rockForce = currentRock.GetComponent<Rock_Force>();
        
        // Update each text field
        UpdateRockInfoText(currentRockIndex, currentRock, rockInfo);
        UpdateShotInfoText(currentRockIndex, rockInfo);
        UpdateVelocityText(rb);
        UpdateLinearForceText(rockForce);
        UpdateAngularForceText(rockForce, currentRock);
    }
    
    /// <summary>
    /// Update rock info: "Rock #X (Team Name) at (x, y, z)"
    /// </summary>
    private void UpdateRockInfoText(int rockIndex, GameObject rock, Rock_Info rockInfo)
    {
        if (rockInfoText == null) return;
        
        Vector3 position = rock.transform.position;
        
        // Only update if changed (avoid string allocation every frame)
        if (rockIndex != lastRockIndex || position != lastRockPos)
        {
            lastRockIndex = rockIndex;
            lastRockPos = position;
            
            string teamName = rockInfo != null ? rockInfo.teamName : "Unknown";
            int rockNumber = rockInfo != null ? rockInfo.rockNumber : 0;
            
            rockInfoText.text = $"<b>Rock #{rockIndex}</b> ({teamName} #{rockNumber})\n" +
                                $"Position: ({position.x:F2}, {position.y:F2}, {position.z:F2})";
        }
    }
    
    /// <summary>
    /// Update shot info: "Shot Type: Takeout | Target: Rock #Y at (x, y)"
    /// </summary>
    private void UpdateShotInfoText(int rockIndex, Rock_Info rockInfo)
    {
        if (shotInfoText == null) return;
        
        // Get shot type from AI_Strategy or player input
        string shotType = "Unknown";
        string targetInfo = "No Target";
        
        // Try to get shot type from various sources
        AIManager aiManager = FindObjectOfType<AIManager>();
        AI_Strategy aiStrategy = FindObjectOfType<AI_Strategy>();
        
        if (aiStrategy != null)
        {
            // Check if AI is shooting this rock
            bool isAIRock = rockInfo != null && 
                            ((rockInfo.teamName == "Test Team Red" && gm.aiTeamRed) ||
                             (rockInfo.teamName == "Test Team Yellow" && gm.aiTeamYellow));
            
            if (isAIRock)
            {
                // AI is shooting - try to get intent
                shotType = "AI Shot"; // Default
                
                // You could add more detailed shot tracking here if needed
                // For now, just show it's an AI shot
            }
            else
            {
                shotType = "Player Shot";
            }
        }
        
        // Try to get target rock info (if applicable)
        // Check if there's a target rock being aimed at
        AI_Target aiTarget = FindObjectOfType<AI_Target>();
        if (aiTarget != null && aiTarget.aiTarget != null)
        {
            Vector3 targetPos = aiTarget.aiTarget.position;
            
            // Find which rock is being targeted (if any)
            GameObject targetRock = FindRockNearPosition(targetPos);
            
            if (targetRock != null)
            {
                Rock_Info targetRockInfo = targetRock.GetComponent<Rock_Info>();
                if (targetRockInfo != null)
                {
                    targetInfo = $"Rock #{targetRockInfo.rockIndex} ({targetRockInfo.teamName}) at ({targetPos.x:F2}, {targetPos.y:F2})";
                }
                else
                {
                    targetInfo = $"Position ({targetPos.x:F2}, {targetPos.y:F2})";
                }
            }
            else
            {
                targetInfo = $"Position ({targetPos.x:F2}, {targetPos.y:F2})";
            }
        }
        
        shotInfoText.text = $"<b>Shot Type:</b> {shotType}\n" +
                            $"<b>Target:</b> {targetInfo}";
    }
    
    /// <summary>
    /// Update velocity: "Velocity: (x, y) | Speed: X.XX m/s | Direction: X°"
    /// </summary>
    private void UpdateVelocityText(Rigidbody2D rb)
    {
        if (velocityText == null) return;
        
        if (rb == null)
        {
            velocityText.text = "<b>Velocity:</b> No Rigidbody2D";
            return;
        }
        
        Vector2 velocity = rb.linearVelocity;
        
        // Only update if changed significantly (avoid micro-updates)
        if (Vector2.Distance(velocity, lastVelocity) > 0.01f)
        {
            lastVelocity = velocity;
            
            float speed = velocity.magnitude;
            float direction = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            
            // Color code based on speed
            string speedColor = speed > 5f ? "#00FF00" : speed > 2f ? "#FFFF00" : "#888888";
            
            velocityText.text = $"<b>Velocity:</b> ({velocity.x:F2}, {velocity.y:F2})\n" +
                                $"<b>Speed:</b> <color={speedColor}>{speed:F2} m/s</color> | <b>Direction:</b> {direction:F1}°";
        }
    }
    
    /// <summary>
    /// Update linear forces: "Linear Force: (x, y) | Magnitude: X.XX N"
    /// </summary>
    private void UpdateLinearForceText(Rock_Force rockForce)
    {
        if (linearForceText == null) return;
        
        if (rockForce == null)
        {
            linearForceText.text = "<b>Linear Force:</b> No Rock_Force component";
            return;
        }
        
        // Get current linear force from Rock_Force
        // Rock_Force doesn't expose force directly, so we'll show friction/ice state
        Rigidbody2D rb = rockForce.GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            linearForceText.text = "<b>Linear Force:</b> No Rigidbody2D";
            return;
        }
        
        // Calculate net force from current physics state
        // Force = mass × acceleration, but we can approximate from drag
        Vector2 currentForce = rb.linearVelocity * rb.linearDamping; // Approximation
        
        if (Vector2.Distance(currentForce, lastLinearForce) > 0.01f)
        {
            lastLinearForce = currentForce;
            
            float magnitude = currentForce.magnitude;
            
            linearForceText.text = $"<b>Linear Force:</b> ({currentForce.x:F2}, {currentForce.y:F2})\n" +
                                   $"<b>Magnitude:</b> {magnitude:F2} N | <b>Drag:</b> {rb.linearDamping:F3}";
        }
    }
    
    /// <summary>
    /// Update angular forces: "Angular Force: X.XX N·m | Rotation: X.XX° | Angular Vel: X.XX rad/s"
    /// </summary>
    private void UpdateAngularForceText(Rock_Force rockForce, GameObject rock)
    {
        if (angularForceText == null) return;
        
        if (rockForce == null)
        {
            angularForceText.text = "<b>Angular Force:</b> No Rock_Force component";
            return;
        }
        
        Rigidbody2D rb = rockForce.GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            angularForceText.text = "<b>Angular Force:</b> No Rigidbody2D";
            return;
        }
        
        // Get rotation and angular velocity
        float rotation = rock.transform.rotation.eulerAngles.z;
        float angularVelocity = rb.angularVelocity;
        float angularDrag = rb.angularDamping;
        
        // Approximate angular force from drag
        float angularForce = angularVelocity * angularDrag;
        
        if (Mathf.Abs(angularForce - lastAngularForce) > 0.01f || Mathf.Abs(rotation - lastRotation) > 0.1f)
        {
            lastAngularForce = angularForce;
            lastRotation = rotation;
            
            // Color code based on spin rate
            string spinColor = Mathf.Abs(angularVelocity) > 10f ? "#00FF00" : 
                               Mathf.Abs(angularVelocity) > 5f ? "#FFFF00" : "#888888";
            
            angularForceText.text = $"<b>Angular Force:</b> {angularForce:F2} N·m\n" +
                                    $"<b>Rotation:</b> {rotation:F1}° | <b>Angular Vel:</b> <color={spinColor}>{angularVelocity:F2} rad/s</color>";
        }
    }
    
    /// <summary>
    /// Clear all text fields (no active rock)
    /// </summary>
    private void ClearAllTexts()
    {
        if (rockInfoText != null)
            rockInfoText.text = "<b>Rock Info:</b> No active rock";
        
        if (shotInfoText != null)
            shotInfoText.text = "<b>Shot Info:</b> Waiting for shot...";
        
        if (velocityText != null)
            velocityText.text = "<b>Velocity:</b> N/A";
        
        if (linearForceText != null)
            linearForceText.text = "<b>Linear Force:</b> N/A";
        
        if (angularForceText != null)
            angularForceText.text = "<b>Angular Force:</b> N/A";
    }
    
    /// <summary>
    /// Helper: Find the rock closest to a target position
    /// </summary>
    private GameObject FindRockNearPosition(Vector3 position)
    {
        if (gm == null) return null;
        
        GameObject closestRock = null;
        float closestDist = 0.5f; // Within 0.5 units
        
        foreach (var rockEntry in gm.rockList)
        {
            if (rockEntry.rock == null || !rockEntry.rock.activeInHierarchy)
                continue;
            
            float dist = Vector3.Distance(rockEntry.rock.transform.position, position);
            
            if (dist < closestDist)
            {
                closestDist = dist;
                closestRock = rockEntry.rock;
            }
        }
        
        return closestRock;
    }
    
    /// <summary>
    /// PUBLIC API: Force immediate update (useful for testing)
    /// </summary>
    public void ForceUpdate()
    {
        UpdateAllDebugTexts();
    }
    
    /// <summary>
    /// PUBLIC API: Toggle debug panel visibility
    /// </summary>
    public void TogglePanel(bool visible)
    {
        if (debugCanvas != null)
        {
            debugCanvas.enabled = visible;
        }
    }
}
