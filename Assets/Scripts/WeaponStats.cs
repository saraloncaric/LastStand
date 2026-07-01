using UnityEngine;

public class WeaponStats : MonoBehaviour
{
    [Header("Luk i strijela (svi vojnici)")]
    public float w1_damage = 20f;
    public float w1_projectileSpeed = 28f;
    public float w1_fireRate = 1.05f;
    public GameObject w1_projectilePrefab;

    [Header("Dijeli se između valova")]
    public GameObject bloodCirclePrefab;

    [HideInInspector] public float damage;
    [HideInInspector] public float projectileSpeed;
    [HideInInspector] public float fireRate;
    [HideInInspector] public float meleeRange;
    [HideInInspector] public bool isMelee;
    [HideInInspector] public GameObject projectilePrefab;

    void Awake()
    {
        ApplyBowStats();
    }

    public void SetWave(int wave)
    {
        ApplyBowStats();
    }

    public void ApplyRoleModifiers(bool chief)
    {
        ApplyBowStats();
        damage *= chief ? 1.1f : 0.8f;
    }

    void ApplyBowStats()
    {
        damage = w1_damage;
        projectileSpeed = w1_projectileSpeed;
        fireRate = w1_fireRate;
        projectilePrefab = w1_projectilePrefab;
        isMelee = false;
        meleeRange = 0f;
    }
}
