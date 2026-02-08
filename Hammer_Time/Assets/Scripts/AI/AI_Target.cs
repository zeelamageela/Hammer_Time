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
    public float iceFriction = 2.5f;
    public float curlStrength = 0.3f;
    public float lateBreakingIntensity = 2.0f;
    public float lateBreakingCurve = 0.8f;
    
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
        // Initialize physics simulator for accurate targeting
        // CRITICAL: linearDamping must be 0.38 to match Rock_Force.cs!
        trajectorySimulator = new TrajectorySimulator(0.38f, curlStrength, lateBreakingIntensity, lateBreakingCurve);
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
        
        if (shotType == "Take Out" || shotType == "Peel")
        {
            // Target Impact Point: Position the SHOOTER before the target
            // FIXED: Use EXACT collision distance (2 * radius), not multiplier!
            // When two circles collide, distance between centers = 2 * radius
            float impactOffset = 2f * rockRadius;  // Exact collision distance = 0.28 units
            targetImpactPoint = new Vector2(
                targetRockPosition.x,
                targetRockPosition.y - impactOffset
            );
            
            // Velocity Aim Point: Far down the ice to get proper weight
            // We aim PAST the target to ensure we have enough drive-through
            velocityAimPoint = new Vector2(
                targetRockPosition.x,
                targetRockPosition.y + 2.0f  // Aim 2 units past for weight
            );
            
            Debug.Log($"[AI_Target] Impact point calculation:\n" +
                      $"  Target rock: {targetRockPosition}\n" +
                      $"  Impact offset: {impactOffset:F3} (multiplier {aimPointRadiusMultiplier} × radius {rockRadius})\n" +
                      $"  Target impact point (shooter center at collision): {targetImpactPoint}\n" +
                      $"  Velocity aim point (for weight calculation): {velocityAimPoint}");
        }
        else if (shotType == "Tap Back" || shotType == "Raise")
        {
            targetImpactPoint = targetRockPosition;
            velocityAimPoint = new Vector2(targetRockPosition.x, Mathf.Min(targetRockPosition.y + 2f, 9f));
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
            // CRITICAL FIX: We're COMPENSATING for curl, not aiming with it!
            // IN-TURN (curls RIGHT): Rock will curl right, so aim LESS left (reduce compensation) = POSITIVE offset
            // OUT-TURN (curls LEFT): Rock will curl left, so aim LESS right (reduce compensation) = NEGATIVE offset
            // This is REVERSED from intuition because we're fighting the curl, not using it!
            float offsetMultiplier = tryInTurn ? 1f : -1f; // REVERSED!
            
            // TWO-PHASE APPROACH:
            // Phase 1: Coarse sweep to find general region (-0.6 to +0.6 in 0.1 steps)
            // Phase 2: Fine sweep around best coarse result (±0.1 in 0.01 steps)
            
            float bestCoarseOffset = 0f;
            float bestCoarseScore = float.MinValue;
            
            // PHASE 1: COARSE SWEEP (13 positions)
            for (float lateralOffsetBase = -0.6f; lateralOffsetBase <= 0.6f; lateralOffsetBase += 0.1f)
            {
                float lateralOffset = lateralOffsetBase * offsetMultiplier;
                
                Vector2 velocityTarget = new Vector2(targetRockPosition.x + lateralOffset, velocityAimPoint.y);
                Vector2 baseVelocity = trajectorySimulator.CalculateVelocityToTarget(launcherPos, velocityTarget, tryInTurn, isCollisionShot: true);
                
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
            
            // PHASE 2: FINE SWEEP around best coarse result (21 positions)
            float fineStart = Mathf.Max(-0.6f, bestCoarseOffset - 0.1f);
            float fineEnd = Mathf.Min(0.6f, bestCoarseOffset + 0.1f);
            
            for (float lateralOffsetBase = fineStart; lateralOffsetBase <= fineEnd; lateralOffsetBase += 0.01f)
            {
                // Apply multiplier based on turn direction
                float lateralOffset = lateralOffsetBase * offsetMultiplier;
                
                // Create velocity aim point (for calculating required speed)
                // This is a point FAR down the ice to get proper weight
                Vector2 velocityTarget = new Vector2(
                    targetRockPosition.x + lateralOffset,
                    velocityAimPoint.y  // Use the far aim point for velocity calculation
                );
                
                // Calculate velocity to velocity aim point
                Vector2 baseVelocity = trajectorySimulator.CalculateVelocityToTarget(
                    launcherPos,
                    velocityTarget,
                    tryInTurn,
                    isCollisionShot: true
                );
                
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
                    
                    if (hitRockGameObject == targetRockGameObject)
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
                        bool isFromBehind = hitVector.y < -0.05f; // Must be approaching from below
                        
                        if (!isFromBehind)
                        {
                            Debug.Log($"[AI_Target] ⚠️ REJECTED - Not hitting from behind! Y offset={hitVector.y:F3} (need Y < -0.05)");
                            continue; // Skip this hit
                        }
                        
                        // HYBRID SCORING: Combine nose angle quality with target impact point accuracy
                        
                        // PART 1: Nose Hit Angle (50% of score)
                        // Perfect nose hit = shooting from DIRECTLY behind at -90°
                        float perfectNoseAngle = -90f;
                        float angleDeviation = Mathf.Abs(hitAngle - perfectNoseAngle);
                        float maxAcceptableAngleDeviation = 30f;
                        float noseAngleQuality = 1.0f - Mathf.Clamp01(angleDeviation / maxAcceptableAngleDeviation);
                        
                        // PART 2: Distance to Target Impact Point (50% of score)
                        // This is where we WANTED the shooter's center to be when hitting
                        // FIXED: Use ACTUAL shooter center from collision info, not calculated!
                        Vector2 shooterCenterAtCollision = collisionInfo.shooterCenterAtCollision;
                        float distanceToTargetImpact = Vector2.Distance(shooterCenterAtCollision, targetImpactPoint);
                        
                        // Perfect = 0 distance, acceptable up to 0.2 units (20cm) away
                        float maxAcceptableDistance = 0.2f;
                        float impactPointQuality = 1.0f - Mathf.Clamp01(distanceToTargetImpact / maxAcceptableDistance);
                        
                        // COMBINED SCORE: Both factors weighted equally
                        float hitQuality = (noseAngleQuality * 0.5f) + (impactPointQuality * 0.5f);
                        
                        Debug.Log($"[AI_Target] Hybrid Scoring:\n" +
                                  $"  Nose Angle Quality: {noseAngleQuality:F3} (angle dev: {angleDeviation:F1}°)\n" +
                                  $"  Impact Point Quality: {impactPointQuality:F3} (dist: {distanceToTargetImpact:F3})\n" +
                                  $"  Combined Hit Quality: {hitQuality:F3}");
                        
                        
                        
                        // 🔍 COMPREHENSIVE DEBUG: ALL INFO IN ONE PLACE
                        Debug.Log($"🎯 HIT DIAGNOSTIC (HYBRID SCORING)\n" +
                                  $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                                  $"TURN: {(tryInTurn ? "IN-TURN (curls RIGHT)" : "OUT-TURN (curls LEFT)")}\n" +
                                  $"TARGETING:\n" +
                                  $"  • Lateral Offset: {lateralOffset:F3}\n" +
                                  $"  • Velocity Aim Point: {velocityTarget} (for weight)\n" +
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
                        
                        if (score > bestScore)
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
                        Debug.Log($"❌ Hit OBSTACLE: {hitRockGameObject.name} (not target) at lateral offset {lateralOffset:F2}");
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
    /// Inverse of the spring physics calculation
    /// Uses the REAL empirically-measured spring constant from the game!
    /// </summary>
    private Vector2 CalculatePullbackFromVelocity(Vector2 desiredVelocity, Vector2 launcherPos, bool isInTurn)
    {
        // From TrajectorySimulator.CalculateInitialVelocityFromSpring():
        // velocity = (launcherPos - pullbackPos).magnitude * 5.9
        //
        // Therefore:
        // pullbackDistance = velocity / 5.9
        // pullbackPos = launcherPos - pullbackDirection * pullbackDistance
        
        float velocityMagnitude = desiredVelocity.magnitude;
        float velocityMultiplier = 5.9f; // EMPIRICAL from game measurements!
        
        float pullbackDistance = velocityMagnitude / velocityMultiplier;
        Vector2 pullbackDirection = desiredVelocity.normalized;
        
        Vector2 pullback = launcherPos - pullbackDirection * pullbackDistance;
        
        // Verify the calculation
        Vector2 springDisplacement = launcherPos - pullback;
        float springDistance = springDisplacement.magnitude;
        Vector2 expectedVelocity = springDisplacement.normalized * (springDistance * velocityMultiplier);
        
        Debug.Log($"[PullbackCalc] Turn: {(isInTurn ? "IN-TURN" : "OUT-TURN")}, " +
                  $"DesiredVel: {desiredVelocity} (mag:{velocityMagnitude:F2}), " +
                  $"PullbackDist: {pullbackDistance:F2}, " +
                  $"Pullback: {pullback}, " +
                  $"Launcher: {launcherPos}, " +
                  $"SpringDist: {springDistance:F2}, " +
                  $"ExpectedVel: {expectedVelocity} (mag:{expectedVelocity.magnitude:F2}) " +
                  $"[Should match DesiredVel!]");

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

            case "Auto Take Out":
                // DEPRECATED: This method uses old magic number calculations
                // Use "Take Out" with a specific target instead, which uses physics-based targeting
                Debug.LogWarning("[AI_Target] 'Auto Take Out' is deprecated - uses old magic numbers. Consider using physics-based 'Take Out' instead.");
                StartCoroutine(TakeOutAutoTarget(rockCurrent));
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

            case "Auto Draw Twelve Foot":
                StartCoroutine(DrawTwelveFoot(rockCurrent));
                break;

            case "Auto Draw Four Foot":
                StartCoroutine(DrawFourFoot(rockCurrent));
                break;

            case "Manual Draw":
                StartCoroutine(DrawTarget(rockCurrent));
                break;

            case "Manual Guard":
                StartCoroutine(GuardTarget(rockCurrent));
                break;

            case "Player Draw":
                targetPos = new Vector2(playerTarget.position.x, playerTarget.position.y);
                StartCoroutine(DrawTarget(rockCurrent));
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
                
                // Scale error: 100 skill = 0.00 max error (PERFECT!), 50 skill = 0.15 max error, 0 skill = 0.25 max error
                // Formula: maxError = 0.25 * (1 - accuracy/100)
                // This gives: 100 skill → 0.00, 75 skill → 0.0625, 50 skill → 0.125, 25 skill → 0.1875, 0 skill → 0.25
                float maxError = 0.25f * (1f - (accuracy / 100f));
                
                if (maxError > 0f)
                {
                    Vector2 errorOffset = Random.insideUnitCircle * maxError;
                    
                    // CRITICAL FIX: Lateral error must respect turn direction
                    // IN-TURN (curls right): pullback on LEFT (negative X) -> negative lateral error moves it MORE left (away from curl)
                    // OUT-TURN (curls left): pullback on RIGHT (positive X) -> positive lateral error moves it MORE right (away from curl)
                    // This ensures error doesn't flip the shot direction
                    float lateralErrorSign = useInTurn ? -1f : 1f;
                    errorOffset.x *= lateralErrorSign;
                    
                    pullbackPos += errorOffset;
                    
                    Debug.Log($"[AI_Target] Accuracy error applied - Max: {maxError:F3}, Actual: {errorOffset.magnitude:F3}\n" +
                              $"Lateral error sign: {lateralErrorSign} (IN-TURN={useInTurn})\n" +
                              $"Original pullback: {originalPullback}\n" +
                              $"Final pullback: {pullbackPos}\n" +
                              $"Pullback lateral offset: {pullbackPos.x:F3}");
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
                      $"Turn: {(useInTurn ? "IN-TURN (curl RIGHT)" : "OUT-TURN (curl LEFT)")}\n" +
                      $"Expected curl direction: {(useInTurn ? "positive X (right)" : "negative X (left)")}\n" +
                      $"========== TAKEOUT DEBUG END ==========\n");
        }
        else
        {
            // Fallback to old method ONLY for pullback position
            Debug.LogWarning($"[AI_Target] Take Out physics FAILED - using fallback position for target: {targetRockPos}");
            
            targetX = targetRockPos.x;
            targetY = targetRockPos.y;

            // Calculate fallback pullback position without changing rm.inturn
            if (rm.inturn)
            {
                takeOutX = (-0.19f * ((targetX + 1.35f) / 2.7f)) + 0.11f;
            }
            else
            {
                takeOutX = (-0.205f * ((targetX + 1.35f) / 2.7f)) + 0.087f;
            }
            
            Debug.LogWarning($"[AI_Target] Fallback - Using existing turn state: {(rm.inturn ? "IN-TURN" : "OUT-TURN")}");
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

        // PHYSICS-BASED: Peel requires high speed to remove rock completely
        // Unlike takeout, we don't care if our rock stays in play
        Vector2 targetRockPos = gm.rockList[rockTarget].rock.transform.position;
        
        Vector2 pullbackPos;
        bool useInTurn;
        bool foundShot = CalculatePhysicsBasedShot(targetRockPos, out pullbackPos, out useInTurn, "Peel", rockTarget);
        
        if (foundShot)
        {
            // CRITICAL: Set rm.inturn from physics calculation ONCE
            rm.inturn = useInTurn;
            takeOutX = pullbackPos.x;
            takeOutY = pullbackPos.y;
            
            Debug.Log($"[AI_Target] Peel SUCCESS - InTurn: {useInTurn}, Target: {targetRockPos}, Pullback: {pullbackPos}");
        }
        else
        {
            // Fallback - use existing turn state, don't override with magic numbers
            Debug.LogWarning($"[AI_Target] Peel physics FAILED - using fallback position for target: {targetRockPos}");
            targetX = targetRockPos.x;
            targetY = targetRockPos.y;

            // Calculate fallback pullback using existing turn direction
            if (rm.inturn)
            {
                takeOutX = (-0.219f * ((targetX + 1.35f) / 2.7f)) + 0.122f;
            }
            else
            {
                takeOutX = (-0.222f * ((targetX + 1.35f) / 2.7f)) + 0.102f;
            }
            
            Debug.LogWarning($"[AI_Target] Fallback - Using existing turn state: {(rm.inturn ? "IN-TURN" : "OUT-TURN")}");
        }

        aiShoot.OnShot("Peel", rockCurrent);
        Debug.Log(gm.rockList[rockTarget].rockInfo.teamName + " " + gm.rockList[rockTarget].rockInfo.rockNumber);
        yield break;
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

        // PHYSICS-BASED: Tap back requires lighter weight - move rock but keep both in play
        // Goal: Tap rock back, have shooter stop in front with separation
        Vector2 targetRockPos = gm.rockList[rockTarget].rock.transform.position;
        
        Vector2 pullbackPos;
        bool useInTurn;
        bool foundShot = CalculatePhysicsBasedShot(targetRockPos, out pullbackPos, out useInTurn, "Tap Back", rockTarget);
        
        if (foundShot)
        {
            // CRITICAL: Set rm.inturn from physics calculation ONCE
            rm.inturn = useInTurn;
            takeOutX = pullbackPos.x;
            takeOutY = pullbackPos.y;
            
            Debug.Log($"[AI_Target] Tap Back SUCCESS - InTurn: {useInTurn}, Target: {targetRockPos}, Pullback: {pullbackPos}");
        }
        else
        {
            // Fallback - use existing turn state
            Debug.LogWarning($"[AI_Target] Tap Back physics FAILED - using fallback position for target: {targetRockPos}");
            targetX = targetRockPos.x;
            targetY = targetRockPos.y;

            // Calculate fallback pullback using existing turn direction
            if (rm.inturn)
            {
                takeOutX = (-0.18f * ((targetX + 1.35f) / 2.7f)) + 0.12f;
            }
            else
            {
                takeOutX = (-0.178f * ((targetX + 1.35f) / 2.7f)) + 0.056f;
            }
            
            Debug.LogWarning($"[AI_Target] Fallback - Using existing turn state: {(rm.inturn ? "IN-TURN" : "OUT-TURN")}");
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
            // Fallback - use existing turn state
            Debug.LogWarning($"[AI_Target] Tick physics FAILED - using fallback position for target: {targetRockPos}");
            targetX = targetRockPos.x;
            targetY = targetRockPos.y;

            // Calculate fallback pullback using existing turn direction
            if (rm.inturn)
            {
                takeOutX = (-0.039f * ((targetX + 0.4f) / 0.8f)) + 0.042f;
            }
            else
            {
                takeOutX = (-0.04f * ((targetX + 0.4f) / 0.8f)) - 0.005f;
            }
            
            Debug.LogWarning($"[AI_Target] Fallback - Using existing turn state: {(rm.inturn ? "IN-TURN" : "OUT-TURN")}");
        }

        aiShoot.OnShot("Tick", rockCurrent);
        Debug.Log("Tick - " + gm.rockList[rockTarget].rockInfo.teamName + " " + gm.rockList[rockTarget].rockInfo.rockNumber);
        yield break;
    }

    IEnumerator DrawTarget(int rockCurrent)
    {
        // PHYSICS-BASED: Draw to a specific target position in the house
        // Examines guards to find best path
        Vector2 targetPosition = targetPos;
        
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
            // Fallback to old method
            Debug.LogWarning("[Physics Draw] Failed, using fallback");
            targetX = targetPos.x;
            targetY = targetPos.y;

            takeOutY = (-0.21f * ((targetY - 5.225f) / 2.55f)) - 26.9f;

            if (rm.inturn == false)
            {
                takeOutX = (-0.169f * ((targetX + 1.35f) / 2.7f)) + 0.021f;
            }
            else
            {
                takeOutX = (-0.15f * ((targetX + 1.35f) / 2.7f)) + 0.12f;
            }

            float angle = Vector2.Angle(Vector2.zero, new Vector2(takeOutX, takeOutY));
            float distance = Vector2.Distance(new Vector2(takeOutX, takeOutY), new Vector2(0f, -25f));
            takeOutX += (distance * Mathf.Sin(angle));
        }

        aiShoot.OnShot("Draw To Target", rockCurrent);
        yield break;
    }
    
    /// <summary>
    /// Physics-based draw shot calculation - finds clear path around guards
    /// </summary>
    private bool CalculatePhysicsBasedDrawShot(Vector2 targetPosition, out Vector2 pullbackPosition, out bool useInTurn)
    {
        Vector2 launcherPos = new Vector2(0f, -25f);
        
        // Get rocks in play (guards are obstacles)
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
            
            // Calculate required velocity
            Vector2 requiredVelocity = trajectorySimulator.CalculateVelocityToTarget(
                launcherPos,
                targetPosition,
                tryInTurn
            );
            
            if (requiredVelocity.magnitude < 3f || requiredVelocity.magnitude > 20f)
                continue;
            
            Vector2 testPullback = CalculatePullbackFromVelocity(requiredVelocity, launcherPos, tryInTurn);
            
            // Simulate to see if we reach target cleanly
            List<Vector2> simulatedPath = trajectorySimulator.SimulateTrajectory(
                launcherPos,
                requiredVelocity,
                tryInTurn,
                250,
                rocksInPlay
            );
            
            if (simulatedPath.Count == 0) continue;
            
            Vector2 finalPos = simulatedPath[simulatedPath.Count - 1];
            float distanceToTarget = Vector2.Distance(finalPos, targetPosition);
            
            // Score: closer to target = better, penalty for hitting guards
            TrajectorySimulator.CollisionInfo collisionInfo = trajectorySimulator.GetCollisionInfo();
            float score = -distanceToTarget;
            
            // Penalty if we hit anything (want clear path)
            if (collisionInfo.hasCollision)
            {
                score -= 5f;
            }
            
            if (score > bestScore)
            {
                bestScore = score;
                bestPullback = testPullback;
                bestInTurn = tryInTurn;
            }
        }
        
        if (bestScore > float.MinValue && bestScore > -1f) // Within 1 unit is acceptable
        {
            pullbackPosition = bestPullback;
            useInTurn = bestInTurn;
            return true;
        }
        
        pullbackPosition = launcherPos + new Vector2(0f, -2f);
        useInTurn = false;
        return false;
    }

    IEnumerator GuardTarget(int rockCurrent)
    {
        // PHYSICS-BASED: Place guard in front of house
        // Target area is in guards zone (y < 5f typically)
        Vector2 targetPosition = targetPos;
        
        Vector2 pullbackPos;
        bool useInTurn;
        
        // Guards require less velocity than draws (shorter distance)
        bool foundShot = CalculatePhysicsBasedDrawShot(targetPosition, out pullbackPos, out useInTurn);
        
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
            
            Debug.Log($"[Physics Guard] Target: {targetPosition}, Pullback: {pullbackPos}, InTurn: {useInTurn}");
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
            case ShotIntent.DrawToButton:
                OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                break;
                
            case ShotIntent.ProtectLead:
            case ShotIntent.CreateOpportunity:
                PlaceStrategicGuard(context, rockCurrent);
                break;
                
            case ShotIntent.ForceBlank:
                // Clear the house - takeout shot rock
                if (gm.houseList.Count > 0)
                {
                    OnTarget("Take Out", rockCurrent, gm.houseList[0].rockInfo.rockIndex);
                }
                else
                {
                    OnTarget("Auto Draw Four Foot", rockCurrent, 0);
                }
                break;
                
            case ShotIntent.ThrowAway:
                // Just throw it out - guard or wide draw
                aiShoot.OnShot("Centre Guard", rockCurrent);
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
        
        if (IsGuardBlocking(cenGuard, targetRock))
        {
            guardToPeel = GetRockIndex(cenGuard);
            peelScore = SimulatePeel(cenGuard.gameObject, guardToPeel, rockCurrent);
            Debug.Log($"  Option 2: Peel center guard - Score: {peelScore:F2}");
        }
        else if (IsGuardBlocking(lCornGuard, targetRock))
        {
            guardToPeel = GetRockIndex(lCornGuard);
            peelScore = SimulatePeel(lCornGuard.gameObject, guardToPeel, rockCurrent);
            Debug.Log($"  Option 2: Peel left guard - Score: {peelScore:F2}");
        }
        else if (IsGuardBlocking(rCornGuard, targetRock))
        {
            guardToPeel = GetRockIndex(rCornGuard);
            peelScore = SimulatePeel(rCornGuard.gameObject, guardToPeel, rockCurrent);
            Debug.Log($"  Option 2: Peel right guard - Score: {peelScore:F2}");
        }
        
        // OPTION 3: Raise a friendly rock into the threat
        float raiseScore = 0f;
        int rockToRaise = FindBestRaiseTarget(targetRock, rockCurrent);
        if (rockToRaise >= 0)
        {
            raiseScore = SimulateRaise(targetRock, rockToRaise, rockCurrent);
            Debug.Log($"  Option 3: Raise rock #{rockToRaise} - Score: {raiseScore:F2}");
        }
        
        // OPTION 4: Tick it out sideways
        float tickScore = SimulateTick(targetRock, context.targetRockIndex, rockCurrent);
        Debug.Log($"  Option 4: Tick shot - Score: {tickScore:F2}");
        
        // PICK THE BEST OPTION!
        float bestScore = Mathf.Max(takeoutScore, peelScore, raiseScore, tickScore);
        
        if (bestScore <= 0f)
        {
            Debug.LogWarning("[AI_Target] No good removal options found, drawing instead");
            OnTarget("Auto Draw Four Foot", rockCurrent, 0);
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
        else if (raiseScore == bestScore && raiseScore > 0f)
        {
            Debug.Log($"[AI_Target] ✓ SELECTED: Raise Rock (score: {raiseScore:F2})");
            OnTarget("Tap Back", rockCurrent, rockToRaise); // Raise uses tap back mechanics
        }
        else if (tickScore == bestScore && tickScore > 0f)
        {
            Debug.Log($"[AI_Target] ✓ SELECTED: Tick Shot (score: {tickScore:F2})");
            OnTarget("Tick Shot", rockCurrent, context.targetRockIndex);
        }
    }
    
    /// <summary>
    /// Simulate a direct takeout - returns quality score (0-100)
    /// </summary>
    private float SimulateTakeout(GameObject targetRock, int targetRockIndex, int rockCurrent)
    {
        Vector2 targetPos = targetRock.transform.position;
        Vector2 launcherPos = new Vector2(0f, -25f);
        
        Vector2 pullbackPos;
        bool useInTurn;
        bool foundShot = CalculatePhysicsBasedShot(targetPos, out pullbackPos, out useInTurn, "Take Out", targetRockIndex);
        
        if (foundShot)
        {
            // Score is based on how well physics simulation found a hit
            // CalculatePhysicsBasedShot returns bestScore (0-100)
            // For simplicity, if we found a shot, assume it's at least 50% quality
            return 60f; // Good option
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
    /// Place a guard strategically based on context
    /// </summary>
    private void PlaceStrategicGuard(ShotContext context, int rockCurrent)
    {
        // Determine best guard position based on rocks in house
        if (gm.houseList.Count > 0)
        {
            GameObject closestRock = gm.houseList[0].rock;
            Vector2 closestPos = closestRock.transform.position;
            
            // Guard in front of closest rock
            if (Mathf.Abs(closestPos.x) < 0.5f)
            {
                aiShoot.OnShot("Centre Guard", rockCurrent);
            }
            else if (closestPos.x < 0f)
            {
                aiShoot.OnShot("Left Corner Guard", rockCurrent);
            }
            else
            {
                aiShoot.OnShot("Right Corner Guard", rockCurrent);
            }
        }
        else
        {
            // No rocks yet - place center guard
            aiShoot.OnShot("Centre Guard", rockCurrent);
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

