# Save/Load Testing Guide

## Phase 1 Critical Fixes - What Changed

### 1. **Concurrent Save Protection**
**Problem**: Multiple saves happening at once could corrupt the file
**Fix**: Added mutex lock to prevent concurrent saves
**Test**: Spam the save button rapidly - should see "Save already in progress" messages

### 2. **Comprehensive Null Checks**
**Problem**: Crashes when save file has null or missing fields  
**Fix**: Null checks at every level with graceful fallbacks
**Test**: Manually edit save file to remove fields, game should recover

### 3. **Enhanced Validation**
**Problem**: Invalid game states were accepted
**Fix**: Validates game settings, auto-clears invalid flags
**Test**: Set `gameInProgress=true` but `endScores=null` - should auto-fix

### 4. **Auto-Repair**
**Problem**: Partial corruption caused crashes
**Fix**: Automatically repairs common issues (missing arrays, invalid values)
**Test**: Delete `endScores` array from save - should create empty array

### 5. **Graceful Degradation**
**Problem**: Any error caused complete failure
**Fix**: Try-catch blocks with sensible fallbacks, never crash
**Test**: Corrupt save in various ways - should always load *something*

## Testing Checklist for Testers

### Normal Operation (Should all pass)

- [ ] **New Career** - Start fresh career, play 1 game, save, quit, continue → Works
- [ ] **Mid-Game Save** - Save during a game, quit, continue → Resume at exact position
- [ ] **End Menu Save** - View end menu, quit, continue → Return to end menu with scores
- [ ] **Tournament Save** - Play tournament, quit between games, continue → Resume tournament
- [ ] **Multiple Saves** - Save multiple times rapidly → No corruption, latest save wins

### Error Recovery (Should all gracefully recover)

- [ ] **Corrupted Save File**
  - Manually truncate `career_save.json` to half size
  - Launch game and hit Continue
  - **Expected**: Load from backup, show warning in console
  
- [ ] **Missing Backup**
  - Delete `career_save_backup.json`
  - Corrupt `career_save.json` (invalid JSON)
  - Launch game and hit Continue  
  - **Expected**: Start fresh career, show error in console
  
- [ ] **Null Game State**
  - Edit save JSON: set `"currentGameState": null`
  - Continue game
  - **Expected**: Load career data, clear game flags, tournament continues
  
- [ ] **Missing End Scores**
  - Edit save JSON: remove `"endScores"` field
  - Set `"gameInProgress": true`
  - Continue game
  - **Expected**: Auto-repair with empty score array OR clear gameInProgress flag
  
- [ ] **Invalid Team Names**
  - Edit save JSON: set `"redTeamName": ""`
  - Continue game
  - **Expected**: Default to "Red" and "Yellow"

- [ ] **Negative Values**
  - Edit save JSON: set `"ends": -1`, `"currentEnd": -5`
  - Continue game
  - **Expected**: Clamp to valid ranges (ends=8, currentEnd=0)

### Stress Testing

- [ ] **Rapid Saves During Gameplay**
  - Use auto-save (every 10 seconds) + manual save button
  - Click save button repeatedly while game is auto-saving
  - **Expected**: Only one save happens at a time, no corruption

- [ ] **Save During Scene Transition**
  - Trigger save, immediately quit to main menu
  - **Expected**: Save completes or is safely cancelled

- [ ] **Low Storage Space** (iOS/Android specific)
  - Fill device storage to <10MB free
  - Try to save
  - **Expected**: "Insufficient storage" error, old save untouched

- [ ] **Quick Save/Load Cycles**
  - Save → Quit → Continue → Save → Quit → Continue (repeat 10x)
  - **Expected**: No degradation, each load shows correct state

## What to Look For In Logs

### Good Signs (Normal Operation)
```
[CareerSaveService] Career saved successfully to: ...
[CareerSaveService] Save size: 188298 bytes
[CareerManager] Restored flags from save - gameInProgress: False, ...
[CareerManager] Game state restored: End 2/8, Rock 0/8
```

### Warning Signs (Auto-Repair Working)
```
[CareerSaveService] Game in progress but endScores is null - clearing gameInProgress flag
[CareerManager] No end scores in save data - gameState.endScores is NULL
[CareerManager] Auto-repair: Creating score array for 8 ends
[CareerManager] End 1: NULL - initialized to 0-0
```

### Error Signs (Needs Investigation)
```
[CareerSaveService] Save already in progress - skipping concurrent save  ← Too frequent = problem
[CareerSaveService] Failed to load backup: ...  ← Both files corrupt
[CareerManager] Critical error during game state restoration: ...  ← Unexpected corrupt data
[CareerSaveService] Insufficient storage space to save  ← Storage issue
```

## Common Issues & Solutions

### Issue: "Save already in progress" every time
**Cause**: Previous save never finished (deadlock)
**Fix**: Restart game - save mutex is reset on launch
**Prevention**: Fixed in Phase 1 with `finally` block

### Issue: Loads backup every time
**Cause**: Main save is consistently corrupt
**Check**: Save file location, file permissions (iOS)
**Fix**: Delete save and start fresh

### Issue: Game state not preserved
**Cause**: Save triggered before state fully set
**Check**: Is `gameInProgress` true when saving?
**Fix**: Ensure save happens AFTER game state is set

### Issue: Scores show as 0-0 after load
**Cause**: Score array not saved or restored properly
**Check**: Look for "No end scores in save data" in logs
**Fix**: Now auto-repairs with Phase 1 fixes

## How to Manually Test Save Files

### View Save File
```bash
# macOS/Linux
cat ~/Library/Application\ Support/ZickyBoy/HammerTime/career_save.json | python -m json.tool | head -50

# Windows
type %APPDATA%\..\LocalLow\ZickyBoy\HammerTime\career_save.json
```

### Corrupt Save File (for testing)
```bash
# Truncate file
head -c 5000 career_save.json > career_save.json

# Invalid JSON
echo "INVALID" >> career_save.json

# Remove field
# Edit with text editor, delete a field like "endScores"
```

### Check File Sizes
```bash
ls -lh career_save*.json
# career_save.json should be 150-250KB
# If significantly different, investigate
```

## Reporting Issues to Developer

If testers find persistent save/load failures, collect:

1. **Console logs** - Full log from launch to failure
2. **Save file** - Copy of `career_save.json` and backup
3. **Steps to reproduce** - Exact sequence of actions
4. **Device info** - iOS/Android version, storage free
5. **Timing** - When did save happen? After what event?

**Include these tags in bug reports:**
- `#save-corruption` - File corrupted
- `#save-failure` - Save didn't write
- `#load-failure` - Load crashed or wrong data
- `#backup-failure` - Both main + backup bad
- `#race-condition` - Concurrent issues
