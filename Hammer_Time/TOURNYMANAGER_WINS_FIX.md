# TournyManager Wins/Losses Double-Counting Fix

## Issue
When returning to a round-robin tournament after playing a game, both the player and opponent were showing double the expected wins/losses (e.g., 2-0 instead of 1-0). This was the same issue that affected `PlayoffManager_SingleK`.

## Root Cause
The wins/losses were being updated in two places:
1. **In `SetupStandings()`** - When processing the game result after returning from a match
2. **In `SimRestDraw()`** - When simulating the rest of the draw

The logic in `SimRestDraw()` attempted to skip the player's game, but it still incremented wins/losses for all teams, causing double-counting.

## Solution

### 1. Created `ProcessPlayerMatchResult()` Helper Method
```csharp
void ProcessPlayerMatchResult()
{
    bool playerWon = false;
    
    // Determine if player won based on their team color
    if (teams[playerTeam].name == gsp.redTeamName)
        playerWon = gsp.redScore > gsp.yellowScore;
    else if (teams[playerTeam].name == gsp.yellowTeamName)
        playerWon = gsp.yellowScore > gsp.redScore;
    
    // Update wins and losses ONCE
    if (playerWon)
    {
        teams[playerTeam].wins++;
        teams[oppTeam].loss++;
    }
    else
    {
        teams[oppTeam].wins++;
        teams[playerTeam].loss++;
    }
}
```

**Benefits:**
- Eliminates code duplication (was in 2 places in `SetupStandings`)
- Centralizes match result logic
- Makes it clear wins/losses are updated exactly once
- Better logging for debugging

### 2. Refactored `SetupStandings()`
**Before:** Had duplicate code blocks processing player match results
```csharp
if (teams[playerTeam].name == gsp.redTeamName)
{
    if (gsp.redScore > gsp.yellowScore)
    {
        teams[oppTeam].loss++;
        teams[playerTeam].wins++;
    }
    // ... etc (repeated twice)
}
```

**After:** Uses the helper method
```csharp
// Find player and opponent teams
for (int i = 0; i < teams.Length; i++)
{
    if (teams[i].player)
        playerTeam = i;
}
for (int i = 0; i < teams.Length; i++)
{
    if (teams[i].name == teams[playerTeam].nextOpp)
        oppTeam = i;
}

// Process player's match result and update wins/losses
ProcessPlayerMatchResult();

// Move to next draw and simulate remaining games
draw--;
StartCoroutine(SimRestDraw());
```

### 3. Refactored `SimRestDraw()`
**Before:** Had confusing logic that tried to skip player's game but still updated wins/losses
```csharp
if (games[i].name == teams[playerTeam].name | games[i].name == teams[oppTeam].name)
{
    Debug.Log("Player Game skip sim - " + i + " - " + games[i].name);
}
else if (Random.Range(0, games[i].strength) > Random.Range(0, games[i + 1].strength))
{
    games[i + 1].loss++;
    games[i].wins++;
}
// ... etc
```

**After:** Properly skips player's game using `continue`
```csharp
// Skip the player's game - wins/losses already updated in ProcessPlayerMatchResult
if (games[i].name == teams[playerTeam].name || games[i].name == teams[oppTeam].name ||
    games[i + 1].name == teams[playerTeam].name || games[i + 1].name == teams[oppTeam].name)
{
    Debug.Log("Player Game skip sim - " + i + " - " + games[i].name);
    continue; // Skip to next iteration
}

// Simulate other games
if (Random.Range(0, games[i].strength) > Random.Range(0, games[i + 1].strength))
{
    games[i + 1].loss++;
    games[i].wins++;
}
// ... etc
```

## Code Flow After Fix

### When Player Returns from Match:
1. `SetupStandings()` loads saved game state
2. Finds `playerTeam` and `oppTeam` indices
3. Calls `ProcessPlayerMatchResult()` ? **Wins/losses updated ONCE**
4. Decrements `draw` counter
5. Calls `SimRestDraw()`
6. `SimRestDraw()` simulates remaining games, **skipping** player's game (no double-counting)

### When Simulating All Games:
1. `SimDraw()` is called
2. Simulates all games including player's game
3. Updates wins/losses for all teams

## Related Fixes
This follows the same pattern used to fix `PlayoffManager_SingleK`:
- `ProcessPlayerMatchResult()` updates wins/losses when returning from played game
- Simulation methods skip the player's game to avoid double-counting
- Clear separation between "processing result" and "simulating games"

## Files Modified
- `Assets\Scripts\Tourny\TournyManager.cs`

## Testing Checklist
- [ ] Play a round-robin tournament game and return - verify 1-0 record (not 2-0)
- [ ] Sim a round-robin draw - verify correct win/loss counts
- [ ] Play multiple games in tournament - verify cumulative records are accurate
- [ ] Verify opponent's record is also correct (not double-counted)
