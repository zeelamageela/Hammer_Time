# Tournament Completion Status Timing Fix

## The Real Problem

The save/load system was **already saving tournament completion data correctly**. The bug was a **timing issue** in the load sequence.

### What Was Happening

```
User saves at Week 4 with "Jim Jumbos Memorial Spiel" offered
?
Save file contains:
  - activeTournamentIDs: [5, 12, 8]  (IDs of tournaments offered this week)
  - regularTournaments:
      - { id: 1, complete: true }    (Fall Rookie Invitational - already played)
      - { id: 5, complete: false }   (Jim Jumbos Memorial Spiel - offered)
      - { id: 12, complete: false }
?
User loads save
?
WRONG SEQUENCE:
  1. LoadFromSaveData() ? loads activeTournamentIDs
  2. SetActiveTournies() ? checks tournies[i].complete (still FALSE - not restored yet!)
  3. ApplyTournamentData() ? sets tournies[i].complete = true (too late!)
?
Result: Fall Rookie Invitational shown again (WRONG!)
```

### The Fix

Reorder the load sequence so completion status is restored **before** tournament generation:

```csharp
// BEFORE (WRONG ORDER)
LoadFromSaveData(saveData);           // Step 1
// ... later ...
ApplyTournamentData(tSel, saveData);  // Step 2 (too late!)

// AFTER (CORRECT ORDER)
ApplyTournamentData(tSel, saveData);  // Step 1 (restore completion status first!)
LoadFromSaveData(saveData);           // Step 2
```

### Code Changes

**CareerManager.cs - LoadCareerJSON()**
```csharp
// CRITICAL FIX: Apply tournament data to TournySelector FIRST
// This ensures tournament completion status is restored before SetActiveTournies() is called
if (tSel != null)
{
    ApplyTournamentData(tSel, saveData);  // ? NOW HAPPENS FIRST
}

// Apply save data to CareerManager
LoadFromSaveData(saveData);
```

**TournySelector.cs - SetUp()**
```csharp
// CRITICAL FIX: Pass 'this' to LoadCareer so it can set tournament completion status
// BEFORE SetActiveTournies() is called
cm.LoadCareer(tSel: this);  // ? Pass reference so CareerManager can apply data early
```

## Why This Fixes the Bug

### Before the Fix

1. `TournySelector.SetUp()` calls `cm.LoadCareer()`
2. `LoadCareer()` loads save data into `CareerManager`
3. `SetUp()` calls `SetActiveTournies()`
   - Checks `tournies[i].complete` ? **Still FALSE** (not restored yet)
   - Finds Fall Rookie Invitational as first non-complete
   - Shows Fall Rookie Invitational (WRONG)
4. `LoadCareer()` calls `ApplyTournamentData()`
   - Sets `tournies[1].complete = true` (too late!)

### After the Fix

1. `TournySelector.SetUp()` calls `cm.LoadCareer(tSel: this)`
2. `LoadCareer()` calls `ApplyTournamentData(tSel, saveData)` **FIRST**
   - Sets `tournies[1].complete = true` (Fall Rookie Invitational)
   - Sets `tournies[5].complete = false` (Jim Jumbos Memorial)
3. `LoadCareer()` loads save data into `CareerManager`
4. `SetUp()` calls `SetActiveTournies()`
   - Checks `tournies[i].complete` ? **Now correct!**
   - Skips Fall Rookie Invitational (complete = true)
   - Shows Jim Jumbos Memorial (CORRECT)

## Result

? Tournament completion status is restored before tournament generation
? Already-played tournaments don't appear again
? Correct tournaments shown for the current week
? No data was missing - just needed correct timing

## Technical Details

### The Save Data Was Always Correct

```json
{
  "activeTournamentIDs": [5, 12, 8],
  "regularTournaments": [
    { "id": 1, "complete": true },    // ? This data WAS in the save file
    { "id": 5, "complete": false },
    { "id": 12, "complete": false }
  ]
}
```

The problem wasn't missing data - it was applying the data in the wrong order.

### What `ApplyTournamentData()` Does

```csharp
private void ApplyTournamentData(TournySelector tSel, CareerSaveData saveData)
{
    // Apply regular tournaments
    if (saveData.regularTournaments != null && tSel.tournies != null)
    {
        for (int i = 0; i < saveData.regularTournaments.Count; i++)
        {
            var data = saveData.regularTournaments[i];
            for (int j = 0; j < tSel.tournies.Length; j++)
            {
                if (tSel.tournies[j] != null && tSel.tournies[j].id == data.id)
                {
                    tSel.tournies[j].complete = data.complete;  // ? Sets completion status
                    tSel.tournies[j].trophyWon = data.trophyWon;
                    break;
                }
            }
        }
    }
    // ... (same for tour, provQual, championships)
}
```

This function was always being called - just too late in the sequence.

## Lesson Learned

**Order matters in initialization sequences!**

When loading save data:
1. Restore static data (completion status, IDs, etc.)
2. THEN run logic that depends on that data (tournament generation)

Don't run dependent logic before its dependencies are restored.
