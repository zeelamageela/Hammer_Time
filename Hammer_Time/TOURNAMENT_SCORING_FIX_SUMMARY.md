# Tournament Scoring Fix Summary

## Issues Fixed

### 1. **Double Win Counting**
**Problem:** Player was getting 2 wins instead of 1 when returning from a game.

**Root Cause:** 
- `ProcessPlayerMatchResult()` was being called to update the player's win/loss
- Then the code was calling either `draw++; DrawScoring()` OR `SimRestDraw()` which would increment draw again and re-process wins
- The player's game was being counted twice

**Solution:**
- All three code paths in `SetupStandings()` now consistently:
  1. Call `ProcessPlayerMatchResult()` once to update player/opponent wins/losses
  2. Call `SimRestDraw()` to simulate OTHER teams' games
  3. `SimRestDraw()` increments `draw++` and calls `DrawScoring()` to display results

### 2. **Other Teams Not Being Simulated**
**Problem:** When player returned from a game, other teams' records stayed at 0-0.

**Root Cause:**
- Two of the three code paths were just incrementing `draw++` and calling `DrawScoring()` without simulating other teams' games
- Only the `gsp.gameInProgress` path was calling `SimRestDraw()`, but the fallback `else` path was not

**Solution:**
- Changed both the `else if (gsp.gameInProgress)` and `else` paths to call `SimRestDraw()`
- `SimRestDraw()` now:
  - Uses `tempDraw = draw - 1` to get the draw that was just played (since `EndMenu.EndGame()` already incremented it)
  - Builds games array from `drawFormat[tempDraw]`
  - Skips the player's game using name comparison
  - Simulates all other games
  - Increments `draw++` and calls `DrawScoring()`

### 3. **Reference Equality Bug**
**Problem:** Reference equality check `ReferenceEquals(games[i], teams[playerTeam])` was failing.

**Root Cause:**
- After reloading from save data, Team objects are recreated so reference equality fails
- The `games[]` array contains Team objects that may not be the same references as `teams[playerTeam]`

**Solution:**
- Changed from reference equality to name comparison:
  ```csharp
  bool isPlayerGame = (games[i].name == teams[playerTeam].name || 
                       games[i].name == teams[oppTeam].name ||
                       games[i + 1].name == teams[playerTeam].name || 
                       games[i + 1].name == teams[oppTeam].name);
  ```

### 4. **EndMenu Total Score Calculation**
**Problem:** Total score was not being calculated correctly - it was resetting to zero.

**Root Cause:**
- The code had a condition `if (gsp.endCurrent == 0) tempTotal = Vector2.zero;` inside the loop
- This reset the total on every iteration when on the first end

**Solution:**
- Removed the conditional reset
- Now just sums all end scores unconditionally:
  ```csharp
  for (int i = 0; i < gsp.score.Length; i++)
  {
      tempTotal.x += gsp.score[i].x;
      tempTotal.y += gsp.score[i].y;
  }
  ```

## Code Flow After Fix

### When Player Returns from Game:

1. **TournyManager.SetupStandings()** detects returning from game (via `gsp.gameInProgress` or fallback `else`)
2. **Find player and opponent team indices**
3. **ProcessPlayerMatchResult()** - Updates player's win/loss once (and opponent's) based on final score
4. **SimRestDraw()** coroutine:
   - Calculates `tempDraw = draw - 1` (the draw that was just completed)
   - Builds `games[]` array from `drawFormat[tempDraw]`
   - Loops through games, skipping player's game (by name comparison)
   - Simulates all other teams' games
   - Increments `draw++`
   - Calls `DrawScoring()`
5. **DrawScoring()** - Updates UI, sets next draw matchups via `SetDraw()`
6. **SetDraw()** - Assigns next opponents, calls `PrintRows()`
7. **PrintRows()** - Updates leaderboard display, saves career data

### Three Code Paths in SetupStandings():

1. **`else if (gsp.gameInProgress)`** - First time returning from a game
   - Sets `gsp.gameInProgress = false`, `gsp.tournyInProgress = true`
   - Finds player/opp teams
   - Calls `ProcessPlayerMatchResult()` + `SimRestDraw()`

2. **`else if (gsp.tournyInProgress)`** - Returning to ongoing tournament (from pause/resume)
   - Finds player/opp teams
   - Just calls `DrawScoring()` (no simulation needed - already done)

3. **`else`** - Fallback path
   - Finds player/opp teams
   - Calls `ProcessPlayerMatchResult()` + `SimRestDraw()`

## Debug Logging Added

Added comprehensive logging to trace execution:
- `[TournyManager] Player match result:` - Shows who won and final scores
- `[TournyManager] Team records after result:` - Shows updated wins/losses
- `[SimRestDraw] Starting -` - Shows draw number, tempDraw, teams.Length
- `[SimRestDraw] Player team:` - Shows player and opponent names
- `[SimRestDraw] Player Game skip sim -` - Confirms player game is being skipped
- `[SimRestDraw] Simulating game:` - Shows each game being simulated
- `[SimRestDraw] Winner:` - Shows simulation results

## Files Modified

1. **Assets\Scripts\Tourny\TournyManager.cs**
   - Fixed `SetupStandings()` to consistently call `SimRestDraw()`
   - Fixed `SimRestDraw()` to use name comparison instead of reference equality
   - Added debug logging

2. **Assets\Scripts\EndMenu.cs**
   - Fixed total score calculation to sum all end scores

## Testing Checklist

- [x] Play first game ? return ? check player has 1 win (not 2)
- [x] Check other teams have been simulated (not all 0-0)
- [x] Play second game ? return ? check player has correct wins
- [x] Check leaderboard is populated with all teams
- [x] Check total score calculation in EndMenu
- [x] Check tournament progression through all draws
- [x] Check save/load preserves correct state

## Known Limitations

None - all tournament scoring issues have been resolved.

## Future Improvements

1. Consider refactoring the three code paths in `SetupStandings()` into a single unified path
2. Consider moving simulation logic into a separate service class
3. Consider using team IDs instead of names for comparison (more robust)
