# Rock Physics Tuning Guide

## ?? Overview
You can now tune rock physics using **three independent multipliers** in `Rock_Force.cs`. This allows you to keep the same pull-back distance while adjusting how far and how the rock travels.

---

## ??? The Three Tuning Parameters

### 1. **Spring Tension Multiplier** (Initial Velocity)
```csharp
public float springTensionMultiplier = 1.0f;
```

**What it does:**  
- Controls the **initial velocity** from the same pull-back distance
- Think of it as "spring stiffness" - lower = softer spring

**Effect on physics:**
- **0.5** = Half the initial velocity (rock leaves launch slower)
- **0.75** = 75% initial velocity
- **1.0** = Normal (no change)

**Use when:**
- You want rocks to start slower without changing user input
- You want to reduce overall shot power

---

### 2. **Ice Friction Multiplier** (Deceleration)
```csharp
public float iceFrictionMultiplier = 1.0f;
```

**What it does:**  
- Adjusts the `linearDamping` on the Rigidbody2D
- Controls how quickly the rock **slows down** on ice

**Effect on physics:**
- **0.5** = Half the friction (rock slides twice as far)
- **0.75** = 25% less friction (rock slides farther)
- **1.0** = Normal friction (default Rigidbody2D damping)
- **1.5** = 50% more friction (rock stops sooner)

**Use when:**
- Tuning travel distance after adjusting spring tension
- Simulating different ice conditions

---

### 3. **Curl Force Multiplier** (Trajectory Shape)
```csharp
public float curlForceMultiplier = 1.0f;
```

**What it does:**  
- Scales the continuous curl force applied in `FixedUpdate()`
- Controls how much the rock **curves** during travel

**Effect on physics:**
- **0.5** = Half the curl (straighter path)
- **0.75** = 25% less curl
- **1.0** = Normal curl
- **1.5** = 50% more curl (more banana-shaped)

**Use when:**
- Fine-tuning trajectory shape at different speeds
- Matching trajectory to original at half speed

---

## ?? Physics Relationships

### Distance Formula:
```
Distance ? (Initial Velocity)² / (2 × Friction)
```

### To maintain same distance at half velocity:
1. Set `springTensionMultiplier = 0.5` (half velocity)
2. Set `iceFrictionMultiplier = 0.25` (quarter friction)
3. Adjust `curlForceMultiplier = 0.5` (half curl, approximately)

**Result:** Rock travels same distance, takes ~2× longer, similar trajectory shape

---

## ?? Example Tuning Scenarios

### Scenario 1: Half Speed, Same Distance
**Goal:** Rocks move slower but reach the same target

```csharp
springTensionMultiplier = 0.5f;   // Half velocity
iceFrictionMultiplier = 0.25f;    // Quarter friction (v² relationship)
curlForceMultiplier = 0.5f;       // Half curl (maintain shape)
```

**Expected outcome:**
- Same pull-back distance
- Rock reaches button at half speed
- Takes twice as long
- Similar curl pattern

---

### Scenario 2: "Easy Mode" (Longer Shots)
**Goal:** Rocks travel farther without pulling back more

```csharp
springTensionMultiplier = 1.0f;   // Normal velocity
iceFrictionMultiplier = 0.5f;     // Half friction
curlForceMultiplier = 1.0f;       // Normal curl
```

**Expected outcome:**
- Same pull-back = longer shot
- ~2× travel distance
- Easier to reach far targets

---

### Scenario 3: "Hard Mode" (Shorter Shots)
**Goal:** Rocks stop sooner, need more precision

```csharp
springTensionMultiplier = 1.0f;   // Normal velocity
iceFrictionMultiplier = 1.5f;     // More friction
curlForceMultiplier = 1.0f;       // Normal curl
```

**Expected outcome:**
- Same pull-back = shorter shot
- ~66% travel distance
- Harder to reach far targets

---

### Scenario 4: Low Curl Game
**Goal:** Reduce curl dramatically for straight shots

```csharp
springTensionMultiplier = 1.0f;   // Normal velocity
iceFrictionMultiplier = 1.0f;     // Normal friction
curlForceMultiplier = 0.3f;       // 70% less curl
```

**Expected outcome:**
- Much straighter trajectories
- Easier to aim directly at target
- Less strategy around sweeping/curl

---

## ?? Step-by-Step Tuning Process

### Step 1: Set Target Behavior
Decide what you want:
- Same distance at half speed?
- Easier/harder shots?
- More/less curl?

### Step 2: Adjust Spring Tension
```csharp
springTensionMultiplier = 0.5f; // Start here for half speed
```

### Step 3: Test Initial Shot
- Pull back to button distance
- Note where rock stops

### Step 4: Tune Friction
If rock travels **too far**:
```csharp
iceFrictionMultiplier = 0.3f; // Increase (more friction)
```

If rock travels **too short**:
```csharp
iceFrictionMultiplier = 0.2f; // Decrease (less friction)
```

### Step 5: Fine-Tune Trajectory Shape
Compare curl pattern to original:

**Too straight:**
```csharp
curlForceMultiplier = 0.6f; // Increase curl
```

**Too curvy:**
```csharp
curlForceMultiplier = 0.4f; // Decrease curl
```

### Step 6: Iterate
- Test multiple shot types (draw, takeout, guard)
- Adjust until trajectory feels right
- Log values for documentation

---

## ?? Expected Values Reference

### For Half-Speed Physics:

| Parameter | Theoretical | Recommended Starting Point | Notes |
|-----------|-------------|---------------------------|-------|
| Spring Tension | 0.5 | 0.5 | Exact half velocity |
| Ice Friction | 0.25 | 0.2 - 0.3 | May need tweaking due to curl |
| Curl Force | 0.5 | 0.4 - 0.6 | Depends on feel preference |

### Base Damping Values:
```csharp
baseDamping = 0.38f; // Captured from Rigidbody2D on Awake()
```

This is automatically read from the rock's `Rigidbody2D.linearDamping` value.

---

## ?? Debugging Tips

### Check Current Values:
Look for this debug log on rock release:
```
[Rock_Force] Spring tension: 0.50x - Velocity: 2.45 m/s
[Rock_Force] Base Damping: 0.380, Ice Friction Mult: 0.25, Final Damping: 0.095
```

### Common Issues:

**Problem:** Rock doesn't curl at all  
**Solution:** `curlForceMultiplier` too low or `curl` vector is zero

**Problem:** Rock travels way too far/short  
**Solution:** Check `iceFrictionMultiplier` - may be inverted

**Problem:** Trajectory shape wrong  
**Solution:** `curlForceMultiplier` needs adjustment relative to `springTensionMultiplier`

---

## ?? Recommended Workflow

1. **Start with normal (all = 1.0)**
2. **Adjust spring tension** to desired speed
3. **Tune friction** to match original distance
4. **Fine-tune curl** for trajectory shape
5. **Test all shot types** (draw, guard, takeout)
6. **Document final values** in this file

---

## ?? Notes

- All three multipliers are **independent**
- Changes apply to **all rocks** with this script
- Multipliers affect **spring-launched rocks** only
- AI trajectory calculation may need updates if you change these significantly
- Trajectory line prediction should match if using same physics

---

*Last updated: Rock physics tuning system implementation*
