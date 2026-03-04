# Quick Test Mode Persistence Fix

## Problem
Quick Test Mode was activating on **every game** after pressing Q once, because the PlayerPrefs flags were never cleared.

## Root Cause
When you press Q to start a Quick Test Game:
```csharp
PlayerPrefs.SetInt("QuickTestMode", 1);
PlayerPrefs.SetInt("DisableSweeping", 1);
PlayerPrefs.Save();
```

These flags persisted forever until manually cleared, so every subsequent game would also be in Quick Test Mode with locked physics multipliers.

## Solution (2-Part Fix)

### Fix 1: Stricter Quick Test Mode Check in Rock_Force
**File:** `Assets/Scripts/Rock/Rock_Force.cs`

**Before:**
```csharp
bool isQuickTestMode = PlayerPrefs.GetInt("QuickTestMode", 0) == 1;
```

**After:**
```csharp
GameSettingsPersist gsp = FindFirstObjectByType<GameSettingsPersist>();
bool isQuickTestMode = PlayerPrefs.GetInt("QuickTestMode", 0) == 1 && (gsp != null && gsp.debug);
```

**Why:** Now Quick Test Mode only activates if **BOTH conditions are true:**
1. ? PlayerPref flag is set (by pressing Q)
2. ? `gsp.debug = true` (set by QuickTestGame.cs)

This means even if PlayerPrefs aren't cleared, normal games won't trigger Quick Test Mode.

### Fix 2: Auto-Clear Flags When QuickTestGame Component Destroyed
**File:** `Assets/Scripts/QuickTestGame.cs`

**Added:**
```csharp
public static void ClearQuickTestMode()
{
    PlayerPrefs.SetInt("QuickTestMode", 0);
    PlayerPrefs.SetInt("DisableSweeping", 0);
    PlayerPrefs.Save();
    Debug.Log("[QuickTestGame] Quick Test Mode flags CLEARED - normal game mode restored");
}

private void OnDestroy()
{
    // CRITICAL: Clear flags when this component is destroyed (returning to menu)
    ClearQuickTestMode();
}
```

**Why:** When you return to the main menu (which destroys the QuickTestGame component), the flags are automatically cleared. This ensures a clean slate for the next game.

## How It Works Now

### Scenario 1: Press Q (Quick Test Game)
1. Press Q in menu
2. `QuickTestGame.StartQuickTestGame()` runs:
   - Sets `PlayerPrefs: QuickTestMode = 1`
   - Sets `gsp.debug = true`
3. Game loads ? `Rock_Force.Awake()` checks:
   - ? QuickTestMode = 1
   - ? gsp.debug = true
   - **Result:** Quick Test Mode ENABLED ?
4. Game ends ? QuickTestGame destroyed ? Flags CLEARED

### Scenario 2: Start Normal Game
1. Load game normally (career, tournament, etc.)
2. `gsp.debug = false` (default)
3. Game loads ? `Rock_Force.Awake()` checks:
   - ? gsp.debug = false
   - **Result:** Quick Test Mode DISABLED ? (even if PlayerPrefs still set)

### Scenario 3: Multiple Quick Tests
1. Press Q ? Quick test game 1 ? Return to menu ? Flags cleared
2. Press Q again ? Quick test game 2 ? Works perfectly ?

## Testing Checklist

- [ ] Press Q in menu ? Start Quick Test ? See "QUICK TEST MODE" log
- [ ] Finish game ? Return to menu
- [ ] Start normal career game ? Should NOT see "QUICK TEST MODE" log
- [ ] Start tournament game ? Should NOT see "QUICK TEST MODE" log
- [ ] Press Q multiple times ? Each test should work independently

## Log Verification

### Quick Test Mode Active (Correct):
```
[QuickTestGame] Quick Test Mode ACTIVE (gsp.debug=true)
[Rock_Force] ?? QUICK TEST MODE: Physics multipliers LOCKED to 1.0 (perfect determinism)
```

### Normal Game (Correct):
```
[TrajectoryLine] Physics initialized - trajectory will use tuned damping ratio
(No Quick Test Mode logs)
```

### Bug Fixed (Was Happening):
```
[TrajectoryLine] Physics initialized
[Rock_Force] ? QUICK TEST MODE: Physics multipliers LOCKED  ? WRONG! (normal game)
```

## Summary
? Quick Test Mode now ONLY activates when:
- You explicitly press Q to start a test game
- `gsp.debug = true` (set by QuickTestGame.cs)

? Normal games are unaffected even if PlayerPrefs linger

? Flags auto-clear when returning to menu

? No more accidental Quick Test Mode in career/tournament games!
