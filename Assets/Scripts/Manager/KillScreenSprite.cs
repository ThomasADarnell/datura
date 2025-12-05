using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KillScreenSprite : MonoBehaviour
{
    [Header("Sprite Settings")]
    [SerializeField] private Sprite[] sprites = new Sprite[4]; // 4 sprites to rotate through
    [SerializeField] private float spriteSize = 200f; // Size of the sprite in pixels
    
    [Header("Timing Settings")]
    [SerializeField] private float delayBeforeStart = 10f; // 10 seconds before first appearance
    [SerializeField] private float bpm = 130f; // Beats per minute (matches killScreenMusic)
    [SerializeField] private float waitBetweenPasses = 3f; // Seconds to wait off-screen before returning
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 500f; // Speed of movement in canvas units per second
    [SerializeField] private float screenPadding = 300f; // Extra distance beyond canvas edges
    
    [Header("Visual Settings")]
    [SerializeField] private float transparency = 0.5f; // Half transparency
    
    private Image imageComponent;
    private RectTransform rectTransform;
    private Canvas canvas;
    private int currentSpriteIndex = 0;
    private float rotationInterval; // Time between sprite rotations
    private Vector2 currentDirection;
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private bool isMoving = false;
    
    void Start()
    {
        // Setup UI components
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("KillScreenSprite: RectTransform not found! This must be a UI element.");
            enabled = false;
            return;
        }
        
        imageComponent = GetComponent<Image>();
        if (imageComponent == null)
        {
            imageComponent = gameObject.AddComponent<Image>();
        }
        
        // Get parent canvas
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("KillScreenSprite: No parent Canvas found! This must be a child of a Canvas.");
            enabled = false;
            return;
        }
        
        // Calculate rotation interval based on BPM
        // 130 BPM = 130 beats per minute = 130/60 beats per second
        // We want to change sprite every beat
        rotationInterval = 60f / bpm;
        
        // Set initial sprite if available
        if (sprites.Length > 0 && sprites[0] != null)
        {
            imageComponent.sprite = sprites[0];
        }
        else
        {
            Debug.LogWarning("KillScreenSprite: No sprites assigned! Please assign 4 sprites in the inspector.");
        }
        
        // Set transparency
        Color color = imageComponent.color;
        color.a = transparency;
        imageComponent.color = color;
        
        // Set size
        rectTransform.sizeDelta = new Vector2(spriteSize, spriteSize);
        
        // Hide initially
        imageComponent.enabled = false;
        
        // Start the sequence after delay
        StartCoroutine(StartSequence());
        
        Debug.Log($"[KillScreenSprite] Initialized. Rotation interval: {rotationInterval}s (at {bpm} BPM)");
    }
    
    void Update()
    {
        if (isMoving)
        {
            // Move towards target
            rectTransform.anchoredPosition = Vector2.MoveTowards(rectTransform.anchoredPosition, targetPosition, moveSpeed * Time.deltaTime);
            
            // Check if reached target (off screen)
            if (Vector2.Distance(rectTransform.anchoredPosition, targetPosition) < 1f)
            {
                isMoving = false;
                imageComponent.enabled = false;
                StartCoroutine(WaitAndReappear());
            }
        }
    }
    
    IEnumerator StartSequence()
    {
        // Wait for the specified delay (10 seconds by default)
        yield return new WaitForSeconds(delayBeforeStart);
        
        Debug.Log("[KillScreenSprite] Starting sprite sequence!");
        
        // Start the sprite rotation coroutine
        StartCoroutine(RotateSprites());
        
        // Start first pass
        StartNewPass();
    }
    
    IEnumerator RotateSprites()
    {
        while (true)
        {
            // Wait for the rotation interval (synced to BPM)
            yield return new WaitForSeconds(rotationInterval);
            
            // Only rotate sprites if we have them
            if (sprites.Length > 0)
            {
                // Move to next sprite
                currentSpriteIndex = (currentSpriteIndex + 1) % sprites.Length;
                
                // Update sprite if not null
                if (sprites[currentSpriteIndex] != null)
                {
                    imageComponent.sprite = sprites[currentSpriteIndex];
                }
            }
        }
    }
    
    void StartNewPass()
    {
        // Get random angle (0-360 degrees)
        float angle = Random.Range(0f, 360f);
        currentDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        
        // Calculate canvas bounds with padding
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        
        float maxDistance = Mathf.Max(canvasWidth, canvasHeight) / 2f + screenPadding;
        
        // Start position is off-screen in the direction opposite to movement
        startPosition = -currentDirection * maxDistance;
        
        // Target position is off-screen in the movement direction
        targetPosition = currentDirection * maxDistance;
        
        // Set position and enable sprite
        rectTransform.anchoredPosition = startPosition;
        imageComponent.enabled = true;
        isMoving = true;
        
        Debug.Log($"[KillScreenSprite] New pass - Angle: {angle:F1}°, From: {startPosition}, To: {targetPosition}");
    }
    
    IEnumerator WaitAndReappear()
    {
        // Wait a few seconds off-screen
        yield return new WaitForSeconds(waitBetweenPasses);
        
        // Start a new pass from a different angle
        StartNewPass();
    }
    
    // Helper method to visualize the sprite path in editor
    void OnDrawGizmos()
    {
        if (Application.isPlaying && isMoving)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPosition, targetPosition);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPosition, 0.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
        }
    }
}
