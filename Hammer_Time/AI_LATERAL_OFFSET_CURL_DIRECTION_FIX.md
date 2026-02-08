# AI Lateral Offset Curl Direction Fix

## Problem Identified

The AI targeting system was **aiming in the wrong direction** when trying to hit rocks with curl.

### The Bug
In `CalculatePhysicsBasedShot()`, the lateral sweep was using:
```csharp
Vector2 aimPoint = new Vector2(targetRockPosition.x + lateralOffset, aimPointY);
```

This DIRECTLY added the offset to the target X position, which is **backwards for curling physics**!

### Example of the Bug
Target rock at **x = -0.605** (left side)
- OUT-TURN lateral offset: -1.00
- Aim point X: -0.605 + (-1.00) = **-1.605** (way too far left!)
- Rock curls LEFT from -1.605, ending at **x = -0.225** (missed target completely)

The rock should have aimed to the RIGHT of the target and curled back LEFT to hit it!

## The Fix

### Understanding Curling Physics
- **IN-TURN**: Rock curls to the RIGHT (+X direction)
  - To hit a target, aim LEFT of it and let it curl back right
- **OUT-TURN**: Rock curls to the LEFT (-X direction)
  - To hit a target, aim RIGHT of it and let it curl back left

### Code Changes

**Location**: `Assets/Scripts/AI/AI_Target.cs` - `CalculatePhysicsBasedShot()` method

Changed the aim point calculation to **invert offset based on turn direction**:

```csharp
// OLD (BROKEN):
Vector2 aimPoint = new Vector2(targetRockPosition.x + lateralOffset, aimPointY);

// NEW (FIXED):
// IN-TURN (curls RIGHT): aim LEFT of target, rock curls back right to hit
// OUT-TURN (curls LEFT): aim RIGHT of target, rock curls back left to hit
float aimOffsetX = tryInTurn ? -lateralOffset : lateralOffset;
Vector2 aimPoint = new Vector2(targetRockPosition.x + aimOffsetX, aimPointY);
```

### Applied to Both Sweeps
1. **Fine sweep** (line ~152): Tight range -0.3 to +0.3
2. **Coarse sweep** (line ~239): Wide range -1.0 to +1.0

Both now correctly invert the lateral offset based on turn direction.

## Expected Behavior After Fix

### For OUT-TURN hitting target at x=-0.605:
- Lateral offset -1.00 becomes aimOffsetX = **+1.00** (inverted!)
- Aim point X: -0.605 + 1.00 = **+0.395** (right side)
- Rock launches toward +0.395, curls LEFT during travel
- Final position: **x ? -0.605** (hits target!)

### For IN-TURN hitting target at x=+0.5:
- Lateral offset +0.8 becomes aimOffsetX = **-0.8** (inverted!)
- Aim point X: 0.5 + (-0.8) = **-0.3** (left side)
- Rock launches toward -0.3, curls RIGHT during travel
- Final position: **x ? +0.5** (hits target!)

## Why This Matters

This was causing the AI to **systematically miss all takeout shots** because:
1. It was aiming in the wrong direction relative to curl
2. The physics simulation found "hits" but they were based on incorrect geometry
3. The actual shot would miss because the aim point was backwards

With this fix, the AI will:
- ? Correctly aim opposite to curl direction
- ? Let the rock curl back to the target
- ? Achieve accurate takeouts and collision shots

## Testing Recommendations

1. **Test OUT-TURN takeouts** on left-side targets (x < 0)
   - Should aim RIGHT of target and curl back LEFT
2. **Test IN-TURN takeouts** on right-side targets (x > 0)
   - Should aim LEFT of target and curl back RIGHT
3. **Verify center targets** (x ? 0)
   - Should have minimal lateral offset
4. **Check debug logs** for aim point positions
   - Aim point X should be OPPOSITE sign from target X when offset is large

## Related Files
- `Assets/Scripts/AI/AI_Target.cs` - Physics-based targeting system
- `Assets/Scripts/UI/TrajectorySimulator.cs` - Curl physics simulation

## Status
? **FIXED** - Build successful, ready for testing
