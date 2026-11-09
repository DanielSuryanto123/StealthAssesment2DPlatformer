
using UnityEngine;
using System.Collections;
using CoLab.Telemetry;
using EventType = CoLab.Telemetry.EventType;

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

    public float PingCooldownProgress01 =>
        pingCooldown <= 0f
            ? 1f
            : 1f - Mathf.Clamp01(pingCooldownTimer / pingCooldown);

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 10f;

    private Rigidbody2D rb;
    private InputHandler inputHandler;
    private bool isGrounded;

    public bool IsGrounded => isGrounded;

    // True from the moment a jump leaves the ground until its outcome (landing or hazard) is logged.
    private bool jumpInProgress;
    // True after a hazard was entered until the player touches solid ground again or respawns.
    private bool inHazard;

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

        if (inputHandler.InteractPressed)
        {
            Interact();
        }
    }

    private void LateUpdate()
    {
        // Cleared after every Update (InputHandler runs first) so all readers see InteractPressed for exactly one frame.
        if (inputHandler.InteractPressed)
        {
            inputHandler.ResetInteract();
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
        Debug.Log(gameObject.name + " is jumping");

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        jumpInProgress = true;
    }

    /// <summary>Called by hazards when this player touches them. A jump in progress counts as failed.</summary>
    public void NotifyHazardEntered()
    {
        inHazard = true;

        if (jumpInProgress)
        {
            jumpInProgress = false;
            LogEvent(EventType.JUMP_FAIL);
        }
    }

    /// <summary>Clears jump/hazard state (used on respawn); no event is logged.</summary>
    public void ResetJumpState()
    {
        jumpInProgress = false;
        inHazard = false;
    }

    private void LogEvent(EventType eventType)
    {
        AnalyticsManager.Instance?.LogEvent(transform, eventType);
    }

    private void Interact()
    {
        // Interaction logic will be added later.
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contactCount > 0 && collision.GetContact(0).normal.y > 0.5f)
        {
            inHazard = false;

            if (jumpInProgress)
            {
                jumpInProgress = false;
                LogEvent(EventType.JUMP_SUCCESS);
            }
        }

        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    // Rule: a ping from a trapped player or one standing in a hazard asks for help (PING_HELP);
    // any other ping points out a position / directs the partner (PING_HERE).
    private EventType GetPingEventType()
    {
        bool trapped = AnalyticsManager.TryGetPlayerID(transform, out PlayerID id)
                       && AnalyticsManager.Instance != null
                       && AnalyticsManager.Instance.IsPlayerTrapped(id);

        return trapped || inHazard ? EventType.PING_HELP : EventType.PING_HERE;
    }

    private IEnumerator DoPing()
    {
        canPing = false;
        pingCooldownTimer = pingCooldown;

        if (pingPrefab != null)
        {
            LogEvent(GetPingEventType());

            Vector3 spawnPos = pingSpawnPoint != null
                ? pingSpawnPoint.position
                : transform.position + Vector3.up;

            GameObject ping = Instantiate(
                pingPrefab,
                spawnPos,
                Quaternion.identity
            );

            Destroy(ping, pingLifetime);
        }

        yield return new WaitForSeconds(pingCooldown);

        canPing = true;
    }
}
