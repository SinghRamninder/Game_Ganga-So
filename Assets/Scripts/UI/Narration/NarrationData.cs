using UnityEngine;
[CreateAssetMenu(fileName = "NarrationData", menuName = "Narration/Line")]
public class NarrationData : ScriptableObject
{
    public string displayText;
    public AudioClip voiceLine;
    public float displayDuration = 4f;
}
