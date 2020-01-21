using UnityEngine;
using CardEnums;

public class UIPopUp : MonoBehaviour {
    
    [System.Serializable]
    public struct UI_CHOICE_INFO
    {
        public UIPanel window;
        public UILabel numberUI;
        public UIProgressBar progressLeft;
        public UIProgressBar progressRight;
    }

    // 팝업창 자식들
    // 순서
    // 0-Hit
    // 1-Stand
    // 2-DoubleDown
    // 3-Split 
    // 4-Insurance
    // 5-Surrender
    public UI_CHOICE_INFO[] UIChoiceInfo;
    public UILabel dealerCardLabel;
    public UILabel playerCardLabel;

    [HideInInspector]
    public delegate void OnExitCallBack();
    [HideInInspector]
    public OnExitCallBack OnExit; 


    //_____________________________Initialize_________________________________________
    void Awake()
    {
    }
    void OnEnable()
    {
        SoundManager.Instance.Play("Effect_Button_PopUp");
    }

    //_______________________________Pop Up___________________________________________
	public void Setting(Situation_Info situation, int d_card, int p_card)
    {
        dealerCardLabel.text = (d_card+1).ToString();
        playerCardLabel.text = ((SITUATION_KIND)p_card).ToString();

        for(int i=0;i< UIChoiceInfo.Length; ++i)
        {
            UIChoiceInfo[i].window.alpha = 1f;
        }
        SetUIChoice(UIChoiceInfo[0], situation.total_Hit, situation.rate_Hit);
        SetUIChoice(UIChoiceInfo[1], situation.total_Stand, situation.rate_Stand);
        SetUIChoice(UIChoiceInfo[2], situation.total_DoubleDown, situation.rate_DoubleDown);

        if (p_card < 24) // Nope
        {
            UIChoiceInfo[3].window.alpha = 0.15f;
        }
        else // 스플릿
        {
            SetUIChoice(UIChoiceInfo[3], situation.total_Split, situation.rate_Split);
        }


        if (d_card != 0) // Nope
        {
            UIChoiceInfo[4].window.alpha = 0.15f;
        }
        else // 인슈어런스
        {
            if (p_card == (int)SITUATION_KIND.BLACKJACK)
            {
                UIChoiceInfo[0].window.alpha = 0.15f;
                UIChoiceInfo[2].window.alpha = 0.15f;
                UIChoiceInfo[5].window.alpha = 0.15f;
            }
            
            SetUIChoice(UIChoiceInfo[4], situation.total_Insurance, situation.rate_Insurance);
        }
    }
	
	void SetUIChoice(UI_CHOICE_INFO UIChoice, int total, float rate)
    {
        UIChoice.numberUI.text =
                total.ToString() + "/" + rate.ToString("f3");

        rate /= 2f; // 최대 수익(-2~2) 설정
        if (rate < 0f)
        {
            UIChoice.progressLeft.value = -rate;
            UIChoice.progressRight.value = 0f;
        }
        else
        {
            UIChoice.progressRight.value = rate;
            UIChoice.progressLeft.value = 0f;
        }
    }

    //_________________________________________Button_________________________________________________________
    public void Exit()
    {
        OnExit();
        gameObject.SetActive(false);
    }
}
