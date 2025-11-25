using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal; // Required for Light2D

public class FlashTrigger2D : MonoBehaviour
{
    public Light2D targetLight;
    public float maxFlashIntensity = 50f;

    // Duration for the light to fade from max intensity down to original intensity
    public float fadeOutDuration = 1.5f;

    private float originalIntensity;
    private bool hasTriggered = false;
    private bool canTrigger = false;

    void Start()
    {
        if (targetLight == null)
        {
            targetLight = FindFirstObjectByType<Light2D>();
        }

        if (targetLight != null)
        {
            originalIntensity = targetLight.intensity;
        }
        else
        {
            Debug.LogError("No Light2D component assigned or found in the scene!");
            // Disable the script if no light is found to prevent errors
            this.enabled = false;
        }
        StartCoroutine(Wait());
    }
    public IEnumerator Wait()
    {
        yield return new WaitForSeconds(1);
        canTrigger = true;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        // Only trigger if it hasn't been triggered yet
        if (canTrigger && !hasTriggered && targetLight != null)
        {
            // Optional: Check for a specific tag like "Player"
            // if (collision.CompareTag("Player"))
            // {
            hasTriggered = true;
            StartCoroutine(FlashEffectRoutine());
            // }
        }
    }

    private IEnumerator FlashEffectRoutine()
    {
        // 1. Immediately "flash" to the max intensity
        targetLight.intensity = maxFlashIntensity;
        // Optionally set color to pure white instantly for the full shock effect
        targetLight.color = Color.white;

        float timer = 0f;

        // 2. Smoothly fade the intensity back down to the original value over time
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            // Use Linear Interpolation to calculate the new intensity
            // Start at maxIntensity (0 on timer), end at originalIntensity (fadeOutDuration on timer)
            targetLight.intensity = Mathf.Lerp(maxFlashIntensity, 0, timer / fadeOutDuration);

            // Wait until the next frame before looping the while loop
            yield return null;
        }

        // Ensure the intensity is exactly the original value at the end of the loop
        targetLight.intensity = 0;
        
        yield return new WaitForSeconds(5);

        timer = 0f;

        // 2. Smoothly fade the intensity back down to the original value over time
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            // Use Linear Interpolation to calculate the new intensity
            // Start at maxIntensity (0 on timer), end at originalIntensity (fadeOutDuration on timer)
            targetLight.intensity = Mathf.Lerp(0, originalIntensity, timer / fadeOutDuration);

            // Wait until the next frame before looping the while loop
            yield return null;
        }

        // Ensure the intensity is exactly the original value at the end of the loop
        targetLight.intensity = originalIntensity;

        // Optional: If you want the trigger to be reusable, uncomment the line below
        hasTriggered = false;
    }
}