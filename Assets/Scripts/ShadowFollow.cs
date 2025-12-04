using UnityEngine;

public class ShadowFollow : MonoBehaviour
{
    private Transform player;
    private GameObject playerObject;
    public float followSpeed = 0.5f;
    public GameObject monsterPrefab;
    public float spawnHeight = 5f;

    private bool triggered = false; // Tracks if monster has been spawned
    private static ShadowFollow currentActiveShadow; // Static reference for the monster to find

    void Awake()
    {
        // Ensure only one shadow is "active" for the monster to report back to
        currentActiveShadow = this;
    }
    void LateUpdate()
    {
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (player != null && !triggered)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, followSpeed * Time.deltaTime);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            Debug.Log("Shadow hit the player! Spawning monster...");
            SpawnMonster(transform.position);
        }
    }

    void SpawnMonster(Vector3 targetPosition)
    {
        if (monsterPrefab != null && player != null)
        {
            Vector3 spawnPosition = player.position + Vector3.up * spawnHeight;
            GameObject spawnedMonster = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
            
            AngelTrumpet monsterAttack = spawnedMonster.GetComponent<AngelTrumpet>();
            if (monsterAttack != null)
            {
                monsterAttack.SetTargetLocation(targetPosition);
                // NEW: Pass a reference to THIS shadow instance to the monster
                monsterAttack.SetShadowSpawner(this); 
            }
        }
    }

    // Public method called by the MonsterAttack script when the monster is destroyed
    public void NotifyMonsterDestroyed()
    {
        Debug.Log("Monster reported destruction. Shadow disappearing now.");
        Destroy(gameObject); // Destroy the shadow GameObject
    }
}