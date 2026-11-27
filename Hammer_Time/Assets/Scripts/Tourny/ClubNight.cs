using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A local club night: four teams, everyone plays everyone once, no playoffs, and the best record takes the whole pot.
/// Ties on wins go to point difference, then points scored; teams still level share the pot.
///
/// The events are the "locals" list on the career's event selector (Arena_Selector). An entry there only needs a name,
/// location, entry fee, background, crowd and picture: the rest is set here. A prize of 0 means the pot is the four entry fees.
/// </summary>
public static class ClubNight
{
    public const int Teams = 4;
    public const int Draws = 3;
    public const string Format = "Club Night";

    /// <summary>Fills in what is the same for every club night.</summary>
    public static void Prepare(Tourny t)
    {
        if (t == null) return;
        t.clubNight = true;
        t.teams = Teams;
        t.format = Format;
        if (t.prizeMoney <= 0) t.prizeMoney = Teams * Mathf.Max(0, t.entryFee);
        t.championship = t.qualifier = t.tour = t.ko1 = t.ko3 = false;
    }

    /// <summary>Which club night is on offer in a week: they take turns, in list order.</summary>
    public static int Pick(int week, int count)
    {
        return count > 0 ? ((week % count) + count) % count : 0;
    }

    public static DrawFormat[] Schedule()
    {
        return new[]
        {
            new DrawFormat { game = new[] { new Vector2Int(0, 1), new Vector2Int(2, 3) } },
            new DrawFormat { game = new[] { new Vector2Int(0, 2), new Vector2Int(1, 3) } },
            new DrawFormat { game = new[] { new Vector2Int(0, 3), new Vector2Int(1, 2) } },
        };
    }

    /// <summary>Adds one game's score to both teams' tie-break figures (wins and losses are counted elsewhere).</summary>
    public static void AddScore(Team a, Team b, int scoreA, int scoreB)
    {
        a.pointsFor += scoreA;
        b.pointsFor += scoreB;
        a.pointDifferential += scoreA - scoreB;
        b.pointDifferential += scoreB - scoreA;
    }

    /// <summary>A score for a simulated game that fits the winner already decided. range(min, max) excludes max.</summary>
    public static void SimScore(bool aWon, Func<int, int, int> range, out int scoreA, out int scoreB)
    {
        int loser = range(1, 6);
        int winner = loser + range(1, 5);
        scoreA = aWon ? winner : loser;
        scoreB = aWon ? loser : winner;
    }

    /// <summary>Whether two teams finished exactly level.</summary>
    public static bool Level(Team a, Team b)
    {
        return a.wins == b.wins && a.pointDifferential == b.pointDifferential && a.pointsFor == b.pointsFor;
    }

    /// <summary>The team or teams that finished first.</summary>
    public static List<Team> Winners(IList<Team> teams)
    {
        var winners = new List<Team>();
        foreach (var t in teams)
        {
            if (winners.Count > 0)
            {
                Team best = winners[0];
                if (Level(t, best)) { winners.Add(t); continue; }
                bool better = t.wins != best.wins ? t.wins > best.wins
                    : t.pointDifferential != best.pointDifferential ? t.pointDifferential > best.pointDifferential
                    : t.pointsFor > best.pointsFor;
                if (!better) continue;
                winners.Clear();
            }
            winners.Add(t);
        }
        return winners;
    }

    /// <summary>What the player's team collects from the pot.</summary>
    public static float Share(float pot, IList<Team> winners)
    {
        foreach (var t in winners)
            if (t.player) return Mathf.Floor(pot / winners.Count);
        return 0f;
    }
}
