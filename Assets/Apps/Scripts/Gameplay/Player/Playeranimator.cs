using UnityEngine;

/// <summary>
/// Bridges PlayerController/InputHandler state into the Animator.
/// Attach this to the same GameObject as PlayerController, InputHandler,
/// Rigidbody2D, SpriteRenderer, and Animator.
///
/// Required Animator Controller parameters:
///   - Speed      (Float)  - drives Idle <-> Run transition
///   - IsGrounded (Bool)   - drives Jump/Fall <-> Idle/Run transitions
///   - Jump       (Trigger) - fired the instant a jump happens
///   - Punch      (Trigger) - fired on Interact press (for the future
///                            "attack contraption" animation)
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("References (auto-filled if left empty)")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private InputHandler inputHandler;
    [SerializeField] private Rigidbody2D rb;

    [Header("Tuning")]
    [Tooltip("Horizontal speed below this counts as not moving, to avoid animation jitter from tiny float values.")]
    [SerializeField] private float moveThreshold = 0.05f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int IsGroundedParam = Animator.StringToHash("IsGrounded");
    private static readonly int JumpTrigger = Animator.StringToHash("Jump");
    private static readonly int PunchTrigger = Animator.StringToHash("Punch");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (playerController == null) playerController = GetComponent<PlayerController>();
        if (inputHandler == null) inputHandler = GetComponent<InputHandler>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (playerController != null)
            playerController.Jumped += HandleJumped;
    }

    private void OnDisable()
    {
        if (playerController != null)
            playerController.Jumped -= HandleJumped;
    }

    private void Update()
    {
        UpdateMovementParams();
        UpdateFacingDirection();
        UpdatePunchTrigger();
    }

    private void UpdateMovementParams()
    {
        float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat(SpeedParam, horizontalSpeed);
        animator.SetBool(IsGroundedParam, playerController.IsGrounded);
    }

    private void UpdateFacingDirection()
    {
        if (Mathf.Abs(inputHandler.MoveInput.x) > moveThreshold)
        {
            // Assumes the sprite's default artwork faces right.
            // Flip if your source art faces left instead.
            spriteRenderer.flipX = inputHandler.MoveInput.x < 0f;
        }
    }

    private void UpdatePunchTrigger()
    {
        if (inputHandler.InteractPressed)
        {
            animator.SetTrigger(PunchTrigger);
            inputHandler.ResetInteract();
        }
    }

    private void HandleJumped()
    {
        animator.SetTrigger(JumpTrigger);
    }
}