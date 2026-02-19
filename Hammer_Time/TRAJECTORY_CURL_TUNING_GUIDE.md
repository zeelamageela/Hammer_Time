# Trajectory Curl Physics Tuning Guide

## Problem Solved

The trajectory simulator was using a **simplified parabolic curl model** that didn't match the real rock physics. Real curling rocks have **angular velocity-dependent curl** that creates the natural "late break" effect.

## Root Cause

### Real Physics (`Rock_Force.cs`)
```csharp
velX = body.angularVelocity;  // Changes over time!
Vector2 vel = new Vector2(velX * scaleFactor, velY);
body.AddForce(curl * vel, ForceMode2D.Force);
```

**Key insight**: Curl force is proportional to **angular velocity**, which:
1. Starts high (from initial torque)
2. Decays **slower** than linear velocity
3. Creates **increasing curl ratio** as rock slows = parabolic trajectory!

### Old Simulator (WRONG)
```csharp
// Constant lateral velocity - NO angular velocity decay!
Vector2 curlForce = curlDirection * curlStrength;
velocity += curlForce * TIME_STEP;
```

This created a **constant curl rate**, not the accelerating curl you see in real curling.

---

## The Fix

### New Angular Velocity Model
```csharp
// EXACT MATCH to Rock_Force.cs!
float velX = angularVelocity * scaleFactor;
Vector2 curlForce = new Vector2(curlVector.x * dirMult * velX, 0f);
Vector2 velocityChange = curlForce * TIME_STEP / rockMass;
velocity += velocityChange * curlForceScale;

// KEY: Angular velocity decays SLOWER than linear velocity
angularVelocity *= (1.0f - angularDamping * TIME_STEP);
```

---

## Tuning Parameters (Unity Inspector)

### In `TrajectoryLine` Component

#### **1. curlVector** (Vector2)
- **Default**: `(0.323, 0)`
- **What it does**: Matches `Rock_Force.curl.x` - controls curl direction and strength
- **How to tune**: 
  - If trajectory curls **opposite** to rocks: Flip the sign (`0.323` ? `-0.323`)
  - If trajectory curls **more** than rocks: Reduce magnitude (`0.323` ? `0.25`)
  - If trajectory curls **less** than rocks: Increase magnitude (`0.323` ? `0.4`)

#### **2. scaleFactor** (float, 0.05-0.2)
- **Default**: `0.1`
- **What it does**: Matches `Rock_Force.scaleFactor` - multiplies angular velocity
- **How to tune**:
  - Increase = More curl overall
  - Decrease = Less curl overall
  - Should match `Rock_Force.cs` value exactly

#### **3. initialAngularVelocity** (float, 30-90)
- **Default**: `60` rad/s
- **What it does**: Matches `Rock_Force.turnValue` - initial spin rate
- **How to tune**:
  - Increase = More curl (especially early)
  - Decrease = Less curl (especially early)
  - Should match `Rock_Force.turnValue` exactly

#### **4. angularDamping** (float, 0.01-0.2)
- **Default**: `0.05`
- **What it does**: How fast spin decays - **KEY TO PARABOLIC CURL!**
- **How to tune**:
  - **Lower** (0.01-0.03) = Spin lasts longer = **MORE late-breaking curl** ??
  - **Higher** (0.1-0.2) = Spin dies fast = **LESS late-breaking curl**
  - **This is the most important parameter for matching the parabolic shape!**

#### **5. curlForceScale** (float, 0.1-2.0)
- **Default**: `0.5`
- **What it does**: Final calibration multiplier for curl force
- **How to tune**:
  - If trajectory **curls less** than rocks despite correct shape: Increase
  - If trajectory **curls more** than rocks despite correct shape: Decrease
  - Start low and increase gradually

#### **6. iceFriction** (float)
- **Default**: `0.42`
- **What it does**: Linear velocity damping - affects **distance**, not curl
- **How to tune**:
  - Must match `Rock.Rigidbody2D.linearDamping` (0.38 in game)
  - If rocks travel **farther** than trajectory: Increase
  - If rocks travel **shorter** than trajectory: Decrease

---

## Calibration Workflow

### Step 1: Match Basic Curl Direction
1. Throw an **in-turn** rock with moderate speed
2. Watch if trajectory curls LEFT (correct) or RIGHT (wrong)
3. If wrong direction: **Flip `curlVector.x` sign**

### Step 2: Match Curl Amount
1. Compare **total lateral displacement** at the end
2. Adjust `curlForceScale`:
   - Trajectory ends 0.5m left of rock? Increase scale by ~0.2
   - Trajectory ends 0.5m right of rock? Decrease scale by ~0.2

### Step 3: Match Parabolic Shape (Late Breaking)
1. Watch curl behavior at **different speeds**:
   - **Fast rocks**: Should curl only near the end
   - **Slow rocks**: Should curl throughout
2. Adjust `angularDamping`:
   - **Late breaking too subtle?** Lower damping (0.03)
   - **Late breaking too dramatic?** Raise damping (0.08)

### Step 4: Fine-Tune Curl Magnitude
1. Adjust `scaleFactor` to match overall curl strength
2. Adjust `initialAngularVelocity` if curl differs early vs late

---

## Expected Behavior

### ? Correctly Tuned Trajectory
- **First 1/3**: Nearly straight, minimal curl
- **Middle 1/3**: Gradual curl acceleration
- **Final 1/3**: **Dramatic curl increase** (late break!)
- **Total path**: Parabolic curve matching real rocks

### ? Incorrectly Tuned Signs
- **Linear curl**: angularDamping too high or curlForceScale too low
- **Opposite direction**: curlVector.x sign flipped
- **Too much/little curl**: curlForceScale needs adjustment

---

## Debug Tips

### View Curl Force in Console
The simulator logs curl details when rocks are slow:
```
[Curl @ slow] speed=0.85, angVel=12.5, velX=1.25, curlForce=(0.4, 0), velChange=(0.008, 0)
```

- `angVel`: Spin rate (should start ~60 and decay)
- `velX`: angVel × scaleFactor
- `curlForce`: Lateral force applied
- `velChange`: How much velocity changed this frame

### Test at Different Speeds
1. **Slow draw** (light pull): Watch late break
2. **Medium draw** (normal pull): Watch mid-game curl
3. **Fast takeout** (hard pull): Watch early curl

All should have **same parabolic shape**, just scaled by speed!

---

## Summary: What Changed

### Before
- ? Constant lateral velocity
- ? Linear curl throughout path
- ? No late-breaking effect
- ? Doesn't match real physics

### After
- ? Angular velocity-dependent curl
- ? Parabolic curl trajectory
- ? Dramatic late-breaking effect
- ? Matches `Rock_Force.cs` physics exactly

---

## Quick Reference

| Parameter | Default | Purpose |
|-----------|---------|---------|
| `curlVector.x` | 0.323 | Curl direction/strength |
| `scaleFactor` | 0.1 | Angular velocity multiplier |
| `initialAngularVelocity` | 60 | Starting spin rate |
| `angularDamping` | **0.05** | **Spin decay (KEY!)** |
| `curlForceScale` | 0.5 | Final calibration |
| `iceFriction` | 0.42 | Distance tuning |

**Most important for parabolic curl**: `angularDamping` (lower = more late break!)

---

## Real Curling Physics Notes

In real curling:
- Rocks have **~60 rad/s** initial rotation (3-4 rotations/second)
- Angular velocity decays due to air resistance and ice friction
- **BUT**: Angular velocity decays **much slower** than linear velocity
- This creates the **curl ratio increase** = late breaking!

The simulator now models this correctly! ??
