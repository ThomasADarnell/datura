using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using InventorySystem;
using static UnityEditor.Progress;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private float attackTimer = 0f;
    public float attackDuration = 0.25f;
    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;
    private bool isAttacking = false;
    private Health health;
    public float distanceToAttack = 1f; //How close you need to be to do damage

    private enum FacingDirection
    {
        Right,
        Left,
        Up,
        Down
    }

    public GameObject ExplosionEffectPrefab;

    private static double CheckProjection(Vector2 p, Vector2 e) {
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
        health = FindAnyObjectByType<Health>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnUse(InputAction.CallbackContext context)
    {
        InventoryUIManager inventory = FindFirstObjectByType<InventoryUIManager>();
        InventoryItem item = inventory.GetActiveItem();
        String type = item.GetItemType();
        Debug.Log("k"+type+"k");
        if (type.IsUnityNull())
        {

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
            else facingAngle = 270f;                 // down

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
        else if(type == "Apple")
        {
            PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
            InventoryController control = FindAnyObjectByType<InventoryController>();
            if (playerHealth != null)
            {
                playerHealth.Heal(3);
                try { control.RemoveItem("Hotbar", item); } catch { }
                    
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

        rb.linearVelocity = moveInput * moveSpeed;

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

}
