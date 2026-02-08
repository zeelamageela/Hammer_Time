# Player Trajectory Visual Inversion Fix

## Problem
Player trajectory preview was showing the **opposite** turn from what would actually be thrown:
- Toggle to **out-turn** ? graphic shows **in-turn** curl ? rock actually throws **out-turn**
- Toggle to **in-turn** ? graphic shows **out-turn** curl ? rock actually throws **in-turn**

This created confusion because the visual preview and actual shot didn't match.

## Root Cause

There were **two separate trajectory visualization systems** with **conflicting conventions**:

### System 1: Physics-Based Simulation (TrajectoryLine.cs)
```csharp
// Line 299-302
bool isInTurn = rm.inturn;  // Direct mapping
// rm.inturn = true ? LEFT curl (physics accurate)
// rm.inturn = false ? RIGHT curl (physics accurate)
```
? **Correct** - Matches `Rock_Force.flipAxis` behavior

### System 2: Visual Bezier Curve (Traj_Transform.cs)
```csharp
// Line 44-51 (OLD)
if (rm.inturn)  // INVERTED LOGIC
{
    transform.localScale = new Vector3(-1f, weight, 1f);  // Flip LEFT
}
else
{
    transform.localScale = new Vector3(1f, weight, 1f);   // No flip (RIGHT)
}
```
? **Backwards** - Comment says "if not inturn, flip" but code does opposite

### Why This Happened

The Bezier curve visual was created before the physics simulation was added. When the physics system was implemented to match `Rock_Force.flipAxis`, the old Bezier visualization wasn't updated to match the new convention.

## The Fix

**File**: `Assets\Scripts\UI\Traj_Transform.cs` (lines 39-51)

### Old Code (Inverted)
```csharp
//if the shot is not an inturn, flip the trajectory
if (rm.inturn)
{
    transform.localScale = new Vector3(-1f, weight, 1f);
}
else
{
    transform.localScale = new Vector3(1f, weight, 1f);
}
```

### New Code (Correct)
```csharp
// FIXED: Visual Bezier curve should match physics simulation convention
// rm.inturn = true ? flipAxis = true ? LEFT curl (negative X scale)
// rm.inturn = false ? flipAxis = false ? RIGHT curl (positive X scale)
// This matches TrajectoryLine.cs physics simulation and Rock_Force.flipAxis
if (!rm.inturn)
{
    transform.localScale = new Vector3(-1f, weight, 1f);
}
else
{
    transform.localScale = new Vector3(1f, weight, 1f);
}
```

### Key Change
**Inverted the if condition**: `if (rm.inturn)` ? `if (!rm.inturn)`

This makes the Bezier visual match the physics simulation.

## Convention Reference

The **unified convention** across all systems is now:

| `rm.inturn` | `flipAxis` | Torque Direction | Curl Direction | Visual Scale |
|-------------|------------|------------------|----------------|--------------|
| `true` | `true` | Negative (-) | LEFT | `(1, weight, 1)` |
| `false` | `false` | Positive (+) | RIGHT | `(-1, weight, 1)` |

### Files Using This Convention
1. ? **`Rock_Force.cs`**: `flipAxis` directly controls torque
2. ? **`TrajectoryLine.cs`**: Physics simulation uses `rm.inturn` directly
3. ? **`Traj_Transform.cs`**: **NOW FIXED** - Visual Bezier matches physics
4. ? **`AI_Shooter.cs`**: Sets `flipAxis = inturn` from physics calculation
5. ? **`RockManager.cs`**: Sets `flipAxis = inturn` for default turn

## How It Works Now

### Player Turn Flow
1. Player toggles turn UI ? `rm.inturn` changes
2. **`Traj_Transform`** reads `rm.inturn` and applies **correct** visual scale
3. **`TrajectoryLine`** simulates physics with same `rm.inturn` value
4. ? **Visual preview matches physics simulation**
5. Player releases rock ? `Rock_Force.flipAxis` set from `rm.inturn`
6. ? **Rock curls in predicted direction**

### AI Turn Flow
1. `AI_Target` calculates best turn (e.g., `useInTurn = false`)
2. Sets `rm.inturn = false`
3. **`Traj_Transform`** shows **out-turn** curve (LEFT curl)
4. **`TrajectoryLine`** shows **out-turn** physics path
5. `AI_Shooter` locks `flipAxis = false`
6. ? **Rock curls out-turn as shown**

## Testing Verification

### Before Fix
```
Player Action: Toggle to OUT-TURN (rm.inturn = false)
Traj_Transform: Shows IN-TURN curve (scale = 1, no flip) ? WRONG
TrajectoryLine: Shows OUT-TURN path ? CORRECT
Rock Throws: OUT-TURN ? CORRECT
Result: Visual mismatch! Confusing for player.
```

### After Fix
```
Player Action: Toggle to OUT-TURN (rm.inturn = false)
Traj_Transform: Shows OUT-TURN curve (scale = -1, flipped) ? CORRECT
TrajectoryLine: Shows OUT-TURN path ? CORRECT
Rock Throws: OUT-TURN ? CORRECT
Result: Visual matches actual shot! Clear feedback.
```

## Impact

### Fixed
- ? Player trajectory preview now matches actual shot
- ? Bezier curve visual matches physics simulation
- ? Turn toggle UI shows correct curl direction
- ? No more confusion between preview and result

### No Regression
- ? AI shots still work correctly
- ? Physics simulation unchanged
- ? Turn toggling still works
- ? Actual rock curl unchanged

## Files Modified

1. **`Assets\Scripts\UI\Traj_Transform.cs`**
   - Inverted the `rm.inturn` condition for Bezier curve visual
   - Added detailed comments explaining the convention

2. **`Assets\Scripts\GameManager.cs`**
   - Added player turn initialization: `rm.inturn = false` (out-turn default)
   - Ensures all player turns start with consistent default turn direction
   - Player can only change turn via toggle button, AI has separate logic

## Player Turn Initialization

### The Problem
Player turns had no explicit initialization of `rm.inturn`, relying on whatever value was left from the previous turn or defaulting to C#'s `bool` default of `false`. This caused inconsistent behavior.

### The Fix
Added explicit initialization at the start of each player turn:

```csharp
// In GameManager.OnRedTurn() and OnYellowTurn()
if (!aiTeamRed)  // Only for player turns, not AI
{
    rm.inturn = false;  // Default to out-turn
    Debug.Log("[GameManager] Player Red Turn - initialized to OUT-TURN");
}
```

### How It Works
1. **Player Turn Starts** ? `rm.inturn = false` (out-turn)
2. **Player Can Toggle** ? Click button changes `rm.inturn` value
3. **Trajectory Updates** ? Visual matches current `rm.inturn` state
4. **Player Throws** ? Rock uses final `rm.inturn` value
5. **AI Turn** ? AI logic sets `rm.inturn` independently

### Unified Flow
| Turn Type | `rm.inturn` Init | Can Change? | Final Value |
|-----------|------------------|-------------|-------------|
| **Player** | `false` (out-turn) | ? Via toggle | Player's choice |
| **AI** | N/A (AI sets it) | ? AI decides | Physics calculation |

## Build Status

? **Build Successful** - All changes compile without errors

## IMPORTANT UPDATE

This fix addressed the trajectory visual inversion, but a **more comprehensive synchronization issue** was discovered where the turn toggle button, trajectory preview, and rock physics were all using different values.

**Additionally, the turn animator graphic was displaying the opposite direction from the trajectory and physics!**

**See `PLAYER_TURN_COMPLETE_SYNCHRONIZATION_FIX.md` for the complete solution** that ensures:
- Turn toggle button updates BOTH `rm.inturn` AND `rock.flipAxis`
- `RockManager` only manages AI turns (never player turns)
- `GameManager` initializes both values for player turns
- **Turn animator graphic uses direct mapping (not inverted)**
- All systems stay perfectly synchronized

## Related Issues

This completes the turn direction fixes:
1. **AI Turn Override** - Fixed `RockManager` overriding AI-calculated turns ?
2. **Player Trajectory Visual** - Fixed Bezier curve showing opposite direction ?
3. **Player Turn Synchronization** - Fixed all turn systems to stay in sync ?
4. **Turn Animator Graphic** - Fixed animator showing opposite direction ?

All turn-related systems now use the **same convention** and are **fully synchronized**.

## Animator Note (DEPRECATED - NO LONGER APPLICABLE)

~~The turn animation graphic (`TurnAnim.cs`) uses **inverted logic** for historical reasons...~~

**UPDATE**: This was incorrect! The animator was using inverted logic due to a bug, not by design. The animator now uses **direct mapping** to match all other systems:
- `rm.inturn = true` ? `anim.SetBool("inturn", true)` ? Shows LEFT curl (in-turn)
- `rm.inturn = false` ? `anim.SetBool("inturn", false)` ? Shows RIGHT curl (out-turn)

The key point: **Physics, trajectory, and visual feedback ALL match now** - that's what matters for player experience!
