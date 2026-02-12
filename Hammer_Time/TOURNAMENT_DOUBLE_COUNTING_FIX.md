# Tournament Double-Counting Fix

## Problem

After implementing the playoff advancement fix, the tournament system had **two separate double-counting issues**:

1. **Player wins/losses were double-counted** - Getting 2 wins/losses per game instead of 1
2. **AI teams were playing extra games** - AI vs AI teams were getting 2 games per round instead of 1

## Root Causes

### Issue 1: Player Double-Counting

The player's game result was being processed in **two places**:

```csharp
// EndMenu.EndGame()
gsp.LoadFromEndMenu();     // ? Updates player wins/losses in gsp.teams

// TournyManager.SetupStandings()
ProcessPlayerMatchResult();  // ? Updates player wins/losses AGAIN!
```

### Issue 2: AI Games Double-Simulation

AI vs AI games were being simulated in **two places**:

```csharp
// EndMenu.EndGame()
SimulateOtherGames();      // ? Simulates ALL AI vs AI games (FIRST TIME)

// TournyManager.SetupStandings()
SimRestDraw();             // ? Simulates ALL AI vs AI games AGAIN! (SECOND TIME)
```

**The Problem Flow:**

1. Player finishes game ? `EndMenu.EndGame()` is called
2. ? `gsp.LoadFromEndMenu()` updates `teams[playerTeam].wins++` 
3. ? `SimulateOtherGames()` simulates all AI vs AI games
4. Save and load tournament home
5. `TournyManager.SetupStandings()` runs with `justFinishedGame = true`
6. ? `ProcessPlayerMatchResult()` AGAIN does `teams[playerTeam].wins++` (Player gets 2 wins!)
7. ? `SimRestDraw()` simulates all AI vs AI games AGAIN (AI teams get 2 games!)

## Why This Happened

Both `LoadFromEndMenu()` and `SimulateOtherGames()` were originally designed for **playoff tournaments** where:
- Team stats need immediate updating for bracket advancement
- There are no "other games" to simulate (single-elimination bracket)

But for **regular tournaments**, the wins/losses and AI game simulation should **ONLY** happen in `TournyManager`:
- `ProcessPlayerMatchResult()` handles the player's game
- `SimRestDraw()` handles all AI vs AI game simulation

The `gsp.teams` array is **the same reference** used by both `EndMenu` and `TournyManager`, so updating/simulating in both places = double-counting!

## The Fix

**Changed:** `EndMenu.EndGame()` to:
1. Only call `LoadFromEndMenu()` for **playoff games**
2. **Never** call `SimulateOtherGames()` (TournyManager handles all AI simulation)

```csharp
// NEW CODE - FIXED
if (gsp.tourny)
{
    // CRITICAL FIX: Only call LoadFromEndMenu for PLAYOFFS
    if (gsp.playoffRound > 0)
    {
        // Playoffs: Update team records in GSP
        gsp.LoadFromEndMenu();
        Debug.Log("[EndMenu] Playoff game - updated team records via LoadFromEndMenu");
    }
    else
    {
        // Regular tournament: Don't update here - TournyManager will handle it
        Debug.Log("[EndMenu] Regular tournament - TournyManager will process player match result");
    }
    
    // CRITICAL FIX: Don't simulate other games here!
    // For regular tournaments, TournyManager.SimRestDraw() handles ALL AI game simulation
    // For playoffs, there are no "other games" to simulate (single-elimination bracket)
    // Simulating here would cause DOUBLE simulation for regular tournaments!
    
    // Set justFinishedGame flag
    gsp.justFinishedGame = true;
    // ...rest of the code
}
```

## How It Works Now

### Regular Tournament Flow (CORRECT)

```
1. Game ends ? EndMenu.EndGame()
        ?
2. Check: gsp.playoffRound > 0?  ? NO (regular tournament)
        ?
3. SKIP LoadFromEndMenu() ? Don't update wins/losses here!
        ?
4. SKIP SimulateOtherGames() ? Don't simulate AI games here!
        ?
5. Set justFinishedGame = true
        ?
6. Save and load tournament home
        ?
7. TournyManager.SetupStandings() runs
        ?
8. Sees justFinishedGame = true
        ?
9. Calls ProcessPlayerMatchResult() ? Updates wins/losses (ONCE!)
        ?
10. Calls SimRestDraw() ? Simulates AI games (ONCE!)
        ?
11. Displays updated standings (correct wins/losses for ALL teams)
```

### Playoff Tournament Flow (CORRECT)

```
1. Game ends ? EndMenu.EndGame()
        ?
2. Check: gsp.playoffRound > 0?  ? YES (playoff game)
        ?
3. Call LoadFromEndMenu() ? Update wins/losses for playoff bracket
        ?
4. SKIP SimulateOtherGames() ? No other games in playoffs!
        ?
5. Set justFinishedGame = true
        ?
6. Save and load tournament home
        ?
7. PlayoffManager.Start() runs
        ?
8. Sees justFinishedGame = true
        ?
9. Calls LoadAndAdvancePlayoffs() ? Advances bracket to next round
        ?
10. Displays updated bracket
```

## Why EndMenu Should NOT Simulate AI Games

**For Regular Tournaments:**
- TournyManager owns the draw system and knows which teams play each other
- TournyManager.SimRestDraw() has the draw format data to simulate correctly
- EndMenu doesn't know about draws - it only knows player vs opponent

**For Playoffs:**
- Single-elimination bracket means there are NO "other games"
- If you're in the playoffs and just finished a game, ALL other games in that round are also done
- There's nothing to simulate!

## Why Playoffs Need LoadFromEndMenu()

Playoffs use `LoadFromEndMenu()` because:
1. The playoff bracket needs the wins/losses to determine which teams advance
2. PlayoffManager.LoadAndAdvancePlayoffs() relies on team stats being current
3. There's no separate "ProcessPlayerMatchResult" in PlayoffManager - it uses the GSP team stats directly

## Testing

### Test 1: Regular Tournament - Player Stats (No Double-Counting)
1. Start a regular tournament
2. Play and win your first game
3. Return to tournament standings
4. ? **Expected:** You should have exactly 1 win, opponent has 1 loss
5. ? **Before Fix:** You would have 2 wins, opponent has 2 losses

### Test 2: Regular Tournament - AI Team Stats (No Extra Games)
1. Start a regular tournament
2. Play your first game (any result)
3. Check standings for AI teams
4. ? **Expected:** Each AI team should have exactly 1 game played (1 win OR 1 loss)
5. ? **Before Fix:** AI teams would have 2 games (2 wins, 2 losses, or 1-1 record)

### Test 3: Regular Tournament - Multiple Rounds
1. Start a regular tournament
2. Play games in rounds 1, 2, and 3
3. Check AI team records after each round
4. ? **Expected:** AI teams should have 1, 2, 3 games played respectively
5. ? **Before Fix:** AI teams would have 2, 4, 6 games played

### Test 4: Playoffs - Still Work Correctly
1. Start a tournament and reach the playoffs
2. Play and win a playoff game
3. Return to playoff bracket
4. ? **Expected:** Bracket advances to next round
5. ? **Expected:** Your wins are counted correctly
6. ? **Expected:** No "extra games" for anyone (playoffs don't have concurrent games)

## Summary

**The fix:** 
1. **Removed** `LoadFromEndMenu()` call for regular tournaments (only used for playoffs)
2. **Removed** `SimulateOtherGames()` call entirely (TournyManager handles ALL AI simulation)

**Responsibilities:**
- **EndMenu:** Only sets flags (`justFinishedGame`) and manages playoff stats
- **TournyManager:** Handles ALL game result processing and AI simulation for regular tournaments
- **PlayoffManager:** Handles bracket advancement using stats from `LoadFromEndMenu()`

**Files Changed:**
- `Assets/Scripts/EndMenu.cs` - Lines 1187-1210

**Result:**
? Regular tournaments: Player gets 1 win per game (not 2)
? Regular tournaments: AI teams get 1 game per round (not 2)
? Playoffs: Still advance correctly with correct stats
? Clean separation of responsibilities between managers
