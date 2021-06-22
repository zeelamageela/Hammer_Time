using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Guard_List : IComparable<Guard_List>
{
    public int rockIndex;
    public bool freeGuard;
    public Transform lastTransform;

    public Guard_List(int newRockIndex, bool newFreeGuard, Transform newLastTransform)
    {
        rockIndex = newRockIndex;
        freeGuard = newFreeGuard;
        lastTransform = newLastTransform;
    }

    public int CompareTo(Guard_List other)
    {
        if(other == null)
        {
            return 1;
        }
        else if (rockIndex < other.rockIndex)
        {
            return -1;
        }
        else if (rockIndex >= other.rockIndex)
        {
            return 1;
        }
        else return 0;
    }
}
