using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyBaseBehavior : MonoBehaviour
{
    public float maxSpeed = 2.5f;
    public float accelerationRate = 2f; // How fast the enemy speeds up
    public float decelerationRate = 5f; // How fast the enemy slows down
    public float stoppingDistance = 1.5f; // Distance from target where slow-down begins
    public float damageCooldown = 2f;

    protected Transform playerTransform;
    protected Vector3 target;
    protected GameObject player;
    protected float currentSpeed = 0f; // New: Tracks the speed in the current frame
    protected Vector3 moveDirection; // Tracks the desired movement direction
    protected float nextDamageTime = 0f;

    protected void Start()
    {
        // Find the player object by tag once
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("EnemyBaseBehavior could not find an object with the 'Player' tag.");
        }
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Time.time >= nextDamageTime)
        { // When player and enemy collide
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();  // get player health
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);  // apply damage
            }
            nextDamageTime = Time.time + damageCooldown;
        }
    }
    protected void MoveEnemy()
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
    protected void ApplyAcceleration(float desiredSpeed)
    {
        // Use Mathf.MoveTowards to smoothly change currentSpeed towards the desiredSpeed
        // The change is limited by accelerationRate * Time.deltaTime
        currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed, accelerationRate * Time.deltaTime);
    }
    protected float GetDeceleratedSpeed(Vector3 currentTarget)
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
}
