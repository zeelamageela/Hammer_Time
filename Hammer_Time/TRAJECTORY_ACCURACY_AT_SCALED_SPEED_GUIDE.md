# Trajectory Accuracy at 0.5x Speed - Quick Fix Guide

**Issue**: Trajectory was perfect at 1.0x speed, but may be inaccurate at 0.5x speed.

**Root Cause**: The trajectory simulator uses a **tuned damping value** (0.62) that was calibrated against rocks with damping 0.38. When we scale both, the ratio changes!

---

## The Math

### Before (1.0x speed):
```
Rock damping: 0.38
Simulator damping: 0.62
Ratio: 0.62 / 0.38 = 1.63x

This ratio was TUNED to match reality (accounts for angular damping, collider drag, etc.)
```

### After (0.5x speed with current code):
```
Rock damping: 0.19 (0.38 × 0.5)
Simulator damping: 0.31 (0.62 × 0.5)
Ratio: 0.31 / 0.19 = 1.63x

SAME RATIO! Should still work! ?
```

---

## Quick Test

### Check if trajectory is accurate:

1. **Aim at button** (Y=6.5)
2. **Pull back to specific distance** (e.g., 2.0 units)
3. **Check trajectory preview** - Does it show button?
4. **Shoot rock** - Does it reach button?

### Expected Results:

| Scenario | Trajectory Shows | Rock Reaches | Status |
|----------|-----------------|--------------|--------|
| **If scaling is perfect** | Y=6.5 | Y=6.5 ± 0.1 | ? No fix needed |
| **If simulator undershoots** | Y=6.5 | Y=7.2 | ?? Need to increase simulator damping |
| **If simulator overshoots** | Y=6.5 | Y=5.8 | ?? Need to decrease simulator damping |

---

## Solution 1: Adjust TrajectoryLine Inspector Settings

The `TrajectoryLine` component has an `iceFriction` parameter that gets passed to the simulator.

### To Fix Overshoot (trajectory predicts short):
**Decrease `iceFriction` in TrajectoryLine**:
```
Current: 0.62
Try: 0.55 (or lower)
```

### To Fix Undershoot (trajectory predicts long):
**Increase `iceFriction` in TrajectoryLine**:
```
Current: 0.62
Try: 0.70 (or higher)
```

---

## Solution 2: Add Automatic Calibration (Better!)

If you want the simulator to auto-calibrate based on `globalSpeedMultiplier`, I can add this logic. But first, **test if it needs adjustment at all!**

---

## Diagnostic Steps

### 1. Add Debug Logging

Check the actual values being used:

**Expected Debug Logs**:
```
[TrajectorySimulator] Global speed multiplier detected: 0.50x
[TrajectorySimulator] Damping scaled: 0.620 ? 0.310 (matches Rock_Force)
```

**Check Rock_Force logs**:
```
[Rock_Force Release] Initial velocity: 8.24 m/s
[Rock_Force] Damping scaled: linear=0.190, angular=0.160
[Rock_Force] Global speed: 0.50x - Velocity: 4.12 m/s, Damping: 0.190
```

### 2. Compare Ratios

**In TrajectoryLine.cs, check the constructor**:
```csharp
// When TrajectoryLine creates the simulator, what friction value does it pass?
trajectorySimulator = new TrajectorySimulator(iceFriction, curlStrength);
```

**The issue might be**: `iceFriction` in TrajectoryLine might need to be a **different base value** for 0.5x speed.

---

## Quick Fix Option A: Scale the Base Friction

If the trajectory is consistently off, you need to adjust the **base friction** value that TrajectoryLine passes to the simulator.

### Add to TrajectoryLine.cs:

```csharp
void Start()
{
    // Get global speed multiplier from any rock
    Rock_Force rockForce = FindFirstObjectByType<Rock_Force>();
    float speedMultiplier = (rockForce != null) ? rockForce.globalSpeedMultiplier : 1.0f;
    
    // Adjust base friction for scaled speeds
    float adjustedFriction = iceFriction * speedMultiplier;
    
    trajectorySimulator = new TrajectorySimulator(adjustedFriction, curlStrength);
    
    Debug.Log($"[TrajectoryLine] Base friction: {iceFriction}, Adjusted: {adjustedFriction}, Speed: {speedMultiplier}x");
}
```

**BUT WAIT!** The simulator already does this scaling internally, so this would **double-scale** it! ?

---

## Quick Fix Option B: Don't Scale Simulator Damping (Simpler!)

The issue is that we're scaling the simulator damping, but the **tuned ratio** (1.63x) was already accounting for the physics differences.

### Change in TrajectorySimulator.cs:

**REMOVE the scaling**:
```csharp
public TrajectorySimulator(float friction, float curl)
{
    baseDamping = friction;
    linearDamping = friction;  // DON'T scale! The ratio is already tuned!
    curlAmount = curl;
    
    Debug.Log($"[TrajectorySimulator] Using friction: {friction:F3} (not scaled by globalSpeedMultiplier)");
}
```

**Why this works**:
- TrajectoryLine passes `iceFriction = 0.62`
- Simulator uses `linearDamping = 0.62` (unchanged)
- But rocks use `linearDamping = 0.19` (scaled)
- Ratio: 0.62 / 0.19 = **3.26x** (different!)

**Hmm, that doesn't work either...** ??

---

## The Real Solution: Test First!

**Before changing anything**, let's test if the trajectory is actually broken:

### Test Script:

1. Open Unity
2. Start a game
3. Set `globalSpeedMultiplier = 0.5` in Rock prefab
4. Aim at button (Y=6.5)
5. Check trajectory preview endpoint
6. Shoot rock
7. Compare actual endpoint

### If they match:
? **No fix needed!** The current scaling is correct!

### If they don't match:
The ratio between simulator damping and rock damping needs adjustment.

---

## Advanced Fix: Dynamic Ratio Adjustment

If the trajectory is off, we need to adjust the **effective damping ratio**.

### Add to TrajectorySimulator.cs:

```csharp
public TrajectorySimulator(float friction, float curl)
{
    baseDamping = friction;
    curlAmount = curl;
    
    // Get rock's actual damping at current speed
    Rock_Force rockForce = GameObject.FindFirstObjectByType<Rock_Force>();
    if (rockForce != null && rockForce.globalSpeedMultiplier != 1.0f)
    {
        // Rock uses: 0.38 × speedMultiplier
        float rockActualDamping = 0.38f * rockForce.globalSpeedMultiplier;
        
        // We want to maintain the TUNED RATIO (0.62 / 0.38 = 1.63x)
        // So: simulatorDamping = rockActualDamping × 1.63
        const float TUNED_RATIO = 1.63f;
        linearDamping = rockActualDamping * TUNED_RATIO;
        
        Debug.Log($"[TrajectorySimulator] Speed: {rockForce.globalSpeedMultiplier:F2}x");
        Debug.Log($"[TrajectorySimulator] Rock damping: {rockActualDamping:F3}");
        Debug.Log($"[TrajectorySimulator] Simulator damping: {linearDamping:F3} (ratio {TUNED_RATIO:F2}x)");
    }
    else
    {
        linearDamping = friction;
        Debug.Log($"[TrajectorySimulator] Using base friction: {friction:F3}");
    }
}
```

---

## My Recommendation

### Step 1: Test Current Implementation
**Don't change anything yet!** Just test if the trajectory matches reality at 0.5x speed.

### Step 2: If Broken, Use the Advanced Fix
If trajectory doesn't match, use the "Dynamic Ratio Adjustment" code above. It maintains the **tuned 1.63x ratio** at any speed.

### Step 3: Fine-Tune if Needed
If the ratio needs adjustment, change `TUNED_RATIO`:
- **Trajectory overshoots**: Decrease ratio (try 1.5x)
- **Trajectory undershoots**: Increase ratio (try 1.8x)

---

## Quick Inspector Tuning (Easiest!)

If you don't want to change code, just adjust in Unity Inspector:

**TrajectoryLine component**:
1. Find `iceFriction` parameter
2. Current value: probably 0.62
3. Adjust up or down by 0.05 increments
4. Test trajectory accuracy after each change

**Tuning guide**:
- `iceFriction = 0.55` ? Trajectory predicts shorter paths
- `iceFriction = 0.62` ? Default (was tuned for 1.0x speed)
- `iceFriction = 0.70` ? Trajectory predicts longer paths

---

## Summary

**Before making any code changes**:
1. ? Test trajectory accuracy at 0.5x speed
2. ? Check if preview matches actual rock path
3. ? Only adjust if there's a measurable error

**If adjustment needed**:
- **Quick fix**: Adjust `iceFriction` in TrajectoryLine Inspector
- **Better fix**: Use "Dynamic Ratio Adjustment" code to maintain tuned ratio

**Most likely outcome**: 
The current implementation is probably **fine** because we scaled both velocity and damping proportionally! The tuned ratio (1.63x) should still be accurate! ?

---

**Test it first and let me know what you find!** ??
