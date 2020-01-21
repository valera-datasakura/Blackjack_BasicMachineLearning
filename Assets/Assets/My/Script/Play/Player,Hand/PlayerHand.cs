using UnityEngine;
using CardEnums;

public class PlayerHand : Hand {

    int bitChoices;

    GameObject stateUIPrefab;
    UIPanel stateUIParent;

    // 시간차 하이라이트, 상태 표시
    bool isHighlightTimeAfter = false;
    float currentHighlightTimeAfter;
    float totalHighlightTimeAfter;

    // 서렌더 여부
    bool isSurrender = false;
    bool isSplitAce = false;
    bool isDouble = false;
    bool isInsurance = false;

    BettingZone bettingZone;
    
    void Awake()
    {
        // 검색 찾기
        bettingZone = transform.GetChild(0).GetComponent<BettingZone>();
    }

    public void Init(GameObject statePrefab, UIPanel stateParent, Transform insuranceTrans, string tag)
    {
        Init(statePrefab, stateParent, tag);

        stateUIPrefab = statePrefab;
        stateUIParent = stateParent;

        bettingZone.Init(insuranceTrans);
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
                SetStateUI(isSurrender);
            }
        }

        // Highlight 업데이트
        if (isHighlightTimeAfter)
        {
            currentHighlightTimeAfter += Time.deltaTime;

            if (currentHighlightTimeAfter >= totalHighlightTimeAfter)
            {
                isHighlightTimeAfter = false;
                
                for (int i = 0; i < cards.Count; ++i)
                {
                    cards[i].EnHighlight();
                }
            }
        }
    }

    //_________________________________________카드 관련___________________________________________________________
    // Only 카드 상황 not Account
    public bool CanHit
    {
        get
        {
            return ((bitChoices & (int)ChoiceKind.Hit) == (int)ChoiceKind.Hit);
        }
    }
    public bool CanStand
    {
        get
        {
            return ((bitChoices & (int)ChoiceKind.Stand) == (int)ChoiceKind.Stand);
        }
    }
    public bool CanDoubleDown
    {
        get
        {
            return (bitChoices & (int)ChoiceKind.DoubleDown) == (int)ChoiceKind.DoubleDown;
        }
    }
    public bool CanSplit
    {
        get
        {
            return (bitChoices & (int)ChoiceKind.Split) == (int)ChoiceKind.Split;
        }
    }
    public bool CanSurrender
    {
        get
        {
            return (bitChoices & (int)ChoiceKind.Surrender) == (int)ChoiceKind.Surrender;
        }
    }

    public bool IsStopChoice // 선택을 다시 할수 있나 없나
    {
        get
        {
            return (value == HAND_VALUE.BURST_PLAYER ||
                value == HAND_VALUE.BLACKJACK ||
                value == HAND_VALUE.VALUE21 ||
                isSurrender ||
                (isSplitAce && (cards[0].Number != cards[1].Number))) ;
        }
    }
    public int GetSituationIndex
    {
        get
        {
            if (cards[0].Number == cards[1].Number) // 더블
            {
                return cards[0].Number + 23; // 24~33(10)
            }
            else if (cards[0].Number == 1) // 소프트
            {
                return cards[1].Number + 13; // 15 ~ 23(9)
            }
            else if (cards[1].Number == 1) // 소프트
            {
                return cards[0].Number + 13;
            }
            else  //  하드
            {
                return cards[0].Number + cards[1].Number - 5; // 0~14
            }
        }
    }
    public bool IsDoubleAce
    {
        get
        {
            if(cards.Count <= 2 &&
                cards[0].Number == 1 &&
                cards[1].Number == 1)
            {
                return true;
            }

            return false;
        }
    }

    public override void Push(Card card, bool isFirstHand)
    {
        base.Push(card, isFirstHand); // 부모설정, 오프셋위치, 리스트push, 값 업데이트

        CheckChoices(isFirstHand); // 가능 초이스 업데이트
    }
    public override void MoveAll(Transform toTrans, float time)
    {
        base.MoveAll(toTrans, time);

        // De 하이라이트
        DisHighlight();

        for (int i = 0; i < cards.Count; ++i)
        {
            cards[i].DisHighlight();
        }
    }
    public override void DiscardAll(DiscardDeck ddeck) // 디스카드덱이 도착하면 호출
    {
        base.DiscardAll(ddeck);

        isSurrender = false;
        isDouble = false;
        isSplitAce = false;
        isInsurance = false;

        DestroyStateUI();
    }

    //아웃라인 하이라이트
    public void Highlight(float delay=0f)// 컨트롤러에서 시작할때, 플레이어에서 인덱스 넘길때 사용
    {
        //if (isHighlightTimeAfter)
        //{
        //    Debug.Log("얼마나 주목받을라구 in Highlight() of PlayerHand");
        //}

        isHighlightTimeAfter = true;
        currentHighlightTimeAfter = 0.0f;
        totalHighlightTimeAfter = delay;
    }
    public void DisHighlight()// 컨트롤러에서 시작할때, 플레이어에서 인덱스 넘길때 사용
    {
        for (int i = 0; i < cards.Count; ++i)
        {
            cards[i].DisHighlight();
        }
    }

    void CheckChoices(bool isFirstHand)
    {
        bitChoices = 0;

        if (Value < (HAND_VALUE)21)
        {
            // Hit
            if (isSplitAce == false)
            {
                bitChoices |= (int)ChoiceKind.Hit;
            }

            // Stand
            bitChoices |= (int)ChoiceKind.Stand;

            if (cards.Count == 2)
            {
                // DoubleDown
                bitChoices |= (int)ChoiceKind.DoubleDown;

                if (cards[0].Number == cards[1].Number)
                {
                    // Split
                    bitChoices |= (int)ChoiceKind.Split;
                }

                if (isFirstHand && isInsurance==false)
                {
                    // Surrender
                    bitChoices |= (int)ChoiceKind.Surrender;
                }
            }
        }
    }
    public void SetSplitAce()
    {
        isSplitAce = true;
    }


    //___________________________________________배팅존 관련_____________________________________________________
    public int AmountOfBetting
    {
        get
        {
            return bettingZone.AmountOfBetting;
        }
    }
    public int AmountOfInsurance
    {
        get
        {
            return bettingZone.AmountOfInsurance;
        }
    }
    public bool IsBetEmpty
    {
        get
        {
            return bettingZone.IsEmpty;
        }
    }
    public bool IsBetInRange
    {
        get
        {
            return bettingZone.IsBettingProper;
        }
    }
    public bool IsSurrender
    {
        get
        {
            return isSurrender;
        }
    }

    // 순수 배팅금 조정 함수
    public void Betting(int amount)
    {
        bettingZone.Betting(amount);
    }
    public void DoubleDownBetting()
    {
        bettingZone.DoubleDownBetting();

        isDouble = true;
    }
    public void SurrenderBetting()
    {
        bettingZone.SurrenderBetting();
        
        isSurrender = true;
    }
    public void InsuranceBetting()
    {
        bettingZone.InsuranceBetting();

        isInsurance = true;
        bitChoices ^= (int)ChoiceKind.Surrender;
    }

    public void RewardOrigin(BET_KIND kind)
    {
        if (isDouble==false)
        {
            bettingZone.RewardOrigin(kind);
        }
        else
        {
            bettingZone.RewardOrigin(BET_KIND.DOUBLEDOWN);
        }
    }
    public void RewardInsurance()
    {
        bettingZone.RewardInsurance();
    }

    public void Collect(out int amount)
    {
        bettingZone.Collect(out amount);
    }
    public void LoseBet(BET_KIND kind)
    {
        bettingZone.EmptyBet(kind);
    }

    //___________________________________________UI_________________________________________________________
    void SetStateUI(bool isSurrender)
    {
        if (stateUI == null)// 없다면 여기서 바로 만들어주자
        {
            GameObject newObj =
            (GameObject)Instantiate(stateUIPrefab, GetMyWorldToUIScreen(), Quaternion.identity);

            stateUI = newObj.GetComponent<UISprite>();
            stateUI.transform.SetParent(stateUIParent.transform);
            stateUI.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            stateUI.gameObject.SetActive(false);
        }

        switch (value)
        {
            case HAND_VALUE.BURST_PLAYER:
                stateUI.spriteName = "Burst";
                stateUI.gameObject.SetActive(true);
                break;
            case HAND_VALUE.BLACKJACK:
                stateUI.spriteName = "Blackjack";
                stateUI.gameObject.SetActive(true);
                break;
            default:
                if (isSurrender)
                {
                    stateUI.spriteName = "Surrender";
                    stateUI.gameObject.SetActive(true);
                }
                break;
        }

        stateUI.transform.position = GetMyWorldToUIScreen();
    }
}
