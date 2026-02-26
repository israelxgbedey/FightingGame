using UnityEngine;

public class Mover2 : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

    [Header("Components")]
    public Animator animator;           // uses trigger parameters for attacks; uses bool for movement
    public Rigidbody2D rb;              // optional, will fall back to transform movement
    public bool flipWithScale = true;   // flip sprite by localScale.x
    private bool facingRight = true;

    [Header("Animator parameter names")]
    [Tooltip("Bool parameter name used for walking (true = walking, false = idle)")]
    public string moveBoolParam = "isMoving";
    [Tooltip("Trigger for primary attack (E)")]
    public string attack1Trigger = "Attack1";
    [Tooltip("Trigger for secondary attack (E + Shift)")]
    public string attack2Trigger = "Attack2";

    [Header("Optional: animator state identifiers used to detect 'attacking'")]
    [Tooltip("Tag your attack states in the Animator with this tag (recommended).")]
    public string attackStateTag = "Attack";
    [Tooltip("Optional: specific state names to treat as attack if you don't use tags (exact match)")]
    public string attack1StateName = "";
    public string attack2StateName = "";

    // cached input / state
    float horizontal = 0f;

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Read horizontal input (arrow keys and A/D axis)
        horizontal = Input.GetAxisRaw("Horizontal");
        if (Mathf.Approximately(horizontal, 0f))
        {
            if (Input.GetKey(KeyCode.RightArrow)) horizontal = 1f;
            else if (Input.GetKey(KeyCode.LeftArrow)) horizontal = -1f;
        }

        bool movingNow = Mathf.Abs(horizontal) > 0.01f;

        // Set movement bool parameter so Animator goes to Idle when false
        if (animator != null && !string.IsNullOrEmpty(moveBoolParam))
        {
            animator.SetBool(moveBoolParam, movingNow);
        }

        // Attack input: E = primary, E + Shift = secondary
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (animator != null)
            {
                // Do not retrigger while currently in an attack state or transitioning into one
                if (!IsInAttackState() && !animator.IsInTransition(0))
                {
                    bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                    if (shift && !string.IsNullOrEmpty(attack2Trigger))
                        animator.SetTrigger(attack2Trigger);
                    else if (!string.IsNullOrEmpty(attack1Trigger))
                        animator.SetTrigger(attack1Trigger);
                }
            }
        }

        // flip sprite based on movement direction
        if (horizontal > 0.01f && !facingRight) Flip();
        else if (horizontal < -0.01f && facingRight) Flip();
    }

    void FixedUpdate()
    {
        // apply horizontal movement (physics if Rigidbody2D present)
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
        }
        else
        {
            transform.Translate(Vector2.right * horizontal * speed * Time.fixedDeltaTime);
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        if (!flipWithScale) return;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    // Return true if the animator is currently in an attack state (or exact named states)
    bool IsInAttackState()
    {
        if (animator == null) return false;
        var info = animator.GetCurrentAnimatorStateInfo(0);
        if (!string.IsNullOrEmpty(attackStateTag) && info.IsTag(attackStateTag))
            return true;
        if (!string.IsNullOrEmpty(attack1StateName) && info.IsName(attack1StateName))
            return true;
        if (!string.IsNullOrEmpty(attack2StateName) && info.IsName(attack2StateName))
            return true;
        return false;
    }
}
