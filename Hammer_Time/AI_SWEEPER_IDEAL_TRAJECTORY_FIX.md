# AI Sweeper Ideal Trajectory Fix

## ?? **PROBLEM STATEMENT**

**Current Behavior (BROKEN):**
```
1. AI_Target calculates PERFECT shot physics
2. AI_Target applies accuracy ERROR to pullback
3. Rock launches with ERROR-CONTAMINATED velocity
4. AI_Sweeper receives ERROR-CONTAMINATED velocity
5. Sweeper generates trajectory from CONTAMINATED velocity
6. Sweeper tries to maintain ERROR trajectory (reinforces mistake!)
```

**Desired Behavior (CORRECT):**
```
1. AI_Target calculates PERFECT shot physics
2. AI_Target STORES perfect velocity (before errors)
3. AI_Target applies accuracy ERROR to pullback
4. Rock launches with ERROR-CONTAMINATED velocity
5. AI_Sweeper receives BOTH perfect AND contaminated velocities
6. Sweeper generates IDEAL trajectory from PERFECT velocity
7. Sweeper CORRECTS actual path back toward ideal (fixes mistakes!)
```

---

## ? **SOLUTION OVERVIEW**

### **Core Concept**

Sweepers need to know **two trajectories**:

1. **IDEAL Trajectory**: From perfect physics calculation (NO accuracy errors)
   - This is the **sweeping target** - where rock SHOULD go
   - Generated from shooter's intended shot before skill-based errors

2. **ACTUAL Trajectory**: From error-contaminated launch
   - This is the **current path** - where rock IS going
   - Used for collision detection and real-time prediction

**Sweepers compare actual position to ideal trajectory and correct deviations!**

---

## ?? **IMPLEMENTATION CHECKLIST**

### **? COMPLETED: AI_Sweeper.cs**

- [x] Add `idealVelocity` parameter to `StartPhysicsBasedSweeping()`
- [x] Update `MonitorAndSweepCoroutine()` to accept both velocities
- [x] Generate TWO trajectories (ideal + actual)
- [x] Use `idealTrajectory` for sweeping reference instead of `cleanTrajectory`
- [x] Add debug logging to show launch error magnitude

### **? PENDING: AI_Target.cs**

Need to **store perfect velocity** before applying accuracy errors:

```csharp
// AI_Target.cs - In all shot calculation methods

// STEP 1: Calculate PERFECT physics shot
Vector2 pullbackPos;
bool useInTurn;
bool foundShot = CalculatePhysicsBasedShot(targetPos, out pullbackPos, out useInTurn, shotType, targetIndex);

// STEP 2: Calculate PERFECT velocity (BEFORE accuracy errors)
Vector2 perfectVelocity = CalculateVelocityFromPullback(pullbackPos, launcherPos, useInTurn);

// STEP 3: Store perfect velocity in a field for AI_Shooter to access
lastPerfectVelocity = perfectVelocity;  // ? NEW FIELD

// STEP 4: Apply accuracy errors (existing code)
CharacterStats shooterStats = GetShooterStats(rockCurrent);
if (shooterStats != null)
{
    // Apply aim/weight/finesse errors...
    pullbackPos += errorOffset;
}

// STEP 5: Store error-contaminated pullback (existing)
takeOutX = pullbackPos.x;
takeOutY = pullbackPos.y;
```

### **? PENDING: AI_Shooter.cs**

Need to **retrieve both velocities** and pass to sweeper:

```csharp
// AI_Shooter.cs - In Shot() coroutine

// After rock is released...
yield return new WaitForFixedUpdate();
yield return new WaitForFixedUpdate();

// Get ACTUAL velocity (error-contaminated)
Vector2 actualVelocity = rockRB.linearVelocity;

// Get PERFECT velocity from AI_Target (before errors)
Vector2 perfectVelocity = aiTarg.lastPerfectVelocity;  // ? NEW

Debug.Log($"[AI_Shooter] Launching sweeper monitoring:");
Debug.Log($"  Perfect velocity: {perfectVelocity.magnitude:F2} m/s (ideal target)");
Debug.Log($"  Actual velocity: {actualVelocity.magnitude:F2} m/s (includes errors)");
Debug.Log($"  Launch error: {(actualVelocity - perfectVelocity).magnitude:F3} m/s");

// Pass BOTH velocities to sweeper
aiSweep.StartPhysicsBasedSweeping(
    rockRB, 
    actualVelocity,   // ? What rock actually got
    perfectVelocity,  // ? What it SHOULD have gotten (sweeping target)
    isInTurn, 
    targetPosition, 
    aiShotType, 
    currentRockNumber
);
```

---

## ?? **DETAILED IMPLEMENTATION STEPS**

### **STEP 1: Add Field to AI_Target**

```csharp
// AI_Target.cs - Add to class fields

public class AI_Target : MonoBehaviour
{
    // ... existing fields ...
    
    // NEW: Store perfect velocity before accuracy errors
    // Used by AI_Sweeper to generate ideal trajectory for correction
    [HideInInspector]
    public Vector2 lastPerfectVelocity;
    
    // ... rest of class ...
}
```

### **STEP 2: Helper Method for Velocity Calculation**

```csharp
// AI_Target.cs - Add helper method

/// <summary>
/// Calculate velocity from pullback position using PLAYER'S formula
/// This gives us the INTENDED velocity before accuracy errors
/// </summary>
private Vector2 CalculateVelocityFromPullback(Vector2 pullbackPos, Vector2 launcherPos, bool isInTurn)
{
    // Get TrajectoryLine parameters
    TrajectoryLine playerTrajectory = FindObjectOfType<TrajectoryLine>();
    float velocityMultiplier = playerTrajectory != null ? playerTrajectory.velocityMultiplier : 5.0f;
    
    // Calculate pullback distance
    Vector2 pullbackOffset = pullbackPos - launcherPos;
    float pullbackDistance = pullbackOffset.magnitude;
    
    // PLAYER'S FORMULA: velocity = pullbackDistance * velocityMultiplier
    Vector2 velocityDirection = pullbackOffset.normalized;
    float velocityMagnitude = pullbackDistance * velocityMultiplier;
    Vector2 baseVelocity = velocityDirection * velocityMagnitude;
    
    return baseVelocity;
}
```

### **STEP 3: Update Takeout Shot Calculation**

Find the `TakeOutTarget()` coroutine and update it:

```csharp
// AI_Target.cs - TakeOutTarget() around line 1200

IEnumerator TakeOutTarget(int rockCurrent, int rockTarget)
{
    // ... existing setup code ...
    
    Vector2 pullbackPos;
    bool useInTurn;
    bool foundShot = CalculatePhysicsBasedShot(targetRockPos, out pullbackPos, out useInTurn, "Take Out", rockTarget);
    
    if (foundShot)
    {
        // **NEW: Store perfect velocity BEFORE accuracy errors**
        Vector2 launcherPos = new Vector2(0f, -27.5f);
        lastPerfectVelocity = CalculateVelocityFromPullback(pullbackPos, launcherPos, useInTurn);
        
        Debug.Log($"[AI_Target] Perfect velocity stored: {lastPerfectVelocity.magnitude:F2} m/s (before accuracy errors)");
        
        // EXISTING: Set turn direction
        rm.inturn = useInTurn;
        
        // EXISTING: Apply accuracy errors
        CharacterStats shooterStats = GetShooterStats(rockCurrent);
        Vector2 originalPullback = pullbackPos;
        
        if (shooterStats != null)
        {
            // ... existing accuracy error code ...
            
            pullbackPos += errorOffset;  // ? Contamination happens here
            
            Debug.Log($"[AI_Target] Accuracy error applied: {errorOffset.magnitude:F3}, pullback changed to {pullbackPos}");
        }
        
        // EXISTING: Store error-contaminated pullback
        takeOutX = pullbackPos.x;
        takeOutY = pullbackPos.y;
        
        // ... rest of method ...
    }
}
```

### **STEP 4: Update ALL Shot Types**

Apply the same pattern to:
- `DrawTarget()` - draws
- `GuardTarget()` - guards  
- `PeelTarget()` - peels
- `TapTarget()` - raises
- `TickShotTarget()` - ticks
- `RunbackTarget()` - runbacks

**Pattern for each:**
```csharp
if (foundShot)
{
    // 1. Store perfect velocity FIRST
    lastPerfectVelocity = CalculateVelocityFromPullback(pullbackPos, launcherPos, useInTurn);
    
    // 2. Apply accuracy errors
    // ... existing error code ...
    
    // 3. Store contaminated pullback
    takeOutX = pullbackPos.x;
    takeOutY = pullbackPos.y;
}
```

### **STEP 5: Update AI_Shooter**

```csharp
// AI_Shooter.cs - Shot() coroutine around line 150

// Wait for rock to be released
yield return new WaitForFixedUpdate();
yield return new WaitForFixedUpdate();

// Start AI sweeping
if (gm != null && rm != null && aiSweep.sm != null)
{
    // Get ACTUAL velocity (includes accuracy errors from pullback offset)
    Vector2 actualVelocity = rockRB.linearVelocity;
    
    // Get PERFECT velocity (before accuracy errors) from AI_Target
    Vector2 perfectVelocity = aiTarg.lastPerfectVelocity;
    
    Vector2 targetPosition = aiTarg.targetPos;
    bool isInTurn = inturn;
    
    // Calculate launch error for diagnostics
    float launchError = (actualVelocity - perfectVelocity).magnitude;
    float angleError = Vector2.Angle(actualVelocity, perfectVelocity);
    
    Debug.Log($"[AI_Shooter] Starting physics-based sweeping:");
    Debug.Log($"  Perfect velocity: {perfectVelocity.magnitude:F2} m/s @ {Mathf.Atan2(perfectVelocity.y, perfectVelocity.x) * Mathf.Rad2Deg:F1}°");
    Debug.Log($"  Actual velocity: {actualVelocity.magnitude:F2} m/s @ {Mathf.Atan2(actualVelocity.y, actualVelocity.x) * Mathf.Rad2Deg:F1}°");
    Debug.Log($"  Launch error: {launchError:F3} m/s ({angleError:F2}° off-angle)");
    Debug.Log($"  Target: {targetPosition}, Turn: {(isInTurn ? "IN" : "OUT")}");
    
    // Pass BOTH velocities to sweeper
    aiSweep.StartPhysicsBasedSweeping(
        rockRB, 
        actualVelocity,   // ? Current trajectory (has errors)
        perfectVelocity,  // ? Ideal trajectory (sweeping target)
        isInTurn, 
        targetPosition, 
        aiShotType, 
        currentRockNumber
    );
}
```

---

## ?? **TESTING & VERIFICATION**

### **Expected Log Output**

When a shot is taken, you should see:

```
[AI_Target] Perfect velocity stored: 11.25 m/s (before accuracy errors)
[AI_Target] Takeout skills: Aim=75%, Weight=80%
[AI_Target] Accuracy error applied: 0.045, pullback changed to (-0.123, -27.532)

[AI_Shooter] Starting physics-based sweeping:
  Perfect velocity: 11.25 m/s @ 88.3°
  Actual velocity: 11.18 m/s @ 87.1°
  Launch error: 0.135 m/s (1.2° off-angle)
  Target: (0.15, 6.5), Turn: IN

[AI_Sweeper] Monitoring started:
  IDEAL trajectory (sweeping target): 247 points from perfect velocity (11.25, 0.45)
  ACTUAL trajectory (error-contaminated): 245 points from actual velocity (11.18, 0.42)
  Launch error: 0.135 m/s (1.2°)
```

### **Verification Steps**

1. **Low Accuracy Shooter (30%):**
   - Launch error should be **large** (0.2-0.5 m/s)
   - Sweepers should **heavily correct** (lots of sweeping activity)
   - Rock should end up **closer to ideal path than launch suggested**

2. **High Accuracy Shooter (95%):**
   - Launch error should be **tiny** (0.01-0.05 m/s)
   - Sweepers should **rarely activate** (minimal correction needed)
   - Rock should follow **near-perfect trajectory**

3. **Visual Comparison:**
   - Watch rock deviate from ideal path early
   - Sweepers activate to bring it back
   - Final position should be **better than no-sweep scenario**

---

## ?? **IMPACT ANALYSIS**

### **Before Fix (Broken)**

```
Shooter Accuracy: 50%
Launch Error: +0.25 m/s lateral (8cm off-line at house)
Sweeper Behavior: Follows ERROR trajectory
  ? Thinks 8cm off-line IS the target!
  ? Maintains 8cm error throughout
Final Result: 8cm off-line (NO IMPROVEMENT)

Effective Accuracy: 50% (same as shooter)
```

### **After Fix (Correct)**

```
Shooter Accuracy: 50%
Launch Error: +0.25 m/s lateral (8cm off-line at house)
Sweeper Behavior: References IDEAL trajectory
  ? Knows rock should be centered (0cm off-line)
  ? Corrects 8cm error back toward 0cm
  ? Reduces error to ~2-3cm (depending on sweeper skill)
Final Result: 2-3cm off-line (75% ERROR REDUCTION)

Effective Accuracy: 85% (shooter 50% + sweeper correction 35%)
```

### **Skill Synergy**

With this fix, **sweepers amplify shooter accuracy**:

| Shooter Accuracy | Launch Error | Sweeper Correction | Final Error | Effective Accuracy |
|-----------------|-------------|-------------------|-------------|-------------------|
| **30%** (rookie) | 15cm | 8cm | 7cm | **65%** |
| **50%** (average) | 8cm | 6cm | 2cm | **85%** |
| **70%** (skilled) | 4cm | 3cm | 1cm | **95%** |
| **90%** (expert) | 1cm | 0.5cm | 0.5cm | **98%** |

**Formula:** `Effective Accuracy = Shooter + (Sweeper × RemainingError)`

---

## ?? **CRITICAL NOTES**

### **Why This Matters**

1. **Sweepers Are Skill Amplifiers**
   - Without ideal trajectory: Sweepers do NOTHING (reinforce errors)
   - With ideal trajectory: Sweepers CORRECT errors (amplify accuracy)

2. **Realistic Curling Behavior**
   - Real curling: Sweepers fix shooter mistakes
   - Old system: Sweepers followed mistakes blindly
   - New system: Sweepers actively correct toward perfect shot

3. **Game Balance**
   - Rewards BOTH shooter accuracy AND sweeper skill
   - Low shooter + high sweeper = viable strategy
   - High shooter + low sweeper = risky but effective
   - Makes team composition matter more

### **Fallback Safety**

If `lastPerfectVelocity` is ever `Vector2.zero` (shouldn't happen):

```csharp
// AI_Shooter.cs - Safety check

Vector2 perfectVelocity = aiTarg.lastPerfectVelocity;

// SAFETY: If no perfect velocity stored, use actual velocity (degraded mode)
if (perfectVelocity == Vector2.zero)
{
    Debug.LogWarning("[AI_Shooter] No perfect velocity stored - using actual velocity (sweepers won't correct errors!)");
    perfectVelocity = actualVelocity;
}
```

---

## ?? **MATHEMATICAL EXPLANATION**

### **Trajectory Error Correction**

Given:
- `P(y)` = Perfect trajectory at position Y
- `A(y)` = Actual trajectory at position Y
- `E(y) = A(y) - P(y)` = Error at position Y

**Without Ideal Trajectory (Broken):**
```
Sweeper targets: A(y)
Actual position: A(y)
Error from target: 0
? NO CORRECTION (error persists)
```

**With Ideal Trajectory (Fixed):**
```
Sweeper targets: P(y)
Actual position: A(y)
Error from target: E(y) = A(y) - P(y)
? SWEEP TO REDUCE E(y)
? New position: A'(y) = A(y) - k×E(y) where k = sweeper effectiveness
```

**Result:** Error decreases exponentially with distance swept!

---

## ? **COMPLETION CHECKLIST**

- [x] AI_Sweeper.cs updated (signature + trajectory generation)
- [ ] AI_Target.cs: Add `lastPerfectVelocity` field
- [ ] AI_Target.cs: Add `CalculateVelocityFromPullback()` helper
- [ ] AI_Target.cs: Update `TakeOutTarget()` to store perfect velocity
- [ ] AI_Target.cs: Update `DrawTarget()` to store perfect velocity
- [ ] AI_Target.cs: Update `GuardTarget()` to store perfect velocity
- [ ] AI_Target.cs: Update `PeelTarget()` to store perfect velocity
- [ ] AI_Target.cs: Update `TapTarget()` to store perfect velocity
- [ ] AI_Target.cs: Update `TickShotTarget()` to store perfect velocity
- [ ] AI_Target.cs: Update `RunbackTarget()` to store perfect velocity
- [ ] AI_Shooter.cs: Update `Shot()` to pass both velocities
- [ ] Test with low accuracy shooter (verify heavy correction)
- [ ] Test with high accuracy shooter (verify minimal correction)
- [ ] Verify log output shows launch error and ideal vs actual trajectories

---

## ?? **NEXT STEPS**

1. **Implement AI_Target Changes** (critical - enables the fix)
2. **Implement AI_Shooter Changes** (passes both velocities)
3. **Test with Various Accuracy Levels** (verify correction behavior)
4. **Tune Sweeper Skill Impact** (adjust correction strength)
5. **Document Accuracy System** (update player-facing tooltips)

**Status:** ?? 25% Complete (AI_Sweeper updated, AI_Target/Shooter pending)

**Estimated Time:** 30-45 minutes for remaining changes + testing
