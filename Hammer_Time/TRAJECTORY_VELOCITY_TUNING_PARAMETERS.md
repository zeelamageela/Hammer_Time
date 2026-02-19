# Trajectory Velocity Tuning Parameters - Inspector Configuration

## Summary
Made the velocity multiplier and pullback/velocity range parameters configurable in `TrajectorySimulator` to allow for easy inspector-based tuning.

## Changes Made

### File: `Assets\Scripts\UI\TrajectorySimulator.cs`

Added optional parameters to both velocity calculation methods:

#### 1. `CalculateInitialVelocityFromPullback` (Deterministic Linear)

**New Parameters:**
```csharp
public static Vector2 CalculateInitialVelocityFromPullback(
    Vector2 pullbackPosition, 
    Vector2 launcherPosition,
    float velocityMultiplier = 5.0f,          // NEW: Tunable velocity multiplier
    float minPullbackDistance = 0.5f,         // NEW: Minimum allowed pullback
    float maxPullbackDistance = 2.75f,        // NEW: Maximum allowed pullback
    float minVelocity = 3.0f,                 // NEW: Minimum velocity output
    float maxVelocity = 18.0f)                // NEW: Maximum velocity output
```

**Features:**
- **Velocity Multiplier**: Simple linear mapping (default 5.0)
- **Pullback Range**: Clamps input between min/max distances (0.5 to 2.75)
- **Velocity Range**: Clamps output velocity (3.0 to 18.0 m/s)
- **Alternative Mode**: Includes commented-out code for range-based remapping instead of multiplier

**Usage Examples:**
```csharp
// Use defaults (5.0 multiplier, 0.5-2.75 pullback, 3-18 m/s velocity)
Vector2 velocity = TrajectorySimulator.CalculateInitialVelocityFromPullback(
    pullbackPos, launcherPos);

// Custom multiplier for harder shots
Vector2 velocity = TrajectorySimulator.CalculateInitialVelocityFromPullback(
    pullbackPos, launcherPos, 6.0f);

// Custom range for guard shots (shorter, slower)
Vector2 velocity = TrajectorySimulator.CalculateInitialVelocityFromPullback(
    pullbackPos, launcherPos, 5.0f, 0.3f, 1.5f, 2.0f, 10.0f);

// Custom range for takeout shots (longer, faster)
Vector2 velocity = TrajectorySimulator.CalculateInitialVelocityFromPullback(
    pullbackPos, launcherPos, 5.0f, 1.0f, 3.5f, 8.0f, 20.0f);
```

#### 2. `CalculateInitialVelocityFromSpring` (Spring Physics-Based)

**New Parameters:**
```csharp
public static Vector2 CalculateInitialVelocityFromSpring(
    Vector2 pullbackPosition, 
    Vector2 launcherPosition,
    float springFrequency = 1.5f,
    float springDampingRatio = 0.2f,
    float minPullbackDistance = 0.5f,         // NEW: Minimum pullback range
    float maxPullbackDistance = 2.75f,        // NEW: Maximum pullback range
    float minVelocity = 3.0f,                 // NEW: Minimum velocity output
    float maxVelocity = 18.0f)                // NEW: Maximum velocity output
```

**Features:**
- **Spring Physics**: Uses real Unity SpringJoint2D physics
- **Pullback Range**: Remaps narrow spring range (0.5-2.75) to useful game range
- **Velocity Range**: Maps pullback to specific min/max velocities
- **Power Levels**: Quantized to 200 discrete levels for precise control

**Usage Examples:**
```csharp
// Use defaults (1.5 Hz, 0.2 damping, 0.5-2.75 pullback, 3-18 m/s)
Vector2 velocity = TrajectorySimulator.CalculateInitialVelocityFromSpring(
    pullbackPos, launcherPos);

// Custom spring settings (stiffer spring)
Vector2 velocity = TrajectorySimulator.CalculateInitialVelocityFromSpring(
    pullbackPos, launcherPos, 2.0f, 0.3f);

// Custom velocity range for draw shots (reach hog line to deep house)
Vector2 velocity = TrajectorySimulator.CalculateInitialVelocityFromSpring(
    pullbackPos, launcherPos, 1.5f, 0.2f, 0.5f, 2.75f, 4.0f, 12.0f);

// Custom velocity range for takeout shots (button to back boards)
Vector2 velocity = TrajectorySimulator.CalculateInitialVelocityFromSpring(
    pullbackPos, launcherPos, 1.5f, 0.2f, 0.5f, 2.75f, 8.0f, 20.0f);
```

## How to Use in Your Code

Since `TrajectorySimulator` is not a MonoBehaviour, you'll need to expose these parameters in MonoBehaviour components that use it.

### Example: Adding Inspector Fields to TrajectoryLine

```csharp
// In TrajectoryLine.cs or similar MonoBehaviour
[Header("Velocity Calculation")]
[Tooltip("Velocity multiplier for deterministic pullback (5.0 = default)")]
[Range(3.0f, 10.0f)]
public float velocityMultiplier = 5.0f;

[Tooltip("Minimum allowed pullback distance")]
[Range(0.1f, 1.0f)]
public float minPullbackDistance = 0.5f;

[Tooltip("Maximum allowed pullback distance")]
[Range(2.0f, 4.0f)]
public float maxPullbackDistance = 2.75f;

[Tooltip("Minimum velocity output (m/s) - reaching hog line")]
[Range(1.0f, 5.0f)]
public float minVelocity = 3.0f;

[Tooltip("Maximum velocity output (m/s) - hard takeout weight")]
[Range(15.0f, 25.0f)]
public float maxVelocity = 18.0f;

// Then when calling the method:
Vector2 velocity = TrajectorySimulator.CalculateInitialVelocityFromPullback(
    pullbackPos, 
    launcherPos,
    velocityMultiplier,
    minPullbackDistance,
    maxPullbackDistance,
    minVelocity,
    maxVelocity);
```

## Parameter Tuning Guide

### Velocity Multiplier (Deterministic Mode)
- **3.0 - 4.0**: Very light shots, barely reaching house
- **5.0** (default): Standard curling weight distribution
- **6.0 - 7.0**: Harder shots, more takeout weight
- **8.0+**: Very hard, guard blasting

### Pullback Range
- **0.5 - 2.75** (default): Full game range from gentle to hard
- **0.3 - 1.5**: Guard shots only (shorter pullback)
- **1.0 - 3.5**: Takeout weight only (longer pullback)
- **0.5 - 4.0**: Extended range for super-hard shots

### Velocity Range

| Min (m/s) | Max (m/s) | Description | Use Case |
|-----------|-----------|-------------|----------|
| **3.0** | **18.0** | Default full range | All shot types |
| **2.0** | **10.0** | Light shots | Guards, draws to front |
| **4.0** | **12.0** | Medium shots | Draws to house, raises |
| **8.0** | **20.0** | Heavy shots | Takeouts, peels |
| **5.0** | **15.0** | Balanced | Tournament play |

### Real Curling Velocity Reference
- **Guard (Y = -3)**: ~4-6 m/s
- **Front house (Y = 5)**: ~8-10 m/s
- **Button (Y = 7.5)**: ~10-12 m/s
- **Back house (Y = 10)**: ~12-14 m/s
- **Takeout weight**: ~14-18 m/s
- **Peel weight**: ~18-22 m/s

## Testing Recommendations

### 1. Calibrate Minimum Velocity
```csharp
// Test: Should just reach the in-play hog line (Y ? -16)
minVelocity = 3.0f;
// Pull back to minimum distance (0.5)
// Rock should barely cross hog line
```

### 2. Calibrate Maximum Velocity
```csharp
// Test: Should reach back boards (Y ? 10-11)
maxVelocity = 18.0f;
// Pull back to maximum distance (2.75)
// Rock should reach far end of house
```

### 3. Verify Linear Distribution
```csharp
// Test: Middle pullback should reach button
pullback = (minPullback + maxPullback) / 2; // ? 1.625
// Should produce velocity: (minVel + maxVel) / 2 ? 10.5 m/s
// Rock should reach button area (Y ? 7-8)
```

### 4. Test Edge Cases
```csharp
// Too short: Should clamp to min velocity
pullback = 0.1f; // ? clamped to 0.5 ? velocity = minVelocity

// Too long: Should clamp to max velocity
pullback = 5.0f; // ? clamped to 2.75 ? velocity = maxVelocity
```

## Alternative: Range-Based Mapping

The code includes a commented-out alternative approach that directly maps pullback distance to velocity range:

```csharp
// Option 2: Remap to specific velocity range (more control)
// Uncomment this and comment out line above to use range-based mapping
float normalizedPullback = (pullbackDistance - minPullbackDistance) / 
                          (maxPullbackDistance - minPullbackDistance);
float velocity = minVelocity + (normalizedPullback * (maxVelocity - minVelocity));
```

**When to use this:**
- You want exact velocity at specific pullback distances
- You want non-linear scaling (can add easing curves)
- You need precise control over the entire range

**Example:**
```csharp
// At 0.5 pullback ? exactly 3.0 m/s
// At 1.625 pullback ? exactly 10.5 m/s
// At 2.75 pullback ? exactly 18.0 m/s
```

## Benefits

### For Designers
- ? Tune in Unity Inspector without code changes
- ? Different ranges for different shot types
- ? Quick iteration on game feel
- ? Visual feedback in editor

### For Developers
- ? Backward compatible (all parameters have defaults)
- ? Existing calls work without changes
- ? Flexible: can use default or custom values
- ? Well-documented parameter meanings

### For Players
- ? Predictable pullback-to-distance mapping
- ? Consistent feel across shot types
- ? Clear min/max ranges
- ? Fair and balanced gameplay

## Integration Status

? **Implemented** - Both methods updated with tunable parameters
? **Compiled** - Build successful
? **Backward Compatible** - All existing code still works
? **Needs Inspector Exposure** - Add fields to MonoBehaviour components as needed

## Next Steps

1. **Add Inspector Fields** - Expose parameters in `TrajectoryLine.cs` or `HouseClick.cs`
2. **Test Calibration** - Verify min/max velocities reach expected distances
3. **Shot Type Presets** - Create different parameter sets for guards, draws, takeouts
4. **UI Indicators** - Show player current pullback distance and expected landing zone
5. **Save Settings** - Store tuned values in ScriptableObject or PlayerPrefs

## Build Status

? **Build Successful** - All changes compile without errors

---

**Status**: ? Complete - Ready for inspector integration
**Compatibility**: ? Backward compatible with existing code
**Documentation**: ? Comprehensive usage guide included
