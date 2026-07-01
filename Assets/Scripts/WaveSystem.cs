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
    [SerializeField] float spawnRadius = 6f;
    [SerializeField] float navMeshSampleDistance = 200f;

    [SerializeField] GameManager gameManager;

    int _lastWave;
    bool _spawning;
    bool _monitoringWave;

    void Update()
    {
        if (gameManager == null)
            return;

        if (gameManager.trenutnafaza == GameManager.GamePhase.Val &&
            gameManager.trenutniVal != _lastWave)
        {
            _lastWave = gameManager.trenutniVal;
            _monitoringWave = false;
            StartCoroutine(SpawnWave(gameManager.trenutniVal - 1));
        }

        if (gameManager.trenutnafaza != GameManager.GamePhase.Val)
        {
            _monitoringWave = false;
            return;
        }

        if (_monitoringWave && !_spawning && CountAliveEnemies() == 0)
        {
            _monitoringWave = false;
            gameManager.ZavrsiVal();
        }
    }

    static int CountAliveEnemies()
    {
        int count = 0;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            Health health = enemies[i].GetComponent<Health>();
            if (health != null && health.currentHealth > 0f)
                count++;
        }
        return count;
    }

    public IEnumerator SpawnWave(int waveIndex)
    {
        if (waves == null || waveIndex < 0 || waveIndex >= waves.Length)
            yield break;

        WaveData wave = waves[waveIndex];
        if (wave.enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
            yield break;

        _spawning = true;

        for (int i = 0; i < wave.enemyCount; i++)
        {
            if (gameManager != null && gameManager.trenutnafaza != GameManager.GamePhase.Val)
            {
                _spawning = false;
                yield break;
            }

            if (TryGetRandomSpawn(out Vector3 position, out Quaternion rotation))
            {
                GameObject enemy = Instantiate(wave.enemyPrefab, position, rotation);

                if (enemy.TryGetComponent(out NavMeshAgent agent))
                {
                    if (!agent.isOnNavMesh)
                        agent.Warp(position);
                }

                if (enemy.TryGetComponent(out EnemyAI enemyAI))
                    enemyAI.ApplyWaveSpeedMultiplier(wave.enemySpeedMultiplier);
                else if (enemy.TryGetComponent(out NavMeshAgent fallbackAgent))
                    fallbackAgent.speed *= wave.enemySpeedMultiplier;
            }

            yield return new WaitForSeconds(wave.spawnInterval);
        }

        _spawning = false;

        if (gameManager != null && gameManager.trenutnafaza == GameManager.GamePhase.Val)
            _monitoringWave = true;
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
        return true;
    }
}
