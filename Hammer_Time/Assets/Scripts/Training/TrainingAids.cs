using UnityEngine;

/// <summary>
/// What the player's training lets the match show or do: the aim circle (and how wide), the collision indicator (and how accurate), the
/// collision warning line, and carving with one sweeper. Matches without a career (the tutorial, a game started straight from a scene)
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

    public static bool AimCircleUnlocked { get { return levels == null || TrainingCatalog.AimCircleDiameter(levels) > 0f; } }

    /// <summary>Width the aim circle should have, or a negative number when it should be left as it is (ungated).</summary>
    public static float AimCircleDiameter { get { return levels == null ? -1f : TrainingCatalog.AimCircleDiameter(levels); } }

    /// <summary>The collision arrows (where the hit rocks will go) are shown from Tactic Studies level 1.</summary>
    public static bool CollisionIndicatorUnlocked { get { return levels == null || TrainingCatalog.CollisionIndicatorError(levels) >= 0f; } }

    /// <summary>The red collision warning line comes with Tactic Studies level 3.</summary>
    public static bool CollisionWarningUnlocked { get { return levels == null || TrainingCatalog.Owned(levels, TrainingTrack.Tactics) >= 3; } }

    /// <summary>How many degrees the shown collision arrows may be off (0 when exact or ungated).</summary>
    public static float CollisionErrorDegrees { get { return levels == null ? 0f : Mathf.Max(0f, TrainingCatalog.CollisionIndicatorError(levels)); } }

    /// <summary>Carving (a tap on one sweeper pushes the rock sideways): 0 = locked, otherwise its strength. Ungated = full (1).</summary>
    public static float CarvingStrength { get { return levels == null ? 1f : TrainingCatalog.DirectionalStrength(levels); } }
}
