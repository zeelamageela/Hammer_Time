# AI Lateral Offset Fix - Out-Turn Missing Right

## ?? Problem

**User Report:** "the weight is good, but the aim is off! seems like an out turn is half a rock to the right"

**Analysis:**
- Velocity is correct (0.35f for takeouts = hack-to-board weight) ?
- Out-turn shots are missing **to the RIGHT by ~0.07m** (half a rock width)
- In-turn shots appear to be accurate

## ?? Root Cause

The issue is in how the `TrajectorySimulator` applies curl direction:

**Current Code (TrajectorySimulator.cs line 247-249):**
```csharp
Vector2 curlDirection = isInTurn 
    ? new Vector2(velocity.y, -velocity.x).normalized  // In-turn: curl RIGHT
    : new Vector2(-velocity.y, velocity.x).normalized; // Out-turn: curl LEFT
```

**Problem:** The perpendicular vector calculation is correct, BUT the sign may be inverted relative to how the actual Rock_Force applies curl in the game.

## ?? Solution

The fix needs to be applied in **TWO locations**:

### 1. Fix Curl Direction in TrajectorySimulator.cs

**Location:** Line ~247 in `TrajectorySimulator.cs`

**Change:**
```csharp
// OLD (incorrect):
Vector2 curlDirection = isInTurn 
    ? new Vector2(velocity.y, -velocity.x).normalized  // In-turn
    : new Vector2(-velocity.y, velocity.x).normalized; // Out-turn

// NEW (corrected):
Vector2 curlDirection = isInTurn 
    ? new Vector2(-velocity.y, velocity.x).normalized  // In-turn: curl LEFT (swap signs)
    : new Vector2(velocity.y, -velocity.x).normalized; // Out-turn: curl RIGHT (swap signs)
```

**Reasoning:** If out-turns are missing RIGHT, it means the curl is being applied in the OPPOSITE direction than it should be. Swapping the signs will reverse the curl direction.

### 2. Alternative: Lateral Offset Compensation

If swapping signs doesn't work (because the actual physics are different), add a lateral offset in `AI_Target.cs`:

**Location:** In `CalculatePullbackFromVelocity()` method

**Add after calculating pullback position:**
```csharp
// LATERAL OFFSET COMPENSATION for out-turn aim error
// Out-turns miss right by ~0.07m, so aim 0.07m left to compensate
if (!useInTurn) // If out-turn
{
    pullbackPosition.x -= 0.07f; // Aim left to compensate for right miss
}
```

## ?? Testing

**Test Case 1 - Out-turn Takeout:**
1. AI throws out-turn at rock on center line
2. **Before Fix:** Misses right by half a rock (~0.07m)
3. **After Fix:** Hits dead center

**Test Case 2 - In-turn Takeout:**
1. AI throws in-turn at rock on center line  
2. **Before/After:** Should hit accurately (no regression)

## ?? Expected Results

| Turn Type | Before Fix | After Fix |
|-----------|-----------|-----------|
| **In-turn** | Accurate ? | Accurate ? |
| **Out-turn** | Miss RIGHT by 0.07m ? | Accurate ? |

## ?? Implementation Steps

1. **Test Current Behavior:**
   - Use QuickTestGame with 100% accuracy AI
   - Place rock at button (center line)
   - Observe out-turn takeout aim

2. **Apply Fix Option 1 (Simulator):**
   - Swap curl direction signs in TrajectorySimulator.cs
   - Rebuild and test

3. **If Option 1 Doesn't Work, Apply Fix Option 2 (Offset):**
   - Add lateral offset in AI_Target.cs
   - Rebuild and test

4. **Verify:**
   - Test both in-turn and out-turn shots
   - Ensure no regression in draw shots
   - Confirm takeouts hit center accurately

## ?? Note

The curl direction in `TrajectorySimulator.cs` must match how `Rock_Force.cs` applies curl in the actual game physics. If they're opposite, the trajectory prediction won't match the actual rock movement.

**Status:** ?? **FIX NEEDED** - Awaiting manual code access to AI_Target.cs to implement lateral offset or curl sign fix.
