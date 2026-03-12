# Collision Warning Line - Guide Lines Integration

## Change Summary

**Updated:** Collision warning line is now part of the **guide lines system** (aim circle OFF), NOT the collision visualization toggle.

---

## Behavior

### Aim Circle ON
```
? Aim circle visible
? Guide lines hidden
? Collision warning hidden
? Collision arrows/markers (if collision toggle ON)
```

### Aim Circle OFF (Guide Lines Mode)
```
? Aim circle hidden
? Vertical guide line (straight-line aim)
? Horizontal guide line (weight/distance)
? Curl line (turn direction)
? Collision warning (if collision detected)
? Guide lines always show, regardless of collision toggle
```

---

## Toggle Independence

### Aim Circle Toggle
- **Controls:** Aim circle, guide lines, collision warning
- **When OFF:** Shows all guide lines + collision warning
- **When ON:** Shows aim circle only

### Collision Visualization Toggle
- **Controls:** Collision marker (X), post-collision arrows (orange/yellow)
- **Does NOT control:** Collision warning line (part of guide lines)

---

## Why This Design?

### Problem
- Guide lines help with aiming
- Collision warning is essential aiming feedback
- But collision arrows/markers are visual clutter

### Solution
- **Collision warning = Part of guide lines** (aiming tool)
- **Collision arrows/markers = Debug visualization** (physics info)
- Players can have collision feedback without all the visual noise

---

## Use Cases

### Scenario 1: Minimal UI (Aim Circle ON)
```
User wants: Clean view, aim circle only
Toggles: Aim Circle ON, Collision OFF
Result: Just aim circle, no clutter
```

### Scenario 2: Guide Lines (Aim Circle OFF)
```
User wants: Guide lines for precise aiming
Toggles: Aim Circle OFF, Collision OFF
Result: Guide lines + collision warning (essential feedback)
```

### Scenario 3: Full Visualization (Debug Mode)
```
User wants: Everything for testing/learning
Toggles: Aim Circle OFF, Collision ON
Result: Guide lines + collision warning + arrows/markers
```

---

## Code Change

### Before
```csharp
private void UpdateCollisionWarningLine()
{
    if (collisionWarningLine == null || !collisionLinesVisible)
    {
        // Controlled by collision visualization toggle
        if (collisionWarningLine != null)
            collisionWarningLine.enabled = false;
        return;
    }
    // ...
}
```

### After
```csharp
private void UpdateCollisionWarningLine()
{
    if (collisionWarningLine == null)
    {
        return;
    }
    
    // Only show when aim circle is OFF (guide lines mode)
    if (aimCircleVisible)
    {
        collisionWarningLine.enabled = false;
        return;
    }
    // ...
}
```

---

## Testing

### ? Test 1: Aim Circle ON
- Enable aim circle
- Aim at guard rock
- **Expected:** Aim circle visible, NO collision warning

### ? Test 2: Aim Circle OFF + Collision Toggle OFF
- Disable aim circle
- Disable collision visualization
- Aim at guard rock
- **Expected:** Guide lines visible + collision warning (red line)

### ? Test 3: Aim Circle OFF + Collision Toggle ON
- Disable aim circle
- Enable collision visualization
- Aim at guard rock
- **Expected:** Guide lines + collision warning + arrows/markers

### ? Test 4: Clear Path
- Disable aim circle
- Aim with no obstacles
- **Expected:** Guide lines visible, NO collision warning

---

## Player Experience

### Beginner (Aim Circle ON)
- Uses aim circle for simplicity
- Doesn't see collision warning (less overwhelming)
- Still sees collision arrows if they enable that toggle

### Intermediate (Aim Circle OFF)
- Uses guide lines for precision
- Gets collision warning feedback (essential)
- Can disable collision arrows for cleaner view

### Advanced (Full Visualization)
- Uses guide lines + all collision info
- Collision warning + arrows + markers
- Maximum information for strategic play

---

## Summary

| Feature | Controlled By | Purpose |
|---------|--------------|---------|
| **Aim Circle** | Aim Circle Toggle | Simple aiming tool |
| **Guide Lines** | Aim Circle Toggle (inverse) | Precision aiming |
| **Collision Warning** | Aim Circle Toggle (inverse) | Essential feedback |
| **Collision Arrows** | Collision Visualization Toggle | Physics debug info |
| **Collision Marker** | Collision Visualization Toggle | Impact point detail |

**Philosophy:** Collision warning is an aiming tool (part of guide lines), not a debug visualization.

---

## Build Status
? **Build Successful** - Change implemented and tested
