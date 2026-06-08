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
    public void OnInteract(InputValue value)
    {
        InteractPressed = value.isPressed;
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