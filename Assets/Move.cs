using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class Move : MonoBehaviour
{

    [Header("Player Controls")]
public KeyCode leftKey = KeyCode.LeftArrow;
public KeyCode rightKey = KeyCode.RightArrow;
public KeyCode jumpKey = KeyCode.UpArrow;
public KeyCode attackKey = KeyCode.Z;

    [Header("Movement")]
    public float speed = 5f;
    public float jumpVelocity = 7f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.12f;
    public LayerMask groundLayer;

    [Header("Double Jump")]
    public bool enableDoubleJump = true;
    public int maxJumps = 2;
    public float doubleJumpVelocity = 7f;
    
    [Header("Sprite / Animation frames (assign PNG frames in order)")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] idleSprites;
    public Sprite[] runSprites;
    public Sprite[] jumpSprites;
    public Sprite[] attackSprites;
    public Sprite[] damageSprites;

    [Tooltip("Frames per second for sprite animations (idle/run/attack/damage)")]
    public float frameRate = 12f;

    [Tooltip("Frames per second specifically for the jump animation (set to 0 to use general frameRate)")]
    public float jumpFrameRate = 12f;

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
    
    [Header("Attack System")]
    public GameObject attackObject;
    public float attackDuration = 0.3f;
    public float attackDistance = 1f;
    public int attackDamage = 10;
    public Vector2 attackSize = new Vector2(1f, 1f);
    
    Rigidbody2D rb;
    bool facingRight = true;
    bool grounded;
    float frameTimer;
    int frameIndex;

    enum State { Idle, Run, Jump, Hit, Attack }
    State state = State.Idle;
    State lastState = State.Idle;

    float moveInput = 0f;
    bool jumpRequested = false;

    // Double jump variables
    int jumpsRemaining = 0;
    bool wasGrounded = false;

    // Attack variables
    bool isAttacking = false;
    float attackTimer = 0f;
    int attackFrameIndex = 0;

    // Hit variables
    bool isTakingDamage = false;

    // Added: determine if this is the second player
    bool useUHJKControls = false;

    // Health text components
    private Image healthBarFill;
 
    private Canvas healthCanvas;
    private Image healthBackground;
    private GameObject healthCanvasObject;
    // Attack instance reference
    private GameObject currentAttackInstance;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.freezeRotation = true;

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        currentHealth = maxHealth;
    }
    void Start()
    {
        DetectPlayerControls();

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        System.Array.Sort(players, (a, b) => a.GetInstanceID().CompareTo(b.GetInstanceID()));
        bool isPlayerOne = players[0] == this.gameObject;

        healthBarFill = GameUIManager.Instance.CreateHealthBar(isPlayerOne);

        // Test the health bar after 2 seconds
       // Invoke("TestHealthBar", 2f);
    }

    void TestHealthBar()
    {
        Debug.Log("Testing health bar...");
        currentHealth = maxHealth / 2; // Set to 50%
        UpdateHealthBar();
    }


    void DetectPlayerControls()
{
    GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
    System.Array.Sort(players, (a, b) => a.GetInstanceID().CompareTo(b.GetInstanceID()));

    if (players.Length >= 1)
    {
        if (players[0] == this.gameObject)
        {
            // Player 1
            leftKey = KeyCode.LeftArrow;
            rightKey = KeyCode.RightArrow;
            jumpKey = KeyCode.UpArrow;
            attackKey = KeyCode.Z;
            Debug.Log("Player 1 controls set: Arrow keys + Z");
        }
    }

    if (players.Length >= 2)
    {
        if (players[1] == this.gameObject)
        {
            // Player 2
            leftKey = KeyCode.H;
            rightKey = KeyCode.K;
            jumpKey = KeyCode.U;
            attackKey = KeyCode.M;
            Debug.Log("Player 2 controls set: H/K/U + M");
        }
    }
}



    void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Crystal"))
    {
        // Set health to 100%
        currentHealth = maxHealth;
            UpdateHealthBar();

        // OPTIONAL: Destroy the crystal after pickup
        Destroy(other.gameObject);
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
            triggerHandler.playerMove = this;
            
            attackObject.SetActive(false);
        }
        // If attackObject is a prefab reference, we'll instantiate it when needed
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

    void UpdateHealthBar()
    {
        if (healthBarFill == null)
        {
            Debug.LogError("HealthBarFill is NULL for " + gameObject.name);
            return;
        }

        float percent = (float)currentHealth / maxHealth;
        healthBarFill.fillAmount = percent;

        // Log to verify values
        Debug.Log($"{gameObject.name} Health: {currentHealth}/{maxHealth} = {percent}, FillAmount set to: {healthBarFill.fillAmount}");

        // Change color based on health
        if (percent > 0.6f)
            healthBarFill.color = Color.green;
        else if (percent > 0.3f)
            healthBarFill.color = Color.yellow;
        else
            healthBarFill.color = Color.red;
    }

    void Update()
{
    if (isTakingDamage) return;

    float h = 0f;

    // Movement
    if (Input.GetKey(leftKey)) h = -1f;
    else if (Input.GetKey(rightKey)) h = 1f;

    // Jump
    if (Input.GetKeyDown(jumpKey))
        jumpRequested = true;

    // Attack
    if (Input.GetKeyDown(attackKey) && !isAttacking)
        StartAttack();

    moveInput = h;

    // Store previous grounded state
    wasGrounded = grounded;
    
    // Check if grounded
    grounded = groundCheck != null
        ? Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null
        : (rb != null ? Mathf.Abs(rb.linearVelocity.y) < 0.01f : true);

    // Reset jumps when landing
    if (grounded && !wasGrounded)
    {
        jumpsRemaining = maxJumps;
    }

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

    // Update state
    if (isAttacking) state = State.Attack;
    else if (!grounded) state = State.Jump;
    else if (Mathf.Abs(moveInput) > 0.01f) state = State.Run;
    else state = State.Idle;

    // Handle flipping
    if (moveInput > 0.01f && !facingRight) Flip();
    else if (moveInput < -0.01f && facingRight) Flip();

    // Animation handling
    HandleAnimation();
}


    void HandleAnimation()
    {
        if (state != lastState)
        {
            frameIndex = 0;
            frameTimer = 0f;
            lastState = state;
        }

        Sprite[] frames = GetFramesForState(state);
        if (frames != null && frames.Length > 0)
        {
            float fps = frameRate;
            if (state == State.Jump && jumpFrameRate > 0f) fps = jumpFrameRate;
            
            frameTimer += Time.deltaTime;
            float frameTime = 1f / Mathf.Max(1f, fps);
            
            if (frameTimer >= frameTime)
            {
                frameTimer -= frameTime;
                frameIndex = (frameIndex + 1) % frames.Length;
                spriteRenderer.sprite = frames[frameIndex];
            }
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        if (isTakingDamage)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // Movement during attack
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        // Reposition attack object during attack to stay in front of player
        if (isAttacking && currentAttackInstance != null)
        {
            Vector3 attackPosition = transform.position + 
                (facingRight ? Vector3.right : Vector3.left) * attackDistance;
            currentAttackInstance.transform.position = attackPosition;
        }

        // Handle jumping (including double jump)
        if (jumpRequested && !isTakingDamage)
        {
            if (grounded)
            {
                // Regular jump from ground
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
                jumpsRemaining = maxJumps - 1;
            }
            else if (enableDoubleJump && jumpsRemaining > 0)
            {
                // Double jump in air
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpVelocity);
                jumpsRemaining--;
            }
        }
        jumpRequested = false;
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
            triggerHandler.playerMove = this;
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
            attackRb.isKinematic = true;
        }
    }

    public void OnAttackHit(Collider2D other)
    {
        if (other.CompareTag("Player") && other.gameObject != this.gameObject)
        {
            Move otherPlayer = other.GetComponent<Move>();
            if (otherPlayer != null && !otherPlayer.isTakingDamage)
            {
                Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                Vector2 knockback = knockbackDirection * 5f;
                otherPlayer.TakeDamage(attackDamage, knockback);
            }
        }
    }

    Sprite[] GetFramesForState(State s)
    {
        switch (s)
        {
            case State.Run: return (runSprites != null && runSprites.Length > 0) ? runSprites : idleSprites;
            case State.Jump: return (jumpSprites != null && jumpSprites.Length > 0) ? jumpSprites : idleSprites;
            case State.Hit: return (damageSprites != null && damageSprites.Length > 0) ? damageSprites : idleSprites;
            case State.Attack: return (attackSprites != null && attackSprites.Length > 0) ? attackSprites : idleSprites;
            default: return (idleSprites != null && idleSprites.Length > 0) ? idleSprites : null;
        }
    }

    public void TakeDamage(int damage, Vector2? knockback = null)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthBar();
        
        if (!isTakingDamage)
        {
            StartCoroutine(HandleDamage(knockback));
        }
    }

    IEnumerator HandleDamage(Vector2? knockback)
    {
        isTakingDamage = true;
        state = State.Hit;
        
        if (rb != null && knockback.HasValue)
            rb.linearVelocity = knockback.Value;

        if (damageSprites != null && damageSprites.Length > 0)
        {
            float frameTime = 1f / Mathf.Max(1f, frameRate);
            
            for (int i = 0; i < damageSprites.Length; i++)
            {
                spriteRenderer.sprite = damageSprites[i];
                yield return new WaitForSeconds(frameTime);
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }

        isTakingDamage = false;
        state = State.Idle;
        frameIndex = 0;
        frameTimer = 0f;
        
        if (idleSprites != null && idleSprites.Length > 0)
        {
            spriteRenderer.sprite = idleSprites[0];
        }
    }

    public void Heal(int healAmount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        UpdateHealthBar();
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
        Gizmos.color = Color.red;
        Vector3 attackPosition = transform.position + 
            (facingRight ? Vector3.right : Vector3.left) * attackDistance;
        Gizmos.DrawWireCube(attackPosition, new Vector3(attackSize.x, attackSize.y, 0));
    }
}

public class AttackTriggerHandler : MonoBehaviour
{
    [HideInInspector]
    public Move playerMove;
    [HideInInspector]
    public PlayerAnimation playerAnimation;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (playerMove != null)
        {
            playerMove.OnAttackHit(other);
        }
        else if (playerAnimation != null)
        {
            playerAnimation.OnAttackHit(other);
        }
    }
}