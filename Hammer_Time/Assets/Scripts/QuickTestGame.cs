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
    
    [Tooltip("Number of rocks per team")]
    public int rocksPerTeam = 4;
    
    [Tooltip("Number of ends to play")]
    public int endsToPlay = 1;
    
    [Tooltip("Opponent stats (0-100, 100 = perfect shots)")]
    [Range(0, 100)]
    public int opponentStatValue = 100;
    
    [Tooltip("Set BOTH teams to AI for testing? (Press W during game for AI vs AI)")]
    public bool bothTeamsAI = false;
    
    [Tooltip("Randomize who gets hammer? (50/50 chance each time)")]
    public bool randomizeHammer = true; // LOCKED: Red always has hammer
    
    [Tooltip("Randomize starting scores? Creates different strategic scenarios")]
    public bool randomizeScores = true; // LOCKED: Always 1-1
    
    [Tooltip("If randomizeScores is true, use weighted scenarios (more tied/close games)?")]
    public bool useWeightedScenarios = true;
    
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
        gsp.debug = true; // Mark as debug/quick test game
        
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
        
        // 🔒 LOCKED SCENARIO: Always 1-5, red has hammer
        gsp.redHammer = true;
        gsp.redScore = 1;
        gsp.yellowScore = 5;
        Debug.Log($"[QuickTestGame] 🔒 LOCKED: Score 1-5, RED has hammer");
        
        // Scores
        gsp.score = new Vector2Int[endsToPlay];
        // Note: Scores are set above in randomization section
        
        // Create opponent team with max stats
        Team opponentTeam = new Team
        {
            name = "Max Stats AI",
            strength = opponentStatValue,
            draw = opponentStatValue,
            guard = opponentStatValue,
            takeOut = opponentStatValue,
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
                draw = opponentStatValue,
                guard = opponentStatValue,
                takeOut = opponentStatValue,
                sweepStrength = opponentStatValue,
                sweepEnduro = opponentStatValue,
                sweepCohesion = opponentStatValue
            });
        }
        
        // Create player team with default stats
        Team playerTeam = new Team
        {
            name = "Test Player",
            strength = 50,
            draw = 50,
            guard = 50,
            takeOut = 50,
            sweepStrength = 50,
            sweepEnduro = 50,
            sweepCohesion = 50,
            player = true,
            players = new List<Player>()
        };
        
        // Add 4 players to the team
        // TeamManager.SetCharacter() will read these values
        for (int i = 0; i < 4; i++)
        {
            playerTeam.players.Add(new Player
            {
                id = i,
                name = $"Test Player {i + 1}",
                draw = 50,
                guard = 50,
                takeOut = 50,
                sweepStrength = 50,
                sweepEnduro = 50,
                sweepCohesion = 50
            });
        }
        
        gsp.redTeam = playerTeam;
        gsp.yellowTeam = opponentTeam;
        
        // Setup CareerManager stats if it exists
        CareerManager cm = FindFirstObjectByType<CareerManager>();
        if (cm != null)
        {
            // Set player stats to reasonable defaults for testing
            cm.cStats.drawAccuracy = 50;
            cm.cStats.guardAccuracy = 50;
            cm.cStats.takeOutAccuracy = 50;
            cm.cStats.sweepStrength = 50;
            cm.cStats.sweepEndurance = 50;
            cm.cStats.sweepCohesion = 50;
            
            // Max out opponent stats
            cm.oppStats.drawAccuracy = opponentStatValue;
            cm.oppStats.guardAccuracy = opponentStatValue;
            cm.oppStats.takeOutAccuracy = opponentStatValue;
            cm.oppStats.sweepStrength = opponentStatValue;
            cm.oppStats.sweepEndurance = opponentStatValue;
            cm.oppStats.sweepCohesion = opponentStatValue;
        }
        
        // Load the game scene
        SceneManager.LoadScene("TournyGame");
        
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
}

