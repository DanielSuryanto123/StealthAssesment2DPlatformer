using UnityEngine;

public class MoveAction
{
    private Rigidbody2D rb;
    private float moveSpeed;

    public MoveAction(Rigidbody2D rb, float moveSpeed)
    {
        this.rb = rb;
        this.moveSpeed = moveSpeed;
    }

    public void Execute(Vector2 moveInput)
    {
        rb.linearVelocity = new Vector2(
            moveInput.x * moveSpeed,
            rb.linearVelocity.y
        );
    }
}