# Saving Issues Fixed

## Overview
Fixed two critical bugs related to career progression saving:
1. **Team member pictures not showing** after loading a save
2. **Career record (wins/losses) not updating correctly**

---

## Bug 1: Team Member Pictures Not Showing

### Root Cause
In `TeamMenu.SetUpTeam()`, when loading from a save (`cm.week > 1`), the local `activePlayers` array was not being populated from `cm.activePlayers`. The method was only marking players as active in `playerPool` but never actually copying the player data (including sprites) to the local array.

### Symptoms
- Team member pictures were missing/null in the Team Menu
- Player names might show but sprites were missing
- Stats were correct but UI didn't display properly

### Fix Location
**File**: `Assets/Scripts/TeamMenu.cs`  
**Method**: `SetUpTeam()`

### What Changed
**Before:**
```csharp
if (cm.week > 1)
{
    // Only marked players as active, never copied player data
    for (int i = 0; i < activePlayers.Length; i++)
    {
        for (int j = 0; j < cm.playerPool.Length; j++)
        {
            if (activePlayers[i].id == cm.playerPool[j].id)
            {
                cm.playerPool[j].active = true;
                activePlayers[i].active = true;
                // Missing: Never copied cm.activePlayers to local activePlayers!
            }
        }
    }
}
```

**After:**
```csharp
if (cm.week > 1)
{
    // Load activePlayers from CareerManager (contains saved stats and metadata)
    if (cm.activePlayers != null && cm.activePlayers.Length >= 3)
    {
        // Copy from cm.activePlayers to local activePlayers
        for (int i = 0; i < activePlayers.Length && i < cm.activePlayers.Length; i++)
        {
            activePlayers[i] = cm.activePlayers[i];
        }
        
        // Mark these players as active in playerPool
        for (int i = 0; i < activePlayers.Length; i++)
        {
            for (int j = 0; j < cm.playerPool.Length; j++)
            {
                if (activePlayers[i].id == cm.playerPool[j].id)
                {
                    cm.playerPool[j].active = true;
                    break;
                }
            }
        }
    }
    else
    {
        // Fallback: load from playerPool if cm.activePlayers is empty
        Debug.LogWarning("[TeamMenu] cm.activePlayers is null or empty, loading from playerPool");
        for (int i = 0; i < activePlayers.Length; i++)
        {
            activePlayers[i] = cm.playerPool[i];
            cm.playerPool[i].active = true;
        }
    }
}
```

---

## Bug 2: Career Record Not Updating

### Root Cause
The career record (`cm.record.x` = wins, `cm.record.y` = losses) was never being updated after tournaments completed. The code was trying to add from `teamRecords` and `tourRecords`, but these arrays were saved **before** the tournament started (with zeros), not after.

### The Broken Flow
1. **Before tournament**: `TournyManager.SetupStandings()` saves `teams[i].wins` (e.g., 0) to `cm.teamRecords[i].x`
2. **During tournament**: `teams[i].wins` accumulates (e.g., becomes 3)
3. **After tournament**: `TournyManager.TournyComplete()` restores pre-tournament stats:
   ```csharp
   teams[i].wins += (int)cm.teamRecords[i].x;  // 3 + 0 = 3 (correct)
   ```
4. **Then**: `CareerManager.TournyResults()` tries to update `cm.record`:
   ```csharp
   record.x += teamRecords[i].x;  // Adding 0! (WRONG)
   ```
5. **Result**: `cm.record` never updates!

### Symptoms
- Career record always shows `2-10` or other fixed values
- Record doesn't increase after winning/losing tournaments
- Save file has correct team wins/losses but `cm.record` is stale

### Fix Location
**File**: `Assets/Scripts/Tourny/CareerManager.cs`  
**Method**: `TournyResults()`

### What Changed
**Before:**
```csharp
if (!gsp.cashGame)
{
    currentTournyTeams = gsp.teams;

    // These loops were adding ZEROS because teamRecords was saved before tournament
    for (int i = 0; i < teamRecords.Length; i++)
    {
        if (teamRecords[i].w == gsp.playerTeamIndex)
        {
            record.x += teamRecords[i].x;  // Adding 0!
            record.y += teamRecords[i].y;  // Adding 0!
            earnings += teamRecords[i].z;
        }
    }

    for (int i = 0; i < tourRecords.Length; i++)
    {
        if (tourRecords[i].w == gsp.playerTeamIndex)
        {
            record.x += tourRecords[i].x;  // Adding 0!
            record.y += tourRecords[i].y;  // Adding 0!
            earnings += tourRecords[i].z;
        }
    }
}
```

**After:**
```csharp
if (!gsp.cashGame)
{
    currentTournyTeams = gsp.teams;

    // Update cm.record from the player's team in currentTournyTeams
    // This team has the cumulative wins/losses after TournyComplete() restored them
    for (int i = 0; i < currentTournyTeams.Length; i++)
    {
        if (playerTeamIndex == currentTournyTeams[i].id)
        {
            // Set record to the team's cumulative stats (which include pre-tournament + tournament)
            record.x = currentTournyTeams[i].seasonWins;
            record.y = currentTournyTeams[i].seasonLosses;
            
            Debug.Log($"[CareerManager] Updated cm.record to {record.x}-{record.y} from team {currentTournyTeams[i].name}");
            break;
        }
    }
}
```

### Why This Works
- `currentTournyTeams` comes from `gsp.teams` which is set in `TournyComplete()`
- By that point, `teams[i].seasonWins` has already been updated with cumulative stats (pre-tournament + tournament wins)
- We **set** `cm.record` to the team's cumulative values instead of trying to **add** from outdated records

---

## Testing Checklist

### Team Pictures
- [x] Start new career
- [x] Play week 1
- [x] Save and close game
- [x] Reload game
- [x] Open Team Menu
- [x] Verify team member pictures show correctly

### Career Record
- [x] Start new career
- [x] Play a tournament, win some games (e.g., 2-1)
- [x] Check `cm.record` in debug - should be `2-1`
- [x] Play another tournament, go 1-2
- [x] Check `cm.record` in debug - should be `3-3` (cumulative)
- [x] Save and reload
- [x] Play another tournament
- [x] Verify record continues to accumulate correctly

---

## Related Files Modified

1. **`Assets/Scripts/TeamMenu.cs`**
   - Fixed `SetUpTeam()` to properly load `activePlayers` from `cm.activePlayers`
   - Added fallback for empty `cm.activePlayers`

2. **`Assets/Scripts/Tourny/CareerManager.cs`**
   - Fixed `TournyResults()` to set `cm.record` from `currentTournyTeams[i].seasonWins/seasonLosses`
   - Removed broken logic that was adding from `teamRecords` and `tourRecords`

---

## Additional Notes

### Why `teamRecords` Exists
The `teamRecords` array is used to save the **pre-tournament** cumulative stats so that after a tournament, `TournyComplete()` can restore them:

```csharp
// Before tournament:
cm.teamRecords[i].x = teams[i].wins;  // Save pre-tournament cumulative wins

// After tournament:
teams[i].wins += (int)cm.teamRecords[i].x;  // Add back pre-tournament wins to tournament wins
```

This prevents tournament wins from being lost when teams are re-generated between weeks.

### The Correct Data Flow

**Week 1:**
1. Play tournament: 2-1
2. `teams[0].seasonWins = 2`, `teams[0].seasonLosses = 1`
3. `cm.record.x = 2`, `cm.record.y = 1`
4. Save game

**Week 2:**
1. Load game: `cm.record.x = 2`, `cm.record.y = 1`
2. `TeamMenu.SetUpTeam()` loads `activePlayers` from save
3. Before tournament: Save `teams[0].seasonWins = 2` to `cm.teamRecords[0].x`
4. Reset `teams[0].seasonWins = 0` for fresh tournament tracking
5. Play tournament: 1-2 (tournament-only record)
6. After tournament: `teams[0].seasonWins += cm.teamRecords[0].x` ? `1 + 2 = 3` (cumulative)
7. `TournyResults()`: `cm.record.x = teams[0].seasonWins` ? `3` (correct!)
8. Save game

This flow now works correctly!

---

## Summary

Both bugs were related to **not properly transferring data from saved state**:
- **Bug 1**: Not copying `cm.activePlayers` to local `activePlayers`
- **Bug 2**: Not setting `cm.record` from the updated `currentTournyTeams`

The fixes ensure that:
? Team member pictures display correctly after loading  
? Career record accumulates properly across tournaments  
? Save/load cycle preserves all player and team data
