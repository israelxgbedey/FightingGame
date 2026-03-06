using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DeathScreenManager : MonoBehaviour
{
    [Header("Death Screen Settings")]
    public string sceneName;             // Type the scene name here in Inspector
    public GameObject deathScreenPrefab; // Assign the prefab in Inspector

    [Header("UI Elements")]
    [Tooltip("Optional: Text component to display which character died")]
    public TMP_Text deathMessageText;    // Assign in Inspector if your prefab has this

    private Move[] players;
    private bool gameOver = false;
    private GameObject deathScreenInstance;

    void Start()
    {
        // Find all players in the active scene
        players = FindObjectsOfType<Move>();
        
        if (players.Length == 0)
        {
            Debug.LogError("DeathScreenManager: No players found in scene!");
            return;
        }

        Debug.Log($"DeathScreenManager found {players.Length} players");

        // Optional: automatically find the scene by name
        Scene targetScene = SceneManager.GetSceneByName(sceneName);

        if (!targetScene.IsValid())
        {
            Debug.LogWarning("DeathScreenManager: Scene not found: " + sceneName + ". Using current scene instead.");
            targetScene = SceneManager.GetActiveScene();
        }

        // Instantiate the prefab in that scene
        if (deathScreenPrefab != null)
        {
          deathScreenInstance = Instantiate(deathScreenPrefab);
deathScreenInstance.SetActive(false);

// ALWAYS get the TMP text from the instance
deathMessageText = deathScreenInstance.GetComponentInChildren<TMP_Text>(true);

if (deathMessageText == null)
{
    Debug.LogError("DeathScreenManager: No TMP_Text found in the death screen prefab!");
}

            // Move to target scene
            SceneManager.MoveGameObjectToScene(deathScreenInstance, targetScene);
        }
        else
        {
            Debug.LogError("DeathScreenManager: deathScreenPrefab is not assigned!");
        }
    }

    void Update()
    {
        if (gameOver || players == null || players.Length == 0) return;

        foreach (Move player in players)
        {
            if (player != null && player.isDead)
            {
                TriggerGameOver(player);
                break;
            }
        }
    }

void TriggerGameOver(Move deadPlayer)
{
    gameOver = true;

    if (deathScreenInstance != null)
    {
        deathScreenInstance.SetActive(true);

        if (deathMessageText != null && deadPlayer != null)
        {
         string characterName = deadPlayer.PlayerName;

if (string.IsNullOrEmpty(characterName))
    characterName = deadPlayer.gameObject.name;

deathMessageText.SetText(characterName + " has been defeated!");
deathMessageText.ForceMeshUpdate();
            Debug.Log(characterName + " died");
        }

        Canvas.ForceUpdateCanvases();
    }

    Time.timeScale = 0f;
}
    // Optional: Public method to get the death screen instance
    public GameObject GetDeathScreenInstance()
    {
        return deathScreenInstance;
    }
}