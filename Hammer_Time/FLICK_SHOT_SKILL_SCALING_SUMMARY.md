# Flick Shot Skill-Based Tolerance Scaling - Implementation Summary

## ? COMPLETE - Skill-based tolerance now rewards character progression!

## What Was Implemented

Added **skill-based tolerance scaling** to the existing dynamic velocity window system. The tolerance window now adjusts based on the shooter's weight accuracy skill using an **INVERTED relationship** for realistic progression.

## The INVERTED Relationship

**Low Weight Skill** = Poor control = **WIDER tolerance** (easier!)  
**High Weight Skill** = Good control = **TIGHTER tolerance** (harder!)

This matches real curling:
- Beginners struggle with weight control ? get more help (forgiving window)
- Experts have precise control ? must demonstrate it (tight window)

## Example: Draw Shot (Target = 8.5 m/s)

### Beginner (Weight Skill = 20%)
```
Base tolerance: ±1.5 m/s
Scaling: Lerp(1.4, 0.7, 0.2) = 1.26x ? WIDER!
Scaled tolerance: ±1.89 m/s
Window: 6.61 - 10.39 m/s (3.78 m/s range)
Difficulty: EASY - large margin for error
```

### Intermediate (Weight Skill = 50%)
```
Base tolerance: ±1.5 m/s
Scaling: Lerp(1.4, 0.7, 0.5) = 1.05x
Scaled tolerance: ±1.58 m/s
Window: 6.92 - 10.08 m/s (3.16 m/s range)
Difficulty: MODERATE - balanced
```

### Expert (Weight Skill = 90%)
```
Base tolerance: ±1.5 m/s
Scaling: Lerp(1.4, 0.7, 0.9) = 0.77x ? TIGHTER!
Scaled tolerance: ±1.16 m/s
Window: 7.34 - 9.66 m/s (2.32 m/s range)
Difficulty: HARD - must be precise!
```

## Inspector Parameters

```csharp
[Header("Skill-Based Tolerance Scaling")]
[Tooltip("Enable skill-based tolerance (uses weight skill from CharacterStats)")]
public bool useSkillScaling = true; // Toggle on/off

[Tooltip("Tolerance multiplier at 0% weight skill (unskilled = WIDE tolerance, forgiving!)")]
[Range(1.0f, 2.0f)]
public float lowSkillScale = 1.4f; // Beginners: 140% of base

[Tooltip("Tolerance multiplier at 100% weight skill (skilled = TIGHT tolerance, precise!)")]
[Range(0.5f, 1.0f)]
public float highSkillScale = 0.7f; // Experts: 70% of base
```

## How It Works

### 1. Get Weight Skill
```csharp
float weightSkill = GetPlayerWeightSkill(); // From CareerManager.cStats or AI team
// Returns value 0-100
```

**Player Shots:**
- Gets `CareerManager.cStats.weightAccuracy`

**AI Shots:**
- Gets `TeamManager.redTeam.weight` or `yellowTeam.weight` depending on turn

### 2. Calculate Scaling Factor
```csharp
float normalizedSkill = weightSkill / 100f; // 0-1
float scalingFactor = Mathf.Lerp(lowSkillScale, highSkillScale, normalizedSkill);
// INVERTED: 0% ? 1.4x (wide), 100% ? 0.7x (tight)
```

### 3. Scale Tolerance
```csharp
float scaledTolerance = velocityTolerance * scalingFactor;
// e.g., 1.5 m/s × 1.26 = 1.89 m/s (beginner)
//   or  1.5 m/s × 0.77 = 1.16 m/s (expert)
```

### 4. Apply to Dynamic Window
```csharp
dynamicMinVelocity = targetRockVelocity - scaledTolerance;
dynamicMaxVelocity = targetRockVelocity + scaledTolerance;
// Window is now skill-scaled AND centered on target!
```

## Configuration Recommendations

### Easy Progression (Noob-Friendly)
```csharp
velocityTolerance = 1.8f;  // Wider base
lowSkillScale = 1.5f;       // Beginners: 150% (very forgiving!)
highSkillScale = 0.8f;      // Experts: 80% (still moderate)
useSkillScaling = true;
```

### Moderate Progression (Balanced) ? RECOMMENDED
```csharp
velocityTolerance = 1.5f;  // Standard base
lowSkillScale = 1.4f;       // Beginners: 140% (forgiving)
highSkillScale = 0.7f;      // Experts: 70% (challenging)
useSkillScaling = true;
```

### Harsh Progression (Skill-Based)
```csharp
velocityTolerance = 1.2f;  // Tighter base
lowSkillScale = 1.3f;       // Beginners: 130% (some help)
highSkillScale = 0.6f;      // Experts: 60% (very tight!)
useSkillScaling = true;
```

### Disable Skill Scaling (Testing/Debug)
```csharp
useSkillScaling = false;
// All players use base velocityTolerance (1.5 m/s)
```

## Debug Logging

When power phase starts, you'll see:
```
[FlickShot Skill] === SKILL-BASED TOLERANCE ===
  Weight skill: 75.0% (normalized: 0.750)
  Scaling factor: 0.88x (INVERTED: low skill = wider)
  Base tolerance: ±1.50 m/s
  Scaled tolerance: ±1.31 m/s
  Skill range: 1.40x (0%) to 0.70x (100%)

[FlickShot] ?? DYNAMIC VELOCITY WINDOW (SKILL-SCALED):
  Target velocity: 8.50 m/s
  Base tolerance: ±1.50 m/s
  Scaled tolerance: ±1.31 m/s (skill-adjusted!)
  Min velocity: 7.19 m/s (target - scaled tolerance)
  Max velocity: 9.81 m/s (target + scaled tolerance)
  Window size: 2.62 m/s
```

## Benefits

### ? RPG Progression
- Weight skill now directly affects flick shot difficulty
- Players feel character growth as they level up
- Equipment that boosts weight becomes more valuable

### ? Realistic Skill Curve
- Beginners get help (wide tolerance)
- Experts must demonstrate precision (tight tolerance)
- Matches real curling skill progression

### ? AI Balance
- AI difficulty automatically scales with their stats
- Weak AI teams have wider tolerance (less accurate)
- Strong AI teams have tighter tolerance (more accurate)

### ? Configurable Difficulty
- Easy mode: High tolerance scales (1.5x/0.8x)
- Hard mode: Low tolerance scales (1.3x/0.6x)
- Can disable for testing

### ? Consistent Per-Shot Difficulty
- Dynamic window still centers on target
- All shots equally challenging *within* skill level
- Skill level determines overall difficulty

## Testing Guide

### Test 1: Beginner Player
1. Set player weight skill to 20 in Inspector (or via save file)
2. Aim at house center (target ~8.5 m/s)
3. Watch logs: Should show ~1.9 m/s tolerance (wide!)
4. Shot should feel easier to hit target

### Test 2: Expert Player
1. Set player weight skill to 90
2. Aim at same target
3. Watch logs: Should show ~1.2 m/s tolerance (tight!)
4. Shot should feel harder, require more precision

### Test 3: Skill Progression
1. Start with low weight (20)
2. Play several shots, note difficulty
3. Level up weight to 50
4. Play same shots - should feel slightly harder
5. Level up to 90 - should feel challenging!

### Test 4: AI Shots
1. Play vs weak AI team (low weight stat)
2. Watch AI shot logs - should show wide tolerance
3. Play vs strong AI team (high weight stat)
4. Watch logs - should show tight tolerance
5. Strong AI should hit targets more consistently

### Test 5: Toggle Off
1. Set `useSkillScaling = false` in Inspector
2. Test shots with different weight skills
3. All should use base tolerance (1.5 m/s)
4. No skill-based difference

## Implementation Files

### Modified
- `Assets\Scripts\Rock\FlickShotController.cs`
  - Added `useSkillScaling`, `lowSkillScale`, `highSkillScale` parameters
  - Added `CalculateSkillScaledTolerance()` method
  - Added `GetPlayerWeightSkill()` method
  - Added `GetAITeamWeightSkill()` method
  - Updated `StartPowerPhase()` to use skill-scaled tolerance
  - Enhanced debug logging

### Related Systems
- `CareerManager.cStats.weightAccuracy` - Player weight skill (0-100)
- `TeamManager.redTeam.weight` / `yellowTeam.weight` - AI weight skill (0-100)
- `Team.weight` - Team stat used for AI shots
- Dynamic velocity window (already implemented)

## Backward Compatibility

? **Fully backward compatible**:
- `useSkillScaling` defaults to `true` (enabled)
- Can disable by setting to `false`
- Old saves work (skill data already exists)
- No breaking changes to existing systems

## Future Enhancements

### Possible Additions
1. **Visual Feedback**: Show skill level on HUD during power phase
2. **Difficulty Indicator**: Color-code velocity guide based on skill (red = expert/hard, green = beginner/easy)
3. **Skill Curves**: Non-linear scaling (e.g., quadratic) for more dramatic differences
4. **Other Skills**: Apply finesse to curl, aim to direction precision
5. **Dynamic Skill Adjustment**: AI adapts difficulty based on player's actual performance

### Performance Notes
- Skill lookup happens once per shot (minimal overhead)
- No per-frame calculations
- Debug logging can be disabled for release builds

## Status

? **Implementation Complete**  
? **Build Successful**  
? **Ready for Testing**

## Summary

The skill-based tolerance scaling adds **meaningful RPG progression** to flick shot mode. Players with low weight skill get a **wider tolerance window** (easier, more forgiving), while high-skill players face a **tighter window** (harder, requires precision). This INVERTED relationship matches real curling where beginners struggle with weight control and experts demonstrate precision. The system works for both players and AI, making the game feel more realistic and rewarding! ??

---

**Next Steps:**
1. Test with different weight skills (20%, 50%, 90%)
2. Tune `lowSkillScale` and `highSkillScale` based on feel
3. Test AI shots to verify their tolerance scales correctly
4. Verify equipment that boosts weight affects flick shot difficulty
5. Consider adding visual feedback for skill level
