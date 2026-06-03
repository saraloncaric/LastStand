using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isTower = false;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;

        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            if(isTower) {
                GameManager.Instance.TriggerGameOver();
            } else {
                Destroy(gameObject);
            }
        }
    }
}