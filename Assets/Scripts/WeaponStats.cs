using UnityEngine;

public class Projectile : MonoBehaviour

{

    public float damage = 20f;

    public float speed = 15f;

    void Update()

    {

        transform.Translate(Vector3.forward \ speed \ Time.deltaTime);

    }

    void OnTriggerEnter(Collider other)

    {

        Health health = other.GetComponent();

        if (health != null)

            health.TakeDamage(damage);

        Destroy(gameObject);

    }

}
