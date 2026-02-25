# Tee Sweeper Activation - Diagnostic & Fix ??

**Issue**: Tee sweeper not appearing when expected

**Expected Behavior**: 
- ? Activate when rock crosses **Y = 6.5** (T-line)
- ? Deactivate when rock crosses **Y = 8.0** (back line)

**Current Behavior**:
- ? Activates when player **taps rock** AND rock Y > 6.5
- ?? Deactivates when Y < 5.0 or Y > 10.0 (wrong!)

---

## Root Cause Analysis

### 1. Activation Trigger

**Current Code** (`TeeSweeperController.cs`):
```csharp
bool IsEligibleForTeeSweep(GameObject rock)
{
    // Rock must be past T-line (Y > 6.5)
    if (rockPos.y <= TEE_LINE_Y)  // TEE_LINE_Y = 6.5
        return false;
    
    // Rock must be moving
    if (!isMoving)
        return false;
    
    return true; // ? Eligible
}
```

**How It Works**:
- Player must **TAP the rock** (mouse click detection in `DetectRockTaps()`)
- Rock must be **moving** (`rock.moving == true`)
- Rock must be **past Y = 6.5** (T-line)

**Problem**: Requires MANUAL tap - not automatic!

---

### 2. Deactivation Trigger

**Current Code** (`TeeSweeperController.cs`):
```csharp
void CheckRockStatus()
{
    // Rock out of bounds - detaching
    if (Mathf.Abs(rockPos.x) > 3f || rockPos.y > 10f || rockPos.y < 5f)
    {
        DetachFromRock();
    }
}
```

**Deactivation Conditions**:
- Rock Y > 10.0 (way past back line) ? Should be 8.0!
- Rock Y < 5.0 (back toward house) ? Correct
- Rock X > 3.0 or X < -3.0 (off to side) ? Correct

**Problem**: Y > 10.0 is too high! Should be Y > 8.0 (back line)

---

## The Fixes Needed

### Fix 1: Update Back Line Threshold

**Change**:
```csharp
// OLD:
if (Mathf.Abs(rockPos.x) > 3f || rockPos.y > 10f || rockPos.y < 5f)

// NEW:
const float BACK_LINE_Y = 8.0f;
if (Mathf.Abs(rockPos.x) > 3f || rockPos.y > BACK_LINE_Y || rockPos.y < 5f)
```

**Result**: Tee sweeper will deactivate at Y = 8.0 (back line) ?

---

### Fix 2: Add Automatic Activation (Optional)

**Current**: Player must tap rock to activate
**Alternative**: Auto-activate when rock crosses Y = 6.5

**Pros of Manual Tap**:
- ? Player control (choose which rock to sweep)
- ? Strategic decision (sweep opponent rock or not?)
- ? Realistic (you decide when to sweep)

**Pros of Automatic**:
- ? No missed opportunities
- ? Easier for new players
- ? Less micromanagement

**Recommendation**: **Keep manual tap**, but add visual indicator when eligible!

---

## Implementation

### Option A: Fix Back Line Only (Recommended)

Just update the Y > 10.0 threshold to Y > 8.0:

```csharp
const float BACK_LINE_Y = 8.0f;  // Back line Y position

void CheckRockStatus()
{
    // ...existing code...
    
    Vector3 rockPos = attachedRockGO.transform.position;
    if (Mathf.Abs(rockPos.x) > 3f || rockPos.y > BACK_LINE_Y || rockPos.y < 5f)
    {
        Debug.Log($"[TeeSweeperController] Rock out of bounds (Y={rockPos.y:F2}, back line={BACK_LINE_Y}) - detaching");
        DetachFromRock();
    }
}
```

**Result**: Sweeper will now properly deactivate at back line (Y=8.0) ?

---

### Option B: Add Visual Indicator for Eligible Rocks

Add a glowing highlight when rocks are eligible for tee sweeping:

```csharp
void Update()
{
    // ...existing code...
    
    HighlightEligibleRocks();  // NEW!
}

void HighlightEligibleRocks()
{
    GameManager gm = FindFirstObjectByType<GameManager>();
    if (gm == null || gm.rockList == null) return;
    
    foreach (var rockEntry in gm.rockList)
    {
        if (rockEntry.rock != null && IsEligibleForTeeSweep(rockEntry.rock))
        {
            // Add glow effect or outline
            SpriteRenderer sr = rockEntry.rock.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.material.SetFloat("_Glow", 0.5f); // Example
            }
        }
    }
}
```

**Result**: Players see which rocks can be tee-swept! ?

---

### Option C: Auto-Activate + Manual Control

Automatically attach when rock crosses Y=6.5, but let player control sweep/whoa:

```csharp
void Update()
{
    // ...existing code...
    
    AutoAttachEligibleRocks();  // NEW!
}

void AutoAttachEligibleRocks()
{
    if (isActive) return; // Already attached to a rock
    
    GameManager gm = FindFirstObjectByType<GameManager>();
    if (gm == null || gm.rockCurrent < 0) return;
    
    GameObject currentRock = gm.rockList[gm.rockCurrent].rock;
    
    if (IsEligibleForTeeSweep(currentRock))
    {
        Debug.Log("[TeeSweeperController] Auto-attaching to eligible rock!");
        AttachToRock(currentRock);
    }
}
```

**Result**: Sweeper appears automatically, player decides whether to sweep! ?

---

## Debugging Steps

### Step 1: Check If TeeSweeperController Is Active

**Add Debug Log**:
```csharp
void Start()
{
    Debug.Log("[TeeSweeperController] Component ACTIVE and ready for taps");
}

void Update()
{
    // At the very start of Update()
    Debug.Log($"[TeeSweeperController] Update() running - isActive={isActive}, frame={Time.frameCount}");
    
    // ...rest of code...
}
```

**What to look for**:
- Is Start() being called? ?
- Is Update() running every frame? ?
- If not, GameObject might be disabled!

---

### Step 2: Check Rock Eligibility

**Test a specific rock**:
```csharp
void Update()
{
    // Test current rock every frame
    GameManager gm = FindFirstObjectByType<GameManager>();
    if (gm != null && gm.rockCurrent >= 0)
    {
        GameObject testRock = gm.rockList[gm.rockCurrent].rock;
        bool eligible = IsEligibleForTeeSweep(testRock);
        
        if (testRock.transform.position.y > 6.5f)
        {
            Debug.Log($"[TeeSweeperController] Current rock Y={testRock.transform.position.y:F2}, eligible={eligible}");
        }
    }
}
```

**What to look for**:
- Is rock past Y = 6.5? ?
- Is rock.moving == true? ?
- If not, rock might have stopped!

---

### Step 3: Check Mouse Click Detection

**Add more debug logs**:
```csharp
void DetectRockTaps()
{
    if (Input.GetMouseButtonDown(0))
    {
        Debug.Log("============ MOUSE CLICK ============");
        Debug.Log($"[TeeSweeperController] Mouse click detected at frame {Time.frameCount}");
        
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Debug.Log($"[TeeSweeperController] Screen pos: {Input.mousePosition}, World pos: {mousePos}");
        
        // ...rest of code...
    }
}
```

**What to look for**:
- Is click being detected? ?
- Is raycast hitting the rock? ?
- Is rock on layer 3? ?

---

### Step 4: Check Sweeper Visibility

**In AttachToRock()**:
```csharp
void AttachToRock(GameObject rock)
{
    // ...existing code...
    
    activeSweeper.gameObject.SetActive(true);
    
    Debug.Log($"============ SWEEPER ACTIVATED ============");
    Debug.Log($"[TeeSweeperController] Active sweeper: {activeSweeper.name}");
    Debug.Log($"[TeeSweeperController] Sweeper.activeSelf: {activeSweeper.gameObject.activeSelf}");
    Debug.Log($"[TeeSweeperController] Sweeper.activeInHierarchy: {activeSweeper.gameObject.activeInHierarchy}");
    Debug.Log($"[TeeSweeperController] Sweeper position: {activeSweeper.transform.position}");
    Debug.Log($"============================================");
    
    // ...rest of code...
}
```

**What to look for**:
- Is sweeper GameObject actually active? ?
- Is sweeper visible in Scene view? ?
- Is sweeper at correct position? ?

---

## Quick Test Scenario

### Manual Test:

1. **Start game** (player vs AI)
2. **Throw a rock** that goes past Y = 6.5
3. **Wait** for rock to cross Y = 6.5
4. **Click on the rock** (tap detection)
5. **Look for debug logs**:
   ```
   [TeeSweeperController] Mouse click detected!
   [TeeSweeperController] Rock clicked: Rock_05
   [TeeSweeperController] Rock eligible - attempting attach
   [TeeSweeperController] SWEEPER NOW VISIBLE: sweeperYellowTee
   ```
6. **Check Scene view** - Is sweeper visible?

---

### Expected vs Actual:

| Event | Expected | Current Code | Issue? |
|-------|----------|--------------|--------|
| Rock at Y=6.4 | Not eligible | Not eligible | ? Correct |
| Rock at Y=6.6 | Tap to activate | Tap to activate | ? Correct |
| Rock at Y=7.0 | Sweeper follows | Sweeper follows | ? Correct |
| Rock at Y=8.0 | Sweeper detaches | Still attached! | ? **BUG!** |
| Rock at Y=10.0 | Sweeper gone | Sweeper detaches | ?? Too late! |

---

## Recommended Fix

### Update Constants in TeeSweeperController.cs:

```csharp
private const float TEE_LINE_Y = 6.5f;      // Start sweeping here ?
private const float BACK_LINE_Y = 8.0f;     // Stop sweeping here ? (NEW!)
private const float VELOCITY_THRESHOLD = 0.01f;
```

### Update CheckRockStatus():

```csharp
void CheckRockStatus()
{
    if (attachedRockRB == null || attachedRockGO == null)
    {
        DetachFromRock();
        return;
    }
    
    float velocity = attachedRockRB.linearVelocity.magnitude;
    if (velocity < VELOCITY_THRESHOLD)
    {
        Debug.Log($"[TeeSweeperController] Rock stopped - detaching");
        DetachFromRock();
        return;
    }
    
    Component rockInfo = attachedRockGO.GetComponent("Rock_Info");
    if (rockInfo != null)
    {
        FieldInfo movingField = rockInfo.GetType().GetField("moving");
        if (movingField != null && !(bool)movingField.GetValue(rockInfo))
        {
            Debug.Log("[TeeSweeperController] Rock no longer in play - detaching");
            DetachFromRock();
            return;
        }
    }
    
    Vector3 rockPos = attachedRockGO.transform.position;
    
    // FIXED: Use BACK_LINE_Y (8.0) instead of 10.0
    if (Mathf.Abs(rockPos.x) > 3f || rockPos.y > BACK_LINE_Y || rockPos.y < TEE_LINE_Y)
    {
        Debug.Log($"[TeeSweeperController] Rock out of zone (Y={rockPos.y:F2}, T-line={TEE_LINE_Y}, Back-line={BACK_LINE_Y}) - detaching");
        DetachFromRock();
    }
}
```

**Changes**:
1. ? Added `BACK_LINE_Y = 8.0f` constant
2. ? Changed Y > 10.0 to Y > BACK_LINE_Y (8.0)
3. ? Changed Y < 5.0 to Y < TEE_LINE_Y (6.5) - sweeper zone is 6.5-8.0 only!

---

## Why It's Not Appearing - Checklist

### Possible Issues:

1. **TeeSweeperController not initialized?**
   - Check: Is `teeController.Initialize()` called in `SweeperManager.SetupSweepers()`?
   - Look for log: `"[TeeSweeperController] Initialized"`

2. **GameObject disabled?**
   - Check: Is `sweepSel.tSweepParent` active?
   - Look for log: `"[SweeperManager] tSweepParent ACTIVE for tap detection"`

3. **Colliders disabled?**
   - Check: During AI turn, are colliders disabled?
   - Look for log: `"[SweeperManager] Player turn - tee sweeper colliders ENABLED"`

4. **Rock not eligible?**
   - Check: Is rock past Y = 6.5?
   - Check: Is rock.moving == true?
   - Look for log: `"[TeeSweeperController] Eligibility: PASSED"`

5. **Tap not registering?**
   - Check: Is `Input.GetMouseButtonDown(0)` firing?
   - Look for log: `"[TeeSweeperController] Mouse click detected!"`
   - Check: Is raycast hitting rock (layer 3)?

---

## Testing Guide

### Test 1: Basic Activation

```
1. Start game (Player vs AI)
2. Throw rock toward back of house
3. Wait for rock to reach Y = 6.6
4. CLICK on the rock
5. Expected:
   - Debug log: "[TeeSweeperController] Mouse click detected!"
   - Debug log: "[TeeSweeperController] Rock eligible - attempting attach"
   - Debug log: "[TeeSweeperController] SWEEPER NOW VISIBLE: sweeperYellowTee"
   - Visual: Sweeper appears and follows rock
```

---

### Test 2: Deactivation at Back Line

```
1. Attach sweeper to rock at Y = 6.6
2. Let rock continue rolling
3. Watch as rock reaches Y = 8.0
4. Expected:
   - Debug log: "[TeeSweeperController] Rock out of zone (Y=8.02, Back-line=8.0) - detaching"
   - Debug log: "[TeeSweeperController] SWEEPER NOW HIDDEN"
   - Visual: Sweeper disappears
```

---

### Test 3: Deactivation When Rock Stops

```
1. Attach sweeper to rock at Y = 7.0
2. Sweep the rock (or let it slow naturally)
3. Rock stops at Y = 7.5
4. Expected:
   - Debug log: "[TeeSweeperController] Rock stopped - detaching"
   - Visual: Sweeper disappears
```

---

## Common Issues & Solutions

### Issue 1: "Sweeper never appears"

**Symptoms**:
- Click on rock
- No debug logs
- No sweeper visible

**Causes**:
1. TeeSweeperController not initialized
   - **Fix**: Check SweeperManager.SetupSweepers() is calling Initialize()
2. GameObject disabled
   - **Fix**: Ensure `sweepSel.tSweepParent.SetActive(true)` in SetupSweepers()
3. Colliders disabled during player turn
   - **Fix**: Check `teeController.EnableColliders()` is called for player turns

**Debug**:
```csharp
// Add to Start()
Debug.Log($"[TeeSweeperController] Component active: {gameObject.activeSelf}, hierarchy: {gameObject.activeInHierarchy}");
```

---

### Issue 2: "Sweeper appears but doesn't follow rock"

**Symptoms**:
- Sweeper appears once
- Doesn't move with rock
- Stays at attachment point

**Causes**:
1. Update() not running
   - **Fix**: Check GameObject active
2. attachedRockRB is null
   - **Fix**: Check AttachToRock() sets attachedRockRB correctly

**Debug**:
```csharp
void UpdatePosition()
{
    if (attachedRockRB == null)
    {
        Debug.LogError("[TeeSweeperController] attachedRockRB is NULL!");
        return;
    }
    
    Debug.Log($"[TeeSweeperController] Following rock: {attachedRockRB.position}");
    transform.position = new Vector3(attachedRockRB.position.x, attachedRockRB.position.y, 0f);
}
```

---

### Issue 3: "Sweeper doesn't disappear at Y=8.0"

**Symptoms**:
- Sweeper appears at Y=6.6
- Follows rock to Y=8.5
- Still visible!

**Causes**:
1. Y > 10.0 threshold (current bug!)
   - **Fix**: Change to Y > 8.0 (see Fix #1 above)

**Debug**:
```csharp
void CheckRockStatus()
{
    Vector3 rockPos = attachedRockGO.transform.position;
    Debug.Log($"[TeeSweeperController] Rock Y={rockPos.y:F2}, checking bounds (back line=8.0)");
    
    if (rockPos.y > BACK_LINE_Y)
    {
        Debug.Log($"[TeeSweeperController] Rock past back line ({rockPos.y:F2} > {BACK_LINE_Y}) - DETACHING!");
        DetachFromRock();
    }
}
```

---

### Issue 4: "Can't click on rocks"

**Symptoms**:
- Click on rock
- Nothing happens
- No debug logs

**Causes**:
1. Colliders disabled
   - **Fix**: Check EnableColliders() is called
2. Wrong layer (not layer 3)
   - **Fix**: Check rock GameObject is on "Rock" layer (3)
3. Camera.main is null
   - **Fix**: Ensure Main Camera has "MainCamera" tag

**Debug**:
```csharp
void DetectRockTaps()
{
    Debug.Log($"[TeeSweeperController] Update() tick - checking for clicks");
    
    if (Input.GetMouseButtonDown(0))
    {
        Debug.Log("============ CLICK DETECTED ============");
        
        // Check camera
        if (Camera.main == null)
        {
            Debug.LogError("[TeeSweeperController] Camera.main is NULL!");
            return;
        }
        
        // ...rest of code...
    }
}
```

---

## Summary of Findings

### Current Behavior:

| Event | Expected Y | Actual Y | Status |
|-------|-----------|----------|--------|
| **Activation zone start** | 6.5 | 6.5 | ? Correct |
| **Activation zone end** | 8.0 | 10.0 | ? **Wrong!** |
| **Activation method** | Tap rock | Tap rock | ? Correct |
| **Deactivation method** | Auto at Y=8.0 | Auto at Y=10.0 | ? **Wrong!** |

### The Fix:

**Change ONE line** in `TeeSweeperController.cs`:

```csharp
// Line 16: Add constant
private const float BACK_LINE_Y = 8.0f;

// Line 193: Update condition
if (Mathf.Abs(rockPos.x) > 3f || rockPos.y > BACK_LINE_Y || rockPos.y < TEE_LINE_Y)
```

**Result**: Tee sweeper will now properly operate in the **Y = 6.5 to Y = 8.0 zone**! ?

---

## Next Steps

1. ? Apply Fix #1 (update back line threshold)
2. ? Test with thrown rock past Y=6.5
3. ? Click on rock and verify sweeper appears
4. ? Verify sweeper disappears at Y=8.0

**Should I implement these fixes now?** ??
