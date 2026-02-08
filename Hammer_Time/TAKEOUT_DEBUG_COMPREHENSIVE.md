# Comprehensive Takeout Debug System

## Changes Made

### 1. ? Smaller Collision Arrows
- **Arrow length**: Reduced from 1.5 units ? **0.8 units**
- **Arrow width**: Reduced from 0.25/0.15 ? **0.15/0.08 units**
- **Result**: Less visual clutter, easier to see the shot

### 2. ? Comprehensive Debug Logging

Added detailed logging at **3 critical points** in the AI takeout pipeline:

#### Point 1: AI_Target.TakeOutTarget()
Logs **BEFORE**, **DURING**, and **AFTER** physics calculation:

```
========== TAKEOUT DEBUG START ==========
Target Rock: #5 at position (0.25, 6.5)
Current rm.inturn BEFORE calculation: false
Shooter: Rock #2

[AI_Target] Physics recommends: OUT-TURN
Setting rm.inturn = false
[AI_Target] Confirmed rm.inturn is now: false

[AI_Target] Shooter skill: 100/100
Max error: 0.050
Actual error: 0.023
Original pullback: (0.12, -27.3)
Final pullback: (0.143, -27.277)

[AI_Target] Take Out SUCCESS
Target: (0.25, 6.5)
Pullback: (0.143, -27.277)
Turn: OUT-TURN (curl LEFT)
========== TAKEOUT DEBUG END ==========
```

#### Point 2: AI_Shooter.OnShot()
Logs what **AI_Shooter receives** from AI_Target:

```
[AI_Shooter] Executing Take Out:
rm.inturn = false (OUT-TURN)
Pullback will be: (0.143, -27.277)
```

#### Point 3: TrajectorySimulator (Already Exists)
Logs the actual **collision physics**:

```
[Collision] Incoming: 92.6°, Exit: -45.0°, HitRock: 106.9°, Normal: 106.9°
InVel: 11.59, OutVel: 6.82, HitVel: 5.34
```

---

## How to Use the Debug System

### Step 1: Watch the Console During Takeouts

Run a test game and watch an AI takeout. You should see **all three debug blocks** in order:

1. **AI_Target** - Shows physics calculation
2. **AI_Shooter** - Shows execution
3. **Collision** - Shows actual collision physics

### Step 2: Check for Consistency

**Question 1: Is the turn direction consistent?**

Look for:
```
rm.inturn BEFORE calculation: false
Physics recommends: OUT-TURN
Setting rm.inturn = false
Confirmed rm.inturn is now: false

[AI_Shooter] rm.inturn = false (OUT-TURN)
```

? **GOOD**: All say `false` (or all say `true`)  
? **BAD**: Values change between logs

**Question 2: Does the turn match the target position?**

If target rock is at `x = 0.5` (RIGHT of center):
- **IN-TURN** should curl RIGHT (away from center)
- **OUT-TURN** should curl LEFT (toward center ? BETTER for hitting)

```
Target: (0.5, 6.5)  ? RIGHT side
Physics recommends: OUT-TURN  ? Curls LEFT (toward target) ?
```

**Question 3: Is the collision happening?**

```
[Collision] Incoming: 92.6°, Exit: 92.6°, HitRock: 0.0°
```

? **BAD**: Exit angle = Incoming angle, HitRock velocity = 0  
? **GOOD**: Exit angle ? Incoming angle, HitRock has velocity

---

## Diagnosing Common Issues

### Issue 1: Turn Direction is Backwards

**Symptoms:**
```
Target: (0.5, 6.5)  ? RIGHT of center
Physics recommends: IN-TURN  ? Curls RIGHT (away) ?
```

**Cause**: Physics simulator has turn directions swapped

**Fix**: Flip the curl direction in `TrajectorySimulator.cs`:
```csharp
Vector2 curlDirection = isInTurn 
    ? new Vector2(-velocity.y, velocity.x).normalized  // SWAP
    : new Vector2(velocity.y, -velocity.x).normalized; // SWAP
```

### Issue 2: Turn Changes Between Calculation and Execution

**Symptoms:**
```
[AI_Target] Setting rm.inturn = false
[AI_Shooter] rm.inturn = true  ? DIFFERENT!
```

**Cause**: Something is overwriting `rm.inturn` between AI_Target and AI_Shooter

**Fix**: Check for code that sets `rm.inturn` after `AI_Target.TakeOutTarget()` runs

### Issue 3: Collision Not Happening

**Symptoms:**
```
[Collision] Exit: 92.6°, HitRock: 0.0°
```

**Cause**: Rocks passing through each other (already fixed)

**Fix**: Already applied - collision response condition was backwards

### Issue 4: Missing to One Side Consistently

**Symptoms:**
```
Target: (0.3, 6.5)  ? Target
Actual hit: (0.5, 6.5)  ? Always misses RIGHT
```

**Cause**: Lateral compensation is wrong direction or too strong

**Look for**:
```
lateralExtension = lateralDistance * lateralDistance * 0.15f
```

**Try**: Reduce to `0.10f` or even `0.05f`

---

## What Each Angle Means

### Collision Angles Breakdown

```
[Collision] Incoming: 92.6°, Exit: -45.0°, HitRock: 106.9°, Normal: 106.9°
```

**Incoming (92.6°)**:
- Direction the thrown rock was traveling BEFORE collision
- 90° = straight up, 0° = right, 180° = left

**Exit (-45.0°)**:
- Direction the thrown rock travels AFTER collision
- Should be DIFFERENT from incoming (if collision worked)
- Negative = deflected backward/sideways

**HitRock (106.9°)**:
- Direction the target rock flies off
- Should be close to the collision normal
- 0° = no velocity (bad - rock didn't move)

**Normal (106.9°)**:
- The line between the two rock centers at collision
- Defines the "axis" of the collision
- HitRock should travel close to this angle

### Good Collision Example

```
Incoming: 85.0°  ? Straight up
Exit: 45.0°      ? Deflected sideways
HitRock: 92.0°   ? Target flies forward
Normal: 90.0°    ? Head-on collision
```

**Analysis**: Thrown rock approaching from below (85°), hits target head-on (normal 90°), deflects sideways (45°), target flies forward (92°) ?

### Bad Collision Example (Fixed Now)

```
Incoming: 92.6°  ? Straight up
Exit: 92.6°      ? SAME ANGLE (no deflection) ?
HitRock: 0.0°    ? Target didn't move ?
Normal: 106.9°   ? Collision detected but not resolved
```

**Analysis**: Rocks passed through each other - collision detected but response skipped

---

## Expected Results

### Perfect Shot (100 Skill)

```
Target: (0.0, 6.5)
Physics recommends: IN-TURN (or OUT-TURN, doesn't matter for center)
Shooter skill: 100/100
Max error: 0.050
Actual error: 0.023  ? Small
Final pullback: (0.012, -27.289)

[Collision] Incoming: 90.1°, Exit: -88.5°, HitRock: 90.0°
```

**Result**: **Near-perfect hit** - small error, clean collision

### Average Shot (50 Skill)

```
Target: (0.5, 6.5)
Physics recommends: OUT-TURN
Shooter skill: 50/100
Max error: 0.150
Actual error: 0.098  ? Medium
Final pullback: (0.234, -27.201)

[Collision] Incoming: 88.3°, Exit: -52.1°, HitRock: 95.2°
```

**Result**: **Hit with some error** - rock deflects but collision succeeds

### Miss (Poor Skill or Unlucky)

```
Target: (0.8, 6.5)
Physics recommends: OUT-TURN
Shooter skill: 30/100
Max error: 0.190
Actual error: 0.156  ? Large!
Final pullback: (0.456, -27.112)

[Collision] No collision detected
```

**Result**: **Miss** - too much error, rock passed wide

---

## Next Steps

1. **Run a test game** with AI takeouts
2. **Copy the console output** to a text file
3. **Share it** so we can analyze:
   - Is turn direction correct?
   - Are values consistent?
   - Is collision working?
   - Are misses due to error or calculation?

With this debug system, we can **see exactly** what's happening at every step and fix the root cause! ??
