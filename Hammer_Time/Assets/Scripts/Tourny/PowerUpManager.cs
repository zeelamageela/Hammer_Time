using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PowerUpManager : MonoBehaviour
{
    CareerManager cm;
    public TournySelector tSel;

    public int[] idList;
    public Card[] cards;
    public Card[] availCards;

    public List<Card> playerCards;

    public CardDisplay[] cardDisplays;
    public GameObject cardParent;
    public GameObject cardPrefab;
    public GameObject playerCardPrefab;
    public GameObject[] cardGOs;

    public Card[] inPlayCards;

    public Scrollbar scrollbar;
    public PowerUpList pUpList;
    public GameObject mainMenu;
    public GameObject infoPanel;
    public Text nextWeekButtonText;
    public GameObject nextWeekButton;
    public GameObject contButton;
    public GameObject backButton;

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

    bool drag;
    int oppStatBase;
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
        cm = FindObjectOfType<CareerManager>();
        tSel = FindObjectOfType<TournySelector>();

        if (cm.week < 4)
        {
            idList = new int[20];
            for (int i = 0; i < idList.Length; i++)
            {
                idList[i] = 99;
            }
            cm.cardIDList = idList;
            //StartCoroutine(WaitForClick());
            contButton.SetActive(false);
            infoPanel.SetActive(true);
            
            tSel.SetUp();
            profileButton.interactable = true;
            mainMenu.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
            idList = cm.cardIDList;
            contButton.SetActive(false);
            infoPanel.SetActive(true);
            profileButton.interactable = false;
            nextWeekButtonText.text = "Next Week>";
        }


        cardGOs = new GameObject[numberOfCards];
        cardDisplays = new CardDisplay[numberOfCards];
        availCards = new Card[numberOfCards];
        playerCards = new List<Card>();
        cards = pUpList.powerUps;

        Debug.Log("Cards Length is " + cards.Length);

        for (int i = 0; i < numberOfCards; i++)
        {
            Debug.Log("i is " + i);
            cardGOs[i] = Instantiate(cardPrefab, cardParent.transform);

            cardDisplays[i] = cardGOs[i].GetComponent<Card_Select>().cardDisplay;
            cardGOs[i].GetComponent<Card_Select>().cardIndex = i;

            for (int j = 0; j < idList.Length; j++)
            {
                if (cards[i].id == idList[j])
                    cards[i].active = true;
            }
        }

        Shuffle(cards);

        for (int i = 0; i < numberOfCards; i++)
        {
            Debug.Log("i is " + i + " - cards is " + cards[i].name);

            availCards[i] = cards[i];
            cardGOs[i].name = cards[i].name;

        }
        scrollbar.value = 0f;

        DisplayCards();
            
    }

    // Update is called once per frame
    void Update()
    {
        if (cm.week < 5)
            oppStatBase = 5;
        else if (cm.week < 10)
            oppStatBase = 7;
        else
            oppStatBase = 10;

        if (cm)
        {
            drawSlider.value = cm.cStats.drawAccuracy + cm.modStats.drawAccuracy;
            guardSlider.value = cm.cStats.guardAccuracy + cm.modStats.guardAccuracy;
            takeOutSlider.value = cm.cStats.takeOutAccuracy + cm.modStats.takeOutAccuracy;
            enduranceSlider.value = cm.cStats.sweepEndurance + cm.modStats.sweepEndurance;
            strengthSlider.value = cm.cStats.sweepStrength + cm.modStats.sweepStrength;
            healthSlider.value = cm.cStats.sweepHealth + cm.modStats.sweepHealth;

            oppDrawSlider.value = oppStatBase + cm.oppStats.drawAccuracy;
            oppGuardSlider.value = oppStatBase + cm.oppStats.guardAccuracy;
            oppTakeOutSlider.value = oppStatBase + cm.oppStats.takeOutAccuracy;
            oppEnduranceSlider.value = oppStatBase + cm.oppStats.sweepEndurance;
            oppStrengthSlider.value = oppStatBase + cm.oppStats.sweepStrength;
            oppHealthSlider.value = (oppStatBase * 10) + cm.oppStats.sweepHealth;

            xp = cm.xp;
            cash = cm.earnings;
            xpText.text = xp.ToString();
            cashText.text = "$" + cash.ToString("n0");
        }
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

    public void ViewCards()
    {
        tSel = FindObjectOfType<TournySelector>();
        tSel.PowerUp(true);

        for (int i = 0; i < cardGOs.Length; i++)
        {
            Destroy(cardGOs[i]);
        }

        nextWeekButton.SetActive(false);
        backButton.SetActive(true);
        for (int i = 0; i < playerCards.Count; i++)
        {
            Debug.Log("i is " + i);
            cardGOs[i] = Instantiate(playerCardPrefab, cardParent.transform);

            cardDisplays[i] = cardGOs[i].GetComponent<Card_Select>().cardDisplay;
            cardGOs[i].GetComponent<Card_Select>().cardIndex = i;

            for (int j = 0; j < idList.Length; j++)
            {
                if (cards[i].id == idList[j])
                    cards[i].active = true;
            }
        }

        for (int i = 0; i < playerCards.Count; i++)
        {
            Debug.Log("i is " + i + " - cards is " + cards[i].name);

            availCards[i] = playerCards[i];
            cardGOs[i].name = playerCards[i].name;
        }

        nextWeekButtonText.text = "<Back";
        scrollbar.value = 0f;

        DisplayCards();
    }

    public void Continue(bool back)
    {
        if (!back)
        {
            //cm.SaveCareer();
            tSel.SetUp();

        }
        profileButton.interactable = true;
        mainMenu.SetActive(true);
        gameObject.SetActive(false);

    }

    IEnumerator WaitForClick()
    {
        tSel = FindObjectOfType<TournySelector>();

        //yield return new WaitForSeconds(3f);
        tSel.SetUp();
        profileButton.interactable = true;
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
        yield return null;
        Debug.Log("Clickeddd");
    }

    public void AssignPoints(int card)
    {
        cm = FindObjectOfType<CareerManager>();

        //contButton.SetActive(true);
        //infoPanel.SetActive(false);

        Debug.Log("id of played card is " + idList[cm.week]);
        Debug.Log("availcards length is " + availCards.Length);
        //int idListLength = 0;

        idList[cm.week] = availCards[card].id;
        //for (int i = 0; i < numberOfCards; i++)
        //{
        //    if (availCards[i].active)
        //        idList[cm.week] = availCards[i].id;
        //}
    }

    public void PreviewPoints(int card)
    {
        cm = FindObjectOfType<CareerManager>();

        cm.modStats.drawAccuracy += availCards[card].draw;
        cm.modStats.guardAccuracy += availCards[card].guard;
        cm.modStats.takeOutAccuracy += availCards[card].takeOut;
        cm.modStats.sweepEndurance += availCards[card].sweepEnduro;
        cm.modStats.sweepStrength += availCards[card].sweepStrength;
        cm.modStats.sweepHealth += (10f * availCards[card].sweepHealth);
        cm.oppStats.drawAccuracy += availCards[card].oppDraw;
        cm.oppStats.guardAccuracy += availCards[card].oppGuard;
        cm.oppStats.takeOutAccuracy += availCards[card].oppTakeOut;
        cm.oppStats.sweepEndurance += availCards[card].oppEnduro;
        cm.oppStats.sweepStrength += availCards[card].oppStrength;
        cm.oppStats.sweepHealth += (10f * availCards[card].oppHealth);

        Debug.Log("Card is " + card + " and the number of Cards is " + numberOfCards + " - " + card / numberOfCards);
        Debug.Log("Card / 2 is " + numberOfCards / 2f);


        //scrollbar.value = Mathf.Lerp((float)card / (numberOfCards - 1f), tempScrollValue, 2 * Time.deltaTime);
        Debug.Log("Scrollbar value is " + scrollbar.value);
    }

    public void UnPreviewPoints(int card)
    {
        cm = FindObjectOfType<CareerManager>();
        DisplayCards();
        cm.modStats.drawAccuracy -= availCards[card].draw;
        cm.modStats.guardAccuracy -= availCards[card].guard;
        cm.modStats.takeOutAccuracy -= availCards[card].takeOut;
        cm.modStats.sweepEndurance -= availCards[card].sweepEnduro;
        cm.modStats.sweepStrength -= availCards[card].sweepStrength;
        cm.modStats.sweepHealth -= (10f * availCards[card].sweepHealth);
        cm.oppStats.drawAccuracy -= availCards[card].oppDraw;
        cm.oppStats.guardAccuracy -= availCards[card].oppGuard;
        cm.oppStats.takeOutAccuracy -= availCards[card].oppTakeOut;
        cm.oppStats.sweepEndurance -= availCards[card].oppEnduro;
        cm.oppStats.sweepStrength -= availCards[card].oppStrength;
        cm.oppStats.sweepHealth -= (10f * availCards[card].oppHealth);
    }

    void DisplayCards()
    {
        int counter = 0;
        for (int i = 0; i < numberOfCards; i++)
        {
            cardDisplays[i].name.text = availCards[i].name;
            cardDisplays[i].description.text = availCards[i].description;
            cardDisplays[i].effect.text = " ";
            cardDisplays[i].cost.text = "$" + availCards[i].cost.ToString("n0");

            for (int j = 0; j < cards[i].effects.Length; j++)
            {
                cardDisplays[i].effect.text += "\n" + availCards[i].effects[j];
            }

            if (availCards[i].active)
            {
                cardDisplays[i].costPanel.SetActive(false);
                counter++;
            }
        }

        for (int i = 0; i < numberOfCards; i++)
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
}
