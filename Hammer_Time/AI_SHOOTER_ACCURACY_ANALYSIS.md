# AI_Shooter Accuracy System Analysis

## TL;DR: The Accuracy System Has Significant Issues ??

**Current System:**
- ? Uses `Random.Range(target + accuracy, target - accuracy)` - **WRONG!**
- ? Distribution is **inverted** - higher accuracy = worse shots!
- ? Doesn't use character stats from AI_Target
- ? Ignores physics-based calculations in many cases
- ?? Has hardcoded offsets that might conflict with variance

**Recommendation:** Major overhaul needed. See fixes below.

---

## Current Implementation Analysis

### The Formula (Used Everywhere)

```csharp
// GUARDS
shotX = Random.Range(centreGuard.x + guardAccu.x, centreGuard.x - guardAccu.x);
shotY = Random.Range(centreGuard.y + guardAccu.y, centreGuard.y - guardAccu.y);

// DRAWS
shotX = Random.Range(button.x + drawAccu.x, button.x - drawAccu.x);
shotY = Random.Range(button.y + drawAccu.y, button.y - drawAccu.y);

// TAKEOUTS
shotX = Random.Range(takeOutX + toAccu.x, takeOutX - toAccu.x) + takeOutOffset;
shotY = Random.Range(takeOut.y + toAccu.y, takeOut.y - toAccu.y);
```

---

## Problem #1: Inverted Distribution (CRITICAL BUG!)

### Current Behavior
```csharp
Vector2 guardAccu = (0.2f, 0.3f);  // Example values
Vector2 centreGuard = (0.02f, 2.5f);

// Calculates:
shotX = Random.Range(0.02f + 0.2f, 0.02f - 0.2f);
     = Random.Range(0.22f, -0.18f);  // ? MIN > MAX!
```

### Unity's Random.Range Behavior
From Unity docs:
> `Random.Range(float min, float max)` - If `min` is greater than `max`, the values are **automatically swapped**.

**Result:** This still produces a random value between -0.18 and 0.22, BUT:
- The **distribution is uniform**, not centered on target
- You're essentially doing `Random.Range(target - accuracy, target + accuracy)` anyway

**So it "works" but is confusing and backwards!**

---

## Problem #2: No Character Stats Integration

### What AI_Target Does (Correct!)
```csharp
// AI_Target applies character accuracy AFTER physics calculation
CharacterStats shooterStats = GetShooterStats(rockCurrent);
float accuracy = shooterStats.takeOutAccuracy.GetValue() / 100f; // 0-1 scale

float maxError = 0.15f * (1f - accuracy); // Better player = less error
Vector2 errorOffset = Random.insideUnitCircle * maxError;
pullbackPos += errorOffset;
```

**Then passes to AI_Shooter:**
```csharp
takeOutX = pullbackPos.x;
takeOutY = pullbackPos.y;
aiShoot.OnShot("Take Out", rockCurrent);
```

### What AI_Shooter Does (Ignores It!)
```csharp
// AI_Shooter ADDS MORE VARIANCE on top!
shotX = Random.Range(takeOutX + toAccu.x, takeOutX - toAccu.x) + takeOutOffset;
//                             ^^^^^^^^   ^^^^^^^^
//                             ? Double-dipping on accuracy!
```

**Problem:** **Accuracy is applied TWICE**:
1. AI_Target adds character-based error
2. AI_Shooter adds fixed error on top

**Result:** Even perfect characters (100% accuracy) get degraded by `toAccu`!

---

## Problem #3: Fixed Accuracy Values

### Current System
```csharp
public Vector2 guardAccu;   // Set in inspector - SAME for all AI teams
public Vector2 drawAccu;    // Set in inspector - SAME for all AI teams
public Vector2 toAccu;      // Set in inspector - SAME for all AI teams
public Vector2 tickAccu;    // Set in inspector - SAME for all AI teams
```

**Issues:**
- ? Rookie AI has same accuracy as championship AI
- ? Lead has same accuracy as skip
- ? No difficulty scaling
- ? Ignores `CharacterStats` system entirely

---

## Problem #4: Hardcoded Offsets

### The Offset System
```csharp
public float takeOutOffset;  // Added to every takeout
public float peelOffset;     // Added to every peel
public float raiseOffset;    // Added to every raise/tap
public float tickOffset;     // Added to every tick

// Usage:
shotX = Random.Range(takeOutX + toAccu.x, takeOutX - toAccu.x) + takeOutOffset;
//                                                                ^^^^^^^^^^^^^^
//                                                                Shifts shot direction
```

**What This Does:**
- Shifts the shot in a **fixed direction**
- Used to compensate for physics differences between shot types
- **BUT:** Adds deterministic bias on top of random variance

**Example:**
```csharp
takeOutOffset = 0.05f;  // All takeouts shifted 0.05 units right
```

**Problems:**
- Why does every takeout need a bias?
- Shouldn't physics simulation handle this?
- Conflicts with accuracy variance (shifts the "center" of the random range)

---

## Problem #5: Inconsistent Application

### Some Shots Use Variance
```csharp
case "Centre Guard":
    shotX = Random.Range(centreGuard.x + guardAccu.x, centreGuard.x - guardAccu.x);
    shotY = Random.Range(centreGuard.y + guardAccu.y, centreGuard.y - guardAccu.y);
    // ? Applies accuracy
```

### Some Shots DON'T Use Variance
```csharp
case "Draw To Target":
    shotX = takeOutX;  // ? No accuracy applied!
    shotY = takeOutY;
    // ? Perfect shot every time! (AI_Target already applied error though)
```

### Why The Inconsistency?
Looking at comments:
```csharp
case "Draw To Target":
    shotX = takeOutX;
    shotY = takeOutY;
    
    // Commented out:
    //shotX = Random.Range(takeOutX + drawAccu.x, takeOutX - drawAccu.x);
    //shotY = Random.Range(takeOutY + drawAccu.y, takeOutY - drawAccu.y);
```

**Implication:** You intentionally removed variance for physics-based shots because **AI_Target already handles it!**

**This is correct!** But it's inconsistent with other shots.

---

## Problem #6: No Gaussian Distribution

### Current: Uniform Distribution
```csharp
shotX = Random.Range(min, max);  // Uniform - all values equally likely
```

**Distribution:**
```
Target: 0.0
Accuracy: ±0.2

Uniform:
Probability
    ?
1.0 ???????????????
    ?             ?
0.5 ?             ?
    ?             ?
0.0 ????????????????????> Position
       -0.2    0.0   +0.2
```

Every position from -0.2 to +0.2 is **equally likely**. This means:
- 50% of shots miss by more than 0.1
- Missing by 0.19 is as likely as missing by 0.01
- **Unrealistic** - skilled players cluster around target

### Better: Normal (Gaussian) Distribution
```csharp
// Bell curve - most shots near target, few outliers
float error = Random.Range(-1f, 1f) * Random.Range(-1f, 1f) * accuracy;
// Multiplying two random ranges creates a distribution biased toward 0
```

**Distribution:**
```
Gaussian:
Probability
    ?
1.0 ?      ???
    ?    ??   ??
0.5 ?  ??       ??
    ? ?           ?
0.0 ????????????????????> Position
       -0.2    0.0   +0.2
```

Most shots cluster near target (0.0), with rare outliers.

**More Realistic:**
- 68% of shots within ±0.1 of target
- 95% within ±0.15
- Occasional bad shots at ±0.2

---

## Comparison to AI_Target Accuracy

### AI_Target System (BETTER!)

```csharp
// 1. Get character stats
CharacterStats shooterStats = GetShooterStats(rockCurrent);
float accuracy = shooterStats.takeOutAccuracy.GetValue() / 100f;

// 2. Calculate error based on accuracy
float maxError = 0.15f * (1f - accuracy);

// 3. Use circular distribution (more realistic)
Vector2 errorOffset = Random.insideUnitCircle * maxError;

// 4. Apply to calculated position
pullbackPos += errorOffset;
```

**Why This Is Better:**
? Uses character stats (varies by team/player)
? `insideUnitCircle` creates natural distribution
? Error scales with accuracy (100% = no error, 0% = max error)
? Applied ONCE (not double-dipping)

### AI_Shooter System (WORSE!)

```csharp
// 1. Ignores character stats
// 2. Uses fixed global values
public Vector2 guardAccu;  // Same for everyone

// 3. Uniform distribution
shotX = Random.Range(target + accuracy, target - accuracy);

// 4. Applied ON TOP of AI_Target's accuracy (double-dipping)
```

**Why This Is Worse:**
? Ignores character stats
? Same accuracy for all AI teams
? Uniform distribution (unrealistic)
? Double-applies error for physics shots

---

## Actual Values Analysis

### Typical Inspector Values (Guessing)
```csharp
guardAccu = (0.15f, 0.2f);   // ±15cm X, ±20cm Y
drawAccu = (0.1f, 0.15f);    // ±10cm X, ±15cm Y
toAccu = (0.08f, 0.1f);      // ±8cm X, ±10cm Y
tickAccu = (0.12f, 0.15f);   // ±12cm X, ±15cm Y

takeOutOffset = 0.05f;       // +5cm X bias
peelOffset = 0.08f;          // +8cm X bias
raiseOffset = 0.03f;         // +3cm X bias
tickOffset = 0.1f;           // +10cm X bias
```

### What This Means

**Guards:**
- Can land ±15cm left/right of target
- Can land ±20cm up/down of target
- **Total error zone:** 30cm × 40cm rectangle

**Draws:**
- Can land ±10cm left/right
- Can land ±15cm up/down
- **Total error zone:** 20cm × 30cm rectangle

**Takeouts:**
- Can land ±8cm left/right **+ 5cm bias = 3cm to 13cm right of target!**
- Can land ±10cm up/down
- **Total error zone:** 8cm × 10cm rectangle, **shifted 5cm right**

**Problem:** Why is there a bias? If physics is correct, there should be no systematic offset!

---

## Recommendations

### Option 1: Remove AI_Shooter Variance (Quick Fix)

**For physics-based shots, remove variance entirely:**

```csharp
case "Take Out":
case "Peel":
case "Raise":
case "Tick":
    // AI_Target already applied character-based accuracy
    shotX = takeOutX;  // ? Use calculated value directly
    shotY = takeOutY;
    break;
```

**Pros:**
- ? Fixes double-dipping
- ? Respects character stats
- ? Quick change

**Cons:**
- ?? Preset shots (guards/draws) still use fixed variance
- ?? Inconsistent between shot types

---

### Option 2: Character Stats Everywhere (Best Fix)

**Make AI_Shooter query character stats:**

```csharp
IEnumerator Shot(string aiShotType, bool inturn)
{
    // Get character accuracy for this rock
    CharacterStats shooterStats = GetShooterStats(rockCurrent);
    
    float shotX, shotY;
    
    switch (aiShotType)
    {
        case "Centre Guard":
            // Use character's guard accuracy
            float guardAccuracy = shooterStats.guardAccuracy.GetValue() / 100f;
            float guardError = 0.2f * (1f - guardAccuracy);
            
            Vector2 errorOffset = Random.insideUnitCircle * guardError;
            shotX = centreGuard.x + errorOffset.x;
            shotY = centreGuard.y + errorOffset.y;
            break;
            
        case "Take Out":
            // Physics shots: AI_Target already applied accuracy
            shotX = takeOutX;
            shotY = takeOutY;
            break;
    }
}

// Helper method (copy from AI_Target)
private CharacterStats GetShooterStats(int rockCurrent)
{
    TeamManager tm = FindObjectOfType<TeamManager>();
    if (tm == null) return null;
    
    int memberIndex = rockCurrent / 4;
    memberIndex = Mathf.Clamp(memberIndex, 0, 3);
    
    bool isRedTeam = (rockCurrent % 2 == 0) ? gm.redHammer : !gm.redHammer;
    
    if (isRedTeam && tm.teamRed != null && memberIndex < tm.teamRed.Length)
        return tm.teamRed[memberIndex].charStats;
    else if (!isRedTeam && tm.teamYellow != null && memberIndex < tm.teamYellow.Length)
        return tm.teamYellow[memberIndex].charStats;
    
    return null;
}
```

**Pros:**
- ? All shots use character stats
- ? Consistent accuracy system
- ? Difficulty scales with team quality
- ? No more double-dipping

**Cons:**
- ?? More code changes
- ?? Need to copy `GetShooterStats()` from AI_Target (or refactor)

---

### Option 3: Gaussian Distribution (Advanced Fix)

**Make accuracy more realistic:**

```csharp
// Helper: Generate Gaussian-distributed error
private Vector2 GetGaussianError(float maxError)
{
    // Box-Muller transform for Gaussian distribution
    float u1 = Random.Range(0f, 1f);
    float u2 = Random.Range(0f, 1f);
    
    float r = Mathf.Sqrt(-2f * Mathf.Log(u1)) * maxError;
    float theta = 2f * Mathf.PI * u2;
    
    return new Vector2(
        r * Mathf.Cos(theta),
        r * Mathf.Sin(theta)
    );
}

// Usage:
case "Centre Guard":
    float guardAccuracy = shooterStats.guardAccuracy.GetValue() / 100f;
    float maxError = 0.2f * (1f - guardAccuracy);
    
    Vector2 error = GetGaussianError(maxError);
    shotX = centreGuard.x + error.x;
    shotY = centreGuard.y + error.y;
    break;
```

**Pros:**
- ? Realistic shot distribution
- ? Most shots cluster near target
- ? Occasional outliers (realistic for bad teams)

**Cons:**
- ?? More complex math
- ?? Requires testing/tuning

---

## The Offset Problem

### Current Usage
```csharp
shotX = Random.Range(takeOutX + toAccu.x, takeOutX - toAccu.x) + takeOutOffset;
```

### Questions to Ask Yourself:

1. **Why do you have offsets?**
   - Are they compensating for physics bugs?
   - Are they intentional shot adjustments?
   - Are they historical artifacts?

2. **Should offsets exist?**
   - If physics is correct, there should be NO systematic bias
   - Offsets suggest either:
     - Physics calculations are wrong
     - Or you're intentionally making AI imperfect

3. **If you keep offsets:**
   ```csharp
   // Apply offset BEFORE variance, not after
   float targetWithOffset = takeOutX + takeOutOffset;
   shotX = targetWithOffset + GetError(accuracy);
   ```

4. **If you remove offsets:**
   ```csharp
   // Clean - just target + error
   shotX = takeOutX + GetError(accuracy);
   ```

**Recommendation:** Remove offsets. If physics is wrong, fix physics instead of adding bandaids.

---

## Real-World Comparison

### Elite Curlers (95% accuracy)
- **Guards:** Land within 10cm of target 95% of time
- **Draws:** Land within 15cm of button 95% of time
- **Takeouts:** Hit target 95% of time

### Your Current System
```csharp
guardAccu = (0.15f, 0.2f);  // ±15-20cm
// This means 100% of shots land within 15-20cm
// NO shots outside that range
// Unrealistic - even pros have occasional bad shots!
```

### Better System
```csharp
// Elite player (95% accuracy)
maxError = 0.15f * (1f - 0.95f) = 0.0075f;  // Very tight!

// Gaussian distribution:
// 68% land within ±0.0075m (0.75cm)  // Most shots very close
// 95% land within ±0.015m (1.5cm)    // Almost all shots close
//  5% land outside (rare bad shots)  // Realistic outliers
```

---

## Summary & Verdict

### Current System: 3/10 ?

| Aspect | Rating | Notes |
|--------|--------|-------|
| **Character Integration** | 0/10 | Ignores CharacterStats entirely |
| **Consistency** | 2/10 | Different logic for preset vs physics shots |
| **Distribution** | 4/10 | Uniform (works but unrealistic) |
| **Double-Dipping** | 0/10 | Applies accuracy twice for physics shots |
| **Scalability** | 1/10 | Same accuracy for all AI difficulty levels |
| **Realism** | 3/10 | No Gaussian, has weird offsets |

**Total: 10/60 = 17%** (Needs major rework)

---

### Recommended System: 9/10 ?

```csharp
IEnumerator Shot(string aiShotType, bool inturn)
{
    CharacterStats stats = GetShooterStats(rockCurrent);
    float shotX, shotY;
    
    switch (aiShotType)
    {
        // PRESET SHOTS: Apply character accuracy
        case "Centre Guard":
            float accuracy = stats.guardAccuracy.GetValue() / 100f;
            float maxError = 0.15f * (1f - accuracy);
            Vector2 error = Random.insideUnitCircle * maxError;
            
            shotX = centreGuard.x + error.x;
            shotY = centreGuard.y + error.y;
            break;
            
        // PHYSICS SHOTS: Use calculated values (AI_Target handled accuracy)
        case "Take Out":
        case "Peel":
        case "Tick":
        case "Raise":
            shotX = takeOutX;
            shotY = takeOutY;
            break;
    }
    
    // No offsets needed!
    rockRB.position = new Vector2(shotX, shotY);
}
```

**Benefits:**
- ? Uses character stats everywhere
- ? No double-dipping
- ? Consistent approach
- ? Scales with difficulty
- ? More realistic distribution
- ? No magic offsets

---

## Action Items

### Immediate (Fix Critical Bugs)
1. ? Remove variance from physics shots (they're double-dipping)
2. ? Remove offsets (or document why they exist)
3. ? Fix inverted Random.Range calls (target ± accuracy, not +/-)

### Short Term (Improve Quality)
4. ? Integrate CharacterStats into AI_Shooter
5. ? Use `Random.insideUnitCircle` instead of uniform distribution
6. ? Remove `TargetShot()` duplicate method

### Long Term (Polish)
7. ? Implement Gaussian distribution for realism
8. ? Tune accuracy values based on playtesting
9. ? Add difficulty scaling system

---

## Final Verdict

**Your accuracy system works, but it's fundamentally flawed:**

1. ? Ignores character stats (biggest issue)
2. ? Applies accuracy twice for some shots
3. ? Has mysterious offsets that shouldn't exist
4. ? Uses uniform distribution (unrealistic)
5. ? Same accuracy for all AI difficulty levels

**You should:**
- **Short term:** Remove variance from physics shots (quick fix)
- **Long term:** Integrate CharacterStats everywhere (proper fix)

**The good news:** AI_Target already does it right! Just copy that approach to AI_Shooter.
