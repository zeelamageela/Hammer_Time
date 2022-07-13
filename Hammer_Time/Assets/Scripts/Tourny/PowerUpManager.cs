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

    public int[] idPUList;
    public int[] idSponsorList;
    public int[] activeIdList;
    public int[] playedPUIdList;
    public int[] playedSponsorIdList;

    public int[] activeLengthList;
    public int[] usedLengthList;

    public Card[] cardsPU;
    public Card_Sponsor[] cardsSponsor;
    public List<Card> availPUCards;
    public List<Card_Sponsor> availSponsorCards;
    public Card[] activeCards;
    public List<Card> playedPUCards;
    public List<Card_Sponsor> playedSponsorCards;
    public Card emptyCard;

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
        SetUp();
    }

    private void Update()
    {
        
    }

    //public void BuyCard(int card)
    //{
    //    cm = FindObjectOfType<CareerManager>();

    //    cm.earnings -= availPUCards[card].cost;
    //    availPUCards[card].active = true;

    //    Debug.Log("Buying " + availPUCards[card].name + " for $" + availPUCards[card].cost + ", money left is " + cm.earnings);

    //    AssignPoints(card);
    //}

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
    void Shuffle(Card_Sponsor[] a)
    {
        // Loops through array
        for (int i = a.Length - 1; i > 0; i--)
        {
            // Randomize a number between 0 and i (so that the range decreases each time)
            int rnd = Random.Range(0, i);

            // Save the value of the current i, otherwise it'll overright when we swap the values
            Card_Sponsor temp = a[i];

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
        cardsPU = new Card[pUpList.powerUps.Length];
        cardsSponsor = new Card_Sponsor[pUpList.sponsors.Length];
        availPUCards = new List<Card>();
        availSponsorCards = new List<Card_Sponsor>();

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

        if (cm.playedCardPUIDList != null)
        {
            playedPUIdList = cm.playedCardPUIDList;
        }

        if (cm.playedCardSponsorIDList != null)
        {
            playedSponsorIdList = cm.playedCardSponsorIDList;
        }

        Shuffle(cardsPU);

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

        Shuffle(cardsPU);

        for (int i = 0; i < activeIdList.Length; i++)
        {
            if (i < 2)
            {
                if (activeIdList[i] == 99)
                {
                    activeCards[i] = emptyCard;
                }
                else
                {
                    for (int j = 0; j < cardsPU.Length; j++)
                    {
                        if (cardsPU[j].id == activeIdList[i])
                        {
                            cardsPU[j].duration--;

                            Debug.Log(cardsPU[j].name + " is " + cardsPU[j].duration + " long");
                            if (cardsPU[j].duration > 0)
                            {
                                cardsPU[j].played = true;
                                activeCards[i] = cardsPU[j];

                                ColorChanger(0, i);
                                //activeCards[i].active = true;
                            }
                            else
                            {
                                cardsPU[j].active = false;
                                cardsPU[j].played = true;
                                activeCards[i] = emptyCard;
                                ColorChanger(2, i);
                            }
                        }
                    }
                }
            }
            else
            {

                if (activeIdList[i] == 99)
                {
                    activeCards[i] = emptyCard;
                }
                else
                {
                    for (int j = 0; j < cardsSponsor.Length; j++)
                    {
                        if (cardsSponsor[j].id == activeIdList[i])
                        {
                            cardsSponsor[j].duration--;

                            Debug.Log(cardsSponsor[j].name + " is " + cardsSponsor[j].duration + " long");
                            if (cardsSponsor[j].duration > 0)
                            {
                                cardsSponsor[j].played = true;
                                activeCards[i] = cardsPU[j];

                                ColorChanger(0, i);
                                //activeCards[i].active = true;
                            }
                            else
                            {
                                cardsPU[j].active = false;
                                cardsPU[j].played = true;
                                activeCards[i] = emptyCard;
                                ColorChanger(2, i);
                            }
                        }
                    }
                }
            }
        }

        for (int i = 0; i < playedPUIdList.Length; i++)
        {
            for (int j = 0; j < cardsPU.Length; j++)
            {
                if (cardsPU[j].id == playedPUIdList[i])
                {
                    cardsPU[j].played = true;
                    playedPUCards.Add(cardsPU[j]);
                }
            }
        }

        for (int i = 0; i < cardsPU.Length; i++)
        {
            if (cardsPU[i].active == false | cardsPU[i].played == true)
            {
                playedPUCards.Add(cardsPU[i]);
            }
        }

        for (int i = 0; i < playedPUIdList.Length; i++)
        {
            for (int j = 0; j < cardsPU.Length; j++)
            {
                if (cardsPU[j].id == playedPUIdList[i])
                {
                    cardsPU[j].played = true;
                    playedPUCards.Add(cardsPU[j]);
                }
            }
        }

        for (int i = 0; i < cardsPU.Length; i++)
        {
            if (cardsPU[i].active == false | cardsPU[i].played == true)
            {
                playedPUCards.Add(cardsPU[i]);
            }
        }

        Debug.Log("AvailCards Length is " + availPUCards.Count);

        for (int i = 0; i < (cardGOs.Length / 2f); i++)
        {
            cardDisplays[i].name.text = activeCards[i].name + " - " + activeCards[i].duration.ToString() + " weeks";
            cardDisplays[i].description.text = activeCards[i].description;
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

            if (availPUCards[i].active)
            {
                cardDisplays[i].costPanel.SetActive(false);
            }
        }

        for (int i = 0; i < activeCards.Length; i++)
        {
            costPerWeek += activeCards[i].cost;
        }

        Debug.Log("costPerWeek is " + costPerWeek);

        for (int i = 0; i < activeCards.Length; i++)
        {
            if (activeCards[i].id == 99)
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
                BuyCard(availPUCards[card - 1].id, card);

            Debug.Log("availPUCards[card].id is " + availPUCards[card].id);
        }
        //CardBuy(card);
    }

    public void ReplaceCard(int card)
    {
        viewPanel.SetActive(false);
        replacePanel.SetActive(true);

        if (card < 2)
        {
            replaceHeading.text = "Power Ups";

            for (int i = 0; i < activeCards.Length; i++)
            {
                UnPreviewPoints(i);

                for (int j = 0; j < cardDisplays[i].effectSliders.Length; j++)
                {
                    cardDisplays[i + 4].effectSliders[j].minValue = -25f;
                }
            }

            cardDisplays[4].name.text = activeCards[card].name + " - " + activeCards[card].duration.ToString() + " weeks";
            cardDisplays[4].description.text = activeCards[card].description;
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

            ColorChanger(1, 0);


            for (int i = 5; i < cardGOs.Length; i++)
            {
                cardDisplays[i].name.text = availPUCards[i - 5].name + " - " + availPUCards[i - 5].duration.ToString() + " weeks";
                cardDisplays[i].description.text = availPUCards[i - 5].description;
                cardDisplays[i].cost.text = "$" + availPUCards[i - 5].cost.ToString("n0");
                cardDisplays[i].costPanel.SetActive(true);

                cardDisplays[i].effectSliders[0].value = availPUCards[i - 5].draw;
                cardDisplays[i].effectSliders[1].value = availPUCards[i - 5].guard;
                cardDisplays[i].effectSliders[2].value = availPUCards[i - 5].takeOut;
                cardDisplays[i].effectSliders[3].value = availPUCards[i - 5].sweepStrength;
                cardDisplays[i].effectSliders[4].value = availPUCards[i - 5].sweepEnduro;
                cardDisplays[i].effectSliders[5].value = availPUCards[i - 5].sweepCohesion;

                cardDisplays[i].effectSliders[6].value = availPUCards[i - 5].oppDraw;
                cardDisplays[i].effectSliders[7].value = availPUCards[i - 5].oppGuard;
                cardDisplays[i].effectSliders[8].value = availPUCards[i - 5].oppTakeOut;
                cardDisplays[i].effectSliders[9].value = availPUCards[i - 5].oppStrength;
                cardDisplays[i].effectSliders[10].value = availPUCards[i - 5].oppEnduro;
                cardDisplays[i].effectSliders[11].value = availPUCards[i - 5].oppCohesion;

                ColorChanger(0, i);
            }


            for (int i = 0; i < activeCards.Length; i++)
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
        else
        {
            replaceHeading.text = "Sponsorships";


        }

    }

    public void BuyCard(int cardSelected, int card, bool pu = true)
    {
        viewPanel.SetActive(true);
        replacePanel.SetActive(false);

        for (int i = 0; i < activeCards.Length; i++)
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
                cardDisplays[i].name.text = activeCards[i].name + " - " + activeCards[i].duration.ToString() + " weeks";
                cardDisplays[i].description.text = activeCards[i].description;
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
                    Card tempCard = activeCards[i];

                    for (int j = 0; j < availPUCards.Count; j++)
                    {
                        if (availPUCards[j].id == cardSelected)
                        {
                            activeCards[i] = availPUCards[j];
                            availPUCards[j].active = true;
                            availPUCards[j].played = true;
                            cardsPU[j] = availPUCards[j];
                            availPUCards[j] = tempCard;
                        }
                    }
                }

                cardDisplays[i].name.text = activeCards[i].name + " - " + activeCards[i].duration.ToString() + " weeks";
                cardDisplays[i].description.text = activeCards[i].description;
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
                cardDisplays[i].costPanel.SetActive(false);
            }

            
        }

        for (int i = 0; i < activeCards.Length; i++)
        {
            if (activeCards[i].id == 99)
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
