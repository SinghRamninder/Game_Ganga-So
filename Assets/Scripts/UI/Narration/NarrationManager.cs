using System.Collections;
using TMPro;
using UnityEngine;

public class NarrationManager : MonoBehaviour
{
    public static NarrationManager Instance { get; private set; }
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private AudioSource voiceSource;

    private Coroutine activeRoutine;

    private void Awake() => Instance = this;

    public void Play(NarrationData data)
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(ShowLine(data));
    }

    private IEnumerator ShowLine(NarrationData data)
    {
        subtitleText.text = data.displayText;
        canvasGroup.alpha = 1f;
        if (data.voiceLine) voiceSource.PlayOneShot(data.voiceLine);

        yield return new WaitForSeconds(data.displayDuration);

        // simple fade out
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime * 2f;
            canvasGroup.alpha = t;
            yield return null;
        }
    }


}
