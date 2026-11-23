# Hammer Time — Pre-Ship Audit

Date: 2026-11-21. Read-only audit; nothing was changed except writing this file.
**Revision 2 (same day):** added a scene/prefab pass (see "Scene audit" below). It refuted or downgraded several first-pass findings; those changes are called out inline and listed in "What the scene pass changed vs Revision 1".
Target: iOS release, Dec 1. Unity 6000.4.5f1.

## How to read this

Every finding carries a status:

- **Verified** — I read the full code path and the claim follows from the code (or a test I ran in this session proves it).
- **Code-verified, runtime unverified** — the code path is real (I traced it / grepped for the missing piece), but I did not run the game to see the symptom.
- **Suspected** — plausible from the code, depends on something I could not see (scene wiring, Inspector values, device behaviour). Each has a short way to confirm it.

Revision 1 did not parse scene YAML, so Inspector-dependent claims were Suspected. Revision 2 parsed all 12 scenes in Build Settings (read-only; the script lives in the scratchpad, nothing in the project was touched). **Blind spot:** the parser reads components that live directly in scenes. It does *not* look inside prefab internals, and I only read prefab-instance overrides for the cases I inspected by hand (e.g. the Career_Menu "Next" button). A button that looks unwired in a scene can still be wired through a prefab override. That already produced one false alarm (Continue) that I corrected.

## Coverage — what I did and did not read

Read in depth: `CareerSaveService`, `CareerSaveData`, the save/load orchestration in `CareerManager` (`SaveCareer`, `SaveCareerJSON`, `LoadCareerJSON`, `RestoreTournamentState`, lifecycle hooks), `CareerSettings` (Start/Player/New/LoadToCM), `GameSettingsPersist` (Awake, load paths), `GameManager` turn loop + end-of-end/game routing, `PauseMenu`, `MainMenu`, `TutorialGameManager`, `TextCalloutManager`, all singleton `Awake`s, `HighScoreService`, plus greps across `Assets/Scripts` for Find*/DontDestroyOnLoad/StartCoroutine/timeScale/LoadScene/File IO.

**Not read (gaps, in rough order of risk):**
1. `CareerManager.ToSaveData()` (~L2217) and `LoadFromSaveData()` (~L2632): about 1,000 lines of field mapping. A field saved but never restored (like `gameList` below) is exactly the bug class that lives here. **This is the biggest remaining save-audit gap.** Worth a dedicated pass or a PlayMode round-trip test.
2. `EndMenu.cs`, `TournySelector.cs`, `PlayoffManager_SingleK.cs`, `PlayoffManager.cs` (Page playoff), `CashGames.cs`, `SponsorManager.cs` — only grepped.
3. `AI_*`, `Sweeping/*`, `RandomRockPlacerment.cs` (4,000+ lines) — skimmed only via greps and the handoff notes.
4. Anything in scene/prefab wiring.

Game Center: **not integrated.** No references to GameKit/`Social`/leaderboard APIs in `Assets/Scripts`, and no Apple/GameKit/Unity Gaming Services package in `Packages/manifest.json`. The "leaderboard" code (`HighScoreService`) is a local JSON file. There is no offline/signed-out Game Center behaviour to audit. Also no network code at all (`UnityWebRequest`/`HttpClient`/reachability: none), so the game is offline-safe by construction.

## Live Editor console

Read via `unity command console` / `get_console_logs` at audit time:

- **Errors currently in buffer:** only the intentional ones logged by my save tests (`[CareerSaveService] Failed to load career: JSON parse error…`). No game errors at that moment — a recompile had cleared earlier ones.
- **Errors observed earlier this session (before the recompile cleared them):**
  1. `MissingReferenceException: Camera has been destroyed` at `CameraManager.cs:346` (`ui.depth = 3`), reached from `Rock_Colliders.OutOfPlay` (`Rock_Colliders.cs:161`) ← `OnTriggerExit2D` (`:384`). See finding S-7.
  2. `Some objects were not cleaned up when closing the scene … GameVisualizationSettings`. See finding L-3.
  Both appeared together at play-mode exit, so they look like teardown artefacts; whether they can occur in a player build on a scene change is Suspected.
- **Warnings:** ~35 compiler warnings, all deprecation/unused-variable (`FindObjectOfType`/`FindFirstObjectByType` obsolete, `Rigidbody2D.isKinematic`, `CinemachineVirtualCamera`, `Team.tourRecord`, `FlickShotController.StartPowerPhase`, one unused variable in `StrategyPatternLibrary.cs:508`). No runtime warnings of note. Not ship-relevant by themselves.

## Scene audit (Revision 2)

Parsed all 12 scenes in Build Settings (SplashMenu, Career_Menu, Arena_Selector, Tourny_Menu_1, Tourny_Home_1/2/3K/SingleK, TournyGame, TutorialGame, End_Menu_Tourny_1/FinalScore).

**Clean results (verified, subject to the prefab blind spot above):**
- **No missing scripts** and **no missing prefab sources** in any build scene.
- **No duplicate singleton components** within a scene.
- Singleton placement: `AudioManager` is a root object in `SplashMenu` only (it is the DontDestroyOnLoad carrier). Every scene except Splash carries a root `GameSettingsPersist`; `CareerManager` is a root in Career_Menu, Arena_Selector, TournyGame, Tourny_Home_3K. In `TournyGame`/`TutorialGame`, `TextCalloutManager`, `TutorialGameManager`, `TutorialSequenceManager`, `SweeperManager` and `GameManager` are children of `Managers` (so not persistent).
- **Inactive by design:** in `SplashMenu` and `TutorialGame` the root `GameSettingsPersist` (and in `TutorialGame` the `CareerManager`) are inactive GameObjects, so their `Awake` never runs. The code copes: there are no unguarded `GameSettingsPersist.instance.` or `CareerManager.instance.` dereferences in the scripts (checked by grep), and `GameManager.SetupGame` builds a runtime GSP if none is found.
- Continue works: the Career_Menu load panel's "Next" button is a `Button_1` prefab instance whose override points at `CareerSettings.LoadToCM`.

**Unassigned serialized references on key managers** (many are filled at runtime: `sweeperL/R/T`, `gsp`, etc.). The ones that matter are S-15 (`TournySelector.options`) and S-16 (broom prefabs). Checked and found guarded or dead: `DialogueController.dialoguePanel` (null-checked, `:494`), `GameManager.tournyIntro(GO)` (null-checked, `:2076-2085`), `TournySelector.earningsText` (usage commented out), `TournySelector.dialogueCanvas/dialogueData/backButton` (no uses).

### What the scene pass changed vs Revision 1
| Item | Rev 1 | Now | Why |
|---|---|---|---|
| B-1 Triple-K `gameList` | ship-blocker | withdrawn | Owner: retired. Scene: `PlayoffManager_TripleK` only in unreachable `Tourny_Home_3K`. |
| B-2 tutorial exit / persistent CM | ship-blocker | S-14, limited | Tutorial CM/GSP are inactive in the scene; no persistent tutorial CM. |
| B-3 New-career delete | ship-blocker (suspected) | ship-blocker, **verified** | Button wiring has no confirm. |
| S-4 dead AIGame buttons | should-fix | later (L-13) | Panels are inactive; no visible button reaches them. |
| S-8 stale callout pool | should-fix (suspected) | refuted | Manager is a child object; DDOL is a no-op. |
| S-3 first launch blank Continue | suspected | still suspected | Not resolvable from scenes alone. |
| (new) S-15, S-16, L-12 to L-14 | none | new | Found in scenes. |

---

# 1. Ship-blockers (data loss, crashes, softlocks)

### B-1. ~~Triple-K `gameList` not restored~~ — WITHDRAWN
Owner confirmed Triple-KO was replaced by the two-pool system. The scene check agrees: `PlayoffManager_TripleK` exists only in `Tourny_Home_3K`, and `TournySettings.cs:100-124` never routes there (KO3 goes to `Tourny_Home_1`, which contains the standard `PlayoffManager`). The save/restore gap is real in the code but unreachable. Moved to Later (L-12) as dead-code cleanup.

### B-2. ~~Mid-tutorial exit leaves a persistent tutorial CareerManager~~ — DOWNGRADED to S-14
The scene pass refuted the mechanism for new players: in `TutorialGame` the `GameSettingsPersist` and `CareerManager` objects are **inactive** in the scene and nothing activates them, so no tutorial `CareerManager` becomes persistent.

### B-3. "New career" deletes the save **and its backup** with no confirmation
**Status:** **Verified** (code + scene wiring). The Career_Menu `New` button (`CareerSettings/GameObject/New`, active) has exactly four persistent calls: `CareerSettings.ClearPlayer`, `SetActive` on itself, `CareerSettings.New`, `Button_Colour.ChangeColour`. There is no confirmation step.
**Where:** `CareerSettings.cs:~268-283` (`New()` → `cm.DeleteCareerSave()` → `CareerSaveService.DeleteSave()` which removes `career_save.json` **and** `career_save_backup.json`). No "confirm"/"are you sure" text anywhere in `CareerSettings.cs`. The `newButton` is shown alongside the Continue panel (`:206-258`).
**What goes wrong:** one mis-tap on the New button on the Continue screen irrecoverably destroys a career (both files). There is no undo, and the backup — the thing designed to protect against this — is deleted in the same call.
**Fix:** (1) confirmation dialog; (2) don't delete: rename to `career_save_replaced.json` and only overwrite that slot on the *next* new career.
**DONE (option 2, code only; 21/21 tests green):** `CareerSaveService.ArchiveSave()` copies the current career (main save, or the backup if the main is corrupt) to `career_save_replaced.json` before `CareerManager.DeleteCareerSave()` deletes anything. One previous career is kept; the next New career overwrites it. 3 tests: exact copy kept, archived file loads after restoring it, falls back to the backup / no-op with no save. Recovery today is manual (rename the file back to `career_save.json`); there is no in-game "restore". **Still open:** the confirmation dialog (option 1) needs new UI in Career_Menu, so it is an Editor job, not a hand-edit of the scene.

---

# 2. Should-fix

### S-1. `PauseMenu.GameIsPaused` is static and never reset — FIXED (compiled, not yet play-tested)
**Status:** Verified (code); effect on volume is Code-verified, runtime unverified.
**Where:** `PauseMenu.cs:10`, `:39-47`, `:83-89`. `MainMenu()` sets `timeScale = 1` but leaves `GameIsPaused = true`.
**What goes wrong:** After Pause → Main Menu, the next game scene's `PauseMenu.Update` sees `GameIsPaused == true` and, every frame, runs `FindObjectOfType<AudioManager>()` and forces `am.maxVol = volumeSlider.value`, overriding the player's volume with the slider's initial value; the Escape handler also thinks the game is paused.
**Fix:** `GameIsPaused = false` in `MainMenu()` and in `Start()`; cache the `AudioManager` instead of `FindObjectOfType` per frame.

### S-2. Unbounded `WaitUntil`s in the game loop can softlock
**Status:** Suspected (requires something upstream to fail).
**Where:** `GameManager.cs:361, 371, 439, 441, 454, 582, 612-614, 620, 628, 1767, 1857` (`rockList.Count == 16`, `rockBar.rockListUI.Count == 16`, `rm.rrp.placed`), and `DialogueController.Instance.IsShowing` at `:597, 1792, 2149` (NRE if a scene has no `DialogueController`).
**What goes wrong:** if any rock fails to register (exception during placement, a mid-game resume with a partial list) the coroutine waits forever with no timeout and no log → permanent black/idle screen.
**Fix:** wrap in a helper `WaitUntilOrTimeout(cond, seconds, onTimeout)` that logs and recovers (or returns to the menu); null-check `DialogueController.Instance`.
**Note:** I found a mitigation for the "rock stops before the hog line" case at `Rock_Colliders.cs:89` — so the obvious weak-shot softlock appears handled.

### S-3. First launch with no save showed a blank "Continue" panel — FIXED and play-verified
**Status:** Suspected.
**Where:** `CareerSettings.cs:87-94`: `cm.LoadCareer(); … Player(!cm.gameOver)`. `gameOver` defaults false, and `LoadCareerJSON` just returns on "no save data" (`CareerManager.cs:~336-340`) without setting anything. So `Player(true)` populates the load panel from default `cm` fields (blank name, week 0) and shows "Continue>". The `else` → `New()` branch only runs when `cm.gameOver` is true.
**What goes wrong:** a brand-new player may see an empty career card and a Continue button; Continue routes through `LoadToCM` → `Arena_Selector` with an uninitialised career.
**Confirm:** delete `career_save.json`, load `Career_Menu`. **Fix:** `Player(CareerSaveService.SaveExists() && !cm.gameOver)`.

### S-4. ~~Dead menu buttons: `AIGame` / `AIGame_2` / `AiGame` not in Build Settings~~ — DOWNGRADED to Later (L-13)
Still true that those scene names aren't in Build Settings, but the scene pass shows the UI that reaches them is hidden: in `SplashMenu` the `GameSettings` and `ContinueMenu` panels are inactive and **no visible button activates them** (visible main-menu buttons: Tutorial, Career, Options; `QuitButton` and `ContinueButton` are inactive). Players can't reach them.

### S-5. `QuickTestGame` ships in release builds
**Status:** Verified (no `#if` guard).
**Where:** `CareerManager.cs:177-181` adds it at runtime; `QuickTestGame.cs:71-100` polls `Input.GetKeyDown` hotkeys (Q / W / runback key) every frame.
**What goes wrong:** an iPad with a hardware keyboard hitting Q mid-career runs `StartQuickTestGame` (`:~255-286`), which rewrites `gsp` and loads `TournyGame`; the next autosave / pause save can then persist the bogus state over the real career.
**Fix:** wrap the `AddComponent` and the class in `#if UNITY_EDITOR || DEVELOPMENT_BUILD`.

### S-6. `HighScoreService` can wipe the leaderboard and isn't crash-safe
**Status:** Code-verified.
**Where:** `HighScoreService.cs:22-49` (parse failure → returns a **new empty** `HighScoreData`), `:54-77` (`File.WriteAllText` straight onto `high_scores.json`), `:82-` (`AddCareerEntry` = load → append → save; called at `CareerManager.cs:~780, ~809`).
**What goes wrong:** a truncated `high_scores.json` (kill mid-write) → next career completion loads "empty", appends one entry, saves, permanently erasing the all-time list and trophy cabinet. Same non-atomic write class I fixed in `CareerSaveService`.
**Fix:** reuse the temp-file-then-replace pattern; on parse failure, copy the bad file aside (`high_scores_failed_load.json`) before returning empty.

### S-7. Camera/rocks accessed while a scene is tearing down
**Status:** Observed in the Editor console; mechanism Suspected.
**Where:** `CameraManager.cs:346` (`ui.depth = 3`) via `Rock_Colliders.cs:141-172` (`OutOfPlay` coroutine, `gm.cm.RockFollow(...)` at `:161`) started from `OnTriggerExit2D` (`:384`).
**What goes wrong:** when a scene unloads (or play mode stops) while a rock is crossing a trigger, the coroutine runs after `gm`/the cameras are destroyed → `MissingReferenceException`. In a player build this is a log error, not a crash, but it can interrupt the rest of `OutOfPlay`.
**Fix:** guard at the top of `RockFollow` (`if (ui == null || main == null) return;`) and in `OutOfPlay` (`if (gm == null) yield break;`).

### S-8. ~~Stale `TextCalloutManager` pool after scene reload~~ — REFUTED
`TextCalloutManager` sits at `Managers/CalloutManager`, a **child** object in both game scenes. `DontDestroyOnLoad` on a non-root object does nothing (Unity logs a warning), so the manager is scene-local and is rebuilt with each scene. No stale pool. Residual: that `DontDestroyOnLoad` call (`TextCalloutManager.cs:35`) is a no-op that logs a warning on each load; remove it.

### S-9. Time scale can be left at 0 on unusual exits — FIXED (compiled, not yet play-tested)
**Status:** Suspected.
**Where:** `DialogueController.cs:250` sets `timeScale = 0` for blocking dialogue; restore only on the normal completion path (`:432-434`). `TutorialSequenceManager.cs:451-466` also pauses. Neither has `OnDisable`/`OnDestroy` restoration. The normal exits (`PauseMenu.MainMenu`, `CompleteTutorial`) do reset it; `DialogueManager.cs:135` has a partial safety net.
**What goes wrong:** any scene change while a blocking step/dialogue is showing leaves the next scene frozen.
**Fix:** `OnDestroy`/`OnDisable` → `Time.timeScale = 1f` in both; also set it in `SplashMenu` start as a belt-and-braces.

### S-10. Replaying Tutorial wipes the career's tutorial-completion flag
**Status:** Suspected (behaviour depends on `CheckAutoStartTutorials` conditions I only partially read).
**Where:** `TutorialGameManager.cs:117-120` always `PlayerPrefs.DeleteKey("CompletedTutorials")` on entry; `CareerSettings.cs:~288` does the same on new career.
**What goes wrong:** a player who finishes the tutorial, then replays it from the menu mid-career, may have first-game tutorials re-trigger in real career games.
**Fix:** use a separate key for "tutorial scene run state" vs "career tutorials seen".

### S-11. Per-frame / per-physics-step `Find` calls
**Status:** Verified (code).
**Where:** `Rock_Info.cs:49` (`FindObjectOfType<GameManager>()` in `FixedUpdate` for every rock whose transform changed — i.e. every moving rock every physics step), `PauseMenu.cs:41` (every frame while paused), `EndManager.cs:54,81` (`Update`). `FindObjectOfType` is a scene-wide search; on a mobile CPU during sweeping this is avoidable cost.
**Fix:** cache the `GameManager` in `Awake/Start` (`Rock_Info` is on 16 rocks).

### S-12. Name-based `GameObject.Find(...).GetComponent<>()` chains with no null check
**Status:** Suspected for runtime failure (works today on device per your handoff); fragile by construction.
**Where:** `GameManager.cs:148-149` (`"BG/Boards_CREATED"`), `Rock_Colliders.cs:38-47` (field initialisers using `GameObject.Find("InPlay_Collider"/"House"/"Launcher"/…)`), `GameSettingsPersist.cs:142` (`"GameSettings"`), `:212` (`"GameManager"`), `Rock_Flick.cs:56-71`, `StorylineManager.cs:67-70`.
**What goes wrong:** depends on object names and creation order (`Boards_CREATED` implies runtime creation). A rename, or a rock instantiated before the arena exists, gives an NRE with no useful message.
**Fix:** inject references or `FindWithTag` + null-check with a clear error.

### S-13. Quit-time saves are unreliable on iOS (design note)
**Status:** Verified (lifecycle semantics).
**Where:** `CareerManager.cs:214-242`. `OnApplicationQuit` is not delivered when iOS terminates a backgrounded app; only `OnApplicationPause(true)` is, and it's the only real guarantee. That's what the code relies on (good), but it means the pause-time write must be fast and atomic — which today's save fix improves. Keep the autosave interval and per-turn saves (`GameManager.cs:1343`).

### S-14. Leaving the tutorial from Pause skips `RestoreCareerGspState` (revised from B-2) — FIXED (compiled, not yet play-tested)
**Status:** Code path verified; impact Suspected and limited.
**Where:** `PauseMenu.cs:83-89` vs `TutorialGameManager.cs:588-630` (restore is only called from `GameManager.cs:~1637, ~1675`).
**What actually happens (corrected):** for a brand-new player there is no active GSP in the tutorial scene, so `GameManager.SetupGame` (`GameManager.cs:195-219`) creates a runtime `GameSettingsPersist` (persistent via its own `Awake`) holding tutorial defaults ("Newbie"/"Opponent", rocks 8, `tutorial=true`). If the player exits via Pause then Main Menu, that object survives with tutorial values. `CareerSettings.New()` resets most flags and `GameManager` re-evaluates `gsp.tutorial` per scene (`:204`), so most of it self-heals. For a returning player with a live GSP, the tutorial writes to it (`TutorialGameManager.cs:165-215`: `tourny`, `aiYellow`, team names/colours, `redTeam`), and even the *normal* exit only restores some of those (`RestoreCareerGspState` doesn't restore `aiYellow/aiRed/redHammer/redTeamColour/teamColour/redTeam`).
**Fix:** run the restore from `TutorialGameManager.OnDestroy` (guarded by `_gspSnapshot.captured`) and extend it to the fields above.

### S-15. `TournySelector.options` is never assigned, so Profile > Options throws — FIXED
**Status:** **Verified** (scene + code).
**Where:** `TournySelector.cs:1473-1476` (`Options(bool on) { options.SetActive(true); }`). `Arena_Selector.unity` leaves `options` null and no code assigns it (the field is referenced in only two places). It is wired from `Canvas/Menu2/ProfilePanel/OptionsButton` (active, label "Options").
**What goes wrong:** the player opens the Profile panel on the career screen and taps Options: `NullReferenceException`, nothing happens.
**Fix:** assign the Options panel in the Inspector (in the Editor, not by hand-editing the scene), or null-check and log; if Arena_Selector has no options panel, hide the button.
**Done:** no Options panel exists in Arena_Selector (the volume slider and clear-high-scores live only in SplashMenu), so `Options()` now null-checks and logs a warning, and the dead `OptionsButton` is hidden in the scene (via the Editor, a one-line scene diff). A real in-career options panel is a v1.1 design task.

### S-16. Broom/handle animation feature is half-wired between scenes — ASSIGNED (broom-only), needs a Play-mode look
**Status:** Verified (scene); expected behaviour Suspected.
**Where:** `SweeperManager` in `TournyGame`: `broomLeftPrefab` and `broomRightPrefab` are **unassigned**; in `TutorialGame` they **are** assigned. `SetupBroomOverlay` null-guards them (`SweeperManager.cs:294-303`), so there is no crash, but real career games silently get no broom overlay while the tutorial does.
**Correction (2026-11-21):** TutorialGame does **not** have the broom prefabs assigned either (I misread that earlier); only TournyGame serializes the fields, both empty. **Decision:** left unassigned. SESSION_HANDOFF.md says the feature is mid-setup: `NC_Broom` is missing from `Sweeper Layers` on `Broom_Left 1` (the broom would not animate), `Broom_Right` is unchecked, the character body is folded into the same prefab (could double the sweeper sprites), and nothing has been validated in Play mode. Assigning it now would ship an unvalidated visual to every career game. Finish handoff steps 1-4 in the Editor first; ship v1 without it if it is not clean.
**UPDATE (same day, you chose broom-only, no characters):** done through the Editor. Both broom prefabs: `Sweeper Layers` = [NC_Broom], `Broom Layer` = NC_Broom, `Colour 1GO` = [NC_Broom's SpriteRenderer] (wooden tier stays white, others take the side colour, per `SweeperParent.SetBroom`); the leftover `NC_Wood` (Left) and `NC_Colour` (Right) layers are switched off, not deleted. `broomLeftPrefab`/`broomRightPrefab` are now assigned on `SweeperManager` in TournyGame. **Not yet seen in Play mode** (art, positions, tier swapping, tint): needs your eyes. The scene diff also contains one unrelated UI position change (`m_AnchoredPosition` y -40 to -15) that Unity re-saved on load.
**CORRECTION 2 (after your screenshots):** the real sweepers were already these two prefabs (`sweeperRedL/R`, `sweeperYellowL/R` point at `Broom_Left 1` / `Broom_Right`), so assigning them as `broomLeftPrefab`/`broomRightPrefab` spawned a second copy on top (doubled sweepers) and my `HealthBar1` switch-off hid the real sweepers' health bars. Both undone: overlay fields cleared again in TournyGame, `HealthBar1` back on. `SweeperManager.SetupBroomOverlay` now applies the handle-tier art and tint straight to the sweepers in play when no overlay prefab is assigned (no-op for prefabs without a `broomLayer`). Tee sweepers (skip) are not tier-swapped. Needs one Play-mode look. Lesson: check what already references a prefab before adding another reference.
**Fix:** decide: assign them in `TournyGame` (and re-test sweeping with the four sweeper layers), or revert the tutorial scene. This matches your handoff note that the Inspector step wasn't finished.

---

# 3. Later

- **L-1. Triple-save churn.** Every turn (`GameManager.cs:1343` → `gsp.AutoSave` → `cm.SaveCareer`) does a full synchronous JSON write; plus a 30 s timer and pause saves. With today's fix, each save also re-parses the existing file to decide whether to rotate the backup (`CareerSaveService.IsValidSaveFile`). Suspected small hitch on older iPhones; measure real save size/time on device, and cache "last save known valid" in memory if it matters.
- **L-2. `GameSettingsPersist2.cs`** — near-duplicate of `GameSettingsPersist` with its own DDOL singleton and `targetFrameRate`; **zero references** in `Assets/Scripts`. If it's on a scene object it creates a second persistent object. Delete (check scenes first).
- **L-3. `GameVisualizationSettings.Instance` creates a GameObject on access** (`GameVisualizationSettings.cs:10-21`); calling it from `OnDestroy`/teardown recreates it and triggers the "not cleaned up when closing the scene" error. Add an `applicationIsQuitting` guard.
- **L-4. Legacy `EasyFileSave` / `BinaryFormatter`** still instantiated in ~10 places (`GameManager.cs:150`, `GameSettingsPersist.cs:173,1111`, `CareerSettings.cs:428`, playoff managers, `TournyManager.cs:147`). `BinaryFormatter` is deprecated/insecure. After B-1 is fixed, migrate and remove.
- **L-5. `com.unity.ads.ios-support` in `manifest.json` with no ad code.** Remove to avoid unneeded App Store privacy/ATT surface.
- **L-6. `FlickShotController` uses string-based reflection** (`GetField("shotTaken")`, `GetProperty("minVelocity")`, `:210-231, 865-890, 1907-1915`). iOS stripping level is Low (`iPhone: 1`) so this is probably safe; if stripping is raised, these silently become null. Prefer direct references.
- **L-7. Debug code in the build:** `Debug/Debug_Shooting.cs`, `Story_1.cs`/`StoryManager.cs` (StoryGame scenes are not in the build; `GameSettingsPersist.StoryGame()` does `GameObject.Find("StoryManager")` with no null check, `:1181`). Strip or `#if` them.
- **L-8. Log volume.** Heavy `Debug.Log` in hot paths (`Rock_Colliders`, `GameManager.NextTurn`, `CareerManager`). Set `Debug.unityLogger.filterLogType` / stack-trace type `None` for release.
- **L-9. Compiler warnings** (~35): migrate off `FindObjectOfType`, `Rigidbody2D.isKinematic`, `CinemachineVirtualCamera`, `Team.tourRecord`.
- **L-10. Carry-overs from the save work:** corrupt save with no backup still starts a fresh career with **no in-game message** (the bad file is now preserved as `career_save_failed_load.json`, but the player isn't told); no save-version migration path (needed for v1.1); `CheckStorageSpace` only tests folder writability; `loadedFromSave = false` is set on every save (`CareerManager.cs:~903`) — check against the equipment-shop generate/load logic.
- **L-12. Triple-K dead code.** `PlayoffManager_TripleK`, `Tourny_Home_3K` (a 1.1 MB scene in Build Settings, unreachable), the `gameList` save/restore stub (`CareerManager.cs:~3103-3125`), and the legacy `EasyFileSave` reads in `PlayoffManager_TripleK.cs:202-257`. Remove from the build to shrink the app and reduce save-path surface.
- **L-13. Hidden/legacy UI and wiring** (all on *inactive* objects, so not player-visible; verified from scenes): Splash `GameSettings` panel (its `Red`/`Yellow` buttons call `GameSettings.SetHammerRed/Yellow`, which don't exist; its `PlayButton` loads `AiGame`), Splash `ContinueMenu` panel and `ContinueButton` (wired to `MainMenu.Tutorial`), Career_Menu `Start`/`Back`/`Clear` (targets `TournySettings.*` on None), `Tourny_Home_2` Play/NextWeek/GameOver buttons (target None; scene only reached for cash games), Arena_Selector `ProfilePanel/Buttons` group (`SpendXPButton` calls `TournySelector.XPWindow`, which **no script defines**; `CardsButton` calls `PowerUpManager.ViewCards`, a class that no longer exists), the `Tourny_Menu_1` scene (never loaded in code; `Clear` calls `TournySettings.ClearPlayer`, which doesn't exist), and `TournyGame`/`TutorialGame` StrategyCanvas buttons with a stale `TournySettings.Help` call. **Resolved in Revision 3:** skill-point spending is live in `Team > XPPanel` (`Canvas/Menu2/TeamPanel/MainFrame/XPPanel`, wired to `XPManager.ButtonAdd/Subtract`) and cards are live in `SponsorPanel` (`Card_Select`, `SponsorManager.CardClick`). The inactive `ProfilePanel/Buttons` group is dead legacy UI and can be deleted.
- **L-14. Direct-play of non-Splash scenes in the Editor** will NRE (`PauseMenu.Start` calls `am.PlayBG`) because `AudioManager` exists only in `SplashMenu` (root, DontDestroyOnLoad). Not a player issue (the build starts at Splash), but relevant to QA/test workflow.
- **L-11. `TextCalloutManager.Instance`** logs an error and does a `FindObjectOfType` on every access when no instance exists.

---

# 4. Arena_Selector deep-dive (Revision 3)

**Scope:** `Arena_Selector.unity` (the career hub) and the scripts it drives: `TournySelector` (read in full), `TournySettings`, `XPManager`, `SponsorManager`, `EquipmentManager` (buy flow), `TeamMenu` (setup), plus the scene's Inspector data and button wiring. Live console: no errors; only compiler warnings (including the `FindFirstObjectByType` deprecation) and the logs from my own save tests.
**Not read in full:** `TeamMenu` (704 lines, only setup), `EquipmentManager` shop generation (~700 lines), `ProvStandings`/`TourStandings` row building, `CareerManager.NextWeek/EndCareer`. Nothing here was run in Play mode; every item says so.

**What the scene itself looks clean on:** no missing scripts, all 5 `menuButtons`/3 `panels`/3 `panelGOs` are assigned, all tournament arrays have no null entries, and `CareerManager`/`GameSettingsPersist` duplicates in the scene self-destruct on arrival (`CareerManager.cs:163-175`). Two inactive buttons are dead (see A-12).

## Ship-blocker candidates (economy; not crashes)

### B-4. ~~Entry fee is charged twice, and backing out loses it~~ — FIXED (4 fee tests pass; 18/18 EditMode green on 2026-11-21; in-game cash check still to do by hand)
**Was:** `TournySelector.PlayTourny` charged `entryFee + costPerWeek` when the event was picked, `TournySettings.LoadToGSP` charged `entryFee` again at Start, and `Back()` refunded only `costPerWeek`. Every paid event cost 2x, and tapping an event then Back lost the fee.
**Fix made:** one charge, at pick time, via new `TournyFees.Total(tourny, costPerWeek)` (`Assets/Scripts/Tourny/TournyFees.cs`); `LoadToGSP` no longer charges; `Back()` refunds the same `TournyFees.Total`. Tests: `Assets/Editor/Tests/TournyFeesTests.cs` (total = fee + weekly; charge-then-refund returns the starting cash; free event; null-safe). The compile finished with no errors, but the Editor stopped answering the CLI before I could run the tests (see status below).
**Check by hand (1 min):** note cash, pick a $1,000 event, confirm cash drops by fee + weekly cost once; press Start and confirm it does not drop again; repeat with Back and confirm cash returns exactly.
**Left alone (A-3):** the panel's affordability check still compares only `entryFee` to cash, so a player can pick an event they can't fully afford; use `TournyFees.Total` there next.

### B-5. ~~Selecting a tournament marks every visible panel complete~~ — DOWNGRADED and FIXED (S-level, ID collisions)
**Status:** Fixed in code; 6 EditMode tests pass. Runtime on device/Editor Play **not yet verified** (see the check below).
**Correction (from you):** consuming the other offered events when you commit to one is **intended** (so they don't come back). My first reading of that as a bug was wrong, and it is no longer a ship-blocker.
**What was actually wrong (code-verified, from the scene data):** completion was applied and undone by raw `Tourny.id`, and IDs are shared across arrays: `emptyTourny.id = 0` equals `tournies[0]` ("Fall Rookie Invitational"); `locals` ids 50-52 equal `provQual` ids 50-52 ("Tour Qualifier"). Consequences:
1. In weeks 4-5 a local event on screen (random id 50-52) silently marked a **different qualifier** complete, so qualifiers you were never offered could be burned, ending the qualifying phase early and risking the Tour/World Championship path.
2. `TournySettings.Back()` un-completed by id too, so backing out of week 2 or later with an empty panel on screen cleared `tournies[0]` and re-opened Fall Rookie for replay.
**Fix made (small, identity-based, keeps your design):**
- New `TournyCompletion` (`Assets/Scripts/Tourny/TournyCompletion.cs`): `MarkOffered` completes every offered, not-yet-complete event, **skipping the empty placeholder and the locals**, and returns exactly what it changed; `Undo` restores exactly those.
- `TournySelector.PlayTourny` now calls it (replacing the four id-matching loops) and remembers the list; new `UndoPendingCompletion()`.
- `TournySettings.Back()` calls `UndoPendingCompletion()` instead of its id-matching loops. An already-complete event that was re-offered (the all-complete fallback) is left alone.
- Tests: `Assets/Editor/Tests/TournyCompletionTests.cs` (consumes the other real events; local doesn't consume a same-id qualifier; empty doesn't touch id-0 event; Undo restores exactly; already-complete survives; null-safe).
**VERIFIED in Editor Play mode (2026-11-21, real week-4 career, in-memory only):** offered Club Night (local, id 51), Tony's Pizza Cup (#6), Match Town Trophy (#5). Picking Tony's consumed Match Town (intended) and left unplayed qualifier 51 untouched (before the fix it would have been burned by the local's id). Back restored the completion set and the offers exactly. B-4 in the same run: cash 20,000 -> 19,600 (100 fee + 300 weekly) once; Start did not charge again; Back returned it to 20,000.
**(Original hand-check, now done):** Editor Play a career to week 4 with a local on screen; pick the right-hand event; confirm the other real events leave and the unseen qualifiers (`Tour Qualifier` 2 and 3) are still available next week; then repeat and press Back to confirm all return.
**Related, suspected, not changed:** the save stores completion by `Tourny.id` and restores it only into `tournies`/`tour`/`provQual` (`CareerManager.cs:605-665`), so progress in the debug `singleKO` mode (whose events are `singleKOs[]`, ids reused from `tournies`) may not survive a reload. It only matters if that mode ships; the IDs themselves are in the save format, so I left them alone.

## Should-fix

### A-1. Spent skill points can be lost or duplicated by a save/kill — FIXED (tests green)
**Status:** Code-verified; runtime unverified.
**Where:** `XPManager.cs:63-81, 93-99, 205-233`; `CareerManager.cs:2236` (`data.skillPoints = skillPoints`). `XPManager` lives on the always-active `TeamSelector` object, so `SaveToCareerManager` runs only on `OnDisable`/`OnDestroy` (scene unload). `ButtonAdd/Subtract` change `skillPoints` locally and write stat changes into `cm.activePlayers`/`cm.cStats` immediately.
**What goes wrong:** spend points in Team > XPPanel, background the app and have iOS kill it. The pause-time save contains the **new stats and the old `skillPoints`**. After relaunch the player has the upgraded stats **and** the points back, so they can spend them again. (Level/xp have the same staleness.)
**Fix:** call `SaveToCareerManager(cm)` at the end of `ButtonAdd/Subtract`, and in `OnApplicationPause`. Better, make `cm.skillPoints` the single source of truth. This is a save/load bug and belongs with your priority #1; I'd add it to the round-trip test.

### A-2. Skill-point exploit: Minus at 0 refunds a point without changing a stat — FIXED (tests green)
**Status:** Code-verified (sliders are 0-100 in the scene); runtime unverified.
**Where:** `XPManager.cs:220-233` (`skillPoints++` unconditionally); `Update` only hides the **plus** buttons when `skillPoints <= 0` (`:115-132`). `ButtonAdd` likewise decrements even when the slider is already at 100.
**What goes wrong:** tap Minus on a stat at 0 repeatedly for unlimited points; Plus at 100 wastes points.
**Fix:** only adjust `skillPoints` if the slider value actually changed (compare before/after). Also decide whether Minus should be allowed below a stat's purchased baseline.

### A-3. `PlayTourny` lets players enter with cash below the true cost
**Status:** Code-verified.
**Where:** `TournySelector.cs:1139` checks `entryFee > cm.cash` only, but `:1281-1282` then subtracts `entryFee + costPerWeek` (and `LoadToGSP` another `entryFee`, see B-4).
**Seen for real:** the live `career_save.json` (week 4, "Fish") has cash **-9,575**. Loading it and opening Arena_Selector takes the bankruptcy branch at `TournySelector.cs:~1104` (`cash < costPerWeek`), which calls `slm.EndOfGame(5)` and throws a NullReferenceException in `DialogueManager.StartDialogue` (`DialogueManager.cs:49`), aborting `SetUp` so the week screen shows three empty panels (suspected cause: dialogue UI not wired in that scene; not investigated). So a negative-cash career can soft-lock on load.
**What goes wrong:** cash goes negative; next load `cm.cash < cm.costPerWeek` triggers the bankruptcy ending (`:806`) with a confusing "Game Over".
**Fix:** compare against the real total once B-4 is settled; disable the panel, not just the button colour.

### A-4. `EndOfSeason` can be started repeatedly, and runs the ending twice — FIXED (compiled, not play-verified)
**Status:** Code-verified; runtime unverified.
**Where:** `TournySelector.cs:693, 747, 811, 835, 860, 890, 915, 990, 1044, 1108, 1174-1183`. `SetActiveTournies` is called from `Start`/`SetUp`, `TournyButton`, `NextWeekButton`, and `TournySettings.Back()`. Each call that reaches a game-over branch starts another `EndOfSeason()`. In the cash/default week>6 path the game-over block (`:734-748`) and the bankruptcy check (`:806-812`) can both fire in one call, calling `slm.EndOfGame(...)` twice.
**What goes wrong:** `cm.EndCareer()` and `LoadScene("SplashMenu")` run more than once, dialogues overlap, and `EndOfSeason` waits on `slm.dialogueGO` forever if `slm` was destroyed. A tap on Tourny after the career is over restarts it.
**Fix:** guard with `if (cm.gameOver) return;` at the top of `SetActiveTournies` and a single `bool endingStarted`; make the bankruptcy check `else if`.

### A-5. Menu panel can fail to open; coroutines race on fast taps
**Status:** Suspected (code-verified wait, device behaviour unverified).
**Where:** `TournySelector.cs:1798` (`yield return new WaitUntil(... sizeDelta.y >= 799f)`), `:1735-1736` (`am.PlayBG` with no null check), `:1726` (`Array.IndexOf` can return -1 for a button not in `menuButtons`, then `menuButtons[-1]`).
**What goes wrong:** the menu panel only opens when the expand animation reaches exactly 799+ px tall. On a device/canvas scale where the animated height tops out lower, or if the Animator doesn't advance (time scale 0 from a leaked pause, S-9), the panel never opens and nothing tells the player. Two taps in quick succession start two coroutines; the first can open its panel over the second. If `AudioManager` is missing the coroutine dies at line 1736.
**Fix:** open the panel on an animation event or after a bounded timeout (e.g. 1 s); cancel the previous coroutine before starting a new one; null-check `AudioManager`.

### A-6. `SetActiveTournies` progression logic is fragile
**Status:** Suspected (read-only; needs a playthrough).
**Where:** `TournySelector.cs:520-553, 772, 783, 876, 880` (and the default-mode copy).
**What goes wrong:** (a) `tourniesComplete` ends up reflecting only the **last** array entry examined (it is overwritten each iteration), so out-of-order completion can make a season look finished or unfinished; (b) `nextTourny2 == 0` / `nextTourny3 == 0` are used as "none found", but 0 is also a valid index, so if the first unfinished event is at index 0 the second is wrongly treated as empty; (c) when all events are complete, `tournies[nextTourny]` falls back to index 0 and can offer an already-completed event; (d) several branches leave `activeTournies` slots from the previous call, and `SetPanels` then reads them. Combined with B-5 this is where "events vanish" comes from.
**Fix:** build lists of unfinished events and take items by position (`List<Tourny> remaining`), use `-1` for none, and rebuild all three slots every call.

### A-7. Week-1 cash reset on every call
**Status:** Code-verified.
**Where:** `TournySelector.cs:617-625, 817-820, 842-845, 866-869, 897-900` (`cm.cash = 100000` when `cm.week == 1`).
**What goes wrong:** in the cash/tour/KO/tournies/provQual modes, any call to `SetActiveTournies` during week 1 (reload, Back, tapping the Tourny button) resets cash to $100,000 and re-triggers `slm.FirstFiveTrigger()`. After paying a fee, tapping Back refunds cash to a reset 100000 (hiding B-4 in those modes).
**Fix:** set starting cash once, when the career is created (`CareerManager.NewSeason`/`New`), not in a view-refresh method.

### A-8. Mid-flow saves: tournament marked complete and cash paid before a game exists — FIXED (compiled, not play-verified)
**Status:** Code-verified; matches your save priority.
**Where:** `TournySelector.cs:1279-1285` saves **twice** around cash deduction, both **before** `TournyMenuLoad`/Start. `SetActiveTournies` also ends with `cm.SaveCareer()` (`:1115`) every refresh.
**What goes wrong:** if the app is killed on the Confirm screen (before Start), the save says the tournament is complete and the money is spent, but no game exists; on relaunch there is nothing to resume. Combined with B-5 it also freezes the wrong completion list into the save.
**Fix:** save once, at Start (`LoadToGSP`), when the game state exists; keep Back as a clean undo with no persisted partial state.

### A-9. Shop and sponsor purchases are not saved immediately — FIXED (compiled, not play-verified)
**Status:** Code-verified.
**Where:** `EquipmentManager.cs:579-635` (cash/inventory/active IDs updated in memory, no `SaveCareer`); `SponsorManager.cs:103-120` has `SaveCareer` commented out (`:114`).
**What goes wrong:** a purchase is lost if the app dies before the next 30 s autosave or pause save; the player keeps the item in memory but not on disk, or the reverse if the cash write lands first. Low odds but easy to hit when buying and immediately swiping the app away.
**Fix:** call `cm.SaveCareer()` at the end of `Confirm(true)` and `AssignPoints`.

### A-18. Equipped items reset to the wrong slots after returning to the arena — FIXED (4 unit tests; not yet run end-to-end on device)
**Where:** `EquipmentManager.SetInventory` non-"loadedFromSave" branch (`activeEquip[i] = inventory[i]`).
**What went wrong:** `CareerManager.SaveCareer` clears `loadedFromSave`, and A-9 made every purchase save, so the next `SetInventory` ignored `activeEquipID` and equipped the first four owned items across all categories. Owning two handles put the second handle in the head slot, and the bought handle was not equipped. Found on the phone.
**Fix:** `SetInventory` always resolves from `activeEquipID` via `ResolveActiveEquip` (per-slot fallback to that category's first owned item). `EquipmentResolveTests`.

**Second cause (the main one, found in the Editor log after a Play test):** match scenes have no `EquipmentManager`, so the save made when a tournament starts wrote an empty `allEquipment`. Back in the arena the shop fell back to the scene's Inspector defaults (handles 0-3 at $0/$500/$1000/$2000) and the bought item was gone. `CareerManager` now keeps `lastEquipmentSnapshot` (falls back to the on-disk copy) and writes that when there is no EquipmentManager; cleared on new career. Test: `SaveFromAMatchScene_KeepsTheOwnedEquipmentCollections`.

### A-10. Possible out-of-range on week in `SponsorManager.AssignPoints`
**Status:** Suspected.
**Where:** `SponsorManager.cs:110-111` (`idPUList[cm.week]`); `idPUList` is allocated from `pUpList.localSponsors.Length` (`:188`).
**What goes wrong:** if a career runs past that many weeks, `IndexOutOfRangeException` when choosing a card. I did not confirm `localSponsors.Length` against the maximum week.
**Fix:** bounds-check or size the array from the season length.

### A-11. `Profile()` rank display indexes the wrong list
**Status:** Suspected.
**Where:** `TournySelector.cs:1363-1379` loops `i < cm.teams.Length` but indexes `cm.provRankList[i]`; `:1367` compares `playerTeamIndex` with `team.id`; ranks of 0 or the 11th-13th produce no/odd suffix logic.
**What goes wrong:** `ArgumentOutOfRangeException` when `provRankList` is shorter than `teams` (e.g. before the first full standings build); the Profile button then fails to open.
**Fix:** iterate `provRankList.Count`; compute the ordinal in one helper (also handle 11th/12th/13th).

## Later (Arena_Selector)

- **A-12. Two dead buttons wired to private/missing methods.** `MainMenu/QuitButton` targets `TournySelector.EndOfSeason`, which is a **private IEnumerator** (UnityEvents can't call it); the button is on an inactive parent, so players never see it. `ProfilePanel/Buttons/{SpendXPButton,CardsButton}` call methods that no longer exist (already in L-13; those buttons live in the inactive group).
- **A-13. `XPManager` label/field mismatch.** `aimSlider` is loaded from `weight` and `weighttSlider` from `aim` (`XPManager.cs:162-163, 171-172, 187-188, 196-197`). The round trip is consistent, so it works, but if the "Aim" label on screen sits on `aimSlider`, upgrading "Aim" actually raises weight (suspected; check the labels).
- **A-14. XP threshold differs between a live session and a reload.** `LoadFromCareerManager` recomputes `xpToNextLevel = 100 + (level-1)*20` (`:87`), but `AddXP` multiplies by 1.2 per level (`:143`), and `xp` is truncated to int (`:86`). Level progress drifts after reload. Cosmetic/progression.
- **A-15. `SponsorManager.PreviewPoints/UnPreviewPoints` mutate `cm.modStats`/`oppStats` with `+=`/`-=`** (`:122-155`). If a preview is not paired with an un-preview (scene change mid-preview, double tap), stat modifiers drift permanently (suspected).
- **A-16. `TournySelector.Update` runs the scroll-snap lerp every frame** (`:415-455`) and writes `hScroll.value` (firing `onValueChanged`) even when the scroll view is hidden. Gate on `tournyScrollGO.activeInHierarchy`.
- **A-17. `FindFirstObjectByType` ordering on DDOL duplicates.** The new scene's `CareerManager`/`GameSettingsPersist` self-destruct, but `Destroy` is deferred, and the compiler warning itself says `FindFirstObjectByType` "relies on instance ID ordering". `TournySelector.Start` (`:104`) works only if the older, persistent instance is the one returned. Prefer `CareerManager.instance` over `Find*` in these hot paths (also silences ~15 warnings).
- **A-18. `TournySelector.WaitForDialogue` (`:390`) is never started** (dead code); The `mode` strings (`"cash"`, `"tour"`, `"singleKO"`...) are compared by string against `cm.debug`, so a typo silently falls into `default`. Replace with an enum.
- **A-19. `SetPanels` only sets the location-bar colour in the "too expensive" branch** (`:1141-1143`) and never restores it in the affordable branch (`:1147`), so a panel that becomes affordable after a refresh keeps the dimmed bar. Cosmetic.
- **A-20. Editor-only side effect:** `Tourny` is a `[Serializable]` class inside the scene, so `complete` flags set while playing in the Editor are not asset changes, but re-entering Play re-reads whatever the scene file holds. If you ever see "tournaments already complete on a new career", check the scene's serialized `complete` values (I checked the current scene: all arrays serialize as not complete).


---

# Notes on today's changes (regression risks I introduced)

Not findings in the original code, but you should know what changed before shipping (`CareerSaveService.cs`, two lines in `CareerManager.cs`, tests under `Assets/Editor/Tests/`):

1. **Invalid-save refusal.** `SaveCareer` now rejects data that `ValidateSaveData` would reject (blank `playerName`, no teams, `version < 1`). If any legitimate early-career moment saves before those exist, that save is now refused (error logged) instead of written. Previously it would have been written but unloadable, so the *outcome* is the same or better, but verify one full new-career → first save on device.
2. **`ValidateSaveData` mutates** `currentGameState.gameInProgress` to false for malformed in-progress states. That now happens at save time too (previously only at load). `GameStateData`'s constructor makes `endScores` non-null and `SaveCareerJSON` clamps `rocks`/`ends`, so I don't expect it to trigger, but it's a behaviour change.
3. **`File.Replace` on iOS/IL2CPP is untested.** It should work (Mono implements it via rename) but I have only run it in the Editor on Windows. Do one device cycle: play, background, force-kill, relaunch, Continue.
4. **Test coverage is the file layer only** (8 EditMode tests, all passing). `ToSaveData`/`LoadFromSaveData` field mapping is untested — see Coverage gap #1.

# Suggested order of work

1. ~~B-5~~ **done and verified in Play mode**. ~~B-4~~ **done and verified in Play mode**. ~~B-3~~ **done (previous career kept; confirm dialog still open, Editor job)**. Next: **A-3** (it is now a real, observed soft-lock), then **A-1/A-2**.
1b. ~~A-3~~ **fixed**: panels dim unless cash covers fee + weekly cost (`TournyFees.CanAfford`), and game over now fires when **nothing offered is affordable** (`AnyAffordable`, both bankruptcy blocks in `SetActiveTournies`; any affordable event, including a free local, keeps the run alive - per design: only winners are rewarded, but nobody is locked out while they can still play). The game-over screen itself is the next piece. The `DialogueManager.StartDialogue` NRE on that path is still open.
1c. ~~A-1/A-2~~ **fixed**: `SkillSpend` (a point is spent or refunded only if the slider moved; no points at 0 means no Plus) and `XPManager` copies points to `CareerManager` on every change (add/subtract/XP/milestone), so a pause-time save is consistent. 5 tests. Decision left as is: Minus still refunds on any stat (free respec).
1d. ~~New-career routed into the old career (week-4 end menu)~~ **fixed in code**: a loaded save restores `gsp.inEndMenu`/`justFinishedGame`/`tournyIntroShown`, and `New()` never cleared them, so `LoadToCM` sent the new career to the old game's end menu. `NewCareerState.ResetGameFlow` now clears them (1 test). Not reproduced end to end: opening Career_Menu with this save auto-ran `New()` and rewrote the save before I could observe it (restored). Worth knowing: whenever `Player(false)` runs, Career_Menu starts a new career by itself, which (with `ArchiveSave`) now keeps your old one as `career_save_replaced.json`.
Play-mode tip: Unity only runs Play mode while focused; `Application.runInBackground = true` (via eval) lets the CLI drive it.
2. **S-15** (Profile > Options NRE) and **S-16** (broom prefabs in `TournyGame`): both are Inspector-level fixes. Do them in the Editor, not by editing scene files.
3. **S-1, S-9, S-14** (pause / time-scale / tutorial-exit state): one small PR.
4. **S-3** (first-launch screen): 2-minute Editor repro first.
5. **S-5** (`QuickTestGame` guard), **S-6** (high-score atomic write), **S-2** (wait timeouts), **S-11** (cached lookups).
6. Dedicated pass on `ToSaveData`/`LoadFromSaveData` with a PlayMode round-trip test (still the largest unread save code).
7. Cleanup: L-12 / L-13 (remove Triple-K and the hidden legacy UI from the build).

---

## Update: S-1 / S-9 / S-14 fixed
- **S-1:** `PauseMenu` resets `GameIsPaused` in `Start()` and `MainMenu()`; the per-frame `FindObjectOfType<AudioManager>()` is gone (uses the cached `am`).
- **S-9:** `DialogueController.OnDestroy` and `TutorialSequenceManager.OnDestroy` restore `timeScale = 1` (only for the live singleton, so a destroyed duplicate can't unpause a real pause); `MainMenu.Start` also sets it to 1.
- **S-14:** `TutorialGameManager.OnDestroy` calls `RestoreCareerGspState()` (a no-op once it has already run). **Not extended** to `aiYellow/aiRed/redHammer/redTeamColour/teamColour/redTeam`: the snapshot doesn't hold them and I don't know their correct career defaults. Impact is limited (those self-heal per scene).
- **Status:** compiled; edit-mode tests not re-run after this change (Editor wedged); no play-mode check yet.

## Update: A-4 / A-8 / A-9 fixed (compiled, 41/41 EditMode tests pass; not play-verified)
- **A-4:** every ending in `TournySelector` goes through `BeginEnding(int)`, guarded by `endingStarted`: the first ending in a scene wins, `EndOfSeason` starts once, and a later bankruptcy check can no longer overwrite a season-end ending number.
- **A-8:** `PlayTourny` no longer saves; it sets `CareerManager.saveDeferred`, which makes `SaveCareer` a no-op (so the pause/auto/quit saves can't persist the half-finished pick either). `TournySettings.LoadToGSP` (Start) clears it and saves; `Back()` clears it, refunds and saves. Also cleared on every scene load, so it can't stay stuck.
- **A-9:** `EquipmentManager.Confirm(true)` and `SponsorManager.AssignPoints` now call `cm.SaveCareer()`. (The sponsor save was commented out in the original; I don't know why. Watch for odd sponsor state after a quick kill.)
- **Hand-check:** from Arena_Selector pick an event, then (a) Back: cash and offers return exactly; (b) force-quit on the Confirm screen and relaunch: the career is as before the pick, event not consumed, cash unspent; (c) Start: the pick persists. Buy an item and kill the app: it should still be owned.

## Update: Back from Confirm showed an empty "Confirm" screen — FIXED and play-verified
`TournySettings.Back()` only hid the Confirm panel; `TournyMenuLoad` had hidden all menu buttons and the tournament list and retitled the screen, and nothing undid that. New `TournySelector.ReturnFromConfirm()` (called by `Back()`) restores the buttons and list. Verified in Play mode on a fresh career: after Back the title is "Available Tournies", the list shows, buttons and offers match the pre-pick state, the event is un-completed.
**Not verified:** the A-8 `saveDeferred` flag. My test ran inside a session that already held two `CareerManager`s (a stale DDOL one), so the flag landed on the non-`instance` one. Worth re-checking in a clean session, and it is another symptom of A-17 (`FindFirstObjectByType<CareerManager>` can return a non-`instance` copy).

## Update: A-8 held saves play-verified (clean session, temp save dir)
Pick an event: `saveDeferred` true, cash 1000 -> 700, event complete, a forced `SaveCareer()` was skipped (file unchanged, skip logged). Back: flag false, cash 1000, event un-completed, list restored. Start (`LoadToGSP`): flag cleared and the save written. Real save untouched during the test; see the note in the working log about the quit-save that overwrote it afterwards (restored from `career_save_backup.json`, hash matched).

## Update: new handle still showed the wooden broom — FIXED in code (compiled, 41/41 tests; needs a play check)
**Cause:** `SweeperManager` read the handle tier from `EquipmentManager`, which exists only in `Arena_Selector`; in the match scene `em` is null, so every match got tier 0 (wood).
**Fix:** `CareerManager.activeHandleTier` (published by `EquipmentManager` on sync and on every purchase, saved as `CareerSaveData.activeHandleTier`, 0 for older saves); `SweeperManager.PlayerHandleTier(em, cm)` falls back to it.
**Notes:** the tier comes from the handle's **price** (<500 wood, <1500 fibreglass, <3500 composite, <7500 carbon, else exotic). An existing save picks the tier up the next time the player passes through `Arena_Selector`. Tee sweepers still don't get tier art.

## Update: A-11 / A-10 / A-5 fixed (compiled, 52/52 EditMode tests; not play-verified); A-13 checked
- **A-11:** removed the Profile pre-pass that looped `cm.teams.Length` but indexed `provRankList` (threw when the list was shorter); both rank labels now use `TextReplacementUtility.FormatRank` (made public; correct for 11th-13th), with null guards. 11 `FormatRank` test cases.
- **A-10:** `SponsorManager.AssignPoints` grows `idPUList` if `cm.week` is past its length (it is sized from the local-sponsor count; the season is 36 weeks).
- **A-5:** the menu expand wait is bounded (1.5 s, unscaled time); a second tap stops the first coroutine; `AudioManager` and an unknown button are null/-1-guarded.
- **A-13 (checked, not changed):** in the XP panel the row labelled **Aim** is wired to `aimSlider`, which loads/saves the player's **`weight`** field, and the row labelled **Weight** is wired to the **`aim`** field. The round trip is consistent. It is a real bug only if `Player.weight`/`Player.aim` mean the opposite in gameplay; needs a look at which stat drives accuracy vs. shot weight.

## Update: save/load round-trip test added (56/56 EditMode tests pass)
`Assets/Editor/Tests/CareerRoundTripTests.cs` fills a `CareerManager` (created on an inactive GameObject so `Awake` never runs; `ToSaveData`/`LoadFromSaveData` are private, so they are called by reflection) with distinct values, then compares **every** field after: (1) ToSaveData -> real save file (`CareerSaveService`, temp directory) -> LoadCareer -> a second manager, (2) the same in memory without the file, (3) a second save/load cycle for drift. A fourth test proves the comparison can fail.
**Covered (35 fields + nested):** identity, colour, week/season/cash/earnings/xp/level/skill points, record, cStats/oppStats, qualifications, teams and tour teams (every Team and Player field), team/tour records, active players, equipment ids, `activeHandleTier`, all five card lists, tournament results, trophy lists, two dialogue-flag arrays, story block. **Result: all pass; nothing is lost or drifts.**
**Not covered (needs a TournySelector / EquipmentManager / GameSettingsPersist in the scene, so a PlayMode test):** tournament completion + trophy ids and championship flags, the saved equipment collections, `currentGameState` (mid-game restore), the remaining dialogue-flag arrays, and `gameOver`/`endScenario` (deliberately not saved).

## Update: tournament-completion round trip (found and fixed a real bug) + New-career confirm dialog
- **Bug found by the new tests (`TournyCompletionRoundTripTests`, 3 tests):** championship **trophy wins were never saved** (only `complete` was). After a reload the Tour/Provincial championship trophy was lost (trophy cabinet, and the new ending logic that reads `trophyWon`). Fixed: `CareerSaveData.tourChampionshipTrophy/provChampionshipTrophy`, written on all three save paths (hub, pending data, match-scene fallback) and restored on both load paths. Tournies, tour events and qualifiers already round-tripped correctly, including across two saves.
- **New career confirm (Career_Menu):** the New button now calls `CareerSettings.AskNewCareer()`, which shows a "Start a new career?" dialog (Replace / Cancel) when a save exists, and skips it when there is nothing to lose. Verified in Play mode on a copy of a save: the dialog shows, Cancel leaves the save alone, Replace archives the old career to `career_save_replaced.json` and opens the new-career screen. The dialog was built through the Editor API (`ConfirmNew` under the `CareerSettings` canvas); the Career_Menu scene file shows a large diff because Unity re-serialised it.
- **Tests: 59/59 EditMode.**
- **Still open:** PlayMode-only coverage (saved equipment collections, `currentGameState`, remaining dialogue flags), an iOS device pass (`File.Replace`, background-kill), S-5/S-6/S-2/S-11, A-6, A-13 (confirm what `weight` vs `aim` mean), tee-sweeper broom art, and the stray `m_AnchoredPosition` (y -40 -> -15) change in `TournyGame.unity`.
