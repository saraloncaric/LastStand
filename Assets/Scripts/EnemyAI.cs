using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 3.5f;
    [SerializeField] float _attackDamage = 10f;

    const float GoalReachDistance = 10f;
    const float AttackCooldown = 2f;

    NavMeshAgent _agent;
    Health _selfHealth;
    Transform _goal;
    float _nextAttackTime;
    bool _deathRewarded;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _selfHealth = GetComponent<Health>();
    }

    void Start()
    {
        var goalObject = GameObject.FindGameObjectWithTag("Goal");
        if (goalObject != null)
            _goal = goalObject.transform;
    }

    void Update()
    {
        TryGrantDeathReward();

        if (_selfHealth != null && _selfHealth.currentHealth <= 0f)
        {
            _agent.isStopped = true;
            return;
        }

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

    void TryGrantDeathReward()
    {
        if (_deathRewarded || _selfHealth == null)
            return;
        if (_selfHealth.currentHealth > 0f)
            return;

        _deathRewarded = true;
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.EnemyKilled();
    }
}
