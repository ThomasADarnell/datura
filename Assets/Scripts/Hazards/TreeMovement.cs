using UnityEngine;

public class TreeMovement : MonoBehaviour
{
    // ... (moveSpeed, leftLimit, rightLimit variables remain the same) ...
    public float moveSpeed = 2.0f;
    public float leftLimit = -5.0f;
    public float rightLimit = 5.0f;

    private int moveDirection = 1;

    // Public property to provide the current position within the step cycle (0 to 1)
    public float StepCycleNormalized { get; private set; }

    void Update()
    {
        // Calculate movement as before
        transform.Translate(Vector3.right * moveDirection * moveSpeed * Time.deltaTime);

        // Update the step cycle timer based on time or distance (using Time.time here for simplicity)
        // This timer loops every 2 * PI seconds if speed is 1.
        // We make it accessible to the child scripts.
        StepCycleNormalized = Mathf.Repeat(Time.time, Mathf.PI * 2);

        // Check for boundaries and reverse direction (as before)
        if (transform.position.x > rightLimit || transform.position.x < leftLimit)
        {
            moveDirection *= -1;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }
}