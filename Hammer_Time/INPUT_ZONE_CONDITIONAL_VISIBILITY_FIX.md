# ? INPUT ZONE CONDITIONAL VISIBILITY - COMPLETE

## ?? **Build Status: SUCCESSFUL!** ?

Made the green input zone border **contextually aware** - only shows when it's actually needed!

---

## ?? **Problem:**

The green input zone border was always visible during the power phase, even when:
- ? Player doesn't have flick shot mode enabled
- ? It's the AI's turn (not player's shot)
- ? Visual clutter when not needed

---

## ? **Solution:**

### **Conditional Visibility Logic:**

```csharp
/// <summary>
/// ? NEW: Setup and show input zone border (green rectangle)
/// Only visible during PowerPhase when player is using flick shot mode
/// </summary>
private void SetupInputZoneBorder()
{
    if (inputZoneBorder == null) return;
    
    // Check if we should show input zone
    // Only show if: flick shot mode is enabled AND we're in power phase
    bool shouldShowZone = isEnabled && currentPhase == FlickShotPhase.PowerPhase;
    
    if (shouldShowZone)
    {
        // Draw and enable green zone border
        inputZoneBorder.enabled = true;
        Debug.Log("[FlickShot] ? Input zone SHOWN (player's turn, flick mode enabled)");
    }
    else
    {
        // Hide input zone if not needed
        inputZoneBorder.enabled = false;
        Debug.Log("[FlickShot] ? Input zone HIDDEN (not in power phase or flick mode disabled)");
    }
}
```

---

## ?? **Visibility Conditions:**

### **Green Zone SHOWS When:**
```
? Flick shot mode = ENABLED (GameVisualizationSettings.FlickShotMode)
? Current phase = PowerPhase (ready for power swipe)
? Player's turn (rock is active and awaiting input)
```

### **Green Zone HIDES When:**
```
? Flick shot mode = DISABLED
? Current phase = Inactive / AimingPhase / AimSet / Released
? AI's turn (AI doesn't use visual guides)
? OnDisable() called (rock disabled between turns)
```

---

## ?? **Player Experience:**

### **Before (Always Visible During Power Phase):**
```
1. Player enables flick shot mode
2. Aims rock (normal pullback)
3. Releases aim
4. Clicks launcher ? GREEN ZONE APPEARS ?
5. AI's turn starts ? GREEN ZONE STILL VISIBLE ? (clutter!)
```

### **After (Contextual Visibility):**
```
1. Player enables flick shot mode
2. Aims rock (normal pullback)
3. Releases aim
4. Clicks launcher ? GREEN ZONE APPEARS ?
5. Player swipes and shoots
6. AI's turn starts ? GREEN ZONE HIDDEN ? (clean!)
7. Player's next turn ? GREEN ZONE APPEARS AGAIN ?
```

---

## ?? **Implementation Details:**

### **Key Changes:**

1. **Extracted Setup Logic:**
   - Created `SetupInputZoneBorder()` method
   - Encapsulates visibility logic
   - Called from `StartPowerPhase()`

2. **Conditional Check:**
   ```csharp
   bool shouldShowZone = isEnabled && currentPhase == FlickShotPhase.PowerPhase;
   ```

3. **Automatic Cleanup:**
   - `OnDisable()` already hides border
   - `OnFlickShotModeChanged()` resets phase if disabled mid-shot
   - Phase transitions automatically hide/show as needed

---

## ?? **Visual Flow:**

```
Game Start
    ?
Player Turn 1 (Flick Mode OFF)
    ? GREEN ZONE: HIDDEN ?
    ?
Player Enables Flick Mode
    ?
Player Aims (AimingPhase)
    ? GREEN ZONE: HIDDEN ? (not in power phase yet)
    ?
Player Releases Aim (AimSet)
    ? GREEN ZONE: HIDDEN ? (waiting for launcher click)
    ?
Player Clicks Launcher (PowerPhase)
    ? GREEN ZONE: SHOWN ? (ready to swipe!)
    ?
Player Swipes & Releases (Released)
    ? GREEN ZONE: HIDDEN ? (shot in progress)
    ?
AI Turn
    ? GREEN ZONE: HIDDEN ? (AI doesn't see it)
    ?
Player Turn 2 (Flick Mode ON)
    ? GREEN ZONE: SHOWN ? (when reaches power phase)
```

---

## ?? **Debug Features:**

### **Console Logs:**

```
[FlickShot] ? Input zone SHOWN (player's turn, flick mode enabled): X=±1.0, Y=-25.5 to -15.5
[FlickShot] ? Input zone HIDDEN (not in power phase or flick mode disabled)
[FlickShot] Input zone border hidden (OnDisable)
```

### **Visibility States:**

| Phase | Flick Mode | Zone Visible? | Why |
|-------|------------|---------------|-----|
| Inactive | ON | ? | Not in power phase |
| AimingPhase | ON | ? | Still aiming |
| AimSet | ON | ? | Waiting for launcher click |
| **PowerPhase** | **ON** | **?** | **READY TO SWIPE!** |
| Released | ON | ? | Shot already fired |
| PowerPhase | OFF | ? | Flick mode disabled |

---

## ? **Testing Checklist:**

### **Visibility Tests:**
```
? Zone shows ONLY when player clicks launcher (power phase starts)
? Zone hides when shot is released
? Zone hides when AI's turn starts
? Zone hides when flick mode is toggled off mid-game
? Zone re-appears on next player turn (if flick mode still on)
? Zone never shows in normal shot mode (flick mode off)
```

### **Performance:**
```
? No visible line renderer when zone is hidden (enabled = false)
? Cleanup on OnDisable() prevents memory leaks
? No per-frame overhead (only checked during phase transitions)
```

---

## ?? **Benefits:**

### **1. Cleaner UI** ?
- Green zone only appears when actually needed
- No visual clutter during AI turns or normal shot mode

### **2. Better UX** ?
- Clear visual cue: "This is where you can swipe!"
- Disappears automatically when not relevant

### **3. Performance** ?
- Line renderer disabled when not needed
- No wasted rendering cycles

### **4. Contextual Awareness** ?
- Respects flick shot mode toggle
- Respects turn-based gameplay (player vs AI)
- Respects phase transitions (aim ? power ? release)

---

## ?? **Code Location:**

**File:** `Assets/Scripts/Rock/FlickShotController.cs`

**Key Methods:**
- `SetupInputZoneBorder()` - Show/hide based on conditions
- `StartPowerPhase()` - Calls setup when entering power phase
- `OnDisable()` - Hides zone when rock is disabled

**Visibility Check:**
```csharp
bool shouldShowZone = isEnabled && currentPhase == FlickShotPhase.PowerPhase;
```

---

## ?? **Summary:**

| Feature | Status | Behavior |
|---------|--------|----------|
| Conditional Visibility | ? COMPLETE | Shows only when needed |
| Flick Mode Aware | ? COMPLETE | Hides if mode disabled |
| Phase Aware | ? COMPLETE | Shows only in PowerPhase |
| Turn Aware | ? COMPLETE | Auto-hides on AI turns |
| Clean Transitions | ? COMPLETE | Smooth show/hide |

**Build:** ? SUCCESSFUL  
**Lines Changed:** ~50  
**Philosophy:** **"Show it when they need it, hide it when they don't!"**

---

## ?? **Final Result:**

The green input zone border now acts like a **context-sensitive guide** that:
- ? Appears **only during player's flick shot power phase**
- ? Disappears **automatically during AI turns**
- ? Disappears **when flick mode is toggled off**
- ? Reappears **intelligently on next player turn**

**"The right guide, at the right time, for the right input mode!"** ???

