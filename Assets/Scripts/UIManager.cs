using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public EconomyManager economy;
    public GameManager gameManager;

    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI fazaText;
    public TextMeshProUGUI valText;
    public TextMeshProUGUI timerText;

    public GameObject gameOverPanel;

    void Start() {
        gameOverPanel.SetActive(false);
    }

    void Update() {
        if (gameManager.trenutnafaza == GameManager.GamePhase.GameOver) {
            return;
        }

        coinsText.text = "Coins: " + economy.coins;
        fazaText.text = "Faza: " + gameManager.trenutnafaza.ToString();

        if (gameManager.trenutniVal > 0) {
            valText.text = "Val: " + gameManager.trenutniVal;
        }
        else {
            valText.text = "Val: -";
        }

        int sekunde = Mathf.CeilToInt(gameManager.timer);
        int minute = sekunde / 60;
        int sek = sekunde % 60;
        timerText.text = string.Format("{0:00}:{1:00}", minute, sek);
    }

    public void PrikaziGameOver() {
        gameOverPanel.SetActive(true);
    }

    public void Restart() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}