using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI Settings")]
    public Vector3 offset = new Vector3(0, 2f, 0);
    public Color textColor = Color.white;
    public int fontSize = 24;
    
    [Header("Follow Target")]
    public Transform targetToFollow; // Assign the player/object to follow in Inspector

    private TMP_Text healthText;
    private RectTransform healthRectTransform;
    private GameObject healthObject;
    private Canvas canvas;
    private Vector3 lastTargetPosition;

    void Start()
    {
        currentHealth = maxHealth;
        
        // If no target specified, use this object
        if (targetToFollow == null)
        {
            targetToFollow = transform;
            Debug.LogWarning("No target specified for health UI. Using current object as target.");
        }
        
        CreateWorldSpaceCanvas();
        UpdateHealthUI();
        
        // Set initial position
        if (targetToFollow != null)
        {
            lastTargetPosition = targetToFollow.position;
            canvas.transform.position = targetToFollow.position + offset;
        }
    }

    private void CreateWorldSpaceCanvas()
    {
        // Create Canvas
        GameObject canvasObject = new GameObject("HealthCanvas");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        // Set camera
        if (Camera.main != null)
        {
            canvas.worldCamera = Camera.main;
        }
        else
        {
            Camera foundCamera = FindObjectOfType<Camera>();
            if (foundCamera != null)
                canvas.worldCamera = foundCamera;
        }
        
        canvasObject.AddComponent<CanvasScaler>();

        // Remove GraphicRaycaster for performance
        GraphicRaycaster raycaster = canvasObject.GetComponent<GraphicRaycaster>();
        if (raycaster != null)
            DestroyImmediate(raycaster);

        // Configure Canvas RectTransform - make it smaller and more manageable
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(100, 50); // Smaller size
        canvasObject.transform.localScale = Vector3.one * 0.005f; // Smaller scale

        // Create Text Object
        healthObject = new GameObject("HealthText");
        healthRectTransform = healthObject.AddComponent<RectTransform>();
        healthObject.transform.SetParent(canvasObject.transform);
        
        // Center the text
        healthRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        healthRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        healthRectTransform.pivot = new Vector2(0.5f, 0.5f);
        healthRectTransform.anchoredPosition = Vector2.zero;
        healthRectTransform.sizeDelta = new Vector2(100, 30);

        // Create Text Component
        healthText = healthObject.AddComponent<TextMeshProUGUI>();
        healthText.text = currentHealth.ToString();
        healthText.fontSize = fontSize;
        healthText.color = textColor;
        healthText.alignment = TextAlignmentOptions.Center;
        healthText.fontStyle = FontStyles.Bold;
    }

    void Update()
    {
        FollowTarget();
    }

    void LateUpdate()
    {
        FollowTarget();
        
        // Face camera (billboarding)
        if (canvas != null && canvas.worldCamera != null)
        {
            canvas.transform.rotation = canvas.worldCamera.transform.rotation;
        }
    }

    void FixedUpdate()
    {
        // Extra position sync in FixedUpdate for physics frames
        FollowTarget();
    }

   private void FollowTarget()
{
    if (canvas != null && targetToFollow != null)
    {
        // Force absolute position every frame, no optimizations
        canvas.transform.position = targetToFollow.position + offset;
        
        // Completely override any other transformations
        canvas.transform.SetPositionAndRotation(
            targetToFollow.position + offset,
            canvas.worldCamera != null ? canvas.worldCamera.transform.rotation : Quaternion.identity
        );
    }
}
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    public void SetTarget(Transform newTarget)
    {
        targetToFollow = newTarget;
        if (targetToFollow != null)
        {
            lastTargetPosition = targetToFollow.position;
        }
    }

    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = currentHealth.ToString();
        }
    }

    void OnDestroy()
    {
        if (canvas != null)
        {
            Destroy(canvas.gameObject);
        }
    }
}