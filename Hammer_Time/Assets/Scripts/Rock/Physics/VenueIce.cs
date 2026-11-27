using UnityEngine;

/// <summary>Which background an event is played in: the order of BGManager's boards and ice, and Tourny.BG.</summary>
public enum VenueBackground { Club = 0, Rink = 1, Bleachers = 2, Stadium = 3, Outdoor = 4, Den = 5 }

/// <summary>
/// The ice of the place the game is in, as speed and curl against normal ice (see IceConditions). Three layers:
/// the kind of venue (outdoor ice is slow and straight, stadium ice fast and curly, the rest club ice), that event's own lasting
/// character (set once per career), and a small shift for this visit (the same for every game of the event, new next season).
/// Nothing is stored but the career's ice seed: the rest is worked out from it, so a reload finds the same sheet.
///
/// A tester option for now (pause menu: Venue Ice).
/// </summary>
public static class VenueIce
{
    const string Key = "VenueIce";

    // how far the event's character and the visit's shift can take speed and curl from the venue kind's
    public const float CharacterSpeed = 0.05f, CharacterCurl = 0.10f;
    public const float VisitSpeed = 0.02f, VisitCurl = 0.04f;

    public static bool Enabled
    {
        get { try { return PlayerPrefs.GetInt(Key, 0) == 1; } catch { return false; } }
        set
        {
            try { PlayerPrefs.SetInt(Key, value ? 1 : 0); PlayerPrefs.Save(); } catch { }
            IceConditions.Refresh();
        }
    }

    /// <summary>Quick Test: play in this background instead of the career's (-1 = no), as this visit.</summary>
    public static int TestBackground = -1;
    public static int TestVisit;

    /// <summary>The background the game is being played in.</summary>
    public static int Background(CareerManager career, GameSettingsPersist settings)
    {
        if (TestBackground >= 0) return TestBackground;
        if (career != null && career.currentTourny != null) return career.currentTourny.BG;
        return settings != null ? settings.bg : 0;
    }

    public static void KindIce(int background, out float speed, out float curl)
    {
        switch ((VenueBackground)background)
        {
            case VenueBackground.Outdoor: speed = 0.85f; curl = 0.65f; break;
            case VenueBackground.Stadium: speed = 1.15f; curl = 1.35f; break;
            default: speed = 1f; curl = 1f; break;
        }
    }

    /// <summary>The sheet at one event: venue says which event (any steady number), visit which time round.</summary>
    public static void Sheet(int background, int careerSeed, int venue, int visit, out float speed, out float curl)
    {
        KindIce(background, out speed, out curl);
        var character = new System.Random(Mix(careerSeed, venue));
        var today = new System.Random(Mix(Mix(careerSeed, venue), visit + 1));
        speed *= (1f + CharacterSpeed * Spread(character)) * (1f + VisitSpeed * Spread(today));
        curl *= (1f + CharacterCurl * Spread(character)) * (1f + VisitCurl * Spread(today));
    }

    /// <summary>The sheet for the game being played.</summary>
    public static void Current(out float speed, out float curl)
    {
        Identify(out int background, out int seed, out int venue, out int visit);
        Sheet(background, seed, venue, visit, out speed, out curl);
    }

    /// <summary>Where the game is being played: the background, the career's ice seed, a number for the event and one for the visit.</summary>
    public static void Identify(out int background, out int seed, out int venue, out int visit)
    {
        var career = Object.FindFirstObjectByType<CareerManager>();
        var settings = Object.FindFirstObjectByType<GameSettingsPersist>();
        background = Background(career, settings);

        seed = career != null ? career.IceSeed : 1;
        Tourny tourny = career != null ? career.currentTourny : null;
        if (TestBackground >= 0) { venue = -1 - background; visit = TestVisit; }
        else if (tourny == null || (settings != null && settings.cashGame)) { venue = -100 - background; visit = career != null ? career.season : 0; }
        else { venue = Mix(tourny.id, TextNumber(tourny.name)); visit = career.season; }
    }

    static float Spread(System.Random random) { return (float)(random.NextDouble() * 2.0 - 1.0); }

    // our own mixing, so the same career finds the same ice on every device and run (string.GetHashCode makes no such promise)
    static int Mix(int a, int b)
    {
        unchecked
        {
            uint h = 2166136261u;
            h = (h ^ (uint)a) * 16777619u;
            h = (h ^ (uint)b) * 16777619u;
            h ^= h >> 15; h *= 2246822519u; h ^= h >> 13;
            return (int)(h & 0x7fffffff);
        }
    }

    static int TextNumber(string text)
    {
        int n = 0;
        if (text != null)
            foreach (char c in text) n = Mix(n, c);
        return n;
    }
}
