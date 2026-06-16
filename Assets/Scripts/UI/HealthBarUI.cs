using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public static HealthBarUI instance;

    [Header("UI Image Components")]
    [SerializeField] private Image heartIcon;       // کامپوننت ایمیج قلب در هایارکی
    [SerializeField] private Image barBackground;  // کامپوننت ایمیج ریل/بک‌گراند نوار خون
    [SerializeField] private Image barFill;        // کامپوننت ایمیج کپسول‌ها (یا fill نوار)

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void ChangeHealthBarVisuals(PlayerPortraitData data)
    {
        if (data == null) return;

        // تغییر عکس قلب
        if (heartIcon != null && data.heartIconSprite != null)
            heartIcon.sprite = data.heartIconSprite;

        // تغییر عکس بک‌گراند نوار
        if (barBackground != null && data.barBackgroundSprite != null)
            barBackground.sprite = data.barBackgroundSprite;

        // تغییر عکس کپسول‌ها/خون اصلی
        if (barFill != null && data.barFillSprite != null)
            barFill.sprite = data.barFillSprite;
    }
}