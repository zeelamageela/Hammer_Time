using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Strategy : MonoBehaviour
{
    public GameManager gm;
    public TutorialManager tm;
    public RockManager rm;

    public AIManager aim;
    public AI_Shooter aiShoot;
    public AI_Target aiTarg;

    Rock_Info rockInfo;
    Rock_Flick rockFlick;
    Rigidbody2D rockRB;

    public Transform cenGuard;
    public Transform tCenGuard;
    public Transform lCornGuard;
    public Transform rCornGuard;

    public float takeOutOffset;
    public float peelOffset;
    public float raiseOffset;
    public float tickOffset;

    public float takeOutX;

    public float osMult;
    GameObject closestRock;
    Rock_Info closestRockInfo;

    string phase;
    
    /// <summary>
    /// Helper: Check if a guard is blocking a target rock
    /// </summary>
    private bool IsGuardBlocking(Transform guard, GameObject targetRock, float tolerance = 0.1f)
    {
        if (guard == null || targetRock == null) return false;
        return Mathf.Abs(guard.position.x - targetRock.transform.position.x) <= tolerance;
    }
    
    /// <summary>
    /// Helper: Get the rock index for a transform (guard or house rock)
    /// </summary>
    private int GetRockIndex(Transform rockTransform)
    {
        if (rockTransform == null) return -1;
        Rock_Info info = rockTransform.GetComponent<Rock_Info>();
        return info != null ? info.rockIndex : -1;
    }

    public string activeTeamName;
    public int activeTeamScore;
    public string oppTeamName;
    public int oppTeamScore;

    private void Update()
    {
        cenGuard = aiTarg.cenGuard;
        tCenGuard = aiTarg.tCenGuard;
        lCornGuard = aiTarg.lCornGuard;
        rCornGuard = aiTarg.rCornGuard;
    }
    public void SimpleAIShoot(int rockCurrent)
    {
        // Initialize active team name based on rock number and hammer
        if (rockCurrent % 2 == 0)
        {
            activeTeamName = gm.redHammer ? gm.yellowTeamName : gm.redTeamName;
        }
        else
        {
            activeTeamName = gm.redHammer ? gm.redTeamName : gm.yellowTeamName;
        }
        
        int valuableRockIndex = GetMostValuableOpponentRockIndex(activeTeamName);

        if (valuableRockIndex >= 0)
        {
            // Conservative: Only take out if the rock is in scoring position (e.g., inside the 8-foot circle)
            float distanceToButton = Vector2.Distance(
                gm.houseList[valuableRockIndex].rock.transform.position,
                new Vector2(0f, 6.5f)
            );
            if (distanceToButton < 1.22f) // 8-foot radius in meters
            {
                aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[valuableRockIndex].rockInfo.rockIndex);
                return;
            }
        }

        // If no high-value takeout, play a guard if few guards, else draw to button
        int guardsInPlay = gm.gList.Count;
        if (guardsInPlay < 2)
            aiTarg.OnTarget("Manual Guard", rockCurrent, 0);
        else
            aiTarg.OnTarget("Manual Draw", rockCurrent, 0);
    }

    private int GetMostValuableOpponentRockIndex(string myTeam)
    {
        int bestIndex = -1;
        float bestValue = float.MinValue;
        for (int i = 0; i < gm.houseList.Count; i++)
        {
            var info = gm.houseList[i].rockInfo;
            // Only consider opponent rocks
            if (info.teamName != myTeam)
            {
                // Example: Value = closer to button is better
                float value = 10f - Vector2.Distance(gm.houseList[i].rock.transform.position, new Vector2(0f, 6.5f));
                if (value > bestValue)
                {
                    bestValue = value;
                    bestIndex = i;
                }
            }
        }
        return bestIndex;
    }
    
    #region HELPER METHODS FOR INTENT-BASED STRATEGY
    
    /// <summary>
    /// Find the biggest threat to my team (opponent rock closest to button)
    /// Returns houseList index, or -1 if no threats found
    /// </summary>
    private int FindBiggestThreat(string myTeamName)
    {
        int bestThreat = -1;
        float bestThreatValue = float.MinValue;
        
        for (int i = 0; i < gm.houseList.Count; i++)
        {
            var rockEntry = gm.houseList[i];
            
            // Only opponent rocks are threats
            if (rockEntry.rockInfo.teamName == myTeamName)
                continue;
            
            // Score threat based on distance to button
            float distToButton = Vector2.Distance(rockEntry.rock.transform.position, new Vector2(0f, 6.5f));
            float threatValue = 10f - distToButton; // Closer = higher threat
            
            // BONUS: If rock is guarded, it's an even bigger threat (harder to remove)
            if (IsGuardBlocking(cenGuard, rockEntry.rock) || 
                IsGuardBlocking(lCornGuard, rockEntry.rock) || 
                IsGuardBlocking(rCornGuard, rockEntry.rock))
            {
                threatValue += 3f; // Guarded rocks are +30% more threatening
            }
            
            if (threatValue > bestThreatValue)
            {
                bestThreatValue = threatValue;
                bestThreat = rockEntry.rockInfo.rockIndex;
            }
        }
        
        return bestThreat;
    }
    
    /// <summary>
    /// Count how many of my rocks are in scoring position
    /// </summary>
    private int CountMyRocksInScoring(string myTeamName)
    {
        int count = 0;
        foreach (var rockEntry in gm.houseList)
        {
            if (rockEntry.rockInfo.teamName == myTeamName)
            {
                float distToButton = Vector2.Distance(rockEntry.rock.transform.position, new Vector2(0f, 6.5f));
                if (distToButton < 1.83f) // Within 12-foot circle
                {
                    count++;
                }
            }
        }
        return count;
    }
    
    /// <summary>
    /// Check if I have a good lead to protect (multiple scoring rocks)
    /// </summary>
    private bool HasStrongLead(string myTeamName)
    {
        int myRocks = CountMyRocksInScoring(myTeamName);
        
        // Need at least 2 rocks in scoring AND be ahead
        if (myRocks < 2) return false;
        if (activeTeamScore <= oppTeamScore) return false;
        
        return true;
    }
    
    #region INTENT-BASED SHOT SELECTION METHODS
    
    /// <summary>
    /// ?? PROOF-OF-CONCEPT: Intent-based shot selection for ConservativeSteal
    /// This demonstrates the NEW architecture - simple, clear, smart!
    /// </summary>
    private bool TryIntentBasedShot_ConservativeSteal(int rockCurrent, string phase)
    {
        Rock_Info rockInfo = gm.rockList[rockCurrent].rockInfo;
        
        Debug.Log($"[IntentBased] ConservativeSteal - {phase} phase");
        
        // PHASE 1: Identify the situation
        int threatRock = FindBiggestThreat(activeTeamName);
        int myRocksInHouse = CountMyRocksInScoring(activeTeamName);
        bool hasGuards = (gm.gList.Count > 0);
        
        // PHASE 2: Decide intent based on situation
        ShotContext context;
        
        // EARLY PHASE: Setup game
        if (phase == "early")
        {
            if (threatRock >= 0)
            {
                // They have a rock in play - remove it
                context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                context.acceptRisk = false; // Conservative - only if good shot available
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (!hasGuards)
            {
                // No threats, build defense
                context = new ShotContext(ShotIntent.CreateOpportunity);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Guards in play, draw behind them
                context = new ShotContext(ShotIntent.ScorePoints);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        // MIDDLE PHASE: Build position or remove threats
        else if (phase == "middle")
        {
            if (threatRock >= 0)
            {
                // Remove biggest threat
                context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                context.acceptRisk = (myRocksInHouse > 0); // More risk if we have rocks
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (myRocksInHouse > 1)
            {
                // We're in good shape - protect lead
                context = new ShotContext(ShotIntent.ProtectLead);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Keep building
                context = new ShotContext(ShotIntent.ScorePoints);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        // LATE PHASE: Aggressive removal or protect lead
        else if (phase == "late")
        {
            if (threatRock >= 0 && myRocksInHouse == 0)
            {
                // Must remove threat if we have nothing
                context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                context.acceptRisk = true; // More aggressive late
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (myRocksInHouse > 0 && threatRock >= 0)
            {
                // Evaluate: remove threat vs draw another
                float threatDist = Vector2.Distance(
                    gm.rockList[threatRock].rock.transform.position,
                    new Vector2(0f, 6.5f)
                );
                
                if (threatDist < 1.0f) // Threat is very close to button
                {
                    // Remove it!
                    context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                    context.acceptRisk = true;
                    context.mustScore = true; // Need to score on this shot to win
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
                else
                {
                    // Draw another rock (multi-point potential)
                    context = new ShotContext(ShotIntent.ScorePoints);
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
            }
            else
            {
                // No threats - just score!
                context = new ShotContext(ShotIntent.ScorePoints);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        // Unknown phase - use fallback
        return false;
    }
    
    /// <summary>
    /// ?? Intent-based: AggressiveHammer - Steal at all costs with hammer advantage
    /// </summary>
    private bool TryIntentBasedShot_AggressiveHammer(int rockCurrent, string phase)
    {
        Debug.Log($"[IntentBased] AggressiveHammer - {phase} phase");
        
        int threatRock = FindBiggestThreat(activeTeamName);
        int myRocksInHouse = CountMyRocksInScoring(activeTeamName);
        int oppRocksInHouse = gm.houseList.Count - myRocksInHouse;
        
        ShotContext context;
        
        // EARLY: Build aggressive setup
        if (phase == "early")
        {
            if (threatRock >= 0)
            {
                // Remove any opposition rock immediately
                context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                context.acceptRisk = true; // Aggressive!
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Build guards for corner game
                context = new ShotContext(ShotIntent.CreateOpportunity);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        // MIDDLE: Aggressive removal or setup multi-point end
        else if (phase == "middle")
        {
            if (threatRock >= 0)
            {
                // Always remove threats when aggressive
                context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                context.acceptRisk = true;
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (myRocksInHouse >= 1)
            {
                // Build on our position
                context = new ShotContext(ShotIntent.ScorePoints);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Keep setting up
                context = new ShotContext(ShotIntent.CreateOpportunity);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        // LATE: Go for the steal or setup
        else if (phase == "late")
        {
            if (activeTeamScore < oppTeamScore)
            {
                // Desperation time - must remove if any threats, even if risky
                context = new ShotContext(ShotIntent.Desperation, threatRock);
                context.acceptRisk = true;
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (threatRock >= 0 && oppRocksInHouse >= 2)
            {
                // They're building points - must remove
                context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                context.acceptRisk = true;
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (myRocksInHouse == 0 && threatRock < 0)
            {
                // Clean house - draw for steal
                context = new ShotContext(ShotIntent.ScorePoints);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Complex situation - score or protect
                if (myRocksInHouse > oppRocksInHouse)
                {
                    context = new ShotContext(ShotIntent.ScorePoints);
                }
                else
                {
                    context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                    context.acceptRisk = true; 
                }
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// ?? Intent-based: ConservativeScoreTwoOrBlankHammer - Need 2 points or keep hammer
    /// </summary>
    private bool TryIntentBasedShot_ScoreTwoOrBlank(int rockCurrent, string phase)
    {
        Debug.Log($"[IntentBased] ScoreTwoOrBlank - {phase} phase");
        
        int threatRock = FindBiggestThreat(activeTeamName);
        int myRocksInHouse = CountMyRocksInScoring(activeTeamName);
        
        ShotContext context;
        
        // Strategy: Clear any threats, build multiple scoring rocks
        
        // EARLY: Remove threats immediately, build corners
        if (phase == "early")
        {
            if (threatRock >= 0)
            {
                // Can't let them have anything - remove it
                context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                context.acceptRisk = false; // Don't risk blank
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Draw to corners for 2-point setup
                context = new ShotContext(ShotIntent.ScorePoints);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        // MIDDLE: Keep clearing, spread rocks
        else if (phase == "middle")
        {
            if (threatRock >= 0)
            {
                // Remove anything in our way
                context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                context.acceptRisk = false;
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (myRocksInHouse >= 2)
            {
                // We have 2+ rocks - protect them!
                context = new ShotContext(ShotIntent.ProtectLead);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Keep building
                context = new ShotContext(ShotIntent.ScorePoints);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        // LATE: Final decision - score 2 or blank
        else if (phase == "late")
        {
            if (myRocksInHouse >= 2 && threatRock < 0)
            {
                // We have 2+ and no threats - DONE! Just add more
                context = new ShotContext(ShotIntent.ScorePoints);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (threatRock >= 0)
            {
                // Remove final threat
                context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                context.acceptRisk = false;
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (myRocksInHouse >= 1)
            {
                // Draw for points
                context = new ShotContext(ShotIntent.ScorePoints);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Only 1 rock - can't score 2, try to blank
                context = new ShotContext(ShotIntent.ForceBlank);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// ?? Intent-based: AggressiveNotHammer - Steal at all costs without hammer
    /// </summary>
    private bool TryIntentBasedShot_AggressiveNotHammer(int rockCurrent, string phase)
    {
        Debug.Log($"[IntentBased] AggressiveNotHammer - {phase} phase");
        
        int threatRock = FindBiggestThreat(activeTeamName);
        int myRocksInHouse = CountMyRocksInScoring(activeTeamName);
        bool hasGuards = (gm.gList.Count > 0);
        
        ShotContext context;
        
        // EARLY: Aggressive setup or removal
        if (phase == "early")
        {
            if (threatRock >= 0)
            {
                // Remove immediately - can't let them build
                context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                context.acceptRisk = true; // Very aggressive
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (!hasGuards)
            {
                // Build tight guards for corner draw setup
                context = new ShotContext(ShotIntent.CreateOpportunity);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Draw behind guards
                context = new ShotContext(ShotIntent.ScorePoints);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        // MIDDLE: Keep pressure, remove threats
        else if (phase == "middle")
        {
            if (threatRock >= 0)
            {
                // Always remove when aggressive
                context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                context.acceptRisk = true;
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (myRocksInHouse > 0)
            {
                // Build on position
                context = new ShotContext(ShotIntent.ScorePoints);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Keep setting up
                context = new ShotContext(ShotIntent.CreateOpportunity);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        // LATE: All-in for steal
        else if (phase == "late")
        {
            if (threatRock >= 0 && myRocksInHouse == 0)
            {
                // Must remove to have any chance
                context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                context.acceptRisk = true;
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (myRocksInHouse > 0 && threatRock >= 0)
            {
                // Decide: protect or attack
                float myBestDist = 999f;
                float theirBestDist = 999f;
                
                foreach (var rock in gm.houseList)
                {
                    float dist = Vector2.Distance(rock.rock.transform.position, new Vector2(0f, 6.5f));
                    if (rock.rockInfo.teamName == activeTeamName && dist < myBestDist)
                        myBestDist = dist;
                    else if (rock.rockInfo.teamName != activeTeamName && dist < theirBestDist)
                        theirBestDist = dist;
                }

                if (theirBestDist < myBestDist)
                {
                    // They're closer - attack!
                    if (rockCurrent >= 6) // Last rock - must score or remove
                    {
                        if (activeTeamScore <= oppTeamScore)
                        {
                            context = new ShotContext(ShotIntent.Desperation, threatRock);
                            context.acceptRisk = true;
                            context.mustScore = true; // Need to score to win
                            aiTarg.ExecuteIntent(context, rockCurrent);
                            return true;
                        }
                        else
                        {
                            context = new ShotContext(ShotIntent.ScorePoints, threatRock);
                            context.acceptRisk = true;
                            aiTarg.ExecuteIntent(context, rockCurrent);
                            return true;
                        }
                    }
                    else 
                    { 
                        context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                        context.acceptRisk = true;
                        aiTarg.ExecuteIntent(context, rockCurrent);
                        return true;
                    }
                }
                else
                {
                    // We're closer - add more
                    context = new ShotContext(ShotIntent.ScorePoints);
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
            }
            else
            {
                // Just score
                context = new ShotContext(ShotIntent.ScorePoints);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// ?? Intent-based: ConservativeStealOrBlank - Force them to 1 or blank the end
    /// </summary>
    private bool TryIntentBasedShot_StealOrBlank(int rockCurrent, string phase)
    {
        Debug.Log($"[IntentBased] StealOrBlank - {phase} phase");
        
        int threatRock = FindBiggestThreat(activeTeamName);
        int myRocksInHouse = CountMyRocksInScoring(activeTeamName);
        int oppRocksInHouse = gm.houseList.Count - myRocksInHouse;
        
        ShotContext context;
        
        // Strategy: Deny them points, steal if possible, blank acceptable
        
        // EARLY: Remove any threats, don't build unless safe
        if (phase == "early")
        {
            if (threatRock >= 0)
            {
                // Remove it - can't let them build
                context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                context.acceptRisk = false; // Conservative
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (myRocksInHouse > 0)
            {
                // We have rocks - guard them
                context = new ShotContext(ShotIntent.ProtectLead);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Draw for steal attempt
                context = new ShotContext(ShotIntent.ScorePoints);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        // MIDDLE: Keep clearing, build cautiously
        else if (phase == "middle")
        {
            if (threatRock >= 0)
            {
                // Remove threats
                context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                context.acceptRisk = false;
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (myRocksInHouse > 0)
            {
                // Protect what we have
                context = new ShotContext(ShotIntent.ProtectLead);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Draw cautiously
                context = new ShotContext(ShotIntent.ScorePoints);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        // LATE: Final decision - steal, blank, or limit damage
        else if (phase == "late")
        {
            if (oppRocksInHouse >= 2)
            {
                // They have 2+ rocks - we MUST remove or blank
                if (threatRock >= 0)
                {
                    context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                    context.acceptRisk = false;
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
                else
                {
                    // Try to blank (draw wide or throw away)
                    context = new ShotContext(ShotIntent.ForceBlank);
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
            }
            else if (threatRock >= 0)
            {
                // Remove final threat
                context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                context.acceptRisk = false;
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (myRocksInHouse > 0)
            {
                // We might steal - protect!
                context = new ShotContext(ShotIntent.ProtectLead);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Blank is fine
                context = new ShotContext(ShotIntent.ForceBlank);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        return false;
    }
    
    #endregion
    
    #endregion // INTENT-BASED SHOT SELECTION METHODS

    public void OnShot(int rockCurrent)
    {
        if (gm.houseList.Count != 0)
        {
            closestRock = gm.houseList[0].rock;
            closestRockInfo = gm.houseList[0].rockInfo;
        }

        if (rockCurrent % 2 == 0)
        {
            if (gm.redHammer)
            {
                activeTeamName = gm.yellowTeamName;
                activeTeamScore = gm.yellowScore;
                oppTeamName = gm.redTeamName;
                oppTeamScore = gm.redScore;
            }
            else
            {
                activeTeamName = gm.redTeamName;
                activeTeamScore = gm.redScore;
                oppTeamName = gm.yellowTeamName;
                oppTeamScore = gm.yellowScore;
            }
        }
        else
        {
            if (gm.redHammer)
            {
                activeTeamName = gm.redTeamName;
                activeTeamScore = gm.redScore;
                oppTeamName = gm.yellowTeamName;
                oppTeamScore = gm.yellowScore;
            }
            else
            {
                activeTeamName = gm.yellowTeamName;
                activeTeamScore = gm.yellowScore;
                oppTeamName = gm.redTeamName;
                oppTeamScore = gm.redScore;
            }
        }
        //early phase is shots 1-2 in an 8 rock game
        if (rockCurrent < 4)
        {
            phase = "early";
        }
        //middle phase is shots 3-5 in an 8 rock game
        else if (rockCurrent < 10)
        {
            phase = "middle";
        }
        //late phase is shots 6-8 in an 8 rock game
        else
        {
            phase = "late";
        }

        if (rockCurrent % 2 == 0)
        {
            //two or more ends left
            if (gm.endTotal - gm.endCurrent >= 2)
            {
                if (activeTeamScore - oppTeamScore >= 2)
                    AggressiveNotHammer(rockCurrent, phase);
                else if (activeTeamScore <= oppTeamScore)
                    ConservativeStealOrBlank(rockCurrent, phase);
                else
                    ConservativeSteal(rockCurrent, phase);
            }
            //one end left
            else if (gm.endTotal - gm.endCurrent == 1)
            {
                if (activeTeamScore - oppTeamScore <= 1)
                    ConservativeStealOrBlank(rockCurrent, phase);
                else
                    AggressiveNotHammer(rockCurrent, phase);
            }
            else if (activeTeamScore < oppTeamScore)
                ConservativeStealOrBlank(rockCurrent, phase);
            else
                AggressiveNotHammer(rockCurrent, phase);
        }
        else
        {
            if (gm.endTotal - gm.endCurrent >= 2)
            {
                if (activeTeamScore < oppTeamScore)
                    AggressiveHammer(rockCurrent, phase);
                else
                    ConservativeScoreTwoOrBlankHammer(rockCurrent, phase);
            }
            else if (gm.endTotal - gm.endCurrent == 1)
            {
                if (activeTeamScore - oppTeamScore <= 1)
                    ConservativeScoreTwoOrBlankHammer(rockCurrent, phase);
                else
                    AggressiveHammer(rockCurrent, phase);
            }
            else
            {
                if (activeTeamScore < oppTeamScore)
                    AggressiveHammer(rockCurrent, phase);
                else
                    ConservativeScoreTwoOrBlankHammer(rockCurrent, phase);
            }
                
        }
        Debug.Log("Phase is " + phase);
    }

    public void ConservativeSteal(int rockCurrent, string phase)
    {
        GameObject rock = gm.rockList[rockCurrent].rock;
        Rock_Info rockInfo = gm.rockList[rockCurrent].rockInfo;

        if (gm.houseList.Count != 0)
        {
            closestRock = gm.houseList[0].rock;
            closestRockInfo = gm.houseList[0].rockInfo;
        }

        Debug.Log("Conservative Steal - " + phase);

        aiTarg.OnTarget("Guard Reading", rockCurrent, 0);
        
        // ? NEW ARCHITECTURE: Try intent-based shot selection FIRST!
        if (TryIntentBasedShot_ConservativeSteal(rockCurrent, phase))
        {
            Debug.Log("[ConservativeSteal] ? Intent-based shot selected!");
            return;
        }
        
        Debug.Log("[ConservativeSteal] ? Intent-based failed, using legacy logic...");

        switch (phase)
        {
            #region Early Not Hammer
            case "early":
                //if there's rocks in the house
                if (gm.houseList.Count > 0)
                {
                    //if I have shot rock
                    if (closestRockInfo.teamName == rockInfo.teamName)
                    {
                        aiTarg.OnTarget("Manual Guard", rockCurrent, 0);
                    }
                    //if they have shot rock
                    else
                    {
                        //if it's in the centre
                        if (Mathf.Abs(closestRock.transform.position.x) <= 0.5f)
                        {
                            if (cenGuard || tCenGuard)
                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            else
                                aiTarg.OnTarget("Tap Back", rockCurrent, closestRockInfo.rockIndex);
                        }
                        //if it's on the wings and it's above the tee line
                        else if (closestRock.transform.position.y <= 6.5f)
                            aiTarg.OnTarget("Tap Back", rockCurrent, closestRockInfo.rockIndex);
                        else
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }
                }
                //if there's guards
                else if (gm.gList.Count != 0)
                {
                    if (cenGuard || tCenGuard)
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    else
                        aiShoot.OnShot("Centre Guard", rockCurrent);
                }
                else
                    aiShoot.OnShot("Centre Guard", rockCurrent);

                break;
            #endregion

            #region Middle Not Hammer
            case "middle":

                if (gm.houseList.Count != 0)
                {
                    //if I have shot rock
                    if (closestRockInfo.teamName == rockInfo.teamName)
                    {
                        if (gm.houseList.Count > 1)
                        {
                            if (gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                            {
                                if (Mathf.Abs(gm.houseList[1].rock.transform.position.x) <= 0.5f)
                                {
                                    if (cenGuard || tCenGuard)
                                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                    else
                                        aiShoot.OnShot("Centre Guard", rockCurrent);
                                }
                                else if (gm.houseList[1].rock.transform.position.x < 0)
                                {
                                if (lCornGuard)
                                    aiTarg.OnTarget("Peel", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                else if (gm.houseList.Count > 1)
                                    aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                else
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                }
                                else
                                {
                                    if (rCornGuard)
                                        aiTarg.OnTarget("Peel", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                    else if (gm.houseList.Count > 1)
                                        aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                    else
                                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                }
                            }
                            else
                            {
                                if (cenGuard || tCenGuard)
                                {
                                    if (lCornGuard)
                                        aiTarg.OnTarget("Peel", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                    else if (rCornGuard)
                                        aiTarg.OnTarget("Peel", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                    else
                                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                }
                                else
                                    aiShoot.OnShot("Centre Guard", rockCurrent);
                            }
                        }
                        else
                        {
                            if (cenGuard || tCenGuard)
                            {
                                if (lCornGuard)
                                    aiTarg.OnTarget("Peel", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                else if (rCornGuard)
                                    aiTarg.OnTarget("Peel", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                else
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            }
                            else
                                aiShoot.OnShot("Centre Guard", rockCurrent);
                        }
                    }
                    //if they have shot rock
                    else
                    {
                        //if it's in the centre
                        if (Mathf.Abs(closestRock.transform.position.x) <= 0.5f)
                        {
                            if (cenGuard || tCenGuard)
                            {
                                if (IsGuardBlocking(cenGuard, closestRock) || IsGuardBlocking(tCenGuard, closestRock))
                                {
                                    if (gm.houseList.Count > 1)
                                    {
                                        if (gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                                        {
                                            if (gm.houseList.Count > 1 && (IsGuardBlocking(cenGuard, gm.houseList[1].rock) || IsGuardBlocking(tCenGuard, gm.houseList[1].rock)))
                                            {
                                                if (cenGuard)
                                                    aiTarg.OnTarget("Peel", rockCurrent, cenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                else if (tCenGuard)
                                                    aiTarg.OnTarget("Peel", rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                else
                                                    aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                            }
                                            else if (gm.houseList.Count > 1)
                                            {
                                                if (gm.houseList[1].rock.transform.position.x < 0)
                                                {
                                                    if (lCornGuard)
                                                        aiTarg.OnTarget("Peel", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                    else
                                                        aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                                }
                                                else
                                                {
                                                    if (rCornGuard)
                                                        aiTarg.OnTarget("Peel", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                    else
                                                        aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                                }
                                            }
                                            else
                                            {
                                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                            }
                                        }
                                        else
                                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                    }
                else
                {
                    if (cenGuard)
                        aiTarg.OnTarget("Peel", rockCurrent, cenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                    else if (tCenGuard)
                        aiTarg.OnTarget("Peel", rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                    else if (gm.houseList.Count > 1)
                        aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                    else
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                }
                                }
                                else
                                    aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);

                            }  
                            else
                                aiTarg.OnTarget("Tap Back", rockCurrent, closestRockInfo.rockIndex);
                        }
                        //if it's on the wings and it's above the tee line
                        else if (closestRock.transform.position.y <= 6.5f)
                        {
                            if (closestRock.transform.position.x < 0)
                            {
                                if (lCornGuard)
                                    aiTarg.OnTarget("Peel", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                else
                                    aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                            }
                            else
                            {
                                if (rCornGuard)
                                    aiTarg.OnTarget("Peel", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                else
                                    aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                            }
                        }
                        else
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }
                }
                //if there's guards
                else if (gm.gList.Count != 0)
                {
                    if (rCornGuard)
                        aiTarg.OnTarget("Peel", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                    else if (lCornGuard)
                        aiTarg.OnTarget("Peel", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                    else
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                }
                else
                    aiShoot.OnShot("Centre Guard", rockCurrent);

                break;
            #endregion

            #region Late Not Hammer
            case "late":
                //there's rocks in the house
                if (gm.houseList.Count != 0)
                {
                    //They have shot rock
                    if (closestRockInfo.teamName != rockInfo.teamName)
                    {
                        if (Mathf.Abs(closestRock.transform.position.x) <= 0.5f && (cenGuard || tCenGuard))
                        {
                            if (cenGuard)
                            {
                                if (Mathf.Abs(cenGuard.position.x - closestRock.transform.position.x) > 0.1f)
                                    aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                                else
                                    aiTarg.OnTarget("Peel", rockCurrent, cenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                            }
                            //there's a tight centre guard
                            else if (tCenGuard)
                            {
                                if (Mathf.Abs(tCenGuard.position.x - closestRock.transform.position.x) > 0.1f)
                                    aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                                else
                                    aiTarg.OnTarget("Peel", rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                            }
                        }
                        //there's a centre guard

                        //it's on the left wing
                        else if (closestRock.transform.position.x < 0 && lCornGuard)
                        {
                            if (Mathf.Abs(lCornGuard.position.x - closestRock.transform.position.x) > 0.1f)
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            else
                                aiTarg.OnTarget("Peel", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                        }
                        //it's on the right wing
                        else if (closestRock.transform.position.x > 0 && rCornGuard)
                        {
                            if (Mathf.Abs(rCornGuard.position.x - closestRock.transform.position.x) > 0.1f)
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            else
                                aiTarg.OnTarget("Peel", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                        }
                        else
                            aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                    }
                    //I have shot rock
                    else if (closestRockInfo.teamName == rockInfo.teamName)
                    {
                        //more than one rock in house
                        if (gm.houseList.Count > 1)
                        {
                            //I have second shot
                            if (gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                            {
                                //second shot is to the left
                                if (gm.houseList[1].rock.transform.position.x <= 0f)
                                    aiShoot.OnShot("Left Corner Guard", rockCurrent);
                                else if (gm.houseList[1].rock.transform.position.x > 0f)
                                    aiShoot.OnShot("Right Corner Guard", rockCurrent);
                                else if (cenGuard || tCenGuard)
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                else
                                    aiShoot.OnShot("Centre Guard", rockCurrent);
                            }
                            else
                            {
                                //second shot is in the middle
                                if (Mathf.Abs(gm.houseList[1].rock.transform.position.x) <= 0.5f)
                                {
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                }
                                //second shot is not in the middle
                                else
                                {
                                    //second shot is to the left
                                    if (gm.houseList[1].rock.transform.position.x < 0)
                                    {
                                        if (lCornGuard)
                                            aiTarg.OnTarget("Peel", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                        else
                                            aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                    }
                                    //second shot is to the right
                                    else
                                    {
                                        if (rCornGuard)
                                            aiTarg.OnTarget("Peel", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                        else
                                            aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                    }
                                }

                            }
                        }
                        //only one rock in house
                        else
                        {
                            if (Mathf.Abs(closestRock.transform.position.x) <= 0.5f)
                            {
                                if (cenGuard || tCenGuard)
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                else
                                    aiShoot.OnShot("Centre Guard", rockCurrent);
                            }
                            else
                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                    }
                }
                else
                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);

                break;
            #endregion

            default:
                break;
        }
    }

    public void AggressiveHammer(int rockCurrent, string phase)
    {
        //Aggressive is to steal at all costs
        GameObject rock = gm.rockList[rockCurrent].rock;
        Rock_Info rockInfo = gm.rockList[rockCurrent].rockInfo;

        if (gm.houseList.Count != 0)
        {
            closestRock = gm.houseList[0].rock;
            closestRockInfo = gm.houseList[0].rockInfo;
        }

        Debug.Log("Aggressive Hammer - " + phase);

        aiTarg.OnTarget("Guard Reading", rockCurrent, 0);
        
        // ? NEW ARCHITECTURE: Try intent-based shot selection FIRST!
        if (TryIntentBasedShot_AggressiveHammer(rockCurrent, phase))
        {
            Debug.Log("[AggressiveHammer] ? Intent-based shot selected!");
            return;
        }
        
        Debug.Log("[AggressiveHammer] ? Intent-based failed, using legacy logic...");

        switch (phase)
        {
            #region Early Hammer
            case "early":

                //left corner guard
                if (lCornGuard)
                {
                    aiTarg.OnTarget("Manual Guard", rockCurrent, 0);
                }
                //right corner guard
                else if (rCornGuard)
                {
                    aiTarg.OnTarget("Manual Guard", rockCurrent, 0);
                }
                else if (cenGuard)
                {
                    aiTarg.OnTarget("Tick Shot", rockCurrent, cenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                }
                else if (tCenGuard)
                {
                    aiTarg.OnTarget("Tick Shot", rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                }
                //left and right corner guard
                else if (rCornGuard && lCornGuard)
                {
                    aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                }
                else
                {
                    if (Random.value > 0.5f)
                        aiShoot.OnShot("Left Corner Guard", rockCurrent);
                    else
                        aiShoot.OnShot("Right Corner Guard", rockCurrent);
                }

                break;
            #endregion

            #region Middle Hammer
            case "middle":

                //no one is in the house
                if (gm.houseList.Count == 0)
                {
                    //tight centre and no centre guard
                    if (lCornGuard && !rCornGuard)
                    {
                        aiTarg.OnTarget("Manual Guard", rockCurrent, 0);
                    }
                    //centre and no tight centre guard
                    else if (rCornGuard && !lCornGuard)
                    {
                        aiTarg.OnTarget("Manual Guard", rockCurrent, 0);
                    }
                    //centre and tight centre guards
                    else if (lCornGuard && rCornGuard)
                    {
                        aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                    }
                    else
                    {
                        if (Random.value > 0.5f)
                            aiShoot.OnShot("Left Corner Guard", rockCurrent);
                        else
                            aiShoot.OnShot("Right Corner Guard", rockCurrent);
                    }
                }
                //closest rock is mine
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    //left guard only
                    if (lCornGuard && !rCornGuard)
                    {
                        if (Mathf.Abs(closestRock.transform.position.x - lCornGuard.position.x) >= 0.1f)
                        {
                            aiShoot.OnShot("Left High Corner Guard", rockCurrent);
                        }
                        else
                        {
                            rm.inturn = false;
                            aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                        }
                    }
                    //right guard only
                    else if (rCornGuard && !lCornGuard)
                    {
                        //the rock is not guarded
                        if (Mathf.Abs(closestRock.transform.position.x - rCornGuard.position.x) >= 0.1f)
                        {
                            aiShoot.OnShot("Right High Corner Guard", rockCurrent);
                        }
                        else
                        {
                            rm.inturn = true;
                            aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                        }
                    }
                    //left and right corner guards
                    else if (rCornGuard && lCornGuard)
                    {
                        //the rock is not guarded on the left
                        if (Mathf.Abs(closestRock.transform.position.x - lCornGuard.position.x) >= 0.1f)
                        {
                            aiShoot.OnShot("Right High Corner Guard", rockCurrent);
                        }
                        //the rock is not guarded on the right
                        else if (Mathf.Abs(closestRock.transform.position.x - rCornGuard.position.x) >= 0.1f)
                        {
                            aiShoot.OnShot("Left High Corner Guard", rockCurrent);
                        }
                        else
                        {
                            if (closestRock.transform.position.x < 0)
                            {
                                rm.inturn = true;
                                aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                            }
                            else if (closestRock.transform.position.x > 0)
                            {
                                rm.inturn = false;
                                aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                            }
                            else aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                        }
                    }
                    //any other guard combo
                    else aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                }
                //closest rock is not mine
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    //if it's in the four foot
                    if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                    {
                        //tight centre guard
                        if (tCenGuard)
                        {
                            //if the tight centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - tCenGuard.position.x) >= 0.1f)
                            {
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            }
                            else
                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        //centre guard
                        if (cenGuard)
                        {
                            //if the tight centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f)
                            {
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            }
                            else
                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else
                        {
                            aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                        }
                    }
                    //if it's not in the four foot and there's more than one rock in the house
                    else if (gm.houseList.Count > 1)
                    {
                        //if the second shot is mine
                        if (gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                        {

                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else
                        {
                            aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                        }
                    }
                    //not in the four foot and the only rock
                    else
                    {
                        aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                    }
                }
                break;
            #endregion

            #region Late Hammer
            case "late":

                //no one is in the house
                if (gm.houseList.Count == 0)
                {
                    if (rockCurrent < 15)
                    {
                        //left corner and no right corner guard
                        if (lCornGuard && !rCornGuard)
                        {
                            aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                        }
                        //right corner and no left corner guard
                        else if (rCornGuard && !lCornGuard)
                        {
                            aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                        }
                        //right corner and left corner guards
                        else if (lCornGuard && rCornGuard)
                        {
                            aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                        }
                        else
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                    }
                    else
                    {
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }
                }
                //closest rock is mine
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    if (rockCurrent < 15)
                    {
                        //left guard only
                        if (lCornGuard && !rCornGuard)
                        {
                            if (Mathf.Abs(closestRock.transform.position.x - lCornGuard.position.x) >= 0.1f)
                            {
                                aiShoot.OnShot("Left High Corner Guard", rockCurrent);
                            }
                            else
                            {
                                rm.inturn = false;
                                aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                            }
                        }
                        //right guard only
                        else if (rCornGuard && !lCornGuard)
                        {
                            //the rock is not guarded
                            if (Mathf.Abs(closestRock.transform.position.x - rCornGuard.position.x) >= 0.1f)
                            {
                                aiShoot.OnShot("Right High Corner Guard", rockCurrent);
                            }
                            else
                            {
                                rm.inturn = true;
                                aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                            }
                        }
                        //left and right corner guards
                        else if (rCornGuard && lCornGuard)
                        {
                            //the rock is not guarded on the left
                            if (Mathf.Abs(closestRock.transform.position.x - lCornGuard.position.x) >= 0.1f)
                            {
                                aiShoot.OnShot("Right High Corner Guard", rockCurrent);
                            }
                            //the rock is not guarded on the right
                            else if (Mathf.Abs(closestRock.transform.position.x - rCornGuard.position.x) >= 0.1f)
                            {
                                aiShoot.OnShot("High Left Corner Guard", rockCurrent);
                            }
                            else
                            {
                                if (closestRock.transform.position.x < 0)
                                {
                                    rm.inturn = true;
                                    aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                                }
                                else if (closestRock.transform.position.x > 0)
                                {
                                    rm.inturn = false;
                                    aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                                }
                                else
                                    aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                            }
                        }
                        //any other guard combo
                        else
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, rockCurrent);
                    }
                    else
                    {
                        //if there's more than one rock in the house
                        if (gm.houseList.Count > 1)
                        {
                            //if the second shot is mine
                            if (gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                            {
                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            }
                            else
                            {
                                aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                            }
                        }
                        else
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                    }
                }
                //closest rock is not mine
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    if (rockCurrent < 15)
                    {
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }
                    else
                    {
                        //if it's in the four foot
                        if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                        {
                            //tight centre guard
                            if (tCenGuard)
                            {
                                //if the tight centre guard is not covering it
                                if (Mathf.Abs(closestRock.transform.position.x - tCenGuard.position.x) >= 0.1f)
                                {
                                    aiTarg.OnTarget("Tap Back", rockCurrent, closestRockInfo.rockIndex);
                                }
                                else
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            }
                            //centre guard
                            if (cenGuard)
                            {
                                //if the centre guard is not covering it
                                if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f)
                                {
                                    aiTarg.OnTarget("Tap Back", rockCurrent, closestRockInfo.rockIndex);
                                }
                                else
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            }
                            else
                            {
                                aiTarg.OnTarget("Tap Back", rockCurrent, closestRockInfo.rockIndex);
                            }
                        }
                        //if it's not in the four foot and there's more than one rock in the house
                        else if (gm.houseList.Count > 1)
                        {
                            //if the second shot is mine
                            if (gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                            {

                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            }
                            else
                            {
                                aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                            }
                        }
                        //not in the four foot and the only rock
                        else
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                    }

                }
                break;
            #endregion

            default:
                Debug.Log("Something's gone wrong in AISweeper");
                break;
        }
    }

    public void ConservativeScoreTwoOrBlankHammer(int rockCurrent, string phase)
    {
        GameObject rock = gm.rockList[rockCurrent].rock;
        Rock_Info rockInfo = gm.rockList[rockCurrent].rockInfo;

        if (gm.houseList.Count != 0)
        {
            closestRock = gm.houseList[0].rock;
            closestRockInfo = gm.houseList[0].rockInfo;
        }

        Debug.Log("Score Two or Blank - " + phase);

        aiTarg.OnTarget("Guard Reading", rockCurrent, 0);
        
        // ? NEW ARCHITECTURE: Try intent-based shot selection FIRST!
        if (TryIntentBasedShot_ScoreTwoOrBlank(rockCurrent, phase))
        {
            Debug.Log("[ScoreTwoOrBlank] ? Intent-based shot selected!");
            return;
        }
        
        Debug.Log("[ScoreTwoOrBlank] ? Intent-based failed, using legacy logic...");

        switch (phase)
        {
            #region Early Hammer
            case "early":
                //if there's rocks in the house
                if (gm.houseList.Count != 0)
                {
                    //if they are shot rock
                    if (closestRockInfo.teamName != rockInfo.teamName)
                    {
                        //take it out
                        aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                    }
                    else
                    {
                        //if the rock is to the left
                        if (closestRock.transform.position.x <= 0f)
                        {
                            aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                        }
                        else
                        {
                            aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                        }
                    }
                }
                else
                {
                    if (Random.value > 0.5f)
                        aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                    else
                        aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                }

                break;
            #endregion

            #region Middle Hammer
            case "middle":

                if (gm.houseList.Count != 0)
                {
                    if (closestRockInfo.teamName != rockInfo.teamName)
                    {

                        aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                    }
                    else if (closestRockInfo.teamName == rockInfo.teamName)
                    {
                        if (gm.houseList.Count > 1)
                        {
                            if (gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                            {
                                if (gm.houseList[1].rock.transform.position.x > 0.5f && rCornGuard)
                                {
                                    aiTarg.OnTarget("Take Out", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                }
                                else if (gm.houseList[1].rock.transform.position.x < -0.5f && lCornGuard)
                                {
                                    aiTarg.OnTarget("Take Out", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                }
                                else
                                    aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                            }
                            else
                            {
                                if (cenGuard)
                                    aiShoot.OnShot("High Centre Guard", rockCurrent);
                                else if (tCenGuard)
                                    aiShoot.OnShot("Centre Guard", rockCurrent);
                                else
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            }
                        }
                        else
                        {
                            if (closestRock.transform.position.y < 6.6f)
                            {
                                if (closestRock.transform.position.x > 0f)
                                    aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                                else
                                    aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                            }
                            else
                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                    }
                }
                else
                {
                    if (rCornGuard)
                    {
                        aiTarg.OnTarget("Take Out", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                    }
                    else if (lCornGuard)
                    {
                        aiTarg.OnTarget("Take Out", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                    }
                    else
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                }

                break;
            #endregion

            #region Late Hammer
            case "late":

                if (gm.houseList.Count > 0)
                {
                    
                    if (closestRockInfo.teamName != rockInfo.teamName)
                    {
                        if (gm.rockCurrent == 15)
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else
                        {
                            bool targFound = false;
                            int rockTarget = 99;
                            for (int i = 0; i < gm.gList.Count; i++)
                            {
                                if (!targFound && Mathf.Abs(gm.gList[i].lastTransform.position.x) < 0.25f)
                                {
                                    rockTarget = gm.gList[i].rockIndex;
                                    targFound = true;
                                }
                            }
                            if (targFound)
                            {
                                aiTarg.OnTarget("Peel", rockCurrent, rockTarget);
                            }
                            else
                            {
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            }
                        }


                    }
                    else if (closestRockInfo.teamName == rockInfo.teamName)
                    {
                        if (gm.rockCurrent == 15)
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else
                        {
                            if (gm.houseList.Count > 1)
                            {
                                if (gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                                {
                                    if (Mathf.Abs(gm.houseList[1].rock.transform.position.x) < 0.5f)
                                    {
                                        bool targFound = false;
                                        int rockTarget = 99;
                                        for (int i = 0; i < gm.gList.Count; i++)
                                        {
                                            if (!targFound && Mathf.Abs(gm.gList[i].lastTransform.position.x) < 0.25f)
                                            {
                                                rockTarget = gm.gList[i].rockIndex;
                                                targFound = true;
                                            }
                                        }
                                        if (targFound)
                                        {
                                            aiTarg.OnTarget("Peel", rockCurrent, rockTarget);
                                        }
                                        else
                                        {
                                            if (closestRock.transform.position.x < 0f)
                                            {
                                                aiShoot.OnShot("Left Corner Guard", rockCurrent);
                                            }
                                            else
                                            {
                                                aiShoot.OnShot("Right Corner Guard", rockCurrent);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (closestRock.transform.position.x < 0f)
                                        {
                                            aiShoot.OnShot("Left Corner Guard", rockCurrent);
                                        }
                                        else
                                        {
                                            aiShoot.OnShot("Right Corner Guard", rockCurrent);
                                        }
                                    }
                                }
                                else
                                {
                                    aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                }
                            }
                            else
                            {
                                if (closestRock.transform.position.x < 0f)
                                {
                                    aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                                }
                                else
                                {
                                    aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                                }
                            }

                        }

                    }
                }
                else
                {
                    if (gm.rockCurrent < 15)
                        aiTarg.OnTarget("Take Out", rockCurrent, 0);
                    else
                    {
                        if (Random.value > 0.5f)
                            aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                        else
                            aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                    }
                }

                break;
            #endregion

            default:
                break;
        }
    }

    public void AggressiveNotHammer(int rockCurrent, string phase)
    {
        //Aggressive is to steal at all costs
        GameObject rock = gm.rockList[rockCurrent].rock;
        Rock_Info rockInfo = gm.rockList[rockCurrent].rockInfo;

        if (gm.houseList.Count != 0)
        {
            closestRock = gm.houseList[0].rock;
            closestRockInfo = gm.houseList[0].rockInfo;
        }

        Debug.Log("Aggressive Not Hammer - " + phase);

        aiTarg.OnTarget("Guard Reading", rockCurrent, 0);
        
        // ? NEW ARCHITECTURE: Try intent-based shot selection FIRST!
        if (TryIntentBasedShot_AggressiveNotHammer(rockCurrent, phase))
        {
            Debug.Log("[AggressiveNotHammer] ? Intent-based shot selected!");
            return;
        }
        
        Debug.Log("[AggressiveNotHammer] ? Intent-based failed, using legacy logic...");

        switch (phase)
        {
            #region Early No Hammer
            case "early":

                //no one is in the house
                if (gm.houseList.Count == 0)
                {
                    //tight centre and no centre guard
                    if (tCenGuard && !cenGuard)
                    {
                        aiShoot.OnShot("Centre Guard", rockCurrent);
                    }
                    //centre and no tight centre guard
                    else if (cenGuard && !tCenGuard)
                    {
                        aiShoot.OnShot("Tight Centre Guard", rockCurrent);
                    }
                    //centre and tight centre guards
                    else if (cenGuard && tCenGuard)
                    {
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, rockCurrent);
                    }
                    else
                    {
                        //randomly choose
                        if (Random.value > 0.5f)
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, rockCurrent);
                        }
                        else aiShoot.OnShot("Tight Centre Guard", rockCurrent);
                    }
                }
                //closest rock is mine
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    //tight centre and no centre guard
                    if (tCenGuard && !cenGuard)
                    {
                        if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                        {
                            aiShoot.OnShot("Tight Centre Guard", rockCurrent);
                        }
                        else
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                    }
                    //centre and no tight centre guard
                    else if (cenGuard && !tCenGuard)
                    {
                        if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                        {
                            aiShoot.OnShot("High Centre Guard", rockCurrent);
                        }
                        else
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                    }
                    //centre and tight centre guards
                    else if (cenGuard && tCenGuard)
                    {
                        if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                        {
                            aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                        }
                        else
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                    }
                    //any other guard combo
                    else
                    {
                        //randomly choose
                        if (Random.value > 0.5f)
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else aiShoot.OnShot("Tight Centre Guard", rockCurrent);
                    }
                }
                //closest rock is not mine
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    //if it's in the four foot
                    if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                    {
                        //if there's guards
                        if (gm.gList.Count != 0)
                        {
                            // Check if guard is blocking - if not, take out the rock
                            if (!IsGuardBlocking(cenGuard, closestRock))
                            {
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            }
                            else
                            {
                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            }
                        }
                    }
                    else if (gm.houseList.Count >= 2)
                    {
                        if (gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else
                        {
                            // Physics-based takeout of second closest rock
                            aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                        }
                    }
                    else
                    {
                        aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                    }
                }
                break;
            #endregion

            #region Middle No Hammer
            case "middle":
                //no one is in the house
                if (gm.houseList.Count == 0)
                {
                    //tight centre and no centre guard
                    if (tCenGuard && !cenGuard)
                    {
                        aiShoot.OnShot("Centre Guard", rockCurrent);
                    }
                    //centre and no tight centre guard
                    else if (cenGuard && !tCenGuard)
                    {
                        aiShoot.OnShot("Tight Centre Guard", rockCurrent);
                    }
                    //centre and tight centre guards
                    else if (cenGuard && tCenGuard)
                    {
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }
                    else
                    {
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }
                }
                //closest rock is mine
                else if (closestRockInfo.teamName == rockInfo.teamName)
                {
                    //tight centre and no centre guard
                    if (tCenGuard && !cenGuard)
                    {
                        if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                        {
                            aiShoot.OnShot("Centre Guard", rockCurrent);
                        }
                        else
                        {
                            aiTarg.OnTarget("DrawFourFoot", rockCurrent, rockCurrent);
                        }
                    }
                    //centre and no tight centre guard
                    else if (cenGuard && !tCenGuard)
                    {
                        if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                        {
                            aiShoot.OnShot("High Centre Guard", rockCurrent);
                        }
                        else
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                    }
                    //centre and tight centre guards
                    else if (cenGuard && tCenGuard)
                    {
                        //if closest is in the four foot
                        if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                        {
                            //if there's more than one rock in the house
                            if (gm.houseList.Count > 1)
                            {
                                //if the second shot is mine
                                if (gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                                {
                                    if (gm.houseList[1].rock.transform.position.x < 0f)
                                    {
                                        aiShoot.OnShot("Left Corner Guard", rockCurrent);
                                    }
                                    else aiShoot.OnShot("Right Corner Guard", rockCurrent);
                                }
                                //if second shot is not mine
                                else
                                {
                                    aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                }
                            }
                            //if there's only one rock in the house
                            else
                                aiTarg.OnTarget("Auto Draw Twelve Foot", rockCurrent, rockCurrent);
                        }
                        else
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                    }
                    //any other guard combo
                    else
                    {
                        aiShoot.OnShot("Centre Guard", rockCurrent);
                    }
                }
                //closest rock is not mine
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    //if it's in the four foot
                    if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                    {
                        //tight centre and no centre guard
                        if (tCenGuard && !cenGuard)
                        {
                            //if the tight centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - tCenGuard.position.x) >= 0.1f)
                            {
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            }
                            else
                                aiTarg.OnTarget("Take Out", rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                        }
                        //centre and no tight centre guard
                        else if (cenGuard && !tCenGuard)
                        {
                            //if the centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f)
                            {
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            }
                            else aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        //centre and tight centre guards
                        else if (cenGuard && tCenGuard)
                        {
                            //if the centre and tight centre guards are not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f & Mathf.Abs(closestRock.transform.position.x - tCenGuard.position.x) >= 0.1f)
                            {
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            }
                            else aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else
                        {
                            aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                        }
                    }
                    //if it's not in the four foot and there's more than one rock in the house
                    else if (gm.houseList.Count > 1)
                    {
                        //if the second shot is mine
                        if (gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                        {

                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else
                        {
                            aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                        }
                    }
                    else
                    {
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }
                }
                break;
            #endregion

            #region Late No Hammer
            case "late":

                #region No Rocks in House
                if (gm.houseList.Count == 0)
                {
                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                }
                #endregion
                #region I have Shot Rock
                                else if (closestRockInfo.teamName == rockInfo.teamName)
                                {
                                    //if it's in the four foot
                                    if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6f)) <= 0.5f)
                                    {
                                        //tight centre and no centre guard
                                        if (tCenGuard && !cenGuard)
                                        {
                                            //if the tight centre guard is not covering it
                                            if (Mathf.Abs(closestRock.transform.position.x - tCenGuard.position.x) >= 0.1f)
                                            {
                                                aiShoot.OnShot("Centre Guard", rockCurrent);
                                            }
                                            //if the tight centre guard is covering it
                                            else
                                            {
                                                //if second shot is mine
                                                if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                                                {
                                                    //if third shot exists and is not mine
                                                    if (gm.houseList.Count > 2 && gm.houseList[2].rockInfo.teamName != rockInfo.teamName)
                                                    {
                                                        if (gm.houseList[2].rock.transform.position.x < 0)
                                                        {
                                                            if (lCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                            else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[2].rockInfo.rockIndex);
                                                        }
                                                        else if (gm.houseList[2].rock.transform.position.x > 0)
                                                        {
                                                            if (rCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                            else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[2].rockInfo.rockIndex);
                                                        }
                                                    }
                                                    else if (gm.houseList[1].rock.transform.position.x < 0)
                                                    {
                                                        if (lCornGuard) aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                                        else aiShoot.OnShot("Left Corner Guard", rockCurrent);
                                                    }
                                                    else if (gm.houseList[1].rock.transform.position.x > 0)
                                                    {
                                                        if (rCornGuard) aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                                        else aiShoot.OnShot("Right Corner Guard", rockCurrent);
                                                    }
                                                }
                                                //if second shot is not mine
                                                else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                                                {
                                                    if (gm.houseList[1].rock.transform.position.x < 0)
                                                    {
                                                        if (lCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                        else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                                    }
                                                    else if (gm.houseList[1].rock.transform.position.x > 0)
                                                    {
                                                        if (rCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                        else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                                    }
                                                    else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                                }
                                                //no second shot rock
                                                else
                                                {
                                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                                }
                                            }

                                        }
                                        //centre and no tight centre guard
                                        else if (cenGuard && !tCenGuard)
                                        {
                                            //if the centre guard is not covering it
                                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f)
                                            {
                                                aiShoot.OnShot("Tight Centre Guard", rockCurrent);
                                            }
                                            //if the centre guard is covering shot rock
                                            else
                                            {
                                                //if second shot exists and is mine
                                                if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                                                {
                                                    //if third shot exists and is not mine
                                                    if (gm.houseList.Count > 2 && gm.houseList[2].rockInfo.teamName != rockInfo.teamName)
                                                    {
                                                        if (gm.houseList[2].rock.transform.position.x < 0)
                                                        {
                                                            if (lCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                            else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[2].rockInfo.rockIndex);
                                                        }
                                                        else if (gm.houseList[2].rock.transform.position.x > 0)
                                                        {
                                                            if (rCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                            else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[2].rockInfo.rockIndex);
                                                        }
                                                    }
                                                    //if second shot is to the left
                                                    else if (gm.houseList[1].rock.transform.position.x < 0)
                                                    {
                                                        if (lCornGuard) aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                                        else aiShoot.OnShot("Left Corner Guard", rockCurrent);
                                                    }
                                                    //if second shot is to the right
                                                    else if (gm.houseList[1].rock.transform.position.x > 0)
                                                    {
                                                        if (rCornGuard) aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                                        else aiShoot.OnShot("Right Corner Guard", rockCurrent);
                                                    }
                                                }
                                                //if second shot is not mine
                                                else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                                                {
                                                    if (gm.houseList[1].rock.transform.position.x < 0)
                                                    {
                                                        if (lCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                        else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                                    }
                                                    else if (gm.houseList[1].rock.transform.position.x > 0)
                                                    {
                                                        if (rCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                                        else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                                    }
                                                    else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                                }
                                                //no second shot rock
                                                else
                                                {
                                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                                }
                                            }
                                        }
                                        //centre and tight centre guards
                                        else if (cenGuard && tCenGuard)
                                        {
                                            //if the centre and tight centre guards are not covering it
                                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f & Mathf.Abs(closestRock.transform.position.x - tCenGuard.position.x) >= 0.1f)
                                            {
                                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                            }
                                            else aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                        }
                                        else
                                        {
                                            //if second shot is mine
                                            if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                                            {
                                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                            }
                                            //if second shot is not mine
                                            else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                                            {
                                                aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                            }
                                        }
                                    }
                                    //if it's not in the four foot
                                    else
                                    {
                                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                    }
                                }
                #endregion
                #region They have Shot Rock
                else if (closestRockInfo.teamName != rockInfo.teamName)
                {
                    //if it's in the four foot
                    if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.5f)
                    {
                        //tight centre and no centre guard
                        if (tCenGuard && !cenGuard)
                        {
                            //if the tight centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - tCenGuard.position.x) >= 0.1f)
                            {
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            }
                            //if the tight centre guard is covering it
                            else
                            {
                                //if second shot is mine
                                if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                                {
                                    aiTarg.OnTarget("Take Out", rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);

                                    //if third shot exists and is not mine
                                    if (gm.houseList.Count > 2 && gm.houseList[2].rockInfo.teamName != rockInfo.teamName)
                                    {
                                        if (gm.houseList[2].rock.transform.position.x < 0)
                                        {
                                            if (lCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                            else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                        }
                                        else if (gm.houseList[2].rock.transform.position.x > 0)
                                        {
                                            if (rCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                            else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                        }
                                    }
                                    else if (gm.houseList[1].rock.transform.position.x < 0)
                                    {
                                        if (lCornGuard) aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                        else aiShoot.OnShot("Left Corner Guard", rockCurrent);
                                    }
                                    else if (gm.houseList[1].rock.transform.position.x > 0)
                                    {
                                        if (rCornGuard) aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                        else aiShoot.OnShot("Right Corner Guard", rockCurrent);
                                    }
                                }
                                //if second shot is not mine
                                else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                                {
                                    if (gm.houseList[1].rock.transform.position.x < 0)
                                    {
                                        if (lCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                        else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                    }
                                    else if (gm.houseList[1].rock.transform.position.x > 0)
                                    {
                                        if (rCornGuard) aiTarg.OnTarget("Take Out", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                        else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                    }
                                    else aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                }
                                //no second shot rock
                                else
                                {
                                    aiTarg.OnTarget("Take Out", rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                }
                            }

                        }
                        //centre and no tight centre guard
                        else if (cenGuard && !tCenGuard)
                        {
                            //if the centre guard is not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f)
                            {
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            }
                            else
                            {
                                //if second shot is mine
                                if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                                {
                                    aiTarg.OnTarget("Take Out", rockCurrent, cenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                }
                                //if second shot is not mine
                                else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                                {
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                }
                            }
                        }
                        //centre and tight centre guards
                        else if (cenGuard && tCenGuard)
                        {
                            //if the centre and tight centre guards are not covering it
                            if (Mathf.Abs(closestRock.transform.position.x - cenGuard.position.x) >= 0.1f & Mathf.Abs(closestRock.transform.position.x - tCenGuard.position.x) >= 0.1f)
                            {
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                            }
                            else aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else
                        {
                            //if second shot is mine
                            if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                            {
                                if (Mathf.Abs(closestRock.transform.position.x - gm.houseList[1].rock.transform.position.x) >= 0.2f)
                                    aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                else if (closestRock.transform.position.x > gm.houseList[1].rockInfo.rockIndex)
                                    aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                else
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            }
                            //if second shot is not mine
                            else if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                            {
                                if (Mathf.Abs(closestRock.transform.position.x - gm.houseList[1].rock.transform.position.x) >= 0.2f)
                                    aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                else if (closestRock.transform.position.x > gm.houseList[1].rockInfo.rockIndex)
                                    aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                else
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            }
                        }
                    }
                    //if it's not in the four foot and there's more than one rock in the house
                    else if (gm.houseList.Count > 1)
                    {
                        //if the second shot is mine
                        if (gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                        {

                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                        }
                        else
                        {
                            aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                        }
                    }
                    else
                    {
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }
                }
                #endregion
                //default
                else
                {
                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    Debug.Log("Default Late Phase");
                }
                break;
#endregion

            default:
                break;
        }
    }

    public void ConservativeStealOrBlank(int rockCurrent, string phase)
    {
        {
            GameObject rock = gm.rockList[rockCurrent].rock;
            Rock_Info rockInfo = gm.rockList[rockCurrent].rockInfo;

            if (gm.houseList.Count != 0)
            {
                closestRock = gm.houseList[0].rock;
                closestRockInfo = gm.houseList[0].rockInfo;
            }

            Debug.Log("Steal or Force - " + phase);

            aiTarg.OnTarget("Guard Reading", rockCurrent, 0);
            
            // ? NEW ARCHITECTURE: Try intent-based shot selection FIRST!
            if (TryIntentBasedShot_StealOrBlank(rockCurrent, phase))
            {
                Debug.Log("[StealOrBlank] ? Intent-based shot selected!");
                return;
            }
            
            Debug.Log("[StealOrBlank] ? Intent-based failed, using legacy logic...");

            switch (phase)
            {
            #region Early Not Hammer
                case "early":

                    if (gm.houseList.Count != 0)
                    {
                        if (closestRockInfo.teamName == rockInfo.teamName)
                            aiShoot.OnShot("Centre Guard", rockCurrent);
                        else
                            aiTarg.OnTarget("Tap Back", rockCurrent, closestRockInfo.rockIndex);
                    }
                    else
                    {
                        Debug.Log("Am I Making it here??");
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }

                    break;
#endregion

            #region Middle Not Hammer
                case "middle":

                    if (gm.houseList.Count != 0)
                    {
                        Debug.Log("Steal or Blank rocks in house");
                        if (closestRockInfo.teamName != rockInfo.teamName)
                        {
                            aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                        }
                        else if (closestRockInfo.teamName == rockInfo.teamName)
                        {
                            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            //if (Mathf.Abs(closestRock.transform.position.x) <= 0.5f)
                            //{
                            //    if (cenGuard)
                            //        aiShoot.OnShot("High Centre Guard", rockCurrent);
                            //    else
                            //        aiShoot.OnShot("Centre Guard", rockCurrent);
                            //}
                            //else if (closestRock.transform.position.x > 0f)
                            //    aiShoot.OnShot("Left Four Foot", rockCurrent);
                            //else
                            //    aiShoot.OnShot("Right Four Foot", rockCurrent);
                        }
                        else aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }
                    else
                    {
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    }

                    break;
#endregion

            #region Late Not Hammer
                case "late":
                    //there's rocks in the house
                    if (gm.houseList.Count != 0)
                    {
                        //They have shot rock
                        if (closestRockInfo.teamName != rockInfo.teamName)
                        {
                            //They have second shot too
                            if (gm.houseList.Count > 1 && gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
                            {
                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            }
                            //there's a centre guard
                            else if (cenGuard)
                            {
                                if (Mathf.Abs(cenGuard.position.x - closestRock.transform.position.x) > 0.1f)
                                    aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                                else
                                    aiTarg.OnTarget("Peel", rockCurrent, cenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                            }
                            //there's a tight centre guard
                            else if (tCenGuard)
                            {
                                if (Mathf.Abs(tCenGuard.position.x - closestRock.transform.position.x) > 0.1f)
                                    aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);
                                else
                                    aiTarg.OnTarget("Peel", rockCurrent, tCenGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                            }
                            else
                                aiTarg.OnTarget("Take Out", rockCurrent, closestRockInfo.rockIndex);

                        }
                        //I have shot rock
                        else if (closestRockInfo.teamName == rockInfo.teamName)
                        {
                            //more than one rock in house
                            if (gm.houseList.Count > 1)
                            {
                                //I have second shot
                                if (gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                                {
                                    //second shot is to the left
                                    if (gm.houseList[1].rock.transform.position.x <= 0f)
                                        aiShoot.OnShot("Left Corner Guard", rockCurrent);
                                    else
                                        aiShoot.OnShot("Right Corner Guard", rockCurrent);
                                }
                                else
                                {
                                    //second shot is in the middle
                                    if (Mathf.Abs(gm.houseList[1].rock.transform.position.x) <= 0.5f)
                                    {
                                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                    }
                                    //second shot is not in the middle
                                    else
                                    {
                                        //second shot is to the left
                                        if (gm.houseList[1].rock.transform.position.x < 0)
                                        {
                                            if (lCornGuard)
                                                aiTarg.OnTarget("Peel", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                            else
                                                aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                        }
                                        //second shot is to the right
                                        else
                                        {
                                            if (rCornGuard)
                                                aiTarg.OnTarget("Peel", rockCurrent, rCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
                                            else if (gm.houseList.Count > 1)
                                                aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
                                            else
                                                aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                        }
                                    }
                                        
                                }
                            }
                            //only one rock in house
                            else
                            {
                                if (Mathf.Abs(closestRock.transform.position.x) <= 0.5f)
                                {
                                    if (cenGuard || tCenGuard)
                                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                                    else
                                        aiShoot.OnShot("Centre Guard", rockCurrent);
                                }
                                else
                                    aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                            }
                        }
                    }
                    else
                        aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);

                    break;
#endregion

                default:
                    break;
            }
        }
    }
}
