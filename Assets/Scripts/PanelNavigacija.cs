using UnityEngine;
using UnityEngine.UI;

public class PanelNavigacija : MonoBehaviour
{
    public GameObject glavniMeniPanel;
    public GameObject healthPanel;
    public GameObject lokacijePanel;
    public GameObject vojniciPanel;

    Transform _vojniciBackButton;

    void Awake()
    {
        if (vojniciPanel != null)
            _vojniciBackButton = vojniciPanel.transform.Find("Natrag");
    }

    void Start()
    {
        glavniMeniPanel.SetActive(false);
        healthPanel.SetActive(false);
        lokacijePanel.SetActive(false);
        vojniciPanel.SetActive(false);
    }

    public void PrikaziGlavniMeni()
    {
        glavniMeniPanel.SetActive(true);
        healthPanel.SetActive(false);
        lokacijePanel.SetActive(false);
        vojniciPanel.SetActive(false);
        SetVojniciBackButton(false);
        SetPanelBlocksRaycasts(vojniciPanel, true);
    }

    public void PrikaziHealth()
    {
        glavniMeniPanel.SetActive(false);
        healthPanel.SetActive(true);
        lokacijePanel.SetActive(false);
        vojniciPanel.SetActive(false);
        SetVojniciBackButton(false);
    }

    public void PrikaziVojnike()
    {
        glavniMeniPanel.SetActive(false);
        healthPanel.SetActive(false);
        lokacijePanel.SetActive(false);
        vojniciPanel.SetActive(true);
        SetVojniciBackButton(true);
        SetPanelBlocksRaycasts(vojniciPanel, false);
    }

    void SetVojniciBackButton(bool visible)
    {
        if (_vojniciBackButton != null)
            _vojniciBackButton.gameObject.SetActive(visible);
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
