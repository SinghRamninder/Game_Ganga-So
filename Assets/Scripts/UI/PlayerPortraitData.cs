using UnityEngine;

[CreateAssetMenu(fileName = "NewPortraitData", menuName = "UI/Player Portrait Data")]
public class PlayerPortraitData : ScriptableObject
{
    [Header("Normal States (Idle)")]
    public Sprite fullHealth;
    public Sprite midHealth;
    public Sprite lowHealth;

    [Header("Hit States (Damage taken)")]
    public Sprite fullHit;
    public Sprite midHit;
    public Sprite lowHit;

    [Header("Blink States (Idles animation)")]
    public Sprite fullBlink;
    public Sprite midBlink;
    public Sprite lowBlink;

    [Header("Dead State")]
    public Sprite dead;
}