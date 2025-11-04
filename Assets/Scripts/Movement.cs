using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    volatile int state;
    private Vector2 movementInput;
    public float cooldownTime = 1.0f;
    private float nextActionTime = 0.0f;

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnUse(InputAction.CallbackContext context)
    {
        Debug.Log("use the item www");
        if (Time.time > nextActionTime)
        {
            // Perform the action
            Debug.Log("Action performed!");
            nextActionTime = Time.time + cooldownTime; // Set the next available time for the action
        }
    }

    public void OnFlashlight(InputAction.CallbackContext context)
    {
        Debug.Log("flashlight toggled");
    }

    public void OnCycleLeft(InputAction.CallbackContext context)
    {
        Debug.Log("switch left");
    }

    public void OnCycleRight(InputAction.CallbackContext context)
    {
        Debug.Log("switch right");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        state = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentPosition = transform.position;

        Vector3 newPosition = currentPosition + new Vector3(
            movementInput.x * 1 * Time.deltaTime,
            0,
            movementInput.y * 1 * Time.deltaTime
        );

        // Apply the new position
        transform.position = newPosition;
    }
}
