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

### Tutorial system fixes (all done)

1. **Tutorial save isolation** — `TutorialGameManager.InitializeTutorialGame()` now explicitly clears `gsp.gameInProgress`, `gsp.tournyInProgress`, `gsp.justFinishedGame`, `gsp.inEndMenu`, and `cm.loadedFromSave` at the end of setup. This prevents a career game-in-progress save from bleeding into tutorial (GameManager won't try to restore career rock positions/scores over tutorial state). Save guard was already in place (`SaveCareer()` returns early when `isTutorialGame = true`).

2. **Multiple tutorial support** — `TutorialGameManager` now has two setup fields:
   - `tutorialSetup` — pull/release tutorial (existing, default)
   - `flickShotTutorialSetup` — flick shot tutorial (new field, assign in Inspector when ready)

   At init, it reads `GameVisualizationSettings.Instance.FlickShotMode` and resolves `activeSetup` (either flick or pull/release). All downstream methods (`RepositionPreConfiguredRocks`, `ShouldTriggerTutorial`, `GetAIShotSuggestion`, validation) use `activeSetup`. **To-do:** create the flick shot `TutorialGameSetup` ScriptableObject and assign it in the Inspector.

3. **Spotlight centering fixed** — `TutorialSequenceManager.SetupStep()` world-target spotlight was using `cutoutMask.position = WorldToScreenPoint(...)` which breaks when a `CanvasScaler` is present. Now uses `RectTransformUtility.ScreenPointToLocalPointInRectangle()` → `cutoutMask.anchoredPosition`, which correctly handles all Canvas render modes and scale factors.

### Key flow to understand

`TournySelector.SetUp()` branches on `cm.week`:
- `== 0` → new career: `ClearAllCompletionFlags()` then `NewSeason()`
- `> 0` → existing career: `LoadCareer()` then `ApplyPendingCompletionData()` then `SyncCompletionFromCareerManager()`

`CareerSettings.New()` sets `cs.week = 0`. `LoadToCM()` calls `cm.LoadSettings()` which copies `cs.week` → `cm.week`.

---

## Known state / no pending bugs

All 3 tutorial fixes resolved. No known open issues at end of last session.

### Upcoming work (not started)
- Create flick shot `TutorialGameSetup` ScriptableObject and wire it to `TutorialGameManager.flickShotTutorialSetup` in the Inspector
