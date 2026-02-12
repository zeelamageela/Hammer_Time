# Save/Load System Comprehensive Fix

## Problems Identified

### 1. GameManager Save/Load Issues
- **SetupGame()**: Sets `gameInProgress = true` even when loading, overriding loaded state
- **LoadGame()**: Doesn't properly use rockPos/rockInPlay from GSP
- **Score array**: Not initialized properly in multiple places
- **LoadGame()**: Never actually called correctly from SetupGame flow

### 2. CareerManager Save Issues  
- **SaveCareer()**: gameInProgress flag properly captured BUT...
- **rockPos/rockInPlay**: These are set in GM.NextTurn() but may not persist properly

### 3. GameSettingsPersist Flag Management
- **TournySetup()**: Clears gameInProgress when starting new game ? (correct)
- **LoadFromGM()**: Doesn't manage gameInProgress flag
- **AutoSave()**: Delegates to CareerManager (correct)
- **LoadTourny()**: Loads career but doesn't properly restore flags

### 4. CareerSettings Load Flow
- **LoadToCM()**: Checks flags but logic is incomplete
- Week 0 check doesn't account for mid-tournament saves

## Root Cause Analysis

The save/load system has multiple layers that don't synchronize properly:

1. **GameManager** stores game state (rocks, scores, current turn)
2. **GameSettingsPersist** acts as middleware, transferring data between scenes
3. **CareerManager** persists everything to disk via CareerSaveService

The issue is that **data flows correctly into GSP** (from GM.NextTurn ? GSP.rockPos/rockInPlay)
but **loading doesn't reverse this flow** properly.

## The Fix

### Phase 1: GameManager - Proper Flag Handling and Load Logic

#### Fix 1: SetupGame() - Don't Override Loaded Flags
```csharp
// In SetupGame(), AFTER loading GSP:
if (!gsp.loadGame)
{
    gsp.gameInProgress = true;
    Debug.Log("[GameManager] NEW game - set gameInProgress = true");
}
else
{
    // When loading, trust the loaded gameInProgress flag
    Debug.Log("[GameManager] LOADING game - gameInProgress already set by load");
}
```

#### Fix 2: Initialize Score Array Safely
```csharp
// In SetupGame(), after loading ends:
if (gsp.score == null || gsp.score.Length < endTotal + 1)
{
    gsp.score = new Vector2Int[endTotal + 1];
}
```

#### Fix 3: Actually Use LoadGame Path
The current code has a LoadGame() method but never properly calls it! The flow should be:
```csharp
if (gsp.loadGame)
{
    StartCoroutine(LoadGame());  // This exists but isn't reached properly!
}
```

#### Fix 4: PlaceRocks() - Use Loaded Data
The PlaceRocks() method exists and looks correct - it uses `gsp.rockPos` and `gsp.rockInPlay`.
The problem is it's only called from LoadGame(), which isn't being reached.

###  Phase 2: CareerSettings - Correct Load Routing

#### Fix: LoadToCM() Logic
```csharp
public void LoadToCM()
{
    cm.LoadSettings();
    
    // CRITICAL: Check flags in correct order
    // 1. If we have a game in progress (mid-game save), load directly into game
    if (gsp.gameInProgress)
    {
        Debug.Log("[CareerSettings] Mid-game save ? TournyGame");
        gsp.loadGame = true;
        SceneManager.LoadScene("TournyGame");
    }
    // 2. If tournament in progress but no game, load tournament home
    else if (gsp.tournyInProgress)
    {
        Debug.Log("[CareerSettings] Tournament in progress ? Tournament Home");
        gsp.loadGame = false; // No game to load, just tournament state
        
        if (gsp.KO3)
            SceneManager.LoadScene("Tourny_Home_3K");
        else if (gsp.KO1)
            SceneManager.LoadScene("Tourny_Home_SingleK");
        else
            SceneManager.LoadScene("Tourny_Home_1");
    }
    // 3. Otherwise, normal arena selector
    else
    {
        Debug.Log("[CareerSettings] Normal career ? Arena Selector");
        gsp.loadGame = false;
        SceneManager.LoadScene("Arena_Selector");
    }
}
```

### Phase 3: GameManager End States - Clear Flags

#### Fix: When End Finishes
```csharp
// In Scoring(), when end finishes but game continues:
if (endCurrent < endTotal)
{
    // Keep gameInProgress = true for next end
    gsp.gameInProgress = true;
    SaveGame();
    SceneManager.LoadScene("End_Menu_Tourny_1");
}
```

#### Fix: When Game Ends
```csharp
// In EndOfGame():
gsp.loadGame = false;
gsp.gameInProgress = false;
Debug.Log("[GameManager] Game ended - cleared flags");
SaveGame();
```

### Phase 4: CareerSettings - New Career

#### Fix: Clear All Flags on New Career
```csharp
public void New()
{
    // Delete save first
    if (cm.SaveFileExists())
    {
        cm.DeleteCareerSave();
    }
    
    // Clear ALL game state flags
    gsp.careerLoad = false;
    gsp.loadGame = false;
    gsp.gameInProgress = false;
    gsp.tournyInProgress = false;
    gsp.draw = 0;
    gsp.playoffRound = 0;
    gsp.rockCurrent = 0;
    gsp.endCurrent = 0;
    
    // ... rest of new career setup
}
```

## Implementation Order

1. ? GameManager.cs - SetupGame flag handling
2. ? GameManager.cs - Score array initialization
3. ? GameManager.cs - Scoring/EndOfGame flag clearing
4. ? CareerSettings.cs - LoadToCM routing logic
5. ? CareerSettings.cs - New() flag clearing
6. ? GameSettingsPersist.cs - TournySetup flag clearing (already has this!)

## Testing Checklist

After fixes:
- [ ] Start new career ? plays first game normally
- [ ] Save mid-game (during turn) ? close ? reopen ? continues from exact position
- [ ] Save between ends ? close ? reopen ? loads to correct end
- [ ] Save between games in tournament ? close ? reopen ? loads to tournament home
- [ ] Finish game ? loads to end menu with correct scores
- [ ] Finish tournament ? clears tournament flags
- [ ] Start new career after completing one ? no leftover state

## Key Insights

The save system DOES work - CareerManager captures everything correctly.
The load system DOESN'T work - the data comes back but isn't applied properly.

The fix is primarily about:
1. Not overwriting loaded flags with defaults
2. Actually using the loaded data (PlaceRocks path)
3. Routing to the correct scene based on what was saved
4. Clearing flags at the right times
