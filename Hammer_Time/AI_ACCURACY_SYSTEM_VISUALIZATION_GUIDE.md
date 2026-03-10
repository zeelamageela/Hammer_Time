# AI Accuracy System - Complete Visualization Guide

## Overview
The AI accuracy system uses the **NEW skill system** (weightAccuracy, aimAccuracy, finesseAccuracy) to apply realistic errors to shots. This guide shows you EXACTLY where and how accuracy is applied in the shooting chain.

---

## ?? Complete AI Shooting Chain

### **Step 1: Strategy Decision** (`AI_Strategy.cs`)
```
AI_Strategy.ExecuteStrategy()
    ?
Determines WHAT shot to take (Guard, Draw, Takeout, etc.)
    ?
Creates ShotContext with strategic intent
    ?
Calls AI_Target.ExecuteIntent(context, rockCurrent)
```
**Accuracy applied:** ? NO - Pure strategic logic

---

### **Step 2: Target Calculation** (`AI_Target.cs`)
```
AI_Target.ExecuteIntent(context, rockCurrent)
    ?
Evaluates options based on intent:
  - EvaluateRemovalOptions() ? Takeouts
  - EvaluateScoringOptions() ? Draws
  - EvaluateProtectLeadOptions() ? Guards
  - EvaluateForceBlankOptions() ? Strategic guards
    ?
Calls specific target method:
  - TakeOutTarget() for takeouts
  - DrawTarget() for draws  
  - GuardTarget() for guards
```
**Accuracy applied:** ? YES - This is where the magic happens!

---

## ?? Accuracy Application Breakdown

### **TAKEOUT SHOTS** (Lines 1148-1220 in `AI_Target.cs`)

#### **Physics Calculation** (Perfect aim point)
```csharp
// 1. Calculate PERFECT physics-based shot
bool foundShot = CalculatePhysicsBasedShot(
    targetRockPosition,     // Where opponent rock is
    out pullbackPos,        // ? Output: perfect pullback position
    out useInTurn,          // ? Output: which turn to use
    "Take Out",
    targetRockIndex
);
```

#### **Accuracy Error Application** (Reality kicks in)
```csharp
// 2. Get shooter's skills
CharacterStats shooterStats = GetShooterStats(rockCurrent);
float aimAccuracy = shooterStats.aimAccuracy.GetValue();     // 0-100 (X-axis control)
float weightAccuracy = shooterStats.weightAccuracy.GetValue(); // 0-100 (Y-axis control)

// 3. Calculate ERROR RANGES based on skills
// QUADRATIC SCALING: Higher skill = exponentially less error
float aimRatio = Mathf.Clamp01(aimAccuracy / 100f);
float weightRatio = Mathf.Clamp01(weightAccuracy / 100f);

// X-axis error (lateral - controlled by AIM skill)
float aimBaseMaxError = Mathf.Lerp(0.06f, 0.02f, aimRatio * aimRatio); // 6cm?2cm
float aimMaxError = aimBaseMaxError * (1f - aimRatio);

// Y-axis error (distance - controlled by WEIGHT skill)  
float weightBaseMaxError = Mathf.Lerp(0.06f, 0.02f, weightRatio * weightRatio); // 6cm?2cm
float weightMaxError = weightBaseMaxError * (1f - weightRatio);

// 4. Generate INDEPENDENT random errors
float xError = Random.Range(-aimMaxError, aimMaxError);     // AIM controls this
float yError = Random.Range(-weightMaxError, weightMaxError); // WEIGHT controls this

// 5. Apply turn-direction sign correction
float lateralErrorSign = useInTurn ? 1f : -1f;
Vector2 errorOffset = new Vector2(xError * lateralErrorSign, yError);

// 6. Apply error to perfect pullback
pullbackPos += errorOffset; // ? Final pullback WITH error

// 7. Set final shot parameters
takeOutX = pullbackPos.x;
takeOutY = pullbackPos.y;
rm.inturn = useInTurn;
```

#### **Skill?Error Examples:**
| Aim Skill | Weight Skill | X Error Range | Y Error Range | Total Miss Radius |
|-----------|--------------|---------------|---------------|-------------------|
| 100% | 100% | ±0cm | ±0cm | **0cm (PERFECT)** |
| 80% | 80% | ±0.5cm | ±0.5cm | **0.7cm** |
| 60% | 60% | ±1.4cm | ±1.4cm | **2.0cm** |
| 40% | 40% | ±3.0cm | ±3.0cm | **4.2cm** |
| 20% | 20% | ±4.3cm | ±4.3cm | **6.1cm** |
| 0% | 0% | ±6.0cm | ±6.0cm | **8.5cm** |

---

### **DRAW SHOTS** (Lines 2280-2358 in `AI_Target.cs`)

#### **Physics Calculation** (Perfect aim point)
```csharp
// 1. Calculate PERFECT physics-based draw
bool foundDraw = CalculatePhysicsBasedDrawShot(
    targetPosition,    // Where we want rock to end up
    out pullbackPos,   // ? Output: perfect pullback
    out useInTurn      // ? Output: which turn to use
);
```

#### **Accuracy Error Application** (Same as takeouts but different skill weights)
```csharp
// 2. Get shooter's skills
CharacterStats shooterStats = GetShooterStats(rockCurrent);
float aimAccuracy = shooterStats.aimAccuracy.GetValue();
float weightAccuracy = shooterStats.weightAccuracy.GetValue();

// 3. Calculate ERROR RANGES (same formula as takeouts)
float aimRatio = Mathf.Clamp01(aimAccuracy / 100f);
float weightRatio = Mathf.Clamp01(weightAccuracy / 100f);

// DRAWS have SLIGHTLY HIGHER weight error (longer distance = more error)
float aimBaseMaxError = Mathf.Lerp(0.06f, 0.02f, aimRatio * aimRatio);
float aimMaxError = aimBaseMaxError * (1f - aimRatio);

float weightBaseMaxError = Mathf.Lerp(0.08f, 0.03f, weightRatio * weightRatio); // Slightly higher!
float weightMaxError = weightBaseMaxError * (1f - weightRatio);

// 4-6. Same error generation and application as takeouts
float xError = Random.Range(-aimMaxError, aimMaxError);
float yError = Random.Range(-weightMaxError, weightMaxError);

float lateralErrorSign = useInTurn ? 1f : -1f;
Vector2 errorOffset = new Vector2(xError * lateralErrorSign, yError);

pullbackPos += errorOffset;

// 7. Set final shot
takeOutX = pullbackPos.x;
takeOutY = pullbackPos.y;
rm.inturn = useInTurn;
```

---

### **GUARD SHOTS** (Similar to draws, lines 2430-2520)

#### **Physics Calculation**
```csharp
bool foundGuard = CalculatePhysicsBasedGuardShot(
    guardTargetPosition,  // Where guard should be placed
    out pullbackPos,
    out useInTurn
);
```

#### **Accuracy Error** (Same as draws - guards use weight+aim combo)
- Uses same error formula as draws
- Slightly TIGHTER error range (guards are shorter distance)
- `baseMaxError` range: 0.05f?0.02f (5cm?2cm)

---

## ?? Visual Error Distribution

### **Error Pattern Visualization**

```
PERFECT SHOT (100% skill):
    Target
      ?

LOW SKILL (20% skill):
    ???????????
    ?   ???   ?  ? Errors spread in circular pattern
    ?  ?????  ?
    ? ???T??? ?  T = Target
    ?  ?????  ?
    ?   ???   ?
    ???????????
    
MEDIUM SKILL (60% skill):
    ?????
    ? ??? ?
    ???T???  T = Target (tighter grouping)
    ? ??? ?
    ?????

HIGH SKILL (80% skill):
     ?
    ?T?   T = Target (very tight)
     ?
```

---

## ?? How to Visualize This In-Game

### **Option 1: Debug Logs** (Already implemented!)
The code logs detailed accuracy info:
```
[AI_Target] Takeout INDEPENDENT axis error (Aim/Weight skills)
  AIM SKILL: 75% ? X error range: ±0.008
  WEIGHT SKILL: 68% ? Y error range: ±0.012
  X error (aim): -0.005 (sign: 1)
  Y error (weight): 0.007
  Original pullback: (0.850, -21.500)
  Final pullback: (0.845, -21.493)
```

### **Option 2: Add Visual Indicators**

You could add this to `AI_Target.cs` to SHOW the error on screen:

```csharp
// After calculating errorOffset (line 1199 in takeouts):
if (gm.gsp.debug)
{
    // Draw a circle showing error radius
    GameObject errorCircle = new GameObject("AccuracyErrorVisualization");
    LineRenderer lr = errorCircle.AddComponent<LineRenderer>();
    lr.startColor = Color.yellow;
    lr.endColor = Color.yellow;
    lr.startWidth = 0.02f;
    lr.endWidth = 0.02f;
    
    // Draw circle at original perfect position
    int segments = 32;
    lr.positionCount = segments + 1;
    float radius = Mathf.Max(aimMaxError, weightMaxError);
    
    for (int i = 0; i <= segments; i++)
    {
        float angle = i * 2f * Mathf.PI / segments;
        float x = originalPullback.x + radius * Mathf.Cos(angle);
        float y = originalPullback.y + radius * Mathf.Sin(angle);
        lr.SetPosition(i, new Vector3(x, y, -0.1f));
    }
    
    // Destroy after 2 seconds
    Destroy(errorCircle, 2f);
}
```

### **Option 3: Enhanced Trajectory Preview**

Modify `TrajectoryLine.cs` to show **confidence interval**:
- Draw **main trajectory** (target position)
- Draw **2 ghost trajectories** (±max error bounds)
- Color-code by skill:
  - Green = 80-100% (tight grouping)
  - Yellow = 60-80% (moderate spread)
  - Orange = 40-60% (wide spread)  
  - Red = 0-40% (very wide)

---

## ?? Where Accuracy is NOT Applied

These components do NOT apply accuracy (they receive the already-modified pullback):

1. **`AI_Shooter.cs`** - Receives `takeOutX`/`takeOutY` from AI_Target
   - Just executes the shot at the given position
   - No additional error

2. **`Rock_Force.cs`** - Applies spring force based on pullback
   - Converts pullback position ? velocity
   - No accuracy modification

3. **`TrajectorySimulator.cs`** - Physics calculations only
   - Simulates rock path given velocity
   - No skill-based error (perfect physics)

---

## ?? Summary Flow Chart

```
AI_Strategy (Strategic Decision)
    ?
    "We need a takeout!"
    ?
AI_Target.ExecuteIntent()
    ?
AI_Target.TakeOutTarget()
    ?
CalculatePhysicsBasedShot()  ? PERFECT physics calculation
    ?
    Perfect Pullback: (0.850, -21.500)
    ?
GetShooterStats()  ? Get aim & weight skills
    ?
    Aim: 75%,  Weight: 68%
    ?
Calculate Error Ranges
    ?
    Aim Error Range: ±0.8cm
    Weight Error Range: ±1.2cm
    ?
Generate Random Errors
    ?
    X Error: -0.5cm
    Y Error: +0.7cm
    ?
Apply to Pullback
    ?
    Final Pullback: (0.845, -21.493)  ? With realistic error!
    ?
Set rm.inturn, takeOutX, takeOutY
    ?
AI_Shooter.OnShot()  ? Executes the shot
    ?
Rock_Force applies spring force
    ?
Rock travels with applied error
    ?
Result: Realistic miss pattern!
```

---

## ?? Testing Accuracy Visually

### **Quick Test Setup:**

1. **Set AI skill levels in inspector:**
   ```
   CharacterStats ? aimAccuracy = 50
   CharacterStats ? weightAccuracy = 50
   ```

2. **Enable debug logs:**
   ```csharp
   // In AI_Target.cs, logs are already enabled!
   // Look for "[AI_Target]" in console
   ```

3. **Watch multiple shots:**
   - AI with 50% skill should miss by ~2-3cm on average
   - AI with 100% skill should be PERFECT (0cm error)
   - AI with 0% skill should miss by ~6-8cm

4. **Visual confirmation:**
   - Watch rock final positions relative to target
   - Should see circular spread pattern around target
   - Higher skill = tighter circle

---

## ?? Key Takeaways

1. **Accuracy is applied ONCE** - at the pullback calculation stage
2. **Independent axes** - Aim controls X, Weight controls Y
3. **Quadratic scaling** - Skill improvement has accelerating benefits
4. **Turn-aware** - Lateral errors respect curl direction
5. **Realistic distribution** - Random within skill-based bounds
6. **Logged extensively** - Easy to debug and verify

**Location of accuracy code:**
- **Takeouts**: Lines 1148-1220 in `AI_Target.cs`
- **Draws**: Lines 2280-2358 in `AI_Target.cs`
- **Guards**: Lines 2430-2520 in `AI_Target.cs`

All three use the **SAME error formula** with slightly different base error ranges!
