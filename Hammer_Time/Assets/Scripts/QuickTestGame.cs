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
        Debug.Log("[QuickTestGame] Quick Test Mode flags CLEARED - normal game mode restored");
    }
    
    private void OnDestroy()
    {
        // CRITICAL: Clear flags when this component is destroyed (returning to menu)
        ClearQuickTestMode();
    }
    
    private void Update()
    {
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
                Debug.Log("[QuickTestGame] ?? Takeout practice mode (W) only works during a game! Press Q to start a test game first.");
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
                Debug.Log("[QuickTestGame] ?? Runback practice mode (E) only works during a game! Press Q to start a test game first.");
            }
        }
    }
    
    /// <summary>
    /// Immediately starts a test game with preset settings
    /// </summary>
    public void StartQuickTestGame()
    {
        Debug.Log("[QuickTestGame] Starting instant test game - Press Q to restart anytime!");
        
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
            Debug.Log("[QuickTestGame] AI vs AI mode - both teams controlled by AI");
        }
        else
        {
            gsp.aiRed = false;  // Player is red
            gsp.aiYellow = true; // AI is yellow
            Debug.Log("[QuickTestGame] Player vs AI mode");
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
        Debug.Log($"[QuickTestGame] ?? LOCKED: Score 6-2, YELLOW has hammer");
        
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
            // ? CRITICAL: Set player stats to 85 for ZERO randomness!
            cm.cStats.weightAccuracy = 85;
            cm.cStats.finesseAccuracy = 85;
            cm.cStats.aimAccuracy = 85;
            cm.cStats.sweepStrength = 85;
            cm.cStats.sweepEndurance = 85;
            cm.cStats.sweepCohesion = 85;
            
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
        
        // ? CRITICAL: Disable sweeping in test mode for perfect determinism
        // Sweeping introduces timing-based variance (when/how long you sweep)
        // For trajectory tuning, we want PURE pullback ? distance relationship
        PlayerPrefs.SetInt("DisableSweeping", 1);
        
        PlayerPrefs.Save();
        
        // Load the game scene
        SceneManager.LoadScene("TournyGame");
        
        Debug.Log($"[QuickTestGame] ? DETERMINISTIC MODE ENABLED!");
        Debug.Log($"[QuickTestGame] Player stats: 85/85"); //(NO randomness, NO skill penalties)");
        Debug.Log($"[QuickTestGame] Physics multipliers: LOCKED to 1.0 (perfect)");
        Debug.Log($"[QuickTestGame] ? SWEEPING DISABLED for perfect distance consistency");
        Debug.Log($"[QuickTestGame] Test game started: {rocksPerTeam} rocks, {endsToPlay} ends, opponent stats: {opponentStatValue}");
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
        Debug.Log("?? [TAKEOUT PRACTICE MODE] Activated! Setting up 4 red rocks in house...");
        Debug.Log("?? Yellow AI will now attempt takeouts on all red rocks!");
        Debug.Log("?? Press W again to reset and try more takeouts");
        
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
                
                Debug.Log($"?? Placed {rockEntry.rockInfo.teamName} Rock #{rockEntry.rockInfo.rockNumber} at {redRockPositions[placedRocks]}");
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
            
            Debug.Log($"?? House list rebuilt with {gm.houseList.Count} red rocks");
            Debug.Log($"?? Forcing Yellow AI to shoot rock #{yellowRockToShoot}...");
            
            // Force yellow turn
            gm.aiTeamYellow = true; // Ensure yellow is AI controlled
            gm.OnYellowTurn();
            
            Debug.Log("?? [TAKEOUT PRACTICE] Yellow AI will now attempt takeout!");
            Debug.Log("?? Watch the diagnostics to see hit quality and accuracy");
            Debug.Log("?? Press W again after shot completes to practice more takeouts");
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
        Debug.Log("?? [RUNBACK PRACTICE MODE] Activated! Setting up finesse-protected target...");
        Debug.Log("?? Yellow AI will attempt to HIT THE GUARD THROUGH to remove the target!");
        Debug.Log("?? Press E again to reset and try different alignments");
        
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
            
            Debug.Log($"?? Placed RED TARGET at {targetPosition} (the rock we want to remove)");
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
            
            Debug.Log($"?? Placed RED GUARD at {guardPosition} (AI must hit THIS rock to run back)");
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
            
            Debug.Log($"?? House list: {gm.houseList.Count} rocks (target)");
            Debug.Log($"?? Guard list: {gm.gList.Count} rocks (obstruction)");
            Debug.Log($"?? Forcing Yellow AI to shoot rock #{yellowRockToShoot}...");
            
            // Force yellow turn
            gm.aiTeamYellow = true; // Ensure yellow is AI controlled
            gm.OnYellowTurn();
            
            Debug.Log("?? [RUNBACK PRACTICE] Yellow AI will now evaluate runback shot!");
            Debug.Log("?? Watch for: 'Option 5: Runback' in console logs");
            Debug.Log("?? AI should hit the GUARD (red rock at y=3.5) with extra velocity");
            Debug.Log("?? to drive through and remove the TARGET (red rock at button)");
            Debug.Log("?? Press E again after shot completes to practice more!");
        }
        else
        {
            Debug.LogWarning("?? [RUNBACK PRACTICE] No yellow rocks available to shoot! Start a new game (Q) and try again.");
        }
    }
}


