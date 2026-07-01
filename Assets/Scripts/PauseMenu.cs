using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    enum Screen { None, Pause, SettingsHub, Audio, Controls }

    enum ConfirmAction { None, Restart, ExitMenu, ExitDesktop }

    [Header("Scene")]
    public string menuSceneName = "";

    bool _paused;
    Screen _screen = Screen.None;

    GameObject _rootOverlay;
    RectTransform _pauseBox;
    RectTransform _settingsHubBox;
    RectTransform _audioBox;
    RectTransform _controlsBox;
    RectTransform _controlsScrollContent;
    RectTransform _confirmBox;
    ScrollRect _controlsScroll;

    ConfirmAction _pendingConfirm;
    TextMeshProUGUI _confirmMessage;

    Slider _musicSlider;
    Slider _sfxSlider;
    Slider _mouseSlider;
    Slider _moveSpeedSlider;
    TextMeshProUGUI _musicLabel;
    TextMeshProUGUI _sfxLabel;
    TextMeshProUGUI _mouseLabel;
    TextMeshProUGUI _moveSpeedLabel;

    string _rebindTarget;
    TextMeshProUGUI _rebindStatus;
    readonly Dictionary<string, TextMeshProUGUI> _keyLabels = new Dictionary<string, TextMeshProUGUI>();
    static bool _sceneHookRegistered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap() => EnsureForActiveScene();

    static void EnsureForActiveScene()
    {
        if (!_sceneHookRegistered)
        {
            _sceneHookRegistered = true;
            SceneManager.sceneLoaded += (_, __) => EnsureForActiveScene();
        }

        EnsureSettingsManagers();

        PauseMenu[] all = Object.FindObjectsByType<PauseMenu>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        PauseMenu keeper = null;
        Scene active = SceneManager.GetActiveScene();

        foreach (PauseMenu menu in all)
        {
            if (menu.gameObject.scene == active)
            {
                keeper = menu;
                break;
            }
        }

        foreach (PauseMenu menu in all)
        {
            if (menu != keeper)
                Object.Destroy(menu.gameObject);
        }

        if (keeper == null)
            new GameObject("PauseMenu (auto)").AddComponent<PauseMenu>();
    }

    static void EnsureSettingsManagers()
    {
        if (FindFirstObjectByType<AudioSettingsManager>() == null)
            new GameObject("AudioSettings (auto)").AddComponent<AudioSettingsManager>();

        if (FindFirstObjectByType<CameraSettingsManager>() == null)
            new GameObject("CameraSettings (auto)").AddComponent<CameraSettingsManager>();

        if (FindFirstObjectByType<ControlSettingsManager>() == null)
            new GameObject("ControlSettings (auto)").AddComponent<ControlSettingsManager>();
    }

    void Start() => Rebuild();

    void Rebuild()
    {
        CleanupUi();
        BuildUI();
        HideAll();
        _paused = false;
        Time.timeScale = 1f;
        IsPaused = false;
    }

    void CleanupUi()
    {
        _keyLabels.Clear();
        _rebindTarget = null;
        _pendingConfirm = ConfirmAction.None;

        if (_rootOverlay != null)
        {
            Destroy(_rootOverlay);
            _rootOverlay = null;
        }

        _pauseBox = null;
        _settingsHubBox = null;
        _audioBox = null;
        _controlsBox = null;
        _controlsScrollContent = null;
        _controlsScroll = null;
        _confirmBox = null;
        _confirmMessage = null;
        _musicSlider = null;
        _sfxSlider = null;
        _mouseSlider = null;
        _moveSpeedSlider = null;
        _musicLabel = null;
        _sfxLabel = null;
        _mouseLabel = null;
        _moveSpeedLabel = null;
        _rebindStatus = null;
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        if (!string.IsNullOrEmpty(_rebindTarget))
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                _rebindTarget = null;
                if (_rebindStatus != null)
                    _rebindStatus.text = "";
                return;
            }

            foreach (Key key in System.Enum.GetValues(typeof(Key)))
            {
                if (key == Key.None)
                    continue;
                if (!Keyboard.current[key].wasPressedThisFrame)
                    continue;
                if (key == Key.Escape)
                    continue;

                if (ControlSettingsManager.Instance != null)
                    ControlSettingsManager.Instance.SetKey(_rebindTarget, key);
                RefreshKeyLabels();
                _rebindTarget = null;
                if (_rebindStatus != null)
                    _rebindStatus.text = "";
                return;
            }
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            HandleEscape();
    }

    void HandleEscape()
    {
        if (_confirmBox != null && _confirmBox.gameObject.activeSelf)
        {
            HideConfirm();
            return;
        }

        if (!_paused)
        {
            Pause();
            return;
        }

        switch (_screen)
        {
            case Screen.Audio:
            case Screen.Controls:
                ShowSettingsHub();
                break;
            case Screen.SettingsHub:
                ShowPause();
                break;
            case Screen.Pause:
                Resume();
                break;
        }
    }

    public void Toggle()
    {
        if (_paused) Resume();
        else Pause();
    }

    public void Pause()
    {
        _paused = true;
        _rebindTarget = null;
        ShowPause();
        Time.timeScale = 0f;
        IsPaused = true;
    }

    public void Resume()
    {
        _paused = false;
        _rebindTarget = null;
        HideAll();
        Time.timeScale = 1f;
        IsPaused = false;
    }

    void ShowPause()
    {
        _screen = Screen.Pause;
        HideConfirm();
        ShowOverlay();
        SetBoxActive(_pauseBox, true);
        SetBoxActive(_settingsHubBox, false);
        SetBoxActive(_audioBox, false);
        SetBoxActive(_controlsBox, false);
    }

    public void OpenSettings()
    {
        ShowSettingsHub();
    }

    void ShowSettingsHub()
    {
        _screen = Screen.SettingsHub;
        _rebindTarget = null;
        ShowOverlay();
        SetBoxActive(_pauseBox, false);
        SetBoxActive(_settingsHubBox, true);
        SetBoxActive(_audioBox, false);
        SetBoxActive(_controlsBox, false);
    }

    void ShowAudio()
    {
        SyncAudioSliders();
        _screen = Screen.Audio;
        ShowOverlay();
        SetBoxActive(_pauseBox, false);
        SetBoxActive(_settingsHubBox, false);
        SetBoxActive(_audioBox, true);
        SetBoxActive(_controlsBox, false);
        ResizeBox(_audioBox, new Vector2(480f, 360f));
    }

    void ShowControls()
    {
        SyncControlSliders();
        RefreshKeyLabels();
        _screen = Screen.Controls;
        ShowOverlay();
        SetBoxActive(_pauseBox, false);
        SetBoxActive(_settingsHubBox, false);
        SetBoxActive(_audioBox, false);
        SetBoxActive(_controlsBox, true);
        ResizeBox(_controlsBox, new Vector2(520f, 620f));
        UiScrollHelper.Refresh(_controlsScroll);
    }

    void ShowOverlay()
    {
        if (_rootOverlay != null)
        {
            _rootOverlay.SetActive(true);
            _rootOverlay.transform.SetAsLastSibling();
        }
    }

    void HideAll()
    {
        _screen = Screen.None;
        HideConfirm();
        if (_rootOverlay != null)
            _rootOverlay.SetActive(false);
    }

    void ShowConfirm(ConfirmAction action, string message)
    {
        _pendingConfirm = action;
        if (_confirmBox == null)
            return;

        TextMeshProUGUI msg = _confirmMessage;
        if (msg != null)
            msg.text = message;

        _confirmBox.gameObject.SetActive(true);
        _confirmBox.SetAsLastSibling();
    }

    void HideConfirm()
    {
        _pendingConfirm = ConfirmAction.None;
        if (_confirmBox != null)
            _confirmBox.gameObject.SetActive(false);
    }

    void ConfirmYes()
    {
        ConfirmAction action = _pendingConfirm;
        HideConfirm();

        switch (action)
        {
            case ConfirmAction.Restart:
                DoRestart();
                break;
            case ConfirmAction.ExitMenu:
                DoExitToMenu();
                break;
            case ConfirmAction.ExitDesktop:
                DoExitToDesktop();
                break;
        }
    }

    void RequestRestart()
    {
        ShowConfirm(ConfirmAction.Restart,
            "Jesi li siguran da želiš ponovno pokrenuti igru?\nSav napredak će biti izgubljen jer se igra ne sprema automatski.");
    }

    void RequestExitToMenu()
    {
        ShowConfirm(ConfirmAction.ExitMenu,
            "Jesi li siguran da želiš izaći u izbornik?\nNapredak se ne sprema automatski i može biti izgubljen.");
    }

    void RequestExitToDesktop()
    {
        ShowConfirm(ConfirmAction.ExitDesktop,
            "Jesi li siguran da želiš izaći iz igre?\nNespremljeni napredak bit će izgubljen.");
    }

    void DoRestart()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void DoExitToMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        if (!string.IsNullOrEmpty(menuSceneName))
            SceneManager.LoadScene(menuSceneName);
        else
            Debug.LogWarning("PauseMenu: 'menuSceneName' nije postavljen.");
    }

    void DoExitToDesktop()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    static void SetBoxActive(RectTransform box, bool active)
    {
        if (box != null)
            box.gameObject.SetActive(active);
    }

    static void ResizeBox(RectTransform box, Vector2 size)
    {
        if (box != null)
            box.sizeDelta = size;
    }

    public void ExitToMenu() => RequestExitToMenu();

    public void ExitToDesktop() => RequestExitToDesktop();

    void BeginRebind(string id)
    {
        _rebindTarget = id;
        if (_rebindStatus != null)
            _rebindStatus.text = "Pritisni novu tipku... (Esc = odustani)";
    }

    void RefreshKeyLabels()
    {
        var c = ControlSettingsManager.Instance;
        if (c == null)
            return;

        foreach (var pair in _keyLabels)
        {
            if (pair.Value != null)
                pair.Value.text = c.GetKeyLabel(pair.Key);
        }
    }

    void SyncAudioSliders()
    {
        var s = AudioSettingsManager.Instance;
        if (s == null)
            return;

        if (_musicSlider != null) { _musicSlider.SetValueWithoutNotify(s.musicVolume); UpdateMusicLabel(s.musicVolume); }
        if (_sfxSlider != null) { _sfxSlider.SetValueWithoutNotify(s.sfxVolume); UpdateSfxLabel(s.sfxVolume); }
    }

    void SyncControlSliders()
    {
        var c = CameraSettingsManager.Instance;
        if (c == null)
            return;

        if (_mouseSlider != null)
        {
            _mouseSlider.SetValueWithoutNotify(c.mouseSensitivityPercent);
            UpdateMouseLabel(c.mouseSensitivityPercent);
        }
        if (_moveSpeedSlider != null)
        {
            _moveSpeedSlider.SetValueWithoutNotify(c.moveSpeedPercent);
            UpdateMoveSpeedLabel(c.moveSpeedPercent);
        }
    }

    void OnMusicChanged(float v)
    {
        if (AudioSettingsManager.Instance != null)
            AudioSettingsManager.Instance.SetMusicVolume(v);
        UpdateMusicLabel(v);
    }

    void OnSfxChanged(float v)
    {
        if (AudioSettingsManager.Instance != null)
            AudioSettingsManager.Instance.SetSfxVolume(v);
        UpdateSfxLabel(v);
    }

    void OnMouseSensitivityChanged(float v)
    {
        if (CameraSettingsManager.Instance != null)
            CameraSettingsManager.Instance.SetMouseSensitivityPercent(v);
        UpdateMouseLabel(v);
    }

    void OnMoveSpeedChanged(float v)
    {
        if (CameraSettingsManager.Instance != null)
            CameraSettingsManager.Instance.SetMoveSpeedPercent(v);
        UpdateMoveSpeedLabel(v);
    }

    void UpdateMusicLabel(float v)
    {
        if (_musicLabel != null) _musicLabel.text = "Glazba: " + Mathf.RoundToInt(v * 100f) + "%";
    }

    void UpdateSfxLabel(float v)
    {
        if (_sfxLabel != null) _sfxLabel.text = "Zvukovi: " + Mathf.RoundToInt(v * 100f) + "%";
    }

    void UpdateMouseLabel(float v)
    {
        if (_mouseLabel != null)
            _mouseLabel.text = "Osjetljivost miša: " + Mathf.RoundToInt(v) + "%";
    }

    void UpdateMoveSpeedLabel(float v)
    {
        if (_moveSpeedLabel != null)
            _moveSpeedLabel.text = "Brzina kamere: " + Mathf.RoundToInt(v) + "%";
    }

    void BuildUI()
    {
        Canvas canvas = GetOrCreatePauseCanvas();
        EnsureEventSystem();

        _rootOverlay = CreateOverlay("PauseRoot", canvas.transform);

        _pauseBox = CreateBox(_rootOverlay.transform, new Vector2(440f, 480f), "PAUZA");
        RectTransform pauseButtons = CreateButtonStack(_pauseBox, 58f, 28f);
        CreateStackButton(pauseButtons, "Natrag u igru", Resume);
        CreateStackButton(pauseButtons, "Postavke", OpenSettings);
        CreateStackButton(pauseButtons, "Ponovi igru", RequestRestart);
        if (!string.IsNullOrEmpty(menuSceneName))
            CreateStackButton(pauseButtons, "Izlaz u izbornik", RequestExitToMenu);
        CreateStackButton(pauseButtons, "Izlaz na desktop", RequestExitToDesktop);

        _settingsHubBox = CreateBox(_rootOverlay.transform, new Vector2(440f, 340f), "POSTAVKE");
        RectTransform settingsButtons = CreateButtonStack(_settingsHubBox, 58f, 28f);
        CreateStackButton(settingsButtons, "Audio", ShowAudio);
        CreateStackButton(settingsButtons, "Controls", ShowControls);
        CreateStackButton(settingsButtons, "Natrag", ShowPause);

        _audioBox = CreateBox(_rootOverlay.transform, new Vector2(480f, 360f), "AUDIO");
        _audioBox.gameObject.SetActive(false);

        var audio = AudioSettingsManager.Instance;
        float music = audio != null ? audio.musicVolume : 0.5f;
        float sfx = audio != null ? audio.sfxVolume : 0.7f;

        _musicLabel = CreateLabel(_audioBox, "", new Vector2(0f, 110f), 18, TextAlignmentOptions.Center);
        _musicSlider = CreateSlider(_audioBox, 70f, 0f, 1f, music);
        _musicSlider.onValueChanged.AddListener(OnMusicChanged);

        _sfxLabel = CreateLabel(_audioBox, "", new Vector2(0f, 10f), 18, TextAlignmentOptions.Center);
        _sfxSlider = CreateSlider(_audioBox, -30f, 0f, 1f, sfx);
        _sfxSlider.onValueChanged.AddListener(OnSfxChanged);

        CreateButton(_audioBox, "Natrag", -120f, ShowSettingsHub);
        UpdateMusicLabel(music);
        UpdateSfxLabel(sfx);

        _controlsBox = CreateBox(_rootOverlay.transform, new Vector2(520f, 620f), "CONTROLS");
        _controlsBox.gameObject.SetActive(false);
        NudgeBoxTitle(_controlsBox, -6f);

        _controlsScroll = CreatePanelScroll(_controlsBox, 58f, 62f, out _controlsScrollContent);
        _controlsScrollContent.sizeDelta = new Vector2(480f, 720f);

        var cam = CameraSettingsManager.Instance;
        float mouseSens = cam != null ? cam.mouseSensitivityPercent : 50f;
        float moveSpeed = cam != null ? cam.moveSpeedPercent : 100f;

        float y = -14f;
        _mouseLabel = CreateScrollLabel(_controlsScrollContent, "", ref y, 28f, 18, TextAlignmentOptions.Center);
        _mouseSlider = CreateScrollSlider(_controlsScrollContent, ref y, 10f, 100f, mouseSens);
        _mouseSlider.wholeNumbers = true;
        _mouseSlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
        UpdateMouseLabel(mouseSens);

        _moveSpeedLabel = CreateScrollLabel(_controlsScrollContent, "", ref y, 28f, 18, TextAlignmentOptions.Center);
        _moveSpeedSlider = CreateScrollSlider(_controlsScrollContent, ref y, 10f, 100f, moveSpeed);
        _moveSpeedSlider.wholeNumbers = true;
        _moveSpeedSlider.onValueChanged.AddListener(OnMoveSpeedChanged);
        UpdateMoveSpeedLabel(moveSpeed);

        y -= 6f;
        CreateScrollLabel(_controlsScrollContent, "Gas u igri: Ctrl + scroll (1–100%)", ref y, 24f, 14, TextAlignmentOptions.Center);
        y -= 4f;
        CreateScrollLabel(_controlsScrollContent, "Tipke — klikni pa pritisni novu", ref y, 26f, 16, TextAlignmentOptions.Center);

        CreateRebindRow(_controlsScrollContent, "moveUp", "Naprijed", ref y);
        CreateRebindRow(_controlsScrollContent, "moveDown", "Natrag", ref y);
        CreateRebindRow(_controlsScrollContent, "moveLeft", "Lijevo", ref y);
        CreateRebindRow(_controlsScrollContent, "moveRight", "Desno", ref y);
        CreateRebindRow(_controlsScrollContent, "rotateLeft", "Rotacija lijevo", ref y);
        CreateRebindRow(_controlsScrollContent, "rotateRight", "Rotacija desno", ref y);
        CreateRebindRow(_controlsScrollContent, "openVojnici", "Prozor Vojnici", ref y);
        CreateRebindRow(_controlsScrollContent, "openToranj", "Prozor Toranj", ref y);

        y -= 8f;
        _rebindStatus = CreateScrollLabel(_controlsScrollContent, "", ref y, 24f, 14, TextAlignmentOptions.Center);
        _controlsScrollContent.sizeDelta = new Vector2(480f, Mathf.Max(520f, -y + 24f));

        CreateButton(_controlsBox, "Natrag", -410f, ShowSettingsHub);

        _confirmBox = CreateConfirmDialog(_rootOverlay.transform);

        HideAll();
    }

    RectTransform CreateConfirmDialog(Transform parent)
    {
        GameObject blocker = new GameObject("ConfirmBlocker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        blocker.layer = 5;
        blocker.transform.SetParent(parent, false);
        RectTransform blockRt = blocker.GetComponent<RectTransform>();
        blockRt.anchorMin = Vector2.zero;
        blockRt.anchorMax = Vector2.one;
        blockRt.offsetMin = Vector2.zero;
        blockRt.offsetMax = Vector2.zero;
        Image blockImg = blocker.GetComponent<Image>();
        blockImg.color = new Color(0f, 0f, 0f, 0.45f);
        blockImg.raycastTarget = true;

        RectTransform box = CreateBox(blocker.transform, new Vector2(460f, 260f), "POTVRDA");
        box.anchorMin = new Vector2(0.5f, 0.5f);
        box.anchorMax = new Vector2(0.5f, 0.5f);
        box.anchoredPosition = Vector2.zero;

        GameObject msgObj = new GameObject("Message", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        msgObj.layer = 5;
        msgObj.transform.SetParent(box, false);
        RectTransform msgRt = msgObj.GetComponent<RectTransform>();
        msgRt.anchorMin = new Vector2(0.5f, 0.5f);
        msgRt.anchorMax = new Vector2(0.5f, 0.5f);
        msgRt.pivot = new Vector2(0.5f, 0.5f);
        msgRt.sizeDelta = new Vector2(400f, 90f);
        msgRt.anchoredPosition = new Vector2(0f, 18f);
        TextMeshProUGUI msgTmp = msgObj.GetComponent<TextMeshProUGUI>();
        msgTmp.text = "";
        msgTmp.fontSize = 17;
        msgTmp.alignment = TextAlignmentOptions.Center;
        msgTmp.color = new Color(0.92f, 0.9f, 0.86f, 1f);
        msgTmp.lineSpacing = 4f;
        _confirmMessage = msgTmp;

        GameObject rowObj = new GameObject("ConfirmButtons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        rowObj.layer = 5;
        RectTransform row = rowObj.GetComponent<RectTransform>();
        row.SetParent(box, false);
        row.anchorMin = new Vector2(0.5f, 0f);
        row.anchorMax = new Vector2(0.5f, 0f);
        row.pivot = new Vector2(0.5f, 0f);
        row.sizeDelta = new Vector2(360f, 54f);
        row.anchoredPosition = new Vector2(0f, 28f);

        HorizontalLayoutGroup hlg = row.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 16f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        CreateStackButton(row, "Da", ConfirmYes, 160f);
        CreateStackButton(row, "Ne", HideConfirm, 160f);

        blocker.SetActive(false);
        return blocker.GetComponent<RectTransform>();
    }

    RectTransform CreateButtonStack(RectTransform box, float topInset, float bottomInset)
    {
        GameObject stack = new GameObject("ButtonStack", typeof(RectTransform), typeof(VerticalLayoutGroup));
        stack.layer = 5;
        stack.transform.SetParent(box, false);

        RectTransform rt = stack.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(28f, bottomInset);
        rt.offsetMax = new Vector2(-28f, -topInset);

        VerticalLayoutGroup vlg = stack.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 12f;
        vlg.padding = new RectOffset(0, 0, 4, 4);
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        return rt;
    }

    Button CreateStackButton(RectTransform parent, string label, UnityEngine.Events.UnityAction onClick, float width = 320f)
    {
        GameObject go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.layer = 5;
        go.transform.SetParent(parent, false);

        LayoutElement le = go.GetComponent<LayoutElement>();
        le.preferredWidth = width;
        le.preferredHeight = 52f;
        le.minHeight = 52f;

        Image img = go.GetComponent<Image>();
        img.color = new Color(0.32f, 0.28f, 0.24f, 1f);

        Button btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.45f, 0.4f, 0.34f, 1f);
        cb.pressedColor = new Color(0.25f, 0.22f, 0.19f, 1f);
        btn.colors = cb;

        FillButtonLabel(go.transform, label, 19);
        return btn;
    }

    static void FillButtonLabel(Transform button, string text, float fontSize)
    {
        GameObject go = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.layer = 5;
        go.transform.SetParent(button, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(8f, 0f);
        rt.offsetMax = new Vector2(-8f, 0f);

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.enableWordWrapping = false;
    }

    void CreateRebindRow(RectTransform parent, string id, string label, ref float y)
    {
        GameObject row = new GameObject("Rebind_" + id, typeof(RectTransform));
        row.layer = 5;
        row.transform.SetParent(parent, false);

        RectTransform rrt = row.GetComponent<RectTransform>();
        rrt.anchorMin = new Vector2(0.5f, 1f);
        rrt.anchorMax = new Vector2(0.5f, 1f);
        rrt.pivot = new Vector2(0.5f, 1f);
        rrt.sizeDelta = new Vector2(400f, 34f);
        rrt.anchoredPosition = new Vector2(0f, y);

        GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObj.layer = 5;
        labelObj.transform.SetParent(row.transform, false);
        RectTransform lrt = labelObj.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0f, 0f);
        lrt.anchorMax = new Vector2(0.65f, 1f);
        lrt.offsetMin = new Vector2(8f, 0f);
        lrt.offsetMax = new Vector2(0f, 0f);
        TextMeshProUGUI tmp = labelObj.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 15;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;

        var c = ControlSettingsManager.Instance;
        string keyText = c != null ? c.GetKeyLabel(id) : "?";

        GameObject btnObj = new GameObject("KeyBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.layer = 5;
        btnObj.transform.SetParent(row.transform, false);
        RectTransform brt = btnObj.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.67f, 0.1f);
        brt.anchorMax = new Vector2(1f, 0.9f);
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = new Vector2(-8f, 0f);

        Image img = btnObj.GetComponent<Image>();
        img.color = new Color(0.34f, 0.3f, 0.26f, 1f);
        Button btn = btnObj.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => BeginRebind(id));

        GameObject keyLabelObj = new GameObject("KeyLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        keyLabelObj.layer = 5;
        keyLabelObj.transform.SetParent(btnObj.transform, false);
        RectTransform klrt = keyLabelObj.GetComponent<RectTransform>();
        klrt.anchorMin = Vector2.zero;
        klrt.anchorMax = Vector2.one;
        klrt.offsetMin = Vector2.zero;
        klrt.offsetMax = Vector2.zero;
        TextMeshProUGUI keyTmp = keyLabelObj.GetComponent<TextMeshProUGUI>();
        keyTmp.text = keyText;
        keyTmp.fontSize = 16;
        keyTmp.alignment = TextAlignmentOptions.Center;
        keyTmp.color = Color.white;

        _keyLabels[id] = keyTmp;
        y -= 38f;
    }

    static void NudgeBoxTitle(RectTransform box, float deltaY)
    {
        if (box == null || box.childCount == 0)
            return;

        Transform title = box.GetChild(0);
        RectTransform rt = title as RectTransform;
        if (rt != null)
            rt.anchoredPosition += new Vector2(0f, deltaY);
    }

    ScrollRect CreatePanelScroll(RectTransform box, float topInset, float bottomInset, out RectTransform content)
    {
        const float scrollbarWidth = 12f;

        GameObject area = new GameObject("ScrollArea", typeof(RectTransform));
        area.layer = 5;
        area.transform.SetParent(box, false);
        RectTransform art = area.GetComponent<RectTransform>();
        art.anchorMin = Vector2.zero;
        art.anchorMax = Vector2.one;
        art.offsetMin = new Vector2(8f, bottomInset);
        art.offsetMax = new Vector2(-8f, -topInset);

        Scrollbar scrollbar = CreatePanelScrollbar(area.transform, scrollbarWidth);

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
        viewport.layer = 5;
        viewport.transform.SetParent(area.transform, false);
        RectTransform vrt = viewport.GetComponent<RectTransform>();
        vrt.anchorMin = Vector2.zero;
        vrt.anchorMax = Vector2.one;
        vrt.offsetMin = new Vector2(2f, 2f);
        vrt.offsetMax = new Vector2(-(scrollbarWidth + 6f), -2f);
        viewport.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.001f);

        GameObject inner = new GameObject("Content", typeof(RectTransform));
        inner.layer = 5;
        inner.transform.SetParent(viewport.transform, false);
        content = inner.GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.offsetMin = new Vector2(0f, 0f);
        content.offsetMax = new Vector2(0f, 0f);

        ScrollRect sr = area.AddComponent<ScrollRect>();
        sr.viewport = vrt;
        sr.content = content;
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;
        sr.scrollSensitivity = 20f;
        sr.verticalScrollbar = scrollbar;
        sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        sr.verticalScrollbarSpacing = 4f;
        area.AddComponent<ScrollRectRefresher>();
        return sr;
    }

    Scrollbar CreatePanelScrollbar(Transform parent, float width)
    {
        GameObject sbGO = new GameObject("Scrollbar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Scrollbar));
        sbGO.layer = 5;
        sbGO.transform.SetParent(parent, false);

        RectTransform sbRT = sbGO.GetComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(1f, 0f);
        sbRT.anchorMax = new Vector2(1f, 1f);
        sbRT.pivot = new Vector2(1f, 0.5f);
        sbRT.sizeDelta = new Vector2(width, 0f);
        sbRT.anchoredPosition = new Vector2(-2f, 0f);
        sbGO.GetComponent<Image>().color = new Color(0.09f, 0.08f, 0.07f, 0.9f);

        Scrollbar sb = sbGO.GetComponent<Scrollbar>();
        sb.direction = Scrollbar.Direction.BottomToTop;

        GameObject slideArea = new GameObject("Sliding Area", typeof(RectTransform));
        slideArea.layer = 5;
        slideArea.transform.SetParent(sbGO.transform, false);
        RectTransform saRT = slideArea.GetComponent<RectTransform>();
        saRT.anchorMin = Vector2.zero;
        saRT.anchorMax = Vector2.one;
        saRT.offsetMin = new Vector2(2f, 6f);
        saRT.offsetMax = new Vector2(-2f, -6f);

        GameObject handle = NewImage("Handle", slideArea.transform, new Color(0.45f, 0.4f, 0.34f, 0.95f));
        RectTransform hRT = handle.GetComponent<RectTransform>();
        hRT.anchorMin = Vector2.zero;
        hRT.anchorMax = Vector2.one;
        hRT.sizeDelta = Vector2.zero;

        sb.handleRect = hRT;
        sb.targetGraphic = handle.GetComponent<Image>();
        return sb;
    }

    TextMeshProUGUI CreateScrollLabel(RectTransform parent, string text, ref float yTop, float height, float fontSize, TextAlignmentOptions align)
    {
        TextMeshProUGUI tmp = CreateLabel(parent, text, new Vector2(0f, yTop), fontSize, align);
        RectTransform rt = tmp.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, yTop);
        rt.sizeDelta = new Vector2(460f, height);
        yTop -= height;
        return tmp;
    }

    Slider CreateScrollSlider(RectTransform parent, ref float yTop, float min, float max, float value)
    {
        Slider slider = CreateSlider(parent, yTop, min, max, value);
        RectTransform rt = slider.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, yTop - 14f);
        yTop -= 38f;
        return slider;
    }

    Canvas GetOrCreatePauseCanvas()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Scene active = SceneManager.GetActiveScene();
        Canvas keeper = null;

        foreach (Canvas canvas in canvases)
        {
            if (canvas.gameObject.name != "PauseCanvas")
                continue;
            if (canvas.gameObject.scene != active)
            {
                Destroy(canvas.gameObject);
                continue;
            }
            keeper = canvas;
        }

        if (keeper != null)
            return keeper;

        GameObject go = new GameObject("PauseCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.layer = 5;
        Canvas canvasNew = go.GetComponent<Canvas>();
        canvasNew.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasNew.sortingOrder = 500;
        canvasNew.pixelPerfect = false;

        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return canvasNew;
    }

    void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    GameObject CreateOverlay(string name, Transform parent)
    {
        GameObject overlay = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlay.layer = 5;
        overlay.transform.SetParent(parent, false);

        RectTransform rt = overlay.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
        rt.anchoredPosition = Vector2.zero;

        Image img = overlay.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.65f);
        img.raycastTarget = true;
        return overlay;
    }

    RectTransform CreateBox(Transform parent, Vector2 size, string title)
    {
        GameObject box = new GameObject("Box_" + title, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        box.layer = 5;
        box.transform.SetParent(parent, false);

        RectTransform rt = box.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

        box.GetComponent<Image>().color = new Color(0.13f, 0.12f, 0.11f, 0.98f);

        CreateTitleLabel(rt, title);
        return rt;
    }

    void CreateTitleLabel(RectTransform box, string title)
    {
        GameObject go = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.layer = 5;
        go.transform.SetParent(box, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(0f, 52f);
        rt.anchoredPosition = Vector2.zero;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = title;
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
    }

    Button CreateButton(RectTransform parent, string label, float yPos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.layer = 5;
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(300f, 54f);
        rt.anchoredPosition = new Vector2(0f, yPos);

        Image img = go.GetComponent<Image>();
        img.color = new Color(0.32f, 0.28f, 0.24f, 1f);

        Button btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.45f, 0.4f, 0.34f, 1f);
        cb.pressedColor = new Color(0.25f, 0.22f, 0.19f, 1f);
        btn.colors = cb;

        FillButtonLabel(go.transform, label, 20);
        return btn;
    }

    TextMeshProUGUI CreateLabel(RectTransform parent, string text, Vector2 pos, float fontSize, TextAlignmentOptions align)
    {
        GameObject go = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.layer = 5;
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(440f, 40f);
        rt.anchoredPosition = pos;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = Color.white;
        return tmp;
    }

    Slider CreateSlider(RectTransform parent, float yPos, float min, float max, float value)
    {
        GameObject root = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
        root.layer = 5;
        root.transform.SetParent(parent, false);

        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(360f, 26f);
        rt.anchoredPosition = new Vector2(0f, yPos);

        Slider slider = root.GetComponent<Slider>();

        GameObject bg = NewImage("Background", root.transform, new Color(0.08f, 0.08f, 0.08f, 1f));
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 0.3f);
        bgRt.anchorMax = new Vector2(1f, 0.7f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.layer = 5;
        fillArea.transform.SetParent(root.transform, false);
        RectTransform faRt = fillArea.GetComponent<RectTransform>();
        faRt.anchorMin = new Vector2(0f, 0.3f);
        faRt.anchorMax = new Vector2(1f, 0.7f);
        faRt.offsetMin = new Vector2(5f, 0f);
        faRt.offsetMax = new Vector2(-15f, 0f);

        GameObject fill = NewImage("Fill", fillArea.transform, new Color(0.3f, 0.7f, 0.9f, 1f));
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.sizeDelta = new Vector2(10f, 0f);

        GameObject hsa = new GameObject("Handle Slide Area", typeof(RectTransform));
        hsa.layer = 5;
        hsa.transform.SetParent(root.transform, false);
        RectTransform hsaRt = hsa.GetComponent<RectTransform>();
        hsaRt.anchorMin = new Vector2(0f, 0f);
        hsaRt.anchorMax = new Vector2(1f, 1f);
        hsaRt.offsetMin = new Vector2(10f, 0f);
        hsaRt.offsetMax = new Vector2(-10f, 0f);

        GameObject handle = NewImage("Handle", hsa.transform, Color.white);
        RectTransform handleRt = handle.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(20f, 0f);

        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = min;
        slider.maxValue = max;
        slider.SetValueWithoutNotify(value);
        return slider;
    }

    GameObject NewImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go;
    }
}
