using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Library of proven multi-shot strategic patterns
/// These are "playbook" strategies that AI can select from
/// </summary>
public static class StrategyPatternLibrary
{
    #region WITHOUT HAMMER STRATEGIES
    
    /// <summary>
    /// Guard-Draw-Protect: Classic steal setup (3 shots)
    /// Rock 1: Place guard
    /// Rock 2: Draw behind guard
    /// Rock 3: Protect with freeze or second guard
    /// </summary>
    public static EndPlan GuardDrawProtect_Steal(GameManager gm, string myTeam, int rockCurrent)
    {
        var plan = new EndPlan
        {
            strategyName = "Guard-Draw-Protect (Steal Setup)",
            reasoning = "Without hammer - build protected steal opportunity",
            expectedOutcome = "1-2 protected rocks behind guard(s)",
            planCreatedAtRock = rockCurrent,
            confidence = 0.75f
        };
        
        // Shot 1: SMART guard placement based on house situation
        string guardReason;
        Vector2 smartGuardPos = CalculateSmartGuardPosition(gm, myTeam, out guardReason);
        
        plan.plannedIntents.Add(ShotIntent.CreateOpportunity);
        plan.targetPositions.Add(smartGuardPos);
        plan.targetRocks.Add(-1);
        
        plan.reasoning += $" | Guard: {guardReason}";
        
        // Shot 2: Draw behind guard (adjust X to match guard)
        plan.plannedIntents.Add(ShotIntent.ScorePoints);
        plan.targetPositions.Add(new Vector2(smartGuardPos.x, 6.5f)); // Draw behind guard's X position
        plan.targetRocks.Add(-1);
        
        // Shot 3: Protect or add second counter
        plan.plannedIntents.Add(ShotIntent.ProtectLead);
        plan.targetPositions.Add(new Vector2(smartGuardPos.x + 0.3f, 6.0f)); // Slightly offset
        plan.targetRocks.Add(-1);
        
        return plan;
    }
    
    /// <summary>
    /// Corner-Guard-Corner: Split house strategy (3 shots)
    /// Forces opponent to choose which side to attack
    /// </summary>
    public static EndPlan CornerGuardCorner_Split(GameManager gm, string myTeam, int rockCurrent)
    {
        var plan = new EndPlan
        {
            strategyName = "Corner-Guard-Corner (Split House)",
            reasoning = "Without hammer - force opponent to choose attack side",
            expectedOutcome = "Rocks on both wings, difficult to clear",
            planCreatedAtRock = rockCurrent,
            confidence = 0.70f
        };
        
        // Shot 1: Left corner guard (smart positioning)
        Vector2 leftGuard = CalculateCornerGuardPosition(leftSide: true, guardDistance: 2.5f);
        plan.plannedIntents.Add(ShotIntent.CreateOpportunity);
        plan.targetPositions.Add(leftGuard);
        plan.targetRocks.Add(-1);
        
        // Shot 2: Right corner draw (behind where right guard would be)
        Vector2 rightGuard = CalculateCornerGuardPosition(leftSide: false, guardDistance: 2.5f);
        plan.plannedIntents.Add(ShotIntent.ScorePoints);
        plan.targetPositions.Add(new Vector2(rightGuard.x, 7.0f)); // Draw right side
        plan.targetRocks.Add(-1);
        
        // Shot 3: Left corner draw (behind left guard)
        plan.plannedIntents.Add(ShotIntent.ScorePoints);
        plan.targetPositions.Add(new Vector2(leftGuard.x, 7.0f)); // Draw left side
        plan.targetRocks.Add(-1);
        
        return plan;
    }
    
    /// <summary>
    /// Remove-Draw-Remove: Aggressive clearing strategy (3 shots)
    /// Clear opponent rocks, place one counter, clear again
    /// </summary>
    public static EndPlan RemoveDrawRemove_Aggressive(GameManager gm, string myTeam, int rockCurrent, int threat1, int threat2)
    {
        var plan = new EndPlan
        {
            strategyName = "Remove-Draw-Remove (Aggressive Clear)",
            reasoning = "Multiple opponent rocks - clear and counter",
            expectedOutcome = "Cleared opponent rocks, single counter remaining",
            planCreatedAtRock = rockCurrent,
            confidence = 0.65f
        };
        
        // Shot 1: Remove first threat
        plan.plannedIntents.Add(ShotIntent.RemoveThreat);
        plan.targetPositions.Add(Vector2.zero);
        plan.targetRocks.Add(threat1);
        
        // Shot 2: Draw to button
        plan.plannedIntents.Add(ShotIntent.ScorePoints);
        plan.targetPositions.Add(new Vector2(0f, 6.5f));
        plan.targetRocks.Add(-1);
        
        // Shot 3: Remove second threat
        plan.plannedIntents.Add(ShotIntent.RemoveThreat);
        plan.targetPositions.Add(Vector2.zero);
        plan.targetRocks.Add(threat2);
        
        return plan;
    }
    
    /// <summary>
    /// Blank-Force: Defensive strategy to blank the end
    /// Used when protecting lead without hammer
    /// </summary>
    public static EndPlan BlankForce_Defensive(GameManager gm, string myTeam, int rockCurrent)
    {
        var plan = new EndPlan
        {
            strategyName = "Blank-Force (Defensive)",
            reasoning = "Protecting lead - force blank to retain hammer for them",
            expectedOutcome = "Blank end, no damage",
            planCreatedAtRock = rockCurrent,
            confidence = 0.80f
        };
        
        // All shots: Clear any rocks, avoid scoring ourselves
        int shotsRemaining = 3;
        for (int i = 0; i < shotsRemaining; i++)
        {
            plan.plannedIntents.Add(ShotIntent.ForceBlank);
            plan.targetPositions.Add(Vector2.zero);
            plan.targetRocks.Add(-1);
        }
        
        return plan;
    }
    
    #endregion
    
    #region WITH HAMMER STRATEGIES
    
    /// <summary>
    /// Guard-Draw-Draw: Build multi-point end with hammer (3 shots)
    /// </summary>
    public static EndPlan GuardDrawDraw_MultiPoint(GameManager gm, string myTeam, int rockCurrent)
    {
        var plan = new EndPlan
        {
            strategyName = "Guard-Draw-Draw (Multi-Point Setup)",
            reasoning = "With hammer - build for 2+ point end",
            expectedOutcome = "2-3 counters protected by guard",
            planCreatedAtRock = rockCurrent,
            confidence = 0.80f
        };
        
        // Shot 1: SMART guard placement
        string guardReason;
        Vector2 smartGuardPos = CalculateSmartGuardPosition(gm, myTeam, out guardReason);
        
        plan.plannedIntents.Add(ShotIntent.CreateOpportunity);
        plan.targetPositions.Add(smartGuardPos);
        plan.targetRocks.Add(-1);
        
        plan.reasoning += $" | Guard: {guardReason}";
        
        // Shot 2: Draw to button (adjust for guard position)
        plan.plannedIntents.Add(ShotIntent.ScorePoints);
        plan.targetPositions.Add(new Vector2(smartGuardPos.x, 6.5f));
        plan.targetRocks.Add(-1);
        
        // Shot 3: Draw to 8-foot (spread slightly)
        plan.plannedIntents.Add(ShotIntent.ScorePoints);
        plan.targetPositions.Add(new Vector2(smartGuardPos.x + 0.4f, 7.5f));
        plan.targetRocks.Add(-1);
        
        return plan;
    }
    
    /// <summary>
    /// Clear-Clear-Score: Aggressive removal then score (3 shots)
    /// Remove opponent setup, then score on last rock
    /// </summary>
    public static EndPlan ClearClearScore_Hammer(GameManager gm, string myTeam, int rockCurrent, int threat1, int threat2)
    {
        var plan = new EndPlan
        {
            strategyName = "Clear-Clear-Score (Hammer Advantage)",
            reasoning = "With hammer - clear opponent rocks then score safely",
            expectedOutcome = "Opponent rocks removed, guaranteed 1+ points",
            planCreatedAtRock = rockCurrent,
            confidence = 0.75f
        };
        
        // Shot 1: Remove first threat
        plan.plannedIntents.Add(ShotIntent.RemoveThreat);
        plan.targetPositions.Add(Vector2.zero);
        plan.targetRocks.Add(threat1);
        
        // Shot 2: Remove second threat
        plan.plannedIntents.Add(ShotIntent.RemoveThreat);
        plan.targetPositions.Add(Vector2.zero);
        plan.targetRocks.Add(threat2);
        
        // Shot 3: Score on last shot (hammer)
        plan.plannedIntents.Add(ShotIntent.ScorePoints);
        plan.targetPositions.Add(new Vector2(0f, 6.5f));
        plan.targetRocks.Add(-1);
        
        return plan;
    }
    
    /// <summary>
    /// Draw-Draw-Draw: Pure scoring strategy (3 shots)
    /// Clean house, pile on points
    /// </summary>
    public static EndPlan DrawDrawDraw_Dominant(GameManager gm, string myTeam, int rockCurrent)
    {
        var plan = new EndPlan
        {
            strategyName = "Draw-Draw-Draw (Dominant Scoring)",
            reasoning = "With hammer, clean house - maximize points",
            expectedOutcome = "3+ rocks in scoring position",
            planCreatedAtRock = rockCurrent,
            confidence = 0.85f
        };
        
        // All shots: Draw to scoring positions
        Vector2[] positions = new Vector2[]
        {
            new Vector2(0f, 6.5f),    // Button
            new Vector2(0.4f, 7.0f),  // 8-foot right
            new Vector2(-0.4f, 7.0f)  // 8-foot left
        };
        
        for (int i = 0; i < 3; i++)
        {
            plan.plannedIntents.Add(ShotIntent.ScorePoints);
            plan.targetPositions.Add(positions[i]);
            plan.targetRocks.Add(-1);
        }
        
        return plan;
    }
    
    /// <summary>
    /// Blank-To-Keep-Hammer: Strategic blank (2-3 shots)
    /// Can't score 2, so blank to keep hammer next end
    /// </summary>
    public static EndPlan BlankToKeepHammer(GameManager gm, string myTeam, int rockCurrent)
    {
        var plan = new EndPlan
        {
            strategyName = "Blank-To-Keep-Hammer (Strategic)",
            reasoning = "Can't score 2+ - blank to retain hammer advantage",
            expectedOutcome = "Blank end, keep hammer for next end",
            planCreatedAtRock = rockCurrent,
            confidence = 0.70f
        };
        
        // All shots: Throw through or peel
        for (int i = 0; i < 3; i++)
        {
            plan.plannedIntents.Add(ShotIntent.ForceBlank);
            plan.targetPositions.Add(Vector2.zero);
            plan.targetRocks.Add(-1);
        }
        
        return plan;
    }
    
    #endregion
    
    #region LATE GAME STRATEGIES
    
    /// <summary>
    /// Desperation-All-Out: Behind in score, last end (3 shots)
    /// Maximum aggression to steal/score
    /// </summary>
    public static EndPlan DesperationAllOut(GameManager gm, string myTeam, int rockCurrent, int pointsNeeded)
    {
        var plan = new EndPlan
        {
            strategyName = $"Desperation All-Out (Need {pointsNeeded} pts)",
            reasoning = $"Behind by {pointsNeeded} - must score or steal",
            expectedOutcome = $"Attempt to score/steal {pointsNeeded}+ points",
            planCreatedAtRock = rockCurrent,
            confidence = 0.50f // Risky!
        };
        
        // Strategy depends on points needed
        if (pointsNeeded >= 3)
        {
            // Need multiple - build complex end
            plan.plannedIntents.Add(ShotIntent.CreateOpportunity); // Guard
            plan.plannedIntents.Add(ShotIntent.ScorePoints);       // Draw
            plan.plannedIntents.Add(ShotIntent.ScorePoints);       // Draw
        }
        else
        {
            // Need 1-2 - clear and score
            plan.plannedIntents.Add(ShotIntent.Desperation);
            plan.plannedIntents.Add(ShotIntent.ScorePoints);
            plan.plannedIntents.Add(ShotIntent.ScorePoints);
        }
        
        // Generic target positions
        for (int i = 0; i < plan.plannedIntents.Count; i++)
        {
            plan.targetPositions.Add(new Vector2(0f, 6.5f));
            plan.targetRocks.Add(-1);
        }
        
        return plan;
    }
    
    /// <summary>
    /// Protect-Lead: Conservative defense of lead (3 shots)
    /// Freeze, guard, or bury rocks to protect advantage
    /// </summary>
    public static EndPlan ProtectLead_Conservative(GameManager gm, string myTeam, int rockCurrent)
    {
        var plan = new EndPlan
        {
            strategyName = "Protect-Lead (Conservative)",
            reasoning = "Ahead in score - protect rocks, minimize opponent chances",
            expectedOutcome = "Maintain counting rocks, limit opponent options",
            planCreatedAtRock = rockCurrent,
            confidence = 0.75f
        };
        
        // All shots: Protect what we have
        for (int i = 0; i < 3; i++)
        {
            plan.plannedIntents.Add(ShotIntent.ProtectLead);
            plan.targetPositions.Add(Vector2.zero);
            plan.targetRocks.Add(-1);
        }
        
        return plan;
    }
    
    #endregion
    
    #region GUARD POSITIONING INTELLIGENCE
    
    /// <summary>
    /// Calculate optimal guard position to protect a specific rock
    /// Guard is placed in front of rock, blocking direct path from hog line
    /// </summary>
    public static Vector2 CalculateProtectionGuardPosition(GameObject protectRock, float guardDistance = 2.5f)
    {
        if (protectRock == null) 
            return new Vector2(0f, guardDistance); // Default center guard
        
        Vector2 rockPos = protectRock.transform.position;
        
        // Guard should be between hog line (Y ~= -16) and rock
        // Place it at guardDistance Y position, same X as rock we're protecting
        Vector2 guardPos = new Vector2(rockPos.x, guardDistance);
        
        return guardPos;
    }
    
    /// <summary>
    /// Calculate optimal guard position to block opponent's rock
    /// Guard is placed between button and opponent's rock to make takeout difficult
    /// </summary>
    public static Vector2 CalculateBlockingGuardPosition(GameObject blockRock, float guardDistance = 2.5f)
    {
        if (blockRock == null)
            return new Vector2(0f, guardDistance); // Default center guard
        
        Vector2 rockPos = blockRock.transform.position;
        Vector2 button = new Vector2(0f, 6.5f);
        
        // Place guard on the line between button and opponent's rock
        // This maximizes blocking effectiveness
        Vector2 direction = (button - rockPos).normalized;
        
        // Guard at guardDistance Y, but shifted slightly toward opponent's rock
        float xOffset = rockPos.x * 0.6f; // 60% of rock's X position
        Vector2 guardPos = new Vector2(xOffset, guardDistance);
        
        // Clamp to reasonable range (don't go too far to corners)
        guardPos.x = Mathf.Clamp(guardPos.x, -1.2f, 1.2f);
        
        return guardPos;
    }
    
    /// <summary>
    /// Calculate optimal corner guard position
    /// Used for split-house strategies
    /// </summary>
    public static Vector2 CalculateCornerGuardPosition(bool leftSide, float guardDistance = 2.5f)
    {
        // Corner guards at ~1.0m from center, standard guard distance
        float xPos = leftSide ? -1.0f : 1.0f;
        return new Vector2(xPos, guardDistance);
    }
    
    /// <summary>
    /// Smart guard placement - analyzes situation and picks best guard type
    /// This is the main entry point for intelligent guard placement
    /// </summary>
    public static Vector2 CalculateSmartGuardPosition(
        GameManager gm,
        string myTeam,
        out string guardReasoning)
    {
        guardReasoning = "Center guard (default)";
        
        // ANALYSIS: What's in the house?
        int myRocksInHouse = 0;
        int oppRocksInHouse = 0;
        GameObject oppBestRock = null;
        GameObject myBestRock = null;
        float oppBestDist = 999f;
        float myBestDist = 999f;
        
        Vector2 button = new Vector2(0f, 6.5f);
        
        foreach (var rockEntry in gm.houseList)
        {
            bool isMine = (rockEntry.rockInfo.teamName == myTeam);
            float dist = Vector2.Distance(rockEntry.rock.transform.position, button);
            
            if (isMine)
            {
                myRocksInHouse++;
                if (dist < myBestDist)
                {
                    myBestDist = dist;
                    myBestRock = rockEntry.rock;
                }
            }
            else
            {
                oppRocksInHouse++;
                if (dist < oppBestDist)
                {
                    oppBestDist = dist;
                    oppBestRock = rockEntry.rock;
                }
            }
        }
        
        // DECISION TREE: What type of guard do we need?
        
        // SCENARIO 1: We have scoring rocks - PROTECT them!
        if (myRocksInHouse >= 1 && myBestRock != null)
        {
            guardReasoning = $"Protecting our counter at X={myBestRock.transform.position.x:F1}";
            return CalculateProtectionGuardPosition(myBestRock, 2.5f);
        }
        
        // SCENARIO 2: Opponent has scoring rocks - BLOCK them!
        else if (oppRocksInHouse >= 1 && oppBestRock != null)
        {
            guardReasoning = $"Blocking opponent's counter at X={oppBestRock.transform.position.x:F1}";
            return CalculateBlockingGuardPosition(oppBestRock, 2.5f);
        }
        
        // SCENARIO 3: Clean house - CENTER GUARD (classic setup)
        else
        {
            guardReasoning = "Center guard for steal setup (clean house)";
            return new Vector2(0f, 2.5f);
        }
    }
    
    /// <summary>
    /// Evaluate guard quality - how good is this guard position?
    /// Returns 0-1 score (higher = better)
    /// </summary>
    public static float EvaluateGuardPosition(Vector2 guardPos, GameManager gm, string myTeam)
    {
        float score = 0.5f; // Base score
        
        Vector2 button = new Vector2(0f, 6.5f);
        
        // Check if guard blocks path to any valuable rocks
        foreach (var rockEntry in gm.houseList)
        {
            bool isMine = (rockEntry.rockInfo.teamName == myTeam);
            Vector2 rockPos = rockEntry.rock.transform.position;
            
            // Calculate if guard is "between" hog line and this rock
            bool guardsThisRock = Mathf.Abs(guardPos.x - rockPos.x) < 0.5f && guardPos.y < rockPos.y;
            
            if (isMine && guardsThisRock)
            {
                // Good! Guards our rock
                float rockValue = 10f - Vector2.Distance(rockPos, button);
                score += rockValue * 0.05f;
            }
            else if (!isMine && guardsThisRock)
            {
                // Also good! Makes their rock harder to draw to
                float rockValue = 10f - Vector2.Distance(rockPos, button);
                score += rockValue * 0.03f;
            }
        }
        
        // Prefer guards not too far in corners (harder to use)
        float centeredness = 1f - Mathf.Abs(guardPos.x) / 1.5f;
        score += centeredness * 0.1f;
        
        return Mathf.Clamp01(score);
    }
    
    #endregion
    
    #region DYNAMIC RISK MANAGEMENT
    
    /// <summary>
    /// Calculate dynamic risk tolerance based on game situation
    /// Returns 0-1 where:
    /// - 0.0 = Maximum conservative (protect lead)
    /// - 0.5 = Balanced play
    /// - 1.0 = Maximum aggression (desperation)
    /// </summary>
    public static float CalculateRiskTolerance(
        int myScore,
        int oppScore,
        int currentEnd,
        int totalEnds,
        int rocksLeft,
        bool hasHammer)
    {
        float baseRisk = 0.5f; // Start balanced
        
        // FACTOR 1: Score differential (biggest impact!)
        int scoreDiff = myScore - oppScore;
        
        if (scoreDiff > 0)
        {
            // AHEAD - Get more conservative as lead increases
            float leadPressure = Mathf.Clamp01(scoreDiff / 3f); // Max at +3 points
            baseRisk -= leadPressure * 0.3f; // Reduce risk by up to 30%
        }
        else if (scoreDiff < 0)
        {
            // BEHIND - Get more aggressive as deficit increases
            float deficitPressure = Mathf.Clamp01(Mathf.Abs(scoreDiff) / 3f); // Max at -3 points
            baseRisk += deficitPressure * 0.4f; // Increase risk by up to 40%
        }
        
        // FACTOR 2: End number urgency
        int endsRemaining = totalEnds - currentEnd;
        
        if (endsRemaining <= 2)
        {
            // LATE GAME - Urgency increases
            if (scoreDiff < 0)
            {
                // Behind in late game = DESPERATE!
                baseRisk += 0.2f;
            }
            else if (scoreDiff > 0)
            {
                // Ahead in late game = PROTECT!
                baseRisk -= 0.15f;
            }
        }
        
        // FACTOR 3: Rocks remaining in current end
        if (rocksLeft <= 2)
        {
            // LAST ROCKS - Critical decision time
            if (scoreDiff < 0)
            {
                // Behind with few rocks left = GO FOR IT!
                baseRisk += 0.15f;
            }
        }
        
        // FACTOR 4: Hammer situation
        if (hasHammer)
        {
            // With hammer, can afford slightly more risk (safety net)
            baseRisk += 0.1f;
        }
        else
        {
            // Without hammer, be slightly more careful
            baseRisk -= 0.05f;
        }
        
        // FACTOR 5: Last end desperation
        if (currentEnd >= totalEnds - 1 && scoreDiff < 0)
        {
            // MUST WIN NOW!
            baseRisk = 1.0f; // Maximum aggression!
        }
        
        // Clamp to valid range
        return Mathf.Clamp01(baseRisk);
    }
    
    /// <summary>
    /// Get risk category for human-readable description
    /// </summary>
    public static string GetRiskCategory(float riskTolerance)
    {
        if (riskTolerance >= 0.8f) return "Maximum Aggression";
        if (riskTolerance >= 0.65f) return "Aggressive";
        if (riskTolerance >= 0.35f) return "Balanced";
        if (riskTolerance >= 0.2f) return "Conservative";
        return "Maximum Defense";
    }
    
    /// <summary>
    /// Adjust plan confidence based on risk tolerance and situation
    /// Higher risk = accept lower confidence plans
    /// Lower risk = demand higher confidence plans
    /// </summary>
    public static void AdjustConfidenceForRisk(EndPlan plan, float riskTolerance, string reasoning = "")
    {
        if (plan == null) return;
        
        // HIGH RISK SITUATIONS: Accept riskier plans
        if (riskTolerance >= 0.7f)
        {
            // Desperate - willing to try risky shots
            plan.confidence *= 0.9f; // Slightly lower bar for execution
            
            if (!string.IsNullOrEmpty(reasoning))
                plan.reasoning += $" | Risk: {reasoning}";
        }
        
        // LOW RISK SITUATIONS: Demand safer plans
        else if (riskTolerance <= 0.3f)
        {
            // Conservative - only high-confidence shots
            plan.confidence *= 1.1f; // Slightly higher bar
            plan.confidence = Mathf.Min(plan.confidence, 0.95f); // Cap at 95%
            
            if (!string.IsNullOrEmpty(reasoning))
                plan.reasoning += $" | Risk: {reasoning} (High confidence required)";
        }
        
        // Clamp final confidence
        plan.confidence = Mathf.Clamp01(plan.confidence);
    }
    
    /// <summary>
    /// Select strategy pattern based on risk tolerance
    /// This modifies the planner's decision-making to be more/less aggressive
    /// </summary>
    public static bool ShouldUseAggressiveStrategy(
        float riskTolerance,
        int myRocksInHouse,
        int oppRocksInHouse)
    {
        // AGGRESSIVE THRESHOLD: Risk tolerance determines when to attack
        float aggressionThreshold = 0.6f;
        
        // SCENARIO 1: High risk tolerance = always aggressive
        if (riskTolerance >= 0.8f)
            return true;
        
        // SCENARIO 2: Moderate risk + opportunity = go for it!
        if (riskTolerance >= aggressionThreshold && oppRocksInHouse >= 2)
            return true; // Multiple threats = attack!
        
        // SCENARIO 3: Low risk tolerance = only attack if necessary
        if (riskTolerance < 0.4f)
        {
            // Conservative - only attack if losing house badly
            return (oppRocksInHouse >= 2 && myRocksInHouse == 0);
        }
        
        // SCENARIO 4: Balanced - attack based on house state
        return (oppRocksInHouse > myRocksInHouse);
    }
    
    /// <summary>
    /// Should we attempt a risky multi-point setup or play safe?
    /// </summary>
    public static bool ShouldAttemptMultiPoint(
        float riskTolerance,
        bool hasHammer,
        int myRocksLeft)
    {
        // Need hammer and rocks to attempt multi-point
        if (!hasHammer || myRocksLeft < 3)
            return false;
        
        // AGGRESSIVE: Always try for multiple
        if (riskTolerance >= 0.7f)
            return true;
        
        // CONSERVATIVE: Only if safe
        if (riskTolerance <= 0.3f)
            return false;
        
        // BALANCED: Try if we have enough rocks
        return (myRocksLeft >= 4);
    }
    
    /// <summary>
    /// Should we blank the end to keep hammer?
    /// </summary>
    public static bool ShouldBlankToKeepHammer(
        float riskTolerance,
        bool hasHammer,
        int myRocksInHouse,
        int oppRocksInHouse)
    {
        if (!hasHammer) return false; // Can't blank without hammer
        
        // AGGRESSIVE: Never blank! Go for points!
        if (riskTolerance >= 0.75f)
            return false;
        
        // CONSERVATIVE: Blank if can't score 2+
        if (riskTolerance <= 0.4f)
        {
            // Blank if we can't clearly score 2 points
            return (myRocksInHouse < 2 || oppRocksInHouse >= 1);
        }
        
        // BALANCED: Only blank if house is messy
        return (myRocksInHouse <= 1 && oppRocksInHouse >= 2);
    }
    
    /// <summary>
    /// Get recommended shot aggressiveness (for shot execution)
    /// Returns 0-1 where higher = throw harder, take more risks
    /// </summary>
    public static float GetShotAggressiveness(float riskTolerance, ShotIntent intent)
    {
        float baseAggression = 0.5f;
        
        // INTENT MODIFIERS
        switch (intent)
        {
            case ShotIntent.RemoveThreat:
            case ShotIntent.Desperation:
                // Takeouts benefit from higher risk tolerance
                baseAggression += riskTolerance * 0.3f;
                break;
            
            case ShotIntent.ScorePoints:
            case ShotIntent.LastShotScoring:
                // Draws benefit from lower risk (precision)
                baseAggression -= (1f - riskTolerance) * 0.2f;
                break;
            
            case ShotIntent.ProtectLead:
            case ShotIntent.ForceBlank:
                // Defensive shots = less aggression
                baseAggression -= riskTolerance * 0.15f;
                break;
        }
        
        return Mathf.Clamp01(baseAggression);
    }
    
    #endregion
    
    #region SHOT EXECUTION FEEDBACK
    
    /// <summary>
    /// ?? Evaluate shot success based on intent and outcome
    /// Returns true if shot achieved its goal
    /// </summary>
    public static bool EvaluateShotSuccess(
        ShotIntent intent,
        Vector2 intendedPosition,
        Vector2 actualPosition,
        int targetRockIndex,
        GameManager gm,
        string myTeamName,
        out float deviation,
        out string failureReason)
    {
        deviation = Vector2.Distance(intendedPosition, actualPosition);
        failureReason = "";
        
        // Different intents have different success criteria
        
        switch (intent)
        {
            case ShotIntent.RemoveThreat:
            case ShotIntent.Desperation:
                // Takeout success: Did we remove the target rock?
                if (targetRockIndex >= 0)
                {
                    // Check if target rock is still in play
                    bool targetRemoved = true;
                    foreach (var rockEntry in gm.houseList)
                    {
                        if (rockEntry.rockInfo.rockIndex == targetRockIndex)
                        {
                            targetRemoved = false;
                            break;
                        }
                    }
                    
                    if (!targetRemoved)
                    {
                        failureReason = $"Failed to remove target rock #{targetRockIndex}";
                        return false;
                    }
                    
                    // Check if we removed it but with collateral damage
                    if (deviation > 1.0f)
                    {
                        failureReason = $"Removed rock but with {deviation:F2}m deviation";
                        return false; // Too messy
                    }
                    
                    return true; // Success!
                }
                else
                {
                    // No specific target - just check position
                    if (deviation > 1.2f)
                    {
                        failureReason = $"Missed position by {deviation:F2}m";
                        return false;
                    }
                    return true;
                }
            
            case ShotIntent.ScorePoints:
            case ShotIntent.LastShotScoring:
                // Draw success: Did we land in scoring position?
                Vector2 button = new Vector2(0f, 6.5f);
                float distToButton = Vector2.Distance(actualPosition, button);
                
                if (distToButton > 1.83f) // Outside 12-foot
                {
                    failureReason = $"Draw missed house (dist to button: {distToButton:F2}m)";
                    return false;
                }
                
                if (deviation > 1.0f)
                {
                    failureReason = $"Draw deviated {deviation:F2}m from target";
                    return false;
                }
                
                return true;
            
            case ShotIntent.CreateOpportunity:
                // Guard success: Did we place guard in good position?
                if (deviation > 0.8f)
                {
                    failureReason = $"Guard misplaced by {deviation:F2}m";
                    return false;
                }
                
                // Check if guard is in valid zone (Y < 4.0)
                if (actualPosition.y > 4.0f)
                {
                    failureReason = "Guard too far forward (past hog line)";
                    return false;
                }
                
                return true;
            
            case ShotIntent.ProtectLead:
                // Protection success: Did we improve our position?
                int myRocksInHouse = 0;
                foreach (var rockEntry in gm.houseList)
                {
                    if (rockEntry.rockInfo.teamName == myTeamName)
                        myRocksInHouse++;
                }
                
                if (myRocksInHouse == 0)
                {
                    failureReason = "Protection shot left no rocks in house";
                    return false;
                }
                
                if (deviation > 1.2f)
                {
                    failureReason = $"Protection shot deviated {deviation:F2}m";
                    return false;
                }
                
                return true;
            
            case ShotIntent.ForceBlank:
                // Blank success: Are we successfully blanking?
                // Check if ANY rocks in house
                if (gm.houseList.Count > 0)
                {
                    // Rocks in house - check if this is acceptable
                    if (deviation > 1.5f)
                    {
                        failureReason = "Blank attempt created messy house";
                        return false;
                    }
                }
                return true;
            
            default:
                // Unknown intent - use position deviation
                if (deviation > 1.0f)
                {
                    failureReason = $"Shot deviated {deviation:F2}m from target";
                    return false;
                }
                return true;
        }
    }
    
    /// <summary>
    /// ?? Get acceptable error threshold for shot intent
    /// Different shot types have different precision requirements
    /// </summary>
    public static float GetAcceptableErrorForIntent(ShotIntent intent)
    {
        switch (intent)
        {
            case ShotIntent.RemoveThreat:
            case ShotIntent.Desperation:
                return 0.8f; // Takeouts need precision
            
            case ShotIntent.CreateOpportunity:
                return 0.6f; // Guards need good placement
            
            case ShotIntent.ScorePoints:
            case ShotIntent.LastShotScoring:
                return 1.0f; // Draws have more tolerance
            
            case ShotIntent.ProtectLead:
                return 1.2f; // Protection shots more forgiving
            
            case ShotIntent.ForceBlank:
                return 1.5f; // Blanks very forgiving
            
            default:
                return 1.0f;
        }
    }
    
    #endregion
    
    #region UTILITY METHODS
    
    /// <summary>
    /// Adjust plan confidence based on character skills
    /// </summary>
    public static void AdjustConfidenceForSkills(EndPlan plan, CharacterStats shooter)
    {
        if (shooter == null) return;
        
        // Check if plan has draws or takeouts
        bool hasDraws = plan.plannedIntents.Contains(ShotIntent.ScorePoints);
        bool hasTakeouts = plan.plannedIntents.Contains(ShotIntent.RemoveThreat);
        bool hasFinesse = plan.plannedIntents.Contains(ShotIntent.CreateOpportunity) || 
                          plan.plannedIntents.Contains(ShotIntent.ProtectLead);
        
        if (hasDraws)
        {
            // Draw shots need good weight control (distance accuracy)
            float weightSkill = shooter.weightAccuracy.GetValue() / 100f;
            plan.confidence *= Mathf.Lerp(0.7f, 1.0f, weightSkill);
        }
        
        if (hasTakeouts)
        {
            // Takeout shots need good aim (lateral accuracy)
            float aimSkill = shooter.aimAccuracy.GetValue() / 100f;
            plan.confidence *= Mathf.Lerp(0.6f, 1.0f, aimSkill);
        }
        
        if (hasFinesse)
        {
            // Finesse shots (guards, freezes) need finesse skill
            float finesseSkill = shooter.finesseAccuracy.GetValue() / 100f;
            plan.confidence *= Mathf.Lerp(0.75f, 1.0f, finesseSkill);
        }
        
        // Clamp confidence
        plan.confidence = Mathf.Clamp01(plan.confidence);
    }
    
    /// <summary>
    /// Get number of rocks remaining for my team
    /// </summary>
    public static int GetMyRocksRemaining(GameManager gm, int rockCurrent, bool hasHammer)
    {
        int rocksPlayed = rockCurrent;
        int totalRocks = 16;
        int rocksRemaining = totalRocks - rocksPlayed;
        
        // How many are mine?
        // If I have hammer, odd rocks are mine (1,3,5,7,9,11,13,15)
        // If I don't have hammer, even rocks are mine (0,2,4,6,8,10,12,14)
        
        int myRocksRemaining = 0;
        for (int i = rockCurrent; i < totalRocks; i++)
        {
            bool isMyRock = hasHammer ? (i % 2 != 0) : (i % 2 == 0);
            if (isMyRock) myRocksRemaining++;
        }
        
        return myRocksRemaining;
    }
    
    #endregion
}
