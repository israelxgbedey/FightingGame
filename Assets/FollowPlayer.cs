using UnityEngine;

public class FollowAllPlayersInstant : MonoBehaviour
{
    [Tooltip("Offset from the players.")]
    public Vector3 offset = new Vector3(0f, 1f, -10f);

    [Tooltip("Optional axis locks.")]
    public bool lockX = false;
    public bool lockY = false;

    void LateUpdate()
    {
        // Find all players
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) return;

        // Compute center position
        Vector3 center = Vector3.zero;
        foreach (var player in players)
        {
            center += player.transform.position;
        }
        center /= players.Length;

        // Add offset
        Vector3 desiredPosition = center + offset;

        // Apply axis locks
        Vector3 current = transform.position;
        if (lockX) desiredPosition.x = current.x;
        if (lockY) desiredPosition.y = current.y;

        // Directly move camera
        transform.position = desiredPosition;
    }
}
