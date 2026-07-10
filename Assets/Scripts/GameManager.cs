using System;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public UIManager uiManager;
    public WaveLighting waveLighting;

    public enum GamePhase { Priprema, Val, GameOver, Pobjeda }
    public GamePhase trenutnafaza;
    public int trenutniVal = 0;
    public float timer = 0f;

    static readonly float FirstPrepDuration = 180f;
    static readonly float PrepDuration = 240f;
    static readonly float[] WaveDurations = { 180f, 240f, 300f, 360f, 420f };

    public static event Action<int> OnWaveChanged;
    public static event Action OnPreparePhase;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start() {
        trenutnafaza = GamePhase.Priprema;
        timer = FirstPrepDuration;
        Debug.Log("Priprema počinje!");
    }

    void Update() {
        if (trenutnafaza == GamePhase.GameOver || trenutnafaza == GamePhase.Pobjeda)
            return;

        if (trenutnafaza == GamePhase.Priprema) {
            timer -= Time.unscaledDeltaTime;
            if (timer <= 0f)
                PocniVal();
            return;
        }

        if (trenutnafaza == GamePhase.Val) {
            timer -= Time.unscaledDeltaTime;
            if (timer <= 0f)
                ZavrsiVal();
        }
    }

    void PocniVal() {
        if (trenutnafaza != GamePhase.Priprema)
            return;

        trenutniVal++;
        trenutnafaza = GamePhase.Val;
        timer = GetWaveDuration(trenutniVal);

        OnWaveChanged?.Invoke(trenutniVal);
        Debug.Log("Val " + trenutniVal + " počinje!");

        if (waveLighting != null)
            waveLighting.SetWave(trenutniVal);
    }

    static float GetWaveDuration(int wave)
    {
        if (wave < 1 || wave > WaveDurations.Length)
            return WaveDurations[0];
        return WaveDurations[wave - 1];
    }

    public void ZavrsiVal() {
        if (trenutnafaza != GamePhase.Val)
            return;

        if (trenutniVal >= 5) {
            TriggerPobjeda();
            return;
        }

        trenutnafaza = GamePhase.Priprema;
        timer = PrepDuration;
        OnPreparePhase?.Invoke();
        Debug.Log("Priprema nakon vala " + trenutniVal);
    }

    public void TriggerGameOver() {
        if (trenutnafaza == GamePhase.GameOver || trenutnafaza == GamePhase.Pobjeda)
            return;
        trenutnafaza = GamePhase.GameOver;
        Debug.Log("Game Over!");
        uiManager.PrikaziGameOver();
    }

    public void TriggerPobjeda() {
        if (trenutnafaza == GamePhase.GameOver || trenutnafaza == GamePhase.Pobjeda)
            return;
        trenutnafaza = GamePhase.Pobjeda;
        Debug.Log("Pobjeda!");
        SceneManager.LoadScene("OutroScene");
    }

    public void PreskociFazu() {
        if (trenutnafaza == GamePhase.GameOver || trenutnafaza == GamePhase.Pobjeda)
            return;
        if (trenutnafaza == GamePhase.Val)
            return;

        PocniVal();
    }
}
