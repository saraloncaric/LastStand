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

        bool wasAlive = currentHealth > 0f;
        currentHealth -= amount;

        if (wasAlive && currentHealth > 0f)
        {
            EnemyAI enemy = GetComponent<EnemyAI>();
            if (enemy != null)
                enemy.NotifyFirstHit();
        }

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

                EnemyAI enemy = GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.NotifyDeath();
                    return;
                }

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
