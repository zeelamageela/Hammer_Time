# Debug Setup Enhancement - Comprehensive Testing Positions

## ? **What Changed**

Enhanced the **W key debug setup** in `Debug_Placement.cs` to spawn rocks at **8 strategic positions** around the house, testing all aspects of the dynamic curl compensation system.

---

## ?? **New Test Setup (Press W)**

### **Previously:**
```
4 rocks in a vertical line at center button
- All at X = 0
- Only tests center line shots
- Doesn't test lateral factor or distance factor
```

### **Now:**
```
8 rocks at strategic positions across the house
- Tests center, left, and right positions
- Tests near, mid, and far distances
- Includes both house rocks and guards
```

---

## ?? **Test Rock Positions**

### **HOUSE ROCKS (Y > 5.0) - For Takeout Testing:**

| # | Position | Coordinates | Tests |
|---|----------|-------------|-------|
| 1 | **Center Button** | (0.0, 6.5) | **Baseline** - No lateral, full distance |
| 2 | **Right Side Button** | (+1.0, 6.5) | **Lateral factor** - Strong right compensation |
| 3 | **Left Side Button** | (-1.0, 6.5) | **Lateral factor** - Strong left compensation |
| 4 | **Back Right** | (+0.5, 7.2) | **Distance + Lateral** - Far + moderate right |
| 5 | **Front Left** | (-0.5, 5.8) | **Short + Lateral** - Close + moderate left |

### **GUARDS (Y < 5.0) - For Peel/Raise Testing:**

| # | Position | Coordinates | Tests |
|---|----------|-------------|-------|
| 6 | **Center Guard** | (0.0, 3.0) | **Short distance** - Reduced distance factor |
| 7 | **Right Corner Guard** | (+0.8, 3.5) | **Short + Lateral** - Guard + right |
| 8 | **Left Corner Guard** | (-0.8, 3.5) | **Short + Lateral** - Guard + left |

---

## ?? **Dynamic Compensation Test Coverage**

### **Distance Factor Testing:**

| Rock | Distance | Normalized | Expected Distance Factor |
|------|----------|-----------|------------------------|
| Center Guard (6) | 28.0 units | 0.89 | **1.27** (lower) |
| Front Left (5) | 30.1 units | 0.96 | **1.29** (high) |
| Center Button (1) | 31.5 units | 1.00 | **1.30** (max) |
| Back Right (4) | 32.4 units | 1.03 | **1.30** (capped at max) |

### **Lateral Factor Testing:**

| Rock | Lateral Position | Expected Lateral Factor |
|------|-----------------|----------------------|
| Center rocks (1, 6) | 0.0 | **1.00** (baseline) |
| Half-side (4, 5) | ±0.5 | **1.08** (moderate) |
| Right Corner (7) | +0.8 | **1.12** (strong) |
| Side buttons (2, 3) | ±1.0 | **1.15** (very strong) |

### **Combined Compensation Examples:**

**Rock #1 (Center Button) - In-Turn:**
```
Distance: 31.5 ? distanceFactor = 1.30
Lateral: 0.0 ? lateralFactor = 1.00
Turn: IN ? turnBias = 1.08
TOTAL = 1.30 * 1.00 * 1.08 = 1.404
```

**Rock #2 (Right Side Button) - Out-Turn:**
```
Distance: 31.6 ? distanceFactor = 1.30
Lateral: 1.0 ? lateralFactor = 1.15
Turn: OUT ? turnBias = 1.15
TOTAL = 1.30 * 1.15 * 1.15 = 1.720  ? HIGHEST!
```

**Rock #6 (Center Guard) - In-Turn:**
```
Distance: 28.0 ? distanceFactor = 1.27
Lateral: 0.0 ? lateralFactor = 1.00
Turn: IN ? turnBias = 1.08
TOTAL = 1.27 * 1.00 * 1.08 = 1.372  ? LOWEST!
```

**Compensation Range: 1.37 to 1.72** (35% variation!)

---

## ?? **How to Test**

### **Setup:**
1. Start a Quick Test Game (vs AI)
2. **Press W** to spawn 8 test rocks

### **Test Procedure:**
1. **Throw your yellow rock out of play** (aim way off to the side)
   - This ensures AI gets to take shots at all the test rocks
   
2. **Watch AI take out each rock:**
   - Note the **turn direction** (in-turn or out-turn)
   - Check if it **hits dead-on** or misses
   - Measure the **error** (how far off from center of rock)

3. **Record results** for each position:
   ```
   Rock #1 (Center Button):
     In-turn: Hit ? | Miss ? | Error: ±0.02 units
     Out-turn: Hit ? | Miss ? | Error: ±0.02 units
   
   Rock #2 (Right Side):
     In-turn: Hit ? | Miss ? | Error: ___
     Out-turn: Hit ? | Miss ? | Error: ___
   
   ... (continue for all 8 rocks)
   ```

### **Expected Results:**
- ? **All 8 rocks should be hit accurately** (within ±0.02 units)
- ? **No systematic bias** (not all misses LEFT or all misses RIGHT)
- ? **Same accuracy** for center, left, and right positions
- ? **Same accuracy** for close and far positions

---

## ?? **Debug Console Output**

When you press W, you'll see:

```
[TAKEOUT TEST] Spawning test rocks across the house...
[TAKEOUT TEST] #1: Center Button at (0.00, 6.50)
[TAKEOUT TEST] #2: Right Side Button (+1.0) at (1.00, 6.50)
[TAKEOUT TEST] #3: Left Side Button (-1.0) at (-1.00, 6.50)
[TAKEOUT TEST] #4: Back Right (+0.5, 7.2) at (0.50, 7.20)
[TAKEOUT TEST] #5: Front Left (-0.5, 5.8) at (-0.50, 5.80)
[TAKEOUT TEST] #6: Center Guard (3.0) at (0.00, 3.00)
[TAKEOUT TEST] #7: Right Corner Guard (+0.8) at (0.80, 3.50)
[TAKEOUT TEST] #8: Left Corner Guard (-0.8) at (-0.80, 3.50)
[TAKEOUT TEST] ? Spawned 8 red rocks across the house!

[TAKEOUT TEST] TEST POSITIONS:
  HOUSE ROCKS (Y > 5.0):
    1. Center Button (0, 6.5) - Baseline test
    2. Right Side (+1.0, 6.5) - Lateral factor test
    3. Left Side (-1.0, 6.5) - Lateral factor test
    4. Back Right (+0.5, 7.2) - Distance + lateral
    5. Front Left (-0.5, 5.8) - Short distance + lateral
  GUARDS (Y < 5.0):
    6. Center (0, 3.0) - Short distance
    7. Right Corner (+0.8, 3.5) - Short + lateral
    8. Left Corner (-0.8, 3.5) - Short + lateral

[TAKEOUT TEST] INSTRUCTIONS:
  1. Throw your yellow rock out of play (miss on purpose)
  2. AI will take out the red rocks one by one
  3. Watch accuracy: Does it hit dead-on or miss?
  4. Check BOTH in-turn and out-turn shots
  5. All 8 positions should be accurate (±0.02 units)
```

---

## ?? **What to Look For**

### **Success Indicators:**
- ? AI hits **center button** accurately (rock #1)
- ? AI hits **right side** accurately (rock #2) - proves lateral factor works
- ? AI hits **left side** accurately (rock #3) - proves lateral factor works
- ? AI hits **back rock** accurately (rock #4) - proves distance + lateral combined
- ? AI hits **front rock** accurately (rock #5) - proves short distance compensation
- ? AI hits **guards** accurately (rocks #6-8) - proves system works at all distances

### **Failure Indicators:**
- ? **Center accurate, sides miss** ? Lateral factor needs tuning
- ? **Close rocks accurate, far rocks miss** ? Distance factor needs tuning
- ? **All in-turn miss, all out-turn hit** ? Turn bias needs adjustment
- ? **Systematic bias** (all miss RIGHT or all miss LEFT) ? Base compensation wrong

---

## ?? **Fine-Tuning Based on Results**

If specific positions still miss, adjust in `TrajectorySimulator.cs`:

### **If side shots miss:**
```csharp
// Increase lateral sensitivity
float lateralFactor = 1.0f + (lateralOffset * 0.20f); // Was 0.15f
```

### **If far shots miss:**
```csharp
// Increase distance sensitivity
float distanceFactor = 1.0f + (normalizedDistance * 0.4f); // Was 0.3f
```

### **If in-turn vs out-turn has different accuracy:**
```csharp
// Adjust turn bias
float turnBias = isInTurn ? 1.10f : 1.18f; // Adjust both values
```

---

## ?? **Files Modified**

1. ? `Assets/Scripts/Debug/Debug_Placement.cs`
   - Enhanced `SpawnTakeoutTestRocks()` method
   - Now spawns **8 rocks** instead of 4
   - Strategic positions test all compensation factors
   - Better debug logging with position names

---

## ? **Build Status**

**Build Successful!** ??

---

## ?? **Testing Checklist**

Use this checklist when testing:

```
[ ] Press W to spawn test rocks
[ ] Throw yellow rock out of play
[ ] Watch AI take shots

RESULTS:
[ ] Rock #1 (Center Button):     In-turn: ___  Out-turn: ___
[ ] Rock #2 (Right Side):         In-turn: ___  Out-turn: ___
[ ] Rock #3 (Left Side):          In-turn: ___  Out-turn: ___
[ ] Rock #4 (Back Right):         In-turn: ___  Out-turn: ___
[ ] Rock #5 (Front Left):         In-turn: ___  Out-turn: ___
[ ] Rock #6 (Center Guard):       In-turn: ___  Out-turn: ___
[ ] Rock #7 (Right Corner Guard): In-turn: ___  Out-turn: ___
[ ] Rock #8 (Left Corner Guard):  In-turn: ___  Out-turn: ___

Overall Accuracy: ___/8 rocks hit accurately
```

---

**Press W and test the dynamic curl compensation system across the entire house!** ????
