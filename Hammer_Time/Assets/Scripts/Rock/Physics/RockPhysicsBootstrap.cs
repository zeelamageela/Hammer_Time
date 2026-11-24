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

    const string StopKey = "StopPower";
    /// <summary>Stop curves for Arcade: firm, soft (default), mellow, extra mellow.</summary>
    public static readonly float[] StopPowers = { 0.5f, 0.6f, 0.7f, 0.8f };
    public static readonly string[] StopNames = { "Firm", "Soft", "Mellow", "Extra mellow" };

    public static PhysicsChoice Current { get; private set; }
    public static float StopPower { get; private set; } = 0.6f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        Current = Load();
        StopPower = LoadStopPower();
        RockPhysicsSettings.Use(Make(Current));
    }

    public static PhysicsChoice Load()
    {
        if (!DevOptions.Enabled) return Shipping;
        try { return (PhysicsChoice)Mathf.Clamp(PlayerPrefs.GetInt(Key, (int)Shipping), 0, 2); }
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
        if (Current == PhysicsChoice.Arcade) RockPhysicsSettings.Request(Make(Current));
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

    public static RockPhysicsProfile Make(PhysicsChoice choice)
    {
        switch (choice)
        {
            case PhysicsChoice.Classic: return RockPhysicsProfile.Legacy();
            case PhysicsChoice.RealTime: return RockPhysicsProfile.Real();
            default: return RockPhysicsProfile.Arcade(StopPower);
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
