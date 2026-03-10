using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Team
{
    public string name;
    public int id;
    public bool player;
    
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
    
    // TOUR CUMULATIVE (across entire season)
    public float tourPoints;
    
    // LEGACY COMPATIBILITY (kept for backwards compatibility during transition)
    // These now map to seasonWins/seasonLosses for existing code
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
    
    // DEPRECATED: Use seasonWins/seasonLosses instead
    [System.Obsolete("Use seasonWins/seasonLosses instead")]
    public Vector2 record
    {
        get { return new Vector2(seasonWins, seasonLosses); }
        set { seasonWins = (int)value.x; seasonLosses = (int)value.y; }
    }
    
    // DEPRECATED: Use tourPoints directly
    [System.Obsolete("Use tourPoints directly")]
    public Vector2 tourRecord;  // Keeping for save compatibility

    // List of players on the team
    public List<Player> players = new List<Player>();

    // --- Six skill categories (team average) ---
    public int weight;
    public int aim;
    public int finesse;
    public int sweepStrength;
    public int sweepEnduro;
    public int sweepCohesion;

    // --- Computed overall strength (legacy support) ---
    public int strength
    {
        get { return Mathf.RoundToInt(CalculateStrength()); }
        set
        {
            weight = value;
            aim = value;
            finesse = value;
            sweepStrength = value;
            sweepEnduro = value;
            sweepCohesion = value;
        }
    }

    // --- Weighted skill calculation ---
    public float CalculateStrength()
    {
        UpdateTeamSkillsFromPlayers();
        return 0.2f * weight +
               0.2f * aim +
               0.15f * finesse +
               0.15f * sweepStrength +
               0.15f * sweepEnduro +
               0.15f * sweepCohesion;
    }

    // --- Calculate team skills as average of player skills ---
    public void UpdateTeamSkillsFromPlayers()
    {
        if (players == null || players.Count == 0)
            return;

        float totalWeight = 0, totalAim = 0, totalFinesse = 0, totalSweepStrength = 0, totalSweepEnduro = 0, totalSweepCohesion = 0;
        foreach (var p in players)
        {
            totalWeight += p.weight;
            totalAim += p.aim;
            totalFinesse += p.finesse;
            totalSweepStrength += p.sweepStrength;
            totalSweepEnduro += p.sweepEnduro;
            totalSweepCohesion += p.sweepCohesion;
        }
        int count = players.Count;
        weight = Mathf.RoundToInt(totalWeight / count);
        aim = Mathf.RoundToInt(totalAim / count);
        finesse = Mathf.RoundToInt(totalFinesse / count);
        sweepStrength = Mathf.RoundToInt(totalSweepStrength / count);
        sweepEnduro = Mathf.RoundToInt(totalSweepEnduro / count);
        sweepCohesion = Mathf.RoundToInt(totalSweepCohesion / count);
    }
    
    // --- Tournament Stats Management ---
    
    /// <summary>
    /// Reset tournament-specific stats at the start of a new tournament
    /// </summary>
    public void StartTournament()
    {
        tournamentWins = 0;
        tournamentLosses = 0;
        tournamentEarnings = 0;
        rank = 0;
        nextOpp = "";
    }
    
    /// <summary>
    /// Add tournament results to season cumulative stats
    /// </summary>
    public void CompleteTournament()
    {
        seasonWins += tournamentWins;
        seasonLosses += tournamentLosses;
        seasonEarnings += tournamentEarnings;
    }
    
    /// <summary>
    /// Reset season stats at the start of a new season
    /// </summary>
    public void StartNewSeason()
    {
        seasonWins = 0;
        seasonLosses = 0;
        seasonEarnings = 0;
        tourPoints = 0;
        StartTournament();
    }
}
