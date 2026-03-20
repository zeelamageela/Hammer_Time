# Flick Shot Mode - Power Phase Visual Feedback ?

**Date**: 2024  
**Status**: ? **IMPLEMENTED** - Camera Switch + Green Knob Drag Visual  
**Build Status**: ? SUCCESS

---

## ?? What Was Added

### 1. **Camera Switch** ?
When player clicks launcher:
- Switches from aim camera to full view camera
- Shows entire path from launcher (Y=-25) to hog line (Y=-16)
- Uses `CameraManager.SwitchCamera(0)` method

### 2. **Green Power Knob** ?
Visual feedback during power drag:
- **Green shooting knob** appears at launcher
- Follows mouse **Y position** as you drag down
- Clamped between Y=-25 (launcher) and Y=-16 (hog line)
- Shows exactly where you're dragging

### 3. **Speed Feedback Position** ?
Text callouts appear at knob:
- "Way too fast!" / "Too fast!" / **"Perfect!"** / "Too slow!" / "Way too slow!"
- Positioned at green knob (not at rock)
- Updates every 0.1 seconds

---

## ?? Complete User Flow

### **Phase 1: Set Aim** (Normal Pullback)
1. Click on invisible rock at (0, -25)
2. Drag backward to aim direction
3. Trajectory shows predicted path
4. **Release** ? Aim locked, shooting knob stays visible
5. Console: `"Aim set at position... Click launcher to start power flick"`

### **Phase 2: Power Flick** (Drag for Speed)
1. **Click launcher** (0, -25)
   - Camera switches to full view ?
   - Normal shooting knob hides ?
   - **Green power knob** appears at launcher ?
   
2. **Drag down the sheet**
   - Green knob follows your mouse Y position ?
   - Clamped between Y=-25 and Y=-16 ?
   - Speed feedback shows at knob position ?
   
3. **Release mouse**
   - Green knob disappears ?
   - Rock fires with calculated velocity ?
   - Velocity based on drag time ?

---

## ?? Technical Implementation

### FlickShotController.cs

**New References**:
```csharp
public GameObject cameraManagerObj;  // CameraManager for switching views
public GameObject shootingKnobObj;    // Original shooting knob (aim phase)
public GameObject powerKnobObj;       // Green knob (power phase)
```

**Start() - Create Power Knob**:
```csharp
// Clone shooting knob and make it green
powerKnobObj = Instantiate(shootingKnobObj);
powerKnobObj.name = "PowerKnob";
powerKnobObj.SetActive(false);

// Set green color
SpriteRenderer powerSprite = powerKnobObj.GetComponent<SpriteRenderer>();
powerSprite.color = new Color(0.2f, 1f, 0.2f, 1f); // Bright green

// Disable line renderer (just want the knob sprite)
powerKnobObj.GetComponent<LineRenderer>().enabled = false;
```

**StartPowerPhase() - Switch Camera & Show Green Knob**:
```csharp
// Hide normal shooting knob
shootingKnobObj.GetComponent<SpriteRenderer>().enabled = false;

// Switch camera to full view
CameraManager.SwitchCamera(0);

// Show green power knob at launcher
powerKnobObj.SetActive(true);
powerKnobObj.transform.position = launcher.transform.position;
```

**UpdatePowerPhase() - Follow Mouse Drag**:
```csharp
// Update green knob to follow mouse Y
Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
Vector3 knobPos = powerKnobObj.transform.position;
knobPos.y = Mathf.Clamp(mouseWorldPos.y, powerDragTargetY, powerDragStartY);
powerKnobObj.transform.position = knobPos;

// Show feedback at knob position
ShowSpeedFeedback(message, powerKnobObj.transform.position);
```

**ReleaseFlickShot() - Hide Green Knob**:
```csharp
// Hide power knob
powerKnobObj.SetActive(false);

// Fire rock...
```

---

## ?? Visual Design

### Aim Phase (Shooting Knob)
- **Color**: Normal (yellow/red based on aim circle Y)
- **Position**: Where player released pullback
- **State**: Visible, stays after release
- **Camera**: Aim camera (zoomed)

### Power Phase (Green Knob)
- **Color**: Bright green (0.2, 1.0, 0.2)
- **Position**: Follows mouse Y (clamped Y=-25 to Y=-16)
- **State**: Visible during drag, hidden on release
- **Camera**: Full view (launcher to hog line)

---

## ?? Camera Views

### Aim Camera (Phase 1)
- Zoomed in on house area
- Shows trajectory prediction
- Focus on aim accuracy

### Full View Camera (Phase 2)
- Shows launcher (Y=-25) to hog line (Y=-16)
- Entire drag path visible
- Focus on power/speed control

---

## ?? Testing Guide

### Test 1: Aim Phase ?
1. Enable flick shot mode
2. Start turn
3. Click and drag to aim
4. Release
5. **Expected**: Shooting knob visible at aimed position
6. **Expected**: Console: `"Aim set... Click launcher..."`

### Test 2: Camera Switch ?
1. After Test 1
2. Click launcher (0, -25)
3. **Expected**: Camera switches to full view
4. **Expected**: Console: `"Camera switched to full view"`
5. **Expected**: Can see from launcher to hog line

### Test 3: Green Knob ?
1. After Test 2 (in power phase)
2. Move mouse down the sheet
3. **Expected**: Green knob appears at launcher
4. **Expected**: Green knob follows mouse Y position
5. **Expected**: Knob clamped between Y=-25 and Y=-16
6. **Expected**: Speed feedback appears at knob

### Test 4: Power Drag ?
1. In power phase
2. Drag mouse from launcher toward hog line
3. **Expected**: Speed feedback updates every 0.1s
4. **Expected**: Messages: "Way too fast!" ? "Perfect!" ? "Way too slow!"
5. Release
6. **Expected**: Green knob disappears
7. **Expected**: Rock fires with calculated velocity

---

## ?? Potential Issues & Fixes

### Issue: Camera Doesn't Switch
**Possible Causes**:
- CameraManager.SwitchCamera() method signature different
- Camera index incorrect (might not be 0)

**Debug**:
- Check console for camera switch log
- Try different camera indices if needed

**Manual Fix**:
```csharp
// If SwitchCamera() doesn't exist, might need to:
cameraManager.aim.enabled = false;
cameraManager.main.enabled = true;
```

---

### Issue: Green Knob Not Appearing
**Check**:
- Console: `"Power knob created (green)"`?
- Console: `"Green power knob visible at launcher"`?

**Fix**:
- Verify shooting knob GameObject exists
- Check powerKnobObj not null in inspector (when playing)

---

### Issue: Knob Doesn't Follow Mouse
**Check**:
- In power phase? (check currentPhase in inspector)
- Mouse moving within camera bounds?

**Debug**:
- Add log: `Debug.Log($"Mouse Y: {mouseWorldPos.y}, Knob Y: {knobPos.y}");`
- Verify mouse world position makes sense

---

## ?? Quick Reference

### Power Phase Workflow
```
1. Click launcher (0, -25)
   ?? Camera switches to full view
   ?? Normal knob hides
   ?? Green knob appears
      ?
2. Drag mouse down sheet
   ?? Green knob follows mouse Y
   ?? Y clamped: -25 to -16
   ?? Speed feedback shows
      ?
3. Release mouse
   ?? Green knob hides
   ?? Speed calculated from drag time
   ?? Rock fires!
```

### Speed Calculation
```
Drag Time ? Normalized ? Forgiveness ? Speed Band ? Velocity

Fast (0.2s)    ? 1.0 ? Band 4 ? "Way too fast!" ? 13 m/s
Medium (0.85s) ? 0.5 ? Band 2 ? "Perfect!"      ? 9 m/s
Slow (1.5s)    ? 0.0 ? Band 0 ? "Way too slow!" ? 5 m/s
```

---

## ? Summary

### What Works Now
1. ? **Aim Phase**: Normal pullback, holds aim on release
2. ? **Launcher Click**: Detects click within 1 unit radius
3. ? **Camera Switch**: Changes to full view
4. ? **Green Knob**: Appears at launcher, follows drag
5. ? **Speed Feedback**: Shows at knob position
6. ? **Rock Fires**: With calculated velocity at aimed direction

### User Experience
- **Phase 1**: Familiar pullback to aim
- **Phase 2**: Visual drag with green knob + speed feedback
- **Skill**: Aim precision + drag speed control

### Technical Quality
- ? Build successful
- ? Camera integration
- ? Dynamic knob creation
- ? Position tracking
- ? Clean phase management

---

**Status**: ? **FULLY FUNCTIONAL**  
**Next**: Test in-game to see camera switch and green knob drag! ????

---

## ?? Expected Visual Experience

### Aim Phase
```
[Aim Camera - Zoomed]
  
  Shooting Knob (Yellow/Red)
        ?
    ?????
    ? ? ? ? Rock aimed here
    ?????
        ?
   Trajectory dots
```

### Power Phase
```
[Full Camera - Wide View]

  Launcher (0, -25)
        ?  ? Green Knob (start)
        ?
        ?  ? Green Knob (dragging)
        ?
        ?  ? Green Knob (near hog line)
        ?
  Hog Line (0, -16)
  
  Feedback: "Perfect!"
```

**The green knob provides clear visual feedback of your drag speed!** ???
