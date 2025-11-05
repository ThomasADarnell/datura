using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BossController : MonoBehaviour
{
    [Header("Boss Prefab and Spawn Area")]
    public GameObject flowerPrefab;
    public int maxDoubleSeeds = 3; // Limit for spawning 2 seeds
    public Collider2D spawnArea; // Use a BoxCollider2D or similar to define the room bounds

    [Header("Current Fight State")]
    public int doubleSeedCounter = 0;
    private List<FlowerBoss> activeFlowers = new List<FlowerBoss>();

    void Start()
    {
        if (spawnArea == null)
        {
            Debug.LogError("Spawn Area Collider2D is not set on BossController!");
            enabled = false;
            return;
        }

        // Start the fight with one initial seed
        SpawnNewSeed(GetRandomSpawnPosition());
    }

    // --- Spawning and Seeding ---

    public void HandleSeeding(Vector3 explodedPosition)
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

    private void SpawnNewSeed(Vector3 position)
    {
        // Instantiate the flower at the random position
        GameObject newFlowerObj = Instantiate(flowerPrefab, position, Quaternion.identity, this.transform);
        FlowerBoss newFlower = newFlowerObj.GetComponent<FlowerBoss>();

        // Setup the new flower instance
        newFlower.bossController = this;
        activeFlowers.Add(newFlower);
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
        activeFlowers.Remove(flower);

        if (activeFlowers.Count == 0)
        {
            EndBossFight();
        }
    }

    private void EndBossFight()
    {
        Debug.Log("BOSS DEFEATED! All flowers are gone.");
        SpawnTreasureChest();
    }

    private void SpawnTreasureChest()
    {
        // Logic to instantiate the treasure chest prefab (e.g., at the center of the room)
        Debug.Log("--- TREASURE CHEST SPAWNED ---");
        // Example: Instantiate(treasureChestPrefab, transform.position, Quaternion.identity);
    }
}