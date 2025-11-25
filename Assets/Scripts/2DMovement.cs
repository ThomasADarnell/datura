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
    public float attackDuration = 0.25f;  // attack duration
    public float distanceToAttack = 2f; // attack range

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
        // Prevent attacking while dashing
        if (isDashing) return;

        isAttacking = true;
        attackTimer = attackDuration;
        EnemyManager enemyManager = FindObjectsByType<EnemyManager>(FindObjectsSortMode.None)[0];
        List<EnemyBaseBehavior> butterflies = enemyManager.Enemies;
        List<EnemyBaseBehavior> butterfliesToDamage = new List<EnemyBaseBehavior>();

        float lx = anim.GetFloat("LastX");
        float ly = anim.GetFloat("LastY");
        float facingAngle;
        if (lx > 0.5f) facingAngle = 0f;        // right
        else if (lx < -0.5f) facingAngle = 180f; // left
        else if (ly > 0.5f) facingAngle = 90f;   // up
        else facingAngle = 270f;                // down
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerStab();
        }

        InventoryUIManager inventory = FindFirstObjectByType<InventoryUIManager>();
        inventory.SetActiveItem();
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

            LocustBehavior[] trees = FindObjectsByType<LocustBehavior>(FindObjectsSortMode.None);
            foreach (LocustBehavior tree in trees)
            {
                if (tree == null) continue;
                float dist = Vector2.Distance(this.transform.position, tree.transform.position);
                if (dist > distanceToAttack) continue;

                double projection = CheckProjection(this.transform.position, tree.transform.position);
                float angleDiff = Mathf.Abs(Mathf.DeltaAngle(facingAngle, (float)projection));
                if (angleDiff <= 45f)
                {
                    if (this.ExplosionEffectPrefab)
                    {
                        GameObject effect = Instantiate(this.ExplosionEffectPrefab, tree.transform.position, Quaternion.identity);
                        Destroy(effect, effect.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
                    }
                    tree.TakeDamage(1);
                    int health = tree.health;
                    health -= 1;
                    if (health < 0)
                    {
                        Destroy(tree.gameObject);
                    }
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

            // Handle Bomber vases
            Bomber[] bombers = FindObjectsByType<Bomber>(FindObjectsSortMode.None);
            foreach (Bomber bomber in bombers)
            {
                if (bomber == null) continue;
                float dist = Vector2.Distance(this.transform.position, bomber.transform.position);
                if (dist > distanceToAttack) continue;

                double projection = CheckProjection(this.transform.position, bomber.transform.position);
                float angleDiff = Mathf.Abs(Mathf.DeltaAngle(facingAngle, (float)projection));
                if (angleDiff <= 45f)
                {
                    if (this.ExplosionEffectPrefab)
                    {
                        GameObject effect = Instantiate(this.ExplosionEffectPrefab, bomber.transform.position, Quaternion.identity);
                        Destroy(effect, effect.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
                    }
                    bomber.TakeDamage(1);
                    Debug.Log("Hit bomber vase!");
                }
                else
                {
                    Debug.Log("Not facing Bomber; angleDiff=" + angleDiff);
                }
            }

            // Handle Fly enemies
            Fly[] flies = FindObjectsByType<Fly>(FindObjectsSortMode.None);
            foreach (Fly fly in flies)
            {
                if (fly == null) continue;
                float dist = Vector2.Distance(this.transform.position, fly.transform.position);
                if (dist > distanceToAttack) continue;

                double projection = CheckProjection(this.transform.position, fly.transform.position);
                float angleDiff = Mathf.Abs(Mathf.DeltaAngle(facingAngle, (float)projection));
                if (angleDiff <= 45f)
                {
                    if (this.ExplosionEffectPrefab)
                    {
                        GameObject effect = Instantiate(this.ExplosionEffectPrefab, fly.transform.position, Quaternion.identity);
                        Destroy(effect, effect.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
                    }
                    fly.TakeDamage(1);
                }
                else
                {
                    Debug.Log("Not facing Fly; angleDiff=" + angleDiff);
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
                }
                catch { }

            }
        }
        else if (type == "ProtectiveGear")
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
    }

    void FixedUpdate()
    {
        if (attackTimer > 0)
        {
            attackTimer -= Time.fixedDeltaTime;
            if (attackTimer <= 0)
            {
                isAttacking = false;
            }
        }

        anim.SetBool("isAttacking", isAttacking);


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