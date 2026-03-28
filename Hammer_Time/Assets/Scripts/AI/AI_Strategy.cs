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

    // ✅ PHASE 1: AI Enhancement Systems
    private AIEnhancementSystems enhancements;

    Rock_Info rockInfo;
    Rock_Flick rockFlick;
    Rigidbody2D rockRB;
    
    private EVEvaluationSystem evSystem;
    private MultiShotPlanner multiShotPlanner;
    
    // ✅ PHASE 2: Cached house analysis (calculated once per turn)
    private HouseAnalysis _cachedHouseAnalysis = null;
    
    /// <summary>
    /// House state analysis - cached for performance
    /// Calculated ONCE per turn, reused across all strategy decisions
    /// </summary>
    private class HouseAnalysis
    {
        public int myRocksInHouse;
        public int oppRocksInHouse;
        public float myBestDistance = 999f;
        public float oppBestDistance = 999f;
        public bool amWinningHouse;
        public int threatRock = -1;
    }

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
    
    [Header("Multi-Shot Planning (NEW!)")]
    [Tooltip("Enable strategic 2-3 shot planning")]
    public bool useMultiShotPlanning = true;
    
    [Tooltip("Show detailed planning logs")]
    public bool planningVerboseLogging = false;

    private void Update()
    {
        cenGuard = aiTarg.cenGuard;
        tCenGuard = aiTarg.tCenGuard;
        lCornGuard = aiTarg.lCornGuard;
        rCornGuard = aiTarg.rCornGuard;
    }
    
    void Start()
    {
        // ✅ PHASE 1: Initialize AI Enhancement Systems
        enhancements = new AIEnhancementSystems();
        
        Debug.Log("[AI_Strategy] Phase 1 Enhancement Systems initialized:");
        Debug.Log("  ✅ Skill-Based Shot Selection");
        Debug.Log("  ✅ Clutch Performance Modifiers");
        Debug.Log("  ✅ Simple Counter-Strategy");
        
        // Initialize EV system
        GameObject evObj = new GameObject("EVSystem");
        evObj.transform.SetParent(transform);
        evSystem = evObj.AddComponent<EVEvaluationSystem>();
        evSystem.useEVEvaluation = useEVOptimization;
        evSystem.evWeight = evWeight;
        evSystem.verboseLogging = evVerboseLogging;
        
        Debug.Log($"[AI_Strategy] EV System initialized (Enabled: {useEVOptimization}, Weight: {evWeight:F2})");
        
        // Initialize Multi-Shot Planner
        multiShotPlanner = new MultiShotPlanner(gm);
        multiShotPlanner.enableMultiShotPlanning = useMultiShotPlanning;
        multiShotPlanner.verboseLogging = planningVerboseLogging;
        
        Debug.Log($"[AI_Strategy] Multi-Shot Planner initialized (Enabled: {useMultiShotPlanning})");
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
    /// ✅ NEW: Should we remove threat instead of placing guard?
    /// CRITICAL: Threat removal should be prioritized over guard placement
    /// </summary>
    private bool ShouldRemoveThreat(HouseAnalysis house, string phase, bool hasHammer)
    {
        // NO THREATS - don't remove
        if (house.threatRock < 0) return false;
        
        // CRITICAL: Opponent has shot rock AND we're losing house
        if (!house.amWinningHouse && house.oppRocksInHouse >= 1)
        {
            return true; // ALWAYS remove when losing
        }
        
        // EARLY PHASE: Remove opponent rocks immediately (don't let them build)
        if (phase == "early")
        {
            return true; // Aggressive early - clear everything
        }
        
        // MIDDLE PHASE: Remove if they have 2+ rocks (multi-point threat)
        if (phase == "middle" && house.oppRocksInHouse >= 2)
        {
            return true; // Don't let them build big end
        }
        
        // LATE PHASE WITHOUT HAMMER: Remove to steal
        if (phase == "late" && !hasHammer && house.oppRocksInHouse >= 1)
        {
            return true; // Need to clear to steal
        }
        
        // LATE PHASE WITH HAMMER: Remove if multiple threats
        if (phase == "late" && hasHammer && house.oppRocksInHouse >= 2)
        {
            return true; // Clear for big end
        }
        
        // DEFAULT: Threat exists but not urgent
        return false;
    }
    
    /// <summary>
    /// ✅ NEW: Get shooter skills to determine shot style
    /// High finesse → fancy shots (freeze, tick, runback)
    /// High weight → power shots (takeout, heavy draw)
    /// </summary>
    private (float finesse, float weight, float aim) GetShooterSkillProfile(int rockCurrent)
    {
        CharacterStats shooter = GetShooterStats(rockCurrent);
        
        if (shooter == null)
            return (50f, 50f, 50f); // Default balanced
        
        return (
            shooter.finesseAccuracy.GetValue(),
            shooter.weightAccuracy.GetValue(),
            shooter.aimAccuracy.GetValue()
        );
    }
    
    /// <summary>
    /// ✅ NEW: Evaluate potential scoring benefit of removing a threat rock
    /// 
    /// Simulates: "If I remove this rock, would I be sitting shot rock? Multiple?"
    /// Returns: Score boost value (0 = no benefit, 1-5 = increasing benefit)
    /// </summary>
    private float EvaluateRemovalScoringBenefit(int threatRockIndex)
    {
        if (threatRockIndex < 0 || threatRockIndex >= gm.rockList.Count) return 0f;
        
        GameObject threatRock = gm.rockList[threatRockIndex].rock;
        if (threatRock == null) return 0f;
        
        Rock_Info threatInfo = gm.rockList[threatRockIndex].rockInfo;
        Vector2 button = new Vector2(0f, 6.5f);
        
        // Build list of rocks AFTER removal (simulate removing threat)
        var remainingRocks = new List<(GameObject rock, string team, float dist)>();
        
        foreach (var houseRock in gm.houseList)
        {
            // Skip the rock we're planning to remove
            if (houseRock.rockInfo.rockIndex == threatRockIndex) continue;
            
            float dist = Vector2.Distance(houseRock.rock.transform.position, button);
            remainingRocks.Add((houseRock.rock, houseRock.rockInfo.teamName, dist));
        }
        
        // Sort by distance (closest = shot rock)
        remainingRocks.Sort((a, b) => a.dist.CompareTo(b.dist));
        
        // Count how many MY rocks would be scoring
        int myRocksScoring = 0;
        int oppRocksScoring = 0;
        bool iHaveShotRock = false;
        
        for (int i = 0; i < remainingRocks.Count; i++)
        {
            var rockEntry = remainingRocks[i];
            bool isMine = (rockEntry.team == activeTeamName);
            
            if (i == 0 && isMine)
            {
                iHaveShotRock = true; // I would have shot rock!
            }
            
            if (isMine)
            {
                myRocksScoring++;
            }
            else
            {
                // Opponent rock - stops my scoring
                break;
            }
        }
        
        // Calculate score boost based on potential scoring
        float scoreBoost = 0f;
        
        if (iHaveShotRock)
        {
            // Base boost for getting shot rock
            scoreBoost += 2.0f;
            
            // Additional boost for multi-point potential
            if (myRocksScoring >= 2)
                scoreBoost += 1.5f; // Sitting 2!
            
            if (myRocksScoring >= 3)
                scoreBoost += 1.0f; // Sitting 3+!
            
            Debug.Log($"[RemovalBenefit] Removing rock #{threatRockIndex} → Would sit {myRocksScoring} (shot rock: {iHaveShotRock}) [Boost: +{scoreBoost:F1}]");
        }
        else if (myRocksScoring > oppRocksScoring)
        {
            // Would be winning house (but not shot rock)
            scoreBoost += 0.5f;
            Debug.Log($"[RemovalBenefit] Removing rock #{threatRockIndex} → Would win house {myRocksScoring}-{oppRocksScoring} [Boost: +{scoreBoost:F1}]");
        }
        else
        {
            // No scoring benefit
            Debug.Log($"[RemovalBenefit] Removing rock #{threatRockIndex} → No scoring benefit (opponent still ahead)");
        }
        
        return scoreBoost;
    }
    
    /// <summary>
    /// ✅ NEW: Should AI play defensively (more takeouts)?
    /// LOOSER CRITERIA than simple "leading" check
    /// 
    /// Philosophy: AI should be defensive when protecting a lead OR in close games
    /// </summary>
    private bool ShouldPlayDefensive(int rockCurrent, bool hasHammer, string phase)
    {
        int scoreDiff = activeTeamScore - oppTeamScore;
        int rocksRemaining = 16 - rockCurrent;
        bool isLastEnd = (gm.endTotal - gm.endCurrent == 1);
        
        // CRITERION 1: Clearly leading (2+ points ahead)
        if (scoreDiff >= 2)
        {
            Debug.Log($"[ShouldPlayDefensive] YES - Leading by {scoreDiff} points");
            return true;
        }
        
        // CRITERION 2: Leading by 1 point in last end
        if (scoreDiff >= 1 && isLastEnd)
        {
            Debug.Log($"[ShouldPlayDefensive] YES - Leading by {scoreDiff} in last end");
            return true;
        }
        
        // CRITERION 3: Tied game in late phase (protect position)
        if (scoreDiff == 0 && phase == "late")
        {
            Debug.Log($"[ShouldPlayDefensive] YES - Tied game in late phase (protect house)");
            return true;
        }
        
        // CRITERION 4: Leading without hammer in late phase (extra defensive)
        if (scoreDiff >= 1 && !hasHammer && phase == "late")
        {
            Debug.Log($"[ShouldPlayDefensive] YES - Leading by {scoreDiff} without hammer in late phase");
            return true;
        }
        
        // CRITERION 5: Last 3 rocks and ANY lead
        if (scoreDiff >= 1 && rocksRemaining <= 3)
        {
            Debug.Log($"[ShouldPlayDefensive] YES - Leading by {scoreDiff} with only {rocksRemaining} rocks left");
            return true;
        }
        
        // DEFAULT: Offensive (trailing or tied early/middle)
        Debug.Log($"[ShouldPlayDefensive] NO - Offensive mode (score diff: {scoreDiff}, phase: {phase})");
        return false;
    }
    
    /// <summary>
    /// ✅ NEW: Should we prioritize removing this threat rock?
    /// Enhanced with POST-REMOVAL SCORING EVALUATION
    /// 
    /// Philosophy: Remove rocks that:
    ///   1. Threaten our lead (defensive)
    ///   2. Give us scoring opportunities (offensive benefit from removal)
    /// </summary>
    private bool ShouldRemoveThreatEnhanced(HouseAnalysis house, int rockCurrent, bool hasHammer, string phase)
    {
        // NO THREATS - don't remove
        if (house.threatRock < 0) return false;
        
        // ✅ NEW: Evaluate post-removal scoring benefit
        float removalBenefit = EvaluateRemovalScoringBenefit(house.threatRock);
        
        // HIGH SCORING BENEFIT: Always worth removing (even if not strictly defensive)
        if (removalBenefit >= 2.0f)
        {
            Debug.Log($"[ShouldRemoveThreatEnhanced] YES - High scoring benefit ({removalBenefit:F1}) from removing threat!");
            return true;
        }
        
        // MEDIUM SCORING BENEFIT: Worth removing in middle/late phases
        if (removalBenefit >= 1.0f && (phase == "middle" || phase == "late"))
        {
            Debug.Log($"[ShouldRemoveThreatEnhanced] YES - Medium scoring benefit ({removalBenefit:F1}) in {phase} phase");
            return true;
        }
        
        // Continue with original threat removal logic (defensive criteria)
        
        // CRITICAL: Opponent has shot rock AND we're losing house
        if (!house.amWinningHouse && house.oppRocksInHouse >= 1)
        {
            Debug.Log($"[ShouldRemoveThreatEnhanced] YES - Losing house (defensive priority)");
            return true; // ALWAYS remove when losing
        }
        
        // EARLY PHASE: Remove opponent rocks immediately (don't let them build)
        if (phase == "early")
        {
            Debug.Log($"[ShouldRemoveThreatEnhanced] YES - Early phase (clear everything)");
            return true; // Aggressive early - clear everything
        }
        
        // MIDDLE PHASE: Remove if they have 2+ rocks (multi-point threat)
        if (phase == "middle" && house.oppRocksInHouse >= 2)
        {
            Debug.Log($"[ShouldRemoveThreatEnhanced] YES - Middle phase, opponent has {house.oppRocksInHouse} rocks");
            return true; // Don't let them build big end
        }
        
        // LATE PHASE WITHOUT HAMMER: Remove to steal
        if (phase == "late" && !hasHammer && house.oppRocksInHouse >= 1)
        {
            Debug.Log($"[ShouldRemoveThreatEnhanced] YES - Late phase without hammer (need to clear)");
            return true; // Need to clear to steal
        }
        
        // LATE PHASE WITH HAMMER: Remove if multiple threats
        if (phase == "late" && hasHammer && house.oppRocksInHouse >= 2)
        {
            Debug.Log($"[ShouldRemoveThreatEnhanced] YES - Late phase with hammer, {house.oppRocksInHouse} opponent rocks");
            return true; // Clear for big end
        }
        
        // DEFAULT: Threat exists but not urgent
        Debug.Log($"[ShouldRemoveThreatEnhanced] NO - Threat exists but no strong removal criteria");
        return false;
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
    /// ✅ PHASE 2: Get cached house analysis (calculated once per turn)
    /// This replaces 5+ duplicate calculations of rock counts and distances
    /// </summary>
    private HouseAnalysis GetHouseAnalysis()
    {
        // Return cached if already calculated this turn
        if (_cachedHouseAnalysis != null) return _cachedHouseAnalysis;
        
        var analysis = new HouseAnalysis
        {
            threatRock = FindBiggestThreat(activeTeamName)
        };
        
        Vector2 button = new Vector2(0f, 6.5f);
        
        foreach (var rock in gm.houseList)
        {
            bool isMine = (rock.rockInfo.teamName == activeTeamName);
            float dist = Vector2.Distance(rock.rock.transform.position, button);
            
            if (isMine)
            {
                analysis.myRocksInHouse++;
                if (dist < analysis.myBestDistance)
                    analysis.myBestDistance = dist;
            }
            else
            {
                analysis.oppRocksInHouse++;
                if (dist < analysis.oppBestDistance)
                    analysis.oppBestDistance = dist;
            }
        }
        
        analysis.amWinningHouse = (analysis.myBestDistance < analysis.oppBestDistance);
        
        _cachedHouseAnalysis = analysis;
        return analysis;
    }
    
    /// <summary>
    /// ✅ PHASE 1: Execute shot with automatic EV evaluation
    /// Eliminates 25+ duplicate blocks of the same 5 lines
    /// </summary>
    private bool ExecuteShot(ShotIntent intent, int targetRock, int rockCurrent, 
                            bool acceptRisk = false, bool mustScore = false, Vector2? targetPos = null)
    {
        ShotContext context = new ShotContext(intent, targetRock);
        context.acceptRisk = acceptRisk;
        context.mustScore = mustScore;
        
        if (targetPos.HasValue)
            context.idealFinalPosition = targetPos.Value;
        
        // Track which systems are active for callout display
        List<string> activeSystems = new List<string>();
        bool skillBasedActive = false;
        bool clutchActive = false;
        bool counterActive = false;
        bool evActive = false;
        bool multiShotActive = false;
        
        // ✅ PHASE 1 ENHANCEMENT 1: Skill-Based Shot Selection
        if (enhancements != null)
        {
            CharacterStats shooter = GetShooterStats(rockCurrent);
            string shooterName = GetShooterName(rockCurrent);
            
            context = enhancements.skillBased.AdjustForSkills(context, shooter, shooterName);
            skillBasedActive = true;
            activeSystems.Add("SKILL");
            
            // Set AI personality based on shooter skills
            enhancements.clutchPerformance.SetPersonalityFromStats(shooter);
        }
        
        // ✅ PHASE 1 ENHANCEMENT 2: Clutch Performance Modifiers
        if (enhancements != null)
        {
            AIGameState gameState = BuildGameState(rockCurrent);
            float pressure = enhancements.clutchPerformance.CalculatePressure(gameState, rockCurrent);
            
            if (pressure >= 30f) // Medium or high pressure
            {
                context = enhancements.clutchPerformance.ApplyClutchModifiers(context, pressure, gameState);
                clutchActive = true;
                activeSystems.Add(pressure >= 60f ? "HIGH CLUTCH" : "CLUTCH");
            }
        }
        
        // ✅ PHASE 1 ENHANCEMENT 3: Counter-Strategy (check if we should override)
        if (enhancements != null)
        {
            var detectedStrategy = enhancements.counterStrategy.GetCurrentStrategy();
            
            if (enhancements.counterStrategy.ShouldCounterStrategy(detectedStrategy))
            {
                ShotIntent counterIntent = enhancements.counterStrategy.GetCounterIntent(detectedStrategy, context.intent);
                
                // Only override if counter-intent is different
                if (counterIntent != context.intent)
                {
                    Debug.Log($"[Counter] OVERRIDING {context.intent} with {counterIntent}");
                    context.intent = counterIntent;
                    counterActive = true;
                    activeSystems.Add("COUNTER");
                }
            }
        }
        
        // Automatic EV evaluation (if enabled)
        if (evSystem != null && useEVOptimization)
        {
            ShotContext originalContext = context;
            context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
            
            // Check if EV actually changed the shot
            if (context.intent != originalContext.intent)
            {
                evActive = true;
                activeSystems.Add("EV OVERRIDE");
            }
            else if (useEVOptimization)
            {
                evActive = true;
                activeSystems.Add("EV");
            }
        }
        
        // 🎨 SHOW CALLOUT: Display active AI systems on the rock
        if (activeSystems.Count > 0)
        {
            TextCalloutManager calloutManager = FindObjectOfType<TextCalloutManager>();
            if (calloutManager != null)
            {
                string systemsText;
                
                // Break into multiple lines if more than 2 systems active
                if (activeSystems.Count > 2)
                {
                    systemsText = "AI SYSTEMS:\n" + string.Join("\n", activeSystems);
                }
                else
                {
                    systemsText = "AI: " + string.Join(" + ", activeSystems);
                }
                
                // FIXED: Attach to launcher position (0, -19) for proper stacking
                Vector3 launcherPosition = new Vector3(0f, -19f, 0f);
                
                // Show callout at launcher (will stack with other launcher callouts)
                calloutManager.ShowCallout(
                    launcherPosition,
                    systemsText,
                    followTarget: false, // Stay at launcher, don't follow anything
                    target: null,
                    duration: 3.0f
                );
                
                Debug.Log($"[AI Systems] Rock #{rockCurrent}: {string.Join(" + ", activeSystems)}");
            }
        }
        
        aiTarg.ExecuteIntent(context, rockCurrent);
        return true;
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
    
    /// <summary>
    /// Get shooter name for tracking purposes
    /// </summary>
    private string GetShooterName(int rockCurrent)
    {
        TeamManager tm = FindObjectOfType<TeamManager>();
        if (tm == null) return "Unknown";
        
        int memberIndex = rockCurrent / 4;
        memberIndex = Mathf.Clamp(memberIndex, 0, 3);
        
        bool isRedTeam = (rockCurrent % 2 == 0) ? gm.redHammer : !gm.redHammer;
        string teamName = isRedTeam ? "Red" : "Yellow";
        
        string[] positions = new string[] { "Lead", "Second", "Third", "Skip" };
        
        return $"{teamName}_{positions[memberIndex]}";
    }
    
    #region INTENT-BASED SHOT SELECTION METHODS
    
    /// <summary>
    /// 🎯 NEW: Multi-Shot Planning System
    /// Try to execute current step of strategic plan
    /// Returns true if plan is being followed, false if plan invalid/unavailable
    /// </summary>
    private bool TryExecutePlannedShot(int rockCurrent, string phase)
    {
        if (!useMultiShotPlanning) return false;
        if (multiShotPlanner == null) return false;
        
        // Get or create strategic plan
        bool hasHammer = (rockCurrent % 2 != 0);
        EndPlan plan = multiShotPlanner.GetPlan(rockCurrent, activeTeamName, hasHammer, activeTeamScore, oppTeamScore);
        
        if (plan == null || !plan.isValid)
        {
            if (planningVerboseLogging)
            {
                Debug.Log($"[MultiShot] No valid plan for rock {rockCurrent} - using single-shot logic");
            }
            return false;
        }
        
        // Execute current step of plan
        ShotIntent plannedIntent = plan.GetCurrentIntent();
        int targetRock = plan.GetCurrentTargetRock();
        Vector2 targetPos = plan.GetCurrentTargetPosition();
        
        Debug.Log($"[MultiShot] Executing plan '{plan.strategyName}' step {plan.currentStep + 1}/{plan.plannedIntents.Count}: {plannedIntent}");
        
        // 🎨 SHOW CALLOUT: Display multi-shot plan info
        TextCalloutManager calloutManager = FindObjectOfType<TextCalloutManager>();
        if (calloutManager != null)
        {
            string planText = $"MULTI-SHOT: {plan.strategyName}\nStep {plan.currentStep + 1}/{plan.plannedIntents.Count}";
            
            // FIXED: Attach to launcher position (0, -19) for proper stacking
            Vector3 launcherPosition = new Vector3(0f, -19f, 0f);
            
            calloutManager.ShowCallout(
                launcherPosition,
                planText,
                followTarget: false, // Stay at launcher, don't follow anything
                target: null,
                duration: 3.5f
            );
        }
        
        // Build shot context from plan
        ShotContext context = new ShotContext(plannedIntent, targetRock);
        
        // Inherit settings from plan
        context.idealFinalPosition = targetPos;
        context.acceptRisk = (plannedIntent == ShotIntent.RemoveThreat || plannedIntent == ShotIntent.Desperation);
        
        // Apply EV evaluation if enabled
        if (evSystem != null && useEVOptimization)
        {
            context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
        }
        
        // Execute the shot
        aiTarg.ExecuteIntent(context, rockCurrent);
        
        // Advance plan for next shot
        multiShotPlanner.AdvancePlan();
        
        return true;
    }
    
    /// <summary>
    /// ?? PROOF-OF-CONCEPT: Intent-based shot selection for ConservativeSteal
    /// This demonstrates the NEW architecture - simple, clear, smart!
    /// </summary>
    private bool TryIntentBasedShot_ConservativeSteal(int rockCurrent, string phase)
    {
        Rock_Info rockInfo = gm.rockList[rockCurrent].rockInfo;
        
        Debug.Log($"[IntentBased] ConservativeSteal - {phase} phase");
        
        // 🎯 STEP 1: Check if we have a multi-shot plan to follow
        if (TryExecutePlannedShot(rockCurrent, phase))
        {
            Debug.Log("[ConservativeSteal] ✅ Following multi-shot strategic plan!");
            return true;
        }
        
        // STEP 2: No plan - use single-shot intent logic
        Debug.Log("[ConservativeSteal] No plan - using single-shot intent logic");
        
        // ✅ REFACTORED: Use cached house analysis and ExecuteShot helper
        var house = GetHouseAnalysis();
        bool hasHammer = (rockCurrent % 2 != 0);
        
        // Get shooter skills for decision making
        var (finesse, weight, aim) = GetShooterSkillProfile(rockCurrent);
        bool isHighFinesse = (finesse >= 70f);
        bool isHighPower = (weight >= 70f && aim >= 70f);
        
        // EARLY PHASE: Setup game
        if (phase == "early")
        {
            // ✅ ENHANCED: Check threatRock exists AND should remove (with scoring benefit)
            if (house.threatRock >= 0 && ShouldRemoveThreatEnhanced(house, rockCurrent, hasHammer, phase))
            {
                Debug.Log($"[ConservativeSteal] EARLY: Removing threat rock #{house.threatRock}");
                return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent);
            }
            
            // PRIORITY 2: Protect my rocks if I have any
            if (house.myRocksInHouse >= 1)
            {
                return ExecuteShot(ShotIntent.CreateOpportunity, -1, rockCurrent); // Guard my rocks
            }
            
            // PRIORITY 3: Clean house - setup OR aggressive draw
            // High power teams → Draw aggressively
            // High finesse teams → Guard + draw behind
            if (isHighPower)
            {
                return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent); // Draw to button
            }
            else
            {
                return ExecuteShot(ShotIntent.CreateOpportunity, -1, rockCurrent); // Guard + draw next
            }
        }
        
        // MIDDLE PHASE: Build position or remove threats
        else if (phase == "middle")
        {
            // ✅ FIX: Simplified - always remove threats when they exist
            if (house.threatRock >= 0)
            {
                Debug.Log($"[ConservativeSteal] MIDDLE: Removing threat rock #{house.threatRock}");
                return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, 
                                  acceptRisk: house.myRocksInHouse > 0);
            }
            else if (house.myRocksInHouse > 1)
            {
                // We're in good shape - protect lead
                return ExecuteShot(ShotIntent.ProtectLead, -1, rockCurrent);
            }
            else
            {
                // Keep building
                return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
            }
        }
        
        // LATE PHASE: WITHOUT HAMMER - Steal or limit damage
        else if (phase == "late")
        {
            // ✅ IMPROVED: Use looser defensive criteria
            bool isDefensive = ShouldPlayDefensive(rockCurrent, hasHammer, phase);
            
            if (isDefensive)
            {
                Debug.Log($"[ConservativeSteal] LATE DEFENSIVE MODE: {activeTeamScore}-{oppTeamScore}");
                
                // SCENARIO 1: Opponent has rocks - REMOVE THEM!
                if (house.threatRock >= 0)
                {
                    Debug.Log($"[ConservativeSteal] LATE DEFENSIVE: Removing threat rock #{house.threatRock} to protect lead!");
                    return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true);
                }
                
                // SCENARIO 2: No threats, but we have rocks - protect them
                else if (house.myRocksInHouse > 0)
                {
                    Debug.Log($"[ConservativeSteal] LATE DEFENSIVE: Protecting {house.myRocksInHouse} rocks");
                    return ExecuteShot(ShotIntent.ProtectLead, -1, rockCurrent);
                }
                
                // SCENARIO 3: Clean house - conservative draw
                else
                {
                    Debug.Log($"[ConservativeSteal] LATE DEFENSIVE: Clean house, drawing conservatively");
                    return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
                }
            }
            
            // OFFENSIVE MODE (trailing or tied) - original steal logic
            Debug.Log($"[ConservativeSteal] LATE OFFENSIVE MODE: {activeTeamScore}-{oppTeamScore}");
            
            // ✅ REFACTORED: Use cached house analysis
            // SCENARIO 1: We have NO rocks, opponent has rock(s)
            if (house.myRocksInHouse == 0 && house.threatRock >= 0)
            {
                // Must remove threat to have ANY chance at stealing
                return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true);
            }
            
            // SCENARIO 2: We're WINNING the house
            else if (house.myRocksInHouse > 0 && house.myRocksInHouse > house.oppRocksInHouse)
            {
                // We're stealing! Protect what we have
                if (house.threatRock >= 0)
                {
                    // Threat exists - evaluate distance
                    float threatDist = Vector2.Distance(
                        gm.rockList[house.threatRock].rock.transform.position,
                        new Vector2(0f, 6.5f)
                    );
                    
                    if (threatDist < urgentThreatDistance)
                    {
                        // Must remove it
                        return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true);
                    }
                    else
                    {
                        // Threat is far - finesse what we have
                        return ExecuteShot(ShotIntent.ProtectLead, -1, rockCurrent);
                    }
                }
                else
                {
                    // No threats - protect lead with finesse
                    return ExecuteShot(ShotIntent.ProtectLead, -1, rockCurrent);
                }
            }
            
            // SCENARIO 3: We're TIED or LOSING the house
            else if (house.myRocksInHouse > 0 && house.threatRock >= 0)
            {
                // Evaluate: Can we steal by removing threat?
                float threatDist = Vector2.Distance(
                    gm.rockList[house.threatRock].rock.transform.position,
                    new Vector2(0f, 6.5f)
                );
                
                if (threatDist < closeThreatDistance)
                {
                    // Remove threat (might steal)
                    return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true);
                }
                else
                {
                    // Draw another rock (try to outscore them)
                    return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
                }
            }
            
            // SCENARIO 4: Clean house - easy steal attempt
            else
            {
                // No one has rocks - weight to button
                return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
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
        
        // 🎯 Check if we have a multi-shot plan to follow
        if (TryExecutePlannedShot(rockCurrent, phase))
        {
            Debug.Log("[AggressiveHammer] ✅ Following multi-shot strategic plan!");
            return true;
        }
        
        // ✅ REFACTORED: Use cached house analysis
        var house = GetHouseAnalysis();
        bool hasHammer = (rockCurrent % 2 != 0);
        
        // Get shooter skills
        var (finesse, weight, aim) = GetShooterSkillProfile(rockCurrent);
        bool isHighPower = (weight >= 70f && aim >= 70f);
        
        // EARLY: Aggressive setup with hammer
        if (phase == "early")
        {
            // PRIORITY 1: Remove threats ALWAYS (aggressive with hammer)
            if (house.threatRock >= 0)
            {
                return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true);
            }
            
            // PRIORITY 2: High power teams → Draw aggressively (no guards needed)
            if (isHighPower)
            {
                return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent); // Draw to button
            }
            
            // PRIORITY 3: Finesse teams → Build setup (guards + draws)
            else
            {
                // First rocks - setup for multi-point
                if (rockCurrent < 4)
                {
                    return ExecuteShot(ShotIntent.CreateOpportunity, -1, rockCurrent); // Guard
                }
                else
                {
                    return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent); // Draw behind guards
                }
            }
        }
        
        // MIDDLE: Aggressive removal or setup multi-point end
        else if (phase == "middle")
        {
            if (house.threatRock >= 0)
            {
                // Always remove threats when aggressive
                return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true);
            }
            else if (house.myRocksInHouse >= 1)
            {
                // Build on our position
                return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
            }
            else
            {
                // Keep setting up
                return ExecuteShot(ShotIntent.CreateOpportunity, -1, rockCurrent);
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
                bool shouldAttemptTakeout = (house.oppRocksInHouse >= 2 && !hasGuardInterference && house.myRocksInHouse >= 2);
                
                if (shouldAttemptTakeout && house.threatRock >= 0)
                {
                    // Attempt high-confidence takeout
                    return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true, mustScore: true);
                }
                
                // DEFAULT: SAFE DRAW TO BUTTON
                Debug.Log($"[HAMMER LAST ROCK] Drawing to button - accept force of 1, avoid blank!");
                return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
            }
            
            // Not last rock - general late-game logic
            if (house.threatRock >= 0 && activeTeamScore < oppTeamScore)
            {
                // Behind in game score - desperate removal
                return ExecuteShot(ShotIntent.Desperation, house.threatRock, rockCurrent, acceptRisk: true);
            }
            else if (house.threatRock >= 0 && house.oppRocksInHouse >= 2)
            {
                // They're building points - evaluate removal vs scoring
                if (house.myRocksInHouse > house.oppRocksInHouse)
                {
                    // We're winning house - keep scoring
                    return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
                }
                else
                {
                    // They're winning house - must remove
                    return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true);
                }
            }
            else if (house.myRocksInHouse == 0 && house.threatRock < 0)
            {
                // Clean house - weight for steal
                return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
            }
            else if (house.threatRock >= 0)
            {
                // Single threat rock - evaluate distance and position
                float threatDist = Vector2.Distance(
                    gm.rockList[house.threatRock].rock.transform.position,
                    new Vector2(0f, 6.5f)
                );
                
                // If threat is close to button and winning, remove it
                if (threatDist < urgentThreatDistance)
                {
                    return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true);
                }
                else
                {
                    // Threat is far or we're ahead - keep scoring
                    return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
                }
            }
            else
            {
                // No threats - just score!
                return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
            }
        }
        
        return false;
    }
    
    private bool TryIntentBasedShot_ScoreTwoOrBlank(int rockCurrent, string phase)
    {
        Debug.Log($"[IntentBased] ScoreTwoOrBlank - {phase} phase");
        
        // 🎯 Check if we have a multi-shot plan to follow
        if (TryExecutePlannedShot(rockCurrent, phase))
        {
            Debug.Log("[ScoreTwoOrBlank] ✅ Following multi-shot strategic plan!");
            return true;
        }
        
        // ✅ REFACTORED: Use cached house analysis
        var house = GetHouseAnalysis();
        bool hasHammer = (rockCurrent % 2 != 0);
        
        // Strategy: Clear any threats, build multiple scoring rocks
        
        // EARLY: Remove threats immediately, build corners
        if (phase == "early")
        {
            // ✅ ENHANCED: Check threatRock exists AND should remove (with scoring benefit)
            if (house.threatRock >= 0 && ShouldRemoveThreatEnhanced(house, rockCurrent, hasHammer, phase))
            {
                Debug.Log($"[ScoreTwoOrBlank] EARLY: Removing threat rock #{house.threatRock}");
                return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent);
            }
            else
            {
                // Draw to corners for 2-point setup
                return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
            }
        }
        
        // MIDDLE: Keep clearing, spread rocks
        else if (phase == "middle")
        {
            // ✅ FIX: Simplified - always remove threats
            if (house.threatRock >= 0)
            {
                Debug.Log($"[ScoreTwoOrBlank] MIDDLE: Removing threat rock #{house.threatRock}");
                return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent);
            }
            else if (house.myRocksInHouse >= 2)
            {
                // We have 2+ rocks - protect them!
                return ExecuteShot(ShotIntent.ProtectLead, -1, rockCurrent);
            }
            else
            {
                // Keep building
                return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
            }
        }
        
        // LATE: WITH HAMMER - Score 2+ or blank to keep hammer
        else if (phase == "late")
        {
            bool isLastRock = (rockCurrent >= 15);
            
            // LAST ROCK LOGIC: Must score something!
            if (isLastRock)
            {
                Debug.Log("[ScoreTwoOrBlank] LAST ROCK - must score!");
                
                // Have 2+ rocks already? Add more OR remove threats!
                if (house.myRocksInHouse >= 1)
                {
                    // ✅ CRITICAL FIX: ALWAYS remove threats first on last rock when leading
                    // Philosophy: When you have hammer + lead, DON'T RISK opponent scoring
                    if (house.threatRock >= 0)
                    {
                        // Check if opponent has shot rock or close rocks
                        bool opponentHasShotRock = false;
                        if (gm.houseList.Count > 0)
                        {
                            Rock_Info shotRockInfo = gm.houseList[0].rockInfo;
                            opponentHasShotRock = (shotRockInfo.teamName != activeTeamName);
                        }
                        
                        bool isDefensiveLastRock = (activeTeamScore > oppTeamScore);
                        
                        // DEFENSIVE + opponent has shot rock = MUST REMOVE!
                        if (isDefensiveLastRock || opponentHasShotRock)
                        {
                            Debug.Log($"[ScoreTwoOrBlank] LAST ROCK DEFENSIVE: Removing threat #{house.threatRock}!");
                            return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true, mustScore: true);
                        }
                        else
                        {
                            // We have 2+ rocks AND we're trailing - add more to score
                            return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
                        }
                    }
                    else
                    {
                        // No threats - add more rocks!
                        return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
                    }
                }
                // Threats exist? Remove and try to score
                else if (house.threatRock >= 0)
                {
                    return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true, mustScore: true);
                }
                // Nothing good? Blank to keep hammer next end
                else
                {
                    return ExecuteShot(ShotIntent.ForceBlank, -1, rockCurrent);
                }
            }
            
            // NOT LAST ROCK: Build for 2-point end or blank
            
            // ✅ IMPROVED: Use looser defensive criteria
            bool isDefensive = ShouldPlayDefensive(rockCurrent, hasHammer, phase);
            
            if (isDefensive)
            {
                Debug.Log($"[ScoreTwoOrBlank] LATE DEFENSIVE MODE: {activeTeamScore}-{oppTeamScore}");
                
                // SCENARIO 1: Opponent has rocks - REMOVE THEM!
                if (house.threatRock >= 0)
                {
                    Debug.Log($"[ScoreTwoOrBlank] LATE DEFENSIVE: Removing threat rock #{house.threatRock} to protect lead!");
                    return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true, mustScore: true);
                }
                
                // SCENARIO 2: No threats, we have rocks - protect them
                else if (house.myRocksInHouse >= 1)
                {
                    Debug.Log($"[ScoreTwoOrBlank] LATE DEFENSIVE: Protecting {house.myRocksInHouse} rocks");
                    return ExecuteShot(ShotIntent.ProtectLead, -1, rockCurrent);
                }
                
                // SCENARIO 3: Clean house - can still build for 2
                else
                {
                    Debug.Log($"[ScoreTwoOrBlank] LATE DEFENSIVE: Clean house, building for 2");
                    return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
                }
            }
            
            // OFFENSIVE MODE (trailing or tied) - original 2-or-blank logic
            Debug.Log($"[ScoreTwoOrBlank] LATE OFFENSIVE MODE: {activeTeamScore}-{oppTeamScore}");
            
            // Already have 1+? Add more or remove threats!
            if (house.myRocksInHouse >= 1)
            {
                if (house.threatRock < 0)
                {
                    // No threats - add more rocks!
                    return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
                }
                else
                {
                    // Threat exists - finesse our lead
                    return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true, mustScore: true);
                }
            }
            // Have 0 rocks? Decide: blank or try for 2
            else
            {
                if (house.threatRock >= 0)
                {
                    // Remove threats (might blank)
                    return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent);
                }
                else if (house.oppRocksInHouse == 0)
                {
                    // Clean house - can still build for 2
                    return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
                }
                else
                {
                    // Can't score 2 - force blank
                    return ExecuteShot(ShotIntent.ForceBlank, -1, rockCurrent);
                }
            }
        }
        
        return false;
    }
    
    private bool TryIntentBasedShot_AggressiveNotHammer(int rockCurrent, string phase)
    {
        Debug.Log($"[IntentBased] AggressiveNotHammer - {phase} phase");
        
        // 🎯 Check if we have a multi-shot plan to follow
        if (TryExecutePlannedShot(rockCurrent, phase))
        {
            Debug.Log("[AggressiveNotHammer] ✅ Following multi-shot strategic plan!");
            return true;
        }
        
        // ✅ REFACTORED: Use cached house analysis
        var house = GetHouseAnalysis();
        bool hasHammer = (rockCurrent % 2 != 0);
        
        // Get shooter skills
        var (finesse, weight, aim) = GetShooterSkillProfile(rockCurrent);
        bool isHighFinesse = (finesse >= 70f);

        // EARLY: Aggressive without hammer - steal at all costs
        if (phase == "early")
        {
            // PRIORITY 1: Remove any threats immediately (can't let them build)
            // ✅ ENHANCED: Check threatRock exists AND should remove (with scoring benefit)
            if (house.threatRock >= 0 && ShouldRemoveThreatEnhanced(house, rockCurrent, hasHammer, phase))
            {
                Debug.Log($"[AggressiveNotHammer] EARLY: Removing threat rock #{house.threatRock}");
                return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true);
            }
            
            // PRIORITY 2: High finesse → Setup steal (guard + draw)
            if (isHighFinesse && rockCurrent < 4)
            {
                return ExecuteShot(ShotIntent.CreateOpportunity, -1, rockCurrent); // Guard for steal
            }
            
            // PRIORITY 3: Default → Aggressive draw (try to steal immediately)
            return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
        }
        
        // MIDDLE: Keep pressure, remove threats
        else if (phase == "middle")
        {
            // ✅ FIX: Simplified logic - if threat exists, REMOVE IT (defensive play)
            if (house.threatRock >= 0)
            {
                Debug.Log($"[AggressiveNotHammer] MIDDLE: Removing threat rock #{house.threatRock}");
                return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true);
            }
            else if (house.myRocksInHouse > 0)
            {
                // Build on position
                return ExecuteShot(ShotIntent.CreateOpportunity, -1, rockCurrent);
            }
            else
            {
                // Keep setting up
                return ExecuteShot(ShotIntent.CreateOpportunity, -1, rockCurrent);
            }
        }
        
        // LATE: WITHOUT HAMMER - All-in for steal
        else if (phase == "late")
        {
            // ✅ IMPROVED: Use looser defensive criteria
            bool isDefensive = ShouldPlayDefensive(rockCurrent, hasHammer, phase);
            
            if (isDefensive)
            {
                Debug.Log($"[AggressiveNotHammer] LATE DEFENSIVE MODE: {activeTeamScore}-{oppTeamScore}");
                
                // SCENARIO 1: Opponent has rocks - REMOVE THEM!
                if (house.threatRock >= 0)
                {
                    Debug.Log($"[AggressiveNotHammer] LATE DEFENSIVE: Removing threat rock #{house.threatRock} to protect lead!");
                    return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true);
                }
                
                // SCENARIO 2: No threats, but we have rocks - protect them
                else if (house.myRocksInHouse > 0)
                {
                    Debug.Log($"[AggressiveNotHammer] LATE DEFENSIVE: Protecting {house.myRocksInHouse} rocks");
                    return ExecuteShot(ShotIntent.ProtectLead, -1, rockCurrent);
                }
                
                // SCENARIO 3: Clean house - conservative draw or force blank
                else
                {
                    Debug.Log($"[AggressiveNotHammer] LATE DEFENSIVE: Clean house, drawing conservatively");
                    return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
                }
            }
            
            // OFFENSIVE MODE (trailing or tied) - original aggressive logic
            Debug.Log($"[AggressiveNotHammer] LATE OFFENSIVE MODE: {activeTeamScore}-{oppTeamScore}");
            
            // SCENARIO 1: No rocks vs threat - desperate removal
            if (house.myRocksInHouse == 0 && house.threatRock >= 0)
            {
                // Must remove to have ANY chance
                return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true);
            }
            
            // SCENARIO 2: We have rocks AND threats exist
            else if (house.myRocksInHouse > 0 && house.threatRock >= 0)
            {
                // They're winning - ATTACK!
                if (!house.amWinningHouse)
                {
                    // Behind in game score? DESPERATION!
                    if (activeTeamScore <= oppTeamScore)
                    {
                        return ExecuteShot(ShotIntent.Desperation, house.threatRock, rockCurrent, acceptRisk: true, mustScore: true);
                    }
                    else
                    {
                        // Just remove threat
                        return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true);
                    }
                }
                // We're winning house - BUILD LEAD!
                else
                {
                    // Threat close? Remove it first
                    float threatDist = Vector2.Distance(
                        gm.rockList[house.threatRock].rock.transform.position,
                        new Vector2(0f, 6.5f)
                    );
                    
                    if (threatDist < closeThreatDistance)
                    {
                        return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true);
                    }
                    else
                    {
                        // Add more rocks to steal multiple
                        return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
                    }
                }
            }
            
            // SCENARIO 3: We have rocks, NO threats - keep scoring!
            else if (house.myRocksInHouse > 0)
            {
                // We're stealing - add more!
                return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
            }
            
            // SCENARIO 4: Clean house - weight for steal
            else
            {
                return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
            }
        }
        
        return false;
    }
    
    private bool TryIntentBasedShot_StealOrBlank(int rockCurrent, string phase)
    {
        Debug.Log($"[IntentBased] StealOrBlank - {phase} phase");
        
        // 🎯 Check if we have a multi-shot plan to follow
        if (TryExecutePlannedShot(rockCurrent, phase))
        {
            Debug.Log("[StealOrBlank] ✅ Following multi-shot strategic plan!");
            return true;
        }
        
        // ✅ REFACTORED: Use cached house analysis
        var house = GetHouseAnalysis();
        bool hasHammer = (rockCurrent % 2 != 0);
        
        // Strategy: Deny them points, steal if possible, blank acceptable
        
        // EARLY: Remove any threats, don't build unless safe
        if (phase == "early")
        {
            // ✅ ENHANCED: Check threatRock exists AND should remove (with scoring benefit)
            if (house.threatRock >= 0 && ShouldRemoveThreatEnhanced(house, rockCurrent, hasHammer, phase))
            {
                Debug.Log($"[StealOrBlank] EARLY: Removing threat rock #{house.threatRock}");
                return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent);
            }
            else if (house.myRocksInHouse > 0)
            {
                // We have rocks - finesse them
                return ExecuteShot(ShotIntent.ProtectLead, -1, rockCurrent);
            }
            else
            {
                // Draw for steal attempt
                return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
            }
        }
        
        // MIDDLE: Keep clearing, build cautiously
        else if (phase == "middle")
        {
            // ✅ FIX: Simplified - always remove threats
            if (house.threatRock >= 0)
            {
                Debug.Log($"[StealOrBlank] MIDDLE: Removing threat rock #{house.threatRock}");
                return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent);
            }
            else if (house.myRocksInHouse > 0)
            {
                // Protect what we have
                return ExecuteShot(ShotIntent.ProtectLead, -1, rockCurrent);
            }
            else
            {
                // Draw cautiously
                return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
            }
        }
        
        // LATE: WITHOUT HAMMER - Deny points or steal
        else if (phase == "late")
        {
            // ✅ IMPROVED: Use looser defensive criteria
            bool isDefensive = ShouldPlayDefensive(rockCurrent, hasHammer, phase);
            
            if (isDefensive)
            {
                Debug.Log($"[StealOrBlank] LATE DEFENSIVE MODE: {activeTeamScore}-{oppTeamScore}");
                
                // SCENARIO 1: Opponent has rocks - REMOVE THEM!
                if (house.threatRock >= 0)
                {
                    Debug.Log($"[StealOrBlank] LATE DEFENSIVE: Removing threat rock #{house.threatRock} to protect lead!");
                    return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent);
                }
                
                // SCENARIO 2: No threats, we have rocks - protect them
                else if (house.myRocksInHouse > 0)
                {
                    Debug.Log($"[StealOrBlank] LATE DEFENSIVE: Protecting {house.myRocksInHouse} rocks");
                    return ExecuteShot(ShotIntent.ProtectLead, -1, rockCurrent);
                }
                
                // SCENARIO 3: Clean house - blank is acceptable to deny points
                else
                {
                    Debug.Log($"[StealOrBlank] LATE DEFENSIVE: Clean house, forcing blank");
                    return ExecuteShot(ShotIntent.ForceBlank, -1, rockCurrent);
                }
            }
            
            // OFFENSIVE MODE (tied or trailing) - original steal/blank logic
            Debug.Log($"[StealOrBlank] LATE OFFENSIVE MODE: {activeTeamScore}-{oppTeamScore}");
            
            // Primary goal: Force them to 1 point OR steal OR blank
            
            // SCENARIO 1: They have 2+ rocks - DANGER!
            if (house.oppRocksInHouse >= 2)
            {
                if (house.threatRock >= 0)
                {
                    // Remove their best rock (try to reduce to 1 point)
                    return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent);
                }
                else
                {
                    // No clear removal - force blank
                    return ExecuteShot(ShotIntent.ForceBlank, -1, rockCurrent);
                }
            }
            
            // SCENARIO 2: They have 1 rock only
            else if (house.oppRocksInHouse == 1)
            {
                if (house.myRocksInHouse > 0)
                {
                    // We're stealing! Protect what we have
                    return ExecuteShot(ShotIntent.ProtectLead, -1, rockCurrent);
                }
                else if (house.threatRock >= 0)
                {
                    // Remove their single rock (steal or blank)
                    return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent);
                }
                else
                {
                    // Try to steal
                    return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
                }
            }
            
            // SCENARIO 3: We're winning the house!
            else if (house.myRocksInHouse > 0)
            {
                // Stealing! Protect the steal
                if (house.threatRock >= 0)
                {
                    // Small threat exists - finesse instead of removing
                    return ExecuteShot(ShotIntent.ProtectLead, -1, rockCurrent);
                }
                else
                {
                    // No threats - finesse the steal
                    return ExecuteShot(ShotIntent.ProtectLead, -1, rockCurrent);
                }
            }
            
            // SCENARIO 4: Clean house - blank is acceptable
            else
            {
                // Blank is fine - keeps hammer for them but no damage
                return ExecuteShot(ShotIntent.ForceBlank, -1, rockCurrent);
            }
        }
        
        return false;
    }
    
    #endregion
    
    #endregion // INTENT-BASED SHOT SELECTION METHODS

    public void OnShot(int rockCurrent)
    {
        // ✅ PHASE 2: Clear analysis cache at start of each turn
        _cachedHouseAnalysis = null;
        
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

        // ✅ CRITICAL: UNIVERSAL REMOVAL CHECK
        // Check if we should be removing threats BEFORE any strategy routing!
        // This applies to BOTH defensive (protecting lead) AND offensive (too many opponent rocks)
        bool hasHammer = (rockCurrent % 2 != 0);
        
        // Calculate phase inline
        string phase;
        if (rockCurrent < 4)
            phase = "early";
        else if (rockCurrent < 10)
            phase = "middle";
        else
            phase = "late";
        
        bool isDefensive = ShouldPlayDefensive(rockCurrent, hasHammer, phase);
        
        // Count opponent rocks in house
        int oppRocksInHouse = 0;
        int myRocksInHouse = 0;
        
        foreach (var houseRock in gm.houseList)
        {
            if (houseRock.rockInfo.teamName != activeTeamName)
            {
                oppRocksInHouse++;
            }
            else
            {
                myRocksInHouse++;
            }
        }
        
        // 🚨 REMOVAL PRIORITY 1: DEFENSIVE (protecting lead with opponent rocks)
        if (isDefensive && oppRocksInHouse > 0)
        {
            var house = GetHouseAnalysis();
            
            Debug.LogError($"[UNIVERSAL DEFENSIVE] Leading {activeTeamScore}-{oppTeamScore}, opponent has {oppRocksInHouse} rocks!");
            Debug.LogError($"[UNIVERSAL DEFENSIVE] FORCING REMOVAL BEFORE ANY STRATEGY!");
            
            if (house.threatRock >= 0)
            {
                Debug.LogError($"[UNIVERSAL DEFENSIVE] Targeting threat rock #{house.threatRock} for immediate removal!");
                
                // Execute shot through ExecuteShot (will apply all enhancements)
                ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true);
                return; // DONE - defensive removal executed!
            }
        }
        
        // 🚨 REMOVAL PRIORITY 2: OFFENSIVE (opponent building too many rocks)
        // Even when trailing, if opponent has 3+ rocks, we MUST start clearing!
        if (!isDefensive && oppRocksInHouse >= 3)
        {
            var house = GetHouseAnalysis();
            
            Debug.LogError($"[UNIVERSAL OFFENSIVE] Opponent has {oppRocksInHouse} rocks - TOO MANY!");
            Debug.LogError($"[UNIVERSAL OFFENSIVE] FORCING REMOVAL even though trailing!");
            
            if (house.threatRock >= 0)
            {
                Debug.LogError($"[UNIVERSAL OFFENSIVE] Targeting threat rock #{house.threatRock} for immediate removal!");
                
                // Execute shot through ExecuteShot (will apply all enhancements)
                ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true);
                return; // DONE - offensive removal executed!
            }
        }
        
        // 🚨 REMOVAL PRIORITY 3: LATE GAME WITHOUT HAMMER (must clear to steal)
        // If late game without hammer and opponent has 2+ rocks, we should clear
        if (phase == "late" && !hasHammer && oppRocksInHouse >= 2)
        {
            var house = GetHouseAnalysis();
            
            Debug.LogError($"[UNIVERSAL STEAL ATTEMPT] Late game without hammer, opponent has {oppRocksInHouse} rocks!");
            Debug.LogError($"[UNIVERSAL STEAL ATTEMPT] FORCING REMOVAL to setup steal!");
            
            if (house.threatRock >= 0)
            {
                Debug.LogError($"[UNIVERSAL STEAL ATTEMPT] Targeting threat rock #{house.threatRock} for immediate removal!");
                
                // Execute shot through ExecuteShot (will apply all enhancements)
                ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true);
                return; // DONE - steal setup removal executed!
            }
        }
        
        // 🚨 REMOVAL PRIORITY 4: LOSING HOUSE (opponent winning shot rock position)
        // If opponent is closer to button than us AND has multiple rocks, clear them!
        if (oppRocksInHouse > myRocksInHouse && oppRocksInHouse >= 2)
        {
            var house = GetHouseAnalysis();
            
            // Check if opponent has shot rock (closest to button)
            bool opponentHasShotRock = false;
            if (gm.houseList.Count > 0 && gm.houseList[0].rockInfo.teamName != activeTeamName)
            {
                opponentHasShotRock = true;
            }
            
            if (opponentHasShotRock && house.threatRock >= 0)
            {
                Debug.LogError($"[UNIVERSAL LOSING HOUSE] Opponent has shot rock + {oppRocksInHouse} total rocks!");
                Debug.LogError($"[UNIVERSAL LOSING HOUSE] FORCING REMOVAL to contest house!");
                Debug.LogError($"[UNIVERSAL LOSING HOUSE] Targeting threat rock #{house.threatRock} for immediate removal!");
                
                // Execute shot through ExecuteShot (will apply all enhancements)
                ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: true);
                return; // DONE - house contest removal executed!
            }
        }
        
        // Only continue to normal strategy routing if NO removal priority triggered
        
        // Continue with normal strategy routing only if NOT in defensive removal mode...
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
