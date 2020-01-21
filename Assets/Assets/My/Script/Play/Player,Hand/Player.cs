using UnityEngine;
using CardEnums;
using System.Collections.Generic;

public class Player : MonoBehaviour {
    
    public Transform insuranceBetTransform;
    GameObject handPrefab;
    Account account;
    List<PlayerHand> hands;

    //------------UI(범용)------------------
    UISprite accountWindow;
    UILabel nameLabel;
    UILabel accountLabel;

    GameObject stateUIPrefab;
    UIPanel stateUIParent;
    //--------------------------------------
    
    int currentHandIndex;
    float splitDistance = 0.7f;

    //____________________________________Initialize____________________________________________________
    public void Awake()
    {
        hands = new List<PlayerHand>();

        account = GetComponent<Account>();
    }
    public void Init(UI_ACCOUNT _account, GameObject _handPf, GameObject _statePf, UIPanel _stateParent)
    {
        account.AmountOfAccount = 1000;

        accountWindow = _account.accountWindow;
        nameLabel = _account.nameLabel;
        accountLabel = _account.accountLabel;

        handPrefab = _handPf;
        stateUIPrefab = _statePf;
        stateUIParent = _stateParent;

        AddHand();// 기본 핸드 하나

        SetAccountLabel(); // 라벨 설정
    }
    //____________________________________Money 관련______________________________________
    public int Budget
    {
        get
        {
            return account.AmountOfAccount;
        }
        set
        {
            account.AmountOfAccount = value;
            SetAccountLabel();
        }
    }
    public bool EnoughBetToDouble
    {
        get
        {
            return Budget >= GetCurrentHand.AmountOfBetting;
        }
    }
    public bool EnoughBetToSplit
    {
        get
        {
            return Budget >= GetCurrentHand.AmountOfBetting;
        }
    }
    public bool EnoughBetToInsurance
    {
        get
        {
            return Budget >= GetCurrentHand.AmountOfBetting / 2;
        }
    }
    public void CollectBet()
    {
        for (int i = 0; i < hands.Count; ++i)
        {
            Budget += hands[i].AmountOfBetting;
            Budget += hands[i].AmountOfInsurance;

            hands[i].LoseBet(BET_KIND.ORIGINAL);
            hands[i].LoseBet(BET_KIND.INSURANCE);
        }
    }
    
    // ____________________________Hand 관련_________________________________________
    public PlayerHand GetCurrentHand
    {
        get
        {
            return hands[currentHandIndex];
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
            return hands[currentHandIndex].Value == HAND_VALUE.BLACKJACK;
        }
    }
    public void ReadyForIteratingHand()
    {
        currentHandIndex = 0;
    }
    public PlayerHand AddHand()
    {
        GameObject newObj = (GameObject)Instantiate(handPrefab, transform.position, transform.rotation);
        newObj.transform.SetParent(this.transform);
        PlayerHand newHand = newObj.GetComponent<PlayerHand>();

        newHand.Init(stateUIPrefab, stateUIParent, insuranceBetTransform, transform.tag);
        hands.Add(newHand);
        SetHandsPosition();

        return newHand;
    }
    void SetHandsPosition()
    {
        Vector3 offset = new Vector3(splitDistance, 0.0f, 0.0f);
        Vector3 firstPos = offset*(hands.Count-1)/2.0f;// 오른쪽이 선이기 때문에 더해준다. 다음 반복문에서 빼줌
        
        for(int i = 0; i < hands.Count; ++i)
        {
            hands[i].transform.localPosition = firstPos - offset * i;
        }
    }
    // 이동, 핸드.추가함수() 호출
    public void GetOpenCard(Card card, float delay)
    {
        card.Move(GetCurrentHand.transform, delay);
        card.Rotate(delay);

        bool isFirstHand = false;
        if (hands.Count == 1 && hands[0].GetCardCount < 2)
        {
            isFirstHand = true;
        }

        GetCurrentHand.Push(card, isFirstHand); // (in)카드 하이라이트 +
    }
    public void MoveAll(Transform toTrans, float delay)
    {
        for (int i = 0; i < hands.Count; ++i)
        {
            hands[i].MoveAll(toTrans, delay);
        }
    }
    public void DiscardAll(DiscardDeck dDeck)// 카드 전달, 핸드 초기화(size 1)
    {
        currentHandIndex = 0;

        // 카드 회수
        for(int i = 0; i < hands.Count; ++i)
        {
            hands[i].DiscardAll(dDeck);
        }

        hands[0].transform.position = this.transform.position;
        
        // 추가된 핸드 삭제
        if (hands.Count > 1)
        {
            for(int i = 1; i < hands.Count; ++i)
            {
                Destroy(hands[i].gameObject);
            }

            hands.RemoveRange(1, hands.Count-1);
        }
    }
    public bool UpdateToNextHand()
    {
        hands[currentHandIndex].DisHighlight();// 전체, 바로 Highlight
        ++currentHandIndex;

        if (currentHandIndex < hands.Count)
        {
            hands[currentHandIndex].Highlight(0.0f);// 전체, 바로 DeHighlight
            return true;
        }
        else
        {
            currentHandIndex = 0;
            return false;
        }
    }
    public bool UpdateAndCheckAllPossibleHand()
    {
        GetCurrentHand.DisHighlight();
        ++currentHandIndex;
        
        while (currentHandIndex < hands.Count)
        {
            if (GetCurrentHand.IsStopChoice == false)
            {
                GetCurrentHand.Highlight(0.0f);
                return true;
            }
        
            ++currentHandIndex;
        }
        currentHandIndex = 0;
        return false;
    }

    public void DoubleDown()
    {
        int half = GetCurrentHand.AmountOfBetting; // 배팅할 금액

        // 배팅
        GetCurrentHand.DoubleDownBetting();
         
        // 계좌 차감
        Budget -= half;
    }
    // Hand추가, 카드 나눔, 배팅 추가,
    // 스플릿 후 핸드 각각에 한장의 카드를 줄때
    // 각각의 핸드에 접근을 해야한다 그런데
    // 핸드 접근방법이 단방향 인덱싱이기 때문에(양방향 할수도있지만 가독성, 안전성을 위해)
    // 2번째 핸드의 객체를 인자로 전달받아서 접근한다
    public PlayerHand Split(bool isAce)
    {
        // 스플릿된 핸드에 전해줄  카드와 배팅액  수치 구함
        Card card = GetCurrentHand.Pop();
        card.DisHighlight();
        int bet = GetCurrentHand.AmountOfBetting;
        Budget -= bet;

        // 추가한 핸드
        PlayerHand splitHand = AddHand();

        if(isAce)
        {
            GetCurrentHand.SetSplitAce();
            splitHand.SetSplitAce();
        }

        // 그 핸드에 정보 전달
        splitHand.Push(card, false);
        card.transform.position = splitHand.transform.position;
        splitHand.Betting(bet);

        return splitHand;
    }
    public void Surrender()
    {
        int half = GetCurrentHand.AmountOfBetting/2; // 반토막 가져옴
        GetCurrentHand.SurrenderBetting(); // 배팅 없앰
        
        // 계좌 추가
        account.AmountOfAccount += half; // 계좌로 회수
    }

    //______________________________UI__________________________________
    void SetAccountLabel()
    {
        accountLabel.text = "$"+Budget.ToString();
    }
    
}
