# Training: design outline (Nov 2026)

Status: the Training screen exists on the Arena_Selector hub (it took the Standings slot) and you can buy levels. **The levels do not change
gameplay yet** - see "What is wired" at the bottom. Everything below is a first pass for you to edit; numbers live in one table,
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
| 3 | Quick Hands | Harder again (1.0), reacts faster |
| 4 | Master Sweeper | Full carving (1.25) |

### Tactic Studies - collision UI
The collision direction indicator (the arrow showing where the struck rock goes). The level is how far off it may be.

| Level | Name | Indicator accuracy |
|---|---|---|
| 1 | Film Room | rough, up to 20 degrees off |
| 2 | Shot Charts | within 12 degrees |
| 3 | Angle Study | within 6 degrees |
| 4 | Hit Analytics | exact, plus the struck rock's whole path |

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

- Done: the data, the saving (old saves load fine), the Training screen on the hub, buying with cash, tests.
- Not yet: any effect. Stat bonuses need to reach the match (they should be added where `modStats` is used); the aim circle, collision
  indicator and carving still come from the pause-menu toggles, not from training; the "locked" look for aids; art for the track icons
  (an empty slot is at `Track_n/Panel/Image` in the scene).
