using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Team
{
    public string name;

    public int wins;
    public int loss;
    public int rank;
    public string nextOpp;

    public int id;
    public float earnings;

    public Vector2 record;
    public Vector2 tourRecord;
    public float tourPoints;

    public bool player;

    // List of players on the team
    public List<Player> players = new List<Player>();

    // --- Six skill categories (team average) ---
    public int draw;
    public int takeOut;
    public int guard;
    public int sweepStrength;
    public int sweepEnduro;
    public int sweepCohesion;

    // --- Computed overall strength (legacy support) ---
    public int strength
    {
        get { return Mathf.RoundToInt(CalculateStrength()); }
        set
        {
            draw = value;
            takeOut = value;
            guard = value;
            sweepStrength = value;
            sweepEnduro = value;
            sweepCohesion = value;
        }
    }

    // --- Weighted skill calculation ---
    public float CalculateStrength()
    {
        UpdateTeamSkillsFromPlayers();
        return 0.2f * draw +
               0.2f * takeOut +
               0.15f * guard +
               0.15f * sweepStrength +
               0.15f * sweepEnduro +
               0.15f * sweepCohesion;
    }

    // --- Calculate team skills as average of player skills ---
    public void UpdateTeamSkillsFromPlayers()
    {
        if (players == null || players.Count == 0)
            return;

        float totalDraw = 0, totalTakeOut = 0, totalGuard = 0, totalSweepStrength = 0, totalSweepEnduro = 0, totalSweepCohesion = 0;
        foreach (var p in players)
        {
            totalDraw += p.draw;
            totalTakeOut += p.takeOut;
            totalGuard += p.guard;
            totalSweepStrength += p.sweepStrength;
            totalSweepEnduro += p.sweepEnduro;
            totalSweepCohesion += p.sweepCohesion;
        }
        int count = players.Count;
        draw = Mathf.RoundToInt(totalDraw / count);
        takeOut = Mathf.RoundToInt(totalTakeOut / count);
        guard = Mathf.RoundToInt(totalGuard / count);
        sweepStrength = Mathf.RoundToInt(totalSweepStrength / count);
        sweepEnduro = Mathf.RoundToInt(totalSweepEnduro / count);
        sweepCohesion = Mathf.RoundToInt(totalSweepCohesion / count);
    }
}
