using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Card_List : IComparable<Card_List>
{
    public Card card;

    public Card_List(Card newCard)
    {
        card = newCard;
    }

    public int CompareTo(Card_List other)
    {
        if (other == null)
        {
            return 1;
        }
        else if (card.id < other.card.id)
        {
            return 1;
        }
        else if (card.id >= other.card.id)
        {
            return -1;
        }
        else return 0;
    }
}
