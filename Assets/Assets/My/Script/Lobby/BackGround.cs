using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BackGround : MonoBehaviour {

    public UISprite backImage1;
    public UISprite backImage2;

    public float maintainingTime=6.5f;
    public float changingTime=2.0f;
    
    List<string> spriteList=new List<string>();
    int currentSpriteIndex;
    float currentTime;

    //___________________________Initialize_______________________________________________
	void Start () {

        spriteList.Add("Background1");
        spriteList.Add("Background2");
        spriteList.Add("Background3");
        spriteList.Add("Background4");
        spriteList.Add("Background5");

        backImage1.spriteName = CurrentSprite();
        backImage1.depth = 0;
        backImage2.spriteName = CurrentSprite();
        backImage2.depth = -1;

        StartCoroutine(CoChange());
    }

    //_____________________________배경 관련___________________________________________________
    string CurrentSprite()
    {
        int curIdx = currentSpriteIndex;
        ++currentSpriteIndex;
        if (currentSpriteIndex >= spriteList.Count)
        {
            currentSpriteIndex = 0;
        }

        return spriteList[curIdx];
    }

    IEnumerator CoChange()
    {
        while (true)
        {
            // 지속 시간
            yield return new WaitForSeconds(maintainingTime);

            // 변경 시간
            while (true)
            {
                yield return new WaitForSeconds(changingTime * Time.deltaTime);

                backImage1.alpha -= Time.deltaTime / changingTime;
                backImage2.alpha += Time.deltaTime / changingTime; // 뒷 이미지가 나옴

                currentTime += Time.deltaTime;
                if (currentTime >= changingTime)
                {
                    currentTime = 0.0f;
                    break;
                }
            }

            UISprite tempImage = backImage1;
            backImage1 = backImage2;
            backImage2 = tempImage;

            backImage2.spriteName = CurrentSprite();
        }
    }

    
}
