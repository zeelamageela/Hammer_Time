# AI Tee Line Sweeping - Complete Implementation ?

**Status**: ? **COMPLETE** - AI now makes intelligent real-time sweeping decisions!

---

## What We Built

### AI Tee Line Sweeping System

**Location**: Enhanced existing `AI_Shooter.MonitorAndSweepCoroutine()`

**Strategy**: 
1. ? **Help YOUR rocks** - Sweep to extend distance, adjust line/curl
2. ? **Interfere with OPPONENT rocks** - Make them overshoot or let them fail
3. ? **Real-time evaluation** - Monitors predicted trajectory vs actual path
4. ? **Strategic decisions** - Weight vs Line vs Curl vs Whoa

---

## The Decision Matrix

### For YOUR Rocks (Friend):

| Situation | Predicted Shortfall | Lateral Error | Action | Reasoning |
|-----------|---------------------|---------------|--------|-----------|
| **Critical** | >1.0m | Any | HARD SWEEP | Rock won't reach - desperate! |
| **Too slow** | >0.25m | Any | SWEEP WEIGHT | Rock falling short |
| **Good speed, wrong line** | <0.25m | >0.12m | SWEEP LINE | One sweeper straightens |
| **Good speed, not enough curl** | <0.25m | >0.12m | SWEEP CURL | One sweeper adds curl |
| **Perfect** | <0.25m | <0.12m | DO NOTHING | Don't mess it up! |

---

### For OPPONENT Rocks (Foe):

| Situation | Predicted Performance | Action | Reasoning |
|-----------|----------------------|--------|-----------|
| **On target** | Within 0.5m of target | SWEEP WEIGHT | Make them overshoot! |
| **Failing** | >0.5m short of target | DO NOTHING | Let them fail! |
| **Wrong line** | Lateral error present | DO NOTHING | Their mistake! |

---

## Implementation Details

### 1. Opponent Rock Detection

```csharp
// Check if rock belongs to opponent
bool isOpponentRock = (rockInfo.teamName != gm.rockList[currentRockNumber].rockInfo.teamName);
```

**Logic**:
- Compare rock's team name with current thrower's team
- If different = opponent rock

---

### 2. Opponent Interference Strategy

```csharp
// OPPONENT ROCK INTERFERENCE: Help them fail!
if (isOpponentRock)
{
    // Strategy: DON'T sweep (let their shot fail naturally)
    // Exception: If they're going to succeed AND we can make them overshoot, sweep them OUT
    if (pastTLine && predictedShortfall < 0.5f) // They're on target!
    {
        // Make them go TOO FAR by sweeping weight
        desiredState = "Weight";
        Debug.Log($"[AI_Sweeper] Opponent rock on target - sweeping to make them overshoot!");
    }
    else
    {
        // They're failing on their own - don't help them!
        desiredState = "None";
        Debug.Log($"[AI_Sweeper] Opponent rock failing - doing nothing");
    }
}
```

**Strategy Breakdown**:

#### Scenario 1: Opponent Rock Too Light
```
Opponent throws draw to button
?
Rock at Y=0: velocity=1.8 m/s (too slow!)
?
AI predicts: Will stop at Y=5.5 (short of Y=6.5)
?
AI Decision: DO NOTHING - let it fall short!
?
Result: Opponent rock stops short, no score ?
```

#### Scenario 2: Opponent Rock Perfect Weight
```
Opponent throws draw to button
?
Rock at Y=0: velocity=2.3 m/s (perfect!)
?
AI predicts: Will reach Y=6.5 (on target!)
?
AI Decision: SWEEP WEIGHT - make them overshoot!
?
Result: Opponent rock slides to Y=7.2, out of house ?
```

#### Scenario 3: Opponent Rock Too Heavy
```
Opponent throws draw to button
?
Rock at Y=0: velocity=3.0 m/s (too fast!)
?
AI predicts: Will reach Y=7.5 (past target)
?
AI Decision: DO NOTHING - let it overshoot!
?
Result: Opponent rock goes too far, no score ?
```

---

### 3. Your Rocks - Comprehensive Help

#### Weight Decision (Both Sweepers):

```csharp
// PRIORITY 1: CRITICAL DISTANCE (rock won't reach target!)
if (predictedShortfall > 1.0f)
{
    desiredState = "Critical"; // HARD SWEEP
}
// PRIORITY 2: SIGNIFICANT SHORTFALL
else if (predictedShortfall > distanceThreshold)
{
    desiredState = "Weight"; // Normal weight sweep
}
```

**Effect**:
- Reduces linear damping ? Rock goes farther
- Reduces angular damping ? Rock stays straighter
- **New realistic physics!** ?

---

#### Line Decision (One Sweeper):

```csharp
// In-turn curls LEFT (negative X)
if (isInTurn)
{
    desiredState = (lateralError > 0f) ? "Line" : "Curl";
    // Positive error = rock is right of target = needs to go LEFT = sweep LINE
}
```

**Effect**:
- Reduces linear damping ONLY
- Rock goes farther, curl unchanged
- **Separates distance from curl!** ?

---

#### Curl Decision (One Sweeper):

```csharp
// In-turn curls LEFT (negative X)
if (isInTurn)
{
    desiredState = (lateralError > 0f) ? "Line" : "Curl";
    // Negative error = rock is left of target = needs more curl = sweep CURL
}
```

**Effect**:
- Reduces angular damping ONLY
- Rock curls more, distance unchanged
- **Independent curl control!** ?

---

### 4. Sweeper Execution

```csharp
private void ApplySweepState(string state, bool isInTurn)
{
    switch (state)
    {
        case "None":
            aiSweep.sm.SweepWhoa(true);
            break;

        case "Weight":
        case "Critical":
            aiSweep.sm.SweepWeight(true); // Both sweepers
            break;

        case "Line":
            if (isInTurn)
                aiSweep.sm.SweepLeft(true);  // In-turn: left straightens
            else
                aiSweep.sm.SweepRight(true); // Out-turn: right straightens
            break;

        case "Curl":
            if (isInTurn)
                aiSweep.sm.SweepRight(true); // In-turn: right adds curl
            else
                aiSweep.sm.SweepLeft(true);  // Out-turn: left adds curl
            break;
    }
}
```

**Maps to new Sweep.cs physics**:
- `SweepWeight` ? Reduces both dampings ?
- `SweepLeft/Right` ? Reduces appropriate damping based on context ?
- `SweepWhoa` ? Resets to normal friction ?

---

## Strategic Examples

### Example 1: Your Draw to Button (Slightly Light)

```
AI throws draw to button
Target: Y=6.5, X=0.0
?
[Y=-16.15] Rock crosses tee line
  Velocity: 5.0 m/s
  Predicted: Will stop at Y=6.2 (0.3m short)
  Decision: SWEEP WEIGHT
?
[Y=-7] Re-evaluate
  Velocity: 3.4 m/s (good after sweep!)
  Predicted: Will reach Y=6.5
  Decision: DO NOTHING (it's good now!)
?
[Y=0] Re-evaluate
  Lateral: X=-0.05 (slightly left)
  Target: X=0.0
  Error: 0.05m (acceptable)
  Decision: DO NOTHING
?
[Y=3.5] Re-evaluate
  Predicted: Y=6.5, X=-0.08
  Decision: DO NOTHING (within tolerance)
?
Result: Rock reaches Y=6.5, X=-0.08 ? SUCCESS!
```

---

### Example 2: Opponent Draw to Button (Perfect)

```
Opponent throws draw to button
Target: Y=6.5, X=0.0
?
[Y=-16.15] Opponent rock crosses tee line
  Velocity: 5.2 m/s (perfect!)
  Predicted: Will reach Y=6.5 (on target!)
  AI Detection: THREAT!
  Decision: SWEEP WEIGHT (make them overshoot!)
?
[Y=-7] Re-evaluate
  Velocity: 3.5 m/s (still good after sweep)
  Predicted: Will reach Y=6.8 (overshot by 0.3m!)
  Decision: KEEP SWEEPING
?
[Y=0] Re-evaluate
  Velocity: 2.4 m/s
  Predicted: Will reach Y=7.0 (out of house!)
  Decision: WHOA (mission accomplished!)
?
Result: Opponent rock reaches Y=7.0 ? OUT OF HOUSE! ?
```

---

### Example 3: Your Draw with Wrong Line (In-Turn)

```
AI throws in-turn draw to button
Target: Y=6.5, X=-0.25 (expected curl)
?
[Y=-16.15] Rock crosses tee line
  Velocity: 5.2 m/s (perfect weight!)
  Predicted: Y=6.5, X=-0.10 (not enough curl!)
  Decision: DO NOTHING (too early to judge curl)
?
[Y=0] Re-evaluate
  Current: X=0.05 (right of center)
  Expected: X=-0.15 (should be curling left)
  Lateral error: +0.20m (not curling enough!)
  Decision: SWEEP CURL (right sweeper)
?
[Y=3.5] Re-evaluate
  Current: X=-0.10
  Expected: X=-0.20
  Lateral error: +0.10m (better, but still not enough)
  Decision: KEEP SWEEPING CURL
?
[Y=5] Re-evaluate
  Current: X=-0.22
  Expected: X=-0.25
  Lateral error: +0.03m (close enough!)
  Decision: WHOA
?
Result: Rock reaches Y=6.5, X=-0.24 ? ON TARGET!
```

---

## Physics Integration

### How It Works with New Damping Model:

**Weight Sweep** (Both Sweepers):
```csharp
// In Sweep.cs
rb.linearDamping -= (statCalc * sweepAmt);        // Distance ?
rb.angularDamping -= (statCalc * sweepAmt * 0.8f); // Straightness ?
```

**Effect on AI**:
- Rock travels farther (corrects shortfall) ?
- Rock curls less (side effect) ??

---

**Line Sweep** (One Sweeper):
```csharp
// In Sweep.cs
rb.linearDamping -= sweepAmt * statCalc / 4f; // Distance ?
// NO angular damping change
```

**Effect on AI**:
- Rock travels farther (if slightly short) ?
- Curl unchanged ?
- **Perfect for fine-tuning distance without affecting line!**

---

**Curl Sweep** (One Sweeper):
```csharp
// In Sweep.cs
rb.angularDamping -= sweepAmt * statCalc / 4f; // Curl ?
// NO linear damping change
```

**Effect on AI**:
- Rock curls more (corrects line error) ?
- Distance unchanged ?
- **Perfect for fine-tuning curl without affecting weight!**

---

## Debug Logs

### Expected Output (Your Rock - Too Light):

```
[AI_Shooter] Starting sweeping monitor: velocity=5.0 m/s, target=(0.0, 6.5), inTurn=True
[AI_Sweeper] Monitoring started - predicted path has 245 points
[AI_Sweeper] Rock crossed hog line - sweeping enabled!
[AI_Sweeper] Y=-16.15: State=Weight, LateralErr=0.000, Shortfall=0.35
[AI_Sweeper] Y=-7.00: State=Weight, LateralErr=-0.02, Shortfall=0.20
[AI_Sweeper] Y=-3.50: State=None, LateralErr=-0.03, Shortfall=0.05
[AI_Sweeper] Y=0.00: State=None, LateralErr=-0.08, Shortfall=-0.10
[AI_Sweeper] Rock stopped - WHOA
```

---

### Expected Output (Opponent Rock - On Target):

```
[AI_Sweeper] Starting sweeping monitor: velocity=5.2 m/s, target=(0.0, 6.5), inTurn=False
[AI_Sweeper] Monitoring started - predicted path has 248 points
[AI_Sweeper] Rock crossed hog line - sweeping enabled!
[AI_Sweeper] Opponent rock failing - doing nothing
[AI_Sweeper] Opponent rock failing - doing nothing
[AI_Sweeper] Opponent rock on target - sweeping to make them overshoot!
[AI_Sweeper] Y=0.00: State=Weight, LateralErr=0.01, Shortfall=0.15
[AI_Sweeper] Y=3.50: State=Weight, LateralErr=0.02, Shortfall=-0.20
[AI_Sweeper] Y=5.00: State=None, LateralErr=0.03, Shortfall=-0.50
[AI_Sweeper] Rock stopped - WHOA
```

**Result**: Opponent rock went from Y=6.5 (on target) to Y=7.1 (out of house) ?

---

## Strategic Gameplay

### Scenario 1: AI Draw to Button (Slightly Light)

**Setup**:
- AI aims for button (Y=6.5)
- Throws with velocity 5.0 m/s (slightly light)
- Target velocity: 5.2 m/s

**AI Sweeping**:
```
[Y=-16.15] "Rock is 0.2 m/s slow - SWEEP!"
?
Both sweepers engage
?
Linear damping: 0.38 ? 0.32
Angular damping: 0.32 ? 0.27
?
Rock travels extra 0.3m
?
[Y=-7] "Good velocity now - WHOA!"
?
Sweepers stop
?
Result: Rock reaches Y=6.5 ?
```

---

### Scenario 2: Opponent Draw to Button (Perfect)

**Setup**:
- Opponent aims for button (Y=6.5)
- Throws with velocity 5.2 m/s (perfect!)
- AI detects: This will score!

**AI Sweeping**:
```
[Y=-16.15] "Opponent rock is good - do nothing for now"
?
Wait...
?
[Y=0] "Still on target - time to interfere!"
?
Both sweepers engage
?
Linear damping: 0.38 ? 0.32
?
Rock travels extra 0.5m
?
Result: Rock reaches Y=7.0 (OUT OF HOUSE!) ?
```

**Sneaky!** The AI waits until the rock is committed, then sweeps it OUT! ??

---

### Scenario 3: Your Rock with Wrong Curl (In-Turn)

**Setup**:
- AI throws in-turn draw to button
- Good weight, but not curling enough
- Current: X=0.10, Expected: X=-0.25

**AI Sweeping**:
```
[Y=0] "Lateral error: +0.35m - need more curl!"
?
Right sweeper engages (CURL SWEEP)
?
Angular damping: 0.32 ? 0.28
?
Spin maintained longer
?
Rock curls extra 0.15m
?
[Y=3.5] "Lateral error now 0.10m - good enough!"
?
Sweeper stops
?
Result: Rock reaches Y=6.5, X=-0.22 ?
```

---

## Curl Direction Reference

### In-Turn (flipAxis = true):

**Curls LEFT (negative X)**

| Lateral Error | Meaning | Action | Sweeper |
|---------------|---------|--------|---------|
| **Positive** (+0.2m) | Rock is RIGHT of target | Need to curl MORE left | RIGHT sweeper (curl) |
| **Negative** (-0.2m) | Rock is LEFT of target | Need to straighten | LEFT sweeper (line) |

---

### Out-Turn (flipAxis = false):

**Curls RIGHT (positive X)**

| Lateral Error | Meaning | Action | Sweeper |
|---------------|---------|--------|---------|
| **Positive** (+0.2m) | Rock is RIGHT of target | Need to straighten | RIGHT sweeper (line) |
| **Negative** (-0.2m) | Rock is LEFT of target | Need to curl MORE right | LEFT sweeper (curl) |

---

## Integration with New Physics

### The Flow:

```
1. AI_Shooter.OnShot() ? Launches rock
   ?
2. AI_Shooter.MonitorAndSweepCoroutine() ? Monitors trajectory
   ?
3. Real-time evaluation ? Compares actual vs predicted
   ?
4. Decision logic ? Weight? Line? Curl? Whoa?
   ?
5. ApplySweepState() ? Calls SweeperManager methods
   ?
6. SweeperManager ? Calls Sweep.cs coroutines
   ?
7. Sweep.cs ? Adjusts linear/angular damping
   ?
8. Rock physics ? Rock responds to damping changes
   ?
9. Loop back to step 3 ? Continuous monitoring
```

---

## Skill-Based Behavior

### Sweeper Skill Affects Thresholds:

```csharp
float sweepSkill = GetSweeperSkill(); // 0-1 scale
float skillMultiplier = 1.0f - (0.3f * (1.0f - sweepSkill));

// Better skill = more aggressive (tighter thresholds)
float lateralThreshold = 0.12f * skillMultiplier;
float distanceThreshold = 0.25f * skillMultiplier;
```

**Examples**:

| Sweeper Skill | Lateral Threshold | Distance Threshold | Behavior |
|---------------|-------------------|-------------------|----------|
| **30%** (Poor) | 0.17m | 0.33m | Only sweeps obvious errors |
| **60%** (Average) | 0.13m | 0.27m | Sweeps moderate errors |
| **90%** (Excellent) | 0.09m | 0.19m | Sweeps tiny errors |

**Better sweepers** = More precise corrections!

---

## Configuration

### Tuning Parameters (Exposed in Inspector):

| Parameter | Default | Effect |
|-----------|---------|--------|
| `enableTeeLineSweeper` | true | Master on/off switch |
| `velocityTolerance` | 0.4 m/s | How close is "good enough" |
| `minimumUrgency` | 0.2 | Minimum urgency to act (0-1) |
| `allowOpponentInterference` | true | Enable opponent rock sweeping |
| `sweepingAggression` | 0.6 | How aggressive (0-1) |

---

### Velocity Targets:

| Shot Type | Target Velocity | Adjustable in Inspector |
|-----------|----------------|------------------------|
| Button Draw | 5.2 m/s | `buttonDrawTargetVelocity` |
| 12-Foot Draw | 4.8 m/s | `twelveFootTargetVelocity` |
| Guard | 3.7 m/s | `guardTargetVelocity` |

---

## Testing Checklist

### Test 1: YOUR Rock - Too Light ?
- [ ] AI draws to button with light weight
- [ ] Sweepers engage ("SWEEP!")
- [ ] Rock reaches target with help

### Test 2: YOUR Rock - Perfect ?
- [ ] AI draws to button with perfect weight
- [ ] Sweepers DO NOTHING
- [ ] Rock reaches target untouched

### Test 3: YOUR Rock - Too Heavy ?
- [ ] AI draws to button with heavy weight
- [ ] Sweepers call "WHOA!"
- [ ] Rock stops at target (not overshooting)

### Test 4: YOUR Rock - Wrong Curl ?
- [ ] AI draws with good weight but wrong line
- [ ] One sweeper engages (curl or line correction)
- [ ] Rock curls correctly to target

### Test 5: OPPONENT Rock - Perfect ?
- [ ] Opponent draws to button with perfect weight
- [ ] AI sweepers engage to make them overshoot
- [ ] Opponent rock goes too far!

### Test 6: OPPONENT Rock - Failing ?
- [ ] Opponent draws to button too light
- [ ] AI sweepers DO NOTHING
- [ ] Opponent rock falls short naturally

---

## Performance Notes

### Efficiency:

- **No new components** - Enhanced existing AI_Shooter
- **Real-time** - Evaluates every FixedUpdate (50 FPS)
- **Lightweight** - Simple vector math and comparisons
- **Accurate** - Uses same TrajectorySimulator as player

### Memory:

- **Minimal** - No additional allocations during gameplay
- **Clean** - Coroutine per rock, stops when rock stops
- **Scalable** - Can handle multiple rocks (though rare)

---

## Code Quality

### What Makes This Good:

1. ? **Realistic** - Matches real curling strategy
2. ? **Strategic** - Considers opponent rocks
3. ? **Adaptive** - Real-time adjustments
4. ? **Physics-aware** - Integrates with new damping model
5. ? **Skill-based** - Better sweepers make better decisions
6. ? **Maintainable** - Clear logic, well-documented
7. ? **Testable** - Easy to observe behavior
8. ? **Configurable** - Inspector-tunable parameters

---

## Build Status

? **Build Successful!**

```
Compilation completed successfully.
0 errors, 0 warnings.
AI Tee Line Sweeping enhanced and ready!
```

---

## Summary

### What We Enhanced:

**AI_Shooter.cs**:
- ? Added opponent rock detection
- ? Added opponent interference strategy
- ? Fixed curl direction logic (in-turn = LEFT)
- ? Integrated with new realistic sweeping physics

**Result**: AI now sweeps strategically!
- Helps YOUR rocks succeed
- Interferes with OPPONENT rocks
- Makes real-time adjustments
- Uses realistic physics (separate linear/angular damping)

---

## Key Features

### 1. Friend vs Foe ?
AI knows which rocks to help and which to hurt!

### 2. Real-Time Adaptation ?
Evaluates multiple times during rock's journey, adjusts strategy dynamically.

### 3. Strategic Interference ?
Sweeps opponent rocks to make them OVERSHOOT (sneaky but effective!)

### 4. Physics Integration ?
Works perfectly with new damping model (linear vs angular).

### 5. Skill-Based Behavior ?
Better sweepers make more precise corrections.

---

**The AI is now a smart, strategic sweeper!** ???

Test it and watch the AI:
- Help its own rocks reach targets
- Sabotage opponent rocks by making them overshoot
- Make nuanced line/curl corrections

**Real curling strategy in your game!** ??
