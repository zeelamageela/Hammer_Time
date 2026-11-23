using UnityEngine;

/// <summary>
/// Compares what the aim preview promised with what the rock actually did, for every real throw, and logs one line at the
/// launch and one when the rock stops. Search the console for "[ShotAudit]". Cheap (a few floats); safe to leave on while
/// the aim line is being tuned, and the output is what the skill-based aim fuzz will be checked against.
/// </summary>
public static class ShotAudit
{
    static Vector2 predictedVelocity;
    static Vector2 predictedEnd;
    static bool predictedInTurn;
    static bool predictedCollision;
    static float predictedAt = -999f;

    static bool armed;
    static GameObject launchedRock;
    static Vector2 launchPosition;
    static Vector2 launchVelocity;
    static bool launchedInTurn;
    static float launchedAt;

    /// <summary>TrajectoryLine calls this every time it redraws the preview.</summary>
    public static void RecordPreview(Vector2 velocity, bool inTurn, Vector2 endOfLine, bool hitsARock)
    {
        predictedVelocity = velocity;
        predictedInTurn = inTurn;
        predictedEnd = endOfLine;
        predictedCollision = hitsARock;
        predictedAt = Time.unscaledTime;
    }

    /// <summary>Rock_Flick calls this when the shot is fired (velocity as set on the rigidbody).</summary>
    public static void LogLaunch(GameObject rock, Vector2 position, Vector2 velocity, bool inTurn)
    {
        launchedRock = rock;
        launchPosition = position;
        launchVelocity = velocity;
        launchedInTurn = inTurn;
        launchedAt = Time.unscaledTime;
        armed = true;

        bool freshPreview = Time.unscaledTime - predictedAt < 3f;
        if (!freshPreview)
        {
            Debug.Log($"[ShotAudit] LAUNCH from {position}: v={velocity} |v|={velocity.magnitude:F3} turn={(inTurn ? "IN" : "OUT")} (no recent aim preview - AI shot?)");
            return;
        }

        Vector2 dv = velocity - predictedVelocity;
        string flags = (inTurn != predictedInTurn ? " TURN-MISMATCH(preview " + (predictedInTurn ? "IN" : "OUT") + ")" : "")
                     + (dv.magnitude > 0.05f ? " VELOCITY-MISMATCH" : "");
        Debug.Log($"[ShotAudit] LAUNCH from {position}: v={velocity} |v|={velocity.magnitude:F3} turn={(inTurn ? "IN" : "OUT")} | preview v={predictedVelocity} (diff {dv.magnitude:F3}) predicted end {predictedEnd}{(predictedCollision ? " (preview shows a collision)" : "")}{flags}");
    }

    /// <summary>Rock_Force calls this once, when the rock first comes to rest.</summary>
    public static void LogRest(GameObject rock, Vector2 position, Vector2 modelPredictionFromRelease)
    {
        if (!armed || rock != launchedRock) return;
        armed = false;
        Vector2 modelMiss = position - modelPredictionFromRelease;
        Debug.Log($"[ShotAudit] MODEL-FROM-ACTUAL-RELEASE predicted rest {modelPredictionFromRelease}; actual {position}; difference ({modelMiss.x:F2}, {modelMiss.y:F2}) - if this is ~0 the difference is BEFORE the hog line, if not it is the friction after it");

        bool freshPreview = launchedAt - predictedAt < 3f && launchedAt >= predictedAt;
        if (!freshPreview || predictedCollision)
        {
            Debug.Log($"[ShotAudit] REST at {position} ({Time.unscaledTime - launchedAt:F1}s after launch)");
            return;
        }

        Vector2 miss = position - predictedEnd;
        Debug.Log($"[ShotAudit] REST at {position}: preview said {predictedEnd}, off by ({miss.x:F2}, {miss.y:F2}) = {miss.magnitude:F2} units ({Time.unscaledTime - launchedAt:F1}s after launch)");
    }

    /// <summary>Rock_Force calls this at the near hog line, when friction starts.</summary>
    public static void LogRelease(GameObject rock, Vector2 position, Vector2 velocity, float spin, Rigidbody2D body)
    {
        if (!armed || rock != launchedRock) return;
        Debug.Log($"[ShotAudit] HOG LINE at {position}: v={velocity} |v|={velocity.magnitude:F3} (launched at |v|={launchVelocity.magnitude:F3}); box2d damping lin={body.linearDamping:F3} ang={body.angularDamping:F3}; " +
                  $"fixedDt={Time.fixedDeltaTime:F4} timeScale={Time.timeScale:F2} interp={body.interpolation} collision={body.collisionDetectionMode} mass={body.mass:F1} t+{Time.unscaledTime - launchedAt:F2}s");
    }
}
