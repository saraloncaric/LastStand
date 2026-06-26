using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 3.5f;
    [SerializeField] float _attackDamage = 8f;

    [Header("Sounds")]
    [SerializeField] SpatialSound walkSound;
    [SerializeField] SpatialSound deathSound;
    [SerializeField] SpatialSound attackSound;

    const float GoalReachDistance = 10f;
    const float AttackCooldown = 2f;
    const float WalkVelocityThreshold = 0.15f;

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
    }

    void Update()
    {
        HandleDeath();

        if (_selfHealth != null && _selfHealth.currentHealth <= 0f)
            return;

        UpdateWalkSound();

        if (_goal == null)
            return;

        if (_agent == null || !_agent.isOnNavMesh)
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
        {
            goalHealth.TakeDamage(_attackDamage);
            GameAudio.PlayOneShot(_audioSource, attackSound);
        }
    }

    void UpdateWalkSound()
    {
        if (!walkSound.IsValid)
            return;

        bool isMoving = !_agent.isStopped && _agent.velocity.sqrMagnitude > WalkVelocityThreshold * WalkVelocityThreshold;

        if (isMoving)
            GameAudio.PlayLoop(_audioSource, walkSound);
        else
            GameAudio.StopLoop(_audioSource);
    }

    void HandleDeath()
    {
        if (_deathHandled || _selfHealth == null)
            return;
        if (_selfHealth.currentHealth > 0f)
            return;

        _deathHandled = true;
        _agent.isStopped = true;
        GameAudio.StopLoop(_audioSource);

        GameAudio.PlayAtPoint(deathSound, transform.position);
    }
}
