# Rock-to-rock collisions: measured (Phase 3, step 1)

Harness: `AgentScripts/Collide.cs` (`Collide.Run`, run in Play). A shooter, aimed with the simulator so the impact offset is known, hits a
stationary rock in the real game (Box2D + RockSlide + Rock_Colliders, Arcade soft stop); the same shot is run through TrajectorySimulator.
Nothing was changed to get these numbers.

## What the game does (Box2D, rock material bounce 0.99 / friction 0.25, equal masses 145)

Speeds are fractions of the shooter's speed just before impact, read two physics steps after the hit.

| offset from centre | shooter keeps | target gets | shooter exit angle | target exit angle | target spin (deg/s) |
|---|---|---|---|---|---|
| 0 (head on) | 0.014 - 0.021 | 0.95 - 0.96 | (stops) | 0 | -24 .. -65 |
| 0.05 m | 0.13 - 0.14 | 0.94 | 77 | -8 | -66 .. -222 |
| 0.12 m | 0.34 - 0.35 | 0.86 - 0.87 | 62.5 | -20 | -124 .. -440 |
| 0.20 m | 0.59 - 0.60 | 0.64 - 0.65 | 41 | -36 | -189 .. -692 |
| 0.27 m (graze) | 0.83 - 0.91 | 0.21 - 0.35 | 12 - 20 | -55 .. -64 | -110 .. -260 |

Nearly elastic (a head-on hit leaves the shooter stuck and passes ~96% to the target). Angles and speeds sit a little off the frictionless
answer (shooter slower, target angle smaller) and the struck rock picks up spin: that is the 0.25 contact friction.

Frozen pair (partner touching, straight behind the target), head-on: target keeps 0.02 - 0.08, **partner leaves at 0.95** of the shooter's speed
(a Newton's cradle), shooter sticks. Box2D already does this on its own.

## What the aim-line simulator does

Normal components swapped, then **both rocks' whole velocity x 0.85 x 0.7 = 0.595**, tangential part included, no contact friction, no spin,
and no knowledge of a rock behind the struck rock.

Result against the real game (rest positions, metres):

| case | struck rock | shooter |
|---|---|---|
| head on, w8.5 / w11 / w14 | goes 52 - 56% as far as the real one (0.7 / 1.7 / 2.7 m short) | within 0.02 - 0.05 |
| 0.12 offset, w8.5 / w11 / w14 | 0.6 / 1.5 / 2.4 m short | 0.25 / 0.8 / 1.6 off |
| 0.20 offset | 0.4 / 1.1 / 1.7 m short | 0.6 / 1.9 / 3.5 off |
| graze 0.27, w14 | 0.1 | **8 m off** (sim shooter loses ~40% speed in a graze; the real one keeps 91%) |
| frozen pair, head on | sim moves the struck rock 0.7 - 3.4 m; the real one stays put | partner is not modelled at all: it really travels 1.3 - 6.4 m |

## Frozen-rock override in Rock_Colliders

`OnCollisionEnter2D` has a rule (struck 0.15 / partner 0.65 / shooter -0.20 of impact speed). It **never fired**: 0 of 7 staged frozen hits here,
and 0 times (0 pairs detected) in the whole editor log, which holds 56 rock hits. The likely reason is that the callback runs after Box2D has
already resolved the hit, so the "target was stationary" test fails; not confirmed. Either way Box2D alone already gives the right answer.

## Not yet checked

- Whether a struck rock's spin ever decays under Arcade/Real (RockSlide only drives spin for thrown rocks).
- Chains: a struck rock that goes on to hit a third rock.
- Classic and Real-time collision numbers (the hit itself is the same Box2D; the slide after it differs).

## After the fix (Phase 3, steps 2 and 3)

Restitution kept at the live value, 0.99 (decided: keep the feel, fix the simulator).

- `RockCollision` (new): the same contact as Box2D for equal rocks: restitution 0.99, friction 0.25 (with spin), no bounce under the 1 m/s
  bounce threshold, all touching rocks solved together with 8 sweeps. Constants are tested equal to `Rock Material.physicsMaterial2D`.
- `TrajectorySimulator` plays a hit out with it: the struck rock, a rock frozen behind it, or a chain all slide and collide until they stop.
  `CollisionInfo.otherRocksMoved` lists the other rocks the hit sets moving.
- A pair counts as touching if it overlaps (Box2D has no contact otherwise): a rock a hair away is hit afterwards, one pair at a time.
- Removed the frozen-rock override in `Rock_Colliders` (never fired). With a touching pair Box2D itself sends the shooter back at about
  0.32 x its speed and the middle and back rock on together at about 0.64 x; the simulator reproduces it.
- Struck rocks' spin now fades with their speed under Arcade/Real (it used to be left alone), so it stops with the rock.

Simulator vs the real game after the fix (rest positions, same harness): head-on single hit, struck rock within 0.02 - 0.10 m (was 0.7 - 2.7 m);
hits up to 0.2 m off centre, 0.02 - 0.3 m; frozen pair, head on and 0.1 m off, 0.00 - 0.32 m. Shooters that glance into the side boards are not
comparable: the real rock bounces off the boards (and is out of play), the simulator has no boards.

Not done: AI-vs-AI games were not replayed with the corrected simulator (the AI plans takeouts from its predictions, so its choices can change).
