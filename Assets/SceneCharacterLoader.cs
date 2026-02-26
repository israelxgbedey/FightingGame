using UnityEngine;

public class SceneCharacterLoader : MonoBehaviour
{
    public Transform spawnPoint; // optional: where to spawn

   void Start()
    {
        if (SelectedCharacterHolder.firstSelectedPrefab != null)
            Instantiate(SelectedCharacterHolder.firstSelectedPrefab, new Vector3(-2f, 0f, 0f), Quaternion.identity);

        if (SelectedCharacterHolder.secondSelectedPrefab != null)
            Instantiate(SelectedCharacterHolder.secondSelectedPrefab, new Vector3(2f, 0f, 0f), Quaternion.identity);
    }
}
