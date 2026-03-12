# AI Sweeping System - Quick Reference

## System Architecture

```
AI_Shooter.cs
    ? (executes shot)
    ? (releases rock)
    ?
AI_Sweeper.StartPhysicsBasedSweeping()
    ?
MonitorAndSweepCoroutine()
    ? (monitors trajectory)
    ? (detects deviations)
    ?
ApplySweepState()
    ?
SweeperManager.SweepWeight/Left/Right/Whoa()
```

---

## Key Philosophy

**BEFORE COLLISION:**
1. Avoid hitting rocks
2. Reach target distance
3. Stay on ideal line

**AFTER COLLISION:**
1. Get to scoring position (house)
2. Find cover behind rocks
3. Stop at optimal location

---

## Decision Tree

```
Has rock collided yet?
?? NO (Pre-collision)
?  ?? Collision imminent? (< 2m)
?     ?? YES ? Avoid collision (Line/Curl/Critical sweep)
?     ?? NO ? Normal trajectory following
?        ?? Shortfall > 1.0m? ? Critical sweep
?        ?? Shortfall > 0.25m? ? Weight sweep
?        ?? |Lateral error| > 0.12m? ? Line/Curl sweep
?        ?? On track ? Whoa
?
?? YES (Post-collision)
   ?? Heading to house?
      ?? YES ? Sweep to reach house
      ?? In house? ? Fine positioning
      ?? Beyond house ? Whoa
```

---

## Sweep States

| State | Command | Effect |
|-------|---------|--------|
| **None** | `SweepWhoa(true)` | Stop sweeping |
| **Weight** | `SweepWeight(true)` | Both sweepers, +6 feet |
| **Critical** | `SweepWeight(true)` | Both sweepers, desperate |
| **Line** | `SweepLeft(true)` or `SweepRight(true)` | Straighten rock |
| **Curl** | `SweepRight(true)` or `SweepLeft(true)` | Add more curl |

### Turn Direction Logic

**IN-TURN (curls LEFT)**
- Rock too far RIGHT ? `Line` ? Sweep LEFT
- Rock too far LEFT ? `Curl` ? Sweep RIGHT

**OUT-TURN (curls RIGHT)**
- Rock too far LEFT ? `Line` ? Sweep RIGHT  
- Rock too far RIGHT ? `Curl` ? Sweep LEFT

---

## Tuning Parameters

### Thresholds (in meters)
```csharp
float lateralErrorThreshold = 0.12f;    // 12cm off-line
float distanceErrorThreshold = 0.25f;   // 25cm short
float predictionLookahead = 3.5f;       // Look 3.5m ahead
float collisionLookahead = 2.0f;        // Collision warning at 2m
```

### Skill Multiplier
```csharp
// Better sweepers ? more aggressive (1.0x thresholds)
// Poor sweepers ? more conservative (0.7x thresholds)
float skillMultiplier = 1.0f - (0.3f * (1.0f - sweepSkill));
```

---

## Common Use Cases

### Use Case 1: Draw to Button
```
Initial: Clean trajectory to button
Monitor: Check lateral error at each frame
Action: Line sweep if |error| > 0.12m, Weight if short
Result: Rock reaches button within 0.12m lateral accuracy
```

### Use Case 2: Guard Shot
```
Initial: Clean trajectory to finesse zone
Monitor: Check if rock will reach Y=3.5
Action: Weight sweep if shortfall detected
Result: Rock stops in finesse zone (Y=2.5-4.5)
```

### Use Case 3: Raise Takeout
```
Initial: Clean trajectory through target rock
Monitor: Collision detection at 2m lookahead
Action: Critical sweep if collision imminent
Post-Collision: Weight sweep to reach house
Result: Target rock removed, shooter in house
```

### Use Case 4: Avoid Guard
```
Initial: Clean trajectory past guard
Monitor: Detect guard in path 2m ahead
Action: Line/Curl sweep to adjust path around guard
Result: Rock avoids collision, continues to target
```

---

## Debug Commands

### View Trajectory
```csharp
Debug.Log($"Clean trajectory has {cleanTrajectory.Count} points");
```

### View Deviations
```csharp
Debug.Log($"Y={currentPos.y:F2}: State={desiredState}, LateralErr={lateralError:F3}, Shortfall={predictedShortfall:F2}");
```

### View Collision
```csharp
Debug.Log($"COLLISION IMMINENT! Distance: {collisionDistance:F2}m at {collisionPoint}");
```

### View Post-Collision
```csharp
Debug.Log($"POST-COLLISION MODE ACTIVATED");
```

---

## Extending the System

### Add New Post-Collision Behavior
```csharp
if (hasCollided)
{
    // YOUR NEW BEHAVIOR HERE
    if (ShouldHideBehindRock())
    {
        desiredState = "Weight";
        Debug.Log("[AI_Sweeper] Seeking cover behind rock");
    }
}
```

### Add Game State Awareness
```csharp
// Get current score
int scoreDiff = gm.redScore - gm.yellowScore;

// Modify thresholds based on score
if (scoreDiff < -3)
{
    // Aggressive when behind
    lateralThreshold *= 1.5f;
    distanceThreshold *= 1.5f;
}
```

### Add Ice Condition Awareness
```csharp
// Get ice speed multiplier
float iceSpeed = FindObjectOfType<GameSettingsPersist>().globalRockSpeedMultiplier;

// Adjust thresholds for faster ice
if (iceSpeed > 1.2f)
{
    lateralThreshold *= 0.8f;  // Tighter control on fast ice
}
```

---

## Troubleshooting

### Problem: Rock always falls short
**Check**: `predictedShortfall` calculation
**Fix**: Lower `distanceErrorThreshold` to trigger Weight sweep earlier

### Problem: Rock over-curls
**Check**: `lateralError` sign logic for in-turn vs out-turn
**Fix**: Verify Line/Curl sweep mapping

### Problem: Collision not detected
**Check**: `collisionLookaheadDistance` value
**Fix**: Increase to 3.0m for earlier detection

### Problem: Too much sweeping
**Check**: `skillMultiplier` calculation
**Fix**: Increase thresholds or reduce skill influence

---

## Performance Notes

- **Trajectory simulation**: Called once per FixedUpdate (~50 Hz)
- **Collision detection**: Uses short 100-point lookahead
- **Memory**: Stores clean trajectory (250 Vector2s)
- **CPU**: Minimal - linear interpolation only

---

## Code Locations

| Component | File | Method |
|-----------|------|--------|
| Entry Point | `AI_Sweeper.cs` | `StartPhysicsBasedSweeping()` |
| Main Loop | `AI_Sweeper.cs` | `MonitorAndSweepCoroutine()` |
| State Application | `AI_Sweeper.cs` | `ApplySweepState()` |
| Trajectory Lookup | `AI_Sweeper.cs` | `GetPredictedPositionAtY()` |
| Skill Calculation | `AI_Sweeper.cs` | `GetSweeperSkill()` |
| Legacy System | `AI_Sweeper.cs` | `TargetShot()` (DISABLED) |

---

## Quick Test

1. Start AI vs AI game
2. Watch for log: `[AI_Sweeper] Monitoring started`
3. Observe sweep state changes as rock travels
4. Check final position vs target
5. Verify collision avoidance when guards present

---

## Success Metrics

? Rocks reach target within 0.25m distance  
? Rocks stay within 0.12m lateral accuracy  
? Collisions avoided when possible  
? Post-collision rocks reach scoring position  
? Skill differences visible in sweeping behavior  
? Clean logs showing decision rationale  
