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
- Same chain behaviour. (Had a duplicate DrawOutro bug — removed this session.)

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
Assets/Scripts/Tutorial/TutorialGameSetup.cs              — TutorialTrigger[] generic trigger list
Assets/Scripts/Tutorial/TutorialGameManager.cs            — ShouldTriggerTutorial iterates trigger list
Assets/Scripts/Tutorial/TutorialSequence.cs               — chainSequenceId field
Assets/Scripts/Tutorial/TutorialSequenceManager.cs        — chain in CompleteTutorial; spotlightWorldOffset applied
Assets/Scripts/Tutorial/TutorialStep.cs                   — spotlightWorldOffset Vector3 field
Assets/Scripts/Dialogue/DialogueController.cs             — nonBlocking pauseGame guard
Assets/Scripts/Rock/Rock_Flick.cs                         — Physics2D.SyncTransforms() after rb.position
Assets/Scripts/CameraManager.cs                           — ui.depth = aim.depth + 1

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

## Uncommitted changes (working tree)

Everything below is modified but not yet committed:
- All tutorial code and asset changes described above
- `GameManager.cs` — `EndOfEnd` + `EndOfGame` redirect to `SplashMenu` when `TutorialGameManager.isTutorialGame` is true
- `Assets/Editor/XcodePostProcess.cs` — new file, auto-patches Xcode project on iOS export
- NC_Hal / NC_Pierre / NC_Sandy / NC_Tracey prefabs — `colour1` changed from orange to cyan/teal
- 5 deleted `.meta` files under `Assets/Feel/` (demo package cleanup)
- Editor layout / `.slnx` / `EditorUserSettings` — Unity housekeeping, commit as-is

---

## Pending / not started

- **Sweep thresholds** — confirmed working well on device
- **Tutorial branching** — infrastructure exists, no steps wired yet. Next: decide which steps should retry (DrawRelease aim check? FlickSwipe velocity?), set assets, tune thresholds from console logs (`lastCapturedSpeed`, aim position logged at release)
- **Menu tutorials** — needs design discussion before any code
- **Flick shot tutorial setup** — `FlickTutorialGameSetup 1.asset` may need additional pre-placed rocks reviewed for the 6-rock scenario
- **General device playtesting** — in progress, reporting issues as found
