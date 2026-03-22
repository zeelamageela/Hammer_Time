# Flick Shot Speed Slider - Implementation Guide

## ?? What You Get

A **visual speed guide** slider that shows:
1. ?? **Ghost rock** (50% opacity) cycling at ideal speed BEFORE player drags
2. ?? **Player rock** (full opacity) following player's drag progress
3. ?? **Color feedback**: Green = perfect match, Yellow = close, Red = way off
4. ?? **Optional**: Links slider to shooter animation

---

## ??? Unity Setup

### Step 1: Create Slider

1. **Right-click Canvas** ? UI ? Slider
2. Name: `FlickShotSpeedSlider`
3. Inspector ? Slider component:
   - **Direction**: Bottom To Top (VERTICAL!)
   - **Min Value**: 0
   - **Max Value**: 1
   - **Interactable**: ? (visual only)

### Step 2: Position Slider

RectTransform settings:
```
Anchor Preset: Right-Stretch (right side of screen)
Position X: -60 (60px from right edge)
Position Y: 0
Width: 80
Height: Full screen height
```

### Step 3: Style the Slider

#### Background:
- Color: Dark gray (0.2, 0.2, 0.2, 0.5)
- Sprite: Simple rectangle

#### Fill Area ? Fill:
- Image Type: Filled
- Fill Method: Vertical
- Color: Transparent (will be controlled by handle)

#### Handle Slide Area ? Handle:
- **Sprite**: Your rock sprite! ??
- **Size**: 60x60 pixels
- **Color**: White (will change to ghost/player/feedback colors)

### Step 4: Assign to FlickShotController

Select the rock GameObject with `FlickShotController`:
```
Inspector ? FlickShotController:
?? Speed Slider: [Drag FlickShotSpeedSlider here]
?? Slider Handle Image: [Drag Handle ? Image component]
?? Shooter Anim: [Optional - drag ShooterAnim if you want animation sync]
```

---

## ?? How It Works

### Phase 1: Before Drag (Ghost Mode)
```
Timer: 0s ? 0.8s ? 0s (cycles)
Handle: ?? 50% opacity white rock
Position: Animates bottom ? top ? bottom
Player: Watching the rhythm
```

### Phase 2: During Drag (Player Mode)
```
Handle: ?? 100% opacity rock (your color)
Position: Follows cursor Y (-25 to -16)
Color: ?? Green if matching ghost speed
       ?? Yellow if close
       ?? Red if way off
```

### Phase 3: After Release
```
Slider: Hidden
Result: Speed callout shows final rating
```

---

## ?? Visual States

### Perfect Match:
```
??????
? ?? ? ? Player rock (green)
?    ?
? ?? ? ? Ghost rock position
?    ?
??????
Player is matching ideal speed!
```

### Too Fast:
```
??????
? ?? ? ? Player rock (red, ahead of ghost)
?    ?
?    ?
? ?? ? ? Ghost rock position
??????
Slow down!
```

### Too Slow:
```
??????
? ?? ? ? Ghost rock position
?    ?
?    ?
? ?? ? ? Player rock (red, behind ghost)
??????
Speed up!
```

---

## ?? Color Feedback Logic

```csharp
Speed Ratio = Player Speed / Ghost Speed

Ratio 0.85-1.15 ? ?? GREEN  (Perfect! ±15%)
Ratio 0.70-0.85 ? ?? YELLOW (Close, bit slow)
Ratio 1.15-1.30 ? ?? YELLOW (Close, bit fast)
Ratio <0.70     ? ?? RED    (Way too slow!)
Ratio >1.30     ? ?? RED    (Way too fast!)
```

---

## ?? Optional: Shooter Animation Sync

If you want the shooter to animate with the player's swipe:

### In ShooterAnim.cs, add:
```csharp
public void SetAnimationProgress(float progress)
{
    // Map progress (0-1) to animation frames
    // 0 = Start of throwing motion
    // 1 = End of throwing motion (release)
    
    Animator anim = GetComponent<Animator>();
    if (anim != null)
    {
        // Set animation to specific normalized time
        anim.Play("ThrowAnimation", 0, progress);
        anim.speed = 0f; // Pause animation (manual control)
    }
}
```

### Result:
- Player drags up ? Shooter animates through throw
- Creates "pulling the shooter" feeling
- Very satisfying UX! ??

---

## ?? Benefits

1. ? **Visual timing guide** - No more guessing!
2. ? **Real-time feedback** - See if you're too fast/slow
3. ? **Ghost rhythm** - Creates natural timing cue
4. ? **Color coded** - Instant understanding
5. ? **Educational** - Players learn ideal timing
6. ? **Skill ceiling** - Mastering timing = mastering flick shots
7. ? **Optional animation sync** - Shooter moves with swipe!

---

## ?? Troubleshooting

### Slider not showing?
- Check `speedSlider.gameObject.SetActive(true)` is called
- Verify Canvas is rendering
- Check slider is child of Canvas

### Ghost not animating?
- Ensure `isPowerDragging == false` initially
- Check `idealDragTime` calculation (should be ~0.8s)
- Verify `Time.time` is progressing

### Handle not following cursor?
- Check `CalculateSwipeProgress()` returns 0-1
- Verify cursor Y is between -25 and -16
- Check camera world-to-screen conversion

### Colors not changing?
- Ensure `sliderHandleImage` is assigned
- Check `CalculateSpeedMatchingRatio()` logic
- Verify color lerp is working

---

## ?? Player Experience

**Before:** "How fast should I swipe? I have no idea!"
**After:** "Oh! Match the ghost rock - I get it now!" ???

The ghost rock creates a **visual metronome** that players naturally sync to, making the flick shot feel **intuitive and rhythmic** instead of random guesswork!

**This is a game-changer for UX!** ??
