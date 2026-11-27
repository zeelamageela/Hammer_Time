# Hammer Time — Session Handoff

Paste this at the start of a new session to restore context.

---

## Project

Unity curling simulation (iOS/Android). Core loop: player throws rocks, sweepers affect trajectory, score if closest to center of house. 8 ends, 8 rocks per team.

**Key scenes:** SplashMenu → CareerSettings → Arena_Selector → TournyGame / TutorialGame

**Key singletons (DontDestroyOnLoad):** `GameSettingsPersist` (gsp), `CareerManager` (cm)

**Save system:** JSON to `Application.persistentDataPath/career_save.json` via `CareerSaveService`

**Shot style flag:** `GameVisualizationSettings.Instance.FlickShotMode` (persisted in PlayerPrefs as `"FlickShotMode"`) — `true` = flick shot, `false` = pull/release

---

## World space coordinates

- Hog line: Y = 0
- Tee line: Y = 6.5
- House center: (0, 6.5), radius = 1.5
- Free Guard Zone: Y between 0 and 6.5, distance from (0, 6.5) > 1.5 (between hog and tee, not in house)

---

## Shot flow

1. `GameManager.RedTurn()` / `YellowTurn()` starts — enables rock components, waits for `rockInfo.shotTaken == true`
2. Player releases → `Rock_Flick.Release()` applies velocity to `rb.linearVelocity`
3. `GameManager` sees `released == true` → calls `SweeperManager.Release()` → activates sweepers → calls `ShotLocation()` → `AI_Sweeper.OnSweep(false, shotType, ...)` → `PlayerSpeed()` coroutine starts (sweep coaching callouts)
4. Rock slides; `Rock_Force.FixedUpdate()` applies curl, detects velocity < 0.01 → sets `rest = true`
5. `GameManager` runs `AllStopped()` → `CheckScore()` → `NextTurn()` increments `rockCurrent`

**`rockCurrent` is ALWAYS an absolute index 0–15 into the full 16-rock end.** `rocksPerTeam` does NOT shorten the end — it controls how many rocks per team are actually thrown live. Rocks `0` through `16 - rocksPerTeam*2 - 1` get pre-placed as "already played" (see `RandomRockPlacerment.cs`), and live play starts at `firstPlayerRock = 16 - (rocksPerTeam * 2)`. Any rule keyed off absolute rock position (FGZ window, AI `phase` early/middle/late) must compare against `rockCurrent` directly, never relative to `firstPlayerRock` — otherwise a short practice game silently re-triggers rules that should already be over.

**Note:** `Rock_Flick.ShotLocation()` is dead code — only `SweeperManager.ShotLocation()` is called.

---

## Free Guard Zone / "5 rock rule" — IMPLEMENTED (Aug 14–15 2026)

**Rule:** first 5 rocks of the end (absolute `rockCurrent < 5`), an opponent's rock sitting in the FGZ (not in house, Y ≤ 6.5) cannot be knocked out of play. Violation: offending rock removed, every other displaced rock restored to its pre-shot position.

**Important:** the window is 5 rocks and is absolute — it was originally built as a 4-rock window relative to `firstPlayerRock`, which was wrong (see "rockCurrent" note above) and got corrected after direct user clarification.

**GameManager.cs `#region Free Guard Zone`:**
- `IsFreeGuardZoneWindow()` (public) — `rockCurrent < 5`, false in tutorial games
- `IsProtectedFreeGuardZoneRock(rockIndex, attackingTeamName)` (public) — opponent rock, inPlay, in the zone. Also used by AI strategy (see below)
- `SnapshotFreeGuardZone(throwingTeamName)` — called right before each shot can move (`RedTurn()`/`YellowTurn()`), snapshots every in-play rock's position/rotation/sprite
- `CheckFreeGuardZoneViolation()` — called from `CheckScore()` right after `AllStopped()`. Violation trigger = protected rock actually knocked **out of play** (`outOfPlay == true`), not just nudged — matches real curling rules

**Penalty execution (`Rock_Colliders.cs`):**
- `ForceOutOfPlay()` — removes the offending rock, reuses the existing removal coroutine/animation
- `RestoreForFreeGuardZone(position, rotation, sprite)` — puts a displaced rock back exactly as it was, including a rock that was mid-way through its out-of-play animation. **Must pass the pre-shot sprite** — disabling the Animator alone leaves the SpriteRenderer stuck on whatever frame the animation reached; the sprite has to be explicitly reset too.

**Feedback:** `TextCalloutManager.Instance.ShowCallout(pos, "Free Guard Zone Violation!", alwaysShow: true)` — **not** `gHUD.Message()`. `CheckScore()` overwrites `mainDisplay` with the "X is Sitting Y" house review a few lines after the FGZ check runs, which silently stomps a `gHUD.Message()` before the player ever sees it.

**AI awareness:** `AI_Strategy.ExecuteShot()` is the one chokepoint every `ShotIntent.RemoveThreat` call routes through (~80 call sites in that file). Added one veto check there: if `gm.IsProtectedFreeGuardZoneRock(targetRock, activeTeamName)`, redirect to `ShotIntent.ScorePoints` (draw) instead of attempting the illegal removal. No distinct "tap" shot type exists — this reuses the existing draw logic, not a new gentle-tap execution path. If true tap behavior is wanted later, that's a separate feature touching `AI_Target.cs`'s execution layer.

**Not yet comprehensively tested** — multi-rock chain reactions, end-of-end boundary, and flick shot mode specifically still need verification.

---

## Callout system

`TextCalloutManager` (DontDestroyOnLoad singleton) — call `Instance.ShowCallout()` or `Instance.ShowRockCallout()`.

- By default, `ShowCallout()` is gated on `gsp.debug`. Pass `alwaysShow: true` to bypass.
- `ShowRockCallout()` also accepts `alwaysShow: true`.
- **Always-on callouts:** sweep coaching (`AI_Sweeper.ShowSweepCallout()`), flick speed feedback (`FlickShotController` — "Perfect!", "Too Fast", etc.), FGZ violation message.
- **Debug-gated:** everything else (AI strategy, raw velocity data, position analysis, timer display)
- Debug toggle in PauseMenu (`PauseMenu.debugToggle`) — wired and working, UI Toggle already set up in the editor.
- `TutorialSequenceManager.CheckAutoStartTutorials()` now also skips when `gsp.debug == true` (a QuickTestGame-only flag, never true for real players) — previously a QuickTestGame session could unexpectedly trigger the shooting tutorial's dialogue since it looks identical to a fresh player's first game.

---

## Multi-camera setup

Two visually similar but functionally distinct cameras — easy to confuse, don't assume:

| Camera | Purpose | Notes |
|---|---|---|
| `house` | Full-screen, aimed at the house | `HouseView()` (toggle) / `HouseViewOn()` / `HouseViewOff()` (explicit). Used for the post-shot "who's sitting" review in `CheckScore()` and by `TutorialSequenceManager` for specific spotlight steps (e.g. `TakeoutDrag`). Has worked for years — if it doesn't appear, check the Camera component's **Enabled** checkbox in the Inspector before assuming a code bug (found disabled once this session, looked identical to a logic bug). |
| `aim` (GameObject `AimCamera`) | Small **inset** overlay on the regular follow/main view | Already driven dynamically and correctly by `CameraManager.Trajectory()`, called every frame from `Rock_Flick`'s drag loop (`TrajectoryLine.DrawTrajectory()`). Turns on only while a human is actively pulled back aiming (`trajTarget.position.y > 0`), off otherwise. This was already correct before Aug 2026 — don't add new code to manage it without first checking whether `Trajectory()` already covers the case. |

Normal depths: `main`=1, `top`=2, `aim`=-1 (inactive) / 3 (active), `ui`=3 / `aim.depth+1` when aim active, `house`=-1 (inactive) / 5 (active, `ui`=6).

`MainDisplay` (`GameHUD.mainDisplay`) turns off the instant a player grabs the rock (`Rock_Flick.isPressed`), not just at release.

Spotlight canvas is **Screen Space - Camera** mode using the `ui` camera (forced in code in `SetupStep()`). This ensures it renders over everything including the aim camera view.

---

## FlickShotController — fixed duplicate visual elements (Aug 15 2026)

Root cause: `Start()` unconditionally created 4 new GameObjects (`SwipeTrail`, `PredictedStopLine`, `InputZoneBorder`, `VelocityGuide`) every time it ran. That component lives on every rock (16 per game), so a full game left 16 orphaned sets in the scene instead of 1.

Fix: added 4 `private static` backing fields. `Start()` now only creates the GameObjects the first time (`if (sharedSwipeTrailLine == null)`); every other rock's `Start()` points its instance fields at the same shared set. Relies on Unity's null-check behavior for destroyed objects — when the scene reloads for a new end, the old shared GameObjects get destroyed and `== null` correctly evaluates true, so a fresh set gets created for the new scene.

---

## Tutorial system architecture

### TutorialGameManager (TutorialGame scene only)
- Reads `GameVisualizationSettings.Instance.FlickShotMode` and resolves `activeSetup` to either `tutorialSetup` (PRTutorialGameSetup) or `flickShotTutorialSetup` (FlickTutorialGameSetup 1).
- `ShouldTriggerTutorial(rockCurrent, out tutorialId)` — iterates `activeSetup.tutorialTriggers[]` and returns the matching `tutorialId`. Entries with `triggerOnFirstPlayerRock = true` use the live-computed first-player rock index (safety net against hammer/AI flag changes). Called from `GameManager.RedTurn()` / `GameManager.YellowTurn()` before `WaitUntil shotTaken`.

### TutorialGameSetup (ScriptableObject)
Fields that matter:
- `rocksPerTeam`, `startingEnd`, `startingScore`, `playerHasHammer`
- `prePlacedRocks[]` — rocks positioned before the tutorial starts
- `tutorialTriggers[]` — list of `{ tutorialId, rockIndex, triggerOnFirstPlayerRock }`. Adding a new tutorial type is just adding a row here; no code changes needed.
- `aiShotSuggestions[]` — override AI shot type per rock index

### TutorialSequence (ScriptableObject)
- `sequenceId` — must match what `tutorialTriggers[].tutorialId` references
- `steps[]` — ordered `TutorialStep` assets
- `autoStart` / `autoStartCondition` — for `CheckAutoStartTutorials()` at scene Start (mostly used by shooting tutorials)
- `chainSequenceId` — sequence to play automatically when this one completes. Used so SweepTutorial runs at end of both shooting tutorials without duplication.

### TutorialStep (ScriptableObject)
Key fields:
- `startCondition` / `endCondition` — `TutorialConditionType` enum (None, WaitForClick, MouseReleased, WaitForSeconds, RockGrabbed, RockBeingDragged, RockPullbackThreshold, RockReleased, RockStopped, RockReachedYPosition, GameStateChange)
- `rockIndex` — `-1` = current rock
- `targetYPosition` / `targetYAbove` — for `RockReachedYPosition` condition
- `pauseGame` / `timeScale`
- `spotlightTargetName` — keyword or name: `$currentRock`, `$shooter`, `$launcher`, `$aimTarget`
- `spotlightWorldOffset` — world-space offset applied to spotlight position before screen projection; tracks dynamically. e.g. `(0, -1, 0)` = one unit below the rock
- `dynamicSpotlight` — re-projects the cutout every frame (required for moving targets)
- `useSpotlightWorldPosition` / `spotlightWorldPosition` — fixed world point (e.g. house centre)
- `branchCondition` / `onSuccessStep` / `onFailureStep` — branching/retry support (infrastructure exists, **paused** — not currently being worked on)

---

## Tutorial sequence map (current)

### PRTutorialGameSetup — Pull/Release mode
`rocksPerTeam: 2`, rocks 13 (player) + 14 (player), `playerHasHammer: false`

| Rock | Tutorial triggered |
|---|---|
| 13 | PullReleaseTutorial (`triggerOnFirstPlayerRock: true`) |
| 14 | TakeoutTutorial |

### FlickTutorialGameSetup 1 — Flick Shot mode
`rocksPerTeam: 6`, player rocks 3 and 5

| Rock | Tutorial triggered |
|---|---|
| 3 | FlickShotTutorial (`triggerOnFirstPlayerRock: true`) |
| 5 | TakeoutTutorial |

---

## Tutorial sequences — step breakdown

### PullReleaseTutorial → chains to SweepTutorial
`DrawIntro → DrawGrab → DrawDrag → DrawRelease`
- Ends at DrawRelease: fires when rock reaches Y ≥ -24 (just released, rock still moving). On completion, SweepTutorial starts immediately while rock is in flight.

### FlickShotTutorial → chains to SweepTutorial
`FlickDrawIntro → DrawGrab → DrawDrag → FlickShooterClick → FlickSwipe → DrawRelease`
- Same chain behaviour.

### SweepTutorial (standalone, `autoStart: 0`)
`SweepIntro → SweepWeight → SweepLine → SweepCurl → SweepOutro`

| Step | startCondition | Spotlight | Notes |
|---|---|---|---|
| SweepIntro | RockReachedYPosition Y≥-5 | off | "To alter your rock's path, you'll need to SWEEP!" Fires while rock is mid-flight. |
| SweepWeight | None (immediate) | $currentRock offset (0,-1,0) | "TAP below the rock to sweep with BOTH sweepers." |
| SweepLine | RockReachedYPosition Y≥-4.5 | $currentRock offset (-1,0,0) | "TAP the sweeper INSIDE the curl to sweep LINE." |
| SweepCurl | None (immediate) | $currentRock offset (1,0,0) | Curl tip — spotlight right of rock |
| SweepOutro | None (immediate) | off | Outro wrap-up, pauseGame |

### TakeoutTutorial
`TakeoutIntro → TakeoutTurn → DrawGrab → TakeoutDrag`

| Step | startCondition | Notes |
|---|---|---|
| TakeoutIntro | CameraStoppedMoving | Intro before shot, no pause |
| TakeoutTurn | None | Turn selection UI explanation |
| DrawGrab | None | Shared step — grab the rock |
| TakeoutDrag | RockReachedYPosition Y≤-28 | nonBlocking dialogue, spotlight on house (0, 6.5) dynamic. Fires just after release. |

---

## Spotlight world offset — how it works

`TutorialStep.spotlightWorldOffset` (Vector3) is added to the resolved world position before `WorldToScreenPoint` in both the initial `SetupStep()` call and the per-frame `DynamicSpotlightUpdate()` coroutine. Works for Transform targets (`$currentRock` etc.) and fixed-world-position targets. Tuned in Play mode with `dynamicSpotlight: true`.

---

## Key files

```
Assets/Scripts/GameManager.cs                             — main game loop, RedTurn/YellowTurn/CheckScore/NextTurn, #region Free Guard Zone
Assets/Scripts/GameSettingsPersist.cs                     — DontDestroyOnLoad state singleton; always use .instance, never GameObject.Find
Assets/Scripts/Tourny/TournyManager.cs                    — career game flow; uses GameSettingsPersist.instance
Assets/Scripts/Sweeping/SweeperManager.cs                 — sweep activation, ShotLocation() → AI_Sweeper.OnSweep()
Assets/Scripts/AI/AI_Strategy.cs                          — AI shot decisions; ExecuteShot() is the single chokepoint for all ShotIntent.RemoveThreat calls, now FGZ-aware
Assets/Scripts/AI/AI_Target.cs                            — ShotIntent → concrete shot execution (EvaluateRemovalOptions, EvaluateScoringOptions, PlaceStrategicGuard, etc.)
Assets/Scripts/Rock/Rock_Flick.cs                         — pull/release shot; Physics2D.SyncTransforms() after rb.position; isPressed set true on OnMouseDown (the canonical "grabbed" signal)
Assets/Scripts/Rock/Rock_Force.cs                         — FixedUpdate: curl force, haptic loop, rest detection
Assets/Scripts/Rock/Rock_Colliders.cs                     — outOfPlay/inPlay/inHouse state machine; ForceOutOfPlay()/RestoreForFreeGuardZone() for FGZ penalty/restore
Assets/Scripts/Rock/FlickShotController.cs                — flick shot; shared static visual helpers (fixed Aug 15 2026, see above)
Assets/Scripts/UI/TextCalloutManager.cs                   — floating text callouts; ShowCallout(alwaysShow:) bypasses debug gate
Assets/Scripts/Splash/PauseMenu.cs                        — pause/resume; stops haptics on pause; debug toggle (wired + UI Toggle in editor, done)
Assets/Scripts/CameraManager.cs                           — house (full-screen) vs aim (inset) cameras, see Multi-camera setup above

Assets/Scripts/Tutorial/TutorialGameSetup.cs              — TutorialTrigger[] generic trigger list
Assets/Scripts/Tutorial/TutorialGameManager.cs            — ShouldTriggerTutorial iterates trigger list
Assets/Scripts/Tutorial/TutorialSequence.cs               — chainSequenceId field
Assets/Scripts/Tutorial/TutorialSequenceManager.cs        — chain in CompleteTutorial; stops haptics when timeScale=0; skips auto-start tutorials when gsp.debug
Assets/Scripts/Tutorial/TutorialStep.cs                   — spotlightWorldOffset Vector3 field
Assets/Scripts/Dialogue/DialogueController.cs             — nonBlocking pauseGame guard
Assets/Scripts/Dialogue/TextReplacementUtility.cs         — dialogue token replacement, actively being expanded (natural-language skill/end descriptions)

Assets/Scripts/Dialogue/Data/TutorialSteps/TournyGame/
  PRTutorialGameSetup.asset                               — PR trigger list (rocks 13 + 14)
  FlickTutorialGameSetup 1.asset                          — Flick trigger list (rocks 3 + 5)
  PullRelease/PullReleaseTutorial.asset                   — 4 steps, chainSequenceId: SweepTutorial
  FlickShot/FlickShotTutorial.asset                       — 6 steps, chainSequenceId: SweepTutorial
  Sweeping/SweepTutorial.asset                            — 5 steps, autoStart: 0
  Takeout/TakeoutTutorial.asset                           — 4 steps wired and in-use
```

---

## iOS / Xcode build notes

Unity 6000.4.5f1 + Xcode 26 requires three fixes to the generated project. These are applied automatically by `Assets/Editor/XcodePostProcess.cs` on every export. If building from a pre-existing Xcode project (no re-export), patch manually via the pbxproj or Xcode Build Settings:

| Setting | Target | Value | Why |
|---|---|---|---|
| `CLANG_WARN_QUOTED_INCLUDE_IN_FRAMEWORK_HEADER` | UnityFramework | NO | PluginBase headers use `"double-quoted"` includes, now an error in Xcode 15+ framework targets |
| `ENABLE_MODULE_VERIFIER` | UnityFramework | NO | Module verifier rejects Unity's PluginBase headers regardless of warning suppression |
| `ENABLE_USER_SCRIPT_SANDBOXING` | Both targets | NO | Xcode 15+ sandbox blocks IL2CPP from loading `libhostfxr.dylib`, so `il2cpp.a` never builds and the linker fails |

**First-launch on device:** IL2CPP + Metal shader compilation can take 2–5 minutes on first install. Normal. Subsequent launches are fast.

**If the app immediately crashes after a hang-kill:** Delete the app from the device (corrupted install), Clean Build Folder in Xcode, reinstall.

---

## Changes made Aug 13–15 2026

- **Fixed two-gsp-instance bug** — `TournyManager.cs:74`, `GameSettings.cs:43`, `GameSettings.cs:90`: `GameObject.Find("GameSettingsPersist")` → `GameSettingsPersist.instance`.
- **Debug callout gate** — `TextCalloutManager.ShowCallout()` gated on `gsp.debug`; `alwaysShow: true` bypasses. Pause menu debug toggle fully wired (code + editor UI Toggle).
- **Sweep coaching callouts restored** — `AI_Sweeper.ShowSweepCallout()` helper uses `alwaysShow: true`. Also fixed: switch case was `"Draw To Target"` (never matched); replaced with actual shot type strings.
- **Flick speed feedback restored** — `FlickShotController` "Perfect!" / "Too Fast" / etc. uses `alwaysShow: true`.
- **Haptics pause** — `PauseMenu.Pause()` and `TutorialSequenceManager.SetupStep()` (when timeScale=0) both call `HapticController.Stop()`.
- **MainDisplay off at grab** — turns off the instant a player grabs the rock (`Rock_Flick.isPressed`), not just at release.
- **QuickTestGame no longer triggers the shooting tutorial** — `TutorialSequenceManager.CheckAutoStartTutorials()` now also skips when `gsp.debug == true`.
- **Free Guard Zone rule implemented** — see dedicated section above. Not yet comprehensively tested.
- **AI strategy is FGZ-aware** — see dedicated section above.
- **FlickShotController duplicate visual elements fixed** — see dedicated section above.

## AI sweeper intelligence — IMPLEMENTED (Aug 19 2026)

Investigated `AI_Sweeper.cs` in depth before changing anything — it's far more sophisticated than it looks at a glance. `MonitorAndSweepCoroutine` (the active system; the old checkpoint-based `TargetShot()` is fully disabled) already:
- Runs a real physics-based trajectory simulation every frame (`TrajectorySimulator`), factoring in every other rock in play, with genuine collision-avoidance logic (steers around an off-line obstacle, hard-sweeps to get past an on-line one, or accepts an unavoidable one without deliberately seeking a bank shot — bank shots are illegal, confirmed this isn't happening)
- Already scales sweep effectiveness by sweeper `CharacterStats` (`GetSweeperSkill()`) for AI, and `Sweep.cs` (the same physics code both human and AI sweeping funnel through) already scales every effect by `sweepStrength` for both sides

So of the three asks, only one was a genuinely new feature:
1. **Path awareness (avoid other rocks)** — already existed, no code changed. Asked the user to report back if they actually observe it failing in practice, since that'd be a bug hunt, not new work.
2. **Strategic restraint on guard shots** — NEW. In `MonitorAndSweepCoroutine`, added `bool guardRestraintApplies = isGuardShot && predictedFinalY > 0f` (guard-type shots identified via `shotType.Contains("Guard")`, matching the existing convention in `AI_Shooter.cs`). This suppresses the two distance-based sweep branches (Critical/Weight) when a guard shot is predicted to land short but still in a sane position (past the hog line) — "any guard usually works" per user's call, not a stricter exposure-comparison check. Lateral line correction still applies independently regardless, since it's the same `if/else-if` chain, just gated with `&& !guardRestraintApplies` on the two distance branches specifically (inserting this as its own chain link would have silently detached the takeout/collision branches above it from the distance/lateral branches below — worth remembering if touching this chain again).
3. **10% global effectiveness boost** — NEW. `Sweep.cs`: added `GLOBAL_EFFECTIVENESS_BOOST = 1.1f`, applied once to `sweepAmt` in `Start()`. Since every sweep formula multiplies through `sweepAmt`, this boosts human and AI sweeping equally without changing how it scales with stats.

**Not yet tested in Play mode or on device.**

## Broom/handle animations by equipment tier — IN PROGRESS (Aug 25 2026)

Sweeper animations should visually reflect the equipped handle, always with team-color accents. User finished the spritesheets; confirmed the animation state graph (Idle/Sweep/etc.) is identical across all broom tiers, so an `AnimatorOverrideController` per tier is the right approach (not separate layers per broom - would mean way more active sprite renderers/GameObjects for no benefit, since only the art differs, not the state machine).

**Actual architecture (corrected after seeing the user's real prefab setup - my first pass assumed the broom would be a field added onto each character's own `SweeperParent`, which was wrong):**
- The broom is a **standalone prefab** (`Broom_Left 1` built so far, `Broom_Right` presumably to follow) with its own `SweeperParent` component - not embedded in the character rig. One prefab per side, reused for both teams; color/AOC are set at runtime, not baked in per-team copies.
- `SweeperManager` instantiates it fresh each turn alongside `sweeperL`/`sweeperR` (same parent, `sweepSel.gameObject.transform`), via new `broomLeftPrefab`/`broomRightPrefab` fields (assign in the Inspector) and private `broomLeft`/`broomRight` instance refs. Destroyed in `ResetSweepers()` alongside the character sweepers.
- Handle tier → broom art mapping is a straight 1:1 index match: the user's `BroomController_Wood, _Comp_01, _Comp_02, _Carbon_01, _Carbon_02` AOC list (in that order) maps directly to `EquipmentManager`'s cost-bracket order (Wooden, Fibreglass, Composite, Carbon Fibre, Exotic Carbon Fibre) - the AOC asset names don't match the tier names but the order does, confirmed with user.

**Existing architecture this builds on (unchanged, just clarifying what was already there):**
- `SweeperParent.sweeperLayers[]` — array of `Sweeper` (each with its own `Animator`), all triggered together by `Sweep()`/`Hard()`/`Whoa()`. This is what "layers" already meant in this codebase.
- `CharColourChanger` — generic, pre-existing: holds `SpriteRenderer[] colour1GO`, `TeamColour(Color)` tints all of them. Already used for shooter/roster tinting in `TeamManager.SetSweepers()` via `teamRedColour`/`teamYellowColour` (`GameSettingsPersist.redTeamColour`/`yellowTeamColour`) — **not** `CareerManager.teamColour` (that's the equipment shop's player-personal color; the sweeper broom uses whichever SIDE, red or yellow, is currently sweeping, matching the existing roster-tinting convention, and this is what colors AI opponents' brooms too since there's no tracked equipment for them - they default to tier 0/Wooden/white).
- `EquipmentManager.activeEquip[0]` = the player's currently equipped handle.

**What's implemented:**
- `EquipmentManager.GetHandleTierIndex(Equipment)` / `GetActiveHandleTierIndex()` — tier 0-4 from cost, same brackets as shop generation.
- `SweeperParent.cs`: `broomLayer` (which `Sweeper` is the swappable one - must ALSO be added to `sweeperLayers[]` itself for Sweep/Hard/Whoa sync), `broomOverrides[]` (5 AOCs, index = tier), `broomColour` (`CharColourChanger` targeting the broom's SpriteRenderer(s)), `SetBroom(tierIndex, sideColor)`. Tier 0 (Wooden) always renders white.
- `SweeperManager.cs`: new `broomLeftPrefab`/`broomRightPrefab` fields + `SetupBroomOverlay()` (instantiates + calls `SetBroom()`). Six new tiny wrapper methods (`SweepL_Sweep/Hard/Whoa`, `SweepR_Sweep/Hard/Whoa`) each call the character sweeper's method **and** the corresponding broom overlay's, so the broom animates in sync — all 11 existing call sites across the file (`SweepTap`, `SweepTapLeft/Right`, `SweepWeight`, `SweepHard`, `SweepHit`, `SweepWhoa`, `SweepLeft/Right`, the tap-timer coroutine) were switched to go through these instead of calling `sweeperL`/`sweeperR` directly.

**Current exact state (Aug 27 2026) — `Broom_Left 1` prefab:**
- `Sweeper Parent` component: `Sweeper Layers` = `[NC_Head, NC_Shirt, NC_Legs]` (3) — the character body was folded directly into this same prefab (see "characters on/off" below) rather than kept as a separate rig.
- `Broom Layer` = `NC_Broom` ✓ correctly assigned (was `None` earlier, now fixed).
- `Broom Overrides` (5) = `BroomController_Wood, _Comp_01, _Comp_02, _Carbon_01, _Carbon_02`, in that order — confirmed 1:1 with the shop tier order (Wooden/Fibreglass/Composite/Carbon Fibre/Exotic Carbon Fibre). Asset names don't match tier names, order does.
- `Broom Colour` = the `Char Colour Changer` on this same GameObject, `Colour 1GO` size 2.
- The 5 `BroomController_*.overrideController` assets + their source spritesheets exist under `Assets/Art/Characters/BroomOnly/` (Wood, Composite_01, Composite_02, Carbon_01, Carbon_02 folders).
- `Broom_Right.prefab` also exists now (mirrors `Broom_Left 1`, not yet cross-checked field-by-field).

**Immediate next action — one bug found, not yet fixed:** `NC_Broom` is **not** in `Sweeper Layers` (only Head/Shirt/Legs are). This means `Sweep()`/`Hard()`/`Whoa()` never reaches the broom's `Animator` — it'll sit frozen on its default state even though the character animates fine. **Fix: add `NC_Broom` as a 4th element in `Sweeper Layers` on `Broom_Left 1` (and presumably `Broom_Right`)** — it belongs in both `Sweeper Layers` (for the trigger fan-out) and `Broom Layer` (for the AOC/color targeting) simultaneously, that's expected/correct, not a duplicate mistake.

**Worth double-checking (not yet confirmed either way):** does `Char Colour Changer.Colour 1GO` (size 2) reference *only* the broom's own SpriteRenderer(s)? If it accidentally also references Head/Shirt/Legs, a Wooden Handle would incorrectly tint the whole character white instead of just the broom.

**Still needed in the Unity Editor, in order:**
1. Add `NC_Broom` to `Sweeper Layers` on `Broom_Left 1` (see above - this is the very next step).
2. Verify `Broom_Right` matches `Broom_Left 1`'s setup (Sweeper Layers including its own broom entry once added, Broom Layer, Broom Overrides x5, Broom Colour).
3. Assign `broomLeftPrefab`/`broomRightPrefab` on the `SweeperManager` component in the scene (not yet done as of Aug 27).
4. First Play mode test — with just this one character's setup. Watch for: does the broom actually animate (Sweep/Hard/Whoa) once step 1 is done; does the color tint apply only to the broom and not the character.
5. Repeat/adapt for other characters (Pierre, Cal, Sandy, Tracey, etc.) if each needs its own broom art position — unclear yet whether one broom prefab per side works for all characters or needs per-character variants. Resolve once the first one is validated.

**Open design question, deliberately deferred until the above is validated:** user wants to revisit "turning sweeper characters on/off" — i.e. whether there should be a way to show just the broom without the character body, or vice versa. Folding Head/Shirt/Legs directly into the same prefab as the broom (current approach) was a reasonable resolution to an earlier "this looks too busy with both visible" concern, but the user explicitly wants to come back to this as its own topic - don't assume it's fully settled.

**Not yet tested** — Editor setup is mid-progress, nothing validated in Play mode yet.

## Equipment shop weekly regeneration bug — FIXED (Aug 19 2026)

**Symptom:** after completing week 1 in career mode, the equipment shop list "reverts" to a weird-looking list (duplicate-looking rows like "Fibreglass Handle $500" / "Fibreglass Handle $1,000"), and colors on owned equipment don't persist.

**Root cause:** `EquipmentManager.GenerateItems()`/`LoadItems()` build each of the 30 handle slots (20 for footwear/apparel) with a **freshly randomized cost every time they run**, then pick tier name + color purely from which price bracket that random cost lands in. Ownership is tracked only by a bare numeric ID (`cm.inventoryID`, plain `int[]` — no name/tier/color persisted with it beyond the separate full-array save path). Since id→tier mapping was random-per-call, id #5 could be "Fibreglass" one week and "Composite" the next after a fresh regeneration — the ownership record still pointed at id #5, but id #5 had become a different item. 30 random costs bucketed into only 5 tier names also produces visually-duplicate-looking rows even within one generation.

**Fix, applied identically to handles/heads/footwear/apparel in both `GenerateItems` and `LoadItems`:**
- New `EquipmentManager.IsExistingSlotOwned(equipList, ownedIds, i)` — true if slot `i`'s existing item's `id` is in `cm.inventoryID` (the authoritative save data, not the `.owned` flag which isn't guaranteed fresh at generation time), or if `i == 0` (the free starter item, always kept once it exists — same guarantee the old code had via a blanket `temp[0] = equipList[0]` hack, now generalized and made per-type-safe).
- Owned slots are copied through unchanged (cost/id/stats never re-rolled). Non-owned slots get a fresh random roll — this is what gives "new shop stock every week" while never touching what's already bought.
- `SetInventory()` now passes `cm.inventoryID` into both methods so this check has the data it needs; this makes the fix robust even if the `cm.loadedFromSave` branch (Generate vs Load) doesn't reliably reflect timing across the week transition, since either path now preserves ownership correctly.
- New `EquipmentManager.GetTeamColor()` (`CareerManager.teamColour`, falls back to `Color.white`) replaces the old per-tier `Random.ColorHSV(...)` / hardcoded `new Color(0f,1f,1f,1f)` calls — equipment color is now always the player's team color, live, rather than randomly rolled and never actually saved. This also trivially fixes the color-persistence symptom since there's no longer any color data to lose.

**Per-tier color rules (Aug 19 2026 refinement)** — not every tier gets the team color, per explicit user rules:
- **Handles:** Wooden Handle → `Color.white`; Fibreglass/Composite/Carbon Fibre/Exotic Carbon Fibre → `GetTeamColor()`
- **Heads:** all tiers → `Color.white` (no color augmentation for heads at all)
- **Footwear:** Half Slider/Full Slider/Premium Slider → `Color.white`; Premium Shoes/Exotic Shoes → `GetTeamColor()`
- **Apparel:** all tiers → `GetTeamColor()` (no exception given)

**Not yet tested** — needs a real multi-week career playthrough to confirm the shop looks right, owned items/colors survive the week transition, and the per-tier white/team-color split displays correctly.

---

## Where things stand (Nov 25 2026) — read this first

- **Everything is merged to `main`** (merge 97a4e0b4, pushed). The `physics-timing` branch is redundant. Work from `main`, small commits, run EditMode tests (134 pass), ask before pushing.
- **Training:** three tracks (Sweep, Tactics, On-Ice), four levels, $500/$1,000/$2,000/$4,000. See `TRAINING_DESIGN.md` (tier sheet + fuzz look).
- **Aim fuzz look:** oval + cone + crosshairs + cycling dots, cold blue, screen-blend shaders. Quick Test (press Q in a game) has live sliders for skill, training levels and look strengths; it never writes the career save.
- **Shooter kick** runs at the rock's real speed in Arcade and Pro (no handoff); Real-time uses weight speed / 3 (`ShooterAnim.KickSlowdown`).
- **Sponsors:** audit in `SPONSOR_AUDIT.md`; decision is to scrap the stat bonuses and tune availability.
- Uncommitted on purpose: Unity churn (prefabs NC_*, slnx, Packages, ProjectSettings, UserSettings, `.claude/`).

**Variable ice, step 1.** `RockPhysicsProfile.WithIce(speed, curl)`: a weight leaves the hand at the same speed, faster ice carries it further; at curl 1 the path to a given spot is identical, curl scales the bend. `IceConditions` (applied in `RockPhysicsBootstrap.Make`) is on only with the pause menu's Variable Ice checkbox (Advanced, dev builds, off by default, never in the tutorial); defaults 1.15 / 1.35, Quick Test sliders `iceSpeed` / `iceCurl`. Aim line, AI and clocks follow the profile. Next: step 2 assign by event level (outdoor 0.85/0.65, club 1/1, big 1.15/1.35), step 3 worn paths; each step gets its own checkbox (`PauseMenu.BuildExperimentToggle`). Hand-tuned AI "Manual Take Out/Peel/Tap" pulls (tutorial, story) do not adjust.

**Venue ice, step 2.** `VenueIce`: kind by background (Outdoor 0.85/0.65, Stadium 1.15/1.35, others 1/1), each event its own character (+-5% speed, +-10% curl, from the career's saved `iceSeed` and the tourny) and a visit shift per season (+-2% / +-4%). Pause > Advanced > Venue Ice (off by default; wins over Test Ice, the old Variable Ice box). Experiment checkboxes now sit in a two-column `Experiments` block. Quick Test has a `venue` dropdown. `IceConditions.Refresh` also runs at each turn. Next: (3) adjustable line: the player's read (speed, curl sliders) draws the aim line, dots included, and everything predicted; hidden true ice; reads saved per event, carried across ends, games and seasons, with a 'last time here' dialog (use notes / start fresh). (4) AI read. (5) ice-reading skill (proposed 4th training track: after-shot note, range on sliders, tighter range, match-the-ice). (6) worn paths.

**Adjustable line, step 3.** Pause > Advanced > Adjustable Line. `PlayerRead` (speed 75-125%, curl 50-160%) is the ice the player's aim line, dots, hit arrows, collision warning, weight call and expected times are drawn on (`TrajectorySimulator.Ice`, `TrajectoryLine.LineIce`); rocks, AI and live clocks stay on `RockPhysicsSettings.Current`. With Venue Ice on, reads are stored per event in PlayerPrefs (`IceRead_<seed>_<venue>`), start at the venue kind's average, and a later season asks Use these notes / Start fresh. `IceReadPanel` is built in code (ICE button right edge, panel above the hack, dialog); `Rock_Flick` ignores presses over it. The pause labels hide the true ice numbers while the line is on. Not yet seen in play. Next: (4) AI read, (5) ice-reading skill, (6) worn paths.

**Ice in seconds and feet, line colours.** `IceUnits`: weight = hog-to-hog seconds of a button draw (14.5*sqrt(speed)), curl = feet it moves (3.6 on normal ice, 4 ft per unit); used by the ice menu (tenths, 12.5-16.2 s, 1.8-5.9 ft), pause labels and Quick Test (`iceSeconds`, `iceFeet`). A player's aim line, dots and pull line are always violet; while reading the aim is violet (pale for a high guard, vivid from takeout weight; `TrajectoryLine.VioletFor`, worn by the pull line and circle too), the traced path dark grey, and after release `ReadGrade` colours the line green/yellow/red by the gap between read and real ice for that throw without shooter error or other rocks (1.5 / 3.5 / 7 ft; heavy shots compared at the back of the house). Idea parked: worn paths as frosty tracks so the gradient stays for accuracy.

**Clocks.** The split and hog-to-hog readouts (live, final and expected ranges) show real-ice seconds on every pace: `IceClock` reads off how far the rock was going on the current pace and gives the Real-time law's time for it, scaled so a button draw is 3.80 / 14.50. Display only. `SplitTimer.Split`/`HogToHog` hold the shown values.

**Delivery, camera and banner.** At release `RockSlide.PushOff` brings a pulled-back rock forward along its line to
where the shooter holds it and accelerates it from 35% to full speed by the end of the push-off (cosmetic: no friction before the hog
line; use `ThrownVelocity` for the thrown speed during the push). During the push the character layers are shifted so the body travels evenly between poses (`ShooterAnim.BodyAt`, measured per pose). `ShooterAnim.Deliver` runs at the rock's pace: the shooter stays put while its pose is
picked from where the rock is (`PoseFor`, using the rock positions measured from `Shooter_Yellow/Red_135_1`: forward swing, kick,
slide), then travels with the rock at the same speed and lets go; the release clip plays once at 12 fps and the shooter coasts about
`CoastDistance`. The sheet's drawn rock is kept on the real one. Per-layer `RelativeJoint2D` stays off; the Real-time kick slowdown is
gone. The banner and "tap anywhere" are dropped by `GameHUD.bannerDrop` (225). `CameraManager.RockFollow` moves from the moment of
release (`GameManager` calls it on `Rock_Flick.springReleased`; the virtual camera tracks an anchor moved with the rock, framed after 10 units).
`ShotSetup` cuts the same anchor straight to `launcher.y + ShotViewLead` (1; the confiner holds it at the bottom of the ice, leaving room for the 5.5 pull); no composer offsets or damping in either view. Computer turns cut to the follow framing instead (`ShotSetup(true)`: hack near the bottom), and `RockFollow` ignores anything more than 1 behind the launcher so the push-off never jolts it. `AI_Shooter.DrawBack` pulls the AI's rock back over 0.6 to 1.1 s by pull distance, and `ShooterAnim` poses to it as for a player.

**Logging.** Chatty logs go through `GameLog.Info`, compiled out unless `HAMMER_LOGS` is in the scripting define symbols (that includes
`[ShotAudit]` and `[SplitTimer]`). Use `Debug.LogWarning`/`LogError` only for real problems.

### Working list (next up, in order)

1. **Timing readout and weight call:** done and played (top-left panel, prefab `Resources/TimingPanel`, names in `WeightNames.cs`). Still to confirm:
   unswept throws land inside the ranges, and the full EditMode run in the editor.
2. **Collision aids:** fixed and played (the fuzzed aim is a real throw, so line, contact, warning line and arrows agree). Tune arrows in `HitArrow.cs`.
3. **Design gaps:** built, needs a career play test (cohesion in `SweepStats.cs`, finesse is now consistency in `AimFuzz.cs`, training $500-$4,000, gear 20 points max,
   sponsors money-only in `SponsorRules.cs`). Open: tune prices against weekly income.
4. **Training screen layout:** done (three rows; `Editor/TrainingScreenLayout`). Open: final icons for Tactics and On-Ice (placeholders now).
5. **Polish:** tier icon art (`Track_n/Panel/Image` in Arena_Selector); pause-menu locked-aid hint; copy the new two-panel pause menu to the other 11 pause-menu scenes; pull-back circles (built: `PullbackSettings.cs`, thumb circle off, cap 0.40, compare in Quick Test; needs a play test); Classic physics is no longer offered (code still there, remove if wanted).
   **Game Pace** slider (Arcade/Pro/Real-time, every player, every build) is in TournyGame's pause menu (`Editor/PauseMenuPaceSlider`); the other scenes only have the dev-only PHYSICS button. Open: Real-time slide animation has too few frames; a swept rock keeps the lower friction after the sweep stops (asked, not decided). Sweep fatigue is rebuilt in `SweepFatigue.cs` (seconds, scales with pace, endurance 0 = half a run, 100 = a whole run); needs a play test.
6. **Playtests:** AI-vs-AI with the corrected collision model; Real-time/Pro kick pace and all new aids on a phone; confirm the line reach past the near hog actually shows.

---

## Physics rebuild (Nov 2026) — merged to `main`

One physics model now drives both the live rock and the aim-line simulator (`RockPhysics`, `RockPhysicsProfile`, `RockSlide`, `RockCollision`, `TrajectorySimulator`). Profiles: **Arcade** (default, soft stop), Pro (half speed), Real-time (real-world seconds, no `Time.timeScale`); Classic (original) is no longer offered. Players choose with the Game Pace slider; the Arcade stop curve stays a tester option (`DevOptions.Enabled`). Flick mode is hidden. Also in: the 2% launch-speed bug fix (shooter bodies' mass), aim-line fuzz from skill, hack-to-hog / hog-to-hog split clocks, Box2D-matched rock collisions with chains and frozen pairs.

- Design, numbers and the not-verified list: `PHYSICS_REBUILD.md`. Collision measurements: `PHYSICS_COLLISIONS.md`. Baselines: `PHYSICS_PHASE0.md`.
- Harnesses live in `AgentScripts/` (run with `unity command run_script`): `Measure.cs`, `Collide.cs`, `Probe.cs`.
- 134 EditMode tests; run them before and after any physics change.
- Needs a human on a device: sweeping, AI takeouts, aim-fuzz and split-clock look, pause-menu layout.

---

## Pending / not started

- **Device testing on Mac** — DONE, confirmed working (Aug 2026). FGZ rule, camera fixes, MainDisplay, FlickShotController dedup all validated on-device.
- **Comprehensive FGZ testing** — chain reactions, end-of-end boundary, flick shot mode specifically still not deliberately exercised
- **AI sweeper changes need testing** — strategic restraint + 10% boost implemented but unverified, see dedicated section above
- **Equipment shop fix needs testing** — owned-item preservation + team color, see dedicated section above, needs a multi-week career playthrough
- **Broom/handle animations** — see dedicated section above, blocked on art + tier count reconciliation
- **Tutorial branching** — infrastructure exists, no steps wired yet. **Paused**, not currently being worked on.
- **Menu tutorials** — needs design discussion before any code
- **Distinct AI "tap" shot type** — current FGZ redirect reuses the existing draw (ScorePoints) logic rather than a true gentle-tap execution path; only worth building if the draw redirect feels wrong in practice
