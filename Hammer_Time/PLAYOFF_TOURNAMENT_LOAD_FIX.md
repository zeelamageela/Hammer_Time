# Playoff Tournament Load Fix

?? **NOTE: This document describes the INITIAL fix attempt. See `PLAYOFF_TOURNAMENT_LOAD_FIX_FINAL.md` for the complete, working solution.**

---

## Problem Summary

When saving a tournament in the semifinals (Round 2) and loading it later, the system would incorrectly show the Finals (Round 3) bracket instead of the Semifinals bracket. The playoff matchups would be incorrect, and the player would be advanced to the wrong round.

## Root Cause

The issue was caused by **two separate problems** working together:

### Problem 1: Incorrect Start() Logic

**Location:** `PlayoffManager.Start()` lines 45-75

**Before:**
```csharp
if (gsp.careerLoad && !gsp.gameInProgress)
{
    // Scenario 3: Loading saved tournament, player is between games
    LoadPlayoffs();
}
else if (gsp.gameInProgress)  // ? WRONG!
{
    // Scenario 2: Returning from a game - need to advance
    LoadAndAdvancePlayoffs();
}
```

**Problem:** The `gsp.gameInProgress` flag is set to `true` while a game is in progress, but it remains `true` even after the game completes (until you return to the tournament screen). This meant that **loading a saved tournament after completing a game** would incorrectly call `LoadAndAdvancePlayoffs()` instead of `LoadPlayoffs()`.

**After (Fixed):**
```csharp
if (gsp.justFinishedGame && !gsp.careerLoad)
{
    // Scenario 2: Just returned from completing a game - need to process result and advance
    LoadAndAdvancePlayoffs();
    gsp.justFinishedGame = false; // Clear flag after processing
}
else if (gsp.careerLoad && !gsp.gameInProgress && !gsp.justFinishedGame)
{
    // Scenario 3: Loading saved tournament, player is between games - just restore state
    LoadPlayoffs();
}
```

**Fix:** Now uses the `justFinishedGame` flag, which is specifically set when returning from a completed game and cleared after processing.

### Problem 2: Round Increment in LoadPlayoffs()

**Location:** `PlayoffManager.LoadPlayoffs()` lines 536-540

**Before:**
```csharp
void LoadPlayoffs()
{
    gsp.careerLoad = false;

    if (gsp.gameInProgress)  // ? WRONG!
    {
        playoffRound++;  // ? Increments even when just loading a saved tournament!
    }
```

**Problem:** This line would increment `playoffRound` when loading **any** saved tournament (because `gameInProgress` was still true). This caused the symptoms you saw:
- Save in Semifinals (Round 2)
- Load game ? `LoadPlayoffs()` is called
- `playoffRound++` runs ? Round becomes 3 (Finals)
- Wrong bracket is displayed!

**After (Fixed):**
```csharp
void LoadPlayoffs()
{
    Debug.Log($"[LoadPlayoffs] Starting - playoffRound={playoffRound}");

    // CRITICAL: This method should ONLY restore saved state, NOT advance rounds
    // Round advancement is handled by LoadAndAdvancePlayoffs() via justFinishedGame flag
    gsp.careerLoad = false;

    Debug.Log($"[LoadPlayoffs] Restoring playoff state for round {playoffRound}");
    // ... rest of method (no increment)
}
```

**Fix:** Removed the `playoffRound++` line entirely. `LoadPlayoffs()` now **only** restores state, it never advances rounds.

## The Four Scenarios

The fixed system now correctly handles four distinct scenarios:

| Scenario | Flags | Method Called | Behavior |
|----------|-------|---------------|----------|
| **1. Fresh Tournament Start** | `playoffRound==0`, `!careerLoad`, `!gameInProgress` | `SetSeeding()` | Seeds teams from standings, starts Round 1 |
| **2. Returning from Completed Game** | `justFinishedGame==true`, `!careerLoad` | `LoadAndAdvancePlayoffs()` | Processes game result, advances to next round |
| **3. Loading Saved Tournament** | `careerLoad==true`, `!gameInProgress`, `!justFinishedGame` | `LoadPlayoffs()` | **Restores exact saved state** (THIS WAS BROKEN!) |
| **4. Resuming Mid-Game** | `careerLoad==true`, `gameInProgress==true` | `LoadPlayoffs()` | Restores in-progress game state |

## Testing Recommendations

To verify the fix works:

1. **Test Scenario 3 (The Bug):**
   - Start a tournament
   - Complete Round 1
   - **Save between games** (in Semifinals screen)
   - Exit game
   - Load save
   - ? **Expected:** Should show Semifinals bracket (Round 2)
   - ? **Before Fix:** Would show Finals bracket (Round 3)

2. **Test Scenario 2 (Advancement):**
   - Start a tournament
   - Complete a game
   - Return to tournament screen
   - ? **Expected:** Should advance to next round with correct results

3. **Test Round Transitions:**
   - Verify Round 1 ? Round 2 works correctly
   - Verify Round 2 ? Round 3 (Finals) works correctly
   - Verify Round 3 ? Round 4 (Complete) works correctly

## Changes Made

### File: `Assets/Scripts/Tourny/PlayoffManager.cs`

1. **Modified `Start()` method** (lines 45-75):
   - Changed condition from `gsp.gameInProgress` to `gsp.justFinishedGame`
   - Added flag clearing after processing
   - Improved scenario detection logic
   - Added comprehensive debug logging

2. **Modified `LoadPlayoffs()` method** (lines 536-540):
   - **Removed** `if (gsp.gameInProgress) { playoffRound++; }` block
   - Added debug logging
   - Added comments explaining the method's purpose

## Related Files

This fix relies on the `justFinishedGame` flag being properly managed in:
- `CareerManager.cs` - Saves and restores the flag
- `GameSettingsPersist.cs` - Stores the flag

The flag is set when a game completes and is saved in the tournament state, ensuring correct behavior when loading saved tournaments.

## Summary

The bug was caused by:
1. Using the wrong flag (`gameInProgress` instead of `justFinishedGame`) to detect returning from a game
2. Incorrectly incrementing `playoffRound` in `LoadPlayoffs()` when it should only restore state

The fix ensures that:
- **Loading a saved tournament** simply restores the exact saved state (no round advancement)
- **Returning from a completed game** processes the result and advances to the next round
- The system correctly distinguishes between these two scenarios using the `justFinishedGame` flag
