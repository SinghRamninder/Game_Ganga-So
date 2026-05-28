using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    // Global variable so other scripts (like weapon firing/camera movement) can easily check if the game is paused
    public static bool IsGamePaused = false;

    [Header("UI References")]
    [Tooltip("Drag your Pause Menu Panel GameObject here.")]
    public GameObject pauseMenuPanel;
    [Tooltip("Drag your Settings Panel GameObject here if you have one.")]
    public GameObject settingsPanel;

    [Header("Scene Management")]
    [Tooltip("Type the exact name of your Main Menu scene to load when clicking Quit to Menu.")]
    public string mainMenuSceneName = "MainMenu";

    [Header("First-Person Shooter Cursor Control")]
    [Tooltip("Check this if you want the script to automatically hide the cursor while playing, and show it while paused.")]
    public bool autoManageCursor = true;

    private void Start()
    {
        // Force the game to start unpaused and ensure UI panels are in the correct state
        Resume();
    }

    private void Update()
    {
        // Listen for Escape or 'P' key to toggle Pause
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (IsGamePaused)
            {
                // If settings are open, close settings first instead of resuming immediately
                if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    ToggleSettings(false);
                }
                else
                {
                    Resume();
                }
            }
            else
            {
                Pause();
            }
        }
    }

    /// <summary>
    /// Resumes normal gameplay. Unfreezes time and hides the pause screen.
    /// </summary>
    public void Resume()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        Time.timeScale = 1f; // Run time at normal speed
        IsGamePaused = false;

        // Hide and lock cursor for first-person gameplay
        if (autoManageCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// Pauses the game. Freezes time and shows the pause screen.
    /// </summary>
    public void Pause()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        Time.timeScale = 0f; // Freeze all physics, movement, and game-time
        IsGamePaused = true;

        // Unlock and show cursor so the player can select buttons
        if (autoManageCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// Toggles the visibility of the Settings Panel inside the pause overlay.
    /// </summary>
    public void ToggleSettings(bool isOpen)
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(isOpen);
        }
        
        // Hide/Show main pause buttons background panel when settings opens/closes
        if (pauseMenuPanel != null)
        {
            // If settings is open, we can hide the main list of buttons so they don't overlap
            pauseMenuPanel.SetActive(!isOpen);
        }
    }

    /// <summary>
    /// Loads the main menu.
    /// </summary>
    public void LoadMainMenu()
    {
        // IMPORTANT: Always set timeScale back to 1 before changing scenes, or the Main Menu will be frozen!
        Time.timeScale = 1f;
        IsGamePaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Closes the built desktop application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting built application...");
        Application.Quit();
    }
}