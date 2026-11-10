
using UnityEngine;

public class PlayerFacing : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private InputHandler inputHandler;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (inputHandler == null)
            inputHandler = GetComponentInParent<InputHandler>();
    }

    private void Update()
    {
        float direction = inputHandler.MoveInput.x;

        if (direction < -0.01f)
            spriteRenderer.flipX = true;
        else if (direction > 0.01f)
            spriteRenderer.flipX = false;
    }
}
