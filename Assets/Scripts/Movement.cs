using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    private Vector2 movementInput;

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnUse(InputAction.CallbackContext context)
    {
        Debug.Log("use the item www");
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
