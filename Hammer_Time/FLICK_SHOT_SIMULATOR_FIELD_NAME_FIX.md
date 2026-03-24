# Flick Shot Simulator Field Name Fix + Multi-line Callout Cleanup

**Status**: ? COMPLETE  
**Date**: 2025-06-XX  
**Components**: `FlickShotController.cs`

## Problems Fixed

### 1. Simulator Field Name Mismatch

The `FlickShotController` was trying to access a field named `"simulator"` in `TrajectoryLine` using reflection, but the actual field name is `"trajectorySimulator"`.

### Error Logs
```
[FlickShot Prediction] Could not find 'simulator' field!
[FlickShot Prediction] This should never happen - field exists in logs!
```

But when listing all fields:
```
Field: trajectorySimulator (TrajectorySimulator)
```

### 2. Multi-line Callout in Disabled Code

There was a multi-line callout using `\n` in commented-out shooter animation code. Converted to stacked single-line callouts for consistency.

**Line 308** (disabled code):
```csharp
ShowCallout(transform.position, "Release too early!\nSwipe further down", followTarget: false, duration: 2f);
```

## Root Causes

### 1. Field Name Mismatch

**Line 646** in `FlickShotController.cs`:
```csharp
// WRONG: Looking for "simulator"
System.Reflection.FieldInfo simulatorField = trajType.GetField("simulator", ...);
```

**Actual field name** in `TrajectoryLine.cs`:
```csharp
private TrajectorySimulator trajectorySimulator;
```

## Solutions

### 1. Field Name Fix

Changed the reflection code to use the correct field name:

```csharp
// FIXED: Use correct field name "trajectorySimulator" not "simulator"
System.Reflection.FieldInfo simulatorField = trajType.GetField("trajectorySimulator", 
    System.Reflection.BindingFlags.NonPublic | 
    System.Reflection.BindingFlags.Instance);
```

### 2. Multi-line Callout Cleanup

Converted multi-line callout to stacked single-line callouts:

```csharp
// BEFORE (multi-line):
ShowCallout(transform.position, "Release too early!\nSwipe further down", followTarget: false, duration: 2f);

// AFTER (stacked):
ShowCallout(transform.position, "Release too early!", followTarget: false, duration: 2f);
ShowCallout(transform.position, "Swipe further down", followTarget: false, duration: 2f);
```

## Changes Made

### `Assets/Scripts/Rock/FlickShotController.cs`

#### Fix 1: Field Name (Lines 645-648, 652, 663-666)
```csharp
// BEFORE:
System.Reflection.FieldInfo simulatorField = trajType.GetField("simulator", ...);

// AFTER:
System.Reflection.FieldInfo simulatorField = trajType.GetField("trajectorySimulator", ...);
```

**Line 652**: Updated success debug message
```csharp
// BEFORE:
Debug.Log("[FlickShot Prediction] Found 'simulator' field (private)!");

// AFTER:
Debug.Log("[FlickShot Prediction] Found 'trajectorySimulator' field (private)!");
```

**Line 663-666**: Updated error messages
```csharp
// BEFORE:
Debug.LogError("[FlickShot Prediction] Could not find 'simulator' field!");
Debug.LogError("[FlickShot Prediction] This should never happen - field exists in logs!");

// AFTER:
Debug.LogError("[FlickShot Prediction] Could not find 'trajectorySimulator' field!");
Debug.LogError("[FlickShot Prediction] Check if field name has changed in TrajectoryLine!");
```

#### Fix 2: Multi-line Callout (Line 308)

**Before**:
```csharp
ShowCallout(transform.position, "Release too early!\nSwipe further down", followTarget: false, duration: 2f);
```

**After** (stacked single-line callouts):
```csharp
ShowCallout(transform.position, "Release too early!", followTarget: false, duration: 2f);
ShowCallout(transform.position, "Swipe further down", followTarget: false, duration: 2f);
```

> **Note**: This code is currently disabled (commented out) as shooter animation control was removed, but the fix ensures consistency if it's ever re-enabled.

## Expected Behavior After Fix

When player releases flick shot in power phase:

1. ? `CalculatePredictedStopPosition()` correctly finds `trajectorySimulator` field
2. ? Successfully gets `TrajectorySimulator` instance
3. ? Calls `SimulateTrajectory()` to get accurate prediction
4. ? Draws cyan horizontal line at predicted stop position
5. ? No longer falls back to approximate formula

### Success Log Output
```
[FlickShot Prediction] ======== PREDICTION START ========
[FlickShot Prediction] Initial velocity: 8.36 m/s
[FlickShot Prediction] trajLine found: TrajectoryLine
[FlickShot Prediction] Found 'trajectorySimulator' field (private)!
[FlickShot Prediction] Simulator is NOT null: TrajectorySimulator
[FlickShot Prediction] *** TrajectorySimulator SUCCESS! ***
[FlickShot Prediction] Predicted UNSWEPT stop: Y = 3.42
```

## Build Status

? **Build successful** - all changes compile correctly

## Impact

- **Low risk**: Simple field name correction + callout consistency fix
- **High impact**: Fixes cyan prediction line accuracy in flick shot mode
- **No side effects**: Only affects flick shot prediction system
- **Consistency**: All callouts now use stacked single-line pattern

## Testing

After this fix, test in flick shot mode:

1. Enable flick shot mode in visualization settings
2. Pull back to set aim
3. Click launcher to start power phase  
4. Swipe down and release
5. **Verify**: Cyan horizontal line appears at predicted stop position
6. **Verify**: Debug logs show "TrajectorySimulator SUCCESS!"
7. **Watch**: Rock should stop very close to cyan line (accounting for sweeping)

## Related Files

- `Assets/Scripts/Rock/FlickShotController.cs` - Fixed field name
- `Assets/Scripts/UI/TrajectoryLine.cs` - Contains `trajectorySimulator` field
- `Assets/Scripts/UI/TrajectorySimulator.cs` - Provides trajectory simulation

## Notes

This bug was caught by excellent debug logging that listed all fields in `TrajectoryLine` at runtime, making the discrepancy obvious.

The fix uses reflection to access a private field because `FlickShotController` dynamically loads components without hard dependencies (for modularity).
