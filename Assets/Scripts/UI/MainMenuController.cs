using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Management")]
    [Tooltip("Type the exact name of your gameplay scene to load when clicking Play.")]
    public string gameplaySceneName = "GameplayScene";

    [Header("Optional UI Panels")]
    [Tooltip("Drag your Settings panel GameObject here if you have one.")]
    public GameObject settingsPanel;
    [Tooltip("Drag your Credits panel GameObject here if you have one.")]
    public GameObject creditsPanel;

    private void Start()
    {
        // Ensure panels are hidden when the game starts
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    /// <summary>
    /// Loads the gameplay scene.
    /// </summary>
    public void PlayGame()
    {
        // Option A: Load by scene name specified in the Inspector
        if (!string.IsNullOrEmpty(gameplaySceneName))
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            // Option B: Fallback to load the next scene in the Build Index queue
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
    /// Toggles the visibility of the Settings Panel.
    /// </summary>
    public void ToggleSettings(bool isOpen)
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(isOpen);
        }
    }

    /// <summary>
    /// Toggles the visibility of the Credits Panel.
    /// </summary>
    public void ToggleCredits(bool isOpen)
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(isOpen);
        }
    }
}