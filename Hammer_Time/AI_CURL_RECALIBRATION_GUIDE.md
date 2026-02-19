# AI Curl Recalibration After Simulator Convention Inversion

## Problem Summary
The trajectory simulator's curl direction was inverted to match actual rock physics:
- **Old**: Out-turn ? RIGHT curl, In-turn ? LEFT curl
- **NEW**: Out-turn ? LEFT curl, In-turn ? RIGHT curl

This means **ALL AI targeting calculations are now incorrect** because they were tuned for the old curl convention.

## Affected Systems

### 1. AI_Target.cs - Physics-Based Targeting
**File**: `Assets\Scripts\AI\AI_Target.cs` (Line ~23)

**Current Value:**
```csharp
[Tooltip("Curl strength for AI simulation. CRITICAL: This must match the curlStrength in TrajectoryLine.cs!")]
public float curlStrength = 13.8f;
```

**Problem**: This was tuned for the **old curl direction**. Now that curl is inverted:
- Out-turn shots will miss **LEFT** (too much curl)
- In-turn shots will miss **RIGHT** (not enough curl)

**Immediate Fix**: Adjust `curlStrength` to compensate for inverted curl

### 2. TrajectorySimulator.cs - Curl Force Scale
**File**: `Assets\Scripts\UI\TrajectorySimulator.cs` (Line 39)

**Current Value:**
```csharp
public float curlForceScale = 0.9f;
```

**This value was tuned for the OLD convention**. Now needs adjustment.

## Step-by-Step Recalibration

### Phase 1: Baseline Testing (Do This First!)

1. **Set Up Test Environment**
   ```csharp
   // In QuickTestGame or similar
   gsp.aiTeamRed = true;
   gsp.aiTeamYellow = false;
   
   // Place target rock at button (0, 6.5)
   ```

2. **Run AI Takeout Tests**
   - AI throws at center button
   - **Expected**: Misses LEFT (out-turn) or RIGHT (in-turn)
   - **Measure**: Lateral offset in console logs

3. **Document Baseline Error**
   ```
   Target Position: (0.0, 6.5)
   Shot Type: Out-turn takeout
   
   OLD BEHAVIOR (before inversion):
   - Hit point: (0.05, 6.5) ? 5cm RIGHT of target ?
   
   NEW BEHAVIOR (after inversion):
   - Hit point: (-0.15, 6.5) ? 15cm LEFT of target ?
   - Error: 20cm lateral shift!
   ```

### Phase 2: Curl Strength Adjustment

#### Option A: Match TrajectoryLine Curl (Recommended)
The player trajectory uses `curlStrength = 0.25` in `TrajectoryLine.cs`.

**Try This First:**
```csharp
// In AI_Target.cs
public float curlStrength = 0.25f; // Match TrajectoryLine exactly
```

**Why**: AI should use the **same physics** as player preview for consistency.

#### Option B: Scale by Convention Change
If the curl **direction** inverted but **magnitude** stayed the same, try:

```csharp
// Old value scaled by -1 (direction flip)
public float curlStrength = -13.8f; // Negative to flip direction?
```

**Note**: This probably won't work because `curlStrength` is a magnitude, not a signed value.

#### Option C: Empirical Tuning
Start with a value and iterate:

```csharp
// Starting guess: Half of old value
public float curlStrength = 6.9f;

// Test and adjust:
// - Too much LEFT miss ? decrease curl
// - Too much RIGHT miss ? increase curl
```

### Phase 3: Curl Force Scale Tuning

After adjusting `curlStrength` in AI_Target, tune the simulator:

```csharp
// In TrajectorySimulator.cs (Line 39)
public float curlForceScale = 0.9f; // Start here

// If AI shots are accurate but player preview is off:
// - Increase: More curl in trajectory preview
// - Decrease: Less curl in trajectory preview
```

### Phase 4: Validation Testing

**Test Matrix:**

| Shot Type | Target Position | Expected Curl | Pass Criteria |
|-----------|----------------|---------------|---------------|
| Out-turn takeout | (0.0, 6.5) | LEFT | Hit within 5cm lateral |
| In-turn takeout | (0.0, 6.5) | RIGHT | Hit within 5cm lateral |
| Out-turn guard | (-0.5, 4.0) | LEFT | Hit within 10cm lateral |
| In-turn guard | (0.5, 4.0) | RIGHT | Hit within 10cm lateral |
| Out-turn draw | (0.0, 6.5) | LEFT | Hit within 5cm lateral |
| In-turn draw | (0.0, 6.5) | RIGHT | Hit within 5cm lateral |

**For Each Test:**
1. Place target rock
2. Let AI shoot
3. Measure lateral error in console
4. Record in spreadsheet

**Success Criteria:**
- 80% of shots within 10cm lateral error
- 50% of shots within 5cm lateral error
- No systematic bias (equal LEFT/RIGHT misses)

## Quick Testing Script

Add this to `AI_Target.cs` for rapid testing:

```csharp
[Header("Curl Calibration Testing")]
[Tooltip("Enable to log curl error for each shot")]
public bool logCurlError = false;

// In TakeOutTarget() after physics simulation:
if (logCurlError)
{
    Vector2 predictedHit = /* calculated hit point from trajectory */;
    Vector2 actualTarget = targetRockPos;
    float lateralError = predictedHit.x - actualTarget.x;
    
    Debug.Log($"[Curl Calibration] Shot Type: {useInTurn ? "IN-TURN" : "OUT-TURN"} | " +
              $"Target: ({actualTarget.x:F3}, {actualTarget.y:F3}) | " +
              $"Predicted: ({predictedHit.x:F3}, {predictedHit.y:F3}) | " +
              $"Lateral Error: {lateralError:F3} ({(lateralError > 0 ? "RIGHT" : "LEFT")})");
}
```

## Expected Results

### Before Recalibration
```
[AI_Target] Out-turn takeout to (0.0, 6.5)
[Physics] Predicted hit: (-0.15, 6.5)
[Actual] Rock hit at: (-0.18, 6.52)
? 15-18cm LEFT miss (too much curl)
```

### After Recalibration
```
[AI_Target] Out-turn takeout to (0.0, 6.5)
[Physics] Predicted hit: (0.02, 6.5)
[Actual] Rock hit at: (0.01, 6.48)
? 1-2cm error (acceptable!)
```

## Tuning Parameters Reference

| Parameter | Location | Old Value | New Value (Guess) | Notes |
|-----------|----------|-----------|-------------------|-------|
| `curlStrength` | AI_Target.cs | 13.8 | **0.25** | Match TrajectoryLine |
| `curlForceScale` | TrajectorySimulator.cs | 0.9 | **0.9-1.5** | Fine-tune after AI |
| `iceFriction` | TrajectoryLine.cs | 0.62 | **0.62** | Keep unchanged |

## Why This Happened

The original curl convention was **documented incorrectly** in `Rock_Force.cs` comments:
- Comments said: `flipAxis=true` ? LEFT curl
- **Reality**: `flipAxis=true` ? RIGHT curl

The trajectory simulator followed the **comments** instead of **reality**, so when we fixed the simulator to match reality, all the AI tuning (which was compensating for the wrong simulator) became incorrect.

## Rollback Plan (If Needed)

If recalibration takes too long, you can **revert the simulator** back to the old (wrong but working) convention:

```csharp
// In TrajectorySimulator.cs (Line 247)
// REVERT: Use old (incorrect) convention for AI compatibility
int dirMult = isInTurn ? -1 : 1;  // OLD: isInTurn ? LEFT, false ? RIGHT

// And in Traj_Transform.cs (Line 44)
// REVERT: Use old visual convention
if (!rm.inturn)
{
    transform.localScale = new Vector3(-1f, weight, 1f);
}
else
{
    transform.localScale = new Vector3(1f, weight, 1f);
}
```

This will restore AI accuracy but **break player trajectory preview accuracy**.

## Recommended Approach

1. ? **Keep the simulator fix** (matches reality now)
2. ? **Start with `curlStrength = 0.25f`** in AI_Target.cs
3. ? **Run 10 test shots** (5 in-turn, 5 out-turn)
4. ? **Measure lateral error** from console logs
5. ? **Adjust curl parameters** based on error direction
6. ? **Repeat until <10cm average error**

## Files to Modify

1. **`Assets\Scripts\AI\AI_Target.cs`** - Update `curlStrength` value
2. **`Assets\Scripts\UI\TrajectorySimulator.cs`** - Possibly adjust `curlForceScale`
3. **Test in QuickTestGame** with AI vs AI mode

## Status

- ? Simulator convention inverted to match reality
- ? Traj_Transform visual updated to match simulator
- ? **AI curl calibration needed** (this document)
- ? Testing and validation required

---

**Next Step**: Set `curlStrength = 0.25f` in `AI_Target.cs` and run test shots!
