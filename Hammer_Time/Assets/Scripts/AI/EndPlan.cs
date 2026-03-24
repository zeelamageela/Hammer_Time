using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shot execution result - tracks how well a shot performed
/// </summary>
public class ShotResult
{
    public bool success;                    // Did shot achieve its goal?
    public float deviationFromTarget;       // How far off target (meters)
    public ShotIntent attemptedIntent;      // What we tried to do
    public string failureReason;            // Why it failed (if applicable)
    public Vector2 actualFinalPosition;     // Where rock ended up
    public Vector2 intendedPosition;        // Where we wanted it
    
    public ShotResult()
    {
        success = true;
        deviationFromTarget = 0f;
        failureReason = "";
    }
}

/// <summary>
/// Represents a multi-shot strategic plan for an end
/// AI will plan 2-3 shots ahead based on game situation
/// </summary>
public class EndPlan
{
    /// <summary>
    /// Sequence of intents to execute (e.g., [Guard, Draw, Protect])
    /// </summary>
    public List<ShotIntent> plannedIntents;
    
    /// <summary>
    /// Human-readable strategy name for debugging
    /// </summary>
    public string strategyName;
    
    /// <summary>
    /// Confidence in this plan (0-1)
    /// Based on:
    /// - Character skills
    /// - Current game state
    /// - Risk factors
    /// </summary>
    public float confidence;
    
    /// <summary>
    /// Which shot in the plan are we currently executing? (0-based)
    /// </summary>
    public int currentStep;
    
    /// <summary>
    /// Rock number when plan was created
    /// </summary>
    public int planCreatedAtRock;
    
    /// <summary>
    /// Is plan still valid or needs re-evaluation?
    /// </summary>
    public bool isValid;
    
    /// <summary>
    /// Why was this plan chosen? (for debug logs)
    /// </summary>
    public string reasoning;
    
    /// <summary>
    /// Expected outcome if plan succeeds
    /// </summary>
    public string expectedOutcome;
    
    /// <summary>
    /// Target positions for each shot (optional - for draws/guards)
    /// </summary>
    public List<Vector2> targetPositions;
    
    /// <summary>
    /// Target rocks for each shot (optional - for takeouts)
    /// -1 means no specific target
    /// </summary>
    public List<int> targetRocks;
    
    /// <summary>
    /// ?? NEW: Shot execution history - tracks results of executed shots
    /// </summary>
    public List<ShotResult> executionHistory;
    
    /// <summary>
    /// ?? NEW: Number of failed shots in this plan
    /// </summary>
    public int failedShotsCount;
    
    public EndPlan()
    {
        plannedIntents = new List<ShotIntent>();
        targetPositions = new List<Vector2>();
        targetRocks = new List<int>();
        executionHistory = new List<ShotResult>();
        currentStep = 0;
        isValid = true;
        confidence = 0.5f;
        failedShotsCount = 0;
    }
    
    /// <summary>
    /// Get the intent for current step
    /// </summary>
    public ShotIntent GetCurrentIntent()
    {
        if (currentStep >= 0 && currentStep < plannedIntents.Count)
            return plannedIntents[currentStep];
        
        return ShotIntent.ScorePoints; // Fallback
    }
    
    /// <summary>
    /// Get target position for current step (if applicable)
    /// </summary>
    public Vector2 GetCurrentTargetPosition()
    {
        if (currentStep >= 0 && currentStep < targetPositions.Count)
            return targetPositions[currentStep];
        
        return new Vector2(0f, 6.5f); // Button as fallback
    }
    
    /// <summary>
    /// Get target rock for current step (if applicable)
    /// </summary>
    public int GetCurrentTargetRock()
    {
        if (currentStep >= 0 && currentStep < targetRocks.Count)
            return targetRocks[currentStep];
        
        return -1; // No target
    }
    
    /// <summary>
    /// Advance to next step in plan
    /// </summary>
    public void AdvanceStep()
    {
        currentStep++;
        
        if (currentStep >= plannedIntents.Count)
        {
            isValid = false; // Plan complete
        }
    }
    
    /// <summary>
    /// ?? NEW: Record shot execution result and evaluate plan viability
    /// Returns true if plan should be invalidated
    /// </summary>
    public bool RecordShotResult(ShotResult result)
    {
        executionHistory.Add(result);
        
        if (!result.success)
        {
            failedShotsCount++;
            
            // CRITICAL FAILURE THRESHOLDS
            
            // Threshold 1: Shot missed by > 1.5m = Major failure
            if (result.deviationFromTarget > 1.5f)
            {
                Debug.LogWarning($"[EndPlan] CRITICAL FAILURE: Shot missed by {result.deviationFromTarget:F2}m! " +
                                $"Reason: {result.failureReason}");
                isValid = false;
                return true; // Invalidate plan immediately
            }
            
            // Threshold 2: 2+ failed shots = Plan not working
            if (failedShotsCount >= 2)
            {
                Debug.LogWarning($"[EndPlan] Multiple failures ({failedShotsCount}) - invalidating plan");
                isValid = false;
                return true;
            }
            
            // Threshold 3: Specific intent failures
            if (result.attemptedIntent == ShotIntent.RemoveThreat && result.deviationFromTarget > 0.8f)
            {
                // Takeout missed significantly - can't continue with this plan
                Debug.LogWarning($"[EndPlan] Takeout failed - plan relies on removal");
                isValid = false;
                return true;
            }
            
            if (result.attemptedIntent == ShotIntent.CreateOpportunity && result.deviationFromTarget > 1.0f)
            {
                // Guard placement failed - plan won't work
                Debug.LogWarning($"[EndPlan] Guard placement failed - plan compromised");
                isValid = false;
                return true;
            }
        }
        
        // Shot succeeded or failure is manageable
        return false;
    }
    
    /// <summary>
    /// ?? NEW: Evaluate if plan should continue based on execution history
    /// </summary>
    public bool ShouldContinuePlan()
    {
        if (!isValid) return false;
        
        // If 50%+ of shots failed, plan isn't working
        if (executionHistory.Count > 0)
        {
            float failureRate = (float)failedShotsCount / executionHistory.Count;
            if (failureRate >= 0.5f)
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// ?? NEW: Get plan success rate (0-1)
    /// </summary>
    public float GetSuccessRate()
    {
        if (executionHistory.Count == 0) return 1.0f; // No data yet
        
        int successCount = 0;
        foreach (var result in executionHistory)
        {
            if (result.success) successCount++;
        }
        
        return (float)successCount / executionHistory.Count;
    }
    
    /// <summary>
    /// Check if plan needs re-evaluation due to unexpected events
    /// </summary>
    public bool NeedsReEvaluation(GameManager gm, string myTeamName)
    {
        if (!isValid) return true;
        
        // Re-evaluate if:
        // 1. Opponent made unexpected strong play
        // 2. Our last shot failed significantly
        // 3. Game state changed dramatically
        
        // Simple heuristic: Check if opponent has more rocks in house than expected
        int oppRocksInHouse = 0;
        foreach (var rock in gm.houseList)
        {
            if (rock.rockInfo.teamName != myTeamName)
                oppRocksInHouse++;
        }
        
        // If opponent suddenly has 2+ rocks when we planned for 0-1, re-evaluate
        if (oppRocksInHouse >= 2 && planCreatedAtRock < gm.rockCurrent - 2)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Generate a debug summary of this plan
    /// </summary>
    public string GetDebugSummary()
    {
        string summary = $"[EndPlan] {strategyName} (Confidence: {confidence:F2})\n";
        summary += $"  Created at rock {planCreatedAtRock}, currently at step {currentStep}/{plannedIntents.Count}\n";
        summary += $"  Reasoning: {reasoning}\n";
        summary += $"  Expected: {expectedOutcome}\n";
        summary += "  Planned shots:\n";
        
        for (int i = 0; i < plannedIntents.Count; i++)
        {
            string marker = (i == currentStep) ? "?" : " ";
            summary += $"  {marker} {i + 1}. {plannedIntents[i]}";
            
            if (i < targetRocks.Count && targetRocks[i] >= 0)
                summary += $" (target rock #{targetRocks[i]})";
            else if (i < targetPositions.Count)
                summary += $" (target pos: {targetPositions[i]})";
            
            summary += "\n";
        }
        
        return summary;
    }
}
