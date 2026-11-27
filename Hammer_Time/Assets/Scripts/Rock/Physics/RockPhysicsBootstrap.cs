using UnityEngine;

/// <summary>Which rock physics the player is using. Persisted; the player picks the Game Pace in the pause menu.</summary>
public enum PhysicsChoice
{
    /// <summary>The original exponential rock (creeps for ~19 s).</summary>
    Classic = 0,
    /// <summary>Constant-deceleration friction at the game's pace: same shots, same paths, snappier stop.</summary>
    Arcade = 1,
    /// <summary>Arcade slowed to real-ice timing: a button draw takes ~14.5 s hog to hog.</summary>
    RealTime = 2,
    /// <summary>Arcade at half speed: a button draw takes ~6.5 s hog to hog, between Arcade (3.3 s) and Real-time.</summary>
    Pro = 3,
}

/// <summary>Picks the profile at startup and offers the choice to the UI.</summary>
public static class RockPhysicsBootstrap
{
    const string Key = "PhysicsProfile";

    /// <summary>The pace a player starts with.</summary>
    public const PhysicsChoice Shipping = PhysicsChoice.Arcade;

    const string StopKey = "StopPower";
    /// <summary>Stop curves for Arcade: firm, soft (default), mellow, extra mellow.</summary>
    public static readonly float[] StopPowers = { 0.5f, 0.6f, 0.7f, 0.8f };
    public static readonly string[] StopNames = { "Firm", "Soft", "Mellow", "Extra mellow" };

    /// <summary>The paces on offer, fastest to slowest (the saved numbers stay as they were; this is only the order). Classic is no
    /// longer offered.</summary>
    public static readonly PhysicsChoice[] Order = { PhysicsChoice.Arcade, PhysicsChoice.Pro, PhysicsChoice.RealTime };

    public static int IndexOf(PhysicsChoice choice) { return System.Array.IndexOf(Order, choice); }

    public static PhysicsChoice FromIndex(int index) { return Order[Mathf.Clamp(index, 0, Order.Length - 1)]; }

    /// <summary>Arcade and Pro both follow the stop-curve setting.</summary>
    static bool UsesStopCurve(PhysicsChoice c) { return c == PhysicsChoice.Arcade || c == PhysicsChoice.Pro; }

    public static PhysicsChoice Current { get; private set; }
    public static float StopPower { get; private set; } = 0.6f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        Current = Load();
        StopPower = LoadStopPower();
        RockPhysicsSettings.Use(Make(Current));
    }

    /// <summary>The saved pace, in every build. Anything no longer offered (an old Classic save) becomes the default.</summary>
    public static PhysicsChoice Load()
    {
        try
        {
            var saved = (PhysicsChoice)PlayerPrefs.GetInt(Key, (int)Shipping);
            return IndexOf(saved) >= 0 ? saved : Shipping;
        }
        catch { return Shipping; }
    }

    static float LoadStopPower()
    {
        if (!DevOptions.Enabled) return 0.6f;
        try { return Mathf.Clamp(PlayerPrefs.GetFloat(StopKey, 0.6f), StopPowers[0], StopPowers[StopPowers.Length - 1]); }
        catch { return 0.6f; }
    }

    /// <summary>Cycle Arcade's stop curve (firm, soft, mellow, extra mellow, back to firm). Only changes Arcade.</summary>
    public static void NextStopCurve()
    {
        SetStopIndex((StopIndex() + 1) % StopPowers.Length);
    }

    /// <summary>Choose Arcade's stop curve by position in StopPowers (firm, soft, mellow, extra mellow). Only changes Arcade.</summary>
    public static void SetStopIndex(int index)
    {
        StopPower = StopPowers[Mathf.Clamp(index, 0, StopPowers.Length - 1)];
        try { PlayerPrefs.SetFloat(StopKey, StopPower); PlayerPrefs.Save(); } catch { }
        if (UsesStopCurve(Current)) RockPhysicsSettings.Request(Make(Current));
    }

    public static int StopIndex()
    {
        int best = 0;
        for (int i = 1; i < StopPowers.Length; i++)
            if (Mathf.Abs(StopPowers[i] - StopPower) < Mathf.Abs(StopPowers[best] - StopPower)) best = i;
        return best;
    }

    public static string StopLabel() { return StopNames[StopIndex()] + " (" + StopPowers[StopIndex()].ToString("0.0") + ")"; }

    /// <summary>Choose and remember a profile. Applied straight away when no rock is moving, otherwise at the next throw.</summary>
    public static void Choose(PhysicsChoice choice)
    {
        Current = choice;
        try { PlayerPrefs.SetInt(Key, (int)choice); PlayerPrefs.Save(); } catch { }
        RockPhysicsSettings.Request(Make(choice));
    }

    /// <summary>The profile for a pace, on the ice in force (see IceConditions).</summary>
    public static RockPhysicsProfile Make(PhysicsChoice choice)
    {
        return IceConditions.Apply(MakeOnNormalIce(choice));
    }

    public static RockPhysicsProfile MakeOnNormalIce(PhysicsChoice choice)
    {
        switch (choice)
        {
            case PhysicsChoice.Classic: return RockPhysicsProfile.Legacy();
            case PhysicsChoice.RealTime: return RockPhysicsProfile.Real();
            case PhysicsChoice.Pro: return RockPhysicsProfile.Pro(StopPower);
            default: return RockPhysicsProfile.Arcade(StopPower);
        }
    }

    public static string Label(PhysicsChoice choice)
    {
        switch (choice)
        {
            case PhysicsChoice.Classic: return "Classic";
            case PhysicsChoice.RealTime: return "Real-time";
            case PhysicsChoice.Pro: return "Pro";
            default: return "Arcade";
        }
    }

    public static PhysicsChoice Next(PhysicsChoice choice)
    {
        return Order[(IndexOf(choice) + 1) % Order.Length];
    }
}
