using UnityEngine;
using TMPro;

public class FloatingHealthText : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;           // The player or object to follow
    public Vector3 offset = new Vector3(0, 2f, 0);

    [Header("Text Settings")]
    public TMP_Text healthText;        // Assign TMP_Text component here
    public float textScale = 0.5f;     // Optional scale

    void Awake()
    {
        // Make sure text cannot interact with physics
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        Collider col = GetComponent<Collider>();
        if (col != null) Destroy(col);

        // Set initial scale
        transform.localScale = Vector3.one * textScale;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Follow the target position + offset
        transform.position = target.position + offset;

        // Face the main camera
        if (Camera.main != null)
        {
            Vector3 direction = transform.position - Camera.main.transform.position;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    // Optional: Update the text value
    public void SetText(string text)
    {
        if (healthText != null)
            healthText.text = text;
    }
}
