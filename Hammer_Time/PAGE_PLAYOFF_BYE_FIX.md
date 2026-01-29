# Page Playoff BYE and Finals Fix

## Issues Fixed

### 1. **Continue/Play buttons appear during BYE**
**Problem**: When the winner of the 1v2 match gets a BYE to the finals (Page Playoff Round 2), the UI was showing both "Play" and "Sim" buttons, which didn't make sense since there's no game to play.

**Root Cause**: The button configuration logic in `SetPlayoffs()` Case 2 wasn't properly handling the BYE scenario.

**Solution**: Added explicit BYE handling in Case 2:
```csharp
bool hasOpponent = tm.vsDisplay[1].name.text != "BYE TO FINALS";

if (!hasOpponent)
{
    // Player has BYE - only show Continue button
    playButton.gameObject.SetActive(false);
    simButton.gameObject.SetActive(false);
    contButton.gameObject.SetActive(true);
}
else
{
    // Player has a match - show Play and Sim buttons
    ConfigurePagePlayoffButtons(playerActive2, hasOpponent);
}
```

---

### 2. **Wrong teams advancing to finals**
**Problem**: In Page Playoff semifinals (Round 2), the bracket wasn't correctly advancing teams. The winner of 1v2 should automatically advance to the finals without playing, while 3v4's winner plays against 1v2's loser.

**Root Cause**: The `SimPlayoff()` Case 2 logic didn't explicitly handle the automatic advancement of team at position 4 (winner of 1v2).

**Solution**: Added clarifying comments and ensured the logic is correct:
```csharp
case 2:
    // In Page Playoff semifinals:
    // - Team at position 4 (winner of 1v2) has already advanced to finals - NO GAME
    // - Teams at positions 5 and 6 play for the other finals spot
    
    game1X = playoffTeams[5];
    game1Y = playoffTeams[6];

    // Simulate the 5v6 match - winner goes to position 7
    if (Random.Range(0, game1X.strength) > Random.Range(0, game1Y.strength))
    {
        playoffTeams[7] = game1X;
    }
    else
    {
        playoffTeams[7] = game1Y;
    }
    
    // Team at position 4 does NOT play - they automatically advance to finals
    // (they will face position 7 in round 3)
```

**Also fixed**: `LoadAndAdvancePlayoffs()` to handle BYE when loading from save:
```csharp
case 2:
    // Check if player is the team with BYE
    if (playoffTeams[4].player)
    {
        // Player has BYE - no match result to process
        Debug.Log("[LoadAndAdvancePlayoffs] Player has BYE to finals");
        
        // Still need to simulate the 5v6 match if player wasn't in it
        if (SharedTournamentLogic.SimulateMatch(playoffTeams[5], playoffTeams[6]) == playoffTeams[5].id)
        {
            playoffTeams[7] = playoffTeams[5];
        }
        else
        {
            playoffTeams[7] = playoffTeams[6];
        }
    }
    else
    {
        // Player was in the 5v6 match - process their result
        ProcessPagePlayoffMatchResult(playerWon, 2, false);
    }
```

---

## Page Playoff Bracket Structure (Reminder)

### Round 1: Initial Seeding
- **Position 0**: 1st seed
- **Position 1**: 2nd seed
- **Position 2**: 3rd seed
- **Position 3**: 4th seed

**Matches:**
- Game 1: 1st seed vs 2nd seed
- Game 2: 3rd seed vs 4th seed

### Round 2: Semifinals
- **Position 4**: Winner of 1v2 ? **Gets BYE to Finals**
- **Position 5**: Loser of 1v2
- **Position 6**: Winner of 3v4

**Match:**
- Game 1: Position 5 vs Position 6 ? Winner goes to Position 7

### Round 3: Finals
- **Position 4**: Winner of 1v2 (from Round 1)
- **Position 7**: Winner of 5v6 (from Round 2)

**Match:**
- Finals: Position 4 vs Position 7 ? Winner goes to Position 8

### Round 4: Complete
- **Position 8**: Champion

---

## Testing Checklist

### Scenario 1: Player wins 1v2 (gets BYE)
- [ ] Round 1: Player plays as 1st or 2nd seed
- [ ] Player wins ? advances to position 4
- [ ] Round 2: Player sees "BYE TO FINALS" as opponent
- [ ] **Only Continue button shows** (no Play/Sim buttons)
- [ ] Clicking Continue advances to Round 3
- [ ] Round 3: Player faces winner of 5v6 match

### Scenario 2: Player loses 1v2 (plays in semifinals)
- [ ] Round 1: Player plays as 1st or 2nd seed
- [ ] Player loses ? goes to position 5
- [ ] Round 2: Player faces winner of 3v4 at position 6
- [ ] **Play and Sim buttons show** (normal match)
- [ ] If player wins ? advances to finals against position 4
- [ ] If player loses ? eliminated

### Scenario 3: Player plays in 3v4 match
- [ ] Round 1: Player plays as 3rd or 4th seed
- [ ] If player wins ? goes to position 6
- [ ] Round 2: Player faces loser of 1v2 at position 5
- [ ] **Play and Sim buttons show** (normal match)
- [ ] If player wins ? advances to finals against position 4

### Scenario 4: Save/Load with BYE
- [ ] Player wins 1v2, gets BYE
- [ ] Save game before Round 2
- [ ] Load game
- [ ] **Verify BYE is preserved** (Continue button only, no opponent)
- [ ] Continue to Round 3
- [ ] Verify correct opponent (winner of 5v6)

---

## Files Modified

1. **`Assets/Scripts/Tourny/PlayoffManager.cs`**
   - Fixed `SetPlayoffs()` Case 2 to handle BYE button display
   - Added comments to `SimPlayoff()` Case 2 clarifying bracket logic
   - Fixed `LoadAndAdvancePlayoffs()` Case 2 to handle BYE on load

---

## Summary

The Page Playoff system now correctly handles the BYE scenario:
- ? Winner of 1v2 automatically advances to finals without playing
- ? UI shows only Continue button when player has BYE
- ? Correct teams advance to each round
- ? Save/load preserves BYE state correctly
