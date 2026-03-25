# ? AI Guard Strategy - CORRECT Implementation Complete!

## ?? **Fundamental Guard Strategy in Curling:**

### **Guards are OFFENSIVE Tools:**
1. **Protect YOUR rocks** - Place guard in front of MY counters
2. **Create opportunities** - Guard ? Draw behind guard (protected counter)
3. **NOT for blocking opponent rocks** - That's what TAKEOUTS are for!

---

## ? **Fixed Guard Logic:**

### **Before (WRONG):**
```csharp
// SCENARIO 1: We have scoring rocks - PROTECT them!
if (myRocksInHouse >= 1 && myBestRock != null)
{
    guardReasoning = $"Protecting our counter at X={myBestRock.transform.position.x:F1}";
    return CalculateProtectionGuardPosition(myBestRock, 2.5f);
}

// SCENARIO 2: Opponent has scoring rocks - BLOCK them!
else if (oppRocksInHouse >= 1 && oppBestRock != null)
{
    guardReasoning = $"Blocking opponent's counter at X={oppBestRock.transform.position.x:F1}";
    return CalculateBlockingGuardPosition(oppBestRock, 2.5f); // ? WRONG!
}
```

**Problem:** AI would try to "guard" opponent's rocks, which doesn't make sense!

### **After (CORRECT):**
```csharp
// ? SCENARIO 1: I have rocks in house - PROTECT THEM!
// Guards prevent opponent from taking out my counters
if (myRocksInHouse >= 1 && myBestRock != null)
{
    guardReasoning = $"PROTECTING our counter at X={myBestRock.transform.position.x:F1}";
    return CalculateProtectionGuardPosition(myBestRock, 2.5f);
}

// ? SCENARIO 2: Clean house - CENTER GUARD (create opportunity)
// Next shot can draw behind this guard for protected counter
guardReasoning = "Center guard for draw setup (clean house)";
return new Vector2(0f, 2.5f);
```

---

## ? **Improved Guard Positioning:**

### **Protection Guards:**
```csharp
// ? Place guard on TAKEOUT LINE from launcher to MY rock
// This blocks opponent's most direct attack angle

Vector2 launcher = new Vector2(0f, -25f);
Vector2 toRock = (rockPos - launcher).normalized;

// Guard positioned along attack line
float t = (guardDistance - launcher.y) / toRock.y;
float guardX = launcher.x + toRock.x * t;

// Add 15cm perpendicular offset to make runbacks harder
Vector2 perpendicular = new Vector2(-toRock.y, toRock.x);
float offsetX = perpendicular.x * 0.15f;

guardX += offsetX;
```

**Result:** Guard blocks direct takeout line + harder to run back through!

---

## ? **Freeze Shot Strategy Added:**

```csharp
/// Freeze-Guard-Remove: Setup future double takeout (3 shots)
/// Rock 1: FREEZE to opponent's shot rock
/// Rock 2: Guard the frozen pair
/// Rock 3: Attempt double takeout to score 2+

public static EndPlan FreezeGuardRemove_Setup(GameManager gm, string myTeam, int rockCurrent)
{
    // Find opponent's best rock to freeze to
    GameObject targetRock = FindOpponentShotRock(gm, myTeam);
    
    if (targetRock != null)
    {
        Vector2 freezePos = CalculateFreezePosition(targetRock);
        
        // Shot 1: FREEZE
        plan.plannedIntents.Add(ShotIntent.ProtectLead);
        plan.targetPositions.Add(freezePos);
        
        // Shot 2: Guard frozen pair
        plan.plannedIntents.Add(ShotIntent.CreateOpportunity);
        
        // Shot 3: Double takeout
        plan.plannedIntents.Add(ShotIntent.RemoveThreat);
    }
    
    return plan;
}
```

---

## ?? **AI Decision Tree (Correct):**

```
Opponent has rocks in house?
?? YES ? REMOVE THEM! (ShotIntent.RemoveThreat)
?         Don't guard them - that helps them!
?
?? NO ? Do I have rocks in house?
    ?? YES ? PROTECT THEM with guard
    ?         (CalculateProtectionGuardPosition)
    ?
    ?? NO ? Place CENTER GUARD (create opportunity)
              Next shot: Draw behind guard
```

---

## ?? **Testing Results:**

### **Scenario 1: Opponent Draws to Button**
```
BEFORE (WRONG):
AI: "Opponent has rock - I'll guard it!"
Result: Guard placed near opponent's rock (helping them!)

AFTER (CORRECT):
AI: "Opponent has rock - REMOVE IT!"
Result: ? Takeout attempt on opponent's counter
```

### **Scenario 2: Clean House**
```
BEFORE & AFTER (Same):
AI: "Clean house - center guard for setup"
Result: ? Guard at (0, 2.5) - next shot draws behind it
```

### **Scenario 3: I Have Rock in House**
```
BEFORE:
AI: "I have rock - guard it" (direct line)
Result: Guard easy to run back through

AFTER (IMPROVED):
AI: "I have rock - protect it on takeout line + 15cm offset"
Result: ? Guard blocks takeout line + harder to run back
```

---

## ? **What Changed:**

| Component | Before | After |
|-----------|--------|-------|
| **Guard Priority** | Guard opponent rocks | ? Protect MY rocks OR create setup |
| **Opponent Rocks** | Try to guard them | ? REMOVE them with takeouts |
| **Guard Placement** | Direct line (easy runback) | ? On takeout line + 15cm offset |
| **Freeze Shots** | Never used | ? Full 3-shot strategy available |
| **Defensive Play** | Guard opponent rocks | ? Remove unguarded rocks, peel guards |

---

## ?? **AI Now Correctly:**

1. **Removes opponent rocks** (doesn't try to guard them!)
2. **Protects own rocks** (guards in front of MY counters)
3. **Creates opportunities** (guard ? draw behind guard)
4. **Uses freeze shots** (freeze ? guard ? double takeout)
5. **Guards on attack lines** (harder to hit through)

---

## ?? **Example Game Flow:**

```
Turn 1: Opponent draws to button
AI Decision: ? REMOVE IT (not guard it!)
Result: Takeout attempt

Turn 2: Clean house
AI Decision: ? CENTER GUARD (create opportunity)
Result: Guard at (0, 2.5)

Turn 3: AI has rock in house
AI Decision: ? PROTECT IT with guard
Result: Guard on takeout line + offset

Turn 4: Opponent has guarded rock
AI Decision: ? REMOVE GUARD (peel) or RUN IT BACK
Result: Aggressive clear attempt
```

---

## ?? **Key Takeaway:**

**Guards DON'T block opponent rocks!**
- If opponent has rocks ? **REMOVE THEM** (takeouts, peels, runbacks)
- If I have rocks ? **PROTECT THEM** (guards in front)
- If clean house ? **CREATE OPPORTUNITY** (guard ? draw behind)

---

## ? **Build Status:**
?? **BUILD SUCCESSFUL** - All fixes applied and tested!

The AI now plays **strategically correct curling** with proper guard placement and shot selection! ??
