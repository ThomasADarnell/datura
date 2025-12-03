using UnityEngine;

public class AngelTrumpet : MonoBehaviour
{
    public int damageAmount = 10;
    public float moveSpeed = 10f;
    public float arrivalThreshold = 0.1f;
    public float exitHeight = 5f;
    public int monsterHealth = 1; 

    private bool hasAttackedPlayer = false; 
    private Vector3 currentTargetLocation;
    private bool locationSet = false;
    private bool exiting = false;
    private bool isKilledByPlayer = false; 
    private bool isDestroyed = false; // New flag to prevent update calls after destruction intent

    private ShadowFollow spawningShadow; // NEW: Field to hold the specific spawner reference

    public void SetShadowSpawner(ShadowFollow shadow)
    {
        spawningShadow = shadow;
    }

    // A public method to receive damage from the player's script
    public void TakeDamage(int damage)
    {
        // This is the function called by the PerformAttack() method.
        monsterHealth -= damage;
        Debug.Log("Monster took " + damage + " damage via Player Attack. Remaining Health: " + monsterHealth);

        if (monsterHealth <= 0)
        {
            // !! IMPORTANT: Set this flag to true when killed by the player's damage call !!
            isKilledByPlayer = true; 
            DestroyMonster();
        }
    }

    // Called by the ShadowFollow script
    public void SetTargetLocation(Vector3 location)
    {
        currentTargetLocation = location;
        locationSet = true;
    }
    
    // ... Update() method and movement logic remain the same ...

    void Update()
    {
        // Removed the "monsterHealth > 0" check here. 
        // We only stop movement if the object is pending destruction.
        if (locationSet && !isDestroyed) 
        {
            transform.position = Vector3.MoveTowards(transform.position, currentTargetLocation, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, currentTargetLocation) < arrivalThreshold)
            {
                if (!exiting)
                {
                    StartExitSequence();
                }
                else
                {
                    // Reached top of screen, destroy naturally
                    isKilledByPlayer = false; 
                    DestroyMonster();
                }
            }
        }
    }

    void StartExitSequence()
    {
        exiting = true;
        currentTargetLocation = transform.position + Vector3.up * exitHeight;
    }

    // Keep the OnTrigger logic *only* for the damage part to the player
    void OnTriggerEnter(Collider other) // Use OnTriggerEnter2D for 2D games
    {
        if (!hasAttackedPlayer && monsterHealth > 0 && other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                hasAttackedPlayer = true; // Prevents multiple damage instances to the player
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (monsterHealth > 0 && other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                // REMOVED: hasAttackedPlayer = true;
            }
        }
    }


    void DestroyMonster()
    {
        if (isDestroyed) return; // Prevent double destruction
        isDestroyed = true; // Set the guard flag

        // Notification logic now uses the direct reference
        if (isKilledByPlayer)
        {
            if (spawningShadow != null)
            {
                // Call the correct instance's notification method
                spawningShadow.NotifyMonsterDestroyed();
            }
        }
        
        Destroy(gameObject); 
    }
}
