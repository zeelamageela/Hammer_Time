# Physics timing — Phase 0 (measurement only, no gameplay changes)

Branch `physics-timing`. Harness: `AgentScripts/Measure.cs` (run with `unity command run_script`; needs the TournyGame scene in Play mode).
Method: the real rock object (`Rock_Force` / `Rock_Release` / hog-line trigger) is launched from the hack with velocity set directly,
exactly as `Rock_Flick.Release` does, with `Physics2D.Simulate(0.02)` stepped by hand (and `Rock_Force.FixedUpdate` invoked before each step).
Checked against a live shot through Unity's own loop: final rest `(-0.941, 5.311)` matches to 3 decimals, intermediate positions agree.
Repeat runs are identical to the last digit; in-turn is the exact mirror of out-turn. No sweeping in any run.

## Geometry and constants (as found)

| Thing | Value |
|---|---|
| Hack / `Launcher` | y = -25 |
| Near hog line (trigger, 0.25 tall) | y = -16.15 (release / damping starts when the rock exits it) |
| Far hog line | y = 0 (`SESSION_HANDOFF.md`); `InPlay_Collider` spans y 0.28 to 8.01 |
| Tee line / button | y = 6.5 |
| Near hog -> far hog | 16.15 units (real sheet: 21.945 m) — the house end is ~1:1 (hog->tee 6.5 vs 6.4 m) but the hog-to-hog span is ~26% shorter than real |
| Rock | mass 145, `linearDamping` 0.38 (0 until the near hog), `angularDamping` 0.32, gravityScale 0, bounciness 0.99, friction 0.25 |
| Fixed step | 0.02 s, timeScale 1, Box2D velocity threshold 0.001, sleep tolerance 0.01 |
| Launch speed range (`TrajectoryLine`) | 5 to 16 per second (pull-back 1 to 2.75 -> speed) |
| Rest rule | `Rock_Force`: speed < 0.01 -> `rest`, damping jumps to 0.90 |

## Results (seconds; start at the hack; NH = near hog, FH = far hog)

| v0 | hack>NH | NH>FH | NH>tee | speed < 0.1 | rest (<0.01) | speed at NH | speed at FH | x at FH | path from hack | rests at |
|---|---|---|---|---|---|---|---|---|---|---|
| 5.0 | 1.77 | never | never | 12.36 | 18.52 | 5.00 | – | – | 22.4 | y -2.67 |
| 6.0 | 1.47 | 16.65 | never | 12.50 | 18.64 | 6.00 | 0.01 | -1.01 | 25.1 | y 0.01 |
| 7.0 | 1.26 | 5.17 | never | 12.66 | 18.78 | 7.00 | 1.01 | -0.52 | 27.7 | y 2.64 |
| 8.0 | 1.11 | 3.68 | never | 12.82 | 18.94 | 8.00 | 2.02 | -0.34 | 30.4 | y 5.31 |
| **8.5 (button draw)** | 1.04 | **3.28** | 16.24 | 12.90 | **19.00** | 8.50 | 2.48 | -0.29 | 31.6 | y 6.52 |
| 9.35 (AI draw) | 0.95 | 2.76 | 6.33 | 13.04 | 19.14 | 9.35 | 3.32 | -0.23 | 33.8 | y 8.74 |
| 10.0 | 0.88 | 2.46 | 5.00 | 13.12 | 19.24 | 10.00 | 3.97 | -0.15 | 35.5 | y 10.49 |
| 12.4 (AI takeout) | 0.71 | 1.78 | 3.07 | 13.50 | 19.58 | 12.40 | 6.37 | -0.05 | 41.8 | y 16.78 |
| 13.75 (AI peel) | 0.64 | 1.53 | 2.54 | 13.70 | 19.80 | 13.75 | 7.79 | -0.03 | 45.5 | y 20.51 |
| 15.8 (AI runback) | 0.56 | 1.29 | 2.06 | 13.96 | 20.06 | 15.80 | 9.73 | -0.02 | 50.7 | y 25.72 |

(No boards/other rocks in these runs, so fast shots run on past where they would be stopped in a game. Out-turn curls left, in-turn right, same size.)
More rows (5.5, 6.5, 7.5, 9, 9.5, 11, 12, 14, 16) are in the script output; they interpolate smoothly.

## What the numbers say

1. **Hog-to-hog is about 3.3 s for the button draw, versus ~14.5 s real: roughly 4.4x too fast.** Stronger shots are faster still (takeout 1.8 s).
2. **Time to stop is already close to real: ~13 s to crawl (<0.1) and ~19 s to rest, whatever the weight.** The reason: damping is *exponential* (`v = v0·e^(−0.38 t)`), so every shot spends its last ~10 s creeping, and "time to rest" barely depends on weight. Real ice is closer to *constant deceleration*, where heavier shots take longer overall and the end of a draw is brisk.
3. **So scaling time uniformly cannot match both ends.** It would give 14.5 s hog-to-hog and an 80+ s stop. Matching real timing and the same path needs a *friction shape* change (constant-decel term plus a small viscous term), not only a scale. This is the main thing to decide in Phase 2.
4. **Curl is weight-dependent by design** (`Rock_Force.Release`: curl magnitude 0.55 to 0.12 by launch speed, spin from one torque impulse, spin decays with angular damping 0.32). Curl pushes sideways in proportion to spin, so it must be scaled in step with friction to keep the same path.

## Dependencies on current speed, friction, damping and collision values

**Rock physics proper (`Rock_Force.cs`, `Rock_Release.cs`, `Rock_Flick.cs`, `FlickShotController.cs`)**
- Launch speed set directly (`rb.linearVelocity = v`), damping 0 until the near hog line, then 0.38 / 0.32 (hard-coded in `Rock_Force.Release`, and again as `originalLinearDamping = 0.38` / `0.32` in `Sweep.cs`).
- Curl: force = `curl.x · angularVelocity · scaleFactor(0.1)`, `curl.x` from a launch-speed table; spin from `AddTorque(turnValue 60 deg2rad)`.
- Rest rule: speed < 0.01 -> `rest`, damping 0.90. Absolute velocity thresholds also in `Rock_Colliders` (stationary < 0.1) and `Rock_Info`.
- `Rock_Colliders.OnCollisionEnter2D` overrides velocities for *frozen-rock* hits with fixed ratios (struck 0.15, partner 0.65, shooter -0.20 of impact speed): ratios, not absolute speeds.

**Sweeping (`Sweep.cs`, `AI_Sweeper.cs`)**
- Sweeps subtract an absolute amount from linearDamping (e.g. `sweepAmt · strength · 0.035` weight sweep, `0.3` hard sweep, `0.015` line/curl) and add absolute lateral velocity (`·0.01–0.02`); angular damping changes by absolute amounts (+0.25, +0.2, +0.10, −0.10). Durations: `sweepTime`, `sweepTapDuration 0.4 s`. All of these would need scaling with any friction or time change.

**A second copy of the physics: `TrajectorySimulator` (`UI/TrajectorySimulator.cs`)**
- Used by the player's aim preview (`TrajectoryLine`), by every AI shot (`AI_Target` ~40 call sites, `AI_Shooter`, `AI_Sweeper`, `RandomRockPlacement`).
- Hard-coded: `linearDamping 0.62` (`TrajectoryLine.iceFriction`), `TIME_STEP 0.05`, `MAX_SIMULATION_TIME 10 s`, `angularDamping 0.32`, curl divided by a fudge `curlEffectiveMass 19.96` (real mass 145), `RESTITUTION 0.85`, `COLLISION_DAMPING 0.7`, speed gates (`> 0.1`, `< 0.01 -> 0.90`).
- It is a separate model, not the Rigidbody. Any physics change needs this updated too, or AI shots and the preview line will land short or long.

**AI shot weights (`AI_Target.cs`)** — shots are chosen as pull-back x `velocityMultiplier (5)`: Draw 9.35, Takeout 12.4, Peel 13.75, Runback 15.8 (comments "recalibrated v2"), then verified through `TrajectorySimulator`. Weights are absolute speeds, so they change if speed/friction changes (or must be defined as fractions of a scaled range).

**Tutorial** — no scripted velocities. `TutorialSequenceManager` steps use `RockReleased`, `RockStopped`, `RockReachedYPosition` (e.g. takeout step at Y ≤ -28), `WaitForSeconds` (uses real time), and per-step `timeScale` (0, 1, or custom). So the tutorial depends on positions and timing windows, not on speeds; slower rocks lengthen "wait for stop" steps.

## Open question found while measuring (needs your call)

`TrajectorySimulator` (damping 0.62) and the real rock (0.38 after the hog) disagree a lot. For a launch of 8.0 the simulator's path ends around y = -0.16; the live rock ends at y = 5.31. For the AI draw (9.35) the simulator ends at 2.18, the live rock at 8.74. The simulator comments say the 0.62 was tuned when the rock "travels ~60% of the distance 0.38 predicts", which looks like it predates the later change that keeps damping at 0 until the near hog. I did not find any extra drag in normal play (`Sweep` only restores 0.38 after a sweep). Before Phase 2 I should confirm in real play (an actual AI draw and a player draw, unswept) whether the rock really goes as far as the harness says. If it does, the preview line and the AI are both currently calibrated to the wrong physics, and that is worth fixing independent of timing.

## Not measured yet
- Rock-to-rock collision speeds in/out (Phase 3 harness).
- Sweeping effect on time/distance (needed to fit the scaling formula in Phase 2; same harness can do it).
