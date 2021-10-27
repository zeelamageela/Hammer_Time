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

    public int week;
    public int seasonLength;
    public string playerName;
    public string teamName;
    public Color teamColour;
    public int playerTeamIndex;
    public float earnings;
    public Vector2 record;
    public bool provQual;
    public bool tourQual;
    public Vector2 tourRecord;

    public bool inProgress;
    public int season;
    public int totalTeams;
    public int tourTeams;
    public int provTeams;

    public Team playerTeam;
    public Team[] teams;
    public Team[] currentTournyTeams;
    public Tourny currentTourny;
    public List<Team_List> tourRankList;
    public List<Standings_List> provRankList;

    public Tourny[] tournies;
    public Tourny[] tour;
    public Tourny[] prov;
    public Tourny[] champ;
    public Tourny[] activeTournies;

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
        //gsp = FindObjectOfType<GameSettingsPersist>();
        tm = FindObjectOfType<TournyManager>();
        pm = FindObjectOfType<PlayoffManager>();
        //ts = FindObjectOfType<TournySettings>();
        //tSel = FindObjectOfType<TournySelector>();

        //teams = new Team[totalTeams];
        tourRankList = new List<Team_List>();
        provRankList = new List<Standings_List>();

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
        tourTeams = 16;
        provTeams = 16;
    }

    public void LoadFromGSP()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();

        earnings = gsp.earnings;
        record = gsp.record;

    }
    public void LoadCareer()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
        tSel = FindObjectOfType<TournySelector>();
        myFile = new EasyFileSave("my_player_data");
        tTeamList = FindObjectOfType<TournyTeamList>();
        teams = new Team[totalTeams];

        if (myFile.Load())
        {
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
            tourRecord = myFile.GetUnityVector2("Tour Record");

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

                for (int i = 0; i < prov.Length; i++)
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
                for (int i = 0; i < tour.Length; i++)
                {
                    tour[i].id = tourIDList[i];

                    for (int j = 0; j < tSel.tour.Length; j++)
                    {
                        if (tour[i].id == tSel.tour[j].id)
                            tour[i] = tSel.tour[j];
                    }
                    tour[i].complete = tourCompleteList[i];
                }
                for (int i = 0; i < tournies.Length; i++)
                {
                    tournies[i].id = tourniesIDList[i];

                    for (int j = 0; j < tSel.tournies.Length; j++)
                    {
                        if (tournies[i].id == tSel.tournies[j].id)
                            tournies[i] = tSel.tournies[j];
                    }
                    tournies[i].complete = tourniesCompleteList[i];
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

            //for (int i = 0; i < teams.Length; i++)
            //{
            //    teams[i] = tTeamList.teams[i];
            //}

            Debug.Log("Total ID List Length is " + idList.Length);
            Debug.Log("Total Teams List Length is " + teams.Length);

            for (int i = 0; i < teams.Length; i++)
            {
                //teams[i].id = idList[i];

                //Debug.Log("Id List - " + idList[i]);
                for (int j = 0; j < tTeamList.teams.Length; j++)
                {
                    if (idList[i] == tTeamList.teams[j].id)
                        teams[i] = tTeamList.teams[j];
                }

                //if (teams[i].id == playerTeamIndex)
                //    teams[i].name = teamName;

                teams[i].wins = winsList[i];
                teams[i].loss = lossList[i];

                provRankList.Add(new Standings_List(teams[i]));
            }

            gsp.inProgress = myFile.GetBool("Tourny In Progress");

            Debug.Log("ProvRankList count is " + provRankList.Count);

            if (gsp.inProgress)
            {
                currentTourny.name = myFile.GetString("Current Tourny Name");
                currentTourny.id = myFile.GetInt("Current Tourny ID");
                currentTourny.tour = myFile.GetBool("Current Tourny Tour");
                currentTourny.qualifier = myFile.GetBool("Current Tourny Qualifier");
                currentTourny.championship = myFile.GetBool("Current Tourny Championship");
                currentTourny.prizeMoney = myFile.GetInt("Prize Money");

                int[] tournyIDList = myFile.GetArray<int>("Tourny Team ID List");
                int[] tournyWinsList = myFile.GetArray<int>("Tourny Wins List");
                int[] tournyLossList = myFile.GetArray<int>("Tourny Loss List");
                //currentTournyTeams = new Team[tournyIDList.Length];

                for (int i = 0; i < currentTournyTeams.Length; i++)
                {
                    currentTournyTeams[i].id = tournyIDList[i];

                    for (int j = 0; j < teams.Length; j++)
                    {
                        if (currentTournyTeams[i].id == teams[j].id)
                            currentTournyTeams[i] = teams[j];
                    }

                    currentTournyTeams[i].wins = tournyWinsList[i];
                    currentTournyTeams[i].loss = tournyLossList[i];
                }

                for (int i = 0; i < teams.Length; i++)
                {
                    if (teams[i].id == playerTeamIndex)
                    {
                        teams[i].name = teamName;
                        teams[i].wins = (int)record.x;
                        teams[i].loss = (int)record.y;
                    }
                }
            }

            myFile.Dispose();
        }
    }

    public void SaveCareer()
    {
        myFile = new EasyFileSave("my_player_data");
        tSel = FindObjectOfType<TournySelector>();
        tm = FindObjectOfType<TournyManager>();
        gsp = FindObjectOfType<GameSettingsPersist>();

        myFile.Add("Tourny In Progress", gsp.inProgress);
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

        int[] idList = new int[teams.Length];
        int[] winsList = new int[teams.Length];
        int[] lossList = new int[teams.Length];

        for (int i = 0; i < teams.Length; i++)
        {
            idList[i] = teams[i].id;
            //Debug.Log("Id List - " + idList[i]);
            winsList[i] = teams[i].wins;
            lossList[i] = teams[i].loss;
        }
        Debug.Log("Total Id List length is " + idList.Length);
        myFile.Add("Total ID List", idList);
        myFile.Add("Total Wins List", winsList);
        myFile.Add("Total Loss List", lossList);

        myFile.Add("Current Tourny Name", currentTourny.name);
        myFile.Add("Current Tourny ID", currentTourny.id);
        myFile.Add("Current Tourny Tour", currentTourny.tour);
        myFile.Add("Current Tourny Qualifier", currentTourny.qualifier);
        myFile.Add("Current Tourny Championship", currentTourny.championship);
        myFile.Add("Prize Money", currentTourny.prizeMoney);

        if (tSel)
        {
            int[] provIDList = new int[tSel.provQual.Length];
            bool[] provCompleteList = new bool[tSel.provQual.Length];
            int[] tourIDList = new int[tSel.tour.Length];
            bool[] tourCompleteList = new bool[tSel.tour.Length];
            int[] tourniesIDList = new int[tSel.tournies.Length];
            bool[] tourniesCompleteList = new bool[tSel.tournies.Length];

            for (int i = 0; i < tSel.provQual.Length; i++)
            {
                provIDList[i] = tSel.provQual[i].id;
                provCompleteList[i] = tSel.provQual[i].complete;
            }
            for (int i = 0; i < tSel.tour.Length; i++)
            {
                tourIDList[i] = tSel.tour[i].id;
                tourCompleteList[i] = tSel.tour[i].complete;
            }
            for (int i = 0; i < tSel.tournies.Length; i++)
            {
                tourniesIDList[i] = tSel.tournies[i].id;
                tourniesCompleteList[i] = tSel.tournies[i].complete;
            }
            myFile.Add("Tour Championship Complete", tSel.tourChampionship.complete);
            myFile.Add("Prov Championship Complete", tSel.provChampionship.complete);
            myFile.Add("Prov ID List", provIDList);
            myFile.Add("Prov Complete List", tourniesCompleteList);
            myFile.Add("Tour ID List", tourniesIDList);
            myFile.Add("Tour Complete List", tourniesCompleteList);
            myFile.Add("Tournies ID List", tourniesIDList);
            myFile.Add("Tournies Complete List", tourniesCompleteList);
            myFile.Add("Number Of Teams", currentTourny.teams);

            Debug.Log("Number of Teams in CM Save - " + currentTourny.teams);
        }

        //int[] tournyTeamIDList = new int[currentTournyTeams.Length];
        //int[] tournyWinsList = new int[currentTournyTeams.Length];
        //int[] tournyLossList = new int[currentTournyTeams.Length];

        //for (int i = 0; i < currentTournyTeams.Length; i++)
        //{
        //    currentTournyTeams[i].id = tournyTeamIDList[i];
        //    currentTournyTeams[i].wins = tournyWinsList[i];
        //    currentTournyTeams[i].loss = tournyLossList[i];
        //}

        if (tm)
        {
            Debug.Log("Saving Career and TM active, Draw is - " + gsp.draw);
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

            for (int i = 0; i < teams.Length; i++)
            {
                tournyNameList[i] = teams[i].name;
                tournyWinsList[i] = teams[i].wins;
                tournyLossList[i] = teams[i].loss;
                tournyRankList[i] = teams[i].rank;
                tournyNextOppList[i] = teams[i].nextOpp;
                tournyStrengthList[i] = teams[i].strength;
                tournyIDList[i] = teams[i].id;
                //Debug.Log("Tourny Id List - " + idList[i]);
            }

            myFile.Add("Tourny Name List", tournyNameList);
            myFile.Add("Tourny Wins List", tournyWinsList);
            myFile.Add("Tourny Loss List", tournyLossList);
            myFile.Add("Tourny Rank List", tournyRankList);
            myFile.Add("Tourny NextOpp List", tournyNextOppList);
            myFile.Add("Tourny Strength List", tournyStrengthList);
            myFile.Add("Tourny Team ID List", tournyIDList);
        }

        //myFile.Add("Tourny Team ID List", tournyTeamIDList);
        //myFile.Add("Tourny Wins List", tournyWinsList);
        //myFile.Add("Tourny Loss List", tournyLossList);

        myFile.Append();
    }

    public void SetupTourny()
    {
        tSel = FindObjectOfType<TournySelector>();
        gsp = FindObjectOfType<GameSettingsPersist>();
        currentTourny = tSel.currentTourny;
        Shuffle(teams);
        currentTournyTeams = new Team[currentTourny.teams];

        bool inList = false;
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
        
        //Shuffle(currentTournyTeams);
        Debug.Log("Player Team is " + playerTeamIndex);
        gsp.teams = currentTournyTeams;
    }

    public void TournyResults()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
        //TournyManager tm = FindObjectOfType<TournyManager>();
        record = gsp.record;
        earnings = gsp.earnings;
        currentTournyTeams = gsp.teams;
        if (currentTourny.qualifier)
        {
            for (int i = 0; i < currentTourny.teams; i++)
            {
                if (playerTeamIndex == currentTournyTeams[i].id)
                {
                    if (currentTournyTeams[i].rank < 5)
                        provQual = true;
                }
            }
        }
        if (currentTourny.tour)
        {
            for (int i = 0; i < tourTeams; i++)
            {
                if (playerTeamIndex == currentTournyTeams[i].id)
                    tourRecord.x += currentTournyTeams[i].wins;
                //if (playerTeamIndex == currentTournyTeams[i].id)
            }
        }
        Debug.Log("Current Team List count is " + currentTournyTeams.Length);

        Debug.Log("Rank List count is " + provRankList.Count);
        Debug.Log("First Prov Team is " + provRankList[0].team.name);
        for (int i = 0; i < teams.Length; i++)
        {
            for (int j = 0; j < provRankList.Count; j++)
            {
                if (provRankList[j].team.id == teams[i].id)
                {
                    provRankList[j].team.wins = teams[i].wins;
                    provRankList[j].team.loss = teams[i].loss;
                }
            }
        }

        Debug.Log("Rank List count is " + provRankList.Count);
        provRankList.Sort();
        Debug.Log("Top Ranked Team is " + provRankList[0].team.name);
        //record += new Vector2(gsp.playerTeam.wins, gsp.playerTeam.loss);
        Debug.Log("Record is " + record.x + " - " + record.y);
        week++;
        SaveCareer();
    }

    public void PlayTourny()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
        inProgress = true;
        
        earnings = gsp.earnings;
        SaveCareer();
    }

    public void NextWeek()
    {
        week++;
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
        playerTeamIndex = teams[0].id;
        gsp.playerTeamIndex = playerTeamIndex;
        week++;
    }

    public void NewSeason()
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
            provRankList.Add(new Standings_List(teams[i]));
        }


        teams[0].name = teamName;
        playerTeamIndex = teams[0].id;
        gsp.playerTeamIndex = playerTeamIndex;
        week++;
        //Shuffle(teams);

        //for (int i = 1; i < tourTeams; i++)
        //{
        //    tourRankList[i].team = teams[i];
        //}
        //tourRankList[0].team = tTeamList.playerTeam;
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
