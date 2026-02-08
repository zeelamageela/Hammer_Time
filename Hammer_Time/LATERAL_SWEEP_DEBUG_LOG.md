# Lateral Sweep Debug Log

## Issue Summary
- **Problem**: Lateral sweep finds 0 hits even though collisions ARE detected in simulation
- **Evidence**: Collision log shows hit at velocity 1.83 ? 1.08, but PathIntersectionQuality returns 0
- **Root Cause**: Path point spacing is too wide - collision happens BETWEEN sampled points

## Test Results

### Simulation Collision Detection
```
[Collision] Incoming: 88.7°, Exit: 171.6°, HitRock: 81.6°, Normal: 81.6°
InVel: 1.83, OutVel: 0.14, HitVel: 1.08
```
? **Collision IS detected by physics simulation**

### Lateral Sweep Results
```
[AI_Target] OUT-TURN finished - Tested: 34, Hits: 0, Best score: -inf
```
? **PathIntersectionQuality finds 0 hits**

## Solution
**Use collision events directly instead of geometric path checking!**

The simulation DETECTS collisions (see collision log), but `PathIntersectionQuality` checks if the PATH POINTS pass close to the target. If the collision happens BETWEEN two path points (likely with TIME_STEP = 0.05), the geometric check misses it.

**Fix**: Check `trajectorySimulator.GetCollisionInfo()` instead of `PathIntersectionQuality(path, ...)`

The collision data is RIGHT THERE in the simulator - we just need to USE it!

---
**Date**: Auto-generated from session  
**Status**: ? **SOLUTION IDENTIFIED** - Switch from geometric to event-based collision detection

