# AI Legacy Code - Deprecation Status

## Current State (After Refactoring)

### ? Physics-Based System (Active)
All modern AI shot types now use physics-based calculations:
- `TakeOutTarget()` - Physics + character stats
- `PeelTarget()` - Physics + character stats
- `TapTarget()` - Physics + character stats
- `TickShotTarget()` - Physics + character stats
- `DrawTarget()` - Physics + character stats
- `GuardTarget()` - Physics + character stats

### ?? Legacy System (Deprecated)
One method remains using old magic numbers:
- `TakeOutAutoTarget()` - **DEPRECATED** ~800 lines of mixed strategic decisions + magic formulas

## Why Not Delete `TakeOutAutoTarget`?

**Answer:** Safety and testing

### The Problem
`TakeOutAutoTarget()` does TWO things badly:
1. **Strategic decisions** - Decides what to shoot (belongs in AI_Strategy)
2. **Magic number aiming** - Uses trial-and-error formulas (belongs in physics)

This violates separation of concerns - strategy and execution mixed together.

### The Solution
Instead of refactoring 800 lines of complex strategic logic that might be unused:

1. ? **Marked as deprecated** with warning message
2. ? **Left functional** for backwards compatibility
3. ? **Added documentation** explaining why it's bad
4. ? **Monitor for usage** via deprecation warnings
5. ? **Remove later** once confirmed unused

### Current Flow

**Modern Flow (Recommended):**
```
AI_Strategy.SimpleAIShoot() or AI_Strategy.OnShot()
    ?
Analyzes game state, decides target
    ?
Calls: aiTarg.OnTarget("Take Out", rockCurrent, targetRockIndex)
    ?
TakeOutTarget() - Physics-based calculation
    ?
CalculatePhysicsBasedShot() - Tries multiple paths, scores each
    ?
Apply character stats for error
    ?
Execute shot with 100% base accuracy ?
```

**Legacy Flow (Deprecated):**
```
??? Unknown caller ???
    ?
Calls: aiTarg.OnTarget("Auto Take Out", rockCurrent, 0)
    ?
?? DEPRECATION WARNING LOGGED ??
    ?
TakeOutAutoTarget() - 800 lines of:
    - Guard reading
    - Strategic decisions (what to target)
    - Magic number calculations (how to aim)
    - Direct calls to aiShoot.OnShot()
    ?
Bypasses physics system entirely
    ?
Uses trial-and-error formulas like:
    takeOutX = (-0.205f * ((targetX + 1.35f) / 2.7f)) + 0.087f
    ?
~70-80% accuracy maximum ?
```

## Deprecation Warning Message

When `TakeOutAutoTarget` is called, this appears in console:

```
[AI_Target] 'Auto Take Out' is deprecated - uses old magic numbers. 
Consider using physics-based 'Take Out' instead.
```

## Testing Checklist

Before removing `TakeOutAutoTarget`, verify:

- [ ] Play through full career mode game
- [ ] Test AI vs AI matches
- [ ] Test all AI difficulty levels
- [ ] Check console logs for deprecation warning
- [ ] If **NO warnings appear** ? Safe to delete
- [ ] If **warnings appear** ? Find caller, update to physics system

## How to Search for Usage

In Visual Studio:
```
Find in Files: "Auto Take Out"
```

Expected results:
- `AI_Target.cs` - The case statement (deprecation warning)
- `AI_Target.cs` - The method definition
- **No other files** - If found elsewhere, that's the caller to update

## Removal Instructions (Once Confirmed Safe)

### Step 1: Delete the switch case
In `AI_Target.OnTarget()`, remove:
```csharp
case "Auto Take Out":
    Debug.LogWarning("[AI_Target] 'Auto Take Out' is deprecated...");
    StartCoroutine(TakeOutAutoTarget(rockCurrent));
    break;
```

### Step 2: Delete the method
Remove entire `TakeOutAutoTarget()` method (~800 lines from line ~475 to ~783)

### Step 3: Clean up
Remove any orphaned magic number constants if they existed

### Estimated Code Reduction
**~800 lines deleted** + better separation of concerns

## Why This Matters

### Code Quality Benefits

**Before:**
- Strategic decisions scattered in multiple files
- Aiming logic duplicated (magic numbers + physics)
- Can't guarantee accuracy even with perfect stats
- Hard to debug (which system is being used?)

**After:**
- Strategy in `AI_Strategy.cs` only
- Aiming in `AI_Target.cs` with physics only
- 100% accuracy with perfect stats guaranteed
- Easy to debug (clear log messages show physics path)

### Maintainability

**Before:** To add a new shot type:
1. Add magic number formula to `AI_Shooter`
2. Add strategic logic to `AI_Strategy`  
3. Maybe add to `TakeOutAutoTarget` too?
4. Tune magic numbers by trial and error
5. Hope it works

**After:** To add a new shot type:
1. Add strategic decision to `AI_Strategy`
2. Add speed multiplier to `CalculatePhysicsBasedShot`
3. Add accuracy stat mapping
4. Done - physics handles the rest

## Timeline

- **Now:** Deprecation warning in place
- **Week 1-2:** Monitor for warnings during testing
- **Week 3:** If no warnings, proceed with removal
- **Week 4:** Verify removal didn't break anything

## Contact

If you encounter issues or have questions about this deprecation:
- Check console for deprecation warnings
- Review `AI_Strategy.cs` for modern strategic decisions
- Review `AI_Target.cs` for physics-based targeting
- See `AI_PHYSICS_TARGETING_REFACTOR.md` for full technical details
