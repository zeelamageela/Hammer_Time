# Tournament Completion Save/Load Debug Guide

## Problem Summary
Tournament completion status is not being properly saved and restored. Tournaments that were completed are showing as incomplete when loading the career.

## Root Cause Analysis

### The Save/Load Flow

1. **Tournament Completion (TournySelector.PlayTourny())**:
   ```
   User selects tournament ? PlayTourny() called
   ?
   Marks tournament complete in TournySelector arrays (tournies[], tour[], provQual[])
   ?
   Syncs to CareerManager arrays (cm.tournies, cm.tour, cm.prov)
   ?
   Calls cm.SaveCareer()
   ?
   Saves completion IDs to JSON
   ```

2. **After Tournament (Return to TournySelector)**:
   ```
   TournySelector scene loads (FRESH ScriptableObject arrays!)
   ?
   TournySelector.SetUp() called
   ?
   cm.LoadCareer(tSel: this) called
   ?
   ApplyTournamentData() restores completion from JSON IDs
   ?
   SetActiveTournies() uses restored completion status
   ```

## Current Implementation

### CareerManager.ToSaveData() (Saving)
```csharp
// Line ~4270
// Try TournySelector first (if it exists)
TournySelector tSel = FindFirstObjectByType<TournySelector>();

if (tSel != null)
{
    // Save from TournySelector arrays
    // Collects completion IDs from tSel.tournies[], tSel.tour[], tSel.provQual[]
}
else
{
    // FALLBACK: Save from CM arrays (in tournament scene)
    // Collects completion IDs from cm.tournies, cm.tour, cm.prov
}
```

### CareerManager.ApplyTournamentData() (Loading)
```csharp
// Line ~1140
// Restore from completedTournamentIDs list
foreach (var tourny in tSel.tour)
{
    if (tourny != null && saveData.completedTournamentIDs.Contains(tourny.id))
    {
        tourny.complete = true;
    }
}
// Same for provQual and tournies
```

## Debugging Steps

### 1. Check What's Being Saved
Add logging to `CareerManager.ToSaveData()` right after collecting IDs:

```csharp
// After collecting completedTournamentIDs
Debug.Log($"[SAVE DEBUG] Completed IDs being saved: {string.Join(", ", data.completedTournamentIDs)}");
Debug.Log($"[SAVE DEBUG] Trophy IDs being saved: {string.Join(", ", data.trophyWonIDs)}");

// Check which path was used
if (tSel != null)
{
    Debug.Log($"[SAVE DEBUG] Saved from TournySelector arrays");
}
else
{
    Debug.Log($"[SAVE DEBUG] Saved from CM arrays (fallback)");
}
```

### 2. Check What's Being Loaded
Add logging to `CareerManager.ApplyTournamentData()`:

```csharp
// At the start
Debug.Log($"[LOAD DEBUG] IDs to restore: completed={saveData.completedTournamentIDs.Count}, trophies={saveData.trophyWonIDs.Count}");
Debug.Log($"[LOAD DEBUG] Completed IDs: {string.Join(", ", saveData.completedTournamentIDs)}");

// For each array
Debug.Log($"[LOAD DEBUG] Checking {tSel.tour.Length} tour tournaments");
foreach (var tourny in tSel.tour)
{
    if (tourny != null)
    {
        bool shouldRestore = saveData.completedTournamentIDs.Contains(tourny.id);
        Debug.Log($"[LOAD DEBUG] Tour '{tourny.name}' (ID {tourny.id}): shouldRestore={shouldRestore}, wasComplete={tourny.complete}");
        
        if (shouldRestore)
        {
            tourny.complete = true;
            Debug.Log($"[LOAD DEBUG] ? Restored!");
        }
    }
}
```

### 3. Check JSON Save File
Manually inspect the save file to verify IDs are present:

**Location**: `Application.persistentDataPath/career_save.json`

Look for:
```json
{
  "completedTournamentIDs": [10, 15, 23],
  "trophyWonIDs": [10],
  "tourChampionshipComplete": false,
  "provChampionshipComplete": false
}
```

### 4. Check Timing Issues

The most likely issue is **timing**. Verify when each method is called:

```csharp
// In TournySelector.SetUp()
Debug.Log($"[TIMING] SetUp() called at frame {Time.frameCount}");

// In CareerManager.LoadCareer()
Debug.Log($"[TIMING] LoadCareer() called at frame {Time.frameCount}");

// In CareerManager.ApplyTournamentData()
Debug.Log($"[TIMING] ApplyTournamentData() called at frame {Time.frameCount}");

// In TournySelector.SetActiveTournies()
Debug.Log($"[TIMING] SetActiveTournies() called at frame {Time.frameCount}");
```

**Expected order**:
1. TournySelector.SetUp()
2. CareerManager.LoadCareer(tSel: this)
3. CareerManager.ApplyTournamentData() ? Completion restored HERE
4. TournySelector.SetActiveTournies() ? Uses restored completion

**If SetActiveTournies() is called BEFORE ApplyTournamentData()**, that's the bug!

## Potential Issues

### Issue 1: TournySelector is null when saving
**Symptom**: Fallback to CM arrays is used, but CM arrays are stale

**Fix**: Ensure CM arrays are synced in `TournySelector.PlayTourny()`:
```csharp
// In TournySelector.PlayTourny() (AFTER marking complete)
cm.tournies = tournies;
cm.tour = tour;
cm.prov = provQual;
cm.champ = new Tourny[2] { tourChampionship, provChampionship };
```

### Issue 2: ScriptableObject arrays reset between save and load
**Symptom**: Tournament objects are recreated, losing completion status

**Fix**: Use ID-based restoration (already implemented). Ensure IDs are stable and unique.

### Issue 3: SetActiveTournies() called before ApplyTournamentData()
**Symptom**: Completion status is restored, but too late

**Fix in TournySelector.SetUp()**:
```csharp
if (cm.week == 0)
{
    cm.NewSeason();
}
else
{
    // CRITICAL: Pass 'this' so CM can restore BEFORE SetActiveTournies()
    cm.LoadCareer(tSel: this);
    
    // CRITICAL: Sync again after load (handles tournament return)
    SyncCompletionFromCareerManager();
}

// MOVED TO LAST - Only called AFTER restoration complete
SetActiveTournies();
```

### Issue 4: Tournament IDs don't match
**Symptom**: Tournaments have different IDs in save vs. load

**Verify**: Print all tournament IDs on save and load:
```csharp
// On save
foreach (var tourny in tournies)
{
    Debug.Log($"[SAVE] Tournament '{tourny.name}' has ID {tourny.id}, complete={tourny.complete}");
}

// On load
foreach (var tourny in tournies)
{
    Debug.Log($"[LOAD] Tournament '{tourny.name}' has ID {tourny.id}, complete={tourny.complete}");
}
```

If IDs don't match, the ScriptableObjects are being recreated with new IDs!

## Testing Checklist

1. ? **Start new career**
   - Week 1, no tournaments complete
   - Save should have: `completedTournamentIDs: []`

2. ? **Complete first tournament**
   - Play week 1 tournament
   - After completion, save should have: `completedTournamentIDs: [<tournament_id>]`

3. ? **Quit and reload**
   - Exit to main menu
   - Load career
   - Check if tournament shows as complete in TournySelector

4. ? **Complete second tournament**
   - Play week 2 tournament
   - After completion, save should have: `completedTournamentIDs: [<id1>, <id2>]`

5. ? **Verify across weeks**
   - Advance multiple weeks
   - Check that all completed tournaments remain marked complete

## Quick Fix

If debugging shows that IDs are saving/loading correctly but tournaments still show as incomplete, the issue is likely in `SetActiveTournies()`. Check this loop:

```csharp
for (int i = 0; i < tournies.Length; i++)
{
    if (tournies[i].complete)
    {
        tourniesComplete = true;
    }
    else
    {
        nextTourny = i;  // ? This assumes the FIRST incomplete is next
        tourniesComplete = false;
        break;
    }
}
```

This logic looks correct. But verify that `tournies[i].complete` is TRUE for completed tournaments at this point.

## Expected Console Output (Normal Operation)

```
[TournySelector] SetUp() called at frame 123
[TournySelector] Existing career (week > 0) - calling LoadCareer()
[CareerManager] LoadCareer() called at frame 123
[CareerManager] Loading career save from 2024-01-15 14:30:00
[CareerManager] Applying tournament completion data to TournySelector BEFORE any other loading
[CareerManager] ApplyTournamentData START - Restoring from ID lists:
[CareerManager]   Completed IDs to restore: 2
[CareerManager]   Trophy IDs to restore: 1
[CareerManager]   Checking 5 tour tournaments
[CareerManager]     ? Marked tour 'Grand Slam' (ID 10) as complete
[CareerManager]   Checking 3 regular tournaments
[CareerManager]     ? Marked tournament 'Regional Cup' (ID 23) as complete
[CareerManager] ? Tournament restoration complete:
[CareerManager]   Tournaments marked complete: 2
[CareerManager]   Trophies marked won: 1
[TournySelector] ? Completion sync complete
[TournySelector] SetActiveTournies() called at frame 124
[TournySelector] tourniesComplete=false, nextTourny=3
```

## If Issue Persists

If tournaments are still not being marked complete after all this debugging:

1. **Check ScriptableObject persistence**: ScriptableObjects should be in the `Assets/` folder, not generated at runtime
2. **Check for duplicate TournySelector instances**: Only one should exist
3. **Verify JSON parsing**: Print the raw JSON to ensure it's not corrupted
4. **Check for race conditions**: Ensure SetActiveTournies() waits for LoadCareer() to finish

## Additional Logging Locations

Add these logs to track the full lifecycle:

```csharp
// In TournySelector.PlayTourny() - BEFORE marking complete
Debug.Log($"[PRE-MARK] Tournament '{currentTourny.name}' (ID {currentTourny.id}): complete={currentTourny.complete}");

// In TournySelector.PlayTourny() - AFTER marking complete
Debug.Log($"[POST-MARK] Tournament '{currentTourny.name}' (ID {currentTourny.id}): complete={currentTourny.complete}");

// In CareerManager.SaveCareer() - BEFORE ToSaveData()
Debug.Log($"[PRE-SAVE] CM.tournies[0]: complete={cm.tournies?[0]?.complete}");

// In CareerManager.SaveCareer() - AFTER ToSaveData()
Debug.Log($"[POST-SAVE] Saved {data.completedTournamentIDs.Count} completion IDs");
```

This will show exactly where the completion status is being lost.
