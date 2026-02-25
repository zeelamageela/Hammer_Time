# AI Tee Line Sweeper Implementation Plan ??

**Goal**: Smart AI sweeping at the tee line that:
1. Helps YOUR rocks (sweep to extend distance)
2. Hurts OPPONENT rocks (don't sweep, or call "whoa" to slow them)
3. Evaluates multiple rocks if there are choices
4. Makes strategic decisions based on game state

---

## System Architecture

### 1. AI_TeeLineSweeper Component

**Location**: `Assets/Scripts/AI/AI_TeeLineSweeper.cs`

**Responsibilities**:
- Monitor rocks crossing tee line (Y = -16.15 to Y = 5.0, approx)
- Evaluate each rock: friend or foe?
- Decide: Sweep? Whoa? Do nothing?
- Execute sweeping commands via SweeperManager

**Key Methods**:
```csharp
void Update()  // Monitor rock positions
bool ShouldSweepRock(GameObject rock, out SweepDecision decision)
void ExecuteSweepDecision(SweepDecision decision)
```

---

### 2. SweepDecision Structure

```csharp
public struct SweepDecision
{
    public enum Action
    {
        DoNothing,      // Rock is fine, leave it alone
        SweepWeight,    // Help it travel farther
        SweepLine,      // Adjust line (one sweeper)
        SweepCurl,      // Adjust curl (one sweeper)
        SweepHard,      // Maximum help (desperate)
        CallWhoa        // Opponent rock - slow it down!
    }
    
    public Action action;
    public string reason;       // For debugging
    public float urgency;       // 0-1 scale
    public GameObject rock;     // Which rock
}
```

---

### 3. Rock Evaluation Criteria

#### For YOUR Rocks:
```
Evaluate at tee line (Y = -16.15):
- Is it going to reach target?
- Is it too heavy?
- Is line/curl correct?

Decision Matrix:
1. Too light ? SWEEP WEIGHT (help it get there!)
2. Perfect weight, wrong line ? SWEEP LINE (one sweeper)
3. Perfect weight, wrong curl ? SWEEP CURL (one sweeper)
4. Way too light ? SWEEP HARD (desperate)
5. Too heavy ? CALL WHOA (slow it down)
6. Perfect ? DO NOTHING (don't mess it up!)
```

#### For OPPONENT Rocks:
```
Evaluate at tee line:
- Will it reach a good position?
- Is it a scoring threat?

Decision Matrix:
1. Good opponent rock ? CALL WHOA! (slow it down!)
2. Bad opponent rock ? DO NOTHING (let it be bad)
```

---

### 4. Strategic Multi-Rock Evaluation

**Scenario**: Multiple rocks in play, which one to focus on?

**Priority System**:
```csharp
Priority Score = BaseScore × Modifiers

Base Scores:
- Your rock going for shot stone: 100
- Your rock going for scoring position: 80
- Your rock setting up guard: 60
- Opponent rock threatening shot stone: 90
- Opponent rock in scoring position: 70
- Opponent rock as guard: 40

Modifiers:
- Close to target: ×1.5
- Far from target: ×0.7
- Last rock of end: ×2.0
- Behind in score: ×1.3
```

**Example**:
```
Rock A: Your draw to button (Base=100, Close=×1.5) = 150 priority
Rock B: Opponent guard (Base=40, Far=×0.7) = 28 priority
Rock C: Your guard (Base=60, Close=×1.5) = 90 priority

Decision: Focus on Rock A (your draw)!
```

---

### 5. Velocity Thresholds

**At Tee Line (Y = -16.15)**:

```csharp
// Button draw target velocity thresholds
const float BUTTON_DRAW_MIN_VELOCITY = 4.5f;  // Too slow
const float BUTTON_DRAW_IDEAL_VELOCITY = 5.2f; // Perfect
const float BUTTON_DRAW_MAX_VELOCITY = 5.9f;  // Too fast

// Guard target velocity thresholds
const float GUARD_MIN_VELOCITY = 3.0f;
const float GUARD_IDEAL_VELOCITY = 3.7f;
const float GUARD_MAX_VELOCITY = 4.4f;

// Tolerance bands
const float VELOCITY_TOLERANCE = 0.3f; // ±0.3 m/s is "acceptable"
```

**Decision Logic**:
```csharp
if (velocity < target_min - VELOCITY_TOLERANCE)
    return SweepWeight;  // Way too slow!
else if (velocity < target_ideal)
    return SweepWeight;  // Slightly slow, help it
else if (velocity > target_max + VELOCITY_TOLERANCE)
    return CallWhoa;     // Way too fast!
else if (velocity > target_ideal)
    return CallWhoa;     // Slightly fast, slow it
else
    return DoNothing;    // Perfect!
```

---

### 6. Integration Points

#### A. AI_Sweeper.cs
**Current**: Hardcoded shot types with fixed velocity checks
**After**: Delegate tee line decisions to AI_TeeLineSweeper

```csharp
// In AI_Sweeper.OnSweep()
if (AI_TeeLineSweeper.IsEnabled)
{
    // Let tee line sweeper handle it
    yield break;
}
else
{
    // Use legacy shot-type specific logic
    // ... existing code ...
}
```

#### B. SweeperManager.cs
**Add**: Methods for tee line sweeper to call

```csharp
public void TeeLineSweepWeight()  // Tee line calls weight sweep
public void TeeLineSweepLine()    // Tee line calls line sweep
public void TeeLineSweepCurl()    // Tee line calls curl sweep
public void TeeLineCallWhoa()     // Tee line calls whoa (opponent rock!)
```

#### C. GameManager.cs
**Track**: Which rocks are "in flight" and need monitoring

```csharp
public List<GameObject> rocksInFlight = new List<GameObject>();
```

---

### 7. Implementation Steps

**Phase 1: Basic Tee Line Monitoring** ?
1. Create AI_TeeLineSweeper component
2. Monitor rocks crossing tee line
3. Detect friend vs foe

**Phase 2: Simple Sweep Decisions** ?
1. Evaluate YOUR rocks (too fast/slow?)
2. Evaluate OPPONENT rocks (threat level?)
3. Make basic sweep/whoa decisions

**Phase 3: Strategic Multi-Rock** ?
1. Priority scoring system
2. Choose best rock to focus on
3. Handle multiple simultaneous rocks

**Phase 4: Refined Strategy** ?
1. Line/curl-specific sweeping
2. Game state awareness (score, end number)
3. Shot type awareness (draw vs guard)

---

### 8. Debug Visualization

**Console Logs**:
```
[TeeLineSweeper] Rock_05 (Red) crossing tee line
[TeeLineSweeper] Target: Button Draw (Y=6.5)
[TeeLineSweeper] Velocity: 4.8 m/s (Target: 5.2 ± 0.3)
[TeeLineSweeper] Decision: SWEEP WEIGHT (too slow by 0.4 m/s)
[TeeLineSweeper] Urgency: 0.7 (important!)
[TeeLineSweeper] Executing: sm.SweepWeight(true)
```

**On-Screen Callouts** (optional):
```
"SWEEP IT!" (for your rocks)
"WHOA!" (for opponent rocks)
```

---

### 9. Configuration Parameters

**In Inspector**:
```csharp
[Header("Tee Line Sweeper Settings")]
[Tooltip("Enable AI tee line sweeping")]
public bool enableTeeLineSweeper = true;

[Tooltip("Velocity tolerance (±m/s)")]
[Range(0.1f, 0.5f)]
public float velocityTolerance = 0.3f;

[Tooltip("Minimum urgency to act (0-1)")]
[Range(0.0f, 1.0f)]
public float minimumUrgency = 0.3f;

[Tooltip("Focus on opponent rocks?")]
public bool allowOpponentInterference = true;
```

---

### 10. Example Scenarios

#### Scenario 1: Your Draw to Button (Perfect Weight)
```
Rock crosses tee line at Y=-16.15
Velocity: 5.2 m/s
Target velocity for button: 5.2 ± 0.3 m/s
Decision: DO NOTHING (it's perfect!)
Result: Rock reaches button untouched ?
```

#### Scenario 2: Your Draw to Button (Too Light)
```
Rock crosses tee line at Y=-16.15
Velocity: 4.6 m/s
Target velocity for button: 5.2 ± 0.3 m/s
Deficit: -0.6 m/s (too slow!)
Decision: SWEEP WEIGHT (help it get there!)
Urgency: 0.8 (high - needs sweep badly)
Result: Both sweepers engage, rock reaches button ?
```

#### Scenario 3: Opponent Draw to Button (Threat!)
```
Rock crosses tee line at Y=-16.15
Velocity: 5.3 m/s
Target velocity for button: 5.2 ± 0.3 m/s
Rock owner: OPPONENT
Decision: CALL WHOA! (slow them down!)
Urgency: 0.9 (critical - deny their points!)
Result: Sweepers call "WHOA!", opponent rock stops short ?
```

#### Scenario 4: Multiple Rocks (Choose Wisely)
```
Rock A: Your draw to button (Priority: 150)
Rock B: Opponent guard (Priority: 28)
Rock C: Your guard (Priority: 90)

Decision: Focus on Rock A (highest priority)
Action: SWEEP WEIGHT on Rock A
Result: Rock A reaches button, ignore others ?
```

---

### 11. Future Enhancements

**Phase 5: Advanced Strategy**
- Sweeping to SET UP future shots
- Sweeping to DENY opponent setups
- Sweeping based on probability (risk/reward)

**Phase 6: Machine Learning** (way future!)
- Learn optimal sweep timing from human players
- Adapt strategy based on ice conditions
- Predict opponent responses

---

## Summary

### What We're Building:

**A smart tee line sweeper that**:
1. ? Monitors all rocks crossing tee line
2. ? Evaluates: Friend or foe? Too fast or slow?
3. ? Decides: Sweep? Whoa? Nothing?
4. ? Prioritizes: Which rock matters most?
5. ? Executes: Calls sweeping commands strategically

### Benefits:

- **Realistic AI** - Sweeps like a real curler would
- **Strategic Depth** - Considers game state, not just shot type
- **Adaptable** - Works with any shot the AI throws
- **Opponent Aware** - Actively works against opponent rocks
- **Configurable** - Easy to tune in Inspector

---

**Ready to implement!** Let's build this system step by step! ??
