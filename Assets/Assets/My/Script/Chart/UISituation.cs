using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class UISituation : MonoBehaviour{

    int playerCard;
    int dealerCard;
    Situation_Info information;
    Vector2 clickEnterPoint;

    public void Init(int p_Hand, int d_Hand, Situation_Info info)
    {
        playerCard = p_Hand;
        dealerCard = d_Hand;
        information = info;

        // 베스트 초이스 출력
        GetComponent<UISprite>().spriteName = info.bestChoice;
    }
}
