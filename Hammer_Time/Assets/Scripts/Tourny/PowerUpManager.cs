using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PowerUpManager : MonoBehaviour
{
    CareerManager cm;
    public int[] idList;
    public Card[] cards;
    public Card[] availCards;

    public List<Card> playerCards;

    public CardDisplay[] cardDisplays;
    public GameObject cardParent;
    public GameObject cardPrefab;
    public GameObject[] cardGOs;

    public Card[] inPlayCards;

    public Scrollbar scrollbar;
    public PowerUpList pUpList;
    public GameObject mainMenu;

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
        //for (int i = 0; i < numberOfCards; i++)
        //{
        //    cardGOs[i] = Instantiate(cardPrefab, cardParent.transform);
        //}

        cm = FindObjectOfType<CareerManager>();


        playerCards = new List<Card>();
        cards = pUpList.powerUps;
        
        Shuffle(cards);

        availCards = new Card[3];
        for (int i = 0; i < 3; i++)
        {
            availCards[i] = cards[i];
        }

        DisplayCards();
    }

    // Update is called once per frame
    void Update()
    {
        drawSlider.value = cm.cStats.drawAccuracy;
        guardSlider.value = cm.cStats.guardAccuracy;
        takeOutSlider.value = cm.cStats.takeOutAccuracy;
        enduranceSlider.value = cm.cStats.sweepEndurance;
        strengthSlider.value = cm.cStats.sweepStrength;
        healthSlider.value = cm.cStats.sweepHealth;

        oppDrawSlider.value = 5 + cm.oppStats.drawAccuracy;
        oppGuardSlider.value = 5 + cm.oppStats.guardAccuracy;
        oppTakeOutSlider.value = 5 + cm.oppStats.takeOutAccuracy;
        oppEnduranceSlider.value = 5 + cm.oppStats.sweepEndurance;
        oppStrengthSlider.value = 5 + cm.oppStats.sweepStrength;
        oppHealthSlider.value = 50 + cm.oppStats.sweepHealth;

        xp = cm.xp;
        cash = cm.earnings;
        xpText.text = xp.ToString();
        cashText.text = "$" + cash.ToString("n0");
    }

    public void BuyCard(int card)
    {
        cm = FindObjectOfType<CareerManager>();

        cm.earnings -= availCards[card].cost;
        availCards[card].active = true;
        playerCards.Add(availCards[card]);
        Debug.Log("Buying " + availCards[card].name + " for $" + availCards[card].cost + ", money left is " + cm.earnings);
        AssignPoints(card);
    }
    public void AssignPoints(int card)
    {
        cm = FindObjectOfType<CareerManager>();

        //cm.cStats.drawAccuracy += availCards[card].draw;
        //cm.cStats.guardAccuracy += availCards[card].guard;
        //cm.cStats.takeOutAccuracy += availCards[card].takeOut;
        //cm.cStats.sweepEndurance += availCards[card].sweepEnduro;
        //cm.cStats.sweepStrength += availCards[card].sweepStrength;
        //cm.cStats.sweepHealth += (10f * availCards[card].sweepHealth);
        //cm.oppStats.drawAccuracy += availCards[card].oppDraw;
        //cm.oppStats.guardAccuracy += availCards[card].oppGuard;
        //cm.oppStats.takeOutAccuracy += availCards[card].oppTakeOut;
        //cm.oppStats.sweepEndurance += availCards[card].oppEnduro;
        //cm.oppStats.sweepStrength += availCards[card].oppStrength;
        //cm.oppStats.sweepHealth += (10f * availCards[card].oppHealth);

        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void PreviewPoints(int card)
    {
        cm = FindObjectOfType<CareerManager>();

        cm.cStats.drawAccuracy += availCards[card].draw;
        cm.cStats.guardAccuracy += availCards[card].guard;
        cm.cStats.takeOutAccuracy += availCards[card].takeOut;
        cm.cStats.sweepEndurance += availCards[card].sweepEnduro;
        cm.cStats.sweepStrength += availCards[card].sweepStrength;
        cm.cStats.sweepHealth += (10f * availCards[card].sweepHealth);
        cm.oppStats.drawAccuracy += availCards[card].oppDraw;
        cm.oppStats.guardAccuracy += availCards[card].oppGuard;
        cm.oppStats.takeOutAccuracy += availCards[card].oppTakeOut;
        cm.oppStats.sweepEndurance += availCards[card].oppEnduro;
        cm.oppStats.sweepStrength += availCards[card].oppStrength;
        cm.oppStats.sweepHealth += (10f * availCards[card].oppHealth);

        switch(card)
        {
            case 0:
                scrollbar.value = -2f;
                break;
            case 1:
                scrollbar.value = 0.5f;
                break;
            case 2:
                scrollbar.value = 2.65f;
                break;
        }
    }

    public void UnPreviewPoints(int card)
    {
        cm = FindObjectOfType<CareerManager>();

        cm.cStats.drawAccuracy -= availCards[card].draw;
        cm.cStats.guardAccuracy -= availCards[card].guard;
        cm.cStats.takeOutAccuracy -= availCards[card].takeOut;
        cm.cStats.sweepEndurance -= availCards[card].sweepEnduro;
        cm.cStats.sweepStrength -= availCards[card].sweepStrength;
        cm.cStats.sweepHealth -= (10f * availCards[card].sweepHealth);
        cm.oppStats.drawAccuracy -= availCards[card].oppDraw;
        cm.oppStats.guardAccuracy -= availCards[card].oppGuard;
        cm.oppStats.takeOutAccuracy -= availCards[card].oppTakeOut;
        cm.oppStats.sweepEndurance -= availCards[card].oppEnduro;
        cm.oppStats.sweepStrength -= availCards[card].oppStrength;
        cm.oppStats.sweepHealth -= (10f * availCards[card].oppHealth);
    }

    void DisplayCards()
    {
        for (int i = 0; i < availCards.Length; i++)
        {
            cardDisplays[i].name.text = cards[i].name;
            cardDisplays[i].description.text = cards[i].description;
            cardDisplays[i].effect.text = " ";
            cardDisplays[i].cost.text = "$" + cards[i].cost.ToString("n0");
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
