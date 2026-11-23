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

## Not yet done / not verified

1. **Sweeping through the new path was not exercised.** `Sweep.cs` now moves the drag values on `RockSlide` instead of the Rigidbody. Play a shot and sweep it (weight, line, curl).
2. **Struck rocks.** A stationary rock now has the at-rest drag from `RockSlide`; the old behaviour relied on Box2D damping set by `Rock_Force`. Hit a rock and watch where it stops (the simulator predicts the struck rock with the same rule).
3. Mid-flight simulations (the AI sweeper re-predicting from a rock already past the hog line) restart the spin from the full release value, as the old simulator did.
4. `ShooterAnim` / `ShooterAnim_Testing` still set `rb.linearDamping` for the old animated shooter; `RockSlide` turns it back to 0 when the rock is thrown.
5. Constant-deceleration friction (`constantDecel`) exists in the model and is covered by a sanity test, but is **off**. Turning it on changes the weight -> distance map, so it needs the launch speeds recalibrated (see below).
6. The "Real-time" profile and the pause-menu checkbox are not built.

## Next (needs a decision)

Real ice slows at a roughly constant rate; ours decays exponentially, which is why every shot creeps for ~10 s and hog-to-hog is 4x too fast.
Proposed: add dry friction, and keep today's weight -> landing-spot map by remapping launch speed (`v_new` chosen so the rock stops where it does today),
so AI weights, FGZ rules and the tutorial keep working. The two speeds (Arcade, Real) are then two profiles, every time-dependent constant scaled together
(drag and spin fade by 1/S, speeds by 1/S, curl by 1/S, sweep amounts by 1/S, thresholds by 1/S), checked by the same parity tests.
