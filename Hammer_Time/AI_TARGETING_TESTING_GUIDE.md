# Testing Guide: AI Collision Shot Improvements

## Quick Test Scenarios

### Test 1: Center Raise (Easiest)
**Setup:**
1. Place a rock at (0.0, 6.5) - dead center button
2. Have AI attempt a raise
3. Watch console for: `[AI_Target] Raise SUCCESS`

**Expected Result:**
- AI should find a shot (score > 0)
- Rock should lift forward into the house
- Console should show good angle bonus (+8 or +4)

### Test 2: Side Raise (Previously Impossible)
**Setup:**
1. Place a rock at (1.0, 6.5) - right side of 12-foot
2. Have AI attempt a raise
3. Watch console logs

**Expected Result:**
- **OLD**: Would likely fail or score very low
- **NEW**: Should find a shot with xOffset between 0.3-0.8
- Should still lift rock forward (maybe slightly off-angle)

### Test 3: Extreme Side Takeout
**Setup:**
1. Place a rock at (1.2, 5.0) - far right guard position
2. Have AI attempt a takeout
3. Monitor success rate over 5 attempts

**Expected Result:**
- **OLD**: ~40% hit rate even with maxed stats
- **NEW**: ~80%+ hit rate with maxed stats

### Test 4: Peel Accuracy
**Setup:**
1. Place a rock at (-0.8, 7.0) - left back 12-foot
2. Have AI attempt a peel
3. Check if rock gets fully removed (not just bumped)

**Expected Result:**
- Should hit rock cleanly
- Should drive through (not just tap)
- Target rock should exit play area

## Debug Testing Mode

### Disable Accuracy Errors (Temporary)
To see the "perfect" AI targeting without randomness:

**In `AI_Shooter.cs`, find `GetAccuracyError()` method:**
```csharp
private Vector2 GetAccuracyError(float accuracy, float baseMaxError)
{
    // TEMPORARY: Return zero error for perfect testing
    return Vector2.zero;
    
    // Original code (re-enable after testing):
    // float accuracyRatio = Mathf.Clamp01(accuracy / 100f);
    // float maxError = baseMaxError * (1f - accuracyRatio);
    // return Random.insideUnitCircle * maxError;
}
```

**Re-enable after testing!**

### Max Out Stats (Temporary)
To see best-case performance:

**In Unity Editor:**
1. Find your AI team members in `TeamManager`
2. Set all accuracy stats to 100:
   - `guardAccuracy` = 100
   - `drawAccuracy` = 100
   - `takeOutAccuracy` = 100

## Console Log Interpretation

### Success Log Example
```
[AI_Target] Raise SUCCESS - Score: 12.35, Pullback: (-0.05, -27.2), InTurn: True, Target: (0.8, 6.5)
```
**Meaning:**
- Found a valid raise shot
- Score of 12.35 (10 for hit + extra for good angle)
- Will pull back to (-0.05, -27.2)
- Using in-turn
- Targeting rock at (0.8, 6.5)

### Failure Log Example
```
[AI_Target] Raise FAILED - No valid shot found! BestScore: -0.85, Target: (1.5, 4.0)
```
**Meaning:**
- Could not find a good shot
- Best attempt scored -0.85 (missed by ~0.85 units)
- Target at (1.5, 4.0) may be unreachable or blocked

## What to Look For

### Raises
? **Good Signs:**
- Score > 8 (indicating good angle bonus)
- Rock lifts forward into house
- Consistent success on center raises

? **Bad Signs:**
- Score < 5 (no angle bonus, poor approach)
- Rock barely moves or moves sideways
- Failure on center raises

### Takeouts/Peels
? **Good Signs:**
- Score > 8 (indicating direct hit)
- Target rock exits play
- Consistent hits even on side shots

? **Bad Signs:**
- Score < 0 (missed entirely)
- Hitting wrong rocks
- Missing side shots consistently

### Ticks
? **Good Signs:**
- Score > 5 (indicating glancing angle bonus)
- Light contact that redirects rock
- Throwing rock continues past

? **Bad Signs:**
- Head-on collision (not a tick)
- Missing target rock
- No angle bonus in console

## Performance Check

### Frame Rate
- Monitor FPS during AI shot selection
- Should have **no noticeable impact** even with 4x more simulations
- If FPS drops > 5%, there may be an issue

### Time Measurement (Optional)
Add this to `AI_Target.CalculatePhysicsBasedShot()` at the start:
```csharp
float startTime = Time.realtimeSinceStartup;
```

And at the end (before return):
```csharp
float elapsedTime = (Time.realtimeSinceStartup - startTime) * 1000f;
Debug.Log($"[AI_Target] {shotType} calculation took {elapsedTime:F2}ms");
```

**Expected**: < 2ms even for raises (worst case)

## Comparison Test Protocol

### Setup
1. Create a test scenario with 5-10 rocks in various positions
2. Note which positions are:
   - Center (x ? 0)
   - Half-side (x ? ±0.5)
   - Full-side (x ? ±1.0)
   - Extreme (x ? ±1.2)

### Test Method
1. **Backup your project first!**
2. Test with NEW system (current code)
3. Note success rates for each position type
4. Revert to OLD system (git checkout or manual rollback)
5. Test same scenarios
6. Compare results

### Expected Results

| Position Type | Old Success % | New Success % | Improvement |
|---------------|---------------|---------------|-------------|
| Center | 80% | 95%+ | +15% |
| Half-side | 60% | 90%+ | +30% |
| Full-side | 30% | 80%+ | +50% |
| Extreme | 5% | 60%+ | +55% |

*With maxed stats (100/100) and no accuracy errors*

## Known Limitations

Even with these improvements:

1. **Blocked Shots**: If guards completely block the target, AI still can't shoot through them
2. **Physics Edge Cases**: Extremely tight angles may still fail due to physics simulation limits
3. **Accuracy Stats**: Lower stats will still cause misses (as intended for game balance)
4. **Unreachable Targets**: Targets behind boards or at impossible angles will still fail

## Troubleshooting

### "Raise still doesn't work!"
- Check if rock is too far back (y > 8.0)
- Check if blocked by guards
- Verify `behindOffset` is actually being applied (add debug log)

### "Takeouts miss more than before!"
- Did you accidentally change accuracy stats?
- Check if `isCollisionShot` flag is being passed correctly
- Verify lateral extension math is correct

### "AI takes too long to shoot!"
- Check frame rate - should have no impact
- Add timing debug logs
- Reduce search range if absolutely necessary (but shouldn't be needed)

## Success Criteria

? **Changes are working if:**
1. Center raises work > 90% of the time (maxed stats, no error)
2. Side raises (x ± 0.8) work > 70% of the time
3. Side takeouts (x ± 1.0) work > 80% of the time
4. Console shows "SUCCESS" logs with scores > 8
5. No performance degradation
6. Raises actually lift rocks forward (not just bump them)

? **Something is wrong if:**
1. Center raises fail > 20% of the time
2. Console shows "FAILED" on easy shots
3. Scores are consistently < 5
4. Frame rate drops noticeably
5. Raises don't lift rocks (physics issue)
