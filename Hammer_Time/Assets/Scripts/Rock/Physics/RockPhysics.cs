using UnityEngine;

/// <summary>How a sliding rock loses speed.</summary>
public enum FrictionLaw
{
    /// <summary>The original rock: Box2D-style exponential drag (speed falls in proportion to speed) and spin that fades with time.
    /// Kept so the parity tests can prove the new law reproduces the old paths.</summary>
    LegacyExponential,
    /// <summary>A soft landing: deceleration = strength * speed^power (power below 1), so the rock slows quickly at first, eases
    /// off as it slows and comes to rest smoothly in finite time (no abrupt stop, no endless creep). The sideways curl fades with
    /// speed too. Distances, stopping points and the whole path have closed forms, so the aim line is exact.</summary>
    SoftStop,
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
/// Profiles: Legacy() = the rock as it played before; Arcade() = soft-stop friction at the game's pace (same paths, same
/// weights, hog-to-hog time of the old rock, but it stops cleanly in ~10 s instead of creeping for 19); Real() = the constant-deceleration
/// shape stretched in time so a button draw takes ~14.5 s hog to hog like real ice. Every rate in a profile scales together under
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

    [Header("Soft-stop law (sliding rock)")]
    [Tooltip("Deceleration = stopStrength * speed^stopPower. 0 = constant (abrupt stop), 1 = exponential (endless creep); 0.6 lands softly.")]
    public float stopPower = 0.6f;
    [Tooltip("Set so the button draw takes the old rock's 3.28 s hog to hog (StopStrengthFor(stopPower)).")]
    public float stopStrength = 0.5174f;
    [Tooltip("The sideways pull fades as speed^lateralFadePower (the spin of a slowing rock).")]
    public float lateralFadePower = 0.4f;
    [Tooltip("Sideways acceleration (units/s^2) = curlStrength * CurlMagnitude(weight) * weight^curlWeightExponent, fitted to the old rock's paths.")]
    public float curlStrength = 0.5603f;
    public float curlWeightExponent = -0.55f;
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

    /// <summary>The game's pace, soft-stop friction (stopPower 0.6). Same weights, same paths as the old rock.</summary>
    public static RockPhysicsProfile Arcade() { return Arcade(0.6f); }

    // Sideways-pull constants fitted offline to the old rock's paths for each stop curve (all within 2.6 cm at every weight).
    static readonly float[] FitPower = { 0.5f, 0.6f, 0.7f, 0.8f };
    static readonly float[] FitFade = { 0.3f, 0.4f, 0.5f, 0.6f };
    static readonly float[] FitStrength = { 0.6548f, 0.5603f, 0.4685f, 0.3807f };
    static readonly float[] FitExponent = { -0.646f, -0.55f, -0.44f, -0.311f };

    /// <summary>
    /// Soft-stop at the game's pace with a chosen stop curve: 0.5 firm (button draw rests ~9 s after the hog), 0.6 soft (~10 s),
    /// 0.7 mellow (~12 s), 0.8 extra mellow (~15 s). Powers between the fitted values are interpolated.
    /// </summary>
    public static RockPhysicsProfile Arcade(float stopPower)
    {
        var p = new RockPhysicsProfile();
        p.law = FrictionLaw.SoftStop;
        p.stopPower = Mathf.Clamp(stopPower, FitPower[0], FitPower[FitPower.Length - 1]);
        p.stopStrength = StopStrengthFor(p.stopPower);
        int i = Mathf.Clamp(Mathf.FloorToInt((p.stopPower - FitPower[0]) / 0.1f + 1e-4f), 0, FitPower.Length - 2);
        float t = Mathf.InverseLerp(FitPower[i], FitPower[i + 1], p.stopPower);
        p.lateralFadePower = Mathf.Lerp(FitFade[i], FitFade[i + 1], t);
        p.curlStrength = Mathf.Lerp(FitStrength[i], FitStrength[i + 1], t);
        p.curlWeightExponent = Mathf.Lerp(FitExponent[i], FitExponent[i + 1], t);
        return p;
    }

    /// <summary>
    /// Real-ice pace: a button draw takes about 14.5 s hog to hog. Real ice slows at a roughly constant rate (stopPower 0) and at this
    /// pace the end of the slide is far too slow to look abrupt, so Real is the constant-deceleration shape stretched in time. It follows
    /// the same paths as Arcade (both are fitted to the old rock) but its timing is real: hog to rest takes ~30 s for a button draw.
    /// </summary>
    public static RockPhysicsProfile Real() { return RealPaceBase().WithTimeStretch(RealTimeStretch); }

    /// <summary>The constant-deceleration shape at game pace (before the time stretch): fitted to the old rock's paths like Arcade.</summary>
    public static RockPhysicsProfile RealPaceBase()
    {
        var p = Arcade();
        p.stopPower = 0f;
        p.stopStrength = 0.929f;
        p.lateralFadePower = 0f;
        p.curlStrength = 1.335f;
        p.curlWeightExponent = -1f;
        return p;
    }

    /// <summary>How many times slower Real() is than its game-pace base: 14.5 s real hog-to-hog / 3.28 s for the game-pace button draw.</summary>
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
        // dv/dt = -k v^p scales as k' = k * S^(p-2); the sideways acceleration scales as 1/S^2
        p.stopStrength = stopStrength * Mathf.Pow(s, stopPower - 2f);
        p.curlStrength = curlStrength / (s * s);
        p.restDryDecel = restDryDecel / (s * s);
        p.restViscous = restViscous / s;
        p.restSpeed = restSpeed / s;
        p.sleepLinearTolerance = sleepLinearTolerance / s;
        p.sleepAngularTolerance = sleepAngularTolerance / s;
        return p;
    }

    public bool IsLegacy { get { return law == FrictionLaw.LegacyExponential; } }

    /// <summary>
    /// The stopStrength that makes the button draw (weight 8.5, 22.37 from the near hog) cross the 16.15 between the hog lines in the
    /// old rock's 3.28 s, for a given power (closed form of dv/dt = -k v^p).
    /// </summary>
    public static float StopStrengthFor(float power)
    {
        const float D = 22.37f, H = 16.15f, T = 3.28f;
        float m = (1f - power) / (2f - power);
        float num = Mathf.Pow(2f - power, m) * (Mathf.Pow(D, m) - Mathf.Pow(D - H, m));
        return Mathf.Pow(num / ((1f - power) * T), 2f - power);
    }

    // ---- weights <-> speeds ----------------------------------------------------------------------------------------

    /// <summary>Speed (this profile's units) a rock released at the near hog needs to travel as far as a shot of this weight.</summary>
    public float LaunchSpeedFromWeight(float weight)
    {
        if (IsLegacy) return weight;
        float distance = Mathf.Max(0.01f, stopDistanceSlope * weight + stopDistanceOffset);
        return Mathf.Pow(stopStrength * (2f - stopPower) * distance, 1f / (2f - stopPower));
    }

    /// <summary>The weight a rock released at this speed has (inverse of LaunchSpeedFromWeight).</summary>
    public float WeightFromLaunchSpeed(float speed)
    {
        if (IsLegacy) return speed;
        float distance = StopDistance(speed);
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
        return IsLegacy ? launchSpeed / linearDamping : Mathf.Pow(Mathf.Max(0f, launchSpeed), 2f - stopPower) / (stopStrength * (2f - stopPower));
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

    /// <summary>Sideways acceleration (units/s^2) at release for a shot of this weight under the soft-stop law; it then fades with speed.</summary>
    public float CurlAcceleration(float weight)
    {
        return curlStrength * CurlMagnitude(weight) * Mathf.Pow(Mathf.Max(weight, 1f), curlWeightExponent);
    }
}

/// <summary>Everything about one rock's motion that the model needs between steps.</summary>
public struct RockState
{
    public Vector2 position;
    public Vector2 velocity;
    /// <summary>Spin, degrees/s (Rigidbody2D.angularVelocity units). Legacy law: drives curl. Soft-stop law: cosmetic only.</summary>
    public float spin;

    /// <summary>True from the throw until the rock crosses the near hog line: no friction, no curl.</summary>
    public bool freeFlight;
    /// <summary>True from the near hog line on: curling.</summary>
    public bool curlActive;
    public bool turnPending;
    public int dirMult;
    /// <summary>Legacy law: curl coefficient (negative = left for dirMult +1).</summary>
    public float curlX;
    /// <summary>Soft-stop law: sideways acceleration at release, units/s^2 (set from the weight and turn; fades with speed).</summary>
    public float curlAccel;
    /// <summary>Soft-stop law: the speed at the near hog line; the sideways pull fades as (speed / releaseSpeed)^lateralFadePower.</summary>
    public float releaseSpeed;
    /// <summary>Scales curl (sweeping changes it). 1 = normal.</summary>
    public float curlMultiplier;

    /// <summary>Legacy law drag values. Sweeping moves these; they start from the profile at release.</summary>
    public float linearDamping;
    public float angularDamping;
    /// <summary>Soft-stop law: scales the friction (sweeping lowers it). 1 = normal.</summary>
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

    /// <summary>
    /// Soft-stop law: the spin a thrown rock is drawn with (degrees/s, same direction as its curl). It carries no force, but it does
    /// enter the contact friction when the rock hits another, so the live rock and the simulator both read it from here.
    /// </summary>
    public static float CosmeticSpin(RockState s, RockPhysicsProfile p)
    {
        if (!s.curlActive || s.releaseSpeed <= 0f) return 0f;
        return s.dirMult * p.spinOnRelease * Mathf.Clamp01(s.velocity.magnitude / s.releaseSpeed);
    }

    /// <summary>
    /// Soft-stop law: spin of a rock nobody is turning any more (one that was hit): it fades with the rock's speed, so it stops when the rock does
    /// (Box2D has no angular drag here, and a spinning rock would otherwise never go to sleep).
    /// </summary>
    public static float FadedSpin(float spin, float speedBefore, float speedAfter)
    {
        if (speedAfter <= 0f) return 0f;
        if (speedBefore <= 0f) return spin;
        return spin * Mathf.Min(1f, speedAfter / speedBefore);
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
        s.releaseSpeed = Mathf.Max(0.0001f, s.velocity.magnitude);
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
        else StepSoftStop(ref s, p, dt);

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

    static void StepSoftStop(ref RockState s, RockPhysicsProfile p, float dt)
    {
        s.turnPending = false;
        if (s.freeFlight) return;

        float speed = s.velocity.magnitude;
        if (speed < p.restSpeed) s.atRest = true;

        // curl: a sideways pull chosen at release that fades with the rock's speed (a rock that was only hit has none)
        if (s.curlActive && s.releaseSpeed > 0f)
        {
            float fade = Mathf.Pow(Mathf.Clamp(speed / s.releaseSpeed, 0f, 1.5f), p.lateralFadePower);
            s.velocity.x += s.curlMultiplier * s.curlAccel * fade * dt;
        }

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
            slowed = speed - p.stopStrength * s.frictionScale * Mathf.Pow(speed, p.stopPower) * dt;
        }

        if (slowed < 0.5f * p.restSpeed)
        {
            s.velocity = Vector2.zero;
            s.spin = 0f;
            s.atRest = true;
            s.curlActive = false;
        }
        else
        {
            s.velocity *= slowed / speed;
        }
    }
}
