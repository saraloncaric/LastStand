using System.Collections.Generic;
using UnityEngine;

public class SoldierAI : MonoBehaviour
{
    [Header("Referenca")]
    public Transform firePoint;
    public float optimalRange = 36f;
    public float detectionRange = 68f;

    [Header("Preciznost na daljinu")]
    [Range(0f, 1f)] public float minHitChanceAtMaxRange = 0.35f;
    [Range(1f, 15f)] public float missSpreadMinDegrees = 4f;
    [Range(1f, 20f)] public float missSpreadMaxDegrees = 11f;

    [Header("Luk")]
    public GameObject bowPrefab;
    public Vector3 bowLocalPosition = new Vector3(0.15f, 1.05f, 0.25f);
    public Vector3 bowLocalEuler = new Vector3(0f, -90f, 0f);
    public float bowScale = 0.75f;

    [Header("Zvuk i animacija")]
    public SpatialSound attackSound;
    public string attackAnimTrigger = "Attack";

    static readonly Vector3 DefaultFirePointLocal = new Vector3(0.25f, 1.2f, 0.4f);

    WeaponStats weaponStats;
    float _baseOptimalRange;
    float _baseMaxRange;
    float _optimalRange;
    float _maxRange;
    bool _isChief;
    float fireCooldown;
    AudioSource audioSource;
    Animator animator;

    void Awake()
    {
        _baseOptimalRange = optimalRange;
        _baseMaxRange = detectionRange;
        ApplyRangeModifiers();
    }

    public void ApplyRoleModifiers(bool chief)
    {
        _isChief = chief;
        ApplyRangeModifiers();
    }

    void ApplyRangeModifiers()
    {
        float roleMult = _isChief ? 1.2f : 0.9f;
        _optimalRange = _baseOptimalRange * roleMult;
        _maxRange = _baseMaxRange * roleMult;
    }

    void Start()
    {
        weaponStats = GetComponent<WeaponStats>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        animator = GetComponent<Animator>();
        if (weaponStats != null)
            GameManager.OnWaveChanged += OnWaveChanged;

        EnsureFirePoint();
        EquipBow();
    }

    void OnWaveChanged(int wave)
    {
        if (weaponStats == null)
            return;
        weaponStats.SetWave(wave);
        weaponStats.ApplyRoleModifiers(_isChief);
    }

    void OnDestroy()
    {
        GameManager.OnWaveChanged -= OnWaveChanged;
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

        if (attackSound != null && attackSound.IsValid)
            GameAudio.PlayOneShot(audioSource, attackSound, SfxCategory.SoldierAttack);

        Shoot(target);
    }

    void Shoot(GameObject target)
    {
        if (target == null || weaponStats == null) return;
        if (!IsValidTarget(target) || !IsInRange(target)) return;

        EnsureFirePoint();

        float distance = FlatDistance(transform.position, target.transform.position);
        float hitChance = GetHitChance(distance);
        bool hit = Random.value <= hitChance;

        Quaternion shotRotation = GetShotRotation(target.transform, distance, hit);
        ApplyAimRotation(target.transform, shotRotation);

        if (hit)
        {
            Health health = target.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(weaponStats.damage);
        }

        if (weaponStats.projectilePrefab == null || firePoint == null) return;

        GameObject proj = Instantiate(weaponStats.projectilePrefab, firePoint.position, shotRotation);
        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.damage = 0f;
            projectile.speed = weaponStats.projectileSpeed;
        }
    }

    float GetHitChance(float distance)
    {
        if (distance <= _optimalRange)
            return 1f;

        if (distance >= _maxRange)
            return 0f;

        float t = (distance - _optimalRange) / (_maxRange - _optimalRange);
        return Mathf.Lerp(1f, minHitChanceAtMaxRange, t);
    }

    Quaternion GetShotRotation(Transform target, float distance, bool hit)
    {
        Vector3 fireAim = target.position;
        fireAim.y = firePoint.position.y;
        Quaternion perfect = Quaternion.LookRotation((fireAim - firePoint.position).normalized);

        if (hit)
            return perfect;

        float t = Mathf.InverseLerp(_optimalRange, _maxRange, distance);
        float spread = Mathf.Lerp(missSpreadMinDegrees, missSpreadMaxDegrees, t);
        return perfect * Quaternion.Euler(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            0f);
    }

    void ApplyAimRotation(Transform target, Quaternion shotRotation)
    {
        Vector3 aimPoint = target.position;
        aimPoint.y = transform.position.y;
        transform.rotation = Quaternion.LookRotation((aimPoint - transform.position).normalized);

        if (firePoint != null)
            firePoint.rotation = shotRotation;
    }

    GameObject FindPriorityTarget()
    {
        var threats = new List<GameObject>();
        var seen = new HashSet<GameObject>();

        Collider[] hits = Physics.OverlapSphere(transform.position, _maxRange);
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

        GameObject closest = threats[0];
        float closestDist = FlatDistance(transform.position, closest.transform.position);

        for (int i = 1; i < threats.Count; i++)
        {
            float dist = FlatDistance(transform.position, threats[i].transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = threats[i];
            }
        }

        return closest;
    }

    bool IsInRange(GameObject enemy)
    {
        return FlatDistance(transform.position, enemy.transform.position) <= _maxRange;
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
