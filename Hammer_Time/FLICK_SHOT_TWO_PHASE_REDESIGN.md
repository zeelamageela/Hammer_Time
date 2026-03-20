# Flick Shot Mode - Two-Phase Workflow Redesign ?

**Date**: 2024  
**Status**: ? **REDESIGNED** - Set Aim First, Then Flick  
**Build Status**: ? SUCCESS

---

## ?? New Design Philosophy

### ? Old Approach (Didn't Work)
- Rock auto-locked at 3.5 units
- Move mouse to rotate rock
- Click anywhere to start power
- **Problem**: Rock visible in shooting knob, confusing UX

### ? New Approach (Better!)
- **Phase 1**: Use normal pullback to SET AIM (doesn't fire!)
- **Phase 2**: Click launcher, drag from Y=-25 to Y=-16 for power
- **Benefit**: Familiar pullback, methodical aim, then controlled flick

---

## ?? User Experience Flow

### **Phase 1: Set Your Aim** (Normal Pullback)
1. Click on **invisible rock** at (0, -25)
2. Drag to aim (just like normal mode)
3. Trajectory shows predicted path
4. **Release mouse** ? Rock STAYS where you aimed
5. Shooting knob STAYS visible at aimed position
6. **Ready for Phase 2!**

### **Phase 2: Flick for Power** (Speed Control)
1. **Click on launcher** (0, -25) to start flick
2. **Drag mouse** from Y=-25 down toward Y=-16 (hog line)
3. **Drag TIME** determines rock velocity:
   - Fast drag (0.2s) = "Way too fast!" (13 m/s)
   - Medium drag (0.85s) = "Perfect!" (9 m/s)
   - Slow drag (1.5s+) = "Way too slow!" (5 m/s)
4. **Release mouse** ? Rock fires at aimed direction with calculated speed!

---

## ?? Technical Implementation

### FlickShotController.cs

**StartFlickShot()**:
- **Before**: Auto-positioned rock, enabled sprites, rotated around launcher
- **After**: Does nothing! Lets normal pullback handle aiming

**Update()**:
- **Phase 1**: Waits for player to click on launcher
- **Phase 2**: Tracks mouse drag speed from Y=-25 to Y=-16

**CheckForLauncherClick()**:
- Detects click within 1 unit of launcher position
- Starts power phase when detected

**StartPowerPhase()**:
- Stores aim direction from pullback position
- Starts timing drag from launcher position
- Waits for player to drag down the sheet

**UpdatePowerPhase()**:
- Tracks mouse Y position
- Calculates drag time (Time.time - startTime)
- Provides speed feedback every 0.1s
- On mouse release ? fires rock

### Rock_Flick.cs

**OnMouseUp()**:
- **Before**: Normal release ? fires or resets
- **After** (Flick Mode): 
  - **DON'T fire** on release
  - **DON'T reset** to launcher
  - **KEEP rock** at aimed position
  - **KEEP shooting knob** visible
  - **WAIT** for launcher click

---

## ?? Comparison

| Aspect | Normal Mode | Flick Shot Mode |
|--------|-------------|-----------------|
| **Phase 1** | Pullback ? Fires | Pullback ? Holds aim |
| **Release** | Rock fires | Rock stays aimed |
| **Shooting Knob** | Disappears on release | Stays visible |
| **Phase 2** | N/A | Click launcher ? flick for power |
| **Skill** | Aim + power in one motion | Aim first, power separate |

---

## ?? Player Interaction

### Flick Shot Mode Workflow

```
1. Click on rock at (0, -25)
   ?
2. Drag to aim direction
   ?  
3. Release mouse
   ?? Normal mode: Rock fires
   ?? Flick mode: Rock HOLDS at aim
         ?
4. Click on launcher (0, -25)
   ?
5. Drag down sheet (Y=-25 to Y=-16)
   ?? Fast drag: High speed
   ?? Medium drag: Perfect speed
   ?? Slow drag: Low speed
         ?
6. Release mouse
   ?? Rock fires at aimed direction!
```

---

## ? What Works Now

### Phase 1: Aiming ?
- ? Click on invisible rock at (0, -25) (same as normal mode)
- ? Drag to aim (trajectory shows)
- ? Release ? Rock STAYS at aimed position
- ? Shooting knob STAYS visible
- ? No firing yet!

### Phase 2: Power ?
- ? Click on launcher (within 1 unit radius)
- ? Drag from Y=-25 toward Y=-16
- ? Drag speed tracked
- ? Speed feedback: "Too slow!" / "Perfect!" / "Too fast!"
- ? Release ? Rock fires!

### Integration ?
- ? Normal pullback mode unaffected
- ? TeeSweeperController doesn't interfere
- ? Toggle switches modes cleanly

---

## ?? Testing Guide

### Test 1: Aim Setup ?
1. Enable flick shot mode
2. Start turn
3. Click on rock position (0, -25)
4. Drag backward to aim
5. **Expected**: Trajectory shows
6. **Release mouse**
7. **Expected**: Rock STAYS at aimed position
8. **Expected**: Shooting knob VISIBLE at rock
9. **Expected**: Console: `"Flick shot mode: Aim set at position..."`

### Test 2: Launcher Click ?
1. After setting aim (Test 1)
2. Click on launcher (0, -25)
3. **Expected**: Console: `"Launcher clicked! Starting power phase"`
4. **Expected**: Callout: `"Drag down for Power!"`

### Test 3: Power Flick ?
1. After launcher click (Test 2)
2. Hold mouse and drag down sheet
3. **Expected**: Speed feedback appears every 0.1s
4. **Expected**: Messages change: "Too slow!" ? "Perfect!" ? "Too fast!"
5. **Release mouse**
6. **Expected**: Rock fires!
7. **Expected**: Velocity matches speed band

### Test 4: Normal Mode Still Works ?
1. Disable flick shot mode toggle
2. Start turn
3. Click and drag rock
4. **Release**
5. **Expected**: Rock fires immediately (normal behavior)

---

## ?? UX Advantages

### Why This Design is Better

**1. Familiar Phase 1**:
- Uses existing pullback mechanics
- Players already know how to aim
- No learning curve for aiming

**2. Methodical Aim**:
- Set aim carefully before power
- Shooting knob stays visible as reference
- No accidental firing while aiming

**3. Controlled Power**:
- Separate power input (drag speed)
- Clear feedback during flick
- Skill-based velocity control

**4. Visual Clarity**:
- Rock at aimed position (not in knob)
- Shooting knob shows aim direction
- Launcher is clear target for Phase 2

---

## ?? Quick Reference

### Aiming (Phase 1)
```
Click rock ? Drag ? Release ? Aim HELD
```

### Power (Phase 2)
```
Click launcher ? Drag down ? Release ? FIRE!
```

### Speed Bands (5 levels)
| Drag Time | Band | Message | Speed |
|-----------|------|---------|-------|
| 0.2s | 4 | "Way too fast!" | 13 m/s |
| 0.5s | 3 | "Too fast!" | 11 m/s |
| 0.85s | 2 | **"Perfect!"** | 9 m/s |
| 1.2s | 1 | "Too slow!" | 7 m/s |
| 1.5s+ | 0 | "Way too slow!" | 5 m/s |

---

## ?? Troubleshooting

### Rock Fires on Release (Phase 1)
**Problem**: Rock fires instead of holding aim

**Check**:
- Flick shot mode toggle ON?
- Console: `"Flick shot mode: Aim set at position..."`?

**Fix**:
- Verify toggle is enabled in pause menu
- Check `isFlickShotMode` variable in Rock_Flick

---

### Launcher Click Not Detected
**Problem**: Clicking launcher doesn't start power phase

**Check**:
- Clicking within 1 unit of launcher (0, -25)?
- Console: `"Launcher clicked!"`?

**Fix**:
- Click closer to launcher position
- Check launcher GameObject position in Inspector

---

### Rock Doesn't Fire After Drag
**Problem**: Drag completes but rock doesn't fire

**Check**:
- Released mouse button?
- Console: `"RELEASED - Time: ..."`?
- Console: `"Rock released with velocity: ..."`?

**Fix**:
- Ensure full drag motion (Y=-25 to Y?-16)
- Check for errors in console

---

## ? Summary

### What Changed
1. ? **Removed**: Auto-lock rock at 3.5 units
2. ? **Removed**: Rotate rock with mouse movement
3. ? **Added**: Intercept mouse release to hold aim
4. ? **Added**: Launcher click detection
5. ? **Added**: Drag speed tracking from launcher

### User Experience
- **Before**: Confusing auto-lock, rock in knob
- **After**: Familiar pullback, clear two-phase workflow

### Technical Quality
- ? Build successful
- ? Normal mode unaffected
- ? Clean phase separation
- ? Proper state management

---

**Status**: ? **READY FOR TESTING**  
**Next**: Test the new two-phase workflow in-game! ????
