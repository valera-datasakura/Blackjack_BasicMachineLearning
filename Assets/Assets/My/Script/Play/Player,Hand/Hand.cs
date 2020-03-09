using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CardEnums;


public class Hand : MonoBehaviour {

    public GameObject statePrefab;

    protected List<Card> cards = new List<Card>();
    protected HAND_VALUE value = HAND_VALUE.NOTHING;
    protected bool isSoft = false;

    //_______________________________________Card관련________________________________

    public bool IsSoft {
        get {
            return isSoft;
        }
    }
    public bool IsHard {
        get {
            return !isSoft;
        }
    }

    public static bool operator <(Hand hand1, Hand hand2)
    {
        if (hand1.Value < hand2.Value)
        {
            return true;
        }
        return false;
    }
    public static bool operator >(Hand hand1, Hand hand2)
    {
        if (hand1.Value > hand2.Value)
        {
            return true;
        }
        return false;
    }
    public static bool operator !=(Hand hand1, Hand hand2)
    {
        if (hand1.Value != hand2.Value)
        {
            return true;
        }
        return false;
    }
    public static bool operator ==(Hand hand1, Hand hand2)
    {
        if (hand1.Value == hand2.Value)
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

    public virtual void Push(Card card, bool isFirstHand)
    {
        card.transform.SetParent(this.transform);
        card.AddOffset(positionOffset * cards.Count);
        cards.Add(card);
        
        CheckValue(isFirstHand);
    }
    public virtual Card Pop()
    {
        Card popCard = cards[cards.Count - 1];
        cards.RemoveAt(cards.Count - 1);

        CheckValue(false);

        return popCard;
    }
    public virtual void MoveAll(Transform toTrans, float delay)
    {
        for (int i = 0; i < this.cards.Count; ++i)
        {
            this.cards[i].Move(toTrans, delay);
            this.cards[i].Rotate(delay);
            this.cards[i].transform.SetParent(null);
        }
        
        stateUI.gameObject.SetActive(false);
    }
    public virtual void DiscardAll(DiscardDeck ddeck) // 이동 끝
    {
        for (int i = 0; i < this.cards.Count; ++i)
        {
            ddeck.Discard(this.cards[i]);
        }
        this.cards.Clear();
    } 
    //______________________________________Value관련_________________________________
    public HAND_VALUE Value
    {
        get
        {
            return value;
        }
    }
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

    private void CheckValue(bool isFirstHand)
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
            total += cards[i].Number;

            if (cards[i].Number == 1)
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

    #region rest

    public Vector3 positionOffset;

    protected UISprite stateUI;

    // 딜레이 관련
    protected bool isStateUIAfter = false;
    protected float currentStateUITimeAfter;
    protected float totalStateUITimeAfter;

    public virtual void Init(UIPanel stateUIParent, string tag)
    {
        GameObject newObj =
            (GameObject)Instantiate(statePrefab, GetMyWorldToUIScreen(), Quaternion.identity);

        stateUI = newObj.GetComponent<UISprite>();
        stateUI.transform.SetParent(stateUIParent.transform);
        stateUI.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        stateUI.gameObject.SetActive(false);

        transform.tag = tag;
    }

    public void DisplayStateUI(float delay)// 컨트롤러에서 시작할때, 플레이어에서 인덱스 넘길때 사용
    {
        isStateUIAfter = true;
        currentStateUITimeAfter = 0.0f;
        totalStateUITimeAfter = delay;
    }
    protected Vector3 GetMyWorldToUIScreen()
    {
        Vector3 screenPos = 
            Camera.main.WorldToScreenPoint(transform.position);
        Vector3 UICamPos = 
            UICamera.mainCamera.ScreenToWorldPoint(screenPos);

        return UICamPos;
    }
    protected void DestroyStateUI()
    {
        Destroy(stateUI.gameObject);
    }
    #endregion
}
