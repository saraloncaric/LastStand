using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DetectionLogic : MonoBehaviour
{
    [SerializeField] float detectionRadius = 15f;
    public UnityEvent OnEnemyDetected;

    SphereCollider _sphereCollider;
    bool _onCooldown;

    void Awake()
    {
        _sphereCollider = GetComponent<SphereCollider>();
        if (_sphereCollider == null)
            _sphereCollider = gameObject.AddComponent<SphereCollider>();

        _sphereCollider.isTrigger = true;
        _sphereCollider.radius = detectionRadius;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy"))
            return;

        if (_onCooldown)
            return;

        OnEnemyDetected?.Invoke();
        StartCoroutine(CooldownRoutine());
    }

    IEnumerator CooldownRoutine()
    {
        _onCooldown = true;
        yield return new WaitForSeconds(5f);
        _onCooldown = false;
    }
}
