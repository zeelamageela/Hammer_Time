using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpManager : MonoBehaviour
{
    CareerManager cm;
    public TournySelector tSel;

    public Color[] bgColors;
    public Color[] textColors;

    public int[] idList;
    public int[] activeIdList;
    public int[] usedIdList;

    public int[] activeLengthList;
    public int[] usedLengthList;

    public Card[] cards;
    public List<Card> availCards;
    public Card[] playerCards;
    public List<Card> usedCards;
    public Card emptyCard;
    //public List<Card> playerCards;

    public CardDisplay[] cardDisplays;
    public GameObject[] cardGOs;

    public GameObject replacePanel;
    public GameObject viewPanel;

    public Text replaceHeading;

    public PowerUpList pUpList;

    public float xp;
    public float cash;
    public Text xpText;
    public Text cashText;
    public GameObject profileGO;
    public Text profileName;
    public Button profileButton;

    public int numberOfCards;

    public Slider drawSlider;
    public Slider guardSlider;
    public Slider takeOutSlider;
    public Slider strengthSlider;
    public Slider enduranceSlider;
    public Slider healthSlider;

    public Slider oppDrawSlider;
    public Slider oppGuardSlider;
    public Slider oppTakeOutSlider;
    public Slider oppStrengthSlider;
    public Slider oppEnduranceSlider;
    public Slider oppHealthSlider;

    public float costPerWeek;

    int selectedPlayerCard;
    bool drag;
    int oppStatBase;
    int clickCount;

    int colorChanger;

    CharacterStats[] cStats;

    // Start is called before the first frame update
    void Start()
    {
        clickCount = 0;
        cm = FindObjectOfType<CareerManager>();
        tSel = FindObjectOfType<TournySelector>();
        SetUp();
    }

    private void Update()
    {
        
    }

    //public void BuyCard(int card)
    //{
    //    cm = FindObjectOfType<CareerManager>();

    //    cm.earnings -= availCards[card].cost;
    //    availCards[card].active = true;

    //    Debug.Log("Buying " + availCards[card].name + " for $" + availCards[card].cost + ", money left is " + cm.earnings);

    //    AssignPoints(card);
    //}

    public void Continue(bool back)
    {
        if (!back)
        {
            //cm.SaveCareer();
            tSel.SetUp();

        }
        profileButton.interactable = true;
        gameObject.SetActive(false);

    }

    IEnumerator WaitForClick()
    {
        tSel = FindObjectOfType<TournySelector>();

        //yield return new WaitForSeconds(3f);
        tSel.SetUp();
        profileButton.interactable = true;
        gameObject.SetActive(false);
        yield return null;
        Debug.Log("Clickeddd");
    }

    public void AssignPoints(int card)
    {
        cm = FindObjectOfType<CareerManager>();

        //contButton.SetActive(true);
        //infoPanel.SetActive(false);

        idList[cm.week] = availCards[card].id;
        Debug.Log("id of played card is " + idList[cm.week]);
        Debug.Log("availcards length is " + availCards.Count);
        //int idListLength = 0;
        //cm.SaveCareer();
        //for (int i = 0; i < numberOfCards; i++)
        //{
        //    if (availCards[i].active)
        //        idList[cm.week] = availCards[i].id;
        //}
    }

    public void PreviewPoints(int card)
    {
        cm = FindObjectOfType<CareerManager>();

        cm.modStats.drawAccuracy += playerCards[card].draw;
        cm.modStats.guardAccuracy += playerCards[card].guard;
        cm.modStats.takeOutAccuracy += playerCards[card].takeOut;
        cm.modStats.sweepEndurance += playerCards[card].sweepEnduro;
        cm.modStats.sweepStrength += playerCards[card].sweepStrength;
        cm.modStats.sweepCohesion += playerCards[card].sweepCohesion;
        cm.oppStats.drawAccuracy += playerCards[card].oppDraw;
        cm.oppStats.guardAccuracy += playerCards[card].oppGuard;
        cm.oppStats.takeOutAccuracy += playerCards[card].oppTakeOut;
        cm.oppStats.sweepEndurance += playerCards[card].oppEnduro;
        cm.oppStats.sweepStrength += playerCards[card].oppStrength;
        cm.oppStats.sweepCohesion += playerCards[card].oppCohesion;

    }

    public void UnPreviewPoints(int card)
    {
        cm = FindObjectOfType<CareerManager>();
        //DisplayCards(numberOfCards);
        cm.modStats.drawAccuracy -= playerCards[card].draw;
        cm.modStats.guardAccuracy -= playerCards[card].guard;
        cm.modStats.takeOutAccuracy -= playerCards[card].takeOut;
        cm.modStats.sweepEndurance -= playerCards[card].sweepEnduro;
        cm.modStats.sweepStrength -= playerCards[card].sweepStrength;
        cm.modStats.sweepCohesion -= playerCards[card].sweepCohesion;
        cm.oppStats.drawAccuracy -= playerCards[card].oppDraw;
        cm.oppStats.guardAccuracy -= playerCards[card].oppGuard;
        cm.oppStats.takeOutAccuracy -= playerCards[card].oppTakeOut;
        cm.oppStats.sweepEndurance -= playerCards[card].oppEnduro;
        cm.oppStats.sweepStrength -= playerCards[card].oppStrength;
        cm.oppStats.sweepCohesion -= playerCards[card].oppCohesion;
    }

    public void ViewCards()
    {
        //DisplayCards(cards.Length);
    }

    void DisplayCards(int numOfCards)
    {
        int counter = 0;
        Debug.Log("Num of Cards " + numOfCards);
        Debug.Log("availCards " + availCards.Count);

        for (int i = 0; i < numOfCards; i++)
        {
            cardDisplays[i].name.text = availCards[i].name;
            cardDisplays[i].description.text = availCards[i].description;
            cardDisplays[i].cost.text = "$" + availCards[i].cost.ToString("n0");

            cardDisplays[i].effectSliders[0].value = playerCards[i].draw;
            cardDisplays[i].effectSliders[1].value = playerCards[i].guard;
            cardDisplays[i].effectSliders[2].value = playerCards[i].takeOut;
            cardDisplays[i].effectSliders[3].value = playerCards[i].sweepStrength;
            cardDisplays[i].effectSliders[4].value = playerCards[i].sweepEnduro;
            cardDisplays[i].effectSliders[5].value = playerCards[i].sweepCohesion;

            cardDisplays[i].effectSliders[6].value = playerCards[i].oppDraw;
            cardDisplays[i].effectSliders[7].value = playerCards[i].oppGuard;
            cardDisplays[i].effectSliders[8].value = playerCards[i].oppTakeOut;
            cardDisplays[i].effectSliders[9].value = playerCards[i].oppStrength;
            cardDisplays[i].effectSliders[10].value = playerCards[i].oppEnduro;
            cardDisplays[i].effectSliders[11].value = playerCards[i].oppCohesion;

            if (availCards[i].active)
            {
                cardDisplays[i].costPanel.SetActive(false);
                counter++;
            }
        }

        for (int i = 0; i < numOfCards; i++)
        {
            if (cm.earnings < availCards[i].cost && !availCards[i].active)
            {
                cardGOs[i].GetComponent<Card_Select>().textColour = cardGOs[i].GetComponent<Card_Select>().colour3;
                cardGOs[i].GetComponent<Card_Select>().bgColour = cardGOs[i].GetComponent<Card_Select>().colour2;
                cardGOs[i].GetComponent<Button>().interactable = false;
                counter++;
            }
        }

        //if (counter >= numberOfCards - 1)
        //{
        //    contButton.SetActive(true);
        //    infoPanel.SetActive(false);
        //}
    }

    void Shuffle(Card[] a)
    {
        // Loops through array
        for (int i = a.Length - 1; i > 0; i--)
        {
            // Randomize a number between 0 and i (so that the range decreases each time)
            int rnd = Random.Range(0, i);

            // Save the value of the current i, otherwise it'll overright when we swap the values
            Card temp = a[i];

            // Swap the new and old values
            a[i] = a[rnd];
            a[rnd] = temp;
        }
    }

    void ColorChanger(int colorMode, int card)
    {
        cardDisplays[card].name.color = textColors[colorMode];
        cardDisplays[card].description.color = textColors[colorMode];
        cardGOs[card].GetComponent<Image>().color = bgColors[colorMode];

        switch (colorMode)
        {
            case 0:
                cardDisplays[card].effectSliders[0].transform.parent.gameObject.SetActive(true);
                break;
            case 1:
                cardDisplays[card].effectSliders[0].transform.parent.gameObject.SetActive(true);
                break;
            case 2:
                cardDisplays[card].effectSliders[0].transform.parent.gameObject.SetActive(false);
                break;
        }
    }

    void SetUp()
    {
        cards = new Card[pUpList.powerUps.Length];
        availCards = new List<Card>();
        if (cm.cardIDList.Length > 0)
        {
            idList = cm.cardIDList;
        }
        else
        {
            idList = new int[cards.Length];
            Shuffle(pUpList.powerUps);

            for (int i = 0; i < idList.Length; i++)
            {
                idList[i] = pUpList.powerUps[i].id;
                cards[i] = pUpList.powerUps[i];
            }
        }

        if (cm.activeCardIDList != null && cm.activeCardIDList.Length > 0)
        {
            activeIdList = cm.activeCardIDList;
            activeLengthList = cm.activeCardLengthList;
        }
        else
        {
            activeIdList = new int[4];
            activeLengthList = new int[4];

            for (int i = 0; i < activeIdList.Length; i++)
            {
                activeIdList[i] = 99;
                activeLengthList[i] = 100;
            }
        }

        Debug.Log("activeList Length is " + activeIdList.Length);

        if (cm.usedCardIDList != null)
        {
            usedIdList = cm.usedCardIDList;
        }

        Shuffle(cards);

        for (int i = 0; i < idList.Length; i++)
        {
            for (int j = 0; j < pUpList.powerUps.Length; j++)
            {
                if (pUpList.powerUps[j].id == idList[i])
                {
                    cards[i] = pUpList.powerUps[j];
                }
            }
        }

        Shuffle(cards);

        for (int i = 0; i < activeIdList.Length; i++)
        {
            if (activeIdList[i] == 99)
            {
                playerCards[i] = emptyCard;
            }
            else
            {
                for (int j = 0; j < cards.Length; j++)
                {
                    if (cards[j].id == activeIdList[i])
                    {
                        cards[j].length--;

                        Debug.Log(cards[j].name + " is " + cards[j].length + " long");
                        if (cards[j].length > 0)
                        {
                            cards[j].played = true;
                            playerCards[i] = cards[j];

                            ColorChanger(0, i);
                            //playerCards[i].active = true;
                        }
                        else
                        {
                            cards[j].active = false;
                            cards[j].played = true;
                            playerCards[i] = emptyCard;
                            ColorChanger(2, i);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < usedIdList.Length; i++)
        {
            for (int j = 0; j < cards.Length; j++)
            {
                if (cards[j].id == usedIdList[i])
                {
                    cards[j].played = true;
                    usedCards.Add(cards[j]);
                }
            }
        }

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i].active != true)
            {
                availCards.Add(cards[i]);
            }
        }


        Debug.Log("AvailCards Length is " + availCards.Count);

        for (int i = 0; i < (playerCards.Length / 2f); i++)
        {
            //if (playerCards[i].active)
            //{
            //    //cardGOs[i].GetComponent<Card_Select>().textColour = cardGOs[i].GetComponent<Card_Select>().colour3;
            //    //cardGOs[i].GetComponent<Card_Select>().bgColour = cardGOs[i].GetComponent<Card_Select>().colour2;
            //    cardDisplays[i].costPanel.SetActive(false);
            //    cardGOs[i].GetComponent<Button>().interactable = true;
            //}
            //else
            //{
            //    cardGOs[i].GetComponent<Card_Select>().textColour = cardGOs[i].GetComponent<Card_Select>().colour3;
            //    cardGOs[i].GetComponent<Card_Select>().bgColour = cardGOs[i].GetComponent<Card_Select>().colour2;
            //    cardGOs[i].GetComponent<Button>().interactable = false;
            //}
        }

        for (int i = 0; i < (cardGOs.Length / 2f); i++)
        {
            cardDisplays[i].name.text = playerCards[i].name + " - " + playerCards[i].length.ToString();
            cardDisplays[i].description.text = playerCards[i].description;
            cardDisplays[i].cost.text = "$" + playerCards[i].cost.ToString("n0");

            cardDisplays[i].effectSliders[0].value = playerCards[i].draw;
            cardDisplays[i].effectSliders[1].value = playerCards[i].guard;
            cardDisplays[i].effectSliders[2].value = playerCards[i].takeOut;
            cardDisplays[i].effectSliders[3].value = playerCards[i].sweepStrength;
            cardDisplays[i].effectSliders[4].value = playerCards[i].sweepEnduro;
            cardDisplays[i].effectSliders[5].value = playerCards[i].sweepCohesion;

            cardDisplays[i].effectSliders[6].value = playerCards[i].oppDraw;
            cardDisplays[i].effectSliders[7].value = playerCards[i].oppGuard;
            cardDisplays[i].effectSliders[8].value = playerCards[i].oppTakeOut;
            cardDisplays[i].effectSliders[9].value = playerCards[i].oppStrength;
            cardDisplays[i].effectSliders[10].value = playerCards[i].oppEnduro;
            cardDisplays[i].effectSliders[11].value = playerCards[i].oppCohesion;

            if (availCards[i].active)
            {
                cardDisplays[i].costPanel.SetActive(false);
            }
        }

        for (int i = 0; i < playerCards.Length; i++)
        {
            costPerWeek += playerCards[i].cost;
        }

        Debug.Log("costPerWeek is " + costPerWeek);

        for (int i = 0; i < playerCards.Length; i++)
        {
            if (playerCards[i].id == 99)
                ColorChanger(2, i);
            else
                ColorChanger(0, i);

            PreviewPoints(i);

            for (int j = 0; j < cardDisplays[i].effectSliders.Length; j++)
            {
                if (cardDisplays[i].effectSliders[j].value < 0)
                {
                    cardDisplays[i].effectSliders[j].value *= -1f;
                    cardDisplays[i].effectSliders[j].gameObject.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().color = Color.red;
                }
                else
                {
                    if (j < 6)
                        cardDisplays[i].effectSliders[j].gameObject.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().color = bgColors[1];
                    else
                        cardDisplays[i].effectSliders[j].gameObject.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().color = bgColors[3];
                }
            }

            for (int j = 0; j < cardDisplays[i].effectSliders.Length; j++)
            {
                cardDisplays[i].effectSliders[j].minValue = 0f;
            }
        }
    }

    public void CardClick(int card)
    {
        clickCount++;
        Debug.Log("click Count is " + clickCount);

        if (clickCount % 2 == 1)
        {
            selectedPlayerCard = card;
            ReplaceCard(card);
        }
        else
        {
            if (card == 0)
                BuyCard(0, card);
            else
                BuyCard(availCards[card - 1].id, card);
            Debug.Log("availCards[card].id is " + availCards[card].id);
        }
        //CardBuy(card);
    }

    public void ReplaceCard(int card)
    {
        viewPanel.SetActive(false);
        replacePanel.SetActive(true);

        if (card < 2)
            replaceHeading.text = "Power Ups";
        else
            replaceHeading.text = "Sponsorships";

        for (int i = 0; i < playerCards.Length; i++)
        {
            UnPreviewPoints(i);

            for (int j = 0; j < cardDisplays[i].effectSliders.Length; j++)
            {
                cardDisplays[i + 4].effectSliders[j].minValue = -25f;
            }
        }

        cardDisplays[4].name.text = playerCards[card].name + " - " + playerCards[card].length.ToString();
        cardDisplays[4].description.text = playerCards[card].description;
        cardDisplays[4].cost.text = "$" + playerCards[card].cost.ToString("n0");

        cardDisplays[4].effectSliders[0].value = playerCards[card].draw;
        cardDisplays[4].effectSliders[1].value = playerCards[card].guard;
        cardDisplays[4].effectSliders[2].value = playerCards[card].takeOut;
        cardDisplays[4].effectSliders[3].value = playerCards[card].sweepStrength;
        cardDisplays[4].effectSliders[4].value = playerCards[card].sweepEnduro;
        cardDisplays[4].effectSliders[5].value = playerCards[card].sweepCohesion;

        cardDisplays[4].effectSliders[6].value = playerCards[card].oppDraw;
        cardDisplays[4].effectSliders[7].value = playerCards[card].oppGuard;
        cardDisplays[4].effectSliders[8].value = playerCards[card].oppTakeOut;
        cardDisplays[4].effectSliders[9].value = playerCards[card].oppStrength;
        cardDisplays[4].effectSliders[10].value = playerCards[card].oppEnduro;
        cardDisplays[4].effectSliders[11].value = playerCards[card].oppCohesion;

        ColorChanger(1, 0);


        for (int i = 5; i < cardGOs.Length; i++)
        {
            cardDisplays[i].name.text = availCards[i - 5].name + " - " + availCards[i - 5].length.ToString();
            cardDisplays[i].description.text = availCards[i - 5].description;
            cardDisplays[i].cost.text = "$" + availCards[i - 5].cost.ToString("n0");
            cardDisplays[i].costPanel.SetActive(true);

            cardDisplays[i].effectSliders[0].value = availCards[i - 5].draw;
            cardDisplays[i].effectSliders[1].value = availCards[i - 5].guard;
            cardDisplays[i].effectSliders[2].value = availCards[i - 5].takeOut;
            cardDisplays[i].effectSliders[3].value = availCards[i - 5].sweepStrength;
            cardDisplays[i].effectSliders[4].value = availCards[i - 5].sweepEnduro;
            cardDisplays[i].effectSliders[5].value = availCards[i - 5].sweepCohesion;

            cardDisplays[i].effectSliders[6].value = availCards[i - 5].oppDraw;
            cardDisplays[i].effectSliders[7].value = availCards[i - 5].oppGuard;
            cardDisplays[i].effectSliders[8].value = availCards[i - 5].oppTakeOut;
            cardDisplays[i].effectSliders[9].value = availCards[i - 5].oppStrength;
            cardDisplays[i].effectSliders[10].value = availCards[i - 5].oppEnduro;
            cardDisplays[i].effectSliders[11].value = availCards[i - 5].oppCohesion;

            ColorChanger(0, i);
        }


        for (int i = 0; i < playerCards.Length; i++)
        {
            PreviewPoints(i);

            for (int j = 0; j < cardDisplays[i].effectSliders.Length; j++)
            {
                if (cardDisplays[i + 4].effectSliders[j].value < 0)
                {
                    cardDisplays[i + 4].effectSliders[j].value *= -1f;
                    cardDisplays[i + 4].effectSliders[j].gameObject.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().color = Color.red;
                }
                else
                {
                    if (j < 6)
                        cardDisplays[i + 4].effectSliders[j].gameObject.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().color = bgColors[1];
                    else
                        cardDisplays[i + 4].effectSliders[j].gameObject.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().color = bgColors[3];
                }
            }

            for (int j = 0; j < cardDisplays[i].effectSliders.Length; j++)
            {
                cardDisplays[i + 4].effectSliders[j].minValue = 0f;
            }
        }
    }

    public void BuyCard(int cardSelected, int card)
    {
        viewPanel.SetActive(true);
        replacePanel.SetActive(false);

        for (int i = 0; i < playerCards.Length; i++)
        {
            UnPreviewPoints(i);

            for (int j = 0; j < cardDisplays[i].effectSliders.Length; j++)
            {
                cardDisplays[i].effectSliders[j].minValue = -25f;
            }
        }


        Debug.Log("Player Card to be replaced is " + selectedPlayerCard);
        if (card == 0)
        { 
            for (int i = 0; i < (cardGOs.Length / 2); i++)
            {
                cardDisplays[i].name.text = playerCards[i].name + " - " + playerCards[i].length.ToString();
                cardDisplays[i].description.text = playerCards[i].description;
                cardDisplays[i].cost.text = "$" + playerCards[i].cost.ToString("n0");

                cardDisplays[i].effectSliders[0].value = playerCards[i].draw;
                cardDisplays[i].effectSliders[1].value = playerCards[i].guard;
                cardDisplays[i].effectSliders[2].value = playerCards[i].takeOut;
                cardDisplays[i].effectSliders[3].value = playerCards[i].sweepStrength;
                cardDisplays[i].effectSliders[4].value = playerCards[i].sweepEnduro;
                cardDisplays[i].effectSliders[5].value = playerCards[i].sweepCohesion;

                cardDisplays[i].effectSliders[6].value = playerCards[i].oppDraw;
                cardDisplays[i].effectSliders[7].value = playerCards[i].oppGuard;
                cardDisplays[i].effectSliders[8].value = playerCards[i].oppTakeOut;
                cardDisplays[i].effectSliders[9].value = playerCards[i].oppStrength;
                cardDisplays[i].effectSliders[10].value = playerCards[i].oppEnduro;
                cardDisplays[i].effectSliders[11].value = playerCards[i].oppCohesion;


                cardDisplays[i].costPanel.SetActive(false);
            }

        }
        else
        {
            costPerWeek = 0;

            for (int i = 0; i < (cardGOs.Length / 2); i++)
            {
                if (i == selectedPlayerCard)
                {
                    Card tempCard = playerCards[i];

                    for (int j = 0; j < availCards.Count; j++)
                    {
                        if (availCards[j].id == cardSelected)
                        {
                            playerCards[i] = availCards[j];
                            availCards[j].active = true;
                            availCards[j].played = true;
                            cards[j] = availCards[j];
                            availCards[j] = tempCard;
                        }
                    }
                }

                cardDisplays[i].name.text = playerCards[i].name + " - " + playerCards[i].length.ToString();
                cardDisplays[i].description.text = playerCards[i].description;
                cardDisplays[i].cost.text = "$" + playerCards[i].cost.ToString("n0");

                cardDisplays[i].effectSliders[0].value = playerCards[i].draw;
                cardDisplays[i].effectSliders[1].value = playerCards[i].guard;
                cardDisplays[i].effectSliders[2].value = playerCards[i].takeOut;
                cardDisplays[i].effectSliders[3].value = playerCards[i].sweepStrength;
                cardDisplays[i].effectSliders[4].value = playerCards[i].sweepEnduro;
                cardDisplays[i].effectSliders[5].value = playerCards[i].sweepCohesion;

                cardDisplays[i].effectSliders[6].value = playerCards[i].oppDraw;
                cardDisplays[i].effectSliders[7].value = playerCards[i].oppGuard;
                cardDisplays[i].effectSliders[8].value = playerCards[i].oppTakeOut;
                cardDisplays[i].effectSliders[9].value = playerCards[i].oppStrength;
                cardDisplays[i].effectSliders[10].value = playerCards[i].oppEnduro;
                cardDisplays[i].effectSliders[11].value = playerCards[i].oppCohesion;

                activeIdList[i] = playerCards[i].id;
                costPerWeek += playerCards[i].cost;
                cardDisplays[i].costPanel.SetActive(false);
            }

            
        }

        for (int i = 0; i < playerCards.Length; i++)
        {
            if (playerCards[i].id == 99)
                ColorChanger(2, i);
            else
                ColorChanger(0, i);

            PreviewPoints(i);

            for (int j = 0; j < cardDisplays[i].effectSliders.Length; j++)
            {
                if (cardDisplays[i].effectSliders[j].value < 0)
                {
                    cardDisplays[i].effectSliders[j].value *= -1f;
                    cardDisplays[i].effectSliders[j].gameObject.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().color = Color.red;
                }
                else
                {
                    if (j < 6)
                        cardDisplays[i].effectSliders[j].gameObject.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().color = bgColors[1];
                    else
                        cardDisplays[i].effectSliders[j].gameObject.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().color = bgColors[3];
                }
            }

            for (int j = 0; j < cardDisplays[i].effectSliders.Length; j++)
            {
                cardDisplays[i].effectSliders[j].minValue = 0f;
            }
        }
    }
}
