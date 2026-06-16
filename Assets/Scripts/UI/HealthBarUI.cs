using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public static HealthBarUI instance;

    [Header("UI Components")]
    [SerializeField] private Image heartIcon;             // کامپوننت عکس قلب
    [SerializeField] private GameObject emptyBarsParent;  // پوشه/والد کپسول‌های خالی
    [SerializeField] private GameObject fillBarsParent;   // پوشه/والد کپسول‌های پر

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void ChangeHealthBarVisuals(PlayerPortraitData data)
    {
        if (data == null) return;

        // ۱. تغییر عکس قلب
        if (heartIcon != null && data.heartIconSprite != null)
            heartIcon.sprite = data.heartIconSprite;

        // ۲. تغییر عکس تمام کپسول‌های خالی به صورت یکجا
        if (emptyBarsParent != null && data.barBackgroundSprite != null)
        {
            Image[] images = emptyBarsParent.GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                img.sprite = data.barBackgroundSprite;
            }
        }

        // ۳. تغییر عکس تمام کپسول‌های پر به صورت یکجا
        if (fillBarsParent != null && data.barFillSprite != null)
        {
            Image[] images = fillBarsParent.GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                img.sprite = data.barFillSprite;
            }
        }
    }
}