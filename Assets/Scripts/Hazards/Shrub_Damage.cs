using UnityEngine;

public class Shrub_Damage : MonoBehaviour {
       public int damageAmount = 1;  // damage is half a player heart

        private void OnTriggerEnter2D(Collider2D other) {

            if (other.CompareTag("Player")) { // When player runs into the shrub
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();  // get player health
                if (playerHealth != null) {
                    playerHealth.TakeDamage(damageAmount);  // apply damage
                }
            }
        }
}
