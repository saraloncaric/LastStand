using UnityEngine;

public class DeploymentSystem : MonoBehaviour
{
    public EconomyManager economy;
    public GameObject vojnik;
    public Transform[] lokacije;
    public int cijenaVojnika = 10;

    private int tenutnaLokcija = 0;
    public void DeployUnit() {
        if(economy.coins >= cijenaVojnika) {
            if(trenutnaLokacija < lokacije.Length) {
                Instantiate(vojnik, lokacije[trenutnaLokacija].position, lokacije[trenutnaLokacija].rotation);
                economy.coins -= cijenaVojnika;
                trenutnaLokacija++;
                Debug.Log("Vojnik postavljen, ostala mjesta: " + (lokacije.Length + trenutnaLokacija));
            } else {
                Debug.Log("Toranj je pun");
            }
        } else {
            Debug.Log("Nemaš dovoljno coinsa za vojnika");
        }
    }
}