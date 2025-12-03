using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    public BossController bossController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object entering the trigger is the Player
        if (other.CompareTag("Player"))
        {
            if (bossController != null)
            {
                bossController.StartBossFight();

                // Optional: Disable the trigger after the fight starts
                GetComponent<Collider2D>().enabled = false;
            }
        }
    }
}