
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(InputHandler))]
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private float runThreshold = 0.1f;
    [SerializeField] private Transform graphics;

    private Animator animator;
    private PlayerController playerController;
    private InputHandler inputHandler;

    private static readonly int IsRunningHash =
        Animator.StringToHash("IsRunning");

    private static readonly int IsJumpingHash =
        Animator.StringToHash("IsJumping");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        inputHandler = GetComponent<InputHandler>();

        if (graphics == null)
            graphics = transform;
    }

    private void Update()
    {
        float moveX = inputHandler.MoveInput.x;

        bool isMoving = Mathf.Abs(moveX) > runThreshold;
        bool isGrounded = playerController.IsGrounded;

        animator.SetBool(IsRunningHash, isMoving && isGrounded);
        animator.SetBool(IsJumpingHash, !isGrounded);

        if (moveX < -runThreshold)
        {
            Vector3 scale = graphics.localScale;
            scale.x = -Mathf.Abs(scale.x);
            graphics.localScale = scale;
        }
        else if (moveX > runThreshold)
        {
            Vector3 scale = graphics.localScale;
            scale.x = Mathf.Abs(scale.x);
            graphics.localScale = scale;
        }
    }
}
