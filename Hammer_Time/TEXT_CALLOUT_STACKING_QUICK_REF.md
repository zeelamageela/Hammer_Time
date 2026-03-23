# ?? TEXT CALLOUT STACKING - QUICK REFERENCE (UPDATED)

## ? **What It Does:**

**Automatically stacks callouts vertically when they spawn at the same location!**

**? NEW:** Now accounts for **full animation range** to prevent mid-animation collisions!

---

## ?? **Visual:**

### **Before Fix:**
```
  +100      ? Spawns, starts animating up
  Great!    ? Spawns at "safe" distance
  
  (0.2s later - COLLISION!)
  +100      ? Moved up during animation!
  Great!    (Too close now!)
```

### **After Fix:**
```
  +100      ? Reserves 0m ? 2m (full animation)
     ?
  Great!    ? Spawns at 2.8m (reserves 2.8m ? 4.8m)
  
  (Always 0.8m+ spacing throughout animation!)
```

---

## ?? **Settings (Inspector):**

```
TextCalloutManager:
?? Stack Spacing: 0.8          (Gap between stacks)
?? Stack Detection Range: 1.5  (Range to check)
?? Debug Stacking: ?           (Show logs)
```

---

## ?? **How It Works:**

1. New callout spawns at position P
2. System checks all active callouts
3. Finds nearby callouts (within 1.5m horizontal)
4. **NEW:** Finds highest **FINAL** Y position (after animation completes)
5. Spawns new callout at `highestFinalY + 0.8m`

**Result:** No overlap, even during animation! ?

---

## ?? **The Fix (v2.0):**

**Problem Found:** Callouts animate upward AFTER spawning, causing mid-animation collisions.

**Solution:** Track **final position** (start + floatDistance), not just current position.

```csharp
// OLD (WRONG):
float highestY = nearby.GetCurrentPosition().y;  // Moves!

// NEW (CORRECT):
float highestY = nearby.GetFinalPosition().y;    // Reserved space!
```

**Result:** Stacking system reserves FULL animation range (0m ? 2m), not just spawn point!

---

## ?? **Code Example:**

```csharp
// Spawn 3 callouts at same rock:
TextCalloutManager.Instance.ShowRockCallout(rock, "Hit!");
TextCalloutManager.Instance.ShowRockCallout(rock, "+50");
TextCalloutManager.Instance.ShowRockCallout(rock, "Combo!");

// Result: All 3 stack vertically automatically!
```

---

## ?? **Key Features:**

? **Automatic** - No code changes needed  
? **Smart** - Target-based + position-based detection  
? **Configurable** - Tune spacing in Inspector  
? **Performant** - Fast O(n) algorithm  
? **Debug mode** - See stacking in console  

---

## ?? **Customization:**

### **Tighter Stacking:**
```
stackSpacing = 0.5f
```

### **Looser Stacking:**
```
stackSpacing = 1.2f
```

### **Wider Detection:**
```
stackDetectionRange = 2.0f
```

---

## ?? **Result:**

**Never worry about overlapping callouts again!** 

The system handles everything automatically behind the scenes. Just spawn callouts normally - stacking works magically! ?

---

**Files Modified (v2.0):**
- ? `TextCalloutManager.cs` - Use GetFinalPosition() for stacking
- ? `TextCallout.cs` - Track finalWorldPosition, add GetFinalPosition()
- ? Build successful (0 errors)

**Full documentation:** 
- `TEXT_CALLOUT_STACKING_SYSTEM_COMPLETE.md` - Original implementation
- `TEXT_CALLOUT_STACKING_ANIMATION_FIX.md` - **NEW: Animation collision fix!**
