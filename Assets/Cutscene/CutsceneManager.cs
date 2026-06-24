using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    [Header("Cutscene")]
    [SerializeField] private PlayableDirector startCutsceneDirector;
    [SerializeField] private CutsceneHandover cutsceneHandler;
    [SerializeField] private FinalAreaLogic finalAreaLogic;

    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject serversParent;

    [Header("Post Cutscene Spawning")]
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private GameObject spawnPoint1;
    [SerializeField] private GameObject spawnPoint2;
    [SerializeField] private GameObject spawnPoint3;

    private ExplosiveServers[] managedServers;

    [HideInInspector] public bool IsCutscenePlaying = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (serversParent != null)
        {
            managedServers = serversParent.GetComponentsInChildren<ExplosiveServers>(true);
        }
    }

    private void Start()
    {
        SetServerColliders(false);
    }

    private void SetServerColliders(bool state)
    {
        if (managedServers != null)
        {
            foreach (ExplosiveServers server in managedServers)
            {
                if (server != null)
                {
                    Collider[] colliders = server.GetComponentsInChildren<Collider>(true);
                    foreach (Collider col in colliders)
                    {
                        if (col != null)
                        {
                            col.enabled = state;
                        }
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GetComponent<Collider>().enabled = false;
            canvas.SetActive(false);
            cutsceneHandler.BeginCutscene();
            IsCutscenePlaying = true;
            startCutsceneDirector.Play();
            startCutsceneDirector.stopped += OnCutscneneFinished;
        }
    }

    private void OnCutscneneFinished(PlayableDirector director)
    {
        IsCutscenePlaying = false;
        SetServerColliders(true);
        
        if (objectToSpawn != null)
        {
            if (spawnPoint1 != null) 
            {
                GameObject spawned1 = Instantiate(objectToSpawn, spawnPoint1.transform.position, spawnPoint1.transform.rotation);
                spawned1.transform.localScale = spawnPoint1.transform.localScale;
            }
            if (spawnPoint2 != null) 
            {
                GameObject spawned2 = Instantiate(objectToSpawn, spawnPoint2.transform.position, spawnPoint2.transform.rotation);
                spawned2.transform.localScale = spawnPoint2.transform.localScale;
            }
            if (spawnPoint3 != null) 
            {
                GameObject spawned3 = Instantiate(objectToSpawn, spawnPoint3.transform.position, spawnPoint3.transform.rotation);
                spawned3.transform.localScale = spawnPoint3.transform.localScale;
            }
        }

        finalAreaLogic.StartSpawningEnemies();
        canvas.SetActive(true);
        cutsceneHandler.EndCutscene();
        startCutsceneDirector.stopped -= OnCutscneneFinished;
    }
}
