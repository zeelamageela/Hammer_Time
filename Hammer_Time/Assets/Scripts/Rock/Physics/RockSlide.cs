using UnityEngine;

/// <summary>The profile the whole game is using right now. The live rocks and the trajectory simulator both read it.</summary>
public static class RockPhysicsSettings
{
    public static RockPhysicsProfile Current = RockPhysicsProfile.Legacy();

    /// <summary>Raised after Use(); listeners (UI, tools) can refresh.</summary>
    public static event System.Action Changed;

    /// <summary>
    /// Switch profile. Do this between shots: a rock that is mid-slide keeps whatever it was doing under the old profile's numbers.
    /// Also moves Box2D's sleep tolerances so a rock goes to sleep at the same point of its slide at any pace.
    /// </summary>
    public static void Use(RockPhysicsProfile profile)
    {
        Current = profile;
        Physics2D.linearSleepTolerance = profile.sleepLinearTolerance;
        Physics2D.angularSleepTolerance = profile.sleepAngularTolerance;
        if (Changed != null) Changed();
    }

    /// <summary>
    /// A speed written in the game's original units (a threshold like "the rock is nearly stopped: 0.1"), converted to this
    /// profile's pace. Arcade is the same pace as the old rock; Real is slower by the time stretch.
    /// </summary>
    public static float Speed(float originalUnits) { return originalUnits / Current.timeStretch; }
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
    /// <summary>Sweeping talks in the original units: these are the drag values the sweep amounts were written against.</summary>
    public const float NominalLinearDamping = 0.38f;
    public const float NominalAngularDamping = 0.32f;

    Rigidbody2D body;
    RockState state;
    bool ready;
    float externalCurlMultiplier = 1f;
    float angularCurlFactor = 1f;
    float releaseSpeed = 1f;

    /// <summary>
    /// Sweeping's friction handle, in the original drag units (0.38 = normal; sweeping subtracts from it). Under the
    /// constant-decel law this scales the rock's friction by value / 0.38, so a sweep takes the same share off the friction as before.
    /// </summary>
    public float LinearDamping
    {
        get { Init(); return Law.IsLegacy ? state.linearDamping : state.frictionScale * NominalLinearDamping; }
        set { Init(); if (Law.IsLegacy) state.linearDamping = value; else state.frictionScale = Mathf.Max(0.05f, value / NominalLinearDamping); }
    }

    /// <summary>
    /// Sweeping's curl handle, in the original angular-drag units (0.32 = normal; higher straightens the rock, lower curls it more).
    /// Under the constant-decel law there is no spin to fade; the old model's total curl was proportional to 1 / angularDamping, so that is the curl factor.
    /// </summary>
    public float AngularDamping
    {
        get { Init(); return Law.IsLegacy ? state.angularDamping : NominalAngularDamping / Mathf.Max(0.01f, angularCurlFactor); }
        set { Init(); if (Law.IsLegacy) state.angularDamping = value; else angularCurlFactor = NominalAngularDamping / Mathf.Max(0.05f, value); ApplyCurl(); }
    }

    /// <summary>Scales curl (1 = normal). Rock_Force sets it each step from its multiplier and the sweepers.</summary>
    public float CurlMultiplier
    {
        get { Init(); return externalCurlMultiplier; }
        set { Init(); externalCurlMultiplier = value; ApplyCurl(); }
    }

    public bool AtRest { get { Init(); return state.atRest; } }
    public Vector2 Curl { get { Init(); return Law.IsLegacy ? new Vector2(state.curlX, 0f) : new Vector2(state.curlAccel, 0f); } }
    /// <summary>Where the model says this rock will stop, worked out from the state it actually had at the hog line (for ShotAudit).</summary>
    public Vector2 PredictedRest { get; private set; }
    public float SpinNow { get { Init(); return state.spin; } }
    /// <summary>Launch speed per unit of weight for the rock last released (1 for the legacy law). Sweeps that push the rock sideways use it to stay in proportion.</summary>
    public float SpeedPerWeight { get; private set; }

    static RockPhysicsProfile Law { get { return RockPhysicsSettings.Current; } }

    void ApplyCurl() { state.curlMultiplier = externalCurlMultiplier * (Law.IsLegacy ? 1f : angularCurlFactor); }

    void Awake() { Init(); }

    void Init()
    {
        if (ready) return;
        body = GetComponent<Rigidbody2D>();
        state = RockPhysics.NewRock(RockPhysicsSettings.Current);
        SpeedPerWeight = 1f;
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
        externalCurlMultiplier = 1f;
        angularCurlFactor = 1f;
        body.linearDamping = 0f;
        body.angularDamping = 0f;
    }

    /// <summary>The rock left the near hog line: friction and curl start.</summary>
    public void Release(bool inTurn, float minWeight, float maxWeight)
    {
        Init();
        var p = RockPhysicsSettings.Current;
        p.minLaunchSpeed = minWeight;
        p.maxLaunchSpeed = maxWeight;
        state.velocity = body.linearVelocity;
        state.position = body.position;
        RockPhysics.Release(ref state, p, inTurn);
        ApplyCurl();

        releaseSpeed = Mathf.Max(0.0001f, state.velocity.magnitude);
        float weight = p.WeightFromLaunchSpeed(releaseSpeed);
        SpeedPerWeight = p.IsLegacy ? 1f : p.LaunchSpeedFromWeight(weight) / Mathf.Max(0.01f, weight);

        RockState ahead = state;
        for (int i = 0; i < 6000 && !ahead.asleep; i++) RockPhysics.Step(ref ahead, p, Time.fixedDeltaTime);
        PredictedRest = ahead.position;
    }

    /// <summary>
    /// The rock's current state for the simulator (the AI sweeper re-predicts from it mid-flight). Unswept by default:
    /// the AI asks "where would it stop if nobody sweeps?".
    /// </summary>
    public RockState Snapshot(bool unswept = true)
    {
        Init();
        RockState s = state;
        s.position = body.position;
        s.velocity = body.linearVelocity;
        if (unswept)
        {
            s.frictionScale = 1f;
            s.curlMultiplier = 1f;
            s.linearDamping = Law.linearDamping;
            s.angularDamping = Law.angularDamping;
        }
        return s;
    }

    void FixedUpdate()
    {
        if (!ready || body == null || body.bodyType != RigidbodyType2D.Dynamic) return;

        state.position = body.position;
        state.velocity = body.linearVelocity;
        state.spin = body.angularVelocity;

        var p = RockPhysicsSettings.Current;
        RockPhysics.Step(ref state, p, Time.fixedDeltaTime, integratePosition: false);

        body.linearVelocity = state.velocity;
        if (p.IsLegacy)
        {
            body.angularVelocity = state.spin;
        }
        else if (state.curlActive)
        {
            // Spin is only for looks under the constant-decel law: it fades as the rock slows, and stops with it.
            body.angularVelocity = -state.dirMult * 342f * Mathf.Clamp01(state.velocity.magnitude / releaseSpeed) / p.timeStretch;
        }
    }
}
