using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isTower = false;

    [Header("Efekt smrti")]
    public GameObject bloodCirclePrefab;
    public float bloodCircleDuration = 3f;

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
            if (isTower)
            {
                GameManager.Instance.TriggerGameOver();
            }
            else
            {
                SpawnBloodCircle();
                Destroy(gameObject);
            }
        }
    }

    void SpawnBloodCircle()
    {
        if (bloodCirclePrefab == null) return;
        Vector3 pos = new Vector3(transform.position.x, 0.02f, transform.position.z);
        GameObject circle = Instantiate(bloodCirclePrefab, pos, Quaternion.Euler(90f, 0f, 0f));
        Destroy(circle, bloodCircleDuration);
    }
}