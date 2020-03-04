using UnityEngine;
using System.Collections;
using CardEnums;
using UnityEngine.SceneManagement;

public enum PLAY_TURN // 변경시 ChangeTurn() 또한 편집해주기
{
    SETTING=0,
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

    const int minBetting = 10;

    //------------------기본-----------------------------
    public GameObject handPrefab;
    
    //---------------------------------------------------
    public Deck deck;
    public DiscardDeck dDeck;
    public DealerHand dealer;

    protected PLAY_TURN turn=PLAY_TURN.SETTING;
    protected bool canChoose = false;

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
    public UIPanel stateUIParent;
    //------------------------------------------------------
    
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
        
        bettingButtonCenterSlider.onCenter = UpdateBettingButtonState;
    }
    void Start()
    {
        player.Init(stateUIParent);

        dealer.Init(stateUIParent,"Dealer");
    }
    void Update()
    {
        UpdateTurn();    
    }
    void OnApplicationQuit()
    {   
    }

    void NextTurn(PLAY_TURN nextTurn)
    {
        turn = nextTurn;

        switch (turn)
        {
            case PLAY_TURN.SETTING:
                break;
            case PLAY_TURN.START:
                StartCoroutine(FirstStart());
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
    
    IEnumerator FirstStart()
    {
        if(player.Budget >= minBetting)
        {
            NextTurn(PLAY_TURN.BETTING);
        }
        else
        {
            Exit();
        }

        yield return null;
    }
    IEnumerator Betting()
    {
        yield return new WaitForSeconds(0.2f + 2f * SoundManager.Instance.Betting() / 3f);

        bettingPanel.gameObject.SetActive(true);

        UpdateBettingButtonState(bettingButtonCenterSlider.centeredObject);
    }
    IEnumerator SetCard()
    {
        bettingPanel.gameObject.SetActive(false);

        player.GetOpenCard(deck.Pop(), delay: 0.5f);
        SoundManager.Instance.Play("Effect_Card");
        yield return new WaitForSeconds(0.9f);

        dealer.GetHiddenCard(deck.Pop(), delay: 0.5f);
        SoundManager.Instance.Play("Effect_Card");
        yield return new WaitForSeconds(0.9f);

        player.GetOpenCard(deck.Pop(), delay: 0.5f);
        SoundManager.Instance.Play("Effect_Card");
        yield return new WaitForSeconds(0.9f);

        dealer.GetOpenCard(deck.Pop(), delay: 0.5f);
        SoundManager.Instance.Play("Effect_Card");
        yield return new WaitForSeconds(0.8f);

        if(dealer.IsInsuranceHand && player.EnoughBetToInsurance)
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
        yield return new WaitForSeconds(SoundManager.Instance.Insurance());

        insurancePanel.gameObject.SetActive(true);
    }
    IEnumerator IsBlackjack()
    {
        player.GetCurrentHand.DisplayStateUI(0.0f);
        dealer.DisplayStateUI(0.0f);

        yield return new WaitForSeconds(0.2f);

        if (player.IsBlackjack)
        {
            yield return new WaitForSeconds(SoundManager.Instance.P_Blackjack());
            NextTurn(PLAY_TURN.DEALER_GET);
        }
        else if (dealer.IsBlackjack)
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
        player.GetCurrentHand.Highlight();

        canChoose = true;

        yield return null;
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

                dealer.GetOpenCard(deck.Pop(), delay: 0.6f);
                dealer.DisplayStateUI(delay: 0.9f);
                SoundManager.Instance.Play("Effect_Card");
            }
        }

        if(dealer.Value == HAND_VALUE.BURST_DEALER)
        {
            yield return new WaitForSeconds(SoundManager.Instance.D_Burst());
        }

        NextTurn(PLAY_TURN.RESULT);
    }
    bool CanDealerProgress
    {
        get
        {
            player.ReadyForIteratingHand();
            
            if (player.GetCurrentHand.IsBlackjack ||
                player.GetCurrentHand.IsSurrender)
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

        int winNum = 0;
        int loseNum = 0;
        do
        {
            if (PlayerWin)
            {
                player.GetCurrentHand.RewardOrigin(player.IsBlackjack ? BET_KIND.BLACKJACK : BET_KIND.ORIGINAL);
                SoundManager.Instance.Play("Effect_Chip");

                ++winNum;
            }
            else if (DealerWin)
            {
                player.GetCurrentHand.LoseBet(BET_KIND.ORIGINAL);

                ++loseNum;
            }

            yield return new WaitForSeconds(0.3f);

        } while (player.UpdateToNextHand());

        if(player.GetCurrentHand.IsSurrender == false)
        {
            if(winNum ==0 && loseNum == 0)
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

        yield return new WaitForSeconds(0.4f);

        player.CollectBet();

        NextTurn(PLAY_TURN.CLEAR);
    }
    bool DealerWin
    {
        get
        {
            return (dealer > player.GetCurrentHand);
        }
    }
    bool PlayerWin
    {
        get
        {
            return (dealer < player.GetCurrentHand);
        }
    }

    protected virtual IEnumerator Clear()
    {
        dealer.MoveAll(dDeck.storeTransform, delay: 0.7f);
        player.MoveAll(dDeck.storeTransform, delay: 0.7f);

        yield return new WaitForSeconds(0.7f);

        dealer.DiscardAll(dDeck);
        player.DiscardAll(dDeck);

        if (deck.IsShuffleTime)
        {
            deck.Shuffle(dDeck, 1.6f);
            SoundManager.Instance.Play("Effect_Shuffle");

            yield return new WaitForSeconds(2.0f);
        }

        NextTurn(PLAY_TURN.START);
    }
    
    void UpdateTurn()
    {

        switch (turn)
        {
            case PLAY_TURN.SETTING:

                if(player != null)
                {
                    NextTurn(PLAY_TURN.CHOICE);
                }
                break;
                
            case PLAY_TURN.CHOICE:

                if (Input.GetKeyDown(KeyCode.H))
                {
                    Hit();
                }
                if (Input.GetKeyDown(KeyCode.T))
                {
                    Stand();
                }
                if (Input.GetKeyDown(KeyCode.D))
                {
                    DoubleDown();
                }
                if (Input.GetKeyDown(KeyCode.S))
                {
                    Split();
                }
                if (Input.GetKeyDown(KeyCode.U))
                {
                    Surrender();
                }
                break;
        }
    }

    //--------------------------------UI, Button 관련-------------------------------------------------------------
    // Button
    public void Bet10()
    {
        player.Budget -= 10;
        player.GetCurrentHand.Betting(10);
        UpdateBettingButtonState(bettingButtonCenterSlider.centeredObject);

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
    public void Bet500()
    {
        player.Budget -= 500;
        player.GetCurrentHand.Betting(500);
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
            if((curCenterBettingButton.name == "500" && player.Budget >= 500) ||
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

    // Choice들(걸러진 value를 사용해 Coroutine호출)
    public void Hit()
    {
        if(canChoose == false)
        {
            return;
        }

        if (player.GetCurrentHand.CanHit)
        {
            StartCoroutine(CoHit());
        }
    }
    public void Stand()
    {
        if(canChoose == false)
        {
            return;
        }

        if (player.GetCurrentHand.CanStand)
        {
            StartCoroutine(CoStand());
        }
    }
    public void DoubleDown()
    {
        if(canChoose == false)
        {
            return;
        }

        if(player.GetCurrentHand.CanDoubleDown && player.EnoughBetToDouble)
        {
            StartCoroutine(CoDoubleDown());
        }
    }
    public void Split()
    {
        if(canChoose == false)
        {
            return;
        }

        if(player.GetCurrentHand.CanSplit && player.EnoughBetToSplit)
        {
            StartCoroutine(CoSplit());
        }
    }
    public void Surrender()
    {
        if(canChoose == false)
        {
            return;
        }

        if (player.GetCurrentHand.CanSurrender)
        {
            StartCoroutine(CoSurrender());
        }
    }
    
    // Coroutines(Choice과정 ,조건문없이 바로 적용)
    IEnumerator CoHit()
    {
        canChoose = false;

        GetOneCard();

        yield return new WaitForSeconds(1.2f);

        if(player.GetCurrentHand.Value == HAND_VALUE.BURST_PLAYER)
        {
            yield return new WaitForSeconds(SoundManager.Instance.P_Burst());
        }

        StartCoroutine(UpdateHand());
    }
    IEnumerator CoStand()
    {
        canChoose = player.UpdateAndCheckAllPossibleHand();

        if (canChoose)
        {
            player.GetCurrentHand.Highlight();
        }
        else
        {
            NextTurn(PLAY_TURN.DEALER_GET);
        }

        yield return null;
    }
    IEnumerator CoDoubleDown()
    {
        canChoose = false;

        player.DoubleDown();

        yield return new WaitForSeconds(SoundManager.Instance.Play("Effect_Chip"));
        yield return new WaitForSeconds(SoundManager.Instance.DoubleDown() + 0.5f);

        GetOneCard();

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
        canChoose = false;

        player.GetCurrentHand.DisHighlight();

        PlayerHand secondHand = null;
        secondHand = player.Split(player.GetCurrentHand.IsDoubleAce);
        SoundManager.Instance.Play("Effect_Chip");
        yield return new WaitForSeconds(0.5f);
        player.GetOpenCard(deck.Pop(), 0.5f);
        SoundManager.Instance.Play("Effect_Card");
        yield return new WaitForSeconds(0.7f);

        Card secondCard = deck.Pop();
        secondCard.Move(secondHand.transform, 0.5f);
        secondCard.Rotate(0.5f);
        secondHand.Push(secondCard, isFirstHand: false);
        SoundManager.Instance.Play("Effect_Card");
        yield return new WaitForSeconds(0.7f);

        player.GetCurrentHand.Highlight(0.0f);

        StartCoroutine(UpdateHand());
    }

    IEnumerator CoSurrender()
    {
        canChoose = false;

        player.Surrender();
        player.GetCurrentHand.DisplayStateUI(0.5f);
        yield return new WaitForSeconds(0.4f);

        NextTurn(PLAY_TURN.DEALER_GET);

        yield return null;
    }
    // 특수 Coroutine
    IEnumerator UpdateHand()
    {
        yield return new WaitForSeconds(0.7f);

        canChoose = true;
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
    }

    void GetOneCard()
    {
        player.GetOpenCard(deck.Pop(), delay: 0.5f);
        player.GetCurrentHand.Highlight(delay: 0.6f);
        player.GetCurrentHand.DisplayStateUI(delay: 0.9f);
        SoundManager.Instance.Play("Effect_Card");
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

    public void OnClickBetAndStartGame()
    {
        SoundManager.Instance.Play("Effect_Button_General");
        NextTurn(PLAY_TURN.SET_CARD);
    }
    public void OnClickCancelBetting()
    {
        int amount;
        player.GetCurrentHand.Collect(out amount);
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
}
