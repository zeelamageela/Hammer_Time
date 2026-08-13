using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages tutorial game setup and flow.
/// Handles pre-placing rocks, triggering tutorials, and AI overrides.
/// </summary>
public class TutorialGameManager : MonoBehaviour
{
    public static TutorialGameManager Instance { get; private set; }
    private const int TotalRocks = 16;
    
    [Header("Configuration")]
    [SerializeField] private TutorialGameSetup tutorialSetup;
    [Tooltip("Tutorial setup used when Flick Shot Mode is enabled. Falls back to tutorialSetup if null.")]
    [SerializeField] private TutorialGameSetup flickShotTutorialSetup;

    [Header("State")]
    public bool isTutorialGame = false;

    [Header("Diagnostics")]
    [SerializeField] private bool runTutorialValidationLogs = true;

    // Resolved once in InitializeTutorialGame() — either flickShotTutorialSetup or tutorialSetup
    private TutorialGameSetup activeSetup;
    private GameManager gameManager;

    // Snapshot of career-critical gsp flags captured before the tutorial clobbers them.
    // Restored in RestoreCareerGspState() before returning to SplashMenu.
    private struct GspSnapshot
    {
        public bool gameInProgress;
        public bool tournyInProgress;
        public bool justFinishedGame;
        public bool inEndMenu;
        public bool loadGame;
        public bool captured;
    }
    private GspSnapshot _gspSnapshot;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        // Only auto-initialize in the TutorialGame scene.
        // TutorialGameManager may also be present in TournyGame; without this guard
        // it would trigger the tutorial there too (isTutorialGame starts false, so
        // the old "if (!isTutorialGame)" condition was always true on first frame).
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "TutorialGame")
        {
            Debug.Log("[TutorialGameManager] Not in TutorialGame scene — skipping auto-init");
            return;
        }

        gameManager = Object.FindAnyObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogWarning("[TutorialGameManager] GameManager not found on Start. Will retry shortly.");
            StartCoroutine(WaitForGameManagerThenInitialize());
            return;
        }

        InitializeTutorialGame();
    }

    private IEnumerator WaitForGameManagerThenInitialize()
    {
        float timeout = 2f;
        float elapsed = 0f;
        while (gameManager == null && elapsed < timeout)
        {
            gameManager = Object.FindAnyObjectByType<GameManager>();
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (gameManager == null)
        {
            Debug.LogError("[TutorialGameManager] GameManager still not found after retry; tutorial initialization aborted.");
            yield break;
        }

        InitializeTutorialGame();
    }
    
    /// <summary>
    /// Check if we should start the tutorial game (first career game ever)
    /// </summary>
    public bool ShouldStartTutorialGame()
    {
        // Start tutorial when no CompletedTutorials flag exists
        string completedTutorials = PlayerPrefs.GetString("CompletedTutorials", "");
        bool tutorialsNotDone = string.IsNullOrEmpty(completedTutorials);
        return tutorialsNotDone;
    }
    
    /// <summary>
    /// Initialize the tutorial game setup
    /// </summary>
    public void InitializeTutorialGame()
    {
        if (tutorialSetup == null || gameManager == null)
        {
            Debug.LogWarning("[TutorialGameManager] Cannot initialize - missing setup or GameManager");
            return;
        }

        // Always reset on entry — TutorialGame scene is only reached by explicitly choosing Tutorial,
        // so the player always wants a fresh run regardless of prior completions.
        PlayerPrefs.DeleteKey("CompletedTutorials");
        PlayerPrefs.Save();

        isTutorialGame = true;

        // Pick flick or pull-release tutorial based on the player's shot style setting
        bool useFlick = GameVisualizationSettings.Instance != null && GameVisualizationSettings.Instance.FlickShotMode;
        activeSetup = (useFlick && flickShotTutorialSetup != null) ? flickShotTutorialSetup : tutorialSetup;
        Debug.Log($"[TutorialGameManager] Shot style: {(useFlick ? "FlickShot" : "PullRelease")} — using setup: {activeSetup?.name ?? "NULL"}");

        Debug.Log("[TutorialGameManager] Initializing tutorial game:");
        Debug.Log($"  - Starting End: {activeSetup.startingEnd}");
        Debug.Log($"  - Score: {activeSetup.startingScore.x}-{activeSetup.startingScore.y}");
        Debug.Log($"  - Player has hammer: {activeSetup.playerHasHammer}");
        Debug.Log($"  - Rocks per team: {activeSetup.rocksPerTeam}");
        
        // Get GSP to update runtime team data (if present)
        GameSettingsPersist gsp = Object.FindAnyObjectByType<GameSettingsPersist>();
        
        // Set game state in both GameManager and GSP
        gameManager.endCurrent = activeSetup.startingEnd - 1; // -1 because it increments at start
        gameManager.redScore = (int)activeSetup.startingScore.y;
        gameManager.yellowScore = (int)activeSetup.startingScore.x;
        gameManager.redHammer = activeSetup.playerHasHammer;  // Set hammer from tutorial setup
        gameManager.rocksPerTeam = activeSetup.rocksPerTeam;
        
        // Set rockCurrent based on rocksPerTeam (e.g., rocksPerTeam=2 means rockCurrent=12)
        // This means 12 rocks are "already placed" and player shoots rocks 12-15
        gameManager.rockCurrent = 16 - (activeSetup.rocksPerTeam * 2);
        
        // Resolve player team name: prefer career name from gsp, fall back to CareerManager, then "Newbie"
        CareerManager cm = FindAnyObjectByType<CareerManager>();
        string playerTeam = "Newbie";
        if (gsp != null && !string.IsNullOrEmpty(gsp.redTeamName))
            playerTeam = gsp.redTeamName;
        else if (cm != null && !string.IsNullOrEmpty(cm.teamName))
            playerTeam = cm.teamName;

        string opponentTeam = string.IsNullOrEmpty(activeSetup.opponentTeamName) ? "Opponent" : activeSetup.opponentTeamName;
        Color playerColor = Color.red;
        Color opponentColor = new Color(0.9f, 0.9f, 0.3f);

        // Red team has hammer in tutorial (player team)
        gameManager.redTeamName = playerTeam;
        gameManager.yellowTeamName = opponentTeam;

        if (gsp != null)
        {
            gsp.endCurrent = activeSetup.startingEnd - 1;
            gsp.redScore = (int)activeSetup.startingScore.y;
            gsp.yellowScore = (int)activeSetup.startingScore.x;
            gsp.redHammer = activeSetup.playerHasHammer;
            gsp.rocks = activeSetup.rocksPerTeam;
            gsp.rockCurrent = 16 - (activeSetup.rocksPerTeam * 2);
            gsp.redTeamName = playerTeam;
            gsp.yellowTeamName = opponentTeam;
            gsp.redTeamColour = playerColor;
            gsp.yellowTeamColour = opponentColor;
            gsp.teamColour = playerColor;

            // Set tournament flags
            gsp.tourny = true;
            gsp.cashGame = false;

            // CRITICAL: Set AI flags so RandomRockPlacement runs for the simulated rocks
            // In tutorial, opponent (Yellow) is AI, player (Red) has hammer
            gsp.aiYellow = true;
            gsp.aiRed = false;

            // Create Red team (player) — use career active players if available, otherwise novice defaults
            gsp.redTeam = new Team
            {
                name = playerTeam,
                player = true,
                players = new List<Player>()
            };

            string[] roles = { "Lead", "Second", "Third", "Skip" };
            for (int i = 0; i < 4; i++)
            {
                // If career has active players, copy their stats; otherwise use novice defaults
                int stat = 45;
                if (cm != null && cm.activePlayers != null && i < cm.activePlayers.Length && cm.activePlayers[i] != null)
                    stat = Mathf.Clamp(cm.activePlayers[i].sweepStrength, 20, 100);

                gsp.redTeam.players.Add(new Player
                {
                    id = i,
                    name = playerTeam + " " + roles[i],
                    weight = stat,
                    finesse = stat,
                    aim = stat,
                    sweepStrength = stat,
                    sweepEnduro = stat,
                    sweepCohesion = stat
                });
            }
            Debug.Log($"[TutorialGameManager] Created red team '{playerTeam}' with {gsp.redTeam.players.Count} players");
            
            // Yellow team = AI opponent
            gsp.yellowTeam = new Team
            {
                name = opponentTeam,
                player = false,
                players = new List<Player>()
            };
            
            // Add 4 AI players with MAXED stats (challenge)
            for (int i = 0; i < 4; i++)
            {
                gsp.yellowTeam.players.Add(new Player
                {
                    id = 200 + i,
                    name = i == 3 ? "Opponent Skip" : i == 0 ? "Opponent Lead" : i == 1 ? "Opponent Second" : "Opponent Third",
                    weight = 100,
                    finesse = 100,
                    aim = 100,
                    sweepStrength = 100,
                    sweepEnduro = 100,
                    sweepCohesion = 100
                });
            }
            Debug.Log($"[TutorialGameManager] Created yellow team with {gsp.yellowTeam.players.Count} players");
        }
        
        Debug.Log($"[TutorialGameManager] Team setup: Player={playerTeam}, Opponent={opponentTeam}");
        
        // Snapshot the career flags before the tutorial clobbers them so we can restore
        // them when returning to SplashMenu, keeping career continuation working correctly.
        if (gsp != null && !_gspSnapshot.captured)
        {
            _gspSnapshot = new GspSnapshot
            {
                gameInProgress  = gsp.gameInProgress,
                tournyInProgress = gsp.tournyInProgress,
                justFinishedGame = gsp.justFinishedGame,
                inEndMenu       = gsp.inEndMenu,
                loadGame        = gsp.loadGame,
                captured        = true,
            };
            Debug.Log($"[TutorialGameManager] Snapshotted career gsp state: gameInProgress={_gspSnapshot.gameInProgress}, tournyInProgress={_gspSnapshot.tournyInProgress}, loadGame={_gspSnapshot.loadGame}");
        }

        // Prevent any career game-in-progress save from resuming during tutorial.
        // LoadFromSaveData() may have restored gameInProgress=true earlier; clear it here
        // so GameManager doesn't try to restore career rock positions/scores over tutorial state.
        if (gsp != null)
        {
            gsp.gameInProgress = false;
            gsp.tournyInProgress = false;
            gsp.justFinishedGame = false;
            gsp.inEndMenu = false;
        }
        if (cm != null)
            cm.loadedFromSave = false;

        // Prevent the LoadGame branch in SetupGame from running instead of the tutorial path
        if (gsp != null)
            gsp.loadGame = false;

        // Force GameHUD to refresh with new team names
        GameHUD gHUD = Object.FindAnyObjectByType<GameHUD>();
        if (gHUD != null)
        {
            gHUD.ScoringPanel();
        }

        if (runTutorialValidationLogs)
        {
            ValidatePostInitState();
        }
    }
    
    /// <summary>
    /// Place ONLY the pre-configured rocks, mark all others as out of play
    /// Called from GameManager.SetupGame() after rocks are created
    /// </summary>
    public IEnumerator RepositionPreConfiguredRocks()
    {
        if (activeSetup == null || gameManager == null || !isTutorialGame)
            yield break;
        
        if (activeSetup.prePlacedRocks == null || activeSetup.prePlacedRocks.Length == 0)
        {
            Debug.Log("[TutorialGameManager] No pre-placed rocks configured");
            yield break;
        }
        
        yield return new WaitForEndOfFrame();
        
        Debug.Log("[TutorialGameManager] Placing tutorial rocks");
        
        int rockCurrent = 16 - (activeSetup.rocksPerTeam * 2);
        Debug.Log($"[TutorialGameManager] rockCurrent={rockCurrent}, marking rocks 0-{rockCurrent-1} as placed");
        
        // First, mark rocks 0 to (rockCurrent-1) as placed and out of play
        for (int i = 0; i < rockCurrent; i++)
        {
            GameObject rock = gameManager.rockList[i].rock;
            Rock_Info rockInfo = gameManager.rockList[i].rockInfo;
            
            if (rock != null && rockInfo != null)
            {
                rockInfo.inPlay = false;
                rockInfo.outOfPlay = true;
                rockInfo.stopped = true;
                rockInfo.rest = true;
                rockInfo.placed = true;
                rockInfo.shotTaken = true;
                rockInfo.released = true;
                rock.SetActive(false);
            }
        }
        
        // Update rock bar for all "placed" rocks
        RockBar rockBar = Object.FindAnyObjectByType<RockBar>();
        if (rockBar != null && rockBar.rockListUI != null && rockBar.rockListUI.Count == 16)
        {
            for (int i = 0; i < rockCurrent; i++)
            {
                rockBar.ShotUpdate(i, true);  // Mark as out of play in UI
            }
        }
        
        Debug.Log($"[TutorialGameManager] Placing {activeSetup.prePlacedRocks.Length} pre-configured rocks");
        
        // Now place ONLY the pre-configured rocks
        foreach (var prePlacedRock in activeSetup.prePlacedRocks)
        {
            if (prePlacedRock.rockIndex < 0 || prePlacedRock.rockIndex >= gameManager.rockList.Count)
            {
                Debug.LogWarning($"[TutorialGameManager] Invalid rock index: {prePlacedRock.rockIndex}");
                continue;
            }
            
            GameObject rock = gameManager.rockList[prePlacedRock.rockIndex].rock;
            Rock_Info rockInfo = gameManager.rockList[prePlacedRock.rockIndex].rockInfo;
            Rigidbody2D rb = rock.GetComponent<Rigidbody2D>();
            
            // Enable and position the rock
            rock.SetActive(true);
            if (rb != null)
            {
                rb.position = prePlacedRock.position;
                rb.rotation = prePlacedRock.rotation;
            }
            rock.transform.position = new Vector3(prePlacedRock.position.x, prePlacedRock.position.y, 0);
            rock.transform.rotation = Quaternion.Euler(0, 0, prePlacedRock.rotation);
            
            // Update rock state - this rock IS in play
            rockInfo.inPlay = true;
            rockInfo.outOfPlay = false;
            rockInfo.stopped = true;
            rockInfo.rest = true;
            rockInfo.shotTaken = true;
            rockInfo.released = true;
            rockInfo.placed = true;
            
            // Enable visual components
            SpriteRenderer sr = rock.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.enabled = true;
                
            CircleCollider2D col = rock.GetComponent<CircleCollider2D>();
            if (col != null)
            {
                col.enabled = true;
                col.radius = 0.14f;
            }
            
            // Disable shooting components since this rock is already placed
            Rock_Flick flick = rock.GetComponent<Rock_Flick>();
            if (flick != null)
                flick.enabled = false;
                
            SpringJoint2D spring = rock.GetComponent<SpringJoint2D>();
            if (spring != null)
                spring.enabled = false;
            
            Debug.Log($"[TutorialGameManager] Placed rock {prePlacedRock.rockIndex} ({(prePlacedRock.isYellow ? "Yellow" : "Red")}) at {prePlacedRock.position}");
        }

        if (runTutorialValidationLogs)
        {
            ValidateRockPlacementState(rockCurrent);
        }
    }

    private void ValidatePostInitState()
    {
        GameSettingsPersist gsp = Object.FindAnyObjectByType<GameSettingsPersist>();
        int expectedRockCurrent = TotalRocks - (activeSetup.rocksPerTeam * 2);

        LogTutorialCheck("Tutorial setup assigned", activeSetup != null, "activeSetup reference resolved");
        LogTutorialCheck("Tutorial mode active", isTutorialGame, "isTutorialGame=true");
        LogTutorialCheck("GameManager exists", gameManager != null, "gameManager reference resolved");

        if (gameManager != null)
        {
            LogTutorialCheck("GameManager rockCurrent", gameManager.rockCurrent == expectedRockCurrent,
                $"expected={expectedRockCurrent}, actual={gameManager.rockCurrent}");
            LogTutorialCheck("GameManager hammer", gameManager.redHammer == activeSetup.playerHasHammer,
                $"expected={activeSetup.playerHasHammer}, actual={gameManager.redHammer}");
            LogTutorialCheck("GameManager rocksPerTeam", gameManager.rocksPerTeam == activeSetup.rocksPerTeam,
                $"expected={activeSetup.rocksPerTeam}, actual={gameManager.rocksPerTeam}");
        }

        if (gsp != null)
        {
            LogTutorialCheck("GSP rockCurrent", gsp.rockCurrent == expectedRockCurrent,
                $"expected={expectedRockCurrent}, actual={gsp.rockCurrent}");
            LogTutorialCheck("GSP hammer", gsp.redHammer == activeSetup.playerHasHammer,
                $"expected={activeSetup.playerHasHammer}, actual={gsp.redHammer}");
            LogTutorialCheck("GSP red team ready", gsp.redTeam != null && gsp.redTeam.players != null && gsp.redTeam.players.Count >= 4,
                gsp.redTeam == null || gsp.redTeam.players == null ? "red team missing" : $"red players={gsp.redTeam.players.Count}");
            LogTutorialCheck("GSP yellow team ready", gsp.yellowTeam != null && gsp.yellowTeam.players != null && gsp.yellowTeam.players.Count >= 4,
                gsp.yellowTeam == null || gsp.yellowTeam.players == null ? "yellow team missing" : $"yellow players={gsp.yellowTeam.players.Count}");
            LogTutorialCheck("Team names assigned", !string.IsNullOrEmpty(gsp.redTeamName) && !string.IsNullOrEmpty(gsp.yellowTeamName),
                $"red='{gsp.redTeamName}', yellow='{gsp.yellowTeamName}'");
        }

        // CareerManager checks removed for tutorial-only flow
    }

    private void ValidateRockPlacementState(int rockCurrent)
    {
        if (gameManager == null || gameManager.rockList == null)
        {
            LogTutorialCheck("Rock list exists", false, "rockList unavailable");
            return;
        }

        bool[] shouldBeInPlay = new bool[TotalRocks];
        for (int i = 0; i < TotalRocks; i++)
            shouldBeInPlay[i] = false;

        for (int i = 0; i < activeSetup.prePlacedRocks.Length; i++)
        {
            int idx = activeSetup.prePlacedRocks[i].rockIndex;
            if (idx >= 0 && idx < rockCurrent)
                shouldBeInPlay[idx] = true;
        }

        int inPlayCount = 0;
        int outOfPlayCount = 0;
        bool allCorrect = true;

        for (int i = 0; i < rockCurrent; i++)
        {
            Rock_Info info = gameManager.rockList[i].rockInfo;
            bool expectedInPlay = shouldBeInPlay[i];
            bool actualInPlay = info != null && info.inPlay;
            bool actualOutOfPlay = info != null && info.outOfPlay;

            if (actualInPlay)
                inPlayCount++;
            if (actualOutOfPlay)
                outOfPlayCount++;

            bool rowPass = expectedInPlay ? (actualInPlay && !actualOutOfPlay) : (!actualInPlay && actualOutOfPlay);
            if (!rowPass)
            {
                allCorrect = false;
                LogTutorialCheck($"Rock {i} state", false,
                    $"expectedInPlay={expectedInPlay}, actualInPlay={actualInPlay}, actualOutOfPlay={actualOutOfPlay}");
            }
        }

        LogTutorialCheck("Rock placement integrity", allCorrect,
            $"range=0-{rockCurrent - 1}, inPlay={inPlayCount}, outOfPlay={outOfPlayCount}");
        LogTutorialCheck("Pre-placed count", inPlayCount == CountConfiguredRocksInRange(rockCurrent),
            $"expected={CountConfiguredRocksInRange(rockCurrent)}, actual={inPlayCount}");
    }

    private int CountConfiguredRocksInRange(int rockCurrent)
    {
        int count = 0;
        for (int i = 0; i < activeSetup.prePlacedRocks.Length; i++)
        {
            int idx = activeSetup.prePlacedRocks[i].rockIndex;
            if (idx >= 0 && idx < rockCurrent)
                count++;
        }
        return count;
    }

    private void LogTutorialCheck(string label, bool pass, string details)
    {
        if (pass)
            Debug.Log($"[TutorialCheck PASS] {label} | {details}");
        else
            Debug.LogError($"[TutorialCheck FAIL] {label} | {details}");
    }
    
    /// <summary>
    /// Check if a tutorial should trigger for the current rock
    /// </summary>
    public bool ShouldTriggerTutorial(int rockIndex, out string tutorialId)
    {
        tutorialId = null;

        if (!isTutorialGame || activeSetup == null || activeSetup.tutorialTriggers == null)
            return false;

        int baseRockIndex = TotalRocks - (activeSetup.rocksPerTeam * 2);
        GameSettingsPersist gsp = Object.FindAnyObjectByType<GameSettingsPersist>();
        bool playerIsRed = gsp == null || !gsp.aiRed;
        int firstPlayerRockIndex = playerIsRed
            ? baseRockIndex + (activeSetup.playerHasHammer ? 1 : 0)
            : baseRockIndex + (activeSetup.playerHasHammer ? 0 : 1);

        foreach (TutorialTrigger trigger in activeSetup.tutorialTriggers)
        {
            if (string.IsNullOrEmpty(trigger.tutorialId))
                continue;

            int triggerRockIndex = trigger.triggerOnFirstPlayerRock ? firstPlayerRockIndex : trigger.rockIndex;
            if (rockIndex == triggerRockIndex)
            {
                tutorialId = trigger.tutorialId;
                Debug.Log($"[TutorialGameManager] Triggering tutorial '{tutorialId}' for rock {rockIndex}" +
                    (trigger.triggerOnFirstPlayerRock ? " (first player rock)" : ""));
                return true;
            }
        }

        return false;
    }
    
    /// <summary>
    /// Get AI shot suggestion for a specific rock (if any)
    /// </summary>
    public AIShotSuggestion GetAIShotSuggestion(int rockIndex)
    {
        if (!isTutorialGame || activeSetup == null || activeSetup.aiShotSuggestions == null)
            return null;
        
        foreach (var suggestion in activeSetup.aiShotSuggestions)
        {
            if (suggestion.rockIndex == rockIndex)
            {
                Debug.Log($"[TutorialGameManager] AI suggestion for rock {rockIndex}: {suggestion.suggestedShotType}");
                return suggestion;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// End the tutorial game (called after game completes)
    /// </summary>
    public void EndTutorialGame()
    {
        if (!isTutorialGame)
            return;

        Debug.Log("[TutorialGameManager] Tutorial game completed - continuing tournament normally");
        isTutorialGame = false;

        // Don't mark tutorials complete here - they're marked complete when actually finished
    }

    /// <summary>
    /// Restores the career-critical gsp flags that InitializeTutorialGame() overwrote.
    /// Call this before loading SplashMenu so the player's career continuation state is intact.
    /// Also destroys the tutorial's CareerManager so the career flow can create a properly
    /// configured one with all inspector references intact.
    /// </summary>
    public void RestoreCareerGspState()
    {
        if (!_gspSnapshot.captured)
            return;

        GameSettingsPersist gsp = Object.FindAnyObjectByType<GameSettingsPersist>();
        if (gsp == null)
            return;

        gsp.gameInProgress   = _gspSnapshot.gameInProgress;
        gsp.tournyInProgress = _gspSnapshot.tournyInProgress;
        gsp.justFinishedGame = _gspSnapshot.justFinishedGame;
        gsp.inEndMenu        = _gspSnapshot.inEndMenu;
        gsp.loadGame         = _gspSnapshot.loadGame;
        // Clear tutorial-injected values that don't belong in the career context
        gsp.tourny     = _gspSnapshot.tournyInProgress; // tourny flag should match tournyInProgress
        gsp.tutorial   = false; // tutorial game sets this true; clear it on return to career
        gsp.rocks      = 0;   // career flow will re-set via TournySettings sliders
        gsp.draw       = 0;   // no in-progress tournament draw
        gsp.careerLoad = false;
        // Clear tutorial team names — TournySetup() will assign correct career team names later
        gsp.redTeamName = "";
        gsp.yellowTeamName = "";

        Debug.Log($"[TutorialGameManager] Restored career gsp state: gameInProgress={gsp.gameInProgress}, tournyInProgress={gsp.tournyInProgress}, loadGame={gsp.loadGame}");
        _gspSnapshot.captured = false;

        // The tutorial scene's CareerManager persisted via DontDestroyOnLoad and may lack the
        // full inspector wiring needed for career play (teamPool, playerPool, etc.).
        // Destroy it if it was a fresh tutorial CM (week == 0), so the career flow creates
        // a properly-configured one. If week > 0 the player already had a career — keep it.
        CareerManager cm = Object.FindAnyObjectByType<CareerManager>();
        if (cm != null && cm.week == 0)
        {
            Debug.Log("[TutorialGameManager] Destroying tutorial CareerManager (week=0) so career flow can create a fresh one");
            CareerManager.instance = null;
            Object.Destroy(cm.gameObject);
        }
        else if (cm != null)
        {
            Debug.Log($"[TutorialGameManager] Keeping CareerManager (week={cm.week} — existing career)");
        }
    }
}
