using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The season tables, and keeping the league's records right. An event is played on copies of the teams once the career
/// has been saved and loaded, so their results have to be copied back into the league when it ends.
/// </summary>
public static class Standings
{
    /// <summary>Money list order: earnings, then wins, then fewer losses, then name.</summary>
    public static int CompareMoney(Team a, Team b)
    {
        int c = b.seasonEarnings.CompareTo(a.seasonEarnings);
        return c != 0 ? c : CompareRecord(a, b);
    }

    /// <summary>Tour order: points, then wins, then fewer losses, then name.</summary>
    public static int CompareTour(Team a, Team b)
    {
        int c = b.tourPoints.CompareTo(a.tourPoints);
        return c != 0 ? c : CompareRecord(a, b);
    }

    static int CompareRecord(Team a, Team b)
    {
        int c = b.seasonWins.CompareTo(a.seasonWins);
        if (c == 0) c = a.seasonLosses.CompareTo(b.seasonLosses);
        return c != 0 ? c : string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
    }

    public static List<Team> Money(IEnumerable<Team> teams) => Sorted(teams, CompareMoney);

    public static List<Team> Tour(IEnumerable<Team> teams) => Sorted(teams, CompareTour);

    static List<Team> Sorted(IEnumerable<Team> teams, Comparison<Team> order)
    {
        var list = teams == null ? new List<Team>() : teams.Where(t => t != null).ToList();
        list.Sort(order);
        return list;
    }

    /// <summary>Where a team stands, counting from 1; 0 if it is not in the table.</summary>
    public static int Place(IList<Team> table, int teamId)
    {
        for (int i = 0; i < table.Count; i++)
            if (table[i].id == teamId) return i + 1;
        return 0;
    }

    /// <summary>
    /// Puts each team's history (wins, losses, earnings, id) back on top of what it did in the event. Matched by id,
    /// because playoffs reorder the event's teams.
    /// </summary>
    public static void AddHistory(Team[] eventTeams, Vector4[] history)
    {
        if (eventTeams == null || history == null) return;
        foreach (var team in eventTeams)
        {
            if (team == null) continue;
            foreach (var h in history)
            {
                if ((int)h.w != team.id) continue;
                team.seasonWins += (int)h.x;
                team.seasonLosses += (int)h.y;
                team.seasonEarnings += h.z;
                break;
            }
        }
    }

    /// <summary>Copies the season figures of the teams that played into the league and the tour list.</summary>
    public static void Merge(Team[] eventTeams, params Team[][] lists)
    {
        if (eventTeams == null) return;
        foreach (var played in eventTeams)
        {
            if (played == null) continue;
            foreach (var list in lists)
            {
                if (list == null) continue;
                foreach (var team in list)
                {
                    if (team == null || team == played || team.id != played.id) continue;
                    team.seasonWins = played.seasonWins;
                    team.seasonLosses = played.seasonLosses;
                    team.seasonEarnings = played.seasonEarnings;
                    team.tourPoints = played.tourPoints;
                }
            }
        }
    }

    /// <summary>Makes the tour list point at the league's own teams, so there is one copy of each record.</summary>
    public static void Link(Team[] league, Team[] tour)
    {
        if (league == null || tour == null) return;
        for (int i = 0; i < tour.Length; i++)
        {
            if (tour[i] == null) continue;
            foreach (var team in league)
            {
                if (team == null || team.id != tour[i].id) continue;
                team.tourPoints = Mathf.Max(team.tourPoints, tour[i].tourPoints);
                tour[i] = team;
                break;
            }
        }
    }
}
