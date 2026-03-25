# ?? AI Guard Strategy Critical Fix

## ?? **CRITICAL BUGS FOUND**

### **Bug #1: Guard Logic is Backwards!**

```csharp
// CURRENT CODE (WRONG!):
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
    return CalculateBlockingGuardPosition(oppBestRock, 2.5f);
}
```

**PROBLEM**: This logic says "If I have rocks, guard MY rocks. If opponent has rocks, guard THEIR rocks."

**RESULT**: AI guards the WRONG team's rocks!
- If AI has rocks in house ? Guards its OWN rocks (blocking itself!)
- If opponent has rocks in house ? Guards OPPONENT's rocks (helping them!)

**THIS IS BACKWARDS!**

---

### **Bug #2: Missing Freeze Shot Strategy**

Freeze shots are **NOT IMPLEMENTED** in any strategy! The AI never considers:
- Freezing to opponent's shot rock to steal
- Freezing to own rock to add points
- Using freeze as setup for future double takeout

---

### **Bug #3: Guard Placement Always Same X Position**

```csharp
// Guards always placed at SAME X as protected rock
Vector2 guardPos = new Vector2(rockPos.x, guardDistance);
```

**PROBLEM**: This creates a **perfect direct line** from launcher ? guard ? rock!
- Makes it EASIER to hit through guard (runback shot)
- Should offset guard slightly LEFT/RIGHT to block angles

---

## ? **THE FIXES**

### **Fix #1: Correct Guard Logic**

```csharp
/// <summary>
/// Smart guard placement - FIXED LOGIC!
/// Guards should BLOCK opponent rocks, PROTECT my rocks
/// </summary>
public static Vector2 CalculateSmartGuardPosition(
    GameManager gm,
    string myTeam,
    out string guardReasoning)
{
    guardReasoning = "Center guard (default)";
    
    // ANALYSIS: What's in the house?
    int myRocksInHouse = 0;
    int oppRocksInHouse = 0;
    GameObject oppBestRock = null;
    GameObject myBestRock = null;
    float oppBestDist = 999f;
    float myBestDist = 999f;
    
    Vector2 button = new Vector2(0f, 6.5f);
    
    foreach (var rockEntry in gm.houseList)
    {
        bool isMine = (rockEntry.rockInfo.teamName == myTeam);
        float dist = Vector2.Distance(rockEntry.rock.transform.position, button);
        
        if (isMine)
        {
            myRocksInHouse++;
            if (dist < myBestDist)
            {
                myBestDist = dist;
                myBestRock = rockEntry.rock;
            }
        }
        else
        {
            oppRocksInHouse++;
            if (dist < oppBestDist)
            {
                oppBestDist = dist;
                oppBestRock = rockEntry.rock;
            }
        }
    }
    
    // ?? FIXED DECISION TREE:
    
    // SCENARIO 1: Opponent has shot rock - BLOCK IT!
    // This is highest priority - deny them points!
    if (oppRocksInHouse >= 1 && oppBestRock != null)
    {
        // Check if they're WINNING the house
        if (oppBestDist < myBestDist || myRocksInHouse == 0)
        {
            guardReasoning = $"BLOCKING opponent's shot rock at X={oppBestRock.transform.position.x:F1}";
            return CalculateBlockingGuardPosition(oppBestRock, 2.5f);
        }
    }
    
    // SCENARIO 2: I have shot rock - PROTECT IT!
    // Only if I'm WINNING the house
    if (myRocksInHouse >= 1 && myBestRock != null && myBestDist < oppBestDist)
    {
        guardReasoning = $"PROTECTING our shot rock at X={myBestRock.transform.position.x:F1}";
        return CalculateProtectionGuardPosition(myBestRock, 2.5f);
    }
    
    // SCENARIO 3: Clean house or tied - CENTER GUARD (steal setup)
    guardReasoning = "Center guard for setup (clean/tied house)";
    return new Vector2(0f, 2.5f);
}
```

**KEY CHANGES:**
1. **Priority 1**: Block opponent's shot rock (if they're winning)
2. **Priority 2**: Protect my shot rock (if I'm winning)
3. **Priority 3**: Center guard (neutral position)

---

### **Fix #2: Add Freeze Shot Strategy**

```csharp
/// <summary>
/// ?? Freeze-to-Score: Setup future double takeout (3 shots)
/// Rock 1: Freeze to opponent's shot rock
/// Rock 2: Guard the freeze
/// Rock 3: Attempt double takeout to score 2+
/// </summary>
public static EndPlan FreezeGuardRemove_Setup(GameManager gm, string myTeam, int rockCurrent)
{
    var plan = new EndPlan
    {
        strategyName = "Freeze-Guard-Remove (Setup Double)",
        reasoning = "Freeze to opponent's rock, then remove both for points",
        expectedOutcome = "Frozen rock setup for future double takeout",
        planCreatedAtRock = rockCurrent,
        confidence = 0.70f
    };
    
    // Find opponent's best rock to freeze to
    GameObject targetRock = FindOpponentShotRock(gm, myTeam);
    
    if (targetRock != null)
    {
        Vector2 freezePos = CalculateFreezePosition(targetRock);
        
        // Shot 1: FREEZE to opponent's rock
        plan.plannedIntents.Add(ShotIntent.ProtectLead); // Reuse intent for freeze
        plan.targetPositions.Add(freezePos);
        plan.targetRocks.Add(targetRock.GetComponent<Rock_Info>().rockIndex);
        
        // Shot 2: Guard the frozen pair
        plan.plannedIntents.Add(ShotIntent.CreateOpportunity);
        plan.targetPositions.Add(CalculateProtectionGuardPosition(targetRock, 2.5f));
        plan.targetRocks.Add(-1);
        
        // Shot 3: Attempt double takeout
        plan.plannedIntents.Add(ShotIntent.RemoveThreat);
        plan.targetPositions.Add(Vector2.zero);
        plan.targetRocks.Add(targetRock.GetComponent<Rock_Info>().rockIndex);
        
        plan.reasoning += $" | Freezing to rock at {targetRock.transform.position}";
    }
    else
    {
        // No good freeze target - fall back to standard strategy
        plan.confidence = 0.0f; // Invalidate plan
    }
    
    return plan;
}

/// <summary>
/// Calculate freeze position (directly in front of target rock)
/// </summary>
private static Vector2 CalculateFreezePosition(GameObject targetRock)
{
    Vector2 rockPos = targetRock.transform.position;
    Vector2 button = new Vector2(0f, 6.5f);
    
    // Freeze position is between button and rock, touching rock
    Vector2 direction = (rockPos - button).normalized;
    float rockRadius = 0.14f;
    
    // Position rock TOUCHING target (2 * radius apart)
    Vector2 freezePos = rockPos - direction * (rockRadius * 2.1f); // Slightly separated
    
    return freezePos;
}

/// <summary>
/// Find opponent's best rock (closest to button)
/// </summary>
private static GameObject FindOpponentShotRock(GameManager gm, string myTeam)
{
    GameObject bestRock = null;
    float bestDist = 999f;
    Vector2 button = new Vector2(0f, 6.5f);
    
    foreach (var rockEntry in gm.houseList)
    {
        if (rockEntry.rockInfo.teamName != myTeam)
        {
            float dist = Vector2.Distance(rockEntry.rock.transform.position, button);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestRock = rockEntry.rock;
            }
        }
    }
    
    return bestRock;
}
```

---

### **Fix #3: Improved Guard Positioning**

```csharp
/// <summary>
/// Calculate optimal guard position to protect a specific rock
/// ?? FIXED: Offset guard slightly to block angles
/// </summary>
public static Vector2 CalculateProtectionGuardPosition(GameObject protectRock, float guardDistance = 2.5f)
{
    if (protectRock == null) 
        return new Vector2(0f, guardDistance);
    
    Vector2 rockPos = protectRock.transform.position;
    Vector2 button = new Vector2(0f, 6.5f);
    
    // ?? NEW: Calculate angle from button to rock
    Vector2 toRock = rockPos - button;
    float angle = Mathf.Atan2(toRock.y, toRock.x);
    
    // Offset guard SLIGHTLY (20cm) perpendicular to line
    // This blocks angled shots better than direct line
    float offsetX = Mathf.Sin(angle) * 0.2f;
    
    // Guard at same X as rock, but with slight angular offset
    Vector2 guardPos = new Vector2(rockPos.x + offsetX, guardDistance);
    
    // Clamp to reasonable range
    guardPos.x = Mathf.Clamp(guardPos.x, -1.5f, 1.5f);
    
    return guardPos;
}

/// <summary>
/// Calculate optimal guard position to block opponent's rock
/// ?? FIXED: Better blocking angle calculation
/// </summary>
public static Vector2 CalculateBlockingGuardPosition(GameObject blockRock, float guardDistance = 2.5f)
{
    if (blockRock == null)
        return new Vector2(0f, guardDistance);
    
    Vector2 rockPos = blockRock.transform.position;
    Vector2 button = new Vector2(0f, 6.5f);
    Vector2 launcher = new Vector2(0f, -25f);
    
    // ?? NEW: Place guard on TAKEOUT LINE from launcher to target
    // This blocks the most common attack angle
    Vector2 toRock = (rockPos - launcher).normalized;
    
    // Guard positioned along attack line at guardDistance Y
    float t = (guardDistance - launcher.y) / toRock.y;
    float guardX = launcher.x + toRock.x * t;
    
    // Clamp to reasonable range
    guardX = Mathf.Clamp(guardX, -1.2f, 1.2f);
    
    return new Vector2(guardX, guardDistance);
}
```

---

## ?? **Impact Summary**

### **Before Fixes:**
- ? AI guards its OWN rocks (blocking itself)
- ? AI guards OPPONENT's rocks (helping them)
- ? Guards placed in perfect runback lines
- ? Freeze shots never used

### **After Fixes:**
- ? AI guards OPPONENT's rocks (blocks threats)
- ? AI protects OWN rocks (only when winning)
- ? Guards offset to block angles
- ? Freeze shots integrated into strategy

---

## ?? **Testing Guide**

### **Test 1: Guard Placement**
1. Press Q to start test game
2. Let opponent draw to button
3. Watch AI place guard
4. **Expected**: Guard should be ON LINE between launcher and opponent's rock
5. **Before Fix**: Guard would be at same X as MY rocks (wrong!)

### **Test 2: Freeze Strategy**
1. Press Q to start test game
2. Draw to button as player
3. Wait for AI turn
4. **Expected**: AI should consider freeze shot to your rock
5. **Before Fix**: AI never attempts freeze

### **Test 3: Multiple Rocks**
1. Press Q to start test game  
2. Place 2 red rocks in house, 1 yellow rock
3. Watch AI guard placement
4. **Expected**: AI guards the RED rocks (opponent)
5. **Before Fix**: AI would guard YELLOW rocks (helping itself?!)

---

## ?? **Ready to Apply?**

The fixes are comprehensive and address all 3 major issues. Would you like me to:

1. **Apply all fixes now** (recommended)
2. **Apply fixes one at a time** (for testing)
3. **Add more freeze shot strategies** (advanced)

Let me know and I'll implement immediately!
