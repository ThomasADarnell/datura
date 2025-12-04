using UnityEngine;
using TMPro;
using System.Collections;

public class EvilManager : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float zoomOutAmount = 5f;
    [SerializeField] private float zoomDuration = 3f;

    [Header("Text Settings")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private string firstMessage = "You're in a dead level.";
    [SerializeField] private string secondMessage = "You cannot escape.";
    [SerializeField] private float firstMessageDuration = 3f;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float delayBetweenMessages = 1f;

    [Header("Glitch Settings")]
    [SerializeField] private float glitchIntensity = 0.1f;
    [SerializeField] private float glitchFrequency = 0.2f;
    [SerializeField] private Color glitchColor1 = Color.red;
    [SerializeField] private Color glitchColor2 = Color.cyan;

    [Header("Lighting Settings")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private float targetLightIntensity = 0.1f;
    [SerializeField] private float lightFadeDuration = 4f;

    private float startOrthographicSize;
    private Vector3 startCameraPosition;
    private Color originalTextColor;
    private bool isGlitching = false;
    private float startLightIntensity;

    void Start()
    {
        if (targetCamera == null)
        {
            Debug.LogError("EvilManager: No camera assigned!");
            return;
        }

        if (messageText == null)
        {
            Debug.LogError("EvilManager: No TextMeshProUGUI assigned!");
            return;
        }

        // Store initial values
        if (targetCamera.orthographic)
        {
            startOrthographicSize = targetCamera.orthographicSize;
        }
        startCameraPosition = targetCamera.transform.position;
        originalTextColor = messageText.color;

        if (directionalLight != null)
        {
            startLightIntensity = directionalLight.intensity;
        }

        // Start with invisible text
        messageText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, 0f);
        messageText.text = firstMessage;

        // Start the sequence
        StartCoroutine(EvilSequence());
    }

    void Update()
    {
        if (isGlitching)
        {
            ApplyGlitchEffects();
        }
    }

    IEnumerator EvilSequence()
    {
        // Wait a moment before starting
        yield return new WaitForSeconds(0.5f);

        // Start camera zoom and text fade simultaneously
        StartCoroutine(ZoomOutCamera());
        StartCoroutine(FadeInText());
        
        // Fade the directional light to darkness
        if (directionalLight != null)
        {
            StartCoroutine(FadeLightToDark());
        }

        // Enable glitching
        isGlitching = true;

        // Wait for first message duration
        yield return new WaitForSeconds(firstMessageDuration);

        // Fade out first message
        yield return StartCoroutine(FadeOutText());

        // Wait between messages
        yield return new WaitForSeconds(delayBetweenMessages);

        // Change to second message
        messageText.text = secondMessage;

        // Fade in second message
        yield return StartCoroutine(FadeInText());

        // Keep second message visible and continue glitching
    }

    IEnumerator ZoomOutCamera()
    {
        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;

            if (targetCamera.orthographic)
            {
                targetCamera.orthographicSize = Mathf.Lerp(startOrthographicSize, startOrthographicSize + zoomOutAmount, t);
            }
            else
            {
                // For perspective camera, move it back
                targetCamera.transform.position = Vector3.Lerp(startCameraPosition, startCameraPosition + Vector3.up * zoomOutAmount, t);
            }

            yield return null;
        }
    }

    IEnumerator FadeInText()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            messageText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, alpha);
            yield return null;
        }

        messageText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, 1f);
    }

    IEnumerator FadeOutText()
    {
        float elapsed = 0f;
        Color currentColor = messageText.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            messageText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, alpha);
            yield return null;
        }

        messageText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, 0f);
    }

    IEnumerator FadeLightToDark()
    {
        float elapsed = 0f;

        while (elapsed < lightFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lightFadeDuration;
            directionalLight.intensity = Mathf.Lerp(startLightIntensity, targetLightIntensity, t);
            yield return null;
        }

        directionalLight.intensity = targetLightIntensity;
    }

    void ApplyGlitchEffects()
    {
        // Random glitch on text
        if (Random.value < glitchFrequency * Time.deltaTime)
        {
            // Offset text position
            Vector3 glitchOffset = new Vector3(
                Random.Range(-glitchIntensity, glitchIntensity) * 100f,
                Random.Range(-glitchIntensity, glitchIntensity) * 100f,
                0f
            );
            messageText.transform.localPosition = glitchOffset;

            // Random color shift
            if (Random.value > 0.5f)
            {
                Color glitchColor = Random.value > 0.5f ? glitchColor1 : glitchColor2;
                messageText.color = new Color(glitchColor.r, glitchColor.g, glitchColor.b, messageText.color.a);
            }
        }
        else
        {
            // Reset position and color
            messageText.transform.localPosition = Vector3.zero;
            float currentAlpha = messageText.color.a;
            messageText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, currentAlpha);
        }

        // Camera glitch
        if (Random.value < glitchFrequency * Time.deltaTime * 0.5f)
        {
            Vector3 cameraGlitch = new Vector3(
                Random.Range(-glitchIntensity, glitchIntensity) * 0.1f,
                Random.Range(-glitchIntensity, glitchIntensity) * 0.1f,
                0f
            );
            targetCamera.transform.position = startCameraPosition + cameraGlitch;
        }
        else
        {
            targetCamera.transform.position = new Vector3(
                startCameraPosition.x,
                startCameraPosition.y,
                targetCamera.transform.position.z
            );
        }
    }
}
