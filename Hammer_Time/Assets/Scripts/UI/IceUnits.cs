using UnityEngine;

/// <summary>
/// Ice as curlers say it: WEIGHT, the hog-to-hog seconds of a button draw (higher = faster ice), and CURL, the feet that draw
/// moves sideways. These are only names for the speed and curl numbers of IceConditions.
/// </summary>
public static class IceUnits
{
    /// <summary>Feet of curl on a button draw on normal ice.</summary>
    public const float NormalCurlFeet = 3.6f;

    /// <summary>The sheet's scale: the 12-foot ring is 3 units across.</summary>
    public const float FeetPerUnit = 4f;

    public const float MinSeconds = 12.5f, MaxSeconds = 16.2f, MinFeet = 1.8f, MaxFeet = 5.9f;

    public static float Seconds(float speed) { return IceClock.ButtonHogToHog * Mathf.Sqrt(speed); }

    public static float SpeedFromSeconds(float seconds)
    {
        float ratio = seconds / IceClock.ButtonHogToHog;
        return ratio * ratio;
    }

    public static float Feet(float curl) { return NormalCurlFeet * curl; }

    public static float CurlFromFeet(float feet) { return feet / NormalCurlFeet; }

    /// <summary>"14.8 s, 3.9 ft".</summary>
    public static string Describe(float speed, float curl) { return Seconds(speed).ToString("0.0") + " s, " + Feet(curl).ToString("0.0") + " ft"; }
}
