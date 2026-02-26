using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Add this for UI components
using System.Collections;
using System.Collections.Generic;

public class PausedMenu : MonoBehaviour
{
    public static bool isPaused = false;
    public GameObject pauseMenuPrefab;
    private GameObject pauseMenuInstance;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (pauseMenuPrefab != null && pauseMenuInstance == null)
        {
            // Instantiate the pause menu Canvas
            pauseMenuInstance = Instantiate(pauseMenuPrefab);

            // Automatically connect buttons
            SetupButtons();
        }

        Time.timeScale = 0f;
        isPaused = true;
    }

    void SetupButtons()
    {
        if (pauseMenuInstance == null) return;

        // Find all buttons in the pause menu
        Button[] buttons = pauseMenuInstance.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            // Connect based on button name or text
            if (button.name.Contains("Resume") || button.name.Contains("resume"))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(ResumeGame);
            }
            else if (button.name.Contains("Menu") || button.name.Contains("menu") ||
                     button.name.Contains("Main") || button.name.Contains("main"))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(LoadMainMenu);
            }
            else if (button.name.Contains("Quit") || button.name.Contains("quit") ||
                     button.name.Contains("Exit") || button.name.Contains("exit"))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(QuitGame);
            }
        }
    }

    public void ResumeGame()
    {
        if (pauseMenuInstance != null)
        {
            Destroy(pauseMenuInstance);
        }

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Make sure to reset time
        SceneManager.LoadScene("MainMenu"); // Replace with your menu scene name
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Game Quit"); // For testing in editor
    }
}