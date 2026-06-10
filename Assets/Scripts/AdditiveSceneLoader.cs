using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditiveSceneLoader : MonoBehaviour
{
    [SerializeField] private string[] additiveScenes;

    private void Start()
    {
        StartCoroutine(LoadAdditiveScenes());
    }

    private IEnumerator LoadAdditiveScenes()
    {
        foreach (string sceneName in additiveScenes)
        {
            if (!SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

                while (!operation.isDone)
                {
                    yield return null;
                }
            }
        }
    }
}