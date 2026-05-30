using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DeploymentSystem : MonoBehaviour
{
    [System.Serializable]
    public class Lokacija {
        public string naziv;
        public Transform spawnPoint;
        public bool zauzeto;
    }

    public EconomyManager economy;
    public GameManager gameManager;
    public GameObject vojnikPrefab;
    public Lokacija[] lokacije;

    public GameObject gumbPrefab;
    public Transform gumbContainer;

    public GameObject oruzjePanel;
    public Transform oruzjeContainer;
    public GameObject oruzjeGumbPrefab;

    int odabraniVojnikIndex = -1;
    bool[] vojnikPostavljen = new bool[12];
    int[] vojnikLokacija = new int[12];

    void Start() {
        for (int i = 0; i < 12; i++) {
            vojnikLokacija[i] = -1;
        }
        oruzjePanel.SetActive(false);
        PrikaziLokacije();
    }

    public void PrikaziLokacije() {
        foreach (Transform child in gumbContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < lokacije.Length; i++) {
            int index = i;
            GameObject gumb = Instantiate(gumbPrefab, gumbContainer);
            string status = lokacije[i].zauzeto ? " (zauzeto)" : " (slobodno)";
            gumb.GetComponentInChildren<TextMeshProUGUI>().text = lokacije[i].naziv + status;
            gumb.GetComponent<Button>().onClick.AddListener(() => PostaviVojnika(index));
        }
    }

    public void PostaviVojnika(int index) {
        if (lokacije[index].zauzeto) {
            Debug.Log("Lokacija je zauzeta!");
            return;
        }

        int cijena = vojnikPostavljen[odabraniVojnikIndex == -1 ? 0 : odabraniVojnikIndex] ? 2 : 1;

        if (economy.coins < cijena) {
            Debug.Log("Nema dovoljno coinsa!");
            return;
        }

        economy.coins -= cijena;
        Instantiate(vojnikPrefab, lokacije[index].spawnPoint.position, lokacije[index].spawnPoint.rotation);
        lokacije[index].zauzeto = true;
        vojnikPostavljen[index] = true;
        vojnikLokacija[index] = index;
        PrikaziLokacije();
    }

    public void OtvoriOruzje(int vojnikIndex) {
        odabraniVojnikIndex = vojnikIndex;
        oruzjePanel.SetActive(true);

        foreach (Transform child in oruzjeContainer)
            Destroy(child.gameObject);

        int val = gameManager.trenutniVal;

        if (val <= 1) {
            DodajOruzje("Luk", 3);
        }
        else if (val == 2) {
            DodajOruzje("Bodež", 5);
        }
        else {
            DodajOruzje("Mač", 6);
            DodajOruzje("Sjekira", 7);
        }
    }

    void DodajOruzje(string naziv, int cijena) {
        GameObject gumb = Instantiate(oruzjeGumbPrefab, oruzjeContainer);
        gumb.GetComponentInChildren<TextMeshProUGUI>().text = naziv + " (" + cijena + " coins)";
        gumb.GetComponent<Button>().onClick.AddListener(() => KupiOruzje(cijena));
    }

    void KupiOruzje(int cijena) {
        if (economy.coins < cijena) {
            Debug.Log("Nema dovoljno coinsa!");
            return;
        }
        economy.coins -= cijena;
        oruzjePanel.SetActive(false);
        Debug.Log("Oružje kupljeno!");
    }
}