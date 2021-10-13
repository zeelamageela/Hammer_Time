using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tourny
{
    public int id;
    public string location;
    public string name;

    public int teams;
    public int prizeMoney;
    public int entryFee;
    public string format;

    public bool championship;
    public bool qualifier;
    public bool tour;
    public bool complete;
}
