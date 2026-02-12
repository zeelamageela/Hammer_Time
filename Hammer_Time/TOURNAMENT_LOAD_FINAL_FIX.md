# Tournament Load Final Fix

## Issues Remaining
1. **Wrong player loaded** - Random team shown as player instead of actual player
2. **cm.record not syncing** - CareerManager record doesn't update with tournament progress

## Root Cause
When loading tournament from save in `TournyManager.SetupStandings()`:
- `teamList` is rebuilt, but `playerTeam` index isn't found
- Teams array may be in different order than when saved
- Need to find player by `team.player` flag, not by saved index

## Solution

Replace lines 189-208 in `TournyManager.cs` (the `if (gsp.draw > 0)` block) with:

```csharp
if (gsp.draw > 0)
{
    playoffRound = gsp.playoffRound;
    teams = gsp.teams;
    draw = gsp.draw;

    // Rebuild teamList from loaded teams
    teamList = new List<Team_List>();
    for (int i = 0; i < teams.Length; i++)
    {
        if (teams[i] != null)
        {
            teamList.Add(new Team_List(teams[i]));
        }
    }
    Debug.Log($"[TournyManager] Rebuilt teamList from save - {teamList.Count} teams");

    // CRITICAL: Find player team by player flag, not by index
    for (int i = 0; i < teams.Length; i++)
    {
        if (teams[i].player)
        {
            playerTeam = i;
            Debug.Log($"[TournyManager] Found player team at index {playerTeam}: {teams[i].name}");
            break;
        }
    }
    
    // Find opponent team
    for (int i = 0; i < teams.Length; i++)
    {
        if (teams[i].name == teams[playerTeam].nextOpp)
        {
            oppTeam = i;
            Debug.Log($"[TournyManager] Found opponent team at index {oppTeam}: {teams[i].name}");
            break;
        }
    }

    // Initialize drawFormat
    Debug.Log($"[TournyManager.SetupStandings] Calling DrawSelector with teams.Length={teams.Length}, games={gsp.games}");
    dfList.DrawSelector(teams.Length, 1, gsp.games);
    yield return new WaitForEndOfFrame();
    drawFormat = dfList.currentFormat;
    
    Debug.Log($"[TournyManager.SetupStandings] draw={draw}, drawFormat.Length={drawFormat?.Length ?? 0}, teamList.Count={teamList.Count}");

    // CRITICAL: Sync cm.record with tournament stats
    if (cm != null)
    {
        cm.record.x = teams[playerTeam].seasonWins;
        cm.record.y = teams[playerTeam].seasonLosses;
        Debug.Log($"[TournyManager] Synced cm.record to {cm.record.x}-{cm.record.y}");
    }

    if (playoffRound > 0)
```

## What This Fixes

### 1. Correct Player Team Loading
- **Before**: Used `gsp.playerTeamIndex` which might be wrong after shuffle
- **After**: Searches for team with `team.player = true` flag

### 2. CareerManager Record Sync
- **Before**: `cm.record` not updated, shows stale data
- **After**: Syncs `cm.record` with current `seasonWins`/`seasonLosses`

### 3. Correct Opponent Team
- **Before**: Opponent not found, causes crashes
- **After**: Finds opponent by matching `nextOpp` name

## Testing
1. Start tournament
2. Play first game  
3. Quit to main menu
4. Continue
5. ? Your team should be highlighted
6. ? Correct opponent shown in VS panel
7. ? Record shows correct wins/losses
