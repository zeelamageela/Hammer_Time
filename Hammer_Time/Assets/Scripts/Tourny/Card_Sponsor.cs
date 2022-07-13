using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Card_Sponsor
{
    public string name;

    public int id;
    [TextArea(3, 10)]
    public string description;

    public float cost;
    public bool active;
    public bool played;

    public int duration;

    public int draw;
    public int guard;
    public int takeOut;
    public int sweepStrength;
    public int sweepEnduro;
    public int sweepCohesion;

    public int oppDraw;
    public int oppGuard;
    public int oppTakeOut;
    public int oppStrength;
    public int oppEnduro;
    public int oppCohesion;

    public string signCondition;
    public string signConditionValue;

    public string bonusCondition;
    public string bonusConditionValue;

    public float bonus;

    public string bonusStart;
    //signing condition
    //the condition that this will become available, ie win threshold or qualification threshold

    //bonus condition
    //the condition for paying a bonus, ie won 3 games, won a tournament
    

}
