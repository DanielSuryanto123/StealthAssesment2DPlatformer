using UnityEngine;

[DefaultExecutionOrder(-100)]
public class InputHandler : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private KeyCode moveLeftKey = KeyCode.A;
    [SerializeField] private KeyCode moveRightKey = KeyCode.D;

    [Header("Actions")]
    [SerializeField] private KeyCode jumpKey = KeyCode.W;
    [SerializeField] private KeyCode pingKey = KeyCode.Q;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool PingPressed { get; private set; }
    public bool InteractPressed { get; private set; }

    private void Update()
    {
        // MOVEMENT
        float x = 0f;
        if (Input.GetKey(moveLeftKey)) x -= 1f;
        if (Input.GetKey(moveRightKey)) x += 1f;
        MoveInput = new Vector2(x, 0f);

        // JUMP
        if (Input.GetKeyDown(jumpKey))
            JumpPressed = true;

        // PING
        if (Input.GetKeyDown(pingKey))
            PingPressed = true;

        // INTERACT (log menyebut nama objek supaya ketahuan P1 atau P2)
        if (Input.GetKeyDown(interactKey))
        {
            InteractPressed = true;
            Debug.Log($"[{gameObject.name}] INPUT DETECTED: {interactKey}");
        }
    }

    public void ResetJump() => JumpPressed = false;
    public void ResetPing() => PingPressed = false;
    public void ResetInteract() => InteractPressed = false;
}