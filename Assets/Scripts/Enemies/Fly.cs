using System.Collections;
using UnityEngine;

public class Fly : EnemyBaseBehavior
{
    [Header("Fly Stats")]
    public int currentHealth = 1;
    public int maxHealth = 1;
    
    [Header("Explosion Settings")]
    public float explosionRadius = 1.5f; // Radius to explode when near player
    public int explosionDamage = 1;
    public GameObject explosionEffectPrefab;
    public float explosionDelay = 0.3f; // Time before actual explosion after attack animation starts
    
    [Header("References")]
    private Animator anim;
    private bool isDying = false;
    private bool isExploding = false;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        anim = GetComponent<Animator>();
        
        if (player == null)
        {
            Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");
        }
        
        // Start moving immediately after spawning
        if (anim != null)
        {
            anim.SetBool("FinishedCurrentAnim", true);
        }
    }
    
    void Update()
    {
        if (isDying || isExploding || player == null) return;
        
        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        
        // Check if close enough to explode
        if (distanceToPlayer <= explosionRadius)
        {
            StartExplosion();
            return;
        }
        
        // Move toward player
        MoveTowardPlayer();
    }
    
    void MoveTowardPlayer()
    {
        // Calculate direction to player
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        moveDirection = directionToPlayer;
        
        // Use max speed to chase player
        float desiredSpeed = maxSpeed;
        base.ApplyAcceleration(desiredSpeed);
        base.MoveEnemy();
    }
    
    void StartExplosion()
    {
        if (isExploding) return;
        
        isExploding = true;
        
        // Stop movement
        currentSpeed = 0f;
        
        // Set attack animation
        if (anim != null)
        {
            anim.SetBool("IsAttacking", true);
            anim.SetBool("FinishedCurrentAnim", false);
        }
        
        // Start explosion coroutine
        StartCoroutine(ExplodeAfterDelay());
    }
    
    IEnumerator ExplodeAfterDelay()
    {
        // Wait for explosion delay
        yield return new WaitForSeconds(explosionDelay);
        
        // Spawn explosion effect
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Animator effectAnim = effect.GetComponent<Animator>();
            if (effectAnim != null)
            {
                Destroy(effect, effectAnim.GetCurrentAnimatorStateInfo(0).length);
            }
            else
            {
                Destroy(effect, 1f);
            }
        }
        
        // Deal damage to player if in range
        DealExplosionDamage();
        
        // Play death sound if available
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyDeath();
        }
        
        // Destroy the fly
        Destroy(gameObject);
    }
    
    void DealExplosionDamage()
    {
        if (player == null) return;
        
        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance <= explosionRadius)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(explosionDamage);
            }
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (isDying || isExploding) return;
        
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Play damage animation
            if (anim != null)
            {
                anim.SetBool("WasDamaged", true);
                anim.SetBool("FinishedCurrentAnim", false);
                Invoke(nameof(ResetDamageFlag), 0.5f);
            }
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
        if (isDying) return;
        
        isDying = true;
        
        if (anim != null)
        {
            anim.SetBool("IsDead", true);
            anim.SetBool("FinishedCurrentAnim", false);
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyDeath();
        }
        
        // Destroy after animation completes
        Destroy(gameObject, 1f);
    }
    
    // Gizmos for debugging explosion radius
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
