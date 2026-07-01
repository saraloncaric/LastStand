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
    public int cijenaPopravka = 50;
    [Range(0.05f, 1f)] public float healPercent = 0.25f;

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
        TryPartialRepair(glavniToranj);
    }

    public void PopraviLijevo() {
        TryPartialRepair(lijeviBocniToranj);
    }

    public void PopraviDesno() {
        TryPartialRepair(desniBocniToranj);
    }

    public void PopraviZid() {
        TryPartialRepair(zid);
    }

    void TryPartialRepair(Health target) {
        if (target == null || economy == null)
            return;
        if (target.currentHealth >= target.maxHealth - 0.5f)
            return;
        if (economy.coins < cijenaPopravka)
            return;

        economy.coins -= cijenaPopravka;
        target.currentHealth = Mathf.Min(target.maxHealth, target.currentHealth + target.maxHealth * healPercent);
    }
}