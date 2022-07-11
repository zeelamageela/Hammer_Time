using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CardDisplay
{
    public Text name;
    public Text description;
    public Slider[] effectSliders;
    public GameObject costPanel;
    public Text cost;
    public Image image;
}
