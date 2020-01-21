using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Chart_Controller : MonoBehaviour{

    // 기능
    // 1. 하위 카운팅들 초기화
    // 2. 좌우 스크롤

    //하위 클래스
    public UICounting[] UICountings;

    //횡 스크롤
    public float moveTime;
    public UILabel currentCount;
    public Transform leftScrollTrans;
    public Transform rightScrollTrans;

    bool isMoving = false;
    int currentUIIndex = 20;  // 0번 카운팅
    int nextUIIndex;

    //클릭
    public UIPopUp popUpUI;
    public Transform clippingMinimum;
    public Transform clippingMaximum;
    public Transform situationMinimum;
    public Transform situationMaximum;

    [HideInInspector]
    public TweenScale curHighlight;

    Vector2 situationSize;
    Vector3 clickPoint;
    Vector3 worldOfScreenMinimum;
    
    
    //__________________________________________Initialize_____________________________________________
    void Start () {
        
        DB_Manager sm = DB_Manager.Instance;

        for(int i = 0; i < DB_Manager.RANGE_CCN_chart; ++i)
        {
            UICountings[i].Init(i + DB_Manager.MIN_CCN_chart);
        }

        worldOfScreenMinimum = UICamera.mainCamera.ScreenToWorldPoint(Vector3.zero);
        UIGrid situationGrid = transform.GetChild(0).GetComponent<UIGrid>();
        situationSize = new Vector2(
            situationMaximum.position.x - situationMinimum.position.x,
            situationMaximum.position.y - situationMinimum.position.y);

        popUpUI.OnExit = DisHighlight;
    }
    
    //__________________________________________횡 스크롤_______________________________________________
    public void Left()
    {
        if (isMoving)
        {
            return;
        }

        StartCoroutine(CoMove(true));
    }
    public void Right()
    {
        if (isMoving)
        {
            return;
        }

        StartCoroutine(CoMove(false));
    }

    IEnumerator CoMove(bool isLeft)
    {
        isMoving = true;

        int curIdx = currentUIIndex;
        SetIndex(isLeft);
        int nextIdx = currentUIIndex; // 인덱스 변경 완료~
        UICounting curUI = UICountings[curIdx];
        UICounting nextUI = UICountings[nextIdx];

        curUI.gameObject.SetActive(true);
        nextUI.gameObject.SetActive(true);

        Vector3 centerPos = curUI.transform.position;
        Vector3 showPos, hidePos;
        Vector3 nextOffset = centerPos - leftScrollTrans.position;
        nextOffset.y = 0f; 
        
        if (isLeft)
        {
            nextOffset *= -1f;
            showPos = rightScrollTrans.position;
            hidePos = leftScrollTrans.position;
        }
        else
        {
            showPos = leftScrollTrans.position;
            hidePos =  rightScrollTrans.position;
        }
        nextUI.transform.position = showPos;
        
        float curTime=0f;
        float factor = 0f;
        
        while (true)
        {
            yield return null;

            curTime += Time.deltaTime;
            factor = curTime / moveTime;
            Vector3 curLerpPos = Vector3.Lerp(centerPos, hidePos, factor);
                
            curUI.transform.position = curLerpPos;
            nextUI.transform.position = curLerpPos - nextOffset;

            if (curTime >= moveTime * 0.99f)
            {
                isMoving = false;
                nextUI.transform.position = centerPos;
                curUI.transform.position = hidePos;
                break;
            }
        }

        curUI.gameObject.SetActive(false);
        currentUIIndex = nextIdx;
    }
    void SetIndex(bool isLeft)
    {
        if (isLeft)
        {
            --currentUIIndex;

            if (currentUIIndex < 0)
            {
                currentUIIndex = 40;
            }
        }
        else
        {
            ++currentUIIndex;

            if (currentUIIndex > 40)
            {
                currentUIIndex = 0;
            }
        }
    }

    //__________________________________________________클릭________________________________________________
    public void OnBeginDrag()
    {
        return;
        clickPoint = Input.mousePosition;
    }
    public void OnEndDrag()
    {
        clickPoint = Input.mousePosition;

        float sqrDist = (clickPoint - Input.mousePosition).sqrMagnitude;
        if (sqrDist < 0.01f)
        {
            clickPoint = UICamera.mainCamera.ScreenToWorldPoint(clickPoint);

            // 클리핑 영역 검사
            if (clippingMinimum.position.x < clickPoint.x && clickPoint.x < clippingMaximum.position.x)
            {
                if(clippingMinimum.position.y < clickPoint.y && clickPoint.y < clippingMaximum.position.y)
                {
                    clickPoint -= situationMinimum.position; // 월드

                    //clickPoint += worldOfScreenMinimum; 
                    
                    //clickPoint = UICamera.mainCamera.WorldToScreenPoint(clickPoint);


                    // 인덱스 계산
                    int d_Idx = Mathf.FloorToInt((clickPoint.x / situationSize.x) * 10);
                    int p_Idx = 33 - Mathf.FloorToInt((clickPoint.y / situationSize.y) * 34);

                    Situation_Info curSitu = DB_Manager.Instance.GetSingle(currentUIIndex - 20, d_Idx, p_Idx);

                    if (p_Idx==23 && d_Idx!=0) // 빈공간
                    {
                        return;
                    }

                    // 명령 전달 - How?
                    popUpUI.gameObject.SetActive(true);
                    
                    popUpUI.Setting(curSitu, d_Idx, p_Idx);

                    // 하이라이트
                    curHighlight = UICountings[currentUIIndex].situations[d_Idx * 34 + p_Idx].gameObject.AddComponent<TweenScale>();
                    curHighlight.from = new Vector3(0.9f, 0.9f, 0.9f);
                    curHighlight.to = new Vector3(1.3f, 1.3f, 1.3f);
                    curHighlight.duration = 0.75f;
                    curHighlight.style = UITweener.Style.PingPong;
                }
            }
        }
    }
    void DisHighlight()
    {
        curHighlight.transform.localScale = new Vector3(1f, 1f, 1f);
        Destroy(curHighlight);
    }
    public void Exit()
    {
        SoundManager.Instance.Play("Effect_Button_PopUp");
        SoundManager.Instance.FadeOutBgm();
        SceneManager.LoadScene("Lobby");
        //Application.LoadLevel("Lobby");
    }
}
