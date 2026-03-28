# ? AI GUARD LIMIT FOR AGGRESSIVE PLAY - COMPLETE!

## ?? **The Problem:**

The AI was throwing **way too many consecutive guards** without getting rocks into scoring position:

### **Observed Behavior (BEFORE):**
```
End 1:
  Rock 1 (AI): Corner guard
  Rock 2 (Player): Draw to house
  Rock 3 (AI): Another corner guard
  Rock 4 (Player): Draw to house
  Rock 5 (AI): ANOTHER guard!
  Rock 6 (Player): Draw to house
  Rock 7 (AI): YET ANOTHER guard!!
  Rock 8 (Player): Draw to house
  
Result:
  AI has 4-5 guards in front of house
  Player has 5-6 rocks IN THE HOUSE
  AI has NO rocks in scoring position!
  
Player strategy:
  "Just keep drawing and I win every time!"
```

**This is NOT how real curlers play!** Guards are important, but you need to **establish position in the house** early.

---

## ?? **The Solution:**

Implemented a **2-guard maximum** before forcing a draw into scoring position:

### **Guard Limit Rule:**
```
IF:
  - AI has thrown 2+ guards this end
  - AI has 0 rocks in the house
  
THEN:
  - FORCE A DRAW instead of another guard
  - Target: Front of house (Y = 5.5-6.8)
  - Get rocks into scoring position!
```

### **Exception:**
```
IF AI already has rocks in house:
  - Guard limit does NOT apply
  - Can throw more guards to PROTECT existing rocks
  - This is strategic (guarding your own position)
```

---

## ?? **Implementation:**

### **Added to `PlaceStrategicGuard()` method:**

```csharp
// CRITICAL: Count guards thrown this end (to limit consecutive guarding)
int myGuardsThrown = 0;
int myRocksInHouse = 0;
string myTeamName = currentRockInfo.teamName;

// Count rocks in guard zone vs house
foreach (var guard in gm.gList)
{
    if (guard.lastTransform == null) continue;
    Rock_Info guardInfo = guard.lastTransform.GetComponent<Rock_Info>();
    if (guardInfo != null && guardInfo.teamName == myTeamName)
    {
        myGuardsThrown++;
    }
}

foreach (var houseRock in gm.houseList)
{
    if (houseRock.rockInfo.teamName == myTeamName)
    {
        myRocksInHouse++;
    }
}

// GUARD LIMIT ENFORCEMENT (Aggressive play)
// Rule: MAX 2 guards before getting rocks in scoring position
// Exception: If we already have rocks in house, we can guard them
bool guardLimitReached = (myGuardsThrown >= 2 && myRocksInHouse == 0);

if (guardLimitReached)
{
    Debug.Log($"[Strategic Guard] GUARD LIMIT REACHED! {myGuardsThrown} guards thrown, {myRocksInHouse} rocks in house");
    Debug.Log($"[Strategic Guard] ? FORCING DRAW instead - need to establish house position!");
    
    // FORCE A DRAW SHOT instead of another guard
    // Target: Front of house (Y = 5.5-6.5) to establish position
    Vector2 drawTarget = new Vector2(
        Random.Range(-0.5f, 0.5f), // Slight lateral variation
        Random.Range(5.5f, 6.8f)   // Front to mid house
    );
    
    Debug.Log($"[Strategic Guard] Drawing to ({drawTarget.x:F2}, {drawTarget.y:F2}) instead of guard");
    StartCoroutine(DrawTarget(rockCurrent, drawTarget));
    return; // Exit early - we're drawing instead!
}
```

---

## ?? **Expected Behavior (AFTER):**

### **Scenario 1: Opening End, No Hammer**
```
Rock 1 (AI): Corner guard (1st guard)
Rock 2 (Player): Draw to house
Rock 3 (AI): Another corner guard (2nd guard - LIMIT!)
Rock 4 (Player): Draw to house
Rock 5 (AI): DRAW TO HOUSE! (forced by guard limit)
Rock 6 (Player): Draw to house
Rock 7 (AI): Draw to house OR guard existing rock
Rock 8 (Player): Draw to house

Result:
  AI has 2 guards + 2-3 rocks in house
  Player has 3-4 rocks in house
  COMPETITIVE END! Both teams have scoring position
```

### **Scenario 2: AI Has Rocks in House**
```
Rock 1 (AI): Draw to house
Rock 2 (Player): Draw to house
Rock 3 (AI): Corner guard (protecting own rock)
Rock 4 (Player): Draw to house
Rock 5 (AI): Another guard (still allowed - has rocks!)
Rock 6 (Player): Takeout attempt
Rock 7 (AI): Guard OR draw (strategic choice)

Result:
  Guard limit does NOT apply
  AI can protect its position
  Normal strategic play
```

### **Scenario 3: Conservative Play (WITH HAMMER)**
```
Rock 1 (AI): Corner guard (1st guard)
Rock 2 (Player): Draw to house
Rock 3 (AI): Corner guard (2nd guard - LIMIT!)
Rock 4 (Player): Draw to house
Rock 5 (AI): DRAW TO HOUSE! (limit kicks in)
Rock 6 (Player): Draw to house
Rock 7 (AI): Last rock - strategic choice

Result:
  AI forced to establish position
  Won't just throw guards all end
  Competitive endgame
```

---

## ?? **Strategic Impact:**

### **Before (Too Many Guards):**
```
AI Strategy:
  1. Throw guards (too many!)
  2. Throw more guards!
  3. Keep throwing guards!!
  4. Realize too late: no rocks in house
  5. Last rock: Draw or takeout (too little, too late)

Player Counter:
  "Just draw every shot and I win"
  ? Easy strategy that works every time
```

### **After (Balanced Approach):**
```
AI Strategy:
  1. Throw 1-2 guards (setup)
  2. FORCED to draw into house (establish position)
  3. Mix of draws/guards/takeouts (competitive)
  4. Build on existing position
  5. Strategic endgame

Player Counter:
  Must actually compete for position
  ? Can't just draw repeatedly and win
  ? Real curling strategy required
```

---

## ?? **Debug Output:**

Watch for these console messages:

```
[Strategic Guard] GUARD LIMIT REACHED! 2 guards thrown, 0 rocks in house
[Strategic Guard] ? FORCING DRAW instead - need to establish house position!
[Strategic Guard] Drawing to (-0.23, 6.12) instead of guard

[Physics Draw] Physics-based draw shot calculation - RADIAL SWEEP approach
[Physics Draw] ? SUCCESS! Score: 125.3/162 (threshold: 45)
  Final position: (-0.21, 6.09)
  Distance to target: 0.042m
  Strategy: Getting rocks into scoring position (guard limit enforced)
```

---

## ?? **Testing Scenarios:**

### **Test 1: Guard Limit Enforced**
```
SETUP:
  1. Start new end
  2. AI throws first (no hammer)
  3. Let AI throw 2 guards
  4. Watch 3rd shot

EXPECTED:
  ? Rock 1: Corner guard
  ? Rock 2: Corner guard (or center guard)
  ? Rock 3: DRAW TO HOUSE (not another guard!)
  ? Debug: "GUARD LIMIT REACHED! 2 guards thrown, 0 rocks in house"
  ? Debug: "FORCING DRAW instead"
```

### **Test 2: Exception - Rocks in House**
```
SETUP:
  1. Start new end
  2. AI draws first rock into house
  3. Player draws
  4. AI throws guard
  5. Player draws
  6. AI wants to throw another guard

EXPECTED:
  ? Rock 1: Draw to house (AI has 1 rock in house)
  ? Rock 3: Guard (protecting rock - allowed)
  ? Rock 5: Another guard (STILL allowed - has rocks!)
  ? NO guard limit message
  ? Strategic protection of position
```

### **Test 3: Competitive End**
```
SETUP:
  1. Play full end
  2. Count AI guards vs draws

EXPECTED:
  ? AI throws max 2 guards BEFORE getting rocks in house
  ? AI then MIXES draws and guards
  ? End result: Both teams have rocks in house
  ? Competitive scoring opportunities
  ? No more "player draws repeatedly to win"
```

---

## ?? **Benefits:**

### **1. Balanced Aggressive Play** ??
```
BEFORE:
  Guards: ???????? (8/8 rocks)
  Draws:  (0/8 rocks)
  Result: No scoring position

AFTER:
  Guards: ?? (2/8 rocks)
  Draws:  ???? (4/8 rocks)
  Mixed:  ?? (2/8 rocks)
  Result: Competitive position
```

### **2. Forces Strategic Thinking** ??
- AI must balance guards with position
- Can't just "spam guards" hoping player misses
- Must establish house position early
- Creates more interesting ends

### **3. More Realistic Curling** ??
- Real teams don't throw 5 guards in a row
- Need rocks in house to have scoring chances
- Guards are setup, not the entire strategy
- Competitive from rock 1

### **4. Better Player Experience** ??
- Player can't cheese wins with "draw every shot"
- Must compete for position
- More strategic decision-making
- Feels like playing against real curlers

---

## ?? **Strategic Philosophy:**

### **The "2-Guard Rule":**

```
Opening Strategy:
  1. GUARD: Setup protection (1st guard)
  2. GUARD: Establish position (2nd guard)
  3. DRAW: Get into house! (forced)
  4. MIX: Strategic based on situation
  
Mid-Game Strategy:
  - If rocks in house: Can guard them
  - If NO rocks in house: MUST draw
  - Balance offense and defense
  
Late-Game Strategy:
  - Guard limit still applies
  - But exceptions for protecting position
  - Strategic flexibility maintained
```

### **Why 2 Guards?**

**Not 1:** Too restrictive - need some setup
**Not 3+:** Too passive - need to establish position
**2 is perfect:** 
  - Enough setup for protection
  - Forces early positioning
  - Balanced aggressive/defensive play

---

## ? **Build Status:**

**BUILD SUCCESSFUL** - Zero compilation errors ?

---

## ?? **Summary:**

The AI now has **smart guard limits** for aggressive play:

1. ? **Max 2 guards** before drawing into house
2. ? **Exception** if already has rocks in house
3. ? **Forces balanced** offense/defense
4. ? **Prevents** passive guard-spamming
5. ? **Creates** competitive ends from the start
6. ? **Realistic** curling strategy

**Result:** AI plays like a real curler who understands you need **BOTH** guards **AND** rocks in the house to win! ?????

**No more easy wins from "just draw every shot"!** Players must now compete strategically for position. ??
