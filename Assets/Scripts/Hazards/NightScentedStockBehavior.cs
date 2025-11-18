using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PoisonFumeEffect : MonoBehaviour
{
    // Configure these public variables in the Unity Inspector
    public float damagePerTick = 5f;
    public float tickRate = 1f; // How often damage is applied (seconds)
    public float effectDuration = 5f; // How long the fume lasts

    // Private variables for tracking
    private List<Collider> objectsInRange = new List<Collider>();
    private float timeSinceLastTick;

    private PlayerHealth playerHealth; // Reference to the player's health component
    private PlayerMovement playerMovement; // Reference to the player's movement component
    private bool isInRange = false;
    private const float poisonRadius = 3f; // Define the radius clearly

    // Use Start() to destroy the effect after its duration
    void Start()
    {

    }

    // Add objects to the list when they enter the trigger area
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth = other.GetComponent<PlayerHealth>();
            playerMovement = other.GetComponent<PlayerMovement>();

            if (playerHealth != null && playerMovement != null)
            {
                // Apply initial damage
                playerHealth.TakeDamage(1);
                isInRange = true;
                StartCoroutine(ApplyPoisonDamage());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
            // Reset player speed when they leave the area
            if (playerMovement != null)
            {
                playerMovement.moveSpeed = 5f;
            }
            // Stop the coroutine when the player leaves
            StopCoroutine(ApplyPoisonDamage());
        }
    }

    // Iterate through all objects currently in range and apply damage
    private IEnumerator ApplyPoisonDamage()
    {
        // Set slow speed while poisoned
        playerMovement.moveSpeed = 2f;

        while (isInRange) // Continue as long as the player is in the trigger zone
        {
            // Re-calculate distance every loop iteration to ensure they are within the *poison* radius (if different from the trigger collider)
            float distance = Vector2.Distance(transform.position, playerHealth.transform.position);

            if (distance <= poisonRadius)
            {
                playerHealth.TakeDamage(1); // Apply damage periodically
            }

            // Wait for a period of time before the next iteration
            yield return new WaitForSeconds(2.0f); // Damage every 1 second
        }
        
        // Code here runs after the while loop (when isInRange is false)
        // Note: speed reset is handled in OnTriggerExit2D in this implementation
    }
}