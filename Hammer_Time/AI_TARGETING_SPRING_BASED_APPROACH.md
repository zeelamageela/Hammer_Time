# AI Targeting - Spring-Based Empirical Approach

## Summary
After extensive debugging, we discovered the core issue: **the simulation physics don't match the real game physics perfectly**, making velocity-based calculations unreliable.

## The Problem
1. Simulator uses `linearDamping = 0.38` (correct!)
2. But curl, collision, and other physics details differ slightly
3. Result: Calculated velocities don't produce expected trajectories
4. Both IN-TURN and OUT-TURN get identical (bad) scores ? AI picks wrong turn

## The Solution: Use REAL Spring Physics!

Instead of trying to calculate perfect velocity, we should:

1. **Try different PULLBACK DISTANCES** (1.8 to 3.5 units)
2. **Use the REAL spring formula** to convert pullback ? velocity:
   ```csharp
   Vector2 springVelocity = TrajectorySimulator.CalculateInitialVelocityFromSpring(
       pullbackPosition,
       launcherPosition
   );
   ```
   This formula is **empirically calibrated** and WORKS in the real game!

3. **Simulate each pullback** to see where it lands
4. **Pick the best one** based on collision quality

## Why This Works
- Spring formula is proven to match real game (velocity multiplier = 5.9)
- We don't need perfect simulation - just need it **consistent enough** to rank options
- Trying many pullback distances = brute force search = finds working shot

## Implementation Status
**NOT YET IMPLEMENTED** - needs code changes to `CalculatePhysicsBasedShot()` in `AI_Target.cs`

The approach would replace the current velocity calculation loop with a pullback distance loop.

## Current Issue  
Rocks falling ~30 units short of target because velocity formula doesn't account for all physics correctly.

## Next Steps
1. Rewrite `CalculatePhysicsBasedShot` to use pullback distance sweep
2. Remove `CalculateVelocityToTarget` dependency 
3. Use spring physics exclusively for velocity conversion
4. Test and iterate on pullback distance range

---
*Created during debugging session after discovering simulation/reality mismatch*
