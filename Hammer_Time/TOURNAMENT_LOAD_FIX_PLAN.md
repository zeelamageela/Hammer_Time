# Tournament Load Fix Plan

## Problem
When loading a saved tournament, the code tries to "process" game results that were already processed and saved. This leads to:
- `draw` value being wrong
- Teams having incorrect win/loss records
- Standings not displaying correctly

## Root Cause
The `SetupStandings()` method has 4 different code paths that all try to "process" results:
1. `justFinishedGame` ? Process result + simulate rest
2. `gameInProgress` ? Process result + simulate rest  
3. `tournyInProgress` ? Just display
4. `else` fallback ? Process result + simulate rest

But when loading from save, **teams already have their correct wins/losses**! We don't need to process anything.

## Solution
**Simplify the logic**: When `gsp.draw > 0` (loading from save), just **display the teams AS-IS**.

The teams loaded from save already have:
- Correct `wins` and `losses`
- Correct `nextOpp`  
- Correct `draw` value

All we need to do is **show the standings**!

## Implementation
Replace the complex conditional tree with:
```csharp
if (gsp.draw > 0)
{
    // Load teams from save
    teams = gsp.teams;  // Already have correct wins/losses!
    draw = gsp.draw;
    
    // Find player team
    for (int i = 0; i < teams.Length; i++)
    {
        if (teams[i].player)
            playerTeam = i;
    }
    
    if (playoffRound > 0)
    {
        // Handle playoffs
        pm.enabled = true;
        standings.SetActive(false);
    }
    else
    {
        // Just display the standings - teams already have correct records!
        gsp.careerLoad = false;
        
        // Find opponent
        for (int i = 0; i < teams.Length; i++)
        {
            if (teams[i].name == teams[playerTeam].nextOpp)
                oppTeam = i;
        }
        
        // Display current state
        yield return new WaitForEndOfFrame();
        StartCoroutine(DrawScoring());
    }
}
```

## Why This Works
1. **EndMenu.EndGame()** already:
   - Updates team wins/losses
   - Increments `draw`
   - Saves everything to file

2. **CareerManager.LoadCareer()** restores:
   - Teams with correct wins/losses
   - Correct `draw` value  
   - Correct tournament state

3. **TournyManager** just needs to:
   - Load the teams
   - Display them!

No processing, no simulation, no incrementing - just **display what's already there**.

## Files to Modify
- `Assets/Scripts/Tourny/TournyManager.cs` - Simplify `SetupStandings()`

## Testing
1. Start tournament
2. Play first game
3. Quit at tournament home
4. Continue
5. **EXPECT**: Standings show with correct wins/losses for all teams
