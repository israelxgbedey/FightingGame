using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class Move : MonoBehaviour
{


[Header("Counter System")]
public KeyCode counterKey = KeyCode.C;
public float counterDuration = 0.6f;
private bool isCountering = false;
private float counterTimer = 0f;
private bool counterSuccess = false; // Track if counter was successfully used
private float counterCooldownTimer = 0f;
private float counterCooldown = 1f; // Cooldown before counter can be used again

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
    
[Header("Attack System - Primary")]
public GameObject attackObject;
public float attackDuration = 0.3f;
public float attackDistance = 1f;
public int attackDamage = 10;
public Vector2 attackSize = new Vector2(1f, 1f);
public Sprite[] attackSprites; // Already exists


[Header("Attack System - Secondary")]
public KeyCode secondaryAttackKey = KeyCode.X; // For Player 1
// For Player 2, you might want to use a different key like KeyCode.N
public GameObject secondaryAttackObject;
public float secondaryAttackDuration = 0.5f;
public float secondaryAttackDistance = 1.5f;
public int secondaryAttackDamage = 20;
public Vector2 secondaryAttackSize = new Vector2(1.5f, 1.5f);
public Sprite[] secondaryAttackSprites;
public float secondaryAttackFrameRate = 12f; // Optional different frame rate for secondary attack

[Header("Combo System")]
public float comboResetTime = 0.8f; // Time allowed between combo presses
private int comboStep = 0;
private float comboTimer = 0f;
private bool comboQueued = false;



[Header("Floating Combo Text")]
public GameObject comboTextPrefab;
public Vector3 comboTextOffset = new Vector3(0, 2f, 0);
public float comboTextDuration = 1.5f;

private GameObject activeComboText;


[Header("Combo UI")]
public TMP_Text comboText;
public CanvasGroup comboCanvasGroup;
public float comboFadeSpeed = 5f;

// Track which attack we're using
bool isUsingSecondaryAttack = false;


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
            secondaryAttackKey = KeyCode.X; // Secondary attack for Player 1
            counterKey = KeyCode.C;
            Debug.Log("Player 1 controls set: Arrow keys + Z (primary) + X (secondary)");
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
            counterKey = KeyCode.B;
            secondaryAttackKey = KeyCode.N; // Secondary attack for Player 2
            Debug.Log("Player 2 controls set: H/K/U + M (primary) + N (secondary)");
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

void StartCounter()
{
    isCountering = true;
    counterTimer = counterDuration;

    Debug.Log(gameObject.name + " is countering!");
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

    // Primary Attack
    if (Input.GetKeyDown(attackKey))
    {
        if (!isAttacking)
        {
            StartComboAttack();
        }
        else
        {
            // If attacking, queue next combo input
            comboQueued = true;
        }
    }

    // Counter - check cooldown too
    if (Input.GetKeyDown(counterKey) && !isCountering && !isAttacking && counterCooldownTimer <= 0f)
    {
        StartCounter();
    }

    // Secondary Attack
    if (Input.GetKeyDown(secondaryAttackKey) && !isAttacking)
        StartAttack(true); // true = secondary attack

    moveInput = h;

    // Handle counter timer and cooldown
    if (isCountering)
    {
        counterTimer -= Time.deltaTime;
        
        // Visual feedback while countering (optional)
        Debug.Log(gameObject.name + " is countering! Time left: " + counterTimer);
        
        if (counterTimer <= 0f)
        {
            isCountering = false;
            counterCooldownTimer = counterCooldown; // Start cooldown
        }
    }
    
    // Handle counter cooldown
    if (counterCooldownTimer > 0f)
    {
        counterCooldownTimer -= Time.deltaTime;
    }

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
            if (comboQueued)
            {
                comboQueued = false;
                isAttacking = false;
                StartComboAttack();
            }
            else
            {
                isAttacking = false;
                
                if (currentAttackInstance != null)
                {
                    Destroy(currentAttackInstance);
                    currentAttackInstance = null;
                }
            }
        }
    }

    if (comboStep > 0)
    {
        comboTimer -= Time.deltaTime;
        if (comboTimer <= 0f)
        {
            comboStep = 0;
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


IEnumerator FadeOutCombo()
{
    while (comboCanvasGroup.alpha > 0f)
    {
        comboCanvasGroup.alpha -= Time.deltaTime * comboFadeSpeed;
        yield return null;
    }

    comboText.text = "";
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
        
        // Use appropriate frame rate based on attack type
        if (state == State.Attack)
        {
            if (isUsingSecondaryAttack && secondaryAttackFrameRate > 0f)
                fps = secondaryAttackFrameRate;
            // Otherwise use default frameRate
        }
        else if (state == State.Jump && jumpFrameRate > 0f)
        {
            fps = jumpFrameRate;
        }
        
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

void StartComboAttack()
{
    // Don't increment combo if we're in a counter state or just got countered
    // This check might be redundant now since we reset combo in OnAttackHit
    
    comboStep++;
    comboStep = Mathf.Clamp(comboStep, 1, 3);

    isAttacking = true;
    isUsingSecondaryAttack = false;

    attackTimer = attackDuration;
    comboTimer = comboResetTime;

    ActivateAttackObject();

    // 🔥 Show combo UI
    UpdateComboUI();
}

void UpdateComboUI()
{
    if (comboStep <= 1) return;

    if (activeComboText != null)
    {
        StopCoroutine(FadeAndDestroyCombo());
        Destroy(activeComboText);
    }

    Vector3 spawnPosition = transform.position + comboTextOffset;

    activeComboText = Instantiate(comboTextPrefab, spawnPosition, Quaternion.identity);

    TMP_Text textComponent = activeComboText.GetComponent<TMP_Text>();
    textComponent.text = comboStep + " HIT COMBO!";

    StartCoroutine(FadeAndDestroyCombo());
}


IEnumerator FadeAndDestroyCombo()
{
    TMP_Text text = activeComboText.GetComponent<TMP_Text>();

    float visibleTime = 3f;
    float fadeTime = 1f;

    // Stay fully visible
    yield return new WaitForSeconds(visibleTime);

    float timer = 0f;
    Color startColor = text.color;

    while (timer < fadeTime)
    {
        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, timer / fadeTime);

        text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        yield return null;
    }

    Destroy(activeComboText);
    activeComboText = null;
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

 void StartAttack(bool isSecondary)
{
    if (!isAttacking && !isTakingDamage)
    {
        isAttacking = true;
        isUsingSecondaryAttack = isSecondary;
        
        if (isSecondary)
        {
            attackTimer = secondaryAttackDuration;
            ActivateSecondaryAttackObject();
        }
        else
        {
            attackTimer = attackDuration;
            ActivateAttackObject();
        }


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


void ActivateSecondaryAttackObject()
{
    // Clean up any existing attack instance
    if (currentAttackInstance != null)
    {
        Destroy(currentAttackInstance);
    }

    Vector3 attackPosition = transform.position + 
        (facingRight ? Vector3.right : Vector3.left) * secondaryAttackDistance;

    // Check if secondaryAttackObject is a prefab or scene object
    if (secondaryAttackObject.scene.rootCount == 0) // It's a prefab
    {
        // Instantiate the prefab
        currentAttackInstance = Instantiate(secondaryAttackObject, attackPosition, Quaternion.identity);
    }
    else // It's a scene object
    {
        // Use the existing scene object
        currentAttackInstance = secondaryAttackObject;
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

    // Configure collider
    Collider2D collider = currentAttackInstance.GetComponent<Collider2D>();
    if (collider == null)
    {
        BoxCollider2D boxCollider = currentAttackInstance.AddComponent<BoxCollider2D>();
        boxCollider.size = secondaryAttackSize;
        boxCollider.isTrigger = true;
    }
    else if (collider is BoxCollider2D boxCollider2D)
    {
        boxCollider2D.size = secondaryAttackSize;
    }

    // Ensure Rigidbody is kinematic
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
            // Check if the other player is countering
            if (otherPlayer.isCountering)
            {
                Debug.Log(gameObject.name + "'s attack was COUNTERED! Combo reset.");
                
                // Reset combo for the attacker
                comboStep = 0;
                comboTimer = 0f;
                comboQueued = false;
                
                // Optional: Hide combo UI if it's showing
                if (activeComboText != null)
                {
                    Destroy(activeComboText);
                    activeComboText = null;
                }
                
                return; // Don't proceed with damage or combo
            }
            
            Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
            Vector2 knockback = knockbackDirection * 5f;
            
            // Use appropriate damage based on attack type
            int damageToApply = isUsingSecondaryAttack ? secondaryAttackDamage : attackDamage + (comboStep * 5);
            otherPlayer.TakeDamage(damageToApply, knockback);
            
            // Combo is only incremented if the attack wasn't countered
            // The combo increment happens in StartComboAttack already, so we don't need to do anything here
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
        case State.Attack: 
            if (isUsingSecondaryAttack && secondaryAttackSprites != null && secondaryAttackSprites.Length > 0)
                return secondaryAttackSprites;
            else if (attackSprites != null && attackSprites.Length > 0)
                return attackSprites;
            else
                return idleSprites;
        default: return (idleSprites != null && idleSprites.Length > 0) ? idleSprites : null;
    }
}

public void TakeDamage(int damage, Vector2? knockback = null)
{
    // CRITICAL FIX: Check if countering and still within counter window
    if (isCountering)
    {
        Debug.Log(gameObject.name + " SUCCESSFULLY COUNTERED the attack!");
        
        // Optional: Add counter visual feedback
        // You could play a special animation or particle effect here
        
        // Optional: Give the countering player an advantage (like a free attack)
        // For example, auto-counter attack or stun the attacker
        
        return; // 🚫 No damage taken, attack successfully countered
    }

    // Check if on counter cooldown (optional - you might still want to take damage normally)
    
    currentHealth = Mathf.Max(0, currentHealth - damage);
    UpdateHealthBar();
    
    if (!isTakingDamage && currentHealth > 0)
    {
        StartCoroutine(HandleDamage(knockback));
    }
    else if (currentHealth <= 0)
    {
        // Handle death here
        Debug.Log(gameObject.name + " has been defeated!");
        // You might want to add death animation/logic
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