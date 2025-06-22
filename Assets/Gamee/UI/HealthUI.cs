using UnityEngine;
using UnityEngine.UI; // Required for Image component
using System.Collections.Generic; // Required for List

public class HealthUI : MonoBehaviour
{
    [Header("UI References")]
    public Player playerScript; // Assign your Player GameObject here in the Inspector
    public GameObject heartImagePrefab; // Drag your 'HeartImage' prefab here
    public Transform healthBarContainer; // Drag your 'HealthBarContainer' GameObject here

    [Header("Heart Sprites")]
    public Sprite fineHeartSprite; // Drag your 'fine' heart sprite here
    public Sprite brokenHeartSprite; // Drag your 'broken' heart sprite here

    private List<Image> heartImages = new List<Image>();

    void Start()
    {
        if (playerScript == null)
        {
            playerScript = FindObjectOfType<Player>();
            if (playerScript == null)
            {
                Debug.LogError("HealthUI: Player script not found in scene! Health UI will not function.");
                enabled = false;
                return;
            }
        }

        if (heartImagePrefab == null)
        {
            Debug.LogError("HealthUI: Heart Image Prefab not assigned!");
            enabled = false;
            return;
        }

        if (healthBarContainer == null)
        {
            Debug.LogError("HealthUI: Health Bar Container not assigned!");
            enabled = false;
            return;
        }

        if (fineHeartSprite == null || brokenHeartSprite == null)
        {
            Debug.LogError("HealthUI: Fine or Broken Heart Sprites not assigned!");
            enabled = false;
            return;
        }

        // Initialize the health bar based on player's max health
        InitializeHealthBar(playerScript.currentMaxHealth);
        // Immediately update to reflect current health
        UpdateHealthDisplay(playerScript.currentHealth);
    }

    // Call this whenever the player's max health changes (e.g., through upgrades)
    public void InitializeHealthBar(int maxHealth)
    {
        // Clear existing hearts
        foreach (Transform child in healthBarContainer)
        {
            Destroy(child.gameObject);
        }
        heartImages.Clear();

        // Create new hearts up to max health
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject heartGO = Instantiate(heartImagePrefab, healthBarContainer);
            Image heartImg = heartGO.GetComponent<Image>();
            if (heartImg != null) 
            {
                heartImages.Add(heartImg);
                heartImg.sprite = fineHeartSprite; // Start with all fine hearts
                Debug.LogError("Heart Ima!");
            }
            else
            {
                Debug.LogError("Heart Image Prefab does not have an Image component!");
            }
        }
    }

    // Call this whenever the player's current health changes
    public void UpdateHealthDisplay(int currentHealth)
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < currentHealth)
            {
                // Heart is full/fine
                heartImages[i].sprite = fineHeartSprite;
            }
            else
            {
                // Heart is empty/broken
                heartImages[i].sprite = brokenHeartSprite;
            }
        }
    }
}