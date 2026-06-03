using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(InputHandler))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 10f;

    private Rigidbody2D rb;
    private InputHandler inputHandler;

    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputHandler = GetComponent<InputHandler>();
    }

    private void Update()
    {
    if (inputHandler.JumpPressed && isGrounded)
    {
        Jump();
        inputHandler.ResetJump();
    }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        rb.linearVelocity = new Vector2(
            inputHandler.MoveInput.x * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}