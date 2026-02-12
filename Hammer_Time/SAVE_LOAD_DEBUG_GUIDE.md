# Save/Load System - Debug Guide

## Quick Diagnosis

If save/load isn't working, check these in order:

### 1. Check Flags After Loading
Look for these log messages when loading a save:

```
[CareerSettings] LoadToCM - tournyInProgress: X, gameInProgress: Y, week: Z
[CareerManager] Game state restored: End X/Y, Rock A/B
[GameManager] LOADING game - gameInProgress preserved from save: X
```

**What to look for:**
- `gameInProgress` should be `true` if you saved mid-game
- `tournyInProgress` should be `true` if you saved during a tournament
- Week should match your career progress

### 2. Check Scene Routing
After the flags, you should see ONE of these:

```
[CareerSettings] Loading mid-game save ? TournyGame
[CareerSettings] Loading tournament (between games) ? Tournament Home  
[CareerSettings] No tournament/game in progress ? Arena Selector
```

**What to look for:**
- Mid-game saves should go to `TournyGame`
- Between-games tournament saves should go to tournament home
- Normal career should go to `Arena_Selector`

### 3. Check Data Restoration
If the game loads but data is wrong:

```
[GameManager] Initializing score array for X ends
[GameManager] Placing Rock Position 0: (x, y)
[GameManager] Updated cm.record to X-Y from team NAME
```

**What to look for:**
- Score array should have `ends + 1` size
- Rock positions should be restored if mid-game
- Record should match your actual wins/losses

## Common Issues & Fixes

### Issue: Save loads to wrong scene

**Symptom**: Mid-game save loads to tournament home instead of game

**Debug**:
1. Check `[CareerSettings] LoadToCM` log - what flags does it see?
2. If `gameInProgress = false` but you saved mid-game, the save didn't capture it correctly

**Fix**: Check that `GameManager.SaveGame()` is called AFTER setting `gsp.rockPos` and `gsp.rockInPlay`

### Issue: Rock positions not restored

**Symptom**: Game loads but all rocks are gone or in wrong positions

**Debug**:
1. Check if `gsp.rockPos` is set: look for `[GM.NextTurn] rockPos[0] = (x, y)` logs
2. Check if `PlaceRocks()` is called: look for `[GameManager] Placing Rock Position` logs
3. Check `gsp.loadGame` flag - should be `true` when loading mid-game

**Fix**: Make sure `gsp.loadGame = true` in `CareerSettings.LoadToCM()` when `gameInProgress = true`

### Issue: Scores are wrong after loading

**Symptom**: Scores show 0-0 or incorrect values

**Debug**:
1. Check `[GameManager] Final score:` log from EndOfGame
2. Check `gsp.redScore` and `gsp.yellowScore` in CareerManager save
3. Check `score` array size - should be `ends + 1`

**Fix**: Make sure `SaveGame()` is called AFTER `gsp.LoadFromGM()` which sets the scores

### Issue: New career has leftover data

**Symptom**: Starting fresh but see old tournament names, scores, etc.

**Debug**:
1. Check `[CareerSettings] Deleting existing save for new career` - did it delete?
2. Check flags after `New()`: all should be false/0/null
3. Check `[CareerSettings] New career - cleared all game state flags`

**Fix**: Make sure `CareerSettings.New()` sets all flags explicitly to default values

## Save Data Inspection

To manually inspect a save file:

**Location**: `{Application.persistentDataPath}/career_save.json`

**Windows**: `C:\Users\{USERNAME}\AppData\LocalLow\{CompanyName}\{GameName}\career_save.json`
**Mac**: `~/Library/Application Support/{CompanyName}/{GameName}/career_save.json`
**iOS**: App sandbox, not directly accessible

**Structure**:
```json
{
  "version": 1,
  "saveDate": "2024-01-15 10:30:00",
  "currentGameState": {
    "gameInProgress": true,    // ? Should be true for mid-game saves
    "tournyInProgress": true,
    "currentEnd": 2,
    "currentRock": 5,
    "rockPositions": [        // ? Should have positions if mid-game
      {"x": 1.5, "y": 3.2},
      ...
    ],
    "rockInPlay": [true, true, false, ...],
    "endScores": [
      {"x": 2, "y": 0},      // End 1: Red 2, Yellow 0
      {"x": 0, "y": 1}       // End 2: Red 0, Yellow 1
    ]
  }
}
```

**What to check:**
- `gameInProgress` matches when you saved
- `rockPositions` has entries if mid-game
- `endScores` matches score history
- `currentEnd` and `currentRock` are correct

## Flag State Machine

The flags should transition like this:

```
NEW CAREER:
  gameInProgress = false
  tournyInProgress = false
  loadGame = false

ENTER TOURNAMENT:
  gameInProgress = false
  tournyInProgress = true  ? Set by TournySetup()
  loadGame = false

START GAME:
  gameInProgress = true    ? Set by TournySetup() or GameManager
  tournyInProgress = true
  loadGame = false

MID-GAME SAVE:
  gameInProgress = true    ? Stays true
  tournyInProgress = true
  loadGame = false         ? Will be set to true on load

LOAD MID-GAME:
  gameInProgress = true    ? From save
  tournyInProgress = true  ? From save
  loadGame = true          ? Set by LoadToCM()

FINISH GAME:
  gameInProgress = false   ? Cleared by EndOfGame()
  tournyInProgress = true  ? Still in tournament
  loadGame = false

FINISH TOURNAMENT:
  gameInProgress = false
  tournyInProgress = false ? Cleared by TournyComplete()
  loadGame = false
```

## Debug Commands (if you add them)

Suggested debug commands to add for testing:

```csharp
// In GameManager or a DebugManager:
public void DebugDumpSaveState()
{
    Debug.Log($"=== SAVE STATE ===");
    Debug.Log($"gameInProgress: {gsp.gameInProgress}");
    Debug.Log($"tournyInProgress: {gsp.tournyInProgress}");
    Debug.Log($"loadGame: {gsp.loadGame}");
    Debug.Log($"endCurrent: {gsp.endCurrent}/{gsp.ends}");
    Debug.Log($"rockCurrent: {gsp.rockCurrent}");
    Debug.Log($"scores: {gsp.redScore}-{gsp.yellowScore}");
    Debug.Log($"rockPos array: {(gsp.rockPos != null ? gsp.rockPos.Length : 0)} rocks");
    Debug.Log($"==================");
}
```

## Performance Notes

The save/load system uses JSON serialization which is:
- ? Human-readable (good for debugging)
- ? Cross-platform compatible
- ?? Relatively slow for very large saves
- ?? Not compressed

If save files become too large (> 1MB), consider:
1. Compressing JSON with GZip
2. Using binary serialization for rock positions
3. Only saving rocks that are in-play

Current typical save size: ~50-100KB (acceptable)

## Support Checklist

If a user reports save/load issues, ask for:

1. **Platform**: iOS, Android, PC, Mac?
2. **When did they save**: Mid-game? Between ends? Between games?
3. **What happens on load**: Wrong scene? Missing data? Crash?
4. **Save file** (if on PC/Mac): Ask them to send `career_save.json`
5. **Logs** (if Unity logs available): Last 50 lines before/after load

Then follow the diagnosis steps above.
