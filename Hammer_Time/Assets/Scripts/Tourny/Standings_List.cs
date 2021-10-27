using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Standings_List : IComparable<Standings_List>
{
    public Team team;

    public Standings_List(Team newTeam)
    {
        team = newTeam;
    }

    public int CompareTo(Standings_List other)
    {
        if (other == null)
        {
            return 1;
        }
        else if (team.wins < other.team.wins)
        {
            return 1;
        }
        else if (team.wins > other.team.wins)
        {
            return -1;
        }
        else if (team.wins == other.team.wins)
        {
            return 0;
        }
        else return 0;
    }
}
