# ?? AI Shot Selection Rebalancing - Takeouts > Guards

## ?? **Problem Identified:**

The AI is currently:
- ? **Over-using guards** (placing guards when should be removing threats)
- ? **Over-using draws** (drawing when should be taking out)
- ? **Under-using takeouts** (threats not being removed aggressively)
- ? **Freeze shots never used properly** (in library but not in plans)

---

## ? **Correct Shot Priority:**

### **1. Threat Assessment (Every Turn)**
```
IF opponent has rocks in house:
  ? PRIORITY: Remove threat (takeout)
  
IF opponent has NO rocks AND I have rocks:
  ? PRIORITY: Protect my rocks (guard in front)
  
IF clean house:
  ? OPTION A: Guard + Draw (setup)
  ? OPTION B: Draw to button (aggressive)
```

### **2. Skill-Based Shot Selection**

#### **High Weight/Aim Team (Power Game):**
- ? **Takeouts** - Direct removal
- ? **Heavy draws** - Power through guards
- ? **Guards** - Protect counters
- ? **Freeze/Runback** - Too finesse-heavy

#### **High Finesse Team (Finesse Game):**
- ? **Freeze shots** - Stick to opponent rocks
- ? **Runbacks** - Hit through guards
- ? **Tick shots** - Precision angles
- ? **Corner guards** - Split house

#### **Balanced Team:**
- ? **Mix of all shots**
- ? **Situational decisions**

---

## ?? **Strategy Fixes Needed:**

### **Fix 1: Remove "Guard First" Logic**

**Current (WRONG):**
```csharp
// EARLY PHASE
if (phase == "early")
{
    // Always place guard - TOO PASSIVE!
    return ExecuteShot(ShotIntent.CreateOpportunity, -1, rockCurrent);
}
```

**Fixed (CORRECT):**
```csharp
// EARLY PHASE
if (phase == "early")
{
    // PRIORITY 1: Remove any threats FIRST
    if (house.threatRock >= 0)
    {
        return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent);
    }
    
    // PRIORITY 2: If I have rocks, guard them
    if (house.myRocksInHouse >= 1)
    {
        return ExecuteShot(ShotIntent.CreateOpportunity, -1, rockCurrent); // Guard my rocks
    }
    
    // PRIORITY 3: Clean house - setup OR aggressive draw
    // Decision based on strategy type
    if (isAggressiveStrategy)
    {
        return ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent); // Draw to button
    }
    else
    {
        return ExecuteShot(ShotIntent.CreateOpportunity, -1, rockCurrent); // Guard + draw next
    }
}
```

---

### **Fix 2: Skill-Based Shot Selection**

```csharp
/// <summary>
/// ? NEW: Select shot type based on team skills
/// High finesse ? fancy shots (freeze, runback)
/// High weight ? power shots (takeout, heavy draw)
/// </summary>
private ShotIntent GetSkillBasedIntent(CharacterStats shooter, ShotIntent baseIntent, HouseAnalysis house)
{
    if (shooter == null) return baseIntent;
    
    float finesseSkill = shooter.finesseAccuracy.GetValue();
    float weightSkill = shooter.weightAccuracy.GetValue();
    float aimSkill = shooter.aimAccuracy.GetValue();
    
    // HIGH FINESSE TEAM (70+) - Consider fancy shots
    if (finesseSkill >= 70f)
    {
        // Can we freeze to opponent's rock?
        if (house.oppRocksInHouse >= 1 && baseIntent == ShotIntent.ProtectLead)
        {
            // Freeze instead of guard (30% chance)
            if (Random.value < 0.3f)
            {
                Debug.Log("[SkillBased] High finesse ? FREEZE shot instead of guard");
                return ShotIntent.ProtectLead; // Reuse intent but target opponent rock
            }
        }
        
        // Can we attempt runback? (if guard blocking)
        if (gm.gList.Count > 0 && house.threatRock >= 0 && baseIntent == ShotIntent.RemoveThreat)
        {
            // Runback instead of peel (40% chance)
            if (Random.value < 0.4f)
            {
                Debug.Log("[SkillBased] High finesse ? RUNBACK shot instead of peel");
                return ShotIntent.RemoveThreat; // Same intent but different execution
            }
        }
    }
    
    // HIGH WEIGHT TEAM (70+) - Prefer power shots
    else if (weightSkill >= 70f && aimSkill >= 70f)
    {
        // Always take out threats (no hesitation)
        if (house.threatRock >= 0 && (baseIntent == ShotIntent.ProtectLead || baseIntent == ShotIntent.CreateOpportunity))
        {
            Debug.Log("[SkillBased] High weight ? TAKEOUT instead of guard");
            return ShotIntent.RemoveThreat;
        }
        
        // Prefer heavy draws over finesse
        if (baseIntent == ShotIntent.ProtectLead && house.myRocksInHouse == 0)
        {
            Debug.Log("[SkillBased] High weight ? HEAVY DRAW instead of guard");
            return ShotIntent.ScorePoints;
        }
    }
    
    return baseIntent; // Default - no change
}
```

---

### **Fix 3: Threat Removal Priority**

```csharp
/// <summary>
/// ? CRITICAL: Threat removal decision tree
/// This should be called BEFORE any guard placement logic
/// </summary>
private bool ShouldRemoveThreat(HouseAnalysis house, string phase, bool hasHammer)
{
    // NO THREATS - don't remove
    if (house.threatRock < 0) return false;
    
    // CRITICAL: Opponent has shot rock AND we're losing house
    if (!house.amWinningHouse && house.oppRocksInHouse >= 1)
    {
        return true; // ALWAYS remove when losing
    }
    
    // EARLY PHASE: Remove any opponent rocks immediately (don't let them build)
    if (phase == "early")
    {
        return true; // Aggressive early - clear everything
    }
    
    // MIDDLE PHASE: Remove if they have 2+ rocks (multi-point threat)
    if (phase == "middle" && house.oppRocksInHouse >= 2)
    {
        return true; // Don't let them build big end
    }
    
    // LATE PHASE WITHOUT HAMMER: Remove to steal
    if (phase == "late" && !hasHammer && house.oppRocksInHouse >= 1)
    {
        return true; // Need to clear to steal
    }
    
    // LATE PHASE WITH HAMMER: Remove if multiple threats
    if (phase == "late" && hasHammer && house.oppRocksInHouse >= 2)
    {
        return true; // Clear for big end
    }
    
    // DEFAULT: Threat exists but not urgent - guard instead
    return false;
}
```

---

## ?? **Shot Selection Priorities (By Situation):**

### **Situation 1: Opponent Has Rocks**
```
Priority 1: TAKEOUT (remove threat)
Priority 2: HEAVY DRAW (if high weight, bury their rock)
Priority 3: FREEZE (if high finesse, stick to their rock)
Priority 4: GUARD (only if I have rocks to protect)
```

### **Situation 2: I Have Rocks, Opponent Doesn't**
```
Priority 1: GUARD (protect my rocks)
Priority 2: DRAW (add more counters)
Priority 3: FREEZE (if high finesse, freeze to my rock)
```

### **Situation 3: Clean House**
```
Priority 1: DRAW TO BUTTON (aggressive)
Priority 2: GUARD + DRAW (setup, conservative)
Priority 3: CORNER GUARD (split house strategy)
```

---

## ?? **Implementation Plan:**

### **Step 1: Fix Early Phase Logic (All 5 Methods)**
- ? Change "guard first" to "remove threats first"
- ? Only guard if I have rocks to protect
- ? Clean house ? draw OR guard (skill-based)

### **Step 2: Add Skill-Based Shot Selection**
- ? High finesse ? Freeze shots (30% chance when applicable)
- ? High weight ? Takeouts always (no hesitation)
- ? Balanced ? Mix of strategies

### **Step 3: Remove Freeze from Default Plans**
- ? Freeze shots ONLY for high finesse teams
- ? NOT in default multi-shot plans
- ? Single-shot decision based on skills

### **Step 4: Update Multi-Shot Plans**
- ? GuardDrawProtect ? RemoveDrawProtect (if threats exist)
- ? GuardDrawDraw ? DrawDrawDraw (if no threats)
- ? Remove FreezeGuardRemove (too situational)

---

## ?? **Expected Behavior After Fix:**

### **Before (TOO PASSIVE):**
```
Turn 1: Opponent draws to button
AI: "I'll place a guard" ? WRONG!

Turn 2: Opponent has 2 rocks
AI: "I'll place another guard" ? WRONG!

Turn 3: Late game, losing
AI: "I'll draw behind my guard" ? TOO LATE!
```

### **After (AGGRESSIVE):**
```
Turn 1: Opponent draws to button
AI: "REMOVE IT!" ? Takeout

Turn 2: Clean house
AI: "Draw to button" OR "Guard + draw setup" ? Situational

Turn 3: I have 2 rocks
AI: "Guard my rocks" ? Protect lead
```

---

## ?? **Changes Needed:**

1. **ConservativeSteal** - Remove threat FIRST in early phase
2. **AggressiveHammer** - Always remove threats (no guards until threats cleared)
3. **ScoreTwoOrBlank** - Aggressive threat removal
4. **AggressiveNotHammer** - Takeout-heavy strategy
5. **StealOrBlank** - Remove threats, then guard

6. **Add `ShouldRemoveThreat()` helper** - Decision logic
7. **Add `GetSkillBasedIntent()` helper** - Finesse vs weight shots
8. **Remove `FreezeGuardRemove_Setup`** - Too specialized, use skill-based instead

---

## ?? **Testing After Fix:**

### **Test 1: Threat Removal**
```
Setup: Opponent has 1 rock in house
Expected: AI removes it (takeout)
Before: AI placed guard ?
After: AI takes out ?
```

### **Test 2: Guard Usage**
```
Setup: I have 2 rocks, opponent has 0
Expected: AI guards my rocks
Before: AI drew more rocks ?
After: AI guards ?
```

### **Test 3: Skill-Based**
```
Setup: High finesse team, opponent has rock
Expected: AI might freeze (30% chance)
Before: AI always guarded ?
After: AI tries freeze ?
```

---

## ? **Ready to Implement?**

This will fix the shot selection priorities to be:
1. **Remove threats FIRST** (takeouts)
2. **Guard situationally** (protect own rocks)
3. **Use skills for fancy shots** (finesse teams)
4. **Aggressive early, protective late** (phase-based)

Should I implement these fixes now?
