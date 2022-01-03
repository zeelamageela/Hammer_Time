using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public int[] idList;
    public Card[] cards;
    public Card[] activeCards;
    public CardDisplay[] cardDisplays;
    public GameObject[] cardGOs;

    public Card[] inPlayCards;

    public PowerUpList pUpList;

    //public static PowerUpManager instance;

    //void Awake()
    //{
    //    DontDestroyOnLoad(gameObject);

    //    if (instance == null)
    //    {
    //        instance = this;
    //    }
    //    else
    //    {
    //        Destroy(gameObject);
    //        return;
    //    }

    //    Application.targetFrameRate = 30;
    //}

    // Start is called before the first frame update
    void Start()
    {
        cards = pUpList.powerUps;

        Shuffle(cards);

        activeCards = new Card[3];
        for (int i = 0; i < 3; i++)
        {
            activeCards[i] = cards[i];
        }

        DisplayCards();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AssignPoints(int card)
    {
        CareerManager cm = FindObjectOfType<CareerManager>();
        cm.cStats.drawAccuracy += activeCards[card].draw;
        cm.cStats.guardAccuracy += activeCards[card].guard;
        cm.cStats.takeOutAccuracy += activeCards[card].takeOut;
        cm.cStats.sweepEndurance += activeCards[card].sweepEnduro;
        cm.cStats.sweepStrength += activeCards[card].sweepStrength;
        cm.cStats.sweepHealth += (10f * activeCards[card].sweepHealth);
        cm.oppStats.drawAccuracy += activeCards[card].oppDraw;
        cm.oppStats.guardAccuracy += activeCards[card].oppGuard;
        cm.oppStats.takeOutAccuracy += activeCards[card].oppTakeOut;
        cm.oppStats.sweepEndurance += activeCards[card].oppEnduro;
        cm.oppStats.sweepStrength += activeCards[card].oppStrength;
        cm.oppStats.sweepHealth += (10f * activeCards[card].oppHealth);
    }

    void EffectConvert(string stat, int value)
    {
        switch (stat)
        {
            case "Draw":
                
                break;
        }
    }

    void DisplayCards()
    {
        for (int i = 0; i < activeCards.Length; i++)
        {
            cardDisplays[i].name.text = cards[i].name;
            cardDisplays[i].description.text = cards[i].description;
            cardDisplays[i].effect.text = " ";
            for (int j = 0; j < cards[i].effects.Length; j++)
            {
                cardDisplays[i].effect.text += "\n" + cards[i].effects[j];
            }
        }
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
}
