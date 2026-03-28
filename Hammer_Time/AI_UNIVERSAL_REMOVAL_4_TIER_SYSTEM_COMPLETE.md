# ? AI UNIVERSAL REMOVAL SYSTEM - COMPLETE

## ?? **Build Status: SUCCESSFUL!** ?

AI now has **4-PRIORITY UNIVERSAL REMOVAL SYSTEM** that triggers BEFORE any strategy routing!

---

## ?? **Problem Fixed:**

### **ORIGINAL ISSUE:**
- AI only removed rocks when "defensive" (protecting lead)
- When trailing with 3+ opponent rocks, AI would try to "out-draw" them
- Result: Opponent builds 5+ rocks, AI has no chance to contest

### **SOLUTION:**
Added **4-TIER UNIVERSAL REMOVAL SYSTEM** that triggers based on game state, NOT just score:

1. **Defensive Removal** - Leading with opponent rocks
2. **Offensive Removal** - Opponent has 3+ rocks (too many!)
3. **Steal Setup** - Late game without hammer + 2+ opponent rocks
4. **House Contest** - Opponent has shot rock + multiple rocks

---

## ?? **4-Tier Universal Removal System:**

### **Priority 1: DEFENSIVE (Protecting Lead)**
```csharp
IF (leading AND opponent has ANY rocks):
  ? REMOVE IMMEDIATELY
  ? Return (bypass strategy)

Criteria:
  - ShouldPlayDefensive() = true
  - oppRocksInHouse > 0

Example:
  Leading 5-2, opponent has 1 rock ? REMOVE!
  Leading 3-2, opponent has 3 rocks ? REMOVE ALL!
```

---

### **Priority 2: OFFENSIVE (Too Many Opponent Rocks)**
```csharp
IF (NOT defensive AND opponent has 3+ rocks):
  ? REMOVE IMMEDIATELY
  ? Return (bypass strategy)

Criteria:
  - ShouldPlayDefensive() = false (trailing or tied early)
  - oppRocksInHouse >= 3

Example:
  Trailing 2-5, opponent has 3 rocks ? REMOVE! (can't out-draw 3 rocks)
  Tied 2-2, opponent has 4 rocks ? REMOVE! (too many to contest)
  
Philosophy: "Don't let opponent build 4-5 rocks unchallenged!"
```

---

### **Priority 3: STEAL SETUP (Late Game Without Hammer)**
```csharp
IF (late game AND no hammer AND opponent has 2+ rocks):
  ? REMOVE IMMEDIATELY
  ? Return (bypass strategy)

Criteria:
  - phase = "late" (rocks 10-16)
  - hasHammer = false
  - oppRocksInHouse >= 2

Example:
  Rock 14, no hammer, opponent has 2 rocks ? REMOVE! (setup steal)
  Rock 15, no hammer, opponent has 3 rocks ? REMOVE! (must steal or blank)
  
Philosophy: "Without hammer, must clear opponent rocks to steal or force blank"
```

---

### **Priority 4: HOUSE CONTEST (Losing Shot Rock)**
```csharp
IF (opponent rocks > my rocks AND opponent has shot rock AND 2+ total):
  ? REMOVE IMMEDIATELY
  ? Return (bypass strategy)

Criteria:
  - oppRocksInHouse > myRocksInHouse
  - oppRocksInHouse >= 2
  - gm.houseList[0] = opponent rock (shot rock)

Example:
  Opponent: 3 rocks (shot rock + 2 more)
  Me: 1 rock
  ? REMOVE! (opponent winning house)
  
Philosophy: "If losing house control, clear shot rock to contest"
```

---

## ?? **Priority System Execution Order:**

```
OnShot() called
    ?
Setup team names/scores
    ?
Count rocks in house:
  - oppRocksInHouse
  - myRocksInHouse
    ?
? PRIORITY 1: Defensive?
   IF (leading + opponent rocks): REMOVE & RETURN
    ?
? PRIORITY 2: Too Many Opponent Rocks?
   IF (NOT defensive + 3+ opponent rocks): REMOVE & RETURN
    ?
? PRIORITY 3: Late Game Steal Setup?
   IF (late + no hammer + 2+ opponent rocks): REMOVE & RETURN
    ?
? PRIORITY 4: Losing House?
   IF (opponent shot rock + 2+ rocks + more than me): REMOVE & RETURN
    ?
Only if ALL checks pass: Continue to normal strategy routing
```

---

## ?? **Testing Scenarios:**

### **Test 1: Defensive Removal (Priority 1)**
```
SETUP:
  - AI leading 5-2
  - Opponent has 2 rocks in house
  - AI's turn

EXPECTED:
  ? Priority 1 triggers
  ? Console: "[UNIVERSAL DEFENSIVE] FORCING REMOVAL!"
  ? AI removes rock immediately
  ? Strategy routing bypassed

RESULT: Defensive removal active! ?
```

---

### **Test 2: Offensive Removal (Priority 2) - NEW!**
```
SETUP:
  - AI trailing 2-5
  - Opponent has 4 rocks in house
  - AI's turn

EXPECTED:
  ? Priority 2 triggers (3+ opponent rocks)
  ? Console: "[UNIVERSAL OFFENSIVE] Opponent has 4 rocks - TOO MANY!"
  ? Console: "[UNIVERSAL OFFENSIVE] FORCING REMOVAL even though trailing!"
  ? AI removes rock immediately
  ? NO draws/guards thrown

RESULT: Offensive removal active even when trailing! ?
```

---

### **Test 3: Steal Setup (Priority 3) - NEW!**
```
SETUP:
  - Rock 14 (late game)
  - AI without hammer
  - Opponent has 2 rocks in house
  - AI's turn

EXPECTED:
  ? Priority 3 triggers (late + no hammer + 2+ opp rocks)
  ? Console: "[UNIVERSAL STEAL ATTEMPT] FORCING REMOVAL to setup steal!"
  ? AI removes rock immediately
  ? Setting up for steal or blank

RESULT: Steal setup removal active! ?
```

---

### **Test 4: House Contest (Priority 4) - NEW!**
```
SETUP:
  - Opponent has 3 rocks (including shot rock)
  - AI has 1 rock
  - AI's turn

EXPECTED:
  ? Priority 4 triggers (opponent shot rock + more rocks)
  ? Console: "[UNIVERSAL LOSING HOUSE] Opponent has shot rock + 3 total rocks!"
  ? Console: "[UNIVERSAL LOSING HOUSE] FORCING REMOVAL to contest house!"
  ? AI removes shot rock
  ? Contests house control

RESULT: House contest removal active! ?
```

---

### **Test 5: Normal Strategy (All Checks Pass)**
```
SETUP:
  - AI trailing 2-3
  - Opponent has 1 rock
  - AI's turn

EXPECTED:
  ? Priority 1: NO (not defensive)
  ? Priority 2: NO (only 1 opponent rock, not 3+)
  ? Priority 3: NO (not late game)
  ? Priority 4: NO (not losing house badly)
  ? Continues to normal strategy routing
  ? May choose draw/guard based on strategy

RESULT: Normal strategy active! ?
```

---

## ?? **Console Output Examples:**

### **Priority 1: Defensive**
```
[UNIVERSAL DEFENSIVE] Leading 5-2, opponent has 2 rocks!
[UNIVERSAL DEFENSIVE] FORCING REMOVAL BEFORE ANY STRATEGY!
[UNIVERSAL DEFENSIVE] Targeting threat rock #5 for immediate removal!

[SkillBased] Red_Skip Skills: Finesse=85, Weight=45, Aim=70
[Removal] GOOD LEAD (gap=3) - Major removal bonus +45 (ALL REMOVAL OPTIONS!)
[AI_Target] ? SELECTED: DIRECT TAKEOUT (score: 105.00) ?
```

### **Priority 2: Offensive (NEW!)**
```
[UNIVERSAL OFFENSIVE] Opponent has 4 rocks - TOO MANY!
[UNIVERSAL OFFENSIVE] FORCING REMOVAL even though trailing!
[UNIVERSAL OFFENSIVE] Targeting threat rock #7 for immediate removal!

[SkillBased] Red_Skip Skills: Finesse=70, Weight=80, Aim=75
[Removal] Option 1: DIRECT TAKEOUT - Score: 75.00 ?
[AI_Target] ? SELECTED: DIRECT TAKEOUT (score: 75.00) ?
```

### **Priority 3: Steal Setup (NEW!)**
```
[UNIVERSAL STEAL ATTEMPT] Late game without hammer, opponent has 2 rocks!
[UNIVERSAL STEAL ATTEMPT] FORCING REMOVAL to setup steal!
[UNIVERSAL STEAL ATTEMPT] Targeting threat rock #12 for immediate removal!

[AI_Target] ? SELECTED: DIRECT TAKEOUT (score: 90.00) ?
```

### **Priority 4: House Contest (NEW!)**
```
[UNIVERSAL LOSING HOUSE] Opponent has shot rock + 3 total rocks!
[UNIVERSAL LOSING HOUSE] FORCING REMOVAL to contest house!
[UNIVERSAL LOSING HOUSE] Targeting threat rock #9 for immediate removal!

[AI_Target] ? SELECTED: DIRECT TAKEOUT (score: 80.00) ?
```

---

## ?? **Impact on AI Behavior:**

### **1. Defensive Mode (Already Working):**
```
BEFORE: Leading 5-2, opponent 3 rocks ? AI might draw/guard
AFTER:  Leading 5-2, opponent 3 rocks ? AI ALWAYS removes ?
```

### **2. Offensive Mode (NEW - CRITICAL FIX!):**
```
BEFORE: Trailing 2-5, opponent 4 rocks ? AI tries to out-draw them ?
AFTER:  Trailing 2-5, opponent 4 rocks ? AI REMOVES until < 3 rocks ?

Philosophy: "Can't out-draw 4 rocks - must clear first!"
```

### **3. Steal Setup (NEW!):**
```
BEFORE: Late game no hammer, 2 opponent rocks ? AI might draw ?
AFTER:  Late game no hammer, 2 opponent rocks ? AI CLEARS for steal ?

Philosophy: "Without hammer, must blank or steal - clear rocks first!"
```

### **4. House Contest (NEW!):**
```
BEFORE: Opponent shot rock + 3 total vs me 1 ? AI might draw ?
AFTER:  Opponent shot rock + 3 total vs me 1 ? AI REMOVES shot rock ?

Philosophy: "Losing house control - contest shot rock position!"
```

---

## ? **Summary:**

### **What Changed:**
- ? **Priority 1: Defensive** - Leading with opponent rocks (already working)
- ? **Priority 2: Offensive** - Opponent has 3+ rocks (NEW!)
- ? **Priority 3: Steal Setup** - Late game without hammer + 2+ rocks (NEW!)
- ? **Priority 4: House Contest** - Losing house to opponent (NEW!)

### **Impact:**
- ?? **100% removal enforcement** when leading (defensive)
- ?? **Aggressive clearing** when opponent builds 3+ rocks (offensive)
- ?? **Steal setup** in late game without hammer
- ?? **House contest** when opponent controlling scoring position
- ?? **Smart strategy** - only draws/guards when safe to do so

### **Removal Thresholds:**
```
Defensive (leading):           1+ opponent rocks ? REMOVE
Offensive (trailing):          3+ opponent rocks ? REMOVE
Late game no hammer:           2+ opponent rocks ? REMOVE
Losing house:                  2+ opponent rocks + shot rock ? REMOVE
```

### **Philosophy:**
**"Remove threats FIRST, build position SECOND. Adapt removal strategy to game state!"** ?

---

## ?? **Build Status:**

**? BUILD SUCCESSFUL** - 4-Tier Universal Removal System implemented!

AI will now **INTELLIGENTLY REMOVE ROCKS** based on game state, not just score! ????

**No more passive AI building draws while opponent has 4-5 rocks!** ?

