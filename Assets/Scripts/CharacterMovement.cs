using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [Tooltip("The speed at which the character moves.")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;

    void Start()
    {
        // 1. Get the Rigidbody2D component.
        rb = GetComponent<Rigidbody2D>();

        // Safety check to remind the user to add the component
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component missing from the character. Please add one for movement to work!");
        }
    }

    void Update()
    {
        // 2. Input Gathering (Horizontal is X, Vertical is Y)
        // GetAxisRaw returns -1, 0, or 1 immediately, which feels snappy for platformers/top-down games.
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space pressed");
        }

        // 3. Calculate Movement Direction
        // Create a vector based on input. We normalize it so diagonal movement isn't faster.
        Vector2 movement = new Vector2(moveX, moveY).normalized;

        // 4. Apply Movement via Rigidbody2D Velocity
        // Setting the velocity directly is the simplest way to move a physics object.
        rb.linearVelocity = movement * moveSpeed;

        /*
        -- Alternative: Transform Movement (Non-Physics) --
        If you didn't want to use a Rigidbody2D (and are not using physics):
        transform.position += new Vector3(moveX, moveY, 0) * moveSpeed * Time.deltaTime;
        */
    }
}
