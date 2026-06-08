using UnityEngine;

public class SoldierAI : MonoBehaviour
{
    [Header("Referenca")]
    public Transform firePoint;
    public float detectionRange = 15f;

    [Header("Zvuk i animacija")]
    public AudioClip attackSound;
    public string attackAnimTrigger = "Attack";

    private WeaponStats weaponStats;
    private float fireCooldown = 0f;
    private AudioSource audioSource;
    private Animator animator;

    void Start()
    {
        weaponStats = GetComponent<WeaponStats>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        if (weaponStats != null)
        {
            GameManager.OnWaveChanged += weaponStats.SetWave;
        }
    }

    void OnDestroy()
    {
        if (weaponStats != null)
        {
            GameManager.OnWaveChanged -= weaponStats.SetWave;
        }
    }

    void Update()
    {
        if (weaponStats == null) return;

        GameObject target = FindClosestEnemy();
        if (target == null) return;

        transform.LookAt(target.transform);

        fireCooldown -= Time.deltaTime;

        if (fireCooldown <= 0f)
        {
            Attack(target);
            fireCooldown = weaponStats.fireRate;
        }
    }

    void Attack(GameObject target)
    {
        if (animator != null)
            animator.SetTrigger(attackAnimTrigger);

        if (audioSource != null && attackSound != null)
            audioSource.PlayOneShot(attackSound);

        if (weaponStats.isMelee)
        {
            float dist = Vector3.Distance(transform.position, target.transform.position);

            if (dist <= weaponStats.meleeRange)
            {
                Health health = target.GetComponent<Health>();

                if (health != null)
                    health.TakeDamage(weaponStats.damage);
            }
        }
        else
        {
            Shoot(target);
        }
    }

    void Shoot(GameObject target)
    {
        if (weaponStats.projectilePrefab == null || firePoint == null) return;

        GameObject proj = Instantiate(
            weaponStats.projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        Projectile p = proj.GetComponent<Projectile>();

        if (p != null)
        {
            p.damage = weaponStats.damage;
            p.speed = weaponStats.projectileSpeed;
        }
    }

    GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float minDist = detectionRange;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }

        return closest;
    }
}
