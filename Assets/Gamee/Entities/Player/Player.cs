using System.Collections.Generic;
using UnityEngine;
using TMPro; // Important: This line is required for TextMeshPro
using FirstGearGames.SmoothCameraShaker;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float baseMoveSpeed = 3.0f; // Base speed, will be affected by upgrades
    public float currentMoveSpeed; // Current speed after upgrades
    public float baseJumpForce = 5.0f; // Base jump force, will be affected by upgrades
    public float currentJumpForce; // Current jump force after upgrades
    public Transform shootpos; // Position from where projectiles are shot
    public Plunger projectile; // Reference to the Plunger projectile prefab

    private float horizontal;
    private bool isFacingRight = true;

    [Header("Ground Check")]
    public Transform groundCheck; // Position for checking if grounded
    public Vector2 groundCheckBoxSize = new Vector2(0.5f, 0.1f); // Size of the ground check box
    public LayerMask groundLayer; // Layer(s) considered ground

    [Header("Coyote Jump")]
    public float coyoteTime = 0.2f; // grace period after leaving ground
    private float coyoteTimeCounter;

    [Header("Plunger Management")]
    public int baseMaxPlungers = 1; // Base max plungers, affected by upgrades
    public int currentMaxPlungers; // Current max plungers after upgrades
    public int currentPlungers; // Player's current number of plungers
    public float retrieveRange = 2.0f; // Range to retrieve plungers

    private List<HighlightablePlunger> highlightedPlungers = new List<HighlightablePlunger>();

    [Header("Health & Damage")]
    public int baseMaxHealth = 3; // Default max health
    public int currentMaxHealth; // Current max health after upgrades
    public int currentHealth; // Player's current health
    public float invincibilityDuration = 1.0f; // How long player is invincible after hit
    public float knockbackForce = 7.0f; // Force of knockback
    public float knockbackDuration = 0.2f; // How long knockback velocity is applied

    private bool isInvincible = false;
    private float invincibilityTimer;
    private bool isKnockedBack = false;
    private float knockbackTimer;

    [SerializeField] private Rigidbody2D rigid; // Reference to the Rigidbody2D
    [SerializeField] private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer (for invincibility visual)
    private Collider2D playerCollider; // Reference to the Player's Collider2D

    // Reference to the UIManager
     public UIManager uiManager;

    public ShakeData damageshakeData;

    [Header("UI References")] // Add this header
    public HealthUI healthUI; // Drag your HealthUI GameObject here in Inspector
    // NEW: Player Score
    [Header("Player Score")]
    public int totalPoints = 0; // Player's current score

    [Header("Visual Effects")]
    // Wobble Effect
    public float wobbleMagnitude = 5f; // Degrees of wobble rotation
    public float wobbleSpeed = 10f;    // Speed of wobble oscillation
    // Removed: private float initialSpriteRotationY; // No longer needed

    // Squash and Stretch Effect
    public float squashAmount = 0.8f;   // How much to squash (e.g., 0.8 means 80% original height)
    public float stretchAmount = 1.2f;  // How much to stretch (e.g., 1.2 means 120% original height)
    public float squashStretchDuration = 0.1f; // How long the effect lasts
    private float squashStretchTimer;
    private Vector3 originalScale;
    private bool isSquashing = false;
    private bool isStretching = false;
    private bool wasGroundedLastFrame = false; // To detect landing

    public ParticleSystem dust;


    void Awake() // Use Awake to ensure components are grabbed before Start
    {
        rigid = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>(); // Get the player's collider

        // Find the SpriteRenderer on a child named "Height/Sprite"
        Transform spriteTransform = transform.Find("Height/Sprite"); // Finds child named "Sprite" under "Height"
        if (spriteTransform != null)
        {
            spriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
            originalScale = spriteRenderer.transform.localScale; // Store original scale for squash/stretch
            // Removed: initialSpriteRotationY = spriteRenderer.transform.localEulerAngles.y; // No longer needed
        }

        if (spriteRenderer == null)
        {
            Debug.LogError("Player: SpriteRenderer not found on 'Height/Sprite' child!", this);
        }
        if (playerCollider == null)
        {
            Debug.LogError("Player: Collider2D not found on Player GameObject!", this);
        }

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        // Adjust boxSize based on collider size (if you want it to be dynamic)
        if (col != null)
        {
            groundCheckBoxSize = new Vector2(col.size.x * 0.9f, 0.1f);
        }

        if (healthUI == null)
        {
            healthUI = FindObjectOfType<HealthUI>();
            if (healthUI == null)
            {
                Debug.LogWarning("Player: HealthUI script not found in scene!");
            }
        }
        // Get UIManager reference
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("Player: UIManager script NOT FOUND in scene! UI will not update.");
        }
    }

    void Start()
    {
        // Initialize current stats based on base values (no upgrades yet)
        ApplyUpgrades();
        currentHealth = currentMaxHealth; // Set current health to max at start
        currentPlungers = currentMaxPlungers; // Set current plungers to max at start

        // Initial UI update from Start, after ApplyUpgrades sets initial max values
        if (uiManager != null)
        {
            uiManager.UpdatePlungerUI(currentPlungers, currentMaxPlungers);
            uiManager.UpdateHealthUI(currentHealth, currentMaxHealth);
            uiManager.UpdatePointsUI(totalPoints); // NEW: Update points UI on start
        }
    }

    void Update()
    {
        // --- Input and Movement State ---
        horizontal = Input.GetAxisRaw("Horizontal");

        UpdateCoyoteTime();
        HandleJumpInput();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryRetrievePlunger();
        }

        // --- Visuals ---
        Flip();
        HighlightPlungersInRange();

        HandleWobbleEffect(); // Handle wobble
        HandleSquashStretchEffect(); // Handle squash and stretch

        // --- Invincibility Timer ---
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
                spriteRenderer.color = Color.white; // Reset color
                // Re-enable collisions with enemies after invincibility ends
                ToggleEnemyCollision(true);
            }
            else
            {
                // Optional: Flash player during invincibility
                spriteRenderer.color = (Mathf.Floor(invincibilityTimer * 5) % 2 == 0) ? Color.white : Color.red;
            }
        }

        // --- Knockback Timer ---
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0)
            {
                isKnockedBack = false;
                rigid.linearVelocity = new Vector2(0, rigid.linearVelocity.y); // Stop horizontal knockback
            }
        }

        wasGroundedLastFrame = isGrounded(); // Update ground status for next frame's landing detection
    }

    void FixedUpdate()
    {
        // Only apply horizontal movement if not knocked back
        if (!isKnockedBack)
        {
            rigid.linearVelocity = new Vector2(horizontal * currentMoveSpeed, rigid.linearVelocity.y);
        }
        // If knocked back, the velocity is managed by the Knockback logic
    }

    // --- Movement Helpers ---
    private void UpdateCoyoteTime()
    {
        if (isGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void HandleJumpInput()
    {
        bool grounded = isGrounded();

        if (Input.GetButtonDown("Jump"))
        {
            if (coyoteTimeCounter > 0f) // Jump initiation
            {
                rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, currentJumpForce);
                coyoteTimeCounter = 0f;
                // Apply squash when initiating jump
                ApplySquash();
                dust.Play();
            }
        }

        if (Input.GetButtonUp("Jump") && rigid.linearVelocity.y > 0)
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, rigid.linearVelocity.y * 0.5f);
        }

        // Apply stretch on landing
        if (grounded && !wasGroundedLastFrame)
        {
            ApplyStretch();
        }
    }

    private void Flip()
    {
        // Only flip if not knocked back, or if horizontal input allows
        if (!isKnockedBack)
        {
            if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
            {
                isFacingRight = !isFacingRight;
                // This flips the entire player GameObject's scale
                Vector3 localscale = transform.localScale;
                localscale.x *= -1f;
                transform.localScale = localscale;
                if (isGrounded())
                {
                    dust.Play();
                }

                // Make sure shootpos also flips correctly
                if (shootpos != null)
                {
                    Vector3 shootPosLocalScale = shootpos.localScale;
                    shootPosLocalScale.x *= -1f;
                    shootpos.localScale = shootPosLocalScale;
                }
            }
        }
    }

    public bool isGrounded()
    {
        return Physics2D.OverlapBox(groundCheck.position, groundCheckBoxSize, 0f, groundLayer);
    }

    // --- Visual Effect Implementations ---
    private void HandleWobbleEffect()
    {
        if (spriteRenderer == null) return;

        // Apply wobble only when moving horizontally and not jumping/falling
        if (isGrounded() && Mathf.Abs(horizontal) > 0.05f && !isKnockedBack && !isSquashing && !isStretching)
        {
            float wobble = Mathf.Sin(Time.time * wobbleSpeed) * wobbleMagnitude;
            // CORRECTED: Only apply wobble to Z-axis. Y-axis rotation is handled by parent's transform.localScale.x.
            spriteRenderer.transform.localEulerAngles = new Vector3(0, 0, wobble);
        }
        else
        {
            // Reset wobble when not running
            // CORRECTED: Ensure Y-axis rotation remains 0.
            spriteRenderer.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
    }

    private void ApplySquash()
    {
        isSquashing = true;
        isStretching = false;
        squashStretchTimer = squashStretchDuration;
    }

    private void ApplyStretch()
    {
        isStretching = true;
        isSquashing = false;
        squashStretchTimer = squashStretchDuration;
    }

    private void HandleSquashStretchEffect()
    {
        if (spriteRenderer == null) return;

        if (isSquashing || isStretching)
        {
            squashStretchTimer -= Time.deltaTime;
            float t = 1 - (squashStretchTimer / squashStretchDuration); // Interpolation factor (0 to 1)

            if (isSquashing)
            {
                // Squash effect (Y decreases, X increases to maintain volume)
                float currentYScale = Mathf.Lerp(originalScale.y, originalScale.y * squashAmount, t);
                float currentXScale = Mathf.Lerp(originalScale.x, originalScale.x * (1f / squashAmount), t); // Inverse for X
                spriteRenderer.transform.localScale = new Vector3(currentXScale, currentYScale, originalScale.z);
            }
            else if (isStretching)
            {
                // Stretch effect (Y increases, X decreases)
                float currentYScale = Mathf.Lerp(originalScale.y, originalScale.y * stretchAmount, t);
                float currentXScale = Mathf.Lerp(originalScale.x, originalScale.x * (1f / stretchAmount), t); // Inverse for X
                spriteRenderer.transform.localScale = new Vector3(currentXScale, currentYScale, originalScale.z);
            }

            if (squashStretchTimer <= 0)
            {
                isSquashing = false;
                isStretching = false;
                spriteRenderer.transform.localScale = originalScale; // Reset to original scale
            }
        }
        else if (spriteRenderer.transform.localScale != originalScale)
        {
            // Smoothly return to original scale if not in an active squash/stretch state
            spriteRenderer.transform.localScale = Vector3.Lerp(spriteRenderer.transform.localScale, originalScale, Time.deltaTime * 10f);
        }
    }

    // --- Plunger Management ---
    private void TryRetrievePlunger()
    {
        // Only retrieve if player doesn't have max plungers
        if (currentPlungers < currentMaxPlungers)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, retrieveRange);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("StuckPlunger"))
                {
                    Plunger plungerToRetrieve = hit.GetComponent<Plunger>();
                    if (plungerToRetrieve != null)
                    {
                        plungerToRetrieve.Retrieve(); // This will trigger the enemy death and plunger destruction
                        currentPlungers++; // Increment player's plunger count here
                        Debug.Log($"Plunger retrieved! Current plungers: {currentPlungers}/{currentMaxPlungers}");
                        if (uiManager != null)
                        {
                            uiManager.UpdatePlungerUI(currentPlungers, currentMaxPlungers);
                        }
                        break; // Only retrieve one plunger at a time
                    }
                }
            }
        }
        else
        {
            Debug.Log("Max plungers reached! Cannot retrieve more.");
        }
    }

    private void HighlightPlungersInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, retrieveRange);
        List<HighlightablePlunger> inRangePlungers = new List<HighlightablePlunger>();

        foreach (var hit in hits)
        {
            if (hit.CompareTag("StuckPlunger"))
            {
                HighlightablePlunger hp = hit.GetComponent<HighlightablePlunger>();
                if (hp != null)
                {
                    hp.SetHighlight(true);
                    hp.SetHighlight2(true, transform);
                    inRangePlungers.Add(hp);
                }
            }
        }

        // Reset outline for plungers no longer in range
        foreach (var oldHp in highlightedPlungers)
        {
            if (!inRangePlungers.Contains(oldHp))
            {
                oldHp.SetHighlight(false);
                oldHp.SetHighlight2(false);
            }
        }
        highlightedPlungers = inRangePlungers;
    }


    // --- Damage & Health System ---
    public void TakeDamage(int damageAmount, Vector2 knockbackDirection)
    {
        if (isInvincible) return;

        currentHealth -= damageAmount;

        currentHealth = Mathf.Max(currentHealth, 0);
        Debug.Log($"Player took {damageAmount} damage. Current Health: {currentHealth}/{currentMaxHealth}");

        if (uiManager != null)
        {
            uiManager.UpdateHealthUI(currentHealth, currentMaxHealth);
        }
        if (healthUI != null)
{
            healthUI.UpdateHealthDisplay(currentHealth);
}
        if (currentHealth <= 0)
        {
            Die();
            return;
        }
        CameraShakerHandler.Shake(damageshakeData);

        isKnockedBack = true;
        knockbackTimer = knockbackDuration;
        rigid.linearVelocity = new Vector2(knockbackDirection.x * knockbackForce, knockbackDirection.y * knockbackForce);

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
        ToggleEnemyCollision(false);
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        if (uiManager != null)
        {
            uiManager.UpdateHealthUI(0, currentMaxHealth);
            uiManager.StopTimer();
        }
        gameObject.SetActive(false);
    }

    // --- Collision Toggle for Invincibility ---
    private void ToggleEnemyCollision(bool enableCollision)
    {
        if (playerCollider == null) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            if (enemyCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, enemyCollider, !enableCollision);
            }
        }
    }

    // NEW: Method to add points to the player
    public void AddPoints(int points)
    {
        totalPoints += points;
        Debug.Log($"Player gained {points} points! Total Points: {totalPoints}");
        if (uiManager != null)
        {
            uiManager.UpdatePointsUI(totalPoints);
        }
    }


    // --- Upgrade System ---
    public void ApplyUpgrades()
    {
        float speedBonus = 0f;
        float jumpBonus = 0f;
        int healthBonus = 0;
        int plungerBonus = 0;

        currentMoveSpeed = baseMoveSpeed + speedBonus;
        currentJumpForce = baseJumpForce + jumpBonus;
        currentMaxHealth = baseMaxHealth + healthBonus;
        currentMaxPlungers = baseMaxPlungers + plungerBonus;

        currentHealth = Mathf.Min(currentHealth, currentMaxHealth);
        currentPlungers = Mathf.Min(currentPlungers, currentMaxPlungers);

        Debug.Log("Player stats updated: " +
                  $"Speed: {currentMoveSpeed}, Jump: {currentJumpForce}, " +
                  $"Health: {currentHealth}/{currentMaxHealth}, Plungers: {currentPlungers}/{currentMaxPlungers}");

        if (uiManager != null)
        {
            uiManager.UpdatePlungerUI(currentPlungers, currentMaxPlungers);
            uiManager.UpdateHealthUI(currentHealth, currentMaxHealth);
            // uiManager.UpdatePointsUI(totalPoints); // This is already called in Start and AddPoints
        }
    }


    // --- Gizmos for editor visualization ---
    void OnDrawGizmosSelected()
    {
        // Ground check box
        if (groundCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckBoxSize);
        }

        // Retrieve range sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, retrieveRange);
    }

    public void Heal(int amount)
{
        
        currentHealth = Mathf.Min(currentHealth, currentMaxHealth);
        Debug.Log($"Player healed {amount}. Current Health: {currentHealth}/{currentMaxHealth}");

        if (healthUI != null)
        {
            healthUI.UpdateHealthDisplay(currentHealth);
        }
    }
}
