using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isTower = false;

    [Header("Efekt smrti")]
    public GameObject bloodCirclePrefab;
    public float bloodCircleDuration = 3f;

    bool _rewardGranted;

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
                GrantDeathReward();
                SpawnBloodCircle();
                Destroy(gameObject);
            }
        }
    }

    void GrantDeathReward()
    {
        if (_rewardGranted)
            return;

        _rewardGranted = true;

        LootSystem loot = GetComponent<LootSystem>();
        if (loot != null)
        {
            loot.GiveLoot();
            return;
        }

        if (EconomyManager.Instance != null)
            EconomyManager.Instance.EnemyKilled();
    }

    void SpawnBloodCircle()
    {
        if (bloodCirclePrefab == null) return;
        Vector3 pos = new Vector3(transform.position.x, 0.02f, transform.position.z);
        GameObject circle = Instantiate(bloodCirclePrefab, pos, Quaternion.Euler(90f, 0f, 0f));
        Destroy(circle, bloodCircleDuration);
    }
}
