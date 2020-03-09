using CardEnums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class Simulator : MonoBehaviour
{
    [SerializeField] private AI_Deck deck;
    [SerializeField] private AI_DiscardDeck dDeck;
    [SerializeField] private AI_DealerHand dealer;
    [SerializeField] private AI_Player player;

    [SerializeField] private UILabel totalLabel;
    [SerializeField] private UILabel winningLabel;

    private int holeCardCountingNumber;
    private float totalBetting;
    private float earningBetting;

    private float curTime = 0;

    private ResultInfo resultInfo;

    void Start()
    {
        player.Init();
        dealer.Init();
    }
    void Update()
    {
        for(int i = 0; i < 100; ++i)
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

        curTime += Time.deltaTime;
        if (curTime >0.2f)
        {
            totalLabel.text = DB_Manager.Instance.TotalCount.ToString();
            winningLabel.text = (totalRate / totalGame).ToString("f5");

            curTime = 0;
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

    static long totalGame = 0;
    static double totalRate = 0;
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

        totalRate += resultInfo.winningRate;
        totalGame++;
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
        do
        {
            bool isLoop = true;
            do
            {
                switch (GetBestChoice(resultInfo.dealerIdx))
                {
                    case ChoiceKind.Hit:

                        GetCardPlayer();

                        isLoop = (!player.GetCurrentHand.IsStopChoice && PreferHit(resultInfo.dealerIdx));

                        break;

                    case ChoiceKind.Stand:
                        isLoop = false;
                        break;

                    case ChoiceKind.DoubleDown:

                        player.DoubleDown();

                        GetCardPlayer();

                        isLoop = false;

                        break;

                    case ChoiceKind.Split:

                        AI_PlayerHand secondHand = player.Split(player.GetCurrentHand.IsDoubleAce);
                        GetCardPlayer();

                        AI_Card secondCard = deck.Pop();
                        player.AddCCNumber(secondCard.cardCountingScore);
                        secondHand.Push(secondCard, false);

                        break;

                    case ChoiceKind.Surrender:

                        player.GetCurrentHand.AmountOfBetting = 0;
                        player.GetCurrentHand.IsSurrender = true;
                        isLoop = false;

                        break;

                    case ChoiceKind.NotDetermined:

                        isLoop = false;

                        break;
                }

            } while (isLoop && !player.GetCurrentHand.IsStopChoice);

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
           ((1f - standWinningRate) * (1f + AI_Controller.HIT_WEIGHT)
            + (1f - hitBurstRate) * (1f - AI_Controller.HIT_WEIGHT))
            / 2f;

        //  3.
        return (average > AI_Controller.HIT_PREFERENCE);
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

}

