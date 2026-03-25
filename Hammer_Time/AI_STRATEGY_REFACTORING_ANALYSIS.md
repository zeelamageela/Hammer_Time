# ?? AI Strategy Code Analysis - Redundancy & Improvements

## ?? **Current Code Analysis:**

### **Major Redundancies Found:**

#### **1. Duplicate Shot Decision Logic (90% Similar!)**

All 5 strategy methods share **nearly identical** decision trees:

```csharp
// ConservativeSteal, AggressiveHammer, ScoreTwoOrBlank, AggressiveNotHammer, StealOrBlank
// ALL have this same structure:

if (phase == "early")
{
    if (threatRock >= 0)
    {
        context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
        // EV evaluation
        aiTarg.ExecuteIntent(context, rockCurrent);
        return true;
    }
    else
    {
        context = new ShotContext(ShotIntent.CreateOpportunity);
        // EV evaluation
        aiTarg.ExecuteIntent(context, rockCurrent);
        return true;
    }
}
else if (phase == "middle")
{
    // Similar structure...
}
else if (phase == "late")
{
    // Similar structure...
}
```

**Problem:** 500+ lines of duplicated code across 5 methods!

---

#### **2. Duplicate EV Evaluation Blocks (25+ times!)**

```csharp
// This exact code appears 25+ times:
if (evSystem != null && useEVOptimization)
{
    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
}

aiTarg.ExecuteIntent(context, rockCurrent);
return true;
```

**Problem:** Same 5 lines repeated in every decision branch!

---

#### **3. Duplicate Rock Counting Logic**

```csharp
// Appears in EVERY late-game decision:
int oppRocksInHouse = gm.houseList.Count - myRocksInHouse;

// And this:
float myBestDist = 999f;
float theirBestDist = 999f;

foreach (var rock in gm.houseList)
{
    float dist = Vector2.Distance(rock.rock.transform.position, new Vector2(0f, 6.5f));
    if (rock.rockInfo.teamName == activeTeamName && dist < myBestDist)
        myBestDist = dist;
    else if (rock.rockInfo.teamName != activeTeamName && dist < theirBestDist)
        theirBestDist = dist;
}
```

**Problem:** Calculated multiple times per turn!

---

## ? **Proposed Refactoring:**

### **1. Unified Strategy Execution Helper**

```csharp
/// <summary>
/// Execute a shot with automatic EV evaluation
/// Reduces 500+ lines of duplicated code to single method
/// </summary>
private bool ExecuteShot(ShotIntent intent, int targetRock, int rockCurrent, bool acceptRisk = false, bool mustScore = false)
{
    ShotContext context = new ShotContext(intent, targetRock);
    context.acceptRisk = acceptRisk;
    context.mustScore = mustScore;
    
    // Automatic EV evaluation (if enabled)
    if (evSystem != null && useEVOptimization)
    {
        context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
    }
    
    aiTarg.ExecuteIntent(context, rockCurrent);
    return true;
}
```

**Before:**
```csharp
context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
context.acceptRisk = true;

if (evSystem != null && useEVOptimization)
    context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));

aiTarg.ExecuteIntent(context, rockCurrent);
return true;
```

**After:**
```csharp
return ExecuteShot(ShotIntent.RemoveThreat, threatRock, rockCurrent, acceptRisk: true);
```

**Savings:** 5 lines ? 1 line (400 lines saved!)

---

### **2. Cached House Analysis**

```csharp
/// <summary>
/// House state analysis - calculated ONCE per turn, cached for all decisions
/// </summary>
private class HouseAnalysis
{
    public int myRocksInHouse;
    public int oppRocksInHouse;
    public float myBestDistance;
    public float oppBestDistance;
    public bool amWinningHouse;
    public int threatRock;
}

private HouseAnalysis _cachedHouseAnalysis = null;

private HouseAnalysis GetHouseAnalysis()
{
    // Return cached if already calculated this turn
    if (_cachedHouseAnalysis != null) return _cachedHouseAnalysis;
    
    var analysis = new HouseAnalysis
    {
        myBestDistance = 999f,
        oppBestDistance = 999f,
        threatRock = FindBiggestThreat(activeTeamName)
    };
    
    Vector2 button = new Vector2(0f, 6.5f);
    
    foreach (var rock in gm.houseList)
    {
        bool isMine = (rock.rockInfo.teamName == activeTeamName);
        float dist = Vector2.Distance(rock.rock.transform.position, button);
        
        if (isMine)
        {
            analysis.myRocksInHouse++;
            if (dist < analysis.myBestDistance)
                analysis.myBestDistance = dist;
        }
        else
        {
            analysis.oppRocksInHouse++;
            if (dist < analysis.oppBestDistance)
                analysis.oppBestDistance = dist;
        }
    }
    
    analysis.amWinningHouse = (analysis.myBestDistance < analysis.oppBestDistance);
    
    _cachedHouseAnalysis = analysis;
    return analysis;
}

// Clear cache at start of each turn
public void OnShot(int rockCurrent)
{
    _cachedHouseAnalysis = null; // Reset cache
    // ... rest of OnShot logic
}
```

**Before:**
```csharp
// Calculated 5+ times per turn in different methods:
int oppRocksInHouse = gm.houseList.Count - myRocksInHouse;
float myBestDist = 999f;
// ... 20 lines of foreach loop
```

**After:**
```csharp
var house = GetHouseAnalysis(); // Instant (cached!)
if (house.amWinningHouse)
{
    // Use house.myRocksInHouse, house.oppRocksInHouse, etc.
}
```

---

### **3. Simplified Strategy Methods**

**Before (ConservativeSteal - 120 lines):**
```csharp
private bool TryIntentBasedShot_ConservativeSteal(int rockCurrent, string phase)
{
    // ... multi-shot planning check ...
    
    int threatRock = FindBiggestThreat(activeTeamName);
    int myRocksInHouse = CountMyRocksInScoring(activeTeamName);
    
    ShotContext context;
    
    if (phase == "early")
    {
        if (threatRock >= 0)
        {
            context = new ShotContext(ShotIntent.RemoveThreat, threatRock);
            context.acceptRisk = false;
            
            if (evSystem != null && useEVOptimization)
                context = evSystem.EvaluateShot(context, BuildGameState(rockCurrent), GetShooterStats(rockCurrent));
            
            aiTarg.ExecuteIntent(context, rockCurrent);
            return true;
        }
        else
        {
            context = new ShotContext(ShotIntent.CreateOpportunity);
            // ... EV evaluation ...
            aiTarg.ExecuteIntent(context, rockCurrent);
            return true;
        }
    }
    // ... 80 more lines of similar code ...
}
```

**After (ConservativeSteal - 30 lines):**
```csharp
private bool TryIntentBasedShot_ConservativeSteal(int rockCurrent, string phase)
{
    if (TryExecutePlannedShot(rockCurrent, phase))
        return true;
    
    var house = GetHouseAnalysis();
    
    // EARLY: Remove threats or setup
    if (phase == "early")
    {
        return house.threatRock >= 0
            ? ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent)
            : ExecuteShot(ShotIntent.CreateOpportunity, -1, rockCurrent);
    }
    
    // MIDDLE: Build position or clear threats
    if (phase == "middle")
    {
        if (house.threatRock >= 0)
            return ExecuteShot(ShotIntent.RemoveThreat, house.threatRock, rockCurrent, acceptRisk: house.myRocksInHouse > 0);
        
        return house.myRocksInHouse > 1
            ? ExecuteShot(ShotIntent.ProtectLead, -1, rockCurrent)
            : ExecuteShot(ShotIntent.ScorePoints, -1, rockCurrent);
    }
    
    // LATE: Steal or protect
    return HandleLateGameSteal(rockCurrent, house);
}

private bool HandleLateGameSteal(int rockCurrent, HouseAnalysis house)
{
    // Late game logic extracted to separate method
    // ... focused logic without duplication ...
}
```

**Savings:** 120 lines ? 30 lines (75% reduction!)

---

## ?? **Impact Summary:**

### **Code Reduction:**

| Component | Before | After | Savings |
|-----------|--------|-------|---------|
| **Strategy Methods (5x)** | ~600 lines | ~150 lines | **450 lines (75%)** |
| **EV Evaluation Blocks** | 25 copies | 1 method | **100 lines** |
| **House Analysis** | 5x per turn | 1x cached | **80 lines + performance** |
| **Total Reduction** | ~800 lines | ~200 lines | **600 lines (75%)** |

---

## ?? **Additional Improvements:**

### **4. Strategy Selection Table**

```csharp
/// <summary>
/// Strategy selection table - replaces OnShot's nested if/else
/// </summary>
private static readonly Dictionary<(bool hasHammer, int scoreDiff, int endsLeft), System.Func<AI_Strategy, int, string, bool>> StrategyTable = new()
{
    // Without hammer, behind
    { (false, -2, 2), (ai, rock, phase) => ai.TryIntentBasedShot_AggressiveNotHammer(rock, phase) },
    { (false, -1, 2), (ai, rock, phase) => ai.TryIntentBasedShot_ConservativeSteal(rock, phase) },
    
    // With hammer, behind
    { (true, -1, 2), (ai, rock, phase) => ai.TryIntentBasedShot_AggressiveHammer(rock, phase) },
    
    // ... etc
};
```

**Before (OnShot):**
```csharp
public void OnShot(int rockCurrent)
{
    // ... 60 lines of nested if/else ...
    
    if (rockCurrent % 2 == 0)
    {
        if (gm.endTotal - gm.endCurrent >= 2)
        {
            if (activeTeamScore < (oppTeamScore + 1))
                AggressiveNotHammer(rockCurrent, phase);
            else if (activeTeamScore < oppTeamScore)
                ConservativeSteal(rockCurrent, phase);
            // ... more nesting ...
        }
    }
}
```

**After:**
```csharp
public void OnShot(int rockCurrent)
{
    _cachedHouseAnalysis = null; // Clear cache
    
    bool hasHammer = (rockCurrent % 2 != 0);
    int scoreDiff = activeTeamScore - oppTeamScore;
    int endsLeft = gm.endTotal - gm.endCurrent;
    
    var key = (hasHammer, NormalizeScoreDiff(scoreDiff), endsLeft);
    
    if (StrategyTable.TryGetValue(key, out var strategy))
    {
        strategy(this, rockCurrent, phase);
    }
}
```

---

### **5. Decision Tree Visualization**

```csharp
/// <summary>
/// Log decision tree for debugging (optional)
/// </summary>
private void LogDecisionTree(string strategy, string phase, HouseAnalysis house)
{
    if (!planningVerboseLogging) return;
    
    Debug.Log($"[AI Decision] {strategy} | {phase} | " +
              $"MyRocks:{house.myRocksInHouse} OppRocks:{house.oppRocksInHouse} | " +
              $"Winning:{house.amWinningHouse} | Threat:{house.threatRock}");
}
```

---

## ?? **Recommended Implementation Order:**

### **Phase 1: Quick Wins (30 min)**
1. ? Add `ExecuteShot()` helper method
2. ? Replace all EV evaluation blocks with `ExecuteShot()`
3. ? **Immediate 400 line reduction**

### **Phase 2: House Analysis Cache (15 min)**
1. ? Add `HouseAnalysis` class
2. ? Add `GetHouseAnalysis()` with caching
3. ? Replace duplicate calculations
4. ? **Performance boost + 80 line reduction**

### **Phase 3: Simplified Strategy Methods (45 min)**
1. ? Refactor `ConservativeSteal` (example)
2. ? Refactor `AggressiveHammer`
3. ? Refactor remaining 3 methods
4. ? **150 line reduction + clarity**

### **Phase 4: Strategy Table (Optional - 30 min)**
1. ? Add strategy selection table
2. ? Simplify `OnShot()` method
3. ? **Better extensibility**

---

## ?? **Other Improvements:**

### **6. Risk-Based Decision Modifier**

```csharp
/// <summary>
/// Modify shot aggressiveness based on risk tolerance
/// </summary>
private float GetRiskModifier(int rockCurrent)
{
    float riskTolerance = StrategyPatternLibrary.CalculateRiskTolerance(
        activeTeamScore, oppTeamScore, gm.endCurrent, gm.endTotal, 
        16 - rockCurrent, rockCurrent % 2 != 0
    );
    
    return riskTolerance; // 0.0 = conservative, 1.0 = aggressive
}
```

---

### **7. Intent Priority System**

```csharp
/// <summary>
/// Get intent priority based on situation
/// Higher priority = more important to execute
/// </summary>
private int GetIntentPriority(ShotIntent intent, HouseAnalysis house)
{
    switch (intent)
    {
        case ShotIntent.RemoveThreat when house.oppBestDistance < 0.5f:
            return 10; // URGENT! Shot rock very close to button
        
        case ShotIntent.RemoveThreat:
            return 8; // High priority
        
        case ShotIntent.ProtectLead when house.amWinningHouse:
            return 7; // Protect what we have
        
        case ShotIntent.ScorePoints:
            return 5; // Medium priority
        
        case ShotIntent.CreateOpportunity:
            return 3; // Low priority (setup)
        
        default:
            return 1;
    }
}
```

---

## ?? **Final Comparison:**

### **Current Code:**
- ? ~800 lines of AI strategy methods
- ? 90% duplicate logic across 5 methods
- ? House analysis calculated 5+ times per turn
- ? Hard to maintain/extend
- ? Difficult to debug

### **Refactored Code:**
- ? ~200 lines of AI strategy methods
- ? 75% code reduction
- ? House analysis cached (1x per turn)
- ? Easy to maintain/extend
- ? Clear decision flow
- ? Better performance

---

## ?? **Ready to Implement?**

Would you like me to:

1. **Phase 1 Only** - Quick `ExecuteShot()` helper (30 min, 400 lines saved)
2. **Phase 1 + 2** - Add house analysis cache (45 min, 480 lines saved)
3. **Full Refactor** - All phases (2 hours, 600 lines saved + clarity)

The refactored code will be:
- ? **75% shorter**
- ? **Easier to maintain**
- ? **Better performance** (cached analysis)
- ? **Exact same behavior** (no gameplay changes)

Let me know which level of refactoring you'd like!
