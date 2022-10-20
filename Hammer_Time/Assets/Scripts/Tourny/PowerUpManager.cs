using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpManager : MonoBehaviour
{
    CareerManager cm;
    public TournySelector tSel;
    public TeamMenu tm;
    public Color[] bgColors;
    public Color[] textColors;

    public int[] idPUList;
    public int[] idSponsorList;
    public int[] activeIdList;
    public int[] playedIdList;

    public int[] activeLengthList;
    public int[] usedLengthList;

    public Card[] cardsPU;
    public Card[] cardsSponsor;
    public List<Card> availPUCards;
    public List<Card> availSponsorCards;
    public Card[] activeCards;
    public List<Card> playedCards;
    public Card emptyPUCard;
    public Card emptySponsorCard;

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
    bool puActive;
    int colorChanger;

    CharacterStats[] cStats;

    // Start is called before the first frame update
    void Start()
    {
        clickCount = 0;
        cm = FindObjectOfType<CareerManager>();
        tSel = FindObjectOfType<TournySelector>();
        tm = FindObjectOfType<TeamMenu>();
    }

    private void Update()
    {
        
    }

    IEnumerator WaitForClick()
    {
        tSel = FindObjectOfType<TournySelector>();

        //yield return new WaitForSeconds(3f);
        //tSel.SetUp();
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

        idPUList[cm.week] = availPUCards[card].id;
        Debug.Log("id of played card is " + idPUList[cm.week]);
        Debug.Log("availPUCards length is " + availPUCards.Count);
        //int idPUListLength = 0;
        //cm.SaveCareer();
        //for (int i = 0; i < numberOfCards; i++)
        //{
        //    if (availPUCards[i].active)
        //        idPUList[cm.week] = availPUCards[i].id;
        //}
    }

    public void PreviewPoints(int card)
    {
        cm = FindObjectOfType<CareerManager>();

        cm.modStats.drawAccuracy += activeCards[card].draw;
        cm.modStats.guardAccuracy += activeCards[card].guard;
        cm.modStats.takeOutAccuracy += activeCards[card].takeOut;
        cm.modStats.sweepEndurance += activeCards[card].sweepEnduro;
        cm.modStats.sweepStrength += activeCards[card].sweepStrength;
        cm.modStats.sweepCohesion += activeCards[card].sweepCohesion;
        cm.oppStats.drawAccuracy += activeCards[card].oppDraw;
        cm.oppStats.guardAccuracy += activeCards[card].oppGuard;
        cm.oppStats.takeOutAccuracy += activeCards[card].oppTakeOut;
        cm.oppStats.sweepEndurance += activeCards[card].oppEnduro;
        cm.oppStats.sweepStrength += activeCards[card].oppStrength;
        cm.oppStats.sweepCohesion += activeCards[card].oppCohesion;

    }

    public void UnPreviewPoints(int card)
    {
        cm = FindObjectOfType<CareerManager>();
        //DisplayCards(numberOfCards);
        cm.modStats.drawAccuracy -= activeCards[card].draw;
        cm.modStats.guardAccuracy -= activeCards[card].guard;
        cm.modStats.takeOutAccuracy -= activeCards[card].takeOut;
        cm.modStats.sweepEndurance -= activeCards[card].sweepEnduro;
        cm.modStats.sweepStrength -= activeCards[card].sweepStrength;
        cm.modStats.sweepCohesion -= activeCards[card].sweepCohesion;
        cm.oppStats.drawAccuracy -= activeCards[card].oppDraw;
        cm.oppStats.guardAccuracy -= activeCards[card].oppGuard;
        cm.oppStats.takeOutAccuracy -= activeCards[card].oppTakeOut;
        cm.oppStats.sweepEndurance -= activeCards[card].oppEnduro;
        cm.oppStats.sweepStrength -= activeCards[card].oppStrength;
        cm.oppStats.sweepCohesion -= activeCards[card].oppCohesion;
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

    void ColorChanger(int bgColor, int textColor, int card)
    {
        cardDisplays[card].name.color = textColors[textColor];
        cardDisplays[card].description.color = textColors[textColor];
        cardGOs[card].GetComponent<Image>().color = bgColors[bgColor];

    }

    public void SetUp()
    {
        cardsPU = new Card[pUpList.powerUps.Length];
        cardsSponsor = new Card[pUpList.sponsors.Length];
        availPUCards = new List<Card>();
        availSponsorCards = new List<Card>();
        idPUList = new int[pUpList.powerUps.Length];
        idSponsorList = new int[pUpList.sponsors.Length];

        //Check if there's a save file
        if (cm.cardPUIDList.Length > 0)
        {
            idPUList = cm.cardPUIDList;
            idSponsorList = cm.cardSponsorIDList;
        }
        else
        {
            idPUList = new int[cardsPU.Length];
            idSponsorList = new int[cardsSponsor.Length];
            Shuffle(pUpList.powerUps);
            Shuffle(pUpList.sponsors);

            for (int i = 0; i < idPUList.Length; i++)
            {
                idPUList[i] = pUpList.powerUps[i].id;
                cardsPU[i] = pUpList.powerUps[i];
            }

            for (int i = 0; i < idSponsorList.Length; i++)
            {
                idSponsorList[i] = pUpList.sponsors[i].id;
                cardsSponsor[i] = pUpList.sponsors[i];
            }
        }

        if (cm.activeCardIDList.Length > 0)
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

        if (cm.playedCardIDList.Length > 0)
        {
            playedIdList = cm.playedCardIDList;
        }

        //match the card ids to load a saved state
        for (int i = 0; i < idPUList.Length; i++)
        {
            for (int j = 0; j < pUpList.powerUps.Length; j++)
            {
                if (pUpList.powerUps[j].id == idPUList[i])
                {
                    cardsPU[i] = pUpList.powerUps[j];
                }
            }
        }

        for (int i = 0; i < idSponsorList.Length; i++)
        {
            for (int j = 0; j < pUpList.sponsors.Length; j++)
            {
                if (idSponsorList[i] == pUpList.sponsors[j].id)
                {
                    cardsSponsor[i] = pUpList.sponsors[j];
                }
            }
        }

        Shuffle(cardsPU);
        Shuffle(cardsSponsor);

        //Determine which cards are active
        for (int i = 0; i < activeIdList.Length; i++)
        {
            if (i < 2)
            {
                //99 is empty code
                if (activeIdList[i] == 99)
                {
                    activeCards[i] = emptyPUCard;
                }
                else
                {
                    for (int j = 0; j < cardsPU.Length; j++)
                    {
                        if (cardsPU[j].id == activeIdList[i])
                        {
                            cardsPU[j].duration = activeLengthList[i];
                            cardsPU[j].duration--;
                            activeLengthList[i] = cardsPU[j].duration;

                            Debug.Log(cardsPU[j].name + " is " + cardsPU[j].duration + " long");
                            if (cardsPU[j].duration > 0)
                            {
                                cardsPU[j].active = true;
                                cardsPU[j].played = true;
                                activeCards[i] = cardsPU[j];

                                ColorChanger(4, 1, i);
                                //activeCards[i].active = true;
                            }
                            else
                            {
                                cardsPU[j].active = false;
                                cardsPU[j].played = true;
                                activeCards[i] = emptyPUCard;
                                ColorChanger(2, 2, i);
                            }
                        }
                    }
                }
            }
            else
            {

                if (activeIdList[i] == 99)
                {
                    activeCards[i] = emptySponsorCard;
                }
                else
                {
                    for (int j = 0; j < cardsSponsor.Length; j++)
                    {
                        if (cardsSponsor[j].id == activeIdList[i])
                        {
                            cardsSponsor[j].duration = activeLengthList[i];
                            cardsSponsor[j].duration--;

                            Debug.Log(cardsSponsor[j].name + " is " + cardsSponsor[j].duration + " long");
                            if (cardsSponsor[j].duration > 0)
                            {
                                cardsSponsor[j].active = true;
                                cardsSponsor[j].played = true;
                                activeCards[i] = cardsSponsor[j];
                                ColorChanger(6, 0, i);
                                //activeCards[i].active = true;
                            }
                            else
                            {
                                cardsSponsor[j].active = false;
                                cardsSponsor[j].played = true;
                                activeCards[i] = emptySponsorCard;
                                ColorChanger(6, 0, i);
                            }
                        }
                    }
                }
            }
        }

        //Determine which cards have been played
        for (int i = 0; i < playedIdList.Length; i++)
        {
            for (int j = 0; j < cardsPU.Length; j++)
            {
                if (cardsPU[j].id == playedIdList[i])
                {
                    cardsPU[j].played = true;
                    playedCards.Add(cardsPU[j]);
                }
            }

            for (int j = 0; j < cardsSponsor.Length; i++)
            {
                if (cardsPU[j].id == playedIdList[i])
                {
                    cardsSponsor[i].played = true;
                    playedCards.Add(cardsSponsor[j]);
                }
            }
        }

        //The rest are available
        for (int i = 0; i < cardsPU.Length; i++)
        {
            if (!cardsPU[i].active | !cardsPU[i].played)
            {
                //identify signing conditions of the sponsorship, then add the card to available
                if (cardsPU[i].signCondition == "wins")
                {
                    if (cardsPU[i].signConditionValue < cm.record.x)
                        availPUCards.Add(cardsPU[i]);
                }
                if (cardsPU[i].signCondition == "earnings")
                {
                    if ((float)cardsPU[i].signConditionValue < cm.earnings)
                        availPUCards.Add(cardsPU[i]);
                }
                if (cardsPU[i].signCondition == "provQual")
                {
                    if (cm.provQual)
                        availPUCards.Add(cardsPU[i]);
                }
                if (cardsPU[i].signCondition == "week")
                {
                    if (cardsPU[i].signConditionValue < cm.week)
                        availPUCards.Add(cardsPU[i]);
                }
                if (cardsPU[i].signCondition == "None")
                {
                    availPUCards.Add(cardsPU[i]);
                }
            }
        }

        for (int i = 0; i < cardsSponsor.Length; i++)
        {
            if (!cardsSponsor[i].active | !cardsSponsor[i].played)
            {
                //identify signing conditions of the sponsorship, then add the card to available
                if (cardsSponsor[i].signCondition == "wins")
                {
                    if (cardsSponsor[i].signConditionValue < cm.record.x)
                        availSponsorCards.Add(cardsSponsor[i]);
                }
                if (cardsSponsor[i].signCondition == "earnings")
                {
                    if ((float)cardsSponsor[i].signConditionValue < cm.earnings)
                        availSponsorCards.Add(cardsSponsor[i]);
                }
                if (cardsSponsor[i].signCondition == "provQual")
                {
                    if (cm.provQual)
                        availSponsorCards.Add(cardsSponsor[i]);
                }
                if (cardsSponsor[i].signCondition == "week")
                {
                    if (cardsSponsor[i].signConditionValue < cm.week)
                        availSponsorCards.Add(cardsSponsor[i]);
                }
                if (cardsSponsor[i].signCondition == "None")
                {
                    availSponsorCards.Add(cardsSponsor[i]);
                }
            }
        }

        //Debug.Log("AvailSponsorCards Length is " + availSponsorCards.Count);

        for (int i = 0; i < (cardGOs.Length / 2f); i++)
        {
            if (activeCards[i].duration > 0)
                cardDisplays[i].name.text = activeCards[i].name + " - " + activeCards[i].duration.ToString() + " weeks";
            else if (activeCards[i].duration == 1)
                cardDisplays[i].name.text = activeCards[i].name + " - " + activeCards[i].duration.ToString() + " week";
            else if (activeCards[i].duration > 50)
                cardDisplays[i].name.text = activeCards[i].name + " - " + " ongoing";
            else
                cardDisplays[i].name.text = "[" + activeCards[i].name + "]";

            if (activeCards[i].cost > 0)
                cardDisplays[i].description.text = "$" + activeCards[i].cost.ToString("n0") + " - " + activeCards[i].description;
            else
                cardDisplays[i].description.text = " ";

            cardDisplays[i].cost.text = "$" + activeCards[i].cost.ToString("n0");

            cardDisplays[i].effectSliders[0].value = activeCards[i].draw;
            cardDisplays[i].effectSliders[1].value = activeCards[i].guard;
            cardDisplays[i].effectSliders[2].value = activeCards[i].takeOut;
            cardDisplays[i].effectSliders[3].value = activeCards[i].sweepStrength;
            cardDisplays[i].effectSliders[4].value = activeCards[i].sweepEnduro;
            cardDisplays[i].effectSliders[5].value = activeCards[i].sweepCohesion;

            cardDisplays[i].effectSliders[6].value = activeCards[i].oppDraw;
            cardDisplays[i].effectSliders[7].value = activeCards[i].oppGuard;
            cardDisplays[i].effectSliders[8].value = activeCards[i].oppTakeOut;
            cardDisplays[i].effectSliders[9].value = activeCards[i].oppStrength;
            cardDisplays[i].effectSliders[10].value = activeCards[i].oppEnduro;
            cardDisplays[i].effectSliders[11].value = activeCards[i].oppCohesion;

            if (activeCards[i].id == 99)
            {
                cardDisplays[i].image.gameObject.SetActive(false);
                cardDisplays[i].description.gameObject.SetActive(false);
                cardDisplays[i].cost.gameObject.SetActive(false);
                cardDisplays[i].costPanel.SetActive(true);
            }
            else
                cardDisplays[i].costPanel.SetActive(false);
        }

        for (int i = 0; i < activeCards.Length; i++)
        {
            costPerWeek += activeCards[i].cost;
        }

        Debug.Log("costPerWeek is " + costPerWeek);

        for (int i = 0; i < activeCards.Length; i++)
        {
            if (activeCards[i].id == 99)
            {
                ColorChanger(6, 1, i);
            }
            else
            {
                if (i < 2)
                    ColorChanger(4, 1, i);
                else
                    ColorChanger(5, 1, i);
            }

            tm.PreviewPoints();

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

        //see if cards is not active and played
        for (int i = 0; i < cardsPU.Length; i++)
        {
            bool dupCheck = false;
            if (cardsPU[i].active == false | cardsPU[i].played == true)
            {
                for (int j = 0; j < playedCards.Count; j++)
                {
                    if (playedCards[j].id == cardsPU[i].id)
                        dupCheck = true;
                }
                    if (!dupCheck)
                    playedCards.Add(cardsPU[i]);
            }
        }

        for (int i = 0; i < cardsSponsor.Length; i++)
        {
            if (cardsSponsor[i].active == false | cardsSponsor[i].played == true)
            {
                bool dupCheck = false;
                for (int j = 0; j < playedCards.Count; j++)
                {
                    if (playedCards[j].id == cardsSponsor[i].id)
                        dupCheck = true;
                }
                if (!dupCheck)
                    playedCards.Add(cardsSponsor[i]);
            }
        }
    }

    public void CardClick(int card)
    {
        clickCount++;
        //Debug.Log("click Count is " + clickCount);

        if (clickCount % 2 == 1)
        {
            if (card < 2)
                puActive = true;
            else
                puActive = false;

            selectedPlayerCard = card;
            ReplaceCard(card);
        }
        else
        {
            if (card == 0)
                BuyCard(0, card);
            else
            {
                if (puActive)
                    BuyCard(availPUCards[card - 1].id, card, puActive);
                else
                    BuyCard(availSponsorCards[card - 1].id, card, puActive);
            }

            Debug.Log("availPUCards[card].id is " + availPUCards[card].id);
        }
        //CardBuy(card);
    }

    public void ReplaceCard(int card)
    {
        viewPanel.SetActive(false);
        replacePanel.SetActive(true);

        for (int i = 0; i < activeCards.Length; i++)
        {
            tm.UnPreviewPoints();

            for (int j = 0; j < cardDisplays[i].effectSliders.Length; j++)
            {
                cardDisplays[i + 4].effectSliders[j].minValue = -25f;
            }
        }

        if (activeCards[card].duration > 50)
            cardDisplays[4].name.text = activeCards[card].name + " - " + " ongoing";
        else if (activeCards[card].duration == 1)
            cardDisplays[4].name.text = activeCards[card].name + " - " + activeCards[card].duration.ToString() + " week";
        else if (activeCards[card].duration > 0)
            cardDisplays[4].name.text = activeCards[card].name + " - " + activeCards[card].duration.ToString() + " weeks";
        else
            cardDisplays[4].name.text = activeCards[card].name;

        cardDisplays[4].description.text = "$" + activeCards[card].cost.ToString("n0") + " - " + activeCards[card].description;

        cardDisplays[4].cost.text = "$" + activeCards[card].cost.ToString("n0");


        cardDisplays[4].effectSliders[0].value = activeCards[card].draw;
        cardDisplays[4].effectSliders[1].value = activeCards[card].guard;
        cardDisplays[4].effectSliders[2].value = activeCards[card].takeOut;
        cardDisplays[4].effectSliders[3].value = activeCards[card].sweepStrength;
        cardDisplays[4].effectSliders[4].value = activeCards[card].sweepEnduro;
        cardDisplays[4].effectSliders[5].value = activeCards[card].sweepCohesion;

        cardDisplays[4].effectSliders[6].value = activeCards[card].oppDraw;
        cardDisplays[4].effectSliders[7].value = activeCards[card].oppGuard;
        cardDisplays[4].effectSliders[8].value = activeCards[card].oppTakeOut;
        cardDisplays[4].effectSliders[9].value = activeCards[card].oppStrength;
        cardDisplays[4].effectSliders[10].value = activeCards[card].oppEnduro;
        cardDisplays[4].effectSliders[11].value = activeCards[card].oppCohesion;


        if (activeCards[card].id == 99)
        {
            cardDisplays[4].image.gameObject.SetActive(false);
            cardDisplays[4].description.gameObject.SetActive(true);
            cardDisplays[4].cost.gameObject.SetActive(false);
            cardDisplays[4].effectSliders[0].transform.parent.gameObject.SetActive(false);
            cardDisplays[4].costPanel.SetActive(false);
            ColorChanger(6, 1, 4);
        }
        else
        {
            cardDisplays[4].image.gameObject.SetActive(true);
            cardDisplays[4].description.gameObject.SetActive(true);
            cardDisplays[4].cost.gameObject.SetActive(false);
            cardDisplays[4].effectSliders[0].transform.parent.gameObject.SetActive(true);
            cardDisplays[4].costPanel.SetActive(false);
            ColorChanger(1, 0, 4);
        }

        if (card < 2)
        {
            replaceHeading.text = "Power Ups";

            for (int i = 5; i < cardGOs.Length; i++)
            {
                cardGOs[i].GetComponent<Button>().interactable = true;
                if (availPUCards[i - 5].duration > 50)
                    cardDisplays[i].name.text = availPUCards[i - 5].name + " - " + " ongoing";
                else if (availPUCards[i - 5].duration == 1)
                    cardDisplays[i].name.text = availPUCards[i - 5].name + " - " + availPUCards[i - 5].duration.ToString() + " week";
                else if (availPUCards[i - 5].duration > 0)
                    cardDisplays[i].name.text = availPUCards[i - 5].name + " - " + availPUCards[i - 5].duration.ToString() + " weeks";
                else
                    cardDisplays[i].name.text = availPUCards[i - 5].name;

                cardDisplays[i].cost.text = "$" + availPUCards[i - 5].cost.ToString("n0");

                if (availPUCards[i - 5].id == 99 | availPUCards[i - 5].cost == 0f)
                {
                    cardDisplays[i].image.gameObject.SetActive(false);
                    cardDisplays[i].description.gameObject.SetActive(false);
                    cardDisplays[i].cost.gameObject.SetActive(false);
                    cardDisplays[i].effectSliders[0].transform.parent.gameObject.SetActive(false);
                    cardDisplays[i].costPanel.SetActive(false);
                    ColorChanger(6, 1, i);
                }
                else
                {
                    cardDisplays[i].image.gameObject.SetActive(false);
                    cardDisplays[i].description.gameObject.SetActive(false);
                    cardDisplays[i].effectSliders[0].transform.parent.gameObject.SetActive(false);
                    cardDisplays[i].cost.gameObject.SetActive(true);
                    cardDisplays[i].costPanel.SetActive(true);
                    ColorChanger(4, 0, i);
                }
            }
        }
        else
        {
            replaceHeading.text = "Sponsorships";

            for (int i = 5; i < cardGOs.Length; i++)
            {
                if (availSponsorCards.Count > i - 5)
                {
                    cardGOs[i].GetComponent<Button>().interactable = true;

                    if (availSponsorCards[i - 5].duration == 1)
                        cardDisplays[i].name.text = availSponsorCards[i - 5].name + " - " + availSponsorCards[i - 5].duration.ToString() + " week";
                    else if (availSponsorCards[i - 5].duration > 50)
                        cardDisplays[i].name.text = availSponsorCards[i - 5].name + " - " + " ongoing";
                    else if (availSponsorCards[i - 5].duration > 0)
                        cardDisplays[i].name.text = availSponsorCards[i - 5].name + " - " + availSponsorCards[i - 5].duration.ToString() + " weeks";
                    else
                        cardDisplays[i].name.text = availSponsorCards[i - 5].name;

                    cardDisplays[i].cost.text = "$" + availSponsorCards[i - 5].cost.ToString("n0");

                    if (availSponsorCards[i - 5].id == 99 | availSponsorCards[i - 5].cost == 0)
                    {
                        cardDisplays[i].description.gameObject.SetActive(false);
                        cardDisplays[i].cost.gameObject.SetActive(false);
                        cardDisplays[i].effectSliders[0].transform.parent.gameObject.SetActive(false);
                        cardDisplays[i].costPanel.SetActive(false);
                        ColorChanger(6, 1, i);
                    }
                    else
                    {
                        cardDisplays[i].description.gameObject.SetActive(false);
                        cardDisplays[i].effectSliders[0].transform.parent.gameObject.SetActive(false);
                        cardDisplays[i].cost.gameObject.SetActive(true);
                        cardDisplays[i].costPanel.SetActive(true);
                        ColorChanger(5, 0, i);
                    }
                }
                else
                {
                    cardGOs[i].GetComponent<Button>().interactable = false;
                    cardDisplays[i].name.text = emptySponsorCard.name;

                    cardDisplays[i].description.gameObject.SetActive(false);
                    cardDisplays[i].cost.gameObject.SetActive(false);
                    cardDisplays[i].effectSliders[0].transform.parent.gameObject.SetActive(false);
                    cardDisplays[i].costPanel.SetActive(false);
                    ColorChanger(6, 1, i);
                }

            }
        }

        //Update the master skillbars and set the skillbars on the card
        for (int i = 0; i < activeCards.Length; i++)
        {
            tm.PreviewPoints();

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

    public void BuyCard(int cardSelected, int card, bool pu = true)
    {
        viewPanel.SetActive(true);
        replacePanel.SetActive(false);

        for (int i = 0; i < activeCards.Length; i++)
        {
            tm.UnPreviewPoints();

            for (int j = 0; j < cardDisplays[i].effectSliders.Length; j++)
            {
                cardDisplays[i].effectSliders[j].minValue = -25f;
            }
        }

        if (card == 0)
        {
            for (int i = 0; i < (cardGOs.Length / 2); i++)
            {
                if (activeCards[i].duration == 1)
                    cardDisplays[i].name.text = activeCards[i].name + " - " + activeCards[i].duration.ToString() + " week";
                if (activeCards[i].duration > 50)
                    cardDisplays[i].name.text = activeCards[i].name + " - " + " ongoing";
                if (activeCards[i].duration > 0)
                    cardDisplays[i].name.text = activeCards[i].name + " - " + activeCards[i].duration.ToString() + " weeks";
                else
                    cardDisplays[i].name.text = activeCards[i].name;

                cardDisplays[i].description.text = "$" + activeCards[i].cost.ToString("n0") + " - " + activeCards[i].description;

                cardDisplays[i].cost.text = "$" + activeCards[i].cost.ToString("n0");


                cardDisplays[i].effectSliders[0].value = activeCards[i].draw;
                cardDisplays[i].effectSliders[1].value = activeCards[i].guard;
                cardDisplays[i].effectSliders[2].value = activeCards[i].takeOut;
                cardDisplays[i].effectSliders[3].value = activeCards[i].sweepStrength;
                cardDisplays[i].effectSliders[4].value = activeCards[i].sweepEnduro;
                cardDisplays[i].effectSliders[5].value = activeCards[i].sweepCohesion;

                cardDisplays[i].effectSliders[6].value = activeCards[i].oppDraw;
                cardDisplays[i].effectSliders[7].value = activeCards[i].oppGuard;
                cardDisplays[i].effectSliders[8].value = activeCards[i].oppTakeOut;
                cardDisplays[i].effectSliders[9].value = activeCards[i].oppStrength;
                cardDisplays[i].effectSliders[10].value = activeCards[i].oppEnduro;
                cardDisplays[i].effectSliders[11].value = activeCards[i].oppCohesion;

                if (activeCards[i].id == 99 | activeCards[i].cost == 0)
                {
                    cardDisplays[i].image.gameObject.SetActive(false);
                    cardDisplays[i].description.gameObject.SetActive(false);
                    cardDisplays[i].cost.gameObject.SetActive(false);
                    cardDisplays[i].effectSliders[0].transform.parent.gameObject.SetActive(false);
                    cardDisplays[i].costPanel.SetActive(false);
                }
                else
                {
                    cardDisplays[i].image.gameObject.SetActive(true);
                    cardDisplays[i].description.gameObject.SetActive(true);
                    cardDisplays[i].effectSliders[0].transform.parent.gameObject.SetActive(true);
                    cardDisplays[i].cost.gameObject.SetActive(false);
                    cardDisplays[i].costPanel.SetActive(false);
                }

                if (activeCards[i].id == 99)
                    ColorChanger(6, 1, i);
                else if (i < 2)
                    ColorChanger(4, 1, i);
                else
                    ColorChanger(5, 1, i);
            }

        }
        else
        {
            costPerWeek = 0;

            for (int i = 0; i < (cardGOs.Length / 2); i++)
            {
                if (i == selectedPlayerCard)
                {
                    Card tempCard = activeCards[i];

                    if (pu)
                    {
                        bool stopReplace = false;
                        for (int j = 0; j < availPUCards.Count; j++)
                        {
                            if (!stopReplace & availPUCards[j].id == cardSelected)
                            {
                                activeCards[i] = availPUCards[j];
                                availPUCards[j].active = true;
                                availPUCards[j].played = true;
                                cardsPU[j] = availPUCards[j];
                                availPUCards[j] = tempCard;
                                stopReplace = true;
                            }
                        }
                    }
                    else
                    {
                        bool stopReplace = false;
                        for (int j = 0; j < availSponsorCards.Count; j++)
                        {
                            if (!stopReplace & availSponsorCards[j].id == cardSelected)
                            {
                                activeCards[i] = availSponsorCards[j];
                                availSponsorCards[j].active = true;
                                availSponsorCards[j].played = true;
                                cardsSponsor[j] = availSponsorCards[j];
                                availSponsorCards[j] = tempCard;
                                stopReplace = true;
                            }
                        }
                    }
                }

                if (activeCards[i].duration > 50)
                    cardDisplays[i].name.text = activeCards[i].name + " - " + " ongoing";
                else if (activeCards[i].duration == 1)
                    cardDisplays[i].name.text = activeCards[i].name + " - " + activeCards[i].duration.ToString() + " week";
                else if (activeCards[i].duration > 0)
                    cardDisplays[i].name.text = activeCards[i].name + " - " + activeCards[i].duration.ToString() + " weeks";
                else
                    cardDisplays[i].name.text = activeCards[i].name;

                cardDisplays[i].description.text = "$" + activeCards[i].cost.ToString("n0") + " - " + activeCards[i].description;

                cardDisplays[i].cost.text = "$" + activeCards[i].cost.ToString("n0");


                cardDisplays[i].effectSliders[0].value = activeCards[i].draw;
                cardDisplays[i].effectSliders[1].value = activeCards[i].guard;
                cardDisplays[i].effectSliders[2].value = activeCards[i].takeOut;
                cardDisplays[i].effectSliders[3].value = activeCards[i].sweepStrength;
                cardDisplays[i].effectSliders[4].value = activeCards[i].sweepEnduro;
                cardDisplays[i].effectSliders[5].value = activeCards[i].sweepCohesion;

                cardDisplays[i].effectSliders[6].value = activeCards[i].oppDraw;
                cardDisplays[i].effectSliders[7].value = activeCards[i].oppGuard;
                cardDisplays[i].effectSliders[8].value = activeCards[i].oppTakeOut;
                cardDisplays[i].effectSliders[9].value = activeCards[i].oppStrength;
                cardDisplays[i].effectSliders[10].value = activeCards[i].oppEnduro;
                cardDisplays[i].effectSliders[11].value = activeCards[i].oppCohesion;

                activeIdList[i] = activeCards[i].id;
                activeLengthList[i] = activeCards[i].duration;
                costPerWeek += activeCards[i].cost;

                if (activeCards[i].id == 99 | activeCards[i].cost == 0)
                {
                    ColorChanger(6, 1, i);
                    cardDisplays[i].image.gameObject.SetActive(false);
                    cardDisplays[i].description.gameObject.SetActive(false);
                    cardDisplays[i].cost.gameObject.SetActive(false);
                    cardDisplays[i].effectSliders[0].transform.parent.gameObject.SetActive(false);
                    cardDisplays[i].costPanel.SetActive(false);
                }
                else
                {
                    if (i < 2)
                        ColorChanger(4, 1, i);
                    else
                        ColorChanger(5, 1, i);
                    cardDisplays[i].image.gameObject.SetActive(true);
                    cardDisplays[i].description.gameObject.SetActive(true);
                    cardDisplays[i].effectSliders[0].transform.parent.gameObject.SetActive(true);
                    cardDisplays[i].cost.gameObject.SetActive(false);
                    cardDisplays[i].costPanel.SetActive(false);
                }

                if (activeCards[i].id == 99)
                {
                    ColorChanger(6, 1, i);
                }
            }
        }

        for (int i = 0; i < activeCards.Length; i++)
        {
            tm.PreviewPoints();

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
