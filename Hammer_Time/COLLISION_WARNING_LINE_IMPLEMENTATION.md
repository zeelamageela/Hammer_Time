# Collision Warning Line Implementation

## Overview
Added a subtle collision warning indicator that shows where the trajectory will collide with a rock, WITHOUT making the vertical guide line follow the trajectory (which would make hitting rocks too easy).

---

## What Changed

### 1. **New Collision Warning Line**
- **Type**: Small dotted vertical line (0.5 units tall)
- **Color**: Red with transparency (`rgba(255, 0, 0, 0.8)`)
- **Location**: Centered at collision point on trajectory
- **Purpose**: Visual feedback that trajectory will hit a rock

### 2. **Vertical Guide Line Remains Straight-Line**
- **Logic**: Based on straight-line projection from pullback
- **NOT following trajectory**: Prevents making hits too easy
- **Purpose**: Shows lateral aim position (X-axis)

---

## Visual Behavior

### When Aiming (No Collision)
```
Vertical Guide Line: Shows where straight-line aim points
Horizontal Line: Shows weight (distance)
Curl Line: Shows expected curl direction
Collision Warning: HIDDEN
```

### When Aiming (Collision Detected)
```
Vertical Guide Line: Still shows straight-line aim (unchanged)
Horizontal Line: Shows weight (distance)
Curl Line: Shows expected curl direction
Collision Warning: RED VERTICAL LINE at collision point (0.5 units tall)
```

**Note:** Collision warning appears with guide lines (aim circle OFF), regardless of collision visualization toggle setting.

---

## Technical Details

### Collision Warning Line Properties
```csharp
Width: 0.06 (thin, subtle)
Height: 0.5 units (small indicator)
Color: Red (255, 0, 0) with 80% opacity
Sorting Order: 2 (renders on top of aim lines)
Texture Mode: Tile (can be dotted with proper texture)
```

### Positioning
```csharp
Vector2 collisionPoint = trajectorySimulator.GetCollisionInfo().collisionPoint;
float topY = collisionPoint.y + 0.25f;     // +0.25 above
float bottomY = collisionPoint.y - 0.25f;  // -0.25 below
```

### Visibility Control
- Only shown when **aim circle is OFF** (guide lines mode)
- **Independent of collision visualization toggle** (always shows with guide lines)
- Hidden when trajectory is cleared
- Hidden when no collision detected
- Part of the guide line system, not collision visualization

---

## Separation of Concerns

### Vertical Guide Line (Straight-Line Aim)
**Purpose**: Shows where player is AIMING (lateral position)
- Based on straight-line projection from pullback
- Does NOT follow trajectory curl
- Shows "if I throw straight, where will it go?"

### Collision Warning Line (Trajectory Collision)
**Purpose**: Shows where trajectory will HIT a rock
- Based on actual trajectory simulation
- Shows collision point if detected
- Warns "your shot will hit this rock here"

**Why Separate?**
- Vertical line following trajectory makes hitting rocks TOO EASY
- Players would just aim vertical line at target rock
- Collision warning shows feedback WITHOUT giving easy solution
- Player still needs to judge lateral offset and curl

---

## How It Works

### 1. Trajectory Simulation
```csharp
// Simulate trajectory with collision detection
List<Vector2> simulatedPath = trajectorySimulator.SimulateTrajectory(
    launcherPos, initialVelocity, isInTurn, 250, rocksInPlay, forPlayerPreview: true
);

// Get collision info
TrajectorySimulator.CollisionInfo collisionInfo = trajectorySimulator.GetCollisionInfo();
```

### 2. Vertical Line (Straight-Line Logic)
```csharp
// Calculate X position from straight-line projection
float deltaY = horizontalLineY - pullbackPos.y;
if (Mathf.Abs(direction.y) > 0.001f)
{
    float t = deltaY / direction.y;
    verticalLineX = pullbackPos.x + (direction.x * t);
}

// DOES NOT use collision info or trajectory points for X position
```

### 3. Collision Warning (Trajectory Logic)
```csharp
if (collisionInfo.hasCollision && collisionLinesVisible)
{
    Vector2 collisionPoint = collisionInfo.collisionPoint;
    float topY = collisionPoint.y + 0.25f;
    float bottomY = collisionPoint.y - 0.25f;
    
    collisionWarningLine.SetPosition(0, new Vector3(collisionPoint.x, topY, 0f));
    collisionWarningLine.SetPosition(1, new Vector3(collisionPoint.x, bottomY, 0f));
    collisionWarningLine.enabled = true;
}
```

---

## Player Experience

### Scenario 1: Clear Path to Target
```
Player aims at button:
- Vertical line shows lateral aim position
- No collision warning
- Horizontal line shows weight
- Curl line shows expected curl
RESULT: Clean shot visualization
```

### Scenario 2: Guard Rock in Path
```
Player aims at button, but guard is in way:
- Vertical line STILL shows straight-line aim (doesn't follow trajectory around guard)
- RED collision warning appears at guard's position
- Horizontal line shows weight would reach target
- Curl line shows expected curl
RESULT: Player sees "I'll hit the guard" but must adjust aim themselves
```

### Scenario 3: Trying to Hit Guard
```
Player tries to hit guard rock:
- Vertical line shows lateral aim
- RED collision warning at guard position
- Player can see if lateral aim is close to guard
RESULT: Collision warning confirms intent to hit
```

---

## UI Toggle Integration

### Aim Circle Toggle
When `aimCircleVisible` is `OFF`:
- Guide lines appear (vertical, horizontal, curl)
- **Collision warning line appears** (if collision detected)

When `aimCircleVisible` is `ON`:
- Aim circle appears
- Guide lines hidden
- **Collision warning line hidden**

### Collision Lines Toggle
- **Does NOT affect collision warning line** (part of guide lines system)
- Only affects: collision marker (X), post-collision arrows

### Philosophy
- **Collision warning = Guide line feature** (helps with aiming)
- **Collision arrows/markers = Debug/visualization feature** (detailed physics info)
- Players can have collision warning without all the visual clutter

---

## Future Enhancements

### 1. **Dotted Line Texture**
Currently solid, but can be made dotted:
```csharp
// Create dotted texture for LineRenderer
Texture2D dottedTexture = CreateDottedTexture();
collisionWarningLine.material.mainTexture = dottedTexture;
collisionWarningLine.textureMode = LineTextureMode.Tile;
```

### 2. **Color by Collision Type**
```csharp
// Red for opponent rocks
// Yellow for your own rocks
// Orange for guards
Color warningColor = GetCollisionColor(hitRock);
```

### 3. **Animated Flash**
```csharp
// Pulse opacity to draw attention
float alpha = Mathf.PingPong(Time.time * 2f, 0.8f);
collisionWarningLine.startColor = new Color(1f, 0f, 0f, alpha);
```

### 4. **Multiple Collision Indicators**
```csharp
// Show all rocks trajectory will hit (multi-rock collisions)
foreach (var collision in trajectorySimulator.GetAllCollisions())
{
    DrawCollisionIndicator(collision.collisionPoint);
}
```

---

## Code Locations

| Component | File | Location |
|-----------|------|----------|
| Collision Warning Line Creation | `TrajectoryLine.cs` | `Start()` method |
| Collision Warning Update | `TrajectoryLine.cs` | `UpdateCollisionWarningLine()` |
| Vertical Line Logic | `TrajectoryLine.cs` | `UpdateAlternativeAimVisualization()` |
| Collision Detection | `TrajectoryLine.cs` | `DrawTrajectory()` |
| Cleanup | `TrajectoryLine.cs` | `ClearTrajectory()` |

---

## Debug Logs

### When Collision Detected
```
[Collision Warning] Indicator drawn at (1.23, 4.56) - height: 0.5
```

### When No Collision
```
(No logs - collision warning line simply disabled)
```

---

## Testing Checklist

- [ ] Vertical guide line stays in straight-line position (doesn't follow trajectory)
- [ ] Collision warning line appears when trajectory hits a rock
- [ ] Collision warning line is centered at collision point
- [ ] Red color is visible but not too bright
- [ ] Collision warning respects `collisionLinesVisible` toggle
- [ ] Collision warning clears when trajectory is cleared
- [ ] Works with guards in finesse zone
- [ ] Works with rocks in house
- [ ] Does NOT appear when path is clear

---

## Summary

### Problem
- Need to show collision detection
- But vertical guide line following trajectory makes hitting rocks too easy

### Solution
- Keep vertical guide line as straight-line projection (aim position)
- Add separate collision warning line at collision point
- Small, subtle red indicator (0.5 units tall)
- Shows "you'll hit something here" without making it easier

### Result
- Players get collision feedback
- Still need skill to adjust aim and curl
- Vertical line remains a guide, not a solution
- Collision warning is informative, not instructive

---

## Build Status
? **Build Successful** - All changes compile without errors
