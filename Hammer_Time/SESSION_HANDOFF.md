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

## What was last worked on

### Tutorial system improvements (all done)

1. **Tutorial save isolation** — `TutorialGameManager.InitializeTutorialGame()` explicitly clears `gsp.gameInProgress`, `gsp.tournyInProgress`, `gsp.justFinishedGame`, `gsp.inEndMenu`, and `cm.loadedFromSave`. Prevents career save state bleeding into tutorial.

2. **Multiple tutorial support** — `TutorialGameManager` has two setup fields:
   - `tutorialSetup` — pull/release tutorial (existing)
   - `flickShotTutorialSetup` — flick shot tutorial (new, assign in Inspector when ready)
   Resolves `activeSetup` at init based on `FlickShotMode`.

3. **Spotlight centering fixed** — Uses `RectTransformUtility.ScreenPointToLocalPointInRectangle()` → `cutoutMask.anchoredPosition` for correct CanvasScaler handling.

4. **New tutorial conditions** — Added to `TutorialConditionType`: `MouseReleased` (wait for mouse up) and `RockReachedYPosition` (wait until rock Y crosses a threshold). `WaitForSeconds` fixed to use `WaitForSecondsRealtime` so it works when `pauseGame = true`.

5. **Spotlight name/tag targeting** — `TutorialStep` has `spotlightTargetName` (supports reserved keywords `$currentRock`, `$shooter`, `$launcher`) and `spotlightTargetTag` for runtime scene object lookup without drag-and-drop.

6. **Dialogue auto-repositioning** — `DialogueController.PositionAroundSpotlight(normalizedPos)` moves the dialogue panel to the opposite vertical half of the screen from the spotlight. `ResetDialoguePosition()` restores it. Called from `TutorialSequenceManager.SetupStep()`.

7. **Character head Z-order fix** — `coachHead` and `announcerHead` are SpriteRenderer-based world-space objects. `EnsureDialogueOnTop()` now calls `BumpSpriteRenderers()` on both, setting their sorting layer to match the dialogue canvas and their `sortingOrder` to `forceSortingOrder + 1` (501). Fixes them appearing behind `Panel (1)` when the canvas is raised to sortingOrder 500.
   - Also bumps any nested `Canvas` components with `overrideSorting = true` to `forceSortingOrder + 1`.
   - **Inspector:** ensure `dialogueCanvas` on `DialogueController` points to the `DialogueCanvas` root (the GO with the Canvas component), not a child panel.

### Key flow to understand

`TournySelector.SetUp()` branches on `cm.week`:
- `== 0` → new career: `ClearAllCompletionFlags()` then `NewSeason()`
- `> 0` → existing career: `LoadCareer()` then `ApplyPendingCompletionData()` then `SyncCompletionFromCareerManager()`

`CareerSettings.New()` sets `cs.week = 0`. `LoadToCM()` calls `cm.LoadSettings()` which copies `cs.week` → `cm.week`.

---

## Known state / no pending bugs

All fixes resolved. No known open issues at end of last session.

### Upcoming work (not started)
- Create flick shot `TutorialGameSetup` ScriptableObject and wire it to `TutorialGameManager.flickShotTutorialSetup` in the Inspector
