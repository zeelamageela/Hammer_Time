# Tournament Refactoring Plan

## Current Problems

### PlayoffManager_SingleK.cs & PlayoffManager_TripleK.cs Issues:
1. **Massive code duplication** - Same logic repeated across all 3 playoff managers
2. **Complex switch statements** - Each round has 200+ lines of repetitive code
3. **Manual bracket management** - Error-prone array indexing for team placement
4. **Fragile state management** - Easy to lose track of tournament state
5. **Inconsistent save/load** - Different logic in each manager
6. **Hard to debug** - Difficult to trace where problems occur

## Recommended Solution: Incremental Refactoring

Rather than a complete rewrite, we'll refactor incrementally to maintain stability.

### Phase 1: Consolidate Common Logic

#### Step 1.1: Create TournamentState Helper Class
Create a simple class to track tournament state:

```csharp
[Serializable]
public class TournamentState
{
    public int currentRound;
    public int playerTeamId;
    public int oppTeamId;
    public Team[] teams;
    public Dictionary<int, int> teamPlacements; // teamId -> rank
    
    // Track which matches have been played
    public HashSet<string> playedMatches = new HashSet<string>();
    
    public void RecordMatch(int round, int team1Id, int team2Id, int winnerId)
    {
        string matchKey = $"R{round}_T{team1Id}vs{team2Id}";
        playedMatches.Add(matchKey);
    }
    
    public bool IsMatchPlayed(int round, int team1Id, int team2Id)
    {
        string matchKey = $"R{round}_T{team1Id}vs{team2Id}";
        return playedMatches.Contains(matchKey);
    }
}
```

#### Step 1.2: Create SharedTournamentLogic Static Class
Move common methods to a static helper:

```csharp
public static class SharedTournamentLogic
{
    /// <summary>
    /// Simulate a match between two teams
    /// </summary>
    public static int SimulateMatch(Team team1, Team team2)
    {
        if (Random.Range(0, team1.strength) > Random.Range(0, team2.strength))
            return team1.id;
        else
            return team2.id;
    }
    
    /// <summary>
    /// Record match result for both teams
    /// </summary>
    public static void RecordMatchResult(Team winner, Team loser)
    {
        winner.tournamentWins++;
        loser.tournamentLosses++;
    }
    
    /// <summary>
    /// Calculate prize distribution using exponential decay
    /// </summary>
    public static float CalculatePrize(int rank, int totalTeams, float totalPrize)
    {
        switch (rank)
        {
            case 1: return totalPrize * 0.5f;
            case 2: return totalPrize * 0.25f;
            case 3: return totalPrize * 0.15f;
            case 4: return totalPrize * 0.075f;
            case 5: return totalPrize * 0.038f;
            default:
                float p = 1.4f;
                float remaining = totalTeams - 5f;
                float prizePayout = ((Mathf.Pow(p, remaining - (rank - 6))) / 
                                   (Mathf.Pow(p, remaining) - 1f)) * 
                                   (totalPrize * 0.15f) * (p - 1);
                return prizePayout;
        }
    }
    
    /// <summary>
    /// Determine player's opponent based on team ID
    /// </summary>
    public static int GetOpponentId(Team[] bracket, int playerTeamId, int matchIndex)
    {
        int team1Index = matchIndex * 2;
        int team2Index = matchIndex * 2 + 1;
        
        if (bracket[team1Index].id == playerTeamId)
            return bracket[team2Index].id;
        else if (bracket[team2Index].id == playerTeamId)
            return bracket[team1Index].id;
        
        return -1;
    }
}
```

### Phase 2: Simplify PlayoffManager_SingleK

#### Step 2.1: Replace LoadAndAdvancePlayoffs
Current code is 200+ lines. Replace with:

```csharp
void LoadAndAdvancePlayoffs()
{
    Debug.Log($"Loading playoffs - Round {playoffRound}");
    
    // Load saved teams
    for (int i = 0; i < playoffTeams.Length; i++)
        playoffTeams[i] = gsp.playoffTeams[i];
    
    // Find player team
    playerTeam = FindPlayerTeamIndex();
    
    // Record player's game result if returning from a match
    if (gsp.redScore != gsp.yellowScore)
    {
        RecordPlayerMatchResult();
    }
    
    // Simulate remaining matches in current round
    SimulateRound(playoffRound);
    
    SetPlayoffs();
}

int FindPlayerTeamIndex()
{
    for (int i = 0; i < playoffTeams.Length; i++)
    {
        if (playoffTeams[i].player)
            return i;
    }
    return -1;
}

void RecordPlayerMatchResult()
{
    bool playerWon = DeterminePlayerWon();
    int oppIndex = GetPlayerOpponentIndex();
    
    if (playerWon)
    {
        AdvanceWinner(playerTeam, oppIndex);
        playoffTeams[oppIndex].rank = GetEliminationRank(playoffRound);
    }
    else
    {
        AdvanceWinner(oppIndex, playerTeam);
        playoffTeams[playerTeam].rank = GetEliminationRank(playoffRound);
    }
}

bool DeterminePlayerWon()
{
    if (gsp.playerTeam.name == gsp.redTeamName)
        return gsp.redScore > gsp.yellowScore;
    else
        return gsp.yellowScore > gsp.redScore;
}

int GetEliminationRank(int round)
{
    // Single elimination ranks by round
    switch (round)
    {
        case 1: return 9;  // Lost in Round of 16
        case 2: return 5;  // Lost in Quarterfinals
        case 3: return 3;  // Lost in Semifinals
        case 4: return 2;  // Lost in Finals
        default: return 99;
    }
}

void AdvanceWinner(int winnerIndex, int loserIndex)
{
    int nextRoundIndex = CalculateNextRoundIndex(playoffRound, winnerIndex);
    playoffTeams[nextRoundIndex] = playoffTeams[winnerIndex];
}

int CalculateNextRoundIndex(int round, int currentIndex)
{
    // Map current position to next round position
    // Round 1 (indices 0-15) -> Round 2 (indices 16-23)
    // Round 2 (indices 16-23) -> Round 3 (indices 24-27)
    // Round 3 (indices 24-27) -> Round 4 (indices 28-29)
    // Round 4 (indices 28-29) -> Finals (index 30)
    
    int[] roundStarts = { 0, 16, 24, 28, 30 };
    int positionInRound = currentIndex - roundStarts[round - 1];
    return roundStarts[round] + (positionInRound / 2);
}
```

#### Step 2.2: Simplify SimPlayoff
Current has massive switch statement. Replace with:

```csharp
IEnumerator SimPlayoff(int skipMatchIndex)
{
    Debug.Log($"Simulating playoffs - Round {playoffRound}");
    
    int[] roundConfig = GetRoundConfiguration(playoffRound);
    int startIndex = roundConfig[0];
    int matchCount = roundConfig[1];
    int nextRoundStart = roundConfig[2];
    
    for (int i = 0; i < matchCount; i++)
    {
        int matchIndex = i * 2;
        int team1Index = startIndex + matchIndex;
        int team2Index = startIndex + matchIndex + 1;
        
        // Skip player's match
        if (skipMatchIndex != 99 && i == skipMatchIndex)
            continue;
        
        // Simulate match
        Team team1 = playoffTeams[team1Index];
        Team team2 = playoffTeams[team2Index];
        int winnerId = SharedTournamentLogic.SimulateMatch(team1, team2);
        
        // Advance winner
        Team winner = (winnerId == team1.id) ? team1 : team2;
        Team loser = (winnerId == team1.id) ? team2 : team1;
        
        playoffTeams[nextRoundStart + i] = winner;
        loser.rank = GetEliminationRank(playoffRound);
        
        SharedTournamentLogic.RecordMatchResult(winner, loser);
    }
    
    // Update UI
    UpdateBracketDisplay();
    
    playoffRound++;
    simButton.gameObject.SetActive(false);
    contButton.gameObject.SetActive(true);
    StartCoroutine(SetPlayoffs());
    
    yield break;
}

int[] GetRoundConfiguration(int round)
{
    // Returns [startIndex, matchCount, nextRoundStart]
    switch (round)
    {
        case 1: return new int[] { 0, 8, 16 };   // Round of 16
        case 2: return new int[] { 16, 4, 24 };  // Quarterfinals
        case 3: return new int[] { 24, 2, 28 };  // Semifinals
        case 4: return new int[] { 28, 1, 30 };  // Finals
        default: return new int[] { 0, 0, 0 };
    }
}

void UpdateBracketDisplay()
{
    int[] config = GetRoundConfiguration(playoffRound);
    int startIndex = config[0];
    int endIndex = config[2];
    
    for (int i = startIndex; i < endIndex; i++)
    {
        int displayIndex = i;
        if (i < roundOf16Display.Length)
        {
            UpdateDisplay(roundOf16Display[i], playoffTeams[i]);
        }
        else if (i < roundOf16Display.Length + quartersDisplay.Length)
        {
            int idx = i - roundOf16Display.Length;
            UpdateDisplay(quartersDisplay[idx], playoffTeams[i]);
        }
        // ... continue for other rounds
    }
}

void UpdateDisplay(BracketDisplay display, Team team)
{
    display.name.text = team.name;
    display.rank.text = team.rank == 0 ? "" : team.rank.ToString();
    display.name.transform.parent.gameObject.SetActive(true);
}
```

### Phase 3: Apply Same Pattern to TripleK

The same refactoring principles apply to PlayoffManager_TripleK:
- Extract common logic
- Use configuration arrays instead of switch statements
- Simplify bracket advancement logic

### Phase 4: Testing Strategy

1. **Test Round Advancement**
   - Verify teams advance correctly after each round
   - Check rankings are assigned properly
   - Ensure player match detection works

2. **Test Save/Load**
   - Save mid-tournament
   - Close and reopen
   - Verify state is restored correctly

3. **Test Prize Distribution**
   - Verify all teams get correct prize money
   - Check player receives proper amount

4. **Test Edge Cases**
   - Player eliminated early
   - Player wins tournament
   - Simulating through entire tournament

## Implementation Timeline

### Week 1: Foundation
- Create SharedTournamentLogic class
- Create TournamentState helper
- Test both classes independently

### Week 2: Single Elimination
- Refactor PlayoffManager_SingleK
- Test thoroughly
- Fix any issues

### Week 3: Triple Knockout
- Refactor PlayoffManager_TripleK
- Test thoroughly
- Fix any issues

### Week 4: Polish
- Add logging for debugging
- Performance optimization
- Final testing

## Benefits

? **Reduced Code** - From 3000+ lines to ~1000 lines  
? **Easier Debugging** - Clear, traceable logic  
? **Maintainable** - Changes in one place affect all tournaments  
? **Testable** - Can unit test helper methods  
? **Extensible** - Easy to add new tournament types  

## Migration Path

1. Keep old files as backup (rename to .OLD)
2. Implement new system alongside old
3. Test new system thoroughly
4. Switch to new system when confident
5. Remove old files

This approach minimizes risk while maximizing improvement!
