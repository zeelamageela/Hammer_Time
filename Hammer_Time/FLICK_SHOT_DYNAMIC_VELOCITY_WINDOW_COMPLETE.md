# Flick Shot Dynamic Velocity Window - Complete Implementation

## Overview

Implemented a **dynamic velocity window** system that centers the velocity range around the target velocity for each shot. This makes the flick shot feel much more natural and forgiving - all shots have the same difficulty regardless of target distance.

## The Problem (Before)

**Fixed Velocity Range** made precision inconsistent:
```
Target: 8.5 m/s
Min: 5.0 m/s ??????????????????? Max: 16.0 m/s
                 ? Target: 8.5 m/s
Player must hit EXACTLY 8.5 out of 11 m/s range = very precise!
```

- Draw shots (8.5 m/s) were extremely hard - had to hit narrow middle of range
- Heavy shots (14.0 m/s) were easier - broader part of range near max
- **Inconsistent difficulty** based on target velocity

## The Solution (After)

**Dynamic Velocity Window** centers range on target:
```
Target: 8.5 m/s
Min: 7.0 m/s ?????? Max: 10.0 m/s
           ? Target: 8.5 m/s (center!)
Player must hit within ±1.5 m/s = much more forgiving!
```

- **All shots feel equally challenging** regardless of target velocity
- Window moves with target: draw shots get low range, heavy shots get high range
- **Configurable tolerance** allows tuning difficulty (default: ±1.5 m/s)

## Implementation Details

### 1. New Parameters (Inspector Configurable)

```csharp
[Header("Dynamic Velocity Window (NEW!)")]
[Tooltip("Velocity tolerance around target (±X m/s) - how much faster/slower than target is allowed")]
[Range(0.5f, 3.0f)]
public float velocityTolerance = 1.5f; // ±1.5 m/s window around target

[Tooltip("Absolute minimum rock velocity (safety clamp)")]
[Range(3.0f, 7.0f)]
public float absoluteMinVelocity = 5.0f; // Never go below this

[Tooltip("Absolute maximum rock velocity (safety clamp)")]
[Range(12.0f, 18.0f)]
public float absoluteMaxVelocity = 16.0f; // Never go above this
```

### 2. Dynamic Window Calculation

At the start of each power phase:
```csharp
// Get target velocity from trajectory
targetRockVelocity = GetTargetVelocityFromTrajectory(); // e.g., 8.5 m/s

// Calculate dynamic min/max around target
dynamicMinVelocity = targetRockVelocity - velocityTolerance; // 7.0 m/s
dynamicMaxVelocity = targetRockVelocity + velocityTolerance; // 10.0 m/s

// Safety clamp to absolute limits
dynamicMinVelocity = Mathf.Max(dynamicMinVelocity, absoluteMinVelocity);
dynamicMaxVelocity = Mathf.Min(dynamicMaxVelocity, absoluteMaxVelocity);
```

### 3. Velocity Mapping

Player's drag velocity is mapped to the dynamic range:
```csharp
// Old (Fixed Range):
normalizedSpeed = InverseLerp(5.0, 16.0, dragVelocity); // 0-1 across fixed range

// New (Dynamic Range):
float minDragVel = CalculateIdealDragVelocityForRockSpeed(dynamicMinVelocity);
float maxDragVel = CalculateIdealDragVelocityForRockSpeed(dynamicMaxVelocity);
normalizedSpeed = InverseLerp(minDragVel, maxDragVel, dragVelocity); // 0-1 across dynamic range
```

### 4. Enhanced Feedback

Shows target velocity and deviation in callouts:
```
Perfect!
8.52 m/s               ? Actual velocity
Target: 8.50 m/s (+0.02) ? Target and error
Swipe: 12.3 units/s
Stop: Y=2.4
1.8m in 0.82s
```

## Benefits

### ? Consistent Difficulty
All shots feel equally challenging:
- **Draw shot** (8.5 m/s): window = 7.0 - 10.0 m/s (3.0 m/s range)
- **Medium shot** (10.5 m/s): window = 9.0 - 12.0 m/s (3.0 m/s range)
- **Heavy shot** (14.0 m/s): window = 12.5 - 15.5 m/s (3.0 m/s range)

Same window size = same difficulty!

### ? Natural Feel
Player just needs to match velocity guide speed:
- Velocity guide shows **exact** target velocity
- Player swipes at that speed
- System automatically adjusts acceptable range

### ? Configurable Forgiveness
Tune difficulty via `velocityTolerance`:
- **±1.0 m/s**: 2.0 m/s window (harder, more skilled)
- **±1.5 m/s**: 3.0 m/s window (default, balanced)
- **±2.0 m/s**: 4.0 m/s window (easier, more forgiving)

### ? Safety Clamping
Never exceeds physics limits:
- Prevents going below `absoluteMinVelocity` (5.0 m/s)
- Prevents going above `absoluteMaxVelocity` (16.0 m/s)
- Maintains rock physics consistency

## Testing Guide

### Test Scenario 1: Draw Shot (Slow)
1. Aim at house center (Y = 0)
2. Expected target: ~8.5 m/s
3. Watch logs for dynamic window
4. Expected: min ? 7.0 m/s, max ? 10.0 m/s

### Test Scenario 2: Heavy Shot (Fast)
1. Aim at back of house (Y = 10)
2. Expected target: ~13.5 m/s
3. Watch logs for dynamic window
4. Expected: min ? 12.0 m/s, max ? 15.0 m/s

### Test Scenario 3: Tolerance Testing
1. Set `velocityTolerance = 1.0` in Inspector (tight)
2. Try a draw shot - should feel harder
3. Set `velocityTolerance = 2.5` in Inspector (loose)
4. Try same shot - should feel easier

### Test Scenario 4: Safety Clamping
1. Aim at extreme back (Y = 15)
2. Expected target: ~15.5 m/s
3. Dynamic max would be 17.0 m/s, but...
4. Should clamp to `absoluteMaxVelocity` (16.0 m/s)

## Configuration Recommendations

### Easy Mode (Beginner Friendly)
```csharp
velocityTolerance = 2.0f; // ±2.0 m/s
forgivenessFactor = 2.0f; // More smoothing
velocityScaleMultiplier = 0.8f; // Slower swipes
```

### Normal Mode (Balanced)
```csharp
velocityTolerance = 1.5f; // ±1.5 m/s (default)
forgivenessFactor = 1.7f; // Moderate smoothing
velocityScaleMultiplier = 1.0f; // Natural feel
```

### Hard Mode (Skilled Players)
```csharp
velocityTolerance = 1.0f; // ±1.0 m/s
forgivenessFactor = 1.2f; // Less smoothing
velocityScaleMultiplier = 1.2f; // Faster swipes
```

## Debug Logging

When power phase starts, you'll see:
```
[FlickShot] ?? DYNAMIC VELOCITY WINDOW:
  Target velocity: 8.50 m/s
  Tolerance: ±1.50 m/s
  Min velocity: 7.00 m/s (target - tolerance)
  Max velocity: 10.00 m/s (target + tolerance)
  Window size: 3.00 m/s
  Absolute limits: 5.00 - 16.00 m/s
```

When rock is released:
```
[FlickShot] *** STACKED SPEED CALLOUTS: Perfect! | 8.52 m/s (target: 8.50, error: +0.02) | ...
```

## Technical Details

### Velocity Mapping Chain

```
Player Input (Swipe Speed)
  ?
Drag Velocity (units/second) = distance / time
  ?
CalculateSpeedFromVelocity(dragVelocity)
  ? Maps to dynamic drag velocity range
Normalized Speed (0-1) = InverseLerp(minDragVel, maxDragVel, dragVelocity)
  ? Apply forgiveness factor
Smoothed Speed (0-1)
  ? Maps to dynamic rock velocity range
GetPredictedVelocity()
  ?
Rock Velocity (m/s) = Lerp(dynamicMinVelocity, dynamicMaxVelocity, smoothedSpeed)
  ? Safety clamp
Final Velocity (m/s) = Clamp(rockVelocity, absoluteMin, absoluteMax)
```

### Key Difference

**Before:**
```csharp
// Fixed range for ALL shots
rockVelocity = Lerp(5.0, 16.0, normalizedSpeed);
```

**After:**
```csharp
// Dynamic range PER shot (centered on target!)
rockVelocity = Lerp(dynamicMinVelocity, dynamicMaxVelocity, normalizedSpeed);
```

## Backward Compatibility

Old parameters are kept for compatibility:
- `minDragVelocity` (DEPRECATED - marked in tooltip)
- `maxDragVelocity` (DEPRECATED - marked in tooltip)

They're no longer used in calculations, but won't break existing scenes.

## Files Modified

### `Assets\Scripts\Rock\FlickShotController.cs`
- Added `velocityTolerance`, `absoluteMinVelocity`, `absoluteMaxVelocity` parameters
- Added `dynamicMinVelocity`, `dynamicMaxVelocity`, `targetRockVelocity` fields
- Updated `StartPowerPhase()` to calculate dynamic window
- Updated `GetPredictedVelocity()` to use dynamic range
- Updated `CalculateSpeedFromVelocity()` to map to dynamic drag range
- Updated `GetSpeedFeedbackMessage()` to show velocity deviation from target
- Enhanced release callouts to show target velocity and error

## Future Enhancements

### Potential Additions
1. **Difficulty Scaling**: Auto-adjust tolerance based on player skill level
2. **Visual Window Indicator**: Show green/yellow/red zones in UI
3. **Training Mode**: Extra-wide tolerance (±3.0 m/s) for learning
4. **Velocity History**: Track player's typical error to auto-tune tolerance
5. **Shot-Specific Tolerance**: Different tolerance for different shot types

### Performance Notes
- Dynamic window calculation happens once per shot (minimal overhead)
- No per-frame recalculation (efficient!)
- Debug logging can be disabled for release builds

## Status

? **Implementation Complete**
? **Build Successful**
? **Ready for Testing**

## Next Steps

1. **Test in-game** with different target velocities
2. **Tune `velocityTolerance`** based on feel (start at 1.5 m/s)
3. **Verify callouts** show target velocity and error correctly
4. **Check edge cases** (very slow/fast targets, safety clamping)
5. **Adjust `forgivenessFactor`** if needed (currently 1.7)

## Summary

The dynamic velocity window makes flick shot feel **natural** and **consistent**. All shots have the same difficulty, and players just need to match the velocity guide's speed. The system automatically adjusts the acceptable range to center on the target, making the game more intuitive and fun to play! ??
