using CardEnums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class Simulator : MonoBehaviour
{
    void Update()
    {
        for(int i = 0; i < 1800; ++i)
        {
            Ready();

            player.GetCurrentHand.Betting();

            SetStartingCards();

            SetSampleInfo();

            CheckInsurance();

            ExecutePlayerTurn();

            ExecuteDealerTurn();

            Result();

            ClearTable();
        }
    }

    void Ready()
    {
        player.account = 0f;
        totalBetting = 0f;
        earningBetting = 0f;
        resultInfo.firstChoice = ChoiceKind.NotDetermined;
    }
    void SetStartingCards()
    {
        GetCardPlayer();
        GetCardDealer(true);
        GetCardPlayer();
        GetCardDealer();
    }
    void SetSampleInfo()
    {
        resultInfo.countingNumber = player.CCNumber;
        resultInfo.playerIdx = player.GetCurrentHand.GetSituationIndex;
        resultInfo.dealerIdx = dealer.GetIndexOfOpen;
    }
    void CheckInsurance()
    {
        if(dealer.IsInsuranceHand)
        {
            if(GetBestChoice(resultInfo.dealerIdx) == ChoiceKind.Insurance)
            {
                player.GetCurrentHand.InsuranceBetting();

                totalBetting += player.GetCurrentHand.AmountOfInsurance;

                resultInfo.firstChoice = ChoiceKind.Insurance;
            }
        }
    }
    void ExecutePlayerTurn()
    {
        if(dealer.IsBlackjack == false && player.IsBlackjack == false)
        {
            Choice();
        }
    }
    void ExecuteDealerTurn()
    {
        if(CanDealerProgress)
        {
            while(true)
            {
                if(dealer.IsStopHitting)
                {
                    break;
                }

                GetCardDealer();
            }
        }
    }
    bool CanDealerProgress {
        get {
            player.ResetHandIndex();

            if(player.GetCurrentHand.IsBlackjack ||
                player.GetCurrentHand.IsSurrender)
                return false;

            do
            {
                if(player.GetCurrentHand.IsBurst == false)
                {
                    return true;
                }

            } while(player.UpdateToNextHand());

            return false;
        }
    }

    static long game1 = 0;
    static double rate1 = 0;
    static long game2 = 0;
    static double rate2 = 0;
    void Result()
    {
        if(player.GetCurrentHand.IsSurrender)
        {
            totalBetting = 1f;
            earningBetting = 0.5f;
        }
        else
        {
            CalculateTotalBetting();

            ExecuteInsuranceBetting();

            ExecuteMainBetting();
        }

        resultInfo.winningRate = earningBetting - totalBetting;

        player.Collect();

        //DB_Manager.Instance.AddResultInfo(resultInfo);

        if(resultInfo.countingNumber >= 10)
        {
            rate1 += resultInfo.winningRate;
            game1++;
        }
        else if(resultInfo.countingNumber <= -10)
        {
            rate2 += resultInfo.winningRate;
            game2++;
        }
        if(Random.Range(0, 1000) == 0)
        {
            Debug.Log("(CCN >= 10) winning rate = " + (rate1 / game1).ToString("f5") + " (CCN <= -10) winning rate = " + (rate2 / game2).ToString("f5"));
        }
    }
    void ExecuteMainBetting()
    {
        do
        {
            if(dealer < player.GetCurrentHand)
            {
                player.GetCurrentHand.RewardOrigin(player.IsBlackjack ? BET_KIND.BLACKJACK : BET_KIND.ORIGINAL);
            }
            else if(dealer > player.GetCurrentHand)
            {
                player.GetCurrentHand.LoseBet(BET_KIND.ORIGINAL);
            }

            earningBetting += player.GetCurrentHand.AmountOfBetting;

        } while(player.UpdateToNextHand());
    }
    void ExecuteInsuranceBetting()
    {
        if(dealer.IsBlackjack)
        {
            player.GetCurrentHand.RewardInsurance();

            earningBetting += player.GetCurrentHand.AmountOfInsurance;
        }
        else
            player.GetCurrentHand.LoseBet(BET_KIND.INSURANCE);
    }
    void CalculateTotalBetting()
    {
        do
        {
            totalBetting += player.GetCurrentHand.AmountOfBetting;
        } while(player.UpdateToNextHand());
    }

    void ClearTable()
    {
        dealer.DiscardAll(ref dDeck);
        player.DiscardAll(ref dDeck);

        player.AddCCNumber(holeCardCountingNumber);
        if(deck.IsShuffleTime)
        {
            player.Reset_CCNumber();
            deck.Shuffle(dDeck);
        }
    }


    void Choice()
    {
        ChoiceKind curChoice = ChoiceKind.NotDetermined;


        do
        {
            // always best choice
            curChoice = GetBestChoice(resultInfo.dealerIdx);

            bool loop = true;
            while(loop)
            {
                switch(curChoice)
                {
                    case ChoiceKind.Hit:

                        GetCardPlayer();

                        loop = (player.GetCurrentHand.CanChoose && PreferHit(resultInfo.dealerIdx));

                        break;

                    case ChoiceKind.Stand:
                        loop = false;
                        break;

                    case ChoiceKind.DoubleDown:

                        player.DoubleDown();
                        
                        GetCardPlayer();
                        
                        loop = false;

                        break;

                    case ChoiceKind.Split:

                        AI_PlayerHand secondHand = null;
                        AI_Card secondCard = deck.Pop();

                        player.AddCCNumber(secondCard.cardCountingScore);

                        secondHand = player.Split(player.GetCurrentHand.IsDoubleAce);
                        GetCardPlayer();
                        secondHand.Push(secondCard, false);

                        curChoice = GetBestChoice(resultInfo.dealerIdx);
                        loop = (curChoice != ChoiceKind.NotDetermined);

                        break;

                    case ChoiceKind.Surrender:

                        player.GetCurrentHand.AmountOfBetting = 0;
                        player.GetCurrentHand.IsSurrender = true;
                        loop = false;

                        break;

                    case ChoiceKind.NotDetermined:

                        loop = false;

                        break;
                }

                if(loop == false)
                    break;

            }// while(Loop)

        } while(player.UpdateAndCheckAllPossibleHand());
    }
    void GetCardPlayer()
    {
        AI_Card newCard = deck.Pop();

        player.AddCCNumber(newCard.cardCountingScore);

        player.GetCard(newCard);
    }
    void GetCardDealer(bool isHold = false)
    {
        AI_Card newCard = deck.Pop(); // Get Card

        //
        if(isHold)
        {
            holeCardCountingNumber = newCard.cardCountingScore;
        }
        else
        {
            player.AddCCNumber(newCard.cardCountingScore);
        }

        dealer.GetCard(newCard);
    }

    bool PreferHit(int dealerNum_idx)
    {
        int playerNum_Idx = FirstHandValue_To_SituationKind.Convert(player.GetCurrentHand.value, player.GetCurrentHand.IsSoft);

        //  1.
        float standWinningRate = (DB_Manager.Instance.GetSingle(player.CCNumber, dealerNum_idx, playerNum_Idx).rate_Stand + 1f) / 2f;

        //  2.
        float hitBurstRate = 0;
        if((int)player.GetCurrentHand.value >= 12 && player.GetCurrentHand.IsHard)
        {
            hitBurstRate = DB_Manager.Instance.GetBurst(player.CCNumber, (int)player.GetCurrentHand.value).rate;
        }
        
        float average =
           ((1f - standWinningRate) * (1f + AI_Controller.HIT_WEIGHT)
            + (1f - hitBurstRate) * (1f - AI_Controller.HIT_WEIGHT))
            / 2f;

        //  3.
        return (average > HIT_PREFERENCE);
    }
    ChoiceKind GetBestChoice(int dealer_idx)
    {
        int player_idx = player.GetCurrentHand.GetSituationIndex;
        Dictionary<ChoiceKind, float> choiceWinningRates = new Dictionary<ChoiceKind, float>();
        AI_PlayerHand playerHand = player.GetCurrentHand;
        Situation_Info info = DB_Manager.Instance.GetSingle(player.CCNumber, dealer_idx, player_idx);

        if(playerHand.CanHit)
        {
            choiceWinningRates.Add(ChoiceKind.Hit, info.rate_Hit);
        }
        if(playerHand.CanStand)
        {
            choiceWinningRates.Add(ChoiceKind.Stand, info.rate_Stand);
        }
        if(playerHand.CanDoubleDown)
        {
            choiceWinningRates.Add(ChoiceKind.DoubleDown, info.rate_DoubleDown);
        }
        if(playerHand.CanSplit)
        {
            choiceWinningRates.Add(ChoiceKind.Split, info.rate_Split);
        }
        if(playerHand.CanSurrender)
        {
            choiceWinningRates.Add(ChoiceKind.Surrender, -0.5f);
        }

        ChoiceKind best = ChoiceKind.NotDetermined;
        float bestRate = float.MinValue;
        foreach(ChoiceKind kind in choiceWinningRates.Keys)
        {
            if(bestRate < choiceWinningRates[kind])
            {
                best = kind;
                bestRate = choiceWinningRates[kind];
            }
        }

        return best;
    }

    #region rest

    [SerializeField] AI_Deck deck;
    [SerializeField] AI_DiscardDeck dDeck;
    [SerializeField] AI_DealerHand dealer;
    [SerializeField] AI_Player player;

    const float HIT_PREFERENCE = AI_Controller.HIT_PREFERENCE;
    
    int holeCardCountingNumber;
    float totalBetting;
    float earningBetting;

    ResultInfo resultInfo;

    void Start()
    {
        player.Init();
        dealer.Init();
    }
#endregion
}

