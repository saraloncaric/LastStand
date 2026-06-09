using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 3.5f;
    [SerializeField] float _attackDamage = 8f;
    [SerializeField] AudioClip walkingSound;
    [SerializeField] AudioClip deathSound;
    [SerializeField, Range(0f, 1f)] float deathVolume = 0.5f;

    const float GoalReachDistance = 10f;
    const float AttackCooldown = 2f;

    [SerializeField] float spawnTargetDelay = 1.5f;

    NavMeshAgent _agent;
    Health _selfHealth;
    AudioSource _audioSource;
    Transform _goal;
    float _nextAttackTime;
    bool _deathHandled;
    float _spawnedAt;

    public bool IsTargetable => Time.time >= _spawnedAt + spawnTargetDelay;

    void Awake()
    {
        _spawnedAt = Time.time;
        _agent = GetComponent<NavMeshAgent>();
        _selfHealth = GetComponent<Health>();
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        EnsureHitCollider();
    }

    void EnsureHitCollider()
    {
        if (GetComponent<Collider>() != null)
            return;

        var capsule = gameObject.AddComponent<CapsuleCollider>();
        capsule.radius = _agent.radius;
        capsule.height = _agent.height;
        capsule.center = new Vector3(0f, _agent.height * 0.5f, 0f);
        capsule.isTrigger = false;
    }

    void Start()
    {
        var goalObject = GameObject.FindGameObjectWithTag("Goal");
        if (goalObject != null)
            _goal = goalObject.transform;

        if (walkingSound != null)
        {
            _audioSource.clip = walkingSound;
            _audioSource.loop = true;
            _audioSource.Play();
        }
    }

    void Update()
    {
        HandleDeath();

        if (_selfHealth != null && _selfHealth.currentHealth <= 0f)
            return;

        if (_goal == null)
            return;

        _agent.isStopped = false;
        _agent.speed = _moveSpeed;
        _agent.SetDestination(_goal.position);

        float distance = Vector3.Distance(transform.position, _goal.position);
        if (distance >= GoalReachDistance)
            return;

        if (Time.time < _nextAttackTime)
            return;

        _nextAttackTime = Time.time + AttackCooldown;

        var goalHealth = _goal.GetComponent<Health>();
        if (goalHealth != null)
            goalHealth.TakeDamage(_attackDamage);
    }

    void HandleDeath()
    {
        if (_deathHandled || _selfHealth == null)
            return;
        if (_selfHealth.currentHealth > 0f)
            return;

        _deathHandled = true;
        _agent.isStopped = true;

        if (_audioSource != null && _audioSource.isPlaying)
            _audioSource.Stop();

        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position, deathVolume);
    }

}
