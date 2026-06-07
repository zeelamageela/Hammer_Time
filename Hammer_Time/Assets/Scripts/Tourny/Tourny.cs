using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tourny
{
    public string name;
    public int id;
    public string location;

    public int teams;
    public int prizeMoney;
    public int entryFee;
    public string format;

    public bool championship;
    public bool qualifier;
    public bool tour;
    public bool complete;

    public bool ko3;
    public bool ko1;
    public int BG;
    public int crowdDensity;

    public bool trophyWon;
    public Sprite image;

    [Header("Dialogue")]
    public DialogueData introDialogue;
    public DialogueData topSeedsDialogue;
    public DialogueData bottomSeedsDialogue;
    public DialogueData qfDialogue;
    public DialogueData sfDialogue;
    public DialogueData finalDialogue;
    public DialogueData firstDialogue;
    public DialogueData secondDialogue;
    public DialogueData thirdDialogue;
    public DialogueData notInPlayoffsDialogue;
    public DialogueData loseDialogue;
}
