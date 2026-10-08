using UnityEngine;

public class LegacyPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Controls")]
    public bool isPlayerOne = true;

    private Rigidbody2D rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Move();
        Jump();
    }

    void Move()
    {
        float moveInput = 0f;

        // PLAYER 1 = WASD
        if (isPlayerOne)
        {
            if (Input.GetKey(KeyCode.A))
                moveInput = -1f;

            if (Input.GetKey(KeyCode.D))
                moveInput = 1f;
        }
        // PLAYER 2 = ARROW KEYS
        else
        {
            if (Input.GetKey(KeyCode.LeftArrow))
                moveInput = -1f;

            if (Input.GetKey(KeyCode.RightArrow))
                moveInput = 1f;
        }

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    void Jump()
    {
        // PLAYER 1 JUMP = W
        if (isPlayerOne)
        {
            if (Input.GetKeyDown(KeyCode.W) && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }
        // PLAYER 2 JUMP = UP ARROW
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }
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