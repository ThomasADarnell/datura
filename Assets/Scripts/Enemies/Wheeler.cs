using UnityEngine;

public class Wheeler : EnemyBaseBehavior
{
    // --- Public Variables (Configurable in Unity Inspector) ---
    [Header("Wheeler Stats")]
    public int currentHealth = 2;
    public int maxHealth = 2;
    
    [Header("Detection Settings")]
    public float detectionRange = 50f; // Range to detect player
    public float idleTimeThreshold = 3f; // How long player must be idle before Wheeler emerges
    public float chaseSpeed = 6f; // Very fast chase speed
    
    [Header("Movement Bounds")]
    public Rect movementBounds = new Rect(3.9f, -21.8f, 33.1f, 61.7f);
    public GameObject boundsObject; // Reference to the bounds object
    
    [Header("References")]
    private Animator anim;
    
    // --- Private State Variables ---
    private enum WheelerState
    {
        Hiding,
        Emerging,
        Chasing
    }
    
    private WheelerState currentState = WheelerState.Hiding;
    private Vector3 lastPlayerPosition;
    private float playerIdleTimer = 0f;
    private bool hasEmerged = false;
    private bool isPlayerScared = false;
    
    // --- Initialization ---
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        anim = GetComponent<Animator>();
        
        // Freeze rotation to prevent sprite from rotating
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
        
        if (player != null)
        {
            lastPlayerPosition = player.transform.position;
        }
        
        // Dynamically find boundsObject if not assigned
        if (boundsObject == null)
        {
            boundsObject = GameObject.FindWithTag("BoundsArea");
            if (boundsObject == null)
            {
                Debug.LogWarning("Bounds object not found. Wheeler will use default bounds.");
            }
        }
        
        // Initialize movement bounds from boundsObject
        if (boundsObject != null)
        {
            var collider = boundsObject.GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                movementBounds = new Rect(
                    collider.bounds.min.x,
                    collider.bounds.min.y,
                    collider.bounds.size.x,
                    collider.bounds.size.y
                );
            }
        }
        
        // Set initial animation state
        if (anim != null)
        {
            anim.SetBool("IsHiding", true);
            anim.SetBool("IsEmerging", false);
            anim.SetBool("IsDead", false);
            anim.SetBool("WasDamaged", false);
            anim.SetBool("FinishedCurrentAnim", false);
        }
        
        Debug.Log($"[{gameObject.name}] Wheeler initialized in Hiding state");
    }
    
    // --- Update Loop ---
    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        
        switch (currentState)
        {
            case WheelerState.Hiding:
                UpdateHidingState(distanceToPlayer);
                break;
                
            case WheelerState.Emerging:
                // Wait for animation to complete
                break;
                
            case WheelerState.Chasing:
                ChasePlayer();
                break;
        }
    }
    
    // --- State Methods ---
    
    void UpdateHidingState(float distanceToPlayer)
    {
        if (distanceToPlayer > detectionRange)
        {
            // Player too far, reset idle timer
            playerIdleTimer = 0f;
            return;
        }
        
        // Check if player is moving
        Vector3 currentPlayerPosition = player.transform.position;
        float playerMovement = Vector3.Distance(lastPlayerPosition, currentPlayerPosition);
        lastPlayerPosition = currentPlayerPosition;
        
        // If player is not moving much, increment idle timer
        if (playerMovement < 0.01f)
        {
            playerIdleTimer += Time.deltaTime;
        }
        else
        {
            playerIdleTimer = 0f;
        }
        
        // Check trigger conditions
        if (playerIdleTimer >= idleTimeThreshold || isPlayerScared)
        {
            StartEmerging();
        }
    }
    
    void StartEmerging()
    {
        if (hasEmerged) return;
        
        currentState = WheelerState.Emerging;
        hasEmerged = true;
        
        Debug.Log($"[{gameObject.name}] Wheeler is emerging!");
        
        if (anim != null)
        {
            anim.SetBool("IsHiding", false);
            anim.SetBool("IsEmerging", true);
            anim.SetBool("FinishedCurrentAnim", false);
        }
        
        // Enable the collider as a trigger when emerging
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        // Start chasing after a short delay (simulating emergence animation)
        Invoke(nameof(StartChasing), 1f);
    }
    
    void StartChasing()
    {
        currentState = WheelerState.Chasing;
        
        if (anim != null)
        {
            anim.SetBool("IsEmerging", false);
            anim.SetBool("FinishedCurrentAnim", true);
        }
        
        Debug.Log($"[{gameObject.name}] Wheeler is now chasing!");
    }
    
    void ChasePlayer()
    {
        if (player == null) return;
        
        // Calculate direction to player
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        moveDirection = directionToPlayer;
        
        // Use fast chase speed
        float desiredSpeed = chaseSpeed;
        base.ApplyAcceleration(desiredSpeed);
        
        // Move without rotating
        MoveWithoutRotation();
        
        // Clamp position to movement bounds to avoid hitting walls
        transform.position = ClampToBounds(transform.position);
    }
    
    // Override movement to prevent rotation
    private void MoveWithoutRotation()
    {
        float actualSpeed = Mathf.Min(currentSpeed, maxSpeed);
        transform.position = Vector3.MoveTowards(
            transform.position,
            transform.position + moveDirection,
            actualSpeed * Time.deltaTime
        );
        // No rotation applied - sprite stays upright
    }
    
    // --- Damage System ---
    
    public void TakeDamage(int damage)
    {
        // Can't damage Wheeler while hiding
        if (currentState == WheelerState.Hiding)
        {
            Debug.Log($"[{gameObject.name}] Wheeler is hiding and cannot be damaged!");
            return;
        }
        
        if (currentHealth <= 0) return;
        
        currentHealth -= damage;
        
        Debug.Log($"[{gameObject.name}] Wheeler took {damage} damage. Health: {currentHealth}/{maxHealth}");

        if (anim != null)
        {
            anim.SetBool("WasDamaged", true);
            anim.SetBool("FinishedCurrentAnim", false);
            // Reset damage flag after a short delay
            Invoke(nameof(ResetDamageFlag), 0.5f);
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyHurt();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void ResetDamageFlag()
    {
        if (anim != null)
        {
            anim.SetBool("WasDamaged", false);
            anim.SetBool("FinishedCurrentAnim", true);
        }
    }
    
    void Die()
    {
        Debug.Log($"[{gameObject.name}] Wheeler died!");
        
        if (anim != null)
        {
            anim.SetBool("IsDead", true);
            anim.SetBool("FinishedCurrentAnim", false);
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyDeath();
        }

        // Notify enemy manager
        EnemyManager enemyManager = FindFirstObjectByType<EnemyManager>();
        if (enemyManager != null)
        {
            enemyManager.enemyKilled(this);
        }
        
        // Destroy after animation completes
        Destroy(gameObject, 1f);
    }
    
    // --- Helper Methods ---
    
    private Vector3 ClampToBounds(Vector3 position)
    {
        return new Vector3(
            Mathf.Clamp(position.x, movementBounds.xMin, movementBounds.xMax),
            Mathf.Clamp(position.y, movementBounds.yMin, movementBounds.yMax),
            position.z
        );
    }
    
    // Public method to be called when player performs "scared" action
    public void OnPlayerScared()
    {
        isPlayerScared = true;
        if (currentState == WheelerState.Hiding)
        {
            StartEmerging();
        }
    }
    
    // --- Gizmos for Debugging ---
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw movement bounds
        Gizmos.color = Color.cyan;
        Vector3 boundsCenter = new Vector3(
            movementBounds.xMin + movementBounds.width / 2,
            movementBounds.yMin + movementBounds.height / 2,
            transform.position.z
        );
        Vector3 boundsSize = new Vector3(movementBounds.width, movementBounds.height, 0.1f);
        Gizmos.DrawWireCube(boundsCenter, boundsSize);
    }
}
