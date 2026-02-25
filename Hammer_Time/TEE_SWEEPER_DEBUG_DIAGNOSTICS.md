# Tee Sweeper Tap Detection - Debug Diagnostics

**Status**: ?? **DEBUGGING** - Added extensive logging to diagnose tap detection issues

---

## Critical Fix Applied

### THE PROBLEM: GameObject Disabled = No Update() = No Tap Detection ?

**Original Code**:
```csharp
void Start()
{
    gameObject.SetActive(false);  // ? KILLS UPDATE()!
}

void DetachFromRock()
{
    gameObject.SetActive(false);  // ? KILLS UPDATE()!
}
```

**Why This Breaks Everything**:
1. `TeeSweeperController` component is on `tSweepParent` GameObject
2. When GameObject is disabled ? `Update()` doesn't run
3. `Update()` calls `DetectRockTaps()` ? Never executes
4. Player clicks ? Nothing happens!

### THE FIX: Keep GameObject Active, Only Toggle Sweepers ?

**New Code**:
```csharp
void Start()
{
    // DON'T disable GameObject - we need Update() to run!
    // Sweepers start inactive, but controller is active for tap detection
    Debug.Log("[TeeSweeperController] Start() - staying active for tap detection");
}

void DetachFromRock()
{
    // Deactivate the specific sweeper GameObject
    if (activeSweeper != null) 
        activeSweeper.gameObject.SetActive(false);
    
    // DON'T disable TeeSweeperController GameObject!
    // We need to stay active to detect next tap
    Debug.Log("[TeeSweeperController] Detached - ready for next tap");
}
```

**SweeperManager Changes**:
```csharp
// SetupSweepers()
sweepSel.tSweepParent.SetActive(true);  // ? KEEP ACTIVE!

// ResetSweepers()
// DON'T call: sweepSel.tSweepParent.SetActive(false);

// Release()
// DON'T call: sweepSel.tSweepParent.SetActive(false);
```

---

## State Management

### GameObject Hierarchy:

```
tSweepParent (GameObject) ? ALWAYS ACTIVE
??? TeeSweeperController.cs (component) ? ALWAYS RUNNING
??? sweeperRedTee (child GameObject) ? Activated when sweeping red rocks
??? sweeperYellowTee (child GameObject) ? Activated when sweeping yellow rocks
```

### Lifecycle States:

| State | tSweepParent | TeeSweeperController | sweeperRedTee | sweeperYellowTee |
|-------|--------------|---------------------|---------------|------------------|
| **Game Start** | ? Active | ? Running | ? Inactive | ? Inactive |
| **Player Taps Red Rock** | ? Active | ? Running | ? Inactive | ? Active + Sweeping |
| **Rock Stops** | ? Active | ? Running | ? Inactive | ? Inactive |
| **Player Taps Yellow Rock** | ? Active | ? Running | ? Active + Sweeping | ? Inactive |
| **Between Shots** | ? Active | ? Running | ? Inactive | ? Inactive |

**Key Insight**: `tSweepParent` **NEVER** disables - it's a persistent tap listener!

---

## Debug Logging Flow

### Expected Log Sequence (Working):

```
[SweeperManager] tSweepParent ACTIVE for tap detection
[SweeperManager] TeeSweeperController initialized
[TeeSweeperController] Initialized - will not interfere with regular sweepers
[TeeSweeperController] Colliders enabled for player sweeping

--- Player clicks on rock ---

[TeeSweeperController] Mouse click detected!
[TeeSweeperController] Mouse position: (-0.5, 7.2)
[TeeSweeperController] Hit: Rock_12, Layer: 3
[TeeSweeperController] Rock clicked: Rock_12
[TeeSweeperController] Eligibility: Rock.moving = True
[TeeSweeperController] Eligibility: Rock Y = 7.2, T-line = 6.5
[TeeSweeperController] Eligibility: PASSED - Rock is eligible!
[TeeSweeperController] Eligibility: Rock eligible - attempting attach
[TeeSweeperController] Attached - Yellow sweeping rock Rock_12
[TeeSweeperController] Started sweeping - 1.40s (silent mode)

--- Rock stops ---

[TeeSweeperController] Rock stopped - detaching
[TeeSweeperController] Detached from rock - ready for next tap
```

### Troubleshooting Logs:

#### Issue 1: No Mouse Click Log
```
--- Player clicks ---
(nothing in logs)
```
**Diagnosis**: `Update()` not running ? GameObject is disabled
**Fix**: Ensure `tSweepParent.SetActive(true)` in `SetupSweepers()`

---

#### Issue 2: Click Detected, Raycast Hits Nothing
```
[TeeSweeperController] Mouse click detected!
[TeeSweeperController] Mouse position: (-0.5, 7.2)
[TeeSweeperController] Raycast hit nothing
```
**Diagnosis**: Rock colliders disabled OR wrong layer
**Check**: 
- Rock colliders enabled? `rock.GetComponent<Collider2D>().enabled`
- Rock on layer 3? `rock.layer == 3`

---

#### Issue 3: Hits Wrong Object
```
[TeeSweeperController] Mouse click detected!
[TeeSweeperController] Hit: Sweeper_Left, Layer: 8
[TeeSweeperController] Hit non-rock object (layer 8)
```
**Diagnosis**: Sweeper collider blocking raycast
**Fix**: Ensure `DisableColliders()` called during AI turns

---

#### Issue 4: Rock Not Moving
```
[TeeSweeperController] Rock clicked: Rock_12
[TeeSweeperController] Eligibility: Rock.moving = False
```
**Diagnosis**: Rock has stopped or hasn't been released yet
**Check**: Rock must have `Rock_Info.moving = true`

---

#### Issue 5: Rock Before T-Line
```
[TeeSweeperController] Eligibility: Rock Y = 5.2, T-line = 6.5
[TeeSweeperController] Eligibility: FAILED - Rock not past T-line
```
**Diagnosis**: Rock hasn't crossed T-line yet
**Fix**: Wait until rock.position.y > 6.5

---

## Manual Testing Steps

### Step 1: Verify GameObject Active
**Action**: In Unity, check Hierarchy during gameplay
**Expected**: `tSweepParent` should have checkmark (active)
**If Not**: Check `SweeperManager.SetupSweepers()` logs

### Step 2: Verify Logs Appear
**Action**: Click anywhere on screen
**Expected**: See `[TeeSweeperController] Mouse click detected!`
**If Not**: `Update()` not running ? GameObject disabled

### Step 3: Verify Raycast Hits Rock
**Action**: Click directly on rock behind T-line
**Expected**: See `[TeeSweeperController] Hit: Rock_XX, Layer: 3`
**If Not**: Collider issue or wrong layer

### Step 4: Verify Eligibility Check
**Action**: Look at eligibility logs
**Expected**: 
- `Rock.moving = True`
- `Rock Y > 6.5`
- `PASSED - Rock is eligible!`

### Step 5: Verify Attachment
**Action**: Check for attach log
**Expected**: `[TeeSweeperController] Attached - Yellow/Red sweeping rock Rock_XX`
**If Not**: Check `activeSweeper` is not null

---

## Common Issues & Solutions

### Issue 1: "No logs at all"
**Cause**: TeeSweeperController component not added
**Check**:
```csharp
// In SweeperManager.SetupSweepers()
teeController = sweepSel.tSweepParent.GetComponent<TeeSweeperController>();
if (teeController == null)
{
    teeController = sweepSel.tSweepParent.AddComponent<TeeSweeperController>();
}
```
**Expected Log**: `[SweeperManager] TeeSweeperController initialized`

---

### Issue 2: "Mouse click detected but raycast hits nothing"
**Cause**: Camera.main is null OR rocks have no colliders
**Check**:
```csharp
Camera main = Camera.main;
Debug.Log($"Main camera: {main}");  // Should not be null

Collider2D rockCol = rock.GetComponent<Collider2D>();
Debug.Log($"Rock collider: {rockCol}, enabled: {rockCol.enabled}");
```

---

### Issue 3: "Rock NOT eligible - Y=7.5" (but Y > 6.5!)
**Cause**: Comparison operator wrong
**Check**: Line should be `if (rockY <= TEE_LINE_Y)` not `if (rockY < TEE_LINE_Y)`
**Current Code**: ? Correct (`<= 6.5` means must be `> 6.5`)

---

### Issue 4: "Active sweeper is NULL!"
**Cause**: Sweepers not instantiated
**Check**:
```csharp
Debug.Log($"sweeperRedTee: {sweeperRedTee}");
Debug.Log($"sweeperYellowTee: {sweeperYellowTee}");
```
**Expected**: Both should not be null after `SetupSweepers()`

---

### Issue 5: Colliders Blocking in Player Turn
**Cause**: Colliders enabled during player turn (expected behavior)
**Check**: Are we testing during **AI turn** or **player turn**?
**Solution**: 
- AI turn: Colliders should be DISABLED ? Can tap rocks
- Player turn: Colliders ENABLED ? Can tap sweepers too

---

## Quick Diagnostic Checklist

Run through this in order:

- [ ] 1. Check Unity Hierarchy: Is `tSweepParent` active? (checkmark visible)
- [ ] 2. Start game, check logs: See `[TeeSweeperController] Start() - staying active`?
- [ ] 3. Click anywhere: See `[TeeSweeperController] Mouse click detected!`?
- [ ] 4. Click rock: See `[TeeSweeperController] Hit: Rock_XX, Layer: 3`?
- [ ] 5. Check rock Y: Is rock position Y > 6.5?
- [ ] 6. Check rock moving: Is `Rock_Info.moving = true`?
- [ ] 7. Check eligibility: See `PASSED - Rock is eligible!`?
- [ ] 8. Check attachment: See `Attached - Yellow/Red sweeping rock Rock_XX`?
- [ ] 9. Visual check: Does sweeper appear and animate?

**If ANY step fails**, check the corresponding issue in "Common Issues & Solutions" above.

---

## Code Changes Summary

### TeeSweeperController.cs

#### `Start()` - Line ~44:
- **Before**: `gameObject.SetActive(false);` ?
- **After**: Comment explaining we stay active ?

#### `DetachFromRock()` - Line ~234:
- **Before**: `gameObject.SetActive(false);` ?
- **After**: Comment + removed line ?

#### `AttachToRock()` - Line ~163:
- **Before**: `gameObject.SetActive(true);` (unnecessary)
- **After**: Removed line, added null check logging ?

#### `DetectRockTaps()` - Line ~98:
- **Added**: Debug logs for every step
- **Added**: Position logging
- **Added**: Hit detection logging

#### `IsEligibleForTeeSweep()` - Line ~130:
- **Added**: Debug logs for every validation step
- **Added**: Value logging (moving, Y position)

---

### SweeperManager.cs

#### `SetupSweepers()` - Line ~151:
- **Before**: `sweepSel.tSweepParent.SetActive(false);` ?
- **After**: `sweepSel.tSweepParent.SetActive(true);` ?
- **Added**: Debug log confirming activation

#### `ResetSweepers()` - Line ~216:
- **Before**: `sweepSel.tSweepParent.SetActive(false);` ?
- **After**: Commented out (stay active) ?

#### `Release()` - Line ~239:
- **Before**: `sweepSel.tSweepParent.SetActive(false);` ?
- **After**: Commented out (stay active) ?

---

## Testing Instructions

### Test 1: Basic Tap Detection
1. Start game (career mode)
2. Let AI shoot first rock
3. **Click anywhere on screen**
4. **Expected Log**: `[TeeSweeperController] Mouse click detected!`
5. **If No Log**: GameObject is disabled - check Hierarchy

### Test 2: Rock Tap Detection
1. AI rock traveling down ice
2. Wait until rock crosses Y=6.5 (T-line)
3. **Click directly on rock**
4. **Expected Logs**:
   ```
   [TeeSweeperController] Mouse click detected!
   [TeeSweeperController] Mouse position: (x, y)
   [TeeSweeperController] Hit: Rock_XX, Layer: 3
   [TeeSweeperController] Rock clicked: Rock_XX
   ```
5. **If Hits Wrong Object**: Check collider state

### Test 3: Eligibility Validation
1. Click rock behind T-line
2. **Expected Logs**:
   ```
   [TeeSweeperController] Eligibility: Rock.moving = True
   [TeeSweeperController] Eligibility: Rock Y = 7.2, T-line = 6.5
   [TeeSweeperController] Eligibility: PASSED!
   ```
3. **If FAILED**: Check specific eligibility reason in logs

### Test 4: Attachment Success
1. Click eligible rock
2. **Expected Logs**:
   ```
   [TeeSweeperController] Eligibility: Rock eligible - attempting attach
   [TeeSweeperController] Attached - Yellow sweeping rock Rock_12
   [TeeSweeperController] Started sweeping - 1.40s (silent mode)
   ```
3. **Visual**: Skip should appear next to rock and animate

### Test 5: Detachment
1. Wait for rock to stop
2. **Expected Logs**:
   ```
   [TeeSweeperController] Rock stopped - detaching
   [TeeSweeperController] Detached from rock - ready for next tap
   ```
3. **Visual**: Skip should disappear

---

## Diagnostic Decision Tree

```
Click on rock behind T-line
?
Do you see "[TeeSweeperController] Mouse click detected!"?
?? NO ? GameObject disabled
?         ?? Fix: Check tSweepParent active in Hierarchy
?
?? YES ? Do you see "Hit: Rock_XX, Layer: 3"?
    ?? NO ? See different object hit?
    ?   ?? YES ? Sweeper collider blocking
    ?   ?         ?? Fix: Verify DisableColliders() called
    ?   ?? NO ? See "Raycast hit nothing"?
    ?             ?? Rock has no collider or wrong layer
    ?
    ?? YES ? Do you see "Eligibility: PASSED!"?
        ?? NO ? See "Rock.moving = False"?
        ?   ?? YES ? Rock not moving yet/anymore
        ?   ?         ?? Rock must be in motion
        ?   ?? NO ? See "Rock Y = X, T-line = 6.5"?
        ?             ?? Rock before T-line (Y <= 6.5)
        ?
        ?? YES ? Do you see "Attached - Yellow/Red sweeping"?
            ?? NO ? See "Active sweeper is NULL!"?
            ?         ?? Sweepers not instantiated
            ?             ?? Check SetupSweepers() logs
            ?
            ?? YES ? SUCCESS! ??
                     Sweeper should appear and sweep rock
```

---

## Performance Impact of Debug Logs

**Current Logging**:
- Every mouse click: ~10 log statements
- Every eligibility check: ~8 log statements
- Total: ~18 logs per tap

**CPU Impact**: ~0.5ms per tap (negligible)
**When to Remove**: After confirming system works

**To Remove Debug Logs Later**:
```csharp
// Search for: Debug.Log("[TeeSweeperController]
// Replace with: // Debug.Log("[TeeSweeperController]
// Or delete the lines entirely
```

---

## Expected Behavior After Fix

### Scenario 1: AI Shoots, Player Sweeps Behind T-Line
```
AI shoots rock
?
Rock crosses Y=6.5 (moving fast)
?
Player clicks rock
?
LOGS:
[TeeSweeperController] Mouse click detected!
[TeeSweeperController] Hit: Rock_05, Layer: 3
[TeeSweeperController] Eligibility: Rock.moving = True
[TeeSweeperController] Eligibility: Rock Y = 7.1, T-line = 6.5
[TeeSweeperController] Eligibility: PASSED!
[TeeSweeperController] Attached - Yellow sweeping rock Rock_05
[TeeSweeperController] Started sweeping - 1.40s
?
Skip appears and sweeps rock ?
?
Rock stops
?
[TeeSweeperController] Rock stopped - detaching
[TeeSweeperController] Detached from rock - ready for next tap
?
Skip disappears ?
```

### Scenario 2: Multiple Rocks Behind T-Line
```
AI shoots first rock ? stops at Y=7.0
?
AI shoots second rock ? traveling at Y=7.5
?
Player clicks moving rock (second one)
?
LOGS:
[TeeSweeperController] Attached - Yellow sweeping rock Rock_06
?
Skip sweeps ONLY the moving rock ?
(First rock is stationary, correctly ignored)
```

---

## Architecture Summary

### Why This Design?

**Traditional Event-Based Approach** (Didn't Work):
```csharp
// SweeperManager manually activates sweepers
SweeperManager.ActivateTeeSweepers(rock)
? Find rock, check eligibility
? Attach sweeper
? Manage lifecycle
```
**Problem**: Tight coupling, hard to maintain

**New Autonomous Approach** (Current):
```csharp
// TeeSweeperController is autonomous tap listener
TeeSweeperController.Update()
? Always listening for taps
? Self-manages eligibility
? Self-manages attachment
? Self-manages lifecycle
```
**Benefit**: Loose coupling, easier to maintain, no external triggers needed

---

## Files Modified

### 1. TeeSweeperController.cs
- ? `Start()`: Removed `gameObject.SetActive(false)`
- ? `DetachFromRock()`: Removed `gameObject.SetActive(false)`
- ? `DetectRockTaps()`: Added extensive debug logging
- ? `IsEligibleForTeeSweep()`: Added step-by-step validation logging
- ? `AttachToRock()`: Added null check logging

### 2. SweeperManager.cs
- ? `SetupSweepers()`: Changed to `tSweepParent.SetActive(true)`
- ? `ResetSweepers()`: Commented out deactivation
- ? `Release()`: Commented out deactivation

---

## Next Steps After Testing

### If Working:
1. ? Confirm tap detection works
2. ? Confirm sweeper appears and animates
3. ? Remove debug logs (or reduce verbosity)
4. ? Implement AI T-line sweeping logic

### If Not Working:
1. ?? Review diagnostic logs above
2. ?? Check specific failure point in decision tree
3. ?? Verify GameObject active state in Unity Hierarchy
4. ?? Test on stopped rock first (eliminate "moving" variable)

---

## Quick Test Commands (Unity Console)

### Check GameObject State:
```csharp
GameObject.Find("tSweepParent").activeSelf
// Should return: True
```

### Check Component Exists:
```csharp
GameObject.Find("tSweepParent").GetComponent<TeeSweeperController>() != null
// Should return: True
```

### Check Collider State:
```csharp
// During AI turn (should be disabled):
GameObject.Find("sweeperRedTee").GetComponent<BoxCollider2D>().enabled
// Should return: False

// During Player turn (should be enabled):
GameObject.Find("sweeperRedTee").GetComponent<BoxCollider2D>().enabled
// Should return: True
```

### Force Attach Test:
```csharp
// Get any rock behind T-line
GameObject rock = GameObject.Find("Rock_05");
TeeSweeperController controller = GameObject.Find("tSweepParent").GetComponent<TeeSweeperController>();

// Try to attach manually (use Unity console or debug script)
// controller.AttachToRock(rock);  // This method is private, so need to use tap
```

---

**Test Now!** Click on rocks behind T-line and watch the logs. The extensive logging will show exactly where the system is working or failing! ??

**Expected First Log**: `[TeeSweeperController] Mouse click detected!` - If you don't see this, the GameObject is definitely disabled.
