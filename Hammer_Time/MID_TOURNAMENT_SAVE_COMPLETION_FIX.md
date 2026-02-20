# Mid-Tournament Save Completion Fix

## Problem Description

When you:
1. Play a tournament
2. Play a draw and save mid-tournament
3. Exit and the save happens
4. Load the game and finish the tournament
5. Return to tournament selector

**Expected:** Tournament should be marked as complete and not available
**Actual:** Tournament appears as available again (not marked as complete)

## Root Cause

The issue is a **save file versioning/timing problem**:

```
Timeline:
1. Start Tournament A (A.complete = false)
2. Play Draw 1, save mid-tournament
   ? SAVE FILE #1 created: A.complete = false

3. Continue tournament
4. Finish Tournament A
   ? TournyResults() sets A.complete = true
   ? SAVE FILE #2 created: A.complete = true

5. User loads SAVE FILE #1 (the mid-tournament save)
   ? A.complete = false is restored (from older save)
   
6. Finish tournament (again)
   ? A.complete = true is set (again)
   ? BUT: User might quit without saving, OR
   ? SaveCareer() is called, but gsp.tournyInProgress = false was already set

7. Return to tournament selector
   ? A.complete = false (from loaded save, never updated!)
```

## The Fix

The issue is that when you complete a tournament after loading from a mid-tournament save, the **tournament completion status needs to be persisted to BOTH**:

1. The active `Tourny` scriptable objects in `TournySelector`
2. The save file via `CareerManager.SaveCareer()`

Currently, `TournyManager.TournyComplete()` calls:
- `cm.TournyResults()` ? (marks tournament complete)
- `cm.LoadCareer()` ? (loads OLD save data, overwriting completion!)
- `SceneManager.LoadScene("Arena_Selector")` ? (no save before switching!)

### Solution: Ensure Save Happens After Tournament Completion

The fix is to **save AFTER marking tournament complete** and **BEFORE loading/switching scenes**.

## Implementation

### Change in `TournyManager.cs` ? `TournyComplete()`

```csharp
public void TournyComplete()
{
    CareerManager cm = FindFirstObjectByType<CareerManager>();
    gsp = FindFirstObjectByType<GameSettingsPersist>();
    gsp.teams = teams;
    
    // Don't overwrite playoff earnings - they're already calculated correctly
    // For cash games, use cm.cash directly
    if (gsp.cashGame)
    {
        gsp.tournyEarnings = cm.cash;
    }
    
    // Restore cumulative team stats (wins/losses/earnings) from before tournament started
    if (gsp.cashGame == false)
    {
        Debug.Log("cm.teamRecords Length is " + cm.teamRecords.Length);

        for (int i = 0; i < teams.Length; i++)
        {
            teams[i].wins += (int)cm.teamRecords[i].x;
            teams[i].loss += (int)cm.teamRecords[i].y;
            teams[i].earnings += cm.teamRecords[i].z;
            teams[i].id = (int)cm.teamRecords[i].w;
        }
        
        Debug.Log($"[TournyManager] Player team cumulative record: {teams[playerTeam].wins}-{teams[playerTeam].loss}, earnings: ${teams[playerTeam].earnings:N0}");
    }

    Debug.Log("PlayerTeam record is " + gsp.tournyRecord.x + " - " + gsp.tournyRecord.y);

    gsp.draw = 0;
    gsp.playoffRound = 0;
    gsp.tournyInProgress = false;
    Debug.Log("gsp.inProgress is " + gsp.tournyInProgress);
    gsp.playoffTeams = null;
    Debug.Log("CM Record is " + cm.record.x + " - " + cm.record.y);
    Debug.Log("CM earnings are " + cm.earnings);
    
    // CRITICAL FIX: Mark tournament as complete BEFORE saving
    cm.TournyResults();  // This sets currentTourny.complete = true
    
    // CRITICAL FIX: Save career AFTER marking tournament complete
    // This ensures the completion status is persisted to the save file
    TournySelector tSel = FindFirstObjectByType<TournySelector>();
    cm.SaveCareer(tSel: tSel);
    
    Debug.Log($"[TournyManager] Saved career after tournament completion - {cm.currentTourny.name}.complete = {cm.currentTourny.complete}");
    
    // NOW it's safe to load scene
    // DO NOT call cm.LoadCareer() here - it would reload old save data!
    SceneManager.LoadScene("Arena_Selector");
}
```

### Change in `CareerManager.cs` ? `TournyResults()`

Add logging to verify tournament completion is being marked:

```csharp
public void TournyResults()
{
    // ... existing XP and earnings calculations ...
    
    // Mark current tournament as complete
    if (currentTourny != null)
    {
        currentTourny.complete = true;
        
        Debug.Log($"[CareerManager.TournyResults] Marked {currentTourny.name} as complete");
        
        // Also mark in the appropriate array
        if (currentTourny.tour)
        {
            for (int i = 0; i < tour.Length; i++)
            {
                if (tour[i] != null && tour[i].id == currentTourny.id)
                {
                    tour[i].complete = true;
                    Debug.Log($"[CareerManager.TournyResults] Marked tour[{i}] ({tour[i].name}) as complete");
                    break;
                }
            }
        }
        else if (currentTourny.qualifier)
        {
            for (int i = 0; i < prov.Length; i++)
            {
                if (prov[i] != null && prov[i].id == currentTourny.id)
                {
                    prov[i].complete = true;
                    Debug.Log($"[CareerManager.TournyResults] Marked prov[{i}] ({prov[i].name}) as complete");
                    break;
                }
            }
        }
        else if (currentTourny.championship)
        {
            for (int i = 0; i < champ.Length; i++)
            {
                if (champ[i] != null && champ[i].id == currentTourny.id)
                {
                    champ[i].complete = true;
                    Debug.Log($"[CareerManager.TournyResults] Marked champ[{i}] ({champ[i].name}) as complete");
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < tournies.Length; i++)
            {
                if (tournies[i] != null && tournies[i].id == currentTourny.id)
                {
                    tournies[i].complete = true;
                    Debug.Log($"[CareerManager.TournyResults] Marked tournies[{i}] ({tournies[i].name}) as complete");
                    break;
                }
            }
        }
    }
    
    // ... rest of method ...
}
```

## Why This Fixes the Issue

**Before the fix:**
1. `TournyResults()` marks tournament complete ?
2. `LoadCareer()` loads old save data (overwrites completion) ?
3. Scene switches ? tournament selector shows incomplete tournament ?

**After the fix:**
1. `TournyResults()` marks tournament complete ?
2. `SaveCareer()` saves completion status to file ?
3. Scene switches ? tournament selector loads correct completion status ?

## Testing

### Test Case 1: Mid-Tournament Save ? Complete
1. Start new career
2. Play tournament A, play 1 draw
3. Save and quit (mid-tournament save created)
4. Load save
5. Finish tournament A
6. Return to tournament selector
7. ? **Expected:** Tournament A should NOT appear in available tournaments

### Test Case 2: Mid-Tournament Save ? Complete ? Next Week
1. Start new career
2. Play tournament A, play 1 draw
3. Save and quit
4. Load save
5. Finish tournament A
6. NextWeek()
7. ? **Expected:** Tournament A should still be marked complete, not available again

### Test Case 3: Multiple Tournaments
1. Play tournament A (complete)
2. Play tournament B, save mid-tournament
3. Load save
4. Complete tournament B
5. Return to selector
6. ? **Expected:** Both A and B should be marked complete

## Additional Considerations

### Scriptable Object Persistence

Note that `Tourny` is a `ScriptableObject`, which means:
- Changes to `tourny.complete` persist **in memory** during play session
- Changes are **lost when Unity editor restarts** (in editor)
- In builds, they persist until app restart

The save system handles this by:
- Saving `tourny.complete` status to JSON file
- Restoring it on load via `ApplyTournamentData()`

### Save Timing

The save must happen **after** `TournyResults()` but **before** scene switch:

```csharp
cm.TournyResults();       // Mark complete
cm.SaveCareer(tSel: tSel); // Persist to file
SceneManager.LoadScene();  // Switch scene
```

**DO NOT** call `cm.LoadCareer()` before switching scenes - it would reload old data!

## Files Changed

1. **`Assets/Scripts/Tourny/TournyManager.cs`**
   - Modified `TournyComplete()` to save after marking tournament complete
   - Removed `cm.LoadCareer()` call (was overwriting completion status)

2. **`Assets/Scripts/Tourny/CareerManager.cs`**
   - Added logging to `TournyResults()` to verify completion marking

## Summary

**Problem:** Mid-tournament saves caused completed tournaments to reappear as available

**Root Cause:** Tournament completion was marked but not saved before loading old save data

**Solution:** Save career AFTER marking tournament complete, and BEFORE switching scenes

**Result:** Tournament completion status persists correctly across save/load cycles
