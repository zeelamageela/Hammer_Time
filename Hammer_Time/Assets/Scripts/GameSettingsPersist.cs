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

    public List<Team_List> teamList;
    public Team[] teams;
    public Team[] playoffTeams;
    public CashGamePlayers[] cgp;
    public int playerTeamIndex;
    public Vector2Int[] score;

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
        teamColour = cm.teamColour;
        //earnings = ts.earnings;
        games = ts.games;
        if (cashGame)
            ends = 1;
        else
            ends = ts.ends;
        endCurrent = 0;
        rocks = ts.rocks;
        //numberOfTeams = ts.teams;
        prize = ts.prize;
        draw = 0;
        playoffRound = 0;
        
        Debug.Log($"[GSP.LoadTournySettings] AFTER: games={games}, ends={ends}, rocks={rocks}");
        Debug.Log($"[GSP.LoadTournySettings] Calling cm.SaveCareer() to persist tournament settings...");
        
        //redScore = myFile.GetInt("Red Score");
        //yellowScore = myFile.GetInt("Yellow Score");
        cm.SaveCareer(this);
        
        Debug.Log($"[GSP.LoadTournySettings] Save complete - games={games} should now be in save file");
    }

    public void TournySetup(int btn = 0)
    {
        Debug.Log("[GSP] TournySetup called");
        TournyManager tm = FindFirstObjectByType<TournyManager>();
        PlayoffManager pm = FindFirstObjectByType<PlayoffManager>();
        PlayoffManager_SingleK pm1k = FindFirstObjectByType<PlayoffManager_SingleK>();
        CareerManager cm = FindFirstObjectByType<CareerManager>();
        CashGames cg = FindFirstObjectByType<CashGames>();
        
        // CRITICAL: Reset GAME state flags when setting up NEW game
        // DO NOT reset TOURNAMENT settings (games, ends, rocks) - they come from TournySettings!
        gameInProgress = false;
        loadGame = false;
        rockCurrent = 0;
        endCurrent = 0;
        redScore = 0;
        yellowScore = 0;
        
        // ? CRITICAL FIX: Initialize score array for new game (don't leave it null!)
        if (score == null || score.Length != ends)
        {
            score = new Vector2Int[ends];
            for (int i = 0; i < ends; i++)
            {
                score[i] = new Vector2Int(0, 0);
            }
            Debug.Log($"[GSP.TournySetup] Initialized score array for {ends} ends");
        }
        else
        {
            // Clear existing array
            for (int i = 0; i < score.Length; i++)
            {
                score[i] = new Vector2Int(0, 0);
            }
            Debug.Log($"[GSP.TournySetup] Cleared existing score array ({score.Length} ends)");
        }
        
        Debug.Log("[GSP] TournySetup - cleared game state (scores, rockCurrent, endCurrent)");
        Debug.Log($"[GSP] TournySetup - preserved tournament settings: games={games}, ends={ends}, rocks={rocks}");
        
        careerLoad = false;
        if (cg != null)
        {
            tourny = false;
            draw = 0;
            playoffRound = 0;
            //playerTeam = teams[playerTeamIndex];
            endCurrent = 0;
            redScore = 0;
            yellowScore = 0;

            playerTeam = teams[0];
            playerTeam.nextOpp = teams[btn + 1].name;
            endCurrent = 0;
            redScore = 0;
            yellowScore = 0;
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
            endCurrent = 0;
            redScore = 0;
            yellowScore = 0;
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
        Debug.Log("[GameSettingsPersist] LoadTourny called");
        CareerManager cm = FindFirstObjectByType<CareerManager>();

        // CRITICAL FIX: Don't reload career if tournament is already in progress
        // The flags (justFinishedGame, tournyInProgress, etc.) are already set correctly in memory
        // Reloading would overwrite them with stale data
        if (!tournyInProgress)
        {
            Debug.Log("[GameSettingsPersist] Loading career for tournament setup");
            cm.LoadCareer(this);
        }
        else
        {
            Debug.Log("[GameSettingsPersist] Tournament in progress - skipping career reload to preserve flags");
            
            // CRITICAL FIX: Even when skipping full reload, we MUST restore KO1/KO3 flags AND playoffTeams!
            // These determine which scene we route to after the game and enable bracket advancement
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
