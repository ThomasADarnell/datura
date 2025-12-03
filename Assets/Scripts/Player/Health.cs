using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth { get; private set; }

    private HeartDisplay heartDisplay;

    void Start()
    {
        heartDisplay = FindAnyObjectByType<HeartDisplay>();

        if (PlayerData.Instance != null)
        {
            maxHealth = PlayerData.Instance.maxHealth;
            currentHealth = PlayerData.Instance.currentHealth;
        }
        else
        {
            currentHealth = maxHealth;
        }

        heartDisplay.InitHearts(maxHealth);
        heartDisplay.UpdateHearts(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(currentHealth - amount, 0);
        PlayerData.Instance.currentHealth = currentHealth;  // update global
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerHurt();
        }

        heartDisplay.UpdateHearts(currentHealth, maxHealth);
        if (currentHealth <= 0) {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        PlayerData.Instance.currentHealth = currentHealth;  // update global

        heartDisplay.UpdateHearts(currentHealth, maxHealth);
    }

    private static void Die()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerDeath();
        }
        //Animation
        SceneManager.LoadScene("Death");
    }
}
