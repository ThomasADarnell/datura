using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class BossSpawn2 : MonoBehaviour
{
    [Header("Boss Settings")]
    public GameObject swordBossPrefab; // Assign the Sword boss prefab in inspector
    public Transform bossSpawnPoint; // Where the boss should spawn
    
    [Header("Lighting Settings")]
    public float startIntensity = 0.2f;
    public float targetIntensity = 0.02f;
    public float fadeDuration = 3f; // Duration of the fade in seconds
    
    [Header("Audio Settings")]
    public bool playBossMusic = false; // Optional: trigger boss music
    
    private bool hasTriggered = false;
    private Light2D globalLight;
    private GameObject spawnedBoss;
    
    void Start()
    {
        // Find the global lighting object
        GameObject lightingObject = GameObject.FindGameObjectWithTag("GlobalLighting");
        
        if (lightingObject != null)
        {
            globalLight = lightingObject.GetComponent<Light2D>();
            
            if (globalLight == null)
            {
                Debug.LogWarning($"[{gameObject.name}] GlobalLighting object found but has no Light2D component!");
            }
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] No object with 'GlobalLighting' tag found!");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player entered the trigger and hasn't triggered before
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            Debug.Log($"[{gameObject.name}] Player entered boss area, starting sequence...");
            StartCoroutine(BossSpawnSequence());
        }
    }
    
    private IEnumerator BossSpawnSequence()
    {
        // Phase 1: Fade the lighting
        if (globalLight != null)
        {
            float elapsedTime = 0f;
            float initialIntensity = globalLight.intensity;
            
            Debug.Log($"[{gameObject.name}] Fading light from {initialIntensity} to {targetIntensity}");
            
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeDuration;
                
                // Smooth interpolation using ease-in-out
                float smoothT = t * t * (3f - 2f * t);
                globalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, smoothT);
                
                yield return null;
            }
            
            // Ensure final value is set
            globalLight.intensity = targetIntensity;
            Debug.Log($"[{gameObject.name}] Lighting fade complete");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] Skipping lighting fade (no Light2D found)");
            yield return new WaitForSeconds(fadeDuration); // Still wait even if no light
        }
        
        // Phase 2: Spawn the boss
        if (swordBossPrefab != null)
        {
            Vector3 spawnPosition = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
            spawnedBoss = Instantiate(swordBossPrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"[{gameObject.name}] Sword boss spawned at {spawnPosition}");
            
            // Optional: Play boss music
            if (playBossMusic && AudioManager.Instance != null)
            {
                // Assuming AudioManager has a boss music method
                // AudioManager.Instance.PlayBossMusic();
            }
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Sword boss prefab not assigned!");
        }
    }
    
    // Optional: Restore lighting when boss is defeated
    public void OnBossDefeated()
    {
        StartCoroutine(RestoreLighting());
    }
    
    private IEnumerator RestoreLighting()
    {
        if (globalLight != null)
        {
            float elapsedTime = 0f;
            float currentIntensity = globalLight.intensity;
            
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeDuration;
                float smoothT = t * t * (3f - 2f * t);
                globalLight.intensity = Mathf.Lerp(currentIntensity, startIntensity, smoothT);
                
                yield return null;
            }
            
            globalLight.intensity = startIntensity;
            Debug.Log($"[{gameObject.name}] Lighting restored");
        }
        
        yield return null;
    }
}
