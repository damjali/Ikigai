using UnityEngine;
using UnityEngine.InputSystem; // Added this

public class Player2Movement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb; // Corrected spelling and name
    private Vector2 moveInput;

    void Start()
    {
        // Ensure the variable name matches the declaration
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Movement is updated here based on the most recent input
        rb.linearVelocity = moveInput * moveSpeed;
    }

    // This must match the signature required by the Player Input component
    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}