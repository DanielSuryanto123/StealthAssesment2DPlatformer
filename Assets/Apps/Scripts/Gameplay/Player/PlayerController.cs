using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(InputHandler))]
public class PlayerController : MonoBehaviour
{
    [Header("Ping")]
    [SerializeField] private GameObject pingPrefab;
    [SerializeField] private Transform pingSpawnPoint;
    [SerializeField] private float pingCooldown = 3f;
    [SerializeField] private float pingLifetime = 1.5f;
    private bool canPing = true;
    private float pingCooldownTimer = 0f;

    // Exposed for UI (e.g. a cooldown ring around a ping button icon).
    public float PingCooldownProgress01 =>
        pingCooldown <= 0f ? 1f : 1f - Mathf.Clamp01(pingCooldownTimer / pingCooldown);

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 10f;

    private Rigidbody2D rb;
    private InputHandler inputHandler;
    private bool isGrounded;

    public bool IsGrounded => isGrounded;
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

        if (pingCooldownTimer > 0f)
        {
            pingCooldownTimer -= Time.deltaTime;
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
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        Jumped?.Invoke();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) isGrounded = false;
    }

    private IEnumerator DoPing()
    {
        canPing = false;
        pingCooldownTimer = pingCooldown;

        if (pingPrefab != null)
        {
            Vector3 spawnPos = pingSpawnPoint != null ? pingSpawnPoint.position : transform.position;
            GameObject ping = Instantiate(pingPrefab, spawnPos, Quaternion.identity);

            PingIndicator indicator = ping.GetComponent<PingIndicator>();
            if (indicator != null)
            {
                indicator.FollowTarget(pingSpawnPoint != null ? pingSpawnPoint : transform);
            }
            else
            {
                Destroy(ping, pingLifetime);
            }
        }

        yield return new WaitForSeconds(pingCooldown);
        canPing = true;
    }
}