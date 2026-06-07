# Hammer Time — Session Handoff

Paste this at the start of a new session to restore context.

---

## Project

Unity curling simulation (iOS/Android). Core loop: player throws rocks, sweepers affect trajectory, score if closest to center of house. 8 ends, 8 rocks per team.

**Key scenes:** SplashMenu → CareerSettings → Arena_Selector → TournyGame / TutorialGame

**Key singletons (DontDestroyOnLoad):** `GameSettingsPersist` (gsp), `CareerManager` (cm)

**Save system:** JSON to `Application.persistentDataPath/career_save.json` via `CareerSaveService`

---

## What was last worked on

### Save/load system overhaul (all fixed)

1. **Old saves without `dialogueFlags` crashed on load** — null guard added in `CareerManager.LoadFromSaveData()`.

2. **Tutorial auto-saved over career saves** — `SaveCareer()` now returns early if `TutorialGameManager.Instance.isTutorialGame` is true.

3. **`IndexOutOfRangeException` in `SetupTourny()`** — old saves had no `teams` field in JSON. Added `RebuildTeamsFromPool()` to `CareerManager`; called from `TournySelector.SetUp()` when `cm.teams` is empty.

4. **`NullReferenceException` in `DialogueController.AdvanceDialogue()`** — null guard added at top of method.

5. **Hardcoded "Newbie"/"Tutorial Opponent" team names** — `TutorialGameManager` now resolves names from `gsp.redTeamName` → `cm.teamName` → "Newbie" fallback.

6. **Tutorial auto-triggered in TournyGame scene** — `TutorialGameManager.Start()` now returns immediately if active scene name is not `"TutorialGame"`.

7. **New career started with completed tournaments from old save** — `CareerSettings.New()` set `week = 1`, causing `TournySelector` to take the existing-career path (LoadCareer → stale ScriptableObject flags). Fixed by changing to `week = 0` so `TournySelector.SetUp()` calls `ClearAllCompletionFlags()` + `NewSeason()`.

### Key flow to understand

`TournySelector.SetUp()` branches on `cm.week`:
- `== 0` → new career: `ClearAllCompletionFlags()` then `NewSeason()`
- `> 0` → existing career: `LoadCareer()` then `ApplyPendingCompletionData()` then `SyncCompletionFromCareerManager()`

`CareerSettings.New()` sets `cs.week = 0`. `LoadToCM()` calls `cm.LoadSettings()` which copies `cs.week` → `cm.week`.

---

## Known state / no pending bugs

All 7 save/load bugs resolved. No known open issues at end of last session.
