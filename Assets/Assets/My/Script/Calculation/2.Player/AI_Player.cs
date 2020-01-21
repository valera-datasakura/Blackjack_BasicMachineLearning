using UnityEngine;
using CardEnums;
using System.Collections.Generic;

public class AI_Player : MonoBehaviour
{
    [SerializeField] GameObject handPrefab;

    [HideInInspector] public float account;

    [HideInInspector] public List<AI_PlayerHand> hands = new List<AI_PlayerHand>();

    [HideInInspector] public int curHand_Idx;
    
    int _CCNumber = 0;

    public void Init()
    {
        AddHand();// 기본 핸드 하나
    }
    public void Collect()
    {
        for (int i = 0; i < hands.Count; ++i)
        {
            account += hands[i].AmountOfBetting;
            account += hands[i].AmountOfInsurance;
            
            float temp;
            hands[i].Collect(out temp); // 배팅금 삭제
        }
    }

    public int CCNumber {

        get {
            return Mathf.Clamp(_CCNumber, DB_Manager.MIN_CCN, DB_Manager.MAX_CCN);
        }
    }
    public int Real_CCNumber {

        get {
            return _CCNumber;
        }
    }
    public void AddCCNumber(int num)
    {
        _CCNumber += num;
    }
    public void Reset_CCNumber()
    {
        _CCNumber = 0;
    }

    // ____________________________Hand 관련_________________________________________
    public AI_PlayerHand GetCurrentHand
    {
        get
        {
            return hands[curHand_Idx];
        }
    }
    public bool IsFirstHand // 하위클래스Hand에서 블랙잭 판별하기 위해서(현재 상황에서 판단=GetCard에서 사용금지)
    {
        get
        {
            if (hands.Count == 1 && hands[0].GetCardCount <= 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public bool IsBlackjack
    {
        get
        {
            return hands[curHand_Idx].value == HAND_VALUE.BLACKJACK;
        }
    }
    public void ResetHandIndex()
    {
        curHand_Idx = 0;
    }
    public AI_PlayerHand AddHand()
    {
        GameObject newObj = (GameObject)Instantiate(handPrefab, Vector3.zero, Quaternion.identity);
        AI_PlayerHand newHand = newObj.GetComponent<AI_PlayerHand>();

        newHand.Init(transform.tag);
        hands.Add(newHand);
        
        return newHand;
    }
    public void GetCard(AI_Card card)
    {
        AI_PlayerHand hand = GetCurrentHand;
        
        bool isFirstHand = false;
        // 카드 추가전에 계산되기 때문에
        // 조건 수치를 2->1로 줄인다(origin=IsFirstHand)
        if (hands.Count == 1 && hands[0].GetCardCount <= 1)
        {
            isFirstHand = true;
        }

        hand.Push(card, isFirstHand); // (in)카드 하이라이트 +
    }
    public void DiscardAll(ref AI_DiscardDeck dDeck)// 카드 전달, 핸드 초기화(size 1)
    {
        // 카드 회수
        for (int i = 0; i < hands.Count; ++i)
        {
            hands[i].DiscardAll(ref dDeck);
        }
        
        // 추가된 핸드 삭제
        if (hands.Count > 1)
        {
            for (int i = 1; i < hands.Count; ++i)
            {
                Destroy(hands[i].gameObject);
            }

            hands.RemoveRange(1, hands.Count - 1);
        }
    }
    public bool UpdateToNextHand()
    {
        ++curHand_Idx;

        if (curHand_Idx < hands.Count)
        {
            return true;
        }
        else
        {
            curHand_Idx = 0;
            return false;
        }
    }
    public bool UpdateAndCheckAllPossibleHand()
    {
        ++curHand_Idx;
        
        while (curHand_Idx < hands.Count)
        {
            if (GetCurrentHand.CanChoose)
            {
                return true;
            }

            ++curHand_Idx;
        }

        curHand_Idx = 0;
        return false;
    }
    public void DoubleDown()
    {
        GetCurrentHand.DoubleDownBetting();
    }
    public AI_PlayerHand Split(bool isAce)
    {
        AI_Card card = GetCurrentHand.Pop();
        float bet = GetCurrentHand.AmountOfBetting;
        
        AI_PlayerHand splitHand = AddHand();
        
        if (isAce)
        {
            GetCurrentHand.SetSplitAce();
            splitHand.SetSplitAce();
        }

        // 그 핸드에 정보 전달
        splitHand.Push(card, false);
        splitHand.Betting();

        return splitHand;
    }
}
