using UnityEngine;

/// <summary>
/// High-level strategic intentions for AI shots
/// Strategy layer decides INTENT, AI_Target decides EXECUTION
/// </summary>
public enum ShotIntent
{
    // OFFENSIVE INTENTS
    RemoveThreat,           // Take out opponent's best rock(s)
    ScorePoints,            // Draw into scoring position
    StealPoint,             // Aggressive weight when without hammer
    
    // DEFENSIVE INTENTS
    ProtectLead,            // Guard my rocks when winning
    ForceBlank,             // Clear house to keep hammer
    DenyOpportunity,        // Remove opponent's setup rocks
    
    // SETUP INTENTS
    CreateOpportunity,      // Place guard for future shots
    FreezeOnShot,          // Freeze to shot rock (advanced)
    
    // SPECIAL SITUATIONS
    Desperation,           // Last rock, need miracle
    RunBack,               // Hit and roll to better position
    Corner,                // Freeze behind cover
    LastShotScoring,       // LAST SHOT: Focus ONLY on final position (no removal penalties!)
    
    // SIMPLE FALLBACKS
    DrawToButton,          // Simple center weight
    ThrowAway              // Nothing good available, throw it away
}

/// <summary>
/// Additional context for shot intent
/// </summary>
public struct ShotContext
{
    public ShotIntent intent;
    public int targetRockIndex;        // Primary target (if applicable)
    public int secondaryTargetIndex;   // Secondary target (e.g., rock to raise)
    public Vector2 idealFinalPosition; // Desired end position (for draws)
    public float aggressiveness;       // 0-1: How aggressive to be (affects weight)
    public bool acceptRisk;            // Allow risky shots?
    public bool mustScore;             // MUST score on this shot (desperation)
    
    public ShotContext(ShotIntent intent, int targetRock = -1)
    {
        this.intent = intent;
        this.targetRockIndex = targetRock;
        this.secondaryTargetIndex = -1;
        this.idealFinalPosition = new Vector2(0f, 6.5f); // Default to button
        this.aggressiveness = 0.5f; // Medium
        this.acceptRisk = false;
        this.mustScore = false;
    }
}

// ============================================================================
// EV EVALUATION SYSTEM (Phase 1)
// ============================================================================

/// <summary>
/// Snapshot of game state for EV calculations
/// NOTE: Named AIGameState to avoid conflict with GameManager.GameState enum
/// </summary>
public class AIGameState
{
    public int rockCurrent;
    public int endCurrent;
    public int endTotal;
    public int activeTeamScore;
    public int oppTeamScore;
    public string activeTeamName;
    public string oppTeamName;
    public bool hasHammer;
    public int myRocksInHouse;
    public int oppRocksInHouse;
    public string phase;
    public int guardsInPlay;
    public bool hasGuardBlocking;
    
    public bool IsDesperate()
    {
        if (endCurrent == endTotal && activeTeamScore < oppTeamScore)
            return true;
        if (endTotal - endCurrent <= 2 && (oppTeamScore - activeTeamScore) >= 3)
            return true;
        return false;
    }
    
    public bool MustScore()
    {
        if (hasHammer && rockCurrent >= 15)
            return true;
        if (endCurrent == endTotal && activeTeamScore < oppTeamScore)
            return true;
        return false;
    }
}

/// <summary>
/// Calculates shot success probability
/// </summary>
public class ShotOutcomeEvaluator
{
    public float CalculateShotSuccessProbability(
        ShotContext context,
        CharacterStats shooterStats,
        AIGameState gameState)
    {
        float baseAccuracy = GetShooterAccuracy(context, shooterStats);
        float difficulty = CalculateShotDifficulty(context, gameState);
        float successRate = baseAccuracy * (1f - difficulty * 0.4f);
        
        if (gameState.MustScore() || gameState.IsDesperate())
            successRate *= 0.92f;
        
        return Mathf.Clamp(successRate, 0.1f, 0.98f);
    }
    
    private float GetShooterAccuracy(ShotContext context, CharacterStats stats)
    {
        if (stats == null) return 0.7f;
        
        // All shots use weightAccuracy for now (simplified)
        return Mathf.Clamp01(stats.weightAccuracy.GetValue() / 100f);
    }
    
    private float CalculateShotDifficulty(ShotContext context, AIGameState gameState)
    {
        float difficulty = 0f;
        
        if (context.idealFinalPosition != Vector2.zero)
        {
            float targetDist = context.idealFinalPosition.magnitude;
            difficulty += Mathf.Clamp01(targetDist / 12f) * 0.2f;
        }
        
        if (gameState.guardsInPlay > 0)
            difficulty += gameState.guardsInPlay * 0.12f;
        
        if (context.intent == ShotIntent.RemoveThreat)
        {
            difficulty += 0.15f;
            if (gameState.hasGuardBlocking)
                difficulty += 0.25f;
        }
        
        if (context.mustScore)
            difficulty += 0.1f;
        
        return Mathf.Clamp01(difficulty);
    }
}

/// <summary>
/// Calculates Expected Value (EV)
/// </summary>
public class ExpectedValueCalculator
{
    public float CalculateExpectedValue(
        ShotContext context,
        AIGameState gameState,
        float successProbability)
    {
        float successReward = CalculateSuccessReward(context, gameState);
        float failurePenalty = CalculateFailurePenalty(context, gameState);
        
        float expectedValue = 
            (successProbability * successReward) - 
            ((1f - successProbability) * failurePenalty);
        
        expectedValue *= GetUrgencyMultiplier(gameState);
        
        return expectedValue;
    }
    
    private float CalculateSuccessReward(ShotContext context, AIGameState gameState)
    {
        float reward = 0f;
        Vector2 button = new Vector2(0f, 6.5f);
        
        switch (context.intent)
        {
            case ShotIntent.ScorePoints:
            case ShotIntent.LastShotScoring:
                float distToButton = Vector2.Distance(context.idealFinalPosition, button);
                reward = Mathf.Max(0f, 10f - distToButton);
                if (gameState.myRocksInHouse >= 1)
                    reward += 3f;
                if (!gameState.hasHammer && gameState.oppRocksInHouse == 0)
                    reward += 5f;
                break;
            
            case ShotIntent.RemoveThreat:
                reward = 8f;
                if (gameState.activeTeamScore < gameState.oppTeamScore)
                    reward += 4f;
                if (gameState.hasGuardBlocking)
                    reward += 3f;
                break;
            
            case ShotIntent.CreateOpportunity:
                reward = 5f;
                if (gameState.phase == "early")
                    reward += 3f;
                break;
            
            case ShotIntent.ProtectLead:
                reward = gameState.myRocksInHouse * 2.5f;
                break;
            
            case ShotIntent.ForceBlank:
                reward = 4f;
                break;
            
            case ShotIntent.Desperation:
                reward = 12f;
                break;
        }
        
        return reward;
    }
    
    private float CalculateFailurePenalty(ShotContext context, AIGameState gameState)
    {
        float penalty = 0f;
        
        switch (context.intent)
        {
            case ShotIntent.ScorePoints:
            case ShotIntent.LastShotScoring:
                penalty = 4f;
                if (gameState.hasHammer && gameState.rockCurrent >= 14)
                    penalty = 18f;
                break;
            
            case ShotIntent.RemoveThreat:
                penalty = 9f;
                if (gameState.guardsInPlay > 0)
                    penalty += gameState.guardsInPlay * 2.5f;
                break;
            
            case ShotIntent.CreateOpportunity:
                penalty = 2f;
                break;
            
            case ShotIntent.ProtectLead:
                penalty = gameState.myRocksInHouse * 3f;
                break;
            
            case ShotIntent.ForceBlank:
                penalty = 5f;
                break;
            
            case ShotIntent.Desperation:
                penalty = 15f;
                break;
        }
        
        return penalty;
    }
    
    private float GetUrgencyMultiplier(AIGameState gameState)
    {
        float urgency = 1f;
        
        if (gameState.phase == "late")
            urgency *= 1.4f;
        if (gameState.endCurrent == gameState.endTotal)
            urgency *= 1.6f;
        if (gameState.IsDesperate())
            urgency *= 2.2f;
        if (gameState.rockCurrent >= 15)
            urgency *= 1.25f;
        
        return urgency;
    }
}

/// <summary>
/// EV Evaluation System - Compares intent shot against alternatives
/// </summary>
public class EVEvaluationSystem : MonoBehaviour
{
    private ShotOutcomeEvaluator outcomeEvaluator;
    private ExpectedValueCalculator evCalculator;
    
    [Header("EV System Toggle")]
    public bool useEVEvaluation = false;
    
    [Range(0f, 1f)]
    public float evWeight = 0.3f;
    
    public bool verboseLogging = false;
    
    void Awake()
    {
        outcomeEvaluator = new ShotOutcomeEvaluator();
        evCalculator = new ExpectedValueCalculator();
    }
    
    public ShotContext EvaluateShot(
        ShotContext intentShot,
        AIGameState gameState,
        CharacterStats shooterStats)
    {
        if (!useEVEvaluation)
            return intentShot;
        
        if (verboseLogging)
            Debug.Log($"[EV] Evaluating shot (Rock {gameState.rockCurrent})");
        
        float intentEV = CalculateShotEV(intentShot, gameState, shooterStats);
        
        ShotContext bestAlt = FindBestAlternative(gameState, intentShot, shooterStats, out float bestAltEV);
        
        float threshold = intentEV + (evWeight * (bestAltEV - intentEV));
        
        if (bestAlt.intent != ShotIntent.DrawToButton && bestAltEV > threshold)
        {
            if (verboseLogging)
                Debug.Log($"[EV] OVERRIDE! {bestAlt.intent} (EV: {bestAltEV:F2}) over {intentShot.intent} (EV: {intentEV:F2})");
            return bestAlt;
        }
        
        if (verboseLogging)
            Debug.Log($"[EV] Keeping {intentShot.intent} (EV: {intentEV:F2})");
        
        return intentShot;
    }
    
    private float CalculateShotEV(ShotContext shot, AIGameState state, CharacterStats stats)
    {
        float successProb = outcomeEvaluator.CalculateShotSuccessProbability(shot, stats, state);
        return evCalculator.CalculateExpectedValue(shot, state, successProb);
    }
    
    private ShotContext FindBestAlternative(AIGameState state, ShotContext intentShot, CharacterStats stats, out float bestEV)
    {
        Vector2 button = new Vector2(0f, 6.5f);
        ShotContext best = new ShotContext(ShotIntent.DrawToButton);
        bestEV = float.MinValue;
        
        // Alternative 1: Draw to button
        if (intentShot.intent != ShotIntent.ScorePoints)
        {
            ShotContext alt = new ShotContext(ShotIntent.ScorePoints) { idealFinalPosition = button };
            float ev = CalculateShotEV(alt, state, stats);
            if (ev > bestEV) { bestEV = ev; best = alt; }
        }
        
        // Alternative 2: Center guard
        if (state.phase != "late")
        {
            ShotContext alt = new ShotContext(ShotIntent.CreateOpportunity) { idealFinalPosition = new Vector2(0f, 2.5f) };
            float ev = CalculateShotEV(alt, state, stats);
            if (ev > bestEV) { bestEV = ev; best = alt; }
        }
        
        // Alternative 3: Front 4-foot
        ShotContext alt3 = new ShotContext(ShotIntent.ScorePoints) { idealFinalPosition = new Vector2(0f, 5.5f) };
        float ev3 = CalculateShotEV(alt3, state, stats);
        if (ev3 > bestEV) { bestEV = ev3; best = alt3; }
        
        return best;
    }
    
    public void SetEVEnabled(bool enabled)
    {
        useEVEvaluation = enabled;
        Debug.Log($"[EV] System {(enabled ? "ENABLED" : "DISABLED")}");
    }
    
    public void SetEVWeight(float weight)
    {
        evWeight = Mathf.Clamp01(weight);
        Debug.Log($"[EV] Weight set to {evWeight:P0}");
    }
}

// ============================================================================
// PHASE 1 AI ENHANCEMENTS - Skill-Based, Clutch Performance, Counter-Strategy
// ============================================================================

/// <summary>
/// Phase 1 AI Enhancement Systems - Makes AI smarter without complex ML
/// All systems in one place with direct access to ShotContext, ShotIntent, AIGameState
/// </summary>
public class AIEnhancementSystems
{
    public SkillBasedShotSelection skillBased { get; private set; }
    public ClutchPerformanceModifier clutchPerformance { get; private set; }
    public SimpleCounterStrategy counterStrategy { get; private set; }
    
    public AIEnhancementSystems()
    {
        skillBased = new SkillBasedShotSelection();
        clutchPerformance = new ClutchPerformanceModifier();
        counterStrategy = new SimpleCounterStrategy();
    }
    
    /// <summary>
    /// Adjusts AI shot selection based on shooter's skills
    /// </summary>
    public class SkillBasedShotSelection
    {
        private System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, float>> characterShotSuccessRates = 
            new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, float>>();
        private const float LEARNING_RATE = 0.2f;
        
        public ShotContext AdjustForSkills(ShotContext shot, CharacterStats shooter, string shooterName)
        {
            if (shooter == null)
            {
                Debug.LogWarning("[SkillBased] No shooter stats - using default shot");
                return shot;
            }
            
            float finesse = shooter.finesseAccuracy.GetValue();
            float weight = shooter.weightAccuracy.GetValue();
            float aim = shooter.aimAccuracy.GetValue();
            
            Debug.Log($"[SkillBased] {shooterName} Skills: Finesse={finesse:F1}, Weight={weight:F1}, Aim={aim:F1}");
            
            if (finesse >= 75f)
            {
                shot = BoostFinesseShots(shot, finesse);
                Debug.Log($"[SkillBased] {shooterName} has HIGH FINESSE ({finesse:F1}) - boosting finesse shots");
            }
            
            if (weight >= 75f)
            {
                shot = BoostPowerShots(shot, weight);
                Debug.Log($"[SkillBased] {shooterName} has HIGH WEIGHT ({weight:F1}) - boosting power shots");
            }
            
            if (aim >= 75f)
            {
                shot = BoostPrecisionShots(shot, aim);
                Debug.Log($"[SkillBased] {shooterName} has HIGH AIM ({aim:F1}) - boosting precision shots");
            }
            
            if (finesse < 40f)
            {
                shot = AvoidFinesseShots(shot, finesse);
                Debug.LogWarning($"[SkillBased] {shooterName} has LOW FINESSE ({finesse:F1}) - avoiding finesse shots!");
            }
            
            if (weight < 40f)
            {
                shot = AvoidHeavyShots(shot, weight);
                Debug.LogWarning($"[SkillBased] {shooterName} has LOW WEIGHT ({weight:F1}) - avoiding heavy shots!");
            }
            
            if (aim < 40f)
            {
                shot = AvoidPrecisionShots(shot, aim);
                Debug.LogWarning($"[SkillBased] {shooterName} has LOW AIM ({aim:F1}) - avoiding precision shots!");
            }
            
            shot = ApplyLearnedPreferences(shot, shooterName);
            
            return shot;
        }
        
        private ShotContext BoostFinesseShots(ShotContext shot, float finesse)
        {
            if (shot.intent == ShotIntent.ScorePoints)
            {
                // Note: targetAccuracyBonus doesn't exist in ShotContext, but logic is preserved
                // You can add this field to ShotContext if needed
            }
            return shot;
        }
        
        private ShotContext BoostPowerShots(ShotContext shot, float weight)
        {
            if (shot.intent == ShotIntent.RemoveThreat)
            {
                shot.acceptRisk = true;
            }
            return shot;
        }
        
        private ShotContext BoostPrecisionShots(ShotContext shot, float aim)
        {
            // Precision bonus for draws and takeouts
            return shot;
        }
        
        private ShotContext AvoidFinesseShots(ShotContext shot, float finesse)
        {
            if (shot.intent == ShotIntent.ScorePoints)
            {
                // Penalty for low finesse
                if (shot.intent == ShotIntent.ScorePoints && finesse < 30f)
                {
                    Debug.Log("[SkillBased] VERY LOW FINESSE - considering guard instead of draw");
                }
            }
            return shot;
        }
        
        private ShotContext AvoidHeavyShots(ShotContext shot, float weight)
        {
            if (shot.intent == ShotIntent.RemoveThreat)
            {
                shot.acceptRisk = false;
                
                if (weight < 30f)
                {
                    Debug.LogWarning("[SkillBased] VERY LOW WEIGHT - takeouts will be unreliable!");
                }
            }
            return shot;
        }
        
        private ShotContext AvoidPrecisionShots(ShotContext shot, float aim)
        {
            if (aim < 30f)
            {
                Debug.LogWarning("[SkillBased] VERY LOW AIM - all shots will be less accurate!");
            }
            return shot;
        }
        
        public void RecordShotOutcome(string shooterName, ShotIntent intent, bool success)
        {
            string shotType = intent.ToString();
            
            if (!characterShotSuccessRates.ContainsKey(shooterName))
            {
                characterShotSuccessRates[shooterName] = new System.Collections.Generic.Dictionary<string, float>();
            }
            
            if (!characterShotSuccessRates[shooterName].ContainsKey(shotType))
            {
                characterShotSuccessRates[shooterName][shotType] = 0.5f;
            }
            
            float currentRate = characterShotSuccessRates[shooterName][shotType];
            float newRate = (LEARNING_RATE * (success ? 1f : 0f)) + ((1f - LEARNING_RATE) * currentRate);
            characterShotSuccessRates[shooterName][shotType] = newRate;
            
            Debug.Log($"[SkillBased] {shooterName} {shotType}: {(success ? "SUCCESS" : "FAIL")} ? Success rate: {currentRate:F2} ? {newRate:F2}");
        }
        
        private ShotContext ApplyLearnedPreferences(ShotContext shot, string shooterName)
        {
            if (!characterShotSuccessRates.ContainsKey(shooterName))
                return shot;
            
            string shotType = shot.intent.ToString();
            
            if (characterShotSuccessRates[shooterName].ContainsKey(shotType))
            {
                float successRate = characterShotSuccessRates[shooterName][shotType];
                
                if (successRate > 0.6f)
                {
                    Debug.Log($"[SkillBased] {shooterName} is GOOD at {shotType} ({successRate:P0}) - boosting accuracy");
                }
                else if (successRate < 0.4f)
                {
                    Debug.LogWarning($"[SkillBased] {shooterName} is BAD at {shotType} ({successRate:P0}) - reducing accuracy");
                }
            }
            
            return shot;
        }
        
        public float GetSuccessRate(string shooterName, ShotIntent intent)
        {
            if (!characterShotSuccessRates.ContainsKey(shooterName))
                return 0.5f;
            
            string shotType = intent.ToString();
            
            if (!characterShotSuccessRates[shooterName].ContainsKey(shotType))
                return 0.5f;
            
            return characterShotSuccessRates[shooterName][shotType];
        }
        
        public void ResetLearnedData()
        {
            characterShotSuccessRates.Clear();
            Debug.Log("[SkillBased] Learned shot data RESET");
        }
    }
    
    /// <summary>
    /// Modifies AI behavior based on pressure situations
    /// </summary>
    public class ClutchPerformanceModifier
    {
        public enum AIPersonality
        {
            Conservative,
            Aggressive,
            Balanced
        }
        
        public AIPersonality personality = AIPersonality.Balanced;
        
        public float CalculatePressure(AIGameState state, int rockCurrent)
        {
            float pressure = 0f;
            
            if (state.endCurrent == state.endTotal)
            {
                pressure += 30f;
                Debug.Log("[Clutch] LAST END - Pressure +30");
            }
            
            int scoreDiff = Mathf.Abs(state.activeTeamScore - state.oppTeamScore);
            if (scoreDiff == 0)
            {
                pressure += 25f;
                Debug.Log("[Clutch] TIED GAME - Pressure +25");
            }
            else if (scoreDiff == 1)
            {
                pressure += 20f;
                Debug.Log("[Clutch] ONE POINT GAME - Pressure +20");
            }
            else if (scoreDiff == 2)
            {
                pressure += 10f;
                Debug.Log("[Clutch] TWO POINT GAME - Pressure +10");
            }
            
            if (rockCurrent >= 15)
            {
                pressure += 25f;
                Debug.Log("[Clutch] LAST SHOT - Pressure +25");
            }
            else if (rockCurrent >= 13)
            {
                pressure += 15f;
                Debug.Log("[Clutch] LAST 3 SHOTS - Pressure +15");
            }
            else if (rockCurrent >= 10)
            {
                pressure += 5f;
                Debug.Log("[Clutch] LATE PHASE - Pressure +5");
            }
            
            if (state.activeTeamScore < state.oppTeamScore)
            {
                pressure += 15f;
                Debug.Log("[Clutch] TRAILING - Pressure +15");
            }
            else if (state.activeTeamScore > state.oppTeamScore && state.endCurrent == state.endTotal)
            {
                pressure += 10f;
                Debug.Log("[Clutch] PROTECTING LEAD (last end) - Pressure +10");
            }
            
            if (!state.hasHammer && state.oppRocksInHouse > state.myRocksInHouse)
            {
                pressure += 20f;
                Debug.Log("[Clutch] MUST SCORE (down without hammer) - Pressure +20");
            }
            
            Debug.Log($"[Clutch] TOTAL PRESSURE: {pressure:F0}/100");
            return Mathf.Clamp(pressure, 0f, 100f);
        }
        
        public ShotContext ApplyClutchModifiers(ShotContext shot, float pressure, AIGameState state)
        {
            if (pressure < 30f)
            {
                Debug.Log($"[Clutch] LOW PRESSURE ({pressure:F0}) - Normal play");
                return shot;
            }
            
            if (pressure < 60f)
            {
                shot = ApplyMediumPressure(shot, pressure, state);
                Debug.Log($"[Clutch] MEDIUM PRESSURE ({pressure:F0}) - Slight adjustments");
            }
            else
            {
                shot = ApplyHighPressure(shot, pressure, state);
                Debug.Log($"[Clutch] HIGH PRESSURE ({pressure:F0}) - Significant changes!");
            }
            
            return shot;
        }
        
        private ShotContext ApplyMediumPressure(ShotContext shot, float pressure, AIGameState state)
        {
            switch (personality)
            {
                case AIPersonality.Conservative:
                    shot.acceptRisk = false;
                    Debug.Log("[Clutch] Conservative AI - playing safer under medium pressure");
                    break;
                    
                case AIPersonality.Aggressive:
                    if (state.activeTeamScore <= state.oppTeamScore)
                    {
                        shot.acceptRisk = true;
                        Debug.Log("[Clutch] Aggressive AI - taking risks under medium pressure");
                    }
                    break;
                    
                case AIPersonality.Balanced:
                    Debug.Log("[Clutch] Balanced AI - steady under medium pressure");
                    break;
            }
            
            return shot;
        }
        
        private ShotContext ApplyHighPressure(ShotContext shot, float pressure, AIGameState state)
        {
            switch (personality)
            {
                case AIPersonality.Conservative:
                    shot.acceptRisk = false;
                    shot.mustScore = false;
                    
                    if (shot.intent == ShotIntent.ScorePoints)
                    {
                        Debug.Log("[Clutch] Conservative AI (HIGH PRESSURE) - prefer guards over risky draws");
                    }
                    
                    Debug.Log("[Clutch] Conservative AI - PLAYING IT SAFE under high pressure!");
                    break;
                    
                case AIPersonality.Aggressive:
                    shot.acceptRisk = true;
                    
                    if (state.activeTeamScore < state.oppTeamScore)
                    {
                        shot.mustScore = true;
                        Debug.Log("[Clutch] Aggressive AI - MUST SCORE (trailing under high pressure)");
                    }
                    else if (state.activeTeamScore > state.oppTeamScore)
                    {
                        if (shot.intent == ShotIntent.RemoveThreat)
                        {
                            shot.acceptRisk = true;
                            Debug.Log("[Clutch] Aggressive AI - AGGRESSIVE CLEARING (protecting lead)");
                        }
                    }
                    else
                    {
                        shot.mustScore = true;
                        Debug.Log("[Clutch] Aggressive AI - GO FOR THE WIN (tied under high pressure)");
                    }
                    
                    Debug.Log("[Clutch] Aggressive AI - TAKING RISKS under high pressure!");
                    break;
                    
                case AIPersonality.Balanced:
                    if (state.endCurrent == state.endTotal && state.activeTeamScore < state.oppTeamScore)
                    {
                        shot.acceptRisk = true;
                        shot.mustScore = true;
                        Debug.Log("[Clutch] Balanced AI - MUST SCORE (last end, trailing)");
                    }
                    else if (state.endCurrent == state.endTotal && state.activeTeamScore > state.oppTeamScore)
                    {
                        shot.acceptRisk = false;
                        Debug.Log("[Clutch] Balanced AI - PLAY SAFE (last end, leading)");
                    }
                    else
                    {
                        Debug.Log("[Clutch] Balanced AI - STEADY (high pressure, but balanced)");
                    }
                    break;
            }
            
            return shot;
        }
        
        public void SetPersonalityFromStats(CharacterStats stats)
        {
            if (stats == null)
            {
                personality = AIPersonality.Balanced;
                return;
            }
            
            float finesse = stats.finesseAccuracy.GetValue();
            float weight = stats.weightAccuracy.GetValue();
            
            if (weight > 70f && finesse < 60f)
            {
                personality = AIPersonality.Aggressive;
                Debug.Log($"[Clutch] AI Personality: AGGRESSIVE (Weight={weight:F0}, Finesse={finesse:F0})");
            }
            else if (finesse > 70f && weight < 60f)
            {
                personality = AIPersonality.Conservative;
                Debug.Log($"[Clutch] AI Personality: CONSERVATIVE (Weight={weight:F0}, Finesse={finesse:F0})");
            }
            else
            {
                personality = AIPersonality.Balanced;
                Debug.Log($"[Clutch] AI Personality: BALANCED (Weight={weight:F0}, Finesse={finesse:F0})");
            }
        }
        
        public bool IsClutchSituation(float pressure)
        {
            return pressure >= 60f;
        }
    }
    
    /// <summary>
    /// Detects patterns in opponent's play and suggests counter-strategies
    /// </summary>
    public class SimpleCounterStrategy
    {
        private System.Collections.Generic.Queue<ShotRecord> recentOpponentShots = new System.Collections.Generic.Queue<ShotRecord>();
        private const int TRACKING_WINDOW = 5;
        
        public class ShotRecord
        {
            public ShotIntent intent;
            public string shotType;
            public Vector2 targetPosition;
            public bool wasSuccessful;
            public int rockNumber;
        }
        
        public enum DetectedStrategy
        {
            Unknown,
            BuildingPosition,
            ProtectingWithGuards,
            AggressiveClearing,
            Mixed
        }
        
        private DetectedStrategy currentStrategy = DetectedStrategy.Unknown;
        
        public void RecordOpponentShot(ShotIntent intent, string shotType, Vector2 targetPos, bool success, int rockNumber)
        {
            ShotRecord record = new ShotRecord
            {
                intent = intent,
                shotType = shotType,
                targetPosition = targetPos,
                wasSuccessful = success,
                rockNumber = rockNumber
            };
            
            recentOpponentShots.Enqueue(record);
            
            while (recentOpponentShots.Count > TRACKING_WINDOW)
            {
                recentOpponentShots.Dequeue();
            }
            
            Debug.Log($"[Counter] Recorded opponent shot: {shotType} ({intent}) - Success: {success}");
            
            AnalyzePattern();
        }
        
        private void AnalyzePattern()
        {
            if (recentOpponentShots.Count < 3)
            {
                currentStrategy = DetectedStrategy.Unknown;
                return;
            }
            
            int drawCount = 0;
            int guardCount = 0;
            int takeoutCount = 0;
            
            foreach (var shot in recentOpponentShots)
            {
                if (shot.intent == ShotIntent.ScorePoints)
                    drawCount++;
                else if (shot.intent == ShotIntent.CreateOpportunity)
                    guardCount++;
                else if (shot.intent == ShotIntent.RemoveThreat)
                    takeoutCount++;
            }
            
            Debug.Log($"[Counter] Pattern Analysis: {drawCount} draws, {guardCount} guards, {takeoutCount} takeouts (last {recentOpponentShots.Count} shots)");
            
            int total = recentOpponentShots.Count;
            
            if (drawCount >= total * 0.6f)
            {
                currentStrategy = DetectedStrategy.BuildingPosition;
                Debug.Log("[Counter] ?? PATTERN DETECTED: Opponent is BUILDING POSITION (multiple draws)");
            }
            else if (guardCount >= total * 0.6f)
            {
                currentStrategy = DetectedStrategy.ProtectingWithGuards;
                Debug.Log("[Counter] ?? PATTERN DETECTED: Opponent is PROTECTING WITH GUARDS");
            }
            else if (takeoutCount >= total * 0.6f)
            {
                currentStrategy = DetectedStrategy.AggressiveClearing;
                Debug.Log("[Counter] ?? PATTERN DETECTED: Opponent is AGGRESSIVELY CLEARING");
            }
            else
            {
                currentStrategy = DetectedStrategy.Mixed;
                Debug.Log("[Counter] Pattern is MIXED - no clear strategy");
            }
        }
        
        public ShotIntent GetCounterIntent(DetectedStrategy strategy, ShotIntent defaultIntent)
        {
            switch (strategy)
            {
                case DetectedStrategy.BuildingPosition:
                    Debug.Log("[Counter] COUNTER-STRATEGY: Opponent building position ? REMOVE THREATS");
                    return ShotIntent.RemoveThreat;
                    
                case DetectedStrategy.ProtectingWithGuards:
                    Debug.Log("[Counter] COUNTER-STRATEGY: Opponent protecting ? PEEL GUARDS or DRAW AROUND");
                    return ShotIntent.RemoveThreat;
                    
                case DetectedStrategy.AggressiveClearing:
                    Debug.Log("[Counter] COUNTER-STRATEGY: Opponent clearing ? CREATE PROTECTED POSITION");
                    return ShotIntent.CreateOpportunity;
                    
                case DetectedStrategy.Mixed:
                case DetectedStrategy.Unknown:
                default:
                    Debug.Log("[Counter] No clear pattern - using default intent");
                    return defaultIntent;
            }
        }
        
        public bool ShouldCounterStrategy(DetectedStrategy strategy)
        {
            return strategy != DetectedStrategy.Unknown && strategy != DetectedStrategy.Mixed;
        }
        
        public DetectedStrategy GetCurrentStrategy()
        {
            return currentStrategy;
        }
        
        public float GetOpponentSuccessRate()
        {
            if (recentOpponentShots.Count == 0)
                return 0.5f;
            
            int successCount = 0;
            foreach (var shot in recentOpponentShots)
            {
                if (shot.wasSuccessful) successCount++;
            }
            
            float successRate = (float)successCount / recentOpponentShots.Count;
            
            Debug.Log($"[Counter] Opponent success rate: {successRate:P0} ({successCount}/{recentOpponentShots.Count})");
            return successRate;
        }
        
        public void ResetTracking()
        {
            recentOpponentShots.Clear();
            currentStrategy = DetectedStrategy.Unknown;
            Debug.Log("[Counter] Tracking RESET for new end");
        }
    }
}


