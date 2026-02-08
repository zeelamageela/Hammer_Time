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
    StealPoint,             // Aggressive draw when without hammer
    
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
    
    // SIMPLE FALLBACKS
    DrawToButton,          // Simple center draw
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
    
    public ShotContext(ShotIntent intent, int targetRock = -1)
    {
        this.intent = intent;
        this.targetRockIndex = targetRock;
        this.secondaryTargetIndex = -1;
        this.idealFinalPosition = new Vector2(0f, 6.5f); // Default to button
        this.aggressiveness = 0.5f; // Medium
        this.acceptRisk = false;
    }
}
