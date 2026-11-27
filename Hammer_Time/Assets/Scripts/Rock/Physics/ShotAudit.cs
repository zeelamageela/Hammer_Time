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
            GameLog.Info($"[ShotAudit] LAUNCH from {position}: v={velocity} |v|={velocity.magnitude:F3} turn={(inTurn ? "IN" : "OUT")} (no recent aim preview - AI shot?)");
            return;
        }

        Vector2 dv = velocity - predictedVelocity;
        string flags = (inTurn != predictedInTurn ? " TURN-MISMATCH(preview " + (predictedInTurn ? "IN" : "OUT") + ")" : "")
                     + (dv.magnitude > 0.05f ? " VELOCITY-MISMATCH" : "");
        GameLog.Info($"[ShotAudit] LAUNCH from {position}: v={velocity} |v|={velocity.magnitude:F3} turn={(inTurn ? "IN" : "OUT")} | preview v={predictedVelocity} (diff {dv.magnitude:F3}) predicted end {predictedEnd}{(predictedCollision ? " (preview shows a collision)" : "")}{flags}");
    }

    /// <summary>Rock_Force calls this once, when the rock first comes to rest.</summary>
    public static void LogRest(GameObject rock, Vector2 position, Vector2 modelPredictionFromRelease)
    {
        if (!armed || rock != launchedRock) return;
        armed = false;
        Vector2 modelMiss = position - modelPredictionFromRelease;
        GameLog.Info($"[ShotAudit] MODEL-FROM-ACTUAL-RELEASE predicted rest {modelPredictionFromRelease}; actual {position}; difference ({modelMiss.x:F2}, {modelMiss.y:F2}) - if this is ~0 the difference is BEFORE the hog line, if not it is the friction after it");

        bool freshPreview = launchedAt - predictedAt < 3f && launchedAt >= predictedAt;
        if (!freshPreview || predictedCollision)
        {
            GameLog.Info($"[ShotAudit] REST at {position} ({Time.unscaledTime - launchedAt:F1}s after launch)");
            return;
        }

        Vector2 miss = position - predictedEnd;
        GameLog.Info($"[ShotAudit] REST at {position}: preview said {predictedEnd}, off by ({miss.x:F2}, {miss.y:F2}) = {miss.magnitude:F2} units ({Time.unscaledTime - launchedAt:F1}s after launch)");
    }

    /// <summary>Rock_Force calls this at the near hog line, when friction starts.</summary>
    public static void LogRelease(GameObject rock, Vector2 position, Vector2 velocity, float spin, Rigidbody2D body)
    {
        if (!armed || rock != launchedRock) return;
        GameLog.Info($"[ShotAudit] HOG LINE at {position}: v={velocity} |v|={velocity.magnitude:F3} (launched at weight |v|={launchVelocity.magnitude:F3}, expected launch speed {RockPhysicsSettings.Current.LaunchSpeedFromWeight(launchVelocity.magnitude):F3}); box2d damping lin={body.linearDamping:F3} ang={body.angularDamping:F3}; " +
                  $"fixedDt={Time.fixedDeltaTime:F4} timeScale={Time.timeScale:F2} interp={body.interpolation} collision={body.collisionDetectionMode} mass={body.mass:F1} t+{Time.unscaledTime - launchedAt:F2}s");
        var sb = new System.Text.StringBuilder("[ShotAudit] FLIGHT speed by y (hack to hog): ");
        for (int i = 0; i < flightCount; i++) sb.Append($"{flightY[i]:F1}:{flightSpeed[i]:F3}  ");
        GameLog.Info(sb.ToString());
    }

    // ---- speed samples during free flight (every ~1 unit of y) so a speed loss before the hog line can be located ----
    const int MaxFlight = 40;
    static readonly float[] flightY = new float[MaxFlight];
    static readonly float[] flightSpeed = new float[MaxFlight];
    static int flightCount;
    static float lastSampleY = -999f;

    /// <summary>RockSlide calls this every physics step while a rock is in free flight.</summary>
    public static void Sample(GameObject rock, float y, float speed)
    {
        if (!armed || rock != launchedRock) return;
        if (y < lastSampleY + 1f || flightCount >= MaxFlight) return;
        flightY[flightCount] = y; flightSpeed[flightCount] = speed; flightCount++; lastSampleY = y;
    }

    static float lastFlightSpeed;
    static float lastPhaseSpeed;
    static string lastPhase = "";

    /// <summary>
    /// RockSlide reports the rock's speed at fixed points of each frame (FixedUpdate start, Update, LateUpdate). A drop between two of them
    /// says where it happened: inside the physics step (FixedUpdate start -> Update of the same tick) or in script code between frames.
    /// </summary>
    public static void Phase(GameObject rock, string phase, float speed)
    {
        if (!armed || rock != launchedRock) return;
        if (lastPhaseSpeed > 0f && speed < lastPhaseSpeed * 0.999f)
            GameLog.Info($"[ShotAudit] PHASE of speed drop: between '{lastPhase}' and '{phase}': {lastPhaseSpeed:F4} -> {speed:F4} (x{speed / lastPhaseSpeed:F4}) frame={Time.frameCount}");
        lastPhaseSpeed = speed; lastPhase = phase;
    }

    /// <summary>For tools: how many speed drops WatchSpeed has seen, and the latest one's ratio and y.</summary>
    public static int DropCount;
    public static float LastDropRatio;
    public static float LastDropY;

    /// <summary>
    /// RockSlide calls this each step of free flight. If the speed falls, it logs the step, the Box2D damping, and everything the rock
    /// is touching or overlapping, to identify what slows a rock that no physics code should be slowing.
    /// </summary>
    public static void WatchSpeed(GameObject rock, Rigidbody2D body)
    {
        if (!armed || rock != launchedRock) return;
        float speed = body.linearVelocity.magnitude;
        if (lastFlightSpeed > 0f && speed < lastFlightSpeed * 0.999f)
        {
            DropCount++; LastDropRatio = speed / lastFlightSpeed; LastDropY = body.position.y;
            var sb = new System.Text.StringBuilder();
            foreach (var c in Physics2D.OverlapCircleAll(body.position, 0.6f))
                if (c != null && c.gameObject != rock)
                    sb.Append($"{c.gameObject.name}[{(c.isTrigger ? "trigger" : "SOLID")}] ");
            var contacts = new ContactPoint2D[8];
            int n = body.GetContacts(contacts);
            GameLog.Info($"[ShotAudit] SPEED DROP in free flight at y={body.position.y:F2}: {lastFlightSpeed:F4} -> {speed:F4} (x{speed / lastFlightSpeed:F4}); box2d damping lin={body.linearDamping:F3}; " +
                      $"touching {n} collider(s); nearby: {sb}; frame={Time.frameCount} fixedTime={Time.fixedTime:F2}");
        }
        lastFlightSpeed = speed;
    }

    /// <summary>Rock_Flick calls this right after the velocity is set: the body and joint settings that could slow a rock down.</summary>
    public static void LogBody(Rigidbody2D body, SpringJoint2D spring)
    {
        flightCount = 0; lastSampleY = -999f; lastFlightSpeed = 0f;
        GameLog.Info($"[ShotAudit] BODY at launch: bodyType={body.bodyType} gravityScale={body.gravityScale} gravity={Physics2D.gravity} constraints={body.constraints} simulated={body.simulated} " +
                  $"damping lin={body.linearDamping:F3} ang={body.angularDamping:F3} spring={(spring != null ? (spring.enabled ? "ENABLED" : "disabled") : "none")} sleepMode={body.sleepMode} " +
                  $"profile={RockPhysicsSettings.Current.law} power={RockPhysicsSettings.Current.stopPower:F2} stretch={RockPhysicsSettings.Current.timeStretch:F2}");
    }
}
