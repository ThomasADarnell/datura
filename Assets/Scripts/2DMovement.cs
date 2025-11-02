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
        Debug.Log("use the item www");
        // health.TakeDamage(1); debug
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
