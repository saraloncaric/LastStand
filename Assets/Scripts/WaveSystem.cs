using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WaveSystem : MonoBehaviour
{
    [System.Serializable]
    public class WaveData
    {
        public string waveName;
        public GameObject enemyPrefab;
        public int enemyCount;
        public float spawnInterval;
        public float enemySpeedMultiplier = 1f;
        public string weaponType = "Default";
    }

    [SerializeField] WaveData[] waves;
    [SerializeField] Transform spawnPoint;
    [SerializeField] GameManager gameManager;

    int _lastWave;

    void Update()
    {
        if (gameManager == null)
            return;

        if (gameManager.trenutnafaza == GameManager.GamePhase.Val &&
            gameManager.trenutniVal != _lastWave)
        {
            _lastWave = gameManager.trenutniVal;
            StartCoroutine(SpawnWave(gameManager.trenutniVal - 1));
        }
    }

    public IEnumerator SpawnWave(int waveIndex)
    {
        if (waves == null || waveIndex < 0 || waveIndex >= waves.Length)
            yield break;

        WaveData wave = waves[waveIndex];
        if (wave.enemyPrefab == null || spawnPoint == null)
            yield break;

        for (int i = 0; i < wave.enemyCount; i++)
        {
            if (gameManager != null && gameManager.trenutnafaza != GameManager.GamePhase.Val)
                yield break;

            GameObject enemy = Instantiate(wave.enemyPrefab, spawnPoint.position, spawnPoint.rotation);

            if (enemy.TryGetComponent(out NavMeshAgent agent))
                agent.speed *= wave.enemySpeedMultiplier;

            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }
}
