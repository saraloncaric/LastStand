using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    public int coins = 51;

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

    public void AddCoins(int amount) {
        if (amount <= 0) return;
        coins += amount;
        Debug.Log("Zaradio si " + amount + " coinsa! Trenutno stanje coinsa: " + coins);
    }

    public void EnemyKilled() {
        AddCoins(3);
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
