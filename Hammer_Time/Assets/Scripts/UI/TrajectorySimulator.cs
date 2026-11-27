using System.Collections.Generic;
using UnityEngine;

public class TrajectorySimulator
{
    // The rock's sliding physics (friction, spin, curl, stopping) lives in RockPhysics and is shared with the live
    // rock (RockSlide), so a predicted path and a thrown path run the same code. Only the stepping and collisions are here.
    private const float FIXED_DT = 0.02f;                 // same step as Unity's FixedUpdate, so the prediction matches the rock
    private const float MAX_SIMULATION_TIME = 30f;        // seconds: a rock is asleep by ~20 s even at the heaviest weights
    private const int STEPS_PER_POINT = 8;                // one trajectory point every 0.16 s (the line renderer and AI don't need more)

    // Rock size (the contact itself is RockCollision, the same arithmetic as the live rocks' Box2D contact)
    private float rockRadius = RockCollision.Radius;
    private const int MAX_HITS = 12;                      // rock-to-rock contacts followed after the first one (chains, grazes)

    // Result data
    public struct CollisionInfo
    {
        public bool hasCollision;
        public int collisionIndex; // Index in trajectory where collision occurs
        public GameObject hitRock;
        public Vector2 collisionPoint; // SURFACE contact point (for visualization)
        public Vector2 shooterCenterAtCollision; // NEW: Shooter's center position at moment of collision (for scoring)
        public Vector2 hitRockCenter; // Where the struck rock sat when it was hit
        public Vector2 shooterVelocityAfter; // Both rocks' velocity just after the contact (profile units): what the hit arrows show
        public Vector2 hitRockVelocityAfter;
        public Vector2 finalPosition; // Where the thrown rock ends up
        public Vector2 hitRockFinalPosition; // Where the hit rock ends up
        public List<Vector2> thrownRockPostCollisionPath; // Full path of thrown rock after collision
        public List<Vector2> hitRockPostCollisionPath; // Full path of hit rock after collision
        public List<KeyValuePair<GameObject, Vector2>> otherRocksMoved; // Other rocks the hit sets moving (a frozen partner, a chain), with where each ends up
    }
    
    public CollisionInfo lastCollisionInfo { get; private set; }
    
    private TrajectoryLine trajLine; // cached: used only for the launch-speed range that picks the curl strength
    
    // Track speed at each trajectory point for visualization
    private List<float> trajectorySpeedList = new List<float>();
    
    /// <param name="friction">UNUSED: friction now comes from RockPhysicsSettings.Current (kept so existing callers still compile).</param>
    /// <param name="curl">UNUSED: curl now comes from RockPhysicsSettings.Current.</param>
    public TrajectorySimulator(float friction, float curl)
    {
    }

    /// <summary>
    /// Simulates the rock's path and returns a list of positions, including collision handling
    /// </summary>
    /// <param name="forPlayerPreview">If true, uses INVERTED curl (for visual trajectory line). If false, uses REAL curl (for AI physics calculation)</param>
    public List<Vector2> SimulateTrajectory(
        Vector2 startPosition, 
        Vector2 initialVelocity, 
        bool isInTurn,
        int maxPoints = 200,
        List<GameObject> rocksInPlay = null,
        bool forPlayerPreview = false)  // no longer changes anything: the preview and the AI now use the one real model
    {
        RockPhysicsProfile profile = PrepareProfile();

        // initialVelocity is a WEIGHT velocity (what the aim, the AI and the tutorial speak); the rock gets the launch speed this
        // profile needs to travel that far. Thrown from the hack: free flight until the near hog line, then the real sliding model.
        RockState rock = RockPhysics.Launch(startPosition, profile.LaunchVelocityFromWeightVelocity(initialVelocity), profile);
        return Simulate(rock, profile, isInTurn, maxPoints, rocksInPlay);
    }

    /// <summary>
    /// Simulate from a rock's real state (position, real velocity, curl as it was set at release): what the AI sweeper uses to
    /// ask "where will this rock stop from here?". Take the state from RockSlide.Snapshot().
    /// </summary>
    public List<Vector2> SimulateTrajectoryFromState(
        RockState rock,
        bool isInTurn,
        int maxPoints = 200,
        List<GameObject> rocksInPlay = null)
    {
        return Simulate(rock, PrepareProfile(), isInTurn, maxPoints, rocksInPlay);
    }

    private RockPhysicsProfile PrepareProfile()
    {
        RockPhysicsProfile profile = RockPhysicsSettings.Current;
        if (trajLine == null) trajLine = GameObject.FindFirstObjectByType<TrajectoryLine>();
        if (trajLine != null)
        {
            profile.minLaunchSpeed = trajLine.minVelocity;
            profile.maxLaunchSpeed = trajLine.maxVelocity;
        }
        return profile;
    }

    private List<Vector2> Simulate(RockState rock, RockPhysicsProfile profile, bool isInTurn, int maxPoints, List<GameObject> rocksInPlay)
    {
        List<Vector2> trajectoryPoints = new List<Vector2>();
        trajectorySpeedList = new List<float>(); // Reset speed tracking

        lastCollisionInfo = new CollisionInfo
        {
            hasCollision = false,
            collisionIndex = -1,
            hitRock = null,
            collisionPoint = Vector2.zero,
            shooterCenterAtCollision = Vector2.zero,
            finalPosition = Vector2.zero,
            hitRockFinalPosition = Vector2.zero,
            thrownRockPostCollisionPath = new List<Vector2>(),
            hitRockPostCollisionPath = new List<Vector2>(),
            otherRocksMoved = new List<KeyValuePair<GameObject, Vector2>>()
        };

        trajectoryPoints.Add(rock.position);
        trajectorySpeedList.Add(profile.WeightFromLaunchSpeed(rock.velocity.magnitude)); // in weight units so the dot sizes look the same at any pace

        float currentTime = 0f;
        int step = 0;
        // A slower profile takes proportionally longer to stop: stretch the time cap and the point spacing with it, so a path has the
        // same number of points and is never cut short.
        float maxTime = MAX_SIMULATION_TIME * profile.timeStretch;
        int stepsPerPoint = Mathf.Max(1, Mathf.RoundToInt(STEPS_PER_POINT * profile.timeStretch));

        while (currentTime < maxTime && trajectoryPoints.Count < maxPoints)
        {
            Vector2 before = rock.position;
            RockPhysics.Step(ref rock, profile, FIXED_DT);
            if (rock.freeFlight && rock.position.y > RockPhysics.ReleaseY)
                RockPhysics.Release(ref rock, profile, isInTurn);
            currentTime += FIXED_DT;
            step++;

            // Collision: did the rock's path this step come within two radii of a stationary rock? (swept test, so a
            // fast rock cannot skip over a rock between steps)
            if (rocksInPlay != null)
            {
                GameObject hitRock = null;
                float earliest = 2f;
                foreach (GameObject candidate in rocksInPlay)
                {
                    if (candidate == null || !candidate.activeInHierarchy)
                        continue;
                    float f;
                    if (SegmentEntersCircle(before, rock.position, candidate.transform.position, rockRadius * 2f, out f) && f < earliest)
                    {
                        earliest = f;
                        hitRock = candidate;
                    }
                }

                if (hitRock != null)
                {
                    Vector2 position = Vector2.Lerp(before, rock.position, earliest);
                    Vector2 rockPos = hitRock.transform.position;

                    // Direction FROM thrown rock TO hit rock; the surfaces touch one radius from the target's centre
                    Vector2 collisionNormal = (rockPos - position).normalized;
                    Vector2 actualContactPoint = rockPos - collisionNormal * rockRadius;

                    // Play the hit out the way the game does: the contact (bounce + friction + spin), then every rock that is moving
                    // keeps sliding and can strike the next one.
                    RockState thrown = rock;
                    thrown.position = position;
                    HitResult result = PlayOutHit(thrown, hitRock, rocksInPlay, profile);

                    lastCollisionInfo = new CollisionInfo
                    {
                        hasCollision = true,
                        collisionIndex = trajectoryPoints.Count,
                        hitRock = hitRock,
                        collisionPoint = actualContactPoint, // Surface contact point (for visualization)
                        shooterCenterAtCollision = position, // Shooter's center at collision (for scoring!)
                        hitRockCenter = rockPos,
                        shooterVelocityAfter = result.thrownVelocityAfterHit,
                        hitRockVelocityAfter = result.hitVelocityAfterHit,
                        finalPosition = result.thrownFinal,
                        hitRockFinalPosition = result.hitFinal,
                        thrownRockPostCollisionPath = result.thrownPath,
                        hitRockPostCollisionPath = result.hitPath,
                        otherRocksMoved = result.others
                    };

                    // Add collision point; the main trajectory stops here - post-collision paths are separate
                    trajectoryPoints.Add(position);
                    trajectorySpeedList.Add(profile.WeightFromLaunchSpeed(result.thrownSpeedAfterHit)); // weight units so the dot sizes look the same at any pace
                    return trajectoryPoints;
                }
            }

            if (step % stepsPerPoint == 0)
            {
                trajectoryPoints.Add(rock.position);
                trajectorySpeedList.Add(profile.WeightFromLaunchSpeed(rock.velocity.magnitude)); // in weight units so the dot sizes look the same at any pace
            }

            if (rock.asleep)
                break;
        }

        // Always end on the exact stopping point
        if (trajectoryPoints[trajectoryPoints.Count - 1] != rock.position)
        {
            trajectoryPoints.Add(rock.position);
            trajectorySpeedList.Add(profile.WeightFromLaunchSpeed(rock.velocity.magnitude)); // in weight units so the dot sizes look the same at any pace
        }

        return trajectoryPoints;
    }

    /// <summary>
    /// Does the straight path from a to b come within `radius` of `center`? If so, f (0..1) is how far along the path
    /// the distance first drops to `radius`.
    /// </summary>
    private static bool SegmentEntersCircle(Vector2 a, Vector2 b, Vector2 center, float radius, out float f)
    {
        f = 0f;
        Vector2 d = b - a;
        Vector2 m = a - center;
        float c = Vector2.Dot(m, m) - radius * radius;
        if (c <= 0f) return true;               // already inside at the start of the step
        float aa = Vector2.Dot(d, d);
        if (aa < 1e-12f) return false;
        float bb = Vector2.Dot(m, d);
        float disc = bb * bb - aa * c;
        if (disc < 0f) return false;
        float t = (-bb - Mathf.Sqrt(disc)) / aa;
        if (t < 0f || t > 1f) return false;
        f = t;
        return true;
    }

    private struct HitResult
    {
        public Vector2 thrownFinal, hitFinal;
        public List<Vector2> thrownPath, hitPath;
        public List<KeyValuePair<GameObject, Vector2>> others;
        public float thrownSpeedAfterHit;
        public Vector2 thrownVelocityAfterHit, hitVelocityAfterHit;
    }

    private class SimBody
    {
        public GameObject go;       // null for the thrown rock
        public RockState s;
        public float spin;          // degrees/s
        public bool thrown;
        public Vector2 start;
        public List<Vector2> path = new List<Vector2>();
        public bool Moving { get { return !s.asleep && s.velocity.sqrMagnitude > 0f; } }
    }

    /// <summary>
    /// The thrown rock has just touched `hitRock`. Resolve that contact with RockCollision (the same bounce, friction and spin Box2D gives the live
    /// rocks), then step every moving rock with the real sliding model and resolve any further contacts the same way, until all have stopped.
    /// A thrown rock keeps curling (its spin carries on); a struck rock only slides, with the at-rest drag.
    /// </summary>
    private HitResult PlayOutHit(RockState thrownState, GameObject hitRock, List<GameObject> rocksInPlay, RockPhysicsProfile profile)
    {
        var bodies = new List<SimBody>();
        var thrown = new SimBody { s = thrownState, thrown = true, start = thrownState.position };
        thrown.spin = profile.IsLegacy ? thrownState.spin : RockPhysics.CosmeticSpin(thrownState, profile);
        bodies.Add(thrown);
        SimBody hit = null;
        foreach (GameObject go in rocksInPlay)
        {
            if (go == null || !go.activeInHierarchy) continue;
            var b = new SimBody { go = go };
            b.s = RockPhysics.NewRestingRock(profile);
            b.s.position = go.transform.position;
            b.start = b.s.position;
            bodies.Add(b);
            if (go == hitRock) hit = b;
        }

        float speedAfterHit = thrown.s.velocity.magnitude;
        if (hit != null)
        {
            if (ApplyContact(bodies, 0, bodies.IndexOf(hit), thrown.s.position, hit.s.position, profile))
                speedAfterHit = thrown.s.velocity.magnitude;
        }
        Vector2 thrownVelocityAfterHit = thrown.s.velocity;
        Vector2 hitVelocityAfterHit = hit != null ? hit.s.velocity : Vector2.zero;
        foreach (var b in bodies) b.path.Add(b.s.position);

        float maxTime = MAX_SIMULATION_TIME * profile.timeStretch;
        int stepsPerPoint = Mathf.Max(1, Mathf.RoundToInt(STEPS_PER_POINT * profile.timeStretch));
        int hits = 0;
        var before = new Vector2[bodies.Count];

        int step = 0;
        for (float time = 0f; time < maxTime; time += FIXED_DT)
        {
            bool any = false;
            for (int i = 0; i < bodies.Count; i++)
            {
                before[i] = bodies[i].s.position;
                if (!bodies[i].Moving) continue;
                any = true;
                float speedBefore = bodies[i].s.velocity.magnitude;
                RockPhysics.Step(ref bodies[i].s, profile, FIXED_DT);
                bodies[i].spin = NextSpin(bodies[i], profile, speedBefore);
            }
            if (!any) break;
            step++;

            // the earliest contact this step between a moving rock and any other rock (swept, so nothing tunnels)
            if (hits < MAX_HITS)
            {
                int bi = -1, bj = -1; float bf = 2f;
                for (int i = 0; i < bodies.Count; i++)
                {
                    for (int j = i + 1; j < bodies.Count; j++)
                    {
                        if (!bodies[i].Moving && !bodies[j].Moving) continue;
                        // work in rock j's frame: does rock i's relative path come within two radii?
                        Vector2 relBefore = before[i] - before[j];
                        Vector2 relAfter = bodies[i].s.position - bodies[j].s.position;
                        float f;
                        if (!SegmentEntersCircle(relBefore, relAfter, Vector2.zero, rockRadius * 2f, out f) || f >= bf) continue;
                        Vector2 pi = Vector2.Lerp(before[i], bodies[i].s.position, f);
                        Vector2 pj = Vector2.Lerp(before[j], bodies[j].s.position, f);
                        Vector2 n = (pj - pi).normalized;
                        if (Vector2.Dot(bodies[i].s.velocity - bodies[j].s.velocity, n) <= 0f) continue;   // touching but not closing
                        bi = i; bj = j; bf = f;
                    }
                }
                if (bi >= 0)
                {
                    SimBody A = bodies[bi], B = bodies[bj];
                    Vector2 pa = Vector2.Lerp(before[bi], A.s.position, bf);
                    Vector2 pb = Vector2.Lerp(before[bj], B.s.position, bf);
                    if (ApplyContact(bodies, bi, bj, pa, pb, profile))
                    {
                        float rest = (1f - bf) * FIXED_DT;
                        A.s.position = pa + A.s.velocity * rest;
                        B.s.position = pb + B.s.velocity * rest;
                        hits++;
                    }
                }
            }

            if (step % stepsPerPoint == 0)
                foreach (var b in bodies) if (b.Moving || b.path[b.path.Count - 1] != b.s.position) b.path.Add(b.s.position);
        }

        foreach (var b in bodies)
            if (b.path[b.path.Count - 1] != b.s.position) b.path.Add(b.s.position);

        var result = new HitResult
        {
            thrownPath = thrown.path,
            thrownFinal = thrown.s.position,
            hitPath = hit != null ? hit.path : new List<Vector2>(),
            hitFinal = hit != null ? hit.s.position : Vector2.zero,
            others = new List<KeyValuePair<GameObject, Vector2>>(),
            thrownSpeedAfterHit = speedAfterHit,
            thrownVelocityAfterHit = thrownVelocityAfterHit,
            hitVelocityAfterHit = hitVelocityAfterHit
        };
        foreach (var b in bodies)
            if (!b.thrown && b != hit && Vector2.Distance(b.s.position, b.start) > 0.01f)
                result.others.Add(new KeyValuePair<GameObject, Vector2>(b.go, b.s.position));
        return result;
    }

    /// <summary>
    /// Rocks i and j have just touched (at pi, pj). Solve the contact together with every rock touching them (a rock frozen behind the one
    /// that is hit is part of the same problem, as it is for Box2D), and give each rock the velocity and spin that comes out.
    /// Returns false if nothing was closing.
    /// </summary>
    private bool ApplyContact(List<SimBody> bodies, int i, int j, Vector2 pi, Vector2 pj, RockPhysicsProfile profile)
    {
        Vector2 Pos(int k) { return k == i ? pi : (k == j ? pj : bodies[k].s.position); }
        float touch = rockRadius * 2f + RockCollision.TouchMargin;

        var group = new List<int> { i, j };
        for (int g = 0; g < group.Count; g++)
            for (int k = 0; k < bodies.Count; k++)
                if (!group.Contains(k) && (Pos(group[g]) - Pos(k)).magnitude <= touch)
                    group.Add(k);

        int n = group.Count;
        var pos = new Vector2[n]; var vel = new Vector2[n]; var spin = new float[n];
        for (int g = 0; g < n; g++)
        {
            pos[g] = Pos(group[g]);
            vel[g] = bodies[group[g]].s.velocity;
            spin[g] = bodies[group[g]].spin;
        }

        // the two rocks that just met must be closing (their order puts the normal from i to j)
        if (Vector2.Dot(vel[0] - vel[1], (pos[1] - pos[0]).normalized) <= 0f) return false;

        var a = new List<int>(); var b = new List<int>();
        for (int x = 0; x < n; x++)
            for (int y = x + 1; y < n; y++)
                if ((pos[x] - pos[y]).magnitude <= touch) { a.Add(x); b.Add(y); }
        RockCollision.SolveContacts(pos, vel, spin, a.ToArray(), b.ToArray(), RockCollision.BounceThreshold);

        for (int g = 0; g < n; g++)
        {
            SimBody body = bodies[group[g]];
            body.s.velocity = vel[g];
            body.spin = spin[g];
            if (profile.IsLegacy) body.s.spin = spin[g];
            body.s.asleep = false;
            body.s.sleepTimer = 0f;
        }
        return true;
    }

    /// <summary>Spin after a step: the legacy law carries it in the state; the soft-stop law draws it (thrown rock) or fades it with speed (struck rock).</summary>
    private static float NextSpin(SimBody b, RockPhysicsProfile profile, float speedBefore)
    {
        if (profile.IsLegacy) return b.s.spin;
        if (b.s.curlActive) return RockPhysics.CosmeticSpin(b.s, profile);
        return RockPhysics.FadedSpin(b.spin, speedBefore, b.s.velocity.magnitude);
    }

    /// <summary>
    /// Check if trajectory will collide with any rocks (legacy method for compatibility)
    /// </summary>
    public bool CheckCollisions(List<Vector2> trajectory, List<GameObject> rocksInPlay, float rockRadius)
    {
        return lastCollisionInfo.hasCollision;
    }
    
    /// <summary>
    /// Get detailed collision information from the last simulation
    /// </summary>
    public CollisionInfo GetCollisionInfo()
    {
        return lastCollisionInfo;
    }
    
    /// <summary>
    /// Get the speed at each trajectory point for visualization (variable dot sizing)
    /// </summary>
    public List<float> GetTrajectorySpeed()
    {
        return trajectorySpeedList;
    }
    
    /// <summary>
    /// DETERMINISTIC launcher: Calculate exact velocity from pullback distance
    /// NO spring physics - pure linear mapping for 100% predictability
    /// </summary>
    public static Vector2 CalculateInitialVelocityFromPullback(
        Vector2 pullbackPosition, 
        Vector2 launcherPosition,
        float velocityMultiplier = 2.75f,
        float minPullbackDistance = 1.5f,
        float maxPullbackDistance = 5f,
        float minVelocity = 5.0f,
        float maxVelocity = 12.0f)
    {
        // Calculate pullback distance
        Vector2 displacement = launcherPosition - pullbackPosition;
        float pullbackDistance = displacement.magnitude;
        
        // Clamp pullback to allowed range
        pullbackDistance = Mathf.Clamp(pullbackDistance, minPullbackDistance, maxPullbackDistance);
        
        // Map pullback distance to velocity range
        // Option 1: Simple linear multiplier (original behavior)
        float velocity = pullbackDistance * velocityMultiplier;
        
        // Option 2: Remap to specific velocity range (more control)
        // Uncomment this and comment out line above to use range-based mapping
        // float normalizedPullback = (pullbackDistance - minPullbackDistance) / (maxPullbackDistance - minPullbackDistance);
        // float velocity = minVelocity + (normalizedPullback * (maxVelocity - minVelocity));
        
        // Clamp final velocity to range
        velocity = Mathf.Clamp(velocity, minVelocity, maxVelocity);
        
        GameLog.Info($"[Deterministic Launcher] Pullback: {pullbackDistance:F3} (clamped {minPullbackDistance:F2}-{maxPullbackDistance:F2}) → Velocity: {velocity:F2} m/s (range {minVelocity:F2}-{maxVelocity:F2})");
        
        return displacement.normalized * velocity;
    }
    
    /// <summary>
    /// Calculate initial velocity from spring pullback using REAL Unity SpringJoint2D physics
    /// This uses the damped harmonic oscillator equation to compute exact initial velocity
    /// Based on Unity's internal spring force: F = -k*x - c*v (Hooke's law + damping)
    /// </summary>
    public static Vector2 CalculateInitialVelocityFromSpring(
        Vector2 pullbackPosition, 
        Vector2 launcherPosition,
        float springFrequency = 1.5f,
        float springDampingRatio = 0.2f,
        float minPullbackDistance = 0.5f,
        float maxPullbackDistance = 2.75f,
        float minVelocity = 3.0f,
        float maxVelocity = 18.0f)
    {
        // Calculate displacement (how far rock is pulled back from launcher)
        Vector2 displacement = launcherPosition - pullbackPosition;
        float rawSpringDistance = displacement.magnitude;
        
        // CRITICAL FIX: SpringJoint2D gives useless velocity range!
        // Useful pullback is only 0.4 units (Y -26.4 to -26.8)
        // We need to REMAP this narrow range to full game velocity range
        
        // Use the configurable range parameters instead of hardcoded constants
        float MIN_USEFUL_SPRING = minPullbackDistance;
        float MAX_USEFUL_SPRING = maxPullbackDistance;
        float USEFUL_RANGE = MAX_USEFUL_SPRING - MIN_USEFUL_SPRING;
        
        // Clamp to useful range
        float clampedDistance = Mathf.Clamp(rawSpringDistance, MIN_USEFUL_SPRING, MAX_USEFUL_SPRING);
        
        // Remap to full 0-1 power range
        float powerRatio = (clampedDistance - MIN_USEFUL_SPRING) / USEFUL_RANGE;
        
        // Quantize to discrete power levels (200 levels for precision control!)
        const float POWER_LEVELS = 200f;
        powerRatio = Mathf.Round(powerRatio * POWER_LEVELS) / POWER_LEVELS;
        
        // Map back to "virtual" spring distance that gives good velocity range
        // Use the configurable range for calculation
        float springDistance = minVelocity + (powerRatio * (maxVelocity - minVelocity));
        
        GameLog.Info($"[SpringPhysics] Raw: {rawSpringDistance:F4} → Power: {powerRatio:F2} → Remapped: {springDistance:F2}");
        
        // PHYSICS-BASED CALCULATION (replaces simple 5.9x multiplier)
        // Unity's SpringJoint2D uses angular frequency: ? = 2? * frequency
        float angularFrequency = 2f * Mathf.PI * springFrequency;
        
        // Spring constant from frequency: k = m * ?²
        // For rock mass = 19.96 kg, frequency = 1.5 Hz
        float rockMass = 19.96f;
        float springConstant = rockMass * angularFrequency * angularFrequency;
        
        // Damping coefficient: c = 2 * ? * ?(k * m)
        // where ? (zeta) = dampingRatio
        float dampingCoefficient = 2f * springDampingRatio * Mathf.Sqrt(springConstant * rockMass);
        
        // Initial velocity from spring release (damped harmonic oscillator)
        // For underdamped system (? < 1), the rock will oscillate but quickly settle
        // Peak velocity occurs at equilibrium: v_max = ? * A
        // where A = initial displacement
        
        // CRITICAL: This is the ACTUAL physics formula for spring release velocity
        // The rock accelerates from rest at pullback position to max velocity at launcher position
        // Energy equation: (1/2)*k*x² = (1/2)*m*v² (spring potential energy ? kinetic energy)
        // Solving for v: v = x * ?(k/m) = x * ? (for undamped spring)
        
        // For damped spring, reduce velocity by damping factor
        // The damping reduces the energy transfer efficiency
        float dampingFactor = Mathf.Exp(-springDampingRatio * angularFrequency * 0.1f); // 0.1s release time
        
        // Calculate theoretical velocity (from energy conservation)
        float theoreticalVelocity = springDistance * angularFrequency;
        
        // Apply damping reduction and empirical calibration
        // The calibration factor accounts for:
        // - Energy loss during spring compression/release cycle
        // - Rock's finite release time (not instantaneous)
        // - Unity's integration timestep effects
        // 
        // TUNED: 2024 - Calibrated from actual spring testing
        // Test results at 2.0 units pullback:
        //   - Predicted: 9.87 m/s
        //   - Actual: 11.61 m/s
        //   - Error: 15% too low ? increased from 0.63 to 0.72
        
        // OPTION 1: Single calibration factor (use this if accurate across all distances)
        float calibrationFactor = 1f; // TUNED: Increased to match actual velocities (was too low at 0.75-0.77)
        
        // OPTION 2: Distance-dependent calibration (uncomment if needed)
        // Uncomment the block below if linear calibration is accurate at one distance
        // but inaccurate at others (e.g., good at 2.0 units, but off at 1.5 or 3.0)
        /*
        float calibrationFactor;
        if (springDistance < 1.5f)
        {
            calibrationFactor = 0.68f; // Very short shots (higher friction dominance)
        }
        else if (springDistance < 2.0f)
        {
            calibrationFactor = 0.70f; // Short shots
        }
        else if (springDistance < 2.5f)
        {
            calibrationFactor = 0.72f; // Medium shots (baseline)
        }
        else if (springDistance < 3.0f)
        {
            calibrationFactor = 0.74f; // Heavy shots (momentum overcomes friction)
        }
        else
        {
            calibrationFactor = 0.76f; // Very heavy shots
        }
        GameLog.Info($"[Calibration] Distance: {springDistance:F2}, Factor: {calibrationFactor:F2}");
        */
        
        float velocity = theoreticalVelocity * dampingFactor * calibrationFactor;
        
        // VALIDATION: At springDistance = 2.0 units:
        // - angularFrequency = 9.42 rad/s
        // - theoreticalVelocity = 18.85 m/s
        // - dampingFactor ? 0.825
        // - final velocity ? 9.8 m/s (matches observed ~10 m/s in game)
        
        // Enhanced debug logging for tuning
        GameLog.Info($"[SpringPhysics] Dist: {springDistance:F4} | " +
                  $"?: {angularFrequency:F2} | " +
                  $"k: {springConstant:F1} | " +
                  $"v_theory: {theoreticalVelocity:F2} | " +
                  $"v_final: {velocity:F2} | " +
                  $"CalibFactor: {calibrationFactor:F2}");
        
        return displacement.normalized * velocity;
    }
    
    /// <summary>
    /// PHYSICS-BASED: Calculate velocity to reach target using iterative simulation
    /// Uses binary search to find the right velocity that lands at the target
    /// This accounts for damping, friction, and actual physics behavior
    /// </summary>
    public Vector2 CalculateVelocityToTarget(
        Vector2 startPosition, 
        Vector2 targetPosition, 
        bool isInTurn,
        bool isCollisionShot = false)
    {
        Vector2 aimDirection = (targetPosition - startPosition).normalized;
        float targetDistance = Vector2.Distance(startPosition, targetPosition);
        
        // BINARY SEARCH: Find velocity that reaches target
        // Start with reasonable bounds
        float minSpeed = 3.0f;  // Minimum useful speed
        float maxSpeed = 15.0f; // Maximum reasonable speed
        
        // For very short distances, use lighter weight
        if (targetDistance < 10f)
        {
            maxSpeed = 10.0f;
        }
        
        const int MAX_ITERATIONS = 42;
        const float TOLERANCE = 0.6f; // Accept within 60cm of target
        
        float bestSpeed = minSpeed;
        float bestDistanceError = float.MaxValue;
        
        for (int iteration = 0; iteration < MAX_ITERATIONS; iteration++)
        {
            float testSpeed = (minSpeed + maxSpeed) * 0.5f;
            Vector2 testVelocity = aimDirection * testSpeed;
            
            // Simulate this velocity
            List<Vector2> testPath = SimulateTrajectory(
                startPosition,
                testVelocity,
                isInTurn,
                250,
                null, // No obstacles for velocity calculation
                forPlayerPreview: false
            );
            
            if (testPath.Count == 0)
            {
                // Simulation failed - try lower speed
                maxSpeed = testSpeed;
                continue;
            }
            
            Vector2 finalPos = testPath[testPath.Count - 1];
            float distanceError = Vector2.Distance(finalPos, targetPosition);
            
            // Track best attempt
            if (distanceError < bestDistanceError)
            {
                bestDistanceError = distanceError;
                bestSpeed = testSpeed;
            }
            
            // Check if we're close enough
            if (distanceError < TOLERANCE)
            {
                GameLog.Info($"[VelocityCalc] ✓ Converged! Speed: {testSpeed:F2}, Error: {distanceError:F3}, Iterations: {iteration + 1}");
                return aimDirection * testSpeed;
            }
            
            // Adjust search range based on overshoot/undershoot
            if (finalPos.y < targetPosition.y)
            {
                // Undershot - need more speed
                minSpeed = testSpeed;
            }
            else
            {
                // Overshot - need less speed
                maxSpeed = testSpeed;
            }
        }
        
        // Didn't converge, use best attempt
        GameLog.Info($"[VelocityCalc] ⚠️ Max iterations reached. Best speed: {bestSpeed:F2}, Error: {bestDistanceError:F3}");
        return aimDirection * bestSpeed;
    }
}
