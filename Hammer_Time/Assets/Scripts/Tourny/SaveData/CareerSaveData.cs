using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main save data structure for career mode.
/// This class contains all the data needed to save and restore a career.
/// Version 1.0 - Initial implementation
/// </summary>
[Serializable]
public class CareerSaveData
    {
        public int version = 1;
        public string saveDate;
        
        // Player Identity
        public string playerName;
        public string teamName;
        public SerializableColor teamColour;
        public int playerTeamIndex;
        
        // Progress & Resources
        public int week;
        public int season;
        public float cash;
        public float earnings;
        public float xp;
        public int level;
        public int skillPoints;
        
        // Career Record (wins-losses)
        public int careerWins;
        public int careerLosses;
        
        // Career Stats
        public CareerStatsData careerStats;
        public CareerStatsData oppStats;
        
        // Qualifications
        public bool provQual;
        public bool tourQual;
        
        // Teams
        public List<TeamData> teams;
        public List<TeamData> tourTeams;
        public List<TeamRecordData> teamRecords;
        public List<TeamRecordData> tourRecords;
        
        // Active Players
        public List<PlayerData> activePlayers;
        
        // Equipment
        public List<int> activeEquipIDs;
        public List<int> inventoryIDs;
        public EquipmentCollectionData allEquipment;
        
        // Cards/Sponsors
        public List<int> cardPowerUpIDs;
        public List<int> cardSponsorIDs;
        public List<int> activeCardIDs;
        public List<int> playedCardIDs;
        public List<int> activeCardLengths;
        
        // Tournaments
        public List<TournamentProgressData> provincialTournaments;
        public List<TournamentProgressData> tourTournaments;
        public List<TournamentProgressData> regularTournaments;
        public bool tourChampionshipComplete;
        public bool provChampionshipComplete;
        
        // Tournament Results
        public List<int> tournamentResults;
        
        // Dialogue Flags
        public DialogueFlagsData dialogueFlags;
        
        // Game State (if in progress)
        public GameStateData currentGameState;
        public TournamentStateData currentTournamentState;
        
        // Trophy Collection
        public List<bool> allTimeTrophies;
        public List<bool> currentSeasonTrophies;
        
        public CareerSaveData()
        {
            saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            teams = new List<TeamData>();
            tourTeams = new List<TeamData>();
            teamRecords = new List<TeamRecordData>();
            tourRecords = new List<TeamRecordData>();
            activePlayers = new List<PlayerData>();
            activeEquipIDs = new List<int>();
            inventoryIDs = new List<int>();
            cardPowerUpIDs = new List<int>();
            cardSponsorIDs = new List<int>();
            activeCardIDs = new List<int>();
            playedCardIDs = new List<int>();
            activeCardLengths = new List<int>();
            provincialTournaments = new List<TournamentProgressData>();
            tourTournaments = new List<TournamentProgressData>();
            regularTournaments = new List<TournamentProgressData>();
            tournamentResults = new List<int>();
            allTimeTrophies = new List<bool>();
            currentSeasonTrophies = new List<bool>();
        }
    }
    
    [Serializable]
    public class SerializableColor
    {
        public float r;
        public float g;
        public float b;
        public float a;
        
        public SerializableColor() { }
        
        public SerializableColor(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }
        
        public Color ToColor()
        {
            return new Color(r, g, b, a);
        }
    }
    
    [Serializable]
    public class CareerStatsData
    {
        public int drawAccuracy;
        public int guardAccuracy;
        public int takeOutAccuracy;
        public int sweepStrength;
        public int sweepEndurance;
        public int sweepCohesion;
    }
    
    [Serializable]
    public class TeamData
    {
        public int id;
        public string name;
        public bool isPlayer;
        
        // TOURNAMENT STATE (reset each tournament)
        public int tournamentWins;
        public int tournamentLosses;
        public float tournamentEarnings;
        public int rank;               // Placement in current tournament
        public string nextOpp;         // Next opponent in current tournament
        
        // SEASON CUMULATIVE (reset each season)
        public int seasonWins;
        public int seasonLosses;
        public float seasonEarnings;
        
        // TOUR CUMULATIVE
        public float tourPoints;
        
        // LEGACY COMPATIBILITY (kept for backwards compatibility)
        public int wins
        {
            get { return seasonWins; }
            set { seasonWins = value; }
        }
        
        public int loss
        {
            get { return seasonLosses; }
            set { seasonLosses = value; }
        }
        
        public float earnings
        {
            get { return seasonEarnings; }
            set { seasonEarnings = value; }
        }
        
        // DEPRECATED: Use tourPoints directly
        public float tourRecordX;  // Kept for backwards compatibility
        public float tourRecordY;  // Kept for backwards compatibility
        
        // Team Stats
        public int draw;
        public int guard;
        public int takeOut;
        public int sweepStrength;
        public int sweepEnduro;
        public int sweepCohesion;
        public int strength;
        
        public List<PlayerData> players;
        
        public TeamData()
        {
            players = new List<PlayerData>();
        }
    }
    
    [Serializable]
    public class PlayerData
    {
        public int id;
        public string name;
        public int draw;
        public int guard;
        public int takeOut;
        public int sweepStrength;
        public int sweepEnduro;
        public int sweepCohesion;
        
        // Opponent stats
        public int oppDraw;
        public int oppGuard;
        public int oppTakeOut;
        public int oppStrength;
        public int oppEnduro;
        public int oppCohesion;
        
        // Metadata (for display purposes)
        public float cost;
        public string description;
        // Note: image (Sprite) cannot be serialized, will be restored from playerPool by ID
    }
    
    [Serializable]
    public class TeamRecordData
    {
        public int teamId;
        public int wins;
        public int losses;
        public float earnings;
    }
    
    [Serializable]
    public class EquipmentCollectionData
    {
        public List<EquipmentData> handles;
        public List<EquipmentData> heads;
        public List<EquipmentData> footwear;
        public List<EquipmentData> apparel;
        
        public EquipmentCollectionData()
        {
            handles = new List<EquipmentData>();
            heads = new List<EquipmentData>();
            footwear = new List<EquipmentData>();
            apparel = new List<EquipmentData>();
        }
    }
    
    [Serializable]
    public class EquipmentData
    {
        public int id;
        public float cost;
        public SerializableColor color;
        public int duration;
        public int[] stats;
        public int[] oppStats;
    }
    
    [Serializable]
    public class TournamentProgressData
    {
        public int id;
        public bool complete;
        public bool trophyWon;
    }
    
    [Serializable]
    public class DialogueFlagsData
    {
        public bool[] coachDialogue;
        public bool[] qualDialogue;
        public bool[] reviewDialogue;
        public bool[] introDialogue;
        public bool[] helpDialogue;
        public bool[] strategyDialogue;
        public bool[] storyDialogue;
        public int storyBlockIndex;
    }
    
    [Serializable]
    public class GameStateData
    {
        public bool gameInProgress;
        public bool tournyInProgress;
        public bool isTournyGame;
        public int ends;
        public int currentEnd;
        public int rocks;
        public int currentRock;
        public bool redHammer;
        public bool aiYellow;
        public bool aiRed;
        public int yellowScore;
        public int redScore;
        public string yellowTeamName;
        public string redTeamName;
        
        public List<Vector2Data> rockPositions;
        public List<bool> rockInPlay;
        public List<Vector2IntData> endScores;
        
        public GameStateData()
        {
            rockPositions = new List<Vector2Data>();
            rockInPlay = new List<bool>();
            endScores = new List<Vector2IntData>();
        }
    }
    
    [Serializable]
    public class Vector2Data
    {
        public float x;
        public float y;
        
        public Vector2Data() { }
        
        public Vector2Data(Vector2 v)
        {
            x = v.x;
            y = v.y;
        }
        
        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }
    }
    
    [Serializable]
    public class Vector2IntData
    {
        public int x;
        public int y;
        
        public Vector2IntData() { }
        
        public Vector2IntData(Vector2Int v)
        {
            x = v.x;
            y = v.y;
        }
        
        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(x, y);
        }
    }
    
    [Serializable]
    public class TournamentStateData
    {
        public string managerType; // "TournyManager", "PlayoffManager", etc.
        public int draw;
        public int numberOfTeams;
        public int prize;
        public int oppTeam;
        public int playoffRound;
        public List<TeamData> teams;
        public List<Vector2Data> gameList;
        
        // Current tournament info
        public string currentTournamentName;
        public int currentTournamentId;
        public bool isTourEvent;
        public bool isQualifier;
        public bool isChampionship;
        public int prizeMoney;
        public int backgroundId;
        public int crowdDensity;
        
        public TournamentStateData()
        {
            teams = new List<TeamData>();
            gameList = new List<Vector2Data>();
        }
    }
