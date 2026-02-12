# Active Tournaments Save/Load Fix

## Problem

When loading a saved career from Week 4 (or any week), the **tournament selection screen would show different tournaments** than were originally offered that week. Tournaments that had already been played in earlier weeks would re-appear.

**Example:**
- **Week 2:** Offered Tournament A, Tournament B, Tournament C
- Player plays Tournament A
- **Week 3:** Save game
- **Week 4:** Offered Tournament D, Tournament E, Tournament F
- Player saves and quits
- **Load save from Week 4:** Shows Tournament B, Tournament C, Tournament G (WRONG! Should be D, E, F)

## Root Cause

The save system had a **timing issue**:
- ? Save file stored which tournaments are **completed** (`tournies[i].complete`)
- ? Save file stored which tournaments are **offered this week** (`activeTournamentIDs`)
- ? BUT: Tournament completion data was being restored **AFTER** `SetActiveTournies()` ran

**The sequence was wrong:**
1. `TournySelector.SetUp()` called `cm.LoadCareer()`
2. `LoadCareer()` loaded save data into CareerManager
3. `SetUp()` called `SetActiveTournies()` ? **Regenerated tournaments from incomplete list**
4. `LoadCareer()` applied tournament completion status ? **Too late!**

**Result:** `SetActiveTournies()` would check `tournies[i].complete` BEFORE the completion status was restored from the save file, so it thought all tournaments were incomplete and showed the wrong ones.

This was happening in `SetActiveTournies()`:

```csharp
// OLD CODE - Finds FIRST non-complete tournaments
for (int i = 0; i < tournies.Length; i++)
{
    if (!tournies[i].complete)
    {
        if (tCount == 1)
        {
            nextTourny2 = i;  // ? Finds FIRST available
            // ...
        }
    }
}
```

## The Solution

**Fixed the timing:** Apply tournament completion status to `TournySelector` **BEFORE** `SetActiveTournies()` runs.

### Changes Made

#### 1. Reordered load sequence in `CareerManager.LoadCareerJSON()`

**OLD CODE - Wrong order:**
```csharp
// Apply save data to CareerManager
LoadFromSaveData(saveData);

// Apply tournament data to TournySelector
if (tSel != null)
{
    ApplyTournamentData(tSel, saveData);  // ? Too late! Already ran SetActiveTournies()
}
```

**NEW CODE - Correct order:**
```csharp
// CRITICAL FIX: Apply tournament data to TournySelector FIRST
// This ensures tournament completion status is restored before SetActiveTournies() is called
if (tSel != null)
{
    ApplyTournamentData(tSel, saveData);  // ? Happens BEFORE SetActiveTournies()
}

// Apply save data to CareerManager
LoadFromSaveData(saveData);
```

#### 2. Pass `TournySelector` reference in `TournySelector.SetUp()`

**OLD CODE:**
```csharp
cm.LoadCareer();  // ? Doesn't know about TournySelector, can't set completion status early
```

**NEW CODE:**
```csharp
// CRITICAL FIX: Pass 'this' to LoadCareer so it can set tournament completion status
// BEFORE SetActiveTournies() is called
cm.LoadCareer(tSel: this);  // ? Now LoadCareer can restore completion status first!
```

## How It Works Now

### Correct Load Sequence

```
1. Player loads save from Week 4
        ?
2. TournySelector.SetUp() calls cm.LoadCareer(tSel: this)
        ?
3. CareerManager.LoadCareerJSON()
        |
        ??? ApplyTournamentData(tSel, saveData)  ? HAPPENS FIRST
        |     |
        |     ??? Restores tournies[i].complete = true/false
        |     ??? Restores tour[i].complete = true/false
        |     ??? Restores provQual[i].complete = true/false
        |
        ??? LoadFromSaveData(saveData)
              |
              ??? Restores activeTournamentIDs
        ?
4. TournySelector.SetActiveTournies()
        |
        ??? Checks if cm.activeTournies already loaded (from save)
        |     ??? YES: Uses saved tournaments (skip regeneration)
        |     ??? NO: Regenerates from tournies[i].complete status
        |              (but completion status is now correct!)
        ?
5. SetPanels() displays correct tournaments
```

### Tournament Completion Status Restoration

The save file already contained tournament completion data:
```json
{
  "regularTournaments": [
    { "id": 1, "complete": true },   // Fall Rookie Invitational (already played)
    { "id": 5, "complete": false },  // Available tournament
    { "id": 12, "complete": false }  // Jim Jumbos Memorial Spiel
  ],
  "activeTournamentIDs": [5, 12, 8]  // Tournaments offered this week
}
```

**Before the fix:**
- `SetActiveTournies()` ran before completion status was restored
- It saw `tournies[1].complete = false` (default value)
- Showed Fall Rookie Invitational again (WRONG)

**After the fix:**
- `ApplyTournamentData()` runs first
- `tournies[1].complete = true` is restored from save
- `SetActiveTournies()` skips Fall Rookie Invitational
- Shows Jim Jumbos Memorial Spiel (CORRECT)

## Files Changed

1. **`Assets/Scripts/Tourny/CareerManager.cs`**
   - Reordered `LoadCareerJSON()` to call `ApplyTournamentData()` BEFORE `LoadFromSaveData()`
   - This ensures tournament completion status is restored before `SetActiveTournies()` runs

2. **`Assets/Scripts/Tourny/TournySelector.cs`**
   - Updated `SetUp()` to pass `this` reference when calling `cm.LoadCareer(tSel: this)`
   - This allows CareerManager to apply tournament data to TournySelector before SetActiveTournies runs

## Root Issue

The save system **already had all the data it needed** - tournament completion status was being saved correctly. The problem was a **timing/ordering bug**:

- Tournament completion status (`tournies[i].complete`) was being restored too late
- `SetActiveTournies()` ran before the completion data was applied
- It saw default `complete = false` values and showed already-played tournaments

## The Fix in Simple Terms

**Before:** 
1. Load save ? 2. Generate tournaments ? 3. Apply completion status ?

**After:**
1. Load save ? 2. Apply completion status ? 3. Generate tournaments ?

The fix was literally just reordering two function calls in `LoadCareerJSON()`.

## Testing

### Test 1: Save and Load Same Week
1. Start new career
2. Week 1: Note which 3 tournaments are offered (e.g., A, B, C)
3. Save and quit
4. Load save
5. ? **Expected:** Same 3 tournaments (A, B, C) are still offered

### Test 2: Play Tournament, Save, Load
1. Start new career
2. Week 2: Note tournaments (e.g., D, E, F)
3. Play tournament D
4. Complete tournament
5. Week 3: Note new tournaments (e.g., G, H, I)
6. Save and quit
7. Load save
8. ? **Expected:** Week 3 shows G, H, I (not D, E, F)

### Test 3: Multi-Week Progression
1. Start new career
2. Play Week 1, Week 2, Week 3 tournaments
3. Save at Week 4
4. Note which tournaments are offered in Week 4
5. Quit and load save
6. ? **Expected:** Same tournaments from Week 4 are offered

### Test 4: Empty Tournament Slots
1. Week 6+ (when some tournaments might be empty)
2. Save with 1-2 empty tournament slots
3. Load save
4. ? **Expected:** Empty slots remain empty (not filled with other tournaments)

## Summary

**The fix:** Reordered the load sequence so tournament completion status is applied BEFORE `SetActiveTournies()` runs.

**Before:**
- Save stored tournament completion status correctly ?
- Load restored completion status AFTER generating active tournaments ?
- Result: Already-played tournaments appeared again

**After:**
- Save stores tournament completion status correctly ?
- Load restores completion status BEFORE generating active tournaments ?
- Result: Consistent tournament selection across save/load

**Technical Change:**
- Moved `ApplyTournamentData()` call to happen before `LoadFromSaveData()` in `LoadCareerJSON()`
- Updated `TournySelector.SetUp()` to pass `this` reference to `LoadCareer()`

**Result:**
? Tournaments offered stay consistent when loading a save
? No more showing already-completed tournaments
? Players see the correct tournaments for their current week
? Tournament progression is now deterministic and predictable

**Note:** The save file already contained all the necessary data (tournament completion status). This was purely a timing/ordering bug in the load sequence.
