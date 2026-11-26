using UnityEngine;

/// <summary>
/// The split and hog-to-hog times to expect from the shot being aimed (shown by TimingPanel, On-Ice Training 2), run through the same
/// physics as the real rock. Like the aim oval they are ranges: they are centred on where the DRAWN line ends (which is off by this
/// throw's error) and are as wide as the biggest long/short error, so the true time is always inside and the middle gives nothing away.
/// Times assume nobody sweeps.
/// </summary>
public static class TimingPredictor
{
    const float Dt = 0.02f;   // Unity's physics step

    /// <summary>Fastest and slowest time in seconds. Negative = the rock never gets to the far hog line.</summary>
    public struct Range
    {
        public float fast, slow;
    }

    /// <summary>Seconds from the back line to the near hog line (the rock runs free, at one speed).</summary>
    public static float Split(Vector2 launchVelocity)
    {
        return launchVelocity.y <= 0.001f ? -1f : (SplitTimer.NearHogY - SplitTimer.BackLineY) / launchVelocity.y;
    }

    /// <summary>Seconds from the near hog line to the far hog line, or -1 if the rock stops short.</summary>
    public static float HogToHog(Vector2 launchVelocity, bool inTurn, RockPhysicsProfile p)
    {
        if (launchVelocity.y <= 0.001f) return -1f;
        // start on the near hog line: it flies free to the release point, then friction and curl take over
        RockState rock = RockPhysics.Launch(new Vector2(0f, SplitTimer.NearHogY), launchVelocity, p);
        float t = 0f, limit = 60f * p.timeStretch;
        while (t < limit && !rock.asleep)
        {
            float prevY = rock.position.y;
            RockPhysics.Step(ref rock, p, Dt);
            if (prevY < SplitTimer.FarHogY && rock.position.y >= SplitTimer.FarHogY)
                return SplitTimer.CrossingTime(prevY, t, rock.position.y, t + Dt, SplitTimer.FarHogY);
            if (rock.freeFlight && rock.position.y > RockPhysics.ReleaseY) RockPhysics.Release(ref rock, p, inTurn);
            t += Dt;
        }
        return -1f;
    }

    /// <summary>
    /// The ranges for an aim. launchVelocity is the true one (profile units); lengthError is how far long (+) or short (-) the drawn
    /// line ends, and halfRange the biggest long/short error this shooter can have.
    /// </summary>
    public static void Predict(Vector2 launchVelocity, bool inTurn, float lengthError, float halfRange, RockPhysicsProfile p,
        out Range split, out Range hogToHog)
    {
        float drawn = p.StopDistance(launchVelocity.magnitude) + lengthError;
        Vector2 direction = launchVelocity.normalized;
        Vector2 fastest = direction * p.SpeedForStopDistance(drawn + halfRange);
        Vector2 slowest = direction * p.SpeedForStopDistance(drawn - halfRange);
        split = new Range { fast = Split(fastest), slow = Split(slowest) };
        hogToHog = new Range { fast = HogToHog(fastest, inTurn, p), slow = HogToHog(slowest, inTurn, p) };
    }

    /// <summary>"03:42-03:60"; one value when the shooter is exact, "14:10+" when the slow end stops short, "--" when both do.</summary>
    public static string Format(Range r)
    {
        if (r.fast < 0f) return "--";
        if (r.slow < 0f) return SplitTimer.Format(r.fast) + "+";
        string a = SplitTimer.Format(r.fast), b = SplitTimer.Format(r.slow);
        return a == b ? a : a + "-" + b;
    }
}
