using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SponsorManager : MonoBehaviour
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
        cardsSponsor = new Card[pUpList.localSponsors.Length + pUpList.nationalSponsors.Length + pUpList.internationalSponsors.Length];
        availPUCards = new List<Card>();
        availSponsorCards = new List<Card>();
        idPUList = new int[pUpList.localSponsors.Length];
        idSponsorList = new int[pUpList.nationalSponsors.Length];

        //Check if there's a save file
        if (cm.cardSponsorIDList != null && cm.cardSponsorIDList.Length > 0)
        {
            idSponsorList = cm.cardSponsorIDList;
        }
        else
        {
            idSponsorList = new int[cardsSponsor.Length];
            Shuffle(pUpList.localSponsors);
            Shuffle(pUpList.nationalSponsors);
            Shuffle(pUpList.internationalSponsors);


            for (int i = 0; i < idSponsorList.Length; i++)
            {
                if (i < pUpList.localSponsors.Length)
                {
                    idSponsorList[i] = pUpList.localSponsors[i].id;
                    cardsSponsor[i] = pUpList.localSponsors[i];
                }
                else if ((i - pUpList.localSponsors.Length) < pUpList.nationalSponsors.Length)
                {
                    int tempCount = i - pUpList.localSponsors.Length;
                    Debug.Log("i - pUpList.localSponsors.Length is " + tempCount);
                    idSponsorList[i] = pUpList.nationalSponsors[i - pUpList.localSponsors.Length].id;
                    cardsSponsor[i] = pUpList.nationalSponsors[i - pUpList.localSponsors.Length];
                }
                else
                {
                    int tempCount = i - pUpList.localSponsors.Length - pUpList.nationalSponsors.Length;
                    Debug.Log("i - pUpList.localSponsors.Length is " + tempCount);
                    idSponsorList[i] = pUpList.internationalSponsors[i - pUpList.localSponsors.Length - pUpList.nationalSponsors.Length].id;
                    cardsSponsor[i] = pUpList.internationalSponsors[i - pUpList.localSponsors.Length - pUpList.nationalSponsors.Length];
                }
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

        //Debug.Log("activeList Length is " + activeIdList.Length);

        if (cm.playedCardIDList.Length > 0)
        {
            playedIdList = cm.playedCardIDList;
        }

        //match the card ids to load a saved state

        for (int i = 0; i < idSponsorList.Length; i++)
        {
            for (int j = 0; j < pUpList.localSponsors.Length; j++)
            {
                if (idSponsorList[i] == pUpList.localSponsors[j].id)
                {
                    cardsSponsor[i] = pUpList.localSponsors[j];
                }
            }
            for (int j = 0; j < pUpList.nationalSponsors.Length; j++)
            {
                if (idSponsorList[i] == pUpList.nationalSponsors[j].id)
                {
                    cardsSponsor[i] = pUpList.nationalSponsors[j];
                }
            }
            for (int j = 0; j < pUpList.internationalSponsors.Length; j++)
            {
                if (idSponsorList[i] == pUpList.internationalSponsors[j].id)
                {
                    cardsSponsor[i] = pUpList.internationalSponsors[j];
                }
            }
        }

        Shuffle(cardsSponsor);
        Debug.Log("cardsSponsor Length is " + cardsSponsor.Length);
        //Determine which cards are active
        for (int i = 0; i < activeIdList.Length; i++)
        {
            if (activeIdList[i] == 99)
            {
                activeCards[i] = emptySponsorCard;
            }
            else
            {
                for (int j = 0; j < cardsSponsor.Length; j++)
                {
                    Debug.Log("activeIDList Length is " + activeIdList.Length);
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
                            ColorChanger(6, 1, i);
                        }
                    }
                }
            }
        }

        //Determine which cards have been played
        for (int i = 0; i < playedIdList.Length; i++)
        {
            for (int j = 0; j < cardsSponsor.Length; i++)
            {
                if (cardsSponsor[j].id == playedIdList[i])
                {
                    cardsSponsor[i].played = true;
                    playedCards.Add(cardsSponsor[j]);
                }
            }
        }

        //The rest are available
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
                    if (cardsSponsor[i].signConditionValue < cm.earnings)
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
            if (activeCards[i].duration == 1)
                cardDisplays[i].name.text = activeCards[i].name + " - " + activeCards[i].duration.ToString() + " week";
            else if (activeCards[i].duration > 0)
                cardDisplays[i].name.text = activeCards[i].name + " - " + activeCards[i].duration.ToString() + " weeks";
            else if (activeCards[i].duration > 50)
                cardDisplays[i].name.text = activeCards[i].name + " - " + " ongoing";
            else
                cardDisplays[i].name.text = "[" + activeCards[i].name + "]";

            if (activeCards[i].cost > 0)
            {
                if (activeCards[i].duration == 1)
                    cardDisplays[i].description.text = "$"
                        + activeCards[i].cost.ToString("n0") + " - "
                        + activeCards[i].description + " - "
                        + activeCards[i].duration.ToString()
                        + " week remaining";
                else if (activeCards[i].duration > 0)
                    cardDisplays[i].description.text = "$"
                        + activeCards[i].cost.ToString("n0") + " - "
                        + activeCards[i].description + " - "
                        + activeCards[i].duration.ToString()
                        + " weeks remaining";
            }
            else
                cardDisplays[i].description.text = " ";

            cardDisplays[i].cost.text = "$" + activeCards[i].cost.ToString("n0");
            cardDisplays[i].image.sprite = activeCards[i].image;

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
            costPerWeek -= activeCards[i].cost;
        }

        //Debug.Log("costPerWeek is " + costPerWeek);

        for (int i = 0; i < activeCards.Length; i++)
        {
            if (activeCards[i].id == 99)
            {
                ColorChanger(6, 1, i);
            }
            else
            {
                ColorChanger(7, 0, i);
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
            {
                BuyCard(availSponsorCards[card - 1].id, card);
                Debug.Log("availPUCards[card].id is " + availSponsorCards[card - 1].id);
            }

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
        cardDisplays[4].image.sprite = activeCards[card].image;

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
            replaceHeading.text = "Add Sponsor";
        }
        else
        {
            cardDisplays[4].image.gameObject.SetActive(true);
            cardDisplays[4].description.gameObject.SetActive(true);
            cardDisplays[4].cost.gameObject.SetActive(false);
            cardDisplays[4].effectSliders[0].transform.parent.gameObject.SetActive(true);
            cardDisplays[4].costPanel.SetActive(false);
            ColorChanger(7, 0, 4);
            replaceHeading.text = "Replace Sponsor";
        }


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
                cardDisplays[i].image.sprite = availSponsorCards[i - 5].image;

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
                    ColorChanger(7, 0, i);
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

    public void BuyCard(int cardSelected, int card)
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


                if (activeCards[i].duration == 1)
                    cardDisplays[i].description.text = "$"
                        + activeCards[i].cost.ToString("n0") + " - "
                        + activeCards[i].description + " - "
                        + activeCards[i].duration.ToString()
                        + " week remaining";
                else if (activeCards[i].duration > 0)
                    cardDisplays[i].description.text = "$"
                        + activeCards[i].cost.ToString("n0") + " - "
                        + activeCards[i].description + " - "
                        + activeCards[i].duration.ToString()
                        + " weeks remaining";

                cardDisplays[i].cost.text = "$" + activeCards[i].cost.ToString("n0");
                cardDisplays[i].image.sprite = activeCards[i].image;

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
                    cardDisplays[i].effectSliders[0].transform.parent.gameObject.SetActive(false);
                    cardDisplays[i].cost.gameObject.SetActive(false);
                    cardDisplays[i].costPanel.SetActive(false);
                }

                if (activeCards[i].id == 99)
                    ColorChanger(6, 1, i);
                else
                    ColorChanger(7, 0, i);
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
                cardDisplays[i].image.sprite = activeCards[i].image;

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
                costPerWeek -= activeCards[i].cost;

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
                    ColorChanger(7, 0, i);
                    cardDisplays[i].image.gameObject.SetActive(true);
                    cardDisplays[i].description.gameObject.SetActive(true);
                    cardDisplays[i].effectSliders[0].transform.parent.gameObject.SetActive(false);
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
