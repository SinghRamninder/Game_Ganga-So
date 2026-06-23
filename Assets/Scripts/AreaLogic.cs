using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AreaLogic : MonoBehaviour
{
    [SerializeField] private List<GameObject> entryExit = new List<GameObject>();
    [SerializeField] private Transform checkpointBeforeStart;
    [SerializeField] private Transform checkpointAfterEnd;
    [SerializeField] private float enemiesSpawnDelay = 0.5f;
    [SerializeField] private float entryExitTransitionTime = 1f;

    private Enemy[] allEnemies;
    private Vector3[] enemiesStartPosition;
    private GameObject[] allEnemiesAnimClone;
    private bool isFighting = false;
    private bool startLateUpdate = false;
    private List<Material> entryExitMaterials = new List<Material>();
    private Coroutine transitionCoroutine;

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
            allEnemiesAnimClone = new GameObject[allEnemies.Length];

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

    }

    private void LateUpdate()
    {
        if (startLateUpdate)
        {
            bool areEnemiesActive = CheckAllEnemies();

            if (!areEnemiesActive)
            {
                startLateUpdate = false;
                StartCoroutine(EndAreaSequence());
            }
        }
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

        foreach (Enemy enemy in allEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }

        isFighting = false;
        Destroy(this);
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

            if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(TransitionEntryExitMaterials(1f, false));

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
                allEnemiesAnimClone[i].transform.position = enemiesStartPosition[i];
                allEnemiesAnimClone[i].SetActive(true);

                Animator animator = allEnemiesAnimClone[i].GetComponent<Animator>();
                float animationTime = animator != null ? animator.GetCurrentAnimatorClipInfo(0)[0].clip.length : 0;

                StartCoroutine(AppearAnimationWait(animationTime, allEnemies[i].gameObject, allEnemiesAnimClone[i]));
            }

            yield return new WaitForSeconds(enemiesSpawnDelay);
        }

        startLateUpdate = true;
    }

    private IEnumerator AppearAnimationWait(float time, GameObject enemyObject, GameObject enemyCloneObject)
    {
        yield return new WaitForSeconds(time);

        enemyObject.SetActive(true);
        enemyCloneObject.SetActive(false);
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
            allEnemiesAnimClone[i].SetActive(false);

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

        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(TransitionEntryExitMaterials(0f, true));
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
