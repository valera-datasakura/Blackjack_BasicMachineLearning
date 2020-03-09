using CardEnums;

public class AI_PlayerHand : AI_Hand
{
    int bitChoices= (int)ChoiceKind.Stand;

    bool isInsurance = false;
    bool isSurrender = false;
    bool isDouble = false;
    bool isSplitAce = false;

    AI_BettingZone bettingZone;

    //_________________________________________Cards___________________________________________________________

    void UpdateChoices(bool isFirstHand)
    {
        bitChoices = (int)ChoiceKind.Stand;

        if (value < (HAND_VALUE)21 && !isSplitAce)
        {
            bitChoices |= (int)ChoiceKind.Hit;

            if (cards.Count == 2)
            {
                bitChoices |= (int)ChoiceKind.DoubleDown;
                
                if (cards[0].number == cards[1].number)
                {
                    // Split
                    bitChoices |= (int)ChoiceKind.Split;
                }

                if (isFirstHand)
                {
                    // Surrender
                    bitChoices |= (int)ChoiceKind.Surrender;
                }
            }
        }
    }

    public bool CanHit
    {
        get
        {
            return (bitChoices & (int)ChoiceKind.Hit) == (int)ChoiceKind.Hit;
        }
    }
    public bool CanStand
    {
        get
        {
            return (bitChoices & (int)ChoiceKind.Stand) == (int)ChoiceKind.Stand;
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

    public bool IsSurrender
    {
        get
        {
            return isSurrender;
        }
        set
        {
            isSurrender = value;
        }
    }

    public bool IsStopChoice
    {
        get
        {
            return (value == HAND_VALUE.BURST_PLAYER ||
               value == HAND_VALUE.BLACKJACK ||
               value == HAND_VALUE.VALUE21 ||
               isSurrender ||
               isSplitAce);
        }
    }
    public bool IsDoubleAce
    {
        get
        {
            if (cards.Count <= 2 &&
                cards[0].number == 1 &&
                cards[1].number == 1)
            {
                return true;
            }

            return false;
        }
    }
    public int GetSituationIndex
    {
        get
        {
            if (cards[0].number == cards[1].number) // 더블
            {
                return cards[0].number + 23; // 24~33(10)
            }
            else if(cards[0].number==1) // 소프트
            {
                return cards[1].number + 13; // 15 ~ 23(9)
            }
            else if (cards[1].number == 1) // 소프트
            {
                return cards[0].number + 13;
            }
            else  //  하드
            {
                return cards[0].number + cards[1].number-5; // 0~14
            }
        }
    }

    public override void Push(AI_Card card, bool isFirstHand)
    {
        base.Push(card, isFirstHand); // 부모설정, 오프셋위치, 리스트push, 값 업데이트

        UpdateChoices(isFirstHand); // 가능 초이스 업데이트
    }
    public override void DiscardAll(ref AI_DiscardDeck ddeck) // 디스카드덱이 도착하면 호출
    {
        base.DiscardAll(ref ddeck);

        isSurrender = false;
        isDouble = false;
        isSplitAce = false;
        isInsurance = false;
    }

  
    
    public void SetSplitAce()
    {
        isSplitAce = true;
    }

    //___________________________________________배팅존 관련_____________________________________________________
    public float AmountOfBetting
    {
        get
        {
            return bettingZone.currentBet;
        }
        set //RemoveAfterComplete
        {
            bettingZone.currentBet = value;
        }
    }
    public float AmountOfInsurance
    {
        get
        {
            return bettingZone.insuranceBet;
        }
    }
    public bool IsBetEmpty
    {
        get
        {
            return bettingZone.IsEmpty;
        }
    }
    
    public void Betting()
    {
        bettingZone.Betting();
    }
    public void DoubleDownBetting()
    {
        bettingZone.DoubleDownBetting();

        isDouble = true;
    }
    public void SurrenderBetting()
    {
        bettingZone.InsuranceBetting();

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
        if (isDouble == false)
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

    public void Collect(out float amount)
    {
        bettingZone.Collect(out amount);
    }
    /// <summary>
    /// 수치를 초기화한다
    /// </summary>
    /// <param name="kind"></param>
    public void LoseBet(BET_KIND kind)
    {
        bettingZone.EmptyBet(kind);
    }

    #region rest

    void Awake()
    {
        // 검색 찾기
        bettingZone = transform.GetChild(0).GetComponent<AI_BettingZone>();
    }

    #endregion
}
