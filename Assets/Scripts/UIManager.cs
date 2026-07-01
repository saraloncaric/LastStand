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

    [Header("HUD stil (opcionalno)")]
    public TMP_FontAsset hudFontOverride;

    const float LeftBtnX = 20f;
    const float SkipBtnY = -150f;

    GameObject _skipButton;
    Image _towerHealthFill;
    RectTransform _towerHealthFillRect;
    TextMeshProUGUI _towerHealthLabel;
    Health _towerHealth;

    TMP_FontAsset _hudFont;

    static readonly Color ValColor = new Color(0.94f, 0.82f, 0.5f, 1f);
    static readonly Color TimerColor = new Color(0.66f, 0.9f, 0.98f, 1f);
    static readonly Color CoinsColor = new Color(1f, 0.88f, 0.53f, 1f);
    static readonly Color TowerTextColor = new Color(0.92f, 0.96f, 0.9f, 1f);
    static readonly Color32 OutlineColor = new Color32(26, 20, 16, 220);

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
        LayoutHud();
        StyleHudTexts();
    }

    TMP_FontAsset GetHudFont()
    {
        if (_hudFont != null)
            return _hudFont;

        if (hudFontOverride != null)
        {
            _hudFont = hudFontOverride;
            return _hudFont;
        }

        _hudFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/Oswald Bold SDF");
        if (_hudFont == null)
            _hudFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/Anton SDF");

        return _hudFont;
    }

    void StyleHudText(TextMeshProUGUI text, Color color, float size, FontStyles style = FontStyles.Bold)
    {
        if (text == null)
            return;

        TMP_FontAsset font = GetHudFont();
        if (font != null)
            text.font = font;

        text.color = color;
        text.fontSize = size;
        text.fontStyle = style;
        text.outlineWidth = 0.22f;
        text.outlineColor = OutlineColor;
        text.characterSpacing = 2f;
    }

    void StyleHudTexts()
    {
        StyleHudText(valText, ValColor, 24f);
        StyleHudText(timerText, TimerColor, 30f);
        StyleHudText(coinsText, CoinsColor, 24f);
        StyleHudText(_towerHealthLabel, TowerTextColor, 17f);
    }

    void LayoutHud() {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        AnchorHud(valText, canvas, new Vector2(0f, 1f), new Vector2(20f, -20f), TextAlignmentOptions.TopLeft);
        AnchorHud(timerText, canvas, new Vector2(0f, 1f), new Vector2(20f, -58f), TextAlignmentOptions.TopLeft);

        if (valText != null) valText.fontSize = 24f;
        if (timerText != null) timerText.fontSize = 30f;
        if (coinsText != null) coinsText.fontSize = 24f;

        if (fazaText != null)
            fazaText.gameObject.SetActive(false);

        AnchorHud(coinsText, canvas, new Vector2(1f, 1f), new Vector2(-20f, -20f), TextAlignmentOptions.TopRight);
    }

    void AnchorHud(TextMeshProUGUI text, Canvas canvas, Vector2 corner, Vector2 offset, TextAlignmentOptions align) {
        if (text == null) return;

        RectTransform rt = text.rectTransform;
        rt.SetParent(canvas.transform, true);
        rt.localScale = Vector3.one;
        rt.anchorMin = corner;
        rt.anchorMax = corner;
        rt.pivot = corner;
        rt.anchoredPosition = offset;
        rt.sizeDelta = new Vector2(320f, 40f);

        text.gameObject.SetActive(true);
        text.alignment = align;
        text.enableAutoSizing = false;
        if (text.fontSize < 18f)
            text.fontSize = 22f;
        text.transform.SetAsLastSibling();
    }

    void Update() {
        if (gameManager != null && gameManager.trenutnafaza == GameManager.GamePhase.GameOver) {
            UpdateTowerHealthBar();
            return;
        }

        if (economy != null && coinsText != null)
        {
            coinsText.text = "◆ " + economy.coins;
            coinsText.color = CoinsColor;
        }

        if (gameManager != null) {
            if (valText != null) {
                if (gameManager.trenutnafaza == GameManager.GamePhase.Priprema) {
                    int sljedeci = gameManager.trenutniVal + 1;
                    valText.text = "PRIPREMA  ·  VAL " + sljedeci;
                }
                else if (gameManager.trenutnafaza == GameManager.GamePhase.Val) {
                    valText.text = "VAL " + gameManager.trenutniVal;
                }
                valText.color = ValColor;
            }

            if (timerText != null) {
                int sekunde = Mathf.CeilToInt(Mathf.Max(0f, gameManager.timer));
                int minute = sekunde / 60;
                int sek = sekunde % 60;
                timerText.text = string.Format("{0:00}:{1:00}", minute, sek);
                timerText.color = TimerColor;
            }

            SetSkipButtonVisible(gameManager.trenutnafaza == GameManager.GamePhase.Priprema);
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
            PositionSkipButton();
            _skipButton.SetActive(false);
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        _skipButton = new GameObject("PreskociVal", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        _skipButton.layer = 5;
        _skipButton.transform.SetParent(canvas.transform, false);

        PositionSkipButton();

        Image skipImage = _skipButton.GetComponent<Image>();
        skipImage.color = new Color(0.32f, 0.28f, 0.24f, 1f);

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
        _skipButton.SetActive(false);
    }

    void PositionSkipButton()
    {
        if (_skipButton == null)
            return;

        RectTransform skipRt = _skipButton.GetComponent<RectTransform>();
        skipRt.anchorMin = new Vector2(0f, 1f);
        skipRt.anchorMax = new Vector2(0f, 1f);
        skipRt.pivot = new Vector2(0f, 1f);
        skipRt.sizeDelta = new Vector2(130f, 46f);
        skipRt.anchoredPosition = new Vector2(LeftBtnX, SkipBtnY);
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
        rootRt.sizeDelta = new Vector2(380f, 50f);
        rootRt.anchoredPosition = new Vector2(0f, -16f);

        GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObj.layer = 5;
        labelObj.transform.SetParent(barRoot.transform, false);
        RectTransform labelRt = labelObj.GetComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0f, 0.5f);
        labelRt.anchorMax = new Vector2(1f, 1f);
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;
        _towerHealthLabel = labelObj.GetComponent<TextMeshProUGUI>();
        _towerHealthLabel.fontSize = 17;
        _towerHealthLabel.alignment = TextAlignmentOptions.Center;
        _towerHealthLabel.color = TowerTextColor;
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

        if (_towerHealthLabel != null) {
            _towerHealthLabel.text = "TORANJ  " + Mathf.CeilToInt(_towerHealth.currentHealth) + " / " + Mathf.CeilToInt(_towerHealth.maxHealth);
            _towerHealthLabel.color = TowerTextColor;
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
