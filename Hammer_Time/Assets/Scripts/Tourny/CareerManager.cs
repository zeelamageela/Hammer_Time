using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TigerForge;
using System.Net.Security;
using System;
using System.Linq;

/// <summary>
/// CareerManager - Main controller for career mode progression and save/load
/// 
/// SAVE SYSTEM:
/// - Uses JSON-based save system (CareerSaveService)
/// - Auto-save enabled by default (30-second intervals)
/// - Supports mid-game and mid-tournament saves
/// - Separate high score system (HighScoreService) for all-time leaderboards
/// 
/// Save Location: Application.persistentDataPath/career_save.json
/// High Scores: Application.persistentDataPath/high_scores.json
/// </summary>
public class CareerManager : MonoBehaviour
{
    // XP and Reward Constants
    private const float XP_TOUR_WIN = 100f;
    private const float XP_TOUR_TOP4 = 60f;
    private const float XP_TOUR_PARTICIPATION = 30f;
    private const float XP_CHAMPIONSHIP_WIN = 200f;
    private const float XP_CHAMPIONSHIP_TOP4 = 120f;
    private const float XP_CHAMPIONSHIP_PARTICIPATION = 60f;
    private const float XP_QUALIFIER_SUCCESS = 50f;
    private const float XP_QUALIFIER_PARTICIPATION = 20f;
    private const float XP_LOCAL_WIN = 40f;
    private const float XP_LOCAL_TOP4 = 20f;
    private const float XP_LOCAL_PARTICIPATION = 10f;
    private const float XP_TOP5_BONUS = 5f;
    private const float XP_QUALIFIER_BONUS = 25f;
    private const float XP_PER_WIN = 3f;
    private const float XP_PER_LOSS = 1f;
    
    // Team Generation Constants
    // DIFFICULTY TUNING: Starting opponents are harder (170 base instead of 150)
    // but progression is slower (20 points/week instead of 30)
    // This creates a challenging early game with steady difficulty ramp
    private const int BASE_STAT_TOTAL = 170;  // Harder starting opponents
    private const int MAX_STAT_TOTAL = 250;   // Slightly higher ceiling
    private const int STAT_VARIATION = 5;
    private const int PLAYERS_PER_TEAM = 4;
    private const int NUM_STATS = 6;
    
    // Starting Values
    private const float STARTING_CASH = 1000f;
    private const int STARTING_STAT_VALUE = 45;
    
    // Misc Constants
    private const int TOP_RANK_THRESHOLD = 5;
    private const int HIGH_SCORE_MAX_ENTRIES = 100;
    
    // Auto-save Configuration
    private const float AUTO_SAVE_INTERVAL = 30f; // Auto-save every 30 seconds
    private float autoSaveTimer = 0f;
    private bool enableAutoSave = true;
    
    public static CareerManager instance;
    TournySettings ts;
    CareerSettings cs;

    public string debug = "blank";
    public int week;
    public int seasonLength;
    public string playerName;
    public string teamName;
    public Color teamColour;
    public int playerTeamIndex;
    public float earnings;
    public float cash;
    public float cashDelta;
    public Vector2 record;
    public bool provQual;
    public bool tourQual;
    public Vector2 tourRecord;

    public int storyBlock;

    public Player[] activePlayers;

    public Player playerCharacter;

    public float xp;
    public int level;

    public CareerStats cStats;
    public CareerStats oppStats;
    public CareerStats modStats;

    public int skillPoints;

    public int[] cardPUIDList;
    public int[] cardSponsorIDList;
    public int[] activeCardIDList;
    public int[] playedCardIDList;
    public int[] activeCardLengthList;

    public int[] activeEquipID;
    public int[] inventoryID;

    public bool inProgress;
    public int season;
    public bool loadedFromSave;
    public List<int> tournyResults;

    [SerializeField]
    public Player[] playerPool; // Editable in the Inspector
    [SerializeField]
    public Team[] teamPool;

    public int totalTeams;
    public int totalTourTeams;
    public int provTeams;

    public Vector4[] teamRecords;
    public Vector4[] tourRecords;
    public Team playerTeam;
    public Team[] teams;
    public Team[] tourTeams;
    public Team[] currentTournyTeams;
    public Tourny currentTourny;

    public CashGamePlayers[] cgp;

    public List<Standings_List> provRankList;
    public List<TourStandings_List> tourRankList;

    public Tourny[] tournies;
    public Tourny[] tour;
    public Tourny[] prov;
    public Tourny[] champ;
    public Tourny[] activeTournies;

    public bool[] coachDialogue;

    public bool[] qualDialogue;
    public bool[] reviewDialogue;
    public bool[] introDialogue;
    public bool[] helpDialogue;
    public bool[] strategyDialogue;
    public bool[] storyDialogue;

    public float costPerWeek;
    public bool teamPaid;
    List<Standings_List> allTimeList;
    public List<bool> allTimeTrophyList;
    public List<bool> currentTrophyList;

    public bool gameOver = false;
    
    // CRITICAL: Preserve completion IDs between load and TournySelector creation
    // These are populated on load and consumed by TournySelector.SetUp()
    private List<int> pendingCompletedTournamentIDs = new List<int>();
    private List<int> pendingTrophyWonIDs = new List<int>();
    private bool pendingTourChampionshipComplete = false;
    private bool pendingProvChampionshipComplete = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Add debug quick test game component (press Q anywhere to test)
        if (GetComponent<QuickTestGame>() == null)
        {
            gameObject.AddComponent<QuickTestGame>();
        }
    }

    private void Start()
    {
        tourRankList = new List<TourStandings_List>();
        provRankList = new List<Standings_List>();
        tournyResults = new List<int>();
        
        // CRITICAL FIX: Don't auto-load career here
        // Let TournySelector.SetUp() handle loading when it's ready
        // This prevents trying to load before save is deleted for new careers
        Debug.Log("[CareerManager] Start() - initialization complete, waiting for TournySelector");
    }

    private void Update()
    {
        // Auto-save timer
        if (enableAutoSave)
        {
            autoSaveTimer += Time.deltaTime;
            
            if (autoSaveTimer >= AUTO_SAVE_INTERVAL)
            {
                autoSaveTimer = 0f;
                AutoSave();
            }
        }
    }
    
    /// <summary>
    /// iOS lifecycle handler - called when app is backgrounded or resumed
    /// </summary>
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // App going to background on iOS - save immediately
            Debug.Log("[CareerManager] App pausing - forcing save");
            SaveCareer();
            
            // Disable auto-save while backgrounded to save battery
            SetAutoSave(false);
        }
        else
        {
            // App resumed - re-enable auto-save
            Debug.Log("[CareerManager] App resumed - enabling auto-save");
            SetAutoSave(true);
            autoSaveTimer = 0f; // Reset timer
        }
    }
    
    /// <summary>
    /// iOS lifecycle handler - called when app is closing
    /// </summary>
    private void OnApplicationQuit()
    {
        // Final save before app closes
        Debug.Log("[CareerManager] App quitting - final save");
        SaveCareer();
    }
    private void UpdateCharacter()
    {
        if (playerCharacter == null)
            return;

        playerCharacter.name = playerName;
        // id is not stored in CareerManager, but you can add it if needed
        // description is not stored in CareerManager, but you can add it if needed
        // cost, image, active, view are not typically stored in CareerManager

        // Main stats
        playerCharacter.draw = cStats.drawAccuracy;
        playerCharacter.guard = cStats.guardAccuracy;
        playerCharacter.takeOut = cStats.takeOutAccuracy;
        playerCharacter.sweepStrength = cStats.sweepStrength;
        playerCharacter.sweepEnduro = cStats.sweepEndurance;
        playerCharacter.sweepCohesion = cStats.sweepCohesion;

        // Opponent stats
        playerCharacter.oppDraw = oppStats.drawAccuracy;
        playerCharacter.oppGuard = oppStats.guardAccuracy;
        playerCharacter.oppTakeOut = oppStats.takeOutAccuracy;
        playerCharacter.oppStrength = oppStats.sweepStrength;
        playerCharacter.oppEnduro = oppStats.sweepEndurance;
        playerCharacter.oppCohesion = oppStats.sweepCohesion;
    }

    public void LoadSettings()
    {
        cs = FindFirstObjectByType<CareerSettings>();

        playerName = cs.playerName;
        teamName = cs.teamName;
        teamColour = cs.teamColour;
        season = cs.season;
        week = cs.week;
        seasonLength = 36;
        totalTeams = 32;
        tourRecord = cs.tourRecord;
        record = cs.record;
        totalTourTeams = 16;
        provTeams = 16;
        
        // CRITICAL FIX: Don't save here! This was overwriting the save file with empty tournament data
        // LoadSettings() is called from CareerSettings.LoadToCM() BEFORE entering the arena
        // At this point, TournySelector doesn't exist yet, so ToSaveData() gets zero tournaments
        // The save will happen naturally when entering the arena scene
        Debug.Log("[CareerManager] LoadSettings complete - NOT saving (to preserve tournament data)");
    }

    public void LoadCareer(
    GameSettingsPersist gsp = null,
    TournySelector tSel = null,
    StorylineManager slm = null,
    TeamMenu teamSel = null,
    SponsorManager pUpM = null,
    GameManager gm = null,
    EquipmentManager em = null)
    {
        // Auto-find if not provided
        if (gsp == null) gsp = FindFirstObjectByType<GameSettingsPersist>();
        if (tSel == null) tSel = FindFirstObjectByType<TournySelector>();
        if (slm == null) slm = FindFirstObjectByType<StorylineManager>();
        if (teamSel == null) teamSel = FindFirstObjectByType<TeamMenu>();
        if (pUpM == null) pUpM = FindFirstObjectByType<SponsorManager>();
        if (gm == null) gm = FindFirstObjectByType<GameManager>();
        if (em == null) em = FindFirstObjectByType<EquipmentManager>();

        Debug.Log("Loading in CM 2");
        Debug.Log(Application.persistentDataPath);

        ClearRankLists();
        
        // Use JSON save system
        LoadCareerJSON(gsp, tSel, slm, teamSel, pUpM, em);
    }
    
    /// <summary>
    /// New JSON-based save loading
    /// </summary>
    private void LoadCareerJSON(
        GameSettingsPersist gsp,
        TournySelector tSel,
        StorylineManager slm,
        TeamMenu teamSel,
        SponsorManager pUpM,
        EquipmentManager em)
    {
        try
        {
            // Load from JSON
            CareerSaveData saveData = CareerSaveService.LoadCareer();
            
            if (saveData == null)
            {
                Debug.LogWarning("[CareerManager] No save data found - starting fresh career");
                return;
            }
            
            // Validate save data version
            if (saveData.version != 1)
            {
                Debug.LogError($"[CareerManager] Unsupported save version {saveData.version}. Expected version 1.");
                Debug.LogWarning("[CareerManager] Save file may be corrupted or from incompatible version");
                return;
            }
            
            Debug.Log($"[CareerManager] Loading career save from {saveData.saveDate}");
            
        // CRITICAL FIX: Apply tournament data to TournySelector FIRST
        // This ensures tournament completion status is restored before SetActiveTournies() is called
        if (tSel != null)
        {
            Debug.Log($"[CareerManager] Applying tournament completion data to TournySelector BEFORE any other loading");
            ApplyTournamentData(tSel, saveData);
            Debug.Log($"[CareerManager] Tournament completion data applied successfully");
        }
        else
        {
            Debug.LogWarning($"[CareerManager] TournySelector is NULL - storing completion IDs for later sync");
            // CRITICAL: Store completion IDs temporarily until TournySelector exists
            // Don't try to restore to CM arrays - they're stale/reset!
            StorePendingCompletionData(saveData);
        }
            
            // Apply save data to CareerManager
            LoadFromSaveData(saveData);
            
            // Apply dialogue data to StorylineManager
            if (slm != null && saveData.dialogueFlags != null)
            {
                slm.blockIndex = saveData.dialogueFlags.storyBlockIndex;
            }
            
            // Equipment data is already restored from save file directly into EquipmentManager
            // Now sync the equipment UI and stats
            if (em != null)
            {
                Debug.Log("[CareerManager] Refreshing equipment manager after load");
                em.SetInventory(); // This will rebuild inventory and apply equipment stats
            }
            
            // Refresh SponsorManager with loaded card/sponsor data
            if (pUpM != null)
            {
                Debug.Log("[CareerManager] Refreshing sponsor manager after load");
                pUpM.SetUp(); // This will rebuild active cards and available sponsors from loaded IDs
            }
            
            // Refresh TeamMenu with loaded team data
            if (teamSel != null && activePlayers != null && activePlayers.Length > 0)
            {
                Debug.Log("[CareerManager] Refreshing team menu after load");
                teamSel.activePlayers = activePlayers; // Sync the active players
                // TeamMenu will refresh itself when the menu is opened via TeamMenuOpen()
            }
            
            // Restore tournament state if tournament was in progress
            if (saveData.currentTournamentState != null && saveData.currentGameState != null && saveData.currentGameState.tournyInProgress)
            {
                RestoreTournamentState(saveData.currentTournamentState, gsp);
            }
            
            // CRITICAL FIX: Always restore game state flags, even if game is not in progress
            // This ensures tournyInProgress and justFinishedGame are restored correctly
            if (saveData.currentGameState != null && gsp != null)
            {
                // Always restore these flags
                gsp.tournyInProgress = saveData.currentGameState.tournyInProgress;
                gsp.justFinishedGame = saveData.currentGameState.justFinishedGame;
                
                Debug.Log($"[CareerManager] Restored flags from save - tournyInProgress: {gsp.tournyInProgress}, justFinishedGame: {gsp.justFinishedGame}");
                Debug.Log($"[CareerManager] Save data had - tournyInProgress: {saveData.currentGameState.tournyInProgress}, justFinishedGame: {saveData.currentGameState.justFinishedGame}");
                
                // Only restore full game state if game was in progress
                if (saveData.currentGameState.gameInProgress)
                {
                    RestoreGameState(saveData.currentGameState, gsp);
                }
            }
            else
            {
                Debug.LogWarning($"[CareerManager] Could not restore flags - currentGameState is null: {saveData.currentGameState == null}, gsp is null: {gsp == null}");
            }
            
            Debug.Log("[CareerManager] Career loaded successfully from JSON");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CareerManager] Failed to load career from JSON: {ex.Message}");
            Debug.LogError($"[CareerManager] Stack trace: {ex.StackTrace}");
            Debug.LogWarning("[CareerManager] Career load failed - starting fresh or check save file integrity");
        }
    }
    
    /// <summary>
    /// Applies tournament completion data using simple ID lists
    /// Source of truth: TournySelector arrays (fresh ScriptableObjects each time)
    /// </summary>
    private void ApplyTournamentData(TournySelector tSel, CareerSaveData saveData)
    {
        if (tSel == null)
        {
            Debug.LogWarning("[CareerManager] TournySelector is null in ApplyTournamentData");
            return;
        }
        
        Debug.Log($"[CareerManager] ApplyTournamentData - Restoring from ID lists (completed: {saveData.completedTournamentIDs?.Count ?? 0}, trophies: {saveData.trophyWonIDs?.Count ?? 0})");
        
        // Restore completed tournaments
        if (saveData.completedTournamentIDs != null && saveData.completedTournamentIDs.Count > 0)
        {
            // Check tour tournaments
            if (tSel.tour != null)
            {
                foreach (var tourny in tSel.tour)
                {
                    if (tourny != null && saveData.completedTournamentIDs.Contains(tourny.id))
                    {
                        tourny.complete = true;
                        Debug.Log($"  ? Marked tour '{tourny.name}' (ID {tourny.id}) as complete");
                    }
                }
            }
            
            // Check provincial tournaments
            if (tSel.provQual != null)
            {
                foreach (var tourny in tSel.provQual)
                {
                    if (tourny != null && saveData.completedTournamentIDs.Contains(tourny.id))
                    {
                        tourny.complete = true;
                        Debug.Log($"  ? Marked qualifier '{tourny.name}' (ID {tourny.id}) as complete");
                    }
                }
            }
            
            // Check regular tournaments
            if (tSel.tournies != null)
            {
                foreach (var tourny in tSel.tournies)
                {
                    if (tourny != null && saveData.completedTournamentIDs.Contains(tourny.id))
                    {
                        tourny.complete = true;
                        Debug.Log($"  ? Marked tournament '{tourny.name}' (ID {tourny.id}) as complete");
                    }
                }
            }
        }
        
        // Restore trophy wins
        if (saveData.trophyWonIDs != null && saveData.trophyWonIDs.Count > 0)
        {
            // Check tour tournaments
            if (tSel.tour != null)
            {
                foreach (var tourny in tSel.tour)
                {
                    if (tourny != null && saveData.trophyWonIDs.Contains(tourny.id))
                    {
                        tourny.trophyWon = true;
                        Debug.Log($"  ?? Marked tour '{tourny.name}' (ID {tourny.id}) trophy won");
                    }
                }
            }
            
            // Check provincial tournaments
            if (tSel.provQual != null)
            {
                foreach (var tourny in tSel.provQual)
                {
                    if (tourny != null && saveData.trophyWonIDs.Contains(tourny.id))
                    {
                        tourny.trophyWon = true;
                        Debug.Log($"  ?? Marked qualifier '{tourny.name}' (ID {tourny.id}) trophy won");
                    }
                }
            }
            
            // Check regular tournaments
            if (tSel.tournies != null)
            {
                foreach (var tourny in tSel.tournies)
                {
                    if (tourny != null && saveData.trophyWonIDs.Contains(tourny.id))
                    {
                        tourny.trophyWon = true;
                        Debug.Log($"  ?? Marked tournament '{tourny.name}' (ID {tourny.id}) trophy won");
                    }
                }
            }
        }
        
        // Apply championship completion
        if (tSel.tourChampionship != null)
        {
            tSel.tourChampionship.complete = saveData.tourChampionshipComplete;
            Debug.Log($"[CareerManager] Tour Championship: complete={saveData.tourChampionshipComplete}");
        }
        if (tSel.provChampionship != null)
        {
            tSel.provChampionship.complete = saveData.provChampionshipComplete;
            Debug.Log($"[CareerManager] Prov Championship: complete={saveData.provChampionshipComplete}");
        }
        
        Debug.Log("[CareerManager] ? Tournament completion restored from ID lists");
    }

    private void ClearRankLists()
    {
        provRankList?.Clear();
        tourRankList?.Clear();
    }
    
    /// <summary>
    /// Stores completion IDs temporarily when TournySelector doesn't exist yet
    /// These will be consumed by TournySelector.SetUp() or ApplyPendingCompletionData()
    /// </summary>
    private void StorePendingCompletionData(CareerSaveData saveData)
    {
        Debug.Log($"[CareerManager] StorePendingCompletionData - Preserving completion IDs until TournySelector loads");
        
        // Store completion IDs
        pendingCompletedTournamentIDs = new List<int>(saveData.completedTournamentIDs ?? new List<int>());
        pendingTrophyWonIDs = new List<int>(saveData.trophyWonIDs ?? new List<int>());
        pendingTourChampionshipComplete = saveData.tourChampionshipComplete;
        pendingProvChampionshipComplete = saveData.provChampionshipComplete;
        
        Debug.Log($"  Stored {pendingCompletedTournamentIDs.Count} completed IDs: [{string.Join(", ", pendingCompletedTournamentIDs)}]");
        Debug.Log($"  Stored {pendingTrophyWonIDs.Count} trophy IDs: [{string.Join(", ", pendingTrophyWonIDs)}]");
        Debug.Log($"  Tour Championship: {pendingTourChampionshipComplete}, Prov Championship: {pendingProvChampionshipComplete}");
        Debug.Log($"[CareerManager] ? Completion IDs preserved - will be applied when TournySelector loads");
    }
    
    /// <summary>
    /// Applies pending completion data to TournySelector when it becomes available
    /// Called by TournySelector.SetUp() after LoadCareer()
    /// </summary>
    public void ApplyPendingCompletionData(TournySelector tSel)
    {
        if (tSel == null)
        {
            Debug.LogWarning("[CareerManager] Cannot apply pending completion - TournySelector is null");
            return;
        }
        
        if (pendingCompletedTournamentIDs.Count == 0 && pendingTrophyWonIDs.Count == 0 && 
            !pendingTourChampionshipComplete && !pendingProvChampionshipComplete)
        {
            Debug.Log("[CareerManager] No pending completion data to apply");
            return;
        }
        
        Debug.Log($"[CareerManager] ApplyPendingCompletionData - Applying stored completion IDs to TournySelector");
        Debug.Log($"  Completed IDs to apply: {pendingCompletedTournamentIDs.Count}");
        Debug.Log($"  Trophy IDs to apply: {pendingTrophyWonIDs.Count}");
        
        int restoredCount = 0;
        int trophyCount = 0;
        
        // Restore completed tournaments
        if (pendingCompletedTournamentIDs.Count > 0)
        {
            // Check tour tournaments
            if (tSel.tour != null)
            {
                foreach (var tourny in tSel.tour)
                {
                    if (tourny != null && pendingCompletedTournamentIDs.Contains(tourny.id))
                    {
                        tourny.complete = true;
                        restoredCount++;
                        Debug.Log($"    ? Marked tour '{tourny.name}' (ID {tourny.id}) as complete");
                    }
                }
            }
            
            // Check provincial tournaments
            if (tSel.provQual != null)
            {
                foreach (var tourny in tSel.provQual)
                {
                    if (tourny != null && pendingCompletedTournamentIDs.Contains(tourny.id))
                    {
                        tourny.complete = true;
                        restoredCount++;
                        Debug.Log($"    ? Marked qualifier '{tourny.name}' (ID {tourny.id}) as complete");
                    }
                }
            }
            
            // Check regular tournaments
            if (tSel.tournies != null)
            {
                foreach (var tourny in tSel.tournies)
                {
                    if (tourny != null && pendingCompletedTournamentIDs.Contains(tourny.id))
                    {
                        tourny.complete = true;
                        restoredCount++;
                        Debug.Log($"    ? Marked tournament '{tourny.name}' (ID {tourny.id}) as complete");
                    }
                }
            }
        }
        
        // Restore trophy wins
        if (pendingTrophyWonIDs.Count > 0)
        {
            if (tSel.tour != null)
            {
                foreach (var tourny in tSel.tour)
                {
                    if (tourny != null && pendingTrophyWonIDs.Contains(tourny.id))
                    {
                        tourny.trophyWon = true;
                        trophyCount++;
                        Debug.Log($"    ?? Marked tour '{tourny.name}' (ID {tourny.id}) trophy won");
                    }
                }
            }
            
            if (tSel.provQual != null)
            {
                foreach (var tourny in tSel.provQual)
                {
                    if (tourny != null && pendingTrophyWonIDs.Contains(tourny.id))
                    {
                        tourny.trophyWon = true;
                        trophyCount++;
                        Debug.Log($"    ?? Marked qualifier '{tourny.name}' (ID {tourny.id}) trophy won");
                    }
                }
            }
            
            if (tSel.tournies != null)
            {
                foreach (var tourny in tSel.tournies)
                {
                    if (tourny != null && pendingTrophyWonIDs.Contains(tourny.id))
                    {
                        tourny.trophyWon = true;
                        trophyCount++;
                        Debug.Log($"    ?? Marked tournament '{tourny.name}' (ID {tourny.id}) trophy won");
                    }
                }
            }
        }
        
        // Apply championships
        if (tSel.tourChampionship != null)
        {
            tSel.tourChampionship.complete = pendingTourChampionshipComplete;
            if (pendingTourChampionshipComplete)
            {
                restoredCount++;
                Debug.Log($"    ? Marked Tour Championship as complete");
            }
        }
        
        if (tSel.provChampionship != null)
        {
            tSel.provChampionship.complete = pendingProvChampionshipComplete;
            if (pendingProvChampionshipComplete)
            {
                restoredCount++;
                Debug.Log($"    ? Marked Prov Championship as complete");
            }
        }
        
        Debug.Log($"[CareerManager] ? Pending completion applied: {restoredCount} tournaments, {trophyCount} trophies");
        
        // Clear pending data after applying
        pendingCompletedTournamentIDs.Clear();
        pendingTrophyWonIDs.Clear();
        pendingTourChampionshipComplete = false;
        pendingProvChampionshipComplete = false;
    }


    // Continue with other helper methods like LoadGameProgress, LoadDialogueStatus, etc.

    // Define all the helper methods with specific loading logic

    /// <summary>
    /// Save high score to persistent leaderboard (survives career deletion)
    /// This is called both during career progression AND at career end
    /// </summary>
    IEnumerator SaveHighScore()
    {
        yield return new WaitForEndOfFrame();
        
        // Find player team
        Team playerTeamData = null;
        for (int i = 0; i < teams.Length; i++)
        {
            if (teams[i].id == playerTeamIndex)
            {
                playerTeamData = teams[i];
                break;
            }
        }
        
        if (playerTeamData == null)
        {
            Debug.LogWarning("[CareerManager] Player team not found in SaveHighScore");
            yield break;
        }
        
        // Format full name
        string fullName = $"{playerName} {teamName}";
        
        // Add to high score service
        HighScoreService.AddCareerEntry(
            playerName: playerName,
            teamName: teamName,
            earnings: earnings,
            season: season,
            wins: (int)record.x,
            losses: (int)record.y,
            trophies: currentTrophyList
        );
        
        Debug.Log($"[CareerManager] High score saved: {fullName} - ${earnings:N0}");
    }
    
    /// <summary>
    /// Update high score immediately (called after tournaments, week progression, etc.)
    /// This allows players to appear on the leaderboard even if they don't finish their career
    /// </summary>
    public void UpdateHighScore()
    {
        // Don't save if career hasn't really started yet
        if (week < 1 || earnings <= 0)
        {
            return;
        }
        
        // Update current trophy list from tournaments
        UpdateCurrentTrophyList();
        
        // Save to high score service immediately (no coroutine needed)
        HighScoreService.AddCareerEntry(
            playerName: playerName,
            teamName: teamName,
            earnings: earnings,
            season: season,
            wins: (int)record.x,
            losses: (int)record.y,
            trophies: currentTrophyList
        );
        
        Debug.Log($"[CareerManager] High score updated: {playerName} {teamName} - ${earnings:N0}, {record.x}-{record.y}");
    }
    
    /// <summary>
    /// Update current trophy list from tournament completion status
    /// </summary>
    private void UpdateCurrentTrophyList()
    {
        currentTrophyList = new List<bool>();
        
        // Add regular tournament trophies
        if (tournies != null)
        {
            foreach (var tourny in tournies)
            {
                if (tourny != null)
                {
                    currentTrophyList.Add(tourny.trophyWon);
                }
            }
        }
        
        // Add tour tournament trophies
        if (tour != null)
        {
            foreach (var tourny in tour)
            {
                if (tourny != null)
                {
                    currentTrophyList.Add(tourny.trophyWon);
                }
            }
        }
        
        // Add championship trophies
        if (champ != null && champ.Length >= 2)
        {
            if (champ[0] != null)
            {
                currentTrophyList.Add(champ[0].trophyWon);
            }
            if (champ[1] != null)
            {
                currentTrophyList.Add(champ[1].trophyWon);
            }
        }
        
        Debug.Log($"[CareerManager] Updated trophy list: {currentTrophyList.Count(t => t)}/{currentTrophyList.Count} won");
    }

    public void SaveTournyState(object tournyStateManager = null, GameSettingsPersist gsp = null)
    {
        // Tournament state is now saved via JSON save system in SaveCareer()
        Debug.Log("[CareerManager] SaveTournyState called - tournament state saved via SaveCareer()");
        return;
    }

    public void SaveCareer(
    GameSettingsPersist gsp = null,
    TeamMenu teamSel = null,
    TournySelector tSel = null,
    StorylineManager slm = null,
    EquipmentManager em = null, 
    SponsorManager sm = null
)
    {
        // Auto-find if not provided
        if (gsp == null) gsp = FindFirstObjectByType<GameSettingsPersist>();
        if (teamSel == null) teamSel = FindFirstObjectByType<TeamMenu>();
        if (tSel == null) tSel = FindFirstObjectByType<TournySelector>();
        if (slm == null) slm = FindFirstObjectByType<StorylineManager>();
        if (em == null) em = FindFirstObjectByType<EquipmentManager>();
        if (sm == null) sm = FindFirstObjectByType<SponsorManager>();

        SaveCareerJSON(gsp, teamSel, tSel, slm, em, sm);
        
        loadedFromSave = false;
    }
    
    /// <summary>
    /// Auto-save method called periodically
    /// </summary>
    private void AutoSave()
    {
        if (gameOver)
        {
            // Don't auto-save if career is over
            return;
        }
        
        Debug.Log("[CareerManager] Auto-saving...");
        SaveCareer();
    }
    
    /// <summary>
    /// Public method to manually trigger save (optional)
    /// </summary>
    public void ManualSave()
    {
        Debug.Log("[CareerManager] Manual save triggered");
        SaveCareer();
        autoSaveTimer = 0f; // Reset auto-save timer
    }
    
    /// <summary>
    /// Enable or disable auto-save
    /// </summary>
    public void SetAutoSave(bool enabled)
    {
        enableAutoSave = enabled;
        Debug.Log($"[CareerManager] Auto-save {(enabled ? "enabled" : "disabled")}");
    }
    
    /// <summary>
    /// Deletes the current career save file
    /// </summary>
    public void DeleteCareerSave()
    {
        CareerSaveService.DeleteSave();
        Debug.Log("[CareerManager] Career save deleted");
    }
    
    /// <summary>
    /// Checks if a save file exists
    /// </summary>
    public bool SaveFileExists()
    {
        return CareerSaveService.SaveExists();
    }
    
    /// <summary>
    /// Gets formatted save file information
    /// </summary>
    public string GetSaveFileInfo()
    {
        return CareerSaveService.GetSaveInfo();
    }
    
    /// <summary>
    /// New JSON-based save
    /// </summary>
    private void SaveCareerJSON(
        GameSettingsPersist gsp,
        TeamMenu teamSel,
        TournySelector tSel,
        StorylineManager slm,
        EquipmentManager em,
        SponsorManager sm)
    {
        try
        {
            // Convert current state to save data
            CareerSaveData saveData = ToSaveData();
            
            // CRITICAL FIX: Save game state if EITHER game is in progress OR tournament is in progress
            // This ensures we save justFinishedGame and tournyInProgress flags even after game ends
            if (gsp != null && (gsp.gameInProgress || gsp.tournyInProgress))
            {
                saveData.currentGameState = new GameStateData
                {
                    gameInProgress = gsp.gameInProgress,
                    tournyInProgress = gsp.tournyInProgress,
                    justFinishedGame = gsp.justFinishedGame,  // CRITICAL: Save justFinishedGame flag!
                    isTournyGame = gsp.tourny,
                    ends = gsp.ends,
                    currentEnd = gsp.endCurrent,
                    rocks = gsp.rocks,
                    currentRock = gsp.rockCurrent,
                    redHammer = gsp.redHammer,
                    aiYellow = gsp.aiYellow,
                    aiRed = gsp.aiRed,
                    yellowScore = gsp.yellowScore,
                    redScore = gsp.redScore,
                    yellowTeamName = gsp.yellowTeamName,
                    redTeamName = gsp.redTeamName
                };
                
                // Save rock positions
                if (gsp.rockPos != null)
                {
                    foreach (var pos in gsp.rockPos)
                    {
                        saveData.currentGameState.rockPositions.Add(new Vector2Data(pos));
                    }
                }
                
                if (gsp.rockInPlay != null)
                {
                    saveData.currentGameState.rockInPlay.AddRange(gsp.rockInPlay);
                }
                
                if (gsp.score != null)
                {
                    foreach (var score in gsp.score)
                    {
                        saveData.currentGameState.endScores.Add(new Vector2IntData(score));
                    }
                }
            }
            
            // Add tournament state if tournament is in progress
            if (gsp != null && gsp.tournyInProgress && currentTourny != null)
            {
                saveData.currentTournamentState = CaptureTournamentState(gsp);
            }
            
            // Save using the service
            bool success = CareerSaveService.SaveCareer(saveData);
            
            if (success)
            {
                Debug.Log($"[CareerManager] Career saved successfully to JSON (Week {week}, Season {season})");
            }
            else
            {
                Debug.LogError("[CareerManager] Failed to save career to JSON - CareerSaveService returned false");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CareerManager] Exception during JSON save: {ex.Message}");
            Debug.LogError($"[CareerManager] Stack trace: {ex.StackTrace}");
            Debug.LogError("[CareerManager] Save operation failed - career progress may not be saved!");
        }
    }
    
    public void SetupTourny(TournySelector tSel, GameSettingsPersist gsp)
    {
        currentTourny = tSel.currentTourny;
        Shuffle(teams);
        currentTournyTeams = new Team[currentTourny.teams];

        bool inList = false;
        if (currentTourny.tour)
        {
            gsp.KO3 = true;
            for (int i = 0; i < currentTourny.teams; i++)
            {
                currentTournyTeams[i] = tourTeams[i];

                if (tourTeams[i].id == playerTeamIndex)
                {
                    Debug.Log("PlayerTeam in Tour list");
                    inList = true;
                }
            }

            if (!inList)
            {
                Debug.Log("PlayerTeam not in Tour list");
                for (int i = 0; i < tourTeams.Length; i++)
                {
                    if (tourTeams[i].id == playerTeamIndex)
                    {
                        currentTournyTeams[0] = tourTeams[i];
                        currentTournyTeams[0].name = teamName;
                        currentTournyTeams[0].id = playerTeamIndex;
                        Debug.Log("Added to List - Player Team is " + playerTeamIndex);
                    }
                }
            }
        }
        else
        {
            gsp.KO3 = false;

            if (currentTourny.ko1)
                gsp.KO1 = true;
            else
                gsp.KO1 = false;

            for (int i = 0; i < tSel.locals.Length; i++)
            {
                if (currentTourny.name == tSel.locals[i].name)
                {
                    gsp.cashGame = true;
                    break;
                }
                else
                {
                    gsp.cashGame = false;
                }
            }

            for (int i = 0; i < currentTourny.teams; i++)
            {
                currentTournyTeams[i] = teams[i];

                if (teams[i].id == playerTeamIndex)
                {
                    Debug.Log("PlayerTeam in list");
                    inList = true;
                }
            }

            if (!inList)
            {
                Debug.Log("PlayerTeam not in list");
                for (int i = 0; i < teams.Length; i++)
                {
                    if (teams[i].id == playerTeamIndex)
                    {
                        currentTournyTeams[0] = teams[i];
                        currentTournyTeams[0].name = teamName;
                        currentTournyTeams[0].id = playerTeamIndex;
                        Debug.Log("Not in List Player Team is " + playerTeamIndex);
                    }
                }
            }
        }
        
        //Shuffle(currentTournyTeams);
        Debug.Log("Player Team is " + playerTeamIndex);
        gsp.teams = currentTournyTeams;
        SaveCareer();
    }

    public void TournyResults()
    {
        Debug.Log("Tourny Results in CM");
        GameSettingsPersist gsp = FindFirstObjectByType<GameSettingsPersist>();
        TournyManager tm = FindFirstObjectByType<TournyManager>();
        
        // CRITICAL FIX: Mark tournament as complete in CM arrays IMMEDIATELY
        // This must happen BEFORE any other logic to ensure it's saved
        if (currentTourny != null)
        {
            Debug.Log($"[CareerManager] TournyResults - Marking tournament '{currentTourny.name}' (ID {currentTourny.id}) as complete in CM arrays");
            
            bool marked = false;
            
            // Mark in the appropriate CM array based on tournament type
            if (currentTourny.tour && tour != null)
            {
                for (int i = 0; i < tour.Length; i++)
                {
                    if (tour[i] != null && tour[i].id == currentTourny.id)
                    {
                        tour[i].complete = true;
                        marked = true;
                        Debug.Log($"  ? Marked tour[{i}] '{tour[i].name}' as complete");
                        break;
                    }
                }
            }
            else if (currentTourny.qualifier && prov != null)
            {
                for (int i = 0; i < prov.Length; i++)
                {
                    if (prov[i] != null && prov[i].id == currentTourny.id)
                    {
                        prov[i].complete = true;
                        marked = true;
                        Debug.Log($"  ? Marked prov[{i}] '{prov[i].name}' as complete");
                        break;
                    }
                }
            }
            else if (currentTourny.championship && champ != null)
            {
                for (int i = 0; i < champ.Length; i++)
                {
                    if (champ[i] != null && champ[i].id == currentTourny.id)
                    {
                        champ[i].complete = true;
                        marked = true;
                        Debug.Log($"  ? Marked champ[{i}] '{champ[i].name}' as complete");
                        break;
                    }
                }
            }
            else if (tournies != null)
            {
                for (int i = 0; i < tournies.Length; i++)
                {
                    if (tournies[i] != null && tournies[i].id == currentTourny.id)
                    {
                        tournies[i].complete = true;
                        marked = true;
                        Debug.Log($"  ? Marked tournies[{i}] '{tournies[i].name}' as complete");
                        break;
                    }
                }
            }
            
            if (!marked)
            {
                Debug.LogError($"[CareerManager] ? Failed to mark tournament '{currentTourny.name}' (ID {currentTourny.id}) - not found in any CM array!");
                Debug.LogError($"  tour: {tour?.Length ?? 0}, prov: {prov?.Length ?? 0}, champ: {champ?.Length ?? 0}, tournies: {tournies?.Length ?? 0}");
            }
        }
        else
        {
            Debug.LogError("[CareerManager] ? currentTourny is NULL in TournyResults - cannot mark complete!");
        }

        cashDelta = gsp.tournyEarnings;
        cash += cashDelta;
        earnings += cashDelta;

        float xpChange = 0f;
        if (!gsp.cashGame)
        {
            currentTournyTeams = gsp.teams;

            // Update cm.record from the player's team in currentTournyTeams
            // This team has the cumulative wins/losses after TournyComplete() restored them
            for (int i = 0; i < currentTournyTeams.Length; i++)
            {
                if (playerTeamIndex == currentTournyTeams[i].id)
                {
                    // Set record to the team's cumulative stats (which include pre-tournament + tournament)
                    record.x = currentTournyTeams[i].seasonWins;
                    record.y = currentTournyTeams[i].seasonLosses;
                    
                    Debug.Log($"[CareerManager] Updated cm.record to {record.x}-{record.y} from team {currentTournyTeams[i].name}");
                    break;
                }
            }

            for (int i = 0; i < currentTournyTeams.Length; i++)
            {
                if (playerTeamIndex == currentTournyTeams[i].id)
                {
                    tournyResults.Add(currentTournyTeams[i].rank);
                    xpChange = currentTournyTeams.Length - currentTournyTeams[i].rank;
                    xpChange += currentTournyTeams[i].wins * 3f;
                    xpChange += currentTournyTeams[i].loss;

                    if (currentTournyTeams[i].rank == 1)
                    {
                        if (currentTourny.tour)
                        {
                            for (int j = 0; j < tour.Length; j++)
                            {
                                if (currentTourny.id == tour[j].id)
                                {
                                    tour[j].trophyWon = true;
                                }
                            }
                        }
                        else if (currentTourny.championship)
                        {
                            for (int j = 0; j < champ.Length; j++)
                            {
                                if (currentTourny.id == champ[j].id)
                                {
                                    champ[j].trophyWon = true;
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < tournies.Length; j++)
                            {
                                if (currentTourny.id == tournies[j].id)
                                {
                                    tournies[j].trophyWon = true;
                                }
                            }
                        }

                        currentTourny.trophyWon = true;
                    }

                    if (currentTournyTeams[i].rank < TOP_RANK_THRESHOLD)
                    {
                        xpChange += XP_TOP5_BONUS;
                    }
                }
            }
        }

        if (currentTourny.qualifier)
        {
            for (int i = 0; i < currentTournyTeams.Length; i++)
            {
                if (playerTeamIndex == currentTournyTeams[i].id)
                {
                    if (currentTournyTeams[i].rank < TOP_RANK_THRESHOLD)
                    {
                        provQual = true;
                        xpChange += XP_QUALIFIER_BONUS;
                    }
                }
            }
        }

        if (currentTourny.tour)
        {
            for (int i = 0; i < currentTournyTeams.Length; i++)
            {
                switch (currentTournyTeams[i].rank)
                {
                    case 1:
                        currentTournyTeams[i].tourPoints += 25f;
                        currentTourny.trophyWon = true;
                        break;
                    case 2:
                        currentTournyTeams[i].tourPoints += 18f;
                        break;
                    case 3:
                        currentTournyTeams[i].tourPoints += 15f;
                        break;
                    case 4:
                        currentTournyTeams[i].tourPoints += 12f;
                        break;
                    case 5:
                        currentTournyTeams[i].tourPoints += 10f;
                        break;
                    case 6:
                        currentTournyTeams[i].tourPoints += 8f;
                        break;
                    case 7:
                        currentTournyTeams[i].tourPoints += 6f;
                        break;
                    case 9:
                        currentTournyTeams[i].tourPoints += 4f;
                        break;
                    case 11:
                        currentTournyTeams[i].tourPoints += 2f;
                        break;
                    case 13:
                        currentTournyTeams[i].tourPoints += 1f;
                        break;
                }
            }

            Debug.Log("Team Record - " + teams[0].name + " - " + teamRecords[0]);
        }

        for (int i = 0; i < teams.Length; i++)
        {
            if (teams[i].id == playerTeamIndex)
            {
                playerTeam = teams[i];
            }
        }

        currentTrophyList = new List<bool>();

        for (int i = 0; i < tournies.Length; i++)
        {
            if (tournies[i].trophyWon)
                currentTrophyList.Add(true);
            else
                currentTrophyList.Add(false);
        }

        for (int i = 0; i < tour.Length; i++)
        {
            if (tour[i].trophyWon)
                currentTrophyList.Add(true);
            else
                currentTrophyList.Add(false);
        }

        for (int i = 0; i < champ.Length; i++)
        {
            if (champ[i].trophyWon)
                currentTrophyList.Add(true);
            else
                currentTrophyList.Add(false);
        }

        if (currentTourny.tour)
        {
            // Tour event
            if (playerTeamIndex == GetPlayerRankedTeamId())
            {
                if (GetPlayerRank() == 1)
                    xpChange += XP_TOUR_WIN;
                else if (GetPlayerRank() <= 4)
                    xpChange += XP_TOUR_TOP4;
                else
                    xpChange += XP_TOUR_PARTICIPATION;
            }
        }
        else if (currentTourny.championship)
        {
            // Championship event
            if (playerTeamIndex == GetPlayerRankedTeamId())
            {
                if (GetPlayerRank() == 1)
                    xpChange += XP_CHAMPIONSHIP_WIN;
                else if (GetPlayerRank() <= 4)
                    xpChange += XP_CHAMPIONSHIP_TOP4;
                else
                    xpChange += XP_CHAMPIONSHIP_PARTICIPATION;
            }
        }
        else if (currentTourny.qualifier)
        {
            // Qualifier event
            if (playerTeamIndex == GetPlayerRankedTeamId())
            {
                if (GetPlayerRank() < TOP_RANK_THRESHOLD)
                    xpChange += XP_QUALIFIER_SUCCESS;
                else
                    xpChange += XP_QUALIFIER_PARTICIPATION;
            }
        }
        else
        {
            // Local or regular tournament
            if (playerTeamIndex == GetPlayerRankedTeamId())
            {
                if (GetPlayerRank() == 1)
                    xpChange += XP_LOCAL_WIN;
                else if (GetPlayerRank() <= 4)
                    xpChange += XP_LOCAL_TOP4;
                else
                    xpChange += XP_LOCAL_PARTICIPATION;
            }
        }

        // Optionally, add XP for wins/losses
        if (playerTeamIndex == GetPlayerRankedTeamId())
        {
            xpChange += GetPlayerWins() * XP_PER_WIN;
            xpChange += GetPlayerLosses() * XP_PER_LOSS;
        }
        //if (allTimeTrophyList.Count <= 0)
        //    allTimeTrophyList = new List<bool>();

        Debug.Log("allTimeTrophyList2 Count - " + currentTrophyList.Count);
        xp += xpChange;
        provRankList.Sort();
        week++;
        RebalanceTeamStrength();
        Debug.Log("CM Tourny Results week is " + week);
        
        // CRITICAL: Update high score after tournament completion
        // This ensures players appear on leaderboard even if they don't finish their career
        UpdateHighScore();
        
        SaveCareer();
    }

    public void PlayTourny()
    {
        GameSettingsPersist gsp = FindFirstObjectByType<GameSettingsPersist>();
        Debug.Log("CM Play Tourny");

        teamRecords = new Vector4[totalTeams];
        tourRecords = new Vector4[totalTourTeams];

        for (int i = 0; i < totalTourTeams; i++)
        {
            tourRecords[i].x = tourTeams[i].tourRecord.x;
            tourRecords[i].y = tourTeams[i].tourRecord.y;
            tourRecords[i].z = tourTeams[i].tourPoints;
            tourRecords[i].w = tourTeams[i].id;
        }

        for (int i = 0; i < totalTeams; i++)
        {
            teamRecords[i].x = teams[i].wins;
            teamRecords[i].y = teams[i].loss;
            teamRecords[i].z = teams[i].earnings;
            teamRecords[i].w = teams[i].id;
        }

        Debug.Log("Tour Record is " + tourRecords + " - Team Record is " + teamRecords);

        //earnings = gsp.earnings;
        SaveCareer();
    }

    public void NextWeek()
    {
        week++;
        
        // Decrement active sponsor card durations
        if (activeCardLengthList != null && activeCardLengthList.Length > 0)
        {
            for (int i = 0; i < activeCardLengthList.Length; i++)
            {
                // Only decrement if duration is less than 50 (50+ means permanent sponsor)
                if (activeCardLengthList[i] > 0 && activeCardLengthList[i] < 50)
                {
                    activeCardLengthList[i]--;
                    Debug.Log($"[CareerManager] Decremented sponsor {i} duration to {activeCardLengthList[i]}");
                    
                    // If duration reaches 0, mark card as expired
                    if (activeCardLengthList[i] == 0)
                    {
                        Debug.Log($"[CareerManager] Sponsor {i} (ID: {activeCardIDList[i]}) has expired");
                        // Card will be handled by SponsorManager.SetUp() which checks duration
                    }
                }
            }
        }
        
        // Update high score after progressing to next week
        // This ensures continuous leaderboard updates
        UpdateHighScore();
        
        SaveCareer();
        //tSel.SetUp();
    }

    public void ContinueSeason()
    {
        Debug.Log("CM - Continue Season");

        GameSettingsPersist gsp = FindFirstObjectByType<GameSettingsPersist>();
        season++;

        Shuffle(teamPool);

        teams = new Team[totalTeams];
        //tourRankList = new List<Team_List>();
        //provQualList = new List<Team_List>();

        for (int i = 0; i < totalTeams; i++)
        {
            teams[i] = teamPool[i];
        }
        teams[0].name = teamName;
        teams[0].player = true;
        playerTeamIndex = teams[0].id;
        gsp.playerTeamIndex = playerTeamIndex;
        week++;
    }

    public void NewSeason()
    {
        GameSettingsPersist gsp = FindFirstObjectByType<GameSettingsPersist>();
        TournySelector tSel = FindFirstObjectByType<TournySelector>();
        EquipmentManager em = FindFirstObjectByType<EquipmentManager>();

        // Reset progression
        provRankList = new List<Standings_List>();
        tourRankList = new List<TourStandings_List>();
        
        xp = 0f;
        skillPoints = 0;
        level = 0;
        
        loadedFromSave = false;
        gameOver = false;
        record = new Vector2(0f, 0f);
        tourRecord = new Vector2(0f, 0f);
        
        // Reset qualifications
        provQual = false;
        tourQual = false;

        // Reset player character stats to starting values
        cStats.drawAccuracy = STARTING_STAT_VALUE;
        cStats.guardAccuracy = STARTING_STAT_VALUE;
        cStats.takeOutAccuracy = STARTING_STAT_VALUE;
        cStats.sweepStrength = STARTING_STAT_VALUE;
        cStats.sweepEndurance = STARTING_STAT_VALUE;
        cStats.sweepCohesion = STARTING_STAT_VALUE;
        
        // Reset modifier stats (from equipment/sponsors)
        modStats.drawAccuracy = 0;
        modStats.guardAccuracy = 0;
        modStats.takeOutAccuracy = 0;
        modStats.sweepStrength = 0;
        modStats.sweepEndurance = 0;
        modStats.sweepCohesion = 0;
        
        // Reset opponent modifier stats
        oppStats.drawAccuracy = 0;
        oppStats.guardAccuracy = 0;
        oppStats.takeOutAccuracy = 0;
        oppStats.sweepStrength = 0;
        oppStats.sweepEndurance = 0;
        oppStats.sweepCohesion = 0;

        season++;

        // Reset team to starting players
        activePlayers = new Player[3];
        for (int i = 0; i < activePlayers.Length; i++)
        {
            activePlayers[i] = playerPool[i];
        }
        
        // Clear equipment
        activeEquipID = null;
        inventoryID = null;
        
        // Clear all sponsor/card data
        activeCardIDList = null;
        cardPUIDList = null;
        cardSponsorIDList = null;
        playedCardIDList = null;
        activeCardLengthList = null;

        Shuffle(teamPool);
        if (em != null)
        {
            em.EnsureDefaultActiveEquip();
        }
        
        teams = new Team[totalTeams];
        tourTeams = new Team[totalTourTeams];
        Debug.Log("provRankList Count is " + provRankList.Count);

        string[] firstNames = { "Alex", "Jamie", "Taylor", "Jordan", "Morgan", "Casey", "Riley", "Drew", "Sam", "Cameron" };
        string[] lastNames = { "Smith", "Johnson", "Lee", "Brown", "Wilson", "Moore", "Clark", "Hall", "Young", "King" };

        int playersPerTeam = PLAYERS_PER_TEAM;

        int baseStatTotal = BASE_STAT_TOTAL;
        int maxStatTotal = MAX_STAT_TOTAL;

        System.Random rand = new System.Random();

        // Define weights for each role: [draw, takeOut, guard, sweepStrength, sweepEnduro, sweepCohesion]
        float[][] roleWeights = new float[][]
        {
        // Lead: heavy sweeping
        new float[] { 0.10f, 0.10f, 0.10f, 0.23f, 0.23f, 0.24f },
        // Second: balanced, but still sweeping focused
        new float[] { 0.13f, 0.13f, 0.13f, 0.20f, 0.20f, 0.21f },
        // Third: shooting focused, but still good at sweeping
        new float[] { 0.18f, 0.18f, 0.18f, 0.15f, 0.15f, 0.16f },
        // Skip: captain, best shooter
        new float[] { 0.22f, 0.22f, 0.22f, 0.11f, 0.11f, 0.12f }
        };

        for (int i = 0; i < totalTeams; i++)
        {
            teams[i] = teamPool[i];
            teams[i].players = new List<Player>();
        }

        // Find min and max strength in the league for scaling
        int minStrength = teams.Min(t => t.strength);
        int maxStrength = teams.Max(t => t.strength);

        for (int i = 0; i < totalTeams; i++)
        {
            float normalized = (maxStrength > minStrength) ? (float)(teams[i].strength - minStrength) / (maxStrength - minStrength) : 0.5f;

            // Calculate stat total based on normalized strength
            int statTotal = Mathf.RoundToInt(baseStatTotal + normalized * (maxStatTotal - baseStatTotal));

            // Add a random weekly variation
            statTotal += rand.Next(-STAT_VARIATION, STAT_VARIATION + 1);

            // Clamp to valid range
            statTotal = Mathf.Clamp(statTotal, baseStatTotal, maxStatTotal);

            // Now use statTotal for this team's stat assignment
            // Example: assign to all players on the team, or store for later
            Debug.Log($"Team {teams[i].name} (strength {teams[i].strength}) statTotal: {statTotal}");

            for (int p = 0; p < playersPerTeam; p++)
            {
                Player player = new Player();
                string firstName = firstNames[rand.Next(firstNames.Length)];
                string lastName = (p == 3) ? teams[i].name : lastNames[rand.Next(lastNames.Length)];
                player.name = firstName + " " + lastName;
                player.id = p;

                float[] weights = roleWeights[p];
                int[] stats = new int[NUM_STATS];
                int assigned = 0;

                // Assign stats based on weights, rounding down
                for (int s = 0; s < NUM_STATS; s++)
                {
                    stats[s] = (int)(statTotal * weights[s]);
                    assigned += stats[s];
                }
                // Distribute any remaining points randomly
                int remaining = statTotal - assigned;
                for (int r = 0; r < remaining; r++)
                {
                    int idx = rand.Next(NUM_STATS);
                    stats[idx]++;
                }

                player.draw = stats[0];
                player.takeOut = stats[1];
                player.guard = stats[2];
                player.sweepStrength = stats[3];
                player.sweepEnduro = stats[4];
                player.sweepCohesion = stats[5];
                teams[i].player = false;
                teams[i].players.Add(player);
            }

            teams[i].UpdateTeamSkillsFromPlayers();
            Debug.Log("Team Strength - " + teams[i].name + " - " + teams[i].strength);
            provRankList.Add(new Standings_List(teams[i]));
        }

        provQual = false;
        tourQual = false;

        earnings = 0f;
        teams[0].name = teamName;
        teams[0].player = true;
        teams[0].players.Clear();

        foreach (var p in activePlayers)
        {
            teams[0].players.Add(p);
        }
        UpdateCharacter();
        teams[0].players.Add(playerCharacter);
        
        // Update team skills from player stats
        teams[0].UpdateTeamSkillsFromPlayers();
        Debug.Log($"[CareerManager] Player team skills: draw={teams[0].draw}, strength={teams[0].strength}");
        
        teams[0].earnings = earnings;
        cash = STARTING_CASH;
        playerTeamIndex = teams[0].id;
        gsp.playerTeamIndex = playerTeamIndex;
        playerTeam = teams[0];
        Shuffle(teams);
        bool inList = true;

        for (int i = 0; i < totalTourTeams; i++)
        {
            tourTeams[i] = teams[i];
        }

        for (int i = 0; i < totalTourTeams; i++)
        {
            if (tourTeams[i].id != playerTeamIndex)
            {
                inList = false;
            }
            else
            {
                inList = true;
                break;
            }
        }

        if (!inList)
        {
            Debug.Log("inList is " + inList);
            for (int i = 0; i < totalTeams; i++)
            {
                if (teams[i].id == playerTeamIndex)
                {
                    tourTeams[0] = teams[i];
                }
            }
        }

        activeCardIDList = null;
        cardPUIDList = null;
        cardSponsorIDList = null;
        week++;
        //inProgress = true;
        SaveCareer();
    }

    public void EndCareer()
    {
        Debug.Log("Ending Career");
        gameOver = true;
        SaveCareer();
        StartCoroutine(SaveHighScore());
    }

    void Shuffle(Team[] a)
    {
        // Loops through array
        for (int i = a.Length - 1; i > 0; i--)
        {
            // Randomize a number between 0 and i (so that the range decreases each time)
            int rnd = UnityEngine.Random.Range(0, i);

            // Save the value of the current i, otherwise it'll overright when we swap the values
            Team temp = a[i];

            // Swap the new and old values
            a[i] = a[rnd];
            a[rnd] = temp;
        }
    }

    public void RebalanceTeamStrength()
    {
        System.Random rand = new System.Random();

        // DIFFICULTY CURVE: Steady ramp-up with championship spike
        // Week 1-6: Gradual increase (+20 stats/week = slower progression)
        // Championship weeks: +50 stat spike for elite competition
        bool isChampionshipWeek = (currentTourny != null && currentTourny.championship);
        
        int weeklyIncrease = isChampionshipWeek ? 50 : 20;  // Championship spike!
        int baseStatTotal = 170 + (week * weeklyIncrease);
        int maxStatTotal = 250 + (week * weeklyIncrease);
        
        Debug.Log($"[CareerManager] Week {week} opponent stats: {baseStatTotal}-{maxStatTotal} (Championship: {isChampionshipWeek})");

        // Find min and max strength in the league for scaling
        int minStrength = teams.Min(t => t.strength);
        int maxStrength = teams.Max(t => t.strength);

        // Define weights for each role: [draw, takeOut, guard, sweepStrength, sweepEnduro, sweepCohesion]
        float[][] roleWeights = new float[][]
        {
        new float[] { 0.10f, 0.10f, 0.10f, 0.23f, 0.23f, 0.24f },
        new float[] { 0.13f, 0.13f, 0.13f, 0.20f, 0.20f, 0.21f },
        new float[] { 0.18f, 0.18f, 0.18f, 0.15f, 0.15f, 0.16f },
        new float[] { 0.22f, 0.22f, 0.22f, 0.11f, 0.11f, 0.12f }
        };

        for (int i = 0; i < teams.Length; i++)
        {
            if (teams[i] == null || teams[i].players == null || teams[i].players.Count != 4)
                continue;

            float normalized = (maxStrength > minStrength)
                ? (float)(teams[i].strength - minStrength) / (maxStrength - minStrength)
                : 0.5f;

            // Calculate stat total based on normalized strength
            int statTotal = Mathf.RoundToInt(baseStatTotal + normalized * (maxStatTotal - baseStatTotal));
            statTotal += rand.Next(-5, 6);
            statTotal = Mathf.Clamp(statTotal, baseStatTotal, maxStatTotal);

            // Redistribute statTotal among all players using the same logic as NewSeason
            int[] allStats = new int[4 * 6];
            int assigned = 0;

            // First, assign base stats using weights
            for (int p = 0; p < 4; p++)
            {
                float[] weights = roleWeights[p];
                for (int s = 0; s < 6; s++)
                {
                    int idx = p * 6 + s;
                    allStats[idx] = (int)(statTotal * weights[s]);
                    assigned += allStats[idx];
                }
            }

            // Distribute any remaining points randomly
            int remaining = statTotal - assigned;
            for (int r = 0; r < remaining; r++)
            {
                int idx = rand.Next(4 * 6);
                allStats[idx]++;
            }

            // Now assign the stats back to the players
            for (int p = 0; p < 4; p++)
            {
                Player player = teams[i].players[p];
                player.draw = allStats[p * 6 + 0];
                player.takeOut = allStats[p * 6 + 1];
                player.guard = allStats[p * 6 + 2];
                player.sweepStrength = allStats[p * 6 + 3];
                player.sweepEnduro = allStats[p * 6 + 4];
                player.sweepCohesion = allStats[p * 6 + 5];
            }

            teams[i].UpdateTeamSkillsFromPlayers();
            Debug.Log("Team Strength - " + teams[i].name + " - " + teams[i].strength);
        }
    }

    int GetPlayerRankedTeamId()
    {
        foreach (var team in currentTournyTeams)
            if (team.id == playerTeamIndex)
                return team.id;
        return -1;
    }

    int GetPlayerRank()
    {
        foreach (var team in currentTournyTeams)
            if (team.id == playerTeamIndex)
                return team.rank;
        return -1;
    }

    int GetPlayerWins()
    {
        foreach (var team in currentTournyTeams)
            if (team.id == playerTeamIndex)
                return team.wins;
        return 0;
    }

    int GetPlayerLosses()
    {
        foreach (var team in currentTournyTeams)
            if (team.id == playerTeamIndex)
                return team.loss;
        return 0;
    }
    
    #region JSON SAVE/LOAD CONVERSION METHODS
    
    /// <summary>
    /// Converts current CareerManager state to CareerSaveData
    /// </summary>
    private CareerSaveData ToSaveData()
    {
        CareerSaveData data = new CareerSaveData();
        
        // Player Identity
        data.playerName = playerName;
        data.teamName = teamName;
        data.teamColour = new SerializableColor(teamColour);
        data.playerTeamIndex = playerTeamIndex;
        
        // Progress & Resources
        data.week = week;
        data.season = season;
        data.cash = cash;
        data.earnings = earnings;
        data.xp = xp;
        data.level = level;
        data.skillPoints = skillPoints;
        
        // Career Record
        data.careerWins = (int)record.x;
        data.careerLosses = (int)record.y;
        
        // Career Stats
        data.careerStats = new CareerStatsData
        {
            drawAccuracy = cStats.drawAccuracy,
            guardAccuracy = cStats.guardAccuracy,
            takeOutAccuracy = cStats.takeOutAccuracy,
            sweepStrength = cStats.sweepStrength,
            sweepEndurance = cStats.sweepEndurance,
            sweepCohesion = cStats.sweepCohesion
        };
        
        data.oppStats = new CareerStatsData
        {
            drawAccuracy = oppStats.drawAccuracy,
            guardAccuracy = oppStats.guardAccuracy,
            takeOutAccuracy = oppStats.takeOutAccuracy,
            sweepStrength = oppStats.sweepStrength,
            sweepEndurance = oppStats.sweepEndurance,
            sweepCohesion = oppStats.sweepCohesion
        };
        
        // Qualifications
        data.provQual = provQual;
        data.tourQual = tourQual;
        
        // Teams
        if (teams != null && teams.Length > 0)
        {
            foreach (var team in teams)
            {
                if (team != null)
                {
                    data.teams.Add(TeamToData(team));
                }
            }
        }
        
        if (tourTeams != null && tourTeams.Length > 0)
        {
            foreach (var team in tourTeams)
            {
                if (team != null)
                {
                    data.tourTeams.Add(TeamToData(team));
                }
            }
        }
        
        // Team Records
        if (teamRecords != null && teamRecords.Length > 0)
        {
            foreach (var record in teamRecords)
            {
                data.teamRecords.Add(new TeamRecordData
                {
                    teamId = (int)record.w,
                    wins = (int)record.x,
                    losses = (int)record.y,
                    earnings = record.z
                });
            }
        }
        
        if (tourRecords != null && tourRecords.Length > 0)
        {
            foreach (var record in tourRecords)
            {
                data.tourRecords.Add(new TeamRecordData
                {
                    teamId = (int)record.w,
                    wins = (int)record.x,
                    losses = (int)record.y,
                    earnings = record.z
                });
            }
        }
        
        // Active Players
        if (activePlayers != null && activePlayers.Length > 0)
        {
            foreach (var player in activePlayers)
            {
                if (player != null)
                {
                    data.activePlayers.Add(PlayerToData(player));
                }
            }
        }
        
        // Equipment
        if (activeEquipID != null) data.activeEquipIDs.AddRange(activeEquipID);
        if (inventoryID != null) data.inventoryIDs.AddRange(inventoryID);
        
        // Serialize all equipment if EquipmentManager exists
        EquipmentManager em = FindFirstObjectByType<EquipmentManager>();
        if (em != null)
        {
            data.allEquipment = new EquipmentCollectionData();
            
            // Serialize handles
            if (em.handles != null)
            {
                foreach (var eq in em.handles)
                {
                    if (eq != null)
                    {
                        data.allEquipment.handles.Add(EquipmentToData(eq));
                    }
                }
            }
            
            // Serialize heads
            if (em.heads != null)
            {
                foreach (var eq in em.heads)
                {
                    if (eq != null)
                    {
                        data.allEquipment.heads.Add(EquipmentToData(eq));
                    }
                }
            }
            
            // Serialize footwear
            if (em.footwear != null)
            {
                foreach (var eq in em.footwear)
                {
                    if (eq != null)
                    {
                        data.allEquipment.footwear.Add(EquipmentToData(eq));
                    }
                }
            }
            
            // Serialize apparel
            if (em.apparel != null)
            {
                foreach (var eq in em.apparel)
                {
                    if (eq != null)
                    {
                        data.allEquipment.apparel.Add(EquipmentToData(eq));
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("[CareerManager] EquipmentManager not found during save - equipment data will not be saved");
            data.allEquipment = new EquipmentCollectionData(); // Save empty collection
        }
        
        // Cards/Sponsors
        if (cardPUIDList != null) data.cardPowerUpIDs.AddRange(cardPUIDList);
        if (cardSponsorIDList != null) data.cardSponsorIDs.AddRange(cardSponsorIDList);
        if (activeCardIDList != null) data.activeCardIDs.AddRange(activeCardIDList);
        if (playedCardIDList != null) data.playedCardIDs.AddRange(playedCardIDList);
        if (activeCardLengthList != null) data.activeCardLengths.AddRange(activeCardLengthList);
        
        // Tournament completion - Save as simple ID lists!
        // Try TournySelector first, fall back to CM arrays if in tournament scene
        TournySelector tSel = FindFirstObjectByType<TournySelector>();
        
        Debug.Log($"[SAVE DEBUG] Starting tournament completion save - TournySelector exists: {tSel != null}");
        
        if (tSel != null)
        {
            Debug.Log("[CareerManager] TournySelector found - saving tournament completion IDs from TournySelector");
            Debug.Log($"[SAVE DEBUG] TournySelector arrays - provQual: {tSel.provQual?.Length ?? 0}, tour: {tSel.tour?.Length ?? 0}, tournies: {tSel.tournies?.Length ?? 0}");
            
            // Collect completed tournament IDs from TournySelector
            if (tSel.provQual != null)
            {
                foreach (var tourny in tSel.provQual)
                {
                    if (tourny != null && tourny.complete)
                    {
                        data.completedTournamentIDs.Add(tourny.id);
                        Debug.Log($"  ? Saved completed: '{tourny.name}' (ID {tourny.id})");
                    }
                    if (tourny != null && tourny.trophyWon)
                    {
                        data.trophyWonIDs.Add(tourny.id);
                        Debug.Log($"  ?? Saved trophy: '{tourny.name}' (ID {tourny.id})");
                    }
                }
            }
            
            if (tSel.tour != null)
            {
                foreach (var tourny in tSel.tour)
                {
                    if (tourny != null && tourny.complete)
                    {
                        data.completedTournamentIDs.Add(tourny.id);
                        Debug.Log($"  ? Saved completed: '{tourny.name}' (ID {tourny.id})");
                    }
                    if (tourny != null && tourny.trophyWon)
                    {
                        data.trophyWonIDs.Add(tourny.id);
                        Debug.Log($"  ?? Saved trophy: '{tourny.name}' (ID {tourny.id})");
                    }
                }
            }
            
            if (tSel.tournies != null)
            {
                foreach (var tourny in tSel.tournies)
                {
                    if (tourny != null && tourny.complete)
                    {
                        data.completedTournamentIDs.Add(tourny.id);
                        Debug.Log($"  ? Saved completed: '{tourny.name}' (ID {tourny.id})");
                    }
                    if (tourny != null && tourny.trophyWon)
                    {
                        data.trophyWonIDs.Add(tourny.id);
                        Debug.Log($"  ?? Saved trophy: '{tourny.name}' (ID {tourny.id})");
                    }
                }
            }
            
            // Save championships
            if (tSel.tourChampionship != null && tSel.tourChampionship.complete)
            {
                data.tourChampionshipComplete = true;
            }
            if (tSel.provChampionship != null && tSel.provChampionship.complete)
            {
                data.provChampionshipComplete = true;
            }
            
            Debug.Log($"[CareerManager] ? Saved from TournySelector: {data.completedTournamentIDs.Count} completed IDs, {data.trophyWonIDs.Count} trophy IDs");
            Debug.Log($"[SAVE DEBUG] Completed IDs saved: [{string.Join(", ", data.completedTournamentIDs)}]");
            Debug.Log($"[SAVE DEBUG] Trophy IDs saved: [{string.Join(", ", data.trophyWonIDs)}]");
        }
        else
        {
            // CRITICAL FIX: TournySelector doesn't exist (we're in tournament scene)
            // PRIORITY 1: Use pending completion data (from previous save) if available
            // PRIORITY 2: Fall back to CM arrays (only valid if just completed tournament)
            Debug.Log("[CareerManager] TournySelector not found - checking pending data vs CM arrays");
            
            bool usedPendingData = false;
            
            // Check if we have pending data from the load
            if (pendingCompletedTournamentIDs.Count > 0 || pendingTrophyWonIDs.Count > 0 ||
                pendingTourChampionshipComplete || pendingProvChampionshipComplete)
            {
                Debug.Log($"[CareerManager] ? Using PENDING data from previous save");
                Debug.Log($"  Pending completed: {pendingCompletedTournamentIDs.Count} IDs: [{string.Join(", ", pendingCompletedTournamentIDs)}]");
                Debug.Log($"  Pending trophies: {pendingTrophyWonIDs.Count} IDs: [{string.Join(", ", pendingTrophyWonIDs)}]");
                
                // Copy pending data to save
                data.completedTournamentIDs.AddRange(pendingCompletedTournamentIDs);
                data.trophyWonIDs.AddRange(pendingTrophyWonIDs);
                data.tourChampionshipComplete = pendingTourChampionshipComplete;
                data.provChampionshipComplete = pendingProvChampionshipComplete;
                
                usedPendingData = true;
                
                Debug.Log($"[CareerManager] ? Preserved pending data: {data.completedTournamentIDs.Count} completed, {data.trophyWonIDs.Count} trophies");
            }
            
            // ALSO check CM arrays and MERGE any NEW completions (from TournyResults)
            List<int> cmCompletedIDs = new List<int>();
            List<int> cmTrophyIDs = new List<int>();
            
            if (prov != null)
            {
                foreach (var tourny in prov)
                {
                    if (tourny != null && tourny.complete && !data.completedTournamentIDs.Contains(tourny.id))
                    {
                        cmCompletedIDs.Add(tourny.id);
                        data.completedTournamentIDs.Add(tourny.id);
                        Debug.Log($"  ? NEW from CM prov: '{tourny.name}' (ID {tourny.id})");
                    }
                    if (tourny != null && tourny.trophyWon && !data.trophyWonIDs.Contains(tourny.id))
                    {
                        cmTrophyIDs.Add(tourny.id);
                        data.trophyWonIDs.Add(tourny.id);
                        Debug.Log($"  ?? NEW trophy from CM prov: '{tourny.name}' (ID {tourny.id})");
                    }
                }
            }
            
            if (tour != null)
            {
                foreach (var tourny in tour)
                {
                    if (tourny != null && tourny.complete && !data.completedTournamentIDs.Contains(tourny.id))
                    {
                        cmCompletedIDs.Add(tourny.id);
                        data.completedTournamentIDs.Add(tourny.id);
                        Debug.Log($"  ? NEW from CM tour: '{tourny.name}' (ID {tourny.id})");
                    }
                    if (tourny != null && tourny.trophyWon && !data.trophyWonIDs.Contains(tourny.id))
                    {
                        cmTrophyIDs.Add(tourny.id);
                        data.trophyWonIDs.Add(tourny.id);
                        Debug.Log($"  ?? NEW trophy from CM tour: '{tourny.name}' (ID {tourny.id})");
                    }
                }
            }
            
            if (tournies != null)
            {
                foreach (var tourny in tournies)
                {
                    if (tourny != null && tourny.complete && !data.completedTournamentIDs.Contains(tourny.id))
                    {
                        cmCompletedIDs.Add(tourny.id);
                        data.completedTournamentIDs.Add(tourny.id);
                        Debug.Log($"  ? NEW from CM tournies: '{tourny.name}' (ID {tourny.id})");
                    }
                    if (tourny != null && tourny.trophyWon && !data.trophyWonIDs.Contains(tourny.id))
                    {
                        cmTrophyIDs.Add(tourny.id);
                        data.trophyWonIDs.Add(tourny.id);
                        Debug.Log($"  ?? NEW trophy from CM tournies: '{tourny.name}' (ID {tourny.id})");
                    }
                }
            }
            
            // Merge championships
            if (champ != null && champ.Length >= 2)
            {
                if (champ[0] != null && champ[0].complete)
                {
                    data.tourChampionshipComplete = true;
                }
                if (champ[1] != null && champ[1].complete)
                {
                    data.provChampionshipComplete = true;
                }
            }
            
            if (cmCompletedIDs.Count > 0 || cmTrophyIDs.Count > 0)
            {
                Debug.Log($"[CareerManager] ? MERGED {cmCompletedIDs.Count} new completions from CM arrays: [{string.Join(", ", cmCompletedIDs)}]");
                Debug.Log($"[CareerManager] ? MERGED {cmTrophyIDs.Count} new trophies from CM arrays: [{string.Join(", ", cmTrophyIDs)}]");
            }
            
            Debug.Log($"[CareerManager] ? FINAL save data: {data.completedTournamentIDs.Count} completed IDs, {data.trophyWonIDs.Count} trophy IDs");
            Debug.Log($"[SAVE DEBUG] Completed IDs saved (MERGED): [{string.Join(", ", data.completedTournamentIDs)}]");
            Debug.Log($"[SAVE DEBUG] Trophy IDs saved (MERGED): [{string.Join(", ", data.trophyWonIDs)}]");
        }
        
        // Tournament Results
        if (tournyResults != null) data.tournamentResults.AddRange(tournyResults);
        
        // Active Tournaments (offered this week)
        if (activeTournies != null && activeTournies.Length > 0)
        {
            foreach (var tourny in activeTournies)
            {
                if (tourny != null)
                {
                    data.activeTournamentIDs.Add(tourny.id);
                }
                else
                {
                    data.activeTournamentIDs.Add(-1); // Empty slot
                }
            }
        }
        
        // Dialogue Flags
        data.dialogueFlags = new DialogueFlagsData
        {
            coachDialogue = coachDialogue ?? new bool[0],
            qualDialogue = qualDialogue ?? new bool[0],
            reviewDialogue = reviewDialogue ?? new bool[0],
            introDialogue = introDialogue ?? new bool[0],
            helpDialogue = helpDialogue ?? new bool[0],
            strategyDialogue = strategyDialogue ?? new bool[0],
            storyDialogue = storyDialogue ?? new bool[0],
            storyBlockIndex = storyBlock
        };
        
        // Trophies
        if (allTimeTrophyList != null) data.allTimeTrophies.AddRange(allTimeTrophyList);
        if (currentTrophyList != null) data.currentSeasonTrophies.AddRange(currentTrophyList);
        
        return data;
    }
    
    /// <summary>
    /// Restores CareerManager state from CareerSaveData
    /// </summary>
    private void LoadFromSaveData(CareerSaveData data)
    {
        if (data == null)
        {
            Debug.LogError("[CareerManager] Cannot load from null save data");
            return;
        }
        
        // Player Identity
        playerName = data.playerName;
        teamName = data.teamName;
        teamColour = data.teamColour.ToColor();
        playerTeamIndex = data.playerTeamIndex;
        
        // Progress & Resources
        week = data.week;
        season = data.season;
        cash = data.cash;
        earnings = data.earnings;
        xp = data.xp;
        level = data.level;
        skillPoints = data.skillPoints;
        
        // Career Record
        record = new Vector2(data.careerWins, data.careerLosses);
        Debug.Log($"[CareerManager] Loaded career record from save: {record.x}-{record.y}");
        
        // Career Stats
        cStats.drawAccuracy = data.careerStats.drawAccuracy;
        cStats.guardAccuracy = data.careerStats.guardAccuracy;
        cStats.takeOutAccuracy = data.careerStats.takeOutAccuracy;
        cStats.sweepStrength = data.careerStats.sweepStrength;
        cStats.sweepEndurance = data.careerStats.sweepEndurance;
        cStats.sweepCohesion = data.careerStats.sweepCohesion;
        
        oppStats.drawAccuracy = data.oppStats.drawAccuracy;
        oppStats.guardAccuracy = data.oppStats.guardAccuracy;
        oppStats.takeOutAccuracy = data.oppStats.takeOutAccuracy;
        oppStats.sweepStrength = data.oppStats.sweepStrength;
        oppStats.sweepEndurance = data.oppStats.sweepEndurance;
        oppStats.sweepCohesion = data.oppStats.sweepCohesion;
        
        // Qualifications
        provQual = data.provQual;
        tourQual = data.tourQual;
        
        // Teams
        teams = new Team[data.teams.Count];
        for (int i = 0; i < data.teams.Count; i++)
        {
            teams[i] = DataToTeam(data.teams[i]);
        }
        
        tourTeams = new Team[data.tourTeams.Count];
        for (int i = 0; i < data.tourTeams.Count; i++)
        {
            tourTeams[i] = DataToTeam(data.tourTeams[i]);
        }
        
        // Team Records
        teamRecords = new Vector4[data.teamRecords.Count];
        for (int i = 0; i < data.teamRecords.Count; i++)
        {
            var record = data.teamRecords[i];
            teamRecords[i] = new Vector4(record.wins, record.losses, record.earnings, record.teamId);
        }
        
        tourRecords = new Vector4[data.tourRecords.Count];
        for (int i = 0; i < data.tourRecords.Count; i++)
        {
            var record = data.tourRecords[i];
            tourRecords[i] = new Vector4(record.wins, record.losses, record.earnings, record.teamId);
        }
        
        // Active Players
        activePlayers = new Player[data.activePlayers.Count];
        for (int i = 0; i < data.activePlayers.Count; i++)
        {
            activePlayers[i] = DataToPlayer(data.activePlayers[i]);
        }
        
        // Equipment
        activeEquipID = data.activeEquipIDs.ToArray();
        inventoryID = data.inventoryIDs.ToArray();
        
        // Restore equipment collections if available
        if (data.allEquipment != null)
        {
            EquipmentManager em = FindFirstObjectByType<EquipmentManager>();
            if (em != null)
            {
                // Restore handles
                if (data.allEquipment.handles != null && data.allEquipment.handles.Count > 0)
                {
                    em.handles = new Equipment[data.allEquipment.handles.Count];
                    for (int i = 0; i < data.allEquipment.handles.Count; i++)
                    {
                        em.handles[i] = DataToEquipment(data.allEquipment.handles[i]);
                    }
                }
                
                // Restore heads
                if (data.allEquipment.heads != null && data.allEquipment.heads.Count > 0)
                {
                    em.heads = new Equipment[data.allEquipment.heads.Count];
                    for (int i = 0; i < data.allEquipment.heads.Count; i++)
                    {
                        em.heads[i] = DataToEquipment(data.allEquipment.heads[i]);
                    }
                }
                
                // Restore footwear
                if (data.allEquipment.footwear != null && data.allEquipment.footwear.Count > 0)
                {
                    em.footwear = new Equipment[data.allEquipment.footwear.Count];
                    for (int i = 0; i < data.allEquipment.footwear.Count; i++)
                    {
                        em.footwear[i] = DataToEquipment(data.allEquipment.footwear[i]);
                    }
                }
                
                // Restore apparel
                if (data.allEquipment.apparel != null && data.allEquipment.apparel.Count > 0)
                {
                    em.apparel = new Equipment[data.allEquipment.apparel.Count];
                    for (int i = 0; i < data.allEquipment.apparel.Count; i++)
                    {
                        em.apparel[i] = DataToEquipment(data.allEquipment.apparel[i]);
                    }
                }
            }
        }
        
        // Cards/Sponsors
        cardPUIDList = data.cardPowerUpIDs.ToArray();
        cardSponsorIDList = data.cardSponsorIDs.ToArray();
        activeCardIDList = data.activeCardIDs.ToArray();
        playedCardIDList = data.playedCardIDs.ToArray();
        activeCardLengthList = data.activeCardLengths.ToArray();
        
        // Tournament Results
        tournyResults = new List<int>(data.tournamentResults);
        
        // Active Tournaments - will be restored by TournySelector.SetActiveTournies()
        activeTournies = new Tourny[data.activeTournamentIDs.Count];
        // Tournament arrays (tournies, tour, prov) are restored by TournySelector
        
        // Dialogue Flags
        coachDialogue = data.dialogueFlags.coachDialogue;
        qualDialogue = data.dialogueFlags.qualDialogue;
        reviewDialogue = data.dialogueFlags.reviewDialogue;
        introDialogue = data.dialogueFlags.introDialogue;
        helpDialogue = data.dialogueFlags.helpDialogue;
        strategyDialogue = data.dialogueFlags.strategyDialogue;
        storyDialogue = data.dialogueFlags.storyDialogue;
        storyBlock = data.dialogueFlags.storyBlockIndex;
        
        // Trophies
        allTimeTrophyList = new List<bool>(data.allTimeTrophies);
        currentTrophyList = new List<bool>(data.currentSeasonTrophies);
        
        loadedFromSave = true;
        
        Debug.Log($"[CareerManager] Loaded save from {data.saveDate}");
    }
    
    /// <summary>
    /// Converts Team to TeamData
    /// </summary>
    private TeamData TeamToData(Team team)
    {
        TeamData data = new TeamData
        {
            id = team.id,
            name = team.name,
            isPlayer = team.player,
            
            // Tournament state
            tournamentWins = team.tournamentWins,
            tournamentLosses = team.tournamentLosses,
            tournamentEarnings = team.tournamentEarnings,
            rank = team.rank,
            nextOpp = team.nextOpp,
            
            // Season cumulative
            seasonWins = team.seasonWins,
            seasonLosses = team.seasonLosses,
            seasonEarnings = team.seasonEarnings,
            
            // Tour
            tourPoints = team.tourPoints,
            
            // Skills
            strength = team.strength,
            draw = team.draw,
            guard = team.guard,
            takeOut = team.takeOut,
            sweepStrength = team.sweepStrength,
            sweepEnduro = team.sweepEnduro,
            sweepCohesion = team.sweepCohesion,
            
            // Legacy compatibility
            tourRecordX = team.tourRecord.x,
            tourRecordY = team.tourRecord.y
        };
        
        if (team.players != null)
        {
            foreach (var player in team.players)
            {
                data.players.Add(PlayerToData(player));
            }
        }
        
        return data;
    }
    
    /// <summary>
    /// Converts TeamData to Team
    /// </summary>
    public Team DataToTeam(TeamData data)
    {
        Team team = new Team
        {
            id = data.id,
            name = data.name,
            player = data.isPlayer,
            
            // Tournament state
            tournamentWins = data.tournamentWins,
            tournamentLosses = data.tournamentLosses,
            tournamentEarnings = data.tournamentEarnings,
            rank = data.rank,
            nextOpp = data.nextOpp ?? "",
            
            // Season cumulative
            seasonWins = data.seasonWins,
            seasonLosses = data.seasonLosses,
            seasonEarnings = data.seasonEarnings,
            
            // Tour
            tourPoints = data.tourPoints,
            
            // Skills
            strength = data.strength,
            draw = data.draw,
            guard = data.guard,
            takeOut = data.takeOut,
            sweepStrength = data.sweepStrength,
            sweepEnduro = data.sweepEnduro,
            sweepCohesion = data.sweepCohesion,
            
            // Legacy compatibility
            tourRecord = new Vector2(data.tourRecordX, data.tourRecordY),
            
            players = new List<Player>()
        };
        
        foreach (var playerData in data.players)
        {
            team.players.Add(DataToPlayer(playerData));
        }
        
        return team;
    }
    
    /// <summary>
    /// Converts Player to PlayerData
    /// </summary>
    private PlayerData PlayerToData(Player player)
    {
        return new PlayerData
        {
            id = player.id,
            name = player.name,
            draw = player.draw,
            guard = player.guard,
            takeOut = player.takeOut,
            sweepStrength = player.sweepStrength,
            sweepEnduro = player.sweepEnduro,
            sweepCohesion = player.sweepCohesion,
            oppDraw = player.oppDraw,
            oppGuard = player.oppGuard,
            oppTakeOut = player.oppTakeOut,
            oppStrength = player.oppStrength,
            oppEnduro = player.oppEnduro,
            oppCohesion = player.oppCohesion,
            cost = player.cost,
            description = player.description
            // Note: image (Sprite) will be restored from playerPool reference
        };
    }
    
    /// <summary>
    /// Converts PlayerData to Player, restoring image from playerPool by ID
    /// </summary>
    private Player DataToPlayer(PlayerData data)
    {
        Player player = new Player
        {
            id = data.id,
            name = data.name,
            draw = data.draw,
            guard = data.guard,
            takeOut = data.takeOut,
            sweepStrength = data.sweepStrength,
            sweepEnduro = data.sweepEnduro,
            sweepCohesion = data.sweepCohesion,
            oppDraw = data.oppDraw,
            oppGuard = data.oppGuard,
            oppTakeOut = data.oppTakeOut,
            oppStrength = data.oppStrength,
            oppEnduro = data.oppEnduro,
            oppCohesion = data.oppCohesion,
            cost = data.cost,
            description = data.description
        };
        
        // Restore image (Sprite) from playerPool by matching ID
        if (playerPool != null)
        {
            foreach (var poolPlayer in playerPool)
            {
                if (poolPlayer.id == data.id)
                {
                    player.image = poolPlayer.image;
                    //Debug.Log($"[CareerManager] Restored image for player {data.name} (ID: {data.id})");
                    break;
                }
            }
        }
        
        if (player.image == null)
        {
            Debug.LogWarning($"[CareerManager] Could not restore image for player {data.name} (ID: {data.id}) - not found in playerPool");
        }
        
        return player;
    }
    
    /// <summary>
    /// Captures current tournament state for saving
    /// </summary>
    private TournamentStateData CaptureTournamentState(GameSettingsPersist gsp)
    {
        var tournamentState = new TournamentStateData();
        
        // Find the active tournament manager
        object manager = FindFirstObjectByType<TournyManager>() ??
                        (object)FindFirstObjectByType<PlayoffManager>() ??
                        (object)FindFirstObjectByType<PlayoffManager_SingleK>() ??
                        (object)FindFirstObjectByType<PlayoffManager_TripleK>();
        
        // Save current tournament info
        if (currentTourny != null)
        {
            tournamentState.currentTournamentName = currentTourny.name;
            tournamentState.currentTournamentId = currentTourny.id;
            tournamentState.isTourEvent = currentTourny.tour;
            tournamentState.isQualifier = currentTourny.qualifier;
            tournamentState.isChampionship = currentTourny.championship;
            tournamentState.prizeMoney = currentTourny.prizeMoney;
            tournamentState.backgroundId = currentTourny.BG;
            tournamentState.crowdDensity = currentTourny.crowdDensity;
        }
        else
        {
            Debug.LogWarning("[CareerManager] currentTourny is null during CaptureTournamentState - using defaults");
            tournamentState.currentTournamentName = "Unknown Tournament";
            tournamentState.currentTournamentId = -1;
        }
        
        // Save manager-specific state
        if (manager is TournyManager tm)
        {
            tournamentState.managerType = "TournyManager";
            tournamentState.draw = tm.draw;
            tournamentState.numberOfTeams = tm.numberOfTeams;
            tournamentState.prize = tm.prize;
            tournamentState.oppTeam = tm.oppTeam;
            tournamentState.playoffRound = tm.playoffRound;
            tournamentState.games = gsp.games;  // CRITICAL: Save games parameter for DrawFormat!
            tournamentState.ends = gsp.ends;    // CRITICAL: Save ends for tournament settings!
            tournamentState.rocks = gsp.rocks;  // CRITICAL: Save rocks for tournament settings!
            
            if (tm.teams != null && tm.teams.Length > 0)
            {
                foreach (var team in tm.teams)
                {
                    if (team != null)
                    {
                        tournamentState.teams.Add(TeamToData(team));
                    }
                }
            }
        }
        else if (manager is PlayoffManager pm)
        {
            tournamentState.managerType = "PlayoffManager";
            tournamentState.oppTeam = pm.oppTeam;
            tournamentState.playoffRound = pm.playoffRound;
            
            if (pm.playoffTeams != null && pm.playoffTeams.Length > 0)
            {
                foreach (var team in pm.playoffTeams)
                {
                    if (team != null)
                    {
                        tournamentState.teams.Add(TeamToData(team));
                    }
                }
            }
        }
        else if (manager is PlayoffManager_SingleK pmSingle)
        {
            tournamentState.managerType = "PlayoffManager_SingleK";
            tournamentState.oppTeam = pmSingle.oppTeam;
            tournamentState.playoffRound = pmSingle.playoffRound;
            tournamentState.KO1 = true; // CRITICAL: Save Single-K flag!
            
            // CRITICAL FIX: Save playoff bracket to playoffTeams list (not regular teams list)
            if (pmSingle.playoffTeams != null && pmSingle.playoffTeams.Length > 0)
            {
                Debug.Log($"[CareerManager] Found pmSingle.playoffTeams with {pmSingle.playoffTeams.Length} slots");
                int nonNullCount = 0;
                foreach (var team in pmSingle.playoffTeams)
                {
                    if (team != null)
                    {
                        tournamentState.playoffTeams.Add(TeamToData(team));
                        nonNullCount++;
                    }
                }
                Debug.Log($"[CareerManager] Saved {nonNullCount}/{pmSingle.playoffTeams.Length} playoff bracket teams for Single-K");
            }
            else
            {
                Debug.LogWarning($"[CareerManager] pmSingle.playoffTeams is null or empty! Cannot save bracket.");
            }
        }
        else if (manager is PlayoffManager_TripleK pmTriple)
        {
            tournamentState.managerType = "PlayoffManager_TripleK";
            tournamentState.playoffRound = pmTriple.playoffRound;
            tournamentState.KO3 = true; // CRITICAL: Save Triple-K flag!
            
            
            if (pmTriple.teams != null && pmTriple.teams.Length > 0)
            {
                foreach (var team in pmTriple.teams)
                {
                    if (team != null)
                    {
                        tournamentState.teams.Add(TeamToData(team));
                    }
                }
            }
            
            if (pmTriple.gameList != null && pmTriple.gameList.Length > 0)
            {
                foreach (var game in pmTriple.gameList)
                {
                    tournamentState.gameList.Add(new Vector2Data(game));
                }
            }
        }
        
        // Save participating teams from currentTournyTeams as fallback
        if (currentTournyTeams != null && currentTournyTeams.Length > 0 && tournamentState.teams.Count == 0)
        {
            foreach (var team in currentTournyTeams)
            {
                if (team != null)
                {
                    tournamentState.teams.Add(TeamToData(team));
                }
            }
        }
        
        // Save draw and playoff round from GameSettingsPersist
        if (gsp != null)
        {
            tournamentState.draw = gsp.draw;
            tournamentState.playoffRound = gsp.playoffRound;
            
            // CRITICAL FIX: Also save games, ends, rocks when no manager exists!
            // This happens when SaveCareer() is called from TournySettings.LoadToGSP()
            // BEFORE the tournament home scene loads (no TournyManager/PlayoffManager exists yet)
            tournamentState.games = gsp.games;
            tournamentState.ends = gsp.ends;
            tournamentState.rocks = gsp.rocks;
            
            // CRITICAL FIX: Also save KO1/KO3 flags from GSP!
            // These are set when entering a tournament and must be preserved
            tournamentState.KO1 = gsp.KO1;
            tournamentState.KO3 = gsp.KO3;
            
            Debug.Log($"[CareerManager] No tournament manager found - saved from gsp: games={gsp.games}, ends={gsp.ends}, rocks={gsp.rocks}, KO1={gsp.KO1}, KO3={gsp.KO3}");
        }
        
        Debug.Log($"[CareerManager] Captured tournament state: {tournamentState.managerType}, {tournamentState.teams.Count} teams");
        
        return tournamentState;
    }
    
    /// <summary>
    /// Restores tournament state from save data
    /// </summary>
    private void RestoreTournamentState(TournamentStateData tournamentState, GameSettingsPersist gsp)
    {
        if (tournamentState == null)
        {
            Debug.LogWarning("[CareerManager] No tournament state to restore");
            return;
        }
        
        Debug.Log($"[CareerManager] Restoring tournament state: {tournamentState.managerType}");
        
        // Restore current tournament info
        if (currentTourny == null)
        {
            currentTourny = new Tourny();
        }
        
        currentTourny.name = tournamentState.currentTournamentName;
        currentTourny.id = tournamentState.currentTournamentId;
        currentTourny.tour = tournamentState.isTourEvent;
        currentTourny.qualifier = tournamentState.isQualifier;
        currentTourny.championship = tournamentState.isChampionship;
        currentTourny.prizeMoney = tournamentState.prizeMoney;
        currentTourny.BG = tournamentState.backgroundId;
        currentTourny.crowdDensity = tournamentState.crowdDensity;
        
        // Restore tournament teams
        if (tournamentState.teams != null && tournamentState.teams.Count > 0)
        {
            currentTournyTeams = new Team[tournamentState.teams.Count];
            for (int i = 0; i < tournamentState.teams.Count; i++)
            {
                currentTournyTeams[i] = DataToTeam(tournamentState.teams[i]);
            }
        }
        
        // CRITICAL FIX: Restore playoff bracket teams for Single-K/Triple-K
        Team[] restoredPlayoffTeams = null;
        if (tournamentState.playoffTeams != null && tournamentState.playoffTeams.Count > 0)
        {
            restoredPlayoffTeams = new Team[tournamentState.playoffTeams.Count];
            for (int i = 0; i < tournamentState.playoffTeams.Count; i++)
            {
                restoredPlayoffTeams[i] = DataToTeam(tournamentState.playoffTeams[i]);
            }
            Debug.Log($"[CareerManager] Restored {restoredPlayoffTeams.Length} playoff bracket teams");
        }
        
        // Restore GameSettingsPersist values
        if (gsp != null)
        {
            gsp.KO3 = tournamentState.KO3; // CRITICAL: Restore Triple-K flag from save!
            gsp.KO1 = tournamentState.KO1; // CRITICAL: Restore Single-K flag from save!
            gsp.draw = tournamentState.draw;
            gsp.playoffRound = tournamentState.playoffRound;
            gsp.games = tournamentState.games;  // CRITICAL: Restore games parameter!
            gsp.ends = tournamentState.ends;    // CRITICAL: Restore ends for tournament!
            gsp.rocks = tournamentState.rocks;  // CRITICAL: Restore rocks for tournament!
            gsp.teams = currentTournyTeams;
            gsp.playoffTeams = restoredPlayoffTeams;  // CRITICAL: Restore playoff bracket!
            
            Debug.Log($"[CareerManager] Restored tournament settings - games={gsp.games}, ends={gsp.ends}, rocks={gsp.rocks}, playoffTeams={gsp.playoffTeams?.Length ?? 0}");
        }
        
        Debug.Log($"[CareerManager] Tournament state restored: {currentTournyTeams?.Length ?? 0} teams, playoff round {tournamentState.playoffRound}");
    }
    
    /// <summary>
    /// Restores game state from save data (for mid-game resume)
    /// </summary>
    private void RestoreGameState(GameStateData gameState, GameSettingsPersist gsp)
    {
        if (gameState == null || gsp == null)
        {
            Debug.LogWarning("[CareerManager] Cannot restore game state: null data");
            return;
        }
        
        Debug.Log("[CareerManager] Restoring game state...");
        
        // Restore game progress flags
        gsp.gameInProgress = gameState.gameInProgress;
        gsp.tournyInProgress = gameState.tournyInProgress;
        gsp.tourny = gameState.isTournyGame;
        gsp.loadGame = true; // Signal to GameManager to load the saved game
        
        // Restore game settings
        gsp.ends = gameState.ends;
        gsp.endCurrent = gameState.currentEnd;
        gsp.rocks = gameState.rocks;
        gsp.rockCurrent = gameState.currentRock;
        gsp.redHammer = gameState.redHammer;
        gsp.aiYellow = gameState.aiYellow;
        gsp.aiRed = gameState.aiRed;
        
        // Restore team names and scores
        gsp.yellowTeamName = gameState.yellowTeamName;
        gsp.redTeamName = gameState.redTeamName;
        gsp.yellowScore = gameState.yellowScore;
        gsp.redScore = gameState.redScore;
        
        // Restore rock positions
        if (gameState.rockPositions != null && gameState.rockPositions.Count > 0)
        {
            gsp.rockPos = new Vector2[gameState.rockPositions.Count];
            for (int i = 0; i < gameState.rockPositions.Count; i++)
            {
                gsp.rockPos[i] = gameState.rockPositions[i].ToVector2();
            }
        }
        
        // Restore rock in-play status
        if (gameState.rockInPlay != null && gameState.rockInPlay.Count > 0)
        {
            gsp.rockInPlay = gameState.rockInPlay.ToArray();
        }
        
        // Restore end scores
        if (gameState.endScores != null && gameState.endScores.Count > 0)
        {
            gsp.score = new Vector2Int[gameState.endScores.Count];
            for (int i = 0; i < gameState.endScores.Count; i++)
            {
                gsp.score[i] = gameState.endScores[i].ToVector2Int();
            }
        }
        
        Debug.Log($"[CareerManager] Game state restored: End {gameState.currentEnd}/{gameState.ends}, Rock {gameState.currentRock}/{gameState.rocks}");
    }
    
    /// <summary>
    /// Converts Equipment to EquipmentData
    /// </summary>
    private EquipmentData EquipmentToData(Equipment equipment)
    {
        EquipmentData data = new EquipmentData
        {
            id = equipment.id,
            cost = equipment.cost,
            color = new SerializableColor(equipment.color),
            duration = equipment.duration,
            stats = equipment.stats != null ? (int[])equipment.stats.Clone() : new int[6],
            oppStats = equipment.oppStats != null ? (int[])equipment.oppStats.Clone() : new int[6]
        };
        
        return data;
    }
    
    /// <summary>
    /// Converts EquipmentData to Equipment
    /// </summary>
    private Equipment DataToEquipment(EquipmentData data)
    {
        Equipment equipment = new Equipment
        {
            id = data.id,
            cost = data.cost,
            color = data.color.ToColor(),
            duration = data.duration,
            stats = data.stats != null ? (int[])data.stats.Clone() : new int[6],
            oppStats = data.oppStats != null ? (int[])data.oppStats.Clone() : new int[6]
        };
        
        // The name, img, text, and type flags (handle, head, footwear, apparel) 
        // will be set by EquipmentManager based on id and cost when loading
        
        return equipment;
    }
    
    #endregion
}
