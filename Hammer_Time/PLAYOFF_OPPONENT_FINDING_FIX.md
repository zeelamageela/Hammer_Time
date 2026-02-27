# Playoff Opponent Finding Fix

## Problem
`LoadAndAdvancePlayoffs()` crashes with NullReferenceException at line 340 when trying to access `tm.teams[playerTeam].nextOpp` after loading a saved playoff game.

**Why it crashes:**
- When loading after completing a game, `nextOpp` hasn't been set yet
- `LoadAndAdvancePlayoffs()` is called BEFORE `SetPlayoffs()` sets up the `nextOpp` field
- The old code assumed `nextOpp` would always be valid

## Solution
Use game scores (`gsp.redTeamName` and `gsp.yellowTeamName`) to find the opponent instead of relying on `nextOpp`.

## Manual Fix Required

**File:** `Assets/Scripts/Tourny/PlayoffManager.cs`

**Location:** Around line 333-342 in `LoadAndAdvancePlayoffs()` method

**REPLACE THIS CODE:**
```csharp
	for (int i = 0; i < tm.teams.Length; i++)
	{
		if (tm.teams[i].player)
			playerTeam = i;
	}
	for (int i = 0; i < tm.teams.Length; i++)
	{
		if (tm.teams[i].name == tm.teams[playerTeam].nextOpp)
			oppTeam = i;
}
Debug.Log("OppTeam is " + oppTeam);
```

**WITH THIS CODE:**
```csharp
	for (int i = 0; i < tm.teams.Length; i++)
	{
		if (tm.teams[i].player)
			playerTeam = i;
	}
	
	// CRITICAL FIX: Find opponent using game scores (more reliable than nextOpp)
	oppTeam = -1;
	
	if (playerTeam >= 0 && playerTeam < tm.teams.Length)
	{
		string playerTeamName = tm.teams[playerTeam].name;
		
		// Use the game that was just played to determine opponent
		if (playerTeamName == gsp.redTeamName)
		{
			// Player was red, opponent was yellow
			for (int i = 0; i < tm.teams.Length; i++)
			{
				if (tm.teams[i].name == gsp.yellowTeamName)
				{
					oppTeam = i;
					break;
				}
			}
		}
		else if (playerTeamName == gsp.yellowTeamName)
		{
			// Player was yellow, opponent was red
			for (int i = 0; i < tm.teams.Length; i++)
			{
				if (tm.teams[i].name == gsp.redTeamName)
				{
					oppTeam = i;
					break;
				}
			}
		}
	}
	
	if (oppTeam < 0)
	{
		Debug.LogError("[LoadAndAdvancePlayoffs] Could not find opponent! Aborting.");
		playoffRound++;
		SetPlayoffs();
		return;
	}
Debug.Log("OppTeam is " + oppTeam);
```

## Why This Works

1. **Game scores are always set**: When a game completes, `gsp.redTeamName` and `gsp.yellowTeamName` contain the teams that just played
2. **No dependency on nextOpp**: Doesn't rely on `nextOpp` being set up first
3. **Safe fallback**: If opponent can't be found, gracefully advances to next screen instead of crashing
4. **Works for all scenarios**: Loading from save, returning from completed game, etc.

## Testing
After applying this fix:
1. Play a playoff game
2. Save and quit mid-tournament
3. Load the tournament
4. Finish the game
5. Return to playoff screen ? **Should NOT crash!**
