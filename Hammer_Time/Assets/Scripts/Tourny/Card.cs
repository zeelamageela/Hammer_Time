using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Card
{
    public string name;

    public int id;
    [TextArea(3, 10)]
    public string description;

    public Sprite image;

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

    public int[] stats;
    public int[] oppStats;

    //signing condition
    //the condition that this will become available, ie win threshold or qualification threshold
    public string signCondition;
    public int signConditionValue;

    //bonus condition
    //the condition for paying a bonus, ie won 3 games, won a tournament
    public string bonusCondition;
    public int bonusConditionValue;

    public float bonus;

    public string bonusStart;

    void Update()
    {
        stats[0] = draw;
        stats[1] = guard;
        stats[2] = takeOut;
        stats[3] = sweepStrength;
        stats[4] = sweepEnduro;
        stats[5] = sweepCohesion;

        oppStats[0] = oppDraw;
        oppStats[1] = oppGuard;
        oppStats[2] = oppTakeOut;
        oppStats[3] = oppStrength;
        oppStats[4] = oppEnduro;
        oppStats[5] = oppCohesion;
    }
}
