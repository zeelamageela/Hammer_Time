using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TigerForge;

/// <summary>
/// ?? DEPRECATED - Triple Knockout Tournament Manager
/// 
/// This format has been ABANDONED for tour events in favor of Page Playoff (4 teams).
/// 
/// Reasons for deprecation:
/// - Complex UI with 20 different DisplayRoundX() methods
/// - 4 separate bracket containers requiring manual positioning
/// - 46 games in gameList causing save/load complexity
/// - Hours of debugging display crashes with minimal gameplay benefit
/// 
/// REPLACEMENT: Tour events now use PlayoffManager.cs (Page Playoff - 4 teams, 3 rounds)
/// 
/// This code is kept for reference but is NOT USED in active gameplay.
/// See: TOUR_EVENTS_PAGE_PLAYOFF_SWITCH.md for full details
/// </summary>
public class PlayoffManager_TripleK : MonoBehaviour
{
    #region Serialized Fields - UI References
    
    [Header("Core References")]
    public CareerManager cm;
    public Team[] teams;
    
    [Header("UI Components")]
    public GameObject playoffs;
    public Text heading;
    public Scrollbar horizScrollBar;
    public Scrollbar vertScrollBar;
    public Text careerEarningsText;
    
    [Header("Bracket Containers")]
    public GameObject winnersBracket;
    public GameObject losersBracket1;
    public GameObject losersBracket2;
    public GameObject finalsBracket;
    
    [Header("Display Colors")]
    public Color green;
    public Color red;
    public Color yellow;
    public Color dimmed;
    
    [Header("VS Display")]
    public VSDisplay[] vsDisplay;
    public Text vsDisplayTitle;
    public Text vsDisplayVS;
    public GameObject vsDisplayGO;
    
    [Header("Buttons")]
    public Button nextButton;
    public Button simButton;
    public Button contButton;
    public Button playButton;
    
    [Header("Winners Bracket Displays")]
    public BracketDisplay[] winnersDisplay1;
    public BracketDisplay[] winnersDisplay3;
    public BracketDisplay[] winnersDisplay7;
    public BracketDisplay[] winnersDisplay12;
    
    [Header("Losers Bracket A Displays")]
    public BracketDisplay[] losersDisplayA2;
    public BracketDisplay[] losersDisplayA4;
    public BracketDisplay[] losersDisplayA8;
    public BracketDisplay[] losersDisplayA9;
    public BracketDisplay[] losersDisplayA13;
    
    [Header("Losers Bracket B Displays")]
    public BracketDisplay[] losersDisplayB5;
    public BracketDisplay[] losersDisplayB6;
    public BracketDisplay[] losersDisplayB10;
    public BracketDisplay[] losersDisplayB11;
    public BracketDisplay[] losersDisplayB14;
    
    [Header("Finals Displays")]
    public BracketDisplay[] finalsDisplay15;
    public BracketDisplay[] finalsDisplay16;
    public BracketDisplay[] finalsDisplay17;
    public BracketDisplay[] finalsDisplay18;
    public BracketDisplay[] finalsDisplay19;
    public BracketDisplay[] finalsDisplay20;
    
    #endregion
    
    #region Private State
    
    private GameSettingsPersist gsp;
    public Vector2[] gameList; // 46 games total in triple knockout - public for CareerManager save
    public int playerTeam; // public for GameSettingsPersist
    public int oppTeam; // public for GameSettingsPersist
    public int playoffRound;
    private bool simInProgress;
    
    #endregion
    
    #region Initialization
    
    private void Start()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
        cm = FindObjectOfType<CareerManager>();
        
        Debug.Log($"[TripleK] START - Round={gsp.playoffRound}, justFinished={gsp.justFinishedGame}, careerLoad={gsp.careerLoad}");
        
        teams = new Team[16];
        gameList = new Vector2[46];
        playoffs.SetActive(true);
        
        if (cm)
        {
            playerTeam = cm.playerTeamIndex;
            Debug.Log($"[TripleK] Player team index: {playerTeam}");
        }
        
        // Handle different initialization scenarios
        if (gsp.justFinishedGame)
        {
            Debug.Log("[TripleK] Returning from player game - advancing tournament");
            gsp.justFinishedGame = false;
            LoadAndAdvancePlayoffs();
        }
        else if (gsp.careerLoad && gsp.tournyInProgress)
        {
            Debug.Log("[TripleK] Loading tournament in progress");
            LoadTournamentState();
        }
        else if (playoffRound > 0)
        {
            Debug.Log("[TripleK] Resuming existing tournament");
            LoadTournamentState();
        }
        else
        {
            Debug.Log("[TripleK] Starting new tournament");
            InitializeNewTournament();
        }
    }
    
    /// <summary>
    /// Initialize a brand new tournament with seeding
    /// </summary>
    private void InitializeNewTournament()
    {
        // Load teams from CareerManager
        for (int i = 0; i < teams.Length; i++)
        {
            teams[i] = cm.teamPool[i];
        }
        
        playoffRound = 1;
        gsp.tournyRecord = Vector2.zero;
        
        // Save pre-tournament records
        cm.teamRecords = new Vector4[teams.Length];
        cm.tourRecords = new Vector4[teams.Length];
        
        for (int i = 0; i < teams.Length; i++)
        {
            cm.teamRecords[i] = new Vector4(teams[i].wins, teams[i].loss, teams[i].earnings, teams[i].id);
            cm.tourRecords[i] = new Vector4(teams[i].tourRecord.x, teams[i].tourRecord.y, teams[i].tourPoints, teams[i].id);
            
            // Reset tournament stats
            teams[i].wins = 0;
            teams[i].loss = 0;
            teams[i].earnings = 0;
            teams[i].tourPoints = 0;
            teams[i].tourRecord = Vector2.zero;
        }
        
        // Create initial pairings (Round 1)
        for (int i = 0; i < 8; i++)
        {
            gameList[i] = new Vector2(teams[i * 2].id, teams[i * 2 + 1].id);
        }
        
        gsp.teams = teams;
        gsp.playoffRound = playoffRound;
        
        SetPlayoffs();
        cm.SaveTournyState();
    }
    
    /// <summary>
    /// Load tournament state from saved data
    /// </summary>
    private void LoadTournamentState()
    {
        // Teams are already loaded via CareerManager's JSON system
        for (int i = 0; i < teams.Length; i++)
        {
            teams[i] = gsp.teams[i];
        }
        
        // Load gameList from legacy save (temporary until fully migrated to JSON)
        EasyFileSave myFile = new EasyFileSave("my_player_data");
        if (myFile.Load())
        {
            int[] gameListX = myFile.GetArray<int>("Tourny Game X List");
            int[] gameListY = myFile.GetArray<int>("Tourny Game Y List");
            
            if (gameListX != null && gameListY != null)
            {
                for (int i = 0; i < gameList.Length; i++)
                {
                    gameList[i] = new Vector2(gameListX[i], gameListY[i]);
                }
            }
            myFile.Dispose();
        }
        
        playoffRound = gsp.playoffRound;
        
        // Find player and opponent
        for (int i = 0; i < teams.Length; i++)
        {
            if (teams[i].player)
                playerTeam = i;
            if (teams[i].name == gsp.playerTeam.nextOpp)
                oppTeam = i;
        }
        
        SetPlayoffs();
    }
    
    /// <summary>
    /// Load saved state and advance after player's completed game
    /// </summary>
    private void LoadAndAdvancePlayoffs()
    {
        Debug.Log($"[TripleK] LoadAndAdvancePlayoffs - Round {gsp.playoffRound}");
        
        // Load teams and gameList
        for (int i = 0; i < teams.Length; i++)
        {
            teams[i] = gsp.teams[i];
        }
        
        EasyFileSave myFile = new EasyFileSave("my_player_data");
        if (myFile.Load())
        {
            int[] gameListX = myFile.GetArray<int>("Tourny Game X List");
            int[] gameListY = myFile.GetArray<int>("Tourny Game Y List");
            
            for (int i = 0; i < gameList.Length; i++)
            {
                gameList[i] = new Vector2(gameListX[i], gameListY[i]);
            }
            myFile.Dispose();
        }
        
        playoffRound = gsp.playoffRound;
        
        // Find player and opponent
        for (int i = 0; i < teams.Length; i++)
        {
            if (teams[i].player)
                playerTeam = i;
            if (teams[i].name == gsp.playerTeam.nextOpp)
            {
                oppTeam = i;
                break;
            }
        }
        
        Debug.Log($"[TripleK] Player={playerTeam} vs Opponent={oppTeam} in Round {playoffRound}");
        
        // Process player's game result
        bool playerWon = SharedTournamentLogic.DeterminePlayerWon(gsp);
        
        if (playerWon)
        {
            teams[playerTeam].wins++;
            teams[oppTeam].loss++;
            Debug.Log($"[TripleK] Player WON - Record: {teams[playerTeam].wins}W-{teams[playerTeam].loss}L");
        }
        else
        {
            teams[oppTeam].wins++;
            teams[playerTeam].loss++;
            Debug.Log($"[TripleK] Player LOST - Record: {teams[playerTeam].wins}W-{teams[playerTeam].loss}L");
        }
        
        // Update GSP
        gsp.playoffTeams = teams;
        gsp.teams = teams;
        
        // Simulate remaining AI games in this round
        StartCoroutine(SimPlayoff());
    }
    
    #endregion
    
    #region Game Simulation
    
    public void OnSim()
    {
        StartCoroutine(SimPlayoff());
    }
    
    /// <summary>
    /// Simulate all games in the current round
    /// </summary>
    private IEnumerator SimPlayoff()
    {
        playButton.gameObject.SetActive(false);
        Debug.Log($"[TripleK.SimPlayoff] Round {playoffRound}");
        
        SimulateRoundGames();
        
        yield return StartCoroutine(RefreshPlayoffPanel());
        
        DisplayRoundResults();
    }
    
    /// <summary>
    /// Simulate games for the current round
    /// </summary>
    private void SimulateRoundGames()
    {
        bool cont = (gsp.redScore != gsp.yellowScore); // Player just played
        
        switch (playoffRound)
        {
            case 1: SimulateRound1(cont); break;
            case 2: SimulateRound2(cont); break;
            case 3: SimulateRound3(cont); break;
            case 4: SimulateRound4(cont); break;
            case 5: SimulateRound5(cont); break;
            case 6: SimulateRound6(cont); break;
            case 7: SimulateRound7(cont); break;
            case 8: SimulateRound8(cont); break;
            case 9: SimulateRound9(cont); break;
            case 10: SimulateRound10(cont); break;
            case 11: SimulateRound11(cont); break;
            case 12: SimulateRound12(cont); break;
            case 13: SimulateRound13(cont); break;
            case 14: SimulateRound14(cont); break;
            case 15: SimulateRound15(cont); break;
            case 16: SimulateRound16(cont); break;
            case 17: SimulateRound17(cont); break;
            case 18: SimulateRound18(cont); break;
            case 19: SimulateRound19(cont); break;
            case 20: SimulateRound20(cont); break;
        }
    }
    
    #region Round Simulation Methods
    
    private void SimulateRound1(bool cont)
    {
        // 8 games: gameList[0-7] ? winners to gameList[12-15].x/y, losers to gameList[8-11].x/y
        for (int i = 0; i < 8; i++)
        {
            Team teamX = GetTeamById((int)gameList[i].x);
            Team teamY = GetTeamById((int)gameList[i].y);
            
            // Safety check
            if (teamX == null || teamY == null)
            {
                Debug.LogWarning($"[TripleK] Round 1 Game {i}: Null team (X ID={gameList[i].x}, Y ID={gameList[i].y})");
                continue;
            }
            
            // Skip player's game if already played (cont = player just finished)
            if (ShouldSkipPlayerGame(teamX, teamY, cont))
            {
                Debug.Log($"[TripleK] Round 1 Game {i}: Skipping player game (already played)");
                
                // Player's result already recorded in LoadAndAdvancePlayoffs
                bool xWon = DetermineWinnerFromStats(teamX, teamY);
                
                if (i % 2 == 0)
                {
                    gameList[(i / 2) + 12].x = xWon ? teamX.id : teamY.id;
                    gameList[(i / 2) + 8].x = xWon ? teamY.id : teamX.id;
                }
                else
                {
                    gameList[((i - 1) / 2) + 12].y = xWon ? teamX.id : teamY.id;
                    gameList[((i - 1) / 2) + 8].y = xWon ? teamY.id : teamX.id;
                }
                continue;
            }
            
            bool xWins = SimulateGame(teamX, teamY, cont);
            
            if (i % 2 == 0)
            {
                gameList[(i / 2) + 12].x = xWins ? teamX.id : teamY.id;
                gameList[(i / 2) + 8].x = xWins ? teamY.id : teamX.id;
            }
            else
            {
                gameList[((i - 1) / 2) + 12].y = xWins ? teamX.id : teamY.id;
                gameList[((i - 1) / 2) + 8].y = xWins ? teamY.id : teamX.id;
            }
            
            if (xWins)
            {
                teamX.wins++;
                teamY.loss++;
            }
            else
            {
                teamY.wins++;
                teamX.loss++;
            }
        }
    }
    
    private void SimulateRound2(bool cont)
    {
        // 4 games: gameList[8-11] ? winners to gameList[16-19].y, losers to gameList[20-23].x (eliminated)
        for (int i = 0; i < 4; i++)
        {
            Team teamX = GetTeamById((int)gameList[i + 8].x);
            Team teamY = GetTeamById((int)gameList[i + 8].y);
            
            // Safety check
            if (teamX == null || teamY == null)
            {
                Debug.LogWarning($"[TripleK] Round 2 Game {i}: Null team (X ID={gameList[i + 8].x}, Y ID={gameList[i + 8].y})");
                continue;
            }
            
            // Skip player's game if already played
            if (ShouldSkipPlayerGame(teamX, teamY, cont))
            {
                Debug.Log($"[TripleK] Round 2 Game {i}: Skipping player game (already played)");
                
                bool xWon = DetermineWinnerFromStats(teamX, teamY);
                
                gameList[i + 16].y = xWon ? teamX.id : teamY.id;
                gameList[i + 20].x = xWon ? teamY.id : teamX.id;
                continue;
            }
            
            bool xWins = SimulateGame(teamX, teamY, cont);
            
            gameList[i + 16].y = xWins ? teamX.id : teamY.id;
            gameList[i + 20].x = xWins ? teamY.id : teamX.id;
            
            if (xWins)
            {
                teamX.wins++;
                teamY.loss++;
            }
            else
            {
                teamY.wins++;
                teamX.loss++;
            }
        }
    }
    
    private void SimulateRound3(bool cont)
    {
        // 4 games: gameList[12-15] ? winners to gameList[26-27].x/y, losers to gameList[16-19].x
        for (int i = 0; i < 4; i++)
        {
            Team teamX = GetTeamById((int)gameList[i + 12].x);
            Team teamY = GetTeamById((int)gameList[i + 12].y);
            
            bool xWon;
            if (ProcessGame(ref teamX, ref teamY, cont, out xWon))
            {
                // Game was skipped (null or player already played) - just advance teams
                if (i % 2 == 0)
                {
                    gameList[(i / 2) + 26].x = xWon ? teamX.id : teamY.id;
                    gameList[19 - i].x = xWon ? teamY.id : teamX.id;
                }
                else
                {
                    gameList[((i - 1) / 2) + 26].y = xWon ? teamX.id : teamY.id;
                    gameList[19 - i].x = xWon ? teamY.id : teamX.id;
                }
                continue;
            }
            
            bool xWins = SimulateGame(teamX, teamY, cont);
            
            if (i % 2 == 0)
            {
                gameList[(i / 2) + 26].x = xWins ? teamX.id : teamY.id;
                gameList[19 - i].x = xWins ? teamY.id : teamX.id;
            }
            else
            {
                gameList[((i - 1) / 2) + 26].y = xWins ? teamX.id : teamY.id;
                gameList[19 - i].x = xWins ? teamY.id : teamX.id;
            }
            
            if (xWins)
            {
                teamX.wins++;
                teamY.loss++;
            }
            else
            {
                teamY.wins++;
                teamX.loss++;
            }
        }
    }
    
    private void SimulateRound4(bool cont)
    {
        // 4 games: gameList[16-19] ? winners to gameList[28-29].x/y, losers to gameList[20-23].y (eliminated)
        for (int i = 0; i < 4; i++)
        {
            Team teamX = GetTeamById((int)gameList[i + 16].x);
            Team teamY = GetTeamById((int)gameList[i + 16].y);
            
            bool xWon;
            if (ProcessGame(ref teamX, ref teamY, cont, out xWon))
            {
                if (i % 2 == 0)
                {
                    gameList[(i / 2) + 28].x = xWon ? teamX.id : teamY.id;
                    gameList[23 - i].y = xWon ? teamY.id : teamX.id;
                }
                else
                {
                    gameList[((i - 1) / 2) + 28].y = xWon ? teamX.id : teamY.id;
                    gameList[23 - i].y = xWon ? teamY.id : teamX.id;
                }
                continue;
            }
            
            bool xWins = SimulateGame(teamX, teamY, cont);
            
            if (i % 2 == 0)
            {
                gameList[(i / 2) + 28].x = xWins ? teamX.id : teamY.id;
                gameList[23 - i].y = xWins ? teamY.id : teamX.id;
            }
            else
            {
                gameList[((i - 1) / 2) + 28].y = xWins ? teamX.id : teamY.id;
                gameList[23 - i].y = xWins ? teamY.id : teamX.id;
            }
            
            if (xWins)
            {
                teamX.wins++;
                teamY.loss++;
            }
            else
            {
                teamY.wins++;
                teamX.loss++;
            }
        }
    }
    
    private void SimulateRound5(bool cont)
    {
        // 4 games: gameList[20-23] ? winners to gameList[24-25].x/y, losers eliminated (rank 13)
        for (int i = 0; i < 4; i++)
        {
            Team teamX = GetTeamById((int)gameList[i + 20].x);
            Team teamY = GetTeamById((int)gameList[i + 20].y);
            
            bool xWon;
            if (ProcessGame(ref teamX, ref teamY, cont, out xWon))
            {
                if (i % 2 == 0)
                {
                    gameList[(i / 2) + 24].x = xWon ? teamX.id : teamY.id;
                }
                else
                {
                    gameList[((i - 1) / 2) + 24].y = xWon ? teamX.id : teamY.id;
                }
                continue;
            }
            
            bool xWins = SimulateGame(teamX, teamY, cont);
            
            if (i % 2 == 0)
            {
                gameList[(i / 2) + 24].x = xWins ? teamX.id : teamY.id;
            }
            else
            {
                gameList[((i - 1) / 2) + 24].y = xWins ? teamX.id : teamY.id;
            }
            
            if (xWins)
            {
                teamX.wins++;
                teamY.loss++;
                teamY.rank = 13;
            }
            else
            {
                teamY.wins++;
                teamX.loss++;
                teamX.rank = 13;
            }
        }
    }
    
    private void SimulateRound6(bool cont)
    {
        // 2 games: gameList[24-25] ? winners to gameList[32-33].y, losers eliminated (rank 11)
        for (int i = 0; i < 2; i++)
        {
            Team teamX = GetTeamById((int)gameList[i + 24].x);
            Team teamY = GetTeamById((int)gameList[i + 24].y);
            
            bool xWon;
            if (ProcessGame(ref teamX, ref teamY, cont, out xWon))
            {
                gameList[32 + i].y = xWon ? teamX.id : teamY.id;
                continue;
            }
            
            bool xWins = SimulateGame(teamX, teamY, cont);
            
            gameList[32 + i].y = xWins ? teamX.id : teamY.id;
            
            if (xWins)
            {
                teamX.wins++;
                teamY.loss++;
                teamY.rank = 11;
            }
            else
            {
                teamY.wins++;
                teamX.loss++;
                teamX.rank = 11;
            }
        }
    }
    
    private void SimulateRound7(bool cont)
    {
        // 2 games: gameList[26-27] ? winners to gameList[36].x/y, losers to gameList[30-31]
        for (int i = 0; i < 2; i++)
        {
            Team teamX = GetTeamById((int)gameList[i + 26].x);
            Team teamY = GetTeamById((int)gameList[i + 26].y);
            
            bool xWon;
            if (ProcessGame(ref teamX, ref teamY, cont, out xWon))
            {
                if (i == 0)
                {
                    gameList[36].x = xWon ? teamX.id : teamY.id;
                    gameList[30].x = xWon ? teamY.id : teamX.id;
                }
                else
                {
                    gameList[36].y = xWon ? teamX.id : teamY.id;
                    gameList[31].y = xWon ? teamY.id : teamX.id;
                }
                continue;
            }
            
            bool xWins = SimulateGame(teamX, teamY, cont);
            
            if (i == 0)
            {
                gameList[36].x = xWins ? teamX.id : teamY.id;
                gameList[30].x = xWins ? teamY.id : teamX.id;
            }
            else
            {
                gameList[36].y = xWins ? teamX.id : teamY.id;
                gameList[31].y = xWins ? teamY.id : teamX.id;
            }
            
            if (xWins)
            {
                teamX.wins++;
                teamY.loss++;
            }
            else
            {
                teamY.wins++;
                teamX.loss++;
            }
        }
    }
    
    private void SimulateRound8(bool cont)
    {
        // 2 games: gameList[28-29] ? winners to gameList[30].y/[31].x, losers to gameList[32-33].x
        for (int i = 0; i < 2; i++)
        {
            Team teamX = GetTeamById((int)gameList[i + 28].x);
            Team teamY = GetTeamById((int)gameList[i + 28].y);
            
            bool xWon;
            if (ProcessGame(ref teamX, ref teamY, cont, out xWon))
            {
                if (i == 0)
                {
                    gameList[30].y = xWon ? teamX.id : teamY.id;
                    gameList[32].x = xWon ? teamY.id : teamX.id;
                }
                else
                {
                    gameList[31].x = xWon ? teamX.id : teamY.id;
                    gameList[33].x = xWon ? teamY.id : teamX.id;
                }
                continue;
            }
            
            bool xWins = SimulateGame(teamX, teamY, cont);
            
            if (i == 0)
            {
                gameList[30].y = xWins ? teamX.id : teamY.id;
                gameList[32].x = xWins ? teamY.id : teamX.id;
            }
            else
            {
                gameList[31].x = xWins ? teamX.id : teamY.id;
                gameList[33].x = xWins ? teamY.id : teamX.id;
            }
            
            if (xWins)
            {
                teamX.wins++;
                teamY.loss++;
            }
            else
            {
                teamY.wins++;
                teamX.loss++;
            }
        }
    }
    
    private void SimulateRound9(bool cont)
    {
        // 2 games: gameList[30-31] ? winners to gameList[37].x/y, losers to gameList[34-35].x
        for (int i = 0; i < 2; i++)
        {
            Team teamX = GetTeamById((int)gameList[i + 30].x);
            Team teamY = GetTeamById((int)gameList[i + 30].y);
            
            bool xWon;
            if (ProcessGame(ref teamX, ref teamY, cont, out xWon))
            {
                if (i == 0)
                {
                    gameList[37].x = xWon ? teamX.id : teamY.id;
                    gameList[35].x = xWon ? teamY.id : teamX.id;
                }
                else
                {
                    gameList[37].y = xWon ? teamX.id : teamY.id;
                    gameList[34].x = xWon ? teamY.id : teamX.id;
                }
                continue;
            }
            
            bool xWins = SimulateGame(teamX, teamY, cont);
            
            if (i == 0)
            {
                gameList[37].x = xWins ? teamX.id : teamY.id;
                gameList[35].x = xWins ? teamY.id : teamX.id;
            }
            else
            {
                gameList[37].y = xWins ? teamX.id : teamY.id;
                gameList[34].x = xWins ? teamY.id : teamX.id;
            }
            
            if (xWins)
            {
                teamX.wins++;
                teamY.loss++;
            }
            else
            {
                teamY.wins++;
                teamX.loss++;
            }
        }
    }
    
    private void SimulateRound10(bool cont)
    {
        // 2 games: gameList[32-33] ? winners to gameList[34-35].y, losers eliminated (rank 9)
        for (int i = 0; i < 2; i++)
        {
            Team teamX = GetTeamById((int)gameList[i + 32].x);
            Team teamY = GetTeamById((int)gameList[i + 32].y);
            
            bool xWon;
            if (ProcessGame(ref teamX, ref teamY, cont, out xWon))
            {
                gameList[34 + i].y = xWon ? teamX.id : teamY.id;
                continue;
            }
            
            bool xWins = SimulateGame(teamX, teamY, cont);
            
            gameList[34 + i].y = xWins ? teamX.id : teamY.id;
            
            if (xWins)
            {
                teamX.wins++;
                teamY.loss++;
                teamY.rank = 9;
            }
            else
            {
                teamY.wins++;
                teamX.loss++;
                teamX.rank = 9;
            }
        }
    }
    
    private void SimulateRound11(bool cont)
    {
        // 2 games: gameList[34-35] ? winners to gameList[38].x/y, losers eliminated (rank 7)
        for (int i = 0; i < 2; i++)
        {
            Team teamX = GetTeamById((int)gameList[i + 34].x);
            Team teamY = GetTeamById((int)gameList[i + 34].y);
            
            bool xWon;
            if (ProcessGame(ref teamX, ref teamY, cont, out xWon))
            {
                if (i == 0)
                {
                    gameList[38].x = xWon ? teamX.id : teamY.id;
                }
                else
                {
                    gameList[38].y = xWon ? teamX.id : teamY.id;
                }
                continue;
            }
            
            bool xWins = SimulateGame(teamX, teamY, cont);
            
            if (i == 0)
            {
                gameList[38].x = xWins ? teamX.id : teamY.id;
            }
            else
            {
                gameList[38].y = xWins ? teamX.id : teamY.id;
            }
            
            if (xWins)
            {
                teamX.wins++;
                teamY.loss++;
                teamY.rank = 7;
            }
            else
            {
                teamY.wins++;
                teamX.loss++;
                teamX.rank = 7;
            }
        }
    }
    
    private void SimulateRound12(bool cont)
    {
        // 1 game: gameList[36] ? winner to gameList[41].x, loser to gameList[40].x
        Team teamX = GetTeamById((int)gameList[36].x);
        Team teamY = GetTeamById((int)gameList[36].y);
        
        bool xWon;
        if (ProcessGame(ref teamX, ref teamY, cont, out xWon))
        {
            gameList[41].x = xWon ? teamX.id : teamY.id;
            gameList[40].x = xWon ? teamY.id : teamX.id;
            return;
        }
        
        bool xWins = SimulateGame(teamX, teamY, cont);
        
        gameList[41].x = xWins ? teamX.id : teamY.id;
        gameList[40].x = xWins ? teamY.id : teamX.id;
        
        if (xWins)
        {
            teamX.wins++;
            teamY.loss++;
        }
        else
        {
            teamY.wins++;
            teamX.loss++;
        }
    }
    
    private void SimulateRound13(bool cont)
    {
        // 1 game: gameList[37] ? winner to gameList[40].y, loser to gameList[39].x
        Team teamX = GetTeamById((int)gameList[37].x);
        Team teamY = GetTeamById((int)gameList[37].y);
        
        bool xWon;
        if (ProcessGame(ref teamX, ref teamY, cont, out xWon))
        {
            gameList[40].y = xWon ? teamX.id : teamY.id;
            gameList[39].x = xWon ? teamY.id : teamX.id;
            return;
        }
        
        bool xWins = SimulateGame(teamX, teamY, cont);
        
        gameList[40].y = xWins ? teamX.id : teamY.id;
        gameList[39].x = xWins ? teamY.id : teamX.id;
        
        if (xWins)
        {
            teamX.wins++;
            teamY.loss++;
        }
        else
        {
            teamY.wins++;
            teamX.loss++;
        }
    }
    
    private void SimulateRound14(bool cont)
    {
        // 1 game: gameList[38] ? winner to gameList[39].y, loser eliminated (rank 6)
        Team teamX = GetTeamById((int)gameList[38].x);
        Team teamY = GetTeamById((int)gameList[38].y);
        
        bool xWon;
        if (ProcessGame(ref teamX, ref teamY, cont, out xWon))
        {
            gameList[39].y = xWon ? teamX.id : teamY.id;
            return;
        }
        
        bool xWins = SimulateGame(teamX, teamY, cont);
        
        gameList[39].y = xWins ? teamX.id : teamY.id;
        
        if (xWins)
        {
            teamX.wins++;
            teamY.loss++;
            teamY.rank = 6;
        }
        else
        {
            teamY.wins++;
            teamX.loss++;
            teamX.rank = 6;
        }
    }
    
    private void SimulateRound15(bool cont)
    {
        // 2 games: gameList[39] (rank 5) and gameList[40] (to gameList[41-42])
        Team team1X = GetTeamById((int)gameList[39].x);
        Team team1Y = GetTeamById((int)gameList[39].y);
        
        bool game1XWon;
        if (!ProcessGame(ref team1X, ref team1Y, cont, out game1XWon))
        {
            game1XWon = SimulateGame(team1X, team1Y, cont);
            
            if (game1XWon)
            {
                team1X.wins++;
                team1Y.loss++;
                team1Y.rank = 5;
            }
            else
            {
                team1Y.wins++;
                team1X.loss++;
                team1X.rank = 5;
            }
        }
        
        gameList[42].y = game1XWon ? team1X.id : team1Y.id;
        
        Team team2X = GetTeamById((int)gameList[40].x);
        Team team2Y = GetTeamById((int)gameList[40].y);
        
        bool game2XWon;
        if (!ProcessGame(ref team2X, ref team2Y, cont, out game2XWon))
        {
            game2XWon = SimulateGame(team2X, team2Y, cont);
            
            if (game2XWon)
            {
                team2X.wins++;
                team2Y.loss++;
            }
            else
            {
                team2Y.wins++;
                team2X.loss++;
            }
        }
        
        gameList[41].y = game2XWon ? team2X.id : team2Y.id;
        gameList[42].x = game2XWon ? team2Y.id : team2X.id;
    }
    
    private void SimulateRound16(bool cont)
    {
        // 1 game: gameList[41] ? winner to gameList[44].x, loser to gameList[43].x
        Team teamX = GetTeamById((int)gameList[41].x);
        Team teamY = GetTeamById((int)gameList[41].y);
        
        bool xWon;
        if (ProcessGame(ref teamX, ref teamY, cont, out xWon))
        {
            gameList[44].x = xWon ? teamX.id : teamY.id;
            gameList[43].x = xWon ? teamY.id : teamX.id;
            return;
        }
        
        bool xWins = SimulateGame(teamX, teamY, cont);
        
        gameList[44].x = xWins ? teamX.id : teamY.id;
        gameList[43].x = xWins ? teamY.id : teamX.id;
        
        if (xWins)
        {
            teamX.wins++;
            teamY.loss++;
        }
        else
        {
            teamY.wins++;
            teamX.loss++;
        }
    }
    
    private void SimulateRound17(bool cont)
    {
        // 1 game: gameList[42] ? winner to gameList[43].y, loser eliminated (rank 4)
        Team teamX = GetTeamById((int)gameList[42].x);
        Team teamY = GetTeamById((int)gameList[42].y);
        
        bool xWon;
        if (ProcessGame(ref teamX, ref teamY, cont, out xWon))
        {
            gameList[43].y = xWon ? teamX.id : teamY.id;
            return;
        }
        
        bool xWins = SimulateGame(teamX, teamY, cont);
        
        gameList[43].y = xWins ? teamX.id : teamY.id;
        
        if (xWins)
        {
            teamX.wins++;
            teamY.loss++;
            teamY.rank = 4;
        }
        else
        {
            teamY.wins++;
            teamX.loss++;
            teamX.rank = 4;
        }
    }
    
    private void SimulateRound18(bool cont)
    {
        // 1 game: gameList[43] ? winner to gameList[44].y, loser eliminated (rank 3)
        Team teamX = GetTeamById((int)gameList[43].x);
        Team teamY = GetTeamById((int)gameList[43].y);
        
        bool xWon;
        if (ProcessGame(ref teamX, ref teamY, cont, out xWon))
        {
            gameList[44].y = xWon ? teamX.id : teamY.id;
            return;
        }
        
        bool xWins = SimulateGame(teamX, teamY, cont);
        
        gameList[44].y = xWins ? teamX.id : teamY.id;
        
        if (xWins)
        {
            teamX.wins++;
            teamY.loss++;
            teamY.rank = 3;
        }
        else
        {
            teamY.wins++;
            teamX.loss++;
            teamX.rank = 3;
        }
    }
    
    private void SimulateRound19(bool cont)
    {
        // 1 game: gameList[44] ? winner to gameList[45].x (rank 1), loser gets rank 2
        Team teamX = GetTeamById((int)gameList[44].x);
        Team teamY = GetTeamById((int)gameList[44].y);
        
        bool xWon;
        if (ProcessGame(ref teamX, ref teamY, cont, out xWon))
        {
            gameList[45].x = xWon ? teamX.id : teamY.id;
            
            if (xWon)
            {
                teamX.rank = 1;
                teamY.rank = 2;
            }
            else
            {
                teamY.rank = 1;
                teamX.rank = 2;
            }
            
            teamX.tourRecord = new Vector2(teamX.wins, teamX.loss);
            teamY.tourRecord = new Vector2(teamY.wins, teamY.loss);
            return;
        }
        
        bool xWins = SimulateGame(teamX, teamY, cont);
        
        gameList[45].x = xWins ? teamX.id : teamY.id;
        
        if (xWins)
        {
            teamX.wins++;
            teamX.rank = 1;
            teamY.loss++;
            teamY.rank = 2;
        }
        else
        {
            teamY.wins++;
            teamY.rank = 1;
            teamX.loss++;
            teamX.rank = 2;
        }
        
        // Update tour records
        teamX.tourRecord = new Vector2(teamX.wins, teamX.loss);
        teamY.tourRecord = new Vector2(teamY.wins, teamY.loss);
    }
    
    private void SimulateRound20(bool cont)
    {
        // This round shouldn't simulate - champion already determined
        Debug.Log("[TripleK] Round 20 - Tournament complete");
    }
    
    #endregion
    
    /// <summary>
    /// Simulate a single game between two teams
    /// </summary>
    private bool SimulateGame(Team teamX, Team teamY, bool cont)
    {
        // Safety check
        if (teamX == null || teamY == null)
        {
            Debug.LogError($"[TripleK] SimulateGame called with null team! X={teamX?.name ?? "NULL"}, Y={teamY?.name ?? "NULL"}");
            return false;
        }
        
        // If this is the player's game result, use actual scores
        if (cont && (gsp.redTeamName == teamX.name || gsp.yellowTeamName == teamX.name))
        {
            bool playerIsX = (gsp.redTeamName == teamX.name || gsp.yellowTeamName == teamX.name);
            bool playerIsRed = (gsp.redTeamName == teamX.name || gsp.redTeamName == teamY.name);
            
            if (playerIsX)
            {
                return playerIsRed ? (gsp.redScore > gsp.yellowScore) : (gsp.redScore < gsp.yellowScore);
            }
            else
            {
                return playerIsRed ? (gsp.redScore < gsp.yellowScore) : (gsp.redScore > gsp.yellowScore);
            }
        }
        
        // AI vs AI: use strength-based random
        return Random.Range(0, teamX.strength) > Random.Range(0, teamY.strength);
    }
    
    /// <summary>
    /// Helper: Check if this game involves the player and should be skipped (already played)
    /// </summary>
    private bool ShouldSkipPlayerGame(Team teamX, Team teamY, bool cont)
    {
        return cont && (teamX.player || teamY.player);
    }
    
    /// <summary>
    /// Helper: Determine winner from current win/loss stats (for already-played player games)
    /// </summary>
    private bool DetermineWinnerFromStats(Team teamX, Team teamY)
    {
        // X won if X has more wins OR Y has more losses
        return (teamX.wins > teamY.wins || teamY.loss > teamX.loss);
    }
    
    /// <summary>
    /// Helper: Universal game processing with null/player checks
    /// Returns true if game should be skipped (null teams or player game already played)
    /// </summary>
    private bool ProcessGame(ref Team teamX, ref Team teamY, bool cont, out bool xWon)
    {
        xWon = false;
        
        // Null check
        if (teamX == null || teamY == null)
        {
            Debug.LogWarning($"[TripleK] Round {playoffRound}: Null team detected");
            return true; // Skip this game
        }
        
        // Check if player's game (already played)
        if (ShouldSkipPlayerGame(teamX, teamY, cont))
        {
            Debug.Log($"[TripleK] Round {playoffRound}: Skipping player game (already played)");
            xWon = DetermineWinnerFromStats(teamX, teamY);
            return true; // Skip simulation but use xWon for bracket
        }
        
        // Game needs to be simulated
        return false;
    }
    
    /// <summary>
    /// Get team by ID
    /// </summary>
    private Team GetTeamById(int id)
    {
        for (int i = 0; i < teams.Length; i++)
        {
            if (teams[i].id == id)
                return teams[i];
        }
        return null;
    }
    
    #endregion
    
    #region Display & UI
    
    /// <summary>
    /// Set up playoff display for current round
    /// </summary>
    public void SetPlayoffs()
    {
        Debug.Log($"[TripleK] SetPlayoffs - Round {playoffRound}");
        
        bool playerGame = false;
        vsDisplayTitle.text = "Next Game";
        
        // Display logic for each round
        switch (playoffRound)
        {
            case 1: playerGame = DisplayRound1(); break;
            case 2: playerGame = DisplayRound2(); break;
            case 3: playerGame = DisplayRound3(); break;
            case 4: playerGame = DisplayRound4(); break;
            case 5: playerGame = DisplayRound5(); break;
            case 6: playerGame = DisplayRound6(); break;
            case 7: playerGame = DisplayRound7(); break;
            case 8: playerGame = DisplayRound8(); break;
            case 9: playerGame = DisplayRound9(); break;
            case 10: playerGame = DisplayRound10(); break;
            case 11: playerGame = DisplayRound11(); break;
            case 12: playerGame = DisplayRound12(); break;
            case 13: playerGame = DisplayRound13(); break;
            case 14: playerGame = DisplayRound14(); break;
            case 15: playerGame = DisplayRound15(); break;
            case 16: playerGame = DisplayRound16(); break;
            case 17: playerGame = DisplayRound17(); break;
            case 18: playerGame = DisplayRound18(); break;
            case 19: playerGame = DisplayRound19(); break;
            case 20: playerGame = DisplayRound20(); break;
        }
        
        // Configure buttons based on player involvement
        if (playerGame)
        {
            simInProgress = false;
            vsDisplayGO.SetActive(true);
            playButton.gameObject.SetActive(true);
            simButton.gameObject.SetActive(true);
            contButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);
        }
        else if (playoffRound < 20)
        {
            vsDisplayGO.SetActive(false);
            nextButton.gameObject.SetActive(false);
            simButton.gameObject.SetActive(false);
            contButton.gameObject.SetActive(false);
            StartCoroutine(SimToFinals());
        }
        else
        {
            nextButton.gameObject.SetActive(true);
            simButton.gameObject.SetActive(false);
            vsDisplayGO.SetActive(true);
        }
        
        StartCoroutine(RefreshPlayoffPanel());
        cm.SaveTournyState();
    }
    
    #region Display Methods (one per round)
    
    private bool DisplayRound1()
    {
        heading.text = "Triple Knockout - Round 1";
        bool playerGame = false;
        
        for (int i = 0; i < winnersDisplay1.Length; i++)
        {
            winnersDisplay1[i].name.text = teams[i].name;
            winnersDisplay1[i].rank.text = FormatLossRecord(teams[i].loss);
            
            if (i % 2 == 0 && gameList[i / 2].x == playerTeam)
            {
                oppTeam = (int)gameList[i / 2].y;
                winnersDisplay1[i].bg.GetComponent<Image>().color = yellow;
                playerGame = true;
            }
            if (i % 2 == 1 && gameList[(i - 1) / 2].y == playerTeam)
            {
                oppTeam = (int)gameList[(i - 1) / 2].x;
                winnersDisplay1[i].bg.GetComponent<Image>().color = yellow;
                playerGame = true;
            }
        }
        
        SetVSDisplay(playerTeam, oppTeam);
        ActivateBracket(winnersBracket, 0, 0);
        SetScrollPosition(0, 1);
        
        return playerGame;
    }
    
    private bool DisplayRound2()
    {
        heading.text = "Loser's Bracket - Draw 2";
        bool playerGame = false;
        
        for (int i = 0; i < losersDisplayA2.Length; i++)
        {
            Team team = GetTeamById((int)(i % 2 == 0 ? gameList[(i / 2) + 8].x : gameList[((i - 1) / 2) + 8].y));
            if (team != null)
            {
                losersDisplayA2[i].name.text = team.name;
                losersDisplayA2[i].rank.text = FormatLossRecord(team.loss);
                
                if (team.player)
                {
                    oppTeam = (int)(i % 2 == 0 ? gameList[(i / 2) + 8].y : gameList[((i - 1) / 2) + 8].x);
                    losersDisplayA2[i].bg.GetComponent<Image>().color = yellow;
                    playerGame = true;
                }
            }
        }
        
        SetVSDisplay(playerTeam, oppTeam);
        ActivateBracket(losersBracket1, 0, 0);
        SetScrollPosition(0, 0.56f);
        
        return playerGame;
    }
    
    private bool DisplayRound3()
    {
        heading.text = "Winner's Bracket - Draw 3";
        bool playerGame = false;
        
        for (int i = 0; i < winnersDisplay3.Length; i++)
        {
            Team team = GetTeamById((int)(i % 2 == 0 ? gameList[(i / 2) + 12].x : gameList[((i - 1) / 2) + 12].y));
            if (team != null)
            {
                winnersDisplay3[i].name.text = team.name;
                winnersDisplay3[i].rank.text = FormatLossRecord(team.loss);
                
                if (team.player)
                {
                    oppTeam = (int)(i % 2 == 0 ? gameList[(i / 2) + 12].y : gameList[((i - 1) / 2) + 12].x);
                    winnersDisplay3[i].bg.GetComponent<Image>().color = yellow;
                    playerGame = true;
                }
            }
        }
        
        SetVSDisplay(playerTeam, oppTeam);
        ActivateBracket(winnersBracket, 1, 1);
        SetScrollPosition(0, 1);
        
        return playerGame;
    }
    
    private bool DisplayRound4()
    {
        heading.text = "Loser's Bracket - Draw 4";
        bool playerGame = false;
        
        for (int i = 0; i < losersDisplayA4.Length; i++)
        {
            losersDisplayA4[i].panel.transform.parent.gameObject.SetActive(true);
            Team team = GetTeamById((int)(i % 2 == 0 ? gameList[(i / 2) + 16].x : gameList[((i - 1) / 2) + 16].y));
            if (team != null)
            {
                losersDisplayA4[i].name.text = team.name;
                losersDisplayA4[i].rank.text = FormatLossRecord(team.loss);
                
                if (team.player)
                {
                    oppTeam = (int)(i % 2 == 0 ? gameList[(i / 2) + 16].y : gameList[((i - 1) / 2) + 16].x);
                    losersDisplayA4[i].bg.GetComponent<Image>().color = yellow;
                    playerGame = true;
                }
            }
        }
        
        SetVSDisplay(playerTeam, oppTeam);
        ActivateBracket(losersBracket1, 1, 1);
        SetScrollPosition(0, 0.74f);
        
        return playerGame;
    }
    
    private bool DisplayRound5()
    {
        heading.text = "Knockout Bracket - Draw 5";
        bool playerGame = false;
        
        for (int i = 0; i < losersDisplayB5.Length; i++)
        {
            Team team = GetTeamById((int)(i % 2 == 0 ? gameList[(i / 2) + 20].x : gameList[((i - 1) / 2) + 20].y));
            if (team != null)
            {
                losersDisplayB5[i].name.text = team.name;
                losersDisplayB5[i].rank.text = FormatLossRecord(team.loss);
                
                if (team.player)
                {
                    oppTeam = (int)(i % 2 == 0 ? gameList[(i / 2) + 20].y : gameList[((i - 1) / 2) + 20].x);
                    losersDisplayB5[i].bg.GetComponent<Image>().color = yellow;
                    playerGame = true;
                }
            }
        }
        
        SetVSDisplay(playerTeam, oppTeam);
        ActivateBracket(losersBracket2, 0, 0);
        SetScrollPosition(0, 0.33f);
        
        return playerGame;
    }
    
    private bool DisplayRound6()
    {
        heading.text = "Knockout Bracket - Draw 6";
        bool playerGame = false;
        
        for (int i = 0; i < losersDisplayB6.Length; i++)
        {
            Team team = GetTeamById((int)(i % 2 == 0 ? gameList[(i / 2) + 24].x : gameList[((i - 1) / 2) + 24].y));
            if (team != null)
            {
                losersDisplayB6[i].name.text = team.name;
                losersDisplayB6[i].rank.text = FormatLossRecord(team.loss);
                
                if (team.player)
                {
                    oppTeam = (int)(i % 2 == 0 ? gameList[(i / 2) + 24].y : gameList[((i - 1) / 2) + 24].x);
                    losersDisplayB6[i].bg.GetComponent<Image>().color = yellow;
                    playerGame = true;
                }
            }
        }
        
        SetVSDisplay(playerTeam, oppTeam);
        ActivateBracket(losersBracket2, 1, 1);
        SetScrollPosition(0, 0.3f);
        
        return playerGame;
    }
    
    private bool DisplayRound7()
    {
        heading.text = "Winner's Bracket - Draw 7";
        bool playerGame = false;
        
        for (int i = 0; i < winnersDisplay7.Length; i++)
        {
            Team team = GetTeamById((int)(i % 2 == 0 ? gameList[(i / 2) + 26].x : gameList[((i - 1) / 2) + 26].y));
            if (team != null)
            {
                winnersDisplay7[i].name.text = team.name;
                winnersDisplay7[i].rank.text = FormatLossRecord(team.loss);
                
                if (team.player)
                {
                    oppTeam = (int)(i % 2 == 0 ? gameList[(i / 2) + 26].y : gameList[((i - 1) / 2) + 26].x);
                    winnersDisplay7[i].bg.GetComponent<Image>().color = yellow;
                    playerGame = true;
                }
            }
        }
        
        SetVSDisplay(playerTeam, oppTeam);
        ActivateBracket(winnersBracket, 2, 2);
        SetScrollPosition(0.35f, 1);
        
        return playerGame;
    }
    
    private bool DisplayRound8()
    {
        heading.text = "Loser's Bracket - Draw 8";
        bool playerGame = false;
        
        for (int i = 0; i < losersDisplayA8.Length; i++)
        {
            Team team = GetTeamById((int)(i % 2 == 0 ? gameList[(i / 2) + 28].x : gameList[((i - 1) / 2) + 28].y));
            if (team != null)
            {
                losersDisplayA8[i].name.text = team.name;
                losersDisplayA8[i].rank.text = FormatLossRecord(team.loss);
                
                if (team.player)
                {
                    oppTeam = (int)(i % 2 == 0 ? gameList[(i / 2) + 28].y : gameList[((i - 1) / 2) + 28].x);
                    losersDisplayA8[i].bg.GetComponent<Image>().color = yellow;
                    playerGame = true;
                }
            }
        }
        
        SetVSDisplay(playerTeam, oppTeam);
        ActivateBracket(losersBracket1, 2, 2);
        SetScrollPosition(0.3f, 0.77f);
        
        return playerGame;
    }
    
    private bool DisplayRound9()
    {
        heading.text = "Loser's Bracket - Draw 9";
        bool playerGame = false;
        
        for (int i = 0; i < 4; i++)
        {
            Team team = GetTeamById((int)(i % 2 == 0 ? gameList[(i / 2) + 30].x : gameList[((i - 1) / 2) + 30].y));
            if (team != null)
            {
                losersDisplayA9[i].name.text = team.name;
                losersDisplayA9[i].rank.text = FormatLossRecord(team.loss);
                
                if (team.player)
                {
                    oppTeam = (int)(i % 2 == 0 ? gameList[(i / 2) + 30].y : gameList[((i - 1) / 2) + 30].x);
                    losersDisplayA9[i].bg.GetComponent<Image>().color = yellow;
                    playerGame = true;
                }
            }
        }
        
        SetVSDisplay(playerTeam, oppTeam);
        ActivateBracket(losersBracket1, 3, 3);
        SetScrollPosition(0.63f, 1);
        
        return playerGame;
    }
    
    private bool DisplayRound10()
    {
        heading.text = "Knockout Bracket - Round 10";
        bool playerGame = false;
        
        for (int i = 0; i < losersDisplayB10.Length; i++)
        {
            losersDisplayB10[i].panel.transform.parent.gameObject.SetActive(true);
            Team team = GetTeamById((int)(i % 2 == 0 ? gameList[(i / 2) + 32].x : gameList[((i - 1) / 2) + 32].y));
            if (team != null)
            {
                losersDisplayB10[i].name.text = team.name;
                losersDisplayB10[i].rank.text = FormatLossRecord(team.loss);
                
                if (team.player)
                {
                    oppTeam = (int)(i % 2 == 0 ? gameList[(i / 2) + 32].y : gameList[((i - 1) / 2) + 32].x);
                    losersDisplayB10[i].bg.GetComponent<Image>().color = yellow;
                    playerGame = true;
                }
            }
        }
        
        SetVSDisplay(playerTeam, oppTeam);
        ActivateBracket(losersBracket2, 2, 2);
        SetScrollPosition(0.3f, 0.76f);
        
        return playerGame;
    }
    
    private bool DisplayRound11()
    {
        heading.text = "Knockout Bracket - Draw 11";
        bool playerGame = false;
        
        for (int i = 0; i < losersDisplayB11.Length; i++)
        {
            Team team = GetTeamById((int)(i % 2 == 0 ? gameList[(i / 2) + 34].x : gameList[((i - 1) / 2) + 34].y));
            if (team != null)
            {
                losersDisplayB11[i].name.text = team.name;
                losersDisplayB11[i].rank.text = FormatLossRecord(team.loss);
                
                if (team.player)
                {
                    oppTeam = (int)(i % 2 == 0 ? gameList[(i / 2) + 34].y : gameList[((i - 1) / 2) + 34].x);
                    losersDisplayB11[i].bg.GetComponent<Image>().color = yellow;
                    playerGame = true;
                }
            }
        }
        
        SetVSDisplay(playerTeam, oppTeam);
        ActivateBracket(losersBracket2, 3, 3);
        SetScrollPosition(0.61f, 1);
        
        return playerGame;
    }
    
    private bool DisplayRound12()
    {
        heading.text = "Winner's Bracket - Draw 12";
        bool playerGame = false;
        
        for (int i = 0; i < 2; i++)
        {
            Team team = GetTeamById((int)(i == 0 ? gameList[36].x : gameList[36].y));
            if (team != null)
            {
                winnersDisplay12[i].name.text = team.name;
                winnersDisplay12[i].rank.text = FormatLossRecord(team.loss);
                
                if (team.player)
                {
                    oppTeam = (int)(i == 0 ? gameList[36].y : gameList[36].x);
                    winnersDisplay12[i].bg.GetComponent<Image>().color = yellow;
                    playerGame = true;
                }
            }
        }
        
        SetVSDisplay(playerTeam, oppTeam);
        ActivateBracket(winnersBracket, -1, -1); // All visible
        SetScrollPosition(0.75f, 0.76f);
        
        return playerGame;
    }
    
    private bool DisplayRound13()
    {
        heading.text = "Loser's Bracket - Draw 13";
        bool playerGame = false;
        
        for (int i = 0; i < 2; i++)
        {
            Team team = GetTeamById((int)(i == 0 ? gameList[37].x : gameList[37].y));
            if (team != null)
            {
                losersDisplayA13[i].name.text = team.name;
                losersDisplayA13[i].rank.text = FormatLossRecord(team.loss);
                
                if (team.player)
                {
                    oppTeam = (int)(i == 0 ? gameList[37].y : gameList[37].x);
                    losersDisplayA13[i].bg.GetComponent<Image>().color = yellow;
                    playerGame = true;
                }
            }
        }
        
        SetVSDisplay(playerTeam, oppTeam);
        ActivateBracket(losersBracket1, -1, -1);
        SetScrollPosition(0.93f, 1);
        
        return playerGame;
    }
    
    private bool DisplayRound14()
    {
        heading.text = "Knockout Bracket - Draw 14";
        bool playerGame = false;
        
        for (int i = 0; i < 2; i++)
        {
            Team team = GetTeamById((int)(i == 0 ? gameList[38].x : gameList[38].y));
            if (team != null)
            {
                losersDisplayB14[i].name.text = team.name;
                losersDisplayB14[i].rank.text = FormatLossRecord(team.loss);
                
                if (team.player)
                {
                    oppTeam = (int)(i == 0 ? gameList[38].y : gameList[38].x);
                    losersDisplayB14[i].bg.GetComponent<Image>().color = yellow;
                    playerGame = true;
                }
            }
        }
        
        SetVSDisplay(playerTeam, oppTeam);
        ActivateBracket(losersBracket2, -1, -1);
        SetScrollPosition(0.93f, 1);
        
        return playerGame;
    }
    
    private bool DisplayRound15()
    {
        heading.text = "Final Bracket - Draw 15";
        bool playerGame = false;
        
        // Display logic for finals round 15
        // (Simplified - would need full implementation)
        
        ActivateBracket(finalsBracket, 0, 0);
        SetScrollPosition(0, 0.9f);
        
        return playerGame;
    }
    
    private bool DisplayRound16()
    {
        heading.text = "Final Bracket - Draw 16";
        bool playerGame = false;
        
        ActivateBracket(finalsBracket, 2, 2);
        SetScrollPosition(0, 0.92f);
        
        return playerGame;
    }
    
    private bool DisplayRound17()
    {
        heading.text = "Final Bracket - Draw 17";
        bool playerGame = false;
        
        ActivateBracket(finalsBracket, 3, 3);
        SetScrollPosition(0.3f, 0.9f);
        
        return playerGame;
    }
    
    private bool DisplayRound18()
    {
        heading.text = "Final Bracket - Draw 18";
        bool playerGame = false;
        
        ActivateBracket(finalsBracket, 3, 3);
        SetScrollPosition(0.61f, 0.95f);
        
        return playerGame;
    }
    
    private bool DisplayRound19()
    {
        heading.text = "Final Bracket - Draw 19";
        bool playerGame = false;
        
        ActivateBracket(finalsBracket, 4, 4);
        SetScrollPosition(0.92f, 1);
        
        return playerGame;
    }
    
    private bool DisplayRound20()
    {
        heading.text = "Finals";
        
        Team champion = GetTeamById((int)gameList[45].x);
        if (champion != null)
        {
            finalsDisplay20[0].name.text = champion.name;
            finalsDisplay20[0].rank.text = FormatLossRecord(champion.loss);
        }
        
        ActivateBracket(finalsBracket, -1, -1);
        SetScrollPosition(0.95f, 0.96f);
        nextButton.gameObject.SetActive(true);
        simButton.gameObject.SetActive(false);
        
        return false;
    }
    
    #endregion
    
    /// <summary>
    /// Display results after simulating a round
    /// </summary>
    private void DisplayRoundResults()
    {
        Debug.Log($"[TripleK] DisplayRoundResults - Round {playoffRound}");
        
        // Update display to show results (winners highlighted)
        // This would be similar to DisplayRoundX but with highlighting
        
        // Update tour records
        for (int i = 0; i < teams.Length; i++)
        {
            teams[i].tourRecord.x = teams[i].wins;
            teams[i].tourRecord.y = teams[i].loss;
        }
        
        // Check if player is still in tournament
        bool playerActive = false;
        for (int i = 0; i < teams.Length; i++)
        {
            if (teams[i].player && teams[i].loss < 3)
            {
                playerActive = true;
                break;
            }
        }
        
        if (!playerActive && playoffRound < 19)
        {
            vsDisplayTitle.text = "XXX";
            vsDisplay[0].rank.text = "KO";
            vsDisplay[0].name.text = teams[playerTeam].name;
            vsDisplayVS.text = "Is";
            vsDisplay[1].rank.text = "-";
            vsDisplay[1].name.text = "Knocked Out!";
        }
        
        playoffRound++;
        
        if (simInProgress)
        {
            simButton.gameObject.SetActive(false);
            contButton.gameObject.SetActive(false);
        }
        else
        {
            simButton.gameObject.SetActive(false);
            contButton.gameObject.SetActive(true);
        }
        
        SetPlayoffs();
    }
    
    /// <summary>
    /// Format loss record as "OOO", "OOX", "OXX", "XXX"
    /// </summary>
    private string FormatLossRecord(int losses)
    {
        switch (losses)
        {
            case 0: return "OOO";
            case 1: return "OOX";
            case 2: return "OXX";
            default: return "XXX";
        }
    }
    
    /// <summary>
    /// Set VS display panel
    /// </summary>
    private void SetVSDisplay(int player, int opp)
    {
        Team playerTeamObj = GetTeamById(player);
        Team oppTeamObj = GetTeamById(opp);
        
        if (playerTeamObj != null)
        {
            vsDisplay[0].name.text = playerTeamObj.name;
            vsDisplay[0].rank.text = FormatLossRecord(playerTeamObj.loss);
        }
        
        if (oppTeamObj != null)
        {
            vsDisplay[1].name.text = oppTeamObj.name;
            vsDisplay[1].rank.text = FormatLossRecord(oppTeamObj.loss);
        }
    }
    
    /// <summary>
    /// Activate a bracket and set visible children
    /// </summary>
    private void ActivateBracket(GameObject bracket, int minChild, int maxChild)
    {
        winnersBracket.SetActive(bracket == winnersBracket);
        losersBracket1.SetActive(bracket == losersBracket1);
        losersBracket2.SetActive(bracket == losersBracket2);
        finalsBracket.SetActive(bracket == finalsBracket);
        
        if (minChild >= 0 && maxChild >= 0)
        {
            for (int i = 0; i < bracket.transform.childCount; i++)
            {
                bracket.transform.GetChild(i).gameObject.SetActive(i >= minChild && i <= maxChild);
            }
        }
        else
        {
            // Activate all
            for (int i = 0; i < bracket.transform.childCount; i++)
            {
                bracket.transform.GetChild(i).gameObject.SetActive(true);
            }
        }
    }
    
    /// <summary>
    /// Set scroll position
    /// </summary>
    private void SetScrollPosition(float horiz, float vert)
    {
        horizScrollBar.value = horiz;
        vertScrollBar.value = vert;
    }
    
    /// <summary>
    /// Refresh UI elements
    /// </summary>
    private IEnumerator RefreshPlayoffPanel()
    {
        yield return new WaitForEndOfFrame();
        
        // Refresh ContentSizeFitters
        GameObject activeBracket = winnersBracket.activeSelf ? winnersBracket :
                                   losersBracket1.activeSelf ? losersBracket1 :
                                   losersBracket2.activeSelf ? losersBracket2 :
                                   finalsBracket;
        
        for (int i = 0; i < activeBracket.transform.childCount; i++)
        {
            Transform child = activeBracket.transform.GetChild(i);
            for (int j = 0; j < child.childCount; j++)
            {
                Transform subchild = child.GetChild(j);
                for (int k = 1; k < 3; k++)
                {
                    if (k < subchild.childCount)
                    {
                        ContentSizeFitter fitter = subchild.GetChild(k).GetComponent<ContentSizeFitter>();
                        if (fitter != null)
                        {
                            fitter.enabled = false;
                            yield return new WaitForEndOfFrame();
                            fitter.enabled = true;
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Auto-simulate to finals if no player game
    /// </summary>
    private IEnumerator SimToFinals()
    {
        simInProgress = true;
        yield return new WaitForSeconds(0.001f);
        
        vsDisplayGO.SetActive(false);
        nextButton.gameObject.SetActive(false);
        simButton.gameObject.SetActive(false);
        contButton.gameObject.SetActive(false);
        
        OnSim();
        
        yield return new WaitForSeconds(0.001f);
        SetPlayoffs();
    }
    
    #endregion
    
    #region Player Actions
    
    public void PlayRound()
    {
        gsp.TournyKOSetup();
        SceneManager.LoadScene("End_Menu_Tourny_1");
    }
    
    public void TournyComplete()
    {
        gsp.teams = teams;
        
        // Restore pre-tournament records and add tournament results
        for (int i = 0; i < teams.Length; i++)
        {
            teams[i].wins += (int)cm.teamRecords[i].x;
            teams[i].loss += (int)cm.teamRecords[i].y;
            teams[i].earnings += cm.teamRecords[i].z;
            teams[i].tourRecord.x += cm.tourRecords[i].x;
            teams[i].tourRecord.y += cm.tourRecords[i].y;
            teams[i].tourPoints += cm.tourRecords[i].z;
        }
        
        // Award prize money
        CalculatePrizePayouts();
        
        // Reset state
        gsp.draw = 0;
        gsp.playoffRound = 0;
        gsp.tournyInProgress = false;
        
        Debug.Log($"[TripleK] Tournament complete - Career earnings: ${gsp.tournyEarnings}");
        
        cm.TournyResults();
        SceneManager.LoadScene("Arena_Selector");
    }
    
    /// <summary>
    /// Calculate and award prize money based on final rankings
    /// </summary>
    private void CalculatePrizePayouts()
    {
        float p = 1.4f;
        float totalTeams = teams.Length - 5f;
        
        for (int i = 0; i < teams.Length; i++)
        {
            float prizePayout = 0;
            
            switch (teams[i].rank)
            {
                case 1: prizePayout = gsp.prize * 0.5f; break;
                case 2: prizePayout = gsp.prize * 0.25f; break;
                case 3: prizePayout = gsp.prize * 0.15f; break;
                case 4: prizePayout = gsp.prize * 0.075f; break;
                case 5: prizePayout = gsp.prize * 0.038f; break;
                default:
                    prizePayout = ((Mathf.Pow(p, totalTeams - ((teams[i].rank - 1) + 1))) / 
                                   (Mathf.Pow(p, totalTeams) - 1f)) * (gsp.prize * 0.15f) * (p - 1);
                    break;
            }
            
            teams[i].earnings += prizePayout;
            
            if (teams[i].player)
            {
                gsp.tournyEarnings = prizePayout;
                gsp.tournyCash = prizePayout;
                
                vsDisplayGO.SetActive(true);
                vsDisplayTitle.text = "Results";
                vsDisplayVS.text = "Wins";
                vsDisplay[0].name.text = teams[i].name;
                vsDisplay[0].rank.text = teams[i].rank.ToString();
                vsDisplay[1].name.text = "$" + prizePayout.ToString("n0");
                vsDisplay[1].rank.gameObject.SetActive(false);
            }
            
            Debug.Log($"[TripleK] Team {teams[i].name} - Rank {teams[i].rank} - Payout ${prizePayout:n0}");
        }
        
        careerEarningsText.text = "$ " + gsp.tournyEarnings.ToString("n0");
    }
    
    public void Menu()
    {
        cm.SaveTournyState();
        SceneManager.LoadScene("SplashMenu");
    }
    
    #endregion
}
