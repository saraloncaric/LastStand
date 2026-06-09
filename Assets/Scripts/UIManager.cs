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
    public TextMeshProUGUI upozorenjeText; 

    public GameObject gameOverPanel;

    void Start() {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (upozorenjeText != null)
            upozorenjeText.gameObject.SetActive(false); 
    }

    void Update() {
        if (gameManager.trenutnafaza == GameManager.GamePhase.GameOver) return;

        if (coinsText != null)
            coinsText.text = "Coins: " + economy.coins;

        if (fazaText != null)
            fazaText.text = "Faza: " + gameManager.trenutnafaza.ToString();

        if (valText != null) {
            if (gameManager.trenutniVal > 0)
                valText.text = "Val: " + gameManager.trenutniVal;
            else
                valText.text = "Val: -";
        }

        if (timerText != null) {
            int sekunde = Mathf.CeilToInt(gameManager.timer);
            int minute = sekunde / 60;
            int sek = sekunde % 60;
            timerText.text = string.Format("{0:00}:{1:00}", minute, sek);
        }
    }

    public void PrikaziUpozorenje() { 
        if (upozorenjeText != null)
            StartCoroutine(PrikaziUpozorenjeRoutine());
    }

    System.Collections.IEnumerator PrikaziUpozorenjeRoutine() { 
        upozorenjeText.text = "⚠ Neprijatelji se približavaju!";
        upozorenjeText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        upozorenjeText.gameObject.SetActive(false);
    }

    public void PrikaziGameOver() {
        gameOverPanel.SetActive(true);
    }

    public void Restart() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}