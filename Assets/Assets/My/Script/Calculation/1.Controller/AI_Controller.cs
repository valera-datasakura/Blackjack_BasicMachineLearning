using CardEnums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class AI_Controller : MonoBehaviour
{
    void Update()
    {
        for (int i = 0; i < 250; ++i)
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
        resultInfo.countingNumber = player.Real_CCNumber;
        resultInfo.playerIdx = player.GetCurrentHand.GetSituationIndex;
        resultInfo.dealerIdx = dealer.GetIndexOfOpen;
    }
    void CheckInsurance()
    {
        if (dealer.IsInsuranceHand)
        {
            if (Random.Range(0, 5) == 0)
            {
                player.GetCurrentHand.InsuranceBetting();

                totalBetting += player.GetCurrentHand.AmountOfInsurance;

                resultInfo.firstChoice = ChoiceKind.Insurance;
            }
            else
                resultInfo.firstChoice = ChoiceKind.NotInsurance;
        }
    }
    void ExecutePlayerTurn()
    {
        if (dealer.IsBlackjack == false && player.IsBlackjack == false)
        {
            Choice();
        }
    }
    void ExecuteDealerTurn()
    {
        if (CanDealerProgress)
        {
            while (true)
            {
                if (dealer.IsStopHitting)
                {
                    break;
                }

                GetCardDealer();
            }
        }
    }
    bool CanDealerProgress
    {
        get
        {
            player.ResetHandIndex();

            if (player.GetCurrentHand.IsBlackjack ||
                player.GetCurrentHand.IsSurrender)
                return false;

            do
            {
                if(player.GetCurrentHand.IsBurst == false)
                {
                    player.ResetHandIndex();
                    return true;
                }

            } while (player.UpdateToNextHand());
            
            player.ResetHandIndex();
            return false;
        }
    }
    
    void Result()
    {
        if (player.GetCurrentHand.IsSurrender)
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

        DB_Manager.Instance.AddResultInfo(resultInfo);
    }
    void ExecuteMainBetting()
    {
        player.ResetHandIndex();

        do
        {
            if (dealer < player.GetCurrentHand)
            {
                player.GetCurrentHand.RewardOrigin(player.IsBlackjack ? BET_KIND.BLACKJACK : BET_KIND.ORIGINAL);
            }
            else if (dealer > player.GetCurrentHand)
            {
                player.GetCurrentHand.LoseBet(BET_KIND.ORIGINAL);
            }

            earningBetting += player.GetCurrentHand.AmountOfBetting;

        } while (player.UpdateToNextHand());
    }
    void ExecuteInsuranceBetting()
    {
        player.ResetHandIndex();

        if (dealer.IsBlackjack)
        {
            player.GetCurrentHand.RewardInsurance();

            earningBetting += player.GetCurrentHand.AmountOfInsurance;
        }
        else
            player.GetCurrentHand.LoseBet(BET_KIND.INSURANCE);
    }
    void CalculateTotalBetting()
    {
        player.ResetHandIndex();

        do
        {
            totalBetting += player.GetCurrentHand.AmountOfBetting;
        } while (player.UpdateToNextHand());
    }

    void ClearTable()
    {
        dealer.DiscardAll(ref dDeck);
        player.DiscardAll(ref dDeck);

        player.AddCCNumber(holeCardCountingNumber);
        
        if (deck.IsShuffleTime)
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
            
            if(IsFirstChoiceNotDetermined())
            {
                ReadOnlyCollection<ChoiceKind> possibleOptions = GetPossibleOptions();

                curChoice = possibleOptions[Random.Range(0, possibleOptions.Count)];

                resultInfo.firstChoice = curChoice;
            }
            else
            {
                curChoice = GetBestChoice(resultInfo.dealerIdx);
            }

            while (true)
            {
                bool loop = true;

                switch (curChoice)
                {
                    case ChoiceKind.Hit:
                        GetCardPlayer();
                        
                        loop = (!player.GetCurrentHand.IsStopChoice && PreferHit(resultInfo.dealerIdx));
                        
                        break;

                    case ChoiceKind.Stand:
                        loop = false;
                        break;

                    case ChoiceKind.DoubleDown:
                        
                        player.DoubleDown();

                        int preCount = player.CCNumber;
                        int preValue = (int)player.GetCurrentHand.value;
                        bool isPreSoft = player.GetCurrentHand.IsSoft;

                        GetCardPlayer();

                        if (preValue >= 12 && isPreSoft == false)
                        {
                            if (player.GetCurrentHand.value == HAND_VALUE.BURST_PLAYER)
                                DB_Manager.Instance.AddBurstInfo(preCount, preValue);
                            else
                                DB_Manager.Instance.AddNotBurstInfo(preCount, preValue);
                        }

                        loop = false;
                            
                        break;

                    case ChoiceKind.Split:
                        
                        AI_PlayerHand secondHand = null;
                        AI_Card secondCard = deck.Pop();

                        player.AddCCNumber(secondCard.cardCountingScore);
                        
                        secondHand=player.Split(player.GetCurrentHand.IsDoubleAce);
                        GetCardPlayer();
                        secondHand.Push(secondCard, false);

                        curChoice = GetBestChoice(resultInfo.dealerIdx);
                        loop = (curChoice != ChoiceKind.Stand);

                        break;

                    case ChoiceKind.Surrender:

                        player.GetCurrentHand.AmountOfBetting = 0;
                        player.GetCurrentHand.IsSurrender = true;
                        loop = false;

                        break;

                    case ChoiceKind.NotDetermined:

                        Debug.LogError("Stand is base choice, so whatever else should be determined in this stage");
                        loop = false;
                        break;
                }

                if (loop == false)
                    break;
            }

        } while (player.UpdateAndCheckAllPossibleHand());
    }
    bool IsFirstChoiceNotDetermined()
    {
        return (resultInfo.firstChoice == ChoiceKind.NotDetermined || resultInfo.firstChoice == ChoiceKind.NotInsurance);
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
        if (isHold)
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
        int player_Idx = FirstHandValue_To_SituationKind.Convert(player.GetCurrentHand.value, player.GetCurrentHand.IsSoft);

        //  1.
        float standWinningRate = (DB_Manager.Instance.GetSingle(player.CCNumber, dealerNum_idx, player_Idx).rate_Stand + 1f) / 2f;

        //  2.
        float hitBurstRate = 0;
        if ((int)player.GetCurrentHand.value >= 12 && player.GetCurrentHand.IsHard)
        {
            hitBurstRate = DB_Manager.Instance.GetBurst(player.CCNumber, (int)player.GetCurrentHand.value).rate;
        }
        
        float average =
           ((1f - standWinningRate) * (1f + HIT_WEIGHT)
            + (1f - hitBurstRate) * (1f - HIT_WEIGHT))
            / 2f;

        //  3.
        return (average > HIT_PREFERENCE);
    }
    ReadOnlyCollection<ChoiceKind> GetPossibleOptions()
    {
        AI_PlayerHand playerHand = player.GetCurrentHand;
        
        List<ChoiceKind> choices = new List<ChoiceKind>();

        if (playerHand.CanHit)
        {
            choices.Add(ChoiceKind.Hit);
        }
        if (playerHand.CanStand)
        {
            choices.Add(ChoiceKind.Stand);
        }
        if (playerHand.CanDoubleDown)
        {
            choices.Add(ChoiceKind.DoubleDown);
        }
        if (playerHand.CanSplit)
        {
            choices.Add(ChoiceKind.Split);
        }

        return choices.AsReadOnly();
    }
    ChoiceKind GetBestChoice(int dealer_idx)
    {
        Dictionary<ChoiceKind, float> choiceWinningRates = new Dictionary<ChoiceKind, float>();
        AI_PlayerHand playerHand = player.GetCurrentHand;
        
        Situation_Info info = DB_Manager.Instance.GetSingle(player.CCNumber, dealer_idx, player.GetCurrentHand.GetSituationIndex);

        if (playerHand.CanHit)
        {
            choiceWinningRates.Add(ChoiceKind.Hit, info.rate_Hit);
        }
        if (playerHand.CanStand)
        {
            choiceWinningRates.Add(ChoiceKind.Stand, info.rate_Stand);
        }
        if (playerHand.CanDoubleDown)
        {
            choiceWinningRates.Add(ChoiceKind.DoubleDown, info.rate_DoubleDown);
        }
        if (playerHand.CanSplit)
        {
            choiceWinningRates.Add(ChoiceKind.Split, info.rate_Split);
        }
        if (playerHand.CanSurrender)
        {
            choiceWinningRates.Add(ChoiceKind.Surrender, -0.5f);
        }

        ChoiceKind best = ChoiceKind.NotDetermined;
        float bestRate = float.MinValue;
        foreach (ChoiceKind kind in choiceWinningRates.Keys)
        {
            if (bestRate < choiceWinningRates[kind])
            {
                best = kind;
                bestRate = choiceWinningRates[kind];
            }
        }

        if(best == ChoiceKind.NotDetermined)
        {
            Debug.LogError("should be determined for best choice");
        }
       
        return best;
    }

    #region rest

    [SerializeField] AI_Deck deck;
    [SerializeField] AI_DiscardDeck dDeck;
    [SerializeField] AI_DealerHand dealer;
    [SerializeField] AI_Player player;

    public const float HIT_WEIGHT = 0.425f;
    public const float HIT_PREFERENCE = 0.6125f;
    
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

