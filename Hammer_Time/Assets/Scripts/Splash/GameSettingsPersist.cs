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

    public string firstName;
    public string teamName;
    public float earnings;
    public float cash;

    public Vector2 record;

    public int week;
    public CareerStats cStats;
    public CareerStats oppStats;

    public string redTeamName;
    public Color redTeamColour;
    public TeamMember[] redTeam;
    public string yellowTeamName;
    public Color yellowTeamColour;
    public TeamMember[] yellowTeam;

    public int draw;
    public int playoffRound;
    public int numberOfTeams;
    public int prize;
    public bool careerLoad;
    public bool tournyInProgress;
    public bool gameInProgress;

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

        teamM = FindObjectOfType<TeamManager>();

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
        //Debug.Log("Ends is " + myFile.GetInt("End Total"));

        ends = gm.endTotal;
        endCurrent = gm.endCurrent;
        rocks = gm.rocksPerTeam;
        rockCurrent = gm.rockCurrent;
        redScore = gm.redScore;
        yellowScore = gm.yellowScore;
        redHammer = gm.redHammer;
        aiYellow = gm.aiTeamYellow;
        aiRed = gm.aiTeamRed;

        //third = gm.target;
        //skip = gm.target;

        //score[endCurrent] = new Vector2Int(redScore, yellowScore);
        //redScore = myFile.GetInt("Red Score");
        //yellowScore = myFile.GetInt("Yellow Score");
    }

    public void LoadFromTournySelector()
    {
        TournySelector ts = FindObjectOfType<TournySelector>();
        CareerManager cm = FindObjectOfType<CareerManager>();

        Debug.Log("Loading Tourny Settings to GSP");
        //Debug.Log("Ends is " + myFile.GetInt("End Total"));
        firstName = cm.playerName;
        teamName = cm.teamName;
        teamColour = cm.teamColour;
        earnings = 0;
        cash = 0;
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

        CareerManager cm = FindObjectOfType<CareerManager>();

        Debug.Log("Loading Tourny Settings to GSP");
        //Debug.Log("Ends is " + myFile.GetInt("End Total"));
        teamColour = cm.teamColour;
        //earnings = ts.earnings;
        week = cm.week;
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
        //redScore = myFile.GetInt("Red Score");
        //yellowScore = myFile.GetInt("Yellow Score");
        cm.LoadFromGSP(this);
    }

    public void TournySetup(int btn = 0)
    {
        Debug.Log("Tourny Setup GSP");
        TournyManager tm = FindObjectOfType<TournyManager>();
        PlayoffManager pm = FindObjectOfType<PlayoffManager>();
        CashGames cg = FindObjectOfType<CashGames>();
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
            else
            {
                PlayoffManager_SingleK pm1k = FindObjectOfType<PlayoffManager_SingleK>();
                playoffRound = pm1k.playoffRound;
                KO1 = true;
            }
            teamList = tm.teamList;
            teams = tm.teams;
            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i].id == playerTeamIndex)
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

            yellowTeamName = playerTeam.nextOpp;
            redTeamName = playerTeam.name;
            redTeamColour = teamColour;
            yellowTeamColour = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f));
        }
        else
        {
            aiRed = true;
            aiYellow = false;

            yellowTeamName = playerTeam.name;
            redTeamName = playerTeam.nextOpp;
            yellowTeamColour = teamColour;
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
        PlayoffManager_TripleK pm = FindObjectOfType<PlayoffManager_TripleK>();
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

            yellowTeamName = playerTeam.nextOpp;
            redTeamName = playerTeam.name;
            redTeamColour = teamColour;
            yellowTeamColour = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f));
        }
        else
        {
            aiRed = true;
            aiYellow = false;
            yellowTeamName = playerTeam.name;
            redTeamName = playerTeam.nextOpp;
            yellowTeamColour = teamColour;
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
        Debug.Log("Load Career GSP");
        CareerManager cm = FindObjectOfType<CareerManager>();
        //teamList = new List<Team_List>();
        firstName = cm.name;
        teamName = cm.teamName;
        teamColour = cm.teamColour;

        //earnings = myFile.GetFloat("Career Earnings");
        //record = myFile.GetUnityVector2("Career Record");
        //inProgress = cm.inProgress;

        //numberOfTeams = myFile.GetInt("Number Of Teams");
        week = cm.week;
    }

    public void LoadTourny()
    {
        Debug.Log("Load Tourny GSP");
        CareerManager cm = FindObjectOfType<CareerManager>();
        bg = cm.currentTourny.BG;
        crowdDensity = cm.currentTourny.crowdDensity;
        prize = cm.currentTourny.prizeMoney;
        numberOfTeams = cm.currentTourny.teams;
        //cm.LoadCareer(this);
        TournyManager tm = FindObjectOfType<TournyManager>();
        teamList = new List<Team_List>();
        myFile = new EasyFileSave("my_player_data");
        //inProgress = true;
        if (myFile.Load())
        {
            draw = myFile.GetInt("Draw");
            ends = myFile.GetInt("Ends");
            games = myFile.GetInt("Games");
            rocks = myFile.GetInt("Rocks");
            numberOfTeams = myFile.GetInt("Number Of Teams");
            playoffRound = myFile.GetInt("Playoff Round");
            playerTeamIndex = myFile.GetInt("Player Team");

            teams = new Team[numberOfTeams];

            for (int i = 0; i < numberOfTeams; i++)
            {
                teams[i] = cm.currentTournyTeams[i];

                if (teams[i].player)
                {
                    Debug.Log("i == playerTeamIndex is " + playerTeamIndex);
                    teams[i].name = teamName;
                    playerTeam = teams[i];
                    Debug.Log("Player Team id is " + teams[i].id);
                }
            }
            //record = new Vector2(playerTeam.wins, playerTeam.loss);
            for (int i = 0; i < numberOfTeams; i++)
            {
                teamList.Add(new Team_List(teams[i]));
            }

            int[] playoffIDList = myFile.GetArray<int>("Playoff ID List");
            int[] playoffRankList = myFile.GetArray<int>("Playoff Rank List");

            playoffTeams = new Team[playoffIDList.Length];

            for (int i = 0; i < playoffIDList.Length; i++)
            {
                if (playoffIDList[i] < 99)
                {
                    for (int j = 0; j < teams.Length; j++)
                    {
                        if (playoffIDList[i] == teams[j].id)
                        {
                            playoffTeams[i] = teams[j];
                            playoffTeams[i].rank = playoffRankList[i];
                        }
                    }
                }
                else
                    playoffTeams[i] = tm.tTeamList.nullTeam;
            }
            Debug.Log("teamList Count is " + teamList.Count);

            //score = new Vector2Int[ends + 1];

            //int[] redScoreList = myFile.GetArray<int>("Red Score List");
            //int[] yellowScoreList = myFile.GetArray<int>("Yellow Score List");

            //Debug.Log("Red Score List is " + redScoreList.Length + " long");
            //for (int i = 0; i < score.Length; i++)
            //{
            //    score[i].x = redScoreList[i];
            //    score[i].y = yellowScoreList[i];
            //}

            myFile.Dispose();
        }
    }

    public void LoadKOTourny()
    {
        TournyTeamList tTeamList = FindObjectOfType<TournyTeamList>();
        CareerManager cm = FindObjectOfType<CareerManager>();
        //cm.LoadCareer();
        PlayoffManager_TripleK pm3k = FindObjectOfType<PlayoffManager_TripleK>();
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
                    teams[i].name = teamName;
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
        CareerManager cm = FindObjectOfType<CareerManager>();
        cm.SaveCareer();
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
        //GameManager gm = FindObjectOfType<GameManager>();

        //gm.endCurrent = 10;
    }

    IEnumerator Wait()
    {
        yield return new WaitForEndOfFrame();
    }
}
