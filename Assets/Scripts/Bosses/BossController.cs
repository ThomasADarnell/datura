using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class BossController : MonoBehaviour
{
    [Header("Boss Prefab and Spawn Area")]
    public GameObject flowerPrefab;
    public int maxDoubleSeeds = 14; // Limit for spawning 2 seeds
    public Collider2D spawnArea; // Use a BoxCollider2D or similar to define the room bounds
    public int currentHealth;
    public int maxHealth = 10;



    [Header("UI References (Set in Editor)")]
    // **NOTE: You must add a 'using TMPro;' and 'using UnityEngine.UI;' at the top**
    public GameObject healthUIGroup; // The parent group to show/hide the UI
    public Slider healthSlider;
    public TextMeshProUGUI healthText;


    [Header("Current Fight State")]
    public int doubleSeedCounter = 0;
    private List<FlowerBoss> activeFlowers = new List<FlowerBoss>();
    private bool hasFightStarted = false;

    void Start()
    {
        currentHealth = maxHealth;
        if (spawnArea == null)
        {
            Debug.LogError("Spawn Area Collider2D is not set on BossController!");
            enabled = false;
            return;
        }

    }

    // --- Spawning and Seeding ---
    public void StartBossFight()
    {
        if (hasFightStarted)
        {
            // Fight already running, do nothing.
            return;
        }

        hasFightStarted = true;
        if(AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBossOneMusic();
        }
        Debug.Log("Player entered the boss room. Starting Flower Boss fight!");

        // Start the fight with one initial seed
        SpawnNewSeed(GetRandomSpawnPosition());
    }

    public void HandleSeeding(Vector3 explodedPosition)
    {
        Debug.Log(currentHealth);
        if (currentHealth > 0)
        {
            int seedsToSpawn = 1;

            if (doubleSeedCounter < maxDoubleSeeds)
            {
                seedsToSpawn = 2;
                doubleSeedCounter++;
                Debug.Log($"Double Seed Cycle used: {doubleSeedCounter}/{maxDoubleSeeds}");
            }
            else
            {
                Debug.Log("Double Seed Limit reached. Spawning one seed.");
            }

            for (int i = 0; i < seedsToSpawn; i++)
            {
                SpawnNewSeed(GetRandomSpawnPosition());
            }
        }
    }

    private void SpawnNewSeed(Vector3 position)
    {
        // Instantiate the flower at the random position
        GameObject newFlowerObj = Instantiate(flowerPrefab, position, Quaternion.identity, this.transform);
        FlowerBoss newFlower = newFlowerObj.GetComponent<FlowerBoss>();

        // Setup the new flower instance
        newFlower.bossController = this;
        activeFlowers.Add(newFlower);

        // NEW: Subscribe to the health event of the newly spawned flower
        newFlower.OnHealthChanged += UpdateHealthUI;

        // NEW: Show the UI when the first flower spawns
        if (healthUIGroup != null)
        {
            healthUIGroup.SetActive(true);
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // Calculate a random point within the defined spawn area bounds
        Bounds bounds = spawnArea.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector3(x, y, 0); // Assuming 2D, Z is 0
    }

    // --- Boss Defeat Check ---

    public void FlowerDefeated(FlowerBoss flower)
    {
        flower.OnHealthChanged -= UpdateHealthUI;

        activeFlowers.Remove(flower);

        if (activeFlowers.Count == 0 || currentHealth <= 0)
        {
            EndBossFight();
        }
        else
        {
            // If another flower exists, update UI with its current health
            UpdateHealthUI(activeFlowers[0].bossController.currentHealth, activeFlowers[0].bossController.maxHealth); // Reset to full health for the next flower phase
        }
    }

    public void UpdateHealthUI(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        if (healthText != null)
        {
            // Example: "Boss Health: 5/10"
            healthText.text = $"Flower Health: {current} / {max}";
        }

        // Since the fight involves multiple flower instances, we assume the UI shows the health 
        // of the *currently active* flower the player is hitting.
    }

    private void EndBossFight()
    {
        Debug.Log("BOSS DEFEATED! All flowers are gone.");
        if (healthUIGroup != null)
        {
            healthUIGroup.SetActive(false);
        }
        SceneManager.LoadScene("Win Screen");
    }
}