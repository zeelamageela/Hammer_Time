using System;
using System.Collections.Generic;

/// <summary>
/// High Score Save Data - Persistent leaderboard independent of career saves
/// Stores all-time best performances across multiple careers
/// </summary>
[Serializable]
public class HighScoreData
{
    public int version = 1;
    public string lastUpdated;
    public List<HighScoreEntry> entries = new List<HighScoreEntry>();
    public List<bool> allTimeTrophies = new List<bool>(); // Trophy cabinet across all careers
    
    public HighScoreData()
    {
        lastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}

/// <summary>
/// Individual high score entry
/// </summary>
[Serializable]
public class HighScoreEntry
{
    public string playerName;
    public string teamName;
    public float earnings;
    public int season;
    public int wins;
    public int losses;
    public string dateAchieved;
    
    public HighScoreEntry(string playerName, string teamName, float earnings, int season, int wins, int losses)
    {
        this.playerName = playerName;
        this.teamName = teamName;
        this.earnings = earnings;
        this.season = season;
        this.wins = wins;
        this.losses = losses;
        this.dateAchieved = DateTime.Now.ToString("yyyy-MM-dd");
    }
}
