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

## Pending / not started

- **Device testing on Mac** — DONE, confirmed working (Aug 2026). FGZ rule, camera fixes, MainDisplay, FlickShotController dedup all validated on-device.
- **Comprehensive FGZ testing** — chain reactions, end-of-end boundary, flick shot mode specifically still not deliberately exercised
- **AI sweeper changes need testing** — strategic restraint + 10% boost implemented but unverified, see dedicated section above
- **Equipment shop fix needs testing** — owned-item preservation + team color, see dedicated section above, needs a multi-week career playthrough
- **Broom/handle animations** — see dedicated section above, blocked on art + tier count reconciliation
- **Tutorial branching** — infrastructure exists, no steps wired yet. **Paused**, not currently being worked on.
- **Menu tutorials** — needs design discussion before any code
- **Distinct AI "tap" shot type** — current FGZ redirect reuses the existing draw (ScorePoints) logic rather than a true gentle-tap execution path; only worth building if the draw redirect feels wrong in practice
