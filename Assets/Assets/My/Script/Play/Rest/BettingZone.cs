using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BET_KIND
{
    NOTHING,
    ORIGINAL,
    DOUBLEDOWN,
    INSURANCE,
    BLACKJACK
}
public class BettingZone : MonoBehaviour {

    public GameObject chip10;
    public GameObject chip50;
    public GameObject chip100;
    public GameObject chip500;
    

    Transform insuranceBetTransform;
    
    Vector2 betRange = new Vector2(10, 1000);
    int currentBet;
    int insuranceBet;
    float chipHeight = 0.05f;
    float chipRandomDistance = 0.02f;
    float closeChipDistance = 0.31f;
        
    List<Chip> originalChips = new List<Chip>();
    List<Chip> insuranceChips = new List<Chip>();
    
    public void Init(Transform trans)
    {
        this.insuranceBetTransform = trans;
    }

    public int AmountOfBetting
    {
        get
        {
            return currentBet;
        }
    }
    public int AmountOfInsurance
    {
        get
        {
            return insuranceBet;
        }
    }

    public bool IsBettingProper
    {
        get
        {
            return (betRange.x <= currentBet && currentBet <= betRange.y);
        }
    }
    public bool IsEmpty
    {
        get
        {
            if (currentBet > 0)
            {
                return false;
            }
            return true;
        }
    }

    //Pure betting setting
    public void Betting(int amount)
    {
        currentBet += amount;
        
        BetStack(BET_KIND.ORIGINAL);
    }
    public void DoubleDownBetting()
    {
        //if (currentBet <= 0)
        //{
        //    Debug.Log("뭔일이 있었던 거야 in DoubleDownBetting() of BettingZone");
        //}

        currentBet *= 2;

        BetStack(BET_KIND.DOUBLEDOWN);
    }
    public void SurrenderBetting()
    {
        //if (currentBet <= 0 || insuranceBet != 0)
        //{
        //    Debug.Log("뭔일이 있었던 거야 in Surrender() of BettingZone");
        //}

        currentBet = 0;
    }
    public void InsuranceBetting()
    {
        //if(currentBet <= 0 || insuranceBet != 0)
        //{
        //    Debug.Log("뭔일이 있었던 거야 in InsuranceBetting() of BettingZone");
        //}

        insuranceBet = currentBet / 2;

        BetStack(BET_KIND.INSURANCE);
    }

    public void RewardOrigin(BET_KIND kind)
    {
        switch (kind)
        {
            case BET_KIND.ORIGINAL:
                currentBet *= 2;
                RewardStack(BET_KIND.ORIGINAL);
                break;
            case BET_KIND.DOUBLEDOWN:
                currentBet *= 2;
                RewardStack(BET_KIND.DOUBLEDOWN);
                break;
            case BET_KIND.BLACKJACK:
                currentBet = currentBet * 2 + currentBet / 2;
                RewardStack(BET_KIND.ORIGINAL);
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
            // 이 함수는 통일성과 코드의 간략화를 위해 Result턴마다 항상 호출을 해준다
            // 그래서 배팅이 없을때 예외처리 
        }

        insuranceBet *= 3;

        RewardStack(BET_KIND.INSURANCE);
    }

    public void Collect(out int betChips)
    {
        betChips = currentBet+insuranceBet;
        currentBet = 0;
        insuranceBet = 0;

        DeleteChips(BET_KIND.ORIGINAL);
        DeleteChips(BET_KIND.INSURANCE);
    }

    void DeleteChips(BET_KIND kind)
    {
        switch (kind)
        {
            case BET_KIND.ORIGINAL: case BET_KIND.DOUBLEDOWN:

                for (int i = 0; i < originalChips.Count; ++i)
                {
                    Destroy(originalChips[i].gameObject);
                }

                originalChips.Clear();
                break;
            case BET_KIND.INSURANCE:
                
                for (int i = 0; i < insuranceChips.Count; ++i)
                {
                    Destroy(insuranceChips[i].gameObject);
                }
                
                insuranceChips.Clear();
                break;
        }
    }

    /// <summary>
    /// 배팅 메쉬, 수치를 초기화한다.
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

        DeleteChips(kind);
    }

    void BetStack(BET_KIND kind) // 현재 베팅액만큼 칩 메쉬를 쌓음
    {
        DeleteChips(kind);
        
        int tempCurBet = 0;
        int stackCount = 0;
        Chip newChip;
        switch (kind)
        {
            case BET_KIND.ORIGINAL:
                tempCurBet = currentBet;
                while (tempCurBet >= 500) // 500짜리
                {
                    newChip = Chip500();
                    Vector3 stackPos = newChip.transform.position;
                    stackPos.y += chipHeight * (stackCount++);
                    newChip.transform.position = stackPos;
                    newChip.transform.SetParent(this.transform);
                    originalChips.Add(newChip);

                    tempCurBet -= 500;
                }
                while (tempCurBet >= 100) // 100짜리
                {
                    newChip = Chip100();
                    Vector3 stackPos = newChip.transform.position;
                    stackPos.y += chipHeight * (stackCount++);
                    newChip.transform.position = stackPos;
                    newChip.transform.SetParent(this.transform);
                    originalChips.Add(newChip);

                    tempCurBet -= 100;
                }
                while (tempCurBet >= 50) // 50짜리
                {
                    newChip = Chip50();
                    Vector3 stackPos = newChip.transform.position;
                    stackPos.y += chipHeight * (stackCount++);
                    newChip.transform.position = stackPos;
                    newChip.transform.SetParent(this.transform);
                    originalChips.Add(newChip);

                    tempCurBet -= 50;
                }
                while (tempCurBet >= 10) // 10짜리
                {
                    newChip = Chip10();
                    Vector3 stackPos = newChip.transform.position;
                    stackPos.y += chipHeight * (stackCount++);
                    newChip.transform.position = stackPos;
                    newChip.transform.SetParent(this.transform);
                    originalChips.Add(newChip);

                    tempCurBet -= 10;
                }
                break;
            case BET_KIND.DOUBLEDOWN:
                for (int i = 0; i < 2; ++i)
                {
                    tempCurBet = currentBet / 2;
                    while (tempCurBet >= 500) // 500짜리
                    {
                        newChip = Chip500();
                        Vector3 stackPos =
                            newChip.transform.position + (Vector3.left * closeChipDistance / 2.0f) + (Vector3.right * closeChipDistance) * i;
                        stackPos.y += chipHeight * (stackCount++);
                        newChip.transform.position = stackPos;
                        newChip.transform.SetParent(this.transform);
                        originalChips.Add(newChip);

                        tempCurBet -= 500;
                    }
                    while (tempCurBet >= 100) // 100짜리
                    {
                        newChip = Chip100();
                        Vector3 stackPos =
                            newChip.transform.position + (Vector3.left * closeChipDistance / 2.0f) + (Vector3.right * closeChipDistance) * i;
                        stackPos.y += chipHeight * (stackCount++);
                        newChip.transform.position = stackPos;
                        newChip.transform.SetParent(this.transform);
                        originalChips.Add(newChip);

                        tempCurBet -= 100;
                    }
                    while (tempCurBet >= 50) // 50짜리
                    {
                        newChip = Chip50();
                        Vector3 stackPos =
                            newChip.transform.position + (Vector3.left * closeChipDistance / 2.0f) + (Vector3.right * closeChipDistance) * i;
                        stackPos.y += chipHeight * (stackCount++);
                        newChip.transform.position = stackPos;
                        newChip.transform.SetParent(this.transform);
                        originalChips.Add(newChip);

                        tempCurBet -= 50;
                    }
                    while (tempCurBet >= 10) // 10짜리
                    {
                        newChip = Chip10();
                        Vector3 stackPos =
                           newChip.transform.position + (Vector3.left * closeChipDistance / 2.0f) + (Vector3.right * closeChipDistance) * i;
                        stackPos.y += chipHeight * (stackCount++);
                        newChip.transform.position = stackPos;
                        newChip.transform.SetParent(this.transform);
                        originalChips.Add(newChip);

                        tempCurBet -= 10;
                    }
                }
                break;
            default: // Insurance
                tempCurBet = insuranceBet;
                while (tempCurBet >= 500) // 500짜리
                {
                    newChip = Chip500();
                    Vector3 stackPos =
                        newChip.transform.position -
                        this.transform.position +
                        insuranceBetTransform.position;
                    stackPos.y += chipHeight * (stackCount++);
                    newChip.transform.position = stackPos;
                    newChip.transform.SetParent(this.transform);
                    insuranceChips.Add(newChip);

                    tempCurBet -= 500;
                }
                while (tempCurBet >= 100) // 100짜리
                {
                    newChip = Chip100();
                    Vector3 stackPos =
                        newChip.transform.position -
                        this.transform.position +
                
                        insuranceBetTransform.position;
                    stackPos.y += chipHeight * (stackCount++);
                    newChip.transform.position = stackPos;
                    newChip.transform.SetParent(this.transform);
                    insuranceChips.Add(newChip);

                    tempCurBet -= 100;
                }
                while (tempCurBet >= 50) // 50짜리
                {
                    newChip = Chip50();
                    Vector3 stackPos =
                        newChip.transform.position -
                        this.transform.position +
                        insuranceBetTransform.position;
                    stackPos.y += chipHeight * (stackCount++);
                    newChip.transform.position = stackPos;
                    newChip.transform.SetParent(this.transform);
                    insuranceChips.Add(newChip);

                    tempCurBet -= 50;
                }
                while (tempCurBet >= 10) // 10짜리
                {
                    newChip = Chip10();
                    Vector3 stackPos =
                        newChip.transform.position -
                        this.transform.position +
                        insuranceBetTransform.position;
                    stackPos.y += chipHeight * (stackCount++);
                    newChip.transform.position = stackPos;
                    newChip.transform.SetParent(this.transform);
                    insuranceChips.Add(newChip);

                    tempCurBet -= 10;
                }
                break;
        }// switch
    }
    void RewardStack(BET_KIND kind)
    {
        DeleteChips(kind);

        int tempCurBet = 0;
        int stackCount = 0;
        Chip newChip;

        switch (kind)
        {
            case BET_KIND.ORIGINAL:
                for (int i = 0; i < 2; ++i)
                {
                    tempCurBet = currentBet / 2;
                    while (tempCurBet >= 500) // 500짜리
                    {
                        newChip = Chip500();
                        Vector3 stackPos =
                            newChip.transform.position + (Vector3.left * closeChipDistance / 2.0f) + (Vector3.right * closeChipDistance) * i;
                        stackPos.y += chipHeight * (stackCount++);
                        newChip.transform.position = stackPos;
                        newChip.transform.SetParent(this.transform);
                        originalChips.Add(newChip);

                        tempCurBet -= 500;
                    }
                    while (tempCurBet >= 100) // 100짜리
                    {
                        newChip = Chip100();
                        Vector3 stackPos =
                            newChip.transform.position + (Vector3.left * closeChipDistance / 2.0f) + (Vector3.right * closeChipDistance) * i;
                        stackPos.y += chipHeight * (stackCount++);
                        newChip.transform.position = stackPos;
                        newChip.transform.SetParent(this.transform);
                        originalChips.Add(newChip);

                        tempCurBet -= 100;
                    }
                    while (tempCurBet >= 50) // 50짜리
                    {
                        newChip = Chip50();
                        Vector3 stackPos =
                            newChip.transform.position + (Vector3.left * closeChipDistance / 2.0f) + (Vector3.right * closeChipDistance) * i;
                        stackPos.y += chipHeight * (stackCount++);
                        newChip.transform.position = stackPos;
                        newChip.transform.SetParent(this.transform);
                        originalChips.Add(newChip);

                        tempCurBet -= 50;
                    }
                    while (tempCurBet >= 10) // 10짜리
                    {
                        newChip = Chip10();
                        Vector3 stackPos =
                           newChip.transform.position + (Vector3.left * closeChipDistance / 2.0f) + (Vector3.right * closeChipDistance) * i;
                        stackPos.y += chipHeight * (stackCount++);
                        newChip.transform.position = stackPos;
                        newChip.transform.SetParent(this.transform);
                        originalChips.Add(newChip);

                        tempCurBet -= 10;
                    }
                }
                break;
            case BET_KIND.DOUBLEDOWN:
                for (int i = 0; i < 2; ++i)
                {
                    for (int j = 0; j < 2; ++j)
                    {
                        tempCurBet = currentBet / 4;
                        while (tempCurBet >= 500) // 500짜리
                        {
                            newChip = Chip500();
                            Vector3 stackPos =
                                newChip.transform.position +
                                (Vector3.left * closeChipDistance / 2.0f) + (Vector3.right * closeChipDistance) * i +
                                (Vector3.back * closeChipDistance / 2.0f) + (Vector3.forward * closeChipDistance) * j;
                            stackPos.y += chipHeight * (stackCount++);
                            newChip.transform.position = stackPos;
                            newChip.transform.SetParent(this.transform);
                            originalChips.Add(newChip);

                            tempCurBet -= 500;
                        }
                        while (tempCurBet >= 100) // 100짜리
                        {
                            newChip = Chip100();
                            Vector3 stackPos =
                                newChip.transform.position +
                                (Vector3.left * closeChipDistance / 2.0f) + (Vector3.right * closeChipDistance) * i +
                                (Vector3.back * closeChipDistance / 2.0f) + (Vector3.forward * closeChipDistance) * j;
                            stackPos.y += chipHeight * (stackCount++);
                            newChip.transform.position = stackPos;
                            newChip.transform.SetParent(this.transform);
                            originalChips.Add(newChip);

                            tempCurBet -= 100;
                        }
                        while (tempCurBet >= 50) // 50짜리
                        {
                            newChip = Chip50();
                            Vector3 stackPos =
                                newChip.transform.position +
                                (Vector3.left * closeChipDistance / 2.0f) + (Vector3.right * closeChipDistance) * i +
                                (Vector3.back * closeChipDistance / 2.0f) + (Vector3.forward * closeChipDistance) * j;
                            stackPos.y += chipHeight * (stackCount++);
                            newChip.transform.position = stackPos;
                            newChip.transform.SetParent(this.transform);
                            originalChips.Add(newChip);

                            tempCurBet -= 50;
                        }
                        while (tempCurBet >= 10) // 10짜리
                        {
                            newChip = Chip10();
                            Vector3 stackPos =
                                newChip.transform.position +
                                (Vector3.left * closeChipDistance / 2.0f) + (Vector3.right * closeChipDistance) * i +
                                (Vector3.back * closeChipDistance / 2.0f) + (Vector3.forward * closeChipDistance) * j;
                            stackPos.y += chipHeight * (stackCount++);
                            newChip.transform.position = stackPos;
                            newChip.transform.SetParent(this.transform);
                            originalChips.Add(newChip);

                            tempCurBet -= 10;
                        }
                    }
                }
                break;
            default: // Insurance
                for (int i = 0; i < 3; ++i)
                {
                    tempCurBet = insuranceBet / 3;
                    Quaternion rot = Quaternion.Euler(0.0f, 120.0f * i, 0.0f);
                    while (tempCurBet >= 500) // 500짜리
                    {
                        newChip = Chip500();
                        Vector3 stackPos =
                            newChip.transform.position -
                            this.transform.position +
                            insuranceBetTransform.position +
                            rot * Vector3.forward;
                        stackPos.y += chipHeight * (stackCount++);
                        newChip.transform.position = stackPos;
                        newChip.transform.SetParent(this.transform);
                        insuranceChips.Add(newChip);

                        tempCurBet -= 500;
                    }
                    while (tempCurBet >= 100) // 100짜리
                    {
                        newChip = Chip100();
                        Vector3 stackPos =
                            newChip.transform.position -
                            this.transform.position +
                            insuranceBetTransform.position;
                        stackPos.y += chipHeight * (stackCount++);
                        newChip.transform.position = stackPos;
                        newChip.transform.SetParent(this.transform);
                        insuranceChips.Add(newChip);

                        tempCurBet -= 100;
                    }
                    while (tempCurBet >= 50) // 50짜리
                    {
                        newChip = Chip50();
                        Vector3 stackPos =
                            newChip.transform.position -
                            this.transform.position +
                            insuranceBetTransform.position;
                        stackPos.y += chipHeight * (stackCount++);
                        newChip.transform.position = stackPos;
                        newChip.transform.SetParent(this.transform);
                        insuranceChips.Add(newChip);

                        tempCurBet -= 50;
                    }
                    while (tempCurBet >= 10) // 10짜리
                    {
                        newChip = Chip10();
                        Vector3 stackPos =
                            newChip.transform.position -
                            this.transform.position +
                            insuranceBetTransform.position;
                        stackPos.y += chipHeight * (stackCount++);
                        newChip.transform.position = stackPos;
                        newChip.transform.SetParent(this.transform);
                        insuranceChips.Add(newChip);

                        tempCurBet -= 10;
                    }
                }
                break;
        }// switch
    }

    Chip Chip10()
    {
        Vector3 randPos =
            transform.position+
            new Vector3(
                Random.Range(-chipRandomDistance, chipRandomDistance),
                0.0f,
                Random.Range(-chipRandomDistance, chipRandomDistance)
                );
        Quaternion randRot =
            transform.rotation *
            Quaternion.Euler(
                0.0f,
                Random.Range(0, 360),
                0.0f
                );
        GameObject newObj = (GameObject)Instantiate(chip10, randPos, randRot);
        Chip newChip = newObj.GetComponent<Chip>();
        return newChip;
    }
    Chip Chip50()
    {
        Vector3 randPos =
            transform.position +
            new Vector3(
                Random.Range(-chipRandomDistance, chipRandomDistance),
                0.0f,
                Random.Range(-chipRandomDistance, chipRandomDistance)
                );
        Quaternion randRot =
            transform.rotation *
            Quaternion.Euler(
                0.0f,
                Random.Range(0, 360),
                0.0f
                );
        GameObject newObj = (GameObject)Instantiate(chip50, randPos, randRot);
        Chip newChip = newObj.GetComponent<Chip>();
        return newChip;
    }
    Chip Chip100()
    {
        Vector3 randPos =
            transform.position +
            new Vector3(
                Random.Range(-chipRandomDistance, chipRandomDistance),
                0.0f,
                Random.Range(-chipRandomDistance, chipRandomDistance)
                );
        Quaternion randRot =
            transform.rotation *
            Quaternion.Euler(
                0.0f,
                Random.Range(0, 360),
                0.0f
                );
        GameObject newObj = (GameObject)Instantiate(chip100, randPos, randRot);
        Chip newChip = newObj.GetComponent<Chip>();
        return newChip;
    }
    Chip Chip500()
    {
        Vector3 randPos =
            transform.position +
            new Vector3(
                Random.Range(-chipRandomDistance, chipRandomDistance),
                0.0f,
                Random.Range(-chipRandomDistance, chipRandomDistance)
                );
        Quaternion randRot =
            transform.rotation *
            Quaternion.Euler(
                0.0f,
                Random.Range(0, 360),
                0.0f
                );
        GameObject newObj = (GameObject)Instantiate(chip500, randPos, randRot);
        Chip newChip = newObj.GetComponent<Chip>();
        return newChip;
    }
}
