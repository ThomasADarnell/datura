using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using InventorySystem;
// using static UnityEditor.Progress;  // can only be used in editor
using Unity.VisualScripting;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    // --- Public Variables ---
    public float moveSpeed = 5f;  // default player speed
    private float attackTimer = 0f;  // attack cooldown
    public float attackDuration = .75f;  // attack cooldown duration (time between attacks)
    public float distanceToAttack = 10f; // attack range

    [Header("Dash Settings")]
    public float dashSpeed = 15f; // dash speed
    public float dashingTime = 0.15f; // dash duration
    public float dashCooldown = 1.0f; // time between dashes

    // --- Private Variables ---
    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    public bool canMove = true; // used to see if player can move or is interacting with hazard
    private bool isDashing = false; // used to see if player is dashing
    public bool canDash = true; // dash cooldown

    private Vector2 lastMoveDir = Vector2.down;
    private bool isAttacking = false;
    private bool canAttack = true;
    private Health health;

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
    void Update()
    {
        // Decrement the attack timer every frame
        if (attackTimer > 0)
        {
            // Use Time.deltaTime here, not FixedDeltaTime
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                // Ensure isAttacking is false when the timer finishes
                StartCoroutine(Cooldown());
                //isAttacking = false;
            }
        }

        // Update the Animator parameter in Update as well for responsiveness
        //anim.SetBool("isAttacking", isAttacking);
    }

    public IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(1);
        canAttack = true;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && canDash && !isDashing && canMove)
        {
            // Only dash after player hits button & after cooldown ends
            StartCoroutine(DashRoutine());
        }
    }

    // Existing OnUse input action
    public void OnUse(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        // Optional: Add this check as a common fix for a Unity Input System bug
        if (!gameObject.scene.IsValid()) return;
        
        // Prevent attacking while dashing or on cooldown
        if (isDashing || attackTimer > 0 || !canAttack) return;

        //isAttacking = true;
        canAttack = false;
        attackTimer = attackDuration;

        anim.SetTrigger("AttackTrigger");
        Debug.Log("Attack Trigger fired!");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerStab();
        }

        InventoryUIManager inventory = FindFirstObjectByType<InventoryUIManager>();
        inventory.SetActiveItem();
        InventoryItem item = inventory.GetActiveItem();
        string type = item.GetItemType();

        if (type.IsUnityNull())
        {
            PerformAttack();
        }
        else if (type == "Apple")
        {
            UseApple(item);
        }
        else if (type == "ProtectiveGear")
        {
            UseProtectiveGear(item);
        }
    }

    private void PerformAttack()
    {
        float facingAngle = GetFacingAngle();
        const float attackAngle = 60f; // Changed from 45f to 60f for wider detection

        // Handle butterflies spawned by EnemyManager
        EnemyManager[] enemyManagers = FindObjectsByType<EnemyManager>(FindObjectsSortMode.None);
        if (enemyManagers != null && enemyManagers.Length > 0)
        {
            EnemyManager enemyManager = enemyManagers[0];
            List<EnemyBaseBehavior> butterflies = enemyManager.Enemies;
            
            if (butterflies != null && butterflies.Count > 0)
            {
                List<EnemyBaseBehavior> butterfliesToDamage = new List<EnemyBaseBehavior>();

                foreach (EnemyBaseBehavior butterfly in butterflies)
                {
                    if (TryAttackTarget(butterfly?.transform, facingAngle, attackAngle))
                    {
                        butterfliesToDamage.Add(butterfly);
                    }
                }

                foreach (EnemyBaseBehavior butterfly in butterfliesToDamage)
                {
                    enemyManager.enemyDamaged(butterfly);
                }
            }
        }

        // Handle all manually-placed enemy types
        AttackEnemyType<Wheeler>(facingAngle, attackAngle, (wheeler) => wheeler.TakeDamage(1));
        AttackEnemyType<LocustBehavior>(facingAngle, attackAngle, (tree) => tree.TakeDamage(1));
        AttackEnemyType<FlowerBoss>(facingAngle, attackAngle, (flower) => flower.TakeDamage(1));
        AttackEnemyType<Bomber>(facingAngle, attackAngle, (bomber) => bomber.TakeDamage(1));
        AttackEnemyType<Fly>(facingAngle, attackAngle, (fly) => fly.TakeDamage(1));
        AttackEnemyType<Sword>(facingAngle, attackAngle, (sword) => sword.TakeDamage(1));
        AttackEnemyType<Grasshopper>(facingAngle, attackAngle, (grasshopper) => grasshopper.TakeDamage(1));

        AttackEnemyType<AngelTrumpet>(facingAngle, attackAngle, (monster) => monster.TakeDamage(1)); 

    }

    private float GetFacingAngle()
    {
        float lx = anim.GetFloat("LastX");
        float ly = anim.GetFloat("LastY");

        if (lx > 0.5f) return 0f;        // right
        if (lx < -0.5f) return 180f;     // left
        if (ly > 0.5f) return 90f;       // up
        return 270f;                     // down
    }

    private bool TryAttackTarget(Transform target, float facingAngle, float attackAngle)
    {
        if (target == null) return false;

        float dist = Vector2.Distance(this.transform.position, target.position);
        if (dist > distanceToAttack) return false;

        double projection = CheckProjection(this.transform.position, target.position);
        float angleDiff = Mathf.Abs(Mathf.DeltaAngle(facingAngle, (float)projection));

        if (angleDiff <= attackAngle)
        {
            SpawnExplosionEffect(target.position);
            return true;
        }

        return false;
    }

    private void AttackEnemyType<T>(float facingAngle, float attackAngle, System.Action<T> damageAction) where T : MonoBehaviour
    {
        T[] enemies = FindObjectsByType<T>(FindObjectsSortMode.None);
        foreach (T enemy in enemies)
        {
            if (TryAttackTarget(enemy?.transform, facingAngle, attackAngle))
            {
                damageAction(enemy);
            }
        }
    }

    private void SpawnExplosionEffect(Vector3 position)
    {
        if (this.ExplosionEffectPrefab)
        {
            GameObject effect = Instantiate(this.ExplosionEffectPrefab, position, Quaternion.identity);
            Destroy(effect, effect.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
        }
    }

    private void UseApple(InventoryItem item)
    {
        PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
        InventoryController control = FindAnyObjectByType<InventoryController>();
        if (playerHealth != null)
        {
            playerHealth.Heal(3);
            try
            {
                control.RemoveItem("Hotbar", item);
            }
            catch { }
        }
    }

    private void UseProtectiveGear(InventoryItem item)
    {
        PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
        InventoryController control = FindAnyObjectByType<InventoryController>();
        if (playerHealth != null)
        {
            playerHealth.awesomeSauce(15f);
            try
            {
                control.RemoveItem("Hotbar", item);
            }
            catch { }
        }
    }

    void FixedUpdate()
    {
        /*if (attackTimer > 0)
        {
            attackTimer -= Time.fixedDeltaTime;
            if (attackTimer <= 0)
            {
                isAttacking = false;
            }
        }

        anim.SetBool("isAttacking", isAttacking);*/


        if (!canMove && !canDash && !isDashing)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isMoving", false);
            // Additional logic to freeze animations if necessary could go here
        }
        // Only move if not restricted AND not currently dashing
        else if (canMove && !isDashing)
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
        }
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;

        // Dash facing last known player direction
        Vector2 dashDirection = (moveInput.sqrMagnitude > 0.01f) ? moveInput.normalized : lastMoveDir;

        rb.linearVelocity = dashDirection * dashSpeed;

        // Dash for only a set time
        yield return new WaitForSeconds(dashingTime);
        isDashing = false;

        // Transition from dash speed to normal speed
        if (canMove)  // can walk
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }

        // Dash cooldown to prevent spamming
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }


    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        // Stop any existing knockback coroutine before starting a new one
        StopCoroutine(KnockbackRoutine(direction, force, duration));
        StartCoroutine(KnockbackRoutine(direction, force, duration));
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float force, float duration)
    {
        canMove = false;

        if (isDashing)
        {
            force = dashSpeed * force;
        }

        rb.AddForce(direction * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(duration);

        canMove = true;
    }
}