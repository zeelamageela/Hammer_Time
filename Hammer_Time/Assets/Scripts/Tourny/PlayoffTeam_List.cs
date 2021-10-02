using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayoffTeam_List : IComparable<PlayoffTeam_List>
{
    public Team team;

    public PlayoffTeam_List(Team newTeam)
    {
        team = newTeam;
    }

    public int CompareTo(PlayoffTeam_List other)
    {
        if (other == null)
        {
            return 1;
        }
        else if (team.rank < other.team.rank)
        {
            return 1;
        }
        else if (team.rank >= other.team.rank)
        {
            return -1;
        }
        else return 0;
    }
}
