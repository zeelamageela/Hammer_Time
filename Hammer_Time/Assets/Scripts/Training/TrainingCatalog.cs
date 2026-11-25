using System;

/// <summary>The three training tracks, in the order they appear in the Training menu.</summary>
public enum TrainingTrack
{
    Sweep = 0,
    Tactics = 1,
    OnIce = 2,
}

/// <summary>One purchasable level of a track: what it is called, what it costs and what it gives.</summary>
public class TrainingLevel
{
    public string name;
    public string effect;
    public float cost;
    /// <summary>The player (XP) level needed before this can be bought; 1 = no gate.</summary>
    public int requiredLevel = 1;
    /// <summary>Stat bonus this level adds, in the same units and order as equipment (see CareerStats).</summary>
    public int weight, finesse, aim, sweepStrength, sweepEndurance, sweepCohesion;
}

/// <summary>
/// The Training menu's content: three tracks of four levels each, one fixed price per level (the same for every track).
/// Pure data and arithmetic so it can be tested; TrainingManager shows it and CareerManager.trainingLevels remembers what the player owns.
/// Levels are cumulative: owning level 3 means owning 1, 2 and 3.
/// </summary>
public static class TrainingCatalog
{
    public const int Tracks = 3;
    public const int MaxLevel = 4;

    /// <summary>What each level costs, for every track. Level 1 is cheap, level 4 is a real investment.</summary>
    public static readonly float[] LevelCost = { 1000f, 2500f, 5000f, 10000f };

    public static string TrackName(TrainingTrack t)
    {
        switch (t)
        {
            case TrainingTrack.Sweep: return "Sweep Training";
            case TrainingTrack.Tactics: return "Tactic Studies";
            default: return "On-Ice Training";
        }
    }

    public static string TrackBlurb(TrainingTrack t)
    {
        switch (t)
        {
            case TrainingTrack.Sweep: return "Carve the rock, and stronger, fitter, more in sync sweepers.";
            case TrainingTrack.Tactics: return "Read what a hit will do.";
            default: return "See where the rock is going to finish, and how long it will take.";
        }
    }

    static TrainingLevel L(string name, string effect, int level, int weight = 0, int finesse = 0, int aim = 0, int strength = 0, int endurance = 0, int cohesion = 0)
    {
        return new TrainingLevel
        {
            name = name, effect = effect, cost = LevelCost[level - 1],
            weight = weight, finesse = finesse, aim = aim, sweepStrength = strength, sweepEndurance = endurance, sweepCohesion = cohesion,
        };
    }

    static readonly TrainingLevel[][] Levels =
    {
        // SWEEP TRAINING: level 1 unlocks carving with one sweeper, then the sweep stats
        new[]
        {
            L("Directional Sweeping", "Unlocks carving: tap one sweeper",   1),
            L("Sweep Drills",       "+4 sweep endurance",                   2, endurance: 4),
            L("Weight Training",    "+5 sweep strength",                    3, strength: 5),
            L("Synchro Sweeping",   "+5 sweep cohesion",                    4, cohesion: 5),
        },
        // TACTIC STUDIES: what a hit will do. Level 1 = pulsing crosshairs (the old aim circle), 2 = collision warning line,
        // 3 = hit arrows (within CollisionErrors degrees), 4 = exact hit arrows
        new[]
        {
            L("Film Room",          "Pulsing crosshairs on the aim point",              1),
            L("Shot Charts",        "Collision warning line",                           2),
            L("Angle Study",        "Hit arrows, within about 12 degrees",              3),
            L("Hit Analytics",      "Exact hit arrows",                                 4),
        },
        // ON-ICE TRAINING: how far the trajectory line reaches (LineReachY), the timing ranges, a little throwing skill
        new[]
        {
            L("Target Practice",    "Cycling dots; the line reaches the far hog line",  1),
            L("Release Drills",     "Split and hog-to-hog timing ranges",               2),
            L("Weight Control",     "The line reaches halfway to the house, +3 weight", 3, weight: 3),
            L("Pinpoint",           "The full line, +3 weight, +3 aim",                 4, weight: 3, aim: 3),
        },
    };

    /// <summary>
    /// Player level needed to buy the FIRST level of each track (you learn the basics as you progress); the rest of a track is cash only.
    /// </summary>
    public const int FirstLevelGate = 2;

    static TrainingCatalog()
    {
        for (int t = 0; t < Tracks; t++)
            Levels[t][0].requiredLevel = FirstLevelGate;
    }

    /// <summary>True if a player of this level may buy the next level of the track (cash is checked separately).</summary>
    public static bool LevelAllows(int[] owned, TrainingTrack t, int playerLevel)
    {
        var next = Next(owned, t);
        return next != null && playerLevel >= next.requiredLevel;
    }

    /// <summary>Level 1..MaxLevel of a track (levelNumber is 1-based). Null if out of range.</summary>
    public static TrainingLevel Level(TrainingTrack t, int levelNumber)
    {
        int ti = (int)t;
        if (ti < 0 || ti >= Tracks || levelNumber < 1 || levelNumber > MaxLevel) return null;
        return Levels[ti][levelNumber - 1];
    }

    /// <summary>How many levels of this track the player owns (0 to MaxLevel); a short, missing or out-of-range save counts as 0.</summary>
    public static int Owned(int[] owned, TrainingTrack t)
    {
        int ti = (int)t;
        if (owned == null || ti < 0 || ti >= owned.Length) return 0;
        return Math.Max(0, Math.Min(MaxLevel, owned[ti]));
    }

    /// <summary>The next level that can be bought, or null when the track is finished.</summary>
    public static TrainingLevel Next(int[] owned, TrainingTrack t)
    {
        return Level(t, Owned(owned, t) + 1);
    }

    /// <summary>Raises a track by one level, returning the array to keep (a new one if the old was null or too short).</summary>
    public static int[] WithLevelBought(int[] owned, TrainingTrack t)
    {
        var result = new int[Tracks];
        if (owned != null) Array.Copy(owned, result, Math.Min(owned.Length, Tracks));
        int ti = (int)t;
        result[ti] = Math.Min(MaxLevel, Owned(result, t) + 1);
        return result;
    }

    /// <summary>All the stat bonuses the owned levels give together.</summary>
    public static CareerStats StatBonus(int[] owned)
    {
        var sum = new CareerStats();
        for (int t = 0; t < Tracks; t++)
        {
            int have = Owned(owned, (TrainingTrack)t);
            for (int l = 1; l <= have; l++)
            {
                var lv = Level((TrainingTrack)t, l);
                sum.weightAccuracy += lv.weight;
                sum.finesseAccuracy += lv.finesse;
                sum.aimAccuracy += lv.aim;
                sum.sweepStrength += lv.sweepStrength;
                sum.sweepEndurance += lv.sweepEndurance;
                sum.sweepCohesion += lv.sweepCohesion;
            }
        }
        return sum;
    }

    // ---- what the tracks unlock (levels are 1-4; 0 = nothing yet) ----------------------------------------------------------

    static readonly float[] CollisionErrors = { -1f, -1f, -1f, 12f, 0f };
    /// <summary>Where the trajectory line stops, as a y on the sheet (hack -25, far hog 0, tee 6.5), by On-Ice level 0-4. Level 0 is the near hog.</summary>
    static readonly float[] LineReaches = { -16.15f, 0f, 0f, 3.25f, float.PositiveInfinity };

    /// <summary>Carving with one sweeper is unlocked by Sweep Training level 1; its strength follows the sweep strength stat.</summary>
    public static bool CarvingUnlocked(int[] owned) { return Owned(owned, TrainingTrack.Sweep) >= 1; }

    /// <summary>The pulsing aim crosshairs come with Tactic Studies level 1.</summary>
    public static bool CrosshairsUnlocked(int[] owned) { return Owned(owned, TrainingTrack.Tactics) >= 1; }

    /// <summary>The red collision warning line comes with Tactic Studies level 2.</summary>
    public static bool CollisionWarningUnlocked(int[] owned) { return Owned(owned, TrainingTrack.Tactics) >= 2; }

    /// <summary>How far, in degrees, the shown hit arrows may be off (negative = no arrows yet, 0 = exact).</summary>
    public static float CollisionIndicatorError(int[] owned) { return CollisionErrors[Owned(owned, TrainingTrack.Tactics)]; }

    /// <summary>The cycling dots on the trajectory line come with On-Ice Training level 1.</summary>
    public static bool DotsUnlocked(int[] owned) { return Owned(owned, TrainingTrack.OnIce) >= 1; }

    /// <summary>The split and hog-to-hog timing ranges come with On-Ice Training level 2.</summary>
    public static bool TimingUnlocked(int[] owned) { return Owned(owned, TrainingTrack.OnIce) >= 2; }

    /// <summary>The y the trajectory line is drawn up to (infinity = the full line).</summary>
    public static float LineReachY(int[] owned) { return LineReaches[Owned(owned, TrainingTrack.OnIce)]; }

    /// <summary>
    /// Turns a saved array from the five-track version (sweep, directional, tactics, on-ice, cross) into the current three.
    /// Directional carving became level 1 of Sweep Training, so the old sweep levels sit one higher; the tactic and on-ice levels carry over.
    /// </summary>
    public static int[] MigrateLegacy(int[] saved)
    {
        if (saved == null || saved.Length != 5) return saved;
        int sweep = saved[1] > 0 ? Math.Min(MaxLevel, 1 + Math.Max(0, saved[0])) : 0;
        return new[] { sweep, Math.Max(0, saved[2]), Math.Max(0, saved[3]) };
    }
}
