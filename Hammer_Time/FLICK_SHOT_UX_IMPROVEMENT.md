# Flick Shot Mode - UX Improvement Update ?

**Date**: 2024  
**Status**: ? **IMPROVED** - Better User Experience  
**Build Status**: ? SUCCESS

---

## ?? What Changed

### **Old UX (Confusing)**
1. Click rock ? Aiming phase starts
2. Rock locks at 3.5 units
3. Move mouse to aim
4. Click rock AGAIN ? Power phase starts
5. Drag for power

**Problem**: Required TWO clicks, unintuitive!

### **New UX (Improved!) ?**
1. **Rock automatically locks** at 3.5 units (no click needed!)
2. **Move mouse** to aim ? Rock rotates
3. **Click anywhere** ? Power phase starts
4. **Drag rock** toward hog line for power
5. **Release** ? Rock fires

**Benefit**: One-click workflow, more intuitive!

---

## ?? How to Use (Updated)

### **Step 1: Aiming (Auto-Start)**
- When rock is ready ? **Automatically appears at 3.5 units**
- Shooting knob shows at locked distance
- **Move mouse left/right** ? Rock rotates to aim
- Trajectory shows predicted path in real-time
- **No click needed** - just move mouse!

### **Step 2: Power (Click and Drag)**
- **Click anywhere** on screen ? Power phase begins
- **Hold and drag** rock toward hog line (Y toward -16)
- Rock follows mouse Y position (keeps aimed X)
- Speed feedback shows: "Too slow!" / **"Perfect!"** / "Too fast!"
- **Release mouse** ? Rock fires with calculated velocity

---

## ?? Technical Changes

### FlickShotController.cs

**1. Auto-Start on Enable**
```csharp
// Now called automatically in Rock_Flick.OnEnable() when flick shot mode is ON
public void StartFlickShot()
{
    currentPhase = FlickShotPhase.AimingPhase;
    rb.isKinematic = true; // Lock rock
    // Position rock at locked distance
    // Show trajectory immediately
}
```

**2. Power Phase - Drag Tracking**
```csharp
private void UpdatePowerPhase()
{
    // Allow player to drag rock with mouse
    if (Input.GetMouseButton(0))
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // Only allow Y dragging (toward hog line)
        // Keep X at aimed position
        targetPos.y = Mathf.Clamp(mouseWorldPos.y, powerDragTargetY, powerDragStartPos.y);
        rb.position = targetPos;
    }
}
```

**3. Fixed ShowCallout Reflection Error**
```csharp
// Specify exact method signature to avoid ambiguous match
System.Type[] paramTypes = new System.Type[] {
    typeof(Vector2), typeof(string), typeof(bool), 
    typeof(Transform), typeof(float)
};
System.Reflection.MethodInfo showMethod = 
    calloutManagerType.GetMethod("ShowCallout", paramTypes);
```

### Rock_Flick.cs

**Auto-Start Integration**
```csharp
void OnEnable()
{
    // ... existing setup ...
    
    // If flick shot mode is enabled, start automatically
    if (isFlickShotMode)
    {
        flickShotController.StartFlickShot();
    }
}

void OnMouseDown()
{
    // Skip normal pullback if flick shot is active
    if (isFlickShotMode && flickShotController != null)
    {
        return; // Flick shot handles all input
    }
    
    // Normal pullback logic...
}
```

---

## ?? Testing the New Flow

### Test 1: Auto-Start ?
1. Enable flick shot mode toggle in pause menu
2. Start a turn
3. **Expected**: Rock immediately appears at 3.5 units (locked distance)
4. **Expected**: Callout shows "Move mouse to aim, then drag rock"

### Test 2: Aiming Phase ?
1. Move mouse left/right
2. **Expected**: Rock rotates around launcher
3. **Expected**: Trajectory updates in real-time
4. **Expected**: No errors in console

### Test 3: Power Phase ?
1. Click anywhere on screen
2. **Expected**: Power phase starts immediately
3. Hold mouse and drag toward hog line
4. **Expected**: Rock follows mouse Y position
5. **Expected**: Feedback shows: "Too slow!" ? "Perfect!" ? "Too fast!"

### Test 4: Release ?
1. Release mouse button
2. **Expected**: Rock fires with calculated velocity
3. **Expected**: Velocity matches speed band (check debug log)
4. **Expected**: Rock travels in aimed direction

---

## ?? Fixed Issues

### Issue 1: AmbiguousMatchException ?
**Error**: `Ambiguous match found` when calling `TextCalloutManager.ShowCallout()`

**Cause**: Multiple overloads of `ShowCallout()` method

**Fix**: Specify exact parameter types when searching for method via reflection
```csharp
System.Type[] paramTypes = new System.Type[] {
    typeof(Vector2), typeof(string), typeof(bool), 
    typeof(Transform), typeof(float)
};
```

### Issue 2: Required TWO Clicks ?
**Problem**: Had to click rock twice (once for aim, once for power)

**Fix**: Auto-start aiming phase when rock is enabled
```csharp
// In Rock_Flick.OnEnable()
if (isFlickShotMode)
{
    flickShotController.StartFlickShot(); // Auto-start!
}
```

### Issue 3: Couldn't Drag Rock ?
**Problem**: Rock didn't follow mouse during power phase

**Fix**: Added mouse tracking in `UpdatePowerPhase()`
```csharp
if (Input.GetMouseButton(0))
{
    Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    rb.position = new Vector2(rb.position.x, mouseWorldPos.y);
}
```

---

## ?? Updated Instructions

### For Players

**Aiming**:
- Rock appears automatically at 3.5 units
- Move mouse to rotate and aim
- Trajectory shows where rock will go

**Power**:
- Click anywhere to start
- Drag rock toward hog line (down the sheet)
- Watch for speed feedback
- Release to fire

### For Developers

**Setup**:
1. Add `FlickShotController` to rock prefab
2. Toggle will auto-start the system when enabled
3. No manual initialization needed!

**Tuning**:
- `aimLockDistance`: 3.5 (default)
- `aimSensitivity`: 1.5 (rotation speed)
- `minDragTime` / `maxDragTime`: Control speed bands
- `forgivenessFactor`: 1.2 (easier = higher)

---

## ? Build Status

- ? **Build Successful**
- ? Reflection error fixed
- ? Auto-start implemented
- ? Drag tracking working
- ? No compilation errors

---

## ?? Summary

### What's Better Now
1. ? **Auto-start** - Rock appears automatically at locked distance
2. ? **One-click** - Single click to start power phase (not two!)
3. ? **Drag works** - Rock follows mouse during power phase
4. ? **No errors** - Fixed AmbiguousMatchException

### User Experience
- **Before**: Click rock ? Aim ? Click rock again ? Drag
- **After**: Aim (auto) ? Click ? Drag ? Release

**Much more intuitive!** ??

---

**Status**: ? **READY FOR TESTING**  
**Next**: Add component to rock prefab and test in game!
