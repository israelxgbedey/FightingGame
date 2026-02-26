using UnityEngine;

public class Shooter2D : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float fireRate = 5f;
    public float projectileLifetime = 3f;

    [Header("Spawn Point")]
    public Transform projectileSpawnPoint;

    [Header("Ammo Settings")]
    public int maxAmmo = 10;
    public float reloadTime = 2f;
    public bool autoReload = true;

    [Header("Direction Detection")]
    [Tooltip("Detect direction from transform.localScale.x")]
    public bool detectFromScale = true;

    private int currentAmmo;
    private float fireCooldown = 0f;
    private bool isReloading = false;
    private int facingDirection = 1;

    void Start()
    {
        currentAmmo = maxAmmo;
        UpdateFacingDirection();
    }

    void Update()
    {
        if (isReloading)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
            return;
        }

        UpdateFacingDirection();
        fireCooldown -= Time.deltaTime;

        if (Input.GetKey(KeyCode.E) && fireCooldown <= 0f)
        {
            if (currentAmmo > 0)
            {
                Shoot();
                fireCooldown = fireRate > 0 ? 1f / fireRate : 0f;
            }
            else if (autoReload)
            {
                StartCoroutine(Reload());
            }
        }
    }

    void UpdateFacingDirection()
    {
        if (detectFromScale)
        {
            // Get direction from scale (negative scale.x = facing left)
            facingDirection = transform.localScale.x >= 0 ? 1 : -1;
        }
        else
        {
            // Detect from input
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                facingDirection = horizontalInput > 0 ? 1 : -1;
            }
        }
    }

    void Shoot()
    {
        currentAmmo--;

        Transform spawn = projectileSpawnPoint != null ? projectileSpawnPoint : transform;
        Vector3 spawnPosition = spawn.position;
        
        // Calculate direction
        Vector2 shootDirection = Vector2.right * facingDirection;
        
        // Calculate rotation
        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // Spawn projectile
        GameObject proj = Instantiate(projectilePrefab, spawnPosition, rotation);
        Destroy(proj, projectileLifetime);

        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = shootDirection * projectileSpeed;
        }
    }

    System.Collections.IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log("Reload complete!");
    }
}