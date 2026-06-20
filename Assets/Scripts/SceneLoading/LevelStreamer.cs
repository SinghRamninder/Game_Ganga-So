using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelStreamer : MonoBehaviour
{
    public static LevelStreamer Instance;

    [SerializeField] string mainScene = "GalaryScene";
    [SerializeField] string[] additiveScenes;        // the extra scenes on top

    [SerializeField] GameObject loadingScreen;       // child, starts OFF
    [SerializeField] Animator transitionAnimator;    // child
    [SerializeField] string inState = "Start";       // covers the screen
    [SerializeField] string outState = "End";        // reveals the screen

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadLevel() => StartCoroutine(LoadRoutine());

    IEnumerator LoadRoutine()
    {
        transitionAnimator.Play(inState);
        yield return WaitForState(transitionAnimator, inState);

        loadingScreen.SetActive(true);
        yield return null;

        yield return SceneManager.LoadSceneAsync(mainScene, LoadSceneMode.Single);

        foreach (string scene in additiveScenes)
            yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

        loadingScreen.SetActive(false);

        transitionAnimator.Play(outState);
        yield return WaitForState(transitionAnimator, outState);
    }

    IEnumerator WaitForState(Animator a, string state)
    {
        yield return null;
        while (a.GetCurrentAnimatorStateInfo(0).IsName(state) &&
               a.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;
    }
}