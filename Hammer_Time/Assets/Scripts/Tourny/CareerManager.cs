using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TigerForge;
using System.Net.Security;

public class CareerManager : MonoBehaviour
{
    public static CareerManager instance;
    TournySettings ts;
    EasyFileSave myFile;
    CareerSettings cs;

    public int week;
    public int seasonLength;
    public string playerName;
    public string teamName;
    public Color teamColour;
    public int playerTeamIndex;
    public float earnings;
    public float cash;
    public Vector2 record;
    public bool provQual;
    public bool tourQual;
    public Vector2 tourRecord;

    public int storyBlock;

    public Player[] activePlayers;

    public float xp;
    public float totalXp;

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
    public List<int> tournyResults;

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
        
        SetUpCareer();
    }

    public void SetUpCareer()
    {
        TournyManager tm = FindObjectOfType<TournyManager>();
        PlayoffManager pm = FindObjectOfType<PlayoffManager>();
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        TournySelector tSel = FindObjectOfType<TournySelector>();
        TournyTeamList tTeamList = FindObjectOfType<TournyTeamList>();
        StorylineManager slm = FindObjectOfType<StorylineManager>();
        TeamMenu teamSel = FindObjectOfType<TeamMenu>();
        SponsorManager pUpM = FindObjectOfType<SponsorManager>();
        GameManager gm = FindObjectOfType<GameManager>();
        EquipmentManager em = FindObjectOfType<EquipmentManager>();

        LoadCareer2(gsp, tSel, tTeamList, slm, teamSel, pUpM, gm, em);
        //inProgress
        //if (inProgress)
        //{
        //}
        //else
        //    NewSeason(gsp, tSel, tTeamList);
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

    public void LoadFromGSP(GameSettingsPersist gsp)
    {
        //earnings = gsp.earnings;
        //record = gsp.record;
        SaveCareer();
        //Debug.Log("Earnings - CM from GSP - " + earnings);
    }

    public void LoadCareer(GameSettingsPersist gsp = null,
        TournySelector tSel = null,
        TournyTeamList tTeamList = null,
        StorylineManager slm = null,
        TeamMenu teamSel = null,
        SponsorManager pUpM = null,
        GameManager gm = null,
        EquipmentManager em = null)
    {
        Debug.Log("Loading in CM");

        //teamRecords = new Vector3[totalTeams];

        if (provRankList != null)
        {
            provRankList.Clear();
        }

        if (tourRankList != null)
        {
            tourRankList.Clear();
        }

        myFile = new EasyFileSave("my_player_data");

        tourTeams = new Team[totalTourTeams];

        if (myFile.Load())
        {
            gsp.tournyInProgress = myFile.GetBool("Tourny In Progress");
            gsp.gameInProgress = myFile.GetBool("Game In Progress");
            Debug.Log("gsp.inProgress is " + gsp.tournyInProgress);

            coachDialogue = myFile.GetArray<bool>("Coach Dialogue Played List");
            qualDialogue = myFile.GetArray<bool>("Qualifying Dialogue Played List");
            reviewDialogue = myFile.GetArray<bool>("Review Dialogue Played List");
            introDialogue = myFile.GetArray<bool>("Intro Dialogue Played List");
            helpDialogue = myFile.GetArray<bool>("Help Dialogue Played List");
            strategyDialogue = myFile.GetArray<bool>("Strategy Dialogue Played List");
            storyDialogue = myFile.GetArray<bool>("Story Dialogue Played List");

            if (slm)
                slm.blockIndex = myFile.GetInt("Story Block");

            gsp.loadGame = myFile.GetBool("Game Load");

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
            totalXp = myFile.GetFloat("Total XP");

            cStats.drawAccuracy = myFile.GetInt("Draw Accuracy");
            cStats.takeOutAccuracy = myFile.GetInt("Take Out Accuracy");
            cStats.guardAccuracy = myFile.GetInt("Guard Accuracy");
            cStats.sweepStrength = myFile.GetInt("Sweep Strength");
            cStats.sweepEndurance = myFile.GetInt("Sweep Endurance");
            cStats.sweepCohesion = myFile.GetInt("Sweep Cohesion");
            gsp.cStats = cStats;
            //tourRecord = myFile.GetUnityVector2("Tour Record");

            if (teamSel)
            {
                int[] playersIdList = myFile.GetArray<int>("Active Players ID List");
                int[] playersDrawList = myFile.GetArray<int>("Active Players Draw List");
                int[] playersGuardList = myFile.GetArray<int>("Active Players Guard List");
                int[] playersTakeOutList = myFile.GetArray<int>("Active Players Takeout List");
                int[] playersStrengthList = myFile.GetArray<int>("Active Players Strength List");
                int[] playersEnduroList = myFile.GetArray<int>("Active Players Endurance List");
                int[] playersCohesionList = myFile.GetArray<int>("Active Players Cohesion List");
                //Debug.Log("Players Id Length - " + playersIdList.Length);

                for (int i = 0; i < playersIdList.Length; i++)
                {
                    //Debug.Log("CM LOADCAREER - Active Players Id - " + i + " - " + playersIdList[i]);
                    teamSel.activePlayers[i].id = playersIdList[i];
                    teamSel.activePlayers[i].draw = playersDrawList[i];
                    teamSel.activePlayers[i].guard = playersGuardList[i];
                    teamSel.activePlayers[i].takeOut = playersTakeOutList[i];
                    teamSel.activePlayers[i].sweepStrength = playersStrengthList[i];
                    teamSel.activePlayers[i].sweepEnduro = playersEnduroList[i];
                    teamSel.activePlayers[i].sweepCohesion = playersCohesionList[i];
                }
            }

            cardPUIDList = myFile.GetArray<int>("Card PowerUp ID List");
            cardSponsorIDList = myFile.GetArray<int>("Card Sponsor ID List");
            activeCardIDList = myFile.GetArray<int>("Active Card ID List");
            playedCardIDList = myFile.GetArray<int>("Played Card ID List");
            activeCardLengthList = myFile.GetArray<int>("Active Card Length List");


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
                    //Debug.Log("prov tourny " + i + " is " + prov[i].complete);
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
                //**//
                gsp.tournyInProgress = false;
            }

            if (tTeamList != null)
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
                    for (int j = 0; j < tTeamList.teams.Length; j++)
                    {
                        if (idList[i] == tTeamList.teams[j].id)
                            teams[i] = tTeamList.teams[j];
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
            }

            else
                Debug.Log("tTeamList = null");

            if (pUpM)
            {
            }

            inventoryID = myFile.GetArray<int>("Inventory ID List");

            activeEquipID = myFile.GetArray<int>("Active Equip ID List");


            if (em != null)
            {
                int[] tempID = myFile.GetArray<int>("Total Item ID List");
                float[] tempCost = myFile.GetArray<float>("Total Item Cost List");
                float[] tempColorX = myFile.GetArray<float>("Total Item Color X List");
                float[] tempColorY = myFile.GetArray<float>("Total Item Color Y List");
                float[] tempColorZ = myFile.GetArray<float>("Total Item Color Z List");
                float[] tempColorA = myFile.GetArray<float>("Total Item Color A List");
                Vector4[] tempColor = new Vector4[tempColorX.Length];
                for (int i = 0; i < tempColor.Length; i++)
                    tempColor[i] = new Vector4(tempColorX[i], tempColorY[i], tempColorZ[i], tempColorA[i]);

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
                for (int i = 0; i < tempID.Length; i++)
                {
                    if (tempID[i] < 30)
                    {
                        em.handles[i] = new Equipment();
                        em.handles[i].id = tempID[i];
                        em.handles[i].cost = tempCost[i];
                        em.handles[i].color = new Color(tempColorX[i], tempColorY[i], tempColorZ[i], tempColorA[i]);
                        em.handles[i].duration = tempDuration[i];
                        em.handles[i].stats[0] = tempStats0[i];
                        em.handles[i].stats[1] = tempStats1[i];
                        em.handles[i].stats[2] = tempStats2[i];
                        em.handles[i].stats[3] = tempStats3[i];
                        em.handles[i].stats[4] = tempStats4[i];
                        em.handles[i].stats[5] = tempStats5[i];
                        em.handles[i].oppStats[0] = tempOppStats0[i];
                        em.handles[i].oppStats[1] = tempOppStats1[i];
                        em.handles[i].oppStats[2] = tempOppStats2[i];
                        em.handles[i].oppStats[3] = tempOppStats3[i];
                        em.handles[i].oppStats[4] = tempOppStats4[i];
                        em.handles[i].oppStats[5] = tempOppStats5[i];
                    }
                    else if (tempID[i] < 60)
                    {
                        int j = i - 30;
                        //Debug.Log("j is " + j);
                        em.heads[j] = new Equipment();
                        em.heads[j].id = tempID[i];
                        em.heads[j].cost = tempCost[i];
                        em.heads[j].color = new Color(tempColorX[i], tempColorY[i], tempColorZ[i], tempColorA[i]);
                        em.heads[j].duration = tempDuration[i];
                        em.heads[j].stats[0] = tempStats0[i];
                        em.heads[j].stats[1] = tempStats1[i];
                        em.heads[j].stats[2] = tempStats2[i];
                        em.heads[j].stats[3] = tempStats3[i];
                        em.heads[j].stats[4] = tempStats4[i];
                        em.heads[j].stats[5] = tempStats5[i];
                        em.heads[j].oppStats[0] = tempOppStats0[i];
                        em.heads[j].oppStats[1] = tempOppStats1[i];
                        em.heads[j].oppStats[2] = tempOppStats2[i];
                        em.heads[j].oppStats[3] = tempOppStats3[i];
                        em.heads[j].oppStats[4] = tempOppStats4[i];
                        em.heads[j].oppStats[5] = tempOppStats5[i];
                    }
                    else if (tempID[i] < 80)
                    {
                        int j = i - 60;
                        em.footwear[j] = new Equipment();
                        em.footwear[j].id = tempID[i];
                        em.footwear[j].cost = tempCost[i];
                        em.footwear[j].color = new Color(tempColorX[i], tempColorY[i], tempColorZ[i], tempColorA[i]);
                        em.footwear[j].duration = tempDuration[i];
                        em.footwear[j].stats[0] = tempStats0[i];
                        em.footwear[j].stats[1] = tempStats1[i];
                        em.footwear[j].stats[2] = tempStats2[i];
                        em.footwear[j].stats[3] = tempStats3[i];
                        em.footwear[j].stats[4] = tempStats4[i];
                        em.footwear[j].stats[5] = tempStats5[i];
                        em.footwear[j].oppStats[0] = tempOppStats0[i];
                        em.footwear[j].oppStats[1] = tempOppStats1[i];
                        em.footwear[j].oppStats[2] = tempOppStats2[i];
                        em.footwear[j].oppStats[3] = tempOppStats3[i];
                        em.footwear[j].oppStats[4] = tempOppStats4[i];
                        em.footwear[j].oppStats[5] = tempOppStats5[i];
                    }
                    else
                    {
                        int j = i - 80;
                        em.apparel[j] = new Equipment();
                        em.apparel[j].id = tempID[i];
                        em.apparel[j].cost = tempCost[i];
                        em.apparel[j].color = new Color(tempColorX[i], tempColorY[i], tempColorZ[i], tempColorA[i]);
                        em.apparel[j].duration = tempDuration[i];
                        em.apparel[j].stats[0] = tempStats0[i];
                        em.apparel[j].stats[1] = tempStats1[i];
                        em.apparel[j].stats[2] = tempStats2[i];
                        em.apparel[j].stats[3] = tempStats3[i];
                        em.apparel[j].stats[4] = tempStats4[i];
                        em.apparel[j].stats[5] = tempStats5[i];
                        em.apparel[j].oppStats[0] = tempOppStats0[i];
                        em.apparel[j].oppStats[1] = tempOppStats1[i];
                        em.apparel[j].oppStats[2] = tempOppStats2[i];
                        em.apparel[j].oppStats[3] = tempOppStats3[i];
                        em.apparel[j].oppStats[4] = tempOppStats4[i];
                        em.apparel[j].oppStats[5] = tempOppStats5[i];
                    }
                }
            }

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

            if (tTeamList != null)
            {
                for (int i = 0; i < teams.Length; i++)
                {
                    provRankList.Add(new Standings_List(teams[i]));
                }

                for (int i = 0; i < tourTeams.Length; i++)
                {
                    tourRankList.Add(new TourStandings_List(tourTeams[i]));
                }

                for (int i = 0; i < teams.Length; i++)
                {
                    if (teams[i].player)
                    {
                        playerTeam = teams[i];
                        //Debug.Log("CM playerTeamIndex is " + teams[i].id);
                    }
                }
            }

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
            myFile.Dispose();
        }
        //else
        //{
        //    NewSeason();
        //}
    }

    private void LoadCareer2(GameSettingsPersist gsp = null,
    TournySelector tSel = null,
    TournyTeamList tTeamList = null,
    StorylineManager slm = null,
    TeamMenu teamSel = null,
    SponsorManager pUpM = null,
    GameManager gm = null,
    EquipmentManager em = null)
    {
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
            LoadTeamDetails(tTeamList);

            LoadSponsorManager(pUpM);
            LoadEquipment(em);
            LoadTourTeamData();
            UpdateCurrentTourny(gsp);
            LoadGameSettings(gsp);

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
            int[] playersDrawList = myFile.GetArray<int>("Active Players Draw List");
            int[] playersGuardList = myFile.GetArray<int>("Active Players Guard List");
            int[] playersTakeOutList = myFile.GetArray<int>("Active Players Takeout List");
            int[] playersStrengthList = myFile.GetArray<int>("Active Players Strength List");
            int[] playersEnduroList = myFile.GetArray<int>("Active Players Endurance List");
            int[] playersCohesionList = myFile.GetArray<int>("Active Players Cohesion List");
            //Debug.Log("Players Id Length - " + playersIdList.Length);

            for (int i = 0; i < playersIdList.Length; i++)
            {
                //Debug.Log("CM LOADCAREER - Active Players Id - " + i + " - " + playersIdList[i]);
                teamSel.activePlayers[i].id = playersIdList[i];
                teamSel.activePlayers[i].draw = playersDrawList[i];
                teamSel.activePlayers[i].guard = playersGuardList[i];
                teamSel.activePlayers[i].takeOut = playersTakeOutList[i];
                teamSel.activePlayers[i].sweepStrength = playersStrengthList[i];
                teamSel.activePlayers[i].sweepEnduro = playersEnduroList[i];
                teamSel.activePlayers[i].sweepCohesion = playersCohesionList[i];
            }
        }
    }

    private void LoadGameProgress(GameSettingsPersist gsp)
    {
        gsp.tournyInProgress = myFile.GetBool("Tourny In Progress");
        gsp.gameInProgress = myFile.GetBool("Game In Progress");
        Debug.Log("gsp.inProgress is " + gsp.tournyInProgress);

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
        totalXp = myFile.GetFloat("Total XP");

        cStats.drawAccuracy = myFile.GetInt("Draw Accuracy");
        cStats.takeOutAccuracy = myFile.GetInt("Take Out Accuracy");
        cStats.guardAccuracy = myFile.GetInt("Guard Accuracy");
        cStats.sweepStrength = myFile.GetInt("Sweep Strength");
        cStats.sweepEndurance = myFile.GetInt("Sweep Endurance");
        cStats.sweepCohesion = myFile.GetInt("Sweep Cohesion");
        gsp.cStats = cStats;
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
        cardSponsorIDList = myFile.GetArray<int>("Card Sponsor ID List");
        activeCardIDList = myFile.GetArray<int>("Active Card ID List");
        playedCardIDList = myFile.GetArray<int>("Played Card ID List");
        activeCardLengthList = myFile.GetArray<int>("Active Card Length List");
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
                //Debug.Log("prov tourny " + i + " is " + prov[i].complete);
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
            //**//
            //gsp.tournyInProgress = false;
        }
    }

    private void LoadSponsorManager(SponsorManager pUpM)
    {

    }

    private void LoadTeamDetails(TournyTeamList tTeamList)
    {

        if (tTeamList != null)
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
                for (int j = 0; j < tTeamList.teams.Length; j++)
                {
                    if (idList[i] == tTeamList.teams[j].id)
                        teams[i] = tTeamList.teams[j];
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

        else
            Debug.Log("tTeamList = null");
    }

    private void LoadEquipment(EquipmentManager em)
    {
        inventoryID = myFile.GetArray<int>("Inventory ID List");

        activeEquipID = myFile.GetArray<int>("Active Equip ID List");

        if (em != null)
        {
            int[] tempID = myFile.GetArray<int>("Total Item ID List");
            float[] tempCost = myFile.GetArray<float>("Total Item Cost List");
            float[] tempColorX = myFile.GetArray<float>("Total Item Color X List");
            float[] tempColorY = myFile.GetArray<float>("Total Item Color Y List");
            float[] tempColorZ = myFile.GetArray<float>("Total Item Color Z List");
            float[] tempColorA = myFile.GetArray<float>("Total Item Color A List");
            Vector4[] tempColor = new Vector4[tempColorX.Length];
            for (int i = 0; i < tempColor.Length; i++)
                tempColor[i] = new Vector4(tempColorX[i], tempColorY[i], tempColorZ[i], tempColorA[i]);

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
            for (int i = 0; i < tempID.Length; i++)
            {
                if (tempID[i] < 30)
                {
                    em.handles[i] = new Equipment();
                    em.handles[i].id = tempID[i];
                    em.handles[i].cost = tempCost[i];
                    em.handles[i].color = new Color(tempColorX[i], tempColorY[i], tempColorZ[i], tempColorA[i]);
                    em.handles[i].duration = tempDuration[i];
                    em.handles[i].stats[0] = tempStats0[i];
                    em.handles[i].stats[1] = tempStats1[i];
                    em.handles[i].stats[2] = tempStats2[i];
                    em.handles[i].stats[3] = tempStats3[i];
                    em.handles[i].stats[4] = tempStats4[i];
                    em.handles[i].stats[5] = tempStats5[i];
                    em.handles[i].oppStats[0] = tempOppStats0[i];
                    em.handles[i].oppStats[1] = tempOppStats1[i];
                    em.handles[i].oppStats[2] = tempOppStats2[i];
                    em.handles[i].oppStats[3] = tempOppStats3[i];
                    em.handles[i].oppStats[4] = tempOppStats4[i];
                    em.handles[i].oppStats[5] = tempOppStats5[i];
                }
                else if (tempID[i] < 60)
                {
                    int j = i - 30;
                    //Debug.Log("j is " + j);
                    em.heads[j] = new Equipment();
                    em.heads[j].id = tempID[i];
                    em.heads[j].cost = tempCost[i];
                    em.heads[j].color = new Color(tempColorX[i], tempColorY[i], tempColorZ[i], tempColorA[i]);
                    em.heads[j].duration = tempDuration[i];
                    em.heads[j].stats[0] = tempStats0[i];
                    em.heads[j].stats[1] = tempStats1[i];
                    em.heads[j].stats[2] = tempStats2[i];
                    em.heads[j].stats[3] = tempStats3[i];
                    em.heads[j].stats[4] = tempStats4[i];
                    em.heads[j].stats[5] = tempStats5[i];
                    em.heads[j].oppStats[0] = tempOppStats0[i];
                    em.heads[j].oppStats[1] = tempOppStats1[i];
                    em.heads[j].oppStats[2] = tempOppStats2[i];
                    em.heads[j].oppStats[3] = tempOppStats3[i];
                    em.heads[j].oppStats[4] = tempOppStats4[i];
                    em.heads[j].oppStats[5] = tempOppStats5[i];
                }
                else if (tempID[i] < 80)
                {
                    int j = i - 60;
                    em.footwear[j] = new Equipment();
                    em.footwear[j].id = tempID[i];
                    em.footwear[j].cost = tempCost[i];
                    em.footwear[j].color = new Color(tempColorX[i], tempColorY[i], tempColorZ[i], tempColorA[i]);
                    em.footwear[j].duration = tempDuration[i];
                    em.footwear[j].stats[0] = tempStats0[i];
                    em.footwear[j].stats[1] = tempStats1[i];
                    em.footwear[j].stats[2] = tempStats2[i];
                    em.footwear[j].stats[3] = tempStats3[i];
                    em.footwear[j].stats[4] = tempStats4[i];
                    em.footwear[j].stats[5] = tempStats5[i];
                    em.footwear[j].oppStats[0] = tempOppStats0[i];
                    em.footwear[j].oppStats[1] = tempOppStats1[i];
                    em.footwear[j].oppStats[2] = tempOppStats2[i];
                    em.footwear[j].oppStats[3] = tempOppStats3[i];
                    em.footwear[j].oppStats[4] = tempOppStats4[i];
                    em.footwear[j].oppStats[5] = tempOppStats5[i];
                }
                else
                {
                    int j = i - 80;
                    em.apparel[j] = new Equipment();
                    em.apparel[j].id = tempID[i];
                    em.apparel[j].cost = tempCost[i];
                    em.apparel[j].color = new Color(tempColorX[i], tempColorY[i], tempColorZ[i], tempColorA[i]);
                    em.apparel[j].duration = tempDuration[i];
                    em.apparel[j].stats[0] = tempStats0[i];
                    em.apparel[j].stats[1] = tempStats1[i];
                    em.apparel[j].stats[2] = tempStats2[i];
                    em.apparel[j].stats[3] = tempStats3[i];
                    em.apparel[j].stats[4] = tempStats4[i];
                    em.apparel[j].stats[5] = tempStats5[i];
                    em.apparel[j].oppStats[0] = tempOppStats0[i];
                    em.apparel[j].oppStats[1] = tempOppStats1[i];
                    em.apparel[j].oppStats[2] = tempOppStats2[i];
                    em.apparel[j].oppStats[3] = tempOppStats3[i];
                    em.apparel[j].oppStats[4] = tempOppStats4[i];
                    em.apparel[j].oppStats[5] = tempOppStats5[i];
                }
            }
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
        gsp.cStats = cStats;

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

    public void SaveCareer()
    {
        TournySelector tSel = FindObjectOfType<TournySelector>();
        TournyManager tm = FindObjectOfType<TournyManager>();
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        SponsorManager pUpM = FindObjectOfType<SponsorManager>();
        TeamMenu teamSel = FindObjectOfType<TeamMenu>();
        GameManager gm = FindObjectOfType<GameManager>();
        EquipmentManager em = FindObjectOfType<EquipmentManager>();

        Debug.Log("Saving Career - " + gsp.tournyInProgress);
        myFile = new EasyFileSave("my_player_data");

        myFile.Add("Story Block", storyBlock);
        myFile.Add("Coach Dialogue Played List", coachDialogue);
        myFile.Add("Qualifying Dialogue Played List", qualDialogue);
        myFile.Add("Review Dialogue Played List", reviewDialogue);
        myFile.Add("Intro Dialogue Played List", introDialogue);
        myFile.Add("Help Dialogue Played List", helpDialogue);
        myFile.Add("Strategy Dialogue Played List", strategyDialogue);
        myFile.Add("Story Dialogue Played List", storyDialogue);

        myFile.Add("Tourny In Progress", gsp.tournyInProgress);
        Debug.Log("gsp.tournyInProgress is " + gsp.tournyInProgress);
        //myFile.Add("Tourny In Progress", true);
        myFile.Add("Game In Progress", gsp.gameInProgress);
        myFile.Add("Game Load", gsp.loadGame);
        myFile.Add("Knockout Tourny", false);
        myFile.Add("Player Name", playerName);
        myFile.Add("Team Name", teamName);
        myFile.Add("Team Colour", teamColour);
        myFile.Add("Player Team Index", playerTeamIndex);
        myFile.Add("Week", week);
        Debug.Log("CM Save Career week is " + week);
        myFile.Add("Season", season);
        //myFile.Add("Career Record", record);
        myFile.Add("Career Cash", cash);
        myFile.Add("Career Earnings", earnings);
        myFile.Add("Game Over", gameOver);
        myFile.Add("Prov Qual", provQual);
        myFile.Add("Tour Qual", tourQual);
        myFile.Add("Tour Record", tourRecord);
        myFile.Add("Total XP", totalXp);
        myFile.Add("XP", xp);
        myFile.Add("Draw Accuracy", cStats.drawAccuracy);
        myFile.Add("Take Out Accuracy", cStats.takeOutAccuracy);
        myFile.Add("Guard Accuracy", cStats.guardAccuracy);
        myFile.Add("Sweep Strength", cStats.sweepStrength);
        myFile.Add("Sweep Endurance", cStats.sweepEndurance);
        myFile.Add("Sweep Cohesion", cStats.sweepCohesion);

        int[] idList = new int[teams.Length];
        int[] winsList = new int[teams.Length];
        int[] lossList = new int[teams.Length];
        float[] earningsList = new float[teams.Length];


        for (int i = 0; i < teams.Length; i++)
        {
            idList[i] = teams[i].id;
            //Debug.Log("Id List - " + idList[i]);
            winsList[i] = teams[i].wins;
            lossList[i] = teams[i].loss;
            earningsList[i] = teams[i].earnings;

            if (playerTeamIndex == teams[i].id)
            {
                myFile.Add("Career Record", new Vector2(teams[i].wins, teams[i].loss));
            }

        }

        //Debug.Log("Total Id List length is " + idList.Length);
        myFile.Add("Total ID List", idList);
        myFile.Add("Total Wins List", winsList);
        myFile.Add("Total Loss List", lossList);
        myFile.Add("Total Earnings List", earningsList);


        //Debug.Log("Tour Teams length is " + tourTeams.Length);

        int[] tourTeamIDList = new int[tourTeams.Length];
        int[] tourWinsList = new int[tourTeams.Length];
        int[] tourLossList = new int[tourTeams.Length];
        float[] tourPointsList = new float[tourTeams.Length];

        Debug.Log("Tour Record length is " + tourWinsList.Length + " - " + tourLossList.Length);

        if (tourTeams.Length > 0)
        {
            for (int i = 0; i < tourTeamIDList.Length; i++)
            {
                tourTeamIDList[i] = tourTeams[i].id;
                //Debug.Log("Id List - " + idList[i]);
                tourWinsList[i] = (int)tourTeams[i].tourRecord.x;
                tourLossList[i] = (int)tourTeams[i].tourRecord.y;
                tourPointsList[i] = tourTeams[i].tourPoints;
                //Debug.Log("Tour Points Length - " + tourPointsList.Length);
                //Debug.Log("Tour Points - " + i + " - " + tourPointsList[i]);
            }
        }

        myFile.Add("Tour Team ID List", tourTeamIDList);
        myFile.Add("Tour Wins List", tourWinsList);
        myFile.Add("Tour Loss List", tourLossList);
        myFile.Add("Tour Points List", tourPointsList);


        int[] playerIdList = new int[activePlayers.Length];
        string[] playerNameList = new string[activePlayers.Length];

        int[] playerDrawList = new int[activePlayers.Length];
        int[] playerGuardList = new int[activePlayers.Length];
        int[] playerTakeoutList = new int[activePlayers.Length];
        int[] playerStrengthList = new int[activePlayers.Length];
        int[] playerEnduroList = new int[activePlayers.Length];
        int[] playerCohesionList = new int[activePlayers.Length];

        int[] playerOppDrawList = new int[activePlayers.Length];
        int[] playerOppGuardList = new int[activePlayers.Length];
        int[] playerOppTakeoutList = new int[activePlayers.Length];
        int[] playerOppStrengthList = new int[activePlayers.Length];
        int[] playerOppEnduroList = new int[activePlayers.Length];
        int[] playerOppCohesionList = new int[activePlayers.Length];

        for (int i = 0; i < playerIdList.Length; i++)
        {
            playerIdList[i] = activePlayers[i].id;
            //Debug.Log("CM SAVECAREER Active Player List " + i + " - " + playerIdList[i]);
            playerNameList[i] = activePlayers[i].name;

            playerDrawList[i] = activePlayers[i].draw;
            playerGuardList[i] = activePlayers[i].guard;
            playerTakeoutList[i] = activePlayers[i].takeOut;
            playerStrengthList[i] = activePlayers[i].sweepStrength;
            playerEnduroList[i] = activePlayers[i].sweepEnduro;
            playerCohesionList[i] = activePlayers[i].sweepCohesion;

            playerOppDrawList[i] = activePlayers[i].oppDraw;
            playerOppGuardList[i] = activePlayers[i].oppGuard;
            playerOppTakeoutList[i] = activePlayers[i].oppTakeOut;
            playerOppStrengthList[i] = activePlayers[i].oppStrength;
            playerOppEnduroList[i] = activePlayers[i].oppEnduro;
            playerOppCohesionList[i] = activePlayers[i].oppCohesion;
        }

        myFile.Add("Active Players ID List", playerIdList);
        myFile.Add("Active Players Name List", playerNameList);
        myFile.Add("Active Players Draw List", playerDrawList);
        myFile.Add("Active Players Guard List", playerGuardList);
        myFile.Add("Active Players Takeout List", playerTakeoutList);
        myFile.Add("Active Players Strength List", playerStrengthList);
        myFile.Add("Active Players Endurance List", playerEnduroList);
        myFile.Add("Active Players Cohesion List", playerCohesionList);
        myFile.Add("Active Players Opp Draw List", playerOppDrawList);
        myFile.Add("Active Players Opp Guard List", playerOppGuardList);
        myFile.Add("Active Players Opp Takeout List", playerOppTakeoutList);
        myFile.Add("Active Players Opp Strength List", playerOppStrengthList);
        myFile.Add("Active Players Opp Endurance List", playerOppEnduroList);
        myFile.Add("Active Players Opp Cohesion List", playerOppCohesionList);


        if (pUpM != null)
        {

            
        }

        myFile.Add("Current Tourny Name", currentTourny.name);
        myFile.Add("Current Tourny ID", currentTourny.id);
        myFile.Add("Current Tourny Tour", currentTourny.tour);
        myFile.Add("Current Tourny Qualifier", currentTourny.qualifier);
        myFile.Add("Current Tourny Championship", currentTourny.championship);
        myFile.Add("Prize Money", currentTourny.prizeMoney);
        myFile.Add("Current Tourny BG", currentTourny.BG);
        myFile.Add("Current Tourny Crowd Density", currentTourny.crowdDensity);

        if (em != null)
        {
            int[] tempID = new int[em.handles.Length + em.heads.Length + em.footwear.Length + em.apparel.Length];
            float[] tempCost = new float[em.handles.Length + em.heads.Length + em.footwear.Length + em.apparel.Length];
            float[] tempColorX = new float[em.handles.Length + em.heads.Length + em.footwear.Length + em.apparel.Length];
            float[] tempColorY = new float[em.handles.Length + em.heads.Length + em.footwear.Length + em.apparel.Length];
            float[] tempColorZ = new float[em.handles.Length + em.heads.Length + em.footwear.Length + em.apparel.Length];
            float[] tempColorA = new float[em.handles.Length + em.heads.Length + em.footwear.Length + em.apparel.Length];
            int[] tempDuration = new int[em.handles.Length + em.heads.Length + em.footwear.Length + em.apparel.Length];
            int[] tempStats0 = new int[em.handles.Length + em.heads.Length + em.footwear.Length + em.apparel.Length];
            int[] tempStats1 = new int[em.handles.Length + em.heads.Length + em.footwear.Length + em.apparel.Length];
            int[] tempStats2 = new int[em.handles.Length + em.heads.Length + em.footwear.Length + em.apparel.Length];
            int[] tempStats3 = new int[em.handles.Length + em.heads.Length + em.footwear.Length + em.apparel.Length];
            int[] tempStats4 = new int[em.handles.Length + em.heads.Length + em.footwear.Length + em.apparel.Length];
            int[] tempStats5 = new int[em.handles.Length + em.heads.Length + em.footwear.Length + em.apparel.Length];

            int[] tempOppStats0 = new int[em.handles.Length + em.heads.Length + em.footwear.Length + em.apparel.Length];
            int[] tempOppStats1 = new int[em.handles.Length + em.heads.Length + em.footwear.Length + em.apparel.Length];
            int[] tempOppStats2 = new int[em.handles.Length + em.heads.Length + em.footwear.Length + em.apparel.Length];
            int[] tempOppStats3 = new int[em.handles.Length + em.heads.Length + em.footwear.Length + em.apparel.Length];
            int[] tempOppStats4 = new int[em.handles.Length + em.heads.Length + em.footwear.Length + em.apparel.Length];
            int[] tempOppStats5 = new int[em.handles.Length + em.heads.Length + em.footwear.Length + em.apparel.Length];

            for (int i = 0; i < em.handles.Length; i++)
            {
                tempID[i] = em.handles[i].id;
                tempCost[i] = em.handles[i].cost;
                tempColorX[i] = em.handles[i].color.r;
                tempColorY[i] = em.handles[i].color.g;
                tempColorZ[i] = em.handles[i].color.b;
                tempColorA[i] = em.handles[i].color.a;
                tempDuration[i] = em.handles[i].duration;
                tempStats0[i] = em.handles[i].stats[0];
                tempStats1[i] = em.handles[i].stats[1];
                tempStats2[i] = em.handles[i].stats[2];
                tempStats3[i] = em.handles[i].stats[3];
                tempStats4[i] = em.handles[i].stats[4];
                tempStats5[i] = em.handles[i].stats[5];
                tempOppStats0[i] = em.handles[i].oppStats[0];
                tempOppStats1[i] = em.handles[i].oppStats[1];
                tempOppStats2[i] = em.handles[i].oppStats[2];
                tempOppStats3[i] = em.handles[i].oppStats[3];
                tempOppStats4[i] = em.handles[i].oppStats[4];
                tempOppStats5[i] = em.handles[i].oppStats[5];
            }

            int j = em.heads[0].id;
            for (int i = 0; i < em.heads.Length; i++)
            {
                tempID[j] = em.heads[i].id;
                tempCost[j] = em.heads[i].cost;
                tempColorX[j] = em.heads[i].color.r;
                tempColorY[j] = em.heads[i].color.g;
                tempColorZ[j] = em.heads[i].color.b;
                tempColorA[j] = em.heads[i].color.a;
                tempDuration[j] = em.heads[i].duration;
                tempStats0[j] = em.heads[i].stats[0];
                tempStats1[j] = em.heads[i].stats[1];
                tempStats2[j] = em.heads[i].stats[2];
                tempStats3[j] = em.heads[i].stats[3];
                tempStats4[j] = em.heads[i].stats[4];
                tempStats5[j] = em.heads[i].stats[5];
                tempOppStats0[j] = em.heads[i].oppStats[0];
                tempOppStats1[j] = em.heads[i].oppStats[1];
                tempOppStats2[j] = em.heads[i].oppStats[2];
                tempOppStats3[j] = em.heads[i].oppStats[3];
                tempOppStats4[j] = em.heads[i].oppStats[4];
                tempOppStats5[j] = em.heads[i].oppStats[5];
                j++;
            }
            j = em.footwear[0].id;
            for (int i = 0; i < em.footwear.Length; i++)
            {
                tempID[j] = em.footwear[i].id;
                tempCost[j] = em.footwear[i].cost;
                tempColorX[j] = em.footwear[i].color.r;
                tempColorY[j] = em.footwear[i].color.g;
                tempColorZ[j] = em.footwear[i].color.b;
                tempColorA[j] = em.footwear[i].color.a;
                tempDuration[j] = em.footwear[i].duration;
                tempStats0[j] = em.footwear[i].stats[0];
                tempStats1[j] = em.footwear[i].stats[1];
                tempStats2[j] = em.footwear[i].stats[2];
                tempStats3[j] = em.footwear[i].stats[3];
                tempStats4[j] = em.footwear[i].stats[4];
                tempStats5[j] = em.footwear[i].stats[5];
                tempOppStats0[j] = em.footwear[i].oppStats[0];
                tempOppStats1[j] = em.footwear[i].oppStats[1];
                tempOppStats2[j] = em.footwear[i].oppStats[2];
                tempOppStats3[j] = em.footwear[i].oppStats[3];
                tempOppStats4[j] = em.footwear[i].oppStats[4];
                tempOppStats5[j] = em.footwear[i].oppStats[5];
                j++;
            }
            j = em.apparel[0].id;
            for (int i = 0; i < em.apparel.Length; i++)
            {
                tempID[j] = em.apparel[i].id;
                tempCost[j] = em.apparel[i].cost;
                tempColorX[j] = em.apparel[i].color.r;
                tempColorY[j] = em.apparel[i].color.g;
                tempColorZ[j] = em.apparel[i].color.b;
                tempColorA[j] = em.apparel[i].color.a;
                tempDuration[j] = em.apparel[i].duration;
                tempStats0[j] = em.apparel[i].stats[0];
                tempStats1[j] = em.apparel[i].stats[1];
                tempStats2[j] = em.apparel[i].stats[2];
                tempStats3[j] = em.apparel[i].stats[3];
                tempStats4[j] = em.apparel[i].stats[4];
                tempStats5[j] = em.apparel[i].stats[5];
                tempOppStats0[j] = em.apparel[i].oppStats[0];
                tempOppStats1[j] = em.apparel[i].oppStats[1];
                tempOppStats2[j] = em.apparel[i].oppStats[2];
                tempOppStats3[j] = em.apparel[i].oppStats[3];
                tempOppStats4[j] = em.apparel[i].oppStats[4];
                tempOppStats5[j] = em.apparel[i].oppStats[5];
                j++;
            }

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

        }

        if (tSel)
        {
            Debug.Log("pUpM idList is " + pUpM.idPUList.Length + " long");
            cardPUIDList = pUpM.idPUList;
            cardSponsorIDList = pUpM.idSponsorList;
            activeCardIDList = pUpM.activeIdList;
            playedCardIDList = pUpM.playedIdList;
            activeCardLengthList = pUpM.activeLengthList;

            myFile.Add("Card PowerUp ID List", cardPUIDList);
            myFile.Add("Card Sponsor ID List", cardSponsorIDList);
            myFile.Add("Active Card ID List", activeCardIDList);
            myFile.Add("Played Card ID List", playedCardIDList);
            myFile.Add("Active Card Length List", activeCardLengthList);

            Debug.Log("CM Save Career activeCardIDList Length is " + activeCardIDList.Length);

            int[] provIDList = new int[tSel.provQual.Length];
            bool[] provCompleteList = new bool[tSel.provQual.Length];
            int[] tourIDList = new int[tSel.tour.Length];
            bool[] tourCompleteList = new bool[tSel.tour.Length];
            int[] tourniesIDList = new int[tSel.tournies.Length];
            bool[] tourniesCompleteList = new bool[tSel.tournies.Length];

            if (inventoryID.Length > 0)
            {
                myFile.Add("Inventory ID List", inventoryID);
                myFile.Add("Active Equip ID List", activeEquipID);
            }

            Debug.Log("Tournies Complete list is " + tourCompleteList.Length + " long");
            for (int i = 0; i < prov.Length; i++)
            {
                provIDList[i] = prov[i].id;
                provCompleteList[i] = prov[i].complete;
                //Debug.Log("provComplete " + i + " - " + provCompleteList[i]);
            }

            for (int i = 0; i < tour.Length; i++)
            {
                tourIDList[i] = tour[i].id;
                tourCompleteList[i] = tour[i].complete;
                //Debug.Log("tourComplete " + i + " - " + tourCompleteList[i]);
            }

            for (int i = 0; i < tournies.Length; i++)
            {
                tourniesIDList[i] = tournies[i].id;
                tourniesCompleteList[i] = tournies[i].complete;
                //Debug.Log("tourniesComplete " + i + " - " + tourniesCompleteList[i]);
            }

            myFile.Add("Tour Championship Complete", tSel.tourChampionship.complete);
            myFile.Add("Prov Championship Complete", tSel.provChampionship.complete);
            myFile.Add("Prov ID List", provIDList);
            myFile.Add("Prov Complete List", provCompleteList);
            myFile.Add("Tour ID List", tourIDList);
            myFile.Add("Tour Complete List", tourCompleteList);
            myFile.Add("Tournies ID List", tourniesIDList);
            myFile.Add("Tournies Complete List", tourniesCompleteList);
            myFile.Add("Number Of Teams", currentTourny.teams);

            //Debug.Log("Number of Teams in CM Save - " + currentTourny.teams);
        }

        if (tournies.Length > 0)
        {
            int[] provIDList = new int[prov.Length];
            bool[] provCompleteList = new bool[prov.Length];
            int[] tourIDList = new int[tour.Length];
            bool[] tourCompleteList = new bool[tour.Length];
            int[] tourniesIDList = new int[tournies.Length];
            bool[] tourniesCompleteList = new bool[tournies.Length];

            if (inventoryID.Length > 0)
            {
                myFile.Add("Inventory ID List", inventoryID);
                myFile.Add("Active Equip ID List", activeEquipID);
            }

            Debug.Log("Tournies Complete list is " + tourCompleteList.Length + " long");
            for (int i = 0; i < prov.Length; i++)
            {
                provIDList[i] = prov[i].id;
                provCompleteList[i] = prov[i].complete;
                //Debug.Log("provComplete " + i + " - " + provCompleteList[i]);
            }

            for (int i = 0; i < tour.Length; i++)
            {
                tourIDList[i] = tour[i].id;
                tourCompleteList[i] = tour[i].complete;
                //Debug.Log("tourComplete " + i + " - " + tourCompleteList[i]);
            }

            for (int i = 0; i < tournies.Length; i++)
            {
                tourniesIDList[i] = tournies[i].id;
                tourniesCompleteList[i] = tournies[i].complete;
                //Debug.Log("tourniesComplete " + i + " - " + tourniesCompleteList[i]);
            }

            myFile.Add("Tour Championship Complete", champ[0].complete);
            myFile.Add("Prov Championship Complete", champ[1].complete);
            myFile.Add("Prov ID List", provIDList);
            myFile.Add("Prov Complete List", provCompleteList);
            myFile.Add("Tour ID List", tourIDList);
            myFile.Add("Tour Complete List", tourCompleteList);
            myFile.Add("Tournies ID List", tourniesIDList);
            myFile.Add("Tournies Complete List", tourniesCompleteList);
            myFile.Add("Number Of Teams", currentTourny.teams);
        }

        if (tm)
        {
            //Debug.Log("Saving Career and TM active, tour is " + currentTourny.tour);
            //myFile.Add("Tourny In Progress", true);
            //myFile.Add("Tourny Record", gsp.record);
            myFile.Add("Tourny In Progress", gsp.tournyInProgress);
            myFile.Add("Draw", tm.draw);
            myFile.Add("Number Of Teams", tm.numberOfTeams);
            myFile.Add("Prize", tm.prize);
            myFile.Add("Rocks", gsp.rocks);
            myFile.Add("Ends", gsp.ends);
            //myFile.Add("Player Team", playerTeam);
            myFile.Add("OppTeam", tm.oppTeam);
            myFile.Add("Playoff Round", tm.playoffRound);

            string[] tournyNameList = new string[currentTournyTeams.Length];
            int[] tournyWinsList = new int[currentTournyTeams.Length];
            int[] tournyLossList = new int[currentTournyTeams.Length];
            int[] tournyRankList = new int[currentTournyTeams.Length];
            string[] tournyNextOppList = new string[currentTournyTeams.Length];
            int[] tournyStrengthList = new int[currentTournyTeams.Length];
            int[] tournyIDList = new int[currentTournyTeams.Length];
            float[] tournyEarningsList = new float[currentTournyTeams.Length];
            //int[] tournyTourTeamIDList = new int[teams.Length];
            //int[] tournyTourWinsList = new int[teams.Length];
            //int[] tournyTourLossList = new int[teams.Length];
            float[] tournyTourPointsList = new float[currentTournyTeams.Length];

            for (int i = 0; i < currentTournyTeams.Length; i++)
            {
                tournyNameList[i] = currentTournyTeams[i].name;
                tournyWinsList[i] = currentTournyTeams[i].wins;
                tournyLossList[i] = currentTournyTeams[i].loss;
                tournyRankList[i] = currentTournyTeams[i].rank;
                tournyNextOppList[i] = currentTournyTeams[i].nextOpp;
                tournyStrengthList[i] = currentTournyTeams[i].strength;
                tournyIDList[i] = currentTournyTeams[i].id;
                tournyEarningsList[i] = currentTournyTeams[i].earnings;
                tournyTourPointsList[i] = currentTournyTeams[i].tourPoints;
                //Debug.Log("Tourny Id List - " + idList[i]);
            }

            myFile.Add("Tourny Name List", tournyNameList);
            myFile.Add("Tourny Wins List", tournyWinsList);
            myFile.Add("Tourny Loss List", tournyLossList);
            myFile.Add("Tourny Rank List", tournyRankList);
            myFile.Add("Tourny NextOpp List", tournyNextOppList);
            myFile.Add("Tourny Strength List", tournyStrengthList);
            myFile.Add("Tourny Team ID List", tournyIDList);
            myFile.Add("Tourny Earnings List", tournyEarningsList);

        }

        if (gm)
        {
            myFile.Add("Tourny Game", gsp.tourny);
            myFile.Add("Game Ends", gm.endTotal);
            myFile.Add("Game Active End", gm.endCurrent);
            myFile.Add("Game Rocks", gm.rocksPerTeam);
            myFile.Add("Game Rock Current", gm.rockCurrent);
            myFile.Add("Game Red Hammer", gm.redHammer);
            myFile.Add("Game AI Yellow", gm.aiTeamYellow);
            myFile.Add("Game AI Red", gm.aiTeamRed);
            myFile.Add("Game Yellow Score", gm.yellowScore);
            myFile.Add("Game Red Score", gm.redScore);
            myFile.Add("Game Yellow Team Name", gm.yellowTeamName);
            myFile.Add("Game Red Team Name", gm.redTeamName);
            //myFile.Add("Game Yellow Team Colour")
        }

        int[] tempTRX = new int[teamRecords.Length];
        int[] tempTRY = new int[teamRecords.Length];
        float[] tempTRZ = new float[teamRecords.Length];
        int[] tempTRW = new int[teamRecords.Length];

        int[] tempTourTRX = new int[tourRecords.Length];
        int[] tempTourTRY = new int[tourRecords.Length];
        float[] tempTourTRZ = new float[tourRecords.Length];
        int[] tempTourTRW = new int[tourRecords.Length];

        for (int i = 0; i < teamRecords.Length; i++)
        {
            tempTRX[i] = (int)teamRecords[i].x;
            tempTRY[i] = (int)teamRecords[i].y;
            tempTRZ[i] = teamRecords[i].z;
            tempTRW[i] = (int)teamRecords[i].w;
        }

        for (int i = 0; i < tourRecords.Length; i++)
        {
            tempTourTRX[i] = (int)tourRecords[i].x;
            tempTourTRY[i] = (int)tourRecords[i].y;
            tempTourTRZ[i] = tourRecords[i].z;
            tempTourTRW[i] = (int)tourRecords[i].w;
        }

        myFile.Add("Team Records X", tempTRX);
        myFile.Add("Team Records Y", tempTRY);
        myFile.Add("Team Records Z", tempTRZ);
        myFile.Add("Team Records W", tempTRW);

        myFile.Add("Tour Records X", tempTourTRX);
        myFile.Add("Tour Records Y", tempTourTRY);
        myFile.Add("Tour Records Z", tempTourTRZ);
        myFile.Add("Tour Records W", tempTourTRW);

        if (gsp)
        {

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
        myFile.Append();
        StartCoroutine(SaveHighScore());
        //activePlayers = teamSel.activePlayers;

        //myFile.Add("Tourny Team ID List", tournyTeamIDList);
        //myFile.Add("Tourny Wins List", tournyWinsList);
        //myFile.Add("Tourny Loss List", tournyLossList);

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
        record = gsp.record;
        earnings += gsp.earnings;


        float xpChange = 0f;
        if (!gsp.cashGame)
        {
            currentTournyTeams = gsp.teams;

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
            //for (int i = 0; i < tourTeams.Length; i++)
            //{
            //    //Debug.Log(tourTeams[i].name + " - " + tourTeams[i].tourPoints + " before TourRecordVector4");
            //    for (int j = 0; j < tourRecords.Length; j++)
            //    {
            //        if (tourTeams[i].id == tourRecords[j].w)
            //        {
            //            tourTeams[i].tourRecord.x += tourRecords[j].x;
            //            tourTeams[i].tourRecord.y += tourRecords[j].y;
            //            tourTeams[i].tourPoints += tourRecords[j].z;
            //        }
            //    }
            //    //Debug.Log(tourTeams[i].name + " - " + tourTeams[i].tourPoints + " AFTER TourRecordVector4");
            //}

            Debug.Log("Team Record - " + teams[0].name + " - " + teamRecords[0]);
        }

        //Debug.Log("Current Team List count is " + currentTournyTeams.Length);

        //Debug.Log("Rank List count is " + provRankList.Count);
        //Debug.Log("First Prov Team is " + provRankList[0].team.name);

        //for (int i = 0; i < teams.Length; i++)
        //{
        //    for (int j = 0; j < teamRecords.Length; j++)
        //    {
        //        for (int k = 0; k < currentTournyTeams.Length; k++)
        //        {
        //            if (currentTournyTeams[k].id == teams[i].id)
        //            {
        //                if (teams[i].id == teamRecords[j].w)
        //                {
        //                    teams[i].wins += (int)teamRecords[j].x;
        //                    teams[i].loss += (int)teamRecords[j].y;
        //                    teams[i].earnings += teamRecords[j].z;
        //                }
        //            }
        //        }
        //    }
        //}

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

        //if (allTimeTrophyList.Count <= 0)
        //    allTimeTrophyList = new List<bool>();

        Debug.Log("allTimeTrophyList2 Count - " + currentTrophyList.Count);
        xp += xpChange;
        totalXp += xpChange;
        //Debug.Log("XP Change is " + xpChange);
        //Debug.Log("Rank List count is " + provRankList.Count);
        provRankList.Sort();
        //Debug.Log("Top Ranked Team is " + provRankList[0].team.name);
        //Debug.Log("Second Place Team is " + provRankList[1].team.name);
        //Debug.Log("Third Place Team is " + provRankList[2].team.name);
        ////record += new Vector2(gsp.playerTeam.wins, gsp.playerTeam.loss);
        //Debug.Log("Record is " + record.x + " - " + record.y);
        week++;
        Debug.Log("CM Tourny Results week is " + week);
        SaveCareer();
    }

    public void PlayTourny()
    {
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        Debug.Log("CM Play Tourny");
        inProgress = true;

        gsp.cStats = cStats;
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

        TournyTeamList tTeamList = FindObjectOfType<TournyTeamList>();
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        season++;

        Shuffle(tTeamList.teams);

        teams = new Team[totalTeams];
        //tourRankList = new List<Team_List>();
        //provQualList = new List<Team_List>();

        for (int i = 0; i < totalTeams; i++)
        {
            teams[i] = tTeamList.teams[i];
        }
        teams[0].name = teamName;
        teams[0].player = true;
        playerTeamIndex = teams[0].id;
        gsp.playerTeamIndex = playerTeamIndex;
        week++;
    }

    public void NewSeason()
    {
        TournyTeamList tTeamList = FindObjectOfType<TournyTeamList>();
        GameSettingsPersist gsp = FindObjectOfType<GameSettingsPersist>();
        TournySelector tSel = FindObjectOfType<TournySelector>();

        provRankList = new List<Standings_List>();
        tourRankList = new List<TourStandings_List>();

        xp = 0f;
        totalXp = 0f;

        record = new Vector2(0f, 0f);
        tourRecord = new Vector2(0f, 0f);

        cStats.drawAccuracy = 3;
        cStats.guardAccuracy = 3;
        cStats.takeOutAccuracy = 3;
        cStats.sweepStrength = 3;
        cStats.sweepEndurance = 3;
        cStats.sweepCohesion = 3;

        //for (int i = 0; i < activePlayers.Length; i++)
        //{
        //    modStats.drawAccuracy += activePlayers[i].draw;
        //    modStats.takeOutAccuracy += activePlayers[i].takeOut;
        //    modStats.guardAccuracy += activePlayers[i].guard;
        //    modStats.sweepStrength += activePlayers[i].sweepStrength;
        //    modStats.sweepEndurance += activePlayers[i].sweepEnduro;
        //    modStats.sweepCohesion += activePlayers[i].sweepCohesion;
        //}

        season++;

        Shuffle(tTeamList.teams);

        teams = new Team[totalTeams];
        tourTeams = new Team[totalTourTeams];
        Debug.Log("provRankList Count is " + provRankList.Count);
        for (int i = 0; i < totalTeams; i++)
        {
            teams[i] = tTeamList.teams[i];
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

        //introDialogue[0] = true;
        activeCardIDList = null;
        cardPUIDList = null;
        cardSponsorIDList = null;
        
        week++;

        //tSel.Expand(tSel.menuButtons[2]);
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
            int rnd = Random.Range(0, i);

            // Save the value of the current i, otherwise it'll overright when we swap the values
            Team temp = a[i];

            // Swap the new and old values
            a[i] = a[rnd];
            a[rnd] = temp;
        }
    }
}
