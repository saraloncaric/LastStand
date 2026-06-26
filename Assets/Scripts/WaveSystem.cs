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

    [Header("Spawn lokacije")]
    [SerializeField] Transform[] spawnPoints;
    [Tooltip("Nasumicni radijus oko odabrane spawn tocke")]
    [SerializeField] float spawnRadius = 6f;
    [Tooltip("Koliko daleko trazi najblizu NavMesh tocku")]
    [SerializeField] float navMeshSampleDistance = 200f;

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
        if (wave.enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
            yield break;

        for (int i = 0; i < wave.enemyCount; i++)
        {
            if (gameManager != null && gameManager.trenutnafaza != GameManager.GamePhase.Val)
                yield break;

            if (TryGetRandomSpawn(out Vector3 position, out Quaternion rotation))
            {
                GameObject enemy = Instantiate(wave.enemyPrefab, position, rotation);

                if (enemy.TryGetComponent(out NavMeshAgent agent))
                {
                    if (!agent.isOnNavMesh)
                        agent.Warp(position);

                    agent.speed *= wave.enemySpeedMultiplier;
                }
            }

            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    bool TryGetRandomSpawn(out Vector3 position, out Quaternion rotation)
    {
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        rotation = point.rotation;

        Vector2 circle = Random.insideUnitCircle * spawnRadius;
        Vector3 candidate = point.position + new Vector3(circle.x, 0f, circle.y);

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
        {
            position = hit.position;
            return true;
        }

        if (NavMesh.SamplePosition(point.position, out NavMeshHit pointHit, navMeshSampleDistance, NavMesh.AllAreas))
        {
            position = pointHit.position;
            return true;
        }

        position = point.position;
        return false;
    }
}
