using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stat
{
    [SerializeField]
    int baseValue;

    public int GetValue()
    {
        return baseValue;
    }

    public int SetBaseValue(int newValue)
    {
        baseValue = newValue;
        return baseValue;
    }
}
