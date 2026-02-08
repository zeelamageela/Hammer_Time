# Collision Visualization: Directional Arrows

## What Changed

The collision visualization now shows **directional arrows radiating from the collision point** instead of full trajectory paths. This makes it much easier to understand the deflection angles at a glance.

---

## Visual Design

### Before (Full Paths)
- **Orange line**: Full curved path showing where shooter rock rolls after collision
- **Yellow line**: Short 3-point path showing hit rock's initial direction
- **Problem**: Hard to see the actual deflection angle, paths overlap

### After (Directional Arrows)
- **?? Orange Arrow**: Shooter rock's exit direction (1.5 units long)
- **?? Yellow Arrow**: Hit rock's exit direction (1.5 units long)
- **? Benefit**: Clear, simple visualization of deflection angles

---

## Arrow Details

### Orange Arrow (Shooter Rock)
- **Start**: Collision point
- **End**: 1.5 units in deflection direction
- **Width**: 0.25 units (start) ? 0.15 units (end) - tapers like an arrow
- **Color**: Bright orange `(1.0, 0.5, 0.0)` ? slightly transparent `(1.0, 0.3, 0.0, 0.8)`
- **Meaning**: "Your rock will bounce off in THIS direction"

### Yellow Arrow (Hit Rock)
- **Start**: Target rock's position at collision
- **End**: 1.5 units in exit direction
- **Width**: 0.25 units (start) ? 0.15 units (end) - tapers like an arrow
- **Color**: Bright yellow `(1.0, 1.0, 0.0)` ? slightly transparent `(1.0, 1.0, 0.0, 0.8)`
- **Meaning**: "Target rock will fly off in THIS direction"

---

## Use Cases

### 1. **Takeout Shots**
When aiming at a rock dead-center:
- **Orange arrow**: Points **backward/sideways** (shooter bounces back)
- **Yellow arrow**: Points **forward** (target flies toward house)
- **Angle between arrows**: ~180° (head-on collision)

### 2. **Glancing Hits**
When hitting a rock at an angle:
- **Orange arrow**: Curves **sideways** (shooter deflects)
- **Yellow arrow**: Angles **diagonally** (target nudges sideways)
- **Angle between arrows**: ~90-120° (oblique collision)

### 3. **Tick Shots**
Very light contact, just nudging:
- **Orange arrow**: Nearly **straight** (shooter keeps moving)
- **Yellow arrow**: Small **sideways** deflection (target barely moves)
- **Angle between arrows**: ~60-90° (minimal deflection)

### 4. **Combination Shots**
Planning a "hit and roll":
- **Orange arrow** shows where YOUR rock will end up
  - Does it stay in the house? ?
  - Does it roll behind guards? ?
  - Does it go out of play? ?
- **Yellow arrow** shows where TARGET rock goes
  - Does it knock out another rock? ??
  - Does it clear a path? ?

---

## Arrow Length Tuning

The arrow length is **1.5 units** by default. You can adjust this:

```csharp
// In TrajectoryLine.cs, line ~XXX
float arrowLength = 1.5f; // Adjust this value
```

### Recommended Values
- **0.5 units**: Very short, shows direction only
- **1.0 units**: Short, good for tight spaces
- **1.5 units**: Default, good balance (? 5 rock diameters)
- **2.0 units**: Long, shows more trajectory
- **3.0 units**: Very long, almost reaches final positions

**Tip**: Longer arrows show more trajectory but can clutter the screen. Shorter arrows are cleaner but less informative.

---

## Console Debug Output

When a collision is predicted, you'll see:

```
[Collision Viz] Shooter arrow (ORANGE): (0.20, 6.50) ? (0.35, 5.25) (angle: -56.3°)
[Collision Viz] Hit rock arrow (YELLOW): (0.25, 6.55) ? (0.30, 8.05) (angle: 86.7°)
```

This tells you:
- **Start point**: Where collision happens
- **End point**: Where arrow points to
- **Angle**: Direction in degrees (0° = right, 90° = up, -90° = down, ±180° = left)

---

## Physics Accuracy

The arrows show the **actual physics-simulated directions**, not geometric approximations:

? **Accounts for**:
- Rock masses (equal in curling)
- Collision elasticity (RESTITUTION = 0.85)
- Energy loss (COLLISION_DAMPING = 0.7)
- Tangential vs normal velocity components
- Post-collision friction

? **Does NOT account for** (intentionally):
- Curl after collision (arrows are straight lines)
- Sweeping effects (not known at prediction time)
- Rock-to-rock friction during contact

**Why straight arrows?** After a collision, rocks tumble and lose controlled rotation. Curl effects are minimal, so straight-line projections are accurate enough.

---

## Combination Shot Planning

### Example: Hit and Roll to Button

**Goal**: Hit opponent rock, have YOUR rock roll to button

**What to look for**:
1. **Yellow arrow** points away from house ? (opponent rock removed)
2. **Orange arrow** points toward center ? (your rock stays in)
3. Arrow length suggests rock will stop near button ?

**Adjustment**:
- If orange arrow too short ? Pull back harder (more speed)
- If orange arrow too long ? Pull back lighter (less speed)
- If orange arrow wrong angle ? Adjust lateral position

### Example: Double Takeout

**Goal**: Hit rock A, which then hits rock B

**What to look for**:
1. **Yellow arrow** points toward rock B ?
2. Arrow length reaches rock B ? (enough speed to make contact)
3. **Orange arrow** points safe direction ? (your rock won't interfere)

**Implementation**:
```csharp
// In AI_Target.cs, CalculatePhysicsBasedShot()
// Check if hit rock's path intersects second target
Vector2 hitRockDirection = (hitRockSecond - hitRockStart).normalized;
foreach (var secondTarget in opponentRocks)
{
    float distToPath = DistanceToLine(
        hitRockStart, 
        hitRockDirection, 
        secondTarget.transform.position
    );
    
    if (distToPath < 0.3f) // Within rock radius
    {
        score += 20f; // HUGE bonus for double!
        Debug.Log($"[DOUBLE TAKEOUT] Possible!");
    }
}
```

---

## Troubleshooting

### Arrows Not Showing
**Check**:
1. Is `showCollisionPrediction` enabled? (Inspector)
2. Is there actually a collision? (no obstacles = no arrows)
3. Console shows `[Collision Viz]` logs?

### Arrows Point Wrong Direction
**Check**:
1. Console logs show angle values - do they match visual?
2. Are you pulling back far enough? (need 2+ trajectory points for direction)
3. Is collision detection working? (marker shows at collision point?)

### Arrows Too Short/Long
**Adjust**:
```csharp
float arrowLength = 1.5f; // Change this value (0.5 - 3.0 typical)
```

Rebuild and test different values until it looks right.

---

## Future Enhancements

### Possible Additions

1. **Arrowheads**: Add actual triangle arrowheads at the end
   ```csharp
   // Draw small triangle at arrow end to show direction
   DrawArrowhead(arrowEnd, direction, 0.2f);
   ```

2. **Speed Indicators**: Color-code by speed
   ```csharp
   // Bright = fast, dim = slow
   float speedRatio = postCollisionSpeed / initialSpeed;
   Color arrowColor = Color.Lerp(dimOrange, brightOrange, speedRatio);
   ```

3. **Dotted vs Solid**: Certainty indicator
   ```csharp
   // Solid = high confidence, dotted = uncertain
   if (collisionInfo.isGlancing)
       arrow.material = dottedLineMaterial;
   ```

4. **Multiple Collisions**: Chain reactions
   ```csharp
   // Show 2nd, 3rd order collisions with different colors
   if (secondaryCollision)
       DrawArrow(point, direction, Color.cyan);
   ```

---

## Summary

### What You Get
- ? **Clear visualization** of collision outcomes
- ? **Orange arrow** = shooter's deflection
- ? **Yellow arrow** = target's exit
- ? **Simple, uncluttered** display
- ? **Physics-accurate** directions

### How to Use
1. Aim at target rock
2. Pull back to desired speed
3. **Orange arrow** shows where YOUR rock goes
4. **Yellow arrow** shows where TARGET rock goes
5. Adjust aim until both arrows point where you want

Perfect for planning **takeouts**, **raises**, **ticks**, and **combination shots**! ??
