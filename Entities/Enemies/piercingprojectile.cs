using UnityEngine;

public class PiercingProjectile : MonoBehaviour
{
    public int damage = 1; // Damage dealt to the player
    public float lifetime = 5f; // How long the projectile exists before being destroyed
    public LayerMask playerLayer; // Set this to the layer your Player is on

    private Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        if (rigid == null)
        {
            Debug.LogError("PiercingProjectile: Rigidbody2D not found!", this);
            enabled = false;
        }

        // Set Rigidbody to Kinematic so it can move through walls
        rigid.bodyType = RigidbodyType2D.Kinematic;
        rigid.gravityScale = 0; // No gravity
    }

    void Start()
    {
        Destroy(gameObject, lifetime); // Destroy projectile after a set lifetime
    }

    public void Launch(Vector2 direction, float speed)
    {
        if (rigid != null)
        {
            rigid.linearVelocity = direction.normalized * speed;
        }
    }

    // Use OnTriggerEnter2D because it's kinematic and we want it to go through walls
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object is on the playerLayer
        if (((1 << other.gameObject.layer) & playerLayer) > 0)
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                // Calculate knockback direction FROM projectile TO player
                Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;
                player.TakeDamage(damage, knockbackDirection);
                Destroy(gameObject); // Destroy projectile after hitting player
            }
        }
    }
}