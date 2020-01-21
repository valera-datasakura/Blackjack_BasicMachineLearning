using UnityEngine;
using CardEnums;

public class DealerHand : Hand {

    //_________________________________CallBack_____________________________
    public override void Init(GameObject statePrefab, UIPanel stateParent, string _tag)
    {
        base.Init(statePrefab, stateParent, _tag);

        stateUI.transform.Translate(0f, 0.3f, 0f);
    }

    void Update()
    {
        // State 업데이트
        if (isStateUIAfter)
        {
            currentStateUITimeAfter += Time.deltaTime;

            if (currentStateUITimeAfter >= totalStateUITimeAfter)
            {
                isStateUIAfter = false;

                // 상태
                SetStateUI();
            }
        }
    }

    //___________________________________카드 관련____________________________
    public bool IsStopHitting
    {
        get
        {
            if (value >= (HAND_VALUE)17 || value == HAND_VALUE.BURST_DEALER)
            {
                return true;
            }

            return false;
        }
    }
    public bool IsInsuranceHand
    {
        get
        {
            if (cards.Count == 2 && cards[1].Number == 1)
            {
                return true;
            }

            return false;
        }
    }
    public int GetIndexOfOpen
    {
        get { return cards[1].Number-1; }
    }
    public void GetOpenCard(Card card, float delay)
    {
        card.Move(transform, delay);
        card.Rotate(delay);

        bool isFirstHand = false;
        if (cards.Count < 2)
        {
            isFirstHand = true;
        }

        Push(card, isFirstHand);
    }
    public void GetHiddenCard(Card card, float delay)
    {
        card.Move(transform, delay);

        bool isFirstHand = false;
        if (cards.Count < 2)
        {
            isFirstHand = true;
        }

        Push(card, isFirstHand);
    }
    public override void DiscardAll(DiscardDeck ddeck) // 이동 끝
    {
        base.DiscardAll(ddeck);

        stateUI.gameObject.SetActive(false);
    }
    public void OpenHole(float delay)
    {
        //if (cards.Count != 2)
        //{
        //    Debug.Log("뭔가 이상한거 같은데 in OpenHole() of DealerHand");
        //}

        cards[0].Rotate(delay);
    }
    
    //________________________________________UI 관련________________________
    void SetStateUI()
    {
        switch (value)
        {
            case HAND_VALUE.BURST_DEALER:
                stateUI.spriteName = "Burst";
                stateUI.gameObject.SetActive(true);
                break;
            case HAND_VALUE.BLACKJACK:
                stateUI.spriteName = "Blackjack";
                stateUI.gameObject.SetActive(true);
                break;
        }
    }
}
