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

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void ShowGameOverScreen(PlayerPortraitData deathData)
    {
        if (gameOverPanel == null) return;

        gameOverPanel.SetActive(true);

        if (deathPortraitImage != null && deathData != null && deathData.dead != null)
        {
            deathPortraitImage.sprite = deathData.dead;
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ContinueGame()
    {
        Time.timeScale = 1f;

        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.SpawnAtCheckpoint();
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}