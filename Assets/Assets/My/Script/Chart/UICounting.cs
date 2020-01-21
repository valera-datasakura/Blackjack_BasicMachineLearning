using UnityEngine;
using System.Collections;

public class UICounting : UIEventTrigger {

    public UISprite[] situations;
    
	public void Init(int countNum)
    {
        for(int i = 0; i < 340; ++i)
        {
            int d_idx = i / 34;
            int p_idx = i % 34;

            //if (countNum > 20 || countNum < -20)
            //{
            //    Debug.Log("카운트 인덱스 범람했잖아 ("+countNum.ToString()+") in Init() of UICounting");
            //    break;
            //}
            
            situations[i].spriteName =
                DB_Manager.Instance.GetSingle(countNum, d_idx, p_idx).BestHand();
        }
        
        for(int i = 0; i < 9; ++i)
        {
            int blackjack_Idx = 23 + (i+1) * 34;
            situations[blackjack_Idx].color = new Color(0f, 0f, 0f, 0f);
        }
    }
}
