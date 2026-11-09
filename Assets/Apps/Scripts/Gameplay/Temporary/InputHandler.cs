using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private KeyCode moveLeftKey = KeyCode.A;
    [SerializeField] private KeyCode moveRightKey = KeyCode.D;

    [Header("Actions")]
    [SerializeField] private KeyCode jumpKey = KeyCode.W;
    [SerializeField] private KeyCode pingKey = KeyCode.Q;
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool PingPressed { get; private set; }
    public bool InteractPressed { get; private set; }

    private void Update()
    {
        float x = 0f;

        if (Input.GetKey(moveLeftKey))
            x -= 1f;

        if (Input.GetKey(moveRightKey))
            x += 1f;

        MoveInput = new Vector2(x, 0f);

        if (Input.GetKeyDown(jumpKey))
    {
        JumpPressed = true;
        Debug.Log(gameObject.name + " received jump input: " + jumpKey);
    }
        if (Input.GetKeyDown(pingKey))
            PingPressed = true;

        if (Input.GetKeyDown(interactKey))
            InteractPressed = true;
    }

    public void ResetJump()
    {
        JumpPressed = false;
    }

    public void ResetPing()
    {
        PingPressed = false;
    }

    public void ResetInteract()
    {
        InteractPressed = false;
    }
}