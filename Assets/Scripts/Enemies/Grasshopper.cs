using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Grasshopper : EnemyBaseBehavior
{
    [Header("Grasshopper Stats")]
    public int currentHealth = 2;
    public int maxHealth = 2;

    [Header("Target and Movement")]
    public Transform target;  // player
    public float travelSpeed = 8f;  // jump speed
    public float jumpDuration = 0.5f;  // jump duration
    public float jumpCooldown = 2.5f;  // time between jumps

    [Header("Jump Arc Visuals")]
    public Transform visualPivot; // Enemy sprite
    public float maxArcHeight = 1.5f; // jump peak

    private Rigidbody2D rb;
    private float lastJumpTime;
    private bool isJumping = false;
    private Vector3 initialVisualLocalPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Grasshopper requires a Rigidbody2D component to function.");
            enabled = false;
        }

        if (visualPivot == null)
        {
            Debug.LogError("Grasshopper requires a visualPivot Transform to create the jump arc.");
            enabled = false;
        }

        rb.gravityScale = 0f;
        initialVisualLocalPosition = visualPivot.localPosition;
        lastJumpTime = Time.time - jumpCooldown;
    }

    void Update()
    {
        if (target != null && !isJumping && Time.time > lastJumpTime + jumpCooldown)
        {
            StartCoroutine(PerformJumpArc());
        }
    }

    private IEnumerator PerformJumpArc()
    {
        isJumping = true;
        lastJumpTime = Time.time;

        // See distance from grasshopper to player
        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * travelSpeed;

        float startTime = Time.time;
        float progress = 0f;

        while (progress < 1f)
        {
            // Used to keep sprite jumping during duration
            progress = (Time.time - startTime) / jumpDuration;

            // Calculate current sprite point in jump
            float arcInterpolation = Mathf.Sin(progress * Mathf.PI);
            float currentHeight = arcInterpolation * maxArcHeight;
            visualPivot.localPosition = initialVisualLocalPosition + new Vector3(0, currentHeight, 0);

            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        visualPivot.localPosition = initialVisualLocalPosition;

        isJumping = false;
    }

    private void DealDamage()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.LogError("Enemy hit");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyHurt();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }


    void Die()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyDeath();
        }

        // Destroy after animation completes
        Destroy(gameObject, 1f);
    }

}