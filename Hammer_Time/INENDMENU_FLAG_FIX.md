# inEndMenu Flag Bug Fix - April 14, 2026

## CRITICAL: Career Settings Screen Must Always Show

**Design Decision**: The Career Settings screen MUST always be displayed when loading a career, even if flags indicate we should route directly to a game/end menu scene. This screen serves as the "escape hatch" for corrupted saves - players can always choose to start a new career if loading fails.

## Problem Description

**User Report**: "When we quit in the middle of a game (from TournyGame), we load to the End Menu and then get stuck when we start the tourny game."

## Root Cause

The `inEndMenu` flag was not being cleared when continuing from the End Menu back to gameplay. This caused the following broken flow:

### Broken Flow
1. Player finishes End 3 (out of 8) in TournyGame
2. GameManager loads End_Menu_Tourny_1 to show results (normal behavior)
3. EndMenu.Start() sets `inEndMenu = true` and saves (correct)
4. Player quits while viewing results
5. **On reload**: Correctly loads back to End_Menu_Tourny_1 ✓
6. **Player clicks Continue()**: Loads TournyGame BUT doesn't clear `inEndMenu` flag ❌
7. **Auto-save triggers**: Saves with `gameInProgress = true` and `inEndMenu = true` (WRONG!)
8. **Next reload**: Routes to End_Menu instead of TournyGame ❌
9. **Stuck in loop**: Every reload goes to End Menu, every continue re-saves with wrong flags

## The Fix

### Critical Design Principle
**Career Settings screen ALWAYS shows** - even when loading from Main Menu Continue button. This ensures:
- Players can start a new career if save is corrupted
- Players can see their career stats before continuing
- No "auto-skip" behavior that could trap players in broken states

### Files Modified

1. **CareerSettings.cs** - `Start()` method
   - **REMOVED**: Auto-skip behavior when `gsp.careerLoad` is true
   - **NOW**: Always shows Career Settings UI with Continue/New options
   - **WHY**: Gives players escape hatch for corrupted saves
   
2. **EndMenu.cs** - `Continue()` method
   - Added: `gsp.inEndMenu = false;` before loading TournyGame
   - Ensures flag is cleared when player continues to next end
   
3. **EndMenu.cs** - `SimEnd()` method  
   - Added: `gsp.inEndMenu = false;` when simulating end
   - Ensures flag is cleared when simulating gameplay
   
4. **GameManager.cs** - `Start()` method (REVERTED)
   - Initially added safety check to clear `inEndMenu`
   - **Removed** because it was too aggressive and cleared flag before End Menu could load
   - Not needed - EndMenu.Start() and Continue() handle flag lifecycle properly

### Code Changes

```csharp
// CareerSettings.cs - Start() - REMOVED AUTO-SKIP
// Before (BAD - trapped players in corrupted saves):
if (cm.SaveFileExists() && gsp.careerLoad)
{
    Debug.Log("[CareerSettings] Auto-loading saved career...");
    LoadToCM();  // Skip UI completely!
    return;
}

// After (GOOD - always show UI):
cm.LoadCareer();
Debug.Log("[CareerSettings] Career loaded - showing UI with Continue option");
Player(!cm.gameOver);  // Always show Career Settings screen

// EndMenu.cs - Continue()
public void Continue()
{
    CareerManager cm = FindFirstObjectByType<CareerManager>();
    gsp = FindFirstObjectByType<GameSettingsPersist>();

    // CRITICAL: Clear inEndMenu flag when continuing to next end
    gsp.inEndMenu = false;
    Debug.Log("[EndMenu] Continue - cleared inEndMenu flag, loading TournyGame");

    SceneManager.LoadScene("TournyGame");
}

// EndMenu.cs - SimEnd()
public void SimEnd()
{
    Debug.Log("[EndMenu] SimEnd called - simulating to end of game");
    gsp.endCurrent++;
    gsp.gameInProgress = true;
    gsp.inEndMenu = false;  // CRITICAL: Clear flag when simulating
    // ... rest of method
}
```

## Fixed Flow

1. Player finishes End 3 in TournyGame
2. GameManager loads End_Menu_Tourny_1 to show results
3. EndMenu.Start() sets `inEndMenu = true` and saves ✓
4. Player quits while viewing results
5. **On reload**: Player navigates to Career_Menu (via Main Menu or direct navigation)
6. **Career Settings loads**: Shows UI with player name, stats, Continue/New buttons ✓
7. **Player can choose**:
   - **Continue**: Calls LoadToCM() → Checks flags → Routes to End_Menu_Tourny_1 ✓
   - **New**: Deletes save and starts fresh career ✓
8. **End Menu loads**: Shows saved scores (Red: 2, Yellow: 0) ✓
9. **Player clicks Continue**: Clears `inEndMenu = false`, loads TournyGame ✓  
10. **Auto-save triggers**: Saves with `gameInProgress = true`, `inEndMenu = false` ✓
11. **Next reload**: Career Settings → Continue → TournyGame ✓

## Flag State Reference

| Scenario | gameInProgress | inEndMenu | Flow |
|----------|---------------|-----------|------|
| Mid-end (throwing rocks) | true | false | Career Settings → Continue → TournyGame |
| Between ends (viewing results) | false | true | Career Settings → Continue → End_Menu → Continue → TournyGame |
| Game over (final results) | false | true | Career Settings → Continue → End_Menu → End Game → Tournament Home |
| Between games (at tourney home) | false | false | Career Settings → Continue → Tourny_Home |
| New game start | false | false | Career Settings → Start → Arena Selector |

## Testing Recommendations

### Test Case 1: Quit Between Ends
1. Start tournament game
2. Play through end 1 completely
3. View End Menu results (auto-loaded)
4. **Force quit app** (don't click any buttons)
5. Relaunch game, navigate to Career_Menu
6. **Expected**: Shows Career Settings screen with player stats ✓
7. Click Continue button
8. **Expected**: Loads to End_Menu_Tourny_1 showing results (Red: 2, Yellow: 0) ✓
9. Click Continue
10. **Expected**: Loads to TournyGame for end 2 ✓
11. Let auto-save trigger (wait 30+ seconds)
12. Force quit again
13. Relaunch, go to Career_Menu → Continue
14. **Expected**: Shows Career Settings → Continue → TournyGame ✓

### Test Case 2: Quit Mid-End
1. Start tournament game
2. Play halfway through end 1 (throw 4-5 rocks)
3. **Force quit app**
4. Relaunch, navigate to Career_Menu
5. **Expected**: Shows Career Settings screen ✓
6. Click Continue
7. **Expected**: Loads directly to TournyGame mid-end ✓

### Test Case 3: Complete Game
1. Play full 8-end game
2. View final End Menu results
3. Click "End Game" button
4. **Expected**: Returns to tournament home, flags cleared ✓

### Test Case 4: Corrupted Save Recovery (NEW!)
1. Corrupt a save file or encounter loading error
2. Navigate to Career_Menu
3. **Expected**: Shows Career Settings screen ✓
4. **Can click "New"** to delete corrupted save and start fresh ✓
5. **This is the escape hatch!** ✓

## Related Files

- [EndMenu.cs](Assets/Scripts/EndMenu.cs) - Between-end results display
- [GameManager.cs](Assets/Scripts/GameManager.cs) - TournyGame scene controller
- [CareerSettings.cs](Assets/Scripts/Tourny/CareerSettings.cs) - Routing logic based on flags
- [GameSettingsPersist.cs](Assets/Scripts/GameSettingsPersist.cs) - Flag storage (DontDestroyOnLoad)

## Why This Happened

The `inEndMenu` flag was added recently to support proper save/load routing to the End Menu scene (previously, the system couldn't distinguish between "mid-game save" and "viewing end menu save"). The flag works correctly for its primary purpose, but we missed clearing it in the "continue to next end" code paths.

Additionally, an **auto-skip feature** was initially implemented that bypassed the Career Settings screen when `gsp.careerLoad` was true. This was removed because:
1. Players need a way to start a new career if saves are corrupted
2. Players should see their career stats before continuing
3. Auto-skipping created a trap where corrupted saves had no escape route

## Prevention

Going forward, when adding new state flags or UI flows:
1. Document ALL entry/exit points clearly
2. Ensure flags are cleared in ALL transitions out of that state
3. Test save/quit/load at EVERY state transition
4. Consider adding runtime validation (assert flag values match current scene)
5. **Never auto-skip UI screens that provide "escape hatches" for players**
6. Always provide a way for players to start fresh if something breaks
