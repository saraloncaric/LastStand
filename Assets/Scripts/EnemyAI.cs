using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 3.5f;
    [SerializeField] float _attackDamage = 10f;
    [SerializeField] AudioClip walkingSound;
    [SerializeField] AudioClip deathSound;
    [SerializeField, Range(0f, 1f)] float deathVolume = 0.5f;

    const float GoalReachDistance = 10f;
    const float AttackCooldown = 2f;

    NavMeshAgent _agent;
    Health _selfHealth;
    AudioSource _audioSource;
    Transform _goal;
    float _nextAttackTime;
    bool _deathRewarded;
    bool _deathHandled;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _selfHealth = GetComponent<Health>();
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
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
        TryGrantDeathReward();
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

    void TryGrantDeathReward()
    {
        if (_deathRewarded || _selfHealth == null)
            return;
        if (_selfHealth.currentHealth > 0f)
            return;

        _deathRewarded = true;

        var loot = GetComponent<LootSystem>();
        if (loot != null)
            loot.GiveLoot();
    }
}
