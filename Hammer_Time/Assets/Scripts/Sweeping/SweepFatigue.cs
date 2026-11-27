using UnityEngine;

/// <summary>
/// How long a sweeper lasts. Times are shares of one "run" (a button draw sliding from the near hog to rest) at the current game pace,
/// so Arcade, Pro and Real-time all tire a sweeper at the same point of the shot. The weakest sweeper (endurance 0) can sweep flat out
/// for half a run, the strongest (100) for the whole run. A hard sweep costs double. Resting wins the energy back.
/// </summary>
public static class SweepFatigue
{
    public const float Full = 100f;
    public const float WorstSweepShare = 0.5f, BestSweepShare = 1f;   // of a run, empty from full
    public const float WorstRestShare = 0.5f, BestRestShare = 0.25f;  // of a run, full from empty
    public const float HardCost = 2f;
    /// <summary>A worn-out sweeper has to win this much back before sweeping again.</summary>
    public const float ReadyAgain = 20f;
    const float ButtonY = 6.5f;

    /// <summary>Seconds a button draw slides from the near hog to rest.</summary>
    public static float RunSeconds(RockPhysicsProfile p)
    {
        if (p.IsLegacy) return 19f;   // the old rock's creep; not offered to players
        float speed = p.SpeedForStopDistance(ButtonY - RockPhysics.ReleaseY);
        return Mathf.Pow(speed, 1f - p.stopPower) / (p.stopStrength * (1f - p.stopPower));
    }

    /// <summary>How long a rested sweeper can keep going.</summary>
    public static float SweepSeconds(float endurance, bool hard, float runSeconds)
    {
        float share = Mathf.Lerp(WorstSweepShare, BestSweepShare, endurance / 100f);
        return runSeconds * share / (hard ? HardCost : 1f);
    }

    /// <summary>How long an empty sweeper takes to be fresh again.</summary>
    public static float RestSeconds(float endurance, float runSeconds)
    {
        return runSeconds * Mathf.Lerp(WorstRestShare, BestRestShare, endurance / 100f);
    }
}
