using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Multi-Shot Planning Engine
/// Analyzes game state and selects optimal 2-3 shot strategic plan
/// </summary>
public class MultiShotPlanner
{
    private GameManager gm;
    
    // Current active plan (null if no plan active)
    private EndPlan currentPlan;
    
    public bool enableMultiShotPlanning = true;
    public bool verboseLogging = false;
    
    public MultiShotPlanner(GameManager gameManager)
    {
        gm = gameManager;
        currentPlan = null;
    }
    
    /// <summary>
    /// Get or create a strategic plan for current situation
    /// </summary>
    public EndPlan GetPlan(int rockCurrent, string myTeam, bool hasHammer, int myScore, int oppScore)
    {
        if (!enableMultiShotPlanning)
        {
            return null; // Planning disabled - use single-shot logic
        }
        
        // Check if we need a new plan
        if (currentPlan == null || !currentPlan.isValid || currentPlan.NeedsReEvaluation(gm, myTeam))
        {
            // Create new plan
            currentPlan = CreateNewPlan(rockCurrent, myTeam, hasHammer, myScore, oppScore);
            
            if (verboseLogging && currentPlan != null)
            {
                Debug.Log(currentPlan.GetDebugSummary());
            }
        }
        
        return currentPlan;
    }
    
    /// <summary>
    /// Mark current shot as complete, advance plan
    /// </summary>
    public void AdvancePlan()
    {
        if (currentPlan != null)
        {
            currentPlan.AdvanceStep();
            
            if (verboseLogging)
            {
                Debug.Log($"[MultiShotPlanner] Advanced to step {currentPlan.currentStep}/{currentPlan.plannedIntents.Count}");
            }
        }
    }
    
    /// <summary>
    /// ?? NEW: Record shot execution result and check if plan should continue
    /// Call this AFTER each shot completes
    /// </summary>
    public void RecordShotExecution(Vector2 intendedPosition, Vector2 actualPosition, bool shotSucceeded, string failureReason = "")
    {
        if (currentPlan == null) return;
        
        // Build shot result
        ShotResult result = new ShotResult
        {
            success = shotSucceeded,
            deviationFromTarget = Vector2.Distance(intendedPosition, actualPosition),
            attemptedIntent = currentPlan.GetCurrentIntent(),
            failureReason = failureReason,
            actualFinalPosition = actualPosition,
            intendedPosition = intendedPosition
        };
        
        // Record result and check if plan should be invalidated
        bool shouldInvalidate = currentPlan.RecordShotResult(result);
        
        if (shouldInvalidate)
        {
            if (verboseLogging)
            {
                Debug.Log($"[MultiShotPlanner] ?? Plan invalidated due to shot failure! " +
                          $"Deviation: {result.deviationFromTarget:F2}m, Reason: {failureReason}");
            }
        }
        else if (!result.success && verboseLogging)
        {
            Debug.Log($"[MultiShotPlanner] ?? Shot suboptimal but plan continues " +
                      $"(Deviation: {result.deviationFromTarget:F2}m)");
        }
    }
    
    /// <summary>
    /// ?? NEW: Simple shot evaluation - did it land near target?
    /// Call this if you just want to check final position
    /// </summary>
    public void EvaluateShotResult(Vector2 intendedPosition, Vector2 actualPosition, float acceptableError = 1.0f)
    {
        float deviation = Vector2.Distance(intendedPosition, actualPosition);
        bool success = (deviation <= acceptableError);
        
        string failureReason = "";
        if (!success)
        {
            failureReason = $"Missed target by {deviation:F2}m (acceptable: {acceptableError:F2}m)";
        }
        
        RecordShotExecution(intendedPosition, actualPosition, success, failureReason);
    }
    
    /// <summary>
    /// Force plan re-evaluation (called when unexpected event occurs)
    /// </summary>
    public void InvalidatePlan(string reason)
    {
        if (currentPlan != null)
        {
            currentPlan.isValid = false;
            
            if (verboseLogging)
            {
                Debug.Log($"[MultiShotPlanner] Plan invalidated: {reason}");
            }
        }
    }
    
    /// <summary>
    /// Create a new strategic plan based on game situation
    /// </summary>
    private EndPlan CreateNewPlan(int rockCurrent, string myTeam, bool hasHammer, int myScore, int oppScore)
    {
        // Calculate how many rocks I have left
        int myRocksLeft = StrategyPatternLibrary.GetMyRocksRemaining(gm, rockCurrent, hasHammer);
        
        if (myRocksLeft < 2)
        {
            // Not enough rocks for multi-shot plan
            return null;
        }
        
        // ?? NEW: Calculate dynamic risk tolerance
        float riskTolerance = StrategyPatternLibrary.CalculateRiskTolerance(
            myScore, oppScore, gm.endCurrent, gm.endTotal, myRocksLeft, hasHammer
        );
        
        string riskCategory = StrategyPatternLibrary.GetRiskCategory(riskTolerance);
        
        // Analyze game state
        int myRocksInHouse = CountRocksInHouse(myTeam);
        int oppRocksInHouse = CountRocksInHouse(GetOpponentTeam(myTeam));
        int scoreDiff = myScore - oppScore;
        bool isLastEnd = (gm.endCurrent >= gm.endTotal - 1);
        
        if (verboseLogging)
        {
            Debug.Log($"[MultiShotPlanner] Planning for rock {rockCurrent}: " +
                      $"Rocks left={myRocksLeft}, Hammer={hasHammer}, Score diff={scoreDiff}, " +
                      $"My rocks in house={myRocksInHouse}, Opp rocks={oppRocksInHouse}, " +
                      $"Risk={riskTolerance:F2} ({riskCategory})");
        }
        
        EndPlan plan = null;
        
        // ?? DECISION TREE WITH RISK MANAGEMENT
        
        // CRITICAL: Last end desperation
        if (isLastEnd && scoreDiff < 0)
        {
            int pointsNeeded = Mathf.Abs(scoreDiff) + 1;
            plan = StrategyPatternLibrary.DesperationAllOut(gm, myTeam, rockCurrent, pointsNeeded);
        }
        
        // CRITICAL: Protecting lead late game
        else if (isLastEnd && scoreDiff > 0 && myRocksInHouse >= 1)
        {
            plan = StrategyPatternLibrary.ProtectLead_Conservative(gm, myTeam, rockCurrent);
        }
        
        // WITH HAMMER strategies
        else if (hasHammer)
        {
            // ?? RISK-ADJUSTED: Should we try for multi-point or play safe?
            bool attemptMultiPoint = StrategyPatternLibrary.ShouldAttemptMultiPoint(
                riskTolerance, hasHammer, myRocksLeft
            );
            
            // Clean house - build multi-point OR play safe
            if (oppRocksInHouse == 0 && myRocksInHouse == 0)
            {
                if (attemptMultiPoint)
                {
                    plan = StrategyPatternLibrary.GuardDrawDraw_MultiPoint(gm, myTeam, rockCurrent);
                }
                else
                {
                    // Conservative: Just draw for 1
                    plan = StrategyPatternLibrary.DrawDrawDraw_Dominant(gm, myTeam, rockCurrent);
                }
            }
            
            // Opponent has rocks - decide: clear or blank?
            else if (oppRocksInHouse >= 1)
            {
                // ?? RISK-ADJUSTED: Use aggressive strategy?
                bool useAggressive = StrategyPatternLibrary.ShouldUseAggressiveStrategy(
                    riskTolerance, myRocksInHouse, oppRocksInHouse
                );
                
                if (useAggressive && myRocksLeft >= 3)
                {
                    // Clear and score
                    int threat1 = FindBiggestThreat(myTeam);
                    int threat2 = FindSecondBiggestThreat(myTeam, threat1);
                    plan = StrategyPatternLibrary.ClearClearScore_Hammer(gm, myTeam, rockCurrent, threat1, threat2);
                }
                else
                {
                    // ?? RISK-ADJUSTED: Should we blank to keep hammer?
                    bool shouldBlank = StrategyPatternLibrary.ShouldBlankToKeepHammer(
                        riskTolerance, hasHammer, myRocksInHouse, oppRocksInHouse
                    );
                    
                    if (shouldBlank && myRocksInHouse < 2 && scoreDiff > 0)
                    {
                        plan = StrategyPatternLibrary.BlankToKeepHammer(gm, myTeam, rockCurrent);
                    }
                    else
                    {
                        // Build multi-point end
                        plan = StrategyPatternLibrary.GuardDrawDraw_MultiPoint(gm, myTeam, rockCurrent);
                    }
                }
            }
            
            // We have rocks, opponent doesn't - keep scoring
            else if (myRocksInHouse >= 1)
            {
                plan = StrategyPatternLibrary.DrawDrawDraw_Dominant(gm, myTeam, rockCurrent);
            }
        }
        
        // WITHOUT HAMMER strategies
        else
        {
            // Clean house - setup steal
            if (oppRocksInHouse == 0 && myRocksInHouse == 0)
            {
                plan = StrategyPatternLibrary.GuardDrawProtect_Steal(gm, myTeam, rockCurrent);
            }
            
            // ?? RISK-ADJUSTED: Aggressive clear or conservative?
            else if (oppRocksInHouse >= 2)
            {
                bool useAggressive = StrategyPatternLibrary.ShouldUseAggressiveStrategy(
                    riskTolerance, myRocksInHouse, oppRocksInHouse
                );
                
                if (useAggressive)
                {
                    int threat1 = FindBiggestThreat(myTeam);
                    int threat2 = FindSecondBiggestThreat(myTeam, threat1);
                    plan = StrategyPatternLibrary.RemoveDrawRemove_Aggressive(gm, myTeam, rockCurrent, threat1, threat2);
                }
                else
                {
                    // Conservative: Guard and draw
                    plan = StrategyPatternLibrary.GuardDrawProtect_Steal(gm, myTeam, rockCurrent);
                }
            }
            
            // We're winning house - protect steal
            else if (myRocksInHouse > oppRocksInHouse && myRocksInHouse >= 1)
            {
                plan = StrategyPatternLibrary.ProtectLead_Conservative(gm, myTeam, rockCurrent);
            }
            
            // Opponent has 1 rock - split house strategy
            else if (oppRocksInHouse == 1 && myRocksLeft >= 3)
            {
                plan = StrategyPatternLibrary.CornerGuardCorner_Split(gm, myTeam, rockCurrent);
            }
            
            // Protecting lead - force blank
            else if (scoreDiff > 0 && oppRocksInHouse >= 1)
            {
                plan = StrategyPatternLibrary.BlankForce_Defensive(gm, myTeam, rockCurrent);
            }
        }
        
        // ?? APPLY RISK ADJUSTMENTS TO PLAN
        if (plan != null)
        {
            // Adjust confidence for shooter skills
            CharacterStats shooter = GetShooterStats(rockCurrent);
            StrategyPatternLibrary.AdjustConfidenceForSkills(plan, shooter);
            
            // Adjust confidence for risk tolerance
            StrategyPatternLibrary.AdjustConfidenceForRisk(plan, riskTolerance, riskCategory);
            
            if (verboseLogging)
            {
                Debug.Log($"[MultiShotPlanner] Selected plan: {plan.strategyName} " +
                          $"(Confidence: {plan.confidence:F2}, Risk: {riskCategory})");
            }
        }
        
        return plan;
    }
    
    #region HELPER METHODS
    
    private int CountRocksInHouse(string teamName)
    {
        int count = 0;
        foreach (var rockEntry in gm.houseList)
        {
            if (rockEntry.rockInfo.teamName == teamName)
            {
                count++;
            }
        }
        return count;
    }
    
    private string GetOpponentTeam(string myTeam)
    {
        return (myTeam == gm.redTeamName) ? gm.yellowTeamName : gm.redTeamName;
    }
    
    private int FindBiggestThreat(string myTeamName)
    {
        int bestThreat = -1;
        float bestThreatValue = float.MinValue;
        
        for (int i = 0; i < gm.houseList.Count; i++)
        {
            var rockEntry = gm.houseList[i];
            
            if (rockEntry.rockInfo.teamName == myTeamName)
                continue;
            
            float distToButton = Vector2.Distance(rockEntry.rock.transform.position, new Vector2(0f, 6.5f));
            float threatValue = 10f - distToButton;
            
            if (threatValue > bestThreatValue)
            {
                bestThreatValue = threatValue;
                bestThreat = rockEntry.rockInfo.rockIndex;
            }
        }
        
        return bestThreat;
    }
    
    private int FindSecondBiggestThreat(string myTeamName, int firstThreat)
    {
        int bestThreat = -1;
        float bestThreatValue = float.MinValue;
        
        for (int i = 0; i < gm.houseList.Count; i++)
        {
            var rockEntry = gm.houseList[i];
            
            if (rockEntry.rockInfo.teamName == myTeamName)
                continue;
            
            if (rockEntry.rockInfo.rockIndex == firstThreat)
                continue; // Skip first threat
            
            float distToButton = Vector2.Distance(rockEntry.rock.transform.position, new Vector2(0f, 6.5f));
            float threatValue = 10f - distToButton;
            
            if (threatValue > bestThreatValue)
            {
                bestThreatValue = threatValue;
                bestThreat = rockEntry.rockInfo.rockIndex;
            }
        }
        
        return bestThreat;
    }
    
    private CharacterStats GetShooterStats(int rockCurrent)
    {
        TeamManager tm = Object.FindObjectOfType<TeamManager>();
        if (tm == null) return null;
        
        int memberIndex = rockCurrent / 4;
        memberIndex = Mathf.Clamp(memberIndex, 0, 3);
        
        bool isRedTeam = (rockCurrent % 2 == 0) ? gm.redHammer : !gm.redHammer;
        
        if (isRedTeam && tm.teamRed != null && memberIndex < tm.teamRed.Length)
            return tm.teamRed[memberIndex].charStats;
        else if (!isRedTeam && tm.teamYellow != null && memberIndex < tm.teamYellow.Length)
            return tm.teamYellow[memberIndex].charStats;
        
        return null;
    }
    
    #endregion
}
