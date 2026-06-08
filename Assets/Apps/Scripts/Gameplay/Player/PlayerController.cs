using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(InputHandler))]
public class PlayerController : MonoBehaviour
{

    [Header("Ping")]
    [SerializeField] private GameObject pingIcon;

    [SerializeField] private float pingDuration = 1.5f;

    [SerializeField] private float pingCooldown = 3f;

    private bool canPing = true;

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
        if (inputHandler.PingPressed && canPing)
        {
            StartCoroutine(DoPing());

            inputHandler.ResetPing();
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
    private IEnumerator DoPing()
    {
        canPing = false;

        pingIcon.SetActive(true);

        yield return new WaitForSeconds(pingDuration);

        pingIcon.SetActive(false);

        yield return new WaitForSeconds(pingCooldown);

        canPing = true;
    }
}