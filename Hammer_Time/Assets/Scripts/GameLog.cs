using System.Diagnostics;

/// <summary>
/// The game's chatty logging. Calls to GameLog.Info are left out of the build entirely - message and all, so they cost nothing -
/// unless HAMMER_LOGS is defined. To see them again, add HAMMER_LOGS under Project Settings > Player > Scripting Define Symbols.
///
/// Use Debug.LogWarning and Debug.LogError for things that are actually wrong; those always show.
/// </summary>
public static class GameLog
{
    [Conditional("HAMMER_LOGS")]
    public static void Info(object message)
    {
        UnityEngine.Debug.Log(message);
    }

    [Conditional("HAMMER_LOGS")]
    public static void Info(object message, UnityEngine.Object context)
    {
        UnityEngine.Debug.Log(message, context);
    }
}
