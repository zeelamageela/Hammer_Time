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

---

## Pending / not started

- **Comprehensive FGZ testing** — chain reactions, end-of-end boundary, flick shot mode specifically
- **Device testing on Mac** — moving there next; none of this session's fixes have been verified on-device yet, only in the Unity Editor
- **Tutorial branching** — infrastructure exists, no steps wired yet. **Paused**, not currently being worked on.
- **Menu tutorials** — needs design discussion before any code
- **Distinct AI "tap" shot type** — current FGZ redirect reuses the existing draw (ScorePoints) logic rather than a true gentle-tap execution path; only worth building if the draw redirect feels wrong in practice
