using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Shooter : MonoBehaviour
{
    public GameManager gm;
    public TutorialManager tm;
    public RockManager rm;

    public AIManager aim;
    public AI_Target aiTarg;
    public AI_Strategy aiStrat;
    public AI_Sweeper aiSweep;

    Rock_Info rockInfo;
    Rock_Flick rockFlick;
    Rigidbody2D rockRB;
    
    int currentRockNumber;

    public Vector2 centreGuard;
    public Vector2 tightCentreGuard;
    public Vector2 highCentreGuard;

    public Vector2 leftHighCornerGuard;
    public Vector2 leftTightCornerGuard;
    public Vector2 leftCornerGuard;
    public Vector2 rightHighCornerGuard;
    public Vector2 rightTightCornerGuard;
    public Vector2 rightCornerGuard;

    public Vector2 topTwelveFoot;
    public Vector2 backTwelveFoot;
    public Vector2 leftTwelveFoot;
    public Vector2 rightTwelveFoot;

    public Vector2 backFourFoot;
    public Vector2 topFourFoot;
    public Vector2 leftFourFoot;
    public Vector2 rightFourFoot;
    public Vector2 button;

    public Vector2 peel;
    public Vector2 takeOut;
    public Vector2 raise;
    public Vector2 tick;


    public Vector2 guardAccu;
    public Vector2 drawAccu;
    public Vector2 toAccu;
    public Vector2 tickAccu;

    public float takeOutOffset;
    public float peelOffset;
    public float raiseOffset;
    public float tickOffset;

    float targetX;
    float targetY;
    public float takeOutX;
    public float takeOutY;
    float raiseY;
    GameSettingsPersist gsp;

    public void Start()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
    }


    public void OnShot(string aiShotType, int rockCurrent)
    {
        rockInfo = gm.rockList[rockCurrent].rockInfo;
        rockFlick = gm.rockList[rockCurrent].rock.GetComponent<Rock_Flick>();
        rockRB = gm.rockList[rockCurrent].rock.GetComponent<Rigidbody2D>();
        currentRockNumber = rockCurrent;

        // CRITICAL FIX: Lock flipAxis IMMEDIATELY to prevent RockManager from overriding!
        // AI_Target has already set rm.inturn, so use that value NOW
        GameObject rock = gm.rockList[currentRockNumber].rock;
        Rock_Force rockForce = rock.GetComponent<Rock_Force>();
        if (rockForce != null)
        {
            rockForce.flipAxis = rm.inturn;
            Debug.Log($"[AI_Shooter.OnShot] LOCKED flipAxis = {rm.inturn} immediately for {aiShotType}");
        }

        StartCoroutine(Shot(aiShotType, rm.inturn));
    }

    IEnumerator Shot(string aiShotType, bool inturn)
    {
        Debug.Log("AI Shot " + aiShotType);
        gm.dbText.text = aiShotType;
        rockFlick.isPressedAI = true;
        takeOutX = aiTarg.takeOutX;
        takeOutY = aiTarg.takeOutY;

        aiSweep.OnSweep(true, aiShotType, aiTarg.targetPos, inturn);
        
        // CRITICAL FIX: Set BOTH flipAxis AND rm.inturn to keep everything synchronized!
        GameObject rock = gm.rockList[currentRockNumber].rock;
        Rock_Force rockForce = rock.GetComponent<Rock_Force>();
        if (rockForce != null)
        {
            rockForce.flipAxis = inturn;
            rm.inturn = inturn;
            Debug.Log($"[AI_Shooter] Set flipAxis AND rm.inturn = {inturn} for {aiShotType}");
        }

        yield return new WaitForSeconds(0.5f);

        
        
        // UNIFIED SHOT HANDLING: All physics-based shots use the same logic
        // AI_Target has already calculated the exact pullback position WITH accuracy error applied
        // Just use that position directly - no need for shot-specific handling!
        
        float shotX;
        float shotY;
        
        // Check if physics calculation succeeded (non-zero position)
        if (takeOutX != 0f || takeOutY != 0f)
        {
            // SUCCESS: Use physics-calculated position (accuracy error already applied by AI_Target)
            shotX = takeOutX;
            shotY = takeOutY;
            
            Debug.Log($"[AI_Shooter] {aiShotType} - Using physics position: ({shotX:F3}, {shotY:F3})");
        }
        else
        {
            // FALLBACK: Physics failed, draw to button with accuracy error
            // This should rarely happen - indicates a problem with AI_Target
            Debug.LogWarning($"[AI_Shooter] {aiShotType} FALLBACK - No physics position available!");
            
            CharacterStats stats = GetShooterStats();
            float accuracy = stats != null ? stats.drawAccuracy.GetValue() : 70f;
            Vector2 error = GetAccuracyError(accuracy, 0.15f);
            
            shotX = button.x + error.x;
            shotY = button.y + error.y;
        }
        
        // Execute shot: Set position and trigger release
        rockFlick.rb.isKinematic = true;
        rockRB.position = new Vector2(shotX, shotY);
        
        Debug.Log($"[AI_Shooter] {aiShotType} final position: ({rockRB.position.x:F3}, {rockRB.position.y:F3})");
        
        yield return new WaitForFixedUpdate();
        rockFlick.mouseUp = true;

        // Wait for rock to actually be released and have velocity
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        // Start AI sweeping coroutine
        if (gm != null && rm != null && aiSweep.sm != null)
        {
            Vector2 initialVelocity = rockRB.linearVelocity;
            Vector2 targetPosition = aiTarg.targetPos;
            bool isInTurn = inturn;

            Debug.Log($"[AI_Shooter] Starting sweeping monitor: velocity={initialVelocity.magnitude:F2} m/s, target={targetPosition}, inTurn={isInTurn}");

            StartCoroutine(MonitorAndSweepCoroutine(rockRB, initialVelocity, isInTurn, targetPosition, aiShotType));
        }
    }


    /// <summary>
    /// Get character stats for the current shooter
    /// </summary>
    private CharacterStats GetShooterStats()
    {
        TeamManager tm = FindObjectOfType<TeamManager>();
        if (tm == null) return null;
        
        int memberIndex = currentRockNumber / 4;
        memberIndex = Mathf.Clamp(memberIndex, 0, 3);
        
        bool isRedTeam = (currentRockNumber % 2 == 0) ? gm.redHammer : !gm.redHammer;
        
        if (isRedTeam && tm.teamRed != null && memberIndex < tm.teamRed.Length)
            return tm.teamRed[memberIndex].charStats;
        else if (!isRedTeam && tm.teamYellow != null && memberIndex < tm.teamYellow.Length)
            return tm.teamYellow[memberIndex].charStats;
        
        return null;
    }
    
    /// <summary>
    /// Apply character-based accuracy using realistic distribution
    /// Returns error offset to add to target position
    /// </summary>
    private Vector2 GetAccuracyError(float accuracy, float baseMaxError)
    {
        // Convert accuracy from 0-100 to 0-1 scale
        float accuracyRatio = Mathf.Clamp01(accuracy / 100f);
        
        // Calculate max error based on accuracy (better accuracy = less error)
        float maxError = baseMaxError * (1f - accuracyRatio);
        
        // Use circular distribution for natural shot spread
        return Random.insideUnitCircle * maxError;
    }
    /// <summary>
    /// Monitor rock position vs predicted trajectory and make sweeping decisions
    /// </summary>
    private IEnumerator MonitorAndSweepCoroutine(Rigidbody2D rockRB, Vector2 initialVelocity, bool isInTurn, Vector2 targetPosition, string shotType)
    {
        GameObject rock = gm.rockList[currentRockNumber].rock;
        if (rock == null)
        {
            Debug.LogWarning("[AI_Sweeper] No active rock found!");
            yield break;
        }

        Rock_Info rockInfo = rock.GetComponent<Rock_Info>();

        bool isOpponentRock = (rockInfo.teamName != gm.rockList[currentRockNumber].rockInfo.teamName);
        bool pastTLine = (rock.transform.position.y > 6.5f);

        // Get trajectory simulator
        TrajectoryLine playerTrajectory = FindObjectOfType<TrajectoryLine>();
        TrajectorySimulator trajectorySimulator = null;

        if (playerTrajectory != null)
        {
            trajectorySimulator = new TrajectorySimulator(
                playerTrajectory.iceFriction,
                playerTrajectory.curlStrength
            );
        }
        else
        {
            Debug.LogWarning("[AI_Sweeper] TrajectoryLine not found!");
            yield break;
        }

        // Generate predicted path
        Vector2 launcherPos = new Vector2(0f, -25f);
        List<GameObject> rocksInPlay = new List<GameObject>();
        foreach (var rockEntry in gm.rockList)
        {
            if (rockEntry.rock != null && rockEntry.rock.activeInHierarchy && rockEntry.rockInfo.inPlay)
            {
                rocksInPlay.Add(rockEntry.rock);
            }
        }

        List<Vector2> predictedPath = trajectorySimulator.SimulateTrajectory(
            launcherPos,
            initialVelocity,
            isInTurn,
            250,
            rocksInPlay,
            forPlayerPreview: false
        );

        Debug.Log($"[AI_Sweeper] Monitoring started - predicted path has {predictedPath.Count} points");

        // Wait until rock crosses hog line (Y > -16.15)
        while (rock.transform.position.y < -16.15f)
        {
            yield return new WaitForFixedUpdate();
        }

        Debug.Log($"[AI_Sweeper] Rock crossed hog line - sweeping enabled!");

        // Sweeping thresholds
        float lateralErrorThreshold = 0.12f; // 12cm lateral error
        float distanceErrorThreshold = 0.25f; // 25cm distance error
        float predictionLookahead = 1.5f; // Look 1.5 units ahead
        
        // COLLISION AVOIDANCE: Check if trajectory will collide with obstacles
        bool collisionImminent = false;
        float collisionDistance = float.MaxValue;
        Vector2 collisionPoint = Vector2.zero;

        string currentSweepState = "None";

        // Monitor rock until it stops
        while (rockInfo != null && !rockInfo.stopped && rockRB.linearVelocity.magnitude > 0.01f)
        {
            Vector2 currentPos = rock.transform.position;

            // Find predicted position at same Y coordinate
            Vector2 predictedPosAtCurrentY = GetPredictedPositionAtY(predictedPath, currentPos.y);
            Vector2 predictedPosAhead = GetPredictedPositionAtY(predictedPath, currentPos.y + predictionLookahead);

            // Calculate errors
            float lateralError = currentPos.x - predictedPosAtCurrentY.x;
            float distanceToTarget = targetPosition.y - currentPos.y;
            float predictedShortfall = targetPosition.y - predictedPosAhead.y;
            
            // COLLISION LOOKAHEAD: Check if rock will hit obstacles in next 2 meters
            collisionImminent = false;
            float collisionLookaheadDistance = 2.0f; // Check 2m ahead
            
            // Re-simulate from current position to check for imminent collisions
            List<Vector2> lookaheadPath = trajectorySimulator.SimulateTrajectory(
                currentPos,
                rockRB.linearVelocity,
                isInTurn,
                100, // Short sim
                rocksInPlay,
                forPlayerPreview: false
            );
            
            TrajectorySimulator.CollisionInfo lookaheadCollision = trajectorySimulator.GetCollisionInfo();
            
            if (lookaheadCollision.hasCollision)
            {
                // Check if collision is imminent (within lookahead distance)
                collisionDistance = Vector2.Distance(currentPos, lookaheadCollision.collisionPoint);
                
                if (collisionDistance < collisionLookaheadDistance)
                {
                    collisionImminent = true;
                    collisionPoint = lookaheadCollision.collisionPoint;
                    
                    Debug.Log($"[AI_Sweeper] COLLISION IMMINENT! Distance: {collisionDistance:F2}m at {collisionPoint}");
                }
            }

            // Get sweeper skill
            float sweepSkill = GetSweeperSkill();
            float skillMultiplier = 1.0f - (0.3f * (1.0f - sweepSkill)); // Better skill = more aggressive

            // Adjust thresholds based on skill
            float lateralThreshold = lateralErrorThreshold * skillMultiplier;
            float distanceThreshold = distanceErrorThreshold * skillMultiplier;

            // DECISION LOGIC
            string desiredState = "None";

            // OPPONENT ROCK INTERFERENCE: Help them fail!
            if (isOpponentRock)
            {
                // Strategy: DON'T sweep (let their shot fail naturally)
                // Exception: If they're going to succeed AND we can make them overshoot, sweep them OUT
                if (pastTLine && predictedShortfall < 0.5f) // They're on target!
                {
                    // Make them go TOO FAR by sweeping weight
                    desiredState = "Weight";
                    Debug.Log($"[AI_Sweeper] Opponent rock on target - sweeping to make them overshoot!");
                }
                else
                {
                    // They're failing on their own - don't help them!
                    desiredState = "None";
                    Debug.Log($"[AI_Sweeper] Opponent rock failing - doing nothing");
                }
            }
            // YOUR ROCKS: Help them succeed!
            // PRIORITY 0: COLLISION AVOIDANCE (highest priority!)
            else if (collisionImminent && !isOpponentRock)
            {
                // Check if sweeping can help avoid collision
                // If rock is going straight into obstacle, sweep hard to either:
                // 1. Get there faster (reach target before collision)
                // 2. Adjust line to miss obstacle
                
                // Determine if collision is on path to target or off-target
                float collisionOffsetX = collisionPoint.x - targetPosition.x;
                
                if (Mathf.Abs(collisionOffsetX) > 0.3f)
                {
                    // Collision is off-line - try to adjust line to avoid it
                    if (collisionOffsetX > 0f)
                    {
                        // Obstacle is right of target - sweep to pull rock LEFT
                        desiredState = isInTurn ? "Curl" : "Line";
                        Debug.Log($"[AI_Sweeper] Collision avoidance - adjusting line LEFT (obstacle right of target)");
                    }
                    else
                    {
                        // Obstacle is left of target - sweep to push rock RIGHT
                        desiredState = isInTurn ? "Line" : "Curl";
                        Debug.Log($"[AI_Sweeper] Collision avoidance - adjusting line RIGHT (obstacle left of target)");
                    }
                }
                else if (collisionDistance < distanceToTarget * 0.8f)
                {
                    // Collision is on-path and before target - try to get past it faster
                    desiredState = "Critical";
                    Debug.Log($"[AI_Sweeper] Collision avoidance - HARD SWEEP to get past obstacle!");
                }
                else
                {
                    // Collision is on-path and near/past target - can't avoid, just optimize
                    desiredState = "Weight";
                    Debug.Log($"[AI_Sweeper] Collision unavoidable - sweeping for best outcome");
                }
            }
            // PRIORITY 1: CRITICAL DISTANCE (rock won't reach target!)
            else if (predictedShortfall > 1.0f)
            {
                desiredState = "Critical";
            }
            // PRIORITY 2: SIGNIFICANT SHORTFALL
            else if (predictedShortfall > distanceThreshold)
            {
                desiredState = "Weight";
            }
            // PRIORITY 3: LATERAL ERROR
            else if (Mathf.Abs(lateralError) > lateralThreshold)
            {
                if (isInTurn)
                {
                    // IN-TURN curls LEFT (negative X)
                    desiredState = (lateralError > 0f) ? "Line" : "Curl";
                }
                else
                {
                    // OUT-TURN curls RIGHT (positive X)
                    desiredState = (lateralError < 0f) ? "Line" : "Curl";
                }
            }

            // Apply sweeping if state changed
            if (desiredState != currentSweepState)
            {
                ApplySweepState(desiredState, isInTurn);
                currentSweepState = desiredState;

                //switch (desiredState)
                //{
                //    case "None":
                //        TextCalloutManager.Instance.ShowRockCallout(rock, "Whoa!!");
                //        break;
                //    case "Weight":
                //        TextCalloutManager.Instance.ShowRockCallout(rock, "Sweep!!");
                //        break;
                //    case "Line":
                //        TextCalloutManager.Instance.ShowRockCallout(rock, "Line!!");
                //        break;
                //    case "Curl":
                //        TextCalloutManager.Instance.ShowRockCallout(rock, "Curl!!");
                //        break;
                //    case "Critical":
                //        TextCalloutManager.Instance.ShowRockCallout(rock, "HARD!!!");
                //        break;
                //}

                Debug.Log($"[AI_Sweeper] Y={currentPos.y:F2}: State={desiredState}, LateralErr={lateralError:F3}, Shortfall={predictedShortfall:F2}, Collision={collisionImminent}");
            }

            yield return new WaitForFixedUpdate();
        }

        // Rock stopped - whoa
        if (currentSweepState != "None")
        {
            aiSweep.sm.SweepWhoa(true);
            Debug.Log($"[AI_Sweeper] Rock stopped - WHOA");
        }
    }

    /// <summary>
    /// Find predicted position at given Y coordinate
    /// </summary>
    private Vector2 GetPredictedPositionAtY(List<Vector2> predictedPath, float targetY)
    {
        if (predictedPath == null || predictedPath.Count < 2)
            return Vector2.zero;

        // Find two points that bracket the target Y
        for (int i = 0; i < predictedPath.Count - 1; i++)
        {
            Vector2 p1 = predictedPath[i];
            Vector2 p2 = predictedPath[i + 1];

            // Check if target Y is between these two points
            if ((p1.y <= targetY && p2.y >= targetY) || (p1.y >= targetY && p2.y <= targetY))
            {
                // Interpolate X position at target Y
                float t = (targetY - p1.y) / (p2.y - p1.y);
                float interpolatedX = Mathf.Lerp(p1.x, p2.x, t);

                return new Vector2(interpolatedX, targetY);
            }
        }

        // If target Y is beyond predicted path, return last point
        if (predictedPath.Count > 0)
            return predictedPath[predictedPath.Count - 1];

        return Vector2.zero;
    }

    /// <summary>
    /// Apply the desired sweeping state
    /// </summary>
    private void ApplySweepState(string state, bool isInTurn)
    {
        switch (state)
        {
            case "None":
                aiSweep.sm.SweepWhoa(true);
                break;

            case "Weight":
            case "Critical":
                // Both sweepers - maximum distance extension
                aiSweep.sm.SweepWeight(true);
                break;

            case "Line":
                // One sweeper on curl side - straighten the rock
                if (isInTurn)
                    aiSweep.sm.SweepLeft(true);  // IN-TURN: Left sweeper
                else
                    aiSweep.sm.SweepRight(true); // OUT-TURN: Right sweeper
                break;

            case "Curl":
                // One sweeper on opposite side - increase curl
                if (isInTurn)
                    aiSweep.sm.SweepRight(true); // IN-TURN: Right sweeper
                else
                    aiSweep.sm.SweepLeft(true);  // OUT-TURN: Left sweeper
                break;
        }
    }

    /// <summary>
    /// Get combined sweeper skill (0-1 scale)
    /// </summary>
    private float GetSweeperSkill()
    {
        if (aiSweep.sm.swprLStats == null || aiSweep.sm.swprRStats == null)
            return 0.5f; // Default medium skill

        // Combine sweep strength (accuracy) and endurance
        float leftSkill = (aiSweep.sm.swprLStats.sweepStrength.GetValue() / 100f + aiSweep.sm.swprLStats.sweepEndurance.GetValue() / 100f) * 0.5f;
        float rightSkill = (aiSweep.sm.swprRStats.sweepStrength.GetValue() / 100f + aiSweep.sm.swprRStats.sweepEndurance.GetValue() / 100f) * 0.5f;

        // Average both sweepers
        return (leftSkill + rightSkill) * 0.5f;
    }
}
