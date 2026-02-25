using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Target : MonoBehaviour
{
    public GameManager gm;
    public TutorialManager tm;
    public RockManager rm;

    public AIManager aim;
    public AI_Shooter aiShoot;
    public AI_Strategy aiStrat;

    Rock_Info rockInfo;
    Rock_Flick rockFlick;
    Rigidbody2D rockRB;

    public GameObject closestRock;
    Rock_Info closestRockInfo;

    public Transform cenGuard;
    public Transform tCenGuard;
    public Transform lCornGuard;
    public Transform rCornGuard;

    public float takeOutOffset;
    public float peelOffset;
    public float raiseOffset;
    public float tickOffset;
    public Transform aiTarget;
    public Transform playerTarget;
    public Vector2 targetPos;
    float targetX;
    float targetY;
    public float takeOutX;
    public float takeOutY;
    float raiseY;
    
    // Physics-based targeting
    private TrajectorySimulator trajectorySimulator;
    
    [Header("Curl Tuning - MUST MATCH TrajectoryLine!")]
    [Tooltip("Curl strength for AI simulation. CRITICAL: This must match the curlStrength in TrajectoryLine.cs!\n" +
             "If AI shots miss laterally (curl too much/little), adjust this to match player trajectory.\n" +
             "Example values:\n" +
             "0.3 = old weak curl\n" +
             "13.8 = strong visible curl\n" +
             "Simulation curl = this value * 0.01 (multiplier in TrajectorySimulator)")]
    public float curlStrength = 13.8f;  // INCREASED to match stronger curl! Was 0.3, now matches player preview
    
    [Header("Takeout Tuning")]
    [Tooltip("How far BEFORE the target to aim (in rock radii). Controls collision geometry for nose hits.\n" +
             "0.5 = very close (shallow angle)\n" +
             "1.5 = balanced (good nose hit)\n" +
             "2.0 = further back (steeper approach)\n" +
             "Scoring now based on hit angle: -90° = perfect nose hit")]
    [Range(0.5f, 3.0f)]
    public float aimPointRadiusMultiplier = 1.5f;
    
    void Start()
    {
        // CRITICAL: AI must use the SAME physics as player trajectory!
        // Get the TrajectoryLine singleton and use its simulator
        TrajectoryLine playerTrajectory = FindObjectOfType<TrajectoryLine>();
        
        if (playerTrajectory != null)
        {
            // Read params from TrajectoryLine to create matching simulator
            trajectorySimulator = new TrajectorySimulator(
                playerTrajectory.iceFriction,
                playerTrajectory.curlStrength
            );
            
            Debug.Log($"[AI_Target] ✓ Using PLAYER trajectory physics: " +
                      $"friction={playerTrajectory.iceFriction:F3}, " +
                      $"curl={playerTrajectory.curlStrength:F3}, " +
                      $"velocityMultiplier={playerTrajectory.velocityMultiplier:F2}, " +
                      $"minVel={playerTrajectory.minVelocity:F2}, " +
                      $"maxVel={playerTrajectory.maxVelocity:F2}");
        }
        else
        {
            // Fallback if TrajectoryLine not found (shouldn't happen)
            Debug.LogWarning("[AI_Target] TrajectoryLine not found! Using fallback physics.");
            trajectorySimulator = new TrajectorySimulator(0.62f, 0.25f);
        }
    }
    
    /// <summary>
    /// Physics-based targeting: Calculate exact pullback position to hit a target rock
    /// NEW APPROACH: Uses lateral sweep with known-good velocities instead of inverse velocity calculation
    /// This is much more robust because it uses empirically-proven speeds and finds actual intersections!
    /// </summary>
    private bool CalculatePhysicsBasedShot(Vector2 targetRockPosition, out Vector2 pullbackPosition, out bool useInTurn, string shotType = "Take Out", int targetRockIndex = -1)
    {
        Vector2 launcherPos = new Vector2(0f, -25f);
        
        // Get all rocks in play - INCLUDING TARGET for collision shots!
        // CRITICAL FIX: For takeout shots, we WANT to hit the target rock, so include it!
        List<GameObject> rocksInPlay = new List<GameObject>();
        for (int i = 0; i < gm.rockList.Count; i++)
        {
            var rockEntry = gm.rockList[i];
            if (rockEntry.rock != null 
                && rockEntry.rock.activeInHierarchy 
                && rockEntry.rockInfo.inPlay)
            {
                // For collision shots (takeout/peel), include ALL rocks including target
                // For draw shots (targetRockIndex < 0), exclude all rocks
                bool shouldInclude = (targetRockIndex >= 0); // Include all if we're trying to hit something
                
                if (shouldInclude || i != targetRockIndex)
                {
                    rocksInPlay.Add(rockEntry.rock);
                    
                    if (i == targetRockIndex)
                    {
                        Debug.Log($"[AI_Target] ✅ TARGET ROCK #{i} ({rockEntry.rock.name}) INCLUDED in simulation at {rockEntry.rock.transform.position}");
                    }
                }
            }
        }
        
        Debug.Log($"[AI_Target] Total rocks in play for simulation: {rocksInPlay.Count}");
        if (targetRockIndex >= 0 && gm.rockList[targetRockIndex].rock != null)
        {
            Debug.Log($"[AI_Target] Target rock reference: {gm.rockList[targetRockIndex].rock.name} at {gm.rockList[targetRockIndex].rock.transform.position}");
            
            // CRITICAL: Verify target is in the obstacles list
            bool targetInObstacles = rocksInPlay.Contains(gm.rockList[targetRockIndex].rock);
            Debug.Log($"[AI_Target] Target rock IS in obstacles list: {targetInObstacles}");
        }
        
        float bestScore = float.MinValue;
        Vector2 bestPullback = Vector2.zero;
        bool bestInTurn = false;
        
        Debug.Log($"[AI_Target] ========== STARTING GEOMETRIC AIM POINT SWEEP ==========");
        Debug.Log($"[AI_Target] Shot type: {shotType}, Target: {targetRockPosition}");
        Debug.Log($"[AI_Target] Launcher: {launcherPos}, Obstacles: {rocksInPlay.Count}");
        
        // STAGE 1: CALCULATE TARGET IMPACT POINT
        // This is where we want the SHOOTER rock's CENTER to be when it hits the target
        // Different from the final aim point (which determines velocity)
        float rockRadius = 0.14f;  // ACTUAL rock radius (not diameter!)
        
        Vector2 targetImpactPoint; // Where we want shooter to be at collision
        Vector2 velocityAimPoint;  // Far point used to calculate required velocity
        
        if (shotType == "Take Out" || shotType == "Peel" || shotType == "Runback")
        {
            // Get TrajectoryLine parameters
            TrajectoryLine playerTrajectory = FindObjectOfType<TrajectoryLine>();
            float velocityMultiplier = playerTrajectory != null ? playerTrajectory.velocityMultiplier : 5.0f;
            
            // CRITICAL: Shot weight calibration!
            // Reference: House draws converge at ~8.7 m/s (proven by physics)
            // Takeout/Peel/Runback must be HEAVIER than draws!
            float desiredPullbackDistance;
            
            if (shotType == "Runback")
            {
                // RUNBACK: HEAVIEST weight - must drive through 2 rocks!
                // Strategy: Hit guard with massive momentum to blast through to target behind
                desiredPullbackDistance = 4.9f; // MAXIMUM weight → 13.5 m/s
                
                // Nose hit on guard, rely on momentum to carry through
                float impactOffset = 2f * rockRadius; // Standard collision distance
                targetImpactPoint = new Vector2(
                    targetRockPosition.x,
                    targetRockPosition.y - impactOffset
                );
                
                Debug.Log($"[AI_Target] RUNBACK: Maximum drive-through\n" +
                          $"  Target (guard): {targetRockPosition}\n" +
                          $"  Pullback: {desiredPullbackDistance:F2} (MAXIMUM)\n" +
                          $"  Expected velocity: {desiredPullbackDistance * velocityMultiplier:F2} m/s\n" +
                          $"  Strategy: Blast through guard to remove target behind");
            }
            else if (shotType == "Peel")
            {
                // PEEL: HEAVY weight + 45° angled hit
                // Strategy: Glancing blow at angle to send both rocks sideways
                // Must be heavier than takeout to ensure both rocks exit
                desiredPullbackDistance = 4.4f; // Heavy → 12.1 m/s for drive-through
                
                // ANGLED HIT: Aim at SIDE of rock (45° approach) for glancing blow
                float angleOffset = rockRadius * 0.7f; // Offset by ~70% of radius
                
                // Alternate sides based on target X
                if (targetRockPosition.x > 0f)
                {
                    angleOffset = -angleOffset; // Target on right, hit from left
                }
                
                targetImpactPoint = new Vector2(
                    targetRockPosition.x + angleOffset, // SIDE impact (not center)
                    targetRockPosition.y - rockRadius * 1.5f // Slightly behind
                );
                
                Debug.Log($"[AI_Target] PEEL: 45° angled hit + heavy weight\n" +
                          $"  Target: {targetRockPosition}\n" +
                          $"  Angle offset: {angleOffset:F3}\n" +
                          $"  Impact point: {targetImpactPoint}\n" +
                          $"  Pullback: {desiredPullbackDistance:F2} (HEAVY)\n" +
                          $"  Expected velocity: {desiredPullbackDistance * velocityMultiplier:F2} m/s");
            }
            else // "Take Out"
            {
                // TAKEOUT: Hit and stay weight
                // Strategy: Nose hit with enough momentum to remove target, shooter stays in play
                // Must be > draw weight (8.7) but < peel weight (12.1)
                desiredPullbackDistance = 3.6f; // Hit-and-stay → 9.9 m/s
                
                // NOSE HIT: Center-to-center collision
                float impactOffset = 2f * rockRadius; // Exact collision distance
                targetImpactPoint = new Vector2(
                    targetRockPosition.x,
                    targetRockPosition.y - impactOffset
                );
                
                Debug.Log($"[AI_Target] TAKEOUT: Nose hit + controlled weight\n" +
                          $"  Target: {targetRockPosition}\n" +
                          $"  Impact offset: {impactOffset:F3}\n" +
                          $"  Impact point: {targetImpactPoint}\n" +
                          $"  Pullback: {desiredPullbackDistance:F2}\n" +
                          $"  Expected velocity: {desiredPullbackDistance * velocityMultiplier:F2} m/s\n" +
                          $"  Strategy: Hit and stay in play");
            }
            
            // Calculate velocity using PLAYER'S formula: velocity = pullback * multiplier
            float desiredVelocityMagnitude = desiredPullbackDistance * velocityMultiplier;
            
            // Aim point is just straight ahead (velocity direction will be adjusted by lateral offset)
            velocityAimPoint = new Vector2(
                targetRockPosition.x,
                launcherPos.y + desiredVelocityMagnitude  // Simple: start + velocity magnitude
            );
            
            Debug.Log($"[AI_Target] {shotType} velocity calculation:\n" +
                      $"  Desired pullback: {desiredPullbackDistance:F3}\n" +
                      $"  Velocity multiplier: {velocityMultiplier:F2}\n" +
                      $"  Desired velocity: {desiredVelocityMagnitude:F2} m/s\n" +
                      $"  Target impact point: {targetImpactPoint}\n" +
                      $"  Velocity aim point: {velocityAimPoint}");
        }
        else if (shotType == "Tap Back" || shotType == "Raise")
        {
            // TAP/RAISE: Very light weight - just nudge the rock forward
            // Much lighter than takeout, just enough to move target without blasting it
            TrajectoryLine playerTrajectory = FindObjectOfType<TrajectoryLine>();
            float velocityMultiplier = playerTrajectory != null ? playerTrajectory.velocityMultiplier : 5.0f;
            float desiredPullbackDistance = 2.8f; // Light touch → ~7.7 m/s
            float velocityMagnitude = desiredPullbackDistance * velocityMultiplier;
            
            // Aim directly at target (no offset needed for light contact)
            targetImpactPoint = targetRockPosition;
            velocityAimPoint = new Vector2(
                targetRockPosition.x,
                launcherPos.y + velocityMagnitude
            );
            
            Debug.Log($"[AI_Target] TAP/RAISE: Light touch shot\n" +
                      $"  Target: {targetRockPosition}\n" +
                      $"  Pullback: {desiredPullbackDistance:F2}\n" +
                      $"  Expected velocity: {velocityMagnitude:F2} m/s\n" +
                      $"  Strategy: Gentle push, both rocks stay in play");
        }
        else // Tick, etc
        {
            targetImpactPoint = targetRockPosition;
            velocityAimPoint = new Vector2(targetRockPosition.x, Mathf.Min(targetRockPosition.y + 1.5f, 8f));
        }
        
        // GEOMETRIC AIM POINT SWEEP: Try different lateral positions to find the BEST angle for a nose hit
        // This is the key to accuracy - we're finding the perfect approach angle!
        
        // Try both turn directions
        for (int turnDir = 0; turnDir < 2; turnDir++)
        {
            bool tryInTurn = (turnDir == 0);
            
            Debug.Log($"[AI_Target] --- Testing {(tryInTurn ? "IN-TURN" : "OUT-TURN")} ---");
            
            // LATERAL SWEEP: Test different lateral offsets to find the best approach angle
            // CURL COMPENSATION LOGIC:
            // IN-TURN (curls LEFT ←): Aim RIGHT of target (POSITIVE offset) to compensate
            // OUT-TURN (curls RIGHT →): Aim LEFT of target (NEGATIVE offset) to compensate
            // This is the OPPOSITE of the curl direction!
            float offsetMultiplier = tryInTurn ? 1f : -1f; // IN-TURN = positive (right), OUT-TURN = negative (left)
            
            Debug.Log($"[AI_Target] Offset multiplier for {(tryInTurn ? "IN-TURN" : "OUT-TURN")}: {offsetMultiplier}");
            
            // FOUR-PHASE MICROSCOPIC SEARCH:
            // Phase 1: Coarse sweep to find general region (0 to +1.2 in 0.12 steps) = 11 positions
            // Phase 2: Medium sweep around best coarse (±0.12 in 0.012 steps) = 21 positions  
            // Phase 3: Fine sweep around best medium (±0.012 in 0.002 steps) = 13 positions
            // Phase 4: Microscopic sweep around best fine (±0.002 in 0.0005 steps) = 9 positions
            // Total: ~54 simulations for SUB-MILLIMETER precision (0.5mm!)
            // NOTE: lateralOffsetBase is UNSIGNED (0 to +1.2), offsetMultiplier determines direction
            
            float bestCoarseOffset = 0f;
            float bestCoarseScore = float.MinValue;
            
            // PHASE 1: COARSE SWEEP (11 positions) - Find general region
            for (float lateralOffsetBase = 0f; lateralOffsetBase <= 1.2f; lateralOffsetBase += 0.12f)
            {
                float lateralOffset = lateralOffsetBase * offsetMultiplier;
                
                // DETERMINISTIC VELOCITY: Recalibrated shot weights
                // Draw: 8.7 m/s | Takeout: 9.9 m/s | Peel: 12.1 m/s | Runback: 13.5 m/s
                TrajectoryLine playerTrajectory = FindObjectOfType<TrajectoryLine>();
                float velocityMultiplier = playerTrajectory != null ? playerTrajectory.velocityMultiplier : 5.0f;
                float desiredPullbackDistance = (shotType == "Runback") ? 4.9f : (shotType == "Peel") ? 4.4f : 3.6f;
                float velocityMagnitude = desiredPullbackDistance * velocityMultiplier;
                
                // Create velocity vector pointing toward target with lateral offset
                Vector2 targetWithOffset = new Vector2(targetRockPosition.x + lateralOffset, targetRockPosition.y);
                Vector2 direction = (targetWithOffset - launcherPos).normalized;
                Vector2 baseVelocity = direction * velocityMagnitude;
                
                if (baseVelocity.magnitude < 3f || baseVelocity.magnitude > 20f) continue;
                
                Vector2 testPullback = CalculatePullbackFromVelocity(baseVelocity, launcherPos, tryInTurn);
                List<Vector2> path = trajectorySimulator.SimulateTrajectory(launcherPos, baseVelocity, tryInTurn, 250, rocksInPlay, forPlayerPreview: false);
                TrajectorySimulator.CollisionInfo collisionInfo = trajectorySimulator.GetCollisionInfo();
                
                if (path.Count == 0) continue;
                if (!collisionInfo.hasCollision || targetRockIndex < 0 || collisionInfo.hitRock == null) continue;
                if (collisionInfo.hitRock != gm.rockList[targetRockIndex].rock) continue;
                
                Vector2 hitVector = collisionInfo.collisionPoint - (Vector2)gm.rockList[targetRockIndex].rock.transform.position;
                if (hitVector.y >= -0.05f) continue; // Must hit from behind
                
                float lateralError = Mathf.Abs(hitVector.x);
                float hitQuality = 1.0f - Mathf.Clamp01(lateralError / 0.1f);
                float score = 100f * hitQuality;
                
                if (score > bestCoarseScore)
                {
                    bestCoarseScore = score;
                    bestCoarseOffset = lateralOffsetBase;
                }
            }
            
            // PHASE 2: MEDIUM SWEEP around best coarse result (21 positions)
            float bestMediumOffset = bestCoarseOffset;
            float bestMediumScore = bestCoarseScore;
            
            float mediumStart = Mathf.Max(0f, bestCoarseOffset - 0.12f);
            float mediumEnd = Mathf.Min(1.2f, bestCoarseOffset + 0.12f);
            
            for (float lateralOffsetBase = mediumStart; lateralOffsetBase <= mediumEnd; lateralOffsetBase += 0.012f)
            {
                float lateralOffset = lateralOffsetBase * offsetMultiplier;
                
                // DETERMINISTIC VELOCITY: Recalibrated weights
                TrajectoryLine playerTrajectory = FindObjectOfType<TrajectoryLine>();
                float velocityMultiplier = playerTrajectory != null ? playerTrajectory.velocityMultiplier : 5.0f;
                float desiredPullbackDistance = (shotType == "Runback") ? 4.9f : (shotType == "Peel") ? 4.4f : 3.6f;
                float velocityMagnitude = desiredPullbackDistance * velocityMultiplier;
                
                Vector2 targetWithOffset = new Vector2(targetRockPosition.x + lateralOffset, targetRockPosition.y);
                Vector2 direction = (targetWithOffset - launcherPos).normalized;
                Vector2 baseVelocity = direction * velocityMagnitude;
                
                if (baseVelocity.magnitude < 3f || baseVelocity.magnitude > 20f) continue;
                
                Vector2 testPullback = CalculatePullbackFromVelocity(baseVelocity, launcherPos, tryInTurn);
                List<Vector2> path = trajectorySimulator.SimulateTrajectory(launcherPos, baseVelocity, tryInTurn, 250, rocksInPlay, forPlayerPreview: false);
                TrajectorySimulator.CollisionInfo collisionInfo = trajectorySimulator.GetCollisionInfo();
                
                if (path.Count == 0) continue;
                if (!collisionInfo.hasCollision || targetRockIndex < 0 || collisionInfo.hitRock == null) continue;
                if (collisionInfo.hitRock != gm.rockList[targetRockIndex].rock) continue;
                
                Vector2 hitVector = collisionInfo.collisionPoint - (Vector2)gm.rockList[targetRockIndex].rock.transform.position;
                if (hitVector.y >= -0.05f) continue;
                
                float lateralError = Mathf.Abs(hitVector.x);
                float hitQuality = 1.0f - Mathf.Clamp01(lateralError / 0.1f);
                float score = 100f * hitQuality;
                
                if (score > bestMediumScore)
                {
                    bestMediumScore = score;
                    bestMediumOffset = lateralOffsetBase;
                }
            }
            
            // PHASE 3: FINE SWEEP around best medium result (13 positions) - 2mm precision!
            float bestFineOffset = bestMediumOffset;
            float bestFineScore = bestMediumScore;
            
            float fineStart = Mathf.Max(0f, bestMediumOffset - 0.012f);
            float fineEnd = Mathf.Min(1.2f, bestMediumOffset + 0.012f);
            
            for (float lateralOffsetBase = fineStart; lateralOffsetBase <= fineEnd; lateralOffsetBase += 0.002f)
            {
                float lateralOffset = lateralOffsetBase * offsetMultiplier;
                
                // DETERMINISTIC VELOCITY: Recalibrated weights
                TrajectoryLine playerTrajectory = FindObjectOfType<TrajectoryLine>();
                float velocityMultiplier = playerTrajectory != null ? playerTrajectory.velocityMultiplier : 5.0f;
                float desiredPullbackDistance = (shotType == "Runback") ? 4.9f : (shotType == "Peel") ? 4.4f : 3.6f;
                float velocityMagnitude = desiredPullbackDistance * velocityMultiplier;
                
                Vector2 targetWithOffset = new Vector2(targetRockPosition.x + lateralOffset, targetRockPosition.y);
                Vector2 direction = (targetWithOffset - launcherPos).normalized;
                Vector2 baseVelocity = direction * velocityMagnitude;
                
                if (baseVelocity.magnitude < 3f || baseVelocity.magnitude > 20f) continue;
                
                Vector2 testPullback = CalculatePullbackFromVelocity(baseVelocity, launcherPos, tryInTurn);
                List<Vector2> path = trajectorySimulator.SimulateTrajectory(launcherPos, baseVelocity, tryInTurn, 250, rocksInPlay, forPlayerPreview: false);
                TrajectorySimulator.CollisionInfo collisionInfo = trajectorySimulator.GetCollisionInfo();
                
                if (path.Count == 0) continue;
                if (!collisionInfo.hasCollision || targetRockIndex < 0 || collisionInfo.hitRock == null) continue;
                if (collisionInfo.hitRock != gm.rockList[targetRockIndex].rock) continue;
                
                Vector2 hitVector = collisionInfo.collisionPoint - (Vector2)gm.rockList[targetRockIndex].rock.transform.position;
                if (hitVector.y >= -0.05f) continue;
                
                float lateralError = Mathf.Abs(hitVector.x);
                float hitQuality = 1.0f - Mathf.Clamp01(lateralError / 0.1f);
                float score = 100f * hitQuality;
                
                if (score > bestFineScore)
                {
                    bestFineScore = score;
                    bestFineOffset = lateralOffsetBase;
                }
            }
            
            // PHASE 4: MICROSCOPIC SWEEP around best fine result (9 positions) - 0.5mm precision! 🔬
            float microStart = Mathf.Max(0f, bestFineOffset - 0.002f);
            float microEnd = Mathf.Min(1.2f, bestFineOffset + 0.002f);
            
            for (float lateralOffsetBase = microStart; lateralOffsetBase <= microEnd; lateralOffsetBase += 0.0005f)
            {
                // Apply multiplier based on turn direction
                float lateralOffset = lateralOffsetBase * offsetMultiplier;
                
                Debug.Log($"[AI_Target] Phase 4: lateralOffsetBase={lateralOffsetBase:F4}, multiplier={offsetMultiplier}, final lateralOffset={lateralOffset:F4}");
                
                // DETERMINISTIC VELOCITY: Recalibrated weights!
                // Takeout: 3.6 → 9.9 m/s | Peel: 4.4 → 12.1 m/s | Runback: 4.9 → 13.5 m/s
                TrajectoryLine playerTrajectory = FindObjectOfType<TrajectoryLine>();
                float velocityMultiplier = playerTrajectory != null ? playerTrajectory.velocityMultiplier : 5.0f;
                float desiredPullbackDistance = (shotType == "Runback") ? 4.9f : (shotType == "Peel") ? 4.4f : 3.6f;
                float velocityMagnitude = desiredPullbackDistance * velocityMultiplier;
                
                // Aim toward target with lateral offset
                Vector2 targetWithOffset = new Vector2(targetRockPosition.x + lateralOffset, targetRockPosition.y);
                Vector2 direction = (targetWithOffset - launcherPos).normalized;
                Vector2 baseVelocity = direction * velocityMagnitude;
                
                // Test this velocity
                if (baseVelocity.magnitude < 3f || baseVelocity.magnitude > 20f)
                {
                    continue;
                }
                
                // Convert to pullback
                Vector2 testPullback = CalculatePullbackFromVelocity(baseVelocity, launcherPos, tryInTurn);
                
                // Simulate trajectory
                List<Vector2> path = trajectorySimulator.SimulateTrajectory(
                    launcherPos, baseVelocity, tryInTurn, 250, rocksInPlay, forPlayerPreview: false);
                
                // CRITICAL: Get collision info IMMEDIATELY!
                TrajectorySimulator.CollisionInfo collisionInfo = trajectorySimulator.GetCollisionInfo();
                
                if (path.Count == 0)
                {
                    continue;
                }
                
                // Check if we hit the target rock
                if (collisionInfo.hasCollision && targetRockIndex >= 0 && collisionInfo.hitRock != null)
                {
                    GameObject hitRockGameObject = collisionInfo.hitRock;
                    GameObject targetRockGameObject = gm.rockList[targetRockIndex].rock;
                    
                    // CRITICAL FIX: Compare by rock name (more reliable than GameObject reference)
                    string hitRockName = hitRockGameObject.name;
                    string targetRockName = targetRockGameObject.name;
                    
                    Debug.Log($"[Hit Detection] Hit rock: '{hitRockName}' vs Target rock: '{targetRockName}'");
                    
                    if (hitRockName == targetRockName || hitRockGameObject == targetRockGameObject)
                    {
                        // HIT! Calculate quality based on how centered the hit is
                        Vector2 liveTargetPos = targetRockGameObject.transform.position;
                        Vector2 collisionPoint = collisionInfo.collisionPoint;
                        float distToCenter = Vector2.Distance(collisionPoint, liveTargetPos);
                        
                        // Calculate hit angle (where on the rock's circumference we hit)
                        Vector2 hitVector = collisionPoint - liveTargetPos;
                        float hitAngle = Mathf.Atan2(hitVector.y, hitVector.x) * Mathf.Rad2Deg;
                        
                        // Calculate approach angle (direction rock was traveling)
                        Vector2 approachDirection = baseVelocity.normalized;
                        float approachAngle = Mathf.Atan2(approachDirection.y, approachDirection.x) * Mathf.Rad2Deg;
                        
                        // CRITICAL: Must hit from behind (below the target)
                        // Y offset should be NEGATIVE (hitting from below = approaching from behind)
                        // EXCEPTION: Tap/Raise shots can hit from ANY angle (lighter contact)
                        bool isFromBehind = hitVector.y < -0.05f; // Must be approaching from below
                        
                        if (!isFromBehind && shotType != "Tap Back" && shotType != "Raise")
                        {
                            Debug.Log($"[AI_Target] ⚠️ REJECTED - Not hitting from behind! Y offset={hitVector.y:F3} (need Y < -0.05)");
                            continue; // Skip this hit
                        }
                        
                        if (!isFromBehind && (shotType == "Tap Back" || shotType == "Raise"))
                        {
                            Debug.Log($"[AI_Target] ℹ️ Tap/Raise from angle - Y offset={hitVector.y:F3} (ANY angle OK for light contact)");
                        }
                        
                        // STRATEGIC DEFLECTION SCORING: Analyze post-collision outcomes
                        
                        // PART 1: Nose Hit Angle (40% of score)
                        float perfectNoseAngle = -90f;
                        float angleDeviation = Mathf.Abs(hitAngle - perfectNoseAngle);
                        float maxAcceptableAngleDeviation = 30f;
                        float noseAngleQuality = 1.0f - Mathf.Clamp01(angleDeviation / maxAcceptableAngleDeviation);
                        
                        // PART 2: Distance to Target Impact Point (40% of score)
                        Vector2 shooterCenterAtCollision = collisionInfo.shooterCenterAtCollision;
                        float distanceToTargetImpact = Vector2.Distance(shooterCenterAtCollision, targetImpactPoint);
                        float maxAcceptableDistance = 0.2f;
                        float impactPointQuality = 1.0f - Mathf.Clamp01(distanceToTargetImpact / maxAcceptableDistance);
                        
                        // PART 3: Strategic Outcome Quality (20% of score) - NEW!
                        // Simplified evaluation - just check rock positioning
                        float strategicQuality = 0.5f; // Default: neutral
                        
                        // Check where shooter ends up
                        Vector2 shooterFinalPos = collisionInfo.finalPosition;
                        Vector2 button = new Vector2(0f, 6.5f);
                        float distToButton = Vector2.Distance(shooterFinalPos, button);
                        
                        // Bonus for good final position
                        if (distToButton < 0.6f) strategicQuality += 0.3f; // In 4-foot
                        else if (distToButton < 1.2f) strategicQuality += 0.1f; // In 8-foot
                        
                        // Check where target ends up
                        Vector2 targetFinalPos = collisionInfo.hitRockFinalPosition;
                        float targetDistToButton = Vector2.Distance(targetFinalPos, button);
                        
                        // Penalty if target stays in good position
                        if (targetDistToButton < 1.2f) strategicQuality -= 0.2f;
                        
                        strategicQuality = Mathf.Clamp01(strategicQuality);
                        
                        // COMBINED SCORE: All three factors
                        float hitQuality = (noseAngleQuality * 0.40f) + (impactPointQuality * 0.40f) + (strategicQuality * 0.20f);
                        
                        Debug.Log($"[AI_Target] Strategic Deflection Scoring:\n" +
                                  $"  Nose Angle Quality: {noseAngleQuality:F3} (angle dev: {angleDeviation:F1}°)\n" +
                                  $"  Impact Point Quality: {impactPointQuality:F3} (dist: {distanceToTargetImpact:F3})\n" +
                                  $"  Strategic Quality: {strategicQuality:F3} (positioning)\n" +
                                  $"  Combined Hit Quality: {hitQuality:F3}");
                        
                        // 🔍 COMPREHENSIVE DEBUG: ALL INFO IN ONE PLACE
                        Debug.Log($"🎯 HIT DIAGNOSTIC (HYBRID SCORING)\n" +
                                  $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                                  $"TURN: {(tryInTurn ? "IN-TURN (curls LEFT ←)" : "OUT-TURN (curls RIGHT →)")}\n" +
                                  $"TARGETING:\n" +
                                  $"  • Lateral Offset: {lateralOffset:F3}\n" +
                                  $"  • Target with Offset: ({targetRockPosition.x + lateralOffset:F3}, {targetRockPosition.y:F3}) (aim point)\n" +
                                  $"  • Velocity: {baseVelocity.magnitude:F2} m/s (DETERMINISTIC: 1.916 pullback × multiplier)\n" +
                                  $"  • Target Impact Point: {targetImpactPoint} (where we WANT shooter at collision)\n" +
                                  $"  • Shooter Center at Collision: {shooterCenterAtCollision} (where shooter ACTUALLY was)\n" +
                                  $"  • Target Rock Center: {liveTargetPos}\n" +
                                  $"  • Aim Point Multiplier: {aimPointRadiusMultiplier:F2}\n" +
                                  $"\n" +
                                  $"VELOCITY:\n" +
                                  $"  • Magnitude: {baseVelocity.magnitude:F2}\n" +
                                  $"  • Direction: {baseVelocity}\n" +
                                  $"  • Approach Angle: {approachAngle:F1}°\n" +
                                  $"\n" +
                                  $"COLLISION:\n" +
                                  $"  • Hit Point: {collisionPoint}\n" +
                                  $"  • Target Center: {liveTargetPos}\n" +
                                  $"  • Distance to Center: {distToCenter:F3} (radius = 0.29)\n" +
                                  $"  • Hit Angle on Rock: {hitAngle:F1}° (perfect nose = -90°)\n" +
                                  $"  • Angle Deviation: {angleDeviation:F1}°\n" +
                                  $"\n" +
                                  $"SCORING BREAKDOWN:\n" +
                                  $"  • Nose Angle Quality: {(noseAngleQuality * 100f):F1}% (50% weight)\n" +
                                  $"  • Impact Point Quality: {(impactPointQuality * 100f):F1}% (50% weight)\n" +
                                  $"  • Distance to Target Impact: {distanceToTargetImpact:F3} units\n" +
                                  $"  • COMBINED Score: {(hitQuality * 100f):F2}/100\n" +
                                  $"  • Current Best: {bestScore:F2}/100\n" +
                                  $"\n" +
                                  $"GEOMETRY:\n" +
                                  $"  • X offset: {hitVector.x:F3} (+ = right, - = left)\n" +
                                  $"  • Y offset: {hitVector.y:F3} (+ = top, - = bottom)\n" +
                                  $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                        
                        float score = 100f * hitQuality;
                        
                        if (score >= bestScore)  // FIXED: >= takes latest shot when tied (better geometry)
                        {
                            bestScore = score;
                            bestPullback = testPullback;
                            bestInTurn = tryInTurn;  // Store the turn direction for THIS hit
                            
                            Debug.Log($"⭐ NEW BEST SHOT! Score: {score:F2}, LateralOffset: {lateralOffset:F2}, " +
                                      $"Turn: {(tryInTurn ? "IN-TURN" : "OUT-TURN")}, " +
                                      $"TryInTurn={tryInTurn}, BestInTurn={bestInTurn}");
                        }
                    }
                    else
                    {
                        Debug.Log($"❌ Hit DIFFERENT ROCK: {hitRockName} (target is {targetRockName}) at lateral offset {lateralOffset:F2}");
                    }
                }
            }
        }
        
        // Return best shot found
        if (bestScore > 0f) // Must have found an intersection!
        {
            pullbackPosition = bestPullback;
            useInTurn = bestInTurn;
            Debug.Log($"[AI_Target] {shotType} SUCCESS! Score: {bestScore:F2}, Pullback: {bestPullback}, InTurn: {useInTurn}");
            return true;
        }
        
        // No intersection found - fallback
        Debug.LogWarning($"[AI_Target] {shotType} FAILED - No intersection found! Target: {targetRockPosition}");
        pullbackPosition = launcherPos + new Vector2(0f, -2f);
        useInTurn = false;
        return false;
    }
    
    /// <summary>
    /// Calculate how well a trajectory path intersects with the target rock
    /// Returns 0 if no intersection, or a quality score (0-1) based on how centered the hit is
    /// </summary>
    private float PathIntersectionQuality(List<Vector2> path, Vector2 targetPos, float rockRadius)
    {
        float bestQuality = 0f;
        float collisionRadius = rockRadius * 2f; // Two rocks touching
        
        for (int i = 0; i < path.Count - 1; i++)
        {
            // Calculate closest point on line segment to target
            float dist = DistancePointToLineSegment(targetPos, path[i], path[i + 1]);
            
            if (dist < collisionRadius)
            {
                // We have an intersection! Calculate quality (1.0 = perfect center hit)
                float quality = 1.0f - (dist / collisionRadius);
                if (quality > bestQuality)
                {
                    bestQuality = quality;
                }
            }
        }
        
        return bestQuality;
    }
    
    /// <summary>
    /// Calculate distance from a point to a line segment
    /// </summary>
    private float DistancePointToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 line = lineEnd - lineStart;
        float len2 = line.sqrMagnitude;
        
        if (len2 < 0.0001f) return Vector2.Distance(point, lineStart);
        
        // Project point onto line segment (clamped to segment)
        float t = Mathf.Clamp01(Vector2.Dot(point - lineStart, line) / len2);
        Vector2 projection = lineStart + t * line;
        
        return Vector2.Distance(point, projection);
    }
    

    /// <summary>
    /// Convert a desired velocity into the required pullback position
    /// CRITICAL: Returns RAW pullback distance (not pre-divided)
    /// The launcher will apply velocityMultiplier when converting back to velocity
    /// </summary>
    private Vector2 CalculatePullbackFromVelocity(Vector2 desiredVelocity, Vector2 launcherPos, bool isInTurn)
    {
        // Get TrajectoryLine parameters for EXACT consistency
        TrajectoryLine playerTrajectory = FindObjectOfType<TrajectoryLine>();
        
        float velocityMultiplier = playerTrajectory != null ? playerTrajectory.velocityMultiplier : 5.0f;
        float minPullbackDistance = playerTrajectory != null ? playerTrajectory.minPullbackDistance : 0.5f;
        float maxPullbackDistance = playerTrajectory != null ? playerTrajectory.maxPullbackDistance : 2.75f;
        
        float velocityMagnitude = desiredVelocity.magnitude;
        
        // CRITICAL: Calculate RAW pullback distance
        // Formula: pullbackDistance = velocity / multiplier
        // This is the PHYSICAL distance the rock is pulled back
        float pullbackDistance = velocityMagnitude / velocityMultiplier;
        
        // Clamp to allowed range
        pullbackDistance = Mathf.Clamp(pullbackDistance, minPullbackDistance, maxPullbackDistance);
        
        // Calculate pullback vector preserving direction
        // Use normalized velocity direction, multiply by raw pullback distance
        Vector2 pullbackDirection = desiredVelocity.normalized;
        Vector2 pullbackOffset = pullbackDirection * pullbackDistance;
        Vector2 pullback = launcherPos - pullbackOffset;
        
        Debug.Log($"[AI Pullback] Velocity: {desiredVelocity} (mag: {velocityMagnitude:F2}) → RAW PullbackDist: {pullbackDistance:F3}\n" +
                  $"  Direction: {pullbackDirection}\n" +
                  $"  Pullback offset: {pullbackOffset} (direction × distance)\n" +
                  $"  Launcher pos: {launcherPos}\n" +
                  $"  Final pullback: {pullback}\n" +
                  $"  (Launcher will multiply {pullbackDistance:F3} × {velocityMultiplier:F2} = {pullbackDistance * velocityMultiplier:F2} m/s)");

        return pullback;
    }
    
    public void PlayerTarget()
    {
        if (playerTarget.position.y >= 5f)
            OnTarget("Player Draw", gm.rockCurrent, 0);
        else
            OnTarget("Player Guard", gm.rockCurrent, 0);
    }

    public void PlayerTakeOut()
    {
        OnTarget("Manual Take Out", gm.rockCurrent, 0);
    }

    public void PlayerPeel()
    {
        OnTarget("Manual Peel", gm.rockCurrent, 0);
    }

    public void OnTarget(string target, int rockCurrent, int rockTarget)
    {
        targetPos = new Vector2(aiTarget.position.x, aiTarget.position.y);

        if (gm.houseList.Count != 0)
        {
            closestRock = gm.houseList[0].rock;
            closestRockInfo = gm.houseList[0].rockInfo;
        }
        rockInfo = gm.rockList[rockCurrent].rockInfo;
        rockFlick = gm.rockList[rockCurrent].rock.GetComponent<Rock_Flick>();
        rockRB = gm.rockList[rockCurrent].rock.GetComponent<Rigidbody2D>();

        switch (target)
        {
            case "Guard Reading":
                StartCoroutine(GuardReading(rockCurrent));
                break;

            case "Manual Take Out":

                targetPos = new Vector2(playerTarget.position.x, playerTarget.position.y);
                StartCoroutine(TakeOutManualTarget(rockCurrent));
                break;

            case "Take Out":

                StartCoroutine(TakeOutTarget(rockCurrent, rockTarget));
                break;

            case "Manual Peel":

                targetPos = new Vector2(playerTarget.position.x, playerTarget.position.y);
                StartCoroutine(PeelManualTarget(rockCurrent));
                break;

            case "Peel":
                StartCoroutine(PeelTarget(rockCurrent, rockTarget));
                break;

            case "Manual Tap Back":
                StartCoroutine(TapManualTarget(rockCurrent));
                break;

            case "Tap Back":
                StartCoroutine(TapTarget(rockCurrent, rockTarget));
                break;

            case "Manual Tick Shot":
                StartCoroutine(TickShotManualTarget(rockCurrent));
                break;

            case "Tick Shot":
                StartCoroutine(TickShotTarget(rockCurrent, rockTarget));
                break;
                
            case "Runback":
                StartCoroutine(RunbackTarget(rockCurrent, rockTarget));
                break;

            case "Auto Guard":
                StartCoroutine(GuardTarget(rockCurrent));
                break;

            case "Auto Draw Twelve Foot":
                StartCoroutine(DrawTarget(rockCurrent, new Vector2 (0f, 5.5f)));
                break;

            case "Auto Draw Four Foot":
                StartCoroutine(DrawTarget(rockCurrent, new Vector2(0f, 6.5f)));
                break;

            case "Freeze":
                StartCoroutine(DrawTarget(rockCurrent, new Vector2 (gm.rockList[rockTarget].rock.transform.position.x, gm.rockList[rockTarget].rock.transform.position.y)));
                break;

            case "Manual Draw":
                StartCoroutine(DrawTarget(rockCurrent, new Vector2(playerTarget.position.x, playerTarget.position.y)));
                break;

            case "Manual Guard":
                StartCoroutine(GuardTarget(rockCurrent));
                break;

            case "Player Draw":
                targetPos = new Vector2(playerTarget.position.x, playerTarget.position.y);
                StartCoroutine(DrawTarget(rockCurrent, new Vector2(playerTarget.position.x, playerTarget.position.y)));
                break;

            case "Player Guard":
                targetPos = new Vector2(playerTarget.position.x, playerTarget.position.y);
                StartCoroutine(GuardTarget(rockCurrent));
                break;
        }
    }

    IEnumerator GuardReading(int rockCurrent)
    {
        //if there's guards
        if (gm.gList.Count != 0)
        {
            //for each item in guard list
            foreach (Guard_List guard in gm.gList)
            {
                float posX;
                float posY;

                posX = guard.lastTransform.position.x;
                posY = guard.lastTransform.position.y;
                // center lane
                if (Mathf.Abs(posX) <= 0.4f)
                {
                    if (posY <= 3.5f)
                    {
                        cenGuard = guard.lastTransform;
                        Debug.Log("Centre Guard - " + guard.lastTransform.position.x + ", " + guard.lastTransform.position.y);
                    }
                    else
                    {
                        tCenGuard = guard.lastTransform;
                        Debug.Log("Tight Centre Guard - " + guard.lastTransform.position.x + ", " + guard.lastTransform.position.y);
                    }
                }
                // left corner 
                else if (posX < -0.4f && posX > -1.25f)
                {
                    lCornGuard = guard.lastTransform;
                    Debug.Log("Left Guard - " + guard.lastTransform.position.x + ", " + guard.lastTransform.position.y);
                }
                // right corner
                else if (posX > 0.4f && posX < 1.25f)
                {
                    rCornGuard = guard.lastTransform;
                    Debug.Log("Right Guard - " + guard.lastTransform.position.x + ", " + guard.lastTransform.position.y);
                }
                else
                {
                    tCenGuard = null;
                    cenGuard = null;
                    lCornGuard = null;
                    rCornGuard = null;
                    Debug.Log("No Guards");
                }
            }

        }
        else
        {
            cenGuard = null;
            lCornGuard = null;
            rCornGuard = null;

            Debug.Log("No Guards");
        }

        yield return new WaitForEndOfFrame();
    }

    IEnumerator TakeOutManualTarget(int rockCurrent)
    {
        targetX = targetPos.x;
        targetY = targetPos.y;

        if (rm.inturn == false)
        {

            takeOutX = (-0.205f * ((targetX + 1.35f) / 2.7f)) + 0.087f;
        }
        else
        {
            takeOutX = (-0.18f * ((targetX + 1.35f) / 2.7f)) + 0.11f;
        }

        aiShoot.OnShot("Take Out", rockCurrent);
        yield break;
    }

    /// <summary>
    /// DEPRECATED: Legacy auto-targeting using magic number formulas
    /// This method makes strategic decisions AND uses magic numbers - bad separation of concerns
    /// Kept for backwards compatibility only. New code should use AI_Strategy + physics-based targeting
    /// </summary>
    IEnumerator TakeOutAutoTarget(int rockCurrent)
    {
        yield return StartCoroutine(GuardReading(rockCurrent));
        yield return new WaitForEndOfFrame();
        #region House Has Rocks
        //if the house has rocks in it
        if (gm.houseList.Count != 0)
        {
            Debug.Log("houseList is not 0");
            //if the closest rock is not my team
            if (closestRockInfo.teamName != rockInfo.teamName)
            {
                //if it's in the four foot
                if (Vector2.Distance(closestRock.transform.position, new Vector2(0f, 6.5f)) <= 0.6f)
                {
                    //if there's no centre guard
                    if (!cenGuard)
                    {
                        targetX = closestRock.transform.position.x;
                        if (targetX > 0f)
                        {
                            rm.inturn = false;
                            takeOutX = (-0.205f * ((targetX + 1.35f) / 2.7f)) + 0.087f;
                        }
                        else
                        {
                            rm.inturn = true;
                            takeOutX = (0.15f * ((targetX - 1.35f) / -2.7f)) - 0.05f;
                        }
                        aiShoot.OnShot("Take Out", rockCurrent);
                        Debug.Log(closestRockInfo.teamName + " " + closestRockInfo.rockNumber);
                        yield break;
                    }
                    else
                    {
                        //if the centre guard is mine
                        if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                        {
                            //let's run it back
                            targetX = cenGuard.position.x;
                            if (targetX > 0f)
                            {
                                rm.inturn = false;
                                takeOutX = (-0.142f * ((targetX) / 1.65f)) - 0.011f;
                            }
                            else
                            {
                                rm.inturn = true;
                                takeOutX = (0.13f * (targetX / -1.65f)) + 0.015f;
                            }
                            aiShoot.OnShot("Take Out", rockCurrent);
                            Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                            yield break;
                        }
                        //if the centre guard is not mine
                        else if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                        {
                            //let's take it out
                            targetX = cenGuard.position.x;
                            if (targetX > 0f)
                            {
                                rm.inturn = false;
                                takeOutX = (-0.142f * ((targetX) / 1.65f)) - 0.011f;
                            }
                            else
                            {
                                rm.inturn = true;
                                takeOutX = (0.13f * (targetX / -1.65f)) + 0.015f;
                            }
                            aiShoot.OnShot("Peel", rockCurrent);
                            Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                            yield break;
                        }
                    }
                }
                //if it's not in the four foot
                else
                {
                    //if there's a centre guard and the closest rock is in the middle
                    if (cenGuard & Mathf.Abs(closestRock.transform.position.x) <= 0.5f)
                    {
                        targetX = cenGuard.position.x;
                        if (targetX > 0f)
                        {
                            rm.inturn = false;
                            takeOutX = (-0.142f * ((targetX) / 1.65f)) - 0.011f;
                        }
                        else
                        {
                            rm.inturn = true;
                            takeOutX = (0.13f * (targetX / -1.65f)) + 0.015f;
                        }
                        aiShoot.OnShot("Raise", rockCurrent);
                        Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                        yield break;
                    }
                    //if the closest rock is to the right and there's a right guard
                    else if (rCornGuard & closestRock.transform.position.x > 0f)
                    {
                        targetX = rCornGuard.position.x;
                        if (targetX > 0f)
                        {
                            rm.inturn = false;
                            takeOutX = (-0.142f * ((targetX) / 1.65f)) - 0.011f;
                        }
                        else
                        {
                            rm.inturn = true;
                            takeOutX = (0.13f * (targetX / -1.65f)) + 0.015f;
                        }
                        aiShoot.OnShot("Peel", rockCurrent);
                        Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                        yield break;
                    }
                    //if there's a left guard and the closest rock is to the left
                    else if (lCornGuard & closestRock.transform.position.x < 0f)
                    {
                        targetX = lCornGuard.position.x;
                        if (targetX > 0f)
                        {
                            rm.inturn = false;
                            takeOutX = (-0.142f * ((targetX) / 1.65f)) - 0.011f;
                        }
                        else
                        {
                            rm.inturn = true;
                            takeOutX = (0.13f * (targetX / -1.65f)) + 0.015f;
                        }
                        aiShoot.OnShot("Peel", rockCurrent);
                        Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                        yield break;
                    }
                    else
                    {
                        Debug.Log("House List Count is " + gm.houseList.Count);
                        targetX = closestRock.transform.position.x;
                        if (targetX > 0f)
                        {
                            rm.inturn = false;
                            takeOutX = (-0.205f * ((targetX + 1.35f) / 2.7f)) + 0.087f;
                        }
                        else
                        {
                            rm.inturn = true;
                            takeOutX = (0.15f * ((targetX - 1.35f) / -2.7f)) - 0.05f;
                        }
                        aiShoot.OnShot("Take Out", rockCurrent);
                        Debug.Log("Target is " + closestRockInfo.teamName + " " + closestRockInfo.rockNumber);
                        yield break;
                    }
                }
            }
            //if the closest rock is my team
            else if (closestRockInfo.teamName == rockInfo.teamName)
            {
                //if there's more than one rock in the house
                if (gm.houseList.Count >= 2)
                {
                    //if the second rock is mine
                    if (gm.houseList[1].rockInfo.teamName == rockInfo.teamName)
                    {
                        //if the second rock is not guarded
                        if (Mathf.Abs(gm.houseList[1].rock.transform.position.x) <= 0.5f & !cenGuard)
                        {
                            aiShoot.OnShot("Left Centre Guard", rockCurrent);
                            Debug.Log("Centre Guard");
                            yield break;
                        }
                        else if (gm.houseList[1].rock.transform.position.x < 0f & !lCornGuard)
                        {
                            aiShoot.OnShot("Left Corner Guard", rockCurrent);
                            Debug.Log("Left Corner Guard");
                            yield break;
                        }
                        else if (gm.houseList[1].rock.transform.position.x > 0f & !rCornGuard)
                        {
                            aiShoot.OnShot("Right Corner Guard", rockCurrent);
                            Debug.Log("Right Corner Guard");
                            yield break;
                        }
                        else
                        {
                            if (gm.houseList.Count >= 3 && gm.houseList[2].rockInfo.teamName != rockInfo.teamName)
                            {

                            }
                            yield return StartCoroutine(DrawFourFoot(gm.rockCurrent));
                            Debug.Log("Drawing Four Foot");
                            yield break;
                        }
                    }
                    //if the second rock is not mine
                    else
                    {
                        //if the second rock is guarded
                        if (Mathf.Abs(gm.houseList[1].rock.transform.position.x) <= 0.5f && cenGuard)
                        {
                            targetX = cenGuard.position.x;
                            takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                            aiShoot.OnShot("Peel", rockCurrent);
                            Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                            yield break;
                        }
                        else if (gm.houseList[1].rock.transform.position.x < 0f && lCornGuard)
                        {
                            targetX = lCornGuard.position.x;
                            takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                            aiShoot.OnShot("Peel", rockCurrent);
                            Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                            yield break;
                        }
                        else if (gm.houseList[1].rock.transform.position.x > 0f && rCornGuard)
                        {
                            targetX = rCornGuard.position.x;
                            takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                            aiShoot.OnShot("Peel", rockCurrent);
                            Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                            yield break;
                        }
                        //if the second rock is not guarded
                        else
                        {
                            targetX = gm.houseList[1].rock.transform.position.x;
                            takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                            aiShoot.OnShot("Take Out", rockCurrent);
                            Debug.Log(gm.houseList[1].rockInfo.teamName + " " + gm.houseList[1].rockInfo.rockNumber);
                            yield break;
                        }
                    }
                }
                //if there's not more that one rock in the house
                else
                {
                    //if the rock is not guarded
                    if (Mathf.Abs(closestRock.transform.position.x) <= 0.5f & !cenGuard)
                    {
                        aiShoot.OnShot("Left Centre Guard", rockCurrent);
                        Debug.Log("Centre Guard");
                        yield break;
                    }
                    else if (closestRock.transform.position.x < 0f & !lCornGuard)
                    {
                        aiShoot.OnShot("Left Corner Guard", rockCurrent);
                        Debug.Log("Left Corner Guard");
                        yield break;
                    }
                    else if (closestRock.transform.position.x > 0f & !rCornGuard)
                    {
                        aiShoot.OnShot("Right Corner Guard", rockCurrent);
                        Debug.Log("Right Corner Guard");
                        yield break;
                    }
                    else
                    {
                        yield return StartCoroutine(DrawFourFoot(gm.rockCurrent));
                        Debug.Log("Drawing Four Foot");
                        yield break;
                    }
                }

            }
        }
        #endregion

        #region No rocks in House, but Guards
        //if there's guards
        else if (gm.gList.Count != 0)
        {
            //centre, left and right guards
            if (cenGuard && rCornGuard && lCornGuard)
            {
                //centre guard is mine
                if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Raise", rockCurrent);
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //right corner guard is not mine
                else if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                    aiShoot.OnShot("Raise", rockCurrent);
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //left corner guard is not mine
                else if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
            }
            //right guard only
            else if (rCornGuard & !cenGuard & !lCornGuard)
            {
                //guard is mine
                if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                    aiShoot.OnShot("Raise", rockCurrent);
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //guard is not mine
                else
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1.65f) / 3.3f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
            }
            //left guard only
            else if (!cenGuard & !rCornGuard & lCornGuard)
            {
                //guard is mine
                if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Raise", rockCurrent);
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //guard is not mine
                else
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
            }
            //right and left guards
            else if (!cenGuard & rCornGuard & lCornGuard)
            {
                //left guard is not mine
                if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //right guard is not mine
                else if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //left guard is mine
                else if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Raise", rockCurrent);
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //right guard is mine
                else if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Raise", rockCurrent);
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else
                {
                    targetX = 0f;
                    takeOutX = 0f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log("No Targets available - Button");
                    yield break;
                }
            }
            //centre and right guards
            else if (cenGuard & rCornGuard & !lCornGuard)
            {
                //centre guard is not mine
                if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //right guard is not mine
                else if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //centre guard is mine
                else if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Raise", rockCurrent);
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //right guard is mine
                else if (rCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = rCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Raise", rockCurrent);
                    Debug.Log(rCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + rCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else
                {
                    targetX = 0f;
                    takeOutX = 0f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log("No Targets available - Button");
                    yield break;
                }
            }
            //centre and left guards
            else if (cenGuard & !rCornGuard & lCornGuard)
            {
                //left guard is not mine
                if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //centre guard is not mine
                else if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName != rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //left guard is mine
                else if (lCornGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = lCornGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(lCornGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + lCornGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //centre guard is mine
                else if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                else
                {
                    aiShoot.OnShot("Button", rockCurrent);
                    Debug.Log("No Targets available - Button");
                    yield break;
                }
            }
            //centre guard only
            else if (cenGuard & !rCornGuard & !lCornGuard)
            {
                //if it's mine
                if (cenGuard.gameObject.GetComponent<Rock_Info>().teamName == rockInfo.teamName)
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Raise", rockCurrent);
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
                //if it's theirs
                else
                {
                    targetX = cenGuard.position.x;
                    takeOutX = (-0.2f * ((targetX + 1f) / 2f)) + 0.1f;
                    aiShoot.OnShot("Peel", rockCurrent);
                    Debug.Log(cenGuard.gameObject.GetComponent<Rock_Info>().teamName + " " + cenGuard.gameObject.GetComponent<Rock_Info>().rockNumber);
                    yield break;
                }
            }
            else
            {
                targetX = 0f;
                takeOutX = 0f;
                yield return StartCoroutine(DrawFourFoot(gm.rockCurrent));
                Debug.Log("No Targets available - Drawing Four Foot");
                yield break;
            }
        }
        #endregion

        #region No guards or rocks in House
        else
        {
            targetX = 0f;
            takeOutX = 0f;
            aiShoot.OnShot("Peel", rockCurrent);
            Debug.Log("No Targets available - Button");
            yield break;
        }
        #endregion

    }

    IEnumerator TakeOutTarget(int rockCurrent, int rockTarget)
    {
        yield return StartCoroutine(GuardReading(rockCurrent));

        // PHYSICS-BASED: Calculate exact shot needed to hit target
        Vector2 targetRockPos = gm.rockList[rockTarget].rock.transform.position;
        Vector2 launcherPos = new Vector2(0f, -27.5f); // Typical launcher position
        
        // CRITICAL DEBUG: Log EVERYTHING about this takeout attempt
        Debug.Log($"\n========== TAKEOUT DEBUG START ==========\n" +
                  $"Target Rock: #{rockTarget} at position {targetRockPos}\n" +
                  $"Launcher position: {launcherPos}\n" +
                  $"Distance to target: {Vector2.Distance(launcherPos, targetRockPos):F2} units\n" +
                  $"Target lateral offset (x): {targetRockPos.x:F3}\n" +
                  $"Current rm.inturn BEFORE calculation: {rm.inturn}\n" +
                  $"Shooter: Rock #{rockCurrent}");
        
        Vector2 pullbackPos;
        bool useInTurn;
        bool foundShot = CalculatePhysicsBasedShot(targetRockPos, out pullbackPos, out useInTurn, "Take Out", rockTarget);
        
        if (foundShot)
        {
            // CRITICAL: Set rm.inturn from physics calculation ONCE
            Debug.Log($"[AI_Target] Physics recommends: {(useInTurn ? "IN-TURN" : "OUT-TURN")}\n" +
                      $"Setting rm.inturn = {useInTurn}");
            
            rm.inturn = useInTurn;
            
            Debug.Log($"[AI_Target] Confirmed rm.inturn is now: {rm.inturn}");
            
            // Apply accuracy-based error to pullback position
            CharacterStats shooterStats = GetShooterStats(rockCurrent);
            Vector2 originalPullback = pullbackPos;
            
            if (shooterStats != null)
            {
                float accuracy = shooterStats.takeOutAccuracy.GetValue(); // 0-100
                
                Debug.Log($"[AI_Target] Shooter takeout skill: {accuracy}/100");
                
                // REALISTIC CURLING ERROR DISTRIBUTION:
                // Weight (Y) errors are 4-5x more common than line (X) errors
                // Professional curlers can control line very well, but weight is hard!
                
                float accuracyRatio = Mathf.Clamp01(accuracy / 100f);
                float baseMaxError = 0.05f;
                float maxError = baseMaxError * (1f - accuracyRatio);
                
                if (maxError > 0f)
                {
                    // Generate separate X and Y errors with weight-dominant distribution
                    float yError = Random.Range(-maxError, maxError); // Full range for weight
                    float xError = Random.Range(-maxError * 0.2f, maxError * 0.2f); // 20% range for line
                    
                    Vector2 errorOffset = new Vector2(xError, yError);
                    
                    // CRITICAL FIX: Lateral error must respect turn direction
                    // IN-TURN (curls LEFT ←): pullback on RIGHT (positive X) -> positive lateral error moves it MORE right (away from curl)
                    // OUT-TURN (curls RIGHT →): pullback on LEFT (negative X) -> negative lateral error moves it MORE left (away from curl)
                    float lateralErrorSign = useInTurn ? 1f : -1f;
                    errorOffset.x *= lateralErrorSign;
                    
                    pullbackPos += errorOffset;
                    
                    Debug.Log($"[AI_Target] Realistic error applied (80% weight / 20% line)\n" +
                              $"  Max error: ±{maxError:F3}\n" +
                              $"  Y error (weight): {yError:F3} (100% range)\n" +
                              $"  X error (line): {xError:F3} (20% range)\n" +
                              $"  Lateral error sign: {lateralErrorSign} (IN-TURN={useInTurn})\n" +
                              $"  Original pullback: {originalPullback}\n" +
                              $"  Final pullback: {pullbackPos}");
                }
                else
                {
                    Debug.Log($"[AI_Target] ⭐ PERFECT ACCURACY (skill 100) - NO ERROR APPLIED! Pullback: {pullbackPos}");
                }
            }
            else
            {
                Debug.Log($"[AI_Target] No shooter stats found - using perfect accuracy (no error)");
            }
            
            takeOutX = pullbackPos.x;
            takeOutY = pullbackPos.y;
            
            // Calculate expected trajectory
            Vector2 shotDirection = (pullbackPos - launcherPos).normalized;
            float shotAngle = Mathf.Atan2(shotDirection.y, shotDirection.x) * Mathf.Rad2Deg;
            
            Debug.Log($"[AI_Target] Take Out SUCCESS\n" +
                      $"Target: {targetRockPos}\n" +
                      $"Pullback: ({takeOutX:F3}, {takeOutY:F3})\n" +
                      $"Shot direction: {shotDirection} (angle: {shotAngle:F1}°)\n" +
                      $"Turn: {(useInTurn ? "IN-TURN (curl LEFT ←)" : "OUT-TURN (curl RIGHT →)")}\n" +
                      $"Expected curl direction: {(useInTurn ? "negative X (left)" : "positive X (right)")}\n" +
                      $"========== TAKEOUT DEBUG END ==========\n");
        }
        else
        {
            // ========================================
            // COMPREHENSIVE FALLBACK SYSTEM
            // ========================================
            Debug.LogWarning($"[AI_Target] Take Out physics FAILED - trying comprehensive fallback for target: {targetRockPos}");
            
            // FALLBACK 1: Try PEEL instead (different shot mechanics might work)
            Debug.Log($"[Fallback 1] Trying PEEL on same target");
            Vector2 peelPullback;
            bool peelInTurn;
            bool foundPeel = CalculatePhysicsBasedShot(targetRockPos, out peelPullback, out peelInTurn, "Peel", rockTarget);
            
            if (foundPeel)
            {
                rm.inturn = peelInTurn;
                takeOutX = peelPullback.x;
                takeOutY = peelPullback.y;
                
                Debug.Log($"[Fallback 1] ✓ PEEL succeeded! Turn: {(peelInTurn ? "IN-TURN" : "OUT-TURN")}, Pullback: ({takeOutX:F3}, {takeOutY:F3})");
                
                aiShoot.OnShot("Peel", rockCurrent);
                Debug.Log($"Fallback Peel - {gm.rockList[rockTarget].rockInfo.teamName} #{gm.rockList[rockTarget].rockInfo.rockNumber}");
                yield break;
            }
            
            // FALLBACK 2: Try ANY other opponent rock in the house
            Debug.Log($"[Fallback 2] Trying other opponent rocks in house");
            Rock_Info currentRockInfo = gm.rockList[rockCurrent].rockInfo;
            
            foreach (var houseRock in gm.houseList)
            {
                if (houseRock.rock == null || !houseRock.rock.activeInHierarchy)
                    continue;
                
                if (houseRock.rockInfo.teamName == currentRockInfo.teamName)
                    continue; // Skip our own rocks
                
                if (houseRock.rockInfo.rockIndex == rockTarget)
                    continue; // Skip original target (already failed)
                
                Vector2 altTargetPos = houseRock.rock.transform.position;
                Debug.Log($"[Fallback 2] Trying alternative rock #{houseRock.rockInfo.rockIndex} at ({altTargetPos.x:F2}, {altTargetPos.y:F2})");
                
                Vector2 altPullback;
                bool altInTurn;
                bool foundAlt = CalculatePhysicsBasedShot(altTargetPos, out altPullback, out altInTurn, "Take Out", houseRock.rockInfo.rockIndex);
                
                if (foundAlt)
                {
                    rm.inturn = altInTurn;
                    takeOutX = altPullback.x;
                    takeOutY = altPullback.y;
                    
                    Debug.Log($"[Fallback 2] ✓ Alternative rock succeeded! Turn: {(altInTurn ? "IN-TURN" : "OUT-TURN")}, Pullback: ({takeOutX:F3}, {takeOutY:F3})");
                    
                    aiShoot.OnShot("Take Out", rockCurrent);
                    Debug.Log($"Fallback Alternative - {houseRock.rockInfo.teamName} #{houseRock.rockInfo.rockIndex}");
                    yield break;
                }
            }
            
            // FALLBACK 3: Try removing ANY opponent guard blocking the center
            Debug.Log($"[Fallback 3] Trying opponent guards");
            
            foreach (var guard in gm.gList)
            {
                if (guard.lastTransform == null)
                    continue;
                
                Rock_Info guardInfo = guard.lastTransform.GetComponent<Rock_Info>();
                if (guardInfo == null || guardInfo.teamName == currentRockInfo.teamName)
                    continue; // Skip our own guards
                
                Vector2 guardPos = guard.lastTransform.position;
                Debug.Log($"[Fallback 3] Trying guard #{guardInfo.rockIndex} at ({guardPos.x:F2}, {guardPos.y:F2})");
                
                Vector2 guardPullback;
                bool guardInTurn;
                bool foundGuard = CalculatePhysicsBasedShot(guardPos, out guardPullback, out guardInTurn, "Take Out", guardInfo.rockIndex);
                
                if (foundGuard)
                {
                    rm.inturn = guardInTurn;
                    takeOutX = guardPullback.x;
                    takeOutY = guardPullback.y;
                    
                    Debug.Log($"[Fallback 3] ✓ Guard takeout succeeded! Turn: {(guardInTurn ? "IN-TURN" : "OUT-TURN")}, Pullback: ({takeOutX:F3}, {takeOutY:F3})");
                    
                    aiShoot.OnShot("Take Out", rockCurrent);
                    Debug.Log($"Fallback Guard - {guardInfo.teamName} #{guardInfo.rockIndex}");
                    yield break;
                }
            }
            
            // FALLBACK 4: Draw to button instead (can't remove anything)
            Debug.Log($"[Fallback 4] All takeout options exhausted - trying draw to button");
            
            Vector2 button = new Vector2(0f, 6.5f);
            Vector2 drawPullback;
            bool drawInTurn;
            bool foundDraw = CalculatePhysicsBasedDrawShot(button, out drawPullback, out drawInTurn);
            
            if (foundDraw)
            {
                rm.inturn = drawInTurn;
                takeOutX = drawPullback.x;
                takeOutY = drawPullback.y;
                
                Debug.Log($"[Fallback 4] ✓ Draw succeeded! Turn: {(drawInTurn ? "IN-TURN" : "OUT-TURN")}, Pullback: ({takeOutX:F3}, {takeOutY:F3})");
                
                aiShoot.OnShot("Draw To Target", rockCurrent);
                Debug.Log($"Fallback Draw - Can't remove anything, drawing to button");
                yield break;
            }
            
            
            // FALLBACK 5: ABSOLUTE LAST RESORT - Just throw away the rock
            Debug.LogError($"[Fallback 5] CATASTROPHIC: Even draw failed - THROWING AWAY ROCK");
            
            // Throw to corner out of bounds
            rm.inturn = (targetRockPos.x < 0f); // In-turn if target is left, out-turn if target is right
            takeOutX = (targetRockPos.x < 0f) ? -1.5f : 1.5f;
            takeOutY = -27.0f;
            
            Debug.LogError($"[Fallback 5] Throwing away - Turn: {(rm.inturn ? "IN-TURN" : "OUT-TURN")}, Pullback: ({takeOutX:F3}, {takeOutY:F3})");
        }

        aiShoot.OnShot("Take Out", rockCurrent);
        Debug.Log(gm.rockList[rockTarget].rockInfo.teamName + " " + gm.rockList[rockTarget].rockInfo.rockNumber);
        yield break;
    }
    
    /// <summary>
    /// Get shooter stats for the current rock
    /// </summary>
    private CharacterStats GetShooterStats(int rockCurrent)
    {
        TeamManager tm = FindObjectOfType<TeamManager>();
        if (tm == null) return null;
        
        // Determine which team member is shooting based on rock number
        int memberIndex = rockCurrent / 4; // 0-3 for lead, second, third, skip
        memberIndex = Mathf.Clamp(memberIndex, 0, 3);
        
        // Get the correct team
        bool isRedTeam = (rockCurrent % 2 == 0) ? gm.redHammer : !gm.redHammer;
        
        if (isRedTeam && tm.teamRed != null && memberIndex < tm.teamRed.Length)
        {
            return tm.teamRed[memberIndex].charStats;
        }
        else if (!isRedTeam && tm.teamYellow != null && memberIndex < tm.teamYellow.Length)
        {
            return tm.teamYellow[memberIndex].charStats;
        }
        
        return null;
    }

    IEnumerator PeelManualTarget(int rockCurrent)
    {
        targetX = targetPos.x;
        targetY = targetPos.y;


        //rm.inturn = false;
        //takeOutX = (-0.205f * ((targetX + 1.35f) / 2.7f)) + 0.087f;

        //rm.inturn = true;
        //takeOutX = (-0.19f * ((targetX + 1.35f) / 2.7f)) + 0.11f;

        if (rm.inturn == false)
        {
            takeOutX = (-0.222f * ((targetX + 1.35f) / 2.7f)) + 0.102f;
        }
        else
        {
            takeOutX = (-0.219f * ((targetX + 1.35f) / 2.7f)) + 0.122f;
        }

        aiShoot.OnShot("Peel", rockCurrent);
        yield break;
    }

    IEnumerator PeelTarget(int rockCurrent, int rockTarget)
    {
        yield return StartCoroutine(GuardReading(rockCurrent));

        // PHYSICS-BASED PEEL: Scan angles to maximize CHAOS (secondary collisions)
        // Strategy: Hit primary target at angle that causes most disruption to other rocks
        Vector2 targetRockPos = gm.rockList[rockTarget].rock.transform.position;
        Rock_Info currentRockInfo = gm.rockList[rockCurrent].rockInfo;
        
        Debug.Log($"[AI_Target] 🎯 PEEL CHAOS MODE - Scanning angles to maximize secondary damage");
        
        // Collect ALL rocks in play (potential secondary targets)
        List<GameObject> rocksInPlay = new List<GameObject>();
        List<GameObject> secondaryTargets = new List<GameObject>();
        
        for (int i = 0; i < gm.rockList.Count; i++)
        {
            var rockEntry = gm.rockList[i];
            if (rockEntry.rock != null && rockEntry.rock.activeInHierarchy && rockEntry.rockInfo.inPlay)
            {
                rocksInPlay.Add(rockEntry.rock);
                
                // Track potential secondary targets (other rocks, especially opponents)
                if (i != rockTarget && rockEntry.rock != gm.rockList[rockTarget].rock)
                {
                    secondaryTargets.Add(rockEntry.rock);
                }
            }
        }
        
        Debug.Log($"[Peel Chaos] Primary target: {targetRockPos}, Secondary targets: {secondaryTargets.Count}");
        
        // ANGLE SWEEP: Try hitting target from different angles (-45° to +45° from direct)
        // More angles = more CPU but better chaos detection
        float[] angleSweep = new float[] { 
            0f,     // Direct hit (baseline)
            -15f,   // Slight left
            15f,    // Slight right
            -30f,   // More left
            30f,    // More right
            -45f,   // Far left (glancing)
            45f     // Far right (glancing)
        };
        
        float bestChaosScore = 0f;
        Vector2 bestPullback = Vector2.zero;
        bool bestInTurn = false;
        float bestAngle = 0f;
        
        foreach (float angle in angleSweep)
        {
            // Calculate aim point with angular offset
            Vector2 launcherPos = new Vector2(0f, -25f);
            Vector2 toLauncher = (launcherPos - targetRockPos).normalized;
            
            // Rotate aim direction by angle
            float angleRad = angle * Mathf.Deg2Rad;
            Vector2 rotatedAim = new Vector2(
                toLauncher.x * Mathf.Cos(angleRad) - toLauncher.y * Mathf.Sin(angleRad),
                toLauncher.x * Mathf.Sin(angleRad) + toLauncher.y * Mathf.Cos(angleRad)
            );
            
            // Aim point is offset from target in this direction
            Vector2 angledTarget = targetRockPos + rotatedAim * 0.15f; // 15cm offset for angle
            
            // Try physics shot at this angle
            Vector2 pullbackPos;
            bool useInTurn;
            bool foundShot = CalculatePhysicsBasedShot(angledTarget, out pullbackPos, out useInTurn, "Peel", rockTarget);
            
            if (!foundShot)
                continue;
            
            // SCORE BASED ON CHAOS: How many rocks get disrupted?
            float chaosScore = ScorePeelChaos(
                currentRockInfo,
                targetRockPos,
                secondaryTargets,
                pullbackPos,
                useInTurn
            );
            
            Debug.Log($"[Peel Chaos] Angle: {angle:F1}° → Chaos Score: {chaosScore:F1}");
            
            if (chaosScore > bestChaosScore)
            {
                bestChaosScore = chaosScore;
                bestPullback = pullbackPos;
                bestInTurn = useInTurn;
                bestAngle = angle;
            }
        }
        
        // Use best chaos shot if found
        if (bestChaosScore > 0f)
        {
            rm.inturn = bestInTurn;
            takeOutX = bestPullback.x;
            takeOutY = bestPullback.y;
            
            Debug.Log($"[AI_Target] 🎯 PEEL CHAOS SUCCESS!\n" +
                      $"  Angle: {bestAngle:F1}°\n" +
                      $"  Chaos Score: {bestChaosScore:F1}\n" +
                      $"  Turn: {(bestInTurn ? "IN-TURN" : "OUT-TURN")}\n" +
                      $"  Pullback: ({takeOutX:F3}, {takeOutY:F3})");
        }
        else
        {
            // FALLBACK PHASE 1: Try SECONDARY opponent rocks in the house
            Debug.LogWarning($"[AI_Target] Primary chaos failed - trying secondary targets");
            
            // Build list of opponent rocks in house (sorted by priority)
            List<GameObject> opponentRocksInHouse = new List<GameObject>();
            
            foreach (var houseRock in gm.houseList)
            {
                if (houseRock.rockInfo.teamName != currentRockInfo.teamName && 
                    houseRock.rock != gm.rockList[rockTarget].rock) // Not primary target
                {
                    opponentRocksInHouse.Add(houseRock.rock);
                }
            }
            
            Debug.Log($"[Peel Fallback] Found {opponentRocksInHouse.Count} secondary opponent rocks in house");
            
            bool foundSecondaryShot = false;
            
            // Try each secondary target
            foreach (GameObject secondaryRock in opponentRocksInHouse)
            {
                if (secondaryRock == null || !secondaryRock.activeInHierarchy)
                    continue;
                
                Vector2 secondaryPos = secondaryRock.transform.position;
                Debug.Log($"[Peel Fallback] Trying secondary target at ({secondaryPos.x:F2}, {secondaryPos.y:F2})");
                
                // Try direct peel on this rock
                Vector2 pullbackPos;
                bool useInTurn;
                bool foundShot = CalculatePhysicsBasedShot(secondaryPos, out pullbackPos, out useInTurn, "Peel", GetRockIndex(secondaryRock.transform));
                
                if (foundShot)
                {
                    rm.inturn = useInTurn;
                    takeOutX = pullbackPos.x;
                    takeOutY = pullbackPos.y;
                    
                    Debug.Log($"[AI_Target] ✓ SECONDARY TARGET SUCCESS!\n" +
                              $"  Secondary rock at ({secondaryPos.x:F2}, {secondaryPos.y:F2})\n" +
                              $"  Turn: {(useInTurn ? "IN-TURN" : "OUT-TURN")}\n" +
                              $"  Pullback: ({takeOutX:F3}, {takeOutY:F3})");
                    
                    foundSecondaryShot = true;
                    break; // Found a good secondary shot!
                }
            }
            
            if (!foundSecondaryShot)
            {
                // FALLBACK PHASE 2: Try direct peel on original target
                Debug.LogWarning($"[AI_Target] Secondary targets failed - trying direct peel on primary");
                
                Vector2 pullbackPos;
                bool useInTurn;
                bool foundShot = CalculatePhysicsBasedShot(targetRockPos, out pullbackPos, out useInTurn, "Peel", rockTarget);
                
                if (foundShot)
                {
                    rm.inturn = useInTurn;
                    takeOutX = pullbackPos.x;
                    takeOutY = pullbackPos.y;
                    
                    Debug.Log($"[AI_Target] Peel direct hit - InTurn: {useInTurn}, Pullback: {pullbackPos}");
                }
                else
                {
                    // FALLBACK PHASE 3: Try to remove guards blocking rocks in the house
                    Debug.LogWarning($"[AI_Target] Direct peel failed - searching for guards blocking house rocks");
                    
                    bool foundGuardTakeout = false;
                    
                    // Look for opponent guards that are blocking rocks in the house
                    foreach (var guard in gm.gList)
                    {
                        if (guard.lastTransform == null)
                            continue;
                        
                        Rock_Info guardInfo = guard.lastTransform.GetComponent<Rock_Info>();
                        if (guardInfo == null || guardInfo.teamName == currentRockInfo.teamName)
                            continue; // Skip our own guards
                        
                        Vector2 guardPos = guard.lastTransform.position;
                        
                        // Check if this guard is blocking access to ANY rock in the house
                        bool blocksHouseRock = false;
                        foreach (var houseRock in gm.houseList)
                        {
                            Vector2 houseRockPos = houseRock.rock.transform.position;
                            float lateralAlignment = Mathf.Abs(guardPos.x - houseRockPos.x);
                            bool inFront = guardPos.y < houseRockPos.y;
                            
                            if (lateralAlignment < 0.5f && inFront)
                            {
                                blocksHouseRock = true;
                                Debug.Log($"[Fallback Guard] Guard at ({guardPos.x:F2}, {guardPos.y:F2}) blocks house rock at ({houseRockPos.x:F2}, {houseRockPos.y:F2})");
                                break;
                            }
                        }
                        
                        if (blocksHouseRock)
                        {
                            // Try to take out this blocking guard
                            int guardIndex = guardInfo.rockIndex;
                            Debug.Log($"[Fallback Guard] Attempting takeout of blocking guard #{guardIndex}");
                            
                            bool foundGuardShot = CalculatePhysicsBasedShot(guardPos, out pullbackPos, out useInTurn, "Take Out", guardIndex);
                            
                            if (foundGuardShot)
                            {
                                rm.inturn = useInTurn;
                                takeOutX = pullbackPos.x;
                                takeOutY = pullbackPos.y;
                                
                                Debug.Log($"[AI_Target] ✓ FALLBACK: Takeout guard #{guardIndex} at ({guardPos.x:F2}, {guardPos.y:F2})\n" +
                                          $"  Turn: {(useInTurn ? "IN-TURN" : "OUT-TURN")}\n" +
                                          $"  Pullback: ({takeOutX:F3}, {takeOutY:F3})\n" +
                                          $"  Strategy: Clear path to house rocks");
                                
                                foundGuardTakeout = true;
                                break; // Found a guard to take out
                            }
                        }
                    }
                    
                    if (!foundGuardTakeout)
                    {
                        // FALLBACK PHASE 4: No blocking guards found - try any opponent guard
                        Debug.LogWarning($"[AI_Target] No blocking guards found - trying ANY opponent guard");
                        
                        foreach (var guard in gm.gList)
                        {
                            if (guard.lastTransform == null)
                                continue;
                            
                            Rock_Info guardInfo = guard.lastTransform.GetComponent<Rock_Info>();
                            if (guardInfo == null || guardInfo.teamName == currentRockInfo.teamName)
                                continue; // Skip our own guards
                            
                            Vector2 guardPos = guard.lastTransform.position;
                            int guardIndex = guardInfo.rockIndex;
                            
                            Debug.Log($"[Fallback Guard] Attempting takeout of any opponent guard #{guardIndex}");
                            
                            bool foundGuardShot = CalculatePhysicsBasedShot(guardPos, out pullbackPos, out useInTurn, "Take Out", guardIndex);
                            
                            if (foundGuardShot)
                            {
                                rm.inturn = useInTurn;
                                takeOutX = pullbackPos.x;
                                takeOutY = pullbackPos.y;
                                
                                Debug.Log($"[AI_Target] ✓ FALLBACK: Takeout ANY guard #{guardIndex} at ({guardPos.x:F2}, {guardPos.y:F2})\n" +
                                          $"  Turn: {(useInTurn ? "IN-TURN" : "OUT-TURN")}\n" +
                                          $"  Pullback: ({takeOutX:F3}, {takeOutY:F3})");
                                
                                foundGuardTakeout = true;
                                break;
                            }
                        }
                    }
                    
                    if (!foundGuardTakeout)
                    {
                        // FALLBACK PHASE 5: Try to hit ANY opponent rock (guards or house)
                        Debug.LogWarning($"[AI_Target] No guards available - trying ANY opponent rock");
                        
                        foreach (var rockEntry in gm.rockList)
                        {
                            if (rockEntry.rock == null || !rockEntry.rock.activeInHierarchy || !rockEntry.rockInfo.inPlay)
                                continue;
                            
                            if (rockEntry.rockInfo.teamName == currentRockInfo.teamName)
                                continue; // Skip our own rocks
                            
                            Vector2 rockPos = rockEntry.rock.transform.position;
                            int rockIndex = rockEntry.rockInfo.rockIndex;
                            
                            Debug.Log($"[Fallback Any Rock] Attempting takeout of opponent rock #{rockIndex}");
                            
                            bool foundAnyShot = CalculatePhysicsBasedShot(rockPos, out pullbackPos, out useInTurn, "Take Out", rockIndex);
                            
                            if (foundAnyShot)
                            {
                                rm.inturn = useInTurn;
                                takeOutX = pullbackPos.x;
                                takeOutY = pullbackPos.y;
                                
                                Debug.Log($"[AI_Target] ✓ FALLBACK: Takeout opponent rock #{rockIndex} at ({rockPos.x:F2}, {rockPos.y:F2})\n" +
                                          $"  Turn: {(useInTurn ? "IN-TURN" : "OUT-TURN")}\n" +
                                          $"  Pullback: ({takeOutX:F3}, {takeOutY:F3})");
                                
                                foundGuardTakeout = true;
                                break;
                            }
                        }
                    }
                    
                    if (!foundGuardTakeout)
                    {
                        // LAST RESORT: Draw to button instead of using magic numbers
                        Debug.LogWarning($"[AI_Target] ALL TAKEOUT OPTIONS FAILED - falling back to draw shot");
                        
                        Vector2 drawPullback;
                        bool drawInTurn;
                        bool foundDrawShot = CalculatePhysicsBasedDrawShot(new Vector2(0f, 6.5f), out drawPullback, out drawInTurn);
                        
                        if (foundDrawShot)
                        {
                            rm.inturn = drawInTurn;
                            takeOutX = drawPullback.x;
                            takeOutY = drawPullback.y;
                            
                            Debug.Log($"[AI_Target] ✓ FINAL FALLBACK: Draw to button\n" +
                                      $"  Turn: {(drawInTurn ? "IN-TURN" : "OUT-TURN")}\n" +
                                      $"  Pullback: ({takeOutX:F3}, {takeOutY:F3})");
                            
                            aiShoot.OnShot("Draw To Target", rockCurrent);
                            Debug.Log("Peel FAILED - Drawing to button instead");
                            yield break;
                        }
                        else
                        {
                            // ABSOLUTE LAST RESORT: Throw away the rock (total failure)
                            Debug.LogError($"[AI_Target] CATASTROPHIC: Even draw shot failed - throwing away rock");
                            
                            rm.inturn = (targetRockPos.x < 0f);
                            takeOutX = (targetRockPos.x < 0f) ? -1.5f : 1.5f;
                            takeOutY = -27.0f;
                            
                            Debug.LogError($"[AI_Target] Throwing away - Turn: {(rm.inturn ? "IN-TURN" : "OUT-TURN")}, Pullback: ({takeOutX:F3}, {takeOutY:F3})");
                        }
                    }
                }
            }
        }

        aiShoot.OnShot("Peel", rockCurrent);
        Debug.Log(gm.rockList[rockTarget].rockInfo.teamName + " " + gm.rockList[rockTarget].rockInfo.rockNumber);
        yield break;
    }
    
    /// <summary>
    /// Score a peel shot based on CHAOS factor - how much disruption does it cause?
    /// High score = removes multiple rocks, especially opponents
    /// </summary>
    private float ScorePeelChaos(
        Rock_Info currentRockInfo,
        Vector2 primaryTargetPos,
        List<GameObject> secondaryTargets,
        Vector2 pullbackPos,
        bool useInTurn)
    {
        // Simulate the peel shot to see what happens
        Vector2 launcherPos = new Vector2(0f, -25f);
        
        // Calculate velocity from pullback (peel uses heavy weight)
        TrajectoryLine playerTrajectory = FindObjectOfType<TrajectoryLine>();
        float velocityMultiplier = playerTrajectory != null ? playerTrajectory.velocityMultiplier : 5.0f;
        float peelPullbackDistance = 4.4f; // Heavy peel weight
        float velocityMagnitude = peelPullbackDistance * velocityMultiplier;
        
        Vector2 direction = (primaryTargetPos - launcherPos).normalized;
        Vector2 velocity = direction * velocityMagnitude;
        
        // Simulate WITH all rocks in play to detect secondary collisions
        List<GameObject> allRocks = new List<GameObject>();
        for (int i = 0; i < gm.rockList.Count; i++)
        {
            var rockEntry = gm.rockList[i];
            if (rockEntry.rock != null && rockEntry.rock.activeInHierarchy && rockEntry.rockInfo.inPlay)
            {
                allRocks.Add(rockEntry.rock);
            }
        }
        
        List<Vector2> path = trajectorySimulator.SimulateTrajectory(
            launcherPos,
            velocity,
            useInTurn,
            250,
            allRocks,
            forPlayerPreview: false
        );
        
        if (path.Count == 0)
            return 0f;
        
        TrajectorySimulator.CollisionInfo collision = trajectorySimulator.GetCollisionInfo();
        
        // SCORING SYSTEM: Chaos = disruption
        float chaosScore = 0f;
        
        // 1. Did we hit the PRIMARY target? (REQUIRED - baseline 30 points)
        if (!collision.hasCollision)
        {
            Debug.Log($"[Peel Chaos] MISS - no collision detected");
            return 0f; // Complete miss
        }
        
        // Check if we hit the right rock
        bool hitPrimaryTarget = Vector2.Distance(collision.collisionPoint, primaryTargetPos) < 0.5f;
        if (!hitPrimaryTarget)
        {
            Debug.Log($"[Peel Chaos] HIT WRONG ROCK - collision at {collision.collisionPoint}, target at {primaryTargetPos}");
            return 5f; // Small consolation prize for hitting something
        }
        
        chaosScore += 30f; // Baseline: hit the primary target
        
        // 2. Where does PRIMARY TARGET end up? (OUT OF PLAY = GOOD!)
        float primaryTargetFinalY = collision.hitRockFinalPosition.y;
        
        if (primaryTargetFinalY > 10f || primaryTargetFinalY < -5f)
        {
            chaosScore += 40f; // PRIMARY REMOVED! (best outcome)
            Debug.Log($"[Peel Chaos] ✓ PRIMARY REMOVED (Y={primaryTargetFinalY:F2})");
        }
        else if (primaryTargetFinalY < 5f)
        {
            chaosScore += 20f; // Pushed out of house (good)
            Debug.Log($"[Peel Chaos] ✓ PRIMARY PUSHED OUT (Y={primaryTargetFinalY:F2})");
        }
        else
        {
            chaosScore += 5f; // Still in house (not ideal)
            Debug.Log($"[Peel Chaos] ⚠️ PRIMARY STILL IN HOUSE (Y={primaryTargetFinalY:F2})");
        }
        
        // 3. Check SECONDARY COLLISIONS along the hit rock's path
        // The hit rock might collide with OTHER rocks on its way out!
        List<Vector2> hitRockPath = collision.hitRockPostCollisionPath;
        
        if (hitRockPath != null && hitRockPath.Count > 0)
        {
            int secondaryHits = 0;
            int opponentSecondaryHits = 0;
            
            // Check distance from hit rock's path to each secondary target
            foreach (GameObject secondaryRock in secondaryTargets)
            {
                if (secondaryRock == null || !secondaryRock.activeInHierarchy)
                    continue;
                
                Vector2 secondaryPos = secondaryRock.transform.position;
                Rock_Info secondaryInfo = secondaryRock.GetComponent<Rock_Info>();
                
                // Check if hit rock's path gets close to this rock
                float closestDist = float.MaxValue;
                foreach (Vector2 pathPoint in hitRockPath)
                {
                    float dist = Vector2.Distance(pathPoint, secondaryPos);
                    if (dist < closestDist)
                        closestDist = dist;
                }
                
                // If within collision range (2 rock radii = ~0.28), count it
                if (closestDist < 0.35f) // Generous threshold for detection
                {
                    secondaryHits++;
                    
                    bool isOpponent = secondaryInfo != null && secondaryInfo.teamName != currentRockInfo.teamName;
                    if (isOpponent)
                    {
                        opponentSecondaryHits++;
                        chaosScore += 15f; // BONUS: Hit opponent's rock!
                        Debug.Log($"[Peel Chaos] ✓ SECONDARY HIT: {secondaryRock.name} (opponent) at dist {closestDist:F3}");
                    }
                    else
                    {
                        chaosScore += 5f; // Hit our own rock (still disruption)
                        Debug.Log($"[Peel Chaos] ⚠️ SECONDARY HIT: {secondaryRock.name} (friendly) at dist {closestDist:F3}");
                    }
                }
            }
            
            // BONUS for multiple secondary hits (CHAOS MULTIPLIER!)
            if (secondaryHits >= 2)
            {
                chaosScore += 20f; // Multi-rock chaos!
                Debug.Log($"[Peel Chaos] 🎯 MULTI-ROCK CHAOS! {secondaryHits} secondary hits");
            }
        }
        
        // 4. Where does SHOOTER end up? (Ideally OUT OF PLAY for clean removal)
        float shooterFinalY = collision.finalPosition.y;
        
        if (shooterFinalY > 10f || shooterFinalY < -5f)
        {
            chaosScore += 10f; // Shooter removed too (clean peel)
            Debug.Log($"[Peel Chaos] ✓ SHOOTER REMOVED (Y={shooterFinalY:F2})");
        }
        else if (shooterFinalY < 5f)
        {
            chaosScore += 5f; // Shooter out of house (acceptable)
            Debug.Log($"[Peel Chaos] ✓ SHOOTER OUT OF HOUSE (Y={shooterFinalY:F2})");
        }
        else
        {
            chaosScore -= 5f; // Shooter stays in house (not ideal for peel)
            Debug.Log($"[Peel Chaos] ⚠️ SHOOTER IN HOUSE (Y={shooterFinalY:F2})");
        }
        
        return chaosScore;
    }

    IEnumerator TapManualTarget(int rockCurrent)
    {
        targetX = targetPos.x;
        targetY = targetPos.y;

        if (rm.inturn == false)
        {
            takeOutX = (-0.178f * ((targetX + 1.35f) / 2.7f)) + 0.056f;
        }
        else
        {
            takeOutX = (-0.18f * ((targetX + 1.35f) / 2.7f)) + 0.12f;
        }

        aiShoot.OnShot("Raise", rockCurrent);
        yield break;
    }

    IEnumerator TapTarget(int rockCurrent, int rockTarget)
    {
        yield return StartCoroutine(GuardReading(rockCurrent));

        // PHYSICS-BASED TAP WITH OPTIMAL ANGLE CALCULATION
        // Strategy: Calculate the best angle to deflect target rock toward button
        Vector2 targetRockPos = gm.rockList[rockTarget].rock.transform.position;
        Rock_Info currentRockInfo = gm.rockList[rockCurrent].rockInfo;
        Vector2 button = new Vector2(0f, 6.5f);
        Vector2 launcherPos = new Vector2(0f, -25f);
        
        Debug.Log($"[Tap Angle] Calculating optimal tap angle for rock at ({targetRockPos.x:F2}, {targetRockPos.y:F2})");
        
        // STEP 1: Determine IDEAL FINAL POSITION for target (closer to button)
        Vector2 targetToButton = button - targetRockPos;
        float distToButton = targetToButton.magnitude;
        
        // Ideal: Push target 60-80% of the way to button (don't overshoot)
        float pushRatio = Mathf.Clamp01(0.7f - (distToButton / 3.0f)); // Closer rocks = lighter push
        Vector2 idealFinalPos = targetRockPos + targetToButton * pushRatio;
        
        Debug.Log($"[Tap Angle] Target dist to button: {distToButton:F2}, Push ratio: {pushRatio:F2}, Ideal final: ({idealFinalPos.x:F2}, {idealFinalPos.y:F2})");
        
        // STEP 2: Calculate DEFLECTION ANGLE needed
        // This is the direction target should move after being hit
        Vector2 desiredDeflection = (idealFinalPos - targetRockPos).normalized;
        float deflectionAngle = Mathf.Atan2(desiredDeflection.y, desiredDeflection.x) * Mathf.Rad2Deg;
        
        Debug.Log($"[Tap Angle] Desired deflection direction: {desiredDeflection}, Angle: {deflectionAngle:F1}°");
        
        // STEP 3: Calculate APPROACH ANGLE for shooter
        // Physics: For elastic collision, approach angle determines deflection
        // For a tap (light hit), target deflects ~60-70° from shooter's approach direction
        // So approach_angle + 70° ≈ desired_deflection_angle
        
        float tapDeflectionOffset = 70f; // Typical deflection angle for light glancing hit
        float requiredApproachAngle = deflectionAngle - tapDeflectionOffset;
        
        // Convert back to direction vector
        float approachRad = requiredApproachAngle * Mathf.Deg2Rad;
        Vector2 approachDirection = new Vector2(Mathf.Cos(approachRad), Mathf.Sin(approachRad));
        
        Debug.Log($"[Tap Angle] Required approach angle: {requiredApproachAngle:F1}°, Direction: {approachDirection}");
        
        // STEP 4: Calculate AIM POINT (where shooter needs to hit on target rock's circumference)
        // For glancing tap, aim at edge of rock, not center
        float rockRadius = 0.14f;
        Vector2 aimPointOffset = -approachDirection * rockRadius * 0.8f; // Aim at 80% of radius (slight glance)
        Vector2 aimPoint = targetRockPos + aimPointOffset;
        
        Debug.Log($"[Tap Angle] Aim point: ({aimPoint.x:F2}, {aimPoint.y:F2}) - offset by {aimPointOffset}");
        
        // STEP 5: Calculate VELOCITY (light tap = 2.8 pullback = ~7.7 m/s)
        TrajectoryLine playerTrajectory = FindObjectOfType<TrajectoryLine>();
        float velocityMultiplier = playerTrajectory != null ? playerTrajectory.velocityMultiplier : 5.0f;
        float tapPullback = 2.8f;
        float tapVelocity = tapPullback * velocityMultiplier;
        
        // Direction from launcher to aim point
        Vector2 launcherToAim = (aimPoint - launcherPos).normalized;
        Vector2 desiredVelocity = launcherToAim * tapVelocity;
        
        Debug.Log($"[Tap Angle] Velocity: {tapVelocity:F2} m/s, Direction: {launcherToAim}");
        
        // STEP 6: Determine TURN DIRECTION (in-turn vs out-turn)
        // Choose turn that naturally curls TOWARD the aim point
        float aimPointX = aimPoint.x;
        
        // IN-TURN curls LEFT (negative X), OUT-TURN curls RIGHT (positive X)
        // If aim point is LEFT of launcher (negative X), use IN-TURN
        // If aim point is RIGHT of launcher (positive X), use OUT-TURN
        bool useInTurn = (aimPointX < launcherPos.x);
        
        Debug.Log($"[Tap Angle] Aim point X={aimPointX:F2}, Launcher X={launcherPos.x:F2} → Turn: {(useInTurn ? "IN-TURN (curl left)" : "OUT-TURN (curl right)")}");
        
        // STEP 7: Try physics simulation with calculated angle
        Vector2 pullbackPos;
        bool foundShot = CalculatePhysicsBasedShot(aimPoint, out pullbackPos, out useInTurn, "Tap Back", rockTarget);
        
        if (foundShot)
        {
            rm.inturn = useInTurn;
            takeOutX = pullbackPos.x;
            takeOutY = pullbackPos.y;
            
            Debug.Log($"[Tap Angle] ✓ SUCCESS! Optimal angle tap shot\n" +
                      $"  Approach angle: {requiredApproachAngle:F1}°\n" +
                      $"  Expected deflection: {deflectionAngle:F1}°\n" +
                      $"  Target will move toward: ({idealFinalPos.x:F2}, {idealFinalPos.y:F2})\n" +
                      $"  Pullback: ({takeOutX:F3}, {takeOutY:F3})\n" +
                      $"  Turn: {(useInTurn ? "IN-TURN" : "OUT-TURN")}");
        }
        else
        {
            // FALLBACK 1: Try direct tap (nose hit) as simpler alternative
            Debug.LogWarning($"[Tap Angle] Optimal angle failed, trying direct tap");
            
            foundShot = CalculatePhysicsBasedShot(targetRockPos, out pullbackPos, out useInTurn, "Tap Back", rockTarget);
            
            if (foundShot)
            {
                rm.inturn = useInTurn;
                takeOutX = pullbackPos.x;
                takeOutY = pullbackPos.y;
                
                Debug.Log($"[Tap Angle] ✓ Direct tap SUCCESS - InTurn: {useInTurn}, Pullback: {pullbackPos}");
            }
            else
            {
                // FALLBACK 2: Draw beside target
                Debug.LogWarning($"[Tap Angle] Direct tap failed - trying DRAW BESIDE as alternative");
                
                Vector2 drawBesideTarget = new Vector2(
                    targetRockPos.x + (targetRockPos.x > 0 ? -0.3f : 0.3f),
                    targetRockPos.y - 0.2f
                );
                
                drawBesideTarget.x = Mathf.Clamp(drawBesideTarget.x, -1.5f, 1.5f);
                drawBesideTarget.y = Mathf.Clamp(drawBesideTarget.y, 5.5f, 8.5f);
                
                bool foundDrawShot = CalculatePhysicsBasedDrawShot(drawBesideTarget, out pullbackPos, out useInTurn);
                
                if (foundDrawShot)
                {
                    rm.inturn = useInTurn;
                    takeOutX = pullbackPos.x;
                    takeOutY = pullbackPos.y;
                    
                    Debug.Log($"[Tap Angle] ✓ DRAW BESIDE SUCCESS - InTurn: {useInTurn}, Pullback: {pullbackPos}");
                    
                    aiShoot.OnShot("Draw To Target", rockCurrent);
                    Debug.Log($"TapBack → Draw Beside - Near {gm.rockList[rockTarget].rockInfo.teamName} #{gm.rockList[rockTarget].rockInfo.rockNumber}");
                    yield break;
                }
                
                
                
                // LAST RESORT: Draw to button instead of magic numbers
                Debug.LogWarning($"[Tap Angle] ALL PHYSICS FAILED - drawing to button as emergency fallback");
                
                Vector2 drawPullback;
                bool drawInTurn;
                bool foundDrawFallback = CalculatePhysicsBasedDrawShot(new Vector2(0f, 6.5f), out drawPullback, out drawInTurn);
                
                if (foundDrawFallback)
                {
                    rm.inturn = drawInTurn;
                    takeOutX = drawPullback.x;
                    takeOutY = drawPullback.y;
                    
                    Debug.Log($"[Tap Angle] Tap → Draw fallback SUCCESS");
                }
                else
                {
                    // CATASTROPHIC: Throw away the rock
                    Debug.LogError($"[Tap Angle] CATASTROPHIC FAILURE - throwing rock away");
                    
                    rm.inturn = Random.value > 0.5f;
                    takeOutX = Random.Range(-1.5f, 1.5f);
                    takeOutY = -27.0f;
                }
            }
        }
        
        aiShoot.OnShot("Raise", rockCurrent);
        Debug.Log("TapBack - " + gm.rockList[rockTarget].rockInfo.teamName + " " + gm.rockList[rockTarget].rockInfo.rockNumber);
        yield break;
    }

    IEnumerator TickShotManualTarget(int rockCurrent)
    {
        targetX = targetPos.x;
        targetY = targetPos.y;

        if (rm.inturn == false)
        {
            takeOutX = (-0.04f * ((targetX + 0.4f) / 0.8f)) - 0.005f;
        }
        else
        {
            takeOutX = (-0.039f * ((targetX + 0.4f) / 0.8f)) + 0.042f;
        }

        aiShoot.OnShot("Tick", rockCurrent);
        yield break;
    }

    IEnumerator TickShotTarget(int rockCurrent, int rockTarget)
    {
        yield return StartCoroutine(GuardReading(rockCurrent));

        // PHYSICS-BASED: Tick shot hits rock at an angle, keeping both in play
        // Very light contact - just nudge the rock
        Vector2 targetRockPos = gm.rockList[rockTarget].rock.transform.position;
        
        Vector2 pullbackPos;
        bool useInTurn;
        bool foundShot = CalculatePhysicsBasedShot(targetRockPos, out pullbackPos, out useInTurn, "Tick", rockTarget);
        
        if (foundShot)
        {
            // CRITICAL: Set rm.inturn from physics calculation ONCE
            rm.inturn = useInTurn;
            takeOutX = pullbackPos.x;
            takeOutY = pullbackPos.y;
            
            Debug.Log($"[AI_Target] Tick SUCCESS - InTurn: {useInTurn}, Target: {targetRockPos}, Pullback: {pullbackPos}");
        }
        else
        {
            // FALLBACK: Try direct takeout if tick fails
            Debug.LogWarning($"[AI_Target] Tick physics FAILED - trying direct takeout on target: {targetRockPos}");
            
            Vector2 takeoutPullback;
            bool takeoutInTurn;
            bool foundTakeout = CalculatePhysicsBasedShot(targetRockPos, out takeoutPullback, out takeoutInTurn, "Take Out", rockTarget);
            
            if (foundTakeout)
            {
                rm.inturn = takeoutInTurn;
                takeOutX = takeoutPullback.x;
                takeOutY = takeoutPullback.y;
                
                Debug.Log($"[AI_Target] Tick → Takeout fallback SUCCESS - InTurn: {takeoutInTurn}, Pullback: {takeoutPullback}");
            }
            else
            {
                // LAST RESORT: Just throw it away (can't hit the target)
                Debug.LogError($"[AI_Target] Tick AND Takeout BOTH FAILED - throwing away rock");
                
                // Throw to corner out of bounds
                rm.inturn = (targetRockPos.x < 0f);
                takeOutX = (targetRockPos.x < 0f) ? -1.5f : 1.5f;
                takeOutY = -27.0f;
            }
        }

        aiShoot.OnShot("Tick", rockCurrent);
        Debug.Log("Tick - " + gm.rockList[rockTarget].rockInfo.teamName + " " + gm.rockList[rockTarget].rockInfo.rockNumber);
        yield break;
    }
    
    /// <summary>
    /// RUNBACK: Hit an obstructing guard rock through to remove the target behind it
    /// This is an advanced double-takeout shot requiring extra velocity
    /// </summary>
    IEnumerator RunbackTarget(int rockCurrent, int rockTarget)
    {
        yield return StartCoroutine(GuardReading(rockCurrent));

        // PHYSICS-BASED: Runback requires hitting the guard with enough velocity
        // to drive through and remove the target rock behind it
        Vector2 guardRockPos = gm.rockList[rockTarget].rock.transform.position;
        
        Debug.Log($"[AI_Target] RUNBACK SHOT - Hitting guard at {guardRockPos} to remove target behind it");
        
        Vector2 pullbackPos;
        bool useInTurn;
        bool foundShot = CalculatePhysicsBasedShot(guardRockPos, out pullbackPos, out useInTurn, "Runback", rockTarget);
        
        if (foundShot)
        {
            // CRITICAL: Set rm.inturn from physics calculation ONCE
            rm.inturn = useInTurn;
            takeOutX = pullbackPos.x;
            takeOutY = pullbackPos.y;
            
            Debug.Log($"[AI_Target] Runback SUCCESS - InTurn: {useInTurn}, Guard: {guardRockPos}, Pullback: {pullbackPos}, Extra velocity for drive-through!");
        }
        else
        {
            // FALLBACK: Try peel on guard if runback fails
            Debug.LogWarning($"[AI_Target] Runback physics FAILED - trying peel on guard: {guardRockPos}");
            
            Vector2 peelPullback;
            bool peelInTurn;
            bool foundPeel = CalculatePhysicsBasedShot(guardRockPos, out peelPullback, out peelInTurn, "Peel", rockTarget);
            
            if (foundPeel)
            {
                rm.inturn = peelInTurn;
                takeOutX = peelPullback.x;
                takeOutY = peelPullback.y;
                
                Debug.Log($"[AI_Target] Runback → Peel fallback SUCCESS - InTurn: {peelInTurn}, Pullback: {peelPullback}");
            }
            else
            {
                // LAST RESORT: Try regular takeout on guard
                Debug.LogWarning($"[AI_Target] Runback AND Peel FAILED - trying regular takeout on guard");
                
                Vector2 takeoutPullback;
                bool takeoutInTurn;
                bool foundTakeout = CalculatePhysicsBasedShot(guardRockPos, out takeoutPullback, out takeoutInTurn, "Take Out", rockTarget);
                
                if (foundTakeout)
                {
                    rm.inturn = takeoutInTurn;
                    takeOutX = takeoutPullback.x;
                    takeOutY = takeoutPullback.y;
                    
                    Debug.Log($"[AI_Target] Runback → Takeout fallback SUCCESS");
                }
                else
                {
                    // ABSOLUTE FAILURE: Draw to button
                    Debug.LogError($"[AI_Target] ALL runback fallbacks FAILED - drawing to button");
                    OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                    yield break;
                }
            }
            
            Debug.LogWarning($"[AI_Target] Fallback - Using existing turn state: {(rm.inturn ? "IN-TURN" : "OUT-TURN")}");
        }

        aiShoot.OnShot("Peel", rockCurrent); // Use Peel shot type for extra velocity
        Debug.Log("Runback - Hitting " + gm.rockList[rockTarget].rockInfo.teamName + " guard #" + gm.rockList[rockTarget].rockInfo.rockNumber);
        yield break;
    }

    IEnumerator DrawTarget(int rockCurrent, Vector2 targetPosition)
    {
        // PHYSICS-BASED: Draw to a specific target position in the house
        // Examines guards to find best path
        if (targetPosition == null)
            targetPosition = new Vector2 (0f, 6.5f);
        
        Vector2 pullbackPos;
        bool useInTurn;
        
        // Try to find a clear path to the target, avoiding guards
        bool foundShot = CalculatePhysicsBasedDrawShot(targetPosition, out pullbackPos, out useInTurn);
        
        if (foundShot)
        {
            // Apply accuracy modifier
            CharacterStats shooterStats = GetShooterStats(rockCurrent);
            if (shooterStats != null)
            {
                float accuracy = shooterStats.drawAccuracy.GetValue() / 100f;
                float maxError = 0.2f * (1f - accuracy); // Draw shots have more tolerance
                Vector2 errorOffset = Random.insideUnitCircle * maxError;
                pullbackPos += errorOffset;
            }
            
            rm.inturn = useInTurn;
            takeOutX = pullbackPos.x;
            takeOutY = pullbackPos.y;
            
            Debug.Log($"[Physics Draw] Target: {targetPosition}, Pullback: {pullbackPos}, InTurn: {useInTurn}");
        }
        else
        {
            // FALLBACK: Draw shot failed - try guard shot instead (lighter weight might work)
            Debug.LogWarning("[Physics Draw] Failed, trying guard shot as fallback");
            
            Vector2 guardPullback;
            bool guardInTurn;
            bool foundGuard = CalculatePhysicsBasedGuardShot(targetPosition, out guardPullback, out guardInTurn);
            
            if (foundGuard)
            {
                rm.inturn = guardInTurn;
                takeOutX = guardPullback.x;
                takeOutY = guardPullback.y;
                
                Debug.Log($"[Physics Draw] Draw → Guard fallback SUCCESS - InTurn: {guardInTurn}, Pullback: {guardPullback}");
            }
            else
            {
                // LAST RESORT: Just place a guard in the open (can't reach target)
                Debug.LogError($"[Physics Draw] Draw AND Guard BOTH FAILED - placing emergency center guard");
                
                // Emergency guard placement - center, medium depth
                rm.inturn = Random.value > 0.5f; // Random turn
                
                Vector2 emergencyGuardTarget = new Vector2(
                    Random.Range(-0.15f, 0.15f), // Center with slight variance
                    Random.Range(3.0f, 3.5f)      // Standard guard depth
                );
                
                Vector2 emergencyPullback;
                bool emergencyInTurn;
                bool foundEmergency = CalculatePhysicsBasedGuardShot(emergencyGuardTarget, out emergencyPullback, out emergencyInTurn);
                
                if (foundEmergency)
                {
                    rm.inturn = emergencyInTurn;
                    takeOutX = emergencyPullback.x;
                    takeOutY = emergencyPullback.y;
                    
                    Debug.Log($"[Physics Draw] Emergency guard placement - InTurn: {emergencyInTurn}");
                }
                else
                {
                    // CATASTROPHIC: Can't even place a guard - throw it away
                    Debug.LogError($"[Physics Draw] CATASTROPHIC FAILURE - throwing rock away");
                    
                    rm.inturn = Random.value > 0.5f;
                    takeOutX = Random.Range(-1.5f, 1.5f);
                    takeOutY = -27.0f; // Very light (will sail past)
                }
            }
        }

        aiShoot.OnShot("Draw To Target", rockCurrent);
        yield break;
    }
    
    /// <summary>
    /// Physics-based draw shot calculation - RADIAL SWEEP around guards to find PROTECTED SCORING positions
    /// STRATEGY: Protected + Scoring > Clean path + Far away
    /// Prioritizes: 1) Behind guards, 2) Closer to button than opponents, 3) Minor bumps near target OK
    /// </summary>
    private bool CalculatePhysicsBasedDrawShot(Vector2 targetPosition, out Vector2 pullbackPosition, out bool useInTurn)
    {
        Vector2 launcherPos = new Vector2(0f, -25f);
        Vector2 button = new Vector2(0f, 6.5f);
        Rock_Info currentRockInfo = gm.rockList[gm.rockCurrent].rockInfo;
        
        Debug.Log($"[Physics Draw] RADIAL SWEEP for protected scoring - Target: ({targetPosition.x:F2}, {targetPosition.y:F2})");
        
        // Get rocks in play (guards and house rocks)
        List<GameObject> rocksInPlay = new List<GameObject>();
        List<GameObject> guards = new List<GameObject>();
        List<GameObject> opponentRocks = new List<GameObject>();
        float closestOpponentDistToButton = 999f;
        
        foreach (var rockEntry in gm.rockList)
        {
            if (rockEntry.rock != null && rockEntry.rock.activeInHierarchy && rockEntry.rockInfo.inPlay)
            {
                rocksInPlay.Add(rockEntry.rock);
                
                Vector2 rockPos = rockEntry.rock.transform.position;
                
                // Identify guards (Y < 5.0, in front of house)
                if (rockPos.y > 0f && rockPos.y < 5.0f)
                {
                    guards.Add(rockEntry.rock);
                }
                
                // Track opponent rocks and find closest to button
                if (rockEntry.rockInfo.teamName != currentRockInfo.teamName && rockPos.y > 5.0f)
                {
                    opponentRocks.Add(rockEntry.rock);
                    float distToButton = Vector2.Distance(rockPos, button);
                    if (distToButton < closestOpponentDistToButton)
                    {
                        closestOpponentDistToButton = distToButton;
                    }
                }
            }
        }
        
        Debug.Log($"[Physics Draw] Obstacles: {rocksInPlay.Count} total, {guards.Count} guards, {opponentRocks.Count} opponent rocks");
        Debug.Log($"[Physics Draw] Closest opponent dist to button: {closestOpponentDistToButton:F2}");
        
        float bestScore = float.MinValue;
        Vector2 bestPullback = Vector2.zero;
        bool bestInTurn = false;
        Vector2 bestFinalPos = Vector2.zero;
        
        // COMPREHENSIVE RADIAL SWEEP: Test positions around TARGET (not guards!)
        // Goal: Get as CLOSE AS POSSIBLE to target position
        // Strategy: Try multiple radii and angles to find cleanest path
        List<Vector2> candidateTargets = new List<Vector2>();
        
        // CANDIDATE 1: Direct to target (baseline - MUST try this!)
        candidateTargets.Add(targetPosition);
        
        // CANDIDATE 2-N: TIGHTER RADIAL SWEEP around TARGET
        // PRECISION TARGETING: Radii kept VERY tight (max 0.4m from target)
        // Philosophy: We want to hit EXACTLY where we aim, not "close enough"
        float[] radii = new float[] { 0.10f, 0.20f, 0.30f, 0.40f }; // Much tighter - within 40cm max
        float[] angles = new float[] { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f }; // Every 45° = 8 positions per radius (fewer tests)
        
        foreach (float radius in radii)
        {
            foreach (float angleDeg in angles)
            {
                float angleRad = angleDeg * Mathf.Deg2Rad;
                Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;
                Vector2 candidatePos = targetPosition + offset; // Offset from TARGET, not button!
                
                // Must be in playable area (within sheet bounds and house)
                if (candidatePos.y > 5.0f && candidatePos.y < 9.0f && Mathf.Abs(candidatePos.x) < 2.0f)
                {
                    candidateTargets.Add(candidatePos);
                }
            }
        }
        
        Debug.Log($"[Physics Draw] Generated {candidateTargets.Count} candidate positions (1 direct + {candidateTargets.Count - 1} radial within 0.4m of target - TIGHT PRECISION)");
        
        // Get velocity multiplier for draw weight calculation
        TrajectoryLine playerTrajectory = FindObjectOfType<TrajectoryLine>();
        float velocityMultiplier = playerTrajectory != null ? playerTrajectory.velocityMultiplier : 5.0f;
        
        // TEST EACH CANDIDATE POSITION
        for (int turnDir = 0; turnDir < 2; turnDir++)
        {
            bool tryInTurn = (turnDir == 0);
            
            Debug.Log($"[Physics Draw] --- Testing {(tryInTurn ? "IN-TURN (curls RIGHT →)" : "OUT-TURN (curls LEFT ←)")} ---");
            
            // STEP 1: Measure curl for this turn direction at draw weight
            // Simulate straight shot to target to see how much curl we get
            float targetDistFromLauncher = Vector2.Distance(launcherPos, targetPosition);
            float desiredPullbackDistance;
            
            if (targetDistFromLauncher < 28f) desiredPullbackDistance = 3.0f;
            else if (targetDistFromLauncher < 32f) desiredPullbackDistance = 3.2f;
            else desiredPullbackDistance = 3.4f;
            
            float drawVelocity = desiredPullbackDistance * velocityMultiplier;
            
            Vector2 straightDirection = (targetPosition - launcherPos).normalized;
            Vector2 straightVelocity = straightDirection * drawVelocity;
            
            List<Vector2> curlTestPath = trajectorySimulator.SimulateTrajectory(
                launcherPos, straightVelocity, tryInTurn, 250, rocksInPlay, forPlayerPreview: false);
            
            float curlOffset = 0f;
            if (curlTestPath.Count > 0)
            {
                Vector2 straightFinal = curlTestPath[curlTestPath.Count - 1];
                curlOffset = straightFinal.x - targetPosition.x; // Measured curl amount
                
                Debug.Log($"[Curl Measurement] 🎯 DETAILED CURL ANALYSIS:\n" +
                          $"  Turn: {(tryInTurn ? "IN-TURN (curls RIGHT →)" : "OUT-TURN (curls LEFT ←)")}\n" +
                          $"  Aim: Straight at target X={targetPosition.x:F3}\n" +
                          $"  Result: Landed at X={straightFinal.x:F3}\n" +
                          $"  Measured Curl: {curlOffset:F3} ({(curlOffset > 0 ? "RIGHT →" : "LEFT ←")})\n" +
                          $"  Compensation: Aim at X={(targetPosition.x - curlOffset):F3} (offset {-curlOffset:F3})\n" +
                          $"  Logic: If shot curls {curlOffset:F3}, aim OPPOSITE {-curlOffset:F3} to hit target");
            }
            
            Debug.Log($"[Physics Draw] Curl compensation for {(tryInTurn ? "IN-TURN" : "OUT-TURN")}: {curlOffset:F3} (will aim {-curlOffset:F3} to compensate)");
            
            foreach (Vector2 candidateTarget in candidateTargets)
            {
                // Apply curl compensation to this candidate
                Vector2 compensatedTarget = new Vector2(candidateTarget.x - curlOffset, candidateTarget.y);
                
                Debug.Log($"[Curl Compensation] 🎯 APPLYING TO CANDIDATE:\n" +
                          $"  Original target: ({candidateTarget.x:F3}, {candidateTarget.y:F3})\n" +
                          $"  Curl offset: {curlOffset:F3}\n" +
                          $"  Compensated aim: ({compensatedTarget.x:F3}, {compensatedTarget.y:F3})\n" +
                          $"  Expected: Curl will bring it back to ({candidateTarget.x:F3}, {candidateTarget.y:F3})");
                
                // Calculate velocity toward COMPENSATED target
                Vector2 direction = (compensatedTarget - launcherPos).normalized;
                Vector2 requiredVelocity = direction * drawVelocity;
                
                if (requiredVelocity.magnitude < 3f || requiredVelocity.magnitude > 20f)
                    continue;
                
                Vector2 testPullback = CalculatePullbackFromVelocity(requiredVelocity, launcherPos, tryInTurn);
                
                // Simulate trajectory
                List<Vector2> simulatedPath = trajectorySimulator.SimulateTrajectory(
                    launcherPos,
                    requiredVelocity,
                    tryInTurn,
                    250,
                    rocksInPlay,
                    forPlayerPreview: false
                );
                
                if (simulatedPath.Count == 0) continue;
                
                Vector2 finalPos = simulatedPath[simulatedPath.Count - 1];
                TrajectorySimulator.CollisionInfo collisionInfo = trajectorySimulator.GetCollisionInfo();
                
                // VERIFICATION: Did compensation work?
                float actualError = finalPos.x - candidateTarget.x;
                float expectedError = 0f; // Should be minimal if compensation worked
                float compensationEffectiveness = 1f - Mathf.Abs(actualError / Mathf.Max(0.01f, Mathf.Abs(curlOffset)));
                
                Debug.Log($"[Curl Verification] 🎯 COMPENSATION RESULT:\n" +
                          $"  Aimed at: ({compensatedTarget.x:F3}, {compensatedTarget.y:F3})\n" +
                          $"  Landed at: ({finalPos.x:F3}, {finalPos.y:F3})\n" +
                          $"  Target was: ({candidateTarget.x:F3}, {candidateTarget.y:F3})\n" +
                          $"  Error from target: {actualError:F3}\n" +
                          $"  Original curl: {curlOffset:F3}\n" +
                          $"  Compensation effectiveness: {(compensationEffectiveness * 100f):F1}%\n" +
                          $"  {(Mathf.Abs(actualError) < 0.1f ? "✅ GOOD" : Mathf.Abs(actualError) < 0.3f ? "⚠️ ACCEPTABLE" : "❌ POOR")}");
                
                // ========================================
                // SOPHISTICATED SCORING SYSTEM
                // ========================================
                
                float score = 0f;
                
                // PART 1: PROXIMITY TO TARGET (60 points max) - HIGHEST PRIORITY!
                // Goal: Get as CLOSE AS POSSIBLE to the requested target position
                // TIGHTER SCORING: More demanding accuracy requirements
                float distToTarget = Vector2.Distance(finalPos, candidateTarget);
                float proximityScore = 0f;
                
                if (distToTarget < 0.08f)
                {
                    proximityScore = 60f; // EXCELLENT (<8cm) = pinpoint accuracy!
                }
                else if (distToTarget < 0.15f)
                {
                    proximityScore = 55f; // Very close (<15cm) = excellent
                }
                else if (distToTarget < 0.25f)
                {
                    proximityScore = 48f; // Close (<25cm) = very good
                }
                else if (distToTarget < 0.40f)
                {
                    proximityScore = 38f; // Acceptable (<40cm) = good
                }
                else if (distToTarget < 0.60f)
                {
                    proximityScore = 25f; // Moderate (<60cm) = okay
                }
                else if (distToTarget < 0.80f)
                {
                    proximityScore = 15f; // Far (<80cm) = poor
                }
                else
                {
                    proximityScore = 5f * Mathf.Max(0f, 1f - (distToTarget / 1.5f)); // Very far = worse
                }
                
                score += proximityScore;
                
                // PART 2: GUARD PROTECTION (15 points max) - Small bonus for ANY guard
                // Philosophy: ALL guards (friendly OR opponent) provide some protection
                // Being under a guard makes you harder to remove, regardless of who placed it
                float protectionScore = 0f;
                GameObject protectingGuard = null;
                
                foreach (GameObject guard in guards)
                {
                    Vector2 guardPos = guard.transform.position;
                    
                    // Check if guard is protecting this position
                    // Protection = guard is BETWEEN launcher and final position
                    bool inFront = guardPos.y < finalPos.y; // Guard is closer to launcher
                    float lateralAlignment = Mathf.Abs(guardPos.x - finalPos.x); // How aligned laterally
                    float depthSeparation = finalPos.y - guardPos.y; // How far behind guard
                    
                    // Good protection: Guard in front, good lateral alignment, reasonable depth
                    if (inFront && lateralAlignment < 0.6f && depthSeparation > 0.3f && depthSeparation < 3.0f)
                    {
                        // Score protection quality
                        float alignmentQuality = 1.0f - Mathf.Clamp01(lateralAlignment / 0.6f);
                        float depthQuality = 1.0f - Mathf.Clamp01(Mathf.Abs(depthSeparation - 1.5f) / 1.5f); // Ideal: 1.5 units behind
                        
                        float guardProtectionQuality = alignmentQuality * 0.6f + depthQuality * 0.4f;
                        
                        if (guardProtectionQuality > protectionScore)
                        {
                            protectionScore = guardProtectionQuality;
                            protectingGuard = guard;
                        }
                    }
                }               
                score += protectionScore * 15f; // Up to 15 points for ANY guard protection (friendly or opponent)
                
                // PART 2: SCORING POSITION (30 points max) - Get in scoring position!
                // Philosophy: MULTIPLE rocks score in curling - being 2nd/3rd shot is still valuable!
                float scoringPositionScore = 0f;
                
                float myDistToButton = Vector2.Distance(finalPos, button);
                
                // Are we closer than opponent's best rock?
                if (myDistToButton < closestOpponentDistToButton)
                {
                    // YES! We'd be shot rock!
                    float beatMargin = closestOpponentDistToButton - myDistToButton;
                    
                    // More margin = better (harder for them to steal)
                    scoringPositionScore = Mathf.Clamp01(beatMargin / 1.0f); // Beat by 1 unit = 100%
                    
                    // BONUS: Beat by a lot = huge advantage
                    if (beatMargin > 0.5f) scoringPositionScore += 0.5f; // Big lead
                    
                    scoringPositionScore = Mathf.Min(1.0f, scoringPositionScore); // Cap at 1.0
                }
                else if (closestOpponentDistToButton > 900f)
                {
                    // NO OPPONENT ROCKS - Score based on distance to button
                    // Closer = better, but any position in house is acceptable
                    scoringPositionScore = Mathf.Clamp01(1.0f - (myDistToButton / 2.0f)); // Within 2 units = positive
                }
                else
                {
                    // We're NOT shot rock - SOFTER penalty (multiple rocks score!)
                    // Philosophy: 2nd shot, 3rd shot still count for points in curling
                    float deficit = myDistToButton - closestOpponentDistToButton;
                    
                    // GENTLE SCALING: Small deficits barely penalized
                    if (deficit < 0.5f)
                    {
                        // Very close to opponent (<0.5 units behind) = Minor penalty
                        scoringPositionScore = -0.05f; // Just -1.5 points
                    }
                    else if (deficit < 1.0f)
                    {
                        // Moderate deficit (0.5-1.0 behind) = Moderate penalty
                        scoringPositionScore = -0.2f; // -6 points
                    }
                    else
                    {
                        // Large deficit (>1.0 behind) = Still accept if in house!
                        scoringPositionScore = -0.4f; // -12 points (was -40!)
                    }
                }
                
                score += scoringPositionScore * 30f; // Up to 30 points for scoring position
                
                // PART 3: COLLISION CONTEXT (variable penalty) - WHERE and WHEN matters!
                float collisionPenalty = 0f;
                
                if (collisionInfo.hasCollision)
                {
                    Vector2 collisionPoint = collisionInfo.collisionPoint;
                    GameObject hitRock = collisionInfo.hitRock;
                    
                    // Calculate collision distance from launcher (how far into trajectory)
                    float collisionDistFromLauncher = Vector2.Distance(launcherPos, collisionPoint);
                    float totalPathLength = Vector2.Distance(launcherPos, finalPos);
                    float collisionRatio = collisionDistFromLauncher / totalPathLength; // 0 = early, 1 = late
                    
                    // EARLY COLLISION (first 60% of path) = BIG PENALTY
                    if (collisionRatio < 0.6f)
                    {
                        collisionPenalty = -25f; // Early collision is bad - disrupts path significantly
                    }
                    // MID COLLISION (60-80% of path) = MINOR PENALTY
                    else if (collisionRatio < 0.8f)
                    {
                        collisionPenalty = -8f; // Acceptable if final position is still good
                    }
                    // LATE COLLISION (last 20% of path) = VERY MINOR or ACCEPTABLE
                    else // collisionRatio >= 0.8 (last 20% of path)
                    {
                        collisionPenalty = -2f; // Small bumps near target are OK!
                    }
                    
                    Debug.Log($"[Collision Context] Hit {hitRock.name} at ratio {collisionRatio:F2} ({(collisionRatio < 0.6f ? "EARLY" : collisionRatio < 0.8f ? "MID" : "LATE")}), Penalty={collisionPenalty:F1}");
                }
                else
                {
                    collisionPenalty = +5f; // Small bonus for completely clean path
                }
                
                score += collisionPenalty;
                
                
                // PART 4: IN-HOUSE BONUS (20 points) - Reward shots that land in scoring position
                float houseBonus = 0f;
                if (finalPos.y >= 5.0f && finalPos.y <= 9.0f) // In the house
                {
                    float distToButton = Vector2.Distance(finalPos, button);
                    
                    if (distToButton < 0.6f) // 4-foot
                        houseBonus = 20f;
                    else if (distToButton < 1.2f) // 8-foot
                        houseBonus = 15f;
                    else if (distToButton < 1.83f) // 12-foot
                        houseBonus = 10f;
                }
                
                score += houseBonus;
                
                // ========================================
                // LOG COMPREHENSIVE SCORING BREAKDOWN
                // ========================================
                Debug.Log($"[Physics Draw] Candidate: ({candidateTarget.x:F2}, {candidateTarget.y:F2}) → Final: ({finalPos.x:F2}, {finalPos.y:F2}), Turn: {(tryInTurn ? "IN" : "OUT")}\n" +
                          $"  Proximity to Target: {proximityScore:F1}/60 (dist: {distToTarget:F2}m)\n" +
                          $"  Guard Protection: {protectionScore * 15f:F1}/15 {(protectingGuard != null ? $"(under {protectingGuard.name})" : "(exposed)")}\n" +
                          $"  Scoring Position: {scoringPositionScore * 30f:F1}/30 (dist to button: {myDistToButton:F2}, opponent closest: {closestOpponentDistToButton:F2})\n" +
                          $"  Collision Context: {collisionPenalty:F1} {(collisionInfo.hasCollision ? $"(hit {collisionInfo.hitRock.name})" : "(clean)")}\n" +
                          $"  In-House Bonus: {houseBonus:F1}/20\n" +
                          $"  TOTAL SCORE: {score:F1}/130");
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPullback = testPullback;
                    bestInTurn = tryInTurn;
                    bestFinalPos = finalPos;
                    
                    Debug.Log($"  ⭐ NEW BEST: score {score:F1}, protected={protectingGuard != null}, scoring={(scoringPositionScore > 0)}");
                }
            }
        }
        
        // ========================================
        // ACCEPTANCE CRITERIA: Demand HIGH QUALITY draw shots
        // ========================================
        // RAISED threshold: 40.0 (was 15.0) to demand precision
        // With tighter radii (0.4m max), we should find accurate shots!
        // Score breakdown for reference:
        //   60 points = proximity (<8cm = 60, <15cm = 55, <25cm = 48)
        //   30 points = scoring position (beat opponent, close to button)
        //   15 points = guard protection
        //   20 points = in-house bonus
        //   -25 to +5 = collision context
        // Threshold of 40 requires: <15cm proximity + decent scoring OR <25cm + guard protection
        if (bestScore > float.MinValue && bestScore >= 40f)
        {
            pullbackPosition = bestPullback;
            useInTurn = bestInTurn;
            
            Debug.Log($"[Physics Draw] ✓ SUCCESS! Score: {bestScore:F1}/130 (threshold: 40)\n" +
                      $"  Final position: ({bestFinalPos.x:F2}, {bestFinalPos.y:F2})\n" +
                      $"  Distance to target: {Vector2.Distance(bestFinalPos, targetPosition):F3}m\n" +
                      $"  Pullback: ({bestPullback.x:F3}, {bestPullback.y:F3})\n" +
                      $"  Turn: {(bestInTurn ? "IN-TURN (curls RIGHT →)" : "OUT-TURN (curls LEFT ←)")}\n" +
                      $"  Tested {candidateTargets.Count} candidates (tight 0.4m radius)\n" +
                      $"  Strategy: PRECISION targeting, proximity prioritized, late collisions OK");
            return true;
        }
        
        // IMPROVED FALLBACK: Try direct physics to button as last resort
        Debug.LogWarning($"[Physics Draw] All candidates scored low (best: {bestScore:F1}), trying direct button fallback");
        
        // Try out-turn first (curls LEFT ←)
        Vector2 directButtonVelocity = trajectorySimulator.CalculateVelocityToTarget(
            launcherPos,
            button,
            false // OUT-TURN
        );
        
        if (directButtonVelocity.magnitude > 3f && directButtonVelocity.magnitude < 20f)
        {
            pullbackPosition = CalculatePullbackFromVelocity(directButtonVelocity, launcherPos, false);
            useInTurn = false;
            
            Debug.Log($"[Physics Draw] ✓ FALLBACK: Direct button shot (OUT-TURN, curls LEFT ←), pullback: {pullbackPosition}");
            return true;
        }
        
        // Try in-turn as last resort (curls RIGHT →)
        directButtonVelocity = trajectorySimulator.CalculateVelocityToTarget(
            launcherPos,
            button,
            true // IN-TURN
        );
        
        if (directButtonVelocity.magnitude > 3f && directButtonVelocity.magnitude < 20f)
        {
            pullbackPosition = CalculatePullbackFromVelocity(directButtonVelocity, launcherPos, true);
            useInTurn = true;
            
            Debug.Log($"[Physics Draw] ✓ FALLBACK: Direct button shot (IN-TURN, curls RIGHT →), pullback: {pullbackPosition}");
            return true;
        }
        
        Debug.LogError($"[Physics Draw] COMPLETE FAILURE - even direct button shot failed! Tested {candidateTargets.Count} candidates");
        pullbackPosition = launcherPos + new Vector2(0f, -2f);
        useInTurn = false;
        return false;
    }
    
    /// <summary>
    /// Physics-based guard shot calculation
    /// STRATEGY: Block friendly scoring stones OR block center lane
    /// </summary>
    private bool CalculatePhysicsBasedGuardShot(Vector2 targetPosition, out Vector2 pullbackPosition, out bool useInTurn)
    {
        Vector2 launcherPos = new Vector2(0f, -25f);
        
        // STRATEGIC DECISION: Where should we place the guard?
        Vector2 guardTarget;
        Rock_Info rockInfo = gm.rockList[gm.rockCurrent].rockInfo;
        
        // Check if we have rocks in the house to protect
        bool haveFriendlyRocks = false;
        Vector2 friendlyRockAvgPos = Vector2.zero;
        int friendlyCount = 0;
        
        foreach (var houseRock in gm.houseList)
        {
            if (houseRock.rockInfo.teamName == rockInfo.teamName)
            {
                friendlyRockAvgPos += (Vector2)houseRock.rock.transform.position;
                friendlyCount++;
                haveFriendlyRocks = true;
            }
        }
        
        if (haveFriendlyRocks && friendlyCount > 0)
        {
            // PROTECT FRIENDLY ROCKS: Place guard between launcher and friendly rocks
            friendlyRockAvgPos /= friendlyCount;
            
            // Guard position: ~60% of the way from launcher to friendly rock
            // This blocks direct takeout attempts
            Vector2 launcherToFriendly = friendlyRockAvgPos - launcherPos;
            guardTarget = launcherPos + launcherToFriendly * 0.35f; // Closer to launcher = better guard
            
            // Clamp to guard zone (Y between 2.0 and 5.0)
            guardTarget.y = Mathf.Clamp(guardTarget.y, 2.5f, 4.5f);
            
            Debug.Log($"[Physics Guard] PROTECT: Guarding friendly rocks at ({friendlyRockAvgPos.x:F2}, {friendlyRockAvgPos.y:F2}) → guard at ({guardTarget.x:F2}, {guardTarget.y:F2})");
        }
        else
        {
            // NO FRIENDLY ROCKS: Block center lane (most common approach)
            // Center guard: X = 0 (or close), Y = 3-4 (standard guard position)
            float guardY = Random.Range(3.0f, 4.0f);
            float guardX = Random.Range(-0.2f, 0.2f); // Slight variance for realism
            
            guardTarget = new Vector2(guardX, guardY);
            Debug.Log($"[Physics Guard] CENTER BLOCK: Placing center guard at ({guardTarget.x:F2}, {guardTarget.y:F2})");
        }
        
        // Get rocks in play (other guards are obstacles - we don't want to hit them)
        List<GameObject> rocksInPlay = new List<GameObject>();
        foreach (var rockEntry in gm.rockList)
        {
            if (rockEntry.rock != null && rockEntry.rock.activeInHierarchy && rockEntry.rockInfo.inPlay)
            {
                rocksInPlay.Add(rockEntry.rock);
            }
        }
        
        float bestScore = float.MinValue;
        Vector2 bestPullback = Vector2.zero;
        bool bestInTurn = false;
        
        // Try both turn directions
        for (int turnDir = 0; turnDir < 2; turnDir++)
        {
            bool tryInTurn = (turnDir == 0);
            
            // Calculate required velocity to reach guard position
            Vector2 requiredVelocity = trajectorySimulator.CalculateVelocityToTarget(
                launcherPos,
                guardTarget,
                tryInTurn
            );
            
            Debug.Log($"[Physics Guard] Calculated velocity: {requiredVelocity.magnitude:F2} m/s to reach {guardTarget}");
            
            if (requiredVelocity.magnitude < 3f || requiredVelocity.magnitude > 15f)
                continue;
            
            Vector2 testPullback = CalculatePullbackFromVelocity(requiredVelocity, launcherPos, tryInTurn);
            
            // Simulate to see if we reach guard position cleanly
            List<Vector2> simulatedPath = trajectorySimulator.SimulateTrajectory(
                launcherPos,
                requiredVelocity,
                tryInTurn,
                250,
                rocksInPlay,
                forPlayerPreview: false
            );
            
            if (simulatedPath.Count == 0) continue;
            
            Vector2 finalPos = simulatedPath[simulatedPath.Count - 1];
            float distanceToTarget = Vector2.Distance(finalPos, guardTarget);
            
            // Score: closer to guard position = better
            TrajectorySimulator.CollisionInfo collisionInfo = trajectorySimulator.GetCollisionInfo();
            float score = -distanceToTarget;
            
            // Penalty if we hit anything (guards should land cleanly)
            if (collisionInfo.hasCollision)
            {
                score -= 3f;
            }
            
            // Bonus if we land in the guard zone (Y between 2.0 and 5.0)
            if (finalPos.y >= 2.0f && finalPos.y <= 5.0f)
            {
                score += 1f;
            }
            
            if (score > bestScore)
            {
                bestScore = score;
                bestPullback = testPullback;
                bestInTurn = tryInTurn;
            }
        }
        
        if (bestScore > float.MinValue && bestScore > -1.0f) // Within 1 unit is acceptable for guards
        {
            pullbackPosition = bestPullback;
            useInTurn = bestInTurn;
            return true;
        }
        
        pullbackPosition = launcherPos + new Vector2(0f, -1.5f);
        useInTurn = false;
        return false;
    }

    IEnumerator GuardTarget(int rockCurrent)
    {
        // PHYSICS-BASED: Place guard in front of house
        // Target area is in guards zone (y < 5f typically)
        Vector2 targetPosition = new Vector2 (0, 2f);
        
        Vector2 pullbackPos;
        bool useInTurn;
        
        // CRITICAL FIX: Call CalculatePhysicsBasedGUARDShot (not DrawShot!)
        // Guards require LESS velocity than draws (shorter distance)
        bool foundShot = CalculatePhysicsBasedGuardShot(targetPosition, out pullbackPos, out useInTurn);
        
        if (foundShot)
        {
            // Apply accuracy modifier
            CharacterStats shooterStats = GetShooterStats(rockCurrent);
            if (shooterStats != null)
            {
                float accuracy = shooterStats.guardAccuracy.GetValue() / 100f;
                float maxError = 0.18f * (1f - accuracy); // Guards have moderate tolerance
                Vector2 errorOffset = Random.insideUnitCircle * maxError;
                pullbackPos += errorOffset;
            }
            
            rm.inturn = useInTurn;
            takeOutX = pullbackPos.x;
            takeOutY = pullbackPos.y;
            
            Debug.Log($"[Physics Guard] SUCCESS - Pullback: {pullbackPos}, InTurn: {useInTurn}");
        }
        else
        {
            // Fallback
            Debug.LogWarning("[Physics Guard] Failed, using fallback");
            targetX = targetPos.x;
            targetY = targetPos.y;

            takeOutY = (-0.25f * (targetY / 5f)) - 26.65f;

            if (rm.inturn == false)
            {
                takeOutX = (-0.165f * ((targetX + 1.35f) / 2.7f)) + 0.029f;
            }
            else
            {
                takeOutX = (-0.165f * ((targetX + 1.35f) / 2.7f)) + 0.12f;
            }
        }

        aiShoot.OnShot("Guard To Target", rockCurrent);
        yield break;
    }

    IEnumerator DrawTwelveFoot(int rockCurrent)
    {
        yield return StartCoroutine(GuardReading(rockCurrent));

        //if there's at least one guard
        if (gm.gList.Count != 0)
        {
            //only a centre guard
            if (cenGuard && !lCornGuard && !rCornGuard)
            {
                //centre guard to the right
                if (cenGuard.position.x > 0f)
                {
                    rm.inturn = true;
                    aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                    yield break;
                }
                //centre guard to the left
                else if (cenGuard.position.x < 0f)
                {
                    rm.inturn = false;
                    aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                    yield break;
                }
            }

            //centre guard and a right guard and a left guard
            else if (cenGuard && rCornGuard && lCornGuard)
            {
                //high centre guard
                if (cenGuard.position.y < 2.0f)
                {
                    //centre guard to the right
                    if (cenGuard.position.x > 0f)
                    {
                        rm.inturn = true;
                        aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                        yield break;
                    }
                    //centre guard to the left
                    else if (cenGuard.position.x < 0f)
                    {
                        rm.inturn = false;
                        aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                        yield break;
                    }
                }
                //centre guard is medium height
                else if (cenGuard.position.y < 3.0f)
                {
                    //corner guards are high
                    if (rCornGuard.position.y < 2.0f && lCornGuard.position.y < 2.0f)
                    {
                        //centre guard to the right
                        if (cenGuard.position.x > 0f)
                        {
                            rm.inturn = true;
                            aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                            yield break;
                        }
                        //centre guard to the left
                        else if (cenGuard.position.x < 0f)
                        {
                            rm.inturn = false;
                            aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                            yield break;
                        }
                    }
                    //left corner guard is high
                    else if (lCornGuard.position.y < 2.0f)
                    {
                        rm.inturn = false;
                        aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                        yield break;
                    }
                    //right corner guard is high
                    else if (rCornGuard.position.y < 2.0f)
                    {
                        rm.inturn = true;
                        aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                        yield break;
                    }
                }
                //low centre guard
                else if (cenGuard.position.y < 4.8f)
                {
                    //both corner guards are higher
                    if (rCornGuard.position.y < cenGuard.position.y && lCornGuard.position.y < cenGuard.position.y)
                    {
                        //centre guard to the right
                        if (cenGuard.position.x > 0f)
                        {
                            rm.inturn = true;
                            aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                            yield break;
                        }
                        //centre guard to the left
                        else if (cenGuard.position.x < 0f)
                        {
                            rm.inturn = false;
                            aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                            yield break;
                        }
                    }
                    //left corner guard is higher
                    else if (lCornGuard.position.y < cenGuard.position.y)
                    {
                        rm.inturn = false;
                        aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                        yield break;
                    }
                    //right corner guard is higher
                    else if (rCornGuard.position.y < cenGuard.position.y)
                    {
                        rm.inturn = true;
                        aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                        yield break;
                    }
                }
                //any other situation
                else
                {
                    //centre guard to the right
                    if (cenGuard.position.x > 0f)
                    {
                        rm.inturn = true;
                        aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                        yield break;
                    }
                    //centre guard to the left
                    else if (cenGuard.position.x < 0f)
                    {
                        rm.inturn = false;
                        aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                        yield break;
                    }
                }
            }

            //centre guard and a left guard
            else if (cenGuard && lCornGuard && !rCornGuard)
            {
                    rm.inturn = false;
                    aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                    yield break;
            }

            //centre guard and a right guard
            else if (cenGuard && rCornGuard && !lCornGuard)
            {
                if (cenGuard.position.x > 0f)
                {
                    rm.inturn = true;
                    aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                    yield break;
                }
                else if (cenGuard.position.x < 0f)
                {
                    rm.inturn = true;
                    aiShoot.OnShot("Top Twelve Foot", rockCurrent);
                    yield break;
                }
            }

            //right and a left guard
            else if (rCornGuard && lCornGuard && !cenGuard)
            {
                if (rCornGuard.position.y < lCornGuard.position.y)
                {
                    rm.inturn = true;
                    aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                    yield break;
                }
                else if (lCornGuard.position.y < rCornGuard.position.y)
                {
                    rm.inturn = false;
                    aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                    yield break;
                }
            }

            //right corner guard
            else if (rCornGuard && !lCornGuard && !cenGuard)
            {
                rm.inturn = true;
                aiShoot.OnShot("Right Twelve Foot", rockCurrent);
                yield break;
            }

            //left corner guard
            else if (lCornGuard && !rCornGuard && !cenGuard)
            {
                rm.inturn = false;
                aiShoot.OnShot("Left Twelve Foot", rockCurrent);
                yield break;
            }
        }

        //if there's no guards
        else
        {
            if (Random.value > 0.5f)
            {
                rm.inturn = true;
            }
            else rm.inturn = false;

            aiShoot.OnShot("Top Twelve Foot", rockCurrent);
            yield break;
        }

    }

    IEnumerator DrawFourFoot(int rockCurrent)
    {
        //read where the guards are
        yield return StartCoroutine(GuardReading(rockCurrent));

        //if there are guards
        if (gm.gList.Count != 0)
        {
            //only a centre guard
            if (cenGuard && !lCornGuard && !rCornGuard)
            {
                //centre guard to the right
                if (cenGuard.position.x > 0f)
                {
                    rm.inturn = true;
                    aiShoot.OnShot("Top Four Foot", rockCurrent);
                    yield break;
                }
                //centre guard to the left
                else
                {
                    rm.inturn = false;
                    aiShoot.OnShot("Top Four Foot", rockCurrent);
                    yield break;
                }
            }

            //centre guard and a right guard and a left guard
            else if (cenGuard && rCornGuard && lCornGuard)
            {
                //high centre guard
                if (cenGuard.position.y < 2.0f)
                {
                    //centre guard to the right
                    if (cenGuard.position.x > 0f)
                    {
                        rm.inturn = true;
                        aiShoot.OnShot("Top Four Foot", rockCurrent);
                        yield break;
                    }
                    //centre guard to the left
                    else
                    {
                        rm.inturn = false;
                        aiShoot.OnShot("Top Four Foot", rockCurrent);
                        yield break;
                    }
                }
                //centre guard is medium height
                else if (cenGuard.position.y < 3.0f)
                {
                    //corner guards are high
                    if (rCornGuard.position.y < 2.0f && lCornGuard.position.y < 2.0f)
                    {
                        //centre guard to the right
                        if (cenGuard.position.x > 0f)
                        {
                            rm.inturn = true;
                            aiShoot.OnShot("Top Four Foot", rockCurrent);
                            yield break;
                        }
                        //centre guard to the left
                        else
                        {
                            rm.inturn = false;
                            aiShoot.OnShot("Top Four Foot", rockCurrent);
                            yield break;
                        }
                    }
                    //left corner guard is high
                    else if (lCornGuard.position.y < 2.0f)
                    {
                        rm.inturn = false;
                        aiShoot.OnShot("Top Four Foot", rockCurrent);
                        yield break;
                    }
                    //right corner guard is high
                    else
                    {
                        rm.inturn = true;
                        aiShoot.OnShot("Top Four Foot", rockCurrent);
                        yield break;
                    }
                }
                //low centre guard
                else if (cenGuard.position.y < 4.8f)
                {
                    //both corner guards are higher
                    if (rCornGuard.position.y < cenGuard.position.y && lCornGuard.position.y < cenGuard.position.y)
                    {
                        //centre guard to the right
                        if (cenGuard.position.x > 0f)
                        {
                            rm.inturn = true;
                            aiShoot.OnShot("Top Four Foot", rockCurrent);
                            yield break;
                        }
                        //centre guard to the left
                        else
                        {
                            rm.inturn = false;
                            aiShoot.OnShot("Top Four Foot", rockCurrent);
                            yield break;
                        }
                    }
                    //left corner guard is higher
                    else if (lCornGuard.position.y < cenGuard.position.y)
                    {
                        rm.inturn = false;
                        aiShoot.OnShot("Top Four Foot", rockCurrent);
                        yield break;
                    }
                    //right corner guard is higher
                    else if (rCornGuard.position.y < cenGuard.position.y)
                    {
                        rm.inturn = true;
                        aiShoot.OnShot("Top Four Foot", rockCurrent);
                        yield break;
                    }
                    else
                    {
                        rm.inturn = false;
                        aiShoot.OnShot("Top Four Foot", rockCurrent);
                        yield break;
                    }
                }
                //any other situation
                else
                {
                    //centre guard to the right
                    if (cenGuard.position.x > 0f)
                    {
                        rm.inturn = true;
                        aiShoot.OnShot("Top Four Foot", rockCurrent);
                        yield break;
                    }
                    //centre guard to the left
                    else
                    {
                        rm.inturn = false;
                        aiShoot.OnShot("Top Four Foot", rockCurrent);
                        yield break;
                    }
                }
            }

            //centre guard and a left guard
            else if (cenGuard && lCornGuard && !rCornGuard)
            {
                if (cenGuard.position.x > 0f)
                {
                    rm.inturn = false;
                    aiShoot.OnShot("Top Four Foot", rockCurrent);
                    yield break;
                }
                else
                {
                    rm.inturn = false;
                    aiShoot.OnShot("Left Four Foot", rockCurrent);
                    yield break;
                }
            }

            //centre guard and a right guard
            else if (cenGuard && rCornGuard && !lCornGuard)
            {
                if (cenGuard.position.x > 0f)
                {
                    rm.inturn = true;
                    aiShoot.OnShot("Right Four Foot", rockCurrent);
                    yield break;
                }
                else
                {
                    rm.inturn = true;
                    aiShoot.OnShot("Top Four Foot", rockCurrent);
                    yield break;
                }
            }

            //right and a left guard
            else if (rCornGuard && lCornGuard && !cenGuard)
            {
                if (rCornGuard.position.y < lCornGuard.position.y)
                {
                    rm.inturn = true;
                    aiShoot.OnShot("Right Four Foot", rockCurrent);
                    yield break;
                }
                else
                {
                    rm.inturn = false;
                    aiShoot.OnShot("Left Four Foot", rockCurrent);
                    yield break;
                }
            }

            //right corner guard
            else if (rCornGuard && !lCornGuard && !cenGuard)
            {
                rm.inturn = true;
                aiShoot.OnShot("Right Four Foot", rockCurrent);
                yield break;
            }

            //left corner guard
            else
            {
                rm.inturn = false;
                aiShoot.OnShot("Left Four Foot", rockCurrent);
                yield break;
            }
        }

        //if there's no guards
        else
        {
            if (Random.value > 0.5f)
            {
                rm.inturn = true;
            }
            else rm.inturn = false;

            if (rockCurrent < 4)
            {
                aiShoot.OnShot("Top Four Foot", rockCurrent);
            }
            else
            {
                aiShoot.OnShot("Button", rockCurrent);
            }
            yield break;
        }
    }
    
    #region INTENT-BASED SHOT SELECTION (NEW ARCHITECTURE)
    
    /// <summary>
    /// NEW ARCHITECTURE: Execute a strategic intent by evaluating ALL tactical options
    /// Strategy layer decides WHAT to do (intent), this method decides HOW (execution)
    /// </summary>
    public void ExecuteIntent(ShotContext context, int rockCurrent)
    {
        Debug.Log($"[AI_Target] ExecuteIntent: {context.intent} for rock #{rockCurrent}");
        
        switch (context.intent)
        {
            case ShotIntent.RemoveThreat:
                EvaluateRemovalOptions(context, rockCurrent);
                break;
                
            case ShotIntent.ScorePoints:
                EvaluateScoringOptions(context, rockCurrent);
                break;

            case ShotIntent.DrawToButton:
                OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                break;
                
            case ShotIntent.ProtectLead:
                EvaluateProtectLeadOptions(context, rockCurrent);
                break;

            case ShotIntent.CreateOpportunity:
                PlaceStrategicGuard(context, rockCurrent);
                break;
                
            case ShotIntent.ForceBlank:
                EvaluateForceBlankOptions(context, rockCurrent);
                break;

            case ShotIntent.Desperation:
                EvaluateDesperationOptions(context, rockCurrent);
                break;

            case ShotIntent.ThrowAway:
                // Just throw it out - guard or wide draw
                OnTarget("Auto Guard", rockCurrent, 0);
                break;
                
            default:
                Debug.LogWarning($"[AI_Target] Unhandled intent: {context.intent}, defaulting to draw");
                OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                break;
        }
    }
    
    /// <summary>
    /// Evaluate ALL options for removing a threat rock, pick the best one
    /// Options: Direct takeout, peel guard, raise friendly rock, tick it out
    /// </summary>
    private void EvaluateRemovalOptions(ShotContext context, int rockCurrent)
    {
        if (context.targetRockIndex < 0 || context.targetRockIndex >= gm.rockList.Count)
        {
            Debug.LogWarning($"[AI_Target] RemoveThreat: Invalid target index {context.targetRockIndex}");
            OnTarget("Auto Draw Four Foot", rockCurrent, 0);
            return;
        }

        GameObject currentRock = gm.rockList[rockCurrent].rock;
        Rock_Info currentRockInfo = gm.rockList[rockCurrent].rockInfo;

        GameObject targetRock = gm.rockList[context.targetRockIndex].rock;
        if (targetRock == null || !targetRock.activeInHierarchy)
        {
            Debug.LogWarning($"[AI_Target] RemoveThreat: Target rock not active");
            OnTarget("Auto Draw Four Foot", rockCurrent, 0);
            return;
        }
        
        Debug.Log($"[AI_Target] Evaluating removal options for rock #{context.targetRockIndex} at {targetRock.transform.position}");
        
        // OPTION 1: Direct takeout (always available)
        float takeoutScore = SimulateTakeout(targetRock, context.targetRockIndex, rockCurrent);
        Debug.Log($"  Option 1: Direct Takeout - Score: {takeoutScore:F2}");
        
        // OPTION 2: Peel the guard (if there's one blocking)
        float peelScore = 0f;
        int guardToPeel = -1;

        for (int i = 0; i < gm.gList.Count; i++)
        {
            var guard = gm.gList[i];
            if (guard.lastTransform == null)
                continue;

            Rock_Info guardInfo = guard.lastTransform.GetComponent<Rock_Info>();
            if (guardInfo == null || guardInfo.teamName == currentRockInfo.teamName)
                continue; // Skip our own guards

            if (IsGuardBlocking(guard.lastTransform, targetRock))
            {
                guardToPeel = guardInfo.rockIndex;
                peelScore = SimulatePeel(guard.lastTransform.gameObject, guardToPeel, rockCurrent);
                Debug.Log($"  Option 2: Peel through guard #{guardToPeel} - Score: {peelScore:F2}");
            }
        }

        // OPTION 3: Runback - hit guard through to target
        float runbackScore = 0f;
        int guardToRunback = -1;

        for (int i = 0; i < gm.gList.Count; i++)
        {
            var guard = gm.gList[i];
            if (guard.lastTransform == null)
                continue;

            Rock_Info guardInfo = guard.lastTransform.GetComponent<Rock_Info>();
            if (guardInfo == null || guardInfo.teamName == currentRockInfo.teamName)
                continue; // Skip our own guards

            if (IsGuardBlocking(guard.lastTransform, targetRock))
            {
                guardToRunback = guardInfo.rockIndex;
                runbackScore = SimulateRunback(guard.lastTransform.gameObject, targetRock, guardToRunback, context.targetRockIndex, rockCurrent);
                Debug.Log($"  Option 3: Runback through guard #{guardToRunback} - Score: {runbackScore:F2}");
            }
        }

        // OPTION 4: Tick it out sideways
        float tickScore = SimulateTick(targetRock, context.targetRockIndex, rockCurrent);
        Debug.Log($"  Option 4: Tick shot - Score: {tickScore:F2}");

        // PICK THE BEST OPTION!
        float bestScore = Mathf.Max(takeoutScore, peelScore, tickScore, runbackScore);
        
        if (bestScore <= 0f)
        {
            Debug.LogWarning("[AI_Target] No good removal options found, drawing instead");
            EvaluateScoringOptions(context, rockCurrent);
            return;
        }
        
        // Execute best option
        if (takeoutScore == bestScore && takeoutScore > 0f)
        {
            Debug.Log($"[AI_Target] ✓ SELECTED: Direct Takeout (score: {takeoutScore:F2})");
            OnTarget("Take Out", rockCurrent, context.targetRockIndex);
        }
        else if (peelScore == bestScore && peelScore > 0f)
        {
            Debug.Log($"[AI_Target] ✓ SELECTED: Peel Guard (score: {peelScore:F2})");
            OnTarget("Peel", rockCurrent, guardToPeel);
        }
        else if (tickScore == bestScore && tickScore > 0f)
        {
            Debug.Log($"[AI_Target] ✓ SELECTED: Tick Shot (score: {tickScore:F2})");
            OnTarget("Tick Shot", rockCurrent, context.targetRockIndex);
        }
        else if (runbackScore == bestScore && runbackScore > 0f)
        {
            Debug.Log($"[AI_Target] ✓ SELECTED: Runback (score: {runbackScore:F2}) - Hit guard through to target!");
            OnTarget("Runback", rockCurrent, guardToRunback);
        }
    }
    
    /// <summary>
    /// Evaluate ALL options for scoring points, pick the best one
    /// Options: Draw to button, freeze on opponent rock, raise friendly rock, tick opponent into house, remove blocker
    /// </summary>
    private void EvaluateScoringOptions(ShotContext context, int rockCurrent)
    {
        Debug.Log($"[AI_Target] Evaluating scoring options for rock #{rockCurrent}");
        
        Vector2 button = new Vector2(0f, 6.5f);
        
        // OPTION 1: Direct draw to button (always available)
        float drawScore = SimulateDraw(button, rockCurrent);
        Debug.Log($"  Option 1: Draw to button - Score: {drawScore:F2}");
        
        // OPTION 2: Freeze on opponent's best rock
        float freezeScore = 0f;
        int rockToFreeze = -1;
        
        if (gm.houseList.Count > 0)
        {
            // Find best opponent rock to freeze on (already has out parameter for score)
            rockToFreeze = FindBestFreezeTarget(rockCurrent, out freezeScore);
            
            if (rockToFreeze >= 0)
            {
                Debug.Log($"  Option 2: Freeze on rock #{rockToFreeze} - Score: {freezeScore:F2}");
            }
        }
        
        // OPTION 3: Raise a friendly rock closer to button
        float raiseScore = 0f;
        int rockToRaiseForScore = FindBestRockToRaiseForScoring(rockCurrent, out raiseScore);
        
        if (rockToRaiseForScore >= 0)
        {
            Debug.Log($"  Option 3: Raise rock #{rockToRaiseForScore} toward button - Score: {raiseScore:F2}");
        }
        
        // OPTION 4: Draw behind existing guard for protection
        float protectedDrawScore = 0f;
        Vector2 protectedDrawTarget = Vector2.zero;
        
        if (gm.gList.Count > 0)
        {
            protectedDrawTarget = FindBestProtectedDrawPosition(rockCurrent, out protectedDrawScore);
            
            if (protectedDrawScore > 0f)
            {
                Debug.Log($"  Option 6: Protected draw at ({protectedDrawTarget.x:F2}, {protectedDrawTarget.y:F2}) - Score: {protectedDrawScore:F2}");
            }
        }
        
        // PICK THE BEST SCORING OPTION!
        float bestScore = Mathf.Max(drawScore, freezeScore, raiseScore, protectedDrawScore);
        
        if (bestScore <= 0f)
        {
            Debug.LogWarning("[AI_Target] No good scoring options found, defaulting to button draw");
            
            OnTarget("Auto Draw Four Foot", rockCurrent, 0);
            return;
        }
        
        // Execute best scoring option
        if (drawScore == bestScore && drawScore > 0f)
        {
            Debug.Log($"[AI_Target] ✓ SELECTED: Draw to button (score: {drawScore:F2})");
            StartCoroutine(DrawTarget(rockCurrent, button));
        }
        else if (freezeScore == bestScore && freezeScore > 0f)
        {
            Debug.Log($"[AI_Target] ✓ SELECTED: Freeze (score: {freezeScore:F2}) - Steal shot rock!");
            OnTarget("Freeze", rockCurrent, rockToFreeze);
        }
        else if (raiseScore == bestScore && raiseScore > 0f)
        {
            Debug.Log($"[AI_Target] ✓ SELECTED: Raise rock (score: {raiseScore:F2}) - Promote to shot rock!");
            OnTarget("Tap Back", rockCurrent, rockToRaiseForScore);
        }
        else if (protectedDrawScore == bestScore && protectedDrawScore > 0f)
        {
            Debug.Log($"[AI_Target] ✓ SELECTED: Protected draw (score: {protectedDrawScore:F2}) - Behind our guard!");
            StartCoroutine(DrawTarget(rockCurrent, protectedDrawTarget));
        }
    }
    
    /// <summary>
    /// Evaluate ALL options for protecting a lead - CONSERVATIVE play
    /// Priority: Remove threats > Clear guards > Safe draws to sides
    /// </summary>
    private void EvaluateProtectLeadOptions(ShotContext context, int rockCurrent)
    {
        Debug.Log($"[AI_Target] PROTECT LEAD - Evaluating conservative options for rock #{rockCurrent}");
        
        Rock_Info currentRockInfo = gm.rockList[rockCurrent].rockInfo;
        
        // PHASE 1: Are there opponent rocks in the house? REMOVE THEM!
        GameObject opponentThreat = null;
        int opponentThreatIndex = -1;
        
        foreach (var houseRock in gm.houseList)
        {
            if (houseRock.rockInfo.teamName != currentRockInfo.teamName)
            {
                opponentThreat = houseRock.rock;
                opponentThreatIndex = houseRock.rockInfo.rockIndex;
                break; // Take out the closest (shot rock)
            }
        }
        
        if (opponentThreat != null)
        {
            Debug.Log($"[Protect Lead] PHASE 1: Opponent rock #{opponentThreatIndex} in house - REMOVE IT!");
            
            // OPTION 1: Direct takeout (PREFERRED - most reliable)
            float takeoutScore = SimulateTakeout(opponentThreat, opponentThreatIndex, rockCurrent);
            Debug.Log($"  Option 1: Direct Takeout - Score: {takeoutScore:F2}");
            
            // OPTION 2: Runback through guard (PREFERRED - removes 2 rocks)
            float runbackScore = 0f;
            int guardToRunback = -1;

            for (int i = 0; i < gm.gList.Count; i++)
            {
                var guard = gm.gList[i];
                if (guard.lastTransform == null)
                    continue;

                Rock_Info guardInfo = guard.lastTransform.GetComponent<Rock_Info>();
                if (guardInfo == null || guardInfo.teamName == currentRockInfo.teamName)
                    continue; // Skip our own guards

                if (IsGuardBlocking(guard.lastTransform, opponentThreat))
                {
                    guardToRunback = guardInfo.rockIndex;
                    runbackScore = SimulateRunback(guard.lastTransform.gameObject, opponentThreat, guardToRunback, opponentThreatIndex, rockCurrent);
                    Debug.Log($"  Option 2: Runback through guard #{guardToRunback} - Score: {runbackScore:F2}");
                }
            }

            // OPTION 3: Raise friendly rock into it (LAST RESORT)
            float raiseScore = 0f;
            int rockToRaise = FindBestRaiseTarget(opponentThreat, rockCurrent);
            if (rockToRaise >= 0)
            {
                raiseScore = SimulateRaise(opponentThreat, rockToRaise, rockCurrent);
                Debug.Log($"  Option 4: Raise rock #{rockToRaise} - Score: {raiseScore:F2}");
            }
            
            // PICK BEST REMOVAL OPTION (prefer takeout/runback)
            float bestScore = Mathf.Max(takeoutScore, runbackScore, raiseScore);
            
            if (takeoutScore == bestScore && takeoutScore > 0f)
            {
                Debug.Log($"[Protect Lead] ✓ SELECTED: Direct Takeout (score: {takeoutScore:F2}) - Clean removal!");
                OnTarget("Take Out", rockCurrent, opponentThreatIndex);
            }
            else if (runbackScore == bestScore && runbackScore > 0f)
            {
                Debug.Log($"[Protect Lead] ✓ SELECTED: Runback (score: {runbackScore:F2}) - Remove guard + threat!");
                OnTarget("Runback", rockCurrent, guardToRunback);
            }
            else if (raiseScore == bestScore && raiseScore > 0f)
            {
                Debug.Log($"[Protect Lead] ✓ SELECTED: Raise rock (score: {raiseScore:F2}) - Last resort!");
                OnTarget("Tap Back", rockCurrent, rockToRaise);
            }
            else
            {
                // Fallback: try peel if nothing else works
                Debug.LogWarning("[Protect Lead] No good removal options, trying peel...");
                OnTarget("Peel", rockCurrent, opponentThreatIndex);
            }
            
            return; // Done - removed the threat!
        }
        
        // PHASE 2: No rocks in house - are there OPPONENT guards blocking center? CLEAR THEM!
        int opponentGuardToPeel = -1;
        float bestGuardPeelScore = 0f;
        
        foreach (var guard in gm.gList)
        {
            if (guard.lastTransform == null)
                continue;
            
            Rock_Info guardInfo = guard.lastTransform.GetComponent<Rock_Info>();
            if (guardInfo == null || guardInfo.teamName == currentRockInfo.teamName)
                continue; // Skip our own guards
            
            Vector2 guardPos = guard.lastTransform.position;
            
            // Score based on how much they block the center lane
            float centerednessScore = 1.0f - Mathf.Clamp01(Mathf.Abs(guardPos.x) / 1.0f);
            float proximityToHouseScore = Mathf.Clamp01((guardPos.y - 2.0f) / 3.0f);
            
            bool blockingCenterPath = Mathf.Abs(guardPos.x) < 0.5f && guardPos.y > 2.0f && guardPos.y < 5.0f;
            float blockingBonus = blockingCenterPath ? 30f : 0f;
            
            float score = (centerednessScore * 35f) + (proximityToHouseScore * 35f) + blockingBonus;
            
            if (score > bestGuardPeelScore)
            {
                bestGuardPeelScore = score;
                opponentGuardToPeel = guardInfo.rockIndex;
            }
        }
        
        // Only peel guards if they're blocking center significantly (score > 50)
        if (opponentGuardToPeel >= 0 && bestGuardPeelScore > 50f)
        {
            Debug.Log($"[Protect Lead] PHASE 2: Opponent guard #{opponentGuardToPeel} blocking center (score: {bestGuardPeelScore:F2}) - PEEL IT!");
            OnTarget("Peel", rockCurrent, opponentGuardToPeel);
            return;
        }
        
        // PHASE 3: No threats, no blocking guards - CONSERVATIVE DRAW to sides
        Debug.Log($"[Protect Lead] PHASE 3: No threats - Conservative draw to side");
        
        Vector2 button = new Vector2(0f, 6.5f);
        
        // OPTION 1: Draw to left side (X = -1.0)
        Vector2 leftSideTarget = new Vector2(-1.0f, button.y);
        float leftSideScore = 50f; // Base conservative score
        
        // Check if left is clear
        bool leftClear = true;
        foreach (var guard in gm.gList)
        {
            if (guard.lastTransform != null)
            {
                Vector2 guardPos = guard.lastTransform.position;
                if (guardPos.x < -0.5f && guardPos.y > 2.0f && guardPos.y < 5.0f)
                {
                    leftClear = false;
                    break;
                }
            }
        }
        
        if (!leftClear) leftSideScore -= 20f; // Penalty for blocked path
        
        // OPTION 2: Draw to right side (X = +1.0)
        Vector2 rightSideTarget = new Vector2(1.0f, button.y);
        float rightSideScore = 50f; // Base conservative score
        
        // Check if right is clear
        bool rightClear = true;
        foreach (var guard in gm.gList)
        {
            if (guard.lastTransform != null)
            {
                Vector2 guardPos = guard.lastTransform.position;
                if (guardPos.x > 0.5f && guardPos.y > 2.0f && guardPos.y < 5.0f)
                {
                    rightClear = false;
                    break;
                }
            }
        }
        
        if (!rightClear) rightSideScore -= 20f; // Penalty for blocked path
        
        // OPTION 3: Draw to button (LAST RESORT - too aggressive for protect lead)
        float centerScore = 30f; // Lower base score - we prefer sides
        
        Debug.Log($"  Conservative draw options:\n" +
                  $"    Left side (X=-1.0): {leftSideScore:F2} (clear: {leftClear})\n" +
                  $"    Right side (X=+1.0): {rightSideScore:F2} (clear: {rightClear})\n" +
                  $"    Center (button): {centerScore:F2}");
        
        // PICK BEST CONSERVATIVE DRAW
        float bestDrawScore = Mathf.Max(leftSideScore, rightSideScore, centerScore);
        
        if (leftSideScore == bestDrawScore)
        {
            Debug.Log($"[Protect Lead] ✓ SELECTED: Draw to LEFT SIDE (score: {leftSideScore:F2}) - Safe play!");
            StartCoroutine(DrawTarget(rockCurrent, leftSideTarget));
        }
        else if (rightSideScore == bestDrawScore)
        {
            Debug.Log($"[Protect Lead] ✓ SELECTED: Draw to RIGHT SIDE (score: {rightSideScore:F2}) - Safe play!");
            StartCoroutine(DrawTarget(rockCurrent, rightSideTarget));
        }
        else
        {
            Debug.Log($"[Protect Lead] ✓ SELECTED: Draw to CENTER (score: {centerScore:F2}) - Fallback!");
            OnTarget("Auto Draw Four Foot", rockCurrent, 0);
        }
    }
    
    /// <summary>
    /// Evaluate ALL options for desperation scoring - AGGRESSIVE comeback play
    /// Context: Down by N points, need to score N to tie (or N+1 to win)
    /// Strategy: Risk everything to score - go for big plays!
    /// </summary>
    private void EvaluateDesperationOptions(ShotContext context, int rockCurrent)
    {
        Rock_Info currentRockInfo = gm.rockList[rockCurrent].rockInfo;
        Vector2 button = new Vector2(0f, 6.5f);
        
        // Calculate desperation level: How many rocks do we need?
        int rocksNeeded = CalculateRocksNeeded(currentRockInfo.teamName);
        
        Debug.Log($"[AI_Target] DESPERATION MODE - Need {rocksNeeded} rock(s) to tie/win!");
        
        // PHASE 1: If opponent has rocks, REMOVE THEM FIRST!
        GameObject opponentRock = null;
        int opponentRockIndex = -1;
        
        foreach (var houseRock in gm.houseList)
        {
            if (houseRock.rockInfo.teamName != currentRockInfo.teamName)
            {
                opponentRock = houseRock.rock;
                opponentRockIndex = houseRock.rockInfo.rockIndex;
                break; // Take out shot rock
            }
        }
        
        if (opponentRock != null)
        {
            Debug.Log($"[Desperation] PHASE 1: Opponent rock #{opponentRockIndex} blocking - MUST REMOVE!");
            
            // Calculate aggression multiplier based on desperation
            float aggressionBonus = rocksNeeded * 10f; // More desperate = more aggressive
            
            // OPTION 1: Direct takeout (ALWAYS try this first)
            float takeoutScore = SimulateTakeout(opponentRock, opponentRockIndex, rockCurrent) + aggressionBonus;
            Debug.Log($"  Option 1: Aggressive Takeout - Score: {takeoutScore:F2} (bonus: {aggressionBonus:F2})");
            
            // OPTION 2: Runback (try to remove 2 rocks at once!)
            float runbackScore = 0f;
            int guardToRunback = -1;
            
            if (IsGuardBlocking(cenGuard, opponentRock))
            {
                guardToRunback = GetRockIndex(cenGuard);
                runbackScore = SimulateRunback(cenGuard.gameObject, opponentRock, guardToRunback, opponentRockIndex, rockCurrent);
                runbackScore += aggressionBonus + 20f; // HUGE bonus for removing 2 rocks!
                Debug.Log($"  Option 2: Runback (center) - Score: {runbackScore:F2} - REMOVE TWO ROCKS!");
            }
            else if (IsGuardBlocking(lCornGuard, opponentRock))
            {
                guardToRunback = GetRockIndex(lCornGuard);
                runbackScore = SimulateRunback(lCornGuard.gameObject, opponentRock, guardToRunback, opponentRockIndex, rockCurrent);
                runbackScore += aggressionBonus + 20f;
                Debug.Log($"  Option 2: Runback (left) - Score: {runbackScore:F2} - REMOVE TWO ROCKS!");
            }
            else if (IsGuardBlocking(rCornGuard, opponentRock))
            {
                guardToRunback = GetRockIndex(rCornGuard);
                runbackScore = SimulateRunback(rCornGuard.gameObject, opponentRock, guardToRunback, opponentRockIndex, rockCurrent);
                runbackScore += aggressionBonus + 20f;
                Debug.Log($"  Option 2: Runback (right) - Score: {runbackScore:F2} - REMOVE TWO ROCKS!");
            }
            
            // OPTION 3: Raise our rock into theirs (creative desperation!)
            float raiseScore = 0f;
            int rockToRaise = FindBestRaiseTarget(opponentRock, rockCurrent);
            if (rockToRaise >= 0)
            {
                raiseScore = SimulateRaise(opponentRock, rockToRaise, rockCurrent) + aggressionBonus * 0.5f;
                Debug.Log($"  Option 3: Raise rock #{rockToRaise} - Score: {raiseScore:F2}");
            }
            
            // PICK BEST REMOVAL
            float bestRemovalScore = Mathf.Max(takeoutScore, runbackScore, raiseScore);
            
            if (runbackScore == bestRemovalScore && runbackScore > 0f)
            {
                Debug.Log($"[Desperation] ✓ SELECTED: Runback (score: {runbackScore:F2}) - GO BIG!");
                OnTarget("Runback", rockCurrent, guardToRunback);
                return;
            }
            else if (takeoutScore == bestRemovalScore && takeoutScore > 0f)
            {
                Debug.Log($"[Desperation] ✓ SELECTED: Aggressive Takeout (score: {takeoutScore:F2})");
                OnTarget("Take Out", rockCurrent, opponentRockIndex);
                return;
            }
            else if (raiseScore == bestRemovalScore && raiseScore > 0f)
            {
                Debug.Log($"[Desperation] ✓ SELECTED: Raise (score: {raiseScore:F2}) - Desperation play!");
                OnTarget("Tap Back", rockCurrent, rockToRaise);
                return;
            }
        }
        
        // PHASE 2: No opponent rocks - SCORE AGGRESSIVELY!
        Debug.Log($"[Desperation] PHASE 2: Clear house - SCORE MULTIPLE ROCKS!");
        
        // Count how many rocks we already have in house
        int myRocksInHouse = 0;
        foreach (var houseRock in gm.houseList)
        {
            if (houseRock.rockInfo.teamName == currentRockInfo.teamName)
                myRocksInHouse++;
        }
        
        int stillNeed = rocksNeeded - myRocksInHouse;
        Debug.Log($"  Current rocks: {myRocksInHouse}, Still need: {stillNeed}");
        
        // DESPERATION STRATEGY: Different approach based on rocks needed
        
        if (stillNeed >= 2)
        {
            // NEED MULTIPLE ROCKS: Go for creative plays!
            Debug.Log($"[Desperation] Need {stillNeed} more rocks - CREATIVE SCORING!");
            
            // OPTION 1: Raise a friendly rock closer
            float raiseScore = 0f;
            int rockToRaise = FindBestRockToRaiseForScoring(rockCurrent, out raiseScore);
            raiseScore += 30f; // Desperation bonus
            
            // OPTION 2: Tick opponent rock into house (steal their rock!)
            float tickScore = 0f;
            int rockToTick = FindBestRockToTickIntoHouse(rockCurrent, out tickScore);
            tickScore += 25f; // Desperation bonus for stealing
            
            // OPTION 3: Draw to button (always solid)
            float drawScore = SimulateDraw(button, rockCurrent);
            drawScore += 20f; // Moderate bonus
            
            // OPTION 4: Bury behind opponent guard (protected scoring)
            float buryScore = 0f;
            Vector2 buryTarget = FindBestBuryPositionBehindOpponentGuard(rockCurrent, out buryScore);
            buryScore += 35f; // Big bonus - protected AND scoring!
            
            float bestScore = Mathf.Max(raiseScore, tickScore, drawScore, buryScore);
            
            if (buryScore == bestScore && buryScore > 0f)
            {
                Debug.Log($"[Desperation] ✓ SELECTED: Bury behind guard (score: {buryScore:F2}) - PROTECTED SCORING!");
                StartCoroutine(DrawTarget(rockCurrent, buryTarget));
            }
            else if (raiseScore == bestScore && raiseScore > 0f)
            {
                Debug.Log($"[Desperation] ✓ SELECTED: Raise rock (score: {raiseScore:F2}) - PUSH FORWARD!");
                OnTarget("Tap Back", rockCurrent, rockToRaise);
            }
            else if (tickScore == bestScore && tickScore > 0f)
            {
                Debug.Log($"[Desperation] ✓ SELECTED: Tick (score: {tickScore:F2}) - STEAL THEIR ROCK!");
                OnTarget("Tick Shot", rockCurrent, rockToTick);
            }
            else
            {
                Debug.Log($"[Desperation] ✓ SELECTED: Draw to button (score: {drawScore:F2}) - AGGRESSIVE DRAW!");
                OnTarget("Auto Draw Four Foot", rockCurrent, 0);
            }
        }
        else if (stillNeed == 1)
        {
            // NEED ONE MORE ROCK: Be aggressive but smart
            Debug.Log($"[Desperation] Need 1 more rock - SMART AGGRESSION!");
            
            // Should we go for 2 to WIN instead of tie?
            bool goForTwo = ShouldGoForTwo(context, myRocksInHouse);
            
            if (goForTwo)
            {
                Debug.Log($"[Desperation] GOING FOR TWO - Trying to WIN, not tie!");
                
                // OPTION 1: Raise friendly rock (promote to shot rock)
                float raiseScore = 0f;
                int rockToRaise = FindBestRockToRaiseForScoring(rockCurrent, out raiseScore);
                raiseScore += 40f; // Big bonus
                
                // OPTION 2: Draw around guard (bury for 2 rocks)
                float buryScore = 0f;
                Vector2 buryTarget = FindBestBuryPositionBehindOpponentGuard(rockCurrent, out buryScore);
                buryScore += 45f; // Biggest bonus
                
                // OPTION 3: Freeze (steal shot rock, maybe get 2)
                float freezeScore = 0f;
                int rockToFreeze = FindBestFreezeTarget(rockCurrent, out freezeScore);
                freezeScore += 35f;
                
                float bestScore = Mathf.Max(raiseScore, buryScore, freezeScore);
                
                if (buryScore == bestScore && buryScore > 0f)
                {
                    Debug.Log($"[Desperation] ✓ SELECTED: Bury draw (score: {buryScore:F2}) - GO FOR TWO!");
                    StartCoroutine(DrawTarget(rockCurrent, buryTarget));
                }
                else if (raiseScore == bestScore && raiseScore > 0f)
                {
                    Debug.Log($"[Desperation] ✓ SELECTED: Raise (score: {raiseScore:F2}) - GO FOR TWO!");
                    OnTarget("Tap Back", rockCurrent, rockToRaise);
                }
                else if (freezeScore == bestScore && freezeScore > 0f)
                {
                    Debug.Log($"[Desperation] ✓ SELECTED: Freeze (score: {freezeScore:F2}) - STEAL SHOT ROCK!");
                    OnTarget("Freeze", rockCurrent, rockToFreeze);
                }
                else
                {
                    // Fallback: simple draw
                    Debug.Log($"[Desperation] Fallback: Draw to button");
                    OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                }
            }
            else
            {
                // Play it SAFE - just need 1 to tie
                Debug.Log($"[Desperation] Playing safe - just need 1 to tie!");
                
                // OPTION 1: Draw to button (safest)
                float drawScore = SimulateDraw(button, rockCurrent) + 30f;
                
                // OPTION 2: Freeze (still safe, might be better position)
                float freezeScore = 0f;
                int rockToFreeze = FindBestFreezeTarget(rockCurrent, out freezeScore);
                freezeScore += 25f;
                
                // OPTION 3: Protected draw (safest of all)
                float protectedScore = 0f;
                Vector2 protectedTarget = FindBestProtectedDrawPosition(rockCurrent, out protectedScore);
                protectedScore += 35f; // Bonus for safety
                
                float bestScore = Mathf.Max(drawScore, freezeScore, protectedScore);
                
                if (protectedScore == bestScore && protectedScore > 0f)
                {
                    Debug.Log($"[Desperation] ✓ SELECTED: Protected draw (score: {protectedScore:F2}) - SAFE TIE!");
                    StartCoroutine(DrawTarget(rockCurrent, protectedTarget));
                }
                else if (drawScore == bestScore)
                {
                    Debug.Log($"[Desperation] ✓ SELECTED: Draw to button (score: {drawScore:F2}) - SAFE TIE!");
                    OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                }
                else
                {
                    Debug.Log($"[Desperation] ✓ SELECTED: Freeze (score: {freezeScore:F2}) - SAFE TIE!");
                    OnTarget("Freeze", rockCurrent, rockToFreeze);
                }
            }
        }
        else
        {
            // Already have enough rocks! Just need to protect them
            Debug.Log($"[Desperation] Already have {myRocksInHouse} rocks - PROTECT THEM!");
            
            // Draw conservatively to add insurance
            float drawScore = SimulateDraw(button, rockCurrent);
            float protectedScore = 0f;
            Vector2 protectedTarget = FindBestProtectedDrawPosition(rockCurrent, out protectedScore);
            
            if (protectedScore > drawScore && protectedScore > 0f)
            {
                Debug.Log($"[Desperation] ✓ SELECTED: Protected draw - INSURANCE ROCK!");
                StartCoroutine(DrawTarget(rockCurrent, protectedTarget));
            }
            else
            {
                Debug.Log($"[Desperation] ✓ SELECTED: Draw to button - INSURANCE ROCK!");
                OnTarget("Auto Draw Four Foot", rockCurrent, 0);
            }
        }
    }
    
    /// <summary>
    /// Calculate how many rocks we need to tie/win
    /// Returns: Number of rocks needed (1 = need 1 to tie, 2 = need 2 to tie, etc.)
    /// </summary>
    private int CalculateRocksNeeded(string myTeamName)
    {
        int myRocksInHouse = 0;
        int opponentRocksInHouse = 0;
        
        foreach (var houseRock in gm.houseList)
        {
            if (houseRock.rockInfo.teamName == myTeamName)
                myRocksInHouse++;
            else
                opponentRocksInHouse++;
        }
        
        // If opponent has more rocks, we need to match them + 1 to win
        // Or just match them to tie
        int rocksNeeded = Mathf.Max(1, opponentRocksInHouse - myRocksInHouse + 1);
        
        Debug.Log($"[RocksNeeded] My rocks: {myRocksInHouse}, Opponent rocks: {opponentRocksInHouse}, Need: {rocksNeeded}");
        
        return rocksNeeded;
    }
    
    /// <summary>
    /// Decide if we should go for 2 rocks (to WIN) instead of 1 (to TIE)
    /// Factors: Aggression level, rocks remaining, game phase
    /// </summary>
    private bool ShouldGoForTwo(ShotContext context, int currentRocksInHouse)
    {
        // Calculate rocks remaining in game
        int rocksRemaining = 16 - gm.rockCurrent; // Total 16 rocks per end
        
        // If we're VERY late (last 2 rocks), go for the win!
        if (rocksRemaining <= 2)
        {
            Debug.Log($"[GoForTwo] LAST ROCK - GO FOR WIN!");
            return true;
        }
        
        // If we already have 1+ rocks, we have a cushion - try for win
        if (currentRocksInHouse >= 1)
        {
            Debug.Log($"[GoForTwo] Have {currentRocksInHouse} rocks already - TRY FOR WIN!");
            return true;
        }
        
        // If opponent has guards (they're playing defensive), be aggressive
        int opponentGuards = 0;
        Rock_Info currentRockInfo = gm.rockList[gm.rockCurrent].rockInfo;
        
        foreach (var guard in gm.gList)
        {
            if (guard.lastTransform != null)
            {
                Rock_Info guardInfo = guard.lastTransform.GetComponent<Rock_Info>();
                if (guardInfo != null && guardInfo.teamName != currentRockInfo.teamName)
                {
                    opponentGuards++;
                }
            }
        }
        
        if (opponentGuards >= 2)
        {
            Debug.Log($"[GoForTwo] Opponent has {opponentGuards} guards - USE THEM FOR WIN!");
            return true; // Use their guards to our advantage!
        }
        
        // If we have 3+ rocks remaining, we can afford to be aggressive
        if (rocksRemaining >= 4)
        {
            Debug.Log($"[GoForTwo] {rocksRemaining} rocks left - TRY FOR WIN!");
            return true;
        }
        
        // Default: play safe, just tie
        Debug.Log($"[GoForTwo] Playing safe - JUST TIE!");
        return false;
    }
    
    /// <summary>
    /// Evaluate ALL options for forcing a blank end - CLEAR THE HOUSE!
    /// Strategy: Remove every rock, shooter must roll out (angled hits preferred)
    /// Goal: Retain hammer for next end by keeping house empty
    /// </summary>
    private void EvaluateForceBlankOptions(ShotContext context, int rockCurrent)
    {
        Rock_Info currentRockInfo = gm.rockList[rockCurrent].rockInfo;
        
        Debug.Log($"[AI_Target] FORCE BLANK - Clear house to retain hammer!");
        
        // PHASE 1: Are there rocks in the house? REMOVE THEM WITH ROLLOUT!
        if (gm.houseList.Count > 0)
        {
            Debug.Log($"[Force Blank] PHASE 1: {gm.houseList.Count} rock(s) in house - CLEAR THEM!");
            
            GameObject targetRock = gm.houseList[0].rock; // Shot rock (closest)
            int targetRockIndex = gm.houseList[0].rockInfo.rockIndex;
            
            // OPTION 1: Angled takeout (45° hit for shooter rollout - PREFERRED!)
            // This is better than nose hit because shooter naturally rolls out
            float angledTakeoutScore = SimulateAngledTakeout(targetRock, targetRockIndex, rockCurrent);
            Debug.Log($"  Option 1: Angled Takeout (45° hit, rolls out) - Score: {angledTakeoutScore:F2}");
            
            // OPTION 2: Runback (removes 2 rocks + shooter rolls through - EXCELLENT!)
            float runbackScore = 0f;
            int guardToRunback = -1;
            
            if (IsGuardBlocking(cenGuard, targetRock))
            {
                guardToRunback = GetRockIndex(cenGuard);
                runbackScore = SimulateRunback(cenGuard.gameObject, targetRock, guardToRunback, targetRockIndex, rockCurrent);
                runbackScore += 30f; // HUGE bonus - removes 2 rocks AND rolls out!
                Debug.Log($"  Option 2: Runback (center guard) - Score: {runbackScore:F2} - REMOVES TWO + ROLLOUT!");
            }
            else if (IsGuardBlocking(lCornGuard, targetRock))
            {
                guardToRunback = GetRockIndex(lCornGuard);
                runbackScore = SimulateRunback(lCornGuard.gameObject, targetRock, guardToRunback, targetRockIndex, rockCurrent);
                runbackScore += 30f;
                Debug.Log($"  Option 2: Runback (left guard) - Score: {runbackScore:F2} - REMOVES TWO + ROLLOUT!");
            }
            else if (IsGuardBlocking(rCornGuard, targetRock))
            {
                guardToRunback = GetRockIndex(rCornGuard);
                runbackScore = SimulateRunback(rCornGuard.gameObject, targetRock, guardToRunback, targetRockIndex, rockCurrent);
                runbackScore += 30f;
                Debug.Log($"  Option 2: Runback (right guard) - Score: {runbackScore:F2} - REMOVES TWO + ROLLOUT!");
            }
            
            // OPTION 3: Direct takeout (ONLY if shooter will roll out)
            // Nose hits can stick - not ideal for blanking
            float directTakeoutScore = SimulateTakeout(targetRock, targetRockIndex, rockCurrent);
            directTakeoutScore *= 0.7f; // Penalty - shooter might stick in house
            Debug.Log($"  Option 3: Direct Takeout (nose hit, might stick) - Score: {directTakeoutScore:F2}");
            
            // OPTION 4: Tick it out (sideways hit = natural rollout)
            float tickScore = SimulateTick(targetRock, targetRockIndex, rockCurrent);
            tickScore += 15f; // Bonus - tick shots usually roll out
            Debug.Log($"  Option 4: Tick Shot (sideways, rolls out) - Score: {tickScore:F2}");
            
            // PICK BEST REMOVAL WITH ROLLOUT!
            float bestScore = Mathf.Max(angledTakeoutScore, runbackScore, directTakeoutScore, tickScore);
            
            if (runbackScore == bestScore && runbackScore > 0f)
            {
                Debug.Log($"[Force Blank] ✓ SELECTED: Runback (score: {runbackScore:F2}) - PERFECT BLANK!");
                OnTarget("Runback", rockCurrent, guardToRunback);
            }
            else if (angledTakeoutScore == bestScore && angledTakeoutScore > 0f)
            {
                Debug.Log($"[Force Blank] ✓ SELECTED: Angled Takeout (score: {angledTakeoutScore:F2}) - ROLLOUT!");
                OnTarget("Take Out", rockCurrent, targetRockIndex); // Same mechanics, just angled aim
            }
            else if (tickScore == bestScore && tickScore > 0f)
            {
                Debug.Log($"[Force Blank] ✓ SELECTED: Tick Shot (score: {tickScore:F2}) - SIDEWAYS ROLLOUT!");
                OnTarget("Tick Shot", rockCurrent, targetRockIndex);
            }
            else if (directTakeoutScore == bestScore && directTakeoutScore > 0f)
            {
                Debug.Log($"[Force Blank] ✓ SELECTED: Direct Takeout (score: {directTakeoutScore:F2}) - HEAVY WEIGHT!");
                OnTarget("Take Out", rockCurrent, targetRockIndex);
            }
            else
            {
                // Fallback: peel with heavy weight
                Debug.LogWarning("[Force Blank] No good rollout options, using peel...");
                OnTarget("Peel", rockCurrent, targetRockIndex);
            }
            
            return;
        }
        
        // PHASE 2: House is already clear! THROW IT AWAY!
        Debug.Log($"[Force Blank] PHASE 2: House clear - THROW AWAY to retain hammer!");
        
        // Throw to out-of-bounds positions to guarantee blank
        Vector2[] throwAwayTargets = new Vector2[]
        {
            new Vector2(-2.0f, 8.0f),  // Left out of bounds
            new Vector2(2.0f, 8.0f),   // Right out of bounds
            new Vector2(-1.5f, 9.0f),  // Far left corner
            new Vector2(1.5f, 9.0f),   // Far right corner
        };
        
        // Pick a random throw-away target for variety
        Vector2 throwAwayTarget = throwAwayTargets[Random.Range(0, throwAwayTargets.Length)];
        
        Debug.Log($"[Force Blank] ✓ SELECTED: Throw away to ({throwAwayTarget.x:F2}, {throwAwayTarget.y:F2}) - GUARANTEE BLANK!");
        
        // Try physics-based throw away
        Vector2 pullbackPos;
        bool useInTurn;
        bool foundShot = CalculatePhysicsBasedDrawShot(throwAwayTarget, out pullbackPos, out useInTurn);
        
        if (foundShot)
        {
            rm.inturn = useInTurn;
            takeOutX = pullbackPos.x;
            takeOutY = pullbackPos.y;
            
            Debug.Log($"[Force Blank] Throw away physics: Pullback=({takeOutX:F3}, {takeOutY:F3}), InTurn={useInTurn}");
        }
        else
        {
            // Fallback: Just throw hard and wide
            Debug.LogWarning("[Force Blank] Physics failed, using fallback throw away");
            
            rm.inturn = (throwAwayTarget.x < 0f); // In-turn if going left
            takeOutX = throwAwayTarget.x * 0.08f; // Approximate lateral offset
            takeOutY = -27.0f; // Very light weight (will sail past house)
        }
        
        aiShoot.OnShot("Draw To Target", rockCurrent);
    }
    
    /// <summary>
    /// Simulate an angled takeout (45° hit) for better shooter rollout
    /// This is better than nose hits for blanking because shooter naturally rolls out
    /// Returns quality score (0-100)
    /// </summary>
    private float SimulateAngledTakeout(GameObject targetRock, int targetRockIndex, int rockCurrent)
    {
        Vector2 targetPos = targetRock.transform.position;
        Vector2 launcherPos = new Vector2(0f, -25f);
        
        // For angled hits, aim slightly to the SIDE of the target (not dead-on)
        // This creates a glancing blow that sends both rocks sideways
        float lateralOffset = 0.15f; // Aim 15cm to the side (about half a rock width)
        
        // Alternate sides based on target position for variety
        if (targetPos.x > 0f)
        {
            lateralOffset = -lateralOffset; // Target on right, hit from left side
        }
        
        Vector2 angledTarget = new Vector2(targetPos.x + lateralOffset, targetPos.y);
        
        Vector2 pullbackPos;
        bool useInTurn;
        bool foundShot = CalculatePhysicsBasedShot(angledTarget, out pullbackPos, out useInTurn, "Take Out", targetRockIndex);
        
        if (foundShot)
        {
            // Angled hits are PERFECT for blanking
            // - Target rock deflects sideways (out of house)
            // - Shooter continues forward (rolls out back)
            // - Cleaner than nose hits (which can stick)
            
            Debug.Log($"[Angled Takeout] Target: {targetPos}, Angled aim: {angledTarget} (offset: {lateralOffset:F3})");
            Debug.Log($"[Angled Takeout] Physics found 45° hit - shooter will roll out!");
            
            return 70f; // Excellent option for blanking
        }
        
        Debug.Log($"[Angled Takeout] No physics solution found for angled hit");
        return 0f;
    }
    
    /// <summary>
    /// Simulate a direct draw to button - returns quality score (0-100)
    /// </summary>
    private float SimulateDraw(Vector2 targetPosition, int rockCurrent)
    {
        Vector2 launcherPos = new Vector2(0f, -25f);
        
        // Base score: how close can we get to button with a clean draw?
        float baseScore = 70f; // Drawing is always a solid option
        
        // PENALTY: If opponent already has rocks closer to button
        float closestOpponentDist = float.MaxValue;
        Rock_Info currentRockInfo = gm.rockList[rockCurrent].rockInfo;
        
        foreach (var houseRock in gm.houseList)
        {
            if (houseRock.rockInfo.teamName != currentRockInfo.teamName)
            {
                float dist = Vector2.Distance(houseRock.rock.transform.position, targetPosition);
                if (dist < closestOpponentDist)
                {
                    closestOpponentDist = dist;
                }
            }
        }
        
        // If opponent is close to button, drawing is less valuable (hard to beat them)
        if (closestOpponentDist < 0.3f) // Within button radius
        {
            baseScore -= 20f; // Harder to beat close rocks
        }
        else if (closestOpponentDist < 0.6f) // Within 4-foot
        {
            baseScore -= 10f; // Moderately harder
        }
        
        // BONUS: If we have guards protecting this lane
        if (gm.gList.Count > 0)
        {
            foreach (Guard_List guard in gm.gList)
            {
                if (guard.lastTransform != null)
                {
                    Vector2 guardPos = guard.lastTransform.position;
                    // Check if guard is roughly in line with target (within 0.2 units laterally)
                    if (Mathf.Abs(targetPosition.x - guardPos.x) < 0.2f)
                    {
                        baseScore += 15f; // Guard is protecting this draw lane
                        Debug.Log($"[Simulate Draw] Guard at ({guardPos.x:F2}, {guardPos.y:F2}) is protecting the draw lane! Bonus +15");
                        break; // Only need one guard to provide protection
                    }
                }
            }
            // Rock is exposed, not behind any guards
            baseScore -= 30f;
        }
        
        return Mathf.Max(0f, baseScore);
    }
    
    /// <summary>
    /// Find the best friendly GUARD to raise into scoring position
    /// A raise shot hits a friendly guard with lighter weight, pushing it into the house
    /// while the shooter stops where the guard was (nose hit mechanics)
    /// Returns guard rock index and score via out parameter
    /// </summary>
    private int FindBestRockToRaiseForScoring(int currentRockIndex, out float bestScore)
    {
        Rock_Info currentRockInfo = gm.rockList[currentRockIndex].rockInfo;
        Vector2 button = new Vector2(0f, 6.5f);
        Vector2 launcherPos = new Vector2(0f, -25f);
        
        int bestRock = -1;
        bestScore = 0f;
        
        // Look through GUARDS (not house rocks!) - rocks in the guard zone (Y < 5.0)
        for (int i = 0; i < gm.rockList.Count; i++)
        {
            var rockEntry = gm.rockList[i];
            
            if (rockEntry.rock == null || !rockEntry.rock.activeInHierarchy || !rockEntry.rockInfo.inPlay)
                continue;
            
            // Must be my team's rock
            if (rockEntry.rockInfo.teamName != currentRockInfo.teamName)
                continue;
            
            Vector2 guardPos = rockEntry.rock.transform.position;
            
            // CRITICAL: Must be a GUARD (outside the house, in front of hog line)
            // Guard zone: Y between 2.0 (hog line) and 5.0 (top of house)
            if (guardPos.y < 2.0f || guardPos.y > 5.0f)
                continue; // Not in guard zone
            
            // Calculate where guard would end up if raised
            // Raise mechanics: Guard moves ~1.5-2.5 units forward from light hit
            float estimatedPushDistance = 2.0f; // Average push from raise shot
            Vector2 estimatedFinalPos = guardPos + new Vector2(0f, estimatedPushDistance);
            
            // Must end up INSIDE the house (Y > 5.0)
            if (estimatedFinalPos.y < 5.0f)
                continue; // Won't reach house
            
            // Score based on:
            // 1. How close final position would be to button (scoring value)
            // 2. How well-aligned guard is with launcher (easier nose hit)
            // 3. Current distance from house (closer = easier raise)
            
            float finalDistToButton = Vector2.Distance(estimatedFinalPos, button);
            float proximityScore = 1.0f - Mathf.Clamp01(finalDistToButton / 2.0f); // Closer to button = better
            
            // Alignment: Guard should be roughly in line with launcher (X close to 0)
            float lateralOffset = Mathf.Abs(guardPos.x);
            float alignmentScore = 1.0f - Mathf.Clamp01(lateralOffset / 1.0f); // Centered = easier
            
            // Distance to house edge: Closer guards need less push
            float distToHouseEdge = 5.0f - guardPos.y;
            float distanceScore = 1.0f - Mathf.Clamp01(distToHouseEdge / 3.0f); // Closer = easier
            
            // BONUS: If final position would be in 4-foot or 8-foot
            float scoringBonus = 0f;
            if (finalDistToButton < 0.6f) scoringBonus = 20f; // Would be in 4-foot!
            else if (finalDistToButton < 1.2f) scoringBonus = 10f; // Would be in 8-foot
            
            float score = (proximityScore * 40f) + (alignmentScore * 30f) + (distanceScore * 20f) + scoringBonus + 10f; // Base 10 for using raise
            
            Debug.Log($"[Raise for Score] GUARD #{i} at ({guardPos.x:F2}, {guardPos.y:F2}):\n" +
                      $"  Estimated final: ({estimatedFinalPos.x:F2}, {estimatedFinalPos.y:F2})\n" +
                      $"  Dist to button: {finalDistToButton:F2}\n" +
                      $"  Alignment: {alignmentScore:F2} (lateral: {lateralOffset:F2})\n" +
                      $"  Distance: {distanceScore:F2} (to house: {distToHouseEdge:F2})\n" +
                      $"  Scoring bonus: {scoringBonus:F1}\n" +
                      $"  TOTAL Score: {score:F1}/100");
            
            if (score > bestScore)
            {
                bestScore = score;
                bestRock = i;
            }
        }
        
        if (bestRock >= 0)
        {
            Debug.Log($"[Raise for Score] ✓ BEST: Guard #{bestRock} with score {bestScore:F1}/100 - Will raise into house!");
        }
        else
        {
            Debug.Log($"[Raise for Score] ✗ NO friendly guards available to raise");
        }
        
        return bestRock;
    }
    
    /// <summary>
    /// Find the best rock to tick into the house for scoring
    /// Looks for rocks just outside the house that could be nudged in
    /// Returns rock index and score via out parameter
    /// </summary>
    private int FindBestRockToTickIntoHouse(int currentRockIndex, out float bestScore)
    {
        Rock_Info currentRockInfo = gm.rockList[currentRockIndex].rockInfo;
        Vector2 button = new Vector2(0f, 6.5f);
        float houseRadius = 1.83f; // 12-foot circle
        
        int bestRock = -1;
        bestScore = 0f;
        
        // Look through ALL active rocks (not just house list)
        for (int i = 0; i < gm.rockList.Count; i++)
        {
            var rockEntry = gm.rockList[i];
            
            if (rockEntry.rock == null || !rockEntry.rock.activeInHierarchy || !rockEntry.rockInfo.inPlay)
                continue;
            
            // Skip our own rocks (we want to steal opponent's rocks!)
            if (rockEntry.rockInfo.teamName == currentRockInfo.teamName)
                continue;
            
            Vector2 rockPos = rockEntry.rock.transform.position;
            float distToButton = Vector2.Distance(rockPos, button);
            
            // Must be JUST OUTSIDE house (within tick range)
            if (distToButton < houseRadius || distToButton > houseRadius + 0.8f)
                continue;
            
            // Must be at an angle we can tick (near edge of sheet)
            if (Mathf.Abs(rockPos.x) < 0.5f)
                continue; // Too centered, can't tick effectively
            
            // Score based on:
            // 1. How close to house edge (easier to tick in)
            // 2. Angle for tick (rocks on edges are easier)
            // 3. Potential final position if ticked in
            
            float distanceFromHouseEdge = distToButton - houseRadius;
            float edgeProximityScore = 1.0f - Mathf.Clamp01(distanceFromHouseEdge / 0.8f);
            
            float lateralPosition = Mathf.Abs(rockPos.x);
            float angleScore = Mathf.Clamp01((lateralPosition - 0.5f) / 1.0f); // Better at edges
            
            // Estimate where it would end up if ticked (rough approximation)
            Vector2 estimatedFinalPos = rockPos + new Vector2(rockPos.x > 0 ? -0.3f : 0.3f, 0.2f);
            float estimatedFinalDist = Vector2.Distance(estimatedFinalPos, button);
            float positionQualityScore = 1.0f - Mathf.Clamp01(estimatedFinalDist / 1.2f);
            
            float score = (edgeProximityScore * 40f) + (angleScore * 30f) + (positionQualityScore * 30f);
            
            Debug.Log($"[Tick Into House] Rock #{i} at ({rockPos.x:F2}, {rockPos.y:F2}): " +
                      $"EdgeDist={distanceFromHouseEdge:F2}, Lateral={lateralPosition:F2}, Score={score:F1}/100");
            
            if (score > bestScore)
            {
                bestScore = score;
                bestRock = i;
            }
        }
        
        if (bestRock >= 0)
        {
            Debug.Log($"[Tick Into House] ✓ BEST: Rock #{bestRock} with score {bestScore:F1}/100");
        }
        
        return bestRock;
    }
    
    /// <summary>
    /// Find the best position to draw behind an OPPONENT'S guard to bury a scoring rock
    /// Opponent guards become OUR advantage - use them as protection!
    /// Returns target position and score via out parameter
    /// </summary>
    private Vector2 FindBestBuryPositionBehindOpponentGuard(int currentRockIndex, out float bestScore)
    {
        Rock_Info currentRockInfo = gm.rockList[currentRockIndex].rockInfo;
        Vector2 button = new Vector2(0f, 6.5f);
        
        Vector2 bestPosition = Vector2.zero;
        bestScore = 0f;
        
        if (gm.gList.Count == 0)
            return bestPosition;
        
        // Look through all guards - we want OPPONENT guards!
        foreach (var guard in gm.gList)
        {
            if (guard.lastTransform == null)
                continue;
            
            Rock_Info guardInfo = guard.lastTransform.GetComponent<Rock_Info>();
            if (guardInfo == null)
                continue;
            
            // CRITICAL: Use OPPONENT'S guards as protection (not our own!)
            // Their guards become OUR advantage when scoring
            if (guardInfo.teamName == currentRockInfo.teamName)
                continue; // Skip our own guards (handled by Option 6)
            
            Vector2 guardPos = guard.lastTransform.position;
            
            // Calculate ideal bury position: behind guard, close to button
            // We want to "hide" behind their guard so they can't easily remove us
            Vector2 guardToButton = button - guardPos;
            Vector2 buryPos = guardPos + guardToButton * 0.7f; // 70% toward button = deeper bury
            
            // Score based on:
            // 1. How close final position is to button (scoring value)
            // 2. How well-protected the position is (harder for opponent to remove)
            // 3. Guard positioning (center guards = better protection)
            
            float distToButton = Vector2.Distance(buryPos, button);
            float proximityScore = 1.0f - Mathf.Clamp01(distToButton / 1.5f); // Closer to button = better
            
            // Protection quality: guard should be in front (lower Y) and close enough to block
            float guardToBuryDist = Vector2.Distance(guardPos, buryPos);
            bool wellProtected = guardPos.y < buryPos.y && guardToBuryDist > 0.5f && guardToBuryDist < 2.5f;
            float protectionScore = wellProtected ? 1.0f : 0.2f;
            
            // Guard positioning: center guards = better protection (block more angles)
            float guardCenteredness = 1.0f - Mathf.Clamp01(Mathf.Abs(guardPos.x) / 1.0f);
            float guardPositionScore = guardCenteredness * Mathf.Clamp01((guardPos.y - 2.0f) / 3.0f);
            
            // BONUS: If bury position is DEEP (Y > 6.0), extra points for scoring threat
            float deepnessBonus = 0f;
            if (buryPos.y > 6.0f) deepnessBonus = 20f;
            
            float score = (proximityScore * 35f) + (protectionScore * 35f) + (guardPositionScore * 20f) + deepnessBonus + 10f; // +10 base for using opponent guard
            
            Debug.Log($"[Bury Draw] Behind OPPONENT guard at ({guardPos.x:F2}, {guardPos.y:F2}) → position ({buryPos.x:F2}, {buryPos.y:F2}): " +
                      $"DistToButton={distToButton:F2}, Protected={wellProtected}, GuardCenter={guardCenteredness:F2}, " +
                      $"Deepness={buryPos.y:F2}, Score={score:F1}/100");
            
            if (score > bestScore)
            {
                bestScore = score;
                bestPosition = buryPos;
            }
        }
        
        if (bestScore > 0f)
        {
            Debug.Log($"[Bury Draw] ✓ BEST: Position ({bestPosition.x:F2}, {bestPosition.y:F2}) with score {bestScore:F1}/100 - Using OPPONENT guard as protection!");
        }
        
        return bestPosition;
    }
    
    /// <summary>
    /// Find the best position to draw behind an existing guard for protection
    /// Returns target position and score via out parameter
    /// </summary>
    private Vector2 FindBestProtectedDrawPosition(int currentRockIndex, out float bestScore)
    {
        Rock_Info currentRockInfo = gm.rockList[currentRockIndex].rockInfo;
        Vector2 button = new Vector2(0f, 6.5f);
        
        Vector2 bestPosition = Vector2.zero;
        bestScore = 0f;
        
        if (gm.gList.Count == 0)
            return bestPosition;
        
        // Look through all guards
        foreach (var guard in gm.gList)
        {
            if (guard.lastTransform == null)
                continue;
            
            Rock_Info guardInfo = guard.lastTransform.GetComponent<Rock_Info>();
            if (guardInfo == null)
                continue;
            
            Vector2 guardPos = guard.lastTransform.position;

            // Calculate ideal protected position: behind guard, toward button
            // Position should be roughly 60% of the way from guard to button
            float guardToButton = button.y - guardPos.y;
            float drawHeight = 6.5f;
            if (guardToButton < 3f)
                drawHeight += 3f - guardToButton;
            Vector2 protectedPos = new(guardPos.x, drawHeight);
            
            // Score based on:
            // 1. How close final position is to button
            // 2. How well-protected the position is
            // 3. Whether guard is well-positioned
            
            float distToButton = Vector2.Distance(protectedPos, button);
            float proximityScore = 1.0f - Mathf.Clamp01(distToButton / 1.5f);
            
            // Protection quality: guard should be in front (lower Y) and not too far
            float guardToProtectedDist = Vector2.Distance(guardPos, protectedPos);
            bool wellProtected = guardPos.y < protectedPos.y && guardToProtectedDist > 0.5f && guardToProtectedDist < 2.0f;
            float protectionScore = wellProtected ? 1.0f : 0.3f;
            
            // Guard positioning: closer to house edge is better (more blocking)
            float guardPositionScore = Mathf.Clamp01((guardPos.y - 2.0f) / 3.0f);
            
            float score = (proximityScore * 40f) + (protectionScore * 40f) + (guardPositionScore * 20f);
            
            Debug.Log($"[Protected Draw] Behind guard at ({guardPos.x:F2}, {guardPos.y:F2}) → position ({protectedPos.x:F2}, {protectedPos.y:F2}): " +
                      $"DistToButton={distToButton:F2}, Protected={wellProtected}, Score={score:F1}/100");
            
            if (score > bestScore)
            {
                bestScore = score;
                bestPosition = protectedPos;
            }
        }
        
        if (bestScore > 0f)
        {
            Debug.Log($"[Protected Draw] ✓ BEST: Position ({bestPosition.x:F2}, {bestPosition.y:F2}) with score {bestScore:F1}/100");
        }
        
        return bestPosition;
    }
    
    /// <summary>
    /// Simulate a direct takeout - returns quality score (0-100)
    /// NOW WITH MULTI-ROCK DETECTION: Bonus score if takeout causes secondary collisions!
    /// </summary>
    private float SimulateTakeout(GameObject targetRock, int targetRockIndex, int rockCurrent)
    {
        Vector2 targetPos = targetRock.transform.position;
        Vector2 launcherPos = new Vector2(0f, -25f);
        Rock_Info currentRockInfo = gm.rockList[rockCurrent].rockInfo;
        
        Vector2 pullbackPos;
        bool useInTurn;
        bool foundShot = CalculatePhysicsBasedShot(targetPos, out pullbackPos, out useInTurn, "Take Out", targetRockIndex);
        
        if (foundShot)
        {
            // Base score: Successfully found a takeout shot
            float baseScore = 60f;
            
            // MULTI-ROCK DETECTION: Check if hit rock will collide with other rocks!
            // Get the post-collision path of the HIT rock (where it goes after being struck)
            TrajectorySimulator.CollisionInfo collisionInfo = trajectorySimulator.GetCollisionInfo();
            
            if (collisionInfo.hasCollision && collisionInfo.hitRockPostCollisionPath != null && collisionInfo.hitRockPostCollisionPath.Count > 0)
            {
                List<Vector2> hitRockPath = collisionInfo.hitRockPostCollisionPath;
                float rockRadius = 0.14f; // Standard rock radius
                
                // Get all OTHER rocks that might be in the way
                List<GameObject> otherRocksInPlay = new List<GameObject>();
                
                for (int i = 0; i < gm.rockList.Count; i++)
                {
                    var rockEntry = gm.rockList[i];
                    
                    // Skip if not in play
                    if (rockEntry.rock == null || !rockEntry.rock.activeInHierarchy || !rockEntry.rockInfo.inPlay)
                        continue;
                    
                    // Skip the PRIMARY target rock (already hitting it)
                    if (i == targetRockIndex)
                        continue;
                    
                    // Skip the SHOOTER rock (it's the one doing the hitting)
                    if (i == rockCurrent)
                        continue;
                    
                    // This is a potential SECONDARY target!
                    otherRocksInPlay.Add(rockEntry.rock);
                }
                
                Debug.Log($"[Multi-Rock Takeout] Checking {otherRocksInPlay.Count} potential secondary targets for hit rock path with {hitRockPath.Count} points");
                
                // Check if hit rock's path gets close to any other rocks
                int secondaryHits = 0;
                int opponentSecondaryHits = 0;
                float totalChaos = 0f;
                
                foreach (GameObject secondaryRock in otherRocksInPlay)
                {
                    Vector2 secondaryPos = secondaryRock.transform.position;
                    Rock_Info secondaryInfo = secondaryRock.GetComponent<Rock_Info>();
                    
                    // Find closest approach distance along hit rock's path
                    float closestDist = float.MaxValue;
                    
                    foreach (Vector2 pathPoint in hitRockPath)
                    {
                        float dist = Vector2.Distance(pathPoint, secondaryPos);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                        }
                    }
                    
                    // Collision threshold: 2 rock radii = ~0.28 units
                    // Use generous threshold since simulation might not be pixel-perfect
                    float collisionThreshold = rockRadius * 2.5f; // ~0.35 units
                    
                    if (closestDist < collisionThreshold)
                    {
                        secondaryHits++;
                        
                        bool isOpponentRock = (secondaryInfo != null && secondaryInfo.teamName != currentRockInfo.teamName);
                        
                        if (isOpponentRock)
                        {
                            opponentSecondaryHits++;
                            totalChaos += 25f; // BIG BONUS: Hit opponent's rock!
                            Debug.Log($"[Multi-Rock] ✓ SECONDARY HIT (opponent): {secondaryRock.name} at dist {closestDist:F3} - CHAOS +25!");
                        }
                        else
                        {
                            totalChaos += 10f; // Smaller bonus: Hit our own rock (still disrupts ice)
                            Debug.Log($"[Multi-Rock] ⚠️ SECONDARY HIT (friendly): {secondaryRock.name} at dist {closestDist:F3} - chaos +10");
                        }
                    }
                }
                
                // MULTI-ROCK CHAOS MULTIPLIER!
                if (secondaryHits >= 3)
                {
                    totalChaos += 30f; // HUGE BONUS: 3+ rocks affected (CHAOS MADNESS!)
                    Debug.Log($"[Multi-Rock] 🎯 CHAOS MADNESS! {secondaryHits} secondary hits - BONUS +30!");
                }
                else if (secondaryHits >= 2)
                {
                    totalChaos += 20f; // Big bonus: 2+ rocks affected
                    Debug.Log($"[Multi-Rock] 🎯 MULTI-ROCK CHAOS! {secondaryHits} secondary hits - BONUS +20!");
                }
                
                // Apply chaos bonus to base score
                float finalScore = baseScore + totalChaos;
                
                if (secondaryHits > 0)
                {
                    Debug.Log($"[Multi-Rock Takeout] PRIMARY hit + {secondaryHits} SECONDARY hits ({opponentSecondaryHits} opponent) → TOTAL SCORE: {finalScore:F1}/100 (base: {baseScore:F1}, chaos: +{totalChaos:F1})");
                }
                
                return Mathf.Min(100f, finalScore); // Cap at 100
            }
            
            // No secondary collisions detected - return base score
            return baseScore;
        }
        
        return 0f; // Can't find a clear shot
    }
    
    /// <summary>
    /// Simulate peeling a guard - returns quality score
    /// </summary>
    private float SimulatePeel(GameObject guardRock, int guardIndex, int rockCurrent)
    {
        if (guardRock == null) return 0f;
        
        Vector2 guardPos = guardRock.transform.position;
        Vector2 pullbackPos;
        bool useInTurn;
        bool foundShot = CalculatePhysicsBasedShot(guardPos, out pullbackPos, out useInTurn, "Peel", guardIndex);
        
        if (foundShot)
        {
            // Peels are harder than takeouts (need more weight)
            return 50f; // Moderate option
        }
        
        return 0f;
    }
    
    /// <summary>
    /// Simulate raising a friendly rock into the threat - returns quality score
    /// </summary>
    private float SimulateRaise(GameObject targetRock, int rockToRaise, int rockCurrent)
    {
        if (rockToRaise < 0) return 0f;
        
        GameObject friendlyRock = gm.rockList[rockToRaise].rock;
        if (friendlyRock == null || !friendlyRock.activeInHierarchy) return 0f;
        
        Vector2 friendlyPos = friendlyRock.transform.position;
        Vector2 pullbackPos;
        bool useInTurn;
        bool foundShot = CalculatePhysicsBasedShot(friendlyPos, out pullbackPos, out useInTurn, "Tap Back", rockToRaise);
        
        if (foundShot)
        {
            // Check if friendly rock is BETWEEN us and threat
            Vector2 targetPos = targetRock.transform.position;
            bool isGoodAngle = (friendlyPos.y > -20f && friendlyPos.y < targetPos.y);
            
            if (isGoodAngle)
            {
                return 40f; // Creative option, but risky
            }
        }
        
        return 0f;
    }
    
    /// <summary>
    /// Simulate a tick shot - returns quality score
    /// </summary>
    private float SimulateTick(GameObject targetRock, int targetIndex, int rockCurrent)
    {
        Vector2 targetPos = targetRock.transform.position;
        
        // Tick shots only work in specific situations (rock near edge of house)
        bool nearEdge = Mathf.Abs(targetPos.x) > 0.8f;
        
        if (nearEdge)
        {
            Vector2 pullbackPos;
            bool useInTurn;
            bool foundShot = CalculatePhysicsBasedShot(targetPos, out pullbackPos, out useInTurn, "Tick", targetIndex);
            
            if (foundShot)
            {
                return 45f; // Decent option for edge rocks
            }
        }
        
        return 0f; // Not a good tick situation
    }
    
    /// <summary>
    /// Simulate a runback - hit guard through to target rock behind it
    /// This is an advanced shot requiring good alignment and extra velocity
    /// </summary>
    private float SimulateRunback(GameObject guardRock, GameObject targetRock, int guardIndex, int targetIndex, int rockCurrent)
    {
        if (guardRock == null || targetRock == null) return 0f;
        
        Vector2 guardPos = guardRock.transform.position;
        Vector2 targetPos = targetRock.transform.position;
        Vector2 launcherPos = new Vector2(0f, -25f);
        
        // CRITICAL: Check alignment - guard must be BETWEEN launcher and target
        // If they're not well-aligned, runback won't work
        float alignmentQuality = CheckRunbackAlignment(launcherPos, guardPos, targetPos);
        
        Debug.Log($"[AI_Target] Runback alignment check: launcher={launcherPos}, guard={guardPos}, target={targetPos}, quality={alignmentQuality:F2}");
        
        if (alignmentQuality < 0.6f) // Need good alignment (60%+ quality)
        {
            Debug.Log($"[AI_Target] Runback rejected - poor alignment ({alignmentQuality:F2} < 0.6)");
            return 0f;
        }
        
        // Check distance - runback works best when guard is not too close to target
        float guardToTargetDist = Vector2.Distance(guardPos, targetPos);
        if (guardToTargetDist < 0.5f || guardToTargetDist > 3.0f)
        {
            Debug.Log($"[AI_Target] Runback rejected - distance ({guardToTargetDist:F2}) outside optimal range (0.5-3.0)");
            return 0f; // Too close or too far
        }
        
        // Try physics shot with runback velocity (needs drive-through power)
        Vector2 pullbackPos;
        bool useInTurn;
        bool foundShot = CalculatePhysicsBasedShot(guardPos, out pullbackPos, out useInTurn, "Runback", guardIndex);
        
        if (foundShot)
        {
            // Score based on alignment quality and distance
            float distanceScore = 1.0f - Mathf.Clamp01((guardToTargetDist - 0.5f) / 2.5f); // Closer = better
            float totalScore = 55f * alignmentQuality * distanceScore;
            
            Debug.Log($"[AI_Target] Runback viable! Alignment={alignmentQuality:F2}, Distance={guardToTargetDist:F2}, Score={totalScore:F2}");
            return totalScore; // Good option if well-aligned
        }
        
        Debug.Log($"[AI_Target] Runback physics shot failed");
        return 0f;
    }
    
    /// <summary>
    /// Check if launcher-guard-target are well-aligned for a runback shot
    /// Returns 0-1 quality (1 = perfect alignment, 0 = perpendicular)
    /// </summary>
    private float CheckRunbackAlignment(Vector2 launcher, Vector2 guard, Vector2 target)
    {
        // Vector from launcher through guard
        Vector2 launcherToGuard = (guard - launcher).normalized;
        
        // Vector from guard to target
        Vector2 guardToTarget = (target - guard).normalized;
        
        // Dot product = 1.0 means perfect alignment (same direction)
        // Dot product = 0.0 means perpendicular (can't runback)
        // Dot product = -1.0 means opposite direction (target is behind launcher)
        float alignment = Vector2.Dot(launcherToGuard, guardToTarget);
        
        // Convert to 0-1 scale where:
        // - 0.8+ dot product = 1.0 quality (excellent alignment)
        // - 0.6 dot product = 0.5 quality (acceptable)
        // - 0.4 dot product = 0.0 quality (too angled)
        float quality = Mathf.Clamp01((alignment - 0.4f) / 0.4f);
        
        Debug.Log($"[AI_Target] Alignment vectors: L→G={launcherToGuard}, G→T={guardToTarget}, dot={alignment:F3}, quality={quality:F2}");
        
        return quality;
    }

    /// <summary>
    /// Find the best friendly rock to raise into a threat
    /// Returns rock index, or -1 if none found
    /// </summary>
    private int FindBestRaiseTarget(GameObject threatRock, int currentRockIndex)
    {
        Rock_Info currentRockInfo = gm.rockList[currentRockIndex].rockInfo;
        Vector2 threatPos = threatRock.transform.position;

        int bestRock = -1;
        float bestScore = float.MinValue;

        // Look through all rocks in house
        foreach (var houseRock in gm.houseList)
        {
            // Must be my team's rock
            if (houseRock.rockInfo.teamName != currentRockInfo.teamName)
                continue;

            // Must be in front of threat (can raise it forward)
            Vector2 rockPos = houseRock.rock.transform.position;
            if (rockPos.y >= threatPos.y)
                continue;

            // Score based on: closer to threat = better, more aligned = better
            float distToThreat = Vector2.Distance(rockPos, threatPos);
            float alignment = Mathf.Abs(rockPos.x - threatPos.x); // Lower = more aligned

            float score = 10f / distToThreat - alignment * 2f;

            if (score > bestScore)
            {
                bestScore = score;
                bestRock = houseRock.rockInfo.rockIndex;
            }
        }

        return bestRock;
    }

    /// <summary>
    /// Find the best opponent rock to freeze on (draw to its edge to be shot rock)
    /// Ideal target: Just behind button (Y > 6.5), close enough to draw beside it
    /// Returns rock index, or -1 if none found. Score is returned via out parameter.
    /// </summary>
    private int FindBestFreezeTarget(int currentRockIndex, out float bestScore)
    {
        Rock_Info currentRockInfo = gm.rockList[currentRockIndex].rockInfo;
        Vector2 button = new Vector2(0f, 6.5f);

        int bestRock = -1;
        bestScore = 0f; // Initialize out parameter - must earn positive score

        // Look through all rocks in house
        foreach (var houseRock in gm.houseList)
        {
            // Must be OPPONENT's rock (we freeze on their rocks to steal shot rock)
            if (houseRock.rockInfo.teamName == currentRockInfo.teamName)
                continue;

            Vector2 rockPos = houseRock.rock.transform.position;
            
            // CRITICAL: Must be BEHIND button (Y > 6.5) to freeze on
            float distBehindButton = rockPos.y - button.y;
            if (distBehindButton <= 0f)
                continue; // In front of or ON button - can't freeze effectively
            
            // Score components (all normalized to 0-100 range):
            
            // 1. BEHIND BUTTON QUALITY (60 points max)
            // Ideal: 0.15-0.6 units behind button (one rock diameter)
            // Too close: Hard to draw beside without hitting
            // Too far: Not threatening shot rock position
            float idealBehindDist = 0.15f; // Sweet spot: half a rock behind button
            float behindDeviation = Mathf.Abs(distBehindButton - idealBehindDist);
            float behindQuality = Mathf.Clamp01(1f - (behindDeviation / 0.6f)); // Within 0.6 units is acceptable
            float behindScore = behindQuality * 60f;
            
            // 2. LATERAL DISTANCE TO BUTTON (20 points max)
            // Closer to center = better (easier to draw beside and be shot rock)
            float lateralDist = Mathf.Abs(rockPos.x - button.x);
            float lateralQuality = Mathf.Clamp01(1f - (lateralDist / 1.2f)); // Within 1.2 units is acceptable
            float lateralScore = lateralQuality * 20f;
            
            // 3. TOTAL DISTANCE TO BUTTON (20 points max)
            // Closer overall = more valuable position
            float totalDist = Vector2.Distance(rockPos, button);
            float distQuality = Mathf.Clamp01(1f - (totalDist / 2.0f)); // Within 2 units is acceptable
            float distScore = distQuality * 20f;
            
            // TOTAL SCORE (0-100)
            float score = behindScore + lateralScore + distScore;
            
            Debug.Log($"[Freeze Target] Rock at ({rockPos.x:F2}, {rockPos.y:F2}): " +
                      $"Behind={distBehindButton:F2} (score:{behindScore:F1}), " +
                      $"Lateral={lateralDist:F2} (score:{lateralScore:F1}), " +
                      $"TotalDist={totalDist:F2} (score:{distScore:F1}) " +
                      $"→ TOTAL: {score:F1}/100");

            if (score > bestScore)
            {
                bestScore = score;
                bestRock = houseRock.rockInfo.rockIndex;
            }
        }
        
        if (bestRock >= 0)
        {
            Debug.Log($"[Freeze Target] ✓ BEST: Rock #{bestRock} with score {bestScore:F1}/100");
        }
        else
        {
            Debug.Log($"[Freeze Target] ✗ NO suitable freeze target found");
        }

        return bestRock;
    }
    /// <summary>
    /// Place a guard strategically based on comprehensive curling strategy
    /// 
    /// OPENING STRATEGY (no rocks in house):
    /// - WITHOUT HAMMER (aggressive): Center guards (X ≈ 0, Y = 3.0-3.5) to clutter button
    /// - WITH HAMMER (conservative): Corner guards (X = 0.7-0.85, Y = 1.5-4.5) to clear center
    /// 
    /// REACTIVE STRATEGY (rocks in house):
    /// - Protect unguarded friendly rock: Low guard matching X (Y = 3.0-4.5)
    /// - Guard shot rock (closest to button)
    /// - Counter opponent's guard placement
    /// - Late game: Tight guards (Y = 4.0-4.5)
    /// </summary>
    private void PlaceStrategicGuard(ShotContext context, int rockCurrent)
    {
        Rock_Info currentRockInfo = gm.rockList[rockCurrent].rockInfo;
        bool hasHammer = (rockCurrent % 2 == 1) ? gm.redHammer : !gm.redHammer;
        
        Vector2 guardTarget = Vector2.zero;
        string guardType = "Center Guard"; // For logging
        bool useOpeningStrategy = false;
        
        // ========================================
        // REACTIVE STRATEGY: Rocks already in house
        // ========================================
        if (gm.houseList.Count > 0)
        {
            Debug.Log($"[Strategic Guard] REACTIVE mode - {gm.houseList.Count} rocks in house");
            
            // PRIORITY 1: Find unguarded FRIENDLY rocks to protect
            GameObject unguardedFriendly = null;
            float closestDistToButton = 999f;
            
            foreach (var houseRock in gm.houseList)
            {
                // Must be our rock
                if (houseRock.rockInfo.teamName != currentRockInfo.teamName)
                    continue;
                
                Vector2 rockPos = houseRock.rock.transform.position;
                
                // Check if already guarded
                bool alreadyGuarded = false;
                foreach (var guard in gm.gList)
                {
                    if (guard.lastTransform == null) continue;
                    
                    Vector2 guardPos = guard.lastTransform.position;
                    float lateralDiff = Mathf.Abs(guardPos.x - rockPos.x);
                    bool inFront = guardPos.y < rockPos.y;
                    
                    if (lateralDiff < 0.4f && inFront) // Guard is blocking this rock
                    {
                        alreadyGuarded = true;
                        break;
                    }
                }
                
                if (!alreadyGuarded)
                {
                    float distToButton = Vector2.Distance(rockPos, new Vector2(0f, 6.5f));
                    
                    // Prefer guarding rocks closest to button (highest value)
                    if (distToButton < closestDistToButton)
                    {
                        closestDistToButton = distToButton;
                        unguardedFriendly = houseRock.rock;
                    }
                }
            }
            
            // GUARD UNGUARDED FRIENDLY ROCK
            if (unguardedFriendly != null)
            {
                Vector2 rockPos = unguardedFriendly.transform.position;
                
                // STRATEGY: Match X position, place in guard zone (Y = 3.0-4.5)
                // Closer rocks get LOWER guards (Y = 3.0-3.5), farther rocks get HIGHER guards (Y = 3.5-4.5)
                float distToButton = Vector2.Distance(rockPos, new Vector2(0f, 6.5f));
                float guardDepth = Mathf.Lerp(3.0f, 4.5f, Mathf.Clamp01((distToButton - 0.5f) / 1.5f));
                
                guardTarget = new Vector2(
                    rockPos.x, // Match X exactly (with slight randomness below)
                    guardDepth
                );
                
                guardType = $"Protective Guard (matching rock at X={rockPos.x:F2})";
                Debug.Log($"[Strategic Guard] PROTECT unguarded friendly at ({rockPos.x:F2}, {rockPos.y:F2}) → guard at ({guardTarget.x:F2}, {guardTarget.y:F2})");
            }
            
            // PRIORITY 2: If no unguarded friendlies, guard SHOT ROCK (if it's ours)
            else if (gm.houseList[0].rockInfo.teamName == currentRockInfo.teamName)
            {
                Vector2 shotRockPos = gm.houseList[0].rock.transform.position;
                
                // Check if shot rock is already guarded
                bool shotRockGuarded = false;
                foreach (var guard in gm.gList)
                {
                    if (guard.lastTransform == null) continue;
                    
                    Vector2 guardPos = guard.lastTransform.position;
                    float lateralDiff = Mathf.Abs(guardPos.x - shotRockPos.x);
                    bool inFront = guardPos.y < shotRockPos.y;
                    
                    if (lateralDiff < 0.4f && inFront)
                    {
                        shotRockGuarded = true;
                        break;
                    }
                }
                
                if (!shotRockGuarded)
                {
                    // GUARD THE SHOT ROCK - most valuable position!
                    guardTarget = new Vector2(
                        shotRockPos.x,
                        Random.Range(3.5f, 4.2f) // Tighter guard (closer to house)
                    );
                    
                    guardType = "Shot Rock Guard";
                    Debug.Log($"[Strategic Guard] PROTECT shot rock at ({shotRockPos.x:F2}, {shotRockPos.y:F2}) → guard at ({guardTarget.x:F2}, {guardTarget.y:F2})");
                }
                else
                {
                    // Shot rock already guarded - use opening strategy
                    useOpeningStrategy = true;
                }
            }
            
            // PRIORITY 3: Counter opponent's guards (block their draw lanes)
            else
            {
                Debug.Log($"[Strategic Guard] No friendly rocks to protect - checking opponent guards");
                
                // Find opponent's most dangerous guard (closest to house, best X position)
                Transform bestOpponentGuard = null;
                float bestOpponentGuardScore = 0f;
                
                foreach (var guard in gm.gList)
                {
                    if (guard.lastTransform == null) continue;
                    
                    Rock_Info guardInfo = guard.lastTransform.GetComponent<Rock_Info>();
                    if (guardInfo == null || guardInfo.teamName == currentRockInfo.teamName)
                        continue; // Skip our guards
                    
                    Vector2 guardPos = guard.lastTransform.position;
                    
                    // Score: Centered guards + closer to house = more dangerous
                    float centeredness = 1.0f - Mathf.Abs(guardPos.x) / 1.5f;
                    float proximity = Mathf.Clamp01((guardPos.y - 2.0f) / 3.0f);
                    float score = centeredness * 50f + proximity * 50f;
                    
                    if (score > bestOpponentGuardScore)
                    {
                        bestOpponentGuardScore = score;
                        bestOpponentGuard = guard.lastTransform;
                    }
                }
                
                if (bestOpponentGuard != null)
                {
                    // COUNTER-GUARD: Block their lane
                    Vector2 opponentGuardPos = bestOpponentGuard.position;
                    
                    guardTarget = new Vector2(
                        opponentGuardPos.x, // Match their X
                        Random.Range(2.5f, 3.5f) // Place HIGHER (closer to launcher)
                    );
                    
                    guardType = "Counter-Guard (blocking opponent lane)";
                    Debug.Log($"[Strategic Guard] COUNTER opponent guard at ({opponentGuardPos.x:F2}, {opponentGuardPos.y:F2}) → guard at ({guardTarget.x:F2}, {guardTarget.y:F2})");
                }
                else
                {
                    // No opponent guards either - use opening strategy
                    useOpeningStrategy = true;
                }
            }
        }
        else
        {
            // No rocks in house - definitely use opening strategy
            useOpeningStrategy = true;
        }
        
        // ========================================
        // OPENING STRATEGY: No rocks in house yet (or no reactive options)
        // ========================================
        if (useOpeningStrategy)
        {
            Debug.Log($"[Strategic Guard] OPENING mode - hasHammer={hasHammer}");
            
            if (!hasHammer)
            {
                // ========================================
                // WITHOUT HAMMER: AGGRESSIVE - Center guards to clutter button
                // ========================================
                guardTarget = new Vector2(
                    Random.Range(-0.15f, 0.15f), // Centered with variance
                    Random.Range(3.0f, 3.5f)      // Standard center guard depth
                );
                
                guardType = "Aggressive Center Guard (clutter center)";
                Debug.Log($"[Strategic Guard] WITHOUT HAMMER → Center guard at ({guardTarget.x:F2}, {guardTarget.y:F2})");
            }
            else
            {
                // ========================================
                // WITH HAMMER: CONSERVATIVE - Corner guards to clear center
                // ========================================
                
                // Check existing guards to balance left/right
                int leftGuards = 0;
                int rightGuards = 0;
                int centerGuards = 0;
                
                foreach (var guard in gm.gList)
                {
                    if (guard.lastTransform == null) continue;
                    
                    Vector2 guardPos = guard.lastTransform.position;
                    if (Mathf.Abs(guardPos.x) < 0.4f)
                        centerGuards++;
                    else if (guardPos.x < 0f)
                        leftGuards++;
                    else
                        rightGuards++;
                }
                
                Debug.Log($"[Strategic Guard] Existing guards: Left={leftGuards}, Center={centerGuards}, Right={rightGuards}");
                
                // Prefer balancing left/right, avoid center
                bool placeLeft;
                if (leftGuards < rightGuards)
                    placeLeft = true;
                else if (rightGuards < leftGuards)
                    placeLeft = false;
                else
                    placeLeft = Random.value < 0.5f; // Balanced, random choice
                
                // CORNER GUARD POSITIONING:
                // X = 0.70-0.85 (blocking position, not too wide)
                // Y = 1.5-4.5 (varies by game phase)
                //   - Early game (rock 0-3): Y = 2.5-3.5 (medium depth)
                //   - Mid game (rock 4-11): Y = 3.0-4.0 (standard)
                //   - Late game (rock 12-15): Y = 3.5-4.5 (tight, closer to house)
                
                float depthMin, depthMax;
                if (rockCurrent < 4)
                {
                    depthMin = 2.5f;
                    depthMax = 3.5f;
                    guardType = "Early Corner Guard";
                }
                else if (rockCurrent < 12)
                {
                    depthMin = 3.0f;
                    depthMax = 4.0f;
                    guardType = "Mid-Game Corner Guard";
                }
                else
                {
                    depthMin = 3.5f;
                    depthMax = 4.5f;
                    guardType = "Late Corner Guard (tight)";
                }
                
                if (placeLeft)
                {
                    guardTarget = new Vector2(
                        Random.Range(-0.85f, -0.70f), // Left corner
                        Random.Range(depthMin, depthMax)
                    );
                    guardType = "LEFT " + guardType;
                }
                else
                {
                    guardTarget = new Vector2(
                        Random.Range(0.70f, 0.85f), // Right corner
                        Random.Range(depthMin, depthMax)
                    );
                    guardType = "RIGHT " + guardType;
                }
                
                Debug.Log($"[Strategic Guard] WITH HAMMER → {guardType} at ({guardTarget.x:F2}, {guardTarget.y:F2})");
            }
        }
        
        // ========================================
        // EXECUTION: Physics-based draw to target + accuracy adjustments
        // ========================================
        Debug.Log($"[Strategic Guard] Final target: ({guardTarget.x:F2}, {guardTarget.y:F2}) - {guardType}");
        
        Vector2 pullbackPos;
        bool useInTurn;
        bool foundShot = CalculatePhysicsBasedGuardShot(guardTarget, out pullbackPos, out useInTurn);
        
        if (foundShot)
        {
            // STEP 1: Get shooter stats for accuracy
            CharacterStats shooterStats = GetShooterStats(rockCurrent);
            
            if (shooterStats != null)
            {
                float accuracy = shooterStats.guardAccuracy.GetValue(); // 0-100
                float accuracyRatio = Mathf.Clamp01(accuracy / 100f);
                
                // STEP 2: Apply unit circle accuracy error (SAME as RandomRockPlacement!)
                // This ensures consistent guard placement between placed and shot guards
                float baseMaxError = 0.20f; // Guards can be off by up to 20cm for 0% accuracy
                float maxError = baseMaxError * (1f - accuracyRatio);
                
                // CIRCULAR ERROR: Unit circle centered on target
                Vector2 targetError = Random.insideUnitCircle * maxError;
                
                // Apply error to TARGET position (not pullback!)
                Vector2 adjustedTarget = guardTarget + targetError;
                
                Debug.Log($"[Guard Accuracy] Original target: {guardTarget}, Accuracy: {accuracy}%, Error: {targetError}, Adjusted target: {adjustedTarget}");
                
                // STEP 3: Recalculate pullback for adjusted target
                Vector2 adjustedPullback;
                bool adjustedInTurn;
                bool foundAdjusted = CalculatePhysicsBasedGuardShot(adjustedTarget, out adjustedPullback, out adjustedInTurn);
                
                if (foundAdjusted)
                {
                    pullbackPos = adjustedPullback;
                    useInTurn = adjustedInTurn;
                    Debug.Log($"[Guard Accuracy] Recalculated for adjusted target: pullback={pullbackPos}, inTurn={useInTurn}");
                }
                else
                {
                    Debug.LogWarning($"[Guard Accuracy] Recalculation failed, using original pullback with small error");
                    // Fallback: Just add small error to original pullback
                    pullbackPos += targetError * 0.1f; // Scale down error for pullback space
                }
            }
            else
            {
                Debug.Log($"[Guard Accuracy] No shooter stats - using perfect accuracy (no error)");
            }
            
            // STEP 4: Set pullback and execute
            rm.inturn = useInTurn;
            takeOutX = pullbackPos.x;
            takeOutY = pullbackPos.y;
            
            Debug.Log($"[Strategic Guard] ✓ EXECUTING {guardType}\n" +
                      $"  Target: ({guardTarget.x:F2}, {guardTarget.y:F2})\n" +
                      $"  Pullback: ({takeOutX:F3}, {takeOutY:F3})\n" +
                      $"  Turn: {(useInTurn ? "IN-TURN (curl right)" : "OUT-TURN (curl left)")}\n" +
                      $"  Has Hammer: {hasHammer}");
            
            aiShoot.OnShot("Guard To Target", rockCurrent);
        }
        else
        {
            // FALLBACK: Physics failed - use direct shot names
            Debug.LogWarning($"[Strategic Guard] Physics calculation failed for {guardType}, using direct shot name");
            
            // Determine shot name based on target position
            if (Mathf.Abs(guardTarget.x) < 0.4f)
            {
                aiShoot.OnShot("Centre Guard", rockCurrent);
            }
            else if (guardTarget.x < 0f)
            {
                aiShoot.OnShot("Left Corner Guard", rockCurrent);
            }
            else
            {
                aiShoot.OnShot("Right Corner Guard", rockCurrent);
            }
        }
    }
    
    /// <summary>
    /// Helper: Check if a guard is blocking a target rock
    /// </summary>
    private bool IsGuardBlocking(Transform guard, GameObject targetRock, float tolerance = 0.1f)
    {
        if (guard == null || targetRock == null) return false;
        return Mathf.Abs(guard.position.x - targetRock.transform.position.x) <= tolerance;
    }
    
    /// <summary>
    /// Helper: Get the rock index for a transform (guard or house rock)
    /// </summary>
    private int GetRockIndex(Transform rockTransform)
    {
        if (rockTransform == null) return -1;
        Rock_Info info = rockTransform.GetComponent<Rock_Info>();
        return info != null ? info.rockIndex : -1;
    }
    
    #endregion
}

