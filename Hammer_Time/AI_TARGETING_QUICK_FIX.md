# AI Targeting Quick Fix - Post Curl Convention Change

## Current Status
? **Trajectory Physics**: PERFECT - deterministic and accurate
? **Player Preview**: PERFECT - matches actual rock path
?? **AI Targeting**: BROKEN - using old curl convention
?? **AI Draw Shots**: BROKEN - using old curl convention

## Why AI is Broken

The AI's physics simulation uses the **same TrajectorySimulator** that we just fixed, BUT:
1. AI was tuned for **OLD convention** (out-turn ? RIGHT)
2. We inverted to **NEW convention** (out-turn ? LEFT)
3. AI parameters need adjustment to compensate

## The Good News

Since AI uses `TrajectorySimulator` (same as player), we **don't need to change the AI's physics code** - just tune the parameters!

## Quick Fix - Phase 1: Immediate Testing

### Step 1: Match Player Curl Strength (5 minutes)

**File**: `Assets\Scripts\AI\AI_Target.cs` (Line ~23)

```csharp
// OLD VALUE (tuned for wrong convention)
public float curlStrength = 13.8f;

// NEW VALUE (match player trajectory)
public float curlStrength = 0.25f; // SAME as TrajectoryLine.cs!
```

**Why**: AI should use **identical physics** to player for consistency. The player trajectory is PERFECT now, so AI should match it exactly.

### Step 2: Test with QuickTestGame (10 minutes)

```csharp
// In QuickTestGame.cs or your test setup
gsp.aiTeamRed = true;
gsp.aiTeamYellow = false;

// Place a rock at button for AI to hit
Debug_Placement.PlaceRockAt(new Vector2(0f, 6.5f));
```

**What to Watch:**
```
Console Output:
[AI_Target] Take Out SUCCESS - InTurn: false (out-turn)
[Physics] Shooting at target: (0.0, 6.5)
[Actual Hit] Rock stopped at: (?, ?)
```

**Measure lateral error**:
- If hit is **LEFT** of target ? AI curl too strong ? decrease `curlStrength`
- If hit is **RIGHT** of target ? AI curl too weak ? increase `curlStrength`
- Goal: Within 0.05 units (5cm) of target

## Expected Results

### Scenario 1: Perfect Match (Best Case)
```
Target: (0.0, 6.5)
AI Shot: Out-turn
Actual Hit: (-0.02, 6.48)
Error: 2cm LEFT ? ACCEPTABLE!
```
**Action**: No further tuning needed! AI is using player physics correctly.

### Scenario 2: Slight Overcurl (Likely)
```
Target: (0.0, 6.5)
AI Shot: Out-turn
Actual Hit: (-0.12, 6.52)
Error: 12cm LEFT ? Too much curl
```
**Action**: Reduce curl slightly
```csharp
public float curlStrength = 0.20f; // Reduced from 0.25f
```

### Scenario 3: Undercurl (Possible)
```
Target: (0.0, 6.5)
AI Shot: Out-turn
Actual Hit: (0.08, 6.48)
Error: 8cm RIGHT ? Not enough curl
```
**Action**: Increase curl slightly
```csharp
public float curlStrength = 0.30f; // Increased from 0.25f
```

## Draw Shots - Special Considerations

Draw shots are **more sensitive to curl** because:
1. Longer trajectory = more time for curl to accumulate
2. Target is stationary (no collision adjustments)
3. Precision matters more (need to land in house, not just hit a rock)

### Draw Shot Testing

**Setup:**
```csharp
// Clear all rocks from house
// AI attempts draw to button (0, 6.5)
aiTarget.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
```

**What to Check:**
- Lateral accuracy (X position within 0.1 units)
- Distance accuracy (Y position within 0.2 units)
- Turn selection (does AI choose correct turn for approach?)

### Draw Shot Parameters (if needed)

If draws are less accurate than takeouts, add separate draw curl:

```csharp
[Header("Shot-Specific Curl")]
[Tooltip("Curl strength for draw shots (longer trajectory = different tuning)")]
public float drawCurlStrength = 0.25f;

[Tooltip("Curl strength for collision shots (takeouts, peels, etc.)")]
public float collisionCurlStrength = 0.25f;
```

Then in `CalculatePhysicsBasedDrawShot()`:
```csharp
// Use draw-specific curl for longer trajectories
trajectorySimulator = new TrajectorySimulator(iceFriction, drawCurlStrength);
```

## Testing Checklist

### Takeout Shots (Priority 1)
- [ ] Out-turn takeout to center button (0, 6.5)
- [ ] In-turn takeout to center button (0, 6.5)
- [ ] Out-turn takeout to left rock (-0.5, 6.5)
- [ ] In-turn takeout to right rock (0.5, 6.5)
- [ ] Measure lateral error for each (goal: <10cm)

### Draw Shots (Priority 2)
- [ ] Draw to button (0, 6.5) - out-turn
- [ ] Draw to button (0, 6.5) - in-turn
- [ ] Draw to four-foot left (-0.4, 6.5) - out-turn
- [ ] Draw to four-foot right (0.4, 6.5) - in-turn
- [ ] Measure final position error (goal: <15cm)

### Guard Shots (Priority 3)
- [ ] Center guard (0, 4.0) - out-turn
- [ ] Center guard (0, 4.0) - in-turn
- [ ] Corner guards (±0.6, 4.0) - both turns

## Quick Debug Logging

Add this to `AI_Target.cs` for instant feedback:

```csharp
[Header("Debug")]
public bool logAIShots = true;

// At end of CalculatePhysicsBasedShot():
if (logAIShots && foundShot)
{
    Debug.Log($"?? [AI Shot Debug] " +
              $"Target: ({targetPosition.x:F3}, {targetPosition.y:F3}) | " +
              $"Turn: {(useInTurn ? "IN" : "OUT")} | " +
              $"Pullback: ({pullbackPosition.x:F3}, {pullbackPosition.y:F3}) | " +
              $"Distance: {Vector2.Distance(pullbackPosition, launcherPos):F3}");
}
```

Then after shot completes:
```csharp
// In Rock_Force.cs when rock stops:
Vector2 finalPos = body.position;
Vector2 targetPos = /* get from AI_Target */;
float lateralError = finalPos.x - targetPos.x;
float distanceError = finalPos.y - targetPos.y;

Debug.Log($"?? [AI Shot Result] " +
          $"Target: ({targetPos.x:F3}, {targetPos.y:F3}) | " +
          $"Actual: ({finalPos.x:F3}, {finalPos.y:F3}) | " +
          $"Error: Lateral={lateralError:F3} ({(lateralError > 0 ? "RIGHT" : "LEFT")}), " +
          $"Distance={distanceError:F3} ({(distanceError > 0 ? "LONG" : "SHORT")})");
```

## Iteration Process

1. **Set `curlStrength = 0.25f`** (match player)
2. **Run 5 test shots** (mix of in/out turn)
3. **Measure average error** from console logs
4. **Adjust curl** based on error direction:
   - Error mostly LEFT ? decrease by 0.05
   - Error mostly RIGHT ? increase by 0.05
   - Error mixed ? try different shot types
5. **Repeat** until average error < 10cm

## Expected Timeline

- **Takeouts**: 30-60 minutes to tune (should be quick!)
- **Draw shots**: 1-2 hours to tune (more iterations needed)
- **All shot types**: 3-4 hours total (comprehensive testing)

## Files to Modify

1. ? `Assets\Scripts\AI\AI_Target.cs` - Update `curlStrength = 0.25f`
2. ? Test and iterate based on results
3. ?? Document final tuned values

## Why This Will Work

- ? Physics system is **100% correct** now (matches player)
- ? AI uses **same physics** as player (TrajectorySimulator)
- ? Only **parameter tuning** needed (no code changes)
- ? Player trajectory is **reference standard** (proven accurate)

## Fallback Options

If `curlStrength = 0.25f` doesn't work well:

### Option A: Scale from Player Value
```csharp
// If AI needs different curl than player for some reason
public float curlStrength = 0.25f * 1.2f; // 20% more curl
```

### Option B: Separate Turn Curl
```csharp
public float inTurnCurlStrength = 0.25f;
public float outTurnCurlStrength = 0.28f; // Slightly different
```

### Option C: Shot-Type Specific
```csharp
public float takeoutCurlStrength = 0.25f;
public float drawCurlStrength = 0.22f;
public float guardCurlStrength = 0.27f;
```

## Success Criteria

**AI is "Fixed" when:**
- ? 80% of takeouts hit within 10cm lateral error
- ? 70% of draws land within 20cm total error
- ? No systematic bias (equal LEFT/RIGHT misses)
- ? Turn selection is appropriate for shot

## Next Steps After AI is Fixed

1. **Fine-tune difficulty levels** - vary curl by character accuracy
2. **Add shot variety** - slight randomization for realism
3. **Optimize strategy** - AI chooses better shots now that targeting works

---

**Ready to Start?**

1. Open `AI_Target.cs`
2. Change line 23 to: `public float curlStrength = 0.25f;`
3. Build and test with QuickTestGame
4. Report back what you see in console! ??

The physics is perfect now, so this should be straightforward tuning! ??
