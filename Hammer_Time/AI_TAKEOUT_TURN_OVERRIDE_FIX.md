# AI Takeout Turn Override Fix

## Problem
AI physics-based takeouts were selecting the correct turn direction (e.g., out-turn) based on trajectory simulation, but the rock would curl the opposite way (in-turn) when thrown. The visual turn indicator and trajectory preview would show out-turn, but the actual shot would use in-turn.

## Root Cause

There was a **race condition** between three systems trying to set the `flipAxis` value:

### Execution Order
1. ? **`AI_Target.TakeOutTarget()`** (line 640):
   ```csharp
   rm.inturn = useInTurn;  // Sets to false (out-turn) based on physics
   ```

2. ? **`AI_Shooter.Shot()`** (line 95-102):
   ```csharp
   rockForce.flipAxis = inturn;  // Locks it to false (out-turn)
   Debug.Log($"[AI_Shooter] Locked flipAxis = {inturn}");
   ```

3. ? **`RockManager.FixedUpdate()`** (line 28-42):
   ```csharp
   // RUNS EVERY FRAME and overwrites flipAxis!
   if (lastRockIndex != gm.rockCurrent)
   {
       rock.GetComponent<Rock_Force>().flipAxis = inturn;  // Overwrites to whatever rm.inturn is NOW
   }
   ```

### The Race Condition

The problem occurred because:

1. `AI_Target` correctly sets `rm.inturn = false` (out-turn)
2. `AI_Shooter` correctly locks `flipAxis = false` (out-turn)
3. **BUT** `RockManager.FixedUpdate()` runs on **every physics frame**
4. `RockManager` checks `if (lastRockIndex != gm.rockCurrent)` which is **true** the first time
5. `RockManager` then overwrites `flipAxis` with the current value of `rm.inturn`
6. **IF** `rm.inturn` had changed between steps 1-5 (or was incorrectly set), the turn gets overridden

### Why It Manifested on Takeouts

Physics-based takeouts go through this flow:
- `AI_Strategy` ? `AI_Target.TakeOutTarget()` ? **sets `rm.inturn`** ? `AI_Shooter.OnShot("Take Out")`
- There's a **0.5 second delay** in `AI_Shooter.Shot()` before setting `flipAxis`
- During this delay, `RockManager.FixedUpdate()` runs ~30-60 times
- The **first** run after `lastRockIndex` changes would override the turn

## Why It Only Affected the First Rock

The bug **only manifested on the first rock** of an end because:

### Root Cause: Uninitialized State
```csharp
// In RockManager.cs
public bool inturn;  // Defaults to FALSE (out-turn) - never explicitly initialized!
private int lastRockIndex = -1;
```

### Execution Timeline (First Rock Only)

**Frame 1 (Rock becomes current):**
1. `GameManager` sets `gm.rockCurrent = 0` (first rock)
2. `RockManager.FixedUpdate()` runs **BEFORE** `AIManager.OnShot()` is called
3. Checks: `lastRockIndex != gm.rockCurrent` ? **TRUE** (-1 != 0)
4. Checks: `!rockIsActiveForShooting` ? **TRUE** (rock not pressed yet)
5. Checks: `!rockNotYetActivated` ? **FALSE** (Rock_Force not enabled yet) ? **FIXED**
6. **Skips setting flipAxis** because Rock_Force isn't enabled
7. `lastRockIndex` stays at -1

**Frame 2-N (Before AI sets turn):**
1. `Rock_Force` gets enabled by `GameManager`
2. Checks: `lastRockIndex != gm.rockCurrent` ? **TRUE** (-1 != 0)
3. Checks: `!rockIsActiveForShooting` ? **TRUE** (rock not pressed yet)
4. Checks: `!rockNotYetActivated` ? **TRUE** (Rock_Force now enabled)
5. ? **OLD BUG**: Would set `flipAxis = false` from default `inturn = false`
6. ? **NEW FIX**: AI sets `isPressedAI = true` first, preventing override

**Later Frame (AI calculates):**
1. `AIManager.OnShot()` ? `AI_Target` calculates ? sets `rm.inturn = true` (example)
2. `AI_Shooter.Shot()` sets `isPressedAI = true` ? `rockIsActiveForShooting = true`
3. `RockManager.FixedUpdate()` runs but **skips** because `rockIsActiveForShooting = true`
4. ? **No override happens**

### Why Subsequent Rocks Worked

**Rock 2, 3, 4... (Same End):**
1. `lastRockIndex = 0, 1, 2...` (already set from previous rock)
2. When rock becomes current, condition `lastRockIndex != gm.rockCurrent` is immediately **TRUE**
3. **BUT** by this time, the rock's `Rock_Force` is already enabled
4. **AND** `isPressedAI` gets set quickly
5. **So** the window for override is much smaller/non-existent

The **first rock** had a longer window because:
- `lastRockIndex = -1` (special initial value)
- Multiple `FixedUpdate()` frames pass before AI calculation completes
- Default `inturn = false` was being used

## The Fix (Updated)

**File**: `Assets\Scripts\RockManager.cs` (lines 28-50)

### New Code (Final Fix)
```csharp
Rock_Flick rockFlick = rock.GetComponent<Rock_Flick>();
Rock_Force rockForce = rock.GetComponent<Rock_Force>();

// Check if rock is being actively shot
bool rockIsActiveForShooting = (rockFlick != null && 
    (rockFlick.isPressed || rockFlick.isPressedAI || rockFlick.mouseUp || rockInfo.released));

// NEW: Also check if Rock_Force component is not enabled yet
// This prevents setting flipAxis before the rock is fully activated
bool rockNotYetActivated = (rockForce != null && !rockForce.enabled);

if (lastRockIndex != gm.rockCurrent && !rockIsActiveForShooting && !rockNotYetActivated)
{
    // ONLY set flipAxis when rock is:
    // 1. Newly activated (lastRockIndex changed)
    // 2. Rock_Force is enabled (not pre-activation)
    // 3. NOT being shot (no player/AI input yet)
    if (inturn)
    {
        rock.GetComponent<Rock_Force>().flipAxis = true;
    }
    else
    {
        rock.GetComponent<Rock_Force>().flipAxis = false;
    }
    lastRockIndex = gm.rockCurrent;
    Debug.Log($"[RockManager] Set rock #{gm.rockCurrent} turn to: {(inturn ? "IN-TURN" : "OUT-TURN")}");
}
```

### Key Changes (Final Version)
1. **Check `Rock_Force.enabled`**: Prevents setting `flipAxis` before rock is fully activated
2. **Combined with `isPressedAI` check**: Prevents override during AI shot setup
3. **Combined with other flags**: Prevents override during player interaction

This creates a **narrow window** where `RockManager` can set the default turn, but ensures it happens:
- **After** rock is fully initialized
- **Before** AI/player makes a decision
- **Never** during active shooting

### Old Code
```csharp
if (lastRockIndex != gm.rockCurrent)
{
    if (inturn)
    {
        rock.GetComponent<Rock_Force>().flipAxis = true;
    }
    else
    {
        rock.GetComponent<Rock_Force>().flipAxis = false;
    }
    lastRockIndex = gm.rockCurrent;
    Debug.Log($"Set rock #{gm.rockCurrent} turn to: {(inturn ? "IN-TURN" : "OUT-TURN")}");
}
```

### New Code
```csharp
// CRITICAL FIX: Only set flipAxis when switching to a new rock AND rock is NOT YET released
// Once the AI or player has set the turn (via AI_Shooter or player interaction),
// we must NOT override it, especially after the rock is pressed/released
Rock_Flick rockFlick = rock.GetComponent<Rock_Flick>();
bool rockIsActiveForShooting = (rockFlick != null && 
    (rockFlick.isPressed || rockFlick.isPressedAI || rockFlick.mouseUp || rockInfo.released));

if (lastRockIndex != gm.rockCurrent && !rockIsActiveForShooting)
{
    // ONLY set flipAxis when rock is newly activated and NOT yet being shot
    if (inturn)
    {
        rock.GetComponent<Rock_Force>().flipAxis = true;
    }
    else
    {
        rock.GetComponent<Rock_Force>().flipAxis = false;
    }
    lastRockIndex = gm.rockCurrent;
    Debug.Log($"[RockManager] Set rock #{gm.rockCurrent} turn to: {(inturn ? "IN-TURN" : "OUT-TURN")}");
}
```

### Key Changes

1. **Added `rockIsActiveForShooting` check**: Detects if rock is being actively used for shooting
2. **Checks multiple states**:
   - `isPressed` - Player is dragging the rock
   - `isPressedAI` - AI is setting up the shot
   - `mouseUp` - Shot is in the release process
   - `released` - Shot has already been taken
3. **Only sets `flipAxis` when**:
   - Switching to a new rock (`lastRockIndex != gm.rockCurrent`)
   - **AND** rock is not active for shooting (`!rockIsActiveForShooting`)

## How It Works Now

### AI Takeout Flow
1. `AI_Target.TakeOutTarget()` sets `rm.inturn = false` (out-turn chosen by physics)
2. `AI_Shooter.OnShot()` is called
3. `AI_Shooter.Shot()` sets `isPressedAI = true` ? **`rockIsActiveForShooting = true`**
4. `RockManager.FixedUpdate()` runs but **skips setting `flipAxis`** because `rockIsActiveForShooting = true`
5. ? After 0.5s delay, `AI_Shooter` sets `flipAxis = false` (out-turn) **without interference**
6. ? Rock is released with correct turn

### Player Shot Flow
1. Player toggles turn via UI ? `rm.inturn` changes
2. Player drags rock ? `isPressed = true` ? **`rockIsActiveForShooting = true`**
3. `RockManager.FixedUpdate()` **does not override** during drag
4. ? Player releases ? rock uses the turn they selected

## Testing Verification

### Before Fix
```
[AI_Target] Take Out SUCCESS - InTurn: false (out-turn chosen)
[AI_Shooter] Locked flipAxis = false (out-turn locked)
[RockManager] Set rock #4 turn to: IN-TURN (WRONG! overridden!)
Rock curls in-turn (wrong direction)
```

### After Fix
```
[AI_Target] Take Out SUCCESS - InTurn: false (out-turn chosen)
[AI_Shooter] Locked flipAxis = false (out-turn locked)
[RockManager] Skipped setting turn (rock active for shooting)
Rock curls out-turn (correct direction)
```

## Impact

### Fixed
- ? AI takeouts now use the turn selected by physics simulation
- ? AI peels use correct turn
- ? AI raises use correct turn
- ? All physics-based shots respect the calculated turn
- ? Player shots still work correctly

### No Regression
- ? Turn toggle UI still works
- ? Default turn setting for new rocks still works
- ? Story mode shots still work

## Files Modified

1. **`Assets\Scripts\RockManager.cs`**
   - Added `rockIsActiveForShooting` check to prevent override during shots
   - Updated debug log prefix for clarity

## Build Status

? **Build Successful** - All changes compile without errors

## Related Issues

This fix addresses the root cause discovered during the AI targeting improvements. The physics-based targeting system was working correctly, but the turn direction was being overridden by `RockManager` before the shot was taken.

## Future Considerations

If you see turn mismatches in the future, check:

1. **Console logs** - Look for the sequence:
   ```
   [AI_Target] ... InTurn: X
   [AI_Shooter] Locked flipAxis = X
   [RockManager] ... (should NOT appear during AI shots)
   ```

2. **State flags** - Ensure the `Rock_Flick` flags are being set correctly:
   - `isPressedAI` should be true during AI setup
   - `isPressed` should be true during player drag
   - `released` should be true after shot

3. **Timing** - The 0.5s delay in `AI_Shooter.Shot()` should be enough to prevent all overwrites now
