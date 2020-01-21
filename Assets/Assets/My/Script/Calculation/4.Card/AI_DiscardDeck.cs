using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI_DiscardDeck : MonoBehaviour
{
    List<AI_Card> discards;
    
    void Start()
    {
        discards = new List<AI_Card>();
    }

    public void Discard(AI_Card card)
    {
        //if (discards.Contains(card))
        //{
        //    Debug.Log("이미 카드가 존재한다 in Push() of Deck");
        //}
        
        discards.Add(card);
    }
    public void ReturnAll(AI_Deck deck)
    {
        //if (discards.Count == 0)
        //{
        //    Debug.Log("카드가 없다 in ReturnAll() of CalcDiscardDeck");
        //}

        for (int i = 0; i < discards.Count; ++i)
        {
            deck.Push(discards[i]);
        }

        discards.Clear();
    }
}
