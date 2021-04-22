using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Rock_List : IComparable<Rock_List>
{
    public GameObject rock;
    public Rock_Info rockInfo;

    public Rock_List(GameObject newRock, Rock_Info newRockInfo)
    {
        rock = newRock;
        rockInfo = newRockInfo;
    }

    public int CompareTo(Rock_List other)
    {
        if (other == null)
        {
            return 1;
        }
        else if (rockInfo.rockNumber <= other.rockInfo.rockNumber)
        {
            return -1;
        }
        else if (rockInfo.rockNumber > other.rockInfo.rockNumber)
        {
            return 1;
        }
        else return 0;
    }
}
