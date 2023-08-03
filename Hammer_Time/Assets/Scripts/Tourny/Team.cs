using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Team
{
    public string name;

    public int wins;
    public int loss;
    public int rank;
    public string nextOpp;

    public int strength;
    public int id;

    public float earnings;

    public Vector2 record;
    public Vector2 tourRecord;
    public float tourPoints;

    public bool player;
}
