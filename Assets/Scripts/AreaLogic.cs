using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AreaLogic : MonoBehaviour
{
    [SerializeField] private List<GameObject> entryExit = new List<GameObject>();
    [SerializeField] private Transform checkpointBeforeStart;
    [SerializeField] private Transform checkpointAfterEnd;
    [SerializeField] private float enemiesSpawnDelay = 0.5f;

    private Enemy[] allEnemies;
    private Vector3[] enemiesStartPosition;
    private bool isFighting = false;
    private bool startLateUpdate = false;

    private void OnEnable()
    {
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.ResetArea += ResetThisArea;
        }
    }

    private void OnDisable()
    {
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.ResetArea -= ResetThisArea;
        }
    }

    private void Awake()
    {
        allEnemies = gameObject.GetComponentsInChildren<Enemy>();

        if (allEnemies.Length > 0)
        {
            enemiesStartPosition = new Vector3[allEnemies.Length];

            for (int i = 0; i < allEnemies.Length; i++)
            {
                if (allEnemies[i] != null)
                {
                    enemiesStartPosition[i] = allEnemies[i].transform.position;
                    allEnemies[i].gameObject.SetActive(false);
                }
            }
        }

        foreach (GameObject obj in entryExit)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }

    }

    private void LateUpdate()
    {
        if (startLateUpdate)
        {
            bool areEnemiesActive = CheckAllEnemies();

            if (!areEnemiesActive)
            {
                if (CheckpointManager.Instance != null)
                {
                    CheckpointManager.Instance.spawnPosition = checkpointAfterEnd;
                }

                foreach (GameObject obj in entryExit)
                {
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }

                foreach (Enemy enemy in allEnemies)
                {
                    if (enemy != null)
                    {
                        Destroy(enemy.gameObject);
                    }
                }

                isFighting = false;
                startLateUpdate = false;
                Destroy(this);
            }
        }
    }

    private bool CheckAllEnemies()
    {
        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i] == null)
                continue;

            if (allEnemies[i].gameObject.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isFighting)
        {
            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.spawnPosition = checkpointBeforeStart;
            }

            foreach (GameObject obj in entryExit)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    Debug.Log("Entry exit active!!");
                }
            }

            StartCoroutine(EnemiesSpawnWaitTime());
        }
    }

    private IEnumerator EnemiesSpawnWaitTime()
    {
        isFighting = true;

        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i] != null)
            {
                allEnemies[i].transform.position = enemiesStartPosition[i];
                allEnemies[i].gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(enemiesSpawnDelay);
        }

        startLateUpdate = true;
    }

    private void ResetThisArea()
    {
        startLateUpdate = false;
        isFighting = false;

        for (int i = 0; i < allEnemies.Length; i++)
        {
            Enemy enemy = allEnemies[i];

            Rigidbody rb = enemy.GetComponent<Rigidbody>();
            Collider[] allColliders = enemy.GetComponentsInChildren<Collider>(true);

            enemy.gameObject.SetActive(false);

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                rb.detectCollisions = true;
                rb.isKinematic = false;
            }

            foreach (Collider col in allColliders)
            {
                if (col != null)
                {
                    col.enabled = true;
                }
            }

            enemy.MakeEnemyAlive();

            foreach (GameObject obj in entryExit)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
}
