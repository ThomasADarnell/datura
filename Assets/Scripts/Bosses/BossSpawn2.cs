using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class BossSpawn2 : MonoBehaviour
{
    [Header("Boss Settings")]
    public GameObject swordBossPrefab; // Assign the Sword boss prefab in inspector
    public Transform bossSpawnPoint; // Where the boss should spawn
    
    [Header("Rock Settings")]
    public GameObject evilRockObject; // Assign the evilassrock object directly in inspector (recommended)
    
    [Header("Lighting Settings")]
    public float startIntensity = 0.2f;
    public float targetIntensity = 0.02f;
    public float fadeDuration = 3f; // Duration of the fade in seconds
    
    [Header("Audio Settings")]
    public bool playBossMusic = false; // Optional: trigger boss music
    
    [Header("UI References (Set in Editor)")]
    public GameObject healthUIGroup; // The parent group to show/hide the UI
    public Slider healthSlider;
    public TextMeshPro healthText;
    
    private bool hasTriggered = false;
    private Light2D globalLight;
    private GameObject spawnedBoss;
    private GameObject evilRock;
    private SpriteRenderer rockSpriteRenderer;
    private Collider2D rockCollider;
    
    void Start()
    {
        // Hide health UI initially
        if (healthUIGroup != null)
        {
            healthUIGroup.SetActive(false);
        }
        
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
        
        // Try to find the evil rock object - first check if assigned in inspector
        if (evilRockObject == null)
        {
            Debug.Log($"[{gameObject.name}] Rock not assigned in inspector, searching by name...");
            evilRock = GameObject.Find("evilassrock");
            
            if (evilRock == null)
            {
                // Try with different capitalization
                evilRock = GameObject.Find("EvilAssRock");
                
                if (evilRock == null)
                {
                    evilRock = GameObject.Find("Evilassrock");
                }
            }
        }
        else
        {
            evilRock = evilRockObject;
            Debug.Log($"[{gameObject.name}] Using rock assigned in inspector: {evilRock.name}");
        }
        
        if (evilRock != null)
        {
            rockSpriteRenderer = evilRock.GetComponent<SpriteRenderer>();
            rockCollider = evilRock.GetComponent<Collider2D>();
            
            Debug.Log($"[{gameObject.name}] Found rock: {evilRock.name}, SpriteRenderer: {rockSpriteRenderer != null}, Collider: {rockCollider != null}");
            
            // Start with rock invisible and collider disabled
            if (rockSpriteRenderer != null)
            {
                Color color = rockSpriteRenderer.color;
                color.a = 0f;
                rockSpriteRenderer.color = color;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Rock found but has no SpriteRenderer!");
            }
            
            if (rockCollider != null)
            {
                rockCollider.enabled = false;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Rock found but has no Collider2D!");
            }
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] 'evilassrock' object not found! Please assign it in the inspector or check the object name in the scene.");
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
        // Play rock sound at the beginning
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayRock();
        }
        
        // Enable rock collider immediately
        if (rockCollider != null)
        {
            rockCollider.enabled = true;
            Debug.Log($"[{gameObject.name}] Rock collider enabled");
        }
        
        // Spawn the boss but keep it invisible initially
        Vector3 spawnPosition = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
        
        if (swordBossPrefab == null)
        {
            Debug.LogError($"[{gameObject.name}] Sword boss prefab not assigned!");
            yield break;
        }
        
        spawnedBoss = Instantiate(swordBossPrefab, spawnPosition, Quaternion.identity);
        
        // Get boss components
        SpriteRenderer bossSpriteRenderer = spawnedBoss.GetComponent<SpriteRenderer>();
        Light2D[] bossLights = spawnedBoss.GetComponentsInChildren<Light2D>();
        float[] targetLightIntensities = new float[bossLights.Length];
        
        // Make boss and its children invisible initially
        if (bossSpriteRenderer != null)
        {
            Color bossColor = bossSpriteRenderer.color;
            bossColor.a = 0f;
            bossSpriteRenderer.color = bossColor;
        }
        
        // Store original light intensities and turn them off
        for (int i = 0; i < bossLights.Length; i++)
        {
            targetLightIntensities[i] = bossLights[i].intensity;
            bossLights[i].intensity = 0f;
        }
        
        Debug.Log($"[{gameObject.name}] Boss spawned invisibly at {spawnPosition}");
        
        // Phase 1: Simultaneous fading - darkness down, rock in, boss in
        float elapsedTime = 0f;
        
        Debug.Log($"[{gameObject.name}] Starting fade sequence...");
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            
            // Smooth interpolation using ease-in-out
            float smoothT = t * t * (3f - 2f * t);
            
            // Fade global lighting down
            if (globalLight != null)
            {
                globalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, smoothT);
            }
            
            // Fade rock sprite in
            if (rockSpriteRenderer != null)
            {
                Color rockColor = rockSpriteRenderer.color;
                rockColor.a = Mathf.Lerp(0f, 1f, smoothT);
                rockSpriteRenderer.color = rockColor;
            }
            
            // Fade boss sprite in
            if (bossSpriteRenderer != null)
            {
                Color bossColor = bossSpriteRenderer.color;
                bossColor.a = Mathf.Lerp(0f, 1f, smoothT);
                bossSpriteRenderer.color = bossColor;
            }
            
            // Fade boss lights in
            for (int i = 0; i < bossLights.Length; i++)
            {
                bossLights[i].intensity = Mathf.Lerp(0f, targetLightIntensities[i], smoothT);
            }
            
            yield return null;
        }
        
        // Ensure final values are set
        if (globalLight != null)
        {
            globalLight.intensity = targetIntensity;
        }
        
        if (rockSpriteRenderer != null)
        {
            Color rockColor = rockSpriteRenderer.color;
            rockColor.a = 1f;
            rockSpriteRenderer.color = rockColor;
        }
        
        if (bossSpriteRenderer != null)
        {
            Color bossColor = bossSpriteRenderer.color;
            bossColor.a = 1f;
            bossSpriteRenderer.color = bossColor;
        }
        
        for (int i = 0; i < bossLights.Length; i++)
        {
            bossLights[i].intensity = targetLightIntensities[i];
        }
        
        Debug.Log($"[{gameObject.name}] Fade complete - boss fully visible");
        
        // Phase 2: Now that boss is visible, play spawn taunt and music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySpawnTaunt();
            
            if (playBossMusic)
            {
                AudioManager.Instance.PlayBossTwoMusic();
            }
        }
        
        // Phase 3: Show health bar and set up boss reference
        if (spawnedBoss != null)
        {
            Sword sword = spawnedBoss.GetComponent<Sword>();
            if (sword != null)
            {
                sword.bossSpawner = this;
                UpdateHealthUI(sword.currentHealth, sword.maxHealth);
                
                if (healthUIGroup != null)
                {
                    healthUIGroup.SetActive(true);
                }
            }
        }
        
        Debug.Log($"[{gameObject.name}] Boss spawn sequence complete!");
    }
    
    public void UpdateHealthUI(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        // if (healthText != null)
        // {
        //     healthText.text = $"Sword Health: {current} / {max}";
        // }
    }
    
    public void OnBossDefeated()
    {
        Debug.Log("SWORD BOSS DEFEATED!");
        
        if (healthUIGroup != null)
        {
            healthUIGroup.SetActive(false);
        }
        
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
