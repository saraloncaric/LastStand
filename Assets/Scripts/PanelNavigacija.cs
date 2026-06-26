using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class PanelNavigacija : MonoBehaviour
{
    public GameObject glavniMeniPanel;
    public GameObject healthPanel;
    public GameObject lokacijePanel;
    public GameObject vojniciPanel;

    [Header("Navigacija (X -> strelica nazad)")]
    [Tooltip("'X' gumb (Zatvori) - pretvara se u strelicu nazad i vraca na glavni meni")]
    public Button zatvoriButton;
    [Tooltip("Opcionalno: stara mrtva strelica koja se sakriva")]
    public GameObject staraStrelica;

    Transform _vojniciBackButton;

    void Awake()
    {
        if (vojniciPanel != null)
            _vojniciBackButton = vojniciPanel.transform.Find("Natrag");
    }

    void Start()
    {
        SetActiveSafe(glavniMeniPanel, false);
        SetActiveSafe(healthPanel, false);
        SetActiveSafe(lokacijePanel, false);
        SetActiveSafe(vojniciPanel, false);

        DisablePanelBackgroundRaycasts();
        SetupBackButton();
        SakrijPopisLokacija();
    }

    void SakrijPopisLokacija()
    {
        if (healthPanel != null)
        {
            Transform popis = healthPanel.transform.Find("Popis lokacija");
            if (popis != null)
                popis.gameObject.SetActive(false);
        }

        if (lokacijePanel != null)
            lokacijePanel.SetActive(false);
    }

    void DisablePanelBackgroundRaycasts()
    {
        SetPanelBlocksRaycasts(glavniMeniPanel, false);
        SetPanelBlocksRaycasts(healthPanel, false);
        SetPanelBlocksRaycasts(lokacijePanel, false);
        SetPanelBlocksRaycasts(vojniciPanel, false);
    }

    void SetupBackButton()
    {
        if (zatvoriButton != null)
        {
            TextMeshProUGUI tmp = zatvoriButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
                tmp.text = "\u2190";

            Text uiText = zatvoriButton.GetComponentInChildren<Text>(true);
            if (uiText != null)
                uiText.text = "\u2190";

            int persistentCount = zatvoriButton.onClick.GetPersistentEventCount();
            for (int i = 0; i < persistentCount; i++)
                zatvoriButton.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);

            zatvoriButton.onClick.RemoveAllListeners();
            zatvoriButton.onClick.AddListener(Natrag);
        }

        if (staraStrelica != null)
            staraStrelica.SetActive(false);
    }

    public void Natrag()
    {
        PrikaziGlavniMeni();
    }

    public void PrikaziGlavniMeni()
    {
        SetActiveSafe(glavniMeniPanel, true);
        SetActiveSafe(healthPanel, false);
        SetActiveSafe(lokacijePanel, false);
        SetActiveSafe(vojniciPanel, false);
        SetVojniciBackButton(false);
        BringBackButtonToFront();
    }

    public void PrikaziHealth()
    {
        SetActiveSafe(glavniMeniPanel, false);
        SetActiveSafe(healthPanel, true);
        SetActiveSafe(lokacijePanel, false);
        SetActiveSafe(vojniciPanel, false);
        SetPanelBlocksRaycasts(healthPanel, false);
        SetVojniciBackButton(false);
        BringBackButtonToFront();
    }

    public void PrikaziVojnike()
    {
        SetActiveSafe(glavniMeniPanel, false);
        SetActiveSafe(healthPanel, false);
        SetActiveSafe(lokacijePanel, false);
        SetActiveSafe(vojniciPanel, true);
        SetPanelBlocksRaycasts(vojniciPanel, false);
        BringTabsToFront(vojniciPanel);
        SetVojniciBackButton(true);
        BringBackButtonToFront();
    }

    void BringBackButtonToFront()
    {
        if (zatvoriButton != null)
            zatvoriButton.transform.SetAsLastSibling();
    }

    // Tab gumbi (Vojnici/Toranj) su prva djeca panela pa ih ScrollRect "Popis vojnika"
    // (zadnji sibling, raycastTarget=1) prekriva i hvata klikove. Dizemo ih iznad.
    void BringTabsToFront(GameObject panel)
    {
        if (panel == null)
            return;

        Transform root = panel.transform;
        var tabs = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.GetComponent<Button>() != null)
                tabs.Add(child);
        }

        foreach (Transform tab in tabs)
            tab.SetAsLastSibling();
    }

    void SetVojniciBackButton(bool visible)
    {
        if (_vojniciBackButton != null)
            _vojniciBackButton.gameObject.SetActive(visible);
    }

    void SetActiveSafe(GameObject go, bool active)
    {
        if (go != null)
            go.SetActive(active);
    }

    void SetPanelBlocksRaycasts(GameObject panel, bool blockRaycasts)
    {
        if (panel == null)
            return;

        Image image = panel.GetComponent<Image>();
        if (image != null)
            image.raycastTarget = blockRaycasts;
    }
}
