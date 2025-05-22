using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettingsPersist2 : MonoBehaviour
{
    public static GameSettingsPersist2 instance;

    // Match/tournament-specific data
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

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }
        Application.targetFrameRate = 30;
    }

    // Called by CareerManager to set up a new match
    public void InitializeMatch(MatchSetupData setup)
    {
        teams = setup.teams;
        playerTeamIndex = setup.playerTeamIndex;
        teamColour = setup.teamColour;
        redTeamName = setup.redTeamName;
        yellowTeamName = setup.yellowTeamName;
        ends = setup.ends;
        rocks = setup.rocks;
        prize = setup.prize;
        // ...set other match fields as needed
    }

    // Called by CareerManager to get match results
    public MatchResult GetMatchResult()
    {
        return new MatchResult
        {
            teams = teams,
            playerTeamIndex = playerTeamIndex,
            score = score,
            redScore = redScore,
            yellowScore = yellowScore,
            // ...add more as needed
        };
    }
}

// Helper class for initializing a match
public class MatchSetupData
{
    public Team[] teams;
    public int playerTeamIndex;
    public Color teamColour;
    public string redTeamName;
    public string yellowTeamName;
    public int ends;
    public int rocks;
    public int prize;
    // Add more as needed
}

// Helper class for returning match results
public class MatchResult
{
    public Team[] teams;
    public int playerTeamIndex;
    public Vector2Int[] score;
    public int redScore;
    public int yellowScore;
    // Add more as needed
}

