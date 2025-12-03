using UnityEngine;
using System.Collections;

public class Sword : EnemyBaseBehavior
{
    // --- Public Variables (Configurable in Unity Inspector) ---
    [Header("Sword Stats")]
    public int currentHealth = 5;
    public int maxHealth = 5;
    
    [Header("Detection Settings")]
    public float detectionRange = 10f; // Range to detect player
    public float attackRange = 2f; // Range to initiate attack
    public float chargeAttackRange = 8f; // Range to initiate charge attack
    
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float chargeSpeed = 8f;
    public float idleTime = 2f; // Time spent idle between movements
    public float walkToRunTransitionTime = 1.5f; // Time walking before running
    public float roamRadius = 5f; // How far to roam from spawn point
    public float roamSpeed = 1f; // Speed when roaming
    
    [Header("Attack Settings")]
    public int normalAttackDamage = 1;
    public int chargeAttackDamage = 2;
    public float attackCooldown = 2f;
    public float chargeAttackCooldown = 5f;
    public float chargeAttackDistance = 5f; // How far they dash during charge
    public float chargeAttackDuration = 0.5f;
    
    [Header("References")]
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    
    // --- Private State Variables ---
    private enum SwordState
    {
        Idle,
        Roaming,
        Walking,
        Running,
        Attacking,
        ChargeAttacking,
        Damaged,
        Dead
    }
    
    private SwordState currentState = SwordState.Idle;
    private float stateTimer = 0f;
    private float attackTimer = 0f;
    private float chargeAttackTimer = 0f;
    private float walkTimer = 0f;
    private bool isCharging = false;
    private Vector3 chargeDirection;
    private Vector3 chargeStartPosition;
    private Vector3 spawnPosition;
    private Vector3 roamTarget;
    private bool isFacingRight = true; // Track sprite facing direction
    
    // --- Initialization ---
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Store spawn position for roaming
        spawnPosition = transform.position;
        
        // Freeze rotation to prevent sprite from rotating
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
        
        // Set initial animation state
        if (anim != null)
        {
            anim.SetBool("isIdle", true);
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);
            anim.SetBool("isAttacking", false);
            anim.SetBool("isChargeAttack", false);
            anim.SetBool("isDamaged", false);
            anim.SetBool("isDead", false);
        }
        
        stateTimer = idleTime;
        SetRandomRoamTarget();
        Debug.Log($"[{gameObject.name}] Sword initialized in Idle state");
    }
    
    // --- Update Loop ---
    void Update()
    {
        if (currentState == SwordState.Dead) return;
        if (player == null) return;
        
        // Update timers
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        if (chargeAttackTimer > 0) chargeAttackTimer -= Time.deltaTime;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        
        // State machine
        switch (currentState)
        {
            case SwordState.Idle:
                UpdateIdleState(distanceToPlayer);
                break;
                
            case SwordState.Roaming:
                UpdateRoamingState(distanceToPlayer);
                break;
                
            case SwordState.Walking:
                UpdateWalkingState(distanceToPlayer);
                break;
                
            case SwordState.Running:
                UpdateRunningState(distanceToPlayer);
                break;
                
            case SwordState.Attacking:
                // Wait for animation to complete
                break;
                
            case SwordState.ChargeAttacking:
                UpdateChargeAttackState();
                break;
                
            case SwordState.Damaged:
                // Wait for animation to complete
                break;
        }
    }
    
    // --- State Methods ---
    
    void UpdateIdleState(float distanceToPlayer)
    {
        stateTimer -= Time.deltaTime;
        
        if (distanceToPlayer <= detectionRange)
        {
            // Player detected, start moving
            TransitionToWalking();
        }
        else if (stateTimer <= 0)
        {
            // Start roaming after idle time
            TransitionToRoaming();
        }
    }
    
    void UpdateRoamingState(float distanceToPlayer)
    {
        if (distanceToPlayer <= detectionRange)
        {
            // Player detected, switch to chasing
            TransitionToWalking();
            return;
        }
        
        // Move toward roam target
        float distanceToTarget = Vector3.Distance(transform.position, roamTarget);
        
        if (distanceToTarget < 0.5f)
        {
            // Reached roam target, go back to idle
            TransitionToIdle();
        }
        else
        {
            // Continue roaming
            Vector3 directionToTarget = (roamTarget - transform.position).normalized;
            moveDirection = directionToTarget;
            
            // Flip sprite based on roam direction (no player to face)
            FlipSpriteBasedOnDirection(directionToTarget);
            
            base.ApplyAcceleration(roamSpeed);
            float actualSpeed = Mathf.Min(currentSpeed, roamSpeed);
            transform.position = Vector3.MoveTowards(
                transform.position,
                transform.position + moveDirection,
                actualSpeed * Time.deltaTime
            );
        }
    }
    
    void UpdateWalkingState(float distanceToPlayer)
    {
        if (distanceToPlayer > detectionRange)
        {
            // Player out of range, return to idle
            TransitionToIdle();
            return;
        }
        
        // Check for attack range
        if (ShouldAttack(distanceToPlayer))
        {
            return; // Attack methods handle state transition
        }
        
        // Face the player
        FlipSpriteToFacePlayer();
        
        // Move toward player at walk speed
        MoveTowardPlayer(walkSpeed);
        
        // Track walk time for transition to running
        walkTimer += Time.deltaTime;
        if (walkTimer >= walkToRunTransitionTime && distanceToPlayer > attackRange)
        {
            TransitionToRunning();
        }
    }
    
    void UpdateRunningState(float distanceToPlayer)
    {
        if (distanceToPlayer > detectionRange)
        {
            // Player out of range, return to idle
            TransitionToIdle();
            return;
        }
        
        // Check for attack range
        if (ShouldAttack(distanceToPlayer))
        {
            return; // Attack methods handle state transition
        }
        
        // Face the player
        FlipSpriteToFacePlayer();
        
        // Move toward player at run speed
        MoveTowardPlayer(runSpeed);
    }
    
    void UpdateChargeAttackState()
    {
        if (!isCharging) return;
        
        // Continue charging in the set direction
        float actualSpeed = chargeSpeed;
        transform.position = Vector3.MoveTowards(
            transform.position,
            chargeStartPosition + chargeDirection * chargeAttackDistance,
            actualSpeed * Time.deltaTime
        );
        
        // Check if charge is complete
        if (Vector3.Distance(transform.position, chargeStartPosition) >= chargeAttackDistance)
        {
            FinishChargeAttack();
        }
    }
    
    // --- Movement ---
    
    void MoveTowardPlayer(float speed)
    {
        if (player == null) return;
        
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        moveDirection = directionToPlayer;
        
        // Move using the base class acceleration system
        base.ApplyAcceleration(speed);
        
        // Apply movement without rotating sprite
        float actualSpeed = Mathf.Min(currentSpeed, speed);
        transform.position = Vector3.MoveTowards(
            transform.position,
            transform.position + moveDirection,
            actualSpeed * Time.deltaTime
        );
    }
    
    // --- Attack Logic ---
    
    bool ShouldAttack(float distanceToPlayer)
    {
        // Determine if charge attack is available and in range
        if (distanceToPlayer <= chargeAttackRange && 
            distanceToPlayer > attackRange && 
            chargeAttackTimer <= 0)
        {
            StartChargeAttack();
            return true;
        }
        
        // Normal attack if in close range
        if (distanceToPlayer <= attackRange && attackTimer <= 0)
        {
            StartNormalAttack();
            return true;
        }
        
        return false;
    }
    
    void StartNormalAttack()
    {
        currentState = SwordState.Attacking;
        attackTimer = attackCooldown;
        currentSpeed = 0f;
        
        // Face the player before attacking
        FlipSpriteToFacePlayer();
        
        if (anim != null)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);
            anim.SetBool("isAttacking", true);
            anim.SetBool("isChargeAttack", false);
        }
        
        Debug.Log($"[{gameObject.name}] Sword performing normal attack!");
        
        // Deal damage to player if in range
        DealDamageToPlayer(normalAttackDamage);
        
        // Return to idle after attack animation
        Invoke(nameof(FinishNormalAttack), 0.8f);
    }
    
    void FinishNormalAttack()
    {
        if (currentState == SwordState.Attacking)
        {
            TransitionToIdle();
        }
    }
    
    void StartChargeAttack()
    {
        currentState = SwordState.ChargeAttacking;
        chargeAttackTimer = chargeAttackCooldown;
        isCharging = true;
        currentSpeed = 0f;
        
        // Face the player before charging
        FlipSpriteToFacePlayer();
        
        // Set charge direction toward player
        chargeDirection = (player.transform.position - transform.position).normalized;
        chargeStartPosition = transform.position;
        
        if (anim != null)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);
            anim.SetBool("isAttacking", true);
            anim.SetBool("isChargeAttack", true);
        }
        
        Debug.Log($"[{gameObject.name}] Sword performing charge attack!");
        
        // Set duration timer
        Invoke(nameof(FinishChargeAttack), chargeAttackDuration);
    }
    
    void FinishChargeAttack()
    {
        isCharging = false;
        
        // Deal damage to player if in range
        DealDamageToPlayer(chargeAttackDamage);
        
        if (currentState == SwordState.ChargeAttacking)
        {
            TransitionToIdle();
        }
    }
    
    void DealDamageToPlayer(int damage)
    {
        if (player == null || currentState == SwordState.Dead) return;
        
        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance <= attackRange + 0.5f) // Small buffer for charge attack
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
    
    // --- State Transitions ---
    
    void TransitionToIdle()
    {
        currentState = SwordState.Idle;
        stateTimer = idleTime;
        currentSpeed = 0f;
        walkTimer = 0f;
        
        if (anim != null)
        {
            anim.SetBool("isIdle", true);
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);
            anim.SetBool("isAttacking", false);
            anim.SetBool("isChargeAttack", false);
        }
    }
    
    void TransitionToWalking()
    {
        currentState = SwordState.Walking;
        walkTimer = 0f;
        
        if (anim != null)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalking", true);
            anim.SetBool("isRunning", false);
            anim.SetBool("isAttacking", false);
            anim.SetBool("isChargeAttack", false);
        }
    }
    
    void TransitionToRoaming()
    {
        currentState = SwordState.Roaming;
        SetRandomRoamTarget();
        
        if (anim != null)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalking", true); // Use walking animation for roaming
            anim.SetBool("isRunning", false);
            anim.SetBool("isAttacking", false);
            anim.SetBool("isChargeAttack", false);
        }
    }
    
    void TransitionToRunning()
    {
        currentState = SwordState.Running;
        
        if (anim != null)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", true);
            anim.SetBool("isAttacking", false);
            anim.SetBool("isChargeAttack", false);
        }
    }
    
    // --- Sprite Flipping ---
    
    void FlipSpriteToFacePlayer()
    {
        if (player == null || spriteRenderer == null) return;
        
        // Determine if player is to the right or left
        bool playerIsRight = player.transform.position.x > transform.position.x;
        
        // Flip sprite if needed
        if (playerIsRight && !isFacingRight)
        {
            spriteRenderer.flipX = false;
            isFacingRight = true;
        }
        else if (!playerIsRight && isFacingRight)
        {
            spriteRenderer.flipX = true;
            isFacingRight = false;
        }
    }
    
    void FlipSpriteBasedOnDirection(Vector3 direction)
    {
        if (spriteRenderer == null) return;
        
        // Flip based on movement direction
        bool movingRight = direction.x > 0;
        
        if (movingRight && !isFacingRight)
        {
            spriteRenderer.flipX = false;
            isFacingRight = true;
        }
        else if (!movingRight && isFacingRight)
        {
            spriteRenderer.flipX = true;
            isFacingRight = false;
        }
    }
    
    // --- Roaming ---
    
    void SetRandomRoamTarget()
    {
        // Pick a random point within roamRadius of spawn position
        Vector2 randomOffset = Random.insideUnitCircle * roamRadius;
        roamTarget = spawnPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
    }
    
    // --- Damage System ---
    
    public void TakeDamage(int damage)
    {
        // Prevent taking damage if already dead or dying
        if (currentState == SwordState.Dead || currentHealth <= 0) return;
        
        currentHealth -= damage;
        
        Debug.Log($"[{gameObject.name}] Sword took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Show damage animation
            currentState = SwordState.Damaged;
            currentSpeed = 0f;
            
            if (anim != null)
            {
                anim.SetBool("isDamaged", true);
                anim.SetBool("isAttacking", false);
                anim.SetBool("isChargeAttack", false);
            }
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnemyHurt();
            }
            
            // Return to idle after damage animation
            Invoke(nameof(RecoverFromDamage), 0.5f);
        }
    }
    
    void RecoverFromDamage()
    {
        if (anim != null)
        {
            anim.SetBool("isDamaged", false);
        }
        
        if (currentState == SwordState.Damaged)
        {
            TransitionToIdle();
        }
    }
    
    void Die()
    {
        currentState = SwordState.Dead;
        currentSpeed = 0f;
        
        // Cancel any pending attacks or state changes
        CancelInvoke(nameof(FinishNormalAttack));
        CancelInvoke(nameof(FinishChargeAttack));
        CancelInvoke(nameof(RecoverFromDamage));
        
        Debug.Log($"[{gameObject.name}] Sword died!");
        
        if (anim != null)
        {
            anim.SetBool("isDead", true);
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);
            anim.SetBool("isAttacking", false);
            anim.SetBool("isChargeAttack", false);
            anim.SetBool("isDamaged", false);
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyDeath();
        }
        
        // Disable collider immediately to prevent player from taking damage
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Disable this script to prevent any further updates
        this.enabled = false;
        
        // Destroy after death animation completes (increased time for full animation)
        Destroy(gameObject, 2f);
    }
    
    // --- Gizmos for Debugging ---
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw charge attack range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chargeAttackRange);
    }
}
