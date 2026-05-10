using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public int coins = 10;
    public void EnemyKilled() {
        coins += 4;
        Debug.Log('Zaradio si 4 coinsa! Trenutno stanje coinsa: ' + coins);
    }
    public bool TryRepair() {
        if(coins >= 4) {
            coins -= 4;
            Debug.Log('Popravak plaćen. Preostalo coinsa: ' + coins);
        } else {
            Debug.Log('Nemaš dovoljno novca za popravak!');
            return false;
        }
    }
}
