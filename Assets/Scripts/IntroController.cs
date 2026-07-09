using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class IntroController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject uiPanel; 
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

    public void KreniGumb()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}