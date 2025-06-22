using UnityEngine;

public class FlyBehavior : Enemy
{
    [Header("Fly Movement Settings")]
    public float moveSpeed = 1.5f;
    public float moveRange = 2.0f; // How far it moves from its starting point
    public bool startsVertical = true; // Determines if it starts moving vertically (true) or horizontally (false)

    private Vector2 startPosition;
    private bool movingPositive = true; // True for up/right, false for down/left

    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;

    protected override void Start()
    {
        base.Start(); // Call the base Enemy's Start method to initialize health

        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rigid == null)
        {
            Debug.LogError("FlyBehavior: Rigidbody2D not found!", this);
            enabled = false; // Disable script if no Rigidbody2D
            return;
        }

        rigid.gravityScale = 0; // Flies typically don't have gravity

        startPosition = transform.position;

        // Randomly decide starting direction based on startsVertical
        if (startsVertical)
        {
            movingPositive = Random.Range(0, 2) == 0; // 0 for down, 1 for up
        }
        else
        {
            movingPositive = Random.Range(0, 2) == 0; // 0 for left, 1 for right
        }
    }

    void Update()
    {
        // If enemy is dead or paralyzed, stop movement
        if (isDead || isParalyzed)
        {
            if (rigid.linearVelocity != Vector2.zero)
            {
                rigid.linearVelocity = Vector2.zero;
            }
            return; // Stop further movement logic
        }

        Vector2 targetPosition;

        // Determine target position based on direction and range
        if (movingPositive)
        {
            targetPosition = startsVertical ?
                             new Vector2(startPosition.x, startPosition.y + moveRange) :
                             new Vector2(startPosition.x + moveRange, startPosition.y);
        }
        else
        {
            targetPosition = startsVertical ?
                             new Vector2(startPosition.x, startPosition.y - moveRange) :
                             new Vector2(startPosition.x - moveRange, startPosition.y);
        }

        // Move towards target
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Check if target reached, then flip direction to create a loop
        if (startsVertical)
        {
            if ((movingPositive && transform.position.y >= startPosition.y + moveRange - 0.01f) || // Added small buffer
                (!movingPositive && transform.position.y <= startPosition.y - moveRange + 0.01f)) // Added small buffer
            {
                movingPositive = !movingPositive; // Flip direction to loop
                Debug.Log("Fly reached vertical limit and is flipping direction.");
            }
        }
        else // Horizontal movement
        {
            if ((movingPositive && transform.position.x >= startPosition.x + moveRange - 0.01f) || // Added small buffer
                (!movingPositive && transform.position.x <= startPosition.x - moveRange + 0.01f)) // Added small buffer
            {
                movingPositive = !movingPositive; // Flip direction to loop
                Debug.Log("Fly reached horizontal limit and is flipping direction.");
            }
            // Flip sprite based on horizontal direction
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = movingPositive; // Assuming positive X is right, flip if moving left
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null && !isDead)
            {
                Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;
                player.TakeDamage(1, knockbackDirection);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            startPosition = transform.position;
        }

        Gizmos.color = Color.cyan;
        if (startsVertical)
        {
            Gizmos.DrawLine(new Vector2(startPosition.x, startPosition.y - moveRange), new Vector2(startPosition.x, startPosition.y + moveRange));
            Gizmos.DrawWireSphere(new Vector2(startPosition.x, startPosition.y - moveRange), 0.1f);
            Gizmos.DrawWireSphere(new Vector2(startPosition.x, startPosition.y + moveRange), 0.1f);
        }
        else
        {
            Gizmos.DrawLine(new Vector2(startPosition.x - moveRange, startPosition.y), new Vector2(startPosition.x + moveRange, startPosition.y));
            Gizmos.DrawWireSphere(new Vector2(startPosition.x - moveRange, startPosition.y), 0.1f);
            Gizmos.DrawWireSphere(new Vector2(startPosition.x + moveRange, startPosition.y), 0.1f);
        }
    }
}