using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject menuPanel;
    public PanelNavigacija panelNavigacija;
    public GameManager gameManager;

    void Start() {
        menuPanel.SetActive(false);
    }

    public void ToggleMenu() {
        bool trebaOtvoriti = !menuPanel.activeSelf;
        menuPanel.SetActive(trebaOtvoriti);

        if (trebaOtvoriti) {
            panelNavigacija.PrikaziGlavniMeni();
            if (gameManager.trenutnafaza == GameManager.GamePhase.Val)
                Time.timeScale = 0f;
        }
        else {
            Time.timeScale = 1f;
        }
    }

    public void ZatvoriMeni() {
        menuPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}