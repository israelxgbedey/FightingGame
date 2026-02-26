using UnityEngine;

public class GoodieManager : MonoBehaviour
{
    [Header("Goodie Settings")]
    public GameObject[] goodies;          // List of objects to drop
    public float dropChance = 0.5f;       // 0.0 to 1.0 (50% chance)
    public float spawnOffsetY = 0.5f;     // Lifts it slightly above ground
    
    // Call this method whenever you want to drop a goodie
    public void DropGoodie(Vector3 position)
    {
        // Roll chance
        float roll = Random.value;
        if (roll > dropChance)
        {
            return; // No drop
        }

        // Pick a random object from the list
        if (goodies.Length == 0)
        {
            Debug.LogWarning("Goodie list is empty!");
            return;
        }

        int randomIndex = Random.Range(0, goodies.Length);
        GameObject goodieToSpawn = goodies[randomIndex];

        // Spawn object at position
        Vector3 spawnPos = position + new Vector3(0, spawnOffsetY, 0);
        Instantiate(goodieToSpawn, spawnPos, Quaternion.identity);
    }
}
