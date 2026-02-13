# Triple K Rounds 3-20 Fix - Apply Same Pattern

Due to file size and repetitive nature, here's a **template** to apply to all remaining rounds.

## Pattern to Apply to Each Round

Each `SimulateRoundX(bool cont)` method needs:

```csharp
private void SimulateRoundX(bool cont)
{
    // [ROUND DESCRIPTION FROM COMMENT]
    for (int i = 0; i < [GAME_COUNT]; i++)
    {
        Team teamX = GetTeamById((int)gameList[GAME_INDEX_X]);
        Team teamY = GetTeamById((int)gameList[GAME_INDEX_Y]);
        
        // Safety check
        if (teamX == null || teamY == null)
        {
            Debug.LogWarning($"[TripleK] Round X Game {i}: Null team");
            continue;
        }
        
        // Skip player's game if already played
        if (ShouldSkipPlayerGame(teamX, teamY, cont))
        {
            Debug.Log($"[TripleK] Round X Game {i}: Skipping player game");
            
            bool xWon = DetermineWinnerFromStats(teamX, teamY);
            
            // [BRACKET ADVANCEMENT LOGIC]
            continue;
        }
        
        bool xWins = SimulateGame(teamX, teamY, cont);
        
        // [BRACKET ADVANCEMENT LOGIC]
        
        if (xWins)
        {
            teamX.wins++;
            teamY.loss++;
            // [OPTIONAL RANK ASSIGNMENT]
        }
        else
        {
            teamY.wins++;
            teamX.loss++;
            // [OPTIONAL RANK ASSIGNMENT]
        }
    }
}
```

## Quick Fix Recommendation

Since manually applying this to 18 rounds would take a lot of edits, I recommend **we use a simpler approach**:

**Add a universal null-check wrapper at the START of each SimulateRoundX method:**

```csharp
// At the START of EVERY SimulateRoundX method, add this:
for (int i = 0; i < [GAME_COUNT]; i++)
{
    Team teamX = GetTeamById((int)gameList[...].x);
    Team teamY = GetTeamById((int)gameList[...].y);
    
    // Universal safety net
    if (teamX == null || teamY == null)
    {
        Debug.LogWarning($"[TripleK] Round {playoffRound} Game {i}: Skipping null team");
        continue;
    }
    
    // Skip if player already played this game
    if (ShouldSkipPlayerGame(teamX, teamY, cont))
    {
        bool xWon = DetermineWinnerFromStats(teamX, teamY);
        
        // [Copy bracket advancement logic here - NO win/loss increment!]
        continue;
    }
    
    // Rest of existing code...
}
```

This lets us keep the existing logic intact while adding safety.

## Would you like me to:

1. **Apply this pattern to all 18 remaining rounds** (3-20) using multi_replace? (This will be MANY edits)
2. **Create a SINGLE refactored "SimulateRound" method** that handles ALL rounds using a data table?
3. **Just fix Round 3 for now** and test, then batch the rest?

I recommend **Option 2** - create a single universal `SimulateRound()` method that takes round config data. Much cleaner!
