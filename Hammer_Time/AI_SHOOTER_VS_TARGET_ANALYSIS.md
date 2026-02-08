# AI System Architecture: AI_Shooter vs AI_Target Analysis

## TL;DR: Yes, You Need Both Scripts ?

**AI_Shooter** = Places rocks using **preset positions** (guards, draws, button)
**AI_Target** = Calculates **targeting** for precision shots (takeouts, peels, taps, physics)

They serve **different, complementary purposes** - you need both!

---

## Architecture Overview

```
AI_Strategy (Brain)
    ?
    ???> Decides WHAT to do (strategy)
    ?
    ???> AI_Shooter (Preset Positions)
    ?    ??? Guards, draws to zones, button
    ?
    ???> AI_Target (Calculated Targeting)
         ??? Takeouts, peels, taps, physics-based shots
```

---

## AI_Shooter: Preset Position Executor

### Purpose
Places rocks at **predefined positions** with accuracy variation. Think of it as "shoot to a zone, not a specific rock."

### What It Handles
```csharp
// GUARDS (13 types)
"Centre Guard"
"Tight Centre Guard"  
"High Centre Guard"
"Left Corner Guard"
"Left Tight Corner Guard"
"Left High Corner Guard"
"Right Corner Guard"
"Right Tight Corner Guard"
"Right High Corner Guard"

// DRAWS (10 types)
"Top Twelve Foot"
"Left Twelve Foot"
"Right Twelve Foot"
"Back Twelve Foot"

"Top Four Foot"
"Left Four Foot"
"Right Four Foot"
"Back Four Foot"
"Button"

// LEGACY TAKEOUTS (uses AI_Target calculations)
"Take Out" - Gets takeOutX/Y from AI_Target
"Peel" - Gets takeOutX/Y from AI_Target
"Raise" - Gets takeOutX/Y from AI_Target
"Tick" - Gets takeOutX/Y from AI_Target
```

### How It Works
```csharp
// Example: Centre Guard
case "Centre Guard":
    if (inturn)
        shotX = -1f * Random.Range(centreGuard.x + guardAccu.x, centreGuard.x - guardAccu.x);
    else
        shotX = Random.Range(centreGuard.x + guardAccu.x, centreGuard.x - guardAccu.x);
    
    shotY = Random.Range(centreGuard.y + guardAccu.y, centreGuard.y - guardAccu.y);
```

**Key Point:** Uses `Vector2 centr eGuard` (preset position) + `Vector2 guardAccu` (accuracy variance)

### When AI_Strategy Calls It
```csharp
// Examples from AI_Strategy
aiShoot.OnShot("Centre Guard", rockCurrent);
aiShoot.OnShot("Left Corner Guard", rockCurrent);
aiShoot.OnShot("Right Twelve Foot", rockCurrent);
aiShoot.OnShot("Button", rockCurrent);
```

---

## AI_Target: Precision Targeting Calculator

### Purpose
**Calculates** the exact shot needed to hit a **specific target rock** using physics simulation. Think "hit THIS rock, not that zone."

### What It Handles
```csharp
// READING
"Guard Reading" - Scans guard positions

// PHYSICS-BASED TARGETING (NEW - 100% physics)
"Take Out" - Hit specific rock, remove it
"Peel" - High-speed removal (don't care about shooter)
"Tap Back" - Light contact, move rock back
"Tick Shot" - Glancing contact, move rock sideways

// AUTO DRAWS (uses physics or guard-aware logic)
"Auto Draw Four Foot" - Finds clear path to button
"Auto Draw Twelve Foot" - Finds clear path to 12-foot

// MANUAL (player-controlled)
"Manual Take Out"
"Manual Peel"
"Manual Tap Back"
"Manual Tick Shot"
"Player Draw"
"Player Guard"

// DEPRECATED (old magic numbers)
"Auto Take Out" - OLD SYSTEM, marked for removal
```

### How It Works (Physics-Based)
```csharp
IEnumerator TakeOutTarget(int rockCurrent, int rockTarget)
{
    // 1. Get target rock position
    Vector2 targetRockPos = gm.rockList[rockTarget].rock.transform.position;
    
    // 2. Use physics simulator to calculate shot
    Vector2 pullbackPos;
    bool useInTurn;
    bool foundShot = CalculatePhysicsBasedShot(
        targetRockPos, 
        out pullbackPos, 
        out useInTurn, 
        "Take Out"
    );
    
    // 3. Apply character accuracy modifier
    CharacterStats shooterStats = GetShooterStats(rockCurrent);
    float accuracy = shooterStats.takeOutAccuracy.GetValue() / 100f;
    float maxError = 0.15f * (1f - accuracy);
    pullbackPos += Random.insideUnitCircle * maxError;
    
    // 4. Pass calculated position to AI_Shooter
    rm.inturn = useInTurn;
    takeOutX = pullbackPos.x;
    takeOutY = pullbackPos.y;
    
    // 5. AI_Shooter executes the shot
    aiShoot.OnShot("Take Out", rockCurrent);
}
```

**Key Point:** **Calculates** `takeOutX/Y` dynamically, then passes to `AI_Shooter` for execution

### When AI_Strategy Calls It
```csharp
// Examples from AI_Strategy
aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
aiTarg.OnTarget("Peel", rockCurrent, cenGuard.GetComponent<Rock_Info>().rockIndex);
aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
aiTarg.OnTarget("Guard Reading", rockCurrent, 0);
```

---

## Division of Labor

### AI_Shooter Responsibilities
? **Execute shots** using preset positions or calculated targets
? **Apply accuracy variance** via `guardAccu`, `drawAccu`, `toAccu`
? **Handle in-turn/out-turn** flipping for mirrored shots
? **Set rock position** via `rockRB.position = new Vector2(shotX, shotY)`
? **Trigger rock release** via `rockFlick.mouseUp = true`
? **Call AI_Sweeper** to set up sweeping

### AI_Target Responsibilities
? **Read guard positions** (`GuardReading()`)
? **Calculate physics-based shots** using `TrajectorySimulator`
? **Find clear paths** around guards for draws
? **Evaluate shot quality** (collision detection, path scoring)
? **Apply character stats** (accuracy modifiers)
? **Set `takeOutX/Y`** for AI_Shooter to use
? **Handle target rock selection** based on strategy

---

## Call Flow Examples

### Example 1: Guard Placement
```
AI_Strategy.ConservativeSteal()
    ?
    ??> aiShoot.OnShot("Centre Guard", rockCurrent)
            ?
            ??> AI_Shooter.Shot("Centre Guard")
                    ?
                    ??> Uses preset: Vector2 centreGuard
                    ??> Adds accuracy: guardAccu
                    ??> Calls: aiSweep.OnSweep()
                    ??> Executes: rockRB.position = (shotX, shotY)
```

**No AI_Target involved** - just preset positions!

### Example 2: Physics-Based Takeout
```
AI_Strategy.ConservativeSteal()
    ?
    ??> aiTarg.OnTarget("Take Out", rockCurrent, targetRockIndex)
            ?
            ??> AI_Target.TakeOutTarget()
                    ?
                    ??> Gets target position
                    ??> CalculatePhysicsBasedShot()
                    ?   ??> TrajectorySimulator.CalculateVelocityToTarget()
                    ?   ??> Simulate trajectory with obstacles
                    ?   ??> Score paths (in-turn vs out-turn)
                    ?   ??> Return best pullback position
                    ??> Apply accuracy modifier (character stats)
                    ??> Set takeOutX/Y
                    ?
                    ??> aiShoot.OnShot("Take Out", rockCurrent)
                            ?
                            ??> AI_Shooter.Shot("Take Out")
                                    ?
                                    ??> Uses calculated: takeOutX, takeOutY
                                    ??> Adds variance: toAccu
                                    ??> Executes shot
```

**Both involved** - AI_Target calculates, AI_Shooter executes!

### Example 3: Auto Draw Four Foot
```
AI_Strategy.AggressiveHammer()
    ?
    ??> aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0)
            ?
            ??> AI_Target.DrawFourFoot()
                    ?
                    ??> GuardReading() - check guards
                    ??> Analyze guard positions
                    ??> Decide best path (left/right/center)
                    ?
                    ??> aiShoot.OnShot("Top Four Foot", rockCurrent)
                            ?                 OR
                            ?      aiShoot.OnShot("Left Four Foot", rockCurrent)
                            ?                 OR
                            ?      aiShoot.OnShot("Button", rockCurrent)
                            ?
                            ??> AI_Shooter.Shot()
                                    ?
                                    ??> Uses preset positions
```

**Both involved** - AI_Target chooses path, AI_Shooter executes!

---

## Why You Need Both

### Scenario 1: "Place a center guard"
- **AI_Shooter**: Has preset `Vector2 centreGuard = (0.02f, 2.5f)`
- **AI_Target**: Not needed - just put rock at that spot!

**Result:** `aiShoot.OnShot("Centre Guard")` ?

### Scenario 2: "Take out the 2nd opponent rock"
- **AI_Shooter**: Doesn't know WHERE that rock is
- **AI_Target**: Finds rock, calculates physics shot, sets `takeOutX/Y`
- **AI_Shooter**: Executes using calculated position

**Result:** `aiTarg.OnTarget("Take Out", rockIndex)` ? calls `aiShoot.OnShot("Take Out")` ?

### Scenario 3: "Draw to button, but avoid guards"
- **AI_Shooter**: Has `button` position, but can't path-find
- **AI_Target**: Scans guards, picks best path (left/right/top)
- **AI_Shooter**: Executes chosen preset position

**Result:** `aiTarg.OnTarget("Auto Draw Four Foot")` ? analyzes ? calls `aiShoot.OnShot("Button")` ?

---

## Usage Statistics (from AI_Strategy)

### AI_Shooter Direct Calls: **~80 times**
```csharp
aiShoot.OnShot("Centre Guard", rockCurrent);           // ×20
aiShoot.OnShot("Left Corner Guard", rockCurrent);      // ×15
aiShoot.OnShot("Right Corner Guard", rockCurrent);     // ×15
aiShoot.OnShot("Left Twelve Foot", rockCurrent);       // ×10
aiShoot.OnShot("Right Twelve Foot", rockCurrent);      // ×10
aiShoot.OnShot("High Centre Guard", rockCurrent);      // ×5
aiShoot.OnShot("Tight Centre Guard", rockCurrent);     // ×5
// ... etc
```

### AI_Target Direct Calls: **~120 times**
```csharp
aiTarg.OnTarget("Take Out", rockCurrent, rockIndex);   // ×40
aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);// ×30
aiTarg.OnTarget("Peel", rockCurrent, guardIndex);      // ×20
aiTarg.OnTarget("Tap Back", rockCurrent, rockIndex);   // ×15
aiTarg.OnTarget("Tick Shot", rockCurrent, guardIndex); // ×10
aiTarg.OnTarget("Guard Reading", rockCurrent, 0);      // ×4 (once per strategy)
// ... etc
```

**Total:** ~200 shot decisions across all 4 strategies

---

## Data Flow

### Shared Variables (AI_Shooter reads, AI_Target writes)
```csharp
// AI_Target calculates these:
public float takeOutX;  // X position for precision shots
public float takeOutY;  // Y position for precision shots
public Vector2 targetPos; // General target position

// AI_Shooter reads these:
takeOutX = aiTarg.takeOutX;  // Line 92 in AI_Shooter
takeOutY = aiTarg.takeOutY;

// RockManager controls turn direction (both read):
rm.inturn
```

### Communication Pattern
```
AI_Target:
    1. Receives: Target rock index
    2. Calculates: Physics shot
    3. Sets: takeOutX, takeOutY, targetPos
    4. Calls: aiShoot.OnShot("Take Out", rockCurrent)

AI_Shooter:
    1. Receives: Shot type ("Take Out")
    2. Reads: takeOutX, takeOutY from AI_Target
    3. Applies: Accuracy variance
    4. Executes: Rock positioning
```

---

## Redundancy Analysis

### Could You Merge Them?

**Technically yes, but it would be a MESS:**

```csharp
// Current (clean separation):
aiShoot.OnShot("Centre Guard", rockCurrent);              // Simple!
aiTarg.OnTarget("Take Out", rockCurrent, targetIndex);    // Clear!

// If merged (nightmare):
aiShooter.OnShot("Centre Guard", rockCurrent, 0, false);            // What's the 0?
aiShooter.OnShot("Take Out", rockCurrent, targetIndex, true);       // What's true?
aiShooter.OnShot("Auto Draw", rockCurrent, 0, false, true, "path"); // ???
```

### Why Separation Is Good

**Single Responsibility Principle:**
- `AI_Shooter` = **Execution engine** (how to place rocks)
- `AI_Target` = **Calculation engine** (where to aim)

**Testability:**
- Test preset positions independently of physics
- Test physics calculations without rock placement

**Maintainability:**
- Change physics without touching preset positions
- Update accuracy system in one place
- Add new preset positions easily

**Performance:**
- Preset shots are fast (no calculations)
- Physics shots are slow (simulations)
- Can choose based on situation

---

## What Could Be Improved?

### 1. Remove Duplicate Code in AI_Shooter
**Issue:** `Shot()` and `TargetShot()` are nearly identical (~95% same code)

**Current:**
```csharp
IEnumerator Shot(string aiShotType, bool inturn) { /* 500 lines */ }
IEnumerator TargetShot(string aiShotType, bool inturn) { /* 500 lines, almost identical */ }
```

**Should Be:**
```csharp
IEnumerator Shot(string aiShotType, bool inturn) 
{ 
    // Single implementation, ~500 lines
}
// Delete TargetShot() entirely
```

### 2. Clarify Naming
**Current:**
- `AI_Target.OnTarget("Auto Draw Four Foot")` - Doesn't target anything!
- `AI_Shooter.OnShot("Take Out")` - Doesn't shoot, just positions!

**Better:**
```csharp
// Calculation layer
AI_Targeting.CalculateShot(shotType, targetRock);

// Execution layer
AI_Executor.PlaceRock(position, accuracy);
```

### 3. Extract Shot Library
**All preset positions could be data:**
```csharp
public class ShotLibrary
{
    public static readonly Vector2 CENTRE_GUARD = new Vector2(0.02f, 2.5f);
    public static readonly Vector2 LEFT_CORNER_GUARD = new Vector2(-1.2f, 3.0f);
    // ... etc
}
```

---

## Conclusion

### ? **YES, You Need Both Scripts**

| Script | Purpose | Can't Be Removed Because... |
|--------|---------|----------------------------|
| **AI_Shooter** | Rock placement execution | Contains all preset positions, accuracy system, rock physics trigger |
| **AI_Target** | Shot calculation & pathfinding | Contains physics simulator, guard analysis, trajectory evaluation |

### Relationship
```
AI_Strategy (decides WHAT)
    ?
    ??> AI_Target (calculates WHERE)
    ?       ?
    ?       ??> Calls AI_Shooter
    ?
    ??> AI_Shooter (executes HOW)
```

### Recommendation
**Keep both, but:**
1. ? Delete `TargetShot()` duplicate method in AI_Shooter
2. ? Use only `Shot()` method (it already handles everything)
3. ? Document which shots use physics vs presets
4. ? Consider renaming to `AI_Executor` and `AI_Calculator` for clarity

---

## Quick Reference: Which Script For What?

### Use AI_Shooter Directly When:
- Placing guards at preset positions
- Drawing to preset zones (12-foot, 4-foot, button)
- No specific rock needs to be targeted
- Simple position + accuracy variance is enough

### Use AI_Target ? AI_Shooter When:
- Targeting specific rocks (takeouts, peels, taps)
- Need physics simulation for accuracy
- Must path around guards
- Evaluating shot quality matters

### Both Work Together When:
- Auto draws (Target picks path, Shooter executes)
- Guard reading (Target scans, Shooter uses info)
- Physics shots (Target calculates, Shooter places)

**Your current architecture is sound!** Just needs minor cleanup (duplicate method removal).
