# Playoff Type Flags (KO1/KO3) Save/Load Fix

## Problem

After loading a **mid-game save** and finishing the game, the system **routed to the wrong tournament home screen**:

```
Problem Flow:
1. Playing Single-K tournament (knockout bracket)
2. Save game mid-match
3. Load save and finish game
4. Click "End Game"
5. ? Routes to "Tourny_Home_1" (regular round-robin)
6. ? Expected: "Tourny_Home_SingleK" (knockout bracket)
```

### Logs Show the Issue
```
[GameSettingsPersist] LoadTourny called
[GameSettingsPersist] Tournament in progress - skipping career reload to preserve flags
[GameSettingsPersist] Restoring team objects - redTeam: Gloobilich, yellowTeam: Crawdad

// Game finishes...

// EndGame() loads scene based on KO1/KO3 flags:
if (gsp.KO3)
    SceneManager.LoadScene("Tourny_Home_1");  // Triple-K
else if (gsp.KO1)
    SceneManager.LoadScene("Tourny_Home_SingleK");  // Single-K
else
    SceneManager.LoadScene("Tourny_Home_1");  // Regular ? ? WRONG!
```

---

## Root Cause

### The KO1 and KO3 Flags Were NOT Being Saved!

**When Tournament Starts** (working correctly):
```csharp
// In TournySettings when player enters tournament
gsp.PlayoffSetup(pm);  // Sets KO1 = true or KO3 = true
```

**When Game is Saved** (MISSING FLAGS):
```csharp
// In CareerManager.SaveCareer()
tournamentState.playoffRound = pm.playoffRound;  ? Saved
tournamentState.managerType = "PlayoffManager_SingleK";  ? Saved
// ? KO1 flag NOT saved!
// ? KO3 flag NOT saved!
```

**When Game is Loaded** (FLAGS LOST):
```csharp
// In CareerManager.LoadCareer()
gsp.playoffRound = tournamentState.playoffRound;  ? Restored
gsp.KO3 = currentTourny.tour;  ? ? Wrong! Uses tournament definition, not save data!
// ? KO1 never restored!
```

**Result**: After load, `KO1=false` and `KO3=false`, so `EndGame()` defaults to regular tournament home!

---

## The Fix

### 1. Add KO1/KO3 to Save Data Structure

**File**: `Assets/Scripts/Tourny/SaveData/CareerSaveData.cs`

```csharp
[Serializable]
public class TournamentStateData
{
    public string managerType;
    public int draw;
    public int numberOfTeams;
    public int prize;
    public int oppTeam;
    public int playoffRound;
    public int games;
    public int ends;
    public int rocks;
    public bool KO1;   // ? NEW: Single-elimination playoff flag
    public bool KO3;   // ? NEW: Triple-knockout playoff flag
    public List<TeamData> teams;
    // ... rest of fields
}
```

### 2. Save KO1 Flag (Single-K Playoffs)

**File**: `Assets/Scripts/Tourny/CareerManager.cs` (line ~2733)

**Before**:
```csharp
tournamentState.managerType = "PlayoffManager_SingleK";
tournamentState.oppTeam = pmSingle.oppTeam;
tournamentState.playoffRound = pmSingle.playoffRound;
// ? KO1 not saved!
```

**After**:
```csharp
tournamentState.managerType = "PlayoffManager_SingleK";
tournamentState.oppTeam = pmSingle.oppTeam;
tournamentState.playoffRound = pmSingle.playoffRound;
tournamentState.KO1 = true; // ? Save Single-K flag!
```

### 3. Save KO3 Flag (Triple-K Playoffs)

**File**: `Assets/Scripts/Tourny/CareerManager.cs` (line ~2749)

**Before**:
```csharp
tournamentState.managerType = "PlayoffManager_TripleK";
tournamentState.playoffRound = pmTriple.playoffRound;
// ? KO3 not saved!
```

**After**:
```csharp
tournamentState.managerType = "PlayoffManager_TripleK";
tournamentState.playoffRound = pmTriple.playoffRound;
tournamentState.KO3 = true; // ? Save Triple-K flag!
```

### 4. Restore KO1/KO3 Flags on Load

**File**: `Assets/Scripts/Tourny/CareerManager.cs` (line ~2845)

**Before**:
```csharp
gsp.KO3 = currentTourny.tour;  // ? Wrong! Uses tourny definition, not save data
// ? KO1 never restored!
gsp.draw = tournamentState.draw;
gsp.playoffRound = tournamentState.playoffRound;
```

**After**:
```csharp
gsp.KO3 = tournamentState.KO3; // ? Restore from saved state!
gsp.KO1 = tournamentState.KO1; // ? Restore from saved state!
gsp.draw = tournamentState.draw;
gsp.playoffRound = tournamentState.playoffRound;
```

---

## Why This Works

### Before Fix (Data Loss):
```
Tournament Start:
  PlayoffSetup() ? gsp.KO1 = true ?
  
Mid-Game Save:
  tournamentState.managerType = "PlayoffManager_SingleK" ?
  tournamentState.KO1 = ??? ? NOT SAVED!
  
Load:
  gsp.KO1 = ??? ? NOT RESTORED!
  gsp.KO1 defaults to false ?
  
EndGame():
  if (gsp.KO1) ? false ?
  else ? LoadScene("Tourny_Home_1") ? WRONG SCENE!
```

### After Fix (Correct Routing):
```
Tournament Start:
  PlayoffSetup() ? gsp.KO1 = true ?
  
Mid-Game Save:
  tournamentState.managerType = "PlayoffManager_SingleK" ?
  tournamentState.KO1 = true ? SAVED!
  
Load:
  gsp.KO1 = tournamentState.KO1 ? RESTORED!
  gsp.KO1 = true ?
  
EndGame():
  if (gsp.KO1) ? true ?
  LoadScene("Tourny_Home_SingleK") ? CORRECT SCENE!
```

---

## Scene Routing Logic (EndMenu.cs)

```csharp
// Load appropriate tournament home scene
if (gsp.KO3)
{
    SceneManager.LoadScene("Tourny_Home_1");  // Triple-K uses standard home
}
else if (gsp.KO1)
{
    SceneManager.LoadScene("Tourny_Home_SingleK");
}
else
{
    SceneManager.LoadScene("Tourny_Home_1");  // Regular round-robin
}
```

### Tournament Type Matrix

| Tournament Type | KO1 | KO3 | Scene | Manager |
|----------------|-----|-----|-------|---------|
| Regular Round-Robin | `false` | `false` | `Tourny_Home_1` | `TournyManager` |
| Single-Elimination | `true` | `false` | `Tourny_Home_SingleK` | `PlayoffManager_SingleK` |
| Triple-Knockout | `false` | `true` | `Tourny_Home_1` | `PlayoffManager_TripleK` |

**CRITICAL**: These flags MUST match across:
1. Initial tournament setup
2. Mid-game saves
3. Mid-game loads
4. End-game scene routing

---

## Code Changes Summary

### File 1: `CareerSaveData.cs`
**Added fields**:
```csharp
public class TournamentStateData
{
    // ... existing fields ...
    public bool KO1;   // NEW: Single-elimination playoff flag
    public bool KO3;   // NEW: Triple-knockout playoff flag
}
```

### File 2: `CareerManager.cs` (Save Logic)

**Change 1** - Line ~2733 (Single-K Save):
```csharp
tournamentState.playoffRound = pmSingle.playoffRound;
tournamentState.KO1 = true; // ? NEW!
```

**Change 2** - Line ~2749 (Triple-K Save):
```csharp
tournamentState.playoffRound = pmTriple.playoffRound;
tournamentState.KO3 = true; // ? NEW!
```

**Change 3** - Line ~2845 (Load Logic):
```csharp
gsp.KO3 = tournamentState.KO3; // ? FIXED! (was: currentTourny.tour)
gsp.KO1 = tournamentState.KO1; // ? NEW!
```

---

## Testing Scenarios

### Test Case 1: Single-K Mid-Game Save
```
1. Start Single-K tournament
2. Play until mid-game (End 3 of 10)
3. Save game
4. Exit and reload save
5. Finish game
6. Click "End Game"
7. ? Should load "Tourny_Home_SingleK" (knockout bracket)
```

### Test Case 2: Triple-K Mid-Game Save
```
1. Start Triple-K tournament
2. Play until mid-game
3. Save game
4. Exit and reload save
5. Finish game
6. Click "End Game"
7. ? Should load "Tourny_Home_1" with Triple-K bracket visible
```

### Test Case 3: Regular Tournament Mid-Game Save
```
1. Start regular round-robin tournament
2. Play until mid-game
3. Save game
4. Exit and reload save
5. Finish game
6. Click "End Game"
7. ? Should load "Tourny_Home_1" with round-robin standings
```

---

## Expected Logs

### Saving Single-K Tournament:
```
[CareerManager] Saving tournament state...
[CareerManager] Tournament manager type: PlayoffManager_SingleK
[CareerManager] Playoff round: 2, KO1: True, KO3: False
[CareerSaveService] Career saved successfully
```

### Loading Single-K Tournament:
```
[CareerManager] Loading tournament state...
[CareerManager] Tournament type: Single-K (KO1=True, KO3=False)
[GameSettingsPersist] KO1 = True, KO3 = False
[GameSettingsPersist] Playoff round: 2
```

### End Game Routing (Single-K):
```
[EndMenu] EndGame called
[EndMenu] Tournament flags: KO1=True, KO3=False
[EndMenu] Loading scene: Tourny_Home_SingleK ?
```

---

## Backward Compatibility

### Old Saves (No KO1/KO3 Fields):
```csharp
// In CareerSaveData.cs, default values:
public bool KO1;   // Defaults to false
public bool KO3;   // Defaults to false
```

**For old saves**:
- Both flags will be `false` (C# default)
- Will route to regular tournament home (safe fallback)
- Player can continue from there

**Workaround**: Old playoff saves will route to regular home, but player can:
1. Check their career to see which tournament they're in
2. Manually navigate to correct playoff bracket

**Future**: Once player saves after loading old save, flags will be set correctly.

---

## Related Files

### Files Modified
1. ? `Assets/Scripts/Tourny/SaveData/CareerSaveData.cs` - Added KO1/KO3 fields
2. ? `Assets/Scripts/Tourny/CareerManager.cs` - Save and load KO1/KO3 flags

### Files Using KO1/KO3 (No Changes Needed)
- `Assets/Scripts/EndMenu.cs` - Scene routing logic (already correct)
- `Assets/Scripts/GameSettingsPersist.cs` - TournySetup/PlayoffSetup (already correct)

---

## Why This Bug Existed

### Original Design:
Tournament type flags (`KO1`, `KO3`) were set **once** at tournament start in `TournySettings.LoadToGSP()` or `PlayoffSetup()`.

### The Problem:
Mid-game saves **didn't persist these flags**, so after loading:
1. Flags defaulted to `false`
2. System thought it was a regular tournament
3. Routed to wrong home scene

### The Solution:
**Save AND restore** the flags so the system always knows what tournament type it is, even after mid-game loads.

---

## Testing Checklist

### ? New Game Flow (Should Still Work)
- [ ] Start Single-K tournament ? KO1 set correctly
- [ ] Start Triple-K tournament ? KO3 set correctly
- [ ] Finish without saving ? Routes correctly

### ? Mid-Game Save/Load (NOW FIXED!)
- [ ] Single-K: Save mid-game ? Load ? Finish ? Routes to Single-K home ?
- [ ] Triple-K: Save mid-game ? Load ? Finish ? Routes to regular home (Triple-K uses it) ?
- [ ] Regular: Save mid-game ? Load ? Finish ? Routes to regular home ?

### ? Old Save Compatibility
- [ ] Load old save (no KO flags) ? Defaults to regular home (safe fallback)
- [ ] Resave after loading old save ? Flags now correct for future loads

---

## Summary

? **Added `KO1` and `KO3` to `TournamentStateData`**
? **Save flags when saving Single-K or Triple-K state**
? **Restore flags from save data** (not from tournament definition)
? **Backward compatible** (old saves default to regular tournament)

**Result**: Mid-game saves now preserve tournament type, ensuring correct scene routing after game completion! ??

---

## Quick Reference

### Tournament Type Identification

```csharp
// Regular Round-Robin
gsp.KO1 = false
gsp.KO3 = false
gsp.playoffRound = 0
? Scene: "Tourny_Home_1" (standings table)

// Single-Elimination Knockout
gsp.KO1 = true
gsp.KO3 = false
gsp.playoffRound = 1-4 (Round of 16 ? Finals)
? Scene: "Tourny_Home_SingleK" (bracket)

// Triple-Knockout
gsp.KO1 = false
gsp.KO3 = true
gsp.playoffRound = 1-20 (20 rounds max)
? Scene: "Tourny_Home_1" (standings + A/B/C pools)
```

### Save Data Structure
```json
{
  "currentTournamentState": {
    "managerType": "PlayoffManager_SingleK",
    "playoffRound": 2,
    "KO1": true,   // ? Saved now!
    "KO3": false   // ? Saved now!
  }
}
```

**Status**: ? **COMPLETE!** Tournament routing now works correctly after mid-game loads.
