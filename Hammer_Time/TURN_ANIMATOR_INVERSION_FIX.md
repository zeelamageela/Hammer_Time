# Turn Animator Inversion Fix

## Problem
The turn toggle button graphic was showing the **opposite direction** from the trajectory preview and actual rock physics.

Example:
- Player starts turn ? `rm.inturn = false` (OUT-TURN)
- Trajectory shows **RIGHT curl** (correct) ?
- Rock throws **RIGHT curl** (correct) ?
- **Button graphic shows LEFT curl** (wrong!) ?

## Root Cause

The animator logic in `TurnAnim.cs` was **inverted**:

```csharp
// OLD CODE (WRONG)
IEnumerator IsPressed(bool inturn)
{
    if (inturn)
    {
        anim.SetBool("inturn", false);  // IN-TURN ? set animator to FALSE?!
    }
    else
    {
        anim.SetBool("inturn", true);   // OUT-TURN ? set animator to TRUE?!
    }
}
```

This made the animator show:
- When `rm.inturn = true` (IN-TURN, LEFT curl) ? Animator gets `false` ? Shows **RIGHT curl** ?
- When `rm.inturn = false` (OUT-TURN, RIGHT curl) ? Animator gets `true` ? Shows **LEFT curl** ?

## The Fix

Changed the animator to use **direct mapping** (not inverted):

```csharp
// NEW CODE (CORRECT)
IEnumerator IsPressed(bool inturn)
{
    if (inturn)
    {
        anim.SetBool("inturn", true);   // IN-TURN ? animator TRUE ? LEFT curl ?
    }
    else
    {
        anim.SetBool("inturn", false);  // OUT-TURN ? animator FALSE ? RIGHT curl ?
    }
}

public void SetTurn(bool inturn)
{
    if (inturn)
    {
        anim.SetBool("inturn", true);   // Match IsPressed()
    }
    else
    {
        anim.SetBool("inturn", false);
    }
}
```

## Unified Convention (Final)

**ALL systems now use the same direct mapping:**

| `rm.inturn` | `flipAxis` | `anim "inturn"` | Torque | Curl Direction | Result |
|-------------|------------|-----------------|--------|----------------|--------|
| `true` | `true` | `true` | `-` (neg) | LEFT | IN-TURN ? |
| `false` | `false` | `false` | `+` (pos) | RIGHT | OUT-TURN ? |

### System Mapping
- **GameManager**: Sets `rm.inturn = false` ? OUT-TURN
- **TurnAnim** (button): Toggles `rm.inturn` ? Updates `flipAxis` ? Updates animator
- **TrajectoryLine**: Reads `rm.inturn` ? Shows trajectory
- **Traj_Transform**: Reads `rm.inturn` ? Shows Bezier curve
- **Rock_Force**: Reads `flipAxis` ? Applies torque
- **Animator**: Reads `anim "inturn"` ? Shows graphic

**All values synchronized!** No more inversions, no more confusion.

## Testing Results

### Before Fix
```
Player turn starts (rm.inturn = false)
?? Trajectory: RIGHT curl ?
?? Rock physics: RIGHT curl ?
?? Button graphic: LEFT curl ? WRONG!
```

### After Fix
```
Player turn starts (rm.inturn = false)
?? Trajectory: RIGHT curl ?
?? Rock physics: RIGHT curl ?
?? Button graphic: RIGHT curl ? CORRECT!

Player toggles button (rm.inturn = true)
?? Trajectory: LEFT curl ?
?? Rock physics: LEFT curl ?
?? Button graphic: LEFT curl ? CORRECT!
```

## Files Modified

**`Assets\Scripts\TurnAnim.cs`** (Lines 107-140)
- Fixed `IsPressed()` to use direct mapping
- Fixed `SetTurn()` to use direct mapping
- Updated comments to reflect correct behavior

## Why This Happened

The old documentation claimed the animator was "intentionally inverted for historical reasons" but this was **incorrect**. The animator asset itself uses standard naming:
- `anim.SetBool("inturn", true)` = Show IN-TURN graphic (left curl)
- `anim.SetBool("inturn", false)` = Show OUT-TURN graphic (right curl)

The inversion was a **bug**, not a feature. It's now fixed.

## Build Status

? **Build Successful** - All changes compile without errors

## Impact

### Fixed
- ? Turn button graphic now matches trajectory preview
- ? Turn button graphic now matches rock physics
- ? All four systems (button, trajectory, Bezier, physics) synchronized
- ? No more player confusion about which way the rock will curl

### No Regression
- ? AI turns still work correctly
- ? Button still toggles properly
- ? Animation still plays smoothly
- ? All other turn systems unchanged

## Summary

The turn animator was the **last piece** of the turn synchronization puzzle. Now:

1. ? **AI Turn Override Fix** - `RockManager` doesn't override AI calculations
2. ? **Trajectory Visual Fix** - Bezier curve matches physics
3. ? **Turn Synchronization Fix** - Button updates both `rm.inturn` and `flipAxis`
4. ? **Animator Fix** - Graphic matches all other systems

**ALL turn-related systems are now perfectly synchronized!** ??
