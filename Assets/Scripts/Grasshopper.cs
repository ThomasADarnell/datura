using UnityEngine;
using System.Collections; // Required for Coroutines

/// <summary>
/// Controls a 2D enemy (Grasshopper) in a top-down game.
/// The enemy moves along the X-Y plane but creates a jump-like arc visually
/// by moving a child visual component up and down.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Grasshopper : MonoBehaviour
{
    [Header("Target and Movement")]
    public Transform target;
    public float travelSpeed = 8f;
    public float jumpDuration = 0.5f;
    public float jumpCooldown = 2.5f;

    [Header("Jump Arc Visuals")]
    public Transform visualPivot; // Drag your Sprite/Mesh child object here
    public float maxArcHeight = 1.5f; // This is the peak of the fake jump

    // NOTE: The 'rotateToFaceTarget' flag is now effectively ignored
    // since the rotation logic has been removed/disabled.

    // Internal State
    private Rigidbody2D rb;
    private float lastJumpTime;
    private bool isJumping = false;
    private Vector3 initialVisualLocalPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Grasshopper requires a Rigidbody2D component to function.");
            enabled = false;
        }

        if (visualPivot == null)
        {
            Debug.LogError("Grasshopper requires a visualPivot Transform (usually a child object) to create the jump arc.");
            enabled = false;
        }

        // IMPORTANT for Top-Down: Ensure gravity is ignored
        rb.gravityScale = 0f;

        // Store the starting local position of the visual component (should be near (0, 0, 0))
        initialVisualLocalPosition = visualPivot.localPosition;

        // Initialize the last jump time to allow jumping immediately
        lastJumpTime = Time.time - jumpCooldown;
    }

    void Update()
    {
        // Only attempt a jump if a target exists and we aren't currently jumping
        if (target != null && !isJumping && Time.time > lastJumpTime + jumpCooldown)
        {
            StartCoroutine(PerformJumpArc());
        }

        // We no longer call FaceTarget() here, keeping the sprite upright.
        // If you need the sprite to flip horizontally (left/right) to face the player
        // without rotating, you would add a horizontal flipping function here instead.
    }

    private IEnumerator PerformJumpArc()
    {
        isJumping = true;
        lastJumpTime = Time.time;

        // 1. Calculate direction vector to the target (movement on X-Y plane)
        Vector2 direction = (target.position - transform.position).normalized;

        // 2. We skip calling FaceDirection(direction) here to keep the sprite upright.

        // 3. Start the horizontal travel immediately
        rb.linearVelocity = direction * travelSpeed;

        float startTime = Time.time;
        float progress = 0f;

        while (progress < 1f)
        {
            // Calculate progress through the jump duration (0.0 to 1.0)
            progress = (Time.time - startTime) / jumpDuration;

            // Calculate the arc height using a parabola formula
            float arcInterpolation = Mathf.Sin(progress * Mathf.PI);
            float currentHeight = arcInterpolation * maxArcHeight;

            // Apply the visual height offset.
            visualPivot.localPosition = initialVisualLocalPosition + new Vector3(0, currentHeight, 0);

            yield return null; // Wait for the next frame
        }

        // --- JUMP ARC ENDED ---

        // 4. Stop the horizontal travel
        rb.linearVelocity = Vector2.zero;

        // 5. Ensure the visual pivot is back at its starting position ("on the ground")
        visualPivot.localPosition = initialVisualLocalPosition;

        isJumping = false;
    }

    // --- REMOVED / DISABLED ROTATION METHODS ---

    /// <summary>
    /// Rotates the sprite to face a given direction vector.
    /// This method is now **disabled** to keep the sprite upright.
    /// </summary>
    private void FaceDirection(Vector2 direction)
    {
        // To keep the sprite upright, we skip setting the rotation.
        // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // rb.rotation = angle;
    }

    /// <summary>
    /// Rotates the sprite to face the current target position.
    /// This method is now **removed/disabled** to keep the sprite upright.
    /// </summary>
    private void FaceTarget()
    {
        // Code removed to prevent rotation
    }
}