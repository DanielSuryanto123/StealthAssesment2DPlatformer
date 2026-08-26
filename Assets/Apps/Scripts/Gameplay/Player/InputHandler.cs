using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool PingPressed { get; private set; }

    public void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            JumpPressed = true;
        }
    }

    public void ResetJump()
    {
        JumpPressed = false;
    }

    // Changed to latch like Jump/Ping, rather than clearing on key release.
    // Otherwise the punch trigger below would risk firing every frame the
    // key is held instead of once per press.
    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            InteractPressed = true;
        }
    }

    public void ResetInteract()
    {
        InteractPressed = false;
    }

    public void OnPing(InputValue value)
    {
        if (value.isPressed)
        {
            PingPressed = true;
        }
    }

    public void ResetPing()
    {
        PingPressed = false;
    }
}