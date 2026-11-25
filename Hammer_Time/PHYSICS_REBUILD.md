# Rock physics: one model for the rock and the simulator

Branch `physics-timing` (not merged). Measurements are in `PHYSICS_PHASE0.md`.

## What changed

Before: the rock slid using Box2D damping plus hand-coded curl in `Rock_Force`; the aim preview and every AI shot used a
*separate* hand-tuned copy in `TrajectorySimulator` (damping 0.62, 0.05 s steps, a curl fudge factor, a 10 s cap). The two
disagreed by 5 to 8 units at the far end of the sheet, and the friction/curl/stop numbers were scattered across five files.

Now there is one model, `Assets/Scripts/Rock/Physics/RockPhysics.cs`:

| Piece | Job |
|---|---|
| `RockPhysicsProfile` | Every sliding number in one class: drag, dry friction (`constantDecel`), spin, spin fade, curl scale, mass, rest/sleep thresholds, curl-by-speed table. `Arcade()` = the game as it has always played. |
| `RockPhysics.Step` | One 0.02 s step of free sliding. Same order as the old `Rock_Force` + Box2D (read spin, torque, rest check, curl force, drag, move). |
| `RockSlide` (component) | Applies `Step` to the real `Rigidbody2D` every FixedUpdate. Added by `Rock_Force.Awake`, so every rock has it with no scene edits, and it runs even when `Rock_Force` is disabled (struck rocks still need friction). Box2D damping is off. |
| `RockPhysicsSettings.Current` | The profile in use. The rock, `Sweep`, and the simulator all read it. |
| `TrajectorySimulator` | Calls the same `RockPhysics.Step`. It keeps its rock-to-rock collision response; collisions now use a swept-circle test. |

## How we know nothing changed for the rock

- `RockPhysicsParityTests`: the model reproduces trajectories recorded from the live rock (3 weights, both turns) to 5 mm over 20 s.
- The full Phase 0 table was re-measured in Play with the live rock now driven by the model: every row identical.
- `TrajectorySimulator` rest points are within 1 cm of the live rock at 8 weights (were 5-8 units off).

## What *did* change in play

- The aim preview line and AI predictions are now accurate. `CalculateVelocityToTarget` (AI button draws, scenario placement) solves against the simulator, so the speeds it picks will change. **Play against the AI and check its shots.**
- `TrajectoryLine.iceFriction` / `curlStrength` in the Inspector no longer do anything (marked UNUSED). Change friction in `RockPhysicsProfile`.
- Rock_Force `turnValue` / `scaleFactor` are likewise unused (spin and curl scale are in the profile).

## Update: constant-deceleration physics is in, with three profiles

`RockPhysicsProfile` now has two laws and three ready-made profiles (`RockPhysicsBootstrap.Choose` / pause-menu button):

| Profile | Friction | Pace | Button draw |
|---|---|---|---|
| Classic | the original exponential rock | game pace | 3.3 s hog to hog, rests after ~19 s (long creep) |
| **Arcade** (default) | soft stop: deceleration = 0.5174 * speed^0.6, sideways pull fades with speed | game pace | 3.2 s hog to hog, rests ~10 s after the hog, last second slows by only 0.06 |
| Real-time | constant deceleration (speed^0), stretched in time by 4.42 | real ice | 14.35 s hog to hog (target 14.5), ~31 s hog to rest |

Why two shapes: at game pace a constant deceleration stops abruptly and flicks the rock sideways at the end (it looked wrong), so Arcade uses a soft landing.
At real pace the same constant deceleration is slow enough that the end is imperceptible, and it matches how real ice behaves, so Real-time uses it.
Both are fitted to the old rock's paths (within 2 cm for Arcade, 5 cm for Real-time). The stop curve is `stopPower` on the profile (0 = abrupt, 1 = endless creep; 0.7 is mellower than 0.6 at ~12 s, 0.5 firmer at ~9 s).

- A shot's **weight** is the old launch speed (5 light draw ... 16 hard takeout) and means "travels this far". `LaunchSpeedFromWeight` converts it for the profile in use; the aim line, the AI and the tutorial keep speaking weights. Launches are remapped in `Rock_Flick.Release` / `FlickShotController`, and `TrajectorySimulator` remaps the same way.
- Sideways acceleration = `0.5603 * CurlMagnitude(weight) * weight^-0.55 * (speed/releaseSpeed)^0.4` for Arcade (`1.335 * CurlMagnitude / weight`, constant, for the Real-time base), fitted offline against the old rock.
- Real-time is its base law with every rate scaled together (speeds /S, accelerations /S^2, rates /S; `WithTimeStretch`): the path is unchanged by construction (tested for each law). No `Time.timeScale` involved, so sweepers, UI and audio are untouched.
- Measured in Play, live rock vs simulator: identical to 0.00 under all three profiles (`AgentScripts/Measure.cs Profiles`).
- Sweeping keeps talking in the original drag units (`RockSlide.LinearDamping` 0.38, `AngularDamping` 0.32). Under the new law these scale friction by value/0.38 and curl by 0.32/value (the old curl was exactly proportional to 1/angular damping). Sideways nudges are scaled by `SpeedPerWeight`.
- The AI sweeper re-predicts from the rock's real state (`RockSlide.Snapshot` -> `SimulateTrajectoryFromState`). Speed thresholds (rest, "moving backward", tee sweeper) go through `RockPhysicsSettings.Speed()`.
- Tester options (the pause-menu physics button) are controlled by `DevOptions.Enabled` (Editor + Development builds only), so a release build has none. A release build uses `RockPhysicsBootstrap.Shipping` (Arcade).
- Flick shot mode is off (`GameVisualizationSettings.FlickModeAvailable = false`, toggle hidden); the tutorial follows the setting and uses the pull/release lessons.

## Update: what else landed (Nov 2026)

- **Launch speed loss fixed.** The three shooter bodies (body, torso, head) were Dynamic, mass 1 each, joined to the 145-mass rock by `RelativeJoint2D`; when `ShooterAnim` switched the joints on (~y -23.8) they took 2% of the rock's momentum, so every throw (all profiles) left the hack ~2% slow and draws finished ~0.7 short. `ShooterAnim.Start` now sets them to 0.01. Live check: hog-line speed 9.564 vs 9.566 expected. This was the "aim line is off" bug.
- **Spin direction.** The cosmetic spin under Arcade/Real had the wrong sign; it now turns the way the rock curls (`RockPhysics.CosmeticSpin`, shared with the simulator).
- **Aim-line fuzz** (`AimFuzz`, `TrajectoryLine.BeginFuzz`): the rock always goes where the physics says; the *drawn* line gets one error per rock (sideways + long/short, rolled once so re-aiming cannot re-roll), bent from nothing at the hack to the full error at the end, applied to the line, dots, aim circle, guide lines and collision marker. Sizes are the AI draw spreads read as metres at the end of the line: sigma 0.15 m sideways / 0.9 m weight at skill 0, falling with (1 - skill)^2, clamped at 2 sigma. Career starts at skill 45 (about 4.5 cm / 27 cm sigma, deliberately subtle); skill 85 is almost exact. Exact in the tutorial. `AimFuzz.Enabled` switches it off. Tune the two base spreads in `AimFuzz` if it should be more visible.
- **Split clocks** (`SplitTimer`, armed from `RockSlide.BeginFlight`; no scene edits): `00:00` above the rock counting from the hack (y -25) to the near hog (-16.15); then it bumps up, flashes 3 times and fades while a hog-to-hog clock runs to the far hog (y 0), which flashes 3 times and fades. Game time, line crossings interpolated inside the physics step, player and AI alike, follows the pace profile. `SplitTimer.Enabled` switches it off. Size, flash and fade constants are at the top of the file. (The old `RockTimerDisplay` is unused and has a wrong far hog.)
- **Rock-to-rock collisions** (Phase 3): see `PHYSICS_COLLISIONS.md`. `RockCollision` is Box2D's contact written out (restitution 0.99, friction 0.25 with spin, 1 m/s bounce threshold, all touching rocks solved together); the simulator plays a hit out with it, so the struck rock, a frozen partner and chains match the live game to 0.02 - 0.3 m (it used to send the struck rock about half as far). The dead frozen-rock override in `Rock_Colliders` is removed. Struck rocks' spin now fades with their speed under Arcade/Real.

## Not yet done / not verified

1. **Sweeping, AI play and a rock being struck under Arcade/Real were not exercised in Play** (unit tests and harnesses only). Play a few ends against the AI in each profile and sweep. **The AI plans takeouts from the simulator, whose struck-rock distance just roughly doubled (now correct): watch AI takeouts and frozen pairs.**
2. Pause-menu button placement was not seen on screen. It exists, is labelled, and cycles; check the layout.
3. Aim fuzz and the split clocks have unit tests only; their look (size, position, readability at zoom, flash timing) has not been seen on a device.
4. Collisions were measured for Arcade only; hits into the side boards are not simulated (the real rock bounces, then is out of play).
5. One Real-time throw stopped 2.5 m short of its own model prediction (possibly a profile switch mid-slide or an early sleep): not investigated.
6. `ShooterAnim_Testing` (unused test shooter) still has the old body masses.
7. `com.unity.pipeline` (agent tooling) is in the working copy's `Packages/manifest.json` but is not committed.
