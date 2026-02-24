# Tournament Completion Save Preservation Fix

## Problem Identified

Tournament completions were being **LOST** when loading a save:

### Before Fix:
```
1. Quit game ? Save: [50, 0, 1] ? (has completions)
2. Load game ? TournyManager.SetDraw() calls SaveCareer()
3. Save: [] ? (EMPTY - overwrites good save!)
```

### Root Cause:
- `TournyManager.PrintRows()` (line 462) calls `cm.SaveCareer()` during tournament setup
- At this point, **TournySelector doesn't exist** (we're in tournament scene)
- `ToSaveData()` falls back to CM arrays which are **empty** during tournaments
- **Pending completion data was ignored** during tournament saves
- Good save gets overwritten with empty data

## The Fix

Modified `ToSaveData()` to **preserve pending completion data** when TournySelector doesn't exist:

### New Logic Flow:

```csharp
if (TournySelector exists)
{
    // Read fresh data from TournySelector ?
}
else
{
    // NO TournySelector (in tournament)
    
    if (pendingCompletionData exists)
    {
        // ? PRESERVE pending data from previous save
        data.completedIDs = pendingCompletedIDs
        data.trophyIDs = pendingTrophyIDs
    }
    
    // ? ALSO check CM arrays for NEW completions (from TournyResults)
    // MERGE new completions with pending data
    
    // Result: NEVER lose data!
}
```

### Key Changes:

1. **Check pending data FIRST** when TournySelector is null
2. **Merge CM arrays** to catch any NEW completions (from `TournyResults()`)
3. **Never overwrite** with empty data

## Code Changes

### File: `Assets/Scripts/Tourny/CareerManager.cs`

**Method**: `ToSaveData()` (around line 2091)

**Before**:
```csharp
else
{
    // FALLBACK: Save from CM arrays (in tournament)
    // Collects from prov, tour, tournies, champ arrays
    // ? Problem: These are EMPTY during tournaments!
}
```

**After**:
```csharp
else
{
    // Check PENDING data first (from previous save)
    if (pendingCompletedTournamentIDs.Count > 0 || ...)
    {
        // ? Use pending data
        data.completedTournamentIDs.AddRange(pendingCompletedTournamentIDs);
        data.trophyWonIDs.AddRange(pendingTrophyWonIDs);
    }
    
    // ALSO check CM arrays for NEW completions
    // MERGE without duplicates using Contains()
    if (prov != null)
    {
        foreach (var tourny in prov)
        {
            if (tourny.complete && !data.completedTournamentIDs.Contains(tourny.id))
            {
                data.completedTournamentIDs.Add(tourny.id); // NEW completion!
            }
        }
    }
    // ... (same for tour, tournies, champ)
}
```

## Expected Behavior After Fix

### Scenario 1: Normal Quit/Load
```
1. Quit ? Save: [50, 0, 1]
2. Load ? StorePendingCompletionData([50, 0, 1])
3. Tournament calls SaveCareer() ? Uses pending data ?
4. Save: [50, 0, 1] (PRESERVED!)
```

### Scenario 2: Complete Tournament During Session
```
1. Load ? Pending: [50, 0, 1]
2. Play tournament #2
3. TournyResults() ? Marks CM.tournies[2].complete = true
4. Tournament calls SaveCareer() ? Merges pending + CM arrays
5. Save: [50, 0, 1, 2] (OLD + NEW!)
```

### Scenario 3: Normal Save from TournySelector
```
1. In TournySelector scene
2. SaveCareer() ? Reads from TournySelector (fresh data)
3. Save: [current completions] (always accurate)
```

## Testing

### Expected Logs on Load:
```
[CareerManager] StorePendingCompletionData - Preserving completion IDs
  Stored 3 completed IDs: [50, 0, 1]
  Stored 0 trophy IDs: []
```

### Expected Logs on Mid-Tournament Save:
```
[CareerManager] TournySelector not found - checking pending data vs CM arrays
[CareerManager] ? Using PENDING data from previous save
  Pending completed: 3 IDs: [50, 0, 1]
  Pending trophies: 0 IDs: []
[CareerManager] ? Preserved pending data: 3 completed, 0 trophies
[SAVE DEBUG] Completed IDs saved (MERGED): [50, 0, 1]
```

### Expected Logs After New Completion:
```
[CareerManager] TournyResults - Marking tournament 'Spring Ring' (ID 9) as complete
  ? Marked tournies[9] 'Spring Ring' as complete
... (later during tournament save) ...
[CareerManager] ? MERGED 1 new completions from CM arrays: [9]
[SAVE DEBUG] Completed IDs saved (MERGED): [50, 0, 1, 9]
```

## Why This Works

1. **Pending data acts as a cache** - preserves completions during tournament
2. **CM arrays add incremental updates** - captures TournyResults() changes
3. **Merge strategy prevents duplicates** - `Contains()` check before adding
4. **Never loses data** - always has fallback to pending data

## Additional Protection

The fix in `TournyResults()` (earlier commit) ensures:
- Completions are marked in CM arrays IMMEDIATELY
- Even if save fails, next save will catch it

Combined with this preservation fix:
- **Completions are bulletproof** - saved from multiple sources
- **Mid-tournament saves don't overwrite** - pending data preserved
- **New completions always captured** - merged from CM arrays

## Verification

After applying this fix:

1. Complete a tournament
2. Quit the game
3. Check logs: `Completed IDs saved: [...]` (should have data)
4. Reload game
5. Check logs: `Stored N completed IDs: [...]` (should match)
6. Load tournament (TournyManager scene)
7. Check logs: `Completed IDs saved (MERGED): [...]` (should STILL have data!)

**Before**: Step 7 would show `[]` (empty)
**After**: Step 7 shows preserved data ?

---

## Related Files
- `Assets/Scripts/Tourny/CareerManager.cs` - Main fix
- `Assets/Scripts/Tourny/TournyManager.cs` - Calls SaveCareer() (triggers the issue)
- `Assets/Scripts/Tourny/TournySelector.cs` - Reads completion data on load

## Next Steps

1. **Test the fix** - Complete tournament, quit, reload, verify data preserved
2. **Monitor logs** - Look for "MERGED" and "Preserved pending data" messages
3. **Report results** - Confirm tournaments stay completed after reload

