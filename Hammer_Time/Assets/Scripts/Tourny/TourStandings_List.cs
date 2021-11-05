using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TourStandings_List : IComparable<TourStandings_List>
{
    public Team team;

    public TourStandings_List(Team newTeam)
    {
        team = newTeam;
    }

    public int CompareTo(TourStandings_List other)
    {
        if (other == null)
        {
            return 1;
        }
        else if (team.tourPoints < other.team.tourPoints)
        {
            return 1;
        }
        else if (team.tourPoints > other.team.tourPoints)
        {
            return -1;
        }
        else if (team.tourPoints == other.team.tourPoints)
        {
            return 0;
        }
        else return 0;
    }
}
