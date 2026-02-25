# Tee Line Sweeping Implementation

**Status**: ? **COMPLETE** - Player can now sweep opponent rocks behind the T-line!

---

## Feature Overview

In curling, once a rock crosses the **tee line (Y = 6.5)**, the **opposing team** can sweep it to influence its final position. This is a strategic tool to:
- Sweep opponent rocks out of the house
- Sweep opponent rocks further (to worse positions)
- Control the final outcome of opponent shots

---

## Implementation Details

### Architecture

**Existing Infrastructure** (was already in place):
- `tSweepParent` - GameObject parent for tee line sweepers
- `sweeperRedTee` / `sweeperYellowTee` - Opposing team sweepers (instantiated in `SetupSweepers`)
- `CheckForOpponentRocksBehindTLine()` - Auto-detection system in `SweeperSelector`

**New Additions**:
1. **State Tracking** in `SweeperManager`:
   - `isTeeSweeping` - Tracks if T-line sweeper is active
   - `activeTeeSweeper` - Reference to current T-line sweeper

2. **Core Methods**:
   - `ActivateTeeSweepers()` - Shows sweep button when opponent rock crosses T-line
   - `SweepTeeTap()` - Player sweeps opponent rock (uses skip's stats)
   - `SweepTeeWhoa()` - Stop sweeping opponent rock
   - `DeactivateTeeSweepers()` - Cleanup when rock stops

---

## How It Works

### 1. **Automatic Detection** (SweeperSelector.cs)
```csharp
CheckForOpponentRocksBehindTLine()  // Called every frame
?
Finds opponent rocks with Y > 6.5
?
Calls sm.ActivateTeeSweepers(rock, isRedRock)
```

### 2. **Player Interaction** (SweeperManager.cs)
```
Opponent rock crosses Y=6.5
?
Sweep button appears
?
Player taps sweep button OR taps sweeper collider
?
SweepTeeTap() executes
?
Single sweeper (skip) sweeps opponent rock
?
Player taps "Whoa" to stop
```

### 3. **Physics & Stats**
- **Single sweeper only** (curling rule: only 1 sweeper behind T-line)
- **Uses skip's stats** (`swprRTStats` or `swprYTStats`)
- **Skip's endurance** determines sweep duration: `duration = endurance * 0.02f`
- **Same sweep physics** as regular sweeping (`sweep.OnSweep()`)

---

## Code Changes

### SweeperManager.cs

**New Fields**:
```csharp
public bool isTeeSweeping;           // Track T-line sweep state
private SweeperParent activeTeeSweeper;  // Current T-line sweeper
```

**New Methods**:

#### `ActivateTeeSweepers(GameObject opponentRock, bool isRedRock)`
- Activates opposing team's T-line sweeper
- Red rock ? Yellow sweeper activates
- Yellow rock ? Red sweeper activates
- Shows sweep button for player

#### `SweepTeeTap()`
- Player taps to sweep opponent rock
- Uses skip's sweeping stats (single sweeper)
- Plays audio/haptics
- Animates sweeper
- Applies sweep physics

#### `SweepTeeWhoa()`
- Stops T-line sweeping
- Calls "Whoa"
- Stops audio/haptics

#### `DeactivateTeeSweepers()`
- Cleanup when rock stops
- Hides sweep buttons
- Deactivates T-line parent

**Updated Methods**:
- `ResetSweepers()` - Now cleans up T-line state

---

### SweeperSelector.cs

**Updated Methods**:

#### `ReAttachToRock(GameObject rock)`
- Simplified to use `sm.ActivateTeeSweepers()`
- Checks if rock is behind T-line (Y > 6.5)

#### `Update()`
- Added click detection for `sweeperTeeCol`
- Calls `sm.SweepTeeTap()` when player taps T-line sweeper

#### `CheckForOpponentRocksBehindTLine()`
- Auto-detects opponent rocks crossing T-line
- Calls `sm.ActivateTeeSweepers()` automatically
- Attaches to first opponent rock found

---

## User Experience Flow

### Player's Turn (Own Rock)
1. Player shoots rock
2. Rock travels down ice
3. **Player sweeps normally** (sweeperL + sweeperR)
4. Rock stops

### Opponent's Turn (AI Rock)
1. AI shoots rock
2. Rock crosses T-line (Y > 6.5)
3. **"SWEEP" button appears** ?
4. Player taps sweep button ? Single sweeper (skip) sweeps opponent rock
5. Player taps "WHOA" ? Stop sweeping
6. Rock continues/stops based on sweeping

---

## Strategic Gameplay

**When to Sweep Opponent Rocks**:
- ? Opponent rock heading to button ? Sweep it through the house
- ? Opponent rock slowing down in scoring position ? DON'T sweep (let it stop short)
- ? Opponent guard in front of house ? Sweep it deeper (less effective guard)

**Player Decision Making**:
- Sweep = Rock goes **further** and **straighter**
- No sweep = Rock **curls more** and **stops sooner**

---

## Technical Details

### Sweeper Stats Source
- **Regular sweeping** (own rocks): Uses lead/second stats (`swprLStats`, `swprRStats`)
- **T-line sweeping** (opponent rocks): Uses **skip stats** (`swprRTStats`, `swprYTStats`)

### Audio/Haptics
- ? Sweep sound plays (single sweeper = `rockSounds[0]` only)
- ? Haptic feedback loops during sweep
- ? "Sweep" and "Whoa" callouts

### Button States
| State | Sweep Button | Whoa Button | Hard Button |
|-------|-------------|-------------|-------------|
| Opponent rock crosses T-line | ? Active | ? Hidden | ? Hidden |
| Player sweeping | ? Hidden | ? Active | ? Hidden |
| Sweeping stopped | ? Active | ? Hidden | ? Hidden |
| Rock stopped | ? Hidden | ? Hidden | ? Hidden |

---

## Future Enhancements (Not Implemented Yet)

### AI T-Line Sweeping
- AI should decide when to sweep opponent rocks
- Strategy: Sweep rocks out of scoring position
- Implementation: Add to `AI_Sweeper.cs`

### Burn Indicator
- Show warning if player sweeps opponent rock BEFORE T-line (illegal)
- Display "BURNED ROCK" warning
- Rock is removed from play in real curling

### Hard Sweeping Behind T-Line
- Currently only "sweep" is available
- Could add "hard" button for aggressive T-line sweeping

---

## Testing Checklist

- ? **Opponent rock crosses T-line** ? Sweep button appears
- ? **Player taps sweep** ? Single sweeper activates
- ? **Audio plays** ? Sweeping sound active
- ? **Haptics work** ? Vibration during sweep
- ? **Physics apply** ? Rock travels further/straighter
- ? **Player taps whoa** ? Sweeping stops
- ? **Rock stops** ? T-line sweeper deactivates

---

## Code References

**Modified Files**:
1. `Assets/Scripts/Sweeping/SweeperManager.cs`
   - Added: `isTeeSweeping`, `activeTeeSweeper`
   - Added: `ActivateTeeSweepers()`, `SweepTeeTap()`, `SweepTeeWhoa()`, `DeactivateTeeSweepers()`
   - Updated: `ResetSweepers()`

2. `Assets/Scripts/Sweeping/SweeperSelector.cs`
   - Updated: `ReAttachToRock()` - Uses new activation method
   - Updated: `Update()` - Handles T-line sweeper clicks
   - Updated: `CheckForOpponentRocksBehindTLine()` - Simplified auto-detection

**Related Files**:
- `Assets/Scripts/Sweeping/SweeperParent.cs` - Sweeper animation/behavior
- `Assets/Scripts/Sweeping/Sweep.cs` - Sweep physics application
- `Assets/Scripts/Stats/CharacterStats.cs` - Skip stats for T-line sweeping

---

## Curling Rules Reference

**T-Line Sweeping Rules**:
- ? After rock crosses tee line (Y = 6.5), **opposing team may sweep**
- ? Only **ONE sweeper** allowed behind tee line
- ? Typically the **skip** sweeps (uses skip's stats)
- ? **Cannot sweep opponent rocks BEFORE tee line** (results in "burned rock")

---

**Implementation Complete!** ??
Players can now sweep opponent rocks behind the T-line, adding strategic depth to the game!
