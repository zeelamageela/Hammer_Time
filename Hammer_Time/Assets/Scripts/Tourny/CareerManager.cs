using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TigerForge;

public class CareerManager : MonoBehaviour
{
    public static CareerManager instance;
    GameSettingsPersist gsp;
    TournyManager tm;
    PlayoffManager pm;
    TournySettings ts;
    TournySelector tSel;
    EasyFileSave myFile;
    TournyTeamList tTeamList;
    CareerSettings cs;
    PowerUpManager pUpM;
    TeamMenu teamSel;

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

    public Player[] activePlayers;

    public float xp;
    public float totalXp;

    public CareerStats cStats;
    public CareerStats oppStats;
    public CareerStats modStats;

    public int skillPoints;

    public int[] cardIDList;
    public int[] activeCardIDList;
    public int[] usedCardIDList;
    public int[] activeCardLengthList;

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
        tm = FindObjectOfType<TournyManager>();
        pm = FindObjectOfType<PlayoffManager>();
        tourRankList = new List<TourStandings_List>();
        provRankList = new List<Standings_List>();
        tournyResults = new List<int>();
        //if (inProgress)
        //{
        //    LoadCareer();
        //}
        //else
        //    NewSeason();
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

    public void LoadFromGSP()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();

        earnings = gsp.earnings;
        record = gsp.record;
        SaveCareer();
        Debug.Log("Earnings - CM from GSP - " + earnings);
    }

    public void LoadCareer()
    {
        Debug.Log("Loading in CM");

        myFile = new EasyFileSave("my_player_data");

        gsp = FindObjectOfType<GameSettingsPersist>();
        tSel = FindObjectOfType<TournySelector>();
        tTeamList = FindObjectOfType<TournyTeamList>();
        teams = new Team[totalTeams];
        tourTeams = new Team[totalTourTeams];
        pUpM = FindObjectOfType<PowerUpManager>();
        teamSel = FindObjectOfType<TeamMenu>();
        //teamRecords = new Vector3[totalTeams];

        if (provRankList != null)
        {
            provRankList.Clear();
        }

        if (tourRankList != null)
        {
            tourRankList.Clear();
        }

        if (myFile.Load())
        {
            coachDialogue = myFile.GetArray<bool>("Coach Dialogue Played List");
            qualDialogue = myFile.GetArray<bool>("Qualifying Dialogue Played List");
            reviewDialogue = myFile.GetArray<bool>("Review Dialogue Played List");
            introDialogue = myFile.GetArray<bool>("Intro Dialogue Played List");
            helpDialogue = myFile.GetArray<bool>("Help Dialogue Played List");
            strategyDialogue = myFile.GetArray<bool>("Strategy Dialogue Played List");
            storyDialogue = myFile.GetArray<bool>("Story Dialogue Played List");

            week = myFile.GetInt("Week");
            season = myFile.GetInt("Season");
            playerName = myFile.GetString("Player Name");
            teamName = myFile.GetString("Team Name");
            teamColour = myFile.GetUnityColor("Team Colour");
            playerTeamIndex = myFile.GetInt("Player Team Index");
            record = myFile.GetUnityVector2("Career Record");
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
                Debug.Log("Players Id Length - " + playersIdList.Length);


                for (int i = 0; i < playersIdList.Length; i++)
                {
                    Debug.Log("CM LOADCAREER - Active Players Id - " + i + " - " + playersIdList[i]);
                    teamSel.activePlayers[i].id = playersIdList[i];
                }
            }

            cardIDList = myFile.GetArray<int>("Card ID List");
            activeCardIDList = myFile.GetArray<int>("Active Card ID List");
            usedCardIDList = myFile.GetArray<int>("Used Card ID List");
            activeCardLengthList = myFile.GetArray<int>("Active Card Length List");
            Debug.Log("cardIdList Length - " + cardIDList.Length);



            if (tSel)
            {
                int[] provIDList = myFile.GetArray<int>("Prov ID List");
                bool[] provCompleteList = myFile.GetArray<bool>("Prov Complete List");
                int[] tourIDList = myFile.GetArray<int>("Tour ID List");
                bool[] tourCompleteList = myFile.GetArray<bool>("Tour Complete List");
                int[] tourniesIDList = myFile.GetArray<int>("Tournies ID List");
                bool[] tourniesCompleteList = myFile.GetArray<bool>("Tournies Complete List");
                int[] activeIDList = myFile.GetArray<int>("Active ID List");

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
                    Debug.Log("prov tourny " + i + " is " + prov[i].complete);
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
                    Debug.Log("Tourny " + tournies[i].id + " complete is " + tournies[i].complete);
                }

                //for (int i = 0; i < activeTournies.Length; i++)
                //{
                //    Debug.Log("Active Tournies i is " + i);
                //    activeTournies[i].id = activeIDList[i];

                //    for (int j = 0; j < tSel.activeTournies.Length; j++)
                //    {
                //        if (activeTournies[i].id == tSel.activeTournies[j].id)
                //            activeTournies[i] = tSel.activeTournies[j];
                //    }
                //}

                tSel.provQual = prov;
                tSel.tour = tour;
                tSel.tournies = tournies;
                tSel.tourChampionship = champ[0];
                tSel.provChampionship = champ[1];
            }

            int[] idList = myFile.GetArray<int>("Total ID List");
            int[] winsList = myFile.GetArray<int>("Total Wins List");
            int[] lossList = myFile.GetArray<int>("Total Loss List");
            float[] earningsList = myFile.GetArray<float>("Total Earnings List");

            Debug.Log("Total ID List Length is " + idList.Length);
            Debug.Log("Total Teams List Length is " + teams.Length);

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

            int[] tourTeamsIDList = myFile.GetArray<int>("Tour Team ID List");
            int[] tourWinsList = myFile.GetArray<int>("Tour Wins List");
            int[] tourLossList = myFile.GetArray<int>("Tour Loss List");
            float[] tourPointsList = myFile.GetArray<float>("Tour Points List");

            Debug.Log("Tour Record List Length is " + tourWinsList.Length + " " + tourLossList.Length);
            Debug.Log("Total Teams List Length is " + tourTeamsIDList.Length);

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

                for (int i = 0; i < tourTeams.Length; i++)
                {
                    for (int j = 0; j < teams.Length; j++)
                    {
                        if (tourTeamsIDList[i] == teams[j].id)
                            tourTeams[i] = teams[j];
                    }
                }
            }


            gsp.inProgress = myFile.GetBool("Tourny In Progress");

            if (gsp.inProgress)
            {
                currentTourny.name = myFile.GetString("Current Tourny Name");
                currentTourny.id = myFile.GetInt("Current Tourny ID");
                currentTourny.tour = myFile.GetBool("Current Tourny Tour");
                currentTourny.qualifier = myFile.GetBool("Current Tourny Qualifier");
                currentTourny.championship = myFile.GetBool("Current Tourny Championship");
                currentTourny.prizeMoney = myFile.GetInt("Prize Money");
                currentTourny.BG = myFile.GetInt("Current Tourny BG");
                currentTourny.crowdDensity = myFile.GetInt("Current Tourny Crowd Density");

                int[] tournyIDList = myFile.GetArray<int>("Tourny Team ID List");
                int[] tournyWinsList = myFile.GetArray<int>("Tourny Wins List");
                int[] tournyLossList = myFile.GetArray<int>("Tourny Loss List");
                float[] tournyEarningsList = myFile.GetArray<float>("Tourny Earnings List");
                bool[] tournyPlayerList = myFile.GetArray<bool>("Tourny Player List");

                for (int i = 0; i < currentTournyTeams.Length; i++)
                {
                    currentTournyTeams[i].id = tournyIDList[i];
                    currentTournyTeams[i].player = tournyPlayerList[i];

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

                    currentTournyTeams[i].wins = tournyWinsList[i];
                    currentTournyTeams[i].loss = tournyLossList[i];
                    currentTournyTeams[i].earnings = tournyEarningsList[i];

                    if (currentTournyTeams[i].player)
                        Debug.Log("Player Team is " + currentTournyTeams[i].name);
                }
            }

            if (provRankList != null)
            {
                for (int i = 0; i < teams.Length; i++)
                {
                    provRankList.Add(new Standings_List(teams[i]));
                }

                for (int i = 0; i < tourTeams.Length; i++)
                {
                    tourRankList.Add(new TourStandings_List(tourTeams[i]));
                }
            }

            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i].player)
                {
                    playerTeam = teams[i];
                    Debug.Log("CM playerTeamIndex is " + teams[i].id);
                }
            }

            myFile.Dispose();
        }
    }

    IEnumerator SaveHighScore()
    {
        myFile = new EasyFileSave("my_hiscore_data");
        allTimeList = new List<Standings_List>();

        if (myFile.Load())
        {
            float[] allTimeEarnings = myFile.GetArray<float>("All Time Earnings");
            string[] allTimeName = myFile.GetArray<string>("All Time Names");

            for (int i = 0; i < allTimeEarnings.Length; i++)
            {
                Team tempTeam = gsp.playerTeam;
                tempTeam.wins = 0;
                tempTeam.loss = 0;
                tempTeam.earnings = 0;
                tempTeam.name = "";

                tempTeam.name = allTimeName[i];
                //tempTeam.wins = Mathf.RoundToInt(allTimeEarnings[i]);
                tempTeam.earnings = allTimeEarnings[i];
                allTimeList.Add(new Standings_List(tempTeam));
            }

            myFile.Dispose();
        }

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < teams.Length; i++)
        {
            if (teams[i].id == playerTeamIndex)
            {
                allTimeList.Add(new Standings_List(teams[i]));
            }
        }

        Debug.Log("All Time List - " + allTimeList.Count);
        allTimeList.Sort();

        float[] allTimeEarningsTemp = new float[allTimeList.Count];
        string[] allTimeNameTemp = new string[allTimeList.Count];

        for (int i = 0; i < allTimeList.Count; i++)
        {
            allTimeEarningsTemp[i] = allTimeList[i].team.earnings;
            allTimeNameTemp[i] = allTimeList[i].team.name;
        }

        Debug.Log("All Time List Length - " + allTimeList.Count);

        //myFile = new EasyFileSave("my_hiscore_data");
        if (myFile.Load())
        {
            myFile.Add("All Time Earnings", allTimeEarningsTemp);
            myFile.Add("All Time Names", allTimeNameTemp);

            myFile.Append();
        }
    }

    public void SaveCareer()
    {
        tSel = FindObjectOfType<TournySelector>();
        tm = FindObjectOfType<TournyManager>();
        gsp = FindObjectOfType<GameSettingsPersist>();
        pUpM = FindObjectOfType<PowerUpManager>();
        teamSel = FindObjectOfType<TeamMenu>();

        Debug.Log("Saving Career - " + gsp.inProgress);
        myFile = new EasyFileSave("my_player_data");

        myFile.Add("Team Paid", teamPaid);
        myFile.Add("Coach Dialogue Played List", coachDialogue);
        myFile.Add("Qualifying Dialogue Played List", qualDialogue);
        myFile.Add("Review Dialogue Played List", reviewDialogue);
        myFile.Add("Intro Dialogue Played List", introDialogue);
        myFile.Add("Help Dialogue Played List", helpDialogue);
        myFile.Add("Strategy Dialogue Played List", strategyDialogue);
        myFile.Add("Story Dialogue Played List", storyDialogue);

        myFile.Add("Tourny In Progress", gsp.inProgress);
        myFile.Add("Knockout Tourny", false);
        myFile.Add("Player Name", playerName);
        myFile.Add("Team Name", teamName);
        myFile.Add("Team Colour", teamColour);
        myFile.Add("Player Team Index", playerTeamIndex);
        myFile.Add("Week", week);
        myFile.Add("Season", season);
        //myFile.Add("Career Record", record);
        myFile.Add("Career Earnings", earnings);
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

        int[] tourTeamIDList = new int[tourTeams.Length];
        int[] tourWinsList = new int[tourTeams.Length];
        int[] tourLossList = new int[tourTeams.Length];
        float[] tourPointsList = new float[tourTeams.Length];

        for (int i = 0; i < teams.Length; i++)
        {
            idList[i] = teams[i].id;
            //Debug.Log("Id List - " + idList[i]);
            winsList[i] = teams[i].wins;
            lossList[i] = teams[i].loss;
            earningsList[i] = teams[i].earnings;

            if (playerTeamIndex == teams[i].id)
            {
                earningsList[i] = earnings;
                teams[i].earnings = earnings;
                Debug.Log("Earnings - CM - " + earnings);
            }

        }

        Debug.Log("Total Id List length is " + idList.Length);
        myFile.Add("Total ID List", idList);
        myFile.Add("Total Wins List", winsList);
        myFile.Add("Total Loss List", lossList);
        myFile.Add("Total Earnings List", earningsList);

        if (pUpM != null)
        {
            Debug.Log("puUpM idList is " + pUpM.idList.Length + " long");
            cardIDList = pUpM.idList;
            activeCardIDList = pUpM.activeIdList;
            usedCardIDList = pUpM.usedIdList;
            activeCardLengthList = pUpM.activeLengthList;
            myFile.Add("Card ID List", cardIDList);
            myFile.Add("Active Card ID List", activeCardIDList);
            myFile.Add("Used Card ID List", usedCardIDList);
            myFile.Add("Active Card Length List", activeCardLengthList);
        }

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
            Debug.Log("CM SAVECAREER Active Player List " + i + " - " + playerIdList[i]);
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
        if (!pUpM)
        {
            for (int i = 0; i < teams.Length; i++)
            {
                idList[i] = teams[i].id;
                //Debug.Log("Id List - " + idList[i]);
                winsList[i] = teams[i].wins;
                lossList[i] = teams[i].loss;
                earningsList[i] = teams[i].earnings;

                //if (playerTeamIndex == teams[i].id)
                //{
                //    earningsList[i] = earnings;
                //    teams[i].earnings = earnings;
                //}

            }
            //Debug.Log("Total Id List length is " + idList.Length);
            //myFile.Add("Total ID List", idList);
            //myFile.Add("Total Wins List", winsList);
            //myFile.Add("Total Loss List", lossList);
            //myFile.Add("Total Earnings List", earningsList);

            for (int i = 0; i < tourTeams.Length; i++)
            {
                if (playerTeamIndex == tourTeams[i].id)
                {
                    Debug.Log("Before Save tourTeams record is " + tourTeams[i].tourRecord.x + " - " + tourTeams[i].tourRecord.y);
                }
            }
            for (int i = 0; i < tourTeams.Length; i++)
            {
                tourTeamIDList[i] = tourTeams[i].id;
                //Debug.Log("Id List - " + idList[i]);
                tourWinsList[i] = (int)tourTeams[i].tourRecord.x;
                tourLossList[i] = (int)tourTeams[i].tourRecord.y;
                tourPointsList[i] = tourTeams[i].tourPoints;
                Debug.Log("Tour Points Length - " + tourPointsList.Length);
                Debug.Log("Tour Points - " + i + " - " + tourPointsList[i]);
            }
            Debug.Log("Tour Record length is " + tourWinsList.Length + " - " + tourLossList.Length);
            myFile.Add("Tour Team ID List", tourTeamIDList);
            myFile.Add("Tour Wins List", tourWinsList);
            myFile.Add("Tour Loss List", tourLossList);
            myFile.Add("Tour Points List", tourPointsList);

            myFile.Add("Current Tourny Name", currentTourny.name);
            myFile.Add("Current Tourny ID", currentTourny.id);
            myFile.Add("Current Tourny Tour", currentTourny.tour);
            myFile.Add("Current Tourny Qualifier", currentTourny.qualifier);
            myFile.Add("Current Tourny Championship", currentTourny.championship);
            myFile.Add("Prize Money", currentTourny.prizeMoney);
            myFile.Add("Current Tourny BG", currentTourny.BG);
            myFile.Add("Current Tourny Crowd Density", currentTourny.crowdDensity);


        }


        if (tSel)
        {
            int[] provIDList = new int[tSel.provQual.Length];
            bool[] provCompleteList = new bool[tSel.provQual.Length];
            int[] tourIDList = new int[tSel.tour.Length];
            bool[] tourCompleteList = new bool[tSel.tour.Length];
            int[] tourniesIDList = new int[tSel.tournies.Length];
            bool[] tourniesCompleteList = new bool[tSel.tournies.Length];

            Debug.Log("Tournies Complete list is " + tourCompleteList.Length + " long");
            for (int i = 0; i < prov.Length; i++)
            {
                provIDList[i] = prov[i].id;
                provCompleteList[i] = prov[i].complete;
                Debug.Log("provComplete " + i + " - " + provCompleteList[i]);
            }

            for (int i = 0; i < tour.Length; i++)
            {
                tourIDList[i] = tour[i].id;
                tourCompleteList[i] = tour[i].complete;
                Debug.Log("tourComplete " + i + " - " + tourCompleteList[i]);
            }

            for (int i = 0; i < tournies.Length; i++)
            {
                tourniesIDList[i] = tournies[i].id;
                tourniesCompleteList[i] = tournies[i].complete;
                Debug.Log("tourniesComplete " + i + " - " + tourniesCompleteList[i]);
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

            Debug.Log("Number of Teams in CM Save - " + currentTourny.teams);
        }

        if (tm)
        {
            Debug.Log("Saving Career and TM active, tour is " + currentTourny.tour);
            //myFile.Add("Tourny In Progress", true);
            myFile.Add("Career Record", gsp.record);
            myFile.Add("In Progress", true);
            myFile.Add("Draw", tm.draw);
            myFile.Add("Number Of Teams", tm.numberOfTeams);
            myFile.Add("Prize", tm.prize);
            myFile.Add("Rocks", gsp.rocks);
            myFile.Add("Ends", gsp.ends);
            //myFile.Add("Player Team", playerTeam);
            myFile.Add("OppTeam", tm.oppTeam);
            myFile.Add("Playoff Round", tm.playoffRound);

            string[] tournyNameList = new string[teams.Length];
            int[] tournyWinsList = new int[teams.Length];
            int[] tournyLossList = new int[teams.Length];
            int[] tournyRankList = new int[teams.Length];
            string[] tournyNextOppList = new string[teams.Length];
            int[] tournyStrengthList = new int[teams.Length];
            int[] tournyIDList = new int[teams.Length];
            float[] tournyEarningsList = new float[teams.Length];
            int[] tournyTourTeamIDList = new int[teams.Length];
            int[] tournyTourWinsList = new int[teams.Length];
            int[] tournyTourLossList = new int[teams.Length];
            float[] tournyTourPointsList = new float[teams.Length];

            for (int i = 0; i < teams.Length; i++)
            {
                tournyNameList[i] = teams[i].name;
                tournyWinsList[i] = teams[i].wins;
                tournyLossList[i] = teams[i].loss;
                tournyRankList[i] = teams[i].rank;
                tournyNextOppList[i] = teams[i].nextOpp;
                tournyStrengthList[i] = teams[i].strength;
                tournyIDList[i] = teams[i].id;
                tournyEarningsList[i] = teams[i].earnings;
                tournyTourPointsList[i] = teams[i].tourPoints;
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


        //activePlayers = teamSel.activePlayers;
        
        //myFile.Add("Tourny Team ID List", tournyTeamIDList);
        //myFile.Add("Tourny Wins List", tournyWinsList);
        //myFile.Add("Tourny Loss List", tournyLossList);

        myFile.Append();
        StartCoroutine(SaveHighScore());
    }

    public void SetupTourny()
    {
        tSel = FindObjectOfType<TournySelector>();
        gsp = FindObjectOfType<GameSettingsPersist>();
        currentTourny = tSel.currentTourny;
        Shuffle(teams);
        currentTournyTeams = new Team[currentTourny.teams];

        bool inList = false;
        if (currentTourny.tour)
        {
            gsp.KO = true;
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
            gsp.KO = false;
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
    }

    public void TournyResults()
    {
        Debug.Log("Tourny Results in CM");
        gsp = FindObjectOfType<GameSettingsPersist>();
        //TournyManager tm = FindObjectOfType<TournyManager>();
        record = gsp.record;
        //earnings = gsp.earnings;
        currentTournyTeams = gsp.teams;

        float xpChange = 0f;
        for (int i = 0; i < currentTournyTeams.Length; i++)
        {
            if (playerTeamIndex == currentTournyTeams[i].id)
            {
                tournyResults.Add(currentTournyTeams[i].rank);
                xpChange = currentTournyTeams.Length - currentTournyTeams[i].rank;
                xpChange += currentTournyTeams[i].wins * 3f;
                xpChange += currentTournyTeams[i].loss;

                if (currentTournyTeams[i].rank < 5)
                {
                    xpChange += 5f;
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
                switch(currentTournyTeams[i].rank)
                {
                    case 1:
                        currentTournyTeams[i].tourPoints = 25f;
                        break;
                    case 2:
                        currentTournyTeams[i].tourPoints = 18f;
                        break;
                    case 3:
                        currentTournyTeams[i].tourPoints = 15f;
                        break;
                    case 4:
                        currentTournyTeams[i].tourPoints = 12f;
                        break;
                    case 5:
                        currentTournyTeams[i].tourPoints = 10f;
                        break;
                    case 6:
                        currentTournyTeams[i].tourPoints = 8f;
                        break;
                    case 7:
                        currentTournyTeams[i].tourPoints = 6f;
                        break;
                    case 9:
                        currentTournyTeams[i].tourPoints = 4f;
                        break;
                    case 11:
                        currentTournyTeams[i].tourPoints = 2f;
                        break;
                    case 13:
                        currentTournyTeams[i].tourPoints = 1f;
                        break;
                }

                for (int j = 0; j < tourTeams.Length; j++)
                {
                    if (currentTournyTeams[i].id == tourTeams[j].id)
                    {
                        tourTeams[j].tourRecord.x = currentTournyTeams[i].wins;
                        tourTeams[j].tourRecord.y = currentTournyTeams[i].loss;
                        tourTeams[j].tourPoints = currentTournyTeams[i].tourPoints;
                    }
                }
                for (int j = 0; j < teams.Length; j++)
                {
                    if (currentTournyTeams[i].id == teams[j].id)
                    {
                        teams[i].wins = currentTournyTeams[i].wins;
                        teams[i].loss = currentTournyTeams[i].loss;
                        teams[i].earnings = currentTournyTeams[i].earnings;
                    }
                }
            }

            for (int i = 0; i < totalTourTeams; i++)
            {
                if (playerTeamIndex == currentTournyTeams[i].id)
                {
                    tourRecord.x += currentTournyTeams[i].wins;
                    tourRecord.y += currentTournyTeams[i].loss;
                }
                //if (playerTeamIndex == currentTournyTeams[i].id)
            }

        }

        for (int i = 0; i < tourTeams.Length; i++)
        {
            Debug.Log(tourTeams[i].name + " - " + tourTeams[i].tourPoints + " before TourRecordVector4");
            for (int j = 0; j < tourRecords.Length; j++)
            {
                for (int k = 0; k < currentTournyTeams.Length; k++)
                {
                    if (currentTournyTeams[k].id == tourTeams[i].id)
                    {
                        if (tourTeams[i].id == tourRecords[j].w)
                        {
                            tourTeams[i].tourRecord.x += tourRecords[j].x;
                            tourTeams[i].tourRecord.y += tourRecords[j].y;
                            tourTeams[i].tourPoints += tourRecords[j].z;
                        }
                    }
                }
            }
            Debug.Log(tourTeams[i].name + " - " + tourTeams[i].tourPoints + " AFTER TourRecordVector4");
        }
        Debug.Log("Current Team List count is " + currentTournyTeams.Length);

        Debug.Log("Rank List count is " + provRankList.Count);
        Debug.Log("First Prov Team is " + provRankList[0].team.name);

        for (int i = 0; i < teams.Length; i++)
        {
            for (int j = 0; j < teamRecords.Length; j++)
            {
                for (int k = 0; k < currentTournyTeams.Length; k++)
                {
                    if (currentTournyTeams[k].id == teams[i].id)
                    {
                        if (teams[i].id == teamRecords[j].w)
                        {
                            teams[i].wins += (int)teamRecords[j].x;
                            teams[i].loss += (int)teamRecords[j].y;
                            teams[i].earnings += teamRecords[j].z;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < teams.Length; i++)
        {
            if (teams[i].id == playerTeamIndex)
            {
                playerTeam = teams[i];
            }
        }
        xp += xpChange;
        totalXp += xpChange;
        Debug.Log("XP Change is " + xpChange);
        Debug.Log("Rank List count is " + provRankList.Count);
        provRankList.Sort();
        Debug.Log("Top Ranked Team is " + provRankList[0].team.name);
        Debug.Log("Second Place Team is " + provRankList[1].team.name);
        Debug.Log("Third Place Team is " + provRankList[2].team.name);
        //record += new Vector2(gsp.playerTeam.wins, gsp.playerTeam.loss);
        Debug.Log("Record is " + record.x + " - " + record.y);
        week++;
        SaveCareer();
    }

    public void PlayTourny()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
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
        
        //earnings = gsp.earnings;
        SaveCareer();
    }

    public void NextWeek()
    {
        week++;
        teamPaid = false;
        tSel.SetActiveTournies();
    }

    public void ContinueSeason()
    {
        tTeamList = FindObjectOfType<TournyTeamList>();
        gsp = FindObjectOfType<GameSettingsPersist>();
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
        tTeamList = FindObjectOfType<TournyTeamList>();
        gsp = FindObjectOfType<GameSettingsPersist>();
        tSel = FindObjectOfType<TournySelector>();
        teamSel = FindObjectOfType<TeamMenu>();

        provRankList = new List<Standings_List>();
        tourRankList = new List<TourStandings_List>();
        coachDialogue = new bool[tSel.coachGreen.dialogue.Length];
        qualDialogue = new bool[tSel.coachGreen.qualDialogue.Length];
        reviewDialogue = new bool[tSel.coachGreen.reviewDialogue.Length];
        introDialogue = new bool[tSel.coachGreen.introDialogue.Length];
        storyDialogue = new bool[tSel.coachGreen.storyDialogue.Length];
        helpDialogue = new bool[tSel.coachGreen.helpDialogue.Length];
        strategyDialogue = new bool[tSel.coachGreen.strategyDialogue.Length];

        for (int i = 0; i < coachDialogue.Length; i++)
            coachDialogue[i] = false;
        for (int i = 0; i < qualDialogue.Length; i++)
            qualDialogue[i] = false;
        for (int i = 0; i < reviewDialogue.Length; i++)
            reviewDialogue[i] = false;
        for (int i = 0; i < introDialogue.Length; i++)
            introDialogue[i] = false;
        for (int i = 0; i < storyDialogue.Length; i++)
            storyDialogue[i] = false;
        for (int i = 0; i < helpDialogue.Length; i++)
            helpDialogue[i] = false;
        for (int i = 0; i < strategyDialogue.Length; i++)
            strategyDialogue[i] = false;

        coachDialogue[0] = true;
        introDialogue[0] = true;

        xp = 0f;
        totalXp = 0f;

        cStats.drawAccuracy = 3;
        cStats.guardAccuracy = 3;
        cStats.takeOutAccuracy = 3;
        cStats.sweepStrength = 3;
        cStats.sweepEndurance = 3;
        cStats.sweepCohesion = 3;

        for (int i = 0; i < activePlayers.Length; i++)
        {
            modStats.drawAccuracy += activePlayers[i].draw;
            modStats.takeOutAccuracy += activePlayers[i].takeOut;
            modStats.guardAccuracy += activePlayers[i].guard;
            modStats.sweepStrength += activePlayers[i].sweepStrength;
            modStats.sweepEndurance += activePlayers[i].sweepEnduro;
            modStats.sweepCohesion += activePlayers[i].sweepCohesion;
        }

        season++;

        Shuffle(tTeamList.teams);

        teams = new Team[totalTeams];
        tourTeams = new Team[totalTourTeams];
        
        for (int i = 0; i < totalTeams; i++)
        {
            teams[i] = tTeamList.teams[i];
            provRankList.Add(new Standings_List(teams[i]));
        }


        teams[0].name = teamName;
        teams[0].player = true;
        teams[0].earnings = earnings;
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

        introDialogue[0] = true;

        
        week++;

        //tSel.Expand(tSel.menuButtons[2]);
    }

    public void EndCareer()
    {
        Debug.Log("Ending Career");
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
