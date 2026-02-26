using UnityEngine;

public class ApplySpawnPoint : MonoBehaviour
{
    public Transform[] spawnPoints; // THESE ARE IN THE GAME SCENE

    private void Start()
    {
        int index = PlayerPrefs.GetInt("SpawnPointIndex", 0);

        if (spawnPoints.Length == 0)
        {
            Debug.LogError("Assign game scene spawn points to ApplySpawnPoint!");
            return;
        }

        Transform spawn = spawnPoints[index];

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length >= 1)
            players[0].transform.position = spawn.position + Vector3.left * 1.5f;
        if (players.Length >= 2)
            players[1].transform.position = spawn.position + Vector3.right * 1.5f;

        Debug.Log("Players spawned at game spawn: " + spawn.name);
    }
}
