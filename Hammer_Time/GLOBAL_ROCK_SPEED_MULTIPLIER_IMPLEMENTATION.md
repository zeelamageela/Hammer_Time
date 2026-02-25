# Global Rock Speed Multiplier Implementation

**Goal**: Make rocks take **twice as long** to reach their targets (50% speed = 2x duration)

---

## Approach: Global Velocity Scaling

### Where to Apply the Multiplier

**Location**: `Rock_Force.cs` - In the `Release()` method

**Why Here?**
- Applied AFTER spring launch completes
- Applied BEFORE curl forces start
- Single point of control for ALL rocks (player + AI)
- No need to recalibrate trajectories

---

## Implementation

### Step 1: Add Global Speed Multiplier to Rock_Force

```csharp
[Header("Physics Tuning")]
[Tooltip("Spring tension multiplier - affects initial velocity from same pull distance. 0.5 = half tension, 1.0 = normal")]
public float springTensionMultiplier = 1.0f;

[Tooltip("GLOBAL SPEED: 0.5 = half speed (2x duration), 1.0 = normal speed. Scales ALL rock velocities uniformly.")]
[Range(0.1f, 2.0f)]
public float globalSpeedMultiplier = 1.0f;  // ? NEW!

[Tooltip("Curl force multiplier - tune this to maintain trajectory shape at different speeds")]
public float curlForceMultiplier = 1.0f;
```

### Step 2: Apply in Release() Method

```csharp
public void Release()
{
    if (flipAxis)
        dirMult = -1;
    else
        dirMult = 1;

    GetComponent<SpriteRenderer>().enabled = true;
    
    // Restore damping NOW (was disabled during launch)
    body.linearDamping = baseDamping;
    
    Debug.Log($"[Rock_Force Release] Initial velocity: {body.linearVelocity.magnitude:F2} m/s");
    
    // Apply spring tension multiplier if configured
    if (springTensionMultiplier != 1.0f)
    {
        body.linearVelocity *= springTensionMultiplier;
        Debug.Log($"[Rock_Force] Tension multiplier: {springTensionMultiplier:F2}x");
    }
    
    // ? NEW: Apply GLOBAL speed multiplier
    if (globalSpeedMultiplier != 1.0f)
    {
        body.linearVelocity *= globalSpeedMultiplier;
        Debug.Log($"[Rock_Force] GLOBAL SPEED: {globalSpeedMultiplier:F2}x - Final velocity: {body.linearVelocity.magnitude:F2} m/s");
    }
    
    Debug.Log($"[Rock_Force] Final velocity: {body.linearVelocity.magnitude:F2} m/s");
    
    // Apply spin NOW (at hog line)
    turnStart = true;
    forceStart = true;
}
```

---

## Why This Works

### Physics Breakdown:

**Time to Target**:
```
distance = velocity × time
time = distance / velocity

If velocity ? 0.5x:
time = distance / (0.5 × velocity)
time = 2 × (distance / velocity)

Result: TIME DOUBLES ?
```

### Curl Behavior:

The curl forces in `FixedUpdate()` are **proportional to velocity**:
```csharp
body.AddForce(curl * vel, ForceMode2D.Force);
```

When velocity is halved:
- Curl force ? halved
- But time spent curling ? doubled
- **Total curl deflection stays the same!** ?

### Damping Behavior:

Linear damping also scales with velocity:
```
deceleration = damping × velocity

If velocity ? 0.5x:
deceleration ? 0.5x
Rock takes 2x longer to stop ?
```

---

## Usage Examples

### For 2x Duration (50% Speed):
```csharp
globalSpeedMultiplier = 0.5f;
```

### For 1.5x Duration (66% Speed):
```csharp
globalSpeedMultiplier = 0.67f;
```

### For 3x Duration (33% Speed):
```csharp
globalSpeedMultiplier = 0.33f;
```

---

## Alternative: Curl Force Scaling

If you want to **maintain curl forces** at the same absolute strength (more curl at slower speeds):

```csharp
// In FixedUpdate() - adjust curl compensation
Vector2 scaledCurl = curl * curlForceMultiplier * (1.0f / globalSpeedMultiplier);
body.AddForce(scaledCurl * vel, ForceMode2D.Force);
```

**Effect**:
- Slower rocks curl MORE (relative to forward motion)
- More realistic (real curling rocks curl more when traveling slower)
- But changes trajectory shapes ? Would need AI recalibration

---

## Testing Plan

### Step 1: Set globalSpeedMultiplier = 0.5

### Step 2: Test Player Draw
1. Aim for button
2. Pull back same distance as before
3. **Expected**: Rock takes 2x longer, still reaches button ?

### Step 3: Test AI Draw
1. Let AI shoot draw
2. **Expected**: AI trajectory still accurate, just takes 2x longer ?

### Step 4: Test Takeouts
1. Test heavy takeout
2. **Expected**: Same accuracy, just slower ?

---

## Advanced: Per-Shot Type Multipliers

If you want different speeds for different shot types:

```csharp
[Header("Speed by Shot Type")]
public float guardSpeedMultiplier = 0.6f;   // Guards slower
public float drawSpeedMultiplier = 0.5f;    // Draws normal slow
public float takeoutSpeedMultiplier = 0.7f; // Takeouts bit faster

// In Release(), check shot type:
float finalMultiplier = globalSpeedMultiplier;

if (GetComponent<Rock_Info>().shotType == "Guard")
{
    finalMultiplier *= guardSpeedMultiplier;
}
else if (GetComponent<Rock_Info>().shotType == "Takeout")
{
    finalMultiplier *= takeoutSpeedMultiplier;
}

body.linearVelocity *= finalMultiplier;
```

---

## Impact on Existing Systems

### ? No Impact (Works Automatically):
- Player trajectories (scaled velocity, same target)
- AI trajectories (scaled velocity, same target)
- Sweeping (proportional to velocity, still effective)
- Collisions (momentum conserved, just slower)
- Rock stopping distance (damping scales with velocity)

### ?? Potential Issues:
- **Audio/haptics** - May need to adjust pitch/intensity for slower speeds
- **Animation timing** - Sweeper animations might look too fast
- **Sweep effectiveness** - May need to increase sweep force multiplier

---

## Recommended Settings

### For "Realistic TV Viewing Speed" (2x duration):
```csharp
globalSpeedMultiplier = 0.5f;  // Half speed
curlForceMultiplier = 1.0f;    // Keep curl proportional
```

### For "Slightly Slower" (1.5x duration):
```csharp
globalSpeedMultiplier = 0.67f;  // 67% speed
curlForceMultiplier = 1.0f;
```

### For "Cinematic Slow-Mo" (3x duration):
```csharp
globalSpeedMultiplier = 0.33f;  // 33% speed
curlForceMultiplier = 1.0f;
```

---

## Implementation Code

### File: Rock_Force.cs

```csharp
[Header("Physics Tuning")]
[Tooltip("Spring tension multiplier - affects initial velocity from same pull distance. 0.5 = half tension, 1.0 = normal")]
public float springTensionMultiplier = 1.0f;

[Tooltip("GLOBAL SPEED MULTIPLIER: Scales all rock velocities uniformly. 0.5 = half speed (2x duration), 1.0 = normal speed.")]
[Range(0.1f, 2.0f)]
public float globalSpeedMultiplier = 1.0f;

[Tooltip("Curl force multiplier - tune this to maintain trajectory shape at different speeds")]
public float curlForceMultiplier = 1.0f;

// ... rest of fields ...

public void Release()
{
    if (flipAxis)
        dirMult = -1;
    else
        dirMult = 1;

    GetComponent<SpriteRenderer>().enabled = true;
    
    // DETERMINISTIC: Restore damping NOW (was disabled during launch)
    body.linearDamping = baseDamping;
    
    Debug.Log($"[Rock_Force Release] Velocity: {body.linearVelocity.magnitude:F2} m/s, flipAxis: {flipAxis}, damping restored to: {baseDamping}");
    
    // Apply spring tension multiplier if configured
    if (springTensionMultiplier != 1.0f)
    {
        body.linearVelocity *= springTensionMultiplier;
        Debug.Log($"[Rock_Force] Tension multiplier: {springTensionMultiplier:F2}x - Velocity: {body.linearVelocity.magnitude:F2} m/s");
    }
    
    // ? Apply GLOBAL speed multiplier (for adjusting game pacing)
    if (globalSpeedMultiplier != 1.0f)
    {
        body.linearVelocity *= globalSpeedMultiplier;
        Debug.Log($"[Rock_Force] Global speed multiplier: {globalSpeedMultiplier:F2}x - Final velocity: {body.linearVelocity.magnitude:F2} m/s");
    }
    
    // REAL CURLING: Apply spin NOW (at hog line, not at launch!)
    turnStart = true;
    forceStart = true;
}
```

---

## Quick Test Script

Add this to a debug script to test different speeds on the fly:

```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.Alpha1))
    {
        SetGlobalSpeed(1.0f);  // Normal speed
    }
    else if (Input.GetKeyDown(KeyCode.Alpha2))
    {
        SetGlobalSpeed(0.75f);  // 75% speed
    }
    else if (Input.GetKeyDown(KeyCode.Alpha3))
    {
        SetGlobalSpeed(0.5f);  // 50% speed (2x duration)
    }
    else if (Input.GetKeyDown(KeyCode.Alpha4))
    {
        SetGlobalSpeed(0.33f);  // 33% speed (3x duration)
    }
}

void SetGlobalSpeed(float multiplier)
{
    Rock_Force[] rocks = FindObjectsOfType<Rock_Force>();
    foreach (var rock in rocks)
    {
        rock.globalSpeedMultiplier = multiplier;
    }
    Debug.Log($"Global speed set to: {multiplier:F2}x");
}
```

---

## Summary

### To Make Rocks Take 2x Longer:

1. ? Add `public float globalSpeedMultiplier = 0.5f;` to `Rock_Force.cs`
2. ? In `Release()`, multiply velocity: `body.linearVelocity *= globalSpeedMultiplier;`
3. ? Test with 0.5x for 2x duration

### Advantages:
- ? Single line of code
- ? No trajectory recalibration needed
- ? Works for player AND AI
- ? Maintains curl behavior
- ? Adjustable in Unity Inspector

### Optional Enhancements:
- Adjust audio pitch based on speed
- Scale sweep force effectiveness
- Per-shot-type speed multipliers

---

**Ready to implement!** Want me to make the code changes now? ??
