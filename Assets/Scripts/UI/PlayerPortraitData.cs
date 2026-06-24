using UnityEngine;

[CreateAssetMenu(fileName = "NewPortraitData", menuName = "UI/Player Portrait Data")]
public class PlayerPortraitData : ScriptableObject
{
    [Header("Base Portrait States")]
    public Sprite fullHealth;
    public Sprite midHealth;
    public Sprite lowHealth;

    [Header("Hit States")]
    public Sprite fullHit;
    public Sprite midHit;
    public Sprite lowHit;

    [Header("Blink States")]
    public Sprite fullBlink;
    public Sprite midBlink;
    public Sprite lowBlink;

    [Header("Dead State")]
    public Sprite dead;

    [Header("Health Bar Visuals")]
    public Sprite heartIconSprite;       // Left heart icon sprite
    public Sprite barFillSprite;         // Active health capsule layer
    public Sprite barBackgroundSprite;   // Missing health rail layer

    [Header("Weapon Overview Info")]
    [Tooltip("The display name of this weapon.")]
    public string weaponName;

    [Tooltip("The description or stats of this weapon.")]
    [TextArea(3, 5)] public string weaponDescription;

    [Tooltip("The specific background card image for this weapon's overview.")]
    public Sprite weaponOverviewCardBG;
}