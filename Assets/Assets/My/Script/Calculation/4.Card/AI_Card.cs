using UnityEngine;
using System.Collections;

public class AI_Card : MonoBehaviour {

    public int number;

    public int cardCountingScore=0;

    public void Init(int num)
    {
        number = num;

        if (2 <= number && number <= 6)
        {
            cardCountingScore = 1;
        }
        else if (number == 10 || number == 1)
        {
            cardCountingScore = -1; 
        }
    }
}
