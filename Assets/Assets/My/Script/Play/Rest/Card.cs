using UnityEngine;
using System.Collections;

public class Card : MonoBehaviour {

    private int number;
    private int countingScore = 0;
    
    public int CountingScore {
        get {
            return countingScore;
        }
    }
    public int Number
    {
        get
        {
            return this.number;
        }
    }
    public bool IsHole
    {
        get
        {
            return (transform.up.y < 0) ? true : false;
        }
    }
	public void Setting(int num)
    {
        this.number = num;

        if (2 <= number && number <= 6)
        {
            countingScore = 1;
        }
        else if (number == 10 || number == 1)
        {
            countingScore = -1;
        }
    }
    
    
    #region rest

    GameObject outLine;
    bool isRotating = false;
    bool isMoving = false;
    Vector3 firstForward;
    Vector3 destForward; // y축 회전을 위해
    Vector3 firstUp;
    Vector3 firstPosition;
    Vector3 destPosition;
    float firstZDgree;
    float curTime;
    float totalTime;

    void Awake()
    {
        outLine = (GameObject)Resources.Load("OutLine");
        outLine = (GameObject)Instantiate(outLine, transform.position, transform.rotation);
        outLine.transform.SetParent(this.transform);
        outLine.SetActive(false);
    }
    void Update()
    {
        curTime += Time.deltaTime;
        float t = curTime / totalTime;

        if (isMoving)
        {
            // 이동보간
            transform.position =
                Vector3.Lerp(firstPosition, destPosition, t);

            // 회전보간 Y
            Vector3 tempForward = transform.forward;

            tempForward =
                Vector3.Lerp(firstForward, destForward, t);
            transform.rotation = Quaternion.LookRotation(tempForward, firstUp);

            if (t > 0.95f)
            {
                isMoving = false;

                transform.position = destPosition;
                transform.rotation = Quaternion.LookRotation(destForward, firstUp);
            }
        }

        // 회전보간 Z
        if (isRotating)
        {
            float rotZ =
                Mathf.Lerp(0.0f, -180.0f, t);

            Vector3 firstZRotVector = transform.rotation.eulerAngles;
            firstZRotVector.z = firstZDgree;
            transform.rotation = Quaternion.Euler(firstZRotVector);

            if (t <= 0.95f)
            {
                transform.rotation *= Quaternion.Euler(0.0f, 0.0f, rotZ);
            }
            else
            {
                isRotating = false;
                transform.rotation *= Quaternion.Euler(0.0f, 0.0f, 180.0f);
            }
        }
    }
    public void AddOffset(Vector3 offset)
    {
        destPosition += offset;
    }
    public void Move(Transform toTrans, float time)
    {
        isMoving = true;
        curTime = 0;
        totalTime = time;

        firstPosition = transform.position;
        destPosition = toTrans.position;
        firstForward = transform.forward;
        destForward = toTrans.forward;
        firstUp = transform.up;

    }
    public void Rotate(float time)
    {
        isRotating = true;
        curTime = 0;
        totalTime = time;

        firstUp = transform.up;
        firstZDgree = transform.rotation.eulerAngles.z;
    }

    public void EnHighlight()
    {
        outLine.gameObject.SetActive(true);
    }
    public void DisHighlight()
    {
        outLine.gameObject.SetActive(false);
    }

    #endregion

}
