using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DiscardDeck : MonoBehaviour
{
    public Transform storeTransform;
    public GameObject discardStack;

    float cardHeight = 0.005f;

    List<Card> discards;

    Vector3 emptyStackHeight;
    Vector3 fullStackHeight;
    float currentTime;
    float totalTime;
    bool isShuffle = false;

    void Start()
    {
        discards = new List<Card>();
        emptyStackHeight = discardStack.transform.position;
    }

    void Update()
    {
        if (isShuffle == false)
        {
            return;
        }

        currentTime += Time.deltaTime;
        float t = currentTime / totalTime;

        Vector3 lerpPos =
            Vector3.Lerp(fullStackHeight, emptyStackHeight, t);

        discardStack.transform.position = lerpPos;

        if (t > 0.9f)
        {
            isShuffle = false;
            discardStack.transform.position = emptyStackHeight;
        }
    }

    public Transform StoreTransform
    {
        get
        {
            return storeTransform;
        }
    }

    public void Discard(Card card) // 이동 끝
    {
        //if (discards.Contains(card))
        //{
        //    Debug.Log("이미 카드가 존재한다 in Push() of Deck");
        //}

        discardStack.transform.Translate(
            0.0f, 
            cardHeight, 
            0.0f);

        card.gameObject.SetActive(false);
        discards.Add(card);
    }
    public void ReturnAll(Deck deck)
    {
        //if (discards.Count==0)
        //{
        //    Debug.Log("카드가 없다 in ReturnAll() of DiscardDeck");
        //}
        
        for (int i = 0; i < discards.Count; ++i)
        {
            deck.Push(discards[i]);
        }

        discards.Clear();
    }
    public void EmptyStackSmooth(float time)
    {
        isShuffle = true;
        totalTime = time;

        fullStackHeight = discardStack.transform.position;
    }
}
