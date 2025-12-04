using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Image dimBackground;
    [SerializeField] private Image pauseIcon;

    [Header("Pause Settings")]
    [SerializeField] private float dimAlpha = 0.7f;
    [SerializeField] private float flashSpeed = 1.5f;
    [SerializeField] private float minAlpha = 0.3f;
    [SerializeField] private float maxAlpha = 1.0f;

    [Header("Canvas Settings (Optional)")]
    [Tooltip("If set, will ensure pause menu renders on top by moving to last sibling position")]
    [SerializeField] private bool ensureRenderOnTop = true;

    private bool isPaused = false;
    private float flashTimer = 0f;
    private Canvas pauseCanvas;

    void Start()
    {
        // Ensure the pause menu is hidden at start
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
            
            // Cache the canvas reference
            pauseCanvas = pauseMenuPanel.GetComponentInParent<Canvas>();
            
            // Ensure pause menu renders on top of other UI elements
            if (ensureRenderOnTop)
            {
                pauseMenuPanel.transform.SetAsLastSibling();
            }
        }
    }

    void Update()
    {
        // Check for Escape key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // Handle flashing pause icon
        if (isPaused && pauseIcon != null)
        {
            FlashPauseIcon();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuPanel != null)
        {
            // Ensure it's on top when showing
            if (ensureRenderOnTop)
            {
                pauseMenuPanel.transform.SetAsLastSibling();
            }
            
            pauseMenuPanel.SetActive(true);
        }

        // Set dim background
        if (dimBackground != null)
        {
            Color color = dimBackground.color;
            color.a = dimAlpha;
            dimBackground.color = color;
        }

        // Reset flash timer
        flashTimer = 0f;
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    private void FlashPauseIcon()
    {
        flashTimer += Time.unscaledDeltaTime * flashSpeed;
        
        // Use PingPong to create a smooth back-and-forth animation
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(flashTimer, 1f));
        
        Color color = pauseIcon.color;
        color.a = alpha;
        pauseIcon.color = color;
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}
