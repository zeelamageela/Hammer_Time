using UnityEngine;

/// <summary>Which rock physics the player is using. Persisted; chosen from the pause menu in dev builds.</summary>
public enum PhysicsChoice
{
    /// <summary>The original exponential rock (creeps for ~19 s).</summary>
    Classic = 0,
    /// <summary>Constant-deceleration friction at the game's pace: same shots, same paths, snappier stop.</summary>
    Arcade = 1,
    /// <summary>Arcade slowed to real-ice timing: a button draw takes ~14.5 s hog to hog.</summary>
    RealTime = 2,
}

/// <summary>Picks the profile at startup and offers the choice to the UI.</summary>
public static class RockPhysicsBootstrap
{
    const string Key = "PhysicsProfile";

    /// <summary>What a player gets when no tester option is available (a release build).</summary>
    public const PhysicsChoice Shipping = PhysicsChoice.Arcade;

    public static PhysicsChoice Current { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        Current = Load();
        RockPhysicsSettings.Use(Make(Current));
    }

    public static PhysicsChoice Load()
    {
        if (!DevOptions.Enabled) return Shipping;
        try { return (PhysicsChoice)Mathf.Clamp(PlayerPrefs.GetInt(Key, (int)Shipping), 0, 2); }
        catch { return Shipping; }
    }

    /// <summary>Choose and remember a profile. Applied straight away when no rock is moving, otherwise at the next throw.</summary>
    public static void Choose(PhysicsChoice choice)
    {
        Current = choice;
        try { PlayerPrefs.SetInt(Key, (int)choice); PlayerPrefs.Save(); } catch { }
        RockPhysicsSettings.Request(Make(choice));
    }

    public static RockPhysicsProfile Make(PhysicsChoice choice)
    {
        switch (choice)
        {
            case PhysicsChoice.Classic: return RockPhysicsProfile.Legacy();
            case PhysicsChoice.RealTime: return RockPhysicsProfile.Real();
            default: return RockPhysicsProfile.Arcade();
        }
    }

    public static string Label(PhysicsChoice choice)
    {
        switch (choice)
        {
            case PhysicsChoice.Classic: return "Classic";
            case PhysicsChoice.RealTime: return "Real-time";
            default: return "Arcade";
        }
    }

    public static PhysicsChoice Next(PhysicsChoice choice)
    {
        return (PhysicsChoice)(((int)choice + 1) % 3);
    }
}
