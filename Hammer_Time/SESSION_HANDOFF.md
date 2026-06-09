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

## What was last worked on

### Session before last — Tutorial system improvements (all done)

1. **Tutorial save isolation** — `TutorialGameManager.InitializeTutorialGame()` explicitly clears `gsp.gameInProgress`, `gsp.tournyInProgress`, `gsp.justFinishedGame`, `gsp.inEndMenu`, and `cm.loadedFromSave`.

2. **Multiple tutorial support** — `TutorialGameManager` has `tutorialSetup` (pull/release) and `flickShotTutorialSetup` (flick shot, assign in Inspector). Resolves `activeSetup` at init from `FlickShotMode`.

3. **Spotlight centering** — Uses `RectTransformUtility.ScreenPointToLocalPointInRectangle()` → `cutoutMask.anchoredPosition`.

4. **New tutorial conditions** — `MouseReleased`, `RockReachedYPosition`. `WaitForSeconds` uses `WaitForSecondsRealtime`.

5. **Spotlight name/tag targeting** — `spotlightTargetName` supports `$currentRock`, `$shooter`, `$launcher`, `$aimTarget` keywords. `spotlightTargetTag` for tag-based lookup.

6. **Dialogue auto-repositioning** — `PositionAroundSpotlight(normalizedPos)` moves dialogue to opposite vertical half. Called from `SetupStep()`.

7. **Character head Z-order fix** — `BumpSpriteRenderers()` sets coach/announcer sorting layer + order to `forceSortingOrder + 1` (501).

---

### Last session — fixes and new features

#### Bug fix: trajectory and shooting knob freeze mid-drag when tutorial step fires

**Root cause:** `DialogueData` ScriptableObjects have their own `pauseGame` field, independent of `TutorialStep.pauseGame`. When the tutorial step fires and shows dialogue, `DialogueController.SetupDialogueUI()` was reading `currentDialogue.pauseGame` and calling `Time.timeScale = 0f` even though the step itself had `pauseGame = false` and `timeScale = 1`. With timeScale=0, `FixedUpdate` stops, `Rigidbody2D.position` writes never sync to `transform.position`, and the knob/trajectory appear frozen.

**Fix — `Assets/Scripts/Dialogue/DialogueController.cs`:**
- `SetupDialogueUI()`: `if (currentDialogue.pauseGame && !nonBlockingMode)` — never pauses during nonBlocking dialogue (shown while player is dragging)
- `TypeText()`: same guard for `WaitForSecondsRealtime` vs `WaitForSeconds`
- `EndDialogue()`: captures `wasNonBlocking` before `HideDialogue()` resets the flag, then conditionally restores timeScale

**Practical rule:** When setting up a tutorial step that fires during player input (dragging, aiming), check the `DialogueData` asset itself in the Inspector and ensure its `pauseGame = false`. The `TutorialStep` guard is a safety net, not a substitute.

---

#### Bug fix: deterministic pullback engine — rb.position not syncing to transform

**Root cause:** Rock pullback was rebuilt from spring-physics to direct `rb.position = mousePosition` in `Rock_Flick.Update()`. For kinematic Rigidbody2D, `rb.position` updates the physics body but `transform.position` only syncs at the start of each `FixedUpdate`. `DrawTrajectory()` reads `transform.position`, so it was one physics step stale. `ShootingKnob` is parented to the rock transform, so it also appeared to lag/freeze.

**Fix — `Assets/Scripts/Rock/Rock_Flick.cs`:**
```csharp
rb.position = Vector2.Scale(Camera.main.ScreenToWorldPoint(Input.mousePosition), posScale);
Physics2D.SyncTransforms(); // ← added: syncs rb.position → transform.position immediately
```

---

#### New: spotlight fixed world-position mode

The spotlight cutout can now be locked to a fixed world-space coordinate projected through the aim camera every frame as it pans.

**`Assets/Scripts/Tutorial/TutorialStep.cs` — new fields:**
- `bool useSpotlightWorldPosition` — enable fixed-position mode
- `Vector3 spotlightWorldPosition` — the world point (e.g. house centre `(0, 6.5, 0)`)

**`Assets/Scripts/Tutorial/TutorialSequenceManager.cs`:**
- `SetupStep()`: calls `ResolveWorldTarget()` once, then uses the Vector3 or Transform as appropriate
- `DynamicSpotlightUpdate()`: resolves world position per-frame from either the fixed Vector3 or a Transform

**Inspector setup for "Aim for the House" TutorialStep** ← STILL NEEDS DOING:
| Field | Value |
|---|---|
| `useSpotlight` | ✅ |
| `useSpotlightWorldPosition` | ✅ |
| `spotlightWorldPosition` | `(0, 6.5, 0)` |
| `dynamicSpotlight` | ✅ |
| `manualCutoutSize` | `(200, 200)` — tune in play mode |

---

#### New: conditional / branching tutorial steps

After a step's end condition fires, a `branchCondition` is evaluated synchronously and the sequence jumps to a named step (by `stepName`). Supports retry loops and success/failure paths.

**`Assets/Scripts/Tutorial/TutorialStep.cs` — new enum + fields:**
```
TutorialBranchConditionType:
  None
  AimPositionNearTarget    — aimCircle within branchThreshold of branchTargetPosition
  AimPositionFarFromTarget — inverse of above
  RockPositionNearTarget   — rock.transform.position within branchThreshold (good after RockStopped)
  FlickVelocityAbove       — rb.velocity.magnitude at release > branchThreshold
  FlickVelocityBelow       — rb.velocity.magnitude at release < branchThreshold

Fields on TutorialStep:
  branchCondition        TutorialBranchConditionType
  branchTargetPosition   Vector3  — world position reference for position conditions
  branchThreshold        float    — distance (units) or speed (units/sec) cutoff
  onSuccessStep          string   — stepName to jump to on true  (empty = continue in order)
  onFailureStep          string   — stepName to jump to on false (empty = continue in order)
```

**`Assets/Scripts/Tutorial/TutorialSequenceManager.cs`:**
- New fields: `lastCapturedSpeed` (float), `lastCapturedAimPos` (Vector3)
- `WaitForRockReleased()`: after condition fires, captures `aimCircle.transform.position`, yields one `WaitForFixedUpdate`, captures `rb.velocity.magnitude`
- `PlaySequenceCoroutine()`: after each step, calls `EvaluateBranchCondition()`, jumps via `currentStepIndex = jumpIdx - 1`
- `EvaluateBranchCondition(TutorialStep)`: reads captured + live state, returns bool
- `FindStepByName(string)`: searches `currentSequence.steps` by `stepName`

**Branch wiring example — aim check:**
| Field | Value |
|---|---|
| `endCondition` | `RockReleased` |
| `branchCondition` | `AimPositionNearTarget` |
| `branchTargetPosition` | `(0, 6.5, 0)` |
| `branchThreshold` | `1.5` — tune via console logs |
| `onSuccessStep` | `"GreatShot"` — must match another step's `stepName` exactly |
| `onFailureStep` | `"MissedHouse"` |

**Branch wiring example — flick velocity:**
| Field | Value |
|---|---|
| `branchCondition` | `FlickVelocityAbove` |
| `branchThreshold` | `5.0` — read `lastCapturedSpeed` from logs to calibrate |
| `onSuccessStep` | `"GoodFlick"` |
| `onFailureStep` | `"TooSlow"` |

---

## Spotlight keyword reference

Set in `spotlightTargetName` on `TutorialStep`. All require `dynamicSpotlight = true` if the target moves.

| Keyword | Resolves to |
|---|---|
| `$currentRock` | `gameManager.rockList[rockCurrent].rock.transform` |
| `$shooter` | `gameManager.shooterGO.transform` |
| `$launcher` | GameObject tagged `"Launcher"` |
| `$aimTarget` | `TrajectoryLine.aimCircle.transform` — trajectory endpoint, updated every frame by `DrawTrajectory()` |

---

## Pending / not started

- **Inspector wiring** for "Aim for the House" `TutorialStep` asset (see world-position spotlight table above)
- **Calibrate branch thresholds**: run aim tutorial, read `lastCapturedSpeed` and aim distance from console logs, set `branchThreshold` values accordingly
- **Flick shot tutorial**: create a `TutorialGameSetup` ScriptableObject (`Assets > Create > Tutorial > Game Setup`) and assign to `TutorialGameManager.flickShotTutorialSetup` in the Inspector. `TutorialGameSetup` supports pre-placed rocks, starting score/end/hammer, rock count, and per-rock AI overrides.

---

## Key files

```
Assets/Scripts/Dialogue/DialogueController.cs       — nonBlocking pauseGame guard
Assets/Scripts/Rock/Rock_Flick.cs                   — Physics2D.SyncTransforms() after rb.position
Assets/Scripts/Tutorial/TutorialStep.cs             — spotlight world-pos fields, branch fields + enum
Assets/Scripts/Tutorial/TutorialSequenceManager.cs  — SetupStep, DynamicSpotlightUpdate, branch eval
Assets/Scripts/CameraManager.cs                     — ui.depth = aim.depth + 1
Assets/Scripts/Tutorial/TutorialGameSetup.cs        — ScriptableObject for scripted game state (read only)
```
