using System.Collections.Generic;
using UnityEngine;

public class SoldierAI : MonoBehaviour
{
    [Header("Referenca")]
    public Transform firePoint;
    public float detectionRange = 40f;

    [Header("Luk")]
    public GameObject bowPrefab;
    public Vector3 bowLocalPosition = new Vector3(0.15f, 1.05f, 0.25f);
    public Vector3 bowLocalEuler = new Vector3(0f, -90f, 0f);
    public float bowScale = 0.75f;

    [Header("Zvuk i animacija")]
    public AudioClip attackSound;
    public string attackAnimTrigger = "Attack";

    static readonly Vector3 DefaultFirePointLocal = new Vector3(0.25f, 1.2f, 0.4f);

    WeaponStats weaponStats;
    float fireCooldown;
    AudioSource audioSource;
    Animator animator;
    Transform goalTransform;

    void Start()
    {
        weaponStats = GetComponent<WeaponStats>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        if (weaponStats != null)
            GameManager.OnWaveChanged += weaponStats.SetWave;

        GameObject goal = GameObject.FindGameObjectWithTag("Goal");
        if (goal != null)
            goalTransform = goal.transform;

        EnsureFirePoint();
        EquipBow();
    }

    void OnDestroy()
    {
        if (weaponStats != null)
            GameManager.OnWaveChanged -= weaponStats.SetWave;
    }

    void EnsureFirePoint()
    {
        if (firePoint == null)
        {
            firePoint = CreateFirePoint();
            return;
        }

        if (firePoint.parent != transform || firePoint.localPosition.magnitude > 3f)
        {
            firePoint.SetParent(transform, false);
            firePoint.localPosition = DefaultFirePointLocal;
            firePoint.localRotation = Quaternion.identity;
        }
    }

    Transform CreateFirePoint()
    {
        var point = new GameObject("FirePoint").transform;
        point.SetParent(transform, false);
        point.localPosition = DefaultFirePointLocal;
        point.localRotation = Quaternion.identity;
        return point;
    }

    void EquipBow()
    {
        if (bowPrefab == null)
            return;

        GameObject bow = Instantiate(bowPrefab, transform);
        bow.transform.localPosition = bowLocalPosition;
        bow.transform.localRotation = Quaternion.Euler(bowLocalEuler);
        bow.transform.localScale = Vector3.one * bowScale;
    }

    void Update()
    {
        if (weaponStats == null) return;

        GameObject target = FindPriorityTarget();
        if (target == null) return;

        AimAtTarget(target.transform);

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            Attack(target);
            fireCooldown = weaponStats.fireRate;
        }
    }

    void AimAtTarget(Transform target)
    {
        Vector3 aimPoint = target.position;
        aimPoint.y = transform.position.y;
        transform.LookAt(aimPoint);

        if (firePoint != null)
        {
            Vector3 fireAim = target.position;
            fireAim.y = firePoint.position.y;
            firePoint.LookAt(fireAim);
        }
    }

    void Attack(GameObject target)
    {
        if (!IsInRange(target))
            return;

        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetTrigger(attackAnimTrigger);
        if (audioSource != null && attackSound != null)
            audioSource.PlayOneShot(attackSound);

        Shoot(target);
    }

    void Shoot(GameObject target)
    {
        if (target == null || weaponStats == null) return;
        if (!IsValidTarget(target) || !IsInRange(target)) return;

        EnsureFirePoint();
        AimAtTarget(target.transform);

        Health health = target.GetComponent<Health>();
        if (health != null)
            health.TakeDamage(weaponStats.damage);

        if (weaponStats.projectilePrefab == null || firePoint == null) return;

        GameObject proj = Instantiate(weaponStats.projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.damage = 0f;
            projectile.speed = weaponStats.projectileSpeed;
        }
    }

    GameObject FindPriorityTarget()
    {
        var threats = new List<GameObject>();
        var seen = new HashSet<GameObject>();

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);
        foreach (Collider hit in hits)
        {
            GameObject enemy = ResolveEnemy(hit.gameObject);
            if (enemy == null || !IsValidTarget(enemy) || !IsInRange(enemy))
                continue;
            if (seen.Add(enemy))
                threats.Add(enemy);
        }

        if (threats.Count == 0)
        {
            foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                if (!IsValidTarget(enemy) || !IsInRange(enemy))
                    continue;
                if (seen.Add(enemy))
                    threats.Add(enemy);
            }
        }

        if (threats.Count == 0)
            return null;

        threats.Sort((a, b) => GetThreatScore(a).CompareTo(GetThreatScore(b)));

        int pick = Mathf.Abs(transform.GetInstanceID()) % threats.Count;
        return threats[pick];
    }

    float GetThreatScore(GameObject enemy)
    {
        if (goalTransform != null)
            return FlatDistance(enemy.transform.position, goalTransform.position);

        return FlatDistance(transform.position, enemy.transform.position);
    }

    bool IsInRange(GameObject enemy)
    {
        return FlatDistance(transform.position, enemy.transform.position) <= detectionRange;
    }

    bool IsValidTarget(GameObject enemy)
    {
        if (enemy == null || !enemy.activeInHierarchy)
            return false;

        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        if (enemyAI != null && !enemyAI.IsTargetable)
            return false;

        Health health = enemy.GetComponent<Health>();
        return health != null && health.currentHealth > 0f;
    }

    static GameObject ResolveEnemy(GameObject obj)
    {
        if (obj.CompareTag("Enemy"))
            return obj;

        Transform root = obj.transform.root;
        if (root != null && root.CompareTag("Enemy"))
            return root.gameObject;

        return null;
    }

    static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
