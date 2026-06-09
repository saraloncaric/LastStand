using UnityEngine;
using TMPro;
using UnityEngine.UI;
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

    GameObject _skipButton;
    Image _towerHealthFill;
    RectTransform _towerHealthFillRect;
    TextMeshProUGUI _towerHealthLabel;
    Health _towerHealth;

    void Start() {
        if (economy == null)
            economy = EconomyManager.Instance;
        if (gameManager == null)
            gameManager = GameManager.Instance;
        if (coinsText == null)
            coinsText = GameObject.Find("CoinsText")?.GetComponent<TextMeshProUGUI>();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (upozorenjeText != null)
            upozorenjeText.gameObject.SetActive(false);

        SetupTowerHealth();
        CreateSkipButton();
        CreateTowerHealthBar();
    }

    void Update() {
        if (gameManager != null && gameManager.trenutnafaza == GameManager.GamePhase.GameOver) {
            UpdateTowerHealthBar();
            return;
        }

        if (economy != null && coinsText != null)
            coinsText.text = "Coins: " + economy.coins;

        if (gameManager != null) {
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

        UpdateTowerHealthBar();
    }

    void SetupTowerHealth() {
        GameObject goal = GameObject.FindGameObjectWithTag("Goal");
        if (goal != null)
            _towerHealth = goal.GetComponent<Health>();
    }

    public void PreskociFazu() {
        if (gameManager == null) return;
        gameManager.PreskociFazu();
    }

    public void SetSkipButtonVisible(bool visible) {
        if (_skipButton != null)
            _skipButton.SetActive(visible);
    }

    void CreateSkipButton() {
        if (GameObject.Find("PreskociVal") != null)
        {
            _skipButton = GameObject.Find("PreskociVal");
            _skipButton.SetActive(true);
            return;
        }

        Button meniBtn = GameObject.Find("Meni")?.GetComponent<Button>();
        if (meniBtn == null) return;

        RectTransform meniRt = meniBtn.GetComponent<RectTransform>();
        Transform canvas = meniRt.parent;
        Sprite buttonSprite = meniBtn.GetComponent<Image>()?.sprite;

        _skipButton = new GameObject("PreskociVal", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        _skipButton.layer = 5;
        _skipButton.transform.SetParent(canvas, false);

        RectTransform skipRt = _skipButton.GetComponent<RectTransform>();
        skipRt.anchorMin = meniRt.anchorMin;
        skipRt.anchorMax = meniRt.anchorMax;
        skipRt.pivot = meniRt.pivot;
        skipRt.sizeDelta = new Vector2(110f, 30f);
        skipRt.anchoredPosition = meniRt.anchoredPosition + new Vector2(130f, 0f);

        Image skipImage = _skipButton.GetComponent<Image>();
        if (buttonSprite != null)
            skipImage.sprite = buttonSprite;
        skipImage.type = Image.Type.Sliced;

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.layer = 5;
        textObj.transform.SetParent(_skipButton.transform, false);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = "Skip";
        tmp.fontSize = 14;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        Button skipBtn = _skipButton.GetComponent<Button>();
        skipBtn.targetGraphic = skipImage;
        skipBtn.onClick.AddListener(PreskociFazu);
        _skipButton.SetActive(true);
    }

    void CreateTowerHealthBar() {
        if (GameObject.Find("TowerHealthBar") != null)
            return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject barRoot = new GameObject("TowerHealthBar", typeof(RectTransform));
        barRoot.layer = 5;
        barRoot.transform.SetParent(canvas.transform, false);

        RectTransform rootRt = barRoot.GetComponent<RectTransform>();
        rootRt.anchorMin = new Vector2(0.5f, 1f);
        rootRt.anchorMax = new Vector2(0.5f, 1f);
        rootRt.pivot = new Vector2(0.5f, 1f);
        rootRt.sizeDelta = new Vector2(260f, 36f);
        rootRt.anchoredPosition = new Vector2(0f, -20f);

        GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObj.layer = 5;
        labelObj.transform.SetParent(barRoot.transform, false);
        RectTransform labelRt = labelObj.GetComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0f, 0.5f);
        labelRt.anchorMax = new Vector2(1f, 1f);
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;
        _towerHealthLabel = labelObj.GetComponent<TextMeshProUGUI>();
        _towerHealthLabel.fontSize = 14;
        _towerHealthLabel.alignment = TextAlignmentOptions.Center;
        _towerHealthLabel.color = Color.white;
        _towerHealthLabel.text = "Toranj";

        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bgObj.layer = 5;
        bgObj.transform.SetParent(barRoot.transform, false);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 0f);
        bgRt.anchorMax = new Vector2(1f, 0.45f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bgImage = bgObj.GetComponent<Image>();
        bgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);

        GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fillObj.layer = 5;
        fillObj.transform.SetParent(bgObj.transform, false);
        RectTransform fillRt = fillObj.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = new Vector2(2f, 2f);
        fillRt.offsetMax = new Vector2(-2f, -2f);
        _towerHealthFill = fillObj.GetComponent<Image>();
        _towerHealthFillRect = fillRt;
        _towerHealthFill.color = new Color(0.2f, 0.85f, 0.25f, 1f);
    }

    void UpdateTowerHealthBar() {
        if (_towerHealth == null)
            SetupTowerHealth();
        if (_towerHealthFillRect == null || _towerHealth == null)
            return;

        float pct = _towerHealth.maxHealth > 0f
            ? Mathf.Clamp01(_towerHealth.currentHealth / _towerHealth.maxHealth)
            : 0f;
        _towerHealthFillRect.anchorMax = new Vector2(pct, 1f);

        if (_towerHealthFill != null) {
            if (pct > 0.5f)
                _towerHealthFill.color = new Color(0.2f, 0.85f, 0.25f, 1f);
            else if (pct > 0.25f)
                _towerHealthFill.color = new Color(0.95f, 0.75f, 0.1f, 1f);
            else
                _towerHealthFill.color = new Color(0.9f, 0.2f, 0.15f, 1f);
        }

        if (_towerHealthLabel != null)
            _towerHealthLabel.text = "Toranj: " + Mathf.CeilToInt(_towerHealth.currentHealth) + " / " + Mathf.CeilToInt(_towerHealth.maxHealth);
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
