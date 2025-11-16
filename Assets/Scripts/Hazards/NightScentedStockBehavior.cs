using System.Collections.Generic;
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

    // Use Start() to destroy the effect after its duration
    void Start()
    {

    }

    // Add objects to the list when they enter the trigger area
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        { // When player runs into the shrub
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();  // get player health
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);  // apply damage
            }
        }
    }

    // Remove objects from the list when they leave the trigger area
    private void OnTriggerExit(Collider other)
    {
        if (objectsInRange.Contains(other))
        {
            objectsInRange.Remove(other);
        }
    }

    // Iterate through all objects currently in range and apply damage
    private void ApplyPoisonDamage()
    {
        // Use a reverse loop or a copy to avoid issues if an object is destroyed while iterating
        for (int i = objectsInRange.Count - 1; i >= 0; i--)
        {
            Collider collider = objectsInRange[i];

            // Get the Health component (make sure this name matches your script!)
            // Example uses 'PlayerHealth' as you used it in your script.
            PlayerHealth healthSystem = collider.GetComponent<PlayerHealth>();

            if (healthSystem != null)
            {
                healthSystem.TakeDamage(damagePerTick);
            }
        }
    }
}