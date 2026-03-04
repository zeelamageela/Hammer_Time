# End Menu Scoring Fix Summary

## Problems Identified

### 1. **Score Array Not Properly Initialized/Maintained**
**Issue:** When starting a new game or continuing after an end, the score array (`gsp.score[]`) is not properly initialized or can be null/wrong size.

**Symptoms:**
- "Index out of range" errors
- Previous ends showing 0-0 when they shouldn't
- Total scores incorrect

### 2. **Total Scores Calculated Inconsistently**
**Issue:** Multiple places calculate totals differently, leading to desyncs.

**Code locations:**
- `Start()` - recalculates from score array
- `SimEnd()` - recalculates multiple times
- GameManager (likely) - updates `gsp.redScore` / `gsp.yellowScore` directly

**Result:** Total displayed ? actual total ? sum of ends

### 3. **Winner Declared Before Scores Updated**
**Issue:** The winner determination logic (`if (gsp.redScore > gsp.yellowScore)`) runs BEFORE the total scores are recalculated from the score array.

**Timeline:**
```csharp
// 1. Game ends
gsp.endCurrent++; // Now = 10

// 2. EndMenu.Start() runs
if (gsp.endCurrent >= ends) {  // 10 >= 10 = TRUE
    if (gsp.redScore > gsp.yellowScore) {  // ? Uses OLD totals!
        info.text = "Team " + gsp.redTeamName + " Wins";
    }
}

// 3. LATER recalculate totals (too late!)
for (int i = 0; i < gsp.score.Length; i++) {
    tempTotal.x += gsp.score[i].x;
}
gsp.redScore = (int)tempTotal.x;  // ? Updates AFTER winner declared!
```

### 4. **SimEnd() Recreates Score Array**
**Issue:** When simulating to end of game, `SimEnd()` creates a brand new score array, potentially losing previous ends.

**Code:**
```csharp
Vector2Int[] tempScore = new Vector2Int[gsp.ends];  // ? New array!
// ... only copies some previous scores ...
gsp.score = tempScore;  // ? Overwrites existing scores!
```

## The Fix

### Fix 1: Initialize Score Array at Game Start (GameManager)
**File:** `Assets/Scripts/GameManager.cs`

**Add to `SetupGame()` coroutine:**
```csharp
// CRITICAL: Initialize score array for tracking each end
if (gsp.score == null || gsp.score.Length != gsp.ends)
{
    gsp.score = new Vector2Int[gsp.ends];
    for (int i = 0; i < gsp.ends; i++)
    {
        gsp.score[i] = new Vector2Int(0, 0);
    }
    Debug.Log($"[GameManager.SetupGame] Initialized score array for {gsp.ends} ends");
}
```

### Fix 2: Save End Score After Each End (GameManager)
**File:** `Assets/Scripts/GameManager.cs`

**Add to wherever the end completes (likely in `CheckScore()` or `EndOfEnd()`):**
```csharp
// CRITICAL: Save this end's score to the array BEFORE incrementing endCurrent
if (gsp.endCurrent < gsp.score.Length)
{
    gsp.score[gsp.endCurrent].x = redEndScore;  // Score for THIS end only
    gsp.score[gsp.endCurrent].y = yellowEndScore;
    Debug.Log($"[GameManager] End {gsp.endCurrent + 1} complete - Red: {redEndScore}, Yellow: {yellowEndScore}");
}

// Update TOTAL scores
gsp.redScore += redEndScore;
gsp.yellowScore += yellowEndScore;

// NOW increment end
gsp.endCurrent++;
```

### Fix 3: Recalculate Totals BEFORE Winner Logic (EndMenu)
**File:** `Assets/Scripts/EndMenu.cs` - `Start()` method

**Move the total score calculation to BEFORE the winner determination:**

```csharp
void Start()
{
    gsp = FindFirstObjectByType<GameSettingsPersist>();
    cm = FindFirstObjectByType<CareerManager>();

    if (gsp)
    {
        ends = gsp.ends;
        
        // ? CRITICAL FIX: Ensure score array exists FIRST
        if (gsp.score == null || gsp.score.Length != ends)
        {
            Debug.LogWarning($"[EndMenu] Score array invalid - initializing");
            gsp.score = new Vector2Int[ends];
        }
        
        // ? CRITICAL FIX: Recalculate totals BEFORE any logic that uses them!
        Vector2 calculatedTotal = Vector2.zero;
        for (int i = 0; i < Mathf.Min(gsp.endCurrent, gsp.score.Length); i++)
        {
            calculatedTotal.x += gsp.score[i].x;
            calculatedTotal.y += gsp.score[i].y;
        }
        
        // Update gsp totals to match
        gsp.redScore = (int)calculatedTotal.x;
        gsp.yellowScore = (int)calculatedTotal.y;
        
        Debug.Log($"[EndMenu.Start] Recalculated totals - Red: {gsp.redScore}, Yellow: {gsp.yellowScore} (from {gsp.endCurrent} completed ends)");
        
        // NOW all the winner/UI logic can use correct totals
        if (gsp.endCurrent == 0) {
            // Start of game...
        }
        else if (gsp.endCurrent >= ends) {
            // ? Now uses CORRECT totals!
            if (gsp.redScore > gsp.yellowScore) {
                info.text = "Team " + gsp.redTeamName + " Wins";
            }
            // ...
        }
        // ...
    }
}
```

### Fix 4: Preserve Scores in SimEnd()
**File:** `Assets/Scripts/EndMenu.cs` - `SimEnd()` method

**Change to preserve ALL previous scores:**

```csharp
// ? FIXED: Preserve ALL existing scores
Vector2Int[] tempScore;

if (gsp.score != null && gsp.score.Length == gsp.ends)
{
    // Copy existing score array
    tempScore = new Vector2Int[gsp.ends];
    for (int j = 0; j < gsp.score.Length; j++)
    {
        tempScore[j] = gsp.score[j];  // Preserve ALL ends
    }
}
else
{
    // Create new array if missing
    tempScore = new Vector2Int[gsp.ends];
}

// Now only update the CURRENT end (endCurrent - 1)
int currentEndIndex = gsp.endCurrent - 1;
if (currentEndIndex >= 0 && currentEndIndex < tempScore.Length)
{
    // Simulate THIS end only
    // ... simulation logic ...
}

// Save back
gsp.score = tempScore;
```

## Testing Checklist

After applying fixes:

- [ ] Start new game ? Score array initialized correctly
- [ ] Complete End 1 ? Score saved to `gsp.score[0]`
- [ ] EndMenu shows correct End 1 score (not 0-0)
- [ ] Complete End 2 ? Both ends show correct scores
- [ ] Totals match sum of individual ends
- [ ] Complete all ends ? Winner declared correctly
- [ ] SimEnd() ? Previous ends preserved, not overwritten
- [ ] Extra end (tie) ? Score array extends correctly
- [ ] Multiple games in tournament ? Each game starts fresh

## Key Principle

**SINGLE SOURCE OF TRUTH:**
- `gsp.score[]` array = authoritative record of each end
- `gsp.redScore` / `gsp.yellowScore` = calculated totals (derived from array)
- Always recalculate totals FROM the array, never update them independently!

This ensures:
- ? Total always matches sum of ends
- ? Previous ends never lost
- ? Winner determination uses correct totals
- ? UI displays consistent data
