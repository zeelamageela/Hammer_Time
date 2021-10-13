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
    public List<Team_List> provQualList;

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
        provQualList = new List<Team_List>();

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
            record = myFile.GetUnityVector2("Career Record");
            earnings = myFile.GetFloat("Career Earnings");
            provQual = myFile.GetBool("Prov Qual");
            tourQual = myFile.GetBool("Tour Qual");
            tourRecord = myFile.GetUnityVector2("Tour Record");

            int[] idList = myFile.GetArray<int>("Total ID List");
            int[] winsList = myFile.GetArray<int>("Total Wins List");
            int[] lossList = myFile.GetArray<int>("Total Loss List");

            for (int i = 0; i < teams.Length; i++)
            {
                teams[i].id = idList[i];
                for (int j = 0; j < tTeamList.teams.Length; j++)
                {
                    if (teams[i].id == tTeamList.teams[j].id)
                        teams[i] = tTeamList.teams[j];
                }

                teams[i].wins = winsList[i];
                teams[i].loss = lossList[i];
            }

            if (myFile.GetBool("Tourny In Progress"))
            {
                int[] tournyIDList = myFile.GetArray<int>("Tourny Team ID List");
                int[] tournyWinsList = myFile.GetArray<int>("Tourny Wins List");
                int[] tournyLossList = myFile.GetArray<int>("Tourny Loss List");

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
            }

            myFile.Dispose();
        }
    }

    void SaveCareer()
    {
        myFile = new EasyFileSave("my_player_data");
        tSel = FindObjectOfType<TournySelector>();

        myFile.Add("Player Name", playerName);
        myFile.Add("Team Name", teamName);
        myFile.Add("Team Colour", teamColour);
        myFile.Add("Player Team Index", playerTeamIndex);
        myFile.Add("Week", week);
        myFile.Add("Season", season);
        myFile.Add("Career Record", record);
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
            winsList[i] = teams[i].wins;
            lossList[i] = teams[i].loss;
        }
        Debug.Log("Total Id List length is " + idList.Length);
        myFile.Add("Total ID List", idList);
        myFile.Add("Total Wins List", winsList);
        myFile.Add("Total Loss List", lossList);

        myFile.Add("Current Tourny ID", currentTourny.id);
        myFile.Add("Current Tourny Tour", currentTourny.tour);
        myFile.Add("Current Tourny Qualifier", currentTourny.qualifier);
        myFile.Add("Current Tourny Championship", currentTourny.championship);

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
        }

        int[] tournyTeamIDList = new int[currentTournyTeams.Length];
        int[] tournyWinsList = new int[currentTournyTeams.Length];
        int[] tournyLossList = new int[currentTournyTeams.Length];

        for (int i = 0; i < currentTournyTeams.Length; i++)
        {
            currentTournyTeams[i].id = tournyTeamIDList[i];
            currentTournyTeams[i].wins = tournyWinsList[i];
            currentTournyTeams[i].loss = tournyLossList[i];
        }

        myFile.Add("Tourny Team ID List", tournyTeamIDList);
        myFile.Add("Tourny Wins List", tournyWinsList);
        myFile.Add("Tourny Loss List", tournyLossList);

        myFile.Save();
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
            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i].id == playerTeamIndex)
                {
                    currentTournyTeams[0] = teams[i];
                }
            }
        }
        
        Shuffle(currentTournyTeams);
        Debug.Log("Player Team is " + playerTeamIndex);
        gsp.teams = currentTournyTeams;
    }

    public void TournyResults()
    {
        TournyManager tm = FindObjectOfType<TournyManager>();
        record = gsp.record;
        earnings = gsp.earnings;
        currentTournyTeams = gsp.teams;
        record += new Vector2(gsp.playerTeam.wins, gsp.playerTeam.loss);
        Debug.Log("Record is " + record.x + " - " + record.y);
        week++;
        SaveCareer();
    }

    public void PlayTourny()
    {
        inProgress = true;
        
        earnings = gsp.earnings;
        SaveCareer();
    }

    public void NextWeek()
    {
        week++;
        tSel.SetActiveTournies();
    }

    public void NewSeason()
    {
        tTeamList = FindObjectOfType<TournyTeamList>();
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
