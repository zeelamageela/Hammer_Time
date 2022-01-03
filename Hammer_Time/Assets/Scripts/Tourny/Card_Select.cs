using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card_Select : MonoBehaviour
{
    public PowerUpManager pm;
    public RectTransform cardSize;
    public Image image;
    public HorizontalLayoutGroup hlg;

    public Scrollbar scrollbar;

    Vector3 initialScale;
    Vector3 maxScale;

    public Color colour1;
    public Color colour2;
    public Color colour3;
    public Color textColour;
    public Color bgColour;

    int activeCard;
    // Start is called before the first frame update
    void Start()
    {
        initialScale = cardSize.localScale;
        maxScale = cardSize.localScale * 2f;
    }

    // Update is called once per frame
    void Update()
    {
            pm.cardDisplays[activeCard].name.color = textColour;
            pm.cardDisplays[activeCard].description.color = textColour;
            pm.cardDisplays[activeCard].effect.color = textColour;

        image.color = bgColour;
    }

    public void CardSelect(int card)
    {
        //Vector2 cardSizeMax = cardSize.localScale * 2;
        //for (int i = 0; i < 1000; i++)
        //{
        //    cardSize.localScale = cardSize.localScale * (i / 500f);
        //    //Vector2.Lerp(cardSize.localScale, cardSizeMax, i / 1000f);
        //}
        if (cardSize.localScale == initialScale)
        {
            cardSize.localScale = maxScale;
            textColour = colour2;
            bgColour = colour2;
            //hlg.spacing = 450;
            for (int i = 0; i < pm.cardGOs.Length; i++)
            {
                if (i != card)
                    pm.cardGOs[i].GetComponent<Card_Select>().cardSize.localScale = initialScale;
            }
            pm.AssignPoints(card);
        }
        else
        {
            cardSize.localScale = initialScale;
            textColour = colour3;
            bgColour = Color.white;
            //hlg.spacing = 0;
        }

        switch (card)
        {
            case 0:
                scrollbar.value = 0f;
                break;
            case 1:
                scrollbar.value = 0.5f;
                break;
            case 2:
                scrollbar.value = 1f;
                break;
        }
        //cardSize.sizeDelta = new Vector2(cardSize.sizeDelta.x * 2f, cardSize.sizeDelta.y * 2f);

        Debug.Log("Size is " + cardSize.localScale);
    }

}
