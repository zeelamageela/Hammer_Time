using UnityEngine;

/// <summary>
/// Skill-based fuzz for the player's aim line. The rock itself always goes exactly where the physics says (the preview and the rock
/// share RockPhysics); what skill changes is how far the DRAWN line can be trusted. Each throw gets one error, rolled once, so
/// re-aiming cannot re-roll it: the line shown is that of a slightly different throw (see BendThrow), one that ends (sideways,
/// long/short) metres off. It is a real throw, so what it is drawn hitting and where the stones go all agree with the line.
/// A shooter with skill 100 gets an exact line.
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
    /// <summary>The roll is cut off at this many sigmas (at consistency 50) so a freak roll cannot send the line somewhere silly.</summary>
    public const float MaxSigmas = 2f;
    /// <summary>How much consistency moves the cut-off: this share smaller at 100, this share bigger at 0.</summary>
    public const float ConsistencySwing = 0.25f;

    /// <summary>
    /// The worst miss, in sigmas, for a shooter of this consistency (the stat saved as finesse). It sets both the largest error a
    /// throw can have and the size of the aim oval; weight and aim set how big a typical miss is.
    /// </summary>
    public static float MaxSigmasFor(float consistency)
    {
        return MaxSigmas * (1f - ConsistencySwing * (Mathf.Clamp(consistency, 0f, 100f) - 50f) / 50f);
    }

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
    public static Vector2 Roll(float aimSkill, float weightSkill, System.Random rng, float consistency = 50f)
    {
        float cut = MaxSigmasFor(consistency);
        return new Vector2(Gaussian(rng, AimSigma(aimSkill), cut), Gaussian(rng, WeightSigma(weightSkill), cut));
    }

    static float Gaussian(System.Random rng, float sigma, float cut)
    {
        if (sigma <= 0f) return 0f;
        double u1 = 1.0 - rng.NextDouble();
        double u2 = rng.NextDouble();
        float z = (float)(System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Cos(2.0 * System.Math.PI * u2));
        return Mathf.Clamp(z, -cut, cut) * sigma;
    }

    /// <summary>
    /// The launch velocity (profile units) of the throw whose stop, on clear ice, is `error` metres from where `launchVelocity` would
    /// stop: x sideways, y long (+) / short (-). The speed sets the length and the angle the sideways part; both are refined a few
    /// times because a different weight also curls differently.
    /// </summary>
    public static Vector2 BendThrow(Vector2 launcher, Vector2 launchVelocity, bool inTurn, Vector2 error, RockPhysicsProfile p)
    {
        if (error == Vector2.zero || launchVelocity.y <= 0.001f) return launchVelocity;
        Vector2 target = StopPoint(launcher, launchVelocity, inTurn, p) + error;
        Vector2 bent = launchVelocity;
        for (int i = 0; i < 6; i++)
        {
            Vector2 end = StopPoint(launcher, bent, inTurn, p);
            Vector2 miss = target - end;
            if (miss.sqrMagnitude < 0.01f * 0.01f) break;
            float speed = p.SpeedForStopDistance(p.StopDistance(bent.magnitude) + miss.y);
            Vector2 direction = bent.normalized;
            direction.x += miss.x / Mathf.Max(1f, end.y - launcher.y);
            bent = direction.normalized * speed;
        }
        return bent;
    }

    /// <summary>Where a throw from the hack comes to rest on clear ice.</summary>
    public static Vector2 StopPoint(Vector2 launcher, Vector2 launchVelocity, bool inTurn, RockPhysicsProfile p)
    {
        const float dt = 0.02f;
        RockState rock = RockPhysics.Launch(launcher, launchVelocity, p);
        for (float t = 0f; t < 30f * p.timeStretch && !rock.asleep; t += dt)
        {
            RockPhysics.Step(ref rock, p, dt);
            if (rock.freeFlight && rock.position.y > RockPhysics.ReleaseY) RockPhysics.Release(ref rock, p, inTurn);
        }
        return rock.position;
    }
}
