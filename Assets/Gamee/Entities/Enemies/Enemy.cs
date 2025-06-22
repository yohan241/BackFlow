using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth; // Kept public as per previous discussion

    public bool isDead = false;
    public bool isParalyzed = false; // NEW: Indicates if the enemy is paralyzed by a plunger
    private GameObject stuckPlunger; // NEW: Reference to the plunger that stuck this enemy

    [Header("Rewards")] // NEW: Header for reward settings
    public int pointsValue = 10; // NEW: Points rewarded when this enemy is killed

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        Debug.Log($"{gameObject.name} initialized with {maxHealth} health.");
    }

    // This method will be called by the Plunger when it hits this enemy.
    public void Paralyze(GameObject plunger)
    {
        if (isDead || isParalyzed) // Cannot paralyze if already dead or paralyzed
            return;

        isParalyzed = true;
        stuckPlunger = plunger;
        Debug.Log($"{gameObject.name} has been paralyzed by a plunger!");

        // Optional: Play a stun animation or change color/material
        // GetComponent<SpriteRenderer>().color = Color.grey;
    }

    // This method will be called by the Player script when the stuck plunger is retrieved.
    public void OnPlungerRetrieved()
    {
        if (isDead) return;

        Debug.Log($"Plunger retrieved from {gameObject.name}. Initiating enemy death.");
        currentHealth = 0; // Ensure health is zero for death condition
        Die();
    }

    public void TakeDamage(int damage) // Removed knockbackDirection as it's not used here for damage
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Current Health: {currentHealth}/{maxHealth}");

        // If you want enemies to die from direct damage (e.g., player stomp) without a plunger,
        // you would keep this check. For your current request, they only die from plunger retrieval.
        // If currentHealth reaches 0 WITHOUT a plunger, Die() could be called here.
        // For now, removing the direct Die() call on currentHealth <= 0 in TakeDamage
        // because death is tied to plunger retrieval.
    }

    protected virtual void Die()
    {
        if (isDead) return; // Prevent multiple death calls

        isDead = true;
        isParalyzed = false; // Ensure paralyzed state is reset
        stuckPlunger = null; // Clear plunger reference
        Debug.Log($"{gameObject.name} has died!");

        // NEW: Reward player with points
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.AddPoints(pointsValue);
        }
        else
        {
            Debug.LogWarning("Enemy: Could not find Player script to reward points.");
        }

        // Play death animation or effects here
        Destroy(gameObject); // Enemy self-destructs
    }

    // Optional: A method to remove the plunger if the enemy dies by other means (e.g., falls off map)
    private void OnDestroy()
    {
        if (stuckPlunger != null)
        {
            // If the enemy is destroyed and a plunger was stuck to it,
            // the plunger should probably also be destroyed (or returned if you want)
            // For now, let's just destroy it since the enemy it was attached to is gone.
            Destroy(stuckPlunger);
        }
    }
}