using System.Collections;
using UnityEngine;

public class Bomber : MonoBehaviour
{
    [Header("Vase Child Reference")]
    public GameObject vase; // Reference to the child vase object
    
    [Header("Spawn Settings")]
    public GameObject flyPrefab; // The fly enemy prefab to spawn
    public int minFliesPerSpawn = 1;
    public int maxFliesPerSpawn = 3;
    public float spawnInterval = 20f; // Spawn flies every 20 seconds
    public float spawnRadius = 1f; // How far from the vase flies spawn
    
    [Header("Vase Health")]
    public int vaseHealth = 3; // How many hits before vase breaks
    private int currentVaseHealth;
    
    private Animator vaseAnimator;
    private bool isBreaking = false;
    private bool isBroken = false;
    private float nextSpawnTime;
    
    void Start()
    {
        // Get the vase child if not assigned
        if (vase == null)
        {
            // Try to find a child named "Vase"
            Transform vaseTransform = transform.Find("Vase");
            if (vaseTransform != null)
            {
                vase = vaseTransform.gameObject;
            }
            else
            {
                Debug.LogError("Vase child object not found! Please assign it in the inspector or name the child 'Vase'.");
                return;
            }
        }
        
        // Get vase animator
        vaseAnimator = vase.GetComponent<Animator>();
        if (vaseAnimator == null)
        {
            Debug.LogWarning("Vase does not have an Animator component!");
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
            // Calculate random position around the vase
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = vase.transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
            
            // Spawn the fly
            GameObject fly = Instantiate(flyPrefab, spawnPosition, Quaternion.identity);
            
            // Optionally parent the fly to this object or leave it independent
            // fly.transform.parent = transform;
        }
        
        Debug.Log($"Spawned {flyCount} flies from vase!");
    }
    
    public void TakeDamage(int damage)
    {
        if (isBreaking || isBroken) return;
        
        currentVaseHealth -= damage;
        
        if (currentVaseHealth <= 0)
        {
            StartBreaking();
        }
        else if (currentVaseHealth == vaseHealth / 2) // If at half health
        {
            // Set damaged animation
            if (vaseAnimator != null)
            {
                vaseAnimator.SetBool("Damaged", true);
            }
        }
    }
    
    void StartBreaking()
    {
        if (isBreaking) return;
        
        isBreaking = true;
        
        // Set breaking animation
        if (vaseAnimator != null)
        {
            vaseAnimator.SetBool("Broken", true);
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
        
        // Destroy the entire bomber object (including the vase)
        Destroy(gameObject);
    }
    
    // Gizmos for visualizing spawn radius
    private void OnDrawGizmosSelected()
    {
        if (vase != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(vase.transform.position, spawnRadius);
        }
    }
}
