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
    
    private EVEvaluationSystem evSystem;

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
    /// Helper: Check if a finesse is blocking a target rock
    /// </summary>
    private bool IsGuardBlocking(Transform guard, GameObject targetRock, float tolerance = 0.1f)
    {
        if (guard == null || targetRock == null) return false;
        return Mathf.Abs(guard.position.x - targetRock.transform.position.x) <= tolerance;
    }
    
    /// <summary>
    /// Helper: Get the rock index for a transform (finesse or house rock)
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
    
    [Header("Late-Game Tuning")]
    [Tooltip("Distance threshold for 'urgent threat' requiring removal (meters)")]
    [Range(0.3f, 1.0f)]
    public float urgentThreatDistance = 0.75f;
    
    [Tooltip("Distance threshold for 'close threat' (meters)")]
    [Range(0.5f, 1.2f)]
    public float closeThreatDistance = 1f;
    
    [Tooltip("When behind by this much, enter desperation mode")]
    [Range(1, 5)]
    public int desperationScoreGap = 2;
    
    [Header("EV System (Experimental)")]
    [Tooltip("Enable EV-based shot optimization")]
    public bool useEVOptimization = false;
    
    [Tooltip("EV weight (0=intent only, 1=EV only)")]
    [Range(0f, 1f)]
    public float evWeight = 0.3f;
    
    [Tooltip("Show detailed EV logs")]
    public bool evVerboseLogging = false;

    private void Update()
    {
        cenGuard = aiTarg.cenGuard;
        tCenGuard = aiTarg.tCenGuard;
        lCornGuard = aiTarg.lCornGuard;
        rCornGuard = aiTarg.rCornGuard;
    }
    
    void Start()
    {
        // Initialize EV system
        GameObject evObj = new GameObject("EVSystem");
        evObj.transform.SetParent(transform);
        evSystem = evObj.AddComponent<EVEvaluationSystem>();
        evSystem.useEVEvaluation = useEVOptimization;
        evSystem.evWeight = evWeight;
        evSystem.verboseLogging = evVerboseLogging;
        
        Debug.Log($"[AI_Strategy] EV System initialized (Enabled: {useEVOptimization}, Weight: {evWeight:F2})");
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

        // If no high-value takeout, play a finesse if few guards, else weight to button
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
    
    /// <summary>
    /// Count how many rocks a team has in the house (any scoring rocks)
    /// </summary>
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
    
    /// <summary>
    /// Build game state snapshot for EV evaluation
    /// </summary>
    private AIGameState BuildGameState(int rockCurrent)
    {
        return new AIGameState
        {
            rockCurrent = rockCurrent,
            endCurrent = gm.endCurrent,
            endTotal = gm.endTotal,
            activeTeamScore = activeTeamScore,
            oppTeamScore = oppTeamScore,
            activeTeamName = activeTeamName,
            oppTeamName = oppTeamName,
            hasHammer = (rockCurrent % 2 != 0),
            myRocksInHouse = CountRocksInHouse(activeTeamName),
            oppRocksInHouse = CountRocksInHouse(oppTeamName),
            phase = phase,
            guardsInPlay = gm.gList.Count,
            hasGuardBlocking = false
        };
    }
    
    /// <summary>
    /// Get character stats for current shooter
    /// </summary>
    private CharacterStats GetShooterStats(int rockCurrent)
    {
        TeamManager tm = FindObjectOfType<TeamManager>();
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
                context.acceptRisk = false;
                
                // EV EVALUATION (optional - only if enabled!)
                if (evSystem != null && useEVOptimization)
                {
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                }
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Guards in play, weight behind them
                context = new ShotContext(ShotIntent.CreateOpportunity);
                
                // EV EVALUATION (optional)
                if (evSystem != null && useEVOptimization)
                {
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                }
                
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
                context.acceptRisk = (myRocksInHouse > 0);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (myRocksInHouse > 1)
            {
                // We're in good shape - protect lead
                context = new ShotContext(ShotIntent.ProtectLead);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Keep building
                context = new ShotContext(ShotIntent.ScorePoints);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        // LATE PHASE: WITHOUT HAMMER - Steal or limit damage
        else if (phase == "late")
        {
            int oppRocksInHouse = gm.houseList.Count - myRocksInHouse;
            
            // SCENARIO 1: We have NO rocks, opponent has rock(s)
            if (myRocksInHouse == 0 && threatRock >= 0)
            {
                // Must remove threat to have ANY chance at stealing
                context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                context.acceptRisk = true;
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            
            // SCENARIO 2: We're WINNING the house
            else if (myRocksInHouse > 0 && myRocksInHouse > oppRocksInHouse)
            {
                // We're stealing! Protect what we have
                if (threatRock >= 0)
                {
                    // Threat exists - evaluate distance
                    float threatDist = Vector2.Distance(
                        gm.rockList[threatRock].rock.transform.position,
                        new Vector2(0f, 6.5f)
                    );
                    
                    if (threatDist < urgentThreatDistance)
                    {
                        // Must remove it
                        context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                        context.acceptRisk = true;
                        
                        // EV EVALUATION
                        if (evSystem != null && useEVOptimization)
                            context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                        
                        aiTarg.ExecuteIntent(context, rockCurrent);
                        return true;
                    }
                    else
                    {
                        // Threat is far - finesse what we have
                        context = new ShotContext(ShotIntent.ProtectLead);
                        
                        // EV EVALUATION
                        if (evSystem != null && useEVOptimization)
                            context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                        
                        aiTarg.ExecuteIntent(context, rockCurrent);
                        return true;
                    }
                }
                else
                {
                    // No threats - protect lead with finesse
                    context = new ShotContext(ShotIntent.ProtectLead);
                    
                    // EV EVALUATION
                    if (evSystem != null && useEVOptimization)
                        context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                    
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
            }
            
            // SCENARIO 3: We're TIED or LOSING the house
            else if (myRocksInHouse > 0 && threatRock >= 0)
            {
                // Evaluate: Can we steal by removing threat?
                float threatDist = Vector2.Distance(
                    gm.rockList[threatRock].rock.transform.position,
                    new Vector2(0f, 6.5f)
                );
                
                if (threatDist < closeThreatDistance)
                {
                    // Remove threat (might steal)
                    context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                    context.acceptRisk = true;
                    
                    // EV EVALUATION
                    if (evSystem != null && useEVOptimization)
                        context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                    
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
                else
                {
                    // Draw another rock (try to outscore them)
                    context = new ShotContext(ShotIntent.ScorePoints);
                    
                    // EV EVALUATION
                    if (evSystem != null && useEVOptimization)
                        context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                    
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
            }
            
            // SCENARIO 4: Clean house - easy steal attempt
            else
            {
                // No one has rocks - weight to button
                context = new ShotContext(ShotIntent.ScorePoints);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        // Unknown phase - use fallback
        return false;
    }
    
    /// <summary>
    /// ✨ NEW: Last Shot Scoring - FINAL shot MUST score (no penalties!)
    /// Called when rock 15 (last rock) is being played
    /// </summary>
    private bool TryIntentBasedShot_LastShotScoring(int rockCurrent)
    {
        Debug.Log($"[LastShotScoring] 🎯 FINAL SHOT - Evaluating scoring options (rock #{rockCurrent}/16)");
        
        Rock_Info currentRockInfo = gm.rockList[rockCurrent].rockInfo;
        Vector2 button = new Vector2(0f, 6.5f);
        
        // Count rocks in house
        int myRocksInHouse = CountRocksInHouse(currentRockInfo.teamName);
        int oppRocksInHouse = 0;
        float closestOpponentDistToButton = 999f;
        
        foreach (var houseRock in gm.houseList)
        {
            if (houseRock.rockInfo.teamName != currentRockInfo.teamName)
            {
                oppRocksInHouse++;
                float dist = Vector2.Distance(houseRock.rock.transform.position, button);
                if (dist < closestOpponentDistToButton)
                {
                    closestOpponentDistToButton = dist;
                }
            }
        }
        
        Debug.Log($"[LastShotScoring] My rocks: {myRocksInHouse}, Opponent rocks: {oppRocksInHouse}, Closest opp dist: {closestOpponentDistToButton:F2}");
        
        ShotContext context;
        
        // CRITICAL DECISION: Do we need to REMOVE or SCORE?
        
        // SCENARIO 1: Opponent has shot rock (closest to button) - MUST REMOVE!
        bool opponentHasShotRock = false;
        int shotRockIndex = -1;
        
        if (gm.houseList.Count > 0)
        {
            GameObject shotRock = gm.houseList[0].rock; // First in sorted list = closest to button
            Rock_Info shotRockInfo = gm.houseList[0].rockInfo;
            
            if (shotRockInfo.teamName != currentRockInfo.teamName)
            {
                opponentHasShotRock = true;
                shotRockIndex = shotRockInfo.rockIndex;
                
                Debug.Log($"[LastShotScoring] ⚠️ OPPONENT HAS SHOT ROCK (rock #{shotRockIndex} at {shotRock.transform.position})");
            }
        }
        
        if (opponentHasShotRock && shotRockIndex >= 0  && gm.houseList[0].rockInfo.distance < 1.0f)
        {
            // MUST REMOVE SHOT ROCK (or we lose!)
            Debug.Log($"[LastShotScoring] 🎯 CRITICAL: Must remove shot rock #{shotRockIndex} to win/tie!");
            
            context = new ShotContext(ShotIntent.RemoveThreat, shotRockIndex);
            context.acceptRisk = true; // Aggressive - MUST hit it!
            context.mustScore = false; // Don't try to score after removal - just get shot rock!
            
            aiTarg.ExecuteIntent(context, rockCurrent);
            return true;
        }
        
        // SCENARIO 2: We have shot rock OR clean house - SCORE!
        Debug.Log($"[LastShotScoring] ✅ We have shot rock OR clean house - AGGRESSIVE SCORING!");
        
        // Use NEW LastShotScoring intent (no removal penalties!)
        context = new ShotContext(ShotIntent.LastShotScoring);
        context.idealFinalPosition = button; // Default to button
        context.aggressiveness = 1.0f; // Maximum aggression
        context.acceptRisk = false; // Don't risk missing - just score!
        context.mustScore = true; // CRITICAL: Must land in house!
        
        aiTarg.ExecuteIntent(context, rockCurrent);
        return true;
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
            if (rockCurrent < 2)
            {
                // First 2 rocks - aggressive corner guards
                context = new ShotContext(ShotIntent.CreateOpportunity);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }

            if (threatRock >= 0)
            {
                // Remove any opposition rock immediately
                context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                context.acceptRisk = true;
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Build guards for corner game
                context = new ShotContext(ShotIntent.CreateOpportunity);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
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
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (myRocksInHouse >= 1)
            {
                // Build on our position
                context = new ShotContext(ShotIntent.ScorePoints);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Keep setting up
                context = new ShotContext(ShotIntent.CreateOpportunity);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        // LATE: Go for the steal or setup
        else if (phase == "late")
        {
            // CRITICAL: Last rock with hammer - MUST score!
            bool isLastRock = (rockCurrent >= 15);
            
            if (isLastRock)
            {
                Debug.Log("[AggressiveHammer] HAMMER LAST ROCK - SMART low-risk strategy!");
                
                // PHILOSOPHY: Accept force of 1 rather than risk blank on failed takeout!
                // ONLY attempt risky takeout if:
                // 1. Clear shot (no guards blocking)
                // 2. Multiple opponent rocks (2+) in scoring
                // 3. We have 2+ rocks that will remain after takeout
                
                // Check for guard interference
                bool hasGuardInterference = false;
                foreach (var rockEntry in gm.rockList)
                {
                    if (rockEntry.rock == null || !rockEntry.rock.activeInHierarchy) continue;
                    Vector2 rockPos = rockEntry.rock.transform.position;
                    if (rockPos.y > 0f && rockPos.y < 3.5f)  // Guard zone
                    {
                        hasGuardInterference = true;
                        break;
                    }
                }
                
                // DECISION: Risky takeout vs safe draw?
                bool shouldAttemptTakeout = false;
                
                if (oppRocksInHouse >= 2 && !hasGuardInterference && myRocksInHouse >= 2)
                {
                    // ONLY attempt if VERY SAFE: No guards, we have 2+ backup rocks
                    shouldAttemptTakeout = true;
                    Debug.Log($"[HAMMER LAST ROCK] High-confidence takeout (clear shot, our rocks: {myRocksInHouse}, opponent: {oppRocksInHouse})");
                }
                else
                {
                    Debug.Log($"[HAMMER LAST ROCK] RISKY takeout avoided (guards: {hasGuardInterference}, our rocks: {myRocksInHouse}, opp: {oppRocksInHouse}) - DRAWING TO BUTTON!");
                }
                
                if (shouldAttemptTakeout && threatRock >= 0)
                {
                    // Attempt high-confidence takeout
                    context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                    context.acceptRisk = true;
                    context.mustScore = true;
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
                
                // DEFAULT: SAFE DRAW TO BUTTON
                // Accept force of 1 - MUCH better than blank end!
                Debug.Log($"[HAMMER LAST ROCK] Drawing to button - accept force of 1, avoid blank!");
                context = new ShotContext(ShotIntent.ScorePoints);
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            
            // Not last rock - general late-game logic
            if (threatRock >= 0 && activeTeamScore < oppTeamScore)
            {
                // Behind in game score - desperate removal
                context = new ShotContext(ShotIntent.Desperation, threatRock);
                context.acceptRisk = true;
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (threatRock >= 0 && oppRocksInHouse >= 2)
            {
                // They're building points - evaluate removal vs scoring
                if (myRocksInHouse > oppRocksInHouse)
                {
                    // We're winning house - keep scoring
                    context = new ShotContext(ShotIntent.ScorePoints);
                    
                    // EV EVALUATION
                    if (evSystem != null && useEVOptimization)
                        context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                    
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
                else
                {
                    // They're winning house - must remove
                    context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                    context.acceptRisk = true;
                    
                    // EV EVALUATION
                    if (evSystem != null && useEVOptimization)
                        context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                    
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
            }
            else if (myRocksInHouse == 0 && threatRock < 0)
            {
                // Clean house - weight for steal
                context = new ShotContext(ShotIntent.ScorePoints);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (threatRock >= 0)
            {
                // Single threat rock - evaluate distance and position
                float threatDist = Vector2.Distance(
                    gm.rockList[threatRock].rock.transform.position,
                    new Vector2(0f, 6.5f)
                );
                
                // If threat is close to button and winning, remove it
                if (threatDist < urgentThreatDistance)
                {
                    context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                    context.acceptRisk = true;
                    
                    // EV EVALUATION
                    if (evSystem != null && useEVOptimization)
                        context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                    
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
                else
                {
                    // Threat is far or we're ahead - keep scoring
                    context = new ShotContext(ShotIntent.ScorePoints);
                    
                    // EV EVALUATION
                    if (evSystem != null && useEVOptimization)
                        context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                    
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
            }
            else
            {
                // No threats - just score!
                context = new ShotContext(ShotIntent.ScorePoints);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
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
                context.acceptRisk = false;
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Draw to corners for 2-point setup
                context = new ShotContext(ShotIntent.ScorePoints);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
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
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (myRocksInHouse >= 2)
            {
                // We have 2+ rocks - protect them!
                context = new ShotContext(ShotIntent.ProtectLead);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Keep building
                context = new ShotContext(ShotIntent.ScorePoints);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        // LATE: WITH HAMMER - Score 2+ or blank to keep hammer
        else if (phase == "late")
        {
            bool isLastRock = (rockCurrent >= 15);
            int oppRocksInHouse = gm.houseList.Count - myRocksInHouse;
            
            // LAST ROCK LOGIC: Must score something!
            if (isLastRock)
            {
                Debug.Log("[ScoreTwoOrBlank] LAST ROCK - must score!");
                
                // Have 2+ rocks already? Add more!
                if (myRocksInHouse >= 1)
                {
                    if (threatRock < 0)
                    {
                        context = new ShotContext(ShotIntent.ScorePoints);
                        aiTarg.ExecuteIntent(context, rockCurrent);
                        return true;
                    }
                    else
                    {
                        if (gm.houseList[0].rockInfo.teamName != activeTeamName || gm.houseList[1].rockInfo.teamName != activeTeamName)
                        {
                            context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                            context.acceptRisk = true;
                            context.mustScore = true; // MUST score after removal
                            aiTarg.ExecuteIntent(context, rockCurrent);
                            return true;
                        }
                        else
                        {
                            // We have 2+ rocks - add more to secure 2 points
                            context = new ShotContext(ShotIntent.ScorePoints);
                            aiTarg.ExecuteIntent(context, rockCurrent);
                            return true;
                        }
                    }
                }
                // Threats exist? Remove and try to score
                else if (threatRock >= 0)
                {
                    context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                    context.acceptRisk = true;
                    context.mustScore = true; // MUST score after removal
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
                // Nothing good? Blank to keep hammer next end
                else
                {
                    context = new ShotContext(ShotIntent.ForceBlank);
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
            }
            
            // NOT LAST ROCK: Build for 2-point end or blank
            
            // Already have 1+? Add more or remove threats!
            if (myRocksInHouse >= 1)
            {
                if (threatRock < 0)
                {
                    // No threats - add more rocks!
                    context = new ShotContext(ShotIntent.ScorePoints);
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
                else
                {
                    // Threat exists - finesse our lead
                    context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                    context.acceptRisk = true;
                    context.mustScore = true; // MUST score after removal
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
            }
            // Have 0 rocks? Decide: blank or try for 2
            else
            {
                if (threatRock >= 0)
                {
                    // Remove threats (might blank)
                    context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                    context.acceptRisk = false;
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
                else if (oppRocksInHouse == 0)
                {
                    // Clean house - can still build for 2
                    context = new ShotContext(ShotIntent.ScorePoints);
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
                else
                {
                    // Can't score 2 - force blank
                    context = new ShotContext(ShotIntent.ForceBlank);
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
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

        // EARLY: Aggressive setup, ignore removal of early threats (they can be dealt with later)
        if (phase == "early")
        {
            // First rocks - aggressive centre guards to set up steal
            context = new ShotContext(ShotIntent.CreateOpportunity);
            
            // EV EVALUATION
            if (evSystem != null && useEVOptimization)
                context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
            
            aiTarg.ExecuteIntent(context, rockCurrent);
            return true;
        }
        
        // MIDDLE: Keep pressure, remove threats
        else if (phase == "middle")
        {
            if (threatRock >= 0)
            {
                if (myRocksInHouse > 0)
                {
                    // Build on position
                    context = new ShotContext(ShotIntent.CreateOpportunity);
                    
                    // EV EVALUATION
                    if (evSystem != null && useEVOptimization)
                        context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                    
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
                else
                {
                    // No rocks in house - try to steal by removing threat
                    context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                    context.acceptRisk = true;
                    
                    // EV EVALUATION
                    if (evSystem != null && useEVOptimization)
                        context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                    
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
            }
            else
            {
                // Keep setting up
                context = new ShotContext(ShotIntent.CreateOpportunity);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        // LATE: WITHOUT HAMMER - All-in for steal
        else if (phase == "late")
        {
            int oppRocksInHouse = gm.houseList.Count - myRocksInHouse;
            
            // SCENARIO 1: No rocks vs threat - desperate removal
            if (myRocksInHouse == 0 && threatRock >= 0)
            {
                // Must remove to have ANY chance
                context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                context.acceptRisk = true;
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            
            // SCENARIO 2: We have rocks AND threats exist
            else if (myRocksInHouse > 0 && threatRock >= 0)
            {
                // Find best rock from each team
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
                
                // They're winning - ATTACK!
                if (theirBestDist < myBestDist)
                {
                    // Behind in game score? DESPERATION!
                    if (activeTeamScore <= oppTeamScore)
                    {
                        context = new ShotContext(ShotIntent.Desperation, threatRock);
                        context.acceptRisk = true;
                        context.mustScore = true;
                        
                        // EV EVALUATION
                        if (evSystem != null && useEVOptimization)
                            context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                        
                        aiTarg.ExecuteIntent(context, rockCurrent);
                        return true;
                    }
                    else
                    {
                        // Just remove threat
                        context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                        context.acceptRisk = true;
                        
                        // EV EVALUATION
                        if (evSystem != null && useEVOptimization)
                            context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                        
                        aiTarg.ExecuteIntent(context, rockCurrent);
                        return true;
                    }
                }
                // We're winning house - BUILD LEAD!
                else
                {
                    // Threat close? Remove it first
                    float threatDist = Vector2.Distance(
                        gm.rockList[threatRock].rock.transform.position,
                        new Vector2(0f, 6.5f)
                    );
                    
                    if (threatDist < closeThreatDistance)
                    {
                        context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                        context.acceptRisk = true;
                        
                        // EV EVALUATION
                        if (evSystem != null && useEVOptimization)
                            context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                        
                        aiTarg.ExecuteIntent(context, rockCurrent);
                        return true;
                    }
                    else
                    {
                        // Add more rocks to steal multiple
                        context = new ShotContext(ShotIntent.ScorePoints);
                        
                        // EV EVALUATION
                        if (evSystem != null && useEVOptimization)
                            context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                        
                        aiTarg.ExecuteIntent(context, rockCurrent);
                        return true;
                    }
                }
            }
            
            // SCENARIO 3: We have rocks, NO threats - keep scoring!
            else if (myRocksInHouse > 0)
            {
                // We're stealing - add more!
                context = new ShotContext(ShotIntent.ScorePoints);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            
            // SCENARIO 4: Clean house - weight for steal
            else
            {
                context = new ShotContext(ShotIntent.ScorePoints);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
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
                context.acceptRisk = false;
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (myRocksInHouse > 0)
            {
                // We have rocks - finesse them
                context = new ShotContext(ShotIntent.ProtectLead);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Draw for steal attempt
                context = new ShotContext(ShotIntent.ScorePoints);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
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
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else if (myRocksInHouse > 0)
            {
                // Protect what we have
                context = new ShotContext(ShotIntent.ProtectLead);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
            else
            {
                // Draw cautiously
                context = new ShotContext(ShotIntent.ScorePoints);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
                aiTarg.ExecuteIntent(context, rockCurrent);
                return true;
            }
        }
        
        // LATE: WITHOUT HAMMER - Deny points or steal
        else if (phase == "late")
        {
            // Primary goal: Force them to 1 point OR steal OR blank
            
            // SCENARIO 1: They have 2+ rocks - DANGER!
            if (oppRocksInHouse >= 2)
            {
                if (threatRock >= 0)
                {
                    // Remove their best rock (try to reduce to 1 point)
                    context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                    context.acceptRisk = false;
                    
                    // EV EVALUATION
                    if (evSystem != null && useEVOptimization)
                        context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                    
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
                else
                {
                    // No clear removal - force blank
                    context = new ShotContext(ShotIntent.ForceBlank);
                    
                    // EV EVALUATION
                    if (evSystem != null && useEVOptimization)
                        context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                    
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
            }
            
            // SCENARIO 2: They have 1 rock only
            else if (oppRocksInHouse == 1)
            {
                if (myRocksInHouse > 0)
                {
                    // We're stealing! Protect what we have
                    context = new ShotContext(ShotIntent.ProtectLead);
                    
                    // EV EVALUATION
                    if (evSystem != null && useEVOptimization)
                        context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                    
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
                else if (threatRock >= 0)
                {
                    // Remove their single rock (steal or blank)
                    context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
                    context.acceptRisk = false;
                    
                    // EV EVALUATION
                    if (evSystem != null && useEVOptimization)
                        context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                    
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
                else
                {
                    // Try to steal
                    context = new ShotContext(ShotIntent.ScorePoints);
                    
                    // EV EVALUATION
                    if (evSystem != null && useEVOptimization)
                        context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                    
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
            }
            
            // SCENARIO 3: We're winning the house!
            else if (myRocksInHouse > 0)
            {
                // Stealing! Protect the steal
                if (threatRock >= 0)
                {
                    // Small threat exists - finesse instead of removing
                    context = new ShotContext(ShotIntent.ProtectLead);
                    
                    // EV EVALUATION
                    if (evSystem != null && useEVOptimization)
                        context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                    
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
                else
                {
                    // No threats - finesse the steal
                    context = new ShotContext(ShotIntent.ProtectLead);
                    
                    // EV EVALUATION
                    if (evSystem != null && useEVOptimization)
                        context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                    
                    aiTarg.ExecuteIntent(context, rockCurrent);
                    return true;
                }
            }
            
            // SCENARIO 4: Clean house - blank is acceptable
            else
            {
                // Blank is fine - keeps hammer for them but no damage
                context = new ShotContext(ShotIntent.ForceBlank);
                
                // EV EVALUATION
                if (evSystem != null && useEVOptimization)
                    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
                
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
                if (activeTeamScore < (oppTeamScore + 1))
                    AggressiveNotHammer(rockCurrent, phase);
                else if (activeTeamScore < oppTeamScore)
                    ConservativeSteal(rockCurrent, phase);
                else
                    ConservativeStealOrBlank(rockCurrent, phase);
            }
            //one end left
            else if (gm.endTotal - gm.endCurrent == 1)
            {
                if (activeTeamScore < oppTeamScore)
                    AggressiveNotHammer(rockCurrent, phase);
                else
                    ConservativeStealOrBlank(rockCurrent, phase);
            }
            else if (activeTeamScore < oppTeamScore)
                ConservativeStealOrBlank(rockCurrent, phase);
            else
                AggressiveNotHammer(rockCurrent, phase);
        }
        else
        {
            if (activeTeamScore < oppTeamScore)
                AggressiveHammer(rockCurrent, phase);
            else
                ConservativeScoreTwoOrBlankHammer(rockCurrent, phase);

        }
        Debug.Log("[AI_Strategy]Phase is " + phase);
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
        
        // ✨ NEW: Check if this is LAST SHOT (rock 15) - use dedicated last-shot logic!
        if (rockCurrent >= 15)
        {
            Debug.Log("[AggressiveHammer] 🎯 LAST SHOT DETECTED - Using LastShotScoring logic!");
            if (TryIntentBasedShot_LastShotScoring(rockCurrent))
            {
                Debug.Log("[AggressiveHammer] ✅ Last shot scoring executed!");
                return;
            }
        }
        
        // ? NEW ARCHITECTURE: Try intent-based shot selection FIRST!
        if (TryIntentBasedShot_AggressiveHammer(rockCurrent, phase))
        {
            Debug.Log("[AggressiveHammer] ? Intent-based shot selected!");
            return;
        }
        
        Debug.Log("[AggressiveHammer] ? Intent-based failed, using legacy logic...");

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
        
        // ✨ NEW: Check if this is LAST SHOT (rock 15) - use dedicated last-shot logic!
        if (rockCurrent >= 15)
        {
            Debug.Log("[ScoreTwoOrBlank] 🎯 LAST SHOT DETECTED - Using LastShotScoring logic!");
            if (TryIntentBasedShot_LastShotScoring(rockCurrent))
            {
                Debug.Log("[ScoreTwoOrBlank] ✅ Last shot scoring executed!");
                return;
            }
        }
        
        // ? NEW ARCHITECTURE: Try intent-based shot selection FIRST!
        if (TryIntentBasedShot_ScoreTwoOrBlank(rockCurrent, phase))
        {
            Debug.Log("[ScoreTwoOrBlank] ? Intent-based shot selected!");
            return;
        }
        
        Debug.Log("[ScoreTwoOrBlank] ? Intent-based failed, using legacy logic...");

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

        aiTarg.OnTarget("Guard Reading", rockCurrent, 0);
        
        // ✨ NEW: Check if this is LAST SHOT (rock 15) - use dedicated last-shot logic!
        if (rockCurrent >= 15)
        {
            Debug.Log("[AggressiveNotHammer] 🎯 LAST SHOT DETECTED - Using LastShotScoring logic!");
            if (TryIntentBasedShot_LastShotScoring(rockCurrent))
            {
                Debug.Log("[AggressiveNotHammer] ✅ Last shot scoring executed!");
                return;
            }
        }
        
        // ? NEW ARCHITECTURE: Try intent-based shot selection FIRST!
        if (TryIntentBasedShot_AggressiveNotHammer(rockCurrent, phase))
        {
            Debug.Log("[AggressiveNotHammer] ? Intent-based shot selected! -- phase is " + phase);
            return;
        }
        
        Debug.Log("[AggressiveNotHammer] ? Intent-based failed, using legacy logic...");

    }

    public void ConservativeStealOrBlank(int rockCurrent, string phase)
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
        
        // ✨ NEW: Check if this is LAST SHOT (rock 15) - use dedicated last-shot logic!
        if (rockCurrent >= 15)
        {
            Debug.Log("[StealOrBlank] 🎯 LAST SHOT DETECTED - Using LastShotScoring logic!");
            if (TryIntentBasedShot_LastShotScoring(rockCurrent))
            {
                Debug.Log("[StealOrBlank] ✅ Last shot scoring executed!");
                return;
            }
        }
            
        // ? NEW ARCHITECTURE: Try intent-based shot selection FIRST!
        if (TryIntentBasedShot_StealOrBlank(rockCurrent, phase))
        {
            Debug.Log("[StealOrBlank] ? Intent-based shot selected!");
            return;
        }
            
        Debug.Log("[StealOrBlank] ? Intent-based failed, using legacy logic...");

    }
}
