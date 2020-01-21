using UnityEngine;
using System.Collections;

public class Account : MonoBehaviour {
    
    int account; // 전투에 사용할 총알 !!!
    public int AmountOfAccount
    {
        get
        {
            return account;
        }
        set
        {
            account = value;
        }
    }
}
