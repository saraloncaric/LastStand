using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public UIManager uiManager;

    public enum GamePhase { Priprema, Val, GameOver }
    public GamePhase trenutnafaza;
    public int trenutniVal = 0;
    public float timer = 0f;

    static readonly float FirstPrepDuration = 180f;
    static readonly float PrepDuration = 300f;
    static readonly float[] WaveDurations = { 300f, 420f, 600f };

    public static event Action<int> OnWaveChanged;
    public static event Action OnPreparePhase;

    void Awake() {
        Instance = this;
    }

    void Start() {
        trenutnafaza = GamePhase.Priprema;
        timer = FirstPrepDuration;
        Debug.Log("Priprema počinje!");
    }

    void Update() {
        if (trenutnafaza == GamePhase.GameOver)
            return;

        if (trenutnafaza == GamePhase.Priprema) {
            timer -= Time.deltaTime;
            if (timer <= 0f)
                PocniVal();
            return;
        }

        if (trenutnafaza == GamePhase.Val) {
            timer -= Time.deltaTime;
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

        if (trenutniVal >= 3) {
            TriggerGameOver();
            return;
        }

        trenutnafaza = GamePhase.Priprema;
        timer = PrepDuration;
        OnPreparePhase?.Invoke();
        Debug.Log("Priprema nakon vala " + trenutniVal);
    }

    public void TriggerGameOver() {
        if (trenutnafaza == GamePhase.GameOver) return;
        trenutnafaza = GamePhase.GameOver;
        Debug.Log("Game Over!");
        uiManager.PrikaziGameOver();
    }

    public void PreskociFazu() {
        if (trenutnafaza == GamePhase.GameOver)
            return;
        if (trenutnafaza == GamePhase.Val)
            return;

        PocniVal();
    }
}
