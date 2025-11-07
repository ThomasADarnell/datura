using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class ButterflyBehavior : EnemyBaseBehavior
{
    // --- Public Variables (Configurable in Unity Inspector) ---
    public float chaseRange = 5f;
    public Tilemap walkableAreaTilemap;
    private Vector2 pointA;
    private Vector2 pointB;
    private int walkableRange = 30;
    private List<Vector2> currentPath;
    private int cycleCount = 0;
    private int currentPathIndex = 0;
    private float pathNodeReachedThreshold = 0.1f;
    private Vector3 lastPosition;
    private float stuckTime = 0f;
    private bool isChasing = false;
    private const float STUCK_THRESHOLD = 1.5f; // Time in seconds before considering stuck
    private const float MOVEMENT_THRESHOLD = 0.05f; // Distance that must be moved to not be considered stuck



    // --- Initialization ---
    void Start()
    {
        if (walkableAreaTilemap == null)
        {
            Debug.LogError($"[{gameObject.name}] walkableAreaTilemap is not assigned!");
            return;
        }

        lastPosition = transform.position;
        pointA = transform.position;
        pointB = chooseRandomPoint();
        target = pointB;
        player = GameObject.FindGameObjectWithTag("Player");
        
        Debug.Log($"[{gameObject.name}] Initialized with walkableAreaTilemap: {walkableAreaTilemap.name}");
        
        // Initialize the move direction
        moveDirection = (target - transform.position).normalized;
    }

    // --- Update Loop ---
    void Update()
    {
        // Check if we're stuck
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        if (distanceMoved < MOVEMENT_THRESHOLD && !isChasing)
        {
            stuckTime += Time.deltaTime;
            if (stuckTime > STUCK_THRESHOLD)
            {
                // Reset path and set current position as target
                Debug.Log($"[{gameObject.name}] Stuck detected! Resetting path.");
                pointA = transform.position;
                pointB = transform.position;
                target = transform.position;
                currentPath = null;
                currentPathIndex = 0;
                stuckTime = 0f;
            }
        }
        else
        {
            stuckTime = 0f;
        }
        lastPosition = transform.position;

        // Prioritize Chase state
        if (player != null && Vector2.Distance(transform.position, player.transform.position) < chaseRange)
        {
            isChasing = true;
            ChasePlayer();
        }
        else
        {
            isChasing = false;
            Patrol();
        }
    }

    // --- Core Methods ---

    void Patrol()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            UpdatePath();
            if (currentPath == null || currentPath.Count == 0)
            {
                // Fallback to direct movement if no path is found
                moveDirection = (target - transform.position).normalized;
            }
        }

        if (currentPath != null && currentPath.Count > 0)
        {
            // Move towards the current path node
            Vector2 currentNode = currentPath[currentPathIndex];
            moveDirection = ((Vector3)currentNode - transform.position).normalized;

            // Check if we reached the current node
            if (Vector2.Distance(transform.position, currentNode) < pathNodeReachedThreshold)
            {
                currentPathIndex++;
                if (currentPathIndex >= currentPath.Count)
                {
                    currentPath = null;
                    currentPathIndex = 0;
                }
            }
        }

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
        if(cycleCount >= 3)
        {
            pointA = transform.position;
            pointB = chooseRandomPoint();
            target = pointB;
            UpdatePath();
            cycleCount = 0;
        }
        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            // Switch the target point
            target = ((Vector2)target == pointA) ? pointB : pointA;

            // Important: Reset the current speed to zero so the next Patrol movement starts with acceleration
            // (You could also implement a brief Idle state here if you wanted a pause!)
            currentSpeed = 0f;
            cycleCount++;
        }
    }

    private List<Rect> GetAllTileWorldRects()
    {
        var rects = new List<Rect>();
        if (walkableAreaTilemap == null) return rects;

        var bounds = walkableAreaTilemap.cellBounds;
        Vector3 cellSize = Vector3.one;
        if (walkableAreaTilemap.layoutGrid != null)
            cellSize = walkableAreaTilemap.layoutGrid.cellSize;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var cellPos = new Vector3Int(x, y, 0);
                if (!walkableAreaTilemap.HasTile(cellPos)) continue;

                Vector3 center = walkableAreaTilemap.GetCellCenterWorld(cellPos);
                float halfX = cellSize.x * 0.5f;
                float halfY = cellSize.y * 0.5f;
                var rect = new Rect(center.x - halfX, center.y - halfY, cellSize.x, cellSize.y);
                rects.Add(rect);
            }
        }

        return rects;
    }
    private void UpdatePath()
    {
        var walkableRects = GetAllTileWorldRects();
        if (walkableRects.Count == 0) return;
        pointA = transform.position;
        pointB = chooseRandomPoint();

        Vector2 start = transform.position;
        Vector2 end = target;

        currentPath = AStar.FindPath(walkableRects, start, end);
        currentPathIndex = 0;
    }

    public Vector2 chooseRandomPoint()
    {
        var tileRects = GetAllTileWorldRects();
        Vector2 currentPos = transform.position;

        if (tileRects.Count == 0)
        {
            // Debug.LogWarning("No walkable tiles found!");
            return currentPos;
        }

        int randomIndex = Random.Range(0, tileRects.Count);
        var rect = tileRects[randomIndex];
        float rx = Random.Range(rect.xMin, rect.xMax);
        float ry = Random.Range(rect.yMin, rect.yMax);
        Vector2 randomPoint = new Vector2(rx, ry);
        
        int attempts = 0;
        while(Vector2.Distance(currentPos, randomPoint) > walkableRange && attempts < 100)
        {
            attempts++;
            if (tileRects.Count <= 1) break;
            
            int ri = Random.Range(0, tileRects.Count);
            var rec = tileRects[ri];
            rx = Random.Range(rec.xMin, rec.xMax);
            ry = Random.Range(rec.yMin, rec.yMax);
            randomPoint = new Vector2(rx, ry);
        }

        return randomPoint;
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