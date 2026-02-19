# Spring Calibration Testing Guide

## Quick Calibration Method

### Setup
1. Start a Quick Test Game (no AI)
2. Open Unity Console (Ctrl+Shift+C)
3. Have a notepad ready to record observations

### Test Procedure

#### Test 1: Short Pullback (1.5 units)
1. Pull rock back to **Y = -25.5** (launcher is at Y = -24, so 1.5 units back)
2. Note console output:
   ```
   [SpringPhysics] Dist: 1.5000, v_final: 7.35
   ```
3. Release and observe:
   - **Expected**: Rock barely reaches the house (Y ? 5-6)
   - **Too far**: Rock goes past Y = 8 ? **DECREASE** calibration factor by 0.05
   - **Too short**: Rock stops at Y = 3-4 ? **INCREASE** calibration factor by 0.05

#### Test 2: Medium Pullback (2.0 units)
1. Pull rock back to **Y = -26.0**
2. Note console output:
   ```
   [SpringPhysics] Dist: 2.0000, v_final: 9.80
   ```
3. Release and observe:
   - **Expected**: Rock reaches center (Y ? 6.5-7.0)
   - **Too far**: Rock goes to Y = 9+ ? **DECREASE** calibration factor by 0.03
   - **Too short**: Rock stops at Y = 5-6 ? **INCREASE** calibration factor by 0.03

#### Test 3: Heavy Pullback (3.0 units)
1. Pull rock back to **Y = -27.0**
2. Note console output:
   ```
   [SpringPhysics] Dist: 3.0000, v_final: 14.70
   ```
3. Release and observe:
   - **Expected**: Rock reaches back of house (Y ? 8-9)
   - **Too far**: Rock exits house at Y = 10+ ? **DECREASE** calibration factor by 0.02
   - **Too short**: Rock stops at Y = 6-7 ? **INCREASE** calibration factor by 0.02

### Calibration Factor Adjustment

Current default: **0.63**

| Observation | Adjustment | New Value |
|-------------|------------|-----------|
| Trajectory 5% too short | +0.03 | **0.66** |
| Trajectory 10% too short | +0.05 | **0.68** |
| Trajectory 5% too far | -0.03 | **0.60** |
| Trajectory 10% too far | -0.05 | **0.58** |

### Advanced: Per-Distance Calibration

If you notice the calibration is good at one distance but off at others, you might need **non-linear calibration**:

```csharp
// In TrajectorySimulator.cs, replace:
float calibrationFactor = 0.63f;

// With distance-dependent calibration:
float calibrationFactor;
if (springDistance < 2.0f)
{
    // Short shots need slightly less energy (friction dominates)
    calibrationFactor = 0.60f;
}
else if (springDistance < 2.5f)
{
    // Medium shots use baseline
    calibrationFactor = 0.63f;
}
else
{
    // Heavy shots need slightly more (overcome initial friction)
    calibrationFactor = 0.66f;
}
```

## Automated Calibration Test

If you want to run many tests automatically:

### Step 1: Create Test Script

Create `Assets/Scripts/Debug/SpringCalibrationTest.cs`:

```csharp
using UnityEngine;
using System.Collections;

public class SpringCalibrationTest : MonoBehaviour
{
    public GameManager gm;
    public TrajectoryLine trajectoryLine;
    
    [Header("Test Parameters")]
    public float[] testDistances = { 1.5f, 2.0f, 2.5f, 3.0f };
    public int testsPerDistance = 5;
    
    [Header("Results")]
    public string results = "";
    
    public void RunCalibrationTests()
    {
        StartCoroutine(RunTests());
    }
    
    private IEnumerator RunTests()
    {
        results = "=== SPRING CALIBRATION TEST RESULTS ===\n\n";
        
        foreach (float distance in testDistances)
        {
            float avgFinalY = 0f;
            
            for (int i = 0; i < testsPerDistance; i++)
            {
                // Setup rock
                GameObject rock = gm.rockList[gm.rockCurrent].rock;
                Vector2 launcherPos = new Vector2(0f, -24f);
                Vector2 pullbackPos = new Vector2(0f, -24f - distance);
                
                // Calculate velocity
                SpringJoint2D spring = rock.GetComponent<SpringJoint2D>();
                Vector2 velocity = TrajectorySimulator.CalculateInitialVelocityFromSpring(
                    pullbackPos, launcherPos, spring.frequency, spring.dampingRatio
                );
                
                // Simulate trajectory
                TrajectorySimulator sim = new TrajectorySimulator(0.38f, 0.25f);
                List<Vector2> trajectory = sim.SimulateTrajectory(
                    launcherPos, velocity, false, 250, null
                );
                
                // Get final position
                TrajectorySimulator.CollisionInfo info = sim.GetCollisionInfo();
                avgFinalY += info.finalPosition.y;
                
                yield return null;
            }
            
            avgFinalY /= testsPerDistance;
            
            results += $"Distance: {distance:F2} units\n";
            results += $"  Avg Final Y: {avgFinalY:F2}\n";
            results += $"  Velocity: {(distance * 9.42f * 0.825f * 0.63f):F2}\n\n";
        }
        
        Debug.Log(results);
    }
}
```

### Step 2: Attach to GameObject

1. Create empty GameObject: "SpringCalibrationTester"
2. Add component: `SpringCalibrationTest`
3. Assign `GameManager` and `TrajectoryLine` references
4. In Inspector, click "Run Calibration Tests" (add button in custom inspector)

### Step 3: Analyze Results

The test will output:
```
=== SPRING CALIBRATION TEST RESULTS ===

Distance: 1.50 units
  Avg Final Y: 5.23
  Velocity: 7.35

Distance: 2.00 units
  Avg Final Y: 6.87
  Velocity: 9.80

Distance: 2.50 units
  Avg Final Y: 8.12
  Velocity: 12.25

Distance: 3.00 units
  Avg Final Y: 9.45
  Velocity: 14.70
```

Compare to your **desired final positions**:
- 1.5 units ? should reach Y = 5-6 (front of house)
- 2.0 units ? should reach Y = 6.5-7 (center)
- 2.5 units ? should reach Y = 7.5-8 (back center)
- 3.0 units ? should reach Y = 8.5-9 (back of house)

If all results are consistently **too low**, increase calibration factor.  
If all results are consistently **too high**, decrease calibration factor.

## Verification Checklist

After adjusting the calibration factor, verify:

### ? Precision Test
- [ ] Pullback 2.000 vs 2.001 vs 2.002 produce different trajectories
- [ ] No "plateau" where different pullbacks give same result
- [ ] Velocity increases smoothly with distance (no jumps)

### ? Accuracy Test
- [ ] Predicted trajectory (dots) matches actual rock path (±5%)
- [ ] Aim circle shows within 0.2 units of actual final position
- [ ] Collision predictions happen at correct point

### ? Consistency Test
- [ ] Same pullback (e.g., 2.0 units) always gives same trajectory
- [ ] No random variation shot-to-shot
- [ ] Physics multipliers (if using) don't affect prediction accuracy

## Common Issues

### Issue: Trajectory correct at 2.0 units but wrong at other distances

**Cause**: Linear calibration doesn't account for non-linear friction/curl effects

**Solution**: Use distance-dependent calibration (see "Advanced" section above)

### Issue: Debug shows correct velocity but rock doesn't match

**Possible causes**:
1. **Rock_Force multipliers active** ? Check `springTensionMultiplier` in Rock_Force.cs
2. **Sweeping affecting path** ? Test with sweeping disabled
3. **Collision response changing velocity** ? Check for early undetected collisions

**Fix**: Ensure Rock_Force uses:
```csharp
springTensionMultiplier = 1.0f;
curlForceMultiplier = 1.0f;
```

### Issue: Console shows velocity but no trajectory draws

**Cause**: TrajectoryLine not calling DrawTrajectory() or dots not spawning

**Fix**: Check TrajectoryLine.cs `DrawTrajectory()` is called on mouse drag

## Final Calibration Values

Once tuned, document your final value:

```csharp
// TrajectorySimulator.cs line ~XXX
float calibrationFactor = 0.XX; // TUNED: [Date] by [Name] - [Brief description of testing method]
```

Example:
```csharp
float calibrationFactor = 0.65f; // TUNED: 2024-01-15 by Dev - Increased from 0.63 because shots were 3% too short across all distances
```

This makes it clear why the value was chosen and when it was last validated.
