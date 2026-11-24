using System;

/// <summary>The five training tracks, in the order they appear in the Training menu.</summary>
public enum TrainingTrack
{
    Sweep = 0,
    Directional = 1,
    Tactics = 2,
    OnIce = 3,
    Cross = 4,
}

/// <summary>One purchasable level of a track: what it is called, what it costs and what it gives.</summary>
public class TrainingLevel
{
    public string name;
    public string effect;
    public float cost;
    /// <summary>Stat bonus this level adds, in the same units and order as equipment (see CareerStats).</summary>
    public int weight, finesse, aim, sweepStrength, sweepEndurance, sweepCohesion;
}

/// <summary>
/// The Training menu's content: five tracks of four levels each, one fixed price per level (the same for every track).
/// Pure data and arithmetic so it can be tested; TrainingManager shows it and CareerManager.trainingLevels remembers what the player owns.
/// Levels are cumulative: owning level 3 means owning 1, 2 and 3.
/// </summary>
public static class TrainingCatalog
{
    public const int Tracks = 5;
    public const int MaxLevel = 4;

    /// <summary>What each level costs, for every track. Level 1 is cheap, level 4 is a real investment.</summary>
    public static readonly float[] LevelCost = { 1000f, 2500f, 5000f, 10000f };

    public static string TrackName(TrainingTrack t)
    {
        switch (t)
        {
            case TrainingTrack.Sweep: return "Sweep Training";
            case TrainingTrack.Directional: return "Directional Sweeping";
            case TrainingTrack.Tactics: return "Tactic Studies";
            case TrainingTrack.OnIce: return "On-Ice Training";
            default: return "Cross Training";
        }
    }

    public static string TrackBlurb(TrainingTrack t)
    {
        switch (t)
        {
            case TrainingTrack.Sweep: return "Stronger, fitter, more in sync sweepers.";
            case TrainingTrack.Directional: return "Carve the rock by tapping one sweeper.";
            case TrainingTrack.Tactics: return "Read what a hit will do.";
            case TrainingTrack.OnIce: return "See where the rock is going to finish.";
            default: return "Off-ice work that makes the whole team better.";
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
        // SWEEP TRAINING: raises the sweep stats, a different one at each step
        new[]
        {
            L("Sweep Drills",       "+4 sweep endurance",                 1, endurance: 4),
            L("Weight Training",    "+5 sweep strength",                  2, strength: 5),
            L("Synchro Sweeping",   "+5 sweep cohesion",                  3, cohesion: 5),
            L("Elite Sweep Camp",   "+4 endurance, strength and cohesion", 4, strength: 4, endurance: 4, cohesion: 4),
        },
        // DIRECTIONAL SWEEPING: unlocks carving with one sweeper; the level is how hard it pushes (DirectionalStrength)
        new[]
        {
            L("Carving Basics",     "Unlocks carving: tap one sweeper",                      1),
            L("Edge Control",       "Carving pushes harder",                                2),
            L("Quick Hands",        "Carving pushes harder again and reacts faster",        3),
            L("Master Sweeper",     "Full-strength carving",                                4),
        },
        // TACTIC STUDIES: the collision indicator, sharper each level (CollisionIndicatorError)
        new[]
        {
            L("Film Room",          "Collision direction indicator (rough)",                1),
            L("Shot Charts",        "Collision indicator within about 12 degrees",          2),
            L("Angle Study",        "Collision indicator within about 6 degrees",           3),
            L("Hit Analytics",      "Exact collision indicator and struck-rock path",       4),
        },
        // ON-ICE TRAINING: the aim circle, tighter each level (AimCircleDiameter)
        new[]
        {
            L("Target Practice",    "Aim circle, 1.5 wide",                                 1),
            L("Release Drills",     "Aim circle, 1.1 wide",                                 2),
            L("Weight Control",     "Aim circle, 0.75 wide",                                3, weight: 3),
            L("Pinpoint",           "Aim circle, 0.5 wide",                                 4, weight: 3, aim: 3),
        },
        // CROSS TRAINING: off-ice activities; each helps a different part of the game
        new[]
        {
            L("Jogging",            "+4 sweep endurance",                                   1, endurance: 4),
            L("Yoga",               "+3 sweep strength, +3 sweep endurance",                2, strength: 3, endurance: 3),
            L("Rowing",             "+4 sweep strength, +4 sweep cohesion",                 3, strength: 4, cohesion: 4),
            L("Pickleball",         "+4 aim, +4 finesse",                                   4, aim: 4, finesse: 4),
        },
    };

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

    // ---- what the UI-unlocking tracks give (0 = not unlocked) ------------------------------------------------------------

    static readonly float[] DirectionalStrengths = { 0f, 0.5f, 0.75f, 1f, 1.25f };
    static readonly float[] CollisionErrors = { -1f, 20f, 12f, 6f, 0f };
    static readonly float[] AimCircles = { 0f, 1.5f, 1.1f, 0.75f, 0.5f };

    /// <summary>Scale of the sideways push a one-sweeper tap gives (0 = carving locked).</summary>
    public static float DirectionalStrength(int[] owned) { return DirectionalStrengths[Owned(owned, TrainingTrack.Directional)]; }

    /// <summary>How far, in degrees, the shown collision direction may be off (negative = no indicator yet, 0 = exact).</summary>
    public static float CollisionIndicatorError(int[] owned) { return CollisionErrors[Owned(owned, TrainingTrack.Tactics)]; }

    /// <summary>Diameter of the aim circle in world units (0 = no aim circle yet).</summary>
    public static float AimCircleDiameter(int[] owned) { return AimCircles[Owned(owned, TrainingTrack.OnIce)]; }
}
