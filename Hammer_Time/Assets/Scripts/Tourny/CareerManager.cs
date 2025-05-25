using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TigerForge;
using System.Net.Security;
using System;

public class CareerManager : MonoBehaviour
{
    public static CareerManager instance;
    TournySettings ts;
    EasyFileSave myFile;
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

    }

    private void Start()
    {
        tourRankList = new List<TourStandings_List>();
        provRankList = new List<Standings_List>();
        tournyResults = new List<int>();
        
        LoadCareer();
    }

    public void LoadSettings()
    {
        cs = FindObjectOfType<CareerSettings>();

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
        if (gsp == null) gsp = FindObjectOfType<GameSettingsPersist>();
        if (tSel == null) tSel = FindObjectOfType<TournySelector>();
        if (slm == null) slm = FindObjectOfType<StorylineManager>();
        if (teamSel == null) teamSel = FindObjectOfType<TeamMenu>();
        if (pUpM == null) pUpM = FindObjectOfType<SponsorManager>();
        if (gm == null) gm = FindObjectOfType<GameManager>();
        if (em == null) em = FindObjectOfType<EquipmentManager>();

        Debug.Log("Loading in CM 2");

        ClearRankLists();
        InitializeFileSave();

        if (myFile.Load())
        {
            LoadGameProgress(gsp);
            LoadDialogueStatus(slm);

            LoadActivePlayers(teamSel);
            LoadCardData();
            LoadTournamentData(tSel);
            LoadTeamDetails();

            LoadSponsorManager(pUpM);
            LoadEquipment(em);
            LoadTourTeamData();
            LoadTeamsFromSave();
            UpdateCurrentTourny(gsp);
            LoadGameSettings(gsp);

            LoadTournyState();
            LoadCurrentGameState(gsp);
            loadedFromSave = true;
            myFile.Dispose();
        }
        // Additional logic if needed...
    }

    private void ClearRankLists()
    {
        provRankList?.Clear();
        tourRankList?.Clear();
    }

    private void InitializeFileSave()
    {
        myFile = new EasyFileSave("my_player_data");
    }

    private void LoadActivePlayers(TeamMenu teamSel)
    {
        if (teamSel)
        {
            int[] playersIdList = myFile.GetArray<int>("Active Players ID List");
            string[] playersNameList = myFile.GetArray<string>("Active Players Name List");
            int[] playersDrawList = myFile.GetArray<int>("Active Players Draw List");
            int[] playersGuardList = myFile.GetArray<int>("Active Players Guard List");
            int[] playersTakeOutList = myFile.GetArray<int>("Active Players Takeout List");
            int[] playersStrengthList = myFile.GetArray<int>("Active Players Strength List");
            int[] playersEnduroList = myFile.GetArray<int>("Active Players Endurance List");
            int[] playersCohesionList = myFile.GetArray<int>("Active Players Cohesion List");
            int[] playersOppDrawList = myFile.GetArray<int>("Active Players Opp Draw List");
            int[] playersOppGuardList = myFile.GetArray<int>("Active Players Opp Guard List");
            int[] playersOppTakeOutList = myFile.GetArray<int>("Active Players Opp Takeout List");
            int[] playersOppStrengthList = myFile.GetArray<int>("Active Players Opp Strength List");
            int[] playersOppEnduroList = myFile.GetArray<int>("Active Players Opp Endurance List");
            int[] playersOppCohesionList = myFile.GetArray<int>("Active Players Opp Cohesion List");

            for (int i = 0; i < playersIdList.Length; i++)
            {
                teamSel.activePlayers[i].id = playersIdList[i];
                teamSel.activePlayers[i].name = playersNameList[i];
                teamSel.activePlayers[i].draw = playersDrawList[i];
                teamSel.activePlayers[i].guard = playersGuardList[i];
                teamSel.activePlayers[i].takeOut = playersTakeOutList[i];
                teamSel.activePlayers[i].sweepStrength = playersStrengthList[i];
                teamSel.activePlayers[i].sweepEnduro = playersEnduroList[i];
                teamSel.activePlayers[i].sweepCohesion = playersCohesionList[i];
                teamSel.activePlayers[i].oppDraw = playersOppDrawList[i];
                teamSel.activePlayers[i].oppGuard = playersOppGuardList[i];
                teamSel.activePlayers[i].oppTakeOut = playersOppTakeOutList[i];
                teamSel.activePlayers[i].oppStrength = playersOppStrengthList[i];
                teamSel.activePlayers[i].oppEnduro = playersOppEnduroList[i];
                teamSel.activePlayers[i].oppCohesion = playersOppCohesionList[i];
            }
        }
    }

    private void LoadGameProgress(GameSettingsPersist gsp)
    {
        gsp.tournyInProgress = myFile.GetBool("Tourny In Progress");
        gsp.gameInProgress = myFile.GetBool("Game In Progress");
        Debug.Log("gsp.inProgress is " + gsp.tournyInProgress);
        debug = myFile.GetString("Mode");
        week = myFile.GetInt("Week");
        Debug.Log("CM Load Career Week is " + week);
        season = myFile.GetInt("Season");
        playerName = myFile.GetString("Player Name");
        teamName = myFile.GetString("Team Name");
        teamColour = myFile.GetUnityColor("Team Colour");
        playerTeamIndex = myFile.GetInt("Player Team Index");
        record = myFile.GetUnityVector2("Career Record");
        cash = myFile.GetFloat("Career Cash");
        earnings = myFile.GetFloat("Career Earnings");
        provQual = myFile.GetBool("Prov Qual");
        tourQual = myFile.GetBool("Tour Qual");
        xp = myFile.GetFloat("XP");
        skillPoints = myFile.GetInt("Skillpoints");
        level = myFile.GetInt("Level");

        cStats.drawAccuracy = myFile.GetInt("Draw Accuracy");
        cStats.takeOutAccuracy = myFile.GetInt("Take Out Accuracy");
        cStats.guardAccuracy = myFile.GetInt("Guard Accuracy");
        cStats.sweepStrength = myFile.GetInt("Sweep Strength");
        cStats.sweepEndurance = myFile.GetInt("Sweep Endurance");
        cStats.sweepCohesion = myFile.GetInt("Sweep Cohesion");
        gsp.loadGame = myFile.GetBool("Game Load");
    }

    private void LoadDialogueStatus(StorylineManager slm)
    {

        coachDialogue = myFile.GetArray<bool>("Coach Dialogue Played List");
        qualDialogue = myFile.GetArray<bool>("Qualifying Dialogue Played List");
        reviewDialogue = myFile.GetArray<bool>("Review Dialogue Played List");
        introDialogue = myFile.GetArray<bool>("Intro Dialogue Played List");
        helpDialogue = myFile.GetArray<bool>("Help Dialogue Played List");
        strategyDialogue = myFile.GetArray<bool>("Strategy Dialogue Played List");
        storyDialogue = myFile.GetArray<bool>("Story Dialogue Played List");

        if (slm)
            slm.blockIndex = myFile.GetInt("Story Block");

    }

    private void LoadCardData()
    {
        cardPUIDList = myFile.GetArray<int>("Card PowerUp ID List");
        if (cardPUIDList == null) cardPUIDList = new int[0];

        cardSponsorIDList = myFile.GetArray<int>("Card Sponsor ID List");
        if (cardSponsorIDList == null) cardSponsorIDList = new int[0];

        activeCardIDList = myFile.GetArray<int>("Active Card ID List");
        if (activeCardIDList == null) activeCardIDList = new int[0];

        playedCardIDList = myFile.GetArray<int>("Played Card ID List");
        if (playedCardIDList == null) playedCardIDList = new int[0];

        activeCardLengthList = myFile.GetArray<int>("Active Card Length List");
        if (activeCardLengthList == null) activeCardLengthList = new int[0];
    }

    private void LoadTournamentData(TournySelector tSel)
    {
        if (tSel != null)
        {
            int[] provIDList = myFile.GetArray<int>("Prov ID List");
            bool[] provCompleteList = myFile.GetArray<bool>("Prov Complete List");
            int[] tourIDList = myFile.GetArray<int>("Tour ID List");
            bool[] tourCompleteList = myFile.GetArray<bool>("Tour Complete List");
            int[] tourniesIDList = myFile.GetArray<int>("Tournies ID List");
            bool[] tourniesCompleteList = myFile.GetArray<bool>("Tournies Complete List");

            prov = tSel.provQual;
            tour = tSel.tour;
            tournies = tSel.tournies;
            champ = new Tourny[2];
            champ[0] = tSel.tourChampionship;
            champ[1] = tSel.provChampionship;
            champ[0].complete = myFile.GetBool("Tour Championship Complete");
            champ[1].complete = myFile.GetBool("Prov Championship Complete");

            for (int i = 0; i < provIDList.Length; i++)
            {
                prov[i].id = provIDList[i];
                for (int j = 0; j < tSel.provQual.Length; j++)
                {
                    if (prov[i].id == tSel.provQual[j].id)
                        prov[i] = tSel.provQual[j];
                }
                prov[i].complete = provCompleteList[i];
            }
            for (int i = 0; i < tourIDList.Length; i++)
            {
                tour[i].id = tourIDList[i];
                for (int j = 0; j < tSel.tour.Length; j++)
                {
                    if (tour[i].id == tSel.tour[j].id)
                        tour[i] = tSel.tour[j];
                }
                tour[i].complete = tourCompleteList[i];
            }
            for (int i = 0; i < tourniesIDList.Length; i++)
            {
                tournies[i].id = tourniesIDList[i];
                for (int j = 0; j < tSel.tournies.Length; j++)
                {
                    if (tournies[i].id == tSel.tournies[j].id)
                        tournies[i] = tSel.tournies[j];
                }
                tournies[i].complete = tourniesCompleteList[i];
            }

            tSel.provQual = prov;
            tSel.tour = tour;
            tSel.tournies = tournies;
            tSel.tourChampionship = champ[0];
            tSel.provChampionship = champ[1];
        }
    }

    private void LoadSponsorManager(SponsorManager pUpM)
    {

    }

    private void LoadTeamDetails()
    {
        teams = new Team[totalTeams];

        int[] idList = myFile.GetArray<int>("Total ID List");
        int[] winsList = myFile.GetArray<int>("Total Wins List");
        int[] lossList = myFile.GetArray<int>("Total Loss List");
        float[] earningsList = myFile.GetArray<float>("Total Earnings List");

        //Debug.Log("Total ID List Length is " + idList.Length);
        //Debug.Log("Total Teams List Length is " + teams.Length);

        for (int i = 0; i < idList.Length; i++)
        {
            for (int j = 0; j < teamPool.Length; j++)
            {
                if (idList[i] == teamPool[j].id)
                    teams[i] = teamPool[j];
            }

            teams[i].wins = winsList[i];
            teams[i].loss = lossList[i];
            teams[i].earnings = earningsList[i];

            if (teams[i].id == playerTeamIndex)
            {
                teams[i].name = teamName;
                earnings = earningsList[i];
                Debug.Log("Earnings - CM from EarningsList - " + earnings);
                teams[i].player = true;
            }
        }

        tourTeams = new Team[totalTourTeams];

        int[] tourTeamsIDList = myFile.GetArray<int>("Tour Team ID List");
        int[] tourWinsList = myFile.GetArray<int>("Tour Wins List");
        int[] tourLossList = myFile.GetArray<int>("Tour Loss List");
        float[] tourPointsList = myFile.GetArray<float>("Tour Points List");

        //Debug.Log("Tour Record List Length is " + tourWinsList.Length + " " + tourLossList.Length);
        //Debug.Log("Tour Teams List Length is " + tourTeamsIDList.Length);

        if (tourTeams.Length > 0)
        {
            for (int i = 0; i < teams.Length; i++)
            {
                for (int j = 0; j < tourTeamsIDList.Length; j++)
                {
                    if (teams[i].id == tourTeamsIDList[j])
                    {
                        teams[i].tourRecord.x = tourWinsList[j];
                        teams[i].tourRecord.y = tourLossList[j];
                        teams[i].tourPoints = tourPointsList[j];
                    }
                }
            }

            for (int i = 0; i < tourTeamsIDList.Length; i++)
            {
                for (int j = 0; j < teams.Length; j++)
                {
                    //Debug.Log("Load Career " + i);
                    if (tourTeamsIDList[i] == teams[j].id)
                        tourTeams[i] = teams[j];
                }
            }
        }
    }

    public void LoadTeamsFromSave()
    {
        myFile = new EasyFileSave("my_player_data");
        if (!myFile.Load())
        {
            Debug.LogWarning("No save file found for teams.");
            return;
        }

        int teamCount = myFile.GetInt("Team Count", 0);
        if (teamCount == 0)
        {
            Debug.LogWarning("No teams found in save file.");
            return;
        }

        teams = new Team[teamCount];

        for (int i = 0; i < teamCount; i++)
        {
            Team team = new Team();
            team.id = myFile.GetInt($"Team_{i}_Id");
            team.name = myFile.GetString($"Team_{i}_Name");
            team.wins = myFile.GetInt($"Team_{i}_Wins");
            team.loss = myFile.GetInt($"Team_{i}_Loss");
            team.earnings = myFile.GetFloat($"Team_{i}_Earnings");
            team.player = myFile.GetBool($"Team_{i}_Player");
            team.rank = myFile.GetInt($"Team_{i}_Rank");
            team.nextOpp = myFile.GetString($"Team_{i}_NextOpp");
            team.draw = myFile.GetInt($"Team_{i}_Draw");
            team.takeOut = myFile.GetInt($"Team_{i}_TakeOut");
            team.guard = myFile.GetInt($"Team_{i}_Guard");
            team.sweepStrength = myFile.GetInt($"Team_{i}_SweepStrength");
            team.sweepEnduro = myFile.GetInt($"Team_{i}_SweepEnduro");
            team.sweepCohesion = myFile.GetInt($"Team_{i}_SweepCohesion");

            int playerCount = myFile.GetInt($"Team_{i}_PlayerCount", 0);
            team.players = new List<Player>();
            for (int j = 0; j < playerCount; j++)
            {
                Player p = new Player();
                p.id = myFile.GetInt($"Team_{i}_Player_{j}_Id");
                p.name = myFile.GetString($"Team_{i}_Player_{j}_Name");
                p.draw = myFile.GetInt($"Team_{i}_Player_{j}_Draw");
                p.takeOut = myFile.GetInt($"Team_{i}_Player_{j}_TakeOut");
                p.guard = myFile.GetInt($"Team_{i}_Player_{j}_Guard");
                p.sweepStrength = myFile.GetInt($"Team_{i}_Player_{j}_SweepStrength");
                p.sweepEnduro = myFile.GetInt($"Team_{i}_Player_{j}_SweepEnduro");
                p.sweepCohesion = myFile.GetInt($"Team_{i}_Player_{j}_SweepCohesion");
                // Add other player fields as needed
                team.players.Add(p);
            }

            teams[i] = team;
        }

        Debug.Log("Teams and players loaded from save.");
    }

    private void LoadEquipment(EquipmentManager em)
    {
        if (em != null)
        {
            inventoryID = myFile.GetArray<int>("Inventory ID List");
            activeEquipID = myFile.GetArray<int>("Active Equip ID List");

            Debug.Log("Loading Equipment - activeId Length is " + activeEquipID.Length);

            int[] tempID = myFile.GetArray<int>("Total Item ID List");
            float[] tempCost = myFile.GetArray<float>("Total Item Cost List");
            float[] tempColorX = myFile.GetArray<float>("Total Item Color X List");
            float[] tempColorY = myFile.GetArray<float>("Total Item Color Y List");
            float[] tempColorZ = myFile.GetArray<float>("Total Item Color Z List");
            float[] tempColorA = myFile.GetArray<float>("Total Item Color A List");
            int[] tempDuration = myFile.GetArray<int>("Total Item Duration List");
            int[] tempStats0 = myFile.GetArray<int>("Total Item Draw List");
            int[] tempStats1 = myFile.GetArray<int>("Total Item Guard List");
            int[] tempStats2 = myFile.GetArray<int>("Total Item Takeout List");
            int[] tempStats3 = myFile.GetArray<int>("Total Item Strength List");
            int[] tempStats4 = myFile.GetArray<int>("Total Item Endurance List");
            int[] tempStats5 = myFile.GetArray<int>("Total Item Cohesion List");
            int[] tempOppStats0 = myFile.GetArray<int>("Total Item Opp Draw List");
            int[] tempOppStats1 = myFile.GetArray<int>("Total Item Opp Guard List");
            int[] tempOppStats2 = myFile.GetArray<int>("Total Item Opp Takeout List");
            int[] tempOppStats3 = myFile.GetArray<int>("Total Item Opp Strength List");
            int[] tempOppStats4 = myFile.GetArray<int>("Total Item Opp Endurance List");
            int[] tempOppStats5 = myFile.GetArray<int>("Total Item Opp Cohesion List");

            em.handles = new Equipment[30];
            em.heads = new Equipment[30];
            em.footwear = new Equipment[20];
            em.apparel = new Equipment[20];

            int handleIdx = 0, headIdx = 0, footIdx = 0, apparelIdx = 0;

            // Build all equipment and a lookup dictionary
            var equipDict = new Dictionary<int, Equipment>();

            for (int i = 0; i < tempID.Length; i++)
            {
                if (tempID[i] == -1)
                {
                    if (i < 30) handleIdx++;
                    else if (i < 60) headIdx++;
                    else if (i < 80) footIdx++;
                    else apparelIdx++;
                    continue;
                }

                Debug.Log("Loading Equipment - tempId Length is " + tempID.Length);

                Equipment eq = new Equipment();
                eq.id = tempID[i];
                eq.cost = tempCost[i];
                eq.color = new Color(tempColorX[i], tempColorY[i], tempColorZ[i], tempColorA[i]);
                eq.duration = tempDuration[i];
                eq.stats = new int[6];
                eq.oppStats = new int[6];
                eq.stats[0] = tempStats0[i];
                eq.stats[1] = tempStats1[i];
                eq.stats[2] = tempStats2[i];
                eq.stats[3] = tempStats3[i];
                eq.stats[4] = tempStats4[i];
                eq.stats[5] = tempStats5[i];
                eq.oppStats[0] = tempOppStats0[i];
                eq.oppStats[1] = tempOppStats1[i];
                eq.oppStats[2] = tempOppStats2[i];
                eq.oppStats[3] = tempOppStats3[i];
                eq.oppStats[4] = tempOppStats4[i];
                eq.oppStats[5] = tempOppStats5[i];

                equipDict[eq.id] = eq;

                if (i < 30)
                {
                    if (handleIdx < em.handles.Length)
                        em.handles[handleIdx++] = eq;
                    else
                        Debug.LogWarning("Handle index out of range while loading equipment.");
                }
                else if (i < 60)
                {
                    if (headIdx < em.heads.Length)
                        em.heads[headIdx++] = eq;
                    else
                        Debug.LogWarning("Head index out of range while loading equipment.");
                }
                else if (i < 80)
                {
                    if (footIdx < em.footwear.Length)
                        em.footwear[footIdx++] = eq;
                    else
                        Debug.LogWarning("Footwear index out of range while loading equipment.");
                }
                else
                {
                    if (apparelIdx < em.apparel.Length)
                        em.apparel[apparelIdx++] = eq;
                    else
                        Debug.LogWarning("Apparel index out of range while loading equipment.");
                }
            }

            // Set activeEquip from saved IDs
            //em.LoadActiveEquipFromCareerManager(this);
        }
    }

    private void LoadTourTeamData()
    {
        int[] tempTRX = myFile.GetArray<int>("Team Records X");
        int[] tempTRY = myFile.GetArray<int>("Team Records Y");
        float[] tempTRZ = myFile.GetArray<float>("Team Records Z");
        int[] tempTRW = myFile.GetArray<int>("Team Records W");

        int[] tempTourTRX = myFile.GetArray<int>("Tour Records X");
        int[] tempTourTRY = myFile.GetArray<int>("Tour Records Y");
        float[] tempTourTRZ = myFile.GetArray<float>("Tour Records Z");
        int[] tempTourTRW = myFile.GetArray<int>("Tour Records W");

        teamRecords = new Vector4[tempTRX.Length];
        tourRecords = new Vector4[tempTourTRX.Length];

        for (int i = 0; i < teamRecords.Length; i++)
        {
            teamRecords[i].x = tempTRX[i];
            teamRecords[i].y = tempTRY[i];
            teamRecords[i].z = tempTRZ[i];
            teamRecords[i].w = tempTRW[i];
        }

        for (int i = 0; i < tourRecords.Length; i++)
        {
            tourRecords[i].x = tempTourTRX[i];
            tourRecords[i].y = tempTourTRY[i];
            tourRecords[i].z = tempTourTRZ[i];
            tourRecords[i].w = tempTourTRW[i];
        }

        Debug.Log("Team Records Length is " + teamRecords.Length);

        if (provRankList == null)
        {
            provRankList = new List<Standings_List>();
        }

        if (tourRankList == null)
        {
            tourRankList = new List<TourStandings_List>();
        }
    }

    private void UpdateCurrentTourny(GameSettingsPersist gsp)
    {

        if (gsp.tournyInProgress)
        {
            currentTourny.name = myFile.GetString("Current Tourny Name");
            currentTourny.id = myFile.GetInt("Current Tourny ID");
            currentTourny.tour = myFile.GetBool("Current Tourny Tour");
            currentTourny.qualifier = myFile.GetBool("Current Tourny Qualifier");
            currentTourny.championship = myFile.GetBool("Current Tourny Championship");
            currentTourny.prizeMoney = myFile.GetInt("Prize Money");
            currentTourny.BG = myFile.GetInt("Current Tourny BG");
            currentTourny.crowdDensity = myFile.GetInt("Current Tourny Crowd Density");

            gsp.KO3 = currentTourny.tour;
            gsp.draw = myFile.GetInt("Current Tourny Draw");
            gsp.playoffRound = myFile.GetInt("Current Tourny Playoff Round");

            string[] tournyNameList = myFile.GetArray<string>("Tourny Name List");
            int[] tournyIDList = myFile.GetArray<int>("Tourny Team ID List");
            int[] tournyWinsList = myFile.GetArray<int>("Tourny Wins List");
            int[] tournyLossList = myFile.GetArray<int>("Tourny Loss List");
            string[] tournyNextOppList = myFile.GetArray<string>("Tourny NextOpp List");
            float[] tournyEarningsList = myFile.GetArray<float>("Tourny Earnings List");
            bool[] tournyPlayerList = myFile.GetArray<bool>("Tourny Player List");
            //Debug.Log("Tourny Earnings List length is " + tournyEarningsList.Length);


            if (currentTournyTeams.Length <= 0)
                currentTournyTeams = new Team[tournyIDList.Length];

            for (int i = 0; i < currentTournyTeams.Length; i++)
            {
                currentTournyTeams[i] = new Team();
                currentTournyTeams[i].id = tournyIDList[i];
                currentTournyTeams[i].player = tournyPlayerList[i];
                currentTournyTeams[i].nextOpp = tournyNextOppList[i];

                for (int j = 0; j < teams.Length; j++)
                {
                    if (currentTournyTeams[i].id == teams[j].id)
                        currentTournyTeams[i] = teams[j];

                }

                if (currentTournyTeams[i].player)
                {
                    Debug.Log("Player Team - " + playerTeamIndex);
                    currentTournyTeams[i].name = teamName;
                    //currentTournyTeams[i].earnings = earnings;
                }

                currentTournyTeams[i].name = tournyNameList[i];
                currentTournyTeams[i].wins = tournyWinsList[i];
                currentTournyTeams[i].loss = tournyLossList[i];
                currentTournyTeams[i].earnings = tournyEarningsList[i];

            }

            //teamRecords = new Vector4[currentTourny.teams];

            //int[] tempTRX = myFile.GetArray<int>("Team Records X");
            //int[] tempTRY = myFile.GetArray<int>("Team Records Y");
            //float[] tempTRZ = myFile.GetArray<float>("Team Records Z");
            //int[] tempTRW = myFile.GetArray<int>("Team Records W");

            //for (int i = 0; i < teamRecords.Length; i++)
            //{
            //    teamRecords[i].x = tempTRX[i];
            //    teamRecords[i].y = tempTRY[i];
            //    teamRecords[i].z = tempTRZ[i];
            //    teamRecords[i].w = tempTRW[i];
            //}
        }
    }

    private void LoadGameSettings(GameSettingsPersist gsp)
    {
        if (gsp)
        {
            gsp.tourny = myFile.GetBool("Tourny Game");
            gsp.ends = myFile.GetInt("Game Ends");
            gsp.endCurrent = myFile.GetInt("Game Active End");
            gsp.rocks = myFile.GetInt("Game Rocks");
            gsp.rockCurrent = myFile.GetInt("Game Rock Current");
            gsp.redHammer = myFile.GetBool("Game Red Hammer");
            gsp.aiYellow = myFile.GetBool("Game AI Yellow");
            gsp.aiRed = myFile.GetBool("Game AI Red");
            gsp.yellowScore = myFile.GetInt("Game Yellow Score");
            gsp.redScore = myFile.GetInt("Game Red Score");
            gsp.yellowTeamName = myFile.GetString("Game Yellow Team Name");
            gsp.redTeamName = myFile.GetString("Game Red Team Name");

            float[] rockPosX = myFile.GetArray<float>("Game Rock Position X List");
            float[] rockPosY = myFile.GetArray<float>("Game Rock Position Y List");
            bool[] rockInPlay = myFile.GetArray<bool>("Game Rock In Play List");
            gsp.rockPos = new Vector2[rockPosX.Length];
            gsp.rockInPlay = new bool[rockInPlay.Length];
            for (int i = 0; i < rockPosX.Length; i++)
            {
                gsp.rockPos[i] = new Vector2(rockPosX[i], rockPosY[i]);
                gsp.rockInPlay[i] = rockInPlay[i];
            }

            int[] redScoreList = myFile.GetArray<int>("Game Red Score List");
            int[] yellowScoreList = myFile.GetArray<int>("Game Yellow Score List");
            Debug.Log("gsp.ends is " + gsp.ends);
            gsp.score = new Vector2Int[gsp.ends];
            for (int i = 0; i < gsp.score.Length; i++)
            {
                gsp.score[i].x = redScoreList[i];
                gsp.score[i].y = yellowScoreList[i];
            }

        }
    }

    private void LoadCurrentGameState(GameSettingsPersist gsp)
    {
        if (gsp == null) gsp = FindObjectOfType<GameSettingsPersist>();
        if (gsp == null) return;

        myFile = new EasyFileSave("my_player_data");
        if (!myFile.Load()) return;

        gsp.tourny = myFile.GetBool("Tourny Game");
        gsp.ends = myFile.GetInt("Game Ends");
        gsp.endCurrent = myFile.GetInt("Game Active End");
        gsp.rocks = myFile.GetInt("Game Rocks");
        gsp.rockCurrent = myFile.GetInt("Game Rock Current");
        gsp.redHammer = myFile.GetBool("Game Red Hammer");
        gsp.aiYellow = myFile.GetBool("Game AI Yellow");
        gsp.aiRed = myFile.GetBool("Game AI Red");
        gsp.yellowScore = myFile.GetInt("Game Yellow Score");
        gsp.redScore = myFile.GetInt("Game Red Score");
        gsp.yellowTeamName = myFile.GetString("Game Yellow Team Name");
        gsp.redTeamName = myFile.GetString("Game Red Team Name");

        gsp.gameInProgress = myFile.GetBool("Game In Progress");
        gsp.tournyInProgress = myFile.GetBool("Tourny In Progress");
        gsp.loadGame = myFile.GetBool("Game Load");

        float[] rockPosX = myFile.GetArray<float>("Game Rock Position X List");
        float[] rockPosY = myFile.GetArray<float>("Game Rock Position Y List");
        bool[] rockInPlay = myFile.GetArray<bool>("Game Rock In Play List");
        gsp.rockPos = new Vector2[rockPosX.Length];
        gsp.rockInPlay = new bool[rockInPlay.Length];
        for (int i = 0; i < rockPosX.Length; i++)
        {
            gsp.rockPos[i] = new Vector2(rockPosX[i], rockPosY[i]);
            gsp.rockInPlay[i] = rockInPlay[i];
        }

        int[] redScoreList = myFile.GetArray<int>("Game Red Score List");
        int[] yellowScoreList = myFile.GetArray<int>("Game Yellow Score List");
        gsp.score = new Vector2Int[redScoreList.Length];
        for (int i = 0; i < gsp.score.Length; i++)
        {
            gsp.score[i].x = redScoreList[i];
            gsp.score[i].y = yellowScoreList[i];
        }
    }

    public void LoadTournyState(object tournyStateManager = null)
    {
        myFile = new EasyFileSave("my_player_data");
        if (!myFile.Load())
        {
            Debug.LogWarning("No save file found for tournament state.");
            return;
        }

        // Auto-find if not provided
        if (tournyStateManager == null)
        {
            tournyStateManager =
                (object)FindObjectOfType<TournyManager>() ??
                (object)FindObjectOfType<PlayoffManager>() ??
                (object)FindObjectOfType<PlayoffManager_SingleK>() ??
                (object)FindObjectOfType<PlayoffManager_TripleK>();
        }

        if (tournyStateManager is TournyManager tm)
        {
            string tag = "TournyManager_";
            tm.draw = myFile.GetInt($"{tag}Draw");
            tm.numberOfTeams = myFile.GetInt($"{tag}NumberOfTeams");
            tm.prize = myFile.GetInt($"{tag}Prize");
            tm.oppTeam = myFile.GetInt($"{tag}OppTeam");
            tm.playoffRound = myFile.GetInt($"{tag}PlayoffRound");

            int teamCount = myFile.GetInt($"{tag}TeamCount");
            tm.teams = new Team[teamCount];
            for (int i = 0; i < teamCount; i++)
            {
                Team t = new Team();
                t.id = myFile.GetInt($"{tag}Team_{i}_Id");
                t.name = myFile.GetString($"{tag}Team_{i}_Name");
                t.wins = myFile.GetInt($"{tag}Team_{i}_Wins");
                t.loss = myFile.GetInt($"{tag}Team_{i}_Loss");
                t.earnings = myFile.GetFloat($"{tag}Team_{i}_Earnings");
                t.player = myFile.GetBool($"{tag}Team_{i}_Player");
                t.rank = myFile.GetInt($"{tag}Team_{i}_Rank");
                t.strength = myFile.GetInt($"{tag}Team_{i}_Strength");
                t.nextOpp = myFile.GetString($"{tag}Team_{i}_NextOpp");
                tm.teams[i] = t;
            }
        }
        else if (tournyStateManager is PlayoffManager pm)
        {
            string tag = "PlayoffManager_";
            pm.oppTeam = myFile.GetInt($"{tag}OppTeam");
            pm.playoffRound = myFile.GetInt($"{tag}PlayoffRound");

            int teamCount = myFile.GetInt($"{tag}TeamCount");
            pm.playoffTeams = new Team[teamCount];
            for (int i = 0; i < teamCount; i++)
            {
                Team t = new Team();
                t.id = myFile.GetInt($"{tag}Team_{i}_Id");
                t.name = myFile.GetString($"{tag}Team_{i}_Name");
                t.wins = myFile.GetInt($"{tag}Team_{i}_Wins");
                t.loss = myFile.GetInt($"{tag}Team_{i}_Loss");
                t.earnings = myFile.GetFloat($"{tag}Team_{i}_Earnings");
                t.player = myFile.GetBool($"{tag}Team_{i}_Player");
                t.rank = myFile.GetInt($"{tag}Team_{i}_Rank");
                t.strength = myFile.GetInt($"{tag}Team_{i}_Strength");
                t.nextOpp = myFile.GetString($"{tag}Team_{i}_NextOpp");
                pm.playoffTeams[i] = t;
            }
        }
        else if (tournyStateManager is PlayoffManager_SingleK pmSingle)
        {
            string tag = "PlayoffManager_SingleK_";
            pmSingle.oppTeam = myFile.GetInt($"{tag}OppTeam");
            pmSingle.playoffRound = myFile.GetInt($"{tag}PlayoffRound");

            int teamCount = myFile.GetInt($"{tag}TeamCount");
            pmSingle.playoffTeams = new Team[teamCount];
            for (int i = 0; i < teamCount; i++)
            {
                Team t = new Team();
                t.id = myFile.GetInt($"{tag}Team_{i}_Id");
                t.name = myFile.GetString($"{tag}Team_{i}_Name");
                t.wins = myFile.GetInt($"{tag}Team_{i}_Wins");
                t.loss = myFile.GetInt($"{tag}Team_{i}_Loss");
                t.earnings = myFile.GetFloat($"{tag}Team_{i}_Earnings");
                t.player = myFile.GetBool($"{tag}Team_{i}_Player");
                t.rank = myFile.GetInt($"{tag}Team_{i}_Rank");
                t.strength = myFile.GetInt($"{tag}Team_{i}_Strength");
                t.nextOpp = myFile.GetString($"{tag}Team_{i}_NextOpp");
                pmSingle.playoffTeams[i] = t;
            }
        }
        else if (tournyStateManager is PlayoffManager_TripleK pmTriple)
        {
            string tag = "PlayoffManager_TripleK_";
            pmTriple.playoffRound = myFile.GetInt($"{tag}PlayoffRound");

            int teamCount = myFile.GetInt($"{tag}TeamCount");
            pmTriple.teams = new Team[teamCount];
            for (int i = 0; i < teamCount; i++)
            {
                Team t = new Team();
                t.id = myFile.GetInt($"{tag}Team_{i}_Id");
                t.name = myFile.GetString($"{tag}Team_{i}_Name");
                t.wins = myFile.GetInt($"{tag}Team_{i}_Wins");
                t.loss = myFile.GetInt($"{tag}Team_{i}_Loss");
                t.earnings = myFile.GetFloat($"{tag}Team_{i}_Earnings");
                t.player = myFile.GetBool($"{tag}Team_{i}_Player");
                t.rank = myFile.GetInt($"{tag}Team_{i}_Rank");
                t.strength = myFile.GetInt($"{tag}Team_{i}_Strength");
                t.nextOpp = myFile.GetString($"{tag}Team_{i}_NextOpp");
                pmTriple.teams[i] = t;
            }

            int gameListCount = myFile.GetInt($"{tag}GameListCount");
            pmTriple.gameList = new Vector2[gameListCount];
            for (int i = 0; i < gameListCount; i++)
            {
                float x = myFile.GetFloat($"{tag}GameList_{i}_X");
                float y = myFile.GetFloat($"{tag}GameList_{i}_Y");
                pmTriple.gameList[i] = new Vector2(x, y);
            }
        }
        else
        {
            Debug.LogWarning("Unknown tournament/playoff manager type passed to LoadTournyState.");
        }
    }

    // Continue with other helper methods like LoadGameProgress, LoadDialogueStatus, etc.

    // Define all the helper methods with specific loading logic

    IEnumerator SaveHighScore()
    {
        myFile = new EasyFileSave("my_hiscore_data");
        allTimeList = new List<Standings_List>();

        if (myFile.Load())
        {
            float[] allTimeEarnings = myFile.GetArray<float>("All Time Earnings");
            string[] allTimeName = myFile.GetArray<string>("All Time Names");
            allTimeTrophyList = myFile.GetList<bool>("All Time Trophies Won");

            if (allTimeTrophyList.Count <= 0)
            {
                allTimeTrophyList.AddRange(currentTrophyList);
            }
            //Debug.Log("All Time Earnings length is " + allTimeEarnings.Length);


            for (int j = 0; j < allTimeEarnings.Length; j++)
            {
                Team tempTeam = new Team();
                //tempTeam.wins = 0;
                //tempTeam.loss = 0;
                //tempTeam.earnings = 0;
                //tempTeam.name = "";

                tempTeam.name = allTimeName[j];
                //tempTeam.wins = Mathf.RoundToInt(allTimeEarnings[i]);
                tempTeam.earnings = allTimeEarnings[j];
                allTimeList.Add(new Standings_List(tempTeam));
            }

            myFile.Dispose();
        }

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < teams.Length; i++)
        {
            if (teams[i].id == playerTeamIndex)
            {
                string fullTemp = playerName + " " + teamName;
                string halfTemp = teams[i].name;
                teams[i].name = fullTemp;
                allTimeList.Add(new Standings_List(teams[i]));
                teams[i].name = halfTemp;
            }
        }

        //Debug.Log("All Time List - " + allTimeList.Count);

        //Debug.Log("Top of the List is " + allTimeList[0].team.name);
        allTimeList.Sort();

        float[] allTimeEarningsTemp = new float[allTimeList.Count];
        if (allTimeList.Count > 100)
            allTimeEarningsTemp = new float[99];

        string[] allTimeNameTemp = new string[allTimeList.Count];
        if (allTimeList.Count > 100)
            allTimeNameTemp = new string[99];

        for (int i = 0; i < allTimeEarningsTemp.Length; i++)
        {
            allTimeEarningsTemp[i] = allTimeList[i].team.earnings;
            allTimeNameTemp[i] = allTimeList[i].team.name;
        }

        //Debug.Log("All Time Trophy List Count - " + allTimeTrophyList.Count);

        if (currentTrophyList.Count > 0)
        {
            for (int i = 0; i < allTimeTrophyList.Count; i++)
            {
                if (allTimeTrophyList[i] == false && currentTrophyList[i] == true)
                    allTimeTrophyList[i] = true;
            }
        }
        

        //Debug.Log("All Time List Length - " + allTimeList.Count);
        myFile.Dispose();

        myFile = new EasyFileSave("my_hiscore_data");

        myFile.Add("All Time Earnings", allTimeEarningsTemp);
        myFile.Add("All Time Names", allTimeNameTemp);
        myFile.Add("All Time Trophies Won", allTimeTrophyList);

        myFile.Append();
    }

    private void SaveGameProgress(GameSettingsPersist gsp)
    {
        if (gsp == null)
            gsp = FindObjectOfType<GameSettingsPersist>();

        myFile = new EasyFileSave("my_player_data");

        myFile.Add("Tourny In Progress", gsp.tournyInProgress);
        myFile.Add("Game In Progress", gsp.gameInProgress);
        myFile.Add("Mode", debug);
        myFile.Add("Week", week);
        myFile.Add("Season", season);
        myFile.Add("Player Name", playerName);
        myFile.Add("Team Name", teamName);
        myFile.Add("Team Colour", teamColour);
        myFile.Add("Player Team Index", playerTeamIndex);
        myFile.Add("Career Record", record);
        myFile.Add("Career Cash", cash);
        myFile.Add("Career Earnings", earnings);
        myFile.Add("Prov Qual", provQual);
        myFile.Add("Tour Qual", tourQual);
        myFile.Add("XP", xp);
        myFile.Add("Skillpoints", skillPoints);
        myFile.Add("Level", level);

        myFile.Add("Draw Accuracy", cStats.drawAccuracy);
        myFile.Add("Take Out Accuracy", cStats.takeOutAccuracy);
        myFile.Add("Guard Accuracy", cStats.guardAccuracy);
        myFile.Add("Sweep Strength", cStats.sweepStrength);
        myFile.Add("Sweep Endurance", cStats.sweepEndurance);
        myFile.Add("Sweep Cohesion", cStats.sweepCohesion);

        myFile.Add("Game Load", gsp.loadGame);

        myFile.Append();
    }

    private void SaveActivePlayers(TeamMenu teamSel)
    {
        if (teamSel && teamSel.activePlayers != null)
        {
            int len = teamSel.activePlayers.Length;
            int[] idList = new int[len];
            string[] nameList = new string[len];
            int[] drawList = new int[len];
            int[] guardList = new int[len];
            int[] takeOutList = new int[len];
            int[] strengthList = new int[len];
            int[] enduroList = new int[len];
            int[] cohesionList = new int[len];
            int[] oppDrawList = new int[len];
            int[] oppGuardList = new int[len];
            int[] oppTakeOutList = new int[len];
            int[] oppStrengthList = new int[len];
            int[] oppEnduroList = new int[len];
            int[] oppCohesionList = new int[len];

            for (int i = 0; i < len; i++)
            {
                var p = teamSel.activePlayers[i];
                idList[i] = p.id;
                nameList[i] = p.name;
                drawList[i] = p.draw;
                guardList[i] = p.guard;
                takeOutList[i] = p.takeOut;
                strengthList[i] = p.sweepStrength;
                enduroList[i] = p.sweepEnduro;
                cohesionList[i] = p.sweepCohesion;
                oppDrawList[i] = p.oppDraw;
                oppGuardList[i] = p.oppGuard;
                oppTakeOutList[i] = p.oppTakeOut;
                oppStrengthList[i] = p.oppStrength;
                oppEnduroList[i] = p.oppEnduro;
                oppCohesionList[i] = p.oppCohesion;
            }

            myFile.Add("Active Players ID List", idList);
            myFile.Add("Active Players Name List", nameList);
            myFile.Add("Active Players Draw List", drawList);
            myFile.Add("Active Players Guard List", guardList);
            myFile.Add("Active Players Takeout List", takeOutList);
            myFile.Add("Active Players Strength List", strengthList);
            myFile.Add("Active Players Endurance List", enduroList);
            myFile.Add("Active Players Cohesion List", cohesionList);
            myFile.Add("Active Players Opp Draw List", oppDrawList);
            myFile.Add("Active Players Opp Guard List", oppGuardList);
            myFile.Add("Active Players Opp Takeout List", oppTakeOutList);
            myFile.Add("Active Players Opp Strength List", oppStrengthList);
            myFile.Add("Active Players Opp Endurance List", oppEnduroList);
            myFile.Add("Active Players Opp Cohesion List", oppCohesionList);
        }
    }

    public void SaveTeamsToSave()
    {
        myFile = new EasyFileSave("my_player_data");
        if (teams == null || teams.Length == 0)
        {
            Debug.LogWarning("No teams to save.");
            return;
        }

        int teamCount = teams.Length;
        myFile.Add("Team Count", teamCount);

        for (int i = 0; i < teamCount; i++)
        {
            Team team = teams[i];
            myFile.Add($"Team_{i}_Id", team.id);
            myFile.Add($"Team_{i}_Name", team.name);
            myFile.Add($"Team_{i}_Wins", team.wins);
            myFile.Add($"Team_{i}_Loss", team.loss);
            myFile.Add($"Team_{i}_Earnings", team.earnings);
            myFile.Add($"Team_{i}_Player", team.player);
            myFile.Add($"Team_{i}_Rank", team.rank);
            myFile.Add($"Team_{i}_NextOpp", team.nextOpp);
            myFile.Add($"Team_{i}_Draw", team.draw);
            myFile.Add($"Team_{i}_TakeOut", team.takeOut);
            myFile.Add($"Team_{i}_Guard", team.guard);
            myFile.Add($"Team_{i}_SweepStrength", team.sweepStrength);
            myFile.Add($"Team_{i}_SweepEnduro", team.sweepEnduro);
            myFile.Add($"Team_{i}_SweepCohesion", team.sweepCohesion);

            int playerCount = team.players != null ? team.players.Count : 0;
            myFile.Add($"Team_{i}_PlayerCount", playerCount);
            for (int j = 0; j < playerCount; j++)
            {
                Player p = team.players[j];
                myFile.Add($"Team_{i}_Player_{j}_Id", p.id);
                myFile.Add($"Team_{i}_Player_{j}_Name", p.name);
                myFile.Add($"Team_{i}_Player_{j}_Draw", p.draw);
                myFile.Add($"Team_{i}_Player_{j}_TakeOut", p.takeOut);
                myFile.Add($"Team_{i}_Player_{j}_Guard", p.guard);
                myFile.Add($"Team_{i}_Player_{j}_SweepStrength", p.sweepStrength);
                myFile.Add($"Team_{i}_Player_{j}_SweepEnduro", p.sweepEnduro);
                myFile.Add($"Team_{i}_Player_{j}_SweepCohesion", p.sweepCohesion);
            }
        }

        myFile.Append();
        Debug.Log("Teams and players saved to file.");
    }

    private void SaveDialogueStatus(StorylineManager slm)
    {
        myFile.Add("Coach Dialogue Played List", coachDialogue);
        myFile.Add("Qualifying Dialogue Played List", qualDialogue);
        myFile.Add("Review Dialogue Played List", reviewDialogue);
        myFile.Add("Intro Dialogue Played List", introDialogue);
        myFile.Add("Help Dialogue Played List", helpDialogue);
        myFile.Add("Strategy Dialogue Played List", strategyDialogue);
        myFile.Add("Story Dialogue Played List", storyDialogue);

        if (slm != null)
            myFile.Add("Story Block", slm.blockIndex);
    }

    private void SaveCardData(SponsorManager sm)
    {
        if (cardPUIDList != null && cardPUIDList.Length > 0)
            myFile.Add("Card PowerUp ID List", cardPUIDList);
        else
            myFile.Add("Card PowerUp ID List", new int[0]);

        if (cardSponsorIDList != null && cardSponsorIDList.Length > 0)
            myFile.Add("Card Sponsor ID List", cardSponsorIDList);
        else
            myFile.Add("Card Sponsor ID List", new int[0]);

        if (activeCardIDList != null && activeCardIDList.Length > 0)
            myFile.Add("Active Card ID List", activeCardIDList);
        else
            myFile.Add("Active Card ID List", new int[0]);

        if (playedCardIDList != null && playedCardIDList.Length > 0)
            myFile.Add("Played Card ID List", playedCardIDList);
        else
            myFile.Add("Played Card ID List", new int[0]);

        if (activeCardLengthList != null && activeCardLengthList.Length > 0)
            myFile.Add("Active Card Length List", activeCardLengthList);
        else
            myFile.Add("Active Card Length List", new int[0]);
    }

    private void SaveTournamentData(TournySelector tSel)
    {
        if (tSel != null)
        {
            int[] provIDList = new int[prov.Length];
            bool[] provCompleteList = new bool[prov.Length];
            int[] tourIDList = new int[tour.Length];
            bool[] tourCompleteList = new bool[tour.Length];
            int[] tourniesIDList = new int[tournies.Length];
            bool[] tourniesCompleteList = new bool[tournies.Length];

            for (int i = 0; i < prov.Length; i++)
            {
                provIDList[i] = prov[i].id;
                provCompleteList[i] = prov[i].complete;
            }
            for (int i = 0; i < tour.Length; i++)
            {
                tourIDList[i] = tour[i].id;
                tourCompleteList[i] = tour[i].complete;
            }
            for (int i = 0; i < tournies.Length; i++)
            {
                tourniesIDList[i] = tournies[i].id;
                tourniesCompleteList[i] = tournies[i].complete;
            }

            myFile.Add("Tour Championship Complete", tSel.tourChampionship.complete);
            myFile.Add("Prov Championship Complete", tSel.provChampionship.complete);
            myFile.Add("Prov ID List", provIDList);
            myFile.Add("Prov Complete List", provCompleteList);
            myFile.Add("Tour ID List", tourIDList);
            myFile.Add("Tour Complete List", tourCompleteList);
            myFile.Add("Tournies ID List", tourniesIDList);
            myFile.Add("Tournies Complete List", tourniesCompleteList);
        }
    }

    private void SaveTourTeamData()
    {
        if (teamRecords != null)
        {
            int[] tempTRX = new int[teamRecords.Length];
            int[] tempTRY = new int[teamRecords.Length];
            float[] tempTRZ = new float[teamRecords.Length];
            int[] tempTRW = new int[teamRecords.Length];
            for (int i = 0; i < teamRecords.Length; i++)
            {
                tempTRX[i] = (int)teamRecords[i].x;
                tempTRY[i] = (int)teamRecords[i].y;
                tempTRZ[i] = teamRecords[i].z;
                tempTRW[i] = (int)teamRecords[i].w;
            }
            myFile.Add("Team Records X", tempTRX);
            myFile.Add("Team Records Y", tempTRY);
            myFile.Add("Team Records Z", tempTRZ);
            myFile.Add("Team Records W", tempTRW);
        }
        if (tourRecords != null)
        {
            int[] tempTourTRX = new int[tourRecords.Length];
            int[] tempTourTRY = new int[tourRecords.Length];
            float[] tempTourTRZ = new float[tourRecords.Length];
            int[] tempTourTRW = new int[tourRecords.Length];
            for (int i = 0; i < tourRecords.Length; i++)
            {
                tempTourTRX[i] = (int)tourRecords[i].x;
                tempTourTRY[i] = (int)tourRecords[i].y;
                tempTourTRZ[i] = tourRecords[i].z;
                tempTourTRW[i] = (int)tourRecords[i].w;
            }
            myFile.Add("Tour Records X", tempTourTRX);
            myFile.Add("Tour Records Y", tempTourTRY);
            myFile.Add("Tour Records Z", tempTourTRZ);
            myFile.Add("Tour Records W", tempTourTRW);
        }
    }

    private void SaveTeamDetails()
    {
        int[] idList = new int[teams.Length];
        int[] winsList = new int[teams.Length];
        int[] lossList = new int[teams.Length];
        float[] earningsList = new float[teams.Length];

        for (int i = 0; i < teams.Length; i++)
        {
            idList[i] = teams[i].id;
            winsList[i] = teams[i].wins;
            lossList[i] = teams[i].loss;
            earningsList[i] = teams[i].earnings;
        }

        myFile.Add("Total ID List", idList);
        myFile.Add("Total Wins List", winsList);
        myFile.Add("Total Loss List", lossList);
        myFile.Add("Total Earnings List", earningsList);


        // Tour teams
        int[] tourTeamsIDList = new int[tourTeams.Length];
        int[] tourWinsList = new int[tourTeams.Length];
        int[] tourLossList = new int[tourTeams.Length];
        float[] tourPointsList = new float[tourTeams.Length];

        for (int i = 0; i < tourTeams.Length; i++)
        {
            if (tourTeams[i] != null)
            {
                tourTeamsIDList[i] = tourTeams[i].id;
                tourWinsList[i] = (int)tourTeams[i].tourRecord.x;
                tourLossList[i] = (int)tourTeams[i].tourRecord.y;
                tourPointsList[i] = tourTeams[i].tourPoints;
            }
            else
            {
                tourTeamsIDList[i] = -1;
                tourWinsList[i] = 0;
                tourLossList[i] = 0;
                tourPointsList[i] = 0f;
                Debug.LogWarning($"tourTeams[{i}] is null in SaveTeamDetails.");
            }
        }

        myFile.Add("Tour Team ID List", tourTeamsIDList);
        myFile.Add("Tour Wins List", tourWinsList);
        myFile.Add("Tour Loss List", tourLossList);
        myFile.Add("Tour Points List", tourPointsList);
    }

    private void SaveEquipment(EquipmentManager em)
    {
        if (em == null)
            return;

        // Ensure arrays are not null
        Equipment[] handles = em.handles ?? new Equipment[0];
        Equipment[] heads = em.heads ?? new Equipment[0];
        Equipment[] footwear = em.footwear ?? new Equipment[0];
        Equipment[] apparel = em.apparel ?? new Equipment[0];
        
        int handlesCount = handles.Length;
        int headsCount = heads.Length;
        int footwearCount = footwear.Length;
        int apparelCount = apparel.Length;
        int total = handlesCount + headsCount + footwearCount + apparelCount;

        // Save counts for robust loading
        myFile.Add("HandlesCount", handlesCount);
        myFile.Add("HeadsCount", headsCount);
        myFile.Add("FootwearCount", footwearCount);
        myFile.Add("ApparelCount", apparelCount);

        Debug.Log("Saving Equipment - Handle Count - " + handlesCount);

        // Prepare flat arrays for all equipment data
        int[] tempID = new int[total];
        float[] tempCost = new float[total];
        float[] tempColorX = new float[total];
        float[] tempColorY = new float[total];
        float[] tempColorZ = new float[total];
        float[] tempColorA = new float[total];
        int[] tempDuration = new int[total];
        int[] tempStats0 = new int[total];
        int[] tempStats1 = new int[total];
        int[] tempStats2 = new int[total];
        int[] tempStats3 = new int[total];
        int[] tempStats4 = new int[total];
        int[] tempStats5 = new int[total];
        int[] tempOppStats0 = new int[total];
        int[] tempOppStats1 = new int[total];
        int[] tempOppStats2 = new int[total];
        int[] tempOppStats3 = new int[total];
        int[] tempOppStats4 = new int[total];
        int[] tempOppStats5 = new int[total];

        int idx = 0;
        Equipment[][] all = { handles, heads, footwear, apparel };
        foreach (var arr in all)
        {
            foreach (var eq in arr)
            {
                if (eq != null)
                {
                    tempID[idx] = eq.id;
                    tempCost[idx] = eq.cost;
                    tempColorX[idx] = eq.color.r;
                    tempColorY[idx] = eq.color.g;
                    tempColorZ[idx] = eq.color.b;
                    tempColorA[idx] = eq.color.a;
                    tempDuration[idx] = eq.duration;
                    tempStats0[idx] = eq.stats != null && eq.stats.Length > 0 ? eq.stats[0] : 0;
                    tempStats1[idx] = eq.stats != null && eq.stats.Length > 1 ? eq.stats[1] : 0;
                    tempStats2[idx] = eq.stats != null && eq.stats.Length > 2 ? eq.stats[2] : 0;
                    tempStats3[idx] = eq.stats != null && eq.stats.Length > 3 ? eq.stats[3] : 0;
                    tempStats4[idx] = eq.stats != null && eq.stats.Length > 4 ? eq.stats[4] : 0;
                    tempStats5[idx] = eq.stats != null && eq.stats.Length > 5 ? eq.stats[5] : 0;
                    tempOppStats0[idx] = eq.oppStats != null && eq.oppStats.Length > 0 ? eq.oppStats[0] : 0;
                    tempOppStats1[idx] = eq.oppStats != null && eq.oppStats.Length > 1 ? eq.oppStats[1] : 0;
                    tempOppStats2[idx] = eq.oppStats != null && eq.oppStats.Length > 2 ? eq.oppStats[2] : 0;
                    tempOppStats3[idx] = eq.oppStats != null && eq.oppStats.Length > 3 ? eq.oppStats[3] : 0;
                    tempOppStats4[idx] = eq.oppStats != null && eq.oppStats.Length > 4 ? eq.oppStats[4] : 0;
                    tempOppStats5[idx] = eq.oppStats != null && eq.oppStats.Length > 5 ? eq.oppStats[5] : 0;
                }
                else
                {
                    tempID[idx] = -1;
                    tempCost[idx] = 0f;
                    tempColorX[idx] = 0f;
                    tempColorY[idx] = 0f;
                    tempColorZ[idx] = 0f;
                    tempColorA[idx] = 0f;
                    tempDuration[idx] = 0;
                    tempStats0[idx] = 0;
                    tempStats1[idx] = 0;
                    tempStats2[idx] = 0;
                    tempStats3[idx] = 0;
                    tempStats4[idx] = 0;
                    tempStats5[idx] = 0;
                    tempOppStats0[idx] = 0;
                    tempOppStats1[idx] = 0;
                    tempOppStats2[idx] = 0;
                    tempOppStats3[idx] = 0;
                    tempOppStats4[idx] = 0;
                    tempOppStats5[idx] = 0;
                }
                idx++;
            }
        }

        Debug.Log("Saving Equipment - Total Item ID Length - " + tempID.Length);
        myFile.Add("Total Item ID List", tempID);
        myFile.Add("Total Item Cost List", tempCost);
        myFile.Add("Total Item Color X List", tempColorX);
        myFile.Add("Total Item Color Y List", tempColorY);
        myFile.Add("Total Item Color Z List", tempColorZ);
        myFile.Add("Total Item Color A List", tempColorA);
        myFile.Add("Total Item Duration List", tempDuration);
        myFile.Add("Total Item Draw List", tempStats0);
        myFile.Add("Total Item Guard List", tempStats1);
        myFile.Add("Total Item Takeout List", tempStats2);
        myFile.Add("Total Item Strength List", tempStats3);
        myFile.Add("Total Item Endurance List", tempStats4);
        myFile.Add("Total Item Cohesion List", tempStats5);
        myFile.Add("Total Item Opp Draw List", tempOppStats0);
        myFile.Add("Total Item Opp Guard List", tempOppStats1);
        myFile.Add("Total Item Opp Takeout List", tempOppStats2);
        myFile.Add("Total Item Opp Strength List", tempOppStats3);
        myFile.Add("Total Item Opp Endurance List", tempOppStats4);
        myFile.Add("Total Item Opp Cohesion List", tempOppStats5);

        // Save active equipment IDs
        activeEquipID = new int[em.activeEquip.Length];
        float[] tempActiveCost = new float[em.activeEquip.Length];
        for (int i = 0; i < em.activeEquip.Length; i++)
        {
            activeEquipID[i] = em.activeEquip[i].id;
            tempActiveCost[i] = em.activeEquip[i].cost;
        }
        myFile.Add("Active Equip ID List", activeEquipID);
        myFile.Add("Active Equip Cost List", tempActiveCost);
    }

    private void SaveCurrentGameState(GameSettingsPersist gsp)
    {
        if (gsp == null) gsp = FindObjectOfType<GameSettingsPersist>();
        if (gsp == null) return;

        myFile.Add("Tourny Game", gsp.tourny);
        myFile.Add("Game Ends", gsp.ends);
        myFile.Add("Game Active End", gsp.endCurrent);
        myFile.Add("Game Rocks", gsp.rocks);
        myFile.Add("Game Rock Current", gsp.rockCurrent);
        myFile.Add("Game Red Hammer", gsp.redHammer);
        myFile.Add("Game AI Yellow", gsp.aiYellow);
        myFile.Add("Game AI Red", gsp.aiRed);
        myFile.Add("Game Yellow Score", gsp.yellowScore);
        myFile.Add("Game Red Score", gsp.redScore);
        myFile.Add("Game Yellow Team Name", gsp.yellowTeamName);
        myFile.Add("Game Red Team Name", gsp.redTeamName);

        myFile.Add("Game In Progress", gsp.gameInProgress);
        myFile.Add("Tourny In Progress", gsp.tournyInProgress);
        myFile.Add("Game Load", gsp.loadGame);

        float[] rockPosX = new float[gsp.rockPos.Length];
        float[] rockPosY = new float[gsp.rockPos.Length];
        for (int i = 0; i < gsp.rockPos.Length; i++)
        {
            rockPosX[i] = gsp.rockPos[i].x;
            rockPosY[i] = gsp.rockPos[i].y;
        }
        myFile.Add("Game Rock Position X List", rockPosX);
        myFile.Add("Game Rock Position Y List", rockPosY);
        myFile.Add("Game Rock In Play List", gsp.rockInPlay);

        int[] redScoreList = new int[gsp.score.Length];
        int[] yellowScoreList = new int[gsp.score.Length];
        for (int i = 0; i < gsp.score.Length; i++)
        {
            redScoreList[i] = gsp.score[i].x;
            yellowScoreList[i] = gsp.score[i].y;
        }
        myFile.Add("Game Red Score List", redScoreList);
        myFile.Add("Game Yellow Score List", yellowScoreList);
    }

    public void SaveTournyState(object tournyStateManager = null, GameSettingsPersist gsp = null)
    {
        myFile = new EasyFileSave("my_player_data");

        if (gsp == null) gsp = FindObjectOfType<GameSettingsPersist>();

        // Auto-find if not provided
        if (tournyStateManager == null)
        {
            tournyStateManager =
                (object)FindObjectOfType<TournyManager>() ??
                (object)FindObjectOfType<PlayoffManager>() ??
                (object)FindObjectOfType<PlayoffManager_SingleK>() ??
                (object)FindObjectOfType<PlayoffManager_TripleK>();
        }

        if (tournyStateManager is TournyManager tm)
        {
            string tag = "TournyManager_";
            myFile.Add($"{tag}Draw", tm.draw);
            myFile.Add($"{tag}NumberOfTeams", tm.numberOfTeams);
            myFile.Add($"{tag}Prize", tm.prize);
            myFile.Add($"{tag}OppTeam", tm.oppTeam);
            myFile.Add($"{tag}PlayoffRound", tm.playoffRound);

            int teamCount = tm.teams.Length;
            myFile.Add($"{tag}TeamCount", teamCount);

            for (int i = 0; i < teamCount; i++)
            {
                Team t = tm.teams[i];
                myFile.Add($"{tag}Team_{i}_Id", t.id);
                myFile.Add($"{tag}Team_{i}_Name", t.name);
                myFile.Add($"{tag}Team_{i}_Wins", t.wins);
                myFile.Add($"{tag}Team_{i}_Loss", t.loss);
                myFile.Add($"{tag}Team_{i}_Earnings", t.earnings);
                myFile.Add($"{tag}Team_{i}_Player", t.player);
                myFile.Add($"{tag}Team_{i}_Rank", t.rank);
                myFile.Add($"{tag}Team_{i}_Strength", t.strength);
                myFile.Add($"{tag}Team_{i}_NextOpp", t.nextOpp);
            }
        }
        else if (tournyStateManager is PlayoffManager pm)
        {
            string tag = "PlayoffManager_";
            myFile.Add($"{tag}OppTeam", pm.oppTeam);
            myFile.Add($"{tag}PlayoffRound", pm.playoffRound);

            int teamCount = pm.playoffTeams.Length;
            myFile.Add($"{tag}TeamCount", teamCount);

            for (int i = 0; i < teamCount; i++)
            {
                Team t = pm.playoffTeams[i];
                myFile.Add($"{tag}Team_{i}_Id", t.id);
                myFile.Add($"{tag}Team_{i}_Name", t.name);
                myFile.Add($"{tag}Team_{i}_Wins", t.wins);
                myFile.Add($"{tag}Team_{i}_Loss", t.loss);
                myFile.Add($"{tag}Team_{i}_Earnings", t.earnings);
                myFile.Add($"{tag}Team_{i}_Player", t.player);
                myFile.Add($"{tag}Team_{i}_Rank", t.rank);
                myFile.Add($"{tag}Team_{i}_Strength", t.strength);
                myFile.Add($"{tag}Team_{i}_NextOpp", t.nextOpp);
            }
        }
        else if (tournyStateManager is PlayoffManager_SingleK pmSingle)
        {
            string tag = "PlayoffManager_SingleK_";
            myFile.Add($"{tag}OppTeam", pmSingle.oppTeam);
            myFile.Add($"{tag}PlayoffRound", pmSingle.playoffRound);

            int teamCount = pmSingle.playoffTeams.Length;
            myFile.Add($"{tag}TeamCount", teamCount);

            for (int i = 0; i < teamCount; i++)
            {
                Team t = pmSingle.playoffTeams[i];
                myFile.Add($"{tag}Team_{i}_Id", t.id);
                myFile.Add($"{tag}Team_{i}_Name", t.name);
                myFile.Add($"{tag}Team_{i}_Wins", t.wins);
                myFile.Add($"{tag}Team_{i}_Loss", t.loss);
                myFile.Add($"{tag}Team_{i}_Earnings", t.earnings);
                myFile.Add($"{tag}Team_{i}_Player", t.player);
                myFile.Add($"{tag}Team_{i}_Rank", t.rank);
                myFile.Add($"{tag}Team_{i}_Strength", t.strength);
                myFile.Add($"{tag}Team_{i}_NextOpp", t.nextOpp);
            }
        }
        else if (tournyStateManager is PlayoffManager_TripleK pmTriple)
        {
            string tag = "PlayoffManager_TripleK_";
            myFile.Add($"{tag}PlayoffRound", pmTriple.playoffRound);

            int teamCount = pmTriple.teams.Length;
            myFile.Add($"{tag}TeamCount", teamCount);

            for (int i = 0; i < teamCount; i++)
            {
                Team t = pmTriple.teams[i];
                myFile.Add($"{tag}Team_{i}_Id", t.id);
                myFile.Add($"{tag}Team_{i}_Name", t.name);
                myFile.Add($"{tag}Team_{i}_Wins", t.wins);
                myFile.Add($"{tag}Team_{i}_Loss", t.loss);
                myFile.Add($"{tag}Team_{i}_Earnings", t.earnings);
                myFile.Add($"{tag}Team_{i}_Player", t.player);
                myFile.Add($"{tag}Team_{i}_Rank", t.rank);
                myFile.Add($"{tag}Team_{i}_Strength", t.strength);
                myFile.Add($"{tag}Team_{i}_NextOpp", t.nextOpp);
            }

            myFile.Add($"{tag}GameListCount", pmTriple.gameList.Length);
            for (int i = 0; i < pmTriple.gameList.Length; i++)
            {
                myFile.Add($"{tag}GameList_{i}_X", pmTriple.gameList[i].x);
                myFile.Add($"{tag}GameList_{i}_Y", pmTriple.gameList[i].y);
            }
        }
        else
        {
            Debug.LogWarning("Unknown tournament/playoff manager type passed to SaveTournyState.");
            return;
        }

        myFile.Append();
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
        if (gsp == null) gsp = FindObjectOfType<GameSettingsPersist>();
        if (teamSel == null) teamSel = FindObjectOfType<TeamMenu>();
        if (tSel == null) tSel = FindObjectOfType<TournySelector>();
        if (slm == null) slm = FindObjectOfType<StorylineManager>();
        if (em == null) em = FindObjectOfType<EquipmentManager>();
        if (sm == null) sm = FindObjectOfType<SponsorManager>();

        myFile = new EasyFileSave("my_player_data");

        // Save main game progress
        SaveGameProgress(gsp);

        // Save active players
        SaveActivePlayers(teamSel);

        // Save teams and players
        SaveTeamsToSave();

        // Save dialogue status
        SaveDialogueStatus(slm);

        // Save card data
        SaveCardData(sm);

        // Save tournament data
        SaveTournamentData(tSel);

        // Save tour/team records
        SaveTourTeamData();

        // Save team details (summary lists)
        SaveTeamDetails();

        // Save equipment
        SaveEquipment(em);

        SaveCurrentGameState(gsp);
        SaveTournyState();
        loadedFromSave = false;
        myFile.Append();
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
            if (currentTourny.ko1)
                gsp.KO1 = true;
            else
                gsp.KO1 = false;
            gsp.KO3 = false;
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
        //SaveCareer();
    }

    public void TournyResults()
    {
        Debug.Log("Tourny Results in CM");
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        TournyManager tm = FindObjectOfType<TournyManager>();

        cashDelta = gsp.tournyEarnings;
        cash += cashDelta;
        earnings += cashDelta;

        float xpChange = 0f;
        if (!gsp.cashGame)
        {
            currentTournyTeams = gsp.teams;

            for (int i = 0; i < teamRecords.Length; i++)
            {
                if (teamRecords[i].w == gsp.playerTeamIndex)
                {
                    record.x += teamRecords[i].x;
                    record.y += teamRecords[i].y;
                    earnings += teamRecords[i].z;
                }
            }

            for (int i = 0; i < tourRecords.Length; i++)
            {
                if (tourRecords[i].w == gsp.playerTeamIndex)
                {
                    record.x += tourRecords[i].x;
                    record.y += tourRecords[i].y;
                    earnings += tourRecords[i].z;
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

                    if (currentTournyTeams[i].rank < 5)
                    {
                        xpChange += 5f;
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
                    if (currentTournyTeams[i].rank < 5)
                    {
                        provQual = true;
                        xpChange += 25f;
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
                    xpChange += 100f; // Winner
                else if (GetPlayerRank() <= 4)
                    xpChange += 60f; // Top 4
                else
                    xpChange += 30f; // Participation
            }
        }
        else if (currentTourny.championship)
        {
            // Championship event
            if (playerTeamIndex == GetPlayerRankedTeamId())
            {
                if (GetPlayerRank() == 1)
                    xpChange += 200f;
                else if (GetPlayerRank() <= 4)
                    xpChange += 120f;
                else
                    xpChange += 60f;
            }
        }
        else if (currentTourny.qualifier)
        {
            // Qualifier event
            if (playerTeamIndex == GetPlayerRankedTeamId())
            {
                if (GetPlayerRank() < 5)
                    xpChange += 50f;
                else
                    xpChange += 20f;
            }
        }
        else
        {
            // Local or regular tournament
            if (playerTeamIndex == GetPlayerRankedTeamId())
            {
                if (GetPlayerRank() == 1)
                    xpChange += 40f;
                else if (GetPlayerRank() <= 4)
                    xpChange += 20f;
                else
                    xpChange += 10f;
            }
        }

        // Optionally, add XP for wins/losses
        if (playerTeamIndex == GetPlayerRankedTeamId())
        {
            xpChange += GetPlayerWins() * 3f;
            xpChange += GetPlayerLosses() * 1f;
        }
        //if (allTimeTrophyList.Count <= 0)
        //    allTimeTrophyList = new List<bool>();

        Debug.Log("allTimeTrophyList2 Count - " + currentTrophyList.Count);
        xp += xpChange;
        provRankList.Sort();
        week++;
        Debug.Log("CM Tourny Results week is " + week);
        SaveCareer();
    }

    public void PlayTourny()
    {
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        Debug.Log("CM Play Tourny");
        inProgress = true;

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
        SaveCareer();
        //tSel.SetUp();
    }

    public void ContinueSeason()
    {
        Debug.Log("CM - Continue Season");

        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
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
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        TournySelector tSel = FindObjectOfType<TournySelector>();

        provRankList = new List<Standings_List>();
        tourRankList = new List<TourStandings_List>();

        xp = 0f;
        skillPoints = 0;
        level = 0;

        record = new Vector2(0f, 0f);
        tourRecord = new Vector2(0f, 0f);

        cStats.drawAccuracy = 45;
        cStats.guardAccuracy = 45;
        cStats.takeOutAccuracy = 45;
        cStats.sweepStrength = 45;
        cStats.sweepEndurance = 45;
        cStats.sweepCohesion = 45;

        season++;

        Shuffle(teamPool);

        teams = new Team[totalTeams];
        tourTeams = new Team[totalTourTeams];
        Debug.Log("provRankList Count is " + provRankList.Count);

        string[] firstNames = { "Alex", "Jamie", "Taylor", "Jordan", "Morgan", "Casey", "Riley", "Drew", "Sam", "Cameron" };
        string[] lastNames = { "Smith", "Johnson", "Lee", "Brown", "Wilson", "Moore", "Clark", "Hall", "Young", "King" };

        int playersPerTeam = 4;
        int statTotal = 300;
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

            for (int p = 0; p < playersPerTeam; p++)
            {
                Player player = new Player();
                string firstName = firstNames[rand.Next(firstNames.Length)];
                string lastName = (p == 3) ? teams[i].name : lastNames[rand.Next(lastNames.Length)];
                player.name = firstName + " " + lastName;
                player.id = p;

                float[] weights = roleWeights[p];
                int[] stats = new int[6];
                int assigned = 0;

                // Assign stats based on weights, rounding down
                for (int s = 0; s < 6; s++)
                {
                    stats[s] = (int)(statTotal * weights[s]);
                    assigned += stats[s];
                }
                // Distribute any remaining points randomly
                int remaining = statTotal - assigned;
                for (int r = 0; r < remaining; r++)
                {
                    int idx = rand.Next(6);
                    stats[idx]++;
                }

                player.draw = stats[0];
                player.takeOut = stats[1];
                player.guard = stats[2];
                player.sweepStrength = stats[3];
                player.sweepEnduro = stats[4];
                player.sweepCohesion = stats[5];

                teams[i].players.Add(player);
            }

            teams[i].UpdateTeamSkillsFromPlayers();
            provRankList.Add(new Standings_List(teams[i]));
        }

        provQual = false;
        tourQual = false;

        earnings = 0f;
        teams[0].name = teamName;
        teams[0].player = true;
        teams[0].earnings = earnings;
        cash = 1000f;
        playerTeamIndex = teams[0].id;
        gsp.playerTeamIndex = playerTeamIndex;

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
    }

    public void EndCareer()
    {
        Debug.Log("Ending Career");
        gameOver = true;
        SaveCareer();
        StartCoroutine(SaveHighScore());
    }

    public void SavePlayoffState(Team[] teams, int playoffRound, Vector2[] gameList = null)
    {
        myFile = new EasyFileSave("my_player_data");
        myFile.Add("PlayoffRound", playoffRound);

        int teamCount = teams.Length;
        myFile.Add("TeamCount", teamCount);

        for (int i = 0; i < teamCount; i++)
        {
            Team t = teams[i];
            myFile.Add($"Team_{i}_Id", t.id);
            myFile.Add($"Team_{i}_Name", t.name);
            myFile.Add($"Team_{i}_Wins", t.wins);
            myFile.Add($"Team_{i}_Loss", t.loss);
            myFile.Add($"Team_{i}_Earnings", t.earnings);
            myFile.Add($"Team_{i}_Player", t.player);
            myFile.Add($"Team_{i}_Rank", t.rank);
            myFile.Add($"Team_{i}_Strength", t.strength);
            myFile.Add($"Team_{i}_NextOpp", t.nextOpp);
        }

        if (gameList != null)
        {
            myFile.Add("GameListCount", gameList.Length);
            for (int i = 0; i < gameList.Length; i++)
            {
                myFile.Add($"GameList_{i}_X", gameList[i].x);
                myFile.Add($"GameList_{i}_Y", gameList[i].y);
            }
        }

        myFile.Append();
    }

    public void LoadPlayoffState(out Team[] teams, out int playoffRound, out Vector2[] gameList)
    {
        myFile = new EasyFileSave("my_playoff_data");
        teams = null;
        playoffRound = 0;
        gameList = null;

        if (!myFile.Load())
            return;

        playoffRound = myFile.GetInt("PlayoffRound");
        int teamCount = myFile.GetInt("TeamCount");
        teams = new Team[teamCount];

        for (int i = 0; i < teamCount; i++)
        {
            Team t = new Team();
            t.id = myFile.GetInt($"Team_{i}_Id");
            t.name = myFile.GetString($"Team_{i}_Name");
            t.wins = myFile.GetInt($"Team_{i}_Wins");
            t.loss = myFile.GetInt($"Team_{i}_Loss");
            t.earnings = myFile.GetFloat($"Team_{i}_Earnings");
            t.player = myFile.GetBool($"Team_{i}_Player");
            t.rank = myFile.GetInt($"Team_{i}_Rank");
            t.strength = myFile.GetInt($"Team_{i}_Strength");
            t.nextOpp = myFile.GetString($"Team_{i}_NextOpp");
            teams[i] = t;
        }

        int gameListCount = myFile.GetInt("GameListCount");
        gameList = new Vector2[gameListCount];
        for (int i = 0; i < gameListCount; i++)
        {
            float x = myFile.GetFloat($"GameList_{i}_X");
            float y = myFile.GetFloat($"GameList_{i}_Y");
            gameList[i] = new Vector2(x, y);
        }
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
}
