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
    [TextArea(3, 10)]
    public string effect;

    public int draw;
    public int guard;
    public int takeOut;
    public int sweepStrength;
    public int sweepEnduro;
    public int sweepHealth;

    public int oppDraw;
    public int oppGuard;
    public int oppTakeOut;
    public int oppStrength;
    public int oppEnduro;
    public int oppHealth;
}
