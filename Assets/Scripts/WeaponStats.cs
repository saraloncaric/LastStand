using UnityEngine;

public class WeaponStats : MonoBehaviour
{
    [Header("Prefabi projektila (za ranged oružja)")]
    public GameObject arrowPrefab;     
    public GameObject spearProjectile;

    [Header("Damage vrijednosti")]
    public float bowDamage = 20f;
    public float daggerDamage = 15f;
    public float swordDamage = 25f;
    public float axeDamage = 35f;
    public float hammerDamage = 45f;
    public float spearDamage = 30f;

    [Header("Brzina napada")]
    public float bowFireRate = 1.05f;
    public float daggerRate = 0.6f;
    public float swordRate = 0.9f;
    public float axeRate = 1.2f;
    public float hammerRate = 1.4f;
    public float spearRate = 1.0f;

    [Header("Range")]
    public float meleeRange = 1.5f;
    public float spearRange = 2.5f;

    [HideInInspector] public float damage;
    [HideInInspector] public float fireRate;
    [HideInInspector] public float range;
    [HideInInspector] public bool isMelee;
    [HideInInspector] public GameObject projectilePrefab;
    [HideInInspector] public string currentWeaponName;

    public void ApplyWeapon(string naziv)
    {
        currentWeaponName = naziv;
        switch (naziv)
        {
            case "Luk":
                isMelee = false;
                damage = bowDamage;
                fireRate = bowFireRate;
                range = 999f;
                projectilePrefab = arrowPrefab;
                break;

            case "Bodež":
                isMelee = true;
                damage = daggerDamage;
                fireRate = daggerRate;
                range = meleeRange;
                projectilePrefab = null;
                break;

            case "Mač":
                isMelee = true;
                damage = swordDamage;
                fireRate = swordRate;
                range = meleeRange;
                projectilePrefab = null;
                break;

            case "Sjekira":
                isMelee = true;
                damage = axeDamage;
                fireRate = axeRate;
                range = meleeRange;
                projectilePrefab = null;
                break;

            case "Čekić":
                isMelee = true;
                damage = hammerDamage;
                fireRate = hammerRate;
                range = meleeRange;
                projectilePrefab = null;
                break;

            case "Koplje":
                isMelee = true; 
                damage = spearDamage;
                fireRate = spearRate;
                range = spearRange;
                projectilePrefab = null; 
                break;

            default:
                isMelee = false;
                damage = bowDamage;
                fireRate = bowFireRate;
                range = 999f;
                projectilePrefab = arrowPrefab;
                break;
        }
    }

    public void ApplyRoleModifiers(bool chief)
    {
        damage *= chief ? 1.15f : 1.0f;
    }
}
