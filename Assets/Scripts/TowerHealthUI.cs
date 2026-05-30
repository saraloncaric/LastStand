using UnityEngine;
using TMPro;

public class TowerHealthUI : MonoBehaviour
{
    public Health glavniToranj;
    public Health lijeviBocniToranj;
    public Health desniBocniToranj;
    public Health zid;

    public TextMeshProUGUI glavniToranjText;
    public TextMeshProUGUI lijevoText;
    public TextMeshProUGUI desnoText;
    public TextMeshProUGUI zidText;

    public EconomyManager economy;
    public int cijenaPopravka = 4;

    void Update() {
        if (glavniToranj != null) {
            glavniToranjText.text = "Glavni toranj: " + glavniToranj.currentHealth + "/" + glavniToranj.maxHealth;
        }
        if (lijeviBocniToranj != null) {
            lijevoText.text = "Lijevi toranj: " + lijeviBocniToranj.currentHealth + "/" + lijeviBocniToranj.maxHealth;
        }
        if (desniBocniToranj != null) {
            desnoText.text = "Desni toranj: " + desniBocniToranj.currentHealth + "/" + desniBocniToranj.maxHealth;
        }
        if (zid != null) {
            zidText.text = "Zid: " + zid.currentHealth + "/" + zid.maxHealth;
        }
    }

    public void PopraviGlavniToranj() {
        if (economy.coins < cijenaPopravka) { 
            Debug.Log("Nema dovoljno coinsa!"); 
            return; 
        }
        economy.coins -= cijenaPopravka;
        glavniToranj.currentHealth = glavniToranj.maxHealth;
    }

    public void PopraviLijevo() {
        if (economy.coins < cijenaPopravka) { 
            Debug.Log("Nema dovoljno coinsa!"); 
            return; 
        }
        economy.coins -= cijenaPopravka;
        lijeviBocniToranj.currentHealth = lijeviBocniToranj.maxHealth;
    }

    public void PopraviDesno() {
        if (economy.coins < cijenaPopravka) { 
            Debug.Log("Nema dovoljno coinsa!"); 
            return; 
        }
        economy.coins -= cijenaPopravka;
        desniBocniToranj.currentHealth = desniBocniToranj.maxHealth;
    }

    public void PopraviZid() {
        if (economy.coins < cijenaPopravka) { 
            Debug.Log("Nema dovoljno coinsa!"); 
            return; 
        }
        economy.coins -= cijenaPopravka;
        zid.currentHealth = zid.maxHealth;
    }
}