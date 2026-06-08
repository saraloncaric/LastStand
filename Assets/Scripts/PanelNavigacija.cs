using UnityEngine;

public class PanelNavigacija : MonoBehaviour
{
    public GameObject glavniMeniPanel;
    public GameObject healthPanel;
    public GameObject lokacijePanel;
    public GameObject vojniciPanel;

    void Start() {
        glavniMeniPanel.SetActive(false);
        healthPanel.SetActive(false);
        lokacijePanel.SetActive(false);
        vojniciPanel.SetActive(false);
    }

    public void PrikaziGlavniMeni() {
        glavniMeniPanel.SetActive(true);
        healthPanel.SetActive(false);
        lokacijePanel.SetActive(false);
        vojniciPanel.SetActive(false);
    }

    public void PrikaziHealth() {
        glavniMeniPanel.SetActive(false);
        healthPanel.SetActive(true);
        lokacijePanel.SetActive(false);
        vojniciPanel.SetActive(false);
    }

    public void PrikaziVojnike() {
        glavniMeniPanel.SetActive(false);
        healthPanel.SetActive(false);
        lokacijePanel.SetActive(false);
        vojniciPanel.SetActive(true);
    }
}