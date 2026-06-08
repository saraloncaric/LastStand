using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject menuPanel;
    public PanelNavigacija panelNavigacija;

    void Start() {
        menuPanel.SetActive(false);
    }

    public void ToggleMenu() {
        bool trebaOtvoriti = !menuPanel.activeSelf;
        menuPanel.SetActive(trebaOtvoriti);

        if (trebaOtvoriti) {
            panelNavigacija.PrikaziGlavniMeni();
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