using UnityEngine;

/// <summary>
/// The player's read of the ice: the speed and curl their aim line is drawn with (the line, dots, hit arrows, collision warning,
/// weight call and expected times). The rock itself, and the computer's shots, stay on the real ice (RockPhysicsSettings), so a
/// wrong read misses the same way every time until it is corrected.
///
/// With Venue Ice on, a read is kept for each event: it carries across ends and games, and on a later visit the player is asked
/// whether to start from those notes. It starts at the average for that kind of venue.
///
/// A tester option for now (pause menu: Adjustable Line).
/// </summary>
public static class PlayerRead
{
    const string Key = "AdjustableLine";
    public static float MinSpeed { get { return IceUnits.SpeedFromSeconds(IceUnits.MinSeconds); } }
    public static float MaxSpeed { get { return IceUnits.SpeedFromSeconds(IceUnits.MaxSeconds); } }
    public static float MinCurl { get { return IceUnits.CurlFromFeet(IceUnits.MinFeet); } }
    public static float MaxCurl { get { return IceUnits.CurlFromFeet(IceUnits.MaxFeet); } }

    public struct Note
    {
        public float speed, curl;
        public int visit;
    }

    public static bool Enabled
    {
        get { try { return PlayerPrefs.GetInt(Key, 0) == 1; } catch { return false; } }
        set { try { PlayerPrefs.SetInt(Key, value ? 1 : 0); PlayerPrefs.Save(); } catch { } }
    }

    /// <summary>Whether the aim line is drawn from the read right now.</summary>
    public static bool Active { get { return Enabled && IceConditions.Allowed; } }

    public static float Speed { get; private set; } = 1f;
    public static float Curl { get; private set; } = 1f;

    /// <summary>The average for the kind of venue: where a read starts.</summary>
    public static float StartSpeed { get; private set; } = 1f;
    public static float StartCurl { get; private set; } = 1f;

    /// <summary>Notes from an earlier visit are waiting for the player to take or leave.</summary>
    public static bool NotesWaiting { get; private set; }
    public static Note LastNotes { get; private set; }

    static string place;     // where the read belongs: null = nowhere (not remembered)
    static int visit;
    static bool prepared;

    /// <summary>Settles which read applies to the game being played. Cheap to call every turn.</summary>
    public static void Prepare()
    {
        string here = null;
        int now = 0;
        float speed = 1f, curl = 1f;
        if (VenueIce.Enabled)
        {
            VenueIce.Identify(out int background, out int seed, out int venue, out now);
            here = "IceRead_" + seed + "_" + venue;
            VenueIce.KindIce(background, out speed, out curl);
        }
        if (prepared && here == place && now == visit) return;

        prepared = true;
        place = here;
        visit = now;
        StartSpeed = Speed = speed;
        StartCurl = Curl = curl;
        NotesWaiting = false;

        if (here == null || !TryUnpack(Load(here), out Note note)) return;
        if (note.visit == now) { Speed = note.speed; Curl = note.curl; }
        else { LastNotes = note; NotesWaiting = true; }
    }

    /// <summary>The player's new read; remembered for this event.</summary>
    public static void Set(float speed, float curl)
    {
        Speed = Mathf.Clamp(speed, MinSpeed, MaxSpeed);
        Curl = Mathf.Clamp(curl, MinCurl, MaxCurl);
        NotesWaiting = false;
        if (place == null) return;
        try
        {
            PlayerPrefs.SetString(place, Pack(new Note { speed = Speed, curl = Curl, visit = visit }));
            PlayerPrefs.Save();
        }
        catch { }
    }

    public static void UseNotes() { Set(LastNotes.speed, LastNotes.curl); }

    public static void StartFresh() { Set(StartSpeed, StartCurl); }

    static string Load(string key) { try { return PlayerPrefs.GetString(key, ""); } catch { return ""; } }

    public static string Pack(Note note)
    {
        var c = System.Globalization.CultureInfo.InvariantCulture;
        return note.speed.ToString("0.###", c) + ";" + note.curl.ToString("0.###", c) + ";" + note.visit.ToString(c);
    }

    public static bool TryUnpack(string text, out Note note)
    {
        note = default;
        if (string.IsNullOrEmpty(text)) return false;
        string[] parts = text.Split(';');
        var c = System.Globalization.CultureInfo.InvariantCulture;
        var any = System.Globalization.NumberStyles.Float;
        return parts.Length == 3 && float.TryParse(parts[0], any, c, out note.speed) && float.TryParse(parts[1], any, c, out note.curl)
            && int.TryParse(parts[2], System.Globalization.NumberStyles.Integer, c, out note.visit);
    }

    static RockPhysicsProfile profile;
    static PhysicsChoice profilePace;
    static float profileStop, profileSpeed, profileCurl;

    /// <summary>The rock on the ice as the player reads it, at the game's pace.</summary>
    public static RockPhysicsProfile Profile
    {
        get
        {
            if (profile == null || profilePace != RockPhysicsBootstrap.Current || profileStop != RockPhysicsBootstrap.StopPower
                || profileSpeed != Speed || profileCurl != Curl)
            {
                profilePace = RockPhysicsBootstrap.Current;
                profileStop = RockPhysicsBootstrap.StopPower;
                profileSpeed = Speed;
                profileCurl = Curl;
                profile = RockPhysicsBootstrap.MakeOnNormalIce(profilePace).WithIce(Speed, Curl);
            }
            return profile;
        }
    }
}
