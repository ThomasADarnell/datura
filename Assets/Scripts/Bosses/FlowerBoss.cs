using UnityEngine;
using System.Collections;
using System;

public class FlowerBoss : MonoBehaviour
{
    public event Action<int, int> OnHealthChanged;

    // --- Public Parameters ---
    [Header("Core Stats")]
    public float stageDuration = 1.0f; // Time spent in Seed, Sprout, and Budding stages
    public float flowerStageDuration = 2.0f; // Time before explosion in Flower stage
    public float explosionWarningTime = 0.5f; // Time for flashing color before explosion
    public float explosionRadius = 3.0f;
    public int playerDamage = 1; // Damage from explosion
    public GameObject ExplosionEffectPrefab;

    // --- References ---
    [Header("Dependencies")]
    public SpriteRenderer spriteRenderer;
    public Sprite seedSprite;
    public Sprite sproutSprite;
    public Sprite stemSprite;
    public Sprite flowerSprite;
    public Color warningColor = Color.red;
    public BossController bossController; // Reference to the main fight manager

    // --- Private State ---
    private enum BossStage { Seed, Sprout, BuddingStem, Flower }
    private BossStage currentStage = BossStage.Seed;
    private Color originalColor;
    private bool isInvulnerable = true;

    void Start()
    {
        originalColor = spriteRenderer.color;

        // Ensure we have a reference to the main controller
        if (bossController == null)
        {
            Debug.LogError("FlowerBoss requires a reference to the BossController!");
            enabled = false;
            return;
        }

        // Update the health bar
        OnHealthChanged?.Invoke(bossController.currentHealth, bossController.maxHealth);

        // Start the growth cycle
        StartCoroutine(GrowthCycle());
    }

    // --- Stage Progression and Coroutine ---

    IEnumerator GrowthCycle()
    {
        while (true)
        {
            switch (currentStage)
            {
                case BossStage.Seed:
                    spriteRenderer.sprite = seedSprite;
                    isInvulnerable = true;
                    yield return new WaitForSeconds(stageDuration);
                    currentStage = BossStage.Sprout;
                    break;

                case BossStage.Sprout:
                    spriteRenderer.sprite = sproutSprite;
                    isInvulnerable = true;
                    yield return new WaitForSeconds(stageDuration);
                    currentStage = BossStage.BuddingStem;
                    break;

                case BossStage.BuddingStem:
                    spriteRenderer.sprite = stemSprite;
                    isInvulnerable = true;
                    yield return new WaitForSeconds(stageDuration);
                    currentStage = BossStage.Flower;
                    break;

                case BossStage.Flower:
                    spriteRenderer.sprite = flowerSprite;
                    isInvulnerable = false; // VULNERABLE
                    yield return StartCoroutine(FlowerStageRoutine());
                    // If the routine finishes without being defeated, it explodes.
                    ExplodeAndSeed();
                    yield break; // End the cycle for this instance
            }
        }
    }

    IEnumerator FlowerStageRoutine()
    {
        // Wait for the main duration minus the warning time
        yield return new WaitForSeconds(flowerStageDuration - explosionWarningTime);

        // --- Warning Flash ---
        float timer = 0f;
        while (timer < explosionWarningTime)
        {
            // Flash color on and off
            spriteRenderer.color = (timer % 0.1f < 0.05f) ? originalColor : warningColor;
            timer += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = originalColor; // Reset color before explosion
    }


    // --- Damage and Collision Logic ---

    // Note: Use 'OnTriggerEnter2D' or 'OnCollisionEnter2D' for real game implementation.
    // This function is for demonstration and typically called by the player's attack script.
    public void TakeDamage(int damage)
    {
        if (currentStage == BossStage.Flower)
        {
            // --- Take Damage (Success) ---
            bossController.currentHealth -= damage;

            OnHealthChanged?.Invoke(bossController.currentHealth, bossController.maxHealth);

            if (bossController.currentHealth <= 0)
            {
                DefeatFlower();
            }
        }
        else if (isInvulnerable) // Safety check, should be true for non-Flower stages
        {
            StopAllCoroutines();
            // --- Stage Reversion (Penalty) ---
            RevertToPreviousStage();
            // Stop the current coroutine and restart the growth cycle
            
            StartCoroutine(GrowthCycle());
        }

    }

    private void RevertToPreviousStage()
    {
        switch (currentStage)
        {
            case BossStage.Sprout:
                currentStage = BossStage.Seed;
                break;
            case BossStage.BuddingStem:
                currentStage = BossStage.Sprout;
                break;
            case BossStage.Flower:
                currentStage = BossStage.BuddingStem;
                break;
            // Seed Stage hit does nothing/reverts to itself
            default:
                break;
        }
    }


    // --- Defeat and Explosion Handlers ---

    private void DefeatFlower()
    {
        // Flower was successfully defeated/cut before explosion
        Debug.Log("Flower cut! Instance destroyed.");
        bossController.FlowerDefeated(this); // Tell the controller it's gone
        Destroy(gameObject);
    }

    private void ExplodeAndSeed()
    {
        Debug.Log("Flower Exploded! Seeding...");

        if (this.ExplosionEffectPrefab)
        {
            GameObject effect = Instantiate(this.ExplosionEffectPrefab, this.transform.position, Quaternion.identity);
            Destroy(effect, effect.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
        }

        // 1. Damage Player (if in radius)
        DealExplosionDamage();

        // 2. Spawn Seeds (managed by the Controller)
        bossController.HandleSeeding(this.transform.position);

        // 3. Remove this instance
        bossController.FlowerDefeated(this); // Tell the controller it's gone
        Destroy(gameObject);
    }

    private void DealExplosionDamage()
    {
        // Simple 2D distance check to a hypothetical Player object
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance <= explosionRadius)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(1);  // apply damage
                }
            
            }
        }
    }

    // --- Optional: Gizmos for Debugging ---
    private void OnDrawGizmosSelected()
    {
        // Draw the explosion radius in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}