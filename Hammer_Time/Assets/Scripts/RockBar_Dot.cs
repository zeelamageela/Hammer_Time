using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RockBar_Dot : MonoBehaviour
{
    public Sprite active;
    public Sprite highlight;
    public Sprite dead;
    public Sprite idle;

    public Image rockImage;
    
    public void ActiveRockSprite()
    {
        GetComponent<Image>().sprite = active;

        GetComponent<RectTransform>().sizeDelta = new Vector2(60f, 60f);
    }

    public void DeadRockSprite()
    {
        GetComponent<Image>().sprite = dead;

        GetComponent<RectTransform>().sizeDelta = new Vector2(60f, 60f);
    }

    public void HighlightRockSprite()
    {
        GetComponent<Image>().sprite = highlight;

        GetComponent<RectTransform>().sizeDelta = new Vector2(49f, 49f);
    }

    public void IdleRockSprite()
    {
        GetComponent<Image>().sprite = idle;

        GetComponent<RectTransform>().sizeDelta = new Vector2(49f, 49f);

        Color col = GetComponent<Image>().color;
        col.a = 0.5f;
        GetComponent<Image>().color = col;
    }
}
