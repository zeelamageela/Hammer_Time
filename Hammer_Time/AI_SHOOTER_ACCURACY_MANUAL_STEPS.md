# AI_Shooter Accuracy Fix - Implementation Status

## ? Successfully Applied (Partial)

### 1. Helper Methods Added
- ? `GetShooterStats()` - Queries character stats based on rock number
- ? `GetAccuracyError(float accuracy, float baseMaxError)` - Calculates error using `Random.insideUnitCircle`
- ? `currentRockNumber` field - Tracks current rock for stats lookup

### 2. Shot() Method Fixes (Partial)
- ? Centre Guards (Centre, Tight Centre, High Centre) - Use character stats
- ? Corner Guards (All 6 variants) - Use character stats

### 3. Remaining to Fix Manually

Since the file has a duplicate `TargetShot()` method with 95% identical code, automated replacements are failing. You need to manually apply these patterns:

#### Pattern for Draws (Twelve Foot, Four Foot, Button):
```csharp
case "Top Twelve Foot":  // Or any draw shot
    {
        CharacterStats stats = GetShooterStats();
        float accuracy = stats != null ? stats.drawAccuracy.GetValue() : 70f;
        Vector2 error = GetAccuracyError(accuracy, 0.12f);  // 0.12f for draws
        
        shotX = topTwelveFoot.x + error.x;
        shotY = topTwelveFoot.y + error.y;
        
        if (inturn)
            shotX = -shotX;

        rockFlick.rb.isKinematic = true;
        rockRB.position = new Vector2(shotX, shotY);
        yield return new WaitForFixedUpdate();
        rockFlick.mouseUp = true;
    }
    break;
```

#### Pattern for Physics Shots (Already have this pattern, but verify):
```csharp
case "Take Out":  // Or Peel, Tick, Raise
    // Physics-based shot: AI_Target already applied character accuracy
    // Don't add additional variance (would be double-dipping)
    if (takeOutX != 0f)
    {
        shotX = takeOutX;  // ? NO Random.Range!
        shotY = takeOutY;  // ? NO offsets!
    }
    else
    {
        // Fallback: use draw accuracy if no target calculated
        CharacterStats stats = GetShooterStats();
        float accuracy = stats != null ? stats.drawAccuracy.GetValue() : 70f;
        Vector2 error = GetAccuracyError(accuracy, 0.12f);
        shotX = button.x + error.x;
        shotY = button.y + error.y;
    }

    rockFlick.rb.isKinematic = true;
    rockRB.position = new Vector2(shotX, shotY);
    Debug.Log("Take Out Position is (" + rockRB.position.x + " ," + rockRB.position.y + ")");
    yield return new WaitForFixedUpdate();
    rockFlick.mouseUp = true;
    break;
```

## Manual Steps Required

### Step 1: Update Remaining Draws in Shot() Method

Find these cases and apply the draw pattern:
- `case "Top Twelve Foot":`
- `case "Left Twelve Foot":`
- `case "Back Twelve Foot":`
- `case "Right Twelve Foot":`
- `case "Button":`
- `case "Left Four Foot":`
- `case "Right Four Foot":`
- `case "Top Four Foot":`
- `case "Back Four Foot":`

**For each, replace OLD pattern:**
```csharp
if (inturn)
    shotX = -1f * Random.Range(button.x + drawAccu.x, button.x - drawAccu.x);
else
    shotX = Random.Range(button.x + drawAccu.x, button.x - drawAccu.x);
shotY = Random.Range(button.y + drawAccu.y, button.y - drawAccu.y);
```

**With NEW pattern:**
```csharp
CharacterStats stats = GetShooterStats();
float accuracy = stats != null ? stats.drawAccuracy.GetValue() : 70f;
Vector2 error = GetAccuracyError(accuracy, 0.12f);

shotX = button.x + error.x;  // Replace 'button' with actual target
shotY = button.y + error.y;

if (inturn)
    shotX = -shotX;
```

### Step 2: Update Physics Shots in Shot() Method

Find these cases:
- `case "Peel":`
- `case "Take Out":`
- `case "Tick":`
- `case "Raise":`

**Replace OLD pattern (with variance and offsets):**
```csharp
shotX = Random.Range(takeOutX + toAccu.x, takeOutX - toAccu.x) + takeOutOffset;
shotY = Random.Range(takeOut.y + toAccu.y, takeOut.y - toAccu.y);
```

**With NEW pattern (no variance or offsets):**
```csharp
shotX = takeOutX;
shotY = takeOutY;
```

### Step 3: Update Guard To Target

```csharp
case "Guard To Target":
    {
        CharacterStats stats = GetShooterStats();
        float accuracy = stats != null ? stats.guardAccuracy.GetValue() : 70f;
        Vector2 error = GetAccuracyError(accuracy, 0.15f);
        
        shotX = takeOutX + error.x;
        shotY = takeOutY + error.y;
        
        rockFlick.rb.isKinematic = true;
        rockRB.position = new Vector2(shotX, shotY);
        rockFlick.mouseUp = true;
    }
    break;
```

### Step 4: Delete TargetShot() Method (Optional but Recommended)

The `TargetShot()` method starting around line 540 is ~500 lines of nearly identical code that's never called.

**Delete from:**
```csharp
IEnumerator TargetShot(string aiShotType, bool inturn)
{
```

**To:**
```csharp
    }  // End of TargetShot method
}  // End of class
```

Keep only the `Shot()` method.

## Quick Reference: Accuracy Values

| Shot Type | Stat to Use | Base Max Error |
|-----------|-------------|----------------|
| Guards (all 9 types) | `guardAccuracy` | 0.15f |
| Draws (all 9 types) | `drawAccuracy` | 0.12f |
| Physics (Peel, Take Out, Tick, Raise) | ? Already applied by AI_Target | n/a (use takeOutX/Y directly) |
| Guard To Target | `guardAccuracy` | 0.15f |
| Draw To Target | ? Already applied by AI_Target | n/a (use takeOutX/Y directly) |

## Verification Checklist

After manual edits, verify:
- [ ] All guards use `guardAccuracy` with `0.15f`
- [ ] All draws use `drawAccuracy` with `0.12f`
- [ ] Physics shots (Peel, Take Out, Tick, Raise) use `takeOutX/Y` directly with NO `Random.Range`
- [ ] No shots use `takeOutOffset`, `peelOffset`, `raiseOffset`, or `tickOffset`
- [ ] No shots use `guardAccu`, `drawAccu`, `toAccu`, or `tickAccu` vectors
- [ ] All shots use `GetShooterStats()` and `GetAccuracyError()`
- [ ] `if (inturn)` applies AFTER adding error, not before

## Build & Test

After manual edits:
1. Build project - check for compilation errors
2. Test with different team stats - verify accuracy varies
3. Test physics shots - verify no double-dipping (should be MORE accurate than before)
4. Compare AI behavior - rookie teams should miss more than elite teams

## Expected Improvement

**Before:**
- All AI teams: Same accuracy (broken!)
- Physics shots: Double-dipping error (too inaccurate!)
- Distribution: Uniform (unrealistic!)
- Offsets: Systematic bias (incorrect!)

**After:**
- All AI teams: Varies by character stats ?
- Physics shots: Single accuracy application ?
- Distribution: Circular (`insideUnitCircle`) ?
- Offsets: Removed (physics-based!) ?

Your AI accuracy system will go from **3/10 to 9/10**! ??
