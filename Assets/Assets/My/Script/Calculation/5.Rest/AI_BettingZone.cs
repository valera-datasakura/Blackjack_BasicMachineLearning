using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI_BettingZone : MonoBehaviour
{
    public float currentBet;
    public float insuranceBet;
    
    public bool IsEmpty
    {
        get
        {
            if (currentBet == 0 && insuranceBet == 0)
            {
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 수익률 나눔과정 생략을 위한 1대입
    /// </summary>
    public void Betting()
    {
        currentBet = 1f;
    }
    public void DoubleDownBetting()
    {
        //if (currentBet <= 0)
        //{
        //    Debug.Log("뭔일이 있었던 거야 in DoubleDownBetting() of BettingZone");
        //}

        currentBet *= 2;
    }
    public void SurrenderBetting()
    {
        //if (currentBet <= 0 || insuranceBet != 0)
        //{
        //    Debug.Log("뭔일이 있었던 거야 in Surrender() of BettingZone");
        //}

        currentBet /= 2;
    }
    public void InsuranceBetting()
    {
        //if (currentBet <= 0 || insuranceBet != 0)
        //{
        //    Debug.Log("뭔일이 있었던 거야 in InsuranceBetting() of BettingZone");
        //}

        insuranceBet = currentBet / 2;
    }

    public void RewardOrigin(BET_KIND kind)
    {
        switch (kind)
        {
            case BET_KIND.ORIGINAL:
                currentBet *= 2;
                break;
            case BET_KIND.DOUBLEDOWN:
                currentBet *= 2;
                break;
            case BET_KIND.BLACKJACK:
                currentBet = currentBet * 2 + currentBet / 2;
                break;
            default:
                //Debug.Log("넌 뭐니 in RewardOrigin() of BettingZone");
                break;
        }
    }
    public void RewardInsurance()
    {
        if (insuranceBet == 0)
        {
            return;
        }

        insuranceBet *= 3;
    }

    public void Collect(out float betChips)
    {
        betChips = currentBet + insuranceBet;
        currentBet = 0;
        insuranceBet = 0;
    }

    /// <summary>
    /// 수치를 초기화한다.
    /// </summary>
    /// <param name="kind"></param>
    public void EmptyBet(BET_KIND kind)
    {
        switch (kind)
        {
            case BET_KIND.ORIGINAL:
                currentBet = 0;
                break;
            case BET_KIND.INSURANCE:
                insuranceBet = 0;
                break;
        }
    }
}
