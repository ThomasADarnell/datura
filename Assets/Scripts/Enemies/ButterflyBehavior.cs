using UnityEngine;

public class ButterflyBehavior : EnemyBaseBehavior
{
    // --- Public Variables (Configurable in Unity Inspector) ---
    public float chaseRange = 8f;
    public float swarmDistance = 2f; // Distance to maintain around player when swarming
    public float swoopDistance = 1f; // How close butterfly gets during swoop attacks
    public float swoopCooldown = 3f; // Time between swoop attacks
    public float circleSpeed = 2f; // Speed multiplier for circling around player
    public Rect movementBounds = new Rect(3.9f, -21.8f, 33.1f, 61.7f); // Define the bounds for movement
    public GameObject boundsObject; // Reference to the invisible sprite or object defining bounds
    
    private bool isChasing = false;
    private bool isSwarming = false;
    private bool isSwooping = false;
    private float swarmAngle; // Current angle around the player
    private float swoopTimer = 0f;
    private Vector3 swoopTarget;
    private Vector3 swarmCenter; // Point to circle around when swarming
    
    // Patrol variables (for when not chasing player)
    private Vector3 patrolTarget;
    private float patrolRadius = 10f;
    private float patrolTimer = 0f;
    private float patrolChangeTime = 5f;



    // --- Initialization ---
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        // Dynamically find boundsObject if not assigned
        if (boundsObject == null)
        {
            boundsObject = GameObject.FindWithTag("BoundsArea"); // Ensure the bounds object has the tag "BoundsArea"
            if (boundsObject == null)
            {
                Debug.LogError("Bounds object not found in the scene. Ensure it has the tag 'BoundsArea'.");
                return;
            }
        }

        // Initialize movement bounds from boundsObject
        var collider = boundsObject.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            movementBounds = new Rect(
                collider.bounds.min.x,
                collider.bounds.min.y,
                collider.bounds.size.x,
                collider.bounds.size.y
            );
        }
        else
        {
            Debug.LogError("Bounds object does not have a BoxCollider2D component.");
        }

        // Initialize patrol behavior
        patrolTarget = transform.position + (Vector3)Random.insideUnitCircle.normalized * patrolRadius;
        swarmAngle = Random.Range(0f, 360f); // Random starting angle for swarming
        
        Debug.Log($"[{gameObject.name}] Butterfly initialized");
        
        // Initialize the move direction toward patrol target
        moveDirection = (patrolTarget - transform.position).normalized;
    }

    // --- Update Loop ---
    void Update()
    {
        // Update swoop timer
        if (swoopTimer > 0f)
        {
            swoopTimer -= Time.deltaTime;
        }

        // Check if player is in range
        if (player != null && Vector2.Distance(transform.position, player.transform.position) < chaseRange)
        {
            if (!isChasing)
            {
                // Just entered chase mode
                isChasing = true;
                isSwarming = false;
                isSwooping = false;
                swarmCenter = player.transform.position;
            }
            
            ChaseAndSwarmPlayer();
        }
        else
        {
            // Out of range - return to patrol
            isChasing = false;
            isSwarming = false;
            isSwooping = false;
            Patrol();
        }
    }

    // --- Core Methods ---

    void Patrol()
    {
        // Update patrol timer
        patrolTimer += Time.deltaTime;
        
        // Change patrol target periodically
        if (patrolTimer >= patrolChangeTime || Vector3.Distance(transform.position, patrolTarget) < 0.5f)
        {
            patrolTarget = transform.position + (Vector3)Random.insideUnitCircle.normalized * patrolRadius;
            patrolTimer = 0f;
        }
        
        // Move toward patrol target
        moveDirection = (patrolTarget - transform.position).normalized;
        
        // Use reduced speed for patrol
        float desiredSpeed = maxSpeed * 0.5f;
        base.ApplyAcceleration(desiredSpeed);
        base.MoveEnemy();
        
        // Clamp position to movement bounds
        transform.position = ClampToBounds(transform.position);
    }

    void ChaseAndSwarmPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        
        // If close enough, enter swarming behavior
        if (distanceToPlayer <= swarmDistance)
        {
            if (!isSwarming)
            {
                isSwarming = true;
                swarmCenter = player.transform.position;
            }
            
            SwarmAroundPlayer();
        }
        else
        {
            // Fly directly toward player
            isSwarming = false;
            isSwooping = false;
            
            moveDirection = (player.transform.position - transform.position).normalized;
            float desiredSpeed = maxSpeed;
            
            base.ApplyAcceleration(desiredSpeed);
            base.MoveEnemy();
        }
    }
    
    void SwarmAroundPlayer()
    {
        // Update swarm center to follow player
        swarmCenter = Vector3.Lerp(swarmCenter, player.transform.position, Time.deltaTime * 2f);
        
        // Check if we should start a swoop attack
        if (swoopTimer <= 0f && !isSwooping && Random.Range(0f, 1f) < 0.02f) // 2% chance per frame
        {
            StartSwoop();
        }
        
        if (isSwooping)
        {
            PerformSwoop();
        }
        else
        {
            CircleAroundPlayer();
        }
    }
    
    void CircleAroundPlayer()
    {
        // Increment angle to circle around player
        swarmAngle += circleSpeed * Time.deltaTime * 50f; // 50 degrees per second at default speed
        if (swarmAngle >= 360f) swarmAngle -= 360f;
        
        // Calculate position on circle around player
        float angleRad = swarmAngle * Mathf.Deg2Rad;
        Vector3 circlePos = swarmCenter + new Vector3(
            Mathf.Cos(angleRad) * swarmDistance,
            Mathf.Sin(angleRad) * swarmDistance,
            0f
        );
        
        // Move toward circle position
        moveDirection = (circlePos - transform.position).normalized;
        
        float desiredSpeed = maxSpeed * circleSpeed;
        base.ApplyAcceleration(desiredSpeed);
        base.MoveEnemy();
    }
    
    void StartSwoop()
    {
        isSwooping = true;
        swoopTimer = swoopCooldown;
        
        // Set swoop target slightly past the player
        Vector3 playerPos = player.transform.position;
        Vector3 directionToPlayer = (playerPos - transform.position).normalized;
        swoopTarget = playerPos + directionToPlayer * swoopDistance;
    }
    
    void PerformSwoop()
    {
        float distanceToSwoopTarget = Vector3.Distance(transform.position, swoopTarget);
        
        if (distanceToSwoopTarget > 0.2f)
        {
            // Still swooping toward target
            moveDirection = (swoopTarget - transform.position).normalized;
            float desiredSpeed = maxSpeed * 1.5f; // Faster during swoop
            
            base.ApplyAcceleration(desiredSpeed);
            base.MoveEnemy();
        }
        else
        {
            // Swoop complete, return to circling
            isSwooping = false;
        }
    }

    // --- Helper Methods ---
    private Vector3 ClampToBounds(Vector3 position)
    {
        return new Vector3(
            Mathf.Clamp(position.x, movementBounds.xMin, movementBounds.xMax),
            Mathf.Clamp(position.y, movementBounds.yMin, movementBounds.yMax),
            position.z
        );
    }

    // private void OnDrawGizmos()
    // {
    //     if (currentPath != null && currentPath.Count > 0)
    //     {
    //         // Draw all path points as yellow solid spheres
    //         Gizmos.color = Color.yellow;
    //         foreach (var point in currentPath)
    //         {
    //             Vector3 pos = new Vector3(point.x, point.y, 5);
    //             Gizmos.DrawSphere(pos, 0.1f);
    //         }
            
    //         // Draw current target node as a larger red sphere
    //         if (currentPathIndex < currentPath.Count)
    //         {
    //             Gizmos.color = Color.red;
    //             Vector3 currentTarget = new Vector3(currentPath[currentPathIndex].x, currentPath[currentPathIndex].y, 5);
    //             Gizmos.DrawSphere(currentTarget, 0.15f);
    //         }

    //         // Draw ultimate target point in blue
    //         Gizmos.color = Color.blue;
    //         Vector3 finalTarget = new Vector3(target.x, target.y, 5);
    //         Gizmos.DrawSphere(finalTarget, 0.2f);
    //     }

    //     // Always draw current position and target
    //     Gizmos.color = Color.green;
    //     Gizmos.DrawSphere(transform.position, 0.15f);
    // }
}