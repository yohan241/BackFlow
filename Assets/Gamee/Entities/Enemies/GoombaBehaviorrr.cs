using UnityEngine;

public class GoombaBehavior : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    [Tooltip("Initial direction: 1 for right, -1 for left.")]
    public int initialMoveDirection = -1;
    private int currentMoveDirection;

    [Header("Collision Checks")]
    public LayerMask obstacleLayer; // For walls (if still used)
    public Vector2 wallCheckOffset = new Vector2(0.5f, 0f); // Existing wall check offset
    public float wallCheckDistance = 0.1f; // Existing wall check distance

    [Header("Edge Detection")] // NEW: Section for edge detection settings
    public LayerMask groundLayer; // NEW: Layer(s) considered ground for edge detection
    public Vector2 edgeCheckOffset = new Vector2(0.4f, -0.1f); // NEW: Offset for the edge check ray (e.g., slightly in front and below the goomba)
    public float edgeCheckDistance = 0.1f; // NEW: Distance to check for ground below

    // Components
    private Rigidbody2D rigid;
    private Enemy enemyBase; // Reference to the base Enemy script
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        enemyBase = GetComponent<Enemy>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (enemyBase == null)
        {
            Debug.LogWarning("Enemy base class missing on Goomba! Please add an Enemy script to this GameObject.", this);
        }
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer missing on Goomba! Sprite flipping will not work.", this);
        }

        currentMoveDirection = initialMoveDirection;
    }

    void Update()
    {
        // If enemy is dead or base script is missing, stop movement
        if (enemyBase == null || enemyBase.isDead)
        {
            rigid.linearVelocity = Vector2.zero;
            return;
        }

        // Check if the base Enemy script indicates it's paralyzed
        if (enemyBase.isParalyzed) // Using the central isParalyzed from Enemy.cs
        {
            rigid.linearVelocity = Vector2.zero; // Stop movement when paralyzed
        }
        else // Normal movement state
        {
            // Check for edge in front
            if (IsNearEdge())
            {
                FlipDirection();
            }
            // Optional: You can keep IsHittingWall() here if you also want it to flip when hitting a vertical wall.
            // For now, it's just edge detection.
            // if (IsHittingWall())
            // {
            //     FlipDirection();
            // }

            rigid.linearVelocity = new Vector2(moveSpeed * currentMoveDirection, rigid.linearVelocity.y);

            // Flip sprite direction
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = (currentMoveDirection == 1);
            }
        }
    }

    // Existing method to check for walls (optional, if you want both wall and edge detection)
    private bool IsHittingWall()
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(wallCheckOffset.x * currentMoveDirection, wallCheckOffset.y);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * currentMoveDirection, wallCheckDistance, obstacleLayer);
        Debug.DrawRay(origin, Vector2.right * currentMoveDirection * wallCheckDistance, Color.red);
        return hit.collider != null;
    }

    // NEW: Method to detect if the Goomba is near an edge
    private bool IsNearEdge()
    {
        // Calculate the origin point for the raycast:
        // Current position + (offset.x * currentMoveDirection to look ahead) + (offset.y to look slightly below)
        Vector2 origin = (Vector2)transform.position + new Vector2(edgeCheckOffset.x * currentMoveDirection, edgeCheckOffset.y);

        // Cast a ray downwards from this origin
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, edgeCheckDistance, groundLayer);

        // Draw the ray in the editor for debugging
        Debug.DrawRay(origin, Vector2.down * edgeCheckDistance, Color.yellow);

        // If the ray DOES NOT hit the ground layer, it means there's an edge
        return hit.collider == null;
    }

    private void FlipDirection()
    {
        currentMoveDirection *= -1; //
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the Goomba hits the Player (assuming Player has a "Player" tag)
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null && !enemyBase.isDead)
            {
                // Calculate knockback direction from enemy to player
                Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;
                player.TakeDamage(1, knockbackDirection); // Goomba deals 1 damage
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw gizmo for wall check (if IsHittingWall is still used)
        Vector2 wallOrigin = (Vector2)transform.position + new Vector2(wallCheckOffset.x * currentMoveDirection, wallCheckOffset.y);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(wallOrigin, wallOrigin + Vector2.right * currentMoveDirection * wallCheckDistance);

        // NEW: Draw gizmo for edge check
        Vector2 edgeOrigin = (Vector2)transform.position + new Vector2(edgeCheckOffset.x * currentMoveDirection, edgeCheckOffset.y);
        Gizmos.color = Color.yellow; // Choose a distinct color for edge check
        Gizmos.DrawLine(edgeOrigin, edgeOrigin + Vector2.down * edgeCheckDistance);
    }
}