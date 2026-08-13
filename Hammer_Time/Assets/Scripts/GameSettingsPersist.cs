using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TigerForge;
using System;
using Random = UnityEngine.Random;

public class GameSettingsPersist : MonoBehaviour
{
    GameSettings gs;
    StoryManager sm;
    TournyManager tm;
    TeamManager teamM;

    public int bg;
    public int crowdDensity;
    public int tournyType;

    public bool redHammer;
    public int ends;
    public int rocks;
    public float volume;
    public bool tutorial;
    public bool loadGame;

    public bool aiYellow;
    public bool aiRed;
    public bool debug;
    public bool mixed;
    public int rockCurrent;
    public int endCurrent;
    public int yellowScore;
    public int redScore;
    public Vector2[] rockPos;
    public bool[] rockInPlay;

    public bool tourny;
    public bool KO3;
    public bool KO1;
    public bool cashGame;
    public int games;

    public float tournyEarnings;
    public float tournyCash;

    public Vector2 tournyRecord;

    public CareerStats cStats;
    public CareerStats oppStats;

    public string redTeamName;
    public Color redTeamColour;
    public Team redTeam;
    public string yellowTeamName;
    public Color yellowTeamColour;
    public Team yellowTeam;

    public int draw;
    public int playoffRound;
    public int numberOfTeams;
    public int prize;
    public bool careerLoad;
    public bool tournyInProgress;
    public bool gameInProgress;
    public bool justFinishedGame;  // NEW: Flag to indicate a game just finished (vs loading between games)
    public bool inEndMenu;  // NEW: Flag to indicate we're viewing end menu (for save/load to end menu)
    public bool tournyIntroShown;  // NEW: Flag to track if tournament intro has been shown for first played game

    public List<Team_List> teamList;
    public Team[] teams;
    public Team[] playoffTeams;
    public CashGamePlayers[] cgp;
    public int playerTeamIndex;
    private Vector2Int[] _score;
    public Vector2Int[] score
    {
        get { return _score; }
        set
        {
            if (value != _score)
            {
                var stackTrace = new System.Diagnostics.StackTrace(1, true);
                var caller = stackTrace.GetFrame(0);
                Debug.Log($"[GSP.score] ARRAY REPLACED by {caller.GetMethod().DeclaringType?.Name}.{caller.GetMethod().Name} at line {caller.GetFileLineNumber()}");
                if (_score != null && _score.Length > 0)
                {
                    Debug.Log($"[GSP.score]   OLD array: [{_score[0].x},{_score[0].y}]" + (value != null && value.Length > 0 ? $", NEW array: [{value[0].x},{value[0].y}]" : ", NEW array: NULL/EMPTY"));
                }
            }
            _score = value;
        }
    }

    public bool skinsGame;
    public int skins;
    public float[] skinValue;

    public Team playerTeam;
    public Color teamColour;
    public TeamMember[] playerGO;
    EasyFileSave myFile;

    public static GameSettingsPersist instance;


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

        Application.targetFrameRate = 30;

    }

    private void Start()
    {
        //myFile = new EasyFileSave("my_game_data");

        //gs = GameObject.Find("GameSettings").GetComponent<GameSettings>();

        if (tutorial)
        {
            OnTutorial();
        }

        teamM = FindFirstObjectByType<TeamManager>();

    }

    public void LoadSettings()
    {
        gs = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        //load all the saved values
        ends = gs.ends;
        endCurrent = 0;
        rocks = gs.rocks;
        rockCurrent = 0;
        redScore = 0;
        yellowScore = 0;
        inEndMenu = false;  // Clear end menu flag - starting new game
        redHammer = gs.redHammer;
        aiYellow = gs.aiYellow;
        aiRed = gs.aiRed;
        mixed = gs.mixed;
        //skip = gs.team;
        debug = gs.debug;
        yellowTeamName = gs.yellowTeamName;
        redTeamName = gs.redTeamName;
        redTeamColour = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f));
        yellowTeamColour = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f));
    }

    public void LoadGame()
    {
        Debug.Log("Load Game GSP");
        loadGame = true;
        myFile = new EasyFileSave("my_game_data");
        //load all the saved values
        if (myFile.Load())
        {
            Debug.Log("Loading to GSP");
            Debug.Log("Ends is " + myFile.GetInt("End Total"));

            bg = myFile.GetInt("Current Tourny BG");
            crowdDensity = myFile.GetInt("Current Tourny Crowd Density");
            ends = myFile.GetInt("End Total");
            endCurrent = myFile.GetInt("Current End");
            rocks = myFile.GetInt("Rocks Per Team");
            rockCurrent = myFile.GetInt("Current Rock");
            redHammer = myFile.GetBool("Red Hammer");
            aiRed = myFile.GetBool("Ai Red");
            aiYellow = myFile.GetBool("Ai Yellow");
            mixed = myFile.GetBool("Mixed");
            //skip = myFile.GetBool("Team");
            debug = myFile.GetBool("Debug");

            redScore = myFile.GetInt("Red Score");
            yellowScore = myFile.GetInt("Yellow Score");
            Debug.Log("ends is " + ends);
            score = new Vector2Int[ends + 1];
            Debug.Log("score length is " + score.Length);
            int[] redScoreList = myFile.GetArray<int>("Red Score List");
            int[] yellowScoreList = myFile.GetArray<int>("Yellow Score List");

            for (int i = 0; i < score.Length; i++)
            {
                score[i] = new Vector2Int(redScoreList[i], yellowScoreList[i]);
                Debug.Log("Score " + i + " - " + score[i].x + ", " + score[i].y);
            }
        }
    }

    public void LoadFromGM()
    {
        Debug.Log("Load From GM  GSP");
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        Debug.Log("Loading to GSP");
        
        // Sync data from GameManager
        ends = gm.endTotal;
        endCurrent = gm.endCurrent;
        rocks = gm.rocksPerTeam;
        rockCurrent = gm.rockCurrent;
        redScore = gm.redScore;
        yellowScore = gm.yellowScore;
        redHammer = gm.redHammer;
        aiYellow = gm.aiTeamYellow;
        aiRed = gm.aiTeamRed;
        
        // ? CRITICAL FIX: DON'T resize the score array here!
        // GameManager.Scoring() already resized it if needed AND saved the scores
        // Resizing here would create a NEW array and lose the scores we just saved
        // Just validate it exists
        if (score == null)
        {
            Debug.LogError($"[GSP.LoadFromGM] CRITICAL: Score array is NULL! This should never happen!");
            score = new Vector2Int[ends];
            for (int i = 0; i < ends; i++)
            {
                score[i] = new Vector2Int(0, 0);
            }
        }
        
        Debug.Log($"[GSP.LoadFromGM] Synced from GameManager - endCurrent: {endCurrent}, redScore: {redScore}, yellowScore: {yellowScore}");
        
        // Log all end scores for debugging
        if (score != null && endCurrent > 0)
        {
            Debug.Log($"[GSP.LoadFromGM] Score array has {score.Length} ends, showing first {endCurrent} completed:");
            for (int i = 0; i < Mathf.Min(endCurrent, score.Length); i++)
            {
                Debug.Log($"  End {i + 1}: Red {score[i].x}, Yellow {score[i].y}");
            }
        }
    }

    public void LoadFromEndMenu()
    {
        Debug.Log("Load From EM  GSP");

        Debug.Log("Loading to GSP");
        //Debug.Log("Ends is " + myFile.GetInt("End Total"));
        
        //third = gm.target;
        //skip = gm.target;

        //score[endCurrent] = new Vector2Int(redScore, yellowScore);
        //redScore = myFile.GetInt("Red Score");
        //yellowScore = myFile.GetInt("Yellow Score");
        if (ends <= endCurrent)
        {
            if (redScore > yellowScore)
            {
                for (int i = 0; i < teams.Length; i++)
                {
                    if (teams[i].name == redTeamName)
                    {
                        teams[i].wins++;
                        if (KO3)
                            teams[i].tourRecord.x++;
                    }
                    if (teams[i].name == yellowTeamName)
                    {
                        teams[i].loss++;
                        if (KO3)
                            teams[i].tourRecord.y++;
                    }
                }
            }
            else
            {
                for (int i = 0; i < teams.Length; i++)
                {
                    if (teams[i].name == yellowTeamName)
                    {
                        teams[i].wins++;
                        if (KO3)
                            teams[i].tourRecord.x++;
                    }
                    if (teams[i].name == redTeamName)
                    {
                        teams[i].loss++;
                        if (KO3)
                            teams[i].tourRecord.y++;
                    }
                }
            }
        }

    }

    public void LoadFromTournySelector()
    {
        TournySelector ts = FindFirstObjectByType<TournySelector>();
        CareerManager cm = FindFirstObjectByType<CareerManager>();

        Debug.Log("Loading Tourny Settings to GSP");
        //Debug.Log("Ends is " + myFile.GetInt("End Total"));
        teamColour = cm.teamColour;
        tournyEarnings = 0;
        tournyCash = 0;
        bg = cm.currentTourny.BG;
        crowdDensity = cm.currentTourny.crowdDensity;
        //if (cm.currentTourny.championship)
        //    bg = 3;
        //else
        //    bg = Random.Range(0, 3);

        playerTeamIndex = cm.playerTeamIndex;

        for (int i = 0; i < cm.currentTourny.teams; i++)
        {
            if (cm.currentTournyTeams[i].id == cm.playerTeamIndex)
            {
                playerTeam = cm.currentTournyTeams[i];
            }
        }

        Debug.Log("Player Team Index in GSP is " + playerTeamIndex);

        endCurrent = 0;
        inEndMenu = false;  // Clear end menu flag - starting new tournament
        numberOfTeams = ts.currentTourny.teams;
        prize = ts.currentTourny.prizeMoney;
        teams = cm.currentTournyTeams;
        draw = 0;
        playoffRound = 0;
        //careerLoad = true;
        
        //redScore = myFile.GetInt("Red Score");
        //yellowScore = myFile.GetInt("Yellow Score");
    }

    public void LoadTournySettings(TournySettings ts)
    {
        Debug.Log($"[GSP.LoadTournySettings] BEFORE: games={games}, ts.games={ts.games}");

        CareerManager cm = FindFirstObjectByType<CareerManager>();

        Debug.Log("Loading Tourny Settings to GSP");
        //Debug.Log("Ends is " + myFile.GetInt("End Total"));
        if (cm != null) teamColour = cm.teamColour;
        //earnings = ts.earnings;
        games = ts.games;
        // Ensure games is valid — if TournySettings couldn't set it (e.g. slider not yet active),
        // calculate a sensible default so DrawSelector never receives 0.
        if (games <= 0)
        {
            games = ts.teams > 12 ? 5 : ts.teams > 8 ? 4 : 3;
            Debug.LogWarning($"[GSP.LoadTournySettings] ts.games was {ts.games} — defaulted to {games} for {ts.teams} teams");
        }
        if (cashGame)
            ends = 1;
        else
            ends = ts.ends;
        endCurrent = 0;
        inEndMenu = false;  // Clear end menu flag - starting new tournament
        rocks = ts.rocks;
        //numberOfTeams = ts.teams;
        prize = ts.prize;
        draw = 0;
        playoffRound = 0;
        
        Debug.Log($"[GSP.LoadTournySettings] AFTER: games={games}, ends={ends}, rocks={rocks}");
        Debug.Log($"[GSP.LoadTournySettings] Calling cm.SaveCareer() to persist tournament settings...");

        //redScore = myFile.GetInt("Red Score");
        //yellowScore = myFile.GetInt("Yellow Score");
        if (cm != null) cm.SaveCareer(this);

        Debug.Log($"[GSP.LoadTournySettings] Save complete - games={games} should now be in save file");
    }

    public void TournySetup(int btn = 0, bool forceNewGame = false)
    {
        // ? FIRST LOG - Before anything else!
        Debug.Log($"[GSP.TournySetup] === ENTRY === btn={btn}, forceNewGame={forceNewGame}");
        
        // ? Log current state IMMEDIATELY
        if (score != null && score.Length > 0)
        {
            Debug.Log($"[GSP.TournySetup] ENTRY score array: [{score[0].x},{score[0].y}], [{(score.Length > 1 ? score[1].x : -1)},{(score.Length > 1 ? score[1].y : -1)}], endCurrent={endCurrent}, redScore={redScore}");
        }
        else
        {
            Debug.Log($"[GSP.TournySetup] ENTRY score array is NULL or empty, endCurrent={endCurrent}, redScore={redScore}");
        }
        
        Debug.Log("[GSP] TournySetup called");
        TournyManager tm = FindFirstObjectByType<TournyManager>();
        PlayoffManager pm = FindFirstObjectByType<PlayoffManager>();
        PlayoffManager_SingleK pm1k = FindFirstObjectByType<PlayoffManager_SingleK>();
        CareerManager cm = FindFirstObjectByType<CareerManager>();
        CashGames cg = FindFirstObjectByType<CashGames>();
        
        // ? CRITICAL FIX: Determine if this is a NEW game or CONTINUING an existing game
        // Priority order:
        // 1. If forceNewGame=true ? NEW game (called from TournyManager.PlayDraw)
        // 2. If endCurrent == 0 AND scores == 0 ? NEW game
        // 3. If endCurrent > 0 OR scores > 0 ? CONTINUING game
        bool isContinuingGame = !forceNewGame && ((endCurrent > 0) || (redScore > 0) || (yellowScore > 0));
        
        Debug.Log($"[GSP.TournySetup] isContinuingGame={isContinuingGame} (forceNewGame={forceNewGame}, endCurrent={endCurrent}, redScore={redScore}, yellowScore={yellowScore})");
        
        // Always reset these flags (they control scene flow, not game state)
        gameInProgress = false;
        loadGame = false;
        
        if (isContinuingGame)
        {
            // CONTINUING an existing game - preserve all game state
            Debug.Log($"[GSP.TournySetup] CONTINUING game - preserving scores: endCurrent={endCurrent}, redScore={redScore}, yellowScore={yellowScore}");
            Debug.Log($"[GSP.TournySetup]   Score array: End1=({(score != null && score.Length > 0 ? score[0].x : -1)},{(score != null && score.Length > 0 ? score[0].y : -1)}), End2=({(score != null && score.Length > 1 ? score[1].x : -1)},{(score != null && score.Length > 1 ? score[1].y : -1)})");
        }
        else
        {
            // NEW game - reset all game state
            rockCurrent = 0;
            endCurrent = 0;
            redScore = 0;
            yellowScore = 0;
            inEndMenu = false;  // Clear end menu flag - only save file should set this
            
            // CRITICAL: Also clear the score array for the new game!
            // This ensures old scores don't leak into the new game
            if (score != null && score.Length > 0)
            {
                Debug.Log($"[GSP.TournySetup] NEW game - clearing score array ({score.Length} ends) - was [{score[0].x},{score[0].y}]");
                for (int i = 0; i < score.Length; i++)
                {
                    score[i] = new Vector2Int(0, 0);
                }
            }
            else
            {
                Debug.Log($"[GSP.TournySetup] NEW game - score array will be created in GameManager.SetupGame()");
            }
            
            Debug.Log($"[GSP.TournySetup] NEW game - reset game state: endCurrent=0, redScore=0, yellowScore=0, inEndMenu=false");
        }
        
        // ? Handle score array initialization
        if (score == null)
        {
            // Brand new game - create fresh array
            score = new Vector2Int[ends];
            for (int i = 0; i < ends; i++)
            {
                score[i] = new Vector2Int(0, 0);
            }
            Debug.Log($"[GSP.TournySetup] Created new score array for {ends} ends");
        }
        else if (score.Length < ends)
        {
            // Array exists but too small - expand while preserving
            Debug.Log($"[GSP.TournySetup] Expanding score array from {score.Length} to {ends} ends (preserving existing scores)");
            Vector2Int[] newScore = new Vector2Int[ends];
            for (int i = 0; i < score.Length; i++)
            {
                newScore[i] = score[i];  // Preserve existing scores
            }
            for (int i = score.Length; i < ends; i++)
            {
                newScore[i] = new Vector2Int(0, 0);  // Initialize new slots
            }
            score = newScore;
        }
        else if (isContinuingGame)
        {
            // Continuing existing game - preserve array as-is
            Debug.Log($"[GSP.TournySetup] CONTINUING game - score array ({score.Length} ends) preserved");
            
            // Log current scores for debugging
            if (score.Length > 0 && endCurrent > 0 && endCurrent <= score.Length)
            {
                for (int i = 0; i < endCurrent; i++)
                {
                    Debug.Log($"[GSP.TournySetup]   End {i + 1}: Red={score[i].x}, Yellow={score[i].y}");
                }
            }
        }
        else
        {
            // NEW game - clear the array
            Debug.Log($"[GSP.TournySetup] NEW game - clearing score array ({score.Length} ends)");
            for (int i = 0; i < score.Length; i++)
            {
                score[i] = new Vector2Int(0, 0);
            }
        }
        
        Debug.Log($"[GSP] TournySetup - game state configured (isContinuingGame={isContinuingGame})");
        Debug.Log($"[GSP] TournySetup - tournament settings: games={games}, ends={ends}, rocks={rocks}");
        
        careerLoad = false;
        if (cg != null)
        {
            tourny = false;
            draw = 0;
            playoffRound = 0;
            //playerTeam = teams[playerTeamIndex];
            // ? DON'T reset game progress - GameManager handles this!
            // endCurrent = 0;
            // redScore = 0;
            // yellowScore = 0;

            playerTeam = teams[0];
            playerTeam.nextOpp = teams[btn + 1].name;
            // ? DON'T reset game progress - GameManager handles this!
            // endCurrent = 0;
            // redScore = 0;
            // yellowScore = 0;
            gameInProgress = true;
            //playerGO = tm.playerGO;
        }
        else
        {
            if (pm != null)
            {
                tourny = true;
                draw = tm.draw;
                playoffRound = pm.playoffRound;

                if (playoffRound > 1)
                    playerTeamIndex = pm.playerTeam;
                else
                    playerTeamIndex = tm.playerTeam;
            }
            else if (pm1k != null)
            {
                playoffRound = pm1k.playoffRound;
                KO1 = true;
            }
            teamList = tm.teamList;
            teams = tm.teams;
            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i].player)
                {
                    playerTeam = teams[i];
                }
            }
            // ? DON'T reset game progress - GameManager handles this!
            // endCurrent = 0;
            // redScore = 0;
            // yellowScore = 0;
            tournyInProgress = true;

            Debug.Log("gsp.inProgress is " + tournyInProgress);

            //playerGO = tm.playerGO;
            if (pm != null)
            {
                if (draw >= tm.drawFormat.Length)
                {
                    playoffTeams = new Team[9];
                    for (int i = 0; i < playoffTeams.Length; i++)
                    {
                        playoffTeams[i] = pm.playoffTeams[i];
                    }
                    //playoffTeams = pm.playoffTeams;
                }
            }
        }

        Debug.Log($"[GSP.TournySetup] Assigning teams — teams.Length={teams?.Length ?? 0}");
        for (int dbg = 0; dbg < (teams?.Length ?? 0); dbg++)
            Debug.Log($"[GSP.TournySetup]   teams[{dbg}] = {teams[dbg]?.name ?? "NULL"} (player={teams[dbg]?.player}, nextOpp={teams[dbg]?.nextOpp})");

        if (Random.Range(0f, 1f) < 0.5f)
        {
            aiYellow = true;
            aiRed = false;

            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i].player)
                    redTeam = teams[i];
            }

            if (redTeam == null)
            {
                Debug.LogError("[GSP.TournySetup] redTeam is null — no team has player=true! Falling back to teams[0]");
                redTeam = teams[0];
            }
            redTeamColour = teamColour;
            redTeamName = redTeam.name;

            for (int i = 0; i < teams.Length; i++)
            {
                if (redTeam.nextOpp == teams[i].name)
                    yellowTeam = teams[i];
            }

            if (yellowTeam == null)
            {
                Debug.LogError($"[GSP.TournySetup] yellowTeam is null — nextOpp '{redTeam.nextOpp}' not found in teams array! Falling back to first non-player team");
                for (int i = 0; i < teams.Length; i++)
                    if (!teams[i].player) { yellowTeam = teams[i]; break; }
            }

            yellowTeamName = yellowTeam?.name ?? "Unknown";
            yellowTeamColour = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f));

        }
        else
        {
            aiRed = true;
            aiYellow = false;

            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i].player)
                    yellowTeam = teams[i];
            }

            if (yellowTeam == null)
            {
                Debug.LogError("[GSP.TournySetup] yellowTeam is null — no team has player=true! Falling back to teams[0]");
                yellowTeam = teams[0];
            }
            yellowTeamColour = teamColour;
            yellowTeamName = yellowTeam.name;

            for (int i = 0; i < teams.Length; i++)
            {
                if (yellowTeam.nextOpp == teams[i].name)
                    redTeam = teams[i];
            }

            if (redTeam == null)
            {
                Debug.LogError($"[GSP.TournySetup] redTeam is null — nextOpp '{yellowTeam.nextOpp}' not found in teams array! Falling back to first non-player team");
                for (int i = 0; i < teams.Length; i++)
                    if (!teams[i].player) { redTeam = teams[i]; break; }
            }

            redTeamName = redTeam?.name ?? "Unknown";
            redTeamColour = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f));

        }

        Debug.Log($"[GSP.TournySetup] *** FINAL team names set: red='{redTeamName}' yellow='{yellowTeamName}'");

        if (Random.Range(0f, 1f) < 0.5f)
        {
            redHammer = true;
        }
        else
        {
            redHammer = false;
        }
    }

    public void TournyKOSetup()
    {
        Debug.Log("Tourny KO Setup GSP");
        PlayoffManager_TripleK pm = FindFirstObjectByType<PlayoffManager_TripleK>();
        careerLoad = false;
        tourny = true;
        draw = 0;
        playoffRound = pm.playoffRound;
        KO3 = true;
        playerTeamIndex = pm.playerTeam;
        
        teams = pm.teams;
        for (int i = 0; i < teams.Length; i++)
        {
            if (teams[i].player)
            {
                playerTeam = teams[i];
            }
            if (teams[i].id == pm.oppTeam)
            {
                playerTeam.nextOpp = teams[i].name;
            }
        }
        endCurrent = 0;
        redScore = 0;
        yellowScore = 0;
        inEndMenu = false;  // Clear end menu flag - starting new playoff game

        //Debug.Log("Loading Tourny Settings to GSP");
        //playerGO = tm.playerGO;

        if (Random.Range(0f, 1f) < 0.5f)
        {
            aiYellow = true;
            aiRed = false;

            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i].player)
                    redTeam = teams[i];
            }
            redTeamColour = teamColour;
            redTeamName = redTeam.name;

            for (int i = 0; i < teams.Length; i++)
            {
                if (redTeam.nextOpp == teams[i].name)
                    yellowTeam = teams[i];
            }
            yellowTeamName = yellowTeam.name;
            yellowTeamColour = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f));
        }
        else
        {
            aiRed = true;
            aiYellow = false;

            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i].player)
                    yellowTeam = teams[i];
            }
            yellowTeamColour = teamColour;
            yellowTeamName = yellowTeam.name;

            for (int i = 0; i < teams.Length; i++)
            {
                if (yellowTeam.nextOpp == teams[i].name)
                    redTeam = teams[i];
            }
            redTeamName = redTeam.name;
            redTeamColour = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f));
        }

        if (Random.Range(0f, 1f) < 0.5f)
        {
            redHammer = true;
        }
        else
        {
            redHammer = false;
        }
    }

    public void LoadCareer()
    {
        Debug.Log("[GameSettingsPersist] LoadCareer called - delegating to CareerManager");
        CareerManager cm = FindFirstObjectByType<CareerManager>();
        
        if (cm == null)
        {
            Debug.LogError("[GameSettingsPersist] CareerManager not found!");
            return;
        }
        
        // Just sync color, don't trigger full load (CareerManager handles loading)
        teamColour = cm.teamColour;
        
        // Don't call cm.LoadCareer() here - it creates circular dependency
        // CareerManager will load itself in its Start() method
    }

    public void LoadTourny()
    {
        Debug.Log($"[GameSettingsPersist] LoadTourny called — tournyInProgress={tournyInProgress}, ends={ends}, rocks={rocks}, red='{redTeamName}', yellow='{yellowTeamName}'");
        CareerManager cm = FindFirstObjectByType<CareerManager>();

        // CRITICAL FIX: Don't reload career if tournament is already in progress
        // The flags (justFinishedGame, tournyInProgress, etc.) are already set correctly in memory
        // Reloading would overwrite them with stale data
        if (!tournyInProgress)
        {
            Debug.Log("[GameSettingsPersist] tournyInProgress=false — running recovery load for tournament setup");

            // Capture the player's in-memory selections BEFORE LoadCareer can overwrite them.
            // RestoreGameState uses old save data (e.g. ends=8 from a previous game's autosave),
            // which would clobber what the player just selected in TournySettings.
            int preLoadEnds  = ends;
            int preLoadRocks = rocks;
            string preLoadRedName    = redTeamName;
            string preLoadYellowName = yellowTeamName;
            Debug.Log($"[LoadTourny] Recovery: captured pre-load ends={preLoadEnds}, rocks={preLoadRocks}, red='{preLoadRedName}', yellow='{preLoadYellowName}'");

            cm.LoadCareer(this);

            // RECOVERY: tournyInProgress was false in memory, so LoadCareer's RestoreTournamentState
            // was skipped (it only runs when the save has tournyInProgress=true). Rocks and team names
            // must be fixed from tournament state or career teams.

            // 1. Restore the player's actual selection if it was valid. RestoreGameState may have
            //    overwritten ends/rocks with stale save data (e.g. a previous game's autosave).
            //    The pre-load in-memory value is the authoritative player choice.
            CareerSaveData saveData = CareerSaveService.LoadCareer();

            if (preLoadRocks > 0 && preLoadRocks <= 8)
            {
                if (rocks != preLoadRocks)
                    Debug.Log($"[LoadTourny] Recovery: rocks={preLoadRocks} from pre-load selection (RestoreGameState had set it to {rocks})");
                rocks = preLoadRocks;
            }
            else if (rocks <= 0 || rocks > 8)
            {
                Debug.Log($"[LoadTourny] Recovery: reset rocks=8 (was {rocks}, invalid)");
                rocks = 8;
            }

            if (preLoadEnds > 0 && preLoadEnds <= 10)
            {
                if (ends != preLoadEnds)
                    Debug.Log($"[LoadTourny] Recovery: ends={preLoadEnds} from pre-load selection (RestoreGameState had set it to {ends})");
                ends = preLoadEnds;
            }
            else if (ends <= 0 || ends > 10)
            {
                Debug.Log($"[LoadTourny] Recovery: reset ends=8 (was {ends}, invalid)");
                ends = 8;
            }

            // Recalculate rockCurrent for the corrected rocks value now that rocks is confirmed.
            // (The earlier rockCurrent correction below will also run, but capture rocks is needed first.)

            // Read tournament state to double-check saved rocks/ends (e.g. if pre-load was also stale).
            int savedTournyRocks = saveData?.currentTournamentState != null ? saveData.currentTournamentState.rocks : 0;
            if (savedTournyRocks > 0 && savedTournyRocks <= 8 && preLoadRocks <= 0)
            {
                // Only use tournament state as fallback when pre-load was also invalid
                Debug.Log($"[LoadTourny] Recovery: rocks={savedTournyRocks} from tournament state (pre-load was invalid)");
                rocks = savedTournyRocks;
            }

            // Tournament state ends: only use as fallback when pre-load was also invalid.
            // If pre-load had a valid selection, we already applied it above — don't let stale
            // tournament state (e.g. from a previous game's autosave) override the player's choice.
            int savedTournyEnds = saveData?.currentTournamentState != null ? saveData.currentTournamentState.ends : 0;
            if (savedTournyEnds > 0 && savedTournyEnds <= 10 && preLoadEnds <= 0)
            {
                Debug.Log($"[LoadTourny] Recovery: ends={savedTournyEnds} from tournament state (pre-load was invalid)");
                ends = savedTournyEnds;
            }
            else if (ends <= 0 || ends > 10)
            {
                Debug.Log($"[LoadTourny] Recovery: reset ends=8 (was {ends}, invalid)");
                ends = 8;
            }

            // 2. Fix team names — but only if the pre-load names were invalid (empty / placeholder).
            // TournySetup already picked correct opponents for this match; if we have valid names
            // from before LoadCareer, preserve them to avoid changing opponents mid-match.
            bool namesAlreadyValid = !string.IsNullOrEmpty(preLoadRedName)
                                  && !string.IsNullOrEmpty(preLoadYellowName)
                                  && preLoadRedName  != "Red"
                                  && preLoadYellowName != "Yellow";
            if (namesAlreadyValid)
            {
                redTeamName    = preLoadRedName;
                yellowTeamName = preLoadYellowName;
                Debug.Log($"[LoadTourny] Recovery: preserved pre-load team names — red='{redTeamName}' yellow='{yellowTeamName}'");
            }

            bool namesFixed = namesAlreadyValid;
            if (!namesFixed && saveData?.currentTournamentState?.teams != null && saveData.currentTournamentState.teams.Count > 0)
            {
                TeamData playerTD = null, oppTD = null;
                foreach (var td in saveData.currentTournamentState.teams)
                    if (td.isPlayer) { playerTD = td; break; }
                if (playerTD != null)
                    foreach (var td in saveData.currentTournamentState.teams)
                        if (td.name == playerTD.nextOpp) { oppTD = td; break; }

                if (playerTD != null && oppTD != null &&
                    !string.IsNullOrEmpty(playerTD.name) && !string.IsNullOrEmpty(oppTD.name))
                {
                    if (aiYellow) { redTeamName = playerTD.name; yellowTeamName = oppTD.name; }
                    else          { yellowTeamName = playerTD.name; redTeamName = oppTD.name; }
                    namesFixed = true;
                    Debug.Log($"[LoadTourny] Recovery: team names from tournament state — red='{redTeamName}' yellow='{yellowTeamName}'");
                }
            }

            if (!namesFixed && cm.teams != null)
            {
                // Fallback: derive from career teams (nextOpp field)
                Team playerT = null;
                foreach (var t in cm.teams)
                    if (t != null && t.player) { playerT = t; break; }
                if (playerT != null && !string.IsNullOrEmpty(playerT.nextOpp))
                {
                    Team oppT = null;
                    foreach (var t in cm.teams)
                        if (t != null && t.name == playerT.nextOpp) { oppT = t; break; }
                    if (oppT != null)
                    {
                        if (aiYellow) { redTeamName = playerT.name; yellowTeamName = oppT.name; }
                        else          { yellowTeamName = playerT.name; redTeamName = oppT.name; }
                        namesFixed = true;
                        Debug.Log($"[LoadTourny] Recovery: team names from career teams — red='{redTeamName}' yellow='{yellowTeamName}'");
                    }
                }
            }

            if (!namesFixed)
                Debug.LogWarning("[LoadTourny] Recovery: could not find real team names — defaulting to Red/Yellow");

            // Recalculate rockCurrent for the corrected rocks value.
            // RestoreGameState set rockCurrent based on the stale (old) rocks count; after we
            // fixed rocks above, rockCurrent is now wrong.  16 - (rocks*2) is always the
            // correct starting rock index for a fresh game of N rocks/team.
            int correctStartRock = 16 - (rocks * 2);
            if (rockCurrent != correctStartRock)
            {
                Debug.Log($"[LoadTourny] Recovery: recalculated rockCurrent={correctStartRock} (was {rockCurrent}, rocks={rocks})");
                rockCurrent = correctStartRock;
            }

            tournyInProgress = true;
        }
        else
        {
            Debug.Log("[GameSettingsPersist] Tournament in progress - skipping career reload to preserve flags");

            // CRITICAL FIX: Even when skipping full reload, we MUST restore:
            // 1. KO1/KO3 flags and playoff teams (for tournament routing)
            // 2. Game state (rockPos, rockCurrent, etc.) for mid-game loads
            CareerSaveData saveData = CareerSaveService.LoadCareer();
            if (saveData != null && saveData.currentTournamentState != null)
            {
                KO1 = saveData.currentTournamentState.KO1;
                KO3 = saveData.currentTournamentState.KO3;
                Debug.Log($"[GameSettingsPersist] Restored tournament type flags: KO1={KO1}, KO3={KO3}");

                // CRITICAL FIX: Restore playoff bracket teams for Single-K/Triple-K
                if (saveData.currentTournamentState.playoffTeams != null &&
                    saveData.currentTournamentState.playoffTeams.Count > 0)
                {
                    Debug.Log($"[GameSettingsPersist] Found {saveData.currentTournamentState.playoffTeams.Count} playoff teams in save");
                    playoffTeams = new Team[saveData.currentTournamentState.playoffTeams.Count];
                    for (int i = 0; i < saveData.currentTournamentState.playoffTeams.Count; i++)
                    {
                        playoffTeams[i] = cm.DataToTeam(saveData.currentTournamentState.playoffTeams[i]);
                    }
                    Debug.Log($"[GameSettingsPersist] Restored {playoffTeams.Length} playoff bracket teams from save");
                }
                else if ((saveData.currentTournamentState.KO1 || saveData.currentTournamentState.KO3) &&
                         saveData.currentTournamentState.teams != null &&
                         saveData.currentTournamentState.teams.Count > 0)
                {
                    // FALLBACK: Old save format - playoff bracket was saved to 'teams' list
                    // This handles saves created before the playoffTeams field was added
                    Debug.LogWarning($"[GameSettingsPersist] Old save format detected - using 'teams' as playoff bracket ({saveData.currentTournamentState.teams.Count} teams)");
                    playoffTeams = new Team[saveData.currentTournamentState.teams.Count];
                    for (int i = 0; i < saveData.currentTournamentState.teams.Count; i++)
                    {
                        playoffTeams[i] = cm.DataToTeam(saveData.currentTournamentState.teams[i]);
                    }
                    Debug.Log($"[GameSettingsPersist] Restored {playoffTeams.Length} playoff bracket teams from legacy format");
                }
                else
                {
                    Debug.Log("[GameSettingsPersist] No playoff bracket teams in save - regular tournament");
                }

                // CRITICAL FIX: Restore game state (rockPos, rockCurrent, etc.)
                // This is essential for continuing from End Menu to TournyGame
                if (saveData.currentGameState != null)
                {
                    // TournySetup already set the correct real team names in gsp.
                    // Capture them before RestoreGameState can overwrite with save's "Red"/"Yellow" placeholders.
                    string tournyRed    = redTeamName;
                    string tournyYellow = yellowTeamName;
                    Debug.Log($"[GameSettingsPersist] Before RestoreGameState: red='{tournyRed}', yellow='{tournyYellow}'");

                    cm.RestoreGameState(saveData.currentGameState, this);

                    // If RestoreGameState replaced the real names with generic placeholders, put them back.
                    bool tournyRedReal    = !string.IsNullOrEmpty(tournyRed)    && tournyRed    != "Red" && tournyRed    != "Yellow";
                    bool tournyYellowReal = !string.IsNullOrEmpty(tournyYellow) && tournyYellow != "Red" && tournyYellow != "Yellow";
                    if (tournyRedReal && (string.IsNullOrEmpty(redTeamName) || redTeamName == "Red" || redTeamName == "Yellow"))
                    {
                        redTeamName = tournyRed;
                        Debug.Log($"[GameSettingsPersist] Reinstated red team name from TournySetup: '{redTeamName}'");
                    }
                    if (tournyYellowReal && (string.IsNullOrEmpty(yellowTeamName) || yellowTeamName == "Red" || yellowTeamName == "Yellow"))
                    {
                        yellowTeamName = tournyYellow;
                        Debug.Log($"[GameSettingsPersist] Reinstated yellow team name from TournySetup: '{yellowTeamName}'");
                    }
                    Debug.Log($"[GameSettingsPersist] After RestoreGameState: red='{redTeamName}', yellow='{yellowTeamName}'");
                }
            }
            else
            {
                Debug.LogWarning("[GameSettingsPersist] Could not restore KO1/KO3 flags - no save data available!");
            }
        }
        
        bg = cm.currentTourny.BG;
        crowdDensity = cm.currentTourny.crowdDensity;
        prize = cm.currentTourny.prizeMoney;
        numberOfTeams = cm.currentTourny.teams;
        teamList = new List<Team_List>();
        
        // CRITICAL FIX: Restore team objects from teams array based on team names
        // This is needed for mid-game loads where TournySetup() hasn't run
        if (teams != null && teams.Length > 0)
        {
            Debug.Log($"[GameSettingsPersist] Restoring team objects - redTeam: {redTeamName}, yellowTeam: {yellowTeamName}");
            
            // Find and restore red team
            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i] != null && teams[i].name == redTeamName)
                {
                    redTeam = teams[i];
                    Debug.Log($"[GameSettingsPersist] Found redTeam: {redTeam.name} with {redTeam.players.Count} players");
                    break;
                }
            }
            
            // Find and restore yellow team
            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i] != null && teams[i].name == yellowTeamName)
                {
                    yellowTeam = teams[i];
                    Debug.Log($"[GameSettingsPersist] Found yellowTeam: {yellowTeam.name} with {yellowTeam.players.Count} players");
                    break;
                }
            }
            
            // Safety check
            if (redTeam == null)
            {
                Debug.LogError($"[GameSettingsPersist] Could not find redTeam '{redTeamName}' in teams array!");
            }
            
            if (yellowTeam == null)
            {
                Debug.LogError($"[GameSettingsPersist] Could not find yellowTeam '{yellowTeamName}' in teams array!");
            }
            
            // CRITICAL FIX: Set playerTeam to whichever team is the player's
            // This is needed for playoff logic to determine win/loss
            if (redTeam != null && redTeam.id == cm.playerTeamIndex)
            {
                playerTeam = redTeam;
                Debug.Log($"[GameSettingsPersist] Player team is RED: {playerTeam.name}");
            }
            else if (yellowTeam != null && yellowTeam.id == cm.playerTeamIndex)
            {
                playerTeam = yellowTeam;
                Debug.Log($"[GameSettingsPersist] Player team is YELLOW: {playerTeam.name}");
            }
            else
            {
                Debug.LogWarning($"[GameSettingsPersist] Could not determine player team! cm.playerTeamIndex={cm.playerTeamIndex}, redTeam.id={redTeam?.id}, yellowTeam.id={yellowTeam?.id}");
            }

            // Sync AI flags with the identified player team. The save used for recovery
            // was made before TournySetup ran, so aiRed/aiYellow may be stale (both false).
            // Exactly one team is the player; the other must be AI.
            if (playerTeam == redTeam)
            {
                aiRed = false;
                aiYellow = true;
                Debug.Log("[GameSettingsPersist] AI flags synced: player=red, AI=yellow");
            }
            else if (playerTeam == yellowTeam)
            {
                aiRed = true;
                aiYellow = false;
                Debug.Log("[GameSettingsPersist] AI flags synced: player=yellow, AI=red");
            }
        }
        else
        {
            Debug.LogWarning("[GameSettingsPersist] teams array is null or empty - cannot restore team objects");
        }

        Debug.Log("teamList Count is " + teamList.Count);
    }

    public void LoadKOTourny()
    {
        CareerManager cm = FindFirstObjectByType<CareerManager>();
        //cm.LoadCareer();
        PlayoffManager_TripleK pm3k = FindFirstObjectByType<PlayoffManager_TripleK>();
        teamList = new List<Team_List>();
        myFile = new EasyFileSave("my_player_data");
        //inProgress = true;
        if (myFile.Load())
        {
            //inProgress = myFile.GetBool("Tourny In Progress");
            prize = myFile.GetInt("Prize Money");
            draw = myFile.GetInt("Draw");
            ends = myFile.GetInt("Ends");
            rocks = myFile.GetInt("Rocks");
            numberOfTeams = myFile.GetInt("Number Of Teams");
            playoffRound = myFile.GetInt("Playoff Round");
            playerTeamIndex = myFile.GetInt("Player Team Index");

            string[] nameList = new string[numberOfTeams];
            int[] winsList = new int[numberOfTeams];
            int[] lossList = new int[numberOfTeams];
            int[] rankList = new int[numberOfTeams];
            string[] nextOppList = new string[numberOfTeams];
            int[] strengthList = new int[numberOfTeams];
            int[] idList = new int[numberOfTeams];

            //Debug.Log("nameList Count is " + nameList.Length);
            //nameList = myFile.GetArray<string>("Tourny Name List");
            Debug.Log("nameList Item 1 is " + nameList[0]);
            winsList = myFile.GetArray<int>("Tourny Wins List");
            lossList = myFile.GetArray<int>("Tourny Loss List");
            rankList = myFile.GetArray<int>("Tourny Rank List");
            nextOppList = myFile.GetArray<string>("Tourny NextOpp List");
            strengthList = myFile.GetArray<int>("Tourny Strength List");
            idList = myFile.GetArray<int>("Tourny Team ID List");
            //StartCoroutine(Wait());
            Debug.Log("nameList Count is " + nameList.Length);

            teams = new Team[numberOfTeams];

            for (int i = 0; i < numberOfTeams; i++)
            {
                teams[i] = cm.currentTournyTeams[i];

                if (teams[i].id == cm.playerTeamIndex)
                {
                    playerTeam = teams[i];
                }
            }
            //StartCoroutine(Wait());
            //playerTeam = teams[playerTeamIndex];
            for (int i = 0; i < numberOfTeams; i++)
            {
                teamList.Add(new Team_List(teams[i]));
            }

            //score = new Vector2Int[ends + 1];

            //int[] gameListX = myFile.GetArray<int>("Tourny Game X List");
            //int[] gameListY = myFile.GetArray<int>("Tourny Game Y List");
            //pm3k.gameList = new Vector2[gameListX.Length];

            //for (int i = 0; i < pm3k.gameList.Length; i++)
            //{
            //    pm3k.gameList[i].x = gameListX[i];
            //    pm3k.gameList[i].y = gameListY[i];
            //}
            Debug.Log("teamList Count is " + teamList.Count);
            myFile.Dispose();
        }
    }

    public void StoryGame()
    {
        //story = true;
        sm = GameObject.Find("StoryManager").GetComponent<StoryManager>();

        Debug.Log("Loading to GSP");
        //Debug.Log("Ends is " + myFile.GetInt("End Total"));

        ends = sm.ends;
        endCurrent = sm.endCurrent;
        rocks = sm.rocks;
        rockCurrent = sm.rockCurrent;
        redScore = sm.redScore;
        yellowScore = sm.yellowScore;
        redHammer = sm.redHammer;
        aiYellow = sm.aiYellow;
        aiRed = sm.aiRed;
        //third = sm.third;
        //skip = sm.skip;

        //redScore = myFile.GetInt("Red Score");
        //yellowScore = myFile.GetInt("Yellow Score");
    }

    public void AutoSave()
    {
        Debug.Log("[GameSettingsPersist] AutoSave called - delegating to CareerManager");
        CareerManager cm = FindFirstObjectByType<CareerManager>();
        
        if (cm != null)
        {
            cm.SaveCareer(this);  // Pass self to save current game state
        }
        else
        {
            Debug.LogWarning("[GameSettingsPersist] CareerManager not found for auto-save");
        }
    }

    private void Update()
    {
        if (gs)
        {
            ends = gs.ends;
            rocks = gs.rocks;
            aiYellow = gs.aiYellow;
            aiRed = gs.aiRed;
            //loadGame = false;
        }

    }

    public void OnTutorial()
    {
        ends = 10;
        rocks = 8;
        redHammer = true;
        //GameManager gm = FindFirstObjectByType<GameManager>();

        //gm.endCurrent = 10;
    }

    IEnumerator Wait()
    {
        yield return new WaitForEndOfFrame();
    }
}
