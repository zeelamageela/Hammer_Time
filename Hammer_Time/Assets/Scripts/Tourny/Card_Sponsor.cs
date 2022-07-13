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

    public string[] effects;

    //

}
