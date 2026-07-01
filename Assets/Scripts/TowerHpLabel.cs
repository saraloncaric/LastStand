using UnityEngine;
using TMPro;

public class TowerHpLabel : MonoBehaviour
{
    public Health health;
    public TMP_Text text;
    public string naziv;

    void Update()
    {
        if (health == null || text == null)
            return;

        text.text = naziv + ": " + Mathf.CeilToInt(health.currentHealth) + " / " + Mathf.CeilToInt(health.maxHealth) + " HP";
    }
}
