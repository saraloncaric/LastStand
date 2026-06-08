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

    public GameObject oruzjeGumbPrefab;

    int odabraniVojnikIndex = -1;
    bool[] vojnikPostavljen = new bool[12];
    int[] vojnikLokacija = new int[12];

    void Start() {
        for (int i = 0; i < 12; i++)
            vojnikLokacija[i] = -1;

        PrikaziVojnike();
    }

    public void PrikaziVojnike() {
        foreach (Transform child in gumbContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < 12; i++) {
            int index = i;
            GameObject red = Instantiate(gumbPrefab, gumbContainer);
            
            string status = vojnikPostavljen[i] ? " postavljen" : "";
            red.transform.Find("Vojnik tekst").GetComponent<TextMeshProUGUI>().text = "Vojnik " + (i + 1) + status;
            
            red.transform.Find("Pozicija").GetComponent<Button>().onClick.AddListener(() => OtvoriLokacije(index));
            red.transform.Find("Oruzje").GetComponent<Button>().onClick.AddListener(() => OtvoriOruzje(index));
        }
    }

    public void OtvoriLokacije(int vojnikIndex) {
        odabraniVojnikIndex = vojnikIndex;

        foreach (Transform child in gumbContainer)
            Destroy(child.gameObject);

        GameObject prazanGumb = Instantiate(oruzjeGumbPrefab, gumbContainer);
        prazanGumb.GetComponentInChildren<TextMeshProUGUI>().text = "Makni vojnika";
        prazanGumb.GetComponentInChildren<Button>().onClick.AddListener(() => MakniVojnika(vojnikIndex));

        for (int i = 0; i < lokacije.Length; i++) {
            int lokIndex = i;
            if (lokacije[i].zauzeto) continue;

            GameObject gumb = Instantiate(oruzjeGumbPrefab, gumbContainer);
            gumb.GetComponentInChildren<TextMeshProUGUI>().text = lokacije[i].naziv;
            gumb.GetComponentInChildren<Button>().onClick.AddListener(() => PostaviVojnika(lokIndex));
        }
    }

    public void PostaviVojnika(int lokacijaIndex) {
        if (lokacije[lokacijaIndex].zauzeto) {
            Debug.Log("Lokacija je zauzeta!");
            return;
        }

        int vojnik = odabraniVojnikIndex == -1 ? 0 : odabraniVojnikIndex;
        int cijena = vojnikPostavljen[vojnik] ? 2 : 1;

        if (economy.coins < cijena) {
            Debug.Log("Nema dovoljno coinsa!");
            return;
        }

        int staraLokacija = vojnikLokacija[vojnik];
        if (staraLokacija != -1) {
            lokacije[staraLokacija].zauzeto = false;
        }

        economy.coins -= cijena;
        Instantiate(vojnikPrefab, lokacije[lokacijaIndex].spawnPoint.position, lokacije[lokacijaIndex].spawnPoint.rotation);
        lokacije[lokacijaIndex].zauzeto = true;
        vojnikPostavljen[vojnik] = true;
        vojnikLokacija[vojnik] = lokacijaIndex;
        PrikaziVojnike();
    }
    public void MakniVojnika(int vojnikIndex) {
        int staraLokacija = vojnikLokacija[vojnikIndex];
        if (staraLokacija != -1) {
            lokacije[staraLokacija].zauzeto = false;
            vojnikLokacija[vojnikIndex] = -1;
            vojnikPostavljen[vojnikIndex] = false;
        }
        PrikaziVojnike();
    }

    public void OtvoriOruzje(int vojnikIndex) {
        odabraniVojnikIndex = vojnikIndex;

        foreach (Transform child in gumbContainer)
            Destroy(child.gameObject);

        GameObject natragGumb = Instantiate(oruzjeGumbPrefab, gumbContainer);
        natragGumb.GetComponentInChildren<TextMeshProUGUI>().text = "< Natrag";
        natragGumb.GetComponentInChildren<Button>().onClick.AddListener(() => PrikaziVojnike());

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
        GameObject gumb = Instantiate(oruzjeGumbPrefab, gumbContainer);
        gumb.GetComponentInChildren<TextMeshProUGUI>().text = naziv + " (" + cijena + " coins)";
        gumb.GetComponentInChildren<Button>().onClick.AddListener(() => KupiOruzje(cijena));
    }

    void KupiOruzje(int cijena) {
        if (economy.coins < cijena) {
            Debug.Log("Nema dovoljno coinsa!");
            return;
        }
        economy.coins -= cijena;
        Debug.Log("Oružje kupljeno!");
    }
}