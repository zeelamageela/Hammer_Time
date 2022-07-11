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

    public GameObject buttons;
    
    Vector3 initialScale;
    Vector3 maxScale;

    public CardDisplay cardDisplay;
    public int cardIndex;

    public Color colour1;
    public Color colour2;
    public Color colour3;
    public Color textColour;
    public Color bgColour;

    int activeCard;
    // Start is called before the first frame update
    void Start()
    {
        pm = FindObjectOfType<PowerUpManager>();
        initialScale = cardSize.localScale;
        maxScale = cardSize.localScale * 2f;
    }

    // Update is called once per frame
    void Update()
    {
        pm.cardDisplays[activeCard].name.color = textColour;
        pm.cardDisplays[activeCard].description.color = textColour;
        //pm.cardDisplays[activeCard].effect.color = textColour;

        image.color = bgColour;
    }

    public void CardSelect()
    {
        //Vector2 cardSizeMax = cardSize.localScale * 2;
        //for (int i = 0; i < 1000; i++)
        //{
        //    cardSize.localScale = cardSize.localScale * (i / 500f);
        //    //Vector2.Lerp(cardSize.localScale, cardSizeMax, i / 1000f);
        //}


        //if (cardSize.localScale == initialScale)
        //{
        //    cardSize.localScale = maxScale; 
        //    textColour = colour3;
        //    bgColour = colour1;
        //    //hlg.spacing = 450;
        //    for (int i = 0; i < pm.cardGOs.Length; i++)
        //    {
        //        if (i != cardIndex)
        //        {
        //            pm.cardGOs[i].GetComponent<Card_Select>().cardSize.localScale = initialScale;
        //            pm.cardGOs[i].GetComponent<Card_Select>().textColour = colour2;
        //            pm.cardGOs[i].GetComponent<Card_Select>().bgColour = colour2;
        //            pm.cardGOs[i].GetComponent<Button>().interactable = false;
        //        }
        //    }
        //    buttons.SetActive(true);
        //    pm.PreviewPoints(cardIndex);
        //}
        //else
        //{
        //    cardSize.localScale = initialScale;
        //    //hlg.spacing = 0;
        //    for (int i = 0; i < pm.cardGOs.Length; i++)
        //    {
        //        pm.cardGOs[i].GetComponent<Card_Select>().cardSize.localScale = initialScale;
        //        pm.cardGOs[i].GetComponent<Card_Select>().textColour = colour3;
        //        pm.cardGOs[i].GetComponent<Card_Select>().bgColour = Color.white;
        //        pm.cardGOs[i].GetComponent<Button>().interactable = true;
        //    }
        //    buttons.SetActive(false);
        //    pm.UnPreviewPoints(cardIndex);
        //}

        //cardSize.sizeDelta = new Vector2(cardSize.sizeDelta.x * 2f, cardSize.sizeDelta.y * 2f);

        buttons.SetActive(true);
        Debug.Log("Size is " + cardSize.localScale);
    }

    public void ViewCard()
    {
        //Vector2 cardSizeMax = cardSize.localScale * 2;
        //for (int i = 0; i < 1000; i++)
        //{
        //    cardSize.localScale = cardSize.localScale * (i / 500f);
        //    //Vector2.Lerp(cardSize.localScale, cardSizeMax, i / 1000f);
        //}
        //if (cardIndex == 0)
        //    pm.scrollbar.value = -2f;
        //else if (cardIndex == pm.numberOfCards - 1f)
        //    pm.scrollbar.value = 2f;
        //else
        //{
        //    pm.scrollbar.value = (float)cardIndex / (pm.numberOfCards - 1);
        //}

        if (cardSize.localScale == initialScale)
        {
            cardSize.localScale = maxScale;
            textColour = colour3;
            bgColour = colour1;
            //hlg.spacing = 450;
            for (int i = 0; i < pm.cardGOs.Length; i++)
            {
                if (i != cardIndex)
                {
                    pm.cardGOs[i].GetComponent<Card_Select>().cardSize.localScale = initialScale;
                    pm.cardGOs[i].GetComponent<Card_Select>().textColour = colour2;
                    pm.cardGOs[i].GetComponent<Card_Select>().bgColour = colour2;
                    pm.cardGOs[i].GetComponent<Button>().interactable = false;
                }
            }
            buttons.SetActive(true);
            //pm.PreviewPoints(cardIndex);
        }
        else
        {
            cardSize.localScale = initialScale;
            //hlg.spacing = 0;
            for (int i = 0; i < pm.cardGOs.Length; i++)
            {
                pm.cardGOs[i].GetComponent<Card_Select>().cardSize.localScale = initialScale;
                pm.cardGOs[i].GetComponent<Card_Select>().textColour = colour3;
                pm.cardGOs[i].GetComponent<Card_Select>().bgColour = colour1;
                pm.cardGOs[i].GetComponent<Button>().interactable = true;
            }
            buttons.SetActive(false);
            //pm.UnPreviewPoints(cardIndex);
        }

    }

    public void BuyButton()
    {
        pm.BuyCard(cardIndex);

        for (int i = 0; i < pm.cardGOs.Length; i++)
        {
            pm.cardGOs[i].GetComponent<Button>().interactable = false;
        }
    }
}
