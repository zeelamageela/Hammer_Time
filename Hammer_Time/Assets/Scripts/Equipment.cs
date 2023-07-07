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
    public Color color;

    public bool active;
    public bool owned;

    public int duration;

    public int[] stats = new int[6];
    public int[] oppStats = new int[6];

}
