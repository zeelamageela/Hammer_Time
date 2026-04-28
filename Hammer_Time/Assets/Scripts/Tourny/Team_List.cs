using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Team_List : IComparable<Team_List>
{
    public Team team;

    public Team_List(Team newTeam)
    {
        team = newTeam;
    }

    public int CompareTo(Team_List other)
    {
        if (other == null)
        {
            return 1;
        }
        // Primary: More wins is better (sort descending)
        else if (team.wins < other.team.wins)
        {
            return 1;
        }
        else if (team.wins > other.team.wins)
        {
            return -1;
        }
        // Tie-breaker 1: Point differential (higher is better)
        else if (team.wins == other.team.wins)
        {
            if (team.pointDifferential > other.team.pointDifferential)
            {
                return -1;
            }
            else if (team.pointDifferential < other.team.pointDifferential)
            {
                return 1;
            }
            // Tie-breaker 2: Player team gets priority
            else
            {
                if (team.player)
                    return -1;
                else
                    return 1;
            }
        }
        else return 0;
    }
}
