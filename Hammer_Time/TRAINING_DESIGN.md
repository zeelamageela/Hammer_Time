# Training: design outline (Nov 2026)

Status: the Training screen exists on the Arena_Selector hub (it took the Standings slot), you can buy levels, and **the levels now change gameplay** - see "What is wired" at the bottom. Everything below is a first pass for you to edit; numbers live in one table,
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
- **Aim circle:** hidden until On-Ice Training 1 (the guide lines show instead, as when the toggle is off), then 1.5 / 1.1 / 0.75 / 0.5 wide.
- **Collision arrows:** hidden until Tactic Studies 1; then each rock's arrows are off by a random amount up to 20 / 12 / 6 / 0 degrees (rolled once per rock). The red warning line comes at level 3.
- **Carving:** until Directional Sweeping 1, tapping one sweeper does the ordinary both-sweepers sweep; after that the sideways push is 0.5 / 0.75 / 1.0 / 1.25.
- No career (tutorial, a game started straight from a scene) means nothing is gated; QuickTest owns every level.
- **Pause menu:** only the aids you have earned show (Trajectory, Guidelines and Curl Line are always there); AI tuning, debug switches and the physics buttons are under one collapsed "Tester options" button, dev builds only.
- **Finding:** equipment and sponsor bonuses (`modStats`) are only shown on the Team screen; no match reads them. Training goes through the match code, so it works, but gear does not yet.

Not done: icons for the tracks (empty slot at `Track_n/Panel/Image`), a "locked" hint in the pause menu, the equipment-bonus finding above, and a look at all of this on a device.

## Tier sheet (what is in the game today, to edit)

Everything below lives in the `Levels` table of [TrainingCatalog.cs](Assets/Scripts/Training/TrainingCatalog.cs) (name, effect text, stat bonuses) and
the small arrays under it (`LevelCost`, `UnlockLevelGate`, carving strength / indicator error / aim-circle size). Prices are the same for every track.
The player-level gate applies only to the three tracks marked "unlock".

| Track | Lv | Name | Cost | Needs player level | Gives |
|---|---|---|---|---|---|
| Sweep Training | 1 | Sweep Drills | $1,000 | - | +4 endurance |
| | 2 | Weight Training | $2,500 | - | +5 strength |
| | 3 | Synchro Sweeping | $5,000 | - | +5 cohesion |
| | 4 | Elite Sweep Camp | $10,000 | - | +4 endurance, strength, cohesion |
| Directional Sweeping (unlock) | 1 | Carving Basics | $1,000 | 2 | carving on, 0.5x push |
| | 2 | Edge Control | $2,500 | 4 | 0.75x push |
| | 3 | Quick Hands | $5,000 | 6 | 1.0x push |
| | 4 | Master Sweeper | $10,000 | 8 | 1.25x push |
| Tactic Studies (unlock) | 1 | Film Room | $1,000 | 2 | hit arrows, up to 20 degrees off |
| | 2 | Shot Charts | $2,500 | 4 | within 12 degrees |
| | 3 | Angle Study | $5,000 | 6 | within 6 degrees, collision warning line |
| | 4 | Hit Analytics | $10,000 | 8 | exact |
| On-Ice Training (unlock) | 1 | Target Practice | $1,000 | 2 | aim circle, 1.5 wide |
| | 2 | Release Drills | $2,500 | 4 | 1.1 wide |
| | 3 | Weight Control | $5,000 | 6 | 0.75 wide, +3 weight |
| | 4 | Pinpoint | $10,000 | 8 | 0.5 wide, +3 weight, +3 aim |
| Cross Training | 1 | Jogging | $1,000 | - | +4 endurance |
| | 2 | Yoga | $2,500 | - | +3 strength, +3 endurance |
| | 3 | Rowing | $5,000 | - | +4 strength, +4 cohesion |
| | 4 | Pickleball | $10,000 | - | +4 aim, +4 finesse |

## What each stat actually does in a match today

Gear and training are added together (`CareerManager.SkillBonus`); gear is recomputed from the four equipped items every time they change.

| Stat | Reaches | Does |
|---|---|---|
| Weight | the shooter (aim fuzz) | the line's length error, sigma 0.9 m x (1 - skill/100)^2 |
| Aim | the shooter (aim fuzz) | the line's sideways error, sigma 0.15 m x (1 - skill/100)^2 |
| Sweep strength | all three player-team sweepers | how much a sweep changes friction and curl |
| Sweep endurance | all three player-team sweepers | shorter sweep cooldown, slower fatigue |
| Sweep cohesion | the sweepers' stats | **nothing yet** - no match code reads it |
| Finesse | the AI's shot choices only | **nothing for the player yet** |

So Rowing's cohesion, Synchro Sweeping, and Pickleball's finesse do not change play until cohesion and finesse are given a job. The sweepers also get the
shooter-side stats added to a player of base 45: a $10k item is up to 60 points, so four good items can add far more than a training track; the
numbers want a balance pass once you have tuned the tiers.
