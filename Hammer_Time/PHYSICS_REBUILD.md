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

| Profile | Law | Pace | Button draw |
|---|---|---|---|
| Classic | the original exponential rock | game pace | 3.3 s hog to hog, stops after ~19 s |
| **Arcade** (default) | constant deceleration 0.929 u/s^2 + constant sideways acceleration | game pace | 3.3 s hog to hog, stops after ~8 s |
| Real-time | Arcade stretched in time by 4.42 | real ice | 14.4 s hog to hog (target 14.5), stops after ~37 s |

- A shot's **weight** is the old launch speed (5 light draw ... 16 hard takeout) and means "travels this far". `LaunchSpeedFromWeight` converts it for the profile in use; the aim line, the AI and the tutorial keep speaking weights. Launches are remapped in `Rock_Flick.Release` / `FlickShotController`, and `TrajectorySimulator` remaps the same way.
- Sideways acceleration = `1.335 * CurlMagnitude(weight) / weight` (the old curl table, fitted offline against the old rock). Arcade and Real stay within 6 cm of the old rock's path at every weight, in-turn and out-turn.
- Real-time is Arcade with every rate scaled together (speeds /S, accelerations /S^2, rates /S): the path is identical by construction. No `Time.timeScale` involved, so sweepers, UI and audio are untouched.
- Measured in Play, live rock vs simulator: identical to 0.00 under all three profiles (`AgentScripts/Measure.cs Profiles`).
- Sweeping keeps talking in the original drag units (`RockSlide.LinearDamping` 0.38, `AngularDamping` 0.32). Under the new law these scale friction by value/0.38 and curl by 0.32/value (the old curl was exactly proportional to 1/angular damping). Sideways nudges are scaled by `SpeedPerWeight`.
- The AI sweeper re-predicts from the rock's real state (`RockSlide.Snapshot` -> `SimulateTrajectoryFromState`). Speed thresholds (rest, "moving backward", tee sweeper) go through `RockPhysicsSettings.Speed()`.
- Tester options (the pause-menu physics button) are controlled by `DevOptions.Enabled` (Editor + Development builds only), so a release build has none. A release build uses `RockPhysicsBootstrap.Shipping` (Arcade).
- Flick shot mode is off (`GameVisualizationSettings.FlickModeAvailable = false`, toggle hidden); the tutorial follows the setting and uses the pull/release lessons.

## Not yet done / not verified

1. **Sweeping, AI play and a rock being struck under Arcade/Real were not exercised in Play** (unit tests and the unswept live harness only). Play a few ends against the AI in each profile and sweep.
2. Pause-menu button placement was not seen on screen (a test dialogue covered the screenshot). It exists, is labelled, and cycles; check the layout.
3. Rock-to-rock collisions are unchanged (Phase 3). A struck rock uses `restViscous` 0.9 + `restDryDecel` 0.15; this keeps old hit-rock distances to within ~10%.
4. The "drift off the aim line" report: unresolved. `ShotAudit` logs launch vs preview and the actual stop for every throw (search the console for `[ShotAudit]`).
5. Phases still to build: split-time display, aim-line fuzz from skill points.
6. `ShooterAnim` / `ShooterAnim_Testing` still set `rb.linearDamping` for the old animated shooter; `RockSlide` zeroes it at each throw.
