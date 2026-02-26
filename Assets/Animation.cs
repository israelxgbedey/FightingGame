using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple state-only animation switcher: idle, walk, run, jump, attack, hit (Get Hit).
/// Assign the GameObjects you want shown for each state in the inspector.
/// Call TriggerHit() from gameplay code to show the "Get Hit" object for a short time.
/// </summary>
public class PlayerAnimation : MonoBehaviour
{
    [Header("Ground check (optional, used to detect jump)")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("State objects (assign one GameObject per state)")]
    public GameObject idle;
    public GameObject walk;
    public GameObject run;
    public GameObject jump;
    public GameObject attack;
    public GameObject getHit;

    [Header("Settings")]
    [Tooltip("Movement magnitude (0..1) above which we consider 'moving'")]
    public float moveThreshold = 0.1f;
    [Tooltip("If LeftShift held (or movement magnitude > runThreshold) treat as run")]
    public float runThreshold = 0.9f;
    public float attackDuration = 1f;
    public float hitDuration = 0.5f;

    [Header("Jump animation")]
    [Tooltip("How long to show the jump state after the player presses jump (seconds)")]
    public float jumpDisplayTime = 0.25f;

    [Header("Attack System")]
    public GameObject attackObject;
    public float attackDistance = 1f;
    public int attackDamage = 10;
    public Vector2 attackSize = new Vector2(1f, 1f);
    
    [Header("Health System")]
    public int maxHealth = 100;
    public int currentHealth;
    
    [Header("Health Text Settings")]
    [Tooltip("Font size for the health text display")]
    public int healthTextFontSize = 14;
    [Tooltip("Offset position above the player")]
    public Vector3 healthTextOffset = new Vector3(0, 1.5f, 0);
    [Tooltip("Background size for the health text")]
    public Vector2 healthBackgroundSize = new Vector2(60, 20);

    // internal
    private List<GameObject> allObjects;
    private float attackTimer = 0f;
    private float hitTimer = 0f;
    private float jumpTimer = 0f;

    // facing / flip
    private bool facingRight = true;
    private bool prevFacingRight = true;

    // track vertical motion without requiring a Rigidbody2D
    private float prevY;
    private const float ascendThreshold = 0.05f; // units/sec threshold to consider "ascending"

    // Health text components (exactly like Move script)
    private TMPro.TextMeshProUGUI healthText;
    private Canvas healthCanvas;
    private RectTransform healthTextTransform;
    private UnityEngine.UI.Image healthBackground;
    private GameObject healthCanvasObject;

    // Health state
    private bool isTakingDamage = false;

    // Attack variables (from Move script)
    private bool isAttacking = false;
    private GameObject currentAttackInstance;

    // Added: determine if this is the second player
    private bool useUHJKControls = false;

    void Start()
    {
        allObjects = new List<GameObject> { idle, walk, run, jump, attack, getHit };
        allObjects.RemoveAll(g => g == null);

        prevFacingRight = facingRight;
        ApplyFlip(facingRight);

        prevY = transform.position.y;

        // Initialize health (exactly like Move script)
        currentHealth = maxHealth;
        CreateHealthText();

        // Initialize attack object
        InitializeAttackObject();

        // Determine control scheme
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 2)
        {
            if (players[1] == this.gameObject)
                useUHJKControls = true;
        }
    }

void InitializeAttackObject()
{
    // Only create attack object at runtime if it doesn't exist
    if (attackObject == null)
    {
        attackObject = new GameObject("AttackObject");
        attackObject.transform.SetParent(transform);
        attackObject.transform.localPosition = Vector3.zero;
        
        BoxCollider2D boxCollider = attackObject.AddComponent<BoxCollider2D>();
        boxCollider.size = attackSize;
        boxCollider.isTrigger = true;
        
        Rigidbody2D attackRb = attackObject.AddComponent<Rigidbody2D>();
attackRb.bodyType = RigidbodyType2D.Kinematic;

        
        AttackTriggerHandler triggerHandler = attackObject.AddComponent<AttackTriggerHandler>();
        triggerHandler.playerAnimation = this; // Set this reference instead of playerMove
        
        attackObject.SetActive(false);
    }
    // If attackObject is a prefab reference, we'll instantiate it when needed
}
    void CreateHealthText()
    {
        healthCanvasObject = new GameObject("HealthCanvas");
        healthCanvasObject.transform.SetParent(transform);
        healthCanvas = healthCanvasObject.AddComponent<Canvas>();
        healthCanvas.renderMode = RenderMode.WorldSpace;
        healthCanvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        RectTransform canvasRect = healthCanvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2, 1);
        canvasRect.localPosition = healthTextOffset;
        canvasRect.localScale = new Vector3(0.02f, 0.02f, 0.02f);

        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(healthCanvasObject.transform);
        healthText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        healthTextTransform = textObj.GetComponent<RectTransform>();

        healthTextTransform.sizeDelta = new Vector2(100, 30);
        healthTextTransform.localPosition = Vector3.zero;
        healthText.alignment = TMPro.TextAlignmentOptions.Center;
        healthText.fontSize = healthTextFontSize;
        healthText.color = Color.white;
        healthText.fontStyle = TMPro.FontStyles.Bold;
        healthText.enableAutoSizing = false;
        healthText.overflowMode = TMPro.TextOverflowModes.Overflow;
        healthText.outlineWidth = 0.1f;
        healthText.outlineColor = Color.black;

        GameObject bgObj = new GameObject("TextBackground");
        bgObj.transform.SetParent(healthCanvasObject.transform);
        healthBackground = bgObj.AddComponent<UnityEngine.UI.Image>();
        healthBackground.color = new Color(0, 0, 0, 0.7f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.sizeDelta = healthBackgroundSize;
        bgRect.localPosition = Vector3.zero;
        bgObj.transform.SetAsFirstSibling();

        UpdateHealthText();
    }

    void LateUpdate()
    {
        if (healthCanvasObject != null)
        {
            Vector3 canvasScale = healthCanvasObject.transform.localScale;
            if (transform.localScale.x < 0)
            {
                canvasScale.x = Mathf.Abs(canvasScale.x) * -1f;
            }
            else
            {
                canvasScale.x = Mathf.Abs(canvasScale.x);
            }
            healthCanvasObject.transform.localScale = canvasScale;
        }
    }

    void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
            float healthPercent = (float)currentHealth / maxHealth;
            if (healthPercent > 0.6f)
                healthText.color = Color.green;
            else if (healthPercent > 0.3f)
                healthText.color = Color.yellow;
            else
                healthText.color = Color.red;
        }
    }

    void Update()
    {
        if (isTakingDamage) return;

        // Handle attack state and timer
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                isAttacking = false;
                if (currentAttackInstance != null)
                {
                    Destroy(currentAttackInstance);
                    currentAttackInstance = null;
                }
            }
        }

        // timers
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;
        if (hitTimer > 0f) hitTimer -= Time.deltaTime;
        if (jumpTimer > 0f) jumpTimer -= Time.deltaTime;

        // ground check (still available for other logic but NOT used to force jump state)
        bool isGrounded = true;
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;

        // input
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // fallback keys if axis not configured
        if (Mathf.Approximately(x, 0f))
        {
            if (Input.GetKey(KeyCode.A)) x = -1f;
            else if (Input.GetKey(KeyCode.D)) x = 1f;
        }
        if (Mathf.Approximately(y, 0f))
        {
            if (Input.GetKey(KeyCode.S)) y = -1f;
            else if (Input.GetKey(KeyCode.W)) y = 1f;
        }

        // update facing based on horizontal input (remember last non-zero horizontal)
        if (!Mathf.Approximately(x, 0f))
        {
            facingRight = x > 0f;
        }

        // apply flip only when changed
        if (facingRight != prevFacingRight)
        {
            ApplyFlip(facingRight);
            prevFacingRight = facingRight;
        }

        // Only consider A/D (horizontal input) for walk/run states:
        bool horizontalKey = !Mathf.Approximately(x, 0f); // true when pressing A or D (or axis non-zero)
        bool moving = horizontalKey; // walk object should play when A or D pressed
        // Run only when A/D is pressed AND LeftShift is held (the "switch" paired with A/D)
        bool running = horizontalKey && Input.GetKey(KeyCode.LeftShift);

        // attack input: Fire1, J, K or E key
        bool attackInput = false;
        if (useUHJKControls)
        {
            if (Input.GetKeyDown(KeyCode.J) && !isAttacking)
            {
                attackInput = true;
            }
        }
        else
        {
            if ((Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Z)) && !isAttacking)
            {
                attackInput = true;
            }
        }

        if (attackInput)
        {
            StartAttack();
        }

        // jump animation trigger when player presses Space (or configured "Jump" button)
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
        {
            jumpTimer = jumpDisplayTime;
        }

        // compute vertical speed from transform (works without Rigidbody2D)
        float verticalSpeed = 0f;
        if (Time.deltaTime > 0f)
            verticalSpeed = (transform.position.y - prevY) / Time.deltaTime;
        prevY = transform.position.y;

        // priority: hit > attack > jump (pressed or ascending) > run/walk > idle
        GameObject toShow = idle;

        if (hitTimer > 0f)
        {
            toShow = getHit ?? idle;
        }
        else if (isAttacking || attackTimer > 0f)
        {
            toShow = attack ?? idle;
        }
        else if (jumpTimer > 0f) // show jump immediately after pressing jump/space
        {
            toShow = jump ?? idle;
        }
        else if (verticalSpeed > ascendThreshold) // only show jump when ascending, not when falling
        {
            toShow = jump ?? idle;
        }
        else if (moving)
        {
            toShow = running ? (run ?? walk ?? idle) : (walk ?? idle);
        }
        else
        {
            toShow = idle;
        }

        ActivateOnly(toShow);

        // Reposition attack object during attack to stay in front of player
        if (isAttacking && currentAttackInstance != null)
        {
            Vector3 attackPosition = transform.position + 
                (facingRight ? Vector3.right : Vector3.left) * attackDistance;
            currentAttackInstance.transform.position = attackPosition;
        }
    }

    void StartAttack()
    {
        if (!isAttacking && !isTakingDamage)
        {
            isAttacking = true;
            attackTimer = attackDuration;
            ActivateAttackObject();
        }
    }

void ActivateAttackObject()
{
    // Clean up any existing attack instance
    if (currentAttackInstance != null)
    {
        Destroy(currentAttackInstance);
    }

    Vector3 attackPosition = transform.position + 
        (facingRight ? Vector3.right : Vector3.left) * attackDistance;

    // Check if attackObject is a prefab or scene object
    if (attackObject.scene.rootCount == 0) // It's a prefab
    {
        // Instantiate the prefab
        currentAttackInstance = Instantiate(attackObject, attackPosition, Quaternion.identity);
    }
    else // It's a scene object
    {
        // Use the existing scene object
        currentAttackInstance = attackObject;
        currentAttackInstance.transform.position = attackPosition;
        currentAttackInstance.SetActive(true);
    }

    // Set up the attack instance
    currentAttackInstance.transform.rotation = Quaternion.identity;
    currentAttackInstance.transform.localScale = Vector3.one;

    // Ensure it has the necessary components
    AttackTriggerHandler triggerHandler = currentAttackInstance.GetComponent<AttackTriggerHandler>();
    if (triggerHandler == null)
    {
        triggerHandler = currentAttackInstance.AddComponent<AttackTriggerHandler>();
        triggerHandler.playerAnimation = this; // Set this reference
    }
    else
    {
        triggerHandler.playerAnimation = this; // Update the reference
        triggerHandler.playerMove = null; // Clear the other reference
    }

    Collider2D collider = currentAttackInstance.GetComponent<Collider2D>();
    if (collider == null)
    {
        BoxCollider2D boxCollider = currentAttackInstance.AddComponent<BoxCollider2D>();
        boxCollider.size = attackSize;
        boxCollider.isTrigger = true;
    }

    Rigidbody2D attackRb = currentAttackInstance.GetComponent<Rigidbody2D>();
    if (attackRb == null)
    {
        attackRb = currentAttackInstance.AddComponent<Rigidbody2D>();
attackRb.bodyType = RigidbodyType2D.Kinematic;

    }
}

    public void OnAttackHit(Collider2D other)
    {
        if (other.CompareTag("Player") && other.gameObject != this.gameObject)
        {
            PlayerAnimation otherPlayer = other.GetComponent<PlayerAnimation>();
            if (otherPlayer != null && !otherPlayer.isTakingDamage)
            {
                Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                Vector2 knockback = knockbackDirection * 5f;
                otherPlayer.TakeDamage(attackDamage, knockback);
            }
        }
    }

    /// <summary>
    /// Apply damage to the player (exactly like Move script)
    /// </summary>
    public void TakeDamage(int damage, Vector2? knockback = null)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthText();
        
        if (!isTakingDamage)
        {
            StartCoroutine(HandleDamage(knockback));
        }
    }

    System.Collections.IEnumerator HandleDamage(Vector2? knockback)
    {
        isTakingDamage = true;
        
        // Apply knockback if provided
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null && knockback.HasValue)
            rb.linearVelocity = knockback.Value;

        // Trigger hit animation
        TriggerHit();

        // Wait for hit duration
        yield return new WaitForSeconds(hitDuration);

        isTakingDamage = false;
        
        // Return to idle state
        if (idle != null)
        {
            ActivateOnly(idle);
        }
    }

    /// <summary>
    /// Heal the player (exactly like Move script)
    /// </summary>
    public void Heal(int healAmount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        UpdateHealthText();
    }

    // Call this from other scripts when the player gets hit
    public void TriggerHit(float duration = -1f)
    {
        hitTimer = (duration > 0f) ? duration : hitDuration;
    }

    // Optional: allow external code to trigger the attack animation
    public void TriggerAttack(float duration = -1f)
    {
        if (!isAttacking && !isTakingDamage)
        {
            isAttacking = true;
            attackTimer = (duration > 0f) ? duration : attackDuration;
            ActivateAttackObject();
        }
    }

    // Auto-detect the attack animation length; fallback to inspector attackDuration
    private float GetAttackDuration()
    {
        if (attack != null)
        {
            var animator = attack.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                var clips = animator.runtimeAnimatorController.animationClips;
                if (clips != null && clips.Length > 0)
                {
                    // prefer clip with "attack" in the name, otherwise return the longest clip
                    AnimationClip best = null;
                    foreach (var c in clips)
                    {
                        if (c == null) continue;
                        if (best == null || c.length > best.length) best = c;
                        if (c.name.ToLower().Contains("attack")) return c.length;
                    }
                    if (best != null) return best.length;
                }
            }

            var legacy = attack.GetComponent<Animation>();
            if (legacy != null)
            {
                foreach (AnimationState st in legacy)
                {
                    if (st.clip != null)
                    {
                        if (st.clip.name.ToLower().Contains("attack")) return st.clip.length;
                        return st.clip.length;
                    }
                }
            }
        }

        return attackDuration;
    }

    void ActivateOnly(GameObject obj)
    {
        foreach (var go in allObjects)
        {
            if (go == null) continue;
            go.SetActive(go == obj);
        }
    }

    // Flip sprite renderers on states to face left/right.
    void ApplyFlip(bool facingRight)
    {
        bool flip = !facingRight;
        foreach (var go in allObjects)
        {
            if (go == null) continue;
            var srs = go.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in srs)
            {
                sr.flipX = flip;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
        // Draw attack range
        Gizmos.color = Color.red;
        Vector3 attackPosition = transform.position + 
            (facingRight ? Vector3.right : Vector3.left) * attackDistance;
        Gizmos.DrawWireCube(attackPosition, new Vector3(attackSize.x, attackSize.y, 0));
    }
}

