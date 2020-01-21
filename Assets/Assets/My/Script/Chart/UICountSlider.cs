using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class UICountSlider : MonoBehaviour{

    public Chart_Controller chartController;

    public UILabel curLabel;
    public UILabel newLabel;
    public Transform leftTransform;
    public Transform rightTransform;

    private float totalTime;

    bool isMoving = false;
    int currentCount = 0;
    float curTime=0f;
    float distBetweenLabel;

    //_______________________________CallBack Function____________________________
	void Awake () {

        distBetweenLabel = 
            Mathf.Abs(curLabel.transform.localPosition.x - newLabel.transform.localPosition.x);

        totalTime = chartController.moveTime + 0.05f;
    }

    //______________________________Slide관련_________________________________
    public void Left()
    {
        SoundManager.Instance.Play("Effect_Button_General");
        if (isMoving)
        {
            return;
        }

        curLabel.text = currentCount.ToString();

        if (currentCount > -20)
        {
            --currentCount;
        }
        else
        {
            currentCount = 20;
        }
        newLabel.text = currentCount.ToString();

        StartCoroutine(CoMove(true));
    }
    public void Right()
    {
        SoundManager.Instance.Play("Effect_Button_General");
        if (isMoving)
        {
            return;
        }

        curLabel.text = currentCount.ToString();

        if (currentCount < 20)
        {
            ++currentCount;
        }
        else
        {
            currentCount = -20;
        }
        newLabel.text = currentCount.ToString();
        
        StartCoroutine(CoMove(false));
    }

    IEnumerator CoMove(bool isLeft)
    {
        isMoving = true;

        Vector3 hidePos;
        float tempDist = distBetweenLabel;

        if (isLeft)
        {
            hidePos = rightTransform.localPosition;
            tempDist *= -1f;
        }
        else
        {
            hidePos = leftTransform.localPosition;
        }

        while (true)
        {
            float factor = curTime / totalTime;
            Vector3 curLabelLerpLPos = Vector3.Lerp(Vector3.zero, hidePos, factor); // 로컬좌표 사용

            curLabel.transform.localPosition = curLabelLerpLPos;
            newLabel.transform.localPosition = new Vector3(
                curLabelLerpLPos.x + tempDist,
                curLabelLerpLPos.y,
                curLabelLerpLPos.z);

            curTime += Time.deltaTime;
            if (curTime >= totalTime * 0.99f)
            {
                isMoving = false;
                curTime = 0;
                curLabel.transform.localPosition = hidePos;
                newLabel.transform.localPosition = Vector3.zero;
                break;
            }

            yield return null;
        }
    }
}
