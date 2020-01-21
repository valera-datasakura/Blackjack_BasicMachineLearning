using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;

public class LobbyController : MonoBehaviour {

    public UIPanel lobbyPanel;
    public UIPanel infoPanel;
    public Transform ruleUI;
    public Transform aboutUI;

    Vector3 firstPosOfRuleUI;
    Vector3 firstPosOfAboutUI;

    //___________________________________________Initialize____________________________________________
    void Awake()
    {
        firstPosOfRuleUI = ruleUI.position;
        firstPosOfAboutUI = aboutUI.position;

        var sound = SoundManager.Instance;
    }

    //____________________________________________Lobby Button 관련______________________________________________
	public void GoToInfo()
    {
        lobbyPanel.gameObject.SetActive(false);
        infoPanel.gameObject.SetActive(true);
        SoundManager.Instance.Play("Effect_Button_General");
    }
    public void GoToLobby()
    {
        lobbyPanel.gameObject.SetActive(true);
        infoPanel.gameObject.SetActive(false);
        SoundManager.Instance.Play("Effect_Button_General");
    }

    public void SceneToPlay()
    {
        SoundManager.Instance.FadeOutBgm();
        SceneManager.LoadScene("SinglePlay");
        //Application.LoadLevel("SinglePlay");
        
        SoundManager.Instance.Play("Effect_Button_General");
    }
    public void SceneToStatistics()
    {
        SoundManager.Instance.FadeOutBgm();
        SoundManager.Instance.Play("Effect_Button_General");
        SceneManager.LoadScene(2);
    }
    public void ExitGame()
    {
        SoundManager.Instance.Play("Effect_Button_General");
        Application.Quit();
    }
    public void ResetKey()
    {
        PlayerPrefs.DeleteAll();
    }

    //______________________________________Info 관련__________________________________________
    public void ShowRule()
    {
        aboutUI.gameObject.SetActive(false);
        ruleUI.gameObject.SetActive(true);

        ruleUI.position = firstPosOfRuleUI;

        SoundManager.Instance.Play("Effect_Button_General");
    }
    public void ShowAbout()
    {
        ruleUI.gameObject.SetActive(false);
        aboutUI.gameObject.SetActive(true);

        aboutUI.position = firstPosOfAboutUI;

        SoundManager.Instance.Play("Effect_Button_General");
    }
}
