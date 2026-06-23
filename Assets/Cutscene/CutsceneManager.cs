using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    [Header("Cutscene")]
    [SerializeField] private PlayableDirector startCutsceneDirector;
    [SerializeField] private CutsceneHandover cutsceneHandler;

    [HideInInspector] public bool IsCutscenePlaying = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GetComponent<Collider>().enabled = false;
            cutsceneHandler.BeginCutscene();
            IsCutscenePlaying = true;
            startCutsceneDirector.Play();
            startCutsceneDirector.stopped += OnCutscneneFinished;
        }
    }

    private void OnCutscneneFinished(PlayableDirector director)
    {
        IsCutscenePlaying = false;
        cutsceneHandler.EndCutscene();
        startCutsceneDirector.stopped -= OnCutscneneFinished;
    }
}
