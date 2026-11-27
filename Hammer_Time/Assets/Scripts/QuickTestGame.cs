using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// DEBUG TOOL: Press Q anywhere to instantly start a test game
/// - Bypasses all career/tournament setup
/// - 8 rocks per team
/// - Opponent has maximum stats
/// - Perfect for testing trajectory, sweeping, physics, etc.
/// 
/// HOW TO USE:
/// 1. Add this component to any GameObject in your career/menu scenes
/// 2. Press Q key to instantly jump into a test game
/// 3. No need to set up teams, tournaments, or anything else!
/// </summary>
public class QuickTestGame : MonoBehaviour
{
    
    [Header("Test Game Settings")]
    [Tooltip("Hotkey to trigger instant test game (default: Q)")]
    public KeyCode testGameKey = KeyCode.Q;
    
    [Tooltip("Hotkey to trigger takeout practice mode (default: W) - Sets up 4 red rocks in house, forces yellow AI to take them out")]
    public KeyCode takeoutPracticeKey = KeyCode.W;
    
    [Tooltip("Hotkey to trigger runback practice mode (default: E) - Sets up a guarded target rock, forces yellow AI to run it back through the finesse")]
    public KeyCode runbackPracticeKey = KeyCode.E;
    
    [Tooltip("Number of rocks per team")]
    public int rocksPerTeam = 4;
    
    [Tooltip("Number of ends to play")]
    public int endsToPlay = 1;
    
    [Tooltip("Opponent stats (0-100, 100 = perfect shots)")]
    [Range(0, 100)]
    public int opponentStatValue = 85;
    
    [Tooltip("Your shooter's aim skill (0-100). Low shows a wide fuzz cone and bands; 85 is nearly exact. Gear and training add to this.")]
    [Range(0, 100)]
    public int playerAimSkill = 50;

    [Tooltip("Your shooter's weight skill (0-100). Low shows a tall fuzz band on the distance line.")]
    [Range(0, 100)]
    public int playerWeightSkill = 50;

    [Tooltip("Your shooter's consistency (0-100): the worst possible miss. High shrinks the oval, low widens it. Gear and training add to this.")]
    [Range(0, 100)]
    public int playerConsistency = 50;

    [Header("Pull-back (applies live while a quick test is running)")]
    [Tooltip("The small circle under your thumb. Off is the new look.")]
    public bool thumbCircle = false;
    [Tooltip("How far sideways the rock can be pulled. 0.34 is the old limit; wider aims further for the same thumb travel.")]
    [Range(0.34f, 0.55f)] public float aimCap = 0.40f;

    [Header("Training levels (apply live while a quick test is running)")]
    [Tooltip("Sweep Training: 1 carving, 2 endurance, 3 strength, 4 cohesion")]
    [Range(0, 4)] public int sweepLevel = 4;
    [Tooltip("Tactic Studies: 1 crosshairs, 2 collision warning, 3 hit arrows, 4 exact arrows")]
    [Range(0, 4)] public int tacticsLevel = 4;
    [Tooltip("On-Ice Training: 1 dots + line to the far hog, 2 timing, 3 line halfway to the house, 4 full line")]
    [Range(0, 4)] public int onIceLevel = 4;

    [Header("Fuzz look (applies live while a quick test is running)")]
    [Tooltip("Brightness of the cone along the shot.")]
    [Range(0f, 1f)] public float coneStrength = 0.275f;
    [Tooltip("The faint fill inside the aim oval.")]
    [Range(0f, 1f)] public float ovalFill = 0.16f;
    [Tooltip("The glowing outline of the aim oval.")]
    [Range(0f, 1f)] public float ovalOutline = 0.275f;
    [Tooltip("The crosshair arms.")]
    [Range(0f, 1f)] public float crossStrength = 0.9f;

    [Tooltip("Set BOTH teams to AI for testing? (Press W during game for AI vs AI)")]
    public bool bothTeamsAI = false;
    
    [Tooltip("Randomize who gets hammer? (50/50 chance each time)")]
    public bool randomizeHammer = true; // LOCKED: Red always has hammer
    
    [Tooltip("Randomize starting scores? Creates different strategic scenarios")]
    public bool randomizeScores = true; // LOCKED: Always 1-1
    
    [Tooltip("If randomizeScores is true, use weighted scenarios (more tied/close games)?")]
    public bool useWeightedScenarios = true;
    
    /// <summary>
    /// Call this to DISABLE Quick Test Mode (clears PlayerPrefs flags)
    /// Automatically called when game scene loads if not started via Q key
    /// </summary>
    public static void ClearQuickTestMode()
    {
        PlayerPrefs.SetInt("QuickTestMode", 0);
        PlayerPrefs.SetInt("DisableSweeping", 0);
        PlayerPrefs.Save();
        GameLog.Info("[QuickTestGame] Quick Test Mode flags CLEARED - normal game mode restored");
    }
    
    private void OnDestroy()
    {
        // CRITICAL: Clear flags when this component is destroyed (returning to menu)
        EndSession();
        ClearQuickTestMode();
    }

    // ---- a quick test borrows the career's stats; this puts them back and keeps the career from being saved with them ----

    /// <summary>True from pressing Q until the quick test is over (the player leaves the game scene, or the app stops). SaveCareer does nothing meanwhile.</summary>
    public static bool SessionActive { get; private set; }

    bool sawGameScene;
    bool haveSnapshot;
    CareerStats snapStats, snapOpp, snapEquip;
    int[] snapTraining;

    static CareerStats Copy(CareerStats c)
    {
        if (c == null) return null;
        return new CareerStats
        {
            weightAccuracy = c.weightAccuracy, finesseAccuracy = c.finesseAccuracy, aimAccuracy = c.aimAccuracy,
            sweepStrength = c.sweepStrength, sweepEndurance = c.sweepEndurance, sweepCohesion = c.sweepCohesion,
        };
    }

    static void CopyInto(CareerStats to, CareerStats from)
    {
        if (to == null || from == null) return;
        to.weightAccuracy = from.weightAccuracy; to.finesseAccuracy = from.finesseAccuracy; to.aimAccuracy = from.aimAccuracy;
        to.sweepStrength = from.sweepStrength; to.sweepEndurance = from.sweepEndurance; to.sweepCohesion = from.sweepCohesion;
    }

    void TakeSnapshot(CareerManager cm)
    {
        if (haveSnapshot || cm == null) return;
        snapStats = Copy(cm.cStats);
        snapOpp = Copy(cm.oppStats);
        snapEquip = Copy(cm.equipmentBonus);
        snapTraining = cm.trainingLevels != null ? (int[])cm.trainingLevels.Clone() : null;
        haveSnapshot = true;
    }

    void EndSession()
    {
        if (haveSnapshot)
        {
            var cm = GetComponent<CareerManager>();
            if (cm != null)
            {
                CopyInto(cm.cStats, snapStats);
                CopyInto(cm.oppStats, snapOpp);
                cm.equipmentBonus = snapEquip != null ? snapEquip : new CareerStats();
                cm.trainingLevels = snapTraining;
                GameLog.Info("[QuickTestGame] Session over: the career's own stats and training are back");
            }
            haveSnapshot = false;
        }
        SessionActive = false;
        sawGameScene = false;
    }
    
    private void Update()
    {
        if (SessionActive)
        {
            bool inGame = SceneManager.GetActiveScene().name == "TournyGame";
            if (inGame) { sawGameScene = true; ApplyLiveSettings(); }
            else if (sawGameScene) { EndSession(); ClearQuickTestMode(); }   // back out to a menu: the test is over
        }

        // Check for quick test hotkey
        if (Input.GetKeyDown(testGameKey))
        {
            StartQuickTestGame();
        }
        
        // Check for takeout practice mode hotkey (only works in-game)
        if (Input.GetKeyDown(takeoutPracticeKey))
        {
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null && SceneManager.GetActiveScene().name == "TournyGame")
            {
                StartCoroutine(ActivateTakeoutPracticeMode(gm));
            }
            else
            {
                GameLog.Info("[QuickTestGame] ?? Takeout practice mode (W) only works during a game! Press Q to start a test game first.");
            }
        }
        
        // Check for runback practice mode hotkey (only works in-game)
        if (Input.GetKeyDown(runbackPracticeKey))
        {
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null && SceneManager.GetActiveScene().name == "TournyGame")
            {
                StartCoroutine(ActivateRunbackPracticeMode(gm));
            }
            else
            {
                GameLog.Info("[QuickTestGame] ?? Runback practice mode (E) only works during a game! Press Q to start a test game first.");
            }
        }
    }
    
    /// <summary>
    /// While a quick test is running, the skill and look sliders above take effect as soon as they are moved (every frame), so you can aim,
    /// let go without shooting, change a number, and aim again to compare. The aim fuzz re-rolls when the skill changes (see TrajectoryLine.BeginFuzz).
    /// Gear and training still add to the skill.
    /// </summary>
    void ApplyLiveSettings()
    {
        var cm = GetComponent<CareerManager>();
        if (cm != null && cm.cStats != null)
        {
            cm.cStats.aimAccuracy = playerAimSkill;
            cm.cStats.weightAccuracy = playerWeightSkill;
            cm.cStats.finesseAccuracy = playerConsistency;
        }
        var levels = new[] { sweepLevel, tacticsLevel, onIceLevel };
        if (cm != null && (cm.trainingLevels == null || !System.Linq.Enumerable.SequenceEqual(cm.trainingLevels, levels)))
        {
            cm.trainingLevels = levels;
            TrainingAids.Refresh();   // the aids read the levels once when a match starts; do it again now
        }
        PullbackSettings.ShowThumbCircle = thumbCircle;
        PullbackSettings.AimCap = aimCap;
        AimFuzzBands.ConeStrength = coneStrength;
        AimFuzzBands.OvalFill = ovalFill;
        AimFuzzBands.OvalOutline = ovalOutline;
        AimFuzzBands.CrossStrength = crossStrength;
    }

    /// <summary>
    /// Immediately starts a test game with preset settings
    /// </summary>
    public void StartQuickTestGame()
    {
        GameLog.Info("[QuickTestGame] Starting instant test game - Press Q to restart anytime!");
        SessionActive = true;   // from now on the career is not saved (see CareerManager.SaveCareer) and its stats are restored at the end
        
        // Find or create GameSettingsPersist
        GameSettingsPersist gsp = FindFirstObjectByType<GameSettingsPersist>();
        if (gsp == null)
        {
            // Create a new one if it doesn't exist
            GameObject gspObj = new GameObject("GameSettingsPersist");
            gsp = gspObj.AddComponent<GameSettingsPersist>();
            DontDestroyOnLoad(gspObj);
        }
        
        // Reset all game state
        gsp.gameInProgress = false;
        gsp.tournyInProgress = false;
        gsp.tourny = false;
        gsp.cashGame = false;
        gsp.loadGame = false;
        gsp.tutorial = false;
        gsp.debug = true; // Mark as debug/quick test game - CRITICAL for deterministic physics!
        
        // Set up test game parameters
        gsp.rocks = rocksPerTeam;
        gsp.ends = endsToPlay;
        gsp.rockCurrent = 0;
        gsp.endCurrent = 0; // Always start at end 1
        
        // Team setup
        gsp.redTeamName = "Test Team Red";
        gsp.yellowTeamName = "Test Team Yellow";
        
        // AI team setup - allow testing AI vs AI
        if (bothTeamsAI)
        {
            gsp.aiRed = true;
            gsp.aiYellow = true;
            GameLog.Info("[QuickTestGame] AI vs AI mode - both teams controlled by AI");
        }
        else
        {
            gsp.aiRed = false;  // Player is red
            gsp.aiYellow = true; // AI is yellow
            GameLog.Info("[QuickTestGame] Player vs AI mode");
        }
        
        if (randomizeScores)
        {
            gsp.redScore = Random.Range(0, 6);
            gsp.yellowScore = Random.Range(0, 6);
        }
        else
        {
            gsp.redScore = 2;
            gsp.yellowScore = 6;
        }
        if (randomizeHammer)
        {
            if (Random.value < 0.50f) // 50% chance red has hammer
            {
                gsp.redHammer = true;
            }
            else
            {
                gsp.redHammer = false;
            }
        }
        else
            gsp.redHammer = false;
        GameLog.Info($"[QuickTestGame] ?? LOCKED: Score {gsp.redScore}-{gsp.yellowScore}, {(gsp.redHammer ? "RED" : "YELLOW")} has hammer");
        
        // Scores
        gsp.score = new Vector2Int[endsToPlay];
        // Note: Scores are set above in randomization section
        
        // Create opponent team with 85% stats
        Team opponentTeam = new Team
        {
            name = "Max Stats AI",
            strength = opponentStatValue,
            weight = opponentStatValue,
            finesse = opponentStatValue,
            aim = opponentStatValue,
            sweepStrength = opponentStatValue,
            sweepEnduro = opponentStatValue,
            sweepCohesion = opponentStatValue,
            player = false,
            players = new List<Player>()
        };
        
        // Add 4 AI players to the team
        // TeamManager.SetCharacter() will read these values and populate CharacterStats
        for (int i = 0; i < 4; i++)
        {
            opponentTeam.players.Add(new Player
            {
                id = i,
                name = $"AI Player {i + 1}",
                weight = opponentStatValue,
                finesse = opponentStatValue,
                aim = opponentStatValue,
                sweepStrength = opponentStatValue,
                sweepEnduro = opponentStatValue,
                sweepCohesion = opponentStatValue
            });
        }
        
        // ? CRITICAL: Create player team with 85% stats for PERFECT DETERMINISTIC SHOTS!
        // NO randomness, NO skill penalties - pure physics for trajectory tuning
        Team playerTeam = new Team
        {
            name = "Test Player (85% Accuracy)",
            strength = 85,  // LOCKED: Perfect stats for trajectory tuning
            weight = 85,
            finesse = 85,
            aim = 85,
            sweepStrength = 85,
            sweepEnduro = 85,
            sweepCohesion = 85,
            player = true,
            players = new List<Player>()
        };
        
        // Add 4 players to the team - ALL with 85% stats
        for (int i = 0; i < 4; i++)
        {
            playerTeam.players.Add(new Player
            {
                id = i,
                name = $"Test Player {i + 1} (Perfect)",
                weight = 85,      // ? NO RANDOMNESS
                finesse = 85,
                aim = 85,
                sweepStrength = 85,
                sweepEnduro = 85,
                sweepCohesion = 85
            });
        }
        
        gsp.redTeam = playerTeam;
        gsp.yellowTeam = opponentTeam;
        
        // Setup CareerManager stats if it exists
        CareerManager cm = FindFirstObjectByType<CareerManager>();
        if (cm != null)
        {
            TakeSnapshot(cm);
            // ? CRITICAL: Set player stats to 85 for ZERO randomness!
            cm.cStats.weightAccuracy = playerWeightSkill;   // a middling shooter by default; drag the sliders down to see a wide fuzz
            cm.cStats.finesseAccuracy = playerConsistency;
            cm.cStats.aimAccuracy = playerAimSkill;
            cm.equipmentBonus = new CareerStats();   // no gear carried over from a career, so the skill is exactly what is set here
            cm.cStats.sweepStrength = 85;
            cm.cStats.sweepEndurance = 85;
            cm.cStats.sweepCohesion = 85;
            cm.trainingLevels = new[] { sweepLevel, tacticsLevel, onIceLevel };   // set with the sliders; all 4 by default so every aid can be tried
            
            // Max out opponent stats
            cm.oppStats.weightAccuracy = opponentStatValue;
            cm.oppStats.finesseAccuracy = opponentStatValue;
            cm.oppStats.aimAccuracy = opponentStatValue;
            cm.oppStats.sweepStrength = opponentStatValue;
            cm.oppStats.sweepEndurance = opponentStatValue;
            cm.oppStats.sweepCohesion = opponentStatValue;
        }
        
        // ? CRITICAL: Set flag for 50% deterministic player physics (no multipliers!)
        PlayerPrefs.SetInt("QuickTestMode", 1);

        // Aim aids for fuzz testing: the aim circle, guide lines and cone; no dots, hit arrows or curl line.
        // These are the same saved switches as the pause menu's toggles, so they stay until changed there.
        PlayerPrefs.SetInt("GuidelinesVisible", 1);
        PlayerPrefs.SetInt("AimCircleVisible", 1);
        PlayerPrefs.SetInt("TrajectoryVisible", 0);
        PlayerPrefs.SetInt("CollisionLinesVisible", 0);
        PlayerPrefs.SetInt("CurlLineVisible", 0);
        PlayerPrefs.SetInt("CollisionWarningVisible", 0);
        
        // ? CRITICAL: Disable sweeping in test mode for perfect determinism
        // Sweeping introduces timing-based variance (when/how long you sweep)
        // For trajectory tuning, we want PURE pullback ? distance relationship
        PlayerPrefs.SetInt("DisableSweeping", 1);
        
        PlayerPrefs.Save();
        
        // Load the game scene
        SceneManager.LoadScene("TournyGame");
        
        GameLog.Info($"[QuickTestGame] ? DETERMINISTIC MODE ENABLED!");
        GameLog.Info($"[QuickTestGame] Player stats: 85/100, except aim {playerAimSkill}, weight {playerWeightSkill} and consistency {playerConsistency} (+ training {cm?.TrainingBonus.aimAccuracy}/{cm?.TrainingBonus.weightAccuracy})"); //(NO randomness, NO skill penalties)");
        GameLog.Info($"[QuickTestGame] Physics multipliers: LOCKED to 1.0 (perfect)");
        GameLog.Info($"[QuickTestGame] ? SWEEPING DISABLED for perfect distance consistency");
        GameLog.Info($"[QuickTestGame] Test game started: {rocksPerTeam} rocks, {endsToPlay} ends, opponent stats: {opponentStatValue}");
    }
    
    /// <summary>
    /// Alternative method to start from inspector or button
    /// </summary>
    public void StartTestGameButton()
    {
        StartQuickTestGame();
    }
    
    /// <summary>
    /// TAKEOUT PRACTICE MODE: Sets up 4 red rocks in house, forces yellow AI to take them out
    /// Perfect for testing AI takeout accuracy over and over!
    /// Press W during a game to activate
    /// </summary>
    private IEnumerator ActivateTakeoutPracticeMode(GameManager gm)
    {
        GameLog.Info("?? [TAKEOUT PRACTICE MODE] Activated! Setting up 4 red rocks in house...");
        GameLog.Info("?? Yellow AI will now attempt takeouts on all red rocks!");
        GameLog.Info("?? Press W again to reset and try more takeouts");
        
        // 1. CLEAR THE HOUSE - Remove all rocks currently in play
        foreach (var rockEntry in gm.rockList)
        {
            if (rockEntry.rock != null && rockEntry.rock.activeInHierarchy)
            {
                // Move rock out of play
                rockEntry.rock.transform.position = new Vector3(0f, 20f, 0f);
                rockEntry.rockInfo.inPlay = false;
                rockEntry.rockInfo.inHouse = false;
                rockEntry.rockInfo.outOfPlay = true;
                rockEntry.rock.SetActive(false);
            }
        }
        
        yield return new WaitForFixedUpdate();
        
        // 2. PLACE 4 RED ROCKS IN STRATEGIC POSITIONS IN THE HOUSE
        // These positions will test different angles and distances for takeouts
        Vector2[] redRockPositions = new Vector2[]
        {
            new Vector2(0.0f, 6.5f),    // Button (center)
            new Vector2(-0.4f, 6.2f),   // Left of button
            new Vector2(0.5f, 6.8f),    // Right, slightly back
            new Vector2(-0.3f, 7.0f)    // Left, back
        };
        
        // Find first 4 red rocks (even indices if red has hammer, odd if yellow has hammer)
        int redRockIndex = gm.redHammer ? 0 : 1;
        int placedRocks = 0;
        
        for (int i = 0; i < gm.rockList.Count && placedRocks < 4; i++)
        {
            var rockEntry = gm.rockList[i];
            
            // Check if this is a red rock
            bool isRedRock = rockEntry.rockInfo.teamName == gm.redTeamName;
            
            if (isRedRock && placedRocks < 4)
            {
                // Activate and position the rock
                rockEntry.rock.SetActive(true);
                rockEntry.rock.transform.position = redRockPositions[placedRocks];
                
                // Enable physics components
                rockEntry.rock.GetComponent<CircleCollider2D>().enabled = true;
                rockEntry.rock.GetComponent<CircleCollider2D>().radius = 0.14f;
                rockEntry.rock.GetComponent<SpriteRenderer>().enabled = true;
                rockEntry.rock.GetComponent<Rock_Release>().enabled = true;
                rockEntry.rock.GetComponent<Rock_Force>().enabled = true;
                rockEntry.rock.GetComponent<Rock_Colliders>().enabled = true;
                
                // Set rock state
                rockEntry.rockInfo.inPlay = true;
                rockEntry.rockInfo.inHouse = true;
                rockEntry.rockInfo.outOfPlay = false;
                rockEntry.rockInfo.placed = true;
                rockEntry.rockInfo.shotTaken = true;
                rockEntry.rockInfo.released = true;
                rockEntry.rockInfo.rest = true;
                rockEntry.rockInfo.stopped = true;
                rockEntry.rockInfo.moving = false;
                
                // Reset velocity
                Rigidbody2D rb = rockEntry.rock.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
                
                GameLog.Info($"?? Placed {rockEntry.rockInfo.teamName} Rock #{rockEntry.rockInfo.rockNumber} at {redRockPositions[placedRocks]}");
                placedRocks++;
            }
        }
        
        yield return new WaitForFixedUpdate();
        
        // 3. FORCE YELLOW AI TO SHOOT NEXT (takeout mode)
        // Find the first yellow rock that's not yet shot
        int yellowRockToShoot = -1;
        for (int i = 0; i < gm.rockList.Count; i++)
        {
            var rockEntry = gm.rockList[i];
            bool isYellowRock = rockEntry.rockInfo.teamName == gm.yellowTeamName;
            
            if (isYellowRock && !rockEntry.rockInfo.shotTaken)
            {
                yellowRockToShoot = i;
                break;
            }
        }
        
        if (yellowRockToShoot >= 0)
        {
            // Update game state
            gm.rockCurrent = yellowRockToShoot;
            gm.houseList.Clear();
            gm.gList.Clear();
            
            // Rebuild house list with our 4 red rocks
            foreach (var rockEntry in gm.rockList)
            {
                if (rockEntry.rockInfo.inHouse && rockEntry.rockInfo.inPlay)
                {
                    gm.houseList.Add(new House_List(rockEntry.rock, rockEntry.rockInfo));
                }
            }
            
            GameLog.Info($"?? House list rebuilt with {gm.houseList.Count} red rocks");
            GameLog.Info($"?? Forcing Yellow AI to shoot rock #{yellowRockToShoot}...");
            
            // Force yellow turn
            gm.aiTeamYellow = true; // Ensure yellow is AI controlled
            gm.OnYellowTurn();
            
            GameLog.Info("?? [TAKEOUT PRACTICE] Yellow AI will now attempt takeout!");
            GameLog.Info("?? Watch the diagnostics to see hit quality and accuracy");
            GameLog.Info("?? Press W again after shot completes to practice more takeouts");
        }
        else
        {
            Debug.LogWarning("?? [TAKEOUT PRACTICE] No yellow rocks available to shoot! Start a new game (Q) and try again.");
        }
    }
    
    /// <summary>
    /// RUNBACK PRACTICE MODE: Sets up a finesse rock protecting a target, forces yellow AI to run it back
    /// Perfect for testing the new runback shot system!
    /// Press E during a game to activate
    /// </summary>
    private IEnumerator ActivateRunbackPracticeMode(GameManager gm)
    {
        GameLog.Info("?? [RUNBACK PRACTICE MODE] Activated! Setting up finesse-protected target...");
        GameLog.Info("?? Yellow AI will attempt to HIT THE GUARD THROUGH to remove the target!");
        GameLog.Info("?? Press E again to reset and try different alignments");
        
        // 1. CLEAR THE HOUSE - Remove all rocks currently in play
        foreach (var rockEntry in gm.rockList)
        {
            if (rockEntry.rock != null && rockEntry.rock.activeInHierarchy)
            {
                // Move rock out of play
                rockEntry.rock.transform.position = new Vector3(0f, 20f, 0f);
                rockEntry.rockInfo.inPlay = false;
                rockEntry.rockInfo.inHouse = false;
                rockEntry.rockInfo.outOfPlay = true;
                rockEntry.rock.SetActive(false);
            }
        }
        
        yield return new WaitForFixedUpdate();
        
        // 2. PLACE RED TARGET ROCK (the one we want to remove)
        Vector2 targetPosition = new Vector2(0.1f, 6.5f);  // Slightly off-center button
        
        // Find first red rock
        int targetRockIndex = -1;
        for (int i = 0; i < gm.rockList.Count; i++)
        {
            var rockEntry = gm.rockList[i];
            bool isRedRock = rockEntry.rockInfo.teamName == gm.redTeamName;
            
            if (isRedRock)
            {
                targetRockIndex = i;
                break;
            }
        }
        
        if (targetRockIndex >= 0)
        {
            var targetRock = gm.rockList[targetRockIndex];
            
            // Activate and position the target rock
            targetRock.rock.SetActive(true);
            targetRock.rock.transform.position = targetPosition;
            
            // Enable physics components
            targetRock.rock.GetComponent<CircleCollider2D>().enabled = true;
            targetRock.rock.GetComponent<CircleCollider2D>().radius = 0.14f;
            targetRock.rock.GetComponent<SpriteRenderer>().enabled = true;
            
            // Set rock state
            targetRock.rockInfo.inPlay = true;
            targetRock.rockInfo.inHouse = true;
            targetRock.rockInfo.outOfPlay = false;
            targetRock.rockInfo.placed = true;
            targetRock.rockInfo.shotTaken = true;
            targetRock.rockInfo.released = true;
            targetRock.rockInfo.rest = true;
            targetRock.rockInfo.stopped = true;
            targetRock.rockInfo.moving = false;
            
            // Reset velocity
            Rigidbody2D rb = targetRock.rock.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            
            GameLog.Info($"?? Placed RED TARGET at {targetPosition} (the rock we want to remove)");
        }
        
        yield return new WaitForFixedUpdate();
        
        // 3. PLACE RED GUARD ROCK (protecting the target - AI must hit THIS rock)
        // Position finesse between launcher and target for good alignment
        Vector2 guardPosition = new Vector2(0.05f, 3.5f);  // Well-aligned with target
        
        // Find second red rock for finesse
        int guardRockIndex = -1;
        int redCount = 0;
        for (int i = 0; i < gm.rockList.Count; i++)
        {
            var rockEntry = gm.rockList[i];
            bool isRedRock = rockEntry.rockInfo.teamName == gm.redTeamName;
            
            if (isRedRock)
            {
                redCount++;
                if (redCount == 2)
                {
                    guardRockIndex = i;
                    break;
                }
            }
        }
        
        if (guardRockIndex >= 0)
        {
            var guardRock = gm.rockList[guardRockIndex];
            
            // Activate and position the finesse rock
            guardRock.rock.SetActive(true);
            guardRock.rock.transform.position = guardPosition;
            
            // Enable physics components
            guardRock.rock.GetComponent<CircleCollider2D>().enabled = true;
            guardRock.rock.GetComponent<CircleCollider2D>().radius = 0.14f;
            guardRock.rock.GetComponent<SpriteRenderer>().enabled = true;
            
            // Set rock state
            guardRock.rockInfo.inPlay = true;
            guardRock.rockInfo.inHouse = false;  // Guard is outside house
            guardRock.rockInfo.outOfPlay = false;
            guardRock.rockInfo.placed = true;
            guardRock.rockInfo.shotTaken = true;
            guardRock.rockInfo.released = true;
            guardRock.rockInfo.rest = true;
            guardRock.rockInfo.stopped = true;
            guardRock.rockInfo.moving = false;
            
            // Reset velocity
            Rigidbody2D rb = guardRock.rock.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            
            GameLog.Info($"?? Placed RED GUARD at {guardPosition} (AI must hit THIS rock to run back)");
        }
        
        yield return new WaitForFixedUpdate();
        
        // 4. FORCE YELLOW AI TO SHOOT (runback mode)
        // Find the first yellow rock that's not yet shot
        int yellowRockToShoot = -1;
        for (int i = 0; i < gm.rockList.Count; i++)
        {
            var rockEntry = gm.rockList[i];
            bool isYellowRock = rockEntry.rockInfo.teamName == gm.yellowTeamName;
            
            if (isYellowRock && !rockEntry.rockInfo.shotTaken)
            {
                yellowRockToShoot = i;
                break;
            }
        }
        
        if (yellowRockToShoot >= 0)
        {
            // Update game state
            gm.rockCurrent = yellowRockToShoot;
            gm.houseList.Clear();
            gm.gList.Clear();
            
            // Rebuild house list with target rock
            foreach (var rockEntry in gm.rockList)
            {
                if (rockEntry.rockInfo.inHouse && rockEntry.rockInfo.inPlay)
                {
                    gm.houseList.Add(new House_List(rockEntry.rock, rockEntry.rockInfo));
                }
            }
            
            // Rebuild finesse list with finesse rock
            foreach (var rockEntry in gm.rockList)
            {
                if (rockEntry.rockInfo.inPlay && !rockEntry.rockInfo.inHouse && rockEntry.rockInfo.placed)
                {
                    // Guard_List constructor expects: (int rockIndex, bool freeGuard, Transform transform)
                    gm.gList.Add(new Guard_List(rockEntry.rockInfo.rockIndex, false, rockEntry.rock.transform));
                }
            }
            
            GameLog.Info($"?? House list: {gm.houseList.Count} rocks (target)");
            GameLog.Info($"?? Guard list: {gm.gList.Count} rocks (obstruction)");
            GameLog.Info($"?? Forcing Yellow AI to shoot rock #{yellowRockToShoot}...");
            
            // Force yellow turn
            gm.aiTeamYellow = true; // Ensure yellow is AI controlled
            gm.OnYellowTurn();
            
            GameLog.Info("?? [RUNBACK PRACTICE] Yellow AI will now evaluate runback shot!");
            GameLog.Info("?? Watch for: 'Option 5: Runback' in console logs");
            GameLog.Info("?? AI should hit the GUARD (red rock at y=3.5) with extra velocity");
            GameLog.Info("?? to drive through and remove the TARGET (red rock at button)");
            GameLog.Info("?? Press E again after shot completes to practice more!");
        }
        else
        {
            Debug.LogWarning("?? [RUNBACK PRACTICE] No yellow rocks available to shoot! Start a new game (Q) and try again.");
        }
    }
}


