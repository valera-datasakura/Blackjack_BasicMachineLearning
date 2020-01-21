using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour {

    static SoundManager sInstance;
    public static SoundManager Instance
    {
        get
        {
            if (sInstance == null)
            {
                GameObject newObj = new GameObject("_SoundManager");
                sInstance = newObj.AddComponent<SoundManager>();
            }
            return sInstance;
        }
    }

    AudioSource audioSource;
    List<AudioSource> listAudioSources = new List<AudioSource>();
    
    Dictionary<string, AudioClip> dicClips = new Dictionary<string, AudioClip>();
    
    //_________________랜덤 Bgm__________________
    string[] arrLobbys = new string[]
    {
        "Bgm_Lobby1",
        "Bgm_Lobby2",
        "Bgm_Lobby3",
        "Bgm_Lobby4",
        "Bgm_Lobby5"
    };
    string[] arrPlays = new string[]
    {
        "Bgm_Play1"
    };
    string[] arrStatistics = new string[]
    {
        "Bgm_Statistics1",
        "Bgm_Statistics2",
        "Bgm_Statistics3",
        "Bgm_Statistics4",
        "Bgm_Statistics5"
    };
    //________________랜덤 Effect_______________
    string[] bettings = new string[]
    {
        "Betting1",
        "Betting2",
        "Betting1",
        "Betting2"
    };
    string[] insurance = new string[]
    {
        "Insurance1",
        "Insurance2",
        "Insurance3"
    };
    string[] p_Blackjack = new string[]
    {
        "PlayerBlackjack1",
        "PlayerBlackjack2"
    };
    string[] d_Blackjack = new string[]
    {
         "DealerBlackjack1",
         "DealerBlackjack2"
    };
    string[] doubleDown = new string[]
    {
        "DoubleDown1",
        "DoubleDown2",
        "DoubleDown3",
        "DoubleDown4"
    };
    string[] p_Burst = new string[]
    {
        "PlayerBurst1",
        "PlayerBurst2",
        "PlayerBurst3",
        "PlayerBurst4"
    };
    string[] d_Burst = new string[]
    {
        "DealerBurst1",
        "DealerBurst2",
        "DealerBurst3",
        "DealerBurst4",
        "DealerBurst5",
        "DealerBurst6"
    };
    string[] win = new string[]
    {
        "Win1",
        "Win2",
        "Win3",
        "Win4"
    };
    string[] lose = new string[]
    {
        "Lose1",
        "Lose2",
        "Lose3",
        "Lose4"
    };
    string[] push = new string[]
    {
        "Push1"
    };

    //______________________________Callback_______________________________
    void Awake ()
    {
        DontDestroyOnLoad(gameObject);
        
        audioSource = GetComponent<AudioSource>();
        AudioClip[] arrSound=
            Resources.LoadAll<AudioClip>("Sound");
        for(int i = 0; i < arrSound.Length; ++i)
        {
            dicClips.Add(arrSound[i].name, arrSound[i]);
        }
    }
    void Update()
    {
        if (audioSource.isPlaying)
        {
            return;
        }

        

        int rand;
        AudioClip newClip;
        while (true)
        {
            bool isDone = false;
            switch (SceneManager.GetActiveScene().name)
            {
                case "Lobby":
                    rand = Random.Range(0, arrLobbys.Length);
                    newClip = dicClips[arrLobbys[rand]];
                    if (audioSource.clip != newClip)
                    {
                        audioSource.clip = newClip;
                        isDone = true;
                    }
                    break;
                case "SinglePlay":
                case "AutoPlay":
                    rand = Random.Range(0, arrPlays.Length);
                    newClip = dicClips[arrPlays[rand]];
                    if (audioSource.clip != newClip)
                    {
                        audioSource.clip = newClip;
                        isDone = true;
                    }
                    else if(arrPlays.Length==1)
                    {
                        isDone = true;
                    }
                    break;
                case "Statistics":
                    rand = Random.Range(0, arrStatistics.Length);
                    newClip = dicClips[arrStatistics[rand]];
                    if (audioSource.clip != newClip)
                    {
                        audioSource.clip = newClip;
                        isDone = true;
                    }
                    break;
                default:
                    isDone = true;
                    break;
            }
            if (isDone)
            {
                break;
            }
        }

        audioSource.volume = 0.075f;
        audioSource.Play();
    }
    //______________________________Access_____________________________
    public AudioClip this[string key]
    {
        get
        {
            return dicClips[key];
        }
    }
    
    //____________________________Effect 관련______________________________________
    public float Betting()
    {
        int rand = Random.Range(0, bettings.Length+3);
        if(rand < bettings.Length)
        {
            return Play(bettings[rand]);
        }
        else
        {
            return 0f;
        }
    }
    public float P_Blackjack()
    {
        int rand = Random.Range(0, p_Blackjack.Length);

        return Play(p_Blackjack[rand]);
    }
    public float D_Blackjack()
    {
        int rand = Random.Range(0, d_Blackjack.Length);
        
        return Play(d_Blackjack[rand]);
    }
    public float Insurance()
    {
        int rand = Random.Range(0, insurance.Length + 2);
        if (rand < insurance.Length)
        {
            return Play(insurance[rand]);
        }
        else
        {
            return 0f;
        }
    }
    public float DoubleDown()
    {
        int rand = Random.Range(0, doubleDown.Length + 2);
        if (rand < doubleDown.Length)
        {
            return Play(doubleDown[rand]);
        }
        else
        {
            return 0f;
        }
    }
    public float P_Burst()
    {
        int rand = Random.Range(0, p_Burst.Length + 2);
        if (rand < p_Burst.Length)
        {
            return Play(p_Burst[rand]);
        }
        else
        {
            return 0f;
        }
    }
    public float D_Burst()
    {
        int rand = Random.Range(0, d_Burst.Length + 2);
        if (rand < d_Burst.Length)
        {
            return Play(d_Burst[rand]);
        }
        else
        {
            return 0f;
        }
    }
    public float Win()
    {
        int rand = Random.Range(0, win.Length + 2);
        if (rand < win.Length)
        {
            return Play(win[rand]);
        }
        else
        {
            return 0f;
        }
    }
    public float Lose()
    {
        int rand = Random.Range(0, lose.Length + 2);
        if (rand < lose.Length)
        {
            return Play(lose[rand]);
        }
        else
        {
            return 0f;
        }
    }
    public float Push()
    {
        int rand = Random.Range(0, push.Length + 1);
        if (rand < push.Length)
        {
            return Play(push[rand]);
        }
        else
        {
            return 0f;
        }
    }

    public float Play(string key)
    {
        AudioClip clip = dicClips[key];
        for(int i = 0; i < listAudioSources.Count; ++i)
        {
            if (listAudioSources[i].isPlaying == false)
            {
                listAudioSources[i].clip = clip;
                listAudioSources[i].Play();
                return clip.length;
            }
        }
        
        AudioSource auSource = gameObject.AddComponent<AudioSource>();
        auSource.clip = clip;
        auSource.Play();
        listAudioSources.Add(auSource);
        return clip.length;
    }

    //__________________________Bgm 관련_________________________________________
    public void FadeOutBgm()
    {
        StartCoroutine(CoFadeOutBgm());
    }
    IEnumerator CoFadeOutBgm()
    {
        while (true)
        {
            yield return null;

            audioSource.volume -= 0.05f;

            if (audioSource.volume < 0.075f)
            {
                audioSource.Stop();
                break;
            }
        }
    }
}
