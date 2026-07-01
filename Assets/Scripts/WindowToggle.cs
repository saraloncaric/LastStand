using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class WindowToggle : MonoBehaviour
{
    public GameObject window;

    public bool startHidden = true;

    void Start()
    {
        if (window != null && startHidden)
            window.SetActive(false);

        GetComponent<Button>().onClick.AddListener(Toggle);
    }

    public void Toggle()
    {
        if (window == null)
            return;

        bool show = !window.activeSelf;
        window.SetActive(show);

        if (show)
            window.transform.SetAsLastSibling();
    }

    public void Open()
    {
        if (window == null) return;
        window.SetActive(true);
        window.transform.SetAsLastSibling();
    }

    public void Close()
    {
        if (window != null)
            window.SetActive(false);
    }
}
