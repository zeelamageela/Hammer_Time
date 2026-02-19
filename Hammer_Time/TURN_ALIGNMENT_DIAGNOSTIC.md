# Turn Alignment Diagnostic - Complete System Analysis

## The Problem
TurnAnim graphic, trajectory preview, and actual rock turn direction are all misaligned and showing different values.

## System Overview

### Three Separate Systems Managing Turn

1. **TurnAnim (UI Graphic)**
   - Reads: `rm.inturn`
   - Sets: `anim.SetBool("inturn", value)`
   - Sets: `rock.flipAxis`
   - **Convention**: Direct mapping (true = left, false = right)

2. **TrajectoryLine (Physics Preview)**
   - Reads: `rm.inturn` OR `rock.flipAxis`
   - Uses: Physics simulation
   - **Convention**: `isInTurn ? -1 : 1` for dirMult

3. **Rock_Force (Actual Physics)**
   - Reads: `rock.flipAxis`
   - Sets: Torque direction
   - **Convention**: `flipAxis ? -1 : 1` for dirMult

## Current Issues Found

### Issue 1: TrajectoryLine Reading Wrong Source
**File**: `Assets\Scripts\UI\TrajectoryLine.cs` (Line ~288)

```csharp
// PROBLEM: Reads rock.flipAxis sometimes, rm.inturn other times
Rock_Force currentRockForce = gm.rockList[gm.rockCurrent].rock.GetComponent<Rock_Force>();
bool currentFlipAxis = currentRockForce != null ? currentRockForce.flipAxis : 
    (rockManager != null ? rockManager.inturn : false);
```

**Why this is a problem:**
- If `Rock_Force` is enabled, reads `flipAxis`
- If `Rock_Force` is disabled, reads `rm.inturn`
- These can have different values!

### Issue 2: TurnAnim Updates Both Values But Out of Sync
**File**: `Assets\Scripts\TurnAnim.cs` (Line 32-45)

```csharp
public void ToggleTurn()
{
    rm.inturn = !rm.inturn;  // Updated immediately
    
    // ... animator update ...
    
    rock = gm.rockList[gm.rockCurrent].rock;
    Rock_Force rockForce = rock.GetComponent<Rock_Force>();
    if (rockForce != null)
    {
        rockForce.flipAxis = rm.inturn;  // Updated after animator
    }
}
```

**Timing Issue:**
1. `rm.inturn` changes
2. Animator reads `rm.inturn` (new value)
3. `rock.flipAxis` updates (new value)
4. But **TrajectoryLine might read between steps 1-3!**

### Issue 3: GameManager Doesn't Initialize flipAxis
**File**: `Assets\Scripts\GameManager.cs` (Lines 281-288, 433-440)

```csharp
if (!aiTeamRed)
{
    rm.inturn = false;  // Initializes rm.inturn ?
    Debug.Log("[GameManager] Player Turn - initialized rm.inturn to OUT-TURN");
}
```

**BUT NO:**
```csharp
rock.GetComponent<Rock_Force>().flipAxis = rm.inturn;  // Missing in player turns!
```

**This means:**
- Player turns: `rm.inturn = false`, but `flipAxis` could be **any value** from previous turn
- Result: Trajectory reads `flipAxis` (old value), rock uses `flipAxis` (old value), graphic shows `rm.inturn` (new value)

## The Root Problem

**Three different sources of truth at different times:**

| System | Reads From | When |
|--------|-----------|------|
| TurnAnim Graphic | `rm.inturn` | Always |
| TrajectoryLine | `rock.flipAxis` OR `rm.inturn` | Depends if Rock_Force enabled |
| Rock_Force | `rock.flipAxis` | Always |

**If these aren't synchronized, they show different turns!**

## The Complete Fix

### Fix 1: GameManager Must Initialize BOTH Values
**File**: `Assets\Scripts\GameManager.cs`

```csharp
// In OnRedTurn() and OnYellowTurn() AFTER enabling Rock_Force
if (!aiTeamRed)
{
    rm.inturn = false;  // Default to out-turn
    redRock_1.GetComponent<Rock_Force>().flipAxis = rm.inturn;  // SYNC flipAxis!
    Debug.Log($"[GameManager] Player Turn - initialized rm.inturn={rm.inturn}, flipAxis={rm.inturn}");
}
```

### Fix 2: TrajectoryLine Must Read Single Source
**File**: `Assets\Scripts\UI\TrajectoryLine.cs`

**Option A: Always read `rm.inturn` (Recommended)**
```csharp
// ALWAYS use rm.inturn as single source of truth
bool isInTurn = rm != null ? rm.inturn : false;
```

**Option B: Always read `rock.flipAxis`**
```csharp
// ALWAYS use rock.flipAxis as single source of truth
Rock_Force rockForce = gm.rockList[gm.rockCurrent].rock.GetComponent<Rock_Force>();
bool isInTurn = rockForce != null ? rockForce.flipAxis : false;
```

**I recommend Option A** because `rm.inturn` is set FIRST (before Rock_Force is enabled).

### Fix 3: TurnAnim Must Update Atomically
**File**: `Assets\Scripts\TurnAnim.cs`

```csharp
public void ToggleTurn()
{
    // ATOMIC UPDATE: Set both values at once, before any reads can happen
    rm.inturn = !rm.inturn;
    
    rock = gm.rockList[gm.rockCurrent].rock;
    Rock_Force rockForce = rock.GetComponent<Rock_Force>();
    if (rockForce != null)
    {
        rockForce.flipAxis = rm.inturn;  // Must happen IMMEDIATELY after rm.inturn
    }
    
    // Update animator AFTER both are synced
    if (rm.inturn)
    {
        anim.SetBool("inturn", true);
    }
    else
    {
        anim.SetBool("inturn", false);
    }
    
    // CRITICAL: Force trajectory redraw AFTER all values synced
    TrajectoryLine trajLine = FindObjectOfType<TrajectoryLine>();
    if (trajLine != null)
    {
        trajLine.DrawTrajectory();  // Reads synchronized rm.inturn
    }
    
    StartCoroutine(ToggleColliderDelay());
}
```

### Fix 4: RockManager Must Not Override Player Turns
**File**: `Assets\Scripts\RockManager.cs`

This is already fixed, but verify:
```csharp
bool isAITurn = (gm.rockCurrent % 2 == 0) 
    ? (gm.redHammer ? gm.aiTeamYellow : gm.aiTeamRed) 
    : (gm.redHammer ? gm.aiTeamRed : gm.aiTeamYellow);

// ONLY set flipAxis for AI turns
if (isAITurn && lastRockIndex != gm.rockCurrent && !rockIsActiveForShooting && !rockNotYetActivated)
{
    rock.GetComponent<Rock_Force>().flipAxis = inturn;
}
```

## Testing Verification

### Test 1: Player Turn Start
**Expected:**
```
[GameManager] Player Red Turn - initialized rm.inturn=false, flipAxis=false
[TurnAnim] SetTurn(false) - animator=false
[TrajectoryLine] Drawing with isInTurn=false (OUT-TURN)
```

### Test 2: Player Toggle Button
**Expected:**
```
[TurnAnim] Toggle - rm.inturn=true, flipAxis=true
[TrajectoryLine] Redrawing with isInTurn=true (IN-TURN)
[TurnAnim] Animator updated to true
```

### Test 3: Player Release Rock
**Expected:**
```
[Rock_Force.Release] flipAxis=true, applying LEFT curl
Actual rock: Curls LEFT ?
```

## Single Source of Truth Recommendation

**Use `rm.inturn` as the ONLY source of truth:**

| System | Responsibility | Action |
|--------|---------------|--------|
| **GameManager** | Initialize turn | Set `rm.inturn` AND `flipAxis` |
| **TurnAnim** | Toggle turn | Update `rm.inturn` AND `flipAxis` atomically |
| **TrajectoryLine** | Preview physics | Read `rm.inturn` only |
| **Rock_Force** | Apply physics | Read `flipAxis` (synced from `rm.inturn`) |
| **RockManager** | AI default | Set both values (AI only) |

**Key Rule**: `flipAxis` is ALWAYS synchronized from `rm.inturn` immediately after `rm.inturn` changes.

## Implementation Priority

1. **CRITICAL**: Fix GameManager to initialize `flipAxis` in player turns
2. **CRITICAL**: Fix TrajectoryLine to read `rm.inturn` consistently
3. **HIGH**: Verify TurnAnim updates atomically
4. **VERIFY**: Confirm RockManager doesn't override player turns

Let me know if you want me to implement these fixes!
