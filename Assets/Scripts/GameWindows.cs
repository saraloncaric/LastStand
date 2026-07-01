using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameWindows : MonoBehaviour
{
    enum ResizeMode { None, VerticalOnly, Both }

    const float TitleBarHeight = 34f;

    GameObject _vojniciWin;
    GameObject _toranjWin;
    DeploymentSystem _deployment;
    readonly List<GameObject> _ownedUi = new List<GameObject>();
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

        GameWindows[] all = Object.FindObjectsByType<GameWindows>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        GameWindows keeper = null;
        Scene active = SceneManager.GetActiveScene();

        foreach (GameWindows gw in all)
        {
            if (gw.gameObject.scene == active)
            {
                keeper = gw;
                break;
            }
        }

        foreach (GameWindows gw in all)
        {
            if (gw != keeper)
                Object.Destroy(gw.gameObject);
        }

        if (keeper == null)
            new GameObject("GameWindows (auto)").AddComponent<GameWindows>();
    }

    void Start() => Rebuild();
    void Rebuild()
    {
        CleanupOwnedUi();

        Canvas canvas = GetGameCanvas();
        if (canvas == null)
            return;

        PanelNavigacija nav = FindFirstObjectByType<PanelNavigacija>(FindObjectsInactive.Include);
        if (nav != null)
            nav.enabled = false;

        MenuManager menu = FindFirstObjectByType<MenuManager>(FindObjectsInactive.Include);
        if (menu != null)
        {
            GameObject meniBtn = menu.meniButton != null ? menu.meniButton : GameObject.Find("Meni");
            if (meniBtn != null) meniBtn.SetActive(false);
            menu.enabled = false;
        }
        else
        {
            GameObject meniBtn = GameObject.Find("Meni");
            if (meniBtn != null) meniBtn.SetActive(false);
        }

        RectTransform vojniciContent;
        _vojniciWin = CreateWindow(canvas, "VOJNICI", new Vector2(-230f, -10f), new Vector2(380f, 300f),
            ResizeMode.VerticalOnly, true, out vojniciContent, CloseVojniciMenu);
        TrackUi(_vojniciWin);
        DeploymentSystem dep = FindFirstObjectByType<DeploymentSystem>(FindObjectsInactive.Include);
        _deployment = dep;
        if (dep != null)
        {
            dep.enabled = true;
            dep.ResetForScene();
            dep.BuildInto(vojniciContent);
        }

        RectTransform toranjContent;
        _toranjWin = CreateWindow(canvas, "TORANJ", new Vector2(250f, -10f), new Vector2(400f, 300f),
            ResizeMode.None, false, out toranjContent);
        TrackUi(_toranjWin);
        BuildToranj(toranjContent);

        CreateSideButton(canvas, "Vojnici", new Vector2(20f, -210f), _vojniciWin);
        CreateSideButton(canvas, "Toranj", new Vector2(20f, -264f), _toranjWin);
    }

    void CleanupOwnedUi()
    {
        for (int i = _ownedUi.Count - 1; i >= 0; i--)
        {
            if (_ownedUi[i] != null)
                Destroy(_ownedUi[i]);
        }
        _ownedUi.Clear();
        _vojniciWin = null;
        _toranjWin = null;
        _deployment = null;
    }

    void TrackUi(GameObject go)
    {
        if (go != null)
            _ownedUi.Add(go);
    }

    static Canvas GetGameCanvas()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Canvas fallback = null;
        foreach (Canvas canvas in canvases)
        {
            if (canvas.gameObject.name == "PauseCanvas")
                continue;
            if (canvas.gameObject.scene != SceneManager.GetActiveScene())
                continue;
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return canvas;
            if (fallback == null)
                fallback = canvas;
        }
        return fallback;
    }

    void Update()
    {
        if (PauseMenu.IsPaused)
            return;

        ControlSettingsManager ctrl = ControlSettingsManager.Instance;
        if (ctrl == null)
            return;

        if (ctrl.WasPressed(ctrl.openVojnici))
            ToggleWindow(_vojniciWin);
        if (ctrl.WasPressed(ctrl.openToranj))
            ToggleWindow(_toranjWin);
    }

    void ToggleWindow(GameObject window)
    {
        if (window == null)
            return;

        bool show = !window.activeSelf;
        window.SetActive(show);
        if (show)
        {
            window.transform.SetAsLastSibling();
            if (window == _vojniciWin && _deployment != null)
                _deployment.OnWindowOpened();
            else if (_deployment != null)
                _deployment.RefreshScroll();
        }
        else if (window == _vojniciWin)
        {
            CloseVojniciMenu();
        }
    }

    void CloseVojniciMenu()
    {
        if (_deployment != null)
            _deployment.CloseOpenMenu();
    }

    void BuildToranj(RectTransform content)
    {
        VerticalLayoutGroup vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 6f;
        vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        EconomyManager eco = FindFirstObjectByType<EconomyManager>(FindObjectsInactive.Include);
        TowerHealthUI thui = FindFirstObjectByType<TowerHealthUI>(FindObjectsInactive.Include);

        int cijena = thui != null ? thui.cijenaPopravka : 50;
        float healPct = thui != null ? thui.healPercent : 0.25f;

        if (thui != null)
        {
            AddTowerRow(content, "Glavni toranj", thui.glavniToranj, eco, cijena, healPct);
            AddTowerRow(content, "Lijevi toranj", thui.lijeviBocniToranj, eco, cijena, healPct);
            AddTowerRow(content, "Desni toranj", thui.desniBocniToranj, eco, cijena, healPct);
            AddTowerRow(content, "Zid", thui.zid, eco, cijena, healPct);
        }
        else
        {
            GameObject goal = GameObject.FindGameObjectWithTag("Goal");
            Health h = goal != null ? goal.GetComponent<Health>() : null;
            AddTowerRow(content, "Toranj", h, eco, cijena, healPct);
        }
    }

    void AddTowerRow(RectTransform parent, string naziv, Health health, EconomyManager eco, int cijena, float healPct)
    {
        if (health == null)
            return;

        GameObject row = new GameObject("Row_" + naziv, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        row.layer = 5;
        row.transform.SetParent(parent, false);
        row.GetComponent<Image>().color = new Color(0.2f, 0.18f, 0.16f, 0.9f);
        row.GetComponent<LayoutElement>().preferredHeight = 48f;

        GameObject info = NewText("Info", row.transform, naziv, 15, TextAlignmentOptions.Left);
        RectTransform irt = info.GetComponent<RectTransform>();
        irt.anchorMin = new Vector2(0f, 0f);
        irt.anchorMax = new Vector2(0.62f, 1f);
        irt.offsetMin = new Vector2(10f, 0f);
        irt.offsetMax = new Vector2(0f, 0f);

        TowerHpLabel hp = info.AddComponent<TowerHpLabel>();
        hp.health = health;
        hp.text = info.GetComponent<TextMeshProUGUI>();
        hp.naziv = naziv;

        GameObject btn = new GameObject("Popravi", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(TowerRepairRow));
        btn.layer = 5;
        btn.transform.SetParent(row.transform, false);
        RectTransform brt = btn.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.64f, 0.2f);
        brt.anchorMax = new Vector2(1f, 0.8f);
        brt.offsetMin = new Vector2(4f, 0f);
        brt.offsetMax = new Vector2(-8f, 0f);
        btn.GetComponent<Image>().color = new Color(0.28f, 0.4f, 0.3f, 1f);

        TowerRepairRow repair = btn.GetComponent<TowerRepairRow>();
        repair.health = health;
        repair.economy = eco;
        repair.repairCost = cijena;
        repair.healPercent = healPct;

        NewTextFill(btn.transform, "Popravi", 13);
    }

    GameObject CreateWindow(Canvas canvas, string title, Vector2 pos, Vector2 size, ResizeMode resize, bool scroll, out RectTransform content, System.Action onClose = null)
    {
        GameObject panel = new GameObject("Window_" + title, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.layer = 5;
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        panel.GetComponent<Image>().color = new Color(0.13f, 0.12f, 0.11f, 0.98f);

        AddTitleBar(panel, title, onClose);

        GameObject area = new GameObject("ContentArea", typeof(RectTransform));
        area.layer = 5;
        area.transform.SetParent(panel.transform, false);
        RectTransform art = area.GetComponent<RectTransform>();
        art.anchorMin = new Vector2(0f, 0f);
        art.anchorMax = new Vector2(1f, 1f);
        art.offsetMin = new Vector2(0f, 0f);
        art.offsetMax = new Vector2(0f, -TitleBarHeight);

        if (scroll)
        {
            const float scrollbarWidth = 12f;

            Scrollbar scrollbar = CreateVerticalScrollbar(area.transform, scrollbarWidth);

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
            viewport.layer = 5;
            viewport.transform.SetParent(area.transform, false);
            RectTransform vrt = viewport.GetComponent<RectTransform>();
            vrt.anchorMin = Vector2.zero;
            vrt.anchorMax = Vector2.one;
            vrt.offsetMin = new Vector2(4f, 4f);
            vrt.offsetMax = new Vector2(-(scrollbarWidth + 8f), -4f);
            viewport.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.001f);

            GameObject inner = new GameObject("Content", typeof(RectTransform));
            inner.layer = 5;
            inner.transform.SetParent(viewport.transform, false);
            RectTransform crt = inner.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0f, 1f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.offsetMin = Vector2.zero;
            crt.offsetMax = Vector2.zero;

            ScrollRect sr = area.AddComponent<ScrollRect>();
            sr.viewport = vrt;
            sr.content = crt;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 24f;
            sr.verticalScrollbar = scrollbar;
            sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            sr.verticalScrollbarSpacing = 4f;
            area.AddComponent<ScrollRectRefresher>();

            content = crt;
        }
        else
        {
            content = art;
        }

        if (resize != ResizeMode.None)
            AddResizeHandle(panel, resize == ResizeMode.Both);

        panel.SetActive(false);
        return panel;
    }

    Scrollbar CreateVerticalScrollbar(Transform parent, float width)
    {
        GameObject sbGO = new GameObject("Scrollbar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Scrollbar));
        sbGO.layer = 5;
        sbGO.transform.SetParent(parent, false);

        RectTransform sbRT = sbGO.GetComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(1f, 0f);
        sbRT.anchorMax = new Vector2(1f, 1f);
        sbRT.pivot = new Vector2(1f, 0.5f);
        sbRT.sizeDelta = new Vector2(width, 0f);
        sbRT.anchoredPosition = new Vector2(-4f, 0f);

        Image sbBg = sbGO.GetComponent<Image>();
        sbBg.color = new Color(0.09f, 0.08f, 0.07f, 0.9f);

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

        GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        handle.layer = 5;
        handle.transform.SetParent(slideArea.transform, false);
        RectTransform hRT = handle.GetComponent<RectTransform>();
        hRT.anchorMin = Vector2.zero;
        hRT.anchorMax = Vector2.one;
        hRT.sizeDelta = Vector2.zero;

        Image handleImg = handle.GetComponent<Image>();
        handleImg.color = new Color(0.45f, 0.4f, 0.34f, 0.95f);

        sb.handleRect = hRT;
        sb.targetGraphic = handleImg;

        return sb;
    }

    void AddTitleBar(GameObject panel, string title, System.Action onClose = null)
    {
        RectTransform prt = panel.GetComponent<RectTransform>();

        GameObject bar = NewImage("TitleBar", panel.transform, new Color(0.08f, 0.07f, 0.06f, 1f));
        RectTransform brt = bar.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0f, 1f);
        brt.anchorMax = new Vector2(1f, 1f);
        brt.pivot = new Vector2(0.5f, 1f);
        brt.sizeDelta = new Vector2(0f, TitleBarHeight);
        brt.anchoredPosition = Vector2.zero;
        bar.GetComponent<Image>().raycastTarget = true;

        DraggableWindow drag = bar.AddComponent<DraggableWindow>();
        drag.window = prt;
        drag.clampToScreen = true;

        GameObject titleObj = NewText("Title", bar.transform, title, 16, TextAlignmentOptions.Left);
        RectTransform trt = titleObj.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 0f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.offsetMin = new Vector2(12f, 0f);
        trt.offsetMax = new Vector2(-38f, 0f);

        GameObject close = new GameObject("Close", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        close.layer = 5;
        close.transform.SetParent(bar.transform, false);
        RectTransform crt = close.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(1f, 0.5f);
        crt.anchorMax = new Vector2(1f, 0.5f);
        crt.pivot = new Vector2(1f, 0.5f);
        crt.sizeDelta = new Vector2(28f, 28f);
        crt.anchoredPosition = new Vector2(-4f, 0f);
        close.GetComponent<Image>().color = new Color(0.6f, 0.2f, 0.18f, 1f);
        Button closeBtn = close.GetComponent<Button>();
        closeBtn.targetGraphic = close.GetComponent<Image>();
        GameObject panelRef = panel;
        closeBtn.onClick.AddListener(() =>
        {
            panelRef.SetActive(false);
            onClose?.Invoke();
        });
        NewTextFill(close.transform, "X", 16);

        bar.transform.SetAsLastSibling();
    }

    void AddResizeHandle(GameObject panel, bool horizontal)
    {
        RectTransform prt = panel.GetComponent<RectTransform>();
        GameObject handle = NewImage("ResizeHandle", panel.transform, new Color(1f, 1f, 1f, 0.35f));
        RectTransform hrt = handle.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(1f, 0f);
        hrt.anchorMax = new Vector2(1f, 0f);
        hrt.pivot = new Vector2(1f, 0f);
        hrt.sizeDelta = new Vector2(20f, 20f);
        hrt.anchoredPosition = Vector2.zero;

        ResizableWindow rs = handle.AddComponent<ResizableWindow>();
        rs.window = prt;
        rs.allowHorizontal = horizontal;
        rs.allowVertical = true;
        handle.transform.SetAsLastSibling();
    }

    void CreateSideButton(Canvas canvas, string label, Vector2 pos, GameObject window)
    {
        GameObject go = new GameObject("SideBtn_" + label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.layer = 5;
        go.transform.SetParent(canvas.transform, false);
        TrackUi(go);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(130f, 46f);
        rt.anchoredPosition = pos;

        Image img = go.GetComponent<Image>();
        img.color = new Color(0.32f, 0.28f, 0.24f, 1f);
        Button btn = go.GetComponent<Button>();
        btn.targetGraphic = img;

        NewTextFill(go.transform, label, 18);

        GameObject win = window;
        btn.onClick.AddListener(() => ToggleWindow(win));
    }

    GameObject NewImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go;
    }

    GameObject NewText(string name, Transform parent, string text, float size, TextAlignmentOptions align)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = Color.white;
        return go;
    }

    void NewTextFill(Transform parent, string text, float size)
    {
        GameObject go = NewText("Label", parent, text, size, TextAlignmentOptions.Center);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
