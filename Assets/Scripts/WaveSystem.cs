using System.Collections;
using UnityEngine;

public class WaveSystem : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public Transform spawnPoint;
    public GameManager gameManager;

    int zadnjiVal = 0;

    void Update() {
        if (gameManager.trenutnafaza == GameManager.GamePhase.Val && gameManager.trenutniVal != zadnjiVal) {
            zadnjiVal = gameManager.trenutniVal;
            StartCoroutine(SpawnVal(gameManager.trenutniVal));
        }
    }

    IEnumerator SpawnVal(int val) {
        int brojNeprijatelja = 25;
        if (val == 2) brojNeprijatelja = 35;
        if (val == 3) brojNeprijatelja = 45;

        for (int i = 0; i < brojNeprijatelja; i++) {
            if (gameManager.trenutnafaza != GameManager.GamePhase.Val) yield break;

            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

            yield return new WaitForSeconds(8f);
        }
    }
}