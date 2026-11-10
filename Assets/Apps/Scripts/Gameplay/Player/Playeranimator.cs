
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(InputHandler))]
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private float runThreshold = 0.1f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Animator animator;
    private PlayerController playerController;
    private InputHandler inputHandler;

    private static readonly int IsRunningHash =
        Animator.StringToHash("IsRunning");

    private static readonly int IsJumpingHash =
        Animator.StringToHash("IsJumping");

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerController = GetComponent<PlayerController>();
        inputHandler = GetComponent<InputHandler>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        float moveX = inputHandler.MoveInput.x;

        bool isMoving = Mathf.Abs(moveX) > runThreshold;
        bool isGrounded = playerController.IsGrounded;

        animator.SetBool(IsRunningHash, isMoving && isGrounded);
        animator.SetBool(IsJumpingHash, !isGrounded);

        if (moveX < -runThreshold)
            spriteRenderer.flipX = true;
        else if (moveX > runThreshold)
            spriteRenderer.flipX = false;
    }
}
