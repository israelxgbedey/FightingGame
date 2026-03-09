using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float lifeTime = 5f; // Auto-destroy after this time
    public int damage = 10; // Add damage property
    public float knockbackForce = 8f; // Optional: control knockback strength
    private GameObject parentObject; // The object that spawned this projectile

    void Start()
    {
        // Set parentObject to whatever this bullet is a child of
        if (transform.parent != null)
            parentObject = transform.parent.gameObject;

        Destroy(gameObject, lifeTime);

        // Ignore collisions with parent automatically
        if (parentObject != null)
        {
            Collider2D bulletCollider = GetComponent<Collider2D>();
            Collider2D parentCollider = parentObject.GetComponent<Collider2D>();
            if (bulletCollider != null && parentCollider != null)
                Physics2D.IgnoreCollision(bulletCollider, parentCollider);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore parent object
        if (collision.gameObject == parentObject) return;

        // Check if we hit a player
        Move player = collision.gameObject.GetComponent<Move>();
        if (player != null)
        {
            // Calculate knockback direction (away from projectile)
            Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
            Vector2 knockback = knockbackDirection * knockbackForce;
            
            // Deal damage to player
            player.TakeDamage(damage, knockback);
            
            Debug.Log($"{collision.gameObject.name} hit by projectile for {damage} damage!");
        }

        // Destroy projectile on impact
        Destroy(gameObject);
    }
}