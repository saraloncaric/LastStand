using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    public int coins = 50;

    void Awake() {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("EconomyManager: više instanci u sceni.");
            return;
        }
        Instance = this;
    }

    void OnDestroy() {
        if (Instance == this)
            Instance = null;
    }
    public void EnemyKilled() {
        coins += 4;
        Debug.Log("Zaradio si 4 coinsa! Trenutno stanje coinsa: " + coins);
    }
    public bool TryRepair() {
        if(coins >= 4) {
            coins -= 4;
            Debug.Log("Popravak plaćen. Preostalo coinsa: " + coins);
            return true;
        } else {
            Debug.Log("Nemaš dovoljno novca za popravak!");
            return false;
        }
    }
}