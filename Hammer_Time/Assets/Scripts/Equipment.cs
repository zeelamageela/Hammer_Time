using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Equipment
{
    public string name;
    public bool footwear;
    public bool apparel;
    public bool head;
    public bool handle;

    public int id;

    public Sprite img;
    public float cost;
    public string text;

    public bool active;
    public bool owned;

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
