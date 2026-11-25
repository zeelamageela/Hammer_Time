using UnityEngine;

/// <summary>
/// Skill-based fuzz for the player's aim line. The rock itself always goes exactly where the physics says (the preview and the rock
/// share RockPhysics); what skill changes is how far the DRAWN line can be trusted. Each throw gets one error, rolled once, so
/// re-aiming cannot re-roll it: the line's end is off by (sideways, long/short) metres, and the whole line bends to match -
/// nothing at the hack, the full error at the end. A shooter with skill 100 gets an exact line.
///
/// The error sizes started as the AI draw-shot spreads (AI_Target: 15 cm sideways, 90 cm in weight); sideways is now 50 cm at skill 0 (so the oval is wide enough to read). Both shrink with the
/// square of the missing skill, read as metres at the end of the line, so a skill point means the same thing for both sides.
/// </summary>
public static class AimFuzz
{
    /// <summary>Master switch (tester option; the tutorial is always exact).</summary>
    public static bool Enabled = true;

    public const float AimSigmaAtZeroSkill = 0.5f;
    public const float WeightSigmaAtZeroSkill = 0.9f;
    /// <summary>The roll is cut off at this many sigmas so a freak roll cannot send the line somewhere silly.</summary>
    public const float MaxSigmas = 2f;

    public static float AimSigma(float aimSkill)
    {
        float miss = 1f - Mathf.Clamp01(aimSkill / 100f);
        return AimSigmaAtZeroSkill * miss * miss;
    }

    public static float WeightSigma(float weightSkill)
    {
        float miss = 1f - Mathf.Clamp01(weightSkill / 100f);
        return WeightSigmaAtZeroSkill * miss * miss;
    }

    /// <summary>One throw's error: x = sideways, y = long (+) / short (-), in metres at the end of the line.</summary>
    public static Vector2 Roll(float aimSkill, float weightSkill, System.Random rng)
    {
        return new Vector2(Gaussian(rng, AimSigma(aimSkill)), Gaussian(rng, WeightSigma(weightSkill)));
    }

    static float Gaussian(System.Random rng, float sigma)
    {
        if (sigma <= 0f) return 0f;
        double u1 = 1.0 - rng.NextDouble();
        double u2 = rng.NextDouble();
        float z = (float)(System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Cos(2.0 * System.Math.PI * u2));
        return Mathf.Clamp(z, -MaxSigmas, MaxSigmas) * sigma;
    }

    /// <summary>
    /// Bends a point of the drawn line by the error: sideways grows from nothing at the launcher to the full amount at the ideal end,
    /// and the line is stretched (or squeezed) along its length so its end lands long or short by the weight error.
    /// </summary>
    public static Vector2 Warp(Vector2 p, Vector2 launcher, Vector2 idealEnd, Vector2 error)
    {
        float length = idealEnd.y - launcher.y;
        if (length < 0.5f) return p;
        float along = Mathf.Clamp01((p.y - launcher.y) / length);
        float stretch = Mathf.Max(0.1f, 1f + error.y / length);
        return new Vector2(p.x + error.x * along, launcher.y + (p.y - launcher.y) * stretch);
    }
}
