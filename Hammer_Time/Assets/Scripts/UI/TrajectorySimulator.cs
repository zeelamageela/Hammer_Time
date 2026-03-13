using System.Collections.Generic;
using UnityEngine;

public class TrajectorySimulator
{
    // Physics constants (you'll need to tune these to match your actual rock physics)
    private const float TIME_STEP = 0.05f; // Larger timestep for better performance (was 0.02f)
    private const float MAX_SIMULATION_TIME = 10f; // Maximum time to simulate (reduced from 15f)
    private const int TRAJECTORY_SAMPLE_RATE = 3; // Only add every Nth point to trajectory for performance
    
    // Ice friction parameters - TUNED TO MATCH ACTUAL ROCK BEHAVIOR
    // Rock has linearDamping = 0.38 in Rigidbody2D BUT effective damping is HIGHER due to:
    //   - Angular damping (0.32) creating additional drag
    //   - Other physics interactions (collider drag, etc.)
    // Empirical data: Rock travels ~60% of distance that 0.38 damping would predict
    // Solution: Use HIGHER damping value in simulation to match reality
    // Unity applies: velocity *= (1 - linearDamping * Time.fixedDeltaTime)
    private float baseDamping = 0.62f; // Base damping (before global speed scaling)
    private float linearDamping = 0.62f; // TUNED: Increased from 0.38 to match ACTUAL behavior (rock stops sooner than 0.38 predicts)
    private float lateralFriction = 0.002f;  // Reduced accordingly
    
    // Curl parameters - SIMPLIFIED (removed late breaking)
    private float curlAmount = 0.3f; // Legacy - now using angularVelocityCurl instead
    
    // Angular velocity curl model (matches Rock_Force.cs EXACTLY - READ FROM ROCK GAMEOBJECT!)
    public Vector2 curlVector = new Vector2(-0.6f, 0f); // FIXED: Was +0.323, rock uses -0.323!
    public float scaleFactor = 0.1f;
    public float initialAngularVelocity = 60f; // This is the REAL value that works!
    public float angularDamping = 0.32f; // FIXED: Was 0.05, Rigidbody2D uses 0.32!
    public float curlForceScale = 1.0f; // TUNED: MUST MATCH Rock_Force.curlForceMultiplier (1.0)!
    
    // Rock properties
    private float rockMass = 145f; // FIXED: Matches actual rock Rigidbody2D mass (was 19.96 - wrong!)
    private float rockRadius = 0.14f; // Rock radius in your world units
    
    // CRITICAL: Curl force compensation for mass
    // Unity's AddForce(ForceMode2D.Force) divides by mass internally
    // We already divide by rockMass explicitly, so we need to compensate
    // Use the "effective mass" for curl calculations to match the visual curl amount
    private float curlEffectiveMass = 19.96f; // The original mass that gave correct curl behavior
    
    // EMPIRICAL CALIBRATION: Trajectory consistently overshoots by ~2.9 units
    // This calibration factor scales the trajectory length to match actual rock behavior
    // Theory: Simulation uses TIME_STEP=0.05s (2.5x FixedUpdate), may accumulate position errors
    //private const float TRAJECTORY_LENGTH_CALIBRATION = 0.85f; // Reduce by 15% to match reality
    
    // Collision parameters
    private const float RESTITUTION = 0.85f; // Bounciness (0-1, curling rocks are fairly elastic)
    private const float COLLISION_DAMPING = 0.7f; // Energy loss on collision
    
    // Result data
    public struct CollisionInfo
    {
        public bool hasCollision;
        public int collisionIndex; // Index in trajectory where collision occurs
        public GameObject hitRock;
        public Vector2 collisionPoint; // SURFACE contact point (for visualization)
        public Vector2 shooterCenterAtCollision; // NEW: Shooter's center position at moment of collision (for scoring)
        public Vector2 finalPosition; // Where the thrown rock ends up
        public Vector2 hitRockFinalPosition; // Where the hit rock ends up
        public List<Vector2> thrownRockPostCollisionPath; // Full path of thrown rock after collision
        public List<Vector2> hitRockPostCollisionPath; // Full path of hit rock after collision
    }
    
    public CollisionInfo lastCollisionInfo { get; private set; }
    
    // Track speed at each trajectory point for visualization
    private List<float> trajectorySpeedList = new List<float>();
    
    public TrajectorySimulator(float friction, float curl)
    {
        baseDamping = friction;
        linearDamping = friction;
        curlAmount = curl;
        
        // NO SCALING! The tuned ratio (0.62 / 0.38 = 1.63x) already accounts for everything
        // This ratio was carefully calibrated and should NOT be changed
        Debug.Log($"[TrajectorySimulator] Initialized with friction: {friction:F3} (Rock's actual linearDamping=0.38, but effective damping is higher)");
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
        bool forPlayerPreview = false)  // NEW: Differentiate AI calc from player preview
    {
        List<Vector2> trajectoryPoints = new List<Vector2>();
        trajectorySpeedList = new List<float>(); // Reset speed tracking
        
        Vector2 position = startPosition;
        Vector2 velocity = initialVelocity;
        float currentTime = 0f;
        
        // === VELOCITY-BASED CURL SCALING ===
        // In real curling: FAST rocks curl LESS, SLOW rocks curl MORE
        // Scale curlVector based on initial velocity (matching Rock_Force.cs)
        float currentVelocity = initialVelocity.magnitude;
        
        // Get min/max velocities from TrajectoryLine (if available)
        TrajectoryLine trajLine = GameObject.FindFirstObjectByType<TrajectoryLine>();
        float minVelocity = (trajLine != null) ? trajLine.minVelocity : 5f;   // Default: 5 m/s
        float maxVelocity = (trajLine != null) ? trajLine.maxVelocity : 11f;  // Default: 11 m/s
        
        // Calculate velocity ratio (0 = slowest, 1 = fastest)
        float velocityRatio = Mathf.Clamp01((currentVelocity - minVelocity) / (maxVelocity - minVelocity));

        // Scale curl: 0.6 (slow shots) to 0.2 (fast shots)
        // At 8 m/s (mid-speed): curl ≈ 0.45 (more realistic for draw shots)
        float curlMagnitude;

        if (velocityRatio < 0.4f)
        {
            curlMagnitude = Mathf.Lerp(0.6f, 0.48f, velocityRatio / 0.33f); // 0 to 0.33 → 0.6 to 0.48
        }
        else if (velocityRatio < 0.66f)
        {
            curlMagnitude = Mathf.Lerp(0.48f, 0.15f, (velocityRatio - 0.33f) / 0.33f); // 0.33 to 0.66 → 0.48 to 0.15
        }
        else
        {
            curlMagnitude = Mathf.Lerp(0.15f, 0.05f, (velocityRatio - 0.66f) / 0.34f); // 0.66 to 1.0 → 0.2 to 0.05
        }

        // Apply to curlVector (will be used throughout simulation)
        // MUST MATCH Rock_Force.cs convention: curlMagnitude * -dirMult
        // Negative of dirMult because torque is inverted but curl isn't
        Vector2 scaledCurlVector = new Vector2(curlMagnitude, 0f);
        
        //Debug.Log($"[TrajectorySimulator Curl Scaling] Velocity: {currentVelocity:F2} m/s, Ratio: {velocityRatio:F2}, Curl: {curlMagnitude:F3} (slow=0.6, fast=0.2)");
        
        // REAL CURLING PHYSICS: NO spin before hog line!
        // Hog line trigger is at Y = -16.15 (BoxCollider2D on "Hog_Line" GameObject)
        const float HOG_LINE_Y = -16.15f;
        bool pastHogLine = false;
        float currentDamping = 0f; // Start with ZERO damping
        
        // REAL CURLING: Spin applied AT hog line, not at launch!
        // Angular velocity starts at ZERO
        float angularVelocity = 0f;
        
        // Reset collision info
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
            hitRockPostCollisionPath = new List<Vector2>()
        };
        
        // Add starting point
        trajectoryPoints.Add(position);
        trajectorySpeedList.Add(velocity.magnitude);
        
        // REMOVED: Debug.Log for performance - only log on errors
        // Debug.Log($"[Simulator] Starting trajectory - initialVel: {initialVelocity} (mag: {initialVelocity.magnitude}), velocity: {velocity}, startPos: {startPosition}, position: {position}, isInTurn: {isInTurn}, rocks: {rocksInPlay?.Count ?? 0}");
        
        int iterationCount = 0; // Track actual loop iterations
        float totalSimDistance = 0f;
        Vector2 lastSimPosition = position;
        
        //Debug.Log($"[Trajectory SIM START] InitVel: {initialVelocity.magnitude:F3} m/s | StartPos: {startPosition}");
        
        while (currentTime < MAX_SIMULATION_TIME && trajectoryPoints.Count < maxPoints)
        {
            // Apply friction (deceleration)
            float speed = velocity.magnitude;
            
            // DIAGNOSTIC: Log every 10 iterations (~0.5 seconds real time)
            if (iterationCount > 0 && iterationCount % 10 == 0)
            {
                float simFrameDist = Vector2.Distance(position, lastSimPosition);
                totalSimDistance += simFrameDist;
                
                //Debug.Log($"[Trajectory Iter {iterationCount}] Pos: ({position.x:F3}, {position.y:F3}) | " +
                //         $"Vel: {speed:F3} m/s | " +
                //         $"AngVel: {angularVelocity:F2} | " +
                //         $"Damping: {currentDamping:F3} | " +
                //         $"IterDist: {simFrameDist:F4} | TotalDist: {totalSimDistance:F3}");
                
                lastSimPosition = position;
            }
            
            if (speed < 0.01f)
            {
                //Debug.Log($"[Trajectory SIM STOPPED] Final Position: ({position.x:F3}, {position.y:F3}) | Total Distance: {totalSimDistance:F3} | Iterations: {iterationCount}");
                // Rock has essentially stopped
                break;
            }
            
            // Check for collisions with other rocks
            if (rocksInPlay != null && !lastCollisionInfo.hasCollision)
            {
                foreach (GameObject rock in rocksInPlay)
                {
                    if (rock == null || !rock.activeInHierarchy)
                        continue;
                    
                    Vector2 rockPos = rock.transform.position;
                    float distance = Vector2.Distance(position, rockPos);
                    
                    // IMPROVED: Detect potential collision earlier and increase resolution
                    bool nearCollision = distance < rockRadius * 4f; // Within 4x rock radius
                    
                    if (nearCollision && iterationCount > 0)
                    {
                        // HIGHER RESOLUTION: Use smaller time steps near collision
                        float highResTimeStep = TIME_STEP * 0.2f; // 5x more detailed
                        Vector2 tempPos = position - velocity * TIME_STEP; // Go back one step
                        Vector2 tempVel = velocity;
                        
                        // Re-simulate this section with high resolution
                        for (int substep = 0; substep < 5; substep++)
                        {
                            tempPos += tempVel * highResTimeStep;
                            
                            // Apply physics at high resolution (match FixedUpdate rate)
                            // CRITICAL: Apply damping at Unity's FixedUpdate rate (0.02s)
                            const float unityFixedDeltaTime = 0.02f;
                            int highResDampingSteps = Mathf.RoundToInt(highResTimeStep / unityFixedDeltaTime);
                            float highResDampingFactor = 1.0f;
                            for (int d = 0; d < highResDampingSteps; d++)
                            {
                                highResDampingFactor *= (1.0f - linearDamping * unityFixedDeltaTime);
                            }
                            
                            if (tempVel.magnitude > 0.1f)
                            {
                                // Use REAL angular velocity model (same as main loop)
                                float tempVelX = angularVelocity * scaleFactor;
                                int dirMult = isInTurn ? -1 : 1;  // MUST MATCH main loop
                                // MUST USE -dirMult formula (matches main loop and Rock_Force.cs)
                                Vector2 curlForce = new Vector2(-dirMult * scaledCurlVector.x * tempVelX, 0f);
                                // CRITICAL: Use curlEffectiveMass for curl calculations
                                Vector2 velocityChange = curlForce * highResTimeStep / curlEffectiveMass;
                                tempVel += velocityChange * curlForceScale;
                            }
                            
                            tempVel *= highResDampingFactor; // Apply Unity-style damping
                            
                            // Check collision at high resolution
                            float distCheck = Vector2.Distance(tempPos, rockPos);
                            if (distCheck < rockRadius * 2f)
                            {
                                // COLLISION DETECTED at high resolution!
                                position = tempPos;
                                velocity = tempVel;
                                distance = distCheck;
                                break;
                            }
                        }
                    }
                    
                    // Collision detected
                    if (distance < rockRadius * 2f)
                    {
                        // Calculate collision normal (direction FROM thrown rock TO hit rock)
                        Vector2 collisionNormal = (rockPos - position).normalized;
                        
                        // CRITICAL FIX: The collision POINT is where the rocks' SURFACES touch,
                        // not where the shooter's center is!
                        // The shooter's center is at 'position', target center is at 'rockPos'
                        // The actual contact point is rockRadius distance from target center, toward shooter
                        Vector2 actualContactPoint = rockPos - collisionNormal * rockRadius;
                        
                        // Calculate collision response
                        Vector2 hitRockVelocity = CalculateCollisionResponse(
                            position, 
                            velocity, 
                            rockPos, 
                            Vector2.zero, // Stationary rock
                            out Vector2 newVelocity
                        );
                        
                        // Debug collision angles
                        float incomingAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                        float exitAngle = Mathf.Atan2(newVelocity.y, newVelocity.x) * Mathf.Rad2Deg;
                        float hitRockAngle = Mathf.Atan2(hitRockVelocity.y, hitRockVelocity.x) * Mathf.Rad2Deg;
                        float normalAngle = Mathf.Atan2(collisionNormal.y, collisionNormal.x) * Mathf.Rad2Deg;
                        
                        Debug.Log($"[Collision] Incoming: {incomingAngle:F1}°, Exit: {exitAngle:F1}°, HitRock: {hitRockAngle:F1}°, Normal: {normalAngle:F1}° | " +
                                  $"InVel: {velocity.magnitude:F2}, OutVel: {newVelocity.magnitude:F2}, HitVel: {hitRockVelocity.magnitude:F2}");
                        
                        // FIXED: Show ACTUAL post-collision paths, not deflection indicators
                        // Orange line: Where the thrown rock goes after hitting
                        // Yellow line: Where the hit rock goes after being hit
                        List<Vector2> thrownRockPath = SimulatePostCollisionPath(position, newVelocity, false);
                        
                        // Simulate hit rock's path (moves along collision normal)
                        List<Vector2> hitRockPath = SimulatePostCollisionPath(rockPos, hitRockVelocity, false);
                        
                        Vector2 thrownRockFinal = thrownRockPath.Count > 0 ? thrownRockPath[thrownRockPath.Count - 1] : position;
                        Vector2 hitRockFinal = hitRockPath.Count > 0 ? hitRockPath[hitRockPath.Count - 1] : rockPos;
                        
                        lastCollisionInfo = new CollisionInfo
                        {
                            hasCollision = true,
                            collisionIndex = trajectoryPoints.Count,
                            hitRock = rock,
                            collisionPoint = actualContactPoint, // Surface contact point (for visualization)
                            shooterCenterAtCollision = position, // Shooter's center at collision (for scoring!)
                            finalPosition = thrownRockFinal,
                            hitRockFinalPosition = hitRockFinal,
                            thrownRockPostCollisionPath = thrownRockPath,
                            hitRockPostCollisionPath = hitRockPath
                        };
                        
                        // Continue simulation with new velocity for a few more points
                        velocity = newVelocity;
                        
                        // Add collision point
                        trajectoryPoints.Add(position);
                        trajectorySpeedList.Add(velocity.magnitude);
                        
                        // Stop main trajectory here - post-collision paths are separate
                        break;
                    }
                }
                
                // If collision detected, stop main simulation
                if (lastCollisionInfo.hasCollision)
                {
                    break;
                }
            }
            
            // Check if we've passed the hog line
            if (!pastHogLine && position.y > HOG_LINE_Y)
            {
                pastHogLine = true;
                currentDamping = linearDamping; // NOW apply damping!
                
                // REAL CURLING: Apply spin NOW (at hog line, matching Rock_Force.Release())
                // CRITICAL FIX: Angular velocity is ALWAYS POSITIVE - dirMult handles the direction!
                // Don't negate based on isInTurn here, or it will double-negate with dirMult
                angularVelocity = initialAngularVelocity;  // Always positive!
                
                //Debug.Log($"[Trajectory] Passed hog line at Y={position.y:F2}, enabling damping: {currentDamping}, spin: {angularVelocity} (dirMult will apply turn direction)");
            }
            
            // CRITICAL: Match Rock_Force behavior - increase damping when rock is nearly stopped!
            // Rock_Force.cs increases linearDamping to 0.75 when speed < 0.01
            // This makes the rock "stick" to the ice at the end for a more realistic stop
            if (speed < 0.01f && currentDamping < 0.90f)
            {
                currentDamping = 0.90f; // Match Rock_Force's stopping damping
            }
            
            // Calculate damping (Unity's multiplicative model)
            // Unity: velocity *= (1 - linearDamping * Time.fixedDeltaTime)
            // CRITICAL FIX: Unity applies damping at FixedUpdate interval (0.02s)
            // Our TIME_STEP is 0.05s (2.5x longer), so we need to apply damping 2.5x
            // Solution: Calculate damping as if applied multiple times at FixedUpdate rate
            float fixedDeltaTime = 0.02f; // Unity's FixedUpdate interval
            int dampingSteps = Mathf.RoundToInt(TIME_STEP / fixedDeltaTime); // How many FixedUpdates in our timestep
            float dampingFactor = 1.0f;
            for (int i = 0; i < dampingSteps; i++)
            {
                dampingFactor *= (1.0f - currentDamping * fixedDeltaTime); // Zero before hog line, linearDamping after
            }
            
            // Apply curl (USING REAL ANGULAR VELOCITY MODEL FROM ROCK_FORCE.CS!)
            // Rock_Force.cs: velX = angularVelocity; vel = Vector2(velX * scaleFactor, velY); AddForce(curl * vel)
            if (speed > 0.1f && Mathf.Abs(angularVelocity) > 0.1f)
            {
                // EXACT MATCH TO Rock_Force.cs physics!
                // vel.x = angularVelocity * scaleFactor (used for curl force calculation)
                float velX = angularVelocity * scaleFactor;
                float velY = 0f; // Not used in curl calculation in Rock_Force
                
                // Curl direction logic - MATCHES Rock_Force.cs
                // flipAxis=true (in-turn) → dirMult=-1 → curl = -(-1) * mag = +mag (RIGHT)
                // flipAxis=false (out-turn) → dirMult=+1 → curl = -(+1) * mag = -mag (LEFT)
                int dirMult = isInTurn ? -1 : 1;
                
                // Apply curl force exactly as Rock_Force.cs does
                // Formula: -dirMult * curlMagnitude
                Vector2 curlForce = new Vector2(-dirMult * scaledCurlVector.x * velX, 0f);
                
                // Rock_Force applies this force every FixedUpdate (0.02s)
                // We apply it every TIME_STEP (0.05s), so scale by time ratio
                float timeScaleCorrection = TIME_STEP / 0.02f;  // Usually ~2.5x
                
                // AddForce in ForceMode2D.Force divides by mass and multiplies by deltaTime
                // force_impulse = force * deltaTime / mass
                // CRITICAL: Use curlEffectiveMass (19.96) instead of rockMass (145) to match visual curl
                // The real mass is used for collisions, but curl force needs the "effective" mass
                Vector2 velocityChange = curlForce * TIME_STEP / curlEffectiveMass;
                velocity += velocityChange * curlForceScale; // Calibration factor
                
                // Apply angular damping (spin decays over time)
                // This is KEY to parabolic curl! Angular velocity decreases SLOWER than linear velocity
                angularVelocity *= (1.0f - angularDamping * TIME_STEP);
                
                // Debug curl at key velocity points
                if (speed < 1.0f && iterationCount % 10 == 0)
                {
                    //Debug.Log($"[Curl @ slow] speed={speed:F2}, angVel={angularVelocity:F2}, velX={velX:F3}, curlForce={curlForce}, velChange={velocityChange}");
                }
            }
            
            // Apply damping (Unity's multiplicative model)
            velocity *= dampingFactor;
            
            // Update position
            position += velocity * TIME_STEP;
            
            // PERFORMANCE: Sample every Nth iteration to reduce trajectory point count
            // Only add points when needed for visualization
            if (iterationCount % TRAJECTORY_SAMPLE_RATE == 0 || speed < 0.5f || iterationCount < 5)
            {
                trajectoryPoints.Add(position);
                trajectorySpeedList.Add(speed); // Track speed at this point
            }
            
            currentTime += TIME_STEP;
            iterationCount++;
        }
        
        // Debug.Log($"[Simulator] Trajectory complete - {trajectoryPoints.Count} points, final pos: {position}, collision: {lastCollisionInfo.hasCollision}");
        
        // Record final position if no collision
        if (!lastCollisionInfo.hasCollision)
        {
            // CALIBRATION: Scale trajectory length to match actual rock behavior
            // The simulation consistently overshoots by ~15%, likely due to TIME_STEP accumulation
            Vector2 startPos = trajectoryPoints[0];
            Vector2 travelVector = position - startPos;
            //Vector2 calibratedPosition = startPos + (travelVector * TRAJECTORY_LENGTH_CALIBRATION);
            
            lastCollisionInfo = new CollisionInfo
            {
                hasCollision = false,
                collisionIndex = -1,
                hitRock = null,
                collisionPoint = Vector2.zero,
                shooterCenterAtCollision = Vector2.zero,
                //finalPosition = calibratedPosition,  // Use calibrated position!
                hitRockFinalPosition = Vector2.zero,
                thrownRockPostCollisionPath = new List<Vector2>(),
                hitRockPostCollisionPath = new List<Vector2>()
            };
        }
        
        return trajectoryPoints;
    }
    
    /// <summary>
    /// Calculate velocity changes for both rocks after collision using realistic elastic collision physics
    /// This properly handles both normal (direct) and tangential (glancing) components
    /// </summary>
    private Vector2 CalculateCollisionResponse(
        Vector2 pos1, Vector2 vel1,
        Vector2 pos2, Vector2 vel2,
        out Vector2 newVel1)
    {
        // Direction from rock1 to rock2 (collision normal)
        Vector2 collisionNormal = (pos2 - pos1).normalized;
        
        // Tangent perpendicular to collision normal
        Vector2 collisionTangent = new Vector2(-collisionNormal.y, collisionNormal.x);
        
        // Decompose velocities into normal and tangential components
        // Rock 1 (thrown rock)
        float vel1Normal = Vector2.Dot(vel1, collisionNormal);
        float vel1Tangent = Vector2.Dot(vel1, collisionTangent);
        
        // Rock 2 (stationary rock)
        float vel2Normal = Vector2.Dot(vel2, collisionNormal);
        float vel2Tangent = Vector2.Dot(vel2, collisionTangent);
        
        // FIXED: Only resolve if velocities are approaching (closing in on each other)
        // vel1Normal should be positive (moving toward rock2)
        // If already separating (vel1Normal <= 0), don't resolve
        if (vel1Normal <= 0)
        {
            newVel1 = vel1;
            return vel2;
        }
        
        // Apply 1D elastic collision equation to normal components only
        // For equal mass objects: velocities are exchanged along normal
        // v1' = ((m1 - m2) * v1 + 2 * m2 * v2) / (m1 + m2)
        // With m1 = m2, this simplifies to: v1' = v2, v2' = v1
        float newVel1Normal = ((rockMass - rockMass) * vel1Normal + 2 * rockMass * vel2Normal) / (2 * rockMass);
        float newVel2Normal = ((rockMass - rockMass) * vel2Normal + 2 * rockMass * vel1Normal) / (2 * rockMass);
        
        // Tangential components remain unchanged (no friction in tangent direction for curling rocks)
        // In real curling, rocks can glance off each other
        float newVel1Tangent = vel1Tangent;
        float newVel2Tangent = vel2Tangent;
        
        // Reconstruct velocity vectors from components
        Vector2 vel1NormalVec = newVel1Normal * collisionNormal;
        Vector2 vel1TangentVec = newVel1Tangent * collisionTangent;
        Vector2 vel2NormalVec = newVel2Normal * collisionNormal;
        Vector2 vel2TangentVec = newVel2Tangent * collisionTangent;
        
        // Combine normal and tangential components
        newVel1 = (vel1NormalVec + vel1TangentVec) * RESTITUTION * COLLISION_DAMPING;
        Vector2 newVel2 = (vel2NormalVec + vel2TangentVec) * RESTITUTION * COLLISION_DAMPING;
        
        return newVel2;
    }
    
    /// <summary>
    /// Simulate a rock's path after collision until it stops
    /// </summary>
    private Vector2 SimulatePostCollision(Vector2 startPos, Vector2 startVel, bool hasRotation)
    {
        Vector2 position = startPos;
        Vector2 velocity = startVel;
        float time = 0f;
        
        while (time < MAX_SIMULATION_TIME)
        {
            float speed = velocity.magnitude;
            
            if (speed < 0.01f)
                break;
            
            // Apply friction (Unity's multiplicative damping)
            float dampingFactor = 1.0f - (linearDamping * TIME_STEP);
            velocity *= dampingFactor;
            
            // Minimal curl effect post-collision (rock is tumbling)
            if (hasRotation && speed > 0.1f)
            {
                Vector2 curlDirection = new Vector2(-velocity.y, velocity.x).normalized;
                float curlStrength = curlAmount * 0.3f * speed / startVel.magnitude;
                velocity += curlDirection * curlStrength * TIME_STEP;
            }
            
            position += velocity * TIME_STEP;
            time += TIME_STEP;
        }
        
        return position;
    }
    
    /// <summary>
    /// Simulate a rock's FULL PATH after collision until it stops, returning all points
    /// </summary>
    private List<Vector2> SimulatePostCollisionPath(Vector2 startPos, Vector2 startVel, bool hasRotation)
    {
        List<Vector2> path = new List<Vector2>();
        Vector2 position = startPos;
        Vector2 velocity = startVel;
        float time = 0f;
        int iterations = 0;
        
        path.Add(position); // Start at collision point
        
        while (time < MAX_SIMULATION_TIME && iterations < 200)
        {
            float speed = velocity.magnitude;
            
            if (speed < 0.01f)
                break;
            
            // FIXED: Only apply friction after collision - NO CURL
            // Rock tumbles after collision and loses controlled rotation
            // Apply damping at Unity's FixedUpdate rate (0.02s)
            float fixedDeltaTime = 0.02f;
            int dampingSteps = Mathf.RoundToInt(TIME_STEP / fixedDeltaTime);
            float dampingFactor = 1.0f;
            for (int i = 0; i < dampingSteps; i++)
            {
                dampingFactor *= (1.0f - linearDamping * fixedDeltaTime);
            }
            velocity *= dampingFactor;
            
            position += velocity * TIME_STEP;
            
            // Sample points for visualization (every few iterations)
            if (iterations % TRAJECTORY_SAMPLE_RATE == 0)
            {
                path.Add(position);
            }
            
            time += TIME_STEP;
            iterations++;
        }
        
        // Always add final position
        if (path.Count == 0 || path[path.Count - 1] != position)
        {
            path.Add(position);
        }
        
        return path;
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
        
        Debug.Log($"[Deterministic Launcher] Pullback: {pullbackDistance:F3} (clamped {minPullbackDistance:F2}-{maxPullbackDistance:F2}) → Velocity: {velocity:F2} m/s (range {minVelocity:F2}-{maxVelocity:F2})");
        
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
        
        Debug.Log($"[SpringPhysics] Raw: {rawSpringDistance:F4} → Power: {powerRatio:F2} → Remapped: {springDistance:F2}");
        
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
        Debug.Log($"[Calibration] Distance: {springDistance:F2}, Factor: {calibrationFactor:F2}");
        */
        
        float velocity = theoreticalVelocity * dampingFactor * calibrationFactor;
        
        // VALIDATION: At springDistance = 2.0 units:
        // - angularFrequency = 9.42 rad/s
        // - theoreticalVelocity = 18.85 m/s
        // - dampingFactor ? 0.825
        // - final velocity ? 9.8 m/s (matches observed ~10 m/s in game)
        
        // Enhanced debug logging for tuning
        Debug.Log($"[SpringPhysics] Dist: {springDistance:F4} | " +
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
                Debug.Log($"[VelocityCalc] ✓ Converged! Speed: {testSpeed:F2}, Error: {distanceError:F3}, Iterations: {iteration + 1}");
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
        Debug.Log($"[VelocityCalc] ⚠️ Max iterations reached. Best speed: {bestSpeed:F2}, Error: {bestDistanceError:F3}");
        return aimDirection * bestSpeed;
    }
}
