using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FreeGuard_List : IComparable<FreeGuard_List>
{
    public int rockIndex;
    public bool freeGuard;
    public Transform lastTransform;

    public FreeGuard_List(int newRockIndex, bool newFreeGuard, Transform newLastTransform)
    {
        rockIndex = newRockIndex;
        freeGuard = newFreeGuard;
        lastTransform = newLastTransform;
    }

    public int CompareTo(FreeGuard_List other)
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
