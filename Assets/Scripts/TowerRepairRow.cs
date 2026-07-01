using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerRepairRow : MonoBehaviour
{
    public Health health;
    public EconomyManager economy;
    public int repairCost = 50;
    [Range(0.05f, 1f)] public float healPercent = 0.25f;

    Button _button;
    Image _buttonImage;
    TextMeshProUGUI _buttonLabel;

    void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
        {
            _buttonImage = _button.GetComponent<Image>();
            _buttonLabel = _button.GetComponentInChildren<TextMeshProUGUI>();
            _button.onClick.AddListener(TryRepair);
        }
    }

    void Update()
    {
        RefreshButton();
    }

    void RefreshButton()
    {
        if (_button == null)
            _button = GetComponent<Button>();
        if (_buttonLabel == null)
            _buttonLabel = GetComponentInChildren<TextMeshProUGUI>();
        if (_buttonImage == null && _button != null)
            _buttonImage = _button.GetComponent<Image>();

        if (_button == null || health == null)
            return;

        bool full = health.currentHealth >= health.maxHealth - 0.5f;
        bool canAfford = economy != null && economy.coins >= repairCost;
        bool canRepair = !full && canAfford;

        _button.interactable = canRepair;

        if (_buttonLabel != null)
        {
            int pct = Mathf.RoundToInt(healPercent * 100f);
            _buttonLabel.text = full ? "Pun HP" : "Popravi (+" + pct + "% / " + repairCost + ")";
        }

        if (_buttonImage != null)
        {
            if (full)
                _buttonImage.color = new Color(0.22f, 0.22f, 0.22f, 1f);
            else if (!canAfford)
                _buttonImage.color = new Color(0.35f, 0.25f, 0.22f, 1f);
            else
                _buttonImage.color = new Color(0.28f, 0.4f, 0.3f, 1f);
        }
    }

    void TryRepair()
    {
        if (health == null || economy == null)
            return;
        if (health.currentHealth >= health.maxHealth - 0.5f)
            return;
        if (economy.coins < repairCost)
            return;

        economy.coins -= repairCost;
        float heal = health.maxHealth * healPercent;
        health.currentHealth = Mathf.Min(health.maxHealth, health.currentHealth + heal);
    }
}
