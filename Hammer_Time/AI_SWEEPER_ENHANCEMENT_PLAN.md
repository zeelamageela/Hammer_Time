# Enhanced AI Sweeper System - Implementation Plan

## ? OVERVIEW

**Goal:** Create a unified, physics-based AI sweeping system that works for ALL shot types by comparing predicted trajectory vs actual rock position in real-time.

**Key Concept:** Instead of hardcoded shot-type rules, the AI sweepers watch the rock and compare its actual path to the predicted trajectory. When the rock deviates (either laterally or in distance), they sweep to correct it.

---

## ???? SWEEPING PHYSICS (From Sweep.cs Analysis)

### 1. **WEIGHT SWEEP** (Both Sweepers)
```csharp
// SweepWeight() coroutine:
rb.linearDamping -= (statCalc * sweepAmt);  // Rock slides FURTHER
rock.GetComponent<Rock_Force>().curl.x += (statCalc * sweepAmt);  // Slight curl INCREASE
```
**Effect:**
- **Primary:** ???? Extends distance (+10-15%)
- **Secondary:** ??? Slight curl increase (~5-10%)
- **Use When:** Rock falling short of target

### 2. **LINE SWEEP** (One Sweeper - Curl Side)
```csharp
// SweepLine() coroutine:
rb.linearDamping -= sweepAmt * statCalc / 4f;  // Some distance (less than weight)
rock.GetComponent<Rock_Force>().curl.x += (statCalc * sweepAmt * 5f);  // BIG curl increase = STRAIGHTENING
```
**Effect:**
- **Primary:** ??? Rock goes STRAIGHTER (reduces curl)
- **Secondary:** ???? Some distance extension (~5-8%)
- **Use When:** Rock curling TOO MUCH (too far right for in-turn, too far left for out-turn)

### 3. **CURL SWEEP** (One Sweeper - Opposite Side)
```csharp
// SweepCurl() coroutine:
rb.linearDamping -= sweepAmt * statCalc / 4f;  // Some distance (less than weight)
rock.GetComponent<Rock_Force>().curl.x -= (sweepAmt * statCalc * 5f);  // BIG curl decrease = MORE CURL
```
**Effect:**
- **Primary:** ??? Rock curls MORE
- **Secondary:** ???? Some distance extension (~5-8%)
- **Use When:** Rock NOT curling enough (needs to move more laterally)

---

## ?? DECISION LOGIC

### Core Algorithm:
```
EVERY FixedUpdate (0.02s):
1. Get current rock position (X, Y)
2. Find predicted position at same Y coordinate from TrajectorySimulator
3. Calculate errors:
   - Lateral Error = Actual X - Predicted X
   - Distance To Target = Target Y - Current Y
   - Predicted Shortfall = Target Y - (Predicted Final Y)

4. PRIORITY 1: CRITICAL DISTANCE
   IF predicted shortfall > 1.0 units:
      ? WEIGHT SWEEP (emergency!)
      
5. PRIORITY 2: SIGNIFICANT SHORTFALL
   IF predicted shortfall > distanceThreshold (0.25 units):
      ? WEIGHT SWEEP
      
6. PRIORITY 3: LATERAL ERROR
   IF abs(lateral error) > lateralThreshold (0.12 units):
      IF rock too far right (positive error):
         ? LINE SWEEP (straighten)
      ELSE IF rock too far left (negative error):
         ? CURL SWEEP (more curl)
         
7. PRIORITY 4: ON TRACK
   IF no errors:
      ? WHOA (don't sweep)
```

### Turn Direction Logic:
```
IN-TURN (curls RIGHT):
  - Rock right of predicted ? LINE sweep (straighten)
  - Rock left of predicted ? CURL sweep (more curl)
  
OUT-TURN (curls LEFT):
  - Rock left of predicted ? LINE sweep (straighten)
  - Rock right of predicted ? CURL sweep (more curl)
```

---

## ?? IMPLEMENTATION STEPS

### Step 1: Modify AI_Shooter.cs

**Location:** `IEnumerator Shot()` method, after `rockFlick.mouseUp = true`

**Add this code:**
```csharp
// Wait for rock to actually be released and have velocity
yield return new WaitForFixedUpdate();
yield return new WaitForFixedUpdate();

// Start AI sweeping coroutine
if (gm != null && rm != null && sm != null)
{
    Vector2 initialVelocity = rockRB.linearVelocity;
    Vector2 targetPosition = aiTarg.targetPos;
    bool isInTurn = inturn;
    
    Debug.Log($"[AI_Shooter] Starting sweeping monitor: velocity={initialVelocity.magnitude:F2} m/s, target={targetPosition}, inTurn={isInTurn}");
    
    StartCoroutine(MonitorAndSweepCoroutine(rockRB, initialVelocity, isInTurn, targetPosition, aiShotType));
}
```

### Step 2: Add Monitoring Coroutine to AI_Shooter.cs

**Add this entire method to AI_Shooter class:**

```csharp
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
        
        // Get sweeper skill
        float sweepSkill = GetSweeperSkill();
        float skillMultiplier = 1.0f - (0.3f * (1.0f - sweepSkill)); // Better skill = more aggressive
        
        // Adjust thresholds based on skill
        float lateralThreshold = lateralErrorThreshold * skillMultiplier;
        float distanceThreshold = distanceErrorThreshold * skillMultiplier;
        
        // DECISION LOGIC
        string desiredState = "None";
        
        // PRIORITY 1: CRITICAL DISTANCE (rock won't reach target!)
        if (predictedShortfall > 1.0f)
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
                // IN-TURN curls RIGHT
                desiredState = (lateralError > 0f) ? "Line" : "Curl";
            }
            else
            {
                // OUT-TURN curls LEFT
                desiredState = (lateralError < 0f) ? "Line" : "Curl";
            }
        }
        
        // Apply sweeping if state changed
        if (desiredState != currentSweepState)
        {
            ApplySweepState(desiredState, isInTurn);
            currentSweepState = desiredState;
            
            Debug.Log($"[AI_Sweeper] Y={currentPos.y:F2}: State={desiredState}, LateralErr={lateralError:F3}, Shortfall={predictedShortfall:F2}");
        }
        
        yield return new WaitForFixedUpdate();
    }
    
    // Rock stopped - whoa
    if (currentSweepState != "None")
    {
        sm.SweepWhoa(true);
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
            sm.SweepWhoa(true);
            break;
            
        case "Weight":
        case "Critical":
            // Both sweepers - maximum distance extension
            sm.SweepWeight(true);
            break;
            
        case "Line":
            // One sweeper on curl side - straighten the rock
            if (isInTurn)
                sm.SweepLeft(true);  // IN-TURN: Left sweeper
            else
                sm.SweepRight(true); // OUT-TURN: Right sweeper
            break;
            
        case "Curl":
            // One sweeper on opposite side - increase curl
            if (isInTurn)
                sm.SweepRight(true); // IN-TURN: Right sweeper
            else
                sm.SweepLeft(true);  // OUT-TURN: Left sweeper
            break;
    }
}

/// <summary>
/// Get combined sweeper skill (0-1 scale)
/// </summary>
private float GetSweeperSkill()
{
    if (sm.swprLStats == null || sm.swprRStats == null)
        return 0.5f; // Default medium skill
    
    // Combine sweep strength (accuracy) and endurance
    float leftSkill = (sm.swprLStats.sweepStrength.GetValue() / 100f + sm.swprLStats.sweepEndurance.GetValue() / 100f) * 0.5f;
    float rightSkill = (sm.swprRStats.sweepStrength.GetValue() / 100f + sm.swprRStats.sweepEndurance.GetValue() / 100f) * 0.5f;
    
    // Average both sweepers
    return (leftSkill + rightSkill) * 0.5f;
}
```

---

## ?? TUNING PARAMETERS

### Thresholds (adjust based on testing):
```csharp
float lateralErrorThreshold = 0.12f;  // When to sweep for line/curl
float distanceErrorThreshold = 0.25f; // When to sweep for weight
float predictionLookahead = 1.5f;     // How far ahead to predict
```

### Skill Impact:
```csharp
float skillMultiplier = 1.0f - (0.3f * (1.0f - sweepSkill));
// sweepSkill = 1.0 (elite) ? multiplier = 1.0 (full aggression)
// sweepSkill = 0.5 (avg) ? multiplier = 0.85 (85% aggression)
// sweepSkill = 0.0 (rookie) ? multiplier = 0.7 (70% aggression)
```

---

## ?? OPPONENT ROCK SWEEPING (Behind T-Line)

**Goal:** Sweep opponent rocks PAST their intended target (make them heavy)

**Implementation (Add to monitoring coroutine):**

```csharp
// At start of MonitorAndSweepCoroutine(), check if this is opponent's rock:
bool isOpponentRock = (rockInfo.teamName != gm.rockList[currentRockNumber].rockInfo.teamName);
bool pastTLine = (rock.transform.position.y > 6.5f);

// Modified decision logic:
if (isOpponentRock && pastTLine)
{
    // STRATEGY: Make opponent rock go TOO FAR
    // Always sweep for weight to push it past their target
    desiredState = "Weight";
    
    Debug.Log($"[AI_Sweeper] Opponent rock past T-line - sweeping to overshoot!");
}
```

---

## ?? TESTING PLAN

### Test 1: Draw Shots
```
Setup: AI draws to button
Expected: 
- Rock falling short ? Weight sweep activates
- Rock on track ? No sweeping
- Rock curling too much ? Line sweep activates
```

### Test 2: Takeout Shots
```
Setup: AI takes out opponent rock
Expected:
- Rock falling short ? Weight sweep
- Rock drifting off target ? Line/Curl sweep
```

### Test 3: Skill Scaling
```
Setup: Play with 90% skill sweepers vs 50% skill sweepers
Expected:
- Elite sweepers react earlier (lower thresholds)
- Rookie sweepers react later (higher thresholds)
```

### Test 4: Opponent Sweeping
```
Setup: Opponent draws behind button
Expected:
- Once rock crosses T-line (Y > 6.5), AI sweeps it
- Rock goes too far (beyond their target)
```

---

## ?? ADVANTAGES OF THIS SYSTEM

1. **? Unified:** Works for ALL shot types (draws, takeouts, guards)
2. **? Physics-Based:** Uses same trajectory prediction as targeting
3. **? Real-Time:** Reacts to actual rock behavior
4. **? Skill-Scaled:** Better sweepers = more aggressive/accurate decisions
5. **? Strategic:** Can sweep opponent rocks to worsen their position
6. **? Realistic:** Matches real curling sweeping decisions

---

## ?? EXPECTED LOGS

```
[AI_Shooter] Starting sweeping monitor: velocity=10.5 m/s, target=(0.00, 6.50), inTurn=True
[AI_Sweeper] Monitoring started - predicted path has 187 points
[AI_Sweeper] Rock crossed hog line - sweeping enabled!
[AI_Sweeper] Y=-15.2: State=None, LateralErr=-0.03, Shortfall=21.70
[AI_Sweeper] Y=-10.5: State=Weight, LateralErr=-0.05, Shortfall=0.35
[AI_Sweeper] Y=-5.2: State=Line, LateralErr=0.15, Shortfall=0.12
[AI_Sweeper] Y=2.5: State=None, LateralErr=0.08, Shortfall=-0.05
[AI_Sweeper] Rock stopped - WHOA
```

---

## ?? DEBUGGING TIPS

### If sweepers never activate:
- Check thresholds (may be too high)
- Verify predicted path is generating
- Check rock has valid Rock_Info component

### If sweepers activate too much:
- Increase thresholds (0.12 ? 0.18 for lateral)
- Reduce skill multiplier impact

### If sweepers choose wrong direction:
- Verify isInTurn is correct
- Check lateral error sign (positive = right, negative = left)
- Verify curl direction (in-turn = right, out-turn = left)

---

## ?? FUTURE ENHANCEMENTS

1. **Distance-Based Weighting:**
   - Sweep more aggressively when close to target
   - Less aggressive when far from target

2. **Opponent Rock Intelligence:**
   - Check WHERE opponent rock is going
   - Only sweep if it makes position worse for them

3. **Multi-Rock Scenarios:**
   - Detect if rock will hit guard
   - Sweep to avoid hitting own guards
   - Sweep to hit opponent guards

4. **Endurance Tracking:**
   - Track how much sweepers have swept
   - Reduce effectiveness when tired
   - Choose between weight/line based on remaining energy

---

## ?? SUMMARY

**What This Does:**
- ? Unifies ALL shot types into ONE sweeping logic
- ? Uses physics prediction (same as targeting)
- ? Reacts intelligently to rock behavior
- ? Scales with sweeper skill
- ? Can sweep opponent rocks strategically

**What You Need To Do:**
1. Add monitoring coroutine to AI_Shooter.cs
2. Call it after rock is released
3. Test with different shot types
4. Tune thresholds based on results

**Estimated Implementation Time:** 30-60 minutes
**Estimated Testing Time:** 1-2 hours

Good luck! ????
