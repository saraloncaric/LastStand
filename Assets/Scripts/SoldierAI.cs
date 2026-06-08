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

                SpawnBloodCircle(target.transform.position);
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
            p.bloodCirclePrefab = weaponStats.bloodCirclePrefab;
        }
    }

    void SpawnBloodCircle(Vector3 position)
    {
        if (weaponStats.bloodCirclePrefab == null) return;

        Vector3 pos = new Vector3(position.x, 0.02f, position.z);

        GameObject circle = Instantiate(
            weaponStats.bloodCirclePrefab,
            pos,
            Quaternion.Euler(90f, 0f, 0f)
        );

        Destroy(circle, 3f);
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
