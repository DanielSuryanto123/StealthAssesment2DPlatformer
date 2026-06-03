using UnityEngine;

public class JumpAction
{
    private Rigidbody2D rb;
    private float jumpForce;

    public JumpAction(Rigidbody2D rb, float jumpForce)
    {
        this.rb = rb;
        this.jumpForce = jumpForce;
    }

    public void Execute()
    {
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );
    }
}