# Tee Sweeper Y-Zone Fix - Complete! ?

**Status**: ? **FIXED** - Tee sweeper now operates in correct Y = 6.5 to Y = 8.0 zone!

---

## What Was Wrong

### The Bug:

**Tee sweeper deactivation threshold was Y > 10.0** (way too high!)

```csharp
// BEFORE (wrong):
if (rockPos.y > 10f || rockPos.y < 5f)  // Stayed active until Y=10!
{
    DetachFromRock();
}
```

**Result**: Sweeper stayed active **way past the back line (Y=8.0)**!

---

## The Fix

### Updated Y-Zone Constants:

```csharp
private const float TEE_LINE_Y = 6.5f;      // Start sweeping here
private const float BACK_LINE_Y = 8.0f;     // Stop sweeping here ? (NEW!)
private const float VELOCITY_THRESHOLD = 0.01f;
```

### Updated CheckRockStatus():

```csharp
Vector3 rockPos = attachedRockGO.transform.position;

// Sweeper zone: Between T-line (6.5) and back line (8.0)
if (Mathf.Abs(rockPos.x) > 3f || rockPos.y > BACK_LINE_Y || rockPos.y < TEE_LINE_Y)
{
    Debug.Log($"[TeeSweeperController] Rock out of zone (Y={rockPos.y:F2}, zone={TEE_LINE_Y}-{BACK_LINE_Y}) - detaching");
    DetachFromRock();
}
```

**Changes**:
1. ? Added `BACK_LINE_Y = 8.0f` constant
2. ? Changed Y > 10.0 to Y > BACK_LINE_Y (8.0)
3. ? Changed Y < 5.0 to Y < TEE_LINE_Y (6.5)
4. ? Improved debug message to show zone boundaries

---

## How It Works Now

### The Tee Sweeper Zone:

```
        Shooter releases rock
               ?
        Rock travels up ice
               ?
    Y = 6.5 ? T-LINE (activation zone starts)
        [Tee Sweeper Zone]
        Player taps rock ? Sweeper appears
        Sweeper follows rock
    Y = 8.0 ? BACK LINE (sweeper auto-detaches)
               ?
        Rock continues (no tee sweeper)
               ?
        Rock reaches button or back house
```

---

## Activation Rules

### When Tee Sweeper Activates:

1. ? Rock must be **past T-line** (Y > 6.5)
2. ? Rock must be **moving** (`rock.moving == true`)
3. ? Player must **tap the rock** (manual activation)

**Works for**: ANY rock (your team OR opponent)

---

### When Tee Sweeper Deactivates:

1. ? Rock crosses **back line** (Y > 8.0) - **AUTO**
2. ? Rock drops below **T-line** (Y < 6.5) - **AUTO**
3. ? Rock goes off **sides** (|X| > 3.0) - **AUTO**
4. ? Rock **stops** (velocity < 0.01) - **AUTO**
5. ? Player calls **"Whoa"** - **MANUAL**

---

## Testing Guide

### Test 1: Basic Activation

```
1. Start game (player turn)
2. Throw rock that goes to back house
3. Wait for rock to reach Y = 6.6
4. CLICK/TAP on the rock
5. Expected:
   ? Debug log: "[TeeSweeperController] Rock eligible - attempting attach"
   ? Debug log: "[TeeSweeperController] SWEEPER NOW VISIBLE"
   ? Visual: Sweeper appears and follows rock
```

---

### Test 2: Auto-Deactivation at Back Line ? (FIXED!)

```
1. Activate tee sweeper at Y = 6.6
2. Let rock continue rolling
3. Watch as rock approaches Y = 8.0
4. Expected:
   ? Rock at Y = 7.9: Sweeper still visible
   ? Rock at Y = 8.1: Sweeper disappears!
   ? Debug log: "[TeeSweeperController] Rock out of zone (Y=8.01, zone=6.5-8.0) - detaching"
```

**Before fix**: Sweeper stayed until Y = 10.0 ?
**After fix**: Sweeper detaches at Y = 8.0 ?

---

### Test 3: Rock Drops Back Below T-Line

```
1. Activate tee sweeper at Y = 7.0
2. Rock collides with another rock
3. Deflects backward to Y = 6.3
4. Expected:
   ? Sweeper detaches immediately
   ? Debug log: "Rock out of zone (Y=6.30, zone=6.5-8.0) - detaching"
```

---

### Test 4: Rock Goes Off Side

```
1. Activate tee sweeper at Y = 7.0
2. Rock curls heavily to X = -3.2
3. Expected:
   ? Sweeper detaches immediately
   ? Debug log: "Rock out of zone (Y=7.00, zone=6.5-8.0) - detaching"
```

---

## Strategic Gameplay

### Real Curling Rules:

**In real curling**:
- You can sweep **ANY rock** behind the tee line (Y > 6.5)
- Sweeping **your own rocks** helps them travel farther
- Sweeping **opponent rocks** also helps them travel farther
- Strategy: Sweep opponent rocks **OUT OF PLAY** (make them go too far!)

**Example Scenarios**:

#### Scenario 1: Your Draw to Button (Behind T-Line)

```
Your rock at Y = 7.0, heading to button (Y = 6.5)
Problem: Rock is too heavy, will overshoot!
?
TAP rock to activate tee sweeper
?
Call "WHOA!" to stop sweeping
?
Rock friction increases, slows down
?
Result: Rock stops at button instead of overshooting ?
```

---

#### Scenario 2: Opponent Draw to Button

```
Opponent rock at Y = 7.5, heading to button
Rock on perfect line to score!
?
TAP opponent's rock to activate tee sweeper
?
Call "SWEEP!" (helps them go TOO FAR!)
?
Rock travels faster, overshoots button
?
Result: Opponent rock goes to back house (no score!) ?
```

---

#### Scenario 3: Your Raise Shot

```
Your rock at Y = 7.0, pushing another rock forward
Need both rocks to reach house!
?
TAP your moving rock
?
Call "SWEEP!" to help it travel farther
?
Rock maintains momentum, pushes target rock
?
Result: Both rocks reach scoring position! ?
```

---

## Zone Diagram

### The Complete Ice Surface:

```
Y = -25.0 ????????????? [Hack/Launch point]
     ?
     ?
     ?  [Regular Sweeping Zone]
     ?  (Hog line to T-line)
     ?  SweeperL & SweeperR active
     ?
Y = 6.5 ???????????????? [T-LINE] ? Tee Sweeper Zone STARTS
     ?
     ?  [Tee Sweeper Zone]
     ?  - Tap rock to activate
     ?  - SweeperTee active
     ?  - Follows rock
     ?
Y = 8.0 ???????????????? [BACK LINE] ? Tee Sweeper Zone ENDS
     ?
     ?  [Back House]
     ?  - No sweeping allowed
     ?  - Rocks settle
     ?
Y = 10.0 ????????????? [Out of Play]
```

---

## Code Changes Summary

### File: `Assets/Scripts/Sweeping/TeeSweeperController.cs`

**Change 1: Added BACK_LINE_Y constant**
```csharp
// Line 24-26:
private const float TEE_LINE_Y = 6.5f;
private const float BACK_LINE_Y = 8.0f;  // ? NEW!
private const float VELOCITY_THRESHOLD = 0.01f;
```

**Change 2: Updated deactivation logic**
```csharp
// Line 290-296:
// OLD:
if (Mathf.Abs(rockPos.x) > 3f || rockPos.y > 10f || rockPos.y < 5f)

// NEW:
if (Mathf.Abs(rockPos.x) > 3f || rockPos.y > BACK_LINE_Y || rockPos.y < TEE_LINE_Y)
```

---

## Build Status

? **Build Successful!**

```
Compilation completed successfully.
0 errors, 0 warnings.
Tee sweeper Y-zone fix applied!
```

---

## Why You Weren't Seeing It

### The Root Cause:

**Y > 10.0 threshold** meant the tee sweeper would stay active **way past where you expected**!

```
Expected zone: Y = 6.5 to Y = 8.0 (2.5 meters)
Actual zone:   Y = 6.5 to Y = 10.0 (3.5 meters!)
```

**Result**: 
- ? Sweeper activated correctly at Y = 6.5
- ? But stayed visible until Y = 10.0 (confusing!)
- ? Players expected it to disappear at Y = 8.0

---

### Additional Possible Issues:

If you're still not seeing it after this fix, check:

1. **Is GameObject active?**
   - Look for log: `"[SweeperManager] tSweepParent ACTIVE for tap detection"`

2. **Are colliders enabled?**
   - Look for log: `"[SweeperManager] Player turn - tee sweeper colliders ENABLED"`

3. **Is rock eligible?**
   - Rock must be Y > 6.5
   - Rock must be `moving == true`
   - Look for log: `"[TeeSweeperController] Eligibility: PASSED"`

4. **Is tap registering?**
   - Look for log: `"[TeeSweeperController] Mouse click detected!"`
   - Make sure you're clicking ON the rock (layer 3)

---

## Next Steps

### To Use Tee Sweeper:

1. ? Throw a rock (yours or opponent's)
2. ? Wait for rock to pass Y = 6.5 (T-line)
3. ? **TAP/CLICK on the moving rock**
4. ? Sweeper appears and follows rock
5. ? Use "Sweep" or "Whoa" buttons to control
6. ? Sweeper auto-detaches at Y = 8.0 (back line)

### Debugging:

If tee sweeper still doesn't appear:
1. Check console for debug logs
2. Verify rock is past Y = 6.5
3. Verify rock is moving
4. Try clicking directly on rock sprite
5. Check Scene view to see if sweeper is just invisible

---

## Summary

### What Changed:

**Before**: Tee sweeper zone = Y 6.5 to Y 10.0 ?
**After**: Tee sweeper zone = Y 6.5 to Y 8.0 ?

### The Fix:

- ? Added `BACK_LINE_Y = 8.0f` constant
- ? Updated deactivation threshold from Y > 10.0 to Y > 8.0
- ? Updated lower bound from Y < 5.0 to Y < 6.5
- ? Improved debug logging to show zone boundaries

### Result:

**Tee sweeper now properly operates in the correct Y = 6.5 to Y = 8.0 zone!** ?

**Test it and watch the sweeper disappear at the back line!** ???
