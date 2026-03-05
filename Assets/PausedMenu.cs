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
    public GameObject quitConfirmPanel;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";
    public string characterSelectSceneName = "CharacterSelect"; // Add this

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
            string buttonName = button.name.ToLower();
            string buttonText = button.GetComponentInChildren<Text>()?.text.ToLower() ?? "";
            
            // Connect based on button name or text
            if (buttonName.Contains("resume") || buttonText.Contains("resume"))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(ResumeGame);
            }
            else if (buttonName.Contains("character") || buttonName.Contains("select") || 
                     buttonName.Contains("charsel") || buttonText.Contains("character") || 
                     buttonText.Contains("select"))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(LoadCharacterSelect);
            }
            else if (buttonName.Contains("menu") || buttonName.Contains("main") ||
                     buttonText.Contains("menu") || buttonText.Contains("main"))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(LoadMainMenu);
            }
            else if (buttonName.Contains("quit") || buttonName.Contains("exit") ||
                     buttonText.Contains("quit") || buttonText.Contains("exit"))
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
            pauseMenuInstance = null;
        }

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Make sure to reset time
        isPaused = false;
        
        // Clean up any spawned players if they exist
        CleanupPlayers();
        
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void LoadCharacterSelect()
    {
        Time.timeScale = 1f; // Make sure to reset time
        isPaused = false;
        
        // Clean up any spawned players if they exist
        CleanupPlayers();
        
        SceneManager.LoadScene(characterSelectSceneName);
    }

    private void CleanupPlayers()
    {
        // Find and destroy any player objects that might have been marked DontDestroyOnLoad
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Destroy(player);
        }

        // Also find and destroy any objects with the name "Player_1" or "Player_2"
        GameObject player1 = GameObject.Find("Player_1");
        if (player1 != null) Destroy(player1);
        
        GameObject player2 = GameObject.Find("Player_2");
        if (player2 != null) Destroy(player2);
    }

    public void QuitGame()
    {
        // Show confirmation panel instead of quitting immediately
        if (quitConfirmPanel != null)
        {
            quitConfirmPanel.SetActive(true);
            if (pauseMenuInstance != null)
            {
                pauseMenuInstance.SetActive(false);
            }
        }
        else
        {
            QuitConfirmed();
        }
    }

    public void QuitConfirmed()
    {
        // Resume time before quitting
        Time.timeScale = 1f;
        
        #if UNITY_EDITOR
            // If we're in the Unity Editor, stop playing
            UnityEditor.EditorApplication.isPlaying = false;
        #elif UNITY_WEBGL
            // WebGL doesn't support Application.Quit()
            Debug.Log("Game Quit - But WebGL doesn't support quitting");
            // You might want to redirect or show a message for WebGL
        #else
            // For standalone builds (Windows, Mac, Linux)
            Application.Quit();
        #endif
        
        Debug.Log("Game Quit");
    }

    public void CancelQuit()
    {
        if (quitConfirmPanel != null)
        {
            quitConfirmPanel.SetActive(false);
            if (pauseMenuInstance != null)
            {
                pauseMenuInstance.SetActive(true); // Show pause menu again
            }
        }
    }
}