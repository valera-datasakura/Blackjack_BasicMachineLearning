using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI_Deck : MonoBehaviour
{
    public GameObject card;
    
    float shuffleTimeOfRemainingPercentage = 0.25f;
    int numOfDeck = 6;

    List<AI_Card> cards;

    //_______________________Initialize_______________________________
    void Start()
    {

        cards = new List<AI_Card>();

        InitCard();
    }

    //________________________________Rest________________________________
    public bool IsShuffleTime
    {
        get
        {
            float curRemainingPtg = cards.Count / (numOfDeck * 52.0f);
           
            if (curRemainingPtg <= shuffleTimeOfRemainingPercentage)
            {
                return true;
            }
            return false;
        }
    }

    void InitCard()
    {
        for (int j = 0; j < numOfDeck; ++j)
        {
            int cardNum = 0; // 변경용 조건문이 대입문보다 앞에 있기때문에 0부터 시작

            for (int i = 0; i < 52; ++i)
            {
                GameObject newObj = (GameObject)Instantiate(card, Vector3.zero, Quaternion.identity);
                AI_Card newCard = newObj.AddComponent<AI_Card>();
                if (i % 4 == 0 && cardNum < 10)
                {
                    ++cardNum;
                }
                newCard.Init(cardNum);
                cards.Add(newCard);
            }
        }
    }

    public void Push(AI_Card card)
    {
        cards.Add(card);
    }
    public AI_Card Pop()
    {
        int randIdx = Random.Range(0, cards.Count);
        
        AI_Card returnCard = cards[randIdx];
        
        cards.RemoveAt(randIdx);

        return returnCard;
    }
    public void Shuffle(AI_DiscardDeck dDeck)
    {
        dDeck.ReturnAll(this);
    }
}
