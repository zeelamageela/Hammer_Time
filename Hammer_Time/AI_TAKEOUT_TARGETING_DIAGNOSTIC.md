# AI Takeout Targeting Diagnostic Guide

## Problem
AI takeouts are not hitting targets consistently even after fixing turn synchronization.

## Diagnostic Steps

### 1. Check Console Logs

When an AI takeout happens, look for these log messages:

**Success Case:**
```
[AI_Target] Take Out SUCCESS - Score: 8.45, Pullback: (0.15, -27.2), InTurn: false, Target: (0.3, 6.5)
[AI_Shooter] Locked flipAxis = false for Take Out
```

**Failure Case:**
```
[AI_Target] Take Out FAILED - No valid shot found! BestScore: -5.23, Target: (0.5, 7.0)
[AI_Target] Fallback - Using existing turn state: OUT-TURN
[AI_Shooter] Locked flipAxis = false for Take Out
```

### 2. Understand the Score

The score tells you how good the shot is:
- **Score > 5**: Direct hit on target rock ?
- **Score 0-5**: Missed target but got close
- **Score < 0**: Way off target or hit wrong rock ?

### 3. Common Problems

#### Problem A: Physics Calculation Failing
**Symptoms:** You see "FAILED - No valid shot found" frequently
**Cause:** Search range too small or target unreachable
**Fix:** Increase `maxOffset` in `CalculatePhysicsBasedShot()`

#### Problem B: Wrong Rock Being Hit
**Symptoms:** Score is positive but wrong rock moves
**Cause:** Collision detection hitting obstacle instead of target
**Fix:** Adjust collision scoring penalties

#### Problem C: Shot Too Weak
**Symptoms:** Rock doesn't reach target, stops short
**Cause:** `speedMultiplier = 0.32f` too low
**Fix:** Increase to `0.35f` or `0.38f`

#### Problem D: Shot Too Strong
**Symptoms:** Rock blows through target, doesn't make contact
**Cause:** `speedMultiplier` too high
**Fix:** Decrease to `0.28f` or `0.30f`

## Quick Fixes to Try

### Fix 1: Increase Search Range
In `AI_Target.cs`, line ~280:

```csharp
// BEFORE (current)
else if (shotType == "Take Out" || shotType == "Peel")
{
    maxOffset = 1.0f;
    offsetStep = 0.025f;
}

// AFTER (wider search)
else if (shotType == "Take Out" || shotType == "Peel")
{
    maxOffset = 1.5f;   // 50% wider search
    offsetStep = 0.02f;  // Finer resolution
}
```

### Fix 2: Adjust Speed Multiplier
In `AI_Target.cs`, line ~265:

```csharp
// BEFORE (current)
case "Take Out":
    requireDirectHit = true;
    speedMultiplier = 0.32f;
    break;

// AFTER (slightly more power)
case "Take Out":
    requireDirectHit = true;
    speedMultiplier = 0.35f;  // +10% power
    break;
```

### Fix 3: Looser Hit Detection
In `AI_Target.cs`, line ~335:

```csharp
// BEFORE (current)
if (hitDistance < 0.3f)
{
    score += 10f; // Big bonus for direct hit
}

// AFTER (more forgiving)
if (hitDistance < 0.4f)  // Wider hit radius
{
    score += 10f;
}
```

### Fix 4: Remove Accuracy Error for Testing
In `AI_Shooter.cs`, line ~432 (takeout case):

```csharp
// TEMPORARILY COMMENT OUT ACCURACY ERROR
if (takeOutX != 0f)
{
    CharacterStats stats = GetShooterStats();
    if (stats != null)
    {
        float accuracy = stats.takeOutAccuracy.GetValue();
        Vector2 error = GetAccuracyError(accuracy, 0.18f);
        
        shotX = takeOutX; // + error.x;  // COMMENTED OUT
        shotY = takeOutY; // + error.y;  // COMMENTED OUT
    }
```

This will tell you if the physics calculation is accurate but the accuracy error is throwing it off.

## Advanced Debugging

### Enable Verbose Logging

Add this at the top of `CalculatePhysicsBasedShot()` in `AI_Target.cs`:

```csharp
private bool CalculatePhysicsBasedShot(Vector2 targetRockPosition, out Vector2 pullbackPosition, out bool useInTurn, string shotType = "Take Out")
{
    Vector2 launcherPos = new Vector2(0f, -25f);
    
    // === ADD VERBOSE LOGGING ===
    Debug.Log($"[AI_Target] Starting {shotType} calculation for target at {targetRockPosition}");
    int attemptCount = 0;
    // ===========================
    
    // ... rest of method
```

And inside the offset loop:

```csharp
for (float xOffset = -maxOffset; xOffset <= maxOffset; xOffset += offsetStep)
{
    attemptCount++;
    
    // ... after calculating score ...
    
    if (score > bestScore)
    {
        Debug.Log($"[AI_Target] Attempt {attemptCount}: NEW BEST - Score: {score:F2}, Offset: {xOffset:F3}, InTurn: {tryInTurn}");
        bestScore = score;
        bestPullback = testPullback;
        bestInTurn = tryInTurn;
    }
}
```

This will show you every attempt and which one wins.

## What To Report Back

After testing, please tell me:

1. **How often does the physics calculation succeed vs fail?**
   - Look for "SUCCESS" vs "FAILED" in console

2. **What are the typical scores for successful shots?**
   - Look at the number after "Score:" in the logs

3. **What happens when it misses?**
   - Does it hit the wrong rock?
   - Does it stop short of the target?
   - Does it go too far past the target?

4. **Does removing accuracy error help?**
   - If yes, the physics is right but error is too high
   - If no, the physics calculation itself needs work

## Expected Behavior

For a well-tuned takeout:
- Physics calculation should **succeed 95%+ of the time**
- Scores should be **> 5** for direct hits
- With 80-100 accuracy stat, should hit target **80-95% of the time**
- With 50-70 accuracy stat, should hit target **60-75% of the time**

## Next Steps

Based on your findings, I can:
1. Adjust the search parameters
2. Fix the scoring algorithm
3. Tune the speed/power settings
4. Improve collision detection
5. Adjust accuracy error distribution

Let me know what you see in the logs!
