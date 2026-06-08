using UnityEngine;

public class RepairSystem : MonoBehaviour
{
    public EconomyManager economy;
    public int cijenaPopravka = 4;

    public void PokusajPopravak() {
        if(economy.coins >= cijenaPopravka) {
            economy.coins -= cijenaPopravka;
            Debug.Log("Popravak uspješan, preostalo je još: " + economy.coins);
        } else {
            Debug.Log("Nema dovoljno coinsa za popravak");
        }
    }
}