# Flick Shot Mode - Implementation Guide ?

**Status**: ? Settings Infrastructure Complete - **Ready for Core Implementation**  
**Date**: 2024  
**Build Status**: ? SUCCESS

---

## ?? Overview

The **Flick Shot Mode** is an alternative shooting system that separates aiming and power into two distinct phases, providing a more skill-based and realistic curling experience.

### Traditional Mode (Current)
- **One Phase**: Drag rock back from launcher to aim & set power simultaneously
- **Pullback distance** = Both direction and power
- **Requires**: Spatial awareness (diagonal pullback)

### Flick Shot Mode (New)
- **Phase 1**: Aim by rotating rock around launcher (locked distance 3.5 units)
- **Phase 2**: Power by dragging rock toward hog line (speed determines power)
- **Separates**: Direction (aim) from strength (power)
- **More Realistic**: Mimics real curling delivery motion

---

## ? Completed Steps

### 1. Settings Infrastructure ?
- Added `FlickShotMode` property to `GameVisualizationSettings`
- Persists to `PlayerPrefs` with key `"FlickShotMode"` (default: OFF)
- Event system for mode changes: `OnFlickShotModeChanged`
- Toggle method: `ToggleFlickShotMode(bool enabled)`

### 2. UI Integration ?
- Added `flickShotModeToggle` reference to `VisualizationToggleUI`
- Callback method: `OnFlickShotModeToggleChanged(bool isOn)`
- Logs mode changes for debugging

---

## ?? Remaining Implementation

### Step 1: Create FlickShotController Script

**File**: `Assets/Scripts/Rock/FlickShotController.cs`

**Key Features**:
```csharp
public class FlickShotController : MonoBehaviour
{
    // Phase tracking
    public enum FlickShotPhase { Inactive, AimingPhase, PowerPhase, Released }
    
    // Phase 1: Aim Settings
    public float aimLockDistance = 3.5f;  // Fixed distance from launcher
    public float aimSensitivity = 1.5f;   // Rotation speed
    
    // Phase 2: Power Settings
    public float powerDragStartY = -25f;   // Start position (hack)
    public float powerDragTargetY = -16f;  // Target position (hog line)
    
    // Speed Quantization
    public int speedBands = 5;  // Very Slow | Slow | Medium | Fast | Very Fast
    public float perfectTolerance = 0.15f;  // ±15% for "Perfect!"
    
    // Skill Tuning
    public float forgivenessFactor = 1.2f;  // Higher = easier
    
    // Visual Feedback
    public bool showSpeedFeedback = true;
    public float feedbackInterval = 0.1f;  // Update frequency
}
```

**Methods**:
1. `StartFlickShot()` - Initialize aiming phase
2. `UpdateAimingPhase()` - Handle aim rotation (mouse delta X)
3. `StartPowerPhase()` - Transition to power phase (on rock click)
4. `UpdatePowerPhase()` - Track drag speed & provide feedback
5. `CalculateSpeedBand(dragTime, dragDistance)` - Quantize speed
6. `ShowSpeedFeedback(message)` - Display text callouts
7. `ReleaseFlickShot()` - Calculate final velocity & apply to rock

---

### Step 2: Integrate with Rock_Flick

**File**: `Assets/Scripts/Rock/Rock_Flick.cs`

**Changes Needed**:

```csharp
// Add reference
FlickShotController flickShotController;
bool isFlickShotMode = false;

// In OnEnable()
void OnEnable()
{
    // ... existing code ...
    
    // Check for flick shot controller
    flickShotController = GetComponent<FlickShotController>();
    if (flickShotController != null)
    {
        isFlickShotMode = GameVisualizationSettings.Instance.FlickShotMode;
        GameVisualizationSettings.Instance.OnFlickShotModeChanged += OnFlickShotModeChanged;
    }
}

// Subscribe to mode changes
void OnFlickShotModeChanged(bool enabled)
{
    isFlickShotMode = enabled;
    Debug.Log($"[Rock_Flick] Flick shot mode: {(enabled ? "ON" : "OFF")}");
}

// Modify OnMouseDown()
void OnMouseDown()
{
    if (!GetComponent<Rock_Info>().released)
    {
        // NEW: Check if flick shot mode
        if (isFlickShotMode && flickShotController != null)
        {
            flickShotController.StartFlickShot();
            return; // Skip normal pullback logic
        }
        
        // EXISTING: Normal pullback logic...
    }
}
```

---

### Step 3: Visual Feedback System

**Use TextCalloutManager** (already implemented):

```csharp
// In FlickShotController.ShowSpeedFeedback()
TextCalloutManager.Instance.ShowCallout(
    rockPosition,
    message,  // "Perfect!", "Too Fast!", "Too Slow!", etc.
    followTarget: true,
    target: rockTransform,
    duration: feedbackInterval * 2f
);
```

**Feedback Messages** (based on speed bands):
- **Band 0** (slowest): `"Way too slow!"`
- **Band 1**: `"Too slow!"`
- **Band 2** (middle/perfect): `"Perfect!"`
- **Band 3**: `"Too fast!"`
- **Band 4** (fastest): `"Way too fast!"`

---

### Step 4: Trajectory Visualization

**Goal**: Show trajectory during aim phase at locked distance

**Approach**:
```csharp
// In UpdateAimingPhase()
void UpdateAimingPhase()
{
    // Update rock position (locked at aimLockDistance)
    Vector2 launcherPos = launcher.transform.position;
    Vector2 lockedPosition = launcherPos + aimDirection * aimLockDistance;
    rockRb.position = lockedPosition;
    
    // Update trajectory visualization
    trajectoryLine.DrawTrajectory();  // Uses locked position
}
```

**No changes needed to TrajectoryLine** - it already reads rock position!

---

### Step 5: Shooting Knob Extension

**Goal**: Extend shooting knob line to locked distance during aim phase

**Option A**: Modify `ShootingKnob.cs` to detect flick shot mode
```csharp
// In ShootingKnob.Update()
if (flickShotMode)
{
    // Draw line from launcher to rock (locked distance)
    lineRenderer.SetPosition(0, launcherPosition);
    lineRenderer.SetPosition(1, rockPosition);  // At aimLockDistance
}
```

**Option B**: Use trajectory line only (simpler - recommended)
- Don't show shooting knob in flick shot mode
- Trajectory line provides visual guidance

---

## ?? User Experience Flow

### Phase 1: Aiming
1. Player clicks on rock ? **Aiming Phase starts**
2. Rock locks at 3.5 units from launcher (straight down initially)
3. Player moves mouse left/right ? **Rock rotates around launcher**
4. Trajectory updates in real-time (shows where rock will go)
5. Player clicks rock again ? **Power Phase starts**

### Phase 2: Power
1. **Drag starts** at rock's current position
2. Player drags rock toward hog line (Y = -16)
3. **Speed feedback** shows during drag:
   - Updates every 0.1 seconds
   - Text callouts: "Too slow!", "Perfect!", "Too fast!"
4. Player releases mouse ? **Rock releases with calculated velocity**

### Skill Elements
1. **Aim Precision**: Rotating to exact angle (locked distance removes distance variable)
2. **Power Control**: Drag speed dictates rock velocity (quantized into bands)
3. **Learning Curve**: Visual feedback helps players learn optimal speed
4. **Forgiveness Factor**: Configurable tolerance for "Perfect" band

---

## ?? Tuning Parameters

### Aim Phase
| Parameter | Default | Range | Purpose |
|-----------|---------|-------|---------|
| `aimLockDistance` | 3.5 | 2.0-5.0 | Locked pullback distance |
| `aimSensitivity` | 1.5 | 0.1-5.0 | Rotation speed |

### Power Phase
| Parameter | Default | Range | Purpose |
|-----------|---------|-------|---------|
| `powerDragStartY` | -25f | -30 to -20 | Drag start position |
| `powerDragTargetY` | -16f | -20 to -10 | Drag target (hog line) |
| `minDragTime` | 0.1s | 0.05-0.5 | Minimum time to register |
| `maxDragTime` | 1.5s | 0.2-2.0 | Time for fastest shot |

### Skill Tuning
| Parameter | Default | Range | Purpose |
|-----------|---------|-------|---------|
| `speedBands` | 5 | 3-10 | Number of speed quantization levels |
| `perfectTolerance` | 0.15 | 0.05-0.3 | ±15% for "Perfect!" band |
| `forgivenessFactor` | 1.2 | 0.5-2.0 | Easier = higher value |

### Visual Feedback
| Parameter | Default | Range | Purpose |
|-----------|---------|-------|---------|
| `showSpeedFeedback` | true | bool | Enable text callouts |
| `feedbackInterval` | 0.1s | 0.05-0.5 | Callout update frequency |

---

## ?? Speed Quantization Logic

### Formula
```csharp
// Normalize drag time to 0-1 (faster = higher)
float normalizedTime = Mathf.Clamp01((dragTime - minDragTime) / (maxDragTime - minDragTime));
normalizedTime = 1f - normalizedTime;  // Invert (faster = higher)

// Apply forgiveness (compress toward center)
normalizedTime = Mathf.Lerp(0.5f, normalizedTime, 1f / forgivenessFactor);

// Calculate speed band (0 = slowest, speedBands-1 = fastest)
int speedBand = Mathf.FloorToInt(normalizedTime * speedBands);
speedBand = Mathf.Clamp(speedBand, 0, speedBands - 1);

// Map to velocity range
float targetSpeed = Mathf.Lerp(minVelocity, maxVelocity, normalizedTime);
```

### Example (5 speed bands)
| Drag Time | Normalized | Band | Message | Velocity |
|-----------|------------|------|---------|----------|
| 0.2s | 1.0 | 4 | "Way too fast!" | 13.0 m/s |
| 0.5s | 0.75 | 3 | "Too fast!" | 11.0 m/s |
| 0.85s | 0.5 | 2 | **"Perfect!"** | 9.0 m/s |
| 1.2s | 0.25 | 1 | "Too slow!" | 7.0 m/s |
| 1.5s | 0.0 | 0 | "Way too slow!" | 5.0 m/s |

---

## ?? Visual Feedback Examples

### During Aim Phase
```
[Rock at -3.5 Y, rotating left/right]
Trajectory dots show predicted path
Shooting knob line extends to rock (optional)
```

### During Power Phase
```
[Rock dragging toward hog line]

?? Text Callout: "Too slow!" (following rock)
   ? (drag faster)
?? Text Callout: "Perfect!" (following rock)
   ? (maintain speed)
?? Text Callout: "Too fast!" (following rock)
```

### At Release
```
[Rock released with velocity]

?? Final Callout: "Perfect!" (large, centered)
     ? Rock travels at medium speed
```

---

## ??? Inspector Setup (After Implementation)

### Rock GameObject
1. Select rock prefab in hierarchy
2. Add `FlickShotController` component
3. **Auto-assign references**:
   - `rb` ? Rigidbody2D (same GameObject)
   - `launcher` ? GameObject with tag "Launcher"
   - `trajectoryLine` ? TrajectoryLine (find in scene)
4. **Tune parameters** (use defaults initially)

### Pause Menu / Options
1. Open pause menu scene
2. Find `VisualizationToggleUI` GameObject
3. **Create new toggle**:
   - Label: "Flick Shot Mode"
   - Tooltip: "Use two-phase aiming (aim then power)"
   - Default: OFF
4. **Assign toggle** to `flickShotModeToggle` field in Inspector

---

## ?? Testing Checklist

### Phase 1: Aiming
- [ ] Rock locks at 3.5 units from launcher on click
- [ ] Mouse movement rotates rock left/right
- [ ] Rotation stays within downward hemisphere (180°-360°)
- [ ] Trajectory updates in real-time during aim
- [ ] Click on rock transitions to power phase

### Phase 2: Power
- [ ] Drag speed tracked correctly
- [ ] Speed feedback appears every 0.1s
- [ ] Text callouts follow rock position
- [ ] Messages change based on speed band
- [ ] Release calculates correct velocity

### Integration
- [ ] Toggle in pause menu saves/loads setting
- [ ] Mode changes apply immediately
- [ ] Normal pullback mode still works when toggle OFF
- [ ] No errors in console during mode switch

### Edge Cases
- [ ] Clicking away from rock cancels shot (resets to launcher)
- [ ] Extremely fast drag handled correctly
- [ ] Extremely slow drag handled correctly
- [ ] Mode change mid-shot doesn't break state

---

## ?? Future Enhancements

### Advanced Feedback
- **Speed Meter UI**: Visual slider showing current speed band
- **Ghost Rock**: Show where rock will end up at current speed
- **Color-Coded Callouts**: Green = perfect, yellow = acceptable, red = too fast/slow

### Skill Progression
- **Adaptive Difficulty**: Forgiveness factor decreases as player improves
- **Challenge Modes**: "Perfect Only" mode (only perfect speed band works)
- **Speed Training**: Practice mode with visual speed indicator

### Accessibility
- **Configurable Bands**: Let players choose 3-10 speed bands
- **Audio Feedback**: Pitch changes based on speed (higher = faster)
- **Haptic Feedback**: Vibration intensity matches speed

---

## ?? Implementation Priority

### Phase 1 (Core) - **3-4 hours**
1. Create `FlickShotController.cs` with aim & power phase logic
2. Integrate with `Rock_Flick.OnMouseDown()`
3. Add text callout feedback system
4. Test basic two-phase flow

### Phase 2 (Polish) - **2-3 hours**
1. Tune speed quantization parameters
2. Add shooting knob extension (optional)
3. Improve trajectory visualization during aim
4. Add cancellation logic (click away from rock)

### Phase 3 (Testing & Refinement) - **2-3 hours**
1. Playtest with different forgiveness factors
2. Adjust speed bands for optimal skill curve
3. Refine feedback messages & timing
4. Balance against traditional pullback mode

**Total Estimated Time**: **7-10 hours**

---

## ? Build Status

- ? **GameVisualizationSettings** updated with FlickShotMode
- ? **VisualizationToggleUI** prepared for flick shot toggle
- ? **Rock_Flick** ready for integration (no breaking changes)
- ? **Build compiles successfully**
- ? **No errors or warnings**

---

## ?? Next Steps

1. **Create FlickShotController.cs** with full implementation
2. **Test aiming phase** (rotation around launcher)
3. **Test power phase** (drag speed & feedback)
4. **Integrate text callouts** (TextCalloutManager)
5. **Add UI toggle** to pause menu
6. **Playtest & tune** parameters

---

**Status**: ? **READY FOR CORE IMPLEMENTATION** ??

The settings infrastructure is complete. Now you can implement the FlickShotController script with the two-phase aiming system!
