# Enhanced AI Sweeper - Quick Reference Card

## ?? ONE-PAGE CHEAT SHEET

### Core Formula:
```
Rock Position = Actual Path (observed)
Predicted Position = Trajectory from physics
Error = Actual - Predicted

IF error too big:
   ? SWEEP to correct!
```

---

## ?? SWEEPING EFFECTS QUICK REF

| Sweep Type | Sweepers | Distance Effect | Curl Effect | When to Use |
|------------|----------|-----------------|-------------|-------------|
| **WEIGHT** | Both (L+R) | +10-15% distance | +5-10% curl | Rock falling SHORT |
| **LINE** | One (curl side) | +5-8% distance | Much STRAIGHTER | Rock curling TOO MUCH |
| **CURL** | One (opposite) | +5-8% distance | Much MORE curl | Rock NOT curling enough |
| **WHOA** | None | No change | No change | Rock ON TRACK |

---

## ?? DECISION TREE

```
START monitoring (every 0.02s after hog line)
  ?
  ?? Predicted shortfall > 1.0 units?
  ?  ?? YES ? CRITICAL WEIGHT SWEEP
  ?
  ?? Predicted shortfall > 0.25 units?
  ?  ?? YES ? WEIGHT SWEEP
  ?
  ?? Lateral error > 0.12 units?
  ?  ?? Rock too far RIGHT?
  ?  ?  ?? YES ? LINE SWEEP (straighten)
  ?  ?
  ?  ?? Rock too far LEFT?
  ?     ?? YES ? CURL SWEEP (more curl)
  ?
  ?? Otherwise ? WHOA (don't sweep)
```

---

## ?? COPY-PASTE CODE LOCATIONS

### 1. In AI_Shooter.cs, add these 4 methods:
- `MonitorAndSweepCoroutine()` - Main loop
- `GetPredictedPositionAtY()` - Find predicted position
- `ApplySweepState()` - Execute sweep commands
- `GetSweeperSkill()` - Read sweeper stats

### 2. In AI_Shooter.Shot(), after `rockFlick.mouseUp = true`:
```csharp
yield return new WaitForFixedUpdate();
yield return new WaitForFixedUpdate();
StartCoroutine(MonitorAndSweepCoroutine(rockRB, initialVelocity, inturn, aiTarg.targetPos, aiShotType));
```

---

## ?? QUICK TUNING

Too Much Sweeping? Increase thresholds:
```csharp
float lateralErrorThreshold = 0.18f;  // Was 0.12
float distanceErrorThreshold = 0.35f; // Was 0.25
```

Too Little Sweeping? Decrease thresholds:
```csharp
float lateralErrorThreshold = 0.08f;  // Was 0.12
float distanceErrorThreshold = 0.15f; // Was 0.25
```

---

## ?? DEBUG CHECKLIST

| Problem | Check This | Solution |
|---------|------------|----------|
| No sweeping at all | Coroutine started? | Add StartCoroutine call |
| | Rock crossed hog line? | Wait for Y > -16.15 |
| | Predicted path generated? | Check TrajectorySimulator |
| Wrong direction | isInTurn correct? | Verify in AI_Shooter.Shot() |
| | Lateral error sign? | + = right, - = left |
| Too aggressive | Thresholds too low | Increase 0.12 ? 0.18 |
| Not aggressive enough | Thresholds too high | Decrease 0.12 ? 0.08 |
| Same for all skills | Skill multiplier | Check GetSweeperSkill() |

---

## ?? EXPECTED LOGS

```
[AI_Shooter] Starting sweeping monitor: velocity=10.5 m/s, target=(0.00, 6.50), inTurn=True
[AI_Sweeper] Monitoring started - predicted path has 187 points
[AI_Sweeper] Rock crossed hog line - sweeping enabled!
[AI_Sweeper] Y=-10.5: State=Weight, LateralErr=-0.05, Shortfall=0.35
[AI_Sweeper] Y=2.5: State=None, LateralErr=0.08, Shortfall=-0.05
[AI_Sweeper] Rock stopped - WHOA
```

---

## ?? TURN DIRECTION RULES

| Turn Type | Curls Toward | Rock Too Far Right | Rock Too Far Left |
|-----------|--------------|-------------------|-------------------|
| **IN-TURN** | RIGHT (+X) | LINE sweep (straighten) | CURL sweep (more curl) |
| **OUT-TURN** | LEFT (-X) | CURL sweep (more curl) | LINE sweep (straighten) |

**Mnemonic:** If rock is on the curl side, sweep to LINE (straighten it)

---

## ?? FILES CREATED

1. `AI_SWEEPER_ENHANCEMENT_PLAN.md` - Full implementation guide (5000+ words)
2. `AI_SWEEPER_ENHANCEMENT_SUMMARY.md` - Overview and benefits
3. `AI_SWEEPER_QUICK_REFERENCE.md` - This cheat sheet

---

## ?? TIME ESTIMATE

- Copy code: 10 minutes
- Test basic: 15 minutes
- Tune: 30 minutes
- **Total: ~1 hour**

---

## ? SUCCESS CRITERIA

- [ ] Draw shots sweep when falling short
- [ ] Takeouts correct lateral drift
- [ ] Elite sweepers more aggressive than rookies
- [ ] Logs show state changes
- [ ] Rock reaches intended target more often

---

## ?? GO!

1. Open `AI_SWEEPER_ENHANCEMENT_PLAN.md`
2. Copy the 4 methods into `AI_Shooter.cs`
3. Add StartCoroutine call
4. Test a draw shot
5. Watch the logs
6. Tune if needed

**You've got this!** ????
