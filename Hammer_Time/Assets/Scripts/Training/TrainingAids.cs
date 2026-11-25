using UnityEngine;

/// <summary>
/// What the player's training lets the match show or do: the aim crosshairs, the collision warning line, the hit arrows (and how accurate), carving with one sweeper, the
/// trajectory line (dots, how far it reaches) and the timing ranges. Matches without a career (the tutorial, a game started straight from a scene)
/// are not gated: everything is available, as before. Call Refresh() when a match starts.
/// </summary>
public static class TrainingAids
{
    static int[] levels;   // null = nothing is gated

    public static bool Gated { get { return levels != null; } }

    public static void Refresh()
    {
        levels = null;
        if (TutorialGameManager.Instance != null && TutorialGameManager.Instance.isTutorialGame) return;
        var cm = Object.FindFirstObjectByType<CareerManager>();
        if (cm == null) return;
        levels = cm.trainingLevels != null ? cm.trainingLevels : new int[TrainingCatalog.Tracks];
    }

    /// <summary>Test hook: pretend these training levels are in force (null = ungated).</summary>
    public static void Use(int[] owned) { levels = owned; }

    public static bool CrosshairsUnlocked { get { return levels == null || TrainingCatalog.CrosshairsUnlocked(levels); } }

    /// <summary>Hit arrows (where the hit rocks will go) come with Tactic Studies level 3.</summary>
    public static bool CollisionIndicatorUnlocked { get { return levels == null || TrainingCatalog.CollisionIndicatorError(levels) >= 0f; } }

    /// <summary>The red collision warning line comes with Tactic Studies level 2.</summary>
    public static bool CollisionWarningUnlocked { get { return levels == null || TrainingCatalog.CollisionWarningUnlocked(levels); } }

    /// <summary>How many degrees the shown hit arrows may be off (0 when exact or ungated).</summary>
    public static float CollisionErrorDegrees { get { return levels == null ? 0f : Mathf.Max(0f, TrainingCatalog.CollisionIndicatorError(levels)); } }

    /// <summary>Carving (a tap on one sweeper pushes the rock sideways) is on from Sweep Training level 1. Ungated = on.</summary>
    public static bool CarvingUnlocked { get { return levels == null || TrainingCatalog.CarvingUnlocked(levels); } }

    public static bool DotsUnlocked { get { return levels == null || TrainingCatalog.DotsUnlocked(levels); } }

    public static bool TimingUnlocked { get { return levels == null || TrainingCatalog.TimingUnlocked(levels); } }

    /// <summary>The y the trajectory line may be drawn up to (infinity = all of it).</summary>
    public static float LineReachY { get { return levels == null ? float.PositiveInfinity : TrainingCatalog.LineReachY(levels); } }
}
