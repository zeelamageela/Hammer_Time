using UnityEngine;

/// <summary>
/// Turns the game's times into real-ice seconds, so a shot reads the same on every Game Pace: a button draw is a 3.80 split and
/// 14.50 hog to hog whether the rock on screen takes two seconds or twenty. Only the numbers shown change; the physics does not.
///
/// A measured time says how far that rock was going to travel on the current pace. The reading is the time real ice (constant
/// friction, the Real-time profile) gives for the same distance, scaled so the button draw lands on the figures below. Sweeping and
/// bumps still show, as a rock that was effectively heavier or lighter. On faster ice the same shot reads longer on both clocks, as
/// on real ice (see IceConditions).
/// </summary>
public static class IceClock
{
    /// <summary>What a button draw reads. Change these to recalibrate the clocks; the other weights follow.</summary>
    public const float ButtonSplit = 3.8f;
    public const float ButtonHogToHog = 14.5f;

    const float ButtonWeight = 8.5f;
    const float RunToFarHog = SplitTimer.FarHogY - RockPhysics.ReleaseY;   // friction starts at the release point
    const float FreeToRelease = RockPhysics.ReleaseY - SplitTimer.NearHogY;
    const float LongestDistance = 200f;   // further than any throw goes

    static RockPhysicsProfile realIce;
    static RockPhysicsProfile RealIce { get { return realIce ?? (realIce = RockPhysicsProfile.Real()); } }

    // on real ice, the time to reach a given spot grows with the square root of the ice's speed
    static float IceStretch(RockPhysicsProfile p) { return Mathf.Sqrt(p.iceSpeed); }

    static float ButtonDistance(RockPhysicsProfile p) { return p.stopDistanceSlope * ButtonWeight + p.stopDistanceOffset; }

    /// <summary>The split to show for one measured (or expected) on pace p. Negative (no time) stays as it is.</summary>
    public static float Split(float seconds, RockPhysicsProfile p)
    {
        if (seconds <= 0f || p.IsLegacy) return seconds;
        float speed = (SplitTimer.NearHogY - SplitTimer.BackLineY) / seconds;
        // the split is run at one speed, and on real ice speed goes with the square root of the distance the rock will travel
        return ButtonSplit * IceStretch(p) * Mathf.Sqrt(ButtonDistance(p) / p.StopDistance(speed));
    }

    /// <summary>The hog-to-hog time to show for one measured (or expected) on pace p. Negative (no time) stays as it is.</summary>
    public static float HogToHog(float seconds, RockPhysicsProfile p)
    {
        if (seconds <= 0f || p.IsLegacy) return seconds;

        // how far was a rock with this time going? (a longer distance is a faster rock and a shorter time)
        float near = RunToFarHog, far = LongestDistance;
        for (int i = 0; i < 40; i++)
        {
            float middle = 0.5f * (near + far);
            if (HogTime(middle, p) > seconds) near = middle; else far = middle;
        }
        return ButtonHogToHog * IceStretch(p) * HogTime(0.5f * (near + far), RealIce) / HogTime(ButtonDistance(RealIce), RealIce);
    }

    /// <summary>How fast the shown clock runs against the game's while a rock is between the lines, from its speed at the first.</summary>
    public static float SplitRate(float speed, RockPhysicsProfile p)
    {
        if (speed <= 0.001f) return 1f;
        float expected = (SplitTimer.NearHogY - SplitTimer.BackLineY) / speed;
        return Split(expected, p) / expected;
    }

    public static float HogRate(float speed, RockPhysicsProfile p)
    {
        if (speed <= 0.001f || p.IsLegacy) return 1f;
        // a rock that will stop short counts at the rate of one that only just gets there
        float expected = HogTime(Mathf.Max(RunToFarHog, p.StopDistance(speed)), p);
        return HogToHog(expected, p) / expected;
    }

    public static TimingPredictor.Range Split(TimingPredictor.Range r, RockPhysicsProfile p)
    {
        return new TimingPredictor.Range { fast = Split(r.fast, p), slow = Split(r.slow, p) };
    }

    public static TimingPredictor.Range HogToHog(TimingPredictor.Range r, RockPhysicsProfile p)
    {
        return new TimingPredictor.Range { fast = HogToHog(r.fast, p), slow = HogToHog(r.slow, p) };
    }

    /// <summary>
    /// Hog-to-hog time on pace p for a rock that will travel this far after the release point (at least to the far hog line):
    /// a short stretch at its release speed, then dv/dt = -k v^power as far as the far hog.
    /// </summary>
    static float HogTime(float distance, RockPhysicsProfile p)
    {
        float release = p.SpeedForStopDistance(distance);
        float left = distance - RunToFarHog;
        float atFarHog = left > 0.01f ? p.SpeedForStopDistance(left) : 0f;
        float power = 1f - p.stopPower;
        return FreeToRelease / release + (Mathf.Pow(release, power) - Mathf.Pow(atFarHog, power)) / (p.stopStrength * power);
    }
}
