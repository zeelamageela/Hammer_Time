using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StoryBlock
{
    public string name;

    public int week;
    public float earnings;
    public bool provQual;
    public bool tourQual;
    public int xp;
    [TextArea(3, 10)]
    public string description;

    public Dialogue[] triggers;

}
