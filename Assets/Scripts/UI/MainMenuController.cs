using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Management")]
    [Tooltip("Type the exact name of your gameplay scene to load when clicking Play.")]
    public string gameplaySceneName = "GameplayScene";

    [Header("UI Panels")]
    [Tooltip("Drag your Main Menu panel GameObject here.")]
    public GameObject mainMenuPanel;
    
    [Tooltip("Drag your Settings panel GameObject here if you have one.")]
    public GameObject settingsPanel;
    
    [Tooltip("Drag your Credits panel GameObject here if you have one.")]
    public GameObject creditsPanel;

    private void Start()
    {
        // Ensure only the main menu is visible when the game starts
        ShowMainMenu();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Loads the gameplay scene.
    /// </summary>
    public void PlayGame()
    {
        if (!string.IsNullOrEmpty(gameplaySceneName))
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    /// <summary>
    /// Closes the application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit Game button pressed! (This will close the built application window)");
        Application.Quit();
    }

    /// <summary>
    /// Opens Settings and hides the main menu.
    /// </summary>
    public void OpenSettings()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    /// <summary>
    /// Opens Credits and hides the main menu.
    /// </summary>
    public void OpenCredits()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    /// <summary>
    /// Returns back to the main menu panel.
    /// </summary>
    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }
}