using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class FinalAreaLogic : MonoBehaviour
{
    [SerializeField] private List<GameObject> entryExit = new List<GameObject>();
    [SerializeField] private Transform checkpointBeforeStart;
    [SerializeField] private Transform checkpointAfterEnd;
    [SerializeField] private float enemiesSpawnDelay = 0.5f;
    [SerializeField] private float entryExitTransitionTime = 1f;

    [Header("Final Area Specifics")]
    [SerializeField] private GameObject serversParent;
    [SerializeField] private float enemyRespawnDelay = 3f;

    private Enemy[] allEnemies;
    private Vector3[] enemiesStartPosition;
    private GameObject[] allEnemiesAnimClone;
    private bool isFighting = false;
    private bool startLateUpdate = false;
    private List<Material> entryExitMaterials = new List<Material>();
    private Coroutine transitionCoroutine;
    
    private ExplosiveServers[] serversToMonitor;
    private bool[] isRespawning;
    private bool isSubscribedToReset = false;

    private void SubscribeToResetEvent()
    {
        if (CheckpointManager.Instance == null || isSubscribedToReset)
        {
            return;
        }

        CheckpointManager.Instance.ResetArea += ResetThisArea;
        isSubscribedToReset = true;
    }

    private void UnsubscribeFromResetEvent()
    {
        if (!isSubscribedToReset || CheckpointManager.Instance == null)
        {
            return;
        }

        CheckpointManager.Instance.ResetArea -= ResetThisArea;
        isSubscribedToReset = false;
    }

    private void OnEnable()
    {
        SubscribeToResetEvent();
    }

    private void OnDisable()
    {
        UnsubscribeFromResetEvent();
    }

    private void Start()
    {
        SubscribeToResetEvent();
    }

    private void Awake()
    {
        allEnemies = gameObject.GetComponentsInChildren<Enemy>();

        if (allEnemies.Length > 0)
        {
            enemiesStartPosition = new Vector3[allEnemies.Length];
            allEnemiesAnimClone = new GameObject[allEnemies.Length];
            isRespawning = new bool[allEnemies.Length];

            for (int i = 0; i < allEnemies.Length; i++)
            {
                if (allEnemies[i] != null)
                {
                    enemiesStartPosition[i] = allEnemies[i].transform.position;

                    if (allEnemies[i].spawnAnimClone != null)
                    {
                        allEnemiesAnimClone[i] = Instantiate(allEnemies[i].spawnAnimClone, enemiesStartPosition[i], Quaternion.identity);
                        allEnemiesAnimClone[i].transform.localScale = allEnemies[i].transform.localScale;
                        allEnemiesAnimClone[i].SetActive(false);
                    }
                    
                    allEnemies[i].gameObject.SetActive(false);
                }
            }
        }

        foreach (GameObject obj in entryExit)
        {
            if (obj != null)
            {
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer r in renderers)
                {
                    entryExitMaterials.Add(r.material);
                    r.material.SetFloat("_AppearAmount", 0f);
                }
                obj.SetActive(false);
            }
        }

        if (serversParent != null)
        {
            serversToMonitor = serversParent.GetComponentsInChildren<ExplosiveServers>(true);
        }
    }

    public void StartSpawningEnemies()
    {
        if (!isFighting && gameObject.activeInHierarchy)
        {
            StartCoroutine(EnemiesSpawnWaitTime());
        }
    }

    private void LateUpdate()
    {
        if (startLateUpdate)
        {
            bool allServersDestroyed = true;

            if (serversToMonitor != null && serversToMonitor.Length > 0)
            {
                for (int i = 0; i < serversToMonitor.Length; i++)
                {
                    if (serversToMonitor[i] != null && !serversToMonitor[i].hasExploded)
                    {
                        allServersDestroyed = false;
                        break;
                    }
                }
            }

            if (allServersDestroyed)
            {
                startLateUpdate = false;
                StartCoroutine(EndAreaSequence());
            }
            else
            {
                // Check if any enemy needs to respawn
                for (int i = 0; i < allEnemies.Length; i++)
                {
                    if (allEnemies[i] != null && !allEnemies[i].gameObject.activeInHierarchy && !isRespawning[i])
                    {
                        StartCoroutine(RespawnEnemyRoutine(i));
                    }
                }
            }
        }
    }
    
    private IEnumerator RespawnEnemyRoutine(int index)
    {
        isRespawning[index] = true;
        
        yield return new WaitForSeconds(enemyRespawnDelay);
        
        if (!isFighting || !startLateUpdate)
        {
            isRespawning[index] = false;
            yield break; // Sequence ended before respawn triggered
        }

        Enemy enemy = allEnemies[index];

        // Reset Physics and Collider to original state
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        Collider[] allColliders = enemy.GetComponentsInChildren<Collider>(true);

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
        
        // Spawn Process
        enemy.transform.position = enemiesStartPosition[index];

        if (allEnemiesAnimClone[index] != null)
        {
            GameObject enemyCloneObject = allEnemiesAnimClone[index];
            enemyCloneObject.transform.position = enemiesStartPosition[index];
            enemyCloneObject.SetActive(true);

            Animator animator = enemyCloneObject.GetComponent<Animator>();
            float animationTime = animator != null ? animator.GetCurrentAnimatorClipInfo(0)[0].clip.length : 0;

            yield return StartCoroutine(AppearAnimationWait(animationTime, enemy.gameObject, enemyCloneObject));
        }
        else
        {
            enemy.gameObject.SetActive(true);
        }

        isRespawning[index] = false;
    }

    private IEnumerator EndAreaSequence()
    {
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.spawnPosition = checkpointAfterEnd;
        }

        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        yield return StartCoroutine(TransitionEntryExitMaterials(0f, false));

        foreach (GameObject obj in entryExit)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i] != null)
            {
                Destroy(allEnemies[i].gameObject);
            }
            if (allEnemiesAnimClone[i] != null)
            {
                Destroy(allEnemiesAnimClone[i]);
            }
        }

        isFighting = false;
        Destroy(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isFighting)
        {
            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.spawnPosition = checkpointBeforeStart;
            }

            if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(TransitionEntryExitMaterials(1f, false));
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
                
                if (allEnemiesAnimClone[i] != null)
                {
                    allEnemiesAnimClone[i].transform.position = enemiesStartPosition[i];
                    allEnemiesAnimClone[i].SetActive(true);

                    Animator animator = allEnemiesAnimClone[i].GetComponent<Animator>();
                    float animationTime = animator != null ? animator.GetCurrentAnimatorClipInfo(0)[0].clip.length : 0;

                    StartCoroutine(AppearAnimationWait(animationTime, allEnemies[i].gameObject, allEnemiesAnimClone[i]));
                }
                else
                {
                    allEnemies[i].gameObject.SetActive(true);
                }
            }

            yield return new WaitForSeconds(enemiesSpawnDelay);
        }

        startLateUpdate = true;
    }

    private IEnumerator AppearAnimationWait(float time, GameObject enemyObject, GameObject enemyCloneObject)
    {
        yield return new WaitForSeconds(time);

        if (enemyObject != null) enemyObject.SetActive(true);
        if (enemyCloneObject != null) enemyCloneObject.SetActive(false);
    }

    private void ResetThisArea()
    {
        startLateUpdate = false;
        isFighting = false;

        if (allEnemies == null || enemiesStartPosition == null || allEnemiesAnimClone == null || isRespawning == null)
        {
            return;
        }

        for (int i = 0; i < allEnemies.Length; i++)
        {
            Enemy enemy = allEnemies[i];

            if (enemy == null)
            {
                if (i < isRespawning.Length)
                {
                    isRespawning[i] = false;
                }

                continue;
            }

            Rigidbody rb = enemy.GetComponent<Rigidbody>();
            Collider[] allColliders = enemy.GetComponentsInChildren<Collider>(true);

            enemy.gameObject.SetActive(false);
            if (i < allEnemiesAnimClone.Length && allEnemiesAnimClone[i] != null)
            {
                allEnemiesAnimClone[i].SetActive(false);
            }
            if (i < isRespawning.Length)
            {
                isRespawning[i] = false;
            }

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
        }

        if (serversToMonitor != null)
        {
            foreach (ExplosiveServers server in serversToMonitor)
            {
                if (server != null)
                {
                    server.RefreshEmissionColorReferences();
                    server.ResetServer();
                }
            }
        }

        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(TransitionEntryExitMaterials(1f, false));

        // Start spawning enemies immediately upon reset
        StartSpawningEnemies();
    }

    private IEnumerator TransitionEntryExitMaterials(float targetAmount, bool disableAfter)
    {
        float currentAmount = entryExitMaterials.Count > 0 && entryExitMaterials[0] != null ? entryExitMaterials[0].GetFloat("_AppearAmount") : (targetAmount > 0f ? 0f : 1f);
        float timeElapsed = 0f;

        if (targetAmount > 0f)
        {
            foreach (GameObject obj in entryExit)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }

        while (timeElapsed < entryExitTransitionTime)
        {
            float amount = Mathf.Lerp(currentAmount, targetAmount, timeElapsed / entryExitTransitionTime);
            foreach (Material mat in entryExitMaterials)
            {
                if (mat != null) mat.SetFloat("_AppearAmount", amount);
            }
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        foreach (Material mat in entryExitMaterials)
        {
            if (mat != null) mat.SetFloat("_AppearAmount", targetAmount);
        }

        if (disableAfter)
        {
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
