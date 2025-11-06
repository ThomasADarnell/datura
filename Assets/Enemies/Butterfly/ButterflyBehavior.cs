using UnityEngine;

public class ButterflyBehavior : EnemyBaseBehavior
{
    // --- Public Variables (Configurable in Unity Inspector) ---
    public float chaseRange = 5f;
    public Transform pointA;
    public Transform pointB;
    

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

    // --- Core Methods ---

    void Patrol()
    {
        // 1. Calculate the desired direction for the next frame
        moveDirection = (target - transform.position).normalized;

        // 2. Adjust speed based on proximity (Deceleration)
        float desiredSpeed = base.GetDeceleratedSpeed(target);

        // 3. Accelerate current speed towards the desired speed
        base.ApplyAcceleration(desiredSpeed);

        // 4. Move the enemy
        base.MoveEnemy();

        // 5. Check if the target is reached
        CheckForTargetReached();
    }

    void ChasePlayer()
    {
        // The enemy will always try to maintain maxSpeed when chasing
        moveDirection = (player.transform.position - transform.position).normalized;
        float desiredSpeed = maxSpeed;

        // Apply acceleration to reach maxSpeed
        base.ApplyAcceleration(desiredSpeed);

        // Move the enemy
        base.MoveEnemy();
    }

    // --- Helper Methods ---

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