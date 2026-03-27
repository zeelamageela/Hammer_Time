# ? AI OPEN SIDE DRAW STRATEGY - COMPLETE!

## ?? **The Strategic Concept:**

In curling, when you already have rocks in the house, drawing to the **OPPOSITE SIDE** (open side) is a powerful strategic move:

### **Why Draw to the Open Side?**

1. **Score Multiple Points (WITH HAMMER):**
   - Spread rocks across both sides of the house
   - Harder for opponent to remove ALL your rocks
   - Increases chance of scoring 2+ points in one end

2. **Force Opponent or Steal (WITHOUT HAMMER):**
   - Force opponent to make difficult multi-rock removes
   - If they miss, you might steal points
   - Even if they remove one, others remain

3. **Strategic Depth:**
   - Makes you less predictable (not just stacking rocks in center)
   - Forces opponent to use multiple shots to clear house
   - Creates complex end scenarios

---

## ?? **Implementation:**

### **1. Open Side Detection**

When considering draw shots, the AI now:

```csharp
// Calculate average X position of existing friendly rocks
int myRocksInHouse = 0;
float myRocksAverageX = 0f;

foreach (var houseRock in gm.houseList)
{
    if (houseRock.rockInfo.teamName == currentRockInfo.teamName)
    {
        myRocksInHouse++;
        myRocksAverageX += houseRock.rock.transform.position.x;
    }
}

if (myRocksInHouse >= 1)
{
    myRocksAverageX /= myRocksInHouse;
    
    // Determine OPPOSITE side
    // If rocks are LEFT (negative X), open side is RIGHT (positive X)
    // If rocks are RIGHT (positive X), open side is LEFT (negative X)
    float openSideX = (myRocksAverageX < 0f) ? 0.8f : -0.8f;
}
```

### **2. Candidate Generation**

The AI adds **HIGH PRIORITY** open side candidates:

```csharp
// CANDIDATE 1: Direct to target (baseline)
candidateTargets.Add(targetPosition);

// CANDIDATE 2: OPEN SIDE DRAW (strategic spread!)
if (myRocksInHouse >= 1 && openSideIsClear)
{
    Vector2 openSideTarget = new Vector2(openSideX, targetPosition.y);
    candidateTargets.Insert(1, openSideTarget); // High priority!
    
    // Also add variations around open side
    candidateTargets.Add(new Vector2(openSideX + 0.2f, targetPosition.y));
    candidateTargets.Add(new Vector2(openSideX - 0.2f, targetPosition.y));
    candidateTargets.Add(new Vector2(openSideX, targetPosition.y + 0.3f)); // Deeper
    candidateTargets.Add(new Vector2(openSideX, targetPosition.y - 0.3f)); // Shallower
}

// CANDIDATE 3-N: Radial sweep around target (existing logic)
```

**Key Features:**
- Only adds if open side is **CLEAR** (no opponent rocks blocking)
- Adds **5 variations** for flexibility (center + 4 nearby positions)
- Inserted at **position 1** (tested second, right after direct target)

### **3. Scoring Bonus System**

Open side draws receive **MASSIVE bonuses** to encourage strategic spreading:

```csharp
// PART 5: OPEN SIDE STRATEGIC BONUS (40 points max)
float openSideBonus = 0f;

if (myRocksInHouse >= 1)
{
    // Check if final position is on OPPOSITE side from existing rocks
    bool isOnOpenSide = false;
    
    if (myRocksAverageX < -0.3f && finalPosX > 0.3f)
    {
        isOnOpenSide = true; // Rocks LEFT, shot RIGHT
    }
    else if (myRocksAverageX > 0.3f && finalPosX < -0.3f)
    {
        isOnOpenSide = true; // Rocks RIGHT, shot LEFT
    }
    
    if (isOnOpenSide)
    {
        openSideBonus = 20f; // Base bonus
        
        // SCALING BONUSES:
        if (myRocksInHouse >= 2)
        {
            openSideBonus += 10f; // 2+ rocks = harder to remove all!
        }
        
        float lateralSeparation = Mathf.Abs(finalPosX - myRocksAverageX);
        if (lateralSeparation > 1.0f)
        {
            openSideBonus += 10f; // Wide spread
        }
        else if (lateralSeparation > 0.6f)
        {
            openSideBonus += 5f; // Moderate spread
        }
    }
}

score += openSideBonus;
```

**Bonus Breakdown:**
- **Base:** 20 points (just for being on open side)
- **Multiple Rocks:** +10 points (if 2+ rocks already in house)
- **Wide Spread (>1.0m):** +10 points (maximum separation)
- **Moderate Spread (>0.6m):** +5 points (good separation)
- **MAXIMUM:** 40 points total (20 + 10 + 10)

---

## ?? **Updated Scoring System:**

### **Total Max Score: 162 points** (was 122)

| Component | Max Points | Purpose |
|-----------|------------|---------|
| Proximity to Target | 70 | How close to requested position |
| Scoring Position | 25 | Distance to button vs opponent |
| Guard Protection | 12 | Is shot behind a guard? |
| In-House Bonus | 15 | Is shot in scoring zone? |
| **Open Side Bonus** | **40** | **Strategic spread!** ? |
| Collision Context | +5 to -25 | Clean path vs early collision |

**Acceptance Threshold:** 45 points (out of 162)

### **Example Scenarios:**

#### **Scenario 1: Perfect Open Side Spread**
```
Existing rocks: 2 rocks at X = -0.7 (left side)
Draw target: Button (X = 0, Y = 6.5)
Open side: X = +0.8 (right side)

Final position: (0.85, 6.4) - Right side, 8cm from button

SCORING:
  Proximity: 65 pts (<15cm from open side target)
  Scoring Position: 20 pts (close to button)
  Guard Protection: 0 pts (exposed, but spread is valuable)
  In-House: 15 pts (in 4-foot)
  Open Side: 40 pts (2 rocks + wide spread 1.55m)
  Collision: +5 pts (clean)
  
TOTAL: 145/162 pts ? EXCELLENT SCORE! ?
```

#### **Scenario 2: Direct to Button (No Spread)**
```
Existing rocks: 2 rocks at X = -0.7 (left side)
Draw target: Button (X = 0, Y = 6.5)

Final position: (0.05, 6.48) - Center, 5cm from button

SCORING:
  Proximity: 70 pts (pinpoint at button)
  Scoring Position: 25 pts (shot rock!)
  Guard Protection: 0 pts (exposed)
  In-House: 15 pts (in 4-foot)
  Open Side: 0 pts (NOT on open side, no spread)
  Collision: +5 pts (clean)
  
TOTAL: 115/162 pts ? Good, but LESS than open side spread! ??
```

**Result:** AI now **PREFERS** spreading rocks strategically! ??

---

## ?? **Strategic Behavior:**

### **WITH HAMMER (Trying to Score 2+):**

```
End State:
  Red (AI) has 1 rock at (-0.6, 6.3) - 4-foot, left side
  Yellow has 0 rocks in house
  Red has hammer

AI Decision:
  ? Draw to OPEN SIDE (right, X ? +0.8)
  ? NOT to button center (no strategic advantage)
  
Reasoning:
  "We have 1 rock on the left. Drawing to the right side 
   spreads our rocks, making it MUCH harder for opponent 
   to remove both rocks next turn. High chance of scoring 2!"
```

### **WITHOUT HAMMER (Trying to Steal/Force):**

```
End State:
  Yellow (AI) has 1 rock at (0.7, 6.8) - 8-foot, right side
  Red has 1 rock at (0.1, 6.2) - 4-foot, center (shot rock)
  Yellow does NOT have hammer

AI Decision:
  ? Draw to OPEN SIDE (left, X ? -0.8)
  ? NOT to freeze on Red's shot rock (less strategic)
  
Reasoning:
  "We have 1 rock on the right. Drawing to the left side
   forces Red to remove BOTH our rocks to blank the end.
   If they only remove one, we might STEAL a point!"
```

### **Multiple Rocks Already (Going for Big End):**

```
End State:
  Red (AI) has 2 rocks at (-0.8, 6.4) and (-0.5, 7.1) - Left side cluster
  Yellow has 0 rocks in house
  Red has hammer

AI Decision:
  ? Draw to OPEN SIDE (right, X ? +0.9) with MAXIMUM bonus!
  
Reasoning:
  "We have 2 rocks clustered on the left. Drawing to the 
   right side creates a WIDE SPREAD (1.5m+ separation).
   This makes it EXTREMELY hard for opponent to clear.
   We're setting up for a big end (3+ points possible)!"
   
Bonus: 40 pts (20 base + 10 for 2 rocks + 10 for wide spread)
```

---

## ?? **Debug Output:**

Watch for these console messages:

```
[Open Side Draw] We have 2 rock(s) at avg X=-0.65 
  ? Open side is X=0.80 (clear: True) 
  ? Added strategic spread target: (0.80, 6.50)

[Open Side Bonus] ? STRATEGIC SPREAD!
  Existing rocks: 2 at avg X=-0.65
  This shot: X=0.82
  Lateral separation: 1.47
  BONUS: +40.0 (base 20 + scaling)

[Physics Draw] Candidate: (0.80, 6.50) ? Final: (0.82, 6.48), Turn: IN
  Proximity to Target: 65.0/70 (dist: 0.12m) ? DOMINANT FACTOR
  Guard Protection: 0.0/12 (exposed)
  Scoring Position: 18.0/25 (dist to button: 0.88, opponent closest: 999.00)
  Collision Context: 5.0 (clean)
  In-House Bonus: 11.0/15
  Open Side Bonus: 40.0/40 ? STRATEGIC SPREAD!
  TOTAL SCORE: 139.0/162

[Physics Draw] ? SUCCESS! Score: 139.0/162 (threshold: 45)
  Final position: (0.82, 6.48)
  Distance to target: 0.117m
  Pullback: (-1.245, -27.823)
  Turn: IN-TURN (curls RIGHT ?)
  Tested 11 candidates (tight 0.4m radius + open side spreads)
  Strategy: PROXIMITY-DOMINANT scoring (70/162 pts), 
           late collisions OK, open side spreads encouraged
```

---

## ?? **Testing Scenarios:**

### **Test 1: Single Rock on Left, Draw to Right**
```
SETUP:
  1. Place Red rock at (-0.7, 6.4) using Debug_Placement
  2. Set AI to draw to button (X=0, Y=6.5)
  3. Run AI shot

EXPECTED:
  ? AI detects 1 rock on left (avg X ? -0.7)
  ? Open side is RIGHT (X ? +0.8)
  ? AI draws to RIGHT side (X ? 0.8-0.9)
  ? Debug shows "STRATEGIC SPREAD!" with ~30-35 bonus points
  ? Final score: ~120-140/162
```

### **Test 2: Two Rocks on Right, Draw to Left**
```
SETUP:
  1. Place Red rocks at (0.6, 6.3) and (0.8, 7.0)
  2. Set AI to draw to button
  3. Run AI shot

EXPECTED:
  ? AI detects 2 rocks on right (avg X ? +0.7)
  ? Open side is LEFT (X ? -0.8)
  ? AI draws to LEFT side (X ? -0.8 to -0.9)
  ? Debug shows "STRATEGIC SPREAD!" with ~40 bonus points
  ? Wide spread bonus: +10 pts
  ? Final score: ~130-150/162
```

### **Test 3: Center Rocks, No Clear Open Side**
```
SETUP:
  1. Place Red rocks at (-0.2, 6.5) and (0.1, 6.8) - Near center
  2. Set AI to draw to button
  3. Run AI shot

EXPECTED:
  ? AI detects rocks near center (avg X ? -0.05)
  ? Open side detection is AMBIGUOUS (rocks too centered)
  ? AI may still try spread, but with lower separation bonus
  ? Or AI draws to button for proximity points
```

### **Test 4: Blocked Open Side**
```
SETUP:
  1. Place Red rock at (-0.7, 6.4) - Left side
  2. Place Yellow rock at (0.8, 6.6) - Right side (BLOCKING!)
  3. Set AI to draw to button
  4. Run AI shot

EXPECTED:
  ? AI detects 1 rock on left (avg X ? -0.7)
  ? Open side is RIGHT (X ? +0.8)
  ? BUT open side is BLOCKED by Yellow rock
  ? AI does NOT add open side candidates
  ? Debug: "open side X=0.80 is BLOCKED by opponent"
  ? AI draws to button instead (fallback)
```

---

## ? **Build Status:**

**BUILD SUCCESSFUL** - Zero compilation errors ?

---

## ?? **Summary:**

The AI now has sophisticated **open side draw strategy**:

1. ? **Detects** when it has rocks in the house
2. ? **Calculates** which side is OPEN (opposite from existing rocks)
3. ? **Checks** if open side is clear (no opponent blocking)
4. ? **Generates** high-priority open side candidates
5. ? **Rewards** strategic spreading with up to **40 bonus points**
6. ? **Scales** bonuses based on:
   - Number of rocks already in house
   - Lateral separation distance
   - Strategic value (scoring 2+ vs forcing opponent)

**Result:** AI now plays like a real curler, strategically spreading rocks to:
- **Score multiple points** when it has hammer
- **Force difficult removes** when opponent has hammer
- **Create complex ends** that are hard to clear

This is a **HUGE strategic upgrade** - the AI is no longer just drawing to the button! ?????
