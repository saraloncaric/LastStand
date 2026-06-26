using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    public GameObject menuPanel;
    public PanelNavigacija panelNavigacija;
    public GameManager gameManager;

    [Tooltip("Stari gumb 'Meni' na ekranu - automatski se sakriva (koristi se tipka M)")]
    public GameObject meniButton;

    bool _meniButtonHidden;

    void Start()
    {
        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    void Update()
    {
        HideMeniButtonOnce();

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.mKey.wasPressedThisFrame)
            ToggleMenu();

        if (menuPanel != null && menuPanel.activeSelf &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
            CloseMenu();
    }

    void HideMeniButtonOnce()
    {
        if (_meniButtonHidden)
            return;

        _meniButtonHidden = true;

        if (meniButton == null)
            meniButton = GameObject.Find("Meni");

        if (meniButton != null)
            meniButton.SetActive(false);
    }

    public void ToggleMenu()
    {
        if (menuPanel != null && menuPanel.activeSelf)
            CloseMenu();
        else
            OpenMenu();
    }

    public void CloseMenu()
    {
        ZatvoriMeni();
    }

    public void ZatvoriMeni()
    {
        if (panelNavigacija != null)
            panelNavigacija.PrikaziGlavniMeni();

        if (menuPanel != null)
            menuPanel.SetActive(false);

        Time.timeScale = 1f;
        SetSkipButtonVisible(true);
    }

    void OpenMenu()
    {
        if (menuPanel == null)
            return;

        menuPanel.SetActive(true);

        if (panelNavigacija != null)
            panelNavigacija.PrikaziGlavniMeni();

        if (gameManager != null && gameManager.trenutnafaza == GameManager.GamePhase.Val)
            Time.timeScale = 0f;

        SetSkipButtonVisible(false);
    }

    void SetSkipButtonVisible(bool visible)
    {
        if (gameManager != null && gameManager.uiManager != null)
            gameManager.uiManager.SetSkipButtonVisible(visible);
    }
}
