using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance {  get; private set; }

    private List<AudioSource> allSources = new List<AudioSource>();
    private bool allPlayChecked = false;
    private bool allPauseChecked = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (PauseMenuController.Instance?.IsGamePaused == true)
        {
            if (allSources.Count > 0 && PauseMenuController.Instance?.allAudioChecked == false)
            {
                foreach (AudioSource source in allSources)
                {
                    if (source != null)
                        source.Pause();
                }

                PauseMenuController.Instance.allAudioChecked = true;
            }
        }

        if (PauseMenuController.Instance?.IsGamePaused == false)
        {
            if (allSources.Count > 0 && PauseMenuController.Instance?.allAudioChecked == false)
            {
                foreach (AudioSource source in allSources)
                {
                    if (source != null)
                        source.Play();
                }

                PauseMenuController.Instance.allAudioChecked = true;
            }
        }
    }

    public IEnumerator PlaySound(GameObject targetAudioSourceObj, AudioClip clip, float volume, bool is3D)
    {
        AudioSource source = targetAudioSourceObj.AddComponent<AudioSource>();
        allSources.Add(source);
        source.clip = clip;
        source.volume = volume;
        if (is3D)
        {
            source.spatialBlend = 1f;
            source.minDistance = 5f;
            source.maxDistance = 50f;
        }
        source.Play();

        yield return new WaitForSeconds(source.clip.length);

        allSources.Remove(source);
        Destroy(source, 2f);
    }
}
