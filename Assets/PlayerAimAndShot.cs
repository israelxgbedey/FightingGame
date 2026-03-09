using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerAimAndShot : MonoBehaviour
{
    [Header("Rotation Settings")]
    public string gunTag = "Gun";                 
    public Camera mainCamera;                     
    public float rotationSpeed = 15f;             
    public float minAngle = -90f; // Minimum rotation angle when facing right
    public float maxAngle = 90f;  // Maximum rotation angle when facing right

[Header("Shooting Settings")]
public GameObject projectilePrefab;           
public float projectileSpeed = 10f;           
public float bulletSpawnDistance = 1f;
public Vector2 bulletSpawnOffset = Vector2.zero; // Additional X/Y offset
public bool useCustomSpawnPoint = false; // Use a specific transform as spawn point
public Transform customSpawnPoint; // Optional: specific spawn point transform      

    [Header("Parent Reference")]
    public Transform playerTransform; // Reference to the player (parent)

    private Transform gunTransform;
    private bool isFacingRight = true; // Track which direction the gun is facing
    private SpriteRenderer gunSpriteRenderer; // To flip the gun sprite if needed

    void Awake()
    {
        GameObject gunObj = GameObject.FindGameObjectWithTag(gunTag);
        if (gunObj != null)
        {
            gunTransform = gunObj.transform;
            gunSpriteRenderer = gunObj.GetComponent<SpriteRenderer>();
        }
        else
            Debug.LogError("Gun with tag '" + gunTag + "' not found!");

        if (mainCamera == null)
            Debug.LogError("Main camera not assigned in PlayerAimAndShot!");
            
        // If playerTransform not set, try to find parent
        if (playerTransform == null && transform.parent != null)
            playerTransform = transform.parent;
    }

    void Update()
    {
        if (gunTransform == null || mainCamera == null) return;

        AimGunAtMouse();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            ShootProjectile();
        }
    }

    void AimGunAtMouse()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(mouseScreenPos.x, mouseScreenPos.y, gunTransform.position.z - mainCamera.transform.position.z)
        );

        Vector3 direction = mouseWorldPos - gunTransform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Apply angle limits based on facing direction
            if (isFacingRight)
            {
                // Normal limits when facing right (-90 to 90)
                angle = Mathf.Clamp(angle, minAngle, maxAngle);
            }
            else
            {
                // Reversed limits when facing left (90 to 270 or -90 to -270)
                // This allows the gun to aim upward when facing left
                if (angle > 90)
                    angle = Mathf.Clamp(angle, 90, 270);
                else if (angle < -90)
                    angle = Mathf.Clamp(angle, -270, -90);
                else
                    angle = Mathf.Clamp(angle, 90, 270); // Default to upward range
            }

            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
            gunTransform.rotation = Quaternion.Lerp(gunTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            // Flip gun sprite if needed (when aiming behind)
            if (gunSpriteRenderer != null)
            {
                // Flip gun sprite when aiming backwards relative to player facing
                bool shouldFlip = (isFacingRight && (angle > 90 || angle < -90)) || 
                                 (!isFacingRight && (angle < 90 && angle > -90));
                gunSpriteRenderer.flipY = shouldFlip;
            }
        }
    }

void ShootProjectile()
{
    if (projectilePrefab == null) return;

    Vector3 spawnPos;
    
    // Use custom spawn point if specified
    if (useCustomSpawnPoint && customSpawnPoint != null)
    {
        spawnPos = customSpawnPoint.position;
    }
    else
    {
        // Calculate spawn position with offset
        Vector3 offset = new Vector3(bulletSpawnOffset.x, bulletSpawnOffset.y, 0f);
        // Transform the offset from local to world space
        offset = gunTransform.TransformDirection(offset);
        
        spawnPos = gunTransform.position + gunTransform.right * bulletSpawnDistance + offset;
    }

    GameObject proj = Instantiate(projectilePrefab, spawnPos, gunTransform.rotation);

    Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
    if (rb != null)
    {
        rb.linearVelocity = gunTransform.right * projectileSpeed;
    }
}

    // Public method called by Move script when player flips
    public void SetFacingDirection(bool facingRight)
    {
        isFacingRight = facingRight;
        
        // Optional: Instantly adjust gun position if needed
        // For example, if gun is a child object, you might need to adjust its local position
        if (gunTransform != null && playerTransform != null)
        {
            // Ensure gun is on the correct side of the player
            Vector3 localPos = gunTransform.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * (facingRight ? 1 : -1);
            gunTransform.localPosition = localPos;
        }
    }
}