using UnityEngine;

/// <summary>The profile the whole game is using right now. The live rocks and the trajectory simulator both read it.</summary>
public static class RockPhysicsSettings
{
    public static RockPhysicsProfile Current = RockPhysicsProfile.Legacy();
}

/// <summary>
/// Applies RockPhysics to a rock's Rigidbody2D every physics step: friction, curl, spin fade. It replaces the
/// Rigidbody's own damping (which is turned off) so the live rock and the trajectory simulator run the identical model.
///
/// Rock_Force adds this in Awake, so it exists on every rock without editing scenes, and it runs whether or not
/// Rock_Force is enabled (Rock_Force is switched off for rocks that are not in flight, but a struck rock still has to slow down).
/// Box2D still does collisions, positions and sleeping.
/// </summary>
[DefaultExecutionOrder(-10)]
public class RockSlide : MonoBehaviour
{
    Rigidbody2D body;
    RockState state;
    bool ready;

    /// <summary>Current drag values: sweeping lowers/raises these while a rock is in flight.</summary>
    public float LinearDamping { get { Init(); return state.linearDamping; } set { Init(); state.linearDamping = value; } }
    public float AngularDamping { get { Init(); return state.angularDamping; } set { Init(); state.angularDamping = value; } }
    /// <summary>Scales curl (1 = normal). Rock_Force sets it each step from its multiplier and the sweepers.</summary>
    public float CurlMultiplier { get { Init(); return state.curlMultiplier; } set { Init(); state.curlMultiplier = value; } }
    public bool AtRest { get { Init(); return state.atRest; } }
    public Vector2 Curl { get { Init(); return new Vector2(state.curlX, 0f); } }
    /// <summary>Where the model says this rock will stop, worked out from the state it actually had at the hog line (for ShotAudit).</summary>
    public Vector2 PredictedRest { get; private set; }
    public float SpinNow { get { Init(); return state.spin; } }

    void Awake() { Init(); }

    void Init()
    {
        if (ready) return;
        body = GetComponent<Rigidbody2D>();
        state = RockPhysics.NewRock(RockPhysicsSettings.Current);
        if (body != null)
        {
            body.linearDamping = 0f;     // RockPhysics applies drag now; leaving Box2D's on would double it
            body.angularDamping = 0f;
        }
        ready = true;
    }

    /// <summary>The rock was just thrown from the hack: free flight (no friction) until the near hog line.</summary>
    public void BeginFlight()
    {
        Init();
        RockPhysics.BeginFlight(ref state, RockPhysicsSettings.Current);
        body.linearDamping = 0f;
        body.angularDamping = 0f;
    }

    /// <summary>The rock left the near hog line: friction and spin start. launchSpeed picks the curl strength.</summary>
    public void Release(bool inTurn, float minLaunchSpeed, float maxLaunchSpeed)
    {
        Init();
        var p = RockPhysicsSettings.Current;
        p.minLaunchSpeed = minLaunchSpeed;
        p.maxLaunchSpeed = maxLaunchSpeed;
        state.velocity = body.linearVelocity;
        state.position = body.position;
        RockPhysics.Release(ref state, p, inTurn);

        RockState ahead = state;
        for (int i = 0; i < 3000 && !ahead.asleep; i++) RockPhysics.Step(ref ahead, p, Time.fixedDeltaTime);
        PredictedRest = ahead.position;
    }

    void FixedUpdate()
    {
        if (!ready || body == null || body.bodyType != RigidbodyType2D.Dynamic) return;

        state.position = body.position;
        state.velocity = body.linearVelocity;
        state.spin = body.angularVelocity;

        RockPhysics.Step(ref state, RockPhysicsSettings.Current, Time.fixedDeltaTime, integratePosition: false);

        body.linearVelocity = state.velocity;
        body.angularVelocity = state.spin;
    }
}
