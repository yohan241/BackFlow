using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FirstGearGames.SmoothCameraShaker;

public class Shooting : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject bullet; // Reference to the Plunger prefab (renamed from bullet to be clearer it's the plunger)
    public Transform firePoint; // Where the projectile spawns and the "aim" visual is
    public float bulletSpeed = 40f; // Speed of the projectile

    [Header("Visuals")]
    public Sprite crosshairSprite; // Assign your crosshair sprite in the inspector
    // [Removed] public Sprite originalPlungerSprite; // This will now be set dynamically from the bullet prefab

    private SpriteRenderer firePointSpriteRenderer; // SpriteRenderer for the firePoint visual
    private Sprite originalFirePointSprite; // Store original sprite of the firePoint itself if it's meant to be something else normally

    [Header("Collision Checks (for visual feedback)")]
    public LayerMask tileLayerMask; // LayerMask for tiles to check if firePoint is overlapping
    public float overlapRadius = 0.1f; // Radius to check overlap at firePoint

    // References
    private Player playerScript; // Cached reference to the Player script

    // Internal state
    private Vector2 lookDirection;
    private float lookAngle;

    public ShakeData shakeData;


    void Awake() // Use Awake to ensure playerScript is found early
    {
        // Find the Player script once at the start. Assumes there's only one player.
        playerScript = GetComponentInParent<Player>(); // Try to get it from parent first
        if (playerScript == null)
        {
            playerScript = FindObjectOfType<Player>();
        }

        if (playerScript == null)
        {
            Debug.LogError("Shooting script: Player component not found in parent or scene!");
            enabled = false; // Disable if no player script found
            return;
        }

        firePointSpriteRenderer = firePoint.GetComponent<SpriteRenderer>();
        if (firePointSpriteRenderer == null)
        {
            Debug.LogWarning("Shooting script: FirePoint missing SpriteRenderer. Visual feedback for aiming might not work.", firePoint);
        }

        // Get the sprite from the bullet prefab's SpriteRenderer
        SpriteRenderer bulletPrefabSR = bullet.GetComponent<SpriteRenderer>();
        if (bulletPrefabSR != null)
        {
            originalFirePointSprite = bulletPrefabSR.sprite; // Use the plunger sprite from the prefab
        }
        else
        {
            Debug.LogWarning("Shooting script: Bullet prefab does not have a SpriteRenderer. Defaulting firePoint sprite to null.");
        }
    }

    void Update()
    {
        // --- Aiming Logic ---
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; // Ensure Z-axis is zero for 2D

        lookDirection = (mouseWorldPos - transform.position).normalized; // Use THIS object's position (the Player)
        // If the Shooting script is on the Player, transform.position is correct.
        // If it's on a child, adjust accordingly or pass player.transform.position.

        float radius = 1f; // Distance of the firePoint from the player's center
        firePoint.position = transform.position + (Vector3)(lookDirection * radius); // Position the firePoint
        lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg; // Calculate rotation angle
        firePoint.rotation = Quaternion.Euler(0, 0, lookAngle); // Apply rotation to firePoint

        // --- Visual Feedback for Plunger Count ---
        // Check if player has any plungers available
        bool playerHasPlungers = playerScript.currentPlungers > 0;

        if (firePointSpriteRenderer != null)
        {
            if (!playerHasPlungers)
            {
                // Show crosshair if no plungers
                if (firePointSpriteRenderer.sprite != crosshairSprite)
                {
                    Debug.Log("Switching firePoint to Crosshair Sprite (no plungers left).");
                }
                firePointSpriteRenderer.sprite = crosshairSprite;
            }
            else
            {
                // Show plunger sprite if plungers are available
                if (firePointSpriteRenderer.sprite != originalFirePointSprite)
                {
                    Debug.Log("Switching firePoint to Plunger Sprite (plungers available).");
                }
                firePointSpriteRenderer.sprite = originalFirePointSprite;
            }
        }


        // --- Shooting Logic ---
        if (Input.GetMouseButtonDown(0) && playerHasPlungers) // Left mouse button and player has plungers
        {
            GameObject newPlunger = Instantiate(bullet); // Instantiate the plunger prefab
            newPlunger.transform.position = firePoint.position; // Set its position
            newPlunger.transform.rotation = Quaternion.Euler(0, 0, lookAngle); // Set its rotation
            newPlunger.GetComponent<Rigidbody2D>().linearVelocity = lookDirection * bulletSpeed; // Apply velocity

            // Decrease plunger count
            CameraShakerHandler.Shake(shakeData);
            playerScript.currentPlungers--;
            Debug.Log($"Plunger shot! Player now has {playerScript.currentPlungers} plungers.");

            // The Plunger script itself handles its tag, stick logic, and self-destruction
            // We no longer need to manually set plungerScript.player here, as Plunger.Awake() finds it.
            // plungerScript.isStuckTag is also handled by the Plunger script's default value.
        }
    }
}