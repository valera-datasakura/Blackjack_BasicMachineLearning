using UnityEngine;
using System.Collections;

public class OrderTest : MonoBehaviour {



	// Use this for initialization
	void Awake () {

        Debug.Log("누가 먼저 실행됨? awake of "+name);
	}
    void Start()
    {   
        Debug.Log("누가 먼저 실행됨? start of"+ name);
    }
}
