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

**rockCurrent:** Resets to `16 - (rocksPerTeam * 2)` at start of each end (= 0 for standard 8-rock game). Increments in `NextTurn()`. Throws 0–3 within an end are the first 4 rocks (FGZ-protected).

**Note:** `Rock_Flick.ShotLocation()` is dead code — only `SweeperManager.ShotLocation()` is called.

---

## Callout system

`TextCalloutManager` (DontDestroyOnLoad singleton) — call `Instance.ShowCallout()` or `Instance.ShowRockCallout()`.

- By default, `ShowCallout()` is gated on `gsp.debug`. Pass `alwaysShow: true` to bypass.
- `ShowRockCallout()` also accepts `alwaysShow: true`.
- **Always-on callouts:** sweep coaching (`AI_Sweeper.ShowSweepCallout()`) and flick speed feedback (`FlickShotController` — "Perfect!", "Too Fast", etc.)
- **Debug-gated:** everything else (AI strategy, raw velocity data, position analysis, timer display)
- Debug toggle in PauseMenu — wire a UI Toggle to `PauseMenu.debugToggle` in the inspector (not yet done in editor)

---

## Multi-camera setup

| Camera | Normal depth | When aim active |
|---|---|---|
| `main` | 1 | 1 |
| `top` | 2 | 2 |
| `aim` | -1 (inactive) | 3 |
| `ui` | 3 | 4 (aim + 1) |
| `house` | -1 or 5 | — |

`CameraManager.Trajectory()` is called every frame from `TrajectoryLine.DrawTrajectory()`. It sets `aim.depth = 3` when the trajectory target Y > 0, and always computes `ui.depth = aim.depth > 0 ? aim.depth + 1 : 3` so the UI camera is always on top.

Spotlight canvas is **Screen Space - Camera** mode using the `ui` camera (forced in code in `SetupStep()`). This ensures it renders over everything including the aim camera view.

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
- `branchCondition` / `onSuccessStep` / `onFailureStep` — branching/retry support

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
Assets/Scripts/GameManager.cs                             — main game loop, RedTurn/YellowTurn/CheckScore/NextTurn
Assets/Scripts/GameSettingsPersist.cs                     — DontDestroyOnLoad state singleton; always use .instance, never GameObject.Find
Assets/Scripts/Tourny/TournyManager.cs                    — career game flow; uses GameSettingsPersist.instance
Assets/Scripts/Sweeping/SweeperManager.cs                 — sweep activation, ShotLocation() → AI_Sweeper.OnSweep()
Assets/Scripts/AI/AI_Sweeper.cs                           — PlayerSpeed() coroutine: sweep coaching callouts for player shots
Assets/Scripts/Rock/Rock_Flick.cs                         — pull/release shot; Physics2D.SyncTransforms() after rb.position
Assets/Scripts/Rock/Rock_Force.cs                         — FixedUpdate: curl force, haptic loop, rest detection
Assets/Scripts/Rock/FlickShotController.cs                — flick shot; GetSpeedFeedbackMessage() → "Perfect!" / "Too Fast" etc.
Assets/Scripts/UI/TextCalloutManager.cs                   — floating text callouts; ShowCallout(alwaysShow:) bypasses debug gate
Assets/Scripts/Splash/PauseMenu.cs                        — pause/resume; stops haptics on pause; debug toggle field
Assets/Scripts/CameraManager.cs                           — ui.depth = aim.depth + 1

Assets/Scripts/Tutorial/TutorialGameSetup.cs              — TutorialTrigger[] generic trigger list
Assets/Scripts/Tutorial/TutorialGameManager.cs            — ShouldTriggerTutorial iterates trigger list
Assets/Scripts/Tutorial/TutorialSequence.cs               — chainSequenceId field
Assets/Scripts/Tutorial/TutorialSequenceManager.cs        — chain in CompleteTutorial; stops haptics when timeScale=0
Assets/Scripts/Tutorial/TutorialStep.cs                   — spotlightWorldOffset Vector3 field
Assets/Scripts/Dialogue/DialogueController.cs             — nonBlocking pauseGame guard

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

## Changes made Aug 13–14 2026

- **Fixed two-gsp-instance bug** — `TournyManager.cs:74`, `GameSettings.cs:43`, `GameSettings.cs:90`: `GameObject.Find("GameSettingsPersist")` → `GameSettingsPersist.instance`. Root cause: scene-local gsp pending destruction was found by Find(); TournySetup() wrote to the wrong object; EndMenu read from the real singleton.
- **Debug callout gate** — `TextCalloutManager.ShowCallout()` gated on `gsp.debug`; `alwaysShow: true` bypasses. Pause menu debug toggle wired in code (`PauseMenu.debugToggle`) — still needs a UI Toggle in the Unity editor.
- **Sweep coaching callouts restored** — `AI_Sweeper.ShowSweepCallout()` helper uses `alwaysShow: true`. Also fixed: switch case was `"Draw To Target"` (never matched); replaced with actual shot type strings.
- **Flick speed feedback restored** — `FlickShotController` "Perfect!" / "Too Fast" / etc. uses `alwaysShow: true`.
- **Haptics pause** — `PauseMenu.Pause()` and `TutorialSequenceManager.SetupStep()` (when timeScale=0) both call `HapticController.Stop()`.

---

## Next session — 5-rock rule (Free Guard Zone)

**Rule:** First 4 rocks of each end, opponent rocks in the FGZ cannot be removed. Violation: remove thrown rock, restore all displaced rocks.

**FGZ definition:** Y between 0 (hog) and 6.5 (tee), distance from (0, 6.5) > 1.5 (not in house).

**Approach:**
1. Before throws 0–3 of each end: snapshot ALL rock positions (including non-FGZ — chain reactions)
2. After `AllStopped()` (inside `CheckScore()`): check if any opponent FGZ rocks from snapshot have left the FGZ
3. If violation: remove thrown rock (mark `outOfPlay`), teleport displaced rocks back to snapshot positions, zero their velocity
4. Must skip during tutorial (`TutorialGameManager.Instance != null && TutorialGameManager.Instance.isTutorialGame`)

**rockList structure:** `List<Rock_List>` — access via `rockList[i].rock` (GameObject) and `rockList[i].rockInfo` (Rock_Info: `teamName`, `inPlay`, `outOfPlay`, `rockNumber`)

**Hook points in GameManager:**
- Before throw: end of `RedTurn()`/`YellowTurn()` setup block, before `WaitUntil shotTaken` (line ~750 / ~939)
- After all stop: after `AllStopped()` in `CheckScore()` (line ~1043)

**Estimate:** ~1 session (2–3 hours coding + testing)

---

## Pending / not started

- **5-rock rule** — see above, next priority
- **PauseMenu debug toggle UI** — code wired, needs a UI Toggle dragged to `PauseMenu.debugToggle` in the inspector
- **Tutorial branching** — infrastructure exists, no steps wired yet. Next: decide which steps should retry (DrawRelease aim check? FlickSwipe velocity?), set assets, tune thresholds from console logs (`lastCapturedSpeed`, aim position logged at release)
- **Menu tutorials** — needs design discussion before any code
- **General device playtesting** — in progress, reporting issues as found
