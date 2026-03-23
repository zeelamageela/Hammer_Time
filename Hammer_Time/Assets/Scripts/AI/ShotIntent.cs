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

