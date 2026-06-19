using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager instance;

    [Header("UI Panels")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("UI Elements")]
    [SerializeField] private Image deathPortraitImage;

    private void Awake()
    {
        if (instance == null) instance = this;
        
        // مطمئن می‌شیم اول بازی منو مخفی باشه
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void ShowGameOverScreen(PlayerPortraitData deathData)
    {
        if (gameOverPanel == null) return;

        // فعال کردن پنل مرگ روی صفحه
        gameOverPanel.SetActive(true);

        // لود کردن عکس مرگ مخصوص تفنگی که دست پلیر بوده
        if (deathPortraitImage != null && deathData != null && deathData.dead != null)
        {
            deathPortraitImage.sprite = deathData.dead;
        }

        // استاپ کردن زمان بازی و آزاد کردن موس برای کلیک
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // عملکرد دکمه کانتینیو (لود کردن دوباره همین مرحله از اول)
    public void ContinueGame()
    {
        Time.timeScale = 1f;
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.SpawnAtCheckpoint();
        }
    }

    // عملکرد دکمه منوی اصلی
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        // حواست باشه اسم سین منوی اصلیت رو به جای "MainMenu" این‌جا بنویسی
        SceneManager.LoadScene("MainMenu"); 
    }
}