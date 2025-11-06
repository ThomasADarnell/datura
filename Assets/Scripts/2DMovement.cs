using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;
    private Health health;
    private FacingDirection direction;
    private float distanceToAttack = 5.0f; //How close you need to be to do damage

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
        NaturalCombinedEnemyBehavior[] butterflies = FindObjectsByType<NaturalCombinedEnemyBehavior>(FindObjectsSortMode.None);

            float lx = anim.GetFloat("LastX");
            float ly = anim.GetFloat("LastY");
            float facingAngle;
            if (lx > 0.5f) facingAngle = 0f;        // right
            else if (lx < -0.5f) facingAngle = 180f; // left
            else if (ly > 0.5f) facingAngle = 90f;  // up
            else facingAngle = 270f;               // down

            foreach (NaturalCombinedEnemyBehavior butterfly in butterflies)
            {
                float dist = Vector2.Distance(this.transform.position, butterfly.transform.position);
                if (dist > distanceToAttack) continue;

                double projection = CheckProjection(this.transform.position, butterfly.transform.position);
                float angleDiff = Mathf.Abs(Mathf.DeltaAngle((float)facingAngle, (float)projection));
                if (angleDiff <= 45f)
                {
                    if (this.ExplosionEffectPrefab) Instantiate(this.ExplosionEffectPrefab, this.transform.position, Quaternion.identity);
                    Destroy(butterfly.gameObject);
                }
                else
                {
                    Debug.Log("Not facing target; angleDiff=" + angleDiff);
                }
            }
    }

    void FixedUpdate()
    {
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
