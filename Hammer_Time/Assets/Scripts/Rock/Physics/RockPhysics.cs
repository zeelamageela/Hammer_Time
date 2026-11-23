using UnityEngine;

/// <summary>
/// Every number that decides how a rock slides, in one place. The live rock (Rock_Force) and the trajectory
/// simulator (TrajectorySimulator, used by the aim preview and the AI) both read the same profile and call the
/// same RockPhysics.Step, so a predicted path and a thrown path cannot drift apart.
///
/// Arcade() reproduces the rock exactly as it played before this class existed (verified against recorded
/// trajectories in RockPhysicsParityTests).
/// </summary>
[System.Serializable]
public class RockPhysicsProfile
{
    [Header("Sliding friction (after the near hog line)")]
    [Tooltip("Viscous drag, 1/s: speed lost is proportional to speed. This is the old Rigidbody linearDamping.")]
    public float linearDamping = 0.38f;
    [Tooltip("Dry friction, units/s^2: a constant slowing regardless of speed (real ice behaves mostly like this). 0 = off.")]
    public float constantDecel = 0f;

    [Header("Curl")]
    [Tooltip("Spin (deg/s) given at the near hog line.")]
    public float spinOnRelease = 341.95f;
    [Tooltip("How fast spin fades, 1/s.")]
    public float angularDamping = 0.32f;
    [Tooltip("Sideways force = curl * spin * curlScale (Rock_Force.scaleFactor).")]
    public float curlScale = 0.1f;

    [Header("Body")]
    public float mass = 145f;

    [Header("Stopping")]
    [Tooltip("Below this speed the rock is considered at rest and drag jumps to restLinearDamping.")]
    public float restSpeed = 0.01f;
    public float restLinearDamping = 0.90f;
    [Tooltip("Box2D puts a body to sleep after this long below the sleep tolerances (Physics2D settings).")]
    public float sleepSeconds = 0.5f;
    public float sleepLinearTolerance = 0.01f;
    public float sleepAngularTolerance = 2f;

    [Header("Launch speed range (TrajectoryLine) - used to pick the curl strength")]
    public float minLaunchSpeed = 5f;
    public float maxLaunchSpeed = 16f;

    /// <summary>The rock as it has always played.</summary>
    public static RockPhysicsProfile Arcade() { return new RockPhysicsProfile(); }

    /// <summary>
    /// Curl strength by launch speed: slow rocks curl more, fast rocks less. (Moved verbatim from Rock_Force.Release,
    /// including its 0.33 divisor, so existing shots behave identically.)
    /// </summary>
    public float CurlMagnitude(float launchSpeed)
    {
        float ratio = Mathf.Clamp01((launchSpeed - minLaunchSpeed) / (maxLaunchSpeed - minLaunchSpeed));
        if (ratio < 0.4f) return Mathf.Lerp(0.55f, 0.48f, ratio / 0.33f);
        if (ratio < 0.66f) return Mathf.Lerp(0.48f, 0.21f, (ratio - 0.33f) / 0.33f);
        return Mathf.Lerp(0.21f, 0.12f, (ratio - 0.66f) / 0.34f);
    }
}

/// <summary>Everything about one rock's motion that the model needs between steps.</summary>
public struct RockState
{
    public Vector2 position;
    public Vector2 velocity;
    /// <summary>Spin, degrees/s (Rigidbody2D.angularVelocity units).</summary>
    public float spin;

    /// <summary>True from the throw until the rock crosses the near hog line: no friction, no curl.</summary>
    public bool freeFlight;
    /// <summary>True from the near hog line on: spin is on and curling.</summary>
    public bool curlActive;
    public bool turnPending;
    public int dirMult;
    public float curlX;
    /// <summary>Scales curl (sweeping changes it). 1 = normal.</summary>
    public float curlMultiplier;

    /// <summary>Current drag values. Sweeping moves these; they start from the profile at release.</summary>
    public float linearDamping;
    public float angularDamping;

    public bool atRest;
    public bool asleep;
    public float sleepTimer;
}

public static class RockPhysics
{
    /// <summary>
    /// Where the rock's centre is when it leaves the near-hog trigger: trigger top -16.025 + rock radius 0.14 + the
    /// physics contact offset 0.01 (the overlap lasts that much longer). Friction and spin begin here. Measured:
    /// a rock whose centre is at -15.880 has not left yet, one at -15.824 has.
    /// </summary>
    public const float ReleaseY = -15.875f;

    /// <summary>A rock sitting on the ice: ordinary friction, no curl. Struck rocks behave like this.</summary>
    public static RockState NewRock(RockPhysicsProfile p)
    {
        return new RockState { curlMultiplier = 1f, linearDamping = p.linearDamping, angularDamping = p.angularDamping };
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
        s.linearDamping = p.linearDamping;
        s.angularDamping = p.angularDamping;
    }

    /// <summary>A thrown rock at the hack with a given velocity (what the simulator and the tests start from).</summary>
    public static RockState Launch(Vector2 position, Vector2 velocity, RockPhysicsProfile p)
    {
        var s = NewRock(p);
        s.position = position;
        s.velocity = velocity;
        BeginFlight(ref s, p);
        return s;
    }

    /// <summary>The rock has just left the near hog trigger: start sliding friction and put the spin on it.</summary>
    public static void Release(ref RockState s, RockPhysicsProfile p, bool inTurn)
    {
        s.freeFlight = false;
        s.curlActive = true;
        s.atRest = false;
        s.asleep = false;
        s.sleepTimer = 0f;
        s.linearDamping = p.linearDamping;
        s.angularDamping = p.angularDamping;
        s.dirMult = inTurn ? -1 : 1;
        s.curlX = -p.CurlMagnitude(s.velocity.magnitude);
        s.turnPending = true;
        if (s.curlMultiplier == 0f) s.curlMultiplier = 1f;
    }

    /// <summary>
    /// One fixed step of free sliding (no collisions). Order matches the old Rock_Force.FixedUpdate followed by
    /// Box2D: spin read, torque on the first step, rest check, curl force, then drag, then position.
    /// integratePosition is false when a Rigidbody2D moves the rock itself.
    /// </summary>
    public static void Step(ref RockState s, RockPhysicsProfile p, float dt, bool integratePosition = true)
    {
        if (s.asleep) return;

        float spinRead = s.spin;
        if (s.turnPending)
        {
            s.spin += s.dirMult * p.spinOnRelease;
            s.turnPending = false;
        }

        if (!s.freeFlight)
        {
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
            if (p.constantDecel > 0f)
            {
                float speed = s.velocity.magnitude;
                float slowed = Mathf.Max(0f, speed - p.constantDecel * dt);
                s.velocity = speed > 0f ? s.velocity * (slowed / speed) : s.velocity;
            }
            s.spin /= 1f + s.angularDamping * dt;
        }

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
}
