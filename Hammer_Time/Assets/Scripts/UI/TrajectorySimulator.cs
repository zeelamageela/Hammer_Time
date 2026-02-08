using System.Collections.Generic;
using UnityEngine;

public class TrajectorySimulator
{
    // Physics constants (you'll need to tune these to match your actual rock physics)
    private const float TIME_STEP = 0.05f; // Larger timestep for better performance (was 0.02f)
    private const float MAX_SIMULATION_TIME = 10f; // Maximum time to simulate (reduced from 15f)
    private const int TRAJECTORY_SAMPLE_RATE = 3; // Only add every Nth point to trajectory for performance
    
    // Ice friction parameters - MATCHED TO REAL GAME PHYSICS
    // Rock has linearDamping = 0.38 in Rigidbody2D (ramps to 0.55 when stopping)
    // Unity applies: velocity *= (1 - linearDamping * Time.fixedDeltaTime)
    private float linearDamping = 0.38f; // CORRECTED: Matches actual rock physics
    private float lateralFriction = 0.002f;  // Reduced accordingly
    
    // Curl parameters - NOW USING ACTUAL ROCK_FORCE MODEL
    // Rock_Force.cs uses: force = curl * (angularVelocity * scaleFactor)
    // With curl.x = -0.323, scaleFactor = 0.1, angularVelocity ? 60 rad/s
    private float curlAmount = 0.3f; // Legacy - now using angularVelocityCurl instead
    private float lateBreakingIntensity = 2.0f; // How much curl increases at end (0.0 = no late breaking, 3.0 = dramatic)
    private float lateBreakingCurve = 0.8f; // Shape of curl curve (0.5 = very subtle, 1.0 = linear, 2.0 = exponential)
    
    // NEW: Angular velocity curl model (matches Rock_Force.cs exactly)
    // CRITICAL: curl.x sign determines curl direction!
    // If actual rocks curl opposite to simulation, flip this sign
    private Vector2 curlVector = new Vector2(0.323f, 0f); // FLIPPED: Was -0.323, now +0.323
    private float scaleFactor = 0.1f; // Matches Rock_Force.cs scaleFactor
    private float initialAngularVelocity = 60f; // Matches Rock_Force.cs turnValue (in rad/s)
    private float angularDamping = 0.05f; // Angular velocity decay rate
    
    // Rock properties
    private float rockMass = 19.96f; // kg (actual curling rock mass)
    private float rockRadius = 0.14f; // Rock radius in your world units
    
    // CALIBRATION: Force-to-velocity conversion factor
    // Unity's AddForce divides by mass, but our units don't match real physics 1:1
    // This factor bridges the gap between simulation and actual rock behavior
    // TUNING: Start very low and increase if curl is too weak
    private float curlForceScale = 0.5f; // Start with minimal curl - tune upward if needed
    
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
    
    public TrajectorySimulator(float friction, float curl, float lateBreakingIntensity = 2.0f, float lateBreakingCurve = 0.8f)
    {
        linearDamping = friction; // Now correctly named!
        curlAmount = curl;
        this.lateBreakingIntensity = lateBreakingIntensity;
        this.lateBreakingCurve = lateBreakingCurve;
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
        
        // NEW: Initialize angular velocity (matches Rock_Force.cs AddTorque)
        // Rock_Force applies: dirMult * turnValue * Mathf.Deg2Rad as impulse torque
        // This creates initial angular velocity
        float angularVelocity = isInTurn ? initialAngularVelocity : -initialAngularVelocity;
        
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
        
        while (currentTime < MAX_SIMULATION_TIME && trajectoryPoints.Count < maxPoints)
        {
            // Apply friction (deceleration)
            float speed = velocity.magnitude;
            
            // REMOVED DEBUG LOGS FOR PERFORMANCE
            
            if (speed < 0.01f)
            {
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
                            
                            // Apply physics at high resolution
                            float highResDampingFactor = 1.0f - (linearDamping * highResTimeStep);
                            
                            if (tempVel.magnitude > 0.1f)
                            {
                                Vector2 curlDirection = isInTurn 
                                    ? new Vector2(tempVel.y, -tempVel.x).normalized 
                                    : new Vector2(-tempVel.y, tempVel.x).normalized;
                                
                                float velocityRatio = tempVel.magnitude / initialVelocity.magnitude;
                                float slowdownFactor = 1.0f - velocityRatio;
                                float curveFactor = Mathf.Pow(slowdownFactor, lateBreakingCurve);
                                float normalizedMultiplier = curveFactor * (1.0f + lateBreakingIntensity);
                                float baseCurlStrength = curlAmount * 0.003f;
                                float curlStrength = baseCurlStrength * normalizedMultiplier;
                                Vector2 curlForce = curlDirection * curlStrength;
                                
                                tempVel += curlForce * highResTimeStep;
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
            
            // Calculate damping (Unity's multiplicative model)
            // Unity: velocity *= (1 - linearDamping * Time.fixedDeltaTime)
            // We need to match this exactly
            float dampingFactor = 1.0f - (linearDamping * TIME_STEP);
            
            // Apply curl (CALIBRATED TO MATCH ROCK_FORCE.CS BEHAVIOR)
            // Rock_Force.cs: Vector2 vel = new Vector2(angularVelocity * scaleFactor, 0);
            //                body.AddForce(curl * vel, ForceMode2D.Force);
            // 
            // Unity's AddForce: acceleration = force / mass, then velocity += acceleration * deltaTime
            // For a 19.96kg rock: velocity += (force / 19.96) * deltaTime
            if (speed > 0.1f && Mathf.Abs(angularVelocity) > 0.1f) // Only curl when moving and spinning
            {
                // Calculate the force vector (matching Rock_Force.cs exactly)
                Vector2 angularVel = new Vector2(angularVelocity * scaleFactor, 0f);
                Vector2 curlForce = new Vector2(curlVector.x * angularVel.x, curlVector.y * angularVel.y);
                
                // Convert force to velocity change (Unity physics: a = F/m, v += a*dt)
                // Scale by curlForceScale to match the actual game's curl behavior
                float acceleration = (curlForce.x / rockMass) * curlForceScale;
                velocity.x += acceleration * TIME_STEP;
                
                // Apply angular damping (spin decays over time)
                angularVelocity *= (1.0f - angularDamping * TIME_STEP);
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
            lastCollisionInfo = new CollisionInfo
            {
                hasCollision = false,
                collisionIndex = -1,
                hitRock = null,
                collisionPoint = Vector2.zero,
                shooterCenterAtCollision = Vector2.zero,
                finalPosition = position,  // Use the actual final position from simulation, not last trajectory point
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
            float dampingFactor = 1.0f - (linearDamping * TIME_STEP);
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
    /// Calculate initial velocity from spring pullback using Unity's spring physics
    /// This matches how the actual rock gets its velocity from the SpringJoint2D
    /// </summary>
    public static Vector2 CalculateInitialVelocityFromSpring(
        Vector2 pullbackPosition, 
        Vector2 launcherPosition,
        float springFrequency = 1.5f,
        float springDampingRatio = 0.2f)
    {
        Vector2 displacement = launcherPosition - pullbackPosition;
        float springDistance = displacement.magnitude;
        
        // The game world is at a smaller scale than real-world physics
        // Based on Rock_Placement.cs: Launcher at y=-25, House center at y=6.5
        // Spring distance ranges from ~1.5 to ~3.25 units
        // Should result in rocks traveling 30-35 units up the sheet
        
        // Simple linear relationship: velocity = distance * multiplier
        // CALIBRATED from actual measurements:
        // SpringDist: 2.07 ? ActualVel: 11.97 (multiplier: 5.78)
        // SpringDist: 1.61 ? ActualVel: 9.72 (multiplier: 6.04)
        // Average multiplier: ~5.9
        float velocityMultiplier = 5.9f;
        float maxVelocity = springDistance * velocityMultiplier;
        
        Debug.Log($"Spring calc: distance={springDistance}, velocity={maxVelocity}, direction={displacement.normalized}");
        
        return displacement.normalized * maxVelocity;
    }
    
    /// <summary>
    /// SIMPLIFIED: Calculate velocity to reach target using basic geometry
    /// Just aim straight at Y=10 (or target position) and return the velocity
    /// The simulation will handle curl - we don't try to pre-compensate!
    /// </summary>
    public Vector2 CalculateVelocityToTarget(
        Vector2 startPosition, 
        Vector2 targetPosition, 
        bool isInTurn,
        bool isCollisionShot = false)
    {
        // SIMPLE: Aim straight at the target
        Vector2 displacement = targetPosition - startPosition;
        float distance = displacement.magnitude;
        Vector2 aimDirection = displacement.normalized;
        
        // Calculate speed based on distance
        // Empirical formula: speed ? distance * 0.38
        float baseSpeed = distance * 0.38f;
        
        // Collision shots need more weight to drive through
        if (isCollisionShot)
        {
            baseSpeed *= 1.15f;
        }
        
        Debug.Log($"[SimpleVelocityCalc] Target: {targetPosition}, Distance: {distance:F2}, Speed: {baseSpeed:F2}, InTurn: {isInTurn}");
        
        return aimDirection * baseSpeed;
    }
}
