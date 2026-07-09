using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class OutroController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject uiPanel;
    public string mainMenuSceneName = "IntroScene";
    public string gameSceneName = "SampleScene";

    void Start()
    {
        uiPanel.SetActive(false);
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.Play();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        uiPanel.SetActive(true);
    }

    public void PonovoIgraj()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void GlavniIzbornik()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}