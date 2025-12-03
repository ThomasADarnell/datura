using System.Collections;
using UnityEngine;

public class Bomber : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject flyPrefab; // The fly enemy prefab to spawn
    public int minFliesPerSpawn = 1;
    public int maxFliesPerSpawn = 3;
    public float spawnInterval = 20f; // Spawn flies every 20 seconds
    public float spawnRadius = 1f; // How far from the vase flies spawn
    
    [Header("Vase Health")]
    public int vaseHealth = 3; // How many hits before vase breaks
    private int currentVaseHealth;
    
    private Animator anim;
    private bool isBreaking = false;
    private bool isBroken = false;
    private float nextSpawnTime;
    
    void Start()
    {
        // Get animator from this object
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogWarning("Bomber does not have an Animator component!");
        }
        
        // Initialize health
        currentVaseHealth = vaseHealth;
        
        // Set first spawn time
        nextSpawnTime = Time.time + spawnInterval;
    }
    
    void Update()
    {
        // Don't spawn if breaking or broken
        if (isBreaking || isBroken) return;
        
        // Check if it's time to spawn flies
        if (Time.time >= nextSpawnTime)
        {
            SpawnFlies();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }
    
    void SpawnFlies()
    {
        if (flyPrefab == null)
        {
            Debug.LogError("Fly prefab not assigned!");
            return;
        }
        
        // Determine how many flies to spawn
        int flyCount = Random.Range(minFliesPerSpawn, maxFliesPerSpawn + 1);
        
        // Spawn flies around the vase
        for (int i = 0; i < flyCount; i++)
        {
            // Calculate random position around this vase
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
            
            // Spawn the fly
            GameObject fly = Instantiate(flyPrefab, spawnPosition, Quaternion.identity);
            
            // Parent flies under the vase for organization
            fly.transform.parent = transform;
        }
        
        Debug.Log($"Spawned {flyCount} flies from vase!");
    }
    
    public void TakeDamage(int damage)
    {
        if (isBreaking || isBroken) return;
        
        currentVaseHealth -= damage;
        
        Debug.Log($"Vase took {damage} damage! Current health: {currentVaseHealth}/{vaseHealth}");
        
        if (currentVaseHealth <= 0)
        {
            StartBreaking();
        }
        else if (currentVaseHealth <= vaseHealth / 2) // If at half health or less
        {
            // Set damaged animation
            if (anim != null)
            {
                anim.SetBool("Damaged", true);
            }
        }
    }
    
    void StartBreaking()
    {
        if (isBreaking) return;
        
        isBreaking = true;
        
        // Set breaking animation
        if (anim != null)
        {
            anim.SetBool("Broken", true);
        }
        
        // Start breaking coroutine
        StartCoroutine(BreakVase());
    }
    
    IEnumerator BreakVase()
    {
        // Wait for breaking animation to complete
        yield return new WaitForSeconds(1f); // Adjust based on animation length
        
        FinishBreaking();
    }
    
    void FinishBreaking()
    {
        isBroken = true;
        
        // Destroy the vase object
        Destroy(gameObject);
    }
    
    // === Animation Event Functions ===
    // These are called by Animation Events in the vase's animations
    
    // Called when the vase breaking animation is complete
    public void OnVaseBroken()
    {
        Debug.Log("Vase breaking animation completed");
        FinishBreaking();
    }
    
    // Called when vase damage animation is complete (optional)
    public void OnVaseDamaged()
    {
        Debug.Log("Vase damage animation completed");
    }
    
    // Called at the start of breaking animation to stop spawning
    public void OnVaseStartBreaking()
    {
        Debug.Log("Vase started breaking");
        isBreaking = true;
    }
    
    // Gizmos for visualizing spawn radius
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
