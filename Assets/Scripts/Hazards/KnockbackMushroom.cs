using UnityEngine;

public class KnockbackMushroom : MonoBehaviour
{
    public float knockbackForce = 500f; // Force applied to the player
    public float knockbackDuration = 0.2f; // How long the knockback lasts

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Something");
        // Check if the object that entered the trigger is the player
        if (other.CompareTag("Player"))
        {
            // Get the Rigidbody2D of the player
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();

            if (playerRb != null)
            {
                // Calculate the direction from the mushroom to the player
                Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;

                // Apply the knockback force.
                // It's often better to call a method on the player's script 
                // to handle the actual application of force, especially if
                // you want to disable player movement temporarily.
                
                // Example of applying force directly:
                // playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

                // A better approach is to call a function on the player's script:
                PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
                if (playerMovement != null)
                {
                    playerMovement.ApplyKnockback(knockbackDirection, knockbackForce, knockbackDuration);
                }
            }
        }
    }
}