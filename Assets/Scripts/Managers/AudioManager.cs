using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance {  get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public IEnumerator PlaySound(GameObject targetAudioSourceObj, AudioClip clip, float volume, bool is3D)
    {
        AudioSource source = targetAudioSourceObj.AddComponent<AudioSource>();
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

        Destroy(source, 2f);
    }
}
