/// <summary>
/// Two switches for how the pull-back feels, so old and new can be compared (Quick Test has both).
/// </summary>
public static class PullbackSettings
{
    /// <summary>The original sideways limit of the pull-back, either side of centre.</summary>
    public const float ClassicAimCap = 0.34f;

    /// <summary>The small circle under the thumb. Off: the coloured pull line does the work.</summary>
    public static bool ShowThumbCircle = false;

    /// <summary>How far to either side the rock can be pulled and still be thrown.</summary>
    public static float AimCap = 0.40f;

    /// <summary>
    /// The rock's sideways position for a thumb at this world x. The same thumb travel always covers the whole range, so a wider cap
    /// needs no more reach (each bit of thumb movement just aims a little further).
    /// </summary>
    public static float XScale { get { return AimCap / ClassicAimCap / 16f; } }
}
