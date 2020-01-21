using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deck : MonoBehaviour {

    public GameObject[] cards; // 오름차순 !!

    public Transform spawnTransform;

    float shuffleTimeOfRemainingPercentage = 0.25f;
    int numOfDeck = 6;

    List<Card> decks;
    
	void Start () {

        decks = new List<Card>();

        InitCard();

        // 랜덤으로 뽑아오기때문에 셔플할 필요없음
	}
    //------------------------Property----------------------------------
    public bool IsShuffleTime
    {
        get
        {
            float curRemainingPtg = decks.Count / (numOfDeck * 52.0f);
            if (curRemainingPtg <= shuffleTimeOfRemainingPercentage)
            {
                return true;
            }
            return false;
        }
    }


    //------------------------Function---------------------------------------
    void InitCard()
    {
        for (int j = 0; j < numOfDeck; ++j)
        {
            int cardNum = 0; // 변경용 조건문이 대입문보다 앞에 있기때문에 0부터 시작

            for (int i = 0; i < cards.Length; ++i)
            {
                GameObject newObj = (GameObject)Instantiate(cards[i], spawnTransform.position, spawnTransform.rotation);
                Card newCard = newObj.AddComponent<Card>();
                if (i % 4 == 0 && cardNum < 10)
                {
                    ++cardNum;
                }
                newCard.Setting(cardNum);
                newCard.gameObject.SetActive(false);
                decks.Add(newCard);
            }
        }
    }

	public void Push(Card card)
    {
        //if (decks.Contains(card))
        //{
        //    Debug.Log("이미 카드가 존재한다 in Push() of Deck");
        //}

        //card.gameObject.SetActive(false); DiscardDeck을 거치기때문에 생략
        card.transform.position = spawnTransform.position;
        card.transform.rotation = spawnTransform.rotation;
        decks.Add(card);
    }
    public Card Pop()
    {
        int randIdx = Random.Range(0, decks.Count);

        Card returnCard = decks[randIdx];
        returnCard.gameObject.SetActive(true);

        decks.RemoveAt(randIdx);

        return returnCard;
    }
    public void Shuffle(DiscardDeck dDeck, float time)
    {
        dDeck.ReturnAll(this);

        dDeck.EmptyStackSmooth(time);
    }
}
