# T-Line Sweeper Skip Stats Implementation

**Status**: ? **COMPLETE** - T-line sweepers now use actual skip stats instead of team averages!

---

## Problem

Previously, T-line sweepers (for sweeping opponent rocks behind Y=6.5) used **team average sweep stats**, which was unrealistic. In real curling, the **skip** (position 4) typically sweeps behind the tee line, so the sweeper should use the skip's individual stats.

---

## Solution

Updated `TeamManager.SetSweepers()` to:
1. **AI Teams**: Use the opposing team's **skip player stats** (player index 3 in `team.players` list)
2. **Player Team**: Use the player's **skip stats** (`cm.cStats` for player character who is always the skip)

---

## Implementation Details

### Team Player Positions
```csharp
team.players[0]  // Lead
team.players[1]  // Second
team.players[2]  // Third
team.players[3]  // Skip (T-line sweeper!)
```

### AI Team T-Line Sweeping
```csharp
if (aiTurn)
{
    // Find opponent team
    Team opponentTeam = GetOpponentTeam();
    
    // Regular sweepers use team average
    sweeperL.sweepStrength.SetBaseValue(aiStats + oppStats.sweepStrength);
    sweeperR.sweepStrength.SetBaseValue(aiStats + oppStats.sweepStrength);
    
    // T-line sweeper uses SKIP's individual stats
    if (opponentTeam.players.Count >= 4)
    {
        Player skipPlayer = opponentTeam.players[3];  // Skip is position 3
        sweeperT.sweepStrength.SetBaseValue(skipPlayer.sweepStrength + oppStats.sweepStrength);
        sweeperT.sweepEndurance.SetBaseValue(skipPlayer.sweepEnduro + oppStats.sweepEndurance);
        sweeperT.sweepCohesion.SetBaseValue(skipPlayer.sweepCohesion + oppStats.sweepCohesion);
        sweeperT.name = skipPlayer.name;  // Show skip's actual name
    }
}
```

### Player Team T-Line Sweeping
```csharp
else  // Player's turn
{
    // Who's shooting determines who sweeps down the ice (sweeperL/R)
    // But T-line sweeper is ALWAYS the skip
    
    if (rockCurrent > 11)  // Skip shooting
    {
        sweeperT.name = cm.activePlayers[2].name;  // Third sweeps T-line
        sweeperT.sweepStrength.SetBaseValue(cm.activePlayers[2].sweepStrength);
    }
    else  // Lead/Second/Third shooting
    {
        sweeperT.name = cm.playerName + " " + cm.teamName;  // Player (skip) sweeps T-line
        sweeperT.sweepStrength.SetBaseValue(cm.cStats.sweepStrength);  // Skip's stats
        sweeperT.sweepEndurance.SetBaseValue(cm.cStats.sweepEndurance);
        sweeperT.sweepCohesion.SetBaseValue(cm.cStats.sweepCohesion);
    }
}
```

---

## Rock Position Logic

| Rock # | Shooter | Regular Sweepers | T-Line Sweeper |
|--------|---------|------------------|----------------|
| 1-4    | Lead    | Second + Third   | **Skip** (player) |
| 5-8    | Second  | Third + Lead     | **Skip** (player) |
| 9-12   | Third   | Lead + Second    | **Skip** (player) |
| 13-16  | Skip    | Lead + Second    | **Third** |

### Key Insight
- When **Lead/Second/Third** shoot ? **Skip** sweeps behind T-line ?
- When **Skip** shoots ? **Third** sweeps behind T-line (skip is shooting)

---

## Stat Modifiers Applied

All sweeper stats get bonuses from:
1. **Base Player Stats** - Individual player's sweep strength/endurance/cohesion
2. **Equipment Modifiers** - From `gsp.oppStats` (opponent) or player equipment
3. **Career Progression** - `aiStats` baseline increases with week progression

**Formula**:
```csharp
finalStat = playerBaseStat + equipmentModifier
```

---

## Debug Logging

Added logging to confirm skip stats are being used:
```
[TeamManager] AI Skip sweeper: John Smith (Strength: 65, Endurance: 70)
[TeamManager] Player Skip T-line sweeper: Zack Thompson (Strength: 45, Endurance: 50)
```

---

## Testing Checklist

- ? **AI shoots ? Player sweeps behind T-line** ? Uses player's skip stats
- ? **Player shoots ? AI sweeps behind T-line** ? Uses AI team's skip stats
- ? **Skip is shooting** ? Third sweeps T-line (not skip)
- ? **Sweeper names display correctly** ? Shows actual skip name
- ? **Stats are realistic** ? Skip stats vary by team strength

---

## Gameplay Impact

### Before Fix
```
AI Team Average: 60 sweep strength
T-line sweeper: 60 strength (generic)
```

### After Fix
```
AI Team Average: 60 sweep strength
AI Skip: 75 strength (specialist!)
T-line sweeper: 75 strength ?
```

**Result**: Stronger teams will have **better skips** who can sweep opponent rocks more effectively behind the T-line. This adds:
- ? **Team differentiation** - Elite skips make a difference
- ? **Strategic depth** - Good sweeping can save/remove opponent rocks
- ? **Realism** - Matches real curling where skip controls T-line sweeping

---

## Code Changes

### Files Modified
1. `Assets/Scripts/TeamManager.cs` - `SetSweepers()` method

### Changes Summary
- **AI Turn Block** (lines ~100-125):
  - Added logic to extract skip player from opponent team
  - Set `sweeperT` stats to skip's individual stats
  - Added skip name display
  - Added fallback to team average if skip not found

- **Player Turn Block** (lines ~127-185):
  - Added comments explaining skip sweeping logic
  - Confirmed `sweeperT` always uses skip stats (except when skip is shooting)
  - Added debug logging for player skip stats

---

## Future Enhancements

### 1. Visual Indicator
Show skip's name above T-line sweeper in game:
```csharp
sweeperT.nameDisplay.text = skipPlayer.name;
```

### 2. Skip Specialization
Add bonus for skips with high sweep stats:
```csharp
if (isSkip)
    sweeperT.sweepStrength += 5;  // Skip bonus
```

### 3. Fatigue System
Skip gets tired faster when sweeping behind T-line:
```csharp
sweeperT.sweepEndurance -= fatigueRate * 1.5f;  // Skip tires faster
```

---

## Related Systems

**Connected To**:
- `SweeperManager.cs` - Activates T-line sweepers when rock crosses Y=6.5
- `SweeperSelector.cs` - Detects opponent rocks behind T-line
- `CharacterStats.cs` - Stores individual player sweep stats
- `Team.cs` - Contains player roster (`players[]` array)
- `CareerManager.cs` - Provides player/team data

**Data Flow**:
```
CareerManager.currentTournyTeams
?
TeamManager.SetSweepers()
?
Extract team.players[3] (skip)
?
Apply skip stats to sweeperT
?
SweeperManager activates sweeper
?
Player sees skip sweeping behind T-line
```

---

## Curling Realism Notes

**Why Skip Sweeps Behind T-Line**:
- Skip has **best strategic view** from house
- Skip **calls the game** and makes sweep decisions
- Skip is typically the **most experienced player**
- **Third player** can also sweep when skip is shooting

**Our Implementation**:
? Skip sweeps opponent rocks behind T-line (when not shooting)
? Skip's individual stats used (not team average)
? Third sweeps when skip is shooting (realistic substitution)

---

**Implementation Complete!** ??
T-line sweepers now use realistic skip stats, adding depth and team differentiation to the game!
