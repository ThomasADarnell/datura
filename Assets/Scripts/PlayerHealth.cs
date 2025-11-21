/*
 *  Author: ariel oliveira [o.arielg@gmail.com]
 */

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public delegate void OnHealthChangedDelegate();
    public OnHealthChangedDelegate onHealthChangedCallback;

    #region Sigleton
    private static PlayerHealth instance;
    public static PlayerHealth Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<PlayerHealth>();
            return instance;
        }
    }
    #endregion

    [SerializeField]
    private float health;
    [SerializeField]
    private float maxHealth;
    [SerializeField]
    private float maxTotalHealth;
    private Color originalColor;

    public float Health { get { return health; } }
    public float MaxHealth { get { return maxHealth; } }
    public float MaxTotalHealth { get { return maxTotalHealth; } }
    public SpriteRenderer spriteRenderer;
    public Color warningColor = Color.red;


    void Start()
    {
        // Get the SpriteRenderer component if not assigned in the Inspector
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Store the character's initial color
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    public void Heal(float health)
    {
        this.health += health;
        ClampHealth();
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerHurt();
        }
        ClampHealth();
        if (health > 0)
        {
            // Start the color flash coroutine instead of using a while loop
            StartCoroutine(DelayColor(.25f)); // Flash for 1 second
        }
    }
    public IEnumerator DelayColor(float time)
    {
        float timer = 0f;
        while (timer < time)
        {
            // Toggle color rapidly
            spriteRenderer.color = (timer % 0.1f < 0.05f) ? originalColor : warningColor;
            timer += Time.deltaTime;
            yield return null; // Wait until the next frame
        }
        // Ensure the color is reset to original after flashing
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    public void AddHealth()
    { 
        if (maxHealth < maxTotalHealth)
        {
            maxHealth += 1;
            health = maxHealth;

            if (onHealthChangedCallback != null)
                onHealthChangedCallback.Invoke();
        }   
    }

    void ClampHealth()
    {
        health = Mathf.Clamp(health, 0, maxHealth);

        if (onHealthChangedCallback != null)
            onHealthChangedCallback.Invoke();
        Debug.Log(health);
        if (health <= 0)
        {
            Die();
        }
    }

    private static void Die()
    {
        AudioManager.Instance.PlayPlayerDeath();
        //Animation
        SceneManager.LoadScene("Kill Screen");
    }
}
