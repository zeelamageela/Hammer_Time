using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TigerForge;
using System;
using Random = UnityEngine.Random;

public class TournySettingsPersist : MonoBehaviour
{
    GameSettings gs;
    StoryManager sm;
    TournyManager tm;
    GameSettingsPersist gsp;

    public bool loadGame;
    public bool aiYellow;
    public bool aiRed;
    public bool debug;
    public bool mixed;
    public bool tourny;
    public string teamName;
    public string redTeamName;
    public string yellowTeamName;
    public int draw;
    public int playoffRound;

    public List<Team_List> teamList;
    public Team[] teams;
    public Team[] playoffTeams;
    public int playerTeamIndex;
    public Vector2Int[] score;

    public Team playerTeam;
    EasyFileSave myFile;

    public static TournySettingsPersist instance;


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

        //Application.targetFrameRate = 30;

    }

    private void Start()
    {
        //myFile = new EasyFileSave("my_game_data");

        //gs = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        gsp = FindObjectOfType<GameSettingsPersist>();

        score = new Vector2Int[12];
    }


    public void LoadTournySettings()
    {
        TournySettings ts = GameObject.Find("TournySettings").GetComponent<TournySettings>();

        Debug.Log("Loading Tourny Settings to TSP");
        //Debug.Log("Ends is " + myFile.GetInt("End Total"));

        teamName = ts.teamName;
        draw = 0;
        playoffRound = 0;
        //redScore = myFile.GetInt("Red Score");
        //yellowScore = myFile.GetInt("Yellow Score");
    }

    public void TournySetup()
    {
        gsp.TournySetup();
        TournyManager tm = GameObject.Find("TournyManager").GetComponent<TournyManager>();
        tourny = true;
        draw = tm.draw;
        playoffRound = tm.playoffRound;
        playerTeamIndex = tm.playerTeam;
        teamList = tm.teamList;
        teams = tm.teams;
        playerTeam = teams[playerTeamIndex];

        if (draw >= tm.drawFormat.Length)
        {
            playoffTeams = tm.playoffTeams;
        }
        if (Random.Range(0f, 1f) < 0.5f)
        {
            aiYellow = true;
            aiRed = false;
            yellowTeamName = playerTeam.nextOpp;
            redTeamName = playerTeam.name;
        }
        else
        {
            aiRed = true;
            aiYellow = false;
            yellowTeamName = playerTeam.name;
            redTeamName = playerTeam.nextOpp;
        }
    }

    private void Update()
    {


    }
}

