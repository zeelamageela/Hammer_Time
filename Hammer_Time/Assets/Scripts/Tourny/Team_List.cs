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
        else if (team.wins < other.team.wins)
        {
            return 1;
        }
        else if (team.wins >= other.team.wins)
        {
            return -1;
        }
        else return 0;
    }
}
