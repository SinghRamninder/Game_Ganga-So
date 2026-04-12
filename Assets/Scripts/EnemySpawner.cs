using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnInfo
{
    public GameObject enemyPrefab;
    [Range(0, 100)] public float spawnPercentage;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Vector3 spawnAreaSize = new Vector3(10f, 0f, 10f);
    public float spawnRate = 2f;
    public bool randomSpawnFrequency = false;
    public List<EnemySpawnInfo> enemiesToSpawn;

    private float spawnTimer;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawCube(transform.position, spawnAreaSize);
    }

    private void Update()
    {
        if (enemiesToSpawn == null || enemiesToSpawn.Count == 0 || spawnRate <= 0) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnEnemy();
            spawnTimer = 1f / spawnRate;
        }
    }

    private void SpawnEnemy()
    {
        GameObject prefabToSpawn = null;
        if (randomSpawnFrequency)
        {
            prefabToSpawn = enemiesToSpawn[Random.Range(0, enemiesToSpawn.Count)].enemyPrefab;
        }
        else
        {
            float total = 0;
            foreach (var e in enemiesToSpawn) total += e.spawnPercentage;
            if (total == 0) return;

            float roll = Random.Range(0, total);
            float current = 0;
            foreach (var e in enemiesToSpawn)
            {
                current += e.spawnPercentage;
                if (roll <= current)
                {
                    prefabToSpawn = e.enemyPrefab;
                    break;
                }
            }
        }

        if (prefabToSpawn != null)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
                Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
            );
            Instantiate(prefabToSpawn, transform.position + randomPos, Quaternion.identity);
        }
    }
}
