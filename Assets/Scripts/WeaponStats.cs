using UnityEngine;

public class WeaponStats : MonoBehaviour
{
    [Header("Val 1 - Luk i strijela")]
    public float w1_damage = 15f;
    public float w1_projectileSpeed = 20f;
    public float w1_fireRate = 1.5f;
    public GameObject w1_projectilePrefab;

    [Header("Val 2 - Jači luk")]
    public float w2_damage = 25f;
    public float w2_projectileSpeed = 25f;
    public float w2_fireRate = 1.2f;
    public GameObject w2_projectilePrefab;

    [Header("Val 3 - Mač / Sjekira")]
    public float w3_damage = 40f;
    public float w3_fireRate = 1.0f;
    public float w3_meleeRange = 2.5f;

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
        SetWave(1);
    }

    public void SetWave(int wave)
    {
        switch (wave)
        {
            case 1:
                damage = w1_damage;
                projectileSpeed = w1_projectileSpeed;
                fireRate = w1_fireRate;
                projectilePrefab = w1_projectilePrefab;
                isMelee = false;
                break;

            case 2:
                damage = w2_damage;
                projectileSpeed = w2_projectileSpeed;
                fireRate = w2_fireRate;
                projectilePrefab = w2_projectilePrefab;
                isMelee = false;
                break;

            case 3:
                damage = w3_damage;
                fireRate = w3_fireRate;
                meleeRange = w3_meleeRange;
                projectilePrefab = null;
                isMelee = true;
                break;
        }

        Debug.Log($"[WeaponStats] Val {wave}: isMelee={isMelee}, damage={damage}");
    }
}
