using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DeploymentSystem : MonoBehaviour
{
    [System.Serializable]
    public class Lokacija {
        public string naziv;
        public Transform spawnPoint;
        [HideInInspector] public bool zauzeto;
    }

    public EconomyManager economy;
    public GameManager gameManager;
    public GameObject vojnikPrefab;
    public Lokacija[] lokacije;

    [Header("Postavke")]
    public int maxVojnika = 12;
    public int cijenaPostavljanja = 15;
    public int chiefLokacijaIndex = 0;

    class Vojnik {
        public bool chief;
        public int lokacija = -1;
        public GameObject instance;
        public string oruzje = "-";
        public int oruzjeTierCijena;
        public bool deployPlacen;
    }

    struct WeaponOffer {
        public string naziv;
        public int cijena;
    }

    enum SubMenu { None, Pozicija, Oruzje }

    readonly List<Vojnik> _vojnici = new List<Vojnik>();
    RectTransform _container;
    int _expanded = -1;
    SubMenu _sub = SubMenu.None;
    bool _init;
    int _lastCoins = int.MinValue;
    bool _renderQueued;

    void EnsureInit()
    {
        if (_init) return;
        _init = true;

        if (economy == null) economy = EconomyManager.Instance;
        if (gameManager == null) gameManager = GameManager.Instance;

        Vojnik chief = new Vojnik { chief = true };
        _vojnici.Add(chief);

        if (lokacije != null && lokacije.Length > 0)
        {
            int idx = (chiefLokacijaIndex >= 0 && chiefLokacijaIndex < lokacije.Length) ? chiefLokacijaIndex : 0;
            Deploy(chief, idx, true);
            AssignDefaultWeapon(chief);
        }

        AddEmptySlotIfNeeded();
    }

    void AddEmptySlotIfNeeded()
    {
        bool imaPrazan = _vojnici.Exists(v => v.lokacija == -1);
        if (!imaPrazan && _vojnici.Count < maxVojnika)
            _vojnici.Add(new Vojnik());
    }

    public void BuildInto(RectTransform container)
    {
        EnsureInit();
        _container = container;

        VerticalLayoutGroup vlg = container.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = container.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 6f;
        vlg.padding = new RectOffset(6, 6, 6, 6);
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = container.GetComponent<ContentSizeFitter>();
        if (csf == null) csf = container.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        RenderNow();
        RefreshScroll();
    }

    void LateUpdate()
    {
        if (!_renderQueued || _container == null)
            return;

        _renderQueued = false;
        if (!_container.gameObject.activeInHierarchy)
            return;

        RenderNow();
    }

    void QueueRender()
    {
        _renderQueued = true;
    }

    public void OnWindowOpened()
    {
        _expanded = -1;
        _sub = SubMenu.None;
        if (_container != null)
            RenderNow();
    }

    void RenderNow()
    {
        if (_container == null) return;

        for (int i = _container.childCount - 1; i >= 0; i--)
            Destroy(_container.GetChild(i).gameObject);

        for (int i = 0; i < _vojnici.Count; i++)
        {
            int index = i;
            Vojnik v = _vojnici[i];

            string ime = v.chief ? "Chief komandir" : "Vojnik " + i;
            string status = v.lokacija >= 0 ? lokacije[v.lokacija].naziv : "neraspoređen";
            string oruzjeLabel = HasWeapon(v) ? v.oruzje : "— nema";
            bool canOruzje = CanUseOruzjeMenu(v);
            bool canPoz = !v.chief && (v.lokacija >= 0 || v.deployPlacen || CanAffordDeploy(v));
            CreateRow(ime, status, oruzjeLabel, !v.chief, canPoz,
                () => TryOpenPozicija(index),
                () => TryOpenOruzje(index),
                true, canOruzje);

            if (_expanded == index && _sub == SubMenu.Pozicija)
                BuildPozicije(v);
            else if (_expanded == index && _sub == SubMenu.Oruzje)
                BuildOruzja(v);
        }

        if (_sub != SubMenu.None)
            ScrollSubmenuIntoView();

        RefreshScroll();

        if (economy != null)
            _lastCoins = economy.coins;
    }

    void Update()
    {
        if (_container == null || economy == null)
            return;
        if (economy.coins == _lastCoins)
            return;

        QueueRender();
    }

    public void RefreshScroll()
    {
        if (_container == null)
            return;

        ScrollRect sr = _container.GetComponentInParent<ScrollRect>();
        UiScrollHelper.Refresh(sr);

        ScrollRectRefresher refresher = sr != null ? sr.GetComponent<ScrollRectRefresher>() : null;
        if (refresher != null)
            refresher.RefreshNow();
    }

    void ScrollSubmenuIntoView()
    {
        if (_container == null)
            return;

        ScrollRect sr = _container.GetComponentInParent<ScrollRect>();
        if (sr == null)
            return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_container);

        float contentH = _container.rect.height;
        float viewH = sr.viewport != null ? sr.viewport.rect.height : 0f;
        if (contentH <= viewH)
            return;

        sr.verticalNormalizedPosition = 0f;
    }

    void ToggleSub(int index, SubMenu sub)
    {
        if (_expanded == index && _sub == sub)
        {
            _expanded = -1;
            _sub = SubMenu.None;
        }
        else
        {
            _expanded = index;
            _sub = sub;
        }
        QueueRender();
    }

    static bool HasWeapon(Vojnik v)
    {
        return v != null && v.oruzje != "-" && !string.IsNullOrEmpty(v.oruzje);
    }

    void TryOpenPozicija(int index)
    {
        if (index < 0 || index >= _vojnici.Count)
            return;
        Vojnik v = _vojnici[index];
        if (!v.chief && v.lokacija < 0 && !v.deployPlacen && !CanAffordDeploy(v))
            return;
        ToggleSub(index, SubMenu.Pozicija);
    }

    bool CanAffordDeploy(Vojnik v)
    {
        if (economy == null)
            return false;
        if (v.lokacija >= 0 || v.deployPlacen)
            return true;
        return economy.coins >= GetDeployCost(v);
    }

    bool NeedsDeployPayment(Vojnik v)
    {
        return !v.chief && !v.deployPlacen;
    }

    void TryOpenOruzje(int index)
    {
        if (index < 0 || index >= _vojnici.Count)
            return;
        Vojnik v = _vojnici[index];
        if (!CanUseOruzjeMenu(v))
            return;
        ToggleSub(index, SubMenu.Oruzje);
    }

    static bool IsDeployed(Vojnik v)
    {
        return v != null && (v.chief || v.lokacija >= 0);
    }

    bool CanUseOruzjeMenu(Vojnik v)
    {
        return IsDeployed(v) && !ShouldGrayOruzjeButton(v);
    }

    bool ShouldGrayOruzjeButton(Vojnik v)
    {
        if (!IsDeployed(v))
            return true;

        int val = gameManager != null ? gameManager.trenutniVal : 1;
        WeaponOffer[] offers = GetWeaponOffers(val);

        if (offers.Length <= 1)
        {
            WeaponOffer only = offers[0];
            if (v.oruzje == only.naziv)
                return true;

            if (v.chief)
                return false;

            return economy == null || economy.coins < only.cijena;
        }

        if (v.chief)
            return false;

        if (!HasWeapon(v))
        {
            int cost = GetCheapestWeapon(val).cijena;
            return economy == null || economy.coins < cost;
        }

        foreach (WeaponOffer offer in offers)
        {
            if (v.oruzje == offer.naziv)
                continue;

            int diff = Mathf.Max(0, offer.cijena - v.oruzjeTierCijena);
            return economy == null || economy.coins < diff;
        }
        return false;
    }

    bool CanAffordWeaponUpgrade(Vojnik v, WeaponOffer offer)
    {
        if (!IsDeployed(v))
            return false;
        if (v.oruzje == offer.naziv)
            return false;
        if (v.chief)
            return true;

        int diff = Mathf.Max(0, offer.cijena - v.oruzjeTierCijena);
        return diff == 0 || (economy != null && economy.coins >= diff);
    }

    int GetDeployCost(Vojnik v)
    {
        if (v.lokacija >= 0 || v.deployPlacen)
            return 0;

        int cost = cijenaPostavljanja;
        if (!HasWeapon(v))
        {
            int val = gameManager != null ? gameManager.trenutniVal : 1;
            cost += GetCheapestWeapon(val).cijena;
        }
        return cost;
    }

    void BuildPozicije(Vojnik v)
    {
        if (v.chief)
            return;

        if (v.lokacija >= 0)
            CreateFullButton("✕ Makni s pozicije", () => { Undeploy(v); QueueRender(); CloseSub(); });

        bool needsPayment = NeedsDeployPayment(v);
        int deployCost = GetDeployCost(v);
        bool canAfford = CanAffordDeploy(v);

        for (int i = 0; i < lokacije.Length; i++)
        {
            int lokIndex = i;
            if (lokacije[i].zauzeto) continue;

            string label = needsPayment
                ? "→ " + lokacije[i].naziv + "  (" + deployCost + " coin)"
                : "→ " + lokacije[i].naziv;

            bool disabled = needsPayment && !canAfford;
            CreateFullButton(label, () =>
            {
                if (Deploy(v, lokIndex, false))
                    CloseSub();
            }, disabled);
        }
    }

    void BuildOruzja(Vojnik v)
    {
        int val = gameManager != null ? gameManager.trenutniVal : 1;
        WeaponOffer[] offers = GetWeaponOffers(val);

        foreach (WeaponOffer offer in offers)
        {
            string naziv = offer.naziv;
            int cijena = offer.cijena;
            bool owned = v.oruzje == naziv;
            int diff = Mathf.Max(0, cijena - v.oruzjeTierCijena);

            string label;
            if (owned)
                label = naziv + "  ✓";
            else if (v.chief)
                label = naziv + "  (besplatno)";
            else if (diff == 0)
                label = naziv + "  (besplatno)";
            else
                label = naziv + "  (+" + diff + " coins)";

            bool disabled = owned || !CanAffordWeaponUpgrade(v, offer);
            CreateFullButton(label, () =>
            {
                if (owned || !CanAffordWeaponUpgrade(v, offer))
                {
                    CloseSub();
                    return;
                }
                TryUpgradeWeapon(v, naziv, cijena);
                CloseSub();
            }, disabled);
        }
    }

    WeaponOffer[] GetWeaponOffers(int val)
    {
        if (val <= 1)
            return new[] { new WeaponOffer { naziv = "Luk", cijena = 7 } };
        if (val == 2)
            return new[] { new WeaponOffer { naziv = "Bodež", cijena = 5 } };
        return new[]
        {
            new WeaponOffer { naziv = "Mač", cijena = 6 },
            new WeaponOffer { naziv = "Sjekira", cijena = 7 }
        };
    }

    WeaponOffer GetMostExpensiveWeapon(int val)
    {
        WeaponOffer[] offers = GetWeaponOffers(val);
        WeaponOffer best = offers[0];
        for (int i = 1; i < offers.Length; i++)
        {
            if (offers[i].cijena > best.cijena)
                best = offers[i];
        }
        return best;
    }

    WeaponOffer GetCheapestWeapon(int val)
    {
        WeaponOffer[] offers = GetWeaponOffers(val);
        WeaponOffer best = offers[0];
        for (int i = 1; i < offers.Length; i++)
        {
            if (offers[i].cijena < best.cijena)
                best = offers[i];
        }
        return best;
    }

    void AssignDefaultWeapon(Vojnik v)
    {
        int val = gameManager != null ? gameManager.trenutniVal : 1;
        WeaponOffer def = v.chief ? GetMostExpensiveWeapon(val) : GetCheapestWeapon(val);
        v.oruzje = def.naziv;
        v.oruzjeTierCijena = def.cijena;
        ApplyWeaponToInstance(v);
    }

    void TryUpgradeWeapon(Vojnik v, string naziv, int cijena)
    {
        if (v.oruzje == naziv)
            return;

        if (!IsDeployed(v))
            return;

        if (!v.chief)
        {
            int diff = Mathf.Max(0, cijena - v.oruzjeTierCijena);
            if (economy != null && economy.coins < diff)
                return;

            if (economy != null)
                economy.coins -= diff;
        }

        v.oruzje = naziv;
        v.oruzjeTierCijena = cijena;
        ApplyWeaponToInstance(v);
        QueueRender();
    }

    void ApplyWeaponToInstance(Vojnik v)
    {
        if (v.instance == null)
            return;

        WeaponStats stats = v.instance.GetComponent<WeaponStats>();
        if (stats != null)
        {
            stats.SetWave(gameManager != null ? gameManager.trenutniVal : 1);
            stats.ApplyRoleModifiers(v.chief);
        }

        SoldierAI ai = v.instance.GetComponent<SoldierAI>();
        if (ai != null)
            ai.ApplyRoleModifiers(v.chief);
    }

    void CloseSub()
    {
        _expanded = -1;
        _sub = SubMenu.None;
        QueueRender();
    }

    public void CloseOpenMenu()
    {
        _expanded = -1;
        _sub = SubMenu.None;
    }

    bool Deploy(Vojnik v, int lokIndex, bool free)
    {
        if (lokacije == null || lokIndex < 0 || lokIndex >= lokacije.Length) return false;
        if (lokacije[lokIndex].zauzeto) return false;

        if (v.chief && lokIndex != chiefLokacijaIndex)
            return false;

        bool noviRaspored = v.lokacija == -1;
        bool needsWeapon = !HasWeapon(v);

        if (!free && NeedsDeployPayment(v))
        {
            int cost = GetDeployCost(v);
            if (economy == null || economy.coins < cost)
                return false;
            economy.coins -= cost;
            v.deployPlacen = true;
        }

        if (v.lokacija >= 0 && v.lokacija < lokacije.Length)
            lokacije[v.lokacija].zauzeto = false;

        if (v.instance != null)
            Destroy(v.instance);

        Lokacija lok = lokacije[lokIndex];
        if (vojnikPrefab != null && lok.spawnPoint != null)
            v.instance = Instantiate(vojnikPrefab, lok.spawnPoint.position, lok.spawnPoint.rotation);

        lok.zauzeto = true;
        v.lokacija = lokIndex;

        if (needsWeapon)
            AssignDefaultWeapon(v);
        else
            ApplyWeaponToInstance(v);

        if (noviRaspored)
            AddEmptySlotIfNeeded();

        QueueRender();
        return true;
    }

    public void ResetForScene()
    {
        _init = false;
        _vojnici.Clear();
        _expanded = -1;
        _sub = SubMenu.None;
        _lastCoins = int.MinValue;
        _container = null;

        if (lokacije != null)
        {
            for (int i = 0; i < lokacije.Length; i++)
                lokacije[i].zauzeto = false;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (gameObject.scene != scene)
            return;
        ResetForScene();
    }

    void Undeploy(Vojnik v)
    {
        if (v.chief)
            return;

        if (v.lokacija >= 0 && v.lokacija < lokacije.Length)
            lokacije[v.lokacija].zauzeto = false;
        if (v.instance != null)
            Destroy(v.instance);
        v.lokacija = -1;
    }

    void CreateRow(string ime, string status, string oruzje, bool showPozicija, bool pozicijaEnabled, UnityEngine.Events.UnityAction onPoz, UnityEngine.Events.UnityAction onOru, bool showOruzje, bool oruzjeEnabled)
    {
        GameObject row = new GameObject("Row", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        row.layer = 5;
        row.transform.SetParent(_container, false);
        row.GetComponent<Image>().color = new Color(0.2f, 0.18f, 0.16f, 0.9f);
        row.GetComponent<LayoutElement>().preferredHeight = 54f;

        GameObject info = NewText("Info", row.transform,
            ime + "\n<size=12>" + status + " | oružje: " + oruzje + "</size>",
            14, TextAlignmentOptions.Left);
        RectTransform irt = info.GetComponent<RectTransform>();
        irt.anchorMin = new Vector2(0f, 0f);
        irt.anchorMax = new Vector2(0.55f, 1f);
        irt.offsetMin = new Vector2(8f, 0f);
        irt.offsetMax = new Vector2(0f, 0f);

        if (showPozicija)
        {
            Vector2 pozAnchor = showOruzje ? new Vector2(0.55f, 0.78f) : new Vector2(0.55f, 1.0f);
            CreateSmallButton("Pozicija", row.transform, pozAnchor, onPoz, pozicijaEnabled);
        }
        if (showOruzje)
            CreateSmallButton("Oružje", row.transform, showPozicija ? new Vector2(0.785f, 1.0f) : new Vector2(0.55f, 1.0f), onOru, oruzjeEnabled);
    }

    void CreateSmallButton(string label, Transform parent, Vector2 anchorX, UnityEngine.Events.UnityAction onClick, bool enabled = true)
    {
        GameObject go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.layer = 5;
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(anchorX.x, 0.18f);
        rt.anchorMax = new Vector2(anchorX.y, 0.82f);
        rt.offsetMin = new Vector2(4f, 0f);
        rt.offsetMax = new Vector2(-4f, 0f);

        Image img = go.GetComponent<Image>();
        img.color = enabled
            ? new Color(0.34f, 0.3f, 0.26f, 1f)
            : new Color(0.22f, 0.2f, 0.18f, 0.75f);

        Button btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.interactable = enabled;
        btn.onClick.AddListener(onClick);

        NewTextFill(go.transform, label, 13);
    }

    void CreateFullButton(string label, UnityEngine.Events.UnityAction onClick, bool disabled = false)
    {
        GameObject go = new GameObject("FullBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.layer = 5;
        go.transform.SetParent(_container, false);
        go.GetComponent<LayoutElement>().preferredHeight = 36f;

        Image img = go.GetComponent<Image>();
        img.color = disabled
            ? new Color(0.22f, 0.24f, 0.22f, 0.85f)
            : new Color(0.28f, 0.32f, 0.3f, 1f);

        Button btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.interactable = !disabled;
        btn.onClick.AddListener(onClick);

        NewTextFill(go.transform, label, 13);
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
        tmp.richText = true;
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
