using UnityEngine;

public class PartBobbing : MonoBehaviour
{
    public float amplitude = 0.05f;

    [Tooltip("How much faster this part bobs relative to the base cycle (e.g., 1 for legs, 2 for top).")]
    public float speedMultiplier = 1.0f;

    [Tooltip("Enter 180 for inverse movement relative to others.")]
    public float phaseOffsetDegrees = 0f;

    private Vector3 startPos;
    private float phaseOffsetRadians;
    private TreeMovement parentMovementScript;

    void Start()
    {
        startPos = transform.localPosition;
        phaseOffsetRadians = phaseOffsetDegrees * Mathf.Deg2Rad;

        // Get the parent script reference
        parentMovementScript = GetComponentInParent<TreeMovement>();
        if (parentMovementScript == null)
        {
            Debug.LogError("PartBobbing needs to be a child of an object with a TreeMovement script!");
            this.enabled = false; // Disable script if setup is wrong
        }
    }

    void Update()
    {
        // Use the parent's shared timer, multiply by speedMultiplier
        float syncedTime = parentMovementScript.StepCycleNormalized * speedMultiplier;

        // Calculate the Y position using the synced time and phase offset
        float yOffset = Mathf.Sin(syncedTime + phaseOffsetRadians) * amplitude;

        transform.localPosition = startPos + new Vector3(0, yOffset, 0);
    }
}