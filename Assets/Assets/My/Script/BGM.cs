using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class BGM : MonoBehaviour {

    public string[] bgmsName;

    AudioSource audio;
    AudioClip[] clips;

    //_______________________________callback__________________________
	void Start () {

        audio = GetComponent<AudioSource>();

        clips = new AudioClip[bgmsName.Length];
        for(int i=0;i< bgmsName.Length; ++i)
        {
            clips[i] = SoundManager.Instance[bgmsName[i]];
        }
    }
    void OnDisable()
    {
    }
	
	void Update () {

        if (audio.isPlaying)
        {
            return;
        }
	}
    //_______________________________클립 관련_____________________________
    AudioClip GetRandClip()
    {
        int rand = Random.Range(0, clips.Length);
        return clips[rand];
    }
}
