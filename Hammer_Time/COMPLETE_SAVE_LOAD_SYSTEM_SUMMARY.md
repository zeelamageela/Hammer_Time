# Complete Save/Load System - Final Summary

## Overview

We fixed TWO separate but related save/load issues:

### Issue #1: Mid-Game Save/Load (Fixed Earlier)
**Problem**: Saved games didn't load rock positions, scores, or game state properly

**Solution**: 
- Don't override `gameInProgress` flag when loading
- Properly initialize score arrays
- Route to correct scene based on flag priority

**Files**: GameManager.cs, CareerSettings.cs

---

### Issue #2: Tournament Progress Save/Load (Fixed Just Now)
**Problem**: After finishing a game and quitting, tournament progress wasn't saved properly

**Solution**:
- Added `justFinishedGame` flag to distinguish "just won" from "loading between games"
- Save BEFORE loading next scene
- TournyManager checks new flag to process results

**Files**: EndMenu.cs, GameSettingsPersist.cs, TournyManager.cs

---

## Complete Flag System

The save/load system now uses **three critical flags**:

### 1. `gameInProgress` (bool)
**Meaning**: Is a game currently active (not finished)?

**Set to TRUE when**:
- Starting a new game (GameManager.SetupGame)
- User is mid-game (can save/quit)

**Set to FALSE when**:
- Game ends (GameManager.EndOfGame)
- Returning to tournament/menu

**Used for**:
- Routing: If true ? load into TournyGame scene
- State: Is this a saved mid-game state?

### 2. `tournyInProgress` (bool)
**Meaning**: Is a tournament currently active?

**Set to TRUE when**:
- Starting a tournament (TournySetup)
- Between games in tournament
- After finishing a game (still in tournament)

**Set to FALSE when**:
- Tournament completes (TournyManager.TournyComplete)
- Returning to arena selector
- Starting new tournament

**Used for**:
- Routing: If true (and gameInProgress false) ? load tournament home
- State: Should we load tournament data?

### 3. `justFinishedGame` (bool) ? **NEW!**
**Meaning**: Did we JUST finish a game and need to process the result?

**Set to TRUE when**:
- EndMenu.EndGame() after winning/losing a game

**Set to FALSE when**:
- TournyManager processes the result (immediately)

**Used for**:
- TournyManager: "Process this game result and update standings"
- Distinguishes "just won game" from "loading between games"

---

## Complete Save/Load Flow

### Scenario A: Mid-Game Save/Load

**SAVE:**
```
1. GameManager.NextTurn()
   - Sets gsp.rockPos[]
   - Sets gsp.rockInPlay[]
   - Calls SaveGame()

2. GameSettingsPersist.AutoSave()
   - Delegates to CareerManager

3. CareerManager.SaveCareer()
   - Captures gameInProgress = true
   - Captures tournyInProgress = true
   - Captures justFinishedGame = false
   - Captures rockPos, rockInPlay, scores
   - Writes to JSON via CareerSaveService
```

**LOAD:**
```
1. CareerSettings.Start()
   - cm.LoadCareer()
   - Restores ALL flags from save

2. CareerSettings.LoadToCM()
   - Checks: gameInProgress == TRUE
   - Sets: gsp.loadGame = true
   - Routes to: "TournyGame"

3. GameManager.SetupGame()
   - Checks: gsp.loadGame == TRUE
   - Preserves: gsp.gameInProgress (don't override!)
   - Calls: LoadGame()

4. GameManager.LoadGame()
   - Calls: PlaceRocks()
   - Uses: gsp.rockPos[], gsp.rockInPlay[]
   - Continues from saved position ?
```

### Scenario B: Finish Game ? Tournament Home

**SAVE:**
```
1. GameManager.EndOfGame()
   - Sets gsp.gameInProgress = false
   - Sets gsp.loadGame = false
   - Calls SaveGame()

2. EndMenu.EndGame()
   - Sets gsp.justFinishedGame = true  ? KEY!
   - Keeps gsp.tournyInProgress = true
   - Increments gsp.draw
   - Calls cm.SaveCareer()  ? BEFORE scene load!
   - Loads "Tourny_Home_1"
```

**IMMEDIATE PROCESSING (no quit):**
```
3. TournyManager.SetupStandings()
   - Checks: gsp.justFinishedGame == TRUE
   - Calls: ProcessPlayerMatchResult()
   - Updates wins/losses
   - Calls: SimRestDraw()
   - Sets gsp.justFinishedGame = false
   - Displays updated standings ?
```

**LOAD (if user quit):**
```
1. CareerSettings.Start()
   - cm.LoadCareer()
   - Restores justFinishedGame = true
   - Restores tournyInProgress = true

2. CareerSettings.LoadToCM()
   - Checks: gameInProgress == FALSE
   - Checks: tournyInProgress == TRUE
   - Routes to: "Tourny_Home_1"  ?

3. TournyManager.SetupStandings()
   - Checks: gsp.justFinishedGame == TRUE
   - Calls: ProcessPlayerMatchResult()  (safe to call again!)
   - Updates wins/losses
   - Sets gsp.justFinishedGame = false
   - Displays updated standings ?
```

### Scenario C: Tournament Ends

**SAVE:**
```
1. TournyManager.TournyComplete()
   - Sets gsp.tournyInProgress = false
   - Sets gsp.gameInProgress = false
   - Calls cm.TournyResults()
   - Calls cm.SaveCareer()
   - Loads "Arena_Selector"
```

**LOAD:**
```
1. CareerSettings.LoadToCM()
   - Checks: gameInProgress == FALSE
   - Checks: tournyInProgress == FALSE
   - Routes to: "Arena_Selector" ?
```

---

## Flag Priority Matrix

When loading, check flags in this order:

| gameInProgress | tournyInProgress | justFinishedGame | Action |
|---------------|------------------|------------------|---------|
| TRUE | TRUE | FALSE | Load into TournyGame (mid-game) |
| FALSE | TRUE | TRUE | Load Tourny Home ? Process result |
| FALSE | TRUE | FALSE | Load Tourny Home ? Display standings |
| FALSE | FALSE | FALSE | Load Arena Selector (normal career) |

**Key Rule**: Check `justFinishedGame` BEFORE `gameInProgress` in TournyManager!

---

## Files Changed (Complete List)

### Original Save/Load Fixes:
1. **Assets/Scripts/GameManager.cs**
   - Don't override gameInProgress on load
   - Properly initialize score array
   - Add logging to EndOfGame

2. **Assets/Scripts/Tourny/CareerSettings.cs**
   - Simplified LoadToCM routing
   - Clear arrays on new career

### Tournament Progress Fixes (Today):
3. **Assets/Scripts/GameSettingsPersist.cs**
   - Added `public bool justFinishedGame;` field

4. **Assets/Scripts/EndMenu.cs**
   - Set justFinishedGame = true before save
   - Save BEFORE loading scene
   - Added logging

5. **Assets/Scripts/Tourny/TournyManager.cs**
   - Check justFinishedGame flag first
   - Process result when flag is set
   - Clear flag after processing

---

## Common Issues & Debugging

### Issue: Save loads to wrong scene

**Check:**
1. Print flags in LoadToCM: `gameInProgress`, `tournyInProgress`, `justFinishedGame`
2. Check routing logic priority
3. Verify save happened before scene load

**Log**: `[CareerSettings] LoadToCM - tournyInProgress: X, gameInProgress: Y`

### Issue: Tournament standings not updated

**Check:**
1. Was `justFinishedGame` set in EndMenu?
2. Did TournyManager process it?
3. Was save called BEFORE scene load?

**Log**: `[TournyManager] Just finished game - processing result`

### Issue: Rock positions not restored

**Check:**
1. Was `gsp.rockPos` set in NextTurn?
2. Was `gsp.loadGame` set to true in LoadToCM?
3. Did GameManager call LoadGame()?

**Log**: `[GameManager] LOADING game - gameInProgress preserved from save`

---

## Testing Protocol

### Test 1: Mid-Game Save
1. Start game
2. Play 2-3 turns
3. Quit game
4. Continue
5. **Verify**: Rocks are in correct positions
6. **Verify**: Scores match
7. **Verify**: Current end/rock correct

### Test 2: Tournament Progress
1. Play first tournament game ? win
2. **Verify**: Standings show win immediately
3. Quit at tournament home
4. Continue
5. **Verify**: Loads to tournament home (not arena)
6. **Verify**: Standings show win
7. **Verify**: Record updated

### Test 3: Tournament Complete
1. Finish last game of tournament
2. **Verify**: Loads to end menu
3. Click finish
4. **Verify**: Loads to arena selector
5. Quit
6. Continue
7. **Verify**: Loads to arena selector (not tournament)

### Test 4: New Career
1. Start new career
2. **Verify**: All flags cleared
3. **Verify**: No leftover data from previous career

---

## Architecture Notes

### Why Three Flags?

**Before (buggy):**
- `gameInProgress` meant "game active" OR "just finished"
- Ambiguous state ? wrong routing

**After (fixed):**
- `gameInProgress` = game active right now
- `justFinishedGame` = game just ended, process it
- `tournyInProgress` = tournament context active

**Result**: Clear, unambiguous states!

### Why Save Before Scene Load?

**Problem**: If scene loads first, user could quit before save
- Tournament result lost!
- Record not updated!

**Solution**: Save FIRST, then load scene
- Even if user quits during scene load, data is safe
- TournyManager can process result multiple times (idempotent)

### Why Clear justFinishedGame?

**Problem**: If not cleared, every load would re-process the result

**Solution**: Clear immediately after processing
- Flag is "single-use"
- Safe to process twice (idempotent) but avoid unnecessary work

---

## Final Status

? **All fixes complete**
? **Build successful**
? **Documentation complete**

**Ready for testing!**

---

## Quick Reference: Flag Checklist

**When saving mid-game:**
- [ ] gameInProgress = true
- [ ] tournyInProgress = true (if in tournament)
- [ ] justFinishedGame = false
- [ ] rockPos[] populated
- [ ] rockInPlay[] populated

**When finishing game:**
- [ ] gameInProgress = false
- [ ] tournyInProgress = true
- [ ] justFinishedGame = true
- [ ] Save called BEFORE scene load

**When tournament ends:**
- [ ] gameInProgress = false
- [ ] tournyInProgress = false
- [ ] justFinishedGame = false

**When starting new career:**
- [ ] ALL flags = false
- [ ] ALL arrays = null or cleared
