using UnityEngine;

public class SoldierAI : MonoBehaviour

{

    public GameObject projectilePrefab;

    public Transform firePoint;

    public float detectionRange = 15f;

    private WeaponStats stats;

    private float fireCooldown = 0f;

    void Start()

    {

        stats = GetComponent();

    }

    void Update()

    {

        GameObject target = FindClosestEnemy();

        if (target == null) return;

        // okreni se prema neprijatelju

        transform.LookAt(target.transform);

        // pucaj

        fireCooldown -= Time.deltaTime;

        if (fireCooldown <= 0f)

        {

            Shoot(target);

            fireCooldown = stats.fireRate;

        }

    }

    GameObject FindClosestEnemy()

    {

        GameObject\[\] enemies = GameObject.FindGameObjectsWithTag("Enemy");

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

    void Shoot(GameObject target)

    {

        if (projectilePrefab == null || firePoint == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        Projectile p = proj.GetComponent();

        if (p != null)

        {

            p.damage = stats.damage;

            p.speed = stats.projectileSpeed;

        }

    }

}