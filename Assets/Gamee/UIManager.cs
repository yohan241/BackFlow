using UnityEngine;
using TMPro; // Important: This line is required for TextMeshPro

public class UIManager : MonoBehaviour
{
    [Header("Player UI Text References")]
    [SerializeField] private TextMeshProUGUI plungerText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private TextMeshProUGUI timeText; // For future points implementation

    // Reference to the Player script to get initial values
    private Player player;

    private float survivalTime = 0f;
    private bool timerRunning = false;

    void Awake()
    {
        // Find the Player script in the scene
        player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("UIManager: Player script not found in scene!");
        }

        // Initialize UI with current player values on Awake
        // This ensures the UI is correct even if the game starts directly from this scene
        if (player != null)
        {
            UpdatePlungerUI(player.currentPlungers, player.currentMaxPlungers);
            UpdateHealthUI(player.currentHealth, player.currentMaxHealth);
            UpdatePointsUI(0); // Initialize points to 0, or load from a save if applicable
            Debug.LogError("UIManager: Player script found in scene!");
        }

        UpdateTimeUI(0f);
        StartTimer();
    }
    void Update()
    {
        if (timerRunning)
        {
            survivalTime += Time.deltaTime;
            UpdateTimeUI(survivalTime);
        }
    }
    public void StartTimer()
    {
        timerRunning = true;
    }

    /// <summary>
    /// Stops the survival timer.
    /// </summary>
    public void StopTimer()
    {
        timerRunning = false;
    }
    public void UpdateTimeUI(float timeInSeconds)
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60);
            timeText.text = $"{minutes:00}:{seconds:00}"; // Formats as MM:SS
        }
    }
    /// <summary>
    /// Updates the displayed plunger count.
    /// </summary>
    /// <param name="current">Current number of plungers.</param>
    /// <param name="max">Maximum number of plungers.</param>
    public void UpdatePlungerUI(int current, int max)
    {
        if (plungerText != null)
        {
            plungerText.text = $"Plungers: {current}/{max}";
        }
    }

    /// <summary>
    /// Updates the displayed health value.
    /// </summary>
    /// <param name="current">Current health.</param>
    /// <param name="max">Maximum health.</param>
    public void UpdateHealthUI(int current, int max)
    {
        if (healthText != null)
        {
            healthText.text = $"Health: {current}/{max}";
        }
    }

    /// <summary>
    /// Updates the displayed points value.
    /// </summary>
    /// <param name="points">Current points.</param>
    public void UpdatePointsUI(int points)
    {
        if (pointsText != null)
        {
            pointsText.text = $"{points}";
        }
    }
}