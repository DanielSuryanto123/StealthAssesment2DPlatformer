using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(InputHandler))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject pingPrefab;
    [SerializeField] private Transform pingSpawnPoint;
    [SerializeField] private float pingCooldown = 3f;
    private bool canPing = true;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 10f;

    private Rigidbody2D rb;
    private InputHandler inputHandler;
    private bool isGrounded;

    // Public read-only access so other components (e.g. PlayerAnimator) can react
    // to grounded state without duplicating the collision logic.
    public bool IsGrounded => isGrounded;

    // Fired exactly once at the moment a jump is actually executed.
    // Subscribe to this instead of polling JumpPressed + isGrounded elsewhere.
    public event Action Jumped;

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
        Jumped?.Invoke();
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
        GameObject ping = Instantiate(
            pingPrefab,
            pingSpawnPoint.position,
            Quaternion.identity
        );
        Destroy(ping, 1.5f);
        yield return new WaitForSeconds(pingCooldown);
        canPing = true;
    }
}