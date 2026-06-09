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

    public static event Action<int> OnWaveChanged;
    public static event Action OnPreparePhase;

    void Awake() {
        Instance = this;
    }

    void Start() {
        trenutnafaza = GamePhase.Priprema;
        timer = 180f; 
        Debug.Log("Priprema počinje!");
    }

    void Update() {
        if (trenutnafaza == GamePhase.GameOver) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
            SljedecaFaza();
    }

    void SljedecaFaza() {
        if (trenutnafaza == GamePhase.Priprema) {
            trenutniVal++;
            trenutnafaza = GamePhase.Val;

            OnWaveChanged?.Invoke(trenutniVal);

            if (trenutniVal == 1) timer = 300f;      
            else if (trenutniVal == 2) timer = 420f; 
            else if (trenutniVal == 3) timer = 600f; 
        }
        else if (trenutnafaza == GamePhase.Val) {
            if (trenutniVal == 3) {
                TriggerGameOver(); 
                return;
            }

            trenutnafaza = GamePhase.Priprema;

            OnPreparePhase?.Invoke();

            timer = 300f; 
        }
    }

    public void TriggerGameOver() {
        if (trenutnafaza == GamePhase.GameOver) return;
        trenutnafaza = GamePhase.GameOver;
        Debug.Log("Game Over!");
        uiManager.PrikaziGameOver();
    }

    public void PreskociFazu() {
        if (trenutnafaza == GamePhase.GameOver) return;
        SljedecaFaza();
    }
}
