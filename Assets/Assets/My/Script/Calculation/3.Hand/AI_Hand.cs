using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CardEnums;

public class AI_Hand : MonoBehaviour
{
    public HAND_VALUE value = HAND_VALUE.NOTHING;

    public List<AI_Card> cards = new List<AI_Card>();
    public bool isSoft = false;
    
    //_______________________________________Cards________________________________
    void CheckValue(bool isFirstHand)
    {
        if (cards.Count <= 1)
        {
            value = HAND_VALUE.NOTHING;
            return;
        }

        int total = 0;
        isSoft = false;
        bool isSoftTemp = false;
        for (int i = 0; i < cards.Count; ++i)
        {
            total += cards[i].number;
            if (cards[i].number == 1)
            {
                isSoftTemp = true;
            }
        }

        if (total > 21)
        {
            if (transform.CompareTag("Player"))
            {
                value = HAND_VALUE.BURST_PLAYER;
            }
            else if (transform.CompareTag("Dealer"))
            {
                value = HAND_VALUE.BURST_DEALER;
            }
        }
        else
        {
            if (!isSoftTemp)// 하드
            {
                value = (HAND_VALUE)total;
            }
            else // Soft
            {
                if (total > 11)
                {
                    value = (HAND_VALUE)total;
                }
                else
                {
                    total += 10;
                    isSoft = true;
                    if (isFirstHand && total == 21 && cards.Count == 2) // Blackjack
                    {
                        value = HAND_VALUE.BLACKJACK;
                    }
                    else
                    {
                        value = (HAND_VALUE)total;
                    }
                }
            }
        }
    }

    public static bool operator <(AI_Hand hand1, AI_Hand hand2)
    {
        if (hand1.value < hand2.value)
        {
            return true;
        }
        return false;
    }
    public static bool operator >(AI_Hand hand1, AI_Hand hand2)
    {
        if (hand1.value > hand2.value)
        {
            return true;
        }
        return false;
    }
    public static bool operator !=(AI_Hand hand1, AI_Hand hand2)
    {
        if (hand1.value != hand2.value)
        {
            return true;
        }
        return false;
    }
    public static bool operator ==(AI_Hand hand1, AI_Hand hand2)
    {
        if (hand1.value == hand2.value)
        {
            return true;
        }
        return false;
    }

    public int GetCardCount
    {
        get
        {
            return cards.Count;
        }
    }

    public virtual void Push(AI_Card card, bool isFirstHand)
    {
        cards.Add(card);

        CheckValue(isFirstHand);
    }
    public virtual AI_Card Pop()
    {
        AI_Card popCard = cards[cards.Count - 1];
        cards.RemoveAt(cards.Count - 1);
        
        return popCard;
    }
    
    public virtual void DiscardAll(ref AI_DiscardDeck ddeck)
    {
        for (int i = 0; i < this.cards.Count; ++i)
        {
            ddeck.Discard(this.cards[i]);
        }
        this.cards.Clear();
    }
    //______________________________________Values_________________________________
    public bool IsBlackjack
    {
        get
        {
            return (value == HAND_VALUE.BLACKJACK) ? true : false;
        }
    }
    public bool IsBurst
    {
        get
        {
            return (value == HAND_VALUE.BURST_DEALER || value == HAND_VALUE.BURST_PLAYER) ? true : false;
        }
    }
    public bool IsSoft{get{return isSoft;}}
    public bool IsHard{get { return !isSoft;}}

    #region rest

    public virtual void Init(string tag)
    {
        transform.tag = tag;
    }

    #endregion
}
