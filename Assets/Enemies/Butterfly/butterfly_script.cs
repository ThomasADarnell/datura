using UnityEngine;

public class NaturalCombinedEnemyBehavior : MonoBehaviour
{
    // --- Public Variables (Configurable in Unity Inspector) ---
    public float maxSpeed = 3f;
    public float chaseRange = 5f;
    public float accelerationRate = 4f; // How fast the enemy speeds up
    public float decelerationRate = 5f; // How fast the enemy slows down
    public float stoppingDistance = 1.5f; // Distance from target where slow-down begins
    public Transform pointA;
    public Transform pointB;
    public float damageCooldown = 2f;

    // --- Private Variables ---
    private Vector3 target;
    private GameObject player;
    private float currentSpeed = 0f; // New: Tracks the speed in the current frame
    private Vector3 moveDirection; // Tracks the desired movement direction
    private float nextDamageTime = 0f;

    // --- Initialization ---
    void Start()
    {
        target = pointA.position;
        player = GameObject.FindGameObjectWithTag("Player");
        // Initialize the move direction
        moveDirection = (target - transform.position).normalized;
    }

    // --- Update Loop ---
    void Update()
    {
        // Prioritize Chase state
        if (player != null && Vector2.Distance(transform.position, player.transform.position) < chaseRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Time.time >= nextDamageTime)
        { // When player runs into the shrub
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();  // get player health
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);  // apply damage
            }
            nextDamageTime = Time.time + damageCooldown;
        }
    }

    // --- Core Methods ---

    void Patrol()
    {
        // 1. Calculate the desired direction for the next frame
        moveDirection = (target - transform.position).normalized;

        // 2. Adjust speed based on proximity (Deceleration)
        float desiredSpeed = GetDeceleratedSpeed(target);

        // 3. Accelerate current speed towards the desired speed
        ApplyAcceleration(desiredSpeed);

        // 4. Move the enemy
        MoveEnemy();

        // 5. Check if the target is reached
        CheckForTargetReached();
    }

    void ChasePlayer()
    {
        // The enemy will always try to maintain maxSpeed when chasing
        moveDirection = (player.transform.position - transform.position).normalized;
        float desiredSpeed = maxSpeed;

        // Apply acceleration to reach maxSpeed
        ApplyAcceleration(desiredSpeed);

        // Move the enemy
        MoveEnemy();
    }

    // --- Helper Methods ---

    // **1. Acceleration Logic**
    void ApplyAcceleration(float desiredSpeed)
    {
        // Use Mathf.MoveTowards to smoothly change currentSpeed towards the desiredSpeed
        // The change is limited by accelerationRate * Time.deltaTime
        currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed, accelerationRate * Time.deltaTime);
    }

    // **2. Deceleration Logic**
    float GetDeceleratedSpeed(Vector3 currentTarget)
    {
        float distance = Vector3.Distance(transform.position, currentTarget);

        if (distance < stoppingDistance)
        {
            // Calculate a slow-down factor based on distance
            float speedFactor = (distance / stoppingDistance);

            // Multiply maxSpeed by the speed factor and the decelerationRate
            float calculatedSpeed = maxSpeed * speedFactor * decelerationRate;

            // Ensure calculated speed is not higher than maxSpeed
            return Mathf.Min(calculatedSpeed, maxSpeed);
        }

        // If not close enough to stop, the desired speed is simply maxSpeed
        return maxSpeed;
    }

    // **3. Movement Logic**
    void MoveEnemy()
    {
        // Clamp the speed to prevent overshooting maxSpeed due to acceleration
        float actualSpeed = Mathf.Min(currentSpeed, maxSpeed);

        // Apply movement using the calculated currentSpeed and direction
        transform.position = Vector3.MoveTowards(
            transform.position,
            transform.position + moveDirection,
            actualSpeed * Time.deltaTime
        );

        // Optional: Rotate the enemy to face the direction of movement
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    // **4. Target Switch Logic**
    void CheckForTargetReached()
    {
        // We check against a very small value since deceleration should bring the speed close to zero
        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            // Switch the target point
            target = target == pointA.position ? pointB.position : pointA.position;

            // Important: Reset the current speed to zero so the next Patrol movement starts with acceleration
            // (You could also implement a brief Idle state here if you wanted a pause!)
            currentSpeed = 0f;
        }
    }
}