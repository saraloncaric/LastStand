using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 4f;
    [SerializeField] float _attackDamage = 8f;
    [SerializeField] float _attackRange = 1.25f;
    [SerializeField] float _attackCooldown = 1.25f;
    [SerializeField] float _deathDestroyDelay = 2f;

    [Header("Sounds")]
    [SerializeField] SpatialSound walkSound;
    [SerializeField] SpatialSound deathSound;
    [SerializeField] SpatialSound attackSound;

    const float WalkVelocityThreshold = 0.15f;
    const float AnimSpeedOnThreshold = 0.25f;
    const float AnimSpeedOffThreshold = 0.12f;
    const float AttackAnimDuration = 0.85f;
    const float HitAnimDuration = 0.65f;

    static readonly int SpeedParam = Animator.StringToHash("Speed");
    static readonly int AttackTrigger = Animator.StringToHash("Attack");
    static readonly int HitTrigger = Animator.StringToHash("Hit");
    static readonly int DieTrigger = Animator.StringToHash("Die");
    static readonly int AttackStateHash = Animator.StringToHash("Attack");
    static readonly int HitStateHash = Animator.StringToHash("Hit");

    [SerializeField] float spawnTargetDelay = 1.5f;

    NavMeshAgent _agent;
    Health _selfHealth;
    AudioSource _walkSource;
    Animator _animator;
    Transform _goal;
    Collider _goalCollider;
    Health _goalHealth;
    float _nextAttackTime;
    float _combatLockUntil;
    float _scaledAttackRange;
    bool _deathHandled;
    bool _hasHitReacted;
    bool _animMoving;
    float _spawnedAt;
    float _waveSpeedMultiplier = 1f;

    public bool IsTargetable => Time.time >= _spawnedAt + spawnTargetDelay;

    public void ApplyWaveSpeedMultiplier(float multiplier)
    {
        _waveSpeedMultiplier = Mathf.Max(0.1f, multiplier);
        ApplyMoveSpeed();
    }

    float GetMoveSpeed() => _moveSpeed * Mathf.Max(1f, transform.lossyScale.y) * _waveSpeedMultiplier;

    void ApplyMoveSpeed()
    {
        if (_agent == null)
            return;

        _agent.speed = GetMoveSpeed();
        _agent.acceleration = Mathf.Max(12f, GetMoveSpeed() * 3f);
    }

    void Awake()
    {
        _spawnedAt = Time.time;
        _agent = GetComponent<NavMeshAgent>();
        _selfHealth = GetComponent<Health>();
        _animator = GetComponent<Animator>();
        _walkSource = GetComponent<AudioSource>();
        if (_walkSource == null)
            _walkSource = gameObject.AddComponent<AudioSource>();

        EnsureHitCollider();

        _attackDamage *= 0.8f;

        _scaledAttackRange = _attackRange;

        if (_agent != null)
        {
            _agent.stoppingDistance = Mathf.Max(0.05f, _scaledAttackRange - _agent.radius * 0.9f);
            ApplyMoveSpeed();
        }

        if (_animator != null)
            _animator.applyRootMotion = false;
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
        {
            _goal = goalObject.transform;
            _goalCollider = goalObject.GetComponent<Collider>();
            if (_goalCollider == null)
                _goalCollider = goalObject.GetComponentInChildren<Collider>();
            _goalHealth = goalObject.GetComponent<Health>();
        }
    }

    void Update()
    {
        if (_deathHandled)
            return;

        HandleDeath();
        if (_deathHandled)
            return;

        UpdateWalkSound();

        if (_goal == null)
            return;

        if (_agent == null || !_agent.isOnNavMesh)
        {
            UpdateAnimatorSpeed();
            return;
        }

        if (Time.time < _combatLockUntil)
        {
            _agent.isStopped = true;
            FaceGoal();
            return;
        }

        float distance = GetHorizontalDistanceToGoal();
        bool inAttackRange = distance <= _scaledAttackRange;

        if (inAttackRange)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
            FaceGoal();

            if (Time.time < _nextAttackTime)
            {
                UpdateAnimatorSpeed();
                return;
            }

            _nextAttackTime = Time.time + _attackCooldown;
            TriggerAttackAnimation();

            if (_goalHealth != null)
                _goalHealth.TakeDamage(_attackDamage);

            if (attackSound != null && attackSound.IsValid)
                GameAudio.PlayAtPoint(attackSound, transform.position, SfxCategory.EnemyAttack);
            return;
        }

        _agent.isStopped = false;
        ApplyMoveSpeed();
        _agent.SetDestination(_goal.position);
        UpdateAnimatorSpeed();
    }

    float GetHorizontalDistanceToGoal()
    {
        if (_goal == null)
            return float.MaxValue;

        Vector3 origin = transform.position + Vector3.up * (_agent != null ? _agent.height * 0.35f : 1f);
        Vector3 toGoal = _goal.position - transform.position;
        toGoal.y = 0f;
        float flatDist = toGoal.magnitude;
        if (flatDist > 0.05f)
        {
            Vector3 dir = toGoal / flatDist;
            if (Physics.Raycast(origin, dir, out RaycastHit hit, flatDist + 1f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
                && IsGoalCollider(hit.collider))
            {
                return hit.distance;
            }
        }

        Vector3 self = transform.position;
        self.y = 0f;

        if (_goalCollider != null)
        {
            Vector3 closest = _goalCollider.bounds.ClosestPoint(transform.position);
            closest.y = 0f;
            return Vector3.Distance(self, closest);
        }

        Vector3 goal = _goal.position;
        goal.y = 0f;
        return Vector3.Distance(self, goal);
    }

    bool IsGoalCollider(Collider col)
    {
        if (col == null)
            return false;
        if (col == _goalCollider)
            return true;
        if (_goal != null && col.transform.IsChildOf(_goal))
            return true;
        return col.CompareTag("Goal");
    }

    void FaceGoal()
    {
        if (_goal == null)
            return;

        Vector3 toGoal = _goal.position - transform.position;
        toGoal.y = 0f;
        if (toGoal.sqrMagnitude < 0.01f)
            return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(toGoal.normalized),
            Time.deltaTime * 12f);
    }

    void UpdateAnimatorSpeed()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null)
            return;

        if (Time.time < _combatLockUntil)
            return;

        float speed = 0f;
        if (_agent != null && _agent.isOnNavMesh && !_agent.isStopped)
            speed = _agent.velocity.magnitude;

        if (!_animMoving && speed > AnimSpeedOnThreshold)
            _animMoving = true;
        else if (_animMoving && speed < AnimSpeedOffThreshold)
            _animMoving = false;

        _animator.SetFloat(SpeedParam, _animMoving ? 1f : 0f);
    }

    void TriggerAttackAnimation()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null)
            return;

        _combatLockUntil = Mathf.Max(_combatLockUntil, Time.time + AttackAnimDuration);
        _animMoving = false;
        _animator.SetFloat(SpeedParam, 0f);
        _animator.ResetTrigger(AttackTrigger);
        _animator.SetTrigger(AttackTrigger);
        _animator.Play(AttackStateHash, 0, 0f);
    }

    public void NotifyFirstHit()
    {
        if (_hasHitReacted || _deathHandled)
            return;

        _hasHitReacted = true;

        if (_animator == null || _animator.runtimeAnimatorController == null)
            return;

        _combatLockUntil = Mathf.Max(_combatLockUntil, Time.time + HitAnimDuration);
        _animMoving = false;
        _animator.SetFloat(SpeedParam, 0f);
        _animator.ResetTrigger(HitTrigger);
        _animator.SetTrigger(HitTrigger);
        _animator.Play(HitStateHash, 0, 0f);
    }

    void UpdateWalkSound()
    {
        if (!walkSound.IsValid)
            return;

        bool isMoving = !_agent.isStopped && _agent.velocity.sqrMagnitude > WalkVelocityThreshold * WalkVelocityThreshold;

        if (isMoving)
            GameAudio.PlayLoop(_walkSource, walkSound, SfxCategory.Footstep);
        else
            GameAudio.StopLoop(_walkSource);
    }

    public void NotifyDeath()
    {
        if (_deathHandled)
            return;

        _deathHandled = true;
        if (_agent != null)
        {
            _agent.isStopped = true;
            _agent.enabled = false;
        }

        GameAudio.StopLoop(_walkSource);
        GameAudio.PlayAtPoint(deathSound, transform.position, SfxCategory.Death);

        if (_animator != null && _animator.runtimeAnimatorController != null)
        {
            _animator.SetFloat(SpeedParam, 0f);
            _animator.SetTrigger(DieTrigger);
            StartCoroutine(DestroyAfterDeathAnimation());
        }
        else
        {
            Destroy(gameObject, _deathDestroyDelay);
        }
    }

    IEnumerator DestroyAfterDeathAnimation()
    {
        yield return null;

        float wait = _deathDestroyDelay;
        if (_animator != null)
        {
            var state = _animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Death") && state.length > 0f)
                wait = state.length;
        }

        yield return new WaitForSeconds(wait);
        Destroy(gameObject);
    }

    void HandleDeath()
    {
        if (_deathHandled || _selfHealth == null)
            return;
        if (_selfHealth.currentHealth > 0f)
            return;

        NotifyDeath();
    }
}
