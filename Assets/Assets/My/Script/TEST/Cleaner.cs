using UnityEngine;
using System.Collections;

public class Cleaner : MonoBehaviour {
    
	void Start () {
        
        EventDelegate.Set(GetComponent<UIButton>().onClick, CleanRemainPrefs);
    }

    public void CleanRemainPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}
