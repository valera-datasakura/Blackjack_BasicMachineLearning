using UnityEngine;
using System.Collections;
using CardEnums;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public enum PLAY_TURN // 변경시 ChangeTurn() 또한 편집해주기
{
    START,
    BETTING,
    SET_CARD,
    INSURANCE,
    IsBLACKJACK,
    CHOICE,
    DEALER_GET,
    RESULT,
    CLEAR
}

public class PlayController : MonoBehaviour {

    public Player player;
    public Player aiOpponent;


    public Deck deck;
    public DiscardDeck dDeck;
    public DealerHand dealer;

    private const int minBetting = 10;
    private PLAY_TURN turn;
    private bool canChoose = false;

    //------------------개인UI관련(통합관리)--------------------------------
    // 배팅 UI
    public UIPanel bettingPanel;
    public UICenterOnChild bettingButtonCenterSlider;
    public UIButton[] bettingButton;
    public UIButton okButton;
    public UILabel curBettingLabel;
    public UIButton xButton;

    protected TweenColor[] chipButtonTweensColor;
    protected TweenScale[] chipButtonTweensScale;
    protected TweenColor okButtonTween;
    protected TweenColor xButtonTween;

    // Insurance UI
    public UIPanel insurancePanel;

    //-------------------범용UI관련(객체관리)------------------------------
    // 핸드 상태 UI
    public UIPanel playerStateUIParent;
    public UIPanel aiOpponentStateUIParent;
    //------------------------------------------------------

    private int ccn = 0;
    private int holeCcn = 0;

    void Awake()
    {
        okButtonTween = okButton.GetComponent<TweenColor>();
        xButtonTween = xButton.GetComponent<TweenColor>();

        chipButtonTweensColor = new TweenColor[4];
        chipButtonTweensScale = new TweenScale[4];
        for (int i = 0; i < bettingButton.Length; ++i)
        {
            chipButtonTweensColor[i] = bettingButton[i].GetComponent<TweenColor>();
            chipButtonTweensScale[i] = bettingButton[i].GetComponent<TweenScale>();
        }

        SceneManager.LoadScene(SceneNames.calculate, LoadSceneMode.Additive);
        SceneManager.LoadScene(SceneNames.simulate, LoadSceneMode.Additive);
        
        bettingButtonCenterSlider.onCenter = UpdateBettingButtonState;
    }
    void Start()
    {
        player.Init(playerStateUIParent);
        aiOpponent.Init(aiOpponentStateUIParent);

        dealer.Init(playerStateUIParent,"Dealer");

        bettingPanel.gameObject.SetActive(false);

        NextTurn(PLAY_TURN.START);
    }
    void Update()
    {
        switch (turn)
        {
            case PLAY_TURN.CHOICE:

                if (canChoose)
                {
                    if (Input.GetKeyDown(KeyCode.H))
                    {
                        StartCoroutine(CoHit());
                    }
                    if (Input.GetKeyDown(KeyCode.T))
                    {
                        StartCoroutine(CoStand());
                    }
                    if (Input.GetKeyDown(KeyCode.D))
                    {
                        StartCoroutine(CoDoubleDown());
                    }
                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        StartCoroutine(CoSplit());
                    }
                    if (Input.GetKeyDown(KeyCode.U))
                    {
                        StartCoroutine(CoSurrender());
                    }
                }
                break;
        }
    }

    void NextTurn(PLAY_TURN nextTurn)
    {
        turn = nextTurn;

        switch (turn)
        {
            case PLAY_TURN.START:
                StartCoroutine(IE_FirstStart());
                break;
            case PLAY_TURN.BETTING:
                StartCoroutine(Betting());
                break;
            case PLAY_TURN.SET_CARD:
                StartCoroutine(SetCard());
                break;
            case PLAY_TURN.INSURANCE:
                StartCoroutine(Insurance());
                break;
            case PLAY_TURN.IsBLACKJACK:
                StartCoroutine(IsBlackjack());
                break;
            case PLAY_TURN.CHOICE:
                StartCoroutine(Choice());
                break;
            case PLAY_TURN.DEALER_GET:
                StartCoroutine(DealerGet());
                break;
            case PLAY_TURN.RESULT:
                StartCoroutine(Result());
                break;
            case PLAY_TURN.CLEAR:
                StartCoroutine(Clear());
                break;
        }
    }

    IEnumerator IE_FirstStart()
    {
        yield return new WaitForSeconds(1.5f);

        if (player.Budget >= minBetting && aiOpponent.Budget >= minBetting)
        {
            NextTurn(PLAY_TURN.BETTING);
        }
        else
        {
            Exit();
        }
    }
    IEnumerator Betting()
    {
        yield return new WaitForSeconds(0.2f+2f * SoundManager.Instance.Betting() / 3f);

        OpponentBet();

        yield return new WaitForSeconds(0.2f);

        bettingPanel.gameObject.SetActive(true);

        UpdateBettingButtonState(bettingButtonCenterSlider.centeredObject);
    }
    IEnumerator SetCard()
    {
        bettingPanel.gameObject.SetActive(false);

        GetPlayerCard(aiOpponent, false);
        yield return new WaitForSeconds(0.5f);

        GetPlayerCard(player, false);
        yield return new WaitForSeconds(0.5f);

        GetDealerCard(true, false);
        yield return new WaitForSeconds(0.6f);

        GetPlayerCard(aiOpponent, false);
        yield return new WaitForSeconds(0.5f);

        GetPlayerCard(player, false);
        yield return new WaitForSeconds(0.5f);

        GetDealerCard(false, false);
        yield return new WaitForSeconds(0.6f);


        if (dealer.IsInsuranceHand &&
            (aiOpponent.EnoughBetToInsurance || player.EnoughBetToInsurance))
        {
            NextTurn(PLAY_TURN.INSURANCE);
        }
        else
        {
            NextTurn(PLAY_TURN.IsBLACKJACK);
        }
    }
    IEnumerator Insurance()
    {
        yield return new WaitForSeconds(SoundManager.Instance.Insurance()+0.5f);

        if (aiOpponent.EnoughBetToInsurance && GetBestChoice()==ChoiceKind.Insurance)
        {
            aiOpponent.Budget -= aiOpponent.GetCurrentHand.AmountOfBetting / 2;
            aiOpponent.GetCurrentHand.InsuranceBetting();
            SoundManager.Instance.Play("Effect_Chip");

            yield return new WaitForSeconds(0.5f);
        }

        insurancePanel.gameObject.SetActive(true);
    }
    IEnumerator IsBlackjack()
    {
        player.GetCurrentHand.DisplayStateUI(0.0f);
        aiOpponent.GetCurrentHand.DisplayStateUI(0.0f);
        dealer.DisplayStateUI(0.0f);

        yield return new WaitForSeconds(0.2f);

        if ((player.IsBlackjack || aiOpponent.IsBlackjack) && !dealer.IsBlackjack)
        {
            yield return new WaitForSeconds(SoundManager.Instance.P_Blackjack());
        }
        if ((player.IsBlackjack && aiOpponent.IsBlackjack) || dealer.IsBlackjack)
        {
            NextTurn(PLAY_TURN.DEALER_GET);
        }
        else
        {
            NextTurn(PLAY_TURN.CHOICE);
        }
    }
    IEnumerator Choice()
    {
        insurancePanel.gameObject.SetActive(false);

        // AI turn
        do
        {
            aiOpponent.GetCurrentHand.Highlight();

            yield return new WaitForSeconds(0.75f);

            bool isLoop = true;
            do
            {
                switch (GetBestChoice())
                {
                    case ChoiceKind.Hit:

                        GetPlayerCard(aiOpponent, true);

                        yield return new WaitForSeconds(1.0f);

                        isLoop = (!player.GetCurrentHand.IsStopChoice && PreferHit());

                        break;

                    case ChoiceKind.Stand:

                        isLoop = false;

                        break;

                    case ChoiceKind.DoubleDown:

                        aiOpponent.DoubleDown();

                        yield return new WaitForSeconds(SoundManager.Instance.Play("Effect_Chip"));
                       
                        GetPlayerCard(aiOpponent, true);

                        yield return new WaitForSeconds(1.2f);


                        isLoop = false;

                        break;

                    case ChoiceKind.Split:

                        aiOpponent.GetCurrentHand.DisHighlight();

                        PlayerHand secondHand = aiOpponent.Split();
                        SoundManager.Instance.Play("Effect_Chip");
                        yield return new WaitForSeconds(0.5f);
                        GetPlayerCard(aiOpponent, false);
                        yield return new WaitForSeconds(0.7f);

                        Card secondCard = deck.Pop();
                        ccn += secondCard.CountingScore;
                        secondCard.Move(secondHand.transform, 0.5f);
                        secondCard.Rotate(0.5f);
                        secondHand.Push(secondCard, false);
                        SoundManager.Instance.Play("Effect_Card");
                        yield return new WaitForSeconds(1.2f);

                        break;

                    case ChoiceKind.Surrender:

                        aiOpponent.Surrender();
                        aiOpponent.GetCurrentHand.DisplayStateUI(0.5f);
                        yield return new WaitForSeconds(0.4f);

                        isLoop = false;

                        break;

                    case ChoiceKind.NotDetermined:
                        
                        Debug.LogError("Stand is base choice, so whatever else should be determined in this stage");
                        isLoop = false;
                        break;
                }

            } while (isLoop && !aiOpponent.GetCurrentHand.IsStopChoice);

        } while (aiOpponent.UpdateAndCheckAllPossibleHand());


        // player turn
        if (!player.GetCurrentHand.IsStopChoice)
        {
            player.GetCurrentHand.Highlight();

            canChoose = true;
        }
        else
        {
            NextTurn(PLAY_TURN.DEALER_GET);
        }

        yield return null;
    }

    bool PreferHit()
    {
        int player_Idx = FirstHandValue_To_SituationKind.Convert(player.GetCurrentHand.Value, player.GetCurrentHand.IsSoft);

        //  1.
        float standWinningRate = (DB_Manager.Instance.GetSingle(ccn, dealer.GetIndexOfOpen, player_Idx).rate_Stand + 1f) / 2f;

        //  2.
        float hitBurstRate = 0;
        if ((int)player.GetCurrentHand.Value >= 12 && player.GetCurrentHand.IsHard)
        {
            hitBurstRate = DB_Manager.Instance.GetBurst(ccn, (int)player.GetCurrentHand.Value).rate;
        }

        float average =
           ((1f - standWinningRate) * (1f + AI_Controller.HIT_WEIGHT)
            + (1f - hitBurstRate) * (1f - AI_Controller.HIT_WEIGHT))
            / 2f;

        //  3.
        return (average > AI_Controller.HIT_PREFERENCE);
    }
    ChoiceKind GetBestChoice()
    {
        Dictionary<ChoiceKind, float> choiceWinningRates = new Dictionary<ChoiceKind, float>();
      
        Situation_Info info = DB_Manager.Instance.GetSingle(ccn, dealer.GetIndexOfOpen, aiOpponent.GetCurrentHand.GetSituationIndex);

        if (aiOpponent.GetCurrentHand.CanHit)
        {
            choiceWinningRates.Add(ChoiceKind.Hit, info.rate_Hit);
        }
        if (aiOpponent.GetCurrentHand.CanStand)
        {
            choiceWinningRates.Add(ChoiceKind.Stand, info.rate_Stand);
        }
        if (aiOpponent.GetCurrentHand.CanDoubleDown && aiOpponent.EnoughBetToDouble)
        {
            choiceWinningRates.Add(ChoiceKind.DoubleDown, info.rate_DoubleDown);
        }
        if (aiOpponent.GetCurrentHand.CanSplit && aiOpponent.EnoughBetToSplit)
        {
            choiceWinningRates.Add(ChoiceKind.Split, info.rate_Split);
        }
        if (aiOpponent.GetCurrentHand.CanSurrender)
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
        if(best==ChoiceKind.NotDetermined)
        {
            Debug.LogError("Stand is base choice, so whatever else should be determined in this stage");
        }

        //Debug.Log("-------ccn:"+ ccn+"-------dealer:"+ dealer.GetIndexOfOpen+"--------ai:"+ aiOpponent.GetCurrentHand.GetSituationIndex + "---------------------");
        //foreach(var item in choiceWinningRates)
        //{
        //    Debug.Log(item.Key + " = "+item.Value);
        //}
        //Debug.Log("** " + best + " **");

        return best;
    }

    IEnumerator DealerGet()
    {
        insurancePanel.gameObject.SetActive(false);

        dealer.OpenHole(delay:0.4f);
        
        if (CanDealerProgress)
        {
            while (true)
            {
                yield return new WaitForSeconds(1.8f);

                if (dealer.IsStopHitting)
                {
                    break;
                }

                GetDealerCard(false, true);
                dealer.DisplayStateUI(delay: 0.9f);
            }
        }

        if(dealer.Value == HAND_VALUE.BURST_DEALER)
        {
            yield return new WaitForSeconds(SoundManager.Instance.D_Burst());
        }

        NextTurn(PLAY_TURN.RESULT);
    }
    private bool CanDealerProgress
    {
        get
        {
            aiOpponent.ReadyForIteratingHand();
            player.ReadyForIteratingHand();
            if (
                (player.GetCurrentHand.IsBlackjack || player.GetCurrentHand.IsSurrender)&& 
                (aiOpponent.GetCurrentHand.IsBlackjack || aiOpponent.GetCurrentHand.IsSurrender))
                return false;

            do
            {
                if (aiOpponent.GetCurrentHand.IsBurst == false)
                {
                    return true;
                }

            } while (aiOpponent.UpdateToNextHand());

            if (player.GetCurrentHand.IsBlackjack || player.GetCurrentHand.IsSurrender)
                return false;

            do
            {
                if (player.GetCurrentHand.IsBurst == false)
                {
                    return true;
                }

            } while (player.UpdateToNextHand());

            return false;
        }
    }


    IEnumerator Result()
    {
        if (dealer.IsBlackjack)
        {
            aiOpponent.GetCurrentHand.RewardInsurance();
            player.GetCurrentHand.RewardInsurance();
            SoundManager.Instance.Play("Effect_Chip");
        }
        else
        {
            player.GetCurrentHand.LoseBet(BET_KIND.INSURANCE);
        }

        yield return new WaitForSeconds(0.4f);

        if (dealer.IsBlackjack)
        {
            yield return new WaitForSeconds(SoundManager.Instance.D_Blackjack());
        }

        do
        {
            if (aiOpponent.GetCurrentHand > dealer)
            {
                aiOpponent.GetCurrentHand.RewardOrigin(aiOpponent.IsBlackjack ? BET_KIND.BLACKJACK : BET_KIND.ORIGINAL);
                SoundManager.Instance.Play("Effect_Chip");
            }
            else if (aiOpponent.GetCurrentHand < dealer)
            {
                aiOpponent.GetCurrentHand.LoseBet(BET_KIND.ORIGINAL);
            }

            yield return new WaitForSeconds(0.4f);

        } while (aiOpponent.UpdateToNextHand());


        yield return new WaitForSeconds(1.0f);

        int winNum = 0;
        int loseNum = 0;
        do
        {
            if (player.GetCurrentHand > dealer)
            {
                player.GetCurrentHand.RewardOrigin(player.IsBlackjack ? BET_KIND.BLACKJACK : BET_KIND.ORIGINAL);
                SoundManager.Instance.Play("Effect_Chip");

                ++winNum;
            }
            else if (player.GetCurrentHand < dealer)
            {
                player.GetCurrentHand.LoseBet(BET_KIND.ORIGINAL);

                ++loseNum;
            }

            yield return new WaitForSeconds(0.3f);

        } while (player.UpdateToNextHand());

        if (player.GetCurrentHand.IsSurrender == false)
        {
            if (winNum == 0 && loseNum == 0)
            {
                yield return new WaitForSeconds(SoundManager.Instance.Push());
            }
            else if (loseNum == 0)
            {
                yield return new WaitForSeconds(SoundManager.Instance.Win());
            }
            else if (winNum == 0)
            {
                yield return new WaitForSeconds(SoundManager.Instance.Lose());
            }
        }

        yield return new WaitForSeconds(0.5f);

        aiOpponent.CollectBet();
        player.CollectBet();

        NextTurn(PLAY_TURN.CLEAR);
    }

    protected virtual IEnumerator Clear()
    {
        aiOpponent.MoveAll(dDeck.storeTransform, delay: 0.7f);
        yield return new WaitForSeconds(0.2f);
        player.MoveAll(dDeck.storeTransform, delay: 0.7f);
        yield return new WaitForSeconds(0.2f);
        dealer.MoveAll(dDeck.storeTransform, delay: 0.7f);

        yield return new WaitForSeconds(0.7f);

        aiOpponent.DiscardAll(dDeck);
        player.DiscardAll(dDeck);
        dealer.DiscardAll(dDeck);

        ccn += holeCcn;
        holeCcn = 0;

        if (deck.IsShuffleTime)
        {
            deck.Shuffle(dDeck, 1.6f);
            SoundManager.Instance.Play("Effect_Shuffle");

            ccn = 0;

            yield return new WaitForSeconds(2.0f);
        }

        NextTurn(PLAY_TURN.START);
    }
    
    IEnumerator CoHit()
    {
        if (!player.GetCurrentHand.CanHit)
            yield break;

        canChoose = false;

        GetPlayerCard(player, true);

        yield return new WaitForSeconds(1.0f);

        if(player.GetCurrentHand.Value == HAND_VALUE.BURST_PLAYER)
        {
            yield return new WaitForSeconds(SoundManager.Instance.P_Burst());
        }

        if (player.GetCurrentHand.IsStopChoice)
        {
            if (canChoose = player.UpdateAndCheckAllPossibleHand())
            {
                player.GetCurrentHand.Highlight();
            }
            else
            {
                NextTurn(PLAY_TURN.DEALER_GET);
            }
        }
        else
            canChoose = true;
    }
    IEnumerator CoStand()
    {
        if (!player.GetCurrentHand.CanStand)
        {
            Debug.LogError("always possible");
            yield break;
        }

        canChoose = false;

        yield return new WaitForSeconds(0.2f);

        if (canChoose = player.UpdateAndCheckAllPossibleHand())
        {
            player.GetCurrentHand.Highlight();
        }
        else
        {
            NextTurn(PLAY_TURN.DEALER_GET);
        }
    }
    IEnumerator CoDoubleDown()
    {
        if (!player.GetCurrentHand.CanDoubleDown || !player.EnoughBetToDouble)
            yield break;

        canChoose = false;

        player.DoubleDown();

        yield return new WaitForSeconds(SoundManager.Instance.Play("Effect_Chip"));
        yield return new WaitForSeconds(SoundManager.Instance.DoubleDown() + 0.5f);

        GetPlayerCard(player, true);

        yield return new WaitForSeconds(1.2f);

        if(player.GetCurrentHand.Value == HAND_VALUE.BURST_PLAYER)
        {
            yield return new WaitForSeconds(SoundManager.Instance.P_Burst());
        }

        if (canChoose = player.UpdateAndCheckAllPossibleHand())
        {
            player.GetCurrentHand.Highlight();
        }
        else
        {
            NextTurn(PLAY_TURN.DEALER_GET);
        }
    }
    IEnumerator CoSplit()
    {
        if (!player.GetCurrentHand.CanSplit || !player.EnoughBetToSplit)
            yield break;

        canChoose = false;

        player.GetCurrentHand.DisHighlight();

        PlayerHand secondHand = player.Split();
        SoundManager.Instance.Play("Effect_Chip");
        yield return new WaitForSeconds(0.5f);
        GetPlayerCard(player, false);
        yield return new WaitForSeconds(0.7f);

        Card secondCard = deck.Pop();
        ccn += secondCard.CountingScore;
        secondCard.Move(secondHand.transform, 0.5f);
        secondCard.Rotate(0.5f);
        secondHand.Push(secondCard, false);
        SoundManager.Instance.Play("Effect_Card");
        yield return new WaitForSeconds(0.7f);

        player.GetCurrentHand.Highlight(0.0f);

        if (player.GetCurrentHand.IsStopChoice)
        {
            if (canChoose = player.UpdateAndCheckAllPossibleHand())
            {
                player.GetCurrentHand.Highlight();
            }
            else
            {
                NextTurn(PLAY_TURN.DEALER_GET);
            }
        }
        else
            canChoose = true;
    }

    IEnumerator CoSurrender()
    {
        if (!player.GetCurrentHand.CanSurrender)
            yield break;

        canChoose = false;

        player.Surrender();
        player.GetCurrentHand.DisplayStateUI(0.5f);
        yield return new WaitForSeconds(0.4f);

        NextTurn(PLAY_TURN.DEALER_GET);

        yield return null;
    }
   
    void GetPlayerCard(Player curPlayer, bool isEvent)
    {
        Card newCard = deck.Pop();
        ccn += newCard.CountingScore;
        SoundManager.Instance.Play("Effect_Card");
        curPlayer.GetOpenCard(newCard, 0.5f);

        if (isEvent)
        {
            curPlayer.GetCurrentHand.Highlight(delay: 0.6f);
            curPlayer.GetCurrentHand.DisplayStateUI(delay: 0.9f);
        }
    }
    void GetDealerCard(bool isHole, bool isEvent)
    {
        Card newCard = deck.Pop();
        SoundManager.Instance.Play("Effect_Card");
        if (isHole)
        {
            holeCcn = newCard.CountingScore;
            dealer.GetHiddenCard(newCard, 0.5f);
        }
        else
        {
            ccn += newCard.CountingScore;
            dealer.GetOpenCard(newCard, 0.5f);
        }

        if (isEvent)
            dealer.DisplayStateUI(delay: 0.9f);
    }

    public void DoInsurance()
    {
        player.Budget -= player.GetCurrentHand.AmountOfBetting / 2;
        player.GetCurrentHand.InsuranceBetting();
        SoundManager.Instance.Play("Effect_Chip");

        NextTurn(PLAY_TURN.IsBLACKJACK);
    }
    public void DontInsurance()
    {
        SoundManager.Instance.Play("Effect_Button_General");
        NextTurn(PLAY_TURN.IsBLACKJACK);
    }

    #region rest
    public void Bet10()
    {
        player.Budget -= 10;
        player.GetCurrentHand.Betting(10);
        UpdateBettingButtonState(bettingButtonCenterSlider.centeredObject);

        SoundManager.Instance.Play("Effect_Chip");
    }
    public void OpponentBet()
    {
        aiOpponent.Budget -= 20;
        aiOpponent.GetCurrentHand.Betting(20);

        SoundManager.Instance.Play("Effect_Chip");
    }
    public void Bet50()
    {
        player.Budget -= 50;
        player.GetCurrentHand.Betting(50);
        UpdateBettingButtonState(bettingButtonCenterSlider.centeredObject);

        SoundManager.Instance.Play("Effect_Chip");
    }
    public void Bet100()
    {
        player.Budget -= 100;
        player.GetCurrentHand.Betting(100);
        UpdateBettingButtonState(bettingButtonCenterSlider.centeredObject);

        SoundManager.Instance.Play("Effect_Chip");
    }
    void UpdateBettingButtonState(GameObject curCenterBettingButton)
    {
        DeactivateAllBettingButton();
        ActivateCurBettingButton(curCenterBettingButton);

        UpdateStartButton();

        UpdateCancelButton();
    }
    void UpdateCancelButton()
    {
        if (player.GetCurrentHand.IsBetEmpty)
        {
            xButton.enabled = false;
            xButtonTween.PlayForward();
        }
        else
        {
            xButton.enabled = true;
            xButtonTween.PlayReverse();
        }
    }
    void UpdateStartButton()
    {
        if (player.GetCurrentHand.IsBetInRange)
        {
            okButton.enabled = true;
            okButtonTween.PlayReverse();
        }
        else
        {
            okButton.enabled = false;
            okButtonTween.PlayForward();
        }
        curBettingLabel.text = "$" + player.GetCurrentHand.AmountOfBetting.ToString();
    }
    void DeactivateAllBettingButton()
    {
        for(int i=0; i<bettingButton.Length; ++i)
        {
            chipButtonTweensColor[i].PlayForward();
            chipButtonTweensScale[i].PlayForward();
            bettingButton[i].enabled = false;
        }
    }
    void ActivateCurBettingButton(GameObject curCenterBettingButton)
    {
        if (curCenterBettingButton)
        {
            if(
                (curCenterBettingButton.name == "100" && player.Budget >= 100) ||
                (curCenterBettingButton.name == "50" && player.Budget >= 50) ||
                (curCenterBettingButton.name == "10" && player.Budget >= 10))
            {
                curCenterBettingButton.GetComponent<TweenColor>().PlayReverse();
                curCenterBettingButton.GetComponent<UIButton>().enabled = true;
            }

            curCenterBettingButton.GetComponent<TweenScale>().PlayReverse();
        }
    }

    public void OnClickBetAndStartGame()
    {
        SoundManager.Instance.Play("Effect_Button_General");
        NextTurn(PLAY_TURN.SET_CARD);
    }
    public void OnClickCancelBetting()
    {
        player.GetCurrentHand.Collect(out int amount);
        player.Budget += amount;

        SoundManager.Instance.Play("Effect_Button_General");
        UpdateBettingButtonState(bettingButtonCenterSlider.centeredObject);
    }

    public void Exit()
    {
        SoundManager.Instance.FadeOutBgm();
        SoundManager.Instance.Play("Effect_Button_General");
        SceneManager.LoadScene("Lobby");
        //Application.LoadLevel("Lobby");

    }
    #endregion
}
