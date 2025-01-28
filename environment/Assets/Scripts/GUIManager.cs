using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    // Reference to the pause panel
    [SerializeField] private GameObject pausePanel;

    // Default game speed
    [SerializeField] private float defaultTimeScale = 1f;

    // Speed multipliers
    [SerializeField] private float speedUpMultiplier = 2f;


    // Reference to the speed display text
    [SerializeField] private Text speedDisplayText;

    // Current time scale tracking
    private float currentTimeScale;

    private void Start()
    {
        // Ensure pause panel is initially hidden
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Initialize speed display
        UpdateSpeedDisplay(1);

        // Set initial time scale
        currentTimeScale = defaultTimeScale;
        Time.timeScale = currentTimeScale;
    }

    private void UpdateSpeedDisplay(int speedLevel)
    {
        if (speedDisplayText != null)
        {
            speedDisplayText.text = $"x{speedLevel}";
        }
    }

    /// <summary>
    /// Speeds up the game
    /// </summary>
    public void SpeedUp()
    {
        // Multiply the current time scale
        currentTimeScale *= speedUpMultiplier;
        
        // Apply the new time scale if it's less than 16x
        if (currentTimeScale <= 16)
        {
            Time.timeScale = currentTimeScale;
        }
        else
        {
            // Reset the time scale to 16x
            currentTimeScale = 16;
            Time.timeScale = currentTimeScale;
        }
        UpdateSpeedDisplay((int)currentTimeScale);
        
        Debug.Log($"Game speed increased to {currentTimeScale}x");
    }

    /// <summary>
    /// Slows down the game
    /// </summary>
    public void SpeedDown()
    {
        currentTimeScale = 1;
        // Apply the new time scale
        Time.timeScale = currentTimeScale;
        UpdateSpeedDisplay((int)currentTimeScale);
        
        Debug.Log($"Game speed decreased to {currentTimeScale}x");
    }

    /// <summary>
    /// Pauses the game and shows pause panel
    /// </summary>
    public void PauseGame()
    {
        // Stop time
        Time.timeScale = 0f;
        
        // Show pause panel if assigned
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        
        Debug.Log("Game Paused");
    }

    public void PrintMessage()
    {
        Debug.Log("clicked:" );
    }

    /// <summary>
    /// Resumes the game and hides pause panel
    /// </summary>
    public void ResumeGame()
    {
        // Restore time scale to the last known value
        Time.timeScale = currentTimeScale;
        
        // Hide pause panel if assigned
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        
        Debug.Log("Game Resumed");
    }

    /// <summary>
    /// Resets the game speed to default
    /// </summary>
    public void ResetSimulation()
    {
        // Ensure time scale is normal before reloading to prevent any loading issues
        Time.timeScale = 1f;
        
        // Get the name of the current active scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        // Reload the current scene
        SceneManager.LoadScene(currentSceneName);
        
        Debug.Log($"Reloading scene: {currentSceneName}");
    }
}