# Tournament Completion Auto-Save Overwrite Fix

## Problem Identified

From your console logs, the issue was crystal clear:

### What Was Happening

1. **During Tournament** (saving correctly):
   ```
   ? Saved completed: 'The Fall Rookie Invitational' (ID 0)
   ? Saved from CM arrays: 1 completed IDs, 0 trophy IDs
   ```

2. **After Loading Career** (data lost):
   ```
   ? TournySelector is NULL in LoadCareerJSON - cannot restore tournament completion!
   ? Auto-save triggers 30 seconds later
   ? Saved from CM arrays: 0 completed IDs  ? OVERWRITES with empty data!
   ```

### Root Cause

**The completion data was being OVERWRITTEN by the auto-save system!**

Here's the sequence of events:

1. Tournament completes ? Completion saved to CM arrays ? JSON file updated ?
2. Player quits to main menu
3. CareerSettings.Start() loads career ? TournySelector doesn't exist yet
4. LoadCareer() can't restore completion (no TournySelector)
5. CM arrays are EMPTY (not restored)
6. **30 seconds later**: Auto-save triggers ? Saves EMPTY CM arrays ? Overwrites good data! ?

The completion data existed in the JSON file, but was immediately overwritten before TournySelector could sync it!

## The Fix

Added **two-phase restoration**:

### Phase 1: Immediate Restoration to CM Arrays

When TournySelector doesn't exist (loading from CareerSettings), restore completion data to CM arrays immediately:

```csharp
if (tSel != null)
{
    // Restore to TournySelector arrays (normal path)
    ApplyTournamentData(tSel, saveData);
}
else
{
    // NEW: Restore to CM arrays as temporary storage
    // This prevents auto-save from overwriting with empty data
    RestoreCompletionToCMArrays(saveData);
}
```

### Phase 2: Sync to TournySelector

When TournySelector loads (in TournySelector.SetUp()), it syncs from CM arrays:

```csharp
// Existing code in TournySelector.SetUp()
cm.LoadCareer(tSel: this);
SyncCompletionFromCareerManager();  // Syncs from CM arrays
SetActiveTournies();  // Uses synced completion
```

## What Changed

### CareerManager.LoadCareerJSON()

**Before**:
```csharp
if (tSel != null)
{
    ApplyTournamentData(tSel, saveData);
}
else
{
    Debug.LogWarning("Cannot restore tournament completion!");
    // CM arrays remain empty ? auto-save overwrites JSON!
}
```

**After**:
```csharp
if (tSel != null)
{
    ApplyTournamentData(tSel, saveData);
}
else
{
    // NEW: Restore to CM arrays to prevent auto-save overwrite
    RestoreCompletionToCMArrays(saveData);
}
```

### New Method: RestoreCompletionToCMArrays()

Restores completion data from save file to CM arrays (tournies, tour, prov, champ):

```csharp
private void RestoreCompletionToCMArrays(CareerSaveData saveData)
{
    // Restore to CM.tournies
    foreach (var tourny in tournies)
    {
        if (saveData.completedTournamentIDs.Contains(tourny.id))
        {
            tourny.complete = true;
        }
    }
    
    // Same for tour, prov, champ arrays
    // ...
}
```

This ensures:
- CM arrays have the correct completion data
- Auto-save won't overwrite with empty data
- TournySelector can sync from CM arrays when it loads

## Testing

Run your same test case:

1. **Start career** (Week 1)
2. **Complete tournament**
3. **Check logs**:
   ```
   ? Saved completed: 'The Fall Rookie Invitational' (ID 0)
   ? Saved from CM arrays: 1 completed IDs
   ```
4. **Quit to main menu**
5. **Load career**
6. **Check logs** - You should now see:
   ```
   [CareerManager] TournySelector is NULL - will restore to CM arrays
   [CareerManager] RestoreCompletionToCMArrays - Restoring to CM arrays
   ? CM.tournies: Marked 'The Fall Rookie Invitational' (ID 0) as complete
   ? Restored 1 completions to CM arrays
   ```
7. **Wait 30 seconds** (auto-save)
8. **Check logs** - Should now save correctly:
   ```
   ? Saved from CM arrays: 1 completed IDs  ? NOT empty anymore!
   ```

## Expected Console Output

### On Load (Before TournySelector Exists)

```
[CareerManager] Loading career save from 2026-02-23 17:13:24
[CareerManager] TournySelector is NULL - will restore to CM arrays
[CareerManager] RestoreCompletionToCMArrays - Restoring to CM arrays as temporary storage
  Completed IDs: 1, Trophy IDs: 0
  ? CM.tournies: Marked 'The Fall Rookie Invitational' (ID 0) as complete
[CareerManager] ? Restored 1 completions to CM arrays (will sync to TournySelector when it loads)
```

### On Auto-Save (30 Seconds Later)

```
[CareerManager] TournySelector not found - saving from CM arrays (in tournament)
  ? Saved completed: 'The Fall Rookie Invitational' (ID 0)
[CareerManager] ? Saved from CM arrays: 1 completed IDs, 0 trophy IDs
[SAVE DEBUG] Completed IDs saved (from CM): [0]
```

### When TournySelector Loads

```
[TournySelector] SetUp() called
[TournySelector] Existing career (week > 0) - calling LoadCareer()
[CareerManager] Applying tournament completion data to TournySelector
  ? Marked tournament 'The Fall Rookie Invitational' (ID 0) as complete
[TournySelector] ? Completion sync complete
```

## Why This Works

The fix creates a **chain of custody** for completion data:

1. **Tournament Scene**: Completion marked in TournySelector ? Synced to CM ? Saved to JSON
2. **Load (No TournySelector)**: JSON ? Restored to CM arrays (NEW!)
3. **Auto-Save**: CM arrays ? JSON (now has data, not empty!)
4. **TournySelector Loads**: CM arrays ? TournySelector arrays ? Display in UI

The completion data is **never lost** because:
- It's always in at least one place (JSON, CM arrays, or TournySelector)
- Auto-save preserves whatever CM arrays have (which now have the restored data)
- TournySelector syncs from CM arrays when it exists

## Files Modified

**CareerManager.cs**:
1. Modified `LoadCareerJSON()` to call `RestoreCompletionToCMArrays()` when TournySelector is null
2. Added new method `RestoreCompletionToCMArrays()` to restore completion to CM arrays

## Summary

**Before**: Auto-save would overwrite completion data with empty arrays because CM arrays weren't restored when loading without TournySelector.

**After**: Completion data is immediately restored to CM arrays on load, preventing auto-save from overwriting with empty data.

The completion data is now **safe** through the entire load/save cycle:
- ? Saved correctly during tournament
- ? Restored to CM arrays on load (even without TournySelector)
- ? Auto-save preserves CM arrays (no longer empty)
- ? TournySelector syncs from CM arrays when it loads
- ? Completion shows correctly in UI

This was a **timing bug** - the data was being saved and loaded correctly, but the auto-save system was overwriting it before TournySelector could sync it. The fix ensures completion data is preserved in CM arrays immediately on load, preventing the auto-save overwrite.
