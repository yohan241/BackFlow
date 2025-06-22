using UnityEngine;

public class TurretEnemy : Enemy // Inherits from your base Enemy class
{
    [Header("Turret Settings")]
    public float detectionRadius = 5f; // Radius to detect the player
    public float rotationSpeed = 200f; // Speed at which the turret head rotates
    public Transform turretHeadPivot; // Assign the GameObject that represents the turret's rotating head/sprite
    public Transform projectileSpawnPoint; // Where the projectile will be instantiated

    [Header("Projectile Settings")]
    public GameObject piercingProjectilePrefab; // Assign your piercing projectile prefab here
    public float timeBetweenShots = 2f; // How often the turret shoots
    public float projectileSpeed = 8f; // Speed of the fired projectile

    [Header("Layers")]
    public LayerMask playerLayer; // Set this to the layer your Player is on

    private Transform playerTarget; // Reference to the player's transform
    private float shotTimer;
    private bool playerDetected = false;

    protected override void Start()
    {
        base.Start(); // Initialize base Enemy properties (health etc.)

        shotTimer = timeBetweenShots; // Initialize timer

        // Try to find the player initially
        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
        {
            playerTarget = playerGO.transform;
        }
        else
        {
            Debug.LogWarning("TurretEnemy: Player GameObject not found with tag 'Player' at Start.", this);
        }

        if (turretHeadPivot == null)
        {
            Debug.LogError("TurretEnemy: 'Turret Head Pivot' not assigned! Turret will not aim.", this);
        }
        if (projectileSpawnPoint == null)
        {
            Debug.LogError("TurretEnemy: 'Projectile Spawn Point' not assigned! Turret will not shoot.", this);
        }
        if (piercingProjectilePrefab == null)
        {
            Debug.LogError("TurretEnemy: 'Piercing Projectile Prefab' not assigned! Turret cannot shoot.", this);
        }
    }

    void Update()
    {
        // If dead or paralyzed, do nothing
        if (isDead || isParalyzed)
        {
            if (turretHeadPivot != null)
            {
                // Optionally reset rotation or play stun animation for head
                // turretHeadPivot.rotation = Quaternion.identity;
            }
            return;
        }

        DetectPlayer();

        if (playerDetected && playerTarget != null)
        {
            AimAtPlayer();
            HandleShooting();
        }
        else
        {
            // Optionally reset turret head to default rotation when no player detected
            if (turretHeadPivot != null)
            {
                turretHeadPivot.localRotation = Quaternion.Slerp(turretHeadPivot.localRotation, Quaternion.identity, rotationSpeed * Time.deltaTime / 100f);
            }
        }
    }

    private void DetectPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, playerLayer);

        playerDetected = false;
        playerTarget = null; // Reset player target

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                playerDetected = true;
                playerTarget = hit.transform;
                // You can add a Line of Sight check here if you want walls to block detection
                // RaycastHit2D losHit = Physics2D.Raycast(projectileSpawnPoint.position, (playerTarget.position - projectileSpawnPoint.position).normalized, detectionRadius, blockLineOfSightLayer);
                // if (losHit.collider != null && losHit.collider.CompareTag("Player")) { playerDetected = true; playerTarget = hit.transform; break; }
                // else { playerDetected = false; }
                break; // Found the player, no need to check others
            }
        }
    }

    private void AimAtPlayer()
    {
        if (turretHeadPivot == null || playerTarget == null) return;

        // Calculate direction to player
        Vector2 directionToPlayer = (playerTarget.position - turretHeadPivot.position).normalized;

        // Calculate angle for the sprite to look at the player
        // Mathf.Atan2 returns radians, convert to degrees
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        // Create target rotation (assuming 0 degrees is facing right)
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        // Smoothly rotate the turret head
        turretHeadPivot.rotation = Quaternion.RotateTowards(turretHeadPivot.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // OPTIONAL: If your turret sprite needs to flip on X based on direction (like your player/goomba)
        // You might need a sprite renderer reference on the turretHeadPivot.
        // if (turretHeadPivot.GetComponent<SpriteRenderer>() != null)
        // {
        //     turretHeadPivot.GetComponent<SpriteRenderer>().flipX = (directionToPlayer.x < 0);
        // }
    }

    private void HandleShooting()
    {
        shotTimer -= Time.deltaTime;
        if (shotTimer <= 0)
        {
            ShootProjectile();
            shotTimer = timeBetweenShots; // Reset timer
        }
    }

    private void ShootProjectile()
    {
        if (piercingProjectilePrefab == null || projectileSpawnPoint == null) return;

        // Ensure the projectile direction is based on the turret's current aim
        Vector2 shootDirection = (turretHeadPivot.right).normalized; // Or turretHeadPivot.up if your sprite faces up

        // If your sprite's "forward" is actually its right, use turretHeadPivot.right
        // If your sprite's "forward" is actually its top, use turretHeadPivot.up

        GameObject projectileGO = Instantiate(piercingProjectilePrefab, projectileSpawnPoint.position, turretHeadPivot.rotation);

        PiercingProjectile piercingProjectile = projectileGO.GetComponent<PiercingProjectile>();
        if (piercingProjectile != null)
        {
            piercingProjectile.Launch(shootDirection, projectileSpeed);
        }
        else
        {
            Debug.LogError("TurretEnemy: Instantiated projectile does not have a PiercingProjectile script!");
        }

        // Add visual/audio feedback for shooting
        // Debug.Log("Turret shot!");
    }

    // Since this is an immobile enemy, it typically won't have an OnCollisionEnter2D for damaging the player directly.
    // Its primary interaction is shooting.

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // Draw the detection radius
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Draw a line to the player if detected (only in play mode)
        if (Application.isPlaying && playerDetected && playerTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(turretHeadPivot != null ? turretHeadPivot.position : transform.position, playerTarget.position);
        }

        // Draw the projectile spawn point
        if (projectileSpawnPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(projectileSpawnPoint.position, 0.1f);
        }
    }
}