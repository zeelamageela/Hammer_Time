# RandomRockPlacement Strategy Analysis

## ?? Purpose & Overview

**What this file does:** Simulates rock placements for the "mixed" end setup feature - pre-placing rocks on the sheet before the end starts to create interesting tactical situations.

**Used in:** Tutorial/training scenarios, potentially cash games or special game modes where you want a pre-configured rock setup.

---

## ?? Strategy System Architecture

### Scenario-Based Decision Making

The system uses **12 scenarios** based on:
1. **Game phase** (Early/Mid/Penultimate/Last end)
2. **Score situation** (Tied/Losing/Winning)

```
Scenario Matrix:
????????????????????????????????????????????????
? Game Phase  ?   Tied   ?  Losing  ?  Winning ?
????????????????????????????????????????????????
? Early (0-2) ? Case 0   ? Case 1   ? Case 2   ?
? Mid (3-5)   ? Case 3   ? Case 4   ? Case 5   ?
? Penult (6)  ? Case 6   ? Case 7   ? Case 8   ?
? Last (7+)   ? Case 9   ? Case 10  ? Case 11  ?
????????????????????????????????????????????????
```

---

## ?? Shot Selection System

### Shot Types (shotSelector values)

| Value | Shot Type | Description |
|-------|-----------|-------------|
| **0** | Draw Random | Draw to house with accuracy variance |
| **1** | Out | Rock goes out of play (miss) |
| **2** | Draw Four Foot | Precise draw to 4-foot circle |
| **3** | Auto Guard | Place a guard (left/center/right) |
| **4** | Takeout | Remove opponent rock |
| **5** | Freeze | Freeze behind opponent rock |
| **6** | Manual Guard | Player-selected guard position |
| **7** | Mixed Setup | Special pre-configured setup |
| **99** | Crash/Error | Takeout missed badly |

---

## ?? Strategy Implementation Analysis

### ? **Fully Implemented Scenarios** (3/12)

#### Case 6: Penultimate End - Tied
**Strategy:** Force opponent to score 1 (without hammer) or blank (with hammer)

**Logic:**
```csharp
// Early rocks (0-4): Clear house, build guards
if (rockCurrent < 5)
{
    if (house has opponent rocks)
        ? Try takeout
    else
        ? Place guard if house > 2 rocks
        ? Otherwise draw
}

// Late rocks (14+): Aggressive clearing
else if (rockCurrent > 13)
{
    if (house has opponent rocks)
        ? Takeout
    else
        ? Draw
}

// Mid rocks (5-13): Complex decision tree
else
{
    if (house has opponent rocks)
        ? Takeout unguarded rocks
    else if (guards exist)
        ? Remove opponent guards
    else
        ? Draw or guard based on house count
}
```

**Quality:** ? **9/10** - Well-structured, handles most situations

---

#### Case 7: Penultimate End - Losing
**Strategy:** Steal 1 (without hammer) or score 2+ (with hammer)

**Key Features:**
- **Freeze shots** when opponent rock is behind tee line
- Aggressive guard removal
- Strategic positioning for multi-rock scoring

**Logic:**
```csharp
// Uses freeze for strategic positioning
if (opponent rock behind tee && hammer last rock)
{
    shotSelector = SkillCheck("Freeze", accuracy);
}
else
{
    shotSelector = SkillCheck("Takeout", accuracy);
}
```

**Quality:** ? **8/10** - Good use of freeze shots, proper aggression

---

#### Case 8: Penultimate End - Winning
**Strategy:** Force blank (with hammer) or force 1 (without hammer)

**Conservative approach:**
- Clear all opponent rocks
- Don't leave scoring opportunities
- Remove guards to prevent steals

**Quality:** ? **8/10** - Appropriately conservative

---

### ? **NOT Implemented Scenarios** (9/12)

These cases have **NO STRATEGY CODE** inside:

#### Case 0: Early Game - Tied
```csharp
case 0:
    #region Early Game - Tied
    Debug.Log("Early Game - Tied");
    #endregion
    break;  // ? EMPTY!
```

#### Case 1: Early Game - Losing
```csharp
case 1:
    #region Early Game - Losing
    Debug.Log("Early Game - Losing");
    #endregion
    break;  // ? EMPTY!
```

#### Cases 2-5, 9-11: Also Empty!

**Impact:** These scenarios fall through to the **fallback AI system** (line 1283):

```csharp
// Fallback: Use simple AI evaluation
if (gsp.aiRed && redTeam || gsp.aiYellow && !redTeam)
{
    shotSelector = EvaluateBestAIShot(isBehind, rocksInHouse, guardsInPlay, aiSkill);
}
```

---

## ?? Fallback AI System

### `EvaluateBestAIShot()` Method

**Simple rule-based logic:**

```csharp
private int EvaluateBestAIShot(bool isBehind, int rocksInHouse, int guardsInPlay, int aiSkill)
{
    // If behind and rocks in house and skilled ? Takeout
    if (isBehind && rocksInHouse > 0 && aiSkill > 7)
        return 4;
    
    // If ahead and few guards and skilled ? Guard
    if (!isBehind && guardsInPlay < 2 && aiSkill > 5)
        return 3;
    
    // If house empty ? Draw
    if (rocksInHouse == 0)
        return 0;
    
    // Fallback: Random draw or guard
    return (Random.value < 0.5f) ? 0 : 3;
}
```

**Issues:**
1. ? **aiSkill is hardcoded to 8** (line 1277) - ignores character stats!
2. ? **Doesn't use `CharacterStats`** that were just retrieved
3. ? **Too simplistic** - doesn't consider game phase or strategy depth
4. ? **Random 50/50 fallback** - not strategic

---

## ?? Shot Execution System

### `SkillCheck()` Method

**Purpose:** Determines if AI successfully executes a shot based on character accuracy.

**Implementation:**

```csharp
int SkillCheck(string shot, int skill)
{
    switch (shot)
    {
        case "Guard":
            if (Random.Range(0f, 100f) <= skill)
                return 3;  // Success ? Place guard
            else
                return Random.value < 0.5f ? 0 : 1;  // Fail ? Draw or Out
        
        case "Draw":
            if (Random.Range(0f, 100f) <= skill)
                return 0;  // Success ? Draw to house
            else
                return Random.value < 0.5f ? 3 : 1;  // Fail ? Guard or Out
        
        case "Takeout":
            if (Random.Range(0f, 100f) <= skill)
                return 4;  // Success ? Execute takeout
            else
                return Random.value < 0.25f ? 99 : 1;  // Fail ? Crash or Out
        
        case "Freeze":
            if (Random.Range(0f, 100f) <= skill)
                return 5;  // Success ? Freeze
            else
                return Random.value < 0.5f ? 3 : 1;  // Fail ? Guard or Out
    }
}
```

**Quality:** ? **7/10**
- Uses character stats correctly
- Realistic failure modes (guards can be short/long)
- Takeouts have 25% crash rate on failure (realistic)

---

## ?? Shot Placement System

### `ShotSelector()` Method

**Executes the chosen shot** by calculating rock positions.

#### Case 0: Draw Random
```csharp
rockPos[rockCurrent] = placePos[9]  // Button position
    + (Random.insideUnitCircle 
    * (1.5f - (0.01f * activeCharStats.drawAccuracy.GetValue())));
```

**Quality:** ? Uses character stats, circular distribution

#### Case 3: Auto Guard
```csharp
// Left/Center/Right guard based on rock parity
if (rockCurrent % 2 == 1)
    guardSelect = Random.value < 0.5f ? 1 : 3;  // Left or Right
else
    guardSelect = 2;  // Center

// Add accuracy-based variance
rockPos[rockCurrent] = placePos[guardSelect]
    + (Random.insideUnitCircle 
    * Random.Range(0f, 1.5f - (0.01f * activeCharStats.guardAccuracy.GetValue())));
```

**Quality:** ? **8/10** - Good variance, uses stats

#### Case 4: Takeout
```csharp
// Shooter rock position (hit and roll)
if (Random.Range(0f, 100f) < activeCharStats.takeOutAccuracy.GetValue())
{
    rockPos[rockCurrent] = rockPos[takeOutSelector]
        + (Random.insideUnitCircle * (1.5f - (0.005f * accuracy)));
}

// Target rock (did it go out?)
if (Random.Range(0f, 100f) < activeCharStats.takeOutAccuracy.GetValue())
{
    rockPos[takeOutSelector] = placePos[10];  // Out of play
}
else
{
    rockPos[takeOutSelector] += Random.insideUnitCircle * variance;  // Moved but in play
}
```

**Quality:** ? **9/10**
- Two separate accuracy checks (realistic!)
- Can hit but not remove (realistic!)
- Uses character stats properly

#### Case 5: Freeze
```csharp
// Position behind target rock
rockPos[rockCurrent].y = rockPos[takeOutSelector].y - 0.25f;
rockPos[rockCurrent].x = rockPos[takeOutSelector].x;

// Add accuracy variance
if (successful)
    rockPos[rockCurrent] += Random.insideUnitCircle * (0.5f - 0.005f * accuracy);
else
    rockPos[rockCurrent] += Random.insideUnitCircle * (2f - 0.01f * accuracy);

// Did we move the target rock?
if (successful)
    rockPos[takeOutSelector].y += 0.5f;  // Slight movement
else
    rockPos[takeOutSelector].y += 1.5f;  // Big movement
```

**Quality:** ? **8/10** - Realistic freeze mechanics

---

## ?? Issues & Problems

### 1. **Empty Strategy Cases** (CRITICAL)

**9 out of 12 scenarios have NO code:**

```csharp
case 0:  // Early Game - Tied       ? EMPTY
case 1:  // Early Game - Losing     ? EMPTY
case 2:  // Early Game - Winning    ? EMPTY
case 3:  // Mid Game - Tied         ? EMPTY
case 4:  // Mid Game - Losing       ? EMPTY
case 5:  // Mid Game - Winning      ? EMPTY
case 9:  // Last End - Tied         ? EMPTY (only logs)
case 10: // Last End - Losing       ? EMPTY (only logs)
case 11: // Last End - Winning      ? EMPTY (only logs)
```

**Impact:**
- 75% of game situations use fallback AI
- Strategy depth is lost
- Predictable patterns in early/mid/late game

---

### 2. **Fallback AI Ignores Character Stats**

**Line 1277:**
```csharp
int aiSkill = 8;  // ? HARDCODED!
// int aiSkill = activeCharStats.drawAccuracy.GetValue();  // Commented out!
```

**Impact:**
- Elite teams play same as rookies (in 9/12 scenarios)
- Character stat system not utilized
- No difficulty scaling

---

### 3. **Inconsistent Accuracy Application**

**Three different formulas:**

```csharp
// Formula 1: Draw shots
Random.insideUnitCircle * (1.5f - (0.01f * accuracy))

// Formula 2: Takeouts
Random.insideUnitCircle * (1.5f - (0.005f * accuracy))  // Half the scaling!

// Formula 3: Guards
Random.insideUnitCircle * Random.Range(0f, 1.5f - (0.01f * accuracy))  // Extra random!
```

**Why different?**
- Takeouts use 0.005f multiplier (less variance reduction)
- Guards use nested Random.Range (more unpredictable)
- No clear reasoning for different formulas

---

### 4. **Guard Selection Randomness**

**Line 1125:**
```csharp
if (rockCurrent % 2 == 1)
{
    if (Random.Range(0f, 1f) < 0.5f)
        guardSelect = 1;  // Left
    else
        guardSelect = 3;  // Right
}
else
{
    guardSelect = 2;  // Center
}
```

**Issues:**
- Odd rocks randomly pick left OR right (never center!)
- Even rocks always pick center
- No strategic reasoning (should consider opponent rock positions)

---

### 5. **Crash Handling**

**When takeout skill check fails:**
```csharp
if (shotSelector == 99)  // Crash
{
    // Try again with broader target search
    TakeOutTarget(activeTeamName, otherTeamName, "All", out hit, out takeOutSelector);
    
    if (hit)
        shotSelector = SkillCheck("Takeout", accuracy);
    else
        shotSelector = 1;  // Give up, go out
}
```

**Quality:** ? **7/10**
- Good fallback logic
- Prevents infinite loops
- But why not try a different shot type?

---

### 6. **Target Selection Logic**

**`TakeOutTarget()` method:**

```csharp
void TakeOutTarget(string activeTeam, string otherTeam, string targetRange, 
                   out bool hit, out int takeOutSelector)
{
    switch (targetRange)
    {
        case "House":
            // Find unguarded opponent rocks in house
            foreach (rock in houseList)
            {
                if (rock.team == opponent && !GuardedCheck(rock))
                {
                    takeOutSelector = rock.rockIndex;
                    return;
                }
            }
            break;
        
        case "Guards":
            // Find guards based on rock parity
            if (rockCurrent % 2 == 1)  // Odd rocks target center guards
                foreach (guard where |x| < 0.75f)
            else                       // Even rocks target corner guards
                foreach (guard where |x| > 0.5f)
            break;
        
        case "All":
            // Desperate - any rock in play
            foreach (rock in rockList where inPlay)
            break;
    }
}
```

**Quality:** ? **8/10**
- Checks for guards (good!)
- Strategic target selection based on position
- Falls back gracefully

**Issue:** Parity-based targeting is arbitrary (why do odd rocks target center?)

---

## ?? Comparison: RandomRockPlacement vs AI_Strategy

| Feature | RandomRockPlacement | AI_Strategy |
|---------|---------------------|-------------|
| **Scenarios Implemented** | 3/12 (25%) | 4/4 (100%) |
| **Uses CharacterStats** | ? Partially (3 scenarios) | ? Yes (all scenarios) |
| **Fallback Quality** | ? Poor (hardcoded skill) | ? Good (uses stats) |
| **Takeout Logic** | ? Excellent (guard checks) | ? Excellent (physics) |
| **Guard Strategy** | ?? Random selection | ? Strategic placement |
| **Freeze Shots** | ? Implemented | ? Not used |
| **Code Structure** | ?? 75% incomplete | ? Complete |
| **Documentation** | ? No comments | ?? Minimal |

---

## ?? Recommendations

### Priority 1: Implement Missing Scenarios (CRITICAL)

**Add code for cases 0-5, 9-11:**

```csharp
case 0:  // Early Game - Tied
    #region Early Game - Tied
    // Conservative: Build rocks, establish position
    if (rockCurrent < 5)
    {
        if (Random.value < 0.7f)
            shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
        else
            shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
    }
    else
    {
        // Mix of draws and guards based on house count
        if (gm.houseList.Count < 2)
            shotSelector = SkillCheck("Draw", activeCharStats.drawAccuracy.GetValue());
        else if (gm.houseList.Count < 4)
            shotSelector = SkillCheck("Guard", activeCharStats.guardAccuracy.GetValue());
        else
            shotSelector = SkillCheck("Takeout", activeCharStats.takeOutAccuracy.GetValue());
    }
    #endregion
    break;
```

**Implement all 9 missing scenarios** with proper curling strategy.

---

### Priority 2: Fix Fallback AI to Use Character Stats

**Line 1277:**
```csharp
// BEFORE:
int aiSkill = 8;  // ? Hardcoded

// AFTER:
int aiSkill = Mathf.RoundToInt(
    (activeCharStats.drawAccuracy.GetValue() + 
     activeCharStats.guardAccuracy.GetValue() + 
     activeCharStats.takeOutAccuracy.GetValue()) / 3f
);  // ? Average of all shooting stats
```

---

### Priority 3: Standardize Accuracy Formulas

**Create helper method:**
```csharp
private Vector2 GetShotError(float accuracy, float baseMaxError, bool useNestedRandom = false)
{
    float accuracyRatio = Mathf.Clamp01(accuracy / 100f);
    float maxError = baseMaxError * (1f - accuracyRatio);
    
    if (useNestedRandom)
        return Random.insideUnitCircle * Random.Range(0f, maxError);
    else
        return Random.insideUnitCircle * maxError;
}

// Usage:
rockPos[rockCurrent] = placePos[9] + GetShotError(drawAccuracy, 1.5f);
```

---

### Priority 4: Improve Guard Selection Logic

**Instead of random:**
```csharp
private int SelectGuardPosition(string activeTeam, string otherTeam)
{
    // Analyze house list to find best guard position
    if (gm.houseList.Count > 0)
    {
        // Guard the leading rock
        Vector2 leadRockPos = gm.houseList[0].rock.transform.position;
        
        if (Mathf.Abs(leadRockPos.x) < 0.5f)
            return 2;  // Center guard for center rocks
        else if (leadRockPos.x < 0)
            return 1;  // Left guard
        else
            return 3;  // Right guard
    }
    
    // Default: Center guard
    return 2;
}
```

---

### Priority 5: Add Strategy Documentation

**Add comments explaining strategy:**
```csharp
case 6:
    #region Penultimate End - Tied
    // STRATEGY: Force 1 (without hammer) or blank (with hammer)
    // - Clear opponent rocks to prevent scoring
    // - Build guards if ahead to protect position
    // - Remove guards if behind to create scoring chances
    
    if (rockCurrent < 5)
    {
        // Early rocks: Clear house or build defensive position
        // ...
```

---

## ?? Code Quality Metrics

### Current State

| Metric | Score | Notes |
|--------|-------|-------|
| **Strategy Coverage** | 3/10 | Only 25% of scenarios implemented |
| **Character Stats Usage** | 4/10 | Used in 3 scenarios, ignored in fallback |
| **Code Consistency** | 5/10 | Multiple accuracy formulas, mixed patterns |
| **Guard Logic** | 4/10 | Random selection, no strategic reasoning |
| **Takeout Logic** | 9/10 | Excellent - checks guards, realistic failure |
| **Freeze Logic** | 8/10 | Well implemented, realistic mechanics |
| **Documentation** | 2/10 | Only region markers, no strategy explanations |
| **Maintainability** | 5/10 | Large switch statements, duplicated code |

**Overall:** **5/10** - Functional but incomplete

---

### After Recommended Fixes

| Metric | Target | Improvement |
|--------|--------|-------------|
| **Strategy Coverage** | 9/10 | +600% (all scenarios) |
| **Character Stats Usage** | 9/10 | +125% (all paths use stats) |
| **Code Consistency** | 8/10 | +60% (unified formulas) |
| **Guard Logic** | 8/10 | +100% (strategic selection) |
| **Documentation** | 8/10 | +300% (strategy comments) |
| **Maintainability** | 7/10 | +40% (helper methods) |

**Overall Target:** **8.5/10** - Production quality

---

## ?? Relationship to AI_Strategy

### Similarities
- Both use scenario-based decision making
- Both use character stats for accuracy
- Both have takeout target selection logic
- Both use skill checks for success/failure

### Differences
- **RandomRockPlacement:** Pre-places rocks for mixed setup
- **AI_Strategy:** Controls live AI shot selection during gameplay
- **RandomRockPlacement:** Simpler (just positions), **AI_Strategy:** Complex (physics + strategy)
- **RandomRockPlacement:** 75% incomplete, **AI_Strategy:** 100% complete

### Integration Point
Both systems should use **consistent accuracy formulas** and **character stat queries**.

---

## ? Summary

### What Works Well
1. ? **Takeout logic** - Excellent guard checking and target selection
2. ? **Freeze shots** - Realistic mechanics and positioning
3. ? **Skill checks** - Proper use of character stats with realistic failure modes
4. ? **Implemented scenarios (6-8)** - Well thought out curling strategy

### What Needs Work
1. ? **75% of scenarios empty** - Missing early/mid/late game logic
2. ? **Fallback AI ignores stats** - Hardcoded skill value of 8
3. ? **Inconsistent formulas** - Three different accuracy calculations
4. ? **Random guard selection** - No strategic reasoning
5. ? **No documentation** - Strategy intent unclear

### Priority Actions
1. ?? **Implement missing scenarios** (Cases 0-5, 9-11)
2. ?? **Fix fallback AI** to use character stats
3. ?? **Standardize accuracy formulas**
4. ?? **Improve guard selection logic**
5. ?? **Add strategy documentation**

---

## ?? Conclusion

**RandomRockPlacement has a solid foundation** for shot execution (takeouts, freezes, accuracy) but is **75% incomplete** in terms of strategic decision-making. The implemented scenarios (6-8) show good understanding of curling strategy, suggesting the missing scenarios were simply **not finished** rather than **poorly designed**.

**Recommended Action:** Complete the missing 9 scenarios using the same quality and patterns as scenarios 6-8. This would elevate the system from "functional but incomplete" to "production-ready strategic AI."

**Estimated Effort:** 
- Implementing missing scenarios: ~4-6 hours
- Fixing fallback AI: ~1 hour
- Standardizing formulas: ~2 hours
- Adding documentation: ~1 hour
**Total: 8-10 hours of development**

The system is **salvageable and worth completing** - the hard parts (shot execution, accuracy, freeze mechanics) are already done well!
