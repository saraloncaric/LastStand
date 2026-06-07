using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 20f;
    public float speed = 15f;

    [Header("Crveni krug")]
    public GameObject bloodCirclePrefab;
    public float bloodCircleDuration = 3f;

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Soldier")) return;

        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);

            if (bloodCirclePrefab != null)
            {
                Vector3 pos = new Vector3(other.transform.position.x, 0.02f, other.transform.position.z);
                GameObject circle = Instantiate(bloodCirclePrefab, pos, Quaternion.Euler(90f, 0f, 0f));
                Destroy(circle, bloodCircleDuration);
            }
        }

        Destroy(gameObject);
    }
}
