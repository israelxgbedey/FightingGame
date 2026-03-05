using UnityEngine;
using System.Collections.Generic;

public class GroupSelfDestruct : MonoBehaviour
{
    public float minLifetime = 2f;
    public float maxLifetime = 4f;

    // Shared across all clones
    private static List<GameObject> allClones = new List<GameObject>();

    void Awake()
    {
        allClones.Add(gameObject);
    }

    void Start()
    {
        float randomTime = Random.Range(minLifetime, maxLifetime);
        Invoke(nameof(DestroyAllClones), randomTime);
    }

    void DestroyAllClones()
    {
        foreach (GameObject obj in allClones)
        {
            if (obj != null)
                Destroy(obj);
        }

        allClones.Clear();
    }

    void OnDestroy()
    {
        allClones.Remove(gameObject);
    }
}