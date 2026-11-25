using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[DefaultExecutionOrder(-100)]
public class InputHandler : MonoBehaviour
{
    [Header("Player Input")]
    [SerializeField] private bool useGamepad = false;

    [Header("Keyboard Movement")]
    [SerializeField] private KeyCode moveLeftKey = KeyCode.A;
    [SerializeField] private KeyCode moveRightKey = KeyCode.D;

    [Header("Keyboard Actions")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode pingKey = KeyCode.Q;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool PingPressed { get; private set; }
    public bool InteractPressed { get; private set; }

    private void Update()
    {
        if (useGamepad)
            ReadGamepadInput();
        else
            ReadKeyboardInput();
    }

    private void ReadKeyboardInput()
    {
        float x = 0f;

        if (Input.GetKey(moveLeftKey)) x -= 1f;
        if (Input.GetKey(moveRightKey)) x += 1f;

        MoveInput = new Vector2(x, 0f);

        if (Input.GetKeyDown(jumpKey))
            JumpPressed = true;

        if (Input.GetKeyDown(pingKey))
            PingPressed = true;

        if (Input.GetKeyDown(interactKey))
        {
            InteractPressed = true;
            Debug.Log($"[{gameObject.name}] KEYBOARD INTERACT DETECTED");
        }
    }

    private void ReadGamepadInput()
    {
        Gamepad gamepad = Gamepad.current;

        if (gamepad == null)
        {
            MoveInput = Vector2.zero;
            return;
        }

        MoveInput = gamepad.leftStick.ReadValue();

        if (gamepad.buttonSouth.wasPressedThisFrame)
            JumpPressed = true;

        if (gamepad.buttonNorth.wasPressedThisFrame)
            PingPressed = true;

        if (gamepad.buttonWest.wasPressedThisFrame)
        {
            InteractPressed = true;
            Debug.Log($"[{gameObject.name}] GAMEPAD INTERACT DETECTED");
        }
    }

    public void ResetJump() => JumpPressed = false;
    public void ResetPing() => PingPressed = false;
    public void ResetInteract() => InteractPressed = false;
}
