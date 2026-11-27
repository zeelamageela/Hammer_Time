using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// How the sheet is playing: its speed and how much it curls. The same pull throws the same weight on any ice; the ice decides how
/// far that goes. Faster ice carries a rock further, so the same shot is thrown softer, and takes longer.
///
/// With curl at 1, any ice gives the same path to the same spot (only the weight and the timing change). Curl then scales the bend.
/// The aim line, the AI and the clocks all follow, since they read the profile in force.
///
/// Two tester options for now (pause menu): Venue Ice takes the numbers from where the game is being played (see VenueIce); Test
/// Ice takes them from Quick Test's sliders. Venue Ice wins if both are on. Never in the tutorial or a release build.
/// </summary>
public static class IceConditions
{
    const string Key = "VariableIce";

    /// <summary>A fast, curly sheet: what the checkbox gives until something else sets the numbers.</summary>
    public const float DefaultSpeed = 1.15f, DefaultCurl = 1.35f;

    /// <summary>How far a given weight travels past the release point, against normal ice (1).</summary>
    public static float Speed { get; private set; } = DefaultSpeed;

    /// <summary>How much a shot bends on its way to the same spot, against normal ice (1).</summary>
    public static float Curl { get; private set; } = DefaultCurl;

    public static bool Enabled
    {
        get { try { return PlayerPrefs.GetInt(Key, 0) == 1; } catch { return false; } }
        set
        {
            try { PlayerPrefs.SetInt(Key, value ? 1 : 0); PlayerPrefs.Save(); } catch { }
            Refresh();
        }
    }

    /// <summary>Whether this game can be played on anything but normal ice.</summary>
    public static bool Allowed
    {
        get
        {
            if (!DevOptions.Enabled) return false;
            // the tutorial and story games throw scripted shots that assume normal ice (asked by scene name: it is known
            // before the tutorial has set itself up)
            string scene = SceneManager.GetActiveScene().name;
            return !scene.Contains("Tutorial") && !scene.Contains("Story");
        }
    }

    public static void Set(float speed, float curl)
    {
        if (Mathf.Approximately(speed, Speed) && Mathf.Approximately(curl, Curl)) return;
        Speed = speed;
        Curl = curl;
        Refresh();
    }

    /// <summary>The conditions games are being played on right now (1, 1 = normal ice).</summary>
    public static bool InForce(out float speed, out float curl)
    {
        speed = curl = 1f;
        bool venue = VenueIce.Enabled;
        if ((!venue && !Enabled) || !Allowed) return false;
        if (venue) VenueIce.Current(out speed, out curl);
        else { speed = Speed; curl = Curl; }
        return true;
    }

    /// <summary>The profile as it plays on the conditions in force.</summary>
    public static RockPhysicsProfile Apply(RockPhysicsProfile pace)
    {
        return InForce(out float speed, out float curl) ? pace.WithIce(speed, curl) : pace;
    }

    /// <summary>Puts the conditions in force: at once if the sheet is still, otherwise at the next throw.</summary>
    public static void Refresh() { RockPhysicsSettings.Request(RockPhysicsBootstrap.Make(RockPhysicsBootstrap.Current)); }

    /// <summary>"fast, curly".</summary>
    public static string Describe() { return Describe(Speed, Curl); }

    public static string Describe(float Speed, float Curl)
    {
        string speed = Speed > 1.02f ? "fast" : Speed < 0.98f ? "slow" : "normal speed";
        string curl = Curl > 1.02f ? "curly" : Curl < 0.98f ? "straight" : "normal curl";
        return speed + ", " + curl;
    }
}
