using UnityEngine;
using System.Collections;

public class Location : MonoBehaviour
{
    [Tooltip("Place empty GameObjects in the scene and assign them here as spawn points. If empty this GameObject's transform will be used.")]
    public Transform[] spawnPoints;

    [Tooltip("Start index into spawnPoints (0-based). If negative, uses this GameObject's transform.")]
    public int startIndex = 0;

    [Tooltip("If true, pick a random spawn point from spawnPoints on start.")]
    public bool randomizeOnStart = false;

    [Tooltip("If true, attempt to find a GameObject tagged 'Player' after spawning and assign it (useful when this script is on a player prefab).")]
    public bool assignTagToSelf = false;

    [Tooltip("If true and a Rigidbody exists, temporarily set isKinematic while placing to avoid physics pushing the object away.")]
    public bool temporarilyKinematicOnSpawn = true;

    void Awake()
    {
        Transform chosen = transform;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            if (randomizeOnStart)
                chosen = spawnPoints[Random.Range(0, spawnPoints.Length)];
            else
            {
                int idx = Mathf.Clamp(startIndex, 0, spawnPoints.Length - 1);
                chosen = spawnPoints[idx];
            }
        }

        // Apply position and rotation immediately so other Start() calls see correct transform
        transform.position = chosen.position;
        transform.rotation = chosen.rotation;

        // If physics exists, stabilize placement
        var rb = GetComponent<Rigidbody>();
        if (rb != null && temporarilyKinematicOnSpawn)
        {
            StartCoroutine(PlaceWithTempKinematic(rb));
        }
        else if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (assignTagToSelf)
            gameObject.tag = "Player";

        Debug.Log($"Location: spawned '{name}' at {transform.position} (source: {(chosen == transform ? "self" : chosen.name)})");
    }

    private IEnumerator PlaceWithTempKinematic(Rigidbody rb)
    {
        bool originalKinematic = rb.isKinematic;
        rb.isKinematic = true;
        // wait one fixed update to let physics settle
        yield return new WaitForFixedUpdate();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = originalKinematic;
    }

    // Optional: draw gizmo at assigned spawn points and the chosen start index for clarity in the Scene view
#if UNITY_EDITOR
void OnDrawGizmosSelected()
{
    if (spawnPoints == null) return;

    Gizmos.color = Color.cyan;
    for (int i = 0; i < spawnPoints.Length; i++)
    {
        if (spawnPoints[i] == null) continue;

        Gizmos.DrawWireSphere(spawnPoints[i].position, 0.25f);
        UnityEditor.Handles.Label(
            spawnPoints[i].position + Vector3.up * 0.2f,
            $"Spawn[{i}]"
        );
    }
}
#endif

}
