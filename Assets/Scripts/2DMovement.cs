using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using InventorySystem;
// using static UnityEditor.Progress;
using Unity.VisualScripting;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    // --- Existing Public Variables ---
    public float moveSpeed = 5f;
    private float attackTimer = 0f;
    public float attackDuration = 0.25f;
    public float distanceToAttack = 2f; //How close you need to be to do damage

    // --- NEW Dash Variables ---
    [Header("Dash Settings")]
    public float dashSpeed = 15f; // How fast the player dashes
    public float dashingTime = 0.15f; // How long the dash lasts
    public float dashCooldown = 1.0f; // Time between dashes (in seconds)

    // --- Private Variables ---
    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private bool canMove = true; // Used for general movement restrictions (knockback, dashing)
    private bool isDashing = false; // NEW: Flag for the active dash duration
    private bool canDash = true; // NEW: Flag for the dash cooldown

    private Vector2 lastMoveDir = Vector2.down;
    private bool isAttacking = false;
    private Health health;

    // ... (Your other existing variables and enums remain here)

    private enum FacingDirection
    {
        Right,
        Left,
        Up,
        Down
    }

    public GameObject ExplosionEffectPrefab;

    private static double CheckProjection(Vector2 p, Vector2 e)
    {
        double angle = Math.Atan2(e.y - p.y, e.x - p.x) * 180.0 / Math.PI;
        if (angle < 0) angle += 360.0;
        return angle;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        health = FindAnyObjectByType<Health>();
    }

    // Existing OnMove input action
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // --- NEW: Input Action for Dashing ---
    // You will need to bind this to a key (e.g., Left Shift or Space) in your Input Actions Asset.
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && canDash && !isDashing && canMove)
        {
            // Only dash if the button is pressed, not during cooldown, 
            // not currently dashing, and not restricted by knockback.
            StartCoroutine(DashRoutine());
        }
    }

    // Existing OnUse input action
    public void OnUse(InputAction.CallbackContext context)
    {
        // Prevent attacking while dashing
        if (isDashing) return;

        isAttacking = true;
        attackTimer = attackDuration;
        EnemyManager enemyManager = FindObjectsByType<EnemyManager>(FindObjectsSortMode.None)[0];
        List<EnemyBaseBehavior> butterflies = enemyManager.Enemies;
        List<EnemyBaseBehavior> butterfliesToDamage = new List<EnemyBaseBehavior>();

        // ... (Your existing OnUse logic remains here)

        float lx = anim.GetFloat("LastX");
        float ly = anim.GetFloat("LastY");
        float facingAngle;
        if (lx > 0.5f) facingAngle = 0f;        // right
        else if (lx < -0.5f) facingAngle = 180f; // left
        else if (ly > 0.5f) facingAngle = 90f;   // up
        else facingAngle = 270f;                // down

        AudioManager.Instance.PlayPlayerStab();

        InventoryUIManager inventory = FindFirstObjectByType<InventoryUIManager>();
        InventoryItem item = inventory.GetActiveItem();
        String type = item.GetItemType();


        if (type.IsUnityNull())
        {
            foreach (EnemyBaseBehavior butterfly in butterflies)
            {

                if (butterfly == null) continue;
                float dist = Vector2.Distance(this.transform.position, butterfly.transform.position);
                if (dist > distanceToAttack) continue;

                double projection = CheckProjection(this.transform.position, butterfly.transform.position);
                float angleDiff = Mathf.Abs(Mathf.DeltaAngle(facingAngle, (float)projection));
                if (angleDiff <= 45f)
                {
                    if (this.ExplosionEffectPrefab)
                    {
                        GameObject effect = Instantiate(this.ExplosionEffectPrefab, butterfly.transform.position, Quaternion.identity);
                        Destroy(effect, effect.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
                    }
                    butterfliesToDamage.Add(butterfly);
                }
                else
                {
                    Debug.Log("Not facing target; angleDiff=" + angleDiff);
                }
            }

            // Process damage after the loop
            foreach (EnemyBaseBehavior butterfly in butterfliesToDamage)
            {
                enemyManager.enemyDamaged(butterfly);
            }

            // Handle Wheeler enemies
            Wheeler[] wheelers = FindObjectsByType<Wheeler>(FindObjectsSortMode.None);
            foreach (Wheeler wheeler in wheelers)
            {
                if (wheeler == null) continue;
                float dist = Vector2.Distance(this.transform.position, wheeler.transform.position);
                if (dist > distanceToAttack) continue;

                double projection = CheckProjection(this.transform.position, wheeler.transform.position);
                float angleDiff = Mathf.Abs(Mathf.DeltaAngle(facingAngle, (float)projection));
                if (angleDiff <= 45f)
                {
                    if (this.ExplosionEffectPrefab)
                    {
                        GameObject effect = Instantiate(this.ExplosionEffectPrefab, wheeler.transform.position, Quaternion.identity);
                        Destroy(effect, effect.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
                    }
                    wheeler.TakeDamage(1);
                }
                else
                {
                    Debug.Log("Not facing Wheeler; angleDiff=" + angleDiff);
                }
            }

            FlowerBoss[] flowers = FindObjectsByType<FlowerBoss>(FindObjectsSortMode.None);

            if (lx > 0.5f) facingAngle = 0f;        // right
            else if (lx < -0.5f) facingAngle = 180f; // left
            else if (ly > 0.5f) facingAngle = 90f;  // up
            else facingAngle = 270f;               // down

            foreach (FlowerBoss flower in flowers)
            {
                float dist = Vector2.Distance(this.transform.position, flower.transform.position);
                if (dist > distanceToAttack) continue;

                double projection = CheckProjection(this.transform.position, flower.transform.position);
                float angleDiff = Mathf.Abs(Mathf.DeltaAngle(facingAngle, (float)projection));
                if (angleDiff <= 45f)
                {
                    if (this.ExplosionEffectPrefab)
                    {
                        GameObject effect = Instantiate(this.ExplosionEffectPrefab, flower.transform.position, Quaternion.identity);
                        Destroy(effect, effect.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
                    }
                    flower.TakeDamage(1);
                }
                else
                {
                    Debug.Log("Not facing target; angleDiff=" + angleDiff);
                }
            }
        }
        else if (type == "Apple")
        {
            PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
            InventoryController control = FindAnyObjectByType<InventoryController>();
            if (playerHealth != null)
            {
                playerHealth.Heal(3);
                try
                {
                    control.RemoveItem("Hotbar", item);
                    control.AddItemPos("Hotbar", new InventoryItem(new ItemInitializer(true)), inventory.GetSlotPosition());
                    item = null;
                }
                catch { }

            }
        }
    }

    void FixedUpdate()
    {
        // Existing attack timer logic
        if (attackTimer > 0)
        {
            attackTimer -= Time.fixedDeltaTime;
            if (attackTimer <= 0)
            {
                isAttacking = false;
            }
        }

        anim.SetBool("isAttacking", isAttacking);

        // Only move if not restricted AND not currently dashing
        if (canMove && !isDashing)
        {
            rb.linearVelocity = moveInput * moveSpeed;

            // Existing animation logic
            bool isMoving = moveInput.sqrMagnitude > 0.01f;

            if (isMoving)
            {
                lastMoveDir = moveInput.normalized;
                anim.SetFloat("MoveX", moveInput.x);
                anim.SetFloat("MoveY", moveInput.y);
            }
            else
            {
                anim.SetFloat("MoveX", lastMoveDir.x);
                anim.SetFloat("MoveY", lastMoveDir.y);
            }

            anim.SetBool("isMoving", isMoving);
            anim.SetFloat("LastX", lastMoveDir.x);
            anim.SetFloat("LastY", lastMoveDir.y);
        }
        else if (isDashing)
        {
            // IMPORTANT: If dashing, we don't want to overwrite the dash velocity in FixedUpdate.
            // We let the velocity applied in the Coroutine take precedence.
            // If you use a very short dash time, you might want to consider setting a constant velocity here
            // based on the direction stored at the start of the dash. For now, we just skip regular movement.
        }
    }

    // --- NEW: Dash Coroutine ---
    private IEnumerator DashRoutine()
    {
        // 1. Start Dash & Setup
        canDash = false; // Start cooldown
        isDashing = true; // Block regular movement

        // Determine dash direction
        // Use current moveInput if moving, otherwise use the last facing direction
        Vector2 dashDirection = (moveInput.sqrMagnitude > 0.01f) ? moveInput.normalized : lastMoveDir;

        // Stop current velocity (optional, but makes the dash feel snappier)
        rb.linearVelocity = Vector2.zero;

        // 2. Apply Dash Velocity
        rb.linearVelocity = dashDirection * dashSpeed;

        // 3. Wait for Dash Duration
        yield return new WaitForSeconds(dashingTime);

        // 4. End Dash
        isDashing = false;

        // Stop the fast dash velocity (or transition smoothly back to walk speed)
        // Set velocity to the normal speed in the current direction, or zero if not holding an input
        if (canMove) // Only adjust velocity if not under knockback restriction
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
        else
        {
            // If we are restricted (e.g., knocked back), just stop the dash velocity
            rb.linearVelocity = Vector2.zero;
        }


        // 5. Start Cooldown
        yield return new WaitForSeconds(dashCooldown);

        // 6. End Cooldown
        canDash = true;
    }

    // Existing Knockback logic
    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        // Stop the dash routine if knockback is applied mid-dash
        if (isDashing)
        {
            StopCoroutine(nameof(DashRoutine));
            isDashing = false; // Immediately end the dash
        }

        // Stop any existing knockback coroutine before starting a new one
        StopCoroutine(KnockbackRoutine(direction, force, duration));
        StartCoroutine(KnockbackRoutine(direction, force, duration));
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float force, float duration)
    {
        canMove = false;

        rb.AddForce(direction * force, ForceMode2D.Impulse); // Use Impulse for instant push

        yield return new WaitForSeconds(duration);

        canMove = true;
    }
}