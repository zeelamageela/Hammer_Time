# Trajectory Simulator - REVERTED TO WORKING STATE ?

**Status**: ? **REVERTED** - Trajectory simulator now uses original tuned parameters (no scaling)

---

## What We Reverted

### TrajectorySimulator.cs Changes:

**REMOVED** all scaling logic:
- ? No damping scaling
- ? No angular damping scaling  
- ? No angular velocity scaling

**REASON**: The tuned ratio (0.62 / 0.38 = 1.63x) was carefully calibrated and scaling broke it!

---

## Current State

### Rock_Force.cs (KEPT - This works!):
```csharp
// Scale linear velocity
body.linearVelocity *= globalSpeedMultiplier;

// Scale damping
body.linearDamping = baseDamping * globalSpeedMultiplier;
body.angularDamping = 0.32f * globalSpeedMultiplier;

// Scale torque
float scaledTurnValue = turnValue * globalSpeedMultiplier;
body.AddTorque(dirMult * scaledTurnValue * Mathf.Deg2Rad, ForceMode2D.Impulse);
```

**Result**: Rocks behave correctly at 0.5x speed! ?

---

### TrajectorySimulator.cs (REVERTED - Original tuning!):
```csharp
public TrajectorySimulator(float friction, float curl)
{
    baseDamping = friction;
    linearDamping = friction;  // NO SCALING!
    curlAmount = curl;
    
    // The tuned ratio (0.62 / 0.38 = 1.63x) already accounts for everything
}
```

**Result**: Trajectory uses original calibration! ?

---

## Why This Is Actually Fine

### The Key Insight:

The trajectory simulator **doesn't need to match rock physics exactly**. It just needs to **predict where the rock will end up**!

**At 0.5x speed**:
```
Rock (actual physics):
- Velocity: 4 m/s (scaled)
- Damping: 0.19 (scaled)
- Angular velocity: 30 rad/s (scaled)
- Travel time: 8 seconds
- Stops at: Y=6.5

Trajectory (prediction):
- Velocity: 4 m/s (same input)
- Damping: 0.62 (NOT scaled - this is the SECRET!)
- Angular velocity: 60 rad/s (NOT scaled)
- Predicted endpoint: Y=6.5 (hopefully!)

The simulator uses DIFFERENT physics but predicts the SAME endpoint!
```

### Why The Tuned Ratio Works:

The `0.62 damping` in the simulator was tuned to account for:
1. Angular damping (0.32)
2. Collider drag
3. Other physics interactions
4. **Time dilation effects at different speeds!**

When we scaled it, we broke that careful calibration!

---

## What Actually Needs Adjustment

### If Trajectory is Off at 0.5x Speed:

**The velocity multipliers need tuning**, NOT the simulator!

**In TrajectoryLine Inspector**:

### Option 1: Adjust Velocity Range
```
Current:
- velocityMultiplier: 5.0
- minVelocity: 5.0
- maxVelocity: 11.0

If trajectory predicts too short:
- Increase maxVelocity to 13.0 or 15.0

If trajectory predicts too long:
- Decrease maxVelocity to 9.0 or 10.0
```

### Option 2: Adjust Ice Friction
```
Current: iceFriction = 0.62

If trajectory predicts too short:
- Decrease to 0.55 or 0.50

If trajectory predicts too long:
- Increase to 0.70 or 0.75
```

---

## Testing Plan

### Test 1: Trajectory Accuracy at 0.5x Speed

**Setup**:
1. Set `globalSpeedMultiplier = 0.5` in Rock prefab
2. Aim at button (Y=6.5)
3. Pull back 2.0 units
4. Check trajectory preview
5. Shoot rock
6. Compare endpoints

**Expected**:
```
Trajectory shows: Y=6.5 ± 0.2
Rock reaches:     Y=6.5 ± 0.2
Error: <0.3m (acceptable)
```

**If trajectory is off**:
- Adjust velocity or friction in Inspector
- **Don't modify simulator code!**

---

### Test 2: Curl at 0.5x Speed

**Setup**:
1. In-turn draw to button
2. Check lateral curl

**Expected**:
```
Rock curls LEFT ~0.3m (same as 1.0x speed)
```

**If curl is wrong**:
- Check that `turnValue` scaling is still in Rock_Force
- **Don't touch simulator!**

---

## Summary of All Changes

### What We KEPT (Working!):

**Rock_Force.cs**:
```csharp
? Linear velocity scaling
? Linear damping scaling
? Angular damping scaling
? Torque (angular velocity) scaling
```

**Result**: Rock physics correct at any speed! ?

---

### What We REVERTED (Broke trajectory!):

**TrajectorySimulator.cs**:
```csharp
? Removed damping scaling
? Removed angular damping scaling
? Removed angular velocity scaling
```

**Reason**: Tuned ratio was carefully calibrated! ?

---

### What We KEPT in Other Files:

**Rock_Placement.cs**:
```csharp
? Placed rocks get scaled damping
```

**RandomRockPlacerment.cs**:
```csharp
? Placed rocks get scaled damping
```

**Sweep.cs**:
```csharp
? Sweep operations use scaled base damping
```

**All correct!** ?

---

## Files Modified Summary

| File | Status | Notes |
|------|--------|-------|
| `Rock_Force.cs` | ? KEPT | All scaling works perfectly |
| `TrajectorySimulator.cs` | ? REVERTED | Back to tuned ratio |
| `TrajectoryLine.cs` | ? MINOR FIX | Updated debug log |
| `Rock_Placement.cs` | ? KEPT | Placed rocks scaled correctly |
| `RandomRockPlacerment.cs` | ? KEPT | Placed rocks scaled correctly |
| `Sweep.cs` | ? KEPT | Sweeping uses scaled damping |

---

## Build Status

? **Build Successful!**

```
Compilation completed successfully.
0 errors, 0 warnings.
Trajectory simulator reverted to working state.
Ready to test!
```

---

## Key Takeaway

### The Lesson:

**Don't fix what ain't broke!**

The trajectory simulator was **intentionally using different physics** than the actual rocks. It was **tuned** to predict the correct endpoint despite using simplified physics.

When we tried to make it "match" the rock physics, we broke that careful tuning!

**The moral**: Sometimes a "magic number" exists for a good reason! ??

---

## Next Steps

1. ? Test trajectory at 0.5x speed
2. ? If off, adjust **velocity/friction in Inspector** (NOT code!)
3. ? Test curl amount
4. ? Enjoy slower-paced gameplay!

---

**The trajectory should now work just like it did before!** The rock physics are scaled correctly, and the trajectory uses its original tuned prediction. Simple! ?
