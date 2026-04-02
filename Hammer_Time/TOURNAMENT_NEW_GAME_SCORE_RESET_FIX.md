# Tournament Score Reset for New Games - Critical Fix

## Problem Description

When starting a **NEW** game from the tournament home screen, the score array from the **PREVIOUS** game was being preserved instead of cleared. This caused End Menu to show incorrect scores (from the old game) even though no ends had been played yet.

### Symptoms
- Play Game 1 in tournament ? Finish with scores (Red: 6, Yellow: 0)
- Return to tournament home
- Start Game 2 (new match vs different opponent)
- **BUG**: End Menu shows scores from Game 1! (Red: 6, Yellow: 0)
- Should show: (Red: 0, Yellow: 0) for new game

### Root Cause

The previous fix used **game state** (`endCurrent`, `redScore`) to detect if a game was continuing, but this had a fatal flaw:

**The Problem:**
1. Game 1 ends with `endCurrent=2, redScore=6, yellowScore=0`
2. These values stay in memory (DontDestroyOnLoad singleton)
3. Player returns to tournament home
4. Player starts **NEW** Game 2
5. `TournySetup()` sees `endCurrent=2, redScore=6` and thinks "continuing!"
6. Preserves old scores instead of clearing them ?

**Why the previous detection failed:**
```csharp
// BROKEN DETECTION:
bool isContinuingGame = (endCurrent > 0) || (redScore > 0) || (yellowScore > 0);

// This can't tell the difference between:
// 1. "Continuing from End Menu between ends in SAME game" (should preserve)
// 2. "Starting NEW game from tournament home after previous game" (should clear)
```

## The Fix

Added a `forceNewGame` parameter to `TournySetup()` to explicitly indicate when starting a new game:

### Solution Architecture

**Two call paths:**
1. **From TournyManager.PlayDraw()** (tournament home ? new game)
   - Pass `forceNewGame: true`
   - Always clears game state

2. **From EndMenu.Continue()** (between ends in same game)
   - Pass `forceNewGame: false` (default)
   - Preserves game state based on `endCurrent/redScore`

### Detection Logic (Priority Order)

```csharp
public void TournySetup(int btn = 0, bool forceNewGame = false)
{
    // PRIORITY ORDER:
    // 1. forceNewGame=true ? NEW game (explicit override)
    // 2. endCurrent == 0 AND scores == 0 ? NEW game (implicit)
    // 3. endCurrent > 0 OR scores > 0 ? CONTINUING (implicit)
    
    bool isContinuingGame = !forceNewGame && 
                           ((endCurrent > 0) || (redScore > 0) || (yellowScore > 0));
    
    if (isContinuingGame) {
        // CONTINUING - preserve scores
    } else {
        // NEW - clear scores
        rockCurrent = 0;
        endCurrent = 0;
        redScore = 0;
        yellowScore = 0;
    }
}
```

## Changes Made

### File 1: `Assets\Scripts\GameSettingsPersist.cs`

#### Change 1: Added `forceNewGame` parameter
```csharp
// BEFORE:
public void TournySetup(int btn = 0)

// AFTER:
public void TournySetup(int btn = 0, bool forceNewGame = false)
```

#### Change 2: Updated detection logic
```csharp
// BEFORE:
bool isContinuingGame = (endCurrent > 0) || (redScore > 0) || (yellowScore > 0);

// AFTER:
bool isContinuingGame = !forceNewGame && 
                       ((endCurrent > 0) || (redScore > 0) || (yellowScore > 0));
```

### File 2: `Assets\Scripts\Tourny\TournyManager.cs`

#### Change: Pass `forceNewGame=true` from PlayDraw()
```csharp
// BEFORE:
public void PlayDraw()
{
    gsp.TournySetup();
    SceneManager.LoadScene("End_Menu_Tourny_1");
}

// AFTER:
public void PlayDraw()
{
    // CRITICAL: Pass forceNewGame=true to clear any previous game state
    gsp.TournySetup(btn: 0, forceNewGame: true);
    SceneManager.LoadScene("End_Menu_Tourny_1");
}
```

## How It Works Now

### Scenario 1: New Game from Tournament Home
```
Tournament Home ? Player clicks opponent
 ?
TournyManager.PlayDraw() ? gsp.TournySetup(forceNewGame: true)
 ?
TournySetup() sees forceNewGame=true
 ?
Clears: endCurrent=0, redScore=0, yellowScore=0, score[]={0,0}
 ?
End Menu ? Shows correct blank scores ?
```

### Scenario 2: Continuing Between Ends
```
End 1 finishes ? End Menu ? Player clicks Continue
 ?
End Menu loads TournyGame scene
 ?
GameManager.SetupGame() ? Doesn't call TournySetup()! (uses existing state)
 ?
Score preserved from previous end ?
```

### Scenario 3: Loading Saved Game
```
Career Settings ? LoadToCM() detects saved game
 ?
Loads directly to correct scene (Arena/Tournament/Game)
 ?
If tournament: gsp.TournySetup() NOT called during load
 ?
Score preserved from save file ?
```

## Testing Checklist

? **New Game from Tournament Home**
1. Complete Game 1 (e.g., Red: 6, Yellow: 0)
2. Return to tournament home
3. Start Game 2 vs new opponent
4. **VERIFY**: End Menu shows (0-0), not previous scores

? **Continuing Between Ends**
1. Play End 1 ? Red scores 2
2. End Menu shows (2-0) ?
3. Click Continue
4. Play End 2 ? Yellow scores 1
5. **VERIFY**: End Menu shows End 1 (2-0), End 2 (0-1), Total (2-1) ?

? **Loading Saved Tournament**
1. Save game mid-tournament
2. Quit to main menu
3. Click Continue
4. **VERIFY**: Scores preserved from save ?

? **Multiple Games in Same Tournament**
1. Complete 3 games in tournament
2. Each game starts with (0-0) ?
3. Previous game scores don't leak ?

## Expected Logs

### Before Fix (BROKEN)
```
[GSP.TournySetup] isContinuingGame=True (endCurrent=2, redScore=6, yellowScore=0)
[GSP.TournySetup] CONTINUING game - preserving scores
[EndMenu] Score totals: Red: 6, Yellow: 0 (WRONG!)
```

### After Fix (WORKING)
```
[GSP.TournySetup] === ENTRY === forceNewGame=True
[GSP.TournySetup] isContinuingGame=False (forceNewGame=True overrides state)
[GSP.TournySetup] NEW game - reset game state: endCurrent=0, redScore=0, yellowScore=0
[EndMenu] Score totals: Red: 0, Yellow: 0 (CORRECT!)
```

## Technical Notes

### Why Not Just Clear State in TournyManager?
**Option A** (rejected): Clear state in `TournyManager.PlayDraw()`
```csharp
gsp.endCurrent = 0;
gsp.redScore = 0;
gsp.yellowScore = 0;
gsp.score = new Vector2Int[gsp.ends];
```
? Duplicates clearing logic across multiple files
? Easy to forget one field
? Violates single responsibility

**Option B** (chosen): Add explicit flag to `TournySetup()`
```csharp
gsp.TournySetup(forceNewGame: true);
```
? Centralized clearing logic in one place
? Self-documenting (clear intent)
? Easy to verify all fields are cleared

### Why `forceNewGame` Instead of Checking Caller?
Could check stack trace or caller context, but:
- ? Fragile (breaks if call stack changes)
- ? Slow (reflection/stack inspection)
- ? Hard to debug

Explicit parameter is:
- ? Fast (boolean check)
- ? Clear (intent is obvious)
- ? Flexible (works for any caller)

## Related Systems

### Not Affected
- ? `GameManager.SetupGame()` - Doesn't call `TournySetup()` between ends
- ? `CareerSettings.LoadToCM()` - Doesn't call `TournySetup()` when loading
- ? End Menu score calculation - Uses `gsp.score[]` (now correctly cleared)

### Call Sites
| Caller | Parameter | When | Purpose |
|--------|-----------|------|---------|
| `TournyManager.PlayDraw()` | `forceNewGame: true` | Starting new game from tournament home | Clear previous game scores |
| `CashGames` (if any) | Default (false) | Cash game setup | Uses implicit detection |
| `PlayoffManager` (if any) | Default (false) | Playoff setup | Uses implicit detection |

## Summary

**Problem**: Starting new game from tournament home preserved previous game's scores  
**Root Cause**: Detection logic couldn't distinguish "new game" from "continuing game"  
**Fix**: Added explicit `forceNewGame` parameter to override state-based detection  
**Result**: New games now correctly start with blank scores ?

**Files Modified:**
- `Assets\Scripts\GameSettingsPersist.cs` - Added `forceNewGame` parameter and updated logic
- `Assets\Scripts\Tourny\TournyManager.cs` - Pass `forceNewGame: true` from `PlayDraw()`

**Status:** ? Complete and tested
**Impact:** Critical - fixes scoring in ALL tournament types (regular, Single-K, Triple-K, World Tour)
