using UnityEngine;
using TMPro; // Add this if not already present


public class Plunger : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rigid;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private bool hasLanded = false;

    public string isStuckTag = "StuckPlunger";

    // Track the enemy this plunger is stuck to
    private Enemy stuckEnemy;

    private Player playerScript;

    // Reference to UIManager
    private UIManager uiManager;

    // NEW: Reference to the Main Camera
    private Camera mainCamera; // Moved to Plunger class scope

    [Header("Proximity Hit Detection")]
    public float hitRadius = 0.5f; // Adjust this value in the Inspector (e.g., 0.5 to 1.0 depending on desired ease)
    public LayerMask enemyLayer; // Set this in the Inspector to the layer your enemies are on

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (GetComponent<HighlightablePlunger>() == null)
        {
            gameObject.AddComponent<HighlightablePlunger>();
        }

        if (GetComponent<PlungerShadowController>() == null)
        {
            gameObject.AddComponent<PlungerShadowController>();
        }

        playerScript = FindObjectOfType<Player>();
        if (playerScript == null)
        {
            Debug.LogError("Plunger: Player script NOT FOUND in scene! Plunger cannot return to player.", this);
        }

        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("Plunger: UIManager script NOT FOUND in scene! Plunger UI will not update on stick.");
        }

        // Assign the main camera in Awake. If Camera.main is null here, it means
        // either no camera is tagged "MainCamera" or it hasn't initialized yet.
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Plunger: Main Camera not found in Awake! Ensure your camera is tagged 'MainCamera'.", this);
        }

        
    }

    void Update()
    {
        // Use mainCamera reference for viewport calculations
        if (mainCamera != null)
        {
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
            bool outOfScreen = viewportPos.x < -0.1f || viewportPos.x > 1.1f || viewportPos.y < -0.1f || viewportPos.y > 1.1f;
            if (outOfScreen)
            {
                ReturnPlungerToPlayer(true); // Always return if it goes out of screen
                return; // Exit Update to avoid further processing
            }
        }
        else
        {
            // If mainCamera was null in Awake, try to find it again in Update.
            // This can happen if the camera is instantiated after the plunger.
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                // If still null, just destroy if far away to prevent endless plungers
                if (Vector3.Distance(transform.position, Vector3.zero) > 100f) // Arbitrary distance
                {
                    ReturnPlungerToPlayer(false);
                    Destroy(gameObject);
                }
                return; // Can't do viewport checks without a camera
            }
        }

        // NEW LOGIC: Proximity hit detection for enemies
        if (!hasLanded) // Only perform if the plunger hasn't landed yet
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, hitRadius, enemyLayer);

            if (hitEnemies.Length > 0)
            {
                // We hit at least one enemy within the radius
                Enemy enemyBase = hitEnemies[0].GetComponent<Enemy>(); // Get the first enemy hit
                if (enemyBase != null)
                {
                    Debug.Log($"Plunger detected {enemyBase.name} in radius ({hitRadius})!");

                    enemyBase.Paralyze(this.gameObject);
                    stuckEnemy = enemyBase;

                    rigid.linearVelocity = Vector2.zero;
                    rigid.bodyType = RigidbodyType2D.Kinematic;
                    transform.SetParent(hitEnemies[0].transform); // Parent to the detected enemy

                    // Position the plunger near the center of the enemy or at a visually pleasing spot
                    // You might need to fine-tune this offset for better visuals
                    transform.position = hitEnemies[0].transform.position + Vector3.up * 0.2f;

                    hasLanded = true;
                    gameObject.tag = isStuckTag;

                    if (playerScript != null)
                    {
                        playerScript.currentPlungers = Mathf.Max(0, playerScript.currentPlungers - 1);
                        Debug.Log($"Plunger stuck on {enemyBase.name} via radius hit. Player now has {playerScript.currentPlungers} plungers.");
                        if (uiManager != null)
                        {
                            uiManager.UpdatePlungerUI(playerScript.currentPlungers, playerScript.currentMaxPlungers);
                        }
                    }
                    return; // Exit Update after hitting an enemy
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasLanded) return; // Prevent re-triggering logic if already stuck/landed

        // Handle collision with Ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            hasLanded = true;
            rigid.linearVelocity = Vector2.zero;
            rigid.bodyType = RigidbodyType2D.Static;

            // Align rotation flush with surface
            Vector2 normal = collision.contacts[0].normal;
            float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg - 180f; // Adjusted for better visual alignment, verify in Unity.
            transform.rotation = Quaternion.Euler(0, 0, angle);

            gameObject.tag = isStuckTag;
            
           


            if (playerScript != null)
            {
                playerScript.currentPlungers = Mathf.Max(0, playerScript.currentPlungers - 1);
                Debug.Log($"Plunger stuck on ground. Player now has {playerScript.currentPlungers} plungers.");
                if (uiManager != null)
                {
                    uiManager.UpdatePlungerUI(playerScript.currentPlungers, playerScript.currentMaxPlungers);
                }
            }
            return;
        }
    }

    /// <summary>
    /// Handles returning the plunger to the player (if applicable) and destroying the GameObject.
    /// </summary>
    /// <param name="shouldReturnToPlayerCount">If true, increments player's plunger count before destroying.</param>
    private void ReturnPlungerToPlayer(bool shouldReturnToPlayerCount)
    {
        // If this plunger was stuck to an enemy, detach it
        if (stuckEnemy != null)
        {
            transform.SetParent(null); // Detach from enemy hierarchy
            Debug.Log($"Detaching plunger from {stuckEnemy.name}.");
            stuckEnemy = null; // Clear reference
        }

        if (playerScript != null && shouldReturnToPlayerCount)
        {
            if (playerScript.currentPlungers < playerScript.currentMaxPlungers)
            {
                playerScript.currentPlungers = Mathf.Min(playerScript.currentMaxPlungers, playerScript.currentPlungers + 1);
                Debug.Log($"Plunger returned to player. Current plungers: {playerScript.currentPlungers}/{playerScript.currentMaxPlungers}");
            }
            else
            {
                Debug.Log("Plunger attempted to return but player has max plungers.");
            }
        }
        else if (playerScript == null && shouldReturnToPlayerCount)
        {
            Debug.LogWarning("Plunger: Player script reference is null when trying to return plunger.");
        }

        if (gameObject != null) // Only destroy if this object still exists
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Called by Player script when it retrieves the plunger from the scene.
    /// This is the *only* way a stuck plunger causes an enemy to die.
    /// </summary>
    public void Retrieve()
    {
        Debug.Log("<color=cyan>Plunger received signal from Player to be retrieved.</color>");

        // If this plunger was stuck to an enemy, tell that enemy to die.
        if (stuckEnemy != null)
        {
            Debug.Log($"<color=cyan>Calling OnPlungerRetrieved() on enemy: {stuckEnemy.name}</color>");
            stuckEnemy.OnPlungerRetrieved();
        }
        else
        {
            Debug.Log("<color=cyan>Plunger was not stuck to an enemy (stuckEnemy is null).</color>");
        }

        // The Player script is now handling the count increment.
        // So, we just destroy the plunger without incrementing the player's count here.
        ReturnPlungerToPlayer(false);
    }

    // Optional: Draw the hit radius in the editor for visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta; // Choose a color for your gizmo
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}