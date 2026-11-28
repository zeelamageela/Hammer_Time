using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A local club night: four teams, everyone plays everyone once, no playoffs, and the best record takes the whole pot.
/// Ties on wins go to point difference, then points scored; teams still level share the pot.
///
/// One is on offer every week at the player's own club. The "locals" list on the career's event selector
/// (Arena_Selector) supplies the look and the fee, taking turns. An entry there only needs a name,
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

    /// <summary>
    /// Who plays: the player and others from the player's club, picked at random. A career with no club (started before
    /// clubs were added), or a club short of members, is filled from the rest of the league.
    /// </summary>
    public static Team[] Field(IList<Team> league, int playerId, int size, Func<int, int> next)
    {
        Team player = null;
        var others = new List<Team>();
        foreach (var t in league)
        {
            if (t == null) continue;
            if (t.id == playerId) player = t;
            else others.Add(t);
        }
        for (int i = others.Count - 1; i > 0; i--)
        {
            int j = next(i + 1);
            (others[i], others[j]) = (others[j], others[i]);
        }

        var field = new List<Team>();
        if (player != null) field.Add(player);
        bool hasClub = player != null && !string.IsNullOrEmpty(player.club);
        for (int pass = hasClub ? 0 : 1; pass < 2; pass++)
            foreach (var t in others)
            {
                if (field.Count >= size) break;
                bool member = hasClub && string.Equals(t.club, player.club, StringComparison.OrdinalIgnoreCase);
                if ((pass == 0 ? member : true) && !field.Contains(t)) field.Add(t);
            }
        return field.ToArray();
    }

    /// <summary>
    /// Which of the selector's three slots takes the club night: the first empty one (left, right, then middle), or the
    /// right-hand one if all three are taken. That event stays unplayed and comes up again.
    /// </summary>
    public static int Slot(bool leftEmpty, bool middleEmpty, bool rightEmpty)
    {
        return leftEmpty ? 0 : rightEmpty ? 2 : middleEmpty ? 1 : 2;
    }

    /// <summary>Names the event after the player's club. Careers without one keep the event's own name.</summary>
    public static void At(Tourny t, string club)
    {
        if (t == null || string.IsNullOrWhiteSpace(club)) return;
        t.name = club + " Club Night";
        t.location = club;
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
