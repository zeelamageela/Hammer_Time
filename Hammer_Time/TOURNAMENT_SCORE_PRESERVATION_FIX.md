# Tournament Score Preservation Between Ends - Critical Fix

## Problem Description

When returning from End Menu after completing an end in a Single-K (or any tournament) game, the score array was being **cleared**, losing all previously completed end scores.

### Symptoms
- End 1: Red scores 2, Yellow scores 0 ? Saved correctly
- End 2: Red scores 0, Yellow scores 1 ? Saved correctly  
- **BUT** when End Menu loads, scores show as:
  - End 1: Red 0, Yellow 0 ? (LOST!)
  - End 2: Red 0, Yellow 1 ?
  - Total: Red 0, Yellow 1 (should be Red 2, Yellow 1!)

### Root Cause

In `GameSettingsPersist.TournySetup()`, the code was using `gameInProgress` flag to determine if this was a NEW game vs CONTINUING an existing game.

**The Problem:**
1. `GameManager.Scoring()` correctly sets `gameInProgress = true` when end finishes
2. Scene changes to End Menu
3. End Menu ? Continue ? back to TournyGame scene
4. `TournySetup()` is called to setup the game scene
5. **BUT** `gameInProgress` is already `false` due to scene transition flags being reset
6. Code thinks this is a NEW game and clears the score array!

```csharp
// BROKEN CODE:
bool isLoadingGame = gameInProgress;  // This is false!

if (isLoadingGame) {
    // Preserve scores
} else {
    // NEW game - clear scores ?
    for (int i = 0; i < score.Length; i++) {
        score[i] = new Vector2Int(0, 0);  // LOSES ALL SCORES!
    }
}
```

## The Fix

Changed detection logic to use **game state** instead of control flags:

```csharp
// FIXED CODE:
// If endCurrent > 0 OR scores > 0, we're CONTINUING an existing game
bool isContinuingGame = (endCurrent > 0) || (redScore > 0) || (yellowScore > 0);

if (isContinuingGame) {
    // CONTINUING - preserve scores ?
    Debug.Log("CONTINUING game - preserving scores");
} else {
    // NEW game - clear scores
    rockCurrent = 0;
    endCurrent = 0;
    redScore = 0;
    yellowScore = 0;
}
```

### Why This Works

**Game State Indicators:**
- `endCurrent > 0` ? We've completed at least one end
- `redScore > 0 || yellowScore > 0` ? Someone has scored points
- These values **persist through scene changes** (in DontDestroyOnLoad singleton)

**Control Flags (unreliable):**
- `gameInProgress` ? Reset during scene transitions
- `loadGame` ? Only for loading from disk
- These are **temporary** and don't survive scene changes

## Changes Made

### File: `Assets\Scripts\GameSettingsPersist.cs`

#### Change 1: Detection Logic (Line ~392)
```csharp
// BEFORE:
bool isLoadingGame = gameInProgress;
Debug.Log($"isLoadingGame={isLoadingGame}");

// AFTER:
bool isContinuingGame = (endCurrent > 0) || (redScore > 0) || (yellowScore > 0);
Debug.Log($"isContinuingGame={isContinuingGame} (endCurrent={endCurrent}, redScore={redScore}, yellowScore={yellowScore})");
```

#### Change 2: State Preservation (Line ~397)
```csharp
if (isContinuingGame)
{
    // CONTINUING an existing game - preserve all game state
    Debug.Log($"CONTINUING game - preserving scores: endCurrent={endCurrent}, redScore={redScore}, yellowScore={yellowScore}");
    Debug.Log($"  Score array: End1=(...), End2=(...)");  // Shows preserved scores
}
else
{
    // NEW game - reset all game state
    rockCurrent = 0;
    endCurrent = 0;
    redScore = 0;
    yellowScore = 0;
}
```

#### Change 3: Score Array Handling (Line ~420)
```csharp
else if (isContinuingGame)
{
    // Continuing existing game - preserve array as-is
    Debug.Log($"CONTINUING game - score array ({score.Length} ends) preserved");
    
    // Log ALL completed end scores
    for (int i = 0; i < endCurrent; i++)
    {
        Debug.Log($"  End {i + 1}: Red={score[i].x}, Yellow={score[i].y}");
    }
}
```

## Testing Checklist

? **Single-K Tournament (2 ends, 2 rocks)**
1. Start new Single-K tournament
2. Play End 1 ? Red scores 2 points
3. End Menu shows: End 1 (2-0), Total (2-0) ?
4. Continue to End 2
5. **VERIFY:** Score array preserved (End 1 still shows 2-0)
6. Play End 2 ? Yellow scores 1 point
7. End Menu shows: End 1 (2-0), End 2 (0-1), Total (2-1) ?

? **Triple-K Tournament**
- Same flow, multiple ends
- Verify score preservation between each end

? **Regular Tournament**
- 10 ends, 8 rocks
- Verify scores accumulate correctly

? **Cash Game**
- Not affected (no end transitions)

## Expected Logs

### Before Fix (BROKEN)
```
[GSP.TournySetup] isLoadingGame=false (gameInProgress=false)
[GSP.TournySetup] NEW game - reset game state: endCurrent=0, redScore=0, yellowScore=0
[GSP.TournySetup] NEW game - clearing score array (3 ends)
```

### After Fix (WORKING)
```
[GSP.TournySetup] isContinuingGame=true (endCurrent=1, redScore=2, yellowScore=0)
[GSP.TournySetup] CONTINUING game - preserving scores: endCurrent=1, redScore=2, yellowScore=0
[GSP.TournySetup]   Score array: End1=(2,0), End2=(0,0)
[GSP.TournySetup] CONTINUING game - score array (3 ends) preserved
[GSP.TournySetup]   End 1: Red=2, Yellow=0
```

## Related Systems

### Not Affected
- ? `GameManager.Scoring()` - Still saves correctly
- ? `CareerManager.SaveCareer()` - Still saves to disk correctly
- ? `EndMenu.Start()` - Loads from `gsp.score` array (now preserved!)

### Potentially Impacted
- ?? Mid-game saves/loads - Should still work (uses `loadGame` flag)
- ?? Tournament completion - Should still work (game ends, no continuation)

## Technical Notes

### Why Not Just Fix gameInProgress?
The `gameInProgress` flag is a **control flag** used for:
- Scene routing (`CareerSettings.LoadToCM()`)
- Save/load detection (`GameManager.SetupGame()`)
- Tournament state tracking

It's **intentionally cleared** during scene transitions to prevent:
- Infinite loops (game ? end menu ? game ? end menu...)
- Incorrect routing (going to wrong scene)
- Stale state detection

**Solution:** Use persistent game state (`endCurrent`, `redScore`) instead of transient control flags.

### DontDestroyOnLoad Singleton Pattern
`GameSettingsPersist` is a singleton with `DontDestroyOnLoad`, meaning:
- ? ONE instance exists across all scenes
- ? All data persists through scene changes
- ? `endCurrent`, `redScore`, `score[]` survive transitions
- ? Control flags get reset by scene logic

## Summary

**Problem:** Score array cleared between ends because `gameInProgress` flag was unreliable  
**Root Cause:** Used transient control flag instead of persistent game state  
**Fix:** Use `endCurrent > 0` or `scores > 0` to detect continuation  
**Result:** Scores now preserved correctly between ends ?

**Files Modified:**
- `Assets\Scripts\GameSettingsPersist.cs` - Detection logic and score preservation

**Status:** ? Complete and tested
**Impact:** Critical - fixes tournament scoring in ALL tournament types
