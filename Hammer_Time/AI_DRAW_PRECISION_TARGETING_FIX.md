# AI Draw Shot Precision Targeting Fix

## Problem

AI draw shots were landing too far from intended targets:
- Wide radial sweep (up to 1.0m from target)
- Low acceptance threshold (15/130 = 11.5%)
- Too many loose candidates accepted

## Changes Made

### 1. Tighter Radial Search (Line ~2690)

**Before**:
```csharp
float[] radii = new float[] { 0.15f, 0.3f, 0.5f, 0.7f, 1.0f }; // Up to 1.0m!
float[] angles = new float[] { 0f, 30f, 60f, 90f, 120f, 150f, 180f, 210f, 240f, 270f, 300f, 330f }; // 12 angles
// = 1 + (5 radii × 12 angles) = 61 candidates
```

**After**:
```csharp
float[] radii = new float[] { 0.10f, 0.20f, 0.30f, 0.40f }; // Max 0.4m (40cm)
float[] angles = new float[] { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f }; // 8 angles
// = 1 + (4 radii × 8 angles) = 33 candidates (HALF as many, but tighter!)
```

**Impact**:
- ? 60% tighter search radius (1.0m ? 0.4m)
- ? 46% fewer candidates (61 ? 33) = faster
- ? Forces physics to find closer paths

### 2. Stricter Proximity Scoring (Line ~2753)

**Before**:
```csharp
if (distToTarget < 0.1f)  proximityScore = 60f;  // <10cm
else if (distToTarget < 0.3f)  proximityScore = 50f;  // <30cm
else if (distToTarget < 0.6f)  proximityScore = 35f;  // <60cm
else if (distToTarget < 1.0f)  proximityScore = 20f;  // <1m
else proximityScore = 10f * ...; // Further = worse
```

**After**:
```csharp
if (distToTarget < 0.08f)  proximityScore = 60f;  // <8cm (tighter!)
else if (distToTarget < 0.15f)  proximityScore = 55f;  // <15cm
else if (distToTarget < 0.25f)  proximityScore = 48f;  // <25cm
else if (distToTarget < 0.40f)  proximityScore = 38f;  // <40cm
else if (distToTarget < 0.60f)  proximityScore = 25f;  // <60cm
else if (distToTarget < 0.80f)  proximityScore = 15f;  // <80cm
else proximityScore = 5f * ...; // Very far = much worse
```

**Impact**:
- ? More granular scoring (7 tiers instead of 5)
- ? Rewards pinpoint accuracy (<8cm)
- ? Penalizes loose shots more aggressively

### 3. Higher Acceptance Threshold (Line ~2950)

**Before**:
```csharp
if (bestScore >= 15f)  // Accept 11.5% quality (15/130)
```

**After**:
```csharp
if (bestScore >= 40f)  // Demand 30.8% quality (40/130)
```

**Score Requirements**:

To reach 40 points, AI needs:
- **Option A**: `<15cm proximity (55 pts)` alone qualifies ?
- **Option B**: `<25cm proximity (48 pts)` alone qualifies ?
- **Option C**: `<40cm proximity (38 pts) + guard protection (15 pts)` = 53 pts ?
- **Option D**: `<40cm proximity (38 pts) + in-house bonus (20 pts)` = 58 pts ?

**Impact**:
- ? Demands <25cm accuracy for clean shots
- ? Allows <40cm if protected/scoring
- ? Rejects loose shots (>40cm unless exceptional strategic value)

## Expected Results

### Before Fix:
```
[Physics Draw] ? SUCCESS! Score: 18.3/130
  Final position: (0.34, 7.20)  ? 80cm from target (0.00, 6.50)
  Tested 61 candidates
```

### After Fix:
```
[Physics Draw] ? SUCCESS! Score: 55.2/130 (threshold: 40)
  Final position: (0.03, 6.58)  ? 12cm from target (0.00, 6.50) ?
  Distance to target: 0.120m
  Tested 33 candidates (tight 0.4m radius)
  Strategy: PRECISION targeting
```

## Philosophy Change

**Old Approach**: "Get in the general area and hope for the best"
- Wide search ? accepts 1m error
- Low threshold ? accepts weak shots

**New Approach**: "Hit the exact spot or find a slightly better nearby position"
- Tight search ? max 40cm error
- High threshold ? demands <25cm accuracy
- Fallback still available if needed

## Statistics

| Metric | Before | After | Change |
|--------|--------|-------|--------|
| Max search radius | 1.0m | 0.4m | **-60%** ?? |
| Candidates tested | 61 | 33 | **-46%** ?? |
| Min acceptance score | 15/130 (11.5%) | 40/130 (30.8%) | **+167%** ?? |
| Required accuracy (clean shot) | <60cm | <25cm | **-58%** ?? |
| Performance | Slower (more tests) | Faster (fewer tests) | **+46%** ? |

## Testing

Test scenarios to verify:

1. **Button draw**: Should land within 15cm of (0.0, 6.5)
2. **Side draw**: Should land within 25cm of target corner
3. **Protected draw**: Can be up to 40cm off if behind guard
4. **Blocked path**: Should still find alternative within 40cm radius

Expected log output:
```
[Physics Draw] Generated 33 candidate positions (tight 0.4m radius)
[Physics Draw] ? SUCCESS! Score: 52.1/130 (threshold: 40)
  Distance to target: 0.187m  ? Should be <0.25m for most shots!
```

If AI can't find a shot (rare):
```
[Physics Draw] All candidates scored low (best: 35.2)
[Physics Draw] ? FALLBACK: Direct button shot
```

## Tuning Guide

If draws are still too loose:
1. **Reduce max radius**: `0.40f ? 0.30f` (line 2690)
2. **Raise threshold**: `40f ? 50f` (line 2950)
3. **Tighten proximity scoring**: Reduce points for >25cm

If draws fail too often:
1. **Increase max radius**: `0.40f ? 0.50f`
2. **Lower threshold**: `40f ? 35f`
3. **Add more angles**: 8 ? 12 for better path finding

## Related Files
- `Assets/Scripts/AI/AI_Target.cs` - Main draw targeting logic
- `Assets/Scripts/AI/AI_Strategy.cs` - Decides when to draw
- `Assets/Scripts/UI/TrajectorySimulator.cs` - Physics simulation

---

## Summary

? **Radii tightened**: 1.0m ? 0.4m (60% tighter)
? **Scoring stricter**: Demands <25cm for clean shots
? **Threshold raised**: 15 ? 40 (167% higher standards)
? **Performance improved**: 33 candidates instead of 61 (46% faster)

The AI will now **hit targets much more precisely** instead of accepting "close enough" positions!
