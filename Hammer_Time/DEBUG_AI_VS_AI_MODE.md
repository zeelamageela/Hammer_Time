# AI vs AI Debug Mode

## Feature Overview
Press **W** during gameplay to instantly convert the current game into an AI vs AI match. This allows you to observe AI strategy, targeting, and turn selection without player input.

## How to Use

### Quick Start
1. Start any game (quick game, tournament, etc.)
2. Press **W** at any point during gameplay
3. Both teams are now controlled by AI
4. Watch the AI play out the rest of the game

### What Happens When You Press W

```csharp
[AIManager] W pressed - Starting AI vs AI game
[AIManager] AI vs AI mode enabled - Red: AI, Yellow: AI
[AIManager] Starting Red AI turn  // (or Yellow, depending on whose turn it is)
```

The system:
1. Sets `gm.aiTeamRed = true`
2. Sets `gm.aiTeamYellow = true`
3. Immediately triggers the appropriate AI turn based on current rock number

## Use Cases

### 1. Testing AI Takeout Targeting
**Purpose**: Observe if AI is hitting takeout targets with the correct turn direction

**What to watch**:
- Console logs: `[AI_Target] Take Out SUCCESS - InTurn: false, Target: (0.3, 6.5)`
- Rock trajectory matches the predicted curve
- Target rock gets hit (not just missed)

### 2. Testing AI Strategy
**Purpose**: See what shot types the AI chooses in different situations

**What to watch**:
- Which shots AI selects (draws, guards, takeouts, etc.)
- Turn direction choices (in-turn vs out-turn)
- Strategy decisions (aggressive vs conservative)

### 3. Testing Turn Synchronization
**Purpose**: Verify that turn graphic, trajectory, and rock curl all match

**What to watch**:
- Turn toggle graphic on rocks
- Trajectory preview direction
- Actual rock curl direction
- All three should **always** match

### 4. Observing Full Game Flow
**Purpose**: Watch a complete AI vs AI game to see strategy over multiple ends

**What to watch**:
- Scoring patterns
- Strategy changes based on score
- Hammer decisions
- Guard placement vs takeouts

## Debug Console Output

When W is pressed and AI takes a turn, you'll see logs like:

```
[AIManager] W pressed - Starting AI vs AI game
[AIManager] AI vs AI mode enabled - Red: AI, Yellow: AI
[AIManager] Starting Red AI turn
[AI_Strategy] OnShot called for rock 4
[AI_Target] Take Out SUCCESS - Score: 8.23, Pullback: (0.12, -27.5), InTurn: false, Target: (0.4, 6.8)
[AI_Shooter] Locked flipAxis = false for Take Out
Take Out Position is (0.14, -27.48)
```

This tells you:
- AI chose a takeout
- Physics calculation succeeded (not using fallback)
- Score was good (8.23 > 5 = direct hit expected)
- Turn direction: OUT-TURN (false)
- Target location and pullback position

## Other Debug Keys (Still Available)

| Key | Action | Description |
|-----|--------|-------------|
| **R** | Reset/Score | Triggers end-of-end scoring |
| **W** | **AI vs AI** | **Converts to AI vs AI game** |
| **A** | Test Shot | Executes `testing` shot type |
| **S** | Test Takeout | Executes `testingTakeOut` with `testingRockNumber` |
| **D** | Player Draw | Manual draw to target position |
| **F** | Auto Draw 4ft | AI draws to four foot |
| **G** | Auto Draw 12ft | AI draws to twelve foot |
| **H** | House Shot | Debug house shot (from Debug_Shooting) |
| **B** | Button Shot | Debug button shot |
| **G** | Guard Shot | Debug guard shot |
| **T** | Takeout Shot | Debug takeout shot |
| **C** | Custom Shot | Debug custom shot |

## Comparing AI vs AI vs Player vs AI

### Player vs AI
- You control one team
- AI controls the other
- You can see your trajectory preview
- Good for testing player experience

### AI vs AI (Press W)
- Both teams controlled by AI
- No trajectory preview shown (AI doesn't need it)
- Faster gameplay (no waiting for player)
- Good for testing AI strategy and targeting

## Tips for Effective Testing

### Test Takeout Accuracy
1. Set up a game with some rocks in the house
2. Press **W** to enable AI vs AI
3. Watch console for `[AI_Target] Take Out SUCCESS` logs
4. Check if **Score > 5** (indicates direct hit)
5. Verify the rock actually hits the target

### Test Turn Direction
1. Start AI vs AI game
2. Watch each takeout attempt
3. Note the `InTurn` value in the log
4. Verify the rock curls in the expected direction
   - `InTurn: false` ? Rock should curl **RIGHT** (out-turn)
   - `InTurn: true` ? Rock should curl **LEFT** (in-turn)

### Test Strategy Decisions
1. Start AI vs AI game from the beginning (rock 0)
2. Watch what shots AI chooses
3. Early rocks: Should place guards
4. Middle rocks: Mix of guards and draws
5. Late rocks (hammer): Takeouts or draws depending on score

## Expected AI Behavior

### Good AI Performance
- **90%+ physics success rate** - Rarely falls back to magic numbers
- **Scores > 5** for most takeout attempts
- **Correct turn direction** - Matches target position
- **Strategic variety** - Not just spamming one shot type

### Problems to Watch For
- **Frequent fallback**: `[AI_Target] Take Out physics FAILED`
- **Low scores**: `Score: 2.1` (far from target)
- **Wrong turn**: Rock curls opposite of what's needed
- **Repetitive**: Always choosing the same shot

## Troubleshooting

### "AI doesn't shoot after pressing W"
**Cause**: Turn might already be in progress
**Fix**: Wait for current rock to finish, then press W

### "Only one team is AI"
**Cause**: One team was already AI before pressing W
**Fix**: This is fine - W sets BOTH teams to AI regardless

### "Game crashes when pressing W"
**Cause**: Rare edge case during turn transition
**Fix**: Try pressing W at the start of a turn, not during rock motion

## Future Enhancements

Potential additions:
- **Auto-play mode**: Game continues AI vs AI without input
- **Speed control**: Fast-forward AI turns
- **AI difficulty toggle**: Switch between easy/medium/hard AI mid-game
- **Shot type forcing**: Force AI to use specific shot types for testing

## Documentation Updated
- Added W key handler to `AIManager.cs`
- Removed old takeout testing mode
- AI vs AI can be triggered at any time during gameplay
