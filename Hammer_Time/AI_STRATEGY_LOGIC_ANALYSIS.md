# AI_Strategy OnShot() Logic Analysis

## Executive Summary

**Status:** ? **MAJOR LOGIC BUGS FOUND**

The `OnShot()` method has several critical issues in phase detection, strategy selection, and case statement bugs that will cause incorrect AI behavior.

---

## Issue #1: ? **CRITICAL - Wrong Case Labels**

### Problem: Case "middle hammer" and "late hammer" Don't Match

**Line 254 - ConservativeSteal():**
```csharp
case "middle hammer":  // ? BUG: phase is set to "middle", not "middle hammer"
```

**Line 391 - ConservativeSteal():**
```csharp
case "late hammer":  // ? BUG: phase is set to "late", not "late hammer"
```

**Root Cause in OnShot() (Lines 164-175):**
```csharp
if (rockCurrent < 4)
{
    phase = "early";   // ? Correct
}
else if (rockCurrent < 10)
{
    phase = "middle";  // ? But case expects "middle hammer"
}
else
{
    phase = "late";    // ? But case expects "late hammer"
}
```

### Impact
**ConservativeSteal() will NEVER execute middle or late phase logic!**
- Middle phase (rocks 4-9): Falls through to `default`, does nothing
- Late phase (rocks 10-15): Falls through to `default`, does nothing

### Fix Required
Either:
1. Change case labels to `"middle"` and `"late"` (recommended)
2. Or change phase assignments to `"middle hammer"` and `"late hammer"`

---

## Issue #2: ? **Wrong Phase Logic in OnShot()**

### Problem: Incorrect Strategy Selection Logic

**Lines 177-210 - Not Hammer (Even Rocks):**
```csharp
if (rockCurrent % 2 == 0)  // Team WITHOUT hammer
{
    if (gm.endTotal - gm.endCurrent >= 2)  // 2+ ends left
    {
        if (activeTeamScore - oppTeamScore >= 2)
            AggressiveNotHammer(rockCurrent, phase);  // ? Ahead by 2+
        if (activeTeamScore <= oppTeamScore)          // ? BUG: Should be "else if"
            ConservativeStealOrBlank(rockCurrent, phase);
        else
            ConservativeSteal(rockCurrent, phase);
    }
```

### Bug Analysis

**Current Logic:**
```
if (ahead by 2+) ? AggressiveNotHammer
if (tied or behind) ? ConservativeStealOrBlank  // ? Can execute BOTH!
else ? ConservativeSteal
```

**Problem:** If `activeTeamScore - oppTeamScore == 2`:
- Executes `AggressiveNotHammer()`
- Then checks `if (2 <= 0)` ? false, skips
- Then hits `else` ? Executes `ConservativeSteal()`
- **TWO strategies execute!**

### Expected Logic (Curling Strategy)

**Team WITHOUT Hammer (Even rocks):**
```
Ahead by 2+    ? Aggressive (force them to take 1 or blank)
Ahead by 1     ? Conservative Steal (try to steal or blank)
Tied or Behind ? Conservative Steal or Blank (don't give up points)
```

**Team WITH Hammer (Odd rocks):**
```
Behind         ? Aggressive (try to steal points)
Tied or Ahead  ? Conservative Score 2+ or Blank (use hammer advantage)
```

---

## Issue #3: ?? **Inconsistent Guard Blocking Logic**

### Problem: Duplicated Logic Without Helper Usage

**Example from ConservativeSteal (lines 317-319):**
```csharp
if (Mathf.Abs(cenGuard.position.x - closestRock.transform.position.x) <= 0.1f | 
    Mathf.Abs(cenGuard.position.x - closestRock.transform.position.x) <= 0.1f)
```

**Issues:**
1. ? **Same condition checked twice** (copy-paste error)
2. ? **Uses bitwise OR `|` instead of logical OR `||`**
3. ? **Doesn't use `IsGuardBlocking()` helper** we just added

### Should Be:
```csharp
if (IsGuardBlocking(cenGuard, closestRock) || IsGuardBlocking(tCenGuard, closestRock))
```

---

## Issue #4: ? **Invalid Rock Index Access**

### Problem: Array Out of Bounds Risk

**ConservativeSteal - Lines 367-374:**
```csharp
if (closestRock.transform.position.x < 0)
{
    if (lCornGuard)
        aiTarg.OnTarget("Peel", rockCurrent, lCornGuard.gameObject.GetComponent<Rock_Info>().rockIndex);
    else
        aiTarg.OnTarget("Take Out", rockCurrent, gm.houseList[1].rockInfo.rockIndex);
        // ? BUG: Accesses [1] without checking if houseList.Count > 1
}
```

**Impact:** If `houseList.Count == 1`, this throws `IndexOutOfRangeException`

### Multiple Occurrences
This pattern appears ~15 times throughout the file:
- ConservativeSteal: Lines 367, 374, 384, 390
- AggressiveNotHammer: Multiple locations
- Others: Various

---

## Issue #5: ?? **Questionable Strategy Logic**

### ConservativeSteal - Middle Phase Bug

**Lines 285-291:**
```csharp
if (gm.houseList[1].rockInfo.teamName != rockInfo.teamName)
{
    if (Mathf.Abs(gm.houseList[1].rock.transform.position.x) <= 0.5f)
    {
        if (cenGuard | tCenGuard)
            aiTarg.OnTarget("Auto Draw Four Foot", rockCurrent, 0);
        else
            aiShoot.OnShot("Centre Guard", rockCurrent);  // ? Why guard when opponent has 2nd shot?
```

**Question:** If opponent has 2nd closest rock in center, why place a guard instead of trying to remove it?

**Expected:** Should probably be attempting to take out the 2nd rock, not guarding it.

---

## Issue #6: ? **Duplicate Unreachable Code**

### ConservativeStealOrBlank - Late Phase

**Lines 1596-1621:**
```csharp
else
{
    if (rCornGuard)
        aiTarg.OnTarget("Peel", rockCurrent, rCornGuard...);
    else
        aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1]...);
}
aiTarg.OnTarget("Tap Back", rockCurrent, gm.houseList[1]...);  
// ? BUG: This line ALWAYS executes after the else block!
```

**Impact:** The else block's logic is overridden by unconditional Tap Back call.

---

## Issue #7: ?? **Missing activeTeamName Initialization**

### Problem: SimpleAIShoot Uses Uninitialized Variable

**SimpleAIShoot (Line 76):**
```csharp
int valuableRockIndex = GetMostValuableOpponentRockIndex(activeTeamName);
// ?? activeTeamName is not set! Only set in OnShot()
```

**Impact:** If `SimpleAIShoot()` is called (which it is by AIManager), `activeTeamName` is null/empty, so `GetMostValuableOpponentRockIndex()` returns -1, never finding opponent rocks.

### Required Fix
`SimpleAIShoot()` needs to set `activeTeamName` like `OnShot()` does.

---

## Correct Curling Strategy (For Reference)

### Hammer Strategy
Team WITH hammer (last rock advantage):

| Situation | Strategy | Reasoning |
|-----------|----------|-----------|
| Behind | Aggressive - Try to steal points | Need points, take risks |
| Tied/Ahead | Conservative - Score 2+ or blank | Protect hammer, don't give up 1 |

### Not Hammer Strategy  
Team WITHOUT hammer (no last rock):

| Situation | Strategy | Reasoning |
|-----------|----------|-----------|
| Ahead by 2+ | Aggressive - Force them to 1 or blank | Can afford to give up points |
| Ahead by 1 | Conservative - Try to steal or blank | Protect lead |
| Tied/Behind | Conservative - Steal or force | Need to steal or force them to 1 |

---

## Current Implementation Issues

### OnShot() Strategy Selection (Lines 177-210)

**Even Rocks (Not Hammer):**
```csharp
if (gm.endTotal - gm.endCurrent >= 2)  // 2+ ends left
{
    if (activeTeamScore - oppTeamScore >= 2)
        AggressiveNotHammer(rockCurrent, phase);  // ? Correct
    if (activeTeamScore <= oppTeamScore)          // ? Should be "else if"
        ConservativeStealOrBlank(rockCurrent, phase);
    else
        ConservativeSteal(rockCurrent, phase);
}
```

**Problem:** When `activeTeamScore - oppTeamScore >= 2`, it:
1. Calls `AggressiveNotHammer()`
2. Then enters `else` and calls `ConservativeSteal()`
3. **Both strategies execute!**

**Fix:**
```csharp
if (activeTeamScore - oppTeamScore >= 2)
    AggressiveNotHammer(rockCurrent, phase);
else if (activeTeamScore <= oppTeamScore)  // Add "else if"
    ConservativeStealOrBlank(rockCurrent, phase);
else
    ConservativeSteal(rockCurrent, phase);
```

---

## All Bugs Summary

| # | Severity | Issue | Location | Impact |
|---|----------|-------|----------|--------|
| 1 | ?? CRITICAL | Case "middle hammer" doesn't match phase "middle" | ConservativeSteal L254 | Middle phase never executes |
| 2 | ?? CRITICAL | Case "late hammer" doesn't match phase "late" | ConservativeSteal L391 | Late phase never executes |
| 3 | ?? CRITICAL | Missing "else if" causes dual strategy execution | OnShot L182 | Two strategies execute simultaneously |
| 4 | ?? CRITICAL | activeTeamName not initialized in SimpleAIShoot | SimpleAIShoot L76 | Never finds opponent rocks |
| 5 | ?? HIGH | Array bounds not checked before accessing [1] | Multiple locations | Crashes if only 1 rock |
| 6 | ?? HIGH | Unreachable code - unconditional after else | ConservativeStealOrBlank L1621 | Wrong shot executed |
| 7 | ?? MEDIUM | Duplicate condition check (copy-paste) | ConservativeSteal L317 | Inefficient, confusing |
| 8 | ?? MEDIUM | Bitwise OR `|` instead of logical `||` | Multiple locations | May work but incorrect |
| 9 | ?? MEDIUM | Helper methods not used | Throughout | Code duplication |
| 10 | ?? LOW | Questionable strategy decisions | Various | May not be optimal play |

---

## Recommended Fixes (Priority Order)

### Priority 1: CRITICAL Bugs (Fix Immediately)

**1. Fix phase case labels in ConservativeSteal:**
```csharp
case "middle hammer":  ? case "middle":
case "late hammer":    ? case "late":
```

**2. Fix OnShot() strategy selection:**
```csharp
if (activeTeamScore - oppTeamScore >= 2)
    AggressiveNotHammer(rockCurrent, phase);
else if (activeTeamScore <= oppTeamScore)  // Add "else if"
    ConservativeStealOrBlank(rockCurrent, phase);
else
    ConservativeSteal(rockCurrent, phase);
```

**3. Fix SimpleAIShoot() initialization:**
```csharp
public void SimpleAIShoot(int rockCurrent)
{
    // Set active team like OnShot() does
    if (rockCurrent % 2 == 0)
    {
        activeTeamName = gm.redHammer ? gm.yellowTeamName : gm.redTeamName;
    }
    else
    {
        activeTeamName = gm.redHammer ? gm.redTeamName : gm.yellowTeamName;
    }
    
    // Now this works correctly
    int valuableRockIndex = GetMostValuableOpponentRockIndex(activeTeamName);
    // ...
}
```

### Priority 2: HIGH Bugs (Fix Soon)

**4. Add bounds checking before array access:**
```csharp
// Before accessing gm.houseList[1]:
if (gm.houseList.Count > 1)
{
    // Safe to access [1]
}
```

**5. Remove duplicate unreachable code:**
```csharp
// ConservativeStealOrBlank line ~1621
// Remove the unconditional Tap Back after the else block
```

### Priority 3: MEDIUM Improvements (Nice to Have)

**6. Fix bitwise OR to logical OR:**
```csharp
if (cenGuard | tCenGuard)  ?  if (cenGuard || tCenGuard)
```

**7. Use helper methods:**
```csharp
// Replace all manual guard blocking checks
if (Mathf.Abs(cenGuard.position.x - closestRock.transform.position.x) <= 0.1f)
// With helper
if (IsGuardBlocking(cenGuard, closestRock))
```

**8. Use GetRockIndex() helper:**
```csharp
cenGuard.gameObject.GetComponent<Rock_Info>().rockIndex
// Replace with
GetRockIndex(cenGuard)
```

---

## Phase Definition Verification

### Current Phase Breakpoints (Lines 164-175)
```csharp
rockCurrent < 4    ? "early"   // Rocks 0-3  (Leads shooting)
rockCurrent < 10   ? "middle"  // Rocks 4-9  (Seconds + start of Thirds)
rockCurrent >= 10  ? "late"    // Rocks 10-15 (End of Thirds + Skips)
```

### Is This Correct for 8-Rock Ends?

In a standard 8-rock end:
- **Rocks 0-3:** Lead players (2 per team)
- **Rocks 4-7:** Second players (2 per team)
- **Rocks 8-11:** Third players (2 per team)
- **Rocks 12-15:** Skip players (2 per team)

**Typical Curling Phases:**
- **Early:** Rocks 0-3 (Leads) - Setup guards, feel out ice
- **Middle:** Rocks 4-9 (Seconds + start Thirds) - Build the end
- **Late:** Rocks 10-15 (End Thirds + Skips) - Critical decisions

**Verdict:** ? Phase definitions are reasonable

But could be improved:
```csharp
// More aligned with player positions
if (rockCurrent < 4)      phase = "early";   // Leads
else if (rockCurrent < 12) phase = "middle"; // Seconds + Thirds
else                       phase = "late";   // Skips only
```

---

## Strategy Selection Logic Review

### Current Decision Tree (Even Rocks - Not Hammer)

**2+ Ends Left:**
```
Ahead by 2+: AggressiveNotHammer     ? Makes sense - force opponent
Tied/Behind: ConservativeStealOrBlank ? Should distinguish tied vs behind
Behind by 1: ConservativeSteal        ? Makes sense
```

**1 End Left:**
```
Within 1 point: ConservativeStealOrBlank  ? Correct - must steal
Ahead by 2+: AggressiveNotHammer          ? Correct - protect lead
```

**Last End (0 ends left):**
```
Behind: ConservativeStealOrBlank  ? Must steal
Ahead: AggressiveNotHammer        ? Run out clock
```

### Current Decision Tree (Odd Rocks - Hammer)

**2+ Ends Left:**
```
Behind: AggressiveHammer                   ? Try to score big
Tied/Ahead: ConservativeScoreTwoOrBlank   ? Protect hammer
```

**1 End Left:**
```
Within 1 point: ConservativeScoreTwoOrBlank  ? Need 2+ to win/tie
Ahead by 2+: AggressiveHammer                 ?? Questionable - should be conservative
```

**Last End:**
```
Behind: ConservativeScoreTwoOrBlank  ? Should be Aggressive if behind
Tied/Ahead: AggressiveHammer          ? Backwards - should be Conservative
```

### Issues in Hammer Logic (Last End)

**Lines 224-230:**
```csharp
else  // Last end (gm.endTotal - gm.endCurrent == 0)
{
    if (activeTeamScore < oppTeamScore)
        ConservativeScoreTwoOrBlankHammer(rockCurrent, phase);  // ? Should be Aggressive!
    else
        AggressiveHammer(rockCurrent, phase);  // ? Should be Conservative!
}
```

**Problem:** This is backwards!
- **Behind in last end WITH hammer:** Need to score multiple points ? **Aggressive**
- **Ahead in last end WITH hammer:** Just need to score 1 ? **Conservative**

---

## Recommended Logic Fixes

### Fix #1: Correct OnShot() Not Hammer Selection

```csharp
if (rockCurrent % 2 == 0)
{
    if (gm.endTotal - gm.endCurrent >= 2)
    {
        if (activeTeamScore - oppTeamScore >= 2)
            AggressiveNotHammer(rockCurrent, phase);
        else if (activeTeamScore <= oppTeamScore)  // ? ADD "else if"
            ConservativeStealOrBlank(rockCurrent, phase);
        else
            ConservativeSteal(rockCurrent, phase);
    }
    else if (gm.endTotal - gm.endCurrent == 1)
    {
        if (activeTeamScore - oppTeamScore <= 1)
            ConservativeStealOrBlank(rockCurrent, phase);
        else
            AggressiveNotHammer(rockCurrent, phase);
    }
    else  // Last end
    {
        if (activeTeamScore < oppTeamScore)
            ConservativeStealOrBlank(rockCurrent, phase);
        else
            AggressiveNotHammer(rockCurrent, phase);
    }
}
```

### Fix #2: Correct OnShot() Hammer Selection (Last End)

```csharp
else  // Last end
{
    if (activeTeamScore < oppTeamScore)
        AggressiveHammer(rockCurrent, phase);  // ? SWAP - need big score when behind
    else
        ConservativeScoreTwoOrBlankHammer(rockCurrent, phase);  // ? SWAP - protect lead
}
```

### Fix #3: Fix Case Labels in ConservativeSteal

```csharp
case "middle":  // Remove "hammer"
    // ...
    break;
    
// Later...

case "late":  // Remove "hammer"  
    // ...
    break;
```

### Fix #4: Add Bounds Checking

```csharp
// Before any access to gm.houseList[1]:
if (gm.houseList.Count > 1)
{
    // Safe to access [1]
    var secondRock = gm.houseList[1];
}
```

### Fix #5: Initialize activeTeamName in SimpleAIShoot

```csharp
public void SimpleAIShoot(int rockCurrent)
{
    // Determine active team based on rock number and hammer
    if (rockCurrent % 2 == 0)
    {
        activeTeamName = gm.redHammer ? gm.yellowTeamName : gm.redTeamName;
    }
    else
    {
        activeTeamName = gm.redHammer ? gm.redTeamName : gm.yellowTeamName;
    }
    
    int valuableRockIndex = GetMostValuableOpponentRockIndex(activeTeamName);
    // ... rest of method
}
```

---

## Testing Recommendations

After fixes, test these scenarios:

### Test 1: Phase Case Matching
- Play through full end (16 rocks)
- Check console for "Conservative Steal - middle" and "Conservative Steal - late"
- Should now appear (currently doesn't due to case mismatch)

### Test 2: Strategy Selection
- Set breakpoints in each strategy method
- Verify only ONE strategy executes per shot
- Check that correct strategy is chosen based on score/hammer

### Test 3: Array Bounds
- Test with only 1 rock in house
- Should not crash (currently might)

### Test 4: Simple AI
- Test SimpleAIShoot with opponent rocks in house
- Verify it finds and targets them (currently doesn't due to null activeTeamName)

---

## Would You Like Me To Fix These Issues?

I can fix all these bugs in priority order:
1. ? Critical bugs (case labels, dual execution, initialization)
2. ? High priority (bounds checking, unreachable code)
3. ? Medium priority (helper method usage, bitwise vs logical OR)

This will make the AI strategy logic **actually work correctly** and be much more readable.

**Ready to proceed with fixes?**
