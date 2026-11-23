using UnityEngine;

/// <summary>How a sliding rock loses speed.</summary>
public enum FrictionLaw
{
    /// <summary>The original rock: Box2D-style exponential drag (speed falls in proportion to speed) and spin that fades with time.
    /// Kept so the parity tests can prove the new law reproduces the old paths.</summary>
    LegacyExponential,
    /// <summary>Real-ice style: a constant deceleration, and a constant sideways (curl) acceleration set by the shot's weight.
    /// A rock under constant forces follows a parabola, so where it stops can be worked out exactly.</summary>
    ConstantDecel,
}

/// <summary>
/// Every number that decides how a rock slides, in one place. The live rock (RockSlide) and the trajectory
/// simulator (TrajectorySimulator, used by the aim preview and the AI) both read the same profile and call the
/// same RockPhysics.Step, so a predicted path and a thrown path cannot drift apart.
///
/// Shots are described by their WEIGHT (the launch speed the old physics used: 5 light draw ... 16 hard takeout).
/// A weight means "travels this far", so LaunchSpeedFromWeight converts it to the speed this profile needs; the game's
/// aiming, the AI's shot choices and the tutorial all keep speaking weights.
///
/// Profiles: Legacy() = the rock as it played before; Arcade() = constant-decel at the game's pace (same paths, same
/// weights, hog-to-hog time of the old rock, but it stops in ~7 s instead of creeping for 19); Real() = Arcade stretched
/// in time so a button draw takes ~14.5 s hog to hog like real ice. Every rate in a profile scales together under
/// WithTimeStretch, so the path (not the timing) is identical.
/// </summary>
[System.Serializable]
public class RockPhysicsProfile
{
    public FrictionLaw law = FrictionLaw.LegacyExponential;
    [Tooltip("1 = game pace. Real() sets this so reports/tools know the profile was stretched.")]
    public float timeStretch = 1f;

    [Header("Legacy exponential law")]
    [Tooltip("Viscous drag, 1/s: speed lost is proportional to speed. This is the old Rigidbody linearDamping.")]
    public float linearDamping = 0.38f;
    [Tooltip("Spin (deg/s) given at the near hog line.")]
    public float spinOnRelease = 341.95f;
    [Tooltip("How fast spin fades, 1/s.")]
    public float angularDamping = 0.32f;
    [Tooltip("Sideways force = curl * spin * curlScale (Rock_Force.scaleFactor).")]
    public float curlScale = 0.1f;
    public float restLinearDamping = 0.90f;

    [Header("Constant-decel law (sliding rock)")]
    [Tooltip("Constant slowing, units/s^2.")]
    public float dryDecel = 0.929f;
    [Tooltip("Sideways acceleration (units/s^2) = curlStrength * CurlMagnitude(weight) / weight.")]
    public float curlStrength = 1.335f;
    [Tooltip("Weight <-> stopping distance from the near hog: distance = slope * weight + offset (matches the old rock, whose distance was weight / 0.38).")]
    public float stopDistanceSlope = 2.6430f;
    public float stopDistanceOffset = -0.117f;

    [Header("Constant-decel law (a rock that has been sitting still and gets hit)")]
    public float restDryDecel = 0.15f;
    public float restViscous = 0.90f;

    [Header("Body")]
    public float mass = 145f;

    [Header("Stopping")]
    [Tooltip("Below this speed the rock is considered at rest.")]
    public float restSpeed = 0.01f;
    [Tooltip("Box2D puts a body to sleep after this long below the sleep tolerances (Physics2D settings).")]
    public float sleepSeconds = 0.5f;
    public float sleepLinearTolerance = 0.01f;
    public float sleepAngularTolerance = 2f;

    [Header("Weight range (TrajectoryLine) - picks the curl strength")]
    public float minLaunchSpeed = 5f;
    public float maxLaunchSpeed = 16f;

    public static RockPhysicsProfile Legacy() { return new RockPhysicsProfile(); }

    /// <summary>The game's pace, constant-decel friction. Same weights, same paths as the old rock.</summary>
    public static RockPhysicsProfile Arcade()
    {
        var p = new RockPhysicsProfile();
        p.law = FrictionLaw.ConstantDecel;
        return p;
    }

    /// <summary>Real-ice pace: a button draw takes about 14.5 s hog to hog. Paths are identical to Arcade().</summary>
    public static RockPhysicsProfile Real() { return Arcade().WithTimeStretch(RealTimeStretch); }

    /// <summary>How many times slower Real() is than Arcade(): 14.5 s real hog-to-hog / 3.28 s for the Arcade button draw.</summary>
    public const float RealTimeStretch = 4.42f;

    /// <summary>
    /// The same rock, S times slower. Dimensional analysis: x(t) -> X(t/S), so speeds /S, accelerations /S^2, rates /S.
    /// Distances, weights and paths are unchanged.
    /// </summary>
    public RockPhysicsProfile WithTimeStretch(float s)
    {
        var p = (RockPhysicsProfile)MemberwiseClone();
        p.timeStretch = timeStretch * s;
        p.linearDamping = linearDamping / s;
        p.angularDamping = angularDamping / s;
        p.spinOnRelease = spinOnRelease / s;
        p.curlScale = curlScale / s;
        p.restLinearDamping = restLinearDamping / s;
        p.dryDecel = dryDecel / (s * s);
        p.curlStrength = curlStrength / (s * s);
        p.restDryDecel = restDryDecel / (s * s);
        p.restViscous = restViscous / s;
        p.restSpeed = restSpeed / s;
        p.sleepLinearTolerance = sleepLinearTolerance / s;
        p.sleepAngularTolerance = sleepAngularTolerance / s;
        return p;
    }

    public bool IsLegacy { get { return law == FrictionLaw.LegacyExponential; } }

    // ---- weights <-> speeds ----------------------------------------------------------------------------------------

    /// <summary>Speed (this profile's units) a rock released at the near hog needs to travel as far as a shot of this weight.</summary>
    public float LaunchSpeedFromWeight(float weight)
    {
        if (IsLegacy) return weight;
        float distance = Mathf.Max(0.01f, stopDistanceSlope * weight + stopDistanceOffset);
        return Mathf.Sqrt(2f * dryDecel * distance);
    }

    /// <summary>The weight a rock released at this speed has (inverse of LaunchSpeedFromWeight).</summary>
    public float WeightFromLaunchSpeed(float speed)
    {
        if (IsLegacy) return speed;
        float distance = speed * speed / (2f * dryDecel);
        return (distance - stopDistanceOffset) / stopDistanceSlope;
    }

    /// <summary>Keeps a velocity's direction, replaces its length: weight -> speed.</summary>
    public Vector2 LaunchVelocityFromWeightVelocity(Vector2 weightVelocity)
    {
        float w = weightVelocity.magnitude;
        return w < 1e-6f ? weightVelocity : weightVelocity * (LaunchSpeedFromWeight(w) / w);
    }

    /// <summary>Keeps a velocity's direction, replaces its length: speed -> weight.</summary>
    public Vector2 WeightVelocityFromLaunchVelocity(Vector2 launchVelocity)
    {
        float s = launchVelocity.magnitude;
        return s < 1e-6f ? launchVelocity : launchVelocity * (WeightFromLaunchSpeed(s) / s);
    }

    /// <summary>Distance a free-sliding rock travels after the near hog at this speed (constant-decel law).</summary>
    public float StopDistance(float launchSpeed)
    {
        return IsLegacy ? launchSpeed / linearDamping : launchSpeed * launchSpeed / (2f * dryDecel);
    }

    // ---- curl ------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Curl strength by weight: slow rocks curl more, fast rocks less. (The original table from Rock_Force.Release, including
    /// its 0.33 divisor, so existing shots behave identically; legacy used it as a force coefficient on the speed.)
    /// </summary>
    public float CurlMagnitude(float weight)
    {
        float ratio = Mathf.Clamp01((weight - minLaunchSpeed) / (maxLaunchSpeed - minLaunchSpeed));
        if (ratio < 0.4f) return Mathf.Lerp(0.55f, 0.48f, ratio / 0.33f);
        if (ratio < 0.66f) return Mathf.Lerp(0.48f, 0.21f, (ratio - 0.33f) / 0.33f);
        return Mathf.Lerp(0.21f, 0.12f, (ratio - 0.66f) / 0.34f);
    }

    /// <summary>Constant sideways acceleration (units/s^2) for a shot of this weight under the constant-decel law.</summary>
    public float CurlAcceleration(float weight)
    {
        return curlStrength * CurlMagnitude(weight) / Mathf.Max(weight, 1f);
    }
}

/// <summary>Everything about one rock's motion that the model needs between steps.</summary>
public struct RockState
{
    public Vector2 position;
    public Vector2 velocity;
    /// <summary>Spin, degrees/s (Rigidbody2D.angularVelocity units). Legacy law: drives curl. Constant-decel law: cosmetic only.</summary>
    public float spin;

    /// <summary>True from the throw until the rock crosses the near hog line: no friction, no curl.</summary>
    public bool freeFlight;
    /// <summary>True from the near hog line on: curling.</summary>
    public bool curlActive;
    public bool turnPending;
    public int dirMult;
    /// <summary>Legacy law: curl coefficient (negative = left for dirMult +1).</summary>
    public float curlX;
    /// <summary>Constant-decel law: sideways acceleration, units/s^2 (set at release from the weight and turn).</summary>
    public float curlAccel;
    /// <summary>Scales curl (sweeping changes it). 1 = normal.</summary>
    public float curlMultiplier;

    /// <summary>Legacy law drag values. Sweeping moves these; they start from the profile at release.</summary>
    public float linearDamping;
    public float angularDamping;
    /// <summary>Constant-decel law: scales the friction (sweeping lowers it). 1 = normal.</summary>
    public float frictionScale;

    public bool atRest;
    public bool asleep;
    public float sleepTimer;
}

public static class RockPhysics
{
    /// <summary>
    /// Where the rock's centre is when it leaves the near-hog trigger: trigger top -16.025 + rock radius 0.14 + the
    /// physics contact offset 0.01 (the overlap lasts that much longer). Friction and curl begin here. Measured:
    /// a rock whose centre is at -15.880 has not left yet, one at -15.824 has.
    /// </summary>
    public const float ReleaseY = -15.875f;

    /// <summary>A rock sitting on the ice: ordinary friction, no curl. Struck rocks behave like this.</summary>
    public static RockState NewRock(RockPhysicsProfile p)
    {
        return new RockState { curlMultiplier = 1f, frictionScale = 1f, linearDamping = p.linearDamping, angularDamping = p.angularDamping };
    }

    /// <summary>A rock that has come to rest on the ice (at-rest drag): what a rock is when another rock hits it.</summary>
    public static RockState NewRestingRock(RockPhysicsProfile p)
    {
        var s = NewRock(p);
        s.atRest = true;
        s.linearDamping = p.restLinearDamping;
        return s;
    }

    /// <summary>The rock leaves the hack: it flies free (no friction, no curl) until Release.</summary>
    public static void BeginFlight(ref RockState s, RockPhysicsProfile p)
    {
        s.freeFlight = true;
        s.curlActive = false;
        s.turnPending = false;
        s.atRest = false;
        s.asleep = false;
        s.sleepTimer = 0f;
        s.curlMultiplier = 1f;
        s.frictionScale = 1f;
        s.linearDamping = p.linearDamping;
        s.angularDamping = p.angularDamping;
    }

    /// <summary>
    /// A thrown rock at the hack. `launchVelocity` is the real velocity (use p.LaunchVelocityFromWeightVelocity to convert
    /// a weight-style velocity first).
    /// </summary>
    public static RockState Launch(Vector2 position, Vector2 launchVelocity, RockPhysicsProfile p)
    {
        var s = NewRock(p);
        s.position = position;
        s.velocity = launchVelocity;
        BeginFlight(ref s, p);
        return s;
    }

    /// <summary>The rock has just left the near hog trigger: friction and curl start.</summary>
    public static void Release(ref RockState s, RockPhysicsProfile p, bool inTurn)
    {
        s.freeFlight = false;
        s.curlActive = true;
        s.atRest = false;
        s.asleep = false;
        s.sleepTimer = 0f;
        s.linearDamping = p.linearDamping;
        s.angularDamping = p.angularDamping;
        s.frictionScale = 1f;
        s.dirMult = inTurn ? -1 : 1;
        float weight = p.WeightFromLaunchSpeed(s.velocity.magnitude);
        s.curlX = -p.CurlMagnitude(weight);
        s.curlAccel = -s.dirMult * p.CurlAcceleration(weight);
        s.turnPending = true;
        if (s.curlMultiplier == 0f) s.curlMultiplier = 1f;
    }

    /// <summary>
    /// One fixed step of free sliding (no collisions). Legacy law: same order as the old Rock_Force.FixedUpdate followed by
    /// Box2D. Constant-decel law: sideways acceleration, then friction on the speed. integratePosition is false when a
    /// Rigidbody2D moves the rock itself.
    /// </summary>
    public static void Step(ref RockState s, RockPhysicsProfile p, float dt, bool integratePosition = true)
    {
        if (s.asleep) return;

        if (p.IsLegacy) StepLegacy(ref s, p, dt);
        else StepConstantDecel(ref s, p, dt);

        if (integratePosition) s.position += s.velocity * dt;

        // Box2D sleeps a body that stays under both tolerances long enough; it then stops dead. (When a Rigidbody2D
        // is moving the rock, Box2D does that itself, so only the standalone use needs it.)
        if (integratePosition && !s.freeFlight && s.velocity.magnitude < p.sleepLinearTolerance && Mathf.Abs(s.spin) < p.sleepAngularTolerance)
        {
            s.sleepTimer += dt;
            if (s.sleepTimer >= p.sleepSeconds)
            {
                s.velocity = Vector2.zero;
                s.spin = 0f;
                s.asleep = true;
            }
        }
        else
        {
            s.sleepTimer = 0f;
        }
    }

    static void StepLegacy(ref RockState s, RockPhysicsProfile p, float dt)
    {
        float spinRead = s.spin;
        if (s.turnPending)
        {
            s.spin += s.dirMult * p.spinOnRelease;
            s.turnPending = false;
        }

        if (s.freeFlight) return;

        if (Mathf.Abs(s.velocity.x) < p.restSpeed && Mathf.Abs(s.velocity.y) < p.restSpeed)
        {
            s.atRest = true;
            s.linearDamping = p.restLinearDamping;
        }

        if (s.curlActive)
        {
            float curlForceX = s.curlMultiplier * s.curlX * (spinRead * p.curlScale);
            s.velocity.x += curlForceX / p.mass * dt;
        }

        s.velocity /= 1f + s.linearDamping * dt;
        s.spin /= 1f + s.angularDamping * dt;
    }

    static void StepConstantDecel(ref RockState s, RockPhysicsProfile p, float dt)
    {
        s.turnPending = false;
        if (s.freeFlight) return;

        float speed = s.velocity.magnitude;
        if (speed < p.restSpeed) s.atRest = true;

        // curl: a constant sideways acceleration chosen at release (a rock that was only hit has none)
        if (s.curlActive)
            s.velocity.x += s.curlMultiplier * s.curlAccel * dt;

        speed = s.velocity.magnitude;
        if (speed <= 0f) return;

        float slowed;
        if (s.atRest)
        {
            // a rock that was sitting still and got hit: viscous drag plus a little dry friction (keeps the old hit-rock distances)
            slowed = speed / (1f + p.restViscous * dt) - p.restDryDecel * s.frictionScale * dt;
        }
        else
        {
            slowed = speed - p.dryDecel * s.frictionScale * dt;
        }

        if (slowed <= 0f)
        {
            s.velocity = Vector2.zero;
            s.spin = 0f;
            s.atRest = true;
            if (!s.freeFlight) s.curlActive = false;
        }
        else
        {
            s.velocity *= slowed / speed;
        }
    }
}
