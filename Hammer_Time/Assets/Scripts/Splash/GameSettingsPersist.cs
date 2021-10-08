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
    public bool tourny;
    public bool story;
    public bool third;
    public bool skip;
    public string teamName;

    public string redTeamName;
    public Color redTeamColour;
    public TeamMember[] redTeam;
    public string yellowTeamName;
    public Color yellowTeamColour;
    public TeamMember[] yellowTeam;

    public int draw;
    public int playoffRound;

    public List<Team_List> teamList;
    public Team[] teams;
    public Team[] playoffTeams;
    public int playerTeamIndex;
    public Vector2Int[] score;

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
        endCurrent = 1;
        rocks = gs.rocks;
        rockCurrent = 0;
        redScore = 0;
        yellowScore = 0;
        redHammer = gs.redHammer;
        aiYellow = gs.aiYellow;
        aiRed = gs.aiRed;
        mixed = gs.mixed;
        skip = gs.team;
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
        loadGame = true;
        myFile = new EasyFileSave("my_game_data");
        //load all the saved values
        if (myFile.Load())
        {
            Debug.Log("Loading to GSP");
            Debug.Log("Ends is " + myFile.GetInt("End Total"));

            ends = myFile.GetInt("End Total");
            endCurrent = myFile.GetInt("Current End");
            rocks = myFile.GetInt("Rocks Per Team");
            rockCurrent = myFile.GetInt("Current Rock");
            redHammer = myFile.GetBool("Red Hammer");
            aiRed = myFile.GetBool("Ai Red");
            aiYellow = myFile.GetBool("Ai Yellow");
            mixed = myFile.GetBool("Mixed");
            skip = myFile.GetBool("Team");
            debug = myFile.GetBool("Debug");

            redScore = myFile.GetInt("Red Score");
            yellowScore = myFile.GetInt("Yellow Score");

            score = new Vector2Int[endCurrent];

            for (int i = 1; i < endCurrent; i++)
            {
                Vector2 tempScore = myFile.GetUnityVector2("End " + i + " Score");
                score[i - 1] = Vector2Int.RoundToInt(tempScore);
                Debug.Log("Score " + i + " - " + score[i].x + ", " + score[i].y);
            }
        }
    }

    public void LoadFromGM()
    {
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
        third = gm.target;
        skip = gm.target;

        score = new Vector2Int[12];
        score[endCurrent - 2] = new Vector2Int(redScore, yellowScore);
        //redScore = myFile.GetInt("Red Score");
        //yellowScore = myFile.GetInt("Yellow Score");
    }

    public void LoadTournySettings()
    {
        TournySettings ts = GameObject.Find("TournySettings").GetComponent<TournySettings>();

        Debug.Log("Loading Tourny Settings to GSP");
        //Debug.Log("Ends is " + myFile.GetInt("End Total"));

        teamName = ts.teamName;
        teamColour = ts.teamColour;
        
        ends = ts.ends;
        endCurrent = 0;
        rocks = ts.rocks;
        draw = 0;
        playoffRound = 0;
        //redScore = myFile.GetInt("Red Score");
        //yellowScore = myFile.GetInt("Yellow Score");
    }

    public void TournySetup()
    {
        TournyManager tm = FindObjectOfType<TournyManager>();
        PlayoffManager pm = FindObjectOfType<PlayoffManager>();

        tourny = true;
        draw = tm.draw;
        playoffRound = pm.playoffRound;
        playerTeamIndex = tm.playerTeam;
        teamList = tm.teamList;
        teams = tm.teams;
        playerTeam = teams[playerTeamIndex];
        endCurrent = 1;
        redScore = 0;
        yellowScore = 0;

        //playerGO = tm.playerGO;
        if (draw >= tm.drawFormat.Length)
        {
            playoffTeams = new Team[9];
            for (int i = 0; i < playoffTeams.Length; i++)
            {
                playoffTeams[i] = pm.playoffTeams[i];
            }
            //playoffTeams = pm.playoffTeams;
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

    public void StoryGame()
    {
        story = true;
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
        third = sm.third;
        skip = sm.skip;

        //redScore = myFile.GetInt("Red Score");
        //yellowScore = myFile.GetInt("Yellow Score");
    }

    public void AutoSave()
    {
        //GameData data = SaveSystem.LoadPlayer();
        //ends = data.endTotal;
        //endCurrent = data.endCurrent;
        //rocks = data.rocks;
        //rockCurrent = data.rockCurrent;
        //redHammer = data.redHammer;
        //aiYellow = data.aiYellow;
        //yellowScore = data.yellowScore;
        //redScore = data.redScore;
    }

    private void Update()
    {
        if (gs)
        {
            ends = gs.ends;
            rocks = gs.rocks;
            aiYellow = gs.aiYellow;
            aiRed = gs.aiRed;
            loadGame = false;
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
}
