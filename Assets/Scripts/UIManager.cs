using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public EconomyManager economy;
    public GameManager gameManager;

    public TextMeshProUGUI CoinsText;
    public TextMeshProUGUI FazaText;

    void Update() {
        CoinsText.text = "Coins: " + economy.coins.ToString();
        FazaText.text = "Faza: " + gameManager.trenutnafaza.ToString();
    }
}
