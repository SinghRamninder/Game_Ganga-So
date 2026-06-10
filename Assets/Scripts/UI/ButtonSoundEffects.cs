using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSoundEffects : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Audio Source")]
    [Tooltip("Drag the AudioSource here (usually on the Canvas or a SoundManager). If left empty, it will try to find one on this object.")]
    public AudioSource audioSource;

    [Header("Audio Clips")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("Volume Control")]
    [Range(0f, 1f)] public float hoverVolume = 0.5f;
    [Range(0f, 1f)] public float clickVolume = 0.8f;

    private void Start()
    {
        // Fallback: If no AudioSource is manually dragged, look for one on this button
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Second Fallback: Look for any AudioSource in the Scene (like on the Canvas)
        //if (audioSource == null)
        //{
        //    audioSource = FindFirstObjectByType<AudioSource>();
        //}
    }

    /// <summary>
    /// Triggered automatically when the mouse pointer enters the button area.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (audioSource != null && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound, hoverVolume);
        }
    }

    /// <summary>
    /// Triggered automatically when the mouse pointer clicks the button.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, clickVolume);
        }
    }
}