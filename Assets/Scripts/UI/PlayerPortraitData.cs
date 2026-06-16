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
    public Sprite heartIconSprite;       // عکس قلب سمت چپ
    public Sprite barFillSprite;         // عکس کپسول‌ها/پیکسل‌های پرکننده خون
    public Sprite barBackgroundSprite;   // عکس ریل یا بک‌گراند نوار خون
}