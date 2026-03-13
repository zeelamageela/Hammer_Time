# AI Finesse-Based Shot Preference Implementation Guide

## Overview

This guide implements **skill-based shot selection** for AI, making high-finesse players prefer advanced shots (freezes, runbacks, angle raises) while keeping low-finesse players conservative.

**Build Status:** ? Ready for implementation

---

## ?? **CORE CONCEPT**

### Current Problem
- AI shot selection is **skill-agnostic**
- All AI players choose shots the same way (same scoring thresholds)
- High-finesse players don't play more creatively
- Freezes, runbacks, and angle raises are underutilized

### Solution
- **Finesse skill** acts as **shot preference modifier**
- Low finesse (0-40%): Conservative play, avoid complex shots (-20 to -15 penalties)
- Medium finesse (40-70%): Balanced play, neutral scoring (0 bonus/penalty)
- High finesse (70-100%): Aggressive play, prefer complex shots (+25 to +30 bonuses)

---

## ?? **IMPLEMENTATION LOCATIONS**

All changes are in `Assets\Scripts\AI\AI_Target.cs`

### **1. EvaluateScoringOptions() - Line ~4035**

**Goal:** Add finesse bonuses to freeze, raise, and alternate scoring shots

#### Code Changes

**Step 1:** Add finesse skill tracking at start of method

```csharp
private void EvaluateScoringOptions(ShotContext context, int rockCurrent)
{
    Debug.Log($"[AI_Target] Evaluating scoring options for rock #{rockCurrent}");
    
    // Get shooter's finesse skill for advanced shot bonuses
    CharacterStats shooterStats = GetShooterStats(rockCurrent);
    float finesseSkill = shooterStats != null ? shooterStats.finesseAccuracy.GetValue() : 50f;
    float finesseRatio = Mathf.Clamp01(finesseSkill / 100f);
    
    Debug.Log($"[Scoring] Shooter finesse: {finesseSkill:F0}% (ratio: {finesseRatio:F2})");
    
    // ... existing context check code ...
}
```

**Step 2:** Enhance freeze scoring with finesse and strategic bonuses

Find the section with `// OPTION 2: Freeze on opponent's best rock` (around line 4050)

Replace:
```csharp
// OPTION 2: Freeze on opponent's best rock
float freezeScore = 0f;
int rockToFreeze = -1;

if (gm.houseList.Count > 0)
{
    rockToFreeze = FindBestFreezeTarget(rockCurrent, out freezeScore);
    
    // PENALTY if called from removal failure
    if (calledFromRemovalFailure && freezeScore > 0f)
    {
        freezeScore -= 15f;
        Debug.Log($"[Scoring] REMOVAL FAILURE PENALTY: Freeze -15 ? {freezeScore:F2}");
    }
    
    if (rockToFreeze >= 0)
    {
        Debug.Log($"  Option 2: Freeze on rock #{rockToFreeze} - Score: {freezeScore:F2}");
    }
}
```

With:
```csharp
// OPTION 2: Freeze on opponent's best rock (ENHANCED with finesse bonus)
float freezeScore = 0f;
int rockToFreeze = -1;

if (gm.houseList.Count > 0)
{
    rockToFreeze = FindBestFreezeTarget(rockCurrent, out freezeScore);
    
    if (rockToFreeze >= 0)
    {
        // FINESSE BONUS: High skill players prefer freezes!
        // Low skill (0%): -20 penalty (avoid complex shots)
        // Mid skill (50%): +0 neutral
        // High skill (100%): +30 bonus (prefer complex shots)
        float finesseBonus = Mathf.Lerp(-20f, 30f, finesseRatio);
        
        // Add strategic value based on game state
        GameObject freezeTarget = gm.rockList[rockToFreeze].rock;
        float strategicBonus = CalculateFreezeStrategicValue(rockCurrent, freezeTarget);
        
        freezeScore += finesseBonus + strategicBonus;
        
        Debug.Log($"[Scoring] Freeze: base={freezeScore - finesseBonus - strategicBonus:F1}, " +
                  $"finesse={finesseBonus:F1} (skill={finesseSkill:F0}%), " +
                  $"strategic={strategicBonus:F1}, FINAL={freezeScore:F1}");
        
        // LATE GAME BONUS: Freezes are valuable when protecting lead
        if (rockCurrent >= 12 && freezeScore > 0f)
        {
            freezeScore += 15f;
            Debug.Log($"[Scoring] Late game freeze bonus: +15 ? {freezeScore:F1}");
        }
        
        // PENALTY if called from removal failure
        if (calledFromRemovalFailure && freezeScore > 0f)
        {
            freezeScore -= 15f;
            Debug.Log($"[Scoring] REMOVAL FAILURE PENALTY: Freeze -15 ? {freezeScore:F2}");
        }
        
        Debug.Log($"  Option 2: Freeze on rock #{rockToFreeze} - Score: {freezeScore:F2}");
    }
}
```

**Step 3:** Enhance raise scoring with finesse bonus

Find the section with `// OPTION 3: Raise a friendly rock` (around line 4070)

Replace:
```csharp
// OPTION 3: Raise a friendly rock closer to button
float raiseScore = 0f;
int rockToRaiseForScore = FindBestRockToRaiseForScoring(rockCurrent, out raiseScore);

if (rockToRaiseForScore >= 0)
{
    Debug.Log($"  Option 3: Raise rock #{rockToRaiseForScore} toward button - Score: {raiseScore:F2}");
}
```

With:
```csharp
// OPTION 3: Raise a friendly rock closer to button (ENHANCED with finesse bonus)
float raiseScore = 0f;
int rockToRaiseForScore = FindBestRockToRaiseForScoring(rockCurrent, out raiseScore);

if (rockToRaiseForScore >= 0)
{
    // FINESSE BONUS: Angle raises are advanced shots!
    // Low skill (0%): -15 penalty (avoid risky plays)
    // Mid skill (50%): +0 neutral
    // High skill (100%): +25 bonus (love creative plays)
    float raiseFinesseBonus = Mathf.Lerp(-15f, 25f, finesseRatio);
    raiseScore += raiseFinesseBonus;
    
    Debug.Log($"[Scoring] Raise: base={raiseScore - raiseFinesseBonus:F1}, " +
              $"finesse bonus={raiseFinesseBonus:F1}, FINAL={raiseScore:F1}");
    
    Debug.Log($"  Option 3: Raise rock #{rockToRaiseForScore} toward button - Score: {raiseScore:F2}");
}
```

---

### **2. SimulateRunback() - Line ~4733**

**Goal:** Add skill-based alignment threshold (high finesse = more aggressive runbacks)

Find the section where alignment quality is checked:

Replace:
```csharp
private float SimulateRunback(GameObject guardRock, GameObject targetRock, int guardIndex, int targetIndex, int rockCurrent)
{
    if (guardRock == null || targetRock == null) return 0f;
    
    Vector2 guardPos = guardRock.transform.position;
    Vector2 targetPos = targetRock.transform.position;
    Vector2 launcherPos = new Vector2(0f, -25f);
    
    // CRITICAL: Check alignment - guard must be BETWEEN launcher and target
    // If they're not well-aligned, runback won't work
    float alignmentQuality = CheckRunbackAlignment(launcherPos, guardPos, targetPos);
    
    Debug.Log($"[AI_Target] Runback alignment check: launcher={launcherPos}, guard={guardPos}, target={targetPos}, quality={alignmentQuality:F2}");
    
    if (alignmentQuality < 0.6f) // Need good alignment (60%+ quality)
    {
        Debug.Log($"[AI_Target] Runback rejected - poor alignment ({alignmentQuality:F2} < 0.6)");
        return 0f;
    }
```

With:
```csharp
private float SimulateRunback(GameObject guardRock, GameObject targetRock, int guardIndex, int targetIndex, int rockCurrent)
{
    if (guardRock == null || targetRock == null) return 0f;
    
    Vector2 guardPos = guardRock.transform.position;
    Vector2 targetPos = targetRock.transform.position;
    Vector2 launcherPos = new Vector2(0f, -25f);
    
    // Get shooter's finesse skill
    CharacterStats shooterStats = GetShooterStats(rockCurrent);
    float finesseSkill = shooterStats != null ? shooterStats.finesseAccuracy.GetValue() : 50f;
    float finesseRatio = Mathf.Clamp01(finesseSkill / 100f);
    
    // CRITICAL: Check alignment - guard must be BETWEEN launcher and target
    // If they're not well-aligned, runback won't work
    float alignmentQuality = CheckRunbackAlignment(launcherPos, guardPos, targetPos);
    
    Debug.Log($"[AI_Target] Runback alignment check: launcher={launcherPos}, guard={guardPos}, target={targetPos}, quality={alignmentQuality:F2}");
    
    // SKILL-BASED ALIGNMENT THRESHOLD:
    // Low skill (0%): 0.70 (70% alignment required - very strict)
    // Mid skill (50%): 0.55 (55% alignment - moderate)
    // High skill (100%): 0.40 (40% alignment - aggressive)
    float minAlignment = Mathf.Lerp(0.70f, 0.40f, finesseRatio);
    
    Debug.Log($"[Runback] Alignment: {alignmentQuality:F2}, " +
              $"Required: {minAlignment:F2} (finesse={finesseSkill:F0}%)");
    
    if (alignmentQuality < minAlignment)
    {
        Debug.Log($"[AI_Target] Runback rejected - alignment too poor for skill level ({alignmentQuality:F2} < {minAlignment:F2})");
        return 0f;
    }
```

Then, at the end of the scoring section (around line 4760), enhance runback scoring:

Replace:
```csharp
// Score based on alignment quality and distance
float distanceScore = 1.0f - Mathf.Clamp01((guardToTargetDist - 0.5f) / 2.5f); // Closer = better
float totalScore = 55f * alignmentQuality * distanceScore;

Debug.Log($"[AI_Target] Runback viable! Alignment={alignmentQuality:F2}, Distance={guardToTargetDist:F2}, Score={totalScore:F2}");
return totalScore; // Good option if well-aligned
```

With:
```csharp
// Score based on alignment quality and distance
float distanceScore = 1.0f - Mathf.Clamp01((guardToTargetDist - 0.5f) / 2.5f); // Closer = better
float baseScore = 55f * alignmentQuality * distanceScore;

// FINESSE BONUS: High skill gets bonus for attempting runbacks!
// Low skill (0%): +0 (no bonus)
// Mid skill (50%): +10
// High skill (100%): +20 (big bonus for advanced play)
float finesseBonus = Mathf.Lerp(0f, 20f, finesseRatio);
float totalScore = baseScore + finesseBonus;

Debug.Log($"[Runback] VIABLE! Base={baseScore:F1}, " +
          $"Finesse bonus=+{finesseBonus:F1}, FINAL={totalScore:F1}");

return totalScore;
```

---

### **3. CalculateFreezeStrategicValue() - NEW METHOD**

Add this new method after `GetShooterWeightAccuracy()` (around line 10050):

```csharp
/// <summary>
/// Calculate strategic value of a freeze shot based on game state
/// Returns bonus score (0-100) based on multiple strategic factors
/// </summary>
private float CalculateFreezeStrategicValue(int rockCurrent, GameObject targetRock)
{
    float strategicValue = 0f;
    
    Rock_Info currentRockInfo = gm.rockList[rockCurrent].rockInfo;
    Vector2 button = new Vector2(0f, 6.5f);
    
    // FACTOR 1: Is target rock the SHOT ROCK?
    bool isShotRock = false;
    if (gm.houseList.Count > 0)
    {
        isShotRock = (gm.houseList[0].rockInfo.rockIndex == 
                      targetRock.GetComponent<Rock_Info>().rockIndex);
    }
    
    if (isShotRock)
    {
        strategicValue += 30f; // HUGE bonus - stealing shot rock!
        Debug.Log($"[Freeze Strategy] Target is SHOT ROCK ? +30");
    }
    
    // FACTOR 2: Game phase - freezes more valuable late game
    if (rockCurrent >= 12) // Last 4 rocks
    {
        strategicValue += 15f;
        Debug.Log($"[Freeze Strategy] Late game ? +15");
    }
    else if (rockCurrent >= 8) // Mid-late game
    {
        strategicValue += 5f;
        Debug.Log($"[Freeze Strategy] Mid-late game ? +5");
    }
    
    // FACTOR 3: Hammer situation
    bool hasHammer = (rockCurrent % 2 == 1) ? gm.redHammer : !gm.redHammer;
    
    if (!hasHammer)
    {
        // WITHOUT HAMMER: Freezes are CRITICAL (only way to steal!)
        strategicValue += 20f;
        Debug.Log($"[Freeze Strategy] Without hammer (stealing!) ? +20");
    }
    else
    {
        // WITH HAMMER: Freezes less valuable (we can draw for points)
        strategicValue -= 10f;
        Debug.Log($"[Freeze Strategy] With hammer (have options) ? -10");
    }
    
    // FACTOR 4: Number of opponent rocks in house
    int opponentRocks = 0;
    foreach (var houseRock in gm.houseList)
    {
        if (houseRock.rockInfo.teamName != currentRockInfo.teamName)
            opponentRocks++;
    }
    
    if (opponentRocks == 1)
    {
        // Only 1 opponent rock - freeze is perfect!
        strategicValue += 15f;
        Debug.Log($"[Freeze Strategy] Exactly 1 opponent rock ? +15");
    }
    else if (opponentRocks >= 3)
    {
        // Multiple opponent rocks - freeze might not be enough
        strategicValue -= 10f;
        Debug.Log($"[Freeze Strategy] {opponentRocks} opponent rocks (crowded) ? -10");
    }
    
    // FACTOR 5: Guard protection
    bool targetIsGuarded = false;
    Vector2 targetPos = targetRock.transform.position;
    
    foreach (var guard in gm.gList)
    {
        if (guard.lastTransform == null) continue;
        
        Vector2 guardPos = guard.lastTransform.position;
        float lateralDiff = Mathf.Abs(guardPos.x - targetPos.x);
        bool inFront = guardPos.y < targetPos.y;
        
        if (lateralDiff < 0.5f && inFront)
        {
            targetIsGuarded = true;
            break;
        }
    }
    
    if (targetIsGuarded)
    {
        // Target has guard - freeze makes it VERY hard to remove!
        strategicValue += 20f;
        Debug.Log($"[Freeze Strategy] Target is guarded (compound difficulty!) ? +20");
    }
    
    Debug.Log($"[Freeze Strategy] TOTAL STRATEGIC VALUE: {strategicValue:F1}");
    return strategicValue;
}
```

---

### **4. FindBestFreezeTarget() - Enhance Scoring**

Find `FindBestFreezeTarget()` method (around line 10200) and update the scoring section:

Replace the section where `score` is calculated (around line 10250):

```csharp
// TOTAL SCORE (0-100)
float score = behindScore + lateralScore + distScore;

Debug.Log($"[Freeze Target] Rock at ({rockPos.x:F2}, {rockPos.y:F2}): " +
          $"Behind={behindScore:F1}, Lateral={lateralScore:F1}, " +
          $"Dist={distScore:F1}, TOTAL={score:F1}/100");
```

With:
```csharp
// Get shooter's finesse skill for relaxed threshold
CharacterStats shooterStats = GetShooterStats(rockCurrent);
float finesseSkill = shooterStats != null ? shooterStats.finesseAccuracy.GetValue() : 50f;
float finesseRatio = Mathf.Clamp01(finesseSkill / 100f);

// RELAXED "BEHIND BUTTON" REQUIREMENT for high finesse:
// Low skill: Ideal = 0.15 behind, max = 0.6
// High skill: Ideal = 0.15 behind, max = 1.2 (more forgiving!)
float maxBehindDeviation = Mathf.Lerp(0.6f, 1.2f, finesseRatio);

float idealBehindDist = 0.15f;
float behindDeviation = Mathf.Abs(distBehindButton - idealBehindDist);
float behindQuality = Mathf.Clamp01(1f - (behindDeviation / maxBehindDeviation));
float behindScore = behindQuality * 60f;

// STRATEGIC BONUS: Freezing on shot rock is HUGE!
float strategicBonus = 0f;
if (gm.houseList.Count > 0 && gm.houseList[0].rockInfo.rockIndex == houseRock.rockInfo.rockIndex)
{
    strategicBonus = 25f; // BIG bonus for freezing on shot rock!
    Debug.Log($"[Freeze] SHOT ROCK TARGET! Bonus +{strategicBonus:F1}");
}

// TOTAL SCORE (0-100+)
float score = behindScore + lateralScore + distScore + strategicBonus;

Debug.Log($"[Freeze Target] Rock at ({rockPos.x:F2}, {rockPos.y:F2}): " +
          $"Behind={behindScore:F1}, Lateral={lateralScore:F1}, " +
          $"Dist={distScore:F1}, Strategic={strategicBonus:F1}, " +
          $"TOTAL={score:F1}/100+");
```

---

## ?? **EXPECTED BEHAVIOR**

### Low Finesse AI (30%)

**Shot Selection:**
```
Direct Takeout: 70% (safe, reliable)
Draw to Button: 20% (basic scoring)
Peel: 5% (clearing guards)
Freeze/Runback/Raise: 5% (rare, only perfect setups)
```

**Alignment Requirements:**
- Runback: 70% minimum (very strict)
- Freeze: Within 0.6 units of ideal (strict)

**Scoring Example:**
```
Draw to button: 70 points (baseline)
Freeze (base 60): 60 - 20 (finesse penalty) = 40 points ? NOT CHOSEN
Runback (60% aligned): 0 points (rejected - below 70% threshold)
```

---

### Medium Finesse AI (60%)

**Shot Selection:**
```
Direct Takeout: 50% (still primary)
Draw to Button: 20% (reliable scoring)
Freeze: 15% (occasional steal attempts)
Runback: 10% (when aligned)
Raise/Tick: 5% (creative plays)
```

**Alignment Requirements:**
- Runback: 55% minimum (moderate)
- Freeze: Within 0.9 units of ideal (moderate)

**Scoring Example:**
```
Draw to button: 70 points
Freeze (base 60): 60 + 6 (finesse) + 15 (strategic) = 81 points ? CHOSEN!
Runback (55% aligned): 55 points ? VIABLE
```

---

### High Finesse AI (90%)

**Shot Selection:**
```
Freeze: 25% (frequent steal attempts!)
Runback: 20% (aggressive multi-rock removal)
Direct Takeout: 30% (still useful)
Draw to Button: 15% (when clear path)
Raise/Tick/Angle: 10% (creative plays)
```

**Alignment Requirements:**
- Runback: 40% minimum (aggressive)
- Freeze: Within 1.2 units of ideal (forgiving)

**Scoring Example:**
```
Draw to button: 70 points
Freeze (base 60): 60 + 27 (finesse) + 30 (strategic, shot rock) + 15 (late game) = 132 points ? CHOSEN!
Runback (45% aligned): 55 + 18 (finesse) = 73 points ? CHOSEN!
```

---

## ?? **TESTING CHECKLIST**

### Low Finesse (30%) Tests
- [ ] Rarely attempts freezes (< 10% of scoring shots)
- [ ] Rejects runbacks unless 70%+ aligned
- [ ] Prefers direct takeouts over complex shots
- [ ] Draw to button is most common scoring option

### Medium Finesse (60%) Tests
- [ ] Occasional freezes (~15% of scoring shots)
- [ ] Accepts runbacks with 55%+ alignment
- [ ] Balanced shot selection
- [ ] Strategic freezes when without hammer

### High Finesse (90%) Tests
- [ ] Frequent freezes (~25% of scoring shots)
- [ ] Aggressive runbacks with 40%+ alignment
- [ ] Prefers advanced shots when available
- [ ] Strategic freeze on shot rock, late game

### General Tests
- [ ] Finesse bonuses scale smoothly (0-100%)
- [ ] Strategic freeze value calculated correctly
- [ ] Late game bonuses applied (+15 for freezes)
- [ ] Hammer situation affects freeze preference

---

## ?? **STRATEGIC FREEZE ALGORITHM**

### When to Freeze (Decision Tree)

```
START: EvaluateScoringOptions()
  ?
  Is target SHOT ROCK? ? YES ? +30 strategic bonus
  ?
  Late game (rock 12+)? ? YES ? +15 bonus
  ?
  Do we have HAMMER? ? NO ? +20 bonus (steal situation!)
                     ? YES ? -10 penalty (have options)
  ?
  Only 1 opponent rock? ? YES ? +15 bonus (perfect freeze!)
  ?
  Is target GUARDED? ? YES ? +20 bonus (compound difficulty!)
  ?
  Calculate finesse bonus: Lerp(-20, +30, finesseRatio)
  ?
  FINAL SCORE = base + finesse + strategic
  ?
  Compare to other options (draw, raise, etc.)
  ?
  PICK HIGHEST SCORE
```

---

## ?? **DEBUGGING**

### Log Output Examples

**Low Finesse (30%) - Freeze Rejected:**
```
[Scoring] Shooter finesse: 30% (ratio: 0.30)
[Freeze] Base=60.0, finesse=-14.0 (skill=30%), strategic=15.0, FINAL=61.0
[Scoring] Draw to button - Score: 70.00
[AI_Target] ? SELECTED: Draw to button (score: 70.00)
```

**High Finesse (90%) - Freeze Chosen:**
```
[Scoring] Shooter finesse: 90% (ratio: 0.90)
[Freeze] SHOT ROCK TARGET! Bonus +25.0
[Freeze Strategy] Late game ? +15
[Freeze Strategy] Without hammer (stealing!) ? +20
[Freeze Strategy] TOTAL STRATEGIC VALUE: 60.0
[Freeze] Base=65.0, finesse=+27.0 (skill=90%), strategic=60.0, FINAL=152.0
[Scoring] Draw to button - Score: 70.00
[AI_Target] ? SELECTED: Freeze (score: 152.00) - Steal shot rock!
```

**Mid Finesse (60%) - Runback Accepted:**
```
[Runback] Alignment: 0.52, Required: 0.58 (finesse=60%)
[Runback] REJECTED - alignment too poor for skill level
```
vs
```
[Runback] Alignment: 0.62, Required: 0.58 (finesse=60%)
[Runback] VIABLE! Base=50.5, Finesse bonus=+12.0, FINAL=62.5
```

---

## ?? **BUILD & DEPLOY**

1. **Backup Files:**
   ```
   Copy AI_Target.cs to AI_Target.cs.backup
   ```

2. **Apply Changes:**
   - Follow implementation steps 1-4 above
   - Add new method `CalculateFreezeStrategicValue()`
   - Update existing methods with finesse bonuses

3. **Build:**
   ```
   Build ? Rebuild Solution
   ```

4. **Test:**
   - Create low/mid/high finesse AI teams
   - Run exhibition matches
   - Observe shot selection logs
   - Verify finesse bonuses applied

5. **Verify:**
   - [ ] Build successful
   - [ ] No compilation errors
   - [ ] Debug logs show finesse calculations
   - [ ] Shot selection varies by finesse level

---

## ?? **IMPORTANT NOTES**

1. **Existing Shot Scoring Unchanged:**
   - Draw base score: 70
   - Takeout base score: 60
   - Raise base score: varies (40-80)
   - Only BONUSES/PENALTIES change based on finesse

2. **Finesse is ADDITIVE:**
   - Does NOT replace other bonuses (late game, strategic, etc.)
   - Stacks with all existing scoring modifiers
   - Final score can exceed 100 (intentional for high-value shots)

3. **Strategic Value is DYNAMIC:**
   - Changes based on game state
   - Shot rock target: +30
   - Late game: +15
   - No hammer: +20
   - Guarded target: +20

4. **Alignment Thresholds are STRICT:**
   - 70% for low finesse (only perfect setups)
   - 55% for mid finesse (reasonable attempts)
   - 40% for high finesse (aggressive play)

---

## ?? **SUMMARY**

### What Changed
- **EvaluateScoringOptions()**: Added finesse tracking, freeze/raise bonuses
- **SimulateRunback()**: Skill-based alignment threshold, finesse bonus
- **FindBestFreezeTarget()**: Relaxed requirements for high finesse
- **CalculateFreezeStrategicValue()**: NEW - analyzes game state for freeze value

### Why It Matters
- **Low Finesse AI**: Plays conservatively, avoids risky shots
- **High Finesse AI**: Plays creatively, attempts advanced plays
- **Realistic Strategy**: Mimics real curling (skilled players try harder shots)
- **Dynamic Gameplay**: AI adapts shot selection to skill level

### Expected Impact
- **More Freezes**: High finesse AI freezes 25% vs low finesse 5%
- **More Runbacks**: Aggressive alignment thresholds for experts
- **Better Strategy**: Shot selection matches player skill level
- **Replayability**: Different finesse levels = different AI personalities

---

**Status:** ? Ready for implementation
**Complexity:** Medium (4 locations, 1 new method)
**Risk:** Low (additive bonuses, doesn't break existing logic)
**Testing:** Required (verify finesse scaling across 0-100% range)
