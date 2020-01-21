using CardEnums;

public class AI_DealerHand : AI_Hand
{
    public void Init()
    {
        base.Init("Dealer");
    }

    //___________________________________카드 정보____________________________
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
            if (cards.Count == 2 && cards[1].number == 1)
            {
                return true;
            }

            return false;
        }
    }
    public int GetIndexOfOpen
    {
        get
        {
            return cards[1].number-1;
        }
    }
    //____________________________________카드 처리 관련____________________________

    public void GetCard(AI_Card card)
    {
        bool isFirstHand = false;
        // 카드 추가전에 계산되기 때문에
        // 조건 수치를 2->1로 줄인다(origin=IsFirstHand)
        if (cards.Count <= 1)
        {
            isFirstHand = true;
        }

        Push(card, isFirstHand);
    }
    public override void DiscardAll(ref AI_DiscardDeck ddeck)
    {
        base.DiscardAll(ref ddeck);
    }
}
