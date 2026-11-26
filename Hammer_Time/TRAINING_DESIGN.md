# Training: design outline (Nov 2026)

Status (Nov 25): **three tracks, merged to main.** The current design is the "Tier sheet (revised Nov 25)" and "Fuzz look" sections further down. The sections
from "How it works" to "Pull-back and the circles" are the original five-track first pass and are kept only for history (Directional Sweeping is now Sweep 1,
Cross Training is gone, the aim circle was replaced by the oval and crosshairs, prices are the fixed $1,000-$10,000 ladder). Numbers live in one table,
[Assets/Scripts/Training/TrainingCatalog.cs](Assets/Scripts/Training/TrainingCatalog.cs).

## How it works

- Five tracks, four levels each, bought in order. Every level costs the same in every track: **$1,000 / $2,500 / $5,000 / $10,000**
  (a full track is $18,500; everything is $92,500). Equipment tops out at $10,000 per item, so a level 4 is one top item's money.
- Levels are cumulative. Training belongs to the career (saved as `trainingLevels`, one number 0-4 per track), not to a player.
- Tap a track, a confirm box asks, then it pays and saves, exactly like buying gear.

## The tracks

### Sweep Training - stats
| Level | Name | Gives |
|---|---|---|
| 1 | Sweep Drills | +4 sweep endurance |
| 2 | Weight Training | +5 sweep strength |
| 3 | Synchro Sweeping | +5 sweep cohesion |
| 4 | Elite Sweep Camp | +4 endurance, strength and cohesion |

### Directional Sweeping - unlock
Level 1 unlocks carving (tap the left or right sweeper to push the rock sideways). **Default sweeping becomes both sweepers together**; this
track is how you earn the single-sweeper tap. Level = how hard the tap pushes: 0.5 / 0.75 / 1.0 / 1.25 of today's directional boost.

| Level | Name | Gives |
|---|---|---|
| 1 | Carving Basics | Unlocks carving, half strength |
| 2 | Edge Control | Carving pushes harder (0.75) |
| 3 | Quick Hands | Harder again (1.0) |
| 4 | Master Sweeper | Full carving (1.25) |

### Tactic Studies - collision UI
The collision direction indicator (the arrow showing where the struck rock goes). The level is how far off it may be.

| Level | Name | Indicator accuracy |
|---|---|---|
| 1 | Film Room | rough, up to 20 degrees off |
| 2 | Shot Charts | within 12 degrees |
| 3 | Angle Study | within 6 degrees, and the red collision warning line unlocks |
| 4 | Hit Analytics | exact |

### On-Ice Training - aim UI
The aim circle, tighter each level (diameter in world units; a rock is 0.28 wide). Level 3 is half of level 1, as you asked.

| Level | Name | Aim circle | Extra |
|---|---|---|---|
| 1 | Target Practice | 1.5 | |
| 2 | Release Drills | 1.1 | |
| 3 | Weight Control | 0.75 | +3 weight |
| 4 | Pinpoint | 0.5 | +3 weight, +3 aim |

Idea worth trying: make the circle's diameter the *honest* uncertainty of the shot (about two sigmas of the aim fuzz). Then the circle
shrinks with training **and** with skill and gear, and it always tells the truth about where the rock can finish.

### Cross Training - stats
| Level | Activity | Gives |
|---|---|---|
| 1 | Jogging | +4 sweep endurance |
| 2 | Yoga | +3 sweep strength, +3 sweep endurance |
| 3 | Rowing | +4 sweep strength, +4 sweep cohesion (team cohesion) |
| 4 | Pickleball | +4 aim, +4 finesse (hand-eye) |

Total stat bonus of everything: endurance +18, strength +21, cohesion +13, aim +7, finesse +4, weight +6. For scale, a top ($10k) item gives
about 60 points spread over three stats, so training is a modest top-up, not a replacement for gear.

## Equipment, with your new split

Your handle / head / slider / uniform descriptions already match how the stats are stored (handle and head: sweep stats, slider: weight, finesse, aim,
uniform: both halves). Not done yet: "decreases fatigue for each player" (there is no fatigue stat; the closest is sweep endurance) and the slider
"especially length" (that is the weight stat - the generator should favour it for sliders).

## Level gate (built)

The three tracks that unlock things (Directional, Tactic, On-Ice) also need a player (XP) level: **2 / 4 / 6 / 8** for their levels 1-4. The stat tracks
(Sweep, Cross) are cash only. A gated row shows "reach player level N" and cannot be tapped until you are there. The numbers are `UnlockLevelGate`
in the catalog.

## XP overhaul: my thoughts

Today: play -> XP -> level -> 5 skill points per level -> sliders on each player's stats. Cash buys gear. Training adds a second cash sink.

Three ways to relate them:

1. **Cash only** (what is built). Simple, but XP and training never touch.
2. **Cash plus a level gate** (recommended). A training level also needs a player level: say level 2 / 4 / 6 / 8 for levels 1-4. The UI tracks
   (Directional, Tactic, On-Ice) feel like things you *learn*, so XP fits them well; the stat tracks (Sweep, Cross) can stay cash only. Early
   levels come fast (100 XP, then x1.2 each), so the gate paces the player without a wall.
3. **Skill points buy training** (replace the stat sliders). Cleaner, one currency for growth, but it deletes the sliders players already
   understand and shifts the whole balance; I would try 2 first and only go here if the two systems feel redundant in playtests.

If you like 2, the work is small: a `requiredLevel` per level in the catalog and a locked look on the row.

## Pull-back and the circles (correction)

I said earlier the sideways aim was limited by the thumb. It is not: the rock's x is the finger's x divided by 16, so the whole screen width
maps to the rock's +-0.34 range, and the 0.34 limit is the real cap. So I did **not** build "relative drag". What I would try instead:
(a) drop the small thumb circle and let the coloured pull line do the work; (b) widen the 0.34 cap a little and see if the aim line
still reads well. Say the word and I will do (a) first, behind a switch so you can compare.

## What is wired

- Saved with the career (old saves load fine), the Training screen on the hub, buying with cash and the level gate.
- **Stats:** the bonuses are added to the three player-team sweepers (TeamManager) and to the shooter's aim and weight skill (the aim fuzz).
- **Aim marker:** hidden until Tactic Studies 1 (the guide lines show instead, as when the toggle is off). It is still the old circle at its normal size until the crosshairs are built.
- **Hit arrows:** hidden until Tactic Studies 3; then each rock's arrows are off by a random amount up to 12 degrees (rolled once per rock), exact at level 4. The red warning line comes at level 2.
- **Carving:** until Sweep Training 1, tapping one sweeper does the ordinary both-sweepers sweep; after that it carves (a flat 1.0x; the strength stat does the scaling).
- No career (tutorial, a game started straight from a scene) means nothing is gated; QuickTest owns every level.
- **Pause menu:** only the aids you have earned show (Trajectory, Guidelines and Curl Line are always there); AI tuning, debug switches and the physics buttons are under one collapsed "Tester options" button, dev builds only.
- **Finding:** equipment and sponsor bonuses (`modStats`) are only shown on the Team screen; no match reads them. Training goes through the match code, so it works, but gear does not yet.

Not done: icons for the tracks (empty slot at `Track_n/Panel/Image`), a "locked" hint in the pause menu, the equipment-bonus finding above, and a look at all of this on a device.

## Tier sheet (revised Nov 25: three tracks)

This replaces the five-track design above (Directional Sweeping became level 1 of Sweep Training, Cross Training was cut). Everything lives in the
`Levels` table of [TrainingCatalog.cs](Assets/Scripts/Training/TrainingCatalog.cs) (name, effect text, stat bonuses) and the small arrays under it
(`LevelCost`, `FirstLevelGate`, `CollisionErrors`, `LineReaches`). Prices are the same for every track; only level 1 of a track needs a player (XP) level (2).
Old five-track saves are converted on load (`MigrateLegacy`).

| Track | Lv | Name | Cost | Gives |
|---|---|---|---|---|
| Sweep Training | 1 | Directional Sweeping | $1,000 | carving: tap one sweeper |
| | 2 | Sweep Drills | $2,500 | +4 endurance |
| | 3 | Weight Training | $5,000 | +5 strength |
| | 4 | Synchro Sweeping | $10,000 | +5 cohesion |
| Tactic Studies | 1 | Film Room | $1,000 | pulsing crosshairs on the aim point (replaces the aim circle) |
| | 2 | Shot Charts | $2,500 | collision warning line |
| | 3 | Angle Study | $5,000 | hit arrows, up to 12 degrees off |
| | 4 | Hit Analytics | $10,000 | exact hit arrows |
| On-Ice Training | 1 | Target Practice | $1,000 | cycling dots on the line, which reaches the far hog line |
| | 2 | Release Drills | $2,500 | split and hog-to-hog timing ranges |
| | 3 | Weight Control | $5,000 | line reaches halfway to the house, +3 weight |
| | 4 | Pinpoint | $10,000 | the full line, +3 weight, +3 aim |

### Fuzz look (agreed, being built in steps)

- Always on, no training needed: a vertical guide, a faint shimmery cone along the trajectory, and a fuzzy end to the length line. The top and bottom of
  the fuzzy end are the largest error the throw could have; more skill (stats + gear + training) makes it crisper. The base error is scaled up so an
  untrained player really can't tell.
- The fuzzy end only shows when the predicted stop is inside the drawn line (On-Ice level sets how far that is); otherwise the line fades at its cap.
- The fuzzed aim is a real, slightly different throw (`AimFuzz.BendThrow`), so the line, contact, warning line and hit arrows agree with each other.
  Hit arrows start on each rock, show the velocity just after contact, scale with travel and are skipped for a rock that barely moves.
- Cycling dots: On-Ice 1. Crosshairs: Tactics 1, pulsing from the middle, never stretched into an ellipse. Timing ranges: On-Ice 2, top corner.
- Built (all on main): the track restructure and save migration, `TrainingAids` unlock flags, the glowing oval at the curled aim point (size from aim and
  weight fuzz), the cone along the path, pulsing crosshairs (Tactics 1), cycling dots (On-Ice 1) and the line reach (`LineReachY`), all tinted cold blue.
  Shaders: `Assets/Resources/Shaders/AimLight.shader` (cone) and `AimOval.shader` (oval). Look constants (`ConeStrength`, `OvalFill`, `OvalOutline`,
  `CrossStrength`) are public statics on `AimFuzzBands` and are live-tunable from Quick Test (press Q in a game).
- **Timing readout (built, not yet seen in play):** split (back line to near hog) and hog-to-hog in a top-left panel. Ranges (On-Ice 2) are centred on the
  drawn line's end, width from the weight fuzz, and assume no sweeping; the live clocks (everyone, AI throws too) sit beside them. They replace the clocks
  above the rock. A weight call ("7 - Button", free for everyone; table in `WeightNames.cs`) is named from the drawn line's end. Look: `Assets/Resources/TimingPanel.prefab` (made by `TimingPanelBuilder` when missing, never overwritten).
- Not verified on a device: dots marching and the line reach past the near hog (possible draw-order issue), the crosshair pulse, collision aids (Tactics 2-4)
  with the new oval.

## What each stat actually does in a match today

Gear and training are added together (`CareerManager.SkillBonus`); gear is recomputed from the four equipped items every time they change.

| Stat | Reaches | Does |
|---|---|---|
| Weight | the shooter (aim fuzz) | the line's length error, sigma 0.9 m x (1 - skill/100)^2 |
| Aim | the shooter (aim fuzz) | the line's sideways error, sigma 0.5 m x (1 - skill/100)^2 |
| Sweep strength | all three player-team sweepers | how much a sweep changes friction and curl |
| Sweep endurance | all three player-team sweepers | shorter sweep cooldown, slower fatigue |
| Sweep cohesion | the sweepers' stats | **nothing yet** - no match code reads it |
| Finesse | the AI's shot choices only | **nothing for the player yet** |

So Rowing's cohesion, Synchro Sweeping, and Pickleball's finesse do not change play until cohesion and finesse are given a job. Balance note: a $10k item is up to 60 stat points (about 20 on each of its three stats), and a player starts around 45, so four good items now add far more than a whole training track. That is a balance pass for once you have tuned the tiers.
