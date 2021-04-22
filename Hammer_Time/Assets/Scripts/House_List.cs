using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class House_List : IComparable<House_List>
{
    public GameObject rock;
    public Rock_Info rockInfo;

    public House_List(GameObject newRock, Rock_Info newRockInfo)
    {
        rock = newRock;
        rockInfo = newRockInfo;
    }

    public int CompareTo(House_List other)
    {
        if (other == null)
        {
            return 1;
        }
        else if (rockInfo.distance <= other.rockInfo.distance)
        {
            return -1;
        }
        else if (rockInfo.distance > other.rockInfo.distance)
        {
            return 1;
        }
        else return 0;

        //return distance.CompareTo(other.distance);
    }
}
