using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 20f;
    public float speed = 15f;

    bool _hasHit;

    void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        foreach (Collider col in GetComponents<Collider>())
        {
            if (!col.isTrigger)
                col.enabled = false;
        }
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    void OnCollisionEnter(Collision collision)
    {
        TryHit(collision.collider);
    }

    void TryHit(Collider other)
    {
        if (_hasHit || other == null)
            return;

        if (other.CompareTag("Soldier"))
            return;

        if (!IsEnemy(other))
            return;

        _hasHit = true;

        Health health = other.GetComponentInParent<Health>();
        if (health != null)
            health.TakeDamage(damage);

        Destroy(gameObject);
    }

    static bool IsEnemy(Collider other)
    {
        if (other.CompareTag("Enemy"))
            return true;

        Transform root = other.transform.root;
        return root != null && root.CompareTag("Enemy");
    }
}
