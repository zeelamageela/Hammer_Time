using UnityEngine;

/// <summary>
/// The computer team's read of the ice: what it plans its shots on, while its rocks run on the real ice. It starts where the player
/// starts, at the average for the kind of venue, and closes on the truth with every rock thrown in the game (its own count fully,
/// the other side's half), faster for a more skilled team. A weaker team never gets it quite right. Each game starts again.
///
/// Worked out from how far the game has got, so there is nothing to save and a reload reads the same.
/// A tester option for now (pause menu: AI Reads Ice); off, the computer knows the ice exactly.
/// </summary>
public static class AiRead
{
    const string Key = "AiReadsIce";

    // how much of what is left to learn one rock teaches, at skill 0 and 100
    public const float SlowLearning = 0.06f, QuickLearning = 0.30f;
    // how wrong a team with no skill stays, at worst
    public const float StubbornSpeed = 0.03f, StubbornCurl = 0.08f;

    public static bool Enabled
    {
        get { try { return PlayerPrefs.GetInt(Key, 0) == 1; } catch { return false; } }
        set { try { PlayerPrefs.SetInt(Key, value ? 1 : 0); PlayerPrefs.Save(); } catch { } }
    }

    public static bool Active { get { return Enabled && IceConditions.Allowed; } }

    /// <summary>
    /// One number of a read (speed or curl). start: where reads begin; truth: the ice; skill: 0..1; rocksSeen: rocks watched so far;
    /// habit: this team's own lean, -1..1; stubborn: the most a skill-less team stays out by.
    /// </summary>
    public static float Read(float start, float truth, float skill, float rocksSeen, float habit, float stubborn)
    {
        skill = Mathf.Clamp01(skill);
        float left = Mathf.Pow(1f - Mathf.Lerp(SlowLearning, QuickLearning, skill), Mathf.Max(0f, rocksSeen));
        return truth + (start - truth) * left + habit * stubborn * (1f - skill);
    }

    /// <summary>The ice the team throwing this rock plans on, or null for the real ice. skill is the shooter's, 0..100.</summary>
    public static RockPhysicsProfile For(GameManager gm, int rockCurrent, float skill)
    {
        if (!Active || gm == null || rockCurrent < 0 || rockCurrent >= gm.rockList.Count) return null;

        IceConditions.InForce(out float speed, out float curl);
        float startSpeed = 1f, startCurl = 1f;
        if (VenueIce.Enabled)
        {
            VenueIce.Identify(out int background, out _, out _, out _);
            VenueIce.KindIce(background, out startSpeed, out startCurl);
        }

        // rocks thrown before this one (ends count from 0; a short game starts each end part way along the 16): each team has
        // thrown half of them
        int perEnd = gm.rocksPerTeam * 2;
        int thrown = Mathf.Max(0, gm.endCurrent) * perEnd + Mathf.Max(0, rockCurrent - (16 - perEnd));
        float seen = thrown * 0.5f * (1f + 0.5f);

        string team = gm.rockList[rockCurrent].rockInfo.teamName ?? "";
        int lean = 17;
        foreach (char c in team) lean = unchecked(lean * 31 + c);
        var habit = new System.Random(lean);
        float speedHabit = (float)(habit.NextDouble() * 2.0 - 1.0), curlHabit = (float)(habit.NextDouble() * 2.0 - 1.0);

        float readSpeed = Read(startSpeed, speed, skill / 100f, seen, speedHabit, StubbornSpeed);
        float readCurl = Read(startCurl, curl, skill / 100f, seen, curlHabit, StubbornCurl);
        GameLog.Info($"[AiRead] {team} (skill {skill:F0}) after {thrown} rocks reads {IceUnits.Describe(readSpeed, readCurl)}; "
            + $"the ice is {IceUnits.Describe(speed, curl)}");

        return RockPhysicsBootstrap.MakeOnNormalIce(RockPhysicsBootstrap.Current).WithIce(readSpeed, readCurl);
    }
}
